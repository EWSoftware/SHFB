using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.CommandLine;

namespace VersionBuilder
{
    public static class Program
    {
        // Methods
        public static int Main(string[] args)
        {
            XPathDocument document;

            ConsoleApplication.WriteBanner();

            OptionCollection options = new OptionCollection {
                new SwitchOption("?", "Show this help page."),
                new StringOption("config", "Specify a configuration file.", "versionCatalog"),
                new StringOption("out", "Specify an output file containing version information.",
                    "outputFile"),
                new BooleanOption("rip", "Specify whether to rip old Apis which are not supported by " +
                    "latest versions.")
            };

            ParseArgumentsResult result = options.ParseArguments(args);

            if(result.Options["?"].IsPresent)
            {
                Console.WriteLine("VersionBuilder [options]");
                options.WriteOptionSummary(Console.Out);
                return 0;
            }

            if(!result.Success)
            {
                result.WriteParseErrors(Console.Out);
                return 1;
            }

            if(result.UnusedArguments.Count != 0)
            {
                Console.WriteLine("No non-option arguments are supported.");
                return 1;
            }

            if(!result.Options["config"].IsPresent)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "You must specify a version catalog file.");
                return 1;
            }

            bool flag = true;

            if(result.Options["rip"].IsPresent && !((bool)result.Options["rip"].Value))
                flag = false;

            string uri = (string)result.Options["config"].Value;

            try
            {
                document = new XPathDocument(uri);
            }
            catch(IOException ioEx)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                    "An error occured while accessing the version catalog file '{0}'. The error message " +
                    "is: {1}", uri, ioEx.Message));
                return 1;
            }
            catch(XmlException xmlEx)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                    "The version catalog file '{0}' is not well-formed. The error message is: {1}", uri,
                    xmlEx.Message));
                return 1;
            }
            XPathNavigator navigator = document.CreateNavigator().SelectSingleNode("versions");
            XPathExpression expr = XPathExpression.Compile("string(ancestor::versions/@name)");
            List<VersionInfo> list = new List<VersionInfo>();
            List<string> latestVersions = new List<string>();
            foreach(XPathNavigator navigator2 in document.CreateNavigator().Select("versions//version[@file]"))
            {
                string group = (string)navigator2.Evaluate(expr);
                string attribute = navigator2.GetAttribute("name", String.Empty);
                if(string.IsNullOrEmpty(attribute))
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "Every version element must have a name attribute.");
                }
                string name = navigator2.GetAttribute("file", String.Empty);
                if(String.IsNullOrEmpty(attribute))
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "Every version element must have a file attribute.");
                }
                name = Environment.ExpandEnvironmentVariables(name);
                VersionInfo item = new VersionInfo(attribute, group, name);
                list.Add(item);
            }
            string str5 = String.Empty;
            foreach(VersionInfo info2 in list)
            {
                if(info2.Group != str5)
                {
                    latestVersions.Add(info2.Name);
                    str5 = info2.Group;
                }
            }
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true
            };
            XmlWriterSettings settings2 = new XmlWriterSettings
            {
                Indent = true
            };
            Dictionary<string, List<KeyValuePair<string, string>>> versionIndex = new Dictionary<string, List<KeyValuePair<string, string>>>();
            Dictionary<string, Dictionary<string, ElementInfo>> dictionary2 = new Dictionary<string, Dictionary<string, ElementInfo>>();
            XPathExpression expression2 = XPathExpression.Compile("string(/api/@id)");
            XPathExpression expression3 = XPathExpression.Compile("string(/api/containers/library/@assembly)");
            XPathExpression expression4 = XPathExpression.Compile("/api/elements/element");
            XPathExpression expression = XPathExpression.Compile("/api/attributes/attribute[type[@api='T:System.ObsoleteAttribute']]");
            XPathExpression expression6 = XPathExpression.Compile("boolean(argument[type[@api='T:System.Boolean'] and value[.='True']])");
            foreach(VersionInfo info3 in list)
            {
                ConsoleApplication.WriteMessage(LogLevel.Info, String.Format(CultureInfo.CurrentCulture,
                    "Indexing version '{0}' using file '{1}'.", info3.Name, info3.File));
                try
                {
                    XmlReader reader = XmlReader.Create(info3.File, settings);
                    try
                    {
                        reader.MoveToContent();
                        while(reader.Read())
                        {
                            if((reader.NodeType == XmlNodeType.Element) && (reader.LocalName == "api"))
                            {
                                string key = String.Empty;
                                List<KeyValuePair<string, string>> list3 = null;
                                string str7 = String.Empty;
                                Dictionary<string, ElementInfo> dictionary3 = null;
                                XmlReader reader2 = reader.ReadSubtree();
                                XPathNavigator navigator3 = new XPathDocument(reader2).CreateNavigator();

                                key = (string)navigator3.Evaluate(expression2);
                                string text2 = (string)navigator3.Evaluate(expression3);

                                if(!versionIndex.TryGetValue(key, out list3))
                                {
                                    list3 = new List<KeyValuePair<string, string>>();
                                    versionIndex.Add(key, list3);
                                }
                                if(!dictionary2.TryGetValue(key, out dictionary3))
                                {
                                    dictionary3 = new Dictionary<string, ElementInfo>();
                                    dictionary2.Add(key, dictionary3);
                                }
                                foreach(XPathNavigator navigator4 in navigator3.Select(expression4))
                                {
                                    ElementInfo info4;
                                    string str8 = navigator4.GetAttribute("api", String.Empty);
                                    if(!dictionary3.TryGetValue(str8, out info4))
                                    {
                                        XPathNavigator elementNode = null;
                                        if((navigator4.SelectSingleNode("*") != null) || (navigator4.SelectChildren(XPathNodeType.Attribute).Count > 1))
                                        {
                                            elementNode = navigator4;
                                        }
                                        info4 = new ElementInfo(info3.Group, info3.Name, elementNode);
                                        dictionary3.Add(str8, info4);
                                        continue;
                                    }
                                    if(!info4.Versions.ContainsKey(info3.Group))
                                    {
                                        info4.Versions.Add(info3.Group, info3.Name);
                                    }
                                }
                                XPathNavigator navigator6 = navigator3.SelectSingleNode(expression);
                                if(navigator6 != null)
                                {
                                    str7 = ((bool)navigator6.Evaluate(expression6)) ? "error" : "warning";
                                }
                                list3.Add(new KeyValuePair<string, string>(info3.Name, str7));
                                str7 = String.Empty;
                                reader2.Close();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                    continue;
                }
                catch(IOException ioEx)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                        "An error occured while accessing the input file '{0}'. The error message is: {1}",
                        info3.File, ioEx.Message));
                    return 1;
                }
                catch(XmlException xmlEx)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format(CultureInfo.CurrentCulture,
                        "The input file '{0}' is not well-formed. The error message is: {1}", info3.File,
                        xmlEx.Message));
                    return 1;
                }
            }

            if(flag)
                RemoveOldApis(versionIndex, latestVersions);

            ConsoleApplication.WriteMessage(LogLevel.Info, String.Format(CultureInfo.CurrentCulture,
                "Indexed {0} entities in {1} versions.", versionIndex.Count, list.Count));

            try
            {
                XmlWriter writer;
                if(result.Options["out"].IsPresent)
                {
                    writer = XmlWriter.Create((string)result.Options["out"].Value, settings2);
                }
                else
                {
                    writer = XmlWriter.Create(Console.Out, settings2);
                }
                try
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("reflection");
                    writer.WriteStartElement("assemblies");
                    Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
                    foreach(VersionInfo info5 in list)
                    {
                        XmlReader reader3 = XmlReader.Create(info5.File, settings);
                        reader3.MoveToContent();
                        while(reader3.Read())
                        {
                            if((reader3.NodeType == XmlNodeType.Element) && (reader3.LocalName == "assembly"))
                            {
                                string str9 = reader3.GetAttribute("name");
                                if(!dictionary4.ContainsKey(str9))
                                {
                                    XmlReader reader4 = reader3.ReadSubtree();
                                    writer.WriteNode(reader4, false);
                                    reader4.Close();
                                    dictionary4.Add(str9, null);
                                }
                            }
                        }
                    }
                    writer.WriteEndElement();
                    writer.WriteStartElement("apis");
                    foreach(VersionInfo info6 in list)
                    {
                        XmlReader reader5 = XmlReader.Create(info6.File, settings);
                        reader5.MoveToContent();
                        while(reader5.Read())
                        {
                            if((reader5.NodeType == XmlNodeType.Element) && (reader5.LocalName == "api"))
                            {
                                string str10 = reader5.GetAttribute("id");
                                if(versionIndex.ContainsKey(str10))
                                {
                                    List<KeyValuePair<string, string>> versions = versionIndex[str10];
                                    KeyValuePair<string, string> pair = versions[0];
                                    if(info6.Name == pair.Key)
                                    {
                                        writer.WriteStartElement("api");
                                        writer.WriteAttributeString("id", str10);
                                        XmlReader reader6 = reader5.ReadSubtree();
                                        reader6.MoveToContent();
                                        reader6.ReadStartElement();
                                        while(!reader6.EOF)
                                        {
                                            if((reader6.NodeType == XmlNodeType.Element) && (reader6.LocalName == "elements"))
                                            {
                                                Dictionary<string, ElementInfo> dictionary5 = dictionary2[str10];
                                                Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
                                                writer.WriteStartElement("elements");
                                                XmlReader reader7 = reader6.ReadSubtree();
                                                foreach(XPathNavigator navigator8 in new XPathDocument(reader7).CreateNavigator().Select("elements/element"))
                                                {
                                                    string str11 = navigator8.GetAttribute("api", String.Empty);
                                                    dictionary6[str11] = null;
                                                    writer.WriteStartElement("element");
                                                    writer.WriteAttributeString("api", str11);
                                                    foreach(string str12 in dictionary5[str11].Versions.Keys)
                                                    {
                                                        writer.WriteAttributeString(str12, dictionary5[str11].Versions[str12]);
                                                    }
                                                    foreach(XPathNavigator navigator9 in navigator8.Select("@*"))
                                                    {
                                                        if(navigator9.LocalName != "api")
                                                        {
                                                            writer.WriteAttributeString(navigator9.LocalName, navigator9.Value);
                                                        }
                                                    }
                                                    foreach(XPathNavigator navigator10 in navigator8.Select("*"))
                                                    {
                                                        writer.WriteNode(navigator10, false);
                                                    }
                                                    writer.WriteEndElement();
                                                }
                                                reader7.Close();
                                                if(dictionary6.Count != dictionary5.Count)
                                                {
                                                    foreach(string str13 in dictionary5.Keys)
                                                    {
                                                        if(dictionary6.ContainsKey(str13) || (flag && !IsLatestElement(dictionary5[str13].Versions.Values, latestVersions)))
                                                        {
                                                            continue;
                                                        }
                                                        writer.WriteStartElement("element");
                                                        writer.WriteAttributeString("api", str13);
                                                        foreach(string str14 in dictionary5[str13].Versions.Keys)
                                                        {
                                                            writer.WriteAttributeString(str14, dictionary5[str13].Versions[str14]);
                                                        }
                                                        if(dictionary5[str13].ElementNode != null)
                                                        {
                                                            foreach(XPathNavigator navigator11 in dictionary5[str13].ElementNode.Select("@*"))
                                                            {
                                                                if(navigator11.LocalName != "api")
                                                                {
                                                                    writer.WriteAttributeString(navigator11.LocalName, navigator11.Value);
                                                                }
                                                            }
                                                            foreach(XPathNavigator navigator12 in dictionary5[str13].ElementNode.Select("*"))
                                                            {
                                                                writer.WriteNode(navigator12, false);
                                                            }
                                                        }
                                                        writer.WriteEndElement();
                                                    }
                                                }
                                                writer.WriteEndElement();
                                                reader6.Read();
                                            }
                                            else if(reader6.NodeType == XmlNodeType.Element)
                                            {
                                                writer.WriteNode(reader6, false);
                                            }
                                            else
                                            {
                                                reader6.Read();
                                            }
                                        }
                                        reader6.Close();
                                        writer.WriteStartElement("versions");
                                        foreach(XPathNavigator navigator13 in navigator.SelectChildren(XPathNodeType.Element))
                                        {
                                            WriteVersionTree(versions, navigator13, writer);
                                        }
                                        writer.WriteEndElement();
                                        writer.WriteEndElement();
                                    }
                                }
                            }
                        }
                        reader5.Close();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                finally
                {
                    writer.Close();
                }
            }
            catch(IOException)
            {
                return 1;
            }

            return 0;
        }

        private static bool IsLatestElement(Dictionary<string, string>.ValueCollection versions, List<string> latestVersions)
        {
            foreach(string str in versions)
                if(latestVersions.Contains(str))
                    return true;

            return false;
        }

        private static void RemoveOldApis(Dictionary<string, List<KeyValuePair<string, string>>> versionIndex, List<string> latestVersions)
        {
            string[] array = new string[versionIndex.Count];
            versionIndex.Keys.CopyTo(array, 0);

            foreach(string str in array)
            {
                List<KeyValuePair<string, string>> list = versionIndex[str];
                bool flag = true;

                foreach(KeyValuePair<string, string> pair in list)
                    if(latestVersions.Contains(pair.Key))
                    {
                        flag = false;
                        break;
                    }

                if(flag)
                    versionIndex.Remove(str);
            }
        }

        private static void WriteVersionTree(List<KeyValuePair<string, string>> versions, XPathNavigator branch, XmlWriter writer)
        {
            string localName = branch.LocalName;
            string attribute = branch.GetAttribute("name", String.Empty);

            switch(localName)
            {
                case "versions":
                    writer.WriteStartElement("versions");

                    if(!String.IsNullOrEmpty(attribute))
                        writer.WriteAttributeString("name", attribute);

                    foreach(XPathNavigator navigator in branch.SelectChildren(XPathNodeType.Element))
                        WriteVersionTree(versions, navigator, writer);

                    writer.WriteEndElement();
                    return;

                case "version":
                    foreach(KeyValuePair<string, string> pair in versions)
                        if(pair.Key == attribute)
                        {
                            writer.WriteStartElement("version");
                            writer.WriteAttributeString("name", attribute);
                            if(!String.IsNullOrEmpty(pair.Value))
                                writer.WriteAttributeString("obsolete", pair.Value);

                            writer.WriteEndElement();
                        }

                    break;
            }
        }
    }
}
