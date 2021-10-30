//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ProxyCredentials.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/05/2021
// Note    : Copyright 2007-2021, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to specify credentials for a proxy server
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/13/2007  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This class is used to specify credentials for a proxy server.
    /// </summary>
    public class ProxyCredentials
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or get the flag indicating whether or not to use the proxy server
        /// </summary>
        /// <value>By default, this is false and <see cref="ProxyServer"/> and <see cref="Credentials"/> will be
        /// ignored.</value>
        public bool UseProxyServer { get; set; }

        /// <summary>
        /// Get or set the proxy server name
        /// </summary>
        /// <value>If <see cref="UseProxyServer"/> is false, this will be ignored</value>
        public Uri ProxyServer { get; set; }

        /// <summary>
        /// Get the user credentials
        /// </summary>
        /// <value>If <see cref="UseProxyServer"/> is false, this will be ignored</value>
        public UserCredentials Credentials { get; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <overloads>There are two overloads for the constructor.</overloads>
        public ProxyCredentials()
        {
            this.Credentials = new UserCredentials();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="useProxy">True to use default the proxy server, false to not use it</param>
        /// <param name="server">The server name to use</param>
        /// <param name="proxyUser">The user credentials to use for the proxy server</param>
        public ProxyCredentials(bool useProxy, Uri server, UserCredentials proxyUser)
        {
            this.UseProxyServer = useProxy;
            this.ProxyServer = server;

            if(proxyUser == null)
                this.Credentials = new UserCredentials();
            else
                this.Credentials = proxyUser;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Create a proxy credentials instance from an XML element
        /// </summary>
        /// <param name="configuration">The XML element from which to obtain the settings</param>
        /// <returns>A <see cref="ProxyCredentials"/> object containing the settings from the XPath navigator</returns>
        /// <remarks>It should contain an element called <b>proxyCredentials</b> with two attributes
        /// (<c>useProxy</c> and <c>proxyServer</c>) and a nested <c>userCredentials</c> element.</remarks>
        public static ProxyCredentials FromXml(XElement configuration)
        {
            ProxyCredentials credentials = new ProxyCredentials();

            if(configuration != null)
            {
                var node = configuration.Element("proxyCredentials");

                if(node != null)
                {
                    credentials.UseProxyServer = (bool)node.Attribute("useProxy");
                    string server = node.Attribute("proxyServer").Value.Trim();

                    if(server.Length != 0)
                        credentials.ProxyServer = new Uri(server, UriKind.RelativeOrAbsolute);

                    UserCredentials user = UserCredentials.FromXml(node);

                    credentials.Credentials.UseDefaultCredentials = user.UseDefaultCredentials;
                    credentials.Credentials.UserName = user.UserName;
                    credentials.Credentials.Password = user.Password;
                }
            }

            return credentials;
        }

        /// <summary>
        /// Converts the proxy credentials to an XML element
        /// </summary>
        /// <returns>The proxy credentials settings in an XML element</returns>
        /// <remarks>The credentials are stored in an element called <c>proxyCredentials</c> with two attributes
        /// (<c>useProxy</c> and <c>proxyServer</c>) and a nested <c>userCredentials</c> element.</remarks>
        public XElement ToXml()
        {
            return new XElement("proxyCredentials",
                new XAttribute("useProxy", this.UseProxyServer),
                new XAttribute("proxyServer", (this.ProxyServer == null) ? String.Empty : this.ProxyServer.OriginalString),
                this.Credentials.ToXml());
        }
        #endregion
    }
}
