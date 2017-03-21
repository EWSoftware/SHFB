// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/03/2012 - EFW - Fixed Method.ImplicitlyImplementedInterfaceMethods so that it recognizes interface
// member matches when the return type is generic.
// 03/28/2012 - EFW - Fixed TypeNode.Attributes so that it won't get stuck in an endless loop if a type's
// attribute references the type being parsed.
// 04/04/2012 - EFW - Fixed TypeNode.NestedTypes so that it won't get stuck in an endless loop when a type
// contains a nested type that itself implements a nested type from within the containing type.
// 11/30/2012 - EFW - Added updates based on changes submitted by ComponentOne to fix crashes caused by
// obfuscated member names.
// 11/21/2013 - EFW - Cleared out the conditional statements and unused code and updated based on changes to
// ListTemplate.cs.
// 03/14/2014 - EFW - Fixed bug in TypeNode.GetMatchingMethod() reported by SHarwell.
// 08/23/2016 - EFW - Added support for reading source code context from PDB files

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Compiler.Metadata;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

using BindingFlags = System.Reflection.BindingFlags;

namespace System.Compiler
{
    /// <summary>
    /// This interface can be used to link an arbitrary source text provider into an IR tree via a DocumentText instance.
    /// </summary>
    public interface ISourceText
    {
        /// <summary>
        /// The number of characters in the source text. 
        /// A "character" corresponds to a System.Char which is actually a Unicode UTF16 code point to be precise.
        /// </summary>
        int Length { get; }
        /// <summary>
        /// Retrieves a substring from this instance. The substring starts with the character at the specified index and has a specified length.
        /// </summary>
        string Substring(int startIndex, int length);
        /// <summary>
        /// Retrieves the character at the given position. The first character is at position zero.
        /// </summary>
        char this[int position] { get; }
        /// <summary>
        /// Indicates that the text has been fully scanned and further references to the text are expected to be infrequent.
        /// The underlying object can now choose to clear cached information if it comes under resource pressure.
        /// </summary>
        void MakeCollectible();
    }

    public unsafe interface ISourceTextBuffer : ISourceText
    {
        /// <summary>
        /// Returns null unless the implementer is based on an ASCII buffer that stays alive as long at the implementer itself.
        /// An implementer that returns a non-null value is merely a wrapper to keep the buffer alive. No further methods will
        /// be called on the interface in this case.
        /// </summary>
        byte* Buffer { get; }
    }

    /// <summary>
    /// This class provides a uniform interface to program sources provided in the form of Unicode strings,
    /// unsafe pointers to ASCII buffers (as obtained from a memory mapped file, for instance) as well as
    /// arbitrary source text providers that implement the ISourceText interface.
    /// </summary>
    public sealed unsafe class DocumentText
    {
        /// <summary>
        /// If this is not null it is used to obtain 8-bit ASCII characters.
        /// </summary>
        public byte* AsciiStringPtr;
        /// <summary>
        /// If this is not null it represents a Unicode string encoded as UTF16.
        /// </summary>
        public string Source;
        /// <summary>
        /// If this is not null the object implement ISourceText provides some way to get at individual characters and substrings.
        /// </summary>
        public ISourceText TextProvider;
        /// <summary>
        /// The number of characters in the source document. 
        /// A "character" corresponds to a System.Char which is actually a Unicode UTF16 code point to be precise.
        /// </summary>
        public int Length;
        public DocumentText(string source)
        {
            if(source == null) { Debug.Assert(false); return; }
            this.Source = source;
            this.Length = source.Length;
        }
        public DocumentText(ISourceText textProvider)
        {
            if(textProvider == null) { Debug.Assert(false); return; }
            this.TextProvider = textProvider;
            this.Length = textProvider.Length;
        }
        public unsafe DocumentText(ISourceTextBuffer textProvider)
        {
            if(textProvider == null) { Debug.Assert(false); return; }
            this.TextProvider = textProvider;
            this.AsciiStringPtr = textProvider.Buffer;
            this.Length = textProvider.Length;
        }
        /// <summary>
        /// Compare this.Substring(offset, length) for equality with str.
        /// Call this only if str.Length is known to be equal to length.
        /// </summary>
        public bool Equals(string str, int position, int length)
        { //TODO: (int position, int length, string str)
            if(str == null) { Debug.Assert(false); return false; }
            if(str.Length != length) { Debug.Assert(false); return false; }
            if(position < 0 || position + length > this.Length) { Debug.Assert(false); return false; }
            unsafe
            {
                byte* p = this.AsciiStringPtr;
                if(p != null)
                {
                    for(int i = position, j = 0; j < length; i++, j++)
                        if(((char)*(p + i)) != str[j])
                            return false;
                    return true;
                }
            }
            string source = this.Source;
            if(source != null)
            {
                for(int i = position, j = 0; j < length; i++, j++)
                    if(source[i] != str[j])
                        return false;
                return true;
            }
            ISourceText myProvider = this.TextProvider;
            if(myProvider == null) { Debug.Assert(false); return false; }
            for(int i = position, j = 0; j < length; i++, j++)
                if(myProvider[i] != str[j])
                    return false;
            return true;
        }
        /// <summary>
        /// Compares the substring of the specified length starting at offset, with the substring in DocumentText starting at textOffset.
        /// </summary>
        /// <param name="offset">The index of the first character of the substring of this DocumentText.</param>
        /// <param name="text">The Document text with the substring being compared to.</param>
        /// <param name="textOffset">The index of the first character of the substring of the DocumentText being compared to.</param>
        /// <param name="length">The number of characters in the substring being compared.</param>
        /// <returns></returns>
        public bool Equals(int offset, DocumentText text, int textOffset, int length)
        { //TODO: (int position, int length, DocumentText text, int textPosition)
            if(offset < 0 || length < 0 || offset + length > this.Length) { Debug.Assert(false); return false; }
            if(textOffset < 0 || text == null || textOffset + length > text.Length) { Debug.Assert(false); return false; }
            unsafe
            {
                byte* p = this.AsciiStringPtr;
                if(p != null)
                {
                    unsafe
                    {
                        byte* q = text.AsciiStringPtr;
                        if(q != null)
                        {
                            for(int i = offset, j = textOffset, n = offset + length; i < n; i++, j++)
                                if(*(p + i) != *(q + j))
                                    return false;
                            return true;
                        }
                    }
                    string textSource = text.Source;
                    if(textSource != null)
                    {
                        for(int i = offset, j = textOffset, n = offset + length; i < n; i++, j++)
                            if(((char)*(p + i)) != textSource[j])
                                return false;
                        return true;
                    }
                    ISourceText textProvider = text.TextProvider;
                    if(textProvider == null) { Debug.Assert(false); return false; }
                    for(int i = offset, j = textOffset, n = offset + length; i < n; i++, j++)
                        if(((char)*(p + i)) != textProvider[j])
                            return false;
                    return true;
                }
            }
            string source = this.Source;
            if(source != null)
            {
                unsafe
                {
                    byte* q = text.AsciiStringPtr;
                    if(q != null)
                    {
                        for(int i = offset, j = textOffset, n = offset + length; i < n; i++, j++)
                            if(source[i] != (char)*(q + j))
                                return false;
                        return true;
                    }
                }
                string textSource = text.Source;
                if(textSource != null)
                {
                    for(int i = offset, j = textOffset, n = offset + length; i < n; i++, j++)
                        if(source[i] != textSource[j])
                            return false;
                    return true;
                }
                ISourceText textProvider = text.TextProvider;
                if(textProvider == null) { Debug.Assert(false); return false; }
                for(int i = offset, j = textOffset, n = offset + length; i < n; i++, j++)
                    if(source[i] != textProvider[j])
                        return false;
                return true;
            }
            {
                ISourceText myProvider = this.TextProvider;
                if(myProvider == null) { Debug.Assert(false); return false; }
                unsafe
                {
                    byte* q = text.AsciiStringPtr;
                    if(q != null)
                    {
                        for(int i = offset, j = textOffset, n = offset + length; i < n; i++, j++)
                            if(myProvider[i] != (char)*(q + j))
                                return false;
                        return true;
                    }
                }
                string textSource = text.Source;
                if(textSource != null)
                {
                    for(int i = offset, j = textOffset, n = offset + length; i < n; i++, j++)
                        if(myProvider[i] != textSource[j])
                            return false;
                    return true;
                }
                ISourceText textProvider = text.TextProvider;
                if(textProvider == null) { Debug.Assert(false); return false; }
                for(int i = offset, j = textOffset, n = offset + length; i < n; i++, j++)
                    if(myProvider[i] != textProvider[j])
                        return false;
                return true;
            }
        }
        /// <summary>
        /// Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.
        /// </summary>
        public string/*!*/ Substring(int position, int length)
        {
            if(position < 0 || length < 0 || position + length > this.Length + 1) { Debug.Assert(false); return ""; }
            if(position + length > this.Length)
                length = this.Length - position; //Allow virtual EOF character to be included in length
            if(this.AsciiStringPtr != null)
            {
                unsafe
                {
                    return new String((sbyte*)this.AsciiStringPtr, position, length, System.Text.Encoding.ASCII);
                }
            }
            else if(this.Source != null)
                return this.Source.Substring(position, length);
            else if(this.TextProvider != null)
                return this.TextProvider.Substring(position, length);
            else
            {
                Debug.Assert(false);
                return "";
            }
        }
        /// <summary>
        /// Retrieves the character at the given position. The first character is at position zero.
        /// </summary>
        public char this[int position]
        {
            get
            {
                if(position < 0 || position >= this.Length) { Debug.Assert(false); return (char)0; }
                if(this.AsciiStringPtr != null)
                {
                    unsafe
                    {
                        unchecked
                        {
                            return (char)*(this.AsciiStringPtr + position);
                        }
                    }
                }
                else if(this.Source != null)
                    return this.Source[position];
                else if(this.TextProvider != null)
                    return this.TextProvider[position];
                else
                {
                    Debug.Assert(false);
                    return (char)0;
                }
            }
        }
    }

    /// <summary>
    /// A source document from which an Abstract Syntax Tree has been derived.
    /// </summary>
    public class Document
    {
        /// <summary>
        /// A Guid that identifies the kind of document to applications such as a debugger. Typically System.Diagnostics.SymbolStore.SymDocumentType.Text.
        /// </summary>
        public System.Guid DocumentType;
        /// <summary>
        /// A Guid that identifies the programming language used in the source document. Typically used by a debugger to locate language specific logic.
        /// </summary>
        public System.Guid Language;
        /// <summary>
        /// A Guid that identifies the compiler vendor programming language used in the source document. Typically used by a debugger to locate vendor specific logic.
        /// </summary>
        public System.Guid LanguageVendor;
        /// <summary>
        /// The line number corresponding to the first character in Text. Typically 1 but can be changed by C# preprocessor directives. 
        /// </summary>
        public int LineNumber;
        /// <summary>
        /// Indicates that the document contains machine generated source code that should not show up in tools such as debuggers.
        /// Can be set by C# preprocessor directives.
        /// </summary>
        public bool Hidden;
        /// <summary>
        /// The name of the document. Typically a file name. Can be a full or relative file path, or a URI or some other kind of identifier.
        /// </summary>
        public string/*!*/ Name;
        /// <summary>
        /// Contains the source text.
        /// </summary>
        public DocumentText Text;
        public Document()
        {
            this.Name = "";
            //^ base();
        }
        public Document(string/*!*/ name, int lineNumber, string text, System.Guid documentType, System.Guid language, System.Guid languageVendor)
            : this(name, lineNumber, new DocumentText(text), documentType, language, languageVendor)
        {
        }
        public Document(string/*!*/ name, int lineNumber, DocumentText text, System.Guid documentType, System.Guid language, System.Guid languageVendor)
        {
            this.DocumentType = documentType;
            this.Language = language;
            this.LanguageVendor = languageVendor;
            this.LineNumber = lineNumber;
            this.Name = name;
            this.Text = text;
            //^ base();
        }

        /// <summary>
        /// Maps the given zero based character position to the number of the source line containing the same character. 
        /// Line number counting starts from the value of LineNumber.
        /// </summary>
        public virtual int GetLine(int position)
        {
            int line = 0;
            int column = 0;
            this.GetPosition(position, out line, out column);
            return line + this.LineNumber;
        }
        /// <summary>
        /// Maps the given zero based character position in the entire text to the position of the same character in a source line.
        /// Counting within the source line starts at 1.
        /// </summary>
        public virtual int GetColumn(int position)
        {
            int line = 0;
            int column = 0;
            this.GetPosition(position, out line, out column);
            return column + 1;
        }

        /// <summary>
        /// Given a startLine, startColum, endLine and endColumn, this returns the corresponding startPos and endPos. In other words it
        /// converts a range expression in line and columns to a range expressed as a start and end character position.
        /// </summary>
        /// <param name="startLine">The number of the line containing the first character. The number of the first line equals this.LineNumber.</param>
        /// <param name="startColumn">The position of the first character relative to the start of the line. Counting from 1.</param>
        /// <param name="endLine">The number of the line contain the character that immediate follows the last character of the range.</param>
        /// <param name="endColumn">The position, in the last line, of the character that immediately follows the last character of the range.</param>
        /// <param name="startPos">The position in the entire text of the first character of the range, counting from 0.</param>
        /// <param name="endPos">The position in the entire text of the character following the last character of the range.</param>
        public virtual void GetOffsets(int startLine, int startColumn, int endLine, int endColumn, out int startPos, out int endPos)
        {
            lock(this)
            {
                if(this.lineOffsets == null)
                    this.ComputeLineOffsets();
                //^ assert this.lineOffsets != null;
                startPos = this.lineOffsets[startLine - this.LineNumber] + startColumn - 1;
                endPos = this.lineOffsets[endLine - this.LineNumber] + endColumn - 1;
            }
        }

        /// <summary>
        /// Retrieves a substring from the text of this Document. The substring starts at a specified character position and has a specified length.
        /// </summary>
        public virtual string Substring(int position, int length)
        {
            if(this.Text == null)
                return null;
            return this.Text.Substring(position, length);
        }

        /// <summary>
        /// Counts the number of end of line marker sequences in the given text.
        /// </summary>
        protected int GetLineCount(string/*!*/ text)
        {
            int n = text == null ? 0 : text.Length;
            int count = 0;
            for(int i = 0; i < n; i++)
            {
                switch(text[i])
                {
                    case '\r':
                        if(i + 1 < n && text[i + 1] == '\n')
                            i++;
                        count++;
                        break;
                    case '\n':
                    case (char)0x2028:
                    case (char)0x2029:
                        count++;
                        break;
                }
            }
            return count;
        }
        /// <summary>An array of offsets, with offset at index i corresponding to the position of the first character of line i, (counting lines from 0).</summary>
        private int[] lineOffsets;
        /// <summary>The number of lines in Text.</summary>
        private int lines;

        /// <summary>
        ///  Returns the index in this.lineOffsets array such that this.lineOffsets[index] is less than or equal to offset
        ///  and offset is less than lineOffsets[index+1]
        /// </summary>
        private int Search(int offset)
        {
tryAgain:
            int[] lineOffsets = this.lineOffsets;
            int lines = this.lines;
            if(lineOffsets == null) { Debug.Assert(false); return -1; }
            if(offset < 0) { Debug.Assert(false); return -1; }
            int mid = 0;
            int low = 0;
            int high = lines - 1;
            while(low < high)
            {
                mid = (low + high) / 2;
                if(lineOffsets[mid] <= offset)
                {
                    if(offset < lineOffsets[mid + 1])
                        return mid;
                    else
                        low = mid + 1;
                }
                else
                    high = mid;
            }
            Debug.Assert(lines == this.lines);
            Debug.Assert(lineOffsets[low] <= offset);
            Debug.Assert(offset < lineOffsets[low + 1]);
            if(lineOffsets != this.lineOffsets)
                goto tryAgain;
            return low;
        }

        /// <summary>
        /// Maps the given zero based character position in the entire text to a (line, column) pair corresponding to the same position.
        /// Counting within the source line starts at 0. Counting source lines start at 0.
        /// </summary>
        private void GetPosition(int offset, out int line, out int column)
        {
            line = 0;
            column = 0;
            if(offset < 0 || this.Text == null || offset > this.Text.Length) { Debug.Assert(false); return; }
            lock(this)
            {
                if(this.lineOffsets == null)
                    this.ComputeLineOffsets();
                if(this.lineOffsets == null) { Debug.Assert(false); return; }
                int[] lineOffsets = this.lineOffsets;
                int index = this.Search(offset);
                Debug.Assert(lineOffsets == this.lineOffsets);
                if(index < 0 || index >= this.lineOffsets.Length) { Debug.Assert(false); return; }
                Debug.Assert(this.lineOffsets[index] <= offset && offset < this.lineOffsets[index + 1]);
                line = index;
                column = offset - this.lineOffsets[index];
            }
        }
        /// <summary>
        /// Adds the given offset to the this.lineOffsets table as the offset corresponding to the start of line this.lines+1.
        /// </summary>
        private void AddOffset(int offset)
        {
            if(this.lineOffsets == null || this.lines < 0) { Debug.Assert(false); return; }
            if(this.lines >= this.lineOffsets.Length)
            {
                int n = this.lineOffsets.Length;
                if(n <= 0)
                    n = 16;
                int[] newLineOffsets = new int[n * 2];
                Array.Copy(this.lineOffsets, newLineOffsets, this.lineOffsets.Length);
                this.lineOffsets = newLineOffsets;
            }
            this.lineOffsets[this.lines++] = offset;
        }
        public virtual void InsertOrDeleteLines(int offset, int lineCount)
        {
            if(lineCount == 0)
                return;
            if(offset < 0 || this.Text == null || offset > this.Text.Length) { Debug.Assert(false); return; }
            lock(this)
            {
                if(this.lineOffsets == null)
                    if(this.lineOffsets == null)
                        this.ComputeLineOffsets();
                if(lineCount < 0)
                    this.DeleteLines(offset, -lineCount);
                else
                    this.InsertLines(offset, lineCount);
            }
        }
        private void DeleteLines(int offset, int lineCount)
        //^ requires offset >= 0 && this.Text != null && offset < this.Text.Length && lineCount > 0 && this.lineOffsets != null;
        {
            Debug.Assert(offset >= 0 && this.Text != null && offset < this.Text.Length && lineCount > 0 && this.lineOffsets != null);
            int index = this.Search(offset);
            if(index < 0 || index >= this.lines) { Debug.Assert(false); return; }
            for(int i = index + 1; i + lineCount < this.lines; i++)
            {
                this.lineOffsets[i] = this.lineOffsets[i + lineCount];
            }
            this.lines -= lineCount;
            if(this.lines <= index) { Debug.Assert(false); this.lines = index + 1; }
        }
        private void InsertLines(int offset, int lineCount)
        //^ requires offset >= 0 && this.Text != null && offset < this.Text.Length && lineCount > 0 && this.lineOffsets != null;
        {
            Debug.Assert(offset >= 0 && this.Text != null && offset < this.Text.Length && lineCount > 0 && this.lineOffsets != null);
            int index = this.Search(offset);
            if(index < 0 || index >= this.lines) { Debug.Assert(false); return; }
            int n = this.lineOffsets[this.lines - 1];
            for(int i = 0; i < lineCount; i++)
                this.AddOffset(++n);
            for(int i = lineCount; i > 0; i--)
            {
                this.lineOffsets[index + i + 1] = this.lineOffsets[index + 1];
            }
        }
        /// <summary>
        /// Populates this.lineOffsets with an array of offsets, with offset at index i corresponding to the position of the first
        /// character of line i, (counting lines from 0).
        /// </summary>
        private void ComputeLineOffsets()
        //ensures this.lineOffsets != null;
        {
            if(this.Text == null) { Debug.Assert(false); return; }
            int n = this.Text.Length;
            this.lineOffsets = new int[n / 10 + 1];
            this.lines = 0;
            this.AddOffset(0);
            for(int i = 0; i < n; i++)
            {
                switch(this.Text[i])
                {
                    case '\r':
                        if(i + 1 < n && this.Text[i + 1] == '\n')
                            i++;
                        this.AddOffset(i + 1);
                        break;
                    case '\n':
                    case (char)0x2028:
                    case (char)0x2029:
                        this.AddOffset(i + 1);
                        break;
                }
            }
            this.AddOffset(n + 1);
            this.AddOffset(n + 2);
        }

        /// <summary> Add one to this every time a Document instance gets a unique key.</summary>
        private static int uniqueKeyCounter;
        private int uniqueKey;
        /// <summary>
        /// An integer that uniquely distinguishes this document instance from every other document instance. 
        /// This provides an efficient equality test to facilitate hashing.
        /// </summary>
        public int UniqueKey
        {
            get
            {
                if(this.uniqueKey == 0)
                {
TryAgain:
                    int c = Document.uniqueKeyCounter;
                    int cp1 = c == int.MaxValue ? 1 : c + 1;
                    if(System.Threading.Interlocked.CompareExchange(ref Document.uniqueKeyCounter, cp1, c) != c)
                        goto TryAgain;
                    this.uniqueKey = cp1;
                }
                return this.uniqueKey;
            }
        }
    }

    internal class UnmanagedDocument : Document
    {
        internal UnmanagedDocument(IntPtr ptrToISymUnmanagedDocument)
        {
            //^ base();
            ISymUnmanagedDocument idoc =
              (ISymUnmanagedDocument)System.Runtime.InteropServices.Marshal.GetTypedObjectForIUnknown(ptrToISymUnmanagedDocument, typeof(ISymUnmanagedDocument));
            if(idoc != null)
            {
                try
                {
                    idoc.GetDocumentType(out this.DocumentType);
                    idoc.GetLanguage(out this.Language);
                    idoc.GetLanguageVendor(out this.LanguageVendor);

                    uint capacity = 1024;
                    uint len = 0;
                    char[] buffer = new char[capacity];
                    while(capacity >= 1024)
                    {
                        idoc.GetURL(capacity, out len, buffer);
                        if(len < capacity)
                            break;
                        capacity += 1024;
                        buffer = new char[capacity];
                    }
                    if(len > 0)
                        this.Name = new String(buffer, 0, (int)len - 1);
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(idoc);
                }
            }

            this.LineNumber = -1;
            this.Text = null;
        }

        private List<int> lineList = new List<int>();
        private List<int> columnList = new List<int>();

        public override int GetLine(int offset)
        {
            return this.lineList[offset];
        }
        public override int GetColumn(int offset)
        {
            return this.columnList[offset];
        }
        public override void GetOffsets(int startLine, int startColumn, int endLine, int endColumn, out int startCol, out int endCol)
        {
            int i = UnmanagedDocument.BinarySearch(this.lineList, startLine);
            List<int> columnList = this.columnList;
            startCol = 0;
            for(int j = i, n = columnList.Count; j < n; j++)
            {
                if(columnList[j] >= startColumn) { startCol = j; break; }
            }
            endCol = 0;
            i = UnmanagedDocument.BinarySearch(this.lineList, endLine);
            for(int j = i, n = columnList.Count; j < n; j++)
            {
                if(columnList[j] >= endColumn) { endCol = j; break; }
            }
        }

        private static int BinarySearch(List<int> list, int value)
        {
            int mid = 0;
            int low = 0;
            int high = list.Count - 1;
            while(low < high)
            {
                mid = low + (high - low) / 2;
                if(list[mid] <= value)
                {
                    if(list[mid + 1] > value)
                        return mid;
                    else
                        low = mid + 1;
                }
                else
                    high = mid;
            }
            return low;
        }
        public override void InsertOrDeleteLines(int offset, int lineCount)
        {
            Debug.Assert(false); //Caller should not be modifying an unmanaged document
        }

        internal int GetOffset(uint line, uint column)
        {
            this.lineList.Add((int)line);
            this.columnList.Add((int)column);
            return this.lineList.Count - 1;
        }
    }

    /// <summary>
    /// Records a location within a source document that corresponds to an Abstract Syntax Tree node.
    /// </summary>
    public struct SourceContext
    {
        /// <summary>The source document within which the AST node is located. Null if the node is not derived from a source document.</summary>
        public Document Document;
        /// <summary>
        /// The zero based index of the first character beyond  the last character in the source document that corresponds to the AST node.
        /// </summary>
        public int EndPos;
        /// <summary>
        /// The zero based index of the first character in the source document that corresponds to the AST node.
        /// </summary>
        public int StartPos;
        public SourceContext(Document document)
            : this(document, 0, document == null ? 0 : (document.Text == null ? 0 : document.Text.Length))
        {
        }
        public SourceContext(Document document, int startPos, int endPos)
        {
            this.Document = document;
            this.StartPos = startPos;
            this.EndPos = endPos;
        }
        public SourceContext(Document/*!*/ document,
          int startLine, int startColumn, int endLine, int endColumn)
        {
            this.Document = document;
            this.Document.GetOffsets(startLine, startColumn, endLine, endColumn, out this.StartPos, out this.EndPos);
        }
        /// <summary>
        /// The number (counting from Document.LineNumber) of the line containing the first character in the source document that corresponds to the AST node.
        /// </summary>
        public int StartLine
        {
            get
            {
                if(this.Document == null)
                    return 0;
                return this.Document.GetLine(this.StartPos);
            }
        }
        /// <summary>
        /// The number (counting from one) of the line column containing the first character in the source document that corresponds to the AST node.
        /// </summary>
        public int StartColumn
        {
            get
            {
                if(this.Document == null)
                    return 0;
                return this.Document.GetColumn(this.StartPos);
            }
        }
        /// <summary>
        /// The number (counting from Document.LineNumber) of the line containing the first character beyond the last character in the source document that corresponds to the AST node.
        /// </summary>
        public int EndLine
        {
            get
            {
                if(this.Document == null || (this.Document.Text == null && !(this.Document is UnmanagedDocument)))
                    return 0;

                if(this.Document.Text != null && this.EndPos >= this.Document.Text.Length)
                    this.EndPos = this.Document.Text.Length;

                return this.Document.GetLine(this.EndPos);
            }
        }
        /// <summary>
        /// The number (counting from one) of the line column containing first character beyond the last character in the source document that corresponds to the AST node.
        /// </summary>
        public int EndColumn
        {
            get
            {
                if(this.Document == null || (this.Document.Text == null && !(this.Document is UnmanagedDocument)))
                    return 0;

                if(this.Document.Text != null && this.EndPos >= this.Document.Text.Length)
                    this.EndPos = this.Document.Text.Length;

                return this.Document.GetColumn(this.EndPos);
            }
        }
        /// <summary>
        /// Returns true if the line and column is greater than or equal the position of the first character 
        /// and less than or equal to the position of the last character
        /// of the source document that corresponds to the AST node.
        /// </summary>
        /// <param name="line">A line number(counting from Document.LineNumber)</param>
        /// <param name="column">A column number (counting from one)</param>
        /// <returns></returns>
        public bool Encloses(int line, int column)
        {
            if(line < this.StartLine || line > this.EndLine)
                return false;
            if(line == this.StartLine)
                return column >= this.StartColumn && (column <= this.EndColumn || line < this.EndLine);
            if(line == this.EndLine)
                return column <= this.EndColumn;
            return true;
        }
        public bool Encloses(SourceContext sourceContext)
        {
            return this.StartPos <= sourceContext.StartPos && this.EndPos >= sourceContext.EndPos && this.EndPos > sourceContext.StartPos;
        }
        /// <summary>
        /// The substring of the source document that corresponds to the AST node.
        /// </summary>
        public string SourceText
        {
            get
            {
                if(this.Document == null)
                    return null;
                return this.Document.Substring(this.StartPos, this.EndPos - this.StartPos);
            }
        }
    }

    public sealed class MarshallingInformation
    {
        private string @class;
        private string cookie;
        private int elementSize;
        private NativeType elementType;
        private NativeType nativeType;
        private int numberOfElements;
        private int paramIndex;
        private int size;
        public MarshallingInformation Clone()
        {
            return (MarshallingInformation)base.MemberwiseClone();
        }
        public string Class
        {
            get { return this.@class; }
            set { this.@class = value; }
        }
        public string Cookie
        {
            get { return this.cookie; }
            set { this.cookie = value; }
        }
        public int ElementSize
        {
            get { return this.elementSize; }
            set { this.elementSize = value; }
        }
        public NativeType ElementType
        {
            get { return this.elementType; }
            set { this.elementType = value; }
        }
        public NativeType NativeType
        {
            get { return this.nativeType; }
            set { this.nativeType = value; }
        }
        public int NumberOfElements
        {
            get { return this.numberOfElements; }
            set { this.numberOfElements = value; }
        }
        public int ParamIndex
        {
            get { return this.paramIndex; }
            set { this.paramIndex = value; }
        }
        public int Size
        {
            get { return this.size; }
            set { this.size = value; }
        }
    }

    public enum NativeType
    {
        Bool = 0x2,      // 4 byte boolean value (true != 0, false == 0)
        I1 = 0x3,      // 1 byte signed value
        U1 = 0x4,      // 1 byte unsigned value
        I2 = 0x5,      // 2 byte signed value
        U2 = 0x6,      // 2 byte unsigned value
        I4 = 0x7,      // 4 byte signed value
        U4 = 0x8,      // 4 byte unsigned value
        I8 = 0x9,      // 8 byte signed value
        U8 = 0xa,      // 8 byte unsigned value
        R4 = 0xb,      // 4 byte floating point
        R8 = 0xc,      // 8 byte floating point
        Currency = 0xf,      // A currency
        BStr = 0x13,    // OLE Unicode BSTR
        LPStr = 0x14,    // Ptr to SBCS string
        LPWStr = 0x15,    // Ptr to Unicode string
        LPTStr = 0x16,    // Ptr to OS preferred (SBCS/Unicode) string
        ByValTStr = 0x17,    // OS preferred (SBCS/Unicode) inline string (only valid in structs)
        IUnknown = 0x19,    // COM IUnknown pointer. 
        IDispatch = 0x1a,    // COM IDispatch pointer
        Struct = 0x1b,    // Structure
        Interface = 0x1c,    // COM interface
        SafeArray = 0x1d,    // OLE SafeArray
        ByValArray = 0x1e,    // Array of fixed size (only valid in structs)
        SysInt = 0x1f,    // Hardware natural sized signed integer
        SysUInt = 0x20,
        VBByRefStr = 0x22,
        AnsiBStr = 0x23,    // OLE BSTR containing SBCS characters
        TBStr = 0x24,    // Ptr to OS preferred (SBCS/Unicode) BSTR
        VariantBool = 0x25,    // OLE defined BOOLEAN (2 bytes, true == -1, false == 0)
        FunctionPtr = 0x26,    // Function pointer
        AsAny = 0x28,    // Paired with Object type and does runtime marshalling determination
        LPArray = 0x2a,    // C style array
        LPStruct = 0x2b,    // Pointer to a structure
        CustomMarshaler = 0x2c, // Native type supplied by custom code   
        Error = 0x2d,
        NotSpecified = 0x50,
    }

    ///0-: Common
    ///1000-: HScript
    ///2000-: EcmaScript
    ///3000-: Zonnon
    ///4000-: Comega
    ///5000-: X++
    ///6000-: Spec#
    ///7000-: Sing#
    ///8000-: Xaml
    ///9000-: C/AL
    ///For your range contact hermanv@microsoft.com
    public enum NodeType
    {
        //Dummy
        Undefined = 0,

        //IL instruction node tags
        Add,
        Add_Ovf,
        Add_Ovf_Un,
        And,
        Arglist,
        Box,
        Branch,
        Call,
        Calli,
        Callvirt,
        Castclass,
        Ceq,
        Cgt,
        Cgt_Un,
        Ckfinite,
        Clt,
        Clt_Un,
        Conv_I,
        Conv_I1,
        Conv_I2,
        Conv_I4,
        Conv_I8,
        Conv_Ovf_I,
        Conv_Ovf_I_Un,
        Conv_Ovf_I1,
        Conv_Ovf_I1_Un,
        Conv_Ovf_I2,
        Conv_Ovf_I2_Un,
        Conv_Ovf_I4,
        Conv_Ovf_I4_Un,
        Conv_Ovf_I8,
        Conv_Ovf_I8_Un,
        Conv_Ovf_U,
        Conv_Ovf_U_Un,
        Conv_Ovf_U1,
        Conv_Ovf_U1_Un,
        Conv_Ovf_U2,
        Conv_Ovf_U2_Un,
        Conv_Ovf_U4,
        Conv_Ovf_U4_Un,
        Conv_Ovf_U8,
        Conv_Ovf_U8_Un,
        Conv_R_Un,
        Conv_R4,
        Conv_R8,
        Conv_U,
        Conv_U1,
        Conv_U2,
        Conv_U4,
        Conv_U8,
        Cpblk,
        DebugBreak,
        Div,
        Div_Un,
        Dup,
        EndFilter,
        EndFinally,
        ExceptionHandler,
        Initblk,
        Isinst,
        Jmp,
        Ldftn,
        Ldlen,
        Ldtoken,
        Ldvirtftn,
        Localloc,
        Mkrefany,
        Mul,
        Mul_Ovf,
        Mul_Ovf_Un,
        Neg,
        Nop,
        Not,
        Or,
        Pop,
        ReadOnlyAddressOf,
        Refanytype,
        Refanyval,
        Rem,
        Rem_Un,
        Rethrow,
        Shl,
        Shr,
        Shr_Un,
        Sizeof,
        SkipCheck,
        Sub,
        Sub_Ovf,
        Sub_Ovf_Un,
        SwitchInstruction,
        Throw,
        Unbox,
        UnboxAny,
        Xor,

        //AST tags that are relevant to the binary reader
        AddressDereference,
        AddressOf,
        AssignmentStatement,
        Block,
        Catch,
        Construct,
        ConstructArray,
        Eq,
        ExpressionStatement,
        FaultHandler,
        Filter,
        Finally,
        Ge,
        Gt,
        Identifier,
        Indexer,
        Instruction,
        InterfaceExpression,
        Le,
        Literal,
        LogicalNot,
        Lt,
        MemberBinding,
        NamedArgument,
        Namespace,
        Ne,
        Return,
        This,
        Try,

        //Metadata node tags
        ArrayType,
        @Assembly,
        AssemblyReference,
        Attribute,
        Class,
        ClassParameter,
        DelegateNode,
        EnumNode,
        Event,
        Field,
        FunctionPointer,
        InstanceInitializer,
        Interface,
        Local,
        Method,
        Module,
        ModuleReference,
        OptionalModifier,
        Parameter,
        Pointer,
        Property,
        Reference,
        RequiredModifier,
        SecurityAttribute,
        StaticInitializer,
        Struct,
        TypeParameter,

        // The following NodeType definitions are not required
        // for examining assembly metadata directly from binaries

        //Serialization tags used for values that are not leaf nodes.
        Array,
        BlockReference,
        CompilationParameters,
        Document,
        EndOfRecord,
        Expression,
        Guid,
        List,
        MarshallingInformation,
        Member,
        MemberReference,
        MissingBlockReference,
        MissingExpression,
        MissingMemberReference,
        String,
        StringDictionary,
        TypeNode,
        Uri,
        XmlNode,

        //Source-based AST node tags
        AddEventHandler,
        AliasDefinition,
        AnonymousNestedFunction,
        ApplyToAll,
        ArglistArgumentExpression,
        ArglistExpression,
        ArrayTypeExpression,
        As,
        Assertion,
        AssignmentExpression,
        Assumption,
        Base,

        BlockExpression,
        BoxedTypeExpression,
        ClassExpression,
        CoerceTuple,
        CollectionEnumerator,
        Comma,
        Compilation,
        CompilationUnit,
        CompilationUnitSnippet,
        Conditional,
        ConstructDelegate,
        ConstructFlexArray,
        ConstructIterator,
        ConstructTuple,
        Continue,
        CopyReference,
        CurrentClosure,
        Decrement,
        DefaultValue,
        DoWhile,
        Exit,
        ExplicitCoercion,
        ExpressionSnippet,
        FieldInitializerBlock,
        Fixed,
        FlexArrayTypeExpression,
        For,
        ForEach,
        FunctionDeclaration,
        FunctionTypeExpression,
        Goto,
        GotoCase,
        If,
        ImplicitThis,
        Increment,
        InvariantTypeExpression,
        Is,
        LabeledStatement,
        LocalDeclaration,
        LocalDeclarationsStatement,
        Lock,
        LogicalAnd,
        LogicalOr,
        LRExpression,
        MethodCall,
        NameBinding,
        NonEmptyStreamTypeExpression,
        NonNullableTypeExpression,
        NonNullTypeExpression,
        NullableTypeExpression,
        NullCoalesingExpression,
        OutAddress,
        Parentheses,
        PointerTypeExpression,
        PostfixExpression,
        PrefixExpression,
        QualifiedIdentifer,
        RefAddress,
        ReferenceTypeExpression,
        RefTypeExpression,
        RefValueExpression,
        RemoveEventHandler,
        Repeat,
        ResourceUse,
        SetterValue,
        StackAlloc,
        StatementSnippet,
        StreamTypeExpression,
        Switch,
        SwitchCase,
        SwitchCaseBottom,
        TemplateInstance,
        TupleTypeExpression,
        TypeExpression,
        TypeIntersectionExpression,
        TypeMemberSnippet,
        Typeof,
        TypeReference,
        Typeswitch,
        TypeswitchCase,
        TypeUnionExpression,
        UnaryPlus,
        UsedNamespace,
        VariableDeclaration,
        While,
        Yield,

        //Extended metadata node tags
        ConstrainedType,
        TupleType,
        TypeAlias,
        TypeIntersection,
        TypeUnion,

        //Query node tags
        Composition,
        QueryAggregate,
        QueryAlias,
        QueryAll,
        QueryAny,
        QueryAxis,
        QueryCommit,
        QueryContext,
        QueryDelete,
        QueryDifference,
        QueryDistinct,
        QueryExists,
        QueryFilter,
        QueryGeneratedType,
        QueryGroupBy,
        QueryInsert,
        QueryIntersection,
        QueryIterator,
        QueryJoin,
        QueryLimit,
        QueryOrderBy,
        QueryOrderItem,
        QueryPosition,
        QueryProject,
        QueryQuantifiedExpression,
        QueryRollback,
        QuerySelect,
        QuerySingleton,
        QueryTransact,
        QueryTypeFilter,
        QueryUnion,
        QueryUpdate,
        QueryYielder,

        //Contract node tags
        Acquire,
        Comprehension,
        ComprehensionBinding,
        Ensures,
        EnsuresExceptional,
        EnsuresNormal,
        Iff,
        Implies,
        Invariant,
        LogicalEqual,
        LogicalImply,
        Maplet,
        MethodContract,
        Modelfield,
        ModelfieldContract,
        OldExpression,
        Range,
        Read,
        Requires,
        RequiresOtherwise,
        RequiresPlain,
        TypeContract,
        Write,

        //Node tags for explicit modifiers in front-end
        OptionalModifierTypeExpression,
        RequiredModifierTypeExpression,

        //Temporary node tags
        Count,
        Exists,
        ExistsUnique,
        Forall,
        Max,
        Min,
        Product,
        Sum,
        Quantifier,
    }

    [Flags]
    public enum AssemblyFlags
    {
        None = 0x0000,
        PublicKey = 0x0001,
        Library = 0x0002,
        Platform = 0x0004,
        NowPlatform = 0x0006,
        SideBySideCompatible = 0x0000,
        NonSideBySideCompatible = 0x0010,
        NonSideBySideProcess = 0x0020,
        NonSideBySideMachine = 0x0030,
        CompatibilityMask = 0x00F0,
        Retargetable = 0x0100,
        DisableJITcompileOptimizer = 0x4000,
        EnableJITcompileTracking = 0x8000
    }

    public enum AssemblyHashAlgorithm
    {
        None = 0x0000,
        MD5 = 0x8003,
        SHA1 = 0x8004
    }

    [Flags]
    public enum CallingConventionFlags
    {
        Default = 0x0,
        C = 0x1,
        StandardCall = 0x2,
        ThisCall = 0x3,
        FastCall = 0x4,
        VarArg = 0x5,
        ArgumentConvention = 0x7,
        Generic = 0x10,
        HasThis = 0x20,
        ExplicitThis = 0x40
    }

    [Flags]
    public enum EventFlags
    {
        None = 0x0000,
        SpecialName = 0x0200,
        ReservedMask = 0x0400,
        RTSpecialName = 0x0400,
        Extend = MethodFlags.Extend, // used for languages with type extensions, e.g. Sing#
    }

    [Flags]
    public enum FieldFlags
    {
        None = 0x0000,
        FieldAccessMask = 0x0007,
        CompilerControlled = 0x0000,
        Private = 0x0001,
        FamANDAssem = 0x0002,
        Assembly = 0x0003,
        Family = 0x0004,
        FamORAssem = 0x0005,
        Public = 0x0006,
        Static = 0x0010,
        InitOnly = 0x0020,
        Literal = 0x0040,
        NotSerialized = 0x0080,
        SpecialName = 0x0200,
        PinvokeImpl = 0x2000,
        ReservedMask = 0x9500,
        RTSpecialName = 0x0400,
        HasFieldMarshal = 0x1000,
        HasDefault = 0x8000,
        HasFieldRVA = 0x0100,
    }

    [Flags]
    public enum FileFlags
    {
        ContainsMetaData = 0x0000,
        ContainsNoMetaData = 0x0001
    }

    [Flags]
    public enum TypeParameterFlags
    {
        NonVariant = 0x0000,
        Covariant = 0x0001,
        Contravariant = 0x0002,
        VarianceMask = 0x0003,
        NoSpecialConstraint = 0x0000,
        ReferenceTypeConstraint = 0x0004,
        ValueTypeConstraint = 0x0008,
        DefaultConstructorConstraint = 0x0010,
        SpecialConstraintMask = 0x001C,
    }

    [Flags]
    public enum MethodImplFlags
    {
        CodeTypeMask = 0x0003,
        IL = 0x0000,
        Native = 0x0001,
        OPTIL = 0x0002,
        Runtime = 0x0003,
        ManagedMask = 0x0004,
        Unmanaged = 0x0004,
        Managed = 0x0000,
        ForwardRef = 0x0010,
        PreserveSig = 0x0080,
        InternalCall = 0x1000,
        Synchronized = 0x0020,
        NoInlining = 0x0008,
        MaxMethodImplVal = 0xffff
    }

    [Flags]
    public enum MethodFlags
    {
        MethodAccessMask = 0x0007,
        CompilerControlled = 0x0000,
        Private = 0x0001,
        FamANDAssem = 0x0002,
        Assembly = 0x0003,
        Family = 0x0004,
        FamORAssem = 0x0005,
        Public = 0x0006,
        Static = 0x0010,
        Final = 0x0020,
        Virtual = 0x0040,
        HideBySig = 0x0080,
        VtableLayoutMask = 0x0100,
        ReuseSlot = 0x0000,
        NewSlot = 0x0100,
        CheckAccessOnOverride = 0x0200,
        Abstract = 0x0400,
        SpecialName = 0x0800,
        PInvokeImpl = 0x2000,
        UnmanagedExport = 0xd000,
        ReservedMask = 0xd000,
        RTSpecialName = 0x1000,
        HasSecurity = 0x4000,
        RequireSecObject = 0x8000,
        Extend = 0x01000000, // used for languages with type extensions, e.g. Sing#
    }

    public enum ModuleKind
    {
        ConsoleApplication,
        WindowsApplication,
        DynamicallyLinkedLibrary,
        ManifestResourceFile,
        UnmanagedDynamicallyLinkedLibrary
    }

    [Flags]
    public enum ParameterFlags
    {
        None = 0x0000,
        In = 0x0001,
        Out = 0x0002,
        Optional = 0x0010,
        ReservedMask = 0xf000,
        HasDefault = 0x1000,
        HasFieldMarshal = 0x2000
    }

    [Flags]
    public enum PEKindFlags
    {
        ILonly = 0x0001,
        Requires32bits = 0x0002,
        Requires64bits = 0x0004,
        AMD = 0x0008
    }

    [Flags]
    public enum PInvokeFlags
    {
        None = 0x0000,
        NoMangle = 0x0001,
        BestFitDisabled = 0x0020,
        BestFitEnabled = 0x0010,
        BestFitUseAsm = 0x0000,
        BestFitMask = 0x0030,
        CharSetMask = 0x0006,
        CharSetNotSpec = 0x0000,
        CharSetAns = 0x0002,
        CharSetUnicode = 0x0004,
        CharSetAuto = 0x0006,
        SupportsLastError = 0x0040,
        CallingConvMask = 0x0700,
        CallConvWinapi = 0x0100,
        CallConvCdecl = 0x0200,
        CallConvStdcall = 0x0300,
        CallConvThiscall = 0x0400,
        CallConvFastcall = 0x0500,
        ThrowOnUnmappableCharMask = 0x3000,
        ThrowOnUnmappableCharEnabled = 0x1000,
        ThrowOnUnmappableCharDisabled = 0x2000,
        ThrowOnUnmappableCharUseAsm = 0x0000
    }

    [Flags]
    public enum PropertyFlags
    {
        None = 0x0000,
        SpecialName = 0x0200,
        ReservedMask = 0xf400,
        RTSpecialName = 0x0400,
        Extend = MethodFlags.Extend, // used for languages with type extensions, e.g. Sing#
    }

    public enum PESection
    {
        Text,
        SData,
        TLS
    };

    [Flags]
    public enum TypeFlags
    {
        None = 0x00000000,
        VisibilityMask = 0x00000007,
        NotPublic = 0x00000000,
        Public = 0x00000001,
        NestedPublic = 0x00000002,
        NestedPrivate = 0x00000003,
        NestedFamily = 0x00000004,
        NestedAssembly = 0x00000005,
        NestedFamANDAssem = 0x00000006,
        NestedFamORAssem = 0x00000007,
        LayoutMask = 0x00000018,
        AutoLayout = 0x00000000,
        SequentialLayout = 0x00000008,
        ExplicitLayout = 0x00000010,
        ClassSemanticsMask = 0x00000020,
        Class = 0x00000000,
        Interface = 0x00000020,
        LayoutOverridden = 0x00000040,   // even AutoLayout can be explicit or implicit
        Abstract = 0x00000080,
        Sealed = 0x00000100,
        SpecialName = 0x00000400,
        Import = 0x00001000,
        Serializable = 0x00002000,
        StringFormatMask = 0x00030000,
        AnsiClass = 0x00000000,
        UnicodeClass = 0x00010000,
        AutoClass = 0x00020000,
        BeforeFieldInit = 0x00100000,
        ReservedMask = 0x00040800,
        RTSpecialName = 0x00000800,
        HasSecurity = 0x00040000,
        Forwarder = 0x00200000, //The type is a stub left behind for backwards compatibility. References to this type are forwarded to another type by the CLR.
        Extend = 0x01000000,  // used for languages with type extensions, e.g. Sing#
    }

    public sealed class TrivialHashtable
    {
        struct HashEntry
        {
            public int Key;
            public object Value;
        }

        private HashEntry[]/*!*/ entries;
        private int count;

        public TrivialHashtable()
        {
            this.entries = new HashEntry[16];
            //this.count = 0;
        }
        private TrivialHashtable(HashEntry[]/*!*/ entries, int count)
        {
            this.entries = entries;
            this.count = count;
        }
        public TrivialHashtable(int expectedEntries)
        {
            int initialSize = 16;
            expectedEntries <<= 1;
            while(initialSize < expectedEntries && initialSize > 0)
                initialSize <<= 1;
            if(initialSize < 0)
                initialSize = 16;
            this.entries = new HashEntry[initialSize];
            //this.count = 0;
        }
        public int Count
        {
            get
            {
                return this.count;
            }
        }
        private void Expand()
        {
            HashEntry[] oldEntries = this.entries;
            int n = oldEntries.Length;
            int m = n * 2;
            if(m <= 0)
                return;
            HashEntry[] entries = new HashEntry[m];
            int count = 0;
            for(int i = 0; i < n; i++)
            {
                int key = oldEntries[i].Key;
                if(key <= 0)
                    continue; //No entry (0) or deleted entry (-1)
                object value = oldEntries[i].Value;
                Debug.Assert(value != null);
                int j = key & (m - 1);
                int k = entries[j].Key;
                while(true)
                {
                    if(k == 0)
                    {
                        entries[j].Value = value;
                        entries[j].Key = key;
                        count++;
                        break;
                    }
                    j++;
                    if(j >= m)
                        j = 0;
                    k = entries[j].Key;
                }
            }
            this.entries = entries;
            this.count = count;
        }
        public object this[int key]
        {
            get
            {
                if(key <= 0)
                    throw new ArgumentException(ExceptionStrings.KeyNeedsToBeGreaterThanZero, "key");
                HashEntry[] entries = this.entries;
                int n = entries.Length;
                int i = key & (n - 1);
                int k = entries[i].Key;
                object result = null;
                while(true)
                {
                    if(k == key) { result = entries[i].Value; break; }
                    if(k == 0)
                        break;
                    i++;
                    if(i >= n)
                        i = 0;
                    k = entries[i].Key;
                }
                return result;
            }
            set
            {
                if(key <= 0)
                    throw new ArgumentException(ExceptionStrings.KeyNeedsToBeGreaterThanZero, "key");
                HashEntry[] entries = this.entries;
                int n = entries.Length;
                int i = key & (n - 1);
                int k = entries[i].Key;
                while(true)
                {
                    if(k == key || k == 0)
                    {
                        entries[i].Value = value;
                        if(k == 0)
                        {
                            if(value == null) { return; }
                            entries[i].Key = key;
                            if(++this.count > n / 2)
                                this.Expand();
                            return;
                        }
                        if(value == null)
                            entries[i].Key = -1;
                        return;
                    }
                    i++;
                    if(i >= n)
                        i = 0;
                    k = entries[i].Key;
                }
            }
        }
        public TrivialHashtable Clone()
        {
            HashEntry[] clonedEntries = (HashEntry[])this.entries.Clone();
            //^ assume clonedEntries != null;
            return new TrivialHashtable(clonedEntries, this.count);
        }
    }

    public sealed class TrivialHashtableUsingWeakReferences
    {
        struct HashEntry
        {
            public int Key;
            public WeakReference Value;
        }

        private HashEntry[]/*!*/ entries;
        private int count;

        public TrivialHashtableUsingWeakReferences()
        {
            this.entries = new HashEntry[16];
            //this.count = 0;
        }
        private TrivialHashtableUsingWeakReferences(HashEntry[]/*!*/ entries, int count)
        {
            this.entries = entries;
            this.count = count;
        }
        public TrivialHashtableUsingWeakReferences(int expectedEntries)
        {
            int initialSize = 16;
            expectedEntries <<= 1;
            while(initialSize < expectedEntries && initialSize > 0)
                initialSize <<= 1;
            if(initialSize < 0)
                initialSize = 16;
            this.entries = new HashEntry[initialSize];
            //this.count = 0;
        }
        public int Count
        {
            get
            {
                return this.count;
            }
        }
        private void Expand()
        {
            HashEntry[] oldEntries = this.entries;
            int n = oldEntries.Length;
            int m = n * 2;
            if(m <= 0)
                return;
            HashEntry[] entries = new HashEntry[m];
            int count = 0;
            for(int i = 0; i < n; i++)
            {
                int key = oldEntries[i].Key;
                if(key <= 0)
                    continue; //No entry (0) or deleted entry (-1)
                WeakReference value = oldEntries[i].Value;
                Debug.Assert(value != null);
                if(value == null || !value.IsAlive)
                    continue; //Collected entry.
                int j = key & (m - 1);
                int k = entries[j].Key;
                while(true)
                {
                    if(k == 0)
                    {
                        entries[j].Value = value;
                        entries[j].Key = key;
                        count++;
                        break;
                    }
                    j++;
                    if(j >= m)
                        j = 0;
                    k = entries[j].Key;
                }
            }
            this.entries = entries;
            this.count = count;
        }
        private void Contract()
        {
            HashEntry[] oldEntries = this.entries;
            int n = oldEntries.Length;
            int m = n / 2;
            if(m < 16)
                return;
            HashEntry[] entries = new HashEntry[m];
            int count = 0;
            for(int i = 0; i < n; i++)
            {
                int key = oldEntries[i].Key;
                if(key <= 0)
                    continue; //No entry (0) or deleted entry (-1)
                WeakReference value = oldEntries[i].Value;
                Debug.Assert(value != null);
                if(value == null || !value.IsAlive)
                    continue; //Collected entry.
                int j = key & (m - 1);
                int k = entries[j].Key;
                while(true)
                {
                    if(k == 0)
                    {
                        entries[j].Value = value;
                        entries[j].Key = key;
                        count++;
                        break;
                    }
                    j++;
                    if(j >= m)
                        j = 0;
                    k = entries[j].Key;
                }
            }
            this.entries = entries;
            this.count = count;
        }
        private void WeedOutCollectedEntries()
        {
            HashEntry[] oldEntries = this.entries;
            int n = oldEntries.Length;
            HashEntry[] entries = new HashEntry[n];
            int count = 0;
            for(int i = 0; i < n; i++)
            {
                int key = oldEntries[i].Key;
                if(key <= 0)
                    continue; //No entry (0) or deleted entry (-1)
                WeakReference value = oldEntries[i].Value;
                Debug.Assert(value != null);
                if(value == null || !value.IsAlive)
                    continue; //Collected entry.
                int j = key & (n - 1);
                int k = entries[j].Key;
                while(true)
                {
                    if(k == 0)
                    {
                        entries[j].Value = value;
                        entries[j].Key = key;
                        count++;
                        break;
                    }
                    j++;
                    if(j >= n)
                        j = 0;
                    k = entries[j].Key;
                }
            }
            this.entries = entries;
            this.count = count;
        }
        public object this[int key]
        {
            get
            {
                if(key <= 0)
                    throw new ArgumentException(ExceptionStrings.KeyNeedsToBeGreaterThanZero, "key");
                HashEntry[] entries = this.entries;
                int n = entries.Length;
                int i = key & (n - 1);
                int k = entries[i].Key;
                object result = null;
                while(true)
                {
                    if(k == key)
                    {
                        WeakReference wref = entries[i].Value;
                        if(wref == null) { Debug.Assert(false); return null; }
                        result = wref.Target;
                        if(result != null)
                            return result;
                        this.WeedOutCollectedEntries();
                        while(this.count < n / 4 && n > 16) { this.Contract(); n = this.entries.Length; }
                        return null;
                    }
                    if(k == 0)
                        break;
                    i++;
                    if(i >= n)
                        i = 0;
                    k = entries[i].Key;
                }
                return result;
            }
            set
            {
                if(key <= 0)
                    throw new ArgumentException(ExceptionStrings.KeyNeedsToBeGreaterThanZero, "key");
                HashEntry[] entries = this.entries;
                int n = entries.Length;
                int i = key & (n - 1);
                int k = entries[i].Key;
                while(true)
                {
                    if(k == key || k == 0)
                    {
                        if(value == null)
                            entries[i].Value = null;
                        else
                            entries[i].Value = new WeakReference(value);
                        if(k == 0)
                        {
                            if(value == null)
                                return;
                            entries[i].Key = key;
                            if(++this.count > n / 2)
                            {
                                this.Expand(); //Could decrease this.count because of collected entries being deleted
                                while(this.count < n / 4 && n > 16) { this.Contract(); n = this.entries.Length; }
                            }
                            return;
                        }
                        if(value == null)
                            entries[i].Key = -1;
                        return;
                    }
                    i++;
                    if(i >= n)
                        i = 0;
                    k = entries[i].Key;
                }
            }
        }
        public TrivialHashtableUsingWeakReferences Clone()
        {
            HashEntry[] clonedEntries = (HashEntry[])this.entries.Clone();
            //^ assume clonedEntries != null;
            return new TrivialHashtableUsingWeakReferences(clonedEntries, this.count);
        }
    }

    public interface IUniqueKey
    {
        int UniqueId { get; }
    }

    /// <summary>
    /// A node in an Abstract Syntax Tree.
    /// </summary>
    public abstract class Node : IUniqueKey
    {

        public bool IsErroneous;

        /// <summary>
        /// The region in the source code that contains the concrete syntax corresponding to this node in the Abstract Syntax Tree.
        /// </summary>

        public SourceContext SourceContext;

#if DEBUG
        public string DebugLabel; // useful for debugging.
#endif
        protected Node(NodeType nodeType)
        {
            this.NodeType = nodeType;
        }

        private NodeType nodeType;

        /// <summary>
        /// A scalar tag that identifies the concrete type of the node. This is provided to allow efficient type membership tests that 
        /// facilitate tree traversal.
        /// </summary>
        public NodeType NodeType
        {
            get { return this.nodeType; }
            set { this.nodeType = value; }
        }
        private static int uniqueKeyCounter;
        private int uniqueKey;
        /// <summary>
        /// An integer that uniquely identifies this node. This provides an efficient equality test to facilitate hashing.
        /// Do not override this.
        /// </summary>
        public virtual int UniqueKey
        {
            get
            {
                if(this.uniqueKey == 0)
                {
TryAgain:
                    int c = Node.uniqueKeyCounter;
                    int cp1 = c + 17;
                    if(cp1 <= 0)
                        cp1 = 1000000;
                    if(System.Threading.Interlocked.CompareExchange(ref Node.uniqueKeyCounter, cp1, c) != c)
                        goto TryAgain;
                    this.uniqueKey = cp1;
                }
                return this.uniqueKey;
            }
        }
        /// <summary>
        /// Makes a shallow copy of the node.
        /// </summary>
        /// <returns>A shallow copy of the node</returns>
        public virtual Node/*!*/ Clone()
        {
            Node result = (Node)this.MemberwiseClone();
            result.uniqueKey = 0;
            return result;
        }

        public virtual object GetVisitorFor(object/*!*/ callingVisitor, string/*!*/ visitorClassName)
        {
            if(callingVisitor == null || visitorClassName == null) { Debug.Fail(""); return null; }
            return Node.GetVisitorFor(this.GetType(), callingVisitor, visitorClassName);
        }
        private static Hashtable VisitorTypeFor; //contains weak references
        private static Object GetVisitorFor(System.Type/*!*/ nodeType, object/*!*/ callingVisitor, string/*!*/ visitorClassName)
        {
            if(nodeType == null || callingVisitor == null || visitorClassName == null) { Debug.Fail(""); return null; }
            if(Node.VisitorTypeFor == null)
                Node.VisitorTypeFor = new Hashtable();
            string customVisitorClassName = visitorClassName;
            if(visitorClassName.IndexOf('.') < 0)
                customVisitorClassName = nodeType.Namespace + "." + visitorClassName;
            if(customVisitorClassName == callingVisitor.GetType().FullName)
            {
                Debug.Assert(false); //This must be a bug, the calling visitor is the one that should handle the nodeType
                return null;
            }
            System.Reflection.AssemblyName visitorAssemblyName = null;
            System.Reflection.Assembly assembly = null;
            WeakReference wref = (WeakReference)Node.VisitorTypeFor[customVisitorClassName];
            Type visitorType = wref == null ? null : (System.Type)wref.Target;
            if(visitorType == typeof(object))
                return null;
            string callerDirectory = null;
            if(visitorType == null)
            {
                assembly = nodeType.Assembly;
                if(assembly == null)
                    return null;
                visitorType = assembly.GetType(customVisitorClassName, false);
            }
            if(visitorType == null)
            {
                //^ assert assembly != null;
                if(assembly.Location == null)
                    return null;
                callerDirectory = Path.GetDirectoryName(assembly.Location);
                visitorAssemblyName = new System.Reflection.AssemblyName();
                visitorAssemblyName.Name = "Visitors";
                visitorAssemblyName.CodeBase = "file:///" + Path.Combine(callerDirectory, "Visitors.dll");
                try
                {
                    assembly = System.Reflection.Assembly.Load(visitorAssemblyName);
                }
                catch { }
                if(assembly != null)
                    visitorType = assembly.GetType(customVisitorClassName, false);
                if(visitorType == null)
                {
                    visitorAssemblyName.Name = customVisitorClassName;
                    visitorAssemblyName.CodeBase = "file:///" + Path.Combine(callerDirectory, customVisitorClassName + ".dll");
                    try
                    {
                        assembly = System.Reflection.Assembly.Load(visitorAssemblyName);
                    }
                    catch { }
                    if(assembly != null)
                        visitorType = assembly.GetType(customVisitorClassName, false);
                }
            }
            if(visitorType == null)
            {
                //Put fake entry into hash table to short circuit future lookups
                visitorType = typeof(object);
                assembly = nodeType.Assembly;
            }
            if(assembly != null)
            { //Only happens if there was a cache miss
                lock(Node.VisitorTypeFor)
                {
                    Node.VisitorTypeFor[customVisitorClassName] = new WeakReference(visitorType);
                }
            }
            if(visitorType == typeof(object))
                return null;
            try
            {
                return System.Activator.CreateInstance(visitorType, new object[] { callingVisitor });
            }
            catch { }
            return null;
        }

        int IUniqueKey.UniqueId { get { return this.UniqueKey; } }
    }

    public abstract class ErrorNode : Node
    {
        public int Code;
        public string[] MessageParameters;

        protected ErrorNode(int code, params string[] messageParameters)
            : base(NodeType.Undefined)
        {
            this.Code = code;
            this.MessageParameters = messageParameters;
        }
        public virtual string GetErrorNumber()
        {
            return this.Code.ToString("0000");
        }
        public string GetMessage()
        {
            return this.GetMessage(null);
        }

        public abstract string GetMessage(System.Globalization.CultureInfo culture);

        public virtual string GetMessage(string key, System.Resources.ResourceManager rm, System.Globalization.CultureInfo culture)
        {
            if(rm == null || key == null)
                return null;
            string localizedString = rm.GetString(key, culture);
            if(localizedString == null)
                localizedString = key;
            string[] messageParameters = this.MessageParameters;
            if(messageParameters == null || messageParameters.Length == 0)
                return localizedString;
            return string.Format(localizedString, messageParameters);
        }
        public abstract int Severity
        {
            get;
        }
        public static int GetCountAtSeverity(ErrorNodeList errors, int minSeverity, int maxSeverity)
        {
            if(errors == null)
                return 0;
            int n = 0;
            for(int i = 0; i < errors.Count; i++)
            {
                ErrorNode e = errors[i];
                if(e == null)
                    continue;
                int s = e.Severity;
                if(minSeverity <= s && s <= maxSeverity)
                    n++;
            }
            return n;
        }
    }

    public class Expose : Statement
    {
        public Expression Instance;
        public Block Body;
        public bool IsLocal;
        public Expose(NodeType nodeType)
            : base(nodeType)
        {
        }
    }
    public class Acquire : Statement
    {
        public bool ReadOnly;
        public Statement Target;
        public Expression Condition;
        public Expression ConditionFunction;
        public Block Body;
        public BlockScope ScopeForTemporaryVariable;
        public Acquire()
            : base(NodeType.Acquire)
        {
        }
    }

    public class Expression : Node
    {
        private TypeNode type;

        public Expression(NodeType nodeType)
            : base(nodeType)
        {
        }
        public Expression(NodeType nodeType, TypeNode type)
            : base(nodeType)
        {
            this.type = type;
        }
        public virtual TypeNode Type
        {
            get { return this.type; }
            set { this.type = value; }
        }
    }

    public class ExpressionSnippet : Expression
    {
        public IParserFactory ParserFactory;

        public ExpressionSnippet()
            : base(NodeType.ExpressionSnippet)
        {
        }
        public ExpressionSnippet(IParserFactory parserFactory, SourceContext sctx)
            : base(NodeType.ExpressionSnippet)
        {
            this.ParserFactory = parserFactory;
            this.SourceContext = sctx;
        }
    }

    public class MemberBinding : Expression
    {
        private int alignment;
        private Member boundMember;

        public Expression BoundMemberExpression;

        private Expression targetObject;
        private bool @volatile;
        public MemberBinding()
            : base(NodeType.MemberBinding)
        {
        }
        public MemberBinding(Expression targetObject, Member/*!*/ boundMember)
            : this(targetObject, boundMember, false, -1)
        {
            if(boundMember is Field)
                this.Volatile = ((Field)boundMember).IsVolatile;
        }

        public MemberBinding(Expression targetObject, Member/*!*/ boundMember, Expression boundMemberExpression)
            : this(targetObject, boundMember, false, -1)
        {
            if(boundMember is Field)
                this.Volatile = ((Field)boundMember).IsVolatile;
            this.BoundMemberExpression = boundMemberExpression;
        }
        public MemberBinding(Expression targetObject, Member/*!*/ boundMember, SourceContext sctx)
            : this(targetObject, boundMember, false, -1)
        {
            if(boundMember is Field)
                this.Volatile = ((Field)boundMember).IsVolatile;
            this.SourceContext = sctx;
        }
        public MemberBinding(Expression targetObject, Member/*!*/ boundMember, SourceContext sctx, Expression boundMemberExpression)
            : this(targetObject, boundMember, false, -1)
        {
            if(boundMember is Field)
                this.Volatile = ((Field)boundMember).IsVolatile;
            this.SourceContext = sctx;
            this.BoundMemberExpression = boundMemberExpression;
        }

        public MemberBinding(Expression targetObject, Member/*!*/ boundMember, bool @volatile, int alignment)
            : base(NodeType.MemberBinding)
        {
            Debug.Assert(boundMember != null);
            this.alignment = alignment;
            this.boundMember = boundMember;
            this.targetObject = targetObject;
            this.@volatile = @volatile;
            switch(boundMember.NodeType)
            {
                case NodeType.Field:
                    this.Type = ((Field)boundMember).Type;
                    break;
                case NodeType.Method:
                    this.Type = ((Method)boundMember).ReturnType;
                    break;
                case NodeType.Event:
                    this.Type = ((Event)boundMember).HandlerType;
                    break;
                default:
                    this.Type = boundMember as TypeNode;
                    break;
            }
        }
        public int Alignment
        {
            get { return this.alignment; }
            set { this.alignment = value; }
        }
        public Member BoundMember
        {
            get { return this.boundMember; }
            set { this.boundMember = value; }
        }
        public Expression TargetObject
        {
            get { return this.targetObject; }
            set { this.targetObject = value; }
        }
        public bool Volatile
        {
            get { return this.@volatile; }
            set { this.@volatile = value; }
        }
    }
    public class AddressDereference : Expression
    {
        private Expression address;
        private int alignment;
        private bool isVolatile;

        public enum ExplicitOp { None = 0, Star, Arrow }

        private ExplicitOp explicitOperation; // was explicit in source (* or ->)

        public AddressDereference()
            : base(NodeType.AddressDereference)
        {
        }
        public AddressDereference(Expression address, TypeNode type)
            : this(address, type, false, -1)
        {
        }

        public AddressDereference(Expression address, TypeNode type, SourceContext sctx)
            : this(address, type, false, -1, sctx)
        {
        }

        public AddressDereference(Expression address, TypeNode type, bool isVolatile, int alignment)
            : base(NodeType.AddressDereference)
        {
            this.address = address;
            this.alignment = alignment;
            this.Type = type;
            this.isVolatile = isVolatile;
        }

        public AddressDereference(Expression address, TypeNode type, bool Volatile, int alignment, SourceContext sctx)
            : base(NodeType.AddressDereference)
        {
            this.address = address;
            this.alignment = alignment;
            this.Type = type;
            this.isVolatile = Volatile;
            this.SourceContext = sctx;
        }

        public Expression Address
        {
            get { return this.address; }
            set { this.address = value; }
        }
        public int Alignment
        {
            get { return this.alignment; }
            set { this.alignment = value; }
        }
        public bool Volatile
        {
            get { return this.isVolatile; }
            set { this.isVolatile = value; }
        }

        public bool Explicit
        {
            get { return this.explicitOperation != ExplicitOp.None; }
        }
        public ExplicitOp ExplicitOperator
        {
            get { return this.explicitOperation; }
            set { this.explicitOperation = value; }
        }

    }

    public class UnaryExpression : Expression
    {
        private Expression operand;
        public UnaryExpression()
            : base(NodeType.Nop)
        {
        }
        public UnaryExpression(Expression operand, NodeType nodeType)
            : base(nodeType)
        {
            this.Operand = operand;
        }

        public UnaryExpression(Expression operand, NodeType nodeType, SourceContext sctx)
            : base(nodeType)
        {
            this.operand = operand;
            this.SourceContext = sctx;
        }

        public UnaryExpression(Expression operand, NodeType nodeType, TypeNode type)
            : base(nodeType)
        {
            this.operand = operand;
            this.Type = type;
        }

        public UnaryExpression(Expression operand, NodeType nodeType, TypeNode type, SourceContext sctx)
            : base(nodeType)
        {
            this.operand = operand;
            this.Type = type;
            this.SourceContext = sctx;
        }

        public Expression Operand
        {
            get { return this.operand; }
            set { this.operand = value; }
        }
    }

    public class PrefixExpression : Expression
    {
        public Expression Expression;
        public NodeType Operator;
        public Method OperatorOverload;
        public PrefixExpression()
            : base(NodeType.PrefixExpression)
        {
        }
        public PrefixExpression(Expression expression, NodeType Operator, SourceContext sourceContext)
            : base(NodeType.PrefixExpression)
        {
            this.Expression = expression;
            this.Operator = Operator;
            this.SourceContext = sourceContext;
        }
    }
    public class PostfixExpression : Expression
    {
        public Expression Expression;
        public NodeType Operator;
        public Method OperatorOverload;
        public PostfixExpression()
            : base(NodeType.PostfixExpression)
        {
        }
        public PostfixExpression(Expression expression, NodeType Operator, SourceContext sourceContext)
            : base(NodeType.PostfixExpression)
        {
            this.Expression = expression;
            this.Operator = Operator;
            this.SourceContext = sourceContext;
        }
    }

    public class BinaryExpression : Expression
    {
        private Expression operand1;
        private Expression operand2;
        public BinaryExpression()
            : base(NodeType.Nop)
        {
        }
        public BinaryExpression(Expression operand1, Expression operand2, NodeType nodeType)
            : base(nodeType)
        {
            this.operand1 = operand1;
            this.operand2 = operand2;
        }
        public BinaryExpression(Expression operand1, Expression operand2, NodeType nodeType, TypeNode resultType)
            : base(nodeType)
        {
            this.operand1 = operand1;
            this.operand2 = operand2;
            this.Type = resultType;
        }

        public BinaryExpression(Expression operand1, Expression operand2, NodeType nodeType, SourceContext ctx)
            : base(nodeType)
        {
            this.operand1 = operand1;
            this.operand2 = operand2;
            this.SourceContext = ctx;
        }
        public BinaryExpression(Expression operand1, Expression operand2, NodeType nodeType, TypeNode resultType, SourceContext ctx)
            : base(nodeType)
        {
            this.operand1 = operand1;
            this.operand2 = operand2;
            this.Type = resultType;
            this.SourceContext = ctx;
        }

        public Expression Operand1
        {
            get { return this.operand1; }
            set { this.operand1 = value; }
        }
        public Expression Operand2
        {
            get { return this.operand2; }
            set { this.operand2 = value; }
        }
    }
    public class TernaryExpression : Expression
    {
        private Expression operand1;
        private Expression operand2;
        private Expression operand3;
        public TernaryExpression()
            : base(NodeType.Nop)
        {
        }
        public TernaryExpression(Expression operand1, Expression operand2, Expression operand3, NodeType nodeType, TypeNode resultType)
            : base(nodeType)
        {
            this.operand1 = operand1;
            this.operand2 = operand2;
            this.operand3 = operand3;
            this.Type = resultType;
        }
        public Expression Operand1
        {
            get { return this.operand1; }
            set { this.operand1 = value; }
        }
        public Expression Operand2
        {
            get { return this.operand2; }
            set { this.operand2 = value; }
        }
        public Expression Operand3
        {
            get { return this.operand3; }
            set { this.operand3 = value; }
        }
    }
    public abstract class NaryExpression : Expression
    {
        public ExpressionList Operands;

        protected NaryExpression()
            : base(NodeType.Nop)
        {
        }
        protected NaryExpression(ExpressionList operands, NodeType nodeType)
            : base(nodeType)
        {
            this.Operands = operands;
        }
    }

    public class ApplyToAll : BinaryExpression
    {
        public Local ElementLocal;
        public Method ResultIterator;
        public ApplyToAll()
            : base(null, null, NodeType.ApplyToAll)
        {
        }
        public ApplyToAll(Expression operand1, Expression operand2)
            : base(operand1, operand2, NodeType.ApplyToAll)
        {
        }
        public ApplyToAll(Expression operand1, Expression operand2, SourceContext ctx)
            : base(operand1, operand2, NodeType.ApplyToAll)
        {
            this.SourceContext = ctx;
        }
    }

    public class NamedArgument : Expression
    {
        private bool isCustomAttributeProperty;
        private Identifier name;
        private Expression value;
        private bool valueIsBoxed;
        public NamedArgument()
            : base(NodeType.NamedArgument)
        {
        }
        public NamedArgument(Identifier name, Expression value)
            : base(NodeType.NamedArgument)
        {
            this.Name = name;
            this.Value = value;
        }

        public NamedArgument(Identifier name, Expression value, SourceContext ctx)
            : base(NodeType.NamedArgument)
        {
            this.Name = name;
            this.Value = value;
            this.SourceContext = ctx;
        }

        public bool IsCustomAttributeProperty
        { //TODO: rename this to IsProperty
            get { return this.isCustomAttributeProperty; }
            set { this.isCustomAttributeProperty = value; }
        }
        public Identifier Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public Expression Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        public bool ValueIsBoxed
        {
            get { return this.valueIsBoxed; }
            set { this.valueIsBoxed = value; }
        }
    }

    /// <summary>
    /// This an Expression wrapper for compile time constants. It is assumed to be correct by construction.
    /// In Normalized IR, the wrapped value must be a primitive numeric type or an enum or a string or null.
    /// If used in custom attributes, types are also allowed as well as single dimensional arrays of other allowed types.
    /// If the wrapped value is null, any reference type is allowed, except in custom attributes, where it must be Type or String.
    /// </summary>
    public class Literal : Expression
    {
        private object value;

        public bool TypeWasExplicitlySpecifiedInSource;
        public Expression SourceExpression;

        public Literal()
            : base(NodeType.Literal)
        {
        }

        public Literal(object Value)
            : base(NodeType.Literal)
        {
            this.value = Value;
        }

        public Literal(object value, TypeNode type)
            : base(NodeType.Literal)
        {
            this.value = value;
            this.Type = type;
        }
        public Literal(object value, TypeNode type, SourceContext sourceContext)
            : base(NodeType.Literal)
        {
            this.value = value;
            this.SourceContext = sourceContext;
            this.Type = type;
        }
        /// <summary>
        /// Holds the wrapped compile time constant value.
        /// </summary>
        public object Value
        {
            get { return this.value; }
        }
        public override string ToString()
        {
            if(this.Value == null)
                return "Literal for null";
            return this.Value.ToString();
        }
    }

    public class This : Parameter
    {
        public This()
        {
            this.NodeType = NodeType.This;
            this.Name = StandardIds.This;
        }
        public This(TypeNode type)
        {
            this.NodeType = NodeType.This;
            this.Name = StandardIds.This;
            this.Type = type;
        }

        public bool IsCtorCall = false;
        public This(SourceContext sctx, bool isCtorCall)
        {
            this.NodeType = NodeType.This;
            this.Name = StandardIds.This;
            this.SourceContext = sctx;
            this.IsCtorCall = isCtorCall;
        }
        public This(TypeNode type, SourceContext sctx)
        {
            this.NodeType = NodeType.This;
            this.Name = StandardIds.This;
            this.Type = type;
            this.SourceContext = sctx;
        }
        public override bool Equals(object obj)
        {
            ThisBinding binding = obj as ThisBinding;
            return obj == this || binding != null && binding.BoundThis == this;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }

    public class ThisBinding : This, IUniqueKey
    {
        public This/*!*/ BoundThis;
        public ThisBinding(This/*!*/ boundThis, SourceContext sctx)
        {
            if(boundThis == null)
                throw new ArgumentNullException("boundThis");
            this.BoundThis = boundThis;
            this.SourceContext = sctx;
            this.Type = boundThis.Type;
            this.Name = boundThis.Name;
            this.TypeExpression = boundThis.TypeExpression;
            this.Attributes = boundThis.Attributes;
            this.DefaultValue = boundThis.DefaultValue;
            this.Flags = boundThis.Flags;
            this.MarshallingInformation = boundThis.MarshallingInformation;
            this.DeclaringMethod = boundThis.DeclaringMethod;
            this.ParameterListIndex = boundThis.ParameterListIndex;
            this.ArgumentListIndex = boundThis.ArgumentListIndex;
            //^ base();
        }
        public override int GetHashCode()
        {
            return this.BoundThis.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            ThisBinding pb = obj as ThisBinding;
            if(pb != null)
                return this.BoundThis.Equals(pb.BoundThis);
            else
                return this.BoundThis.Equals(obj);
        }
        int IUniqueKey.UniqueId
        {
            get { return this.BoundThis.UniqueKey; }
        }
        /// <summary>
        /// Must forward type to underlying binding, since ThisBindings get built at times when
        /// the bound This node does not have its final type yet.
        /// </summary>
        public override TypeNode Type
        {
            get
            {
                return BoundThis.Type;
            }
            set
            {
                BoundThis.Type = value;
            }
        }
    }
    public class Base : Expression
    {
        /// <summary>
        /// When the source uses the C# compatibility mode, base calls cannot be put after non-null
        /// field initialization, but must be put before the body. But the user can specify where
        /// the base ctor call should be performed by using "base;" as a marker. During parsing
        /// this flag is set so the right code transformations can be performed at code generation.
        /// </summary>
        public bool UsedAsMarker;
        public bool IsCtorCall = false;
        public Base()
            : base(NodeType.Base)
        {
        }
        public Base(SourceContext sctx, bool isCtorCall)
            : base(NodeType.Base)
        {
            this.SourceContext = sctx;
            this.IsCtorCall = isCtorCall;
        }
    }
    public class ImplicitThis : Expression
    {
        public int LexLevel;
        public Class MostNestedScope;
        public ImplicitThis()
            : base(NodeType.ImplicitThis)
        {
        }
        public ImplicitThis(Class mostNestedScope, int lexLevel)
            : base(NodeType.ImplicitThis)
        {
            this.LexLevel = lexLevel;
            this.MostNestedScope = mostNestedScope;
        }
    }
    public class CurrentClosure : Expression
    {
        public Method Method;
        public CurrentClosure()
            : base(NodeType.CurrentClosure)
        {
        }
        public CurrentClosure(Method method, TypeNode type)
            : base(NodeType.CurrentClosure)
        {
            this.Method = method;
            this.Type = type;
        }
        public CurrentClosure(Method method, TypeNode type, SourceContext sctx)
            : base(NodeType.CurrentClosure)
        {
            this.Method = method;
            this.Type = type;
            this.SourceContext = sctx;
        }
    }
    public class SetterValue : Expression
    {
        public SetterValue()
            : base(NodeType.SetterValue)
        {
        }
    }

    public class Identifier : Expression
    {
        private int hashCode;
        internal int length;
        private string name;
        private int offset;

        private DocumentText text;

        public Identifier Prefix;

        /// <summary>An identifier with the empty string ("") as its value.</summary>
        public static readonly Identifier/*!*/ Empty = new Identifier("");

        private Identifier(DocumentText/*!*/ text, int offset, int length)
            : base(NodeType.Identifier)
        {
            this.text = text;
            this.offset = offset;
            this.length = length;
            ulong hcode = 0;
            for(int i = offset, n = length + i; i < n; i++)
            {
                char ch = text[i];
                hcode = hcode * 17 + ch;
            }
            this.hashCode = ((int)hcode) & int.MaxValue;
        }
        public static Identifier/*!*/ For(SourceContext sctx)
        {
            DocumentText text = null;
            if(sctx.Document != null)
                text = sctx.Document.Text;
            if(text == null)
                text = new DocumentText("");
            Identifier id = new Identifier(text, sctx.StartPos, sctx.EndPos - sctx.StartPos);
            id.SourceContext = sctx;
            return id;
        }

        public Identifier(string name)
            : base(NodeType.Identifier)
        {
            if(name == null)
                name = "";
            this.name = name;
            int n = this.length = name.Length;
            ulong hcode = 0;
            for(int i = 0; i < n; i++)
            {
                char ch = name[i];
                hcode = hcode * 17 + ch;
            }
            this.hashCode = ((int)hcode) & int.MaxValue;
        }

        public Identifier(string name, SourceContext sctx)
            : this(name)
        {
            this.SourceContext = sctx;
        }

        public static Identifier/*!*/ For(string/*!*/ name)
        {
            return new Identifier(name);
        }
        private unsafe Identifier(byte* pointer, int offset)
            : base(NodeType.Identifier)
        {
            this.offset = offset;
            bool isASCII = true;
            int length = 0;
            ulong hcode = 0;
            for(int i = offset; ; i++)
            {
                byte b = *(pointer + i);
                if(b == 0)
                    break;
                if((b & 0x80) != 0)
                    isASCII = false;
                hcode = hcode * 17 + b;
                length++;
            }
            if(isASCII)
            {
                this.hashCode = ((int)hcode) & int.MaxValue;
                this.length = length;
                this.name = new string((sbyte*)pointer, offset, length, Encoding.ASCII);
                return;
            }
            hcode = 0;
            string name = this.name = new string((sbyte*)pointer, offset, length, Encoding.UTF8);
            for(int i = 0, n = this.length = name.Length; i < n; i++)
            {
                char ch = name[i];
                hcode = hcode * 17 + ch;
            }
            this.hashCode = ((int)hcode) & int.MaxValue;
        }
        /// <summary>
        /// Use when pointer+offset points to a null terminated string of UTF8 code points.
        /// </summary>
        internal unsafe static Identifier/*!*/ For(byte* pointer, int offset)
        {
            //TODO: first look for identifier in cache
            return new Identifier(pointer, offset);
        }

        private unsafe Identifier(byte* pointer, uint length)
            : base(NodeType.Identifier)
        {
            //this.offset = 0;
            this.length = (int)length;
            ulong hcode = 0;
            for(uint i = 0; i < length; i++)
            {
                byte b = *(pointer + i);
                if((b & 0x80) != 0)
                    goto doUTF8decoding;
                hcode = hcode * 17 + b;
            }
            this.hashCode = ((int)hcode) & int.MaxValue;
            this.name = new string((sbyte*)pointer, 0, this.length, Encoding.ASCII);
            return;
doUTF8decoding:
            string name = this.name = new string((sbyte*)pointer, 0, this.length, Encoding.UTF8);
            for(int i = 0, n = this.length = name.Length; i < n; i++)
            {
                char ch = name[i];
                hcode = hcode * 17 + ch;
            }
            this.hashCode = ((int)hcode) & int.MaxValue;
        }
        /// <summary>
        /// Use when pointer points to a string of UTF8 code points of a given length
        /// </summary>
        internal unsafe static Identifier/*!*/ For(byte* pointer, uint length)
        {
            //TODO: first look for identifier in cache
            return new Identifier(pointer, length);
        }
        private static readonly object/*!*/ Lock = new object();
        private struct CanonicalIdentifier
        {
            internal string/*!*/ Name;
            internal int UniqueIdKey;
            internal int HashCode;

            internal CanonicalIdentifier(string/*!*/ name, int uniqueIdKey, int hashCode)
            {
                this.Name = name;
                this.UniqueIdKey = uniqueIdKey;
                this.HashCode = hashCode;
            }
        }
        private static CanonicalIdentifier[]/*!*/ HashTable = new CanonicalIdentifier[16 * 1024];
        private static int count;
        private int GetUniqueIdKey()
        {
            lock(Identifier.Lock)
            {
                int hcode = this.hashCode;
                CanonicalIdentifier[] hTable = Identifier.HashTable;
                int length = hTable.Length;
                int i = hcode % length;
                CanonicalIdentifier id = hTable[i];
                while(id.Name != null)
                {
                    if(this.HasSameNameAs(id))
                        return id.UniqueIdKey;
                    i = (i + 1) % length;
                    id = hTable[i];
                }
                int count = Identifier.count;
                int countp1 = count + 1;
                Identifier.count = countp1;
                string name = this.Name; //Get a local copy of the name and drop any reference to a DocumentText instance
                hTable[i] = new CanonicalIdentifier(name, countp1, hcode);
                if(countp1 > length / 2)
                    Rehash(); //Threshold exceeded, need to rehash        
                return countp1;
            }
        }
        private unsafe bool HasSameNameAs(CanonicalIdentifier id)
        {
            int myLength = this.length;
            int idLength = id.Name.Length;
            if(myLength != idLength)
                return false;
            string myName = this.name;
            string idName = id.Name;

            if(myName == null)
            {
                int myOffset = this.offset;
                if(this.text != null && this.text.Equals(idName, myOffset, myLength))
                {
                    this.name = idName;
                    this.text = null;
                    return true;
                }
                return false;
            }

            return myName == idName;
        }
        public string/*!*/ Name
        { //TODO: need a better name for this property
            get
            {
                if(this.name != null)
                    return this.name;
                lock(this)
                {
                    if(this.name != null)
                        return this.name;
                    //^ assume this.text != null;
                    int length = this.length;
                    int offset = this.offset;
                    this.name = this.text.Substring(offset, length);
                    this.text = null;
                    return this.name;
                }
            }
        }
        private static void Rehash()
        {
            CanonicalIdentifier[] hTable = Identifier.HashTable;
            int n = hTable.Length;
            int n2 = n * 2;
            CanonicalIdentifier[] newhTable = new CanonicalIdentifier[n2];
            for(int i = 0; i < n; i++)
            {
                CanonicalIdentifier id = hTable[i];
                if(id.Name == null)
                    continue;
                int j = id.HashCode % n2;
                CanonicalIdentifier id2 = newhTable[j];
                while(id2.Name != null)
                {
                    j = (j + 1) % n2;
                    id2 = newhTable[j];
                }
                newhTable[j] = id;
            }
            Identifier.HashTable = newhTable;
        }
        public override string/*!*/ ToString()
        {

            if(this.Prefix != null)
                return this.Prefix.Name + ":" + this.Name;

            if(this.Name == null)
                return "";
            return this.Name;
        }

        private int uniqueIdKey;

        /// <summary>
        /// Returns an integer that is the same for every Identifier instance that has the same string value, and that is different from
        /// every other identifier instance that has a different string value. Useful for efficient equality tests when hashing identifiers.
        /// </summary>
        public int UniqueIdKey
        {
            get
            {
                int result = this.uniqueIdKey;
                if(result != 0)
                    return result;
                return this.uniqueIdKey = this.GetUniqueIdKey();
            }
        }
        [Obsolete("Use Identifier.UniqueIdKey instead")]
        public new int UniqueKey
        {
            get
            {
                int result = this.uniqueIdKey;
                if(result != 0)
                    return result;
                return this.uniqueIdKey = this.GetUniqueIdKey();
            }
        }
    }

    public class QualifiedIdentifier : Expression
    {
        public Identifier Identifier;
        public Expression Qualifier;
        public Expression BoundMember;
        public bool QualifierIsNamespace;

        public QualifiedIdentifier()
            : base(NodeType.QualifiedIdentifer)
        {
        }
        public QualifiedIdentifier(Expression qualifier, Identifier identifier)
            : base(NodeType.QualifiedIdentifer)
        {
            this.Identifier = identifier;
            this.Qualifier = qualifier;
        }
        public QualifiedIdentifier(Expression qualifier, Identifier identifier, SourceContext sourceContext)
            : base(NodeType.QualifiedIdentifer)
        {
            this.Identifier = identifier;
            this.Qualifier = qualifier;
            this.SourceContext = sourceContext;
        }
        public QualifiedIdentifier(Expression qualifier, Identifier identifier, SourceContext sourceContext, bool qualifierIsNamespace)
            : base(NodeType.QualifiedIdentifer)
        {
            this.Identifier = identifier;
            this.Qualifier = qualifier;
            this.SourceContext = sourceContext;
            this.QualifierIsNamespace = qualifierIsNamespace;
        }
        public override string/*!*/ ToString()
        {
            string str = this.Identifier == null ? "" : this.Identifier.ToString();
            if(this.Qualifier == null)
                return str;
            string separator = this.QualifierIsNamespace ? "::" : "+";
            return this.Qualifier.ToString() + separator + str;
        }
    }
    public class Quantifier : Expression
    {
        public NodeType QuantifierType;
        public TypeNode SourceType; // the type of elements the quantifier consumes
        public Comprehension Comprehension;
        public Quantifier()
            : base(NodeType.Quantifier)
        {
        }
        public Quantifier(Comprehension comprehension)
            : base(NodeType.Quantifier)
        {
            this.Comprehension = comprehension;
        }
        public Quantifier(NodeType t, Comprehension comprehension)
            : base(NodeType.Quantifier)
        {
            this.QuantifierType = t;
            this.Comprehension = comprehension;
        }
    }
    public enum ComprehensionBindingMode { In, Gets };
    public class ComprehensionBinding : Expression
    {
        public ComprehensionBindingMode Mode = ComprehensionBindingMode.In;
        public TypeNode TargetVariableType;
        public TypeNode TargetVariableTypeExpression;
        public Expression TargetVariable;

        public TypeNode AsTargetVariableType;
        public TypeNode AsTargetVariableTypeExpression;

        public Expression SourceEnumerable;
        public BlockScope ScopeForTemporaryVariables;
        public ComprehensionBinding()
            : base(NodeType.ComprehensionBinding)
        {
        }
    }
    public enum ComprehensionMode { Reduction, Comprehension };
    // {1,2,3} ==> Comprehension with BindingsAndFilters = null and Elements = [1,2,3]
    // i.e., for a "display", there are no bindings and the elements have one entry per value in the comprehension
    // { int x in A, P(x); T(x); default } ==> Comprehension with BindingsAndFilters = [int x in A, P(x)] and Elements = [T(x), default]
    // i.e., for "true" comprehensions, the list of elements will always have either one or two elements (two if there is a default)
    public class Comprehension : Expression
    {
        public ComprehensionMode Mode = ComprehensionMode.Comprehension;
        public ExpressionList BindingsAndFilters;
        public ExpressionList Elements;

        public Node nonEnumerableTypeCtor; // used only when the comprehension should generate code for an IList, e.g.
        public Method AddMethod; // used only when the comprehension should generate code for an IList, e.g.
        public TypeNode TemporaryHackToHoldType;

        public Comprehension()
            : base(NodeType.Comprehension)
        {
        }

        public bool IsDisplay
        {
            get
            {
                return this.BindingsAndFilters == null;
            }
        }
    }
    public class NameBinding : Expression
    {
        public Identifier Identifier;
        public MemberList BoundMembers;
        public Expression BoundMember;
        public int LexLevel;
        public Class MostNestedScope;
        public NameBinding()
            : base(NodeType.NameBinding)
        {
        }
        public NameBinding(Identifier identifier, MemberList boundMembers)
            : base(NodeType.NameBinding)
        {
            this.Identifier = identifier;
            this.BoundMembers = boundMembers;
        }
        public NameBinding(Identifier identifier, MemberList boundMembers, SourceContext sctx)
            : base(NodeType.NameBinding)
        {
            this.Identifier = identifier;
            this.BoundMembers = boundMembers;
            this.SourceContext = sctx;
        }
        public NameBinding(Identifier identifier, MemberList boundMembers, Class mostNestedScope, int lexLevel)
            : base(NodeType.NameBinding)
        {
            this.Identifier = identifier;
            this.BoundMembers = boundMembers;
            this.LexLevel = lexLevel;
            this.MostNestedScope = mostNestedScope;
        }
        public NameBinding(Identifier identifier, MemberList boundMembers, Class mostNestedScope, int lexLevel, SourceContext sctx)
            : base(NodeType.NameBinding)
        {
            this.Identifier = identifier;
            this.BoundMembers = boundMembers;
            this.LexLevel = lexLevel;
            this.MostNestedScope = mostNestedScope;
            this.SourceContext = sctx;
        }
        public override string ToString()
        {
            return this.Identifier == null ? "" : this.Identifier.ToString();
        }
    }
    public class TemplateInstance : Expression
    {
        public Expression Expression;
        public TypeNodeList TypeArguments;
        public TypeNodeList TypeArgumentExpressions;
        public bool IsMethodTemplate;
        public MemberList BoundMembers;

        public TemplateInstance()
            : this(null, null)
        {
        }
        public TemplateInstance(Expression expression, TypeNodeList typeArguments)
            : base(NodeType.TemplateInstance)
        {
            this.Expression = expression;
            this.TypeArguments = typeArguments;
        }
    }
    public class StackAlloc : Expression
    {
        public TypeNode ElementType;
        public TypeNode ElementTypeExpression;
        public Expression NumberOfElements;

        public StackAlloc()
            : base(NodeType.StackAlloc)
        {
        }
        public StackAlloc(TypeNode elementType, Expression numberOfElements, SourceContext sctx)
            : base(NodeType.StackAlloc)
        {
            this.ElementType = this.ElementTypeExpression = elementType;
            this.NumberOfElements = numberOfElements;
            this.SourceContext = sctx;
        }
    }

    public class MethodCall : NaryExpression
    {
        private Expression callee;
        private TypeNode constraint;
        private bool isTailCall;

        public Expression CalleeExpression;
        public bool GiveErrorIfSpecialNameMethod;
        public bool ArgumentListIsIncomplete;
        public MethodCall()
        {
            this.NodeType = NodeType.MethodCall;
        }
        public MethodCall(Expression callee, ExpressionList arguments)
            : base(arguments, NodeType.MethodCall)
        {
            this.callee = this.CalleeExpression = callee;
            this.isTailCall = false;
        }

        public MethodCall(Expression callee, ExpressionList arguments, NodeType typeOfCall)
            : base(arguments, typeOfCall)
        {
            this.callee = callee;

            this.CalleeExpression = callee;

            //this.isTailCall = false;
        }

        public MethodCall(Expression callee, ExpressionList arguments, NodeType typeOfCall, TypeNode resultType)
            : this(callee, arguments, typeOfCall)
        {
            this.Type = resultType;
        }
        public MethodCall(Expression callee, ExpressionList arguments, NodeType typeOfCall, TypeNode resultType, SourceContext sctx)
            : this(callee, arguments, typeOfCall, resultType)
        {
            this.SourceContext = sctx;
        }

        public Expression Callee
        {
            get { return this.callee; }
            set { this.callee = value; }
        }
        public bool IsTailCall
        {
            get { return this.isTailCall; }
            set { this.isTailCall = value; }
        }
        public TypeNode Constraint
        {
            get { return this.constraint; }
            set { this.constraint = value; }
        }
    }
    public class Construct : NaryExpression
    {
        private Expression constructor;

        public Expression Owner;

        public Construct()
        {
            this.NodeType = NodeType.Construct;
        }
        public Construct(Expression constructor, ExpressionList arguments)
            : base(arguments, NodeType.Construct)
        {
            this.constructor = constructor;
        }

        public Construct(Expression constructor, ExpressionList arguments, SourceContext sctx)
            : base(arguments, NodeType.Construct)
        {
            this.constructor = constructor;
            this.SourceContext = sctx;
        }
        public Construct(Expression constructor, ExpressionList arguments, TypeNode type)
            : base(arguments, NodeType.Construct)
        {
            this.constructor = constructor;
            this.Type = type;
        }
        public Construct(Expression constructor, ExpressionList arguments, TypeNode type, SourceContext sctx)
            : base(arguments, NodeType.Construct)
        {
            this.constructor = constructor;
            this.Type = type;
            this.SourceContext = sctx;
        }

        public Expression Constructor
        {
            get { return this.constructor; }
            set { this.constructor = value; }
        }
    }
    public class ConstructArray : NaryExpression
    {
        private TypeNode elementType;
        private int rank;

        public TypeNode ElementTypeExpression;
        public ExpressionList Initializers;
        public Expression Owner;

        public ConstructArray()
        {
            this.NodeType = NodeType.ConstructArray;
            this.rank = 1;
        }
        public ConstructArray(TypeNode elementType, ExpressionList sizes, ExpressionList initializers)
            : base(sizes, NodeType.ConstructArray)
        {
            this.elementType = elementType;
            this.Operands = sizes;
            this.rank = sizes == null ? 1 : sizes.Count;

            this.Initializers = initializers;
        }

        public ConstructArray(TypeNode elementType, ExpressionList initializers)
            : base(null, NodeType.ConstructArray)
        {
            this.elementType = elementType;
            this.Initializers = initializers;
            this.rank = 1;
            if(elementType != null)
                this.Type = elementType.GetArrayType(1);
        }
        public ConstructArray(TypeNode elementType, int rank, ExpressionList initializers)
            : base(null, NodeType.ConstructArray)
        {
            this.elementType = elementType;
            this.Initializers = initializers;
            this.rank = rank;
            if(elementType != null)
                this.Type = elementType.GetArrayType(1);
        }

        public TypeNode ElementType
        {
            get { return this.elementType; }
            set { this.elementType = value; }
        }
        public int Rank
        {
            get { return this.rank; }
            set { this.rank = value; }
        }
    }

    public class ConstructFlexArray : NaryExpression
    {
        public TypeNode ElementType;
        public TypeNode ElementTypeExpression;
        public ExpressionList Initializers;
        public ConstructFlexArray()
        {
            this.NodeType = NodeType.ConstructFlexArray;
        }
        public ConstructFlexArray(TypeNode elementType, ExpressionList sizes, ExpressionList initializers)
            : base(sizes, NodeType.ConstructFlexArray)
        {
            this.ElementType = elementType;
            this.Operands = sizes;
            this.Initializers = initializers;
        }
    }
    public class ConstructDelegate : Expression
    {
        public TypeNode DelegateType;
        public TypeNode DelegateTypeExpression;
        public Identifier MethodName;
        public Expression TargetObject;
        public ConstructDelegate()
            : base(NodeType.ConstructDelegate)
        {
        }
        public ConstructDelegate(TypeNode delegateType, Expression targetObject, Identifier methodName)
            : base(NodeType.ConstructDelegate)
        {
            this.DelegateType = delegateType;
            this.MethodName = methodName;
            this.TargetObject = targetObject;
        }
        public ConstructDelegate(TypeNode delegateType, Expression targetObject, Identifier methodName, SourceContext sctx)
            : base(NodeType.ConstructDelegate)
        {
            this.DelegateType = delegateType;
            this.MethodName = methodName;
            this.TargetObject = targetObject;
            this.SourceContext = sctx;
        }
    }
    public class ConstructIterator : Expression
    {
        public Class State;
        public Block Body;
        public TypeNode ElementType;
        public ConstructIterator()
            : base(NodeType.ConstructIterator)
        {
        }
        public ConstructIterator(Class state, Block body, TypeNode elementType, TypeNode type)
            : base(NodeType.ConstructIterator)
        {
            this.State = state;
            this.Body = body;
            this.ElementType = elementType;
            this.Type = type;
        }
    }
    public class ConstructTuple : Expression
    {
        public FieldList Fields;
        public ConstructTuple()
            : base(NodeType.ConstructTuple)
        {
        }
    }
    public class CoerceTuple : ConstructTuple
    {
        public Expression OriginalTuple;
        public Local Temp;
        public CoerceTuple()
        {
            this.NodeType = NodeType.CoerceTuple;
        }
    }

    public class Indexer : NaryExpression
    {

        public Property CorrespondingDefaultIndexedProperty;
        public bool ArgumentListIsIncomplete;

        public Indexer()
        {
            this.NodeType = NodeType.Indexer;
        }
        public Indexer(Expression @object, ExpressionList arguments)
            : base(arguments, NodeType.Indexer)
        {
            this.@object = @object;
        }

        public Indexer(Expression Object, ExpressionList arguments, SourceContext sctx)
            : base(arguments, NodeType.Indexer)
        {
            this.@object = Object;
            this.SourceContext = sctx;
        }
        public Indexer(Expression Object, ExpressionList arguments, TypeNode elementType)
            : base(arguments, NodeType.Indexer)
        {
            this.@object = Object;
            this.elementType = this.Type = elementType;
        }
        public Indexer(Expression Object, ExpressionList arguments, TypeNode elementType, SourceContext sctx)
            : base(arguments, NodeType.Indexer)
        {
            this.@object = Object;
            this.elementType = this.Type = elementType;
            this.SourceContext = sctx;
        }

        private Expression @object;
        public Expression Object
        {
            get { return this.@object; }
            set { this.@object = value; }
        }
        private TypeNode elementType;
        /// <summary>
        /// This type is normally expected to be the same the value of Type. However, if the indexer applies to an array of enums, then
        /// Type will be the enum type and ElementType will be the underlying type of the enum.
        /// </summary>
        public TypeNode ElementType
        {
            get { return this.elementType; }
            set { this.elementType = value; }
        }
    }

    public class CollectionEnumerator : Expression
    {
        public Expression Collection;
        public Method DefaultIndexerGetter;
        public Method LengthPropertyGetter;
        public Method GetEnumerator;
        public Method MoveNext;
        public Method GetCurrent;
        public Local ElementLocal;
        public Expression ElementCoercion;
        public CollectionEnumerator()
            : base(NodeType.CollectionEnumerator)
        {
        }
    }

    /// <summary>
    /// An expression that is used on the left hand as well as the right hand side of an assignment statement. For example, e in (e += 1).
    /// </summary>
    public class LRExpression : Expression
    {
        public Expression Expression;

        public LRExpression(Expression/*!*/ expression) : base(NodeType.LRExpression)
        {
            this.Expression = expression;
            this.Type = expression.Type;
        }
    }

    public class AssignmentExpression : Expression
    {
        public Statement AssignmentStatement;
        public AssignmentExpression()
            : base(NodeType.AssignmentExpression)
        {
        }
        public AssignmentExpression(AssignmentStatement assignment)
            : base(NodeType.AssignmentExpression)
        {
            this.AssignmentStatement = assignment;
        }
    }


    public class BlockExpression : Expression
    {
        public Block Block;
        public BlockExpression()
            : base(NodeType.BlockExpression)
        {
        }
        public BlockExpression(Block block)
            : base(NodeType.BlockExpression)
        {
            this.Block = block;
        }
        public BlockExpression(Block block, TypeNode type)
            : base(NodeType.BlockExpression)
        {
            this.Block = block;
            this.Type = type;
        }
        public BlockExpression(Block block, TypeNode type, SourceContext sctx)
            : base(NodeType.BlockExpression)
        {
            this.Block = block;
            this.Type = type;
            this.SourceContext = sctx;
        }
    }


    public class AnonymousNestedFunction : Expression
    {
        public ParameterList Parameters;
        public Block Body;
        public Method Method;
        public Expression Invocation;

        public AnonymousNestedFunction()
            : base(NodeType.AnonymousNestedFunction)
        {
        }
        public AnonymousNestedFunction(ParameterList parameters, Block body)
            : base(NodeType.AnonymousNestedFunction)
        {
            this.Parameters = parameters;
            this.Body = body;
        }
        public AnonymousNestedFunction(ParameterList parameters, Block body, SourceContext sctx)
            : base(NodeType.AnonymousNestedFunction)
        {
            this.Parameters = parameters;
            this.Body = body;
            this.SourceContext = sctx;
        }
    }

    public class Instruction : Node
    {
        private OpCode opCode;
        private int offset;
        private object value;
        public Instruction()
            : base(NodeType.Instruction)
        {
        }
        public Instruction(OpCode opCode, int offset)
            : this(opCode, offset, null)
        {
        }
        public Instruction(OpCode opCode, int offset, object value)
            : base(NodeType.Instruction)
        {
            this.opCode = opCode;
            this.offset = offset;
            this.value = value;
        }
        /// <summary>The actual value of the opcode</summary>
        public OpCode OpCode
        {
            get { return this.opCode; }
            set { this.opCode = value; }
        }
        /// <summary>The offset from the start of the instruction stream of a method</summary>
        public int Offset
        {
            get { return this.offset; }
            set { this.offset = value; }
        }
        /// <summary>Immediate data such as a string, the address of a branch target, or a metadata reference, such as a Field</summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
    }
    public class Statement : Node
    {
        public Statement(NodeType nodeType)
            : base(nodeType)
        {
        }

        public Statement(NodeType nodeType, SourceContext sctx)
            : base(nodeType)
        {
            this.SourceContext = sctx;
        }

    }
    public class Block : Statement
    {
        private StatementList statements;

        public bool Checked;
        public bool SuppressCheck;


        public bool HasLocals;


        public bool IsUnsafe;
        public BlockScope Scope;

        public Block()
            : base(NodeType.Block)
        {
        }
        public Block(StatementList statements)
            : base(NodeType.Block)
        {
            this.statements = statements;
        }

        public Block(StatementList statements, SourceContext sourceContext)
            : base(NodeType.Block)
        {
            this.SourceContext = sourceContext;
            this.statements = statements;
        }
        public Block(StatementList statements, bool Checked, bool SuppressCheck, bool IsUnsafe)
            : base(NodeType.Block)
        {
            this.Checked = Checked;
            this.IsUnsafe = IsUnsafe;
            this.SuppressCheck = SuppressCheck;
            this.statements = statements;
        }
        public Block(StatementList statements, SourceContext sourceContext, bool Checked, bool SuppressCheck, bool IsUnsafe)
            : base(NodeType.Block)
        {
            this.Checked = Checked;
            this.IsUnsafe = IsUnsafe;
            this.SuppressCheck = SuppressCheck;
            this.SourceContext = sourceContext;
            this.statements = statements;
        }
        public override string ToString()
        {
            return "B#" + this.UniqueKey.ToString();
        }

        public StatementList Statements
        {
            get { return this.statements; }
            set { this.statements = value; }
        }
    }

    public class LabeledStatement : Block
    {
        public Identifier Label;
        public Statement Statement;
        public LabeledStatement()
        {
            this.NodeType = NodeType.LabeledStatement;
        }
    }
    public class FunctionDeclaration : Statement
    {
        public Identifier Name;
        public ParameterList Parameters;
        public TypeNode ReturnType;
        public TypeNode ReturnTypeExpression;
        public Block Body;
        public Method Method;

        public FunctionDeclaration()
            : base(NodeType.FunctionDeclaration)
        {
        }
        public FunctionDeclaration(Identifier name, ParameterList parameters, TypeNode returnType, Block body)
            : base(NodeType.FunctionDeclaration)
        {
            this.Name = name;
            this.Parameters = parameters;
            this.ReturnType = returnType;
            this.Body = body;
        }
    }
    public class Assertion : Statement
    {
        public Expression Condition;
        public Assertion()
            : base(NodeType.Assertion)
        {
        }
        public Assertion(Expression condition)
            : base(NodeType.Assertion)
        {
            this.Condition = condition;
        }
    }
    public class Assumption : Statement
    {
        public Expression Condition;
        public Assumption()
            : base(NodeType.Assumption)
        {
        }
        public Assumption(Expression condition)
            : base(NodeType.Assumption)
        {
            this.Condition = condition;
        }
    }

    public class AssignmentStatement : Statement
    {
        private NodeType @operator;
        private Expression source;
        private Expression target;

        public Method OperatorOverload;
        ///<summary>A Type two which both operands must be coerced before carrying out the operation (if any).</summary>
        public TypeNode UnifiedType;

        public AssignmentStatement()
            : base(NodeType.AssignmentStatement)
        {
            this.Operator = NodeType.Nop;
        }
        public AssignmentStatement(Expression target, Expression source)
            : this(target, source, NodeType.Nop)
        {
        }

        public AssignmentStatement(Expression target, Expression source, SourceContext context)
            : this(target, source, NodeType.Nop)
        {
            this.SourceContext = context;
        }

        public AssignmentStatement(Expression target, Expression source, NodeType @operator)
            : base(NodeType.AssignmentStatement)
        {
            this.target = target;
            this.source = source;
            this.@operator = @operator;
        }

        public AssignmentStatement(Expression target, Expression source, NodeType Operator, SourceContext context)
            : this(target, source, Operator)
        {
            this.SourceContext = context;
        }

        public NodeType Operator
        {
            get { return this.@operator; }
            set { this.@operator = value; }
        }
        public Expression Source
        {
            get { return this.source; }
            set { this.source = value; }
        }
        public Expression Target
        {
            get { return this.target; }
            set { this.target = value; }
        }
    }
    public class ExpressionStatement : Statement
    {
        private Expression expression;
        public ExpressionStatement()
            : base(NodeType.ExpressionStatement)
        {
        }
        public ExpressionStatement(Expression expression)
            : base(NodeType.ExpressionStatement)
        {
            this.Expression = expression;
        }

        public ExpressionStatement(Expression expression, SourceContext sctx)
            : base(NodeType.ExpressionStatement)
        {
            this.Expression = expression;
            this.SourceContext = sctx;
        }

        public Expression Expression
        {
            get { return this.expression; }
            set { this.expression = value; }
        }
    }
    public class Branch : Statement
    {
        private Expression condition;
        private bool leavesExceptionBlock;
        internal bool shortOffset;
        private Block target;
        public bool BranchIfUnordered;
        public Branch()
            : base(NodeType.Branch)
        {
        }

        public Branch(Expression condition, Block target)
            : this(condition, target, false, false, false)
        {
        }
        public Branch(Expression condition, Block target, SourceContext sourceContext)
            : this(condition, target, false, false, false)
        {
            this.SourceContext = sourceContext;
        }
        public Branch(Expression condition, Block target, SourceContext sourceContext, bool unordered)
            : this(condition, target, false, false, false)
        {
            this.BranchIfUnordered = unordered;
            this.SourceContext = sourceContext;
        }

        public Branch(Expression condition, Block target, bool shortOffset, bool unordered, bool leavesExceptionBlock)
            : base(NodeType.Branch)
        {
            this.BranchIfUnordered = unordered;
            this.condition = condition;
            this.leavesExceptionBlock = leavesExceptionBlock;
            this.shortOffset = shortOffset;
            this.target = target;
        }
        public Expression Condition
        {
            get { return this.condition; }
            set { this.condition = value; }
        }
        public bool LeavesExceptionBlock
        {
            get { return this.leavesExceptionBlock; }
            set { this.leavesExceptionBlock = value; }
        }
        public bool ShortOffset
        {
            get { return this.shortOffset; }
            set { this.shortOffset = value; }
        }
        public Block Target
        {
            get { return this.target; }
            set { this.target = value; }
        }
    }
    public class Return : ExpressionStatement
    {
        public Return() : base()
        {
            this.NodeType = NodeType.Return;
        }

        public Return(Expression expression) : base(expression)
        {
            this.NodeType = NodeType.Return;
        }

        public Return(SourceContext sctx)
            : base()
        {
            this.NodeType = NodeType.Return;
            this.SourceContext = sctx;
        }
        public Return(Expression expression, SourceContext sctx)
            : base(expression)
        {
            this.NodeType = NodeType.Return;
            this.SourceContext = sctx;
        }

    }

    public class Yield : ExpressionStatement
    {
        public Yield()
            : base()
        {
            this.NodeType = NodeType.Yield;
        }
        public Yield(Expression expression)
            : base(expression)
        {
            this.NodeType = NodeType.Yield;
        }
        public Yield(Expression expression, SourceContext sctx)
            : base(expression)
        {
            this.NodeType = NodeType.Yield;
            this.SourceContext = sctx;
        }
    }
    public class Try : Statement
    {
        private CatchList catchers;
        private FilterList filters;
        private FaultHandlerList faultHandlers;
        private Finally finallyClause;
        private Block tryBlock;
        public Try()
            : base(NodeType.Try)
        {
        }
        public Try(Block tryBlock, CatchList catchers, FilterList filters, FaultHandlerList faultHandlers, Finally Finally)
            : base(NodeType.Try)
        {
            this.catchers = catchers;
            this.faultHandlers = faultHandlers;
            this.filters = filters;
            this.finallyClause = Finally;
            this.tryBlock = tryBlock;
        }
        public CatchList Catchers
        {
            get { return this.catchers; }
            set { this.catchers = value; }
        }
        public FilterList Filters
        {
            get { return this.filters; }
            set { this.filters = value; }
        }
        public FaultHandlerList FaultHandlers
        {
            get { return this.faultHandlers; }
            set { this.faultHandlers = value; }
        }
        public Finally Finally
        {
            get { return this.finallyClause; }
            set { this.finallyClause = value; }
        }
        public Block TryBlock
        {
            get { return this.tryBlock; }
            set { this.tryBlock = value; }
        }
    }
    public class Catch : Statement
    {
        private Block block;
        private TypeNode type;
        private Expression variable;
        public TypeNode TypeExpression;
        public Catch()
            : base(NodeType.Catch)
        {
        }
        public Catch(Block block, Expression variable, TypeNode type)
            : base(NodeType.Catch)
        {
            this.block = block;
            this.variable = variable;
            this.type = type;
        }
        public Block Block
        {
            get { return this.block; }
            set { this.block = value; }
        }
        public TypeNode Type
        {
            get { return this.type; }
            set { this.type = value; }
        }
        public Expression Variable
        {
            get { return this.variable; }
            set { this.variable = value; }
        }
    }
    public class Finally : Statement
    {
        private Block block;
        public Finally()
            : base(NodeType.Finally)
        {
        }
        public Finally(Block block)
            : base(NodeType.Finally)
        {
            this.block = block;
        }
        public Block Block
        {
            get { return this.block; }
            set { this.block = value; }
        }
    }

    public class EndFinally : Statement
    {
        public EndFinally()
            : base(NodeType.EndFinally)
        {
        }
    }

    public class Filter : Statement
    {
        private Block block;
        private Expression expression;

        public Filter()
            : base(NodeType.Filter)
        {
        }
        public Filter(Block block, Expression expression)
            : base(NodeType.Filter)
        {
            this.block = block;
            this.expression = expression;
        }
        public Block Block
        {
            get { return this.block; }
            set { this.block = value; }
        }
        public Expression Expression
        {
            get { return this.expression; }
            set { this.expression = value; }
        }
    }

    public class EndFilter : Statement
    {
        private Expression value;
        public EndFilter()
            : base(NodeType.EndFilter)
        {
        }
        public EndFilter(Expression value)
            : base(NodeType.EndFilter)
        {
            this.value = value;
        }
        public Expression Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
    }

    public class FaultHandler : Statement
    {
        private Block block;

        public FaultHandler()
            : base(NodeType.FaultHandler)
        {
        }
        public FaultHandler(Block block)
            : base(NodeType.FaultHandler)
        {
            this.block = block;
        }
        public Block Block
        {
            get { return this.block; }
            set { this.block = value; }
        }
    }

    public class Throw : Statement
    {
        private Expression expression;

        public Throw()
            : base(NodeType.Throw)
        {
        }
        public Throw(Expression expression)
            : base(NodeType.Throw)
        {
            this.expression = expression;
        }

        public Throw(Expression expression, SourceContext context)
            : base(NodeType.Throw)
        {
            this.expression = expression;
            this.SourceContext = context;
        }

        public Expression Expression
        {
            get { return this.expression; }
            set { this.expression = value; }
        }
    }

    public class If : Statement
    {
        public Expression Condition;
        public Block TrueBlock;
        public Block FalseBlock;
        public SourceContext ConditionContext;
        public SourceContext ElseContext;
        public SourceContext EndIfContext;
        public If()
            : base(NodeType.If)
        {
        }
        public If(Expression condition, Block trueBlock, Block falseBlock)
            : base(NodeType.If)
        {
            this.Condition = condition;
            if(condition != null)
                this.ConditionContext = condition.SourceContext;
            this.TrueBlock = trueBlock;
            this.FalseBlock = falseBlock;
        }
    }
    public class For : Statement
    {
        public Block Body;
        public Expression Condition;
        public StatementList Incrementer;
        public StatementList Initializer;
        public ExpressionList Invariants;
        public For()
            : base(NodeType.For)
        {
        }
        public For(StatementList initializer, Expression condition, StatementList incrementer, Block body)
            : base(NodeType.For)
        {
            this.Body = body;
            this.Condition = condition;
            this.Incrementer = incrementer;
            this.Initializer = initializer;
        }
    }
    public class ForEach : Statement
    {
        public Block Body;
        public Expression SourceEnumerable;
        public BlockScope ScopeForTemporaryVariables;
        public Expression TargetVariable;
        public TypeNode TargetVariableType;
        public TypeNode TargetVariableTypeExpression;
        public Expression InductionVariable;
        public ExpressionList Invariants;
        public bool StatementTerminatesNormallyIfEnumerableIsNull = true;
        public bool StatementTerminatesNormallyIfEnumeratorIsNull = true;
        public ForEach()
            : base(NodeType.ForEach)
        {
        }
        public ForEach(TypeNode targetVariableType, Expression targetVariable, Expression sourceEnumerable, Block body)
            : base(NodeType.ForEach)
        {
            this.TargetVariable = targetVariable;
            this.TargetVariableType = targetVariableType;
            this.SourceEnumerable = sourceEnumerable;
            this.Body = body;
        }
    }
    public class Exit : Statement
    {
        public Literal Level;
        public Exit()
            : base(NodeType.Exit)
        {
        }
        public Exit(Literal level)
            : base(NodeType.Exit)
        {
            this.Level = level;
        }
    }
    public class Continue : Statement
    {
        public Literal Level;
        public Continue()
            : base(NodeType.Continue)
        {
        }
        public Continue(Literal level)
            : base(NodeType.Continue)
        {
            this.Level = level;
        }
    }
    public class Switch : Statement
    {
        public SwitchCaseList Cases;
        public Expression Expression;
        public Local Nullable;
        public Expression NullableExpression;
        public BlockScope Scope;
        public Switch()
            : base(NodeType.Switch)
        {
        }
        public Switch(Expression expression, SwitchCaseList cases)
            : base(NodeType.Switch)
        {
            this.Cases = cases;
            this.Expression = expression;
        }
    }
    public class SwitchCase : Node
    {
        public Block Body;
        public Expression Label;
        public SwitchCase()
            : base(NodeType.SwitchCase)
        {
        }
        public SwitchCase(Expression label, Block body)
            : base(NodeType.SwitchCase)
        {
            this.Body = body;
            this.Label = label;
        }
    }
    public class GotoCase : Statement
    {
        public Expression CaseLabel;
        public GotoCase(Expression caseLabel)
            : base(NodeType.GotoCase)
        {
            this.CaseLabel = caseLabel;
        }
    }

    public class SwitchInstruction : Statement
    {
        private Expression expression;
        private BlockList targets;
        public SwitchInstruction()
            : base(NodeType.SwitchInstruction)
        {
        }
        public SwitchInstruction(Expression expression, BlockList targets)
            : base(NodeType.SwitchInstruction)
        {
            this.expression = expression;
            this.targets = targets;
        }
        public Expression Expression
        {
            get { return this.expression; }
            set { this.expression = value; }
        }
        public BlockList Targets
        {
            get { return this.targets; }
            set { this.targets = value; }
        }
    }

    public class Typeswitch : Statement
    {
        public TypeswitchCaseList Cases;
        public Expression Expression;
        public Typeswitch()
            : base(NodeType.Typeswitch)
        {
        }
        public Typeswitch(Expression expression, TypeswitchCaseList cases)
            : base(NodeType.Typeswitch)
        {
            this.Cases = cases;
            this.Expression = expression;
        }
    }
    public class TypeswitchCase : Node
    {
        public Block Body;
        public TypeNode LabelType;
        public TypeNode LabelTypeExpression;
        public Expression LabelVariable;
        public TypeswitchCase()
            : base(NodeType.TypeswitchCase)
        {
        }
        public TypeswitchCase(TypeNode labelType, Expression labelVariable, Block body)
            : base(NodeType.TypeswitchCase)
        {
            this.Body = body;
            this.LabelType = labelType;
            this.LabelVariable = labelVariable;
        }
    }
    public class While : Statement
    {
        public Expression Condition;
        public ExpressionList Invariants;
        public Block Body;
        public While()
            : base(NodeType.While)
        {
        }
        public While(Expression condition, Block body)
            : base(NodeType.While)
        {
            this.Condition = condition;
            this.Body = body;
        }
    }
    public class DoWhile : Statement
    {
        public Expression Condition;
        public ExpressionList Invariants;
        public Block Body;
        public DoWhile()
            : base(NodeType.DoWhile)
        {
        }
        public DoWhile(Expression condition, Block body)
            : base(NodeType.DoWhile)
        {
            this.Condition = condition;
            this.Body = body;
        }
    }
    public class Repeat : Statement
    {
        public Expression Condition;
        public Block Body;
        public Repeat()
            : base(NodeType.Repeat)
        {
        }
        public Repeat(Expression condition, Block body)
            : base(NodeType.Repeat)
        {
            this.Condition = condition;
            this.Body = body;
        }
    }
    public class Fixed : Statement
    {
        public Statement Declarators;
        public Block Body;
        public BlockScope ScopeForTemporaryVariables;
        public Fixed()
            : base(NodeType.Fixed)
        {
        }
    }
    public class Lock : Statement
    {
        public Expression Guard;
        public Block Body;
        public BlockScope ScopeForTemporaryVariable;
        public Lock()
            : base(NodeType.Lock)
        {
        }
    }
    public class ResourceUse : Statement
    {
        public Statement ResourceAcquisition;
        public Block Body;
        public BlockScope ScopeForTemporaryVariable;
        public ResourceUse()
            : base(NodeType.ResourceUse)
        {
        }
    }
    public class Goto : Statement
    {
        public Identifier TargetLabel;
        public Goto()
            : base(NodeType.Goto)
        {
        }
        public Goto(Identifier targetLabel)
            : base(NodeType.Goto)
        {
            this.TargetLabel = targetLabel;
        }
    }
    public class VariableDeclaration : Statement
    {
        public Expression Initializer;
        public Identifier Name;
        public TypeNode Type;
        public TypeNode TypeExpression;
        public Field Field;
        public VariableDeclaration()
            : base(NodeType.VariableDeclaration)
        {
        }
        public VariableDeclaration(Identifier name, TypeNode type, Expression initializer)
            : base(NodeType.VariableDeclaration)
        {
            this.Initializer = initializer;
            this.Name = name;
            this.Type = type;
        }
    }
    public class LocalDeclaration : Node
    {
        public Field Field;
        public Identifier Name;
        public Expression InitialValue;
        /// <summary>
        /// Used when converting a declaration with initializer into an assignment statement.
        /// Usually Nop, but could be set to CopyReference to avoid dereferencing on either side.
        /// </summary>
        public NodeType AssignmentNodeType = NodeType.Nop;
        public LocalDeclaration()
            : base(NodeType.LocalDeclaration)
        {
        }
        public LocalDeclaration(Identifier name, Expression initialValue)
            : base(NodeType.LocalDeclaration)
        {
            this.Name = name;
            this.InitialValue = initialValue;
        }
        public LocalDeclaration(Identifier name, Expression initialValue, NodeType assignmentNodeType)
            : base(NodeType.LocalDeclaration)
        {
            this.Name = name;
            this.InitialValue = initialValue;
            this.AssignmentNodeType = assignmentNodeType;
        }

    }
    public class LocalDeclarationsStatement : Statement
    {
        public bool Constant;
        public bool InitOnly;
        public TypeNode Type;
        public TypeNode TypeExpression;
        public LocalDeclarationList Declarations;

        public LocalDeclarationsStatement()
            : base(NodeType.LocalDeclarationsStatement)
        {
        }

        public LocalDeclarationsStatement(LocalDeclaration ldecl, TypeNode type)
            : base(NodeType.LocalDeclarationsStatement)
        {
            Declarations = new LocalDeclarationList();
            Declarations.Add(ldecl);
            this.Type = type;
        }
    }

    public class StatementSnippet : Statement
    {
        public IParserFactory ParserFactory;

        public StatementSnippet()
            : base(NodeType.StatementSnippet)
        {
        }
        public StatementSnippet(IParserFactory parserFactory, SourceContext sctx)
            : base(NodeType.StatementSnippet)
        {
            this.ParserFactory = parserFactory;
            this.SourceContext = sctx;
        }
    }
    /// <summary>
    /// Associates an identifier with a type or a namespace or a Uri or a list of assemblies. 
    /// In C# alias identifiers are used as root identifiers in qualified expressions, or as identifier prefixes.
    /// </summary>
    public class AliasDefinition : Node
    {

        /// <summary>The identifier that serves as an alias for the type, namespace, Uri or list of assemblies.</summary>
        public Identifier Alias;

        /// <summary>The list of assemblies being aliased.</summary>
        public AssemblyReferenceList AliasedAssemblies;

        /// <summary>The expression that was (or should be) resolved into a type, namespace or Uri.</summary>
        public Expression AliasedExpression;

        /// <summary>The namespace being aliased.</summary>
        public Identifier AliasedNamespace;

        /// <summary>A reference to the type being aliased.</summary>
        public TypeReference AliasedType;

        /// <summary>The Uri being aliased.</summary>
        public Identifier AliasedUri;

        /// <summary>
        /// If an alias definition conflicts with a type definition and this causes an ambiguity, the conflicting type is stored here
        /// by the code that detects the ambiguity. A later visitor is expected to report an error if this is not null.
        /// </summary>
        public TypeNode ConflictingType;

        public bool RestrictToInterfaces;
        public bool RestrictToClassesAndInterfaces;

        public AliasDefinition()
            : base(NodeType.AliasDefinition)
        {
        }
        public AliasDefinition(Identifier alias, Expression aliasedExpression)
            : base(NodeType.AliasDefinition)
        {
            this.Alias = alias;
            this.AliasedExpression = aliasedExpression;
        }
        public AliasDefinition(Identifier alias, Expression aliasedExpression, SourceContext sctx)
            : base(NodeType.AliasDefinition)
        {
            this.Alias = alias;
            this.AliasedExpression = aliasedExpression;
            this.SourceContext = sctx;
        }
    }
    public class UsedNamespace : Node
    {
        public Identifier Namespace;
        public Identifier URI;
        public UsedNamespace()
            : base(NodeType.UsedNamespace)
        {
        }
        public UsedNamespace(Identifier Namespace)
            : base(NodeType.UsedNamespace)
        {
            this.Namespace = Namespace;
        }
        public UsedNamespace(Identifier Namespace, SourceContext sctx)
            : base(NodeType.UsedNamespace)
        {
            this.Namespace = Namespace;
            this.SourceContext = sctx;
        }
    }

    public class ExceptionHandler : Node
    {
        private NodeType handlerType;
        private Block tryStartBlock;
        private Block blockAfterTryEnd;
        private Block handlerStartBlock;
        private Block blockAfterHandlerEnd;
        private Block filterExpression;
        private TypeNode filterType;
        public ExceptionHandler()
            : base(NodeType.ExceptionHandler)
        {
        }
        public NodeType HandlerType
        {
            get { return this.handlerType; }
            set { this.handlerType = value; }
        }
        public Block TryStartBlock
        {
            get { return this.tryStartBlock; }
            set { this.tryStartBlock = value; }
        }
        public Block BlockAfterTryEnd
        {
            get { return this.blockAfterTryEnd; }
            set { this.blockAfterTryEnd = value; }
        }
        public Block HandlerStartBlock
        {
            get { return this.handlerStartBlock; }
            set { this.handlerStartBlock = value; }
        }
        public Block BlockAfterHandlerEnd
        {
            get { return this.blockAfterHandlerEnd; }
            set { this.blockAfterHandlerEnd = value; }
        }
        public Block FilterExpression
        {
            get { return this.filterExpression; }
            set { this.filterExpression = value; }
        }
        public TypeNode FilterType
        {
            get { return this.filterType; }
            set { this.filterType = value; }
        }
    }

    public class AttributeNode : Node
    {
        public bool IsPseudoAttribute;

        public AttributeNode()
            : base(NodeType.Attribute)
        {
        }
        public AttributeNode(Expression constructor, ExpressionList expressions)
            : base(NodeType.Attribute)
        {
            this.constructor = constructor;
            this.expressions = expressions;
            this.target = AttributeTargets.All;
        }

        public AttributeNode(Expression constructor, ExpressionList expressions, AttributeTargets target)
            : base(NodeType.Attribute)
        {
            this.constructor = constructor;
            this.expressions = expressions;
            this.target = target;
        }

        private Expression constructor;
        public Expression Constructor
        {
            get { return this.constructor; }
            set { this.constructor = value; }
        }
        private ExpressionList expressions;
        /// <summary>
        /// Invariant: positional arguments occur first and in order in the expression list. Named arguments
        /// follow positional arguments in any order.
        /// </summary>
        public ExpressionList Expressions
        {
            get { return this.expressions; }
            set { this.expressions = value; }
        }
        private AttributeTargets target;
        public AttributeTargets Target
        {
            get { return this.target; }
            set { this.target = value; }
        }
        private bool allowMultiple;
        public virtual bool AllowMultiple
        {
            get
            {
                if(this.usageAttribute == null)
                    this.GetUsageInformation();
                return this.allowMultiple;
            }
            set
            {
                this.allowMultiple = value;
            }
        }
        private bool inherited;
        public virtual bool Inherited
        {
            get
            {
                if(this.usageAttribute == null)
                    this.GetUsageInformation();
                return this.inherited;
            }
            set
            {
                this.inherited = value;
            }
        }
        private AttributeTargets validOn;
        public virtual AttributeTargets ValidOn
        {
            get
            {
                if(this.usageAttribute == null)
                    this.GetUsageInformation();
                return this.validOn;
            }
            set
            {
                this.validOn = value;
            }
        }
        private TypeNode type;
        public virtual TypeNode Type
        {
            get
            {
                if(this.type == null)
                {
                    MemberBinding mb = this.Constructor as MemberBinding;
                    Member cons = mb == null ? null : mb.BoundMember;
                    this.type = cons == null ? null : cons.DeclaringType;
                }
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
        private AttributeNode usageAttribute;
        private void GetUsageInformation()
        {
            AttributeNode attr = null;
            TypeNode attrType = this.Type;
            while(attrType != null)
            {
                attr = attrType.GetAttribute(SystemTypes.AttributeUsageAttribute);
                if(attr != null)
                    break;
                attrType = attrType.BaseType;
            }
            if(attr == null)
            {
                this.usageAttribute = AttributeNode.DoesNotExist;
                return;
            }
            ExpressionList args = attr.Expressions;
            if(args == null || args.Count < 1)
                return;
            Literal lit = args[0] as Literal;

            if(lit == null || !(lit.Value is int))
                return;

            //^ assert lit.Value != null;
            this.validOn = (AttributeTargets)(int)lit.Value;

            for(int i = 1, n = args.Count; i < n; i++)
            {
                NamedArgument narg = args[i] as NamedArgument;
                if(narg == null || narg.Name == null)
                    continue;
                lit = narg.Value as Literal;
                if(lit == null)
                    continue;
                if(narg.Name.UniqueIdKey == StandardIds.AllowMultiple.UniqueIdKey)
                {
                    if(lit.Value == null || !(lit.Value is bool))
                        continue;
                    this.allowMultiple = (bool)lit.Value;
                }
                else if(narg.Name.UniqueIdKey == StandardIds.Inherited.UniqueIdKey)
                {
                    if(lit.Value == null || !(lit.Value is bool))
                        continue;
                    this.inherited = (bool)lit.Value;
                }
            }
        }

        public static readonly AttributeNode DoesNotExist = new AttributeNode();

        public virtual System.Attribute GetRuntimeAttribute()
        {
            MemberBinding mb = this.Constructor as MemberBinding;
            if(mb == null)
                return null;
            InstanceInitializer constr = mb.BoundMember as InstanceInitializer;
            if(constr == null)
                return null;
            ParameterList parameters = constr.Parameters;
            int paramCount = parameters == null ? 0 : parameters.Count;
            object[] argumentValues = new object[paramCount];
            ExpressionList argumentExpressions = this.Expressions;
            int exprCount = argumentExpressions == null ? 0 : argumentExpressions.Count;
            for(int i = 0, j = 0; i < paramCount; i++)
            {
                if(j >= exprCount)
                    return null;
                //^ assert argumentExpressions != null;
                Expression argExpr = argumentExpressions[j++];
                Literal lit = argExpr as Literal;
                if(lit == null)
                    continue;
                argumentValues[i] = this.GetCoercedLiteralValue(lit.Type, lit.Value);
            }
            System.Attribute attr = this.ConstructAttribute(constr, argumentValues);
            if(attr == null)
                return null;
            for(int i = 0; i < exprCount; i++)
            {
                //^ assert argumentExpressions != null;
                NamedArgument namedArg = argumentExpressions[i] as NamedArgument;
                if(namedArg == null)
                    continue;
                if(namedArg.Name == null)
                    continue;
                Literal lit = namedArg.Value as Literal;
                if(lit == null)
                    continue;
                object val = this.GetCoercedLiteralValue(lit.Type, lit.Value);
                if(namedArg.IsCustomAttributeProperty)
                {
                    TypeNode t = constr.DeclaringType;
                    while(t != null)
                    {
                        Property prop = t.GetProperty(namedArg.Name);
                        if(prop != null)
                        {
                            this.SetAttributeProperty(prop, attr, val);
                            t = null;
                        }
                        else
                            t = t.BaseType;
                    }
                }
                else
                {
                    TypeNode t = constr.DeclaringType;
                    while(t != null)
                    {
                        Field f = constr.DeclaringType.GetField(namedArg.Name);
                        if(f != null)
                        {
                            System.Reflection.FieldInfo fieldInfo = f.GetFieldInfo();
                            if(fieldInfo != null)
                                fieldInfo.SetValue(attr, val);
                            t = null;
                        }
                        else
                            t = t.BaseType;
                    }
                }
            }
            return attr;
        }
        /// <summary>
        /// Gets the value of the literal coercing literals of TypeNode, EnumNode, TypeNode[], and EnumNode[] as needed.
        /// </summary>
        /// <param name="type">A TypeNode representing the type of the literal</param>
        /// <param name="value">The value of the literal</param>
        /// <returns>An object that has been coerced to the appropriate runtime type</returns>
        protected object GetCoercedLiteralValue(TypeNode type, object value)
        {
            if(type == null || value == null)
                return null;
            switch(type.typeCode)
            {
                case ElementType.Class:
                    return ((TypeNode)value).GetRuntimeType();
                case ElementType.ValueType:
                    return System.Enum.ToObject(type.GetRuntimeType(), value);
                case ElementType.SzArray:
                    return this.GetCoercedArrayLiteral((ArrayType)type, (Array)value);
                default:
                    Literal lit = value as Literal;
                    if(lit != null && type == CoreSystemTypes.Object && lit.Type is EnumNode)
                        return this.GetCoercedLiteralValue(lit.Type, lit.Value);
                    break;
            }
            return value;
        }
        /// <summary>
        /// Gets the array literal in arrayValue coercing TypeNode[] and EnumNode[] as needed.
        /// </summary>
        /// <param name="arrayType">A TypeNode representing the array type</param>
        /// <param name="arrayValue">The value of the array literal to coerce</param>
        /// <returns>An Array object that has been coerced to the appropriate runtime type</returns>
        protected Array GetCoercedArrayLiteral(ArrayType arrayType, Array arrayValue)
        {
            if(arrayType == null)
                return null;
            if(arrayValue == null)
                return null;
            // Multi-dimensional arrays are not legal in attribute instances according section 17.1.3 of the C# 1.0 spec
            if(arrayValue.Rank != 1)
                return null;
            TypeNode elemType = arrayType.ElementType;
            if(elemType.typeCode != ElementType.ValueType && elemType.typeCode != ElementType.Class)
                return arrayValue;
            int arraySize = arrayValue.GetLength(0);
            Type et = elemType.GetRuntimeType();
            if(et == null)
                return null;
            Array val = Array.CreateInstance(et, arraySize);
            for(int i = 0; i < arraySize; i++)
                val.SetValue(this.GetCoercedLiteralValue(elemType, arrayValue.GetValue(i)), i);
            return val;
        }
        private void SetAttributeProperty(Property/*!*/ prop, System.Attribute attr, object val)
        {
            //This could execute partially trusted code, so set up a very restrictive execution environment
            //TODO: skip this if the attribute is from a trusted assembly
            System.Reflection.PropertyInfo propInfo = prop.GetPropertyInfo();
            if(propInfo == null)
                return;
            //Because we invoke the setter through reflection, a stack walk is performed. The following two commented-out statements
            //would cause the stack walk to fail.
            //For now, we will run the setter in full trust until we work around this.
            //For VS2005 and later, we will construct a DynamicMethod, wrap it in a delegate, and invoke that.

            //System.Security.PermissionSet perm = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
            //perm.PermitOnly();
            try
            {
                propInfo.SetValue(attr, val, null);
            }
            catch { }
        }
        private System.Attribute ConstructAttribute(InstanceInitializer/*!*/ constr, object[] argumentValues)
        {
            //This could execute partially trusted code, so set up a very restrictive execution environment
            //TODO: skip this if the attribute is from a trusted assembly
            System.Reflection.ConstructorInfo consInfo = constr.GetConstructorInfo();
            if(consInfo == null)
                return null;
            //Because we invoke the constructor through reflection, a stack walk is performed. The following two commented-out statements
            //would cause the stack walk to fail.
            //For VS2003 and earlier, we will run the constructor in full trust.
            //For VS2005 and later, we will construct a DynamicMethod, wrap it in a delegate, and invoke that.

            //System.Security.PermissionSet perm = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
            //perm.PermitOnly();
            try
            {
                return consInfo.Invoke(argumentValues) as System.Attribute;
            }
            catch { }
            return null;
        }

        public Expression GetPositionalArgument(int position)
        {
            if(this.Expressions == null || this.Expressions.Count <= position)
                return null;
            Expression e = this.Expressions[position];
            NamedArgument na = e as NamedArgument;
            if(na != null)
                return null;
            return e;
        }
        public Expression GetNamedArgument(Identifier name)
        {
            if(name == null || this.Expressions == null)
                return null;
            foreach(Expression e in this.Expressions)
            {
                NamedArgument na = e as NamedArgument;
                if(na == null)
                    continue;
                if(na.Name == null)
                    continue;
                if(na.Name.UniqueIdKey == name.UniqueIdKey)
                    return na.Value;
            }
            return null;
        }
    }
    public class SecurityAttribute : Node
    {
        public SecurityAttribute()
            : base(NodeType.SecurityAttribute)
        {
        }
        private System.Security.Permissions.SecurityAction action;
        public System.Security.Permissions.SecurityAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }
        private AttributeList permissionAttributes;
        public AttributeList PermissionAttributes
        {
            get { return this.permissionAttributes; }
            set { this.permissionAttributes = value; }
        }
        protected string serializedPermissions;
        public string SerializedPermissions
        {
            get
            {

                if(this.serializedPermissions == null && this.PermissionAttributes != null)
                {
                    lock(this)
                    {
                        if(this.serializedPermissions != null)
                            return this.serializedPermissions;
                        System.Security.PermissionSet permissions = this.Permissions;
                        if(permissions == null)
                            return null;
                        System.Security.SecurityElement xml = permissions.ToXml();
                        if(xml == null)
                            return null;
                        this.serializedPermissions = xml.ToString();
                        //TODO: if the target platform is different from the host platform, replace references to host platform
                        //assemblies with references to target platform assemblies
                    }
                }

                return this.serializedPermissions;
            }
            set
            {
                this.serializedPermissions = value;
            }
        }

        protected System.Security.PermissionSet permissions;

        public System.Security.PermissionSet Permissions
        {
            get
            {
                if(this.permissions == null)
                {
                    lock(this)
                    {
                        if(this.permissions != null)
                            return this.permissions;
                        System.Security.PermissionSet permissions = null;

                        if(this.PermissionAttributes != null)
                        {
                            permissions = this.InstantiatePermissionAttributes();
                        }
                        else if(this.serializedPermissions != null)
                        {
                            permissions = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
                            permissions.FromXml(this.GetSecurityElement());
                        }

                        this.permissions = permissions;
                    }
                }
                return this.permissions;
            }
            set
            {
                this.permissions = value;
            }
        }

        protected System.Security.SecurityElement GetSecurityElement()
        {
            return System.Security.SecurityElement.FromString(this.serializedPermissions);
        }

        protected System.Security.PermissionSet InstantiatePermissionAttributes()
        {
            System.Security.PermissionSet permissions = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
            AttributeList permissionAttributes = this.PermissionAttributes;
            for(int i = 0, n = permissionAttributes == null ? 0 : permissionAttributes.Count; i < n; i++)
            {
                //^ assert permissionAttributes != null;
                object result = this.GetPermissionOrSetOfPermissionsFromAttribute(permissionAttributes[i]);
                if(result == null)
                    continue;
                if(result is System.Security.PermissionSet)
                    permissions = permissions.Union((System.Security.PermissionSet)result);
                else
                {
                    System.Security.IPermission permission = result as System.Security.IPermission;
                    if(permission == null)
                        continue;
                    permissions.AddPermission(permission);
                }
            }
            return permissions;
        }

        protected object GetPermissionOrSetOfPermissionsFromAttribute(AttributeNode attr)
        {
            if(attr == null)
                return null;
            System.Security.Permissions.SecurityAttribute secAttr = attr.GetRuntimeAttribute() as System.Security.Permissions.SecurityAttribute;
            if(secAttr == null)
                return null;
            System.Security.Permissions.PermissionSetAttribute pSetAttr = secAttr as System.Security.Permissions.PermissionSetAttribute;
            if(pSetAttr != null)
                return pSetAttr.CreatePermissionSet();
            else
                return this.CreatePermission(secAttr);
        }

        private System.Security.IPermission CreatePermission(System.Security.Permissions.SecurityAttribute/*!*/ secAttr)
        {
            //This could execute partially trusted code, so set up a very restrictive execution environment
            System.Security.PermissionSet perm = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
            //TODO: add permissions if the attribute is from a trusted assembly
            perm.PermitOnly();
            try
            {
                return secAttr.CreatePermission();
            }
            catch { }
            return null;
        }
    }

    public struct Resource
    {
        private bool isPublic;
        private string name;
        private Module definingModule;
        private byte[] data;
        public bool IsPublic
        {
            get { return this.isPublic; }
            set { this.isPublic = value; }
        }
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public Module DefiningModule
        {
            get { return this.definingModule; }
            set { this.definingModule = value; }
        }
        public byte[] Data
        {
            get { return this.data; }
            set { this.data = value; }
        }
    }
    public struct Win32Resource
    {
        private string typeName;
        private int typeId;
        private string name;
        private int id;
        private int languageId;
        private int codePage;
        private byte[] data;
        public string TypeName
        {
            get { return this.typeName; }
            set { this.typeName = value; }
        }
        public int TypeId
        {
            get { return this.typeId; }
            set { this.typeId = value; }
        }
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }
        public int LanguageId
        {
            get { return this.languageId; }
            set { this.languageId = value; }
        }
        public int CodePage
        {
            get { return this.codePage; }
            set { this.codePage = value; }
        }
        public byte[] Data
        {
            get { return this.data; }
            set { this.data = value; }
        }
    }

    public class Module : Node, IDisposable
    {
        internal Reader reader;
        public delegate void TypeNodeListProvider(Module/*!*/ module);
        protected TypeNodeListProvider provideTypeNodeList;
        public delegate TypeNode TypeNodeProvider(Identifier/*!*/ @namespace, Identifier/*!*/ name);
        protected TypeNodeProvider provideTypeNode;
        protected TrivialHashtable namespaceTable = new TrivialHashtable();
        protected NamespaceList namespaceList;
        protected int savedTypesLength;
        public delegate void CustomAttributeProvider(Module/*!*/ module);
        protected CustomAttributeProvider provideCustomAttributes;
        public delegate void ResourceProvider(Module/*!*/ module);
        protected ResourceProvider provideResources;
        public delegate AssemblyNode AssemblyReferenceResolver(AssemblyReference/*!*/ assemblyReference, Module/*!*/ referencingModule);
        public event AssemblyReferenceResolver AssemblyReferenceResolution;
        public event AssemblyReferenceResolver AssemblyReferenceResolutionAfterProbingFailed;

        public delegate void PostAssemblyLoadProcessor(AssemblyNode loadedAssembly);
        public event PostAssemblyLoadProcessor AfterAssemblyLoad;

        public delegate XmlDocument DocumentationResolver(Module referencingModule);
        public event DocumentationResolver DocumentationResolution = null;

        public bool IsNormalized;

        internal int FileAlignment = 512;
        internal readonly static object GlobalLock = new object();

        public Module() : base(NodeType.Module)
        {
            this.IsNormalized = false;
        }

        public Module(TypeNodeProvider provider, TypeNodeListProvider listProvider, CustomAttributeProvider provideCustomAttributes, ResourceProvider provideResources)
            : base(NodeType.Module)
        {
            this.provideCustomAttributes = provideCustomAttributes;
            this.provideResources = provideResources;
            this.provideTypeNode = provider;
            this.provideTypeNodeList = listProvider;
            this.IsNormalized = true;
        }

        public virtual void Dispose()
        {
            if(this.reader != null)
                this.reader.Dispose();

            this.reader = null;
            ModuleReferenceList mrefs = this.moduleReferences;

            for(int i = 0, n = mrefs == null ? 0 : mrefs.Count; i < n; i++)
            {
                //^ assert mrefs != null;
                ModuleReference mr = mrefs[i];

                if(mr != null && mr.Module == null)
                    continue;

                mr.Module.Dispose();
            }

            this.moduleReferences = null;

            GC.SuppressFinalize(this);
        }

        private AssemblyReferenceList assemblyReferences;
        public AssemblyReferenceList AssemblyReferences
        {
            get { return this.assemblyReferences; }
            set { this.assemblyReferences = value; }
        }
        private AssemblyNode containingAssembly;
        /// <summary>The assembly, if any, that includes this module in its ModuleReferences.</summary>
        public AssemblyNode ContainingAssembly
        {
            get { return this.containingAssembly; }
            set { this.containingAssembly = value; }
        }
        private string directory;
        public string Directory
        {
            get { return this.directory; }
            set { this.directory = value; }
        }
        private AssemblyHashAlgorithm hashAlgorithm = AssemblyHashAlgorithm.SHA1;
        public AssemblyHashAlgorithm HashAlgorithm
        {
            get { return this.hashAlgorithm; }
            set { this.hashAlgorithm = value; }
        }
        private byte[] hashValue;
        public byte[] HashValue
        {
            get { return this.hashValue; }
            set { this.hashValue = value; }
        }
        private ModuleKind kind;
        /// <summary>An enumeration that indicates if the module is an executable, library or resource, and so on.</summary>
        public ModuleKind Kind
        {
            get { return this.kind; }
            set { this.kind = value; }
        }
        private string location;
        /// <summary>The path of the file from which this module or assembly was loaded or will be stored in.</summary>
        public string Location
        {
            get { return this.location; }
            set { this.location = value; }
        }
        private System.Guid mvid;
        public System.Guid Mvid
        {
            get { return this.mvid; }
            set { this.mvid = value; }
        }
        private string targetRuntimeVersion;
        /// <summary>Identifies the version of the CLR that is required to load this module or assembly.</summary>
        public string TargetRuntimeVersion
        {
            get { return this.targetRuntimeVersion; }
            set { this.targetRuntimeVersion = value; }
        }
        private int linkerMajorVersion = 6;
        public int LinkerMajorVersion
        {
            get { return this.linkerMajorVersion; }
            set { this.linkerMajorVersion = value; }
        }
        private int linkerMinorVersion;
        public int LinkerMinorVersion
        {
            get { return this.linkerMinorVersion; }
            set { this.linkerMinorVersion = value; }
        }
        private int metadataFormatMajorVersion;
        public int MetadataFormatMajorVersion
        {
            get { return this.metadataFormatMajorVersion; }
            set { this.metadataFormatMajorVersion = value; }
        }
        private int metadataFormatMinorVersion;
        public int MetadataFormatMinorVersion
        {
            get { return this.metadataFormatMinorVersion; }
            set { this.metadataFormatMinorVersion = value; }
        }
        private string name;
        /// <summary>The name of the module or assembly. Includes the file extension if the module is not an assembly.</summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        private PEKindFlags peKind = PEKindFlags.ILonly;
        public PEKindFlags PEKind
        {
            get { return this.peKind; }
            set { this.peKind = value; }
        }
        private bool trackDebugData;
        public bool TrackDebugData
        {
            get { return this.trackDebugData; }
            set { this.trackDebugData = value; }
        }

        private ArrayList metadataImportErrors;
        /// <summary>
        /// If any exceptions were encountered while reading in this module, they are recorded here. Since reading is lazy,
        /// this list can grow dynamically during the use of a module.
        /// </summary>
        public ArrayList MetadataImportErrors
        {
            get { return this.metadataImportErrors; }
            set { this.metadataImportErrors = value; }
        }

        protected AttributeList attributes;
        /// <summary>
        /// The attributes associated with this module or assembly. This corresponds to C# custom attributes with the assembly or module target specifier.
        /// </summary>
        public virtual AttributeList Attributes
        {
            get
            {
                if(this.attributes != null)
                    return this.attributes;
                if(this.provideCustomAttributes != null)
                {
                    lock(Module.GlobalLock)
                    {
                        if(this.attributes == null)
                            this.provideCustomAttributes(this);
                    }
                }
                else
                    this.attributes = new AttributeList();
                return this.attributes;
            }
            set
            {
                this.attributes = value;
            }
        }

        protected SecurityAttributeList securityAttributes;
        /// <summary>
        /// Declarative security for the module or assembly.
        /// </summary>
        public virtual SecurityAttributeList SecurityAttributes
        {
            get
            {
                if(this.securityAttributes != null)
                    return this.securityAttributes;
                if(this.provideCustomAttributes != null)
                {
                    AttributeList dummy = this.Attributes; //As a side effect, this.securityAttributes gets populated
                    if(dummy != null)
                        dummy = null;
                }
                else
                    this.securityAttributes = new SecurityAttributeList();
                return this.securityAttributes;
            }
            set
            {
                this.securityAttributes = value;
            }
        }

        /// <summary>The source code, if any, corresponding to the value in Documentation.</summary>
        public Node DocumentationNode;

        protected XmlDocument documentation;
        /// <summary>An XML Document Object Model for a document containing all of the documentation comments applicable to members
        /// defined in this module.</summary>
        public virtual XmlDocument Documentation
        {
            get
            {
                XmlDocument documentation = this.documentation;
                if(documentation != null)
                    return documentation;
                if(this.DocumentationResolution != null)
                    documentation = this.documentation = this.DocumentationResolution(this);
                if(documentation != null)
                    return documentation;
                XmlDocument doc = null;
                if(this.Directory != null && this.Name != null)
                {
                    string fileName = this.Name + ".xml";
                    System.Globalization.CultureInfo cc = System.Globalization.CultureInfo.CurrentUICulture;
                    while(cc != null && cc != System.Globalization.CultureInfo.InvariantCulture)
                    {
                        doc = this.ProbeForXmlDocumentation(this.Directory, cc.Name, fileName);
                        if(doc != null)
                            break;
                        cc = cc.Parent;
                    }
                    if(doc == null)
                        doc = this.ProbeForXmlDocumentation(this.Directory, null, fileName);
                }
                if(doc == null)
                    doc = new XmlDocument();
                return this.documentation = doc;
            }
            set
            {
                this.documentation = value;
            }
        }
        public virtual XmlDocument ProbeForXmlDocumentation(string dir, string subDir, string fileName)
        {
            try
            {
                if(dir == null || fileName == null)
                    return null;
                if(subDir != null)
                    dir = Path.Combine(dir, subDir);
                string docFileName = Path.Combine(dir, fileName);
                if(File.Exists(docFileName))
                {
                    XmlDocument doc = new XmlDocument();
                    using(TextReader reader = File.OpenText(docFileName))
                    {
                        doc.Load(reader);
                        return doc;
                    }
                }
            }
            catch(Exception e)
            {
                if(this.MetadataImportErrors == null)
                    this.MetadataImportErrors = new ArrayList();
                this.MetadataImportErrors.Add(e);
            }
            return null;
        }

        protected internal static readonly Method NoSuchMethod = new Method();
        protected Method entryPoint;
        /// <summary>If this module is an executable, this method is the one that gets called to start the execution of managed code.</summary>
        public virtual Method EntryPoint
        {
            get
            {
                if(this.entryPoint == null)
                {
                    if(this.provideCustomAttributes != null)
                    {
                        AttributeList dummy = this.Attributes; //Gets the entry point as a side effect
                        if(dummy != null)
                            dummy = null;
                    }
                    else
                        this.entryPoint = Module.NoSuchMethod;
                }
                if(this.entryPoint == Module.NoSuchMethod)
                    return null;
                return this.entryPoint;
            }
            set
            {
                this.entryPoint = value;
            }
        }
        protected ModuleReferenceList moduleReferences;
        /// <summary>The list of modules (excluding assemblies) defining members that are referred to in this module or assembly.</summary>
        public ModuleReferenceList ModuleReferences
        {
            get
            {
                //Populating the type list may cause module references to be added
                if(this.Types == null)
                    return this.moduleReferences;
                return this.moduleReferences;
            }
            set
            {
                this.moduleReferences = value;
            }
        }

        public virtual bool ContainsModule(Module module)
        {
            if(module == null || this.ModuleReferences == null || this.ModuleReferences.Count == 0)
                return false;
            int n = this.ModuleReferences.Count;
            for(int i = 0; i < n; ++i)
            {
                ModuleReference mr = this.ModuleReferences[i];
                if(mr == null)
                    continue;
                if(mr.Module == module)
                    return true;
            }
            return false;
        }

        protected ResourceList resources;

        /// <summary>
        /// A list of managed resources linked or embedded into this module or assembly.
        /// </summary>
        public virtual ResourceList Resources
        {
            get
            {
                if(this.resources != null)
                    return this.resources;
                if(this.provideResources != null)
                {
                    lock(Module.GlobalLock)
                    {
                        if(this.resources == null)
                            this.provideResources(this);
                    }
                }
                else
                    this.resources = new ResourceList();
                return this.resources;
            }
            set
            {
                this.resources = value;
            }
        }
        protected Win32ResourceList win32Resources;
        /// <summary>
        /// A list of Win32 resources embedded in this module or assembly.
        /// </summary>
        public virtual Win32ResourceList Win32Resources
        {
            get
            {
                if(this.win32Resources != null)
                    return this.win32Resources;
                if(this.provideResources != null)
                {
                    ResourceList dummy = this.Resources; //gets the win32 resources as a side effect
                    if(dummy != null)
                        dummy = null;
                }
                else
                    this.win32Resources = new Win32ResourceList();
                return this.win32Resources;
            }
            set
            {
                this.win32Resources = value;
            }
        }

        /// <summary>
        /// Gets the first attribute of the given type in the custom attribute list of this module. Returns null if none found.
        /// This should not be called until the module has been processed to replace symbolic references
        /// to members with references to the actual members.
        /// </summary>
        public virtual AttributeNode GetAttribute(TypeNode attributeType)
        {
            AttributeList attributes = this.GetAttributes(attributeType, 1);
            if(attributes != null && attributes.Count > 0)
                return attributes[0];
            return null;
        }

        public virtual AttributeList GetAttributes(TypeNode attributeType)
        {
            return GetAttributes(attributeType, Int32.MaxValue);
        }

        public virtual AttributeList GetAttributes(TypeNode attributeType, int maxCount)
        {
            AttributeList foundAttributes = new AttributeList();
            if(attributeType == null)
                return foundAttributes;
            AttributeList attributes = this.Attributes;
            for(int i = 0, count = 0, n = attributes == null ? 0 : attributes.Count; i < n && count < maxCount; i++)
            {
                AttributeNode attr = attributes[i];
                if(attr == null)
                    continue;
                MemberBinding mb = attr.Constructor as MemberBinding;
                if(mb != null)
                {
                    if(mb.BoundMember == null)
                        continue;
                    if(mb.BoundMember.DeclaringType != attributeType)
                        continue;
                    foundAttributes.Add(attr);
                    count++;
                    continue;
                }
                Literal lit = attr.Constructor as Literal;
                if(lit == null)
                    continue;
                if((lit.Value as TypeNode) != attributeType)
                    continue;
                foundAttributes.Add(attr);
                count++;
            }
            return foundAttributes;
        }

        protected TrivialHashtable memberDocumentationCache;
        public TrivialHashtable GetMemberDocumentationCache()
        {
            TrivialHashtable cache = this.memberDocumentationCache;
            if(cache != null)
                return cache;
            lock(this)
            {
                if(this.memberDocumentationCache != null)
                    return this.memberDocumentationCache;
                XmlDocument doc = this.Documentation;
                if(doc == null && this.ContainingAssembly != null && this.ContainingAssembly != this)
                    return this.memberDocumentationCache = this.ContainingAssembly.memberDocumentationCache;
                cache = this.memberDocumentationCache = new TrivialHashtable();
                if(doc == null)
                    return cache;
                XmlNode docElem = doc.DocumentElement;
                if(docElem == null)
                    return cache;
                XmlNode membersNode = null;
                if(docElem.HasChildNodes)
                {
                    foreach(XmlNode dec in docElem.ChildNodes)
                        if(dec.Name == "members") { membersNode = dec; break; }
                }
                if(membersNode == null)
                    return cache;
                if(membersNode.HasChildNodes)
                {
                    foreach(XmlNode member in membersNode.ChildNodes)
                    {
                        if(member.Name != "member")
                            continue;
                        XmlNode nameAttr = member.Attributes.GetNamedItem("name");
                        if(nameAttr == null)
                            continue;
                        cache[Identifier.For(nameAttr.Value).UniqueIdKey] = member;
                    }
                }
                return cache;
            }
        }

        protected TrivialHashtable validNamespaces;
        public NamespaceList GetNamespaceList()
        {
            if(this.reader != null)
                return this.GetNamespaceListFromReader();

            TypeNodeList types = this.Types;
            int n = types == null ? 0 : types.Count;
            if(this.namespaceList == null || n > this.savedTypesLength)
            {
                lock(this)
                {
                    if(this.namespaceList != null && this.types != null && this.types.Count == this.savedTypesLength)
                        return this.namespaceList;
                    NamespaceList nsList = this.namespaceList = new NamespaceList();
                    TrivialHashtable nsTable = this.validNamespaces = new TrivialHashtable();
                    for(int i = 0; i < n; i++)
                    {
                        //^ assert this.types != null;
                        TypeNode t = this.types[i];
                        if(t == null)
                            continue;
                        if(t.Namespace == null)
                            t.Namespace = Identifier.Empty;
                        Namespace ns = nsTable[t.Namespace.UniqueIdKey] as Namespace;
                        if(ns != null)
                        {
                            if(t.IsPublic)
                                ns.isPublic = true;
                            ns.Types.Add(t);
                            continue;
                        }
                        ns = new Namespace(t.Namespace);
                        ns.isPublic = t.IsPublic;
                        ns.Types = new TypeNodeList();
                        ns.Types.Add(t);
                        nsTable[t.Namespace.UniqueIdKey] = ns;
                        nsList.Add(ns);
                    }
                }
            }

            return this.namespaceList;
        }
        private NamespaceList GetNamespaceListFromReader()
        //^ requires this.reader != null;
        {
            if(this.namespaceList == null)
            {
                lock(Module.GlobalLock)
                {
                    this.reader.GetNamespaces();
                    NamespaceList nsList = this.namespaceList = this.reader.namespaceList;
                    TrivialHashtable nsTable = this.validNamespaces = new TrivialHashtable();
                    for(int i = 0, n = nsList == null ? 0 : nsList.Count; i < n; i++)
                    {
                        //^ assert nsList != null;
                        Namespace ns = nsList[i];
                        if(ns == null || ns.Name == null)
                            continue;
                        ns.ProvideTypes = new Namespace.TypeProvider(this.GetTypesForNamespace);
                        nsTable[ns.Name.UniqueIdKey] = ns;
                    }
                }
            }
            return this.namespaceList;
        }
        private void GetTypesForNamespace(Namespace nspace, object handle)
        {
            if(nspace == null || nspace.Name == null)
                return;
            lock(Module.GlobalLock)
            {
                int key = nspace.Name.UniqueIdKey;
                TypeNodeList types = this.Types;
                TypeNodeList nsTypes = nspace.Types = new TypeNodeList();
                for(int i = 0, n = types == null ? 0 : types.Count; i < n; i++)
                {
                    TypeNode t = types[i];
                    if(t == null || t.Namespace == null)
                        continue;
                    if(t.Namespace.UniqueIdKey == key)
                        nsTypes.Add(t);
                }
            }
        }
        public bool IsValidNamespace(Identifier nsName)
        {
            if(nsName == null)
                return false;
            this.GetNamespaceList();
            //^ assert this.validNamespaces != null;
            return this.validNamespaces[nsName.UniqueIdKey] != null;
        }
        public bool IsValidTypeName(Identifier nsName, Identifier typeName)
        {
            if(nsName == null || typeName == null)
                return false;
            if(!this.IsValidNamespace(nsName))
                return false;
            if(this.reader != null)
                return this.reader.IsValidTypeName(nsName, typeName);
            return this.GetType(nsName, typeName) != null;
        }
        public Module GetNestedModule(string moduleName)
        {
            if(this.Types == null) { Debug.Assert(false); } //Just get the types to pull in any exported types
            ModuleReferenceList moduleReferences = this.ModuleReferences; //This should now contain all interesting referenced modules
            for(int i = 0, n = moduleReferences == null ? 0 : moduleReferences.Count; i < n; i++)
            {
                ModuleReference mref = moduleReferences[i];
                if(mref == null)
                    continue;
                if(mref.Name == moduleName)
                    return mref.Module;
            }
            return null;
        }
        internal TrivialHashtableUsingWeakReferences/*!*/ StructurallyEquivalentType
        {
            get
            {
                if(this.structurallyEquivalentType == null)
                    this.structurallyEquivalentType = new TrivialHashtableUsingWeakReferences();
                return this.structurallyEquivalentType;
            }
        }
        private TrivialHashtableUsingWeakReferences structurallyEquivalentType;
        /// <summary>
        /// The identifier represents the structure via some mangling scheme. The result can be either from this module,
        /// or any module this module has a reference to.
        /// </summary>
        public virtual TypeNode GetStructurallyEquivalentType(Identifier ns, Identifier/*!*/ id)
        {
            return this.GetStructurallyEquivalentType(ns, id, id, true);
        }
        public virtual TypeNode GetStructurallyEquivalentType(Identifier ns, Identifier/*!*/ id, Identifier uniqueMangledName, bool lookInReferencedAssemblies)
        {
            if(uniqueMangledName == null)
                uniqueMangledName = id;
            TypeNode result = (TypeNode)this.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey];
            if(result == Class.DoesNotExist)
                return null;
            if(result != null)
                return result;
            lock(Module.GlobalLock)
            {
                result = this.GetType(ns, id);
                if(result != null)
                {
                    this.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey] = result;
                    return result;
                }
                if(!lookInReferencedAssemblies)
                    goto notfound;
                AssemblyReferenceList refs = this.AssemblyReferences;
                for(int i = 0, n = refs == null ? 0 : refs.Count; i < n; i++)
                {
                    AssemblyReference ar = refs[i];
                    if(ar == null)
                        continue;
                    AssemblyNode a = ar.Assembly;
                    if(a == null)
                        continue;
                    result = a.GetType(ns, id);
                    if(result != null)
                    {
                        this.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey] = result;
                        return result;
                    }
                }
notfound:
                this.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey] = Class.DoesNotExist;
                return null;
            }
        }
        public virtual TypeNode GetType(Identifier @namespace, Identifier name, bool lookInReferencedAssemblies)
        {
            return this.GetType(@namespace, name, lookInReferencedAssemblies, lookInReferencedAssemblies ? new TrivialHashtable() : null);
        }
        protected virtual TypeNode GetType(Identifier @namespace, Identifier name, bool lookInReferencedAssemblies, TrivialHashtable assembliesAlreadyVisited)
        {
            if(assembliesAlreadyVisited != null)
            {
                if(assembliesAlreadyVisited[this.UniqueKey] != null)
                    return null;
                assembliesAlreadyVisited[this.UniqueKey] = this;
            }
            TypeNode result = this.GetType(@namespace, name);
            if(result != null || !lookInReferencedAssemblies)
                return result;
            AssemblyReferenceList refs = this.AssemblyReferences;
            for(int i = 0, n = refs == null ? 0 : refs.Count; i < n; i++)
            {
                AssemblyReference ar = refs[i];
                if(ar == null)
                    continue;
                AssemblyNode a = ar.Assembly;
                if(a == null)
                    continue;
                result = a.GetType(@namespace, name, true, assembliesAlreadyVisited);
                if(result != null)
                    return result;
            }
            return null;
        }
        public virtual TypeNode GetType(Identifier @namespace, Identifier name)
        {
            if(@namespace == null || name == null)
                return null;
            TypeNode result = null;
            if(this.namespaceTable == null)
                this.namespaceTable = new TrivialHashtable();
            TrivialHashtable nsTable = (TrivialHashtable)this.namespaceTable[@namespace.UniqueIdKey];
            if(nsTable != null)
            {
                result = (TypeNode)nsTable[name.UniqueIdKey];
                if(result == Class.DoesNotExist)
                    return null;
                if(result != null)
                    return result;
            }
            else
            {
                lock(Module.GlobalLock)
                {
                    nsTable = (TrivialHashtable)this.namespaceTable[@namespace.UniqueIdKey];
                    if(nsTable == null)
                        this.namespaceTable[@namespace.UniqueIdKey] = nsTable = new TrivialHashtable(32);
                }
            }
            if(this.provideTypeNode != null)
            {
                lock(Module.GlobalLock)
                {
                    result = (TypeNode)nsTable[name.UniqueIdKey];
                    if(result == Class.DoesNotExist)
                        return null;
                    if(result != null)
                        return result;
                    result = this.provideTypeNode(@namespace, name);
                    if(result != null)
                    {
                        nsTable[name.UniqueIdKey] = result;
                        return result;
                    }

                    // !EFW - Removed by ComponentOne
                    //nsTable[name.UniqueIdKey] = Class.DoesNotExist;

                    return null;
                }
            }
            if(this.types != null && this.types.Count > this.savedTypesLength)
            {
                int n = this.savedTypesLength = this.types.Count;
                for(int i = 0; i < n; i++)
                {
                    TypeNode t = this.types[i];
                    if(t == null)
                        continue;
                    if(t.Namespace == null)
                        t.Namespace = Identifier.Empty;
                    nsTable = (TrivialHashtable)this.namespaceTable[t.Namespace.UniqueIdKey];
                    if(nsTable == null)
                        this.namespaceTable[t.Namespace.UniqueIdKey] = nsTable = new TrivialHashtable();
                    nsTable[t.Name.UniqueIdKey] = t;
                }
                return this.GetType(@namespace, name);
            }
            return null;
        }
        protected internal TypeNodeList types;
        /// <summary>The types contained in this module or assembly.</summary>
        public virtual TypeNodeList Types
        {
            get
            {
                if(this.types != null)
                    return this.types;
                if(this.provideTypeNodeList != null)
                {
                    lock(Module.GlobalLock)
                    {
                        if(this.types == null)
                            this.provideTypeNodeList(this);
                    }
                }
                else
                    this.types = new TypeNodeList();
                return this.types;
            }
            set
            {
                this.types = value;
            }
        }

        protected TrivialHashtable referencedModulesAndAssemblies;

        public virtual bool HasReferenceTo(Module module)
        {
            if(module == null)
                return false;
            AssemblyNode assembly = module as AssemblyNode;
            if(assembly != null)
            {
                AssemblyReferenceList arefs = this.AssemblyReferences;
                for(int i = 0, n = arefs == null ? 0 : arefs.Count; i < n; i++)
                {
                    AssemblyReference aref = arefs[i];
                    if(aref == null)
                        continue;
                    if(aref.Matches(assembly.Name, assembly.Version, assembly.Culture, assembly.PublicKeyToken))
                        return true;
                }
            }
            if(this.ContainingAssembly != module.ContainingAssembly)
                return false;
            ModuleReferenceList mrefs = this.ModuleReferences;
            for(int i = 0, n = mrefs == null ? 0 : mrefs.Count; i < n; i++)
            {
                //^ assert mrefs != null;
                ModuleReference mref = mrefs[i];
                if(mref == null || mref.Name == null)
                    continue;
                if(0 == PlatformHelpers.StringCompareOrdinalIgnoreCase(mref.Name, module.Name))
                    return true;
            }
            return false;
        }
        internal void InitializeAssemblyReferenceResolution(Module referringModule)
        {
            if(this.AssemblyReferenceResolution == null && referringModule != null)
            {
                this.AssemblyReferenceResolution = referringModule.AssemblyReferenceResolution;
                this.AssemblyReferenceResolutionAfterProbingFailed = referringModule.AssemblyReferenceResolutionAfterProbingFailed;
            }
        }

        public static Module GetModule(byte[] buffer)
        {
            return Module.GetModule(buffer, null, false, false, true, false);
        }
        public static Module GetModule(byte[] buffer, IDictionary cache)
        {
            return Module.GetModule(buffer, null, false, false, false, false);
        }
        public static Module GetModule(byte[] buffer, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache)
        {
            return Module.GetModule(buffer, cache, doNotLockFile, getDebugInfo, useGlobalCache, false);
        }
        public static Module GetModule(byte[] buffer, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches)
        {
            if(buffer == null)
                return null;
            return (new Reader(buffer, cache, doNotLockFile, getDebugInfo, useGlobalCache, false)).ReadModule();
        }

        public static Module GetModule(string location)
        {
            return Module.GetModule(location, null, false, false, true, false);
        }
        public static Module GetModule(string location, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache)
        {
            return Module.GetModule(location, null, doNotLockFile, getDebugInfo, useGlobalCache, false);
        }
        public static Module GetModule(string location, IDictionary cache)
        {
            return Module.GetModule(location, cache, false, false, false, false);
        }
        public static Module GetModule(string location, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache)
        {
            return Module.GetModule(location, cache, doNotLockFile, getDebugInfo, useGlobalCache, false);
        }
        public static Module GetModule(string location, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches)
        {
            if(location == null)
                return null;
            return (new Reader(location, cache, doNotLockFile, getDebugInfo, useGlobalCache, preserveShortBranches)).ReadModule();
        }
        public virtual AssemblyNode Resolve(AssemblyReference assemblyReference)
        {
            if(this.AssemblyReferenceResolution == null)
                return null;
            return this.AssemblyReferenceResolution(assemblyReference, this);
        }
        public virtual AssemblyNode ResolveAfterProbingFailed(AssemblyReference assemblyReference)
        {
            if(this.AssemblyReferenceResolutionAfterProbingFailed == null)
                return null;
            return this.AssemblyReferenceResolutionAfterProbingFailed(assemblyReference, this);
        }

        public virtual void AfterAssemblyLoadProcessing()
        {
            if(this.AfterAssemblyLoad != null)
            {
                this.AfterAssemblyLoad(this as AssemblyNode);
            }
        }

        public virtual void WriteDocumentation(System.IO.TextWriter doc)
        {
            if(this.documentation == null)
                return;
            XmlTextWriter xwriter = new XmlTextWriter(doc);
            xwriter.Formatting = Formatting.Indented;
            xwriter.Indentation = 2;
            xwriter.WriteProcessingInstruction("xml", "version=\"1.0\"");
            xwriter.WriteStartElement("doc");
            AssemblyNode assem = this as AssemblyNode;
            if(assem != null)
            {
                xwriter.WriteStartElement("assembly");
                xwriter.WriteElementString("name", assem.Name);
                xwriter.WriteEndElement();
            }
            xwriter.WriteStartElement("members");
            TypeNodeList types = this.Types;
            for(int i = 1, n = types == null ? 0 : types.Count; i < n; i++)
            {
                //^ assert types != null;
                TypeNode t = types[i];
                if(t == null)
                    continue;
                t.WriteDocumentation(xwriter);
            }
            xwriter.WriteEndElement();
            xwriter.WriteEndElement();
            xwriter.Close();
        }
    }

    // An assembly is a module with a strong name
    public class AssemblyNode : Module
    {
        private static Hashtable CompiledAssemblies;// so we can find in-memory compiled assemblies later (contains weak references)

        protected AssemblyNode contractAssembly;
        /// <summary>A separate assembly that supplied the type and method contracts for this assembly.</summary>
        public virtual AssemblyNode ContractAssembly
        {
            get
            {
                return this.contractAssembly;
            }
            set
            {
                if(this.contractAssembly != null)
                {
                    Debug.Assert(false);
                    return;
                }
                this.contractAssembly = value;

                if(value == null)
                    return;

                #region Copy over any external references from the contract assembly to this one (if needed)

                // These external references are needed only for the contract deserializer
                AssemblyReferenceList ars = new AssemblyReferenceList();
                AssemblyReferenceList contractReferences = value.AssemblyReferences;

                // see if contractReferences[i] is already in the external references of "this"
                for(int i = 0, n = contractReferences == null ? 0 : contractReferences.Count; i < n; i++)
                {
                    //^ assert contractReferences != null;
                    AssemblyReference aref = contractReferences[i];
                    if(aref == null)
                        continue;
                    if(aref.Assembly != this)
                    { // don't copy the contract's external reference to "this"
                        int j = 0;
                        int m = this.AssemblyReferences == null ? 0 : this.AssemblyReferences.Count;
                        while(j < m)
                        {
                            if(aref.Assembly.Name != null &&
                              this.AssemblyReferences[j].Name != null &&
                              aref.Assembly.Name.Equals(this.AssemblyReferences[j].Name))
                                break;
                            j++;
                        }
                        if(j == m)
                        { // then it wasn't found in the list of the real references
                            ars.Add(contractReferences[i]);
                        }
                    }
                }

                if(this.AssemblyReferences == null)
                    this.AssemblyReferences = new AssemblyReferenceList(ars);
                else
                    this.AssemblyReferences.AddRange(ars);

                #endregion Copy over any external references from the contract assembly to this one (if needed)

                TypeNodeList instantiatedTypes = null;
                if(this.reader != null)
                    instantiatedTypes = this.reader.GetInstantiatedTypes();
                if(instantiatedTypes != null)
                    for(int i = 0, n = instantiatedTypes.Count; i < n; i++)
                    {
                        TypeNode t = instantiatedTypes[i];
                        if(t == null)
                            continue;

                        if(t.members == null)
                            continue;

                    }
            }
        }

        internal static readonly AssemblyNode/*!*/ Dummy = new AssemblyNode();
        protected string strongName;
        /// <summary>
        /// A string containing the name, version, culture and key of this assembly, formatted as required by the CLR loader.
        /// </summary>
        public virtual string/*!*/ StrongName
        {
            get
            {
                if(this.strongName == null)
                    this.strongName = AssemblyNode.GetStrongName(this.Name, this.Version, this.Culture, this.PublicKeyOrToken, (this.Flags & AssemblyFlags.Retargetable) != 0);
                return this.strongName;
            }
        }

        [Obsolete("Please use GetAttribute(TypeNode attributeType)")]
        public virtual AttributeNode GetAttributeByName(TypeNode attributeType)
        {
            if(attributeType == null)
                return null;
            AttributeList attributes = this.Attributes;
            for(int i = 0, n = attributes == null ? 0 : attributes.Count; i < n; i++)
            {
                //^ assert attributes != null;
                AttributeNode attr = attributes[i];
                if(attr == null)
                    continue;
                MemberBinding mb = attr.Constructor as MemberBinding;
                if(mb != null)
                {
                    if(mb.BoundMember == null || mb.BoundMember.DeclaringType == null)
                        continue;
                    if(mb.BoundMember.DeclaringType.FullName != attributeType.FullName)
                        continue;
                    return attr;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the first attribute of the given type in the custom attribute list of this member. Returns null if none found.
        /// The member is assumed to be either imported, or already in a form suitable for export.
        /// </summary>
        public virtual AttributeNode GetModuleAttribute(TypeNode attributeType)
        {
            if(attributeType == null)
                return null;
            AttributeList attributes = this.ModuleAttributes;
            for(int i = 0, n = attributes == null ? 0 : attributes.Count; i < n; i++)
            {
                //^ assert attributes != null;
                AttributeNode attr = attributes[i];
                if(attr == null)
                    continue;
                MemberBinding mb = attr.Constructor as MemberBinding;
                if(mb != null)
                {
                    if(mb.BoundMember == null)
                        continue;
                    if(mb.BoundMember.DeclaringType != attributeType)
                        continue;
                    return attr;
                }
                Literal lit = attr.Constructor as Literal;
                if(lit == null)
                    continue;
                if((lit.Value as TypeNode) != attributeType)
                    continue;
                return attr;
            }
            return null;
        }

        public AssemblyNode()
            : base()
        {
            this.NodeType = NodeType.Assembly;
            this.ContainingAssembly = this;
        }
        public AssemblyNode(TypeNodeProvider provider, TypeNodeListProvider listProvider,
          CustomAttributeProvider provideCustomAttributes, ResourceProvider provideResources, string directory)
            : base(provider, listProvider, provideCustomAttributes, provideResources)
        {
            this.Directory = directory;
            this.NodeType = NodeType.Assembly;
            this.ContainingAssembly = this;
        }
        public override void Dispose()
        {
            if(this.cachedRuntimeAssembly != null)
                this.cachedRuntimeAssembly.Dispose();
            this.cachedRuntimeAssembly = null;

            lock(Reader.StaticAssemblyCache)
            {
                foreach(object key in new ArrayList(Reader.StaticAssemblyCache.Keys))
                {
                    if(Reader.StaticAssemblyCache[key] == this)
                        Reader.StaticAssemblyCache.Remove(key);
                }
                AssemblyReference aRef = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[Identifier.For(this.Name).UniqueIdKey];
                if(aRef != null && aRef.Assembly == this)
                {
                    aRef.Assembly = null;
                    //TODO: what about other static references to the assembly, such as SystemTypes.SystemXmlAssembly?
                }
            }
            base.Dispose();
        }
        private string culture;
        /// <summary>The target culture of any localized resources in this assembly.</summary>
        public string Culture
        {
            get { return this.culture; }
            set { this.culture = value; }
        }
        private AssemblyFlags flags;
        /// <summary>An enumeration that identifies the what kind of assembly this is.</summary>
        public AssemblyFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }
        private string moduleName;
        /// <summary>Attributes that specifically target a module rather an assembly.</summary>
        public string ModuleName
        { //An assembly can have a different name from the module.
            get { return this.moduleName; }
            set { this.moduleName = value; }
        }
        private byte[] publicKeyOrToken;
        /// <summary>The public part of the key pair used to sign this assembly, or a hash of the public key.</summary>
        public byte[] PublicKeyOrToken
        {
            get { return this.publicKeyOrToken; }
            set { this.publicKeyOrToken = value; }
        }
        private System.Version version;
        /// <summary>The version of this assembly.</summary>
        public System.Version Version
        {
            get { return this.version; }
            set { this.version = value; }
        }
        private DateTime fileLastWriteTimeUtc;
        public DateTime FileLastWriteTimeUtc
        {
            get { return this.fileLastWriteTimeUtc; }
            set { this.fileLastWriteTimeUtc = value; }
        }
        protected TypeNodeList exportedTypes;
        /// <summary>
        /// Public types defined in other modules making up this assembly and to which other assemblies may refer to.
        /// </summary>
        public virtual TypeNodeList ExportedTypes
        {
            get
            {
                if(this.exportedTypes != null)
                    return this.exportedTypes;
                if(this.provideTypeNodeList != null)
                {
                    TypeNodeList types = this.Types; //Gets the exported types as a side-effect
                    if(types != null)
                        types = null;
                }
                else
                    this.exportedTypes = new TypeNodeList();
                return this.exportedTypes;
            }
            set
            {
                this.exportedTypes = value;
            }
        }
        public bool GetDebugSymbols
        {
            get
            {
                if(this.reader == null)
                    return false;
                return this.reader.getDebugSymbols;
            }
            set
            {
                if(this.reader == null)
                    return;
                this.reader.getDebugSymbols = value;
            }
        }

        public static AssemblyNode GetAssembly(byte[] buffer)
        {
            return AssemblyNode.GetAssembly(buffer, null, false, false, true, false);
        }
        public static AssemblyNode GetAssembly(byte[] buffer, IDictionary cache)
        {
            return AssemblyNode.GetAssembly(buffer, cache, false, false, false, false);
        }
        public static AssemblyNode GetAssembly(byte[] buffer, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache)
        {
            return AssemblyNode.GetAssembly(buffer, cache, doNotLockFile, getDebugInfo, useGlobalCache, false);
        }
        public static AssemblyNode GetAssembly(byte[] buffer, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches)
        {
            if(buffer == null)
                return null;
            if(CoreSystemTypes.SystemAssembly == null)
                Debug.Fail("");
            return (new Reader(buffer, cache, doNotLockFile, getDebugInfo, useGlobalCache, preserveShortBranches)).ReadModule() as AssemblyNode;
        }

        public static AssemblyNode GetAssembly(string location)
        {
            return AssemblyNode.GetAssembly(location, null, false, false, true, false);
        }
        public static AssemblyNode GetAssembly(string location, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache)
        {
            return AssemblyNode.GetAssembly(location, null, doNotLockFile, getDebugInfo, useGlobalCache, false);
        }
        public static AssemblyNode GetAssembly(string location, IDictionary cache)
        {
            return AssemblyNode.GetAssembly(location, cache, false, false, false, false);
        }
        public static AssemblyNode GetAssembly(string location, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache)
        {
            return AssemblyNode.GetAssembly(location, cache, doNotLockFile, getDebugInfo, useGlobalCache, false);
        }
        public static AssemblyNode GetAssembly(string location, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches)
        {
            if(location == null)
                return null;
            if(CoreSystemTypes.SystemAssembly == null)
                Debug.Fail("");
            return (new Reader(location, cache, doNotLockFile, getDebugInfo, useGlobalCache, preserveShortBranches)).ReadModule() as AssemblyNode;
        }

        public static AssemblyNode GetAssembly(AssemblyReference assemblyReference)
        {
            return AssemblyNode.GetAssembly(assemblyReference, null, false, false, true, false);
        }
        public static AssemblyNode GetAssembly(AssemblyReference assemblyReference, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache)
        {
            return AssemblyNode.GetAssembly(assemblyReference, null, doNotLockFile, getDebugInfo, useGlobalCache, false);
        }
        public static AssemblyNode GetAssembly(AssemblyReference assemblyReference, IDictionary cache)
        {
            return AssemblyNode.GetAssembly(assemblyReference, cache, false, false, false, false);
        }
        public static AssemblyNode GetAssembly(AssemblyReference assemblyReference, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache)
        {
            return AssemblyNode.GetAssembly(assemblyReference, cache, doNotLockFile, getDebugInfo, useGlobalCache, false);
        }
        public static AssemblyNode GetAssembly(AssemblyReference assemblyReference, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches)
        {
            if(assemblyReference == null)
                return null;
            if(CoreSystemTypes.SystemAssembly == null)
                Debug.Fail("");
            Reader reader = new Reader(cache, doNotLockFile, getDebugInfo, useGlobalCache, preserveShortBranches);
            return assemblyReference.Assembly = reader.GetAssemblyFromReference(assemblyReference);
        }

        public static AssemblyNode GetAssembly(System.Reflection.Assembly runtimeAssembly)
        {
            return AssemblyNode.GetAssembly(runtimeAssembly, null, false, true, false);
        }
        public static AssemblyNode GetAssembly(System.Reflection.Assembly runtimeAssembly, IDictionary cache)
        {
            return AssemblyNode.GetAssembly(runtimeAssembly, cache, false, false, false);
        }
        public static AssemblyNode GetAssembly(System.Reflection.Assembly runtimeAssembly, IDictionary cache, bool getDebugInfo, bool useGlobalCache)
        {
            return AssemblyNode.GetAssembly(runtimeAssembly, cache, getDebugInfo, useGlobalCache, false);
        }
        public static AssemblyNode GetAssembly(System.Reflection.Assembly runtimeAssembly, IDictionary cache, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches)
        {
            if(runtimeAssembly == null)
                return null;
            if(CoreSystemTypes.SystemAssembly == null)
                Debug.Fail("");
            if(runtimeAssembly.GetName().Name == "mscorlib")
            {
                return CoreSystemTypes.SystemAssembly;
            }
            if(AssemblyNode.CompiledAssemblies != null)
            {
                WeakReference weakRef = (WeakReference)AssemblyNode.CompiledAssemblies[runtimeAssembly];
                if(weakRef != null)
                {
                    AssemblyNode assem = (AssemblyNode)weakRef.Target;
                    if(assem == null)
                        AssemblyNode.CompiledAssemblies.Remove(runtimeAssembly); //Remove the dead WeakReference
                    return assem;
                }
            }
            if(runtimeAssembly.Location != null && runtimeAssembly.Location.Length > 0)
                return AssemblyNode.GetAssembly(runtimeAssembly.Location, cache, false, getDebugInfo, useGlobalCache, preserveShortBranches);
            //Get here for in memory assemblies that were not loaded from a known AssemblyNode
            //Need CLR support to handle such assemblies. For now return null.
            return null;
        }

        public void SetupDebugReader(string pdbSearchPath)
        {
            if(this.reader == null) { Debug.Assert(false); return; }
            this.reader.SetupDebugReader(this.Location, pdbSearchPath);
        }
        internal static string/*!*/ GetStrongName(string name, Version version, string culture, byte[] publicKey, bool retargetable)
        {
            if(version == null)
                version = new Version();
            StringBuilder result = new StringBuilder();
            result.Append(name);
            result.Append(", Version=");
            result.Append(version.ToString());
            result.Append(", Culture=");
            result.Append(((culture == null || culture.Length == 0) ? "neutral" : culture));
            result.Append(AssemblyNode.GetKeyString(publicKey));
            if(retargetable)
                result.Append(", Retargetable=Yes");
            return result.ToString();
        }
        private System.Reflection.AssemblyName assemblyName;
        public System.Reflection.AssemblyName GetAssemblyName()
        {
            if(this.assemblyName == null)
            {
                System.Reflection.AssemblyName aName = new System.Reflection.AssemblyName();
                if(this.Location != null && this.Location != "unknown:location")
                {
                    StringBuilder sb = new StringBuilder("file:///");
                    sb.Append(Path.GetFullPath(this.Location));
                    sb.Replace('\\', '/');
                    aName.CodeBase = sb.ToString();
                }
                aName.CultureInfo = new System.Globalization.CultureInfo(this.Culture);
                if(this.PublicKeyOrToken != null && this.PublicKeyOrToken.Length > 8)
                    aName.Flags = System.Reflection.AssemblyNameFlags.PublicKey;
                if((this.Flags & AssemblyFlags.Retargetable) != 0)
                    aName.Flags |= (System.Reflection.AssemblyNameFlags)AssemblyFlags.Retargetable;
                aName.HashAlgorithm = (System.Configuration.Assemblies.AssemblyHashAlgorithm)this.HashAlgorithm;
                if(this.PublicKeyOrToken != null && this.PublicKeyOrToken.Length > 0)
                    aName.SetPublicKey(this.PublicKeyOrToken);
                else
                    aName.SetPublicKey(new byte[0]);
                aName.Name = this.Name;
                aName.Version = this.Version;
                switch(this.Flags & AssemblyFlags.CompatibilityMask)
                {
                    case AssemblyFlags.NonSideBySideCompatible:
                        aName.VersionCompatibility = System.Configuration.Assemblies.AssemblyVersionCompatibility.SameDomain;
                        break;
                    case AssemblyFlags.NonSideBySideProcess:
                        aName.VersionCompatibility = System.Configuration.Assemblies.AssemblyVersionCompatibility.SameProcess;
                        break;
                    case AssemblyFlags.NonSideBySideMachine:
                        aName.VersionCompatibility = System.Configuration.Assemblies.AssemblyVersionCompatibility.SameMachine;
                        break;
                }
                this.assemblyName = aName;
            }
            return this.assemblyName;
        }

        private sealed class CachedRuntimeAssembly : IDisposable
        {
            internal System.Reflection.Assembly Value;
            internal CachedRuntimeAssembly(System.Reflection.Assembly assembly)
            {
                this.Value = assembly;
            }
            ~CachedRuntimeAssembly()
            {
                this.Dispose();
            }
            public void Dispose()
            {
                if(this.Value != null)
                {
                    if(AssemblyNode.CompiledAssemblies != null)
                        AssemblyNode.CompiledAssemblies.Remove(this.Value);
                }
                this.Value = null;
                GC.SuppressFinalize(this);
            }
        }
        private CachedRuntimeAssembly cachedRuntimeAssembly;
        public System.Reflection.Assembly GetRuntimeAssembly()
        {
            return this.GetRuntimeAssembly(null, null);
        }

        public System.Reflection.Assembly GetRuntimeAssembly(System.Security.Policy.Evidence evidence)
        {
            return this.GetRuntimeAssembly(evidence, null);
        }
        public System.Reflection.Assembly GetRuntimeAssembly(AppDomain targetAppDomain)
        {
            return this.GetRuntimeAssembly(null, targetAppDomain);
        }

        // TODO: Evidence is obsolete but I'm not sure if it can be removed yet as this can parse assemblies from
        // prior framework versions that do use it so we'll just suppress the warning for now.
#pragma warning disable 0618

        public System.Reflection.Assembly GetRuntimeAssembly(System.Security.Policy.Evidence evidence, AppDomain targetAppDomain)
        {
            System.Reflection.Assembly result = this.cachedRuntimeAssembly == null ? null : this.cachedRuntimeAssembly.Value;
            if(result == null || evidence != null || targetAppDomain != null)
            {
                lock(this)
                {
                    if(this.cachedRuntimeAssembly != null && evidence == null && targetAppDomain == null)
                        return this.cachedRuntimeAssembly.Value;
                    if(targetAppDomain == null)
                        targetAppDomain = AppDomain.CurrentDomain;
                    if(this.Location != null)
                    {
                        string name = this.StrongName;
                        System.Reflection.Assembly[] alreadyLoadedAssemblies = targetAppDomain.GetAssemblies();
                        if(alreadyLoadedAssemblies != null)
                            for(int i = 0, n = alreadyLoadedAssemblies.Length; i < n; i++)
                            {
                                System.Reflection.Assembly a = alreadyLoadedAssemblies[i];
                                if(a == null)
                                    continue;
                                if(a.FullName == name)
                                {
                                    result = a;
                                    break;
                                }
                            }
                        if(result == null)
                        {
                            if(evidence != null)
                                result = targetAppDomain.Load(this.GetAssemblyName(), evidence);
                            else
                                result = targetAppDomain.Load(this.GetAssemblyName());
                        }
                    }

                    if(result != null && evidence == null && targetAppDomain == AppDomain.CurrentDomain)
                    {
                        this.AddCachedAssembly(result);
                        this.cachedRuntimeAssembly = new CachedRuntimeAssembly(result);
                    }
                }
            }
            return result;
        }
#pragma warning restore 0618

        private void AddCachedAssembly(System.Reflection.Assembly/*!*/ runtimeAssembly)
        {
            if(AssemblyNode.CompiledAssemblies == null)
                AssemblyNode.CompiledAssemblies = Hashtable.Synchronized(new Hashtable());
            AssemblyNode.CompiledAssemblies[runtimeAssembly] = new WeakReference(this);
        }

        private static string GetKeyString(byte[] publicKey)
        {
            if(publicKey == null)
                return null;
            int n = publicKey.Length;
            StringBuilder str;
            if(n > 8)
            {
                System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                publicKey = sha1.ComputeHash(publicKey);
                byte[] token = new byte[8];
                for(int i = 0, m = publicKey.Length - 1; i < 8; i++)
                    token[i] = publicKey[m - i];
                publicKey = token;
                n = 8;
            }
            if(n == 0)
                str = new StringBuilder(", PublicKeyToken=null");
            else
                str = new StringBuilder(", PublicKeyToken=", n * 2 + 17);
            for(int i = 0; i < n; i++)
                str.Append(publicKey[i].ToString("x2"));
            return str.ToString();
        }
        protected TrivialHashtable friends;
        public virtual bool MayAccessInternalTypesOf(AssemblyNode assembly)
        {
            if(this == assembly)
                return true;
            if(assembly == null || SystemTypes.InternalsVisibleToAttribute == null)
                return false;
            if(this.friends == null)
                this.friends = new TrivialHashtable();
            object ob = this.friends[assembly.UniqueKey];
            if(ob == (object)string.Empty)
                return false;
            if(ob == this)
                return true;
            AttributeList attributes = assembly.Attributes;
            for(int i = 0, n = attributes == null ? 0 : attributes.Count; i < n; i++)
            {
                //^ assert attributes != null;
                AttributeNode attr = attributes[i];
                if(attr == null)
                    continue;
                MemberBinding mb = attr.Constructor as MemberBinding;
                if(mb != null)
                {
                    if(mb.BoundMember == null)
                        continue;
                    if(mb.BoundMember.DeclaringType != SystemTypes.InternalsVisibleToAttribute)
                        continue;
                }
                else
                {
                    Literal lit = attr.Constructor as Literal;
                    if(lit == null)
                        continue;
                    if((lit.Value as TypeNode) != SystemTypes.InternalsVisibleToAttribute)
                        continue;
                }
                if(attr.Expressions == null || attr.Expressions.Count < 1)
                    continue;
                Literal argLit = attr.Expressions[0] as Literal;
                if(argLit == null)
                    continue;
                string friendName = argLit.Value as string;
                if(friendName == null)
                    continue;
                try
                {
                    AssemblyReference ar = new AssemblyReference(friendName);
                    byte[] tok = ar.PublicKeyToken;
                    if(tok != null && this.PublicKeyOrToken != null)
                        tok = this.PublicKeyToken;
                    if(!ar.Matches(this.Name, ar.Version, ar.Culture, tok))
                        continue;
                }
                catch(ArgumentException e)
                {
                    if(this.MetadataImportErrors == null)
                        this.MetadataImportErrors = new ArrayList();
                    this.MetadataImportErrors.Add(e.Message);
                    continue;
                }

                this.friends[assembly.UniqueKey] = this;
                return true;
            }

            this.friends[assembly.UniqueKey] = string.Empty;
            return false;
        }

        public AssemblyReferenceList GetFriendAssemblies()
        {
            if(SystemTypes.InternalsVisibleToAttribute == null)
                return null;

            AttributeList attributes = this.Attributes;

            if(attributes == null)
                return null;

            int n = attributes.Count;

            if(n == 0)
                return null;

            AssemblyReferenceList result = new AssemblyReferenceList();

            for(int i = 0; i < n; i++)
            {
                AttributeNode attr = attributes[i];

                if(attr == null)
                    continue;

                MemberBinding mb = attr.Constructor as MemberBinding;

                if(mb != null)
                {
                    if(mb.BoundMember == null)
                        continue;

                    if(mb.BoundMember.DeclaringType != SystemTypes.InternalsVisibleToAttribute)
                        continue;
                }
                else
                {
                    Literal lit = attr.Constructor as Literal;

                    if(lit == null)
                        continue;

                    if((lit.Value as TypeNode) != SystemTypes.InternalsVisibleToAttribute)
                        continue;
                }

                if(attr.Expressions == null || attr.Expressions.Count < 1)
                    continue;

                Literal argLit = attr.Expressions[0] as Literal;

                if(argLit == null)
                    continue;

                string friendName = argLit.Value as string;

                if(friendName == null)
                    continue;

                result.Add(new AssemblyReference(friendName));
            }

            return result;
        }
        /// <summary>
        /// The attributes associated with this module. This corresponds to C# custom attributes with the module target specifier.
        /// </summary>
        public virtual AttributeList ModuleAttributes
        {
            get
            {
                if(this.moduleAttributes != null)
                    return this.moduleAttributes;
                if(this.provideCustomAttributes != null)
                {
                    lock(Module.GlobalLock)
                    {
                        if(this.moduleAttributes == null)
                            this.provideCustomAttributes(this);
                    }
                }
                else
                    this.moduleAttributes = new AttributeList();
                return this.moduleAttributes;
            }
            set
            {
                this.moduleAttributes = value;
            }
        }
        protected AttributeList moduleAttributes;

        protected byte[] token;
        public virtual byte[] PublicKeyToken
        {
            get
            {
                if(this.token != null)
                    return this.token;
                if(this.PublicKeyOrToken == null || this.PublicKeyOrToken.Length == 0)
                    return null;
                if(this.PublicKeyOrToken.Length == 8)
                    return this.token = this.PublicKeyOrToken;

                System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                byte[] hashedKey = sha1.ComputeHash(this.PublicKeyOrToken);
                byte[] token = new byte[8];
                for(int i = 0, n = hashedKey.Length - 1; i < 8; i++)
                    token[i] = hashedKey[n - i];
                return this.token = token;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class AssemblyReference : Node
    {
        private byte[] token;
        internal Reader Reader;

        public AssemblyReference() : base(NodeType.AssemblyReference)
        {
        }

        public AssemblyReference(AssemblyNode/*!*/ assembly)
            : base(NodeType.AssemblyReference)
        {
            this.culture = assembly.Culture;
            this.flags = assembly.Flags & ~AssemblyFlags.PublicKey;
            this.hashValue = assembly.HashValue;
            this.name = assembly.Name;
            this.publicKeyOrToken = assembly.PublicKeyOrToken;
            if(assembly.PublicKeyOrToken != null && assembly.PublicKeyOrToken.Length > 8)
                this.flags |= AssemblyFlags.PublicKey;
            this.location = assembly.Location;
            this.version = assembly.Version;
            this.assembly = assembly;
        }

        public AssemblyReference(string assemblyStrongName, SourceContext sctx)
            : this(assemblyStrongName)
        {
            this.SourceContext = sctx;
        }

        public AssemblyReference(string assemblyStrongName)
            : base(NodeType.AssemblyReference)
        {
            AssemblyFlags flags = AssemblyFlags.None;
            if(assemblyStrongName == null) { Debug.Assert(false); assemblyStrongName = ""; }
            int i = 0, n = assemblyStrongName.Length;
            string name = ParseToken(assemblyStrongName, ref i);
            string version = null;
            string culture = null;
            string token = null;
            while(i < n)
            {
                if(assemblyStrongName[i] != ',')
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                        ExceptionStrings.InvalidAssemblyStrongName, assemblyStrongName), "assemblyStrongName");
                i++;
                while(i < n && char.IsWhiteSpace(assemblyStrongName[i]))
                    i++;
                switch(assemblyStrongName[i])
                {
                    case 'v':
                    case 'V':
                        version = ParseAssignment(assemblyStrongName, "Version", ref i);
                        break;
                    case 'c':
                    case 'C':
                        culture = ParseAssignment(assemblyStrongName, "Culture", ref i);
                        break;
                    case 'p':
                    case 'P':
                        if(PlatformHelpers.StringCompareOrdinalIgnoreCase(assemblyStrongName, i, "PublicKeyToken", 0, "PublicKeyToken".Length) == 0)
                            token = ParseAssignment(assemblyStrongName, "PublicKeyToken", ref i);
                        else
                        {
                            token = ParseAssignment(assemblyStrongName, "PublicKey", ref i);
                            flags |= AssemblyFlags.PublicKey;
                        }
                        break;
                    case 'r':
                    case 'R':
                        string yesOrNo = ParseAssignment(assemblyStrongName, "Retargetable", ref i);
                        if(PlatformHelpers.StringCompareOrdinalIgnoreCase(yesOrNo, "Yes") == 0)
                            flags |= AssemblyFlags.Retargetable;
                        break;
                }
                while(i < n && assemblyStrongName[i] == ']')
                    i++;
            }
            while(i < n && char.IsWhiteSpace(assemblyStrongName[i]))
                i++;
            if(i < n)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                    ExceptionStrings.InvalidAssemblyStrongName, assemblyStrongName), "assemblyStrongName");
            if(PlatformHelpers.StringCompareOrdinalIgnoreCase(culture, "neutral") == 0)
                culture = null;
            if(PlatformHelpers.StringCompareOrdinalIgnoreCase(token, "null") == 0)
                token = null;
            byte[] tok = null;
            if(token != null && (n = token.Length) > 0)
            {
                if(n > 16)
                {
                    ArrayList tokArr = new ArrayList();
                    if(n % 2 == 1)
                    {
                        tokArr.Add(byte.Parse(token.Substring(0, 1), System.Globalization.NumberStyles.HexNumber, null));
                        n--;
                    }
                    for(i = 0; i < n; i += 2)
                    {
                        byte b = 0;
                        bool goodByte = byte.TryParse(token.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out b);
                        Debug.Assert(goodByte);
                        tokArr.Add(b);
                    }
                    tok = (byte[])tokArr.ToArray(typeof(byte));
                }
                else
                {
                    ulong tk = ulong.Parse(token, System.Globalization.NumberStyles.HexNumber, null);
                    tok = new byte[8];
                    tok[0] = (byte)(tk >> 56);
                    tok[1] = (byte)(tk >> 48);
                    tok[2] = (byte)(tk >> 40);
                    tok[3] = (byte)(tk >> 32);
                    tok[4] = (byte)(tk >> 24);
                    tok[5] = (byte)(tk >> 16);
                    tok[6] = (byte)(tk >> 8);
                    tok[7] = (byte)tk;
                }
            }
            this.culture = culture;
            this.name = name;
            this.publicKeyOrToken = tok;
            this.version = version == null || version.Length == 0 ? null : new Version(version);
            this.flags = flags;
        }
        private static string ParseToken(string/*!*/ assemblyStrongName, ref int i)
        {
            Debug.Assert(assemblyStrongName != null);
            int n = assemblyStrongName.Length;
            Debug.Assert(0 <= i && i < n);
            while(i < n && char.IsWhiteSpace(assemblyStrongName[i]))
                i++;
            StringBuilder sb = new StringBuilder(n);
            while(i < n)
            {
                char ch = assemblyStrongName[i];
                if(ch == ',' || ch == ']' || char.IsWhiteSpace(ch))
                    break;
                sb.Append(ch);
                i++;
            }
            while(i < n && char.IsWhiteSpace(assemblyStrongName[i]))
                i++;
            return sb.ToString();
        }
        private static string ParseAssignment(string/*!*/ assemblyStrongName, string/*!*/ target, ref int i)
        {
            Debug.Assert(assemblyStrongName != null && target != null);
            int n = assemblyStrongName.Length;
            Debug.Assert(0 < i && i < n);
            if(PlatformHelpers.StringCompareOrdinalIgnoreCase(assemblyStrongName, i, target, 0, target.Length) != 0)
                goto throwError;
            i += target.Length;
            while(i < n && char.IsWhiteSpace(assemblyStrongName[i]))
                i++;
            if(i >= n || assemblyStrongName[i] != '=')
                goto throwError;
            i++;
            if(i >= n)
                goto throwError;
            return ParseToken(assemblyStrongName, ref i);
throwError:
            throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
              ExceptionStrings.InvalidAssemblyStrongName, assemblyStrongName), "assemblyStrongName");
        }
        private string culture;
        public string Culture
        {
            get { return this.culture; }
            set { this.culture = value; }
        }
        private AssemblyFlags flags;
        public AssemblyFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }
        private byte[] hashValue;
        public byte[] HashValue
        {
            get { return this.hashValue; }
            set { this.hashValue = value; }
        }
        private string name;
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        private byte[] publicKeyOrToken;
        public byte[] PublicKeyOrToken
        {
            get { return this.publicKeyOrToken; }
            set { this.publicKeyOrToken = value; }
        }
        private System.Version version;
        public System.Version Version
        {
            get { return this.version; }
            set { this.version = value; }
        }
        private string location;
        public string Location
        {
            get { return this.location; }
            set { this.location = value; }
        }
        protected internal AssemblyNode assembly;
        public virtual AssemblyNode Assembly
        {
            get
            {
                if(this.assembly != null)
                    return this.assembly;
                if(this.Reader != null)
                    return this.assembly = this.Reader.GetAssemblyFromReference(this);
                return null;
            }
            set
            {
                this.assembly = value;
            }
        }
        protected string strongName;
        public virtual string StrongName
        {
            get
            {
                if(this.strongName == null)
                    this.strongName = AssemblyNode.GetStrongName(this.Name, this.Version, this.Culture, this.PublicKeyOrToken, (this.Flags & AssemblyFlags.Retargetable) != 0);
                return this.strongName;
            }
        }
        private System.Reflection.AssemblyName assemblyName;
        public System.Reflection.AssemblyName GetAssemblyName()
        {
            if(this.assemblyName == null)
            {
                System.Reflection.AssemblyName aName = new System.Reflection.AssemblyName();
                aName.CultureInfo = new System.Globalization.CultureInfo(this.Culture == null ? "" : this.Culture);
                if(this.PublicKeyOrToken != null && this.PublicKeyOrToken.Length > 8)
                    aName.Flags = System.Reflection.AssemblyNameFlags.PublicKey;
                if((this.Flags & AssemblyFlags.Retargetable) != 0)
                    aName.Flags |= (System.Reflection.AssemblyNameFlags)AssemblyFlags.Retargetable;
                aName.HashAlgorithm = System.Configuration.Assemblies.AssemblyHashAlgorithm.SHA1;
                if(this.PublicKeyOrToken != null)
                {
                    if(this.PublicKeyOrToken.Length > 8)
                        aName.SetPublicKey(this.PublicKeyOrToken);
                    else if(this.PublicKeyOrToken.Length > 0)
                        aName.SetPublicKeyToken(this.PublicKeyOrToken);
                }
                else
                    aName.SetPublicKey(new byte[0]);
                aName.Name = this.Name;
                aName.Version = this.Version;
                switch(this.Flags & AssemblyFlags.CompatibilityMask)
                {
                    case AssemblyFlags.NonSideBySideCompatible:
                        aName.VersionCompatibility = System.Configuration.Assemblies.AssemblyVersionCompatibility.SameDomain;
                        break;
                    case AssemblyFlags.NonSideBySideProcess:
                        aName.VersionCompatibility = System.Configuration.Assemblies.AssemblyVersionCompatibility.SameProcess;
                        break;
                    case AssemblyFlags.NonSideBySideMachine:
                        aName.VersionCompatibility = System.Configuration.Assemblies.AssemblyVersionCompatibility.SameMachine;
                        break;
                }
                this.assemblyName = aName;
            }
            return this.assemblyName;
        }
        public bool Matches(string name, Version version, string culture, byte[] publicKeyToken)
        {
            if(culture != null && culture.Length == 0)
                culture = null;
            if(this.Culture != null && this.Culture.Length == 0)
                this.Culture = null;
            if(this.Version != version && this.Version != null && (version == null || !this.Version.Equals(version)))
                return false;
            if(PlatformHelpers.StringCompareOrdinalIgnoreCase(this.Name, name) != 0 ||
              PlatformHelpers.StringCompareOrdinalIgnoreCase(this.Culture, culture) != 0)
                return false;
            if((this.Flags & AssemblyFlags.Retargetable) != 0)
                return true;
            byte[] thisToken = this.PublicKeyToken;
            if(publicKeyToken == null)
                return thisToken == null;
            if(thisToken == publicKeyToken)
                return true;
            if(thisToken == null)
                return false;
            int n = publicKeyToken.Length;
            if(n != thisToken.Length)
                return false;
            for(int i = 0; i < n; i++) if(thisToken[i] != publicKeyToken[i])
                    return false;
            return true;
        }
        public bool MatchesIgnoringVersion(AssemblyReference reference)
        {
            if(reference == null)
                return false;
            return this.Matches(reference.Name, this.Version, reference.Culture, reference.PublicKeyToken);
        }
        public byte[] PublicKeyToken
        {
            get
            {
                if(this.token != null)
                    return this.token;
                if(this.PublicKeyOrToken == null || this.PublicKeyOrToken.Length == 0)
                    return null;
                if(this.PublicKeyOrToken.Length == 8)
                    return this.token = this.PublicKeyOrToken;

                System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                byte[] hashedKey = sha.ComputeHash(this.PublicKeyOrToken);
                byte[] token = new byte[8];
                for(int i = 0, n = hashedKey.Length - 1; i < 8; i++)
                    token[i] = hashedKey[n - i];
                return this.token = token;
            }
        }
    }
    public class ModuleReference : Node
    {
        private Module module;
        private string name;
        public ModuleReference()
            : base(NodeType.ModuleReference)
        {
        }
        public ModuleReference(string name, Module module)
            : base(NodeType.ModuleReference)
        {
            this.name = name;
            this.module = module;
        }
        public Module Module
        {
            get { return this.module; }
            set { this.module = value; }
        }
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }
    /// <summary>
    /// A member of a Namespace or a TypeNode
    /// </summary>
    public abstract class Member : Node
    {
        /// <summary>The namespace of which this node is a member. Null if this node is a member of type.</summary>
        public Namespace DeclaringNamespace;
        /// <summary>
        /// Indicates that the signature of this member may include unsafe types such as pointers. For methods and properties, it also indicates that the
        /// code may contain unsafe constructions such as pointer arithmetic.
        /// </summary>
        public bool IsUnsafe;
        /// <summary>A list of other nodes that refer to this member. Must be filled in by client code.</summary>
        public NodeList References;

        protected Member(NodeType nodeType)
            : base(nodeType)
        {
        }
        protected Member(TypeNode declaringType, AttributeList attributes, Identifier name, NodeType nodeType)
            : base(nodeType)
        {
            this.attributes = attributes;
            this.declaringType = declaringType;
            this.name = name;
        }
        private TypeNode declaringType;
        /// <summary>The type of which this node is a member. Null if this node is a member of a Namespace.</summary>
        public TypeNode DeclaringType
        {
            get { return this.declaringType; }
            set { this.declaringType = value; }
        }
        private Identifier name;
        /// <summary>The unqualified name of the member.</summary>
        public Identifier Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        protected AttributeList attributes;
        private bool notObsolete;
        private ObsoleteAttribute obsoleteAttribute;

        /// <summary>
        /// The attributes of this member. Corresponds to custom attribute annotations in C#.
        /// </summary>
        public virtual AttributeList Attributes
        {
            get
            {
                if(this.attributes != null)
                    return this.attributes;
                return this.attributes = new AttributeList();
            }
            set
            {
                this.attributes = value;
            }
        }
        protected Member hiddenMember;
        public virtual Member HiddenMember
        {
            get
            {
                return this.hiddenMember;
            }
            set
            {
                this.hiddenMember = value;
            }
        }
        protected bool hidesBaseClassMemberSpecifiedExplicitly;
        protected bool hidesBaseClassMember;
        /// <summary>Indicates if this is a member of a subclass that intentionally has the same signature as a member of a base class. Corresponds to the "new" modifier in C#.</summary>
        public bool HidesBaseClassMember
        {
            get
            {
                if(this.hidesBaseClassMemberSpecifiedExplicitly)
                    return this.hidesBaseClassMember;
                else
                    return this.HiddenMember != null;
            }
            set
            {
                this.hidesBaseClassMember = value;
                this.hidesBaseClassMemberSpecifiedExplicitly = true;
            }
        }
        protected Member overriddenMember;
        public virtual Member OverriddenMember
        {
            get
            {
                return this.overriddenMember;
            }
            set
            {
                this.overriddenMember = value;
            }
        }
        protected bool overridesBaseClassMemberSpecifiedExplicitly;
        protected bool overridesBaseClassMember;
        /// <summary>Indicates if this is a virtual method of a subclass that intentionally overrides a method of a base class. Corresponds to the "override" modifier in C#.</summary>
        public virtual bool OverridesBaseClassMember
        {
            get
            {
                if(this.overridesBaseClassMemberSpecifiedExplicitly)
                    return this.overridesBaseClassMember;
                else
                    return this.OverriddenMember != null;
            }
            set
            {
                this.overridesBaseClassMember = value;
                this.overridesBaseClassMemberSpecifiedExplicitly = true;
            }
        }
        /// <summary>
        /// Gets the first attribute of the given type in the attribute list of this member. Returns null if none found.
        /// This should not be called until the AST containing this member has been processed to replace symbolic references
        /// to members with references to the actual members.
        /// </summary>
        public virtual AttributeNode GetAttribute(TypeNode attributeType)
        {
            if(attributeType == null)
                return null;
            AttributeList attributes = this.Attributes;
            for(int i = 0, n = attributes == null ? 0 : attributes.Count; i < n; i++)
            {
                AttributeNode attr = attributes[i];
                if(attr == null)
                    continue;
                MemberBinding mb = attr.Constructor as MemberBinding;
                if(mb != null)
                {
                    if(mb.BoundMember == null)
                        continue;
                    if(mb.BoundMember.DeclaringType != attributeType)
                        continue;
                    return attr;
                }
                Literal lit = attr.Constructor as Literal;
                if(lit == null)
                    continue;
                if((lit.Value as TypeNode) != attributeType)
                    continue;
                return attr;
            }
            return null;
        }
        public virtual AttributeList GetFilteredAttributes(TypeNode attributeType)
        {
            if(attributeType == null)
                return this.Attributes;
            AttributeList attributes = this.Attributes;
            AttributeList filtered = new AttributeList();
            for(int i = 0, n = attributes == null ? 0 : attributes.Count; i < n; i++)
            {
                AttributeNode attr = attributes[i];
                if(attr == null)
                    continue;
                MemberBinding mb = attr.Constructor as MemberBinding;
                if(mb != null)
                {
                    if(mb.BoundMember != null && mb.BoundMember.DeclaringType == attributeType)
                        continue;
                    filtered.Add(attr);
                    continue;
                }
                Literal lit = attr.Constructor as Literal;
                if(lit != null && (lit.Value as TypeNode) == attributeType)
                    continue;
                filtered.Add(attr);
            }
            return filtered;
        }

        /// <summary>
        /// The concatenation of the FullName of the containing member and the name of this member. 
        /// Separated with a '.' character if the containing member is a namespace and a '+' character if the containing member is a Type.
        /// Includes the parameter type full names when this member is a method or a property. Also includes (generic) template arguments.
        /// </summary>
        public abstract string/*!*/ FullName { get; }
        /// <summary>True if all references to this member must be from the assembly containing the definition of this member. </summary>
        public abstract bool IsAssembly { get; }
        /// <summary>
        /// True if access to this member is controlled by the compiler and not the runtime. Cannot be accessed from other assemblies since these
        /// are not necessarily controlled by the same compiler.
        /// </summary>
        public abstract bool IsCompilerControlled { get; }
        /// <summary>True if this member can only be accessed from subclasses of the class declaring this member.</summary>
        public abstract bool IsFamily { get; }
        /// <summary>True if this member can only be accessed from subclasses of the class declaring this member, provided that these subclasses are also
        /// contained in the assembly containing this member.</summary>
        public abstract bool IsFamilyAndAssembly { get; }
        /// <summary>True if all references to this member must either be from the assembly containing the definition of this member,
        /// or from a subclass of the class declaring this member.</summary>
        public abstract bool IsFamilyOrAssembly { get; }
        /// <summary>True if all references to this member must be from members of the type declaring this member./// </summary>
        public abstract bool IsPrivate { get; }
        /// <summary>True if the member can be accessed from anywhere./// </summary>
        public abstract bool IsPublic { get; }
        /// <summary>True if the name of this member conforms to a naming pattern with special meaning. For example the name of a property getter.</summary>
        public abstract bool IsSpecialName { get; }
        /// <summary>True if this member always has the same value or behavior for all instances the declaring type.</summary>
        public abstract bool IsStatic { get; }
        /// <summary>True if another assembly can contain a reference to this member.</summary>
        public abstract bool IsVisibleOutsideAssembly { get; }
        /// <summary>A cached reference to the first Obsolete attribute of this member. Null if no such attribute exists.</summary>
        public ObsoleteAttribute ObsoleteAttribute
        {
            get
            {
                if(this.notObsolete)
                    return null;
                if(this.obsoleteAttribute == null)
                {
                    AttributeNode attr = this.GetAttribute(SystemTypes.ObsoleteAttribute);
                    if(attr != null)
                    {
                        ExpressionList args = attr.Expressions;
                        int numArgs = args == null ? 0 : args.Count;
                        Literal lit0 = numArgs > 0 ? args[0] as Literal : null;
                        Literal lit1 = numArgs > 1 ? args[1] as Literal : null;
                        string message = lit0 != null ? lit0.Value as string : null;
                        object isError = lit1 != null ? lit1.Value : null;
                        if(isError is bool)
                            return this.obsoleteAttribute = new ObsoleteAttribute(message, (bool)isError);
                        else
                            return this.obsoleteAttribute = new ObsoleteAttribute(message);
                    }
                    this.notObsolete = true;
                }
                return this.obsoleteAttribute;
            }
            set
            {
                this.obsoleteAttribute = value;
                this.notObsolete = false;
            }
        }

        /// <summary>The source code, if any, corresponding to the value in Documentation.</summary>
        public Node DocumentationNode;

        protected XmlNode documentation;
        /// <summary>The body of an XML element containing a description of this member. Used to associated documentation (such as this comment) with members.
        /// The fragment usually conforms to the structure defined in the C# standard.</summary>
        public virtual XmlNode Documentation
        {
            get
            {
                XmlNode documentation = this.documentation;
                if(documentation != null)
                    return documentation;
                TypeNode t = this.DeclaringType;
                if(t == null)
                    t = this as TypeNode;
                Module m = t == null ? null : t.DeclaringModule;
                TrivialHashtable cache = m == null ? null : m.GetMemberDocumentationCache();
                if(cache == null)
                    return null;
                return this.documentation = (XmlNode)cache[this.DocumentationId.UniqueIdKey];
            }
            set
            {
                this.documentation = value;
            }
        }
        protected Identifier documentationId;
        protected virtual Identifier GetDocumentationId()
        {
            return Identifier.Empty;
        }
        /// <summary>
        /// The value of the name attribute of the XML element whose body is the XML fragment returned by Documentation.
        /// </summary>
        public Identifier DocumentationId
        {
            get
            {
                Identifier documentationId = this.documentationId;
                if(documentationId != null)
                    return documentationId;
                return this.DocumentationId = this.GetDocumentationId();
            }
            set
            {
                this.documentationId = value;
            }
        }
        protected string helpText;
        /// <summary>
        /// The value of the summary child element of the XML fragment returned by Documentation. All markup is stripped from the value.
        /// </summary>
        public virtual string HelpText
        {
            get
            {
                string helpText = this.helpText;
                if(helpText != null)
                    return helpText;
                XmlNode documentation = this.Documentation;
                if(documentation != null && documentation.HasChildNodes)
                {
                    //^ assume documentation.ChildNodes != null;
                    foreach(XmlNode child in documentation.ChildNodes)
                        if(child.Name == "summary")
                            return this.helpText = this.GetHelpText(child);
                }
                return this.helpText = "";
            }
            set
            {
                this.helpText = value;
            }
        }
        public virtual string GetParameterHelpText(string parameterName)
        {
            XmlNode documentation = this.Documentation;
            if(documentation == null || documentation.ChildNodes == null)
                return null;
            foreach(XmlNode cdoc in documentation.ChildNodes)
            {
                if(cdoc == null)
                    continue;
                if(cdoc.Name != "param")
                    continue;
                if(cdoc.Attributes == null)
                    continue;
                foreach(XmlAttribute attr in cdoc.Attributes)
                {
                    if(attr == null || attr.Name != "name" || attr.Value != parameterName)
                        continue;
                    if(!cdoc.HasChildNodes)
                        continue;
                    return this.GetHelpText(cdoc);
                }
            }
            return null;
        }
        private string GetHelpText(XmlNode node)
        {
            if(node == null)
                return "";
            StringBuilder sb = new StringBuilder();
            if(node.HasChildNodes)
            {
                foreach(XmlNode child in node.ChildNodes)
                {
                    switch(child.NodeType)
                    {
                        case XmlNodeType.Element:
                            string str = this.GetHelpText(child);
                            if(str == null || str.Length == 0)
                                continue;
                            if(sb.Length > 0 && !Char.IsPunctuation(str[0]))
                                sb.Append(' ');
                            sb.Append(str);
                            break;
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Entity:
                        case XmlNodeType.Text:
                            this.AppendValue(sb, child);
                            break;
                    }
                }
            }
            else if(node.Attributes != null)
            {
                foreach(XmlAttribute attr in node.Attributes)
                {
                    this.AppendValue(sb, attr);
                }
            }
            return sb.ToString();
        }
        private int filterPriority;
        public virtual System.ComponentModel.EditorBrowsableState FilterPriority
        {
            get
            {
                if(this.filterPriority > 0)
                    return (System.ComponentModel.EditorBrowsableState)(this.filterPriority - 1);
                int prio = 0;
                XmlNode documentation = this.Documentation;
                if(documentation != null && documentation.HasChildNodes)
                {
                    foreach(XmlNode child in documentation.ChildNodes)
                        if(child.Name == "filterpriority")
                        {
                            PlatformHelpers.TryParseInt32(child.InnerText, out prio);
                            switch(prio)
                            {
                                case 2:
                                    this.filterPriority = (int)System.ComponentModel.EditorBrowsableState.Advanced;
                                    break;
                                case 3:
                                    this.filterPriority = (int)System.ComponentModel.EditorBrowsableState.Never;
                                    break;
                                default:
                                    this.filterPriority = (int)System.ComponentModel.EditorBrowsableState.Always;
                                    break;
                            }
                            this.filterPriority++;
                            return (System.ComponentModel.EditorBrowsableState)(this.filterPriority - 1);
                        }
                }
                AttributeList attributes = this.Attributes;
                for(int i = 0, n = attributes == null ? 0 : attributes.Count; i < n; i++)
                {
                    //^ assert attributes != null;
                    AttributeNode attr = attributes[i];
                    if(attr == null || attr.Type == null)
                        continue;
                    if(attr.Expressions == null || attr.Expressions.Count < 1)
                        continue;
                    if(attr.Type.FullName != "System.ComponentModel.EditorBrowsableAttribute")
                        continue;
                    Literal lit = attr.Expressions[0] as Literal;
                    if(lit == null || !(lit.Value is int))
                        continue;
                    //^ assert lit.Value != null;
                    prio = (int)lit.Value;
                    return (System.ComponentModel.EditorBrowsableState)((this.filterPriority = prio + 1) - 1);
                }
                return (System.ComponentModel.EditorBrowsableState)((this.filterPriority = 1) - 1);
            }
            set
            {
                this.filterPriority = ((int)value) + 1;
            }
        }
        /// <summary>
        /// Writes out an element with tag "element", name attribute DocumentationId.ToString() and body Documentation using the provided XmlTextWriter instance.
        /// </summary>
        public virtual void WriteDocumentation(XmlTextWriter xwriter)
        {
            if(this.documentation == null || xwriter == null)
                return;
            xwriter.WriteStartElement("member");
            if(this.DocumentationId == null)
                return;
            xwriter.WriteAttributeString("name", this.DocumentationId.ToString());
            this.documentation.WriteContentTo(xwriter);
            xwriter.WriteEndElement();
        }
        private readonly static char[]/*!*/ tags = { 'E', 'F', 'M', 'P', 'T' };
        private void AppendValue(StringBuilder/*!*/ sb, XmlNode/*!*/ node)
        {
            string str = node.Value;
            if(str != null)
            {
                str = str.Trim();
                if(str.Length > 2 && str[1] == ':' && str.LastIndexOfAny(tags, 0, 1) == 0)
                {
                    char tag = str[0];
                    str = str.Substring(2);
                    if(tag == 'T' && str.IndexOf(TargetPlatform.GenericTypeNamesMangleChar) >= 0)
                    {
                        Module mod = null;
                        if(this.DeclaringType != null)
                            mod = this.DeclaringType.DeclaringModule;
                        else if(this is TypeNode)
                            mod = ((TypeNode)this).DeclaringModule;
                        if(mod != null)
                        {
                            Identifier ns;
                            Identifier tn;
                            int i = str.LastIndexOf('.');
                            if(i < 0 || i >= str.Length)
                            {
                                ns = Identifier.Empty;
                                tn = Identifier.For(str);
                            }
                            else
                            {
                                ns = Identifier.For(str.Substring(0, i));
                                tn = Identifier.For(str.Substring(i + 1));
                            }
                            TypeNode t = mod.GetType(ns, tn, true);
                            if(t != null)
                                str = t.GetFullUnmangledNameWithTypeParameters();
                        }
                    }
                }
                if(str == null || str.Length == 0)
                    return;
                bool lastCharWasSpace = false;
                if(sb.Length > 0 && !Char.IsPunctuation(str[0]) && !Char.IsWhiteSpace(str[0]))
                {
                    sb.Append(' ');
                    lastCharWasSpace = true;
                }
                foreach(char ch in str)
                {
                    if(Char.IsWhiteSpace(ch))
                    {
                        if(lastCharWasSpace)
                            continue;
                        lastCharWasSpace = true;
                        sb.Append(' ');
                    }
                    else
                    {
                        lastCharWasSpace = false;
                        sb.Append(ch);
                    }
                }
                if(sb.Length > 0 && Char.IsWhiteSpace(sb[sb.Length - 1]))
                    sb.Length -= 1;
            }
        }
    }

    public class TypeMemberSnippet : Member
    {
        public IParserFactory ParserFactory;

        public TypeMemberSnippet()
            : base(NodeType.TypeMemberSnippet)
        {
        }
        public TypeMemberSnippet(IParserFactory parserFactory, SourceContext sctx)
            : base(NodeType.TypeMemberSnippet)
        {
            this.ParserFactory = parserFactory;
            this.SourceContext = sctx;
        }
        public override string/*!*/ FullName
        {
            get { throw new InvalidOperationException(); }
        }
        public override bool IsCompilerControlled
        {
            get { throw new InvalidOperationException(); }
        }
        public override bool IsAssembly
        {
            get { throw new InvalidOperationException(); }
        }
        public override bool IsFamily
        {
            get { throw new InvalidOperationException(); }
        }
        public override bool IsFamilyAndAssembly
        {
            get { throw new InvalidOperationException(); }
        }
        public override bool IsFamilyOrAssembly
        {
            get { throw new InvalidOperationException(); }
        }
        public override bool IsPrivate
        {
            get { throw new InvalidOperationException(); }
        }
        public override bool IsPublic
        {
            get { throw new InvalidOperationException(); }
        }
        public override bool IsSpecialName
        {
            get { throw new InvalidOperationException(); }
        }
        public override bool IsStatic
        {
            get { throw new InvalidOperationException(); }
        }
        public override bool IsVisibleOutsideAssembly
        {
            get { throw new InvalidOperationException(); }
        }

    }

    /// <summary>
    /// The common base class for all types. This type should not be extended directly.
    /// Instead extend one of the standard subclasses such as Class, Struct or Interface, since in
    /// the CLR a type has to be an instance of one the subclasses, and a type which does not extend
    /// one of these types will have no equivalent in the CLR.
    /// </summary>
    public abstract class TypeNode : Member
    {
        private int classSize;
        /// <summary>Specifies the total size in bytes of instances of types with prescribed layout.</summary>
        public int ClassSize
        {
            get { return this.classSize; }
            set { this.classSize = value; }
        }
        private Module declaringModule;
        /// <summary>The module or assembly to which the compiled type belongs.</summary>
        public Module DeclaringModule
        {
            get { return this.declaringModule; }
            set { this.declaringModule = value; }
        }
        private TypeFlags flags;
        public TypeFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }
        /// <summary>The interfaces implemented by this class or struct, or the extended by this interface.</summary>
        public virtual InterfaceList Interfaces
        {
            get { return this.interfaces == null ? new InterfaceList() : this.interfaces; }
            set { this.interfaces = value; }
        }
        protected InterfaceList interfaces;

        public InterfaceList InterfaceExpressions;

        private Identifier @namespace;
        /// <summary>The namespace to which this type belongs. Null if the type is nested inside another type.</summary>
        public Identifier Namespace
        {
            get { return this.@namespace; }
            set { this.@namespace = value; }
        }
        private int packingSize;
        /// <summary>Specifies the alignment of fields within types with prescribed layout.</summary>
        public int PackingSize
        {
            get { return this.packingSize; }
            set { this.packingSize = value; }
        }

        /// <summary>If this type is the combined result of a number of partial type definitions, this lists the partial definitions.</summary>
        public TypeNodeList IsDefinedBy;

        /// <summary>
        /// True if this type is the result of a template instantiation with arguments that are themselves template parameters. 
        /// Used to model template instantiations occurring inside templates.
        /// </summary>
        public bool IsNotFullySpecialized;
        public bool NewTemplateInstanceIsRecursive;

        /// <summary>
        /// If this type is a partial definition, the value of this is the combined type resulting from all the partial definitions.
        /// </summary>
        public TypeNode PartiallyDefines;
        /// <summary>
        /// The list of extensions of this type, if it's a non-extension type.
        /// all extensions implement the IExtendTypeNode interface (in the Sing# code base).
        /// null = empty list
        /// </summary>
        private TypeNodeList extensions = null;
        /// <summary>
        /// Whether or not the list of extensions has been examined;
        /// it's a bug to record a new extension after extensions have been examined.
        /// </summary>
        private bool extensionsExamined = false;
        /// <summary>
        /// Record another extension of this type.
        /// </summary>
        /// <param name="extension"></param>
        public void RecordExtension(TypeNode extension)
        {
            Debug.Assert(!extensionsExamined, "adding an extension after they've already been examined");
            if(this.extensions == null)
                this.extensions = new TypeNodeList();
            this.extensions.Add(extension);
        }
        /// <summary>
        /// The property that should be accessed by clients to get the list of extensions of this type.
        /// </summary>
        public TypeNodeList Extensions
        {
            get
            {
                this.extensionsExamined = true;
                return this.extensions;
            }
            set
            {
                Debug.Assert(!extensionsExamined, "setting extensions after they've already been examined");
                this.extensions = value;
            }
        }
        /// <summary>
        /// When duplicating a type node, we want to transfer the extensions and the extensionsExamined flag without 
        /// treating this as a "touch" that sets the examined flag.  Pretty ugly, though.
        /// </summary>
        public TypeNodeList ExtensionsNoTouch
        {
            get { return this.extensions; }
        }
        /// <summary>
        /// Copy a (possibly transformed) set of extensions from source to the
        /// receiver, including whether or not the extensions have been examined.
        /// </summary>
        public void DuplicateExtensions(TypeNode source, TypeNodeList newExtensions)
        {
            if(source == null)
                return;
            this.extensions = newExtensions;
            this.extensionsExamined = source.extensionsExamined;
        }
        /// <summary>
        /// If the receiver is a type extension, return the extendee, otherwise return the receiver.
        /// [The identity function, except for dialects (e.g. Extensible Sing#) that allow
        /// extensions and differing views of types]
        /// </summary>
        public virtual TypeNode/*!*/ EffectiveTypeNode
        {
            get
            {
                return this;
            }
        }
        /// <summary>
        /// Return whether t1 represents the same type as t2 (or both are null).
        /// This copes with the cases where t1 and/or t2 may be type views and/or type extensions, as
        /// in Extensible Sing#.
        /// </summary>
        public static bool operator ==(TypeNode t1, TypeNode t2)
        {
            return
              (object)t1 == null ?
                (object)t2 == null :
                (object)t2 != null && (object)t1.EffectiveTypeNode == (object)t2.EffectiveTypeNode;
        }
        // modify the other operations related to equality
        public static bool operator !=(TypeNode t1, TypeNode t2)
        {
            return !(t1 == t2);
        }
        public override bool Equals(Object other)
        {
            return this == (other as TypeNode);
        }
        public override int GetHashCode()
        {
            TypeNode tn = this.EffectiveTypeNode;
            if((object)tn == (object)this)
            {
                return base.GetHashCode();
            }
            else
            {
                return tn.GetHashCode();
            }
        }

        /// <summary>
        /// A delegate that is called the first time Members is accessed, if non-null.
        /// Provides for incremental construction of the type node.
        /// Must not leave Members null.
        /// </summary>
        public TypeMemberProvider ProvideTypeMembers;
        /// <summary>
        /// The type of delegates that fill in the Members property of the given type.
        /// </summary>
        public delegate void TypeMemberProvider(TypeNode/*!*/ type, object/*!*/ handle);
        /// <summary>
        /// A delegate that is called the first time NestedTypes is accessed, if non-null.
        /// </summary>
        public NestedTypeProvider ProvideNestedTypes;
        /// <summary>
        /// The type of delegates that fill in the NestedTypes property of the given type.
        /// </summary>
        public delegate void NestedTypeProvider(TypeNode/*!*/ type, object/*!*/ handle);
        /// <summary>
        /// A delegate that is called the first time Attributes is accessed, if non-null.
        /// Provides for incremental construction of the type node.
        /// Must not leave Attributes null.
        /// </summary>
        public TypeAttributeProvider ProvideTypeAttributes;
        /// <summary>
        /// The type of delegates that fill in the Attributes property of the given type.
        /// </summary>
        public delegate void TypeAttributeProvider(TypeNode/*!*/ type, object/*!*/ handle);
        /// <summary>
        /// Opaque information passed as a parameter to the delegates in ProvideTypeMembers et al.
        /// Typically used to associate this namespace instance with a helper object.
        /// </summary>
        public object ProviderHandle;
        private TypeNodeList templateInstances;
        /// <summary>Contains all the types instantiated from this non generic template type.</summary>
        public TypeNodeList TemplateInstances
        {
            get { return this.templateInstances; }
            set { this.templateInstances = value; }
        }

        /// <summary>
        /// The element names if this is a something that has named elements such as <c>ValueTuple</c>
        /// </summary>
        internal Collections.Specialized.StringEnumerator ElementNames { get; set; }

        internal TypeNode(NodeType nodeType)
            : base(nodeType)
        {
        }

        internal TypeNode(NodeType nodeType, NestedTypeProvider provideNestedTypes, TypeAttributeProvider provideAttributes, TypeMemberProvider provideMembers, object handle)
            : base(nodeType)
        {
            this.ProvideNestedTypes = provideNestedTypes;
            this.ProvideTypeAttributes = provideAttributes;
            this.ProvideTypeMembers = provideMembers;
            this.ProviderHandle = handle;
            this.isNormalized = true;
        }

        internal TypeNode(Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags flags,
          Identifier Namespace, Identifier name, InterfaceList interfaces, MemberList members, NodeType nodeType)
            : base(null, attributes, name, nodeType)
        {
            this.DeclaringModule = declaringModule;
            this.DeclaringType = declaringType;
            this.Flags = flags;
            this.Interfaces = interfaces;
            this.members = members;
            this.Namespace = Namespace;
        }

        public override AttributeList Attributes
        {
            get
            {
                if(this.attributes == null)
                {
                    if(this.ProvideTypeAttributes != null && this.ProviderHandle != null)
                    {
                        lock(Module.GlobalLock)
                        {
                            if(this.attributes == null)
                            {
                                // !EFW - Assign an empty list first.  If not, it can get stuck in an
                                // endless loop if a type has an attribute with a type parameter that
                                // references the type being parsed.  For example:
                                // 
                                // [SomeAttribute(typeof(SomeClass<object>))]
                                // public class SomeClass<T> { }
                                //
                                this.attributes = new AttributeList();

                                this.ProvideTypeAttributes(this, this.ProviderHandle);
                            }
                        }
                    }
                    else
                        this.attributes = new AttributeList();
                }

                return this.attributes;
            }
            set
            {
                this.attributes = value;
            }
        }

        protected SecurityAttributeList securityAttributes;

        /// <summary>Contains declarative security information associated with the type.</summary>
        public SecurityAttributeList SecurityAttributes
        {
            get
            {
                if(this.securityAttributes != null)
                    return this.securityAttributes;

                if(this.attributes == null)
                {
                    // Getting the type attributes also gets the security attributes in the case of a type that
                    // was read in by the Reader
                    AttributeList al = this.Attributes;

                    if(al != null)
                        al = null;

                    if(this.securityAttributes != null)
                        return this.securityAttributes;
                }

                return this.securityAttributes = new SecurityAttributeList();
            }
            set
            {
                this.securityAttributes = value;
            }
        }

        /// <summary>The type from which this type is derived. Null in the case of interfaces and System.Object.</summary>
        public virtual TypeNode BaseType
        {
            get
            {
                switch(this.NodeType)
                {
                    case NodeType.ArrayType:
                        return CoreSystemTypes.Array;
                    case NodeType.ClassParameter:
                    case NodeType.Class:
                        return ((Class)this).BaseClass;
                    case NodeType.DelegateNode:
                        return CoreSystemTypes.MulticastDelegate;
                    case NodeType.EnumNode:
                        return CoreSystemTypes.Enum;
                    case NodeType.Struct:
                    case NodeType.TupleType:
                    case NodeType.TypeAlias:
                    case NodeType.TypeIntersection:
                    case NodeType.TypeUnion:
                        return CoreSystemTypes.ValueType;
                    default:
                        return null;
                }
            }
        }
        protected internal MemberList defaultMembers;
        /// <summary>A list of any members of this type that have the DefaultMember attribute.</summary>
        public virtual MemberList DefaultMembers
        {
            get
            {
                int n = this.Members.Count;
                if(n != this.memberCount)
                {
                    this.UpdateMemberTable(n);
                    this.defaultMembers = null;
                }
                if(this.defaultMembers == null)
                {
                    AttributeList attrs = this.Attributes;
                    Identifier defMemName = null;
                    for(int j = 0, m = attrs == null ? 0 : attrs.Count; j < m; j++)
                    {
                        //^ assert attrs != null;
                        AttributeNode attr = attrs[j];
                        if(attr == null)
                            continue;
                        MemberBinding mb = attr.Constructor as MemberBinding;
                        if(mb != null && mb.BoundMember != null && mb.BoundMember.DeclaringType == SystemTypes.DefaultMemberAttribute)
                        {
                            if(attr.Expressions != null && attr.Expressions.Count > 0)
                            {
                                Literal lit = attr.Expressions[0] as Literal;
                                if(lit != null && lit.Value is string)
                                    defMemName = Identifier.For((string)lit.Value);
                            }
                            break;
                        }
                        Literal litc = attr.Constructor as Literal;
                        if(litc != null && (litc.Value as TypeNode) == SystemTypes.DefaultMemberAttribute)
                        {
                            if(attr.Expressions != null && attr.Expressions.Count > 0)
                            {
                                Literal lit = attr.Expressions[0] as Literal;
                                if(lit != null && lit.Value is string)
                                    defMemName = Identifier.For((string)lit.Value);
                            }
                            break;
                        }
                    }

                    if(defMemName != null)
                        this.defaultMembers = this.GetMembersNamed(defMemName);
                    else
                        this.defaultMembers = new MemberList();
                }
                return this.defaultMembers;
            }
            set
            {
                this.defaultMembers = value;
            }
        }
        protected string fullName;
        public override string/*!*/ FullName
        {
            get
            {
                if(this.fullName != null)
                    return this.fullName;
                if(this.DeclaringType != null)
                    return this.fullName = this.DeclaringType.FullName + "+" + (this.Name == null ? "" : this.Name.ToString());
                else if(this.Namespace != null && this.Namespace.UniqueIdKey != Identifier.Empty.UniqueIdKey)
                    return this.fullName = this.Namespace.ToString() + "." + (this.Name == null ? "" : this.Name.ToString());
                else if(this.Name != null)
                    return this.fullName = this.Name.ToString();
                else
                    return this.fullName = "";
            }
        }

        // the same as FullName, except for dialects like Sing# with type extensions where names of
        // type extensions may get mangled; in that case, this reports the name of the effective type node.
        public virtual string FullNameDuringParsing
        {
            get { return this.FullName; }
        }

        public virtual string GetFullUnmangledNameWithoutTypeParameters()
        {
            if(this.DeclaringType != null)
                return this.DeclaringType.GetFullUnmangledNameWithoutTypeParameters() + "+" + this.GetUnmangledNameWithoutTypeParameters();
            else if(this.Namespace != null && this.Namespace.UniqueIdKey != Identifier.Empty.UniqueIdKey)
                return this.Namespace.ToString() + "." + this.GetUnmangledNameWithoutTypeParameters();
            else
                return this.GetUnmangledNameWithoutTypeParameters();
        }
        public virtual string GetFullUnmangledNameWithTypeParameters()
        {
            if(this.DeclaringType != null)
                return this.DeclaringType.GetFullUnmangledNameWithTypeParameters() + "+" + this.GetUnmangledNameWithTypeParameters(true);
            else if(this.Namespace != null && this.Namespace.UniqueIdKey != Identifier.Empty.UniqueIdKey)
                return this.Namespace.ToString() + "." + this.GetUnmangledNameWithTypeParameters(true);
            else
                return this.GetUnmangledNameWithTypeParameters(true);
        }
        public virtual string GetUnmangledNameWithTypeParameters()
        {
            return this.GetUnmangledNameWithTypeParameters(false);
        }
        private string GetUnmangledNameWithTypeParameters(bool fullNamesForTypeParameters)
        {
            StringBuilder sb = new StringBuilder(this.GetUnmangledNameWithoutTypeParameters());
            TypeNodeList templateParameters = this.TemplateParameters;
            if(this.Template != null)
                templateParameters = this.TemplateArguments;
            for(int i = 0, n = templateParameters == null ? 0 : templateParameters.Count; i < n; i++)
            {
                //^ assert templateParameters != null;
                TypeNode tpar = templateParameters[i];
                if(tpar == null)
                    continue;
                if(i == 0)
                    sb.Append('<');
                else
                    sb.Append(',');
                if(tpar.Name != null)
                    if(fullNamesForTypeParameters)
                        sb.Append(tpar.GetFullUnmangledNameWithTypeParameters());
                    else
                        sb.Append(tpar.GetUnmangledNameWithTypeParameters());
                if(i == n - 1)
                    sb.Append('>');
            }
            return sb.ToString();
        }

        public virtual string/*!*/ GetUnmangledNameWithoutTypeParameters()
        {
            char[] mangleChars = new[] { TargetPlatform.GenericTypeNamesMangleChar, '>' };

            if(this.Template != null)
                return this.Template.GetUnmangledNameWithoutTypeParameters();

            if(this.Name == null)
                return "";

            string name = this.Name.ToString();

            if(this.TemplateParameters != null && this.TemplateParameters.Count > 0)
            {
                int lastMangle = name.LastIndexOfAny(mangleChars);

                if(lastMangle >= 0)
                {
                    if(name[lastMangle] == '>')
                        lastMangle++;

                    return name.Substring(0, lastMangle);
                }
            }

            return name;
        }

        public virtual string GetSerializedTypeName()
        {
            bool isAssemblyQualified = true;
            return this.GetSerializedTypeName(this, ref isAssemblyQualified);
        }
        string GetSerializedTypeName(TypeNode/*!*/ type, ref bool isAssemblyQualified)
        {
            if(type == null)
                return null;
            StringBuilder sb = new StringBuilder();
            TypeModifier tMod = type as TypeModifier;
            if(tMod != null)
                type = tMod.ModifiedType;
            ArrayType arrType = type as ArrayType;
            if(arrType != null)
            {
                type = arrType.ElementType;
                bool isAssemQual = false;
                this.AppendSerializedTypeName(sb, arrType.ElementType, ref isAssemQual);
                if(arrType.IsSzArray())
                    sb.Append("[]");
                else
                {
                    sb.Append('[');
                    if(arrType.Rank == 1)
                        sb.Append('*');
                    for(int i = 1; i < arrType.Rank; i++)
                        sb.Append(',');
                    sb.Append(']');
                }
                goto done;
            }
            Pointer pointer = type as Pointer;
            if(pointer != null)
            {
                type = pointer.ElementType;
                bool isAssemQual = false;
                this.AppendSerializedTypeName(sb, pointer.ElementType, ref isAssemQual);
                sb.Append('*');
                goto done;
            }
            Reference reference = type as Reference;
            if(reference != null)
            {
                type = reference.ElementType;
                bool isAssemQual = false;
                this.AppendSerializedTypeName(sb, reference.ElementType, ref isAssemQual);
                sb.Append('&');
                goto done;
            }
            if(type.Template == null)
                sb.Append(type.FullName);
            else
            {
                sb.Append(type.Template.FullName);
                sb.Append('[');
                for(int i = 0, n = type.TemplateArguments == null ? 0 : type.TemplateArguments.Count; i < n; i++)
                {
                    //^ assert type.TemplateArguments != null;
                    bool isAssemQual = true;
                    this.AppendSerializedTypeName(sb, type.TemplateArguments[i], ref isAssemQual);
                    if(i < n - 1)
                        sb.Append(',');
                }
                sb.Append(']');
            }
done:
            if(isAssemblyQualified)
                this.AppendAssemblyQualifierIfNecessary(sb, type, out isAssemblyQualified);
            return sb.ToString();
        }
        void AppendAssemblyQualifierIfNecessary(StringBuilder/*!*/ sb, TypeNode type, out bool isAssemQualified)
        {
            isAssemQualified = false;
            if(type == null)
                return;
            AssemblyNode referencedAssembly = type.DeclaringModule as AssemblyNode;
            if(referencedAssembly != null)
            {
                sb.Append(", ");
                sb.Append(referencedAssembly.StrongName);
                isAssemQualified = true;
            }
        }
        void AppendSerializedTypeName(StringBuilder/*!*/ sb, TypeNode type, ref bool isAssemQualified)
        {
            if(type == null)
                return;
            string argTypeName = this.GetSerializedTypeName(type, ref isAssemQualified);
            if(isAssemQualified)
                sb.Append('[');
            sb.Append(argTypeName);
            if(isAssemQualified)
                sb.Append(']');
        }

        /// <summary>
        /// Return the name the constructor should have in this type node.  By default, it's
        /// the same as the name of the enclosing type node, but it can be different in e.g. 
        /// extensions in Extensible Sing#
        /// </summary>
        public virtual Identifier ConstructorName
        {
            get
            {
                if(this.constructorName == null)
                {
                    Identifier id = this.Name;
                    if(this.IsNormalized && this.IsGeneric)
                        id = Identifier.For(this.GetUnmangledNameWithoutTypeParameters());
                    this.constructorName = id;
                }
                return this.constructorName;
            }
        }
        private Identifier constructorName;


        /// <summary>True if the type is an abstract class or an interface.</summary>
        public virtual bool IsAbstract
        {
            get
            {
                return (this.Flags & TypeFlags.Abstract) != 0;
            }
        }
        public override bool IsAssembly
        {
            get
            {
                TypeFlags visibility = this.Flags & TypeFlags.VisibilityMask;
                return visibility == TypeFlags.NotPublic || visibility == TypeFlags.NestedAssembly;
            }
        }
        public override bool IsCompilerControlled
        {
            get { return false; }
        }
        public override bool IsFamily
        {
            get { return (this.Flags & TypeFlags.VisibilityMask) == TypeFlags.NestedFamily; }
        }
        public override bool IsFamilyAndAssembly
        {
            get { return (this.Flags & TypeFlags.VisibilityMask) == TypeFlags.NestedFamANDAssem; }
        }
        public override bool IsFamilyOrAssembly
        {
            get { return (this.Flags & TypeFlags.VisibilityMask) == TypeFlags.NestedFamORAssem; }
        }
        protected bool isGeneric;
        /// <summary>True if this type is a template conforming to the rules of a generic type in the CLR.</summary>
        public virtual bool IsGeneric
        {
            get
            {
                return this.isGeneric;
            }
            set
            {
                this.isGeneric = value;
            }
        }

        public virtual bool IsNestedAssembly
        {
            get { return (this.Flags & TypeFlags.VisibilityMask) == TypeFlags.NestedAssembly; }
        }

        public virtual bool IsNestedFamily
        {
            get { return (this.Flags & TypeFlags.VisibilityMask) == TypeFlags.NestedFamily; }
        }
        public virtual bool IsNestedFamilyAndAssembly
        {
            get { return (this.Flags & TypeFlags.VisibilityMask) == TypeFlags.NestedFamANDAssem; }
        }
        public virtual bool IsNestedInternal
        {
            get { return (this.Flags & TypeFlags.VisibilityMask) == TypeFlags.NestedFamORAssem; }
        }
        public virtual bool IsNestedIn(TypeNode type)
        {
            for(TypeNode decType = this.DeclaringType; decType != null; decType = decType.DeclaringType)
            {
                if(decType == type)
                    return true;
            }
            return false;
        }
        public virtual bool IsNestedPublic
        {
            get { return (this.Flags & TypeFlags.VisibilityMask) == TypeFlags.NestedPublic; }
        }
        public virtual bool IsNonPublic
        {
            get { return (this.Flags & TypeFlags.VisibilityMask) == TypeFlags.NotPublic; }
        }

        protected bool isNormalized;

        /// <summary>
        /// True if the type node is in "normal" form. A node is in "normal" form if it is effectively a node in an AST formed directly
        /// from CLR module or assembly. Such a node can be written out as compiled code to an assembly or module without further processing.
        /// </summary>
        public virtual bool IsNormalized
        {
            get
            {
                if(this.isNormalized)
                    return true;
                if(this.DeclaringModule == null)
                    return false;
                return this.isNormalized = this.DeclaringModule.IsNormalized;
            }
            set
            {
                this.isNormalized = value;
            }
        }

        public override bool IsPrivate
        {
            get { return (this.Flags & TypeFlags.VisibilityMask) == TypeFlags.NestedPrivate; }
        }
        /// <summary>True if values of this type can be compared directly in CLR IL instructions.</summary>
        public virtual bool IsPrimitiveComparable
        {
            get
            {
                switch(this.typeCode)
                {
                    case ElementType.Boolean:
                    case ElementType.Char:
                    case ElementType.Int8:
                    case ElementType.Int16:
                    case ElementType.Int32:
                    case ElementType.Int64:
                    case ElementType.IntPtr:
                    case ElementType.UInt8:
                    case ElementType.UInt16:
                    case ElementType.UInt32:
                    case ElementType.UInt64:
                    case ElementType.UIntPtr:
                    case ElementType.Single:
                    case ElementType.Double:
                        return true;
                    default:
                        return !(this is Struct) || this is EnumNode || this is Pointer;
                }
            }
        }
        /// <summary>True if values of this type are integers that can be processed by CLR IL instructions.</summary>
        public virtual bool IsPrimitiveInteger
        {
            get
            {
                switch(this.typeCode)
                {
                    case ElementType.Int8:
                    case ElementType.Int16:
                    case ElementType.Int32:
                    case ElementType.Int64:
                    case ElementType.IntPtr:
                    case ElementType.UInt8:
                    case ElementType.UInt16:
                    case ElementType.UInt32:
                    case ElementType.UInt64:
                    case ElementType.UIntPtr:
                        return true;
                    default:
                        return false;
                }
            }
        }
        /// <summary>True if values of this type are integers or floating point numbers that can be processed by CLR IL instructions.</summary>
        public virtual bool IsPrimitiveNumeric
        {
            get
            {
                switch(this.typeCode)
                {
                    case ElementType.Int8:
                    case ElementType.Int16:
                    case ElementType.Int32:
                    case ElementType.Int64:
                    case ElementType.IntPtr:
                    case ElementType.UInt8:
                    case ElementType.UInt16:
                    case ElementType.UInt32:
                    case ElementType.UInt64:
                    case ElementType.UIntPtr:
                    case ElementType.Single:
                    case ElementType.Double:
                        return true;
                    default:
                        return false;
                }
            }
        }
        /// <summary>True if values of this type are unsigned integers that can be processed by CLR IL instructions.</summary>
        public virtual bool IsPrimitiveUnsignedInteger
        {
            get
            {
                switch(this.typeCode)
                {
                    case ElementType.UInt8:
                    case ElementType.UInt16:
                    case ElementType.UInt32:
                    case ElementType.UInt64:
                    case ElementType.UIntPtr:
                        return true;
                    default:
                        return false;
                }
            }
        }
        public override bool IsPublic
        {
            get
            {
                TypeFlags visibility = this.Flags & TypeFlags.VisibilityMask;
                return visibility == TypeFlags.Public || visibility == TypeFlags.NestedPublic;
            }
        }
        /// <summary>True if values of this type can be processed by CLR IL instructions.</summary>
        public virtual bool IsPrimitive
        {
            get
            {
                switch(this.typeCode)
                {
                    case ElementType.Boolean:
                    case ElementType.Char:
                    case ElementType.Double:
                    case ElementType.Int16:
                    case ElementType.Int32:
                    case ElementType.Int64:
                    case ElementType.Int8:
                    case ElementType.IntPtr:
                    case ElementType.Single:
                    case ElementType.String:
                    case ElementType.UInt16:
                    case ElementType.UInt32:
                    case ElementType.UInt64:
                    case ElementType.UInt8:
                    case ElementType.UIntPtr:
                        return true;
                    default:
                        return false;
                }
            }
        }
        /// <summary>True if the type cannot be derived from.</summary>
        public virtual bool IsSealed
        {
            get
            {
                return (this.Flags & TypeFlags.Sealed) != 0;
            }
        }
        public override bool IsSpecialName
        {
            get { return (this.Flags & TypeFlags.SpecialName) != 0; }
        }
        public override bool IsStatic
        {
            get { return true; }
        }
        /// <summary>True if the identity of the type depends on its structure rather than its name. 
        /// Arrays, pointers and generic type instances are examples of such types.</summary>
        public virtual bool IsStructural
        {
            get
            {
                return this.Template != null;
            }
        }
        /// <summary>True if the type serves as a parameter to a type template.</summary>
        public virtual bool IsTemplateParameter
        {
            get
            {
                return false;
            }
        }

        /// <summary>True if the type is a value type containing only fields of unmanaged types.</summary>
        public virtual bool IsUnmanaged
        {
            get
            {
                return false;
            }
        }

        /// <summary>A list of the types that contribute to the structure of a structural type.</summary>
        public virtual TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList result = this.TemplateArguments;
                if(result != null && result.Count > 0)
                    return result;
                return this.TemplateParameters;
            }
        }
        /// <summary>True if values of this type are unsigned integers that can be processed by CLR IL instructions.</summary>
        public virtual bool IsUnsignedPrimitiveNumeric
        {
            get
            {
                switch(this.typeCode)
                {
                    case ElementType.UInt8:
                    case ElementType.UInt16:
                    case ElementType.UInt32:
                    case ElementType.UInt64:
                    case ElementType.UIntPtr:
                        return true;
                    default:
                        return false;
                }
            }
        }
        /// <summary>True if instances of this type have no identity other than their value and are copied upon assignment.</summary>
        public virtual bool IsValueType
        {
            get
            {
                switch(this.NodeType)
                {
                    case NodeType.EnumNode:
                    case NodeType.ConstrainedType:
                    case NodeType.TupleType:
                    case NodeType.TypeAlias:
                    case NodeType.TypeIntersection:
                    case NodeType.TypeUnion:
                        return true;

                    case NodeType.Struct:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// True if underlying type (modulo type modifiers) is a pointer type (Pointer)
        /// </summary>
        public virtual bool IsPointerType
        {
            get { return false; }
        }

        public override bool IsVisibleOutsideAssembly
        {
            get
            {
                if(this.DeclaringType != null && !this.DeclaringType.IsVisibleOutsideAssembly)
                    return false;
                switch(this.Flags & TypeFlags.VisibilityMask)
                {
                    case TypeFlags.Public:
                    case TypeFlags.NestedPublic:
                        return true;
                    case TypeFlags.NestedFamily:
                    case TypeFlags.NestedFamORAssem:
                        return this.DeclaringType != null && !this.DeclaringType.IsSealed;
                    default:
                        return false;
                }
            }
        }
        // This field stores those members declared syntactically within
        // this type node.  (Under Extended Sing#, additional members can
        // be logically part of a type node but declared in a separate
        // syntactic type node.)
        protected internal MemberList members;
        protected volatile internal bool membersBeingPopulated;
        /// <summary>
        /// The list of members contained inside this type, by default ignoring any extensions of this type.
        /// (Subclasses in the Extensible Sing# dialect override this to include members of visible extensions.)
        /// If the value of members is null and the value of ProvideTypeMembers is not null, the 
        /// TypeMemberProvider delegate is called to fill in the value of this property.
        /// </summary>
        public virtual MemberList Members
        {
            get
            {
                if(this.members == null || this.membersBeingPopulated)
                    if(this.ProvideTypeMembers != null && this.ProviderHandle != null)
                    {
                        lock(Module.GlobalLock)
                        {
                            if(this.members == null)
                            {
                                this.membersBeingPopulated = true;
                                this.ProvideTypeMembers(this, this.ProviderHandle);
                                this.membersBeingPopulated = false;
                            }
                        }
                    }
                    else
                        this.members = new MemberList();
                return this.members;
            }
            set
            {
                this.members = value;
                this.memberCount = 0;
                this.memberTable = null;
                this.constructors = null;
                this.defaultMembers = null;
                this.explicitCoercionFromTable = null;
                this.explicitCoercionMethods = null;
                this.explicitCoercionToTable = null;
                this.implicitCoercionFromTable = null;
                this.implicitCoercionMethods = null;
                this.implicitCoercionToTable = null;
                this.opFalse = null;
                this.opTrue = null;
            }
        }

        protected TypeNode template;

        /// <summary>The (generic) type template from which this type was instantiated. Null if this is not a (generic) type template instance.</summary>
        public virtual TypeNode Template
        {
            get
            {
                TypeNode result = this.template;
                if(result == null)
                {
                    if(this.isGeneric || TargetPlatform.GenericTypeNamesMangleChar != '_')
                        return null;
                    AttributeList attributes = this.Attributes;
                    lock(this)
                    {
                        if(this.template != null)
                        {
                            if(this.template == TypeNode.NotSpecified)
                                return null;
                            return this.template;
                        }

                        if(result == null)
                            this.template = TypeNode.NotSpecified;
                    }
                }
                else if(result == TypeNode.NotSpecified)
                    return null;
                return result;
            }
            set
            {
                this.template = value;
            }
        }

        public TypeNode TemplateExpression;

        protected TypeNodeList templateArguments;
        /// <summary>The arguments used when this (generic) type template instance was instantiated.</summary>
        public virtual TypeNodeList TemplateArguments
        {
            get
            {
                if(this.template == null)
                {
                    TypeNode templ = this.Template; //Will fill in the arguments
                    if(templ != null)
                        templ = null;
                }
                return this.templateArguments;
            }
            set
            {
                this.templateArguments = value;
            }
        }

        public TypeNodeList TemplateArgumentExpressions;

        internal TypeNodeList consolidatedTemplateArguments;
        public virtual TypeNodeList ConsolidatedTemplateArguments
        {
            get
            {
                if(this.consolidatedTemplateArguments == null)
                    this.consolidatedTemplateArguments = this.GetConsolidatedTemplateArguments();
                return this.consolidatedTemplateArguments;
            }
            set
            {
                this.consolidatedTemplateArguments = value;
            }
        }

        private void AddTemplateParametersFromAttributeEncoding(TypeNodeList result)
        {
        }

        internal TypeNodeList templateParameters;

        /// <summary>The type parameters of this type. Null if this type is not a (generic) type template.</summary>
        public virtual TypeNodeList TemplateParameters
        {
            get
            {
                TypeNodeList result = this.templateParameters;
                if(result == null)
                {
                    if(this.isGeneric || TargetPlatform.GenericTypeNamesMangleChar != '_')
                        return null; //Can happen when this is nested in a generic type
                    TypeNodeList nestedTypes = this.NestedTypes;
                    lock(this)
                    {
                        if((result = this.templateParameters) != null)
                            return result.Count == 0 ? null : result;
                        result = new TypeNodeList();
                        for(int i = 0, n = nestedTypes == null ? 0 : nestedTypes.Count; i < n; i++)
                        {
                            TypeNode nt = nestedTypes[i];
                            if(nt == null)
                                continue;
                            if(nt is MethodTypeParameter)
                                continue;
                            if(nt.NodeType == NodeType.TypeParameter || nt.NodeType == NodeType.ClassParameter)
                                result.Add(nt);
                        }
                        this.AddTemplateParametersFromAttributeEncoding(result);
                        this.TemplateParameters = result;
                    }
                }
                if(result.Count == 0)
                    return null;
                return result;
            }
            set
            {
                if(value == null)
                {
                    if(this.templateParameters == null)
                        return;

                    if(this.templateParameters.Count > 0)
                        value = new TypeNodeList();
                }

                this.templateParameters = value;
            }
        }
        protected internal TypeNodeList consolidatedTemplateParameters;
        public virtual TypeNodeList ConsolidatedTemplateParameters
        {
            get
            {
                if(this.consolidatedTemplateParameters == null)
                    this.consolidatedTemplateParameters = this.GetConsolidatedTemplateParameters();
                return this.consolidatedTemplateParameters;
            }
            set
            {
                this.consolidatedTemplateParameters = value;
            }
        }
        internal ElementType typeCode = ElementType.Class;
        /// <summary>The System.TypeCode value that Convert.GetTypeCode will return pass an instance of this type as parameter.</summary>
        public virtual System.TypeCode TypeCode
        {
            get
            {
                switch(this.typeCode)
                {
                    case ElementType.Boolean:
                        return System.TypeCode.Boolean;
                    case ElementType.Char:
                        return System.TypeCode.Char;
                    case ElementType.Double:
                        return System.TypeCode.Double;
                    case ElementType.Int16:
                        return System.TypeCode.Int16;
                    case ElementType.Int32:
                        return System.TypeCode.Int32;
                    case ElementType.Int64:
                        return System.TypeCode.Int64;
                    case ElementType.Int8:
                        return System.TypeCode.SByte;
                    case ElementType.Single:
                        return System.TypeCode.Single;
                    case ElementType.UInt16:
                        return System.TypeCode.UInt16;
                    case ElementType.UInt32:
                        return System.TypeCode.UInt32;
                    case ElementType.UInt64:
                        return System.TypeCode.UInt64;
                    case ElementType.UInt8:
                        return System.TypeCode.Byte;
                    case ElementType.Void:
                        return System.TypeCode.Empty;
                    default:
                        if(this == CoreSystemTypes.String)
                            return System.TypeCode.String;
                        if(this == CoreSystemTypes.Decimal)
                            return System.TypeCode.Decimal;
                        if(this == CoreSystemTypes.DateTime)
                            return System.TypeCode.DateTime;
                        if(this == CoreSystemTypes.DBNull)
                            return System.TypeCode.DBNull;

                        return System.TypeCode.Object;
                }
            }
        }

        private readonly static TypeNode NotSpecified = new Class();

        protected internal TrivialHashtableUsingWeakReferences structurallyEquivalentMethod;

        /// <summary>
        /// Returns the methods of an abstract type that have been left unimplemented. Includes methods inherited from
        /// base classes and interfaces, and methods from any (known) extensions.
        /// </summary>
        /// <param name="result">A method list to which the abstract methods must be appended.</param>
        public virtual void GetAbstractMethods(MethodList/*!*/ result)
        {
            if(!this.IsAbstract)
                return;
            //For each interface, get abstract methods and keep those that are not implemented by this class or a base class
            InterfaceList interfaces = this.Interfaces;
            for(int i = 0, n = interfaces == null ? 0 : interfaces.Count; i < n; i++)
            {
                Interface iface = interfaces[i];
                if(iface == null)
                    continue;
                MemberList imembers = iface.Members;
                for(int j = 0, m = imembers == null ? 0 : imembers.Count; j < m; j++)
                {
                    Method meth = imembers[j] as Method;
                    if(meth == null)
                        continue;
                    if(this.ImplementsExplicitly(meth))
                        continue;
                    if(this.ImplementsMethod(meth, true))
                        continue;
                    result.Add(meth);
                }
            }
        }

        protected internal TrivialHashtable szArrayTypes;

        /// <summary>
        /// Returns a type representing an array whose elements are of this type. Will always return the same instance for the same rank.
        /// </summary>
        /// <param name="rank">The number of dimensions of the array.</param>
        public virtual ArrayType/*!*/ GetArrayType(int rank)
        {
            return this.GetArrayType(rank, false);
        }
        public virtual ArrayType/*!*/ GetArrayType(int rank, bool lowerBoundIsUnknown)
        {
            if(rank > 1 || lowerBoundIsUnknown)
                return this.GetArrayType(rank, 0, 0, new int[0], new int[0]);
            if(this.szArrayTypes == null)
                this.szArrayTypes = new TrivialHashtable();
            ArrayType result = (ArrayType)this.szArrayTypes[rank];
            if(result != null)
                return result;
            lock(this)
            {
                result = (ArrayType)this.szArrayTypes[rank];
                if(result != null)
                    return result;
                this.szArrayTypes[rank] = result = new ArrayType(this, 1);
                result.Flags &= ~TypeFlags.VisibilityMask;
                result.Flags |= this.Flags & TypeFlags.VisibilityMask;
                return result;
            }
        }
        protected internal TrivialHashtable arrayTypes;
        /// <summary>
        /// Returns a type representing an array whose elements are of this type. Will always return the same instance for the same rank, sizes and bounds.
        /// </summary>
        /// <param name="rank">The number of dimensions of the array.</param>
        /// <param name="sizes">The size of each dimension.</param>
        /// <param name="loBounds">The lower bound for indices. Defaults to zero.</param>
        public virtual ArrayType/*!*/ GetArrayType(int rank, int[] sizes, int[] loBounds)
        {
            return this.GetArrayType(rank, sizes == null ? 0 : sizes.Length, loBounds == null ? 0 : loBounds.Length, sizes == null ? new int[0] : sizes, loBounds == null ? new int[0] : loBounds);
        }
        internal ArrayType/*!*/ GetArrayType(int rank, int numSizes, int numLoBounds, int[]/*!*/ sizes, int[]/*!*/ loBounds)
        {
            if(this.arrayTypes == null)
                this.arrayTypes = new TrivialHashtable();
            StringBuilder sb = new StringBuilder(rank * 5);
            for(int i = 0; i < rank; i++)
            {
                if(i < numLoBounds)
                    sb.Append(loBounds[i]);
                else
                    sb.Append('0');
                if(i < numSizes) { sb.Append(':'); sb.Append(sizes[i]); }
                sb.Append(',');
            }
            Identifier id = Identifier.For(sb.ToString());
            ArrayType result = (ArrayType)this.arrayTypes[id.UniqueIdKey];
            if(result != null)
                return result;
            lock(this)
            {
                result = (ArrayType)this.arrayTypes[id.UniqueIdKey];
                if(result != null)
                    return result;
                if(loBounds == null)
                    loBounds = new int[0];
                this.arrayTypes[id.UniqueIdKey] = result = new ArrayType(this, rank, sizes, loBounds);
                result.Flags &= ~TypeFlags.VisibilityMask;
                result.Flags |= this.Flags & TypeFlags.VisibilityMask;
                return result;
            }
        }
        protected internal MemberList constructors;
        public virtual MemberList GetConstructors()
        {
            if(this.Members.Count != this.memberCount)
                this.constructors = null;
            if(this.constructors != null)
                return this.constructors;
            lock(this)
            {
                if(this.constructors != null)
                    return this.constructors;
                return this.constructors = TypeNode.WeedOutNonSpecialMethods(this.GetMembersNamed(StandardIds.Ctor), MethodFlags.RTSpecialName);
            }
        }
        /// <summary>
        /// Returns the constructor with the specified parameter types. Returns null if this type has no such constructor.
        /// </summary>
        public virtual InstanceInitializer GetConstructor(params TypeNode[] types)
        {
            return (InstanceInitializer)GetMethod(this.GetConstructors(), types);
        }

        protected override Identifier GetDocumentationId()
        {
            if(this.DeclaringType == null)
                return Identifier.For("T:" + this.FullName);
            else
                return Identifier.For(this.DeclaringType.DocumentationId + "." + this.Name);
        }

        internal virtual void AppendDocumentIdMangledName(StringBuilder/*!*/ sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            if(this.DeclaringType != null)
            {
                this.DeclaringType.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
                sb.Append('.');
                sb.Append(this.GetUnmangledNameWithoutTypeParameters());
            }
            else
                sb.Append(this.GetFullUnmangledNameWithoutTypeParameters());
            TypeNodeList templateArguments = this.TemplateArguments;
            int n = templateArguments == null ? 0 : templateArguments.Count;
            if(n == 0)
                return;
            sb.Append('{');
            for(int i = 0; i < n; i++)
            {
                //^ assert templateArguments != null;
                TypeNode templArg = templateArguments[i];
                if(templArg == null)
                    continue;
                templArg.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
                if(i < n - 1)
                    sb.Append(',');
            }
            sb.Append('}');
        }

        internal TrivialHashtable modifierTable;
        internal TypeNode/*!*/ GetModified(TypeNode/*!*/ modifierType, bool optionalModifier)
        {
            if(this.modifierTable == null)
                this.modifierTable = new TrivialHashtable();
            TypeNode result = (TypeNode)this.modifierTable[modifierType.UniqueKey];
            if(result != null)
                return result;
            result = optionalModifier ? (TypeNode)new OptionalModifier(modifierType, this) : (TypeNode)new RequiredModifier(modifierType, this);
            this.modifierTable[modifierType.UniqueKey] = result;
            return result;
        }
        public virtual TypeNode/*!*/ GetGenericTemplateInstance(Module/*!*/ module, TypeNodeList/*!*/ consolidatedArguments)
        {
            if(this.DeclaringType == null)
                return this.GetTemplateInstance(module, null, null, consolidatedArguments);
            TypeNodeList myArgs = this.GetOwnTemplateArguments(consolidatedArguments);
            if(myArgs == consolidatedArguments)
                return this.GetTemplateInstance(module, null, this.DeclaringType, consolidatedArguments);
            int n = consolidatedArguments.Count;
            int m = myArgs == null ? 0 : myArgs.Count;
            int k = n - m;
            Debug.Assert(k > 0);
            TypeNodeList parentArgs = new TypeNodeList();
            for(int i = 0; i < k; i++)
                parentArgs.Add(consolidatedArguments[i]);
            TypeNode declaringType = this.DeclaringType.GetGenericTemplateInstance(module, parentArgs);
            TypeNode nestedType = declaringType.GetNestedType(this.Name);
            if(nestedType == null) { Debug.Fail("template declaring type dummy instance does not have a nested type corresponding to template"); nestedType = this; }
            if(m == 0) { Debug.Assert(nestedType.template != null); return nestedType; }
            return nestedType.GetTemplateInstance(module, null, declaringType, myArgs);
        }
        public virtual TypeNode/*!*/ GetTemplateInstance(Module module, params TypeNode[] typeArguments)
        {
            return this.GetTemplateInstance(module, null, null, new TypeNodeList(typeArguments));
        }
        protected virtual void TryToFindExistingInstance(Module/*!*/ module, TypeNode declaringType, TypeNodeList/*!*/ templateArguments, Identifier/*!*/ mangledName,
          Identifier/*!*/ uniqueMangledName, out TypeNode result, out Identifier unusedMangledName)
        {
            unusedMangledName = null;
            string mangledNameString = mangledName.Name;
            int n = templateArguments.Count;
            int tiCount = 0;
            bool lookInReferencedAssemblies = TargetPlatform.GenericTypeNamesMangleChar != '`'; //REVIEW: huh? why not use IsGeneric?
            result = module.GetStructurallyEquivalentType(this.Namespace == null ? Identifier.Empty : this.Namespace, mangledName, uniqueMangledName, lookInReferencedAssemblies);
            if(this.IsGeneric)
            {
                if(result == null)
                    unusedMangledName = mangledName;
                return;
            }
            while(result != null)
            {
                //Mangled name is the same. But mangling is not unique (types are not qualified with assemblies), so check for equality.
                if(this == result.Template && (declaringType == result.DeclaringType || !this.IsGeneric))
                {
                    bool goodMatch = (result.TemplateArguments != null || n == 0) && result.TemplateArguments.Count == n;
                    for(int i = 0; goodMatch && i < n; i++)
                        goodMatch = templateArguments[i] == result.TemplateArguments[i];
                    if(goodMatch)
                        return;
                }
                //Mangle some more
                mangledName = new Identifier(mangledNameString + (++tiCount).ToString());
                result = module.GetStructurallyEquivalentType(this.Namespace == null ? Identifier.Empty : this.Namespace, mangledName, null, lookInReferencedAssemblies);
            }
            if(result == null)
                unusedMangledName = mangledName;
        }
        public virtual Identifier/*!*/ GetMangledTemplateInstanceName(TypeNodeList/*!*/ templateArguments, out Identifier/*!*/ uniqueMangledName, out bool notFullySpecialized)
        {
            StringBuilder mangledNameBuilder = new StringBuilder(this.Name.ToString());
            StringBuilder uniqueMangledNameBuilder = new StringBuilder(this.Name.ToString());
            uniqueMangledNameBuilder.Append(this.UniqueKey);
            notFullySpecialized = false;
            for(int i = 0, n = templateArguments.Count; i < n; i++)
            {
                if(i == 0) { mangledNameBuilder.Append('<'); uniqueMangledNameBuilder.Append('<'); }
                TypeNode t = templateArguments[i];
                if(t == null || t.Name == null)
                    continue;
                if(TypeIsNotFullySpecialized(t))
                    notFullySpecialized = true;
                mangledNameBuilder.Append(t.FullName);
                uniqueMangledNameBuilder.Append(t.FullName);
                uniqueMangledNameBuilder.Append(t.UniqueKey);
                if(i < n - 1)
                {
                    mangledNameBuilder.Append(',');
                    uniqueMangledNameBuilder.Append(',');
                }
                else
                {
                    mangledNameBuilder.Append('>');
                    uniqueMangledNameBuilder.Append('>');
                }
            }
            uniqueMangledName = Identifier.For(uniqueMangledNameBuilder.ToString());
            return Identifier.For(mangledNameBuilder.ToString());
        }
        private static bool TypeIsNotFullySpecialized(TypeNode t)
        {
            TypeNode tt = TypeNode.StripModifiers(t);
            //^ assert tt != null;        
            if(tt is TypeParameter || tt is ClassParameter || tt.IsNotFullySpecialized)
                return true;
            for(int j = 0, m = tt.StructuralElementTypes == null ? 0 : tt.StructuralElementTypes.Count; j < m; j++)
            {
                TypeNode et = tt.StructuralElementTypes[j];
                if(et == null)
                    continue;
                if(TypeIsNotFullySpecialized(et))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Gets an instance for the given template arguments of this (generic) template type.
        /// </summary>
        /// <param name="referringType">The type in which the reference to the template instance occurs. If the template is not
        /// generic, the instance becomes a nested type of the referring type so that it has the same access privileges as the
        /// code referring to the instance.</param>
        /// <param name="templateArguments">The template arguments.</param>
        /// <returns>An instance of the template. Always the same instance for the same arguments.</returns>
        public virtual TypeNode/*!*/ GetTemplateInstance(TypeNode referringType, params TypeNode[] templateArguments)
        {
            return this.GetTemplateInstance(referringType, new TypeNodeList(templateArguments));
        }
        /// <summary>
        /// Gets an instance for the given template arguments of this (generic) template type.
        /// </summary>
        /// <param name="referringType">The type in which the reference to the template instance occurs. If the template is not
        /// generic, the instance becomes a nested type of the referring type so that it has the same access privileges as the
        /// code referring to the instance.</param>
        /// <param name="templateArguments">The template arguments.</param>
        /// <returns>An instance of the template. Always the same instance for the same arguments.</returns>
        public virtual TypeNode/*!*/ GetTemplateInstance(TypeNode referringType, TypeNodeList templateArguments)
        {
            if(referringType == null)
                return this;
            Module module = referringType.DeclaringModule;
            return this.GetTemplateInstance(module, referringType, this.DeclaringType, templateArguments);
        }
        class CachingModuleForGenericsInstances : Module
        {
            public override TypeNode GetStructurallyEquivalentType(Identifier ns, Identifier/*!*/ id, Identifier uniqueMangledName, bool lookInReferencedAssemblies)
            {
                if(uniqueMangledName == null)
                    return null;
                return (TypeNode)this.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey];
            }
        }
        protected static Module/*!*/ cachingModuleForGenericInstances = new CachingModuleForGenericsInstances();
        public virtual TypeNode/*!*/ GetTemplateInstance(Module module, TypeNode referringType, TypeNode declaringType, TypeNodeList templateArguments)
        {
            TypeNodeList templateParameters = this.TemplateParameters;
            if(module == null || templateArguments == null || (declaringType == null && (templateParameters == null || templateParameters.Count == 0)))
                return this;
            if(this.IsGeneric)
            {
                referringType = null;
                module = TypeNode.cachingModuleForGenericInstances;
            }
            bool notFullySpecialized;
            Identifier/*!*/ uniqueMangledName;
            Identifier mangledName = this.GetMangledTemplateInstanceName(templateArguments, out uniqueMangledName, out notFullySpecialized);
            TypeNode result;
            Identifier dummyId;
            this.TryToFindExistingInstance(module, declaringType, templateArguments, mangledName, uniqueMangledName, out result, out dummyId);
            if(result != null)
                return result;
            if(this.NewTemplateInstanceIsRecursive)
                return this; //An instance of this template is trying to instantiate the template again
            lock(Module.GlobalLock)
            {
                Identifier unusedMangledName;
                this.TryToFindExistingInstance(module, declaringType, templateArguments, mangledName, uniqueMangledName, out result, out unusedMangledName);
                if(result != null)
                    return result;
                //^ assume unusedMangledName != null;
                TypeNodeList consolidatedTemplateArguments =
                  declaringType == null ? templateArguments : declaringType.GetConsolidatedTemplateArguments(templateArguments);
                Duplicator duplicator = new Duplicator(module, referringType);
                duplicator.RecordOriginalAsTemplate = true;
                duplicator.SkipBodies = true;
                duplicator.TypesToBeDuplicated[this.UniqueKey] = this;
                result = duplicator.VisitTypeNode(this, unusedMangledName, consolidatedTemplateArguments, this.Template == null ? this : this.Template, true);
                //^ assume result != null;
                if(module == this.DeclaringModule)
                {
                    if(this.TemplateInstances == null)
                        this.TemplateInstances = new TypeNodeList();
                    this.TemplateInstances.Add(result);
                }
                result.Name = unusedMangledName;
                result.Name.SourceContext = this.Name.SourceContext;
                result.fullName = null;
                if(this.IsGeneric)
                    result.DeclaringModule = this.DeclaringModule;
                result.DeclaringType = this.IsGeneric || referringType == null ? declaringType : referringType;
                result.Template = this;
                result.templateParameters = new TypeNodeList();
                result.consolidatedTemplateParameters = new TypeNodeList();
                result.templateArguments = templateArguments;
                result.consolidatedTemplateArguments = consolidatedTemplateArguments;
                result.IsNotFullySpecialized = notFullySpecialized || (declaringType != null && declaringType.IsNotFullySpecialized);
                module.StructurallyEquivalentType[unusedMangledName.UniqueIdKey] = result;
                module.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey] = result;
                Specializer specializer = new Specializer(module, this.ConsolidatedTemplateParameters, consolidatedTemplateArguments);
                specializer.VisitTypeNode(result);
                TypeFlags visibility = this.Flags & TypeFlags.VisibilityMask;
                for(int i = 0, m = templateArguments.Count; i < m; i++)
                {
                    TypeNode t = templateArguments[i];
                    if(t == null)
                        continue;
                    if(t is TypeParameter || t is ClassParameter || t.IsNotFullySpecialized)
                        continue;
                    visibility = TypeNode.GetVisibilityIntersection(visibility, t.Flags & TypeFlags.VisibilityMask);
                }
                result.Flags &= ~TypeFlags.VisibilityMask;
                result.Flags |= visibility;
                if(this.IsGeneric)
                    return result;

                return result;
            }
        }

        protected virtual TypeNodeList GetConsolidatedTemplateArguments()
        {
            TypeNodeList typeArgs = this.TemplateArguments;

            if(this.DeclaringType == null)
                return typeArgs;

            TypeNodeList result = this.DeclaringType.ConsolidatedTemplateArguments;

            if(result == null)
            {
                if(this.DeclaringType.IsGeneric && this.DeclaringType.Template == null)
                    result = this.DeclaringType.ConsolidatedTemplateParameters;

                if(result == null)
                    return typeArgs;
            }

            int n = (typeArgs == null) ? 0 : typeArgs.Count;

            if(n == 0)
                return result;

            result = new TypeNodeList(result);
            result.AddRange(typeArgs);

            return result;
        }

        protected virtual TypeNodeList GetConsolidatedTemplateArguments(TypeNodeList typeArgs)
        {
            TypeNodeList result = this.ConsolidatedTemplateArguments;

            if(result == null || result.Count == 0)
            {
                if(this.IsGeneric && this.Template == null)
                    result = this.ConsolidatedTemplateParameters;
                else
                    return typeArgs;
            }

            int n = (typeArgs == null) ? 0 : typeArgs.Count;

            if(n == 0)
                return result;

            //^ assert typeArgs != null;

            result = new TypeNodeList(result);
            result.AddRange(typeArgs);

            return result;
        }

        protected virtual TypeNodeList GetConsolidatedTemplateParameters()
        {
            TypeNodeList typeParams = this.TemplateParameters;
            TypeNode declaringType = this.DeclaringType;

            if(declaringType == null)
                return typeParams;

            while(declaringType.Template != null)
                declaringType = declaringType.Template;

            TypeNodeList result = declaringType.ConsolidatedTemplateParameters;

            if(result == null)
                return typeParams;

            int n = (typeParams == null) ? 0 : typeParams.Count;

            if(n == 0)
                return result;

            result = new TypeNodeList(result);
            result.AddRange(typeParams);

            return result;
        }

        protected virtual TypeNodeList GetOwnTemplateArguments(TypeNodeList consolidatedTemplateArguments)
        {
            int n = this.TemplateParameters == null ? 0 : this.TemplateParameters.Count;
            int m = consolidatedTemplateArguments == null ? 0 : consolidatedTemplateArguments.Count;
            int k = m - n;

            if(k <= 0)
                return consolidatedTemplateArguments;

            TypeNodeList result = new TypeNodeList();

            if(consolidatedTemplateArguments != null)
                for(int i = 0; i < n; i++)
                    result.Add(consolidatedTemplateArguments[i + k]);

            return result;
        }

        protected internal Pointer pointerType;

        public virtual Pointer/*!*/ GetPointerType()
        {
            Pointer result = this.pointerType;
            if(result == null)
            {
                lock(this)
                {
                    if(this.pointerType != null)
                        return this.pointerType;
                    result = this.pointerType = new Pointer(this);
                    result.Flags &= ~TypeFlags.VisibilityMask;
                    result.Flags |= this.Flags & TypeFlags.VisibilityMask;
                    result.DeclaringModule = this.DeclaringModule;
                }
            }
            return result;
        }
        protected internal Reference referenceType;
        public virtual Reference/*!*/ GetReferenceType()
        {
            Reference result = this.referenceType;
            if(result == null)
            {
                lock(this)
                {
                    if(this.referenceType != null)
                        return this.referenceType;
                    result = this.referenceType = new Reference(this);
                    result.Flags &= ~TypeFlags.VisibilityMask;
                    result.Flags |= this.Flags & TypeFlags.VisibilityMask;
                    result.DeclaringModule = this.DeclaringModule;
                }
            }
            return result;
        }
        //^ [Microsoft.Contracts.SpecPublic]
        protected internal TrivialHashtable memberTable;
        protected internal int memberCount;
        /// <summary>
        /// Returns a list of all the members declared directly by this type with the specified name.
        /// Returns an empty list if this type has no such members.
        /// </summary>
        public virtual MemberList/*!*/ GetMembersNamed(Identifier name)
        {
            if(name == null)
                return new MemberList();

            MemberList members = this.Members;

            int n = members == null ? 0 : members.Count;

            if(n != this.memberCount || this.memberTable == null)
                this.UpdateMemberTable(n);

            //^ assert this.memberTable != null;
            MemberList result = (MemberList)this.memberTable[name.UniqueIdKey];

            if(result == null)
            {
                lock(this)
                {
                    result = (MemberList)this.memberTable[name.UniqueIdKey];

                    if(result != null)
                        return result;

                    result = new MemberList();
                    this.memberTable[name.UniqueIdKey] = result;
                }
            }
            return result;
        }
        /// <summary>
        /// Returns the first event declared by this type with the specified name.
        /// Returns null if this type has no such event.
        /// </summary>
        public virtual Event GetEvent(Identifier name)
        {
            MemberList members = this.GetMembersNamed(name);
            for(int i = 0, n = members.Count; i < n; i++)
            {
                Event ev = members[i] as Event;
                if(ev != null)
                    return ev;
            }
            return null;
        }
        /// <summary>
        /// Returns the first field declared by this type with the specified name. Returns null if this type has no such field.
        /// </summary>
        public virtual Field GetField(Identifier name)
        {
            MemberList members = this.GetMembersNamed(name);
            for(int i = 0, n = members.Count; i < n; i++)
            {
                Field field = members[i] as Field;
                if(field != null)
                    return field;
            }
            return null;
        }
        /// <summary>
        /// Returns the first method declared by this type with the specified name and parameter types. Returns null if this type has no such method.
        /// </summary>
        /// <returns></returns>
        public virtual Method GetMethod(Identifier name, params TypeNode[] types)
        {
            return GetMethod(this.GetMembersNamed(name), types);
        }
        private static Method GetMethod(MemberList members, params TypeNode[] types)
        {
            if(members == null)
                return null;
            int m = types == null ? 0 : types.Length;
            TypeNodeList typeNodes = m == 0 ? null : new TypeNodeList(types);
            for(int i = 0, n = members.Count; i < n; i++)
            {
                Method meth = members[i] as Method;
                if(meth == null)
                    continue;
                if(meth.ParameterTypesMatchStructurally(typeNodes))
                    return meth;
            }
            return null;
        }

        public Method GetMatchingMethod(Method method)
        {
            if(method == null || method.Name == null)
                return null;

            MemberList members = this.GetMembersNamed(method.Name);

            for(int i = 0, n = members == null ? 0 : members.Count; i < n; i++)
            {
                Method m = members[i] as Method;

                // !EFW - Bug fix for problem reported by SHarwell.  Must compare type parameter counts too.
                // If not, overloaded methods, one with and one without generic parameters, will be matched
                // incorrectly.
                if(m == null || !m.TypeParameterCountsMatch(method))
                    continue;

                if(m.ParametersMatchStructurally(method.Parameters))
                    return m;
            }

            return null;
        }

        /// <summary>
        /// Returns the first nested type declared by this type with the specified name. Returns null if this type has no such nested type.
        /// </summary>
        public virtual TypeNode GetNestedType(Identifier name)
        {
            if(name == null)
                return null;
            if(this.members != null)
            {
                MemberList members = this.GetMembersNamed(name);
                for(int i = 0, n = members.Count; i < n; i++)
                {
                    TypeNode type = members[i] as TypeNode;
                    if(type != null)
                        return type;
                }
                return null;
            }
            TypeNodeList nestedTypes = this.NestedTypes;
            for(int i = 0, n = nestedTypes == null ? 0 : nestedTypes.Count; i < n; i++)
            {
                TypeNode type = nestedTypes[i];
                if(type != null && type.Name.UniqueIdKey == name.UniqueIdKey)
                    return type;
            }
            return null;
        }

        protected internal TypeNodeList nestedTypes;

        // !EFW - Yes, static.  See below.
        private static int recursionCounter;

        public virtual TypeNodeList NestedTypes
        {
            get
            {
                // !EFW - Okay, this is a REALLY ugly hack but I couldn't think of any other way around it.
                // If a type contains a nested type that itself implements a nested type from within the
                // containing type (still with me?), this gets into an endless loop and eventually overflows
                // the stack.  The problem is, we can't just ignore all subsequent recursions or it can
                // throw a different error about a missing template type later on.  The trick is to let it
                // recurse enough to get all of the information it needs but not enough to overflow the
                // stack.  The full test case worked at 9 levels of recursion so 20 should be more than
                // enough for any case.  It overflowed at 256 levels of recursion.
                //
                // The abbreviated example:
                //
                // public interface IConsumer {}
                // public interface IConsumeContext<TMessage> {}
                //
                // public class Consumes<TMessage> where TMessage : class
                // {
                //     public interface All : IConsumer
		        //     {
                //         void Consume(TMessage message);
                //     }
                //
                //     public interface Context : Consumes<IConsumeContext<TMessage>>.All
		        //     {
		        //     }
                // }
                //
                // Note that this abbreviated example will overflow the stack but will not manifest the
                // secondary "missing template type" error if the recursion count is too low.  In case
                // you're wondering, the full test case was the Mass Transit project on GitHub which was
                // being used as a reference assembly by the person that reported the error.
                //
                if(recursionCounter > 20)
                    return null;

                if(this.nestedTypes != null && (this.members == null || this.members.Count == this.memberCount))
                    return this.nestedTypes;

                if(this.ProvideNestedTypes != null && this.ProviderHandle != null)
                {
                    lock(Module.GlobalLock)
                    {
                        recursionCounter++;

                        this.ProvideNestedTypes(this, this.ProviderHandle);

                        recursionCounter--;
                    }
                }
                else
                {
                    MemberList members = this.Members;
                    TypeNodeList nestedTypes = new TypeNodeList();
                    for(int i = 0, n = members == null ? 0 : members.Count; i < n; i++)
                    {
                        TypeNode nt = members[i] as TypeNode;
                        if(nt == null)
                            continue;
                        nestedTypes.Add(nt);
                    }
                    this.nestedTypes = nestedTypes;
                }
                return this.nestedTypes;
            }
            set
            {
                this.nestedTypes = value;
            }
        }
        /// <summary>
        /// Returns the first property declared by this type with the specified name and parameter types. Returns null if this type has no such property.
        /// </summary>
        public virtual Property GetProperty(Identifier name, params TypeNode[] types)
        {
            return GetProperty(this.GetMembersNamed(name), types);
        }
        private static Property GetProperty(MemberList members, params TypeNode[] types)
        {
            if(members == null)
                return null;
            int m = types == null ? 0 : types.Length;
            TypeNodeList typeNodes = m == 0 ? null : new TypeNodeList(types);
            for(int i = 0, n = members.Count; i < n; i++)
            {
                Property prop = members[i] as Property;
                if(prop == null)
                    continue;
                if(prop.ParameterTypesMatch(typeNodes))
                    return prop;
            }
            return null;
        }

        protected internal MemberList explicitCoercionMethods;
        public virtual MemberList ExplicitCoercionMethods
        {
            get
            {
                if(this.Members.Count != this.memberCount)
                    this.explicitCoercionMethods = null;
                if(this.explicitCoercionMethods != null)
                    return this.explicitCoercionMethods;
                lock(this)
                {
                    if(this.explicitCoercionMethods != null)
                        return this.explicitCoercionMethods;
                    return this.explicitCoercionMethods = TypeNode.WeedOutNonSpecialMethods(this.GetMembersNamed(StandardIds.opExplicit), MethodFlags.SpecialName);
                }
            }
        }
        protected internal MemberList implicitCoercionMethods;
        public virtual MemberList ImplicitCoercionMethods
        {
            get
            {
                if(this.Members.Count != this.memberCount)
                    this.implicitCoercionMethods = null;
                if(this.implicitCoercionMethods != null)
                    return this.implicitCoercionMethods;
                lock(this)
                {
                    if(this.implicitCoercionMethods != null)
                        return this.implicitCoercionMethods;
                    return this.implicitCoercionMethods = TypeNode.WeedOutNonSpecialMethods(this.GetMembersNamed(StandardIds.opImplicit), MethodFlags.SpecialName);
                }
            }
        }
        protected readonly static Method MethodDoesNotExist = new Method();
        protected internal TrivialHashtable explicitCoercionFromTable;
        public virtual Method GetExplicitCoercionFromMethod(TypeNode sourceType)
        {
            if(sourceType == null)
                return null;
            Method result = null;
            if(this.explicitCoercionFromTable != null)
                result = (Method)this.explicitCoercionFromTable[sourceType.UniqueKey];
            if(result == TypeNode.MethodDoesNotExist)
                return null;
            if(result != null)
                return result;
            lock(this)
            {
                if(this.explicitCoercionFromTable != null)
                    result = (Method)this.explicitCoercionFromTable[sourceType.UniqueKey];
                if(result == TypeNode.MethodDoesNotExist)
                    return null;
                if(result != null)
                    return result;
                MemberList coercions = this.ExplicitCoercionMethods;
                for(int i = 0, n = coercions.Count; i < n; i++)
                {
                    Method m = (Method)coercions[i];
                    if(sourceType == m.Parameters[0].Type) { result = m; break; }
                }
                if(this.explicitCoercionFromTable == null)
                    this.explicitCoercionFromTable = new TrivialHashtable();
                if(result == null)
                    this.explicitCoercionFromTable[sourceType.UniqueKey] = TypeNode.MethodDoesNotExist;
                else
                    this.explicitCoercionFromTable[sourceType.UniqueKey] = result;
                return result;
            }
        }
        protected internal TrivialHashtable explicitCoercionToTable;
        public virtual Method GetExplicitCoercionToMethod(TypeNode targetType)
        {
            if(targetType == null)
                return null;
            Method result = null;
            if(this.explicitCoercionToTable != null)
                result = (Method)this.explicitCoercionToTable[targetType.UniqueKey];
            if(result == TypeNode.MethodDoesNotExist)
                return null;
            if(result != null)
                return result;
            lock(this)
            {
                if(this.explicitCoercionToTable != null)
                    result = (Method)this.explicitCoercionToTable[targetType.UniqueKey];
                if(result == TypeNode.MethodDoesNotExist)
                    return null;
                if(result != null)
                    return result;
                MemberList coercions = this.ExplicitCoercionMethods;
                for(int i = 0, n = coercions.Count; i < n; i++)
                {
                    Method m = (Method)coercions[i];
                    if(m.ReturnType == targetType) { result = m; break; }
                }
                if(this.explicitCoercionToTable == null)
                    this.explicitCoercionToTable = new TrivialHashtable();
                if(result == null)
                    this.explicitCoercionToTable[targetType.UniqueKey] = TypeNode.MethodDoesNotExist;
                else
                    this.explicitCoercionToTable[targetType.UniqueKey] = result;
            }
            return result;
        }
        protected internal TrivialHashtable implicitCoercionFromTable;
        public virtual Method GetImplicitCoercionFromMethod(TypeNode sourceType)
        {
            if(sourceType == null)
                return null;
            Method result = null;
            if(this.implicitCoercionFromTable != null)
                result = (Method)this.implicitCoercionFromTable[sourceType.UniqueKey];
            if(result == TypeNode.MethodDoesNotExist)
                return null;
            if(result != null)
                return result;
            lock(this)
            {
                if(this.implicitCoercionFromTable != null)
                    result = (Method)this.implicitCoercionFromTable[sourceType.UniqueKey];
                if(result == TypeNode.MethodDoesNotExist)
                    return null;
                if(result != null)
                    return result;
                MemberList coercions = this.ImplicitCoercionMethods;
                for(int i = 0, n = coercions.Count; i < n; i++)
                {
                    Method m = (Method)coercions[i];
                    if(sourceType.IsStructurallyEquivalentTo(TypeNode.StripModifiers(m.Parameters[0].Type))) { result = m; break; }
                }
                if(this.implicitCoercionFromTable == null)
                    this.implicitCoercionFromTable = new TrivialHashtable();
                if(result == null)
                    this.implicitCoercionFromTable[sourceType.UniqueKey] = TypeNode.MethodDoesNotExist;
                else
                    this.implicitCoercionFromTable[sourceType.UniqueKey] = result;
                return result;
            }
        }
        protected internal TrivialHashtable implicitCoercionToTable;
        public virtual Method GetImplicitCoercionToMethod(TypeNode targetType)
        {
            if(targetType == null)
                return null;
            Method result = null;
            if(this.implicitCoercionToTable != null)
                result = (Method)this.implicitCoercionToTable[targetType.UniqueKey];
            if(result == TypeNode.MethodDoesNotExist)
                return null;
            if(result != null)
                return result;
            lock(this)
            {
                if(this.implicitCoercionToTable != null)
                    result = (Method)this.implicitCoercionToTable[targetType.UniqueKey];
                if(result == TypeNode.MethodDoesNotExist)
                    return null;
                if(result != null)
                    return result;
                MemberList coercions = this.ImplicitCoercionMethods;
                for(int i = 0, n = coercions.Count; i < n; i++)
                {
                    Method m = (Method)coercions[i];
                    if(m.ReturnType == targetType) { result = m; break; }
                }
                if(this.implicitCoercionToTable == null)
                    this.implicitCoercionToTable = new TrivialHashtable();
                if(result == null)
                    this.implicitCoercionToTable[targetType.UniqueKey] = TypeNode.MethodDoesNotExist;
                else
                    this.implicitCoercionToTable[targetType.UniqueKey] = result;
                return result;
            }
        }
        protected Method opFalse;
        public virtual Method GetOpFalse()
        {
            Method result = this.opFalse;
            if(result == TypeNode.MethodDoesNotExist)
                return null;
            if(result != null)
                return result;
            MemberList members = this.Members; //evaluate for side effect
            if(members != null)
                members = null;
            lock(this)
            {
                result = this.opFalse;
                if(result == TypeNode.MethodDoesNotExist)
                    return null;
                if(result != null)
                    return result;
                TypeNode t = this;
                while(t != null)
                {
                    MemberList opFalses = t.GetMembersNamed(StandardIds.opFalse);
                    if(opFalses != null)
                        for(int i = 0, n = opFalses.Count; i < n; i++)
                        {
                            Method opFalse = opFalses[i] as Method;
                            if(opFalse == null)
                                continue;
                            if(!opFalse.IsSpecialName || !opFalse.IsStatic || !opFalse.IsPublic || opFalse.ReturnType != CoreSystemTypes.Boolean ||
                              opFalse.Parameters == null || opFalse.Parameters.Count != 1)
                                continue;
                            return this.opFalse = opFalse;
                        }
                    t = t.BaseType;
                }
                this.opFalse = TypeNode.MethodDoesNotExist;
                return null;
            }
        }
        protected Method opTrue;
        public virtual Method GetOpTrue()
        {
            Method result = this.opTrue;
            if(result == TypeNode.MethodDoesNotExist)
                return null;
            if(result != null)
                return result;
            MemberList members = this.Members; //evaluate for side effect
            if(members != null)
                members = null;
            lock(this)
            {
                result = this.opTrue;
                if(result == TypeNode.MethodDoesNotExist)
                    return null;
                if(result != null)
                    return result;
                TypeNode t = this;
                while(t != null)
                {
                    MemberList opTrues = t.GetMembersNamed(StandardIds.opTrue);
                    if(opTrues != null)
                        for(int i = 0, n = opTrues.Count; i < n; i++)
                        {
                            Method opTrue = opTrues[i] as Method;
                            if(opTrue == null)
                                continue;
                            if(!opTrue.IsSpecialName || !opTrue.IsStatic || !opTrue.IsPublic || opTrue.ReturnType != CoreSystemTypes.Boolean ||
                              opTrue.Parameters == null || opTrue.Parameters.Count != 1)
                                continue;
                            return this.opTrue = opTrue;
                        }
                    t = t.BaseType;
                }
                this.opTrue = TypeNode.MethodDoesNotExist;
                return null;
            }
        }

        private static Hashtable typeMap; //contains weak references

        /// <summary>
        /// Gets a TypeNode instance corresponding to the given System.Type instance.
        /// </summary>
        /// <param name="type">A runtime type.</param>
        /// <returns>A TypeNode instance.</returns>
        public static TypeNode GetTypeNode(System.Type type)
        {
            if(type == null)
                return null;
            Hashtable typeMap = TypeNode.typeMap;
            if(typeMap == null)
                TypeNode.typeMap = typeMap = Hashtable.Synchronized(new Hashtable());
            TypeNode result = null;
            WeakReference wr = (WeakReference)typeMap[type];
            if(wr != null)
            {
                result = wr.Target as TypeNode;
                if(result == Class.DoesNotExist)
                    return null;
                if(result != null)
                    return result;
            }

            if(type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                try
                {
                    TypeNode template = TypeNode.GetTypeNode(type.GetGenericTypeDefinition());
                    if(template == null)
                        return null;
                    TypeNodeList templateArguments = new TypeNodeList();
                    foreach(Type arg in type.GetGenericArguments())
                        templateArguments.Add(TypeNode.GetTypeNode(arg));
                    return template.GetGenericTemplateInstance(template.DeclaringModule, templateArguments);
                }
                catch
                {
                    //TODO: log error
                    return null;
                }
            }
            if(type.IsGenericParameter)
            {
                try
                {
                    int parIndx = type.GenericParameterPosition;
                    System.Reflection.MethodInfo mi = type.DeclaringMethod as System.Reflection.MethodInfo;
                    if(mi != null)
                    {
                        Method m = Method.GetMethod(mi);
                        if(m == null)
                            return null;
                        if(m.TemplateParameters != null && m.TemplateParameters.Count > parIndx)
                            return m.TemplateParameters[parIndx];
                    }
                    else
                    {
                        System.Type ti = type.DeclaringType;
                        TypeNode t = TypeNode.GetTypeNode(ti);
                        if(t == null)
                            return null;
                        if(t.TemplateParameters != null && t.TemplateParameters.Count > parIndx)
                            return t.TemplateParameters[parIndx];
                    }
                    return null;
                }
                catch
                {
                    //TODO: log error
                    return null;
                }
            }

            if(type.HasElementType)
            {
                TypeNode elemType = TypeNode.GetTypeNode(type.GetElementType());
                if(elemType == null)
                    return null;
                if(type.IsArray)
                    result = elemType.GetArrayType(type.GetArrayRank());
                else if(type.IsByRef)
                    result = elemType.GetReferenceType();
                else if(type.IsPointer)
                    result = elemType.GetPointerType();
                else
                {
                    Debug.Assert(false);
                    result = null;
                }
            }
            else if(type.DeclaringType != null)
            {
                TypeNode dType = TypeNode.GetTypeNode(type.DeclaringType);
                if(dType == null)
                    return null;
                result = dType.GetNestedType(Identifier.For(type.Name));
            }
            else
            {
                AssemblyNode assem = AssemblyNode.GetAssembly(type.Assembly);
                if(assem != null)
                {
                    result = assem.GetType(Identifier.For(type.Namespace), Identifier.For(type.Name));
                }
            }
            if(result == null)
                typeMap[type] = new WeakReference(Class.DoesNotExist);
            else
                typeMap[type] = new WeakReference(result);
            return result;
        }
        protected internal Type runtimeType;
        /// <summary>
        /// Gets a System.Type instance corresponding to this type. The assembly containin this type must be normalized
        /// and must have a location on disk or must have been loaded via AssemblyNode.GetRuntimeAssembly.
        /// </summary>
        /// <returns>A System.Type instance. (A runtime type.)</returns>
        public virtual Type GetRuntimeType()
        {
            if(this.runtimeType == null)
            {
                lock(this)
                {
                    if(this.runtimeType != null)
                        return this.runtimeType;

                    if(this.IsGeneric && this.Template != null)
                    {
                        try
                        {
                            TypeNode rootTemplate = this.Template;
                            while(rootTemplate.Template != null)
                                rootTemplate = rootTemplate.Template;
                            Type genType = rootTemplate.GetRuntimeType();
                            if(genType == null)
                                return null;
                            TypeNodeList args = this.ConsolidatedTemplateArguments;
                            Type[] arguments = new Type[args.Count];
                            for(int i = 0; i < args.Count; i++)
                                arguments[i] = args[i].GetRuntimeType();
                            return genType.MakeGenericType(arguments);
                        }
                        catch
                        {
                            //TODO: add error to metadata import errors if type is imported
                            return null;
                        }
                    }

                    if(this.DeclaringType != null)
                    {
                        Type dt = this.DeclaringType.GetRuntimeType();
                        if(dt != null)
                        {
                            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.DeclaredOnly;
                            if(this.IsPublic)
                                flags |= System.Reflection.BindingFlags.Public;
                            else
                                flags |= System.Reflection.BindingFlags.NonPublic;
                            this.runtimeType = dt.GetNestedType(this.Name.ToString(), flags);
                        }
                    }
                    else if(this.DeclaringModule != null && this.DeclaringModule.IsNormalized && this.DeclaringModule.ContainingAssembly != null)
                    {
                        System.Reflection.Assembly runtimeAssembly = this.DeclaringModule.ContainingAssembly.GetRuntimeAssembly();
                        if(runtimeAssembly != null)
                            this.runtimeType = runtimeAssembly.GetType(this.FullName, false);
                    }
                }
            }
            return this.runtimeType;
        }

        public static TypeFlags GetVisibilityIntersection(TypeFlags vis1, TypeFlags vis2)
        {
            switch(vis2)
            {
                case TypeFlags.Public:
                case TypeFlags.NestedPublic:
                    return vis1;
                case TypeFlags.NotPublic:
                case TypeFlags.NestedAssembly:
                    switch(vis1)
                    {
                        case TypeFlags.Public:
                            return vis2;
                        case TypeFlags.NestedPublic:
                        case TypeFlags.NestedFamORAssem:
                            return TypeFlags.NestedAssembly;
                        case TypeFlags.NestedFamily:
                            return TypeFlags.NestedFamANDAssem;
                        default:
                            return vis1;
                    }
                case TypeFlags.NestedFamANDAssem:
                    switch(vis1)
                    {
                        case TypeFlags.Public:
                        case TypeFlags.NestedPublic:
                        case TypeFlags.NestedFamORAssem:
                        case TypeFlags.NestedFamily:
                            return TypeFlags.NestedFamANDAssem;
                        default:
                            return vis1;
                    }
                case TypeFlags.NestedFamORAssem:
                    switch(vis1)
                    {
                        case TypeFlags.Public:
                        case TypeFlags.NestedPublic:
                            return TypeFlags.NestedFamORAssem;
                        default:
                            return vis1;
                    }
                case TypeFlags.NestedFamily:
                    switch(vis1)
                    {
                        case TypeFlags.Public:
                        case TypeFlags.NestedPublic:
                        case TypeFlags.NestedFamORAssem:
                            return TypeFlags.NestedFamily;
                        case TypeFlags.NestedAssembly:
                            return TypeFlags.NestedFamANDAssem;
                        default:
                            return vis1;
                    }
                default:
                    return TypeFlags.NestedPrivate;
            }
        }
        private TrivialHashtable explicitInterfaceImplementations;
        public bool ImplementsExplicitly(Method method)
        {
            if(method == null)
                return false;
            TrivialHashtable explicitInterfaceImplementations = this.explicitInterfaceImplementations;
            if(explicitInterfaceImplementations == null)
            {
                MemberList members = this.Members;
                lock(this)
                {
                    if((explicitInterfaceImplementations = this.explicitInterfaceImplementations) == null)
                    {
                        explicitInterfaceImplementations = this.explicitInterfaceImplementations = new TrivialHashtable();
                        for(int i = 0, n = members.Count; i < n; i++)
                        {
                            Method m = members[i] as Method;
                            if(m == null)
                                continue;
                            MethodList implementedInterfaceMethods = m.ImplementedInterfaceMethods;
                            if(implementedInterfaceMethods != null)
                                for(int j = 0, k = implementedInterfaceMethods.Count; j < k; j++)
                                {
                                    Method im = implementedInterfaceMethods[j];
                                    if(im == null)
                                        continue;
                                    explicitInterfaceImplementations[im.UniqueKey] = m;
                                }
                        }
                    }
                }
            }
            return explicitInterfaceImplementations[method.UniqueKey] != null;
        }

        internal bool ImplementsMethod(Method meth, bool checkPublic)
        {
            return this.GetImplementingMethod(meth, checkPublic) != null;
        }
        public Method GetImplementingMethod(Method meth, bool checkPublic)
        {
            if(meth == null)
                return null;
            MemberList mems = this.GetMembersNamed(meth.Name);
            for(int j = 0, m = mems == null ? 0 : mems.Count; j < m; j++)
            {
                Method locM = mems[j] as Method;
                if(locM == null || !locM.IsVirtual || (checkPublic && !locM.IsPublic))
                    continue;
                if((locM.ReturnType != meth.ReturnType && !(locM.ReturnType != null && locM.ReturnType.IsStructurallyEquivalentTo(meth.ReturnType))) ||
                  !locM.ParametersMatchStructurally(meth.Parameters))
                    continue;
                return locM;
            }
            if(checkPublic && this.BaseType != null)
                return this.BaseType.GetImplementingMethod(meth, true);
            return null;
        }

        /// <summary>
        /// Returns true if the CLR CTS allows a value of this type may be assigned to a variable of the target type (possibly after boxing),
        /// either because the target type is the same or a base type, or because the target type is an interface implemented by this type or the implementor of this type,
        /// or because this type and the target type are zero based single dimensional arrays with assignment compatible reference element types
        /// </summary>
        public virtual bool IsAssignableTo(TypeNode targetType)
        {
            if(this == CoreSystemTypes.Void)
                return false;
            if(targetType == this)
                return true;
            if(this == CoreSystemTypes.Object)
                return false;
            if(targetType == CoreSystemTypes.Object || this.IsStructurallyEquivalentTo(targetType) ||
              this.BaseType != null && (this.BaseType.IsAssignableTo(targetType)))
                return true;
            if(this.BaseType != null && this.ConsolidatedTemplateParameters != null && this.BaseType.Template != null && this.BaseType.Template.IsAssignableTo(targetType))
                return true; //When seeing if one template is assignable to another, be sure to strip off template instances along the inheritance chain
            InterfaceList interfaces = this.Interfaces;
            if(interfaces == null)
                return false;
            for(int i = 0, n = interfaces.Count; i < n; i++)
            {
                Interface iface = interfaces[i];
                if(iface == null)
                    continue;
                if(iface.IsAssignableTo(targetType))
                    return true;
                if(iface.Template != null && this.ConsolidatedTemplateParameters != null && iface.Template.IsAssignableTo(targetType))
                    return true; //When seeing if one template is assignable to another, be sure to strip off template instances along the inheritance chain
            }
            return false;
        }
        /// <summary>
        /// Returns true if this type is assignable to some instance of the given template.
        /// </summary>
        public virtual bool IsAssignableToInstanceOf(TypeNode targetTemplate)
        {
            if(this == CoreSystemTypes.Void || targetTemplate == null)
                return false;
            if(targetTemplate.IsStructurallyEquivalentTo(this.Template == null ? this : this.Template) ||
              this.BaseType != null && (this.BaseType.IsAssignableToInstanceOf(targetTemplate) ||
              this.BaseType.Template != null && this.BaseType.Template.IsAssignableToInstanceOf(targetTemplate)))
                return true;
            InterfaceList interfaces = this.Interfaces;
            if(interfaces == null)
                return false;
            for(int i = 0, n = interfaces.Count; i < n; i++)
            {
                Interface iface = interfaces[i];
                if(iface == null)
                    continue;
                if(iface.IsAssignableToInstanceOf(targetTemplate))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Returns true if this type is assignable to some instance of the given template.
        /// </summary>
        public virtual bool IsAssignableToInstanceOf(TypeNode targetTemplate, out TypeNodeList templateArguments)
        {
            templateArguments = null;
            if(this == CoreSystemTypes.Void || targetTemplate == null)
                return false;
            if(targetTemplate == this.Template)
            {
                templateArguments = this.TemplateArguments;
                return true;
            }
            if(this != CoreSystemTypes.Object && this.BaseType != null && this.BaseType.IsAssignableToInstanceOf(targetTemplate, out templateArguments))
                return true;
            InterfaceList interfaces = this.Interfaces;
            if(interfaces == null)
                return false;
            for(int i = 0, n = interfaces.Count; i < n; i++)
            {
                Interface iface = interfaces[i];
                if(iface == null)
                    continue;
                if(iface.IsAssignableToInstanceOf(targetTemplate, out templateArguments))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Returns true if otherType is the base class of this type or if the base class of this type is derived from otherType.
        /// </summary>
        public virtual bool IsDerivedFrom(TypeNode otherType)
        {
            if(otherType == null)
                return false;
            TypeNode baseType = this.BaseType;
            while(baseType != null)
            {
                if(baseType == otherType)
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }

        //  Not thread safe code...
        bool isCheckingInheritedFrom = false;
        public virtual bool IsInheritedFrom(TypeNode otherType)
        {
            if(otherType == null)
                return false;
            if(this == otherType)
                return true;
            bool result = false;
            if(this.isCheckingInheritedFrom)
                goto done;
            this.isCheckingInheritedFrom = true;
            if(this.Template != null)
            {
                result = this.Template.IsInheritedFrom(otherType);
                goto done;
            }
            if(otherType.Template != null)
            {
                otherType = otherType.Template;
            }
            TypeNode baseType = this.BaseType;
            if(baseType != null && baseType.IsInheritedFrom(otherType))
            {
                result = true;
                goto done;
            }
            InterfaceList interfaces = this.Interfaces;
            if(interfaces == null)
                goto done;
            for(int i = 0, n = interfaces.Count; i < n; i++)
            {
                Interface iface = interfaces[i];
                if(iface == null)
                    continue;
                if(iface.IsInheritedFrom(otherType))
                {
                    result = true;
                    goto done;
                }
            }
done:
            this.isCheckingInheritedFrom = false;
            return result;
        }

        public virtual bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if(null == (object)type)
                return false;
            if(this == type)
                return true;
            if(this.Template == (object)null || type.Template == (object)null)
            {
                if(((object)this) == (object)type.Template || ((object)this.Template) == (object)type)
                    return true;
                Identifier thisName = this.Template == null ? this.Name : this.Template.Name;
                Identifier typeName = type.Template == null ? type.Name : type.Template.Name;
                if(thisName == null || typeName == null || thisName.UniqueIdKey != typeName.UniqueIdKey)
                    return false;
                if(this.NodeType != type.NodeType)
                    return false;
                if(this.DeclaringType == null || type.DeclaringType == null)
                    return false;
            }
            if(this.TemplateArguments == null || type.TemplateArguments == null)
            {
                if(this.DeclaringType != null && (this.TemplateArguments == null || this.TemplateArguments.Count == 0) &&
                  (type.TemplateArguments == null || type.TemplateArguments.Count == 0))
                    return this.DeclaringType.IsStructurallyEquivalentTo(type.DeclaringType);
                return false;
            }
            int n = this.TemplateArguments.Count;
            if(n != type.TemplateArguments.Count)
                return false;
            if(this.Template != type.Template && !this.Template.IsStructurallyEquivalentTo(type.Template))
                return false;
            for(int i = 0; i < n; i++)
            {
                TypeNode ta1 = this.TemplateArguments[i];
                TypeNode ta2 = type.TemplateArguments[i];
                if(null == (object)ta1 || null == (object)ta2)
                    return false;
                if(ta1 == ta2)
                    continue;
                if(!ta1.IsStructurallyEquivalentTo(ta2))
                    return false;
            }
            if(this.DeclaringType != null)
                return this.DeclaringType.IsStructurallyEquivalentTo(type.DeclaringType);
            return true;
        }
        public virtual bool IsStructurallyEquivalentList(TypeNodeList list1, TypeNodeList list2)
        {
            if(list1 == null)
                return list2 == null;
            if(list2 == null)
                return false;
            int n = list1.Count;
            if(list2.Count != n)
                return false;
            for(int i = 0; i < n; i++)
            {
                TypeNode t1 = list1[i];
                TypeNode t2 = list2[i];
                if(null == (object)t1 || null == (object)t2)
                    return false;
                if(t1 == t2)
                    continue;
                if(!t1.IsStructurallyEquivalentTo(t2))
                    return false;
            }
            return true;
        }
        public static TypeNode StripModifiers(TypeNode type)
        //^ ensures t != null ==> result != null;
        {
            for(TypeModifier tmod = type as TypeModifier; tmod != null; tmod = type as TypeModifier)
                type = tmod.ModifiedType;
            // Don't strip under pointers or refs. We only strip top-level modifiers.
            return type;
        }

        public static TypeNode DeepStripModifiers(TypeNode type)
        //^ ensures type != null ==> result != null;
        {
            // strip off any outer type modifiers
            for(TypeModifier tmod = type as TypeModifier; tmod != null; tmod = type as TypeModifier)
                type = tmod.ModifiedType;
            // For arrays and references, strip the inner type and then reconstruct the array or reference
            ArrayType ar = type as ArrayType;
            if(ar != null)
            {
                TypeNode t = TypeNode.DeepStripModifiers(ar.ElementType);
                return t.GetArrayType(1);
            }
            Reference rt = type as Reference;
            if(rt != null)
            {
                TypeNode t = TypeNode.DeepStripModifiers(rt.ElementType);
                return t.GetReferenceType();
            }
            return type;
        }
        /// <summary>
        /// Strip the given modifier from the type, modulo substructures that are instantiated with respect
        /// to the given template type. In other words, traverse type and templateType in parallel, stripping common
        /// non-null modifiers, but stop when reaching a type variable in the template type.
        /// <param name="type">Type to be stripped</param>
        /// <param name="modifiers">Modifiers to strip off</param>
        /// <param name="templateType">Template bounding the stripping of type. Passing null for the templateType performs a full DeepStrip</param>
        /// </summary>
        public static TypeNode DeepStripModifiers(TypeNode type, TypeNode templateType, params TypeNode[] modifiers)
        {
            if(templateType == null)
                return DeepStripModifiers(type, modifiers);
            if(templateType is ITypeParameter)
                return type;
            // strip off inner modifiers then outer type modifier if it matches
            OptionalModifier optmod = type as OptionalModifier;
            if(optmod != null)
            {
                OptionalModifier optmodtemplate = (OptionalModifier)templateType; // must be in sync
                TypeNode t = TypeNode.DeepStripModifiers(optmod.ModifiedType, optmodtemplate.ModifiedType, modifiers);
                for(int i = 0; i < modifiers.Length; ++i)
                {
                    if(optmod.Modifier == modifiers[i])
                    {
                        // strip it
                        return t;
                    }
                }
                return OptionalModifier.For(optmod.Modifier, t);
            }
            RequiredModifier reqmod = type as RequiredModifier;
            if(reqmod != null)
            {
                RequiredModifier reqmodtemplate = (RequiredModifier)templateType; // must be in sync
                TypeNode t = TypeNode.DeepStripModifiers(reqmod.ModifiedType, reqmodtemplate.ModifiedType, modifiers);
                for(int i = 0; i < modifiers.Length; ++i)
                {
                    if(reqmod.Modifier == modifiers[i])
                    {
                        // strip it
                        return t;
                    }
                }
                return RequiredModifier.For(reqmod.Modifier, t);
            }
            // For arrays and references, strip the inner type and then reconstruct the array or reference
            ArrayType ar = type as ArrayType;
            if(ar != null)
            {
                ArrayType artemplate = (ArrayType)templateType;
                TypeNode t = TypeNode.DeepStripModifiers(ar.ElementType, artemplate.ElementType, modifiers);
                return t.GetArrayType(1);
            }
            Reference rt = type as Reference;
            if(rt != null)
            {
                Reference rttemplate = (Reference)templateType;
                TypeNode t = TypeNode.DeepStripModifiers(rt.ElementType, rttemplate.ElementType, modifiers);
                return t.GetReferenceType();
            }
            // strip template arguments
            if(type.Template != null && type.TemplateArguments != null && type.TemplateArguments.Count > 0)
            {
                TypeNodeList strippedArgs = new TypeNodeList();

                for(int i = 0; i < type.TemplateArguments.Count; i++)
                {
                    //FIX: bug introduced by check in 16494 
                    //templateType may have type parameters in either the TemplateArguments position or the templateParameters position.
                    //This may indicate an inconsistency in the template instantiation representation elsewhere.
                    TypeNodeList templateTypeArgs = templateType.TemplateArguments != null ? templateType.TemplateArguments : templateType.TemplateParameters;
                    strippedArgs.Add(DeepStripModifiers(type.TemplateArguments[i], templateTypeArgs[i], modifiers));
                }

                return type.Template.GetTemplateInstance(type, strippedArgs);
            }
            return type;
        }

        public static TypeNode DeepStripModifiers(TypeNode type, params TypeNode[] modifiers)
        {
            // strip off inner modifiers then outer type modifier if it matches
            OptionalModifier optmod = type as OptionalModifier;
            if(optmod != null)
            {
                TypeNode t = TypeNode.DeepStripModifiers(optmod.ModifiedType, modifiers);
                for(int i = 0; i < modifiers.Length; ++i)
                {
                    if(optmod.Modifier == modifiers[i])
                    {
                        // strip it
                        return t;
                    }
                }
                return OptionalModifier.For(optmod.Modifier, t);
            }
            RequiredModifier reqmod = type as RequiredModifier;
            if(reqmod != null)
            {
                TypeNode t = TypeNode.DeepStripModifiers(reqmod.ModifiedType, modifiers);
                for(int i = 0; i < modifiers.Length; ++i)
                {
                    if(reqmod.Modifier == modifiers[i])
                    {
                        // strip it
                        return t;
                    }
                }
                return RequiredModifier.For(reqmod.Modifier, t);
            }
            // For arrays and references, strip the inner type and then reconstruct the array or reference
            ArrayType ar = type as ArrayType;
            if(ar != null)
            {
                TypeNode t = TypeNode.DeepStripModifiers(ar.ElementType, modifiers);
                return t.GetArrayType(1);
            }
            Reference rt = type as Reference;
            if(rt != null)
            {
                TypeNode t = TypeNode.DeepStripModifiers(rt.ElementType, modifiers);
                return t.GetReferenceType();
            }
            // strip template arguments
            if(type.Template != null && type.TemplateArguments != null && type.TemplateArguments.Count > 0)
            {
                TypeNodeList strippedArgs = new TypeNodeList();

                for(int i = 0; i < type.TemplateArguments.Count; i++)
                    strippedArgs.Add(DeepStripModifiers(type.TemplateArguments[i], modifiers));

                return type.Template.GetTemplateInstance(type, strippedArgs);
            }
            return type;
        }

        public static bool HasModifier(TypeNode type, TypeNode modifier)
        {
            // Don't look under pointers or refs.
            TypeModifier tmod = type as TypeModifier;
            if(tmod != null)
            {
                if(tmod.Modifier == modifier)
                    return true;
                return TypeNode.HasModifier(tmod.ModifiedType, modifier);
            }
            return false;
        }
        public static TypeNode StripModifier(TypeNode type, TypeNode modifier)
        {
            // Don't strip under pointers or refs. We only strip top-level modifiers
            TypeModifier tmod = type as TypeModifier;
            if(tmod != null)
            {
                TypeNode et = TypeNode.StripModifier(tmod.ModifiedType, modifier);
                //^ assert et != null;
                if(tmod.Modifier == modifier)
                    return et;
                if(et == tmod.ModifiedType)
                    return tmod;
                if(tmod is OptionalModifier)
                    return OptionalModifier.For(tmod.Modifier, et);
                return RequiredModifier.For(tmod.Modifier, et);
            }
            return type;
        }

        /// <summary>
        /// Needed whenever we change the id of an existing member
        /// </summary>
        public virtual void ClearMemberTable()
        {
            lock(this)
            {
                this.memberTable = null;
                this.memberCount = 0;
            }
        }

        protected virtual void UpdateMemberTable(int range)
        //^ ensures this.memberTable != null;
        {
            MemberList thisMembers = this.Members;
            lock(this)
            {
                if(this.memberTable == null)
                    this.memberTable = new TrivialHashtable(32);
                for(int i = this.memberCount; i < range; i++)
                {
                    Member mem = thisMembers[i];
                    if(mem == null || mem.Name == null)
                        continue;
                    MemberList members = (MemberList)this.memberTable[mem.Name.UniqueIdKey];
                    if(members == null)
                        this.memberTable[mem.Name.UniqueIdKey] = members = new MemberList();
                    members.Add(mem);
                }
                this.memberCount = range;
                this.constructors = null;
            }
        }
        protected static MemberList WeedOutNonSpecialMethods(MemberList members, MethodFlags mask)
        {
            if(members == null)
                return null;
            bool membersOK = true;
            for(int i = 0, n = members.Count; i < n; i++)
            {
                Method m = members[i] as Method;
                if(m == null || (m.Flags & mask) == 0)
                {
                    membersOK = false;
                    break;
                }
            }
            if(membersOK)
                return members;
            MemberList newMembers = new MemberList();
            for(int i = 0, n = members.Count; i < n; i++)
            {
                Method m = members[i] as Method;
                if(m == null || (m.Flags & mask) == 0)
                    continue;
                newMembers.Add(m);
            }
            return newMembers;
        }

        public override void WriteDocumentation(XmlTextWriter xwriter)
        {
            base.WriteDocumentation(xwriter);
            MemberList members = this.Members;
            for(int i = 0, n = members == null ? 0 : members.Count; i < n; i++)
            {
                Member mem = members[i];
                if(mem == null)
                    continue;
                mem.WriteDocumentation(xwriter);
            }
        }

        public override string ToString()
        {
            return this.GetFullUnmangledNameWithTypeParameters();
        }
    }

    public class Class : TypeNode
    {
        internal readonly static Class DoesNotExist = new Class();
        internal readonly static Class Dummy = new Class();
        internal Class baseClass;

        public Class BaseClassExpression;
        public bool IsAbstractSealedContainerForStatics;

        public Class()
            : base(NodeType.Class)
        {
        }
        public Class(NestedTypeProvider provideNestedTypes, TypeAttributeProvider provideAttributes, TypeMemberProvider provideMembers, object handle)
            : base(NodeType.Class, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
        }

        public Class(Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags flags,
          Identifier Namespace, Identifier name, Class baseClass, InterfaceList interfaces, MemberList members)
            : base(declaringModule, declaringType, attributes, flags, Namespace, name, interfaces, members, NodeType.Class)
        {
            this.baseClass = baseClass;
        }

        /// <summary>
        /// The class from which this class has been derived. Null if this class is System.Object.
        /// </summary>
        public virtual Class BaseClass
        {
            get
            {
                return baseClass;
            }
            set
            {
                baseClass = value;
            }
        }

        public override void GetAbstractMethods(MethodList/*!*/ result)
        {
            if(!this.IsAbstract)
                return;
            MethodList candidates = new MethodList();
            if(this.BaseClass != null)
            {
                this.BaseClass.GetAbstractMethods(candidates);
                for(int i = 0, n = candidates.Count; i < n; i++)
                {
                    Method meth = candidates[i];
                    if(!this.ImplementsMethod(meth, false))
                        result.Add(meth);
                }
            }
            //Add any abstract methods declared inside this class
            MemberList members = this.Members;
            for(int i = 0, n = members.Count; i < n; i++)
            {
                Method meth = members[i] as Method;
                if(meth == null)
                    continue;
                if(meth.IsAbstract)
                    result.Add(meth);
            }
            //For each interface, get abstract methods and keep those that are not implemented by this class or a base class
            InterfaceList interfaces = this.Interfaces;
            if(interfaces != null)
                for(int i = 0, n = interfaces.Count; i < n; i++)
                {
                    Interface iface = interfaces[i];
                    if(iface == null)
                        continue;
                    MemberList imembers = iface.Members;
                    if(imembers == null)
                        continue;
                    for(int j = 0, m = imembers.Count; j < m; j++)
                    {
                        Method meth = imembers[j] as Method;
                        if(meth == null)
                            continue;
                        if(this.ImplementsExplicitly(meth))
                            continue;
                        if(this.ImplementsMethod(meth, true))
                            continue;
                        if(this.AlreadyInList(result, meth))
                            continue;
                        result.Add(meth);
                    }
                }
        }
        protected bool AlreadyInList(MethodList list, Method method)
        {
            if(list == null)
                return false;
            for(int i = 0, n = list.Count; i < n; i++)
            {
                if(list[i] == method)
                    return true;
            }
            return false;
        }
    }

    public class ClosureClass : Class
    {
        public ClosureClass()
        {
        }
    }
    /// <summary>
    /// Does not model a real type, but leverages the symbol table methods of Class. In other words, this is implementation inheritance, not an ISA relationship.
    /// </summary>
    //TODO: use delegation rather than inheritance to achieve this
    public class Scope : Class
    {
        public Scope()
        {
        }
        public Scope(Scope outerScope)
        {
            this.OuterScope = outerScope;
        }
        protected Scope outerScope;
        public SourceContext LexicalSourceExtent;
        public Scope OuterScope
        {
            get
            {
                if(this.outerScope == null)
                    this.outerScope = (Scope)this.baseClass;
                return this.outerScope;
            }
            set
            {
                this.baseClass = this.outerScope = value;
            }
        }
        public virtual TypeNode GetType(Identifier typeName)
        {
            return this.GetNestedType(typeName);
        }
    }
    public class TypeScope : Scope
    {
        public TypeNode Type;
        public TypeScope() { }
        public TypeScope(Scope parentScope, TypeNode/*!*/ type)
        {
            this.baseClass = parentScope;
            this.DeclaringModule = type.DeclaringModule;
            this.Type = type;
            if(type != null && type.PartiallyDefines != null)
                this.Type = type.PartiallyDefines;
            this.templateParameters = type.TemplateParameters;
            if(type != null)
                this.LexicalSourceExtent = type.SourceContext;
        }

        public override MemberList/*!*/ GetMembersNamed(Identifier name)
        {
            TypeNode t = this.Type;
            MemberList result = null;

            while(t != null)
            {
                result = t.GetMembersNamed(name);

                if(result.Count > 0)
                    return result;

                t = t.BaseType;
            }

            if(result != null)
                return result;

            return new MemberList();
        }

        public override MemberList Members
        {
            get
            {
                return this.Type.Members;
            }
            set
            {
                base.Members = value;
            }
        }
    }
    public class MethodScope : Scope
    {
        protected Class closureClass;
        public virtual Class ClosureClass
        {
            get
            {
                //if (this.DeclaringMethod == null) return null;
                Class c = this.closureClass;

                if(c == null)
                {
                    c = this.closureClass = new ClosureClass();
                    c.Name = Identifier.For("closure:" + this.UniqueKey);
                    c.BaseClass = CoreSystemTypes.Object;
                    Class bclass = this.BaseClass;
                    c.DeclaringModule = bclass.DeclaringModule;
                    TypeScope tscope = bclass as TypeScope;

                    if(tscope != null)
                        c.DeclaringType = tscope.Type;
                    else
                    {
                        MethodScope mscope = bclass as MethodScope;

                        if(mscope != null)
                            c.DeclaringType = mscope.ClosureClass;
                        else
                            c.DeclaringType = ((BlockScope)bclass).ClosureClass;
                    }

                    c.IsGeneric = c.DeclaringType.IsGeneric || this.DeclaringMethod.IsGeneric;
                    c.TemplateParameters = this.CopyMethodTemplateParameters(c.DeclaringModule, c.DeclaringType);
                    c.Flags = TypeFlags.NestedPrivate | TypeFlags.SpecialName | TypeFlags.Sealed;
                    c.Interfaces = new InterfaceList();

                    if(this.ThisType != null)
                    {
                        Field f = new Field(c, null, FieldFlags.CompilerControlled | FieldFlags.SpecialName, StandardIds.ThisValue, this.ThisType, null);
                        this.ThisField = f;
                        c.Members.Add(f);
                    }
                }

                return c;
            }
        }
        private TypeNodeList CopyMethodTemplateParameters(Module/*!*/ module, TypeNode/*!*/ type)
        //^ requires this.DeclaringMethod != null;
        {
            TypeNodeList methTemplParams = this.DeclaringMethod.TemplateParameters;
            if(methTemplParams == null || methTemplParams.Count == 0)
                return null;
            this.tpDup = new TemplateParameterDuplicator(module, type);
            return this.tpDup.VisitTypeParameterList(methTemplParams);
        }
        private TemplateParameterDuplicator tpDup;
        private class TemplateParameterDuplicator : Duplicator
        {
            public TemplateParameterDuplicator(Module/*!*/ module, TypeNode/*!*/ type)
                : base(module, type)
            {
            }

            public override TypeNode VisitTypeParameter(TypeNode typeParameter)
            {
                if(typeParameter == null)
                    return null;
                TypeNode result = (TypeNode)this.DuplicateFor[typeParameter.UniqueKey];
                if(result != null)
                    return result;
                MethodTypeParameter mtp = typeParameter as MethodTypeParameter;
                if(mtp != null)
                {
                    TypeParameter tp = new TypeParameter();
                    this.DuplicateFor[typeParameter.UniqueKey] = tp;
                    tp.Name = mtp.Name;
                    tp.Interfaces = this.VisitInterfaceReferenceList(mtp.Interfaces);
                    tp.TypeParameterFlags = mtp.TypeParameterFlags;
                    tp.DeclaringModule = mtp.DeclaringModule;
                    tp.DeclaringMember = this.TargetType;
                    result = tp;
                }
                else
                {
                    MethodClassParameter mcp = typeParameter as MethodClassParameter;
                    if(mcp != null)
                    {
                        ClassParameter cp = new ClassParameter();
                        this.DuplicateFor[typeParameter.UniqueKey] = cp;
                        cp.Name = mcp.Name;
                        cp.BaseClass = (Class)this.VisitTypeReference(mcp.BaseClass);
                        cp.Interfaces = this.VisitInterfaceReferenceList(mcp.Interfaces);
                        cp.TypeParameterFlags = mcp.TypeParameterFlags;
                        cp.DeclaringModule = mcp.DeclaringModule;
                        cp.DeclaringMember = this.TargetType;
                        result = cp;
                    }
                }
                if(result == null)
                    return typeParameter;
                return result;
            }
            public override TypeNode VisitTypeReference(TypeNode type)
            {
                TypeNode result = base.VisitTypeReference(type);
                if(result == type && (type is MethodClassParameter || type is MethodTypeParameter))
                    return this.VisitTypeParameter(type);
                return result;
            }
        }
        public virtual Class ClosureClassTemplateInstance
        {
            get
            {
                if(this.closureClassTemplateInstance == null)
                {
                    if(this.DeclaringMethod == null || !this.DeclaringMethod.IsGeneric)
                        this.closureClassTemplateInstance = this.ClosureClass;
                    else
                        this.closureClassTemplateInstance = (Class)this.ClosureClass.GetTemplateInstance(this.DeclaringMethod.DeclaringType, this.DeclaringMethod.TemplateParameters);
                }
                return this.closureClassTemplateInstance;
            }
        }
        Class closureClassTemplateInstance;
        public TypeNode FixTypeReference(TypeNode type)
        {
            if(this.tpDup == null)
                return type;
            return this.tpDup.VisitTypeReference(type);
        }

        public virtual Boolean CapturedForClosure
        {
            get
            {
                return this.closureClass != null;
            }
        }
        public UsedNamespaceList UsedNamespaces;
        public Field ThisField;
        public TypeNode ThisType;
        public TypeNode ThisTypeInstance;
        public Method DeclaringMethod;
        public MethodScope() { }
        public MethodScope(Class/*!*/ parentScope, UsedNamespaceList usedNamespaces)
            : this(parentScope, usedNamespaces, null)
        {
        }
        public MethodScope(Class/*!*/ parentScope, UsedNamespaceList usedNamespaces, Method method)
        {
            this.baseClass = parentScope;
            this.UsedNamespaces = usedNamespaces;
            this.DeclaringModule = parentScope.DeclaringModule;
            this.DeclaringMethod = method;
            if(method != null && (method.Flags & MethodFlags.Static) == 0)
                this.ThisType = this.ThisTypeInstance = method.DeclaringType;
            if(method != null)
                this.LexicalSourceExtent = method.SourceContext;
        }
    }
    public class BlockScope : Scope
    {
        public Block AssociatedBlock;
        public bool MembersArePinned;
        public virtual Class ClosureClass
        {
            get
            {
                BlockScope bscope = this.BaseClass as BlockScope;
                if(bscope != null)
                    return bscope.ClosureClass;
                MethodScope mscope = this.BaseClass as MethodScope;
                if(mscope != null)
                    return mscope.ClosureClass;
                return ((TypeScope)this.BaseClass).Type as Class;
            }
        }
        public virtual Boolean CapturedForClosure
        {
            get
            {
                BlockScope bscope = this.BaseClass as BlockScope;
                if(bscope != null)
                    return bscope.CapturedForClosure;
                MethodScope mscope = this.BaseClass as MethodScope;
                if(mscope != null)
                    return mscope.CapturedForClosure;
                return false;
            }
        }
        public BlockScope()
        {
        }
        public BlockScope(Scope/*!*/ parentScope, Block associatedBlock)
        {
            this.AssociatedBlock = associatedBlock;
            if(associatedBlock != null)
            {
                associatedBlock.HasLocals = true; //TODO: set only if there really are locals
                associatedBlock.Scope = this;
            }
            this.baseClass = parentScope;
            this.DeclaringModule = parentScope.DeclaringModule;
            if(associatedBlock != null)
                this.LexicalSourceExtent = associatedBlock.SourceContext;
        }
    }
    public class AttributeScope : Scope
    {
        public AttributeNode AssociatedAttribute;
        public AttributeScope(Scope parentScope, AttributeNode associatedAttribute)
        {
            this.AssociatedAttribute = associatedAttribute;
            this.baseClass = parentScope;
            if(associatedAttribute != null)
                this.LexicalSourceExtent = associatedAttribute.SourceContext;
        }
    }
    public class NamespaceScope : Scope
    {
        public Namespace AssociatedNamespace;
        public Module AssociatedModule;
        public TrivialHashtable AliasedType;
        public TrivialHashtable AliasedNamespace;
        protected TrivialHashtable/*!*/ aliasFor = new TrivialHashtable();
        protected TrivialHashtable/*!*/ typeFor = new TrivialHashtable();
        protected TrivialHashtable/*!*/ namespaceFor = new TrivialHashtable();
        protected TrivialHashtable/*!*/ nestedNamespaceFullName = new TrivialHashtable();
        protected readonly static AliasDefinition/*!*/ noSuchAlias = new AliasDefinition();

        public NamespaceScope()
        {
        }
        public NamespaceScope(Scope outerScope, Namespace associatedNamespace, Module associatedModule)
            : base(outerScope)
        {
            //^ base;
            this.AssociatedNamespace = associatedNamespace;
            this.AssociatedModule = associatedModule;
            this.DeclaringModule = associatedModule; //TODO: make this go away
            if(associatedNamespace != null)
                this.LexicalSourceExtent = associatedNamespace.SourceContext;
        }
        public virtual AliasDefinition GetAliasFor(Identifier name)
        {
            if(name == null || this.AssociatedNamespace == null || this.AssociatedModule == null || this.aliasFor == null)
            {
                Debug.Assert(false);
                return null;
            }
            AliasDefinition alias = (AliasDefinition)this.aliasFor[name.UniqueIdKey];
            if(alias == noSuchAlias)
                return null;
            if(alias != null)
                return alias;
            //Check if there is an alias with this uri
            Scope scope = this;
            while(scope != null)
            {
                NamespaceScope nsScope = scope as NamespaceScope;
                if(nsScope != null && nsScope.AssociatedNamespace != null)
                {
                    AliasDefinitionList aliases = nsScope.AssociatedNamespace.AliasDefinitions;
                    if(aliases != null)
                        for(int i = 0, n = aliases.Count; i < n; i++)
                        {
                            AliasDefinition aliasDef = aliases[i];
                            if(aliasDef == null || aliasDef.Alias == null)
                                continue;
                            if(aliasDef.Alias.UniqueIdKey == name.UniqueIdKey) { alias = aliasDef; goto done; }
                        }
                }
                scope = scope.OuterScope;
            }
done:
            if(alias != null)
                this.aliasFor[name.UniqueIdKey] = alias;
            else
                this.aliasFor[name.UniqueIdKey] = noSuchAlias;
            return alias;
        }
        public virtual AliasDefinition GetConflictingAlias(Identifier name)
        {
            if(name == null || this.typeFor == null || this.AssociatedNamespace == null || this.AssociatedModule == null)
            {
                Debug.Assert(false);
                return null;
            }
            TypeNode type = this.AssociatedModule.GetType(this.AssociatedNamespace.FullNameId, name);
            if(type != null)
            {
                AliasDefinitionList aliases = this.AssociatedNamespace.AliasDefinitions;
                for(int i = 0, n = aliases == null ? 0 : aliases.Count; i < n; i++)
                {
                    //^ assert aliases != null;
                    AliasDefinition aliasDef = aliases[i];
                    if(aliasDef == null || aliasDef.Alias == null)
                        continue;
                    if(aliasDef.Alias.UniqueIdKey == name.UniqueIdKey)
                        return aliasDef;
                }
            }
            Scope scope = this;
            while(scope != null)
            {
                NamespaceScope outerScope = scope.OuterScope as NamespaceScope;
                if(outerScope != null)
                    return outerScope.GetConflictingAlias(name);
                scope = scope.OuterScope;
            }
            return null;
        }
        public virtual Identifier GetUriFor(Identifier name)
        {
            AliasDefinition aliasDef = this.GetAliasFor(name);
            if(aliasDef == null)
                return null;
            return aliasDef.AliasedUri;
        }
        public virtual Identifier GetNamespaceFullNameFor(Identifier name)
        {
            if(name == null || this.AssociatedNamespace == null || this.AssociatedModule == null || this.nestedNamespaceFullName == null)
            {
                Debug.Assert(false);
                return null;
            }
            Identifier fullName = (Identifier)this.nestedNamespaceFullName[name.UniqueIdKey];
            if(fullName == Identifier.Empty)
                return null;
            if(fullName != null)
                return fullName;
            //Check if there is an alias with this namespace
            AliasDefinition aliasDef = this.GetAliasFor(name);
            if(aliasDef != null && aliasDef.AliasedUri == null && aliasDef.AliasedType == null)
                return aliasDef.AliasedExpression as Identifier;
            //Check if module has a type with namespace equal to this namespace + name
            fullName = name;
            if(this.AssociatedNamespace.Name != null && this.AssociatedNamespace.Name.UniqueIdKey != Identifier.Empty.UniqueIdKey)
                fullName = Identifier.For(this.AssociatedNamespace.FullName + "." + name);
            if(this.AssociatedModule.IsValidNamespace(fullName))
            {
                this.namespaceFor[fullName.UniqueIdKey] = new TrivialHashtable();
                goto returnFullName;
            }
            // If an inner type shadows an outer namespace, don't return the namespace
            if(this.AssociatedModule.IsValidTypeName(this.AssociatedNamespace.Name, name)) { return null; }
            AssemblyReferenceList arefs = this.AssociatedModule.AssemblyReferences;
            for(int i = 0, n = arefs == null ? 0 : arefs.Count; i < n; i++)
            {
                AssemblyReference ar = arefs[i];
                if(ar == null || ar.Assembly == null)
                    continue;
                if(ar.Assembly.IsValidNamespace(fullName))
                    goto returnFullName;
                // If an inner type shadows an outer namespace, don't return the namespace
                if(ar.Assembly.IsValidTypeName(this.AssociatedNamespace.Name, name)) { return null; }
            }
            ModuleReferenceList mrefs = this.AssociatedModule.ModuleReferences;
            if(mrefs != null)
                for(int i = 0, n = mrefs.Count; i < n; i++)
                {
                    ModuleReference mr = mrefs[i];
                    if(mr == null || mr.Module == null)
                        continue;
                    if(mr.Module.IsValidNamespace(fullName))
                        goto returnFullName;
                    // If an inner type shadows an outer namespace, don't return the namespace
                    if(mr.Module.IsValidTypeName(this.AssociatedNamespace.Name, name)) { return null; }
                }
            Scope scope = this.OuterScope;
            while(scope != null && !(scope is NamespaceScope))
                scope = scope.OuterScope;
            if(scope != null)
                return ((NamespaceScope)scope).GetNamespaceFullNameFor(name);
            return null;
returnFullName:
            this.nestedNamespaceFullName[name.UniqueIdKey] = fullName;
            return fullName;
        }
        /// <summary>
        /// Search this namespace for a type with this name nested in the given namespace. Also considers used name spaces.
        /// If more than one type is found, a list is returned in duplicates.
        /// </summary>
        public virtual TypeNode GetType(Identifier Namespace, Identifier name, out TypeNodeList duplicates)
        {
            duplicates = null;
            if(Namespace == null || name == null || this.AssociatedNamespace == null || this.AssociatedModule == null)
            {
                Debug.Assert(false);
                return null;
            }
            if(this.namespaceFor == null)
            {
                Debug.Assert(false);
                this.namespaceFor = new TrivialHashtable();
            }
            TrivialHashtable typeFor = (TrivialHashtable)this.namespaceFor[Namespace.UniqueIdKey];
            if(typeFor == null)
                this.namespaceFor[Namespace.UniqueIdKey] = typeFor = new TrivialHashtable();
            TypeNode result = (TypeNode)typeFor[name.UniqueIdKey];
            if(result == Class.DoesNotExist)
                return null;
            if(result != null)
                return result;
            //If the associated module declares a type with the given name in a nested namespace, it wins
            Scope scope = this;
            while(scope != null)
            {
                NamespaceScope nsScope = scope as NamespaceScope;
                if(nsScope != null && nsScope.AssociatedNamespace != null)
                {
                    Identifier nestedNamespace = Namespace;
                    if(nsScope.AssociatedNamespace.FullNameId != null && nsScope.AssociatedNamespace.FullNameId.UniqueIdKey != Identifier.Empty.UniqueIdKey)
                        nestedNamespace = Identifier.For(nsScope.AssociatedNamespace.FullName + "." + Namespace);
                    result = this.AssociatedModule.GetType(nestedNamespace, name);
                    if(result != null)
                        break;
                }
                scope = scope.OuterScope;
            }
            if(result == null)
            {
                //Now get into situations where there might be duplicates.
                duplicates = new TypeNodeList();
                //Check the used namespaces of this and outer namespace scopes
                TrivialHashtable alreadyUsed = new TrivialHashtable();
                scope = this;
                while(scope != null)
                {
                    NamespaceScope nsScope = scope as NamespaceScope;
                    if(nsScope != null && nsScope.AssociatedNamespace != null)
                    {
                        UsedNamespaceList usedNamespaces = nsScope.AssociatedNamespace.UsedNamespaces;
                        int n = usedNamespaces == null ? 0 : usedNamespaces.Count;
                        if(usedNamespaces != null)
                            for(int i = 0; i < n; i++)
                            {
                                UsedNamespace usedNs = usedNamespaces[i];
                                if(usedNs == null || usedNs.Namespace == null)
                                    continue;
                                int key = usedNs.Namespace.UniqueIdKey;
                                if(alreadyUsed[key] != null)
                                    continue;
                                alreadyUsed[key] = usedNs.Namespace;
                                Identifier usedNestedNamespace = Identifier.For(usedNs.Namespace + "." + Namespace);
                                result = this.AssociatedModule.GetType(usedNestedNamespace, name);
                                if(result != null)
                                    duplicates.Add(result);
                            }
                    }
                    scope = scope.OuterScope;
                }
                if(duplicates.Count > 0)
                    result = duplicates[0];
            }
            if(result == null)
            {
                //The associated module does not have a type by this name, so check its referenced modules and assemblies
                int numDups = 0;
                //Check this namespace and outer namespaces
                scope = this;
                while(scope != null && result == null)
                {
                    NamespaceScope nsScope = scope as NamespaceScope;
                    if(nsScope != null && nsScope.AssociatedNamespace != null)
                    {
                        Identifier nestedNamespace = Namespace;
                        if(nsScope.AssociatedNamespace.FullNameId != null && nsScope.AssociatedNamespace.FullNameId.UniqueIdKey != Identifier.Empty.UniqueIdKey)
                            nestedNamespace = Identifier.For(nsScope.AssociatedNamespace.FullName + "." + Namespace);
                        nsScope.GetReferencedTypes(nestedNamespace, name, duplicates);
                        numDups = duplicates.Count;
                        for(int i = numDups - 1; i >= 0; i--)
                        {
                            TypeNode dup = duplicates[i];
                            if(dup == null || !dup.IsPublic)
                                numDups--;
                            result = dup;
                        }
                    }
                    scope = scope.OuterScope;
                }
                if(numDups == 0)
                {
                    if(duplicates.Count > 0)
                        duplicates = new TypeNodeList();
                    //Check the used namespaces of this and outer namespace scopes
                    TrivialHashtable alreadyUsed = new TrivialHashtable();
                    scope = this;
                    while(scope != null)
                    {
                        NamespaceScope nsScope = scope as NamespaceScope;
                        if(nsScope != null && nsScope.AssociatedNamespace != null)
                        {
                            UsedNamespaceList usedNamespaces = this.AssociatedNamespace.UsedNamespaces;
                            int n = usedNamespaces == null ? 0 : usedNamespaces.Count;
                            if(usedNamespaces != null)
                                for(int i = 0; i < n; i++)
                                {
                                    UsedNamespace usedNs = usedNamespaces[i];
                                    if(usedNs == null)
                                        continue;
                                    int key = usedNs.Namespace.UniqueIdKey;
                                    if(alreadyUsed[key] != null)
                                        continue;
                                    alreadyUsed[key] = usedNs.Namespace;
                                    Identifier usedNestedNamespace = Identifier.For(usedNs.Namespace + "." + Namespace);
                                    this.GetReferencedTypes(usedNestedNamespace, name, duplicates);
                                }
                        }
                        scope = scope.OuterScope;
                    }
                    numDups = duplicates.Count;
                    for(int i = numDups - 1; i >= 0; i--)
                    {
                        TypeNode dup = duplicates[i];
                        if(dup == null || !dup.IsPublic)
                            numDups--;
                        result = dup;
                    }
                }
                if(numDups <= 1)
                    duplicates = null;
            }
            if(result == null)
                typeFor[name.UniqueIdKey] = Class.DoesNotExist;
            else
                typeFor[name.UniqueIdKey] = result;
            return result;
        }
        /// <summary>
        /// Searches this namespace for a type with this name. Also considers aliases and used name spaces, including those of outer namespaces.
        /// If more than one type is found, a list is returned in duplicates. Types defined in the associated
        /// module mask types defined in referenced modules and assemblies. Results are cached and duplicates are returned only when
        /// there is a cache miss.
        /// </summary>
        public virtual TypeNode GetType(Identifier name, out TypeNodeList duplicates)
        {
            return this.GetType(name, out duplicates, false);
        }
        public virtual TypeNode GetType(Identifier name, out TypeNodeList duplicates, bool returnNullIfHiddenByNestedNamespace)
        {
            duplicates = null;
            if(name == null || this.typeFor == null || this.AssociatedNamespace == null || this.AssociatedModule == null)
            {
                Debug.Assert(false);
                return null;
            }
            AssemblyNode associatedAssembly = this.AssociatedModule as AssemblyNode;
            TypeNode result = (TypeNode)this.typeFor[name.UniqueIdKey];
            if(result == Class.DoesNotExist)
                return null;
            if(result != null)
                return result;
            //If the associated module declares a type with the given name in this namespace, it wins
            result = this.AssociatedModule.GetType(this.AssociatedNamespace.FullNameId, name);
            if(result == null && returnNullIfHiddenByNestedNamespace)
            {
                //Do not proceed to outer namespaces or look at aliases. The nested namespace hides these.
                Identifier fullName = name;
                if(this.AssociatedNamespace.FullName != null && this.AssociatedNamespace.Name.UniqueIdKey != Identifier.Empty.UniqueIdKey)
                    fullName = Identifier.For(this.AssociatedNamespace.FullName + "." + name);
                if(this.AssociatedModule.IsValidNamespace(fullName))
                    result = Class.DoesNotExist;
            }
            if(result == null)
            {
                //If the namespace (or an outer namespace) has an alias definition with this name it wins. (Expected to be mutually exclusive with above.)        
                Scope scope = this;
                while(scope != null && result == null)
                {
                    NamespaceScope nsScope = scope as NamespaceScope;
                    if(nsScope != null && nsScope.AliasedType != null)
                        result = (TypeNode)nsScope.AliasedType[name.UniqueIdKey];
                    if(result == null && returnNullIfHiddenByNestedNamespace && nsScope != null &&
                    nsScope.AliasedNamespace != null && nsScope.AliasedNamespace[name.UniqueIdKey] != null)
                        result = Class.DoesNotExist;
                    scope = scope.OuterScope;
                }
            }
            if(result == null)
            {
                //Now get into situations where there might be duplicates.
                duplicates = new TypeNodeList();
                //Check the used namespaces of this and outer namespace scopes
                TrivialHashtable alreadyUsed = new TrivialHashtable();
                Scope scope = this;
                while(scope != null)
                {
                    NamespaceScope nsScope = scope as NamespaceScope;
                    if(nsScope != null && nsScope.AssociatedNamespace != null && nsScope.AssociatedModule != null)
                    {
                        UsedNamespaceList usedNamespaces = nsScope.AssociatedNamespace.UsedNamespaces;
                        int n = usedNamespaces == null ? 0 : usedNamespaces.Count;
                        if(usedNamespaces != null)
                            for(int i = 0; i < n; i++)
                            {
                                UsedNamespace usedNs = usedNamespaces[i];
                                if(usedNs == null || usedNs.Namespace == null)
                                    continue;
                                int key = usedNs.Namespace.UniqueIdKey;
                                if(alreadyUsed[key] != null)
                                    continue;
                                alreadyUsed[key] = usedNs.Namespace;
                                result = this.AssociatedModule.GetType(usedNs.Namespace, name);
                                //^ assert duplicates != null;
                                if(result != null)
                                    duplicates.Add(result);
                            }
                    }
                    if(returnNullIfHiddenByNestedNamespace)
                        break;
                    scope = scope.OuterScope;
                }
                if(duplicates.Count > 0)
                    result = duplicates[0];
            }
            if(result == null)
                //First see if the current module has a class by this name in the empty namespace
                result = this.AssociatedModule.GetType(Identifier.Empty, name);
            if(result == null)
            {
                //The associated module does not have a type by this name, so check its referenced modules and assemblies
                //First check this namespace
                this.GetReferencedTypes(this.AssociatedNamespace.FullNameId, name, duplicates);
                int numDups = duplicates.Count;
                if(numDups == 1)
                {
                    result = duplicates[0];
                    if(this.IsNotAccessible(associatedAssembly, result)) { numDups--; result = null; }
                }
                else
                {
                    for(int i = numDups - 1; i >= 0; i--)
                    {
                        TypeNode dup = duplicates[i];
                        if(this.IsNotAccessible(associatedAssembly, dup)) { numDups--; continue; }
                        result = dup;
                    }
                    if(numDups == 0 && duplicates.Count > 0)
                    {
                        result = duplicates[0];
                        numDups = duplicates.Count;
                    }
                }
                if(numDups == 0)
                {
                    if(duplicates.Count > 0)
                        duplicates = new TypeNodeList();
                    //Check the used namespaces of this and outer namespace scopes
                    TrivialHashtable alreadyUsed = new TrivialHashtable();
                    Scope scope = this;
                    while(scope != null)
                    {
                        NamespaceScope nsScope = scope as NamespaceScope;
                        if(nsScope != null)
                        {
                            UsedNamespaceList usedNamespaces = nsScope.AssociatedNamespace.UsedNamespaces;
                            int n = usedNamespaces == null ? 0 : usedNamespaces.Count;
                            if(usedNamespaces != null)
                                for(int i = 0; i < n; i++)
                                {
                                    UsedNamespace usedNs = usedNamespaces[i];
                                    if(usedNs == null || usedNs.Namespace == null)
                                        continue;
                                    int key = usedNs.Namespace.UniqueIdKey;
                                    if(alreadyUsed[key] != null)
                                        continue;
                                    alreadyUsed[key] = usedNs.Namespace;
                                    this.GetReferencedTypes(usedNs.Namespace, name, duplicates);
                                }
                        }
                        scope = scope.OuterScope;
                        if(returnNullIfHiddenByNestedNamespace)
                            break;
                    }
                    numDups = duplicates.Count;
                    for(int i = numDups - 1; i >= 0; i--)
                    {
                        TypeNode dup = duplicates[i];
                        if(this.IsNotAccessible(associatedAssembly, dup))
                        {
                            numDups--;
                            continue;
                        }
                        result = dup;
                    }
                }
                if(numDups == 0)
                {
                    if(duplicates.Count > 0)
                        duplicates = new TypeNodeList();
                    this.GetReferencedTypes(Identifier.Empty, name, duplicates);
                    numDups = duplicates.Count;
                    for(int i = numDups - 1; i >= 0; i--)
                    {
                        TypeNode dup = duplicates[i];
                        if(this.IsNotAccessible(associatedAssembly, dup))
                        {
                            numDups--;
                            continue;
                        }
                        result = dup;
                    }
                }
                if(numDups <= 1)
                    duplicates = null;
            }
            if(result == null)
                this.typeFor[name.UniqueIdKey] = Class.DoesNotExist;
            else
                this.typeFor[name.UniqueIdKey] = result;
            if(result == Class.DoesNotExist)
                return null;
            if(duplicates != null && duplicates.Count > 1 && this.AssociatedNamespace != null && this.AssociatedNamespace.Name != null && this.AssociatedNamespace.Name.Name != null)
            {
                result = null;
                for(int i = 0, n = duplicates.Count; i < n; i++)
                {
                    TypeNode t = duplicates[i];
                    if(t == null || t.Namespace == null)
                        continue;
                    if(this.AssociatedNamespace.Name.Name.StartsWith(t.Namespace.Name))
                    {
                        if(result != null)
                        {
                            result = null;
                            break;
                        }
                        result = t;
                    }
                }
                if(result != null)
                    duplicates = null;
                else
                    result = duplicates[0];
            }
            return result;
        }
        private bool IsNotAccessible(AssemblyNode associatedAssembly, TypeNode dup)
        {
            if(dup == null)
                return false;
            return !dup.IsPublic && (associatedAssembly == null ||
                    !associatedAssembly.MayAccessInternalTypesOf(dup.DeclaringModule as AssemblyNode)) && !this.AssociatedModule.ContainsModule(dup.DeclaringModule);
        }
        /// <summary>
        /// Searches the module and assembly references of the associated module to find types
        /// </summary>
        public virtual void GetReferencedTypes(Identifier Namespace, Identifier name, TypeNodeList types)
        {
            if(Namespace == null || name == null || types == null || this.AssociatedModule == null) { Debug.Assert(false); return; }
            AssemblyReferenceList arefs = this.AssociatedModule.AssemblyReferences;
            for(int i = 0, n = arefs == null ? 0 : arefs.Count; i < n; i++)
            {
                AssemblyReference ar = arefs[i];
                if(ar == null || ar.Assembly == null)
                    continue;
                TypeNode t = ar.Assembly.GetType(Namespace, name);
                if(t == null)
                    continue;
                //TODO: deal with type forwarding
                types.Add(t);
            }
            ModuleReferenceList mrefs = this.AssociatedModule.ModuleReferences;
            if(mrefs != null)
                for(int i = 0, n = mrefs.Count; i < n; i++)
                {
                    ModuleReference mr = mrefs[i];
                    if(mr == null || mr.Module == null)
                        continue;
                    TypeNode t = mr.Module.GetType(Namespace, name);
                    if(t == null)
                        continue;
                    types.Add(t);
                }
        }
    }

    public class DelegateNode : TypeNode
    {
        internal static readonly DelegateNode/*!*/ Dummy = new DelegateNode();
        protected ParameterList parameters;
        public virtual ParameterList Parameters
        {
            get
            {
                ParameterList pList = this.parameters;
                if(pList == null)
                {
                    MemberList members = this.Members; //Evaluate for side effect
                    if(members != null)
                        members = null;
                    lock(this)
                    {
                        if(this.parameters != null)
                            return this.parameters;
                        MemberList invokers = this.GetMembersNamed(StandardIds.Invoke);
                        for(int i = 0, n = invokers.Count; i < n; i++)
                        {
                            Method m = invokers[i] as Method;
                            if(m == null)
                                continue;
                            this.parameters = pList = m.Parameters;
                            this.returnType = m.ReturnType;
                            break;
                        }
                    }
                }
                return pList;
            }
            set
            {
                this.parameters = value;
            }
        }
        protected TypeNode returnType;
        public virtual TypeNode ReturnType
        {
            get
            {
                TypeNode rt = this.returnType;
                if(rt == null)
                {
                    ParameterList pars = this.Parameters; //Evaluate for side effect
                    if(pars != null)
                        pars = null;
                    rt = this.returnType;
                }
                return rt;
            }
            set
            {
                this.returnType = value;
            }
        }

        public TypeNode ReturnTypeExpression;

        public DelegateNode()
            : base(NodeType.DelegateNode)
        {
        }
        public DelegateNode(NestedTypeProvider provideNestedTypes, TypeAttributeProvider provideAttributes, TypeMemberProvider provideMembers, object handle)
            : base(NodeType.DelegateNode, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
        }

        public DelegateNode(Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags flags,
          Identifier Namespace, Identifier name, TypeNode returnType, ParameterList parameters)
            : base(declaringModule, declaringType, attributes, flags, Namespace, name, null, null, NodeType.DelegateNode)
        {
            this.parameters = parameters;
            this.returnType = returnType;
        }

        private bool membersAlreadyProvided;

        public virtual void ProvideMembers()
        {
            if(this.membersAlreadyProvided)
                return;

            this.membersAlreadyProvided = true;
            this.memberCount = 0;
            MemberList members = this.members = new MemberList();

            //ctor
            ParameterList parameters = new ParameterList();
            parameters.Add(new Parameter(null, ParameterFlags.None, StandardIds.Object, CoreSystemTypes.Object, null, null));
            parameters.Add(new Parameter(null, ParameterFlags.None, StandardIds.Method, CoreSystemTypes.IntPtr, null, null));
            InstanceInitializer ctor = new InstanceInitializer(this, null, parameters, null);
            ctor.Flags |= MethodFlags.Public | MethodFlags.HideBySig;
            ctor.CallingConvention = CallingConventionFlags.HasThis;
            ctor.ImplFlags = MethodImplFlags.Runtime;
            members.Add(ctor);

            //Invoke
            Method invoke = new Method(this, null, StandardIds.Invoke, this.Parameters, this.ReturnType, null);
            invoke.Flags = MethodFlags.Public | MethodFlags.HideBySig | MethodFlags.Virtual;
            invoke.CallingConvention = CallingConventionFlags.HasThis;
            invoke.ImplFlags = MethodImplFlags.Runtime;
            members.Add(invoke);

            //BeginInvoke
            ParameterList dparams = this.parameters;
            int n = dparams == null ? 0 : dparams.Count;
            parameters = new ParameterList();

            for(int i = 0; i < n; i++)
            {
                //^ assert dparams != null;
                Parameter p = dparams[i];
                if(p == null)
                    continue;
                parameters.Add((Parameter)p.Clone());
            }

            parameters.Add(new Parameter(null, ParameterFlags.None, StandardIds.callback, SystemTypes.AsyncCallback, null, null));
            parameters.Add(new Parameter(null, ParameterFlags.None, StandardIds.Object, CoreSystemTypes.Object, null, null));

            Method beginInvoke = new Method(this, null, StandardIds.BeginInvoke, parameters, SystemTypes.IASyncResult, null);
            beginInvoke.Flags = MethodFlags.Public | MethodFlags.HideBySig | MethodFlags.NewSlot | MethodFlags.Virtual;
            beginInvoke.CallingConvention = CallingConventionFlags.HasThis;
            beginInvoke.ImplFlags = MethodImplFlags.Runtime;
            members.Add(beginInvoke);

            //EndInvoke
            parameters = new ParameterList();

            for(int i = 0; i < n; i++)
            {
                Parameter p = dparams[i];

                if(p == null || p.Type == null || !(p.Type is Reference))
                    continue;

                parameters.Add((Parameter)p.Clone());
            }

            parameters.Add(new Parameter(null, ParameterFlags.None, StandardIds.result, SystemTypes.IASyncResult, null, null));
            Method endInvoke = new Method(this, null, StandardIds.EndInvoke, parameters, this.ReturnType, null);
            endInvoke.Flags = MethodFlags.Public | MethodFlags.HideBySig | MethodFlags.NewSlot | MethodFlags.Virtual;
            endInvoke.CallingConvention = CallingConventionFlags.HasThis;
            endInvoke.ImplFlags = MethodImplFlags.Runtime;
            members.Add(endInvoke);
            if(!this.IsGeneric)
            {
                TypeNodeList templPars = this.TemplateParameters;
                for(int i = 0, m = templPars == null ? 0 : templPars.Count; i < m; i++)
                {
                    //^ assert templPars != null;
                    TypeNode tpar = templPars[i];
                    if(tpar == null)
                        continue;
                    members.Add(tpar);
                }
            }
        }

    }

    public class FunctionType : DelegateNode
    {
        private FunctionType(Identifier name, TypeNode returnType, ParameterList parameters)
        {
            this.Flags = TypeFlags.Public | TypeFlags.Sealed;
            this.Namespace = StandardIds.StructuralTypes;
            this.Name = name;
            this.returnType = returnType;
            this.parameters = parameters;
        }
        public static FunctionType For(TypeNode returnType, ParameterList parameters, TypeNode referringType)
        {
            if(returnType == null || referringType == null)
                return null;
            Module module = referringType.DeclaringModule;
            if(module == null)
                return null;
            TypeFlags visibility = returnType.Flags & TypeFlags.VisibilityMask;
            StringBuilder name = new StringBuilder();
            name.Append("Function_");
            name.Append(returnType.Name.ToString());
            int n = parameters == null ? 0 : parameters.Count;
            if(parameters != null)
                for(int i = 0; i < n; i++)
                {
                    Parameter p = parameters[i];
                    if(p == null || p.Type == null)
                        continue;
                    visibility = TypeNode.GetVisibilityIntersection(visibility, p.Type.Flags & TypeFlags.VisibilityMask);
                    name.Append('_');
                    name.Append(p.Type.Name.ToString());
                }
            FunctionType func = null;
            int count = 0;
            string fNameString = name.ToString();
            Identifier fName = Identifier.For(fNameString);
            TypeNode result = module.GetStructurallyEquivalentType(StandardIds.StructuralTypes, fName);
            while(result != null)
            {
                //Mangled name is the same. But mangling is not unique (types are not qualified with assemblies), so check for equality.
                func = result as FunctionType;
                bool goodMatch = func != null && func.ReturnType == returnType;
                if(goodMatch)
                {
                    //^ assert func != null;
                    ParameterList fpars = func.Parameters;
                    int m = fpars == null ? 0 : fpars.Count;
                    goodMatch = n == m;
                    if(parameters != null && fpars != null)
                        for(int i = 0; i < n && goodMatch; i++)
                        {
                            Parameter p = parameters[i];
                            Parameter q = fpars[i];
                            goodMatch = p != null && q != null && p.Type == q.Type;
                        }
                }
                if(goodMatch)
                    return func;
                //Mangle some more
                fName = Identifier.For(fNameString + (++count).ToString());
                result = module.GetStructurallyEquivalentType(StandardIds.StructuralTypes, fName);
            }

            if(parameters != null)
            {
                ParameterList clonedParams = new ParameterList();

                for(int i = 0; i < n; i++)
                {
                    Parameter p = parameters[i];
                    if(p != null)
                        p = (Parameter)p.Clone();
                    clonedParams.Add(p);
                }

                parameters = clonedParams;
            }

            func = new FunctionType(fName, returnType, parameters);
            func.DeclaringModule = module;
            switch(visibility)
            {
                case TypeFlags.NestedFamANDAssem:
                case TypeFlags.NestedFamily:
                case TypeFlags.NestedPrivate:
                    referringType.Members.Add(func);
                    func.DeclaringType = referringType;
                    func.Flags &= ~TypeFlags.VisibilityMask;
                    func.Flags |= TypeFlags.NestedPrivate;
                    break;
                default:
                    module.Types.Add(func);
                    break;
            }
            module.StructurallyEquivalentType[func.Name.UniqueIdKey] = func;
            func.ProvideMembers();
            return func;
        }
        public override bool IsStructural
        {
            get { return true; }
        }
        protected TypeNodeList structuralElementTypes;
        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList result = this.structuralElementTypes;
                if(result != null)
                    return result;
                this.structuralElementTypes = result = new TypeNodeList();
                result.Add(this.ReturnType);
                ParameterList pars = this.Parameters;
                for(int i = 0, n = pars == null ? 0 : pars.Count; i < n; i++)
                {
                    Parameter par = pars[i];
                    if(par == null || par.Type == null)
                        continue;
                    result.Add(par.Type);
                }
                return result;
            }
        }
        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if(type == null)
                return false;
            if(this == type)
                return true;
            FunctionType t = type as FunctionType;
            if(t == null)
                return false;
            if(this.Template != null)
                return base.IsStructurallyEquivalentTo(t);
            if(this.Flags != t.Flags)
                return false;
            if(this.ReturnType == null || t.ReturnType == null)
                return false;
            if(this.ReturnType != t.ReturnType && !this.ReturnType.IsStructurallyEquivalentTo(t.ReturnType))
                return false;
            if(this.Parameters == null)
                return t.Parameters == null;
            if(t.Parameters == null)
                return false;
            int n = this.Parameters.Count;
            if(n != t.Parameters.Count)
                return false;
            for(int i = 0; i < n; i++)
            {
                Parameter p1 = this.Parameters[i];
                Parameter p2 = t.Parameters[i];
                if(p1 == null || p2 == null)
                    return false;
                if(p1.Type == null || p2.Type == null)
                    return false;
                if(p1.Type != p2.Type && !p1.Type.IsStructurallyEquivalentTo(p2.Type))
                    return false;
            }
            return true;
        }
    }

    public class EnumNode : TypeNode
    {
        internal readonly static EnumNode/*!*/ Dummy = new EnumNode();

        public EnumNode()
            : base(NodeType.EnumNode)
        {
            this.typeCode = ElementType.ValueType;
            this.Flags |= TypeFlags.Sealed;
        }
        public EnumNode(NestedTypeProvider provideNestedTypes, TypeAttributeProvider provideAttributes, TypeMemberProvider provideMembers, object handle)
            : base(NodeType.EnumNode, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
            this.typeCode = ElementType.ValueType;
            this.Flags |= TypeFlags.Sealed;
        }

        public EnumNode(Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags typeAttributes,
          Identifier Namespace, Identifier name, InterfaceList interfaces, MemberList members)
            : base(declaringModule, declaringType, attributes, typeAttributes, Namespace, name, interfaces, members, NodeType.EnumNode)
        {
            this.typeCode = ElementType.ValueType;
            this.Flags |= TypeFlags.Sealed;
        }

        public override bool IsUnmanaged
        {
            get
            {
                return true;
            }
        }
        protected internal TypeNode underlyingType;
        /// <summary>
        /// The underlying integer type used to store values of this enumeration.
        /// </summary>
        public virtual TypeNode UnderlyingType
        {
            get
            {
                if(this.underlyingType == null)
                {
                    if(this.template is EnumNode)
                        return this.underlyingType = ((EnumNode)this.template).UnderlyingType;
                    this.underlyingType = CoreSystemTypes.Int32;
                    MemberList members = this.Members;
                    for(int i = 0, n = members.Count; i < n; i++)
                    {
                        Member mem = members[i];
                        Field f = mem as Field;
                        if(f != null && (f.Flags & FieldFlags.Static) == 0)
                            return this.underlyingType = f.Type;
                    }
                }
                return this.underlyingType;
            }
            set
            {
                this.underlyingType = value;
                MemberList members = this.Members;
                for(int i = 0, n = members.Count; i < n; i++)
                {
                    Member mem = members[i];
                    Field f = mem as Field;
                    if(f != null && (f.Flags & FieldFlags.Static) == 0)
                    {
                        f.Type = value;
                        return;
                    }
                }
                this.Members.Add(new Field(this, null, FieldFlags.Public | FieldFlags.SpecialName | FieldFlags.RTSpecialName, StandardIds.Value__, value, null));
            }
        }

        public TypeNode UnderlyingTypeExpression;
    }

    public class Interface : TypeNode
    {
        protected TrivialHashtable jointMemberTable;
        protected MemberList jointDefaultMembers;

        internal static readonly Interface/*!*/ Dummy = new Interface();

        public Interface()
            : base(NodeType.Interface)
        {
            this.Flags = TypeFlags.Interface | TypeFlags.Abstract;
        }
        public Interface(InterfaceList baseInterfaces)
            : base(NodeType.Interface)
        {
            this.Interfaces = baseInterfaces;
            this.Flags = TypeFlags.Interface | TypeFlags.Abstract;
        }
        public Interface(InterfaceList baseInterfaces, NestedTypeProvider provideNestedTypes, TypeAttributeProvider provideAttributes, TypeMemberProvider provideMembers, object handle)
            : base(NodeType.Interface, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
            this.Interfaces = baseInterfaces;
        }

        public Interface(Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags flags,
          Identifier Namespace, Identifier name, InterfaceList baseInterfaces, MemberList members)
            : base(declaringModule, declaringType, attributes, flags, Namespace, name, baseInterfaces, members, NodeType.Interface)
        {
            this.Flags |= TypeFlags.Interface | TypeFlags.Abstract;
        }
        public override void GetAbstractMethods(MethodList/*!*/ result)
        {
            MemberList members = this.Members;
            if(members == null)
                return;
            for(int i = 0, n = members.Count; i < n; i++)
            {
                Method m = members[i] as Method;
                if(m != null)
                    result.Add(m);
            }
        }
        public virtual MemberList GetAllDefaultMembers()
        {
            if(this.jointDefaultMembers == null)
            {
                this.jointDefaultMembers = new MemberList();
                MemberList defs = this.DefaultMembers;
                for(int i = 0, n = defs == null ? 0 : defs.Count; i < n; i++)
                    this.jointDefaultMembers.Add(defs[i]);
                InterfaceList interfaces = this.Interfaces;
                if(interfaces != null)
                    for(int j = 0, m = interfaces.Count; j < m; j++)
                    {
                        Interface iface = interfaces[j];
                        if(iface == null)
                            continue;
                        defs = iface.GetAllDefaultMembers();
                        if(defs == null)
                            continue;
                        for(int i = 0, n = defs.Count; i < n; i++)
                            this.jointDefaultMembers.Add(defs[i]);
                    }
            }
            return this.jointDefaultMembers;
        }
        public virtual MemberList GetAllMembersNamed(Identifier/*!*/ name)
        {
            lock(this)
            {
                TrivialHashtable memberTable = this.jointMemberTable;
                if(memberTable == null)
                    this.jointMemberTable = memberTable = new TrivialHashtable();
                MemberList result = (MemberList)memberTable[name.UniqueIdKey];
                if(result != null)
                    return result;
                memberTable[name.UniqueIdKey] = result = new MemberList();
                MemberList members = this.GetMembersNamed(name);
                for(int i = 0, n = members == null ? 0 : members.Count; i < n; i++)
                    result.Add(members[i]);
                InterfaceList interfaces = this.Interfaces;
                for(int j = 0, m = interfaces == null ? 0 : interfaces.Count; j < m; j++)
                {
                    Interface iface = interfaces[j];
                    if(iface == null)
                        continue;
                    members = iface.GetAllMembersNamed(name);
                    if(members != null)
                        for(int i = 0, n = members.Count; i < n; i++)
                            result.Add(members[i]);
                }
                members = CoreSystemTypes.Object.GetMembersNamed(name);
                for(int i = 0, n = members == null ? 0 : members.Count; i < n; i++)
                    result.Add(members[i]);
                return result;
            }
        }
    }

    public class Struct : TypeNode
    {
        internal static readonly Struct/*!*/ Dummy = new Struct();

        public Struct()
            : base(NodeType.Struct)
        {
            this.typeCode = ElementType.ValueType;
            this.Flags = TypeFlags.Sealed;
        }
        public Struct(NestedTypeProvider provideNestedTypes, TypeAttributeProvider provideAttributes, TypeMemberProvider provideMembers, object handle)
            : base(NodeType.Struct, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
            this.typeCode = ElementType.ValueType;
        }

        public Struct(Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags flags,
          Identifier Namespace, Identifier name, InterfaceList interfaces, MemberList members)
            : base(declaringModule, declaringType, attributes, flags, Namespace, name, interfaces, members, NodeType.Struct)
        {
            this.Interfaces = interfaces;
            this.typeCode = ElementType.ValueType;
            this.Flags |= TypeFlags.Sealed;
        }
        protected bool cachedUnmanaged;
        protected bool cachedUnmanagedIsValid;
        /// <summary>True if the type is a value type containing only fields of unmanaged types.</summary>
        public override bool IsUnmanaged
        {
            get
            {
                if(this.cachedUnmanagedIsValid)
                    return this.cachedUnmanaged;
                this.cachedUnmanagedIsValid = true; //protect against cycles
                this.cachedUnmanaged = true; //Self references should not influence the answer
                if(this.IsPrimitive)
                    return this.cachedUnmanaged = true;
                MemberList members = this.Members;
                bool isUnmanaged = true;
                for(int i = 0, n = members == null ? 0 : members.Count; i < n; i++)
                {
                    Field f = members[i] as Field;
                    if(f == null || f.Type == null || f.IsStatic)
                        continue;
                    if(!f.Type.IsUnmanaged) { isUnmanaged = false; break; }
                }
                return this.cachedUnmanaged = isUnmanaged;
            }
        }
    }
    public interface ITypeParameter
    {
        Member DeclaringMember { get; set; }
        /// <summary>
        /// Zero based index into a parameter list containing this parameter.
        /// </summary>
        int ParameterListIndex { get; set; }
        TypeParameterFlags TypeParameterFlags { get; set; }
        bool IsUnmanaged { get; }

        Identifier Name { get; }
        Module DeclaringModule { get; }
        TypeNode DeclaringType { get; }
        SourceContext SourceContext { get; }
        int UniqueKey { get; }
        TypeFlags Flags { get; }
    }

    public class TypeParameter : Interface, ITypeParameter
    {

        public TypeParameter()
            : base()
        {
            this.NodeType = NodeType.TypeParameter;
            this.Flags = TypeFlags.Interface | TypeFlags.NestedPublic | TypeFlags.Abstract;
            this.Namespace = StandardIds.TypeParameter;
        }
        public TypeParameter(InterfaceList baseInterfaces, NestedTypeProvider provideNestedTypes, TypeAttributeProvider provideAttributes, TypeMemberProvider provideMembers, object handle)
            : base(baseInterfaces, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
            this.NodeType = NodeType.TypeParameter;
            this.Flags = TypeFlags.Interface | TypeFlags.NestedPublic | TypeFlags.Abstract;
            this.Namespace = StandardIds.TypeParameter;
        }

        public Member DeclaringMember
        {
            get { return this.declaringMember; }
            set { this.declaringMember = value; }
        }
        private Member declaringMember;

        public override Type GetRuntimeType()
        {
            TypeNode t = this.DeclaringMember as TypeNode;
            if(t == null)
                return null;
            Type rt = t.GetRuntimeType();
            if(rt == null)
                return null;
            System.Type[] typeParameters = rt.GetGenericArguments();
            if(this.ParameterListIndex >= typeParameters.Length)
                return null;
            return typeParameters[this.ParameterListIndex];
        }

        /// <summary>
        /// Zero based index into a parameter list containing this parameter.
        /// </summary>
        public int ParameterListIndex
        {
            get { return this.parameterListIndex; }
            set { this.parameterListIndex = value; }
        }
        private int parameterListIndex;

        public TypeParameterFlags TypeParameterFlags
        {
            get
            {
                return this.typeParameterFlags;
            }
            set
            {
                this.typeParameterFlags = value;
            }
        }
        private TypeParameterFlags typeParameterFlags;
        public override bool IsStructural
        {
            get { return true; }
        }
        /// <summary>True if the type serves as a parameter to a type template.</summary>
        public override bool IsTemplateParameter
        {
            get
            {
                return true;
            }
        }
        public override bool IsValueType
        {
            get
            {
                return ((this.TypeParameterFlags & TypeParameterFlags.ValueTypeConstraint) == TypeParameterFlags.ValueTypeConstraint);
            }
        }

        public override XmlNode Documentation
        {
            get
            {
                if(this.documentation == null && this.declaringMember != null && this.Name != null)
                {
                    XmlNode parentDoc = this.declaringMember.Documentation;
                    if(parentDoc != null && parentDoc.HasChildNodes)
                    {
                        string myName = this.Name.Name;
                        foreach(XmlNode child in parentDoc.ChildNodes)
                        {
                            if(child.Name == "typeparam" && child.Attributes != null)
                            {
                                foreach(XmlAttribute attr in child.Attributes)
                                {
                                    if(attr != null && attr.Name == "name" && attr.Value == myName)
                                        return this.documentation = child;
                                }
                            }
                        }
                    }
                }
                return this.documentation;
            }
            set
            {
                this.documentation = value;
            }
        }
        public override string HelpText
        {
            get
            {
                if(this.helpText == null)
                {
                    XmlNode doc = this.Documentation;
                    if(doc != null)
                        this.helpText = doc.InnerText;
                }
                return this.helpText;
            }
            set
            {
                this.helpText = value;
            }
        }

        protected internal TypeNodeList structuralElementTypes;
        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList result = this.structuralElementTypes;
                if(result != null)
                    return result;
                this.structuralElementTypes = result = new TypeNodeList();
                if(this.BaseType != null)
                    result.Add(this.BaseType);
                InterfaceList interfaces = this.Interfaces;
                for(int i = 0, n = interfaces == null ? 0 : interfaces.Count; i < n; i++)
                {
                    Interface iface = interfaces[i];
                    if(iface == null)
                        continue;
                    result.Add(iface);
                }
                return result;
            }
        }

        internal override void AppendDocumentIdMangledName(StringBuilder/*!*/ sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            if(TargetPlatform.GenericTypeNamesMangleChar != 0)
            {
                int n = methodTypeParameters == null ? 0 : methodTypeParameters.Count;
                for(int i = 0; i < n; i++)
                {
                    //^ assert methodTypeParameters != null;
                    TypeNode mpar = methodTypeParameters[i];
                    if(mpar != this)
                        continue;
                    sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                    sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                    sb.Append(i);
                    return;
                }
                n = typeParameters == null ? 0 : typeParameters.Count;
                for(int i = 0; i < n; i++)
                {
                    TypeNode tpar = typeParameters[i];
                    if(tpar != this)
                        continue;
                    sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                    sb.Append(i);
                    return;
                }
                sb.Append("not found:");
            }
            sb.Append(this.FullName);
        }

        public override string GetFullUnmangledNameWithoutTypeParameters()
        {
            return this.GetUnmangledNameWithoutTypeParameters();
        }
        public override string GetFullUnmangledNameWithTypeParameters()
        {
            return this.GetUnmangledNameWithTypeParameters();
        }
        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if(null == (object)type)
                return false;
            if(this == type)
                return true;
            ITypeParameter itype = type as ITypeParameter;
            if(null == (object)itype)
                return false;
            if(this.Name != null && type.Name != null && this.Name.UniqueIdKey != type.Name.UniqueIdKey)
            {
                if(this.DeclaringMember == itype.DeclaringMember)
                    return false;
            }
            TypeNode bType = this.BaseType;
            TypeNode tbType = type.BaseType;
            if(null == (object)bType)
                bType = CoreSystemTypes.Object;
            if(null == (object)tbType)
                tbType = CoreSystemTypes.Object;
            if(bType != tbType /*&& !bType.IsStructurallyEquivalentTo(tbType)*/)
                return false;
            if(this.Interfaces == null)
                return type.Interfaces == null || type.Interfaces.Count == 0;
            if(type.Interfaces == null)
                return this.Interfaces.Count == 0;
            int n = this.Interfaces.Count;
            if(n != type.Interfaces.Count)
                return false;
            for(int i = 0; i < n; i++)
            {
                Interface i1 = this.Interfaces[i];
                Interface i2 = type.Interfaces[i];
                if(null == (object)i1 || null == (object)i2)
                    return false;
                if(i1 != i2 /*&& !i1.IsStructurallyEquivalentTo(i2)*/)
                    return false;
            }
            return true;
        }

        Module ITypeParameter.DeclaringModule { get { return this.DeclaringModule; } }
        TypeFlags ITypeParameter.Flags { get { return this.Flags; } }
        SourceContext ITypeParameter.SourceContext { get { return this.SourceContext; } }
    }

    public class MethodTypeParameter : TypeParameter
    {
        public MethodTypeParameter()
            : base()
        {
            this.NodeType = NodeType.TypeParameter;
            this.Flags = TypeFlags.Interface | TypeFlags.NestedPublic | TypeFlags.Abstract;
            this.Namespace = StandardIds.TypeParameter;
        }

        public MethodTypeParameter(InterfaceList baseInterfaces, NestedTypeProvider provideNestedTypes, TypeAttributeProvider provideAttributes, TypeMemberProvider provideMembers, object handle)
            : base(baseInterfaces, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
            this.NodeType = NodeType.TypeParameter;
            this.Flags = TypeFlags.Interface | TypeFlags.NestedPublic | TypeFlags.Abstract;
            this.Namespace = StandardIds.TypeParameter;
        }

        public override Type GetRuntimeType()
        {
            Method m = this.DeclaringMember as Method;
            if(m == null)
                return null;
            System.Reflection.MethodInfo mi = m.GetMethodInfo();
            if(mi == null)
                return null;
            System.Type[] typeParameters = mi.GetGenericArguments();
            if(this.ParameterListIndex >= typeParameters.Length)
                return null;
            return typeParameters[this.ParameterListIndex];
        }

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if(object.ReferenceEquals(this, type))
                return true;
            ITypeParameter tp = type as ITypeParameter;
            if(tp == null)
                return false;
            if(this.ParameterListIndex == tp.ParameterListIndex && this.DeclaringMember == tp.DeclaringMember)
                return true;
            return base.IsStructurallyEquivalentTo(type as MethodTypeParameter);
        }
    }
    public class ClassParameter : Class, ITypeParameter
    {
        protected TrivialHashtable jointMemberTable;

        public ClassParameter()
            : base()
        {
            this.NodeType = NodeType.ClassParameter;
            this.baseClass = CoreSystemTypes.Object;
            this.Flags = TypeFlags.NestedPublic | TypeFlags.Abstract;
            this.Namespace = StandardIds.TypeParameter;
        }
        public ClassParameter(NestedTypeProvider provideNestedTypes, TypeAttributeProvider provideAttributes, TypeMemberProvider provideMembers, object handle)
            : base(provideNestedTypes, provideAttributes, provideMembers, handle)
        {
            this.NodeType = NodeType.ClassParameter;
            this.baseClass = CoreSystemTypes.Object;
            this.Flags = TypeFlags.NestedPrivate | TypeFlags.Abstract | TypeFlags.SpecialName;
            this.Namespace = StandardIds.TypeParameter;
        }

        public Member DeclaringMember
        {
            get { return this.declaringMember; }
            set { this.declaringMember = value; }
        }
        private Member declaringMember;

        public virtual MemberList GetAllMembersNamed(Identifier/*!*/ name)
        {
            lock(this)
            {
                TrivialHashtable memberTable = this.jointMemberTable;
                if(memberTable == null)
                    this.jointMemberTable = memberTable = new TrivialHashtable();
                MemberList result = (MemberList)memberTable[name.UniqueIdKey];
                if(result != null)
                    return result;
                memberTable[name.UniqueIdKey] = result = new MemberList();
                TypeNode t = this;
                while(t != null)
                {
                    MemberList members = t.GetMembersNamed(name);
                    if(members != null)
                        for(int i = 0, n = members.Count; i < n; i++)
                            result.Add(members[i]);
                    t = t.BaseType;
                }
                InterfaceList interfaces = this.Interfaces;
                if(interfaces != null)
                    for(int j = 0, m = interfaces.Count; j < m; j++)
                    {
                        Interface iface = interfaces[j];
                        if(iface == null)
                            continue;
                        members = iface.GetAllMembersNamed(name);
                        if(members != null)
                            for(int i = 0, n = members == null ? 0 : members.Count; i < n; i++)
                                result.Add(members[i]);
                    }
                members = CoreSystemTypes.Object.GetMembersNamed(name);
                if(members != null)
                    for(int i = 0, n = members.Count; i < n; i++)
                        result.Add(members[i]);
                return result;
            }
        }

        public override Type GetRuntimeType()
        {
            TypeNode t = this.DeclaringMember as TypeNode;
            if(t == null)
                return null;
            Type rt = t.GetRuntimeType();
            if(rt == null)
                return null;
            System.Type[] typeParameters = rt.GetGenericArguments();
            if(this.ParameterListIndex >= typeParameters.Length)
                return null;
            return typeParameters[this.ParameterListIndex];
        }

        /// <summary>
        /// Zero based index into a parameter list containing this parameter.
        /// </summary>
        public int ParameterListIndex
        {
            get { return this.parameterListIndex; }
            set { this.parameterListIndex = value; }
        }
        private int parameterListIndex;
        public TypeParameterFlags TypeParameterFlags
        {
            get { return this.typeParameterFlags; }
            set { this.typeParameterFlags = value; }
        }
        private TypeParameterFlags typeParameterFlags;
        public override bool IsValueType
        {
            get
            {
                return ((this.typeParameterFlags & TypeParameterFlags.SpecialConstraintMask) == TypeParameterFlags.ValueTypeConstraint);
            }
        }
        public override bool IsStructural
        {
            get { return true; }
        }
        /// <summary>True if the type serves as a parameter to a type template.</summary>
        public override bool IsTemplateParameter
        {
            get
            {
                return true;
            }
        }

        public override XmlNode Documentation
        {
            get
            {
                if(this.documentation == null && this.declaringMember != null && this.Name != null)
                {
                    XmlNode parentDoc = this.declaringMember.Documentation;
                    if(parentDoc != null && parentDoc.HasChildNodes)
                    {
                        string myName = this.Name.Name;
                        foreach(XmlNode child in parentDoc.ChildNodes)
                        {
                            if(child.Name == "typeparam" && child.Attributes != null)
                            {
                                foreach(XmlAttribute attr in child.Attributes)
                                {
                                    if(attr != null && attr.Name == "name" && attr.Value == myName)
                                        return this.documentation = child;
                                }
                            }
                        }
                    }
                }
                return this.documentation;
            }
            set
            {
                this.documentation = value;
            }
        }
        public override string HelpText
        {
            get
            {
                if(this.helpText == null)
                {
                    XmlNode doc = this.Documentation;
                    if(doc != null)
                        this.helpText = doc.InnerText;
                }
                return this.helpText;
            }
            set
            {
                this.helpText = value;
            }
        }

        protected internal TypeNodeList structuralElementTypes;
        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList result = this.structuralElementTypes;
                if(result != null)
                    return result;
                this.structuralElementTypes = result = new TypeNodeList();
                if(this.BaseType != null)
                    result.Add(this.BaseType);
                InterfaceList interfaces = this.Interfaces;
                for(int i = 0, n = interfaces == null ? 0 : interfaces.Count; i < n; i++)
                {
                    Interface iface = interfaces[i];
                    if(iface == null)
                        continue;
                    result.Add(iface);
                }
                return result;
            }
        }

        internal override void AppendDocumentIdMangledName(StringBuilder/*!*/ sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            if(TargetPlatform.GenericTypeNamesMangleChar != 0)
            {
                int n = methodTypeParameters == null ? 0 : methodTypeParameters.Count;
                for(int i = 0; i < n; i++)
                {
                    //^ assert methodTypeParameters != null;
                    TypeNode mpar = methodTypeParameters[i];
                    if(mpar != this)
                        continue;
                    sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                    sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                    sb.Append(i);
                    return;
                }
                n = typeParameters == null ? 0 : typeParameters.Count;
                for(int i = 0; i < n; i++)
                {
                    TypeNode tpar = typeParameters[i];
                    if(tpar != this)
                        continue;
                    sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                    sb.Append(i);
                    return;
                }
                sb.Append("not found:");
            }
            sb.Append(this.FullName);
        }

        public override string GetFullUnmangledNameWithoutTypeParameters()
        {
            return this.GetUnmangledNameWithoutTypeParameters();
        }
        public override string GetFullUnmangledNameWithTypeParameters()
        {
            return this.GetUnmangledNameWithTypeParameters();
        }
        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if(null == (object)type)
                return false;
            if(this == type)
                return true;
            ITypeParameter itype = type as ITypeParameter;
            if(null == (object)itype)
                return false;
            if(this.Name != null && type.Name != null && this.Name.UniqueIdKey != type.Name.UniqueIdKey)
            {
                if(this.DeclaringMember == itype.DeclaringMember)
                    return false;
            }
            TypeNode bType = this.BaseType;
            TypeNode tbType = type.BaseType;
            if(null == (object)bType)
                bType = CoreSystemTypes.Object;
            if(null == (object)tbType)
                tbType = CoreSystemTypes.Object;
            if(bType != tbType /*&& !bType.IsStructurallyEquivalentTo(tbType)*/)
                return false;
            if(this.Interfaces == null)
                return type.Interfaces == null || type.Interfaces.Count == 0;
            if(type.Interfaces == null)
                return this.Interfaces.Count == 0;
            int n = this.Interfaces.Count;
            if(n != type.Interfaces.Count)
                return false;
            for(int i = 0; i < n; i++)
            {
                Interface i1 = this.Interfaces[i];
                Interface i2 = type.Interfaces[i];
                if(null == (object)i1 || null == (object)i2)
                    return false;
                if(i1 != i2 /*&& !i1.IsStructurallyEquivalentTo(i2)*/)
                    return false;
            }
            return true;
        }

        SourceContext ITypeParameter.SourceContext { get { return this.SourceContext; } }
        Module ITypeParameter.DeclaringModule { get { return this.DeclaringModule; } }
        TypeFlags ITypeParameter.Flags { get { return this.Flags; } }
    }

    public class MethodClassParameter : ClassParameter
    {
        public MethodClassParameter()
            : base()
        {
            this.NodeType = NodeType.ClassParameter;
            this.baseClass = CoreSystemTypes.Object;
            this.Flags = TypeFlags.NestedPublic | TypeFlags.Abstract;
            this.Namespace = StandardIds.TypeParameter;
        }

        public override Type GetRuntimeType()
        {
            Method m = this.DeclaringMember as Method;
            if(m == null)
                return null;
            System.Reflection.MethodInfo mi = m.GetMethodInfo();
            if(mi == null)
                return null;
            System.Type[] typeParameters = mi.GetGenericArguments();
            if(this.ParameterListIndex >= typeParameters.Length)
                return null;
            return typeParameters[this.ParameterListIndex];
        }

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if(object.ReferenceEquals(this, type))
                return true;
            ITypeParameter tp = type as ITypeParameter;
            if(tp == null)
                return false;
            if(this.ParameterListIndex == tp.ParameterListIndex /* && this.DeclaringMember == tp.DeclaringMember*/)
                return true;
            return base.IsStructurallyEquivalentTo(type as MethodClassParameter);
        }
    }

    public class ArrayType : TypeNode
    {
        private TypeNode/*!*/ elementType;
        private int rank;
        private int[] lowerBounds;
        private int[] sizes;
        internal ArrayType()
            : base(NodeType.ArrayType)
        {
        }
        internal ArrayType(TypeNode/*!*/ elementType, int rank)
            : this(elementType, rank, new int[0], new int[0])
        {
            if(rank == 1)
                this.typeCode = Metadata.ElementType.SzArray;
            else
                this.typeCode = Metadata.ElementType.Array;
        }
        internal ArrayType(TypeNode/*!*/ elementType, int rank, int[] sizes)
            : this(elementType, rank, sizes, new int[0])
        {
        }
        internal ArrayType(TypeNode/*!*/ elementType, int rank, int[] sizes, int[] lowerBounds)
            : base(null, null, null, elementType.Flags, null, null, null, null, NodeType.ArrayType)
        {
            Debug.Assert(elementType != null);
            this.rank = rank;
            this.elementType = elementType;
            this.DeclaringModule = elementType.DeclaringModule;
            this.lowerBounds = lowerBounds;
            this.sizes = sizes;
            if(rank == 1)
                this.typeCode = Metadata.ElementType.SzArray;
            else
                this.typeCode = Metadata.ElementType.Array;
            if(elementType == null || elementType.Name == null)
                return;
            StringBuilder name = new StringBuilder(this.ElementType.Name.ToString());

            name.Append('[');
            int k = this.Sizes == null ? 0 : this.Sizes.Length;
            int m = this.LowerBounds == null ? 0 : this.LowerBounds.Length;
            for(int i = 0, n = this.Rank; i < n; i++)
            {
                if(i < k && this.Sizes[i] != 0)
                {
                    if(i < m && this.LowerBounds[i] != 0)
                    {
                        name.Append(this.LowerBounds[i]);
                        name.Append(':');
                    }
                    name.Append(this.Sizes[i]);
                }
                if(i < n - 1)
                    name.Append(',');
            }
            name.Append(']');

            this.Name = Identifier.For(name.ToString());
            this.Namespace = elementType.Namespace;
        }
        public TypeNode/*!*/ ElementType
        {
            get { return this.elementType; }
            set { this.elementType = value; }
        }
        /// <summary>The interfaces implemented by this class or struct, or the extended by this interface.</summary>
        public override InterfaceList Interfaces
        {
            get
            {
                if(this.interfaces == null)
                {
                    this.interfaces = new InterfaceList(new[] { SystemTypes.ICloneable,
                        SystemTypes.IList, SystemTypes.ICollection, SystemTypes.IEnumerable });

                    if(this.Rank == 1)
                    {
                        if(SystemTypes.GenericIEnumerable != null && SystemTypes.GenericIEnumerable.DeclaringModule == CoreSystemTypes.SystemAssembly)
                        {
                            this.interfaces.Add((Interface)SystemTypes.GenericIEnumerable.GetTemplateInstance(this, elementType));

                            if(SystemTypes.GenericICollection != null)
                                this.interfaces.Add((Interface)SystemTypes.GenericICollection.GetTemplateInstance(this, elementType));

                            if(SystemTypes.GenericIList != null)
                                this.interfaces.Add((Interface)SystemTypes.GenericIList.GetTemplateInstance(this, elementType));
                        }
                    }
                }

                return this.interfaces;
            }
            set { this.interfaces = value; }
        }
        public int Rank
        {
            get { return this.rank; }
            set { this.rank = value; }
        }
        public int[] LowerBounds
        {
            get { return this.lowerBounds; }
            set { this.lowerBounds = value; }
        }
        public int[] Sizes
        {
            get { return this.sizes; }
            set { this.sizes = value; }
        }
        public bool IsSzArray()
        {
            return this.typeCode == Metadata.ElementType.SzArray;
        }
        private MemberList ctorList = null;
        private MemberList getterList = null;
        private MemberList setterList = null;
        private MemberList addressList = null;

        public override MemberList Members
        {
            get
            {
                if(this.members == null || this.membersBeingPopulated)
                {
                    lock(this)
                    {
                        if(this.members == null)
                        {
                            this.membersBeingPopulated = true;

                            MemberList members = this.members = new MemberList();

                            members.Add(this.Constructor);
                            //^ assume this.ctorList != null && this.ctorList.Length > 1;
                            members.Add(this.ctorList[1]);
                            members.Add(this.Getter);
                            members.Add(this.Setter);
                            members.Add(this.Address);

                            this.membersBeingPopulated = false;
                        }
                    }
                }
                return this.members;
            }
            set
            {
                this.members = value;
            }
        }
        public override string/*!*/ FullName
        {
            get
            {
                if(this.ElementType != null && this.ElementType.DeclaringType != null)
                    return this.ElementType.DeclaringType.FullName + "+" + (this.Name == null ? "" : this.Name.ToString());
                else if(this.Namespace != null && this.Namespace.UniqueIdKey != Identifier.Empty.UniqueIdKey)
                    return this.Namespace.ToString() + "." + (this.Name == null ? "" : this.Name.ToString());
                else if(this.Name != null)
                    return this.Name.ToString();
                else
                    return "";
            }
        }

        internal override void AppendDocumentIdMangledName(StringBuilder/*!*/ sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            if(this.ElementType == null)
                return;
            this.ElementType.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
            sb.Append('[');
            int k = this.Sizes == null ? 0 : this.Sizes.Length;
            int m = this.LowerBounds == null ? 0 : this.LowerBounds.Length;
            for(int i = 0, n = this.Rank; i < n; i++)
            {
                if(i < k && this.Sizes[i] != 0)
                {
                    if(i < m && this.LowerBounds[i] != 0)
                    {
                        sb.Append(this.LowerBounds[i]);
                        sb.Append(':');
                    }
                    sb.Append(this.Sizes[i]);
                }
                if(i < n - 1)
                    sb.Append(',');
            }
            sb.Append(']');
        }

        public virtual void SetLowerBoundToUnknown()
        {
            Debug.Assert(this.Rank == 1);
            this.typeCode = Metadata.ElementType.Array;
        }
        public virtual int GetLowerBound(int dimension)
        {
            if(this.LowerBounds == null || this.LowerBounds.Length <= dimension)
                return 0;
            return this.LowerBounds[dimension];
        }
        public virtual int GetSize(int dimension)
        {
            if(this.Sizes == null || this.Sizes.Length <= dimension)
                return 0;
            return this.Sizes[dimension];
        }

        public override MemberList/*!*/ GetMembersNamed(Identifier name)
        {
            if(name == null)
                return new MemberList();

            if(name.UniqueIdKey == StandardIds.Get.UniqueIdKey)
            {
                if(this.getterList == null)
                {
                    Method getter = this.Getter;

                    if(getter != null)
                        getter = null;
                    //^ assume this.getterList != null;
                }

                return this.getterList;
            }

            if(name.UniqueIdKey == StandardIds.Set.UniqueIdKey)
            {
                if(this.setterList == null)
                {
                    Method setter = this.Setter;

                    if(setter != null)
                        setter = null;
                    //^ assume this.setterList != null;
                }

                return this.setterList;
            }

            if(name.UniqueIdKey == StandardIds.Ctor.UniqueIdKey)
            {
                if(this.ctorList == null)
                {
                    Method ctor = this.Constructor;

                    if(ctor != null)
                        ctor = null;
                    //^ assume this.ctorList != null;
                }

                return this.ctorList;
            }

            if(name.UniqueIdKey == StandardIds.Address.UniqueIdKey)
            {
                if(this.addressList == null)
                {
                    Method addr = this.Address;

                    if(addr != null)
                        addr = null;
                    //^ assume this.addressList != null;
                }

                return this.addressList;
            }
            
            return new MemberList();
        }

        public override Type GetRuntimeType()
        {
            if(this.runtimeType == null)
            {
                if(this.ElementType == null)
                    return null;

                Type eType = this.ElementType.GetRuntimeType();

                if(eType == null)
                    return null;

                if(this.IsSzArray())
                    this.runtimeType = eType.MakeArrayType();
                else
                    this.runtimeType = eType.MakeArrayType(this.Rank);
            }

            return this.runtimeType;
        }

        public Method Constructor
        {
            get
            {
                if(this.ctorList == null)
                {
                    lock(this)
                    {
                        if(this.ctorList == null)
                        {
                            InstanceInitializer ctor = new InstanceInitializer();
                            ctor.DeclaringType = this;
                            ctor.Flags |= MethodFlags.Public;
                            int n = this.Rank;
                            ctor.Parameters = new ParameterList();

                            for(int i = 0; i < n; i++)
                            {
                                Parameter par = new Parameter();
                                par.DeclaringMethod = ctor;
                                par.Type = CoreSystemTypes.Int32;
                                ctor.Parameters.Add(par);
                            }

                            this.ctorList = new MemberList();

                            this.ctorList.Add(ctor);

                            ctor = new InstanceInitializer();
                            ctor.DeclaringType = this;
                            ctor.Flags |= MethodFlags.Public;

                            n = n * 2;
                            ctor.Parameters = new ParameterList();

                            for(int i = 0; i < n; i++)
                            {
                                Parameter par = new Parameter();
                                par.Type = CoreSystemTypes.Int32;
                                par.DeclaringMethod = ctor;
                                ctor.Parameters.Add(par);
                            }

                            this.ctorList.Add(ctor);
                        }
                    }
                }

                return (Method)this.ctorList[0];
            }
        }

        public Method Getter
        {
            get
            {
                if(this.getterList == null)
                {
                    lock(this)
                    {
                        if(this.getterList == null)
                        {
                            Method getter = new Method();
                            getter.Name = StandardIds.Get;
                            getter.DeclaringType = this;
                            getter.CallingConvention = CallingConventionFlags.HasThis;
                            getter.Flags = MethodFlags.Public;
                            getter.Parameters = new ParameterList();
                            for(int i = 0, n = this.Rank; i < n; i++)
                            {
                                Parameter par = new Parameter();
                                par.Type = CoreSystemTypes.Int32;
                                par.DeclaringMethod = getter;
                                getter.Parameters.Add(par);
                            }
                            getter.ReturnType = this.ElementType;
                            this.getterList = new MemberList();
                            this.getterList.Add(getter);
                        }
                    }
                }
                return (Method)this.getterList[0];
            }
        }
        public Method Setter
        {
            get
            {
                if(this.setterList == null)
                {
                    lock(this)
                    {
                        if(this.setterList == null)
                        {
                            Method setter = new Method();
                            setter.Name = StandardIds.Set;
                            setter.DeclaringType = this;
                            setter.CallingConvention = CallingConventionFlags.HasThis;
                            setter.Flags = MethodFlags.Public;
                            setter.Parameters = new ParameterList();
                            Parameter par;
                            for(int i = 0, n = this.Rank; i < n; i++)
                            {
                                par = new Parameter();
                                par.Type = CoreSystemTypes.Int32;
                                par.DeclaringMethod = setter;
                                setter.Parameters.Add(par);
                            }
                            par = new Parameter();
                            par.Type = this.ElementType;
                            par.DeclaringMethod = setter;
                            setter.Parameters.Add(par);
                            setter.ReturnType = CoreSystemTypes.Void;
                            this.setterList = new MemberList();
                            this.setterList.Add(setter);
                        }
                    }
                }
                return (Method)this.setterList[0];
            }
        }
        public Method Address
        {
            get
            {
                if(this.addressList == null)
                {
                    lock(this)
                    {
                        if(this.addressList == null)
                        {
                            Method address = new Method();
                            address.Name = StandardIds.Address;
                            address.DeclaringType = this;
                            address.CallingConvention = CallingConventionFlags.HasThis;
                            address.Flags = MethodFlags.Public;
                            address.Parameters = new ParameterList();
                            for(int i = 0, n = this.Rank; i < n; i++)
                            {
                                Parameter par = new Parameter();
                                par.Type = CoreSystemTypes.Int32;
                                par.DeclaringMethod = address;
                                address.Parameters.Add(par);
                            }
                            address.ReturnType = this.ElementType.GetReferenceType();
                            this.addressList = new MemberList();
                            this.addressList.Add(address);
                        }
                    }
                }
                return (Method)this.addressList[0];
            }
        }
        public override bool IsAssignableTo(TypeNode targetType)
        {
            if(targetType == null)
                return false;
            if(targetType == this || targetType == CoreSystemTypes.Object || targetType == CoreSystemTypes.Array || targetType == SystemTypes.ICloneable)
                return true;
            if(CoreSystemTypes.Array.IsAssignableTo(targetType))
                return true;
            if(targetType.Template != null && SystemTypes.GenericIEnumerable != null && SystemTypes.GenericIEnumerable.DeclaringModule == CoreSystemTypes.SystemAssembly)
            {
                if(targetType.Template == SystemTypes.GenericIEnumerable || targetType.Template == SystemTypes.GenericICollection ||
                  targetType.Template == SystemTypes.GenericIList)
                {
                    if(targetType.TemplateArguments == null || targetType.TemplateArguments.Count != 1)
                    {
                        Debug.Assert(false);
                        return false;
                    }
                    TypeNode ienumElementType = targetType.TemplateArguments[0];
                    if(this.ElementType == ienumElementType)
                        return true;
                    if(this.ElementType.IsValueType)
                        return false;
                    return this.ElementType.IsAssignableTo(ienumElementType);
                }
            }

            ArrayType targetArrayType = targetType as ArrayType;

            if(targetArrayType == null)
                return false;

            if(this.Rank != 1 || targetArrayType.Rank != 1)
                return false;

            TypeNode thisElementType = this.ElementType;

            if(thisElementType == null)
                return false;

            if(thisElementType == targetArrayType.ElementType)
                return true;

            if(thisElementType.IsValueType)
                return false;

            return thisElementType.IsAssignableTo(targetArrayType.ElementType);
        }

        public override bool IsStructural
        {
            get { return true; }
        }

        protected TypeNodeList structuralElementTypes;

        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList result = this.structuralElementTypes;

                if(result != null)
                    return result;

                this.structuralElementTypes = result = new TypeNodeList();
                result.Add(this.ElementType);

                return result;
            }
        }

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if(type == null)
                return false;
            if(this == type)
                return true;
            ArrayType t = type as ArrayType;
            if(t == null)
                return false;
            if(this.Rank != t.Rank)
                return false;
            if(this.ElementType == null || t.ElementType == null)
                return false;
            if(this.ElementType != t.ElementType && !this.ElementType.IsStructurallyEquivalentTo(t.ElementType))
                return false;
            if(this.Sizes == null)
                return t.Sizes == null;
            if(t.Sizes == null)
                return false;
            int n = this.Sizes.Length;
            if(n != t.Sizes.Length)
                return false;
            for(int i = 0; i < n; i++)
            {
                if(this.Sizes[i] != t.Sizes[i])
                    return false;
            }
            if(this.LowerBounds == null)
                return t.LowerBounds == null;
            if(t.LowerBounds == null)
                return false;
            n = this.LowerBounds.Length;
            if(n != t.LowerBounds.Length)
                return false;
            for(int i = 0; i < n; i++)
            {
                if(this.LowerBounds[i] != t.LowerBounds[i])
                    return false;
            }
            return true;
        }
    }
    public class Pointer : TypeNode
    {
        internal Pointer(TypeNode/*!*/ elementType)
            : base(NodeType.Pointer)
        {
            this.elementType = elementType;
            this.typeCode = Metadata.ElementType.Pointer;
            this.Name = Identifier.For(elementType.Name + "*");
            this.Namespace = elementType.Namespace;
        }
        private TypeNode/*!*/ elementType;
        public TypeNode/*!*/ ElementType
        {
            get { return this.elementType; }
            set { this.elementType = value; }
        }
        public override string/*!*/ FullName
        {
            get
            {
                if(this.ElementType != null && this.ElementType.DeclaringType != null)
                    return this.ElementType.DeclaringType.FullName + "+" + (this.Name == null ? "" : this.Name.ToString());
                else if(this.Namespace != null && this.Namespace.UniqueIdKey != Identifier.Empty.UniqueIdKey)
                    return this.Namespace.ToString() + "." + (this.Name == null ? "" : this.Name.ToString());
                else if(this.Name != null)
                    return this.Name.ToString();
                else
                    return "";
            }
        }

        internal override void AppendDocumentIdMangledName(StringBuilder/*!*/ sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            if(this.elementType == null)
                return;
            this.elementType.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
            sb.Append('*');
        }

        public override Type GetRuntimeType()
        {
            if(this.runtimeType == null)
            {
                if(this.ElementType == null)
                    return null;

                Type eType = this.ElementType.GetRuntimeType();

                if(eType == null)
                    return null;

                this.runtimeType = eType.MakePointerType();
            }
            return this.runtimeType;
        }

        public override bool IsAssignableTo(TypeNode targetType)
        {
            return targetType == this || (targetType is Pointer && ((Pointer)targetType).ElementType == CoreSystemTypes.Void);
        }
        public override bool IsUnmanaged
        {
            get
            {
                return true;
            }
        }
        public override bool IsStructural
        {
            get { return true; }
        }
        public override bool IsPointerType
        {
            get
            {
                return true;
            }
        }

        protected TypeNodeList structuralElementTypes;

        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList result = this.structuralElementTypes;

                if(result != null)
                    return result;

                this.structuralElementTypes = result = new TypeNodeList();
                result.Add(this.ElementType);

                return result;
            }
        }

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if(type == null)
                return false;
            if(this == type)
                return true;
            Pointer t = type as Pointer;
            if(t == null)
                return false;
            if(this.ElementType == null || t.ElementType == null)
                return false;
            return this.ElementType == t.ElementType || this.ElementType.IsStructurallyEquivalentTo(t.ElementType);
        }
    }
    public class Reference : TypeNode
    {
        internal Reference(TypeNode/*!*/ elementType)
            : base(NodeType.Reference)
        {
            this.elementType = elementType;
            this.typeCode = Metadata.ElementType.Reference;
            this.Name = Identifier.For(elementType.Name + "@");
            this.Namespace = elementType.Namespace;
        }
        private TypeNode/*!*/ elementType;
        public TypeNode/*!*/ ElementType
        {
            get { return this.elementType; }
            set { this.elementType = value; }
        }

        internal override void AppendDocumentIdMangledName(StringBuilder/*!*/ sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            if(this.elementType == null)
                return;
            this.elementType.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
            sb.Append('@');
        }

        public override bool IsAssignableTo(TypeNode targetType)
        {
            return targetType == this ||
              (targetType is Pointer && (((Pointer)targetType).ElementType == this.ElementType ||
                                         ((Pointer)targetType).ElementType == CoreSystemTypes.Void));
        }

        public override string/*!*/ FullName
        {
            get
            {
                if(this.ElementType != null && this.ElementType.DeclaringType != null)
                    return this.ElementType.DeclaringType.FullName + "+" + (this.Name == null ? "" : this.Name.ToString());
                else if(this.Namespace != null && this.Namespace.UniqueIdKey != Identifier.Empty.UniqueIdKey)
                    return this.Namespace.ToString() + "." + (this.Name == null ? "" : this.Name.ToString());
                else if(this.Name != null)
                    return this.Name.ToString();
                else
                    return "";
            }
        }

        public override Type GetRuntimeType()
        {
            if(this.runtimeType == null)
            {
                if(this.ElementType == null)
                    return null;

                Type eType = this.ElementType.GetRuntimeType();

                if(eType == null)
                    return null;

                this.runtimeType = eType.MakeByRefType();
            }

            return this.runtimeType;
        }

        public override bool IsStructural
        {
            get { return true; }
        }

        protected TypeNodeList structuralElementTypes;

        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList result = this.structuralElementTypes;

                if(result != null)
                    return result;

                this.structuralElementTypes = result = new TypeNodeList();
                result.Add(this.ElementType);

                return result;
            }
        }

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if(type == null)
                return false;
            if(this == type)
                return true;
            Reference t = type as Reference;
            if(t == null)
                return false;
            if(this.ElementType == null || t.ElementType == null)
                return false;
            return this.ElementType == t.ElementType || this.ElementType.IsStructurallyEquivalentTo(t.ElementType);
        }
    }

    public abstract class TypeModifier : TypeNode
    {
        private TypeNode/*!*/ modifier;
        private TypeNode/*!*/ modifiedType;

        public TypeNode ModifierExpression;
        public TypeNode ModifiedTypeExpression;

        internal TypeModifier(NodeType type, TypeNode/*!*/ modifier, TypeNode/*!*/ modified)
            : base(type)
        {
            this.modifier = modifier;
            this.modifiedType = modified;
            this.DeclaringModule = modified.DeclaringModule;
            this.Namespace = modified.Namespace;
            if(type == NodeType.OptionalModifier)
            {
                this.typeCode = ElementType.OptionalModifier;
                this.Name = Identifier.For("optional(" + modifier.Name + ") " + modified.Name);
                this.fullName = "optional(" + modifier.FullName + ") " + modified.FullName;
            }
            else
            {
                this.typeCode = ElementType.RequiredModifier;
                this.Name = Identifier.For("required(" + modifier.Name + ") " + modified.Name);
                this.fullName = "required(" + modifier.FullName + ") " + modified.FullName;
            }
            this.Flags = modified.Flags;
        }
        public TypeNode/*!*/ Modifier
        {
            get { return this.modifier; }
            set { this.modifier = value; }
        }
        public TypeNode/*!*/ ModifiedType
        {
            get { return this.modifiedType; }
            set { this.modifiedType = value; }
        }
        public override Node/*!*/ Clone()
        {
            Debug.Assert(false);
            return base.Clone();
        }
        public override string GetFullUnmangledNameWithoutTypeParameters()
        {
            return this.ModifiedType.GetFullUnmangledNameWithoutTypeParameters();
        }
        public override string GetFullUnmangledNameWithTypeParameters()
        {
            return this.ModifiedType.GetFullUnmangledNameWithTypeParameters();
        }
        public override string/*!*/ GetUnmangledNameWithoutTypeParameters()
        {
            return this.ModifiedType.GetUnmangledNameWithoutTypeParameters();
        }
        public override bool IsUnmanaged
        {
            get { return this.ModifiedType.IsUnmanaged; }
        }
        public override bool IsStructural
        {
            get { return true; }
        }
        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if(type == null)
                return false;
            if(this == type)
                return true;
            if(this.NodeType != type.NodeType)
                return false;
            TypeModifier t = type as TypeModifier;
            if(t == null) { Debug.Assert(false); return false; }
            if(this.Modifier != t.Modifier && (this.Modifier == null || !this.Modifier.IsStructurallyEquivalentTo(t.Modifier)))
                return false;
            if(this.ModifiedType != t.ModifiedType && (this.ModifiedType == null || !this.ModifiedType.IsStructurallyEquivalentTo(t.ModifiedType)))
                return false;
            return true;
        }
        public override bool IsValueType
        {
            get
            {
                return this.ModifiedType.IsValueType;
            }
        }

        public override bool IsPointerType
        {
            get
            {
                return this.ModifiedType.IsPointerType;
            }
        }

        public override bool IsTemplateParameter
        {
            get
            {
                return this.ModifiedType.IsTemplateParameter;
            }
        }

        protected TypeNodeList structuralElementTypes;

        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList result = this.structuralElementTypes;

                if(result != null)
                    return result;

                this.structuralElementTypes = result = new TypeNodeList();
                result.Add(this.ModifiedType);
                result.Add(this.Modifier);

                return result;
            }
        }
    }

    public class OptionalModifier : TypeModifier
    {
        internal OptionalModifier(TypeNode/*!*/ modifier, TypeNode/*!*/ modified)
            : base(NodeType.OptionalModifier, modifier, modified)
        {
        }
        public static OptionalModifier/*!*/ For(TypeNode/*!*/ modifier, TypeNode/*!*/ modified)
        {
            return (OptionalModifier)modified.GetModified(modifier, true);
        }

        internal override void AppendDocumentIdMangledName(StringBuilder/*!*/ sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            this.ModifiedType.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
            sb.Append('!');
            this.Modifier.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
        }
    }

    public class RequiredModifier : TypeModifier
    {
        internal RequiredModifier(TypeNode/*!*/ modifier, TypeNode/*!*/ modified)
            : base(NodeType.RequiredModifier, modifier, modified)
        {
        }
        public static RequiredModifier/*!*/ For(TypeNode/*!*/ modifier, TypeNode/*!*/ modified)
        {
            return (RequiredModifier)modified.GetModified(modifier, false);
        }

        internal override void AppendDocumentIdMangledName(StringBuilder/*!*/ sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            this.ModifiedType.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
            sb.Append('|');
            this.Modifier.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
        }
    }

    public class OptionalModifierTypeExpression : TypeNode
    {
        public TypeNode ModifiedType;
        public TypeNode Modifier;

        public OptionalModifierTypeExpression(TypeNode elementType, TypeNode modifier)
            : base(NodeType.OptionalModifierTypeExpression)
        {
            this.ModifiedType = elementType;
            this.Modifier = modifier;
        }
        public OptionalModifierTypeExpression(TypeNode elementType, TypeNode modifier, SourceContext sctx)
            : this(elementType, modifier)
        {
            this.SourceContext = sctx;
        }
        /// <summary>
        /// Only needed because IsUnmanaged test is performed in the Looker rather than checker. Once the test
        /// is moved, this code can be removed.
        /// </summary>
        public override bool IsUnmanaged
        {
            get
            {
                return this.ModifiedType == null ? false : this.ModifiedType.IsUnmanaged;
            }
        }

    }
    public class RequiredModifierTypeExpression : TypeNode
    {
        public TypeNode ModifiedType;
        public TypeNode Modifier;

        public RequiredModifierTypeExpression(TypeNode elementType, TypeNode modifier)
            : base(NodeType.RequiredModifierTypeExpression)
        {
            this.ModifiedType = elementType;
            this.Modifier = modifier;
        }
        public RequiredModifierTypeExpression(TypeNode elementType, TypeNode modifier, SourceContext sctx)
            : this(elementType, modifier)
        {
            this.SourceContext = sctx;
        }
        /// <summary>
        /// Can be removed once the Unmanaged check moves from Looker to Checker.
        /// </summary>
        public override bool IsUnmanaged
        {
            get
            {
                return this.ModifiedType == null ? false : this.ModifiedType.IsUnmanaged;
            }
        }

    }

    public class FunctionPointer : TypeNode
    {
        internal FunctionPointer(TypeNodeList parameterTypes, TypeNode/*!*/ returnType, Identifier name)
            : base(NodeType.FunctionPointer)
        {
            this.Name = name;
            this.Namespace = returnType.Namespace;
            this.parameterTypes = parameterTypes;
            this.returnType = returnType;
            this.typeCode = ElementType.FunctionPointer;
            this.varArgStart = int.MaxValue;
        }
        private CallingConventionFlags callingConvention;
        public CallingConventionFlags CallingConvention
        {
            get { return this.callingConvention; }
            set { this.callingConvention = value; }
        }
        private TypeNodeList parameterTypes;
        public TypeNodeList ParameterTypes
        {
            get { return this.parameterTypes; }
            set { this.parameterTypes = value; }
        }
        private TypeNode returnType;
        public TypeNode ReturnType
        {
            get { return this.returnType; }
            set { this.returnType = value; }
        }
        private int varArgStart;
        public int VarArgStart
        {
            get { return this.varArgStart; }
            set { this.varArgStart = value; }
        }
        public override bool IsStatic
        {
            get { return (this.CallingConvention & CallingConventionFlags.HasThis) == 0; }
        }
        public override bool IsStructural
        {
            get { return true; }
        }
        protected TypeNodeList structuralElementTypes;
        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList result = this.structuralElementTypes;
                if(result != null)
                    return result;
                this.structuralElementTypes = result = new TypeNodeList();
                result.Add(this.ReturnType);
                TypeNodeList ptypes = this.ParameterTypes;
                for(int i = 0, n = ptypes == null ? 0 : ptypes.Count; i < n; i++)
                {
                    TypeNode ptype = ptypes[i];
                    if(ptype == null)
                        continue;
                    result.Add(ptype);
                }
                return result;
            }
        }
        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if(type == null)
                return false;
            if(this == type)
                return true;
            FunctionPointer t = type as FunctionPointer;
            if(t == null)
                return false;
            if(this.Flags != t.Flags || this.CallingConvention != t.CallingConvention || this.VarArgStart != t.VarArgStart)
                return false;
            if(this.ReturnType == null || t.ReturnType == null)
                return false;
            if(this.ReturnType != t.ReturnType && !this.ReturnType.IsStructurallyEquivalentTo(t.ReturnType))
                return false;
            return this.IsStructurallyEquivalentList(this.ParameterTypes, t.ParameterTypes);
        }
        public static FunctionPointer/*!*/ For(TypeNodeList/*!*/ parameterTypes, TypeNode/*!*/ returnType)
        {
            Module mod = returnType.DeclaringModule;
            if(mod == null) { Debug.Fail(""); mod = new Module(); }
            StringBuilder sb = new StringBuilder("function pointer ");
            sb.Append(returnType.FullName);
            sb.Append('(');
            for(int i = 0, n = parameterTypes == null ? 0 : parameterTypes.Count; i < n; i++)
            {
                TypeNode type = parameterTypes[i];
                if(type == null)
                    continue;
                if(i != 0)
                    sb.Append(',');
                sb.Append(type.FullName);
            }
            sb.Append(')');
            Identifier name = Identifier.For(sb.ToString());
            TypeNode t = mod.GetStructurallyEquivalentType(returnType.Namespace, name);
            int counter = 1;
            while(t != null)
            {
                FunctionPointer fp = t as FunctionPointer;
                if(fp != null)
                {
                    if(fp.ReturnType == returnType && FunctionPointer.ParameterTypesAreEquivalent(fp.ParameterTypes, parameterTypes))
                        return fp;
                }
                name = Identifier.For(name.ToString() + counter++);
                t = mod.GetStructurallyEquivalentType(returnType.Namespace, name);
            }
            FunctionPointer result = t as FunctionPointer;
            if(result == null)
            {
                result = new FunctionPointer(parameterTypes, returnType, name);
                result.DeclaringModule = mod;
                mod.StructurallyEquivalentType[name.UniqueIdKey] = result;
            }
            return result;
        }
        private static bool ParameterTypesAreEquivalent(TypeNodeList list1, TypeNodeList list2)
        {
            if(list1 == null || list2 == null)
                return list1 == list2;
            int n = list1.Count;
            if(n != list2.Count)
                return false;
            for(int i = 0; i < n; i++)
                if(list1[i] != list2[i])
                    return false;
            return true;
        }
    }

    public class ArrayTypeExpression : ArrayType
    {
        //TODO: add expressions for elementType, rank, sizes and lower bounds
        public bool LowerBoundIsUnknown;
        public ArrayTypeExpression()
            : base()
        {
            this.NodeType = NodeType.ArrayTypeExpression;
        }
        public ArrayTypeExpression(TypeNode/*!*/ elementType, int rank)
            : base(elementType, rank)
        {
            this.NodeType = NodeType.ArrayTypeExpression;
        }
        public ArrayTypeExpression(TypeNode/*!*/ elementType, int rank, int[] sizes)
            : base(elementType, rank, sizes)
        {
            this.NodeType = NodeType.ArrayTypeExpression;
        }
        public ArrayTypeExpression(TypeNode/*!*/ elementType, int rank, int[] sizes, int[] lowerBounds)
            : base(elementType, rank, sizes, sizes)
        {
            this.NodeType = NodeType.ArrayTypeExpression;
        }
        public ArrayTypeExpression(TypeNode/*!*/ elementType, int rank, SourceContext sctx)
            : base(elementType, rank)
        {
            this.NodeType = NodeType.ArrayTypeExpression;
            this.SourceContext = sctx;
        }
        public ArrayTypeExpression(TypeNode/*!*/ elementType, int rank, int[] sizes, SourceContext sctx)
            : base(elementType, rank, sizes)
        {
            this.NodeType = NodeType.ArrayTypeExpression;
            this.SourceContext = sctx;
        }
        public ArrayTypeExpression(TypeNode/*!*/ elementType, int rank, int[] sizes, int[] lowerBounds, SourceContext sctx)
            : base(elementType, rank, sizes, sizes)
        {
            this.NodeType = NodeType.ArrayTypeExpression;
            this.SourceContext = sctx;
        }
    }
    public class ClassExpression : Class
    {
        public Expression Expression;

        public ClassExpression(Expression expression)
        {
            this.NodeType = NodeType.ClassExpression;
            this.Expression = expression;
        }

        public ClassExpression(Expression expression, TypeNodeList templateArguments)
        {
            this.NodeType = NodeType.ClassExpression;
            this.Expression = expression;
            this.TemplateArguments = templateArguments;

            if(templateArguments != null)
                this.TemplateArgumentExpressions = new TypeNodeList(templateArguments);
        }

        public ClassExpression(Expression expression, SourceContext sctx)
        {
            this.NodeType = NodeType.ClassExpression;
            this.Expression = expression;
            this.SourceContext = sctx;
        }

        public ClassExpression(Expression expression, TypeNodeList templateArguments, SourceContext sctx)
        {
            this.NodeType = NodeType.ClassExpression;
            this.Expression = expression;
            this.TemplateArguments = this.TemplateArgumentExpressions = templateArguments;

            if(templateArguments != null)
                this.TemplateArgumentExpressions = new TypeNodeList(templateArguments);

            this.SourceContext = sctx;
        }
    }

    public class InterfaceExpression : Interface
    {
        private Expression expression;
        public InterfaceExpression(Expression expression)
            : base(null)
        {
            this.NodeType = NodeType.InterfaceExpression;
            this.Expression = expression;
        }

        public InterfaceExpression(Expression expression, SourceContext sctx)
            : base(null)
        {
            this.NodeType = NodeType.InterfaceExpression;
            this.Expression = expression;
            this.SourceContext = sctx;
        }

        public Expression Expression
        {
            get { return this.expression; }
            set { this.expression = value; }
        }
    }

    public class FlexArrayTypeExpression : TypeNode
    {
        public TypeNode ElementType;
        public FlexArrayTypeExpression(TypeNode elementType)
            : base(NodeType.FlexArrayTypeExpression)
        {
            this.ElementType = elementType;
        }
        public FlexArrayTypeExpression(TypeNode elementType, SourceContext sctx)
            : base(NodeType.FlexArrayTypeExpression)
        {
            this.ElementType = elementType;
            this.SourceContext = sctx;
        }
    }
    public class FunctionTypeExpression : TypeNode
    {
        public ParameterList Parameters;
        public TypeNode ReturnType;
        public FunctionTypeExpression(TypeNode returnType, ParameterList parameters)
            : base(NodeType.FunctionTypeExpression)
        {
            this.ReturnType = returnType;
            this.Parameters = parameters;
        }
        public FunctionTypeExpression(TypeNode returnType, ParameterList parameters, SourceContext sctx)
            : base(NodeType.FunctionTypeExpression)
        {
            this.ReturnType = returnType;
            this.Parameters = parameters;
            this.SourceContext = sctx;
        }
    }
    public class PointerTypeExpression : Pointer
    {
        public PointerTypeExpression(TypeNode/*!*/ elementType)
            : base(elementType)
        {
            this.NodeType = NodeType.PointerTypeExpression;
        }
        public PointerTypeExpression(TypeNode/*!*/ elementType, SourceContext sctx)
            : base(elementType)
        {
            this.NodeType = NodeType.PointerTypeExpression;
            this.SourceContext = sctx;
        }
        /// <summary>
        /// This is only needed because the Unmanaged test is done in the Looker rather than the checker.
        /// (Once the check moves, this can be removed).
        /// </summary>
        public override bool IsUnmanaged
        {
            get
            {
                return true;
            }
        }

    }
    public class ReferenceTypeExpression : Reference
    {
        public ReferenceTypeExpression(TypeNode/*!*/ elementType)
            : base(elementType)
        {
            this.NodeType = NodeType.ReferenceTypeExpression;
        }
        public ReferenceTypeExpression(TypeNode/*!*/ elementType, SourceContext sctx)
            : base(elementType)
        {
            this.NodeType = NodeType.ReferenceTypeExpression;
            this.SourceContext = sctx;
        }
    }
    public class StreamTypeExpression : TypeNode
    {
        public TypeNode ElementType;
        public StreamTypeExpression(TypeNode elementType)
            : base(NodeType.StreamTypeExpression)
        {
            this.ElementType = elementType;
        }
        public StreamTypeExpression(TypeNode elementType, SourceContext sctx)
            : base(NodeType.StreamTypeExpression)
        {
            this.ElementType = elementType;
            this.SourceContext = sctx;
        }
    }
    public class NonEmptyStreamTypeExpression : TypeNode
    {
        public TypeNode ElementType;
        public NonEmptyStreamTypeExpression(TypeNode elementType)
            : base(NodeType.NonEmptyStreamTypeExpression)
        {
            this.ElementType = elementType;
        }
        public NonEmptyStreamTypeExpression(TypeNode elementType, SourceContext sctx)
            : base(NodeType.NonEmptyStreamTypeExpression)
        {
            this.ElementType = elementType;
            this.SourceContext = sctx;
        }
    }
    public class BoxedTypeExpression : TypeNode
    {
        public TypeNode ElementType;
        public BoxedTypeExpression(TypeNode elementType)
            : base(NodeType.BoxedTypeExpression)
        {
            this.ElementType = elementType;
        }
        public BoxedTypeExpression(TypeNode elementType, SourceContext sctx)
            : base(NodeType.BoxedTypeExpression)
        {
            this.ElementType = elementType;
            this.SourceContext = sctx;
        }
    }
    public class InvariantTypeExpression : TypeNode
    {
        public TypeNode ElementType;
        public InvariantTypeExpression(TypeNode elementType)
            : base(NodeType.InvariantTypeExpression)
        {
            this.ElementType = elementType;
        }
        public InvariantTypeExpression(TypeNode elementType, SourceContext sctx)
            : base(NodeType.InvariantTypeExpression)
        {
            this.ElementType = elementType;
            this.SourceContext = sctx;
        }
    }
    public class NonNullTypeExpression : TypeNode
    {
        public TypeNode ElementType;
        public NonNullTypeExpression(TypeNode elementType)
            : base(NodeType.NonNullTypeExpression)
        {
            this.ElementType = elementType;
        }
        public NonNullTypeExpression(TypeNode elementType, SourceContext sctx)
            : base(NodeType.NonNullTypeExpression)
        {
            this.ElementType = elementType;
            this.SourceContext = sctx;
        }
    }
    public class NonNullableTypeExpression : TypeNode
    {
        public TypeNode ElementType;
        public NonNullableTypeExpression(TypeNode elementType)
            : base(NodeType.NonNullableTypeExpression)
        {
            this.ElementType = elementType;
        }
        public NonNullableTypeExpression(TypeNode elementType, SourceContext sctx)
            : base(NodeType.NonNullableTypeExpression)
        {
            this.ElementType = elementType;
            this.SourceContext = sctx;
        }
    }
    public class NullableTypeExpression : TypeNode
    {
        public TypeNode ElementType;
        public NullableTypeExpression(TypeNode elementType)
            : base(NodeType.NullableTypeExpression)
        {
            this.ElementType = elementType;
        }
        public NullableTypeExpression(TypeNode elementType, SourceContext sctx)
            : base(NodeType.NullableTypeExpression)
        {
            this.ElementType = elementType;
            this.SourceContext = sctx;
        }
    }
    public class TupleTypeExpression : TypeNode
    {
        public FieldList Domains;
        public TupleTypeExpression(FieldList domains)
            : base(NodeType.TupleTypeExpression)
        {
            this.Domains = domains;
        }
        public TupleTypeExpression(FieldList domains, SourceContext sctx)
            : base(NodeType.TupleTypeExpression)
        {
            this.Domains = domains;
            this.SourceContext = sctx;
        }
    }
    public class TypeIntersectionExpression : TypeNode
    {
        public TypeNodeList Types;
        public TypeIntersectionExpression(TypeNodeList types)
            : base(NodeType.TypeIntersectionExpression)
        {
            this.Types = types;
        }
        public TypeIntersectionExpression(TypeNodeList types, SourceContext sctx)
            : base(NodeType.TypeIntersectionExpression)
        {
            this.Types = types;
            this.SourceContext = sctx;
        }
    }
    public class TypeUnionExpression : TypeNode
    {
        public TypeNodeList Types;
        public TypeUnionExpression(TypeNodeList types)
            : base(NodeType.TypeUnionExpression)
        {
            this.Types = types;
        }
        public TypeUnionExpression(TypeNodeList types, SourceContext sctx)
            : base(NodeType.TypeUnionExpression)
        {
            this.Types = types;
            this.SourceContext = sctx;
        }
    }
    public class TypeExpression : TypeNode
    {
        public Expression Expression;
        public int Arity;

        public TypeExpression(Expression expression)
            : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
        }
        public TypeExpression(Expression expression, TypeNodeList templateArguments)
            : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
            this.templateArguments = this.TemplateArgumentExpressions = templateArguments;
        }
        public TypeExpression(Expression expression, int arity)
            : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
            this.Arity = arity;
        }
        public TypeExpression(Expression expression, SourceContext sctx)
            : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
            this.SourceContext = sctx;
        }
        public TypeExpression(Expression expression, TypeNodeList templateArguments, SourceContext sctx)
            : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
            this.templateArguments = this.TemplateArgumentExpressions = templateArguments;
            this.SourceContext = sctx;
        }
        public TypeExpression(Expression expression, int arity, SourceContext sctx)
            : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
            this.Arity = arity;
            this.SourceContext = sctx;
        }
        public override bool IsUnmanaged
        {
            get
            {
                Literal lit = this.Expression as Literal;
                if(lit != null)
                {
                    TypeNode t = lit.Value as TypeNode;
                    if(t != null)
                        return t.IsUnmanaged;
                    if(lit.Value is TypeCode)
                        return true;
                }
                return true;
            }
        }
    }
    public class TypeReference : Node
    {
        public TypeNode Type;
        public TypeNode Expression;

        public TypeReference(TypeNode typeExpression)
            : base(NodeType.TypeReference)
        {
            this.Expression = typeExpression;
            if(typeExpression != null)
                this.SourceContext = typeExpression.SourceContext;
        }
        public TypeReference(TypeNode typeExpression, TypeNode type)
            : base(NodeType.TypeReference)
        {
            this.Expression = typeExpression;
            this.Type = type;
            if(typeExpression != null)
                this.SourceContext = typeExpression.SourceContext;
        }
        public static explicit operator TypeNode(TypeReference typeReference)
        {
            return null == (object)typeReference ? null : typeReference.Type;
        }
        public static bool operator ==(TypeReference typeReference, TypeNode type)
        {
            return null == (object)typeReference ? null == (object)type : typeReference.Type == type;
        }
        public static bool operator ==(TypeNode type, TypeReference typeReference)
        {
            return null == (object)typeReference ? null == (object)type : typeReference.Type == type;
        }
        public static bool operator !=(TypeReference typeReference, TypeNode type)
        {
            return null == (object)typeReference ? null != (object)type : typeReference.Type != type;
        }
        public static bool operator !=(TypeNode type, TypeReference typeReference)
        {
            return null == (object)typeReference ? null != (object)type : typeReference.Type != type;
        }
        public override bool Equals(object obj)
        {
            return obj == (object)this || obj == (object)this.Type;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public class ArglistArgumentExpression : NaryExpression
    {
        public ArglistArgumentExpression(ExpressionList args, SourceContext sctx)
            : base(args, NodeType.ArglistArgumentExpression)
        {
            this.SourceContext = sctx;
        }
    }
    public class ArglistExpression : Expression
    {
        public ArglistExpression(SourceContext sctx)
            : base(NodeType.ArglistExpression)
        {
            this.SourceContext = sctx;
        }
    }
    public class RefValueExpression : BinaryExpression
    {
        public RefValueExpression(Expression typedreference, Expression type, SourceContext sctx)
            : base(typedreference, type, NodeType.RefValueExpression)
        {
            this.SourceContext = sctx;
        }

    }
    public class RefTypeExpression : UnaryExpression
    {
        public RefTypeExpression(Expression typedreference, SourceContext sctx)
            : base(typedreference, NodeType.RefTypeExpression)
        {
            this.SourceContext = sctx;
        }

    }

    public class Event : Member
    {
        private EventFlags flags;
        private Method handlerAdder;
        private Method handlerCaller;
        private MethodFlags handlerFlags;
        private Method handlerRemover;
        private TypeNode handlerType;
        private MethodList otherMethods;

        public TypeNode HandlerTypeExpression;
        /// <summary>The list of types (just one in C#) that contain abstract or virtual events that are explicitly implemented or overridden by this event.</summary>
        public TypeNodeList ImplementedTypes;
        public TypeNodeList ImplementedTypeExpressions;
        /// <summary>Provides a delegate instance that is added to the event upon initialization.</summary>
        public Expression InitialHandler;
        public Field BackingField;

        public Event() : base(NodeType.Event)
        {
        }

        public Event(TypeNode declaringType, AttributeList attributes, EventFlags flags, Identifier name,
          Method handlerAdder, Method handlerCaller, Method handlerRemover, TypeNode handlerType)
            : base(declaringType, attributes, name, NodeType.Event)
        {
            this.Flags = flags;
            this.HandlerAdder = handlerAdder;
            this.HandlerCaller = handlerCaller;
            this.HandlerRemover = handlerRemover;
            this.HandlerType = handlerType;
        }

        /// <summary>Bits characterizing this event.</summary>
        public EventFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }
        /// <summary>The method to be called in order to add a handler to an event. Corresponds to the add clause of a C# event declaration.</summary>
        public Method HandlerAdder
        {
            get { return this.handlerAdder; }
            set { this.handlerAdder = value; }
        }
        /// <summary>The method that gets called to fire an event. There is no corresponding C# syntax.</summary>
        public Method HandlerCaller
        {
            get { return this.handlerCaller; }
            set { this.handlerCaller = value; }
        }
        public MethodFlags HandlerFlags
        {
            get { return this.handlerFlags; }
            set { this.handlerFlags = value; }
        }
        /// <summary>The method to be called in order to remove a handler from an event. Corresponds to the remove clause of a C# event declaration.</summary>
        public Method HandlerRemover
        {
            get { return this.handlerRemover; }
            set { this.handlerRemover = value; }
        }
        /// <summary>The delegate type that a handler for this event must have. Corresponds to the type clause of C# event declaration.</summary>
        public TypeNode HandlerType
        {
            get { return this.handlerType; }
            set { this.handlerType = value; }
        }
        public MethodList OtherMethods
        {
            get { return this.otherMethods; }
            set { this.otherMethods = value; }
        }
        protected string fullName;
        public override string/*!*/ FullName
        {
            get
            {
                string result = this.fullName;
                if(result == null)
                    this.fullName = result = this.DeclaringType.FullName + "." + (this.Name == null ? "" : this.Name.ToString());
                return result;
            }
        }

        protected override Identifier GetDocumentationId()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("E:");
            if(this.DeclaringType == null)
                return Identifier.Empty;
            this.DeclaringType.AppendDocumentIdMangledName(sb, null, null);
            sb.Append(".");
            if(this.Name == null)
                return Identifier.Empty;
            sb.Append(this.Name.Name);
            return Identifier.For(sb.ToString());
        }

        public static Event GetEvent(System.Reflection.EventInfo eventInfo)
        {
            if(eventInfo == null)
                return null;
            TypeNode tn = TypeNode.GetTypeNode(eventInfo.DeclaringType);
            if(tn == null)
                return null;
            return tn.GetEvent(Identifier.For(eventInfo.Name));
        }
        protected System.Reflection.EventInfo eventInfo;
        public virtual System.Reflection.EventInfo GetEventInfo()
        {
            if(this.eventInfo == null)
            {
                TypeNode tn = this.DeclaringType;
                if(tn == null)
                    return null;
                Type t = tn.GetRuntimeType();
                if(t == null)
                    return null;
                System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.DeclaredOnly;
                if(this.IsPublic)
                    flags |= System.Reflection.BindingFlags.Public;
                else
                    flags |= System.Reflection.BindingFlags.NonPublic;
                if(this.IsStatic)
                    flags |= System.Reflection.BindingFlags.Static;
                else
                    flags |= System.Reflection.BindingFlags.Instance;
                this.eventInfo = t.GetEvent(this.Name.ToString(), flags);
            }
            return this.eventInfo;
        }

        /// <summary>
        /// True if the methods constituting this event are abstract.
        /// </summary>
        public bool IsAbstract
        {
            get { return (this.HandlerFlags & MethodFlags.Abstract) != 0; }
        }
        public override bool IsAssembly
        {
            get { return (this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.Assembly; }
        }
        public override bool IsCompilerControlled
        {
            get { return (this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.CompilerControlled; }
        }
        public override bool IsFamily
        {
            get { return (this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.Family; }
        }
        public override bool IsFamilyAndAssembly
        {
            get { return (this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.FamANDAssem; }
        }
        public override bool IsFamilyOrAssembly
        {
            get { return (this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.FamORAssem; }
        }
        public bool IsFinal
        {
            get { return (this.HandlerFlags & MethodFlags.Final) != 0; }
        }
        public override bool IsPrivate
        {
            get { return (this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.Private; }
        }
        public override bool IsPublic
        {
            get { return (this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.Public; }
        }
        public override bool IsSpecialName
        {
            get { return (this.Flags & EventFlags.SpecialName) != 0; }
        }
        public override bool IsStatic
        {
            get { return (this.HandlerFlags & MethodFlags.Static) != 0; }
        }
        /// <summary>
        /// True if that the methods constituting this event are virtual.
        /// </summary>
        public bool IsVirtual
        {
            get { return (this.HandlerFlags & MethodFlags.Virtual) != 0; }
        }
        public override bool IsVisibleOutsideAssembly
        {
            get
            {
                return (this.HandlerAdder != null && this.HandlerAdder.IsVisibleOutsideAssembly) ||
                  (this.HandlerCaller != null && this.HandlerCaller.IsVisibleOutsideAssembly) ||
                  (this.HandlerRemover != null && this.HandlerRemover.IsVisibleOutsideAssembly);
            }
        }
        public static readonly Event NotSpecified = new Event();
        public override Member HiddenMember
        {
            get
            {
                return this.HiddenEvent;
            }
            set
            {
                this.HiddenEvent = (Event)value;
            }
        }
        protected Property hiddenEvent;
        public virtual Event HiddenEvent
        {
            get
            {
                if(this.hiddenMember == Event.NotSpecified)
                    return null;
                Event hiddenEvent = this.hiddenMember as Event;
                if(hiddenEvent != null)
                    return hiddenEvent;

                Method hiddenAdder = this.HandlerAdder == null ? null : this.HandlerAdder.HiddenMethod;
                Method hiddenCaller = this.HandlerCaller == null ? null : this.HandlerCaller.HiddenMethod;
                Method hiddenRemover = this.HandlerRemover == null ? null : this.HandlerRemover.HiddenMethod;
                Event hiddenAdderEvent = hiddenAdder == null ? null : hiddenAdder.DeclaringMember as Event;
                Event hiddenCallerEvent = hiddenCaller == null ? null : hiddenCaller.DeclaringMember as Event;
                Event hiddenRemoverEvent = hiddenRemover == null ? null : hiddenRemover.DeclaringMember as Event;

                hiddenEvent = hiddenAdderEvent;
                if(hiddenCallerEvent != null)
                {
                    if(hiddenEvent == null ||
                      (hiddenCallerEvent.DeclaringType != null && hiddenCallerEvent.DeclaringType.IsDerivedFrom(hiddenEvent.DeclaringType)))
                        hiddenEvent = hiddenCallerEvent;
                }
                if(hiddenRemoverEvent != null)
                {
                    if(hiddenEvent == null ||
                      (hiddenRemoverEvent.DeclaringType != null && hiddenRemoverEvent.DeclaringType.IsDerivedFrom(hiddenEvent.DeclaringType)))
                        hiddenEvent = hiddenRemoverEvent;
                }
                if(hiddenEvent == null)
                {
                    this.hiddenMember = Event.NotSpecified;
                    return null;
                }
                this.hiddenMember = hiddenEvent;
                return hiddenEvent;
            }
            set
            {
                this.hiddenMember = value;
            }
        }
        public override Member OverriddenMember
        {
            get
            {
                return this.OverriddenEvent;
            }
            set
            {
                this.OverriddenEvent = (Event)value;
            }
        }
        protected Property overriddenEvent;
        public virtual Event OverriddenEvent
        {
            get
            {
                if(this.overriddenMember == Event.NotSpecified)
                    return null;
                Event overriddenEvent = this.overriddenMember as Event;
                if(overriddenEvent != null)
                    return overriddenEvent;

                Method overriddenAdder = this.HandlerAdder == null ? null : this.HandlerAdder.OverriddenMethod;
                Method overriddenCaller = this.HandlerCaller == null ? null : this.HandlerCaller.OverriddenMethod;
                Method overriddenRemover = this.HandlerRemover == null ? null : this.HandlerRemover.OverriddenMethod;
                Event overriddenAdderEvent = overriddenAdder == null ? null : overriddenAdder.DeclaringMember as Event;
                Event overriddenCallerEvent = overriddenCaller == null ? null : overriddenCaller.DeclaringMember as Event;
                Event overriddenRemoverEvent = overriddenRemover == null ? null : overriddenRemover.DeclaringMember as Event;

                overriddenEvent = overriddenAdderEvent;
                if(overriddenCallerEvent != null)
                {
                    if(overriddenEvent == null ||
                    (overriddenCallerEvent.DeclaringType != null && overriddenCallerEvent.DeclaringType.IsDerivedFrom(overriddenEvent.DeclaringType)))
                        overriddenEvent = overriddenCallerEvent;
                }
                if(overriddenRemoverEvent != null)
                {
                    if(overriddenEvent == null ||
                      (overriddenRemoverEvent.DeclaringType != null && overriddenRemoverEvent.DeclaringType.IsDerivedFrom(overriddenEvent.DeclaringType)))
                        overriddenEvent = overriddenRemoverEvent;
                }
                if(overriddenEvent == null)
                {
                    this.overriddenMember = Event.NotSpecified;
                    return null;
                }
                this.overriddenMember = overriddenEvent;
                return overriddenEvent;
            }
            set
            {
                this.overriddenMember = value;
            }
        }
    }

    public class Method : Member
    {
        public TypeNodeList ImplementedTypes;
        public TypeNodeList ImplementedTypeExpressions;
        public bool HasCompilerGeneratedSignature = true;
        public TypeNode ReturnTypeExpression;
        /// <summary>Provides a way to retrieve the parameters and local variables defined in this method given their names.</summary>
        public MethodScope Scope;
        public bool HasOutOfBandContract = false;
        protected TrivialHashtable/*!*/ Locals = new TrivialHashtable();

        public LocalList LocalList;
        protected SecurityAttributeList securityAttributes;

        /// <summary>Contains declarative security information associated with the type.</summary>
        public SecurityAttributeList SecurityAttributes
        {
            get
            {
                if(this.securityAttributes != null)
                    return this.securityAttributes;

                if(this.attributes == null)
                {
                    // Getting the type attributes also gets the security attributes in the case of a type that
                    // was read in by the Reader.
                    AttributeList al = this.Attributes;

                    if(al != null)
                        al = null;

                    if(this.securityAttributes != null)
                        return this.securityAttributes;
                }

                return this.securityAttributes = new SecurityAttributeList();
            }
            set
            {
                this.securityAttributes = value;
            }
        }

        public delegate void MethodBodyProvider(Method/*!*/ method, object/*!*/ handle, bool asInstructionList);
        public MethodBodyProvider ProvideBody;
        public object ProviderHandle; //Opaque information to be used by the method body provider

        public Method()
            : base(NodeType.Method)
        {
        }

        public Method(MethodBodyProvider provider, object handle)
            : base(NodeType.Method)
        {
            this.ProvideBody = provider;
            this.ProviderHandle = handle;
        }
        public Method(TypeNode declaringType, AttributeList attributes, Identifier name, ParameterList parameters, TypeNode returnType, Block body)
            : base(declaringType, attributes, name, NodeType.Method)
        {
            this.body = body;
            this.Parameters = parameters; // important to use setter here.
            this.returnType = returnType;
        }
        private MethodFlags flags;
        public MethodFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }
        private MethodImplFlags implFlags;
        public MethodImplFlags ImplFlags
        {
            get { return this.implFlags; }
            set { this.implFlags = value; }
        }
        private MethodList implementedInterfaceMethods;
        public MethodList ImplementedInterfaceMethods
        {
            get { return this.implementedInterfaceMethods; }
            set { this.implementedInterfaceMethods = value; }
        }

        private MethodList implicitlyImplementedInterfaceMethods;
        /// <summary>
        /// Computes the implicitly implemented methods for any method, not necessarily being compiled.
        /// </summary>
        public MethodList ImplicitlyImplementedInterfaceMethods
        {
            get
            {
                if(this.implicitlyImplementedInterfaceMethods == null)
                {
                    this.implicitlyImplementedInterfaceMethods = new MethodList();

                    // There are several reasons that this method cannot implicitly implement any interface
                    // method.
                    if((this.ImplementedInterfaceMethods == null ||
                      this.ImplementedInterfaceMethods.Count == 0) && this.IsPublic && !this.IsStatic)
                    {
                        // It can implicitly implement an interface method for those interfaces that
                        // the method's type explicitly declares it implements
                        if(this.DeclaringType != null && this.DeclaringType.Interfaces != null)
                        {
                            foreach(Interface i in this.DeclaringType.Interfaces)
                            {
                                if(i == null)
                                    continue;

                                Method match = i.GetMatchingMethod(this);

                                // !EFW - Bug Fix.  Interface methods returning a generic type fail the
                                // ReturnType comparison as the types are considered unique.  I added an
                                // extra check based on the full name to catch those cases so that it does
                                // include the implemented generic method.  I suppose I could modify the
                                // equality operator for TypeNode but I don't want to inadvertently break
                                // anything so I'm playing it safe and only fixing the immediate issue here.

                                // But it cannot implicitly implement an interface method if there is
                                // an explicit implementation in the same type.
                                if(match != null && (match.ReturnType == this.ReturnType ||
                                  match.ReturnType.FullName == this.ReturnType.FullName) &&
                                  !this.DeclaringType.ImplementsExplicitly(match))
                                    this.implicitlyImplementedInterfaceMethods.Add(match);
                            }
                        }

                        // It can implicitly implement an interface method if it overrides a base class
                        // method and *that* method implicitly implements the interface method.
                        // (Note: if this method's type does *not* explicitly declare that it implements
                        // the interface, then unless the method overrides a method that does, it is *not*
                        // used as an implicit implementation.)
                        if(this.OverriddenMethod != null)
                        {
                            foreach(Method method in this.OverriddenMethod.ImplicitlyImplementedInterfaceMethods)
                                // But it cannot implicitly implement an interface method if there is
                                // an explicit implementation in the same type.
                                if(!this.DeclaringType.ImplementsExplicitly(method))
                                    this.implicitlyImplementedInterfaceMethods.Add(method);
                        }
                    }
                }

                return this.implicitlyImplementedInterfaceMethods;
            }
            set
            {
                this.implicitlyImplementedInterfaceMethods = value;
            }
        }

        private CallingConventionFlags callingConvention;
        public CallingConventionFlags CallingConvention
        {
            get { return this.callingConvention; }
            set { this.callingConvention = value; }
        }
        private bool initLocals = true;
        /// <summary>True if all local variables are to be initialized to default values before executing the method body.</summary>
        public bool InitLocals
        {
            get { return this.initLocals; }
            set { this.initLocals = value; }
        }
        private bool isGeneric;
        /// <summary>True if this method is a template that conforms to the rules for a CLR generic method.</summary>
        public bool IsGeneric
        {
            get { return this.isGeneric; }
            set { this.isGeneric = value; }
        }
        private ParameterList parameters;
        /// <summary>The parameters this method has to be called with.</summary>
        public ParameterList Parameters
        {
            get { return this.parameters; }
            set
            {
                this.parameters = value;
                if(value != null)
                {
                    for(int i = 0, n = value.Count; i < n; i++)
                    {
                        Parameter par = parameters[i];
                        if(par == null)
                            continue;
                        par.DeclaringMethod = this;
                    }
                }
            }
        }
        private PInvokeFlags pInvokeFlags = PInvokeFlags.None;
        public PInvokeFlags PInvokeFlags
        {
            get { return this.pInvokeFlags; }
            set { this.pInvokeFlags = value; }
        }
        private Module pInvokeModule;
        public Module PInvokeModule
        {
            get { return this.pInvokeModule; }
            set { this.pInvokeModule = value; }
        }
        private string pInvokeImportName;
        public string PInvokeImportName
        {
            get { return this.pInvokeImportName; }
            set { this.pInvokeImportName = value; }
        }
        private AttributeList returnAttributes;
        /// <summary>Attributes that apply to the return value of this method.</summary>
        public AttributeList ReturnAttributes
        {
            get { return this.returnAttributes; }
            set { this.returnAttributes = value; }
        }
        private MarshallingInformation returnTypeMarshallingInformation;
        public MarshallingInformation ReturnTypeMarshallingInformation
        {
            get { return this.returnTypeMarshallingInformation; }
            set { this.returnTypeMarshallingInformation = value; }
        }
        private TypeNode returnType;
        /// <summary>The type of value that this method may return.</summary>
        public TypeNode ReturnType
        {
            get { return this.returnType; }
            set { this.returnType = value; }
        }
        private Member declaringMember;
        /// <summary>Provides the declaring event or property of an accessor.</summary>
        public Member DeclaringMember
        {
            get { return this.declaringMember; }
            set { this.declaringMember = value; }
        }
        private This thisParameter;
        public This ThisParameter
        {
            get
            {
                if(this.thisParameter == null && !this.IsStatic && this.DeclaringType != null)
                {
                    if(this.DeclaringType.IsValueType)
                        this.ThisParameter = new This(this.DeclaringType.GetReferenceType());
                    else
                        this.ThisParameter = new This(this.DeclaringType);
                }
                return this.thisParameter;
            }
            set
            {
                if(value != null)
                    value.DeclaringMethod = this;
                this.thisParameter = value;
            }
        }
        protected internal Block body;
        /// <summary>The instructions constituting the body of this method, in the form of a tree.</summary>
        public virtual Block Body
        {
            get
            {
                if(this.body != null)
                    return this.body;
                if(this.ProvideBody != null && this.ProviderHandle != null)
                {
                    lock(Module.GlobalLock)
                    {
                        if(this.body == null)
                            this.ProvideBody(this, this.ProviderHandle, false);
                    }
                }
                return this.body;
            }
            set
            {
                this.body = value;
            }
        }
        /// <summary>
        /// A delegate that is called the first time Attributes is accessed, if non-null.
        /// Provides for incremental construction of the type node.
        /// Must not leave Attributes null.
        /// </summary>
        public MethodAttributeProvider ProvideMethodAttributes;
        /// <summary>
        /// The type of delegates that fill in the Attributes property of the given method.
        /// </summary>
        public delegate void MethodAttributeProvider(Method/*!*/ method, object/*!*/ handle);
        public override AttributeList Attributes
        {
            get
            {
                if(this.attributes == null)
                {
                    if(this.ProvideMethodAttributes != null && this.ProviderHandle != null)
                    {
                        lock(Module.GlobalLock)
                        {
                            if(this.attributes == null)
                                this.ProvideMethodAttributes(this, this.ProviderHandle);
                        }
                    }
                    else
                        this.attributes = new AttributeList();
                }

                return this.attributes;
            }
            set
            {
                this.attributes = value;
            }
        }

        public void ClearBody()
        {
            lock(Module.GlobalLock)
            {
                this.Body = null;
                this.Instructions = null;
                this.LocalList = null;
            }
        }

        protected string conditionalSymbol;
        protected bool doesNotHaveAConditionalSymbol;

        public string ConditionalSymbol
        {
            get
            {
                if(this.doesNotHaveAConditionalSymbol)
                    return null;
                if(this.conditionalSymbol == null)
                {
                    lock(this)
                    {
                        if(this.conditionalSymbol != null)
                            return this.conditionalSymbol;
                        AttributeNode condAttr = this.GetAttribute(SystemTypes.ConditionalAttribute);
                        if(condAttr != null && condAttr.Expressions != null && condAttr.Expressions.Count > 0)
                        {
                            Literal lit = condAttr.Expressions[0] as Literal;
                            if(lit != null)
                            {
                                this.conditionalSymbol = lit.Value as string;
                                if(this.conditionalSymbol != null)
                                    return this.conditionalSymbol;
                            }
                        }
                        this.doesNotHaveAConditionalSymbol = true;
                    }
                }
                return this.conditionalSymbol;
            }
            set
            {
                this.conditionalSymbol = value;
            }
        }
        protected InstructionList instructions;
        /// <summary>The instructions constituting the body of this method, in the form of a linear list of Instruction nodes.</summary>
        public virtual InstructionList Instructions
        {
            get
            {
                if(this.instructions != null)
                    return this.instructions;
                if(this.ProvideBody != null && this.ProviderHandle != null)
                {
                    lock(Module.GlobalLock)
                    {
                        if(this.instructions == null)
                            this.ProvideBody(this, this.ProviderHandle, true);
                    }
                }

                return this.instructions;
            }
            set
            {
                this.instructions = value;
            }
        }

        protected ExceptionHandlerList exceptionHandlers;

        public virtual ExceptionHandlerList ExceptionHandlers
        {
            get
            {
                if(this.exceptionHandlers != null)
                    return this.exceptionHandlers;

                Block dummy = this.Body;

                if(this.exceptionHandlers == null)
                    this.exceptionHandlers = new ExceptionHandlerList();

                return this.exceptionHandlers;
            }
            set
            {
                this.exceptionHandlers = value;
            }
        }

        protected override Identifier GetDocumentationId()
        {
            if(this.Template != null)
                return this.Template.GetDocumentationId();
            StringBuilder sb = new StringBuilder(this.DeclaringType.DocumentationId.ToString());
            sb[0] = 'M';
            sb.Append('.');
            if(this.NodeType == NodeType.InstanceInitializer)
                sb.Append("#ctor");
            else if(this.Name != null)
            {
                sb.Append(this.Name.ToString());
                if(TargetPlatform.GenericTypeNamesMangleChar != 0 && this.TemplateParameters != null && this.TemplateParameters.Count > 0)
                {
                    sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                    sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                    sb.Append(this.TemplateParameters.Count);
                }
            }
            ParameterList parameters = this.Parameters;
            for(int i = 0, n = parameters == null ? 0 : parameters.Count; i < n; i++)
            {
                Parameter par = parameters[i];
                if(par == null || par.Type == null)
                    continue;
                if(i == 0)
                    sb.Append('(');
                else
                    sb.Append(',');
                par.Type.AppendDocumentIdMangledName(sb, this.TemplateParameters, this.DeclaringType.TemplateParameters);
                if(i == n - 1)
                    sb.Append(')');
            }
            if(this.IsSpecialName && this.ReturnType != null && this.Name != null &&
              (this.Name.UniqueIdKey == StandardIds.opExplicit.UniqueIdKey || this.Name.UniqueIdKey == StandardIds.opImplicit.UniqueIdKey))
            {
                sb.Append('~');
                this.ReturnType.AppendDocumentIdMangledName(sb, this.TemplateParameters, this.DeclaringType.TemplateParameters);
            }
            return Identifier.For(sb.ToString());
        }

        protected internal string fullName;
        public override string/*!*/ FullName
        {
            get
            {
                if(this.fullName != null)
                    return this.fullName;
                StringBuilder sb = new StringBuilder();
                if(this.DeclaringType != null)
                {
                    sb.Append(this.DeclaringType.FullName);
                    sb.Append('.');
                    if(this.NodeType == NodeType.InstanceInitializer)
                        sb.Append("#ctor");
                    else if(this.Name != null)
                        sb.Append(this.Name.ToString());
                    ParameterList parameters = this.Parameters;
                    for(int i = 0, n = parameters == null ? 0 : parameters.Count; i < n; i++)
                    {
                        Parameter par = parameters[i];
                        if(par == null || par.Type == null)
                            continue;
                        if(i == 0)
                            sb.Append('(');
                        else
                            sb.Append(',');
                        sb.Append(par.Type.FullName);
                        if(i == n - 1)
                            sb.Append(')');
                    }
                }
                return this.fullName = sb.ToString();
            }
        }

        public virtual string GetUnmangledNameWithoutTypeParameters()
        {
            return this.GetUnmangledNameWithoutTypeParameters(false);
        }

        public virtual string GetUnmangledNameWithoutTypeParameters(bool omitParameterTypes)
        {
            StringBuilder sb = new StringBuilder();
            if(this.NodeType == NodeType.InstanceInitializer)
                sb.Append("#ctor");
            else if(this.Name != null)
            {
                string name = this.Name.ToString();
                int lastDot = name.LastIndexOf('.');
                int lastMangle = name.LastIndexOf('>');
                // explicit interface method overrides will have type names in
                // their method name, which may also contain type parameters
                if(lastMangle < lastDot)
                    lastMangle = -1;
                if(lastMangle > 0)
                    sb.Append(name.Substring(0, lastMangle + 1));
                else
                    sb.Append(name);
            }
            if(omitParameterTypes)
                return sb.ToString();
            ParameterList parameters = this.Parameters;
            for(int i = 0, n = parameters == null ? 0 : parameters.Count; i < n; i++)
            {
                Parameter par = parameters[i];
                if(par == null || par.Type == null)
                    continue;
                if(i == 0)
                    sb.Append('(');
                else
                    sb.Append(',');
                sb.Append(par.Type.GetFullUnmangledNameWithTypeParameters());
                if(i == n - 1)
                {
                    if(this.IsVarArg)
                    {
                        sb.Append(", __arglist");
                    }

                    sb.Append(')');
                }
            }
            return sb.ToString();
        }
        public virtual string GetUnmangledNameWithTypeParameters()
        {
            return this.GetUnmangledNameWithTypeParameters(false);
        }
        public virtual string GetUnmangledNameWithTypeParameters(bool omitParameterTypes)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.GetUnmangledNameWithoutTypeParameters(true));
            TypeNodeList templateParameters = this.TemplateParameters;
            for(int i = 0, n = templateParameters == null ? 0 : templateParameters.Count; i < n; i++)
            {
                TypeNode tpar = templateParameters[i];
                if(tpar == null)
                    continue;
                if(i == 0)
                    sb.Append('<');
                else
                    sb.Append(',');
                sb.Append(tpar.Name.ToString());
                if(i == n - 1)
                    sb.Append('>');
            }
            if(omitParameterTypes)
                return sb.ToString();
            ParameterList parameters = this.Parameters;
            for(int i = 0, n = parameters == null ? 0 : parameters.Count; i < n; i++)
            {
                Parameter par = parameters[i];
                if(par == null || par.Type == null)
                    continue;
                if(i == 0)
                    sb.Append('(');
                else
                    sb.Append(',');
                sb.Append(par.Type.GetFullUnmangledNameWithTypeParameters());
                if(i == n - 1)
                    sb.Append(')');
            }
            return sb.ToString();
        }
        public virtual string GetFullUnmangledNameWithTypeParameters()
        {
            return this.GetFullUnmangledNameWithTypeParameters(false);
        }
        public virtual string GetFullUnmangledNameWithTypeParameters(bool omitParameterTypes)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.DeclaringType.GetFullUnmangledNameWithTypeParameters());
            sb.Append('.');
            sb.Append(this.GetUnmangledNameWithTypeParameters());
            return sb.ToString();
        }
        public static MethodFlags GetVisibilityUnion(Method m1, Method m2)
        {
            if(m1 == null && m2 != null)
                return m2.Flags & MethodFlags.MethodAccessMask;
            if(m2 == null && m1 != null)
                return m1.Flags & MethodFlags.MethodAccessMask;
            if(m1 == null || m2 == null)
                return MethodFlags.CompilerControlled;
            return Method.GetVisibilityUnion(m1.Flags, m2.Flags);
        }
        public static MethodFlags GetVisibilityUnion(MethodFlags vis1, MethodFlags vis2)
        {
            vis1 &= MethodFlags.MethodAccessMask;
            vis2 &= MethodFlags.MethodAccessMask;
            switch(vis1)
            {
                case MethodFlags.Public:
                    return MethodFlags.Public;
                case MethodFlags.Assembly:
                    switch(vis2)
                    {
                        case MethodFlags.Public:
                            return MethodFlags.Public;
                        case MethodFlags.FamORAssem:
                        case MethodFlags.Family:
                            return MethodFlags.FamORAssem;
                        default:
                            return vis1;
                    }
                case MethodFlags.FamANDAssem:
                    switch(vis2)
                    {
                        case MethodFlags.Public:
                            return MethodFlags.Public;
                        case MethodFlags.Assembly:
                            return MethodFlags.Assembly;
                        case MethodFlags.FamORAssem:
                            return MethodFlags.FamORAssem;
                        case MethodFlags.Family:
                            return MethodFlags.Family;
                        default:
                            return vis1;
                    }
                case MethodFlags.FamORAssem:
                    switch(vis2)
                    {
                        case MethodFlags.Public:
                            return MethodFlags.Public;
                        default:
                            return vis1;
                    }
                case MethodFlags.Family:
                    switch(vis2)
                    {
                        case MethodFlags.Public:
                            return MethodFlags.Public;
                        case MethodFlags.FamORAssem:
                        case MethodFlags.Assembly:
                            return MethodFlags.FamORAssem;
                        default:
                            return vis1;
                    }
                default:
                    return vis2;
            }
        }

        public virtual object Invoke(object targetObject, params object[] arguments)
        {
            System.Reflection.MethodInfo methInfo = this.GetMethodInfo();
            if(methInfo == null)
                return null;
            return methInfo.Invoke(targetObject, arguments);
        }
        public virtual Literal Invoke(Literal/*!*/ targetObject, params Literal[] arguments)
        {
            int n = arguments == null ? 0 : arguments.Length;
            object[] args = n == 0 ? null : new object[n];
            if(args != null && arguments != null)
                for(int i = 0; i < n; i++)
                {
                    Literal lit = arguments[i];
                    args[i] = lit == null ? null : lit.Value;
                }
            return new Literal(this.Invoke(targetObject.Value, args));
        }

        protected bool isNormalized;
        public virtual bool IsNormalized
        {
            get
            {
                if(this.isNormalized)
                    return true;
                if(this.DeclaringType == null || this.SourceContext.Document != null)
                    return false;
                return this.isNormalized = this.DeclaringType.IsNormalized;
            }
            set
            {
                this.isNormalized = value;
            }
        }

        public virtual bool IsAbstract
        {
            get { return (this.Flags & MethodFlags.Abstract) != 0; }
        }
        public override bool IsAssembly
        {
            get { return (this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.Assembly; }
        }
        public override bool IsCompilerControlled
        {
            get { return (this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.CompilerControlled; }
        }
        public virtual bool IsExtern
        {
            get { return (this.Flags & MethodFlags.PInvokeImpl) != 0 || (this.ImplFlags & (MethodImplFlags.Runtime | MethodImplFlags.InternalCall)) != 0; }
        }
        public override bool IsFamily
        {
            get { return (this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.Family; }
        }
        public override bool IsFamilyAndAssembly
        {
            get { return (this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.FamANDAssem; }
        }
        public override bool IsFamilyOrAssembly
        {
            get { return (this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.FamORAssem; }
        }
        public virtual bool IsFinal
        {
            get { return (this.Flags & MethodFlags.Final) != 0; }
        }

        public virtual bool IsInternalCall
        {
            get { return (this.ImplFlags & MethodImplFlags.InternalCall) != 0; }
        }

        public override bool IsPrivate
        {
            get { return (this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.Private; }
        }
        public override bool IsPublic
        {
            get { return (this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.Public; }
        }
        public override bool IsSpecialName
        {
            get { return (this.Flags & MethodFlags.SpecialName) != 0; }
        }
        public override bool IsStatic
        {
            get { return (this.Flags & MethodFlags.Static) != 0; }
        }
        /// <summary>
        /// True if this method can in principle be overridden by a method in a derived class.
        /// </summary>
        public virtual bool IsVirtual
        {
            get { return (this.Flags & MethodFlags.Virtual) != 0; }
        }

        public virtual bool IsNonSealedVirtual
        {
            get
            {
                return (this.Flags & MethodFlags.Virtual) != 0 && (this.Flags & MethodFlags.Final) == 0 &&
                  (this.DeclaringType == null || (this.DeclaringType.Flags & TypeFlags.Sealed) == 0);
            }
        }
        public virtual bool IsVirtualAndNotDeclaredInStruct
        {
            get
            {
                return (this.Flags & MethodFlags.Virtual) != 0 && (this.DeclaringType == null || !(this.DeclaringType is Struct));
            }
        }

        public override bool IsVisibleOutsideAssembly
        {
            get
            {
                if(this.DeclaringType != null && !this.DeclaringType.IsVisibleOutsideAssembly)
                    return false;
                switch(this.Flags & MethodFlags.MethodAccessMask)
                {
                    case MethodFlags.Public:
                        return true;
                    case MethodFlags.Family:
                    case MethodFlags.FamORAssem:
                        if(this.DeclaringType != null && !this.DeclaringType.IsSealed)
                            return true;
                        goto default;
                    default:
                        for(int i = 0, n = this.ImplementedInterfaceMethods == null ? 0 : this.ImplementedInterfaceMethods.Count; i < n; i++)
                        {
                            Method m = this.ImplementedInterfaceMethods[i];
                            if(m == null)
                                continue;
                            if(m.DeclaringType != null && !m.DeclaringType.IsVisibleOutsideAssembly)
                                continue;
                            if(m.IsVisibleOutsideAssembly)
                                return true;
                        }
                        return false;
                }
            }
        }

        public bool IsVarArg
        {
            get { return (this.CallingConvention & CallingConventionFlags.VarArg) != 0; }
        }
        // whether this is a FieldInitializerMethod (declared in Sing#)
        public virtual bool IsFieldInitializerMethod
        {
            get
            {
                return false;
            }
        }

        public override Member HiddenMember
        {
            get
            {
                return this.HiddenMethod;
            }
            set
            {
                this.HiddenMethod = (Method)value;
            }
        }
        public virtual Method HiddenMethod
        {
            get
            {
                if(this.hiddenMember == Method.NotSpecified)
                    return null;
                Method hiddenMethod = this.hiddenMember as Method;
                if(hiddenMethod != null)
                    return hiddenMethod;
                if(this.ProvideBody == null)
                    return null;
                if(this.IsVirtual && (this.Flags & MethodFlags.VtableLayoutMask) != MethodFlags.NewSlot)
                    return null;
                TypeNode baseType = this.DeclaringType.BaseType;
                while(baseType != null)
                {
                    MemberList baseMembers = baseType.GetMembersNamed(this.Name);
                    if(baseMembers != null)
                        for(int i = 0, n = baseMembers.Count; i < n; i++)
                        {
                            Method bmeth = baseMembers[i] as Method;
                            if(bmeth == null)
                                continue;
                            if(!bmeth.ParametersMatch(this.Parameters))
                            {
                                if(this.TemplateParameters != null && this.TemplateParametersMatch(bmeth.TemplateParameters))
                                {
                                    if(!bmeth.ParametersMatchStructurally(this.Parameters))
                                        continue;
                                }
                                else
                                    continue;
                            }
                            hiddenMethod = bmeth;
                            goto done;
                        }
                    baseType = baseType.BaseType;
                }
done:
                if(hiddenMethod == null)
                {
                    this.hiddenMember = Method.NotSpecified;
                    return null;
                }
                this.hiddenMember = hiddenMethod;
                return hiddenMethod;
            }
            set
            {
                this.hiddenMember = value;
            }
        }
        public override Member OverriddenMember
        {
            get
            {
                return this.OverriddenMethod;
            }
            set
            {
                this.OverriddenMethod = (Method)value;
            }
        }
        public virtual Method OverriddenMethod
        {
            get
            {
                if((this.Flags & MethodFlags.VtableLayoutMask) == MethodFlags.NewSlot)
                    return null;
                if(this.overriddenMember == Method.NotSpecified)
                    return null;
                Method overriddenMethod = this.overriddenMember as Method;
                if(overriddenMethod != null)
                    return overriddenMethod;
                if(this.ProvideBody == null)
                    return null;
                if(!this.IsVirtual)
                    return null;
                TypeNode baseType = this.DeclaringType.BaseType;
                while(baseType != null)
                {
                    MemberList baseMembers = baseType.GetMembersNamed(this.Name);
                    if(baseMembers != null)
                        for(int i = 0, n = baseMembers.Count; i < n; i++)
                        {
                            Method bmeth = baseMembers[i] as Method;
                            if(bmeth == null)
                                continue;
                            if(!bmeth.ParametersMatch(this.Parameters))
                            {
                                if(this.TemplateParameters != null && this.TemplateParametersMatch(bmeth.TemplateParameters))
                                {
                                    if(!bmeth.ParametersMatchStructurally(this.Parameters))
                                        continue;
                                }
                                else
                                    continue;
                            }
                            overriddenMethod = bmeth;
                            goto done;
                        }
                    baseType = baseType.BaseType;
                }
done:
                if(overriddenMethod == null)
                {
                    this.overriddenMember = Method.NotSpecified;
                    return null;
                }
                this.overriddenMember = overriddenMethod;
                return overriddenMethod;
            }
            set
            {
                this.overriddenMember = value;
            }
        }

        public static Method GetMethod(System.Reflection.MethodInfo methodInfo)
        {
            if(methodInfo == null)
                return null;

            if(methodInfo.IsGenericMethod && !methodInfo.IsGenericMethodDefinition)
            {
                try
                {
                    Method template = Method.GetMethod(methodInfo.GetGenericMethodDefinition());
                    if(template == null)
                        return null;
                    TypeNodeList templateArguments = new TypeNodeList();
                    foreach(Type arg in methodInfo.GetGenericArguments())
                        templateArguments.Add(TypeNode.GetTypeNode(arg));
                    return template.GetTemplateInstance(template.DeclaringType, templateArguments);
                }
                catch
                {
                    //TODO: log error
                    return null;
                }
            }

            TypeNode tn = TypeNode.GetTypeNode(methodInfo.DeclaringType);
            if(tn == null)
                return null;
            System.Reflection.ParameterInfo[] paramInfos = methodInfo.GetParameters();
            int n = paramInfos == null ? 0 : paramInfos.Length;
            TypeNode[] parameterTypes = new TypeNode[n];
            for(int i = 0; i < n; i++)
            {
                System.Reflection.ParameterInfo param = paramInfos[i];
                if(param == null)
                    return null;
                parameterTypes[i] = TypeNode.GetTypeNode(param.ParameterType);
            }
            TypeNodeList paramTypes = new TypeNodeList(parameterTypes);
            TypeNode returnType = TypeNode.GetTypeNode(methodInfo.ReturnType);
            MemberList members = tn.GetMembersNamed(Identifier.For(methodInfo.Name));
            for(int i = 0, m = members == null ? 0 : members.Count; i < m; i++)
            {
                Method meth = members[i] as Method;
                if(meth == null)
                    continue;
                if(!meth.ParameterTypesMatch(paramTypes))
                    continue;
                if(meth.ReturnType != returnType)
                    continue;
                return meth;
            }
            return null;
        }

        protected System.Reflection.Emit.DynamicMethod dynamicMethod;

        protected System.Reflection.MethodInfo methodInfo;

        public virtual System.Reflection.MethodInfo GetMethodInfo()
        {
            if(this.methodInfo == null)
            {
                if(this.DeclaringType == null)
                    return null;

                if(this.IsGeneric && this.Template != null)
                {
                    try
                    {
                        System.Reflection.MethodInfo templateInfo = this.Template.GetMethodInfo();
                        if(templateInfo == null)
                            return null;
                        TypeNodeList args = this.TemplateArguments;
                        Type[] arguments = new Type[args.Count];
                        for(int i = 0; i < args.Count; i++)
                            arguments[i] = args[i].GetRuntimeType();
                        return templateInfo.MakeGenericMethod(arguments);
                    }
                    catch
                    {
                        //TODO: log error
                        return null;
                    }
                }

                Type t = this.DeclaringType.GetRuntimeType();
                if(t == null)
                    return null;
                Type retType = typeof(object);
                if(!this.isGeneric)
                {
                    //Can't do this for generic methods since it may involve a method type parameter
                    retType = this.ReturnType.GetRuntimeType();
                    if(retType == null)
                        return null;
                }
                ParameterList pars = this.Parameters;
                int n = pars == null ? 0 : pars.Count;
                Type[] types = new Type[n];
                for(int i = 0; i < n; i++)
                {
                    Parameter p = pars[i];
                    if(p == null || p.Type == null)
                        return null;
                    Type pt;
                    if(this.isGeneric)
                        pt = types[i] = typeof(object); //Have to cheat here since the type might involve a type parameter of the method and getting the runtime type for that is a problem
                    //unless we already have the method info in hand
                    else
                        pt = types[i] = p.Type.GetRuntimeType();
                    if(pt == null)
                        return null;
                }
                System.Reflection.MemberInfo[] members = t.GetMember(this.Name.ToString(), System.Reflection.MemberTypes.Method,
                  BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach(System.Reflection.MethodInfo meth in members)
                {
                    if(meth == null)
                        continue;
                    if(meth.IsStatic != this.IsStatic)
                        continue;
                    if(meth.ReturnType != retType)
                        continue;

                    if(meth.IsGenericMethodDefinition)
                    {
                        TypeNodeList templateParams = this.TemplateParameters;
                        Type[] genericArgs = meth.GetGenericArguments();
                        if(templateParams == null || genericArgs == null || templateParameters.Count != genericArgs.Length)
                            goto tryNext;
                        for(int i = 0, m = genericArgs.Length; i < m; i++)
                        {
                            TypeNode t1 = templateParameters[i];
                            Type t2 = genericArgs[i];
                            if(t1 == null || t2 == null || t1.Name == null || t1.Name.Name != t2.Name)
                                goto tryNext;
                        }
                    }

                    System.Reflection.ParameterInfo[] parameters = meth.GetParameters();
                    int parCount = parameters == null ? 0 : parameters.Length;
                    if(parCount != n)
                        continue;
                    for(int i = 0; i < n; i++)
                    {
                        //^ assert parameters != null;
                        System.Reflection.ParameterInfo par = parameters[i];
                        if(par == null)
                            goto tryNext;
                        if(this.isGeneric)
                        {
                            //We don't have the runtime type for the parameter, so just check that the name is the same
                            Parameter p = pars[i];
                            if(par.Name != p.Name.Name)
                                goto tryNext;
                        }
                        else
                        {
                            if(par.ParameterType != types[i])
                                goto tryNext;
                        }
                    }
                    return this.methodInfo = meth;
tryNext:
                    ;
                }
            }
            return this.methodInfo;
        }

        protected TypeNode[] parameterTypes;

        public virtual TypeNode[]/*!*/ GetParameterTypes()
        {
            if(this.parameterTypes != null)
                return this.parameterTypes;
            ParameterList pars = this.Parameters;
            int n = pars == null ? 0 : pars.Count;
            TypeNode[] types = this.parameterTypes = new TypeNode[n];
            for(int i = 0; i < n; i++)
            {
                Parameter p = pars[i];
                if(p == null)
                    continue;
                types[i] = p.Type;
            }
            return types;
        }

        public virtual bool ParametersMatch(ParameterList parameters)
        {
            ParameterList pars = this.Parameters;
            int n = pars == null ? 0 : pars.Count;
            int m = parameters == null ? 0 : parameters.Count;
            if(n != m)
                return false;
            if(parameters == null)
                return true;
            for(int i = 0; i < n; i++)
            {
                Parameter par1 = pars[i];
                Parameter par2 = parameters[i];
                if(par1 == null || par2 == null)
                    return false;
                if(par1.Type != par2.Type)
                    return false;
            }
            return true;
        }

        public virtual bool ParametersMatchExceptLast(ParameterList parameters)
        {
            ParameterList pars = this.Parameters;
            int n = pars == null ? 0 : pars.Count;
            int m = parameters == null ? 0 : parameters.Count;
            if(n != m)
                return false;
            if(parameters == null)
                return true;
            for(int i = 0; i < n - 1; i++)
            {
                Parameter par1 = pars[i];
                Parameter par2 = parameters[i];
                if(par1 == null || par2 == null)
                    return false;
                if(par1.Type != par2.Type)
                    return false;
            }
            return true;
        }

        public virtual bool ParametersMatchStructurally(ParameterList parameters)
        {
            ParameterList pars = this.Parameters;
            int n = pars == null ? 0 : pars.Count;
            int m = parameters == null ? 0 : parameters.Count;
            if(n != m)
                return false;
            if(parameters == null)
                return true;
            for(int i = 0; i < n; i++)
            {
                Parameter par1 = pars[i];
                Parameter par2 = parameters[i];
                if(par1 == null || par2 == null)
                    return false;
                if(par1.Type == null || par2.Type == null)
                    return false;
                if(par1.Type != par2.Type && !par1.Type.IsStructurallyEquivalentTo(par2.Type))
                    return false;
            }
            return true;
        }

        public virtual bool ParametersMatchStructurallyIncludingOutFlag(ParameterList parameters)
        {
            return this.ParametersMatchStructurallyIncludingOutFlag(parameters, false);
        }

        public virtual bool ParametersMatchStructurallyIncludingOutFlag(ParameterList parameters, bool allowCoVariance)
        {
            ParameterList pars = this.Parameters;
            int n = pars == null ? 0 : pars.Count;
            int m = parameters == null ? 0 : parameters.Count;
            if(n != m)
                return false;
            if(parameters == null)
                return true;
            for(int i = 0; i < n; i++)
            {
                Parameter par1 = pars[i];
                Parameter par2 = parameters[i];
                if(par1 == null || par2 == null)
                    return false;
                if(par1.Type == null || par2.Type == null)
                    return false;
                if((par1.Flags & ParameterFlags.Out) != (par2.Flags & ParameterFlags.Out))
                    return false;
                if(par1.Type != par2.Type && !par1.Type.IsStructurallyEquivalentTo(par2.Type))
                {
                    if(allowCoVariance && !par2.Type.IsValueType)
                        return par2.Type.IsAssignableTo(par1.Type);
                    return false;
                }
            }
            return true;
        }

        public virtual bool ParametersMatchStructurallyExceptLast(ParameterList parameters)
        {
            ParameterList pars = this.Parameters;
            int n = pars == null ? 0 : pars.Count;
            int m = parameters == null ? 0 : parameters.Count;
            if(n != m)
                return false;
            if(parameters == null)
                return true;
            for(int i = 0; i < n - 1; i++)
            {
                Parameter par1 = pars[i];
                Parameter par2 = parameters[i];
                if(par1 == null || par2 == null)
                    return false;
                if(par1.Type == null || par2.Type == null)
                    return false;
                if(par1.Type != par2.Type && !par1.Type.IsStructurallyEquivalentTo(par2.Type))
                    return false;
            }
            return true;
        }

        public virtual bool ParametersMatchIncludingOutFlag(ParameterList parameters)
        {
            ParameterList pars = this.Parameters;
            int n = pars == null ? 0 : pars.Count;
            int m = parameters == null ? 0 : parameters.Count;
            if(n != m)
                return false;
            if(parameters == null)
                return true;
            for(int i = 0; i < n; i++)
            {
                Parameter par1 = pars[i];
                Parameter par2 = parameters[i];
                if(par1.Type != par2.Type)
                    return false;
                if((par1.Flags & ParameterFlags.Out) != (par2.Flags & ParameterFlags.Out))
                    return false;
            }
            return true;
        }

        public virtual bool ParameterTypesMatch(TypeNodeList argumentTypes)
        {
            int n = this.Parameters == null ? 0 : this.Parameters.Count;
            int m = argumentTypes == null ? 0 : argumentTypes.Count;
            if(n != m)
                return false;
            if(argumentTypes == null)
                return true;
            for(int i = 0; i < n; i++)
            {
                Parameter par = this.Parameters[i];
                if(par == null)
                    return false;
                TypeNode argType = argumentTypes[i];
                if(par.Type != argType)
                {
                    TypeNode pType = TypeNode.StripModifiers(par.Type);
                    argType = TypeNode.StripModifiers(argType);
                    if(pType != argType)
                        return false;
                }
            }
            return true;
        }
        public virtual bool ParameterTypesMatchStructurally(TypeNodeList argumentTypes)
        {
            int n = this.Parameters == null ? 0 : this.Parameters.Count;
            int m = argumentTypes == null ? 0 : argumentTypes.Count;
            if(n != m)
                return false;
            if(argumentTypes == null)
                return true;
            for(int i = 0; i < n; i++)
            {
                Parameter par = this.Parameters[i];
                TypeNode argType = argumentTypes[i];
                if(par.Type != argType)
                {
                    TypeNode pType = TypeNode.StripModifiers(par.Type);
                    argType = TypeNode.StripModifiers(argType);
                    if(pType == null || !pType.IsStructurallyEquivalentTo(argType))
                        return false;
                }
            }
            return true;
        }
        public virtual bool TemplateParametersMatch(TypeNodeList templateParameters)
        {
            TypeNodeList locPars = this.TemplateParameters;
            if(locPars == null)
                return templateParameters == null || templateParameters.Count == 0;
            if(templateParameters == null)
                return false;
            int n = locPars.Count;
            if(n != templateParameters.Count)
                return false;
            for(int i = 0; i < n; i++)
            {
                TypeNode tp1 = locPars[i];
                TypeNode tp2 = templateParameters[i];
                if(tp1 == null || tp2 == null)
                    return false;
                if(tp1 != tp2 && !tp1.IsStructurallyEquivalentTo(tp2))
                    return false;
            }
            return true;
        }

        internal TrivialHashtable contextForOffset;

        /// <summary>
        /// This is used to get or set the source context of the first line of code in the method
        /// </summary>
        internal SourceContext FirstLineContext { get; set; }

        internal void RecordSequencePoints(ISymUnmanagedMethod methodInfo)
        {
            if(methodInfo == null || this.contextForOffset != null)
                return;

            this.contextForOffset = new TrivialHashtable();
            uint count = methodInfo.GetSequencePointCount();

            IntPtr[] docPtrs = new IntPtr[count];
            uint[] startLines = new uint[count];
            uint[] startCols = new uint[count];
            uint[] endLines = new uint[count];
            uint[] endCols = new uint[count];
            uint[] offsets = new uint[count];
            uint numPoints;
            bool firstLineSet = false;

            methodInfo.GetSequencePoints(count, out numPoints, offsets, docPtrs, startLines, startCols, endLines, endCols);
            Debug.Assert(count == numPoints);

            for(int i = 0; i < count; i++)
            {
                // The magic hex constant below works around weird data reported from GetSequencePoints.
                // The constant comes from ILDASM's source code, which performs essentially the same test.
                const uint Magic = 0xFEEFEE;
                if(startLines[i] >= Magic || endLines[i] >= Magic)
                    continue;
                UnmanagedDocument doc = new UnmanagedDocument(docPtrs[i]);

                SourceContext context = new SourceContext(doc, doc.GetOffset(startLines[i], startCols[i]),
                    doc.GetOffset(endLines[i], endCols[i]));

                if(!firstLineSet)
                {
                    this.FirstLineContext = context;
                    firstLineSet = true;
                }

                this.contextForOffset[(int)offsets[i] + 1] = context;
            }

            for(int i = 0; i < count; i++)
                System.Runtime.InteropServices.Marshal.Release(docPtrs[i]);
        }

        private static Method NotSpecified = new Method();
        private Method template;
        /// <summary>The (generic) method template from which this method was instantiated. Null if this is not a (generic) method template instance.</summary>
        public Method Template
        {
            get
            {
                Method result = this.template;

                if(result == Method.NotSpecified)
                    return null;

                return result;
            }
            set
            {
                this.template = value;
            }
        }
        private TypeNodeList templateArguments;
        /// <summary>
        /// The arguments used when this (generic) method template instance was instantiated.
        /// </summary>
        public TypeNodeList TemplateArguments
        {
            get { return this.templateArguments; }
            set { this.templateArguments = value; }
        }
        internal TypeNodeList templateParameters;
        public virtual TypeNodeList TemplateParameters
        {
            get
            {
                TypeNodeList result = this.templateParameters;
                return result;
            }
            set
            {
                this.templateParameters = value;
            }
        }
        public virtual Method/*!*/ GetTemplateInstance(TypeNode referringType, params TypeNode[] typeArguments)
        {
            return this.GetTemplateInstance(referringType, new TypeNodeList(typeArguments));
        }
        public virtual Method/*!*/ GetTemplateInstance(TypeNode referringType, TypeNodeList typeArguments)
        {
            if(referringType == null || this.DeclaringType == null) { Debug.Assert(false); return this; }
            if(this.IsGeneric)
                referringType = this.DeclaringType;
            if(referringType != this.DeclaringType && referringType.DeclaringModule == this.DeclaringType.DeclaringModule)
                return this.GetTemplateInstance(this.DeclaringType, typeArguments);
            if(referringType.structurallyEquivalentMethod == null)
                referringType.structurallyEquivalentMethod = new TrivialHashtableUsingWeakReferences();
            Module module = referringType.DeclaringModule;
            if(module == null)
                return this;
            int n = typeArguments == null ? 0 : typeArguments.Count;
            if(n == 0 || typeArguments == null)
                return this;
            StringBuilder sb = new StringBuilder(this.Name.ToString());
            sb.Append('<');
            for(int i = 0; i < n; i++)
            {
                TypeNode ta = typeArguments[i];
                if(ta == null)
                    continue;
                sb.Append(ta.FullName);
                if(i < n - 1)
                    sb.Append(',');
            }
            sb.Append('>');
            Identifier mangledName = Identifier.For(sb.ToString());
            lock(this)
            {
                Method m = (Method)referringType.structurallyEquivalentMethod[mangledName.UniqueIdKey];
                int counter = 1;
                while(m != null)
                {
                    if(m.template == this && Method.TypeListsAreEquivalent(m.TemplateArguments, typeArguments))
                        return m;
                    mangledName = Identifier.For(mangledName.ToString() + counter++);
                    m = (Method)referringType.structurallyEquivalentMethod[mangledName.UniqueIdKey];
                }
                Duplicator duplicator = new Duplicator(referringType.DeclaringModule, referringType);
                duplicator.RecordOriginalAsTemplate = true;
                duplicator.SkipBodies = true;
                Method result = duplicator.VisitMethod(this);
                //^ assume result != null;
                result.Attributes = this.Attributes; //These do not get specialized, but may need to get normalized
                result.Name = mangledName;
                result.fullName = null;
                result.template = this;
                result.TemplateArguments = typeArguments;
                TypeNodeList templateParameters = result.TemplateParameters;
                result.TemplateParameters = null;

                result.IsNormalized = true;

                if(!this.IsGeneric)
                {
                    ParameterList pars = this.Parameters;
                    ParameterList rpars = result.Parameters;
                    if(pars != null && rpars != null && rpars.Count >= pars.Count)
                        for(int i = 0, count = pars.Count; i < count; i++)
                        {
                            Parameter p = pars[i];
                            Parameter rp = rpars[i];
                            if(p == null || rp == null)
                                continue;
                            rp.Attributes = p.Attributes; //These do not get specialized, but may need to get normalized
                        }
                }

                if(!this.IsGeneric && !(result.IsStatic) && this.DeclaringType != referringType)
                {
                    result.Flags &= ~(MethodFlags.Virtual | MethodFlags.NewSlot);
                    result.Flags |= MethodFlags.Static;
                    result.CallingConvention &= ~CallingConventionFlags.HasThis;
                    result.CallingConvention |= CallingConventionFlags.ExplicitThis;

                    ParameterList pars = result.Parameters;

                    if(pars == null)
                        result.Parameters = pars = new ParameterList();

                    Parameter thisPar = new Parameter(StandardIds.This, this.DeclaringType);
                    pars.Add(thisPar);

                    for(int i = pars.Count - 1; i > 0; i--)
                        pars[i] = pars[i - 1];

                    pars[0] = thisPar;
                }

                referringType.structurallyEquivalentMethod[mangledName.UniqueIdKey] = result;
                Specializer specializer = new Specializer(module, templateParameters, typeArguments);
                specializer.VisitMethod(result);
                if(this.IsGeneric) { result.DeclaringType = this.DeclaringType; return result; }
                if(this.IsAbstract)
                    return result;
                referringType.Members.Add(result);
                return result;
            }
        }
        private static bool TypeListsAreEquivalent(TypeNodeList list1, TypeNodeList list2)
        {
            if(list1 == null || list2 == null)
                return list1 == list2;
            int n = list1.Count;
            if(n != list2.Count)
                return false;
            for(int i = 0; i < n; i++)
                if(list1[i] != list2[i])
                    return false;
            return true;
        }

        /// <summary>
        /// Returns the local associated with the given field, allocating a new local if necessary.
        /// </summary>
        public virtual Local/*!*/ GetLocalForField(Field/*!*/ f)
        {
            Local loc = (Local)this.Locals[f.UniqueKey];
            if(loc == null)
            {
                this.Locals[f.UniqueKey] = loc = new Local(f.Name, f.Type);
                loc.SourceContext = f.Name.SourceContext;
            }
            return loc;
        }

        //TODO: Also need to add a method for allocating locals
        public Method CreateExplicitImplementation(TypeNode implementingType, ParameterList parameters, StatementList body)
        {
            Method m = new Method(implementingType, null, this.Name, parameters, this.ReturnType, new Block(body));
            m.CallingConvention = CallingConventionFlags.HasThis;
            m.Flags = MethodFlags.Public | MethodFlags.HideBySig | MethodFlags.Virtual | MethodFlags.NewSlot | MethodFlags.Final;
            m.ImplementedInterfaceMethods = new MethodList(new[] { this });
            //m.ImplementedTypes = new TypeNodeList(this.DeclaringType);
            return m;
        }

        public virtual bool TypeParameterCountsMatch(Method meth2)
        {
            if(meth2 == null)
                return false;
            int n = this.TemplateParameters == null ? 0 : this.TemplateParameters.Count;
            int m = meth2.TemplateParameters == null ? 0 : meth2.TemplateParameters.Count;
            return n == m;
        }
        public override string ToString()
        {
            return this.DeclaringType.GetFullUnmangledNameWithTypeParameters() + "." + this.Name;
        }

        public bool GetIsCompilerGenerated()
        {
            InstanceInitializer ii = this as InstanceInitializer;
            return this.HasCompilerGeneratedSignature || (ii != null && ii.IsCompilerGenerated);
        }
    }

    public class ProxyMethod : Method
    {
        public Method ProxyFor;
        public ProxyMethod(TypeNode declaringType, AttributeList attributes, Identifier name, ParameterList parameters, TypeNode returnType, Block body)
            : base(declaringType, attributes, name, parameters, returnType, body) { }
    }

    public class InstanceInitializer : Method
    {
        /// <summary>
        /// True if this constructor calls a constructor declared in the same class, as opposed to the base class.
        /// </summary>
        public bool IsDeferringConstructor;
        /// <summary>
        /// When the source uses the C# compatibility mode, base calls cannot be put after non-null
        /// field initialization, but must be put before the body. But the user can specify where
        /// the base ctor call should be performed by using "base;" as a marker. During parsing
        /// this flag is set so the right code transformations can be performed at code generation.
        /// </summary>
        public bool ContainsBaseMarkerBecauseOfNonNullFields;
        public Block BaseOrDefferingCallBlock;
        public bool IsCompilerGenerated = false;

        public InstanceInitializer()
            : base()
        {
            this.NodeType = NodeType.InstanceInitializer;
            this.CallingConvention = CallingConventionFlags.HasThis;
            this.Flags = MethodFlags.SpecialName | MethodFlags.RTSpecialName;
            this.Name = StandardIds.Ctor;
            this.ReturnType = CoreSystemTypes.Void;
        }
        public InstanceInitializer(MethodBodyProvider provider, object handle)
            : base(provider, handle)
        {
            this.NodeType = NodeType.InstanceInitializer;
        }

        public InstanceInitializer(TypeNode declaringType, AttributeList attributes, ParameterList parameters, Block body)
            : this(declaringType, attributes, parameters, body, CoreSystemTypes.Void)
        {
        }
        public InstanceInitializer(TypeNode declaringType, AttributeList attributes, ParameterList parameters, Block body, TypeNode returnType)
            : base(declaringType, attributes, StandardIds.Ctor, parameters, null, body)
        {
            this.NodeType = NodeType.InstanceInitializer;
            this.CallingConvention = CallingConventionFlags.HasThis;
            this.Flags = MethodFlags.SpecialName | MethodFlags.RTSpecialName;
            this.Name = StandardIds.Ctor;
            this.ReturnType = returnType;
        }

        protected System.Reflection.ConstructorInfo constructorInfo;

        public virtual System.Reflection.ConstructorInfo GetConstructorInfo()
        {
            if(this.constructorInfo == null)
            {
                if(this.DeclaringType == null)
                    return null;
                Type t = this.DeclaringType.GetRuntimeType();
                if(t == null)
                    return null;
                ParameterList pars = this.Parameters;
                int n = pars == null ? 0 : pars.Count;
                Type[] types = new Type[n];
                for(int i = 0; i < n; i++)
                {
                    Parameter p = pars[i];
                    if(p == null || p.Type == null)
                        return null;
                    Type pt = types[i] = p.Type.GetRuntimeType();
                    if(pt == null)
                        return null;
                }
                System.Reflection.MemberInfo[] members = t.GetMember(this.Name.ToString(), System.Reflection.MemberTypes.Constructor,
                  BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach(System.Reflection.ConstructorInfo cons in members)
                {
                    if(cons == null)
                        continue;
                    System.Reflection.ParameterInfo[] parameters = cons.GetParameters();
                    if(parameters != null)
                    {
                        if(parameters.Length != n)
                            continue;
                        for(int i = 0; i < n; i++)
                        {
                            System.Reflection.ParameterInfo par = parameters[i];
                            if(par == null || par.ParameterType != types[i])
                                goto tryNext;
                        }
                    }
                    return this.constructorInfo = cons;
tryNext:
                    ;
                }
            }
            return this.constructorInfo;
        }

        public override System.Reflection.MethodInfo GetMethodInfo()
        {
            return null;
        }
        public virtual object Invoke(params object[] arguments)
        {
            System.Reflection.ConstructorInfo constr = this.GetConstructorInfo();
            if(constr == null)
                return null;
            return constr.Invoke(arguments);
        }
        public virtual Literal Invoke(params Literal[] arguments)
        {
            int n = arguments == null ? 0 : arguments.Length;
            object[] args = n == 0 ? null : new object[n];
            if(args != null && arguments != null)
                for(int i = 0; i < n; i++)
                {
                    Literal lit = arguments[i];
                    args[i] = lit == null ? null : lit.Value;
                }
            return new Literal(this.Invoke(args));
        }

        //initializers never override a base class initializer
        public override bool OverridesBaseClassMember
        {
            get { return false; }
            set { }
        }
        public override Member OverriddenMember
        {
            get { return null; }
            set { }
        }
        public override Method OverriddenMethod
        {
            get { return null; }
            set { }
        }
        public override string ToString()
        {
            return this.DeclaringType.GetFullUnmangledNameWithTypeParameters() + "(" + this.Parameters + ")";
        }

        public virtual MemberList GetAttributeConstructorNamedParameters()
        {
            TypeNode type = this.DeclaringType;

            if(type == null || !type.IsAssignableTo(SystemTypes.Attribute) || type.Members == null)
                return null;

            MemberList memList = type.Members;
            int n = memList.Count;
            MemberList ml = new MemberList();

            for(int i = 0; i < n; ++i)
            {
                Property p = memList[i] as Property;

                if(p != null && p.IsPublic)
                {
                    if(p.Setter != null && p.Getter != null)
                        ml.Add(p);

                    continue;
                }

                Field f = memList[i] as Field;

                if(f != null && !f.IsInitOnly && f.IsPublic)
                    ml.Add(f);
            }

            return ml;
        }
    }

    public class StaticInitializer : Method
    {
        public StaticInitializer()
            : base()
        {
            this.NodeType = NodeType.StaticInitializer;
            this.Flags = MethodFlags.SpecialName | MethodFlags.RTSpecialName | MethodFlags.Static | MethodFlags.HideBySig | MethodFlags.Private;
            this.Name = StandardIds.CCtor;
            this.ReturnType = CoreSystemTypes.Void;
        }
        public StaticInitializer(MethodBodyProvider provider, object handle)
            : base(provider, handle)
        {
            this.NodeType = NodeType.StaticInitializer;
        }

        public StaticInitializer(TypeNode declaringType, AttributeList attributes, Block body)
            : base(declaringType, attributes, StandardIds.CCtor, null, null, body)
        {
            this.NodeType = NodeType.StaticInitializer;
            this.Flags = MethodFlags.SpecialName | MethodFlags.RTSpecialName | MethodFlags.Static | MethodFlags.HideBySig | MethodFlags.Private;
            this.Name = StandardIds.CCtor;
            this.ReturnType = CoreSystemTypes.Void;
        }
        public StaticInitializer(TypeNode declaringType, AttributeList attributes, Block body, TypeNode voidTypeExpression)
            : base(declaringType, attributes, StandardIds.CCtor, null, null, body)
        {
            this.NodeType = NodeType.StaticInitializer;
            this.Flags = MethodFlags.SpecialName | MethodFlags.RTSpecialName | MethodFlags.Static | MethodFlags.HideBySig | MethodFlags.Private;
            this.Name = StandardIds.CCtor;
            this.ReturnType = voidTypeExpression;
        }

        protected System.Reflection.ConstructorInfo constructorInfo;

        public virtual System.Reflection.ConstructorInfo GetConstructorInfo()
        {
            if(this.constructorInfo == null)
            {
                if(this.DeclaringType == null)
                    return null;
                Type t = this.DeclaringType.GetRuntimeType();
                if(t == null)
                    return null;
                ParameterList pars = this.Parameters;
                int n = pars == null ? 0 : pars.Count;
                Type[] types = new Type[n];
                for(int i = 0; i < n; i++)
                {
                    Parameter p = pars[i];
                    if(p == null || p.Type == null)
                        return null;
                    Type pt = types[i] = p.Type.GetRuntimeType();
                    if(pt == null)
                        return null;
                }
                System.Reflection.MemberInfo[] members = t.GetMember(this.Name.ToString(), System.Reflection.MemberTypes.Constructor,
                  BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach(System.Reflection.ConstructorInfo cons in members)
                {
                    if(cons == null)
                        continue;
                    System.Reflection.ParameterInfo[] parameters = cons.GetParameters();
                    int numPars = parameters == null ? 0 : parameters.Length;
                    if(numPars != n)
                        continue;
                    if(parameters != null)
                        for(int i = 0; i < n; i++)
                        {
                            System.Reflection.ParameterInfo par = parameters[i];
                            if(par == null || par.ParameterType != types[i])
                                goto tryNext;
                        }
                    return this.constructorInfo = cons;
tryNext:
                    ;
                }
            }
            return this.constructorInfo;
        }

        public override System.Reflection.MethodInfo GetMethodInfo()
        {
            return null;
        }

        //initializers never override a base class initializer
        public override bool OverridesBaseClassMember
        {
            get { return false; }
            set { }
        }
        public override Member OverriddenMember
        {
            get { return null; }
            set { }
        }
        public override Method OverriddenMethod
        {
            get { return null; }
            set { }
        }
    }

    public class FieldInitializerBlock : Block
    {
        public TypeNode Type;
        public bool IsStatic;
        public FieldInitializerBlock()
            : base()
        {
            this.NodeType = NodeType.FieldInitializerBlock;
        }
        public FieldInitializerBlock(TypeNode type, bool isStatic)
            : base()
        {
            this.NodeType = NodeType.FieldInitializerBlock;
            this.Type = type;
            this.IsStatic = isStatic;
        }
    }

    public class ParameterField : Field
    {
        protected Parameter parameter;
        public ParameterField()
        {
        }
        public ParameterField(TypeNode declaringType, AttributeList attributes, FieldFlags flags, Identifier name,
          TypeNode Type, Literal defaultValue)
            : base(declaringType, attributes, flags, name, Type, defaultValue)
        {
        }
        public virtual Parameter Parameter
        {
            get
            {
                return this.parameter;
            }
            set
            {
                this.parameter = value;
            }
        }
    }

    public class Field : Member
    {
        /// <summary>Provides a value that is assigned to the field upon initialization.</summary>
        public Expression Initializer;
        public TypeNode TypeExpression;
        public bool HasOutOfBandContract;
        public InterfaceList ImplementedInterfaces;
        public InterfaceList ImplementedInterfaceExpressions;
        // if this is the backing field for some event, then ForEvent is that event
        public Event ForEvent;
        public bool IsModelfield = false; //set to true if this field serves as the representation of a modelfield in a class

        public Field()
            : base(NodeType.Field)
        {
        }
        public Field(Identifier name)
            : base(NodeType.Field)
        {
            this.Name = name;
        }
        public Field(TypeNode declaringType, AttributeList attributes, FieldFlags flags, Identifier name,
          TypeNode type, Literal defaultValue)
            : base(declaringType, attributes, name, NodeType.Field)
        {
            this.defaultValue = defaultValue;
            this.flags = flags;
            this.type = type;
        }
        private Literal defaultValue;
        /// <summary>The compile-time value to be substituted for references to this field if it is a literal.</summary>
        public Literal DefaultValue
        { //TODO: rename this to LiteralValue
            get { return this.defaultValue; }
            set { this.defaultValue = value; }
        }
        private FieldFlags flags;
        public FieldFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }
        private int offset;
        public int Offset
        {
            get { return this.offset; }
            set { this.offset = value; }
        }
        private bool isVolatile;
        /// <summary>True if the field may not be cached. Used for sharing data between multiple threads.</summary>
        public bool IsVolatile
        {
            get { return this.isVolatile; }
            set { this.isVolatile = value; }
        }
        private TypeNode type;
        /// <summary>The type of values that may be stored in the field.</summary>
        public TypeNode Type
        {
            get { return this.type; }
            set { this.type = value; }
        }
        private MarshallingInformation marshallingInformation;
        public MarshallingInformation MarshallingInformation
        {
            get { return this.marshallingInformation; }
            set { this.marshallingInformation = value; }
        }
        private byte[] initialData;
        public byte[] InitialData
        {
            get { return this.initialData; }
            set { this.initialData = value; }
        }
        internal PESection section;
        public PESection Section
        {
            get { return this.section; }
            set { this.section = value; }
        }
        protected string fullName;
        public override string/*!*/ FullName
        {
            get
            {
                string result = this.fullName;
                if(result == null)
                    this.fullName = result = this.DeclaringType.FullName + "." + (this.Name == null ? "" : this.Name.ToString());
                return result;
            }
        }

        protected override Identifier GetDocumentationId()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("F:");
            if(this.DeclaringType == null)
                return Identifier.Empty;
            sb.Append(this.DeclaringType.FullName);
            sb.Append(".");
            if(this.Name == null)
                return Identifier.Empty;
            sb.Append(this.Name.Name);
            return Identifier.For(sb.ToString());
        }

        public static Field GetField(System.Reflection.FieldInfo fieldInfo)
        {
            if(fieldInfo == null)
                return null;
            TypeNode tn = TypeNode.GetTypeNode(fieldInfo.DeclaringType);
            if(tn == null)
                return null;
            return tn.GetField(Identifier.For(fieldInfo.Name));
        }

        protected System.Reflection.FieldInfo fieldInfo;
        public virtual System.Reflection.FieldInfo GetFieldInfo()
        {
            if(this.fieldInfo == null)
            {
                TypeNode tn = this.DeclaringType;
                if(tn == null)
                    return null;
                Type t = tn.GetRuntimeType();
                if(t == null)
                    return null;
                System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.DeclaredOnly;
                if(this.IsPublic)
                    flags |= System.Reflection.BindingFlags.Public;
                else
                    flags |= System.Reflection.BindingFlags.NonPublic;
                if(this.IsStatic)
                    flags |= System.Reflection.BindingFlags.Static;
                else
                    flags |= System.Reflection.BindingFlags.Instance;
                this.fieldInfo = t.GetField(this.Name.ToString(), flags);
            }
            return this.fieldInfo;
        }

        /// <summary>True if all references to the field are replaced with a value that is determined at compile-time.</summary>
        public virtual bool IsLiteral
        {
            get { return (this.Flags & FieldFlags.Literal) != 0; }
        }
        public override bool IsAssembly
        {
            get { return (this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.Assembly; }
        }
        public override bool IsCompilerControlled
        {
            get { return (this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.CompilerControlled; }
        }
        public override bool IsFamily
        {
            get { return (this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.Family; }
        }
        public override bool IsFamilyAndAssembly
        {
            get { return (this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.FamANDAssem; }
        }
        public override bool IsFamilyOrAssembly
        {
            get { return (this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.FamORAssem; }
        }
        /// <summary>True if the field may only be assigned to inside the constructor.</summary>
        public virtual bool IsInitOnly
        {
            get { return (this.Flags & FieldFlags.InitOnly) != 0; }
        }
        public override bool IsPrivate
        {
            get { return (this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.Private; }
        }
        public override bool IsPublic
        {
            get { return (this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.Public; }
        }
        public override bool IsSpecialName
        {
            get { return (this.Flags & FieldFlags.SpecialName) != 0; }
        }
        public override bool IsStatic
        {
            get { return (this.Flags & FieldFlags.Static) != 0; }
        }
        public override bool IsVisibleOutsideAssembly
        {
            get
            {
                if(this.DeclaringType != null && !this.DeclaringType.IsVisibleOutsideAssembly)
                    return false;
                switch(this.Flags & FieldFlags.FieldAccessMask)
                {
                    case FieldFlags.Public:
                        return true;
                    case FieldFlags.Family:
                    case FieldFlags.FamORAssem:
                        return this.DeclaringType != null && !this.DeclaringType.IsSealed;
                    default:
                        return false;
                }
            }
        }

        public virtual object GetValue(object targetObject)
        {
            System.Reflection.FieldInfo fieldInfo = this.GetFieldInfo();
            if(fieldInfo == null)
                return null;
            return fieldInfo.GetValue(targetObject);
        }
        public virtual Literal GetValue(Literal/*!*/ targetObject)
        {
            return new Literal(this.GetValue(targetObject.Value));
        }
        public virtual void SetValue(object targetObject, object value)
        {
            System.Reflection.FieldInfo fieldInfo = this.GetFieldInfo();
            if(fieldInfo == null)
                return;
            fieldInfo.SetValue(targetObject, value);
        }
        public virtual void SetValue(Literal/*!*/ targetObject, Literal/*!*/ value)
        {
            this.SetValue(targetObject.Value, value.Value);
        }

        public override string ToString()
        {
            return this.DeclaringType.GetFullUnmangledNameWithTypeParameters() + "." + this.Name;
        }
    }

    public class Property : Member
    {
        /// <summary>
        /// The list of types (just one in C#) that contain abstract or virtual properties that are explicitly implemented or overridden by this property.
        /// </summary>
        public TypeNodeList ImplementedTypes;
        public TypeNodeList ImplementedTypeExpressions;
        public bool IsModelfield = false;   //set to true if this property serves as the representation of a modelfield in an interface

        public Property() : base(NodeType.Property)
        {
        }

        public Property(TypeNode declaringType, AttributeList attributes, PropertyFlags flags, Identifier name,
          Method getter, Method setter)
            : base(declaringType, attributes, name, NodeType.Property)
        {
            this.flags = flags;
            this.getter = getter;
            this.setter = setter;
            if(getter != null)
                getter.DeclaringMember = this;
            if(setter != null)
                setter.DeclaringMember = this;
        }

        private PropertyFlags flags;
        public PropertyFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }
        private Method getter;
        /// <summary>The method that is called to get the value of this property. Corresponds to the get clause in C#.</summary>
        public Method Getter
        {
            get { return this.getter; }
            set { this.getter = value; }
        }
        private Method setter;
        /// <summary>The method that is called to set the value of this property. Corresponds to the set clause in C#.</summary>
        public Method Setter
        {
            get { return this.setter; }
            set { this.setter = value; }
        }
        private MethodList otherMethods;
        /// <summary>Other methods associated with the property. No equivalent in C#.</summary>
        public MethodList OtherMethods
        {
            get { return this.otherMethods; }
            set { this.otherMethods = value; }
        }
        protected string fullName;
        public override string/*!*/ FullName
        {
            get
            {
                if(this.fullName != null)
                    return this.fullName;
                StringBuilder sb = new StringBuilder();
                sb.Append(this.DeclaringType.FullName);
                sb.Append('.');
                if(this.Name != null)
                    sb.Append(this.Name.ToString());
                ParameterList parameters = this.Parameters;
                for(int i = 0, n = parameters == null ? 0 : parameters.Count; i < n; i++)
                {
                    Parameter par = parameters[i];
                    if(par == null || par.Type == null)
                        continue;
                    if(i == 0)
                        sb.Append('(');
                    else
                        sb.Append(',');
                    sb.Append(par.Type.FullName);
                    if(i == n - 1)
                        sb.Append(')');
                }
                return this.fullName = sb.ToString();
            }
        }

        public virtual Method GetBaseGetter()
        {
            TypeNode t = this.DeclaringType;
            if(t == null)
                return null;
            while(t.BaseType != null)
            {
                t = t.BaseType;
                MemberList mems = t.GetMembersNamed(this.Name);
                for(int i = 0, n = mems == null ? 0 : mems.Count; i < n; i++)
                {
                    Property bprop = mems[i] as Property;
                    if(bprop == null)
                        continue;
                    if(!bprop.ParametersMatch(this.Parameters))
                        continue;
                    if(bprop.Getter != null)
                        return bprop.Getter;
                }
            }
            return null;
        }
        public virtual Method GetBaseSetter()
        {
            TypeNode t = this.DeclaringType;
            if(t == null)
                return null;
            while(t.BaseType != null)
            {
                t = t.BaseType;
                MemberList mems = t.GetMembersNamed(this.Name);
                for(int i = 0, n = mems == null ? 0 : mems.Count; i < n; i++)
                {
                    Property bprop = mems[i] as Property;
                    if(bprop == null)
                        continue;
                    if(!bprop.ParametersMatch(this.Parameters))
                        continue;
                    if(bprop.Setter != null)
                        return bprop.Setter;
                }
            }
            return null;
        }

        protected override Identifier GetDocumentationId()
        {
            StringBuilder sb = new StringBuilder(this.DeclaringType.DocumentationId.ToString());
            sb[0] = 'P';
            sb.Append('.');
            if(this.Name != null)
                sb.Append(this.Name.ToString());
            ParameterList parameters = this.Parameters;
            for(int i = 0, n = parameters == null ? 0 : parameters.Count; i < n; i++)
            {
                Parameter par = parameters[i];
                if(par == null || par.Type == null)
                    continue;
                if(i == 0)
                    sb.Append('(');
                else
                    sb.Append(',');
                par.Type.AppendDocumentIdMangledName(sb, null, this.DeclaringType.TemplateParameters);
                if(i == n - 1)
                    sb.Append(')');
            }
            return Identifier.For(sb.ToString());
        }

        public static Property GetProperty(System.Reflection.PropertyInfo propertyInfo)
        {
            if(propertyInfo == null)
                return null;
            TypeNode tn = TypeNode.GetTypeNode(propertyInfo.DeclaringType);
            if(tn == null)
                return null;
            System.Reflection.ParameterInfo[] paramInfos = propertyInfo.GetIndexParameters();
            int n = paramInfos == null ? 0 : paramInfos.Length;
            TypeNode[] parameterTypes = new TypeNode[n];
            if(paramInfos != null)
                for(int i = 0; i < n; i++)
                {
                    System.Reflection.ParameterInfo param = paramInfos[i];
                    if(param == null)
                        return null;
                    parameterTypes[i] = TypeNode.GetTypeNode(param.ParameterType);
                }
            return tn.GetProperty(Identifier.For(propertyInfo.Name), parameterTypes);
        }

        protected System.Reflection.PropertyInfo propertyInfo;
        public virtual System.Reflection.PropertyInfo GetPropertyInfo()
        {
            if(this.propertyInfo == null)
            {
                if(this.DeclaringType == null)
                    return null;
                Type t = this.DeclaringType.GetRuntimeType();
                if(t == null)
                    return null;
                if(this.Type == null)
                    return null;
                Type retType = this.Type.GetRuntimeType();
                if(retType == null)
                    return null;
                ParameterList pars = this.Parameters;
                int n = pars == null ? 0 : pars.Count;
                Type[] types = new Type[n];
                for(int i = 0; i < n; i++)
                {
                    Parameter p = pars[i];
                    if(p == null || p.Type == null)
                        return null;
                    Type pt = types[i] = p.Type.GetRuntimeType();
                    if(pt == null)
                        return null;
                }
                System.Reflection.MemberInfo[] members =
                  t.GetMember(this.Name.ToString(), System.Reflection.MemberTypes.Property,
                  BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach(System.Reflection.PropertyInfo prop in members)
                {
                    if(prop == null || prop.PropertyType != retType)
                        continue;
                    System.Reflection.ParameterInfo[] parameters = prop.GetIndexParameters();
                    if(parameters == null || parameters.Length != n)
                        continue;
                    for(int i = 0; i < n; i++)
                    {
                        System.Reflection.ParameterInfo parInfo = parameters[i];
                        if(parInfo == null || parInfo.ParameterType != types[i])
                            goto tryNext;
                    }
                    return this.propertyInfo = prop;
tryNext:
                    ;
                }
            }
            return this.propertyInfo;
        }
        public virtual object GetValue(object targetObject, params object[] indices)
        {
            System.Reflection.PropertyInfo propInfo = this.GetPropertyInfo();
            if(propInfo == null)
                throw new InvalidOperationException();
            return propInfo.GetValue(targetObject, indices);
        }
        public virtual Literal GetValue(Literal/*!*/ targetObject, params Literal[] indices)
        {
            int n = indices == null ? 0 : indices.Length;
            object[] inds = n == 0 ? null : new object[n];
            if(inds != null && indices != null)
                for(int i = 0; i < n; i++)
                {
                    Literal lit = indices[i];
                    inds[i] = lit == null ? null : lit.Value;
                }
            return new Literal(this.GetValue(targetObject.Value, inds));
        }
        public virtual void SetValue(object targetObject, object value, params object[] indices)
        {
            System.Reflection.PropertyInfo propInfo = this.GetPropertyInfo();
            if(propInfo == null)
                throw new InvalidOperationException();
            propInfo.SetValue(targetObject, value, indices);
        }
        public virtual void SetValue(Literal/*!*/ targetObject, Literal/*!*/ value, params Literal[] indices)
        {
            int n = indices == null ? 0 : indices.Length;
            object[] inds = n == 0 ? null : new object[n];
            if(inds != null && indices != null)
                for(int i = 0; i < n; i++)
                {
                    Literal lit = indices[i];
                    inds[i] = lit == null ? null : lit.Value;
                }
            System.Reflection.PropertyInfo propInfo = this.GetPropertyInfo();
            if(propInfo == null)
                throw new InvalidOperationException();
            propInfo.SetValue(targetObject.Value, value.Value, inds);
        }

        public override string HelpText
        {
            get
            {
                if(this.helpText != null)
                    return this.helpText;
                StringBuilder sb = new StringBuilder(base.HelpText);
                // if there is already some help text, start the contract on a new line
                bool startWithNewLine = (sb.Length != 0);
                if(this.Getter != null && this.Getter.HelpText != null && this.Getter.HelpText.Length > 0)
                {
                    if(startWithNewLine)
                    {
                        sb.Append("\n");
                        startWithNewLine = false;
                    }
                    sb.Append("get\n");
                    int i = sb.Length;
                    sb.Append(this.Getter.HelpText);
                    if(sb.Length > i)
                        startWithNewLine = true;
                }
                if(this.Setter != null && this.Setter.HelpText != null && this.Setter.HelpText.Length > 0)
                {
                    if(startWithNewLine)
                    {
                        sb.Append("\n");
                        startWithNewLine = false;
                    }
                    sb.Append("set\n");
                    sb.Append(this.Setter.HelpText);
                }
                return this.helpText = sb.ToString();
            }
            set
            {
                base.HelpText = value;
            }
        }

        public override bool IsAssembly
        {
            get { return Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.Assembly; }
        }
        public override bool IsCompilerControlled
        {
            get { return Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.CompilerControlled; }
        }
        public override bool IsFamily
        {
            get { return Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.Family; }
        }
        public override bool IsFamilyAndAssembly
        {
            get { return Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.FamANDAssem; }
        }
        public override bool IsFamilyOrAssembly
        {
            get { return Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.FamORAssem; }
        }
        public bool IsFinal
        {
            get { return (this.Getter == null || this.Getter.IsFinal) && (this.Setter == null || this.Setter.IsFinal); }
        }
        public override bool IsPrivate
        {
            get { return Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.Private; }
        }
        public override bool IsPublic
        {
            get { return Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.Public; }
        }
        public override bool IsSpecialName
        {
            get { return (this.Flags & PropertyFlags.SpecialName) != 0; }
        }
        public override bool IsStatic
        {
            get { return (this.Getter == null || this.Getter.IsStatic) && (this.Setter == null || this.Setter.IsStatic); }
        }
        /// <summary>
        /// True if this property can in principle be overridden by a property in a derived class.
        /// </summary>
        public bool IsVirtual
        {
            get { return (this.Getter == null || this.Getter.IsVirtual) && (this.Setter == null || this.Setter.IsVirtual); }
        }
        public override bool IsVisibleOutsideAssembly
        {
            get { return (this.Getter != null && this.Getter.IsVisibleOutsideAssembly) || (this.Setter != null && this.Setter.IsVisibleOutsideAssembly); }
        }
        public static readonly Property NotSpecified = new Property();
        public override Member HiddenMember
        {
            get
            {
                return this.HiddenProperty;
            }
            set
            {
                this.HiddenProperty = (Property)value;
            }
        }
        protected Property hiddenProperty;
        public virtual Property HiddenProperty
        {
            get
            {
                if(this.hiddenMember == Property.NotSpecified)
                    return null;
                Property hiddenProperty = this.hiddenMember as Property;
                if(hiddenProperty != null)
                    return hiddenProperty;

                Method hiddenGetter = this.Getter == null ? null : this.Getter.HiddenMethod;
                Method hiddenSetter = this.Setter == null ? null : this.Setter.HiddenMethod;
                Property hiddenGetterProperty = hiddenGetter == null ? null : hiddenGetter.DeclaringMember as Property;
                Property hiddenSetterProperty = hiddenSetter == null ? null : hiddenSetter.DeclaringMember as Property;
                hiddenProperty = hiddenGetterProperty;
                if(hiddenSetterProperty != null)
                {
                    if(hiddenProperty == null ||
                      (hiddenSetterProperty.DeclaringType != null && hiddenSetterProperty.DeclaringType.IsDerivedFrom(hiddenProperty.DeclaringType)))
                        hiddenProperty = hiddenSetterProperty;
                }
                this.hiddenMember = hiddenProperty;
                return hiddenProperty;
            }
            set
            {
                this.hiddenMember = value;
            }
        }
        public override Member OverriddenMember
        {
            get
            {
                return this.OverriddenProperty;
            }
            set
            {
                this.OverriddenProperty = (Property)value;
            }
        }
        protected Property overriddenProperty;
        public virtual Property OverriddenProperty
        {
            get
            {
                if(this.overriddenMember == Property.NotSpecified)
                    return null;
                Property overriddenProperty = this.overriddenMember as Property;
                if(overriddenProperty != null)
                    return overriddenProperty;

                Method overriddenGetter = this.Getter == null ? null : this.Getter.OverriddenMethod;
                Method overriddenSetter = this.Setter == null ? null : this.Setter.OverriddenMethod;
                Property overriddenGetterProperty = overriddenGetter == null ? null : overriddenGetter.DeclaringMember as Property;
                Property overriddenSetterProperty = overriddenSetter == null ? null : overriddenSetter.DeclaringMember as Property;
                overriddenProperty = overriddenGetterProperty;
                if(overriddenSetterProperty != null)
                {
                    if(overriddenProperty == null ||
                      (overriddenSetterProperty.DeclaringType != null && overriddenSetterProperty.DeclaringType.IsDerivedFrom(overriddenProperty.DeclaringType)))
                        overriddenProperty = overriddenSetterProperty;
                }
                this.overriddenMember = overriddenProperty;
                return overriddenProperty;
            }
            set
            {
                this.overriddenMember = value;
            }
        }
        private ParameterList parameters;
        /// <summary>
        /// The parameters of this property if it is an indexer.
        /// </summary>
        public ParameterList Parameters
        {
            get
            {
                if(this.parameters != null)
                    return this.parameters;
                if(this.Getter != null)
                    return this.parameters = this.Getter.Parameters;
                ParameterList setterPars = this.Setter == null ? null : this.Setter.Parameters;
                int n = setterPars == null ? 0 : setterPars.Count - 1;

                ParameterList propPars = this.parameters = new ParameterList();

                if(setterPars != null)
                    for(int i = 0; i < n; i++)
                        propPars.Add(setterPars[i]);

                return propPars;
            }
            set
            {
                this.parameters = value;
            }
        }
        public virtual bool ParametersMatch(ParameterList parameters)
        {
            ParameterList pars = this.Parameters;
            int n = pars == null ? 0 : pars.Count;
            int m = parameters == null ? 0 : parameters.Count;
            if(n != m)
                return false;
            if(parameters == null)
                return true;
            for(int i = 0; i < n; i++)
            {
                Parameter par1 = pars[i];
                Parameter par2 = parameters[i];
                if(par1.Type != par2.Type)
                    return false;
            }
            return true;
        }
        public virtual bool ParametersMatchStructurally(ParameterList parameters)
        {
            ParameterList pars = this.Parameters;
            int n = pars == null ? 0 : pars.Count;
            int m = parameters == null ? 0 : parameters.Count;
            if(n != m)
                return false;
            if(parameters == null)
                return true;
            for(int i = 0; i < n; i++)
            {
                Parameter par1 = pars[i];
                Parameter par2 = parameters[i];
                if(par1 == null || par2 == null)
                    return false;
                if(par1.Type == null || par2.Type == null)
                    return false;
                if(par1.Type != par2.Type && !par1.Type.IsStructurallyEquivalentTo(par2.Type))
                    return false;
            }
            return true;
        }
        public virtual bool ParameterTypesMatch(TypeNodeList argumentTypes)
        {
            ParameterList pars = this.Parameters;
            int n = pars == null ? 0 : pars.Count;
            int m = argumentTypes == null ? 0 : argumentTypes.Count;
            if(n != m)
                return false;
            if(argumentTypes == null)
                return true;
            for(int i = 0; i < n; i++)
            {
                Parameter par = this.Parameters[i];
                if(par == null)
                    return false;
                TypeNode argType = argumentTypes[i];
                if(par.Type != argType)
                    return false;
            }
            return true;
        }
        protected TypeNode type;
        /// <summary>
        /// The type of value that this property holds.
        /// </summary>
        public virtual TypeNode Type
        {
            get
            {
                if(this.type != null)
                    return this.type;
                if(this.Getter != null)
                    return this.type = this.Getter.ReturnType;
                if(this.Setter != null && this.Setter.Parameters != null)
                    return this.type = this.Setter.Parameters[this.Setter.Parameters.Count - 1].Type;
                return CoreSystemTypes.Object;
            }
            set
            {
                this.type = value;
            }
        }

        public TypeNode TypeExpression;

        public override string ToString()
        {
            return this.DeclaringType.GetFullUnmangledNameWithTypeParameters() + "." + this.Name;
        }
    }
    public class Variable : Expression
    {
        private Identifier name;

        public TypeNode TypeExpression;

        public Variable(NodeType type)
            : base(type)
        {
        }
        /// <summary>The name of a stack location. For example the name of a local variable or the name of a method parameter.</summary>
        public Identifier Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }
    public class Parameter : Variable
    {
        private AttributeList attributes;
        /// <summary>The (C# custom) attributes of this parameter.</summary>
        public AttributeList Attributes
        {
            get
            {
                if(this.attributes != null)
                    return this.attributes;
                return this.attributes = new AttributeList();
            }
            set { this.attributes = value; }
        }
        private Expression defaultValue;
        /// <summary>The value that should be supplied as the argument value of this optional parameter if the source code omits an explicit argument value.</summary>
        public Expression DefaultValue
        {
            get { return this.defaultValue; }
            set { this.defaultValue = value; }
        }
        private ParameterFlags flags;
        public ParameterFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }
        private MarshallingInformation marshallingInformation;
        public MarshallingInformation MarshallingInformation
        {
            get { return this.marshallingInformation; }
            set { this.marshallingInformation = value; }
        }
        private Method declaringMethod;
        public Method DeclaringMethod
        {
            get { return this.declaringMethod; }
            set { this.declaringMethod = value; }
        }
        private int parameterListIndex;
        /// <summary>
        /// Zero based index into a parameter list containing this parameter.
        /// </summary>
        public int ParameterListIndex
        {
            get { return this.parameterListIndex; }
            set { this.parameterListIndex = value; }
        }
        private int argumentListIndex;
        /// <summary>
        /// Zero based index into the list of arguments on the evaluation stack. 
        /// Instance methods have the this object as parameter zero, which means that the first parameter will have value 1, not 0.
        /// </summary>
        public int ArgumentListIndex
        {
            get { return this.argumentListIndex; }
            set { this.argumentListIndex = value; }
        }
        public Parameter()
            : base(NodeType.Parameter)
        {
        }
        public Parameter(Identifier name, TypeNode type)
            : base(NodeType.Parameter)
        {
            this.Name = name;
            this.Type = type;
        }

        public Parameter(AttributeList attributes, ParameterFlags flags, Identifier name, TypeNode type,
          Literal defaultValue, MarshallingInformation marshallingInformation)
            : base(NodeType.Parameter)
        {
            this.attributes = attributes;
            this.defaultValue = defaultValue;
            this.flags = flags;
            this.marshallingInformation = marshallingInformation;
            this.Name = name;
            this.Type = type;
        }

        /// <summary>
        /// True if the corresponding argument value is used by the callee. (This need not be the case for a parameter marked as IsOut.) 
        /// </summary>
        public virtual bool IsIn
        {
            get
            {
                return (this.Flags & ParameterFlags.In) != 0;
            }
            set
            {
                if(value)
                    this.Flags |= ParameterFlags.In;
                else
                    this.Flags &= ~ParameterFlags.In;
            }
        }
        /// <summary>
        /// True if the caller can omit providing an argument for this parameter.
        /// </summary>
        public virtual bool IsOptional
        {
            get
            {
                return (this.Flags & ParameterFlags.Optional) != 0;
            }
            set
            {
                if(value)
                    this.Flags |= ParameterFlags.Optional;
                else
                    this.Flags &= ~ParameterFlags.Optional;
            }
        }
        /// <summary>
        /// True if the corresponding argument must be a left hand expression and will be updated when the call returns.
        /// </summary>
        public virtual bool IsOut
        {
            get
            {
                return (this.Flags & ParameterFlags.Out) != 0;
            }
            set
            {
                if(value)
                    this.Flags |= ParameterFlags.Out;
                else
                    this.Flags &= ~ParameterFlags.Out;
            }
        }

        protected TypeNode paramArrayElementType = null;

        /// <summary>
        /// If the parameter is a param array, this returns the element type of the array. If not, it returns null.
        /// </summary>
        public virtual TypeNode GetParamArrayElementType()
        {
            TypeNode result = this.paramArrayElementType;
            if(result == null)
            {
                AttributeNode attr = this.GetParamArrayAttribute();
                if(attr != null)
                {
                    TypeNode t = TypeNode.StripModifiers(this.Type);
                    Reference r = t as Reference;
                    if(r != null)
                        t = r.ElementType;
                    ArrayType arr = t as ArrayType;
                    if(arr != null && arr.Rank == 1)
                        return this.paramArrayElementType = arr.ElementType;
                }
                this.paramArrayElementType = result = Class.DoesNotExist;
            }
            if(result == Class.DoesNotExist)
                return null;
            return result;
        }
        protected AttributeNode paramArrayAttribute = null;
        public virtual AttributeNode GetParamArrayAttribute()
        {
            AttributeNode result = this.paramArrayAttribute;
            if(result == null)
            {
                AttributeList attributes = this.Attributes;
                for(int i = 0, n = attributes == null ? 0 : attributes.Count; i < n; i++)
                {
                    AttributeNode attr = attributes[i];
                    if(attr == null)
                        continue;
                    TypeNode attrType = null;
                    MemberBinding mb = attr.Constructor as MemberBinding;
                    if(mb != null)
                        attrType = mb.BoundMember.DeclaringType;
                    else
                    {
                        Literal lit = attr.Constructor as Literal;
                        if(lit == null)
                            continue;
                        attrType = lit.Value as TypeNode;
                    }
                    if(attrType == SystemTypes.ParamArrayAttribute)
                        return this.paramArrayAttribute = attr;
                }
                result = this.paramArrayAttribute = AttributeNode.DoesNotExist;
            }
            if(result == AttributeNode.DoesNotExist)
                return null;
            return result;
        }
        public override bool Equals(object obj)
        {
            ParameterBinding binding = obj as ParameterBinding;
            return obj == this || binding != null && binding.BoundParameter == this;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// Gets the first attribute of the given type in the attribute list of this parameter. Returns null if none found.
        /// This should not be called until the AST containing this member has been processed to replace symbolic references
        /// to members with references to the actual members.
        /// </summary>
        public virtual AttributeNode GetAttribute(TypeNode attributeType)
        {
            if(attributeType == null)
                return null;
            AttributeList attributes = this.Attributes;
            for(int i = 0, n = attributes == null ? 0 : attributes.Count; i < n; i++)
            {
                AttributeNode attr = attributes[i];
                if(attr == null)
                    continue;
                MemberBinding mb = attr.Constructor as MemberBinding;
                if(mb != null)
                {
                    if(mb.BoundMember == null)
                        continue;
                    if(mb.BoundMember.DeclaringType != attributeType)
                        continue;
                    return attr;
                }
                Literal lit = attr.Constructor as Literal;
                if(lit == null)
                    continue;
                if((lit.Value as TypeNode) != attributeType)
                    continue;
                return attr;
            }
            return null;
        }

        public override string ToString()
        {
            if(this.Name == null)
                return "";
            if(this.Type == null)
                return this.Name.ToString();
            return this.Type.ToString() + " " + this.Name.ToString();
        }
    }

    public class ParameterBinding : Parameter, IUniqueKey
    {
        public Parameter/*!*/ BoundParameter;

        public ParameterBinding(Parameter/*!*/ boundParameter, SourceContext sctx)
        {
            if(boundParameter == null)
                throw new ArgumentNullException("boundParameter");
            this.BoundParameter = boundParameter;
            this.SourceContext = sctx;
            this.Type = boundParameter.Type;
            this.Name = boundParameter.Name;
            this.TypeExpression = boundParameter.TypeExpression;
            this.Attributes = boundParameter.Attributes;
            this.DefaultValue = boundParameter.DefaultValue;
            this.Flags = boundParameter.Flags;
            this.MarshallingInformation = boundParameter.MarshallingInformation;
            this.DeclaringMethod = boundParameter.DeclaringMethod;
            this.ParameterListIndex = boundParameter.ParameterListIndex;
            this.ArgumentListIndex = boundParameter.ArgumentListIndex;
            //^ base();
        }
        public override int GetHashCode()
        {
            return this.BoundParameter.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            ParameterBinding pb = obj as ParameterBinding;
            if(pb != null)
                return this.BoundParameter.Equals(pb.BoundParameter);
            else
                return this.BoundParameter.Equals(obj);
        }
        int IUniqueKey.UniqueId
        {
            get { return this.BoundParameter.UniqueKey; }
        }
    }

    public class Local : Variable
    {
        public Block DeclaringBlock;
        public bool InitOnly;
        public int Index;

        public Local()
            : base(NodeType.Local)
        {
        }
        public Local(TypeNode type)
            : base(NodeType.Local)
        {
            this.Name = Identifier.Empty;
            if(type == null)
                type = CoreSystemTypes.Object;
            this.Type = type;
        }
        public Local(Identifier name, TypeNode type)
            : this(type)
        {
            this.Name = name;
        }

        public Local(TypeNode type, SourceContext context)
            : this(Identifier.Empty, type, null)
        {
            this.SourceContext = context;
        }
        public Local(Identifier name, TypeNode type, SourceContext context)
            : this(name, type, null)
        {
            this.SourceContext = context;
        }
        public Local(Identifier name, TypeNode type, Block declaringBlock)
            : base(NodeType.Local)
        {
            this.DeclaringBlock = declaringBlock;
            this.Name = name;
            if(type == null)
                type = CoreSystemTypes.Object;
            this.Type = type;
        }

        private bool pinned;
        public bool Pinned
        {
            get { return this.pinned; }
            set { this.pinned = value; }
        }

        public override bool Equals(object obj)
        {
            LocalBinding binding = obj as LocalBinding;
            return obj == this || binding != null && binding.BoundLocal == this;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            if(this.Name == null)
                return "No name";
            return this.Name.ToString();
        }
    }

    public class LocalBinding : Local, IUniqueKey
    {
        public Local/*!*/ BoundLocal;

        public LocalBinding(Local/*!*/ boundLocal, SourceContext sctx)
        {
            if(boundLocal == null)
                throw new ArgumentNullException("boundLocal");
            this.BoundLocal = boundLocal;
            //^ base();
            this.SourceContext = sctx;
            this.Type = boundLocal.Type;
            this.Name = boundLocal.Name;
            this.TypeExpression = boundLocal.TypeExpression;
            this.DeclaringBlock = boundLocal.DeclaringBlock;
            this.Pinned = boundLocal.Pinned;
            this.InitOnly = boundLocal.InitOnly;
            this.Index = boundLocal.Index;
        }
        public override int GetHashCode()
        {
            return this.BoundLocal.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            LocalBinding lb = obj as LocalBinding;
            if(lb != null)
                return this.BoundLocal.Equals(lb.BoundLocal);
            else
                return this.BoundLocal.Equals(obj);
        }
        int IUniqueKey.UniqueId
        {
            get { return this.BoundLocal.UniqueKey; }
        }
    }

    /// <summary>
    /// A named container of types and nested namespaces. 
    /// The name of the container implicitly qualifies the names of the contained types and namespaces.
    /// </summary>
    public class Namespace : Member
    {
        /// <summary>The FullName of the namespace in the form of an Identifier rather than in the form of a string.</summary>
        public Identifier FullNameId;

        /// <summary>
        /// Provides alternative names for types and nested namespaces. Useful for introducing shorter names or for resolving name clashes.
        /// The names should be added to the scope associated with this namespace.
        /// </summary>
        public AliasDefinitionList AliasDefinitions;
        /// <summary>
        /// The list of namespaces that are fully contained inside this namespace.
        /// </summary>
        public NamespaceList NestedNamespaces;
        /// <summary>
        /// The Universal Resource Identifier that should be associated with all declarations inside this namespace.
        /// Typically used when the types inside the namespace are serialized as an XML Schema Definition. (XSD)
        /// </summary>
        public Identifier URI;
        /// <summary>
        /// The list of the namespaces of types that should be imported into the scope associated with this namespace.
        /// </summary>
        public UsedNamespaceList UsedNamespaces;

        /// <summary>
        /// A delegate that is called the first time Types is accessed. Provides for incremental construction of the namespace node.
        /// </summary>
        public TypeProvider ProvideTypes;
        /// <summary>
        /// Opaque information passed as a parameter to the delegate in ProvideTypes. Typically used to associate this namespace
        /// instance with a helper object.
        /// </summary>
        public object ProviderHandle;
        /// <summary>
        /// A method that fills in the Types property of the given namespace. Must not leave Types null.
        /// </summary>
        public delegate void TypeProvider(Namespace @namespace, object handle);

        protected string fullName;
        protected TypeNodeList types;

        public Namespace()
            : base(NodeType.Namespace)
        {
        }
        public Namespace(Identifier name)
            : base(NodeType.Namespace)
        {
            this.Name = name;
            this.FullNameId = name;
            if(name != null)
                this.fullName = name.ToString();
        }

        public Namespace(Identifier name, TypeProvider provideTypes, object providerHandle)
            : base(NodeType.Namespace)
        {
            this.Name = name;
            this.FullNameId = name;
            if(name != null)
                this.fullName = name.ToString();
            this.ProvideTypes = provideTypes;
            this.ProviderHandle = providerHandle;
        }
        public Namespace(Identifier name, Identifier fullName, AliasDefinitionList aliasDefinitions, UsedNamespaceList usedNamespaces,
          NamespaceList nestedNamespaces, TypeNodeList types)
            : base(NodeType.Namespace)
        {
            this.Name = name;
            this.FullNameId = fullName;
            if(fullName != null)
                this.fullName = fullName.ToString();
            this.AliasDefinitions = aliasDefinitions;
            this.NestedNamespaces = nestedNamespaces;
            this.Types = types;
            this.UsedNamespaces = usedNamespaces;
        }

        public override string/*!*/ FullName
        {
            get { return this.fullName == null ? "" : this.fullName; }
        }
        public override bool IsAssembly { get { return false; } }
        public override bool IsCompilerControlled { get { return false; } }
        public override bool IsFamily { get { return false; } }
        public override bool IsFamilyAndAssembly { get { return false; } }
        public override bool IsFamilyOrAssembly { get { return false; } }
        public override bool IsPrivate { get { return !this.IsPublic; } }
        public override bool IsPublic { get { return this.isPublic; } }
        protected internal bool isPublic;
        public override bool IsSpecialName { get { return false; } }
        public override bool IsStatic { get { return false; } }
        public override bool IsVisibleOutsideAssembly { get { return false; } }
        /// <summary>
        /// The list of types contained inside this namespace. If the value of Types is null and the value of ProvideTypes is not null, the 
        /// TypeProvider delegate is called to fill in the value of this property.
        /// </summary>
        public TypeNodeList Types
        {
            get
            {
                if(this.types == null)
                    if(this.ProvideTypes != null)
                        lock(this)
                        {
                            if(this.types == null)
                            {
                                this.ProvideTypes(this, this.ProviderHandle);
                            }
                        }
                    else
                        this.types = new TypeNodeList();
                return this.types;
            }
            set
            {
                this.types = value;
            }
        }
    }

    /// <summary>
    /// The root node of an Abstract Syntax Tree. Typically corresponds to multiple source files compiled to form a single target.
    /// </summary>
    public class Compilation : Node
    {
        /// <summary>
        /// The compilation parameters that are used for this compilation.
        /// </summary>
        public System.CodeDom.Compiler.CompilerParameters CompilerParameters;
        /// <summary>
        /// The target code object that is produced as a result of this compilation.
        /// </summary>
        public Module TargetModule;
        /// <summary>
        /// A list of all the compilation units (typically source files) that make up this compilation.
        /// </summary>
        public CompilationUnitList CompilationUnits;
        /// <summary>
        /// A scope for symbols that belong to the compilation as a whole. No C# equivalent. Null if not applicable.
        /// </summary>
        public Scope GlobalScope;

        public DateTime LastModified = DateTime.Now;
        public DateTime LastCompiled = DateTime.MinValue;

        public Compilation()
            : base(NodeType.Compilation)
        {
        }
        public Compilation(Module targetModule, CompilationUnitList compilationUnits, System.CodeDom.Compiler.CompilerParameters compilerParameters, Scope globalScope)
            : base(NodeType.Compilation)
        {
            this.CompilationUnits = compilationUnits;
            this.TargetModule = targetModule;
            this.CompilerParameters = compilerParameters;
            this.GlobalScope = globalScope;
        }

        public virtual Compilation CloneCompilationUnits()
        {
            Compilation clone = (Compilation)base.Clone();
            CompilationUnitList cus = this.CompilationUnits;

            if(cus != null)
            {
                clone.CompilationUnits = cus = new CompilationUnitList(cus);

                for(int i = 0, n = cus.Count; i < n; i++)
                {
                    CompilationUnit cu = cus[i];

                    if(cu == null)
                        continue;

                    cus[i] = cu = (CompilationUnit)cu.Clone();
                    cu.Compilation = clone;
                    cu.Nodes = null;
                }
            }

            return clone;
        }
    }
    /// <summary>
    /// The root node of an Abstract Syntax Tree. Corresponds to the starting production of the syntax. Equivalent to C# compilation-unit.
    /// Typically a compilation unit corresponds to a single source file.
    /// </summary>
    public class CompilationUnit : Node
    {
        /// <summary>
        /// An identifier that can be used to retrieve the source text of the compilation unit.
        /// </summary>
        public Identifier Name;
        /// <summary>
        /// An anonymous (name is Identifier.Empty) namespace holding types and nested namespaces.
        /// </summary>
        public NodeList Nodes;
        /// <summary>
        /// The preprocessor symbols that are to treated as defined when compiling this CompilationUnit into the TargetModule.
        /// </summary>
        public Hashtable PreprocessorDefinedSymbols;
        /// <summary>
        /// Pragma warning information.
        /// </summary>
        public TrivialHashtable PragmaWarnInformation;
        /// <summary>
        /// The compilation of which this unit forms a part.
        /// </summary>
        public Compilation Compilation;

        public CompilationUnit()
            : base(NodeType.CompilationUnit)
        {
        }
        public CompilationUnit(Identifier name)
            : base(NodeType.CompilationUnit)
        {
            this.Name = name;
        }
    }
    public class CompilationUnitSnippet : CompilationUnit
    {
        public DateTime LastModified = DateTime.Now;
        public IParserFactory ParserFactory;
        public Method ChangedMethod;
        public int OriginalEndPosOfChangedMethod;

        public CompilationUnitSnippet()
        {
            this.NodeType = NodeType.CompilationUnitSnippet;
        }
        public CompilationUnitSnippet(Identifier name, IParserFactory parserFactory, SourceContext sctx)
        {
            this.NodeType = NodeType.CompilationUnitSnippet;
            this.Name = name;
            this.ParserFactory = parserFactory;
            this.SourceContext = sctx;
        }
    }
    public abstract class Composer
    {
        public abstract Node Compose(Node node, Composer context, bool hasContextReference, Class scope);
        private class NullComposer : Composer
        {
            public override Node Compose(Node node, Composer context, bool hasContextReference, Class scope)
            {
                return node;
            }
        }
        public static readonly Composer Null = new NullComposer();
    }
    public class Composition : Expression
    {
        public Expression Expression;
        public Composer Composer;
        public Class Scope;
        public Composition(Expression exp, Composer composer, Class scope)
            : base(NodeType.Composition)
        {
            this.Expression = exp;
            this.Composer = composer;
            this.Scope = scope;
            if(exp != null)
                this.Type = exp.Type;
        }
    }

    /// <summary>
    /// An object that knows how to produce a particular scope's view of a type.
    /// </summary>
    public class TypeViewer
    {
        /// <summary>
        /// Return a scope's view of the argument type, where the scope's view is represented
        /// by a type viewer.
        /// [The identity function, except for dialects (e.g. Extensible Sing#) that allow
        /// extensions and differing views of types].
        /// Defined as a static method to allow the type viewer to be null,
        /// meaning an identity-function view.
        /// </summary>
        public static TypeNode/*!*/ GetTypeView(TypeViewer typeViewer, TypeNode/*!*/ type)
        {
            return typeViewer == null ? type.EffectiveTypeNode : typeViewer.GetTypeView(type);
        }

        /// <summary>
        /// Return the typeViewer's view of the argument type.  Overridden by subclasses
        /// that support non-identity-function type viewers, e.g. Extensible Sing#.
        /// </summary>
        protected virtual TypeNode/*!*/ GetTypeView(TypeNode/*!*/ type)
        {
            return type.EffectiveTypeNode;
        }
    }

    static class PlatformHelpers
    {
        internal static bool TryParseInt32(String s, out Int32 result)
        {
            return Int32.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }
        internal static int StringCompareOrdinalIgnoreCase(string strA, int indexA, string strB, int indexB, int length)
        {
            return string.Compare(strA, indexA, strB, indexB, length, StringComparison.OrdinalIgnoreCase);
        }
        internal static int StringCompareOrdinalIgnoreCase(string strA, string strB)
        {
            return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);
        }
    }
}
