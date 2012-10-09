//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : DeploymentLocation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/24/2007
// Note    : Copyright 2007, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to specify a deployment location.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/17/2007  EFW  Created the code
//=============================================================================

using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This represents a deployment location
    /// </summary>
    public class DeploymentLocation
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private Uri location;
        private UserCredentials userCreds;
        private ProxyCredentials proxyCreds;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// The location to which the help file is deployed
        /// </summary>
        public Uri Location
        {
            get { return location; }
            set { location = value; }
        }

        /// <summary>
        /// The user credentials for the location
        /// </summary>
        public UserCredentials UserCredentials
        {
            get { return userCreds; }
        }

        /// <summary>
        /// The proxy credentials for the location
        /// </summary>
        public ProxyCredentials ProxyCredentials
        {
            get { return proxyCreds; }
        }
        #endregion

        //=====================================================================
        // Methods, etc.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <overloads>There are two overloads for the constructor</overloads>
        public DeploymentLocation()
        {
            userCreds = new UserCredentials();
            proxyCreds = new ProxyCredentials();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="deployTo">The deployment location</param>
        /// <param name="user">The user credentials, if any</param>
        /// <param name="proxy">The proxy credentials, if any</param>
        public DeploymentLocation(Uri deployTo, UserCredentials user,
            ProxyCredentials proxy)
        {
            location = deployTo;
            userCreds = user ?? new UserCredentials();
            proxyCreds = proxy ?? new ProxyCredentials();
        }

        /// <summary>
        /// Create a deployment location instance from an XPath navigator
        /// containing the settings.
        /// </summary>
        /// <param name="navigator">The XPath navigator from which to
        /// obtain the settings.</param>
        /// <param name="id">The id of the element to load</param>
        /// <returns>A <see cref="DeploymentLocation"/> object containing the
        /// settings from the XPath navigator.</returns>
        /// <remarks>It should contain an element called <b>deploymentLocation</b>
        /// with two attributes (<b>id</b> with the specified ID value and
        /// <b>location</b>) and nested <b>userCredentials</b> and
        /// <b>proxyCredentials</b> elements.</remarks>
        public static DeploymentLocation FromXPathNavigator(
          XPathNavigator navigator, string id)
        {
            DeploymentLocation depLoc = new DeploymentLocation();
            UserCredentials user;
            ProxyCredentials proxy;
            string location;

            if(navigator != null)
            {
                navigator = navigator.SelectSingleNode(
                    "deploymentLocation[@id='" + id + "']");

                if(navigator != null)
                {
                    location = navigator.GetAttribute("location",
                        String.Empty).Trim();

                    if(location.Length != 0)
                        depLoc.Location = new Uri(location,
                            UriKind.RelativeOrAbsolute);

                    user = UserCredentials.FromXPathNavigator(navigator);

                    depLoc.UserCredentials.UseDefaultCredentials =
                        user.UseDefaultCredentials;
                    depLoc.UserCredentials.UserName = user.UserName;
                    depLoc.UserCredentials.Password = user.Password;

                    proxy = ProxyCredentials.FromXPathNavigator(navigator);

                    depLoc.ProxyCredentials.UseProxyServer =
                        proxy.UseProxyServer;
                    depLoc.ProxyCredentials.ProxyServer = proxy.ProxyServer;
                    depLoc.ProxyCredentials.Credentials.UseDefaultCredentials =
                        proxy.Credentials.UseDefaultCredentials;
                    depLoc.ProxyCredentials.Credentials.UserName =
                        proxy.Credentials.UserName;
                    depLoc.ProxyCredentials.Credentials.Password =
                        proxy.Credentials.Password;
                }
            }

            return depLoc;
        }

        /// <summary>
        /// Store the deployment location as a node in the given XML document
        /// </summary>
        /// <param name="config">The XML document</param>
        /// <param name="root">The node in which to store the element</param>
        /// <param name="id">The id of the element to create</param>
        /// <returns>Returns the node that was added or the one that
        /// already existed in the document.</returns>
        /// <remarks>The deployment location is stored in an element called
        /// <b>deploymentLocation</b> with two attributes (<b>id</b> matching
        /// the specified id and <b>location</b>) and nested
        /// <b>userCredentials</b> and <b>proxyCredentials</b> elements.  It
        /// is created if it does not already exist.</remarks>
        public XmlNode ToXml(XmlDocument config, XmlNode root, string id)
        {
            XmlNode node;
            XmlAttribute attr;

            if(root == null)
                throw new ArgumentNullException("root");

            node = root.SelectSingleNode("deploymentLocation[@id='" +
                id + "']");

            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element,
                    "deploymentLocation", null);
                root.AppendChild(node);

                attr = config.CreateAttribute("id");
                attr.Value = id;
                node.Attributes.Append(attr);
                attr = config.CreateAttribute("location");
                node.Attributes.Append(attr);
            }

            node.Attributes["location"].Value = (location == null) ?
                String.Empty : location.OriginalString;

            userCreds.ToXml(config, node);
            proxyCreds.ToXml(config, node);

            return node;
        }
    }
}
