// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{

    public class ResolveArtLinksComponent : BuildComponent
    {

        public ResolveArtLinksComponent(BuildAssembler assembler, XPathNavigator configuration)
            : base(assembler, configuration)
        {

            XPathNodeIterator targets_nodes = configuration.Select("targets");
            foreach(XPathNavigator targets_node in targets_nodes)
            {

                string input = targets_node.GetAttribute("input", String.Empty);
                if(String.IsNullOrEmpty(input))
                    WriteMessage(MessageLevel.Error, "Each targets element must have an input attribute specifying a directory containing art files.");
                input = Environment.ExpandEnvironmentVariables(input);
                if(!Directory.Exists(input))
                    WriteMessage(MessageLevel.Error, String.Format("The art input directory '{0}' does not exist.", input));

                string baseOutputPath = targets_node.GetAttribute("baseOutput", String.Empty);
                if(!String.IsNullOrEmpty(baseOutputPath))
                {
                    baseOutputPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(baseOutputPath));
                }

                string outputPath_value = targets_node.GetAttribute("outputPath", string.Empty);
                if(string.IsNullOrEmpty(outputPath_value))
                    WriteMessage(MessageLevel.Error, "Each targets element must have an output attribute specifying a directory in which to place referenced art files.");
                XPathExpression output_XPath = XPathExpression.Compile(outputPath_value);

                string linkValue = targets_node.GetAttribute("link", String.Empty);
                if(String.IsNullOrEmpty(linkValue))
                    linkValue = "../art";
                //linkValue = Environment.ExpandEnvironmentVariables(linkValue);

                string map = targets_node.GetAttribute("map", String.Empty);
                if(String.IsNullOrEmpty(map))
                    WriteMessage(MessageLevel.Error, "Each targets element must have a map attribute specifying a file that maps art ids to files in the input directory.");
                map = Environment.ExpandEnvironmentVariables(map);
                if(!File.Exists(map))
                    WriteMessage(MessageLevel.Error, String.Format("The art map file '{0}' does not exist.", map));

                string format = targets_node.GetAttribute("format", String.Empty);
                XPathExpression format_xpath = String.IsNullOrEmpty(format) ? null : XPathExpression.Compile(format);

                string relative_to = targets_node.GetAttribute("relative-to", String.Empty);
                XPathExpression relative_to_xpath = String.IsNullOrEmpty(relative_to) ? null : XPathExpression.Compile(relative_to);

                AddTargets(map, input, baseOutputPath, output_XPath, linkValue, format_xpath, relative_to_xpath);

            }

            WriteMessage(MessageLevel.Info, String.Format("Indexed {0} art targets.", targets.Count));

        }

        private void AddTargets(string map, string input, string baseOutputPath, XPathExpression outputXPath, string link, XPathExpression formatXPath, XPathExpression relativeToXPath)
        {

            XPathDocument document = new XPathDocument(map);

            XPathNodeIterator items = document.CreateNavigator().Select("/*/item");
            foreach(XPathNavigator item in items)
            {

                string id = (string)item.Evaluate(artIdExpression);
                string file = (string)item.Evaluate(artFileExpression);
                string text = (string)item.Evaluate(artTextExpression);

                id = id.ToLower();
                string name = Path.GetFileName(file);

                ArtTarget target = new ArtTarget();
                target.Id = id;
                target.InputPath = Path.Combine(input, file);
                target.baseOutputPath = baseOutputPath;
                target.OutputXPath = outputXPath;

                if(string.IsNullOrEmpty(name))
                    target.LinkPath = link;
                else
                    target.LinkPath = string.Format("{0}/{1}", link, name);

                target.Text = text;
                target.Name = name;
                target.FormatXPath = formatXPath;
                target.RelativeToXPath = relativeToXPath;

                targets[id] = target;
                // targets.Add(id, target);
            }

        }

        private XPathExpression artIdExpression = XPathExpression.Compile("string(@id)");
        private XPathExpression artFileExpression = XPathExpression.Compile("string(image/@file)");
        private XPathExpression artTextExpression = XPathExpression.Compile("string(image/altText)");

        private Dictionary<string, ArtTarget> targets = new Dictionary<string, ArtTarget>();

        public override void Apply(XmlDocument document, string id)
        {

            XPathNodeIterator artLinkIterator = document.CreateNavigator().Select(artLinkExpression);
            XPathNavigator[] artLinks = BuildComponentUtilities.ConvertNodeIteratorToArray(artLinkIterator);
            foreach(XPathNavigator artLink in artLinks)
            {

                string name = artLink.GetAttribute("target", String.Empty).ToLower();

                if(targets.ContainsKey(name))
                {
                    ArtTarget target = targets[name];

                    // evaluate the path
                    string path = document.CreateNavigator().Evaluate(target.OutputXPath).ToString();

                    if(target.baseOutputPath != null)
                        path = Path.Combine(target.baseOutputPath, path);
                    string outputPath = Path.Combine(path, target.Name);

                    string targetDirectory = Path.GetDirectoryName(outputPath);

                    if(!Directory.Exists(targetDirectory))
                        Directory.CreateDirectory(targetDirectory);

                    if(File.Exists(target.InputPath))
                    {

                        if(File.Exists(outputPath))
                        {
                            File.SetAttributes(outputPath, FileAttributes.Normal);
                        }

                        File.Copy(target.InputPath, outputPath, true);
                    }
                    else
                    {
                        base.WriteMessage(id, MessageLevel.Warn, "The file '{0}' for the art target '{1}' was not found.", target.InputPath, name);
                    }

                    // Get the relative art path for HXF generation.
                    int index = target.LinkPath.IndexOf('/');
                    string artPath = target.LinkPath.Substring(index + 1, target.LinkPath.Length - (index + 1));

                    FileCreatedEventArgs fe = new FileCreatedEventArgs(artPath, Path.GetDirectoryName(path));
                    OnComponentEvent(fe);

                    XmlWriter writer = artLink.InsertAfter();

                    writer.WriteStartElement("img");
                    if(!String.IsNullOrEmpty(target.Text))
                        writer.WriteAttributeString("alt", target.Text);

                    if(target.FormatXPath == null)
                    {
                        writer.WriteAttributeString("src", target.LinkPath);
                    }
                    else
                    {
                        // WebDocs way, which uses the 'format' xpath expression to calculate the target path
                        // and then makes it relative to the current page if the 'relative-to' attribute is
                        // used.
                        string src = BuildComponentUtilities.EvalXPathExpr(document, target.FormatXPath, "key", Path.GetFileName(outputPath));
                        if(target.RelativeToXPath != null)
                            src = BuildComponentUtilities.GetRelativePath(src, BuildComponentUtilities.EvalXPathExpr(document, target.RelativeToXPath, "key", id));
                        writer.WriteAttributeString("src", src);
                    }

                    writer.WriteEndElement();

                    writer.Close();

                    artLink.DeleteSelf();

                }
                else
                {
                    base.WriteMessage(id, MessageLevel.Warn, "Unknown art target '{0}'", name);
                }

            }

        }

        private static XPathExpression artLinkExpression = XPathExpression.Compile("//artLink");

    }

    internal class ArtTarget
    {

        public string Id;

        public string InputPath;

        public string baseOutputPath;

        public XPathExpression OutputXPath;

        public string LinkPath;

        public string Text;

        public string Name;

        public XPathExpression FormatXPath;

        public XPathExpression RelativeToXPath;

    }

}