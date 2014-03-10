//===============================================================================================================
// System  : Sandcastle Tools Standard Presentation Styles
// File    : OpenXml.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/15/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the presentation style definition for the Open XML presentation style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/15/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;

namespace OpenXML
{
    /// <summary>
    /// This defines a presentation style used to generate Open XML documents
    /// </summary>
    [PresentationStyleExport("OpenXML", "Open XML Document", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This generates an Open XML document containing the " +
        "help content.\r\nOpen XML documents can be converted to other formats such as PDF files.")]
    public sealed class OpenXMLPresentationStyle : PresentationStyleSettings
    {
        /// <inheritdoc />
        public override string Location
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OpenXMLPresentationStyle()
        {
            this.BasePath = "OpenXML";

            this.SupportedFormats = HelpFileFormats.OpenXml;

            this.SupportsNamespaceGrouping = true;

            // If relative, these paths are relative to the base path
            this.ResourceItemsPath = "Content";
            this.ToolResourceItemsPath = "SHFBContent";

            this.DocumentModelTransformation = new TransformationFile(
                @"%SHFBROOT%\ProductionTransforms\ApplyVSDocModel.xsl", new Dictionary<string, string>
                {
                    { "IncludeAllMembersTopic", "false" },
                    { "IncludeInheritedOverloadTopics", "false" },
                    { "project", "{@ProjectNodeIDOptional}" }
                });

            this.IntermediateTocTransformation = new TransformationFile(
                @"%SHFBROOT%\ProductionTransforms\CreateVSToc.xsl");

            this.ConceptualBuildConfiguration = @"Configuration\SHFBConceptual.config";
            this.ReferenceBuildConfiguration = @"Configuration\SHFBReference.config";

            this.ContentFiles.Add(new ContentFiles(this.SupportedFormats, null, @"DocumentParts\*.*",
                String.Empty, new[] { ".xml" } ));

            this.TransformComponentArguments.Add(new TransformComponentArgument("maxVersionParts", false, true,
                null, "The maximum number of assembly version parts to show in API member topics.  Set to 2, " +
                "3, or 4 to limit it to 2, 3, or 4 parts or leave it blank for all parts including the " +
                "assembly file version value if specified."));
        }
    }
}
