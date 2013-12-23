// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.CommandLine;

namespace MergeXml
{
    public static class Program
    {
        static int Main(string[] args)
        {
            ConsoleApplication.WriteBanner();

            // get and validate args
            OptionCollection programOptions = new OptionCollection();
            programOptions.Add(new SwitchOption("?", "Show this help page."));
            programOptions.Add(new StringOption("out", "Path to the file that the input files should be " +
                "merged in to. Required.") { RequiredMessage = "An output file path is required" });
            programOptions.Add(new StringOption("position", "The name of the element or elements to which the " +
                "input elements will be appended. Required.") { RequiredMessage =
                "A position value naming the element or elements to include is required" });
            programOptions.Add(new StringOption("include", @"An XPath expression indicating which elements " +
                "from the source files should be introduced in to the output file. The default is '/'"));

            ParseArgumentsResult options = programOptions.ParseArguments(args);

            if(options.Options["?"].IsPresent || !options.Options["out"].IsPresent || !options.Options["position"].IsPresent)
            {
                programOptions.WriteOptionSummary(Console.Error);
                Console.WriteLine();
                Console.WriteLine("file1 file2 ...");
                Console.WriteLine("The input files to operate on.");
                return 0;
            }

            // ensure output file exists
            string outputPath = options.Options["out"].Value.ToString();

            if(!File.Exists(outputPath))
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "The specified output file, which the input files are to be merged in to, doesn't exist.");
                return 1;
            }

            // ensure a postition element name was passed
            if(String.IsNullOrEmpty(options.Options["position"].Value.ToString()))
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "No position element name was provided.");
                return 1;
            }

            string positionName = options.Options["position"].Value.ToString();

            // validate xpaths ("include" switch)
            string xpath;
            if(options.Options["include"].IsPresent)
                xpath = options.Options["include"].Value.ToString();
            else
                xpath = @"/";
            XPathExpression includeExpression;
            try
            {
                includeExpression = XPathExpression.Compile(xpath);
            }
            catch(XPathException)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "The xpath expression provided by the include switch, '" + xpath + "', is invalid.");
                return 1;
            }

            // get list of input files to operate on
            if(options.UnusedArguments.Count == 0)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "No input files were provided.");
                return 1;
            }
            string[] inputFiles = new string[options.UnusedArguments.Count];
            options.UnusedArguments.CopyTo(inputFiles, 0);

            // ensure all input files exist
            foreach(string path in inputFiles)
            {
                if(!File.Exists(path))
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "Specified input file '" + path + "' doesn't exist.");
                    return 1;
                }
            }


            // open the output file and move to the position
            XmlWriterSettings outputSettings = new XmlWriterSettings();
            outputSettings.Indent = true;
            outputSettings.Encoding = Encoding.UTF8;
            using(XmlWriter output = XmlWriter.Create(outputPath + ".tmp", outputSettings))
            {
                // start printing output doc string until the selected node is matched
                using(XmlReader source = XmlReader.Create(outputPath))
                {
                    while(!source.EOF)
                    {
                        source.Read();

                        switch(source.NodeType)
                        {
                            case XmlNodeType.Element:
                                output.WriteStartElement(source.Prefix, source.LocalName, source.NamespaceURI);
                                output.WriteAttributes(source, true);
                                if(source.IsEmptyElement)
                                {
                                    output.WriteEndElement();
                                }
                                if(String.Equals(source.Name, positionName, StringComparison.OrdinalIgnoreCase))
                                {
                                    // start introducing the elements from the input files
                                    foreach(string path in inputFiles)
                                    {
                                        XPathDocument inputDoc = new XPathDocument(path);
                                        XPathNavigator inputNav = inputDoc.CreateNavigator();
                                        XPathNodeIterator inputNodesIterator = inputNav.Select(includeExpression);
                                        while(inputNodesIterator.MoveNext())
                                        {
                                            output.WriteNode(inputNodesIterator.Current, true);
                                        }
                                    }
                                }
                                break;
                            case XmlNodeType.Text:
                                output.WriteString(source.Value);
                                break;
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                output.WriteWhitespace(source.Value);
                                break;
                            case XmlNodeType.CDATA:
                                output.WriteCData(source.Value);
                                break;
                            case XmlNodeType.EntityReference:
                                output.WriteEntityRef(source.Name);
                                break;
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.ProcessingInstruction:
                                output.WriteProcessingInstruction(source.Name, source.Value);
                                break;
                            case XmlNodeType.DocumentType:
                                output.WriteDocType(source.Name, source.GetAttribute("PUBLIC"), source.GetAttribute("SYSTEM"), source.Value);
                                break;
                            case XmlNodeType.Comment:
                                output.WriteComment(source.Value);
                                break;
                            case XmlNodeType.EndElement:
                                output.WriteFullEndElement();
                                break;
                        }
                    }
                }
                output.WriteEndDocument();
                output.Close();
            }

            File.Delete(outputPath);
            File.Move(outputPath + ".tmp", outputPath);

            return 0; // pau
        }
    }
}