//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : XmlCommentsFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/30/2012
// Note    : Copyright 2006-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing an XML comment file and is used
// when searching for and adding missing documentation tag information.
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
// 1.3.3.1  12/07/2006  EFW  Added C++ comments fixup
// 1.6.0.1  10/25/2007  EFW  Made the first fix-up more generic, added another
// 1.9.4.0  03/30/2012  EFW  Added interior_ptr<T> fixup
//=============================================================================

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This represents an XML comment file and is used when searching for and
    /// adding missing documentation tag information.
    /// </summary>
    public class XmlCommentsFile
    {
        #region Private data members
        //=====================================================================

        private static Regex reFixupComments1 = new Regex("`[0-9]+(\\{)");

        private static Regex reFixupComments2 = new Regex(
            "(member name=\".*?System\\.Collections\\.Generic.*?)(\\^)");

        private static Regex reFixupComments3 = new Regex("cref=\"!:([EFMNPT]|Overload):");

        private static Regex reInteriorPtrFixup = new Regex(@"cli\.interior_ptr{([^}]+?)}");

        private string sourcePath;
        private Encoding enc;
        private XmlDocument comments;
        private XmlNode members;
        private bool wasModified;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the source path of the file
        /// </summary>
        public string SourcePath
        {
            get { return sourcePath; }
        }

        /// <summary>
        /// This is used to load the comments file on first use
        /// </summary>
        public XmlDocument Comments
        {
            get
            {
                XmlNode node;
                string content, origPath = sourcePath;

                if(comments == null)
                {
                    // Although Visual Studio doesn't add an encoding, the files are UTF-8 encoded.
                    enc = Encoding.UTF8;
                    comments = new XmlDocument();

                    do
                    {
                        // Read it with the appropriate encoding
                        content = BuildProcess.ReadWithEncoding(sourcePath, ref enc);
                        comments.LoadXml(content);

                        // If redirected, load the specified file
                        node = comments.SelectSingleNode("doc/@redirect");

                        if(node != null)
                        {
                            sourcePath = Environment.ExpandEnvironmentVariables(node.Value);

                            // Some may contain %CORSYSDIR% which may not be defined.  If so, use an
                            // appropriate default.
                            if(sourcePath.IndexOf("%CORSYSDIR%", StringComparison.Ordinal) != -1)
                            {
                                sourcePath = sourcePath.Replace("%CORSYSDIR%",
                                    @"%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\");
                                sourcePath = Environment.ExpandEnvironmentVariables(sourcePath);
                            }
                        }

                    } while(node != null);

                    // If redirected, point back to the temporary copy and make sure it gets saved to keep
                    // the comments for the builds.
                    if(sourcePath != origPath)
                    {
                        sourcePath = origPath;
                        wasModified = true;
                    }

                    comments.NodeChanged += comments_NodeChanged;
                    comments.NodeInserted += comments_NodeChanged;
                }

                return comments;
            }
        }

        /// <summary>
        /// Mark the file as modified if a node is changed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        void comments_NodeChanged(object sender, XmlNodeChangedEventArgs e)
        {
            wasModified = true;
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
                        throw new InvalidOperationException(sourcePath +
                            " does not contain a 'doc/members' node");
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
        /// <exception cref="ArgumentNullException">This is thrown if the filename is null or an
        /// empty string.</exception>
        public XmlCommentsFile(string filename)
        {
            if(String.IsNullOrEmpty(filename))
                throw new ArgumentException("filename cannot be null", "filename");

            sourcePath = filename;
        }
        #endregion

        #region Methods
        //=====================================================================

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
        /// This is called to fixup the comments for C++ compiler generated XML comments files.
        /// </summary>
        /// <remarks>The C++ compiler generates method signatures that differ from the other .NET compilers
        /// for methods that take generics as parameters.  These methods fail to get documented as they do
        /// not match the output of <b>MRefBuilder</b>.  The C# and VB.NET compilers generate names that do
        /// match it and this option is not needed for comments files generated by them.  The C++ compiler
        /// also has problems resolving references to some members if it hasn't seen them yet.  These are
        /// prefixed with "!:" which is removed by the fix-up code.  Parameters that use
        /// interior_ptr&lt;T&gt; also do not match the reflection output and need to be converted to
        /// the explicit dereference syntax.</remarks>
        public void FixupComments()
        {
            this.Save();
            comments = null;
            members = null;
            enc = Encoding.UTF8;

            // Read it from the XML document as it handles redirection
            string content = this.Comments.OuterXml;

            // Strip out "`" followed by digits and "^" in member names and fix-up cref attributes that
            // the compiler couldn't figure out.  Also convert interior_ptr<T> to explicit dereferences.
            content = reFixupComments1.Replace(content, "$1");
            content = reFixupComments2.Replace(content, "$1");
            content = reFixupComments3.Replace(content, "cref=\"$1:");
            content = reInteriorPtrFixup.Replace(content,
                "$1@!System.Runtime.CompilerServices.IsExplicitlyDereferenced");

            // Write the file back out using its original encoding
            using(StreamWriter sw = new StreamWriter(sourcePath, false, enc))
            {
                sw.Write(content);
                comments = null;
                members = null;
                wasModified = false;
            }
        }
        #endregion
    }
}
