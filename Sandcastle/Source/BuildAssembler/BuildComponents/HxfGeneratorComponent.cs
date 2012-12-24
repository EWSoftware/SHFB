// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;

namespace Microsoft.Ddue.Tools
{
    public class FileCreatedEventArgs : EventArgs
    {
        private string filePath;
        private string hxfPath;

        public FileCreatedEventArgs(string filePath, string hxfPath)
        {
            this.filePath = filePath;
            this.hxfPath = hxfPath;
        }

        public string FilePath
        {
            get { return filePath; }
        }

        public string HxfPath
        {
            get { return hxfPath; }
        }

    }

    public class HxfGeneratorComponent : BuildComponent
    {
        public HxfGeneratorComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {

            // get configuration data
            inputValue = configuration.GetAttribute("input", String.Empty);

            if(!String.IsNullOrEmpty(inputValue))
                inputValue = Environment.ExpandEnvironmentVariables(inputValue);

            outputValue = configuration.GetAttribute("output", String.Empty);

            if(!String.IsNullOrEmpty(outputValue))
                outputValue = Environment.ExpandEnvironmentVariables(outputValue);

            // subscribe to component events
            assembler.ComponentEvent += new EventHandler(FileCreatedHandler);
        }

        private string inputValue;

        private string outputValue;

        private XmlWriter writer;

        // TODO: This can probably go away.  There's only ever one output file right?
        // Path names are compared case insensitively
        private Dictionary<string, XmlWriter> writers = new Dictionary<string, XmlWriter>(StringComparer.OrdinalIgnoreCase);

        private void FileCreatedHandler(Object o, EventArgs e)
        {
            FileCreatedEventArgs fe = e as FileCreatedEventArgs;

            if(fe == null)
                return;

            string path = Path.Combine(fe.HxfPath, outputValue);

            XmlWriter tempWriter;

            if(!writers.TryGetValue(path, out tempWriter))
            {
                if(writer != null)
                {
                    writer.WriteEndDocument();
                    writer.Close();
                }

                WriteFile(path);
            }

            WriteFileElement(fe.FilePath);
        }

        private void WriteFileElement(string url)
        {
            writer.WriteStartElement("File");
            writer.WriteAttributeString("Url", url);
            writer.WriteEndElement();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                writer.WriteEndDocument();
                writer.Close();
            }

            base.Dispose(disposing);
        }

        public void WriteFile(string path)
        {
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;
            writer = XmlWriter.Create(path);
            writer.WriteStartDocument();
            writer.WriteStartElement("HelpFileList");
            writer.WriteAttributeString("DTDVersion", "1.0");

            // use the input to seed the output
            if(!String.IsNullOrEmpty(inputValue))
            {
                try
                {
                    TextReader reader = File.OpenText(inputValue);

                    try
                    {
                        while(true)
                        {
                            string line = reader.ReadLine();

                            if(line == null)
                                break;

                            WriteFileElement(line);
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                catch(IOException ex)
                {
                    WriteMessage(MessageLevel.Error, "An access error occured while attempting to copy the " +
                        "input HxF data. The error message is:", ex.Message);
                }
            }

            writers.Add(path, writer);
        }

        // don't do anything for individual files
        public override void Apply(XmlDocument document, string key)
        {
        }
    }
}
