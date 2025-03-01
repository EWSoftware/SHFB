//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : AdditionalNoticesPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/26/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a plug-in designed to add additional notice message definitions to the selected
// presentation style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/25/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

using Sandcastle.Core.PresentationStyle.Transformation;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is designed to add additional notice definitions to the selected presentation style
    /// </summary>
    /// <remarks>Notices are messages that appear at the top of topics and in tags on the member list pages to
    /// indicate special situations such as preliminary, obsolete, or experimental APIs.  They are detected
    /// based on the presence of an XML comment element or attribute on the type or member.</remarks>
    [HelpFileBuilderPlugInExport("Additional Notices", RunsInPartialBuild = false,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
      Description = "This plug-in is used to add additional notice messages to topics and tags to the member " +
        "list topics to indicate such things as preliminary, obsolete, or experimental APIs.")]
    public sealed class AdditionalNoticesPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private BuildProcess builder;
        private HashSet<string> attributeNames, elementNames;

        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in should
        /// be invoked during the build process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints { get; } = new List<ExecutionPoint>
        {
            new ExecutionPoint(BuildStep.GenerateReflectionInfo, ExecutionBehaviors.Before),
            new ExecutionPoint(BuildStep.CreateBuildAssemblerConfigs, ExecutionBehaviors.After)
        };

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

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            if(configuration.IsEmpty)
                throw new BuilderException("AN0001", "The Additional Notices plug-in has not been configured yet");

            // Load the notice settings
            attributeNames = new HashSet<string>();
            elementNames = new HashSet<string>();

            int noticeCount = 0;

            foreach(var n in configuration.Descendants("Notice"))
            {
                var notice = Notice.FromXml(n);
                
                builder.PresentationStyle.TopicTransformation.AddNoticeDefinition(notice);

                // Keep track of attribute names and element names as they need adding to the MRefBuilder
                // attribute filter and the Build Assembler configuration file.
                if(!String.IsNullOrWhiteSpace(notice.AttributeTypeName))
                    attributeNames.Add(notice.AttributeTypeName);

                if(!String.IsNullOrWhiteSpace(notice.ElementName))
                    elementNames.Add(notice.ElementName);

                noticeCount++;
            }

            if(noticeCount == 0)
            {
                throw new BuilderException("AN0002", "At least one notice definition is required for the " +
                    "Additional Notices plug-in.");
            }
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            // Add additional attribute types to the MRefBuilder API filter so that they are not removed if
            // attributes are not included.
            if(context.BuildStep == BuildStep.GenerateReflectionInfo && attributeNames.Count != 0)
            {
                string configFile = Directory.EnumerateFiles(builder.WorkingFolder, "MRefBuilder.config").FirstOrDefault();

                // The file should be there.  The build will fail if it doesn't exist so just return.
                if(configFile == null)
                    return;

                builder.ReportProgress("Adding attribute type names to the API filter");

                var config = XDocument.Load(configFile);
                var attributeFilter = config.Descendants("attributeFilter").FirstOrDefault();

                if(attributeFilter != null)
                {
                    foreach(string attributeName in attributeNames)
                    {
                        string ns, typeName;
                        int pos = attributeName.LastIndexOf('.');

                        if(pos != -1)
                        {
                            ns = attributeName.Substring(0, pos);
                            typeName = attributeName.Substring(pos + 1);

                            // The prefix isn't used in the API filter
                            if(ns[1] == ':')
                                ns = ns.Substring(2);
                        }
                        else
                        {
                            // Unlikely, but it's probably in the global namespace
                            ns = String.Empty;
                            typeName = attributeName;
                        }

                        var match = attributeFilter.Descendants("namespace").FirstOrDefault(
                            n => n.Attribute("name")?.Value == ns);

                        if(match != null)
                        {
                            var typeMatch = attributeFilter.Descendants("type").FirstOrDefault(
                                t => t.Attribute("name")?.Value == typeName);

                            if(typeMatch != null)
                            {
                                typeMatch.RemoveAttributes();
                                typeMatch.Add(new XAttribute("required", "true"));
                            }
                            else
                            {
                                match.Add(new XElement("type",
                                    new XAttribute("name", typeName),
                                    new XAttribute("required", "true")));
                            }
                        }
                        else
                        {
                            attributeFilter.Add(new XElement("namespace",
                                new XAttribute("name", ns),
                                new XAttribute("expose", "true"),
                                    new XElement("type", new XAttribute("name", typeName),
                                        new XAttribute("required", "true"))));
                        }
                    }

                    config.Save(configFile);
                }
                else
                {
                    builder.ReportWarning("AN0003", "Unable to locate attributeFilter element.  " +
                        "Unable to add additional attribute types to the filter.");
                }
            }

            // Add additional element names to the comments copied into member list topic elements
            if(context.BuildStep == BuildStep.CreateBuildAssemblerConfigs && elementNames.Count != 0)
            {
                string configFile = Directory.EnumerateFiles(builder.WorkingFolder, "BuildAssembler.config").FirstOrDefault();

                // The file should be there.  The build will fail if it doesn't exist so just return.
                if(configFile == null)
                    return;

                builder.ReportProgress("Adding element names to the Copy From Index component");

                var config = XDocument.Load(configFile);
                var copyElement = config.Descendants("copy").Where(c => c.Attribute("name")?.Value == "comments" &&
                    (c.Attribute("source")?.Value.IndexOf("|preliminary") ?? -1) != -1).FirstOrDefault();

                if(copyElement != null)
                {
                    copyElement.Attribute("source").Value += "|" + String.Join("|", elementNames);
                    config.Save(configFile);
                }
                else
                {
                    builder.ReportWarning("AN0004", "Unable to locate Copy From Index component element.  " +
                        "Unable to add additional notice text elements.");
                }
            }
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
