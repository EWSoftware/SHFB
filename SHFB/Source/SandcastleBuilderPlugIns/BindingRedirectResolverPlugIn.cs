//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : BindingRedirectResolverPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/14/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
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

// Ignore Spelling: gac

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is used to add assembly binding redirection support to the MRefBuilder configuration
    /// file.
    /// </summary>
    [HelpFileBuilderPlugInExport("Assembly Binding Redirection", RunsInPartialBuild = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
      Description = "This plug-in is used to add assembly binding redirection support to the MRefBuilder " +
        "configuration file.")]
    public sealed class BindingRedirectResolverPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;
        private BuildProcess builder;

        private bool useGac;
        private List<BindingRedirectSettings> redirects;
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
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        public void Initialize(BuildProcess buildProcess, XElement configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            builder = buildProcess ?? throw new ArgumentNullException(nameof(buildProcess));

            redirects = new List<BindingRedirectSettings>();
            ignoreIfUnresolved = new List<string>();

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            // Transform the configuration elements to handle substitution tags
            string config = buildProcess.SubstitutionTags.TransformText(configuration.ToString());

            var settings = XElement.Parse(config);

            if(settings.IsEmpty)
            {
                throw new BuilderException("ABR0001", "The Assembly Binding Redirection Resolver plug-in " +
                    "has not been configured yet");
            }

            // Load the configuration
            useGac = (bool)settings.Attribute("useGAC");

            foreach(var r in settings.Element("assemblyBinding").Descendants("dependentAssembly"))
                redirects.Add(BindingRedirectSettings.FromXml(builder.CurrentProject, r));

            foreach(var i in settings.Element("ignoreIfUnresolved").Descendants("assemblyIdentity"))
                ignoreIfUnresolved.Add(i.Attribute("name").Value);
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            string configFile = Path.Combine(builder.WorkingFolder, "MRefBuilder.config");

            // If the project doesn't exist we have nothing to do.  However, it could be that some other plug-in
            // has bypassed it so only issue a warning.
            if(!File.Exists(configFile))
            {
                builder.ReportWarning("ABR0003", "The MRefBuilder configuration file '{0}' could not be " +
                    "found.  The Assembly Binding Redirection plug-in did not run.", configFile);
                return;
            }

            var config = XDocument.Load(configFile);
            var ddueTools = config.Root.Element("dduetools");
    
            if(ddueTools == null)
            {
                throw new BuilderException("ABR0002", "Unable to locate configuration/dduetools element in " +
                    "MRefBuilder.config");
            }

            var resolver = ddueTools.Element("resolver");

            if(resolver == null)
            {
                builder.ReportProgress("Default resolver element not found, adding new element");
                resolver = new XElement("resolver",
                    new XAttribute("type", "Sandcastle.Tools.Reflection.AssemblyResolver"),
                    new XAttribute("use-gac", "false"));
            }

            // Allow turning GAC resolution on
            resolver.Attribute("use-gac").Value = useGac.ToString().ToLowerInvariant();

            if(redirects.Count != 0)
            {
                builder.ReportProgress("Adding binding redirections to assembly resolver configuration:");

                foreach(var brs in redirects)
                {
                    builder.ReportProgress("    {0}", brs.BindingRedirectDescription);
                    resolver.Add(brs.ToXml(false));
                }
            }

            if(ignoreIfUnresolved.Count != 0)
            {
                builder.ReportProgress("Adding ignored assembly names to assembly resolver configuration:");

                var ignoreNode = resolver.Element("ignoreIfUnresolved");

                if(ignoreNode == null)
                {
                    ignoreNode = new XElement("ignoreIfUnresolved");
                    resolver.Add(ignoreNode);
                }
                else
                    ignoreNode.RemoveAll();

                foreach(string ignoreName in ignoreIfUnresolved)
                {
                    builder.ReportProgress("    {0}", ignoreName);
                    ignoreNode.Add(new XElement("assemblyIdentity", new XAttribute("name", ignoreName)));
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
