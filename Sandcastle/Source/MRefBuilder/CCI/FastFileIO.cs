// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Diagnostics;
#if CCINamespace
using Microsoft.Cci.Metadata;
#else
using System.Compiler.Metadata;
#endif
using System.Globalization;
using System.Text;

#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler
{
#endif
#if !(FxCop || NoWriter)
    /// <summary>
    /// High performance replacement for System.IO.BinaryWriter.
    /// </summary>
    public sealed class BinaryWriter
    {
        public MemoryStream/*!*/ BaseStream;
        private bool UTF8 = true;
        public BinaryWriter(MemoryStream/*!*/ output)
        {
            this.BaseStream = output;
            //^ base();
        }
        public BinaryWriter(MemoryStream/*!*/ output, Encoding/*!*/ encoding)
        {
            Debug.Assert(encoding == Encoding.Unicode);
            this.BaseStream = output;
            this.UTF8 = false;
            //^ base();
        }
        public void Write(bool value)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            m.Position = i + 1;
            m.Buffer[i] = (byte)(value ? 1 : 0);
        }
        public void Write(byte value)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            m.Position = i + 1;
            m.Buffer[i] = value;
        }
        public void Write(sbyte value)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            m.Position = i + 1;
            m.Buffer[i] = (byte)value;
        }
        public void Write(byte[] buffer)
        {
            if (buffer == null) return;
            this.BaseStream.Write(buffer, 0, buffer.Length);
        }
        public void Write(char ch)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            if (this.UTF8)
            {
                if (ch < 0x80)
                {
                    m.Position = i + 1;
                    m.Buffer[i] = (byte)ch;
                }
                else
                    this.Write(new char[] { ch });
            }
            else
            {
                m.Position = i + 2;
                byte[] buffer = m.Buffer;
                buffer[i++] = (byte)ch;
                buffer[i] = (byte)(ch >> 8);
            }
        }
        public void Write(char[] chars)
        {
            if (chars == null) return;
            MemoryStream m = this.BaseStream;
            int n = chars.Length;
            int i = m.Position;
            if (this.UTF8)
            {
                m.Position = i + n;
                byte[] buffer = m.Buffer;
                for (int j = 0; j < n; j++)
                {
                    char ch = chars[j];
                    if ((ch & 0x80) != 0) goto writeUTF8;
                    buffer[i++] = (byte)ch;
                }
                return;
            writeUTF8:
                int ch32 = 0;
                for (int j = n - (m.Position - i); j < n; j++)
                {
                    char ch = chars[j];
                    if (ch < 0x80)
                    {
                        m.Position = i + 1;
                        buffer = m.Buffer;
                        buffer[i++] = (byte)ch;
                    }
                    else if (ch < 0x800)
                    {
                        m.Position = i + 2;
                        buffer = m.Buffer;
                        buffer[i++] = (byte)(((ch >> 6) & 0x1F) | 0xC0);
                        buffer[i] = (byte)((ch & 0x3F) | 0x80);
                    }
                    else if (0xD800 <= ch && ch <= 0xDBFF)
                    {
                        ch32 = (ch & 0x3FF) << 10;
                    }
                    else if (0xDC00 <= ch && ch <= 0xDFFF)
                    {
                        ch32 |= ch & 0x3FF;
                        m.Position = i + 4;
                        buffer = m.Buffer;
                        buffer[i++] = (byte)(((ch32 >> 18) & 0x7) | 0xF0);
                        buffer[i++] = (byte)(((ch32 >> 12) & 0x3F) | 0x80);
                        buffer[i++] = (byte)(((ch32 >> 6) & 0x3F) | 0x80);
                        buffer[i] = (byte)((ch32 & 0x3F) | 0x80);
                    }
                    else
                    {
                        m.Position = i + 3;
                        buffer = m.Buffer;
                        buffer[i++] = (byte)(((ch >> 12) & 0xF) | 0xE0);
                        buffer[i++] = (byte)(((ch >> 6) & 0x3F) | 0x80);
                        buffer[i] = (byte)((ch & 0x3F) | 0x80);
                    }
                }
            }
            else
            {
                m.Position = i + n * 2;
                byte[] buffer = m.Buffer;
                for (int j = 0; j < n; j++)
                {
                    char ch = chars[j];
                    buffer[i++] = (byte)ch;
                    buffer[i++] = (byte)(ch >> 8);
                }
            }
        }
        public unsafe void Write(double value)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            m.Position = i + 8;
            fixed (byte* b = m.Buffer)
                *((double*)(b + i)) = value;
        }
        public void Write(short value)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            m.Position = i + 2;
            byte[] buffer = m.Buffer;
            buffer[i++] = (byte)value;
            buffer[i] = (byte)(value >> 8);
        }
        public unsafe void Write(ushort value)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            m.Position = i + 2;
            byte[] buffer = m.Buffer;
            buffer[i++] = (byte)value;
            buffer[i] = (byte)(value >> 8);
        }
        public void Write(int value)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            m.Position = i + 4;
            byte[] buffer = m.Buffer;
            buffer[i++] = (byte)value;
            buffer[i++] = (byte)(value >> 8);
            buffer[i++] = (byte)(value >> 16);
            buffer[i] = (byte)(value >> 24);
        }
        public void Write(uint value)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            m.Position = i + 4;
            byte[] buffer = m.Buffer;
            buffer[i++] = (byte)value;
            buffer[i++] = (byte)(value >> 8);
            buffer[i++] = (byte)(value >> 16);
            buffer[i] = (byte)(value >> 24);
        }
        public void Write(long value)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            m.Position = i + 8;
            byte[] buffer = m.Buffer;
            uint lo = (uint)value;
            uint hi = (uint)(value >> 32);
            buffer[i++] = (byte)lo;
            buffer[i++] = (byte)(lo >> 8);
            buffer[i++] = (byte)(lo >> 16);
            buffer[i++] = (byte)(lo >> 24);
            buffer[i++] = (byte)hi;
            buffer[i++] = (byte)(hi >> 8);
            buffer[i++] = (byte)(hi >> 16);
            buffer[i] = (byte)(hi >> 24);
        }
        public unsafe void Write(ulong value)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            m.Position = i + 8;
            byte[] buffer = m.Buffer;
            uint lo = (uint)value;
            uint hi = (uint)(value >> 32);
            buffer[i++] = (byte)lo;
            buffer[i++] = (byte)(lo >> 8);
            buffer[i++] = (byte)(lo >> 16);
            buffer[i++] = (byte)(lo >> 24);
            buffer[i++] = (byte)hi;
            buffer[i++] = (byte)(hi >> 8);
            buffer[i++] = (byte)(hi >> 16);
            buffer[i] = (byte)(hi >> 24);
        }
        public unsafe void Write(float value)
        {
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            m.Position = i + 4;
            fixed (byte* b = m.Buffer)
                *((float*)(b + i)) = value;
        }
        public void Write(string str)
        {
            this.Write(str, false);
        }
        public void Write(string str, bool emitNullTerminator)
        {
            if (str == null)
            {
                Debug.Assert(!emitNullTerminator);
                this.Write((byte)0xff);
                return;
            }
            int n = str.Length;
            if (!emitNullTerminator)
            {
                if (this.UTF8)
                    Ir2md.WriteCompressedInt(this, this.GetUTF8ByteCount(str));
                else
                    Ir2md.WriteCompressedInt(this, n * 2);
            }
            MemoryStream m = this.BaseStream;
            int i = m.Position;
            if (this.UTF8)
            {
                m.Position = i + n;
                byte[] buffer = m.Buffer;
                for (int j = 0; j < n; j++)
                {
                    char ch = str[j];
                    if (ch >= 0x80) goto writeUTF8;
                    buffer[i++] = (byte)ch;
                }
                if (emitNullTerminator)
                {
                    m.Position = i + 1;
                    buffer = m.Buffer;
                    buffer[i] = 0;
                }
                return;
            writeUTF8:
                int ch32 = 0;
                for (int j = n - (m.Position - i); j < n; j++)
                {
                    char ch = str[j];
                    if (ch < 0x80)
                    {
                        m.Position = i + 1;
                        buffer = m.Buffer;
                        buffer[i++] = (byte)ch;
                    }
                    else if (ch < 0x800)
                    {
                        m.Position = i + 2;
                        buffer = m.Buffer;
                        buffer[i++] = (byte)(((ch >> 6) & 0x1F) | 0xC0);
                        buffer[i++] = (byte)((ch & 0x3F) | 0x80);
                    }
                    else if (0xD800 <= ch && ch <= 0xDBFF)
                    {
                        ch32 = (ch & 0x3FF) << 10;
                    }
                    else if (0xDC00 <= ch && ch <= 0xDFFF)
                    {
                        ch32 |= ch & 0x3FF;
                        m.Position = i + 4;
                        buffer = m.Buffer;
                        buffer[i++] = (byte)(((ch32 >> 18) & 0x7) | 0xF0);
                        buffer[i++] = (byte)(((ch32 >> 12) & 0x3F) | 0x80);
                        buffer[i++] = (byte)(((ch32 >> 6) & 0x3F) | 0x80);
                        buffer[i++] = (byte)((ch32 & 0x3F) | 0x80);
                    }
                    else
                    {
                        m.Position = i + 3;
                        buffer = m.Buffer;
                        buffer[i++] = (byte)(((ch >> 12) & 0xF) | 0xE0);
                        buffer[i++] = (byte)(((ch >> 6) & 0x3F) | 0x80);
                        buffer[i++] = (byte)((ch & 0x3F) | 0x80);
                    }
                }
                if (emitNullTerminator)
                {
                    m.Position = i + 1;
                    buffer = m.Buffer;
                    buffer[i] = 0;
                }
            }
            else
            {
                m.Position = i + n * 2;
                byte[] buffer = m.Buffer;
                for (int j = 0; j < n; j++)
                {
                    char ch = str[j];
                    buffer[i++] = (byte)ch;
                    buffer[i++] = (byte)(ch >> 8);
                }
                if (emitNullTerminator)
                {
                    m.Position = i + 2;
                    buffer = m.Buffer;
                    buffer[i++] = 0;
                    buffer[i] = 0;
                }
            }
        }
        public int GetUTF8ByteCount(string str)
        {
            int count = 0;
            for (int i = 0, n = str == null ? 0 : str.Length; i < n; i++)
            {
                //^ assume str != null;
                char ch = str[i];
                if (ch < 0x80)
                {
                    count += 1;
                }
                else if (ch < 0x800)
                {
                    count += 2;
                }
                else if (0xD800 <= ch && ch <= 0xDBFF)
                {
                    count += 2;
                }
                else if (0xDC00 <= ch && ch <= 0xDFFF)
                {
                    count += 2;
                }
                else
                {
                    count += 3;
                }
            }
            return count;
        }
    }
    public sealed class MemoryStream
    {
        public byte[]/* ! */ Buffer;
        public int Length;
        public int position;
        public int Position
        {
            get { return this.position; }
            set
            {
                byte[] myBuffer = this.Buffer;
                int n = myBuffer.Length;
                if (value >= n) this.Grow(myBuffer, n, value);
                if (value > this.Length) this.Length = value;
                this.position = value;
            }
        }
        public MemoryStream()
        {
            this.Buffer = new byte[64];
            this.Length = 0;
            this.position = 0;
            //^ base();
        }
        public MemoryStream(byte[]/*!*/ bytes)
        {
            if (bytes == null) { Debug.Fail(""); }
            this.Buffer = bytes;
            this.Length = bytes.Length;
            this.position = 0;
            //^ base();
        }
        private void Grow(byte[]/*!*/ myBuffer, int n, int m)
        {
            if (myBuffer == null) { Debug.Fail(""); return; }
            int n2 = n * 2;
            while (m >= n2) n2 = n2 * 2;
            byte[] newBuffer = this.Buffer = new byte[n2];
            for (int i = 0; i < n; i++)
                newBuffer[i] = myBuffer[i]; //TODO: optimize this
        }
        public void Seek(long offset, SeekOrigin loc)
        {
            Debug.Assert(loc == SeekOrigin.Begin);
            Debug.Assert(offset <= int.MaxValue);
            this.Position = (int)offset;
        }
        public byte[]/*!*/ ToArray()
        {
            int n = this.Length;
            byte[] source = this.Buffer;
            if (source.Length == n) return this.Buffer; //unlikely, but the check is cheap
            byte[] result = new byte[n];
            for (int i = 0; i < n; i++)
                result[i] = source[i]; //TODO: optimize this
            return result;
        }
        public void Write(byte[]/*!*/ buffer, int index, int count)
        {
            int p = this.position;
            this.Position = p + count;
            byte[] myBuffer = this.Buffer;
            for (int i = 0, j = p, k = index; i < count; i++)
                myBuffer[j++] = buffer[k++]; //TODO: optimize this
        }
        public void WriteTo(MemoryStream/*!*/ stream)
        {
            stream.Write(this.Buffer, 0, this.Length);
        }
        public void WriteTo(System.IO.Stream/*!*/ stream)
        {
            stream.Write(this.Buffer, 0, this.Length);
        }
    }
    public enum SeekOrigin { Begin, Current, End }
#endif
    /// <summary>
    /// A version of System.IO.Path that does not throw exceptions.
    /// </summary>
#if FxCop || NoWriter
  internal sealed class BetterPath {
#else
    public sealed class BetterPath
    {
#endif
        public static readonly char AltDirectorySeparatorChar = System.IO.Path.AltDirectorySeparatorChar;
        public static readonly char DirectorySeparatorChar = System.IO.Path.DirectorySeparatorChar;
        public static readonly char VolumeSeparatorChar = System.IO.Path.VolumeSeparatorChar;

        public static string ChangeExtension(string path, string extension)
        {
            if (path == null) return null;
            string text1 = path;
            int num1 = path.Length;
            while (--num1 >= 0)
            {
                char ch1 = path[num1];
                if (ch1 == '.')
                {
                    text1 = path.Substring(0, num1);
                    break;
                }
                if (((ch1 == BetterPath.DirectorySeparatorChar) || (ch1 == BetterPath.AltDirectorySeparatorChar)) || (ch1 == BetterPath.VolumeSeparatorChar))
                    break;
            }
            if (extension == null || path.Length == 0) return text1;
            if (extension.Length == 0 || extension[0] != '.')
                text1 = text1 + ".";
            return text1 + extension;
        }
        public static string Combine(string path1, string path2)
        {
            if (path1 == null || path1.Length == 0) return path2;
            if (path2 == null || path2.Length == 0) return path1;
            char ch = path2[0];
            if (ch == BetterPath.DirectorySeparatorChar || ch == BetterPath.AltDirectorySeparatorChar || (path2.Length >= 2 && path2[1] == BetterPath.VolumeSeparatorChar))
                return path2;
            ch = path1[path1.Length - 1];
            if (ch != BetterPath.DirectorySeparatorChar && ch != BetterPath.AltDirectorySeparatorChar && ch != BetterPath.VolumeSeparatorChar)
                return (path1 + BetterPath.DirectorySeparatorChar + path2);
            return path1 + path2;
        }
        public static string GetExtension(string path)
        {
            if (path == null) return null;
            int length = path.Length;
            for (int i = length; --i >= 0; )
            {
                char ch = path[i];
                if (ch == '.')
                {
                    if (i != length - 1)
                        return path.Substring(i, length - i);
                    else
                        return String.Empty;
                }
                if (ch == BetterPath.DirectorySeparatorChar || ch == BetterPath.AltDirectorySeparatorChar || ch == BetterPath.VolumeSeparatorChar)
                    break;
            }
            return string.Empty;
        }
        public static String GetFileName(string path)
        {
            if (path == null) return null;
            int length = path.Length;
            for (int i = length; --i >= 0; )
            {
                char ch = path[i];
                if (ch == BetterPath.DirectorySeparatorChar || ch == BetterPath.AltDirectorySeparatorChar || ch == BetterPath.VolumeSeparatorChar)
                    return path.Substring(i + 1);
            }
            return path;
        }
        public static string GetFileNameWithoutExtension(string path)
        {
            int num1;
            path = BetterPath.GetFileName(path);
            if (path == null) return null;
            if ((num1 = path.LastIndexOf('.')) == -1) return path;
            return path.Substring(0, num1);
        }
        public static String GetDirectoryName(string path)
        {
            if (path == null) return null;
            int length = path.Length;
            for (int i = length; --i >= 0; )
            {
                char ch = path[i];
                if (ch == BetterPath.DirectorySeparatorChar || ch == BetterPath.AltDirectorySeparatorChar || ch == BetterPath.VolumeSeparatorChar)
                    return path.Substring(0, i);
            }
            return path;
        }
        public static char[] GetInvalidFileNameChars()
        {
#if WHIDBEY
            return System.IO.Path.GetInvalidFileNameChars();
#else
      return System.IO.Path.InvalidPathChars;
#endif
        }
        public static char[] GetInvalidPathChars()
        {
#if WHIDBEY
            return System.IO.Path.GetInvalidPathChars();
#else
      return System.IO.Path.InvalidPathChars;
#endif
        }
        public static string GetTempFileName()
        {
            return System.IO.Path.GetTempFileName();
        }
        public static bool HasExtension(string path)
        {
            if (path != null)
            {
                int num1 = path.Length;
                while (--num1 >= 0)
                {
                    char ch1 = path[num1];
                    if (ch1 == '.')
                    {
                        if (num1 != (path.Length - 1))
                        {
                            return true;
                        }
                        return false;
                    }
                    if (((ch1 == BetterPath.DirectorySeparatorChar) || (ch1 == BetterPath.AltDirectorySeparatorChar)) || (ch1 == BetterPath.VolumeSeparatorChar))
                    {
                        break;
                    }
                }
            }
            return false;
        }
    }
}
