// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 11/23/2013 - EFW - Updated based on changes to ListTemplate.cs.  Cleaned up the code and removed unused
// members.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This add-in is used to add information to the reflection data about XAML attached properties and
    /// attached events.
    /// </summary>
    public class XamlAttachedMembersAddIn : MRefBuilderAddIn
    {
        #region Private data members
        //=====================================================================

        private Dictionary<Object, Field> attachedMembers;
        private ManagedReflectionWriter mrw;

        private string dependencyPropertyTypeName;
        private string dependencyPropertySuffix;
        private string routedEventTypeName;
        private string routedEventSuffix;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="writer">The API visitor and reflection data writer</param>
        /// <param name="configuration">This add-in has no configurable options</param>
        public XamlAttachedMembersAddIn(ManagedReflectionWriter writer, XPathNavigator configuration) :
          base(writer, configuration)
        {
            attachedMembers = new Dictionary<Object, Field>();

            // Keep track of the writer
            mrw = writer;

            // Register processors as callbacks
            writer.RegisterStartTagCallback("apis", AddAttachedMembers);
            writer.RegisterStartTagCallback("apidata", WriteAttachedMember);
            writer.RegisterEndTagCallback("api", WriteAttachmentData);

            // Determine the type names and suffixes of our relevant fields
            dependencyPropertyTypeName = 
                GetConfigurationValue(configuration, "dependencyPropertyTypeName") ?? 
                "System.Windows.DependencyProperty";

            dependencyPropertySuffix =
                GetConfigurationValue(configuration, "dependencyPropertySuffix") ??
                "Property";

            routedEventTypeName =
                GetConfigurationValue(configuration, "routedEventTypeName") ??
                "System.Windows.RoutedEvent";

            routedEventSuffix =
                GetConfigurationValue(configuration, "routedEventSuffix") ??
                "Event";
        }
        #endregion

        #region Add-in callback methods
        //=====================================================================

        /// <summary>
        /// This finds all attached properties and events, adds information about them to the types, and tracks
        /// them for adding to the reflection data later in the other callbacks.
        /// </summary>
        /// <param name="writer">The reflection data XML writer</param>
        /// <param name="info">For this callback, the information object is a namespace list</param>
        private void AddAttachedMembers(XmlWriter writer, object info)
        {
            NamespaceList spaces = (NamespaceList)info;

            foreach(Namespace space in spaces)
            {
                TypeNodeList types = space.Types;

                foreach(TypeNode type in types)
                {
                    MemberList members = new MemberList(type.Members);

                    // Go through the members looking for fields signaling attached properties
                    foreach(Member member in members)
                    {
                        // We need a visible, static, field...
                        if(!member.IsStatic || !member.IsVisibleOutsideAssembly || member.NodeType != NodeType.Field)
                            continue;

                        Field field = (Field)member;

                        // ... of type dependency property ...
                        if(field.Type.FullName != dependencyPropertyTypeName)
                            continue;

                        // ... with a name ending in "Property".
                        string name = field.Name.Name;

                        if(!name.EndsWith(dependencyPropertySuffix, StringComparison.Ordinal))
                            continue;

                        name = name.Substring(0, name.Length - dependencyPropertySuffix.Length);

                        // Look for a getter and/or a setter
                        Method getter = null;

                        MemberList candidateGetters = type.GetMembersNamed(new Identifier("Get" + name));

                        foreach(var candidateGetter in candidateGetters)
                            if(candidateGetter.NodeType == NodeType.Method && candidateGetter.IsStatic &&
                              candidateGetter.IsVisibleOutsideAssembly)
                                getter = (Method)candidateGetter;

                        Method setter = null;

                        MemberList candidateSetters = type.GetMembersNamed(new Identifier("Set" + name));

                        foreach(var candidateSetter in candidateSetters)
                            if(candidateSetter.NodeType == NodeType.Method && candidateSetter.IsStatic &&
                              candidateSetter.IsVisibleOutsideAssembly)
                                setter = (Method)candidateSetter;

                        if(getter == null && setter == null)
                            continue;

                        // Make sure there isn't already such a property
                        Property existingProperty = type.GetProperty(new Identifier(name), new TypeNode[0]);

                        if(existingProperty != null && existingProperty.IsVisibleOutsideAssembly)
                            continue;

                        // Okay, this really is an indication of an attached property, so create one
                        Property attachedProperty = new Property(type, null, PropertyFlags.None, new Identifier(name), getter, setter);

                        // Attached properties have no parameters
                        attachedProperty.Parameters = ParameterList.Empty;

                        // Attached properties are instance properties
                        type.Members.Add(attachedProperty);

                        attachedMembers.Add(attachedProperty, field);
                    }

                    // Go through the members, looking for fields signaling attached events
                    foreach(Member member in members)
                    {
                        // Follow a similar approach as above but for an event
                        if(!member.IsStatic || !member.IsVisibleOutsideAssembly)
                            continue;

                        if(member.NodeType != NodeType.Field)
                            continue;

                        Field field = (Field)member;

                        if(field.Type.FullName != routedEventTypeName)
                            continue;

                        string name = field.Name.Name;

                        if(!name.EndsWith(routedEventSuffix, StringComparison.Ordinal))
                            continue;

                        name = name.Substring(0, name.Length - routedEventSuffix.Length);

                        Method adder = null;

                        MemberList candidateAdders = type.GetMembersNamed(new Identifier("Add" + name + "Handler"));

                        foreach(var candidateAdder in candidateAdders)
                            if(candidateAdder.NodeType == NodeType.Method && candidateAdder.IsStatic)
                                adder = (Method)candidateAdder;

                        Method remover = null;

                        MemberList candidateRemovers = type.GetMembersNamed(new Identifier("Remove" + name + "Handler"));

                        foreach(var candidateRemover in candidateRemovers)
                            if(candidateRemover.NodeType == NodeType.Method && candidateRemover.IsStatic)
                                remover = (Method)candidateRemover;

                        if(adder == null || remover == null)
                            continue;

                        // Make sure there isn't already such an event
                        Event existingEvent = type.GetEvent(new Identifier(name));

                        if(existingEvent != null && existingEvent.IsVisibleOutsideAssembly)
                            continue;

                        // Okay, this really is an indication of an attached event, so create one
                        TypeNode handler = adder.Parameters[1].Type;

                        Event attachedEvent = new Event(type, null, EventFlags.None, new Identifier(name), adder, null, remover, handler);
                        attachedEvent.HandlerFlags = adder.Flags;

                        type.Members.Add(attachedEvent);

                        attachedMembers.Add(attachedEvent, field);
                    }
                }
            }
        }

        /// <summary>
        /// This is used to add a <c>subsubgroup</c> attribute that identifies the added members as an attached
        /// property or event.
        /// </summary>
        /// <param name="writer">The reflection data XML writer</param>
        /// <param name="info">For this callback, this is the potential attached event or property member</param>
        private void WriteAttachedMember(XmlWriter writer, object info)
        {
            if(attachedMembers.ContainsKey(info))
                if(info is Property)
                    writer.WriteAttributeString("subsubgroup", "attachedProperty");
                else
                    if(info is Event)
                        writer.WriteAttributeString("subsubgroup", "attachedEvent");
        }

        /// <summary>
        /// This is used to add the attached property or event data to the added members
        /// </summary>
        /// <param name="writer">The reflection data XML writer</param>
        /// <param name="info">For this callback, this is the potential attached event or property member</param>
        private void WriteAttachmentData(XmlWriter writer, object info)
        {
            if(attachedMembers.ContainsKey(info))
            {
                Member attachedMember = (Member)info;

                if(attachedMember.NodeType == NodeType.Property)
                {
                    Property attachedProperty = (Property)attachedMember;

                    writer.WriteStartElement("attachedpropertydata");

                    string fieldName = attachedMember.Name + "Property";

                    Field field = attachedMember.DeclaringType.GetField(new Identifier(fieldName));

                    writer.WriteStartElement("field");
                    mrw.WriteMemberReference(field);
                    writer.WriteEndElement();

                    Method getter = attachedProperty.Getter;

                    if(getter != null)
                    {
                        writer.WriteStartElement("getter");
                        mrw.WriteMemberReference(getter);
                        writer.WriteEndElement();
                    }

                    Method setter = attachedProperty.Setter;

                    if(setter != null)
                    {
                        writer.WriteStartElement("setter");
                        mrw.WriteMemberReference(setter);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
                else
                    if(attachedMember.NodeType == NodeType.Event)
                    {
                        Event attachedEvent = (Event)attachedMember;

                        writer.WriteStartElement("attachedeventdata");

                        string fieldName = attachedMember.Name + "Event";

                        Field field = attachedMember.DeclaringType.GetField(new Identifier(fieldName));

                        writer.WriteStartElement("field");
                        mrw.WriteMemberReference(field);
                        writer.WriteEndElement();

                        Method adder = attachedEvent.HandlerAdder;

                        writer.WriteStartElement("adder");
                        mrw.WriteMemberReference(adder);
                        writer.WriteEndElement();

                        Method remover = attachedEvent.HandlerRemover;

                        writer.WriteStartElement("remover");
                        mrw.WriteMemberReference(remover);
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
            }
        }
        #endregion

        #region Helper methods
        private string GetConfigurationValue(XPathNavigator configuration, string name)
        {
            var node = configuration.SelectSingleNode(name);
            if (node == null)
            {
                return null;
            }

            return String.IsNullOrWhiteSpace(node.Value) ? null : node.Value;
        }
        #endregion
    }
}
