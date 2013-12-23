using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.CommandLine;

namespace SegregateByAssembly
{
    public static class Program
    {
        // Fields
        private static XPathExpression apiExpression = XPathExpression.Compile("/*/apis/api");
        private static XPathExpression apiAssemblyExpression = XPathExpression.Compile("string(containers/library/@assembly)");
        private static XPathExpression assemblyExpression = XPathExpression.Compile("/*/assemblies/assembly");
        private static XPathExpression assemblyIdExpression = XPathExpression.Compile("string(@name)");

        // Methods
        public static int Main(string[] args)
        {
            XPathDocument document;

            ConsoleApplication.WriteBanner();

            OptionCollection options = new OptionCollection {
                new SwitchOption("?", "Show this help page."),
                new StringOption("out", "Specify an output directory. If unspecified, output goes to the " +
                    "current directory.", "outputDirectory")
            };

            ParseArgumentsResult result = options.ParseArguments(args);

            if(result.Options["?"].IsPresent)
            {
                Console.WriteLine("SegregateByAssembly [options] reflectionDataFile");
                options.WriteOptionSummary(Console.Out);
                return 0;
            }

            if(!result.Success)
            {
                result.WriteParseErrors(Console.Out);
                return 1;
            }

            if(result.UnusedArguments.Count != 1)
            {
                Console.WriteLine("Specify one reflection data file.");
                return 1;
            }

            string uri = Environment.ExpandEnvironmentVariables(result.UnusedArguments[0]);
            string outputPath = null;

            if(result.Options["out"].IsPresent)
                outputPath = Environment.ExpandEnvironmentVariables((string)result.Options["out"].Value);

            try
            {
                document = new XPathDocument(uri);
            }
            catch(IOException ioEx)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                    "An error occurred while attempting to access the file '{0}'.  The error message is: {1}",
                    uri, ioEx.Message));
                return 1;
            }
            catch(XmlException xmlEx)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                    "The input file '{0}' is not well-formed.  The error message is: {1}", uri, xmlEx.Message));
                return 1;
            }

            Dictionary<string, XmlWriter> dictionary = new Dictionary<string, XmlWriter>();

            try
            {
                XmlWriter writer;
                XPathNodeIterator iterator = document.CreateNavigator().Select(assemblyExpression);

                foreach(XPathNavigator navigator in iterator)
                {
                    string key = (string)navigator.Evaluate(assemblyIdExpression);
                    string filename = key + ".xml";

                    if(outputPath != null)
                        filename = Path.Combine(outputPath, filename);

                    try
                    {
                        writer = CreateAssemblyWriter(filename, navigator);
                        dictionary.Add(key, writer);
                    }
                    catch(IOException ioEx)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                            "An access error occurred while attempting to create the output file '{0}'.  " +
                            "The error message is: {1}", filename, ioEx.Message));
                        return 1;
                    }
                }

                string namespaceFile = "Namespaces.xml";

                if(outputPath != null)
                    namespaceFile = Path.Combine(outputPath, namespaceFile);

                try
                {
                    writer = CreateAssemblyWriter(namespaceFile, null);
                    dictionary.Add(string.Empty, writer);
                }
                catch(IOException ioEx)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                        "An access error occurred while attempting to create the output file '{0}'.  The " +
                        "error message is: {1}", namespaceFile, ioEx.Message));
                    return 1;
                }

                XPathNodeIterator apiIterator = document.CreateNavigator().Select(apiExpression);

                foreach(XPathNavigator navigator2 in apiIterator)
                {
                    string apiAssembly = (string)navigator2.Evaluate(apiAssemblyExpression);
                    writer = dictionary[apiAssembly];
                    navigator2.WriteSubtree(writer);
                }

                foreach(XmlWriter w in dictionary.Values)
                {
                    w.WriteEndElement();
                    w.WriteEndElement();
                    w.WriteEndDocument();
                }

                ConsoleApplication.WriteMessage(LogLevel.Info, String.Format(CultureInfo.CurrentCulture,
                    "Wrote information on {0} APIs to {1} files.", apiIterator.Count, dictionary.Count));
            }
            finally
            {
                foreach(XmlWriter writer in dictionary.Values)
                    writer.Close();
            }

            return 0;
        }

        private static XmlWriter CreateAssemblyWriter(string path, XPathNavigator assembly)
        {
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
            XmlWriter writer = XmlWriter.Create(path, settings);

            writer.WriteStartElement("reflection");

            if(assembly != null)
            {
                writer.WriteStartElement("assemblies");
                assembly.WriteSubtree(writer);
                writer.WriteEndElement();
            }

            writer.WriteStartElement("apis");

            return writer;
        }
    }
}
