//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PresentationStyleSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/23/2022
// Note    : Copyright 2012-2022, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to contain settings information for a specific presentation style
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/24/2012  EFW  Created the code
// 06/21/2013  EFW  Added support for format-specific help content files
// 11/30/2013  EFW  Merged changes from Stazzz to support namespace grouping
// 01/04/2014  EFW  Moved the code into Sandcastle.Core and made it an abstract base class
// 05/14/2014  EFW  Added support for defining dependent plug-ins
// 02/27/2022  EFW  Added support for code-base transformations
//===============================================================================================================

// Ignore Spelling: Stazzz

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Sandcastle.Core.PresentationStyle.Transformation;

namespace Sandcastle.Core.PresentationStyle
{
    /// <summary>
    /// This abstract base class is used to define the settings and common functionality for a specific
    /// presentation style.
    /// </summary>
    /// <remarks>Presentation styles are singletons by nature.  The composition container will create instances
    /// as needed.</remarks>
    public abstract class PresentationStyleSettings
    {
        #region Private data members
        //=====================================================================

        private readonly List<ContentFiles> contentFiles;
        private readonly List<PlugInDependency> plugInDependencies;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is overridden in derived classes to provide the location of the presentation style files
        /// </summary>
        /// <value>Typically, this will return the path of the executing assembly</value>
        public abstract string Location { get; }

        /// <summary>
        /// This is used to get or set the presentation style base path used to resolve relative paths within the
        /// presentation style.
        /// </summary>
        /// <value>If null or empty, the <see cref="Location"/> path is used as the base path.  If relative, it
        /// is considered to be relative to the <see cref="Location"/> path.</value>
        /// <remarks>This is useful for defining a base path with multiple presentation styles are included
        /// such as with the standard presentation styles.</remarks>
        public string BasePath { get; protected set; }

        /// <summary>
        /// This is used to get or set the help file formats supported by the presentation style
        /// </summary>
        public HelpFileFormats SupportedFormats { get; protected set; }

        /// <summary>
        /// This is used to get or set whether or not namespace grouping is supported by the presentation style
        /// </summary>
        public bool SupportsNamespaceGrouping { get; protected set; }

        /// <summary>
        /// This is used to get or set whether or not code snippet grouping is supported by the presentation
        /// style.
        /// </summary>
        /// <remarks>If true, code snippets will be grouped and sorted based on the syntax generators present
        /// in the project.</remarks>
        public bool SupportsCodeSnippetGrouping { get; protected set; }

        /// <summary>
        /// This read-only property returns the list of help content file locations
        /// </summary>
        public IList<ContentFiles> ContentFiles => contentFiles;

        /// <summary>
        /// This is used to get or set the document model applicator
        /// </summary>
        public IApplyDocumentModel DocumentModelApplicator { get; protected set; }

        /// <summary>
        /// This is used to get or set the table of content generator for API content
        /// </summary>
        public IApiTocGenerator ApiTableOfContentsGenerator { get; set; }

        /// <summary>
        /// This is used to get or set the BuildAssembler configuration filename
        /// </summary>
        public string BuildAssemblerConfiguration { get; protected set; }

        /// <summary>
        /// This read-only property returns the topic transformation to use
        /// </summary>
        public TopicTransformationCore TopicTranformation { get; protected set; }

        /// <summary>
        /// This read-only property returns any plug-in dependencies required by the presentation style
        /// </summary>
        /// <remarks>This is used to ensure that any dependent plug-ins are added to the build.  If any of the
        /// plug-ins are visible to the user and have been added to the project, the project configuration will
        /// override the default configuration supplied here.</remarks>
        public IList<PlugInDependency> PlugInDependencies => plugInDependencies;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        protected PresentationStyleSettings()
        {
            contentFiles = new List<ContentFiles>();
            plugInDependencies = new List<PlugInDependency>();
        }
        #endregion

        #region Helper methods
        //=====================================================================


        /// <summary>
        /// This is used to get an enumerable list of BuildAssembler resource item files used by the presentation
        /// style.
        /// </summary>
        /// <param name="languageName">The language name for the localized resources.  If the specific language
        /// is not found, it falls back to the en-US resources which will always exist.</param>
        /// <returns>An enumerable list of resource item files used by the presentation style</returns>
        public abstract IEnumerable<string> ResourceItemFiles(string languageName);

        /// <summary>
        /// This is used to check the presentation style for errors
        /// </summary>
        /// <returns>An enumerable list of problems found or an empty list if everything is okay</returns>
        public IEnumerable<string> CheckForErrors()
        {
            List<string> errors = new List<string>();

            if(this.SupportedFormats == 0)
                errors.Add(nameof(SupportedFormats) + " has not been specified");

            if(this.DocumentModelApplicator == null)
                errors.Add(nameof(DocumentModelApplicator) + " has not been specified");

            if(this.ApiTableOfContentsGenerator == null)
                errors.Add(nameof(ApiTableOfContentsGenerator) + " has not been specified");

            if(this.TopicTranformation == null)
                errors.Add(nameof(TopicTranformation) + " has not been specified");

            if(String.IsNullOrWhiteSpace(this.BuildAssemblerConfiguration))
                errors.Add(nameof(BuildAssemblerConfiguration) + " has not been specified");

            if(!this.ResourceItemFiles("en-US").Any())
                errors.Add(nameof(ResourceItemFiles) + " did not return any resource item files");

            return errors;
        }

        /// <summary>
        /// This is used to resolve a presentation style path
        /// </summary>
        /// <param name="path">The path to resolve</param>
        /// <returns>The resolved path</returns>
        /// <remarks>Environment variables in the path will be expanded.  If relative, the path is converted
        /// to a full path relative to the presentation style's base path.</remarks>
        public string ResolvePath(string path)
        {
            string rootPath;

            path = ResolveEnvironmentVariables(path);

            if(!Path.IsPathRooted(path))
            {
                rootPath = ResolveEnvironmentVariables(this.BasePath);

                if(path == null || path.Equals(rootPath, StringComparison.OrdinalIgnoreCase))
                    path = String.Empty;

                if(!Path.IsPathRooted(rootPath))
                    rootPath = Path.Combine(this.Location, rootPath ?? String.Empty);

                path = Path.Combine(rootPath, path);
            }

            return path;
        }

        /// <summary>
        /// This is used to resolve environment variables in a path with the added step of resolving
        /// <c>%SHFBROOT%</c> to the path found by the component utilities if it does not resolve automatically.
        /// </summary>
        /// <param name="path">The path in which to resolve an environment variable</param>
        /// <returns>The resolved path value</returns>
        private static string ResolveEnvironmentVariables(string path)
        {
            if(!String.IsNullOrWhiteSpace(path))
            {
                path = Environment.ExpandEnvironmentVariables(path);

                if(path.IndexOf("%SHFBROOT%", StringComparison.Ordinal) != -1)
                    path = path.Replace("%SHFBROOT%", ComponentUtilities.RootFolder + Path.DirectorySeparatorChar);
            }

            return path;
        }

        /// <summary>
        /// This is used to copy the presentation style help file content to the given destination folder
        /// </summary>
        /// <param name="format">The help file format for which to copy files</param>
        /// <param name="destinationBasePath">The destination base path to which the files are copied</param>
        /// <param name="progressReporter">An optional action delegate used to report progress</param>
        /// <param name="transformTemplate">A action delegate used to transform a template file (file, source
        /// folder, destination folder)</param>
        public void CopyHelpContent(HelpFileFormats format, string destinationBasePath,
          Action<string, object[]> progressReporter, Action<string, string, string> transformTemplate)
        {
            string sourcePath, destPath;

            if(transformTemplate == null)
                throw new ArgumentNullException(nameof(transformTemplate));

            foreach(var content in contentFiles)
            {
                if((content.HelpFileFormats & format) != 0)
                {
                    if(content.BasePath == null)
                        sourcePath = this.ResolvePath(content.SourcePathWildcard);
                    else
                        sourcePath = this.ResolvePath(Path.Combine(content.BasePath, content.SourcePathWildcard));

                    if(content.DestinationFolder == null)
                        destPath = Path.Combine(destinationBasePath,
                            Path.GetFileName(Path.GetDirectoryName(content.SourcePathWildcard)));
                    else
                        if(content.DestinationFolder.Length == 0)
                            destPath = destinationBasePath;
                        else
                            destPath = Path.Combine(destinationBasePath, content.DestinationFolder);

                    RecursiveCopy(sourcePath, destPath, progressReporter, content.TemplateFileExtensions,
                        transformTemplate);
                }
            }
        }

        /// <summary>
        /// This copies files from the specified source folder to the specified destination folder.  If any
        /// subfolders are found below the source folder and the wildcard is "*.*", the subfolders are also
        /// copied recursively.
        /// </summary>
        /// <param name="sourcePath">The source path from which to copy</param>
        /// <param name="destPath">The destination path to which to copy</param>
        /// <param name="progressReporter">An optional action delegate used to report progress</param>
        /// <param name="templateFileExtensions">An enumerable list of file extensions that will be treated like
        /// template files and will have substitution tags replaced.</param>
        /// <param name="transformTemplate">A action delegate used to transform a template file (file, source
        /// folder, destination folder)</param>
        private static void RecursiveCopy(string sourcePath, string destPath,
          Action<string, object[]> progressReporter, IEnumerable<string> templateFileExtensions,
          Action<string, string, string> transformTemplate)
        {
            if(sourcePath == null)
                throw new ArgumentNullException(nameof(sourcePath));

            if(destPath == null)
                throw new ArgumentNullException(nameof(destPath));

            int idx = sourcePath.LastIndexOf('\\');

            string dirName = sourcePath.Substring(0, idx), fileSpec = sourcePath.Substring(idx + 1), filename;

            if(destPath[destPath.Length - 1] != '\\')
                destPath += @"\";

            foreach(string name in Directory.EnumerateFiles(dirName, fileSpec))
            {
                filename = destPath + Path.GetFileName(name);

                // Don't overwrite content that was copied from the project or by a plug-in.  This allows project
                // content to override the standard content.
                if(!File.Exists(filename))
                {
                    if(!Directory.Exists(destPath))
                        Directory.CreateDirectory(destPath);

                    // Copy as-is or perform tag substitution
                    if(templateFileExtensions.Any(e => e.Equals(Path.GetExtension(name), StringComparison.OrdinalIgnoreCase)))
                        transformTemplate(Path.GetFileName(name), Path.GetDirectoryName(name), destPath);
                    else
                    {
                        File.Copy(name, filename, true);

                        // All attributes are turned off so that we can delete it later
                        File.SetAttributes(filename, FileAttributes.Normal);
                    }

                    progressReporter?.Invoke("{0} -> {1}", new[] { name, filename });
                }
            }

            // For "*.*", copy subfolders too
            if(fileSpec == "*.*")
            {
                // Ignore hidden folders as they may be under source control and are not wanted
                foreach(string folder in Directory.EnumerateDirectories(dirName))
                    if((File.GetAttributes(folder) & FileAttributes.Hidden) != FileAttributes.Hidden)
                        RecursiveCopy(folder + @"\*.*", destPath + folder.Substring(dirName.Length + 1) + @"\",
                            progressReporter, templateFileExtensions, transformTemplate);
            }
        }
        #endregion
    }
}
