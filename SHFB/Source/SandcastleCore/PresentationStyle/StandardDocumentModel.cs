//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : StandardDocumentModel.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/02/2025
// Note    : Copyright 2021-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to modify the reflection information file by adding elements needed for the
// standard documentation model.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/05/2021  EFW  Created the code
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
    /// This is used to add the standard document model elements to the reflection information file
    /// </summary>
    /// <remarks>The generator has the following behaviors:
    /// 
    /// <list type="bullet">
    ///     <item>If a root namespace container ID is specified, a root namespace container API entry (R:) is
    /// added with a list of the namespaces.  If not specified, it is omitted.</item>
    ///     <item>A <c>topicdata</c> element is added to each API entry to identify the topic group.  For
    /// enumeration members (the fields), a <c>notopic</c> attribute is added to indicate that it should not
    /// get a separate topic.  For explicitly implemented members, an <c>eiiname</c> attribute is added to
    /// identify the explicitly implemented type and member name.</item>
    ///     <item><c>library</c> elements are updated with the assembly version and, for type members, a
    /// <c>NoAptca</c> element is added if the assembly does not allow partially trusted callers.</item>
    ///     <item>Elements for overloaded methods are grouped into Overload elements within their type's API
    /// entry.  Extension methods are not grouped if overloaded but do get an <c>overload</c> attribute.</item>
    ///     <item>The API entries for overloaded members have an <c>overload</c> attribute added to their
    /// <c>memberdata</c> element to identify the overload topic ID.</item>
    ///     <item>For all types except enumerations, member list topic API entries are created for methods,
    /// operators, properties, events, fields, attached properties, attached events, and overloaded members.</item>
    /// </list>
    ///</remarks>
    public class StandardDocumentModel : IApplyDocumentModel
    {
        #region Private data members
        //=====================================================================

        private readonly Dictionary<string, ApiMember> apiMembers = [];
        private readonly Dictionary<string, ApiAssemblyProperties> assemblies = [];

        #endregion

        #region IApplyDocumentModel implementation
        //=====================================================================

        /// <inheritdoc />
        public string RootNamespaceContainerId { get; set; }

        /// <inheritdoc />
        public void ApplyDocumentModel(string reflectionDataFile, string docModelReflectionDataFile)
        {
            // Don't use simplified using statements here as we're reading the reflection data file twice.

            // Load the information needed to apply the document model
            using(XmlReader reader = XmlReader.Create(reflectionDataFile, new XmlReaderSettings {
              IgnoreWhitespace = true, CloseInput = true }))
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
                                    var node = (XElement)XNode.ReadFrom(reader);
                                    var apiMember = new ApiMember(node, null) { Node = node };

                                    apiMembers[apiMember.MemberId] = apiMember;
                                    break;

                                case "assembly":
                                    var assembly = new ApiAssemblyProperties((XElement)XNode.ReadFrom(reader));

                                    assemblies[assembly.AssemblyName] = assembly;
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

            // Clone the file and add the document model elements
            using(XmlReader reader = XmlReader.Create(reflectionDataFile, new XmlReaderSettings {
              IgnoreWhitespace = true, CloseInput = true }))
            using(XmlWriter writer = XmlWriter.Create(docModelReflectionDataFile, new XmlWriterSettings {
              Indent = true, CloseOutput = true }))
            {
                writer.WriteStartDocument();
                reader.Read();

                while(!reader.EOF)
                {
                    switch(reader.NodeType)
                    {
                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.EndElement:
                            if(reader.Name == "apis" && !String.IsNullOrWhiteSpace(this.RootNamespaceContainerId))
                                this.AddRootNamespaceContainer(writer);

                            reader.Read();
                            break;

                        case XmlNodeType.Element:
                            switch(reader.Name)
                            {
                                case "apis":
                                case "reflection":
                                    writer.WriteStartElement(reader.Name);
                                    reader.Read();
                                    break;

                                case "api":
                                    string id = reader.GetAttribute("id");

                                    // Discard the file copy and use the one we loaded above
                                    reader.ReadToNextSibling("api");

                                    var apiMember = apiMembers[id];

                                    if(apiMember.ApiGroup == ApiMemberGroup.Type)
                                        this.UpdateTypeApiNode(apiMember);
                                    else
                                        this.UpdateGeneralApiNode(apiMember);

                                    apiMember.Node.WriteTo(writer);

                                    if(apiMember.ApiGroup == ApiMemberGroup.Type && 
                                      apiMember.ApiSubgroup != ApiMemberGroup.Enumeration &&
                                      apiMember.Node.Element("elements") != null)
                                    {
                                        this.AddMemberListApiElement(writer, ApiMemberGroup.Methods, apiMember);
                                        this.AddMemberListApiElement(writer, ApiMemberGroup.Operators, apiMember);
                                        this.AddMemberListApiElement(writer, ApiMemberGroup.Properties, apiMember);
                                        this.AddMemberListApiElement(writer, ApiMemberGroup.Events, apiMember);
                                        this.AddMemberListApiElement(writer, ApiMemberGroup.Fields, apiMember);
                                        this.AddMemberListApiElement(writer, ApiMemberGroup.AttachedProperties, apiMember);
                                        this.AddMemberListApiElement(writer, ApiMemberGroup.AttachedEvents, apiMember);
                                        this.AddOverloadListApiElements(writer, apiMember);
                                    }
                                    break;

                                default:
                                    writer.WriteNode(reader.ReadSubtree(), true);
                                    break;
                            }
                            break;

                        default:
                            reader.Read();
                            break;
                    }
                }

                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// This adds a root namespace container element
        /// </summary>
        /// <param name="writer">The XML writer to which the element is added</param>
        private void AddRootNamespaceContainer(XmlWriter writer)
        {
            writer.WriteStartElement("api");
            writer.WriteAttributeString("id", "R:" + this.RootNamespaceContainerId);
            
            writer.WriteStartElement("topicdata");
            writer.WriteAttributeString("group", "root");
            writer.WriteEndElement();

            writer.WriteStartElement("elements");

            foreach(var ns in apiMembers.Values.Where(m => m.ApiGroup == ApiMemberGroup.Namespace))
            {
                writer.WriteStartElement("element");
                writer.WriteAttributeString("api", ns.MemberId);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();   // elements
            writer.WriteEndElement();   // api
        }

        /// <summary>
        /// This adds documentation model elements to a type node
        /// </summary>
        /// <param name="typeNode">The type node to update</param>
        private void UpdateTypeApiNode(ApiMember typeNode)
        {
            typeNode.Node.AddFirst(new XElement("topicdata",
                new XAttribute("group", ApiMemberGroup.Api.ToString().ToLowerInvariant())));

            var containers = typeNode.Node.Element("containers");

            if(containers != null)
                foreach(var l in containers.Elements("library"))
                    this.AddLibraryAssemblyData(l, false);

            if(typeNode.ApiSubgroup != ApiMemberGroup.Enumeration && typeNode.Node.Element("elements") != null)
            {
                var memberList = new List<ApiMember>();
                var elements = typeNode.Node.Element("elements");

                foreach(var element in elements.Elements("element"))
                {
                    // Members with an apidata element are extension methods
                    if(element.Element("apidata") == null)
                    {
                        // Odd case: Sometimes a member makes it to the element list but doesn't appear in the
                        // file as an entry.  These will be omitted.
                        if(apiMembers.TryGetValue(element.Attribute("api").Value, out ApiMember m))
                            memberList.Add(m);
                        /*
                        // TODO: These are inherited members from types marked as EditorBrowsableState.Never
                        // such as a couple of designer classes in the .NET Framework.  May look into fixing this
                        // later but it's a minor issue resolved by omitting them here as they shouldn't appear
                        // anyway.
                        else
                            System.Diagnostics.Debug.WriteLine("No API entry for " + element.Attribute("api").Value);
                        */
                    }
                    else
                        memberList.Add(new ApiMember(element, null) { Node = element });
                }

                // Update the element list with overloads topics
                var newElementList = new List<XElement>();

                foreach(var el in UpdateMemberListElements(typeNode, memberList))
                    newElementList.Add(el);

                elements.RemoveAll();

                foreach(var el in newElementList)
                    elements.Add(el);
            }
        }

        /// <summary>
        /// This adds documentation model elements to a general node
        /// </summary>
        /// <param name="memberNode">The member node to update</param>
        private void UpdateGeneralApiNode(ApiMember memberNode)
        {
            var containers = memberNode.Node.Element("containers");
            var type = containers?.Element("type");
            var topicData = new XElement("topicdata",
                new XAttribute("group", ApiMemberGroup.Api.ToString().ToLowerInvariant()),
                (type != null && apiMembers[type.Attribute("api").Value].ApiSubgroup == ApiMemberGroup.Enumeration) ?
                    new XAttribute("notopic", String.Empty) : null);
            string implementedApiType = null;

            // For EII members, add the name of the type to which the implemented member belongs to keep them
            // distinct if the name appears in multiple explicitly implemented types.
            if(memberNode.IsExplicitlyImplemented)
            {
                implementedApiType = memberNode.ImplementedType;

                int pos = implementedApiType.LastIndexOf('.');

                if(pos != -1)
                    topicData.Add(new XAttribute("eiiName", implementedApiType.Substring(pos + 1) + "." + memberNode.Name));
                else
                    topicData.Add(new XAttribute("eiiName", implementedApiType.Substring(2) + "." + memberNode.Name));
            }

            memberNode.Node.AddFirst(topicData);

            if(containers != null)
                foreach(var l in containers.Elements("library"))
                    this.AddLibraryAssemblyData(l, true);

            var memberData = memberNode.Node.Element("memberdata");

            if(memberData == null || memberNode.ApiSubgroup == ApiMemberGroup.Field)
                return;

            // Add an overload ID to the memberdata element if needed.
            var typeNode = apiMembers[type.Attribute("api").Value];
            string overloadId = DetermineOverloadId(typeNode, memberNode);

            if(typeNode.Node.Element("elements").Elements("element").Any(
              el => el.Attribute("api").Value == overloadId))
            {
                memberData.Add(new XAttribute("overload", overloadId));
            }
        }

        /// <summary>
        /// Add a member list topic of the specified type for the given type node
        /// </summary>
        /// <param name="writer">The XML writer to which the element is added</param>
        /// <param name="listType">The list type to add</param>
        /// <param name="typeNode">The type node to update</param>
        private void AddMemberListApiElement(XmlWriter writer, ApiMemberGroup listType, ApiMember typeNode)
        {
            ApiMemberGroup subgroup = ApiMemberGroup.None, subsubgroup = ApiMemberGroup.None;

            switch(listType)
            {
                case ApiMemberGroup.Methods:
                    subgroup = ApiMemberGroup.Method;
                    break;

                case ApiMemberGroup.Operators:
                    subsubgroup = ApiMemberGroup.Operator;
                    break;

                case ApiMemberGroup.Properties:
                    subgroup = ApiMemberGroup.Property;
                    break;

                case ApiMemberGroup.Events:
                    subgroup = ApiMemberGroup.Event;
                    break;

                case ApiMemberGroup.Fields:
                    subgroup = ApiMemberGroup.Field;
                    break;

                case ApiMemberGroup.AttachedProperties:
                    subsubgroup = ApiMemberGroup.AttachedProperty;
                    break;

                case ApiMemberGroup.AttachedEvents:
                    subsubgroup = ApiMemberGroup.AttachedEvent;
                    break;

                default:
                    throw new InvalidOperationException("Unexpected list type encountered");
            }

            var memberList = new List<ApiMember>();
            ApiMember listElement, compareElement;

            // Get members matching the subgroup or sub-subgroup
            foreach(var element in typeNode.Node.Element("elements").Elements("element"))
            {
                // For overloads, use the first element so that we get the proper subgroup for comparison
                if(element.Attribute("api").Value.StartsWith("Overload:", StringComparison.Ordinal))
                {
                    listElement = new ApiMember(element, null) { Node = element };
                    var firstEl = element.Element("element");

                    if(!apiMembers.TryGetValue(firstEl.Attribute("api").Value, out compareElement))
                    {
                        compareElement = new ApiMember(firstEl, null) { Node = firstEl };
                    }
                }
                else
                {
                    // Members with an apidata element are extension methods
                    if(element.Element("apidata") == null)
                        listElement = compareElement = apiMembers[element.Attribute("api").Value];
                    else
                        listElement = compareElement = new ApiMember(element, null) { Node = element };
                }

                if(subgroup == ApiMemberGroup.Method)
                {
                    // For methods we want everything but operators
                    if(compareElement.ApiSubgroup == subgroup && compareElement.ApiSubSubgroup != ApiMemberGroup.Operator)
                        memberList.Add(listElement);
                }
                else
                {
                    if((subgroup == ApiMemberGroup.None && compareElement.ApiSubSubgroup == subsubgroup) ||
                      (subgroup == compareElement.ApiSubgroup && subsubgroup == compareElement.ApiSubSubgroup))
                    {
                        memberList.Add(listElement);
                    }
                }
            }

            if(memberList.Count == 0)
                return;

            XElement apidata = typeNode.Node.Element("apidata"), typedata = typeNode.Node.Element("typedata"),
                templates = typeNode.Node.Element("templates"), containers = new(typeNode.Node.Element("containers"));

            // Add or update the type element.  A type element will already exist for nested types.  In such
            // cases, we just replace it for the list topic.
            var t = containers.Element("type");

            if(t != null)
            {
                t.RemoveAll();
                t.Add(new XAttribute("api", typeNode.MemberId));
            }
            else
                containers.Add(new XElement("type", new XAttribute("api", typeNode.MemberId)));

            // Create the member list node and add it to the file
            var apiList = new XElement("api",
                new XAttribute("id", listType.ToString() + "." + typeNode.MemberId),
                new XElement("topicdata",
                    new XAttribute("name", typeNode.Name),
                    new XAttribute("group", ApiMemberGroup.List.ToString().ToLowerInvariant()),
                    new XAttribute("subgroup", listType),
                    (subsubgroup != ApiMemberGroup.None) ? new XAttribute("subsubgroup", listType) : null,
                    new XAttribute("typeTopicId", typeNode.MemberId)),
                apidata, typedata, templates,
                new XElement("elements",
                    memberList.Select(m => UpdateElementNode(typeNode, m))),
                containers);

            apiList.WriteTo(writer);
        }

        /// <summary>
        /// Add overload list topics for each overloaded member in the type
        /// </summary>
        /// <param name="writer">The XML writer to which the element is added</param>
        /// <param name="typeNode">The type node to update</param>
        private void AddOverloadListApiElements(XmlWriter writer, ApiMember typeNode)
        {
            XElement containers = null;
            var memberList = new List<ApiMember>();

            // Get members matching the subgroup or sub-subgroup
            foreach(var element in typeNode.Node.Element("elements").Elements("element"))
            {
                // Members with an apidata element are extension methods
                if(element.Attribute("api").Value.StartsWith("Overload:", StringComparison.Ordinal))
                {
                    var ol = new ApiMember(element, null) { Node = element };

                    // Only generate an overload topic if there is at least one member in the containing type.
                    // If all overloads are inherited, ignore it.
                    var firstDeclaredMember = ol.Node.Elements("element").FirstOrDefault(
                        el => el.Attribute("api").Value.Substring(2).StartsWith(
                      typeNode.MemberIdWithoutPrefix + ".", StringComparison.Ordinal));

                    if(firstDeclaredMember != null)
                    {
                        // Use the container from the first declared member for the overloads topic
                        if(containers == null)
                        {
                            var fm = apiMembers[firstDeclaredMember.Attribute("api").Value];
                            containers = new XElement(fm.Node.Element("containers"));

                            foreach(var l in containers.Elements("library"))
                                this.AddLibraryAssemblyData(l, false);
                        }

                        memberList.Add(ol);
                    }
                }
            }

            if(memberList.Count == 0)
                return;

            // Create the member list node and add it to the file
            foreach(var m in memberList.OrderBy(m => m.MemberId))
            {
                var firstEl = m.Node.Element("element");

                if(!apiMembers.TryGetValue(firstEl.Attribute("api").Value, out ApiMember firstMember))
                    firstMember = new ApiMember(firstEl, null) { Node = firstEl };

                var apidata = firstMember.Node.Element("apidata");

                var apiList = new XElement("api",
                    new XAttribute("id", m.MemberId),
                    new XElement("topicdata",
                        new XAttribute("name", firstMember.Name),
                        new XAttribute("group", ApiMemberGroup.List.ToString().ToLowerInvariant()),
                        new XAttribute("subgroup", ApiMemberGroup.Overload.ToString().ToLowerInvariant()),
                        new XAttribute("memberSubgroup", firstMember.ApiSubgroup.ToString().ToLowerInvariant()),
                        new XAttribute("pseudo", "true")),
                    apidata,
                    new XElement("elements",
                        m.Node.Elements("element").Select(el => new XElement(el))),
                    containers);

                apiList.WriteTo(writer);
            }
        }

        /// <summary>
        /// Update the member list elements with overload set entries
        /// </summary>
        /// <returns>An enumerable list of <c>element</c> elements for the member list</returns>
        private static IEnumerable<XElement> UpdateMemberListElements(ApiMember typeNode, IEnumerable<ApiMember> memberList)
        {
            string declaredPrefix = typeNode.MemberIdWithoutPrefix + ".";

            // Group EII members separately from non-EII members.  Extension methods are also separated.
            List<ApiMember> eiiMembers = [], nonEiiMembers = [], extensionMethods = [];

            foreach(var m in memberList)
            {
                if(m.IsExplicitlyImplemented)
                    eiiMembers.Add(m);
                else
                {
                    if(m.ApiSubSubgroup == ApiMemberGroup.Extension)
                        extensionMethods.Add(m);
                    else
                        nonEiiMembers.Add(m);
                }
            }

            // Extension methods are never grouped even when overloaded.  We just mark them with an attribute.
            var groupedMembers = eiiMembers.GroupBy(m => m.ImplementedType + "/" + m.Name + "/" +
                m.ApiSubgroup.ToString() + "/" + m.ApiSubSubgroup).Concat(
                nonEiiMembers.GroupBy(m => m.Name + "/" + m.ApiSubgroup.ToString() + "/" + m.ApiSubSubgroup)).Concat(
                extensionMethods.GroupBy(m => m.Name)).OrderBy(
                    m => m.First().ApiSubSubgroup == ApiMemberGroup.Extension ? 0 : 1).ThenBy(
                    m => m.First().MemberId).ToList();

            foreach(var g in groupedMembers)
            {
                var firstMember = g.First();

                if(g.Count() == 1)
                    yield return UpdateElementNode(typeNode, firstMember);
                else
                {
                    // Extension methods just get an attribute
                    if(firstMember.ApiSubSubgroup == ApiMemberGroup.Extension)
                    {
                        foreach(var m in g.OrderBy(m => m.ParameterCount).ThenBy(m => m.FirstParameterTypeName))
                        {
                            var elementNode = UpdateElementNode(typeNode, m);

                            elementNode.Add(new XAttribute("overload", "true"));

                            yield return elementNode;
                        }
                    }
                    else
                    {
                        var overloadElement = new XElement("element");

                        overloadElement.Add(new XAttribute("api", DetermineOverloadId(typeNode, firstMember)));

                        foreach(var m in g.OrderBy(m => m.ParameterCount).ThenBy(m => m.FirstParameterTypeName).ThenBy(
                          m => m.MemberIdWithoutPrefix))
                        {
                            overloadElement.Add(UpdateElementNode(typeNode, m));
                        }

                        yield return overloadElement;
                    }
                }
            }
        }

        /// <summary>
        /// This is used to determine the ID of an overloads member entry
        /// </summary>
        /// <param name="typeNode">The type node to use</param>
        /// <param name="member">The member to use</param>
        /// <returns>The ID for the overloads member entry</returns>
        private static string DetermineOverloadId(ApiMember typeNode, ApiMember member)
        {
            string overloadId;

            if(member.IsExplicitlyImplemented)
            {
                string memberId = member.MemberIdWithoutParameters.Substring(2);
                overloadId = typeNode.MemberIdWithoutPrefix + "." + memberId.Substring(memberId.LastIndexOf('.') + 1);

                int pos = overloadId.LastIndexOf("``", StringComparison.Ordinal);

                if(pos != -1)
                    overloadId = overloadId.Substring(0, pos);
            }
            else
            {
                overloadId = typeNode.MemberIdWithoutPrefix;

                if(member.ApiSubgroup == ApiMemberGroup.Constructor)
                    overloadId += ".#ctor";
                else
                {
                    if(member.ApiSubSubgroup == ApiMemberGroup.Operator)
                        overloadId += ".op_" + member.Name;
                    else
                        overloadId += "." + member.Name;
                }
            }

            return "Overload:" + overloadId;
        }

        /// <summary>
        /// Update an element node
        /// </summary>
        /// <param name="typeNode">The type node</param>
        /// <param name="member">The member for which to update the element node</param>
        /// <returns>The node itself if it is an <c>element</c> element or the original <c>element</c> node
        /// from the containing type if not.</returns>
        private static XElement UpdateElementNode(ApiMember typeNode, ApiMember member)
        {
            // If the node is an element, return it
            if(member.Node.Name == "element")
                return member.Node;

            // Otherwise, look up and return the original element node in the type's element list
            var el = typeNode.Node.Element("elements").Elements("element").FirstOrDefault(
                mel => mel.Attribute("api").Value == member.MemberId);

            return el;
        }

        /// <summary>
        /// Add library assembly information to a library element
        /// </summary>
        /// <param name="library">The library element to which the information is added</param>
        /// <param name="addNoAptca">True to add a <c>noAptca</c> element if partially trusted callers are not
        /// allowed, false to skip it.</param>
        private void AddLibraryAssemblyData(XElement library, bool addNoAptca)
        {
            if(assemblies.TryGetValue(library.Attribute("assembly").Value, out ApiAssemblyProperties assembly))
            {
                library.Add(new XElement("assemblydata",
                    new XAttribute("version", assembly.Version)));

                if(addNoAptca && !assembly.AllowsPartiallyTrustedCallers)
                    library.Add(new XElement("noAptca"));
            }
        }
        #endregion
    }
}
