//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : DeploymentLocation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2025
// Note    : Copyright 2007-2025, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to specify a deployment location.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/17/2007  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This represents a deployment location
    /// </summary>
    public class DeploymentLocation
    {
        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// The location to which the help file is deployed
        /// </summary>
        public Uri Location { get; set; }

        /// <summary>
        /// The user credentials for the location
        /// </summary>
        public UserCredentials UserCredentials { get; }

        /// <summary>
        /// The proxy credentials for the location
        /// </summary>
        public ProxyCredentials ProxyCredentials { get; }

        #endregion

        //=====================================================================
        // Methods, etc.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <overloads>There are two overloads for the constructor</overloads>
        public DeploymentLocation()
        {
            this.UserCredentials = new UserCredentials();
            this.ProxyCredentials = new ProxyCredentials();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="deployTo">The deployment location</param>
        /// <param name="user">The user credentials, if any</param>
        /// <param name="proxy">The proxy credentials, if any</param>
        public DeploymentLocation(Uri deployTo, UserCredentials user, ProxyCredentials proxy)
        {
            this.Location = deployTo;
            this.UserCredentials = user ?? new UserCredentials();
            this.ProxyCredentials = proxy ?? new ProxyCredentials();
        }

        /// <summary>
        /// Create a deployment location instance from an XML element containing the settings
        /// </summary>
        /// <param name="configuration">The XML element from which to obtain the settings</param>
        /// <param name="id">The id of the element to load</param>
        /// <returns>A <see cref="DeploymentLocation"/> object containing the settings from the XPath navigator.</returns>
        /// <remarks>It should contain an element called <b>deploymentLocation</b> with two attributes (<b>id</b>
        /// with the specified ID value and <b>location</b>) and nested <b>userCredentials</b> and
        /// <b>proxyCredentials</b> elements.</remarks>
        public static DeploymentLocation FromXml(XElement configuration, string id)
        {
            DeploymentLocation depLoc = new();

            configuration = configuration?.XPathSelectElement("deploymentLocation[@id='" + id + "']");

            if(configuration != null)
            {
                string location = configuration.Attribute("location").Value;

                if(!String.IsNullOrWhiteSpace(location))
                    depLoc.Location = new Uri(location, UriKind.RelativeOrAbsolute);

                UserCredentials user = UserCredentials.FromXml(configuration);

                depLoc.UserCredentials.UseDefaultCredentials = user.UseDefaultCredentials;
                depLoc.UserCredentials.UserName = user.UserName;
                depLoc.UserCredentials.Password = user.Password;

                ProxyCredentials proxy = ProxyCredentials.FromXml(configuration);

                depLoc.ProxyCredentials.UseProxyServer = proxy.UseProxyServer;
                depLoc.ProxyCredentials.ProxyServer = proxy.ProxyServer;
                depLoc.ProxyCredentials.Credentials.UseDefaultCredentials = proxy.Credentials.UseDefaultCredentials;
                depLoc.ProxyCredentials.Credentials.UserName = proxy.Credentials.UserName;
                depLoc.ProxyCredentials.Credentials.Password = proxy.Credentials.Password;
            }

            return depLoc;
        }

        /// <summary>
        /// Return the deployment location as an XML element
        /// </summary>
        /// <param name="id">The id of the element to create</param>
        /// <returns>The deployment location settings as an XML element</returns>
        /// <remarks>The deployment location is stored in an element called <strong>deploymentLocation</strong>
        /// with two attributes (<strong>id</strong> matching the specified id and <strong>location</strong>) and
        /// nested <strong>userCredentials</strong> and <strong>proxyCredentials</strong> elements.</remarks>
        public XElement ToXml(string id)
        {
            return new XElement("deploymentLocation",
                new XAttribute("id", id),
                new XAttribute("location", (this.Location == null) ? String.Empty : this.Location.OriginalString),
                this.UserCredentials.ToXml(),
                this.ProxyCredentials.ToXml());
        }
    }
}
