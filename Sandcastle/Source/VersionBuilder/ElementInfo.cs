using System.Collections.Generic;
using System.Xml.XPath;

namespace VersionBuilder
{
    internal class ElementInfo
    {
        // Fields
        public XPathNavigator ElementNode { get; private set; }

        public Dictionary<string, string> versions = new Dictionary<string, string>();

        public Dictionary<string, string> Versions
        {
            get { return versions; }
        }

        // Methods
        public ElementInfo(string versionGroup, string version, XPathNavigator elementNode)
        {
            // TODO: This is rather odd.  It's an instance member so it will never contain the
            // key on construction.  Is it supposed to be static?
            if(!this.Versions.ContainsKey(versionGroup))
                this.Versions.Add(versionGroup, version);

            // TODO: Uh, this.ElementNode is always going to be null here.  So....?
            if(elementNode != null && this.ElementNode == null)
                this.ElementNode = elementNode;
        }
    }
}
