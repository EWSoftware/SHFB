//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TopicTransformationExtensions.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/08/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains various extension and utility methods for presentation styles
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/14/2022  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: wbr

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This contains various extension and utility methods for presentation style transformations
    /// </summary>
    public static class TopicTransformationExtensions
    {
        #region Private data members
        //=====================================================================

        private static readonly char[] UniqueIdChars = new char[32] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6' };

        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to convert the given size to an indent consisting of two non-breaking spaces for each
        /// indent level.
        /// </summary>
        /// <param name="size">The indent size (zero for no indent)</param>
        /// <returns>A string containing two non-breaking spaces for each indent level</returns>
        public static string ToIndent(this int size)
        {
            if(size < 1)
                return String.Empty;

            var sb = new StringBuilder(size * 12);

            // XText doesn't write out entities so use the literal character instead
            sb.Insert(0, "\xA0", size * 2);

            return sb.ToString();
        }

        /// <summary>
        /// Remove namespaces from an element and all of its descendants including attributes
        /// </summary>
        /// <param name="element">The element from which to remove namespaces</param>
        public static void RemoveNamespaces(this XElement element)
        {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            element.Name = element.Name.LocalName;
            element.ReplaceAttributes(element.Attributes().Where(a => !a.IsNamespaceDeclaration).Select(
                a => new XAttribute(a.Name.LocalName, a.Value)));

            foreach(var descendant in element.Descendants())
                RemoveNamespaces(descendant);
        }

        /// <summary>
        /// Normalize whitespace in a string by replacing consecutive runs of whitespace with a single space
        /// and removing leading and trailing whitespace.
        /// </summary>
        /// <param name="text">The text to normalize</param>
        /// <returns>The normalized string</returns>
        /// <remarks>This is based on the normalize-space() XSLT function implementation in the .NET Framework</remarks>
        public static string NormalizeWhiteSpace(this string text)
        {
            StringBuilder normalized = null;
            int startIndex = 0, nextIndex = 0, index;

            if(text == null)
                return String.Empty;

            for(index = 0; index < text.Length; index++)
            {
                if(Char.IsWhiteSpace(text[index]))
                {
                    // Skip consecutive whitespace before or after a segment
                    if(index == startIndex)
                        startIndex++;
                    else
                    {
                        // Only append a segment if we hit whitespace other than an actual space or see another
                        // whitespace character after the last one.
                        if(text[index] != ' ' || nextIndex == index)
                        {
                            if(normalized == null)
                                normalized = new StringBuilder(text.Length);
                            else
                                normalized.Append(' ');

                            if(nextIndex == index)
                                normalized.Append(text, startIndex, index - startIndex - 1);
                            else
                                normalized.Append(text, startIndex, index - startIndex);

                            startIndex = index + 1;
                        }
                        else
                            nextIndex = index + 1;
                    }
                }
            }

            // Append the final segment if necessary
            if(normalized == null)
            {
                if(startIndex == index)
                    return String.Empty;

                if(startIndex == 0 && nextIndex != index)
                    return text;

                normalized = new StringBuilder(text.Length);
            }
            else
            {
                if(index != startIndex)
                    normalized.Append(' ');
            }

            if(nextIndex == index)
                normalized.Append(text, startIndex, index - startIndex - 1);
            else
                normalized.Append(text, startIndex, index - startIndex);

            return normalized.ToString();
        }

        /// <summary>
        /// Insert word break opportunities into HTML text to allow better word wrapping when the text container
        /// is narrow like the Table of Contents pane.
        /// </summary>
        /// <param name="text">The text into which word break markers will be inserted</param>
        /// <returns>An enumerable list of the text parts and word break elements</returns>
        public static IEnumerable<XNode> InsertWordBreakOpportunities(this string text)
        {
            if(String.IsNullOrWhiteSpace(text))
            {
                if(text != null)
                    yield return new XText(text);
            }
            else
            {
                int start = 0, end = 0;

                while(end < text.Length)
                {
                    // Split between camel case words, digits, and punctuation with no intervening whitespace
                    if(end != 0 && end < text.Length - 1 && ((Char.IsLower(text[end]) &&
                      Char.IsUpper(text[end + 1])) || (Char.IsLetter(text[end]) &&
                      Char.IsDigit(text[end + 1])) || (!Char.IsLetterOrDigit(text[end]) &&
                      !Char.IsWhiteSpace(text[end - 1]) && Char.IsLetterOrDigit(text[end + 1]))))
                    {
                        yield return new XText(text.Substring(start, end - start + 1));
                        yield return new XElement("wbr");

                        start = end + 1;
                    }

                    // Skip over non-word/non-punctuation characters
                    do
                    {
                        end++;
                    } while(end < text.Length && !Char.IsLetterOrDigit(text[end]) && !Char.IsPunctuation(text[end]));
                }

                if(start == 0 && end == text.Length)
                    yield return new XText(text);
                else
                {
                    if(start < end)
                        yield return new XText(text.Substring(start));
                }
            }
        }

        /// <summary>
        /// This is used to generate a unique ID for an XML node
        /// </summary>
        /// <param name="node">The node for which to generate a unique ID</param>
        /// <returns>This is loosely based on the generate-id() XSLT function implementation in the .NET Framework</returns>
        public static string GenerateUniqueId(this XNode node)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));

            StringBuilder id = new StringBuilder("ID", 10);

            while(node.Parent != null)
            {
                int idx = 0;

                foreach(var n in node.Parent.Nodes())
                {
                    if(n == node)
                        break;

                    idx++;
                }

                if(idx <= 31)
                    id.Append(UniqueIdChars[idx]);
                else
                {
                    id.Append('0');

                    do
                    {
                        id.Append(UniqueIdChars[idx & 31]);
                        idx >>= 5;

                    } while(idx != 0);

                    id.Append('0');
                }

                node = node.Parent;
            }

            return id.ToString();
        }

        /// <summary>
        /// Return all preceding sibling elements that optionally match the given element's name
        /// </summary>
        /// <param name="element">The starting element</param>
        /// <param name="name">The element name to match or null for all preceding siblings</param>
        /// <returns>An enumerable list of the preceding sibling elements optionally limited to those with a
        /// matching name</returns>
        public static IEnumerable<XElement> PrecedingSiblings(this XElement element, XName name)
        {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            XNode node = element;

            while(node.PreviousNode != null)
            {
                node = node.PreviousNode;

                if(node is XElement e && (name == null || e.Name == name))
                    yield return e;
            }
        }

        /// <summary>
        /// Return all following sibling elements that optionally match the given element's name
        /// </summary>
        /// <param name="element">The starting element</param>
        /// <param name="name">The element name to match or null for all preceding siblings</param>
        /// <returns>An enumerable list of the following sibling elements optionally limited to those with a
        /// matching name</returns>
        public static IEnumerable<XElement> FollowingSiblings(this XElement element, XName name)
        {
            if(element == null)
                throw new ArgumentNullException(nameof(element));
            
            XNode node = element;

            while(node.NextNode != null)
            {
                node = node.NextNode;

                if(node is XElement e && (name == null || e.Name == name))
                    yield return e;
            }
        }

        /// <summary>
        /// This is used to retrieve the first attribute of the given type from an API element's
        /// <c>attributes</c> node.
        /// </summary>
        /// <param name="element">The element containing the attribute information</param>
        /// <param name="attributeTypeName">The fully qualified type name of the attribute.  The "T:" prefix will
        /// be added if not specified.</param>
        /// <returns>The first attribute element for the given type if found or null if not</returns>
        public static XElement AttributeOfType(this XElement element, string attributeTypeName)
        {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            if(String.IsNullOrWhiteSpace(attributeTypeName))
                return null;

            if(attributeTypeName.Length < 2 || attributeTypeName[1] != ':' && attributeTypeName[0] != 'T')
                attributeTypeName = "T:" + attributeTypeName;

            return element.Element("attributes")?.Elements("attribute").FirstOrDefault(
                a => a.Element("type").Attribute("api").Value == attributeTypeName);
        }

        /// <summary>
        /// Format a signed enumeration value using the given options
        /// </summary>
        /// <param name="enumValue">The value to format</param>
        /// <param name="format">The format</param>
        /// <param name="minWidth">The minimum width for hex and bit flag values</param>
        /// <param name="separatorGroupSize">The separator group size (4 or 8) or zero for no separators</param>
        /// <returns>The formatted enumeration value</returns>
        public static string FormatSignedEnumValue(string enumValue, EnumValueFormat format, int minWidth, int separatorGroupSize)
        {
            if(!Int64.TryParse(enumValue, out long value))
                return enumValue;

            if(format == EnumValueFormat.IntegerValue)
                return value.ToString("N0", CultureInfo.InvariantCulture);

            string formattedValue;

            switch(value)
            {
                case long v when v >= SByte.MinValue && v <= SByte.MaxValue:
                    if(format == EnumValueFormat.HexValue)
                        formattedValue = $"{(sbyte)value:X}".PadLeft(minWidth, '0');
                    else
                    {
                        formattedValue = Convert.ToString((sbyte)value, 2);

                        if(value < 0 && formattedValue.Length > 8 && minWidth <= 8)
                            formattedValue = formattedValue.Substring(8);
                        else
                            formattedValue = formattedValue.PadLeft(minWidth, '0');
                    }
                    break;

                case long v when v >= Int16.MinValue && v <= Int16.MaxValue:
                    if(format == EnumValueFormat.HexValue)
                        formattedValue = $"{(short)value:X}".PadLeft(minWidth, '0');
                    else
                        formattedValue = Convert.ToString((short)value, 2).PadLeft(minWidth, '0');
                    break;

                case long v when v >= Int32.MinValue && v <= Int32.MaxValue:
                    if(format == EnumValueFormat.HexValue)
                        formattedValue = $"{(int)value:X}".PadLeft(minWidth, '0');
                    else
                        formattedValue = Convert.ToString((int)value, 2).PadLeft(minWidth, '0');
                    break;

                default:
                    if(format == EnumValueFormat.HexValue)
                        formattedValue = $"{value:X}".PadLeft(minWidth, '0');
                    else
                        formattedValue = Convert.ToString(value, 2).PadLeft(minWidth, '0');
                    break;
            }

            if(separatorGroupSize != 0 && formattedValue.Length > separatorGroupSize)
            {
                formattedValue = String.Join("_", Enumerable.Range(0, formattedValue.Length / separatorGroupSize).Select(
                    i => formattedValue.Substring(i * separatorGroupSize, separatorGroupSize)));
            }

            return ((format == EnumValueFormat.HexValue) ? "0x" : "0b") + formattedValue;
        }

        /// <summary>
        /// Format an unsigned enumeration value using the given options
        /// </summary>
        /// <param name="enumValue">The value to format</param>
        /// <param name="format">The format</param>
        /// <param name="minWidth">The minimum width for hex and bit flag values</param>
        /// <param name="separatorGroupSize">The separator group size (4 or 8) or zero for no separators</param>
        /// <returns>The formatted enumeration value</returns>
        public static string FormatUnsignedEnumValue(string enumValue, EnumValueFormat format, int minWidth, int separatorGroupSize)
        {
            if(!UInt64.TryParse(enumValue, out ulong value))
                return enumValue;

            if(format == EnumValueFormat.IntegerValue)
                return value.ToString("N0", CultureInfo.InvariantCulture);

            string formattedValue;

            switch(value)
            {
                case ulong v when v >= Byte.MinValue && v <= Byte.MaxValue:
                    if(format == EnumValueFormat.HexValue)
                        formattedValue = $"{(byte)value:X}".PadLeft(minWidth, '0');
                    else
                        formattedValue = Convert.ToString((byte)value, 2).PadLeft(minWidth, '0');
                    break;

                case ulong v when v >= UInt16.MinValue && v <= UInt16.MaxValue:
                    if(format == EnumValueFormat.HexValue)
                        formattedValue = $"{(ushort)value:X}".PadLeft(minWidth, '0');
                    else
                        formattedValue = Convert.ToString((ushort)value, 2).PadLeft(minWidth, '0');
                    break;

                case ulong v when v >= UInt32.MinValue && v <= UInt32.MaxValue:
                    if(format == EnumValueFormat.HexValue)
                        formattedValue = $"{(uint)value:X}".PadLeft(minWidth, '0');
                    else
                        formattedValue = Convert.ToString((uint)value, 2).PadLeft(minWidth, '0');
                    break;

                default:
                    if(format == EnumValueFormat.HexValue)
                        formattedValue = $"{value:X}".PadLeft(minWidth, '0');
                    else
                    {
                        char[] bits = new char[64];

                        for(int i = 63; i >= 0; i--)
                        {
                            ulong mask = (ulong)1 << i;
                            bits[63 - i] = (value & mask) != 0 ? '1' : '0';
                        }

                        formattedValue = new String(bits);
                    }
                    break;
            }

            if(separatorGroupSize != 0 && formattedValue.Length > separatorGroupSize)
            {
                formattedValue = String.Join("_", Enumerable.Range(0, formattedValue.Length / separatorGroupSize).Select(
                    i => formattedValue.Substring(i * separatorGroupSize, separatorGroupSize)));
            }

            return ((format == EnumValueFormat.HexValue) ? "0x" : "0b") + formattedValue;
        }

        /// <summary>
        /// This converts an attribute value to a Boolean value.  If not present, blank, or invalid, it
        /// returns false.
        /// </summary>
        /// <param name="attribute">The attribute to convert</param>
        /// <returns>The attribute value if it is a Boolean or false if not</returns>
        /// <remarks>Explicit casting of an attribute to <c>bool</c> or <c>bool?</c> works but if the value is
        /// invalid or blank, the cast throws an exception.  This will return false in those cases too.</remarks>
        public static bool ToBoolean(this XAttribute attribute)
        {
            if(String.IsNullOrWhiteSpace(attribute?.Value))
                return false;

            if(!Boolean.TryParse(attribute.Value, out bool value))
                value = false;

            return value;
        }
        #endregion
    }
}
