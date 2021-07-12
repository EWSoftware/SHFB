//===============================================================================================================
// System  : Sandcastle Tools - Windows platform specific code
// File    : UiUtility.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/16/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains a utility class with extension and utility methods
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/14/2021  EFW  Moved the code to the Windows platform assembly from SandcastleBuilder.Utils
//===============================================================================================================

using System;
using System.Windows.Forms;

using Sandcastle.Core;

namespace Sandcastle.Platform.Windows
{
    /// <summary>
    /// This class contains utility and extension methods
    /// </summary>
    public static class UiUtility
    {
        #region General utility methods
        //=====================================================================

        /// <summary>
        /// Show a help topic by opening the SHFB help website
        /// </summary>
        /// <param name="topic">The topic ID to display (will be formatted as "html/[Topic_ID].htm")</param>
        public static void ShowHelpTopic(string topic)
        {
            string anchor = String.Empty;

            if(String.IsNullOrEmpty(topic))
                throw new ArgumentException("A topic must be specified", nameof(topic));

            try
            {
                // If there's an anchor, split it off
                int pos = topic.IndexOf('#');

                if(pos != -1)
                {
                    anchor = topic.Substring(pos);
                    topic = topic.Substring(0, pos);
                }

                System.Diagnostics.Process.Start($"https://ewsoftware.github.io/SHFB/html/{topic}.htm{anchor}");
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Unable to view online help\r\n\r\nReason: {ex.Message}",
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion
    }
}
