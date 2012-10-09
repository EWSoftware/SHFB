// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

#if !NoWriter
using System;
using System.Collections;
#if CCINamespace
using Microsoft.Cci.Metadata;
#else
using System.Compiler.Metadata;
#endif
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
#if !ROTOR
using System.Security.Cryptography;
#endif
using System.Text;

#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler
{
#endif
#if !ROTOR
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006"), SuppressUnmanagedCodeSecurity]
    interface ISymUnmanagedDocumentWriter
    {
        void SetSource(uint sourceSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] source);
        void SetCheckSum(ref Guid algorithmId, uint checkSumSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] checkSum);
    };
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("2DE91396-3844-3B1D-8E91-41C24FD672EA"), SuppressUnmanagedCodeSecurity]
    interface ISymUnmanagedWriter
    {
        ISymUnmanagedDocumentWriter DefineDocument(string url, ref Guid language, ref Guid languageVendor, ref Guid documentType);
        void SetUserEntryPoint(uint entryMethod);
        void OpenMethod(uint method);
        void CloseMethod();
        uint OpenScope(uint startOffset);
        void CloseScope(uint endOffset);
        void SetScopeRange(uint scopeID, uint startOffset, uint endOffset);
        void DefineLocalVariable(string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint startOffset, uint endOffset);
        void DefineParameter(string name, uint attributes, uint sequence, uint addrKind, uint addr1, uint addr2, uint addr3);
        void DefineField(uint parent, string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint addr3);
        void DefineGlobalVariable(string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint addr3);
        void Close();
        void SetSymAttribute(uint parent, string name, uint cData, IntPtr signature);
        void OpenNamespace(string name);
        void CloseNamespace();
        void UsingNamespace(string fullName);
        void SetMethodSourceRange(ISymUnmanagedDocumentWriter startDoc, uint startLine, uint startColumn, object endDoc, uint endLine, uint endColumn);
        void Initialize([MarshalAs(UnmanagedType.IUnknown)]object emitter, string filename, [MarshalAs(UnmanagedType.IUnknown)]object pIStream, bool fFullBuild);
        void GetDebugInfo(ref ImageDebugDirectory pIDD, uint cData, out uint pcData, IntPtr data);
        void DefineSequencePoints(ISymUnmanagedDocumentWriter document, uint spCount,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] uint[] offsets,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] uint[] lines,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] uint[] columns,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] uint[] endLines,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] uint[] endColumns);
        void RemapToken(uint oldToken, uint newToken);
        void Initialize2([MarshalAs(UnmanagedType.IUnknown)]object emitter, string tempfilename, [MarshalAs(UnmanagedType.IUnknown)]object pIStream, bool fFullBuild, string finalfilename);
        void DefineConstant(string name, object value, uint cSig, IntPtr signature);
    }
    struct ImageDebugDirectory
    {
        internal int Characteristics;
        internal int TimeDateStamp;
        internal short MajorVersion;
        internal short MinorVersion;
        internal int Type;
        internal int SizeOfData;
        internal int AddressOfRawData;
        internal int PointerToRawData;
        public ImageDebugDirectory(bool zeroFill)
        {
            this.Characteristics = 0;
            this.TimeDateStamp = 0;
            this.MajorVersion = 0;
            this.MinorVersion = 0;
            this.Type = 0;
            this.SizeOfData = 0;
            this.AddressOfRawData = 0;
            this.PointerToRawData = 0;
        }
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("BA3FEE4C-ECB9-4e41-83B7-183FA41CD859")]
    unsafe public interface IMetaDataEmit
    {
        void SetModuleProps(string szName);
        void Save(string szFile, uint dwSaveFlags);
        void SaveToStream(void* pIStream, uint dwSaveFlags);
        uint GetSaveSize(uint fSave);
        uint DefineTypeDef(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements);
        uint DefineNestedType(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements, uint tdEncloser);
        void SetHandler([MarshalAs(UnmanagedType.IUnknown), In]object pUnk);
        uint DefineMethod(uint td, char* zName, uint dwMethodFlags, byte* pvSigBlob, uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags);
        void DefineMethodImpl(uint td, uint tkBody, uint tkDecl);
        uint DefineTypeRefByName(uint tkResolutionScope, char* szName);
        uint DefineImportType(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue, IMetaDataImport pImport,
          uint tdImport, IntPtr pAssemEmit);
        uint DefineMemberRef(uint tkImport, string szName, byte* pvSigBlob, uint cbSigBlob);
        uint DefineImportMember(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue,
          IMetaDataImport pImport, uint mbMember, IntPtr pAssemEmit, uint tkParent);
        uint DefineEvent(uint td, string szEvent, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods);
        void SetClassLayout(uint td, uint dwPackSize, COR_FIELD_OFFSET* rFieldOffsets, uint ulClassSize);
        void DeleteClassLayout(uint td);
        void SetFieldMarshal(uint tk, byte* pvNativeType, uint cbNativeType);
        void DeleteFieldMarshal(uint tk);
        uint DefinePermissionSet(uint tk, uint dwAction, void* pvPermission, uint cbPermission);
        void SetRVA(uint md, uint ulRVA);
        uint GetTokenFromSig(byte* pvSig, uint cbSig);
        uint DefineModuleRef(string szName);
        void SetParent(uint mr, uint tk);
        uint GetTokenFromTypeSpec(byte* pvSig, uint cbSig);
        void SaveToMemory(void* pbData, uint cbData);
        uint DefineUserString(string szString, uint cchString);
        void DeleteToken(uint tkObj);
        void SetMethodProps(uint md, uint dwMethodFlags, uint ulCodeRVA, uint dwImplFlags);
        void SetTypeDefProps(uint td, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements);
        void SetEventProps(uint ev, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods);
        uint SetPermissionSetProps(uint tk, uint dwAction, void* pvPermission, uint cbPermission);
        void DefinePinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL);
        void SetPinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL);
        void DeletePinvokeMap(uint tk);
        uint DefineCustomAttribute(uint tkObj, uint tkType, void* pCustomAttribute, uint cbCustomAttribute);
        void SetCustomAttributeValue(uint pcv, void* pCustomAttribute, uint cbCustomAttribute);
        uint DefineField(uint td, string szName, uint dwFieldFlags, byte* pvSigBlob, uint cbSigBlob, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
        uint DefineProperty(uint td, string szProperty, uint dwPropFlags, byte* pvSig, uint cbSig, uint dwCPlusTypeFlag,
          void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods);
        uint DefineParam(uint md, uint ulParamSeq, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
        void SetFieldProps(uint fd, uint dwFieldFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
        void SetPropertyProps(uint pr, uint dwPropFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods);
        void SetParamProps(uint pd, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
        uint DefineSecurityAttributeSet(uint tkObj, IntPtr rSecAttrs, uint cSecAttrs);
        void ApplyEditAndContinue([MarshalAs(UnmanagedType.IUnknown)]object pImport);
        uint TranslateSigWithScope(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue,
          IMetaDataImport import, byte* pbSigBlob, uint cbSigBlob, IntPtr pAssemEmit, IMetaDataEmit emit, byte* pvTranslatedSig, uint cbTranslatedSigMax);
        void SetMethodImplFlags(uint md, uint dwImplFlags);
        void SetFieldRVA(uint fd, uint ulRVA);
        void Merge(IMetaDataImport pImport, IntPtr pHostMapToken, [MarshalAs(UnmanagedType.IUnknown)]object pHandler);
        void MergeEnd();
    }
    public struct COR_FIELD_OFFSET
    {
        public uint ridOfField;
        public uint ulOffset;
    }
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
    unsafe public interface IMetaDataImport
    {
        [PreserveSig]
        void CloseEnum(uint hEnum);
        uint CountEnum(uint hEnum);
        void ResetEnum(uint hEnum, uint ulPos);
        uint EnumTypeDefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rTypeDefs, uint cMax);
        uint EnumInterfaceImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] rImpls, uint cMax);
        uint EnumTypeRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rTypeRefs, uint cMax);
        uint FindTypeDefByName(string szTypeDef, uint tkEnclosingClass);
        Guid GetScopeProps(StringBuilder szName, uint cchName, out uint pchName);
        uint GetModuleFromScope();
        uint GetTypeDefProps(uint td, IntPtr szTypeDef, uint cchTypeDef, out uint pchTypeDef, IntPtr pdwTypeDefFlags);
        uint GetInterfaceImplProps(uint iiImpl, out uint pClass);
        uint GetTypeRefProps(uint tr, out uint ptkResolutionScope, StringBuilder szName, uint cchName);
        uint ResolveTypeRef(uint tr, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppIScope);
        uint EnumMembers(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] rMembers, uint cMax);
        uint EnumMembersWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rMembers, uint cMax);
        uint EnumMethods(ref uint phEnum, uint cl, uint* rMethods, uint cMax);
        uint EnumMethodsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rMethods, uint cMax);
        uint EnumFields(ref uint phEnum, uint cl, uint* rFields, uint cMax);
        uint EnumFieldsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rFields, uint cMax);
        uint EnumParams(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] rParams, uint cMax);
        uint EnumMemberRefs(ref uint phEnum, uint tkParent, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] rMemberRefs, uint cMax);
        uint EnumMethodImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rMethodBody,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rMethodDecl, uint cMax);
        uint EnumPermissionSets(ref uint phEnum, uint tk, uint dwActions, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rPermission,
          uint cMax);
        uint FindMember(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pvSigBlob, uint cbSigBlob);
        uint FindMethod(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pvSigBlob, uint cbSigBlob);
        uint FindField(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pvSigBlob, uint cbSigBlob);
        uint FindMemberRef(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pvSigBlob, uint cbSigBlob);
        uint GetMethodProps(uint mb, out uint pClass, IntPtr szMethod, uint cchMethod, out uint pchMethod, IntPtr pdwAttr,
          IntPtr ppvSigBlob, IntPtr pcbSigBlob, IntPtr pulCodeRVA);
        unsafe uint GetMemberRefProps(uint mr, ref uint ptk, StringBuilder szMember, uint cchMember, out uint pchMember, out byte* ppvSigBlob);
        uint EnumProperties(ref uint phEnum, uint td, uint* rProperties, uint cMax);
        uint EnumEvents(ref uint phEnum, uint td, uint* rEvents, uint cMax);
        uint GetEventProps(uint ev, out uint pClass, StringBuilder szEvent, uint cchEvent, out uint pchEvent, out uint pdwEventFlags,
          out uint ptkEventType, out uint pmdAddOn, out uint pmdRemoveOn, out uint pmdFire,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 11)] uint[] rmdOtherMethod, uint cMax);
        uint EnumMethodSemantics(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] rEventProp, uint cMax);
        uint GetMethodSemantics(uint mb, uint tkEventProp);
        uint GetClassLayout(uint td, out uint pdwPackSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] COR_FIELD_OFFSET[] rFieldOffset, uint cMax, out uint pcFieldOffset);
        unsafe uint GetFieldMarshal(uint tk, out byte* ppvNativeType);
        uint GetRVA(uint tk, out uint pulCodeRVA);
        unsafe uint GetPermissionSetProps(uint pm, out uint pdwAction, out void* ppvPermission);
        unsafe uint GetSigFromToken(uint mdSig, out byte* ppvSig);
        uint GetModuleRefProps(uint mur, StringBuilder szName, uint cchName);
        uint EnumModuleRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rModuleRefs, uint cmax);
        unsafe uint GetTypeSpecFromToken(uint typespec, out byte* ppvSig);
        uint GetNameFromToken(uint tk);
        uint EnumUnresolvedMethods(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rMethods, uint cMax);
        uint GetUserString(uint stk, StringBuilder szString, uint cchString);
        uint GetPinvokeMap(uint tk, out uint pdwMappingFlags, StringBuilder szImportName, uint cchImportName, out uint pchImportName);
        uint EnumSignatures(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rSignatures, uint cmax);
        uint EnumTypeSpecs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rTypeSpecs, uint cmax);
        uint EnumUserStrings(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rStrings, uint cmax);
        [PreserveSig]
        int GetParamForMethodIndex(uint md, uint ulParamSeq, out uint pParam);
        uint EnumCustomAttributes(ref uint phEnum, uint tk, uint tkType, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rCustomAttributes, uint cMax);
        uint GetCustomAttributeProps(uint cv, out uint ptkObj, out uint ptkType, out void* ppBlob);
        uint FindTypeRef(uint tkResolutionScope, string szName);
        uint GetMemberProps(uint mb, out uint pClass, StringBuilder szMember, uint cchMember, out uint pchMember, out uint pdwAttr,
          out byte* ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags, out uint pdwCPlusTypeFlag, out void* ppValue);
        uint GetFieldProps(uint mb, out uint pClass, StringBuilder szField, uint cchField, out uint pchField, out uint pdwAttr,
          out byte* ppvSigBlob, out uint pcbSigBlob, out uint pdwCPlusTypeFlag, out void* ppValue);
        uint GetPropertyProps(uint prop, out uint pClass, StringBuilder szProperty, uint cchProperty, out uint pchProperty, out uint pdwPropFlags,
          out byte* ppvSig, out uint pbSig, out uint pdwCPlusTypeFlag, out void* ppDefaultValue, out uint pcchDefaultValue, out uint pmdSetter,
          out uint pmdGetter, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 14)] uint[] rmdOtherMethod, uint cMax);
        uint GetParamProps(uint tk, out uint pmd, out uint pulSequence, StringBuilder szName, uint cchName, out uint pchName,
          out uint pdwAttr, out uint pdwCPlusTypeFlag, out void* ppValue);
        uint GetCustomAttributeByName(uint tkObj, string szName, out void* ppData);
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsValidToken(uint tk);
        uint GetNestedClassProps(uint tdNestedClass);
        uint GetNativeCallConvFromSig(void* pvSig, uint cbSig);
        int IsGlobal(uint pd);
    }
    [SuppressUnmanagedCodeSecurity]
    internal sealed class Ir2md : IMetaDataEmit, IMetaDataImport
    {
#else
  internal sealed class Ir2md{
#endif
        private AssemblyNode assembly;
        private Module/*!*/ module;
        private MetadataWriter/*!*/ writer;
        private bool UseGenerics = false;
        private bool StripOptionalModifiersFromLocals
        {
            get { return this.module.StripOptionalModifiersFromLocals; }
        }
        private BinaryWriter/*!*/ blobHeap = new BinaryWriter(new MemoryStream(), System.Text.Encoding.Unicode);
#if WHIDBEYwithGenerics || WHIDBEYwithGenericsAndIEqualityComparer
        private Hashtable/*!*/ blobHeapIndex = new Hashtable(new ByteArrayKeyComparer());
#else
    private Hashtable/*!*/ blobHeapIndex = new Hashtable(new ByteArrayHasher(), new ByteArrayComparer());
#endif
        private Hashtable/*!*/ blobHeapStringIndex = new Hashtable();
        private NodeList/*!*/ nodesWithCustomAttributes = new NodeList();
        private int customAttributeCount = 0;
        private NodeList/*!*/ nodesWithSecurityAttributes = new NodeList();
        private int securityAttributeCount = 0;
        private NodeList/*!*/ constantTableEntries = new NodeList();
        private TrivialHashtable/*!*/ assemblyRefIndex = new TrivialHashtable();
        private AssemblyReferenceList/*!*/ assemblyRefEntries = new AssemblyReferenceList();
        private TypeNodeList/*!*/ classLayoutEntries = new TypeNodeList();
        private TrivialHashtable/*!*/ documentMap = new TrivialHashtable();
        private TrivialHashtable/*!*/ eventIndex = new TrivialHashtable();
        private EventList/*!*/ eventEntries = new EventList();
        private TrivialHashtable/*!*/ eventMapIndex = new TrivialHashtable();
        private EventList/*!*/ eventMapEntries = new EventList();
        private TrivialHashtable/*!*/ exceptionBlock = new TrivialHashtable();
        private TrivialHashtable/*!*/ fieldIndex = new TrivialHashtable();
        private FieldList/*!*/ fieldEntries = new FieldList();
        private FieldList/*!*/ fieldLayoutEntries = new FieldList();
        private FieldList/*!*/ fieldRvaEntries = new FieldList();
        private Hashtable/*!*/ fileTableIndex = new Hashtable();
        private ModuleList/*!*/ fileTableEntries = new ModuleList();
        private Hashtable/*!*/ genericParamIndex = new Hashtable();
        private MemberList/*!*/ genericParamEntries = new MemberList();
        private TypeNodeList/*!*/ genericParameters = new TypeNodeList();
        private TypeNodeList/*!*/ genericParamConstraintEntries = new TypeNodeList();
        private ArrayList/*!*/ guidEntries = new ArrayList();
        private Hashtable/*!*/ guidIndex = new Hashtable();
        private MethodList/*!*/ implMapEntries = new MethodList();
        private TypeNodeList/*!*/ interfaceEntries = new TypeNodeList();
        private NodeList/*!*/ marshalEntries = new NodeList();
        private TrivialHashtable/*!*/ memberRefIndex = new TrivialHashtable();
        private MemberList/*!*/ memberRefEntries = new MemberList();
        private TrivialHashtable/*!*/ methodBodiesHeapIndex = new TrivialHashtable();
        private BinaryWriter/*!*/ methodBodiesHeap = new BinaryWriter(new MemoryStream());
        private BinaryWriter/*!*/ methodBodyHeap;
        private MethodList/*!*/ methodEntries = new MethodList();
        private TrivialHashtable/*!*/ methodIndex = new TrivialHashtable();
        private MethodList/*!*/ methodImplEntries = new MethodList();
        private MethodInfo/*!*/ methodInfo;
        private Method currentMethod;
        private MemberList/*!*/ methodSemanticsEntries = new MemberList();
        private MethodList/*!*/ methodSpecEntries = new MethodList();
        private Hashtable/*!*/ methodSpecIndex = new Hashtable();
        private ModuleReferenceList/*!*/ moduleRefEntries = new ModuleReferenceList();
        private Hashtable/*!*/ moduleRefIndex = new Hashtable();
        private TypeNodeList/*!*/ nestedClassEntries = new TypeNodeList();
        private TrivialHashtable/*!*/ paramIndex = new TrivialHashtable();
        private ParameterList/*!*/ paramEntries = new ParameterList();
        private TrivialHashtable/*!*/ propertyIndex = new TrivialHashtable();
        private PropertyList/*!*/ propertyEntries = new PropertyList();
        private TrivialHashtable/*!*/ propertyMapIndex = new TrivialHashtable();
        private PropertyList/*!*/ propertyMapEntries = new PropertyList();
        private BinaryWriter/*!*/ resourceDataHeap = new BinaryWriter(new MemoryStream());
        private BinaryWriter/*!*/ sdataHeap = new BinaryWriter(new MemoryStream());
#if !ROTOR
        private ISymUnmanagedWriter symWriter = null;
#endif
        private int stackHeight;
        private int stackHeightMax;
        private int stackHeightExitTotal;
        private ArrayList/*!*/ standAloneSignatureEntries = new ArrayList();
        private BinaryWriter/*!*/ stringHeap = new BinaryWriter(new MemoryStream());
        private Hashtable/*!*/ stringHeapIndex = new Hashtable();
        private BinaryWriter/*!*/ tlsHeap = new BinaryWriter(new MemoryStream());
        private TrivialHashtable/*!*/ typeDefIndex = new TrivialHashtable();
        private TypeNodeList/*!*/ typeDefEntries = new TypeNodeList();
        private TrivialHashtable/*!*/ typeRefIndex = new TrivialHashtable();
        private TypeNodeList/*!*/ typeRefEntries = new TypeNodeList();
        private TrivialHashtable/*!*/ typeSpecIndex = new TrivialHashtable();
        private TypeNodeList/*!*/ typeSpecEntries = new TypeNodeList();
        private TrivialHashtable/*!*/ typeParameterNumber = new TrivialHashtable();
        private BinaryWriter/*!*/ userStringHeap = new BinaryWriter(new MemoryStream(), System.Text.Encoding.Unicode);
        private Hashtable/*!*/ userStringHeapIndex = new Hashtable();
        private byte[] PublicKey;

        internal Ir2md(Module/*!*/ module)
        {
            this.assembly = module as AssemblyNode;
            this.module = module;
            //^ base();
            this.blobHeap.Write((byte)0);
            this.stringHeap.Write((byte)0);
            this.userStringHeap.Write((byte)0);
            if (this.assembly != null)
                this.PublicKey = this.assembly.PublicKeyOrToken;
        }
        internal static void WritePE(Module/*!*/ module, string debugSymbolsLocation, BinaryWriter/*!*/ writer)
        {
            Ir2md ir2md = new Ir2md(module);
            try
            {
                ir2md.SetupMetadataWriter(debugSymbolsLocation);
                MetadataWriter mdWriter = ir2md.writer;
                mdWriter.WritePE(writer);
            }
            finally
            {
#if !ROTOR
                if (ir2md.symWriter != null)
                    ir2md.symWriter.Close();
#endif
                ir2md.assembly = null;
                ir2md.assemblyRefEntries = null;
                ir2md.assemblyRefIndex = null;
                ir2md.blobHeap = null;
                ir2md.blobHeapIndex = null;
                ir2md.blobHeapStringIndex = null;
                ir2md.classLayoutEntries = null;
                ir2md.constantTableEntries = null;
                ir2md.documentMap = null;
                ir2md.eventEntries = null;
                ir2md.eventIndex = null;
                ir2md.eventMapEntries = null;
                ir2md.eventMapIndex = null;
                ir2md.exceptionBlock = null;
                ir2md.fieldEntries = null;
                ir2md.fieldIndex = null;
                ir2md.fieldLayoutEntries = null;
                ir2md.fieldRvaEntries = null;
                ir2md.fileTableEntries = null;
                ir2md.fileTableIndex = null;
                ir2md.genericParamConstraintEntries = null;
                ir2md.genericParamEntries = null;
                ir2md.genericParameters = null;
                ir2md.genericParamIndex = null;
                ir2md.guidEntries = null;
                ir2md.guidIndex = null;
                ir2md.implMapEntries = null;
                ir2md.interfaceEntries = null;
                ir2md.marshalEntries = null;
                ir2md.memberRefEntries = null;
                ir2md.memberRefIndex = null;
                ir2md.methodBodiesHeap = null;
                ir2md.methodBodiesHeapIndex = null;
                ir2md.methodBodyHeap = null;
                ir2md.methodEntries = null;
                ir2md.methodImplEntries = null;
                ir2md.methodIndex = null;
                ir2md.methodInfo = null;
                ir2md.currentMethod = null;
                ir2md.methodSemanticsEntries = null;
                ir2md.methodSpecEntries = null;
                ir2md.methodSpecIndex = null;
                ir2md.module = null;
                ir2md.moduleRefEntries = null;
                ir2md.moduleRefIndex = null;
                ir2md.nestedClassEntries = null;
                ir2md.nodesWithCustomAttributes = null;
                ir2md.nodesWithSecurityAttributes = null;
                ir2md.paramEntries = null;
                ir2md.paramIndex = null;
                ir2md.propertyEntries = null;
                ir2md.propertyIndex = null;
                ir2md.propertyMapEntries = null;
                ir2md.propertyMapIndex = null;
                ir2md.PublicKey = null;
                ir2md.resourceDataHeap = null;
                ir2md.sdataHeap = null;
                ir2md.standAloneSignatureEntries = null;
                ir2md.stringHeap = null;
                ir2md.stringHeapIndex = null;
#if !ROTOR
                ir2md.symWriter = null;
#endif
                ir2md.tlsHeap = null;
                ir2md.typeDefEntries = null;
                ir2md.typeDefIndex = null;
                ir2md.typeParameterNumber = null;
                ir2md.typeRefEntries = null;
                ir2md.typeRefIndex = null;
                ir2md.typeSpecEntries = null;
                ir2md.typeSpecIndex = null;
                ir2md.unspecializedFieldFor = null;
                ir2md.unspecializedMethodFor = null;
                ir2md.userStringHeap = null;
                ir2md.userStringHeapIndex = null;
                ir2md.writer = null;
                ir2md = null;
            }
        }

        private static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
        private static Guid IID_IClassFactory = new Guid("00000001-0000-0000-C000-000000000046");

        [ComImport(), Guid("00000001-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        interface IClassFactory
        {
            int CreateInstance(
                   [In, MarshalAs(UnmanagedType.Interface)] object unused,
                   [In] ref Guid refiid,
                   [MarshalAs(UnmanagedType.Interface)] out Object ppunk);
            int LockServer(
                   int fLock);
        }

        delegate int GetClassObjectDelegate([In] ref Guid refclsid,
                                            [In] ref Guid refiid,
                                            [MarshalAs(UnmanagedType.Interface)] out IClassFactory ppUnk);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern int LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern GetClassObjectDelegate GetProcAddress(int hModule, string lpProcName);

        private object CrossCompileActivate(string server, Guid guid)
        {
            // Poor man's version of Activator.CreateInstance or CoCreate
            object o = null;
            int hmod = LoadLibrary(server);
            if (hmod != 0)
            {
                GetClassObjectDelegate del = GetProcAddress(hmod, "DllGetClassObject");
                if (del != null)
                {
                    IClassFactory icf;
                    int hr = del(ref guid, ref IID_IClassFactory, out icf);
                    if (hr == 0 && icf != null)
                    {
                        object temp = null;
                        hr = icf.CreateInstance(null, ref IID_IUnknown, out temp);
                        if (hr == 0) o = temp;
                    }
                }
            }
            return o;
        }
        private void SetupMetadataWriter(string debugSymbolsLocation)
        {
            Version v = TargetPlatform.TargetVersion;
            this.UseGenerics = TargetPlatform.UseGenerics;
#if !ROTOR
            if (debugSymbolsLocation != null)
            {
                // If targeting RTM (Version.Major = 1 and Version.Minor = 0)
                // then use Symwriter.pdb as ProgID else use CorSymWriter_SxS
                // (Note that RTM version 1.0.3705 has Assembly version 1.0.3300,
                // hence the <= 3705 expression.  This also leaves room for RTM SP releases
                // with slightly different build numbers).
                Type t = null;
                if (v.Major == 1 && v.Minor == 0 && v.Build <= 3705)
                {
                    try
                    {
                        t = Type.GetTypeFromProgID("Symwriter.pdb", false);
                        this.symWriter = (ISymUnmanagedWriter)Activator.CreateInstance(t);
                        if (this.symWriter != null)
                            this.symWriter.Initialize(this, debugSymbolsLocation, null, true);
                    }
                    catch (Exception)
                    {
                        t = null;
                        this.symWriter = null;
                    }
                }
                if (t == null)
                {
                    Debug.Assert(this.symWriter == null);
                    t = Type.GetTypeFromProgID("CorSymWriter_SxS", false);
                    if (t != null)
                    {
                        Guid guid = t.GUID;

                        // If the compiler was built with Whidbey, then mscoree will pick a matching
                        // diasymreader.dll out of the Whidbey directory.  But if we are cross-
                        // compiling, this is *NOT* what we want.  Instead, we want to override
                        // the shim's logic and explicitly pick a diasymreader.dll from the place
                        // that matches the version of the output file we are emitting.  This is
                        // strictly illegal by the CLR's rules.  However, the CLR does not yet
                        // support cross-compilation, so we have little choice.
                        if (!UseGenerics)
                        {
                            Version vcompiler = typeof(object).Assembly.GetName().Version;
                            if (vcompiler.Major >= 2)
                            {
                                // This is the only cross-compilation case we currently support.
                                string server = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location),
                                                                                 "..\\v1.1.4322\\diasymreader.dll");
                                object o = CrossCompileActivate(server, guid);
                                this.symWriter = (ISymUnmanagedWriter)o;
                            }
                        }
                        if (this.symWriter == null)
                        {
                            this.symWriter = (ISymUnmanagedWriter)Activator.CreateInstance(t);
                        }
                        if (this.symWriter != null)
                            this.symWriter.Initialize(this, debugSymbolsLocation, null, true);
                    }
                    else
                    {
                        throw new DebugSymbolsCouldNotBeWrittenException();
                    }
                }
            }
#endif
            //Visit the module, building lists etc.
            this.VisitModule(this.module);
            //Use the lists to populate the tables in the metadata writer
#if !ROTOR
            MetadataWriter writer = this.writer = new MetadataWriter(this.symWriter);
#else
      MetadataWriter writer = this.writer = new MetadataWriter();
#endif
            writer.UseGenerics = this.UseGenerics;
            if (module.EntryPoint != null)
            {
                writer.entryPointToken = this.GetMethodToken(module.EntryPoint);
#if !ROTOR
                if (this.symWriter != null) this.symWriter.SetUserEntryPoint((uint)writer.entryPointToken);
#endif
            }
            writer.moduleKind = module.Kind;
            writer.peKind = module.PEKind;
            writer.TrackDebugData = module.TrackDebugData;
            writer.fileAlignment = module.FileAlignment;
            if (writer.fileAlignment < 512) writer.fileAlignment = 512;
            writer.PublicKey = this.PublicKey;
            if (this.assembly != null) this.PopulateAssemblyTable();
            this.PopulateClassLayoutTable();
            this.PopulateConstantTable();
            this.PopulateGenericParamTable(); //Needs to happen before PopulateCustomAttributeTable since it the latter refers to indices in the sorted table
            this.PopulateCustomAttributeTable();
            this.PopulateDeclSecurityTable();
            this.PopulateEventMapTable();
            this.PopulateEventTable();
            this.PopulateExportedTypeTable();
            this.PopulateFieldTable();
            this.PopulateFieldLayoutTable();
            this.PopulateFieldRVATable();
            this.PopulateManifestResourceTable(); //This needs to happen before PopulateFileTable because resources are not visited separately
            this.PopulateFileTable();
            this.PopulateGenericParamConstraintTable();
            this.PopulateImplMapTable();
            this.PopulateInterfaceImplTable();
            this.PopulateMarshalTable();
            this.PopulateMethodTable();
            this.PopulateMethodImplTable();
            this.PopulateMemberRefTable();
            this.PopulateMethodSemanticsTable();
            this.PopulateMethodSpecTable();
            this.PopulateModuleTable();
            this.PopulateModuleRefTable();
            this.PopulateNestedClassTable();
            this.PopulateParamTable();
            this.PopulatePropertyTable();
            this.PopulatePropertyMapTable();
            this.PopulateStandAloneSigTable();
            this.PopulateTypeDefTable();
            this.PopulateTypeRefTable();
            this.PopulateTypeSpecTable();
            this.PopulateGuidTable();
            this.PopulateAssemblyRefTable();
            this.writer.BlobHeap = (MemoryStream)this.blobHeap.BaseStream; //this.blobHeap = null;
            this.writer.SdataHeap = (MemoryStream)this.sdataHeap.BaseStream; //this.sdataHeap = null;
            this.writer.TlsHeap = (MemoryStream)this.tlsHeap.BaseStream; //this.tlsHeap = null;
            this.writer.StringHeap = (MemoryStream)this.stringHeap.BaseStream; //this.stringHeap = null;
            this.writer.UserstringHeap = (MemoryStream)this.userStringHeap.BaseStream; //this.userStringHeap = null;
            this.writer.MethodBodiesHeap = (MemoryStream)this.methodBodiesHeap.BaseStream; //this.methodBodiesHeap = null;
            this.writer.ResourceDataHeap = (MemoryStream)this.resourceDataHeap.BaseStream; //this.resourceDataHeap = null;
            this.writer.Win32Resources = this.module.Win32Resources;
        }
        int GetAssemblyRefIndex(AssemblyNode/*!*/ assembly)
        {
            if (assembly.Location == "unknown:location")
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                  ExceptionStrings.UnresolvedAssemblyReferenceNotAllowed, assembly.Name));
            Object index = this.assemblyRefIndex[assembly.UniqueKey];
            if (index == null)
            {
                index = this.assemblyRefEntries.Count + 1;
                AssemblyReference aref = new AssemblyReference(assembly);
                if (this.module.UsePublicKeyTokensForAssemblyReferences)
                {
                    aref.PublicKeyOrToken = aref.PublicKeyToken;
                    aref.HashValue = null;
                    aref.Flags = aref.Flags & ~AssemblyFlags.PublicKey;
                }
                this.assemblyRefEntries.Add(aref);
                this.assemblyRefIndex[assembly.UniqueKey] = index;
            }
            return (int)index;
        }
        int GetBlobIndex(ExpressionList expressions, ParameterList parameters)
        {
            MemoryStream sig = new MemoryStream();
            BinaryWriter signature = new BinaryWriter(sig);
            this.WriteCustomAttributeSignature(expressions, parameters, false, signature);
            byte[] sigBytes = sig.ToArray();
            int length = sigBytes.Length;
            int index = (int)this.blobHeap.BaseStream.Position;
            Ir2md.WriteCompressedInt(this.blobHeap, length);
            this.blobHeap.BaseStream.Write(sigBytes, 0, length);
            return index;
        }
        void WriteCustomAttributeSignature(ExpressionList expressions, ParameterList parameters, bool onlyWriteNamedArguments, BinaryWriter signature)
        {
            int n = parameters == null ? 0 : parameters.Count;
            int m = expressions == null ? 0 : expressions.Count;
            Debug.Assert(m >= n);
            int numNamed = m > n ? m - n : 0;
            if (onlyWriteNamedArguments)
            {
                Ir2md.WriteCompressedInt(signature, numNamed);
            }
            else
            {
                signature.Write((short)1);
                if (parameters != null && expressions != null)
                {
                    for (int i = 0; i < n; i++)
                    {
                        Parameter p = parameters[i];
                        Expression e = expressions[i];
                        if (p == null || e == null) continue;
                        Literal l = e as Literal;
                        if (l == null) { Debug.Assert(false); continue; }
                        this.WriteCustomAttributeLiteral(signature, l, p.Type == CoreSystemTypes.Object);
                    }
                }
                signature.Write((short)numNamed);
            }
            if (expressions != null)
            {
                for (int i = n; i < m; i++)
                {
                    Expression e = expressions[i];
                    NamedArgument narg = e as NamedArgument;
                    if (narg == null) { Debug.Assert(false); continue; }
                    signature.Write((byte)(narg.IsCustomAttributeProperty ? 0x54 : 0x53));
                    if (narg.ValueIsBoxed)
                        signature.Write((byte)ElementType.BoxedEnum);
                    else if (narg.Value.Type is EnumNode)
                    {
                        signature.Write((byte)ElementType.Enum);
                        this.WriteSerializedTypeName(signature, narg.Value.Type);
                    }
                    else if (narg.Value.Type == CoreSystemTypes.Type)
                        signature.Write((byte)ElementType.Type);
                    else if (narg.Value.Type is ArrayType)
                    {
                        ArrayType arrT = (ArrayType)narg.Value.Type;
                        if (arrT.ElementType == CoreSystemTypes.Type)
                        {
                            signature.Write((byte)ElementType.SzArray);
                            signature.Write((byte)ElementType.Type);
                        }
                        else
                            this.WriteTypeSignature(signature, narg.Value.Type);
                    }
                    else
                        this.WriteTypeSignature(signature, narg.Value.Type);
                    signature.Write(narg.Name.Name, false);
                    this.WriteCustomAttributeLiteral(signature, (Literal)narg.Value, narg.ValueIsBoxed);
                }
            }
        }
        int GetBlobIndex(byte[]/*!*/ blob)
        {
            object indexOb = this.blobHeapIndex[blob];
            if (indexOb != null) return (int)indexOb;
            int index = (int)this.blobHeap.BaseStream.Position;
            int length = blob.Length;
            Ir2md.WriteCompressedInt(this.blobHeap, length);
            this.blobHeap.BaseStream.Write(blob, 0, length);
            this.blobHeapIndex[blob] = index;
            return index;
        }
        int GetBlobIndex(string/*!*/ str)
        {
            object indexOb = this.blobHeapStringIndex[str];
            if (indexOb != null) return (int)indexOb;
            int index = (int)this.blobHeap.BaseStream.Position;
            this.blobHeap.Write((string)str);
            this.blobHeapStringIndex[str] = index;
            return index;
        }
        int GetBlobIndex(Field/*!*/ field)
        {
            if (field != null && field.DeclaringType != null && field.DeclaringType.Template != null && field.DeclaringType.Template.IsGeneric)
                field = this.GetUnspecializedField(field);
            MemoryStream sig = new MemoryStream();
            BinaryWriter signature = new BinaryWriter(sig);
            signature.Write((byte)0x6);
            TypeNode fieldType = field.Type;
#if ExtendedRuntime
      if (field.HasOutOfBandContract) fieldType = TypeNode.DeepStripModifiers(fieldType, null, SystemTypes.NonNullType);
#endif
            if (fieldType == null) { Debug.Fail(""); fieldType = SystemTypes.Object; }
            this.WriteTypeSignature(signature, fieldType, true);
            return this.GetBlobIndex(sig.ToArray());
        }
        int GetBlobIndex(MarshallingInformation/*!*/ marshallingInformation)
        {
            MemoryStream sig = new MemoryStream();
            BinaryWriter signature = new BinaryWriter(sig);
            signature.Write((byte)marshallingInformation.NativeType);
            switch (marshallingInformation.NativeType)
            {
                case NativeType.SafeArray:
                    signature.Write((byte)marshallingInformation.ElementType);
                    if (marshallingInformation.Class != null && marshallingInformation.Class.Length > 0)
                        signature.Write(marshallingInformation.Class, false);
                    break;
                case NativeType.LPArray:
                    signature.Write((byte)marshallingInformation.ElementType);
                    if (marshallingInformation.ParamIndex >= 0 || marshallingInformation.ElementSize > 0)
                    {
                        if (marshallingInformation.ParamIndex < 0)
                        {
                            Debug.Fail("MarshallingInformation.ElementSize > 0 should imply that ParamIndex >= 0");
                            marshallingInformation.ParamIndex = 0;
                        }
                        Ir2md.WriteCompressedInt(signature, marshallingInformation.ParamIndex);
                    }
                    if (marshallingInformation.ElementSize > 0)
                    {
                        Ir2md.WriteCompressedInt(signature, marshallingInformation.ElementSize);
                        if (marshallingInformation.NumberOfElements > 0)
                            Ir2md.WriteCompressedInt(signature, marshallingInformation.NumberOfElements);
                    }
                    break;
                case NativeType.ByValArray:
                    Ir2md.WriteCompressedInt(signature, marshallingInformation.Size);
                    if (marshallingInformation.ElementType != NativeType.NotSpecified)
                        signature.Write((byte)marshallingInformation.ElementType);
                    break;
                case NativeType.ByValTStr:
                    Ir2md.WriteCompressedInt(signature, marshallingInformation.Size);
                    break;
                case NativeType.CustomMarshaler:
                    signature.Write((short)0);
                    signature.Write(marshallingInformation.Class);
                    signature.Write(marshallingInformation.Cookie);
                    break;
            }
            return this.GetBlobIndex(sig.ToArray());
        }
        int GetBlobIndex(Literal/*!*/ literal)
        {
            int index = (int)this.blobHeap.BaseStream.Position;
            TypeNode lType = literal.Type;
            EnumNode eType = lType as EnumNode;
            if (eType != null) lType = eType.UnderlyingType;
            IConvertible ic = literal.Value as IConvertible;
            if (ic == null) ic = "";
            switch (lType.typeCode)
            {
                case ElementType.Boolean: this.blobHeap.Write((byte)1); this.blobHeap.Write(ic.ToBoolean(null)); break;
                case ElementType.Char: this.blobHeap.Write((byte)2); this.blobHeap.Write(ic.ToChar(null)); break;
                case ElementType.Int8: this.blobHeap.Write((byte)1); this.blobHeap.Write(ic.ToSByte(null)); break;
                case ElementType.UInt8: this.blobHeap.Write((byte)1); this.blobHeap.Write(ic.ToByte(null)); break;
                case ElementType.Int16: this.blobHeap.Write((byte)2); this.blobHeap.Write(ic.ToInt16(null)); break;
                case ElementType.UInt16: this.blobHeap.Write((byte)2); this.blobHeap.Write(ic.ToUInt16(null)); break;
                case ElementType.Int32: this.blobHeap.Write((byte)4); this.blobHeap.Write(ic.ToInt32(null)); break;
                case ElementType.UInt32: this.blobHeap.Write((byte)4); this.blobHeap.Write(ic.ToUInt32(null)); break;
                case ElementType.Int64: this.blobHeap.Write((byte)8); this.blobHeap.Write(ic.ToInt64(null)); break;
                case ElementType.UInt64: this.blobHeap.Write((byte)8); this.blobHeap.Write(ic.ToUInt64(null)); break;
                case ElementType.Single: this.blobHeap.Write((byte)4); this.blobHeap.Write(ic.ToSingle(null)); break;
                case ElementType.Double: this.blobHeap.Write((byte)8); this.blobHeap.Write(ic.ToDouble(null)); break;
                case ElementType.String: this.blobHeap.Write((string)literal.Value, false); break;
                case ElementType.Array:
                case ElementType.Class:
                case ElementType.Object:
                case ElementType.Reference:
                case ElementType.SzArray: this.blobHeap.Write((byte)4); this.blobHeap.Write((int)0); break; //REVIEW: standard implies this should be 0, peverify thinks otherwise.
                default: Debug.Assert(false, "Unexpected Literal type"); return 0;
            }
            return index;
        }
        int GetBlobIndex(FunctionPointer/*!*/ fp)
        {
            MemoryStream sig = new MemoryStream();
            BinaryWriter signature = new BinaryWriter(sig);
            this.WriteMethodSignature(signature, fp);
            return this.GetBlobIndex(sig.ToArray());
        }
        int GetBlobIndex(Method/*!*/ method, bool methodSpecSignature)
        {
            MemoryStream sig = new MemoryStream();
            BinaryWriter signature = new BinaryWriter(sig);
            if (methodSpecSignature)
                this.WriteMethodSpecSignature(signature, method);
            else
                this.WriteMethodSignature(signature, method);
            return this.GetBlobIndex(sig.ToArray());
        }
        int GetBlobIndex(AttributeList/*!*/ securityAttributes)
        {
            MemoryStream sig = new MemoryStream();
            BinaryWriter signature = new BinaryWriter(sig);
            signature.Write((byte)'.');
            Ir2md.WriteCompressedInt(signature, securityAttributes.Count);
            foreach (AttributeNode attr in securityAttributes)
                this.WriteSecurityAttribute(signature, attr);
            return this.GetBlobIndex(sig.ToArray());
        }
        private void WriteSecurityAttribute(BinaryWriter signature, AttributeNode attr)
        {
            this.WriteSerializedTypeName(signature, attr.Type);
            MemoryStream sig = new MemoryStream();
            BinaryWriter casig = new BinaryWriter(sig);
            MemberBinding mb = attr.Constructor as MemberBinding;
            if (mb == null) return;
            InstanceInitializer constructor = mb.BoundMember as InstanceInitializer;
            if (constructor == null) return;
            this.WriteCustomAttributeSignature(attr.Expressions, constructor.Parameters, true, casig);
            byte[] sigBytes = sig.ToArray();
            int length = sigBytes.Length;
            Ir2md.WriteCompressedInt(signature, length);
            signature.BaseStream.Write(sigBytes, 0, length);
        }
        int GetBlobIndex(Property/*!*/ prop)
        {
            MemoryStream sig = new MemoryStream();
            BinaryWriter signature = new BinaryWriter(sig);
            this.WritePropertySignature(signature, prop);
            return this.GetBlobIndex(sig.ToArray());
        }
        int GetBlobIndex(TypeNode/*!*/ type)
        {
            MemoryStream sig = new MemoryStream();
            BinaryWriter signature = new BinaryWriter(sig);
            this.WriteTypeSignature(signature, type, true);
            return this.GetBlobIndex(sig.ToArray());
        }
        int GetCustomAttributeParentCodedIndex(Node/*!*/ node)
        {
            switch (node.NodeType)
            {
                case NodeType.InstanceInitializer:
                case NodeType.StaticInitializer:
                case NodeType.Method: return this.GetMethodIndex((Method)node) << 5;
                case NodeType.Field: return (this.GetFieldIndex((Field)node) << 5) | 1;
                case NodeType.Parameter: return (this.GetParamIndex((Parameter)node) << 5) | 4;
                case NodeType.Class:
                case NodeType.DelegateNode:
                case NodeType.EnumNode:
                case NodeType.Interface:
                case NodeType.Struct:
#if !MinimalReader
                case NodeType.TupleType:
                case NodeType.TypeAlias:
                case NodeType.TypeIntersection:
                case NodeType.TypeUnion:
#endif
                    TypeNode t = (TypeNode)node;
                    if (this.IsStructural(t) && (!t.IsGeneric || (t.Template != null && t.ConsolidatedTemplateArguments != null && t.ConsolidatedTemplateArguments.Count > 0)))
                        return (this.GetTypeSpecIndex(t) << 5) | 13;
                    else
                        return (this.GetTypeDefIndex(t) << 5) | 3;
                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                    if (!this.UseGenerics) goto case NodeType.Class;
                    return (this.GetGenericParamIndex((TypeNode)node) << 5) | 19;
                case NodeType.Property: return (this.GetPropertyIndex((Property)node) << 5) | 9;
                case NodeType.Event: return (this.GetEventIndex((Event)node) << 5) | 10;
                case NodeType.Module: return (1 << 5) | 7;
                case NodeType.Assembly: return (1 << 5) | 14;
                default: Debug.Assert(false, "Unexpect custom attribute parent"); return 0;
            }
        }
#if !ROTOR
        ISymUnmanagedDocumentWriter GetDocumentWriter(Document/*!*/ doc)
        //^ requires this.symWriter != null;
        {
            int key = Identifier.For(doc.Name).UniqueIdKey;
            object writer = this.documentMap[key];
            if (writer == null)
            {
                writer = this.symWriter.DefineDocument(doc.Name, ref doc.Language, ref doc.LanguageVendor, ref doc.DocumentType);
                this.documentMap[key] = writer;
            }
            return (ISymUnmanagedDocumentWriter)writer;
        }
#endif
        int GetEventIndex(Event/*!*/ e)
        {
            return (int)this.eventIndex[e.UniqueKey];
        }
        int GetFieldIndex(Field/*!*/ f)
        {
            Object index = this.fieldIndex[f.UniqueKey];
            if (index == null)
            {
                if (this.fieldEntries == null) return 1;
                index = this.fieldEntries.Count + 1;
                this.fieldEntries.Add(f);
                this.fieldIndex[f.UniqueKey] = index;
                if (f.DefaultValue != null && !(f.DefaultValue.Value is Parameter))
                    this.constantTableEntries.Add(f);
                if (!f.IsStatic && f.DeclaringType != null && (f.DeclaringType.Flags & TypeFlags.ExplicitLayout) != 0)
                    this.fieldLayoutEntries.Add(f);
                if ((f.Flags & FieldFlags.HasFieldRVA) != 0)
                    this.fieldRvaEntries.Add(f);
                if (f.MarshallingInformation != null)
                    this.marshalEntries.Add(f);
            }
            return (int)index;
        }
        int GetGenericParamIndex(TypeNode/*!*/ gp)
        {
            return (int)this.genericParamIndex[gp.UniqueKey];
        }
        int GetFieldToken(Field/*!*/ f)
        {
            if (f.DeclaringType == null || (f.DeclaringType.DeclaringModule == this.module && !this.IsStructural(f.DeclaringType)))
                return 0x04000000 | this.GetFieldIndex(f);
            else
                return 0x0a000000 | this.GetMemberRefIndex(f);
        }
        bool IsStructural(TypeNode type)
        {
            if (type == null) return false;
            if (this.UseGenerics && (type.IsGeneric || type.Template != null && type.Template.IsGeneric)) return true;
            switch (type.NodeType)
            {
                case NodeType.ArrayType:
                case NodeType.Pointer:
                case NodeType.Reference:
                case NodeType.OptionalModifier:
                case NodeType.RequiredModifier:
                    return true;
                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                    return this.UseGenerics;
            }
            return false;
        }
        int GetFileTableIndex(Module/*!*/ module)
        {
            Object index = this.fileTableIndex[module];
            if (index == null)
            {
                index = this.fileTableEntries.Count + 1;
                this.fileTableEntries.Add(module);
                this.fileTableIndex[module] = index;
            }
            return (int)index;
        }
        int GetGuidIndex(Guid guid)
        {
            Object index = this.guidIndex[guid];
            if (index == null)
            {
                index = this.guidEntries.Count + 1;
                this.guidEntries.Add(guid);
                this.guidIndex[guid] = index;
            }
            return (int)index;
        }
        internal int GetLocalVarIndex(Local/*!*/ loc)
        {
#if !MinimalReader
            LocalBinding lb = loc as LocalBinding;
            if (lb != null) loc = lb.BoundLocal;
#endif
            if (this.StripOptionalModifiersFromLocals)
                loc.Type = TypeNode.StripModifiers(loc.Type);
            MethodInfo methInfo = this.methodInfo;

            if (methInfo.localVarSignature == null)
            {
                methInfo.localVarSignature = new BinaryWriter(new MemoryStream());
                methInfo.localVarSignature.Write((short)0);
                methInfo.localVarIndex = new TrivialHashtable();
                methInfo.localVarSigTok = 0x11000000 | this.GetStandAloneSignatureIndex(methInfo.localVarSignature);
            }
            object index = methInfo.localVarIndex[loc.UniqueKey];
            if (index == null)
            {
                methInfo.localVarIndex[loc.UniqueKey] = index = methInfo.localVarIndex.Count;
#if !ROTOR
                int startPosition = 0;
                if (this.symWriter != null && loc.Name != null && loc.Name.UniqueIdKey != Identifier.Empty.UniqueIdKey)
                {
                    methInfo.debugLocals.Add(loc);
                    methInfo.signatureOffsets.Add(startPosition = methInfo.localVarSignature.BaseStream.Position);
                    if (loc.Pinned) methInfo.localVarSignature.Write((byte)ElementType.Pinned);
                    this.WriteTypeSignature(methInfo.localVarSignature, loc.Type, true);
                    methInfo.signatureLengths.Add(methInfo.localVarSignature.BaseStream.Position - startPosition);
                }
                else
                {
#endif
                    if (loc.Pinned) methInfo.localVarSignature.Write((byte)ElementType.Pinned);
                    this.WriteTypeSignature(methInfo.localVarSignature, loc.Type, true);
#if !ROTOR
                }
#endif
            }
            return (int)index;
        }
        int GetMemberRefParentEncoded(TypeNode type)
        {
            if (type == null) return 0;
            if (this.IsStructural(type)) return (this.GetTypeSpecIndex(type) << 3) | 4;
            if (type.DeclaringModule == this.module) return this.GetTypeDefIndex(type) << 3;
            if (type.DeclaringModule != null) return (this.GetTypeRefIndex(type) << 3) | 1;
            if (type.typeCode == ElementType.Class || type.typeCode == ElementType.ValueType)
                return this.GetTypeDefIndex(type) << 3; //REVIEW: when does this happen?
            Debug.Assert(false);
            return 0;
        }
        int GetMemberRefIndex(Member/*!*/ m)
        {
            Object index = this.memberRefIndex[m.UniqueKey];
            if (index == null)
            {
                index = this.memberRefEntries.Count + 1;
                this.memberRefEntries.Add(m);
                this.memberRefIndex[m.UniqueKey] = index;
                TypeNode type = m.DeclaringType;
                this.VisitReferencedType(type);
            }
            return (int)index;
        }
        class VarargMethodCallSignature : FunctionPointer
        {
            internal Method method;
            internal VarargMethodCallSignature(Method/*!*/ method, TypeNodeList/*!*/ parameterTypes)
                : base(parameterTypes, method.ReturnType, method.Name)
            {
                this.method = method;
                this.DeclaringType = method.DeclaringType;
            }
        }
        int GetMemberRefToken(Method/*!*/ m, ExpressionList arguments)
        {
            int numArgs = arguments == null ? 0 : arguments.Count;
            TypeNodeList parTypes = new TypeNodeList(numArgs);
            int varArgStart = m.Parameters.Count;
            for (int i = 0; i < varArgStart; i++)
                parTypes.Add(m.Parameters[i].Type);
            for (int i = varArgStart; i < numArgs; i++)
            {
                //^ assert arguments != null;
                parTypes.Add(arguments[i].Type);
            }
            VarargMethodCallSignature sig = new VarargMethodCallSignature(m, parTypes);
            sig.VarArgStart = varArgStart;
            sig.CallingConvention = m.CallingConvention;
            return 0x0a000000 | this.GetMemberRefIndex(sig);
        }
        int GetMethodDefOrRefEncoded(Method/*!*/ m)
        {
            if (m.DeclaringType.DeclaringModule == this.module && !this.IsStructural(m.DeclaringType))
                return this.GetMethodIndex(m) << 1;
            else
                return (this.GetMemberRefIndex(m) << 1) | 0x1;
        }
        int GetMethodIndex(Method/*!*/ m)
        {
            Object index = this.methodIndex[m.UniqueKey];
            if (index == null)
            {
                if (this.methodEntries == null) return 1;
                index = this.methodEntries.Count + 1;
                this.methodEntries.Add(m);
                this.methodIndex[m.UniqueKey] = index;
                if (m.ReturnTypeMarshallingInformation != null || (m.ReturnAttributes != null && m.ReturnAttributes.Count > 0))
                {
                    Parameter p = new Parameter();
                    p.ParameterListIndex = -1;
                    p.Attributes = m.ReturnAttributes;
                    if (m.ReturnTypeMarshallingInformation != null)
                    {
                        p.MarshallingInformation = m.ReturnTypeMarshallingInformation;
                        p.Flags = ParameterFlags.HasFieldMarshal;
                        this.marshalEntries.Add(p);
                    }
                    this.paramEntries.Add(p);
                    this.paramIndex[m.UniqueKey] = this.paramEntries.Count;
                    this.paramIndex[p.UniqueKey] = this.paramEntries.Count;
                    this.VisitAttributeList(p.Attributes, p);
                }
                int offset = m.IsStatic ? 0 : 1;
                if (m.Parameters != null)
                {
                    for (int i = 0, n = m.Parameters.Count; i < n; i++)
                    {
                        Parameter p = m.Parameters[i];
                        if (p == null) continue;
                        if (p == null) continue;
                        if (p.DeclaringMethod == null) p.DeclaringMethod = m;
                        p.ParameterListIndex = i;
                        p.ArgumentListIndex = i + offset;
                        int j = this.paramEntries.Count + 1;
                        this.paramEntries.Add(p); //TODO: provide a way to suppress the param table entries unless param has custom attributes or flags
                        this.paramIndex[p.UniqueKey] = j;
                        if (p.DefaultValue != null)
                            this.constantTableEntries.Add(p);
                        if (p.MarshallingInformation != null)
                            this.marshalEntries.Add(p);
                    }
                }
                if (m.IsGeneric)
                    this.VisitGenericParameterList(m, m.TemplateParameters);
            }
            return (int)index;
        }
        int GetMethodSpecIndex(Method/*!*/ m)
        {
            int structuralKey = m.UniqueKey;
            int blobIndex = this.GetBlobIndex(m, true);
            if (m.Template != null)
                structuralKey = (m.Template.UniqueKey << 8) + blobIndex;
            else
                Debug.Assert(false);
            Object index = this.methodSpecIndex[m.UniqueKey];
            if (index == null)
            {
                index = this.methodSpecIndex[structuralKey];
                if (index is int)
                {
                    Method otherMethod = this.methodSpecEntries[((int)index) - 1];
                    if (otherMethod != null && otherMethod.Template == m.Template && blobIndex == this.GetBlobIndex(otherMethod, true))
                        return (int)index;
                }
                index = this.methodSpecEntries.Count + 1;
                this.methodSpecEntries.Add(m);
                this.methodSpecIndex[m.UniqueKey] = index;
                this.methodSpecIndex[structuralKey] = index;
                this.GetMemberRefIndex(m.Template);
                Method templ = m.Template;
                if (templ != null)
                {
                    while (templ.Template != null) templ = templ.Template;
                    TypeNodeList templParams = templ.TemplateParameters;
                    if (templParams != null)
                    {
                        for (int i = 0, n = templParams.Count; i < n; i++)
                        {
                            TypeNode templParam = templParams[i];
                            if (templParam == null) continue;
                            this.typeParameterNumber[templParam.UniqueKey] = -(i + 1);
                        }
                    }
                }
            }
            return (int)index;
        }
        int GetMethodToken(Method/*!*/ m)
        {
            if (this.UseGenerics && m.Template != null && m.Template.IsGeneric)
                return 0x2b000000 | this.GetMethodSpecIndex(m);
            else if (m.DeclaringType.DeclaringModule == this.module && !this.IsStructural(m.DeclaringType))
                return 0x06000000 | this.GetMethodIndex(m);
            else
                return 0x0a000000 | this.GetMemberRefIndex(m);
        }
        int GetMethodDefToken(Method/*!*/ m)
        {
            if (m.DeclaringType.DeclaringModule == this.module)
                return 0x06000000 | this.GetMethodIndex(m);
            else
                return 0x0a000000 | this.GetMemberRefIndex(m);
        }
        int GetMethodBodiesHeapIndex(Method/*!*/ m)
        {
            return (int)this.methodBodiesHeapIndex[m.UniqueKey];
        }
        int GetModuleRefIndex(Module/*!*/ module)
        {
            if (module.Location == "unknown:location") throw new InvalidOperationException(ExceptionStrings.UnresolvedModuleReferenceNotAllowed);
            Object index = this.moduleRefIndex[module.Name];
            if (index == null)
            {
                index = this.moduleRefEntries.Count + 1;
                this.moduleRefEntries.Add(new ModuleReference(module.Name, module));
                this.moduleRefIndex[module.Name] = index;
                if (module.HashValue != null && module.HashValue.Length > 0)
                    this.GetFileTableIndex(module);
            }
            return (int)index;
        }
        int GetOffset(Block target, int addressOfNextInstruction)
        {
            if (target == null) return 0;
            int fixupLocation = (int)(this.methodBodyHeap.BaseStream.Position);
            Object ob = this.methodInfo.fixupIndex[target.UniqueKey];
            if (ob is int) return ((int)ob) - addressOfNextInstruction;
            Fixup fixup = new Fixup();
            fixup.addressOfNextInstruction = addressOfNextInstruction;
            fixup.fixupLocation = fixupLocation;
            fixup.shortOffset = false;
            fixup.nextFixUp = (Fixup)ob;
            this.methodInfo.fixupIndex[target.UniqueKey] = fixup;
            return 0;
        }
        int GetOffset(Block target, ref bool shortOffset)
        {
            if (target == null) return 0;
            int fixupLocation = (int)(this.methodBodyHeap.BaseStream.Position + 1);
            Object ob = this.methodInfo.fixupIndex[target.UniqueKey];
            if (ob is int)
            {
                int targetAddress = (int)ob;
                int offset = targetAddress - (fixupLocation + 1);
                if (-128 > offset || offset > 127)
                {
                    offset = targetAddress - (fixupLocation + 4);
                    Debug.Assert(offset < -128, "Forward short branch out of range");
                    shortOffset = false;
                }
                else
                    shortOffset = true;
                return offset;
            }
            Fixup fixup = new Fixup();
            fixup.fixupLocation = fixup.addressOfNextInstruction = fixupLocation;
            if (shortOffset) fixup.addressOfNextInstruction += 1; else fixup.addressOfNextInstruction += 4;
            fixup.shortOffset = shortOffset;
            fixup.nextFixUp = (Fixup)ob;
            this.methodInfo.fixupIndex[target.UniqueKey] = fixup;
            return 0;
        }
        int GetParamIndex(Parameter p)
        {
            if (p == null) return 0;
#if !MinimalReader
            ParameterBinding pb = p as ParameterBinding;
            if (pb != null) p = pb.BoundParameter;
#endif
            return (int)this.paramIndex[p.UniqueKey];
        }
        int GetPropertyIndex(Property/*!*/ p)
        {
            return (int)this.propertyIndex[p.UniqueKey];
        }
        int GetSecurityAttributeParentCodedIndex(Node/*!*/ node)
        {
            switch (node.NodeType)
            {
                case NodeType.InstanceInitializer:
                case NodeType.StaticInitializer:
                case NodeType.Method: return (this.GetMethodIndex((Method)node) << 2) | 1;
                case NodeType.Class:
                case NodeType.Interface:
                case NodeType.DelegateNode:
                case NodeType.EnumNode:
                case NodeType.Struct: return (this.GetTypeDefIndex((TypeNode)node) << 2) | 0;
                case NodeType.Assembly: return (1 << 2) | 2;
                default: Debug.Assert(false, "Unexpected security attribute parent"); return 0;
            }
        }
        int GetStandAloneSignatureIndex(BinaryWriter signatureWriter)
        {
            this.standAloneSignatureEntries.Add(signatureWriter);
            return this.standAloneSignatureEntries.Count;
        }
        int GetStaticDataIndex(byte[] data, PESection targetSection)
        {
            int result = 0;
            switch (targetSection)
            {
                case PESection.SData:
                    result = (int)this.sdataHeap.BaseStream.Position;
                    this.sdataHeap.Write(data);
                    break;
                case PESection.Text:
                    result = (int)this.methodBodiesHeap.BaseStream.Position;
                    this.methodBodiesHeap.Write(data);
                    break;
                case PESection.TLS:
                    result = (int)this.tlsHeap.BaseStream.Position;
                    this.tlsHeap.Write(data);
                    break;
            }
            return result;
        }
        int GetResourceDataIndex(byte[]/*!*/ data)
        {
            int index = (int)this.resourceDataHeap.BaseStream.Position;
            this.resourceDataHeap.Write((int)data.Length);
            this.resourceDataHeap.Write(data);
            return index;
        }
        int GetStringIndex(string str)
        {
            if (str == null || str.Length == 0) return 0;
            Object index = this.stringHeapIndex[str];
            if (index == null)
            {
                index = (int)this.stringHeap.BaseStream.Position;
                this.stringHeap.Write(str, true);
                this.stringHeapIndex[str] = index;
            }
            return (int)index;
        }
        int GetUserStringIndex(string/*!*/ str)
        {
            Object index = this.userStringHeapIndex[str];
            if (index == null)
            {
                index = (int)this.userStringHeap.BaseStream.Position;
                Ir2md.WriteCompressedInt(this.userStringHeap, str.Length * 2 + 1);
                this.userStringHeap.Write(str.ToCharArray());
                this.userStringHeapIndex[str] = index;
                //Write out a trailing byte indicating if the string is really quite simple
                ulong stringKind = 0; //no funny business
                foreach (char ch in str)
                {
                    if (ch >= 0x7F) stringKind += 1;
                    else
                        switch ((int)ch)
                        {
                            case 0x1:
                            case 0x2:
                            case 0x3:
                            case 0x4:
                            case 0x5:
                            case 0x6:
                            case 0x7:
                            case 0x8:
                            case 0xE:
                            case 0xF:
                            case 0x10:
                            case 0x11:
                            case 0x12:
                            case 0x13:
                            case 0x14:
                            case 0x15:
                            case 0x16:
                            case 0x17:
                            case 0x18:
                            case 0x19:
                            case 0x1A:
                            case 0x1B:
                            case 0x1C:
                            case 0x1D:
                            case 0x1E:
                            case 0x1F:
                            case 0x27:
                            case 0x2D:
                                stringKind += 1;
                                break;
                            default:
                                break;
                        }
                }
                if (stringKind > 0) stringKind = 1;
                this.userStringHeap.Write((byte)stringKind);
            }
            return (int)index;
        }
        int GetTypeDefIndex(TypeNode/*!*/ type)
        {
            Object index = this.typeDefIndex[type.UniqueKey];
            if (index == null)
            {
                if (this.typeDefEntries == null) return 0;
                index = this.typeDefEntries.Count + 1;
                this.typeDefEntries.Add(type);
                this.typeDefIndex[type.UniqueKey] = index;
                if (type.IsGeneric && type.Template == null)
                    this.VisitGenericParameterList(type, type.ConsolidatedTemplateParameters);
            }
            return (int)index;
        }
        int GetTypeDefOrRefOrSpecEncoded(TypeNode type)
        {
            if (type == null) return 0;
            if (!this.UseGenerics)
            {
                ClassParameter cp = type as ClassParameter;
                if (cp != null) { Debug.Assert(!cp.IsGeneric); return this.GetTypeDefOrRefOrSpecEncoded(cp.BaseClass); } //REVIEW: why???
            }
            if (this.IsStructural(type)) return (this.GetTypeSpecIndex(type) << 2) | 2;
            if (type.DeclaringModule == this.module) return this.GetTypeDefIndex(type) << 2;
            return (this.GetTypeRefIndex(type) << 2) | 1;
        }
        int GetTypeToken(TypeNode/*!*/ type)
        {
            if (this.IsStructural(type) && (!type.IsGeneric || (type.ConsolidatedTemplateArguments != null && type.ConsolidatedTemplateArguments.Count > 0)))
                return 0x1b000000 | this.GetTypeSpecIndex(type);
            if (type.IsGeneric)
            {
                TypeNode foundType = type.GetTemplateInstance(type, type.TemplateParameters);
                Debug.Assert(foundType != type);
                return this.GetTypeToken(foundType);
            }
            if (type.DeclaringModule == this.module)
                return 0x02000000 | this.GetTypeDefIndex(type);
            else if (type.DeclaringModule != null)
                return 0x01000000 | this.GetTypeRefIndex(type);
            else if (type.typeCode == ElementType.ValueType || type.typeCode == ElementType.Class)
            {
                type.DeclaringModule = this.module;
                return 0x02000000 | this.GetTypeDefIndex(type);
            }
            Debug.Assert(false);
            return 0;
        }
        int GetTypeDefToken(TypeNode/*!*/ type)
        {
            if (this.IsStructural(type) && (!type.IsGeneric || (type.Template != null && type.ConsolidatedTemplateArguments != null && type.ConsolidatedTemplateArguments.Count > 0)))
                return 0x1b000000 | this.GetTypeSpecIndex(type);
            if (type.DeclaringModule == this.module)
                return 0x02000000 | this.GetTypeDefIndex(type);
            else if (type.DeclaringModule != null)
                return 0x01000000 | this.GetTypeRefIndex(type);
            else if (type.typeCode == ElementType.ValueType || type.typeCode == ElementType.Class)
            {
                type.DeclaringModule = this.module;
                return 0x02000000 | this.GetTypeDefIndex(type);
            }
            Debug.Assert(false);
            return 0;
        }
        int GetTypeRefIndex(TypeNode/*!*/ type)
        {
            Object index = this.typeRefIndex[type.UniqueKey];
            if (index == null)
            {
                index = this.typeRefEntries.Count + 1;
                this.typeRefEntries.Add(type);
                this.typeRefIndex[type.UniqueKey] = index;
                Module module = type.DeclaringModule;
                AssemblyNode assembly = module as AssemblyNode;
                if (assembly != null)
                    this.GetAssemblyRefIndex(assembly);
                else
                    this.GetModuleRefIndex(module);
                if (type.DeclaringType != null)
                    this.GetTypeRefIndex(type.DeclaringType);
            }
            return (int)index;
        }
        int GetTypeSpecIndex(TypeNode/*!*/ type)
        {
            int structuralKey = type.UniqueKey;
            int blobIndex = 0;
            if (type.Template != null)
            {
                blobIndex = this.GetBlobIndex(type);
                structuralKey = ((type.Template.UniqueKey << 8) & int.MaxValue) + blobIndex;
            }
            Object index = this.typeSpecIndex[type.UniqueKey];
            if (index == null)
            {
                if (type.Template != null)
                {
                    index = this.typeSpecIndex[structuralKey];
                    if (index is int)
                    {
                        TypeNode otherType = this.typeSpecEntries[((int)index) - 1];
                        if (otherType != null && otherType.Template == type.Template && blobIndex == this.GetBlobIndex(otherType))
                            return (int)index;
                    }
                }
                index = this.typeSpecEntries.Count + 1;
                this.typeSpecEntries.Add(type);
                this.typeSpecIndex[type.UniqueKey] = index;
                if (type.Template != null)
                    this.typeSpecIndex[structuralKey] = index;
                if (type.Template != null)
                {
                    if (type.Template.DeclaringModule != this.module)
                        this.GetTypeRefIndex(type.Template);
                    TypeNodeList templArgs = type.ConsolidatedTemplateArguments;
                    for (int i = 0, n = templArgs == null ? 0 : templArgs.Count; i < n; i++)
                    {
                        this.VisitReferencedType(templArgs[i]);
                    }
                }
                else
                {
                    TypeNodeList telems = type.StructuralElementTypes;
                    for (int i = 0, n = telems == null ? 0 : telems.Count; i < n; i++)
                        this.VisitReferencedType(telems[i]);
                }
            }
            return (int)index;
        }
        TrivialHashtable/*!*/ unspecializedFieldFor = new TrivialHashtable();
        Field/*!*/ GetUnspecializedField(Field/*!*/ field)
        {
            if (field == null || field.DeclaringType == null || !field.DeclaringType.IsGeneric) { Debug.Fail(""); return field; }
            Field unspecializedField = (Field)this.unspecializedFieldFor[field.UniqueKey];
            if (unspecializedField != null) return unspecializedField;
            TypeNode template = field.DeclaringType;
            if (template == null) { Debug.Assert(false); return field; }
            while (template.Template != null) template = template.Template;
            MemberList specializedMembers = field.DeclaringType.Members;
            MemberList unspecializedMembers = template.Members;
            for (int i = 0, n = specializedMembers.Count; i < n; i++)
            {
                if (specializedMembers[i] != field) continue;
                unspecializedField = (Field)unspecializedMembers[i];
                if (unspecializedField == null) { Debug.Fail(""); unspecializedField = field; }
                this.unspecializedFieldFor[field.UniqueKey] = unspecializedField;
                this.VisitReferencedType(unspecializedField.DeclaringType);
                return unspecializedField;
            }
            Debug.Fail("");
            return field;
        }
        TrivialHashtable/*!*/ unspecializedMethodFor = new TrivialHashtable();
        Method/*!*/ GetUnspecializedMethod(Method/*!*/ method)
        {
            Debug.Assert(method != null && method.DeclaringType != null && method.DeclaringType.IsGeneric);
            Method unspecializedMethod = (Method)this.unspecializedMethodFor[method.UniqueKey];
            if (unspecializedMethod != null) return unspecializedMethod;
            TypeNode template = method.DeclaringType;
            if (template == null) { Debug.Assert(false); return method; }
            while (template.Template != null) template = template.Template;
            MemberList specializedMembers = method.DeclaringType.Members;
            MemberList unspecializedMembers = template.Members;
            for (int i = 0, n = specializedMembers.Count; i < n; i++)
            {
                if (specializedMembers[i] != method) continue;
                unspecializedMethod = unspecializedMembers[i] as Method;
                if (unspecializedMethod == null) break;
                this.unspecializedMethodFor[method.UniqueKey] = unspecializedMethod;
                template = unspecializedMethod.DeclaringType;
                while (template.Template != null) template = template.Template;
                this.VisitReferencedType(template);
                for (int j = 0, m = unspecializedMethod.TemplateParameters == null ? 0 : unspecializedMethod.TemplateParameters.Count; j < m; j++)
                {
                    TypeNode p = unspecializedMethod.TemplateParameters[j];
                    if (p == null) continue;
                    this.typeParameterNumber[p.UniqueKey] = -(j + 1);
                }
                return unspecializedMethod;
            }
            Debug.Assert(false);
            return method;
        }
        internal void IncrementStackHeight()
        {
            this.stackHeight++;
            if (this.stackHeight > this.stackHeightMax) this.stackHeightMax = this.stackHeight;
        }
        void PopulateAssemblyTable()
        //^ requires this.assembly != null;
        {
            AssemblyNode assembly = this.assembly;
            AssemblyRow[] assemblyTable = this.writer.assemblyTable = new AssemblyRow[1];
            assemblyTable[0].HashAlgId = (int)AssemblyHashAlgorithm.SHA1;
            assemblyTable[0].Flags = (int)assembly.Flags;
            if (assembly.Version == null) assembly.Version = new Version(1, 0, 0, 0);
            assemblyTable[0].MajorVersion = assembly.Version.Major;
            assemblyTable[0].MinorVersion = assembly.Version.Minor;
            assemblyTable[0].RevisionNumber = assembly.Version.Revision;
            assemblyTable[0].BuildNumber = assembly.Version.Build;
            if (assembly.PublicKeyOrToken != null && 0 < assembly.PublicKeyOrToken.Length)
                assemblyTable[0].PublicKey = this.GetBlobIndex(assembly.PublicKeyOrToken);
            if (assembly.Name != null)
                assemblyTable[0].Name = this.GetStringIndex(assembly.Name);
            else
                Debug.Assert(false, "Assembly must have a name");
            if (assembly.Culture != null && assembly.Culture.Length > 0)
                assemblyTable[0].Culture = this.GetStringIndex(assembly.Culture);
            this.writer.assemblyTable = assemblyTable;
        }
        void PopulateAssemblyRefTable()
        {
            AssemblyReferenceList arList = this.module.AssemblyReferences = this.assemblyRefEntries;
            if (arList == null) return;
            int n = arList.Count;
            AssemblyRefRow[] arRows = this.writer.assemblyRefTable = new AssemblyRefRow[n];
            for (int i = 0; i < n; i++)
            {
                AssemblyReference ar = arList[i];
                if (ar.Version == null)
                    Debug.Assert(false, "assembly reference without a version");
                else
                {
                    arRows[i].MajorVersion = ar.Version.Major;
                    arRows[i].MinorVersion = ar.Version.Minor;
                    arRows[i].RevisionNumber = ar.Version.Revision;
                    arRows[i].BuildNumber = ar.Version.Build;
                    arRows[i].Flags = (int)ar.Flags;
                }
                if (ar.PublicKeyOrToken != null && 0 < ar.PublicKeyOrToken.Length)
                    arRows[i].PublicKeyOrToken = this.GetBlobIndex(ar.PublicKeyOrToken);
                if (ar.Name == null)
                    Debug.Assert(false, "assembly reference without a name");
                else
                    arRows[i].Name = this.GetStringIndex(ar.Name);
                if (ar.Culture != null && ar.Culture.Length > 0)
                    arRows[i].Culture = this.GetStringIndex(ar.Culture);
                if (ar.HashValue != null)
                    arRows[i].HashValue = this.GetBlobIndex(ar.HashValue);
            }
            //this.assemblyRefEntries = null;
        }
        void PopulateClassLayoutTable()
        {
            int n = this.classLayoutEntries.Count;
            if (n == 0) return;
            ClassLayoutRow[] clr = this.writer.classLayoutTable = new ClassLayoutRow[n];
            for (int i = 0; i < n; i++)
            {
                TypeNode t = this.classLayoutEntries[i];
                clr[i].ClassSize = t.ClassSize;
                clr[i].PackingSize = t.PackingSize;
                clr[i].Parent = this.GetTypeDefIndex(t);
            }
            //this.classLayoutEntries = null;
        }
        void PopulateConstantTable()
        {
            int n = this.constantTableEntries.Count;
            if (n == 0) return;
            ConstantRow[] cr = this.writer.constantTable = new ConstantRow[n];
            for (int i = 0; i < n; i++)
            {
                Parameter p = this.constantTableEntries[i] as Parameter;
                if (p != null)
                {
                    cr[i].Parent = (this.GetParamIndex(p) << 2) | 1;
                    cr[i].Value = this.GetBlobIndex((Literal)p.DefaultValue);
                    TypeNode t = p.DefaultValue.Type;
                    if (t.NodeType == NodeType.EnumNode) t = ((EnumNode)t).UnderlyingType;
                    cr[i].Type = (int)t.typeCode;
                    if (t is Reference || (t != p.Type && Literal.IsNullLiteral(p.DefaultValue)))
                        cr[i].Type = (int)ElementType.Class;
                }
                else
                {
                    Field f = (Field)this.constantTableEntries[i];
                    cr[i].Parent = (this.GetFieldIndex(f) << 2);
                    cr[i].Value = this.GetBlobIndex(f.DefaultValue);
                    TypeNode t = f.DefaultValue.Type;
                    if (t.NodeType == NodeType.EnumNode) t = ((EnumNode)t).UnderlyingType;
                    cr[i].Type = (int)t.typeCode;
                    if (t is Reference || (t != f.Type && Literal.IsNullLiteral(f.DefaultValue)))
                        cr[i].Type = (int)ElementType.Class;
                }
                ConstantRow temp = cr[i];
                int parent = temp.Parent;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (cr[j].Parent > parent)
                    {
                        cr[j + 1] = cr[j];
                        if (j == 0)
                        {
                            cr[0] = temp;
                            break;
                        }
                    }
                    else
                    {
                        if (j < i - 1) cr[j + 1] = temp;
                        break;
                    }
                }
            }
            //TODO: more efficient sort
            //this.constantTableEntries = null;
        }
        void PopulateCustomAttributeTable()
        {
            if (this.customAttributeCount == 0) return;
            CustomAttributeRow[] table = this.writer.customAttributeTable = new CustomAttributeRow[this.customAttributeCount];
            int k = 0;
            int prevCodedIndex = 0;
            for (int i = 0, n = this.nodesWithCustomAttributes.Count; i < n; i++)
            {
                AttributeList attrs = null;
                Node node = this.nodesWithCustomAttributes[i];
                int codedIndex = 0;
                switch (node.NodeType)
                {
                    case NodeType.Method:
                    case NodeType.InstanceInitializer:
                    case NodeType.StaticInitializer:
                        Method m = (Method)node;
                        codedIndex = this.GetMethodIndex(m) << 5;
                        attrs = m.Attributes;
                        break;
                    case NodeType.Field:
                        Field f = (Field)node;
                        codedIndex = (this.GetFieldIndex(f) << 5) | 1;
                        attrs = f.Attributes;
                        break;
                    case NodeType.Parameter:
                        Parameter par = (Parameter)node;
                        codedIndex = (this.GetParamIndex(par) << 5) | 4;
                        attrs = par.Attributes;
                        break;
                    case NodeType.Class:
                    case NodeType.DelegateNode:
                    case NodeType.EnumNode:
                    case NodeType.Interface:
                    case NodeType.Struct:
#if !MinimalReader
                    case NodeType.TupleType:
                    case NodeType.TypeAlias:
                    case NodeType.TypeIntersection:
                    case NodeType.TypeUnion:
#endif
                        TypeNode t = (TypeNode)node;
                        if (this.IsStructural(t) && (!t.IsGeneric || (t.Template != null && t.ConsolidatedTemplateArguments != null && t.ConsolidatedTemplateArguments.Count > 0)))
                            codedIndex = (this.GetTypeSpecIndex(t) << 5) | 13;
                        else
                            codedIndex = (this.GetTypeDefIndex(t) << 5) | 3;
                        attrs = t.Attributes;
                        break;
                    case NodeType.ClassParameter:
                    case NodeType.TypeParameter:
                        if (!this.UseGenerics) goto case NodeType.Class;
                        t = (TypeNode)node;
                        codedIndex = (this.GetGenericParamIndex(t) << 5) | 19;
                        attrs = t.Attributes;
                        break;
                    case NodeType.Property:
                        Property p = (Property)node;
                        codedIndex = (this.GetPropertyIndex(p) << 5) | 9;
                        attrs = p.Attributes;
                        break;
                    case NodeType.Event:
                        Event e = (Event)node;
                        codedIndex = (this.GetEventIndex(e) << 5) | 10;
                        attrs = e.Attributes;
                        break;
                    case NodeType.Module:
                    case NodeType.Assembly:
                        codedIndex = (1 << 5) | (node.NodeType == NodeType.Module ? 7 : 14);
                        attrs = ((Module)node).Attributes;
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
                if (attrs == null) continue;
                if (UseGenerics)
                {
                    Debug.Assert(codedIndex > prevCodedIndex);
                }
                prevCodedIndex = codedIndex;
                for (int j = 0, m = attrs.Count; j < m; j++)
                {
                    AttributeNode a = attrs[j];
                    if (a == null) continue;
                    table[k].Parent = codedIndex;
                    Debug.Assert(a.Constructor is MemberBinding);
                    Method cons = (Method)((MemberBinding)a.Constructor).BoundMember;
                    if (cons.DeclaringType.DeclaringModule == this.module && !this.IsStructural(cons.DeclaringType))
                        table[k].Constructor = (this.GetMethodIndex(cons) << 3) | 2;
                    else
                        table[k].Constructor = (this.GetMemberRefIndex(cons) << 3) | 3;
                    table[k].Value = this.GetBlobIndex(a.Expressions, cons.Parameters);
                    k++;
                }
            }
        }
        void PopulateDeclSecurityTable()
        {
            if (this.securityAttributeCount == 0) return;
            DeclSecurityRow[] table = this.writer.declSecurityTable = new DeclSecurityRow[this.securityAttributeCount];
            int k = 0;
            int prevCodedIndex = 0;
            for (int i = 0, n = this.nodesWithSecurityAttributes.Count; i < n; i++)
            {
                SecurityAttributeList attrs = null;
                Node node = this.nodesWithSecurityAttributes[i];
                int codedIndex = 0;
                switch (node.NodeType)
                {
                    case NodeType.Method:
                    case NodeType.InstanceInitializer:
                    case NodeType.StaticInitializer:
                        Method m = (Method)node;
                        codedIndex = (this.GetMethodIndex(m) << 2) | 1;
                        attrs = m.SecurityAttributes;
                        break;
                    case NodeType.Class:
                    case NodeType.Interface:
                    case NodeType.DelegateNode:
                    case NodeType.EnumNode:
                    case NodeType.Struct:
                        TypeNode t = (TypeNode)node;
                        codedIndex = (this.GetTypeDefIndex(t) << 2) | 0;
                        attrs = t.SecurityAttributes;
                        break;
                    case NodeType.Assembly:
                        codedIndex = (1 << 2) | 2;
                        attrs = ((AssemblyNode)node).SecurityAttributes;
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
                if (attrs == null) continue;
                Debug.Assert(codedIndex > prevCodedIndex);
                prevCodedIndex = codedIndex;
                for (int j = 0, m = attrs.Count; j < m; j++)
                {
                    SecurityAttribute a = attrs[j];
                    if (a == null) continue;
                    this.VisitReferencedType(CoreSystemTypes.SecurityAction);
                    table[k].Action = (int)a.Action;
                    table[k].Parent = codedIndex;
                    if (CoreSystemTypes.SystemAssembly.MetadataFormatMajorVersion == 1 && CoreSystemTypes.SystemAssembly.MetadataFormatMinorVersion < 1)
                        table[k].PermissionSet = this.GetBlobIndex(a.SerializedPermissions);
                    else
                    {
                        if (a.PermissionAttributes != null)
                        {
                            table[k].PermissionSet = this.GetBlobIndex(a.PermissionAttributes);
                        }
                        else
                        {
                            // Came across some assemblies that had a metadata version > 1.0, but still used
                            // serialized security attributes. So might as well try to see if this is the case
                            // if the PermissionAttributes are null.
                            table[k].PermissionSet = this.GetBlobIndex(a.SerializedPermissions);
                        }
                    }
                    k++;
                }
            }
        }
        void PopulateEventMapTable()
        {
            int n = this.eventMapEntries.Count;
            if (n == 0) return;
            EventMapRow[] emr = this.writer.eventMapTable = new EventMapRow[n];
            for (int i = 0; i < n; i++)
            {
                Event e = this.eventMapEntries[i];
                emr[i].Parent = this.GetTypeDefIndex(e.DeclaringType);
                emr[i].EventList = this.GetEventIndex(e);
            }
            //this.eventMapEntries = null;
        }
        void PopulateEventTable()
        {
            int n = this.eventEntries.Count;
            if (n == 0) return;
            EventRow[] er = this.writer.eventTable = new EventRow[n];
            for (int i = 0; i < n; i++)
            {
                Event e = this.eventEntries[i];
                if (e == null || e.Name == null) continue;
                er[i].Flags = (int)e.Flags;
                er[i].Name = this.GetStringIndex(e.Name.ToString());
                er[i].EventType = this.GetTypeDefOrRefOrSpecEncoded(e.HandlerType);
            }
            //this.eventEntries = null;
        }
        void PopulateExportedTypeTable()
        {
            if (this.assembly == null) return;
            TypeNodeList exportedTypes = this.assembly.ExportedTypes;
            int n = exportedTypes == null ? 0 : exportedTypes.Count;
            if (n == 0) return;
            ExportedTypeRow[] ett = this.writer.exportedTypeTable = new ExportedTypeRow[n];
            for (int i = 0; i < n; i++)
            {
                TypeNode et = exportedTypes[i];
                if (et == null || et.Namespace == null || et.Name == null) continue;
                ett[i].TypeDefId = 0;
                ett[i].TypeNamespace = this.GetStringIndex(et.Namespace.ToString());
                ett[i].TypeName = this.GetStringIndex(et.Name.ToString());
                ett[i].Flags = (int)(et.Flags & TypeFlags.VisibilityMask);
                if (et.DeclaringType != null)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (exportedTypes[j] == et.DeclaringType)
                        {
                            ett[i].Implementation = (j << 2) | 2;
                            break;
                        }
                    }
                }
                else if (et.DeclaringModule != this.module && et.DeclaringModule is AssemblyNode)
                {
                    ett[i].Implementation = (this.GetAssemblyRefIndex((AssemblyNode)et.DeclaringModule) << 2) | 1;
                    ett[i].Flags = (int)TypeFlags.Forwarder;
                }
                else
                    ett[i].Implementation = (this.GetFileTableIndex(et.DeclaringModule) << 2) | 0;
            }
        }
        void PopulateFieldTable()
        {
            int n = this.fieldEntries.Count;
            if (n == 0) return;
            FieldRow[] fr = this.writer.fieldTable = new FieldRow[n];
            for (int i = 0; i < n; i++)
            {
                Field f = this.fieldEntries[i];
                fr[i].Flags = (int)f.Flags;
                fr[i].Name = this.GetStringIndex(f.Name.Name); // we don't store prefixes in field names.
                fr[i].Signature = this.GetBlobIndex(f);
            }
            //this.fieldEntries = null;
        }
        void PopulateFieldLayoutTable()
        {
            int n = this.fieldLayoutEntries.Count;
            if (n == 0) return;
            FieldLayoutRow[] flr = this.writer.fieldLayoutTable = new FieldLayoutRow[n];
            for (int i = 0; i < n; i++)
            {
                Field f = this.fieldLayoutEntries[i];
                flr[i].Field = this.GetFieldIndex(f);
                flr[i].Offset = f.Offset;
            }
            //this.fieldLayoutEntries = null;
        }
        void PopulateFieldRVATable()
        {
            int n = this.fieldRvaEntries.Count;
            if (n == 0) return;
            FieldRvaRow[] frr = this.writer.fieldRvaTable = new FieldRvaRow[n];
            for (int i = 0; i < n; i++)
            {
                Field f = this.fieldRvaEntries[i];
                frr[i].Field = this.GetFieldIndex(f);
                if (f.InitialData != null)
                    frr[i].RVA = this.GetStaticDataIndex(f.InitialData, f.Section); //Fixed up to be an RVA inside MetadataWriter.
                else
                    frr[i].RVA = f.Offset;
                frr[i].TargetSection = f.Section;
            }
            //this.fieldRvaEntries = null;
        }
        void PopulateFileTable()
        {
            int n = this.fileTableEntries.Count;
            if (n == 0) return;
            bool readContents = false;
            FileRow[] ftr = this.writer.fileTable = new FileRow[n];
            for (int i = 0; i < n; i++)
            {
                Module module = this.fileTableEntries[i];
                switch (module.Kind)
                {
                    case ModuleKindFlags.ConsoleApplication:
                    case ModuleKindFlags.DynamicallyLinkedLibrary:
                    case ModuleKindFlags.WindowsApplication:
                        ftr[i].Flags = (int)FileFlags.ContainsMetaData;
                        break;
                    case ModuleKindFlags.ManifestResourceFile:
                        readContents = true;
                        ftr[i].Flags = (int)FileFlags.ContainsNoMetaData;
                        break;
                    case ModuleKindFlags.UnmanagedDynamicallyLinkedLibrary:
                        ftr[i].Flags = (int)FileFlags.ContainsNoMetaData;
                        break;
                }
                if (module.HashValue != null)
                    ftr[i].HashValue = this.GetBlobIndex(module.HashValue);
                else
                    ftr[i].HashValue = 0;
                ftr[i].Name = this.GetStringIndex(module.Name);
                if (readContents)
                {
                    try
                    {
                        FileStream fs = File.OpenRead(module.Location);
                        long size = fs.Length;
                        byte[] buffer = new byte[size];
                        fs.Read(buffer, 0, (int)size);
                        System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                        byte[] hash = sha1.ComputeHash(buffer);
                        ftr[i].HashValue = this.GetBlobIndex(hash);
                    }
                    catch { }
                }
            }
            //this.fileTableEntries = null;
        }
        void PopulateGuidTable()
        {
            int n = this.guidEntries.Count;
            Guid[] guids = this.writer.GuidHeap = new Guid[n];
            for (int i = 0; i < n; i++)
                guids[i] = (Guid)this.guidEntries[i];
            //this.guidEntries = null;
        }
        void PopulateGenericParamTable()
        {
            int n = this.genericParamEntries.Count;
            if (n == 0) return;
            GenericParamRow[] gpr = this.writer.genericParamTable = new GenericParamRow[n];
            Member lastMember = null;
            int number = 0;
            for (int i = 0; i < n; i++)
            {
                Member m = this.genericParamEntries[i];
                TypeNode paramType = this.genericParameters[i];
                if (paramType == null || paramType.Name == null) continue;
                Method meth = m as Method;
                TypeNode type = m as TypeNode;
                if (m != lastMember) number = 0;
                gpr[i].GenericParameter = paramType;
                gpr[i].Number = number++;
                if (type != null)
                {
                    gpr[i].Name = this.GetStringIndex(paramType.Name.ToString());
                    gpr[i].Owner = (this.GetTypeDefIndex(type) << 1) | 0;
                }
                else
                {
                    //^ assert meth != null;
                    gpr[i].Name = this.GetStringIndex(paramType.Name.ToString());
                    gpr[i].Owner = (this.GetMethodIndex(meth) << 1) | 1;
                }
                ITypeParameter tp = paramType as ITypeParameter;
                if (tp != null)
                    gpr[i].Flags = (int)tp.TypeParameterFlags;
                else
                {
                    Debug.Assert(false);
                    gpr[i].Flags = 0;
                }
                lastMember = m;
                GenericParamRow temp = gpr[i];
                int owner = temp.Owner;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (gpr[j].Owner > owner)
                    {
                        gpr[j + 1] = gpr[j];
                        if (j == 0)
                        {
                            gpr[0] = temp;
                            break;
                        }
                    }
                    else
                    {
                        if (j < i - 1) gpr[j + 1] = temp;
                        break;
                    }
                }
            }
            for (int i = 0; i < n; i++)
            {
                Member genPar = gpr[i].GenericParameter;
                if (genPar == null) continue;
                this.genericParamIndex[genPar.UniqueKey] = i + 1;
            }
            for (int i = 0; i < n; i++)
            {
                Member genPar = gpr[i].GenericParameter;
                if (genPar == null) continue;
                this.VisitAttributeList(genPar.Attributes, genPar);
            }
            //this.genericParamEntries = null;
            //this.genericParameters = null;
        }
        void PopulateGenericParamConstraintTable()
        {
            int n = this.genericParamConstraintEntries.Count;
            if (n == 0) return;
            GenericParamConstraintRow[] gpcr = this.writer.genericParamConstraintTable = new GenericParamConstraintRow[n];
            TypeNode lastParameter = null;
            int paramIndex = 0;
            int constraintIndex = 0;
            int indexOffset = 0;
            for (int i = 0; i < n; i++)
            {
                TypeNode t = this.genericParamConstraintEntries[i];
                if (t != lastParameter)
                {
                    paramIndex = this.GetGenericParamIndex(t);
                    constraintIndex = 0;
                    indexOffset = 0;
                }
                gpcr[i].Param = paramIndex;
                TypeNode constraint;
                if (constraintIndex == 0 && t.BaseType != null && t.BaseType != CoreSystemTypes.Object)
                {
                    constraint = t.BaseType; indexOffset = 1;
                }
                else
                    constraint = t.Interfaces[constraintIndex - indexOffset];
                gpcr[i].Constraint = this.GetTypeDefOrRefOrSpecEncoded(constraint);
                lastParameter = t;
                constraintIndex++;
                GenericParamConstraintRow temp = gpcr[i];
                int param = temp.Param;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (gpcr[j].Param > param)
                    {
                        gpcr[j + 1] = gpcr[j];
                        if (j == 0)
                        {
                            gpcr[0] = temp;
                            break;
                        }
                    }
                    else
                    {
                        if (j < i - 1) gpcr[j + 1] = temp;
                        break;
                    }
                }
            }
            //this.genericParamConstraintEntries = null;
        }
        void PopulateImplMapTable()
        {
            int n = this.implMapEntries.Count;
            if (n == 0) return;
            ImplMapRow[] imr = this.writer.implMapTable = new ImplMapRow[n];
            for (int i = 0; i < n; i++)
            {
                Method m = this.implMapEntries[i];
                imr[i].ImportName = this.GetStringIndex(m.PInvokeImportName);
                imr[i].ImportScope = this.GetModuleRefIndex(m.PInvokeModule);
                imr[i].MappingFlags = (int)m.PInvokeFlags;
                imr[i].MemberForwarded = (this.GetMethodIndex(m) << 1) | 1;
            }
            //this.implMapEntries = null;
        }
        void PopulateInterfaceImplTable()
        {
            int n = this.interfaceEntries.Count;
            if (n == 0) return;
            InterfaceImplRow[] iir = this.writer.interfaceImplTable = new InterfaceImplRow[n];
            TypeNode prevT = null;
            for (int i = 0, j = 0; i < n; i++)
            {
                TypeNode t = this.interfaceEntries[i];
                if (t == prevT)
                    j++;
                else
                {
                    j = 0;
                    prevT = t;
                }
                int ti = iir[i].Class = this.GetTypeDefIndex(t);
                Interface iface = null;
#if ExtendedRuntime
        if (t is ITypeParameter){
          int numIfaces = t.Interfaces == null ? 0 : t.Interfaces.Count;
          if (j == numIfaces)
            iface = SystemTypes.ITemplateParameter;
          else
            iface = t.Interfaces[j];
        }else
#endif
                iface = t.Interfaces[j];
                if (iface == null) { i--; continue; }
                int ii = iir[i].Interface = this.GetTypeDefOrRefOrSpecEncoded(iface);
                for (int k = 0; k < i; k++)
                { //REVIEW: is a more efficient sort worthwhile?
                    if (iir[k].Class > ti)
                    {
                        for (int kk = i; kk > k; kk--)
                        {
                            iir[kk].Class = iir[kk - 1].Class;
                            iir[kk].Interface = iir[kk - 1].Interface;
                        }
                        iir[k].Class = ti;
                        iir[k].Interface = ii;
                        break;
                    }
                }
            }
            //this.interfaceEntries = null;
        }
        void PopulateManifestResourceTable()
        {
            ResourceList resources = this.module.Resources;
            int n = resources == null ? 0 : resources.Count;
            if (n == 0) return;
            ManifestResourceRow[] mresources = this.writer.manifestResourceTable = new ManifestResourceRow[n];
            for (int i = 0; i < n; i++)
            {
                Resource r = resources[i];
                mresources[i].Flags = r.IsPublic ? 1 : 2;
                mresources[i].Name = this.GetStringIndex(r.Name);
                if (r.Data != null)
                    mresources[i].Offset = this.GetResourceDataIndex(r.Data);
                else if (r.DefiningModule is AssemblyNode)
                    mresources[i].Implementation = (this.GetAssemblyRefIndex((AssemblyNode)r.DefiningModule) << 2) | 1;
                else
                    mresources[i].Implementation = (this.GetFileTableIndex(r.DefiningModule) << 2) | 0;
            }
        }
        void PopulateMarshalTable()
        {
            int n = this.marshalEntries.Count;
            if (n == 0) return;
            FieldMarshalRow[] fmr = this.writer.fieldMarshalTable = new FieldMarshalRow[n];
            for (int i = 0; i < n; i++)
            {
                MarshallingInformation mi;
                Field f = this.marshalEntries[i] as Field;
                if (f != null)
                {
                    fmr[i].Parent = (this.GetFieldIndex(f) << 1) | 0;
                    mi = f.MarshallingInformation;
                }
                else
                {
                    Parameter p = (Parameter)this.marshalEntries[i];
                    fmr[i].Parent = (this.GetParamIndex(p) << 1) | 1;
                    mi = p.MarshallingInformation;
                }
                int nt = fmr[i].NativeType = this.GetBlobIndex(mi);
                int pi = fmr[i].Parent;
                for (int k = 0; k < i; k++)
                { //REVIEW: is a more efficient sort worthwhile?
                    if (fmr[k].Parent > pi)
                    {
                        for (int kk = i; kk > k; kk--)
                        {
                            fmr[kk].Parent = fmr[kk - 1].Parent;
                            fmr[kk].NativeType = fmr[kk - 1].NativeType;
                        }
                        fmr[k].Parent = pi;
                        fmr[k].NativeType = nt;
                        break;
                    }
                }
            }
            //this.marshalEntries = null;
        }
        void PopulateMemberRefTable()
        {
            int n = this.memberRefEntries.Count;
            if (n == 0) return;
            MemberRefRow[] mr = this.writer.memberRefTable = new MemberRefRow[n];
            for (int i = 0; i < n; i++)
            {
                Member member = this.memberRefEntries[i];
                if (member == null || member.Name == null) continue;
                mr[i].Name = this.GetStringIndex(member.Name.ToString());
                Field f = member as Field;
                if (f != null)
                    mr[i].Signature = this.GetBlobIndex(f);
                else
                {
                    FunctionPointer fp = member as FunctionPointer;
                    if (fp != null)
                    {
                        mr[i].Signature = this.GetBlobIndex(fp);
                        if (fp is VarargMethodCallSignature)
                        {
                            Method m = ((VarargMethodCallSignature)member).method;
                            if (m != null && m.DeclaringType.DeclaringModule == this.module && !this.IsStructural(m.DeclaringType))
                            {
                                mr[i].Class = (this.GetMethodIndex(m) << 3) | 3;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        Method m = (Method)member;
                        if (m.IsGeneric && m.Template != null) m = this.GetUnspecializedMethod(m);
                        mr[i].Signature = this.GetBlobIndex(m, false);
                        if (m.DeclaringType.DeclaringModule == this.module && !this.IsStructural(m.DeclaringType) && !m.IsGeneric)
                        {
                            mr[i].Class = (this.GetMethodIndex(m) << 3) | 3;
                            continue;
                        }
                        //TODO: if the declaring type is the special global members type of another module, set class to a module ref
                    }
                }
                int j = mr[i].Class = this.GetMemberRefParentEncoded(member.DeclaringType);
                if ((j & 0x3) == 2) mr[i].Class = (j & ~0x3) | 4;
            }
            //this.memberRefEntries = null;
        }
        void PopulateMethodTable()
        {
            int n = this.methodEntries.Count;
            if (n == 0) return;
            MethodRow[] mr = this.writer.methodTable = new MethodRow[n];
            for (int i = 0; i < n; i++)
            {
                Method m = this.methodEntries[i];
                if (m == null || m.Name == null) continue;
                if (m.IsAbstract || m.Body == null || m.Body.Statements == null || m.Body.Statements.Count == 0)
                    mr[i].RVA = -1;
                else
                    mr[i].RVA = this.GetMethodBodiesHeapIndex(m); //Fixed up to be an RVA inside MetadataWriter.
                mr[i].Flags = (int)m.Flags;
                mr[i].ImplFlags = (int)m.ImplFlags;
                mr[i].Name = this.GetStringIndex(m.Name.ToString());
                mr[i].Signature = this.GetBlobIndex(m, false);
                if (m.ReturnTypeMarshallingInformation != null || (m.ReturnAttributes != null && m.ReturnAttributes.Count > 0))
                    mr[i].ParamList = (int)this.paramIndex[m.UniqueKey];
                else
                {
                    ParameterList pars = m.Parameters;
                    if (pars != null && pars.Count > 0)
                    {
                        Debug.Assert(pars[0] != null && pars[0].DeclaringMethod == m);
                        mr[i].ParamList = this.GetParamIndex(pars[0]);
                    }
                    else
                        mr[i].ParamList = 0;
                }
            }
            //this.methodEntries = null;
        }
        void PopulateMethodImplTable()
        {
            int n = this.methodImplEntries.Count;
            if (n == 0) return;
            MethodImplRow[] mir = this.writer.methodImplTable = new MethodImplRow[n];
            int j = 0;
            Method lastMethod = null;
            for (int i = 0; i < n; i++)
            {
                Method m = this.methodImplEntries[i];
                if (lastMethod != m) j = 0;
                mir[i].Class = this.GetTypeDefIndex(m.DeclaringType);
                if (m.DeclaringType.DeclaringModule == this.module)
                    mir[i].MethodBody = this.GetMethodIndex(m) << 1;
                else
                    mir[i].MethodBody = (this.GetMemberRefIndex(m) << 1) | 0x1;
                Method im = m.ImplementedInterfaceMethods[j++];
                while (im == null) im = m.ImplementedInterfaceMethods[j++];
                mir[i].MethodDeclaration = this.GetMethodDefOrRefEncoded(im);
                lastMethod = m;
            }
            //this.methodImplEntries = null;
        }
        void PopulateMethodSpecTable()
        {
            int n = this.methodSpecEntries.Count;
            if (n == 0) return;
            MethodSpecRow[] msr = this.writer.methodSpecTable = new MethodSpecRow[n];
            for (int i = 0; i < n; i++)
            {
                Method m = this.methodSpecEntries[i];
                msr[i].Method = this.GetMethodDefOrRefEncoded(m.Template);
                msr[i].Instantiation = this.GetBlobIndex(m, true);
                //TODO: sort this and eliminate duplicates.
                //Duplicates can arise when methods are instantiated with method parameters from different methods.
                //TODO: perhaps this duplication should be prevented by Method.GetTemplateInstance?
            }
            //this.methodEntries = null;
        }
        void PopulateMethodSemanticsTable()
        {
            int n = this.methodSemanticsEntries.Count;
            if (n == 0) return;
            MethodSemanticsRow[] msr = this.writer.methodSemanticsTable = new MethodSemanticsRow[n];
            Member previousOwner = null;
            int index = -1;
            for (int i = 0; i < n; i++)
            {
                Member owner = this.methodSemanticsEntries[i];
                Property ownerProperty = owner as Property;
                if (ownerProperty != null)
                {
                    msr[i].Association = (this.GetPropertyIndex(ownerProperty) << 1) | 1;
                    if (owner != previousOwner)
                    {
                        previousOwner = owner;
                        index = -1;
                        if (ownerProperty.Getter != null)
                        {
                            msr[i].Method = this.GetMethodIndex(ownerProperty.Getter);
                            msr[i].Semantics = 0x0002;
                            continue;
                        }
                    }
                    if (index == -1)
                    {
                        index = 0;
                        if (ownerProperty.Setter != null)
                        {
                            msr[i].Method = this.GetMethodIndex(ownerProperty.Setter);
                            msr[i].Semantics = 0x0001;
                            continue;
                        }
                    }
                    msr[i].Method = this.GetMethodIndex(ownerProperty.OtherMethods[index]);
                    msr[i].Semantics = 0x0004;
                    index++;
                    continue;
                }
                Event ownerEvent = owner as Event;
                if (ownerEvent == null) { Debug.Fail(""); continue; }
                msr[i].Association = this.GetEventIndex(ownerEvent) << 1;
                if (owner != previousOwner)
                {
                    previousOwner = owner;
                    index = -2;
                    if (ownerEvent.HandlerAdder != null)
                    {
                        msr[i].Method = this.GetMethodIndex(ownerEvent.HandlerAdder);
                        msr[i].Semantics = 0x0008;
                        continue;
                    }
                }
                if (index == -2)
                {
                    index = -1;
                    if (ownerEvent.HandlerRemover != null)
                    {
                        msr[i].Method = this.GetMethodIndex(ownerEvent.HandlerRemover);
                        msr[i].Semantics = 0x0010;
                        continue;
                    }
                }
                if (index == -1)
                {
                    index = 0;
                    if (ownerEvent.HandlerCaller != null)
                    {
                        msr[i].Method = this.GetMethodIndex(ownerEvent.HandlerCaller);
                        msr[i].Semantics = 0x0020;
                        continue;
                    }
                }
                msr[i].Method = this.GetMethodIndex(ownerEvent.OtherMethods[i]);
                msr[i].Semantics = 0x0004;
                index++;
                continue;
            }
            System.Array.Sort(msr, new MethodSemanticsRowComparer());
            //this.methodSemanticsEntries = null;
        }
        class MethodSemanticsRowComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                MethodSemanticsRow xr = (MethodSemanticsRow)x;
                MethodSemanticsRow yr = (MethodSemanticsRow)y;
                int result = xr.Association - yr.Association;
                if (result == 0) result = xr.Method - yr.Method;
                return result;
            }
        }
        void PopulateModuleTable()
        {
            ModuleRow[] mr = this.writer.moduleTable = new ModuleRow[1];
            string name = this.module.Name;
            if (this.assembly != null)
            {
                if (this.assembly.ModuleName != null)
                    name = this.assembly.ModuleName;
                else
                {
                    string extension = ".exe";
                    if (this.module.Kind == ModuleKindFlags.DynamicallyLinkedLibrary) extension = ".dll";
                    name = name + extension;
                }
            }
            mr[0].Name = this.GetStringIndex(name);
            mr[0].Mvid = this.GetGuidIndex(Guid.NewGuid());
        }
        void PopulateModuleRefTable()
        {
            int n = this.moduleRefEntries.Count;
            if (n == 0) return;
            ModuleRefRow[] mrr = this.writer.moduleRefTable = new ModuleRefRow[n];
            for (int i = 0; i < n; i++)
            {
                ModuleReference moduleRef = this.moduleRefEntries[i];
                mrr[i].Name = this.GetStringIndex(moduleRef.Name);
            }
            //this.moduleRefEntries = null;
        }
        void PopulateNestedClassTable()
        {
            int n = this.nestedClassEntries.Count;
            if (n == 0) return;
            NestedClassRow[] ncr = this.writer.nestedClassTable = new NestedClassRow[n];
            for (int i = 0; i < n; i++)
            {
                TypeNode nt = this.nestedClassEntries[i];
                ncr[i].NestedClass = this.GetTypeDefIndex(nt);
                ncr[i].EnclosingClass = this.GetTypeDefIndex(nt.DeclaringType);
            }
            //this.nestedClassEntries = null;
        }
        void PopulateParamTable()
        {
            int n = this.paramEntries.Count;
            if (n == 0) return;
            ParamRow[] pr = this.writer.paramTable = new ParamRow[n];
            for (int i = 0; i < n; i++)
            {
                Parameter p = this.paramEntries[i];
                if (p == null) continue;
                pr[i].Flags = (int)p.Flags;
                pr[i].Sequence = p.ParameterListIndex + 1;
                pr[i].Name = p.Name == null ? 0 : this.GetStringIndex(p.Name.ToString());
            }
            //this.paramEntries = null;
        }
        void PopulatePropertyTable()
        {
            int n = this.propertyEntries.Count;
            if (n == 0) return;
            PropertyRow[] pr = this.writer.propertyTable = new PropertyRow[n];
            for (int i = 0; i < n; i++)
            {
                Property p = this.propertyEntries[i];
                if (p == null || p.Name == null) continue;
                pr[i].Flags = (int)p.Flags;
                pr[i].Name = this.GetStringIndex(p.Name.ToString());
                pr[i].Signature = this.GetBlobIndex(p);
            }
            //this.propertyEntries = null;
        }
        void PopulatePropertyMapTable()
        {
            int n = this.propertyMapEntries.Count;
            if (n == 0) return;
            PropertyMapRow[] pmr = this.writer.propertyMapTable = new PropertyMapRow[n];
            for (int i = 0; i < n; i++)
            {
                Property p = this.propertyMapEntries[i];
                pmr[i].Parent = this.GetTypeDefIndex(p.DeclaringType);
                pmr[i].PropertyList = this.GetPropertyIndex(p);
            }
            //this.propertyMapEntries = null;
        }
        void PopulateStandAloneSigTable()
        {
            int n = this.standAloneSignatureEntries.Count;
            if (n == 0) return;
            StandAloneSigRow[] sasr = this.writer.standAloneSigTable = new StandAloneSigRow[n];
            for (int i = 0; i < n; i++)
            {
                BinaryWriter sigWriter = (BinaryWriter)this.standAloneSignatureEntries[i];
                sasr[i].Signature = this.GetBlobIndex(((MemoryStream)sigWriter.BaseStream).ToArray());
            }
        }
        void PopulateTypeDefTable()
        {
            int n = this.typeDefEntries.Count;
            if (n == 0) return;
            TypeDefRow[] tdr = this.writer.typeDefTable = new TypeDefRow[n];
            for (int i = 0; i < n; i++)
            {
                TypeNode t = this.typeDefEntries[i];
                if (t == null) continue;
                tdr[i].Flags = (int)t.Flags;
                tdr[i].Name = this.GetStringIndex(t.Name == null ? "" : t.Name.ToString());
                tdr[i].Namespace = t.Namespace == null ? 0 : this.GetStringIndex(t.Namespace == null ? "" : t.Namespace.ToString());
                tdr[i].Extends = this.GetTypeDefOrRefOrSpecEncoded(t.BaseType);
                MemberList members = t.Members;
                int m = members.Count;
                for (int j = 0; j < m; j++)
                {
                    Member mem = members[j];
                    if (mem == null) continue;
                    if (mem.NodeType == NodeType.Field)
                    {
                        tdr[i].FieldList = this.GetFieldIndex((Field)mem);
                        break;
                    }
                }
                for (int j = 0; j < m; j++)
                {
                    Member mem = members[j];
                    if (mem == null) continue;
                    switch (mem.NodeType)
                    {
                        case NodeType.Method:
                        case NodeType.InstanceInitializer:
                        case NodeType.StaticInitializer:
                            tdr[i].MethodList = this.GetMethodIndex((Method)mem);
                            goto done;
                    }
                }
            done: continue;
            }
            //this.typeDefEntries = null;
        }
        void PopulateTypeRefTable()
        {
            int n = this.typeRefEntries.Count;
            if (n == 0) return;
            TypeRefRow[] trr = this.writer.typeRefTable = new TypeRefRow[n];
            for (int i = 0; i < n; i++)
            {
                TypeNode t = this.typeRefEntries[i];
                if (t == null || t.Name == null || t.Namespace == null) continue;
                trr[i].Name = this.GetStringIndex(t.Name.ToString());
                trr[i].Namespace = this.GetStringIndex(t.Namespace.ToString());
                if (t.DeclaringType == null)
                    if (t.DeclaringModule is AssemblyNode)
                        trr[i].ResolutionScope = (this.GetAssemblyRefIndex((AssemblyNode)t.DeclaringModule) << 2) | 2;
                    else
                        trr[i].ResolutionScope = (this.GetModuleRefIndex(t.DeclaringModule) << 2) | 1;
                else
                    trr[i].ResolutionScope = (this.GetTypeRefIndex(t.DeclaringType) << 2) | 3;
            }
            //this.typeRefEntries = null;
        }
        void PopulateTypeSpecTable()
        {
            int n = this.typeSpecEntries.Count;
            if (n == 0) return;
            TypeSpecRow[] tsr = this.writer.typeSpecTable = new TypeSpecRow[n];
            for (int i = 0; i < n; i++)
            {
                TypeNode t = this.typeSpecEntries[i];
                tsr[i].Signature = this.GetBlobIndex(t);
                //TODO: eliminate duplicates
            }
            //this.typeSpecEntries = null;
        }
        void Visit(Node node)
        {
            if (node == null) return;
            switch (node.NodeType)
            {
                case NodeType.AddressDereference:
                    this.VisitAddressDereference((AddressDereference)node); return;
                case NodeType.Arglist:
                    this.VisitExpression((Expression)node); return;
                case NodeType.AssignmentStatement:
                    this.VisitAssignmentStatement((AssignmentStatement)node); return;
#if !MinimalReader
                case NodeType.Base:
                    this.VisitBase((Base)node); return;
#endif
                case NodeType.Block:
                    this.VisitBlock((Block)node); return;
#if !MinimalReader
                case NodeType.BlockExpression:
                    this.VisitBlockExpression((BlockExpression)node); return;
#endif
                case NodeType.Branch:
                    this.VisitBranch((Branch)node); return;
                case NodeType.DebugBreak:
                    this.VisitStatement((Statement)node); return;
                case NodeType.Call:
                case NodeType.Calli:
                case NodeType.Callvirt:
                case NodeType.Jmp:
#if !MinimalReader
                case NodeType.MethodCall:
#endif
                    this.VisitMethodCall((MethodCall)node); return;
                case NodeType.Class:
                case NodeType.ClassParameter:
                    this.VisitClass((Class)node); return;
                case NodeType.Construct:
                    this.VisitConstruct((Construct)node); return;
                case NodeType.ConstructArray:
                    this.VisitConstructArray((ConstructArray)node); return;
                case NodeType.DelegateNode:
                    this.VisitDelegateNode((DelegateNode)node); return;
                case NodeType.Dup:
                    this.VisitExpression((Expression)node); return;
                case NodeType.EndFilter:
                    this.VisitEndFilter((EndFilter)node); return;
                case NodeType.EndFinally:
                    this.VisitStatement((Statement)node); return;
                case NodeType.EnumNode:
                    this.VisitEnumNode((EnumNode)node); return;
                case NodeType.Event:
                    this.VisitEvent((Event)node); return;
                case NodeType.ExpressionStatement:
                    this.VisitExpressionStatement((ExpressionStatement)node); return;
                case NodeType.Field:
                    this.VisitField((Field)node); return;
                case NodeType.Indexer:
                    this.VisitIndexer((Indexer)node); return;
                case NodeType.InstanceInitializer:
                case NodeType.StaticInitializer:
                case NodeType.Method:
                    this.VisitMethod((Method)node); return;
                case NodeType.TypeParameter:
                case NodeType.Interface:
                    this.VisitInterface((Interface)node); return;
                case NodeType.Literal:
                    this.VisitLiteral((Literal)node); return;
                case NodeType.Local:
                    this.VisitLocal((Local)node); return;
#if !MinimalReader
                case NodeType.LocalDeclarationsStatement:
                    this.VisitLocalDeclarationsStatement((LocalDeclarationsStatement)node); return;
#endif
                case NodeType.MemberBinding:
                    this.VisitMemberBinding((MemberBinding)node); return;
                case NodeType.Nop:
                    this.VisitStatement((Statement)node); return;
                case NodeType.Parameter:
                    this.VisitParameter((Parameter)node); return;
                case NodeType.Pop:
                    this.VisitExpression((Expression)node); return;
                case NodeType.Property:
                    this.VisitProperty((Property)node); return;
                case NodeType.Rethrow:
                case NodeType.Throw:
                    this.VisitThrow((Throw)node); return;
                case NodeType.Return:
                    this.VisitReturn((Return)node); return;
                case NodeType.Struct:
#if !MinimalReader
                case NodeType.TypeAlias:
                case NodeType.TypeIntersection:
                case NodeType.TypeUnion:
                case NodeType.TupleType:
#endif
                    this.VisitStruct((Struct)node); return;
#if !MinimalReader
                case NodeType.SwitchCaseBottom:
                    return;
#endif
                case NodeType.SwitchInstruction:
                    this.VisitSwitchInstruction((SwitchInstruction)node); return;
                case NodeType.This:
                    this.VisitThis((This)node); return;

                case NodeType.Cpblk:
                case NodeType.Initblk:
                    this.VisitTernaryExpression((TernaryExpression)node); return;

                case NodeType.Add:
                case NodeType.Add_Ovf:
                case NodeType.Add_Ovf_Un:
                case NodeType.And:
                case NodeType.Box:
                case NodeType.Castclass:
                case NodeType.Ceq:
                case NodeType.Cgt:
                case NodeType.Cgt_Un:
                case NodeType.Clt:
                case NodeType.Clt_Un:
                case NodeType.Div:
                case NodeType.Div_Un:
                case NodeType.Eq:
                case NodeType.Ge:
                case NodeType.Gt:
#if !MinimalReader
                case NodeType.Is:
#endif
                case NodeType.Isinst:
                case NodeType.Ldvirtftn:
                case NodeType.Le:
                case NodeType.Lt:
                case NodeType.Mkrefany:
                case NodeType.Mul:
                case NodeType.Mul_Ovf:
                case NodeType.Mul_Ovf_Un:
                case NodeType.Ne:
                case NodeType.Or:
                case NodeType.Refanyval:
                case NodeType.Rem:
                case NodeType.Rem_Un:
                case NodeType.Shl:
                case NodeType.Shr:
                case NodeType.Shr_Un:
                case NodeType.Sub:
                case NodeType.Sub_Ovf:
                case NodeType.Sub_Ovf_Un:
                case NodeType.Unbox:
                case NodeType.UnboxAny:
                case NodeType.Xor:
                    this.VisitBinaryExpression((BinaryExpression)node); return;


                case NodeType.AddressOf:
#if !MinimalReader
                case NodeType.OutAddress:
                case NodeType.RefAddress:
#endif
                case NodeType.ReadOnlyAddressOf:
                    this.VisitAddressOf((UnaryExpression)node); return;
                case NodeType.Ckfinite:
                case NodeType.Conv_I:
                case NodeType.Conv_I1:
                case NodeType.Conv_I2:
                case NodeType.Conv_I4:
                case NodeType.Conv_I8:
                case NodeType.Conv_Ovf_I:
                case NodeType.Conv_Ovf_I1:
                case NodeType.Conv_Ovf_I1_Un:
                case NodeType.Conv_Ovf_I2:
                case NodeType.Conv_Ovf_I2_Un:
                case NodeType.Conv_Ovf_I4:
                case NodeType.Conv_Ovf_I4_Un:
                case NodeType.Conv_Ovf_I8:
                case NodeType.Conv_Ovf_I8_Un:
                case NodeType.Conv_Ovf_I_Un:
                case NodeType.Conv_Ovf_U:
                case NodeType.Conv_Ovf_U1:
                case NodeType.Conv_Ovf_U1_Un:
                case NodeType.Conv_Ovf_U2:
                case NodeType.Conv_Ovf_U2_Un:
                case NodeType.Conv_Ovf_U4:
                case NodeType.Conv_Ovf_U4_Un:
                case NodeType.Conv_Ovf_U8:
                case NodeType.Conv_Ovf_U8_Un:
                case NodeType.Conv_Ovf_U_Un:
                case NodeType.Conv_R4:
                case NodeType.Conv_R8:
                case NodeType.Conv_R_Un:
                case NodeType.Conv_U:
                case NodeType.Conv_U1:
                case NodeType.Conv_U2:
                case NodeType.Conv_U4:
                case NodeType.Conv_U8:
                case NodeType.Ldftn:
                case NodeType.Ldlen:
                case NodeType.Ldtoken:
                case NodeType.Localloc:
                case NodeType.Neg:
                case NodeType.Not:
                case NodeType.Refanytype:
                case NodeType.Sizeof:
                    this.VisitUnaryExpression((UnaryExpression)node); return;

                default:
                    // handle type extensions with new NodeType's, that are emitted as ordinary structs and classes
                    Class cl = node as Class;
                    if (cl != null)
                    {
                        this.VisitClass(cl); return;
                    }
                    Struct st = node as Struct;
                    if (st != null)
                    {
                        this.VisitStruct(st); return;
                    }
                    Debug.Assert(false, "invalid node: " + node.NodeType.ToString());
                    return;
            }
        }
        void VisitAddressDereference(AddressDereference/*!*/ adr)
        {
            this.Visit(adr.Address);
            if (adr.Alignment > 0)
            {
                this.methodBodyHeap.Write((byte)0xfe);
                this.methodBodyHeap.Write((byte)0x12);
                this.methodBodyHeap.Write((byte)adr.Alignment);
            }
            if (adr.Volatile)
            {
                this.methodBodyHeap.Write((byte)0xfe);
                this.methodBodyHeap.Write((byte)0x13);
            }
            switch (adr.Type.typeCode)
            {
                case ElementType.Int8: this.methodBodyHeap.Write((byte)0x46); return;
                case ElementType.UInt8: this.methodBodyHeap.Write((byte)0x47); return;
                case ElementType.Int16: this.methodBodyHeap.Write((byte)0x48); return;
                case ElementType.Char:
                case ElementType.UInt16: this.methodBodyHeap.Write((byte)0x49); return;
                case ElementType.Int32: this.methodBodyHeap.Write((byte)0x4a); return;
                case ElementType.UInt32: this.methodBodyHeap.Write((byte)0x4b); return;
                case ElementType.Int64:
                case ElementType.UInt64: this.methodBodyHeap.Write((byte)0x4c); return;
                //case ElementType.UIntPtr:
                //case ElementType.IntPtr: this.methodBodyHeap.Write((byte)0x4d); return;
                case ElementType.Single: this.methodBodyHeap.Write((byte)0x4e); return;
                case ElementType.Double: this.methodBodyHeap.Write((byte)0x4f); return;
                default:
                    if (adr.Type.IsValueType || (adr.Type is ITypeParameter && this.UseGenerics))
                    {
                        this.methodBodyHeap.Write((byte)0x71);
                        this.methodBodyHeap.Write((int)this.GetTypeToken(adr.Type));
                        return;
                    }
                    else if (TypeNode.StripModifiers(adr.Type) is Pointer)
                    {
                        this.methodBodyHeap.Write((byte)0x4d); return;
                    }
                    this.methodBodyHeap.Write((byte)0x50);
                    return;
            }
        }
        void VisitAttributeList(AttributeList attrs, Node/*!*/ node)
        {
            if (attrs == null) return;
            int n = attrs.Count;
            if (n == 0) return;
            int m = n;
            for (int j = 0; j < n; j++)
            {
                AttributeNode a = attrs[j];
                if (a == null) m--;
            }
            if (m == 0) return;
            n = m;
            int codedIndex = this.GetCustomAttributeParentCodedIndex(node);
            this.customAttributeCount += n;
            m = this.nodesWithCustomAttributes.Count;
            this.nodesWithCustomAttributes.Add(node);
            int i = 0; //after the for loop i will be position where the new node should be in sorted list
            NodeList nodes = this.nodesWithCustomAttributes;
            for (i = m; i > 0; i--)
            {
                Node other = nodes[i - 1];
                int oci = this.GetCustomAttributeParentCodedIndex(other);
                if (oci < codedIndex) break;
                if (UseGenerics)
                {
                    if (oci == codedIndex) Debug.Assert(false);
                }
            }
            if (i == m) return; //node is already where it should be
            for (int j = m; j > i; j--) nodes[j] = nodes[j - 1]; //Make space at postion i
            nodes[i] = node;
        }
        void VisitAddressOf(UnaryExpression/*!*/ expr)
        {
            Expression operand = expr.Operand;
            if (operand == null) return;
            switch (operand.NodeType)
            {
                case NodeType.Indexer:
                    Indexer indexer = (Indexer)operand;
                    this.Visit(indexer.Object);
                    if (indexer.Operands == null || indexer.Operands.Count < 1) return;
                    this.Visit(indexer.Operands[0]);
                    if (expr.NodeType == NodeType.ReadOnlyAddressOf)
                    {
                        this.methodBodyHeap.Write((byte)0xfe);
                        this.methodBodyHeap.Write((byte)0x1e);
                    }
                    this.methodBodyHeap.Write((byte)0x8f);
                    this.methodBodyHeap.Write((int)this.GetTypeToken(indexer.ElementType));
                    this.stackHeight--;
                    return;
                case NodeType.Local:
                    int li = this.GetLocalVarIndex((Local)operand);
                    if (li < 256)
                    {
                        this.methodBodyHeap.Write((byte)0x12);
                        this.methodBodyHeap.Write((byte)li);
                    }
                    else
                    {
                        this.methodBodyHeap.Write((byte)0xfe);
                        this.methodBodyHeap.Write((byte)0x0d);
                        this.methodBodyHeap.Write((ushort)li);
                    }
                    this.IncrementStackHeight();
                    return;
                case NodeType.MemberBinding:
                    MemberBinding mb = (MemberBinding)operand;
                    if (mb.TargetObject != null)
                    {
                        this.Visit(mb.TargetObject);
                        this.methodBodyHeap.Write((byte)0x7c);
                    }
                    else
                    {
                        this.methodBodyHeap.Write((byte)0x7f);
                        this.IncrementStackHeight();
                    }
                    this.methodBodyHeap.Write((int)this.GetFieldToken((Field)mb.BoundMember));
                    return;
                case NodeType.Parameter:
#if !MinimalReader
                    ParameterBinding pb = operand as ParameterBinding;
                    if (pb != null) operand = pb.BoundParameter;
#endif
                    int pi = ((Parameter)operand).ArgumentListIndex;
                    if (pi < 256)
                    {
                        this.methodBodyHeap.Write((byte)0x0f);
                        this.methodBodyHeap.Write((byte)pi);
                    }
                    else
                    {
                        this.methodBodyHeap.Write((byte)0xfe);
                        this.methodBodyHeap.Write((byte)0x0a);
                        this.methodBodyHeap.Write((ushort)pi);
                    }
                    this.IncrementStackHeight();
                    return;
            }
        }
        void VisitAssignmentStatement(AssignmentStatement/*!*/ assignment)
        {
            this.DefineSequencePoint(assignment);
            Expression target = assignment.Target;
            switch (assignment.Target.NodeType)
            {
                case NodeType.Local:
                    Local loc = (Local)target;
                    this.Visit(assignment.Source);
                    this.stackHeight--;
                    int li = this.GetLocalVarIndex(loc);
                    switch (li)
                    {
                        case 0: this.methodBodyHeap.Write((byte)0x0a); return;
                        case 1: this.methodBodyHeap.Write((byte)0x0b); return;
                        case 2: this.methodBodyHeap.Write((byte)0x0c); return;
                        case 3: this.methodBodyHeap.Write((byte)0x0d); return;
                        default:
                            if (li < 256)
                            {
                                this.methodBodyHeap.Write((byte)0x13);
                                this.methodBodyHeap.Write((byte)li);
                            }
                            else
                            {
                                this.methodBodyHeap.Write((byte)0xfe);
                                this.methodBodyHeap.Write((byte)0x0e);
                                this.methodBodyHeap.Write((ushort)li);
                            }
                            return;
                    }
                case NodeType.MemberBinding:
                    MemberBinding mb = (MemberBinding)target;
                    if (mb.TargetObject != null) this.Visit(mb.TargetObject);
                    this.Visit(assignment.Source);
                    if (mb.TargetObject != null)
                    {
                        if (mb.Alignment != -1)
                        {
                            this.methodBodyHeap.Write((byte)0xfe);
                            this.methodBodyHeap.Write((byte)0x12);
                            this.methodBodyHeap.Write((byte)mb.Alignment);
                        }
                        if (mb.Volatile)
                        {
                            this.methodBodyHeap.Write((byte)0xfe);
                            this.methodBodyHeap.Write((byte)0x13);
                        }
                        this.methodBodyHeap.Write((byte)0x7d);
                    }
                    else
                    {
                        if (mb.Volatile)
                        {
                            this.methodBodyHeap.Write((byte)0xfe);
                            this.methodBodyHeap.Write((byte)0x13);
                        }
                        this.methodBodyHeap.Write((byte)0x80);
                    }
                    this.methodBodyHeap.Write((int)this.GetFieldToken((Field)mb.BoundMember));
                    if (mb.TargetObject != null)
                        this.stackHeight -= 2;
                    else
                        this.stackHeight--;
                    return;
                case NodeType.Parameter:
#if !MinimalReader
                    ParameterBinding pb = target as ParameterBinding;
                    if (pb != null) target = pb.BoundParameter;
#endif
                    Parameter par = (Parameter)target;
                    this.Visit(assignment.Source);
                    int pi = par.ArgumentListIndex;
                    if (pi < 256)
                    {
                        this.methodBodyHeap.Write((byte)0x10);
                        this.methodBodyHeap.Write((byte)pi);
                    }
                    else
                    {
                        this.methodBodyHeap.Write((byte)0xfe);
                        this.methodBodyHeap.Write((byte)0x0b);
                        this.methodBodyHeap.Write((ushort)pi);
                    }
                    this.stackHeight--;
                    return;
                case NodeType.Indexer:
                    Indexer indexer = (Indexer)target;
                    this.Visit(indexer.Object);
                    if (indexer.Operands == null || indexer.Operands.Count < 1) return;
                    this.Visit(indexer.Operands[0]);
                    this.Visit(assignment.Source);
                    byte opCode;
                    switch (indexer.ElementType.typeCode)
                    {
                        case ElementType.UIntPtr:
                        case ElementType.IntPtr: opCode = 0x9b; break;
                        case ElementType.Boolean:
                        case ElementType.Int8:
                        case ElementType.UInt8: opCode = 0x9c; break;
                        case ElementType.Char:
                        case ElementType.Int16:
                        case ElementType.UInt16: opCode = 0x9d; break;
                        case ElementType.Int32:
                        case ElementType.UInt32: opCode = 0x9e; break;
                        case ElementType.Int64:
                        case ElementType.UInt64: opCode = 0x9f; break;
                        case ElementType.Single: opCode = 0xa0; break;
                        case ElementType.Double: opCode = 0xa1; break;
                        default:
                            if (this.UseGenerics && (indexer.ElementType is ITypeParameter))
                                opCode = 0xa4;
                            else if (TypeNode.StripModifiers(indexer.ElementType) is Pointer)
                                opCode = 0x9b;
                            else
                                opCode = 0xa2;
                            break;
                    }
                    this.methodBodyHeap.Write((byte)opCode);
                    if (opCode == 0xa4) this.methodBodyHeap.Write((int)this.GetTypeToken(indexer.ElementType));
                    this.stackHeight -= 3;
                    return;
                case NodeType.AddressDereference:
                    AddressDereference adr = (AddressDereference)target;
                    this.Visit(adr.Address);
                    if (adr.Type.IsValueType || adr.Type is ITypeParameter)
                    {
                        Literal lit = assignment.Source as Literal;
                        if (lit != null && lit.Value == null)
                        {
                            this.methodBodyHeap.Write((byte)0xfe);
                            this.methodBodyHeap.Write((byte)0x15);
                            this.methodBodyHeap.Write((int)this.GetTypeToken(adr.Type));
                            this.stackHeight--;
                            return;
                        }
                    }
                    this.Visit(assignment.Source);
                    this.stackHeight -= 2;
                    if (adr.Alignment > 0)
                    {
                        this.methodBodyHeap.Write((byte)0xfe);
                        this.methodBodyHeap.Write((byte)0x12);
                        this.methodBodyHeap.Write((byte)adr.Alignment);
                    }
                    if (adr.Volatile)
                    {
                        this.methodBodyHeap.Write((byte)0xfe);
                        this.methodBodyHeap.Write((byte)0x13);
                    }
                    TypeNode adrType = TypeNode.StripModifiers(adr.Type);
                    if (adrType == null) return;
                    switch (adrType.typeCode)
                    {
                        case ElementType.Int8:
                        case ElementType.UInt8: this.methodBodyHeap.Write((byte)0x52); return;
                        case ElementType.Int16:
                        case ElementType.UInt16: this.methodBodyHeap.Write((byte)0x53); return;
                        case ElementType.Int32:
                        case ElementType.UInt32: this.methodBodyHeap.Write((byte)0x54); return;
                        case ElementType.Int64:
                        case ElementType.UInt64: this.methodBodyHeap.Write((byte)0x55); return;
                        case ElementType.Single: this.methodBodyHeap.Write((byte)0x56); return;
                        case ElementType.Double: this.methodBodyHeap.Write((byte)0x57); return;
                        case ElementType.UIntPtr:
                        case ElementType.IntPtr: this.methodBodyHeap.Write((byte)0xdf); return;
                        default:
                            if (adrType != null && (adrType.IsValueType ||
                              this.UseGenerics && (adrType is ITypeParameter)))
                            {
                                this.methodBodyHeap.Write((byte)0x81);
                                this.methodBodyHeap.Write((int)this.GetTypeToken(adrType));
                                return;
                            }
                            if (adrType.NodeType == NodeType.Pointer)
                            {
                                this.methodBodyHeap.Write((byte)0xdf); return;
                            }
                            this.methodBodyHeap.Write((byte)0x51);
                            return;
                    }
                default:
                    Debug.Assert(false, "unexpected assignment target");
                    return;
            }
        }
#if !MinimalReader
        void VisitBase(Base/*!*/ Base)
        {
            this.IncrementStackHeight();
            this.methodBodyHeap.Write((byte)0x02);
        }
#endif
        void VisitBinaryExpression(BinaryExpression/*!*/ binaryExpression)
        {
            byte opCode = 0;
            this.Visit(binaryExpression.Operand1);
            switch (binaryExpression.NodeType)
            {
                case NodeType.Castclass: opCode = 0x74; goto writeOpCodeAndToken;
                case NodeType.Isinst: opCode = 0x75; goto writeOpCodeAndToken;
                case NodeType.Unbox: opCode = 0x79; goto writeOpCodeAndToken;
                case NodeType.UnboxAny: opCode = 0xa5; goto writeOpCodeAndToken;
                case NodeType.Box: opCode = 0x8c; goto writeOpCodeAndToken;
                case NodeType.Refanyval: opCode = 0xc2; goto writeOpCodeAndToken;
                case NodeType.Mkrefany: opCode = 0xc6; goto writeOpCodeAndToken;
                writeOpCodeAndToken:
                    this.methodBodyHeap.Write((byte)opCode);
                Literal lit = binaryExpression.Operand2 as Literal;
                if (lit != null)
                    this.methodBodyHeap.Write((int)this.GetTypeToken((TypeNode)lit.Value));
                else
                {
                    // TODO: Normalized IR should never use a MemberBinding to represent a type
                    this.methodBodyHeap.Write((int)this.GetTypeToken((TypeNode)((MemberBinding)binaryExpression.Operand2).BoundMember));
                }
                return;
                case NodeType.Ldvirtftn: opCode = 0x07; this.methodBodyHeap.Write((byte)0xfe);
                this.methodBodyHeap.Write((byte)opCode);
                this.methodBodyHeap.Write((int)this.GetMethodToken((Method)((MemberBinding)binaryExpression.Operand2).BoundMember));
                return;
            }
            this.Visit(binaryExpression.Operand2);
            switch (binaryExpression.NodeType)
            {
                case NodeType.Add: opCode = 0x58; break;
                case NodeType.Sub: opCode = 0x59; break;
                case NodeType.Mul: opCode = 0x5a; break;
                case NodeType.Div: opCode = 0x5b; break;
                case NodeType.Div_Un: opCode = 0x5c; break;
                case NodeType.Rem: opCode = 0x5d; break;
                case NodeType.Rem_Un: opCode = 0x5e; break;
                case NodeType.And: opCode = 0x5f; break;
                case NodeType.Or: opCode = 0x60; break;
                case NodeType.Xor: opCode = 0x61; break;
                case NodeType.Shl: opCode = 0x62; break;
                case NodeType.Shr: opCode = 0x63; break;
                case NodeType.Shr_Un: opCode = 0x64; break;
                case NodeType.Add_Ovf: opCode = 0xd6; break;
                case NodeType.Add_Ovf_Un: opCode = 0xd7; break;
                case NodeType.Mul_Ovf: opCode = 0xd8; break;
                case NodeType.Mul_Ovf_Un: opCode = 0xd9; break;
                case NodeType.Sub_Ovf: opCode = 0xda; break;
                case NodeType.Sub_Ovf_Un: opCode = 0xdb; break;
                case NodeType.Ceq: opCode = 0x01; this.methodBodyHeap.Write((byte)0xfe); break;
                case NodeType.Cgt: opCode = 0x02; this.methodBodyHeap.Write((byte)0xfe); break;
                case NodeType.Cgt_Un: opCode = 0x03; this.methodBodyHeap.Write((byte)0xfe); break;
                case NodeType.Clt: opCode = 0x04; this.methodBodyHeap.Write((byte)0xfe); break;
                case NodeType.Clt_Un: opCode = 0x05; this.methodBodyHeap.Write((byte)0xfe); break;
            }
            this.methodBodyHeap.Write((byte)opCode);
            this.stackHeight--;
        }
        void VisitBlock(Block/*!*/ block)
        {
            MethodInfo mInfo = this.methodInfo;
            int currentAddress = (int)this.methodBodyHeap.BaseStream.Position;
            this.VisitFixupList((Fixup)this.methodInfo.fixupIndex[block.UniqueKey], currentAddress);
            mInfo.fixupIndex[block.UniqueKey] = currentAddress;
            this.methodBodyHeap.BaseStream.Position = currentAddress;
            int savedStackHeight = this.stackHeight;
            if (this.exceptionBlock[block.UniqueKey] != null) this.stackHeight = 1;
            StatementList statements = block.Statements;
            if (statements == null) return;
#if !ROTOR
            if (this.symWriter != null && block.HasLocals)
            {
                LocalList savedDebugLocals = mInfo.debugLocals;
                Int32List savedSignatureLengths = mInfo.signatureLengths;
                Int32List savedSignatureOffsets = mInfo.signatureOffsets;
                mInfo.debugLocals = new LocalList();
                mInfo.signatureLengths = new Int32List();
                mInfo.signatureOffsets = new Int32List();
                this.symWriter.OpenScope((uint)currentAddress);
                for (int i = 0, n = statements.Count; i < n; i++)
                    this.Visit(statements[i]);
                if (this.stackHeight > 0) this.stackHeightExitTotal += this.stackHeight;
                this.DefineLocalVariables(currentAddress, mInfo.debugLocals);
                mInfo.debugLocals = savedDebugLocals;
                mInfo.signatureLengths = savedSignatureLengths;
                mInfo.signatureOffsets = savedSignatureOffsets;
            }
            else
            {
#endif
                for (int i = 0, n = statements.Count; i < n; i++)
                    this.Visit(statements[i]);
                if (this.stackHeight > savedStackHeight) this.stackHeightExitTotal += (this.stackHeight - savedStackHeight);
#if !ROTOR
            }
#endif
            this.stackHeight = savedStackHeight;
        }
#if !MinimalReader
        void VisitBlockExpression(BlockExpression/*!*/ blockExpression)
        {
            if (blockExpression.Block == null) return;
            this.VisitBlock(blockExpression.Block);
        }
#endif
        void VisitBranch(Branch/*!*/ branch)
        {
            this.DefineSequencePoint(branch);
            BinaryExpression bex = branch.Condition as BinaryExpression;
            UnaryExpression uex = null;
            NodeType typeOfCondition = NodeType.Nop;
            if (bex != null)
            {
                switch (bex.NodeType)
                {
                    case NodeType.Eq:
                    case NodeType.Ge:
                    case NodeType.Gt:
                    case NodeType.Le:
                    case NodeType.Lt:
                    case NodeType.Ne:
                        this.Visit(bex.Operand1);
                        this.Visit(bex.Operand2);
                        typeOfCondition = bex.NodeType;
                        this.stackHeight -= 2;
                        break;
                    case NodeType.And:
                    case NodeType.Or:
                    case NodeType.Xor:
                    case NodeType.Isinst:
                    case NodeType.Castclass:
                        typeOfCondition = bex.NodeType;
                        goto default;
                    default:
                        this.Visit(branch.Condition);
                        this.stackHeight--;
                        break;
                }
            }
            else
            {
                uex = branch.Condition as UnaryExpression;
                if (uex != null && uex.NodeType == NodeType.LogicalNot)
                {
                    this.Visit(uex.Operand);
                    typeOfCondition = NodeType.LogicalNot;
                    this.stackHeight--;
                }
                else if (branch.Condition != null)
                {
                    // Undefined is used here simply as a sentinel value
                    typeOfCondition = NodeType.Undefined;
                    this.Visit(branch.Condition);
                    this.stackHeight--;
                }
            }
            int target = this.GetOffset(branch.Target, ref branch.shortOffset);
            if (branch.ShortOffset)
            {
                switch (typeOfCondition)
                {
                    case NodeType.Nop:
                        if (branch.Condition == null)
                        {
                            if (branch.LeavesExceptionBlock)
                                this.methodBodyHeap.Write((byte)0xde);
                            else
                                this.methodBodyHeap.Write((byte)0x2b);
                            break;
                        }
                        else
                        {
                            this.methodBodyHeap.Write((byte)0x2d); break;
                        }
                    case NodeType.And:
                    case NodeType.Or:
                    case NodeType.Xor:
                    case NodeType.Isinst:
                    case NodeType.Castclass:
                    case NodeType.Undefined:
                        this.methodBodyHeap.Write((byte)0x2d); break;
                    case NodeType.LogicalNot:
                        this.methodBodyHeap.Write((byte)0x2c); break;
                    case NodeType.Eq:
                        this.methodBodyHeap.Write((byte)0x2e); break;
                    case NodeType.Ge:
                        if (branch.BranchIfUnordered)
                            this.methodBodyHeap.Write((byte)0x34);
                        else
                            this.methodBodyHeap.Write((byte)0x2f);
                        break;
                    case NodeType.Gt:
                        if (branch.BranchIfUnordered)
                            this.methodBodyHeap.Write((byte)0x35);
                        else
                            this.methodBodyHeap.Write((byte)0x30);
                        break;
                    case NodeType.Le:
                        if (branch.BranchIfUnordered)
                            this.methodBodyHeap.Write((byte)0x36);
                        else
                            this.methodBodyHeap.Write((byte)0x31);
                        break;
                    case NodeType.Lt:
                        if (branch.BranchIfUnordered)
                            this.methodBodyHeap.Write((byte)0x37);
                        else
                            this.methodBodyHeap.Write((byte)0x32);
                        break;
                    case NodeType.Ne:
                        this.methodBodyHeap.Write((byte)0x33);
                        break;
                }
                this.methodBodyHeap.Write((sbyte)target);
            }
            else
            {
                switch (typeOfCondition)
                {
                    case NodeType.Nop:
                        if (branch.Condition == null)
                        {
                            if (branch.LeavesExceptionBlock)
                                this.methodBodyHeap.Write((byte)0xdd);
                            else
                                this.methodBodyHeap.Write((byte)0x38);
                            break;
                        }
                        else
                        {
                            this.methodBodyHeap.Write((byte)0x3a); break;
                        }
                    case NodeType.And:
                    case NodeType.Or:
                    case NodeType.Xor:
                    case NodeType.Isinst:
                    case NodeType.Castclass:
                    case NodeType.Undefined:
                        this.methodBodyHeap.Write((byte)0x3a); break;
                    case NodeType.LogicalNot:
                        this.methodBodyHeap.Write((byte)0x39); break;
                    case NodeType.Eq:
                        this.methodBodyHeap.Write((byte)0x3b); break;
                    case NodeType.Ge:
                        if (branch.BranchIfUnordered)
                            this.methodBodyHeap.Write((byte)0x41);
                        else
                            this.methodBodyHeap.Write((byte)0x3c);
                        break;
                    case NodeType.Gt:
                        if (branch.BranchIfUnordered)
                            this.methodBodyHeap.Write((byte)0x42);
                        else
                            this.methodBodyHeap.Write((byte)0x3d);
                        break;
                    case NodeType.Le:
                        if (branch.BranchIfUnordered)
                            this.methodBodyHeap.Write((byte)0x43);
                        else
                            this.methodBodyHeap.Write((byte)0x3e);
                        break;
                    case NodeType.Lt:
                        if (branch.BranchIfUnordered)
                            this.methodBodyHeap.Write((byte)0x44);
                        else
                            this.methodBodyHeap.Write((byte)0x3f);
                        break;
                    case NodeType.Ne:
                        this.methodBodyHeap.Write((byte)0x40); break;
                }
                this.methodBodyHeap.Write((int)target);
            }
        }
        void VisitMethodCall(MethodCall/*!*/ call)
        {
            MemberBinding mb = (MemberBinding)call.Callee;
            TypeNode constraint = call.Constraint;
            this.Visit(mb.TargetObject);
            ExpressionList arguments = call.Operands;
            int pops = 0;
            if (arguments != null)
            {
                this.VisitExpressionList(arguments);
                pops = arguments.Count;
            }
            if (call.Type != CoreSystemTypes.Void) { this.VisitReferencedType(call.Type); pops--; }
            if (pops >= 0)
                this.stackHeight -= pops;
            else
                this.IncrementStackHeight(); //make sure the high water mark moves up if necessary
            if (call.IsTailCall)
            {
                this.methodBodyHeap.Write((byte)0xfe);
                this.methodBodyHeap.Write((byte)0x14);
            }
            else if (constraint != null)
            {
                this.methodBodyHeap.Write((byte)0xfe);
                this.methodBodyHeap.Write((byte)0x16);
                this.methodBodyHeap.Write((int)this.GetTypeToken(constraint));
            }
            switch (call.NodeType)
            {
                case NodeType.Calli:
                    this.methodBodyHeap.Write((byte)0x29);
                    BinaryWriter sig = new BinaryWriter(new MemoryStream());
                    this.WriteMethodSignature(sig, (FunctionPointer)mb.BoundMember);
                    this.methodBodyHeap.Write((int)(0x11000000 | this.GetStandAloneSignatureIndex(sig)));
                    return;
                case NodeType.Callvirt: this.methodBodyHeap.Write((byte)0x6f); break;
                case NodeType.Jmp: this.methodBodyHeap.Write((byte)0x27); break;
                default: this.methodBodyHeap.Write((byte)0x28); break;
            }
            Method method = (Method)mb.BoundMember;
            if ((method.CallingConvention & (CallingConventionFlags)7) == CallingConventionFlags.VarArg ||
              (method.CallingConvention & (CallingConventionFlags)7) == CallingConventionFlags.C)
            {
                this.methodBodyHeap.Write((int)this.GetMemberRefToken(method, arguments));
            }
            else
                this.methodBodyHeap.Write((int)this.GetMethodToken(method));
        }
        void VisitClass(Class/*!*/ Class)
        {
            if (this.UseGenerics && Class.Template != null && Class.Template.IsGeneric) return;
            this.VisitAttributeList(Class.Attributes, Class);
            this.VisitSecurityAttributeList(Class.SecurityAttributes, Class);
            if (Class.BaseClass != null) this.VisitReferencedType(Class.BaseClass);
            for (int i = 0, n = Class.Interfaces == null ? 0 : Class.Interfaces.Count; i < n; i++)
            {
                this.GetTypeDefOrRefOrSpecEncoded(Class.Interfaces[i]);
                if (Class.Interfaces[i] != null) this.interfaceEntries.Add(Class);
            }
            if (Class.NodeType == NodeType.ClassParameter && !(Class is MethodClassParameter))
                this.interfaceEntries.Add(Class);
            for (int i = 0, n = Class.Members.Count; i < n; i++)
            {
                Member mem = Class.Members[i];
                if (mem == null || mem is TypeNode) continue;
                this.Visit(mem);
            }
            if ((Class.Flags & (TypeFlags.ExplicitLayout | TypeFlags.SequentialLayout)) != 0 && (Class.PackingSize != 0 || Class.ClassSize != 0))
                this.classLayoutEntries.Add(Class);
        }
        void VisitConstruct(Construct/*!*/ cons)
        {
            int pops = -1;
            ExpressionList operands = cons.Operands;
            if (operands != null)
            {
                this.VisitExpressionList(cons.Operands);
                pops = operands.Count - 1;
            }
            if (pops >= 0)
                this.stackHeight -= pops;
            else
                this.IncrementStackHeight();
            this.methodBodyHeap.Write((byte)0x73);
            Method method = ((MemberBinding)cons.Constructor).BoundMember as Method;
            if (method == null) return;
            this.methodBodyHeap.Write((int)this.GetMethodToken(method)); //REVIEW: varargs?
        }
        void VisitConstructArray(ConstructArray/*!*/ consArr)
        {
            if (consArr == null || consArr.Operands == null || consArr.Operands.Count < 1) return;
            this.Visit(consArr.Operands[0]);
            this.methodBodyHeap.Write((byte)0x8d);
            this.methodBodyHeap.Write((int)this.GetTypeToken(consArr.ElementType));
        }
        void VisitDelegateNode(DelegateNode/*!*/ delegateNode)
        {
            if (this.UseGenerics && delegateNode.Template != null && delegateNode.Template.IsGeneric) return;
            this.VisitAttributeList(delegateNode.Attributes, delegateNode);
            this.VisitSecurityAttributeList(delegateNode.SecurityAttributes, delegateNode);
            this.VisitReferencedType(CoreSystemTypes.MulticastDelegate);
            for (int i = 0, n = delegateNode.Interfaces == null ? 0 : delegateNode.Interfaces.Count; i < n; i++)
            { //REVIEW: is this valid?
                this.GetTypeDefOrRefOrSpecEncoded(delegateNode.Interfaces[i]);
                if (delegateNode.Interfaces[i] != null) this.interfaceEntries.Add(delegateNode);
            }
            for (int i = 0, n = delegateNode.Members.Count; i < n; i++)
            {
                Member mem = delegateNode.Members[i];
                if (mem == null || mem is TypeNode) continue;
                this.Visit(mem);
            }
        }
        void VisitEndFilter(EndFilter/*!*/ endFilter)
        {
            this.DefineSequencePoint(endFilter);
            this.Visit(endFilter.Value);
            this.methodBodyHeap.Write((byte)0xfe);
            this.methodBodyHeap.Write((byte)0x11);
            this.stackHeight--;
        }
        void VisitEnumNode(EnumNode/*!*/ enumNode)
        {
            this.VisitAttributeList(enumNode.Attributes, enumNode);
            this.VisitSecurityAttributeList(enumNode.SecurityAttributes, enumNode);
            this.VisitReferencedType(CoreSystemTypes.Enum);
            for (int i = 0, n = enumNode.Interfaces == null ? 0 : enumNode.Interfaces.Count; i < n; i++)
            {
                this.GetTypeDefOrRefOrSpecEncoded(enumNode.Interfaces[i]);
                if (enumNode.Interfaces[i] != null) this.interfaceEntries.Add(enumNode);
            }
            for (int i = 0, n = enumNode.Members.Count; i < n; i++)
                this.Visit(enumNode.Members[i]);
        }
        void VisitEvent(Event/*!*/ Event)
        {
            object eindex = this.eventIndex[Event.UniqueKey];
            if (eindex != null) return;
            int index = this.eventEntries.Count + 1;
            this.eventEntries.Add(Event);
            this.eventIndex[Event.UniqueKey] = index;
            object evindex = this.eventMapIndex[Event.DeclaringType.UniqueKey];
            if (evindex == null)
            {
                this.eventMapEntries.Add(Event);
                this.eventMapIndex[Event.DeclaringType.UniqueKey] = this.eventMapEntries.Count;
            }
            if (Event.HandlerAdder != null) this.methodSemanticsEntries.Add(Event);
            if (Event.HandlerRemover != null) this.methodSemanticsEntries.Add(Event);
            if (Event.HandlerCaller != null) this.methodSemanticsEntries.Add(Event);
            if (Event.OtherMethods != null)
                for (int i = 0, n = Event.OtherMethods.Count; i < n; i++)
                    this.methodSemanticsEntries.Add(Event);
            this.VisitAttributeList(Event.Attributes, Event);
        }
        void VisitExpression(Expression/*!*/ expression)
        {
            switch (expression.NodeType)
            {
                case NodeType.Dup:
                    this.methodBodyHeap.Write((byte)0x25);
                    this.IncrementStackHeight();
                    return;
                case NodeType.Pop:
                    UnaryExpression unex = expression as UnaryExpression;
                    if (unex != null)
                    {
                        this.Visit(unex.Operand);
                        this.stackHeight--;
                        this.methodBodyHeap.Write((byte)0x26);
                    }
                    return;
                case NodeType.Arglist:
                    this.IncrementStackHeight();
                    this.methodBodyHeap.Write((byte)0xfe);
                    this.methodBodyHeap.Write((byte)0x00);
                    return;
            }
        }
        void VisitExpressionList(ExpressionList expressions)
        {
            if (expressions == null) return;
            for (int i = 0, n = expressions.Count; i < n; i++)
                this.Visit(expressions[i]);
        }
        void VisitExpressionStatement(ExpressionStatement/*!*/ statement)
        {
#if !MinimalReader
            if (!(statement.Expression is BlockExpression))
#endif
                this.DefineSequencePoint(statement);
            this.Visit(statement.Expression);
        }
        void VisitField(Field/*!*/ field)
        {
            this.VisitAttributeList(field.Attributes, field);
            this.GetFieldIndex(field);
            if (field.IsVolatile)
                field.Type = RequiredModifier.For(CoreSystemTypes.IsVolatile, field.Type);
            this.VisitReferencedType(field.Type);
        }
        void VisitFixupList(Fixup fixup, int targetAddress)
        {
            while (fixup != null)
            {
                this.methodBodyHeap.BaseStream.Position = fixup.fixupLocation;
                if (fixup.shortOffset)
                {
                    int offset = targetAddress - fixup.addressOfNextInstruction;
                    Debug.Assert(-128 <= offset && offset <= 127, "Invalid short branch");
                    this.methodBodyHeap.Write((byte)offset);
                }
                else
                    this.methodBodyHeap.Write((int)(targetAddress - fixup.addressOfNextInstruction));
                fixup = fixup.nextFixUp;
            }
        }
        void VisitGenericParameterList(Member/*!*/ member, TypeNodeList/*!*/ parameters)
        {
            if (member == null || parameters == null || !this.UseGenerics) return;
            int sign = member is Method ? -1 : 1;
            for (int i = 0, n = parameters.Count; i < n; i++)
            {
                TypeNode parameter = parameters[i];
                if (parameter == null) continue;
                this.typeParameterNumber[parameter.UniqueKey] = sign * (i + 1);
                this.genericParamEntries.Add(member);
                if (((ITypeParameter)parameter).DeclaringMember != member)
                    parameter = (TypeNode)parameter.Clone();
                this.genericParameters.Add(parameter);
                if (parameter.BaseType is Class && parameter.BaseType != CoreSystemTypes.Object)
                    this.genericParamConstraintEntries.Add(parameter);
                for (int j = 0, m = parameter.Interfaces == null ? 0 : parameter.Interfaces.Count; j < m; j++)
                    this.genericParamConstraintEntries.Add(parameter);
            }
        }
        void VisitIndexer(Indexer/*!*/ indexer)
        {
            this.Visit(indexer.Object);
            if (indexer.Operands == null || indexer.Operands.Count < 1) return;
            this.Visit(indexer.Operands[0]);
            byte opCode;
            switch (indexer.ElementType.typeCode)
            {
                case ElementType.Boolean:
                case ElementType.Int8: opCode = 0x90; break;
                case ElementType.UInt8: opCode = 0x91; break;
                case ElementType.Int16: opCode = 0x92; break;
                case ElementType.Char:
                case ElementType.UInt16: opCode = 0x93; break;
                case ElementType.Int32: opCode = 0x94; break;
                case ElementType.UInt32: opCode = 0x95; break;
                case ElementType.Int64:
                case ElementType.UInt64: opCode = 0x96; break;
                case ElementType.UIntPtr:
                case ElementType.IntPtr: opCode = 0x97; break;
                case ElementType.Single: opCode = 0x98; break;
                case ElementType.Double: opCode = 0x99; break;
                default:
                    if (this.UseGenerics && indexer.ElementType is ITypeParameter)
                        opCode = 0xa3;
                    else if (TypeNode.StripModifiers(indexer.ElementType) is Pointer)
                        opCode = 0x97;
                    else
                        opCode = 0x9a;
                    break;
            }
            this.methodBodyHeap.Write((byte)opCode);
            if (opCode == 0xa3) this.methodBodyHeap.Write((int)this.GetTypeToken(indexer.ElementType));
            this.stackHeight--;
        }
        void VisitInterface(Interface/*!*/ Interface)
        {
            if (this.UseGenerics && Interface.Template != null && Interface.Template.IsGeneric) return;
            this.VisitAttributeList(Interface.Attributes, Interface);
            this.VisitSecurityAttributeList(Interface.SecurityAttributes, Interface);
            InterfaceList interfaces = Interface.Interfaces;
            for (int i = 0, n = interfaces == null ? 0 : interfaces.Count; i < n; i++)
            {
                this.GetTypeDefOrRefOrSpecEncoded(interfaces[i]);
                if (interfaces[i] != null) this.interfaceEntries.Add(Interface);
            }
            if (Interface.NodeType == NodeType.TypeParameter && !(Interface is MethodTypeParameter))
                this.interfaceEntries.Add(Interface);
            for (int i = 0, n = Interface.Members.Count; i < n; i++)
            {
                Member mem = Interface.Members[i];
                if (mem == null || mem is TypeNode) continue;
                this.Visit(mem);
            }
        }
        void VisitLocal(Local/*!*/ local)
        {
            this.IncrementStackHeight();
            int li = this.GetLocalVarIndex(local);
            switch (li)
            {
                case 0: this.methodBodyHeap.Write((byte)0x06); return;
                case 1: this.methodBodyHeap.Write((byte)0x07); return;
                case 2: this.methodBodyHeap.Write((byte)0x08); return;
                case 3: this.methodBodyHeap.Write((byte)0x09); return;
                default:
                    if (li < 256)
                    {
                        this.methodBodyHeap.Write((byte)0x11);
                        this.methodBodyHeap.Write((byte)li);
                    }
                    else
                    {
                        this.methodBodyHeap.Write((byte)0xfe);
                        this.methodBodyHeap.Write((byte)0x0c);
                        this.methodBodyHeap.Write((ushort)li);
                    }
                    return;
            }
        }
#if !MinimalReader
        /// <summary>
        /// This just gets the local variable index for each local declaration.
        /// That associates the debug information with the right block because
        /// it is the block the local is declared in rather than the subblock
        /// it is first referenced in. (When different, the debugger only knows
        /// about the local when control is in the subblock.)
        /// </summary>
        /// <param name="localDeclarations">The list of locals declared at this statement</param>
        void VisitLocalDeclarationsStatement(LocalDeclarationsStatement/*!*/ localDeclarations)
        {
            if (localDeclarations == null) return;
            LocalDeclarationList decls = localDeclarations.Declarations;
            for (int i = 0, n = decls == null ? 0 : decls.Count; i < n; i++)
            {
                //^ assert decls != null;
                LocalDeclaration decl = decls[i];
                if (decl == null) continue;
                Field f = decl.Field;
                if (f == null) continue;
                //^ assume this.currentMethod != null;
                Local loc = this.currentMethod.GetLocalForField(f);
                loc.Type = localDeclarations.Type;
                this.GetLocalVarIndex(loc);
            }
        }
#endif
        void VisitLiteral(Literal/*!*/ literal)
        {
            this.IncrementStackHeight();
            IConvertible ic = literal.Value as IConvertible;
            if (ic == null)
            {
                Debug.Assert(literal.Value == null && !literal.Type.IsValueType);
                this.methodBodyHeap.Write((byte)0x14); return;
            }
            TypeCode tc = ic.GetTypeCode();
            switch (tc)
            {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                    long n = ic.ToInt64(null);
                    switch (n)
                    {
                        case -1: this.methodBodyHeap.Write((byte)0x15); break;
                        case 0: this.methodBodyHeap.Write((byte)0x16); break;
                        case 1: this.methodBodyHeap.Write((byte)0x17); break;
                        case 2: this.methodBodyHeap.Write((byte)0x18); break;
                        case 3: this.methodBodyHeap.Write((byte)0x19); break;
                        case 4: this.methodBodyHeap.Write((byte)0x1a); break;
                        case 5: this.methodBodyHeap.Write((byte)0x1b); break;
                        case 6: this.methodBodyHeap.Write((byte)0x1c); break;
                        case 7: this.methodBodyHeap.Write((byte)0x1d); break;
                        case 8: this.methodBodyHeap.Write((byte)0x1e); break;
                        default:
                            if (n >= System.SByte.MinValue && n <= System.SByte.MaxValue)
                            {
                                this.methodBodyHeap.Write((byte)0x1f);
                                this.methodBodyHeap.Write((byte)n);
                            }
                            else if (n >= System.Int32.MinValue && n <= System.Int32.MaxValue ||
                              n <= System.UInt32.MaxValue && (tc == TypeCode.Char || tc == TypeCode.UInt16 || tc == TypeCode.UInt32))
                            {
                                if (n == System.UInt32.MaxValue && tc != TypeCode.Int64)
                                    this.methodBodyHeap.Write((byte)0x15);
                                else
                                {
                                    this.methodBodyHeap.Write((byte)0x20);
                                    this.methodBodyHeap.Write((int)n);
                                }
                            }
                            else
                            {
                                this.methodBodyHeap.Write((byte)0x21);
                                this.methodBodyHeap.Write((long)n);
                                tc = TypeCode.Empty; //Suppress conversion to long
                            }
                            break;
                    }
                    if (tc == TypeCode.Int64)
                        this.methodBodyHeap.Write((byte)0x6a);
                    return;

                case TypeCode.UInt64:
                    this.methodBodyHeap.Write((byte)0x21);
                    this.methodBodyHeap.Write(ic.ToUInt64(null));
                    return;

                case TypeCode.Single:
                    this.methodBodyHeap.Write((byte)0x22);
                    this.methodBodyHeap.Write(ic.ToSingle(null));
                    return;

                case TypeCode.Double:
                    this.methodBodyHeap.Write((byte)0x23);
                    this.methodBodyHeap.Write(ic.ToDouble(null));
                    return;

                case TypeCode.String:
                    this.methodBodyHeap.Write((byte)0x72);
                    this.methodBodyHeap.Write((int)(this.GetUserStringIndex((String)literal.Value) | 0x70000000));
                    return;
            }
            Debug.Assert(false, "Unexpected literal type");
        }
        void VisitMemberBinding(MemberBinding/*!*/ memberBinding)
        {
            if (memberBinding.TargetObject != null)
            {
                this.Visit(memberBinding.TargetObject);
                if (memberBinding.Volatile)
                {
                    this.methodBodyHeap.Write((byte)0xfe);
                    this.methodBodyHeap.Write((byte)0x13);
                }
                this.methodBodyHeap.Write((byte)0x7b);
            }
            else
            {
                this.IncrementStackHeight();
                if (memberBinding.Volatile)
                {
                    this.methodBodyHeap.Write((byte)0xfe);
                    this.methodBodyHeap.Write((byte)0x13);
                }
                this.methodBodyHeap.Write((byte)0x7e);
            }
            this.methodBodyHeap.Write((int)this.GetFieldToken((Field)memberBinding.BoundMember));
            return;
        }
        void VisitMethod(Method/*!*/ method)
        {
            if (this.UseGenerics && method.Template != null && method.Template.IsGeneric) return;
            this.GetMethodIndex(method);
            this.VisitAttributeList(method.Attributes, method);
            this.VisitSecurityAttributeList(method.SecurityAttributes, method);
            for (int i = 0, n = method.Parameters == null ? 0 : method.Parameters.Count; i < n; i++)
            {
                Parameter par = method.Parameters[i];
                if (par == null) continue;
                this.VisitAttributeList(par.Attributes, par);
                this.VisitReferencedType(par.Type);
            }
            if (method.ReturnType != null)
                this.VisitReferencedType(method.ReturnType);
            if (!method.IsAbstract && method.Body != null)
            {
                if (method.Body.Statements != null && method.Body.Statements.Count > 0)
                    this.VisitMethodBody(method);
            }
            MethodList implementedInterfaceMethods = method.ImplementedInterfaceMethods;
            for (int i = 0, n = implementedInterfaceMethods == null ? 0 : implementedInterfaceMethods.Count; i < n; i++)
            {
                Method im = implementedInterfaceMethods[i];
                if (im == null) continue;
                this.methodImplEntries.Add(method);
            }
            if ((method.Flags & MethodFlags.PInvokeImpl) != 0 && method.PInvokeImportName != null && method.PInvokeModule != null)
            {
                this.implMapEntries.Add(method);
                this.GetStringIndex(method.PInvokeImportName);
                this.GetModuleRefIndex(method.PInvokeModule);
            }
        }
        void VisitMethodBody(Method/*!*/ method)
        {
            //Visit body, emitting IL bytes and gathering information
            this.methodBodyHeap = new BinaryWriter(new MemoryStream());
            this.methodInfo = new MethodInfo();
            this.currentMethod = method;
            this.stackHeightMax = 0;
            this.stackHeightExitTotal = 0;
#if !ROTOR
            if (this.symWriter != null)
            {
                this.methodInfo.debugLocals = new LocalList();
                this.methodInfo.signatureLengths = new Int32List();
                this.methodInfo.signatureOffsets = new Int32List();
                this.methodInfo.statementNodes = new NodeList();
                this.methodInfo.statementOffsets = new Int32List();
                this.symWriter.OpenMethod((uint)this.GetMethodDefToken(method));
                this.symWriter.OpenScope(0u);
#if !MinimalReader
                MethodScope scope = method.Scope;
                if (scope != null)
                {
                    UsedNamespaceList usedNamespaces = scope.UsedNamespaces;
                    for (int i = 0, n = usedNamespaces == null ? 0 : usedNamespaces.Count; i < n; i++)
                    {
                        //^ assert usedNamespaces != null;
                        UsedNamespace uns = usedNamespaces[i];
                        if (uns == null || uns.Namespace == null) continue;
                        this.symWriter.UsingNamespace(uns.Namespace.ToString());
                    }
                }
#endif
            }
#endif
#if !FxCop
            int originalAddress = 0;
            if (method.LocalList != null)
            {
                for (int i = 0, n = method.LocalList.Count; i < n; i++)
                {
                    Local loc = method.LocalList[i];
                    if (loc == null) continue;
                    this.GetLocalVarIndex(loc);
                }
#if !ROTOR
                if (this.symWriter != null)
                {
                    int currentAddress = (int)this.methodBodyHeap.BaseStream.Position;
                    originalAddress = currentAddress;
                    this.symWriter.OpenScope((uint)currentAddress);
                }
#endif
            }
#endif
            int exceptionHandlersCount = method.ExceptionHandlers == null ? 0 : method.ExceptionHandlers.Count;
            if (exceptionHandlersCount > 0)
            {
                this.exceptionBlock = new TrivialHashtable();
                for (int i = 0; i < exceptionHandlersCount; i++)
                {
                    ExceptionHandler eh = method.ExceptionHandlers[i];
                    if (eh == null || eh.HandlerStartBlock == null || (eh.HandlerType != NodeType.Catch && eh.HandlerType != NodeType.Filter)) continue;
                    this.exceptionBlock[eh.HandlerStartBlock.UniqueKey] = eh;
                }
            }
            this.VisitBlock(method.Body);

#if !FxCop
            if (method.LocalList != null)
            {
#if !ROTOR
                if (this.symWriter != null)
                {
                    DefineLocalVariables(originalAddress, method.LocalList);
                }
#endif
            }
#endif

            this.methodBodiesHeapIndex[method.UniqueKey] = (int)this.methodBodiesHeap.BaseStream.Position;
            int maxStack = this.stackHeightExitTotal + this.stackHeightMax; //Wildly pessimistic estimate. Works dandy if BBlocks never leave anything on the stack.
            if (exceptionHandlersCount > 0 && maxStack == 0) maxStack = 1;
            int codeSize = (int)this.methodBodyHeap.BaseStream.Position;
            int localVarSigTok = this.methodInfo.localVarSigTok;
            bool fatHeader = codeSize >= 64 || exceptionHandlersCount > 0 || maxStack > 8 || localVarSigTok != 0;
            if (fatHeader)
            {
                //Emit fat header
                byte header = 0x03;
                if (method.InitLocals) header |= 0x10;
                if (exceptionHandlersCount > 0) header |= 0x08;
                this.methodBodiesHeap.Write((byte)header);
                this.methodBodiesHeap.Write((byte)0x30); //top 4 bits represent length of fat header in dwords. Heaven only knows why.
                this.methodBodiesHeap.Write((short)maxStack);
                this.methodBodiesHeap.Write((int)codeSize);
                if (localVarSigTok != 0)
                {
                    if (this.methodInfo.localVarIndex.Count > 127)
                    {
                        //Need to make space for the two byte count
                        this.methodInfo.localVarSignature.Write((byte)0);
                        byte[] buf = this.methodInfo.localVarSignature.BaseStream.Buffer;
                        int n = buf.Length;
                        for (int i = n - 2; i > 1; i--) buf[i + 1] = buf[i];
                    }
                    this.methodInfo.localVarSignature.BaseStream.Position = 0;
                    this.methodInfo.localVarSignature.Write((byte)7);
                    Ir2md.WriteCompressedInt(this.methodInfo.localVarSignature, this.methodInfo.localVarIndex.Count);
                    Debug.Assert(this.methodInfo.localVarIndex.Count <= 0xFFFE);
                }
                this.methodBodiesHeap.Write((int)localVarSigTok);
            }
            else
            {
                //Emit tiny header
                this.methodBodiesHeap.Write((byte)(codeSize << 2 | 2));
            }
            //Copy body to bodies heap
            ((MemoryStream)this.methodBodyHeap.BaseStream).WriteTo(this.methodBodiesHeap.BaseStream);
            int pad = (int)this.methodBodiesHeap.BaseStream.Position;
            while (pad % 4 != 0) { pad++; this.methodBodiesHeap.Write((byte)0); }
            if (fatHeader)
            {
                //Emit exception handler entries
                int[] tryOffsets = new int[exceptionHandlersCount];
                int[] tryLengths = new int[exceptionHandlersCount];
                int[] handlerOffsets = new int[exceptionHandlersCount];
                int[] handlerLengths = new int[exceptionHandlersCount];
                bool fatFormat = false;
                for (int i = 0; i < exceptionHandlersCount; i++)
                {
                    ExceptionHandler eh = method.ExceptionHandlers[i];
                    int tryOffset = tryOffsets[i] = (int)this.methodInfo.fixupIndex[eh.TryStartBlock.UniqueKey];
                    int tryLength = tryLengths[i] = ((int)this.methodInfo.fixupIndex[eh.BlockAfterTryEnd.UniqueKey]) - tryOffset;
                    int handlerOffset = handlerOffsets[i] = (int)this.methodInfo.fixupIndex[eh.HandlerStartBlock.UniqueKey];
                    int handlerLength = handlerLengths[i] = ((int)this.methodInfo.fixupIndex[eh.BlockAfterHandlerEnd.UniqueKey]) - handlerOffset;
                    if (tryOffset > 0xffff || tryLength > 0xff || handlerOffset > 0xffff || handlerLength > 0xff) fatFormat = true;
                }
                if (exceptionHandlersCount * 12 > 0xff) fatFormat = true;
                if (fatFormat)
                {
                    int dataSize = exceptionHandlersCount * 24 + 4;
                    this.methodBodiesHeap.Write((byte)0x41);
                    this.methodBodiesHeap.Write((byte)(dataSize & 0xff));
                    this.methodBodiesHeap.Write((short)((dataSize >> 8) & 0xffff));
                }
                else
                {
                    int dataSize = exceptionHandlersCount * 12 + 4;
                    this.methodBodiesHeap.Write((byte)0x01);
                    this.methodBodiesHeap.Write((byte)dataSize);
                    this.methodBodiesHeap.Write((short)0);
                }
                for (int i = 0; i < exceptionHandlersCount; i++)
                {
                    ExceptionHandler eh = method.ExceptionHandlers[i];
                    byte flags = 0;
                    switch (eh.HandlerType)
                    {
                        case NodeType.Filter: flags = 0x0001; break;
                        case NodeType.Finally: flags = 0x0002; break;
                        case NodeType.FaultHandler: flags = 0x0004; break;
                    }
                    if (fatFormat)
                    {
                        this.methodBodiesHeap.Write((int)flags);
                        this.methodBodiesHeap.Write((int)tryOffsets[i]);
                        this.methodBodiesHeap.Write((int)tryLengths[i]);
                        this.methodBodiesHeap.Write((int)handlerOffsets[i]);
                        this.methodBodiesHeap.Write((int)handlerLengths[i]);
                    }
                    else
                    {
                        this.methodBodiesHeap.Write((short)flags);
                        this.methodBodiesHeap.Write((ushort)tryOffsets[i]);
                        this.methodBodiesHeap.Write((byte)tryLengths[i]);
                        this.methodBodiesHeap.Write((ushort)handlerOffsets[i]);
                        this.methodBodiesHeap.Write((byte)handlerLengths[i]);
                    }
                    if (eh.FilterType != null)
                        this.methodBodiesHeap.Write((int)this.GetTypeToken(eh.FilterType));
                    else if (eh.FilterExpression != null)
                        this.methodBodiesHeap.Write((int)this.methodInfo.fixupIndex[eh.FilterExpression.UniqueKey]);
                    else
                        this.methodBodiesHeap.Write((int)0);
                }
            }
#if !ROTOR
            if (this.symWriter != null)
            {
                MethodInfo mInfo = this.methodInfo;
                NodeList statementNodes = mInfo.statementNodes;
                Int32List statementOffsets = mInfo.statementOffsets;
                int n = statementNodes.Count;
                int j = 0;
                int k = 0;
                Document d = null;
                ISymUnmanagedDocumentWriter doc = null;
                for (int i = 0; i < n; i++)
                {
                    Document e = statementNodes[i].SourceContext.Document;
                    if (e == null) continue;
                    if (e != d)
                    {
                        d = e;
                        if (doc != null) this.DefineSequencePoints(statementNodes, statementOffsets, j, k, doc);
                        doc = this.GetDocumentWriter(d);
                        j = i;
                        k = 0;
                    }
                    k++;
                }
                this.DefineSequencePoints(statementNodes, statementOffsets, j, k, doc);
                this.symWriter.CloseScope((uint)this.methodBodyHeap.BaseStream.Position);
                this.symWriter.CloseMethod();
            }
#endif
            //this.methodBodyHeap = null;
            //this.methodInfo = null;
            //this.currentMethod = null;
        }

#if !ROTOR
        private void DefineLocalVariables(int startAddress, LocalList locals)
        {
            MethodInfo mInfo = this.methodInfo;
            for (int i = 0, n = locals.Count; i < n; i++)
            {
                Local loc = locals[i];
                string name = loc.Name.ToString();
                unsafe
                {
                    fixed (byte* p = mInfo.localVarSignature.BaseStream.Buffer)
                    {
                        IntPtr sp = (IntPtr)(p + mInfo.signatureOffsets[i]);
                        uint c = (uint)mInfo.signatureLengths[i];
                        this.symWriter.DefineLocalVariable(name, 0u, c, sp, 1u, (uint)this.GetLocalVarIndex(loc), 0u, 0u, 0u);
                    }
                }
            }
            int posOfFirstInstructionOfNextBlock = this.methodBodyHeap.BaseStream.Position;
            if (posOfFirstInstructionOfNextBlock > startAddress)
                this.symWriter.CloseScope((uint)(posOfFirstInstructionOfNextBlock - 1));
            else
                this.symWriter.CloseScope((uint)startAddress);
        }
#endif
        void DefineSequencePoint(Node node)
        {
#if !ROTOR
            if (this.symWriter != null && node != null && node.SourceContext.Document != null && !node.SourceContext.Document.Hidden)
            {
                this.methodInfo.statementNodes.Add(node);
                this.methodInfo.statementOffsets.Add(this.methodBodyHeap.BaseStream.Position);
            }
#endif
        }
#if !ROTOR
        void DefineSequencePoints(NodeList/*!*/ statementNodes, Int32List/*!*/ statementOffsets, int start, int count, ISymUnmanagedDocumentWriter doc)
        //^ requires this.symWriter != null;
        {
            if (count == 0) return;
            uint[] offsets = new uint[count];
            uint[] lines = new uint[count];
            uint[] columns = new uint[count];
            uint[] endLines = new uint[count];
            uint[] endColumns = new uint[count];
            for (int i = 0; i < count; i++)
            {
                Node n = statementNodes[i + start];
                offsets[i] = i + start == 0 ? 0 : (uint)statementOffsets[i + start];
                lines[i] = (uint)n.SourceContext.StartLine;
                columns[i] = (uint)n.SourceContext.StartColumn;
                endLines[i] = (uint)n.SourceContext.EndLine;
                endColumns[i] = (uint)n.SourceContext.EndColumn;
            }
            this.symWriter.DefineSequencePoints(doc, (uint)count, offsets, lines, columns, endLines, endColumns);
        }
#endif
        void VisitModule(Module/*!*/ module)
        {
            //REVIEW: check that module has no explicit lists of assembly/module references?
            this.ForceTemplateTypeMethodBodiesToGetSpecialized(module);
            this.VisitAttributeList(module.Attributes, module);
            if (this.assembly != null)
            {
                Module m = new Module();
                m.Attributes = this.assembly.ModuleAttributes;
                this.VisitAttributeList(m.Attributes, m);
                this.VisitSecurityAttributeList(this.assembly.SecurityAttributes, this.assembly);
            }
            TypeNodeList allTypes = module.Types.Clone();
            for (int k = 0; k < allTypes.Count; )
            {
                int typeCount = module.Types.Count;
                for (int i = k, n = k, m = allTypes.Count; i < (n = allTypes.Count); )
                {
                    for (; i < n; i++)
                    {
                        TypeNode t = allTypes[i];
                        if (t == null) continue;
                        if (this.UseGenerics && t.Template != null && t.Template.IsGeneric)
                        {
                            allTypes[i] = null;
                            continue;
                        }
                        this.GetTypeDefIndex(t);
                        if (i >= m) this.nestedClassEntries.Add(t);
                        MemberList members = t.Members;
                        if (members != null)
                        {
                            for (int j = 0, numMembers = members.Count; j < numMembers; j++)
                            {
                                TypeNode nt = members[j] as TypeNode;
                                if (nt != null) allTypes.Add(nt);
                            }
                        }
                    }
                }
                for (int i = k, n = allTypes.Count; i < n; i++)
                {
                    TypeNode t = allTypes[i];
                    if (t == null) continue;
                    if (this.UseGenerics && t.Template != null && t.Template.IsGeneric)
                    {
                        allTypes[i] = null;
                        continue;
                    }
                    MemberList mems = t.Members;
                    if (t is EnumNode)
                    { //Work around JIT bug in Beta2
                        for (int jj = 0, mm = mems.Count; jj < mm; jj++)
                        {
                            Field f = mems[jj] as Field;
                            if (f == null || f.IsStatic) continue;
                            mems[jj] = mems[0];
                            mems[0] = f;
                            break;
                        }
                    }
                    for (int j = 0, m = mems.Count; j < m; j++)
                    {
                        Member mem = mems[j];
                        if (mem == null) continue;
                        switch (mem.NodeType)
                        {
                            case NodeType.Field: this.GetFieldIndex((Field)mem); break;
                            case NodeType.Method:
                            case NodeType.InstanceInitializer:
                            case NodeType.StaticInitializer:
                                Method meth = (Method)mem;
                                if (this.UseGenerics && meth.Template != null && meth.Template.IsGeneric)
                                    this.GetMethodSpecIndex(meth);
                                else
                                    this.GetMethodIndex(meth);
                                break;
                        }
                    }
                }
                for (int i = k, n = allTypes.Count; i < n; i++, k++)
                {
                    TypeNode t = allTypes[i];
                    if (t == null) continue;
                    this.Visit(t);
                }
                for (int i = typeCount, n = module.Types.Count; i < n; i++)
                {
                    TypeNode t = module.Types[i];
                    if (t == null) continue;
                    Debug.Assert(t.IsNotFullySpecialized);
                    //allTypes.Add(t);
                }
            }
        }
        sealed class MethodSpecializer : StandardVisitor
        {
            private Module/*!*/ module;

            internal MethodSpecializer(Module/*!*/ module)
            {
                this.module = module;
                //^ base();
            }

            public override Method VisitMethod(Method method)
            {
                if (method == null) return null;
                if (method.Template == null || method.Template.IsGeneric) return method;
                TypeNodeList templateParameters = null;
                TypeNodeList templateArguments = null;
                if (method.TemplateArguments != null && method.TemplateArguments.Count > 0)
                {
                    templateParameters = method.Template.TemplateParameters;
                    templateArguments = method.TemplateArguments;
                }
                else
                {
                    TypeNode tdt = method.Template.DeclaringType;
                    TypeNode dt = method.DeclaringType;
                    templateParameters = tdt.ConsolidatedTemplateParameters;
                    templateArguments = dt.ConsolidatedTemplateArguments;
                    if (templateArguments == null) templateArguments = templateParameters;
                }
                if (templateParameters == null || templateParameters.Count == 0) return method;
                TypeNode declaringTemplate = method.DeclaringType == null ? null : method.DeclaringType.Template;
                bool savedNewTemplateInstanceIsRecursive = false;
                if (declaringTemplate != null)
                {
                    savedNewTemplateInstanceIsRecursive = declaringTemplate.NewTemplateInstanceIsRecursive;
                    declaringTemplate.NewTemplateInstanceIsRecursive = method.DeclaringType.IsNotFullySpecialized;
                }
                Duplicator duplicator = new Duplicator(this.module, method.DeclaringType);
#if !MinimalReader
                TypeNode closureClone = null;
                if (method.Template.Scope != null && method.Template.Scope.CapturedForClosure)
                {
                    duplicator.TypesToBeDuplicated[method.Template.Scope.ClosureClass.UniqueKey] = method.Template.Scope.ClosureClass;
                    duplicator.RecordOriginalAsTemplate = true;
                    closureClone = duplicator.VisitTypeNode(method.Template.Scope.ClosureClass);
                }
#endif
                int n = method.Parameters == null ? 0 : method.Parameters.Count;
                int m = method.Template.Parameters == null ? 0 : method.Template.Parameters.Count;
                if (n != m) { Debug.Assert(false); if (n > m) n = m; }
                for (int i = 0; i < n; i++)
                {
                    Parameter par = method.Parameters[i];
                    Parameter tpar = method.Template.Parameters[i];
                    if (par == null || tpar == null) continue;
                    duplicator.DuplicateFor[tpar.UniqueKey] = par;
                }
                n = method.TemplateParameters == null ? 0 : method.TemplateParameters.Count;
                m = method.Template.TemplateParameters == null ? 0 : method.Template.TemplateParameters.Count;
                if (n != m && n > 0) { Debug.Assert(false); if (n > m) n = m; }
                for (int i = 0; i < n; i++)
                {
                    TypeNode tpar = method.TemplateParameters[i];
                    TypeNode ttpar = method.Template.TemplateParameters[i];
                    if (tpar == null || ttpar == null) continue;
                    duplicator.DuplicateFor[ttpar.UniqueKey] = tpar;
                }
                Method dup = duplicator.VisitMethod(method.Template);
                //^ assume dup != null;
                Specializer specializer = new Specializer(this.module, templateParameters, templateArguments);
                specializer.VisitMethod(dup);
#if !MinimalReader
                if (closureClone != null)
                {
                    specializer.VisitTypeNode(closureClone);
                    if (method.TemplateArguments != null && method.TemplateArguments.Count > 0)
                        closureClone.Name = Identifier.For(closureClone.Name.ToString() + closureClone.UniqueKey);
                    MemberList dtMembers = method.DeclaringType.Members;
                    for (int i = 0, nmems = dtMembers == null ? 0 : dtMembers.Count; i < nmems; i++)
                    {
                        ClosureClass closureRef = dtMembers[i] as ClosureClass;
                        if (closureRef != null && closureRef.Name.UniqueIdKey == closureClone.Name.UniqueIdKey)
                        {
                            //This happens when the declaring type was instantiated after Normalizer has already injected a closure into the template
                            dtMembers[i] = closureClone;
                            closureClone = null;
                            break;
                        }
                    }
                    if (closureClone != null)
                        method.DeclaringType.Members.Add(closureClone);
                }
#endif
                if (method.Template.DeclaringType.DeclaringModule != this.module)
                {
                    //Dealing with imported IR that misses important type information if it contains explicit stack operations (push, pop, dup) 
                    //Call a helper visitor to remove these stack operations and in the process supply the missing type information.
                    Unstacker unstacker = new Unstacker();
                    unstacker.Visit(dup);
                }
                MethodBodySpecializer mbSpecializer = this.module.GetMethodBodySpecializer(templateParameters, templateArguments);
                mbSpecializer.methodBeingSpecialized = method;
                mbSpecializer.dummyMethod = dup;
                mbSpecializer.VisitMethod(dup);
                method.Body = dup.Body;
                // HACK to try to fix parameter declaring method back to the way it was before:
                method.Parameters = method.Parameters;
                method.ExceptionHandlers = dup.ExceptionHandlers;
                if (declaringTemplate != null)
                    declaringTemplate.NewTemplateInstanceIsRecursive = savedNewTemplateInstanceIsRecursive;
                return method;
            }
        }
        void ForceTemplateTypeMethodBodiesToGetSpecialized(Module/*!*/ module)
        {
            MethodSpecializer visitor = new MethodSpecializer(module);
            if (module == null) return;
            TypeNodeList types = module.Types;
            if (types == null) return;
            for (int i = 0; i < types.Count; i++)
                this.ForceTemplateTypeMethodBodiesToGetSpecialized(types[i], visitor);
        }
        void ForceTemplateTypeMethodBodiesToGetSpecialized(TypeNode/*!*/ type, MethodSpecializer/*!*/ visitor)
        {
            if (type == null) return;
            if (type.IsNotFullySpecialized || type.IsGeneric) return;
            bool savedNewTemplateInstanceIsRecursive = type.NewTemplateInstanceIsRecursive;
            type.NewTemplateInstanceIsRecursive = type.IsNotFullySpecialized;
            MemberList members = type.Members;
            if (members == null) return;
            for (int j = 0; j < members.Count; j++)
            {
                Member mem = members[j];
                if (mem == null) continue;
                TypeNode t = mem as TypeNode;
                if (t != null)
                    this.ForceTemplateTypeMethodBodiesToGetSpecialized(t, visitor);
                else
                    visitor.VisitMethod(mem as Method);
            }
            type.NewTemplateInstanceIsRecursive = savedNewTemplateInstanceIsRecursive;
        }
        void VisitParameter(Parameter/*!*/ parameter)
        {
            this.IncrementStackHeight();
#if !MinimalReader
            ParameterBinding pb = parameter as ParameterBinding;
            if (pb != null) parameter = pb.BoundParameter;
#endif
            int pi = parameter.ArgumentListIndex;
            switch (pi)
            {
                case 0: this.methodBodyHeap.Write((byte)0x02); return;
                case 1: this.methodBodyHeap.Write((byte)0x03); return;
                case 2: this.methodBodyHeap.Write((byte)0x04); return;
                case 3: this.methodBodyHeap.Write((byte)0x05); return;
                default:
                    if (pi < 256)
                    {
                        this.methodBodyHeap.Write((byte)0x0e);
                        this.methodBodyHeap.Write((byte)pi);
                    }
                    else
                    {
                        this.methodBodyHeap.Write((byte)0xfe);
                        this.methodBodyHeap.Write((byte)0x09);
                        this.methodBodyHeap.Write((ushort)pi);
                    }
                    return;
            }
        }
        void VisitProperty(Property/*!*/ property)
        {
            object pindex = this.propertyIndex[property.UniqueKey];
            if (pindex != null) return;
            int index = this.propertyEntries.Count + 1;
            this.propertyEntries.Add(property);
            this.propertyIndex[property.UniqueKey] = index;
            object pmindex = this.propertyMapIndex[property.DeclaringType.UniqueKey];
            if (pmindex == null)
            {
                this.propertyMapEntries.Add(property);
                this.propertyMapIndex[property.DeclaringType.UniqueKey] = this.propertyMapEntries.Count;
            }
            if (property.Getter != null) this.methodSemanticsEntries.Add(property);
            if (property.Setter != null) this.methodSemanticsEntries.Add(property);
            if (property.OtherMethods != null)
                for (int i = 0, n = property.OtherMethods.Count; i < n; i++)
                    this.methodSemanticsEntries.Add(property);
            this.VisitAttributeList(property.Attributes, property);
        }
        void VisitReferencedType(TypeNode type)
        {
            if (type == null) return;
            if (type.IsGeneric && type.Template == null)
            {
                TypeNodeList templParams = type.ConsolidatedTemplateParameters;
                for (int i = 0, n = templParams == null ? 0 : templParams.Count; i < n; i++)
                    this.typeParameterNumber[templParams[i].UniqueKey] = i + 1;
            }
            switch (type.typeCode)
            {
                case ElementType.Pointer: this.VisitReferencedType(((Pointer)type).ElementType); return;
                case ElementType.Reference: this.VisitReferencedType(((Reference)type).ElementType); return;
                case ElementType.Array:
                case ElementType.SzArray: this.VisitReferencedType(((ArrayType)type).ElementType); return;
                case ElementType.OptionalModifier:
                case ElementType.RequiredModifier:
                    TypeModifier tm = (TypeModifier)type;
                    this.VisitReferencedType(tm.Modifier);
                    this.VisitReferencedType(tm.ModifiedType);
                    return;
                case ElementType.FunctionPointer:
                    FunctionPointer fp = (FunctionPointer)type;
                    this.VisitReferencedType(fp.ReturnType);
                    for (int i = 0, n = fp.ParameterTypes == null ? 0 : fp.ParameterTypes.Count; i < n; i++)
                        this.VisitReferencedType(fp.ParameterTypes[i]);
                    return;
                case ElementType.ValueType:
                case ElementType.Class:
                    break;
                default:
                    return;
            }
            if (this.IsStructural(type))
                this.GetTypeSpecIndex(type);
            else if (type.DeclaringModule == this.module)
                this.GetTypeDefIndex(type);
            else if (type.DeclaringModule != null)
                this.GetTypeRefIndex(type);
            else if (type.typeCode == ElementType.ValueType || type.typeCode == ElementType.Class)
            {
                //Get here for type parameters
                if (this.UseGenerics && this.typeParameterNumber[type.UniqueKey] != null) return;
                type.DeclaringModule = this.module;
                this.GetTypeDefIndex(type);
            }
            else
                Debug.Assert(false);
        }
        void VisitReturn(Return/*!*/ Return)
        {
            this.DefineSequencePoint(Return);
            if (Return.Expression != null)
            {
                this.Visit(Return.Expression);
                this.stackHeight--;
            }
            this.methodBodyHeap.Write((byte)0x2a);
        }
        void VisitSecurityAttributeList(SecurityAttributeList attrs, Node/*!*/ node)
        {
            if (attrs == null) return;
            int n = attrs.Count;
            if (n == 0) return;
            int m = n;
            for (int j = 0; j < n; j++)
            {
                SecurityAttribute a = attrs[j];
                if (a == null) m--;
            }
            if (m == 0) return;
            n = m;
            int codedIndex = this.GetSecurityAttributeParentCodedIndex(node);
            this.securityAttributeCount += n;
            m = this.nodesWithSecurityAttributes.Count;
            this.nodesWithSecurityAttributes.Add(node);
            int i = 0; //after the for loop i will be position where the new node should be in sorted list
            NodeList nodes = this.nodesWithSecurityAttributes;
            for (i = m; i > 0; i--)
            {
                Node other = nodes[i - 1];
                int oci = this.GetSecurityAttributeParentCodedIndex(other);
                if (oci < codedIndex) break;
            }
            if (i == m) return; //node is already where it should be
            for (int j = m; j > i; j--) nodes[j] = nodes[j - 1]; //Make space at postion i
            nodes[i] = node;
        }
        void VisitStatement(Statement/*!*/ statement)
        {
            this.DefineSequencePoint(statement);
            switch (statement.NodeType)
            {
                case NodeType.Nop: this.methodBodyHeap.Write((byte)0x00); break;
                case NodeType.DebugBreak: this.methodBodyHeap.Write((byte)0x01); break;
                case NodeType.EndFinally: this.methodBodyHeap.Write((byte)0xdc); break;
            }
        }
        void VisitStruct(Struct/*!*/ Struct)
        {
            if (this.UseGenerics && Struct.Template != null && Struct.Template.IsGeneric) return;
            this.VisitAttributeList(Struct.Attributes, Struct);
            this.VisitSecurityAttributeList(Struct.SecurityAttributes, Struct);
            this.VisitReferencedType(CoreSystemTypes.ValueType);
            InterfaceList interfaces = Struct.Interfaces;
            for (int i = 0, n = interfaces == null ? 0 : interfaces.Count; i < n; i++)
            {
                this.GetTypeDefOrRefOrSpecEncoded(interfaces[i]);
                if (interfaces[i] != null) this.interfaceEntries.Add(Struct);
            }
            for (int i = 0, n = Struct.Members.Count; i < n; i++)
            {
                Member m = Struct.Members[i];
                if (m is TypeNode) continue;
                this.Visit(m);
            }
            if ((Struct.Flags & (TypeFlags.ExplicitLayout | TypeFlags.SequentialLayout)) != 0 && (Struct.PackingSize != 0 || Struct.ClassSize != 0))
                this.classLayoutEntries.Add(Struct);
        }
        void VisitSwitchInstruction(SwitchInstruction/*!*/ switchInstruction)
        {
            this.Visit(switchInstruction.Expression);
            this.stackHeight--;
            BlockList targets = switchInstruction.Targets;
            int n = targets != null ? targets.Count : 0;
            int addressOfNextInstruction = ((int)this.methodBodyHeap.BaseStream.Position) + 5 + 4 * n;
            this.methodBodyHeap.Write((byte)0x45);
            this.methodBodyHeap.Write((uint)n);
            for (int i = 0; i < n; i++)
                this.methodBodyHeap.Write((int)this.GetOffset(targets[i], addressOfNextInstruction));
        }
        void VisitTernaryExpression(TernaryExpression/*!*/ expression)
        {
            this.Visit(expression.Operand1);
            this.Visit(expression.Operand2);
            this.Visit(expression.Operand3);
            this.methodBodyHeap.Write((byte)0xfe);
            if (expression.NodeType == NodeType.Cpblk)
                this.methodBodyHeap.Write((byte)0x17);
            else
                this.methodBodyHeap.Write((byte)0x18);
            this.stackHeight -= 3;
        }
        void VisitThis(This/*!*/ This)
        {
            this.IncrementStackHeight();
            this.methodBodyHeap.Write((byte)0x02);
        }
        void VisitThrow(Throw/*!*/ Throw)
        {
            this.DefineSequencePoint(Throw);
            if (Throw.NodeType == NodeType.Rethrow)
            {
                this.methodBodyHeap.Write((byte)0xfe);
                this.methodBodyHeap.Write((byte)0x1a);
            }
            else
            {
                this.Visit(Throw.Expression);
                this.methodBodyHeap.Write((byte)0x7a);
            }
            this.stackHeight--;
        }
        void VisitUnaryExpression(UnaryExpression/*!*/ unaryExpression)
        {
            switch (unaryExpression.NodeType)
            {
                case NodeType.Ldtoken:
                    this.methodBodyHeap.Write((byte)0xd0);
                    Literal lit = unaryExpression.Operand as Literal;
                    if (lit != null)
                    {
                        if (lit.Value == null) return;
                        this.methodBodyHeap.Write((int)this.GetTypeDefToken((TypeNode)lit.Value));
                    }
                    else
                    {
                        if (unaryExpression.Operand == null) return;
                        Member m = ((MemberBinding)unaryExpression.Operand).BoundMember;
                        if (m == null) return;
                        Method meth = m as Method;
                        if (meth != null)
                            this.methodBodyHeap.Write((int)this.GetMethodToken(meth));
                        else
                            this.methodBodyHeap.Write((int)this.GetFieldToken((Field)m));
                    }
                    this.IncrementStackHeight();
                    return;

                case NodeType.Ldftn:
                    this.methodBodyHeap.Write((byte)0xfe);
                    this.methodBodyHeap.Write((byte)0x06);
                    this.methodBodyHeap.Write((int)this.GetMethodToken((Method)((MemberBinding)unaryExpression.Operand).BoundMember));
                    this.IncrementStackHeight();
                    return;

                case NodeType.Sizeof:
                    this.methodBodyHeap.Write((byte)0xfe);
                    this.methodBodyHeap.Write((byte)0x1c);
                    this.methodBodyHeap.Write((int)this.GetTypeToken((TypeNode)((Literal)unaryExpression.Operand).Value));
                    this.IncrementStackHeight();
                    return;

                case NodeType.SkipCheck:
                    this.methodBodyHeap.Write((byte)0xfe);
                    this.methodBodyHeap.Write((byte)0x19);
                    switch (unaryExpression.Operand.NodeType)
                    {
                        case NodeType.Castclass:
                        case NodeType.Unbox:
                            this.methodBodyHeap.Write((byte)0x01);
                            break;
                        default:
                            Debug.Assert(false);
                            this.methodBodyHeap.Write((byte)0x00);
                            break;
                    }
                    this.VisitExpression(unaryExpression.Operand);
                    return;
            }
            this.Visit(unaryExpression.Operand);
            byte opCode = 0;
            switch (unaryExpression.NodeType)
            {
                case NodeType.Neg: opCode = 0x65; break;
                case NodeType.Not: opCode = 0x66; break;
                case NodeType.Conv_I1: opCode = 0x67; break;
                case NodeType.Conv_I2: opCode = 0x68; break;
                case NodeType.Conv_I4: opCode = 0x69; break;
                case NodeType.Conv_I8: opCode = 0x6a; break;
                case NodeType.Conv_R4: opCode = 0x6b; break;
                case NodeType.Conv_R8: opCode = 0x6c; break;
                case NodeType.Conv_U4: opCode = 0x6d; break;
                case NodeType.Conv_U8: opCode = 0x6e; break;
                case NodeType.Conv_R_Un: opCode = 0x76; break;
                case NodeType.Conv_Ovf_I1_Un: opCode = 0x82; break;
                case NodeType.Conv_Ovf_I2_Un: opCode = 0x83; break;
                case NodeType.Conv_Ovf_I4_Un: opCode = 0x84; break;
                case NodeType.Conv_Ovf_I8_Un: opCode = 0x85; break;
                case NodeType.Conv_Ovf_U1_Un: opCode = 0x86; break;
                case NodeType.Conv_Ovf_U2_Un: opCode = 0x87; break;
                case NodeType.Conv_Ovf_U4_Un: opCode = 0x88; break;
                case NodeType.Conv_Ovf_U8_Un: opCode = 0x89; break;
                case NodeType.Conv_Ovf_I_Un: opCode = 0x8a; break;
                case NodeType.Conv_Ovf_U_Un: opCode = 0x8b; break;
                case NodeType.Ldlen: opCode = 0x8e; break;
                case NodeType.Conv_Ovf_I1: opCode = 0xb3; break;
                case NodeType.Conv_Ovf_U1: opCode = 0xb4; break;
                case NodeType.Conv_Ovf_I2: opCode = 0xb5; break;
                case NodeType.Conv_Ovf_U2: opCode = 0xb6; break;
                case NodeType.Conv_Ovf_I4: opCode = 0xb7; break;
                case NodeType.Conv_Ovf_U4: opCode = 0xb8; break;
                case NodeType.Conv_Ovf_I8: opCode = 0xb9; break;
                case NodeType.Conv_Ovf_U8: opCode = 0xba; break;
                case NodeType.Ckfinite: opCode = 0xc3; break;
                case NodeType.Conv_U2: opCode = 0xd1; break;
                case NodeType.Conv_U1: opCode = 0xd2; break;
                case NodeType.Conv_I: opCode = 0xd3; break;
                case NodeType.Conv_Ovf_I: opCode = 0xd4; break;
                case NodeType.Conv_Ovf_U: opCode = 0xd5; break;
                case NodeType.Conv_U: opCode = 0xe0; break;
                case NodeType.Localloc: opCode = 0x0f; this.methodBodyHeap.Write((byte)0xfe); break;
                case NodeType.Refanytype: opCode = 0x1d; this.methodBodyHeap.Write((byte)0xfe); break;
            }
            this.methodBodyHeap.Write((byte)opCode);
        }
        static void WriteArrayShape(BinaryWriter/*!*/ target, ArrayType/*!*/ arrayType)
        {
            Ir2md.WriteCompressedInt(target, arrayType.Rank);
            int n = arrayType.Sizes == null ? 0 : arrayType.Sizes.Length;
            Ir2md.WriteCompressedInt(target, n);
            for (int i = 0; i < n; i++)
            {
                //^ assert arrayType.Sizes != null;
                Ir2md.WriteCompressedInt(target, arrayType.Sizes[i]);
            }
            n = arrayType.LowerBounds == null ? 0 : arrayType.LowerBounds.Length;
            Ir2md.WriteCompressedInt(target, n);
            for (int i = 0; i < n; i++)
            {
                //^ assert arrayType.LowerBounds != null;
                Ir2md.WriteCompressedInt(target, arrayType.LowerBounds[i]);
            }
        }
        internal static void WriteCompressedInt(BinaryWriter/*!*/ target, int val)
        {
            if (val <= 0x7f)
                target.Write((byte)val);
            else if (val < 0x3fff)
            {
                target.Write((byte)((val >> 8) | 0x80));
                target.Write((byte)(val & 0xff));
            }
            else if (val < 0x1fffffff)
            {
                target.Write((byte)((val >> 24) | 0xc0));
                target.Write((byte)((val & 0xff0000) >> 16));
                target.Write((byte)((val & 0xff00) >> 8));
                target.Write((byte)(val & 0xff));
            }
            else
                Debug.Assert(false, "index too large for compression");
        }
        TypeNode/*!*/ WriteCustomModifiers(BinaryWriter/*!*/ target, TypeNode/*!*/ type)
        {
            switch (type.NodeType)
            {
                case NodeType.RequiredModifier:
                case NodeType.OptionalModifier:
                    TypeModifier tm = (TypeModifier)type;
                    target.Write((byte)tm.typeCode);
                    this.WriteTypeDefOrRefEncoded(target, tm.Modifier);
                    return this.WriteCustomModifiers(target, tm.ModifiedType);
            }
            return type;
        }
        void WriteCustomAttributeLiteral(BinaryWriter/*!*/ writer, Literal/*!*/ literal, bool needsTag)
        {
            if (literal.Type == null) return;
            ElementType typeCode = literal.Type.typeCode;
            if (needsTag)
            {
                if (typeCode == ElementType.ValueType)
                { //Boxed enum
                    writer.Write((byte)0x55);
                    this.WriteSerializedTypeName(writer, literal.Type);
                }
                else if (typeCode == ElementType.Class)
                { //a Type value
                    writer.Write((byte)0x50);
                }
                else if (typeCode != ElementType.Object) //a primitive
                    writer.Write((byte)typeCode);
            }
            Object value = literal.Value;
            //if (value == null) return; //TODO: nope, find some other way
            switch (typeCode)
            {
                case ElementType.Boolean: writer.Write((bool)value); return;
                case ElementType.Char: writer.Write((ushort)(char)value); return;
                case ElementType.Double: writer.Write((double)value); return;
                case ElementType.Single: writer.Write((float)value); return;
                case ElementType.Int16: writer.Write((short)value); return;
                case ElementType.Int32: writer.Write((int)value); return;
                case ElementType.Int64: writer.Write((long)value); return;
                case ElementType.Int8: writer.Write((sbyte)value); return;
                case ElementType.UInt16: writer.Write((ushort)value); return;
                case ElementType.UInt32: writer.Write((uint)value); return;
                case ElementType.UInt64: writer.Write((ulong)value); return;
                case ElementType.UInt8: writer.Write((byte)value); return;
                case ElementType.String: writer.Write((string)value, false); return;
                case ElementType.ValueType: this.WriteCustomAttributeLiteral(writer, new Literal(value, ((EnumNode)literal.Type).UnderlyingType), false); return;
                case ElementType.Class: this.WriteSerializedTypeName(writer, (TypeNode)value); return;
                case ElementType.SzArray:
                    TypeNode elemType = ((ArrayType)literal.Type).ElementType;
                    if (needsTag)
                        writer.Write((byte)elemType.typeCode);
                    Array array = (Array)value;
                    int numElems = array == null ? -1 : array.Length;
                    writer.Write((int)numElems);
                    for (int i = 0; i < numElems; i++)
                        this.WriteCustomAttributeLiteral(writer, new Literal(array.GetValue(i), elemType), false);
                    return;
                case ElementType.Object:
                    Literal lit = (Literal)literal.Clone();
                    TypeNode t = null;
                    switch (Convert.GetTypeCode(lit.Value))
                    {
                        case TypeCode.Boolean: t = CoreSystemTypes.Boolean; break;
                        case TypeCode.Byte: t = CoreSystemTypes.UInt8; break;
                        case TypeCode.Char: t = CoreSystemTypes.Char; break;
                        case TypeCode.Double: t = CoreSystemTypes.Double; break;
                        case TypeCode.Int16: t = CoreSystemTypes.Int16; break;
                        case TypeCode.Int32: t = CoreSystemTypes.Int32; break;
                        case TypeCode.Int64: t = CoreSystemTypes.Int64; break;
                        case TypeCode.SByte: t = CoreSystemTypes.Int8; break;
                        case TypeCode.Single: t = CoreSystemTypes.Single; break;
                        case TypeCode.String: t = CoreSystemTypes.String; break;
                        case TypeCode.UInt16: t = CoreSystemTypes.UInt16; break;
                        case TypeCode.UInt32: t = CoreSystemTypes.UInt32; break;
                        case TypeCode.UInt64: t = CoreSystemTypes.UInt64; break;
                        case TypeCode.Empty:
                        case TypeCode.Object:
                            Array arr = lit.Value as Array;
                            if (arr != null)
                            {
#if !NoReflection
                                t = TypeNode.GetTypeNode(arr.GetType());
#else
                System.Type reflType = arr.GetType();
                System.Type reflElemType = reflType.GetElementType();
                AssemblyNode assem = AssemblyNode.GetAssembly(reflType.Assembly.Location);
                TypeNode cciElemType = assem.GetType(Identifier.For(reflElemType.Namespace), Identifier.For(reflElemType.Name));
                t = cciElemType.GetArrayType(reflType.GetArrayRank());
#endif
                            }
                            else
                                t = CoreSystemTypes.Type;
                            break;
                    }
                    if (t == null) break;
                    lit.Type = t;
                    this.WriteCustomAttributeLiteral(writer, lit, true);
                    return;
            }
            Debug.Assert(false, "Unexpected type in custom attribute");
        }
        bool AttributesContains(AttributeList al, TypeNode/*!*/ a)
        {
            if (al == null) return false;
            for (int i = 0, n = al.Count; i < n; i++)
            {
                if (al[i] != null && al[i].Type == a)
                    return true;
            }
            return false;
        }
        void WriteMethodSignature(BinaryWriter/*!*/ target, Method/*!*/ method)
        {
            if (this.UseGenerics)
            {
                if (method.Template != null && method.Template.IsGeneric)
                {
                    //Signature is being used in MethodDef table
                    TypeNodeList types = method.TemplateArguments;
                    int m = types == null ? 0 : types.Count;
                    target.Write((byte)(method.CallingConvention | CallingConventionFlags.Generic));
                    Ir2md.WriteCompressedInt(target, m);
                }
                else if (method.DeclaringType.Template != null && method.DeclaringType.Template.IsGeneric)
                {
                    Method unspecializedMethod = this.GetUnspecializedMethod(method);
                    this.WriteMethodSignature(target, unspecializedMethod);
                    return;
                }
                else if (method.IsGeneric)
                {
                    TypeNodeList types = method.TemplateParameters;
                    int m = types == null ? 0 : types.Count;
                    target.Write((byte)(method.CallingConvention | CallingConventionFlags.Generic));
                    Ir2md.WriteCompressedInt(target, m);
                }
                else
                    target.Write((byte)method.CallingConvention);
            }
            else
                target.Write((byte)method.CallingConvention);
            ParameterList pars = method.Parameters;
            int n = pars == null ? 0 : pars.Count;
            Ir2md.WriteCompressedInt(target, n);

            TypeNode returnType = method.ReturnType;
#if ExtendedRuntime
      if (method.HasOutOfBandContract || this.AttributesContains(method.ReturnAttributes, SystemTypes.NotNullAttribute)) {
        returnType = TypeNode.DeepStripModifiers(returnType, (method.Template != null) ? method.Template.ReturnType : null, SystemTypes.NonNullType, SystemTypes.NullableType);
    //    returnType = TypeNode.DeepStripModifier(returnType, SystemTypes.NullableType, (method.Template != null) ? returnType.GetTemplateInstance(returnType, returnType.TemplateArguments) : null);
      }
#endif
            if (returnType == null) returnType = SystemTypes.Object;
            this.WriteTypeSignature(target, returnType, true);
            for (int i = 0; i < n; i++)
            {
                Parameter p = pars[i];
                if (p == null) continue;
                TypeNode parameterType = p.Type;
#if ExtendedRuntime
        if (method.HasOutOfBandContract || this.AttributesContains(p.Attributes, SystemTypes.NotNullAttribute)) {
          parameterType = TypeNode.DeepStripModifiers(parameterType, (method.Template != null) ? method.Template.Parameters[i].Type : null, SystemTypes.NonNullType, SystemTypes.NullableType);
          //parameterType = TypeNode.DeepStripModifier(parameterType, SystemTypes.NullableType, (method.Template != null) ? parameterType.GetTemplateInstance(parameterType, parameterType.TemplateArguments) : null);
        }
#endif
                if (parameterType == null) parameterType = SystemTypes.Object;
                this.WriteTypeSignature(target, parameterType);
            }
        }
        void WriteMethodSpecSignature(BinaryWriter/*!*/ target, Method/*!*/ method)
        //^ requires this.UseGenerics && method.Template != null && method.Template.IsGeneric;
        {
            Debug.Assert(this.UseGenerics && method.Template != null && method.Template.IsGeneric);
            target.Write((byte)0x0a);
            TypeNodeList types = method.TemplateArguments;
            int m = types == null ? 0 : types.Count;
            Ir2md.WriteCompressedInt(target, m);
            for (int i = 0; i < m; i++)
            {
                //^ assert types != null;
                this.WriteTypeSignature(target, types[i]);
            }
        }
        void WriteMethodSignature(BinaryWriter/*!*/ target, FunctionPointer/*!*/ fp)
        {
            target.Write((byte)fp.CallingConvention);
            TypeNodeList parTypes = fp.ParameterTypes;
            int n = parTypes == null ? 0 : parTypes.Count;
            Ir2md.WriteCompressedInt(target, n);
            if (fp.ReturnType != null)
                this.WriteTypeSignature(target, fp.ReturnType);
            int m = fp.VarArgStart;
            for (int i = 0; i < n; i++)
            {
                //^ assert parTypes != null;
                if (i == m) target.Write((byte)0x41); //Sentinel
                this.WriteTypeSignature(target, parTypes[i]);
            }
        }
        void WritePropertySignature(BinaryWriter/*!*/ target, Property/*!*/ prop)
        {
            byte propHeader = (byte)0x8;
            if (!prop.IsStatic) propHeader |= (byte)0x20; //bizarre redundant way to indicate that property accessors are instance methods
            target.Write(propHeader);
            ParameterList pars = prop.Parameters;
            int n = pars == null ? 0 : pars.Count;
            Ir2md.WriteCompressedInt(target, n);
            if (prop.Type != null) this.WriteTypeSignature(target, prop.Type);
            for (int i = 0; i < n; i++)
            {
                //^ assert pars != null;
                Parameter par = pars[i];
                if (par == null || par.Type == null) continue;
                this.WriteTypeSignature(target, par.Type);
            }
        }
        void WriteSerializedTypeName(BinaryWriter target, TypeNode type)
        {
            if (target == null || type == null) return;
            target.Write(this.GetSerializedTypeName(type), false);
        }
        string GetSerializedTypeName(TypeNode/*!*/ type)
        {
            bool isAssemblyQualified = true;
            return this.GetSerializedTypeName(type, ref isAssemblyQualified);
        }
        string GetSerializedTypeName(TypeNode/*!*/ type, ref bool isAssemblyQualified)
        {
            if (type == null) return null;
            this.VisitReferencedType(type);
            StringBuilder sb = new StringBuilder();
            TypeModifier tMod = type as TypeModifier;
            if (tMod != null)
            {
                sb.Append(this.GetTypeDefOrRefOrSpecEncoded(type));
                sb.Append('!');
                return sb.ToString();
            }
            ArrayType arrType = type as ArrayType;
            if (arrType != null)
            {
                type = arrType.ElementType;
                bool isAssemQual = false;
                this.AppendSerializedTypeName(sb, arrType.ElementType, ref isAssemQual);
                if (arrType.IsSzArray())
                    sb.Append("[]");
                else
                {
                    sb.Append('[');
                    if (arrType.Rank == 1) sb.Append('*');
                    for (int i = 1; i < arrType.Rank; i++) sb.Append(',');
                    sb.Append(']');
                }
                goto done;
            }
            Pointer pointer = type as Pointer;
            if (pointer != null)
            {
                type = pointer.ElementType;
                bool isAssemQual = false;
                this.AppendSerializedTypeName(sb, pointer.ElementType, ref isAssemQual);
                sb.Append('*');
                goto done;
            }
            Reference reference = type as Reference;
            if (reference != null)
            {
                type = reference.ElementType;
                bool isAssemQual = false;
                this.AppendSerializedTypeName(sb, reference.ElementType, ref isAssemQual);
                sb.Append('&');
                goto done;
            }
            if (type.Template == null)
                sb.Append(type.FullName);
            else
            {
                sb.Append(type.Template.FullName);
                sb.Append('[');
                for (int i = 0, n = type.TemplateArguments == null ? 0 : type.TemplateArguments.Count; i < n; i++)
                {
                    //^ assert type.TemplateArguments != null;
                    bool isAssemQual = true;
                    this.AppendSerializedTypeName(sb, type.TemplateArguments[i], ref isAssemQual);
                    if (i < n - 1) sb.Append(',');
                }
                sb.Append(']');
            }
        done:
            if (isAssemblyQualified)
                this.AppendAssemblyQualifierIfNecessary(sb, type, out isAssemblyQualified);
            return sb.ToString();
        }
        void AppendAssemblyQualifierIfNecessary(StringBuilder/*!*/ sb, TypeNode type, out bool isAssemQualified)
        {
            isAssemQualified = false;
            if (type == null) return;
            AssemblyNode referencedAssembly = type.DeclaringModule as AssemblyNode;
            if (referencedAssembly != null && referencedAssembly != this.module /*&& referencedAssembly != CoreSystemTypes.SystemAssembly*/)
            {
                sb.Append(", ");
                sb.Append(referencedAssembly.StrongName);
                isAssemQualified = true;
            }
        }
        void AppendSerializedTypeName(StringBuilder/*!*/ sb, TypeNode type, ref bool isAssemQualified)
        {
            if (type == null) return;
            string argTypeName = this.GetSerializedTypeName(type, ref isAssemQualified);
            if (isAssemQualified) sb.Append('[');
            sb.Append(argTypeName);
            if (isAssemQualified) sb.Append(']');
        }
        void WriteTypeDefOrRefEncoded(BinaryWriter/*!*/ target, TypeNode/*!*/ type)
        {
            if (!type.IsGeneric && this.IsStructural(type) && !(type is ITypeParameter))
                this.WriteTypeSpecEncoded(target, type);
            else if (type.DeclaringModule == this.module)
                this.WriteTypeDefEncoded(target, type);
            else if (type.DeclaringModule != null)
                this.WriteTypeRefEncoded(target, type);
            else
                Debug.Assert(false);
        }
        void WriteTypeDefEncoded(BinaryWriter/*!*/ target, TypeNode/*!*/ type)
        {
            int tok = this.GetTypeDefIndex(type);
            Ir2md.WriteCompressedInt(target, (tok << 2));
        }
        void WriteTypeRefEncoded(BinaryWriter/*!*/ target, TypeNode/*!*/ type)
        {
            int tok = this.GetTypeRefIndex(type);
            Ir2md.WriteCompressedInt(target, (tok << 2) | 1);
        }
        void WriteTypeSpecEncoded(BinaryWriter/*!*/ target, TypeNode/*!*/ type)
        {
            int tok = this.GetTypeSpecIndex(type);
            Ir2md.WriteCompressedInt(target, (tok << 2) | 2);
        }
        void WriteTypeSignature(BinaryWriter/*!*/ target, TypeNode/*!*/ type)
        {
            this.WriteTypeSignature(target, type, false);
        }
        void WriteTypeSignature(BinaryWriter/*!*/ target, TypeNode/*!*/ type, bool instantiateGenericTypes)
        {
            if (type == null) return;
            TypeNode t = this.WriteCustomModifiers(target, type);
            if (this.UseGenerics)
            {
                if (t.Template != null && t.Template.IsGeneric && t.TemplateParameters == null)
                {
                    target.Write((byte)0x15);
                    TypeNode template = t.Template;
                    while (template.Template != null) template = template.Template;
                    this.WriteTypeSignature(target, template);
                    TypeNodeList templArgs = t.ConsolidatedTemplateArguments;
                    int n = templArgs == null ? 0 : templArgs.Count;
                    Ir2md.WriteCompressedInt(target, n);
                    for (int i = 0; i < n; i++)
                    {
                        //^ assume templArgs != null;
                        TypeNode targ = templArgs[i];
                        if (targ == null) continue;
                        this.WriteTypeSignature(target, targ);
                    }
                    return;
                }
                else if (t.IsGeneric && instantiateGenericTypes)
                {
                    while (t.Template != null) t = t.Template;
                    target.Write((byte)0x15);
                    this.WriteTypeSignature(target, t);
                    TypeNodeList templPars = t.ConsolidatedTemplateParameters;
                    int n = templPars == null ? 0 : templPars.Count;
                    Ir2md.WriteCompressedInt(target, n);
                    for (int i = 0; i < n; i++)
                    {
                        //^ assume templPars != null;
                        TypeNode tp = templPars[i];
                        if (tp == null) continue;
                        this.WriteTypeSignature(target, tp);
                    }
                    return;
                }
                if (t is ITypeParameter)
                {
                    object num = this.typeParameterNumber[t.UniqueKey];
                    if (num is int)
                    {
                        int number = (int)num;
                        if (number < 0)
                        {
                            target.Write((byte)0x1e); number = -number;
                        }
                        else
                            target.Write((byte)0x13);
                        Ir2md.WriteCompressedInt(target, number - 1);
                        return;
                    }
                }
            }
            target.Write((byte)t.typeCode);
            switch (t.typeCode)
            {
                case ElementType.Pointer: this.WriteTypeSignature(target, ((Pointer)t).ElementType); break;
                case ElementType.Reference: this.WriteTypeSignature(target, ((Reference)t).ElementType); break;
                case ElementType.ValueType:
                case ElementType.Class: this.WriteTypeDefOrRefEncoded(target, t); break;
                case ElementType.Array: this.WriteTypeSignature(target, ((ArrayType)t).ElementType); Ir2md.WriteArrayShape(target, (ArrayType)t); break;
                case ElementType.FunctionPointer: this.WriteMethodSignature(target, (FunctionPointer)t); break;
                case ElementType.SzArray: this.WriteTypeSignature(target, ((ArrayType)t).ElementType); break;
            }
        }

#if !ROTOR
        void IMetaDataEmit.SetModuleProps(string szName)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.Save(string szFile, uint dwSaveFlags)
        {
            throw new NotImplementedException();
        }
        unsafe void IMetaDataEmit.SaveToStream(void* pIStream, uint dwSaveFlags)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataEmit.GetSaveSize(uint fSave)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineTypeDef(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineNestedType(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements, uint tdEncloser)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.SetHandler([MarshalAs(UnmanagedType.IUnknown), In]object pUnk)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineMethod(uint td, char* zName, uint dwMethodFlags, byte* pvSigBlob, uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.DefineMethodImpl(uint td, uint tkBody, uint tkDecl)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineTypeRefByName(uint tkResolutionScope, char* szName)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineImportType(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue, IMetaDataImport pImport,
          uint tdImport, IntPtr pAssemEmit)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineMemberRef(uint tkImport, string szName, byte* pvSigBlob, uint cbSigBlob)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineImportMember(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue,
          IMetaDataImport pImport, uint mbMember, IntPtr pAssemEmit, uint tkParent)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineEvent(uint td, string szEvent, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods)
        {
            throw new NotImplementedException();
        }
        unsafe void IMetaDataEmit.SetClassLayout(uint td, uint dwPackSize, COR_FIELD_OFFSET* rFieldOffsets, uint ulClassSize)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.DeleteClassLayout(uint td)
        {
            throw new NotImplementedException();
        }
        unsafe void IMetaDataEmit.SetFieldMarshal(uint tk, byte* pvNativeType, uint cbNativeType)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.DeleteFieldMarshal(uint tk)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefinePermissionSet(uint tk, uint dwAction, void* pvPermission, uint cbPermission)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.SetRVA(uint md, uint ulRVA)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.GetTokenFromSig(byte* pvSig, uint cbSig)
        {
            BinaryWriter sig = new BinaryWriter(new MemoryStream());
            for (int i = 0; i < cbSig; i++) sig.Write(*(pvSig + i));
            return (uint)(0x11000000 | this.GetStandAloneSignatureIndex(sig));
        }
        uint IMetaDataEmit.DefineModuleRef(string szName)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.SetParent(uint mr, uint tk)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.GetTokenFromTypeSpec(byte* pvSig, uint cbSig)
        {
            throw new NotImplementedException();
        }
        unsafe void IMetaDataEmit.SaveToMemory(void* pbData, uint cbData)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataEmit.DefineUserString(string szString, uint cchString)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.DeleteToken(uint tkObj)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.SetMethodProps(uint md, uint dwMethodFlags, uint ulCodeRVA, uint dwImplFlags)
        {
            throw new NotImplementedException();
        }
        unsafe void IMetaDataEmit.SetTypeDefProps(uint td, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements)
        {
            throw new NotImplementedException();
        }
        unsafe void IMetaDataEmit.SetEventProps(uint ev, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.SetPermissionSetProps(uint tk, uint dwAction, void* pvPermission, uint cbPermission)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.DefinePinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.SetPinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.DeletePinvokeMap(uint tk)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineCustomAttribute(uint tkObj, uint tkType, void* pCustomAttribute, uint cbCustomAttribute)
        {
            throw new NotImplementedException();
        }
        unsafe void IMetaDataEmit.SetCustomAttributeValue(uint pcv, void* pCustomAttribute, uint cbCustomAttribute)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineField(uint td, string szName, uint dwFieldFlags, byte* pvSigBlob, uint cbSigBlob, uint dwCPlusTypeFlag,
          void* pValue, uint cchValue)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineProperty(uint td, string szProperty, uint dwPropFlags, byte* pvSig, uint cbSig, uint dwCPlusTypeFlag,
          void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.DefineParam(uint md, uint ulParamSeq, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue)
        {
            throw new NotImplementedException();
        }
        unsafe void IMetaDataEmit.SetFieldProps(uint fd, uint dwFieldFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue)
        {
            throw new NotImplementedException();
        }
        unsafe void IMetaDataEmit.SetPropertyProps(uint pr, uint dwPropFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods)
        {
            throw new NotImplementedException();
        }
        unsafe void IMetaDataEmit.SetParamProps(uint pd, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataEmit.DefineSecurityAttributeSet(uint tkObj, IntPtr rSecAttrs, uint cSecAttrs)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.ApplyEditAndContinue([MarshalAs(UnmanagedType.IUnknown)]object pImport)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataEmit.TranslateSigWithScope(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue,
          IMetaDataImport import, byte* pbSigBlob, uint cbSigBlob, IntPtr pAssemEmit, IMetaDataEmit emit, byte* pvTranslatedSig, uint cbTranslatedSigMax)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.SetMethodImplFlags(uint md, uint dwImplFlags)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.SetFieldRVA(uint fd, uint ulRVA)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.Merge(IMetaDataImport pImport, IntPtr pHostMapToken, [MarshalAs(UnmanagedType.IUnknown)]object pHandler)
        {
            throw new NotImplementedException();
        }
        void IMetaDataEmit.MergeEnd()
        {
            throw new NotImplementedException();
        }
        [PreserveSig]
        void IMetaDataImport.CloseEnum(uint hEnum)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.CountEnum(uint hEnum)
        {
            throw new NotImplementedException();
        }
        void IMetaDataImport.ResetEnum(uint hEnum, uint ulPos)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumTypeDefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rTypeDefs, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumInterfaceImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] rImpls, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumTypeRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rTypeRefs, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.FindTypeDefByName(string szTypeDef, uint tkEnclosingClass)
        {
            throw new NotImplementedException();
        }
        Guid IMetaDataImport.GetScopeProps(StringBuilder szName, uint cchName, out uint pchName)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.GetModuleFromScope()
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetTypeDefProps(uint td, IntPtr szTypeDef, uint cchTypeDef, out uint pchTypeDef, IntPtr pdwTypeDefFlags)
        {
            pchTypeDef = 0;
            if (td == 0) return 0;
            TypeNode t = null;
            if ((td & 0xFF000000) == 0x1B000000)
            {
                t = this.typeSpecEntries[(int)(td & 0xFFFFFF) - 1];
                if (t.Template != null) t = t.Template;
            }
            else
                t = this.typeDefEntries[(int)(td & 0xFFFFFF) - 1];
            if (t == null || t.Name == null) return 0;
            string tName = t.Name.ToString();
            if (tName == null) return 0;
            pchTypeDef = (uint)tName.Length;
            if (pchTypeDef >= cchTypeDef) pchTypeDef = cchTypeDef - 1;
            char* pTypeDef = (char*)szTypeDef.ToPointer();
            for (int i = 0; i < pchTypeDef; i++) *(pTypeDef + i) = tName[i];
            *(pTypeDef + pchTypeDef) = (char)0;
            uint* pFlags = (uint*)pdwTypeDefFlags.ToPointer();
            *(pFlags) = (uint)t.Flags;
            TypeNode bt = t.BaseType;
            if (bt == null) return 0;
            return (uint)this.GetTypeToken(bt);
        }
        uint IMetaDataImport.GetInterfaceImplProps(uint iiImpl, out uint pClass)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.GetTypeRefProps(uint tr, out uint ptkResolutionScope, StringBuilder szName, uint cchName)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.ResolveTypeRef(uint tr, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppIScope)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumMembers(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] rMembers, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumMembersWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rMembers, uint cMax)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.EnumMethods(ref uint phEnum, uint cl, uint* rMethods, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumMethodsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rMethods, uint cMax)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.EnumFields(ref uint phEnum, uint cl, uint* rFields, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumFieldsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rFields, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumParams(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] rParams, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumMemberRefs(ref uint phEnum, uint tkParent, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] rMemberRefs, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumMethodImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rMethodBody,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rMethodDecl, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumPermissionSets(ref uint phEnum, uint tk, uint dwActions, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rPermission,
          uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.FindMember(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pvSigBlob, uint cbSigBlob)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.FindMethod(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pvSigBlob, uint cbSigBlob)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.FindField(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pvSigBlob, uint cbSigBlob)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.FindMemberRef(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pvSigBlob, uint cbSigBlob)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetMethodProps(uint mb, out uint pClass, IntPtr szMethod, uint cchMethod, out uint pchMethod, IntPtr pdwAttr,
          IntPtr ppvSigBlob, IntPtr pcbSigBlob, IntPtr pulCodeRVA)
        {
            Method m = null;
            if ((mb & 0xFF000000) == 0x0A000000)
                m = this.memberRefEntries[(int)(mb & 0xFFFFFF) - 1] as Method;
            else
                m = this.methodEntries[(int)(mb & 0xFFFFFF) - 1];
            pchMethod = 0;
            pClass = 0;
            if (m == null || m.DeclaringType == null) return 0;
            pClass = (uint)this.GetTypeDefToken(m.DeclaringType);
            string methName = m.Name == null ? null : m.Name.ToString();
            if (methName == null) return 0;
            pchMethod = (uint)methName.Length;
            char* pMethName = (char*)szMethod.ToPointer();
            for (int i = 0; i < pchMethod; i++) *(pMethName + i) = methName[i];
            *(pMethName + pchMethod) = (char)0;
            return 0;
        }
        unsafe uint IMetaDataImport.GetMemberRefProps(uint mr, ref uint ptk, StringBuilder szMember, uint cchMember, out uint pchMember, out byte* ppvSigBlob)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.EnumProperties(ref uint phEnum, uint td, uint* rProperties, uint cMax)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.EnumEvents(ref uint phEnum, uint td, uint* rEvents, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.GetEventProps(uint ev, out uint pClass, StringBuilder szEvent, uint cchEvent, out uint pchEvent, out uint pdwEventFlags,
          out uint ptkEventType, out uint pmdAddOn, out uint pmdRemoveOn, out uint pmdFire,
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 11)] uint[] rmdOtherMethod, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumMethodSemantics(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] uint[] rEventProp, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.GetMethodSemantics(uint mb, uint tkEventProp)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.GetClassLayout(uint td, out uint pdwPackSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] COR_FIELD_OFFSET[] rFieldOffset, uint cMax, out uint pcFieldOffset)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetFieldMarshal(uint tk, out byte* ppvNativeType)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.GetRVA(uint tk, out uint pulCodeRVA)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetPermissionSetProps(uint pm, out uint pdwAction, out void* ppvPermission)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetSigFromToken(uint mdSig, out byte* ppvSig)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.GetModuleRefProps(uint mur, StringBuilder szName, uint cchName)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumModuleRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rModuleRefs, uint cmax)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetTypeSpecFromToken(uint typespec, out byte* ppvSig)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.GetNameFromToken(uint tk)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumUnresolvedMethods(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rMethods, uint cMax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.GetUserString(uint stk, StringBuilder szString, uint cchString)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.GetPinvokeMap(uint tk, out uint pdwMappingFlags, StringBuilder szImportName, uint cchImportName, out uint pchImportName)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumSignatures(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rSignatures, uint cmax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumTypeSpecs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rTypeSpecs, uint cmax)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumUserStrings(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] rStrings, uint cmax)
        {
            throw new NotImplementedException();
        }
        [PreserveSig]
        int IMetaDataImport.GetParamForMethodIndex(uint md, uint ulParamSeq, out uint pParam)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.EnumCustomAttributes(ref uint phEnum, uint tk, uint tkType, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] uint[] rCustomAttributes, uint cMax)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetCustomAttributeProps(uint cv, out uint ptkObj, out uint ptkType, out void* ppBlob)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.FindTypeRef(uint tkResolutionScope, string szName)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetMemberProps(uint mb, out uint pClass, StringBuilder szMember, uint cchMember, out uint pchMember, out uint pdwAttr,
          out byte* ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags, out uint pdwCPlusTypeFlag, out void* ppValue)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetFieldProps(uint mb, out uint pClass, StringBuilder szField, uint cchField, out uint pchField, out uint pdwAttr,
          out byte* ppvSigBlob, out uint pcbSigBlob, out uint pdwCPlusTypeFlag, out void* ppValue)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetPropertyProps(uint prop, out uint pClass, StringBuilder szProperty, uint cchProperty, out uint pchProperty, out uint pdwPropFlags,
          out byte* ppvSig, out uint pbSig, out uint pdwCPlusTypeFlag, out void* ppDefaultValue, out uint pcchDefaultValue, out uint pmdSetter,
          out uint pmdGetter, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 14)] uint[] rmdOtherMethod, uint cMax)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetParamProps(uint tk, out uint pmd, out uint pulSequence, StringBuilder szName, uint cchName, out uint pchName,
          out uint pdwAttr, out uint pdwCPlusTypeFlag, out void* ppValue)
        {
            throw new NotImplementedException();
        }
        unsafe uint IMetaDataImport.GetCustomAttributeByName(uint tkObj, string szName, out void* ppData)
        {
            throw new NotImplementedException();
        }
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IMetaDataImport.IsValidToken(uint tk)
        {
            throw new NotImplementedException();
        }
        uint IMetaDataImport.GetNestedClassProps(uint tdNestedClass)
        {
            TypeNode t = null;
            if ((tdNestedClass & 0xFF000000) == 0x1B000000)
                t = this.typeSpecEntries[(int)(tdNestedClass & 0xFFFFFF) - 1];
            else
                t = this.typeDefEntries[(int)(tdNestedClass & 0xFFFFFF) - 1];
            if (t == null || t.DeclaringType == null) return 0;
            return (uint)this.GetTypeToken(t.DeclaringType);
        }
        unsafe uint IMetaDataImport.GetNativeCallConvFromSig(void* pvSig, uint cbSig)
        {
            throw new NotImplementedException();
        }
        int IMetaDataImport.IsGlobal(uint pd)
        {
            throw new NotImplementedException();
        }
#endif
    }
#if WHIDBEYwithGenericsAndIEqualityComparer
    public class ByteArrayKeyComparer : IEqualityComparer, IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            if (x == null || y == null) throw new ArgumentNullException();
            byte[] xa = (byte[])x;
            byte[] ya = (byte[])y;
            int n = xa.Length;
            int result = n - ya.Length;
            if (result != 0) return result;
            for (int i = 0; i < n; i++)
            {
                result = xa[i] - ya[i];
                if (result != 0) return result;
            }
            return 0;
        }
        bool IEqualityComparer.Equals(object x, object y)
        {
            if (x == null || y == null) return x == y;
            return ((IComparer)this).Compare(x, y) == 0;
        }
        int IEqualityComparer.GetHashCode(object/*!*/ x)
        {
            Debug.Assert(x != null);
            byte[] xa = (byte[])x;
            int hcode = 1;
            for (int i = 0, n = xa.Length; i < n; i++)
                hcode = hcode * 17 + xa[i];
            return hcode;
        }
    }
#elif WHIDBEYwithGenerics
  public class ByteArrayKeyComparer : IKeyComparer{
    int IComparer.Compare(object x, object y) {
      if (x == null || y == null) throw new ArgumentNullException();
      byte[] xa = (byte[])x;
      byte[] ya = (byte[])y;
      int n = xa.Length;
      int result = n - ya.Length;
      if (result != 0) return result;
      for (int i = 0; i < n; i++){
        result = xa[i] - ya[i];
        if (result != 0) return result;
      }
      return 0;
    }
    bool IKeyComparer.Equals(object x, object y){
      return ((IKeyComparer)this).Compare(x, y) == 0;
    }
    int IHashCodeProvider.GetHashCode(object x) {
      Debug.Assert(x != null);
      byte[] xa = (byte[])x;
      int hcode = 1;
      for (int i = 0, n = xa.Length; i < n; i++)
        hcode = hcode * 17 + xa[i];
      return hcode;
    }
  }
#else
  public class ByteArrayComparer : IComparer{
    int IComparer.Compare(object x, object y){
      if (x == null || y == null) throw new ArgumentNullException();
      byte[] xa = (byte[])x;
      byte[] ya = (byte[])y;
      int n = xa.Length;
      int result = n - ya.Length;
      if (result != 0) return result;
      for (int i = 0; i < n; i++){
        result = xa[i] - ya[i];
        if (result != 0) return result;
      }
      return 0;
    }
  }
  public class ByteArrayHasher : IHashCodeProvider{
    int IHashCodeProvider.GetHashCode(object x){
      Debug.Assert(x != null);
      byte[] xa = (byte[])x;
      int hcode = 1;
      for (int i = 0, n = xa.Length; i < n; i++)
        hcode = hcode*17 + xa[i];
      return hcode;
    }
  }
#endif
    internal class Fixup
    {
        internal int fixupLocation;
        internal int addressOfNextInstruction;
        internal bool shortOffset;
        internal Fixup nextFixUp;
    }
    internal class MethodInfo
    {
        internal TrivialHashtable/*!*/ fixupIndex = new TrivialHashtable();
        internal int localVarSigTok;
        internal BinaryWriter/*!*/ localVarSignature;
        internal TrivialHashtable/*!*/ localVarIndex;
#if !ROTOR
        internal NodeList/*!*/ statementNodes;
        internal LocalList/*!*/ debugLocals;
        internal Int32List/*!*/ signatureLengths;
        internal Int32List/*!*/ signatureOffsets;
        internal Int32List/*!*/ statementOffsets;
#endif

        public MethodInfo()
        {
            //^ base();
        }
    }
    public class KeyFileNotFoundException : System.ArgumentException { }
    public class AssemblyCouldNotBeSignedException : System.ApplicationException { }
    public class DebugSymbolsCouldNotBeWrittenException : System.ApplicationException { }
    internal class Writer
    {
        private Writer() { }
        internal static void WritePE(System.CodeDom.Compiler.CompilerParameters/*!*/ compilerParameters, Module/*!*/ module)
        //^ requires module.Location != null;
        {
            if (compilerParameters == null) { Debug.Assert(false); return; }
            CompilerOptions options = compilerParameters as CompilerOptions;
            if (options == null)
                Writer.WritePE(module.Location, compilerParameters.IncludeDebugInformation, module, false, null, null);
            else
            {
                if (options.FileAlignment > 512) module.FileAlignment = options.FileAlignment;
                Writer.WritePE(module.Location, options.IncludeDebugInformation, module, options.DelaySign, options.AssemblyKeyFile, options.AssemblyKeyName);
            }
        }
        internal static void WritePE(string/*!*/ location, bool writeDebugSymbols, Module/*!*/ module)
        {
            Writer.WritePE(location, writeDebugSymbols, module, false, null, null);
        }
        private static void WritePE(string/*!*/ location, bool writeDebugSymbols, Module/*!*/ module, bool delaySign, string keyFileName, string keyName)
        {
            AssemblyNode assem = module as AssemblyNode;
            location = Path.GetFullPath(location);
            module.Directory = Path.GetDirectoryName(location);
            bool keyFileNameDoesNotExist = false;
            if (assem != null)
            {
                assem.KeyContainerName = keyName;
                if (keyFileName != null && keyFileName.Length > 0)
                {
                    if (!File.Exists(keyFileName)) keyFileName = Path.Combine(module.Directory, keyFileName);
                    if (File.Exists(keyFileName))
                    {
                        using (FileStream keyFile = File.OpenRead(keyFileName))
                        {
                            long size = keyFile.Length;
                            if (size > int.MaxValue) throw new System.IO.FileLoadException();
                            int n = (int)size;
                            byte[] key = new byte[n];
                            keyFile.Read(key, 0, n);
                            assem.KeyBlob = key;
                        }
                    }
                    else
                        keyFileNameDoesNotExist = true;
                }
                assem.PublicKeyOrToken = Writer.GetPublicKey(assem);
            }
            using (FileStream exeFstream = new FileStream(location, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                string debugSymbolsLocation = writeDebugSymbols ? Path.ChangeExtension(location, "pdb") : null;
                if (debugSymbolsLocation != null && File.Exists(debugSymbolsLocation))
                    File.Delete(debugSymbolsLocation);
                MemoryStream exeMstream = new MemoryStream();
                Ir2md.WritePE(module, debugSymbolsLocation, new BinaryWriter(exeMstream));
                exeMstream.WriteTo(exeFstream);
            }
            if (keyFileNameDoesNotExist) throw new KeyFileNotFoundException();
            if (delaySign || assem == null) return;
            if (assem.KeyBlob != null || (assem.KeyContainerName != null && assem.KeyContainerName.Length > 0))
            {
                try
                {
                    if (!Writer.StrongNameSignatureGeneration(location, keyName, assem.KeyBlob, assem.KeyBlob == null ? 0 : assem.KeyBlob.Length, IntPtr.Zero, IntPtr.Zero))
                        throw new AssemblyCouldNotBeSignedException();
                }
                catch
                {
                    if (!Writer.MscorsnStrongNameSignatureGeneration(location, keyName, assem.KeyBlob, assem.KeyBlob == null ? 0 : assem.KeyBlob.Length, IntPtr.Zero, IntPtr.Zero))
                        throw new AssemblyCouldNotBeSignedException();
                }
            }
        }
        [DllImport("mscoree.dll", EntryPoint = "StrongNameSignatureGeneration",
           SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
           CallingConvention = CallingConvention.StdCall)]
        private static extern bool StrongNameSignatureGeneration(
          string wszFilePath,        // [in] valid path to the PE file for the assembly
          string wszKeyContainer,    // [in] desired key container name
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]
      byte[] pbKeyBlob,          // [in] public/private key blob (optional)
          int cbKeyBlob,
          IntPtr ppbSignatureBlob,   // [out] signature blob
          IntPtr pcbSignatureBlob);
        [DllImport("mscorsn.dll", EntryPoint = "StrongNameSignatureGeneration",
           SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
           CallingConvention = CallingConvention.StdCall)]
        private static extern bool MscorsnStrongNameSignatureGeneration(
          string wszFilePath,        // [in] valid path to the PE file for the assembly
          string wszKeyContainer,    // [in] desired key container name
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]
      byte[] pbKeyBlob,          // [in] public/private key blob (optional)
          int cbKeyBlob,
          IntPtr ppbSignatureBlob,   // [out] signature blob
          IntPtr pcbSignatureBlob);
        private unsafe static byte[] GetPublicKey(AssemblyNode/*!*/ assem)
        {
            Debug.Assert(assem != null);
            IntPtr publicKey = IntPtr.Zero;
            int size;
            try
            {
                if (assem.KeyBlob != null)
                {
                    Writer.StrongNameGetPublicKey(null, assem.KeyBlob, assem.KeyBlob.Length, out publicKey, out size);
                    if (publicKey == IntPtr.Zero) return assem.KeyBlob;
                }
                else if (assem.KeyContainerName != null)
                {
                    Writer.StrongNameGetPublicKey(assem.KeyContainerName, null, 0, out publicKey, out size);
                    if (publicKey == IntPtr.Zero) return null;
                }
                else
                    return assem.PublicKeyOrToken;
                byte[] result = new byte[size];
                byte* ptr = (byte*)publicKey;
                for (int i = 0; i < size; i++) result[i] = *ptr++;
                return result;
            }
            catch { }
            {
                if (assem.KeyBlob != null)
                {
                    Writer.MscorsnStrongNameGetPublicKeyUsing(null, assem.KeyBlob, assem.KeyBlob.Length, out publicKey, out size);
                    if (publicKey == IntPtr.Zero) return assem.KeyBlob;
                }
                else if (assem.KeyContainerName != null)
                {
                    Writer.MscorsnStrongNameGetPublicKeyUsing(assem.KeyContainerName, null, 0, out publicKey, out size);
                    if (publicKey == IntPtr.Zero) return null;
                }
                else
                    return assem.PublicKeyOrToken;
                byte[] result = new byte[size];
                byte* ptr = (byte*)publicKey;
                for (int i = 0; i < size; i++) result[i] = *ptr++;
                return result;
            }
        }
        [DllImport("mscoree.dll", EntryPoint = "StrongNameGetPublicKey",
           SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
           CallingConvention = CallingConvention.StdCall)]
        private static extern bool StrongNameGetPublicKey(
          string wszKeyContainer,    // [in] desired key container name
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
      byte[] pbKeyBlob,          // [in] public/private key blob (optional)
          int cbKeyBlob,
          [Out] out IntPtr ppbPublicKeyBlob,   // [out] public key blob
          [Out] out int pcbPublicKeyBlob);
        [DllImport("mscorsn.dll", EntryPoint = "StrongNameGetPublicKey",
           SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
           CallingConvention = CallingConvention.StdCall)]
        private static extern bool MscorsnStrongNameGetPublicKeyUsing(
          string wszKeyContainer,    // [in] desired key container name
          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
      byte[] pbKeyBlob,          // [in] public/private key blob (optional)
          int cbKeyBlob,
          [Out] out IntPtr ppbPublicKeyBlob,   // [out] public key blob
          [Out] out int pcbPublicKeyBlob);

        internal static void WritePE(Stream/*!*/ executable, Stream debugSymbols, Module/*!*/ module)
        {
            MemoryStream mstream = new MemoryStream();
            Ir2md.WritePE(module, null, new BinaryWriter(mstream)); //TODO: need to write the PDB symbols to the stream
            mstream.WriteTo(executable);
        }
        internal static void WritePE(out byte[] executable, Module/*!*/ module)
        {
            MemoryStream mstream = new MemoryStream();
            Ir2md.WritePE(module, null, new BinaryWriter(mstream));
            executable = mstream.ToArray();
        }
        internal static void WritePE(out byte[] executable, out byte[] debugSymbols, Module/*!*/ module)
        {
            MemoryStream mstream = new MemoryStream();
            Ir2md.WritePE(module, null, new BinaryWriter(mstream));
            executable = mstream.ToArray();
            debugSymbols = null;
        }
        internal unsafe static void AddWin32Icon(Module/*!*/ module, string win32IconFilePath)
        {
            if (module == null || win32IconFilePath == null) { Debug.Assert(false); return; }
            using (System.IO.FileStream resStream = File.OpenRead(win32IconFilePath))
            {
                Writer.AddWin32Icon(module, resStream);
            }
        }
        internal unsafe static void AddWin32Icon(Module/*!*/ module, Stream win32IconStream)
        {
            if (module == null || win32IconStream == null) { Debug.Assert(false); return; }
            long size = win32IconStream.Length;
            if (size > int.MaxValue) throw new System.IO.FileLoadException();
            int n = (int)size;
            byte[] buffer = new byte[n];
            win32IconStream.Read(buffer, 0, n);
            byte* pb = (byte*)Marshal.AllocHGlobal(n);
            for (int i = 0; i < n; i++) pb[i] = buffer[i];
            MemoryCursor cursor = new MemoryCursor(pb, n/*, module*/);
            if (module.Win32Resources == null) module.Win32Resources = new Win32ResourceList();
            int reserved = cursor.ReadUInt16();
            if (reserved != 0) throw new NullReferenceException();
            int resourceType = cursor.ReadUInt16();
            if (resourceType != 1) throw new NullReferenceException();
            int imageCount = cursor.ReadUInt16();
            BinaryWriter indexHeap = new BinaryWriter(new MemoryStream());
            indexHeap.Write((ushort)0); //Reserved
            indexHeap.Write((ushort)1); //idType
            indexHeap.Write((ushort)imageCount);
            Win32Resource resource = new Win32Resource();
            for (int i = 0; i < imageCount; i++)
            {
                resource = new Win32Resource();
                resource.CodePage = 0;
                resource.Id = module.Win32Resources.Count + 2;
                resource.LanguageId = 0;
                resource.Name = null;
                resource.TypeId = 3;
                resource.TypeName = null;
                indexHeap.Write(cursor.ReadByte()); //width
                indexHeap.Write(cursor.ReadByte()); //height
                indexHeap.Write(cursor.ReadByte()); //color count
                indexHeap.Write(cursor.ReadByte()); //reserved
                indexHeap.Write(cursor.ReadUInt16()); //planes
                indexHeap.Write(cursor.ReadUInt16()); //bit count
                int len = cursor.ReadInt32();
                int offset = cursor.ReadInt32();
                indexHeap.Write((int)len);
                indexHeap.Write((int)module.Win32Resources.Count + 2);
                MemoryCursor c = new MemoryCursor(cursor);
                c.Position = offset;
                resource.Data = c.ReadBytes(len);
                module.Win32Resources.Add(resource);
            }
            resource.CodePage = 0;
            resource.Data = indexHeap.BaseStream.ToArray();
            resource.Id = 0x7f00;
            resource.LanguageId = 0;
            resource.Name = null;
            resource.TypeId = 0xe;
            resource.TypeName = null;
            module.Win32Resources.Add(resource);
        }
        internal unsafe static void AddWin32ResourceFileToModule(Module/*!*/ module, string/*!*/ win32ResourceFilePath)
        {
            if (module == null || win32ResourceFilePath == null) { Debug.Assert(false); return; }
            using (System.IO.FileStream resStream = File.OpenRead(win32ResourceFilePath))
            {
                Writer.AddWin32ResourceFileToModule(module, resStream);
            }
        }
        internal unsafe static void AddWin32ResourceFileToModule(Module/*!*/ module, Stream/*!*/ win32ResourceStream)
        {
            if (module == null || win32ResourceStream == null) { Debug.Assert(false); return; }
            long size = win32ResourceStream.Length;
            if (size > int.MaxValue) throw new System.IO.FileLoadException();
            int n = (int)size;
            byte[] buffer = new byte[n];
            win32ResourceStream.Read(buffer, 0, n);
            byte* pb = (byte*)Marshal.AllocHGlobal(n);
            for (int i = 0; i < n; i++) pb[i] = buffer[i];
            MemoryCursor cursor = new MemoryCursor(pb, n/*, module*/);
            if (module.Win32Resources == null) module.Win32Resources = new Win32ResourceList();
            while (cursor.Position < n)
            {
                Win32Resource resource = new Win32Resource();
                resource.CodePage = 0; //Review: Should this be settable?
                int dataSize = cursor.ReadInt32();
                cursor.ReadUInt32(); //headerSize
                if (cursor.Int16(0) == -1)
                {
                    cursor.ReadInt16();
                    resource.TypeId = cursor.ReadUInt16();
                    resource.TypeName = null;
                }
                else
                {
                    resource.TypeId = 0;
                    resource.TypeName = cursor.ReadUTF16();
                }
                if (cursor.Int16(0) == -1)
                {
                    cursor.ReadInt16();
                    resource.Id = cursor.ReadUInt16();
                    resource.Name = null;
                }
                else
                {
                    resource.Id = 0;
                    resource.Name = cursor.ReadUTF16();
                }
                cursor.ReadUInt32(); //dataVersion
                cursor.ReadUInt16(); //memoryFlags
                resource.LanguageId = cursor.ReadUInt16();
                cursor.ReadUInt32(); //version
                cursor.ReadUInt32(); //characteristics
                resource.Data = cursor.ReadBytes(dataSize);
                if (resource.Data != null)
                    module.Win32Resources.Add(resource);
            }
        }
        internal static void AddWin32VersionInfo(Module/*!*/ module, CompilerOptions/*!*/ options)
        {
            if (module == null || options == null) { Debug.Assert(false); return; }
            Win32Resource resource = new Win32Resource();
            resource.CodePage = 0;
            resource.Id = 1;
            resource.LanguageId = 0;
            resource.Name = null;
            resource.TypeId = 0x10;
            resource.TypeName = null;
            resource.Data = Writer.FillInVsVersionStructure(module, options);
            if (module.Win32Resources == null) module.Win32Resources = new Win32ResourceList();
            module.Win32Resources.Add(resource);
        }
        private static byte[] FillInVsVersionStructure(Module/*!*/ module, CompilerOptions/*!*/ options)
        {
            AssemblyNode assembly = module as AssemblyNode;
            BinaryWriter data = new BinaryWriter(new MemoryStream(), Encoding.Unicode);
            data.Write((ushort)0); //Space for length
            data.Write((ushort)0x34); //VS_FIXEDFILEINFO length
            data.Write((ushort)0); //Type of data in version resource
            data.Write("VS_VERSION_INFO", true);
            data.Write((ushort)0); //Padding to 4 byte boundary
            // VS_FIXEDFILEINFO starts here
            data.Write((uint)0xFEEF04BD); //Signature
            data.Write((uint)0x00010000); //Version of VS_FIXEDFILEINFO
            Version fileVersion = Writer.ParseVersion(options.TargetInformation.Version, true);
            if (fileVersion == null && assembly != null) fileVersion = assembly.Version;
            if (fileVersion == null) fileVersion = new Version();
            data.Write((ushort)fileVersion.Minor);
            data.Write((ushort)fileVersion.Major);
            data.Write((ushort)fileVersion.Revision);
            data.Write((ushort)fileVersion.Build);
            Version productVersion = Writer.ParseVersion(options.TargetInformation.ProductVersion, true);
            if (productVersion == null) productVersion = fileVersion;
            data.Write((ushort)productVersion.Minor);
            data.Write((ushort)productVersion.Major);
            data.Write((ushort)productVersion.Revision);
            data.Write((ushort)productVersion.Build);
            data.Write((uint)0x3f); //FileFlagsMask
            data.Write((uint)0x0); //FileFlags
            data.Write((uint)0x4); //OS: Win32 (After all, this is a Win32 resource.)
            if (options.GenerateExecutable)
                data.Write((uint)1); //App
            else
                data.Write((uint)2); //Dll
            data.Write((uint)0); //File subtype
            data.Write((ulong)0); //File Date
            // VarFileInfo
            data.Write((ushort)0x44); //Length of VarFileInfo
            data.Write((ushort)0x0); //Length of value
            data.Write((ushort)0x1); //type (text)
            data.Write("VarFileInfo", true);
            data.Write((ushort)0); //padding to 4 byte boundary
            // Var
            data.Write((ushort)0x24); //Length of Var
            data.Write((ushort)0x04); //length of Value
            data.Write((ushort)0); //Type (binary)
            data.Write("Translation", true);
            data.Write((uint)0); //Padding
            data.Write((ushort)0x4b0); //Code Page for Unicode
            // StringFileInfo
            int positionOfInfoLength = data.BaseStream.Position;
            data.Write((ushort)0); //length of rest of resource
            data.Write((ushort)0); //Value length, always 0
            data.Write((ushort)1); //Type (text)
            data.Write("StringFileInfo", true);
            // StringInfo
            int stringInfoLengthPos = data.BaseStream.Position;
            data.Write((ushort)0); //Space for length
            data.Write((ushort)0); //Value length, always 0
            data.Write((ushort)1); //Type (text)
            data.Write("000004b0", true); //Code page for Unicode
            Writer.WriteVersionString(data, options.TargetInformation.Description, "Comments");
            Writer.WriteVersionString(data, options.TargetInformation.Company, "CompanyName");
            Writer.WriteVersionString(data, options.TargetInformation.Title, "FileDescription");
            Writer.WriteVersionString(data, Writer.ConvertToString(fileVersion), "FileVersion");
            string fileName = module.Name + (options.GenerateExecutable ? ".exe" : ".dll");
            Writer.WriteVersionString(data, fileName, "InternalName");
            Writer.WriteVersionString(data, options.TargetInformation.Copyright, "LegalCopyright");
            Writer.WriteVersionString(data, options.TargetInformation.Trademark, "LegalTrademarks");
            Writer.WriteVersionString(data, fileName, "OriginalFilename");
            Writer.WriteVersionString(data, options.TargetInformation.Product, "ProductName");
            Writer.WriteVersionString(data, Writer.ConvertToString(productVersion), "ProductVersion");
            if (assembly != null)
                Writer.WriteVersionString(data, assembly.Version == null ? "" : assembly.Version.ToString(), "Assembly Version");
            int len = data.BaseStream.Position;
            data.BaseStream.Position = stringInfoLengthPos;
            data.Write((ushort)(len - stringInfoLengthPos));
            data.BaseStream.Position = 0;
            data.Write((ushort)len);
            data.BaseStream.Position = positionOfInfoLength;
            data.Write((ushort)len - positionOfInfoLength);
            return data.BaseStream.ToArray();
        }
        private static void WriteVersionString(BinaryWriter/*!*/ data, string value, string/*!*/ key)
        {
            if (value == null) return;
            int totalLength = 6;
            totalLength += key.Length * 2;
            totalLength += 4 - (totalLength % 4);
            totalLength += value.Length * 2;
            totalLength += 4 - (totalLength % 4);
            data.Write((ushort)totalLength);
            data.Write((ushort)(value.Length + 1));
            data.Write((ushort)1); //Type (text)
            data.Write(key, true);
            if (data.BaseStream.Position % 4 != 0) data.Write((char)0);
            data.Write(value, true);
            if (data.BaseStream.Position % 4 != 0) data.Write((char)0);
        }
        private static string/*!*/ ConvertToString(Version/*!*/ version)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(version.Major.ToString());
            if (version.Minor != 0 || version.Build != 0 || version.Revision != 0)
            {
                sb.Append('.');
                sb.Append(version.Minor.ToString());
            }
            if (version.Build != 0 || version.Revision != 0)
            {
                sb.Append('.');
                sb.Append(version.Build.ToString());
            }
            if (version.Revision != 0)
            {
                sb.Append('.');
                sb.Append(version.Revision.ToString());
            }
            return sb.ToString();
        }
        private static Version ParseVersion(string vString, bool allowWildcards)
        {
            if (vString == null) return null;
            ushort major = 1;
            ushort minor = 0;
            ushort build = 0;
            ushort revision = 0;
            try
            {
                int n = vString.Length;
                int i = vString.IndexOf('.', 0);
                if (i < 0) throw new FormatException();
                major = UInt16.Parse(vString.Substring(0, i), CultureInfo.InvariantCulture);
                int j = vString.IndexOf('.', i + 1);
                if (j < i + 1)
                    minor = UInt16.Parse(vString.Substring(i + 1, n - i - 1), CultureInfo.InvariantCulture);
                else
                {
                    minor = UInt16.Parse(vString.Substring(i + 1, j - i - 1), CultureInfo.InvariantCulture);
                    if (vString[j + 1] == '*' && allowWildcards)
                    {
                        if (j + 1 < n - 1) return null;
                        build = Writer.DaysSince2000();
                        revision = Writer.SecondsSinceMidnight();
                    }
                    else
                    {
                        int k = vString.IndexOf('.', j + 1);
                        if (k < j + 1)
                            build = UInt16.Parse(vString.Substring(j + 1, n - j - 1), CultureInfo.InvariantCulture);
                        else
                        {
                            build = UInt16.Parse(vString.Substring(j + 1, k - j - 1), CultureInfo.InvariantCulture);
                            if (vString[k + 1] == '*' && allowWildcards)
                            {
                                if (j + 1 < n - 1) return null;
                                revision = Writer.SecondsSinceMidnight();
                            }
                            else
                                revision = UInt16.Parse(vString.Substring(k + 1, n - k - 1), CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            catch (FormatException)
            {
                major = minor = build = revision = UInt16.MaxValue;
            }
            catch (OverflowException)
            {
                major = minor = build = revision = UInt16.MaxValue;
            }
            if (major == UInt16.MaxValue && minor == UInt16.MaxValue && build == UInt16.MaxValue && revision == UInt16.MaxValue)
            {
                return null;
            }
            return new Version(major, minor, build, revision);
        }
        private static ushort DaysSince2000()
        {
            return (ushort)(DateTime.Now - new DateTime(2000, 1, 1)).Days;
        }
        private static ushort SecondsSinceMidnight()
        {
            TimeSpan sinceMidnight = DateTime.Now - DateTime.Today;
            return (ushort)((sinceMidnight.Hours * 60 * 60 + sinceMidnight.Minutes * 60 + sinceMidnight.Seconds) / 2);
        }

    }
}
#endif
