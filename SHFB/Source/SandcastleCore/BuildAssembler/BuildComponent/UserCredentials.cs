//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : UserCredentials.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2025
// Note    : Copyright 2007-2025, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to specify user credentials
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

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This class is used to specify user credentials.
    /// </summary>
    public class UserCredentials
    {
        #region Private data members
        //=====================================================================

        private string userName, password;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or get the flag indicating whether or not to use default credentials
        /// </summary>
        /// <value>By default, this is true and <see cref="UserName"/> and <see cref="Password"/> will be ignored</value>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// Get or set the user name
        /// </summary>
        /// <value>If <see cref="UseDefaultCredentials"/> is true, this will be ignored</value>
        public string UserName
        {
            get => userName;
            set
            {
                if(String.IsNullOrWhiteSpace(value))
                    userName = String.Empty;
                else
                    userName = value;
            }
        }

        /// <summary>
        /// Get or set the password
        /// </summary>
        /// <value>If <see cref="UseDefaultCredentials"/> is true, this will be ignored</value>
        public string Password
        {
            get => password;
            set
            {
                if(String.IsNullOrWhiteSpace(value))
                    password = String.Empty;
                else
                    password = value;
            }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <overloads>There are three overloads for the constructor.</overloads>
        public UserCredentials()
        {
            this.UseDefaultCredentials = true;
            userName = password = String.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="useDefault">True to use default credentials, false to use the supplied user name and
        /// password.</param>
        /// <param name="userName">The user name to use.</param>
        /// <param name="password">The password to use.</param>
        public UserCredentials(bool useDefault, string userName, string password)
        {
            this.UseDefaultCredentials = useDefault;
            this.UserName = userName;
            this.Password = password;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Create a user credentials instance from an XML element
        /// </summary>
        /// <param name="configuration">The XML element from which to obtain the settings</param>
        /// <returns>A <see cref="UserCredentials"/> object containing the settings from the XPath navigator</returns>
        /// <remarks>It should contain an element called <c>userCredentials</c> with three attributes:
        /// <c>useDefault</c>, <c>userName</c>, and <c>password</c>.</remarks>
        public static UserCredentials FromXml(XElement configuration)
        {
            UserCredentials credentials = new();

            if(configuration != null)
            {
                var node = configuration.Element("userCredentials");

                if(node != null)
                {
                    credentials.UseDefaultCredentials = (bool)node.Attribute("useDefault");
                    credentials.UserName = node.Attribute("userName").Value.Trim();
                    credentials.Password = node.Attribute("password").Value.Trim();
                }
            }

            return credentials;
        }

        /// <summary>
        /// Converts the user credentials to an XML element
        /// </summary>
        /// <returns>The user credentials settings in an XML element</returns>
        /// <remarks>The credentials are stored in an element called <c>userCredentials</c> with three
        /// attributes: <c>useDefault</c>, <c>userName</c>, and <c>password</c>.</remarks>
        public XElement ToXml()
        {
            return new XElement("userCredentials",
                new XAttribute("useDefault", this.UseDefaultCredentials),
                new XAttribute("userName", userName),
                new XAttribute("password", password));
        }
        #endregion
    }
}
