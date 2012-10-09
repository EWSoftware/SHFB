// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

using Microsoft.Ddue.Tools.Reflection;

namespace Microsoft.Ddue.Tools {

    // XAML add in

    public class XamlAttachedMembersAddIn : MRefBuilderAddIn {

        private Dictionary < Object, Field > attachedMembers = new Dictionary < Object, Field >();

        private Dictionary < TypeNode, Property > contentProperties = new Dictionary < TypeNode, Property >();

        private ManagedReflectionWriter mrw;

        public XamlAttachedMembersAddIn(ManagedReflectionWriter writer, XPathNavigator configuration) : base(writer, configuration) {
            // keep track of the writer
            mrw = writer;

            // register processors as callbacks
            writer.RegisterStartTagCallback("apis", new MRefBuilderCallback(AddAttachedMembers));
            writer.RegisterStartTagCallback("apidata", new MRefBuilderCallback(WriteAttachedMember));
            writer.RegisterStartTagCallback("typedata", new MRefBuilderCallback(WriteContentPropertyData));
            writer.RegisterEndTagCallback("api", new MRefBuilderCallback(WriteAttachmentData));
        }

        private void AddAttachedMembers(XmlWriter writer, object info) {
            NamespaceList spaces = (NamespaceList)info;
            foreach (Namespace space in spaces) {
                TypeNodeList types = space.Types;
                foreach (TypeNode type in types) {

                    MemberList members = type.Members;

                    // go through the members, looking for fields signaling attached properties
                    foreach (Member member in members) {

                        // we need a visible, static, field...
                        if (!member.IsStatic || !member.IsVisibleOutsideAssembly || member.NodeType != NodeType.Field) continue;
                        Field field = (Field)member;

                        // of type dependency property....
                        if (field.Type.FullName != "System.Windows.DependencyProperty") continue;

                        // with a name ending in "Property"...
                        string name = field.Name.Name;
                        if (!name.EndsWith("Property")) continue;
                        name = name.Substring(0, name.Length - 8);

                        // look for a getter and/or a setter

                        Method getter = null;
                        MemberList candidateGetters = type.GetMembersNamed(new Identifier("Get" + name));
                        for (int i = 0; i < candidateGetters.Count; i++) {
                            Member candidateGetter = candidateGetters[i];
                            if ((candidateGetter.NodeType == NodeType.Method) && candidateGetter.IsStatic && candidateGetter.IsVisibleOutsideAssembly) getter = (Method)candidateGetter;
                        }

                        Method setter = null;
                        MemberList candidateSetters = type.GetMembersNamed(new Identifier("Set" + name));
                        for (int i = 0; i < candidateSetters.Count; i++) {
                            Member candidateSetter = candidateSetters[i];
                            if ((candidateSetter.NodeType == NodeType.Method) && candidateSetter.IsStatic && candidateSetter.IsVisibleOutsideAssembly) setter = (Method)candidateSetter;
                        }

                        if ((getter == null) && (setter == null)) continue;

                        // make sure there isn't already such a property

                        Property existingProperty = type.GetProperty(new Identifier(name), new TypeNode[0]);
                        if (existingProperty != null && existingProperty.IsVisibleOutsideAssembly) continue;

                        // okay, this really is an indication of an attached property, so create one

                        Property attachedProperty = new Property(type, null, PropertyFlags.None, new Identifier(name), getter, setter);
                        // attached properties have no parameters
                        attachedProperty.Parameters = ParameterList.Empty;
                        // attached properties are instance properties
                        // somehow mark as attached?
                        type.Members.Add(attachedProperty);
                        attachedMembers.Add(attachedProperty, field);

                    }

                    // go through the members, looking for fields signaling attached events
                    foreach (Member member in members) {

                        if (!member.IsStatic || !member.IsVisibleOutsideAssembly) continue;

                        if (member.NodeType != NodeType.Field) continue;
                        Field field = (Field)member;

                        if (field.Type.FullName != "System.Windows.RoutedEvent") continue;

                        string name = field.Name.Name;
                        if (!name.EndsWith("Event")) continue;
                        name = name.Substring(0, name.Length - 5);

                        Method adder = null;
                        MemberList candidateAdders = type.GetMembersNamed(new Identifier("Add" + name + "Handler"));
                        for (int i = 0; i < candidateAdders.Count; i++) {
                            Member candidateAdder = candidateAdders[i];
                            if ((candidateAdder.NodeType == NodeType.Method) && candidateAdder.IsStatic) adder = (Method)candidateAdder;
                        }

                        Method remover = null;
                        MemberList candidateRemovers = type.GetMembersNamed(new Identifier("Remove" + name + "Handler"));
                        for (int i = 0; i < candidateRemovers.Count; i++) {
                            Member candidateRemover = candidateRemovers[i];
                            if ((candidateRemover.NodeType == NodeType.Method) && candidateRemover.IsStatic) remover = (Method)candidateRemover;
                        }

                        if ((adder == null) || (remover == null)) continue;

                        // make sure there isn't already such an event

                        Event existingEvent = type.GetEvent(new Identifier(name));
                        if (existingEvent != null && existingEvent.IsVisibleOutsideAssembly) continue;

                        // okay, this really is an indication of an attached event, so create one

                        TypeNode handler = adder.Parameters[1].Type;
                        Event attachedEvent = new Event(type, null, EventFlags.None, new Identifier(name), adder, null, remover, handler);
                        attachedEvent.HandlerFlags = adder.Flags;
                        // attached events are instance events
                        // mark as attached?

                        type.Members.Add(attachedEvent);
                        attachedMembers.Add(attachedEvent, field);

                    }

                }
            }

        }

        private Property GetContentProperty(TypeNode type) {
            return (null);
        }

        private void WriteAttachedMember(XmlWriter writer, object info) {

            if (attachedMembers.ContainsKey(info)) {
                if (info is Property) {
                    writer.WriteAttributeString("subsubgroup", "attachedProperty");
                } else if (info is Event) {
                    writer.WriteAttributeString("subsubgroup", "attachedEvent");
                }
            }

        }

        private void WriteAttachmentData(XmlWriter writer, object info) {

            if (attachedMembers.ContainsKey(info)) {

                Member attachedMember = (Member)info;
                if (attachedMember.NodeType == NodeType.Property) {

                    Property attachedProperty = (Property)attachedMember;

                    writer.WriteStartElement("attachedpropertydata");

                    string fieldName = attachedMember.Name + "Property";
                    Field field = attachedMember.DeclaringType.GetField(new Identifier(fieldName));
                    writer.WriteStartElement("field");
                    mrw.WriteMemberReference(field);
                    writer.WriteEndElement();

                    Method getter = attachedProperty.Getter;
                    if (getter != null) {
                        writer.WriteStartElement("getter");
                        mrw.WriteMemberReference(getter);
                        writer.WriteEndElement();
                    }

                    Method setter = attachedProperty.Setter;
                    if (setter != null) {
                        writer.WriteStartElement("setter");
                        mrw.WriteMemberReference(setter);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                } else if (attachedMember.NodeType == NodeType.Event) {

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

        private void WriteContentPropertyData(XmlWriter writer, object info) {
            TypeNode type = (TypeNode)info;
            if (contentProperties.ContainsKey(type)) {

                // get default constructors
                InstanceInitializer constructor = type.GetConstructor(new TypeNode[0]);
                if ((constructor != null) && (!constructor.IsPublic)) constructor = null;

                if (constructor != null) writer.WriteAttributeString("defaultConstructor", mrw.ApiNamer.GetMemberName(constructor));

            }
        }

    }

}
