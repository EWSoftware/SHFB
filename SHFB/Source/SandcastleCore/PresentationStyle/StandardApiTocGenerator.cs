//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : StandardApiTocGenerator.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/27/2022
// Note    : Copyright 2021-2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to generate a standard table of contents for API content
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/31/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using Sandcastle.Core.Reflection;

namespace Sandcastle.Core.PresentationStyle
{
    /// <summary>
    /// This is used to generate a standard table of contents for API content
    /// </summary>
    /// <remarks>The generator has the following behaviors:
    ///
    /// <list type="bullet">
    ///     <item>The reflection information is assumed to contain list entries for properties, methods, events,
    /// etc.  These are used to generate corresponding list topic entries in the table of contents.</item>
    ///     <item>A root namespace container (R:) is supported if present.</item>
    ///     <item>Namespace grouping is supported if grouping elements (G:) are present.</item>
    ///     <item>Overloads are assumed to have a container topic and each overload has its own subtopic below
    /// it.</item>
    ///     <item>Namespaces, types, and members are sorted in ascending order by name.  Explicit interface
    /// implementations are list ahead of the type's other members sorted by member name.  Overloads are sorted
    /// by parameter count and then by the type name of the first parameter.</item>
    ///     <item>The default order of the member list topics is properties, methods, events, operators, fields,
    /// attached properties, and finally attached events.</item>
    /// </list>
    /// </remarks>
    public class StandardApiTocGenerator : IApiTocGenerator
    {
        #region Private data members
        //=====================================================================

        private readonly Dictionary<string, ApiMember> apiMembers;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public StandardApiTocGenerator()
        {
            apiMembers = new Dictionary<string, ApiMember>();

            this.ListTopicOrder = new ApiMemberGroup[] { ApiMemberGroup.Properties, ApiMemberGroup.Methods,
                ApiMemberGroup.Events, ApiMemberGroup.Operators, ApiMemberGroup.Fields,
                ApiMemberGroup.AttachedProperties, ApiMemberGroup.AttachedEvents };
        }
        #endregion

        #region IApiTocGenerator implementation
        //=====================================================================

        /// <inheritdoc />
        public IEnumerable<ApiMemberGroup> ListTopicOrder { get; set; }

        /// <inheritdoc />
        public void GenerateApiTocFile(string reflectionDataFile, string tocFile)
        {
            ApiMember root = null, rootGroup = null;
            var namespaces = new List<ApiMember>();

            // Load the API member information to determine how the TOC should be generated
            using(XmlReader reader = XmlReader.Create(reflectionDataFile, new XmlReaderSettings { IgnoreWhitespace = true }))
            {
                reader.Read();

                while(!reader.EOF)
                {
                    switch(reader.NodeType)
                    {
                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.EndElement:
                            reader.Read();
                            break;

                        case XmlNodeType.Element:
                            switch(reader.Name)
                            {
                                case "api":
                                    var apiMember = new ApiMember((XElement)XNode.ReadFrom(reader), null);
#if DEBUG
                                    if(apiMember.ApiGroup == ApiMemberGroup.Unknown || apiMember.ApiSubgroup == ApiMemberGroup.Unknown ||
                                      apiMember.TopicGroup == ApiMemberGroup.Unknown || apiMember.TopicSubgroup == ApiMemberGroup.Unknown)
                                    {
                                        throw new InvalidOperationException("Unknown group value: " + apiMember.MemberId +
                                            $" - API Data: {apiMember.ApiGroup}/{apiMember.ApiSubgroup} Topic Data: {apiMember.TopicGroup} {apiMember.TopicSubgroup}");
                                    }
#endif
                                    if(apiMember.TopicGroup == ApiMemberGroup.Root)
                                    {
                                        if(root != null)
                                            throw new InvalidOperationException("Unexpected duplicate root container entry");

                                        root = apiMember;
                                    }
                                    else
                                    {
                                        if(apiMember.TopicGroup == ApiMemberGroup.RootGroup)
                                        {
                                            if(rootGroup != null)
                                                throw new InvalidOperationException("Unexpected duplicate root group container entry");

                                            rootGroup = apiMember;
                                        }
                                        else
                                        {
                                            if(apiMember.ApiGroup == ApiMemberGroup.Namespace)
                                                namespaces.Add(apiMember);
                                        }
                                    }

                                    apiMembers[apiMember.MemberId] = apiMember;
                                    break;

                                default:
                                    reader.Read();
                                    break;
                            }
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }
            }

            // Generate the TOC
            using(XmlWriter writer = XmlWriter.Create(tocFile, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("topics");

                if(root != null)
                {
                    // "R:" root namespace container.  Create a root entry and an entry for each namespace and
                    // namespace group and their children.
                    writer.WriteStartElement("topic");
                    writer.WriteAttributeString("id", root.MemberId);
                    writer.WriteAttributeString("file", root.TopicFilename);

                    foreach(string child in root.ChildElements)
                        AddNamespaceOrTypeTopic(writer, child);

                    writer.WriteEndElement();
                }
                else if(rootGroup != null)
                {
                    // No root namespace container but has a "G:" root group.  There is no topic generated for
                    // the root group node, just its children.
                    foreach(string child in rootGroup.ChildElements)
                        AddNamespaceOrTypeTopic(writer, child);
                }
                else
                {
                    // No root namespace container and no grouped namespaces.  List the namespaces at the root
                    // level, each containing their children.
                    foreach(var ns in namespaces.OrderBy(n => n.MemberIdWithoutPrefix))
                        AddNamespaceOrTypeTopic(writer, ns.MemberId);
                }

                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// Add a namespace, namespace group, or type topic for the given member ID.  Topics are added
        /// recursively for each of its children and their children.
        /// </summary>
        /// <param name="writer">The XML writer to which the output is written</param>
        /// <param name="memberId">The member ID to add</param>
        private void AddNamespaceOrTypeTopic(XmlWriter writer, string memberId)
        {
            var member = apiMembers[memberId];

            switch(member.ApiGroup)
            {
                case ApiMemberGroup.Namespace:
                case ApiMemberGroup.NamespaceGroup:
                    writer.WriteStartElement("topic");
                    writer.WriteAttributeString("id", member.MemberId);
                    writer.WriteAttributeString("file", member.TopicFilename);

                    foreach(string child in member.ChildElements)
                        AddNamespaceOrTypeTopic(writer, child);

                    writer.WriteEndElement();
                    break;

                case ApiMemberGroup.Type:
                    writer.WriteStartElement("topic");
                    writer.WriteAttributeString("id", member.MemberId);
                    writer.WriteAttributeString("file", member.TopicFilename);

                    if(member.ApiSubgroup == ApiMemberGroup.Class || member.ApiSubgroup == ApiMemberGroup.Structure ||
                      member.ApiSubgroup == ApiMemberGroup.Interface)
                    {
                        AddMemberListTopics(writer, member);
                    }

                    writer.WriteEndElement();
                    break;

                default:
                    throw new InvalidOperationException("Unexpected member group");
            }
        }

        /// <summary>
        /// Add member list topics for a type (constructors, properties, methods, events, etc.).  The members are
        /// added as children of each list topic.
        /// </summary>
        /// <param name="writer">The XML writer to which the output is written</param>
        /// <param name="apiType">The type member for which to add member list topics</param>
        private void AddMemberListTopics(XmlWriter writer, ApiMember apiType)
        {
            string declaringTypePrefix = apiType.MemberIdWithoutPrefix + ".";

            // Only include direct members of the given type.  Ignore inherited members.
            var constructors = apiType.ChildElements.Select(id => apiMembers.ContainsKey(id) ? apiMembers[id] : null).Where(
                m => m != null && m.MemberIdWithoutPrefix.StartsWith(declaringTypePrefix, StringComparison.Ordinal) &&
                     m.ApiSubgroup == ApiMemberGroup.Constructor);

            // Constructors don't have a containing list topic so we need to get them separately
            foreach(var m in constructors)
                AddMember(writer, m, declaringTypePrefix);

            // Add lists for properties, methods, events, etc.  Some categories may not appear for a type.
            foreach(ApiMemberGroup listType in this.ListTopicOrder)
                if(apiMembers.TryGetValue(listType.ToString() + "." + apiType.MemberId, out ApiMember list))
                    AddMemberListTree(writer, list, declaringTypePrefix);
        }

        /// <summary>
        /// Add a member list topic and its children
        /// </summary>
        /// <param name="writer">The XML writer to which the output is written</param>
        /// <param name="list">The member list topic to output</param>
        /// <param name="declaringTypePrefix">The declaring type's prefix used to filter members</param>
        private void AddMemberListTree(XmlWriter writer, ApiMember list, string declaringTypePrefix)
        {
            writer.WriteStartElement("topic");
            writer.WriteAttributeString("id", list.MemberId);
            writer.WriteAttributeString("file", list.TopicFilename);

            // Add the child elements of the list.  Only include direct members of the given type.  Ignore
            // inherited members.  Sort by name (explicit interface implementation name if present or
            // the member name if not.
            var childMembers = list.ChildElements.Select(id => apiMembers.ContainsKey(id) ? apiMembers[id] : null).Where(
                m => m != null && m.MemberIdWithoutPrefix.StartsWith(declaringTypePrefix, StringComparison.Ordinal)).OrderBy(
                m => m.TopicEiiName ?? m.Name);

            foreach(var m in childMembers)
                AddMember(writer, m, declaringTypePrefix);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Add a table of contents entry for a member
        /// </summary>
        /// <param name="writer">The XML writer to which the output is written</param>
        /// <param name="member">The member for which to add TOC entry</param>
        /// <param name="declaringTypePrefix">The declaring type's prefix used to filter members</param>
        private void AddMember(XmlWriter writer, ApiMember member, string declaringTypePrefix)
        {
            writer.WriteStartElement("topic");
            writer.WriteAttributeString("id", member.MemberId);
            writer.WriteAttributeString("file", member.TopicFilename);

            // Add child elements if any (members for an overload for example).  Only include direct members of
            // the given type.  Ignore inherited members.  Sort by parameter count and then by the type name
            // of the first parameter.
            var childMembers = member.ChildElements.Select(id => apiMembers.ContainsKey(id) ? apiMembers[id] : null).Where(
                m => m != null && m.MemberIdWithoutPrefix.StartsWith(declaringTypePrefix, StringComparison.Ordinal)).OrderBy(
                m => m.ParameterCount).ThenBy(m => m.FirstParameterTypeName);

            foreach(var m in childMembers)
                AddMember(writer, m, declaringTypePrefix);

            writer.WriteEndElement();
        }
        #endregion
    }
}
