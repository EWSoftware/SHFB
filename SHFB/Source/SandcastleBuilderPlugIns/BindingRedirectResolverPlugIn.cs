//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : BindingRedirectResolverPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/30/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that is used to add assembly binding
// redirection support to the MRefBuilder configuration file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.1  11/07/2008  EFW  Created the code
//=============================================================================

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is used to add assembly binding redirection support to the MRefBuilder
    /// configuration file.
    /// </summary>
    public class BindingRedirectResolverPlugIn : SandcastleBuilder.Utils.PlugIn.IPlugIn
    {
        #region Private data members
        //=====================================================================

        private ExecutionPointCollection executionPoints;
        private BuildProcess builder;

        private bool useGac;
        private BindingRedirectSettingsCollection redirects;
        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a friendly name for the plug-in
        /// </summary>
        public string Name
        {
            get { return "Assembly Binding Redirection"; }
        }

        /// <summary>
        /// This read-only property returns the version of the plug-in
        /// </summary>
        public Version Version
        {
            get
            {
                // Use the assembly version
                Assembly asm = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

                return new Version(fvi.ProductVersion);
            }
        }

        /// <summary>
        /// This read-only property returns the copyright information for the plug-in
        /// </summary>
        public string Copyright
        {
            get
            {
                // Use the assembly copyright
                Assembly asm = Assembly.GetExecutingAssembly();
                AssemblyCopyrightAttribute copyright =
                    (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(asm,
                        typeof(AssemblyCopyrightAttribute));

                return copyright.Copyright;
            }
        }

        /// <summary>
        /// This read-only property returns a brief description of the plug-in
        /// </summary>
        public string Description
        {
            get
            {
                return "This plug in is used to add assembly binding redirection support to the " +
                    "MRefBuilder configuration file.";
            }
        }

        /// <summary>
        /// This plug-in runs in partial builds
        /// </summary>
        public bool RunsInPartialBuild
        {
            get { return false; }
        }

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in
        /// should be invoked during the build process.
        /// </summary>
        public ExecutionPointCollection ExecutionPoints
        {
            get
            {
                if(executionPoints == null)
                {
                    executionPoints = new ExecutionPointCollection();

                    executionPoints.Add(new ExecutionPoint(BuildStep.GenerateReflectionInfo,
                        ExecutionBehaviors.Before));
                }

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
            using(BindingRedirectResolverConfigDlg dlg = new BindingRedirectResolverConfigDlg(
              project, currentConfig))
            {
                if(dlg.ShowDialog() == DialogResult.OK)
                    currentConfig = dlg.Configuration;
            }

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build
        /// process.</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize
        /// itself.</param>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            XPathNavigator root;

            builder = buildProcess;

            builder.ReportProgress("{0} Version {1}\r\n{2}\r\n", this.Name, this.Version, this.Copyright);

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
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            XmlDocument config = new XmlDocument();
            XmlAttribute attr;
            XmlNode resolver, ddue;

            config.Load(builder.WorkingFolder + "MRefBuilder.config");
            resolver = config.SelectSingleNode("configuration/dduetools/resolver");

            if(resolver == null)
            {
                ddue = config.SelectSingleNode("configuration/dduetools");

                if(ddue == null)
                    throw new BuilderException("ABR0002", "Unable to locate configuration/dduetools " +
                        "or its child resolver element in MRefBuilder.config");

                builder.ReportProgress("Default resolver element not found, adding new element");
                resolver = config.CreateNode(XmlNodeType.Element, "resolver", null);
                ddue.AppendChild(resolver);

                attr = config.CreateAttribute("type");
                attr.Value = "Microsoft.Ddue.Tools.Reflection.AssemblyResolver";
                resolver.Attributes.Append(attr);

                attr = config.CreateAttribute("assembly");
                attr.Value = builder.TransformText(@"{@SandcastlePath}ProductionTools\MRefBuilder.exe");
                resolver.Attributes.Append(attr);

                attr = config.CreateAttribute("use-gac");
                attr.Value = "false";
                resolver.Attributes.Append(attr);
            }

            // Allow turning GAC resolution on
            resolver.Attributes["use-gac"].Value = useGac.ToString().ToLowerInvariant();

            builder.ReportProgress("Adding binding redirections to assembly resolver configuration:");

            foreach(BindingRedirectSettings brs in redirects)
                builder.ReportProgress("    {0}", brs);

            redirects.ToXml(config, resolver);

            config.Save(builder.WorkingFolder + "MRefBuilder.config");
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the plug-in if not done explicity
        /// with <see cref="Dispose()"/>.
        /// </summary>
        ~BindingRedirectResolverPlugIn()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the plug-in object
        /// </summary>
        /// <overloads>There are two overloads for this method.</overloads>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This can be overridden by derived classes to add their own disposal code if necessary
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to
        /// just dispose of the unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to dispose of in this one
        }
        #endregion
    }
}
