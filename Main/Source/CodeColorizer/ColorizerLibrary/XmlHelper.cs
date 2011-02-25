//=============================================================================
// System  : Code Colorizer Library
// File    : CodeColorizer.cs
// Author  : Jonathan de Halleux, (c) 2003
// Updated : 11/20/2006
// Compiler: Microsoft Visual C#
//
// This is used to colorize blocks of code for output as HTML.  The original
// Code Project article by Jonathan can be found at:
// http://www.codeproject.com/csharp/highlightcs.asp.
//
// Modifications by Eric Woodruff (Eric@EWoodruff.us) 11/2006:
//
//      Modified XmlAddChildCDATAElem to move trailing CR/LF's into their own
//      code tag.  This keeps all tags within a single line and we can more
//      easily add line numbering and region folding to the end result.
//
//      Modified XmlSetAttribute to only create the node if it doesn't already
//      exist to save the document from detaching and reattaching it.
//
//      Made the class internal and sealed as it serves no purpose outside of
//      the colorizer.
//
//=============================================================================

using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Web;

namespace ColorizerLibrary
{
	/// <summary>
	/// Some basic function helpers for xml creation.
	/// </summary>
	internal static class XmlHelper
	{
        // This is used to move trailing CR/LF's into their own code tag.
        // This keeps all tags within a single line and we can more easily
        // add line numbering and region folding to the end result.
        private static Regex reTrailingCrLfs = new Regex("[^\r\n]([\r\n]+$)");

		/// <summary>adds a CDATA child elem</summary>
		/// <param name="node">node to append child</param>
		/// <param name="nodeName">new child node name</param>
		/// <param name="cdata">CDATA value</param>
		/// <exception>If could not create child node</exception>
		/// <exception>If could not create CDATA node</exception>
		internal static void XmlAddChildCDATAElem(XmlNode node,
          string nodeName, string cdata)
		{
			XmlNode newNode = node.OwnerDocument.CreateElement(nodeName);

			if(newNode == null)
				return;

            // Hack workaround to prevent stuff like blank line comments
            // from consuming more than they should.
            if(nodeName != "code" && cdata == "\r")
                cdata = " \r";

			node.AppendChild(newNode);

            Match m = reTrailingCrLfs.Match(cdata);

            // Move trailing CR/LF's into their own code tag
            if(m.Success)
                cdata = cdata.Substring(0, m.Index + 1);

            XmlNode newCDATANode = node.OwnerDocument.CreateCDataSection(
                HttpUtility.HtmlEncode(cdata));

			if(newCDATANode == null)
				return;

			newNode.AppendChild(newCDATANode);

            // Add the node for the trailing CR/LFs if needed
            if(m.Success)
                XmlHelper.XmlAddChildCDATAElem(node, "code", m.Groups[1].Value);
		}

		/// <summary>
		/// Adds or updates an attribute in the node
		/// <param name="node">node to modify</param>
		/// <param name="name">Attribute name</param>
		/// <param name="text">Attribute value</param>
		/// </summary>
		internal static void XmlSetAttribute(XmlNode node, string name,
          string text)
		{
            XmlAttribute attr = node.Attributes[name];

            if(attr == null)
            {
			    attr = node.OwnerDocument.CreateAttribute(null,name,null);
                node.Attributes.Append(attr);
            }

            attr.Value = text;
		}
	}
}
