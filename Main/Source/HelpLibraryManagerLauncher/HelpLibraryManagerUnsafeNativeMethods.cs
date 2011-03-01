//=============================================================================
// System  : Help Library Manager Launcher
// File    : HelpLibraryManagerUnsafeNativeMethods.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/27/2011
// Note    : Copyright 2010-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an internal class used to call some Win32 API functions.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.2.0  02/27/2011  EFW  Created the code
//=============================================================================

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace SandcastleBuilder.MicrosoftHelpViewer
{
    /// <summary>
    /// This internal class is used for access to some Win32 API functions.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static partial class UnsafeNativeMethods
    {
        #region Registry access definitions and external methods
        //=====================================================================

        private static readonly UIntPtr HKEY_LOCAL_MACHINE = (UIntPtr)0x80000002; // Local Machine key
        private const int KEY_WOW64_64KEY = 0x100;  // Access the 64-bit registry
        private const int KEY_WOW64_32KEY = 0x200;  // Access the 32-bit registry
        private const int KEY_READ = 0x20019;       // Read only access

        // Win32 API registry access methods
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyExW", SetLastError = true)]
        private static extern int RegOpenKeyEx(UIntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out UIntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        private static extern int RegQueryValueEx(UIntPtr hKey, string lpValueName, IntPtr lpReserved, out uint lpType,
            System.Text.StringBuilder lpData, ref uint lpcbData);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(UIntPtr hKey);
        #endregion

        #region Registry helper method
        //=====================================================================

        /// <summary>
        /// This is used to try and find the specified regsitry key value in the 64 bit part
        /// of the registry. If not found, an attempt is made to try and find it in the 32 bit
        /// part of the registry.
        /// </summary>
        /// <param name="registryKeyPath">The registry key path to find</param>
        /// <param name="valueName">The value name to find</param>
        /// <returns>The value in the named registry key value</returns>
        /// <remarks>This method uses an API call that is unsupported on Windows 2000.</remarks>
        internal static string GetRegistryValue(string registryKeyPath, string valueName)
        {
            UIntPtr regKeyHandle = UIntPtr.Zero;
            string value = null;
            uint size = 4096;
            uint type;
            StringBuilder keyBuffer = new StringBuilder((int)size);

            if(String.IsNullOrEmpty(registryKeyPath))
                throw new ArgumentNullException("registryKeyPath", "registryKeyPath cannot be null or empty");

            if(String.IsNullOrEmpty(valueName))
                throw new ArgumentNullException("valueName", "valueName cannot be null or empty");

            try
            {
                // See if the registry key can be found in the 64 bit registry
                if(RegOpenKeyEx(HKEY_LOCAL_MACHINE, registryKeyPath, 0, KEY_READ | KEY_WOW64_64KEY,
                  out regKeyHandle) == 0)
                {
                    // See if the value exists
                    if(RegQueryValueEx(regKeyHandle, valueName, IntPtr.Zero, out type, keyBuffer, ref size) == 0)
                        value = keyBuffer.ToString();
                }
                else
                {
                    // See if the registry key can be found in the 32 bit registry
                    if(RegOpenKeyEx(HKEY_LOCAL_MACHINE, registryKeyPath, 0, KEY_READ | KEY_WOW64_32KEY,
                      out regKeyHandle) == 0)
                    {
                        // See if the value exists
                        if(RegQueryValueEx(regKeyHandle, valueName, IntPtr.Zero, out type, keyBuffer, ref size) == 0)
                            value = keyBuffer.ToString();
                    }
                }
            }
            finally
            {
                if(regKeyHandle != UIntPtr.Zero)
                    RegCloseKey(regKeyHandle);
            }

            return value;
        }

        #endregion
    }
}
