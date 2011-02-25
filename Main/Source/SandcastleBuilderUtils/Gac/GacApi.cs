//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : GacApi.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/02/2006
// Note    : Copyright 2006, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a minimal implementation of the Fusion GAC API based on
// information found in the Microsoft KB article KB317540.  It provides basic
// support for enumerating the GAC to provide support for the Sandcastle Help
// File Builder features that need it.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.2.0.0  08/04/2006  EFW  Created the code
//=============================================================================

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace SandcastleBuilder.Utils.Gac
{
    /// <summary>
    /// This defines the different cache types that can be used
    /// </summary>
    /// <exclude/>
    [Flags]
    internal enum ASM_CACHE_FLAGS
    {
        ASM_CACHE_ZAP      = 0x01,
        ASM_CACHE_GAC      = 0x02,
        ASM_CACHE_DOWNLOAD = 0x04
    }

    /// <summary>
    /// This defines the items that GetDisplayName returns
    /// </summary>
    /// <exclude/>
    [Flags]
    internal enum ASM_DISPLAY_FLAGS
    {
        ALL                   = 0x00,
        VERSION               = 0x01,
        CULTURE               = 0x02,
        PUBLIC_KEY_TOKEN      = 0x04,
        PUBLIC_KEY            = 0x08,
        CUSTOM                = 0x10,
        PROCESSORARCHITECTURE = 0x20,
        LANGUAGEID            = 0x40
    }

    /// <summary>
    /// This defines the parts to use when comparing assembly information
    /// </summary>
    /// <exclude/>
    [Flags]
    internal enum ASM_CMP_FLAGS
    {
        NAME             = 0x001,
        MAJOR_VERSION    = 0x002,
        MINOR_VERSION    = 0x004,
        BUILD_NUMBER     = 0x008,
        REVISION_NUMBER  = 0x010,
        PUBLIC_KEY_TOKEN = 0x020,
        CULTURE          = 0x040,
        CUSTOM           = 0x080,
        ALL = NAME | MAJOR_VERSION | MINOR_VERSION | REVISION_NUMBER |
            BUILD_NUMBER | PUBLIC_KEY_TOKEN | CULTURE | CUSTOM,
        DEFAULT          = 0x100
    }

    /// <summary>
    /// This defines the various ID values used for assembly properties
    /// </summary>
    /// <exclude/>
    internal enum ASM_NAME
    {
        PUBLIC_KEY,
        PUBLIC_KEY_TOKEN,
        HASH_VALUE,
        NAME,
        MAJOR_VERSION,
        MINOR_VERSION,
        BUILD_NUMBER,
        REVISION_NUMBER,
        CULTURE,
        PROCESSOR_ID_ARRAY,
        OSINFO_ARRAY,
        HASH_ALGID,
        ALIAS,
        CODEBASE_URL,
        CODEBASE_LASTMOD,
        NULL_PUBLIC_KEY,
        NULL_PUBLIC_KEY_TOKEN,
        CUSTOM,
        NULL_CUSTOM,
        MVID
    }

    /// <summary>
    /// This is used to represent an assembly name
    /// </summary>
    /// <exclude/>
    [ComImport, Guid("CD193BC0-B4BC-11D2-9833-00C04FC31D2E"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAssemblyName
    {
        [PreserveSig]
        int SetProperty(ASM_NAME PropertyId, IntPtr pvProperty,
            uint cbProperty);

        [PreserveSig]
        int GetProperty(ASM_NAME PropertyId, IntPtr pvProperty,
            ref uint pcbProperty);

        [PreserveSig]
        int Finalize();

        [PreserveSig]
        int GetDisplayName(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szDisplayName,
            ref uint pccDisplayName, ASM_DISPLAY_FLAGS dwDisplayFlags);

        [PreserveSig]
        int BindToObject(ref Guid refIID,
            [MarshalAs(UnmanagedType.IUnknown)] object pUnkSink,
            [MarshalAs(UnmanagedType.IUnknown)] object pUnkContext,
            [MarshalAs(UnmanagedType.LPWStr)] string szCodeBase,
            long llFlags, IntPtr pvReserved, uint cbReserved, out IntPtr ppv);

        [PreserveSig]
        int GetName(ref uint lpcwBuffer,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwzName);

        [PreserveSig]
        int GetVersion(out uint pdwVersionHi, out uint pdwVersionLow);

        [PreserveSig]
        int IsEqual(IAssemblyName pName, ASM_CMP_FLAGS dwCmpFlags);

        [PreserveSig]
        int Clone(out IAssemblyName pName);
    }

    /// <summary>
    /// This is used to enumerate the assemblies in the GAC
    /// </summary>
    /// <exclude/>
    [ComImport, Guid("21B8916C-F28E-11D2-A473-00C04F8EF448"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAssemblyEnum
    {
        [PreserveSig]
        int GetNextAssembly(IntPtr pvReserved, out IAssemblyName ppName,
            uint dwFlags);

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int Clone(out IAssemblyEnum ppEnum);
    }

    /// <summary>
    /// This defines the one and only method we need in the help file builder
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// This is used to obtain an instance of the assembly enumerator
        /// </summary>
		/// <param name="ppEnum">A reference that will end up containing the
        /// IAssemblyEnum interface.</param>
		/// <param name="pUnkReserved">Reserved.  Must always be null.</param>
		/// <param name="pName">An assembly name that is used to filter the
        /// enumeration.  This can be null to enumerate all assemblies in the
        /// GAC.</param>
		/// <param name="dwFlags">Specify one (and only one) ASM_CACHE_FLAGS
        /// value.</param>
		/// <param name="pvReserved">Reserved.  Must always be null.</param>
        [DllImport("fusion.dll", PreserveSig=false)]
        internal static extern void CreateAssemblyEnum(out IAssemblyEnum ppEnum,
            IntPtr pUnkReserved, IAssemblyName pName, ASM_CACHE_FLAGS dwFlags,
            IntPtr pvReserved);
    }
}
