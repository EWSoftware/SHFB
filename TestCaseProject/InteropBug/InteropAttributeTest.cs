using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TestDoc.InteropAttributesBug
{
    /// <summary>
    /// Demonstrate bug with certain System.Runtime.InteropServices attributes.  Fixed in Sandcastle 2.7.5.0.
    /// </summary>
    /// <remarks>Certain System.Runtime.InteropServices attributes will not show up in the reflection information
    /// even when the attribute filter is completely removed.</remarks>
    [ProgId("TestDoc.InteropAttributeTest")]
    public class InteropAttributeTest
    {
        /// <summary>
        /// Test PreserveSig on a method
        /// </summary>
        [PreserveSig]
        public static extern void Test();

        /// <summary>
        /// Test the other DllImport attribute settings to make sure they appear in the syntax section
        /// </summary>
        /// <param name="lpszUrlSearchPattern">URL search pattern</param>
        /// <param name="lpFirstCacheEntryInfo">First cache entry info</param>
        /// <param name="lpdwFirstCacheEntryInfoBufferSize">First cache entry info buffer size</param>
        /// <returns>IntPtr</returns>
        [DllImport("wininetXYZ", BestFitMapping = false, ExactSpelling = true, ThrowOnUnmappableChar = true,
            CallingConvention = CallingConvention.Cdecl, PreserveSig = false)]
        public static extern IntPtr ThisFunctionDoesNotExist(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszUrlSearchPattern,
            IntPtr lpFirstCacheEntryInfo,
            ref int lpdwFirstCacheEntryInfoBufferSize);

        /// <summary>
        /// This begins the enumeration of the Internet cache
        /// </summary>
        /// <param name="lpszUrlSearchPattern">URL search pattern</param>
        /// <param name="lpFirstCacheEntryInfo">First cache entry info</param>
        /// <param name="lpdwFirstCacheEntryInfoBufferSize">First cache entry info buffer size</param>
        /// <returns>IntPtr</returns>
        [DllImport("wininet", SetLastError = true, CharSet = CharSet.Auto,
          EntryPoint = "FindFirstUrlCacheEntryA")]
        public static extern IntPtr FindFirstUrlCacheEntry(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszUrlSearchPattern,
            IntPtr lpFirstCacheEntryInfo,
            ref int lpdwFirstCacheEntryInfoBufferSize);

        /// <summary>
        /// This retrieves the next entry in the Internet cache
        /// </summary>
        /// <param name="hFind">Find handle</param>
        /// <param name="lpNextCacheEntryInfo">Next cache entry info</param>
        /// <param name="lpdwNextCacheEntryInfoBufferSize">Next cache entry info buffer size</param>
        /// <returns>Boolean</returns>
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
        /// <summary>Structure size</summary>
        [FieldOffset(0)]
        public uint dwStructSize;
        /// <summary>Source URL name</summary>
        [FieldOffset(4)]
        public IntPtr lpszSourceUrlName;
        /// <summary>Local filename</summary>
        [FieldOffset(8)]
        public IntPtr lpszLocalFileName;
        /// <summary>Cache entry type</summary>
        [FieldOffset(12)]
        public uint CacheEntryType;
        /// <summary>Use count</summary>
        [FieldOffset(16)]
        public uint dwUseCount;
        /// <summary>Hit rate</summary>
        [FieldOffset(20)]
        public uint dwHitRate;
        /// <summary>Size low</summary>
        [FieldOffset(24)]
        public uint dwSizeLow;
        /// <summary>Size high</summary>
        [FieldOffset(28)]
        public uint dwSizeHigh;
        /// <summary>Last modified time</summary>
        [FieldOffset(32)]
        public System.Runtime.InteropServices.ComTypes.FILETIME LastModifiedTime;
        /// <summary>Expire time</summary>
        [FieldOffset(40)]
        public System.Runtime.InteropServices.ComTypes.FILETIME ExpireTime;
        /// <summary>Last access time</summary>
        [FieldOffset(48)]
        public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        /// <summary>Last sync time</summary>
        [FieldOffset(56)]
        public System.Runtime.InteropServices.ComTypes.FILETIME LastSyncTime;
        /// <summary>Header info</summary>
        [FieldOffset(64)]
        public IntPtr lpHeaderInfo;
        /// <summary>Header info size</summary>
        [FieldOffset(68)]
        public uint dwHeaderInfoSize;
        /// <summary>File extension</summary>
        [FieldOffset(72)]
        public IntPtr lpszFileExtension;
        /// <summary>Reserved</summary>
        [FieldOffset(76)]
        public uint dwReserved;
        /// <summary>Exempt delta</summary>
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
        /// <summary>Get next assembly</summary>
        /// <param name="pvReserved">Reserved</param>
        /// <param name="ppName">Name</param>
        /// <param name="dwFlags">Flags</param>
        /// <returns><para><see cref="int"/></para></returns>
        [PreserveSig]
        int GetNextAssembly(IntPtr pvReserved, out /*IAssemblyName*/ object ppName, uint dwFlags);

        /// <summary>Reset</summary>
        /// <returns>Int</returns>
        [PreserveSig]
        int Reset();

        /// <summary>Clone</summary>
        /// <param name="ppEnum">Assembly enum</param>
        /// <returns>int</returns>
        [PreserveSig]
        int Clone(out IAssemblyEnum ppEnum);
    }

    /// <summary>
    /// This is a fake PIA (Primary Interop Assembly) embedded interop type used to test the visibility filter.
    /// </summary>
    [CompilerGenerated, TypeIdentifier]
    public interface FakeNoPIAType
    {
        /// <summary>
        /// Test method 1
        /// </summary>
        [DispId(1)]
        void Method1();

        /// <summary>
        /// Test method 2
        /// </summary>
        /// <param name="x">Parameter X</param>
        /// <param name="y">Parameter Y</param>
        [DispId(2)]
        void Method2(int x, string y);
    }
}
