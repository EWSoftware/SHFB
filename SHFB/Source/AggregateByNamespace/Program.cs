using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.CommandLine;

namespace AggregateByNamespace
{
    public static class Program
    {
        // Fields
        private static XPathExpression namespaceIdExpression = XPathExpression.Compile("string(containers/namespace/@api)");
        private static XPathExpression typeIdExpression = XPathExpression.Compile("string(@id)");
        private static XPathExpression typesExpression = XPathExpression.Compile("/*/apis/api[apidata/@group='type']");

        // Methods
        public static int Main(string[] args)
        {
            XmlWriter writer;
            string key;

            ConsoleApplication.WriteBanner();

            OptionCollection options = new OptionCollection {
                new SwitchOption("?", "Show this help page."),
                new StringOption("out", "Specify an output file. If unspecified, output goes to the " +
                    "console.", "outputFilePath"),
                new StringOption("name", "Specify a name for the project node. If a name is specified, a " +
                    "root topic is created.", "projectName")
            };

            ParseArgumentsResult result = options.ParseArguments(args);

            if(result.Options["?"].IsPresent)
            {
                Console.WriteLine("AggregateByNamespace reflection_files [options]");
                options.WriteOptionSummary(Console.Out);
                return 0;
            }

            if(!result.Success)
            {
                result.WriteParseErrors(Console.Out);
                return 1;
            }

            if(result.UnusedArguments.Count == 0)
            {
                Console.WriteLine("Specify one or more reflection files for processing.");
                return 1;
            }

            Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();

            int num = 0;

            foreach(string reflectionFilename in result.UnusedArguments)
            {
                string fullPath = Path.GetFullPath(Path.GetDirectoryName(reflectionFilename));

                foreach(string str2 in Directory.EnumerateFiles(fullPath, Path.GetFileName(reflectionFilename)))
                {
                    try
                    {
                        XPathDocument document = new XPathDocument(str2);
                        num++;

                        XPathNodeIterator iterator = document.CreateNavigator().Select(typesExpression);

                        foreach(XPathNavigator navigator in iterator)
                        {
                            List<string> list;
                            string item = (string)navigator.Evaluate(typeIdExpression);

                            if(item.Length == 0)
                                Console.WriteLine("Moo");

                            key = (string)navigator.Evaluate(namespaceIdExpression);

                            if(key.Length == 0)
                                Console.WriteLine("foo");

                            if(!dictionary.TryGetValue(key, out list))
                            {
                                list = new List<string>();
                                dictionary.Add(key, list);
                            }

                            list.Add(item);
                        }
                    }
                    catch(IOException ioEx)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                            "An error occured while reading the input file '{0}'. The error message follows: {1}",
                            str2, ioEx.Message));
                        return 1;
                    }
                    catch(XmlException xmlEx)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                            "The input file '{0}' is not well-formed. The error message follows: {1}", str2,
                            xmlEx.Message));
                        return 1;
                    }
                }
            }

            ConsoleApplication.WriteMessage(LogLevel.Info, String.Format(CultureInfo.CurrentCulture,
                "Parsed {0} files", num));

            XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

            int num2 = 0, num3 = 0;

            if(result.Options["out"].IsPresent)
            {
                string outputFileName = (string)result.Options["out"].Value;

                try
                {
                    writer = XmlWriter.Create(outputFileName, settings);
                }
                catch(IOException ioEx)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                        "An error occured while trying to create the output file. The error message is: {0}",
                        ioEx.Message));
                    return 1;
                }
            }
            else
                writer = XmlWriter.Create(Console.Out, settings);

            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("reflection");
                writer.WriteStartElement("apis");

                foreach(KeyValuePair<string, List<string>> pair in dictionary)
                {
                    num2++;
                    key = pair.Key;
                    List<string> list2 = pair.Value;
                    string str6 = key.Substring(2);
                    writer.WriteStartElement("api");
                    writer.WriteAttributeString("id", key);
                    writer.WriteStartElement("apidata");
                    writer.WriteAttributeString("group", "namespace");
                    writer.WriteAttributeString("name", str6);
                    writer.WriteEndElement();
                    writer.WriteStartElement("elements");

                    foreach(string str3 in list2)
                    {
                        num3++;
                        writer.WriteStartElement("element");
                        writer.WriteAttributeString("api", str3);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                if(result.Options["name"].IsPresent)
                {
                    string str7 = "R:" + ((string)result.Options["name"].Value);

                    writer.WriteStartElement("api");
                    writer.WriteAttributeString("id", str7);
                    writer.WriteStartElement("apidata");
                    writer.WriteAttributeString("group", "root");
                    writer.WriteAttributeString("pseudo", "true");
                    writer.WriteEndElement();
                    writer.WriteStartElement("elements");

                    foreach(string str4 in dictionary.Keys)
                    {
                        writer.WriteStartElement("element");
                        writer.WriteAttributeString("api", str4);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            finally
            {
                writer.Close();
            }

            ConsoleApplication.WriteMessage(LogLevel.Info, String.Format(CultureInfo.CurrentCulture,
                "Wrote {0} namespaces containing {1} types.", num2, num3));

            return 0;
        }
    }
}
