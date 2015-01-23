//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : ProxyCredentials.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/28/2013
// Note    : Copyright 2007-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to specify credentials for a proxy server
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.5.2.0  09/13/2007  EFW  Created the code
//===============================================================================================================

using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This class is used to specify credentials for a proxy server.
    /// </summary>
    public class ProxyCredentials
    {
        #region Private data members
        //=====================================================================

        private bool useProxyServer;
        private Uri proxyServer;
        private UserCredentials credentials;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or get the flag indicating whether or not to use the proxy server
        /// </summary>
        /// <value>By default, this is false and <see cref="ProxyServer"/> and <see cref="Credentials"/> will be
        /// ignored.</value>
        public bool UseProxyServer
        {
            get { return useProxyServer; }
            set { useProxyServer = value; }
        }

        /// <summary>
        /// Get or set the proxy server name
        /// </summary>
        /// <value>If <see cref="UseProxyServer"/> is false, this will be ignored</value>
        public Uri ProxyServer
        {
            get { return proxyServer; }
            set { proxyServer = value; }
        }

        /// <summary>
        /// Get the user credentials
        /// </summary>
        /// <value>If <see cref="UseProxyServer"/> is false, this will be ignored</value>
        public UserCredentials Credentials
        {
            get { return credentials; }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <overloads>There are two overloads for the constructor.</overloads>
        public ProxyCredentials()
        {
            credentials = new UserCredentials();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="useProxy">True to use default the proxy server, false to not use it</param>
        /// <param name="server">The server name to use</param>
        /// <param name="proxyUser">The user credentials to use for the proxy server</param>
        public ProxyCredentials(bool useProxy, Uri server, UserCredentials proxyUser)
        {
            useProxyServer = useProxy;
            proxyServer = server;

            if(proxyUser == null)
                credentials = new UserCredentials();
            else
                credentials = proxyUser;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Create a proxy credentials instance from an XPath navigator containing the settings
        /// </summary>
        /// <param name="navigator">The XPath navigator from which to obtain the settings</param>
        /// <returns>A <see cref="ProxyCredentials"/> object containing the settings from the XPath navigator</returns>
        /// <remarks>It should contain an element called <b>proxyCredentials</b> with two attributes
        /// (<c>useProxy</c> and <c>proxyServer</c>) and a nested <c>userCredentials</c> element.</remarks>
        public static ProxyCredentials FromXPathNavigator(XPathNavigator navigator)
        {
            ProxyCredentials credentials = new ProxyCredentials();
            UserCredentials user;
            string server;

            if(navigator != null)
            {
                navigator = navigator.SelectSingleNode("proxyCredentials");

                if(navigator != null)
                {
                    credentials.UseProxyServer = Convert.ToBoolean(navigator.GetAttribute("useProxy",
                        String.Empty), CultureInfo.InvariantCulture);
                    server = navigator.GetAttribute("proxyServer", String.Empty).Trim();

                    if(server.Length != 0)
                        credentials.ProxyServer = new Uri(server, UriKind.RelativeOrAbsolute);

                    user = UserCredentials.FromXPathNavigator(navigator);
                    credentials.Credentials.UseDefaultCredentials = user.UseDefaultCredentials;
                    credentials.Credentials.UserName = user.UserName;
                    credentials.Credentials.Password = user.Password;
                }
            }

            return credentials;
        }

        /// <summary>
        /// Store the credentials as a node in the given XML document
        /// </summary>
        /// <param name="config">The XML document</param>
        /// <param name="root">The node in which to store the element</param>
        /// <returns>Returns the node that was added or the one that already existed in the document</returns>
        /// <remarks>The credentials are stored in an element called <c>proxyCredentials</c> with two attributes
        /// (<c>useProxy</c> and <c>proxyServer</c>) and a nested <c>userCredentials</c> element.  It is created
        /// if it does not already exist.</remarks>
        public XmlNode ToXml(XmlDocument config, XmlNode root)
        {
            XmlNode node;
            XmlAttribute attr;

            if(root == null)
                throw new ArgumentNullException("root");

            node = root.SelectSingleNode("proxyCredentials");

            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element, "proxyCredentials", null);
                root.AppendChild(node);

                attr = config.CreateAttribute("useProxy");
                node.Attributes.Append(attr);
                attr = config.CreateAttribute("proxyServer");
                node.Attributes.Append(attr);
            }

            node.Attributes["useProxy"].Value = useProxyServer.ToString().ToLowerInvariant();
            node.Attributes["proxyServer"].Value = (proxyServer == null) ? String.Empty : proxyServer.OriginalString;

            credentials.ToXml(config, node);

            return node;
        }
        #endregion
    }
}
