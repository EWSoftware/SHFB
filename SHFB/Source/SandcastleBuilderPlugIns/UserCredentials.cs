//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : UserCredentials.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/24/2007
// Note    : Copyright 2007, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to specify user credentials.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/13/2007  EFW  Created the code
//=============================================================================

using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This class is used to specify user credentials.
    /// </summary>
    public class UserCredentials
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private bool useDefaultCredentials;
        private string userName, password;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This is used to set or get the flag indicating whether or not to
        /// use default credentials.
        /// </summary>
        /// <value>By default, this is true and <see cref="UserName"/> and
        /// <see cref="Password"/> will be ignored.</value>
        public bool UseDefaultCredentials
        {
            get { return useDefaultCredentials; }
            set { useDefaultCredentials = value; }
        }

        /// <summary>
        /// Get or set the user name
        /// </summary>
        /// <value>If <see cref="UseDefaultCredentials"/> is true, this will
        /// be ignored.</value>
        public string UserName
        {
            get { return userName; }
            set
            {
                if(value == null)
                    userName = String.Empty;
                else
                    userName = value;
            }
        }

        /// <summary>
        /// Get or set the password
        /// </summary>
        /// <value>If <see cref="UseDefaultCredentials"/> is true, this will
        /// be ignored.</value>
        public string Password
        {
            get { return password; }
            set
            {
                if(value == null)
                    password = String.Empty;
                else
                    password = value;
            }
        }
        #endregion

        //=====================================================================
        // Methods, etc.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <overloads>There are two overloads for the constructor.</overloads>
        public UserCredentials()
        {
            useDefaultCredentials = true;
            userName = password = String.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="useDefault">True to use default credentials, false to
        /// use the supplied user name and password.</param>
        /// <param name="user">The user name to use.</param>
        /// <param name="pwd">The password to use.</param>
        public UserCredentials(bool useDefault, string user, string pwd)
        {
            useDefaultCredentials = useDefault;
            userName = user;
            password = pwd;
        }

        /// <summary>
        /// Create a user credentials instance from an XPath navigator
        /// containing the settings.
        /// </summary>
        /// <param name="navigator">The XPath navigator from which to
        /// obtain the settings.</param>
        /// <returns>A <see cref="UserCredentials"/> object containing the
        /// settings from the XPath navigator.</returns>
        /// <remarks>It should contain an element called <b>userCredentials</b>
        /// with three attributes: <b>useDefault</b>, <b>userName</b>, and
        /// <b>password</b>.</remarks>
        public static UserCredentials FromXPathNavigator(
          XPathNavigator navigator)
        {
            UserCredentials credentials = new UserCredentials();

            if(navigator != null)
            {
                navigator = navigator.SelectSingleNode("userCredentials");

                if(navigator != null)
                {
                    credentials.UseDefaultCredentials = Convert.ToBoolean(
                        navigator.GetAttribute("useDefault", String.Empty),
                        CultureInfo.InvariantCulture);
                    credentials.UserName = navigator.GetAttribute("userName",
                        String.Empty).Trim();
                    credentials.Password = navigator.GetAttribute("password",
                        String.Empty).Trim();
                }
            }

            return credentials;
        }

        /// <summary>
        /// Store the credentials as a node in the given XML document
        /// </summary>
        /// <param name="config">The XML document</param>
        /// <param name="root">The node in which to store the element</param>
        /// <returns>Returns the node that was added or the one that
        /// already existed in the document.</returns>
        /// <remarks>The credentials are stored in an element called
        /// <b>userCredentials</b> with three attributes:  <b>useDefault</b>,
        /// <b>userName</b>, and <b>password</b>.  It is created if it
        /// does not already exist.</remarks>
        public XmlNode ToXml(XmlDocument config, XmlNode root)
        {
            XmlNode node;
            XmlAttribute attr;

            if(root == null)
                throw new ArgumentNullException("root");

            node = root.SelectSingleNode("userCredentials");

            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element,
                    "userCredentials", null);
                root.AppendChild(node);

                attr = config.CreateAttribute("useDefault");
                node.Attributes.Append(attr);
                attr = config.CreateAttribute("userName");
                node.Attributes.Append(attr);
                attr = config.CreateAttribute("password");
                node.Attributes.Append(attr);
            }

            node.Attributes["useDefault"].Value =
                useDefaultCredentials.ToString().ToLower(
                    CultureInfo.InvariantCulture);
            node.Attributes["userName"].Value = userName;
            node.Attributes["password"].Value = password;

            return node;
        }
    }
}
