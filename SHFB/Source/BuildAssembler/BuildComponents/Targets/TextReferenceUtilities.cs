// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 02/14/2013 - EFW - Removed RegexOptions.Compiled from the Regex instances as it doesn't appear to make any
// real difference in performance.
// 05/16/2014 - EFW - Added separating period after namespace in WriteSimpleMemberReference()

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// Logic for constructing references from code entity reference strings.  Anything that depends on the
    /// specific form of the ID strings lives here.
    /// </summary>
    public static class TextReferenceUtilities
    {
        /// <summary>
        /// Create a reference
        /// </summary>
        /// <param name="api">The member ID for which to create a reference</param>
        /// <returns>The reference</returns>
        public static Reference CreateReference(string api)
        {
            if(String.IsNullOrEmpty(api))
                throw new ArgumentException("api cannot be null or empty");

            Reference reference;

            char start = api[0];

            if(start == 'N')
                reference = CreateNamespaceReference(api);
            else
                if(start == 'T')
                    reference = CreateTypeReference(api);
                else
                    reference = CreateMemberReference(api);

            return reference ?? new InvalidReference(api);
        }

        /// <summary>
        /// Create a namespace reference
        /// </summary>
        /// <param name="api">The member ID for which to create a reference</param>
        /// <returns>The namespace reference</returns>
        public static NamespaceReference CreateNamespaceReference(string api)
        {
            if(ValidNamespace.IsMatch(api))
                return new NamespaceReference(api);

            return null;
        }

        /// <summary>
        /// Create a type reference
        /// </summary>
        /// <param name="api">The member ID for which to create a reference</param>
        /// <returns>The type reference</returns>
        public static TypeReference CreateTypeReference(string api)
        {
            if(api == null)
                throw new ArgumentNullException(nameof(api));

            if(ValidSimpleType.IsMatch(api))
            {
                // This is a reference to a "normal" simple type
                return CreateSimpleTypeReference(api);
            }

            if(ValidSpecializedType.IsMatch(api))
            {
                // This is a reference to a specialized type
                return CreateSpecializedTypeReference(api);
            }

            if(ValidDecoratedType.IsMatch(api))
            {
                // This is a reference to a type that is decorated or is a template process array, reference,
                // and pointer decorations.
                char lastCharacter = api[api.Length - 1];

                if(lastCharacter == ']')
                {
                    // Arrays
                    int lastBracketPosition = api.LastIndexOf('[');
                    int rank = api.Length - lastBracketPosition - 1;
                    string elementApi = api.Substring(0, lastBracketPosition);
                    TypeReference elementReference = CreateTypeReference(elementApi);
                    return new ArrayTypeReference(elementReference, rank);
                }

                if(lastCharacter == '@')
                {
                    // References
                    string referedApi = api.Substring(0, api.Length - 1);
                    TypeReference referedReference = CreateTypeReference(referedApi);
                    return new ReferenceTypeReference(referedReference);
                }

                if(lastCharacter == '*')
                {
                    // Pointers
                    string pointedApi = api.Substring(0, api.Length - 1);
                    TypeReference pointedReference = CreateTypeReference(pointedApi);
                    return new PointerTypeReference(pointedReference);
                }

                // Process templates
                if(api.StartsWith("T:``", StringComparison.OrdinalIgnoreCase))
                {
                    int position = Convert.ToInt32(api.Substring(4), CultureInfo.InvariantCulture);

                    if(genericTypeContext == null)
                        return new NamedTemplateTypeReference("UMP");

                    return new IndexedTemplateTypeReference(genericTypeContext.Id, position);
                }

                if(api.StartsWith("T:`", StringComparison.OrdinalIgnoreCase))
                {
                    int position = Convert.ToInt32(api.Substring(3), CultureInfo.InvariantCulture);

                    if(genericTypeContext == null)
                        return (new NamedTemplateTypeReference("UTP"));

                    return new IndexedTemplateTypeReference(genericTypeContext.Id, position);
                }

                // We shouldn't get here, because one of those test should have been satisfied if the regex matched
                throw new InvalidOperationException("Could not parse valid type expression");
            }

            return null;
        }

        private static SimpleTypeReference CreateSimpleTypeReference(string api)
        {
            return new SimpleTypeReference(api);
        }

        private static SpecializedTypeReference CreateSpecializedTypeReference(string api)
        {
            List<Specialization> specializations = new List<Specialization>();

            string text = String.Copy(api);

            // At the moment we are only handling one specialization; need to iterate
            int specializationStart = text.IndexOf('{');
            int specializationEnd = FindMatchingEndBracket(text, specializationStart);
            string list = text.Substring(specializationStart + 1, specializationEnd - specializationStart - 1);
            IList<string> types = SeparateTypes(list);
            string template = text.Substring(0, specializationStart) + String.Format(CultureInfo.InvariantCulture,
                "`{0}", types.Count);

            SimpleTypeReference templateReference = CreateSimpleTypeReference(template);
            TypeReference[] argumentReferences = new TypeReference[types.Count];

            for(int i = 0; i < types.Count; i++)
                argumentReferences[i] = CreateTypeReference(types[i]);

            Specialization specialization = new Specialization(templateReference, argumentReferences);

            specializations.Add(specialization);

            // end iteration

            return new SpecializedTypeReference(specializations);
        }

        /// <summary>
        /// Create a member reference
        /// </summary>
        /// <param name="api">The member ID for which to create a reference</param>
        /// <returns>The member reference</returns>
        public static MemberReference CreateMemberReference(string api)
        {
            if(api == null)
                throw new ArgumentNullException(nameof(api));

            if(ValidSimpleMember.IsMatch(api))
            {
                // This is just a normal member of a simple type
                return new SimpleMemberReference(api);
            }

            if(ValidSpecializedMember.IsMatch(api))
            {
                // This is a member of a specialized type; we need to extract:
                // (1) the underlying specialized type, (2) the member name, (3) the arguments

                // Separate the member prefix
                int colonPosition = api.IndexOf(':');
                string prefix = api.Substring(0, colonPosition);
                string text = api.Substring(colonPosition + 1);

                // Get the arguments
                string arguments = String.Empty;
                int startParenthesisPosition = text.IndexOf('(');

                if(startParenthesisPosition > 0)
                {
                    int endParenthesisPosition = text.LastIndexOf(')');
                    arguments = text.Substring(startParenthesisPosition + 1, endParenthesisPosition - startParenthesisPosition - 1);
                    text = text.Substring(0, startParenthesisPosition);
                }

                // Separate the type and member name
                int lastDotPosition;
                int firstHashPosition = text.IndexOf('#');

                if(firstHashPosition > 0)
                {
                    // If this is an EII, the boundary is at the last dot before the hash
                    lastDotPosition = text.LastIndexOf('.', firstHashPosition);
                }
                else
                {
                    // Otherwise, the boundary is at the last dot
                    lastDotPosition = text.LastIndexOf('.');
                }

                string name = text.Substring(lastDotPosition + 1);
                text = text.Substring(0, lastDotPosition);

                // Text now contains a specialized generic type; use it to create a reference
                SpecializedTypeReference type = CreateSpecializedTypeReference("T:" + text);

                // If there are no arguments, we simply create a reference to a member whose identifier we
                // construct in the specialized type.
                if(String.IsNullOrEmpty(arguments))
                {
                    string typeId = type.Specializations[type.Specializations.Count - 1].TemplateType.Id;
                    string memberId = String.Format(CultureInfo.InvariantCulture, "{0}:{1}.{2}", prefix,
                        typeId.Substring(2), name);
                    SimpleMemberReference member = new SimpleMemberReference(memberId);
                    return new SpecializedMemberReference(member, type);
                }

                // If there are arguments, life is not so simple.  We can't be sure we can identify the
                // corresponding member of the template type because any particular type that appears in
                // the argument might have come from the template or it might have come from the specialization.
                // We need to create a special kind of reference to handle this situation.
                IList<string> parameterTypeCers = SeparateTypes(arguments);
                TypeReference[] parameterTypes = new TypeReference[parameterTypeCers.Count];

                for(int i = 0; i < parameterTypeCers.Count; i++)
                    parameterTypes[i] = CreateTypeReference(parameterTypeCers[i]);

                return new SpecializedMemberWithParametersReference(prefix, type, name, parameterTypes);
            }

            return null;
        }

        // Template context logic

        private static SimpleTypeReference genericTypeContext;

        /// <summary>
        /// Set the generic context
        /// </summary>
        /// <param name="codeEntityReference">The member ID for which to set the context</param>
        public static void SetGenericContext(string codeEntityReference)
        {
            // Reset the context
            genericTypeContext = null;

            // Get the new context
            Reference context = CreateReference(codeEntityReference);

            if(context == null)
                return;

            // If it is a type context, set it to be the type context
            if(context is SimpleTypeReference typeContext)
            {
                genericTypeContext = typeContext;
                return;
            }

            // If it is a member context, set it to be the member context and use it to obtain a type context, too
            if(context is SimpleMemberReference memberContext)
            {
                DecomposeMemberIdentifier(memberContext.Id, out string typeId, out _, out _);
                genericTypeContext = CreateSimpleTypeReference(typeId);
                return;
            }
        }

        /// <summary>
        /// This read-only property returns the generic context
        /// </summary>
        public static SimpleTypeReference GenericContext => genericTypeContext;

        // Code entity reference validation logic

        // iterate -> specializedTypePattern -> decoratedTypePattern -> decoratedTypeListPattern
        // to get a patterns that enforce the contents of specialization brackets

        static TextReferenceUtilities()
        {
            string namePattern = @"[_a-zA-Z0-9]+";

            // Namespace patterns
            string namespacePattern = String.Format(CultureInfo.InvariantCulture, @"({0}\.)*({0})?", namePattern);

            string optionalNamespacePattern = String.Format(CultureInfo.InvariantCulture, @"({0}\.)*", namePattern);

            // Type patterns
            string simpleTypePattern = String.Format(CultureInfo.InvariantCulture, @"{0}({1}(`\d+)?\.)*{1}(`\d+)?",
                optionalNamespacePattern, namePattern);

            //string specializedTypePattern = String.Format(CultureInfo.InvariantCulture, @"{0}({1}(\{{.+\}})?\.)*{1}(\{{.+\}})?", optionalNamespacePattern, namePattern);
            string specializedTypePattern = String.Format(CultureInfo.InvariantCulture,
                @"({0}(\{{.+\}})?\.)*{0}(\{{.+\}})?", namePattern);

            //string baseTypePattern = String.Format(CultureInfo.InvariantCulture, @"({0})|({1})", simpleTypePattern, specializedTypePattern);

            string decoratedTypePattern = String.Format(CultureInfo.InvariantCulture,
                @"(({0})|(`\d+)|(``\d+))(@|\*|(\[\]))*", specializedTypePattern);

            string decoratedTypeListPattern = String.Format(CultureInfo.InvariantCulture,
                @"({0},)*{0}", decoratedTypePattern);

            string explicitInterfacePattern = String.Format(CultureInfo.InvariantCulture,
                @"({0}(\{{[^\.]+\}})?#)*", namePattern);

            // Members of non-specialized types
            string simpleFieldPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}",
                simpleTypePattern, namePattern);

            string simpleEventPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}{2}",
                simpleTypePattern, explicitInterfacePattern, namePattern);

            string simplePropertyPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}{2}(\({3}\))?",
                simpleTypePattern, explicitInterfacePattern, namePattern, decoratedTypeListPattern);

            string simpleMethodPattern = String.Format(CultureInfo.InvariantCulture,
                @"{0}\.{1}{2}(``\d+)?(\({3}\))?(~{4})?", simpleTypePattern, explicitInterfacePattern,
                namePattern, decoratedTypeListPattern, decoratedTypePattern);

            string simpleConstructorPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.#ctor(\({1}\))?",
                simpleTypePattern, decoratedTypeListPattern);

            string simpleOverloadPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}{2}",
                simpleTypePattern, explicitInterfacePattern, namePattern);

            string simpleConstructorOverloadPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.#ctor",
                simpleTypePattern);

            // Members of specialized types
            string specializedFieldPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}",
                specializedTypePattern, namePattern);

            string specializedEventPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}{2}",
                specializedTypePattern, explicitInterfacePattern, namePattern);

            string specializedPropertyPattern = String.Format(CultureInfo.InvariantCulture,
                @"{0}\.{1}{2}(\({3}\))?", specializedTypePattern, explicitInterfacePattern, namePattern,
                decoratedTypeListPattern);

            string specializedMethodPattern = String.Format(CultureInfo.InvariantCulture,
                @"{0}\.{1}{2}(``\d+)?(\({3}\))?(~{4})?", specializedTypePattern, explicitInterfacePattern,
                namePattern, decoratedTypeListPattern, decoratedTypePattern);

            string specializedOverloadPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}{2}",
                specializedTypePattern, explicitInterfacePattern, namePattern);

            // Create regexes using this patterns
            ValidNamespace = new Regex(String.Format(CultureInfo.InvariantCulture, @"^N:{0}$", namespacePattern));

            ValidSimpleType = new Regex(String.Format(CultureInfo.InvariantCulture, @"^T:{0}$",
                simpleTypePattern));

            ValidDecoratedType = new Regex(String.Format(CultureInfo.InvariantCulture, @"^T:{0}$",
                decoratedTypePattern));

            ValidSpecializedType = new Regex(String.Format(CultureInfo.InvariantCulture, @"^T:{0}$",
                specializedTypePattern));

            ValidSimpleMember = new Regex(String.Format(CultureInfo.InvariantCulture,
                @"^((M:{0})|(M:{1})|(P:{2})|(F:{3})|(E:{4})|(Overload:{5})|(Overload:{6}))$",
                simpleMethodPattern, simpleConstructorPattern, simplePropertyPattern, simpleFieldPattern,
                simpleEventPattern, simpleOverloadPattern, simpleConstructorOverloadPattern));

            ValidSpecializedMember = new Regex(String.Format(CultureInfo.InvariantCulture,
                @"^((M:{0})|(P:{1})|(F:{2})|(E:{3})|(Overload:{4}))$", specializedMethodPattern,
                specializedPropertyPattern, specializedFieldPattern, specializedEventPattern,
                specializedOverloadPattern));
        }

        private static readonly Regex ValidNamespace;

        private static readonly Regex ValidSimpleType;

        private static readonly Regex ValidDecoratedType;

        private static readonly Regex ValidSpecializedType;

        private static readonly Regex ValidSimpleMember;

        private static readonly Regex ValidSpecializedMember;

        // Code entity reference string manipulation utilities

        internal static IList<string> SeparateTypes(string typelist)
        {
            List<string> types = new List<string>();

            int start = 0;
            int specializationCount = 0;

            for(int index = 0; index < typelist.Length; index++)
                switch(typelist[index])
                {
                    case '{':
                    case '[':
                        specializationCount++;
                        break;

                    case '}':
                    case ']':
                        specializationCount--;
                        break;

                    case ',':
                        if(specializationCount == 0)
                        {
                            types.Add("T:" + typelist.Substring(start, index - start).Trim());
                            start = index + 1;
                        }
                        break;
                }

            types.Add("T:" + typelist.Substring(start).Trim());

            return types;
        }

        internal static void DecomposeMemberIdentifier(string memberCer, out string typeCer, out string memberName, out string arguments)
        {
            // Drop the member prefix
            int colonPosition = memberCer.IndexOf(':');
            string text = memberCer.Substring(colonPosition + 1);

            // Get the arguments
            arguments = String.Empty;
            int startParenthesisPosition = text.IndexOf('(');

            if(startParenthesisPosition > 0)
            {
                int endParenthesisPosition = text.LastIndexOf(')');
                arguments = text.Substring(startParenthesisPosition + 1, endParenthesisPosition - startParenthesisPosition - 1);
                text = text.Substring(0, startParenthesisPosition);
            }

            // Separate the type and member name
            int lastDotPosition;
            int firstHashPosition = text.IndexOf('#');

            if(firstHashPosition > 0)
            {
                // If this is an EII, the boundary is at the last dot before the hash
                lastDotPosition = text.LastIndexOf('.', firstHashPosition);
            }
            else
            {
                // Otherwise, the boundary is at the last dot
                lastDotPosition = text.LastIndexOf('.');
            }

            memberName = text.Substring(lastDotPosition + 1);
            typeCer = "T:" + text.Substring(0, lastDotPosition);
        }

        private static int FindMatchingEndBracket(string text, int position)
        {
            if(text == null)
                throw new ArgumentNullException(nameof(text));

            if(position < 0 || position >= text.Length)
                throw new ArgumentOutOfRangeException(nameof(position), String.Format(CultureInfo.InvariantCulture,
                    "The position {0} is not within the given text string.", position));

            if(text[position] != '{')
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Position {0} " +
                    "of the string '{1}' does not contain and ending curly bracket.", position, text));

            int count = 1;

            for(int index = position + 1; index < text.Length; index++)
            {
                if(text[index] == '{')
                    count++;
                else
                    if(text[index] == '}')
                        count--;

                if(count == 0)
                    return index;
            }

            throw new FormatException("No opening brace matches the closing brace.");
        }

        // Writing link text for unresolved simple references

        internal static void WriteNamespaceReference(NamespaceReference space, XmlWriter writer)
        {
            writer.WriteString(space.Id.Substring(2));
        }

        internal static void WriteSimpleTypeReference(SimpleTypeReference type, DisplayOptions options, XmlWriter writer)
        {
            // This logic won't correctly deal with nested types, but type CER strings simply don't include that
            // information, so this is our best guess under the assumption of a non-nested type
            string cer = type.Id;

            // Get the name
            string name;
            int lastDotPosition = cer.LastIndexOf('.');

            if(lastDotPosition > 0)
            {
                // Usually, the name will start after the last dot
                name = cer.Substring(lastDotPosition + 1);
            }
            else
            {
                // But if there is no dot, this is a type in the default namespace and the name is everything
                // after the colon.
                name = cer.Substring(2);
            }

            // Remove any generic tics from the name
            int tickPosition = name.IndexOf('`');

            if(tickPosition > 0)
                name = name.Substring(0, tickPosition);

            if((options & DisplayOptions.ShowContainer) > 0)
            {
                // work out namespace
            }

            writer.WriteString(name);

            if((options & DisplayOptions.ShowTemplates) > 0)
            {
                // work out templates
            }
        }

        internal static void WriteSimpleMemberReference(SimpleMemberReference member, DisplayOptions options, XmlWriter writer, LinkTextResolver resolver)
        {
            string cer = member.Id;

            DecomposeMemberIdentifier(cer, out string typeCer, out string memberName, out string arguments);

            if((options & DisplayOptions.ShowContainer) > 0)
            {
                SimpleTypeReference type = CreateSimpleTypeReference(typeCer);
                WriteSimpleTypeReference(type, options & ~DisplayOptions.ShowContainer, writer);
                writer.WriteString(".");
            }

            // Change this so that we deal with EII names correctly, too
            writer.WriteString(memberName);

            if((options & DisplayOptions.ShowParameters) > 0)
            {
                if(String.IsNullOrEmpty(arguments))
                {
                    Parameter[] parameters = Array.Empty<Parameter>();
                    resolver.WriteMethodParameters(parameters, writer);
                }
                else
                {
                    IList<string> parameterTypeCers = SeparateTypes(arguments);
                    Parameter[] parameters = new Parameter[parameterTypeCers.Count];

                    for(int i = 0; i < parameterTypeCers.Count; i++)
                    {
                        TypeReference parameterType = CreateTypeReference(parameterTypeCers[i]);

                        if(parameterType == null)
                            parameterType = new NamedTemplateTypeReference("UAT");

                        parameters[i] = new Parameter(String.Empty, parameterType);
                    }

                    resolver.WriteMethodParameters(parameters, writer);
                }
            }
        }
    }
}
