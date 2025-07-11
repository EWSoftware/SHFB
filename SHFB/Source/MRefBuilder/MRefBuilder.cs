//===============================================================================================================
// System  : Sandcastle MRefBuilder Tool
// File    : MRefBuilder.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/09/2025
//
// This file contains the class used to make MRefBuilder callable from MSBuild projects.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Date        Who  Comments
// ==============================================================================================================
// 12/10/2013  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: dep

using System;
using System.Collections.Generic;
using System.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Sandcastle.Tools.Reflection;

using Sandcastle.Core;
using Sandcastle.Core.Reflection;

namespace Sandcastle.Tools.MSBuild
{
    public class MRefBuilder : Task, ICancelableTask
    {
        #region Private data members
        //=====================================================================

        private ManagedReflectionWriter apiVisitor;
#if DEBUG
        private bool waitCancelled;
#endif
        #endregion

        #region Task properties
        //=====================================================================

        /// <summary>
        /// This is used to pass in the working folder where the files are located
        /// </summary>
        /// <value>If not set, no working folder will be set for the build</value>
        public string WorkingFolder { get; set; }

        /// <summary>
        /// This is used to pass in the MRefBuilder configuration file name
        /// </summary>
        /// <value>If not set, the default configuration file is used</value>
        public string ConfigurationFile { get; set; }

        /// <summary>
        /// This is used to pass in the filename to which the reflection data is written
        /// </summary>
        [Required]
        public string ReflectionFilename { get; set; }

        /// <summary>
        /// This is used to pass in the assemblies to reflect over
        /// </summary>
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        /// <summary>
        /// This is used to pass in any resolved references
        /// </summary>
        /// <value>References are optional</value>
        public ITaskItem[] References { get; set; }

#if DEBUG
        /// <summary>
        /// This is used to indicate whether or not to wait for the debugger to attach to the process
        /// </summary>
        public bool WaitForDebugger { get; set; }
#endif
        #endregion

        #region ICancelableTask Members
        //=====================================================================

        /// <summary>
        /// Cancel the build
        /// </summary>
        /// <remarks>The build will be canceled as soon as the current type has finished being visited</remarks>
        public void Cancel()
        {
            if(apiVisitor != null)
                apiVisitor.Canceled = true;
#if DEBUG
            waitCancelled = true;
#endif
        }
        #endregion

        #region Task execution
        //=====================================================================

        /// <summary>
        /// This executes the task
        /// </summary>
        /// <returns>True on success, false on failure</returns>
        public override bool Execute()
        {
            string currentDirectory = null;
            bool success = false;
#if DEBUG
            if(this.WaitForDebugger)
            {
                while(!Debugger.IsAttached && !waitCancelled)
                {
#if NET9_0_OR_GREATER
                    this.Log.LogMessage("DEBUG MODE: Waiting for debugger to attach (process ID: {0})",
                        Environment.ProcessId);
#else
                    this.Log.LogMessage("DEBUG MODE: Waiting for debugger to attach (process ID: {0})",
                        Process.GetCurrentProcess().Id);
#endif
                    System.Threading.Thread.Sleep(1000);
                }

                if(waitCancelled)
                    return false;

                Debugger.Break();
            }
#endif
                    Assembly application = Assembly.GetCallingAssembly();
            System.Reflection.AssemblyName applicationData = application.GetName();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(application.Location);

            this.Log.LogMessage("{0} (v{1})", applicationData.Name, fvi.ProductVersion);

            var copyrightAttributes = application.GetCustomAttributes<AssemblyCopyrightAttribute>();

            foreach(AssemblyCopyrightAttribute copyrightAttribute in copyrightAttributes)
                this.Log.LogMessage(copyrightAttribute.Copyright);

            if(this.Assemblies == null || this.Assemblies.Length == 0)
            {
                this.WriteMessage(MessageLevel.Error, "At least one assembly (.dll or .exe) is " +
                    "required for MRefBuilder to parse");
                return false;
            }

            try
            {
                // Switch to the working folder for the build so that relative paths are resolved properly
                if(!String.IsNullOrWhiteSpace(this.WorkingFolder))
                {
                    currentDirectory = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(Path.GetFullPath(this.WorkingFolder));
                }

                success = this.GenerateReflectionInformation();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                this.WriteMessage(MessageLevel.Error, "An unexpected error occurred trying to " +
                    "execute the MRefBuilder MSBuild task: {0}", ex);
            }
            finally
            {
                if(currentDirectory != null)
                    Directory.SetCurrentDirectory(currentDirectory);
            }

            return success;
        }
        #endregion

        #region Main program entry point
        //=====================================================================

        /// <summary>
        /// Main program entry point (MSBuild task)
        /// </summary>
        /// <returns>True on success or false failure</returns>
        public bool GenerateReflectionInformation()
        {
            string path, version, framework = null, assemblyPath, typeName;

            // Load the configuration file
            XPathDocument config;
            XPathNavigator configNav;

            string configDirectory = Path.GetDirectoryName(this.ConfigurationFile);

            try
            {
                config = new XPathDocument(this.ConfigurationFile);
                configNav = config.CreateNavigator();
            }
            catch(IOException e)
            {
                this.WriteMessage(MessageLevel.Error, "An error occurred while attempting to read " +
                    "the configuration file '{0}'. The error message is: {1}", this.ConfigurationFile, e.Message);
                return false;
            }
            catch(UnauthorizedAccessException e)
            {
                this.WriteMessage(MessageLevel.Error, "An error occurred while attempting to read " +
                    "the configuration file '{0}'. The error message is: {1}", this.ConfigurationFile, e.Message);
                return false;
            }
            catch(XmlException e)
            {
                this.WriteMessage(MessageLevel.Error, "The configuration file '{0}' is not " +
                    "well-formed. The error message is: {1}", this.ConfigurationFile, e.Message);
                return false;
            }

            // Adjust the target platform
            var platformNode = configNav.SelectSingleNode("/configuration/dduetools/platform");

            if(platformNode == null)
            {
                this.WriteMessage(MessageLevel.Error, "A platform element is required to define the " +
                    "framework type and version to use.");
                return false;
            }

            // !EFW - Use the framework definition file to load core framework assembly reference information
            version = platformNode.GetAttribute("version", String.Empty);
            framework = platformNode.GetAttribute("framework", String.Empty);

            // Get component locations used to find additional reflection data definition files
            List<string> componentFolders = [];
            var locations = configNav.SelectSingleNode("/configuration/dduetools/componentLocations");

            if(locations != null)
            {
                foreach(XPathNavigator folder in locations.Select("location/@folder"))
                {
                    if(!String.IsNullOrWhiteSpace(folder.Value) && Directory.Exists(folder.Value))
                        componentFolders.Add(folder.Value);
                }
            }

            // Get the dependencies
            var dependencies = new List<string>();

            if(this.References != null)
                dependencies.AddRange(this.References.Select(r => r.ItemSpec));

            if(!String.IsNullOrEmpty(framework) && !String.IsNullOrEmpty(version))
            {
                var coreNames = new HashSet<string>(["netstandard", "mscorlib", "System.Runtime"],
                    StringComparer.OrdinalIgnoreCase);

                var coreFrameworkAssemblies = this.Assemblies.Select(a => a.ItemSpec).Concat(dependencies).Where(
                    d => coreNames.Contains(Path.GetFileNameWithoutExtension(d)));

                TargetPlatform.SetFrameworkInformation(framework, version, componentFolders,
                    coreFrameworkAssemblies);
            }
            else
            {
                this.WriteMessage(MessageLevel.Error, "Unknown target framework version '{0} {1}'.",
                    framework, version);
                return false;
            }

            // Create an API member namer
            ApiNamer namer;

            // Apply a different naming method to assemblies using these frameworks
            if(framework == PlatformType.DotNetCore || framework == PlatformType.DotNetPortable ||
              framework == PlatformType.WindowsPhone || framework == PlatformType.WindowsPhoneApp)
            {
                namer = new WindowsStoreAndPhoneNamer();
            }
            else
                namer = new OrcasNamer();

            XPathNavigator namerNode = configNav.SelectSingleNode("/configuration/dduetools/namer");

            if(namerNode != null)
            {
                assemblyPath = namerNode.GetAttribute("assembly", String.Empty);
                typeName = namerNode.GetAttribute("type", String.Empty);

                if(!String.IsNullOrWhiteSpace(assemblyPath))
                {
                    assemblyPath = Environment.ExpandEnvironmentVariables(assemblyPath);

                    if(!Path.IsPathRooted(assemblyPath))
                        assemblyPath = Path.Combine(configDirectory, assemblyPath);
                }

                try
                {
                    Assembly assembly = !String.IsNullOrWhiteSpace(assemblyPath) ? Assembly.LoadFrom(assemblyPath) :
                        Assembly.GetExecutingAssembly();
                    namer = (ApiNamer)assembly.CreateInstance(typeName);

                    if(namer == null)
                    {
                        this.WriteMessage(MessageLevel.Error, "The type '{0}' was not found in the " +
                            "component assembly '{1}'.", typeName, assembly.Location);
                        return false;
                    }
                }
                catch(IOException e)
                {
                    this.WriteMessage(MessageLevel.Error, "A file access error occurred while " +
                        "attempting to load the component assembly '{0}'. The error message is: {1}",
                        assemblyPath, e.Message);
                    return false;
                }
                catch(UnauthorizedAccessException e)
                {
                    this.WriteMessage(MessageLevel.Error, "A file access error occurred while " +
                        "attempting to load the component assembly '{0}'. The error message is: {1}",
                        assemblyPath, e.Message);
                    return false;
                }
                catch(BadImageFormatException)
                {
                    this.WriteMessage(MessageLevel.Error, "The component assembly '{0}' is not a " +
                        "valid managed assembly.", assemblyPath);
                    return false;
                }
                catch(TypeLoadException)
                {
                    this.WriteMessage(MessageLevel.Error, "The type '{0}' was not found in the " +
                        "component assembly '{1}'.", typeName, assemblyPath);
                    return false;
                }
                catch(MissingMethodException)
                {
                    this.WriteMessage(MessageLevel.Error, "No appropriate constructor exists for " +
                        "the type'{0}' in the component assembly '{1}'.", typeName, assemblyPath);
                    return false;
                }
                catch(TargetInvocationException e)
                {
                    this.WriteMessage(MessageLevel.Error, "An error occurred while initializing the " +
                        "type '{0}' in the component assembly '{1}'. The error message and stack trace " +
                        "follows: {2}", typeName, assemblyPath, e.InnerException);
                    return false;
                }
                catch(InvalidCastException)
                {
                    this.WriteMessage(MessageLevel.Error, "The type '{0}' in the component assembly " +
                        "'{1}' is not a component type.", typeName, assemblyPath);
                    return false;
                }
            }

            // Create a resolver
            AssemblyResolver resolver = new();
            XPathNavigator resolverNode = configNav.SelectSingleNode("/configuration/dduetools/resolver");

            if(resolverNode != null)
            {
                assemblyPath = resolverNode.GetAttribute("assembly", String.Empty);
                typeName = resolverNode.GetAttribute("type", String.Empty);

                if(!String.IsNullOrWhiteSpace(assemblyPath))
                {
                    assemblyPath = Environment.ExpandEnvironmentVariables(assemblyPath);

                    if(!Path.IsPathRooted(assemblyPath))
                        assemblyPath = Path.Combine(configDirectory, assemblyPath);
                }

                try
                {
                    Assembly assembly = !String.IsNullOrWhiteSpace(assemblyPath) ? Assembly.LoadFrom(assemblyPath) :
                        Assembly.GetExecutingAssembly();
                    resolver = (AssemblyResolver)assembly.CreateInstance(typeName, false, BindingFlags.Public |
                        BindingFlags.Instance, null, [resolverNode], null, null);

                    if(resolver == null)
                    {
                        this.WriteMessage(MessageLevel.Error, "The type '{0}' was not found in the " +
                            "component assembly '{1}'.", typeName, assembly.Location);
                        return false;
                    }
                }
                catch(IOException e)
                {
                    this.WriteMessage(MessageLevel.Error, "A file access error occurred while " +
                        "attempting to load the component assembly '{0}'. The error message is: {1}",
                        assemblyPath, e.Message);
                    return false;
                }
                catch(UnauthorizedAccessException e)
                {
                    this.WriteMessage(MessageLevel.Error, "A file access error occurred while " +
                        "attempting to load the component assembly '{0}'. The error message is: {1}",
                        assemblyPath, e.Message);
                    return false;
                }
                catch(BadImageFormatException)
                {
                    this.WriteMessage(MessageLevel.Error, "The component assembly '{0}' is not a " +
                        "valid managed assembly.", assemblyPath);
                    return false;
                }
                catch(TypeLoadException)
                {
                    this.WriteMessage(MessageLevel.Error, "The type '{0}' was not found in the " +
                        "component assembly '{1}'.", typeName, assemblyPath);
                    return false;
                }
                catch(MissingMethodException)
                {
                    this.WriteMessage(MessageLevel.Error, "No appropriate constructor exists for " +
                        "the type'{0}' in the component assembly '{1}'.", typeName, assemblyPath);
                    return false;
                }
                catch(TargetInvocationException e)
                {
                    this.WriteMessage(MessageLevel.Error, "An error occurred while initializing the " +
                        "type '{0}' in the component assembly '{1}'. The error message and stack trace " +
                        "follows: {2}", typeName, assemblyPath, e.InnerException);
                    return false;
                }
                catch(InvalidCastException)
                {
                    this.WriteMessage(MessageLevel.Error, "The type '{0}' in the component assembly " +
                        "'{1}' is not a component type.", typeName, assemblyPath);
                    return false;
                }
            }

            resolver.MessageLogger = (lvl, msg) => this.WriteMessage(lvl, msg);
            resolver.UnresolvedAssemblyReference += UnresolvedAssemblyReferenceHandler;

            // Get a text writer for output
            StreamWriter output;

            try
            {
                output = new StreamWriter(this.ReflectionFilename, false, Encoding.UTF8);
            }
            catch(IOException e)
            {
                this.WriteMessage(MessageLevel.Error, "An error occurred while attempting to " +
                    "create an output file. The error message is: {0}", e.Message);
                return false;
            }
            catch(UnauthorizedAccessException e)
            {
                this.WriteMessage(MessageLevel.Error, "An error occurred while attempting to " +
                    "create an output file. The error message is: {0}", e.Message);
                return false;
            }

            try
            {
                // Create a writer
                string sourceCodeBasePath = ((string)configNav.Evaluate(
                    "string(/configuration/dduetools/sourceContext/@basePath)")).CorrectFilePathSeparators();
                bool warnOnMissingContext = false;

                if(!String.IsNullOrWhiteSpace(sourceCodeBasePath))
                {
                    warnOnMissingContext = (bool)configNav.Evaluate(
                        "boolean(/configuration/dduetools/sourceContext[@warnOnMissingSourceContext='true'])");
                }
                else
                    this.WriteMessage(MessageLevel.Info, "No source code context base path " +
                        "specified.  Source context information is unavailable.");

                apiVisitor = new ManagedReflectionWriter(output, namer, resolver, sourceCodeBasePath,
                    warnOnMissingContext, new ApiFilter(configNav.SelectSingleNode("/configuration/dduetools")))
                {
                    MessageLogger = (lvl, msg) => this.WriteMessage(lvl, msg)
                };

                // Register add-ins to the builder
                XPathNodeIterator addinNodes = configNav.Select("/configuration/dduetools/addins/addin");

                foreach(XPathNavigator addinNode in addinNodes)
                {
                    assemblyPath = addinNode.GetAttribute("assembly", String.Empty);
                    typeName = addinNode.GetAttribute("type", String.Empty);

                    if(!String.IsNullOrWhiteSpace(assemblyPath))
                    {
                        assemblyPath = Environment.ExpandEnvironmentVariables(assemblyPath);

                        if(!Path.IsPathRooted(assemblyPath))
                            assemblyPath = Path.Combine(configDirectory, assemblyPath);
                    }

                    try
                    {
                        Assembly assembly = !String.IsNullOrWhiteSpace(assemblyPath) ? Assembly.LoadFrom(assemblyPath) :
                            Assembly.GetExecutingAssembly();
                        MRefBuilderAddIn addin = (MRefBuilderAddIn)assembly.CreateInstance(typeName, false,
                            BindingFlags.Public | BindingFlags.Instance, null,
                            [apiVisitor, addinNode], null, null);

                        if(addin == null)
                        {
                            this.WriteMessage(MessageLevel.Error, "The type '{0}' was not found in " +
                                "the add-in assembly '{1}'.", typeName, assembly.Location);
                            return false;
                        }
                    }
                    catch(IOException e)
                    {
                        this.WriteMessage(MessageLevel.Error, "A file access error occurred while " +
                            "attempting to load the add-in assembly '{0}'. The error message is: {1}",
                            assemblyPath, e.Message);
                        return false;
                    }
                    catch(BadImageFormatException)
                    {
                        this.WriteMessage(MessageLevel.Error, "The add-in assembly '{0}' is not a " +
                            "valid managed assembly.", assemblyPath);
                        return false;
                    }
                    catch(TypeLoadException)
                    {
                        this.WriteMessage(MessageLevel.Error, "The type '{0}' was not found in the " +
                            "add-in assembly '{1}'.", typeName, assemblyPath);
                        return false;
                    }
                    catch(MissingMethodException)
                    {
                        this.WriteMessage(MessageLevel.Error, "No appropriate constructor exists " +
                            "for the type '{0}' in the add-in assembly '{1}'.", typeName, assemblyPath);
                        return false;
                    }
                    catch(TargetInvocationException e)
                    {
                        this.WriteMessage(MessageLevel.Error, "An error occurred while initializing " +
                            "the type '{0}' in the add-in assembly '{1}'. The error message and stack trace " +
                            "follows: {2}", typeName, assemblyPath, e.InnerException);
                        return false;
                    }
                    catch(InvalidCastException)
                    {
                        this.WriteMessage(MessageLevel.Error, "The type '{0}' in the add-in " +
                            "assembly '{1}' is not an MRefBuilderAddIn type.", typeName, assemblyPath);
                        return false;
                    }
                }

                // Load dependencies
                foreach(string dependency in dependencies)
                {
                    try
                    {
                        // Expand environment variables
                        path = Environment.ExpandEnvironmentVariables(dependency);

                        // If x86 but it didn't exist, assume it's a 32-bit system and change the name
                        if(path.IndexOf("%ProgramFiles(x86)%", StringComparison.Ordinal) != -1)
                            path = Environment.ExpandEnvironmentVariables(path.Replace("(x86)", String.Empty));

                        apiVisitor.LoadAccessoryAssemblies(path);
                    }
                    catch(IOException e)
                    {
                        this.WriteMessage(MessageLevel.Error, "An error occurred while loading " +
                            "dependency assemblies. The error message is: {0}", e.Message);
                        return false;
                    }
                }

                // Parse the assemblies
                foreach(string dllPath in this.Assemblies.Select(a => a.ItemSpec).OrderBy(d =>
                  d.IndexOf("System.Runtime.dll", StringComparison.OrdinalIgnoreCase) != -1 ? 0 :
                  d.IndexOf("netstandard.dll", StringComparison.OrdinalIgnoreCase) != -1 ? 1 :
                  d.IndexOf("mscorlib.dll", StringComparison.OrdinalIgnoreCase) != -1 ? 2 : 3))
                {
                    try
                    {
                        // Expand environment variables
                        path = Environment.ExpandEnvironmentVariables(dllPath);

                        // If x86 but it didn't exist, assume it's a 32-bit system and change the name
                        if(path.IndexOf("%ProgramFiles(x86)%", StringComparison.Ordinal) != -1)
                            path = Environment.ExpandEnvironmentVariables(path.Replace("(x86)", String.Empty));

                        apiVisitor.LoadAssemblies(path);
                    }
                    catch(IOException e)
                    {
                        this.WriteMessage(MessageLevel.Error, "An error occurred while loading " +
                            "assemblies for reflection. The error message is: {0}", e.Message);
                        return false;
                    }
                }

                this.WriteMessage(MessageLevel.Info, "Loaded {0} assemblies for reflection and " +
                    "{1} dependency assemblies.", apiVisitor.Assemblies.Count(),
                    apiVisitor.AccessoryAssemblies.Count());

                apiVisitor.VisitApis();

                if(apiVisitor.Canceled)
                    this.WriteMessage(MessageLevel.Error, "MRefBuilder task canceled");
                else
                {
                    this.WriteMessage(MessageLevel.Info, "Wrote information on {0} namespaces, " +
                        "{1} types, and {2} members", apiVisitor.NamespaceCount, apiVisitor.TypeCount,
                        apiVisitor.MemberCount);

                    this.WriteMessage(MessageLevel.Info, "Merging duplicate type and member information");

                    var (mergedTypes, mergedMembers) = apiVisitor.MergeDuplicateReflectionData(this.ReflectionFilename);

                    this.WriteMessage(MessageLevel.Info, "Merged {0} duplicate type(s) and {1} " +
                        "duplicate member(s)", mergedTypes, mergedMembers);
                }
            }
            finally
            {
                apiVisitor?.Dispose();
                output?.Close();
            }

            return apiVisitor != null && !apiVisitor.Canceled;
        }
        #endregion

        #region Event handlers and helper methods
        //=====================================================================

        /// <summary>
        /// This is used to report unresolved assembly information
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>An unresolved assembly reference will terminate the application</remarks>
        private void UnresolvedAssemblyReferenceHandler(object sender, AssemblyReferenceEventArgs e)
        {
            this.WriteMessage(MessageLevel.Error, "Unresolved assembly reference: {0} ({1}) required by {2}",
                e.Reference.Name, e.Reference.StrongName, e.Referrer.Name);

            // If in the debugger, break so that we can see what was missed
            if(System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();

            Environment.Exit(1);
        }

        /// <summary>
        /// Write a formatted message to the task log with the given parameters
        /// </summary>
        /// <param name="level">The log level of the message</param>
        /// <param name="format">The message format string</param>
        /// <param name="args">The list of arguments to format into the message</param>
        private void WriteMessage(MessageLevel level, string format, params object[] args)
        {
            switch(level)
            {
                case MessageLevel.Diagnostic:
                    this.Log.LogMessage(MessageImportance.High, format, args);
                    break;

                case MessageLevel.Warn:
                    this.Log.LogWarning(null, null, null, this.GetType().Name, 0, 0, 0, 0, format, args);
                    break;

                case MessageLevel.Error:
                    this.Log.LogError(null, null, null, this.GetType().Name, 0, 0, 0, 0, format, args);
                    break;

                default:     // Info or unknown level
                    this.Log.LogMessage(format, args);
                    break;
            }
        }
        #endregion
    }
}
