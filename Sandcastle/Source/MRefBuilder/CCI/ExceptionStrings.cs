// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 11/23/2013 - EFW - Cleared out the conditional statements

using System.Resources;

namespace System.Compiler
{
    sealed class ExceptionStrings
    {
        private readonly static WeakReference/*!*/ resMgr = new WeakReference(null);

        private ExceptionStrings()
        {
        }

        internal static System.Resources.ResourceManager/*!*/ ResourceManager
        {
            get
            {
                System.Resources.ResourceManager rMgr = ExceptionStrings.resMgr.Target as System.Resources.ResourceManager;
                if (rMgr == null)
                {
                    rMgr = new System.Resources.ResourceManager("Microsoft.Ddue.Tools.CCI.ExceptionStrings",
                        typeof(ExceptionStrings).Assembly);
                    ExceptionStrings.resMgr.Target = rMgr;
                }
                return rMgr;
            }
        }
        internal static string/*!*/ AssemblyReferenceNotResolved
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("AssemblyReferenceNotResolved", null);
            }
        }
        internal static string/*!*/ BadBlobHeapIndex
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadBlobHeapIndex", null);
            }
        }
        internal static string/*!*/ BadCLIHeader
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadCLIHeader", null);
            }
        }
        internal static string/*!*/ BadCOFFHeaderSignature
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadCOFFHeaderSignature", null);
            }
        }
        internal static string/*!*/ BadConstantParentIndex
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadConstantParentIndex", null);
            }
        }
        internal static string/*!*/ BadCustomAttributeTypeEncodedToken
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadCustomAttributeTypeEncodedToken", null);
            }
        }
        internal static string/*!*/ BadCalliSignature
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadCalliSignature", null);
            }
        }
        internal static string/*!*/ BadExceptionHandlerType
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadExceptionHandlerType", null);
            }
        }
        internal static string/*!*/ BadGuidHeapIndex
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadGuidHeapIndex", null);
            }
        }
        internal static string/*!*/ BadMagicNumber
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadMagicNumber", null);
            }
        }
        internal static string/*!*/ BadMemberToken
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadMemberToken", null);
            }
        }
        internal static string/*!*/ BadMetadataHeaderSignature
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadMetadataHeaderSignature", null);
            }
        }
        internal static string/*!*/ BadMetadataInExportTypeTableNoSuchAssemblyReference
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadMetadataInExportTypeTableNoSuchAssemblyReference", null);
            }
        }
        internal static string/*!*/ BadMetadataInExportTypeTableNoSuchParentType
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadMetadataInExportTypeTableNoSuchParentType", null);
            }
        }
        internal static string/*!*/ BadMethodHeaderSection
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadMethodHeaderSection", null);
            }
        }
        internal static string/*!*/ BadMethodTypeParameterInPosition
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadMethodTypeParameterInPosition", null);
            }
        }
        internal static string/*!*/ BadPEHeaderMagicNumber
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadPEHeaderMagicNumber", null);
            }
        }
        internal static string/*!*/ BadSecurityPermissionSetBlob
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadSecurityPermissionSetBlob", null);
            }
        }
        internal static string/*!*/ BadSerializedTypeName
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadSerializedTypeName", null);
            }
        }
        internal static string/*!*/ BadStringHeapIndex
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadStringHeapIndex", null);
            }
        }
        internal static string BadTargetPlatformLocation
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadTargetPlatformLocation", null);
            }
        }
        internal static string BadTypeDefOrRef
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadTypeDefOrRef", null);
            }
        }
        internal static string/*!*/ BadTypeParameterInPositionForType
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadTypeParameterInPositionForType", null);
            }
        }
        internal static string/*!*/ BadUserStringHeapIndex
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("BadUserStringHeapIndex", null);
            }
        }
        internal static string/*!*/ CannotLoadTypeExtension
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("CannotLoadTypeExtension", null);
            }
        }
        internal static string/*!*/ CollectionIsReadOnly
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("CollectionIsReadOnly", null);
            }
        }
        internal static string CouldNotFindExportedNestedTypeInType
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("CouldNotFindExportedNestedTypeInType", null);
            }
        }
        internal static string/*!*/ CouldNotFindExportedTypeInAssembly
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("CouldNotFindExportedTypeInAssembly", null);
            }
        }
        internal static string CouldNotFindExportedTypeInModule
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("CouldNotFindExportedTypeInModule", null);
            }
        }
        internal static string/*!*/ CouldNotFindReferencedModule
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("CouldNotFindReferencedModule", null);
            }
        }
        internal static string/*!*/ CouldNotResolveMemberReference
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("CouldNotResolveMemberReference", null);
            }
        }
        internal static string/*!*/ CouldNotResolveType
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("CouldNotResolveType", null);
            }
        }
        internal static string CouldNotResolveTypeReference
        {
            get
            {
                return ResourceManager.GetString("CouldNotResolveTypeReference", null);
            }
        }
        internal static string/*!*/ CreateFileMappingReturnedErrorCode
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("CreateFileMappingReturnedErrorCode", null);
            }
        }
        internal static string/*!*/ ENCLogTableEncountered
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("ENCLogTableEncountered", null);
            }
        }
        internal static string/*!*/ ENCMapTableEncountered
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("ENCMapTableEncountered", null);
            }
        }
        internal static string/*!*/ FileTooBig
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("FileTooBig", null);
            }
        }
        internal static string/*!*/ GetReaderForFileReturnedUnexpectedHResult
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("GetReaderForFileReturnedUnexpectedHResult", null);
            }
        }
        internal static string/*!*/ InternalCompilerError
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("InternalCompilerError", null);
            }
        }
        internal static string/*!*/ InvalidBaseClass
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("InvalidBaseClass", null);
            }
        }
        internal static string/*!*/ InvalidFatMethodHeader
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("InvalidFatMethodHeader", null);
            }
        }
        internal static string/*!*/ InvalidLocalSignature
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("InvalidLocalSignature", null);
            }
        }
        internal static string InvalidModuleTable
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("InvalidModuleTable", null);
            }
        }
        internal static string/*!*/ InvalidTypeTableIndex
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("InvalidTypeTableIndex", null);
            }
        }
        internal static string MalformedSignature
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("MalformedSignature", null);
            }
        }
        internal static string/*!*/ MapViewOfFileReturnedErrorCode
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("MapViewOfFileReturnedErrorCode", null);
            }
        }
        internal static string/*!*/ ModuleOrAssemblyDependsOnMoreRecentVersionOfCoreLibrary
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("ModuleOrAssemblyDependsOnMoreRecentVersionOfCoreLibrary", null);
            }
        }
        internal static string/*!*/ ModuleError
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("ModuleError", null);
            }
        }
        internal static string/*!*/ NoMetadataStream
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("NoMetadataStream", null);
            }
        }
        internal static string/*!*/ PdbAssociatedWithFileIsOutOfDate
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("PdbAssociatedWithFileIsOutOfDate", null);
            }
        }
        internal static string/*!*/ SecurityAttributeTypeDoesNotHaveADefaultConstructor
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("SecurityAttributeTypeDoesNotHaveADefaultConstructor", null);
            }
        }
        internal static string/*!*/ TooManyMethodHeaderSections
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("TooManyMethodHeaderSections", null);
            }
        }
        internal static string/*!*/ UnexpectedTypeInCustomAttribute
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("UnexpectedTypeInCustomAttribute", null);
            }
        }
        internal static string/*!*/ UnknownConstantType
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("UnknownConstantType", null);
            }
        }
        internal static string/*!*/ UnknownOpCode
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("UnknownOpCode", null);
            }
        }
        internal static string/*!*/ UnknownOpCodeEncountered
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("UnknownOpCodeEncountered", null);
            }
        }
        internal static string/*!*/ UnknownVirtualAddress
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("UnknownVirtualAddress", null);
            }
        }
        internal static string/*!*/ UnsupportedTableEncountered
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("UnsupportedTableEncountered", null);
            }
        }
        internal static string/*!*/ InvalidAssemblyStrongName
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("InvalidAssemblyStrongName", null);
            }
        }
        internal static string/*!*/ KeyNeedsToBeGreaterThanZero
        {
            get
            {
                return /*^ (!) ^*/ ResourceManager.GetString("KeyNeedsToBeGreaterThanZero", null);
            }
        }
    }
}
