using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace SandcastleGui
{
    public class ProjectFile
    {
        private StringCollection m_comments = new StringCollection();
        private StringCollection m_dependents = new StringCollection();
        private StringCollection m_dlls = new StringCollection();
        private bool m_hasChm;
        private bool m_hasHxs;
        private bool m_hasWeb;
        private int m_languageId;
        private string m_name;
        private string m_topicStyle;

        public void Load(string fileName)
        {
            XmlNamespaceManager resolver = new XmlNamespaceManager(new NameTable());
            resolver.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
            XPathDocument document = new XPathDocument(fileName);
            XPathNavigator navigator = document.CreateNavigator().SelectSingleNode("/ns:Project", resolver);
            string[] strArray = navigator.SelectSingleNode("@DefaultTargets").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            StringCollection strings = new StringCollection();
            foreach (string str in strArray)
            {
                strings.Add(str.ToLower());
            }
            if (strings.Contains("chm"))
            {
                this.m_hasChm = true;
            }
            if (strings.Contains("hxs"))
            {
                this.m_hasHxs = true;
            }
            if (strings.Contains("web"))
            {
                this.m_hasWeb = true;
            }
            XPathNavigator navigator2 = navigator.SelectSingleNode("ns:PropertyGroup/ns:Name", resolver);
            this.m_name = navigator2.Value;
            navigator2 = navigator.SelectSingleNode("ns:PropertyGroup/ns:TopicStyle", resolver);
            this.m_topicStyle = navigator2.Value.ToLower();
            navigator2 = navigator.SelectSingleNode("ns:PropertyGroup/ns:LanguageId", resolver);
            this.m_languageId = Convert.ToInt32(navigator2.Value);
            foreach (XPathNavigator navigator3 in navigator.Select("ns:ItemGroup/ns:Dlls/@Include", resolver))
            {
                this.m_dlls.Add(navigator3.Value);
            }
            foreach (XPathNavigator navigator4 in navigator.Select("ns:ItemGroup/ns:Comments/@Include", resolver))
            {
                this.m_comments.Add(navigator4.Value);
            }
            foreach (XPathNavigator navigator5 in navigator.Select("ns:ItemGroup/ns:Dependents/@Include", resolver))
            {
                this.m_dependents.Add(navigator5.Value);
            }
        }

        public void Save(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8) {
                    Formatting = Formatting.Indented,
                    Indentation = 4
                };
                writer.WriteStartDocument();
                writer.WriteStartElement("Project");
                writer.WriteAttributeString("xmlns", "http://schemas.microsoft.com/developer/msbuild/2003");
                string str = string.Empty;
                if (this.m_hasChm)
                {
                    str = str + ";Chm";
                }
                if (this.m_hasHxs)
                {
                    str = str + ";Hxs";
                }
                if (this.m_hasWeb)
                {
                    str = str + ";Web";
                }
                if (str.Length > 0)
                {
                    str = str.Substring(1);
                }
                writer.WriteAttributeString("DefaultTargets", str);
                writer.WriteStartElement("PropertyGroup");
                writer.WriteElementString("Name", this.m_name);
                writer.WriteElementString("TopicStyle", this.m_topicStyle);
                writer.WriteElementString("LanguageId", this.m_languageId.ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("ItemGroup");
                foreach (string str2 in this.m_dlls)
                {
                    writer.WriteStartElement("Dlls");
                    writer.WriteAttributeString("Include", str2);
                    writer.WriteEndElement();
                }
                foreach (string str3 in this.m_comments)
                {
                    writer.WriteStartElement("Comments");
                    writer.WriteAttributeString("Include", str3);
                    writer.WriteEndElement();
                }
                foreach (string str4 in this.m_dependents)
                {
                    writer.WriteStartElement("Dependents");
                    writer.WriteAttributeString("Include", str4);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", @"$(DXROOT)\Examples\Generic\generic.targets");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
        }

        public StringCollection Comments
        {
            get
            {
                return this.m_comments;
            }
        }

        public StringCollection Dependents
        {
            get
            {
                return this.m_dependents;
            }
        }

        public StringCollection Dlls
        {
            get
            {
                return this.m_dlls;
            }
        }

        public bool HasChm
        {
            get
            {
                return this.m_hasChm;
            }
            set
            {
                this.m_hasChm = value;
            }
        }

        public bool HasHxs
        {
            get
            {
                return this.m_hasHxs;
            }
            set
            {
                this.m_hasHxs = value;
            }
        }

        public bool HasWeb
        {
            get
            {
                return this.m_hasWeb;
            }
            set
            {
                this.m_hasWeb = value;
            }
        }

        public int LanguageId
        {
            get
            {
                return this.m_languageId;
            }
            set
            {
                this.m_languageId = value;
            }
        }

        public string Name
        {
            get
            {
                return this.m_name;
            }
            set
            {
                this.m_name = value;
            }
        }

        public string TopicStyle
        {
            get
            {
                return this.m_topicStyle;
            }
            set
            {
                this.m_topicStyle = value;
            }
        }
    }
}

