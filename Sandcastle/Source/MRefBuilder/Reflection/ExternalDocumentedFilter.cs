// Copyright (c) Microsoft Corporation.  All rights reserved.
//

// Change history:
// 02/02/2012 - EFW - Added CompilerGeneratedAttribute check to IsExposedType() as public members can
// sometimes be compiler generated (i.e. generated types for public fields that use the "fixed" keyword).

using System;
using System.Compiler;
using System.Linq;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools.Reflection
{
    /// <summary>
    /// Exposes all APIs for which documentation is written.  This includes all visible members except for
    /// property and event accessors (e.g. get_ methods) and delegate members (e.g. Invoke).
    /// enumeration members are included.
    /// </summary>
    public class ExternalDocumentedFilter : ApiFilter
    {
        bool protectedSealedVisible = false;
        bool noPIA = false;

        public ExternalDocumentedFilter() : base() { }

        public ExternalDocumentedFilter(XPathNavigator configuration) : base(configuration)
        {
            protectedSealedVisible = (bool)configuration.Evaluate("boolean(protectedSealed[@expose='true'])");
            noPIA = (bool)configuration.Evaluate("not(boolean(noPIA[@expose='false']))");
        }

        public override bool IsExposedMember(Member member)
        {
            if(member == null)
                throw new ArgumentNullException("member");

            TypeNode type = member.DeclaringType;

            // if the member isn't visible, we certainly won't expose it...
            if(!member.IsVisibleOutsideAssembly && !(protectedSealedVisible && type.IsSealed && (member.IsFamily || member.IsFamilyOrAssembly)))
                return (false);
            // ...but there are also some visible members we won't expose.
            // member of delegates are not exposed
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

            // the value field of enumerations is not exposed
            if(member.IsSpecialName && (type.NodeType == NodeType.EnumNode) && (member.NodeType == NodeType.Field))
            {
                string name = member.Name.Name;
                if(name == "value__")
                    return (false);
            }

            // protected members of sealed types are not exposed
            // change of plan -- yes they are
            // if (type.IsSealed && (member.IsFamily || member.IsFamilyOrAssembly)) return(false);

            // One more test to deal with a wierd case: a private method is an explicit implementation for
            // a property accessor, but is not marked with the special name flag. To find these, test for
            // the accessibility of the methods they implement
            if(member.IsPrivate && member.NodeType == NodeType.Method)
            {
                Method method = (Method)member;
                MethodList implements = method.ImplementedInterfaceMethods;
                if((implements.Count > 0) && (!IsExposedMember(implements[0])))
                    return (false);
            }

            // okay, passed all tests, the member is exposed as long as the filters allow it
            return (base.IsExposedMember(member));
        }

        // we are satistied with the default namespace expose test, so don't override it

        public override bool IsExposedType(TypeNode type)
        {
            if(type == null)
                throw new ArgumentNullException("type");

            // !EFW - Bug fix.  Compiler generated types can be public (i.e. member using the fixed keyword)
            // Don't include compiler-generated types.  Check this and all parents for compiler attributes.
            TypeNode curType = type;

            while(curType != null)
            {
                if(ListContainsAttribute(curType.Attributes, "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
                    return false;

                curType = curType.DeclaringType;    // Check the next parent
            }

            if(!type.IsVisibleOutsideAssembly)
                return false;

            // Filter out no-PIA COM types
            if(!noPIA)
            {
                if(IsEmbeddedInteropType(type))
                    return false;
            }

            // Expose any visible types allowed by the base filter
            return (base.IsExposedType(type));
        }

        // ApiFilter was extended to support interfaces that are filtered
        // out (embedded interop types) but still contribute to
        // the list of a type's implemented interfaces. See change
        // to MrefWriter.cs, method GetExposedInterfaces.

        public override bool IsDocumentedInterface(TypeNode type)
        {
            if(!noPIA && !IsEmbeddedInteropType(type))
                return true;

            return base.IsDocumentedInterface(type);
        }

        private bool IsEmbeddedInteropType(TypeNode type)
        {
            bool compilerGeneratedAttribute = false, typeIdentifierAttribute = false;

            for(int i = 0; i < type.Attributes.Count; i++)
            {
                if(type.Attributes[i].Type.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute")
                    compilerGeneratedAttribute = true;

                if(type.Attributes[i].Type.FullName == "System.Runtime.InteropServices.TypeIdentifierAttribute")
                    typeIdentifierAttribute = true;
            }

            if(compilerGeneratedAttribute && typeIdentifierAttribute)
                return true;

            return false;
        }

        private static bool ListContainsAttribute(AttributeList attributes, string name)
        {
            for(int i = 0; i < attributes.Count; i++)
                if(attributes[i].Type.FullName == name)
                    return (true);

            return (false);
        }

        private static bool NameContains(string name, string substring)
        {
            return (name.Contains(substring));
        }
    }
}
