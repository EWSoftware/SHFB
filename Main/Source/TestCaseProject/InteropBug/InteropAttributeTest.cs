using System;
using System.Runtime.InteropServices;

namespace TestDoc.InteropAttributesBug
{
    /// <summary>
    /// Demonstrate bug with certain System.Runtime.InteropServices
    /// attributes.
    /// </summary>
    /// <remarks>Certain System.Runtime.InteropServices attributes will not
    /// show up in the reflection information even when the attribute filter
    /// is completely removed.</remarks>
    public class InteropAttributeTest
    {
        /// <summary>
        /// This begins the enumeration of the Internet cache
        /// </summary>
        [DllImport("wininet", SetLastError = true, CharSet = CharSet.Auto,
          EntryPoint = "FindFirstUrlCacheEntryA")]
        public static extern IntPtr FindFirstUrlCacheEntry(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszUrlSearchPattern,
            IntPtr lpFirstCacheEntryInfo,
            ref int lpdwFirstCacheEntryInfoBufferSize);

        /// <summary>
        /// This retrieves the next entry in the Internet cache
        /// </summary>
        [DllImport("wininet", SetLastError = true, CharSet = CharSet.Auto,
          EntryPoint = "FindNextUrlCacheEntryA")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindNextUrlCacheEntry(IntPtr hFind,
            IntPtr lpNextCacheEntryInfo,
            ref int lpdwNextCacheEntryInfoBufferSize);
    }

    /// <summary>
    /// Another case where the attributes don't show up
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    public struct INTERNET_CACHE_ENTRY_INFOA
    {
        /// <summary></summary>
        [FieldOffset(0)]
        public uint dwStructSize;
        /// <summary></summary>
        [FieldOffset(4)]
        public IntPtr lpszSourceUrlName;
        /// <summary></summary>
        [FieldOffset(8)]
        public IntPtr lpszLocalFileName;
        /// <summary></summary>
        [FieldOffset(12)]
        public uint CacheEntryType;
        /// <summary></summary>
        [FieldOffset(16)]
        public uint dwUseCount;
        /// <summary></summary>
        [FieldOffset(20)]
        public uint dwHitRate;
        /// <summary></summary>
        [FieldOffset(24)]
        public uint dwSizeLow;
        /// <summary></summary>
        [FieldOffset(28)]
        public uint dwSizeHigh;
        /// <summary></summary>
        [FieldOffset(32)]
        public System.Runtime.InteropServices.ComTypes.FILETIME LastModifiedTime;
        /// <summary></summary>
        [FieldOffset(40)]
        public System.Runtime.InteropServices.ComTypes.FILETIME ExpireTime;
        /// <summary></summary>
        [FieldOffset(48)]
        public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        /// <summary></summary>
        [FieldOffset(56)]
        public System.Runtime.InteropServices.ComTypes.FILETIME LastSyncTime;
        /// <summary></summary>
        [FieldOffset(64)]
        public IntPtr lpHeaderInfo;
        /// <summary></summary>
        [FieldOffset(68)]
        public uint dwHeaderInfoSize;
        /// <summary></summary>
        [FieldOffset(72)]
        public IntPtr lpszFileExtension;
        /// <summary></summary>
        [FieldOffset(76)]
        public uint dwReserved;
        /// <summary></summary>
        [FieldOffset(76)]
        public uint dwExemptDelta;
    }

    /// <summary>
    /// Certain attributes will show up such as Guid and InterfaceType.
    /// ComImport and PreserveSig do not.
    /// </summary>
    [ComImport, Guid("21B8916C-F28E-11D2-A473-00C04F8EF448"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssemblyEnum
    {
        /// <summary></summary>
        [PreserveSig]
        int GetNextAssembly(IntPtr pvReserved, out /*IAssemblyName*/ object ppName,
            uint dwFlags);

        /// <summary></summary>
        [PreserveSig]
        int Reset();

        /// <summary></summary>
        [PreserveSig]
        int Clone(out IAssemblyEnum ppEnum);
    }
}
