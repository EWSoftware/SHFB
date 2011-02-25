//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConvertFromSandcastleGui.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to convert project files created by Stephan
// Smetsers Sandcastle GUI to the MSBuild format project files used by SHFB.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/23/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils.Conversion
{
    /// <summary>
    /// This class is used to convert Stephan Smetsers Sandcastle GUI project
    /// files to the MSBuild format project files used by the help file builder.
    /// </summary>
    public class ConvertFromSandcastleGui : ConvertToMSBuildFormat
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// An XML reader isn't used by this converter
        /// </summary>
        protected internal override XmlTextReader Reader
        {
            get { return null; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldProjectFile">The old project filename</param>
        /// <param name="folder">The folder in which to place the new project
        /// and its related files.  This cannot be the same folder as the
        /// old project file.</param>
        public ConvertFromSandcastleGui(string oldProjectFile, string folder) :
            base(oldProjectFile, folder)
        {
        }
        #endregion

        #region Conversion methods
        //=====================================================================

        /// <summary>
        /// This is used to perform the actual conversion
        /// </summary>
        /// <returns>The new project filename on success.  An exception is
        /// thrown if the conversion fails.</returns>
        public override string ConvertProject()
        {
            SandcastleProject project = base.Project;
            FileItem fileItem;
            List<string> syntaxFilters = new List<string> { "CSharp", "VisualBasic", "CPlusPlus" };
            string option, lastProperty = null, value;
            int pos;

            try
            {
                project.HelpTitle = project.HtmlHelpName =
                    Path.GetFileNameWithoutExtension(base.OldProjectFile);

                using(StreamReader sr = new StreamReader(base.OldProjectFile))
                    while(!sr.EndOfStream)
                    {
                        option = sr.ReadLine();

                        if(String.IsNullOrEmpty(option))
                            continue;

                        pos = option.IndexOf('=');
                        if(pos == -1)
                            continue;

                        lastProperty = option.Substring(0, pos).Trim().ToLower(
                            CultureInfo.InvariantCulture);
                        value = option.Substring(pos + 1).Trim();

                        switch(lastProperty)
                        {
                            case "copyright":
                                project.CopyrightText = value;
                                break;

                            case "productname":
                                project.HelpTitle = value;
                                break;

                            case "assemblydir":
                                project.DocumentationSources.Add(Path.Combine(
                                    value, "*.*"), null, null, false);
                                break;

                            case "docdir":
                                this.ConvertAdditionalContent(value);
                                break;

                            case "logo":
                                fileItem = project.AddFileToProject(value,
                                    Path.Combine(base.ProjectFolder,
                                    Path.GetFileName(value)));

                                // Since the logo is copied by the
                                // Post-Transform component, set the build
                                // action to None so that it isn't added
                                // to the help file as content.
                                fileItem.BuildAction = BuildAction.None;
                                break;

                            case "msdnlinks":
                                if(String.Compare(value, "false", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    project.HtmlSdkLinkType = project.WebsiteSdkLinkType = HtmlSdkLinkType.None;
                                    project.MSHelp2SdkLinkType = MSHelp2SdkLinkType.Index;
                                    project.MSHelpViewerSdkLinkType = MSHelpViewerSdkLinkType.Id;
                                }
                                break;

                            case "outputtype":
                                if(value.IndexOf("website",
                                  StringComparison.OrdinalIgnoreCase) != -1)
                                    project.HelpFileFormat = HelpFileFormat.HtmlHelp1 |
                                        HelpFileFormat.Website;
                                break;

                            case "friendlyfilenames":
                                if(String.Compare(value, "true",
                                  StringComparison.OrdinalIgnoreCase) == 0)
                                    project.NamingMethod = NamingMethod.MemberName;
                                break;

                            case "template":
                                project.PresentationStyle =
                                    PresentationStyleTypeConverter.FirstMatching(value);
                                break;

                            case "internals":
                                if(String.Compare(value, "true",
                                  StringComparison.OrdinalIgnoreCase) == 0)
                                    project.DocumentPrivates =
                                        project.DocumentInternals = true;
                                break;

                            case "cssyntaxdeclaration":
                                if(String.Compare(value, "false",
                                  StringComparison.OrdinalIgnoreCase) == 0)
                                    syntaxFilters.Remove("CSharp");
                                break;

                            case "vbsyntaxdeclaration":
                                if(String.Compare(value, "false",
                                  StringComparison.OrdinalIgnoreCase) == 0)
                                    syntaxFilters.Remove("VisualBasic");
                                break;

                            case "cppsyntaxdeclaration":
                                if(String.Compare(value, "false",
                                  StringComparison.OrdinalIgnoreCase) == 0)
                                    syntaxFilters.Remove("CPlusPlus");
                                break;

                            case "javascriptsyntaxdeclaration":
                                if(String.Compare(value, "true",
                                  StringComparison.OrdinalIgnoreCase) == 0)
                                    syntaxFilters.Add("JavaScript");
                                break;

                            default:    // Ignored
                                break;
                        }
                    }

                // Set the syntax filters
                project.SyntaxFilters = String.Join(", ", syntaxFilters.ToArray());

                base.CreateFolderItems();
                project.SaveProject(project.Filename);
            }
            catch(Exception ex)
            {
                throw new BuilderException("CVT0005", String.Format(
                    CultureInfo.CurrentCulture, "Error reading project " +
                    "from '{0}' (last property = {1}):\r\n{2}",
                    base.OldProjectFile, lastProperty, ex.Message), ex);
            }

            return project.Filename;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Add additional content to the project
        /// </summary>
        /// <param name="folder">The folder containing the content</param>
        private void ConvertAdditionalContent(string folder)
        {
            string fileSpec, source, dest, destFile;
            int pos;

            fileSpec = base.FullPath(Path.Combine(folder, "*.*"));

            source = Path.GetDirectoryName(fileSpec);
            dest = base.ProjectFolder;

            pos = source.LastIndexOf('\\');
            if(pos != -1)
                dest = Path.Combine(dest, source.Substring(pos + 1));

            foreach(string file in ExpandWildcard(fileSpec, true))
            {
                // Remove the base path but keep any subfolders
                destFile = file.Substring(source.Length + 1);
                destFile = Path.Combine(dest, destFile);
                base.Project.AddFileToProject(file, destFile);
            }
        }
        #endregion
    }
}
