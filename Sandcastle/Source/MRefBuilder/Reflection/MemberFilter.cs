// Copyright (c) Microsoft Corporation.  All rights reserved.
//

// Change history:
// 03/28/2012 - EFW - Fixed IsExposedMember() so that it compares generic members using the "Name<T>" and
// "Name{T}" syntax so that it gets a match either way.
// 11/24/2013 - EFW - Cleaned up the code and removed unused members

using System;
using System.Globalization;
using System.Xml;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection
{
    /// <summary>
    /// This class implements the type member filter
    /// </summary>
    public class MemberFilter
    {
        #region Private data members
        //=====================================================================

        private string name;
        private bool exposed;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The XML reader from which to get the configuration information</param>
        public MemberFilter(XmlReader configuration)
        {
            if(configuration.NodeType != XmlNodeType.Element || configuration.Name != "member")
                throw new InvalidOperationException("The configuration element must be named 'member'");

            name = configuration.GetAttribute("name");
            exposed = Convert.ToBoolean(configuration.GetAttribute("expose"), CultureInfo.InvariantCulture);
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Check to see if the member is exposed or not by this entry
        /// </summary>
        /// <param name="member">The member to check</param>
        /// <returns>Null if the member is not represented by this entry, true if it is and it is exposed or
        /// false if it is and it is not exposed.</returns>
        public bool? IsExposedMember(Member member)
        {
            // Try for an exact match first
            if(member.Name.Name == name)
                return exposed;

            // !EFW - If the member name contains "<" and our name contains "{", this is probably a
            // generic using the XML comments syntax member ID so try for a match that way too.
            if(member.Name.Name.IndexOf('<') != -1 && name.IndexOf('{') != -1 &&
              member.Name.Name.Replace('<', '{').Replace('>', '}') == name)
                return exposed;

            return null;
        }
        #endregion
    }
}
