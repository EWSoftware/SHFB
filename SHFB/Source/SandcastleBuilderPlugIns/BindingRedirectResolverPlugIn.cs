//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : BindingRedirectResolverPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/05/2016
// Note    : Copyright 2008-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that is used to add assembly binding redirection support to the MRefBuilder
// configuration file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/07/2008  EFW  Created the code
// 11/25/2012  EFW  Added support for the ignoreIfUnresolved configuration element
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is used to add assembly binding redirection support to the MRefBuilder configuration
    /// file.
    /// </summary>
    [HelpFileBuilderPlugInExport("Assembly Binding Redirection", IsConfigurable = true, RunsInPartialBuild = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
      Description = "This plug-in is used to add assembly binding redirection support to the MRefBuilder " +
        "configuration file.")]
    public sealed class BindingRedirectResolverPlugIn : SandcastleBuilder.Utils.BuildComponent.IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;
        private BuildProcess builder;

        private bool useGac;
        private BindingRedirectSettingsCollection redirects;
        private List<string> ignoreIfUnresolved;
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
                        new ExecutionPoint(BuildStep.GenerateReflectionInfo, ExecutionBehaviors.Before)
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
            using(BindingRedirectResolverConfigDlg dlg = new BindingRedirectResolverConfigDlg(project,
              currentConfig))
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
            XPathNavigator root;

            ignoreIfUnresolved = new List<string>();

            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            root = configuration.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                throw new BuilderException("ABR0001", "The Assembly Binding Redirection Resolver plug-in " +
                    "has not been configured yet");

            // Load the configuration
            string value = root.GetAttribute("useGAC", String.Empty);

            if(!Boolean.TryParse(value, out useGac))
                useGac = false;

            redirects = new BindingRedirectSettingsCollection();
            redirects.FromXml(builder.CurrentProject, root);

            foreach(XPathNavigator nav in root.Select("ignoreIfUnresolved/assemblyIdentity/@name"))
                ignoreIfUnresolved.Add(nav.Value);
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            XmlDocument config = new XmlDocument();
            XmlAttribute attr;
            XmlNode resolver, ddue, ignoreNode, assemblyNode;
            string configFile = builder.WorkingFolder + "MRefBuilder.config";

            // If the project doesn't exist we have nothing to do.  However, it could be that some other plug-in
            // has bypassed it so only issue a warning.
            if(!File.Exists(configFile))
            {
                builder.ReportWarning("ABR0003", "The MRefBuilder configuration file '{0}' could not be " +
                    "found.  The Assembly Binding Redirection plug-in did not run.", configFile);
                return;
            }

            config.Load(configFile);
            resolver = config.SelectSingleNode("configuration/dduetools/resolver");

            if(resolver == null)
            {
                ddue = config.SelectSingleNode("configuration/dduetools");

                if(ddue == null)
                    throw new BuilderException("ABR0002", "Unable to locate configuration/dduetools or its " +
                        "child resolver element in MRefBuilder.config");

                builder.ReportProgress("Default resolver element not found, adding new element");
                resolver = config.CreateNode(XmlNodeType.Element, "resolver", null);
                ddue.AppendChild(resolver);

                attr = config.CreateAttribute("type");
                attr.Value = "Microsoft.Ddue.Tools.Reflection.AssemblyResolver";
                resolver.Attributes.Append(attr);

                attr = config.CreateAttribute("assembly");
                attr.Value = builder.SubstitutionTags.TransformText(@"{@SHFBFolder}MRefBuilder.exe");
                resolver.Attributes.Append(attr);

                attr = config.CreateAttribute("use-gac");
                attr.Value = "false";
                resolver.Attributes.Append(attr);
            }

            // Allow turning GAC resolution on
            resolver.Attributes["use-gac"].Value = useGac.ToString().ToLowerInvariant();

            if(redirects.Count != 0)
            {
                builder.ReportProgress("Adding binding redirections to assembly resolver configuration:");

                foreach(BindingRedirectSettings brs in redirects)
                    builder.ReportProgress("    {0}", brs);

                redirects.ToXml(config, resolver, false);
            }

            if(ignoreIfUnresolved.Count != 0)
            {
                builder.ReportProgress("Adding ignored assembly names to assembly resolver configuration:");

                ignoreNode = resolver.SelectSingleNode("ignoreIfUnresolved");

                if(ignoreNode == null)
                {
                    ignoreNode = config.CreateNode(XmlNodeType.Element, "ignoreIfUnresolved", null);
                    resolver.AppendChild(ignoreNode);
                }
                else
                    ignoreNode.RemoveAll();

                foreach(string ignoreName in ignoreIfUnresolved)
                {
                    assemblyNode = config.CreateNode(XmlNodeType.Element, "assemblyIdentity", null);
                    ignoreNode.AppendChild(assemblyNode);

                    attr = config.CreateAttribute("name");
                    attr.Value = ignoreName;
                    assemblyNode.Attributes.Append(attr);

                    builder.ReportProgress("    {0}", ignoreName);
                }
            }

            config.Save(configFile);
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
