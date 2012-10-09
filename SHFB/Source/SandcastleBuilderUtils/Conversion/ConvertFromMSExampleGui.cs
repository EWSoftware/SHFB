//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConvertFromMSExampleGui.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/08/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to convert the Microsoft example Sandcastle
// GUI project files to the MSBuild format project files used by SHFB.
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
// 1.9.3.4  01/08/2012  EFW  Added constructor to support use from VSPackage
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils.Conversion
{
    /// <summary>
    /// This class is used to convert the Microsoft example Sandcastle GUI
    /// project files to the MSBuild format project files used by the help
    /// file builder.
    /// </summary>
    public class ConvertFromMSExampleGui : ConvertToMSBuildFormat
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

        #region Constructors
        //=====================================================================

        /// <inheritdoc />
        public ConvertFromMSExampleGui(string oldProjectFile, string folder) : base(oldProjectFile, folder)
        {
        }

        /// <inheritdoc />
        public ConvertFromMSExampleGui(string oldProjectFile, SandcastleProject newProject) :
            base(oldProjectFile, newProject)
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
            XmlNamespaceManager nsm;
            XmlDocument sourceFile;
            XPathNavigator navProject, navProp;
            SandcastleProject project = base.Project;
            string includePath, lastProperty = null;

            try
            {
                project.HelpTitle = project.HtmlHelpName =
                    Path.GetFileNameWithoutExtension(base.OldProjectFile);

                sourceFile = new XmlDocument();
                sourceFile.Load(base.OldProjectFile);
                nsm = new XmlNamespaceManager(sourceFile.NameTable);
                nsm.AddNamespace("prj", sourceFile.DocumentElement.NamespaceURI);

                navProject = sourceFile.CreateNavigator();

                // Get the name, topic style, and language ID
                lastProperty = "Name";
                navProp = navProject.SelectSingleNode(
                    "//prj:Project/prj:PropertyGroup/prj:Name", nsm);
                if(navProp != null)
                    project.HtmlHelpName = project.HelpTitle = navProp.Value;

                lastProperty = "TopicStyle";
                navProp = navProject.SelectSingleNode(
                    "//prj:Project/prj:PropertyGroup/prj:TopicStyle", nsm);
                if(navProp != null)
                    project.PresentationStyle = PresentationStyleTypeConverter.FirstMatching(
                        navProp.Value);

                lastProperty = "LanguageId";
                navProp = navProject.SelectSingleNode(
                    "//prj:Project/prj:PropertyGroup/prj:LanguageId", nsm);
                if(navProp != null)
                    project.Language = new CultureInfo(Convert.ToInt32(
                        navProp.Value, CultureInfo.InvariantCulture));

                // Add the documentation sources
                lastProperty = "Dlls|Comments";
                foreach(XPathNavigator docSource in navProject.Select(
                  "//prj:Project/prj:ItemGroup/prj:Dlls|" +
                  "//prj:Project/prj:ItemGroup/prj:Comments", nsm))
                {
                    includePath = docSource.GetAttribute("Include", String.Empty);

                    if(!String.IsNullOrEmpty(includePath))
                        project.DocumentationSources.Add(includePath, null,
                            null, false);
                }

                // Add the dependencies
                lastProperty = "Dependents";

                foreach(XPathNavigator dependency in navProject.Select(
                  "//prj:Project/prj:ItemGroup/prj:Dependents", nsm))
                {
                    includePath = dependency.GetAttribute("Include", String.Empty);

                    if(includePath.IndexOfAny(new char[] { '*', '?' }) == -1)
                        project.References.AddReference(Path.GetFileNameWithoutExtension(includePath), includePath);
                    else
                        foreach(string file in Directory.EnumerateFiles(Path.GetDirectoryName(includePath),
                          Path.GetFileName(includePath)).Where(f =>
                          f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                          f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)))
                            project.References.AddReference(Path.GetFileNameWithoutExtension(file), file);
                }

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
    }
}
