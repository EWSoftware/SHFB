// Copyright (c) Microsoft Corporation.  All rights reserved.
//

// Change history:
// 01/30/2012 - EFW - Fixed IsExposedMember() and IsExposedType() so that they ignore unrecognized
// visibilities.  This can happen in obfuscated assemblies.
// 02/16/2012 - EFW - Fixed IsExposedExpression() so that it doesn't exclude a type in an attribute
// expression as long as the hidden type has exposed members thus exposing the type.
// 03/01/2012 - EFW - Fixed IsExposedNamespace(), IsExposedType(), and IsExposedMember() so that they
// exclude members with names containing characters that are not valid in XML (i.e. obfuscated member
// names).
// 03/02/2012 - EFW - Added HasExposedMembers() check to IsExposedType() so that it doesn't exclude
// a type that contains exposed members when the type is marked as not exposed.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection
{
    public class ApiFilter
    {
        #region Member Variables

        private RootFilter apiFilter = new RootFilter(true);

        private RootFilter attributeFilter = new RootFilter(true);

        private Dictionary<string, bool> namespaceCache = new Dictionary<string, bool>(),
            typeExposedCache = new Dictionary<string,bool>();
        #endregion

        #region Constructors

        // stored filters

        public ApiFilter()
        {
        }

        public ApiFilter(XPathNavigator configuration)
        {

            if(configuration == null)
                throw new ArgumentNullException("configuration");

            // API filter nodes
            XPathNavigator apiFilterNode = configuration.SelectSingleNode("apiFilter");
            if(apiFilterNode != null)
            {
                XmlReader configurationReader = apiFilterNode.ReadSubtree();
                configurationReader.MoveToContent();
                apiFilter = new RootFilter(configurationReader);
                configurationReader.Close();
            }

            // Attribute filter nodes
            XPathNavigator attributeFilterNode = configuration.SelectSingleNode("attributeFilter");
            if(attributeFilterNode != null)
            {
                XmlReader configurationReader = attributeFilterNode.ReadSubtree();
                configurationReader.MoveToContent();
                attributeFilter = new RootFilter(configurationReader);
                configurationReader.Close();
            }

        }

        #endregion

        #region Public API

        public virtual bool IsDocumentedInterface(TypeNode type)
        {
            if(type == null)
                throw new ArgumentException("type");
            return (apiFilter.IsExposedType(type));
        }

        public virtual bool HasExposedMembers(TypeNode type)
        {
            if(type == null)
                throw new ArgumentNullException("type");
            return (apiFilter.HasExposedMembers(type));
        }

        // exposure logic for artibrary APIs
        // call the appropriate particular exposure logic

        public virtual bool IsExposedApi(Member api)
        {

            Namespace space = api as Namespace;
            if(space != null)
                return (IsExposedNamespace(space));

            TypeNode type = api as TypeNode;
            if(type != null)
                return (IsExposedType(type));

            return (IsExposedMember(api));
        }

        public virtual bool IsExposedAttribute(AttributeNode attribute)
        {
            if(attribute == null)
                throw new ArgumentNullException("attribute");

            // check whether attribte type is exposed
            TypeNode attributeType = attribute.Type;
            if(!IsExposedType(attributeType))
                return (false);

            // check whether expressions used to instantiate attribute are exposed
            ExpressionList expressions = attribute.Expressions;
            for(int i = 0; i < expressions.Count; i++)
            {
                if(!IsExposedExpression(expressions[i]))
                    return (false);
            }

            // apply user filters to attribute
            return (attributeFilter.IsExposedType(attributeType));
        }

        public virtual bool IsExposedMember(Member member)
        {
            if(member == null)
                throw new ArgumentNullException("member");

            // !EFW - Bug fix.  Some obfuscated assemblies have mangled names containing characters that
            // are not valid in XML.  Exclude those by default.
            if(member.FullName.ToCharArray().Any(ch => ch < 0x20 || ch > 0xFFFD))
                return false;

            // !EFW - Bug fix.  If not a recognized visibility, ignore it as it's probably an obfuscated
            // member and it won't be of any use anyway.
            if(!member.IsPublic && !member.IsAssembly && !member.IsFamilyOrAssembly && !member.IsFamily &&
              !member.IsFamilyAndAssembly && !member.IsPrivate)
                return false;

            return (apiFilter.IsExposedMember(member));
        }

        // namespce logic
        // a namespace is exposed if any type in it is exposed

        public virtual bool IsExposedNamespace(Namespace space)
        {
            if(space == null)
                throw new ArgumentNullException("space");

            // !EFW - Bug fix.  Some obfuscated assemblies have mangled names containing characters that
            // are not valid in XML.  Exclude those by default.
            if(space.FullName.ToCharArray().Any(ch => ch < 0x20 || ch > 0xFFFD))
                return false;

            string name = space.Name.Name;

            // look in cache to see if namespace exposure is already determined
            bool exposed;

            if(!namespaceCache.TryGetValue(name, out exposed))
            {
                // it is not; determine exposure now

                // the namespace is exposed if any types in it are exposed              
                exposed = NamespaceContainesExposedTypes(space) ?? false;

                // the namespace is also exposed if it contains exposed members, even if all types are hidden
                if(!exposed)
                {
                    exposed = NamespaceContainsExposedMembers(space);
                }

                // cache the result 
                namespaceCache.Add(name, exposed);

            }
            return (exposed);
        }

        // type and member logic
        // by default, types and members are exposed if a user filter says it, or if no user filter forbids it

        public virtual bool IsExposedType(TypeNode type)
        {
            bool exposed;

            if(type == null)
                throw new ArgumentNullException("type");

            // !EFW - Bug fix.  Some obfuscated assemblies have mangled names containing characters that
            // are not valid in XML.  Exclude those by default.
            if(type.FullName.ToCharArray().Any(ch => ch < 0x20 || ch > 0xFFFD))
                return false;

            // !EFW - Bug fix.  If not a recognized visibility, ignore it as it's probably an obfuscated
            // type and it won't be of any use anyway.
            if(!type.IsPublic && !type.IsAssembly && !type.IsFamilyOrAssembly && !type.IsFamily &&
              !type.IsFamilyAndAssembly && !type.IsPrivate)
                return false;

            // !EFW - Added a check for exposed members in unexposed types.  This effectively exposes the
            // type and it should be included whenever this check occurs for it.
            if(!typeExposedCache.TryGetValue(type.FullName, out exposed))
            {
                exposed = apiFilter.IsExposedType(type);

                if(!exposed)
                {
                    // The type is exposed if any of its members are exposed
                    exposed = this.HasExposedMembers(type);

                    // Cache the result 
                    typeExposedCache.Add(type.FullName, exposed);
                }
            }

            return exposed;
        }

        #endregion

        #region Implementation

        private bool IsExposedExpression(Expression expression)
        {
            if(expression.NodeType == NodeType.Literal)
            {
                Literal literal = (Literal)expression;
                TypeNode type = literal.Type;

                if(!IsExposedType(type))
                    return false;

                if(type.FullName == "System.Type")
                {
                    // if the value is itself a type, we need to test whether that type is visible
                    TypeNode value = literal.Value as TypeNode;

                    // !EFW - Bug Fix.  Don't exclude the type if it has exposed members.  That
                    // effectively exposes the type as well.
                    if(value != null && !IsExposedType(value) && !HasExposedMembers(value))
                        return false;
                }

                return true;
            }
            else if(expression.NodeType == NodeType.NamedArgument)
            {
                NamedArgument assignment = (NamedArgument)expression;
                return IsExposedExpression(assignment.Value);
            }
            else
            {
                throw new InvalidOperationException("Encountered unrecognized expression");
            }
        }

        private bool? NamespaceContainesExposedTypes(Namespace space)
        {
            TypeNodeList types = space.Types;

            for(int i = 0; i < types.Count; i++)
            {
                TypeNode type = types[i];
                if(IsExposedType(type))
                    return (true);
            }

            if(apiFilter.NamespaceFilterCount < 1)
            {
                return null; //this apiFilter does not contain any namespaces
            }

            return (false);
        }


        /** <summary>Check for any exposed members in any of the types.
         * Returns true if the type has an exposed memeber filter and
         * it is matched. This is used to determine if the namespace
         * should be visited if the namespace and all types are set to 
         * false for exposed, we still want to visit them if any members
         * are set to true.
         * </summary> */
        private bool NamespaceContainsExposedMembers(Namespace space)
        {
            TypeNodeList types = space.Types;
            for(int i = 0; i < types.Count; i++)
            {
                TypeNode type = types[i];

                if(HasExposedMembers(type))
                    return true;
            }
            return (false);
        }

        #endregion

    }

}
