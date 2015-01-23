//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : DbcsFixPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/22/2014
// Note    : Copyright 2008-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in designed to modify the HTML files and alter the build so as to overcome the
// encoding issues encountered when building HTML Help 1 (.chm) files for various foreign languages.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.6.0.5  02/18/2008  EFW  Created the code
// 1.8.0.0  07/15/2008  EFW  Updated for use with MSBuild project format
// 1.9.0.0  06/07/2010  EFW  Added support for multi-format build output
// -------  12/17/2013  EFW  Updated to use MEF for the plug-ins
//          07/31/2014  EFW  Made the localize app optional
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to modify the HTML files and alter the build so as to overcome the
    /// encoding issues encountered when building HTML Help 1 (.chm) files for various foreign languages.
    /// </summary>
    /// <remarks>This uses the <see href="http://www.steelbytes.com/?mid=45">Steel Bytes SBAppLocale</see> tool
    /// to run the HTML Help 1 compiler using the correct locale.</remarks>
    [HelpFileBuilderPlugInExport("DBCS Fix for CHM Builds", IsConfigurable = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright + "\r\nSBAppLocale is Copyright \xA9 " +
      "2005-2014 Steel Bytes, All Rights Reserved",
      Description = "This plug-in is used to modify the HTML files and alter the build so as to overcome the " +
        "encoding issues encountered when building HTML Help 1 (.chm) files for various foreign languages.")]
    public sealed class DbcsFixPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;

        private BuildProcess builder;

        private string sbAppLocalePath;
        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in should
        /// be invoked during the build process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints
        {
            get
            {
                if(executionPoints == null)
                    executionPoints = new List<ExecutionPoint>
                    {
                        new ExecutionPoint(BuildStep.ExtractingHtmlInfo, ExecutionBehaviors.Before),
                        new ExecutionPoint(BuildStep.CompilingHelpFile, ExecutionBehaviors.Before)
                    };

                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used by the Sandcastle Help File Builder to let the plug-in perform its own
        /// configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file builder project</remarks>
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            using(DbcsFixConfigDlg dlg = new DbcsFixConfigDlg(currentConfig))
            {
                if(dlg.ShowDialog() == DialogResult.OK)
                    currentConfig = dlg.Configuration;
            }

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            XPathNavigator root, node;

            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            root = configuration.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                throw new BuilderException("DFP0001", "The DBCS Fix plug-in has not been configured yet");

            node = root.SelectSingleNode("sbAppLocale");

            if(node != null)
                sbAppLocalePath = node.GetAttribute("path", String.Empty).Trim();

            if(String.IsNullOrWhiteSpace(sbAppLocalePath))
            {
                builder.ReportWarning("DFP0002", "A path to the Steel Bytes App Locale tool was not specified " +
                    "and it will not be used for this build.");
            }
            else
            {
                // If relative, the path is relative to the project folder
                sbAppLocalePath = FilePath.RelativeToAbsolutePath(builder.ProjectFolder,
                    builder.TransformText(sbAppLocalePath));

                if(!File.Exists(sbAppLocalePath))
                    throw new BuilderException("DFP0003", "Unable to locate SBAppLocale tool at " + sbAppLocalePath);
            }

            // If not building HTML Help 1, there's nothing to do
            if((builder.CurrentProject.HelpFileFormat & HelpFileFormats.HtmlHelp1) == 0)
            {
                executionPoints.Clear();
                builder.ReportWarning("DFP0007", "An HTML Help 1 file is not being built.  This plug-in will " +
                    "not be ran.");
            }
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            XmlNamespaceManager nsm;
            XmlDocument project;
            XmlNode property;
            string projectFile;

            // Localize the content when extracting keyword and TOC info
            if(context.BuildStep == BuildStep.ExtractingHtmlInfo)
            {
                // Since we need to localize all of the content, we must manually copy the presentation style
                // Help 1 content which isn't normally copied until after the current build step.  This assumes
                // that none of the replacement tags in the standard content depend on information generated in
                // this step (i.e. it wouldn't work for website output in the older presentation styles which
                // rely on the WebTOC.xml file for the index page).
                builder.ReportProgress("Copying Help 1 presentation style content ready for localization");

                builder.PresentationStyle.CopyHelpContent(HelpFileFormats.HtmlHelp1, String.Format(
                    CultureInfo.InvariantCulture, @"{0}Output\{1}", builder.WorkingFolder, HelpFileFormats.HtmlHelp1),
                    builder.ReportProgress, (name, source, dest) => builder.TransformTemplate(name, source, dest));

                builder.ReportProgress("Adding DBCS Fix localization folder");

                projectFile = builder.WorkingFolder + "ExtractHtmlInfo.proj";
                project = new XmlDocument();
                project.Load(projectFile);
                nsm = new XmlNamespaceManager(project.NameTable);
                nsm.AddNamespace("MSBuild", project.DocumentElement.NamespaceURI);

                property = project.SelectSingleNode("//MSBuild:LocalizedFolder", nsm);

                if(property == null)
                    throw new BuilderException("DFP0004", "Unable to locate LocalizedFolder element in " +
                        "project file");

                property.InnerText = @".\Localized";
                project.Save(projectFile);
                return;
            }

            if(builder.CurrentFormat != HelpFileFormats.HtmlHelp1)
                return;

            builder.ReportProgress("Adding localization options to build task");

            projectFile = builder.WorkingFolder + "Build1xHelpFile.proj";
            project = new XmlDocument();
            project.Load(projectFile);
            nsm = new XmlNamespaceManager(project.NameTable);
            nsm.AddNamespace("MSBuild", project.DocumentElement.NamespaceURI);

            property = project.SelectSingleNode("//MSBuild:WorkingFolder", nsm);

            if(property == null)
                throw new BuilderException("DFP0005", "Unable to locate WorkingFolder element in project file");

            property.InnerText = @".\Localized";

            property = project.SelectSingleNode("//MSBuild:LocalizeApp", nsm);

            if(property == null)
                throw new BuilderException("DFP0006", "Unable to locate LocalizeApp element in project file");

            property.InnerText = (sbAppLocalePath ?? String.Empty);
            project.Save(projectFile);
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the plug-in object
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose of in this one
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
