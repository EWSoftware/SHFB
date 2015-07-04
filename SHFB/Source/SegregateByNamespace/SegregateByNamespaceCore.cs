// Change history:
// 12/15/2013 - EFW - Added check for invalid filename characters in the namespace filenames
// 07/03/2015 - EFW - Added MSBuild task support

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.CommandLine;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This takes a reflection data file and splits it into multiple reflection data files, each containing the
    /// types from a specific namespace.
    /// </summary>
    public static class SegregateByNamespaceCore
    {
        // Fields
        private static XPathExpression apiExpression = XPathExpression.Compile("/*/apis/api");
        private static XPathExpression apiNamespaceExpression = XPathExpression.Compile("string(containers/namespace/@api)");
        private static XPathExpression assemblyNameExpression = XPathExpression.Compile("string(containers/library/@assembly)");
        private static XPathExpression namespaceIdExpression = XPathExpression.Compile("string(@id)");

        /// <summary>
        /// This property is used to cancel the operation from the MSBuild task
        /// </summary>
        public static bool Canceled { get; set; }

        /// <summary>
        /// Main program entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Zero on success or non-zero on failure</returns>
        public static int Main(string[] args)
        {
            XPathDocument document;

            ConsoleApplication.WriteBanner();

            OptionCollection options = new OptionCollection {
                new SwitchOption("?", "Show this help page."),
                new StringOption("out", "Specify an output directory.  If not specified, output goes to the " +
                    "current directory.", "outputDirectory")
            };

            ParseArgumentsResult result = options.ParseArguments(args);

            if(result.Options["?"].IsPresent)
            {
                Console.WriteLine("SegregateByNamespace [options] reflectionDataFile");
                options.WriteOptionSummary(Console.Out);
                return 1;
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
            {
                outputPath = Environment.ExpandEnvironmentVariables((string)result.Options["out"].Value);

                if(!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);
            }

            try
            {
                document = new XPathDocument(uri);
            }
            catch(IOException ioEx)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                    "An error occurred while attempting to access the file '{0}'. The error message is: {1}",
                    uri, ioEx.Message));
                return 1;
            }
            catch(XmlException xmlEx)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                    "An exception processing the input file '{0}'. The error message is: {1}", uri,
                    xmlEx.Message));
                return 1;
            }

            if(!Canceled)
                WriteNamespaceFiles(document, outputPath);

            return Canceled ? 1 : 0;
        }

        private static void WriteNamespaceFiles(XPathDocument source, string outputDir)
        {
            Dictionary<string, object> dictionary3;
            XmlWriter writer;
            string current;
            char[] invalidChars = Path.GetInvalidFileNameChars();

            Dictionary<string, Dictionary<string, object>> dictionary = new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, XmlWriter> dictionary2 = new Dictionary<string, XmlWriter>();
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

            try
            {
                XPathNodeIterator iterator = source.CreateNavigator().Select(apiExpression);

                foreach(XPathNavigator navigator in iterator)
                {
                    if(Canceled)
                        return;

                    current = (string)navigator.Evaluate(apiNamespaceExpression);

                    if(!String.IsNullOrEmpty(current))
                    {
                        String key = (string)navigator.Evaluate(assemblyNameExpression);

                        if(!dictionary.TryGetValue(current, out dictionary3))
                        {
                            dictionary3 = new Dictionary<string, object>();
                            dictionary.Add(current, dictionary3);
                        }

                        if(!dictionary3.ContainsKey(key))
                            dictionary3.Add(key, null);
                    }
                }

                foreach(string currentKey in dictionary.Keys)
                {
                    if(Canceled)
                        return;

                    string filename = currentKey.Substring(2) + ".xml";

                    if(filename == ".xml")
                        filename = "default_namespace.xml";
                    else
                        if(filename.IndexOfAny(invalidChars) != -1)
                            foreach(char c in invalidChars)
                                filename = filename.Replace(c, '_');

                    if(outputDir != null)
                        filename = Path.Combine(outputDir, filename);

                    writer = XmlWriter.Create(filename, settings);

                    dictionary2.Add(currentKey, writer);

                    writer.WriteStartElement("reflection");
                    writer.WriteStartElement("assemblies");

                    dictionary3 = dictionary[currentKey];

                    foreach(string assemblyName in dictionary3.Keys)
                    {
                        if(Canceled)
                            return;

                        XPathNavigator navigator2 = source.CreateNavigator().SelectSingleNode(
                            "/*/assemblies/assembly[@name='" + assemblyName + "']");

                        if(navigator2 != null)
                            navigator2.WriteSubtree(writer);
                        else
                            ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                                "Input file does not contain node for '{0}' assembly", assemblyName));
                    }

                    writer.WriteEndElement();
                    writer.WriteStartElement("apis");
                }

                foreach(XPathNavigator navigator in iterator)
                {
                    if(Canceled)
                        return;

                    current = (string)navigator.Evaluate(apiNamespaceExpression);

                    if(string.IsNullOrEmpty(current))
                        current = (string)navigator.Evaluate(namespaceIdExpression);

                    writer = dictionary2[current];
                    navigator.WriteSubtree(writer);
                }

                foreach(XmlWriter w in dictionary2.Values)
                {
                    if(Canceled)
                        return;

                    w.WriteEndElement();
                    w.WriteEndElement();
                    w.WriteEndDocument();
                }

                ConsoleApplication.WriteMessage(LogLevel.Info, String.Format(CultureInfo.CurrentCulture,
                    "Wrote information on {0} APIs to {1} files.", iterator.Count, dictionary2.Count));
            }
            finally
            {
                foreach(XmlWriter w in dictionary2.Values)
                    w.Close();
            }
        }
    }
}
