// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/24/2012 - Updated to use the new FileCreatedEventArgs.  Moved ArtTarget out into its own file.  Inlined
// the add targets method.  Updated it to only copy the image if not already there as it seems redundant to
// copy it every time the same ID is encountered.  As with the SharedContentComponent, this one doesn't load
// enough info to warrant trying to share the common data across all instances.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.Targets;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This component is used to resolve links to media files (i.e images)
    /// </summary>
    public class ResolveArtLinksComponent : BuildComponentCore
    {
        #region Private data members
        //=====================================================================

        private static XPathExpression artLinkExpression = XPathExpression.Compile("//artLink");

        // IDs are compared case insensitively
        private Dictionary<string, ArtTarget> targets = new Dictionary<string, ArtTarget>(
            StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public ResolveArtLinksComponent(BuildAssemblerCore assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            XPathExpression artIdExpression = XPathExpression.Compile("string(@id)"),
                artFileExpression = XPathExpression.Compile("string(image/@file)"),
                artTextExpression = XPathExpression.Compile("string(image/altText)");

            XPathNodeIterator targetsNodes = configuration.Select("targets");

            foreach(XPathNavigator targetsNode in targetsNodes)
            {
                // Get the configuration values for this target
                string inputPath = targetsNode.GetAttribute("input", String.Empty);

                if(String.IsNullOrEmpty(inputPath))
                    base.WriteMessage(MessageLevel.Error, "Each targets element must have an input attribute " +
                        "specifying a directory containing art files.");

                inputPath = Environment.ExpandEnvironmentVariables(inputPath);

                if(!Directory.Exists(inputPath))
                    base.WriteMessage(MessageLevel.Error, "The art input directory '{0}' does not exist.",
                        inputPath);

                string baseOutputPath = targetsNode.GetAttribute("baseOutput", String.Empty);

                if(!String.IsNullOrEmpty(baseOutputPath))
                    baseOutputPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(baseOutputPath));

                string outputPathValue = targetsNode.GetAttribute("outputPath", string.Empty);

                if(string.IsNullOrEmpty(outputPathValue))
                    base.WriteMessage(MessageLevel.Error, "Each targets element must have an output attribute " +
                        "specifying a directory in which to place referenced art files.");

                XPathExpression outputXPath = XPathExpression.Compile(outputPathValue);

                string linkPath = targetsNode.GetAttribute("link", String.Empty);

                if(String.IsNullOrEmpty(linkPath))
                    linkPath = "../art";

                string map = targetsNode.GetAttribute("map", String.Empty);

                if(String.IsNullOrEmpty(map))
                    base.WriteMessage(MessageLevel.Error, "Each targets element must have a map attribute " +
                        "specifying a file that maps art IDs to files in the input directory.");

                map = Environment.ExpandEnvironmentVariables(map);

                if(!File.Exists(map))
                    base.WriteMessage(MessageLevel.Error, "The art map file '{0}' does not exist.", map);

                string format = targetsNode.GetAttribute("format", String.Empty);

                XPathExpression formatXPath = String.IsNullOrEmpty(format) ? null :
                    XPathExpression.Compile(format);

                string relativeTo = targetsNode.GetAttribute("relative-to", String.Empty);

                XPathExpression relativeToXPath = String.IsNullOrEmpty(relativeTo) ? null :
                    XPathExpression.Compile(relativeTo);

                // Load the content of the media map file
                XPathDocument mediaMap = new XPathDocument(map);
                XPathNodeIterator items = mediaMap.CreateNavigator().Select("/*/item");

                foreach(XPathNavigator item in items)
                {
                    string id = (string)item.Evaluate(artIdExpression);
                    string file = (string)item.Evaluate(artFileExpression);
                    string text = (string)item.Evaluate(artTextExpression);
                    string name = Path.GetFileName(file);

                    targets[id] = new ArtTarget
                    {
                        InputPath = Path.Combine(inputPath, file),
                        BaseOutputPath = baseOutputPath,
                        OutputXPath = outputXPath,
                        LinkPath = String.IsNullOrEmpty(name) ? linkPath : String.Concat(linkPath, "/", name),
                        Text = text,
                        Name = name,
                        FormatXPath = formatXPath,
                        RelativeToXPath = relativeToXPath
                    };
                }
            }

            base.WriteMessage(MessageLevel.Info, "Indexed {0} art targets.", targets.Count);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            ArtTarget target;

            foreach(XPathNavigator artLink in document.CreateNavigator().Select(artLinkExpression).ToArray())
            {
                string name = artLink.GetAttribute("target", String.Empty);

                if(targets.TryGetValue(name, out target))
                {
                    // Evaluate the path
                    string path = document.CreateNavigator().Evaluate(target.OutputXPath).ToString();

                    if(target.BaseOutputPath != null)
                        path = Path.Combine(target.BaseOutputPath, path);

                    string outputPath = Path.Combine(path, target.Name);

                    string targetDirectory = Path.GetDirectoryName(outputPath);

                    if(!Directory.Exists(targetDirectory))
                        Directory.CreateDirectory(targetDirectory);

                    if(File.Exists(target.InputPath))
                    {
                        if(!File.Exists(outputPath))
                        {
                            File.Copy(target.InputPath, outputPath, true);
                            File.SetAttributes(outputPath, FileAttributes.Normal);
                        }
                    }
                    else
                        base.WriteMessage(key, MessageLevel.Warn, "The file '{0}' for the art target '{1}' " +
                            "was not found.", target.InputPath, name);

                    // Raise an event to indicate that a file was created
                    OnComponentEvent(new FileCreatedEventArgs(outputPath, true));

                    XmlWriter writer = artLink.InsertAfter();

                    writer.WriteStartElement("img");

                    if(!String.IsNullOrEmpty(target.Text))
                        writer.WriteAttributeString("alt", target.Text);

                    if(target.FormatXPath == null)
                        writer.WriteAttributeString("src", target.LinkPath);
                    else
                    {
                        // Microsoft legacy code.  Probably not needed, but we'll retain it.

                        // WebDocs way, which uses the 'format' xpath expression to calculate the target path and
                        // then makes it relative to the current page if the 'relative-to' attribute is used.
                        string src = document.EvalXPathExpr(target.FormatXPath, "key",
                            Path.GetFileName(outputPath));

                        if(target.RelativeToXPath != null)
                            src = src.GetRelativePath(document.EvalXPathExpr(target.RelativeToXPath, "key", key));

                        writer.WriteAttributeString("src", src);
                    }

                    writer.WriteEndElement();
                    writer.Close();

                    artLink.DeleteSelf();
                }
                else
                    base.WriteMessage(key, MessageLevel.Warn, "Unknown art target '{0}'", name);
            }
        }
        #endregion
    }
}
