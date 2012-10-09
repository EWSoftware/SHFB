// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System.Compiler;
using System.Linq;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools.Reflection
{
    /// <summary>
    /// This exposes all apis, including internal apis, for which documentation is written.  This includes
    /// all members except for property and event accessors (e.g. get_ methods) and delegate members (e.g.
    /// Invoke).  Enumeration members are included
    /// </summary>
    public class AllDocumentedFilter : ApiFilter
    {
        public AllDocumentedFilter() : base() { }

        public AllDocumentedFilter(XPathNavigator configuration) : base(configuration) { }

        public override bool IsExposedMember(Member member)
        {
            // member of delegates are not exposed
            TypeNode type = member.DeclaringType;

            if(type.NodeType == NodeType.DelegateNode)
                return (false);

            // accessor methods for properties and events are not exposed
            if(member.IsSpecialName && (member.NodeType == NodeType.Method))
            {
                string name = member.Name.Name;

                if(NameContains(name, "get_"))
                    return (false);

                if(NameContains(name, "set_"))
                    return (false);

                if(NameContains(name, "add_"))
                    return (false);

                if(NameContains(name, "remove_"))
                    return (false);

                if(NameContains(name, "raise_"))
                    return (false);
            }

            // The value field of enumerations is not exposed
            if(member.IsSpecialName && (type.NodeType == NodeType.EnumNode) &&
              (member.NodeType == NodeType.Field))
            {
                string name = member.Name.Name;

                if(name == "value__")
                    return (false);
            }

            // Members marked as compiler-generated are not exposed
            if(ListContainsAttribute(member.Attributes, "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
                return (false);

            // Okay, passed all tests, so member is exposed
            return (base.IsExposedMember(member));
        }

        // All namespace and all types are exposed

        /// <summary>
        /// Check the given type and all parent types for compiler attributes.  If none are found look for
        /// any filters to determine if it is exposed.
        /// </summary>
        public override bool IsExposedType(TypeNode type)
        {
            // Don't include compiler-generated types
            // Check this and all parents for compiler attributes
            TypeNode curType = type; //cursor

            while(curType != null)
            {
                if(ListContainsAttribute(curType.Attributes, "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
                    return false;

                curType = curType.DeclaringType; //check the next parent
            }

            // Continue on with checking if the type is exposed
            return base.IsExposedType(type);
        }

        private static bool ListContainsAttribute(AttributeList attributes, string name)
        {
            for(int i = 0; i < attributes.Count; i++)
            {
                if(attributes[i].Type.FullName == name)
                    return (true);
            }
            return (false);
        }

        private static bool NameContains(string name, string substring)
        {
            return (name.Contains(substring));
        }
    }
}
