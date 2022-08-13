//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : PackageReferenceResolver.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/13/2022
// Note    : Copyright 2017-2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to resolve PackageReference elements in MSBuild project files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/20/2017  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Microsoft.Build.Evaluation;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This is used to resolved package references (<c>PackageReference</c> elements) in MSBuild project files
    /// </summary>
    /// <remarks>Package references are handled by the NuGet MSBuild targets.  Trying to figure out how they
    /// work would be quite difficult as would trying to plug them into the reflection data generation project.
    /// However, those tasks create an asset file that contains all of the information we need so we use it to
    /// figure out the reference assembly locations along with any dependencies.</remarks>
    public class PackageReferenceResolver
    {
        #region Private data members
        //=====================================================================

        private readonly BuildProcess buildProcess;
        private readonly HashSet<string> resolvedDependencies, packageReferences, implicitPackageFolders;
        private JsonProperty? packages;
        private string projectFilename;
        private string[] nugetPackageFolders;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns a list of all resolved package reference assemblies along with all dependency assembly
        /// references.
        /// </summary>
        public IEnumerable<string> ReferenceAssemblies
        {
            get
            {
                if(packages != null && packageReferences.Count != 0 && nugetPackageFolders != null)
                {
                    foreach(string assembly in this.ResolvePackageReferencesInternal(packageReferences))
                        foreach(string folder in nugetPackageFolders)
                        {
                            string path = Path.Combine(folder, assembly.Replace("/", @"\"));

                            if(File.Exists(path))
                                yield return path;
                        }
                }

                foreach(string folder in implicitPackageFolders)
                    foreach(string reference in Directory.EnumerateFiles(folder, "*.dll"))
                        yield return reference;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildProcess">The build process that will make use of the resolver</param>
        public PackageReferenceResolver(BuildProcess buildProcess)
        {
            this.buildProcess = buildProcess;

            resolvedDependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            packageReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            implicitPackageFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to load the package reference information from the given project
        /// </summary>
        /// <param name="project">The MSBuild project from which to get the package references</param>
        /// <param name="targetFramework">A target framework value used to resolve implicit package references</param>
        /// <returns>True if package reference info was loaded, false if not</returns>
        public bool LoadPackageReferenceInfo(Project project, string targetFramework)
        {
            if(project == null)
                throw new ArgumentNullException(nameof(project));

            if(targetFramework == null)
                throw new ArgumentNullException(nameof(targetFramework));

            packages = null;
            resolvedDependencies.Clear();
            packageReferences.Clear();

            // This is mostly guesswork so it may still need some revisions
            string targetingPackRoot = project.GetPropertyValue("NetCoreTargetingPackRoot");

            // Strip off the OS part if there
            int dash = targetFramework.IndexOf('-');

            if(dash > 0)
                targetFramework = targetFramework.Substring(0, dash);

            try
            {
                projectFilename = project.FullPath;

                if(project.GetPropertyValue("NuGetProjectStyle") == "PackageReference")
                {
                    string assetFile = project.GetPropertyValue("ProjectAssetsFile");

                    if(!String.IsNullOrWhiteSpace(assetFile) && File.Exists(assetFile))
                    {
                        JsonElement root;

                        using(JsonDocument packageInfo = JsonDocument.Parse(File.ReadAllText(assetFile),
                          new JsonDocumentOptions { AllowTrailingCommas = true,
                            CommentHandling = JsonCommentHandling.Skip }))
                        {
                            root = packageInfo.RootElement.Clone();
                        }

                        if(root.TryGetProperty("targets", out JsonElement targets))
                        {
                            packages = targets.EnumerateObject().First();

                            string folders = project.GetPropertyValue("NuGetPackageFolders");

                            if(!String.IsNullOrWhiteSpace(folders))
                                nugetPackageFolders = folders.Split(';');
                            else
                                nugetPackageFolders = Array.Empty<string>();

                            foreach(var reference in project.GetItems("PackageReference"))
                            {
                                var version = reference.Metadata.FirstOrDefault(m => m.Name == "Version");

                                if(version != null)
                                    packageReferences.Add(reference.EvaluatedInclude + "/" + version.EvaluatedValue);
                            }
                        }
                        else
                        {
                            buildProcess.ReportWarning("BE0011", "Unable to load package reference information " +
                                "for '{0}'.  Reason: Unable to locate targets element in project assets file '{1}'.",
                                projectFilename, assetFile);
                        }

                        // See if there's a frameworks element.  If so, get the package names from it to use
                        // with the implicit package references below.
                        if(root.TryGetProperty("project", out JsonElement projectNode))
                        {
                            if(projectNode.TryGetProperty("frameworks", out JsonElement frameworks) &&
                              frameworks.EnumerateObject().Any())
                            {
                                var f = frameworks.EnumerateObject().First();

                                if(f.Value.TryGetProperty("frameworkReferences", out JsonElement references))
                                {
                                    foreach(var prop in references.EnumerateObject())
                                    {
                                        string rootPath = Path.Combine(targetingPackRoot, prop.Name);

                                        // Most exist with a ".Ref" suffix
                                        if(!Directory.Exists(rootPath))
                                        {
                                            rootPath += ".Ref";

                                            // We may need to strip off the last part of the identifier (i.e. ".Windows")
                                            if(!Directory.Exists(rootPath))
                                            {
                                                rootPath = rootPath.Substring(0, rootPath.Length - 4);

                                                int dot = rootPath.LastIndexOf('.');

                                                if(dot != -1)
                                                {
                                                    rootPath = rootPath.Substring(0, dot);

                                                    if(!Directory.Exists(rootPath))
                                                        rootPath += ".Ref";
                                                }
                                            }
                                        }

                                        if(Directory.Exists(rootPath))
                                        {
                                            foreach(string path in Directory.EnumerateDirectories(rootPath, "*", SearchOption.AllDirectories))
                                                if(path.EndsWith(targetFramework, StringComparison.OrdinalIgnoreCase))
                                                    implicitPackageFolders.Add(path);
                                        }
                                    }

                                }
                            }
                        }
                    }
                    else
                        buildProcess.ReportWarning("BE0011", "Unable to load package reference information " +
                            "for '{0}'.  Reason: Project assets file '{1}' does not exist.", projectFilename,
                            assetFile);
                }
            }
            catch(Exception ex)
            {
                // We won't prevent the build from continuing if there's an error.  We'll just report it.
                System.Diagnostics.Debug.WriteLine(ex);

                buildProcess.ReportWarning("BE0011", "Unable to load package reference information for '{0}'.  " +
                    "Reason: {1}", projectFilename, ex.Message);
            }

            // If we have a target framework, look for default implicit packages.  This will helps us find
            // references for .NETStandard 2.1 and later which don't have an explicit package reference.
            if(!String.IsNullOrWhiteSpace(targetFramework))
            {
                string defaultImplicitPackages = project.GetPropertyValue("DefaultImplicitPackages");

                if(!String.IsNullOrWhiteSpace(defaultImplicitPackages) && !String.IsNullOrWhiteSpace(targetingPackRoot))
                {
                    foreach(string package in defaultImplicitPackages.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string rootPath = Path.Combine(targetingPackRoot, package);

                        // Most exist with a ".Ref" suffix
                        if(!Directory.Exists(rootPath))
                        {
                            rootPath += ".Ref";

                            // We may need to strip off the last part of the identifier (i.e. ".Windows")
                            if(!Directory.Exists(rootPath))
                            {
                                rootPath = rootPath.Substring(0, rootPath.Length - 4);

                                int dot = rootPath.LastIndexOf('.');

                                if(dot != -1)
                                {
                                    rootPath = rootPath.Substring(0, dot);

                                    if(!Directory.Exists(rootPath))
                                        rootPath += ".Ref";
                                }
                            }
                        }

                        if(Directory.Exists(rootPath))
                        {
                            foreach(string path in Directory.EnumerateDirectories(rootPath, "*", SearchOption.AllDirectories))
                                if(path.EndsWith(targetFramework, StringComparison.OrdinalIgnoreCase))
                                    implicitPackageFolders.Add(path);
                        }
                    }
                }
            }
            
            return (packages != null && packageReferences.Count != 0) || implicitPackageFolders.Count != 0;
        }

        /// <summary>
        /// This is used to resolve package references by looking up the package IDs in the asset file created
        /// by the NuGet Restore task.
        /// </summary>
        /// <param name="referencesToResolve">The package references to resolve</param>
        /// <returns>An enumerable list of assembly names.</returns>
        /// <remarks>If a package has dependencies, those will be resolved and returned as well</remarks>
        private IEnumerable<string> ResolvePackageReferencesInternal(IEnumerable<string> referencesToResolve)
        {
            HashSet<string> references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach(var p in referencesToResolve)
                try
                {
                    JsonElement? match = null;
                    string packageName = p;

                    resolvedDependencies.Add(packageName);

                    // If we don't get a match, try it with ".0" on the end.  Sometimes the reference version
                    // leaves it off.
                    if(!packages.Value.Value.TryGetProperty(packageName, out JsonElement m))
                    {
                        packageName += ".0";

                        if(packages.Value.Value.TryGetProperty(packageName, out m))
                        {
                            resolvedDependencies.Add(packageName);
                            match = m;
                        }

                        // Other times, it leaves off all trailing zero parts
                        while(packageName.EndsWith(".0", StringComparison.Ordinal))
                        {
                            packageName = packageName.Substring(0, packageName.Length - 2);

                            if(packages.Value.Value.TryGetProperty(packageName, out m))
                            {
                                resolvedDependencies.Add(packageName);
                                match = m;
                                break;
                            }
                        }
                    }
                    else
                        match = m;

                    if(match != null)
                    {
                        JsonElement? assemblyInfo = null;

                        if(match.Value.TryGetProperty("compile", out JsonElement c))
                            assemblyInfo = c;
                        else
                        {
                            if(match.Value.TryGetProperty("runtime", out JsonElement r))
                                assemblyInfo = r;
                        }

                        if(assemblyInfo != null)
                        {
                            foreach(var assemblyName in assemblyInfo.Value.EnumerateObject())
                            {
                                // Ignore mscorlib.dll as it's types will have been redirected elsewhere so we
                                // don't need it.  "_._" occurs in the framework SDK packages and we can ignore
                                // it too.
                                if(!assemblyName.Name.EndsWith("/mscorlib.dll", StringComparison.OrdinalIgnoreCase) &&
                                  !assemblyName.Name.EndsWith("_._", StringComparison.Ordinal))
                                {
                                    references.Add(Path.Combine(packageName, assemblyName.Name));
                                }
                            }
                        }

                        if(match.Value.TryGetProperty("dependencies", out JsonElement dependencies))
                        {
                            var deps = dependencies.EnumerateObject().Select(t => t.Name + "/" + t.Value).Where(
                                d => !resolvedDependencies.Contains(d)).ToList();

                            if(deps.Count != 0)
                            {
                                // Track the ones we've seen to prevent getting stuck due to circular references
                                resolvedDependencies.UnionWith(deps);
                                references.UnionWith(this.ResolvePackageReferencesInternal(deps));
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    // We won't prevent the build from continuing if there's an error.  We'll just report it.
                    System.Diagnostics.Debug.WriteLine(ex);

                    buildProcess.ReportWarning("BE0011", "Unable to load package reference information for " +
                        "'{0}' in '{1}'.  Reason: {2}", p, projectFilename, ex.Message);
                }

            return references;
        }
        #endregion
    }
}
