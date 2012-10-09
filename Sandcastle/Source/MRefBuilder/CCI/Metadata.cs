// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
#if FxCop
using Win32ResourceList = Microsoft.Cci.Win32ResourceCollection;
using TypeNodeList = Microsoft.Cci.TypeNodeCollection;
#endif
#if CCINamespace
using Microsoft.Cci;
#else
using System.Compiler;
#endif
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;

//^ using Microsoft.Contracts;

/* These classes help with parsing and producing PE files. They are best understood in conjunction with the ECMA 335 Specification
 * (Common Language Infrastructure), particularly Partition II. Also see "Inside Microsoft .NET IL Assembler" by Serge Lidin. */

#if CCINamespace
namespace Microsoft.Cci.Metadata{
#else
namespace System.Compiler.Metadata
{
#endif
    internal struct AssemblyRow
    {
        internal int HashAlgId;
        internal int MajorVersion;
        internal int MinorVersion;
        internal int BuildNumber;
        internal int RevisionNumber;
        internal int Flags;
        internal int PublicKey;
        internal int Name;
        internal int Culture;
    }
    internal struct AssemblyRefRow
    {
        internal int MajorVersion;
        internal int MinorVersion;
        internal int BuildNumber;
        internal int RevisionNumber;
        internal int Flags;
        internal int PublicKeyOrToken;
        internal int Name;
        internal int Culture;
        internal int HashValue;
        internal AssemblyReference AssemblyReference;
    }
    internal struct ClassLayoutRow
    {
        internal int PackingSize;
        internal int ClassSize;
        internal int Parent;
    }
    internal struct ConstantRow
    {
        internal int Type;
        internal int Parent;
        internal int Value;
    }
    internal struct CustomAttributeRow
    {
        internal int Parent;
        internal int Constructor;
        internal int Value;
    }
    internal struct DeclSecurityRow
    {
        internal int Action;
        internal int Parent;
        internal int PermissionSet;
    }
    internal struct EventMapRow
    {
        internal int Parent;
        internal int EventList;
    }
    internal struct EventPtrRow
    {
        internal int Event;
    }
    internal struct EventRow
    {
        internal int Flags;
        internal int Name;
        internal int EventType;
    }
    internal struct ExportedTypeRow
    {
        internal int Flags;
        internal int TypeDefId;
        internal int TypeName;
        internal int TypeNamespace;
        internal int Implementation;
    }
    internal struct FieldRow
    {
        internal int Flags;
        internal int Name;
        internal int Signature;
        internal Field Field;
    }
    internal struct FieldLayoutRow
    {
        internal int Offset;
        internal int Field;
    }
    internal struct FieldMarshalRow
    {
        internal int Parent;
        internal int NativeType;
    }
    internal struct FieldPtrRow
    {
        internal int Field;
    }
    internal struct FieldRvaRow
    {
        internal int RVA;
        internal int Field;
        internal PESection TargetSection;
    }
    internal struct FileRow
    {
        internal int Flags;
        internal int Name;
        internal int HashValue;
    }
    internal struct GenericParamRow
    {
        internal int Number;
        internal int Flags;
        internal int Owner;
        internal int Name;
        internal Member GenericParameter;
    }
    internal struct GenericParamConstraintRow
    {
        internal int Param;
        internal int Constraint;
    }
    internal struct ImplMapRow
    {
        internal int MappingFlags;
        internal int MemberForwarded;
        internal int ImportName;
        internal int ImportScope;
    }
    internal struct InterfaceImplRow
    {
        internal int Class;
        internal int Interface;
    }
    internal struct ManifestResourceRow
    {
        internal int Offset;
        internal int Flags;
        internal int Name;
        internal int Implementation;
    }
    internal struct MemberRefRow
    {
        internal int Class;
        internal int Name;
        internal int Signature;
        internal Member Member;
        internal TypeNodeList VarargTypes;
    }
    internal struct MethodRow
    {
        internal int RVA;
        internal int ImplFlags;
        internal int Flags;
        internal int Name;
        internal int Signature;
        internal int ParamList;
        internal Method Method;
    }
    internal struct MethodImplRow
    {
        internal int Class;
        internal int MethodBody;
        internal int MethodDeclaration;
    }
    internal struct MethodPtrRow
    {
        internal int Method;
    }
    internal struct MethodSemanticsRow
    {
        internal int Semantics;
        internal int Method;
        internal int Association;
    }
    internal struct MethodSpecRow
    {
        internal int Method;
        internal int Instantiation;
        internal Method InstantiatedMethod;
    }
    internal struct ModuleRow
    {
        internal int Generation;
        internal int Name;
        internal int Mvid;
        internal int EncId;
        internal int EncBaseId;
    }
    internal struct ModuleRefRow
    {
        internal int Name;
#if FxCop
    internal ModuleNode Module;
#else
        internal Module Module;
#endif
    }
    internal struct NestedClassRow
    {
        internal int NestedClass;
        internal int EnclosingClass;
    }
    internal struct ParamRow
    {
        internal int Flags;
        internal int Sequence;
        internal int Name;
    }
    internal struct ParamPtrRow
    {
        internal int Param;
    }
    internal struct PropertyRow
    {
        internal int Flags;
        internal int Name;
        internal int Signature;
    }
    internal struct PropertyPtrRow
    {
        internal int Property;
    }
    internal struct PropertyMapRow
    {
        internal int Parent;
        internal int PropertyList;
    }
    internal struct StandAloneSigRow
    {
        internal int Signature;
    }
    internal struct TypeDefRow
    {
        internal int Flags;
        internal int Name;
        internal int Namespace;
        internal int Extends;
        internal int FieldList;
        internal int MethodList;
        internal TypeNode Type;
        internal Identifier NamespaceId;
        internal int NamespaceKey;
        internal int NameKey;
    }
    internal struct TypeRefRow
    {
        internal int ResolutionScope;
        internal int Name;
        internal int Namespace;
        internal TypeNode Type;
    }
    internal struct TypeSpecRow
    {
        internal int Signature;
        internal TypeNode Type;
    }
    [Serializable]
    public sealed class InvalidMetadataException : System.Exception
    {
        public InvalidMetadataException() { }
        public InvalidMetadataException(string message)
            : base(message)
        {
        }
        public InvalidMetadataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        private InvalidMetadataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    internal class CLIHeader
    {
        internal int cb;
        internal ushort majorRuntimeVersion;
        internal ushort minorRuntimeVersion;
        internal DirectoryEntry metaData;
        internal int flags;
        internal int entryPointToken;
        internal DirectoryEntry resources;
        internal DirectoryEntry strongNameSignature;
        internal DirectoryEntry codeManagerTable;
        internal DirectoryEntry vtableFixups;
        internal DirectoryEntry exportAddressTableJumps;

        internal CLIHeader()
        {
            this.cb = 72;
            this.majorRuntimeVersion = 2;
            this.minorRuntimeVersion = 5;
            // initialization provided by runtime
            //this.flags = 0;
            //this.entryPointToken = 0;
        }
    }
    internal struct DirectoryEntry
    {
        internal int virtualAddress;
        internal int size;
    }
    internal class MetadataHeader
    {
        internal int signature;
        internal ushort majorVersion;
        internal ushort minorVersion;
        internal int reserved;
        internal string versionString;
        internal int flags;
        internal StreamHeader[] streamHeaders;
    }
    internal class NTHeader
    {
        internal int signature;
        internal ushort machine;
        internal ushort numberOfSections;
        internal int timeDateStamp;
        internal int pointerToSymbolTable;
        internal int numberOfSymbols;
        internal ushort sizeOfOptionalHeader;
        internal ushort characteristics;
        internal ushort magic;
        internal byte majorLinkerVersion;
        internal byte minorLinkerVersion;
        internal int sizeOfCode;
        internal int sizeOfInitializedData;
        internal int sizeOfUninitializedData;
        internal int addressOfEntryPoint;
        internal int baseOfCode;
        internal int baseOfData;
        internal long imageBase;
        internal int sectionAlignment;
        internal int fileAlignment;
        internal ushort majorOperatingSystemVersion;
        internal ushort minorOperatingSystemVersion;
        internal ushort majorImageVersion;
        internal ushort minorImageVersion;
        internal ushort majorSubsystemVersion;
        internal ushort minorSubsystemVersion;
        internal int win32VersionValue;
        internal int sizeOfImage;
        internal int sizeOfHeaders;
        internal int checkSum;
        internal ushort subsystem;
        internal ushort dllCharacteristics;
        internal long sizeOfStackReserve;
        internal long sizeOfStackCommit;
        internal long sizeOfHeapReserve;
        internal long sizeOfHeapCommit;
        internal int loaderFlags;
        internal int numberOfDataDirectories;
        internal DirectoryEntry exportTable;
        internal DirectoryEntry importTable;
        internal DirectoryEntry resourceTable;
        internal DirectoryEntry exceptionTable;
        internal DirectoryEntry certificateTable;
        internal DirectoryEntry baseRelocationTable;
        internal DirectoryEntry debugTable;
        internal DirectoryEntry copyrightTable;
        internal DirectoryEntry globalPointerTable;
        internal DirectoryEntry threadLocalStorageTable;
        internal DirectoryEntry loadConfigTable;
        internal DirectoryEntry boundImportTable;
        internal DirectoryEntry importAddressTable;
        internal DirectoryEntry delayImportTable;
        internal DirectoryEntry cliHeaderTable;
        internal DirectoryEntry reserved;

        internal NTHeader()
        {
            this.signature = 0x00004550; /* "PE\0\0" */
            this.machine = 0x14c;
            this.sizeOfOptionalHeader = 224;
            this.characteristics = 0x0002 | 0x0004 | 0x008 | 0x0100;
            this.magic = 0x10B;
            this.majorLinkerVersion = 6;
            this.baseOfCode = 0x2000;
            this.imageBase = 0x400000; //TODO: make this settable
            this.sectionAlignment = 8192;
            this.fileAlignment = 512;
            this.majorOperatingSystemVersion = 4;
            this.majorSubsystemVersion = 4;
            this.dllCharacteristics = 0x400;
            this.sizeOfStackReserve = 1048576;
            this.sizeOfStackCommit = 4096;
            this.sizeOfHeapReserve = 1048576;
            this.sizeOfHeapCommit = 4096;
            this.numberOfDataDirectories = 16;

            // initialization provided by runtime
            //this.numberOfSections = 0;
            //this.timeDateStamp = 0;
            //this.pointerToSymbolTable = 0;
            //this.numberOfSymbols = 0;
            //this.minorLinkerVersion = 0;
            //this.sizeOfCode = 0;
            //this.sizeOfInitializedData = 0;
            //this.sizeOfUninitializedData = 0;
            //this.addressOfEntryPoint = 0;
            //this.baseOfData = 0;
            //this.minorOperatingSystemVersion = 0;
            //this.majorImageVersion = 0;
            //this.minorImageVersion = 0;
            //this.minorSubsystemVersion = 0;
            //this.win32VersionValue = 0;
            //this.sizeOfImage = 0;
            //this.sizeOfHeaders = 0;
            //this.checkSum = 0;
            //this.subsystem = 0;
            //this.loaderFlags = 0x0;
        }
    }
    internal struct SectionHeader
    {
        internal string name;
        internal int virtualSize;
        internal int virtualAddress;
        internal int sizeOfRawData;
        internal int pointerToRawData;
        internal int pointerToRelocations;
        internal int pointerToLinenumbers;
        internal ushort numberOfRelocations;
        internal ushort numberOfLinenumbers;
        internal int characteristics;
    }
    internal class StreamHeader
    {
        internal int offset;
        internal int size;
        internal String name;
    }
    internal class TablesHeader
    {
        internal int reserved;
        internal byte majorVersion;
        internal byte minorVersion;
        internal byte heapSizes;
        internal byte rowId;
        internal long maskValid;
        internal long maskSorted;
        internal int[] countArray;
    }
    internal enum TableIndices
    {
        Module = 0x00,
        TypeRef = 0x01,
        TypeDef = 0x02,
        FieldPtr = 0x03,
        Field = 0x04,
        MethodPtr = 0x05,
        Method = 0x06,
        ParamPtr = 0x07,
        Param = 0x08,
        InterfaceImpl = 0x09,
        MemberRef = 0x0A,
        Constant = 0x0B,
        CustomAttribute = 0x0C,
        FieldMarshal = 0x0D,
        DeclSecurity = 0x0E,
        ClassLayout = 0x0F,
        FieldLayout = 0x10,
        StandAloneSig = 0x11,
        EventMap = 0x12,
        EventPtr = 0x13,
        Event = 0x14,
        PropertyMap = 0x15,
        PropertyPtr = 0x16,
        Property = 0x17,
        MethodSemantics = 0x18,
        MethodImpl = 0x19,
        ModuleRef = 0x1A,
        TypeSpec = 0x1B,
        ImplMap = 0x1C,
        FieldRva = 0x1D,
        EncLog = 0x1E,
        EncMap = 0x1F,
        Assembly = 0x20,
        AssemblyProcessor = 0x21,
        AssemblyOS = 0x22,
        AssemblyRef = 0x23,
        AssemblyRefProcessor = 0x24,
        AssemblyRefOS = 0x25,
        File = 0x26,
        ExportedType = 0x27,
        ManifestResource = 0x28,
        NestedClass = 0x29,
        GenericParam = 0x2a,
        MethodSpec = 0x2b,
        GenericParamConstraint = 0x2c,
        Count
    }
    internal enum ElementType
    {
        End = 0x00,
        Void = 0x01,
        Boolean = 0x02,
        Char = 0x03,
        Int8 = 0x04,
        UInt8 = 0x05,
        Int16 = 0x06,
        UInt16 = 0x07,
        Int32 = 0x08,
        UInt32 = 0x09,
        Int64 = 0x0a,
        UInt64 = 0x0b,
        Single = 0x0c,
        Double = 0x0d,
        String = 0x0e,
        Pointer = 0x0f,
        Reference = 0x10,
        ValueType = 0x11,
        Class = 0x12,
        TypeParameter = 0x13,
        Array = 0x14,
        GenericTypeInstance = 0x15,
        DynamicallyTypedReference = 0x16,
        IntPtr = 0x18,
        UIntPtr = 0x19,
        FunctionPointer = 0x1b,
        Object = 0x1c,
        SzArray = 0x1d,
        MethodParameter = 0x1e,
        RequiredModifier = 0x1f,
        OptionalModifier = 0x20,
        Internal = 0x21,
        Modifier = 0x40,
        Sentinel = 0x41,
        Pinned = 0x45,
        Type = 0x50,
        BoxedEnum = 0x51,
        Enum = 0x55
    }

    unsafe internal class MetadataReader : IDisposable
    {
#if !ROTOR
        private MemoryMappedFile memmap;
#endif
        private MemoryCursor/*!*/ cursor;
        internal int entryPointToken;
        internal int fileAlignment;
        internal ModuleKindFlags moduleKind;
        internal PEKindFlags peKind;
        internal bool TrackDebugData;
        private int mdOffset;
        private int resourcesOffset;
        private int win32ResourcesOffset;
        private SectionHeader[] sectionHeaders;
        //^ [SpecPublic]
        private StreamHeader identifierStringHeap;
        //^ [SpecPublic]
        private StreamHeader generalStringHeap;
        private StreamHeader blobHeap;
        //^ [SpecPublic]
        private StreamHeader guidHeap;
        private StreamHeader tables;
        internal TablesHeader tablesHeader;
        internal string targetRuntimeVersion;
        internal int linkerMajorVersion;
        internal int linkerMinorVersion;
        internal int metadataFormatMajorVersion;
        internal int metadataFormatMinorVersion;
        private int blobRefSize;
        private int constantParentRefSize;
        private int customAttributeParentRefSize;
        private int customAttributeConstructorRefSize;
        private int declSecurityParentRefSize;
        private int fieldMarshalParentRefSize;
        private int guidRefSize;
        private int hasSemanticRefSize;
        private int implementationRefSize;
        private int methodDefOrRefSize;
        private int memberRefParentSize;
        private int memberForwardedRefSize;
        private int typeDefOrRefOrSpecSize;
        private int typeDefOrMethodDefSize;
        private int resolutionScopeRefSize;
        private int stringRefSize;
        private int[] tableSize;
        private int[] tableRefSize;
        private int[] tableOffset;
        internal byte[] HashValue;

#if !ROTOR
        internal MetadataReader(string path)
        {
            MemoryMappedFile memmap = this.memmap = new MemoryMappedFile(path);
            try
            {
                this.cursor = new MemoryCursor(memmap);
                //^ base();
                ReadHeader();
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }
#endif

        internal MetadataReader(byte* buffer, int length)
        {
            this.cursor = new MemoryCursor(buffer, length);
            //^ base();
            ReadHeader();
        }

        public void Dispose()
        {
#if !ROTOR
            if (this.memmap != null) this.memmap.Dispose();
            this.memmap = null;
#endif
            //this.cursor = null;
            this.sectionHeaders = null;
            this.identifierStringHeap = null;
            this.generalStringHeap = null;
            this.blobHeap = null;
            this.guidHeap = null;
            this.tables = null;
            this.tablesHeader = null;
            this.targetRuntimeVersion = null;
            this.tableSize = null;
            this.tableRefSize = null;
            this.tableOffset = null;
            this.HashValue = null;
        }

        private AssemblyRow[] assemblyTable;
        private AssemblyRefRow[] assemblyRefTable;
        private ClassLayoutRow[] classLayoutTable;
        private ConstantRow[] constantTable;
        private CustomAttributeRow[] customAttributeTable;
        private DeclSecurityRow[] declSecurityTable;
        private EventMapRow[] eventMapTable;
        private EventPtrRow[] eventPtrTable;
        private EventRow[] eventTable;
        private ExportedTypeRow[] exportedTypeTable;
        private FieldRow[] fieldTable;
        private FieldLayoutRow[] fieldLayoutTable;
        private FieldMarshalRow[] fieldMarshalTable;
        private FieldPtrRow[] fieldPtrTable;
        private FieldRvaRow[] fieldRvaTable;
        private FileRow[] fileTable;
        private GenericParamRow[] genericParamTable;
        private GenericParamConstraintRow[] genericParamConstraintTable;
        private ImplMapRow[] implMapTable;
        private InterfaceImplRow[] interfaceImplTable;
        private ManifestResourceRow[] manifestResourceTable;
        private MemberRefRow[] memberRefTable;
        private MethodRow[] methodTable;
        private MethodPtrRow[] methodPtrTable;
        private MethodImplRow[] methodImplTable;
        private MethodSemanticsRow[] methodSemanticsTable;
        private MethodSpecRow[] methodSpecTable;
        private ModuleRow[] moduleTable;
        private ModuleRefRow[] moduleRefTable;
        private NestedClassRow[] nestedClassTable;
        private ParamRow[] paramTable;
        private ParamPtrRow[] paramPtrTable;
        private PropertyRow[] propertyTable;
        private PropertyMapRow[] propertyMapTable;
        private PropertyPtrRow[] propertyPtrTable;
        private StandAloneSigRow[] standAloneSigTable;
        private TypeDefRow[] typeDefTable;
        private TypeRefRow[] typeRefTable;
        private TypeSpecRow[] typeSpecTable;

        internal AssemblyRow[]/*!*/ AssemblyTable
        {
            get { if (this.assemblyTable == null) this.ReadAssemblyTable(); return this.assemblyTable; }
        }
        internal AssemblyRefRow[]/*!*/ AssemblyRefTable
        {
            get { if (this.assemblyRefTable == null) this.ReadAssemblyRefTable(); return this.assemblyRefTable; }
        }
        internal ClassLayoutRow[]/*!*/ ClassLayoutTable
        {
            get { if (this.classLayoutTable == null) this.ReadClassLayoutTable(); return this.classLayoutTable; }
        }
        internal ConstantRow[]/*!*/ ConstantTable
        {
            get { if (this.constantTable == null) this.ReadConstantTable(); return this.constantTable; }
        }
        internal CustomAttributeRow[]/*!*/ CustomAttributeTable
        {
            get { if (this.customAttributeTable == null) this.ReadCustomAttributeTable(); return this.customAttributeTable; }
        }
        internal DeclSecurityRow[]/*!*/ DeclSecurityTable
        {
            get { if (this.declSecurityTable == null) this.ReadDeclSecurityTable(); return this.declSecurityTable; }
        }
        internal EventMapRow[]/*!*/ EventMapTable
        {
            get { if (this.eventMapTable == null) this.ReadEventMapTable(); return this.eventMapTable; }
        }
        internal EventPtrRow[]/*!*/ EventPtrTable
        {
            get { if (this.eventPtrTable == null) this.ReadEventPtrTable(); return this.eventPtrTable; }
        }
        internal EventRow[]/*!*/ EventTable
        {
            get { if (this.eventTable == null) this.ReadEventTable(); return this.eventTable; }
        }
        internal ExportedTypeRow[]/*!*/ ExportedTypeTable
        {
            get { if (this.exportedTypeTable == null) this.ReadExportedTypeTable(); return this.exportedTypeTable; }
        }
        internal FieldRow[]/*!*/ FieldTable
        {
            get { if (this.fieldTable == null) this.ReadFieldTable(); return this.fieldTable; }
        }
        internal FieldLayoutRow[]/*!*/ FieldLayoutTable
        {
            get { if (this.fieldLayoutTable == null) this.ReadFieldLayoutTable(); return this.fieldLayoutTable; }
        }
        internal FieldMarshalRow[]/*!*/ FieldMarshalTable
        {
            get { if (this.fieldMarshalTable == null) this.ReadFieldMarshalTable(); return this.fieldMarshalTable; }
        }
        internal FieldPtrRow[]/*!*/ FieldPtrTable
        {
            get { if (this.fieldPtrTable == null) this.ReadFieldPtrTable(); return this.fieldPtrTable; }
        }
        internal FieldRvaRow[]/*!*/ FieldRvaTable
        {
            get { if (this.fieldRvaTable == null) this.ReadFieldRvaTable(); return this.fieldRvaTable; }
        }
        internal FileRow[]/*!*/ FileTable
        {
            get { if (this.fileTable == null) this.ReadFileTable(); return this.fileTable; }
        }
        internal GenericParamRow[]/*!*/ GenericParamTable
        {
            get { if (this.genericParamTable == null) this.ReadGenericParamTable(); return this.genericParamTable; }
        }
        internal GenericParamConstraintRow[]/*!*/ GenericParamConstraintTable
        {
            get { if (this.genericParamConstraintTable == null) this.ReadGenericParamConstraintTable(); return this.genericParamConstraintTable; }
        }
        internal ImplMapRow[]/*!*/ ImplMapTable
        {
            get { if (this.implMapTable == null) this.ReadImplMapTable(); return this.implMapTable; }
        }
        internal InterfaceImplRow[]/*!*/ InterfaceImplTable
        {
            get { if (this.interfaceImplTable == null) this.ReadInterfaceImplTable(); return this.interfaceImplTable; }
        }
        internal ManifestResourceRow[]/*!*/ ManifestResourceTable
        {
            get { if (this.manifestResourceTable == null) this.ReadManifestResourceTable(); return this.manifestResourceTable; }
        }
        internal MemberRefRow[]/*!*/ MemberRefTable
        {
            get { if (this.memberRefTable == null) this.ReadMemberRefTable(); return this.memberRefTable; }
        }
        internal MethodRow[]/*!*/ MethodTable
        {
            get { if (this.methodTable == null) this.ReadMethodTable(); return this.methodTable; }
        }
        internal MethodImplRow[]/*!*/ MethodImplTable
        {
            get { if (this.methodImplTable == null) this.ReadMethodImplTable(); return this.methodImplTable; }
        }
        internal MethodPtrRow[]/*!*/ MethodPtrTable
        {
            get { if (this.methodPtrTable == null) this.ReadMethodPtrTable(); return this.methodPtrTable; }
        }
        internal MethodSemanticsRow[]/*!*/ MethodSemanticsTable
        {
            get { if (this.methodSemanticsTable == null) this.ReadMethodSemanticsTable(); return this.methodSemanticsTable; }
        }
        internal MethodSpecRow[]/*!*/ MethodSpecTable
        {
            get { if (this.methodSpecTable == null) this.ReadMethodSpecTable(); return this.methodSpecTable; }
        }
        internal ModuleRow[]/*!*/ ModuleTable
        {
            get { if (this.moduleTable == null) this.ReadModuleTable(); return this.moduleTable; }
        }
        internal ModuleRefRow[]/*!*/ ModuleRefTable
        {
            get { if (this.moduleRefTable == null) this.ReadModuleRefTable(); return this.moduleRefTable; }
        }
        internal NestedClassRow[]/*!*/ NestedClassTable
        {
            get { if (this.nestedClassTable == null) this.ReadNestedClassTable(); return this.nestedClassTable; }
        }
        internal ParamRow[]/*!*/ ParamTable
        {
            get { if (this.paramTable == null) this.ReadParamTable(); return this.paramTable; }
        }
        internal ParamPtrRow[]/*!*/ ParamPtrTable
        {
            get { if (this.paramPtrTable == null) this.ReadParamPtrTable(); return this.paramPtrTable; }
        }
        internal PropertyRow[]/*!*/ PropertyTable
        {
            get { if (this.propertyTable == null) this.ReadPropertyTable(); return this.propertyTable; }
        }
        internal PropertyMapRow[]/*!*/ PropertyMapTable
        {
            get { if (this.propertyMapTable == null) this.ReadPropertyMapTable(); return this.propertyMapTable; }
        }
        internal PropertyPtrRow[]/*!*/ PropertyPtrTable
        {
            get { if (this.propertyPtrTable == null) this.ReadPropertyPtrTable(); return this.propertyPtrTable; }
        }
        internal StandAloneSigRow[]/*!*/ StandAloneSigTable
        {
            get { if (this.standAloneSigTable == null) this.ReadStandAloneSigTable(); return this.standAloneSigTable; }
        }
        internal TypeDefRow[]/*!*/ TypeDefTable
        {
            get { if (this.typeDefTable == null) this.ReadTypeDefTable(); return this.typeDefTable; }
        }
        internal TypeRefRow[]/*!*/ TypeRefTable
        {
            get { if (this.typeRefTable == null) this.ReadTypeRefTable(); return this.typeRefTable; }
        }
        internal TypeSpecRow[]/*!*/ TypeSpecTable
        {
            get { if (this.typeSpecTable == null) this.ReadTypeSpecTable(); return this.typeSpecTable; }
        }

        internal void SetCurrentPosition(int pos)
        {
            this.cursor.Position = pos;
        }
        internal void AlignTo32BitBoundary()
        {
            this.cursor.Align(4);
        }
        internal void Skip(int bytes)
        {
            this.cursor.SkipByte(bytes);
        }
        internal byte[]/*!*/ GetBlob(int blobIndex)
        {
            MemoryCursor c = this.cursor;
            c.Position = PositionOfBlob(blobIndex);
            return c.ReadBytes(c.ReadCompressedInt());
        }
        internal MemoryCursor/*!*/ GetBlobCursor(int blobIndex)
        {
            MemoryCursor c = this.cursor;
            c.Position = PositionOfBlob(blobIndex);
            c.ReadCompressedInt();
            return new MemoryCursor(c);
        }
        internal MemoryCursor/*!*/ GetBlobCursor(int blobIndex, out int blobLength)
        {
            MemoryCursor c = this.cursor;
            c.Position = PositionOfBlob(blobIndex);
            blobLength = c.ReadCompressedInt();
            return new MemoryCursor(c);
        }
        internal System.Guid GetGuid(int guidIndex)
        //^ requires this.guidHeap != null;
        {
            int guidOffset = guidIndex * 16;
            if (guidOffset < 16 || this.guidHeap.size < guidOffset)
                throw new System.ArgumentOutOfRangeException("guidIndex", ExceptionStrings.BadGuidHeapIndex);
            MemoryCursor c = this.cursor;
            c.Position = this.mdOffset + this.guidHeap.offset + guidOffset - 16;
            return new System.Guid(c.ReadBytes(16));
        }
        internal Identifier/*!*/ GetIdentifier(int stringHeapIndex)
        //^ requires this.identifierStringHeap != null;
        {
            int position = this.mdOffset + this.identifierStringHeap.offset + stringHeapIndex;
            MemoryCursor c = this.cursor;
            return Identifier.For(c.GetBuffer(), position/*, c.KeepAlive*/);
        }
        internal byte GetMethodBodyHeaderByte(int RVA)
        {
            MemoryCursor c = this.cursor;
            c.Position = this.RvaToOffset(RVA);
            return c.ReadByte();
        }
        internal MemoryCursor/*!*/ GetNewCursor()
        {
            return new MemoryCursor(this.cursor);
        }
        internal MemoryCursor/*!*/ GetNewCursor(int RVA, out PESection targetSection)
        {
            MemoryCursor c = new MemoryCursor(this.cursor);
            c.Position = this.RvaToOffset(RVA, out targetSection);
            return c;
        }
        internal byte GetByte()
        {
            MemoryCursor c = this.cursor;
            return c.ReadByte();
        }
        internal int GetCurrentPosition()
        {
            return this.cursor.Position;
        }
        internal int GetInt32()
        {
            MemoryCursor c = this.cursor;
            return c.ReadInt32();
        }
        internal short GetInt16()
        {
            MemoryCursor c = this.cursor;
            return c.ReadInt16();
        }
        internal ushort GetUInt16()
        {
            MemoryCursor c = this.cursor;
            return c.ReadUInt16();
        }
        internal int GetSignatureLength(int blobIndex)
        {
            MemoryCursor c = this.cursor;
            c.Position = this.PositionOfBlob(blobIndex);
            return c.ReadCompressedInt();
        }
        internal string/*!*/ GetString(int stringHeapIndex)
        //^ requires this.identifierStringHeap != null;
        {
            if (stringHeapIndex < 0 || this.identifierStringHeap.size <= stringHeapIndex)
                throw new System.ArgumentOutOfRangeException("stringHeapIndex", ExceptionStrings.BadStringHeapIndex);
            MemoryCursor c = this.cursor;
            c.Position = this.mdOffset + this.identifierStringHeap.offset + stringHeapIndex;
            return c.ReadUTF8();
        }
        internal string/*!*/ GetUserString(int stringHeapIndex)
        //^ requires this.generalStringHeap != null;
        {
            if (stringHeapIndex < 0 || this.generalStringHeap.size <= stringHeapIndex)
                throw new System.ArgumentOutOfRangeException("stringHeapIndex", ExceptionStrings.BadUserStringHeapIndex);
            MemoryCursor c = this.cursor;
            c.Position = this.mdOffset + this.generalStringHeap.offset + stringHeapIndex;
            int strLength = c.ReadCompressedInt();
            return c.ReadUTF16(strLength / 2);
        }
        internal string/*!*/ GetBlobString(int blobIndex)
        {
            MemoryCursor c = this.cursor;
            c.Position = this.PositionOfBlob(blobIndex);
            int blobLength = c.ReadCompressedInt();
            return c.ReadUTF16(blobLength / 2);
        }
        internal object GetValueFromBlob(int type, int blobIndex)
        {
            MemoryCursor c = this.cursor;
            c.Position = this.PositionOfBlob(blobIndex);
            int blobLength = c.ReadCompressedInt();
            switch ((ElementType)type)
            {
                case ElementType.Boolean: return c.ReadBoolean();
                case ElementType.Char: return (char)c.ReadUInt16();
                case ElementType.Double: return c.ReadDouble();
                case ElementType.Single: return c.ReadSingle();
                case ElementType.Int16: return c.ReadInt16();
                case ElementType.Int32: return c.ReadInt32();
                case ElementType.Int64: return c.ReadInt64();
                case ElementType.Int8: return c.ReadSByte();
                case ElementType.UInt16: return c.ReadUInt16();
                case ElementType.UInt32: return c.ReadUInt32();
                case ElementType.UInt64: return c.ReadUInt64();
                case ElementType.UInt8: return c.ReadByte();
                case ElementType.Class: return null;
                case ElementType.String: return c.ReadUTF16(blobLength / 2);
            }
            throw new InvalidMetadataException(ExceptionStrings.UnknownConstantType);
        }
        internal byte[] GetResourceData(int resourceOffset)
        {
            this.cursor.Position = this.resourcesOffset + resourceOffset;
            int length = this.cursor.ReadInt32();
            return this.cursor.ReadBytes(length);
        }
        private int PositionOfBlob(int blobIndex)
        //^ requires this.blobHeap != null;
        {
            if (blobIndex < 0 || this.blobHeap.size <= blobIndex)
                throw new System.ArgumentOutOfRangeException("blobIndex", ExceptionStrings.BadBlobHeapIndex);
            return this.mdOffset + this.blobHeap.offset + blobIndex;
        }
        private void ReadHeader()
        { //TODO: break up this method
            MemoryCursor c = this.cursor;
            c.Position = 0;

            ReadDOSHeader(c);
            NTHeader ntHeader = ReadNTHeader(c);
            this.linkerMajorVersion = ntHeader.majorLinkerVersion;
            this.linkerMinorVersion = ntHeader.minorLinkerVersion;
            this.fileAlignment = ntHeader.fileAlignment;
            if ((ntHeader.characteristics & 0x2000) != 0)
                this.moduleKind = ModuleKindFlags.DynamicallyLinkedLibrary;
            else
                this.moduleKind = ntHeader.subsystem == 0x3 ? ModuleKindFlags.ConsoleApplication : ModuleKindFlags.WindowsApplication;

            int sectionCount = ntHeader.numberOfSections;
            SectionHeader[] sectionHeaders = this.sectionHeaders = new SectionHeader[sectionCount];
            int resourceSectionIndex = -1;
            for (int i = 0; i < sectionCount; i++)
            {
                sectionHeaders[i] = ReadSectionHeader(c);
                if (sectionHeaders[i].name == ".rsrc") resourceSectionIndex = i;
            }
            if (resourceSectionIndex >= 0)
                this.win32ResourcesOffset = sectionHeaders[resourceSectionIndex].pointerToRawData;
            else
                this.win32ResourcesOffset = -1;

            DirectoryEntry de = ntHeader.cliHeaderTable;
            int cliHeaderOffset = this.RvaToOffset(de.virtualAddress);
            c.Position = cliHeaderOffset;

            CLIHeader cliHeader = ReadCLIHeader(c);
            this.entryPointToken = cliHeader.entryPointToken;
            if ((cliHeader.flags & 1) != 0)
                this.peKind = PEKindFlags.ILonly;
            if ((cliHeader.flags & 0x10) != 0)
                this.entryPointToken = 0; //Native entry point. Ignore.
            switch (ntHeader.machine)
            {
                case 0x0200:
                    this.peKind |= PEKindFlags.Requires64bits;
                    break;
                case 0x8664:
                    this.peKind |= PEKindFlags.Requires64bits | PEKindFlags.AMD;
                    break;
                default:
                    if (ntHeader.magic == 0x20B) //Optional header magic for PE32+
                        this.peKind |= PEKindFlags.Requires64bits;
                    else if ((cliHeader.flags & 2) != 0)
                        this.peKind |= PEKindFlags.Requires32bits;
                    break;
            }
            this.TrackDebugData = (cliHeader.flags & 0x10000) != 0;
            if (cliHeader.resources.size > 0)
                this.resourcesOffset = this.RvaToOffset(cliHeader.resources.virtualAddress);

            int snSize = cliHeader.strongNameSignature.size;
            if (snSize > 0)
            {
                long hashOffset = this.RvaToOffset(cliHeader.strongNameSignature.virtualAddress);
                c.Position = (int)hashOffset;
                this.HashValue = c.ReadBytes(snSize);
                bool zeroHash = true;
                for (int i = 0; i < snSize; i++) if (this.HashValue[i] != 0) zeroHash = false;
                if (zeroHash) this.HashValue = null; //partially signed assembly
            }

            long mdOffset = this.mdOffset = this.RvaToOffset(cliHeader.metaData.virtualAddress);
            c.Position = (int)mdOffset;
            MetadataHeader mdHeader = ReadMetadataHeader(c);
            this.targetRuntimeVersion = mdHeader.versionString;

            foreach (StreamHeader sheader in mdHeader.streamHeaders)
            {
                //^ assume sheader != null;
                switch (sheader.name)
                {
                    case "#Strings": this.identifierStringHeap = sheader; continue;
                    case "#US": this.generalStringHeap = sheader; continue;
                    case "#Blob": this.blobHeap = sheader; continue;
                    case "#GUID": this.guidHeap = sheader; continue;
                    case "#~": this.tables = sheader; continue;
                    case "#-": this.tables = sheader; continue;
                    default: continue;
                }
            }
            if (this.tables == null) throw new InvalidMetadataException(ExceptionStrings.NoMetadataStream);
            c.Position = (int)(mdOffset + this.tables.offset);
            TablesHeader tablesHeader = this.tablesHeader = ReadTablesHeader(c);
            this.metadataFormatMajorVersion = tablesHeader.majorVersion;
            this.metadataFormatMinorVersion = tablesHeader.minorVersion;

            int[] tableSize = this.tableSize = new int[(int)TableIndices.Count];
            int[] tableRefSize = this.tableRefSize = new int[(int)TableIndices.Count];
            long valid = tablesHeader.maskValid;
            int[] countArray = tablesHeader.countArray;
            //^ assume countArray != null;
            for (int i = 0, j = 0; i < (int)TableIndices.Count; i++)
            {
                if (valid % 2 == 1)
                {
                    int m = tableSize[i] = countArray[j++];
                    tableRefSize[i] = m < 0x10000 ? 2 : 4;
                }
                else
                    tableRefSize[i] = 2;
                valid /= 2;
            }
            int blobRefSize = this.blobRefSize = ((tablesHeader.heapSizes & 0x04) == 0 ? 2 : 4);
            int constantParentRefSize = this.constantParentRefSize =
              tableSize[(int)TableIndices.Param] < 0x4000 &&
              tableSize[(int)TableIndices.Field] < 0x4000 &&
              tableSize[(int)TableIndices.Property] < 0x4000 ? 2 : 4;
            int customAttributeParentRefSize = 0;
            if (this.metadataFormatMajorVersion > 1 || this.metadataFormatMinorVersion > 0)
            {
                customAttributeParentRefSize = this.customAttributeParentRefSize =
                  tableSize[(int)TableIndices.Method] < 0x0800 &&
                  tableSize[(int)TableIndices.Field] < 0x0800 &&
                  tableSize[(int)TableIndices.TypeRef] < 0x0800 &&
                  tableSize[(int)TableIndices.TypeDef] < 0x0800 &&
                  tableSize[(int)TableIndices.Param] < 0x0800 &&
                  tableSize[(int)TableIndices.InterfaceImpl] < 0x0800 &&
                  tableSize[(int)TableIndices.MemberRef] < 0x0800 &&
                  tableSize[(int)TableIndices.Module] < 0x0800 &&
                  tableSize[(int)TableIndices.DeclSecurity] < 0x0800 &&
                  tableSize[(int)TableIndices.Property] < 0x0800 &&
                  tableSize[(int)TableIndices.Event] < 0x0800 &&
                  tableSize[(int)TableIndices.StandAloneSig] < 0x0800 &&
                  tableSize[(int)TableIndices.ModuleRef] < 0x0800 &&
                  tableSize[(int)TableIndices.TypeSpec] < 0x0800 &&
                  tableSize[(int)TableIndices.Assembly] < 0x0800 &&
                  tableSize[(int)TableIndices.File] < 0x0800 &&
                  tableSize[(int)TableIndices.ExportedType] < 0x0800 &&
                  tableSize[(int)TableIndices.ManifestResource] < 0x0800 &&
                  tableSize[(int)TableIndices.GenericParam] < 0x0800 &&
                  tableSize[(int)TableIndices.MethodSpec] < 0x0800 &&
                  tableSize[(int)TableIndices.GenericParamConstraint] < 0x0800 ? 2 : 4;
            }
            else
            {
                customAttributeParentRefSize = this.customAttributeParentRefSize =
                  tableSize[(int)TableIndices.Method] < 0x0800 &&
                  tableSize[(int)TableIndices.Field] < 0x0800 &&
                  tableSize[(int)TableIndices.TypeRef] < 0x0800 &&
                  tableSize[(int)TableIndices.TypeDef] < 0x0800 &&
                  tableSize[(int)TableIndices.Param] < 0x0800 &&
                  tableSize[(int)TableIndices.InterfaceImpl] < 0x0800 &&
                  tableSize[(int)TableIndices.MemberRef] < 0x0800 &&
                  tableSize[(int)TableIndices.Module] < 0x0800 &&
                  tableSize[(int)TableIndices.DeclSecurity] < 0x0800 &&
                  tableSize[(int)TableIndices.Property] < 0x0800 &&
                  tableSize[(int)TableIndices.Event] < 0x0800 &&
                  tableSize[(int)TableIndices.StandAloneSig] < 0x0800 &&
                  tableSize[(int)TableIndices.ModuleRef] < 0x0800 &&
                  tableSize[(int)TableIndices.TypeSpec] < 0x0800 &&
                  tableSize[(int)TableIndices.Assembly] < 0x0800 &&
                  tableSize[(int)TableIndices.File] < 0x0800 &&
                  tableSize[(int)TableIndices.ExportedType] < 0x0800 &&
                  tableSize[(int)TableIndices.ManifestResource] < 0x0800 ? 2 : 4;
            }
            int customAttributeConstructorRefSize = this.customAttributeConstructorRefSize =
              tableSize[(int)TableIndices.Method] < 0x2000 &&
              tableSize[(int)TableIndices.MemberRef] < 0x2000 ? 2 : 4;
            int declSecurityParentRefSize = this.declSecurityParentRefSize =
              tableSize[(int)TableIndices.TypeDef] < 0x4000 &&
              tableSize[(int)TableIndices.Method] < 0x4000 &&
              tableSize[(int)TableIndices.Assembly] < 0x4000 ? 2 : 4;
            int fieldMarshalParentRefSize = this.fieldMarshalParentRefSize =
              tableSize[(int)TableIndices.Field] < 0x8000 &&
              tableSize[(int)TableIndices.Param] < 0x8000 ? 2 : 4;
            int guidRefSize = this.guidRefSize = ((tablesHeader.heapSizes & 0x02) == 0 ? 2 : 4);
            int hasSemanticRefSize = this.hasSemanticRefSize =
              tableSize[(int)TableIndices.Event] < 0x8000 &&
              tableSize[(int)TableIndices.Property] < 0x8000 ? 2 : 4;
            int implementationRefSize = this.implementationRefSize =
              tableSize[(int)TableIndices.File] < 0x4000 &&
              tableSize[(int)TableIndices.AssemblyRef] < 0x4000 &&
              tableSize[(int)TableIndices.ExportedType] < 0x4000 ? 2 : 4;
            int methodDefOrRefSize = this.methodDefOrRefSize =
              tableSize[(int)TableIndices.Method] < 0x8000 &&
              tableSize[(int)TableIndices.MemberRef] < 0x8000 ? 2 : 4;
            int memberRefParentSize = this.memberRefParentSize =
              tableSize[(int)TableIndices.TypeDef] < 0x2000 &&
              tableSize[(int)TableIndices.TypeRef] < 0x2000 &&
              tableSize[(int)TableIndices.ModuleRef] < 0x2000 &&
              tableSize[(int)TableIndices.Method] < 0x2000 &&
              tableSize[(int)TableIndices.TypeSpec] < 0x2000 ? 2 : 4;
            int memberForwardedRefSize = this.memberForwardedRefSize =
              tableSize[(int)TableIndices.Field] < 0x8000 &&
              tableSize[(int)TableIndices.Method] < 0x8000 ? 2 : 4;
            int typeDefOrMethodDefSize = this.typeDefOrMethodDefSize =
              tableSize[(int)TableIndices.TypeDef] < 0x8000 &&
              tableSize[(int)TableIndices.Method] < 0x8000 ? 2 : 4;
            int typeDefOrRefOrSpecSize = this.typeDefOrRefOrSpecSize =
              tableSize[(int)TableIndices.TypeDef] < 0x4000 &&
              tableSize[(int)TableIndices.TypeRef] < 0x4000 &&
              tableSize[(int)TableIndices.TypeSpec] < 0x4000 ? 2 : 4;
            int resolutionScopeRefSize = this.resolutionScopeRefSize =
              tableSize[(int)TableIndices.Module] < 0x4000 &&
              tableSize[(int)TableIndices.ModuleRef] < 0x4000 &&
              tableSize[(int)TableIndices.AssemblyRef] < 0x4000 &&
              tableSize[(int)TableIndices.TypeRef] < 0x4000 ? 2 : 4;
            int stringRefSize = this.stringRefSize = ((tablesHeader.heapSizes & 0x01) == 0 ? 2 : 4);

            int[] tableOffset = this.tableOffset = new int[(int)TableIndices.Count];
            int offset = this.mdOffset + this.tables.offset + 24 + countArray.Length * 4;
            for (int i = 0; i < (int)TableIndices.Count; i++)
            {
                int m = tableSize[i];
                if (m == 0) continue;
                tableOffset[i] = offset;
                switch ((TableIndices)i)
                {
                    case TableIndices.Module: offset += m * (2 + stringRefSize + 3 * guidRefSize); break;
                    case TableIndices.TypeRef: offset += m * (resolutionScopeRefSize + 2 * stringRefSize); break;
                    case TableIndices.TypeDef: offset += m * (4 + 2 * stringRefSize + typeDefOrRefOrSpecSize + tableRefSize[(int)TableIndices.Field] + tableRefSize[(int)TableIndices.Method]); break;
                    case TableIndices.FieldPtr: offset += m * (tableRefSize[(int)TableIndices.Field]); break;
                    case TableIndices.Field: offset += m * (2 + stringRefSize + blobRefSize); break;
                    case TableIndices.MethodPtr: offset += m * (tableRefSize[(int)TableIndices.Method]); break;
                    case TableIndices.Method: offset += m * (8 + stringRefSize + blobRefSize + tableRefSize[(int)TableIndices.Param]); break;
                    case TableIndices.ParamPtr: offset += m * (tableRefSize[(int)TableIndices.Param]); break;
                    case TableIndices.Param: offset += m * (4 + stringRefSize); break;
                    case TableIndices.InterfaceImpl: offset += m * (tableRefSize[(int)TableIndices.TypeDef] + typeDefOrRefOrSpecSize); break;
                    case TableIndices.MemberRef: offset += m * (memberRefParentSize + stringRefSize + blobRefSize); break;
                    case TableIndices.Constant: offset += m * (2 + constantParentRefSize + blobRefSize); break;
                    case TableIndices.CustomAttribute: offset += m * (customAttributeParentRefSize + customAttributeConstructorRefSize + blobRefSize); break;
                    case TableIndices.FieldMarshal: offset += m * (fieldMarshalParentRefSize + blobRefSize); break;
                    case TableIndices.DeclSecurity: offset += m * (2 + declSecurityParentRefSize + blobRefSize); break;
                    case TableIndices.ClassLayout: offset += m * (6 + tableRefSize[(int)TableIndices.TypeDef]); break;
                    case TableIndices.FieldLayout: offset += m * (4 + tableRefSize[(int)TableIndices.Field]); break;
                    case TableIndices.StandAloneSig: offset += m * (blobRefSize); break;
                    case TableIndices.EventMap: offset += m * (tableRefSize[(int)TableIndices.TypeDef] + tableRefSize[(int)TableIndices.Event]); break;
                    case TableIndices.EventPtr: offset += m * (tableRefSize[(int)TableIndices.Event]); break;
                    case TableIndices.Event: offset += m * (2 + stringRefSize + typeDefOrRefOrSpecSize); break;
                    case TableIndices.PropertyMap: offset += m * (tableRefSize[(int)TableIndices.TypeDef] + tableRefSize[(int)TableIndices.Property]); break;
                    case TableIndices.PropertyPtr: offset += m * (tableRefSize[(int)TableIndices.Property]); break;
                    case TableIndices.Property: offset += m * (2 + stringRefSize + blobRefSize); break;
                    case TableIndices.MethodSemantics: offset += m * (2 + tableRefSize[(int)TableIndices.Method] + hasSemanticRefSize); break;
                    case TableIndices.MethodImpl: offset += m * (tableRefSize[(int)TableIndices.TypeDef] + 2 * methodDefOrRefSize); break;
                    case TableIndices.ModuleRef: offset += m * (stringRefSize); break;
                    case TableIndices.TypeSpec: offset += m * (blobRefSize); break;
                    case TableIndices.ImplMap: offset += m * (2 + memberForwardedRefSize + stringRefSize + tableRefSize[(int)TableIndices.ModuleRef]); break;
                    case TableIndices.FieldRva: offset += m * (4 + tableRefSize[(int)TableIndices.Field]); break;
                    case TableIndices.EncLog: throw new InvalidMetadataException(ExceptionStrings.ENCLogTableEncountered);
                    case TableIndices.EncMap: throw new InvalidMetadataException(ExceptionStrings.ENCMapTableEncountered);
                    case TableIndices.Assembly: offset += m * (16 + blobRefSize + 2 * stringRefSize); break;
                    case TableIndices.AssemblyProcessor: offset += m * (4); break;
                    case TableIndices.AssemblyOS: offset += m * (12); break;
                    case TableIndices.AssemblyRef: offset += m * (12 + 2 * blobRefSize + 2 * stringRefSize); break;
                    case TableIndices.AssemblyRefProcessor: offset += m * (4 + tableRefSize[(int)TableIndices.AssemblyRef]); break;
                    case TableIndices.AssemblyRefOS: offset += m * (12 + tableRefSize[(int)TableIndices.AssemblyRef]); break;
                    case TableIndices.File: offset += m * (4 + stringRefSize + blobRefSize); break;
                    case TableIndices.ExportedType: offset += m * (8 + 2 * stringRefSize + implementationRefSize); break;
                    case TableIndices.ManifestResource: offset += m * (8 + stringRefSize + implementationRefSize); break;
                    case TableIndices.NestedClass: offset += m * (2 * tableRefSize[(int)TableIndices.TypeDef]); break;
                    case TableIndices.GenericParam:
                        if (this.metadataFormatMajorVersion == 1 && this.metadataFormatMinorVersion == 0)
                            offset += m * (6 + typeDefOrMethodDefSize + stringRefSize + typeDefOrRefOrSpecSize);
                        else if (this.metadataFormatMajorVersion == 1 && this.metadataFormatMinorVersion == 1)
                            offset += m * (4 + typeDefOrMethodDefSize + stringRefSize + typeDefOrRefOrSpecSize);
                        else
                            offset += m * (4 + typeDefOrMethodDefSize + stringRefSize);
                        break;
                    case TableIndices.MethodSpec: offset += m * (methodDefOrRefSize + blobRefSize); break;
                    case TableIndices.GenericParamConstraint: offset += m * (tableRefSize[(int)TableIndices.GenericParam] + typeDefOrRefOrSpecSize); break;
                    default: throw new InvalidMetadataException(ExceptionStrings.UnsupportedTableEncountered);
                }
            }
        }
        internal Win32ResourceList ReadWin32Resources()
        {
            Win32ResourceList rs = new Win32ResourceList();
            int startPos = this.win32ResourcesOffset;
            if (startPos < 0) return rs;
            MemoryCursor c = this.cursor;
            c.Position = startPos;
            int sizeOfTypeDirectory = ReadWin32ResourceDirectoryHeader(c);
            for (int i = 0; i < sizeOfTypeDirectory; i++)
            {
                string TypeName = null;
                int TypeID = c.ReadInt32();
                if (TypeID < 0)
                {
                    MemoryCursor nac = new MemoryCursor(c);
                    nac.Position = startPos + (TypeID & 0x7FFFFFFF);
                    int strLength = nac.ReadUInt16();
                    TypeName = nac.ReadUTF16(strLength);
                }
                int offset = c.ReadInt32();
                if (offset >= 0)
                    rs.Add(this.ReadWin32ResourceDataEntry(c, startPos + offset, TypeName, TypeID, null, 0, 0));
                else
                {
                    MemoryCursor nc = new MemoryCursor(c);
                    nc.Position = startPos + (offset & 0x7FFFFFFF);
                    int sizeOfNameDirectory = ReadWin32ResourceDirectoryHeader(nc);
                    for (int j = 0; j < sizeOfNameDirectory; j++)
                    {
                        string Name = null;
                        int ID = nc.ReadInt32();
                        if (ID < 0)
                        {
                            MemoryCursor nac = new MemoryCursor(c);
                            int strLength = nac.ReadUInt16();
                            Name = nac.ReadUTF16(strLength);
                        }
                        offset = nc.ReadInt32();
                        if (offset >= 0)
                            rs.Add(this.ReadWin32ResourceDataEntry(c, startPos + offset, TypeName, TypeID, Name, ID, 0));
                        else
                        {
                            MemoryCursor lc = new MemoryCursor(c);
                            lc.Position = startPos + (offset & 0x7FFFFFFF);
                            int sizeOfLanguageDirectory = ReadWin32ResourceDirectoryHeader(lc);
                            for (int k = 0; k < sizeOfLanguageDirectory; k++)
                            {
                                int LanguageID = lc.ReadInt32();
                                offset = lc.ReadInt32();
                                rs.Add(this.ReadWin32ResourceDataEntry(c, startPos + offset, TypeName, TypeID, Name, ID, LanguageID));
                            }
                        }
                    }
                }
            }
            return rs;
        }
        private static int ReadWin32ResourceDirectoryHeader(MemoryCursor/*!*/ c)
        {
            c.ReadInt32(); //Characteristics
            c.ReadInt32(); //TimeDate stamp
            c.ReadInt32(); //Version
            int numberOfNamedEntries = c.ReadUInt16();
            int numberOfIdEntries = c.ReadUInt16();
            return numberOfNamedEntries + numberOfIdEntries;
        }
        private Win32Resource ReadWin32ResourceDataEntry(MemoryCursor/*!*/ c, int position,
          string TypeName, int TypeID, string Name, int ID, int LanguageID)
        {
            Win32Resource rsrc = new Win32Resource();
            rsrc.TypeName = TypeName;
            rsrc.TypeId = TypeID;
            rsrc.Name = Name;
            rsrc.Id = ID;
            rsrc.LanguageId = LanguageID;
            c = new MemoryCursor(c);
            c.Position = position;
            int dataRVA = c.ReadInt32();
            int dataSize = c.ReadInt32();
            rsrc.CodePage = c.ReadInt32();
            c.Position = this.RvaToOffset(dataRVA);
            rsrc.Data = c.ReadBytes(dataSize);
            return rsrc;
        }
        private void ReadAssemblyTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.Assembly];
            AssemblyRow[] result = this.assemblyTable = new AssemblyRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.Assembly];
            for (int i = 0; i < n; i++)
            {
                AssemblyRow row;
                row.HashAlgId = c.ReadInt32();
                row.MajorVersion = c.ReadUInt16();
                row.MinorVersion = c.ReadUInt16();
                row.BuildNumber = c.ReadUInt16();
                row.RevisionNumber = c.ReadUInt16();
                row.Flags = c.ReadInt32();
                row.PublicKey = c.ReadReference(this.blobRefSize);
                row.Name = c.ReadReference(this.stringRefSize);
                row.Culture = c.ReadReference(this.stringRefSize);
                result[i] = row;
            }
        }
        private void ReadAssemblyRefTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.AssemblyRef];
            AssemblyRefRow[] result = this.assemblyRefTable = new AssemblyRefRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.AssemblyRef];
            for (int i = 0; i < n; i++)
            {
                AssemblyRefRow row;
                row.MajorVersion = c.ReadUInt16();
                row.MinorVersion = c.ReadUInt16();
                row.BuildNumber = c.ReadUInt16();
                row.RevisionNumber = c.ReadUInt16();
                row.Flags = c.ReadInt32();
                row.PublicKeyOrToken = c.ReadReference(this.blobRefSize);
                row.Name = c.ReadReference(this.stringRefSize);
                row.Culture = c.ReadReference(this.stringRefSize);
                row.HashValue = c.ReadReference(this.blobRefSize);
                row.AssemblyReference = null;
                result[i] = row;
            }
        }
        private void ReadClassLayoutTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.ClassLayout];
            ClassLayoutRow[] result = this.classLayoutTable = new ClassLayoutRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.ClassLayout];
            for (int i = 0; i < n; i++)
            {
                ClassLayoutRow row;
                row.PackingSize = c.ReadUInt16();
                row.ClassSize = c.ReadInt32();
                row.Parent = c.ReadReference(this.tableRefSize[(int)TableIndices.TypeDef]);
                result[i] = row;
            }
        }
        private void ReadConstantTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.Constant];
            ConstantRow[] result = this.constantTable = new ConstantRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.Constant];
            for (int i = 0; i < n; i++)
            {
                ConstantRow row;
                row.Type = c.ReadByte();
                c.ReadByte();
                row.Parent = c.ReadReference(this.constantParentRefSize);
                row.Value = c.ReadReference(this.blobRefSize);
                result[i] = row;
            }
        }
        private void ReadCustomAttributeTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.CustomAttribute];
            CustomAttributeRow[] result = this.customAttributeTable = new CustomAttributeRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.CustomAttribute];
            for (int i = 0; i < n; i++)
            {
                CustomAttributeRow row;
                row.Parent = c.ReadReference(this.customAttributeParentRefSize);
                row.Constructor = c.ReadReference(this.customAttributeConstructorRefSize);
                row.Value = c.ReadReference(this.blobRefSize);
                result[i] = row;
            }
        }
        private void ReadDeclSecurityTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.DeclSecurity];
            DeclSecurityRow[] result = this.declSecurityTable = new DeclSecurityRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.DeclSecurity];
            for (int i = 0; i < n; i++)
            {
                DeclSecurityRow row;
                row.Action = c.ReadUInt16();
                row.Parent = c.ReadReference(this.declSecurityParentRefSize);
                row.PermissionSet = c.ReadReference(this.blobRefSize);
                result[i] = row;
            }
        }
        private void ReadEventMapTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.EventMap];
            EventMapRow[] result = this.eventMapTable = new EventMapRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.EventMap];
            for (int i = 0; i < n; i++)
            {
                EventMapRow row;
                row.Parent = c.ReadReference(this.tableRefSize[(int)TableIndices.TypeDef]);
                row.EventList = c.ReadReference(this.tableRefSize[(int)TableIndices.Event]);
                result[i] = row;
            }
        }
        private void ReadEventPtrTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.EventPtr];
            EventPtrRow[] result = this.eventPtrTable = new EventPtrRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.EventPtr];
            for (int i = 0; i < n; i++)
            {
                EventPtrRow row;
                row.Event = c.ReadReference(this.tableRefSize[(int)TableIndices.Event]);
                result[i] = row;
            }
        }
        private void ReadEventTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.Event];
            EventRow[] result = this.eventTable = new EventRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.Event];
            for (int i = 0; i < n; i++)
            {
                EventRow row;
                row.Flags = c.ReadUInt16();
                row.Name = c.ReadReference(this.stringRefSize);
                row.EventType = c.ReadReference(this.typeDefOrRefOrSpecSize);
                result[i] = row;
            }
        }
        private void ReadExportedTypeTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.ExportedType];
            ExportedTypeRow[] result = this.exportedTypeTable = new ExportedTypeRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.ExportedType];
            for (int i = 0; i < n; i++)
            {
                ExportedTypeRow row;
                row.Flags = c.ReadInt32();
                row.TypeDefId = c.ReadInt32();
                row.TypeName = c.ReadReference(this.stringRefSize);
                row.TypeNamespace = c.ReadReference(this.stringRefSize);
                row.Implementation = c.ReadReference(this.implementationRefSize);
                result[i] = row;
            }
        }
        private void ReadFieldTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.Field];
            FieldRow[] result = this.fieldTable = new FieldRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.Field];
            for (int i = 0; i < n; i++)
            {
                FieldRow row;
                row.Flags = c.ReadUInt16();
                row.Name = c.ReadReference(this.stringRefSize);
                row.Signature = c.ReadReference(this.blobRefSize);
                row.Field = null;
                result[i] = row;
            }
        }
        private void ReadFieldLayoutTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.FieldLayout];
            FieldLayoutRow[] result = this.fieldLayoutTable = new FieldLayoutRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.FieldLayout];
            for (int i = 0; i < n; i++)
            {
                FieldLayoutRow row;
                row.Offset = c.ReadInt32();
                row.Field = c.ReadReference(this.tableRefSize[(int)TableIndices.Field]);
                result[i] = row;
            }
        }
        private void ReadFieldMarshalTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.FieldMarshal];
            FieldMarshalRow[] result = this.fieldMarshalTable = new FieldMarshalRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.FieldMarshal];
            for (int i = 0; i < n; i++)
            {
                FieldMarshalRow row;
                row.Parent = c.ReadReference(this.fieldMarshalParentRefSize);
                row.NativeType = c.ReadReference(this.blobRefSize);
                result[i] = row;
            }
        }
        private void ReadFieldPtrTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.FieldPtr];
            FieldPtrRow[] result = this.fieldPtrTable = new FieldPtrRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.FieldPtr];
            for (int i = 0; i < n; i++)
            {
                FieldPtrRow row;
                row.Field = c.ReadReference(this.tableRefSize[(int)TableIndices.Field]);
                result[i] = row;
            }
        }
        private void ReadFieldRvaTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.FieldRva];
            FieldRvaRow[] result = this.fieldRvaTable = new FieldRvaRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.FieldRva];
            for (int i = 0; i < n; i++)
            {
                FieldRvaRow row;
                row.RVA = c.ReadInt32();
                row.Field = c.ReadReference(this.tableRefSize[(int)TableIndices.Field]);
                row.TargetSection = 0; //Ignored on reading
                result[i] = row;
            }
        }
        private void ReadFileTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.File];
            FileRow[] result = this.fileTable = new FileRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.File];
            for (int i = 0; i < n; i++)
            {
                FileRow row;
                row.Flags = c.ReadInt32();
                row.Name = c.ReadReference(this.stringRefSize);
                row.HashValue = c.ReadReference(this.blobRefSize);
                result[i] = row;
            }
        }
        private void ReadGenericParamTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.GenericParam];
            GenericParamRow[] result = this.genericParamTable = new GenericParamRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.GenericParam];
            bool reallyOldGenericsFileFormat = this.metadataFormatMajorVersion == 1 && this.metadataFormatMinorVersion == 0;
            bool oldGenericsFileFormat = this.metadataFormatMajorVersion == 1 && this.metadataFormatMinorVersion == 1;
            for (int i = 0; i < n; i++)
            {
                GenericParamRow row;
                row.Number = c.ReadUInt16();
                row.Flags = c.ReadUInt16();
                row.Owner = c.ReadReference(this.typeDefOrMethodDefSize);
                row.Name = c.ReadReference(this.stringRefSize);
                row.GenericParameter = null;
                if (oldGenericsFileFormat) c.ReadReference(this.typeDefOrRefOrSpecSize);
                if (reallyOldGenericsFileFormat) c.ReadInt16();
                result[i] = row;
            }
        }
        private void ReadGenericParamConstraintTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.GenericParamConstraint];
            GenericParamConstraintRow[] result = this.genericParamConstraintTable = new GenericParamConstraintRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.GenericParamConstraint];
            for (int i = 0; i < n; i++)
            {
                GenericParamConstraintRow row;
                row.Param = c.ReadReference(this.tableRefSize[(int)TableIndices.GenericParam]);
                row.Constraint = c.ReadReference(this.typeDefOrRefOrSpecSize);
                result[i] = row;
            }
        }
        private void ReadImplMapTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.ImplMap];
            ImplMapRow[] result = this.implMapTable = new ImplMapRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.ImplMap];
            for (int i = 0; i < n; i++)
            {
                ImplMapRow row;
                row.MappingFlags = c.ReadUInt16();
                row.MemberForwarded = c.ReadReference(this.memberForwardedRefSize);
                row.ImportName = c.ReadReference(this.stringRefSize);
                row.ImportScope = c.ReadReference(this.tableRefSize[(int)TableIndices.ModuleRef]);
                result[i] = row;
            }
        }
        private void ReadInterfaceImplTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.InterfaceImpl];
            InterfaceImplRow[] result = this.interfaceImplTable = new InterfaceImplRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.InterfaceImpl];
            for (int i = 0; i < n; i++)
            {
                InterfaceImplRow row;
                row.Class = c.ReadReference(this.tableRefSize[(int)TableIndices.TypeDef]);
                row.Interface = c.ReadReference(this.typeDefOrRefOrSpecSize);
                result[i] = row;
            }
        }
        private void ReadManifestResourceTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.ManifestResource];
            ManifestResourceRow[] result = this.manifestResourceTable = new ManifestResourceRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.ManifestResource];
            for (int i = 0; i < n; i++)
            {
                ManifestResourceRow row;
                row.Offset = c.ReadInt32();
                row.Flags = c.ReadInt32();
                row.Name = c.ReadReference(this.stringRefSize);
                row.Implementation = c.ReadReference(this.implementationRefSize);
                result[i] = row;
            }
        }
        private void ReadMemberRefTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.MemberRef];
            MemberRefRow[] result = this.memberRefTable = new MemberRefRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.MemberRef];
            for (int i = 0; i < n; i++)
            {
                MemberRefRow row;
                row.Class = c.ReadReference(this.memberRefParentSize);
                row.Name = c.ReadReference(this.stringRefSize);
                row.Signature = c.ReadReference(this.blobRefSize);
                row.Member = null;
                row.VarargTypes = null;
                result[i] = row;
            }
        }
        private void ReadMethodTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.Method];
            MethodRow[] result = this.methodTable = new MethodRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.Method];
            for (int i = 0; i < n; i++)
            {
                MethodRow row;
                row.RVA = c.ReadInt32();
                row.ImplFlags = c.ReadUInt16();
                row.Flags = c.ReadUInt16();
                row.Name = c.ReadReference(this.stringRefSize);
                row.Signature = c.ReadReference(this.blobRefSize);
                row.ParamList = c.ReadReference(this.tableRefSize[(int)TableIndices.Param]);
                row.Method = null;
                result[i] = row;
            }
        }
        private void ReadMethodImplTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.MethodImpl];
            MethodImplRow[] result = this.methodImplTable = new MethodImplRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.MethodImpl];
            for (int i = 0; i < n; i++)
            {
                MethodImplRow row;
                row.Class = c.ReadReference(this.tableRefSize[(int)TableIndices.TypeDef]);
                row.MethodBody = c.ReadReference(this.methodDefOrRefSize);
                row.MethodDeclaration = c.ReadReference(this.methodDefOrRefSize);
                result[i] = row;
            }
        }
        private void ReadMethodPtrTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.MethodPtr];
            MethodPtrRow[] result = this.methodPtrTable = new MethodPtrRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.MethodPtr];
            for (int i = 0; i < n; i++)
            {
                MethodPtrRow row;
                row.Method = c.ReadReference(this.tableRefSize[(int)TableIndices.Method]);
                result[i] = row;
            }
        }
        private void ReadMethodSemanticsTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.MethodSemantics];
            MethodSemanticsRow[] result = this.methodSemanticsTable = new MethodSemanticsRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.MethodSemantics];
            for (int i = 0; i < n; i++)
            {
                MethodSemanticsRow row;
                row.Semantics = c.ReadUInt16();
                row.Method = c.ReadReference(this.tableRefSize[(int)TableIndices.Method]);
                row.Association = c.ReadReference(this.hasSemanticRefSize);
                result[i] = row;
            }
        }
        private void ReadMethodSpecTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.MethodSpec];
            MethodSpecRow[] result = this.methodSpecTable = new MethodSpecRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.MethodSpec];
            for (int i = 0; i < n; i++)
            {
                MethodSpecRow row;
                row.Method = c.ReadReference(this.methodDefOrRefSize);
                row.Instantiation = c.ReadReference(this.blobRefSize);
                row.InstantiatedMethod = null;
                result[i] = row;
            }
        }
        private void ReadModuleTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.Module];
            ModuleRow[] result = this.moduleTable = new ModuleRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.Module];
            for (int i = 0; i < n; i++)
            {
                ModuleRow row;
                row.Generation = c.ReadUInt16();
                row.Name = c.ReadReference(this.stringRefSize);
                row.Mvid = c.ReadReference(this.guidRefSize);
                row.EncId = c.ReadReference(this.guidRefSize);
                row.EncBaseId = c.ReadReference(this.guidRefSize);
                result[i] = row;
            }
        }
        private void ReadModuleRefTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.ModuleRef];
            ModuleRefRow[] result = this.moduleRefTable = new ModuleRefRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.ModuleRef];
            for (int i = 0; i < n; i++)
            {
                ModuleRefRow row;
                row.Name = c.ReadReference(this.stringRefSize);
                row.Module = null;
                result[i] = row;
            }
        }
        private void ReadNestedClassTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.NestedClass];
            NestedClassRow[] result = this.nestedClassTable = new NestedClassRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.NestedClass];
            for (int i = 0; i < n; i++)
            {
                NestedClassRow row;
                row.NestedClass = c.ReadReference(this.tableRefSize[(int)TableIndices.TypeDef]);
                row.EnclosingClass = c.ReadReference(this.tableRefSize[(int)TableIndices.TypeDef]);
                result[i] = row;
            }
        }
        private void ReadParamTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.Param];
            ParamRow[] result = this.paramTable = new ParamRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.Param];
            for (int i = 0; i < n; i++)
            {
                ParamRow row;
                row.Flags = c.ReadUInt16();
                row.Sequence = c.ReadUInt16();
                row.Name = c.ReadReference(this.stringRefSize);
                result[i] = row;
            }
        }
        private void ReadParamPtrTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.ParamPtr];
            ParamPtrRow[] result = this.paramPtrTable = new ParamPtrRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.ParamPtr];
            for (int i = 0; i < n; i++)
            {
                ParamPtrRow row;
                row.Param = c.ReadReference(this.tableRefSize[(int)TableIndices.Param]);
                result[i] = row;
            }
        }
        private void ReadPropertyTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.Property];
            PropertyRow[] result = this.propertyTable = new PropertyRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.Property];
            for (int i = 0; i < n; i++)
            {
                PropertyRow row;
                row.Flags = c.ReadUInt16();
                row.Name = c.ReadReference(this.stringRefSize);
                row.Signature = c.ReadReference(this.blobRefSize);
                result[i] = row;
            }
        }
        private void ReadPropertyMapTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.PropertyMap];
            PropertyMapRow[] result = this.propertyMapTable = new PropertyMapRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.PropertyMap];
            for (int i = 0; i < n; i++)
            {
                PropertyMapRow row;
                row.Parent = c.ReadReference(this.tableRefSize[(int)TableIndices.TypeDef]);
                row.PropertyList = c.ReadReference(this.tableRefSize[(int)TableIndices.Property]);
                result[i] = row;
            }
        }
        private void ReadPropertyPtrTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.PropertyPtr];
            PropertyPtrRow[] result = this.propertyPtrTable = new PropertyPtrRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.PropertyPtr];
            for (int i = 0; i < n; i++)
            {
                PropertyPtrRow row;
                row.Property = c.ReadReference(this.tableRefSize[(int)TableIndices.Property]);
                result[i] = row;
            }
        }
        private void ReadStandAloneSigTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.StandAloneSig];
            StandAloneSigRow[] result = this.standAloneSigTable = new StandAloneSigRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.StandAloneSig];
            for (int i = 0; i < n; i++)
            {
                StandAloneSigRow row;
                row.Signature = c.ReadReference(this.blobRefSize);
                result[i] = row;
            }
        }
        private void ReadTypeDefTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.TypeDef];
            TypeDefRow[] result = this.typeDefTable = new TypeDefRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.TypeDef];
            for (int i = 0; i < n; i++)
            {
                TypeDefRow row;
                row.Flags = c.ReadInt32();
                row.Name = c.ReadReference(this.stringRefSize);
                row.Namespace = c.ReadReference(this.stringRefSize);
                row.Extends = c.ReadReference(this.typeDefOrRefOrSpecSize);
                row.FieldList = c.ReadReference(this.tableRefSize[(int)TableIndices.Field]);
                row.MethodList = c.ReadReference(this.tableRefSize[(int)TableIndices.Method]);
                row.Type = null;
                row.NameKey = 0;
                row.NamespaceId = null;
                row.NamespaceKey = 0;
                result[i] = row;
            }
            for (int i = 0; i < n; i++)
            {
                result[i].NameKey = this.GetIdentifier(result[i].Name).UniqueIdKey;
                result[i].NamespaceId = this.GetIdentifier(result[i].Namespace);
                //^ assume result[i].NamespaceId != null;
                result[i].NamespaceKey = result[i].NamespaceId.UniqueIdKey;
            }
        }
        private void ReadTypeRefTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.TypeRef];
            TypeRefRow[] result = this.typeRefTable = new TypeRefRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.TypeRef];
            for (int i = 0; i < n; i++)
            {
                TypeRefRow row;
                row.ResolutionScope = c.ReadReference(this.resolutionScopeRefSize);
                row.Name = c.ReadReference(this.stringRefSize);
                row.Namespace = c.ReadReference(this.stringRefSize);
                row.Type = null;
                result[i] = row;
            }
        }
        private void ReadTypeSpecTable()
        //^ requires this.tableSize != null;
        //^ requires this.tableOffset != null;
        {
            int n = this.tableSize[(int)TableIndices.TypeSpec];
            TypeSpecRow[] result = this.typeSpecTable = new TypeSpecRow[n];
            if (n == 0) return;
            MemoryCursor c = this.cursor;
            c.Position = this.tableOffset[(int)TableIndices.TypeSpec];
            for (int i = 0; i < n; i++)
            {
                TypeSpecRow row;
                row.Signature = c.ReadReference(this.blobRefSize);
                row.Type = null;
                result[i] = row;
            }
        }
        internal int GetOffsetToEndOfSection(int virtualAddress)
        {
            foreach (SectionHeader section in this.sectionHeaders)
                if (virtualAddress >= section.virtualAddress && virtualAddress < section.virtualAddress + section.sizeOfRawData)
                    return (section.sizeOfRawData - (virtualAddress - section.virtualAddress));
            return -1;
        }
        internal bool NoOffsetFor(int virtualAddress)
        {
            foreach (SectionHeader section in this.sectionHeaders)
                if (virtualAddress >= section.virtualAddress && virtualAddress < section.virtualAddress + section.sizeOfRawData)
                    return false;
            return true;
        }
        private int RvaToOffset(int virtualAddress)
        {
            foreach (SectionHeader section in this.sectionHeaders)
                if (virtualAddress >= section.virtualAddress && virtualAddress < section.virtualAddress + section.sizeOfRawData)
                    return (virtualAddress - section.virtualAddress + section.pointerToRawData);
            throw new InvalidMetadataException(String.Format(CultureInfo.CurrentCulture,
              ExceptionStrings.UnknownVirtualAddress, virtualAddress));
        }
        private int RvaToOffset(int virtualAddress, out PESection targetSection)
        {
            foreach (SectionHeader section in this.sectionHeaders)
                if (virtualAddress >= section.virtualAddress && virtualAddress < section.virtualAddress + section.sizeOfRawData)
                {
                    if (section.name == ".tls") targetSection = PESection.TLS;
                    else if (section.name == ".sdata") targetSection = PESection.SData;
                    else targetSection = PESection.Text;
                    return (virtualAddress - section.virtualAddress + section.pointerToRawData);
                }
            throw new InvalidMetadataException(String.Format(
              CultureInfo.CurrentCulture, ExceptionStrings.UnknownVirtualAddress, +virtualAddress));
        }
        private static CLIHeader/*!*/ ReadCLIHeader(MemoryCursor/*!*/ c)
        {
            CLIHeader header = new CLIHeader();
            header.cb = c.Int32(0); c.SkipInt32(1);
            header.majorRuntimeVersion = c.UInt16(0);
            header.minorRuntimeVersion = c.UInt16(1); c.SkipUInt16(2);
            header.metaData = ReadDirectoryEntry(c);
            header.flags = c.Int32(0);
            header.entryPointToken = c.Int32(1); c.SkipInt32(2);
            header.resources = ReadDirectoryEntry(c);
            header.strongNameSignature = ReadDirectoryEntry(c);
            header.codeManagerTable = ReadDirectoryEntry(c);
            header.vtableFixups = ReadDirectoryEntry(c);
            header.exportAddressTableJumps = ReadDirectoryEntry(c);
            if (header.majorRuntimeVersion < 2)
                throw new InvalidMetadataException(ExceptionStrings.BadCLIHeader);
            return header;
        }
        private static DirectoryEntry ReadDirectoryEntry(MemoryCursor/*!*/ c)
        {
            DirectoryEntry entry = new DirectoryEntry();
            entry.virtualAddress = c.Int32(0);
            entry.size = c.Int32(1); c.SkipInt32(2);
            return entry;
        }
        internal static void ReadDOSHeader(MemoryCursor/*!*/ c)
        {
            c.Position = 0;
            int magicNumber = c.UInt16(0);
            if (magicNumber != 0x5a4d) throw new InvalidMetadataException(ExceptionStrings.BadMagicNumber);
            c.Position = 0x3c;
            int ntHeaderOffset = c.Int32(0);
            c.Position = ntHeaderOffset;
        }
        private static MetadataHeader/*!*/ ReadMetadataHeader(MemoryCursor/*!*/ c)
        {
            MetadataHeader header = new MetadataHeader();
            header.signature = c.ReadInt32();
            if (header.signature != 0x424a5342)
                throw new InvalidMetadataException(ExceptionStrings.BadMetadataHeaderSignature);
            header.majorVersion = c.ReadUInt16();
            header.minorVersion = c.ReadUInt16();
            header.reserved = c.ReadInt32();
            int len = c.ReadInt32();
            header.versionString = c.ReadASCII(len);
            while (len++ % 4 != 0) c.ReadByte();
            header.flags = c.ReadUInt16();
            int n = c.ReadUInt16();
            StreamHeader[] streamHeaders = header.streamHeaders = new StreamHeader[n];
            for (int i = 0; i < n; i++)
                streamHeaders[i] = ReadStreamHeader(c);
            return header;
        }
        internal static NTHeader/*!*/ ReadNTHeader(MemoryCursor/*!*/ c)
        {
            NTHeader header = new NTHeader();
            header.signature = c.ReadInt32();
            header.machine = c.ReadUInt16();
            header.numberOfSections = c.ReadUInt16();
            header.timeDateStamp = c.ReadInt32();
            header.pointerToSymbolTable = c.ReadInt32();
            header.numberOfSymbols = c.ReadInt32();
            header.sizeOfOptionalHeader = c.ReadUInt16();
            header.characteristics = c.ReadUInt16();
            header.magic = c.ReadUInt16();
            header.majorLinkerVersion = c.ReadByte();
            header.minorLinkerVersion = c.ReadByte();
            header.sizeOfCode = c.ReadInt32();
            header.sizeOfInitializedData = c.ReadInt32();
            header.sizeOfUninitializedData = c.ReadInt32();
            header.addressOfEntryPoint = c.ReadInt32();
            header.baseOfCode = c.ReadInt32();
            if (header.magic == 0x10B)
            {
                header.baseOfData = c.ReadInt32();
                header.imageBase = c.ReadInt32();
            }
            else
            {
                header.baseOfData = 0;
                header.imageBase = c.ReadInt64();
            }
            header.sectionAlignment = c.ReadInt32();
            header.fileAlignment = c.ReadInt32();
            header.majorOperatingSystemVersion = c.ReadUInt16();
            header.minorOperatingSystemVersion = c.ReadUInt16();
            header.majorImageVersion = c.ReadUInt16();
            header.minorImageVersion = c.ReadUInt16();
            header.majorSubsystemVersion = c.ReadUInt16();
            header.minorSubsystemVersion = c.ReadUInt16();
            header.win32VersionValue = c.ReadInt32();
            header.sizeOfImage = c.ReadInt32();
            header.sizeOfHeaders = c.ReadInt32();
            header.checkSum = c.ReadInt32();
            header.subsystem = c.ReadUInt16();
            header.dllCharacteristics = c.ReadUInt16();
            if (header.magic == 0x10B)
            {
                header.sizeOfStackReserve = c.ReadInt32();
                header.sizeOfStackCommit = c.ReadInt32();
                header.sizeOfHeapReserve = c.ReadInt32();
                header.sizeOfHeapCommit = c.ReadInt32();
            }
            else
            {
                header.sizeOfStackReserve = c.ReadInt64();
                header.sizeOfStackCommit = c.ReadInt64();
                header.sizeOfHeapReserve = c.ReadInt64();
                header.sizeOfHeapCommit = c.ReadInt64();
            }
            header.loaderFlags = c.ReadInt32();
            header.numberOfDataDirectories = c.ReadInt32();

            // Verify that the header signature and magic number are valid
            if (header.signature != 0x00004550 /* "PE\0\0" */)
                throw new InvalidMetadataException(ExceptionStrings.BadCOFFHeaderSignature);
            if (header.magic != 0x010B && header.magic != 0x020B)
                throw new InvalidMetadataException(ExceptionStrings.BadPEHeaderMagicNumber);

            //Read the data directories
            header.exportTable = ReadDirectoryEntry(c);
            header.importTable = ReadDirectoryEntry(c);
            header.resourceTable = ReadDirectoryEntry(c);
            header.exceptionTable = ReadDirectoryEntry(c);
            header.certificateTable = ReadDirectoryEntry(c);
            header.baseRelocationTable = ReadDirectoryEntry(c);
            header.debugTable = ReadDirectoryEntry(c);
            header.copyrightTable = ReadDirectoryEntry(c);
            header.globalPointerTable = ReadDirectoryEntry(c);
            header.threadLocalStorageTable = ReadDirectoryEntry(c);
            header.loadConfigTable = ReadDirectoryEntry(c);
            header.boundImportTable = ReadDirectoryEntry(c);
            header.importAddressTable = ReadDirectoryEntry(c);
            header.delayImportTable = ReadDirectoryEntry(c);
            header.cliHeaderTable = ReadDirectoryEntry(c);
            header.reserved = ReadDirectoryEntry(c);

            return header;
        }
        internal static SectionHeader ReadSectionHeader(MemoryCursor/*!*/ c)
        {
            SectionHeader header = new SectionHeader();
            header.name = c.ReadASCII(8);
            header.virtualSize = c.Int32(0);
            header.virtualAddress = c.Int32(1);
            header.sizeOfRawData = c.Int32(2);
            header.pointerToRawData = c.Int32(3);
            header.pointerToRelocations = c.Int32(4);
            header.pointerToLinenumbers = c.Int32(5); c.SkipInt32(6);
            header.numberOfRelocations = c.UInt16(0);
            header.numberOfLinenumbers = c.UInt16(1); c.SkipInt16(2);
            header.characteristics = c.Int32(0); c.SkipInt32(1);
            return header;
        }
        private static StreamHeader ReadStreamHeader(MemoryCursor/*!*/ c)
        {
            StreamHeader header = new StreamHeader();
            header.offset = c.ReadInt32();
            header.size = c.ReadInt32();
            header.name = c.ReadASCII();
            int n = header.name.Length + 1;
            c.Position += (4 - (n % 4)) % 4;
            return header;
        }
        private static TablesHeader/*!*/ ReadTablesHeader(MemoryCursor/*!*/ c)
        {
            TablesHeader header = new TablesHeader();
            header.reserved = c.ReadInt32(); // Must be zero
            header.majorVersion = c.ReadByte();  // Must be one
            header.minorVersion = c.ReadByte();  // Must be zero
            header.heapSizes = c.ReadByte();  // Bits for heap sizes
            header.rowId = c.ReadByte();  // log-base-2 of largest rowId
            header.maskValid = c.ReadInt64(); // Present table counts
            header.maskSorted = c.ReadInt64(); // Sorted tables
            int n = 0;
            ulong mask = (ulong)header.maskValid;
            while (mask != 0)
            {
                if (mask % 2 == 1) n++;
                mask /= 2;
            }
            int[] countArray = header.countArray = new int[n];
            for (int i = 0; i < n; i++)
                countArray[i] = c.ReadInt32();
            return header;
        }
    }
#if !NoWriter
    internal class MetadataWriter
    {
        internal MemoryStream StringHeap;
        internal MemoryStream BlobHeap;
        internal MemoryStream UserstringHeap;
        internal MemoryStream ResourceDataHeap;
        internal MemoryStream SdataHeap;
        internal MemoryStream TlsHeap;
        internal Guid[] GuidHeap;
        internal MemoryStream MethodBodiesHeap;
        internal Win32ResourceList Win32Resources;
        internal AssemblyRow[] assemblyTable;
        internal AssemblyRefRow[] assemblyRefTable;
        internal ClassLayoutRow[] classLayoutTable;
        internal ConstantRow[] constantTable;
        internal CustomAttributeRow[] customAttributeTable;
        internal DeclSecurityRow[] declSecurityTable;
        internal EventMapRow[] eventMapTable;
        internal EventRow[] eventTable;
        internal ExportedTypeRow[] exportedTypeTable = null;
        internal FieldRow[] fieldTable;
        internal FieldLayoutRow[] fieldLayoutTable;
        internal FieldMarshalRow[] fieldMarshalTable = null;
        internal FieldRvaRow[] fieldRvaTable = null;
        internal FileRow[] fileTable;
        internal GenericParamRow[] genericParamTable;
        internal GenericParamConstraintRow[] genericParamConstraintTable;
        internal ImplMapRow[] implMapTable;
        internal InterfaceImplRow[] interfaceImplTable;
        internal ManifestResourceRow[] manifestResourceTable = null;
        internal MemberRefRow[] memberRefTable;
        internal MethodRow[] methodTable;
        internal MethodImplRow[] methodImplTable;
        internal MethodSemanticsRow[] methodSemanticsTable;
        internal MethodSpecRow[] methodSpecTable;
        internal ModuleRow[] moduleTable;
        internal ModuleRefRow[] moduleRefTable;
        internal NestedClassRow[] nestedClassTable;
        internal ParamRow[] paramTable;
        internal PropertyRow[] propertyTable;
        internal PropertyMapRow[] propertyMapTable;
        internal StandAloneSigRow[] standAloneSigTable;
        internal TypeDefRow[] typeDefTable;
        internal TypeRefRow[] typeRefTable;
        internal TypeSpecRow[] typeSpecTable;
        internal int entryPointToken;
        internal int fileAlignment;
        internal ModuleKindFlags moduleKind;
        internal PEKindFlags peKind;
        internal bool TrackDebugData;
        internal bool UseGenerics = false;
        internal byte[] PublicKey;

        private int blobRefSize;
        private int constantParentRefSize;
        private int customAttributeParentRefSize;
        private int customAttributeConstructorRefSize;
        private int declSecurityParentRefSize;
        private int fieldMarshalParentRefSize;
        private int guidRefSize;
        private int hasSemanticRefSize;
        private int implementationRefSize;
        private int methodDefOrRefSize;
        private int memberRefParentSize;
        private int memberForwardedRefSize;
        private int typeDefOrMethodDefSize;
        private int typeDefOrRefOrSpecSize;
        private int resolutionScopeRefSize;
        private int stringRefSize;
#if !ROTOR
        private ISymUnmanagedWriter symWriter;
#endif
        private int[] tableRefSize;
        private int[] tableSize;
        private long validMask;

#if !ROTOR
        internal MetadataWriter(ISymUnmanagedWriter symWriter)
        {
            this.symWriter = symWriter;
        }
#else
    internal MetadataWriter(){
    }
#endif

        private void SerializeMetadata(BinaryWriter/*!*/ writer, int virtualAddressBase, Fixup/*!*/ sdataFixup, Fixup/*!*/ tlsFixup)
        //^ requires this.MethodBodiesHeap != null;
        //^ requires this.ResourceDataHeap != null;
        //^ requires this.StringHeap != null;
        //^ requires this.UserstringHeap != null;
        //^ requires this.BlobHeap != null;
        //^ requires this.GuidHeap != null;
        //^ requires TargetPlatform.TargetRuntimeVersion != null;
        {
            int tableOffset = 0;
            tableOffset += (int)this.MethodBodiesHeap.Length;
            this.MethodBodiesHeap.WriteTo(writer.BaseStream);
            while (tableOffset % 4 != 0) { writer.Write((byte)0); tableOffset++; }
            if (this.PublicKey != null && 0 < this.PublicKey.Length)
            {
                this.cliHeader.strongNameSignature.virtualAddress = virtualAddressBase + 72 + tableOffset;
                int keysize = this.PublicKey.Length - 32;
                if (keysize < 128) keysize = 128;
                this.cliHeader.strongNameSignature.size = keysize;
                tableOffset += keysize;
                writer.BaseStream.Position += keysize;
            }
            if (this.ResourceDataHeap.Length > 0)
            {
                this.cliHeader.resources.virtualAddress = virtualAddressBase + 72 + tableOffset;
                this.ResourceDataHeap.WriteTo(writer.BaseStream);
                int sizeOfResources = (int)this.ResourceDataHeap.Length;
                while (sizeOfResources % 4 != 0) { writer.Write((byte)0); sizeOfResources++; }
                this.cliHeader.resources.size = sizeOfResources;
                tableOffset += sizeOfResources;
            }
            this.cliHeader.metaData.virtualAddress = virtualAddressBase + 72 + tableOffset;
            int startPos = (int)writer.BaseStream.Position;
            writer.Write((int)0x424a5342); //Magic signature
            writer.Write((short)1); //Major version
            writer.Write((short)1); //Minor version
            writer.Write((int)0); //Reserved
            writer.Write((int)12); // version must be 12 chars
            char[] version = new char[12];
            char[] aversion = TargetPlatform.TargetRuntimeVersion.ToCharArray();
            Array.Copy(aversion, 0, version, 0, Math.Min(12, aversion.Length));
            writer.Write(version);
            writer.Write((short)0); //flags
            writer.Write((short)5); //number of streams
            int offsetFromStartOfMetadata = 108;
            writer.Write((int)offsetFromStartOfMetadata);
            int cbStringHeapPad = 0;
            offsetFromStartOfMetadata += (int)this.StringHeap.Length;
            while (offsetFromStartOfMetadata % 4 != 0)
            {
                offsetFromStartOfMetadata++;
                cbStringHeapPad++;
            }
            writer.Write((int)this.StringHeap.Length + cbStringHeapPad);
            writer.Write(new char[] { '#', 'S', 't', 'r', 'i', 'n', 'g', 's', '\0', '\0', '\0', '\0' });

            writer.Write((int)offsetFromStartOfMetadata);
            offsetFromStartOfMetadata += (int)this.UserstringHeap.Length;
            int cbUserStringHeapPad = 0;
            while (offsetFromStartOfMetadata % 4 != 0)
            {
                offsetFromStartOfMetadata++;
                cbUserStringHeapPad++;
            }

            writer.Write((int)this.UserstringHeap.Length + cbUserStringHeapPad);
            writer.Write(new char[] { '#', 'U', 'S', '\0' });

            writer.Write((int)offsetFromStartOfMetadata);
            writer.Write((int)this.BlobHeap.Length);
            writer.Write(new char[] { '#', 'B', 'l', 'o', 'b', '\0', '\0', '\0' });
            offsetFromStartOfMetadata += (int)this.BlobHeap.Length;
            while (offsetFromStartOfMetadata % 4 != 0) offsetFromStartOfMetadata++;
            writer.Write((int)offsetFromStartOfMetadata);
            writer.Write((int)this.GuidHeap.Length * 16);
            writer.Write(new char[] { '#', 'G', 'U', 'I', 'D', '\0', '\0', '\0' });
            offsetFromStartOfMetadata += this.GuidHeap.Length * 16;
            writer.Write((int)offsetFromStartOfMetadata);
            int tabsL = this.TablesLength();
            writer.Write((int)tabsL);
            writer.Write(new char[] { '#', '~', '\0', '\0' });
            this.StringHeap.WriteTo(writer.BaseStream);
            int p = (int)this.StringHeap.Length;// +cbStringHeapPad;
            while (p % 4 != 0) { writer.Write((byte)0); p++; }
            this.UserstringHeap.WriteTo(writer.BaseStream);
            p = (int)this.UserstringHeap.Length;// +cbUserStringHeapPad;
            while (p % 4 != 0) { writer.Write((byte)0); p++; }
            this.BlobHeap.WriteTo(writer.BaseStream);
            p = (int)this.BlobHeap.Length;
            while (p % 4 != 0) { writer.Write((byte)0); p++; }
            for (int i = 0, n = this.GuidHeap.Length; i < n; i++)
                writer.Write(this.GuidHeap[i].ToByteArray());
            this.SerializeTables(writer, virtualAddressBase + 72, sdataFixup, tlsFixup);
            this.cliHeader.metaData.size = ((int)writer.BaseStream.Position) - startPos;
        }
#if !ROTOR
        private unsafe void WriteReferenceToPDBFile(BinaryWriter/*!*/ writer, int virtualAddressBase, int fileBase)
        //^ requires this.symWriter != null;
        {
            int startPos = writer.BaseStream.Position;
            this.ntHeader.debugTable.virtualAddress = startPos - fileBase + virtualAddressBase;
            this.ntHeader.debugTable.size = 28;
            ImageDebugDirectory debugDir = new ImageDebugDirectory(true);
            uint pcData = 0;
            this.symWriter.GetDebugInfo(ref debugDir, 0, out pcData, IntPtr.Zero);
            byte[] data = new byte[pcData];
            fixed (byte* pb = data)
            {
                this.symWriter.GetDebugInfo(ref debugDir, pcData, out pcData, (IntPtr)pb);
            }
            writer.Write((int)debugDir.Characteristics);
            writer.Write(this.ntHeader.timeDateStamp);
            writer.Write((ushort)debugDir.MajorVersion);
            writer.Write((ushort)debugDir.MinorVersion);
            writer.Write((int)debugDir.Type);
            writer.Write((int)debugDir.SizeOfData);
            writer.Write((int)startPos + 28 - fileBase + virtualAddressBase); //AddressOfRawData
            writer.Write((int)startPos + 28); //PointerToRawData
            writer.Write((byte[])data);
        }
#endif
        private void SerializeTables(BinaryWriter/*!*/ writer, int mbRVAOffset, Fixup/*!*/ sdataFixup, Fixup/*!*/ tlsFixup)
        //^ requires this.StringHeap != null;
        //^ requires this.GuidHeap != null;
        //^ requires this.BlobHeap != null;
        //^ requires this.tableSize != null;
        //^ requires this.tableRefSize != null;
        {
            writer.Write((int)0); //Reserved
            writer.Write((byte)TargetPlatform.MajorVersion);
            writer.Write((byte)TargetPlatform.MinorVersion);
            byte heapSizes = 0;
            if (this.StringHeap.Length >= 0x10000) heapSizes |= 0x01;
            if (this.GuidHeap.Length >= 0x10000) heapSizes |= 0x02;
            if (this.BlobHeap.Length >= 0x10000) heapSizes |= 0x04;
            writer.Write(heapSizes);
            writer.Write((byte)0); //Reserved
            writer.Write(this.validMask); //Tables that are present
            if (this.UseGenerics)
                writer.Write((long)0x16003301fa00); //Tables that are sorted
            else
                writer.Write((long)0x02003301fa00); //Tables that are sorted
            int[] tableSize = this.tableSize;
            for (int i = 0, n = 0; i < (int)TableIndices.Count; i++)
                if ((n = tableSize[i]) > 0) writer.Write(n);
            if (this.moduleTable != null) this.SerializeModuleTable(writer);
            if (this.typeRefTable != null) this.SerializeTypeRefTable(writer);
            if (this.typeDefTable != null) this.SerializeTypeDefTable(writer);
            if (this.fieldTable != null) this.SerializeFieldTable(writer);
            if (this.methodTable != null) this.SerializeMethodTable(writer, mbRVAOffset);
            if (this.paramTable != null) this.SerializeParamTable(writer);
            if (this.interfaceImplTable != null) this.SerializeInterfaceImplTable(writer);
            if (this.memberRefTable != null) this.SerializeMemberRefTable(writer);
            if (this.constantTable != null) this.SerializeConstantTable(writer);
            if (this.customAttributeTable != null) this.SerializeCustomAttributeTable(writer);
            if (this.fieldMarshalTable != null) this.SerializeFieldMarshalTable(writer);
            if (this.declSecurityTable != null) this.SerializeDeclSecurityTable(writer);
            if (this.classLayoutTable != null) this.SerializeClassLayoutTable(writer);
            if (this.fieldLayoutTable != null) this.SerializeFieldLayoutTable(writer);
            if (this.standAloneSigTable != null) this.SerializeStandAloneSigTable(writer);
            if (this.eventMapTable != null) this.SerializeEventMapTable(writer);
            if (this.eventTable != null) this.SerializeEventTable(writer);
            if (this.propertyMapTable != null) this.SerializePropertyMapTable(writer);
            if (this.propertyTable != null) this.SerializePropertyTable(writer);
            if (this.methodSemanticsTable != null) this.SerializeMethodSemanticsTable(writer);
            if (this.methodImplTable != null) this.SerializeMethodImplTable(writer);
            if (this.moduleRefTable != null) this.SerializeModuleRefTable(writer);
            if (this.typeSpecTable != null) this.SerializeTypeSpecTable(writer);
            if (this.implMapTable != null) this.SerializeImplMapTable(writer);
            if (this.fieldRvaTable != null) this.SerializeFieldRvaTable(writer, mbRVAOffset, sdataFixup, tlsFixup);
            if (this.assemblyTable != null) this.SerializeAssemblyTable(writer);
            if (this.assemblyRefTable != null) this.SerializeAssemblyRefTable(writer);
            if (this.fileTable != null) this.SerializeFileTable(writer);
            if (this.exportedTypeTable != null) this.SerializeExportedTypeTable(writer);
            if (this.manifestResourceTable != null) this.SerializeManifestResourceTable(writer);
            if (this.nestedClassTable != null) this.SerializeNestedClassTable(writer);
            if (this.genericParamTable != null) this.SerializeGenericParamTable(writer);
            if (this.methodSpecTable != null) this.SerializeMethodSpecTable(writer);
            if (this.genericParamConstraintTable != null) this.SerializeGenericParamConstraintTable(writer);
        }
        private void SerializeAssemblyTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.assemblyTable != null;
        {
            int n = this.tableSize[(int)TableIndices.Assembly];
            for (int i = 0; i < n; i++)
            {
                AssemblyRow row = this.assemblyTable[i];
                writer.Write((int)row.HashAlgId);
                writer.Write((short)row.MajorVersion);
                writer.Write((short)row.MinorVersion);
                writer.Write((short)row.BuildNumber);
                writer.Write((short)row.RevisionNumber);
                writer.Write((int)row.Flags);
                this.WriteReference(writer, row.PublicKey, this.blobRefSize);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Culture, this.stringRefSize);
            }
        }
        private void SerializeAssemblyRefTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.assemblyRefTable != null;
        {
            int n = this.tableSize[(int)TableIndices.AssemblyRef];
            for (int i = 0; i < n; i++)
            {
                AssemblyRefRow row = this.assemblyRefTable[i];
                writer.Write((short)row.MajorVersion);
                writer.Write((short)row.MinorVersion);
                writer.Write((short)row.BuildNumber);
                writer.Write((short)row.RevisionNumber);
                writer.Write((int)row.Flags);
                this.WriteReference(writer, row.PublicKeyOrToken, this.blobRefSize);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Culture, this.stringRefSize);
                this.WriteReference(writer, row.HashValue, this.blobRefSize);
            }
        }
        private void SerializeClassLayoutTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.classLayoutTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.ClassLayout];
            for (int i = 0; i < n; i++)
            {
                ClassLayoutRow row = this.classLayoutTable[i];
                writer.Write((short)row.PackingSize);
                writer.Write((int)row.ClassSize);
                this.WriteReference(writer, row.Parent, this.tableRefSize[(int)TableIndices.TypeDef]);
            }
        }
        private void SerializeConstantTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.constantTable != null;
        {
            int n = this.tableSize[(int)TableIndices.Constant];
            for (int i = 0; i < n; i++)
            {
                ConstantRow row = this.constantTable[i];
                writer.Write((byte)row.Type);
                writer.Write((byte)0);
                this.WriteReference(writer, row.Parent, this.constantParentRefSize);
                this.WriteReference(writer, row.Value, this.blobRefSize);
            }
        }
        private void SerializeCustomAttributeTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.customAttributeTable != null;
        {
            int n = this.tableSize[(int)TableIndices.CustomAttribute];
            for (int i = 0; i < n; i++)
            {
                CustomAttributeRow row = this.customAttributeTable[i];
                this.WriteReference(writer, row.Parent, this.customAttributeParentRefSize);
                this.WriteReference(writer, row.Constructor, this.customAttributeConstructorRefSize);
                this.WriteReference(writer, row.Value, this.blobRefSize);
            }
        }
        private void SerializeDeclSecurityTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.declSecurityTable != null;
        {
            int n = this.tableSize[(int)TableIndices.DeclSecurity];
            for (int i = 0; i < n; i++)
            {
                DeclSecurityRow row = this.declSecurityTable[i];
                writer.Write((short)row.Action);
                this.WriteReference(writer, row.Parent, this.declSecurityParentRefSize);
                this.WriteReference(writer, row.PermissionSet, this.blobRefSize);
            }
        }
        private void SerializeEventMapTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.eventMapTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.EventMap];
            for (int i = 0; i < n; i++)
            {
                EventMapRow row = this.eventMapTable[i];
                this.WriteReference(writer, row.Parent, this.tableRefSize[(int)TableIndices.TypeDef]);
                this.WriteReference(writer, row.EventList, this.tableRefSize[(int)TableIndices.Event]);
            }
        }
        private void SerializeEventTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.eventTable != null;
        {
            int n = this.tableSize[(int)TableIndices.Event];
            for (int i = 0; i < n; i++)
            {
                EventRow row = this.eventTable[i];
                writer.Write((short)row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.EventType, this.typeDefOrRefOrSpecSize);
            }
        }
        private void SerializeExportedTypeTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.exportedTypeTable != null;
        {
            int n = this.tableSize[(int)TableIndices.ExportedType];
            for (int i = 0; i < n; i++)
            {
                ExportedTypeRow row = this.exportedTypeTable[i];
                writer.Write((int)row.Flags);
                writer.Write((int)row.TypeDefId);
                this.WriteReference(writer, row.TypeName, this.stringRefSize);
                this.WriteReference(writer, row.TypeNamespace, this.stringRefSize);
                this.WriteReference(writer, row.Implementation, this.implementationRefSize);
            }
        }
        private void SerializeFieldTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.fieldTable != null;
        {
            int n = this.tableSize[(int)TableIndices.Field];
            for (int i = 0; i < n; i++)
            {
                FieldRow row = this.fieldTable[i];
                writer.Write((short)row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Signature, this.blobRefSize);
            }
        }
        private void SerializeFieldLayoutTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.fieldLayoutTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.FieldLayout];
            for (int i = 0; i < n; i++)
            {
                FieldLayoutRow row = this.fieldLayoutTable[i];
                writer.Write((int)row.Offset);
                this.WriteReference(writer, row.Field, this.tableRefSize[(int)TableIndices.Field]);
            }
        }

        private void SerializeFieldMarshalTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.fieldMarshalTable != null;
        {
            int n = this.tableSize[(int)TableIndices.FieldMarshal];
            for (int i = 0; i < n; i++)
            {
                FieldMarshalRow row = this.fieldMarshalTable[i];
                this.WriteReference(writer, row.Parent, this.fieldMarshalParentRefSize);
                this.WriteReference(writer, row.NativeType, this.blobRefSize);
            }
        }
        private void SerializeFieldRvaTable(BinaryWriter/*!*/ writer, int mbRVAOffset, Fixup/*!*/ sdataFixup, Fixup/*!*/ tlsFixup)
        //^ requires this.tableSize != null;
        //^ requires this.fieldRvaTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.FieldRva];
            for (int i = 0; i < n; i++)
            {
                FieldRvaRow row = this.fieldRvaTable[i];
                switch (row.TargetSection)
                {
                    case PESection.SData:
                    case PESection.TLS:
                        Fixup fixup = new Fixup();
                        fixup.fixupLocation = writer.BaseStream.Position;
                        fixup.addressOfNextInstruction = row.RVA;
                        if (row.TargetSection == PESection.SData)
                        {
                            sdataFixup.nextFixUp = fixup;
                            sdataFixup = fixup;
                        }
                        else
                        {
                            sdataFixup.nextFixUp = fixup;
                            sdataFixup = fixup;
                        }
                        writer.Write((int)0);
                        break;
                    case PESection.Text:
                        writer.Write((int)row.RVA + mbRVAOffset);
                        break;
                }
                this.WriteReference(writer, row.Field, this.tableRefSize[(int)TableIndices.Field]);
            }
        }
        private void SerializeFileTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.fileTable != null;
        {
            int n = this.tableSize[(int)TableIndices.File];
            for (int i = 0; i < n; i++)
            {
                FileRow row = this.fileTable[i];
                writer.Write((int)row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.HashValue, this.blobRefSize);
            }
        }
        private void SerializeGenericParamTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.genericParamTable != null;
        {
            int n = this.tableSize[(int)TableIndices.GenericParam];
            bool reallyOldGenericsFileFormat = TargetPlatform.MajorVersion == 1 && TargetPlatform.MinorVersion == 0;
            bool oldGenericsFileFormat = TargetPlatform.MajorVersion == 1 && TargetPlatform.MinorVersion == 1;
            for (int i = 0; i < n; i++)
            {
                GenericParamRow row = this.genericParamTable[i];
                writer.Write((short)row.Number);
                writer.Write((short)row.Flags);
                this.WriteReference(writer, row.Owner, this.typeDefOrMethodDefSize);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                if (oldGenericsFileFormat) this.WriteReference(writer, 0, this.typeDefOrRefOrSpecSize);
                if (reallyOldGenericsFileFormat) writer.Write((short)0);
            }
        }
        private void SerializeGenericParamConstraintTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.genericParamConstraintTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.GenericParamConstraint];
            for (int i = 0; i < n; i++)
            {
                GenericParamConstraintRow row = this.genericParamConstraintTable[i];
                this.WriteReference(writer, row.Param, this.tableRefSize[(int)TableIndices.GenericParam]);
                this.WriteReference(writer, row.Constraint, this.typeDefOrRefOrSpecSize);
            }
        }
        private void SerializeImplMapTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.implMapTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.ImplMap];
            for (int i = 0; i < n; i++)
            {
                ImplMapRow row = this.implMapTable[i];
                writer.Write((short)row.MappingFlags);
                this.WriteReference(writer, row.MemberForwarded, this.memberForwardedRefSize);
                this.WriteReference(writer, row.ImportName, this.stringRefSize);
                this.WriteReference(writer, row.ImportScope, this.tableRefSize[(int)TableIndices.ModuleRef]);
            }
        }
        private void SerializeInterfaceImplTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.interfaceImplTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.InterfaceImpl];
            for (int i = 0; i < n; i++)
            {
                InterfaceImplRow row = this.interfaceImplTable[i];
                this.WriteReference(writer, row.Class, this.tableRefSize[(int)TableIndices.TypeDef]);
                this.WriteReference(writer, row.Interface, this.typeDefOrRefOrSpecSize);
            }
        }
        private void SerializeManifestResourceTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.manifestResourceTable != null;
        {
            int n = this.tableSize[(int)TableIndices.ManifestResource];
            for (int i = 0; i < n; i++)
            {
                ManifestResourceRow row = this.manifestResourceTable[i];
                writer.Write((int)row.Offset);
                writer.Write((int)row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Implementation, this.implementationRefSize);
            }
        }
        private void SerializeMemberRefTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.memberRefTable != null;
        {
            int n = this.tableSize[(int)TableIndices.MemberRef];
            for (int i = 0; i < n; i++)
            {
                MemberRefRow row = this.memberRefTable[i];
                this.WriteReference(writer, row.Class, this.memberRefParentSize);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Signature, this.blobRefSize);
            }
        }
        private void SerializeMethodTable(BinaryWriter/*!*/ writer, int mbRVAOffset)
        //^ requires this.tableSize != null;
        //^ requires this.methodTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.Method];
            int pn = this.paramTable == null ? 1 : this.paramTable.Length + 1;
            for (int i = n - 1; i >= 0; i--)
            {
                MethodRow row = this.methodTable[i];
                if (row.ParamList != 0) pn = row.ParamList; else this.methodTable[i].ParamList = pn;
            }
            for (int i = 0; i < n; i++)
            {
                MethodRow row = this.methodTable[i];
                if (row.RVA < 0)
                    writer.Write((int)0);
                else
                    writer.Write((int)row.RVA + mbRVAOffset);
                writer.Write((short)row.ImplFlags);
                writer.Write((short)row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Signature, this.blobRefSize);
                this.WriteReference(writer, row.ParamList, this.tableRefSize[(int)TableIndices.Param]);
            }
        }
        private void SerializeMethodImplTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.methodImplTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.MethodImpl];
            for (int i = 0; i < n; i++)
            {
                MethodImplRow row = this.methodImplTable[i];
                this.WriteReference(writer, row.Class, this.tableRefSize[(int)TableIndices.TypeDef]);
                this.WriteReference(writer, row.MethodBody, this.methodDefOrRefSize);
                this.WriteReference(writer, row.MethodDeclaration, this.methodDefOrRefSize);
            }
        }
        private void SerializeMethodSemanticsTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.methodSemanticsTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.MethodSemantics];
            for (int i = 0; i < n; i++)
            {
                MethodSemanticsRow row = this.methodSemanticsTable[i];
                writer.Write((short)row.Semantics);
                this.WriteReference(writer, row.Method, this.tableRefSize[(int)TableIndices.Method]);
                this.WriteReference(writer, row.Association, this.hasSemanticRefSize);
            }
        }
        private void SerializeMethodSpecTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.assemblyTable != null;
        //^ requires this.methodSpecTable != null;
        {
            int n = this.tableSize[(int)TableIndices.MethodSpec];
            for (int i = 0; i < n; i++)
            {
                MethodSpecRow row = this.methodSpecTable[i];
                this.WriteReference(writer, row.Method, this.methodDefOrRefSize);
                this.WriteReference(writer, row.Instantiation, this.blobRefSize);
            }
        }
        private void SerializeModuleTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.moduleTable != null;
        {
            int n = this.tableSize[(int)TableIndices.Module];
            for (int i = 0; i < n; i++)
            {
                ModuleRow row = this.moduleTable[i];
                writer.Write((short)row.Generation);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Mvid, this.guidRefSize);
                this.WriteReference(writer, row.EncId, this.guidRefSize);
                this.WriteReference(writer, row.EncBaseId, this.guidRefSize);
            }
        }
        private void SerializeModuleRefTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.moduleRefTable != null;
        {
            int n = this.tableSize[(int)TableIndices.ModuleRef];
            for (int i = 0; i < n; i++)
            {
                ModuleRefRow row = this.moduleRefTable[i];
                this.WriteReference(writer, row.Name, this.stringRefSize);
            }
        }
        private void SerializeNestedClassTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.nestedClassTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.NestedClass];
            for (int i = 0; i < n; i++)
            {
                NestedClassRow row = this.nestedClassTable[i];
                this.WriteReference(writer, row.NestedClass, this.tableRefSize[(int)TableIndices.TypeDef]);
                this.WriteReference(writer, row.EnclosingClass, this.tableRefSize[(int)TableIndices.TypeDef]);
            }
        }
        private void SerializeParamTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.paramTable != null;
        {
            int n = this.tableSize[(int)TableIndices.Param];
            for (int i = 0; i < n; i++)
            {
                ParamRow row = this.paramTable[i];
                writer.Write((short)row.Flags);
                writer.Write((short)row.Sequence);
                this.WriteReference(writer, row.Name, this.stringRefSize);
            }
        }
        private void SerializePropertyTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.propertyTable != null;
        {
            int n = this.tableSize[(int)TableIndices.Property];
            for (int i = 0; i < n; i++)
            {
                PropertyRow row = this.propertyTable[i];
                writer.Write((short)row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Signature, this.blobRefSize);
            }
        }
        private void SerializePropertyMapTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.propertyMapTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.PropertyMap];
            for (int i = 0; i < n; i++)
            {
                PropertyMapRow row = this.propertyMapTable[i];
                this.WriteReference(writer, row.Parent, this.tableRefSize[(int)TableIndices.TypeDef]);
                this.WriteReference(writer, row.PropertyList, this.tableRefSize[(int)TableIndices.Property]);
            }
        }
        private void SerializeStandAloneSigTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.assemblyTable != null;
        //^ requires this.standAloneSigTable != null;
        {
            int n = this.tableSize[(int)TableIndices.StandAloneSig];
            for (int i = 0; i < n; i++)
            {
                StandAloneSigRow row = this.standAloneSigTable[i];
                this.WriteReference(writer, row.Signature, this.blobRefSize);
            }
        }
        private void SerializeTypeDefTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.typeDefTable != null;
        //^ requires this.tableRefSize != null;
        {
            int n = this.tableSize[(int)TableIndices.TypeDef];
            int fn = this.fieldTable == null ? 1 : this.fieldTable.Length + 1;
            int mn = this.methodTable == null ? 1 : this.methodTable.Length + 1;
            for (int i = n - 1; i >= 0; i--)
            {
                TypeDefRow row = this.typeDefTable[i];
                if (row.FieldList != 0) fn = row.FieldList; else this.typeDefTable[i].FieldList = fn;
                if (row.MethodList != 0) mn = row.MethodList; else this.typeDefTable[i].MethodList = mn;
            }
            for (int i = 0; i < n; i++)
            {
                TypeDefRow row = this.typeDefTable[i];
                writer.Write((int)row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Namespace, this.stringRefSize);
                this.WriteReference(writer, row.Extends, this.typeDefOrRefOrSpecSize);
                this.WriteReference(writer, row.FieldList, this.tableRefSize[(int)TableIndices.Field]);
                this.WriteReference(writer, row.MethodList, this.tableRefSize[(int)TableIndices.Method]);
            }
        }
        private void SerializeTypeRefTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.typeRefTable != null;
        {
            int n = this.tableSize[(int)TableIndices.TypeRef];
            for (int i = 0; i < n; i++)
            {
                TypeRefRow row = this.typeRefTable[i];
                this.WriteReference(writer, row.ResolutionScope, this.resolutionScopeRefSize);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Namespace, this.stringRefSize);
            }
        }
        private void SerializeTypeSpecTable(BinaryWriter/*!*/ writer)
        //^ requires this.tableSize != null;
        //^ requires this.typeSpecTable != null;
        {
            int n = this.tableSize[(int)TableIndices.TypeSpec];
            for (int i = 0; i < n; i++)
            {
                TypeSpecRow row = this.typeSpecTable[i];
                this.WriteReference(writer, row.Signature, this.blobRefSize);
            }
        }
        private int TablesLength()
        //^ requires this.BlobHeap != null;
        //^ requires this.GuidHeap != null;
        //^ requires this.StringHeap != null;
        {
            int[] tableSize = this.tableSize = new int[(int)TableIndices.Count];
            int[] tableRefSize = this.tableRefSize = new int[(int)TableIndices.Count];
            int tableCount = 0;
            long validMask = 0;
            for (int i = 0; i < (int)TableIndices.Count; i++)
            {
                int j = 0;
                switch ((TableIndices)i)
                {
                    case TableIndices.Module: if (this.moduleTable != null) j = this.moduleTable.Length; break;
                    case TableIndices.TypeRef: if (this.typeRefTable != null) j = this.typeRefTable.Length; break;
                    case TableIndices.TypeDef: if (this.typeDefTable != null) j = this.typeDefTable.Length; break;
                    case TableIndices.Field: if (this.fieldTable != null) j = this.fieldTable.Length; break;
                    case TableIndices.Method: if (this.methodTable != null) j = this.methodTable.Length; break;
                    case TableIndices.Param: if (this.paramTable != null) j = this.paramTable.Length; break;
                    case TableIndices.InterfaceImpl: if (this.interfaceImplTable != null) j = this.interfaceImplTable.Length; break;
                    case TableIndices.MemberRef: if (this.memberRefTable != null) j = this.memberRefTable.Length; break;
                    case TableIndices.Constant: if (this.constantTable != null) j = this.constantTable.Length; break;
                    case TableIndices.CustomAttribute: if (this.customAttributeTable != null) j = this.customAttributeTable.Length; break;
                    case TableIndices.FieldMarshal: if (this.fieldMarshalTable != null) j = this.fieldMarshalTable.Length; break;
                    case TableIndices.DeclSecurity: if (this.declSecurityTable != null) j = this.declSecurityTable.Length; break;
                    case TableIndices.ClassLayout: if (this.classLayoutTable != null) j = this.classLayoutTable.Length; break;
                    case TableIndices.FieldLayout: if (this.fieldLayoutTable != null) j = this.fieldLayoutTable.Length; break;
                    case TableIndices.StandAloneSig: if (this.standAloneSigTable != null) j = this.standAloneSigTable.Length; break;
                    case TableIndices.EventMap: if (this.eventMapTable != null) j = this.eventMapTable.Length; break;
                    case TableIndices.Event: if (this.eventTable != null) j = this.eventTable.Length; break;
                    case TableIndices.PropertyMap: if (this.propertyMapTable != null) j = this.propertyMapTable.Length; break;
                    case TableIndices.Property: if (this.propertyTable != null) j = this.propertyTable.Length; break;
                    case TableIndices.MethodSemantics: if (this.methodSemanticsTable != null) j = this.methodSemanticsTable.Length; break;
                    case TableIndices.MethodImpl: if (this.methodImplTable != null) j = this.methodImplTable.Length; break;
                    case TableIndices.ModuleRef: if (this.moduleRefTable != null) j = this.moduleRefTable.Length; break;
                    case TableIndices.TypeSpec: if (this.typeSpecTable != null) j = this.typeSpecTable.Length; break;
                    case TableIndices.ImplMap: if (this.implMapTable != null) j = this.implMapTable.Length; break;
                    case TableIndices.FieldRva: if (this.fieldRvaTable != null) j = this.fieldRvaTable.Length; break;
                    case TableIndices.Assembly: if (this.assemblyTable != null) j = this.assemblyTable.Length; break;
                    case TableIndices.AssemblyRef: if (this.assemblyRefTable != null) j = this.assemblyRefTable.Length; break;
                    case TableIndices.File: if (this.fileTable != null) j = this.fileTable.Length; break;
                    case TableIndices.ExportedType: if (this.exportedTypeTable != null) j = this.exportedTypeTable.Length; break;
                    case TableIndices.ManifestResource: if (this.manifestResourceTable != null) j = this.manifestResourceTable.Length; break;
                    case TableIndices.NestedClass: if (this.nestedClassTable != null) j = this.nestedClassTable.Length; break;
                    case TableIndices.GenericParam: if (this.genericParamTable != null) j = this.genericParamTable.Length; break;
                    case TableIndices.MethodSpec: if (this.methodSpecTable != null) j = this.methodSpecTable.Length; break;
                    case TableIndices.GenericParamConstraint: if (this.genericParamConstraintTable != null) j = this.genericParamConstraintTable.Length; break;
                }
                tableSize[i] = j;
                if (j > 0)
                {
                    tableCount++;
                    validMask |= 1L << i;
                }
            }
            this.validMask = validMask;

            for (int i = 0; i < (int)TableIndices.Count; i++)
                tableRefSize[i] = tableSize[i] < 0x10000 ? 2 : 4;
            int blobRefSize = this.blobRefSize = this.BlobHeap.Length < 0x10000 ? 2 : 4;
            int constantParentRefSize = this.constantParentRefSize =
              tableSize[(int)TableIndices.Param] < 0x4000 &&
              tableSize[(int)TableIndices.Field] < 0x4000 &&
              tableSize[(int)TableIndices.Property] < 0x4000 ? 2 : 4;
            int customAttributeParentRefSize = this.customAttributeParentRefSize =
              tableSize[(int)TableIndices.Method] < 0x0800 &&
              tableSize[(int)TableIndices.Field] < 0x0800 &&
              tableSize[(int)TableIndices.TypeRef] < 0x0800 &&
              tableSize[(int)TableIndices.TypeDef] < 0x0800 &&
              tableSize[(int)TableIndices.Param] < 0x0800 &&
              tableSize[(int)TableIndices.InterfaceImpl] < 0x0800 &&
              tableSize[(int)TableIndices.MemberRef] < 0x0800 &&
              tableSize[(int)TableIndices.Module] < 0x0800 &&
              tableSize[(int)TableIndices.DeclSecurity] < 0x0800 &&
              tableSize[(int)TableIndices.Property] < 0x0800 &&
              tableSize[(int)TableIndices.Event] < 0x0800 &&
              tableSize[(int)TableIndices.StandAloneSig] < 0x0800 &&
              tableSize[(int)TableIndices.ModuleRef] < 0x0800 &&
              tableSize[(int)TableIndices.TypeSpec] < 0x0800 &&
              tableSize[(int)TableIndices.Assembly] < 0x0800 &&
              tableSize[(int)TableIndices.File] < 0x0800 &&
              tableSize[(int)TableIndices.ExportedType] < 0x0800 &&
              tableSize[(int)TableIndices.ManifestResource] < 0x0800 &&
              tableSize[(int)TableIndices.GenericParam] < 0x0800 &&
              tableSize[(int)TableIndices.MethodSpec] < 0x0800 &&
              tableSize[(int)TableIndices.GenericParamConstraint] < 0x0800 ? 2 : 4;
            int customAttributeConstructorRefSize = this.customAttributeConstructorRefSize =
              tableSize[(int)TableIndices.Method] < 0x2000 &&
              tableSize[(int)TableIndices.MemberRef] < 0x2000 ? 2 : 4;
            int declSecurityParentRefSize = this.declSecurityParentRefSize =
              tableSize[(int)TableIndices.TypeDef] < 0x4000 &&
              tableSize[(int)TableIndices.Method] < 0x4000 &&
              tableSize[(int)TableIndices.Assembly] < 0x4000 ? 2 : 4;
            int fieldMarshalParentRefSize = this.fieldMarshalParentRefSize =
              tableSize[(int)TableIndices.Field] < 0x8000 &&
              tableSize[(int)TableIndices.Param] < 0x8000 ? 2 : 4;
            int guidRefSize = this.guidRefSize = this.GuidHeap.Length < 0x10000 ? 2 : 4;
            int hasSemanticRefSize = this.hasSemanticRefSize =
              tableSize[(int)TableIndices.Event] < 0x8000 &&
              tableSize[(int)TableIndices.Property] < 0x8000 ? 2 : 4;
            int implementationRefSize = this.implementationRefSize =
              tableSize[(int)TableIndices.File] < 0x4000 &&
              tableSize[(int)TableIndices.AssemblyRef] < 0x4000 &&
              tableSize[(int)TableIndices.ExportedType] < 0x4000 ? 2 : 4;
            int methodDefOrRefSize = this.methodDefOrRefSize =
              tableSize[(int)TableIndices.Method] < 0x8000 &&
              tableSize[(int)TableIndices.MemberRef] < 0x8000 ? 2 : 4;
            int memberRefParentSize = this.memberRefParentSize =
              tableSize[(int)TableIndices.TypeDef] < 0x2000 &&
              tableSize[(int)TableIndices.TypeRef] < 0x2000 &&
              tableSize[(int)TableIndices.ModuleRef] < 0x2000 &&
              tableSize[(int)TableIndices.Method] < 0x2000 &&
              tableSize[(int)TableIndices.TypeSpec] < 0x2000 ? 2 : 4;
            int memberForwardedRefSize = this.memberForwardedRefSize =
              tableSize[(int)TableIndices.Field] < 0x8000 &&
              tableSize[(int)TableIndices.Method] < 0x8000 ? 2 : 4;
            int typeDefOrMethodDefSize = this.typeDefOrMethodDefSize =
              tableSize[(int)TableIndices.TypeDef] < 0x8000 &&
              tableSize[(int)TableIndices.Method] < 0x8000 ? 2 : 4;
            int typeDefOrRefOrSpecSize = this.typeDefOrRefOrSpecSize =
              tableSize[(int)TableIndices.TypeDef] < 0x4000 &&
              tableSize[(int)TableIndices.TypeRef] < 0x4000 &&
              tableSize[(int)TableIndices.TypeSpec] < 0x4000 ? 2 : 4;
            int resolutionScopeRefSize = this.resolutionScopeRefSize =
              tableSize[(int)TableIndices.Module] < 0x4000 &&
              tableSize[(int)TableIndices.ModuleRef] < 0x4000 &&
              tableSize[(int)TableIndices.AssemblyRef] < 0x4000 &&
              tableSize[(int)TableIndices.TypeRef] < 0x4000 ? 2 : 4;
            int stringRefSize = this.stringRefSize = this.StringHeap.Length < 0x10000 ? 2 : 4;

            int length = 0;
            for (int i = 0; i < (int)TableIndices.Count; i++)
            {
                int m = tableSize[i];
                if (m == 0) continue;
                switch ((TableIndices)i)
                {
                    case TableIndices.Module: length += m * (2 + stringRefSize + 3 * guidRefSize); break;
                    case TableIndices.TypeRef: length += m * (resolutionScopeRefSize + 2 * stringRefSize); break;
                    case TableIndices.TypeDef: length += m * (4 + 2 * stringRefSize + typeDefOrRefOrSpecSize + tableRefSize[(int)TableIndices.Field] + tableRefSize[(int)TableIndices.Method]); break;
                    case TableIndices.Field: length += m * (2 + stringRefSize + blobRefSize); break;
                    case TableIndices.Method: length += m * (8 + stringRefSize + blobRefSize + tableRefSize[(int)TableIndices.Param]); break;
                    case TableIndices.Param: length += m * (4 + stringRefSize); break;
                    case TableIndices.InterfaceImpl: length += m * (tableRefSize[(int)TableIndices.TypeDef] + typeDefOrRefOrSpecSize); break;
                    case TableIndices.MemberRef: length += m * (memberRefParentSize + stringRefSize + blobRefSize); break;
                    case TableIndices.Constant: length += m * (2 + constantParentRefSize + blobRefSize); break;
                    case TableIndices.CustomAttribute: length += m * (customAttributeParentRefSize + customAttributeConstructorRefSize + blobRefSize); break;
                    case TableIndices.FieldMarshal: length += m * (fieldMarshalParentRefSize + blobRefSize); break;
                    case TableIndices.DeclSecurity: length += m * (2 + declSecurityParentRefSize + blobRefSize); break;
                    case TableIndices.ClassLayout: length += m * (6 + tableRefSize[(int)TableIndices.TypeDef]); break;
                    case TableIndices.FieldLayout: length += m * (4 + tableRefSize[(int)TableIndices.Field]); break;
                    case TableIndices.StandAloneSig: length += m * (blobRefSize); break;
                    case TableIndices.EventMap: length += m * (tableRefSize[(int)TableIndices.TypeDef] + tableRefSize[(int)TableIndices.Event]); break;
                    case TableIndices.Event: length += m * (2 + stringRefSize + typeDefOrRefOrSpecSize); break;
                    case TableIndices.PropertyMap: length += m * (tableRefSize[(int)TableIndices.TypeDef] + tableRefSize[(int)TableIndices.Property]); break;
                    case TableIndices.Property: length += m * (2 + stringRefSize + blobRefSize); break;
                    case TableIndices.MethodSemantics: length += m * (2 + tableRefSize[(int)TableIndices.Method] + hasSemanticRefSize); break;
                    case TableIndices.MethodImpl: length += m * (tableRefSize[(int)TableIndices.TypeDef] + 2 * methodDefOrRefSize); break;
                    case TableIndices.ModuleRef: length += m * (stringRefSize); break;
                    case TableIndices.TypeSpec: length += m * (blobRefSize); break;
                    case TableIndices.ImplMap: length += m * (2 + memberForwardedRefSize + stringRefSize + tableRefSize[(int)TableIndices.ModuleRef]); break;
                    case TableIndices.FieldRva: length += m * (4 + tableRefSize[(int)TableIndices.Field]); break;
                    case TableIndices.EncLog: throw new InvalidMetadataException(ExceptionStrings.ENCLogTableEncountered);
                    case TableIndices.EncMap: throw new InvalidMetadataException(ExceptionStrings.ENCMapTableEncountered);
                    case TableIndices.Assembly: length += m * (16 + blobRefSize + 2 * stringRefSize); break;
                    case TableIndices.AssemblyRef: length += m * (12 + 2 * blobRefSize + 2 * stringRefSize); break;
                    case TableIndices.File: length += m * (4 + stringRefSize + blobRefSize); break;
                    case TableIndices.ExportedType: length += m * (8 + 2 * stringRefSize + implementationRefSize); break;
                    case TableIndices.ManifestResource: length += m * (8 + stringRefSize + implementationRefSize); break;
                    case TableIndices.NestedClass: length += m * (2 * tableRefSize[(int)TableIndices.TypeDef]); break;
                    case TableIndices.GenericParam:
                        if (TargetPlatform.MajorVersion == 1 && TargetPlatform.MinorVersion == 0)
                            length += m * (6 + typeDefOrMethodDefSize + stringRefSize + typeDefOrRefOrSpecSize);
                        else if (TargetPlatform.MajorVersion == 1 && TargetPlatform.MinorVersion == 1)
                            length += m * (4 + typeDefOrMethodDefSize + stringRefSize + typeDefOrRefOrSpecSize);
                        else
                            length += m * (4 + typeDefOrMethodDefSize + stringRefSize);
                        break;
                    case TableIndices.MethodSpec: length += m * (methodDefOrRefSize + blobRefSize); break;
                    case TableIndices.GenericParamConstraint: length += m * (tableRefSize[(int)TableIndices.GenericParam] + typeDefOrRefOrSpecSize); break;
                }
            }
            length += 24 + (tableCount * 4);
            return length;
        }

        private NTHeader/*!*/ ntHeader = new NTHeader();
        private CLIHeader/*!*/ cliHeader = new CLIHeader();
        private SectionHeader[] sectionHeaders;

        internal void WritePE(BinaryWriter/*!*/ writer)
        //^ requires this.SdataHeap != null;
        //^ requires this.TlsHeap != null;
        {
            this.cliHeader.entryPointToken = this.entryPointToken;
            switch (this.moduleKind)
            {
                case ModuleKindFlags.ConsoleApplication:
                    this.ntHeader.subsystem = 3;
                    break;
                case ModuleKindFlags.DynamicallyLinkedLibrary:
                    this.ntHeader.characteristics |= 0x2000;
                    this.ntHeader.subsystem = 3;
                    break;
                case ModuleKindFlags.WindowsApplication:
                    this.ntHeader.subsystem = 2;
                    break;
            }
            int numSectionHeaders = 2;
            if (this.SdataHeap.Length > 0) numSectionHeaders++;
            if (this.TlsHeap.Length > 0) numSectionHeaders++;
            if (this.Win32Resources != null && this.Win32Resources.Count > 0) numSectionHeaders++;
            this.sectionHeaders = new SectionHeader[numSectionHeaders];
            this.ntHeader.numberOfSections = (ushort)numSectionHeaders;
            this.ntHeader.timeDateStamp = (int)((DateTime.Now.ToUniversalTime() - NineteenSeventy).TotalSeconds);

            //Write out .text section for meta data tables, method bodies, address tables and entry point stub
            Fixup sdataFixup = new Fixup();
            Fixup tlsFixup = new Fixup();
            SectionHeader textSection = new SectionHeader();
            textSection.name = ".text";
            textSection.virtualAddress = 8192;
            int sizeOfPeHeaders = 376 + 40 * numSectionHeaders;
            textSection.pointerToRawData = ((int)Math.Ceiling(sizeOfPeHeaders / (double)this.fileAlignment)) * this.fileAlignment;
            textSection.characteristics = 0x60000020;
            writer.BaseStream.Position = textSection.pointerToRawData + 72; //Leave 72 bytes for CLI header
            this.SerializeMetadata(writer, textSection.virtualAddress, sdataFixup, tlsFixup);
            int RVAofEntryPointJumpTarget = this.WriteImportTableAndEntryPointStub(writer, ref textSection);
#if !ROTOR
            if (this.symWriter != null) this.WriteReferenceToPDBFile(writer, textSection.virtualAddress, textSection.pointerToRawData);
#endif
            int len = textSection.virtualSize = ((int)writer.BaseStream.Position) - textSection.pointerToRawData;
            textSection.sizeOfRawData = ((int)Math.Ceiling(len / (double)this.fileAlignment)) * this.fileAlignment;
            this.sectionHeaders[0] = textSection;
            writer.BaseStream.Position = textSection.pointerToRawData;
            this.ntHeader.cliHeaderTable.virtualAddress = textSection.virtualAddress;
            this.ntHeader.cliHeaderTable.size = 72;
            WriteCLIHeader(writer); //Write CLI header last so that forward pointers can be filled in first

            int sectionHeaderIndex = 1;
            SectionHeader previousSection = textSection;
            int n = this.ntHeader.sectionAlignment;
            int m = this.fileAlignment;

            if (this.SdataHeap.Length > 0)
            {
                SectionHeader sdataSection = new SectionHeader();
                sdataSection.name = ".sdata";
                int vaddr = sdataSection.virtualAddress = previousSection.virtualAddress + n * (int)Math.Ceiling(previousSection.sizeOfRawData / (double)n);
                sdataSection.virtualSize = this.SdataHeap.Length;
                sdataSection.pointerToRawData = previousSection.pointerToRawData + m * (int)Math.Ceiling(previousSection.sizeOfRawData / (double)m);
                sdataSection.characteristics = unchecked((int)0xC0000040);
                writer.BaseStream.Position = sdataSection.pointerToRawData;
                this.SdataHeap.WriteTo(writer.BaseStream);
                len = sdataSection.virtualSize = ((int)writer.BaseStream.Position) - sdataSection.pointerToRawData;
                writer.BaseStream.Position += m - (len % this.fileAlignment) - 1;
                writer.Write((byte)0);
                sdataSection.sizeOfRawData = ((int)Math.Ceiling(len / (double)this.fileAlignment)) * this.fileAlignment;
                sdataFixup = sdataFixup.nextFixUp; //Skip over dummy header
                while (sdataFixup != null)
                {
                    writer.BaseStream.Position = sdataFixup.fixupLocation;
                    writer.Write((int)(vaddr + sdataFixup.addressOfNextInstruction));
                    sdataFixup = sdataFixup.nextFixUp;
                }
                this.sectionHeaders[sectionHeaderIndex++] = sdataSection;
                previousSection = sdataSection;
            }

            if (this.TlsHeap.Length > 0)
            {
                SectionHeader tlsSection = new SectionHeader();
                tlsSection.name = ".tls";
                int vaddr = tlsSection.virtualAddress = previousSection.virtualAddress + n * (int)Math.Ceiling(previousSection.sizeOfRawData / (double)n);
                tlsSection.virtualSize = this.SdataHeap.Length;
                tlsSection.pointerToRawData = previousSection.pointerToRawData + m * (int)Math.Ceiling(previousSection.sizeOfRawData / (double)m);
                tlsSection.characteristics = unchecked((int)0xC0000040);
                writer.BaseStream.Position = tlsSection.pointerToRawData;
                this.TlsHeap.WriteTo(writer.BaseStream);
                len = tlsSection.virtualSize = ((int)writer.BaseStream.Position) - tlsSection.pointerToRawData;
                writer.BaseStream.Position += m - (len % this.fileAlignment) - 1;
                writer.Write((byte)0);
                tlsSection.sizeOfRawData = ((int)Math.Ceiling(len / (double)this.fileAlignment)) * this.fileAlignment;
                tlsFixup = tlsFixup.nextFixUp; //Skip over dummy header
                while (tlsFixup != null)
                {
                    writer.BaseStream.Position = tlsFixup.fixupLocation;
                    writer.Write((int)(vaddr + tlsFixup.addressOfNextInstruction));
                    tlsFixup = tlsFixup.nextFixUp;
                }
                this.sectionHeaders[sectionHeaderIndex++] = tlsSection;
                previousSection = tlsSection;
            }

            if (this.Win32Resources != null && this.Win32Resources.Count > 0)
            {
                SectionHeader rsrcSection = new SectionHeader();
                rsrcSection.name = ".rsrc";
                rsrcSection.virtualAddress = previousSection.virtualAddress + n * (int)Math.Ceiling(previousSection.sizeOfRawData / (double)n);
                rsrcSection.pointerToRawData = previousSection.pointerToRawData + m * (int)Math.Ceiling(previousSection.sizeOfRawData / (double)m);
                rsrcSection.characteristics = 0x40000040;
                writer.BaseStream.Position = rsrcSection.pointerToRawData;
                this.WriteWin32Resources(writer, rsrcSection.virtualAddress);
                len = rsrcSection.virtualSize = ((int)writer.BaseStream.Position) - rsrcSection.pointerToRawData;
                writer.BaseStream.Position += m - (len % this.fileAlignment) - 1;
                writer.Write((byte)0);
                rsrcSection.sizeOfRawData = ((int)Math.Ceiling(len / (double)this.fileAlignment)) * this.fileAlignment;
                this.sectionHeaders[sectionHeaderIndex++] = rsrcSection;
                this.ntHeader.resourceTable.virtualAddress = rsrcSection.virtualAddress;
                this.ntHeader.resourceTable.size = rsrcSection.virtualSize;
                this.ntHeader.sizeOfInitializedData += rsrcSection.sizeOfRawData;
                previousSection = rsrcSection;
            }

            //Write out .reloc section for entry point stub relocation table
            SectionHeader relocSection = new SectionHeader();
            relocSection.name = ".reloc";
            relocSection.virtualAddress = previousSection.virtualAddress + n * (int)Math.Ceiling(previousSection.sizeOfRawData / (double)n);
            relocSection.virtualSize = 12;
            relocSection.pointerToRawData = previousSection.pointerToRawData + m * (int)Math.Ceiling(previousSection.sizeOfRawData / (double)m);
            relocSection.sizeOfRawData = m;
            relocSection.characteristics = 0x42000040;
            writer.BaseStream.Position = relocSection.pointerToRawData;
            writer.Write((int)((RVAofEntryPointJumpTarget / 4096) * 4096)); //Page RVA
            writer.Write((int)12);
            int offsetWithinPage = RVAofEntryPointJumpTarget % 4096;
            short s = (short)(3 << 12 | offsetWithinPage);
            writer.Write(s);
            writer.Write((short)0); //next chunk's RVA
            writer.BaseStream.Position += m - 13;
            writer.Write((byte)0);
            this.sectionHeaders[sectionHeaderIndex] = relocSection;
            this.ntHeader.baseRelocationTable.virtualAddress = relocSection.virtualAddress;
            this.ntHeader.baseRelocationTable.size = relocSection.virtualSize;
            this.ntHeader.sizeOfInitializedData += relocSection.sizeOfRawData;

            //Write PE headers. Do this last because forward pointers are filled in by preceding code
            writer.BaseStream.Position = 0;
            writer.Write(dosHeader);
            WriteNTHeader(writer);
            WriteSectionHeaders(writer);
        }
        private class Directory
        {
            internal string Name;
            internal int ID;
            internal int NumberOfNamedEntries;
            internal int NumberOfIdEntries;
            internal ArrayList/*!*/ Entries;
            internal Directory(string Name, int ID)
            {
                this.Name = Name;
                this.ID = ID;
                this.Entries = new ArrayList();
                //^ base();
            }
        }
        private void WriteWin32Resources(BinaryWriter/*!*/ writer, int virtualAddressBase)
        //^ requires this.Win32Resources != null;
        {
            Win32ResourceList rsrcs = this.Win32Resources;
            BinaryWriter dataHeap = new BinaryWriter(new MemoryStream(), System.Text.Encoding.Unicode);
            //Construct a tree of array lists to represent the directory and make it easier to compute offsets
            Directory TypeDirectory = new Directory("", 0);
            Directory NameDirectory = null;
            Directory LanguageDirectory = null;
            int lastTypeID = int.MinValue;
            string lastTypeName = null;
            int lastID = int.MinValue;
            string lastName = null;
            int sizeOfDirectoryTree = 16;
            for (int i = 0, n = rsrcs.Count; i < n; i++)
            {
                Win32Resource r = rsrcs[i];
                bool typeDifferent = (r.TypeId < 0 && r.TypeName != lastTypeName) || r.TypeId > lastTypeID;
                if (typeDifferent)
                {
                    lastTypeID = r.TypeId;
                    lastTypeName = r.TypeName;
                    if (lastTypeID < 0) TypeDirectory.NumberOfNamedEntries++; else TypeDirectory.NumberOfIdEntries++;
                    sizeOfDirectoryTree += 24;
                    TypeDirectory.Entries.Add(NameDirectory = new Directory(lastTypeName, lastTypeID));
                }
                //^ assume NameDirectory != null;
                if (typeDifferent || (r.Id < 0 && r.Name != lastName) || r.Id > lastID)
                {
                    lastID = r.Id;
                    lastName = r.Name;
                    if (lastID < 0) NameDirectory.NumberOfNamedEntries++; else NameDirectory.NumberOfIdEntries++;
                    sizeOfDirectoryTree += 24;
                    NameDirectory.Entries.Add(LanguageDirectory = new Directory(lastName, lastID));
                }
                //^ assume LanguageDirectory != null;
                LanguageDirectory.NumberOfIdEntries++;
                sizeOfDirectoryTree += 8;
                LanguageDirectory.Entries.Add(r);
                continue;
            }
            this.WriteDirectory(TypeDirectory, writer, 0, 0, sizeOfDirectoryTree, virtualAddressBase, dataHeap);
            dataHeap.BaseStream.WriteTo(writer.BaseStream);
        }
        private void WriteDirectory(Directory/*!*/ directory, BinaryWriter/*!*/ writer, int offset, int level, int sizeOfDirectoryTree,
          int virtualAddressBase, BinaryWriter/*!*/ dataHeap)
        {
            writer.Write((int)0); //Characteristics
            writer.Write((int)0); //Timestamp
            writer.Write((int)0); //Version
            writer.Write((short)directory.NumberOfNamedEntries);
            writer.Write((short)directory.NumberOfIdEntries);
            int n = directory.Entries.Count;
            int k = offset + 16 + n * 8;
            for (int i = 0; i < n; i++)
            {
                int id = int.MinValue;
                string name = null;
                int nOff = dataHeap.BaseStream.Position + sizeOfDirectoryTree;
                int dOff = k;
                Directory subDir = directory.Entries[i] as Directory;
                if (subDir != null)
                {
                    id = subDir.ID;
                    name = subDir.Name;
                    if (level == 0)
                        k += this.SizeOfDirectory(subDir);
                    else
                        k += 16 + 8 * subDir.Entries.Count;
                }
                else
                {
                    Win32Resource r = (Win32Resource)directory.Entries[i];
                    id = level == 0 ? r.TypeId : level == 1 ? r.Id : r.LanguageId;
                    name = level == 0 ? r.TypeName : level == 1 ? r.Name : null;
                    dataHeap.Write((int)(virtualAddressBase + sizeOfDirectoryTree + 16 + dataHeap.BaseStream.Position));
                    dataHeap.Write((int)r.Data.Length);
                    dataHeap.Write((int)r.CodePage);
                    dataHeap.Write((int)0);
                    dataHeap.Write(r.Data);
                }
                if (id >= 0)
                    writer.Write(id);
                else
                {
                    if (name == null) name = "";
                    writer.Write(((uint)nOff) | 0x80000000);
                    dataHeap.Write((ushort)name.Length);
                    dataHeap.Write(name.ToCharArray());  //REVIEW: what happens if the name contains chars that do not fit into a single utf8 code point?
                }
                if (subDir != null)
                    writer.Write(((uint)dOff) | 0x80000000);
                else
                    writer.Write(nOff);
            }
            k = offset + 16 + n * 8;
            for (int i = 0; i < n; i++)
            {
                Directory subDir = directory.Entries[i] as Directory;
                if (subDir != null)
                {
                    this.WriteDirectory(subDir, writer, k, level + 1, sizeOfDirectoryTree, virtualAddressBase, dataHeap);
                    if (level == 0)
                        k += this.SizeOfDirectory(subDir);
                    else
                        k += 16 + 8 * subDir.Entries.Count;
                }
            }
        }
        private int SizeOfDirectory(Directory/*!*/ directory)
        {
            int n = directory.Entries.Count;
            int size = 16 + 8 * n;
            for (int i = 0; i < n; i++)
            {
                Directory subDir = directory.Entries[i] as Directory;
                if (subDir != null)
                    size += 16 + 8 * subDir.Entries.Count;
            }
            return size;
        }
        private static readonly byte[] dosHeader = new byte[]{
       0x4d, 0x5a, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00,
       0x04, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00,
       0xb8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
       0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
       0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00,
       0x0e, 0x1f, 0xba, 0x0e, 0x00, 0xb4, 0x09, 0xcd,
       0x21, 0xb8, 0x01, 0x4c, 0xcd, 0x21, 0x54, 0x68,
       0x69, 0x73, 0x20, 0x70, 0x72, 0x6f, 0x67, 0x72,
       0x61, 0x6d, 0x20, 0x63, 0x61, 0x6e, 0x6e, 0x6f,
       0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6e,
       0x20, 0x69, 0x6e, 0x20, 0x44, 0x4f, 0x53, 0x20,
       0x6d, 0x6f, 0x64, 0x65, 0x2e, 0x0d, 0x0d, 0x0a,
       0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    };

        static readonly DateTime NineteenSeventy = new DateTime(1970, 1, 1).ToUniversalTime();
        private void WriteNTHeader(BinaryWriter/*!*/ writer)
        //^ requires this.sectionHeaders != null;
        {
            NTHeader ntHeader = this.ntHeader;
            writer.Write(ntHeader.signature);
            if ((this.peKind & PEKindFlags.Requires64bits) == 0)
            {
                ntHeader.magic = 0x10B; //PE32
                ntHeader.machine = 0x014c; //I386
            }
            else
            {
                ntHeader.characteristics &= 0xFEFF; //Not 32-bit
                ntHeader.characteristics |= 0x0020; //Can handle >2gb addresses
                ntHeader.magic = 0x20B; //PE32+
                if ((this.peKind & PEKindFlags.AMD) != 0)
                    ntHeader.machine = 0x8664; //AMD64
                else
                    ntHeader.machine = 0x0200; //IA64
                ntHeader.sizeOfOptionalHeader += 16;
            }
            writer.Write(ntHeader.machine);
            writer.Write(ntHeader.numberOfSections);
            writer.Write(ntHeader.timeDateStamp);
            writer.Write(ntHeader.pointerToSymbolTable);
            writer.Write(ntHeader.numberOfSymbols);
            writer.Write(ntHeader.sizeOfOptionalHeader);
            writer.Write(ntHeader.characteristics);
            writer.Write(ntHeader.magic);
            writer.Write((byte)TargetPlatform.LinkerMajorVersion);
            writer.Write((byte)TargetPlatform.LinkerMinorVersion);
            writer.Write(this.sectionHeaders[0].sizeOfRawData); //sizeOfCode
            writer.Write(ntHeader.sizeOfInitializedData);
            writer.Write(ntHeader.sizeOfUninitializedData);
            writer.Write(ntHeader.addressOfEntryPoint);
            writer.Write(this.sectionHeaders[0].virtualAddress);  //baseOfCode
            if (ntHeader.magic == 0x10B)
            {
                if (this.sectionHeaders.Length > 1)
                    writer.Write(this.sectionHeaders[1].virtualAddress);  //baseOfData
                else
                    writer.Write((int)0);
                writer.Write((int)ntHeader.imageBase);
            }
            else
            {
                writer.Write(ntHeader.imageBase);
            }
            writer.Write(ntHeader.sectionAlignment);
            writer.Write(this.fileAlignment);
            writer.Write(ntHeader.majorOperatingSystemVersion);
            writer.Write(ntHeader.minorOperatingSystemVersion);
            writer.Write(ntHeader.majorImageVersion);
            writer.Write(ntHeader.minorImageVersion);
            writer.Write(ntHeader.majorSubsystemVersion);
            writer.Write(ntHeader.minorSubsystemVersion);
            writer.Write(ntHeader.win32VersionValue);
            int sectionPages = (int)(Math.Ceiling(this.sectionHeaders[this.sectionHeaders.Length - 1].virtualSize /
                                                 (double)ntHeader.sectionAlignment) * ntHeader.sectionAlignment);
            writer.Write(this.sectionHeaders[this.sectionHeaders.Length - 1].virtualAddress + sectionPages); //sizeOfImage
            writer.Write(this.sectionHeaders[0].pointerToRawData); //sizeOfHeaders
            writer.Write(ntHeader.checkSum);
            writer.Write(ntHeader.subsystem);
            writer.Write(ntHeader.dllCharacteristics);
            if (ntHeader.magic == 0x10B)
            {
                writer.Write((int)ntHeader.sizeOfStackReserve);
                writer.Write((int)ntHeader.sizeOfStackCommit);
                writer.Write((int)ntHeader.sizeOfHeapReserve);
                writer.Write((int)ntHeader.sizeOfHeapCommit);
            }
            else
            {
                writer.Write(ntHeader.sizeOfStackReserve);
                writer.Write(ntHeader.sizeOfStackCommit);
                writer.Write(ntHeader.sizeOfHeapReserve);
                writer.Write(ntHeader.sizeOfHeapCommit);
            }
            writer.Write(ntHeader.loaderFlags);
            writer.Write(ntHeader.numberOfDataDirectories);
            writer.Write(ntHeader.exportTable.virtualAddress);
            writer.Write(ntHeader.exportTable.size);
            writer.Write(ntHeader.importTable.virtualAddress);
            writer.Write(ntHeader.importTable.size);
            writer.Write(ntHeader.resourceTable.virtualAddress);
            writer.Write(ntHeader.resourceTable.size);
            writer.Write(ntHeader.exceptionTable.virtualAddress);
            writer.Write(ntHeader.exceptionTable.size);
            writer.Write(ntHeader.certificateTable.virtualAddress);
            writer.Write(ntHeader.certificateTable.size);
            writer.Write(ntHeader.baseRelocationTable.virtualAddress);
            writer.Write(ntHeader.baseRelocationTable.size);
            writer.Write(ntHeader.debugTable.virtualAddress);
            writer.Write(ntHeader.debugTable.size);
            writer.Write(ntHeader.copyrightTable.virtualAddress);
            writer.Write(ntHeader.copyrightTable.size);
            writer.Write(ntHeader.globalPointerTable.virtualAddress);
            writer.Write(ntHeader.globalPointerTable.size);
            writer.Write(ntHeader.threadLocalStorageTable.virtualAddress);
            writer.Write(ntHeader.threadLocalStorageTable.size);
            writer.Write(ntHeader.loadConfigTable.virtualAddress);
            writer.Write(ntHeader.loadConfigTable.size);
            writer.Write(ntHeader.boundImportTable.virtualAddress);
            writer.Write(ntHeader.boundImportTable.size);
            writer.Write(ntHeader.importAddressTable.virtualAddress);
            writer.Write(ntHeader.importAddressTable.size);
            writer.Write(ntHeader.delayImportTable.virtualAddress);
            writer.Write(ntHeader.delayImportTable.size);
            writer.Write(ntHeader.cliHeaderTable.virtualAddress);
            writer.Write(ntHeader.cliHeaderTable.size);
            writer.Write((long)0);
        }
        private void WriteSectionHeaders(BinaryWriter/*!*/ writer)
        //^ requires this.sectionHeaders != null;
        {
            SectionHeader[] sectionHeaders = this.sectionHeaders;
            for (int i = 0, n = this.sectionHeaders.Length; i < n; i++)
            {
                SectionHeader hdr = sectionHeaders[i];
                //^ assume hdr.name != null;
                for (int j = 0, m = hdr.name.Length; j < 8; j++)
                    if (j < m) writer.Write(hdr.name[j]); else writer.Write((byte)0);
                writer.Write(hdr.virtualSize);
                writer.Write(hdr.virtualAddress);
                writer.Write(hdr.sizeOfRawData);
                writer.Write(hdr.pointerToRawData);
                writer.Write(hdr.pointerToRelocations);
                writer.Write(hdr.pointerToLinenumbers);
                writer.Write(hdr.numberOfRelocations);
                writer.Write(hdr.numberOfLinenumbers);
                writer.Write(hdr.characteristics);
            }
        }
        private void WriteCLIHeader(BinaryWriter/*!*/ writer)
        {
            CLIHeader hdr = this.cliHeader;
            writer.Write(hdr.cb);
            writer.Write((ushort)2);
            if (this.UseGenerics)
                writer.Write((ushort)5); //Violates the ECMA standard (25.3.3 of Partition II), but apparently necessary
            else
                writer.Write((ushort)0);
            writer.Write(hdr.metaData.virtualAddress);
            writer.Write(hdr.metaData.size);
            if ((this.peKind & PEKindFlags.Requires32bits) != 0) hdr.flags |= 2;
            if ((this.peKind & PEKindFlags.ILonly) != 0) hdr.flags |= 1;
            if (this.TrackDebugData) hdr.flags |= 0x10000;
            writer.Write(hdr.flags);
            writer.Write(hdr.entryPointToken);
            writer.Write(hdr.resources.virtualAddress);
            writer.Write(hdr.resources.size);
            writer.Write(hdr.strongNameSignature.virtualAddress);
            writer.Write(hdr.strongNameSignature.size);
            writer.Write(hdr.codeManagerTable.virtualAddress);
            writer.Write(hdr.codeManagerTable.size);
            writer.Write(hdr.vtableFixups.virtualAddress);
            writer.Write(hdr.vtableFixups.size);
        }
        private int WriteImportTableAndEntryPointStub(BinaryWriter/*!*/ writer, ref SectionHeader textSection)
        {
            bool use32bitAddresses = (this.peKind & PEKindFlags.Requires64bits) == 0;
            int pos = (int)writer.BaseStream.Position;
            while (pos % 4 != 0) { pos++; writer.Write((byte)0); }
            int ITrva = textSection.virtualAddress + pos - textSection.pointerToRawData;
            int ITLrva = ITrva + 40;
            int rvasize = 12; // size of relocation sections
            int hintRva = ITLrva + (use32bitAddresses ? 8 : 16);  //position of name of entry point in runtime dll
            int nameRva = hintRva + 14; // position of name of runtime dll
            int ITArva = nameRva + 14 +  // size of name of runtime dll
                                   4 + // size of entry point code 0000ff25 
                                   rvasize; // size of relocation fixup

            this.ntHeader.addressOfEntryPoint = ITArva - rvasize - 2;
            this.ntHeader.importTable.virtualAddress = ITrva;
            this.ntHeader.importTable.size = this.ntHeader.addressOfEntryPoint - ITrva - 2;
            this.ntHeader.importAddressTable.virtualAddress = ITArva;
            this.ntHeader.importAddressTable.size = 8;
            //Import table
            writer.Write((int)ITLrva);
            writer.Write((int)0);
            writer.Write((int)0);
            writer.Write((int)nameRva);
            writer.Write((int)ITArva);
            writer.BaseStream.Position += 20;
            //Import Lookup table
            if (use32bitAddresses)
            {
                writer.Write((int)hintRva);
                writer.Write((int)0);
            }
            else
            {
                writer.Write((long)hintRva);
                writer.Write((long)0);
            }
            //Hint table
            writer.Write((short)0); //Hint
            string entryPointName = this.moduleKind == ModuleKindFlags.DynamicallyLinkedLibrary ? "_CorDllMain" : "_CorExeMain";
            foreach (char ch in entryPointName.ToCharArray()) writer.Write((byte)ch);
            writer.Write((byte)0);
            //name of CLR runtime dll
            foreach (char ch in "mscoree.dll".ToCharArray()) writer.Write((byte)ch);
            writer.Write((byte)0);
            writer.Write((short)0);
            //entry point code, consisting of a jump indirect to _CorXXXMain
            writer.Write((short)0); //padding so that address to replace is on a word boundary
            writer.Write((byte)0xff);
            writer.Write((byte)0x25);
            writer.Write((int)ITArva + (int)this.ntHeader.imageBase); //REVIEW: is this OK for 64 bit?
            writer.Write((int)0); // relocation fixup must be 12 bytes.
            writer.Write((int)0);
            //Import Address Table
            if (use32bitAddresses)
            {
                writer.Write((int)hintRva);
                writer.Write((int)0);
            }
            else
            {
                writer.Write((long)hintRva);
                writer.Write((long)0);
            }
            //Return RVA of the target address of the indirect jump at the entry point
            return ITArva - rvasize;
        }
        private void WriteReference(BinaryWriter/*!*/ writer, int index, int refSize)
        {
            if (refSize == 2) writer.Write((short)index); else writer.Write(index);
        }
    }
#endif
}
