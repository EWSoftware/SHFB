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
// 11/30/2012 - EFW - Added updates based on changes submitted by ComponentOne to fix crashes caused by
// obfuscated member names.
// 11/19/2013 - EFW - Merged common code from AllDocumentedFilter and ExternalDocumentedFilter into this class.
// 11/20/2013 - EFW - Merged code from Stazzz to implement namespace grouping support.  Cleaned up the code and
// removed unused members.  Merged all SHFB visibility options into the API filter.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection
{
    /// <summary>
    /// This class is used to implement the API filter which removes unwanted members from the output
    /// </summary>
    public class ApiFilter
    {
        #region Private data members
        //=====================================================================

        private VisibleItems visibleItems;
        private RootFilter apiFilter, attributeFilter;
        private Dictionary<string, bool> namespaceCache, typeExposedCache, namespaceGroupCache;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set whether or not attributes on types and members are included in the output
        /// </summary>
        /// <value>Set to true to include attributes or false to hide them.  When false certain required
        /// attributes will still be included such as <c>ObsoleteAttribute</c>, <c>ExtensionAttribute</c>, etc.</value>
        public bool IncludeAttributes
        {
            get { return ((visibleItems & VisibleItems.Attributes) != 0); }
            set
            {
                if(value)
                    visibleItems |= VisibleItems.Attributes;
                else
                    visibleItems &= ~VisibleItems.Attributes;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not explicit interface implementations are included in the
        /// output.
        /// </summary>
        /// <value>Set to true to include explicit interface implementations or false to hide them</value>
        public bool IncludeExplicitInterfaceImplementations
        {
            get { return ((visibleItems & VisibleItems.ExplicitInterfaceImplementations) != 0); }
            set
            {
                if(value)
                    visibleItems |= VisibleItems.ExplicitInterfaceImplementations;
                else
                    visibleItems &= ~VisibleItems.ExplicitInterfaceImplementations;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not inherited members are included in the output
        /// </summary>
        /// <value>Set to true to include inherited members or false to hide them</value>
        public bool IncludeInheritedMembers
        {
            get { return ((visibleItems & VisibleItems.InheritedMembers) != 0); }
            set
            {
                if(value)
                    visibleItems |= VisibleItems.InheritedMembers;
                else
                    visibleItems &= ~(VisibleItems.InheritedMembers | VisibleItems.InheritedFrameworkMembers |
                        VisibleItems.InheritedFrameworkInternalMembers |
                        VisibleItems.InheritedFrameworkPrivateMembers);
            }
        }

        /// <summary>
        /// This is used to get or set whether or not inherited framework members are included in the output
        /// </summary>
        /// <value>Set to true to include inherited framework members or false to hide them.  For this to work,
        /// <see cref="IncludeInheritedMembers"/> must also be enabled.</value>
        public bool IncludeInheritedFrameworkMembers
        {
            get { return ((visibleItems & VisibleItems.InheritedFrameworkMembers) != 0); }
            set
            {
                if(value)
                    visibleItems |= (VisibleItems.InheritedMembers | VisibleItems.InheritedFrameworkMembers);
                else
                    visibleItems &= ~(VisibleItems.InheritedFrameworkMembers |
                        VisibleItems.InheritedFrameworkInternalMembers |
                        VisibleItems.InheritedFrameworkPrivateMembers);
            }
        }

        /// <summary>
        /// This is used to get or set whether or not inherited private framework members are included in the
        /// output.
        /// </summary>
        /// <value>Set to true to include inherited private framework members or false to hide them.  For this to
        /// work, <see cref="IncludeInheritedFrameworkMembers"/> and <see cref="IncludePrivates"/> must also be
        /// enabled.</value>
        public bool IncludeInheritedFrameworkPrivateMembers
        {
            get { return ((visibleItems & VisibleItems.InheritedFrameworkPrivateMembers) != 0); }
            set
            {
                if(value)
                {
                    visibleItems |= (VisibleItems.InheritedMembers | VisibleItems.InheritedFrameworkMembers |
                        VisibleItems.InheritedFrameworkPrivateMembers | VisibleItems.Privates);
                }
                else
                    visibleItems &= ~VisibleItems.InheritedFrameworkPrivateMembers;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not inherited internal framework members are included in the
        /// output.
        /// </summary>
        /// <value>Set to true to include inherited internal framework members or false to hide them.  For this
        /// to work, <see cref="IncludeInheritedFrameworkMembers"/> and <see cref="IncludeInternals"/> must also
        /// be enabled.</value>
        public bool IncludeInheritedFrameworkInternalMembers
        {
            get { return ((visibleItems & VisibleItems.InheritedFrameworkInternalMembers) != 0); }
            set
            {
                if(value)
                {
                    visibleItems |= (VisibleItems.InheritedMembers | VisibleItems.InheritedFrameworkMembers |
                        VisibleItems.InheritedFrameworkInternalMembers | VisibleItems.Internals);
                }
                else
                    visibleItems &= ~VisibleItems.InheritedFrameworkInternalMembers;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not internal members are included in the output
        /// </summary>
        /// <value>Set to true to include internal members or false to hide them</value>
        public bool IncludeInternals
        {
            get { return ((visibleItems & VisibleItems.Internals) != 0); }
            set
            {
                if(value)
                    visibleItems |= VisibleItems.Internals;
                else
                    visibleItems &= ~(VisibleItems.Internals | VisibleItems.InheritedFrameworkInternalMembers);
            }
        }

        /// <summary>
        /// This is used to get or set whether or not private members are included in the output
        /// </summary>
        /// <value>Set to true to include private members or false to hide them</value>
        public bool IncludePrivates
        {
            get { return ((visibleItems & VisibleItems.Privates) != 0); }
            set
            {
                if(value)
                    visibleItems |= VisibleItems.Privates;
                else
                    visibleItems &= ~(VisibleItems.Privates | VisibleItems.PrivateFields |
                        VisibleItems.InheritedFrameworkPrivateMembers);
            }
        }

        /// <summary>
        /// This is used to get or set whether or not private fields are included in the output
        /// </summary>
        /// <value>Set to true to include private fields or false to hide them.  For this to work,
        /// <see cref="IncludePrivates"/> must also be enabled.</value>
        /// <remarks>Private fields are most often used to back properties and do not have documentation.  With
        /// this set to false, they are omitted from the output to reduce unnecessary clutter.</remarks>
        public bool IncludePrivateFields
        {
            get { return ((visibleItems & VisibleItems.PrivateFields) != 0); }
            set
            {
                if(value)
                    visibleItems |= (VisibleItems.Privates | VisibleItems.PrivateFields);
                else
                    visibleItems &= ~VisibleItems.PrivateFields;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not protected members are included in the output
        /// </summary>
        /// <value>Set to true to include protected members or false to hide them</value>
        public bool IncludeProtected
        {
            get { return ((visibleItems & VisibleItems.Protected) != 0); }
            set
            {
                if(value)
                    visibleItems |= VisibleItems.Protected;

                else
                    visibleItems &= ~(VisibleItems.Protected | VisibleItems.SealedProtected);
            }
        }

        /// <summary>
        /// This is used to get or set whether or not "protected internal" members are output as "protected" only
        /// in the output.
        /// </summary>
        /// <value>Set to true to output "protected internal" members as "protected" only or false to include
        /// them normally.  This option is ignored if <see cref="IncludeProtected"/> is false.</value>
        public bool IncludeProtectedInternalAsProtected
        {
            get { return ((visibleItems & VisibleItems.ProtectedInternalAsProtected) != 0); }
            set
            {
                if(value)
                    visibleItems |= VisibleItems.ProtectedInternalAsProtected;
                else
                    visibleItems &= ~VisibleItems.ProtectedInternalAsProtected;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not protected members of sealed classes are included in the
        /// output.
        /// </summary>
        /// <value>Set to true to include protected members of sealed classes or false to hide them.  For this to
        /// work, <see cref="IncludeProtected"/> must also be enabled.</value>
        public bool IncludeSealedProtected
        {
            get { return ((visibleItems & VisibleItems.SealedProtected) != 0); }
            set
            {
                if(value)
                    visibleItems |= (VisibleItems.SealedProtected | VisibleItems.Protected);
                else
                    visibleItems &= ~VisibleItems.SealedProtected;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not no-PIA (Primary Interop Assembly) embedded interop types
        /// are included in the output.
        /// </summary>
        /// <value>Set to true to include no-PIA embedded interop types or false to hide them</value>
        public bool IncludeNoPIATypes
        {
            get { return ((visibleItems & VisibleItems.NoPIATypes) != 0); }
            set
            {
                if(value)
                    visibleItems |= VisibleItems.NoPIATypes;
                else
                    visibleItems &= ~VisibleItems.NoPIATypes;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The XPath navigator from which to read the configuration</param>
        public ApiFilter(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException("configuration");

            namespaceCache = new Dictionary<string, bool>();
            typeExposedCache = new Dictionary<string, bool>();
            namespaceGroupCache = new Dictionary<string, bool>();

            // Visibility settings
            this.IncludeAttributes = (bool)configuration.Evaluate("boolean(visibility/attributes[@expose='true'])");
            this.IncludeExplicitInterfaceImplementations = (bool)configuration.Evaluate("boolean(visibility/explicitInterfaceImplementations[@expose='true'])");
            this.IncludeInheritedMembers = (bool)configuration.Evaluate("boolean(visibility/inheritedMembers[@expose='true'])");
            this.IncludeInheritedFrameworkMembers = (bool)configuration.Evaluate("boolean(visibility/inheritedFrameworkMembers[@expose='true'])");
            this.IncludeInheritedFrameworkInternalMembers = (bool)configuration.Evaluate("boolean(visibility/inheritedFrameworkInternalMembers[@expose='true'])");
            this.IncludeInheritedFrameworkPrivateMembers = (bool)configuration.Evaluate("boolean(visibility/inheritedFrameworkPrivateMembers[@expose='true'])");
            this.IncludeInternals = (bool)configuration.Evaluate("boolean(visibility/internals[@expose='true'])");
            this.IncludePrivates = (bool)configuration.Evaluate("boolean(visibility/privates[@expose='true'])");
            this.IncludePrivateFields = (bool)configuration.Evaluate("boolean(visibility/privateFields[@expose='true'])");
            this.IncludeProtected = (bool)configuration.Evaluate("boolean(visibility/protected[@expose='true'])");
            this.IncludeSealedProtected = (bool)configuration.Evaluate("boolean(visibility/sealedProtected[@expose='true'])");
            this.IncludeProtectedInternalAsProtected = (bool)configuration.Evaluate("boolean(visibility/protectedInternalAsProtected[@expose='true'])");
            this.IncludeNoPIATypes = (bool)configuration.Evaluate("boolean(visibility/noPIATypes[@expose='true'])");

            // API filter
            XPathNavigator apiFilterNode = configuration.SelectSingleNode("apiFilter");

            if(apiFilterNode != null)
            {
                XmlReader configurationReader = apiFilterNode.ReadSubtree();
                configurationReader.MoveToContent();
                apiFilter = new RootFilter(configurationReader);
                configurationReader.Close();
            }
            else
                apiFilter = new RootFilter();

            // Attribute filter
            XPathNavigator attributeFilterNode = configuration.SelectSingleNode("attributeFilter");

            if(attributeFilterNode != null)
            {
                XmlReader configurationReader = attributeFilterNode.ReadSubtree();
                configurationReader.MoveToContent();
                attributeFilter = new RootFilter(configurationReader);
                configurationReader.Close();
            }
            else
                attributeFilter = new RootFilter();
        }
        #endregion

        #region API filter methods
        //=====================================================================

        /// <summary>
        /// This is used to see if a type has exposed members
        /// </summary>
        /// <param name="type">The type to check for exposed members</param>
        /// <returns>True if it has exposed members, false if not</returns>
        public virtual bool HasExposedMembers(TypeNode type)
        {
            if(type == null)
                throw new ArgumentNullException("type");

            return apiFilter.HasExposedMembers(type);
        }

        /// <summary>
        /// This is used to see if an API member is exposed based on its type
        /// </summary>
        /// <param name="api">The API member to check</param>
        /// <returns>True if it is exposed, false if not</returns>
        public virtual bool IsExposedApi(Member api)
        {
            Namespace space = api as Namespace;

            if(space != null)
                return this.IsExposedNamespace(space);

            TypeNode type = api as TypeNode;

            if(type != null)
                return this.IsExposedType(type);

            return this.IsExposedMember(api);
        }

        /// <summary>
        /// This is used to see if a namespace is exposed
        /// </summary>
        /// <param name="space">The namespace to check</param>
        /// <returns>True if the namespace contains any exposed types or types with exposed members, false if not</returns>
        public virtual bool IsExposedNamespace(Namespace space)
        {
            if(space == null)
                throw new ArgumentNullException("space");

            // !EFW - Bug fix.  Some obfuscated assemblies have mangled names containing characters that
            // are not valid in XML.  Exclude those by default.
            if(space.FullName.HasInvalidXmlCharacters())
                return false;

            string name = space.Name.Name;

            // Look in cache to see if namespace exposure is already determined
            bool exposed;

            if(!namespaceCache.TryGetValue(name, out exposed))
            {
                // The namespace is exposed if any types in it are exposed              
                exposed = this.NamespaceContainsExposedTypes(space) ?? false;

                // the namespace is also exposed if it contains exposed members, even if all types are hidden
                if(!exposed)
                    exposed = this.NamespaceContainsExposedMembers(space);

                // Cache the result 
                namespaceCache.Add(name, exposed);
            }

            return exposed;
        }

        /// <summary>
        /// This is used to see if a namespace group is exposed
        /// </summary>
        /// <param name="namespaceGroup">The namespace group to check</param>
        /// <returns>True if the namespace group is exposed, false if not</returns>
        public virtual Boolean IsExposedNamespaceGroup(string namespaceGroup)
        {
            if(namespaceGroup == null)
                throw new ArgumentNullException("namespaceGroup");

            // !EFW - Bug fix.  Some obfuscated assemblies have mangled names containing characters that
            // are not valid in XML.  Exclude those by default.
            if(namespaceGroup.HasInvalidXmlCharacters())
                return false;

            // Look in cache to see if namespace exposure is already determined
            bool exposed;

            if(!namespaceGroupCache.TryGetValue(namespaceGroup, out exposed))
            {
                exposed = this.apiFilter.IsExposedNamespaceGroup(namespaceGroup);
                namespaceGroupCache.Add(namespaceGroup, exposed);
            }

            return exposed;
        }

        /// <summary>
        /// This is used to see if a type is exposed
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if the type is exposed, false if not</returns>
        public virtual bool IsExposedType(TypeNode type)
        {
            bool exposed;

            if(type == null)
                throw new ArgumentNullException("type");

            // !EFW - Bug fix.  Some obfuscated assemblies have mangled names containing characters that
            // are not valid in XML.  Exclude those by default.
            if(type.FullName.HasInvalidXmlCharacters())
                return false;

            // !EFW - Bug fix.  If not a recognized visibility, ignore it as it's probably an obfuscated
            // type and it won't be of any use anyway.
            if(!type.IsPublic && !type.IsAssembly && !type.IsFamilyOrAssembly && !type.IsFamily &&
              !type.IsFamilyAndAssembly && !type.IsPrivate)
                return false;

            // !EFW - Added a check for exposed members in unexposed types.  This effectively exposes the type
            // and it should be included whenever this check occurs for it.
            if(!typeExposedCache.TryGetValue(type.FullName, out exposed))
            {
                exposed = apiFilter.IsExposedType(type);

                // The type is exposed if any of its members are exposed
                if(!exposed)
                    exposed = this.HasExposedMembers(type);

                // !EFW - Bug fix.  Compiler generated types can be public (i.e. member using the fixed keyword).
                // Don't include compiler-generated types.  Check this and all parents for a compiler generated
                // attribute.  No-PIA types are kept if wanted though.
                TypeNode curType = type;

                while(curType != null)
                {
                    if(curType.Attributes.Any(
                      attr => attr.Type.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute") &&
                      (!this.IncludeNoPIATypes || !IsEmbeddedInteropType(curType)))
                        return false;

                    curType = curType.DeclaringType;    // Check the next parent
                }

                if(!this.IsVisible(type))
                    exposed = false;

                // Filter out no-PIA embedded interop types
                if(!this.IncludeNoPIATypes && IsEmbeddedInteropType(type))
                    exposed = false;

                // Cache the result 
                typeExposedCache.Add(type.FullName, exposed);
            }

            return exposed;
        }

        /// <summary>
        /// This is used to determine if the given interface is to be documented
        /// </summary>
        /// <param name="type">The type node for the interface</param>
        /// <returns>True if it is to be documented, false if not</returns>
        public virtual bool IsDocumentedInterface(TypeNode type)
        {
            if(type == null)
                throw new ArgumentNullException("type");

            // ApiFilter was extended to support interfaces that are filtered out (embedded interop types) but
            // still contribute to the list of a type's implemented interfaces.  See change to MrefWriter.cs,
            // method GetExposedInterfaces.
            if(!this.IncludeNoPIATypes && !IsEmbeddedInteropType(type))
                return true;

            if(!this.IsVisible(type))
                return false;

            return apiFilter.IsExposedType(type);
        }

        /// <summary>
        /// This is used to see if a member is exposed
        /// </summary>
        /// <param name="Member">The member node to check</param>
        /// <returns>True if the member is exposed, false if not</returns>
        public virtual bool IsExposedMember(Member member)
        {
            if(member == null)
                throw new ArgumentNullException("member");

            // !EFW - Bug fix.  Some obfuscated assemblies have mangled names containing characters that
            // are not valid in XML.  Exclude those by default.
            if(member.FullName.HasInvalidXmlCharacters())
                return false;

            // !EFW - Bug fix.  If not a recognized visibility, ignore it as it's probably an obfuscated
            // member and it won't be of any use anyway.
            if(!member.IsPublic && !member.IsAssembly && !member.IsFamilyOrAssembly && !member.IsFamily &&
              !member.IsFamilyAndAssembly && !member.IsPrivate)
                return false;

            TypeNode type = member.DeclaringType;

            // Members of delegates are not exposed
            if(type.NodeType == NodeType.DelegateNode)
                return false;

            // Accessor methods for properties and events are not exposed
            if(member.IsSpecialName && member.NodeType == NodeType.Method)
            {
                string name = member.Name.Name;

                if(name.Contains("get_"))
                    return false;

                if(name.Contains("set_"))
                    return false;

                if(name.Contains("add_"))
                    return false;

                if(name.Contains("remove_"))
                    return false;

                if(name.Contains("raise_"))
                    return false;
            }

            // The value field of enumerations is not exposed
            if(member.IsSpecialName && type.NodeType == NodeType.EnumNode && member.NodeType == NodeType.Field &&
              member.Name.Name == "value__")
                return false;

            // Members marked as compiler-generated are not exposed
            if(member.Attributes.Any(attr => attr.Type.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
                return false;

            // If not visible based on the visibility settings, ignore it
            if(!this.IsVisible(member))
                return false;

            // One more test to deal with a weird case: a private method is an explicit implementation for
            // a property accessor, but is not marked with the special name flag. To find these, test for
            // the accessibility of the methods they implement.
            if(member.IsPrivate && member.NodeType == NodeType.Method)
            {
                Method method = (Method)member;
                MethodList implements = method.ImplementedInterfaceMethods;

                if(implements != null && implements.Count > 0 && !this.IsExposedMember(implements[0]))
                    return false;
            }

            // Same for explicit property implementations
            if(member.IsPrivate && member.NodeType == NodeType.Property)
            {
                Property property = (Property)member;
                var implements = property.GetImplementedProperties().ToList();

                if(implements.Count > 0 && !this.IsExposedMember(implements[0]))
                    return false;
            }

            // Same for explicit event implementations
            if(member.IsPrivate && member.NodeType == NodeType.Event)
            {
                Event evt = (Event)member;
                var implements = evt.GetImplementedEvents().ToList();

                if(implements.Count > 0 && !this.IsExposedMember(implements[0]))
                    return false;
            }

            // Okay, passed all tests, the member is exposed as long as the base filters allow it
            return apiFilter.IsExposedMember(member);
        }

        /// <summary>
        /// This is used to see if an attribute type is exposed
        /// </summary>
        /// <param name="attribute">The attribute node to check</param>
        /// <returns>True if the attribute is exposed, false if not</returns>
        public virtual bool IsExposedAttribute(AttributeNode attribute)
        {
            if(attribute == null)
                throw new ArgumentNullException("attribute");

            // Check whether the attribute type is exposed
            TypeNode attributeType = attribute.Type;

            if(!this.IsExposedType(attributeType))
                return false;

            // Check whether expressions used to instantiate the attribute are exposed
            foreach(var expression in attribute.Expressions)
                if(!this.IsExposedExpression(expression))
                    return false;

            // If excluding attributes, just check for ones that are required
            if(!this.IncludeAttributes)
                return attributeFilter.IsRequiredType(attributeType);

            // Apply user filters to the attribute
            return attributeFilter.IsExposedType(attributeType);
        }

        /// <summary>
        /// This is used to determine if a type or member is visible based on the visibility settings
        /// </summary>
        /// <param name="member">The type or member to check</param>
        /// <returns>True if visible based on the current visibility settings, false if not</returns>
        public virtual bool IsVisible(Member member)
        {
            // Handle types first as a limited set of options apply to them
            TypeNode type = member as TypeNode;

            if(type != null)
            {
                // All parent types must be visible
                if(type.DeclaringType != null && !this.IsVisible(type.DeclaringType))
                    return false;

                // For protected, if it's protected internal, only remove them if not including internals
                if((!this.IncludePrivates && type.IsPrivate) || (!this.IncludeInternals && type.IsAssembly) ||
                  (!this.IncludeProtected && (type.IsFamily || (type.IsFamilyOrAssembly && !this.IncludeInternals))))
                    return false;

                return true;
            }

            type = member.DeclaringType;
            Property property = member as Property;
            Event evt = member as Event;
            Method method = member as Method;

            // Explicit member implementation?
            if((property != null && property.IsPrivate && property.IsVirtual) ||
              (evt != null && evt.IsPrivate && evt.IsVirtual) ||
              (method != null && method.IsPrivate && method.IsVirtual))
            {
                if(!this.IsVisible(type))
                    return false;

                return this.IncludeExplicitInterfaceImplementations;
            }

            if(this.IncludePrivates && member.IsPrivate)
            {
                Field field = member as Field;

                if(field != null)
                {
                    if(!this.IncludePrivateFields)
                        return false;

                    // Remove backing fields that correspond to events.  These can never be documented and should
                    // not show up.
                    if(field.Type.Name.Name.Contains("EventHandler"))
                        return false;
                }

                return this.IsVisible(type);
            }
                
            if(this.IncludeInternals && (member.IsAssembly || member.IsFamilyAndAssembly))
                return this.IsVisible(type);

            // For protected, if it's protected internal, only remove them if not including internals
            if(!this.IncludeProtected && (member.IsFamily || (member.IsFamilyOrAssembly && !this.IncludeInternals)))
                return false;

            // If the member isn't visible outside the assembly, we won't expose it unless including protected
            // members of sealed classes, private classes, and/or internal classes.
            if(!member.IsVisibleOutsideAssembly)
            {
                if(!this.IncludeSealedProtected && type.IsSealed && (member.IsFamily || member.IsFamilyOrAssembly))
                    return false;

                if(!member.IsPrivate && !(member.IsAssembly || member.IsFamilyAndAssembly))
                    return this.IsVisible(type);

                return false;
            }

            return true;
        }

        /// <summary>
        /// This is used to see if a member is an inherited base framework member and, if so, if it should be
        /// excluded.
        /// </summary>
        /// <param name="type">The type to compare the member against</param>
        /// <param name="member">The potential inherited base framework member</param>
        /// <returns>True if the inherited base framework member is to be excluded, false to include it</returns>
        public bool IsExcludedFrameworkMember(TypeNode type, Member member)
        {
            string memberNamespace = member.DeclaringType.Namespace.Name;

            if(type.Namespace.Name != memberNamespace && (memberNamespace == "System" ||
              memberNamespace == "Microsoft" ||
              memberNamespace.StartsWith("System.", StringComparison.Ordinal) ||
              memberNamespace.StartsWith("Microsoft.", StringComparison.Ordinal)))
            {
                Property property = member as Property;
                Event evt = member as Event;
                Method method = member as Method;

                // Explicit member implementation?  Keep these unless removing all framework members.
                if((property != null && property.IsPrivate && property.IsVirtual) ||
                  (evt != null && evt.IsPrivate && evt.IsVirtual) ||
                  (method != null && method.IsPrivate && method.IsVirtual))
                {
                    return !this.IncludeInheritedFrameworkMembers;
                }

                return (!this.IncludeInheritedFrameworkMembers ||
                    (!this.IncludeInheritedFrameworkInternalMembers && member.IsAssembly) ||
                    (!this.IncludeInheritedFrameworkPrivateMembers && (member.IsPrivate ||
                    member.IsFamilyAndAssembly)));
            }

            return false;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to get a description of the member's visibility
        /// </summary>
        /// <param name="api">The API member for which to get the visibility description</param>
        /// <returns>The visibility description</returns>
        public string GetVisibility(Member api)
        {
            if(api == null)
                throw new ArgumentNullException("api");

            if(api.IsPublic)
                return "public";

            // Internal
            if(api.IsAssembly)
                return "assembly";

            // Protected internal
            if(api.IsFamilyOrAssembly)
            {
                if(this.IncludeProtectedInternalAsProtected)
                    return "family";

                return "family or assembly";
            }

            // Protected
            if(api.IsFamily)
                return "family";

            // Protected private (accessible by the class and those derived from it within the same assembly).
            // This is a C++ feature not supported in other languages.
            if(api.IsFamilyAndAssembly)
                return "family and assembly";

            if(api.IsPrivate)
                return "private";

            throw new InvalidOperationException("Unknown access level for " + api.FullName);
        }

        /// <summary>
        /// Check for any exposed types in a namespace
        /// </summary>
        /// <param name="space">The namespace to check</param>
        /// <returns>True if the namespace contains exposed types, false if it does not, or null if there are no
        /// exposed types but the API filter is empty.</returns>
        private bool? NamespaceContainsExposedTypes(Namespace space)
        {
            foreach(var type in space.Types)
                if(this.IsExposedType(type))
                    return true;

            return (apiFilter.NamespaceFilterCount == 0) ? (bool?)null : false;
        }

        /// <summary>
        /// Check for any exposed members in any of the types
        /// </summary>
        /// <param name="space">The namespace to check</param>
        /// <returns>True if the type has an exposed member filter and it is matched are set to true.</returns>
        /// <remarks>This is used to determine if the namespace should be visited.  If the namespace and all
        /// types are set to false for exposed, we still want to visit them if any members are visible.</remarks>
        private bool NamespaceContainsExposedMembers(Namespace space)
        {
            foreach(var type in space.Types)
                if(this.HasExposedMembers(type))
                    return true;

            return false;
        }

        /// <summary>
        /// Check to see if an expression contains exposed types
        /// </summary>
        /// <param name="expression">The expression to check</param>
        /// <returns>True if the expression is exposed, false if not</returns>
        private bool IsExposedExpression(Expression expression)
        {
            if(expression.NodeType == NodeType.Literal)
            {
                Literal literal = (Literal)expression;
                TypeNode type = literal.Type;

                if(!this.IsExposedType(type))
                    return false;

                if(type.FullName == "System.Type")
                {
                    // If the value is itself a type, we need to test whether that type is visible
                    TypeNode value = literal.Value as TypeNode;

                    // !EFW - Bug Fix.  Don't exclude the type if it has exposed members.  That effectively
                    // exposes the type as well.
                    if(value != null && !this.IsExposedType(value) && !this.HasExposedMembers(value))
                        return false;
                }

                return true;
            }
            else
                if(expression.NodeType == NodeType.NamedArgument)
                {
                    NamedArgument assignment = (NamedArgument)expression;
                    return this.IsExposedExpression(assignment.Value);
                }

            throw new InvalidOperationException("Encountered unrecognized expression");
        }

        /// <summary>
        /// This is used to check a type to see if it is an embedded interop type
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if it is an embedded interop type, false if not.  To be true, the type must have a
        /// <see cref="System.Runtime.CompilerServices.CompilerGeneratedAttribute"/> and a
        /// <see cref="System.Runtime.InteropServices.TypeIdentifierAttribute"/>.</returns>
        private static bool IsEmbeddedInteropType(TypeNode type)
        {
            bool compilerGeneratedAttribute = false, typeIdentifierAttribute = false;

            foreach(var attribute in type.Attributes)
            {
                if(attribute.Type.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute")
                    compilerGeneratedAttribute = true;

                if(attribute.Type.FullName == "System.Runtime.InteropServices.TypeIdentifierAttribute")
                    typeIdentifierAttribute = true;
            }

            return (compilerGeneratedAttribute && typeIdentifierAttribute);
        }
        #endregion
    }
}
