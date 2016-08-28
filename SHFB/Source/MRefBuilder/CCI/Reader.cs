// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 11/21/2013 - EFW - Cleared out the conditional statements and updated based on changes to ListTemplate.cs.
// 12/15/2013 - EFW - Fixed a bug found when parsing the .NET 4.5.1 Framework assemblies
// 08/23/2016 - EFW - Added support for reading source code context from PDB files

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Marshal = System.Runtime.InteropServices.Marshal;
using System.Runtime.InteropServices;

using Sandcastle.Core;

namespace System.Compiler.Metadata
{
    enum CorOpenFlags : uint
    {
        ofRead = 0x00000000,     // Open scope for read
        ofWrite = 0x00000001,     // Open scope for write.
        ofCopyMemory = 0x00000002,     // Open scope with memory. Ask metadata to maintain its own copy of memory.
        ofCacheImage = 0x00000004,     // EE maps but does not do relocations or verify image
        ofNoTypeLib = 0x00000080,     // Don't OpenScope on a typelib.
    }
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("809c652e-7396-11d2-9771-00A0C9B4D50C")]
    interface IMetaDataDispenser
    {
        void DefineScope(ref Guid clsid, uint createFlags, [In] ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object retval);
        [PreserveSig]
        int OpenScope(string scope, uint openFlags, [In] ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object import);
        void OpenScopeOnMemory(IntPtr data, uint dataSize, uint openFlags, [In] ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object retval);
    }
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("AA544D42-28CB-11d3-BD22-0000F80849BD")]
    interface ISymUnmanagedBinder
    {
        [PreserveSig]
        int GetReaderForFile([MarshalAs(UnmanagedType.IUnknown)] object importer, string filename, string searchPath, out ISymUnmanagedReader reader);
        ISymUnmanagedReader GetReaderForStream([MarshalAs(UnmanagedType.IUnknown)] object importer, [MarshalAs(UnmanagedType.IUnknown)] object stream);
    }
    [ComImport, Guid("ACCEE350-89AF-4ccb-8B40-1C2C4C6F9434"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false)]
    interface ISymUnmanagedBinder2 : ISymUnmanagedBinder
    {
        void GetReaderForFile(IntPtr importer, [MarshalAs(UnmanagedType.LPWStr)] String filename, [MarshalAs(UnmanagedType.LPWStr)] String SearchPath, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedReader retVal);
        void GetReaderFromStream(IntPtr importer, IntPtr stream, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedReader retVal);
        [PreserveSig]
        int GetReaderForFile2([MarshalAs(UnmanagedType.IUnknown)] object importer, [MarshalAs(UnmanagedType.LPWStr)] String fileName, [MarshalAs(UnmanagedType.LPWStr)] String searchPath, int searchPolicy, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedReader pRetVal);
        //    void GetReaderForFile3(IntPtr importer, [MarshalAs(UnmanagedType.LPWStr)] String fileName, [MarshalAs(UnmanagedType.LPWStr)] String searchPath, int searchPolicy, IntPtr callback, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedReader pRetVal);
    }
    [ComImport, Guid("AA544D41-28CB-11d3-BD22-0000F80849BD")]
    class CorSymBinder
    {
    }
    [ComImport, Guid("0A29FF9E-7F9C-4437-8B11-F424491E3931")]
    class CorSymBinder2
    {
    }
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B4CE6286-2A6B-3712-A3B7-1EE1DAD467B5")]
    interface ISymUnmanagedReader
    {
        ISymUnmanagedDocument GetDocument(string url, ref Guid language, ref Guid languageVendor, ref Guid documentType);
        void GetDocuments(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedDocument[] docs);
        uint GetUserEntryPoint();
        [PreserveSig]
        int GetMethod(uint token, ref ISymUnmanagedMethod method);
        ISymUnmanagedMethod GetMethodByVersion(uint token, int version);
        void GetVariables(uint parent, uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ISymUnmanagedVariable[] vars);
        void GetGlobalVariables(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] vars);
        ISymUnmanagedMethod GetMethodFromDocumentPosition(ISymUnmanagedDocument document, uint line, uint column);
        void GetSymAttribute(uint parent, string name, ulong size, ref uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer);
        void GetNamespaces(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] namespaces);
        void Initialize([MarshalAs(UnmanagedType.IUnknown)] object importer, string filename, string searchPath, [MarshalAs(UnmanagedType.IUnknown)] object stream);
        void UpdateSymbolStore(string filename, [MarshalAs(UnmanagedType.IUnknown)] object stream);
        void ReplaceSymbolStore(string filename, [MarshalAs(UnmanagedType.IUnknown)] object stream);
        void GetSymbolStoreFileName(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] name);
        void GetMethodsFromDocumentPosition(ISymUnmanagedDocument document, uint line, uint column, uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] ISymUnmanagedMethod[] retval);
        void GetDocumentVersion(ISymUnmanagedDocument doc, out int version, out bool isLatest);
        void GetMethodVersion(ISymUnmanagedMethod method, out int version);
    }
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B62B923C-B500-3158-A543-24F307A8B7E1")]
    interface ISymUnmanagedMethod
    {
        uint GetToken();
        uint GetSequencePointCount();
        ISymUnmanagedScope GetRootScope();
        ISymUnmanagedScope GetScopeFromOffset(uint offset);
        uint Getoffset(ISymUnmanagedDocument document, uint line, uint column);
        void GetRanges(ISymUnmanagedDocument document, uint line, uint column, uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] ranges);
        void GetParameters(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] parms);
        IntPtr GetNamespace();
        bool GetSourceStartEnd([MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] ISymUnmanagedDocument[] docs, [MarshalAs(UnmanagedType.LPArray)] uint[] lines, [MarshalAs(UnmanagedType.LPArray)] uint[] columns);
        void GetSequencePoints(uint size, out uint length,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] offsets,
          [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, SizeParamIndex = 0)] IntPtr[] documents,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] lines,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] columns,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] endLines,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] endColumns);
    }
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("40DE4037-7C81-3E1E-B022-AE1ABFF2CA08")]
    interface ISymUnmanagedDocument
    {
        void GetURL(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] url);
        void GetDocumentType(out Guid retval);
        void GetLanguage(out Guid retval);
        void GetLanguageVendor(out Guid retval);
        void GetCheckSumAlgorithmId(out Guid retval);
        void GetCheckSum(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] data);
        uint FindClosestLine(uint line);
        bool HasEmbeddedSource();
        uint GetSourceLength();
        void GetSourceRange(uint startLine, uint startColumn, uint endLine, uint endColumn, uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] source);
    }
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9F60EEBE-2D9A-3F7C-BF58-80BC991C60BB")]
    interface ISymUnmanagedVariable
    {
        void GetName(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] name);
        uint GetAttributes();
        void GetSignature(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] name);
        uint GetAddressKind();
        uint GetAddressField1();
        uint GetAddressField2();
        uint GetAddressField3();
        uint GetStartOffset();
        uint GetEndOffset();
    }
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("68005D0F-B8E0-3B01-84D5-A11A94154942")]
    interface ISymUnmanagedScope
    {
        ISymUnmanagedMethod GetMethod();
        ISymUnmanagedScope GetParent();
        void GetChildren(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] children);
        uint GetStartOffset();
        uint GetEndOffset();
        uint GetLocalCount();
        void GetLocals(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] locals);
        void GetNamespaces(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] namespaces);
    }

    internal sealed class UnmanagedBuffer : IDisposable
    {
        internal IntPtr Pointer;
        internal UnmanagedBuffer(int length)
        {
            this.Pointer = Marshal.AllocHGlobal(length);
        }
        public void Dispose()
        {
            if (this.Pointer != IntPtr.Zero)
                Marshal.FreeHGlobal(this.Pointer);
            this.Pointer = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }
        ~UnmanagedBuffer()
        {
            this.Dispose();
        }
    }
    internal unsafe class Reader : IDisposable
    {
        private string directory;
        private string fileName;
        private bool doNotLockFile;
        private Module/*!*/ module = new Module();
        internal TypeNode currentType;
        private long sortedTablesMask;
        internal MetadataReader/*!*/ tables;
        private UnmanagedBuffer unmanagedBuffer;
        private int bufferLength;
        private IDictionary/*!*/ localAssemblyCache; //use for simple names
        internal readonly static IDictionary/*!*/ StaticAssemblyCache = new SynchronizedWeakDictionary(); //use for strong names
        private bool useStaticCache;
        //^ [Microsoft.Contracts.SpecInternal]
        private TrivialHashtable namespaceTable;
        internal NamespaceList namespaceList;
        internal ISymUnmanagedReader debugReader;

        internal bool getDebugSymbols;
        private bool getDebugSymbolsFailed;
        private TypeNodeList currentTypeParameters;
        private TypeNodeList currentMethodTypeParameters;
        internal bool preserveShortBranches;

        /// <summary>
        /// This read-only property indicates whether or not the PDB file exists
        /// </summary>
        internal bool PdbExists { get; set; }

        /// <summary>
        /// This read-only property indicates whether or not the PDB file is out of date
        /// </summary>
        internal bool PdbOutOfDate { get; set; }

        internal unsafe Reader(byte[]/*!*/ buffer, IDictionary localAssemblyCache, bool doNotLockFile, bool getDebugInfo, bool useStaticCache, bool preserveShortBranches)
        {
            Debug.Assert(buffer != null);
            if (localAssemblyCache == null) localAssemblyCache = new Hashtable();
            this.localAssemblyCache = localAssemblyCache;
            this.getDebugSymbols = getDebugInfo;
            this.doNotLockFile = false;
            this.useStaticCache = useStaticCache;
            this.preserveShortBranches = preserveShortBranches;
            int n = this.bufferLength = buffer.Length;
            this.unmanagedBuffer = new UnmanagedBuffer(n);
            //^ base();
            byte* pb = (byte*)this.unmanagedBuffer.Pointer;
            for (int i = 0; i < n; i++) *pb++ = buffer[i];
        }

        internal Reader(string/*!*/ fileName, IDictionary localAssemblyCache, bool doNotLockFile, bool getDebugInfo, bool useStaticCache, bool preserveShortBranches)
        {
            if (localAssemblyCache == null) localAssemblyCache = new Hashtable();
            this.localAssemblyCache = localAssemblyCache;
            fileName = System.IO.Path.GetFullPath(fileName);
            this.fileName = fileName;
            this.directory = System.IO.Path.GetDirectoryName(fileName);
            this.getDebugSymbols = getDebugInfo;
            this.doNotLockFile = doNotLockFile;
            this.useStaticCache = useStaticCache;
            this.preserveShortBranches = preserveShortBranches;
            //^ base();
        }
        internal Reader(IDictionary localAssemblyCache, bool doNotLockFile, bool getDebugInfo, bool useStaticCache, bool preserveShortBranches)
        {
            if (localAssemblyCache == null) localAssemblyCache = new Hashtable();
            this.localAssemblyCache = localAssemblyCache;
            this.directory = System.IO.Directory.GetCurrentDirectory();
            this.getDebugSymbols = getDebugInfo;
            this.doNotLockFile = doNotLockFile;
            this.useStaticCache = useStaticCache;
            this.preserveShortBranches = preserveShortBranches;
            //^ base();
        }
        public void Dispose()
        {
            if (this.unmanagedBuffer != null)
                this.unmanagedBuffer.Dispose();
            this.unmanagedBuffer = null;
            if (this.tables != null)
                this.tables.Dispose();
            //this.tables = null;

            if (this.debugReader != null)
                Marshal.ReleaseComObject(this.debugReader);
            this.debugReader = null;
        }
        private unsafe void SetupReader()
        {
            Debug.Assert(this.localAssemblyCache != null);

            if (this.doNotLockFile)
            {
                using (System.IO.FileStream inputStream = new System.IO.FileStream(this.fileName,
                         System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                {
                    this.ReadFileIntoUnmanagedBuffer(inputStream);
                }
            }
            if (this.unmanagedBuffer == null)
                this.tables = new MetadataReader(this.fileName); //Uses a memory map that locks the file
            else
                this.tables = new MetadataReader((byte*)this.unmanagedBuffer.Pointer, this.bufferLength);

            //^ assume this.tables.tablesHeader != null;
            this.sortedTablesMask = this.tables.tablesHeader.maskSorted;
        }

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern unsafe bool ReadFile(IntPtr FileHandle, byte* Buffer, int NumberOfBytesToRead, int* NumberOfBytesRead, IntPtr Overlapped);

        private unsafe void ReadFileIntoUnmanagedBuffer(System.IO.FileStream/*!*/ inputStream)
        {
            long size = inputStream.Seek(0, System.IO.SeekOrigin.End);
            if (size > int.MaxValue) throw new System.IO.FileLoadException();
            inputStream.Seek(0, System.IO.SeekOrigin.Begin);
            int n = (int)size;
            this.bufferLength = n;
            this.unmanagedBuffer = new UnmanagedBuffer(n);
            byte* pb = (byte*)this.unmanagedBuffer.Pointer;

            if(!Reader.ReadFile(inputStream.SafeFileHandle.DangerousGetHandle(), pb, n, &n, IntPtr.Zero))
                throw new System.IO.FileLoadException();
        }

        internal void SetupDebugReader(string filename, string pdbSearchPath)
        {
            if(filename == null)
                return;

            CorSymBinder binderObj1 = null;
            CorSymBinder2 binderObj2 = null;
            getDebugSymbolsFailed = false;
            object importer = null;

            try
            {
                int hresult = 0;

                try
                {
                    binderObj2 = new CorSymBinder2();
                    ISymUnmanagedBinder2 binder2 = (ISymUnmanagedBinder2)binderObj2;

                    importer = new EmptyImporter();
                    hresult = binder2.GetReaderForFile(importer, filename, pdbSearchPath, out this.debugReader);
                }
                catch(COMException e)
                {
                    // Could not instantiate ISymUnmanagedBinder2, fall back to ISymUnmanagedBinder
                    if((uint)e.ErrorCode == 0x80040111)
                    {
                        binderObj1 = new CorSymBinder();
                        ISymUnmanagedBinder binder = (ISymUnmanagedBinder)binderObj1;
                        hresult = binder.GetReaderForFile(importer, filename, null, out this.debugReader);
                    }
                    else
                        throw;
                }

                switch((uint)hresult)
                {
                    case 0x0:
                        this.PdbExists = true;
                        break;

                    case 0x806d0005:    // EC_NOT_FOUND
                    case 0x806d0014:    // EC_INVALID_EXE_TIMESTAMP
                        // Sometimes, GetReaderForFile erroneously reports missing PDB files as being out of date 
                        // so we check if the file actually exists before reporting the error.  The mere absence
                        // of a PDB file is not an error.
                        if(File.Exists(Path.ChangeExtension(filename, ".pdb")))
                            this.PdbOutOfDate = true;
                        break;

                    default:
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                          ExceptionStrings.GetReaderForFileReturnedUnexpectedHResult, hresult.ToString("X")));
                }
            }
            catch(Exception e)
            {
                this.getDebugSymbols = false;
                this.getDebugSymbolsFailed = true;

                if(this.module.MetadataImportErrors == null)
                    this.module.MetadataImportErrors = new ArrayList();

                this.module.MetadataImportErrors.Add(e);
            }
            finally
            {
                if(binderObj1 != null)
                    Marshal.ReleaseComObject(binderObj1);

                if(binderObj2 != null)
                    Marshal.ReleaseComObject(binderObj2);
            }
        }

        private AssemblyNode ReadAssembly()
        {
            try
            {
                AssemblyNode assembly = new AssemblyNode(new Module.TypeNodeProvider(this.GetTypeFromName),
                  new Module.TypeNodeListProvider(this.GetTypeList), new Module.CustomAttributeProvider(this.GetCustomAttributesFor),
                  new Module.ResourceProvider(this.GetResources), this.directory);
                assembly.reader = this;
                this.ReadModuleProperties(assembly);
                this.ReadAssemblyProperties(assembly); //Hashvalue, Name, etc.
                this.module = assembly;
                this.ReadAssemblyReferences(assembly);
                this.ReadModuleReferences(assembly);
                AssemblyNode cachedAssembly = this.GetCachedAssembly(assembly);
                if (cachedAssembly != null) return cachedAssembly;
                if (this.getDebugSymbols) assembly.SetupDebugReader(null);

                assembly.AfterAssemblyLoadProcessing();

                return assembly;
            }
            catch (Exception e)
            {
                if (this.module == null) return null;
                if (this.module.MetadataImportErrors == null) this.module.MetadataImportErrors = new ArrayList();
                this.module.MetadataImportErrors.Add(e);
                return this.module as AssemblyNode;
            }
        }

        private AssemblyNode GetCachedAssembly(AssemblyNode/*!*/ assembly)
        {
            //Always return the one true mscorlib. Things get too weird if more than one mscorlib is being read at the same time.
            //if (CoreSystemTypes.SystemAssembly != null && CoreSystemTypes.SystemAssembly.Name == assembly.Name && CoreSystemTypes.SystemAssembly.reader != null) {
            //  if (CoreSystemTypes.SystemAssembly.reader != this) {
            //    if (this.getDebugSymbols && !CoreSystemTypes.SystemAssembly.reader.getDebugSymbols && !CoreSystemTypes.SystemAssembly.reader.getDebugSymbolsFailed)
            //      CoreSystemTypes.SystemAssembly.SetupDebugReader(null);
            //    this.Dispose();
            //  }
            //  return CoreSystemTypes.SystemAssembly;
            //}
            if (assembly.PublicKeyOrToken == null || assembly.PublicKeyOrToken.Length == 0)
            {
                AssemblyNode cachedAssembly = null;
                if (assembly.Location != null)
                    cachedAssembly = this.localAssemblyCache[assembly.Location] as AssemblyNode;
                if (cachedAssembly == null && assembly.Name != null)
                {
                    cachedAssembly = this.localAssemblyCache[assembly.Name] as AssemblyNode;
                    if (cachedAssembly != null && assembly.Location != null)
                        this.localAssemblyCache[assembly.Location] = cachedAssembly;
                }
                if (cachedAssembly != null)
                {
                    if (cachedAssembly.reader != this && cachedAssembly.reader != null)
                    {
                        if (this.getDebugSymbols && !cachedAssembly.reader.getDebugSymbols && !cachedAssembly.reader.getDebugSymbolsFailed)
                            cachedAssembly.SetupDebugReader(null);
                        this.Dispose();
                    }
                    return cachedAssembly;
                }
                lock (Reader.StaticAssemblyCache)
                {
                    if (assembly.Name != null)
                        this.localAssemblyCache[assembly.Name] = assembly;
                    if (this.fileName != null)
                        this.localAssemblyCache[this.fileName] = assembly;
                }
            }
            else
            {
                string assemblyStrongName = assembly.StrongName;
                AssemblyNode cachedAssembly = null;
                if (this.useStaticCache)
                {
                    //See if assembly is a platform assembly (and apply unification)
                    AssemblyReference assemblyReference = new AssemblyReference(assembly);
                    AssemblyReference aRef = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[Identifier.For(assemblyReference.Name).UniqueIdKey];
                    if (aRef != null && assemblyReference.Version != null && aRef.Version >= assemblyReference.Version && aRef.MatchesIgnoringVersion(assemblyReference))
                    {
                        AssemblyNode platformAssembly = aRef.assembly;
                        if (platformAssembly == null)
                        {
                            Debug.Assert(aRef.Location != null);
                            if (Path.GetFullPath(aRef.Location) == assembly.Location)
                            {
                                if (aRef.Version != assemblyReference.Version)
                                {
                                    HandleError(assembly, String.Format(CultureInfo.CurrentCulture, ExceptionStrings.BadTargetPlatformLocation, assembly.Name, TargetPlatform.PlatformAssembliesLocation, assembly.Version, aRef.Version));
                                }
                                lock (Reader.StaticAssemblyCache)
                                {
                                    Reader.StaticAssemblyCache[assemblyStrongName] = assembly;
                                    if (aRef.Location != null)
                                        Reader.StaticAssemblyCache[aRef.Location] = assembly;
                                }
                                return null; //Prevents infinite recursion
                            }
                            platformAssembly = AssemblyNode.GetAssembly(aRef.Location, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache);
                        }
                        if (platformAssembly != null)
                        {
                            lock (Reader.StaticAssemblyCache)
                            {
                                if (aRef.Location != null)
                                    Reader.StaticAssemblyCache[aRef.Location] = platformAssembly;
                                Reader.StaticAssemblyCache[assemblyStrongName] = platformAssembly;
                            }
                            return aRef.assembly = platformAssembly;
                        }
                    }
                    cachedAssembly = Reader.StaticAssemblyCache[assemblyStrongName] as AssemblyNode;
                    if (cachedAssembly != null)
                    {
                        if (aRef == null && assembly.FileLastWriteTimeUtc > cachedAssembly.FileLastWriteTimeUtc &&
                          assembly.Location != null && cachedAssembly.Location != null && assembly.Location == cachedAssembly.Location)
                        {
                            lock (Reader.StaticAssemblyCache)
                            {
                                Reader.StaticAssemblyCache[assemblyStrongName] = assembly;
                            }
                            return null;
                        }
                        if (cachedAssembly.reader != this && cachedAssembly.reader != null)
                        {
                            if (this.getDebugSymbols && !cachedAssembly.reader.getDebugSymbols && !cachedAssembly.reader.getDebugSymbolsFailed)
                                cachedAssembly.SetupDebugReader(null);
                            this.Dispose();
                        }
                        return cachedAssembly;
                    }
                    lock (Reader.StaticAssemblyCache)
                    {
                        Reader.StaticAssemblyCache[assemblyStrongName] = assembly;
                        if (this.fileName != null)
                        {
                            Reader.StaticAssemblyCache[this.fileName] = assembly;
                        }
                    }
                }
                else
                {
                    cachedAssembly = this.localAssemblyCache[assemblyStrongName] as AssemblyNode;
                    if (cachedAssembly != null)
                    {
                        if (assembly.FileLastWriteTimeUtc > cachedAssembly.FileLastWriteTimeUtc &&
                          assembly.Location != null && cachedAssembly.Location != null && assembly.Location == cachedAssembly.Location)
                        {
                            this.localAssemblyCache[assemblyStrongName] = assembly;
                            return null;
                        }
                        if (cachedAssembly.reader != this && cachedAssembly.reader != null)
                        {
                            if (this.getDebugSymbols && cachedAssembly.reader.debugReader == null && !cachedAssembly.reader.getDebugSymbolsFailed)
                                cachedAssembly.SetupDebugReader(null);

                            this.Dispose();
                        }
                        return cachedAssembly;
                    }
                    this.localAssemblyCache[assemblyStrongName] = assembly;
                    if (this.fileName != null) this.localAssemblyCache[this.fileName] = assembly;
                }
            }
            return null;
        }
        internal Module ReadModule()
        {
            try
            {
                if (this.fileName != null)
                {
                    if (!System.IO.File.Exists(this.fileName)) return null;
                    AssemblyNode cachedAssembly;
                    if (this.useStaticCache)
                    {
                        cachedAssembly = Reader.StaticAssemblyCache[this.fileName] as AssemblyNode;
                        if (cachedAssembly != null && cachedAssembly.FileLastWriteTimeUtc == System.IO.File.GetLastWriteTimeUtc(this.fileName))
                        {
                            this.Dispose();
                            return cachedAssembly;
                        }
                    }
                    cachedAssembly = this.localAssemblyCache[this.fileName] as AssemblyNode;
                    if (cachedAssembly != null && cachedAssembly.FileLastWriteTimeUtc == System.IO.File.GetLastWriteTimeUtc(this.fileName))
                    {
                        this.Dispose();
                        return cachedAssembly;
                    }
                }
                this.SetupReader();
                if (this.tables.AssemblyTable.Length > 0) return this.ReadAssembly();
                Module module = this.module = new Module(new Module.TypeNodeProvider(this.GetTypeFromName),
                  new Module.TypeNodeListProvider(this.GetTypeList), new Module.CustomAttributeProvider(this.GetCustomAttributesFor),
                  new Module.ResourceProvider(this.GetResources));
                module.reader = this;
                this.ReadModuleProperties(module);
                this.module = module;
                this.ReadAssemblyReferences(module);
                this.ReadModuleReferences(module);
                if (this.getDebugSymbols) this.SetupDebugReader(this.fileName, null);
                return module;
            }
            catch (Exception e)
            {
                if (this.module == null) return null;
                if (this.module.MetadataImportErrors == null) this.module.MetadataImportErrors = new ArrayList();
                this.module.MetadataImportErrors.Add(e);
                return module;
            }
        }

        private void ReadModuleProperties(Module/*!*/ module)
        {
            ModuleRow[] mods = this.tables.ModuleTable;
            if (mods.Length != 1) throw new InvalidMetadataException(ExceptionStrings.InvalidModuleTable);
            ModuleRow mrow = mods[0];
            module.reader = this;
            module.FileAlignment = this.tables.fileAlignment;
            module.HashValue = this.tables.HashValue;
            module.Kind = this.tables.moduleKind;
            module.Location = this.fileName;
            module.TargetRuntimeVersion = this.tables.targetRuntimeVersion;
            module.LinkerMajorVersion = this.tables.linkerMajorVersion;
            module.LinkerMinorVersion = this.tables.linkerMinorVersion;
            module.MetadataFormatMajorVersion = this.tables.metadataFormatMajorVersion;
            module.MetadataFormatMinorVersion = this.tables.metadataFormatMinorVersion;
            module.Name = this.tables.GetString(mrow.Name);
            module.Mvid = this.tables.GetGuid(mrow.Mvid);
            module.PEKind = this.tables.peKind;
            module.TrackDebugData = this.tables.TrackDebugData;
        }
        private void ReadAssemblyProperties(AssemblyNode/*!*/ assembly)
        {
            AssemblyRow assemblyRow = this.tables.AssemblyTable[0];
            assembly.HashAlgorithm = (AssemblyHashAlgorithm)assemblyRow.HashAlgId;
            assembly.Version = new System.Version(assemblyRow.MajorVersion, assemblyRow.MinorVersion, assemblyRow.BuildNumber, assemblyRow.RevisionNumber);
            assembly.Flags = (AssemblyFlags)assemblyRow.Flags;
            assembly.PublicKeyOrToken = this.tables.GetBlob(assemblyRow.PublicKey);
            assembly.ModuleName = assembly.Name;
            assembly.Name = this.tables.GetString(assemblyRow.Name);
            assembly.Culture = this.tables.GetString(assemblyRow.Culture);
            if (this.fileName != null)
            {
                assembly.FileLastWriteTimeUtc = System.IO.File.GetLastWriteTimeUtc(this.fileName);
            }
            assembly.ContainingAssembly = assembly;
        }

        private void ReadAssemblyReferences(Module/*!*/ module)
        {
            AssemblyRefRow[] assems = this.tables.AssemblyRefTable;
            int n = assems.Length;

            AssemblyReferenceList assemblies = module.AssemblyReferences = new AssemblyReferenceList();

            for(int i = 0; i < n; i++)
            {
                AssemblyRefRow arr = assems[i];
                AssemblyReference assemRef = new AssemblyReference();
                assemRef.Version = new System.Version(arr.MajorVersion, arr.MinorVersion, arr.BuildNumber, arr.RevisionNumber);
                assemRef.Flags = (AssemblyFlags)arr.Flags;
                assemRef.PublicKeyOrToken = this.tables.GetBlob(arr.PublicKeyOrToken);
                assemRef.Name = this.tables.GetString(arr.Name);

                //if (CoreSystemTypes.SystemAssembly != null && CoreSystemTypes.SystemAssembly.Name == assemRef.Name && 
                //  assemRef.Version > CoreSystemTypes.SystemAssembly.Version){
                //  HandleError(module, ExceptionStrings.ModuleOrAssemblyDependsOnMoreRecentVersionOfCoreLibrary);
                //}

                assemRef.Culture = this.tables.GetString(arr.Culture);

                if(assemRef.Culture != null && assemRef.Culture.Length == 0)
                    assemRef.Culture = null;

                assemRef.HashValue = this.tables.GetBlob(arr.HashValue);
                assemRef.Reader = this;
                assems[i].AssemblyReference = assemRef;

                assemblies.Add(assemRef);
            }
        }

        private void ReadModuleReferences(Module/*!*/ module)
        {
            FileRow[] files = this.tables.FileTable;
            ModuleRefRow[] modRefs = this.tables.ModuleRefTable;
            int n = modRefs.Length;
            ModuleReferenceList modules = module.ModuleReferences = new ModuleReferenceList();

            for(int i = 0; i < n; i++)
            {
                Module mod;
                int nameIndex = modRefs[i].Name;
                string name = this.tables.GetString(nameIndex);
                string dir = GetDirectoryName(this.module.Location);
                string location = Combine(dir, name);

                for (int j = 0, m = files == null ? 0 : files.Length; j < m; j++)
                {
                    if(files[j].Name != nameIndex)
                        continue;

                    if ((files[j].Flags & (int)FileFlags.ContainsNoMetaData) == 0)
                        mod = Module.GetModule(location, this.doNotLockFile, this.getDebugSymbols, false);
                    else
                        mod = null;

                    if(mod == null)
                    {
                        mod = new Module();
                        mod.Name = name;
                        mod.Location = location;
                        mod.Kind = ModuleKind.UnmanagedDynamicallyLinkedLibrary;
                    }

                    mod.HashValue = this.tables.GetBlob(files[j].HashValue);
                    mod.ContainingAssembly = module.ContainingAssembly;
                    modRefs[i].Module = mod;
                    modules.Add(new ModuleReference(name, mod));
                    goto nextModRef;
                }

                mod = new Module();
                mod.Name = name;
                mod.Kind = ModuleKind.UnmanagedDynamicallyLinkedLibrary;
                if (System.IO.File.Exists(location)) mod.Location = location;
                mod.ContainingAssembly = module.ContainingAssembly;
                modRefs[i].Module = mod;
                modules.Add(new ModuleReference(name, mod));
nextModRef:     ;
            }
        }

        private static string ReadSerString(MemoryCursor/*!*/ sigReader)
        {
            int n = sigReader.ReadCompressedInt();
            if (n < 0) return null;
            return sigReader.ReadUTF8(n);
        }
        private void AddFieldsToType(TypeNode/*!*/ type, FieldRow[]/*!*/ fieldDefs, FieldPtrRow[]/*!*/ fieldPtrs, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                int ii = i;
                if (fieldPtrs.Length > 0) ii = fieldPtrs[i - 1].Field;
                Field field = this.GetFieldFromDef(ii, type);
                if (field != null) type.Members.Add(field);
            }
        }
        private void GetUnderlyingTypeOfEnumNode(EnumNode /*!*/enumNode, FieldRow[]/*!*/ fieldDefs, FieldPtrRow[]/*!*/ fieldPtrs, int start, int end)
        {
            TypeNode underlyingType = null;
            for (int i = start; i < end; i++)
            {
                int ii = i;
                if (fieldPtrs.Length > 0) ii = fieldPtrs[i - 1].Field;
                FieldRow fld = fieldDefs[ii - 1];
                if (fld.Field != null && !fld.Field.IsStatic)
                {
                    underlyingType = fld.Field.Type;
                    break;
                }
                FieldFlags fieldFlags = (FieldFlags)fld.Flags;
                if ((fieldFlags & FieldFlags.Static) == 0)
                {
                    this.tables.GetSignatureLength(fld.Signature);
                    MemoryCursor sigReader = this.tables.GetNewCursor();
                    GetAndCheckSignatureToken(6, sigReader);
                    underlyingType = this.ParseTypeSignature(sigReader);
                    break;
                }
            }
            enumNode.underlyingType = underlyingType;
        }
        private void AddMethodsToType(TypeNode/*!*/ type, MethodPtrRow[]/*!*/ methodPtrs, int start, int end)
        //^ requires type.members != null;
        {
            for (int i = start; i < end; i++)
            {
                int ii = i;
                if (methodPtrs.Length > 0) ii = methodPtrs[i - 1].Method;
                Method method = this.GetMethodFromDef(ii, type);
                if (method != null && ((method.Flags & MethodFlags.RTSpecialName) == 0 || method.Name.UniqueIdKey != StandardIds._Deleted.UniqueIdKey))
                    type.members.Add(method);
            }
        }
        private void AddMoreStuffToParameters(Method method, ParameterList parameters, int start, int end)
        {
            ParamRow[] pars = this.tables.ParamTable;
            int n = parameters == null ? 0 : parameters.Count;
            for (int i = start; i < end; i++)
            {
                ParamRow pr = pars[i - 1];
                if (pr.Sequence == 0 && method != null)
                {
                    //The parameter entry with sequence 0 is used as a target for custom attributes that apply to the return value
                    method.ReturnAttributes = this.GetCustomAttributesFor((i << 5) | 4);
                    if ((pr.Flags & (int)ParameterFlags.HasFieldMarshal) != 0)
                        method.ReturnTypeMarshallingInformation = this.GetMarshallingInformation((i << 1) | 1);
                    this.AddMoreStuffToParameters(null, parameters, start + 1, end);
                    return;
                }
                int j = pr.Sequence;
                if (j < 1 || j > n) continue; //Bad metadata, ignore
                if (parameters == null) continue;
                Parameter par = parameters[j - 1];
                par.Attributes = this.GetCustomAttributesFor((i << 5) | 4);
                par.Flags = (ParameterFlags)pr.Flags;
                if ((par.Flags & ParameterFlags.HasDefault) != 0)
                    par.DefaultValue = this.GetLiteral((i << 2) | 1, par.Type);
                if ((par.Flags & ParameterFlags.HasFieldMarshal) != 0)
                    par.MarshallingInformation = this.GetMarshallingInformation((i << 1) | 1);
                par.Name = tables.GetIdentifier(pr.Name);
            }
        }

        private void AddPropertiesToType(TypeNode/*!*/ type, PropertyRow[]/*!*/ propertyDefs, PropertyPtrRow[]/*!*/ propertyPtrs, int start, int end)
        //requires type.members != null;
        {
            MetadataReader tables = this.tables;
            for (int i = start; i < end; i++)
            {
                int ii = i;
                if (propertyPtrs.Length > 0) ii = propertyPtrs[i - 1].Property;
                PropertyRow prop = propertyDefs[ii - 1];
                Property property = new Property();
                property.Attributes = this.GetCustomAttributesFor((ii << 5) | 9);
                property.DeclaringType = type;
                property.Flags = (PropertyFlags)prop.Flags;
                property.Name = tables.GetIdentifier(prop.Name);
                if ((property.Flags & PropertyFlags.RTSpecialName) == 0 || property.Name.UniqueIdKey != StandardIds._Deleted.UniqueIdKey)
                {
                    this.AddMethodsToProperty(ii, property);
                    type.members.Add(property);
                }
                //REVIEW: the signature seems to be redundant. Is there any point in retrieving it?
            }
        }
        private void AddMethodsToProperty(int propIndex, Property/*!*/ property)
        {
            int codedPropIndex = (propIndex << 1) | 1;
            MetadataReader tables = this.tables;
            MethodRow[] methods = tables.MethodTable;
            MethodSemanticsRow[] methodSemantics = tables.MethodSemanticsTable;
            int i = 0, n = methodSemantics.Length, j = n - 1;
            bool sorted = (this.sortedTablesMask >> (int)TableIndices.MethodSemantics) % 2 == 1;
            if (sorted)
            {
                while (i < j)
                {
                    int k = (i + j) / 2;
                    if (methodSemantics[k].Association < codedPropIndex)
                        i = k + 1;
                    else
                        j = k;
                }
                while (i > 0 && methodSemantics[i - 1].Association == codedPropIndex) i--;
            }
            for (; i < n; i++)
            {
                MethodSemanticsRow meth = methodSemantics[i];
                Method propertyMethod = methods[meth.Method - 1].Method;
                if (propertyMethod == null) continue;
                if (meth.Association == codedPropIndex)
                {
                    propertyMethod.DeclaringMember = property;
                    switch (meth.Semantics)
                    {
                        case 0x0001: property.Setter = propertyMethod; break;
                        case 0x0002: property.Getter = propertyMethod; break;
                        default:
                            if (property.OtherMethods == null) property.OtherMethods = new MethodList();
                            property.OtherMethods.Add(propertyMethod); break;
                    }
                }
                else if (sorted)
                    break;
            }
        }
        private void AddEventsToType(TypeNode/*!*/ type, EventRow[]/*!*/ eventDefs, EventPtrRow[]/*!*/ eventPtrs, int start, int end)
        {
            MetadataReader tables = this.tables;
            for (int i = start; i < end; i++)
            {
                int ii = i;
                if (eventPtrs.Length > 0) ii = eventPtrs[i].Event;
                EventRow ev = eventDefs[ii - 1];
                Event evnt = new Event();
                evnt.Attributes = this.GetCustomAttributesFor((ii << 5) | 10);
                evnt.DeclaringType = type;
                evnt.Flags = (EventFlags)ev.Flags;
                evnt.HandlerType = this.DecodeAndGetTypeDefOrRefOrSpec(ev.EventType);
                evnt.Name = tables.GetIdentifier(ev.Name);
                if ((evnt.Flags & EventFlags.RTSpecialName) == 0 || evnt.Name.UniqueIdKey != StandardIds._Deleted.UniqueIdKey)
                {
                    this.AddMethodsToEvent(ii, evnt);
                    type.Members.Add(evnt);
                }
            }
        }
        private void AddMethodsToEvent(int eventIndex, Event/*!*/ evnt)
        {
            int codedEventIndex = eventIndex << 1;
            MetadataReader tables = this.tables;
            MethodRow[] methods = tables.MethodTable;
            MethodSemanticsRow[] methodSemantics = tables.MethodSemanticsTable;
            int i = 0, n = methodSemantics.Length, j = n - 1;
            bool sorted = (this.sortedTablesMask >> (int)TableIndices.MethodSemantics) % 2 == 1;
            if (sorted)
            {
                while (i < j)
                {
                    int k = (i + j) / 2;
                    if (methodSemantics[k].Association < codedEventIndex)
                        i = k + 1;
                    else
                        j = k;
                }
                while (i > 0 && methodSemantics[i - 1].Association == codedEventIndex) i--;
            }
            MethodFlags handlerFlags = (MethodFlags)0;
            for (; i < n; i++)
            {
                MethodSemanticsRow meth = methodSemantics[i];
                Method eventMethod = methods[meth.Method - 1].Method;
                if (eventMethod == null) continue;
                if (meth.Association == codedEventIndex)
                {
                    eventMethod.DeclaringMember = evnt;
                    switch (meth.Semantics)
                    {
                        case 0x0008: evnt.HandlerAdder = eventMethod; handlerFlags = eventMethod.Flags; break;
                        case 0x0010: evnt.HandlerRemover = eventMethod; handlerFlags = eventMethod.Flags; break;
                        case 0x0020: evnt.HandlerCaller = eventMethod; break;
                        default:
                            if (evnt.OtherMethods == null) evnt.OtherMethods = new MethodList();
                            evnt.OtherMethods.Add(eventMethod); break;
                    }
                }
                else if (sorted)
                    break;
            }
            evnt.HandlerFlags = handlerFlags;
        }
        private bool TypeDefOrRefOrSpecIsClass(int codedIndex)
        {
            if (codedIndex == 0) return false;
            switch (codedIndex & 0x3)
            {
                case 0x00: return this.TypeDefIsClass(codedIndex >> 2);
                case 0x01: TypeNode t = this.GetTypeFromRef(codedIndex >> 2); return t is Class;
                case 0x02: return this.TypeSpecIsClass(codedIndex >> 2);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadTypeDefOrRef);
        }
        private bool TypeDefOrRefOrSpecIsClassButNotValueTypeBaseClass(int codedIndex)
        {
            if (codedIndex == 0) return false;
            switch (codedIndex & 0x3)
            {
                case 0x00: return this.TypeDefIsClassButNotValueTypeBaseClass(codedIndex >> 2);
                case 0x01:
                    TypeNode t = this.GetTypeFromRef(codedIndex >> 2);
                    return t != CoreSystemTypes.ValueType && t != CoreSystemTypes.Enum && t is Class;
                case 0x02: return this.TypeSpecIsClass(codedIndex >> 2);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadTypeDefOrRef);
        }
        private TypeNode DecodeAndGetTypeDefOrRefOrSpec(int codedIndex)
        {
            if (codedIndex == 0) return null;
            switch (codedIndex & 0x3)
            {
                case 0x00: return this.GetTypeFromDef(codedIndex >> 2);
                case 0x01: return this.GetTypeFromRef(codedIndex >> 2);
                case 0x02: return this.GetTypeFromSpec(codedIndex >> 2);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadTypeDefOrRef);
        }
        private TypeNode DecodeAndGetTypeDefOrRefOrSpec(int codedIndex, bool expectStruct)
        {
            if (codedIndex == 0) return null;
            switch (codedIndex & 0x3)
            {
                case 0x00: return this.GetTypeFromDef(codedIndex >> 2);
                case 0x01: return this.GetTypeFromRef(codedIndex >> 2, expectStruct);
                case 0x02: return this.GetTypeFromSpec(codedIndex >> 2);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadTypeDefOrRef);
        }

        private TypeNode GetTypeIfNotGenericInstance(int codedIndex)
        {
            if (codedIndex == 0) return null;
            switch (codedIndex & 0x3)
            {
                case 0x00: return this.GetTypeFromDef(codedIndex >> 2);
                case 0x01: return this.GetTypeFromRef(codedIndex >> 2, false);
            }
            return null;
        }
        internal AssemblyNode/*!*/ GetAssemblyFromReference(AssemblyReference/*!*/ assemblyReference)
        {
            lock(this)
            {
                if(CoreSystemTypes.SystemAssembly != null && CoreSystemTypes.SystemAssembly.Name == assemblyReference.Name)
                    return CoreSystemTypes.SystemAssembly;

                string strongName = null;
                object cachedValue = null;

                if (assemblyReference.PublicKeyOrToken == null || assemblyReference.PublicKeyOrToken.Length == 0)
                {
                    if (assemblyReference.Location != null)
                        cachedValue = this.localAssemblyCache[assemblyReference.Location];

                    if (cachedValue == null)
                    {
                        cachedValue = this.localAssemblyCache[assemblyReference.Name];
                        if (cachedValue != null && assemblyReference.Location != null)
                            this.localAssemblyCache[assemblyReference.Location] = cachedValue;
                    }
                }
                else
                {
                    strongName = assemblyReference.StrongName;
                    if (this.useStaticCache)
                    {
                        //See if reference is to an assembly that lives in the GAC.
                        if (assemblyReference.Location != null)
                            cachedValue = Reader.StaticAssemblyCache[assemblyReference.Location];
                        if (cachedValue == null)
                            cachedValue = Reader.StaticAssemblyCache[strongName];
                    }
                    if (cachedValue == null)
                        cachedValue = this.localAssemblyCache[strongName];
                }
                if (cachedValue == null)
                {
                    //See if assembly is a platform assembly (and apply unification)
                    AssemblyReference aRef = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[Identifier.For(assemblyReference.Name).UniqueIdKey];
                    if (aRef != null && assemblyReference.Version != null && aRef.Version >= assemblyReference.Version && aRef.MatchesIgnoringVersion(assemblyReference))
                    {
                        AssemblyNode platformAssembly = aRef.assembly;
                        if (platformAssembly == null)
                        {
                            Debug.Assert(aRef.Location != null);
                            platformAssembly = AssemblyNode.GetAssembly(aRef.Location, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache);
                        }
                        if (platformAssembly != null)
                        {
                            if (strongName == null) strongName = assemblyReference.Name;
                            lock (Reader.StaticAssemblyCache)
                            {
                                if (aRef.Location != null)
                                    Reader.StaticAssemblyCache[aRef.Location] = platformAssembly;
                                Reader.StaticAssemblyCache[strongName] = platformAssembly;
                            }
                            return aRef.assembly = platformAssembly;
                        }
                    }
                }
                AssemblyNode assembly = cachedValue as AssemblyNode;
                if (assembly != null) goto done;

                //No cached assembly and no cached reader for this assembly. Look for a resolver.
                if (this.module != null)
                {
                    assembly = this.module.Resolve(assemblyReference);
                    if (assembly != null)
                    {
                        if (strongName == null)
                        {
                            this.localAssemblyCache[assembly.Name] = assembly;
                            if (assembly.Location != null) this.localAssemblyCache[assembly.Location] = assembly;
                        }
                        else
                        {
                            if (CoreSystemTypes.SystemAssembly != null && CoreSystemTypes.SystemAssembly.Name == assembly.Name) return CoreSystemTypes.SystemAssembly;
                            lock (Reader.StaticAssemblyCache)
                            {
                                if (this.useStaticCache)
                                {
                                    if (assembly.Location != null)
                                        Reader.StaticAssemblyCache[assembly.Location] = assembly;
                                    Reader.StaticAssemblyCache[strongName] = assembly;
                                }
                                else
                                {
                                    this.localAssemblyCache[strongName] = assembly;
                                    if (assembly.Location != null) this.localAssemblyCache[assembly.Location] = assembly;
                                }
                            }
                        }
                        goto done;
                    }
                }

                //Look for an assembly with the given name in the same directory as the referencing module
                if (this.directory != null)
                {
                    string fileName = System.IO.Path.Combine(this.directory, assemblyReference.Name + ".dll");
                    if (System.IO.File.Exists(fileName))
                    {
                        assembly = AssemblyNode.GetAssembly(fileName, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache);
                        if (assembly != null)
                        {
                            if (strongName == null) goto cacheIt; //found something
                            //return assembly only if it matches the strong name of the reference
                            if (assemblyReference.Matches(assembly.Name, assembly.Version, assembly.Culture, assembly.PublicKeyToken)) goto cacheIt;
                        }
                    }
                    fileName = System.IO.Path.Combine(this.directory, assemblyReference.Name + ".exe");
                    if (System.IO.File.Exists(fileName))
                    {
                        assembly = AssemblyNode.GetAssembly(fileName, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache);
                        if (assembly != null)
                        {
                            if (strongName == null) goto cacheIt; //found something
                            //return assembly only if it matches the strong name of the reference
                            if (assemblyReference.Matches(assembly.Name, assembly.Version, assembly.Culture, assembly.PublicKeyToken)) goto cacheIt;
                        }
                    }
                    fileName = System.IO.Path.Combine(this.directory, assemblyReference.Name + ".ill");
                    if (System.IO.File.Exists(fileName))
                    {
                        assembly = AssemblyNode.GetAssembly(fileName, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache);
                        if (assembly != null)
                        {
                            if (strongName == null) goto cacheIt; //found something
                            //return assembly only if it matches the strong name of the reference
                            if (assemblyReference.Matches(assembly.Name, assembly.Version, assembly.Culture, assembly.PublicKeyToken)) goto cacheIt;
                        }
                    }
                }
                //Look for an assembly in the same directory as the application using Reader.
                {
                    string fileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyReference.Name + ".dll");
                    if (System.IO.File.Exists(fileName))
                    {
                        assembly = AssemblyNode.GetAssembly(fileName, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache);
                        if (assembly != null)
                        {
                            if (strongName == null) goto cacheIt; //found something
                            //return assembly only if it matches the strong name of the reference
                            if (assemblyReference.Matches(assembly.Name, assembly.Version, assembly.Culture, assembly.PublicKeyToken)) goto cacheIt;
                        }
                    }
                    fileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyReference.Name + ".exe");
                    if (System.IO.File.Exists(fileName))
                    {
                        assembly = AssemblyNode.GetAssembly(fileName, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache);
                        if (assembly != null)
                        {
                            if (strongName == null) goto cacheIt; //found something
                            //return assembly only if it matches the strong name of the reference
                            if (assemblyReference.Matches(assembly.Name, assembly.Version, assembly.Culture, assembly.PublicKeyToken)) goto cacheIt;
                        }
                    }
                }
                assembly = null;

                //Probe the GAC
                string gacLocation = null;
                if (strongName != null)
                {
                    //Look for the assembly in the system's Global Assembly Cache
                    gacLocation = GlobalAssemblyCache.GetLocation(assemblyReference);
                    if (gacLocation != null && gacLocation.Length == 0) gacLocation = null;

                    if (gacLocation != null)
                    {
                        assembly = AssemblyNode.GetAssembly(gacLocation, this.useStaticCache ? Reader.StaticAssemblyCache : this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache);
                        if (assembly != null)
                        {
                            lock (Reader.StaticAssemblyCache)
                            {
                                if (this.useStaticCache)
                                {
                                    Reader.StaticAssemblyCache[gacLocation] = assembly;
                                    Reader.StaticAssemblyCache[strongName] = assembly;
                                }
                                else
                                {
                                    this.localAssemblyCache[gacLocation] = assembly;
                                    this.localAssemblyCache[strongName] = assembly;
                                }
                            }
                        }
                    }
                }
                goto done;
            cacheIt:
                if (strongName == null)
                {
                    this.localAssemblyCache[assembly.Name] = assembly;
                    if (assembly.Location != null) this.localAssemblyCache[assembly.Location] = assembly;
                }
                else
                {
                    this.localAssemblyCache[strongName] = assembly;
                    if (assembly.Location != null) this.localAssemblyCache[assembly.Location] = assembly;
                }
            done:
                if (assembly != null)
                    assembly.InitializeAssemblyReferenceResolution(this.module);
                if (assembly == null)
                {
                    if (this.module != null)
                    {
                        assembly = this.module.ResolveAfterProbingFailed(assemblyReference);
                        if (assembly != null) goto cacheIt;
                        HandleError(this.module, String.Format(CultureInfo.CurrentCulture, ExceptionStrings.AssemblyReferenceNotResolved, assemblyReference.StrongName));
                    }
                    assembly = new AssemblyNode();
                    assembly.Culture = assemblyReference.Culture;
                    assembly.Name = assemblyReference.Name;
                    assembly.PublicKeyOrToken = assemblyReference.PublicKeyOrToken;
                    assembly.Version = assemblyReference.Version;
                    assembly.Location = "unknown:location";
                    goto cacheIt;
                }
                return assembly;
            }
        }
        private static void GetAndCheckSignatureToken(int expectedToken, MemoryCursor/*!*/ sigReader)
        {
            int tok = sigReader.ReadCompressedInt();
            if (tok != expectedToken) throw new InvalidMetadataException(ExceptionStrings.MalformedSignature);
        }
        private Method GetConstructorDefOrRef(int codedIndex, out TypeNodeList varArgTypes)
        {
            varArgTypes = null;
            switch (codedIndex & 0x7)
            {
                case 0x02: return this.GetMethodFromDef(codedIndex >> 3);
                case 0x03: return (Method)this.GetMemberFromRef(codedIndex >> 3, out varArgTypes);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadCustomAttributeTypeEncodedToken);
        }

        private void GetResources(Module/*!*/ module)
        {
            ManifestResourceRow[] manifestResourceTable = this.tables.ManifestResourceTable;
            int n = manifestResourceTable.Length;
            ResourceList resources = new ResourceList();

            for (int i = 0; i < n; i++)
            {
                ManifestResourceRow mrr = manifestResourceTable[i];
                Resource r = new Resource();
                r.Name = this.tables.GetString(mrr.Name);
                r.IsPublic = (mrr.Flags & 7) == 1;
                int impl = mrr.Implementation;
                if (impl != 0)
                {
                    switch (impl & 0x3)
                    {
                        case 0x0:
                            string modName = this.tables.GetString(this.tables.FileTable[(impl >> 2) - 1].Name);
                            if ((this.tables.FileTable[(impl >> 2) - 1].Flags & (int)FileFlags.ContainsNoMetaData) != 0)
                            {
                                r.DefiningModule = new Module();
                                r.DefiningModule.Directory = module.Directory;
                                r.DefiningModule.Location = Path.Combine(module.Directory, modName);
                                r.DefiningModule.Name = modName;
                                r.DefiningModule.Kind = ModuleKind.ManifestResourceFile;
                                r.DefiningModule.ContainingAssembly = module.ContainingAssembly;
                                r.DefiningModule.HashValue = this.tables.GetBlob(this.tables.FileTable[(impl >> 2) - 1].HashValue);
                            }
                            else
                            {
                                string modLocation = modName;
                                r.DefiningModule = GetNestedModule(module, modName, ref modLocation);
                            }
                            break;
                        case 0x1:
                            r.DefiningModule = this.tables.AssemblyRefTable[(impl >> 2) - 1].AssemblyReference.Assembly;
                            break;
                    }
                }
                else
                {
                    r.DefiningModule = module;
                    r.Data = this.tables.GetResourceData(mrr.Offset);
                }

                resources.Add(r);
            }
            module.Resources = resources;
            module.Win32Resources = this.tables.ReadWin32Resources();
        }

        private SecurityAttribute GetSecurityAttribute(int i)
        {
            DeclSecurityRow dsr = this.tables.DeclSecurityTable[i];
            SecurityAttribute attr = new SecurityAttribute();
            attr.Action = (System.Security.Permissions.SecurityAction)dsr.Action;
            if (this.module.MetadataFormatMajorVersion > 1 || this.module.MetadataFormatMinorVersion > 0)
            {
                attr.PermissionAttributes = this.GetPermissionAttributes(dsr.PermissionSet, attr.Action);
                if (attr.PermissionAttributes != null) return attr;
            }
            attr.SerializedPermissions = (string)this.tables.GetBlobString(dsr.PermissionSet);
            return attr;
        }
        private AttributeList GetPermissionAttributes(int blobIndex, System.Security.Permissions.SecurityAction action)
        {
            AttributeList result = new AttributeList();
            int blobLength;
            MemoryCursor sigReader = this.tables.GetBlobCursor(blobIndex, out blobLength);
            if (blobLength == 0) return null;
            byte header = sigReader.ReadByte();
            if (header != (byte)'*')
            {
                if (header == (byte)'<') return null;
                if (header == (byte)'.') return this.GetPermissionAttributes2(blobIndex, action);
                HandleError(this.module, ExceptionStrings.BadSecurityPermissionSetBlob);
                return null;
            }
            sigReader.ReadInt32(); //Skip over the token for the attribute target
            sigReader.ReadInt32(); //Skip over the security action
            int numAttrs = sigReader.ReadInt32();
            for (int i = 0; i < numAttrs; i++)
                result.Add(this.GetPermissionAttribute(sigReader));
            return result;
        }
        private AttributeNode GetPermissionAttribute(MemoryCursor/*!*/ sigReader)
        {
            sigReader.ReadInt32(); //Skip over index
            int typeNameLength = sigReader.ReadInt32();
            sigReader.ReadUTF8(typeNameLength); //Skip over type name
            int constructorToken = sigReader.ReadInt32();
            sigReader.ReadInt32(); //Skip over attribute type token
            sigReader.ReadInt32(); //Skip over assembly ref token
            int caBlobLength = sigReader.ReadInt32();
            sigReader.ReadInt32(); //Skip over the number of parameters in the CA blob
            TypeNodeList varArgTypes; //Ignored because vararg constructors are not allowed in Custom Attributes
            Method cons = this.GetConstructorDefOrRef(constructorToken, out varArgTypes);
            if (cons == null) cons = new Method();
            return this.GetCustomAttribute(cons, sigReader, caBlobLength);
        }
        private AttributeList GetPermissionAttributes2(int blobIndex, System.Security.Permissions.SecurityAction action)
        {
            AttributeList result = new AttributeList();
            int blobLength;
            MemoryCursor sigReader = this.tables.GetBlobCursor(blobIndex, out blobLength);
            if (blobLength == 0) return null;
            byte header = sigReader.ReadByte();
            if (header != (byte)'.')
            {
                HandleError(this.module, ExceptionStrings.BadSecurityPermissionSetBlob);
                return null;
            }
            int numAttrs = sigReader.ReadCompressedInt();
            for (int i = 0; i < numAttrs; i++)
                result.Add(this.GetPermissionAttribute2(sigReader, action));
            return result;
        }
        private AttributeNode GetPermissionAttribute2(MemoryCursor/*!*/ sigReader, System.Security.Permissions.SecurityAction action)
        {
            int typeNameLength = sigReader.ReadCompressedInt();
            string serializedTypeName = sigReader.ReadUTF8(typeNameLength);
            TypeNode attrType = null;
            try
            {
                attrType = this.GetTypeFromSerializedName(serializedTypeName);
            }
            catch (InvalidMetadataException) { }
            if (attrType == null)
            {
                HandleError(this.module, String.Format(CultureInfo.CurrentCulture, ExceptionStrings.CouldNotResolveType, serializedTypeName));
                return null;
            }
            InstanceInitializer cons = attrType.GetConstructor(CoreSystemTypes.SecurityAction);
            if (cons == null)
            {
                HandleError(this.module, String.Format(CultureInfo.CurrentCulture,
                ExceptionStrings.SecurityAttributeTypeDoesNotHaveADefaultConstructor, serializedTypeName));
                return null;
            }

            sigReader.ReadCompressedInt(); //caBlobLength

            int numProps = sigReader.ReadCompressedInt(); //Skip over the number of properties in the CA blob

            ExpressionList arguments = new ExpressionList();

            arguments.Add(new Literal(action, CoreSystemTypes.SecurityAction));

            this.GetCustomAttributeNamedArguments(arguments, (ushort)numProps, sigReader);

            return new AttributeNode(new MemberBinding(null, cons), arguments);
        }
        private static void HandleError(Module mod, string errorMessage)
        {
            if (mod != null)
            {
                if (mod.MetadataImportErrors == null) mod.MetadataImportErrors = new ArrayList();
                mod.MetadataImportErrors.Add(new InvalidMetadataException(errorMessage));
            }
        }
        private AttributeNode GetCustomAttribute(int i)
        {
            CustomAttributeRow ca = this.tables.CustomAttributeTable[i];
            TypeNodeList varArgTypes; //Ignored because vararg constructors are not allowed in Custom Attributes
            Method cons = this.GetConstructorDefOrRef(ca.Constructor, out varArgTypes);
            if (cons == null) cons = new Method();
            int blobLength;
            MemoryCursor sigReader = this.tables.GetBlobCursor(ca.Value, out blobLength);
            return this.GetCustomAttribute(cons, sigReader, blobLength);
        }
        private AttributeNode GetCustomAttribute(Method/*!*/ cons, MemoryCursor/*!*/ sigReader, int blobLength)
        {
            AttributeNode attr = new AttributeNode();
            attr.Constructor = new MemberBinding(null, cons);

            int n = cons.Parameters == null ? 0 : cons.Parameters.Count;

            ExpressionList arguments = attr.Expressions = new ExpressionList();

            int posAtBlobStart = sigReader.Position;

            sigReader.ReadUInt16(); //Prolog

            for (int j = 0; j < n; j++)
            {
                TypeNode t = TypeNode.StripModifiers(cons.Parameters[j].Type);
                if (t == null) continue;
                TypeNode/*!*/ pt = t;
                object val = null;
                try
                {
                    val = this.GetCustomAttributeLiteralValue(sigReader, ref pt);
                }
                catch (Exception e)
                {
                    if (this.module.MetadataImportErrors == null) this.module.MetadataImportErrors = new ArrayList();
                    this.module.MetadataImportErrors.Add(e);
                }
                Literal lit = val as Literal;
                if (lit == null) lit = new Literal(val, pt);
                arguments.Add(lit);
            }
            if (sigReader.Position + 1 < posAtBlobStart + blobLength)
            {
                ushort numNamed = sigReader.ReadUInt16();
                this.GetCustomAttributeNamedArguments(arguments, numNamed, sigReader);
            }
            return attr;
        }
        private void GetCustomAttributeNamedArguments(ExpressionList/*!*/ arguments, ushort numNamed, MemoryCursor/*!*/ sigReader)
        {
            for (int j = 0; j < numNamed; j++)
            {
                int nameTag = sigReader.ReadByte();
                bool mustBox = sigReader.Byte(0) == (byte)ElementType.BoxedEnum;
                TypeNode/*!*/ vType = this.ParseTypeSignature(sigReader);
                Identifier id = sigReader.ReadIdentifierFromSerString();
                object val = this.GetCustomAttributeLiteralValue(sigReader, ref vType);
                Literal lit = val as Literal;
                if (lit == null) lit = new Literal(val, vType);
                NamedArgument narg = new NamedArgument(id, lit);
                narg.Type = vType;
                narg.IsCustomAttributeProperty = nameTag == 0x54;
                narg.ValueIsBoxed = mustBox;
                arguments.Add(narg);
            }
        }
        private object GetCustomAttributeLiteralValue(MemoryCursor/*!*/ sigReader, TypeNode/*!*/ type)
        {
            TypeNode/*!*/ t = type;
            object result = this.GetCustomAttributeLiteralValue(sigReader, ref t);
            EnumNode enumType = t as EnumNode;
            if (enumType != null && type == CoreSystemTypes.Object) result = new Literal(result, enumType);
            return result;
        }
        private object GetCustomAttributeLiteralValue(MemoryCursor/*!*/ sigReader, ref TypeNode/*!*/ type)
        {
            if (type == null) return sigReader.ReadInt32();
            switch (type.typeCode)
            {
                case ElementType.Boolean: return sigReader.ReadBoolean();
                case ElementType.Char: return sigReader.ReadChar();
                case ElementType.Double: return sigReader.ReadDouble();
                case ElementType.Single: return sigReader.ReadSingle();
                case ElementType.Int16: return sigReader.ReadInt16();
                case ElementType.Int32: return sigReader.ReadInt32();
                case ElementType.Int64: return sigReader.ReadInt64();
                case ElementType.Int8: return sigReader.ReadSByte();
                case ElementType.UInt16: return sigReader.ReadUInt16();
                case ElementType.UInt32: return sigReader.ReadUInt32();
                case ElementType.UInt64: return sigReader.ReadUInt64();
                case ElementType.UInt8: return sigReader.ReadByte();
                case ElementType.String: return ReadSerString(sigReader);
                case ElementType.ValueType:
                    EnumNode etype = GetCustomAttributeEnumNode(ref type);
                    return this.GetCustomAttributeLiteralValue(sigReader, etype.UnderlyingType);
                case ElementType.Class: return this.GetTypeFromSerializedName(ReadSerString(sigReader));
                case ElementType.SzArray:
                    int numElems = sigReader.ReadInt32();
                    TypeNode elemType = ((ArrayType)type).ElementType;
                    return this.GetCustomAttributeLiteralArray(sigReader, numElems, elemType);
                case ElementType.Object:
                    {
                        type = this.ParseTypeSignature(sigReader);
                        return this.GetCustomAttributeLiteralValue(sigReader, ref type);
                    }
            }
            throw new InvalidMetadataException(ExceptionStrings.UnexpectedTypeInCustomAttribute);
        }
        private static EnumNode/*!*/ GetCustomAttributeEnumNode(ref TypeNode/*!*/ type)
        {
            EnumNode etype = ((TypeNode)type) as EnumNode;
            if (etype == null || etype.UnderlyingType == null)
            {
                //Happens when type is declared in a assembly that has not been resolved. In that case only the type name
                //and the fact that it is a value type is known. There is no completely safe recovery from it, but at this point we
                //can fake up an enum with Int32 as underlying type. This works in most situations.
                etype = new EnumNode();
                etype.Name = type.Name;
                etype.Namespace = type.Namespace;
                etype.DeclaringModule = type.DeclaringModule;
                etype.UnderlyingType = CoreSystemTypes.Int32;
                type = etype;
            }
            return etype;
        }
        private Array GetCustomAttributeLiteralArray(MemoryCursor/*!*/ sigReader, int numElems, TypeNode/*!*/ elemType)
        {
            Array array = this.ConstructCustomAttributeLiteralArray(numElems, elemType);
            for (int i = 0; i < numElems; i++)
            {
                object elem = this.GetCustomAttributeLiteralValue(sigReader, elemType);
                array.SetValue(elem, i);
            }
            return array;
        }
        private Array ConstructCustomAttributeLiteralArray(int numElems, TypeNode/*!*/ elemType)
        {
            if (numElems == -1) return null;
            if (numElems < 0) throw new InvalidMetadataException(ExceptionStrings.UnexpectedTypeInCustomAttribute);
            switch (elemType.typeCode)
            {
                case ElementType.Boolean: return new Boolean[numElems];
                case ElementType.Char: return new Char[numElems];
                case ElementType.Double: return new Double[numElems];
                case ElementType.Single: return new Single[numElems];
                case ElementType.Int16: return new Int16[numElems];
                case ElementType.Int32: return new Int32[numElems];
                case ElementType.Int64: return new Int64[numElems];
                case ElementType.Int8: return new SByte[numElems];
                case ElementType.UInt16: return new UInt16[numElems];
                case ElementType.UInt32: return new UInt32[numElems];
                case ElementType.UInt64: return new UInt64[numElems];
                case ElementType.UInt8: return new Byte[numElems];
                case ElementType.String: return new String[numElems];
                // Only enum value types are legal in attribute instances as stated in section 17.1.3 of the C# 1.0 spec
                case ElementType.ValueType:
                    TypeNode/*!*/ elType = elemType;
                    EnumNode eType = GetCustomAttributeEnumNode(ref elType);
                    return this.ConstructCustomAttributeLiteralArray(numElems, eType.UnderlyingType);
                // This needs to be a TypeNode since GetCustomAttributeLiteralValue will return a Struct if the Type is a value type
                case ElementType.Class: return new TypeNode[numElems];
                // REVIEW: Is this the right exception? Is this the right exception string?
                // Multi-dimensional arrays are not legal in attribute instances according section 17.1.3 of the C# 1.0 spec
                case ElementType.SzArray: throw new InvalidMetadataException(ExceptionStrings.BadCustomAttributeTypeEncodedToken);
                case ElementType.Object: return new Object[numElems];
            }
            throw new InvalidMetadataException(ExceptionStrings.UnexpectedTypeInCustomAttribute);
        }
        //TODO: rewrite this entire mess using a proper grammar based parser
        private TypeNode/*!*/ GetTypeFromSerializedName(string serializedName)
        {
            if (serializedName == null) return null;
            string assemblyName = null;
            string typeName = serializedName;
            int firstComma = FindFirstCommaOutsideBrackets(serializedName);
            if (firstComma > 0)
            {
                int i = 1;
                while (firstComma + i < serializedName.Length && serializedName[firstComma + i] == ' ') i++;
                assemblyName = serializedName.Substring(firstComma + i);
                typeName = serializedName.Substring(0, firstComma);
            }
            return this.GetTypeFromSerializedName(typeName, assemblyName);
        }
        private static int FindFirstCommaOutsideBrackets(string/*!*/ serializedName)
        {
            int numBrackets = 0;
            int numAngles = 0;
            for (int i = 0, n = serializedName == null ? 0 : serializedName.Length; i < n; i++)
            {
                char ch = serializedName[i];
                if (ch == '[')
                    numBrackets++;
                else if (ch == ']')
                {
                    if (--numBrackets < 0) return -1;
                }
                else if (ch == '<')
                    numAngles++;
                else if (ch == '>')
                {
                    if (--numAngles < 0) return -1;
                }
                else if (ch == ',' && numBrackets == 0 && numAngles == 0)
                    return i;
            }
            return -1;
        }
        private TypeNode/*!*/ GetTypeFromSerializedName(string/*!*/ typeName, string assemblyName)
        {
            string/*!*/ nspace, name;
            int i;
            ParseTypeName(typeName, out nspace, out name, out i);
            Module tMod = null;
            TypeNode t = this.LookupType(nspace, name, assemblyName, out tMod);
            if (t == null)
            {
                if (i < typeName.Length && typeName[i] == '!')
                {
                    int codedIndex = 0;
                    if (PlatformHelpers.TryParseInt32(typeName.Substring(0, i), out codedIndex))
                    {
                        t = this.DecodeAndGetTypeDefOrRefOrSpec(codedIndex);
                        if (t != null) return t;
                    }
                }
                t = this.GetDummyTypeNode(Identifier.For(nspace), Identifier.For(name), tMod, null, false);
            }
            if (i >= typeName.Length) return t;
            char ch = typeName[i];
            if (ch == '+') return this.GetTypeFromSerializedName(typeName.Substring(i + 1), t);
            if (ch == '&') return t.GetReferenceType();
            if (ch == '*') return t.GetPointerType();
            if (ch == '[') return this.ParseArrayOrGenericType(typeName.Substring(i + 1, typeName.Length - 1 - i), t);
            throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName);
        }
        private TypeNode/*!*/ GetTypeFromSerializedName(string/*!*/ typeName, TypeNode/*!*/ nestingType)
        {
            string/*!*/ name;
            int i = 0;
            ParseSimpleTypeName(typeName, out name, ref i);
            TypeNode t = nestingType.GetNestedType(Identifier.For(name));
            if (t == null)
                t = this.GetDummyTypeNode(Identifier.Empty, Identifier.For(name), nestingType.DeclaringModule, nestingType, false);
            if (i >= typeName.Length) return t;
            char ch = typeName[i];
            if (ch == '+') return this.GetTypeFromSerializedName(typeName.Substring(i + 1), t);
            if (ch == '&') return t.GetReferenceType();
            if (ch == '*') return t.GetPointerType();
            if (ch == '[') return this.ParseArrayOrGenericType(typeName.Substring(i + 1, typeName.Length - 1 - i), t);
            throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName);
        }
        private TypeNode/*!*/ ParseArrayOrGenericType(string typeName, TypeNode/*!*/ rootType)
        {
            if (typeName == null || rootType == null) { Debug.Assert(false); return rootType; }
            //Get here after "rootType[" has been parsed. What follows is either an array type specifier or some generic type arguments.
            if (typeName.Length == 0)
                throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName); //Something ought to follow the [
            if (typeName[0] == ']')
            { //Single dimensional array with zero lower bound
                if (typeName.Length == 1) return rootType.GetArrayType(1);
                if (typeName[1] == '[' && typeName.Length > 2)
                    return this.ParseArrayOrGenericType(typeName.Substring(2), rootType.GetArrayType(1));
                throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName);
            }
            if (typeName[0] == '*')
            { //Single dimensional array with unknown lower bound
                if (typeName.Length > 1 && typeName[1] == ']')
                {
                    if (typeName.Length == 2) return rootType.GetArrayType(1, true);
                    if (typeName[2] == '[' && typeName.Length > 3)
                        return this.ParseArrayOrGenericType(typeName.Substring(3), rootType.GetArrayType(1, true));
                }
                throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName);
            }
            if (typeName[0] == ',')
            { //Muti dimensional array
                int rank = 1;
                while (rank < typeName.Length && typeName[rank] == ',') rank++;
                if (rank < typeName.Length && typeName[rank] == ']')
                {
                    if (typeName.Length == rank + 1) return rootType.GetArrayType(rank + 1);
                    if (typeName[rank + 1] == '[' && typeName.Length > rank + 2)
                        return this.ParseArrayOrGenericType(typeName.Substring(rank + 2), rootType.GetArrayType(rank));
                }
                throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName);
            }
            //Generic type instance
            int offset = 0;
            if (typeName[0] == '[') offset = 1; //Assembly qualified type name forming part of a generic parameter list        
            TypeNodeList arguments = new TypeNodeList();
            int commaPos = FindFirstCommaOutsideBrackets(typeName);
            while (commaPos > 1)
            {
                arguments.Add(this.GetTypeFromSerializedName(typeName.Substring(offset, commaPos - offset)));
                typeName = typeName.Substring(commaPos + 1);
                offset = typeName[0] == '[' ? 1 : 0;
                commaPos = FindFirstCommaOutsideBrackets(typeName);
            }
            //Find the position of the first unbalanced ].
            int lastCharPos = offset;
            for (int leftBracketCount = 0; lastCharPos < typeName.Length; lastCharPos++)
            {
                char ch = typeName[lastCharPos];
                if (ch == '[') leftBracketCount++;
                else if (ch == ']')
                {
                    leftBracketCount--;
                    if (leftBracketCount < 0) break;
                }
            }
            arguments.Add(this.GetTypeFromSerializedName(typeName.Substring(offset, lastCharPos - offset)));
            TypeNode retVal = rootType.GetGenericTemplateInstance(this.module, arguments);
            if (lastCharPos + 1 < typeName.Length && typeName[lastCharPos + 1] == ']')
                lastCharPos++;
            if (lastCharPos + 1 < typeName.Length)
            {
                //The generic type is complete, but there is yet more to the type
                char ch = typeName[lastCharPos + 1];
                if (ch == '+') retVal = this.GetTypeFromSerializedName(typeName.Substring(lastCharPos + 2), retVal);
                if (ch == '&') retVal = retVal.GetReferenceType();
                if (ch == '*') retVal = retVal.GetPointerType();
                if (ch == '[') retVal = this.ParseArrayOrGenericType(typeName.Substring(lastCharPos + 2, typeName.Length - 1 - lastCharPos - 1), retVal);
            }
            return retVal;
        }
        private static void ParseSimpleTypeName(string/*!*/ source, out string/*!*/ name, ref int i)
        {
            int n = source.Length;
            int start = i;
            for (; i < n; i++)
            {
                char ch = source[i];
                if (ch == '\\') { i++; continue; }
                if (ch == '.' || ch == '+' || ch == '&' || ch == '*' || ch == '[' || ch == '!') break;
                if (ch == '<')
                {
                    int unmatched = 1;
                    while (unmatched > 0 && ++i < n)
                    {
                        ch = source[i];
                        if (ch == '\\') i++;
                        else if (ch == '<') unmatched++;
                        else if (ch == '>') unmatched--;
                    }
                }
            }
            if (i < n)
                name = source.Substring(start, i - start);
            else
                name = source.Substring(start);
        }
        private static void ParseTypeName(string/*!*/ source, out string/*!*/ nspace, out string/*!*/ name, out int i)
        {
            i = 0;
            int n = source.Length;
            nspace = string.Empty;
            while (true)
            {
                int start = i;
                ParseSimpleTypeName(source, out name, ref i);
                if (i < n && source[i] == '.') { i++; continue; }
                if (start != 0) nspace = source.Substring(0, start - 1);
                return;
            }
        }
        private TypeNode LookupType(string/*!*/ nameSpace, string/*!*/ name, string assemblyName, out Module module)
        {
            Identifier namespaceId = Identifier.For(nameSpace);
            Identifier nameId = Identifier.For(name);
            module = this.module;
            //^ assume module != null;
            if (assemblyName == null)
            {
                TypeNode t = module.GetType(namespaceId, nameId);
                if (t != null) return t;
                module = CoreSystemTypes.SystemAssembly;
                return CoreSystemTypes.SystemAssembly.GetType(namespaceId, nameId);
            }
            //See if the type is in one of the assemblies explicitly referenced by the current module
            AssemblyReferenceList arefs = module.AssemblyReferences;
            for (int i = 0, n = arefs == null ? 0 : arefs.Count; i < n; i++)
            {
                AssemblyReference aref = arefs[i];
                if (aref != null && aref.StrongName == assemblyName && aref.Assembly != null)
                {
                    module = aref.Assembly;
                    return aref.Assembly.GetType(namespaceId, nameId);
                }
            }
            //Construct an assembly reference and probe for it
            AssemblyReference aRef = new AssemblyReference(assemblyName);
            AssemblyNode referringAssembly = this.module as AssemblyNode;
            if (referringAssembly != null && (referringAssembly.Flags & AssemblyFlags.Retargetable) != 0)
                aRef.Flags |= AssemblyFlags.Retargetable;
            AssemblyNode aNode = this.GetAssemblyFromReference(aRef);
            if (aNode != null)
            {
                module = aNode;
                TypeNode result = aNode.GetType(namespaceId, nameId);
                return result;
            }
            return null;
        }
        private void GetCustomAttributesFor(Module/*!*/ module)
        {
            try
            {
                if (this.tables.entryPointToken != 0)
                    module.EntryPoint = (Method)this.GetMemberFromToken(this.tables.entryPointToken);
                else
                    module.EntryPoint = Module.NoSuchMethod;
                if (module.NodeType == NodeType.Module)
                {
                    module.Attributes = this.GetCustomAttributesFor((1 << 5) | 7);
                    return;
                }
                AssemblyNode assembly = (AssemblyNode)module;
                assembly.SecurityAttributes = this.GetSecurityAttributesFor((1 << 2) | 2);
                assembly.Attributes = this.GetCustomAttributesFor((1 << 5) | 14);
                assembly.ModuleAttributes = this.GetCustomAttributesFor((1 << 5) | 7);
            }
            catch (Exception e)
            {
                if(this.module == null)
                    return;

                if(this.module.MetadataImportErrors == null)
                    this.module.MetadataImportErrors = new ArrayList();

                this.module.MetadataImportErrors.Add(e);
                module.Attributes = new AttributeList();
            }
        }
        private AttributeList/*!*/ GetCustomAttributesFor(int parentIndex)
        {
            CustomAttributeRow[] customAttributes = this.tables.CustomAttributeTable;
            AttributeList attributes = new AttributeList();
            try
            {
                int i = 0, n = customAttributes.Length, j = n - 1;
                if (n == 0) return attributes;
                bool sorted = (this.sortedTablesMask >> (int)TableIndices.CustomAttribute) % 2 == 1;
                if (sorted)
                {
                    while (i < j)
                    {
                        int k = (i + j) / 2;
                        if (customAttributes[k].Parent < parentIndex)
                            i = k + 1;
                        else
                            j = k;
                    }
                    while (i > 0 && customAttributes[i - 1].Parent == parentIndex) i--;
                }
                for (; i < n; i++)
                    if (customAttributes[i].Parent == parentIndex)
                        attributes.Add(this.GetCustomAttribute(i));
                    else if (sorted)
                        break;
            }
            catch (Exception e)
            {
                if (this.module == null) return attributes;
                if (this.module.MetadataImportErrors == null) this.module.MetadataImportErrors = new ArrayList();
                this.module.MetadataImportErrors.Add(e);
            }

            return attributes;
        }
        private SecurityAttributeList GetSecurityAttributesFor(int parentIndex)
        {
            DeclSecurityRow[] securityAttributes = this.tables.DeclSecurityTable;
            SecurityAttributeList attributes = new SecurityAttributeList();
            try
            {
                int i = 0, n = securityAttributes.Length, j = n - 1;
                if (n == 0) return attributes;
                bool sorted = (this.sortedTablesMask >> (int)TableIndices.DeclSecurity) % 2 == 1;
                if (sorted)
                {
                    while (i < j)
                    {
                        int k = (i + j) / 2;
                        if (securityAttributes[k].Parent < parentIndex)
                            i = k + 1;
                        else
                            j = k;
                    }
                    while (i > 0 && securityAttributes[i - 1].Parent == parentIndex) i--;
                }
                for (; i < n; i++)
                    if (securityAttributes[i].Parent == parentIndex)
                        attributes.Add(this.GetSecurityAttribute(i));
                    else if (sorted)
                        break;
            }
            catch (Exception e)
            {
                if (this.module == null) return attributes;
                if (this.module.MetadataImportErrors == null) this.module.MetadataImportErrors = new ArrayList();
                this.module.MetadataImportErrors.Add(e);
            }

            return attributes;
        }
        private void GetTypeParameterConstraints(int parentIndex, TypeNodeList parameters)
        {
            if (parameters == null) return;
            GenericParamRow[] genericParameters = this.tables.GenericParamTable;
            int i = 0, n = genericParameters.Length, j = n - 1;
            bool sorted = (this.sortedTablesMask >> (int)TableIndices.GenericParam) % 2 == 1;
            if (sorted)
            {
                while (i < j)
                {
                    int k = (i + j) / 2;
                    if (genericParameters[k].Owner < parentIndex)
                        i = k + 1;
                    else
                        j = k;
                }
                while (i > 0 && genericParameters[i - 1].Owner == parentIndex) i--;
            }
            for (int k = 0; i < n && k < parameters.Count; i++, k++)
                if (genericParameters[i].Owner == parentIndex)
                {
                    TypeNode gp = parameters[k];
                    this.GetGenericParameterConstraints(i, ref gp);
                    parameters[k] = gp;
                }
                else if (sorted)
                    break;
        }
        private TypeNodeList GetTypeParametersFor(int parentIndex, Member parent)
        {
            GenericParamRow[] genericParameters = this.tables.GenericParamTable;
            TypeNodeList types = new TypeNodeList();
            int i = 0, n = genericParameters.Length, j = n - 1;
            bool sorted = (this.sortedTablesMask >> (int)TableIndices.GenericParam) % 2 == 1;
            if (sorted)
            {
                while (i < j)
                {
                    int k = (i + j) / 2;
                    if (genericParameters[k].Owner < parentIndex)
                        i = k + 1;
                    else
                        j = k;
                }
                while (i > 0 && genericParameters[i - 1].Owner == parentIndex) i--;
            }
            for (int index = 0; i < n; i++, index++)
                if (genericParameters[i].Owner == parentIndex)
                    types.Add(this.GetGenericParameter(i, index, parent));
                else if (sorted)
                    break;
            if (types.Count == 0) return null;
            return types;
        }
        private TypeNode GetGenericParameter(int index, int parameterListIndex, Member parent)
        {
            GenericParamRow[] genericParameters = this.tables.GenericParamTable;
            GenericParamRow gpr = genericParameters[index++];
            string name = this.tables.GetString(gpr.Name);
            GenericParamConstraintRow[] genericParameterConstraints = this.tables.GenericParamConstraintTable;
            bool isClass = false;
            int i = 0, n = genericParameterConstraints.Length, j = n - 1;
            bool sorted = (this.sortedTablesMask >> (int)TableIndices.GenericParamConstraint) % 2 == 1;
            if (sorted)
            {
                while (i < j)
                {
                    int k = (i + j) / 2;
                    if (genericParameterConstraints[k].Param < index)
                        i = k + 1;
                    else
                        j = k;
                }
                while (i > 0 && genericParameterConstraints[i - 1].Param == index) i--;
            }
            for (; i < n && !isClass; i++)
            {
                if (genericParameterConstraints[i].Param == index)
                {
                    isClass = this.TypeDefOrRefOrSpecIsClass(genericParameterConstraints[i].Constraint);
                }
                else if (sorted)
                    break;
            }
            if (isClass)
            {
                ClassParameter cp = parent is Method ? new MethodClassParameter() : new ClassParameter();
                cp.DeclaringMember = parent;
                cp.ParameterListIndex = parameterListIndex;
                cp.Name = Identifier.For(name);
                cp.DeclaringModule = this.module;
                cp.TypeParameterFlags = (TypeParameterFlags)gpr.Flags;
                return cp;
            }
            TypeParameter tp = parent is Method ? new MethodTypeParameter() : new TypeParameter();
            tp.DeclaringMember = parent;
            tp.ParameterListIndex = parameterListIndex;
            tp.Name = Identifier.For(name);
            tp.DeclaringModule = this.module;
            tp.TypeParameterFlags = (TypeParameterFlags)gpr.Flags;
            return tp;
        }
        private void GetGenericParameterConstraints(int index, ref TypeNode/*!*/ parameter)
        {
            Debug.Assert(parameter != null);
            index++;
            GenericParamConstraintRow[] genericParameterConstraints = this.tables.GenericParamConstraintTable;
            TypeNodeList constraints = new TypeNodeList();
            Class baseClass = null;
            InterfaceList interfaces = new InterfaceList();
            int i = 0, n = genericParameterConstraints.Length, j = n - 1;
            bool sorted = (this.sortedTablesMask >> (int)TableIndices.GenericParamConstraint) % 2 == 1;
            if (sorted)
            {
                while (i < j)
                {
                    int k = (i + j) / 2;
                    if (genericParameterConstraints[k].Param < index)
                        i = k + 1;
                    else
                        j = k;
                }
                while (i > 0 && genericParameterConstraints[i - 1].Param == index) i--;
            }
            for (; i < n; i++)
            {
                if (genericParameterConstraints[i].Param == index)
                {
                    TypeNode t = this.DecodeAndGetTypeDefOrRefOrSpec(genericParameterConstraints[i].Constraint);
                    Class c = t as Class;
                    if (c != null)
                        baseClass = c;
                    else if (t is Interface)
                        interfaces.Add((Interface)t);
                    constraints.Add(t);
                }
                else if (sorted)
                    break;
            }
            ClassParameter cp = parameter as ClassParameter;
            if (cp == null && baseClass != null)
            {
                cp = ((ITypeParameter)parameter).DeclaringMember is Method ? new MethodClassParameter() : new ClassParameter();
                cp.Name = parameter.Name;
                cp.DeclaringMember = ((ITypeParameter)parameter).DeclaringMember;
                cp.ParameterListIndex = ((ITypeParameter)parameter).ParameterListIndex;
                cp.DeclaringModule = this.module;
                cp.TypeParameterFlags = ((ITypeParameter)parameter).TypeParameterFlags;
                parameter = cp;
            }
            if (cp != null)
                cp.structuralElementTypes = constraints;
            else
                ((TypeParameter)parameter).structuralElementTypes = constraints;
            if (baseClass != null && cp != null) cp.BaseClass = baseClass;
            parameter.Interfaces = interfaces;
        }
        internal static Block/*!*/ GetOrCreateBlock(TrivialHashtable/*!*/ blockMap, int address)
        {
            Block block = (Block)blockMap[address + 1];
            if (block == null)
            {
                blockMap[address + 1] = block = new Block(new StatementList());
                block.SourceContext.StartPos = address;
            }
            return block;
        }
        internal Field GetFieldFromDef(int i)
        {
            return this.GetFieldFromDef(i, null);
        }
        internal Field GetFieldFromDef(int i, TypeNode declaringType)
        {
            FieldRow[] fieldDefs = this.tables.FieldTable;
            FieldRow fld = fieldDefs[i - 1];
            if (fld.Field != null) return fld.Field;
            Field field = new Field();
            fieldDefs[i - 1].Field = field;
            field.Attributes = this.GetCustomAttributesFor((i << 5) | 1);
            field.Flags = (FieldFlags)fld.Flags;
            field.Name = tables.GetIdentifier(fld.Name);
            if ((field.Flags & FieldFlags.RTSpecialName) != 0 && field.Name.UniqueIdKey == StandardIds._Deleted.UniqueIdKey) return null;
            tables.GetSignatureLength(fld.Signature); //sigLength
            MemoryCursor sigReader = this.tables.GetNewCursor();
            GetAndCheckSignatureToken(6, sigReader);
            field.Type = this.ParseTypeSignature(sigReader);
            RequiredModifier reqMod = field.Type as RequiredModifier;
            if (reqMod != null && reqMod.Modifier == CoreSystemTypes.IsVolatile)
            {
                field.IsVolatile = true;
                field.Type = reqMod.ModifiedType;
            }
            if ((field.Flags & FieldFlags.HasDefault) != 0)
                field.DefaultValue = this.GetLiteral(i << 2, field.Type);
            if ((field.Flags & FieldFlags.HasFieldMarshal) != 0)
                field.MarshallingInformation = this.GetMarshallingInformation((i << 1) | 0);
            if ((field.Flags & FieldFlags.HasFieldRVA) != 0)
                field.InitialData = this.GetInitialData(i, field.Type, out field.section);
            if (declaringType == null)
            {
                TypeDefRow[] typeDefs = this.tables.TypeDefTable;
                int indx = i;
                FieldPtrRow[] fieldPtrs = this.tables.FieldPtrTable;
                int n = fieldPtrs.Length;
                for (int j = 0; j < n; j++)
                {
                    if (fieldPtrs[j].Field == i)
                    {
                        indx = j + 1; break;
                    }
                }
                n = typeDefs.Length;
                for (int j = n - 1; j >= 0; j--)
                { //TODO: binary search
                    TypeDefRow tdr = typeDefs[j];
                    if (tdr.FieldList <= indx)
                    {
                        declaringType = this.GetTypeFromDef(j + 1);
                        break;
                    }
                }
            }
            field.DeclaringType = declaringType;
            if (declaringType != null && (declaringType.Flags & TypeFlags.ExplicitLayout) != 0)
            {
                FieldLayoutRow[] fieldLayouts = this.tables.FieldLayoutTable;
                int n = fieldLayouts.Length;
                for (int j = n - 1; j >= 0; j--)
                { //TODO: binary search
                    FieldLayoutRow flr = fieldLayouts[j];
                    if (flr.Field == i)
                    {
                        field.Offset = flr.Offset;
                        break;
                    }
                }
            }
            return field;
        }
        private byte[] GetInitialData(int fieldIndex, TypeNode fieldType, out PESection targetSection)
        {
            targetSection = PESection.Text;
            FieldRvaRow[] fieldRvaTable = this.tables.FieldRvaTable;
            bool sorted = (this.sortedTablesMask >> (int)TableIndices.FieldRva) % 2 == 1;
            int i = 0, n = fieldRvaTable.Length, j = n - 1;
            if (n == 0) return null;
            if (sorted)
            {
                while (i < j)
                {
                    int k = (i + j) / 2;
                    if (fieldRvaTable[k].Field < fieldIndex)
                        i = k + 1;
                    else
                        j = k;
                }
            }
            else
                for (; i < j; i++)
                    if (fieldRvaTable[i].Field == fieldIndex) break;
            FieldRvaRow frr = fieldRvaTable[i];
            if (frr.Field != fieldIndex) return null;
            Field fld = this.tables.FieldTable[fieldIndex - 1].Field;
            if (fld != null) fld.Offset = frr.RVA;
            fieldType = TypeNode.StripModifiers(fieldType);
            EnumNode enumType = fieldType as EnumNode;
            if (enumType != null) fieldType = TypeNode.StripModifiers(enumType.UnderlyingType);
            if (fieldType == null) { Debug.Fail(""); return null; }
            int size = fieldType.ClassSize;
            if (size <= 0)
            {
                switch (fieldType.typeCode)
                {
                    case ElementType.Boolean: size = 1; break;
                    case ElementType.Char: size = 2; break;
                    case ElementType.Double: size = 8; break;
                    case ElementType.Int16: size = 2; break;
                    case ElementType.Int32: size = 4; break;
                    case ElementType.Int64: size = 8; break;
                    case ElementType.Int8: size = 1; break;
                    case ElementType.Single: size = 4; break;
                    case ElementType.UInt16: size = 2; break;
                    case ElementType.UInt32: size = 4; break;
                    case ElementType.UInt64: size = 8; break;
                    case ElementType.UInt8: size = 1; break;
                    default:
                        if (fieldType is Pointer || fieldType is FunctionPointer)
                        {
                            size = 4; break;
                        }
                        //TODO: this seems wrong
                        if (i < n - 1)
                            size = fieldRvaTable[i + 1].RVA - frr.RVA;
                        else if (targetSection != PESection.Text)
                            size = this.tables.GetOffsetToEndOfSection(frr.RVA);
                        break;
                }
            }
            if (size <= 0) return null;
            if (this.tables.NoOffsetFor(frr.RVA) || this.tables.NoOffsetFor(frr.RVA + size - 1))
                return null;
            MemoryCursor c = this.tables.GetNewCursor(frr.RVA, out targetSection);
            byte[] result = new byte[size];
            for (i = 0; i < size; i++)
                result[i] = c.ReadByte();
            return result;
        }
        private Literal GetLiteral(int parentCodedIndex, TypeNode/*!*/ type)
        {
            ConstantRow[] constants = this.tables.ConstantTable;
            //TODO: do a binary search
            for (int i = 0, n = constants.Length; i < n; i++)
            {
                if (constants[i].Parent != parentCodedIndex) continue;
                object value = this.tables.GetValueFromBlob(constants[i].Type, constants[i].Value);
                TypeCode valTypeCode = System.Convert.GetTypeCode(value);
                TypeNode underlyingType = type;
                if (type is EnumNode) underlyingType = ((EnumNode)type).UnderlyingType;
                if (underlyingType.TypeCode != valTypeCode) type = CoreSystemTypes.Object;
                if (type == CoreSystemTypes.Object && value != null)
                {
                    switch (valTypeCode)
                    {
                        case TypeCode.Boolean: type = CoreSystemTypes.Boolean; break;
                        case TypeCode.Byte: type = CoreSystemTypes.UInt8; break;
                        case TypeCode.Char: type = CoreSystemTypes.Char; break;
                        case TypeCode.Double: type = CoreSystemTypes.Double; break;
                        case TypeCode.Int16: type = CoreSystemTypes.Int16; break;
                        case TypeCode.Int32: type = CoreSystemTypes.Int32; break;
                        case TypeCode.Int64: type = CoreSystemTypes.Int64; break;
                        case TypeCode.SByte: type = CoreSystemTypes.Int8; break;
                        case TypeCode.Single: type = CoreSystemTypes.Single; break;
                        case TypeCode.String: type = CoreSystemTypes.String; break;
                        case TypeCode.UInt16: type = CoreSystemTypes.UInt16; break;
                        case TypeCode.UInt32: type = CoreSystemTypes.UInt32; break;
                        case TypeCode.UInt64: type = CoreSystemTypes.UInt64; break;
                        case TypeCode.Empty:
                        case TypeCode.Object: type = CoreSystemTypes.Type; break;
                    }
                }
                return new Literal(value, type);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadConstantParentIndex);
        }
        internal FunctionPointer GetCalliSignature(int ssigToken)
        {
            StandAloneSigRow ssr = this.tables.StandAloneSigTable[(ssigToken & 0xFFFFFF) - 1];
            MemoryCursor sigReader = this.tables.GetBlobCursor(ssr.Signature);
            return this.ParseFunctionPointer(sigReader);
        }
        internal void GetLocals(int localIndex, LocalList/*!*/ locals, Hashtable/*!*/ localSourceNames)
        {
            if (localIndex == 0) return;
            StandAloneSigRow ssr = this.tables.StandAloneSigTable[(localIndex & 0xFFFFFF) - 1];
            this.tables.GetSignatureLength(ssr.Signature);
            MemoryCursor sigReader = this.tables.GetNewCursor();
            if (sigReader.ReadByte() != 0x7) throw new InvalidMetadataException(ExceptionStrings.InvalidLocalSignature);
            int count = sigReader.ReadCompressedInt();
            for (int i = 0; i < count; i++)
            {
                string lookupName = (string)localSourceNames[i];
                string name = lookupName == null ? "local" + i : lookupName;
                bool pinned = false;
                TypeNode locType = this.ParseTypeSignature(sigReader, ref pinned);
                Local loc = new Local(Identifier.For(name), locType);
                loc.Pinned = pinned;
                locals.Add(loc);
            }
        }

        internal void GetLocalSourceNames(ISymUnmanagedScope/*!*/ scope, Hashtable/*!*/ localSourceNames)
        {
            uint numLocals = scope.GetLocalCount();
            IntPtr[] localPtrs = new IntPtr[numLocals];
            scope.GetLocals((uint)localPtrs.Length, out numLocals, localPtrs);

            char[] nameBuffer = new char[100];
            uint nameLen;
            for (int i = 0; i < numLocals; i++)
            {
                ISymUnmanagedVariable local =
                  (ISymUnmanagedVariable)System.Runtime.InteropServices.Marshal.GetTypedObjectForIUnknown(localPtrs[i], typeof(ISymUnmanagedVariable));
                if (local != null)
                {
                    local.GetName((uint)nameBuffer.Length, out nameLen, nameBuffer);
                    int localIndex = (int)local.GetAddressField1();
                    localSourceNames[localIndex] = new String(nameBuffer, 0, (int)nameLen - 1);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(local);
                }
                System.Runtime.InteropServices.Marshal.Release(localPtrs[i]);
            }

            IntPtr[] subscopes = new IntPtr[100];
            uint numScopes;
            scope.GetChildren((uint)subscopes.Length, out numScopes, subscopes);
            for (int i = 0; i < numScopes; i++)
            {
                ISymUnmanagedScope subscope =
                  (ISymUnmanagedScope)System.Runtime.InteropServices.Marshal.GetTypedObjectForIUnknown(subscopes[i], typeof(ISymUnmanagedScope));
                if (subscope != null)
                {
                    this.GetLocalSourceNames(subscope, localSourceNames);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(subscope);
                }
                System.Runtime.InteropServices.Marshal.Release(subscopes[i]);
                //TODO: need to figure out how map these scope to blocks and set HasLocals on those blocks
            }
        }

        private MarshallingInformation GetMarshallingInformation(int parentCodedIndex)
        {
            FieldMarshalRow[] mtypes = this.tables.FieldMarshalTable;
            bool sorted = (this.sortedTablesMask >> (int)TableIndices.FieldMarshal) % 2 == 1;
            int i = 0, n = mtypes.Length, j = n - 1;
            if (n == 0) return null;
            if (sorted)
            {
                while (i < j)
                {
                    int k = (i + j) / 2;
                    if (mtypes[k].Parent < parentCodedIndex)
                        i = k + 1;
                    else
                        j = k;
                }
                while (i > 0 && mtypes[i - 1].Parent == parentCodedIndex) i--;
            }
            else
                for (; i < j; i++)
                    if (mtypes[i].Parent == parentCodedIndex) break;
            FieldMarshalRow fmr = mtypes[i];
            if (fmr.Parent != parentCodedIndex) return null;
            MarshallingInformation result = new MarshallingInformation();
            int blobSize = 0;
            MemoryCursor c = this.tables.GetBlobCursor(fmr.NativeType, out blobSize);
            int initialPosition = c.Position;
            result.NativeType = (NativeType)c.ReadByte();
            if (result.NativeType == NativeType.CustomMarshaler)
            {
                c.ReadUInt16(); //Skip over 0
                result.Class = ReadSerString(c);
                result.Cookie = ReadSerString(c);
            }
            else if (blobSize > 1)
            {
                if (result.NativeType == NativeType.LPArray)
                {
                    result.ElementType = (NativeType)c.ReadByte();
                    result.ParamIndex = -1;
                    int bytesRead = 2;
                    if (bytesRead < blobSize)
                    {
                        int pos = c.Position;
                        result.ParamIndex = c.ReadCompressedInt();
                        bytesRead += c.Position - pos;
                        if (bytesRead < blobSize)
                        {
                            pos = c.Position;
                            result.ElementSize = c.ReadCompressedInt();
                            bytesRead += c.Position - pos;
                            if (bytesRead < blobSize)
                                result.NumberOfElements = c.ReadCompressedInt();
                        }
                    }
                }
                else if (result.NativeType == NativeType.SafeArray)
                {
                    result.ElementType = (NativeType)c.ReadByte(); //Actually a variant type. TODO: what about VT_VECTOR VT_ARRAY and VT_BYREF?
                    if (c.Position < initialPosition + blobSize - 1)
                        result.Class = ReadSerString(c);
                }
                else
                {
                    result.Size = c.ReadCompressedInt();
                    if (result.NativeType == NativeType.ByValArray)
                    {
                        if (blobSize > 2)
                            result.ElementType = (NativeType)c.ReadByte();
                        else
                            result.ElementType = NativeType.NotSpecified;
                    }
                }
            }
            return result;
        }

        private void GetMethodBody(Method/*!*/ method, object/*!*/ i, bool asInstructionList)
        {
            if(asInstructionList)
            {
                this.GetMethodInstructions(method, i);
                return;
            }

            TypeNodeList savedCurrentMethodTypeParameters = this.currentMethodTypeParameters;
            this.currentMethodTypeParameters = method.templateParameters;
            TypeNode savedCurrentType = this.currentType;
            this.currentType = method.DeclaringType;

            try
            {
                MethodRow meth = this.tables.MethodTable[((int)i) - 1];
                StatementList statements;

                if(meth.RVA != 0 && (((MethodImplFlags)meth.ImplFlags) & MethodImplFlags.ManagedMask) == MethodImplFlags.Managed)
                {
                    if(this.getDebugSymbols)
                        this.GetMethodDebugSymbols(method, 0x6000000 | (uint)(int)i);

                    statements = this.ParseMethodBody(method, (int)i, meth.RVA);
                }
                else
                    statements = new StatementList();

                method.Body = new Block(statements);
                method.Body.HasLocals = true;
            }
            catch(Exception e)
            {
                if(this.module != null)
                {
                    if(this.module.MetadataImportErrors == null)
                        this.module.MetadataImportErrors = new ArrayList();

                    this.module.MetadataImportErrors.Add(e);
                }

                method.Body = new Block(new StatementList());
            }
            finally
            {
                this.currentMethodTypeParameters = savedCurrentMethodTypeParameters;
                this.currentType = savedCurrentType;
            }
        }

        internal void GetMethodDebugSymbols(Method/*!*/ method, uint methodToken)
        {
            ISymUnmanagedMethod methodInfo = null;
            try
            {
                if(this.debugReader != null)
                    try
                    {
                        this.debugReader.GetMethod(methodToken, ref methodInfo);
                        method.RecordSequencePoints(methodInfo);
                    }
                    catch(COMException)
                    {
                    }
                    catch(InvalidCastException)
                    {
                    }
                    catch(InvalidComObjectException)
                    {
                    }
            }
            finally
            {
                if(methodInfo != null)
                    Marshal.ReleaseComObject(methodInfo);
            }
        }

        private void GetMethodInstructions(Method/*!*/ method, object/*!*/ i)
        {
            TypeNodeList savedCurrentMethodTypeParameters = this.currentMethodTypeParameters;
            this.currentMethodTypeParameters = method.templateParameters;
            TypeNode savedCurrentType = this.currentType;
            this.currentType = method.DeclaringType;
            try
            {
                MethodRow meth = this.tables.MethodTable[((int)i) - 1];
                if (meth.RVA != 0 && (((MethodImplFlags)meth.ImplFlags) & MethodImplFlags.ManagedMask) == MethodImplFlags.Managed)
                {
                    if (this.getDebugSymbols) this.GetMethodDebugSymbols(method, 0x6000000 | (uint)(int)i);
                    method.Instructions = this.ParseMethodInstructions(method, (int)i, meth.RVA);
                }
                else
                    method.Instructions = new InstructionList();
            }
            catch (Exception e)
            {
                if (this.module != null)
                {
                    if (this.module.MetadataImportErrors == null) this.module.MetadataImportErrors = new ArrayList();
                    this.module.MetadataImportErrors.Add(e);
                }

                method.Instructions = new InstructionList();
            }
            finally
            {
                this.currentMethodTypeParameters = savedCurrentMethodTypeParameters;
                this.currentType = savedCurrentType;
            }
        }
        private Method GetMethodDefOrRef(int codedIndex)
        {
            switch (codedIndex & 0x1)
            {
                case 0x00: return this.GetMethodFromDef(codedIndex >> 1);
                case 0x01:
                    TypeNodeList varArgTypes;
                    return (Method)this.GetMemberFromRef(codedIndex >> 1, out varArgTypes);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadCustomAttributeTypeEncodedToken);
        }
        private Method GetMethodDefOrRef(int codedIndex, int numberOfGenericArguments)
        {
            switch (codedIndex & 0x1)
            {
                case 0x00: return this.GetMethodFromDef(codedIndex >> 1);
                case 0x01:
                    TypeNodeList varArgTypes;
                    return (Method)this.GetMemberFromRef(codedIndex >> 1, out varArgTypes, numberOfGenericArguments);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadCustomAttributeTypeEncodedToken);
        }
        internal Method/*!*/ GetMethodFromDef(int index)
        {
            return this.GetMethodFromDef(index, null);
        }
        internal Method/*!*/ GetMethodFromDef(int index, TypeNode declaringType)
        {
            TypeNodeList savedCurrentMethodTypeParameters = this.currentMethodTypeParameters;
            TypeNodeList savedCurrentTypeParameters = this.currentTypeParameters;
            MethodRow[] methodDefs = this.tables.MethodTable;
            MethodRow meth = methodDefs[index - 1];
            if (meth.Method != null) return meth.Method;
            if (declaringType == null)
            {
                int indx = index;
                MethodPtrRow[] methodPtrs = this.tables.MethodPtrTable;
                int n = methodPtrs.Length, i = 0, j = n - 1;
                bool sorted = (this.sortedTablesMask >> (int)TableIndices.MethodPtr) % 2 == 1;
                if (sorted)
                {
                    while (i < j)
                    {
                        int k = (i + j) / 2;
                        if (methodPtrs[k].Method < index)
                            i = k + 1;
                        else
                            j = k;
                    }
                    while (i > 0 && methodPtrs[i - 1].Method == index) i--;
                }
                for (; i < n; i++)
                {
                    if (methodPtrs[i].Method == index)
                    {
                        indx = i + 1; break;
                    }
                }
                TypeDefRow[] typeDefs = this.tables.TypeDefTable;
                n = typeDefs.Length; i = 0; j = n - 1;
                sorted = (this.sortedTablesMask >> (int)TableIndices.TypeDef) % 2 == 1;
                if (sorted)
                {
                    while (i < j)
                    {
                        int k = (i + j) / 2;
                        if (typeDefs[k].MethodList < indx)
                            i = k + 1;
                        else
                            j = k;
                    }
                    j = i;
                    while (j < n - 1 && typeDefs[j + 1].MethodList == indx) j++;
                }
                for (; j >= 0; j--)
                {
                    if (typeDefs[j].MethodList <= indx)
                    {
                        declaringType = this.GetTypeFromDef(j + 1);
                        break;
                    }
                }
            }
            Method.MethodBodyProvider provider = new Method.MethodBodyProvider(this.GetMethodBody);
            Identifier name = tables.GetIdentifier(meth.Name);
            Method method;
            if ((((MethodFlags)meth.Flags) & MethodFlags.SpecialName) != 0 &&
              (((MethodFlags)meth.Flags) & MethodFlags.SpecialName) != 0)
            {
                if (name.Name == ".ctor")
                    method = methodDefs[index - 1].Method = new InstanceInitializer(provider, index);
                else if (name.Name == ".cctor")
                    method = methodDefs[index - 1].Method = new StaticInitializer(provider, index);
                else
                    method = methodDefs[index - 1].Method = new Method(provider, index);
            }
            else
                method = methodDefs[index - 1].Method = new Method(provider, index);
            method.ProvideMethodAttributes = new Method.MethodAttributeProvider(this.GetMethodAttributes);
            //method.Attributes = this.GetCustomAttributesFor((index << 5)|0); //TODO: get attributes lazily
            method.Flags = (MethodFlags)meth.Flags;
            method.ImplFlags = (MethodImplFlags)meth.ImplFlags;
            method.Name = name;
            if (declaringType != null && declaringType.IsGeneric)
            {
                if (declaringType.Template != null)
                    this.currentTypeParameters = declaringType.ConsolidatedTemplateArguments;
                else
                    this.currentTypeParameters = declaringType.ConsolidatedTemplateParameters;
            }

            tables.GetSignatureLength(meth.Signature);
            MemoryCursor sigReader = this.tables.GetNewCursor();
            method.CallingConvention = (CallingConventionFlags)sigReader.ReadByte();
            if (method.IsGeneric = (method.CallingConvention & CallingConventionFlags.Generic) != 0)
            {
                int numTemplateParameters = sigReader.ReadCompressedInt();
                this.currentMethodTypeParameters = new TypeNodeList();
                this.currentMethodTypeParameters = method.TemplateParameters = this.GetTypeParametersFor((index << 1) | 1, method);
                this.GetTypeParameterConstraints((index << 1) | 1, method.TemplateParameters);
            }
            int numParams = sigReader.ReadCompressedInt();
            method.ReturnType = this.ParseTypeSignature(sigReader);
            if (declaringType != null && declaringType.IsValueType)
                method.ThisParameter = new This(declaringType.GetReferenceType());
            else
                method.ThisParameter = new This(declaringType);

            ParameterList paramList = method.Parameters = new ParameterList();

            if(numParams > 0)
            {
                int offset = method.IsStatic ? 0 : 1;
                for (int i = 0; i < numParams; i++)
                {
                    Parameter param = new Parameter();
                    param.ParameterListIndex = i;
                    param.ArgumentListIndex = i + offset;
                    param.Type = this.ParseTypeSignature(sigReader);
                    param.DeclaringMethod = method;
                    paramList.Add(param);
                }
                int end = this.tables.ParamTable.Length + 1;
                if (index < methodDefs.Length) end = methodDefs[index].ParamList;
                this.AddMoreStuffToParameters(method, paramList, meth.ParamList, end);
                for (int i = 0; i < numParams; i++)
                {
                    Parameter param = paramList[i];
                    if (param.Name == null)
                        param.Name = Identifier.For("param" + (i));
                }
            }
            else if (method.ReturnType != CoreSystemTypes.Void)
            {
                //check for custom attributes and marshaling information on return value
                int i = meth.ParamList;
                ParamPtrRow[] parPtrs = this.tables.ParamPtrTable; //TODO: why use ParamPtrTable in the branch and not the one above? Factor this out.
                ParamRow[] pars = this.tables.ParamTable;
                int n = methodDefs.Length;
                int m = pars.Length;
                if (index < n) m = methodDefs[index].ParamList - 1;
                if (parPtrs.Length > 0)
                {
                    if (pars != null && 0 < i && i <= m)
                    {
                        int j = parPtrs[i - 1].Param;
                        ParamRow pr = pars[j - 1];
                        if (pr.Sequence == 0)
                            this.AddMoreStuffToParameters(method, null, j, j + 1);
                    }
                }
                else
                {
                    if (pars != null && 0 < i && i <= m)
                    {
                        ParamRow pr = pars[i - 1];
                        if (pr.Sequence == 0)
                            this.AddMoreStuffToParameters(method, null, i, i + 1);
                    }
                }
            }

            //if ((method.Flags & MethodFlags.HasSecurity) != 0)
            //  method.SecurityAttributes = this.GetSecurityAttributesFor((index << 2)|1);

            if ((method.Flags & MethodFlags.PInvokeImpl) != 0)
            {
                ImplMapRow[] implMaps = this.tables.ImplMapTable;
                int n = implMaps.Length, i = 0, j = n - 1;
                bool sorted = (this.sortedTablesMask >> (int)TableIndices.ImplMap) % 2 == 1;
                if (sorted)
                {
                    while (i < j)
                    {
                        int k = (i + j) / 2;
                        if ((implMaps[k].MemberForwarded >> 1) < index)
                            i = k + 1;
                        else
                            j = k;
                    }
                    while (i > 0 && (implMaps[i - 1].MemberForwarded >> 1) == index) i--;
                }
                for (; i < n; i++)
                {
                    ImplMapRow imr = implMaps[i];
                    if (imr.MemberForwarded >> 1 == index)
                    {
                        method.PInvokeFlags = (PInvokeFlags)imr.MappingFlags;
                        method.PInvokeImportName = tables.GetString(imr.ImportName);
                        method.PInvokeModule = this.module.ModuleReferences[imr.ImportScope - 1].Module;
                        break;
                    }
                }
            }
            method.DeclaringType = declaringType;
            this.currentMethodTypeParameters = savedCurrentMethodTypeParameters;
            this.currentTypeParameters = savedCurrentTypeParameters;
            return method;
        }
        private void GetMethodAttributes(Method/*!*/ method, object/*!*/ handle)
        {
            TypeNodeList savedCurrentTypeParameters = this.currentTypeParameters;
            TypeNodeList savedCurrentMethodTypeParameters = this.currentMethodTypeParameters;
            try
            {
                MetadataReader tables = this.tables;
                int index = (int)handle;
                MethodRow[] methodDefs = tables.MethodTable;
                int n = methodDefs.Length;
                if (index < 1 || index > n)
                    throw new System.ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                MethodRow md = methodDefs[index - 1];
                if (method != md.Method) throw new System.ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                //Get custom attributes   
                method.Attributes = this.GetCustomAttributesFor((index << 5) | 0);
                this.currentTypeParameters = savedCurrentTypeParameters;
                this.currentMethodTypeParameters = savedCurrentMethodTypeParameters;
                //Get security attributes
                if ((method.Flags & MethodFlags.HasSecurity) != 0)
                    method.SecurityAttributes = this.GetSecurityAttributesFor((index << 2) | 1);
            }
            catch(Exception e)
            {
                if(this.module != null)
                {
                    if(this.module.MetadataImportErrors == null)
                        this.module.MetadataImportErrors = new ArrayList();

                    this.module.MetadataImportErrors.Add(e);
                }

                method.Attributes = new AttributeList();
                this.currentTypeParameters = savedCurrentTypeParameters;
                this.currentMethodTypeParameters = savedCurrentMethodTypeParameters;
            }
        }

        private Method/*!*/ GetMethodFromSpec(int i)
        {
            MethodSpecRow[] methodSpecs = this.tables.MethodSpecTable;
            MethodSpecRow msr = methodSpecs[i - 1];
            if (msr.InstantiatedMethod != null) return msr.InstantiatedMethod;
            MemoryCursor sigReader = this.tables.GetBlobCursor(msr.Instantiation);
            byte header = sigReader.ReadByte(); //skip over redundant header byte
            Debug.Assert(header == 0x0a);
            TypeNodeList templateArguments = this.ParseTypeList(sigReader);
            Method template = this.GetMethodDefOrRef(msr.Method, templateArguments.Count);
            if (template == null) return new Method();
            if (template.TemplateParameters == null) return template; //Likely a dummy method
            return template.GetTemplateInstance(this.currentType, templateArguments);
        }
        internal Member/*!*/ GetMemberFromToken(int tok)
        {
            TypeNodeList varArgTypes;
            return this.GetMemberFromToken(tok, out varArgTypes);
        }
        internal Member/*!*/ GetMemberFromToken(int tok, out TypeNodeList varArgTypes)
        {
            varArgTypes = null;
            Member member = null;
            switch ((TableIndices)(tok >> 24))
            {
                case TableIndices.Field: member = this.GetFieldFromDef(tok & 0xFFFFFF); break;
                case TableIndices.Method: member = this.GetMethodFromDef(tok & 0xFFFFFF); break;
                case TableIndices.MemberRef: member = this.GetMemberFromRef(tok & 0xFFFFFF, out varArgTypes); break;
                case TableIndices.TypeDef: member = this.GetTypeFromDef(tok & 0xFFFFFF); break;
                case TableIndices.TypeRef: member = this.GetTypeFromRef(tok & 0xFFFFFF); break;
                case TableIndices.TypeSpec: member = this.GetTypeFromSpec(tok & 0xFFFFFF); break;
                case TableIndices.MethodSpec: member = this.GetMethodFromSpec(tok & 0xFFFFFF); break;
                default: throw new InvalidMetadataException(ExceptionStrings.BadMemberToken);
            }
            if (member == null) throw new InvalidMetadataException(ExceptionStrings.BadMemberToken);
            return member;
        }
        internal Member GetMemberFromRef(int i, out TypeNodeList varArgTypes)
        {
            return this.GetMemberFromRef(i, out varArgTypes, 0);
        }
        internal Member GetMemberFromRef(int i, out TypeNodeList varArgTypes, int numGenericArgs)
        {
            MemberRefRow mref = this.tables.MemberRefTable[i - 1];
            if (mref.Member != null)
            {
                varArgTypes = mref.VarargTypes;
                return mref.Member;
            }
            varArgTypes = null;
            Member result = null;
            int codedIndex = mref.Class;
            if (codedIndex == 0) return null;
            TypeNode parent = null;
            TypeNodeList savedCurrentTypeParameters = this.currentTypeParameters;
            switch (codedIndex & 0x7)
            {
                case 0x00: parent = this.GetTypeFromDef(codedIndex >> 3); break;
                case 0x01: parent = this.GetTypeFromRef(codedIndex >> 3); break;
                case 0x02: parent = this.GetTypeGlobalMemberContainerTypeFromModule(codedIndex >> 3); break;
                case 0x03: result = this.GetMethodFromDef(codedIndex >> 3);
                    if ((((Method)result).CallingConvention & CallingConventionFlags.VarArg) != 0)
                    {
                        MemoryCursor sRdr = this.tables.GetBlobCursor(mref.Signature);
                        sRdr.ReadByte(); //hdr
                        int pCount = sRdr.ReadCompressedInt();
                        this.ParseTypeSignature(sRdr); //rType
                        bool genParameterEncountered = false;
                        this.ParseParameterTypes(out varArgTypes, sRdr, pCount, ref genParameterEncountered);
                    }
                    goto done;
                case 0x04: parent = this.GetTypeFromSpec(codedIndex >> 3); break;
                default: throw new InvalidMetadataException("");
            }
            if (parent != null && parent.IsGeneric)
            {
                if (parent.Template != null)
                    this.currentTypeParameters = parent.ConsolidatedTemplateArguments;
                else
                    this.currentTypeParameters = parent.ConsolidatedTemplateParameters;
            }
            Identifier memberName = this.tables.GetIdentifier(mref.Name);
            MemoryCursor sigReader = this.tables.GetBlobCursor(mref.Signature);
            byte header = sigReader.ReadByte();
            if (header == 0x6)
            {
                TypeNode fieldType = this.ParseTypeSignature(sigReader);
                TypeNode fType = TypeNode.StripModifiers(fieldType);
                TypeNode parnt = parent;
                while (parnt != null)
                {
                    MemberList members = parnt.GetMembersNamed(memberName);
                    for (int j = 0, n = members.Count; j < n; j++)
                    {
                        Field f = members[j] as Field;
                        if (f == null) continue;
                        if (TypeNode.StripModifiers(f.Type) == fType) { result = f; goto done; }
                    }
                    Class c = parnt as Class;
                    if (c != null) parnt = c.BaseClass; else break;
                }
                if (result == null)
                {
                    result = new Field(memberName);
                    result.DeclaringType = parent;
                    ((Field)result).Type = fieldType;
                    goto error;
                }
                goto done;
            }
            int typeParamCount = int.MinValue;
            CallingConventionFlags callingConvention = CallingConventionFlags.Default;
            if ((header & 0x20) != 0) callingConvention |= CallingConventionFlags.HasThis;
            if ((header & 0x40) != 0) callingConvention |= CallingConventionFlags.ExplicitThis;
            switch (header & 7)
            {
                case 1: callingConvention |= CallingConventionFlags.C; break;
                case 2: callingConvention |= CallingConventionFlags.StandardCall; break;
                case 3: callingConvention |= CallingConventionFlags.ThisCall; break;
                case 4: callingConvention |= CallingConventionFlags.FastCall; break;
                case 5: callingConvention |= CallingConventionFlags.VarArg; break;
            }
            if ((header & 0x10) != 0)
            {
                typeParamCount = sigReader.ReadCompressedInt();
                callingConvention |= CallingConventionFlags.Generic;
            }
            int paramCount = sigReader.ReadCompressedInt();
            TypeNodeList savedMethodTypeParameters = this.currentMethodTypeParameters;
            this.currentTypeParameters = parent.ConsolidatedTemplateArguments;
            TypeNode pnt = parent;
            if (numGenericArgs > 0)
            {
                while (pnt != null)
                {
                    MemberList members = pnt.GetMembersNamed(memberName);
                    for (int k = 0, n = members.Count; k < n; k++)
                    {
                        Method m = members[k] as Method;
                        if (m == null) continue;
                        if (m.TemplateParameters == null || m.TemplateParameters.Count != numGenericArgs) continue;
                        if (m.Parameters == null || m.Parameters.Count != paramCount) continue;
                        this.currentMethodTypeParameters = m.TemplateParameters;
                        this.currentTypeParameters = pnt.ConsolidatedTemplateArguments;
                        goto parseSignature;
                    }
                    Class c = pnt as Class;
                    if (c != null) pnt = c.BaseClass; else break;
                }
            }
        parseSignature:
            TypeNode returnType = this.ParseTypeSignature(sigReader);
            if (returnType == null) returnType = CoreSystemTypes.Object;
            bool genericParameterEncountered = returnType.IsGeneric;
            TypeNodeList paramTypes = this.ParseParameterTypes(out varArgTypes, sigReader, paramCount, ref genericParameterEncountered);
            this.currentMethodTypeParameters = savedMethodTypeParameters;
            this.currentTypeParameters = savedCurrentTypeParameters;
            pnt = parent;
            while (pnt != null)
            {
                MemberList members = pnt.GetMembersNamed(memberName);
                for (int k = 0, n = members.Count; k < n; k++)
                {
                    Method m = members[k] as Method;
                    if (m == null) continue;
                    if (m.ReturnType == null) continue;
                    TypeNode mrtype = TypeNode.StripModifiers(m.ReturnType);
                    //^ assert mrtype != null;
                    if (!mrtype.IsStructurallyEquivalentTo(TypeNode.StripModifiers(returnType))) continue;
                    if (!m.ParameterTypesMatchStructurally(paramTypes)) continue;
                    if (m.CallingConvention != callingConvention) continue;
                    if (typeParamCount != int.MinValue && (!m.IsGeneric || m.TemplateParameters == null || m.TemplateParameters.Count != typeParamCount))
                        continue;
                    result = m;
                    goto done;
                }
                if (memberName.UniqueIdKey == StandardIds.Ctor.UniqueIdKey)
                {
                    //Can't run up the base class chain for constructors.
                    members = pnt.GetConstructors();
                    if (members != null && members.Count == 1 && paramCount == 0)
                    {
                        //Only one constructor. The CLR metadata API's seem to think that this should match the empty signature
                        result = members[0];
                        goto done;
                    }
                    break;
                }
                Class c = pnt as Class;
                if (c != null) pnt = c.BaseClass; else break;
            }
            if (result == null)
            {
                ParameterList parameters = new ParameterList();

                for (int j = 0; j < paramCount; j++)
                {
                    Parameter p = new Parameter(Identifier.Empty, paramTypes[j]);
                    parameters.Add(p);
                }

                //TODO: let the caller indicate if it expects a constructor
                Method meth = new Method(parent, null, memberName, parameters, returnType, null);
                meth.CallingConvention = callingConvention;
                if ((callingConvention & CallingConventionFlags.HasThis) == 0) meth.Flags |= MethodFlags.Static;
                result = meth;
            }
        error:
            if (this.module != null)
            {
                HandleError(this.module, String.Format(CultureInfo.CurrentCulture,
                  ExceptionStrings.CouldNotResolveMemberReference, parent.FullName + "::" + memberName));
                if (parent != null) parent.Members.Add(result);
            }
        done:
            if (Reader.CanCacheMember(result))
            {
                this.tables.MemberRefTable[i - 1].Member = result;
                this.tables.MemberRefTable[i - 1].VarargTypes = varArgTypes;
            }
            this.currentTypeParameters = savedCurrentTypeParameters;
            return result;
        }
        private static bool CanCacheMethodHelper(Method/*!*/ method)
        {
            if (method.IsGeneric)
            {
                if (method.TemplateArguments == null)
                    return false;
                for (int i = 0; i < method.TemplateArguments.Count; i++)
                    if (!CanCacheTypeNode(method.TemplateArguments[i]))
                        return false;
            }
            return true;
        }
        private static bool CanCacheMember(Member/*!*/ member)
        {
            return (member.DeclaringType == null || CanCacheTypeNode(member.DeclaringType)) &&
              (member.NodeType != NodeType.Method || CanCacheMethodHelper((Method)member));
        }

        private TypeNodeList/*!*/ ParseParameterTypes(out TypeNodeList varArgTypes, MemoryCursor/*!*/ sigReader,
          int paramCount, ref bool genericParameterEncountered)
        {
            varArgTypes = null;
            TypeNodeList paramTypes = new TypeNodeList();

            for(int j = 0; j < paramCount; j++)
            {
                TypeNode paramType = this.ParseTypeSignature(sigReader);

                if(paramType == null)
                {
                    // Got a sentinel
                    varArgTypes = new TypeNodeList();
                    j--;
                    continue;
                }

                if(varArgTypes != null)
                {
                    varArgTypes.Add(paramType);
                    continue;
                }

                if(paramType.IsGeneric)
                    genericParameterEncountered = true;

                paramTypes.Add(paramType);
            }

            return paramTypes;
        }

        private bool TypeDefIsClass(int i)
        {
            if (i == 0) return false;
            TypeDefRow typeDef = this.tables.TypeDefTable[i - 1];
            if (typeDef.Type != null) return typeDef.Type is Class;
            if ((typeDef.Flags & (int)TypeFlags.Interface) != 0) return false;
            return this.TypeDefOrRefOrSpecIsClassButNotValueTypeBaseClass(typeDef.Extends);
        }
        private bool TypeDefIsClassButNotValueTypeBaseClass(int i)
        {
            if (i == 0) return false;
            TypeDefRow typeDef = this.tables.TypeDefTable[i - 1];
            if (typeDef.Type != null) return typeDef.Type != CoreSystemTypes.ValueType && typeDef.Type != CoreSystemTypes.Enum && typeDef.Type is Class;
            if ((typeDef.Flags & (int)TypeFlags.Interface) != 0) return false;
            return this.TypeDefOrRefOrSpecIsClassButNotValueTypeBaseClass(typeDef.Extends);
        }
        internal TypeNodeList GetInstantiatedTypes()
        {
            TypeNodeList result = null;
            TypeDefRow[] typeDefs = this.tables.TypeDefTable;
            for (int i = 0, n = typeDefs.Length; i < n; i++)
            {
                TypeNode t = typeDefs[i].Type;
                if (t == null) continue;
                if (result == null) result = new TypeNodeList();
                result.Add(t);
            }
            return result;
        }
        internal TypeNode/*!*/ GetTypeFromDef(int i)
        {
            TypeDefRow typeDef = this.tables.TypeDefTable[i - 1];
            if (typeDef.Type != null) return typeDef.Type;
            // Save current state because the helper might change it but this method must not.
            TypeNodeList savedCurrentTypeParameters = this.currentTypeParameters;
            TypeNode savedCurrentType = this.currentType;
            try
            {
                return this.GetTypeFromDefHelper(i);
            }
            catch (Exception e)
            {
                if (this.module == null) return new Class();
                if (this.module.MetadataImportErrors == null) this.module.MetadataImportErrors = new ArrayList();
                this.module.MetadataImportErrors.Add(e);
                return new Class();
            }
            finally
            {
                this.currentTypeParameters = savedCurrentTypeParameters;
                this.currentType = savedCurrentType;
            }
        }
        internal TypeNode/*!*/ GetTypeFromDefHelper(int i)
        {
            // This is added to prevent loops. 
            //  Check the code in GetTypeFromDef which checks != null before calling this function
            this.tables.TypeDefTable[i - 1].Type = Class.Dummy;
            TypeDefRow typeDef = this.tables.TypeDefTable[i - 1];
            Identifier name = this.tables.GetIdentifier(typeDef.Name);
            Identifier namesp = this.tables.GetIdentifier(typeDef.Namespace);
            int firstInterfaceIndex;
            int lastInterfaceIndex;
            this.GetInterfaceIndices(i, out firstInterfaceIndex, out lastInterfaceIndex);
            InterfaceList interfaces = new InterfaceList();
            TypeNode result = this.ConstructCorrectTypeNodeSubclass(i, namesp, firstInterfaceIndex, lastInterfaceIndex,
              (TypeFlags)typeDef.Flags, interfaces, typeDef.Extends,
              name.UniqueIdKey == StandardIds.Enum.UniqueIdKey && namesp.UniqueIdKey == StandardIds.System.UniqueIdKey);
            result.DeclaringModule = this.module;
            result.Name = name;
            result.Namespace = namesp;
            TypeNodeList typeParameters = this.currentTypeParameters = this.GetTypeParametersFor((i << 1) | 0, result);
            result.TemplateParameters = typeParameters;
            result.IsGeneric = typeParameters != null;
            this.tables.TypeDefTable[i - 1].Type = result;
            this.currentType = result;
            this.RemoveTypeParametersBelongToDeclaringType(i, ref typeParameters, result);
            //Now that the type instance has been allocated, it is safe to get hold of things that could refer to this type.
            if (result is Class && result.BaseType == null)
            {
                TypeNode baseType = this.DecodeAndGetTypeDefOrRefOrSpec(typeDef.Extends);
                ((Class)result).BaseClass = baseType as Class;
                if (baseType != null && !(baseType is Class) && this.module != null)
                {
                    HandleError(this.module, ExceptionStrings.InvalidBaseClass);
                }
            }
            if (result.IsGeneric)
                this.GetTypeParameterConstraints((i << 1) | 0, typeParameters);
            if (firstInterfaceIndex >= 0)
                this.GetInterfaces(i, firstInterfaceIndex, interfaces);
            if ((result.Flags & (TypeFlags.ExplicitLayout | TypeFlags.SequentialLayout)) != 0)
                this.GetClassSizeAndPackingSize(i, result);
            return result;
        }

        private void GetInterfaceIndices(int i, out int firstInterfaceIndex, out int lastInterfaceIndex)
        {
            firstInterfaceIndex = -1;
            lastInterfaceIndex = -1;
            InterfaceImplRow[] intfaces = this.tables.InterfaceImplTable;
            //TODO: binary search
            for (int j = 0, n = intfaces.Length; j < n; j++)
            {
                if (intfaces[j].Class != i) continue;
                if (firstInterfaceIndex == -1)
                    firstInterfaceIndex = j;
                lastInterfaceIndex = j;
            }
        }

        private void GetClassSizeAndPackingSize(int i, TypeNode/*!*/ result)
        {
            ClassLayoutRow[] classLayouts = tables.ClassLayoutTable;
            for (int j = 0, n = classLayouts.Length; j < n; j++)
            { //TODO: binary search
                ClassLayoutRow clr = classLayouts[j];
                if (clr.Parent == i)
                {
                    result.ClassSize = clr.ClassSize;
                    result.PackingSize = clr.PackingSize;
                    break;
                }
            }
        }

        private void GetInterfaces(int i, int firstInterfaceIndex, InterfaceList/*!*/ interfaces)
        {
            InterfaceImplRow[] intfaces = this.tables.InterfaceImplTable;
            for (int j = firstInterfaceIndex, n = intfaces.Length; j < n; j++)
            {
                if (intfaces[j].Class != i) continue; //TODO: break if sorted
                TypeNode ifaceT = this.DecodeAndGetTypeDefOrRefOrSpec(intfaces[j].Interface);
                Interface iface = ifaceT as Interface;
                if (iface == null)
                {
                    iface = new Interface();
                    if (ifaceT != null)
                    {
                        iface.DeclaringModule = ifaceT.DeclaringModule;
                        iface.Namespace = ifaceT.Namespace;
                        iface.Name = ifaceT.Name;
                    }
                }
                interfaces.Add(iface);
            }
        }

        private void RemoveTypeParametersBelongToDeclaringType(int i, ref TypeNodeList typeParameters, TypeNode/*!*/ type)
        {
            NestedClassRow[] nestedClasses = tables.NestedClassTable;

            for(int j = 0, n = nestedClasses.Length; j < n; j++)
            { //TODO: binary search
                NestedClassRow ncr = nestedClasses[j];

                if(ncr.NestedClass == i)
                {
                    type.DeclaringType = this.GetTypeFromDef(ncr.EnclosingClass);

                    if(type.DeclaringType != null && type.DeclaringType.IsGeneric)
                    {
                        //remove type parameters that belong to declaring type from nested type's list
                        if(type.templateParameters != null)
                        {
                            int icount = GetInheritedTypeParameterCount(type);
                            int rcount = type.templateParameters.Count;

                            if(icount >= rcount)
                                type.templateParameters = null;
                            else
                            {
                                TypeNodeList tpars = new TypeNodeList();

                                for(int k = icount; k < rcount; k++)
                                    tpars.Add(type.templateParameters[k]);

                                type.templateParameters = tpars;
                            }

                            this.currentTypeParameters = typeParameters = type.ConsolidatedTemplateParameters;
                        }
                    }
                    break;
                }
            }
        }

        private TypeNode/*!*/ ConstructCorrectTypeNodeSubclass(int i, Identifier/*!*/ namesp, int firstInterfaceIndex, int lastInterfaceIndex,
          TypeFlags flags, InterfaceList interfaces, int baseTypeCodedIndex, bool isSystemEnum)
        {
            TypeNode result;
            TypeNode.TypeAttributeProvider attributeProvider = new TypeNode.TypeAttributeProvider(this.GetTypeAttributes);
            TypeNode.NestedTypeProvider nestedTypeProvider = new TypeNode.NestedTypeProvider(this.GetNestedTypes);
            TypeNode.TypeMemberProvider memberProvider = new TypeNode.TypeMemberProvider(this.GetTypeMembers);
            bool isTemplateParameter = false;

            if ((flags & TypeFlags.Interface) != 0)
            {
                if (isTemplateParameter)
                    result = new TypeParameter(interfaces, nestedTypeProvider, attributeProvider, memberProvider, i);
                else
                    result = new Interface(interfaces, nestedTypeProvider, attributeProvider, memberProvider, i);
            }
            else if (isTemplateParameter)
            {
                result = new ClassParameter(nestedTypeProvider, attributeProvider, memberProvider, i);
            }
            else
            {
                result = null;
                TypeNode baseClass = this.GetTypeIfNotGenericInstance(baseTypeCodedIndex);
                if (baseClass != null)
                {
                    if (baseClass == CoreSystemTypes.MulticastDelegate) //TODO: handle single cast delegates
                        result = new DelegateNode(nestedTypeProvider, attributeProvider, memberProvider, i);
                    else if (baseClass == CoreSystemTypes.Enum)
                        result = new EnumNode(nestedTypeProvider, attributeProvider, memberProvider, i);
                    else if (baseClass == CoreSystemTypes.ValueType &&
                      !(isSystemEnum && (flags & TypeFlags.Sealed) == 0))
                    {
                        result = new Struct(nestedTypeProvider, attributeProvider, memberProvider, i);
                    }
                }

                if(result == null)
                    result = new Class(nestedTypeProvider, attributeProvider, memberProvider, i);
            }

            result.Flags = flags;
            result.Interfaces = interfaces;
            return result;
        }

        private TrivialHashtable/*<Ident,TypeExtensionProvider>*//*!*/ TypeExtensionTable = new TrivialHashtable();
        delegate TypeNode TypeExtensionProvider(TypeNode.NestedTypeProvider nprovider, TypeNode.TypeAttributeProvider aprovider, TypeNode.TypeMemberProvider mprovider, TypeNode baseType, object handle);

        private static TypeNode DummyTypeExtensionProvider(TypeNode.NestedTypeProvider nprovider, TypeNode.TypeAttributeProvider aprovider, TypeNode.TypeMemberProvider mprovider, TypeNode baseType, object handle)
        {
            return null;
        }
        private TypeExtensionProvider/*!*/ dummyTEProvider = new TypeExtensionProvider(DummyTypeExtensionProvider);

        private TypeNode GetTypeExtensionFromDef(TypeNode.NestedTypeProvider nestedTypeProvider, TypeNode.TypeAttributeProvider attributeProvider, TypeNode.TypeMemberProvider memberProvider, object handle, TypeNode baseType, Interface/*!*/ lastInterface)
        {
            if (lastInterface.Namespace.UniqueIdKey == StandardIds.CciTypeExtensions.UniqueIdKey)
            {
                TypeExtensionProvider teprovider = (TypeExtensionProvider)TypeExtensionTable[lastInterface.Name.UniqueIdKey];
                if (teprovider == null)
                {
                    string loc = lastInterface.DeclaringModule.Location.ToLower(CultureInfo.InvariantCulture);
                    if (loc.EndsWith(".runtime.dll"))
                    {
                        loc = System.IO.Path.GetFileName(loc);
                        string compilerDllName = loc.Replace(".runtime.dll", "");
                        System.Reflection.Assembly rassem;
                        try
                        {
                            rassem = System.Reflection.Assembly.Load(compilerDllName);
                        }
                        catch
                        {
                            HandleError(this.module, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.CannotLoadTypeExtension, lastInterface.FullName, compilerDllName));
                            goto ExtensionNotFound;
                        }
                        if (rassem == null) goto ExtensionNotFound;
                        System.Type tprov = rassem.GetType(StandardIds.CciTypeExtensions.Name + "." + lastInterface.Name.Name + "Provider");
                        if (tprov == null) goto ExtensionNotFound;
                        System.Reflection.MethodInfo providerMethod = tprov.GetMethod("For");
                        if (providerMethod == null) goto ExtensionNotFound;
                        teprovider = (TypeExtensionProvider)Delegate.CreateDelegate(typeof(TypeExtensionProvider), providerMethod);
                    ExtensionNotFound: ;
                        if (teprovider == null)
                        {
                            // install a not-found dummy provider
                            teprovider = this.dummyTEProvider;
                        }
                        TypeExtensionTable[lastInterface.Name.UniqueIdKey] = teprovider;
                    }
                }
                if (teprovider == null) return null;
                return teprovider(nestedTypeProvider, attributeProvider, memberProvider, baseType, handle);
            }
            return null;
        }

        private static int GetInheritedTypeParameterCount(TypeNode type)
        {
            if (type == null) return 0;
            int n = 0;
            type = type.DeclaringType;
            while (type != null)
            {
                n += type.templateParameters == null ? 0 : type.templateParameters.Count;
                type = type.DeclaringType;
            }
            return n;
        }

        private TypeNode/*!*/ GetTypeGlobalMemberContainerTypeFromModule(int i)
        {
            ModuleRefRow mr = this.tables.ModuleRefTable[i - 1];
            Module mod = mr.Module;
            TypeNode result = null;

            if(mod != null && mod.Types != null && mod.Types.Count > 0)
                result = mod.Types[0];

            if(result != null)
                return result;

            result = this.GetDummyTypeNode(Identifier.Empty, Identifier.For("<Module>"), mod, null, false);

            if(mod != null)
                mod.Types = new TypeNodeList(new[] { result });

            return result;
        }

        internal void GetNamespaces()
        //^ ensures this.namespaceTable != null;
        {
            TypeDefRow[] typeDefs = this.tables.TypeDefTable;
            int n = typeDefs.Length;
            TrivialHashtable nsT = this.namespaceTable = new TrivialHashtable(n * 2);
            TrivialHashtable nsFor = new TrivialHashtable();
            NamespaceList nsL = this.namespaceList = new NamespaceList();

            for(int i = 0; i < n; i++)
            {
                TypeDefRow typeDef = typeDefs[i];
                TrivialHashtable ns = (TrivialHashtable)nsT[typeDef.NamespaceKey];
                Namespace nSpace = (Namespace)nsFor[typeDef.NamespaceKey];

                if(ns == null)
                {
                    nsT[typeDef.NamespaceKey] = ns = new TrivialHashtable();
                    nsFor[typeDef.NamespaceKey] = nSpace = new Namespace(typeDef.NamespaceId);
                    nsL.Add(nSpace);
                }

                Debug.Assert(nSpace != null);

                if((typeDef.Flags & (int)TypeFlags.VisibilityMask) == 0)
                    ns[typeDef.NameKey] = i + 1;
                else
                    if((typeDef.Flags & (int)TypeFlags.VisibilityMask) == 1)
                    {
                        nSpace.isPublic = true;
                        ns[typeDef.NameKey] = i + 1;
                    }
            }
        }

        private TypeNode GetTypeFromName(Identifier/*!*/ Namespace, Identifier/*!*/ name)
        {
            try
            {
                if (this.namespaceTable == null) this.GetNamespaces();
                //^ assert this.namespaceTable != null;
                TrivialHashtable nsTable = (TrivialHashtable)this.namespaceTable[Namespace.UniqueIdKey];
                if (nsTable == null) return this.GetForwardedTypeFromName(Namespace, name);
                object ti = nsTable[name.UniqueIdKey];
                if (ti == null) return this.GetForwardedTypeFromName(Namespace, name);
                TypeNode t = this.GetTypeFromDef((int)ti);
                return t;
            }
            catch (Exception e)
            {
                if (this.module == null) return null;
                if (this.module.MetadataImportErrors == null) this.module.MetadataImportErrors = new ArrayList();
                this.module.MetadataImportErrors.Add(e);
                return null;
            }
        }
        private TypeNode GetForwardedTypeFromName(Identifier/*!*/ Namespace, Identifier/*!*/ name)
        {
            ExportedTypeRow[] exportedTypes = this.tables.ExportedTypeTable;
            for (int i = 0, n = exportedTypes == null ? 0 : exportedTypes.Length; i < n; i++)
            {
                ExportedTypeRow etr = exportedTypes[i];
                if ((etr.Flags & (int)TypeFlags.Forwarder) == 0) continue;
                if (this.tables.GetString(etr.TypeNamespace) != Namespace.Name ||
                    this.tables.GetString(etr.TypeName) != name.Name) continue;
                int index = etr.Implementation >> 2;
                AssemblyRefRow arr = this.tables.AssemblyRefTable[index - 1];
                return arr.AssemblyReference.Assembly.GetType(Namespace, name);
            }
            return null;
        }
        internal bool IsValidTypeName(Identifier/*!*/ Namespace, Identifier/*!*/ name)
        {
            try
            {
                if (this.namespaceTable == null) this.GetNamespaces();
                //^ assert this.namespaceTable != null;
                TrivialHashtable nsTable = (TrivialHashtable)this.namespaceTable[Namespace.UniqueIdKey];
                if (nsTable == null) return false;
                return nsTable[name.UniqueIdKey] != null;
            }
            catch (Exception e)
            {
                if (this.module == null) return false;
                if (this.module.MetadataImportErrors == null) this.module.MetadataImportErrors = new ArrayList();
                this.module.MetadataImportErrors.Add(e);
                return false;
            }
        }
        internal TypeNode/*!*/ GetTypeFromRef(int i)
        {
            return this.GetTypeFromRef(i, false);
        }
        internal TypeNode/*!*/ GetTypeFromRef(int i, bool expectStruct)
        {
            TypeRefRow[] trtable = this.tables.TypeRefTable;
            TypeRefRow trr = trtable[i - 1];
            TypeNode result = trr.Type;
            if (result != null) return result;
            Identifier name = tables.GetIdentifier(trr.Name);
            Identifier namesp = tables.GetIdentifier(trr.Namespace);
            int resolutionScope = trr.ResolutionScope;
            Module declaringModule = null;
            TypeNode declaringType = null;
            int index = resolutionScope >> 2;
            switch (resolutionScope & 0x3)
            {
                case 0:
                    declaringModule = this.module;
                    //^ assume declaringModule != null;
                    result = declaringModule.GetType(namesp, name);
                    //REVIEW: deal with case where ref is in same (multi-module) assembly, but not the current module? index == 0
                    break;
                case 1:
                    declaringModule = this.tables.ModuleRefTable[index - 1].Module;
                    if (declaringModule != null)
                        result = declaringModule.GetType(namesp, name);
                    break;
                case 2:
                    declaringModule = this.tables.AssemblyRefTable[index - 1].AssemblyReference.Assembly;
                    if (declaringModule != null)
                        result = declaringModule.GetType(namesp, name);
                    break;
                case 3:
                    declaringType = this.GetTypeFromRef(index);
                    declaringModule = declaringType.DeclaringModule;
                    if (namesp == null || namesp.length == 0)
                        result = (TypeNode)declaringType.GetMembersNamed(name).FirstOrDefault();
                    else
                        result = (TypeNode)declaringType.GetMembersNamed(Identifier.For(namesp.Name + "." + name.Name)).FirstOrDefault();
                    break;
                default:
                    declaringModule = this.module;
                    break;
            }
            if (result == null)
                result = this.GetDummyTypeNode(namesp, name, declaringModule, declaringType, expectStruct);
            trtable[i - 1].Type = result;
            if (!Reader.CanCacheTypeNode(result))
                trtable[i - 1].Type = null;
            return result;
        }
        private TypeNode/*!*/ GetDummyTypeNode(Identifier namesp, Identifier name, Module declaringModule, TypeNode declaringType, bool expectStruct)
        {
            TypeNode result = null;
            if (this.module != null)
            {
                string modName = declaringModule == null ? "" : declaringModule.Name == null ? "" : declaringModule.Name.ToString();
                HandleError(this.module, String.Format(CultureInfo.CurrentCulture, ExceptionStrings.CouldNotResolveTypeReference,
                  "[" + modName + "]" + namesp + "." + name));
            }
            result = expectStruct ? (TypeNode)new Struct() : (TypeNode)new Class();
            if (name != null && name.ToString().StartsWith("I") && name.ToString().Length > 1 && char.IsUpper(name.ToString()[1]))
                result = new Interface();
            result.Flags |= TypeFlags.Public;
            result.Name = name;
            result.Namespace = namesp;
            if (declaringType != null)
            {
                result.DeclaringType = declaringType;
                result.DeclaringModule = declaringType.DeclaringModule;
                declaringType.Members.Add(result);
            }
            else
            {
                if (declaringModule == null) declaringModule = this.module;
                //^ assume declaringModule != null;
                result.DeclaringModule = declaringModule;
                if (declaringModule.types != null)
                    declaringModule.types.Add(result);
            }
            return result;
        }
        private bool TypeSpecIsClass(int i)
        {
            TypeSpecRow tsr = this.tables.TypeSpecTable[i - 1];
            if (tsr.Type != null) return tsr.Type is Class;
            this.tables.GetSignatureLength(tsr.Signature);
            return this.TypeSignatureIsClass(this.tables.GetNewCursor());
        }
        internal TypeNode/*!*/ GetTypeFromSpec(int i)
        {
            TypeSpecRow tsr = this.tables.TypeSpecTable[i - 1];
            if (tsr.Type != null) return tsr.Type;
            this.tables.GetSignatureLength(tsr.Signature);
            bool pinned = false;
            bool isTypeArgument = false;
            TypeNode result = this.ParseTypeSignature(this.tables.GetNewCursor(), ref pinned, ref isTypeArgument);
            if (result == null) result = new Class();
            //Get custom attributes
            AttributeList attributes = this.GetCustomAttributesFor((i << 5) | 13);
            if (attributes.Count > 0)
            {
                //Append attributes "inherited" from template to metadata attributes
                AttributeList templAttributes = result.Attributes;
                for (int j = 0, n = templAttributes == null ? 0 : templAttributes.Count; j < n; j++)
                {
                    AttributeNode attr = result.Attributes[j];
                    if (attr == null) continue;
                    attributes.Add(attr);
                }
                result.Attributes = attributes;
            }
            if (!isTypeArgument && Reader.CanCacheTypeNode(result))
                this.tables.TypeSpecTable[i - 1].Type = result;
            return result;
        }
        private static bool CanCacheTypeNode(TypeNode/*!*/ type)
        {
            return !type.IsGeneric && (type.Template == null || !type.IsNotFullySpecialized) &&
                type.NodeType != NodeType.TypeParameter && type.NodeType != NodeType.ClassParameter &&
                type.NodeType != NodeType.InterfaceExpression;
        }
        private static Module GetNestedModule(Module module, string modName, ref string modLocation)
        {
            if (module == null || modName == null) { Debug.Assert(false); return null; }
            Module mod = module.GetNestedModule(modName);
            if (mod == null)
            {
                if (module.Location != null)
                    modLocation = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(module.Location), modName);
                if (modLocation != null && System.IO.File.Exists(modLocation))
                {
                    mod = Module.GetModule(modLocation);
                    if (mod != null)
                    {
                        mod.ContainingAssembly = module.ContainingAssembly;
                        module.ModuleReferences.Add(new ModuleReference(modName, mod));
                    }
                }
            }
            if (mod == null)
            {
                HandleError(module, String.Format(CultureInfo.CurrentCulture,
                  ExceptionStrings.CouldNotFindReferencedModule, modLocation));
                mod = new Module();
                mod.Name = modName;
                mod.ContainingAssembly = module.ContainingAssembly;
                mod.Kind = ModuleKind.DynamicallyLinkedLibrary;
            }
            return mod;
        }
        private void GetTypeList(Module/*!*/ module)
        {
            TypeNodeList types = new TypeNodeList();
            TypeDefRow[] typeDefs = this.tables.TypeDefTable;
            for (int i = 0, n = typeDefs.Length; i < n; i++)
            {
                TypeNode t = this.GetTypeFromDef(i + 1);
                if (t != null && t.DeclaringType == null) types.Add(t);
            }
            module.Types = types;
            AssemblyNode assem = module as AssemblyNode;
            if (assem == null) return;
            types = new TypeNodeList();
            ExportedTypeRow[] exportedTypes = this.tables.ExportedTypeTable;
            for (int i = 0, n = exportedTypes.Length; i < n; i++)
            {
                ExportedTypeRow etr = exportedTypes[i];
                Identifier nameSpace = Identifier.For(this.tables.GetString(etr.TypeNamespace));
                Identifier typeName = Identifier.For(this.tables.GetString(etr.TypeName));
                TypeNode exportedType = null;
                switch (etr.Implementation & 0x3)
                {
                    case 0:
                        string modName = this.tables.GetString(this.tables.FileTable[(etr.Implementation >> 2) - 1].Name);
                        string modLocation = modName;
                        Module mod = GetNestedModule(assem, modName, ref modLocation);
                        if (mod == null) { Debug.Assert(false); break; }
                        exportedType = mod.GetType(nameSpace, typeName);
                        if (exportedType == null)
                        {
                            HandleError(assem, String.Format(CultureInfo.CurrentCulture,
                              ExceptionStrings.CouldNotFindExportedTypeInModule, nameSpace + "." + typeName, modLocation));
                            exportedType = new Class();
                            exportedType.Name = typeName;
                            exportedType.Namespace = nameSpace;
                            exportedType.Flags = TypeFlags.Class | TypeFlags.Public;
                            exportedType.DeclaringModule = mod;
                        }
                        break;
                    case 1:
                        AssemblyReference aref = this.tables.AssemblyRefTable[(etr.Implementation >> 2) - 1].AssemblyReference;
                        if (aref == null)
                        {
                            HandleError(assem, ExceptionStrings.BadMetadataInExportTypeTableNoSuchAssemblyReference);
                            aref = new AssemblyReference("dummy assembly for bad reference");
                        }
                        AssemblyNode a = aref.Assembly;
                        if (a == null) { Debug.Assert(false); continue; }
                        exportedType = a.GetType(nameSpace, typeName);
                        if (exportedType == null)
                        {
                            HandleError(assem, String.Format(CultureInfo.CurrentCulture,
                              ExceptionStrings.CouldNotFindExportedTypeInAssembly, nameSpace + "." + typeName, a.StrongName));
                            exportedType = new Class();
                            exportedType.Name = typeName;
                            exportedType.Namespace = nameSpace;
                            exportedType.Flags = TypeFlags.Class | TypeFlags.Public;
                            exportedType.DeclaringModule = a;
                        }
                        break;
                    case 2:
                        TypeNode parentType = types[(etr.Implementation >> 2) - 1];
                        if (parentType == null)
                        {
                            HandleError(assem, ExceptionStrings.BadMetadataInExportTypeTableNoSuchParentType);
                            parentType = new Class();
                            parentType.DeclaringModule = this.module;
                            parentType.Name = Identifier.For("Missing parent type");
                        }
                        exportedType = parentType.GetNestedType(typeName);
                        if (exportedType == null)
                        {
                            HandleError(assem, String.Format(CultureInfo.CurrentCulture,
                              ExceptionStrings.CouldNotFindExportedNestedTypeInType, typeName, parentType.FullName));
                            exportedType = new Class();
                            exportedType.Name = typeName;
                            exportedType.Flags = TypeFlags.Class | TypeFlags.NestedPublic;
                            exportedType.DeclaringType = parentType;
                            exportedType.DeclaringModule = parentType.DeclaringModule;
                        }
                        break;
                }
                types.Add(exportedType);
            }
            assem.ExportedTypes = types;
        }

        private void GetNestedTypes(TypeNode/*!*/ type, object/*!*/ handle)
        {
            type.nestedTypes = null;
            TypeNodeList result = new TypeNodeList();
            TypeNodeList savedCurrentTypeParameters = this.currentTypeParameters;

            try
            {
                if(type.IsGeneric)
                {
                    if(type.templateParameters == null)
                        type.templateParameters = new TypeNodeList();

                    this.currentTypeParameters = type.ConsolidatedTemplateParameters;
                }

                this.currentType = type;
                TypeNode declaringType = type.DeclaringType;

                while(this.currentTypeParameters == null && declaringType != null)
                {
                    if(declaringType.IsGeneric)
                    {
                        if(declaringType.templateParameters == null)
                            declaringType.templateParameters = new TypeNodeList();

                        this.currentTypeParameters = declaringType.ConsolidatedTemplateParameters;
                    }

                    declaringType = declaringType.DeclaringType;
                }

                MetadataReader tables = this.tables;
                int typeTableIndex = (int)handle;
                TypeDefRow[] typeDefs = tables.TypeDefTable;
                int n = typeDefs.Length;

                if(typeTableIndex < 1 || typeTableIndex > n)
                    throw new System.ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);

                NestedClassRow[] nestedClasses = tables.NestedClassTable;
                n = nestedClasses.Length;

                for (int i = 0; i < n; i++)
                { //TODO: binary lookup
                    NestedClassRow ncr = nestedClasses[i];

                    if(ncr.EnclosingClass != typeTableIndex)
                        continue;

                    TypeNode t = this.GetTypeFromDef(ncr.NestedClass);

                    if(t != null)
                    {
                        if(type.nestedTypes != null)
                            return; //A recursive call to GetNestedTypes has already done the deed

                        t.DeclaringType = type;

                        if((t.Flags & TypeFlags.RTSpecialName) == 0 || t.Name.UniqueIdKey != StandardIds._Deleted.UniqueIdKey)
                            result.Add(t);
                    }
                    else
                    {
                        throw new InvalidMetadataException("Invalid nested class row");
                    }
                }

                type.nestedTypes = result;
            }
            catch(Exception e)
            {
                if(this.module != null)
                {
                    if(this.module.MetadataImportErrors == null)
                        this.module.MetadataImportErrors = new ArrayList();

                    this.module.MetadataImportErrors.Add(e);
                }

                this.currentTypeParameters = savedCurrentTypeParameters;
            }
        }

        private void GetTypeMembers(TypeNode/*!*/ type, object/*!*/ handle)
        {
            TypeNodeList savedCurrentTypeParameters = this.currentTypeParameters;

            try
            {
                MetadataReader tables = this.tables;
                int typeTableIndex = (int)handle;
                TypeDefRow[] typeDefs = tables.TypeDefTable;
                FieldRow[] fieldDefs = tables.FieldTable;
                FieldPtrRow[] fieldPtrs = tables.FieldPtrTable;
                MethodRow[] methodDefs = tables.MethodTable;
                MethodPtrRow[] methodPtrs = tables.MethodPtrTable;
                EventMapRow[] eventMaps = tables.EventMapTable;
                EventRow[] eventDefs = tables.EventTable;
                EventPtrRow[] eventPtrs = tables.EventPtrTable;
                MethodImplRow[] methodImpls = tables.MethodImplTable;
                PropertyMapRow[] propertyMaps = tables.PropertyMapTable;
                PropertyPtrRow[] propertyPtrs = tables.PropertyPtrTable;
                PropertyRow[] propertyDefs = this.tables.PropertyTable;
                NestedClassRow[] nestedClasses = tables.NestedClassTable;
                int n = typeDefs.Length;

                if(typeTableIndex < 1 || typeTableIndex > n)
                    throw new System.ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);

                TypeDefRow td = typeDefs[typeTableIndex - 1];

                if(type != td.Type)
                    throw new System.ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);

                //Get type members
                if(type.IsGeneric)
                {
                    if(type.templateParameters == null)
                        type.templateParameters = new TypeNodeList();

                    this.currentTypeParameters = type.ConsolidatedTemplateParameters;
                }

                this.currentType = type;
                TypeNode declaringType = type.DeclaringType;

                while(this.currentTypeParameters == null && declaringType != null)
                {
                    if(declaringType.IsGeneric)
                    {
                        if(declaringType.templateParameters == null)
                            declaringType.templateParameters = new TypeNodeList();

                        this.currentTypeParameters = declaringType.ConsolidatedTemplateParameters;
                    }

                    declaringType = declaringType.DeclaringType;
                }

                type.members = new MemberList();
                n = nestedClasses.Length;

                for(int i = 0; i < n; i++)
                {
                    NestedClassRow ncr = nestedClasses[i];

                    if(ncr.EnclosingClass != typeTableIndex)
                        continue;

                    TypeNode t = this.GetTypeFromDef(ncr.NestedClass);

                    if(t != null)
                    {
                        t.DeclaringType = type;

                        if((t.Flags & TypeFlags.RTSpecialName) == 0 || t.Name.UniqueIdKey != StandardIds._Deleted.UniqueIdKey)
                            type.Members.Add(t);
                    }
                }

                n = typeDefs.Length;
                int m = fieldDefs.Length;
                int start = td.FieldList;
                int end = m + 1; if (typeTableIndex < n) end = typeDefs[typeTableIndex].FieldList;

                if(type is EnumNode)
                    this.GetUnderlyingTypeOfEnumNode((EnumNode)type, fieldDefs, fieldPtrs, start, end);

                this.AddFieldsToType(type, fieldDefs, fieldPtrs, start, end);
                m = methodDefs.Length;
                start = td.MethodList;
                end = m + 1; if (typeTableIndex < n) end = typeDefs[typeTableIndex].MethodList;
                this.AddMethodsToType(type, methodPtrs, start, end);
                n = propertyMaps.Length;
                m = propertyDefs.Length;

                for(int i = 0; i < n; i++)
                { //TODO: binary search
                    PropertyMapRow pm = propertyMaps[i];

                    if(pm.Parent != typeTableIndex)
                        continue;

                    start = pm.PropertyList;
                    end = m + 1; if (i < n - 1) end = propertyMaps[i + 1].PropertyList;

                    this.AddPropertiesToType(type, propertyDefs, propertyPtrs, start, end);
                }

                n = eventMaps.Length;
                m = eventDefs.Length;

                for(int i = 0; i < n; i++)
                { //TODO: binary search
                    EventMapRow em = eventMaps[i];

                    if(em.Parent != typeTableIndex)
                        continue;

                    start = em.EventList;
                    end = m + 1; if (i < n - 1) end = eventMaps[i + 1].EventList;

                    this.AddEventsToType(type, eventDefs, eventPtrs, start, end);
                }

                n = methodImpls.Length;

                for(int i = 0; i < n; i++)
                { //TODO: binary search
                    MethodImplRow mir = methodImpls[i];

                    if(mir.Class != typeTableIndex)
                        continue;

                    Method implementer = this.GetMethodDefOrRef(mir.MethodBody);

                    if(implementer == null)
                        continue;

                    MethodList implementedInterfaceMethods = implementer.ImplementedInterfaceMethods;

                    if(implementedInterfaceMethods == null)
                        implementedInterfaceMethods = implementer.ImplementedInterfaceMethods = new MethodList();

                    TypeNodeList savedMethodTypeParameters = this.currentMethodTypeParameters;
                    this.currentMethodTypeParameters = implementer.TemplateParameters;
                    implementedInterfaceMethods.Add(this.GetMethodDefOrRef(mir.MethodDeclaration));
                    this.currentMethodTypeParameters = savedMethodTypeParameters;
                }

                this.currentTypeParameters = savedCurrentTypeParameters;
            }
            catch(Exception e)
            {
                if(this.module != null)
                {
                    if(this.module.MetadataImportErrors == null)
                        this.module.MetadataImportErrors = new ArrayList();

                    this.module.MetadataImportErrors.Add(e);
                }

                type.Members = new MemberList();

                this.currentTypeParameters = savedCurrentTypeParameters;
            }
        }

        private void GetTypeAttributes(TypeNode/*!*/ type, object/*!*/ handle)
        {
            TypeNodeList savedCurrentTypeParameters = this.currentTypeParameters;
            try
            {
                MetadataReader tables = this.tables;
                int typeTableIndex = (int)handle;
                TypeDefRow[] typeDefs = tables.TypeDefTable;
                int n = typeDefs.Length;
                if (typeTableIndex < 1 || typeTableIndex > n)
                    throw new System.ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                TypeDefRow td = typeDefs[typeTableIndex - 1];
                if (type != td.Type) throw new System.ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                //Get custom attributes   
                type.Attributes = this.GetCustomAttributesFor((typeTableIndex << 5) | 3);
                this.currentTypeParameters = savedCurrentTypeParameters;
                //Get security attributes
                if ((type.Flags & TypeFlags.HasSecurity) != 0)
                    type.SecurityAttributes = this.GetSecurityAttributesFor((typeTableIndex << 2) | 0);
            }
            catch(Exception e)
            {
                if(this.module != null)
                {
                    if(this.module.MetadataImportErrors == null)
                        this.module.MetadataImportErrors = new ArrayList();

                    this.module.MetadataImportErrors.Add(e);
                }

                type.Attributes = new AttributeList();
                this.currentTypeParameters = savedCurrentTypeParameters;
            }
        }

        private TypeNodeList/*!*/ ParseTypeList(MemoryCursor/*!*/ sigReader)
        {
            int n = sigReader.ReadCompressedInt();
            TypeNodeList result = new TypeNodeList();

            for(int i = 0; i < n; i++)
            {
                TypeNode t = this.ParseTypeSignature(sigReader);

                if(t == null || t == Struct.Dummy)
                {
                    // Can happen when dealing with a primitive type that implements an interface that references the primitive type.
                    // For example, System.String implements IComparable<System.String>.
                    if(this.currentType != null && !CoreSystemTypes.Initialized)
                        t = this.currentType;
                    else
                    {
                        Debug.Assert(false);
                        t = new TypeParameter();
                        t.Name = Identifier.For("Bad type parameter in position " + i);
                        t.DeclaringModule = this.module;
                    }
                }

                result.Add(t);
            }

            return result;
        }

        private bool TypeSignatureIsClass(MemoryCursor/*!*/ sigReader)
        {
            ElementType tok = (ElementType)sigReader.ReadCompressedInt();
            switch (tok)
            {
                case ElementType.Pinned:
                case ElementType.Pointer:
                case ElementType.Reference:
                    return this.TypeSignatureIsClass(sigReader);
                case ElementType.OptionalModifier:
                case ElementType.RequiredModifier:
                    sigReader.ReadCompressedInt();
                    return this.TypeSignatureIsClass(sigReader);
                case ElementType.Class:
                    return true;
                case ElementType.GenericTypeInstance:
                    return this.TypeSignatureIsClass(sigReader);
                case ElementType.TypeParameter:
                    {
                        int pnum = sigReader.ReadCompressedInt();
                        if (this.currentTypeParameters != null && this.currentTypeParameters.Count > pnum)
                        {
                            TypeNode tPar = this.currentTypeParameters[pnum];
                            return tPar != null && tPar is Class;
                        }
                        return false;
                    }
                case ElementType.MethodParameter:
                    {
                        int pnum = sigReader.ReadCompressedInt();
                        if (this.currentMethodTypeParameters != null && this.currentMethodTypeParameters.Count > pnum)
                        {
                            TypeNode tPar = this.currentMethodTypeParameters[pnum];
                            return tPar != null && tPar is Class;
                        }
                        return false;
                    }
                default:
                    return false;
            }
        }
        private TypeNode ParseTypeSignature(MemoryCursor/*!*/ sigReader)
        {
            bool junk = false;
            return this.ParseTypeSignature(sigReader, ref junk, ref junk);
        }
        private TypeNode ParseTypeSignature(MemoryCursor/*!*/ sigReader, ref bool pinned)
        {
            bool junk = false;
            return this.ParseTypeSignature(sigReader, ref pinned, ref junk);
        }
        private TypeNode ParseTypeSignature(MemoryCursor/*!*/ sigReader, ref bool pinned, ref bool isTypeArgument)
        {
            TypeNode elementType;
            ElementType tok = (ElementType)sigReader.ReadCompressedInt();
            if (tok == ElementType.Pinned)
            {
                pinned = true;
                tok = (ElementType)sigReader.ReadCompressedInt();
            }
            switch (tok)
            {
                case ElementType.Boolean: return CoreSystemTypes.Boolean;
                case ElementType.Char: return CoreSystemTypes.Char;
                case ElementType.Double: return CoreSystemTypes.Double;
                case ElementType.Int16: return CoreSystemTypes.Int16;
                case ElementType.Int32: return CoreSystemTypes.Int32;
                case ElementType.Int64: return CoreSystemTypes.Int64;
                case ElementType.Int8: return CoreSystemTypes.Int8;
                case ElementType.IntPtr: return CoreSystemTypes.IntPtr;
                case ElementType.BoxedEnum:
                case ElementType.Object: return CoreSystemTypes.Object;
                case ElementType.Single: return CoreSystemTypes.Single;
                case ElementType.String: return CoreSystemTypes.String;
                case ElementType.DynamicallyTypedReference: return CoreSystemTypes.DynamicallyTypedReference;
                case ElementType.UInt16: return CoreSystemTypes.UInt16;
                case ElementType.UInt32: return CoreSystemTypes.UInt32;
                case ElementType.UInt64: return CoreSystemTypes.UInt64;
                case ElementType.UInt8: return CoreSystemTypes.UInt8;
                case ElementType.UIntPtr: return CoreSystemTypes.UIntPtr;
                case ElementType.Void: return CoreSystemTypes.Void;
                case ElementType.Pointer:
                    elementType = this.ParseTypeSignature(sigReader, ref pinned);
                    if (elementType == null) elementType = CoreSystemTypes.Object;
                    if (elementType == null) return null;
                    return elementType.GetPointerType();
                case ElementType.Reference:
                    elementType = this.ParseTypeSignature(sigReader, ref pinned);
                    if (elementType == null) elementType = CoreSystemTypes.Object;
                    return elementType.GetReferenceType();
                case ElementType.FunctionPointer:
                    return this.ParseFunctionPointer(sigReader);
                case ElementType.OptionalModifier:
                case ElementType.RequiredModifier:
                    TypeNode modifier = this.DecodeAndGetTypeDefOrRefOrSpec(sigReader.ReadCompressedInt());
                    if (modifier == null) modifier = CoreSystemTypes.Object;
                    TypeNode modified = this.ParseTypeSignature(sigReader, ref pinned);
                    if (modified == null) modified = CoreSystemTypes.Object;
                    if (modified == null || modified == null) return null;
                    if (tok == ElementType.RequiredModifier)
                        return RequiredModifier.For(modifier, modified);
                    else
                        return OptionalModifier.For(modifier, modified);
                case ElementType.Class:
                    return this.DecodeAndGetTypeDefOrRefOrSpec(sigReader.ReadCompressedInt());
                case ElementType.ValueType:
                    return this.DecodeAndGetTypeDefOrRefOrSpec(sigReader.ReadCompressedInt(), true);
                case ElementType.TypeParameter:
                    TypeNode tPar = null;
                    int pnum = sigReader.ReadCompressedInt();
                    if (this.currentTypeParameters != null && this.currentTypeParameters.Count > pnum)
                        tPar = this.currentTypeParameters[pnum];
                    if (tPar == null)
                    {
                        HandleError(this.module, String.Format(CultureInfo.CurrentCulture,
                            ExceptionStrings.BadTypeParameterInPositionForType, pnum, this.currentType == null ? "" : this.currentType.FullName));
                        tPar = new TypeParameter();
                        tPar.Name = Identifier.For("Bad type parameter in position " + pnum);
                        tPar.DeclaringModule = this.module;
                    }
                    isTypeArgument = true;
                    return tPar;
                case ElementType.MethodParameter:
                    TypeNode mTPar = null;
                    pnum = sigReader.ReadCompressedInt();
                    if (this.currentMethodTypeParameters != null && this.currentMethodTypeParameters.Count > pnum)
                        mTPar = this.currentMethodTypeParameters[pnum];
                    if (mTPar == null)
                    {
                        HandleError(this.module, String.Format(CultureInfo.CurrentCulture,
                            ExceptionStrings.BadMethodTypeParameterInPosition, pnum));
                        mTPar = new MethodTypeParameter();
                        mTPar.Name = Identifier.For("Bad method type parameter in position " + pnum);
                    }
                    isTypeArgument = true;
                    return mTPar;
                case ElementType.GenericTypeInstance:
                    TypeNodeList savedCurrentTypeParameters = this.currentTypeParameters;
                    TypeNode template = this.ParseTypeSignature(sigReader, ref pinned);
                    this.currentTypeParameters = savedCurrentTypeParameters;
                    if (template == null || template.ConsolidatedTemplateParameters == null) return template; //Likely a dummy type
                    if (CoreSystemTypes.Initialized)
                    {
                        if (this.currentTypeParameters == null || this.currentTypeParameters.Count == 0)
                            this.currentTypeParameters = template.ConsolidatedTemplateParameters;
                        TypeNodeList genArgs = this.ParseTypeList(sigReader);
                        if (this.module == null) return null;
                        TypeNode genInst = template.GetGenericTemplateInstance(this.module, genArgs);
                        this.currentTypeParameters = savedCurrentTypeParameters;
                        return genInst;
                    }
                    InterfaceExpression ifaceExpr = new InterfaceExpression(null);
                    ifaceExpr.Template = template;
                    ifaceExpr.Namespace = template.Namespace;
                    ifaceExpr.Name = template.Name;
                    ifaceExpr.TemplateArguments = this.ParseTypeList(sigReader);
                    this.currentTypeParameters = savedCurrentTypeParameters;
                    return ifaceExpr;
                case ElementType.SzArray:
                    elementType = this.ParseTypeSignature(sigReader, ref pinned);
                    if (elementType == null) elementType = CoreSystemTypes.Object;
                    if (elementType == null) return null;
                    return elementType.GetArrayType(1);
                case ElementType.Array:
                    elementType = this.ParseTypeSignature(sigReader, ref pinned);
                    if (elementType == null) elementType = CoreSystemTypes.Object;
                    if (elementType == null) return null;
                    int rank = sigReader.ReadCompressedInt();
                    int numSizes = sigReader.ReadCompressedInt();
                    int[] sizes = new int[numSizes];
                    for (int i = 0; i < numSizes; i++) sizes[i] = sigReader.ReadCompressedInt();
                    int numLoBounds = sigReader.ReadCompressedInt();
                    int[] loBounds = new int[numLoBounds];
                    for (int i = 0; i < numLoBounds; i++) loBounds[i] = sigReader.ReadCompressedInt();
                    return elementType.GetArrayType(rank, numSizes, numLoBounds, sizes, loBounds);
                case ElementType.Sentinel: return null;
                case ElementType.Type: return CoreSystemTypes.Type;
                case ElementType.Enum: return this.GetTypeFromSerializedName(ReadSerString(sigReader));
            }
            throw new InvalidMetadataException(ExceptionStrings.MalformedSignature);
        }

        private FunctionPointer/*!*/ ParseFunctionPointer(MemoryCursor/*!*/ sigReader)
        {
            CallingConventionFlags convention = (CallingConventionFlags)sigReader.ReadByte();
            int n = sigReader.ReadCompressedInt();
            TypeNode returnType = this.ParseTypeSignature(sigReader);

            if(returnType == null)
                returnType = CoreSystemTypes.Object;

            TypeNodeList parameterTypes = new TypeNodeList();
            int m = n;

            for(int i = 0; i < n; i++)
            {
                TypeNode t = this.ParseTypeSignature(sigReader);

                if (t == null)
                    m = i--;
                else
                    parameterTypes.Add(t);
            }

            FunctionPointer fp = FunctionPointer.For(parameterTypes, returnType);
            fp.CallingConvention = convention;
            fp.VarArgStart = m;

            return fp;
        }

        private StatementList ParseMethodBody(Method/*!*/ method, int methodIndex, int RVA)
        {
            TypeNodeList savedCurrentTypeParameters = this.currentTypeParameters;
            if (method.DeclaringType.Template != null)
                this.currentTypeParameters = method.DeclaringType.ConsolidatedTemplateArguments;
            else
                this.currentTypeParameters = method.DeclaringType.ConsolidatedTemplateParameters;
            BodyParser parser = new BodyParser(this, method, methodIndex, RVA);
            StatementList result = parser.ParseStatements();
            this.currentTypeParameters = savedCurrentTypeParameters;
            return result;
        }
        private InstructionList ParseMethodInstructions(Method/*!*/ method, int methodIndex, int RVA)
        {
            TypeNodeList savedCurrentTypeParameters = this.currentTypeParameters;
            if (method.DeclaringType.Template != null)
                this.currentTypeParameters = method.DeclaringType.ConsolidatedTemplateArguments;
            else
                this.currentTypeParameters = method.DeclaringType.ConsolidatedTemplateParameters;
            InstructionParser parser = new InstructionParser(this, method, methodIndex, RVA);
            InstructionList result = parser.ParseInstructions();
            this.currentTypeParameters = savedCurrentTypeParameters;
            return result;
        }

        // A version of System.IO.Path.Combine that does not throw exceptions
        private static string Combine(string path1, string path2)
        {
            if(path1 == null || path1.Length == 0)
                return path2;

            if(path2 == null || path2.Length == 0)
                return path1;

            char ch = path2[0];

            if(ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar ||
              (path2.Length >= 2 && path2[1] == Path.VolumeSeparatorChar))
                return path2;

            ch = path1[path1.Length - 1];

            if(ch != Path.DirectorySeparatorChar && ch != Path.AltDirectorySeparatorChar &&
              ch != Path.VolumeSeparatorChar)
                return (path1 + Path.DirectorySeparatorChar + path2);

            return path1 + path2;
        }

        // A version of System.IO.Path.GetDirectoryName that does not throw exceptions
        private static String GetDirectoryName(string path)
        {
            if(path == null)
                return null;

            int length = path.Length;

            for(int i = length; --i >= 0; )
            {
                char ch = path[i];

                if(ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar ||
                  ch == Path.VolumeSeparatorChar)
                    return path.Substring(0, i);
            }

            return path;
        }
    }

    internal abstract class ILParser
    {
        internal int counter;
        protected Reader/*!*/ reader;
        protected MemoryCursor/*!*/ bodyReader;
        internal int size;
        protected Method/*!*/ method;
        protected int methodIndex;
        protected int RVA;
        protected LocalList/*!*/ locals = new LocalList();

        internal ILParser(Reader/*!*/ reader, Method/*!*/ method, int methodIndex, int RVA)
        {
            this.reader = reader;
            this.bodyReader = reader.tables.GetNewCursor();
            this.method = method;
            this.method.LocalList = this.locals;
            this.methodIndex = methodIndex;
            this.RVA = RVA;
            //^ base();
        }
        protected Expression Parameters(int i)
        {
            if (this.method.IsStatic) return this.method.Parameters[i];
            if (i == 0) return this.method.ThisParameter;
            return this.method.Parameters[i - 1];
        }
        protected void ParseHeader()
        {
            byte header = this.reader.tables.GetMethodBodyHeaderByte(this.RVA);
            if ((header & 0x3) == 2)
            {
                this.size = header >> 2;
                this.bodyReader = this.reader.tables.GetNewCursor();
                this.reader.tables.Skip(size);
            }
            else
            {
                method.InitLocals = (header & 0x10) != 0;
                byte header2 = this.reader.tables.GetByte();
                int fatHeaderSize = header2 >> 4;
                if (fatHeaderSize == 2) return;
                if (fatHeaderSize != 3) throw new InvalidMetadataException(ExceptionStrings.InvalidFatMethodHeader);
                this.reader.tables.Skip(2); //Skip over maxstack. No need to remember it.
                this.size = this.reader.tables.GetInt32();
                int localIndex = this.reader.tables.GetInt32();
                this.bodyReader = this.reader.tables.GetNewCursor();
                this.reader.tables.Skip(size);
                this.reader.tables.AlignTo32BitBoundary();
                while ((header & 0x8) != 0)
                {
                    header = this.reader.tables.GetByte();
                    if ((header & 3) != 1) throw new InvalidMetadataException(ExceptionStrings.BadMethodHeaderSection);
                    if ((header & 0x80) != 0) throw new InvalidMetadataException(ExceptionStrings.TooManyMethodHeaderSections);
                    this.ParseExceptionHandlerEntry((header & 0x40) == 0);
                }

                Hashtable localSourceNames = new Hashtable();

                if (this.reader.getDebugSymbols && this.reader.debugReader != null)
                {
                    ISymUnmanagedMethod methodInfo = null;
                    try
                    {
                        try
                        {
                            this.reader.debugReader.GetMethod(0x6000000 | (uint)methodIndex, ref methodInfo);
                            if (methodInfo != null)
                            {
                                ISymUnmanagedScope rootScope = methodInfo.GetRootScope();
                                try
                                {
                                    this.reader.GetLocalSourceNames(rootScope, localSourceNames);
                                }
                                finally
                                {
                                    if (rootScope != null)
                                        Marshal.ReleaseComObject(rootScope);
                                }
                            }
                        }
                        catch (COMException)
                        {
                        }
                        catch (InvalidCastException)
                        {
                        }
                        catch (System.Runtime.InteropServices.InvalidComObjectException) { }
                    }
                    finally
                    {
                        if (methodInfo != null)
                            Marshal.ReleaseComObject(methodInfo);
                    }
                }

                this.reader.GetLocals(localIndex, this.locals, localSourceNames);
            }
        }

        abstract protected void ParseExceptionHandlerEntry(bool smallSection);

        protected byte GetByte()
        {
            this.counter += 1;
            return this.bodyReader.ReadByte();
        }
        protected sbyte GetSByte()
        {
            this.counter += 1;
            return this.bodyReader.ReadSByte();
        }
        protected short GetInt16()
        {
            this.counter += 2;
            return this.bodyReader.ReadInt16();
        }
        protected int GetInt32()
        {
            this.counter += 4;
            return this.bodyReader.ReadInt32();
        }
        protected long GetInt64()
        {
            this.counter += 8;
            return this.bodyReader.ReadInt64();
        }
        protected float GetSingle()
        {
            this.counter += 4;
            return this.bodyReader.ReadSingle();
        }
        protected double GetDouble()
        {
            this.counter += 8;
            return this.bodyReader.ReadDouble();
        }
        protected Member/*!*/ GetMemberFromToken()
        {
            return this.reader.GetMemberFromToken(this.GetInt32());
        }
        protected Member/*!*/ GetMemberFromToken(out TypeNodeList varArgTypes)
        {
            return this.reader.GetMemberFromToken(this.GetInt32(), out varArgTypes);
        }
        protected string/*!*/ GetStringFromToken()
        {
            int tok = this.GetInt32();
            return this.reader.tables.GetUserString(tok & 0xFFFFFF);
        }
        protected OpCode GetOpCode()
        {
            int result = this.GetByte();
            if (result == (int)OpCode.Prefix1)
                result = result << 8 | this.GetByte();
            return (OpCode)result;
        }
    }

    sealed internal class BodyParser : ILParser
    {
        private readonly ExpressionStack/*!*/ operandStack = new ExpressionStack();
        private readonly TrivialHashtable/*!*/ blockMap = new TrivialHashtable();
        private int alignment = -1;
        private bool isReadOnly;
        private bool isTailCall;
        private bool isVolatile;
        private TypeNode constraint;

        internal BodyParser(Reader/*!*/ reader, Method/*!*/ method, int methodIndex, int RVA)
            : base(reader, method, methodIndex, RVA)
        {
            //^ base;
        }

        override protected void ParseExceptionHandlerEntry(bool smallSection)
        {
            int dataSize = this.reader.tables.GetByte();
            int n = (int)(ushort)this.reader.tables.GetInt16();

            if(smallSection)
                n = dataSize / 12;
            else
                n = (dataSize + (n << 8)) / 24;

            if(n < 0)
                n = 0;

            this.method.ExceptionHandlers = new ExceptionHandlerList();

            for(int i = 0; i < n; i++)
            {
                int flags, tryOffset, tryLength, handlerOffset, handlerLength, tokenOrOffset;
                if (smallSection)
                {
                    flags = this.reader.tables.GetInt16();
                    tryOffset = this.reader.tables.GetUInt16();
                    tryLength = this.reader.tables.GetByte();
                    handlerOffset = this.reader.tables.GetUInt16();
                    handlerLength = this.reader.tables.GetByte();
                }
                else
                {
                    flags = this.reader.tables.GetInt32();
                    tryOffset = this.reader.tables.GetInt32();
                    tryLength = this.reader.tables.GetInt32();
                    handlerOffset = this.reader.tables.GetInt32();
                    handlerLength = this.reader.tables.GetInt32();
                }
                tokenOrOffset = this.reader.tables.GetInt32();
                ExceptionHandler eh = new ExceptionHandler();
                switch (flags)
                {
                    case 0x00:
                        eh.HandlerType = NodeType.Catch;
                        int pos = this.reader.tables.GetCurrentPosition();
                        eh.FilterType = (TypeNode)this.reader.GetMemberFromToken(tokenOrOffset);
                        this.reader.tables.SetCurrentPosition(pos);
                        break;
                    case 0x01:
                        eh.HandlerType = NodeType.Filter;
                        eh.FilterExpression = Reader.GetOrCreateBlock(blockMap, tokenOrOffset);
                        break;
                    case 0x02: eh.HandlerType = NodeType.Finally; break;
                    case 0x04: eh.HandlerType = NodeType.FaultHandler; break;
                    default: throw new InvalidMetadataException(ExceptionStrings.BadExceptionHandlerType);
                }
                eh.TryStartBlock = Reader.GetOrCreateBlock(this.blockMap, tryOffset);
                eh.BlockAfterTryEnd = Reader.GetOrCreateBlock(this.blockMap, tryOffset + tryLength);
                eh.HandlerStartBlock = Reader.GetOrCreateBlock(this.blockMap, handlerOffset);
                eh.BlockAfterHandlerEnd = Reader.GetOrCreateBlock(this.blockMap, handlerOffset + handlerLength);
                this.method.ExceptionHandlers.Add(eh);
            }
        }

        private AssignmentStatement/*!*/ ParseArrayElementAssignment(OpCode opCode)
        {
            Expression rhvalue = PopOperand();
            ExpressionList indexers = new ExpressionList();
            indexers.Add(PopOperand());
            Expression array = PopOperand();
            Indexer indexer = new Indexer(array, indexers);
            TypeNode t = CoreSystemTypes.Object;
            switch (opCode)
            {
                case OpCode.Stelem_I: t = CoreSystemTypes.IntPtr; break;
                case OpCode.Stelem_I1: t = CoreSystemTypes.Int8; break;
                case OpCode.Stelem_I2: t = CoreSystemTypes.Int16; break;
                case OpCode.Stelem_I4: t = CoreSystemTypes.Int32; break;
                case OpCode.Stelem_I8: t = CoreSystemTypes.Int64; break;
                case OpCode.Stelem_R4: t = CoreSystemTypes.Single; break;
                case OpCode.Stelem_R8: t = CoreSystemTypes.Double; break;
                case OpCode.Stelem: t = (TypeNode)this.GetMemberFromToken(); break;
                default:
                    ArrayType arrT = array.Type as ArrayType;
                    if (arrT != null) t = arrT.ElementType;
                    break;
            }
            indexer.ElementType = indexer.Type = t;
            return new AssignmentStatement(indexer, rhvalue);
        }
        private Indexer/*!*/ ParseArrayElementLoad(OpCode opCode, TypeNode elementType)
        {
            ExpressionList indexers = new ExpressionList();
            indexers.Add(PopOperand());
            Expression array = PopOperand();
            Indexer indexer = new Indexer(array, indexers);
            TypeNode t = elementType;
            switch (opCode)
            {
                case OpCode.Ldelem_I1: t = CoreSystemTypes.Int8; break;
                case OpCode.Ldelem_U1: t = CoreSystemTypes.UInt8; break;
                case OpCode.Ldelem_I2: t = CoreSystemTypes.Int16; break;
                case OpCode.Ldelem_U2: t = CoreSystemTypes.UInt16; break;
                case OpCode.Ldelem_I4: t = CoreSystemTypes.Int32; break;
                case OpCode.Ldelem_U4: t = CoreSystemTypes.UInt32; break;
                case OpCode.Ldelem_I8: t = CoreSystemTypes.Int64; break;
                case OpCode.Ldelem_I: t = CoreSystemTypes.IntPtr; break;
                case OpCode.Ldelem_R4: t = CoreSystemTypes.Single; break;
                case OpCode.Ldelem_R8: t = CoreSystemTypes.Double; break;
                case OpCode.Ldelem: t = (TypeNode)this.GetMemberFromToken(); break;
                default:
                    if (t != null) break;
                    t = CoreSystemTypes.Object;
                    ArrayType arrT = array.Type as ArrayType;
                    if (arrT != null) t = arrT.ElementType;
                    break;
            }
            indexer.ElementType = indexer.Type = t;
            return indexer;
        }
        private UnaryExpression/*!*/ ParseArrayElementLoadAddress()
        {
            TypeNode elemType = (TypeNode)this.GetMemberFromToken();
            return new UnaryExpression(this.ParseArrayElementLoad(0, elemType), this.isReadOnly ? NodeType.ReadOnlyAddressOf : NodeType.AddressOf, elemType.GetReferenceType());
        }
        private static UnaryExpression/*!*/ SetType(UnaryExpression/*!*/ uex)
        {
            if (uex == null || uex.Operand == null) return uex;
            TypeNode elemType = uex.Operand.Type;
            if (elemType == null) return uex;
            uex.Type = elemType.GetReferenceType();
            return uex;
        }
        private BinaryExpression/*!*/ ParseBinaryComparison(NodeType oper)
        {
            Expression op2 = PopOperand();
            Expression op1 = PopOperand();
            BinaryExpression result = new BinaryExpression(op1, op2, oper);
            result.Type = CoreSystemTypes.Int32;
            return result;
        }
        private BinaryExpression/*!*/ ParseBinaryOperation(NodeType oper)
        {
            Expression op2 = PopOperand();
            Expression op1 = PopOperand();
            BinaryExpression result = new BinaryExpression(op1, op2, oper);
            result.Type = op1.Type;
            if (result.Type == null) result.Type = op2.Type;
            return result;
        }
        private UnaryExpression/*!*/ ParseUnaryOperation(NodeType oper)
        {
            Expression op = PopOperand();
            return new UnaryExpression(op, oper, op.Type);
        }
        private Branch/*!*/ ParseBranch(NodeType operatorType, int operandCount, bool shortOffset, bool unordered)
        {
            return this.ParseBranch(operatorType, operandCount, shortOffset, unordered, false);
        }
        private Branch/*!*/ ParseBranch(NodeType operatorType, int operandCount, bool shortOffset, bool unordered, bool leavesExceptionBlock)
        {
            Expression operand2 = operandCount > 1 ? PopOperand() : null;
            Expression operand1 = operandCount > 0 ? PopOperand() : null;
            Expression condition = operandCount > 1 ? (Expression)new BinaryExpression(operand1, operand2, operatorType) :
              (operandCount > 0 ? (operatorType == NodeType.Nop ? operand1 : (Expression)new UnaryExpression(operand1, operatorType)) : null);
            int targetAddress = shortOffset ? this.GetSByte() : this.GetInt32();
            Block targetBlock = (Block)this.blockMap[targetAddress + this.counter + 1];
            Debug.Assert(targetBlock != null);
            if (targetAddress >= 0 && !this.reader.preserveShortBranches) shortOffset = false;
            return new Branch(condition, targetBlock, shortOffset, unordered, leavesExceptionBlock);
        }
        private MethodCall/*!*/ ParseCall(NodeType typeOfCall, out bool isStatement)
        {
            TypeNodeList varArgTypes;
            Method meth = (Method)this.GetMemberFromToken(out varArgTypes);
            int numVarArgs = varArgTypes == null ? 0 : varArgTypes.Count;
            isStatement = BodyParser.TypeIsVoid(meth.ReturnType);
            int n = meth.Parameters == null ? 0 : meth.Parameters.Count;

            if(typeOfCall == NodeType.Jmp)
                n = 0;
            else
                n += numVarArgs;

            Expression[] args = new Expression[n];
            ExpressionList arguments = new ExpressionList();

            for(int i = n - 1; i >= 0; i--)
                args[i] = PopOperand();

            for(int i = 0; i < n; i++)
                arguments.Add(args[i]);

            if(varArgTypes != null)
            {
                for (int i = n - 1, j = numVarArgs; j > 0; j--, i--)
                {
                    Expression e = arguments[i];
                    TypeNode t = varArgTypes[j - 1];
                    if (e != null && t != null) e.Type = t;
                }
            }

            Expression thisob = meth.IsStatic ? null : PopOperand();
            MemberBinding methBinding = new MemberBinding(thisob, meth);
            MethodCall result = new MethodCall(methBinding, arguments, typeOfCall);
            result.Type = meth.ReturnType;
            result.IsTailCall = this.isTailCall;

            if(this.constraint != null)
            {
                result.Constraint = this.constraint;
                this.constraint = null;
            }

            return result;
        }
        private static bool TypeIsVoid(TypeNode t)
        {
            if (t == null) return false;
            for (; ; )
            {
                switch (t.NodeType)
                {
                    case NodeType.OptionalModifier:
                    case NodeType.RequiredModifier:
                        t = ((TypeModifier)t).ModifiedType;
                        break;
                    default:
                        return t == CoreSystemTypes.Void;
                }
            }
        }

        private MethodCall/*!*/ ParseCalli(out bool isStatement)
        {
            FunctionPointer fp = this.reader.GetCalliSignature(this.GetInt32());

            if(fp == null)
                throw new InvalidMetadataException(ExceptionStrings.BadCalliSignature);

            isStatement = BodyParser.TypeIsVoid(fp.ReturnType);
            int n = fp.ParameterTypes.Count;

            Expression[] args = new Expression[n + 1];
            ExpressionList arguments = new ExpressionList();

            for(int i = n; i >= 0; i--)
                args[i] = PopOperand();
            
            for(int i = 0; i <= n; i++)
                arguments.Add(args[i]);

            Expression thisob = fp.IsStatic ? null : PopOperand();
            MemberBinding methBinding = new MemberBinding(thisob, fp);
            MethodCall result = new MethodCall(methBinding, arguments, NodeType.Calli);
            result.Type = fp.ReturnType;
            result.IsTailCall = this.isTailCall;
            
            return result;
        }

        private static Expression/*!*/ ParseTypeCheck(Expression operand, TypeNode type, NodeType typeOfCheck)
        {
            TypeNode etype = type;
            if (typeOfCheck == NodeType.Unbox) etype = type.GetReferenceType();
            Expression expr = new BinaryExpression(operand, new Literal(type, CoreSystemTypes.Type), typeOfCheck, etype);
            return expr;
        }

        private Construct/*!*/ ParseConstruct()
        {
            TypeNodeList varArgTypes;
            Method meth = (Method)this.GetMemberFromToken(out varArgTypes);
            int n = meth.Parameters.Count;

            Expression[] args = new Expression[n];
            ExpressionList arguments = new ExpressionList();

            for(int i = n - 1; i >= 0; i--)
                args[i] = PopOperand();

            for(int i = 0; i < n; i++)
                arguments.Add(args[i]);

            Construct result = new Construct(new MemberBinding(null, meth), arguments);
            result.Type = meth.DeclaringType;
            
            return result;
        }

        private AssignmentStatement/*!*/ ParseCopyObject()
        {
            TypeNode type = (TypeNode)this.GetMemberFromToken();
            Expression rhaddr = PopOperand();
            Expression lhaddr = PopOperand();
            return new AssignmentStatement(new AddressDereference(lhaddr, type, this.isVolatile, this.alignment), new AddressDereference(rhaddr, type));
        }
        private UnaryExpression /*!*/ ParseLoadRuntimeMetadataToken()
        {
            Expression expr = null;
            TypeNode exprType = null;
            Member member = this.GetMemberFromToken();
            TypeNode t = member as TypeNode;
            if (t == null)
            {
                exprType = (member.NodeType == NodeType.Field)
                  ? CoreSystemTypes.RuntimeFieldHandle : CoreSystemTypes.RuntimeMethodHandle;
                expr = new MemberBinding(null, member);
            }
            else
            {
                exprType = CoreSystemTypes.RuntimeTypeHandle;
                expr = new Literal(t, CoreSystemTypes.Type);
            }
            return new UnaryExpression(expr, NodeType.Ldtoken, exprType);
        }
        private AssignmentStatement/*!*/ ParseInitObject()
        {
            TypeNode type = (TypeNode)this.GetMemberFromToken();
            Expression lhaddr = PopOperand();
            return new AssignmentStatement(new AddressDereference(lhaddr, type, this.isVolatile, this.alignment), new Literal(null, CoreSystemTypes.Object));
        }

        private ConstructArray/*!*/ ParseNewArray()
        {
            TypeNode type = (TypeNode)this.GetMemberFromToken();
            ExpressionList sizes = new ExpressionList();
            sizes.Add(PopOperand());
            ConstructArray result = new ConstructArray(type, sizes, null);
            result.Type = type.GetArrayType(1);
            return result;
        }

        internal StatementList/*!*/ ParseStatements()
        {
            this.ParseHeader();

            if(this.size == 0)
                return new StatementList();

            this.CreateBlocksForBranchTargets();
            StatementList result = new StatementList();
            Block currentBlock = null;

            while(this.counter < size)
            {
                if (currentBlock == null)
                {
                    currentBlock = Reader.GetOrCreateBlock(this.blockMap, this.counter);
                    result.Add(currentBlock);
                }

                bool endOfBasicBlock = this.ParseStatement(currentBlock);

                if(endOfBasicBlock)
                    currentBlock = null;
            }

            result.Add(Reader.GetOrCreateBlock(this.blockMap, this.counter));
            return result;
        }

        private bool ParseStatement(Block/*!*/ block)
        {
            //parse instructions and put in expression tree until an assignment, void call, branch target, or branch is encountered
            StatementList statementList = block.Statements;
            Expression expr = null;
            Statement statement = null;
            bool transferStatement = false;
            int startingAddress = 0;

            SourceContext sourceContext = new SourceContext();
            sourceContext.StartPos = this.counter;

            if (this.method.contextForOffset != null)
            {
                object sctx = this.method.contextForOffset[this.counter + 1];
                if (sctx != null) sourceContext = (SourceContext)sctx;
            }

            while (true)
            {
                bool isStatement = false;
                startingAddress = this.counter + 1; //Add one so that it is never zero (the latter means no entry to the TrivialHashtable)

                OpCode opCode = this.GetOpCode();

                switch (opCode)
                {
                    case OpCode.Nop: statement = new Statement(NodeType.Nop); goto done;
                    case OpCode.Break: statement = new Statement(NodeType.DebugBreak); goto done;
                    case OpCode.Ldarg_0: expr = this.Parameters(0); break;
                    case OpCode.Ldarg_1: expr = this.Parameters(1); break;
                    case OpCode.Ldarg_2: expr = this.Parameters(2); break;
                    case OpCode.Ldarg_3: expr = this.Parameters(3); break;
                    case OpCode.Ldloc_0: expr = this.locals[0]; break;
                    case OpCode.Ldloc_1: expr = this.locals[1]; break;
                    case OpCode.Ldloc_2: expr = this.locals[2]; break;
                    case OpCode.Ldloc_3: expr = this.locals[3]; break;
                    case OpCode.Stloc_0: statement = new AssignmentStatement(this.locals[0], PopOperand()); goto done;
                    case OpCode.Stloc_1: statement = new AssignmentStatement(this.locals[1], PopOperand()); goto done;
                    case OpCode.Stloc_2: statement = new AssignmentStatement(this.locals[2], PopOperand()); goto done;
                    case OpCode.Stloc_3: statement = new AssignmentStatement(this.locals[3], PopOperand()); goto done;
                    case OpCode.Ldarg_S: expr = this.Parameters(this.GetByte()); break;
                    case OpCode.Ldarga_S: expr = SetType(new UnaryExpression(this.Parameters(this.GetByte()), NodeType.AddressOf)); break;
                    case OpCode.Starg_S: statement = new AssignmentStatement(this.Parameters(this.GetByte()), PopOperand()); goto done;
                    case OpCode.Ldloc_S: expr = this.locals[this.GetByte()]; break;
                    case OpCode.Ldloca_S: expr = SetType(new UnaryExpression(this.locals[this.GetByte()], NodeType.AddressOf)); break;
                    case OpCode.Stloc_S: statement = new AssignmentStatement(this.locals[this.GetByte()], PopOperand()); goto done;
                    case OpCode.Ldnull: expr = new Literal(null, CoreSystemTypes.Object); break;
                    case OpCode.Ldc_I4_M1: expr = new Literal(-1, CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I4_0: expr = new Literal(0, CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I4_1: expr = new Literal(1, CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I4_2: expr = new Literal(2, CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I4_3: expr = new Literal(3, CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I4_4: expr = new Literal(4, CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I4_5: expr = new Literal(5, CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I4_6: expr = new Literal(6, CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I4_7: expr = new Literal(7, CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I4_8: expr = new Literal(8, CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I4_S: expr = new Literal((int)this.GetSByte(), CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I4: expr = new Literal(this.GetInt32(), CoreSystemTypes.Int32); break;
                    case OpCode.Ldc_I8: expr = new Literal(this.GetInt64(), CoreSystemTypes.Int64); break;
                    case OpCode.Ldc_R4: expr = new Literal(this.GetSingle(), CoreSystemTypes.Single); break;
                    case OpCode.Ldc_R8: expr = new Literal(this.GetDouble(), CoreSystemTypes.Double); break;
                    case OpCode.Dup: statement = new ExpressionStatement(new Expression(NodeType.Dup)); goto done;
                    case OpCode.Pop: statement = new ExpressionStatement(new UnaryExpression(PopOperand(), NodeType.Pop)); goto done;
                    case OpCode.Jmp: expr = this.ParseCall(NodeType.Jmp, out isStatement); if (isStatement) goto done; break;
                    case OpCode.Call: expr = this.ParseCall(NodeType.Call, out isStatement); if (isStatement) goto done; break;
                    case OpCode.Calli: expr = this.ParseCalli(out isStatement); if (isStatement) goto done; break;
                    case OpCode.Ret:
                        Expression retVal = BodyParser.TypeIsVoid(this.method.ReturnType) ? null : PopOperand();
                        statement = new Return(retVal);
                        transferStatement = true; goto done;
                    case OpCode.Br_S: statement = this.ParseBranch(NodeType.Nop, 0, true, false); transferStatement = true; goto done;
                    case OpCode.Brfalse_S: statement = this.ParseBranch(NodeType.LogicalNot, 1, true, false); transferStatement = true; goto done;
                    case OpCode.Brtrue_S: statement = this.ParseBranch(NodeType.Nop, 1, true, false); transferStatement = true; goto done;
                    case OpCode.Beq_S: statement = this.ParseBranch(NodeType.Eq, 2, true, false); transferStatement = true; goto done;
                    case OpCode.Bge_S: statement = this.ParseBranch(NodeType.Ge, 2, true, false); transferStatement = true; goto done;
                    case OpCode.Bgt_S: statement = this.ParseBranch(NodeType.Gt, 2, true, false); transferStatement = true; goto done;
                    case OpCode.Ble_S: statement = this.ParseBranch(NodeType.Le, 2, true, false); transferStatement = true; goto done;
                    case OpCode.Blt_S: statement = this.ParseBranch(NodeType.Lt, 2, true, false); transferStatement = true; goto done;
                    case OpCode.Bne_Un_S: statement = this.ParseBranch(NodeType.Ne, 2, true, true); transferStatement = true; goto done;
                    case OpCode.Bge_Un_S: statement = this.ParseBranch(NodeType.Ge, 2, true, true); transferStatement = true; goto done;
                    case OpCode.Bgt_Un_S: statement = this.ParseBranch(NodeType.Gt, 2, true, true); transferStatement = true; goto done;
                    case OpCode.Ble_Un_S: statement = this.ParseBranch(NodeType.Le, 2, true, true); transferStatement = true; goto done;
                    case OpCode.Blt_Un_S: statement = this.ParseBranch(NodeType.Lt, 2, true, true); transferStatement = true; goto done;
                    case OpCode.Br: statement = this.ParseBranch(NodeType.Nop, 0, false, false); transferStatement = true; goto done;
                    case OpCode.Brfalse: statement = this.ParseBranch(NodeType.LogicalNot, 1, false, false); transferStatement = true; goto done;
                    case OpCode.Brtrue: statement = this.ParseBranch(NodeType.Nop, 1, false, false); transferStatement = true; goto done;
                    case OpCode.Beq: statement = this.ParseBranch(NodeType.Eq, 2, false, false); transferStatement = true; goto done;
                    case OpCode.Bge: statement = this.ParseBranch(NodeType.Ge, 2, false, false); transferStatement = true; goto done;
                    case OpCode.Bgt: statement = this.ParseBranch(NodeType.Gt, 2, false, false); transferStatement = true; goto done;
                    case OpCode.Ble: statement = this.ParseBranch(NodeType.Le, 2, false, false); transferStatement = true; goto done;
                    case OpCode.Blt: statement = this.ParseBranch(NodeType.Lt, 2, false, false); transferStatement = true; goto done;
                    case OpCode.Bne_Un: statement = this.ParseBranch(NodeType.Ne, 2, false, true); transferStatement = true; goto done;
                    case OpCode.Bge_Un: statement = this.ParseBranch(NodeType.Ge, 2, false, true); transferStatement = true; goto done;
                    case OpCode.Bgt_Un: statement = this.ParseBranch(NodeType.Gt, 2, false, true); transferStatement = true; goto done;
                    case OpCode.Ble_Un: statement = this.ParseBranch(NodeType.Le, 2, false, true); transferStatement = true; goto done;
                    case OpCode.Blt_Un: statement = this.ParseBranch(NodeType.Lt, 2, false, true); transferStatement = true; goto done;
                    case OpCode.Switch: statement = this.ParseSwitchInstruction(); transferStatement = true; goto done;
                    case OpCode.Ldind_I1: expr = new AddressDereference(PopOperand(), CoreSystemTypes.Int8, this.isVolatile, this.alignment); break;
                    case OpCode.Ldind_U1: expr = new AddressDereference(PopOperand(), CoreSystemTypes.UInt8, this.isVolatile, this.alignment); break;
                    case OpCode.Ldind_I2: expr = new AddressDereference(PopOperand(), CoreSystemTypes.Int16, this.isVolatile, this.alignment); break;
                    case OpCode.Ldind_U2: expr = new AddressDereference(PopOperand(), CoreSystemTypes.UInt16, this.isVolatile, this.alignment); break;
                    case OpCode.Ldind_I4: expr = new AddressDereference(PopOperand(), CoreSystemTypes.Int32, this.isVolatile, this.alignment); break;
                    case OpCode.Ldind_U4: expr = new AddressDereference(PopOperand(), CoreSystemTypes.UInt32, this.isVolatile, this.alignment); break;
                    case OpCode.Ldind_I8: expr = new AddressDereference(PopOperand(), CoreSystemTypes.Int64, this.isVolatile, this.alignment); break;
                    case OpCode.Ldind_I: expr = new AddressDereference(PopOperand(), CoreSystemTypes.IntPtr, this.isVolatile, this.alignment); break;
                    case OpCode.Ldind_R4: expr = new AddressDereference(PopOperand(), CoreSystemTypes.Single, this.isVolatile, this.alignment); break;
                    case OpCode.Ldind_R8: expr = new AddressDereference(PopOperand(), CoreSystemTypes.Double, this.isVolatile, this.alignment); break;
                    case OpCode.Ldind_Ref: expr = new AddressDereference(PopOperand(), CoreSystemTypes.Object, this.isVolatile, this.alignment); break;
                    case OpCode.Stind_Ref: statement = this.ParseStoreIndirect(CoreSystemTypes.Object); goto done;
                    case OpCode.Stind_I1: statement = this.ParseStoreIndirect(CoreSystemTypes.Int8); goto done;
                    case OpCode.Stind_I2: statement = this.ParseStoreIndirect(CoreSystemTypes.Int16); goto done;
                    case OpCode.Stind_I4: statement = this.ParseStoreIndirect(CoreSystemTypes.Int32); goto done;
                    case OpCode.Stind_I8: statement = this.ParseStoreIndirect(CoreSystemTypes.Int64); goto done;
                    case OpCode.Stind_R4: statement = this.ParseStoreIndirect(CoreSystemTypes.Single); goto done;
                    case OpCode.Stind_R8: statement = this.ParseStoreIndirect(CoreSystemTypes.Double); goto done;
                    case OpCode.Add: expr = this.ParseBinaryOperation(NodeType.Add); break;
                    case OpCode.Sub: expr = this.ParseBinaryOperation(NodeType.Sub); break;
                    case OpCode.Mul: expr = this.ParseBinaryOperation(NodeType.Mul); break;
                    case OpCode.Div: expr = this.ParseBinaryOperation(NodeType.Div); break;
                    case OpCode.Div_Un: expr = this.ParseBinaryOperation(NodeType.Div_Un); break;
                    case OpCode.Rem: expr = this.ParseBinaryOperation(NodeType.Rem); break;
                    case OpCode.Rem_Un: expr = this.ParseBinaryOperation(NodeType.Rem_Un); break;
                    case OpCode.And: expr = this.ParseBinaryOperation(NodeType.And); break;
                    case OpCode.Or: expr = this.ParseBinaryOperation(NodeType.Or); break;
                    case OpCode.Xor: expr = this.ParseBinaryOperation(NodeType.Xor); break;
                    case OpCode.Shl: expr = this.ParseBinaryOperation(NodeType.Shl); break;
                    case OpCode.Shr: expr = this.ParseBinaryOperation(NodeType.Shr); break;
                    case OpCode.Shr_Un: expr = this.ParseBinaryOperation(NodeType.Shr_Un); break;
                    case OpCode.Neg: expr = this.ParseUnaryOperation(NodeType.Neg); break;
                    case OpCode.Not: expr = this.ParseUnaryOperation(NodeType.Not); break;
                    case OpCode.Conv_I1: expr = new UnaryExpression(PopOperand(), NodeType.Conv_I1, CoreSystemTypes.Int8); break;
                    case OpCode.Conv_I2: expr = new UnaryExpression(PopOperand(), NodeType.Conv_I2, CoreSystemTypes.Int16); break;
                    case OpCode.Conv_I4: expr = new UnaryExpression(PopOperand(), NodeType.Conv_I4, CoreSystemTypes.Int32); break;
                    case OpCode.Conv_I8: expr = new UnaryExpression(PopOperand(), NodeType.Conv_I8, CoreSystemTypes.Int64); break;
                    case OpCode.Conv_R4: expr = new UnaryExpression(PopOperand(), NodeType.Conv_R4, CoreSystemTypes.Single); break;
                    case OpCode.Conv_R8: expr = new UnaryExpression(PopOperand(), NodeType.Conv_R8, CoreSystemTypes.Double); break;
                    case OpCode.Conv_U4: expr = new UnaryExpression(PopOperand(), NodeType.Conv_U4, CoreSystemTypes.UInt32); break;
                    case OpCode.Conv_U8: expr = new UnaryExpression(PopOperand(), NodeType.Conv_U8, CoreSystemTypes.UInt64); break;
                    case OpCode.Callvirt: expr = this.ParseCall(NodeType.Callvirt, out isStatement); if (isStatement) goto done; break;
                    case OpCode.Cpobj: statement = this.ParseCopyObject(); goto done;
                    case OpCode.Ldobj: expr = new AddressDereference(PopOperand(), (TypeNode)this.GetMemberFromToken(), this.isVolatile, this.alignment); break;
                    case OpCode.Ldstr: expr = new Literal(this.GetStringFromToken(), CoreSystemTypes.String); break;
                    case OpCode.Newobj: expr = this.ParseConstruct(); break;
                    case OpCode.Castclass: expr = ParseTypeCheck(PopOperand(), (TypeNode)this.GetMemberFromToken(), NodeType.Castclass); break;
                    case OpCode.Isinst: expr = ParseTypeCheck(PopOperand(), (TypeNode)this.GetMemberFromToken(), NodeType.Isinst); break;
                    case OpCode.Conv_R_Un: expr = new UnaryExpression(PopOperand(), NodeType.Conv_R_Un, CoreSystemTypes.Double); break;
                    case OpCode.Unbox: expr = ParseTypeCheck(PopOperand(), (TypeNode)this.GetMemberFromToken(), NodeType.Unbox); break;
                    case OpCode.Throw: statement = new Throw(PopOperand()); transferStatement = true; goto done;
                    case OpCode.Ldfld:
                        expr = new MemberBinding(PopOperand(), this.GetMemberFromToken(), this.isVolatile, this.alignment);
                        break;
                    case OpCode.Ldflda:
                        expr = SetType(new UnaryExpression(new MemberBinding(PopOperand(), this.GetMemberFromToken(), this.isVolatile, this.alignment), NodeType.AddressOf));
                        break;
                    case OpCode.Stfld: statement = this.ParseStoreField(); goto done;
                    case OpCode.Ldsfld: expr = new MemberBinding(null, this.GetMemberFromToken(), this.isVolatile, this.alignment); break;
                    case OpCode.Ldsflda: expr = SetType(new UnaryExpression(new MemberBinding(null, this.GetMemberFromToken(), this.isVolatile, this.alignment), NodeType.AddressOf)); break;
                    case OpCode.Stsfld: statement = new AssignmentStatement(new MemberBinding(null, this.GetMemberFromToken(), this.isVolatile, this.alignment), PopOperand()); goto done;
                    case OpCode.Stobj: statement = this.ParseStoreIndirect((TypeNode)this.GetMemberFromToken()); goto done;
                    case OpCode.Conv_Ovf_I1_Un: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_I1_Un, CoreSystemTypes.Int8); break;
                    case OpCode.Conv_Ovf_I2_Un: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_I2_Un, CoreSystemTypes.Int16); break;
                    case OpCode.Conv_Ovf_I4_Un: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_I4_Un, CoreSystemTypes.Int32); break;
                    case OpCode.Conv_Ovf_I8_Un: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_I8_Un, CoreSystemTypes.Int64); break;
                    case OpCode.Conv_Ovf_U1_Un: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_U1_Un, CoreSystemTypes.UInt8); break;
                    case OpCode.Conv_Ovf_U2_Un: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_U2_Un, CoreSystemTypes.UInt16); break;
                    case OpCode.Conv_Ovf_U4_Un: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_U4_Un, CoreSystemTypes.UInt32); break;
                    case OpCode.Conv_Ovf_U8_Un: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_U8_Un, CoreSystemTypes.UInt64); break;
                    case OpCode.Conv_Ovf_I_Un: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_I_Un, CoreSystemTypes.IntPtr); break;
                    case OpCode.Conv_Ovf_U_Un: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_U_Un, CoreSystemTypes.UIntPtr); break;
                    case OpCode.Box:
                        TypeNode t = (TypeNode)this.GetMemberFromToken();
                        TypeNode bt = t is EnumNode ? CoreSystemTypes.Enum : CoreSystemTypes.ValueType;
                        expr = new BinaryExpression(PopOperand(), new Literal(t, CoreSystemTypes.Type), NodeType.Box, bt); break;
                    case OpCode.Newarr: expr = this.ParseNewArray(); break;
                    case OpCode.Ldlen: expr = new UnaryExpression(PopOperand(), NodeType.Ldlen, CoreSystemTypes.UIntPtr); break;
                    case OpCode.Ldelema: expr = this.ParseArrayElementLoadAddress(); break;
                    case OpCode.Ldelem_I1:
                    case OpCode.Ldelem_U1:
                    case OpCode.Ldelem_I2:
                    case OpCode.Ldelem_U2:
                    case OpCode.Ldelem_I4:
                    case OpCode.Ldelem_U4:
                    case OpCode.Ldelem_I8:
                    case OpCode.Ldelem_I:
                    case OpCode.Ldelem_R4:
                    case OpCode.Ldelem_R8:
                    case OpCode.Ldelem_Ref: expr = this.ParseArrayElementLoad(opCode, null); break;
                    case OpCode.Stelem_I:
                    case OpCode.Stelem_I1:
                    case OpCode.Stelem_I2:
                    case OpCode.Stelem_I4:
                    case OpCode.Stelem_I8:
                    case OpCode.Stelem_R4:
                    case OpCode.Stelem_R8:
                    case OpCode.Stelem_Ref: statement = this.ParseArrayElementAssignment(opCode); goto done;
                    case OpCode.Ldelem: expr = this.ParseArrayElementLoad(opCode, null); break;
                    case OpCode.Stelem: statement = this.ParseArrayElementAssignment(opCode); goto done;
                    case OpCode.Unbox_Any: expr = ParseTypeCheck(PopOperand(), (TypeNode)this.GetMemberFromToken(), NodeType.UnboxAny); break;
                    case OpCode.Conv_Ovf_I1: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_I1, CoreSystemTypes.Int8); break;
                    case OpCode.Conv_Ovf_U1: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_U1, CoreSystemTypes.UInt8); break;
                    case OpCode.Conv_Ovf_I2: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_I2, CoreSystemTypes.Int16); break;
                    case OpCode.Conv_Ovf_U2: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_U2, CoreSystemTypes.UInt16); break;
                    case OpCode.Conv_Ovf_I4: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_I4, CoreSystemTypes.Int32); break;
                    case OpCode.Conv_Ovf_U4: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_U4, CoreSystemTypes.UInt32); break;
                    case OpCode.Conv_Ovf_I8: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_I8, CoreSystemTypes.Int64); break;
                    case OpCode.Conv_Ovf_U8: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_U8, CoreSystemTypes.UInt64); break;
                    case OpCode.Refanyval: expr = new BinaryExpression(PopOperand(), new Literal(this.GetMemberFromToken(), CoreSystemTypes.Type), NodeType.Refanyval, CoreSystemTypes.IntPtr); break;
                    case OpCode.Ckfinite: expr = this.ParseUnaryOperation(NodeType.Ckfinite); break;
                    case OpCode.Mkrefany: expr = new BinaryExpression(PopOperand(), new Literal(this.GetMemberFromToken(), CoreSystemTypes.Type), NodeType.Mkrefany, CoreSystemTypes.DynamicallyTypedReference); break;
                    case OpCode.Ldtoken: expr = ParseLoadRuntimeMetadataToken(); break;
                    case OpCode.Conv_U2: expr = new UnaryExpression(PopOperand(), NodeType.Conv_U2, CoreSystemTypes.UInt16); break;
                    case OpCode.Conv_U1: expr = new UnaryExpression(PopOperand(), NodeType.Conv_U1, CoreSystemTypes.UInt8); break;
                    case OpCode.Conv_I: expr = new UnaryExpression(PopOperand(), NodeType.Conv_I, CoreSystemTypes.IntPtr); break;
                    case OpCode.Conv_Ovf_I: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_I, CoreSystemTypes.IntPtr); break;
                    case OpCode.Conv_Ovf_U: expr = new UnaryExpression(PopOperand(), NodeType.Conv_Ovf_U, CoreSystemTypes.UIntPtr); break;
                    case OpCode.Add_Ovf: expr = this.ParseBinaryOperation(NodeType.Add_Ovf); break;
                    case OpCode.Add_Ovf_Un: expr = this.ParseBinaryOperation(NodeType.Add_Ovf_Un); break;
                    case OpCode.Mul_Ovf: expr = this.ParseBinaryOperation(NodeType.Mul_Ovf); break;
                    case OpCode.Mul_Ovf_Un: expr = this.ParseBinaryOperation(NodeType.Mul_Ovf_Un); break;
                    case OpCode.Sub_Ovf: expr = this.ParseBinaryOperation(NodeType.Sub_Ovf); break;
                    case OpCode.Sub_Ovf_Un: expr = this.ParseBinaryOperation(NodeType.Sub_Ovf_Un); break;
                    case OpCode.Endfinally: statement = new EndFinally(); transferStatement = true; goto done;
                    case OpCode.Leave: statement = this.ParseBranch(NodeType.Nop, 0, false, false, true); transferStatement = true; goto done;
                    case OpCode.Leave_S: statement = this.ParseBranch(NodeType.Nop, 0, true, false, true); transferStatement = true; goto done;
                    case OpCode.Stind_I: statement = this.ParseStoreIndirect(CoreSystemTypes.IntPtr); goto done;
                    case OpCode.Conv_U: expr = new UnaryExpression(PopOperand(), NodeType.Conv_U, CoreSystemTypes.UIntPtr); break;
                    case OpCode.Arglist: expr = new Expression(NodeType.Arglist, CoreSystemTypes.ArgIterator); break;
                    case OpCode.Ceq: expr = this.ParseBinaryComparison(NodeType.Ceq); break;
                    case OpCode.Cgt: expr = this.ParseBinaryComparison(NodeType.Cgt); break;
                    case OpCode.Cgt_Un: expr = this.ParseBinaryComparison(NodeType.Cgt_Un); break;
                    case OpCode.Clt: expr = this.ParseBinaryComparison(NodeType.Clt); break;
                    case OpCode.Clt_Un: expr = this.ParseBinaryComparison(NodeType.Clt_Un); break;
                    case OpCode.Ldftn: expr = new UnaryExpression(new MemberBinding(null, this.GetMemberFromToken()), NodeType.Ldftn, CoreSystemTypes.IntPtr); break;
                    case OpCode.Ldvirtftn: expr = new BinaryExpression(PopOperand(), new MemberBinding(null, this.GetMemberFromToken()), NodeType.Ldvirtftn, CoreSystemTypes.IntPtr); break;
                    case OpCode.Ldarg: expr = this.Parameters((ushort)this.GetInt16()); break;
                    case OpCode.Ldarga: expr = SetType(new UnaryExpression(this.Parameters((ushort)this.GetInt16()), NodeType.AddressOf)); break;
                    case OpCode.Starg: statement = new AssignmentStatement(this.Parameters((ushort)this.GetInt16()), PopOperand()); goto done;
                    case OpCode.Ldloc: expr = this.locals[(ushort)this.GetInt16()]; break;
                    case OpCode.Ldloca: expr = SetType(new UnaryExpression(this.locals[(ushort)this.GetInt16()], NodeType.AddressOf)); break;
                    case OpCode.Stloc: statement = new AssignmentStatement(this.locals[(ushort)this.GetInt16()], PopOperand()); goto done;
                    case OpCode.Localloc: expr = new UnaryExpression(PopOperand(), NodeType.Localloc, CoreSystemTypes.Void); break;
                    case OpCode.Endfilter: statement = new EndFilter(PopOperand()); transferStatement = true; goto done;
                    case OpCode.Unaligned_: this.alignment = this.GetByte(); continue;
                    case OpCode.Volatile_: this.isVolatile = true; continue;
                    case OpCode.Tail_: this.isTailCall = true; continue;
                    case OpCode.Initobj: statement = this.ParseInitObject(); goto done;
                    case OpCode.Constrained_: this.constraint = this.GetMemberFromToken() as TypeNode; continue;
                    case OpCode.Cpblk: expr = this.ParseTernaryOperation(NodeType.Cpblk); goto done;
                    case OpCode.Initblk: expr = this.ParseTernaryOperation(NodeType.Initblk); goto done;
                    case OpCode.Rethrow: statement = new Throw(null); statement.NodeType = NodeType.Rethrow; transferStatement = true; goto done;
                    case OpCode.Sizeof: expr = new UnaryExpression(new Literal(this.GetMemberFromToken(), CoreSystemTypes.Type), NodeType.Sizeof, CoreSystemTypes.Int32); break;
                    case OpCode.Refanytype: expr = new UnaryExpression(PopOperand(), NodeType.Refanytype, CoreSystemTypes.RuntimeTypeHandle); break;
                    case OpCode.Readonly_: this.isReadOnly = true; continue;
                    default: throw new InvalidMetadataException(ExceptionStrings.UnknownOpCode);
                }
                if (this.blockMap[this.counter + 1] != null)
                {
                    transferStatement = true; //Falls through to the next basic block, so implicitly a "transfer" statement
                    goto done;
                }
                //^ assume expr != null;
                this.operandStack.Push(expr);
                this.isReadOnly = false;
                this.isVolatile = false;
                this.isTailCall = false;
                this.alignment = -1;
            }
        done:
            for (int i = 0; i <= this.operandStack.top; i++)
            {
                Expression e = this.operandStack.elements[i];
                //^ assume e != null;
                Statement s = new ExpressionStatement(e);
                statementList.Add(s);
            }

            this.operandStack.top = -1;

            if (statement == null)
                statement = new ExpressionStatement(expr);

            statement.SourceContext = sourceContext;
            statementList.Add(statement);
            if (transferStatement) return true;
            return this.blockMap[this.counter + 1] != null;
        }
        private AssignmentStatement ParseStoreField()
        {
            Expression rhvalue = PopOperand();
            Expression thisob = PopOperand();
            AssignmentStatement s = new AssignmentStatement(new MemberBinding(thisob, this.GetMemberFromToken(), this.isVolatile, this.alignment), rhvalue);
            return s;
        }
        private AssignmentStatement ParseStoreIndirect(TypeNode type)
        {
            Expression rhvalue = PopOperand();
            Expression lhaddr = PopOperand();
            return new AssignmentStatement(new AddressDereference(lhaddr, type, this.isVolatile, this.alignment), rhvalue);
        }

        private SwitchInstruction ParseSwitchInstruction()
        {
            int numTargets = this.GetInt32();
            int offset = this.counter + numTargets * 4;

            BlockList targetList = new BlockList();

            for(int i = 0; i < numTargets; i++)
            {
                int targetAddress = this.GetInt32() + offset;
                targetList.Add(Reader.GetOrCreateBlock(this.blockMap, targetAddress));
            }

            return new SwitchInstruction(PopOperand(), targetList);
        }

        private TernaryExpression ParseTernaryOperation(NodeType oper)
        {
            Expression op3 = PopOperand();
            Expression op2 = PopOperand();
            Expression op1 = PopOperand();
            return new TernaryExpression(op1, op2, op3, oper, null);
        }
        private void CreateBlocksForBranchTargets()
        {
            int savedPosition = bodyReader.Position;
            while (this.counter < this.size)
                this.ProcessOneILInstruction();
            this.counter = 0;
            bodyReader.Position = savedPosition;
        }
        private void ProcessOneILInstruction()
        {
            OpCode opc = this.GetOpCode();
            switch (opc)
            {
                case OpCode.Ldarg_S:
                case OpCode.Ldarga_S:
                case OpCode.Starg_S:
                case OpCode.Ldloc_S:
                case OpCode.Ldloca_S:
                case OpCode.Stloc_S:
                case OpCode.Ldc_I4_S:
                    this.GetByte(); return;
                case OpCode.Ldc_I4:
                case OpCode.Jmp:
                case OpCode.Call:
                case OpCode.Calli:
                case OpCode.Callvirt:
                case OpCode.Cpobj:
                case OpCode.Ldobj:
                case OpCode.Ldstr:
                case OpCode.Newobj:
                case OpCode.Castclass:
                case OpCode.Isinst:
                case OpCode.Unbox:
                case OpCode.Ldfld:
                case OpCode.Ldflda:
                case OpCode.Stfld:
                case OpCode.Ldsfld:
                case OpCode.Ldsflda:
                case OpCode.Stsfld:
                case OpCode.Stobj:
                case OpCode.Box:
                case OpCode.Newarr:
                case OpCode.Ldelema:
                case OpCode.Ldelem:
                case OpCode.Stelem:
                case OpCode.Unbox_Any:
                case OpCode.Refanyval:
                case OpCode.Mkrefany:
                case OpCode.Ldtoken:
                    this.GetInt32(); return;
                case OpCode.Ldc_I8:
                    this.GetInt64(); return;
                case OpCode.Ldc_R4:
                    this.GetSingle(); return;
                case OpCode.Ldc_R8:
                    this.GetDouble(); return;
                case OpCode.Br_S:
                case OpCode.Brfalse_S:
                case OpCode.Brtrue_S:
                case OpCode.Beq_S:
                case OpCode.Bge_S:
                case OpCode.Bgt_S:
                case OpCode.Ble_S:
                case OpCode.Blt_S:
                case OpCode.Bne_Un_S:
                case OpCode.Bge_Un_S:
                case OpCode.Bgt_Un_S:
                case OpCode.Ble_Un_S:
                case OpCode.Blt_Un_S:
                case OpCode.Leave_S:
                    this.SkipBranch(true); return;
                case OpCode.Br:
                case OpCode.Brfalse:
                case OpCode.Brtrue:
                case OpCode.Beq:
                case OpCode.Bge:
                case OpCode.Bgt:
                case OpCode.Ble:
                case OpCode.Blt:
                case OpCode.Bne_Un:
                case OpCode.Bge_Un:
                case OpCode.Bgt_Un:
                case OpCode.Ble_Un:
                case OpCode.Blt_Un:
                case OpCode.Leave:
                    this.SkipBranch(false); return;
                case OpCode.Switch:
                    this.SkipSwitch(); return;
                case OpCode.Ldftn:
                case OpCode.Ldvirtftn:
                case OpCode.Initobj:
                case OpCode.Constrained_:
                case OpCode.Sizeof:
                    this.GetInt32(); return;
                case OpCode.Ldarg:
                case OpCode.Ldarga:
                case OpCode.Ldloc:
                case OpCode.Ldloca:
                case OpCode.Starg:
                case OpCode.Stloc:
                    this.GetInt16(); return;
                case OpCode.Unaligned_:
                    this.GetByte(); return;
                default:
                    return;
            }
        }
        private void SkipBranch(bool shortOffset)
        {
            int offset = shortOffset ? this.GetSByte() : this.GetInt32();
            Reader.GetOrCreateBlock(blockMap, this.counter + offset);
        }
        private void SkipSwitch()
        {
            int numCases = this.GetInt32();
            int offset = this.counter + numCases * 4;
            for (int i = 0; i < numCases; i++)
            {
                int targetAddress = this.GetInt32() + offset;
                Reader.GetOrCreateBlock(this.blockMap, targetAddress);
            }
        }
        private Expression PopOperand()
        {
            return this.operandStack.Pop();
        }
    }

    internal class InstructionParser : ILParser
    {
        private readonly TrivialHashtable/*!*/ ehMap;
        internal InstructionParser(Reader/*!*/ reader, Method/*!*/ method, int methodIndex, int RVA)
            : base(reader, method, methodIndex, RVA)
        {
            this.ehMap = new TrivialHashtable();
        }
        override protected void ParseExceptionHandlerEntry(bool smallSection)
        {
            TrivialHashtable tryMap = new TrivialHashtable();
            int dataSize = this.reader.tables.GetByte();
            int n = (int)(ushort)this.reader.tables.GetInt16();
            if (smallSection)
                n = dataSize / 12;
            else
                n = (dataSize + (n << 8)) / 24;
            for (int i = 0; i < n; i++)
            {
                Instruction matchingInstruction;
                int flags, tryOffset, tryLength, handlerOffset, handlerLength, tokenOrOffset;
                if (smallSection)
                {
                    flags = this.reader.tables.GetInt16();
                    tryOffset = this.reader.tables.GetUInt16();
                    tryLength = this.reader.tables.GetByte();
                    handlerOffset = this.reader.tables.GetUInt16();
                    handlerLength = this.reader.tables.GetByte();
                }
                else
                {
                    flags = this.reader.tables.GetInt32();
                    tryOffset = this.reader.tables.GetInt32();
                    tryLength = this.reader.tables.GetInt32();
                    handlerOffset = this.reader.tables.GetInt32();
                    handlerLength = this.reader.tables.GetInt32();
                }
                tokenOrOffset = this.reader.tables.GetInt32();
                if (tryMap[tryOffset + tryLength] == null)
                {
                    matchingInstruction = this.AddInstruction(OpCode._Try, tryOffset);
                    this.AddInstruction(OpCode._EndTry, tryOffset + tryLength, matchingInstruction);
                    tryMap[tryOffset + tryLength] = String.Empty;
                }
                switch (flags)
                {
                    case 0x00:
                        int pos = this.reader.tables.GetCurrentPosition();
                        TypeNode catchType = (TypeNode)this.reader.GetMemberFromToken(tokenOrOffset);
                        this.reader.tables.SetCurrentPosition(pos);
                        matchingInstruction = this.AddInstruction(OpCode._Catch, handlerOffset, catchType);
                        this.AddInstruction(OpCode._EndHandler, handlerOffset + handlerLength, matchingInstruction);
                        break;
                    case 0x01:
                        matchingInstruction = this.AddInstruction(OpCode._Filter, tokenOrOffset);
                        this.AddInstruction(OpCode._EndFilter, handlerOffset, matchingInstruction);
                        matchingInstruction = this.AddInstruction(OpCode._Catch, handlerOffset);
                        this.AddInstruction(OpCode._EndHandler, handlerOffset + handlerLength, matchingInstruction);
                        break;
                    case 0x02:
                        matchingInstruction = this.AddInstruction(OpCode._Finally, handlerOffset);
                        this.AddInstruction(OpCode._EndHandler, handlerOffset + handlerLength, matchingInstruction);
                        break;
                    case 0x04:
                        matchingInstruction = this.AddInstruction(OpCode._Fault, handlerOffset);
                        this.AddInstruction(OpCode._EndHandler, handlerOffset + handlerLength, matchingInstruction);
                        break;
                    default: throw new InvalidMetadataException(ExceptionStrings.BadExceptionHandlerType);
                }
            }
        }
        private Instruction AddInstruction(OpCode opCode, int offset)
        {
            return this.AddInstruction(opCode, offset, null);
        }

        private Instruction AddInstruction(OpCode opCode, int offset, object value)
        {
            Instruction instruction = new Instruction(opCode, offset, value);
            InstructionList instructions = (InstructionList)this.ehMap[offset + 1];

            if(instructions == null)
                this.ehMap[offset + 1] = instructions = new InstructionList();

            instructions.Add(instruction);

            if(this.method.contextForOffset != null)
            {
                object sctx = this.method.contextForOffset[offset + 1];

                if(sctx != null)
                    instruction.SourceContext = (SourceContext)sctx;
            }

            return instruction;
        }

        private List<int> ParseSwitchInstruction()
        {
            int numTargets = this.GetInt32();
            List<int> result = new List<int>();

            int offset = this.counter + numTargets * 4;

            for(int i = 0; i < numTargets; i++)
            {
                int targetAddress = this.GetInt32() + offset;
                result.Add(targetAddress);
            }

            return result;
        }

        internal InstructionList ParseInstructions()
        {
            this.ParseHeader();

            if(this.size == 0)
                return new InstructionList();

            InstructionList result = new InstructionList();

            result.Add(new Instruction(OpCode._Locals, 0, this.locals));

            while(this.counter <= size)
            {
                InstructionList instructions = (InstructionList)this.ehMap[this.counter + 1];

                if(instructions != null)
                {
                    for(int i = 0; i < instructions.Count; i++)
                        result.Add(instructions[i]);
                }

                if(this.counter < size)
                    result.Add(this.ParseInstruction());
                else
                    break;
            }

            return result;
        }

        private SourceContext sourceContext = new SourceContext();

        internal Instruction ParseInstruction()
        {
            if (this.counter >= this.size)
                return null;
            int offset = this.counter;

            if (this.method.contextForOffset != null)
            {
                object sctx = this.method.contextForOffset[offset + 1];
                if (sctx != null) this.sourceContext = (SourceContext)sctx;
            }

            object value = null;
            OpCode opCode = this.GetOpCode();
            switch (opCode)
            {
                case OpCode.Nop:
                case OpCode.Break:
                    break;
                case OpCode.Ldarg_0: value = this.Parameters(0); break;
                case OpCode.Ldarg_1: value = this.Parameters(1); break;
                case OpCode.Ldarg_2: value = this.Parameters(2); break;
                case OpCode.Ldarg_3: value = this.Parameters(3); break;
                case OpCode.Ldloc_0: value = this.locals[0]; break;
                case OpCode.Ldloc_1: value = this.locals[1]; break;
                case OpCode.Ldloc_2: value = this.locals[2]; break;
                case OpCode.Ldloc_3: value = this.locals[3]; break;
                case OpCode.Stloc_0: value = this.locals[0]; break;
                case OpCode.Stloc_1: value = this.locals[1]; break;
                case OpCode.Stloc_2: value = this.locals[2]; break;
                case OpCode.Stloc_3: value = this.locals[3]; break;
                case OpCode.Ldarg_S:
                case OpCode.Ldarga_S:
                case OpCode.Starg_S:
                    value = this.Parameters(this.GetByte()); break;
                case OpCode.Ldloc_S:
                case OpCode.Ldloca_S:
                case OpCode.Stloc_S:
                    value = this.locals[this.GetByte()]; break;
                case OpCode.Ldnull:
                    break;
                case OpCode.Ldc_I4_M1: value = (Int32)(-1); break;
                case OpCode.Ldc_I4_0: value = (Int32)0; break;
                case OpCode.Ldc_I4_1: value = (Int32)1; break;
                case OpCode.Ldc_I4_2: value = (Int32)2; break;
                case OpCode.Ldc_I4_3: value = (Int32)3; break;
                case OpCode.Ldc_I4_4: value = (Int32)4; break;
                case OpCode.Ldc_I4_5: value = (Int32)5; break;
                case OpCode.Ldc_I4_6: value = (Int32)6; break;
                case OpCode.Ldc_I4_7: value = (Int32)7; break;
                case OpCode.Ldc_I4_8: value = (Int32)8; break;
                case OpCode.Ldc_I4_S: value = (Int32)this.GetSByte(); break;
                case OpCode.Ldc_I4: value = this.GetInt32(); break;
                case OpCode.Ldc_I8: value = this.GetInt64(); break;
                case OpCode.Ldc_R4: value = this.GetSingle(); break;
                case OpCode.Ldc_R8: value = this.GetDouble(); break;
                case OpCode.Dup:
                case OpCode.Pop:
                    break;
                case OpCode.Jmp:
                case OpCode.Call:
                    value = (Method)this.GetMemberFromToken(); break;
                case OpCode.Calli:
                    value = (FunctionPointer)this.reader.GetCalliSignature(this.GetInt32()); break;
                case OpCode.Ret: break;
                case OpCode.Br_S:
                case OpCode.Brfalse_S:
                case OpCode.Brtrue_S:
                case OpCode.Beq_S:
                case OpCode.Bge_S:
                case OpCode.Bgt_S:
                case OpCode.Ble_S:
                case OpCode.Blt_S:
                case OpCode.Bne_Un_S:
                case OpCode.Bge_Un_S:
                case OpCode.Bgt_Un_S:
                case OpCode.Ble_Un_S:
                case OpCode.Blt_Un_S:
                    value = this.counter + 1 + this.GetSByte(); break;
                case OpCode.Br:
                case OpCode.Brfalse:
                case OpCode.Brtrue:
                case OpCode.Beq:
                case OpCode.Bge:
                case OpCode.Bgt:
                case OpCode.Ble:
                case OpCode.Blt:
                case OpCode.Bne_Un:
                case OpCode.Bge_Un:
                case OpCode.Bgt_Un:
                case OpCode.Ble_Un:
                case OpCode.Blt_Un:
                    value = this.counter + 4 + this.GetInt32(); break;
                case OpCode.Switch:
                    value = this.ParseSwitchInstruction(); break;
                case OpCode.Ldind_I1:
                case OpCode.Ldind_U1:
                case OpCode.Ldind_I2:
                case OpCode.Ldind_U2:
                case OpCode.Ldind_I4:
                case OpCode.Ldind_U4:
                case OpCode.Ldind_I8:
                case OpCode.Ldind_I:
                case OpCode.Ldind_R4:
                case OpCode.Ldind_R8:
                case OpCode.Ldind_Ref:
                case OpCode.Stind_Ref:
                case OpCode.Stind_I1:
                case OpCode.Stind_I2:
                case OpCode.Stind_I4:
                case OpCode.Stind_I8:
                case OpCode.Stind_R4:
                case OpCode.Stind_R8:
                case OpCode.Add:
                case OpCode.Sub:
                case OpCode.Mul:
                case OpCode.Div:
                case OpCode.Div_Un:
                case OpCode.Rem:
                case OpCode.Rem_Un:
                case OpCode.And:
                case OpCode.Or:
                case OpCode.Xor:
                case OpCode.Shl:
                case OpCode.Shr:
                case OpCode.Shr_Un:
                case OpCode.Neg:
                case OpCode.Not:
                case OpCode.Conv_I1:
                case OpCode.Conv_I2:
                case OpCode.Conv_I4:
                case OpCode.Conv_I8:
                case OpCode.Conv_R4:
                case OpCode.Conv_R8:
                case OpCode.Conv_U4:
                case OpCode.Conv_U8:
                    break;
                case OpCode.Callvirt: value = (Method)this.GetMemberFromToken(); break;
                case OpCode.Cpobj:
                case OpCode.Ldobj:
                    value = (TypeNode)this.GetMemberFromToken(); break;
                case OpCode.Ldstr: value = this.GetStringFromToken(); break;
                case OpCode.Newobj: value = (Method)this.GetMemberFromToken(); break;
                case OpCode.Castclass:
                case OpCode.Isinst:
                    value = (TypeNode)this.GetMemberFromToken(); break;
                case OpCode.Conv_R_Un: break;
                case OpCode.Unbox: value = (TypeNode)this.GetMemberFromToken(); break;
                case OpCode.Throw: break;
                case OpCode.Ldfld:
                case OpCode.Ldflda:
                case OpCode.Stfld:
                case OpCode.Ldsfld:
                case OpCode.Ldsflda:
                case OpCode.Stsfld:
                case OpCode.Stobj:
                    value = this.GetMemberFromToken(); break;
                case OpCode.Conv_Ovf_I1_Un:
                case OpCode.Conv_Ovf_I2_Un:
                case OpCode.Conv_Ovf_I4_Un:
                case OpCode.Conv_Ovf_I8_Un:
                case OpCode.Conv_Ovf_U1_Un:
                case OpCode.Conv_Ovf_U2_Un:
                case OpCode.Conv_Ovf_U4_Un:
                case OpCode.Conv_Ovf_U8_Un:
                case OpCode.Conv_Ovf_I_Un:
                case OpCode.Conv_Ovf_U_Un:
                    break;
                case OpCode.Box:
                case OpCode.Newarr: value = (TypeNode)this.GetMemberFromToken(); break;
                case OpCode.Ldlen: break;
                case OpCode.Ldelema: value = (TypeNode)this.GetMemberFromToken(); break;
                case OpCode.Ldelem_I1:
                case OpCode.Ldelem_U1:
                case OpCode.Ldelem_I2:
                case OpCode.Ldelem_U2:
                case OpCode.Ldelem_I4:
                case OpCode.Ldelem_U4:
                case OpCode.Ldelem_I8:
                case OpCode.Ldelem_I:
                case OpCode.Ldelem_R4:
                case OpCode.Ldelem_R8:
                case OpCode.Ldelem_Ref:
                case OpCode.Stelem_I:
                case OpCode.Stelem_I1:
                case OpCode.Stelem_I2:
                case OpCode.Stelem_I4:
                case OpCode.Stelem_I8:
                case OpCode.Stelem_R4:
                case OpCode.Stelem_R8:
                case OpCode.Stelem_Ref:
                    break;
                case OpCode.Ldelem:
                    value = (TypeNode)this.GetMemberFromToken();
                    break;
                case OpCode.Stelem: value = (TypeNode)this.GetMemberFromToken(); break;
                case OpCode.Unbox_Any: value = this.GetMemberFromToken(); break;
                case OpCode.Conv_Ovf_I1:
                case OpCode.Conv_Ovf_U1:
                case OpCode.Conv_Ovf_I2:
                case OpCode.Conv_Ovf_U2:
                case OpCode.Conv_Ovf_I4:
                case OpCode.Conv_Ovf_U4:
                case OpCode.Conv_Ovf_I8:
                case OpCode.Conv_Ovf_U8:
                    break;
                case OpCode.Refanyval: value = this.GetMemberFromToken(); break;
                case OpCode.Ckfinite: break;
                case OpCode.Mkrefany: value = this.GetMemberFromToken(); break;
                case OpCode.Ldtoken: value = this.GetMemberFromToken(); break;
                case OpCode.Conv_U2:
                case OpCode.Conv_U1:
                case OpCode.Conv_I:
                case OpCode.Conv_Ovf_I:
                case OpCode.Conv_Ovf_U:
                case OpCode.Add_Ovf:
                case OpCode.Add_Ovf_Un:
                case OpCode.Mul_Ovf:
                case OpCode.Mul_Ovf_Un:
                case OpCode.Sub_Ovf:
                case OpCode.Sub_Ovf_Un:
                case OpCode.Endfinally:
                    break;
                case OpCode.Leave: value = this.counter + 4 + this.GetInt32(); break;
                case OpCode.Leave_S: value = this.counter + 1 + this.GetSByte(); break;
                case OpCode.Stind_I:
                case OpCode.Conv_U:
                case OpCode.Prefix7:
                case OpCode.Prefix6:
                case OpCode.Prefix5:
                case OpCode.Prefix4:
                case OpCode.Prefix3:
                case OpCode.Prefix2:
                case OpCode.Prefix1:
                case OpCode.Arglist:
                case OpCode.Ceq:
                case OpCode.Cgt:
                case OpCode.Cgt_Un:
                case OpCode.Clt:
                case OpCode.Clt_Un:
                    break;
                case OpCode.Ldftn:
                case OpCode.Ldvirtftn:
                    value = this.GetMemberFromToken(); break;
                case OpCode.Ldarg:
                case OpCode.Ldarga:
                case OpCode.Starg:
                    value = this.Parameters(this.GetInt16()); break;
                case OpCode.Ldloc:
                case OpCode.Ldloca:
                case OpCode.Stloc:
                    value = this.locals[this.GetInt16()]; break;
                case OpCode.Localloc:
                case OpCode.Endfilter:
                    break;
                case OpCode.Unaligned_: value = this.GetByte(); break;
                case OpCode.Volatile_:
                case OpCode.Tail_:
                    break;
                case OpCode.Initobj: value = (TypeNode)this.GetMemberFromToken(); break;
                case OpCode.Constrained_: value = this.GetMemberFromToken() as TypeNode; break;
                case OpCode.Cpblk:
                case OpCode.Initblk:
                    break;
                case OpCode.Rethrow:
                    break;
                case OpCode.Sizeof: value = this.GetMemberFromToken(); break;
                case OpCode.Refanytype:
                case OpCode.Readonly_:
                    break;
                default: throw new InvalidMetadataException(String.Format(CultureInfo.CurrentCulture,
                  ExceptionStrings.UnknownOpCodeEncountered, opCode.ToString("x")));
            }
            Instruction instruction = new Instruction(opCode, offset, value);
            instruction.SourceContext = this.sourceContext;
            return instruction;
        }
    }

    internal class ExpressionStack
    {
        internal Expression[]/*!*/ elements = new Expression[64];
        internal int top = -1;

        internal ExpressionStack()
        {
            //^ base();
        }

        private void Grow()
        {
            int n = this.elements.Length;
            Expression[] newElements = new Expression[n + 64];
            for (int i = 0; i < n; i++) newElements[i] = this.elements[i];
            this.elements = newElements;
        }
        internal Expression/*!*/ Pop()
        {
            if (this.top < 0) return new Expression(NodeType.Pop);
            Expression e = this.elements[this.top--];
            //^ assume e != null;
            return e;
        }
        internal void Push(Expression/*!*/ e)
        {
            if (++this.top >= this.elements.Length) this.Grow();
            this.elements[this.top] = e;
        }
    }

    /// <summary>
    /// A thin wrapper for a synchronized System.Collections.Hashtable that inserts and strips WeakReference wrappers for the values stored in the table.
    /// </summary>
    internal class SynchronizedWeakDictionary : IDictionary
    {
        private Hashtable/*!*/ Hashtable = System.Collections.Hashtable.Synchronized(new Hashtable());

        internal SynchronizedWeakDictionary()
        {
            //^ base();
        }

        public void Add(object/*!*/ key, object value)
        {
            this.Hashtable.Add(key, new WeakReference(value));
        }
        public void Clear()
        {
            this.Hashtable.Clear();
        }
        public bool Contains(object/*!*/ key)
        {
            return this.Hashtable.Contains(key);
        }
        public IDictionaryEnumerator/*!*/ GetEnumerator()
        {
            return this.Hashtable.GetEnumerator();
        }
        public bool IsFixedSize
        {
            get { return false; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public ICollection/*!*/ Keys
        {
            get { return this.Hashtable.Keys; }
        }
        public void Remove(object/*!*/ key)
        {
            this.Hashtable.Remove(key);
        }
        public ICollection/*!*/ Values
        {
            get { return new WeakValuesCollection(this.Hashtable.Values); }
        }
        public object this[object/*!*/ key]
        {
            get
            {
                WeakReference wref = (WeakReference)this.Hashtable[key];
                if (wref == null) return null;
                return wref.Target;
            }
            set
            {
                this.Hashtable[key] = new WeakReference(value);
            }
        }
        public void CopyTo(Array/*!*/ array, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); i++)
                array.SetValue(enumerator.Current, index + i);
        }
        public int Count
        {
            get { return this.Hashtable.Count; }
        }
        public bool IsSynchronized
        {
            get { return false; }
        }
        public object/*!*/ SyncRoot
        {
            get { return this.Hashtable.SyncRoot; }
        }
        IEnumerator/*!*/ IEnumerable.GetEnumerator()
        {
            return new WeakValuesEnumerator(this.Hashtable.GetEnumerator());
        }
    }

    internal class WeakValuesCollection : ICollection
    {
        private ICollection/*!*/ collection;

        internal WeakValuesCollection(ICollection/*!*/ collection)
        {
            this.collection = collection;
            //^ base();
        }

        public void CopyTo(Array/*!*/ array, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); i++)
                array.SetValue(enumerator.Current, index + i);
        }
        public int Count
        {
            get { return this.collection.Count; }
        }
        public bool IsSynchronized
        {
            get { return this.collection.IsSynchronized; }
        }
        public object/*!*/ SyncRoot
        {
            get { return this.collection.SyncRoot; }
        }
        public IEnumerator/*!*/ GetEnumerator()
        {
            return new WeakValuesEnumerator(this.collection.GetEnumerator());
        }
    }

    internal class WeakValuesEnumerator : IEnumerator
    {
        private IEnumerator/*!*/ enumerator;

        internal WeakValuesEnumerator(IEnumerator/*!*/ enumerator)
        {
            this.enumerator = enumerator;
            //^ base();
        }

        public object Current
        {
            get
            {
                object curr = this.enumerator.Current;
                if (curr is DictionaryEntry)
                {
                    DictionaryEntry dicEntry = (DictionaryEntry)curr;
                    curr = dicEntry.Value;
                }
                WeakReference wref = curr as WeakReference;
                if (wref != null) return wref.Target;
                return null;
            }
        }
        public bool MoveNext()
        {
            return this.enumerator.MoveNext();
        }
        public void Reset()
        {
            this.enumerator.Reset();
        }
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
    internal interface IMetaDataImport
    {
    }

    internal class EmptyImporter : IMetaDataImport
    {
    }
}
