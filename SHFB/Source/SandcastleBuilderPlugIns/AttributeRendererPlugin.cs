//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : XPathReflectionFileFilterPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)  Based on code by Eyal Post
// Updated : 06/17/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a plug-in that is used to filter out unwanted information from the reflection information
// file using XPath queries.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/31/2008  EFW  Created the code
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

// Ignore Spelling: Eyal

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is used to define how attributes are rendered
    /// </summary>
    [HelpFileBuilderPlugInExport("Attribute Renderer Plugin", RunsInPartialBuild = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright + "\r\nBased on code submitted by " +
      "Eyal Post", Description = "This plug-in is used to define how attributes such as 'Obsolete' are rendered.")]
    public sealed class AttributeRendererPlugin : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;
        private BuildProcess builder;
        private readonly ObservableCollection<AttributeRepresentationEntry> attributeRepresentationEntries = new ObservableCollection<AttributeRepresentationEntry>();

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
                        new ExecutionPoint(BuildStep.ApplyDocumentModel, ExecutionBehaviors.Before)
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

            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            this.ReadConfiguration(configuration);
            
            if(attributeRepresentationEntries.Count == 0)
            {
                throw new BuilderException("ATTR0001", "No AttributeRepresentations defined " +
                                                       "Attribute Renderer plug-in");
            }
        }

        public void ReadConfiguration(XElement configuration)
        {
            foreach(var expr in configuration.Descendants("AttributeRepresentationEntry"))
                AttributeRepresentationEntries.Add(AttributeRepresentationEntry.FromXml(this, expr));
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            foreach(var item in attributeRepresentationEntries)
            {
                if (item.HasLongRepresentation) builder.PresentationStyle.LongAttributeRepresentations[item.AttributeClassName] = item.GetLongRepresentation();
                if (item.HasShortRepresentation) builder.PresentationStyle.ShortAttributeRepresentations[item.AttributeClassName] = item.GetShortRepresentation();
            }
        }
        #endregion

        #region Properties
        
        public ObservableCollection<AttributeRepresentationEntry> AttributeRepresentationEntries => attributeRepresentationEntries;
        
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
