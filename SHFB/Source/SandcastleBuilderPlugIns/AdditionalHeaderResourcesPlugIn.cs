//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : AdditionalHeaderResourcesPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/17/2026
// Note    : Copyright 2026, Eric Woodruff, All rights reserved
//
// This file contains a plug-in that can be used to add additional header resources to HTML help topics such as
// metadata, style sheets, and script references.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/17/2026  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.PlugIn;
using Sandcastle.Core.Project;

namespace SandcastleBuilder.PlugIns;

/// <summary>
/// This plug-in class can be used to add additional header resources to HTML help topics such as metadata, style
/// sheets, and script references.
/// </summary>
[HelpFileBuilderPlugInExport("Additional Header Resources", Version = AssemblyInfo.ProductVersion,
  Copyright = AssemblyInfo.Copyright, Description = "This plug-in can be used to add additional header " +
  "resources to HTML help topics such as metadata, style sheets, and script references.")]
public sealed class AdditionalHeaderResourcesPlugIn : IPlugIn
{
    #region Private data members
    //=====================================================================

    private IBuildProcess builder;
    private XElement configuration;

    #endregion

    #region IPlugIn implementation
    //=====================================================================

    /// <summary>
    /// This read-only property returns a collection of execution points that define when the plug-in should
    /// be invoked during the build process.
    /// </summary>
    public IEnumerable<ExecutionPoint> ExecutionPoints { get; } =
    [
        new ExecutionPoint(BuildStep.MergeCustomConfigs, ExecutionBehaviors.After)
    ];

    /// <summary>
    /// This method is used to initialize the plug-in at the start of the build process
    /// </summary>
    /// <param name="buildProcess">A reference to the current build process</param>
    /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
    public void Initialize(IBuildProcess buildProcess, XElement configuration)
    {
        builder = buildProcess;
        this.configuration = configuration;

        var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
            typeof(HelpFileBuilderPlugInExportAttribute), false).First();

        builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

        if(!configuration.HasElements)
        {
            throw new BuilderException("AHR0001", "At least one header resource definition is required for the " +
                "Additional Header Resources plug-in.");
        }
    }

    /// <summary>
    /// This method is used to execute the plug-in during the build process
    /// </summary>
    /// <param name="context">The current execution context</param>
    public void Execute(ExecutionContext context)
    {
        if(context == null)
            throw new ArgumentNullException(nameof(context));

        // Skip unsupported formats
        if(builder.CurrentFormat == HelpFileFormats.Markdown || builder.CurrentFormat == HelpFileFormats.OpenXml)
            return;

        string configFile = Directory.EnumerateFiles(builder.WorkingFolder, "BuildAssembler.config").FirstOrDefault();

        // The file should be there.  The build will fail if it doesn't exist so just return.
        if(configFile == null)
            return;

        builder.ReportProgress("Adding additional header resources to BuildAssembler configuration file");

        var config = XDocument.Load(configFile);

        // The VS2013 presentation style contains an instance of the component already but we'll insert a
        // second copy with the user-defined resources.  This way, we don't have to search for it specifically
        // since no other presentation styles use it.  The component is inserted before the previous node which
        // will be a Shared Content Component instance.  We don't search for it since it can appear elsewhere.
        // The Resolve Art Links Component is the better choice for determining placement.
        var resolveArtLinkComponents = config.Descendants("component").Where(
            c => c.Attribute("id").Value == "Resolve Art Links Component");

        foreach(var c in resolveArtLinkComponents)
        {
            c.PreviousNode.AddBeforeSelf(new XElement("component",
                new XAttribute("id", "Additional Header Resources Component"),
                configuration.Elements().Select(e => new XElement(e))));
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
