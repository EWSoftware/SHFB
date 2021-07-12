//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : XmlCommentsFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/15/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains a class representing an XML comment file and is used when searching for and adding missing
// documentation tag information.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/26/2006  EFW  Created the code
// 12/07/2006  EFW  Added C++ comments fix-up
// 10/25/2007  EFW  Made the first fix-up more generic, added another
// 03/30/2012  EFW  Added interior_ptr<T> fix-up
// 05/04/2015  EFW  Removed comments fix-up code.  The Member ID Fix-Ups plug-in replaced it.
//===============================================================================================================

using System;
using System.IO;
using System.Text;
using System.Xml;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This represents an XML comment file and is used when searching for and adding missing documentation tag
    /// information.
    /// </summary>
    public class XmlCommentsFile
    {
        #region Private data members
        //=====================================================================

        private readonly string sourcePath;
        private string invalidReason;
        private Encoding enc;
        private XmlDocument comments;
        private XmlNode members;
        private bool wasModified, isValid;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the source path of the file
        /// </summary>
        public string SourcePath => sourcePath;

        /// <summary>
        /// This read-only property is used to get the encoding, typically UTF-8
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                if(comments == null)
                    this.LoadXmlComments();

                return enc;
            }
        }

        /// <summary>
        /// This read-only property indicates whether or not the comments file contains valid, well-formed XML
        /// </summary>
        /// <returns>True if it does, false if not.  Invalid XML comments files will not be used as a source
        /// for comments during the build.  <see cref="InvalidReason"/> returns the cause of the problem.</returns>
        public bool IsValid
        {
            get
            {
                if(comments == null)
                    this.LoadXmlComments();

                return isValid;
            }
        }

        /// <summary>
        /// If <see cref="IsValid"/> returns false, this returns the reason that the XML comments file is invalid
        /// </summary>
        public string InvalidReason => invalidReason;

        /// <summary>
        /// This is used to load the comments file on first use
        /// </summary>
        public XmlDocument Comments
        {
            get
            {
                if(comments == null)
                    this.LoadXmlComments();

                return comments;
            }
        }

        /// <summary>
        /// This read-only property is used to get the root members node
        /// </summary>
        public XmlNode Members
        {
            get
            {
                if(members == null)
                {
                    members = this.Comments.SelectSingleNode("doc/members");

                    if(members == null)
                    {
                        // Some framework NuGet packages have invalid XML comments files containing a root
                        // span element.  Try to fix those up automatically.
                        if(this.Comments.LastChild != null && this.Comments.LastChild.LocalName == "span" &&
                          this.Comments.LastChild.HasChildNodes)
                        {
                            members = this.Comments.LastChild.FirstChild;

                            if(members.LocalName == "doc")
                            {
                                this.Comments.RemoveChild(this.Comments.LastChild);
                                this.Comments.AppendChild(members);

                                members = members.SelectSingleNode("members");
                            }
                            else
                                members = null;
                        }

                        if(members == null)
                            throw new InvalidOperationException(sourcePath + " does not contain a 'doc/members' node");
                    }
                }

                return members;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">The XML comments filename</param>
        /// <exception cref="ArgumentNullException">This is thrown if the filename is null or an empty string</exception>
        public XmlCommentsFile(string filename)
        {
            if(String.IsNullOrEmpty(filename))
                throw new ArgumentException("filename cannot be null", nameof(filename));

            sourcePath = filename;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Mark the file as modified if a node is changed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void comments_NodeChanged(object sender, XmlNodeChangedEventArgs e)
        {
            wasModified = true;
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to load the XML comments file and ensure it is valid
        /// </summary>
        private void LoadXmlComments()
        {
            // Although Visual Studio doesn't add an encoding, the files are UTF-8 encoded
            enc = Encoding.UTF8;
            comments = new XmlDocument();

            try
            {
                comments.LoadXml(Utility.ReadWithEncoding(sourcePath, ref enc));
                isValid = true;
                invalidReason = null;
            }
            catch(Exception ex)
            {
                isValid = false;
                invalidReason = ex.Message;
                comments.LoadXml("<?xml version=\"1.0\"?><doc><assembly><name>BadXmlComments</name></assembly><members/></doc>");
            }

            comments.NodeChanged += comments_NodeChanged;
            comments.NodeInserted += comments_NodeChanged;
            comments.NodeRemoved += comments_NodeChanged;
        }

        /// <summary>
        /// Save the comments file if it was modified
        /// </summary>
        public void Save()
        {
            // Write the file back out with the appropriate encoding if it was modified
            if(wasModified)
            {
                using(StreamWriter sw = new StreamWriter(sourcePath, false, enc))
                {
                    comments.Save(sw);
                }

                wasModified = false;
            }
        }

        /// <summary>
        /// This can be used to force a reload of the comments file if changes were made to it outside of this
        /// instance.
        /// </summary>
        public void ForceReload()
        {
            comments = null;
            members = null;
            wasModified = false;
        }
        #endregion
    }
}
