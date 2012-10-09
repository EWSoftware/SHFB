//=============================================================================
// System  : Sandcastle Help File Builder
// File    : UnsafeNativeMethods.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/27/2011
// Note    : Copyright 2007-2011, Eric Woodruff, All rights reserved
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
// 1.5.0.2  07/03/2007  EFW  Created the code
//=============================================================================

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace SandcastleBuilder.Gui
{
	/// <summary>
    /// This internal class is used for access to some Win32 API functions.
	/// </summary>
    /// <remarks>The window placement functions are used so as to support
    /// restoring the window on multi-monitor systems.</remarks>
    [SuppressUnmanagedCodeSecurity]
	internal static class UnsafeNativeMethods
	{
        /// <summary>Show window in normal position</summary>
        internal const int SW_SHOWNORMAL = 1;
        /// <summary>Show window minimized</summary>
        internal const int SW_SHOWMINIMIZED = 2;

        /// <summary>
        /// Get the window's placement information
        /// </summary>
        /// <param name="hWnd">The window handle for which to get information</param>
        /// <param name="placement">The placement information structure</param>
        /// <returns>True on success, false on failure</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd,
            out WINDOWPLACEMENT placement);

        /// <summary>
        /// Set the window's placement information
        /// </summary>
        /// <param name="hWnd">The window handle for which to set the information</param>
        /// <param name="placement">The placement information structure</param>
        /// <returns>True if successful, false on failure</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPlacement(IntPtr hWnd,
            [In] ref WINDOWPLACEMENT placement);

        /// <summary>
        /// Convert a character value to a virtual-key code and shift state
        /// for the current keyboard.
        /// </summary>
        /// <param name="ch">The character to translate</param>
        /// <returns>A short containing the virtual-key code in the low order
        /// byte and the shift state in the high order byte.</returns>
        [DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);
 
        /// <summary>
        /// This is used to convert a <see cref="Char" /> value to a
        /// <see cref="Keys" /> value.
        /// </summary>
        /// <param name="ch">The character to convert</param>
        /// <returns>The <see cref="Keys" /> value corresponding to the
        /// character.  Note that not all keys are represented in the
        /// <b>Keys</b> enumeration.  Some may return rather odd combinations
        /// or no actual enumerated value at all.</returns>
        internal static Keys CharToKeys(char ch)
        {
            short scanCode = VkKeyScan(ch);
            return (Keys)(((scanCode & 0xFF00) << 8) | (scanCode & 0x00FF));
        }
     }
}
