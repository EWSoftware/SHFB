// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;


namespace Microsoft.Ddue.Tools {

    /// <summary>
    /// The LiveExampleComponent replaces ddue:CodeReference elements with links to runnable code samples,
    /// if the type is "run" or "view". All other kinds of code samples are passed through to the 
    /// standard example component, except that the type prefix is removed. A Parsnip "approval" file can 
    /// be used to omit code samples that did not pass validation, or to replace broken samples with a message 
    /// to that effect.
    /// </summary>
    public class LiveExampleComponent : BuildComponent {

        readonly string wdxNamespace = "http://temp.uri/wdx";
        readonly string wdxPrefix = "wdx";

        XPathExpression selector;
        XmlNamespaceManager context;

        bool omitBadExamples;
        //bool runBadExamples;

        Dictionary<string, SampleInfo> sampleInfoTable;

        public LiveExampleComponent(BuildAssembler assembler, XPathNavigator configuration)
            : base(assembler, configuration) {

            XPathNavigator parsnip_node = configuration.SelectSingleNode("parsnip");
            string approvedFile = null;
            if (parsnip_node != null) {
                approvedFile = parsnip_node.GetAttribute("approved-file", String.Empty);

                string omitBadExamplesValue = parsnip_node.GetAttribute("omit-bad-examples", String.Empty);
                if (!string.IsNullOrEmpty(omitBadExamplesValue))
                    omitBadExamples = Boolean.Parse(omitBadExamplesValue);

                //string runBadExamplesValue = parsnip_node.GetAttribute("run-bad-examples", String.Empty);
                //if (!string.IsNullOrEmpty(runBadExamplesValue))
                //    runBadExamples = Boolean.Parse(runBadExamplesValue);
            }

            if (string.IsNullOrEmpty(approvedFile))
                WriteMessage(MessageLevel.Warn, "No approved samples file specified; all available samples will be included.");
            else
                LoadApprovedFile(approvedFile);

            context = new CustomContext();
            context.AddNamespace("ddue", "http://ddue.schemas.microsoft.com/authoring/2003/5");

            selector = XPathExpression.Compile("//ddue:codeReference");
            selector.SetContext(context);
        }

        static XPathNavigator[] ConvertIteratorToArray(XPathNodeIterator iterator) {
            XPathNavigator[] result = new XPathNavigator[iterator.Count];
            for (int i = 0; i < result.Length; i++) {
                iterator.MoveNext();
                result[i] = iterator.Current.Clone();
            }
            return (result);
        }

        public override void Apply(XmlDocument document, string key) {
            XPathNodeIterator nodesIterator = document.CreateNavigator().Select(selector);
            XPathNavigator[] nodes = ConvertIteratorToArray(nodesIterator);

            foreach (XPathNavigator node in nodes) {
                CodeReference cref = new CodeReference(node.Value);

                SampleInfo si = null;
                if (sampleInfoTable != null && cref.ExampleName != null)
                    sampleInfoTable.TryGetValue(cref.ExampleName, out si);

                WriteMessage(MessageLevel.Info, string.Format("*** codeReference={0}; approved={1}; type={2}",
                    node.Value, (si == null) ? false : si.IsApproved("CS"), cref.Type));


                switch (cref.Type) {
                    case CodeReferenceType.Msdn:
                        // TODO: remove "msdn:" from code reference and let ExampleComponent handle the snippet.
                        // We'll either pass this through to the regular ExampleComponent or delete the node.
                        WriteMessage(MessageLevel.Warn, "MSDN-only links not implemented yet.");
                        break;

                    case CodeReferenceType.Run:
                    case CodeReferenceType.View:
                        if (si != null || !omitBadExamples) {
                            WriteMessage(MessageLevel.Info, string.Format("+ LiveCode: Kind={0}, SampleName={1}", cref.Type.ToString(), cref.ExamplePath));
                            XmlWriter writer = node.InsertAfter();
                            writer.WriteStartElement(wdxPrefix, "LiveCode", wdxNamespace);
                            writer.WriteAttributeString("Kind", cref.Type.ToString());
                            writer.WriteAttributeString("SampleName", cref.ExamplePath);
                            writer.WriteAttributeString("runat", "server");
                            writer.WriteEndElement();
                            writer.Close();
                            node.DeleteSelf();
                        }
                        break;

                    case CodeReferenceType.Snippet:
                        // Ignore; let ExampleComponent handle the snippet.
                        break;

                    default:
                        WriteMessage(MessageLevel.Warn, string.Format("Invalid code example reference ignored: '{0}'", node.Value));
                        break;
                }
            }
        }

        /// <summary>
        /// LoadApprovedFile reads the Parsnip approval file into a memory structure for easy lookup. We're assuming that the
        /// content of this file is well formed and valid. Semantic errors will be silently ignored. Only samples with
        /// approved units will be added to the lookup table.
        /// </summary>
        void LoadApprovedFile(string pathName) {
            WriteMessage(MessageLevel.Info, string.Format("Loading Parsnip 'Approved' file: {0}", pathName));
            sampleInfoTable = new Dictionary<string, SampleInfo>();
            using (XmlReader reader = XmlReader.Create(pathName)) {
                SampleInfo si = null;
                string sample_name = null;
                while (reader.Read()) {
                    switch (reader.NodeType) {
                        case XmlNodeType.Element:
                            if (reader.Name == "Sample") {
                                sample_name = reader.GetAttribute("name");
                                if (!string.IsNullOrEmpty(sample_name))
                                    si = new SampleInfo(sample_name);
                            }
                            else if (si != null && reader.Name == "Unit") {
                                if (reader.GetAttribute("include") == "true")
                                    si.AddApprovedUnit(reader.GetAttribute("name"));
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name == "Sample") {
                                if (si != null) {
                                    try {
                                        if (si.ApprovedUnitsCount > 0)
                                            sampleInfoTable.Add(si.Name, si);
                                    }
                                    catch (Exception x) {
                                        WriteMessage(MessageLevel.Warn, string.Format("Sample {0} cannot be loaded {1}", si.Name, x.Message));
                                    }
                                }
                                si = null;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    internal class SampleInfo {
        string _name;
        internal string Name {
            get { return _name; }
        }

        List<string> _approvedUnits;

        internal SampleInfo(string name) {
            _name = name;
        }

        internal void AddApprovedUnit(string unit_name) {
            if (_approvedUnits == null)
                _approvedUnits = new List<string>();
            _approvedUnits.Add(unit_name);
        }

        internal bool IsApproved(string unit_name) {
            return (_approvedUnits == null) ? false : _approvedUnits.Contains(unit_name);
        }

        internal int ApprovedUnitsCount {
            get { return (_approvedUnits == null) ? 0 : _approvedUnits.Count; }
        }
    }
}