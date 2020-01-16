//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : JavaScriptSerializer.cs
// Author  : J. Ritchie Carroll  (rcarroll@gmail.com)
// Updated : 02/05/2016
// Note    : Copyright 2007-2020, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// These namespaces and classes only exist to satisfy compiler from within a .NET core compilation.
//
// Design Decision:
//    These classes do not not exist in .NET core, so this just adds the types to make project compile.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/15/2020  JRC  Created the code
//
//===============================================================================================================

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace SandcastleBuilder.Utils.Design
{
    [CompilerGenerated]
    internal class FilePathObjectEditor { }

    [CompilerGenerated]
    internal class FilePathTypeConverter { }
    
    [CompilerGenerated]
    internal class FilePathStringEditor { }

    [CompilerGenerated]
    internal class FolderPathObjectEditor { }

    [CompilerGenerated]
    internal class FolderPathTypeConverter { }
    
    [CompilerGenerated]
    internal class FolderPathStringEditor { }
}

namespace System.Drawing.Design
{
    [CompilerGenerated]
    internal class UITypeEditor { }
}

namespace System.Windows.Forms
{
    [CompilerGenerated]
    internal enum MessageBoxButtons
    {
        OK,
        OKCancel,
        AbortRetryIgnore,
        YesNoCancel,
        YesNo,
        RetryCancel
    }

    [CompilerGenerated]
    internal enum MessageBoxIcon
    {
        None,
        Hand,
        Question,
        Exclamation,
        Asterisk,
        Stop,
        Erro,
        Warning,
        Information
    }

    [CompilerGenerated]
    internal static class MessageBox
    {
        public static void Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Console.WriteLine($"[{caption}: {text}");
        }
    }
}

namespace Microsoft.Build.BuildEngine
{
    // This code derived from MIT licensed source:
    // https://github.com/microsoft/msbuild/blob/master/src/Shared/EscapingUtilities.cs
    [CompilerGenerated]
    internal static class Utilities
    {
        public static string Escape(string value)
        {
            if (String.IsNullOrEmpty(value) || !ContainsReservedCharacters(value))
                return value;

            StringBuilder escapedStringBuilder = new StringBuilder();
            AppendEscapedString(escapedStringBuilder, value);
            return escapedStringBuilder.ToString();
        }

        private static bool IsHexDigit(char character)
        {
            return character >= '0' && character <= '9'
                || character >= 'A' && character <= 'F'
                || character >= 'a' && character <= 'f';
        }

        private static bool ContainsReservedCharacters(string unescapedString)
        {
            return -1 != unescapedString.IndexOfAny(s_charsToEscape);
        }

        private static char HexDigitChar(int x)
        {
            return (char)(x + (x < 10 ? '0' : 'a' - 10));
        }

        private static void AppendEscapedChar(StringBuilder sb, char ch)
        {
            // Append the escaped version which is a percent sign followed by two hexadecimal digits
            sb.Append('%');
            sb.Append(HexDigitChar(ch / 0x10));
            sb.Append(HexDigitChar(ch & 0x0F));
        }

        private static void AppendEscapedString(StringBuilder sb, string unescapedString)
        {
            // Replace each unescaped special character with an escape sequence one
            for (int idx = 0; ;)
            {
                int nextIdx = unescapedString.IndexOfAny(s_charsToEscape, idx);

                if(nextIdx == -1)
                {
                    sb.Append(unescapedString, idx, unescapedString.Length - idx);
                    break;
                }

                sb.Append(unescapedString, idx, nextIdx - idx);
                AppendEscapedChar(sb, unescapedString[nextIdx]);
                idx = nextIdx + 1;
            }
        }

        private static readonly char[] s_charsToEscape = { '%', '*', '?', '@', '$', '(', ')', ';', '\'' };
    }
}