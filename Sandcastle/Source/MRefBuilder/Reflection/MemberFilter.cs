// Copyright (c) Microsoft Corporation.  All rights reserved.
//

// Change history:
// 03/28/2012 - EFW - Fixed IsExposedMember() so that it compares generic members using the
// "Name<T>" and "Name{T}" syntax so that it gets a match either way.

using System;
using System.Xml;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection
{

    public class MemberFilter
    {

        #region Member Variables

        private bool exposed;

        private string name;

        #endregion

        #region Constructors
        public MemberFilter(string name, bool exposed)
        {
            if(name == null)
                throw new ArgumentNullException("name");
            this.name = name;
            this.exposed = exposed;
        }

        public MemberFilter(XmlReader configuration)
        {
            if((configuration.NodeType != XmlNodeType.Element) || (configuration.Name != "member"))
                throw new InvalidOperationException();
            name = configuration.GetAttribute("name");
            exposed = Convert.ToBoolean(configuration.GetAttribute("expose"));
        }
        #endregion

        #region Public API
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
