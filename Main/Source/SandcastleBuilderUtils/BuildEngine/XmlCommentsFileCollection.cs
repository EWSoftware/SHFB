//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : XmlCommentsFileCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/27/2012
// Note    : Copyright 2006-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the XML comments files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.3.1.0  09/26/2006  EFW  Created the code
// 1.6.0.2  11/10/2007  EFW  Moved the CommentFileList method from
//                           XmlCommentsFileCollection to this class.
// 1.6.0.5  03/02/2008  EFW  Added support for the <inheritdoc /> tag.
// 1.6.0.6  03/08/2008  EFW  Added support for NamespaceDoc classes
//=============================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This collection class is used to hold the XML comments files
    /// during the build process.
    /// </summary>
    public class XmlCommentsFileCollection : BindingList<XmlCommentsFile>
    {
        /// <summary>
        /// This read-only property returns true if any of the comments files
        /// contain an &lt;inheritdoc /&gt; tag indicating that the Inherited
        /// Documentation tool will need to be ran.
        /// </summary>
        public bool ContainsInheritedDocumentation
        {
            get
            {
                foreach(XmlCommentsFile f in this)
                    if(f.Members.SelectSingleNode("//inheritdoc") != null)
                        return true;

                return false;
            }
        }

        /// <summary>
        /// Save the comments files
        /// </summary>
        public void Save()
        {
            foreach(XmlCommentsFile file in this)
                file.Save();
        }

        /// <summary>
        /// Search all comments files for the specified member.  If not found,
        /// add the blank member to the first file.
        /// </summary>
        /// <param name="memberName">The member name for which to search.</param>
        /// <returns>The XML node of the found or added member</returns>
        public XmlNode FindMember(string memberName)
        {
            XmlDocument doc;
            XmlNode member = null;
            XmlAttribute name;

            string xPathQuery = String.Format(CultureInfo.InvariantCulture, "member[@name='{0}']",
                memberName);

            foreach(XmlCommentsFile f in this)
            {
                member = f.Members.SelectSingleNode(xPathQuery);

                if(member != null)
                    break;
            }

            // If not found at all, add an entry for it to the first file
            if(member == null)
            {
                doc = this[0].Comments;
                member = doc.CreateNode(XmlNodeType.Element, "member", null);

                name = doc.CreateAttribute("name");
                name.Value = memberName;
                member.Attributes.Append(name);

                this[0].Members.AppendChild(member);
            }

            return member;
        }

        /// <summary>
        /// This will search for all type member comments where the ID
        /// contains <b>NamespaceDoc</b> and convert them to namespace entries
        /// for the containing namespace.
        /// </summary>
        /// <remarks>The converted ID effectively converts the comments into
        /// comments for the class's containing namespace.</remarks>
        public void ReplaceNamespaceDocEntries()
        {
            foreach(XmlCommentsFile f in this)
                foreach(XmlNode member in f.Members.SelectNodes(
                  "member[starts-with(@name, 'T:') and contains(@name, '.NamespaceDoc')]/@name"))
                    member.Value = "N:" + member.Value.Substring(2, member.Value.Length - 15);
        }

        /// <summary>
        /// Returns a list of the comment file paths in a format suitable
        /// for inserting into a Sandcastle Configuration file.
        /// </summary>
        /// <param name="workingFolder">The working folder path</param>
        /// <param name="forInheritedDocs">True if generating the list for the
        /// inherited documentation tool or false for sandcastle.config.</param>
        /// <returns>The comment file list XML tags</returns>
        internal string CommentFileList(string workingFolder, bool forInheritedDocs)
        {
            StringBuilder sb = new StringBuilder(2048);
            string tagName, dupWarning = String.Empty;

            if(forInheritedDocs)
                tagName = "scan file";
            else
            {
                tagName = "data files";
                dupWarning = " duplicateWarning=\"false\" ";
            }

            // The path is not altered if the file is already in or under the working folder (i.e.
            // files added by plug-ins).
            foreach(XmlCommentsFile f in this)
                if(!f.SourcePath.StartsWith(workingFolder, StringComparison.OrdinalIgnoreCase))
                    sb.AppendFormat("            <{0}=\"{1}{2}\"{3} />\r\n", tagName,
                        HttpUtility.HtmlEncode(workingFolder), Path.GetFileName(f.SourcePath), dupWarning);
                else
                    sb.AppendFormat("            <{0}=\"{1}\"{2} />\r\n", tagName,
                        HttpUtility.HtmlEncode(f.SourcePath), dupWarning);

            return sb.ToString();
        }
    }
}
