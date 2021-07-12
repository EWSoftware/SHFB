// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 11/23/2013 - EFW - Cleared out the conditional statements

using System.Resources;

namespace System.Compiler
{
    internal static class ExceptionStrings
    {
        private static ResourceManager resMgr;

        internal static ResourceManager/*!*/ ResourceManager
        {
            get
            {
                if(resMgr == null)
                {
                    // NOTE: DO NOT NAME THE RESX FILE THE SAME AS THIS CLASS!
                    //       The compiler has different naming conventions for .NET Framework versus .NET Core.
                    //       For .NET Framework, it names it using the default namespace.  For .NET Core, it uses
                    //       the type's namespace.  This causes it to fail to load under .NET core.  By giving it
                    //       a name different from the class, both name it using the default namespace.
                    resMgr = new ResourceManager("Sandcastle.Tools.MSBuild.CCI.ExceptionStringResources",
                        typeof(ExceptionStrings).Assembly);
                }

                return resMgr;
            }
        }
        
        internal static string/*!*/ AssemblyReferenceNotResolved => ResourceManager.GetString("AssemblyReferenceNotResolved", null);

        internal static string/*!*/ BadBlobHeapIndex => ResourceManager.GetString("BadBlobHeapIndex", null);

        internal static string/*!*/ BadCLIHeader => ResourceManager.GetString("BadCLIHeader", null);

        internal static string/*!*/ BadCOFFHeaderSignature => ResourceManager.GetString("BadCOFFHeaderSignature", null);

        internal static string/*!*/ BadConstantParentIndex => ResourceManager.GetString("BadConstantParentIndex", null);

        internal static string/*!*/ BadCustomAttributeTypeEncodedToken => ResourceManager.GetString("BadCustomAttributeTypeEncodedToken", null);

        internal static string/*!*/ BadCalliSignature => ResourceManager.GetString("BadCalliSignature", null);

        internal static string/*!*/ BadExceptionHandlerType => ResourceManager.GetString("BadExceptionHandlerType", null);

        internal static string/*!*/ BadGuidHeapIndex => ResourceManager.GetString("BadGuidHeapIndex", null);

        internal static string/*!*/ BadMagicNumber => ResourceManager.GetString("BadMagicNumber", null);

        internal static string/*!*/ BadMemberToken => ResourceManager.GetString("BadMemberToken", null);
        
        internal static string/*!*/ BadMetadataHeaderSignature => ResourceManager.GetString("BadMetadataHeaderSignature", null);

        internal static string/*!*/ BadMetadataInExportTypeTableNoSuchAssemblyReference => ResourceManager.GetString("BadMetadataInExportTypeTableNoSuchAssemblyReference", null);

        internal static string/*!*/ BadMetadataInExportTypeTableNoSuchParentType => ResourceManager.GetString("BadMetadataInExportTypeTableNoSuchParentType", null);

        internal static string/*!*/ BadMethodHeaderSection => ResourceManager.GetString("BadMethodHeaderSection", null);

        internal static string/*!*/ BadMethodTypeParameterInPosition => ResourceManager.GetString("BadMethodTypeParameterInPosition", null);

        internal static string/*!*/ BadPEHeaderMagicNumber => ResourceManager.GetString("BadPEHeaderMagicNumber", null);

        internal static string/*!*/ BadSecurityPermissionSetBlob => ResourceManager.GetString("BadSecurityPermissionSetBlob", null);

        internal static string/*!*/ BadSerializedTypeName => ResourceManager.GetString("BadSerializedTypeName", null);

        internal static string/*!*/ BadStringHeapIndex => ResourceManager.GetString("BadStringHeapIndex", null);

        internal static string BadTargetPlatformLocation => ResourceManager.GetString("BadTargetPlatformLocation", null);

        internal static string BadTypeDefOrRef => ResourceManager.GetString("BadTypeDefOrRef", null);

        internal static string/*!*/ BadTypeParameterInPositionForType => ResourceManager.GetString("BadTypeParameterInPositionForType", null);

        internal static string/*!*/ BadUserStringHeapIndex => ResourceManager.GetString("BadUserStringHeapIndex", null);

        internal static string/*!*/ CannotLoadTypeExtension => ResourceManager.GetString("CannotLoadTypeExtension", null);

        internal static string/*!*/ CollectionIsReadOnly => ResourceManager.GetString("CollectionIsReadOnly", null);

        internal static string CouldNotFindExportedNestedTypeInType => ResourceManager.GetString("CouldNotFindExportedNestedTypeInType", null);

        internal static string/*!*/ CouldNotFindExportedTypeInAssembly => ResourceManager.GetString("CouldNotFindExportedTypeInAssembly", null);

        internal static string CouldNotFindExportedTypeInModule => ResourceManager.GetString("CouldNotFindExportedTypeInModule", null);

        internal static string/*!*/ CouldNotFindReferencedModule => ResourceManager.GetString("CouldNotFindReferencedModule", null);

        internal static string/*!*/ CouldNotResolveMemberReference => ResourceManager.GetString("CouldNotResolveMemberReference", null);

        internal static string/*!*/ CouldNotResolveType => ResourceManager.GetString("CouldNotResolveType", null);

        internal static string CouldNotResolveTypeReference => ResourceManager.GetString("CouldNotResolveTypeReference", null);

        internal static string/*!*/ CreateFileMappingReturnedErrorCode => ResourceManager.GetString("CreateFileMappingReturnedErrorCode", null);

        internal static string/*!*/ ENCLogTableEncountered => ResourceManager.GetString("ENCLogTableEncountered", null);

        internal static string/*!*/ ENCMapTableEncountered => ResourceManager.GetString("ENCMapTableEncountered", null);

        internal static string/*!*/ FileTooBig => ResourceManager.GetString("FileTooBig", null);

        internal static string/*!*/ GetReaderForFileReturnedUnexpectedHResult => ResourceManager.GetString("GetReaderForFileReturnedUnexpectedHResult", null);

        internal static string/*!*/ InternalCompilerError => ResourceManager.GetString("InternalCompilerError", null);

        internal static string/*!*/ InvalidBaseClass => ResourceManager.GetString("InvalidBaseClass", null);

        internal static string/*!*/ InvalidFatMethodHeader => ResourceManager.GetString("InvalidFatMethodHeader", null);

        internal static string/*!*/ InvalidLocalSignature => ResourceManager.GetString("InvalidLocalSignature", null);

        internal static string InvalidModuleTable => ResourceManager.GetString("InvalidModuleTable", null);

        internal static string/*!*/ InvalidTypeTableIndex => ResourceManager.GetString("InvalidTypeTableIndex", null);

        internal static string MalformedSignature => ResourceManager.GetString("MalformedSignature", null);

        internal static string/*!*/ MapViewOfFileReturnedErrorCode => ResourceManager.GetString("MapViewOfFileReturnedErrorCode", null);

        internal static string/*!*/ ModuleOrAssemblyDependsOnMoreRecentVersionOfCoreLibrary => ResourceManager.GetString("ModuleOrAssemblyDependsOnMoreRecentVersionOfCoreLibrary", null);

        internal static string/*!*/ ModuleError => ResourceManager.GetString("ModuleError", null);

        internal static string/*!*/ NoMetadataStream => ResourceManager.GetString("NoMetadataStream", null);

        internal static string/*!*/ PdbAssociatedWithFileIsOutOfDate => ResourceManager.GetString("PdbAssociatedWithFileIsOutOfDate", null);

        internal static string/*!*/ SecurityAttributeTypeDoesNotHaveADefaultConstructor => ResourceManager.GetString("SecurityAttributeTypeDoesNotHaveADefaultConstructor", null);

        internal static string/*!*/ TooManyMethodHeaderSections => ResourceManager.GetString("TooManyMethodHeaderSections", null);

        internal static string/*!*/ UnexpectedTypeInCustomAttribute => ResourceManager.GetString("UnexpectedTypeInCustomAttribute", null);

        internal static string/*!*/ UnknownConstantType => ResourceManager.GetString("UnknownConstantType", null);

        internal static string/*!*/ UnknownOpCode => ResourceManager.GetString("UnknownOpCode", null);

        internal static string/*!*/ UnknownOpCodeEncountered => ResourceManager.GetString("UnknownOpCodeEncountered", null);

        internal static string/*!*/ UnknownVirtualAddress => ResourceManager.GetString("UnknownVirtualAddress", null);

        internal static string/*!*/ UnsupportedTableEncountered => ResourceManager.GetString("UnsupportedTableEncountered", null);

        internal static string/*!*/ InvalidAssemblyStrongName => ResourceManager.GetString("InvalidAssemblyStrongName", null);

        internal static string/*!*/ KeyNeedsToBeGreaterThanZero => ResourceManager.GetString("KeyNeedsToBeGreaterThanZero", null);
    }
}
