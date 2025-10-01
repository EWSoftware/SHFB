// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 11/22/2013 - EFW - Cleared out the conditional statements
// 12/16/2013 - EFW - Added hack to work around a bug when parsing .NETCore 4.5 assemblies.

// Ignore Spelling: Substring

using System.Diagnostics;
using System.IO;
using System.Text;

using Microsoft.Win32.SafeHandles;

namespace System.Compiler.Metadata
{
    unsafe internal sealed class MemoryCursor
    {
        private readonly byte* buffer;
        private byte* pb;
        readonly internal int Length;

        internal MemoryCursor(MemoryMappedFile/*!*/ memoryMap) : this(memoryMap.Buffer, memoryMap.Length)
        {
        }

        internal MemoryCursor(byte* buffer, int length, int position)
        {
            this.buffer = buffer;
            this.pb = buffer + position;
            this.Length = length;
        }
        internal MemoryCursor(byte* buffer, int length)
            : this(buffer, length, 0)
        {
        }
        internal MemoryCursor(MemoryCursor/*!*/ c)
        {
            this.buffer = c.buffer;
            this.pb = c.pb;
            this.Length = c.Length;
        }

        internal byte* GetBuffer()
        {
            return this.buffer;
        }

        internal int Position
        {
            get => (int)(this.pb - this.buffer);
            set => this.pb = this.buffer + value;
        }
        internal void Align(int size)
        {
            Debug.Assert(size == 2 || size == 4 || size == 8 || size == 16 || size == 32 || size == 64);
            int remainder = Position & (size - 1);
            if (remainder != 0)
                pb += size - remainder;
        }

        ////internal System.Char Char(int i){ return *(System.Char*)(this.pb+i*sizeof(System.Char)); }
        ////internal System.SByte SByte(int i){ return *(System.SByte*)(this.pb+i*sizeof(System.SByte)); }
        internal System.Int16 Int16(int i) { return *(System.Int16*)(this.pb + i * sizeof(System.Int16)); }
        internal System.Int32 Int32(int i) { return *(System.Int32*)(this.pb + i * sizeof(System.Int32)); }
        ////internal System.Int64 Int64(int i){ return *(System.Int64*)(this.pb+i*sizeof(System.Int64)); }
        internal System.Byte Byte(int i) { return *(System.Byte*)(this.pb + i * sizeof(System.Byte)); }
        internal System.UInt16 UInt16(int i) { return *(System.UInt16*)(this.pb + i * sizeof(System.UInt16)); }
        ////internal System.UInt32 UInt32(int i){ return *(System.UInt32*)(this.pb+i*sizeof(System.UInt32)); }
        ////internal System.UInt64 UInt64(int i){ return *(System.UInt64*)(this.pb+i*sizeof(System.UInt64)); }
        ////internal System.Boolean Boolean(int i){ return *(System.Boolean*)(this.pb+i*sizeof(System.Boolean)); }
        ////internal System.Single Single(int i){ return *(System.Single*)(this.pb+i*sizeof(System.Single)); }
        ////internal System.Double Double(int i){ return *(System.Double*)(this.pb+i*sizeof(System.Double)); }

        ////internal void SkipChar(int c){ this.pb += c*sizeof(System.Char); }
        ////internal void SkipSByte(int c){ this.pb += c*sizeof(System.SByte); }
        internal void SkipInt16(int c) { this.pb += c * sizeof(System.Int16); }
        internal void SkipInt32(int c) { this.pb += c * sizeof(System.Int32); }
        ////internal void SkipInt64(int c){ this.pb += c*sizeof(System.Int64); }
        internal void SkipByte(int c) { this.pb += c * sizeof(System.Byte); }
        internal void SkipUInt16(int c) { this.pb += c * sizeof(System.UInt16); }
        ////internal void SkipUInt32(int c){ this.pb += c*sizeof(System.UInt32); }
        ////internal void SkipUInt64(int c){ this.pb += c*sizeof(System.UInt64); }
        ////internal void SkipBoolean(int c){ this.pb += c*sizeof(System.Boolean); }
        ////internal void SkipSingle(int c){ this.pb += c*sizeof(System.Single); }
        ////internal void SkipDouble(int c){ this.pb += c*sizeof(System.Double); }

        internal System.Char ReadChar() { byte* pb = this.pb; System.Char v = *(System.Char*)pb; this.pb = pb + sizeof(System.Char); return v; }
        internal System.SByte ReadSByte() { byte* pb = this.pb; System.SByte v = *(System.SByte*)pb; this.pb = pb + sizeof(System.SByte); return v; }
        internal System.Int16 ReadInt16() { byte* pb = this.pb; System.Int16 v = *(System.Int16*)pb; this.pb = pb + sizeof(System.Int16); return v; }
        internal System.Int32 ReadInt32() { byte* pb = this.pb; System.Int32 v = *(System.Int32*)pb; this.pb = pb + sizeof(System.Int32); return v; }
        internal System.Int64 ReadInt64() { byte* pb = this.pb; System.Int64 v = *(System.Int64*)pb; this.pb = pb + sizeof(System.Int64); return v; }
        internal System.Byte ReadByte() { byte* pb = this.pb; System.Byte v = *(System.Byte*)pb; this.pb = pb + sizeof(System.Byte); return v; }
        internal System.UInt16 ReadUInt16() { byte* pb = this.pb; System.UInt16 v = *(System.UInt16*)pb; this.pb = pb + sizeof(System.UInt16); return v; }
        internal System.UInt32 ReadUInt32() { byte* pb = this.pb; System.UInt32 v = *(System.UInt32*)pb; this.pb = pb + sizeof(System.UInt32); return v; }
        internal System.UInt64 ReadUInt64() { byte* pb = this.pb; System.UInt64 v = *(System.UInt64*)pb; this.pb = pb + sizeof(System.UInt64); return v; }
        internal System.Boolean ReadBoolean() { byte* pb = this.pb; System.Boolean v = *(System.Boolean*)pb; this.pb = pb + sizeof(System.Boolean); return v; }
        internal System.Single ReadSingle() { byte* pb = this.pb; System.Single v = *(System.Single*)pb; this.pb = pb + sizeof(System.Single); return v; }
        internal System.Double ReadDouble() { byte* pb = this.pb; System.Double v = *(System.Double*)pb; this.pb = pb + sizeof(System.Double); return v; }

        internal System.Int32 ReadReference(int refSize)
        {
            if (refSize == 2) return ReadUInt16();
            return ReadInt32();
        }

        internal int ReadCompressedInt()
        {
            byte headerByte = ReadByte();
            int result;
            if ((headerByte & 0x80) == 0x00)
                result = headerByte;
            else if ((headerByte & 0x40) == 0x00)
                result = ((headerByte & 0x3f) << 8) | ReadByte();
            else if (headerByte == 0xFF)
                result = -1;
            else
                result = ((headerByte & 0x3f) << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte();
            return result;
        }

        internal byte[]/*!*/ ReadBytes(int c)
        {
            byte[] result = new byte[c];
            byte* pb = this.pb;
            for (int i = 0; i < c; i++)
                result[i] = *pb++;
            this.pb = pb;
            return result;
        }

        internal unsafe Identifier/*!*/ ReadIdentifierFromSerString()
        {
            byte* pb = this.pb;
            byte headerByte = *pb++;
            uint length = 0;
            if ((headerByte & 0x80) == 0x00)
                length = headerByte;
            else if ((headerByte & 0x40) == 0x00)
                length = (uint)((headerByte & 0x3f) << 8) | *pb++;
            else
                length = (uint)((headerByte & 0x3f) << 24) | (uint)(*pb++ << 16) | (uint)(*pb++ << 8) | (*pb++);
            this.pb = pb + length;
            return Identifier.For(pb, length/*, this.KeepAlive*/);
        }

        internal string/*!*/ ReadUTF8(int bytesToRead)
        {
            // !EFW - Hack bug fix.  When parsing .NETCore 4.5 assemblies, the offset is off in some cases on
            // attribute names.  Not sure why.  It seems to be consistent so we'll just adjust for it and hope
            // for the best.
            if(bytesToRead > 32767)
            {
                bytesToRead = 0;

                this.pb += 5;

                while(*(this.pb + bytesToRead) > '\x1F' && bytesToRead < 256)
                    bytesToRead++;
            }

            char[] buffer = new char[bytesToRead];
            byte* pb = this.pb;
            this.pb += bytesToRead;
            int j = 0;

            while(bytesToRead > 0)
            {
                byte b = *pb++; bytesToRead--;

                if((b & 0x80) == 0 || bytesToRead == 0)
                {
                    buffer[j++] = (char)b;
                    continue;
                }

                char ch;
                byte b1 = *pb++; bytesToRead--;

                if((b & 0x20) == 0)
                    ch = (char)(((b & 0x1F) << 6) | (b1 & 0x3F));
                else
                {
                    if(bytesToRead == 0)
                    {
                        //Dangling lead bytes, do not decompose
                        buffer[j++] = (char)((b << 8) | b1);
                        break;
                    }

                    byte b2 = *pb++; bytesToRead--;
                    uint ch32;

                    if((b & 0x10) == 0)
                        ch32 = (uint)(((b & 0x0F) << 12) | ((b1 & 0x3F) << 6) | (b2 & 0x3F));
                    else
                    {
                        if(bytesToRead == 0)
                        {
                            //Dangling lead bytes, do not decompose
                            buffer[j++] = (char)((b << 8) | b1);
                            buffer[j++] = (char)b2;
                            break;
                        }

                        byte b3 = *pb++; bytesToRead--;
                        ch32 = (uint)(((b & 0x07) << 18) | ((b1 & 0x3F) << 12) | ((b2 & 0x3F) << 6) | (b3 & 0x3F));
                    }

                    if((ch32 & 0xFFFF0000) == 0)
                        ch = (char)ch32;
                    else
                    {
                        //break up into UTF16 surrogate pair
                        buffer[j++] = (char)((ch32 >> 10) | 0xD800);
                        ch = (char)((ch32 & 0x3FF) | 0xDC00);
                    }
                }
                buffer[j++] = ch;
            }

            if(j > 0 && buffer[j - 1] == 0)
                j--;

            return new String(buffer, 0, j);
        }

        internal string/*!*/ ReadUTF8()
        {
            byte* pb = this.pb;
            StringBuilder sb = new();
            byte b = 0;
            for (; ; )
            {
                b = *pb++;
                if (b == 0) break;
                if ((b & 0x80) == 0)
                {
                    sb.Append((char)b);
                    continue;
                }
                char ch;
                byte b1 = *pb++;
                if (b1 == 0)
                { //Dangling lead byte, do not decompose
                    sb.Append((char)b);
                    break;
                }
                if ((b & 0x20) == 0)
                {
                    ch = (char)(((b & 0x1F) << 6) | (b1 & 0x3F));
                }
                else
                {
                    byte b2 = *pb++;
                    if (b2 == 0)
                    { //Dangling lead bytes, do not decompose
                        sb.Append((char)((b << 8) | b1));
                        break;
                    }
                    uint ch32;
                    if ((b & 0x10) == 0)
                        ch32 = (uint)(((b & 0x0F) << 12) | ((b1 & 0x3F) << 6) | (b2 & 0x3F));
                    else
                    {
                        byte b3 = *pb++;
                        if (b3 == 0)
                        { //Dangling lead bytes, do not decompose
                            sb.Append((char)((b << 8) | b1));
                            sb.Append((char)b2);
                            break;
                        }
                        ch32 = (uint)(((b & 0x07) << 18) | ((b1 & 0x3F) << 12) | ((b2 & 0x3F) << 6) | (b3 & 0x3F));
                    }
                    if ((ch32 & 0xFFFF0000) == 0)
                        ch = (char)ch32;
                    else
                    { //break up into UTF16 surrogate pair
                        sb.Append((char)((ch32 >> 10) | 0xD800));
                        ch = (char)((ch32 & 0x3FF) | 0xDC00);
                    }
                }
                sb.Append(ch);
            }
            this.pb = pb;
            return sb.ToString();
        }

        internal string/*!*/ ReadUTF16(int charsToRead)
        {
            char* pc = (char*)this.pb;
            char[] buffer = new char[charsToRead];
            for (int i = 0; i < charsToRead; i++)
                buffer[i] = *pc++;
            this.pb = (byte*)pc;
            return new String(buffer, 0, charsToRead);
        }

        internal string/*!*/ ReadUTF16()
        {
            string result = new((char*)this.pb);
            this.pb += (result.Length + 1) * 2;
            return result;
        }

        internal string/*!*/ ReadASCII(int bytesToRead)
        {
            int c = bytesToRead;
            if (bytesToRead == -1) c = 128; //buffer size
            byte* pb = this.pb;
            char[] buffer = new char[c];
            int j = 0;
            byte b = 0;
        Restart:
            while (j < c)
            {
                b = *pb++;
                if (b == 0) break;
                buffer[j++] = (char)b;
            }
            if (bytesToRead == -1)
            {
                if (b != 0)
                {
                    char[] newBuffer = new char[c *= 2];
                    for (int copy = 0; copy < j; copy++)
                        newBuffer[copy] = buffer[copy];
                    buffer = newBuffer;
                    goto Restart;
                }
                this.pb = pb;
            }
            else
                this.pb += bytesToRead;
            return new String(buffer, 0, j);
        }

        internal string/*!*/ ReadASCII() { return ReadASCII(-1); }
    }


    /// <summary>
    /// Public only for use by the Framework. Do not use this class.
    /// Well, if you really, really must, use it only if you can tolerate keeping the file locked for at least as long as any Identifier
    /// derived from the file stays alive.
    /// </summary>   
    unsafe public sealed class MemoryMappedFile : IDisposable, ISourceTextBuffer
    {
        private readonly byte* buffer;
        private readonly int length;
        private SafeMemoryMappedViewHandle handle;
        private bool success;

        public MemoryMappedFile(string fileName)
        {
            // Using .NET 6.0+ MemoryMappedFile APIs.  We need to use a file stream with shared read access since
            // the assembly may be open in another process.
            using var mmfStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using IO.MemoryMappedFiles.MemoryMappedFile mmf = IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(
                mmfStream, null, 0, IO.MemoryMappedFiles.MemoryMappedFileAccess.Read, HandleInheritability.None, false);
            using IO.MemoryMappedFiles.MemoryMappedViewStream viewStream = mmf.CreateViewStream(0, 0, IO.MemoryMappedFiles.MemoryMappedFileAccess.Read);

            handle = viewStream.SafeMemoryMappedViewHandle;

            // use the underlying handle to access the memory (note this will throw an exception if the handle is invalid)
            handle.DangerousAddRef(ref success);
            buffer = (byte*)viewStream.SafeMemoryMappedViewHandle.DangerousGetHandle();
            length = (int)viewStream.Length;
        }

        ~MemoryMappedFile()
        {
            this.CloseMap();
        }

        public void Dispose()
        {
            this.CloseMap();
            GC.SuppressFinalize(this);
        }

        public byte* Buffer
        {
            get
            {
                Debug.Assert(this.buffer != null);
                return this.buffer;
            }
        }
        
        public int Length
        {
            get
            {
                Debug.Assert(this.buffer != null);
                return this.length;
            }
        }

        string ISourceText.Substring(int start, int length)
        {
            Debug.Assert(false, "Can't use Substring on memory mapped files");
            return null;
        }
        char ISourceText.this[int index]
        {
            get
            {
                Debug.Assert(false, "Can't access memory mapped files via an indexer, use Buffer");
                return ' ';
            }
        }

        private void CloseMap()
        {
            // clean up the memory mapped file handle if successfully created
            if(success)
            {
                handle.DangerousRelease();
                handle.Dispose();
                handle = null;
                success = false;
            }
        }

        void ISourceText.MakeCollectible()
        {
        }
    }
}
