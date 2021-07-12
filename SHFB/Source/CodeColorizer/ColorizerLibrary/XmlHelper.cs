//===============================================================================================================
// System  : Code Colorizer Library
// File    : XmlHelper.cs
// Author  : Jonathan de Halleux, (c) 2003
// Updated : 04/06/2021
//
// This file contains some XML node extension methods.
//
// Modifications by Eric Woodruff (Eric@EWoodruff.us) 11/2006-12/2012:
//
//      Modified XmlAddChildCDATAElem to move trailing CR/LF's into their own code tag.  This keeps all tags
//      within a single line and we can more easily add line numbering and region folding to the end result.
//
//      Modified XmlSetAttribute to only create the node if it doesn't already exist to save the document from
//      detaching and reattaching it.
//
//      Made the class internal and sealed as it serves no purpose outside of the colorizer.
//
//      Made the methods extension methods.
//
//===============================================================================================================

using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

namespace ColorizerLibrary
{
    /// <summary>
    /// This contains some basic extension methods for XML creation.
    /// </summary>
    internal static class XmlHelper
    {
        // This is used to move trailing CR/LF's into their own code tag.  This keeps all tags within a single
        // line and we can more easily add line numbering and region folding to the end result.
        private static readonly Regex reTrailingCrLfs = new Regex("[^\r\n]([\r\n]+$)");

        /// <summary>
        /// Adds a CDATA child element
        /// </summary>
        /// <param name="node">node to append child</param>
        /// <param name="nodeName">new child node name</param>
        /// <param name="cdata">CDATA value</param>
        /// <exception>If could not create child node</exception>
        /// <exception>If could not create CDATA node</exception>
        internal static void XmlAddChildCDATAElement(this XmlNode node, string nodeName, string cdata)
        {
            XmlNode newNode = node.OwnerDocument.CreateElement(nodeName);

            if(newNode == null)
                return;

            // Hack workaround to prevent stuff like blank line comments from consuming more than they should
            if(nodeName != "code" && cdata == "\r")
                cdata = " \r";

            node.AppendChild(newNode);

            Match m = reTrailingCrLfs.Match(cdata);

            // Move trailing CR/LF's into their own code tag
            if(m.Success)
                cdata = cdata.Substring(0, m.Index + 1);

            XmlNode newCDATANode = node.OwnerDocument.CreateCDataSection(WebUtility.HtmlEncode(cdata));

            if(newCDATANode == null)
                return;

            newNode.AppendChild(newCDATANode);

            // Add the node for the trailing CR/LFs if needed
            if(m.Success)
                node.XmlAddChildCDATAElement("code", m.Groups[1].Value);
        }

        /// <summary>
        /// Adds or updates an attribute in the node
        /// </summary>
        /// <param name="node">node to modify</param>
        /// <param name="name">Attribute name</param>
        /// <param name="text">Attribute value</param>
        internal static void XmlSetAttribute(this XmlNode node, string name, string text)
        {
            XmlAttribute attr = node.Attributes[name];

            if(attr == null)
            {
                attr = node.OwnerDocument.CreateAttribute(null, name, null);
                node.Attributes.Append(attr);
            }

            attr.Value = text;
        }
    }
}
