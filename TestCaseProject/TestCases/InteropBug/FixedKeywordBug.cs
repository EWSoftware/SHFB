using System.Runtime.InteropServices;

namespace TestDoc.InteropBug
{
    /// <summary>
    /// Win32 structure for a device
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WinDeviceInfo
    {
        // The syntax generators in the June 2010 and earlier releases don't handle this correctly.
        // They need updating to look for the FixedBufferAttribute and handle the formatting accordingly.

        /// <summary>
        /// Profile name
        /// </summary>
        public fixed char Profile[32];
    }
}
