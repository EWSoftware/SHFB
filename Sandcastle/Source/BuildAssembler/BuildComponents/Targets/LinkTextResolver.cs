// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// Link text writing logic
    /// </summary>
    public class LinkTextResolver
    {

        public LinkTextResolver(TargetCollection targets)
        {
            this.targets = targets;
        }

        private TargetCollection targets;

        public void WriteTarget(Target target, DisplayOptions options, XmlWriter writer)
        {
            if(target == null)
                throw new ArgumentNullException("target");

            if(writer == null)
                throw new ArgumentNullException("writer");

            NamespaceTarget space = target as NamespaceTarget;

            if(space != null)
            {
                WriteNamespaceTarget(space, writer);
                return;
            }

            TypeTarget type = target as TypeTarget;

            if(type != null)
            {
                WriteTypeTarget(type, options, writer);
                return;
            }

            MemberTarget member = target as MemberTarget;

            if(member != null)
            {
                WriteMemberTarget(member, options, writer);
                return;
            }

            if(target.Id.StartsWith("R:", StringComparison.InvariantCultureIgnoreCase))
            {
                WriteInvalid(new InvalidReference(target.Id), writer);
                return;
            }

            throw new InvalidOperationException();
        }

        public static void WriteNamespaceTarget(NamespaceTarget space, XmlWriter writer)
        {
            if(space == null)
                throw new ArgumentNullException("space");

            if(writer == null)
                throw new ArgumentNullException("writer");

            writer.WriteString(space.Name);
        }

        public void WriteTypeTarget(TypeTarget type, DisplayOptions options, XmlWriter writer)
        {
            if(type == null)
                throw new ArgumentNullException("type");

            if(writer == null)
                throw new ArgumentNullException("writer");

            WriteTypeTarget(type, options, true, writer);
        }

        private void WriteTypeTarget(TypeTarget type, DisplayOptions options, bool showOuterType, XmlWriter writer)
        {

            // write namespace, if containers are requested
            if((options & DisplayOptions.ShowContainer) > 0)
            {
                WriteNamespace(type.Namespace, writer);
                WriteSeparator(writer);
            }

            // write outer type, if one exists
            if(showOuterType && (type.OuterType != null))
            {
                WriteSimpleType(type.OuterType, DisplayOptions.Default, writer);
                WriteSeparator(writer);
            }

            // write the type name
            writer.WriteString(type.Name);

            // write if template parameters, if they exist and we are requested
            if((options & DisplayOptions.ShowTemplates) > 0)
            {
                WriteTemplateParameters(type.Templates, writer);
            }
        }

        public void WriteMemberTarget(MemberTarget target, DisplayOptions options, XmlWriter writer)
        {
            WriteMemberTarget(target, options, writer, null);
        }


        private void WriteMemberTarget(MemberTarget target, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            if(target == null)
                throw new ArgumentNullException("target");
            if(writer == null)
                throw new ArgumentNullException("writer");

            if((options & DisplayOptions.ShowContainer) > 0)
            {
                TypeReference type = target.Type;
                WriteType(type, options & ~DisplayOptions.ShowContainer, writer);
                MethodTarget methodTarget = target as MethodTarget;
                if(methodTarget != null)
                {
                    if(methodTarget.conversionOperator)
                    {
                        writer.WriteString(" ");
                    }
                    else
                    {
                        WriteSeparator(writer);
                    }
                }
                else
                {
                    WriteSeparator(writer);
                }
            }

            // special logic for writing methods
            MethodTarget method = target as MethodTarget;
            if(method != null)
            {
                WriteMethod(method, options, writer, dictionary);
                return;
            }

            // special logic for writing properties
            PropertyTarget property = target as PropertyTarget;
            if(property != null)
            {
                WriteProperty(property, options, writer);
                return;
            }

            // special logic for writing constructors
            ConstructorTarget constructor = target as ConstructorTarget;
            if(constructor != null)
            {
                WriteConstructor(constructor, options, writer);
                return;
            }

            // special logic for writing events
            EventTarget trigger = target as EventTarget;

            if(trigger != null)
            {
                WriteEvent(trigger, writer);
                return;
            }

            // by default, just write name
            writer.WriteString(target.Name);
        }

        public void WriteReference(Reference reference, DisplayOptions options, XmlWriter writer)
        {
            if(reference == null)
                throw new ArgumentNullException("reference");
            if(writer == null)
                throw new ArgumentNullException("writer");

            NamespaceReference space = reference as NamespaceReference;

            if(space != null)
            {
                WriteNamespace(space, writer);
                return;
            }

            TypeReference type = reference as TypeReference;
            if(type != null)
            {
                WriteType(type, options, writer);
                return;
            }

            MemberReference member = reference as MemberReference;
            if(member != null)
            {
                WriteMember(member, options, writer);
                return;
            }

            ExtensionMethodReference extMethod = reference as ExtensionMethodReference;
            if(extMethod != null)
            {
                WriteExtensionMethod(extMethod, options, writer);
                return;
            }

            InvalidReference invalid = reference as InvalidReference;

            if(invalid != null)
            {
                WriteInvalid(invalid, writer);
                return;
            }

            throw new InvalidOperationException();
        }

        public void WriteNamespace(NamespaceReference spaceReference, XmlWriter writer)
        {
            if(spaceReference == null)
                throw new ArgumentNullException("spaceReference");

            if(writer == null)
                throw new ArgumentNullException("writer");

            NamespaceTarget spaceTarget = spaceReference.Resolve(targets) as NamespaceTarget;

            if(spaceTarget != null)
                WriteNamespaceTarget(spaceTarget, writer);
            else
                TextReferenceUtilities.WriteNamespaceReference(spaceReference, writer);
        }

        public void WriteType(TypeReference type, DisplayOptions options, XmlWriter writer)
        {
            WriteType(type, options, writer, null);
        }

        private void WriteType(TypeReference type, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {

            if(type == null)
                throw new ArgumentNullException("type");
            if(writer == null)
                throw new ArgumentNullException("writer");

            SimpleTypeReference simple = type as SimpleTypeReference;
            if(simple != null)
            {
                WriteSimpleType(simple, options, writer);
                return;
            }

            SpecializedTypeReference specialized = type as SpecializedTypeReference;
            if(specialized != null)
            {
                WriteSpecializedType(specialized, options, writer);
                return;
            }

            ArrayTypeReference array = type as ArrayTypeReference;
            if(array != null)
            {
                WriteArrayType(array, options, writer, dictionary);
                return;
            }

            ReferenceTypeReference reference = type as ReferenceTypeReference;
            if(reference != null)
            {
                WriteReferenceType(reference, options, writer, dictionary);
                return;
            }

            PointerTypeReference pointer = type as PointerTypeReference;
            if(pointer != null)
            {
                WritePointerType(pointer, options, writer, dictionary);
                return;
            }

            TemplateTypeReference template = type as TemplateTypeReference;
            if(template != null)
            {
                WriteTemplateType(template, options, writer, dictionary);
                return;
            }

            throw new InvalidOperationException("Unknown type reference type");

        }

        public void WriteSimpleType(SimpleTypeReference simple, DisplayOptions options, XmlWriter writer)
        {
            WriteSimpleType(simple, options, true, writer);
        }

        private void WriteSimpleType(SimpleTypeReference simple, DisplayOptions options, bool showOuterType, XmlWriter writer)
        {
            TypeTarget type = simple.Resolve(targets) as TypeTarget;
            if(type != null)
            {
                WriteTypeTarget(type, options, showOuterType, writer);
            }
            else
            {
                TextReferenceUtilities.WriteSimpleTypeReference(simple, options, writer);
            }
        }

        private static void WriteTemplateParameters(string[] templates, XmlWriter writer)
        {

            if(templates.Length == 0)
                return;

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString("<");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString("(Of ");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString("<");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString("(");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString("<'");
            writer.WriteEndElement();

            writer.WriteEndElement();

            for(int i = 0; i < templates.Length; i++)
            {
                if(i > 0)
                    writer.WriteString(", ");
                writer.WriteString(templates[i]);
            }

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString(")");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString(")");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private void WriteSpecializedType(SpecializedTypeReference special, DisplayOptions options, XmlWriter writer)
        {

            Specialization[] specializations = special.Specializations;
            for(int i = 0; i < specializations.Length; i++)
            {
                if(i == 0)
                {
                    WriteSpecialization(specializations[0], options, writer);
                }
                else
                {
                    WriteSeparator(writer);
                    WriteSpecialization(specializations[i], options & ~DisplayOptions.ShowContainer, writer);
                }

            }

        }

        private void WriteSpecialization(Specialization specialization, DisplayOptions options, XmlWriter writer)
        {
            // write the type itself (without outer types, because those will be written be other calls to this routine)
            WriteSimpleType(specialization.TemplateType, (options & ~DisplayOptions.ShowTemplates), false, writer);

            // then write the template arguments
            WriteTemplateArguments(specialization.Arguments, writer);
        }

        private void WriteTemplateArguments(TypeReference[] specialization, XmlWriter writer)
        {

            if(specialization.Length == 0)
                return;

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString("<");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString("(Of ");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString("<");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString("<'");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString("(");
            writer.WriteEndElement();

            writer.WriteEndElement();

            for(int i = 0; i < specialization.Length; i++)
            {
                if(i > 0)
                    writer.WriteString(", ");
                WriteType(specialization[i], DisplayOptions.Default, writer);
            }

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString(")");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString(")");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void WriteArrayType(ArrayTypeReference reference, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            // C++ array notation (left)
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString("array<");
            writer.WriteEndElement();
            writer.WriteEndElement(); // end of <span class="languageSpecificText"> element

            // the underlying type
            WriteType(reference.ElementType, options, writer, dictionary);

            // C++ array notation (right)
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");

            if(reference.Rank > 1)
                writer.WriteString("," + reference.Rank.ToString(CultureInfo.InvariantCulture));

            writer.WriteString(">");
            writer.WriteEndElement();

            // C# array notation
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString("[");

            for(int i = 1; i < reference.Rank; i++)
                writer.WriteString(",");

            writer.WriteString("]");
            writer.WriteEndElement();

            // VB array notation
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString("(");

            for(int i = 1; i < reference.Rank; i++)
                writer.WriteString(",");

            writer.WriteString(")");
            writer.WriteEndElement();

            // neutral array notation
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString("[");

            for(int i = 1; i < reference.Rank; i++)
                writer.WriteString(",");

            writer.WriteString("]");
            writer.WriteEndElement();

            // F# array notation
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString("[");

            for(int i = 1; i < reference.Rank; i++)
                writer.WriteString(",");

            writer.WriteString("]");
            writer.WriteEndElement();

            writer.WriteEndElement(); // end of <span class="languageSpecificText"> element
        }

        private static void WriteSeparator(XmlWriter writer)
        {
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            // C# separator
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString(".");
            writer.WriteEndElement();

            // VB separator
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString(".");
            writer.WriteEndElement();

            // C++ separator
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString("::");
            writer.WriteEndElement();

            // neutral separator
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString(".");
            writer.WriteEndElement();

            // F# separator
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString(".");
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private void WritePointerType(PointerTypeReference pointer, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            WriteType(pointer.PointedToType, options, writer, dictionary);
            writer.WriteString("*");
        }

        private void WriteReferenceType(ReferenceTypeReference reference, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            WriteType(reference.ReferredToType, options, writer, dictionary);

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            // add % in C++
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString("%");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void WriteTemplateType(TemplateTypeReference template, DisplayOptions options, XmlWriter writer)
        {
            WriteTemplateType(template, options, writer, null);
        }

        private void WriteTemplateType(TemplateTypeReference template, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            // if we have the name, just write it
            NamedTemplateTypeReference namedTemplate = template as NamedTemplateTypeReference;
            if(namedTemplate != null)
            {
                writer.WriteString(namedTemplate.Name);
                return;
            }

            IndexedTemplateTypeReference indexedTemplate = template as IndexedTemplateTypeReference;
            if(indexedTemplate != null)
            {
                if(dictionary != null && dictionary.ContainsKey(indexedTemplate))
                    WriteType(dictionary[indexedTemplate], options, writer);
                else
                    writer.WriteString(GetTemplateName(indexedTemplate.TemplateId, indexedTemplate.Index));

                return;
            }

            TypeTemplateTypeReference typeTemplate = template as TypeTemplateTypeReference;

            if(typeTemplate != null)
            {

                TypeReference value = null;
                if(dictionary != null)
                {
                    IndexedTemplateTypeReference key = new IndexedTemplateTypeReference(typeTemplate.TemplateType.Id, typeTemplate.Position);

                    if(dictionary.ContainsKey(key))
                        value = dictionary[key];
                }

                if(value == null)
                    writer.WriteString(GetTypeTemplateName(typeTemplate.TemplateType, typeTemplate.Position));
                else
                    WriteType(value, options, writer);

                return;
            }

            throw new InvalidOperationException();
        }

        private string GetTemplateName(string templateId, int position)
        {
            Target target = targets[templateId];

            if(target == null)
            {
                return ("UTT");
            }
            else
            {

                TypeTarget type = target as TypeTarget;
                if(type != null)
                {
                    string[] templates = type.Templates;
                    if(templates.Length > position)
                    {
                        return (templates[position]);
                    }
                    else
                    {
                        return ("UTT");
                    }
                }

                MethodTarget method = target as MethodTarget;
                if(method != null)
                {
                    string[] templates = method.Templates;
                    if(templates.Length > position)
                    {
                        return (templates[position]);
                    }
                    else
                    {
                        return ("UTT");
                    }
                }

                return ("UTT");
            }
        }

        private string GetTypeTemplateName(SimpleTypeReference type, int position)
        {
            TypeTarget target = type.Resolve(targets) as TypeTarget;
            if(target != null)
            {
                string[] templates = target.Templates;
                if(templates.Length > position)
                {
                    return (templates[position]);
                }
                else if(target.OuterType != null)
                {
                    return (GetTypeTemplateName(target.OuterType, position));
                }
                else
                {
                    return ("UTT");
                }
            }
            else
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "Unknown type reference '{0}'", type.Id));
            }
        }

        public void WriteExtensionMethod(ExtensionMethodReference extMethod, DisplayOptions options, XmlWriter writer)
        {
            if(extMethod == null)
                throw new ArgumentNullException("extMethod");
            if(writer == null)
                throw new ArgumentNullException("writer");

            // write the unqualified method name
            writer.WriteString(extMethod.Name);

            // if this is a generic method, write any template params or args
            if(extMethod.TemplateArgs != null && extMethod.TemplateArgs.Length > 0)
            {
                WriteTemplateArguments(extMethod.TemplateArgs, writer);
            }

            // write parameters
            if((options & DisplayOptions.ShowParameters) > 0)
                WriteMethodParameters(extMethod.Parameters, writer);
        }

        public void WriteMember(MemberReference member, DisplayOptions options, XmlWriter writer)
        {

            if(member == null)
                throw new ArgumentNullException("member");
            if(writer == null)
                throw new ArgumentNullException("writer");

            SimpleMemberReference simple = member as SimpleMemberReference;
            if(simple != null)
            {
                WriteSimpleMember(simple, options, writer);
                return;
            }

            SpecializedMemberReference special = member as SpecializedMemberReference;
            if(special != null)
            {
                WriteSpecializedMember(special, options, writer);
                return;
            }

            SpecializedMemberWithParametersReference ugly = member as SpecializedMemberWithParametersReference;
            if(ugly != null)
            {
                WriteSpecializedMemberWithParameters(ugly, options, writer);
                return;
            }

            throw new InvalidOperationException();

        }

        private void WriteSpecializedMember(SpecializedMemberReference member, DisplayOptions options, XmlWriter writer)
        {

            if((options & DisplayOptions.ShowContainer) > 0)
            {
                WriteType(member.SpecializedType, options & ~DisplayOptions.ShowContainer, writer);
                WriteSeparator(writer);
            }

            Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary = member.SpecializedType.GetSpecializationDictionary();
            WriteSimpleMember(member.TemplateMember, options & ~DisplayOptions.ShowContainer, writer, dictionary);

        }

        private void WriteSimpleMember(SimpleMemberReference member, DisplayOptions options, XmlWriter writer)
        {
            WriteSimpleMember(member, options, writer, null);
        }

        private void WriteSimpleMember(SimpleMemberReference member, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            MemberTarget target = member.Resolve(targets) as MemberTarget;

            if(target != null)
                WriteMemberTarget(target, options, writer, dictionary);
            else
                TextReferenceUtilities.WriteSimpleMemberReference(member, options, writer, this);
        }

        private void WriteProcedureName(ProcedureTarget target, XmlWriter writer)
        {
            MemberReference implements = target.ExplicitlyImplements;

            if(implements == null)
            {
                if(target.conversionOperator)
                    WriteConversionOperator(target, writer);
                else
                    writer.WriteString(target.Name);
            }
            else
                WriteMember(implements, DisplayOptions.ShowContainer, writer);
        }

        private void WriteMethod(MethodTarget target, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            WriteProcedureName(target, writer);

            if((options & DisplayOptions.ShowTemplates) > 0)
            {
                // if this is a generic method, write any template params or args
                if(target.TemplateArgs != null && target.TemplateArgs.Length > 0)
                    WriteTemplateArguments(target.TemplateArgs, writer);
            }

            if((options & DisplayOptions.ShowParameters) > 0)
            {
                Parameter[] parameters = target.Parameters;

                if(target.ConversionOperator)
                {
                    TypeReference returns = target.returnType;
                    WriteConversionOperatorParameters(parameters, returns, writer, dictionary);
                }
                else
                    WriteMethodParameters(parameters, writer, dictionary);
            }
        }

        private static void WriteConversionOperator(ProcedureTarget target, XmlWriter writer)
        {
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString(target.Name);
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            if(target.name == "Explicit")
            {
                writer.WriteString("Narrowing");
            }
            else if(target.name == "Implicit")
            {
                writer.WriteString("Widening");
            }
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString(target.name);
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString(target.name);
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString(target.name);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        internal void WriteMethodParameters(Parameter[] parameters, XmlWriter writer)
        {
            WriteMethodParameters(parameters, writer, null);
        }


        private void WriteMethodParameters(Parameter[] parameters, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            if(parameters.Length > 0)
            {
                writer.WriteString("(");

                // show parameters
                // we need to deal with type template substitutions!
                for(int i = 0; i < parameters.Length; i++)
                {
                    if(i > 0)
                        writer.WriteString(", ");
                    WriteType(parameters[i].Type, DisplayOptions.Default, writer, dictionary);
                }

                writer.WriteString(")");
            }
            else
            {
                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "languageSpecificText");
                // when there are no parameters, VB shows no parenthesis

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "cs");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "cpp");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "nu");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "fs");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteEndElement();

            }
        }

        private void WriteConversionOperatorParameters(Parameter[] parameters, TypeReference returns, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            if(parameters.Length > 0 || returns != null)
                writer.WriteString("(");

            if(parameters.Length > 0)
                WriteType(parameters[0].Type, DisplayOptions.Default, writer, dictionary);

            if(parameters.Length > 0 && returns != null)
                writer.WriteString(" to ");

            if(returns != null)
                WriteType(returns, DisplayOptions.Default, writer, dictionary);

            if(parameters.Length > 0 || returns != null)
                writer.WriteString(")");

            if(parameters.Length == 0 && returns == null)
            {
                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "languageSpecificText");
                // when there are no parameters, VB shows no parenthesis

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "cs");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "cpp");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "nu");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "fs");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        private void WriteProperty(PropertyTarget target, DisplayOptions options, XmlWriter writer)
        {
            WriteProcedureName(target, writer);

            if((options & DisplayOptions.ShowParameters) > 0)
            {
                Parameter[] parameters = target.Parameters;

                // VB only shows parenthesis when there are parameters
                if(parameters.Length > 0)
                {
                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "languageSpecificText");
                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "cs");
                    writer.WriteString("[");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "vb");
                    writer.WriteString("(");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "cpp");
                    writer.WriteString("[");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "nu");
                    writer.WriteString("(");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "fs");
                    writer.WriteString(" ");
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    // show parameters
                    // we need to deal with type template substitutions!
                    for(int i = 0; i < parameters.Length; i++)
                    {
                        if(i > 0)
                            writer.WriteString(", ");
                        WriteType(parameters[i].Type, DisplayOptions.Default, writer);
                    }

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "languageSpecificText");
                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "cs");
                    writer.WriteString("]");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "vb");
                    writer.WriteString(")");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "cpp");
                    writer.WriteString("]");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "nu");
                    writer.WriteString(")");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "fs");
                    writer.WriteString(" ");
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }

            }

        }

        private void WriteEvent(EventTarget trigger, XmlWriter writer)
        {
            WriteProcedureName(trigger, writer);
        }

        private void WriteConstructor(ConstructorTarget constructor, DisplayOptions options, XmlWriter writer)
        {


            WriteType(constructor.Type, options & ~DisplayOptions.ShowContainer, writer);

            if((options & DisplayOptions.ShowParameters) > 0)
            {
                Parameter[] parameters = constructor.Parameters;
                WriteMethodParameters(parameters, writer);
            }

        }

        private void WriteSpecializedMemberWithParameters(SpecializedMemberWithParametersReference ugly, DisplayOptions options, XmlWriter writer)
        {

            if((options & DisplayOptions.ShowContainer) > 0)
            {
                WriteSpecializedType(ugly.SpecializedType, options & ~DisplayOptions.ShowContainer, writer);
                WriteSeparator(writer);
            }

            writer.WriteString(ugly.MemberName);

            if((options & DisplayOptions.ShowParameters) > 0)
            {

                writer.WriteString("(");

                TypeReference[] parameterTypes = ugly.ParameterTypes;
                for(int i = 0; i < parameterTypes.Length; i++)
                {
                    if(i > 0)
                        writer.WriteString(", ");

                    WriteType(parameterTypes[i], DisplayOptions.Default, writer);
                }

                writer.WriteString(")");
            }
        }

        private static void WriteInvalid(InvalidReference invalid, XmlWriter writer)
        {
            writer.WriteString("[" + invalid.Id + "]");
        }
    }
}
