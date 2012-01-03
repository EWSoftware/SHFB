//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : Utility.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/15/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a utility class with extension and utility methods.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/15/2011  EFW  Created the code
//=============================================================================

using System;
using System.IO;
using System.Windows.Forms;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This class contains utility and extension methods
    /// </summary>
    public static class Utility
    {
        #region General utility methods
        //=====================================================================

        /// <summary>
        /// Show a help topic in the SHFB help file
        /// </summary>
        /// <param name="topic">The topic ID to display (will be formatted as "html/[Topic_ID].htm")</param>
        /// <remarks>Since the standalone GUI already has a Help 1 file, we'll just display the topic
        /// that it contains rather than integrating an MSHC help file into the VS 2010 collection.</remarks>
        public static void ShowHelpTopic(string topic)
        {
            string path = null, anchor = String.Empty;
            int pos;

            if(String.IsNullOrEmpty(topic))
                throw new ArgumentException("A topic must be specified", "topic");

            try
            {
#if DEBUG
                // In debug builds, SHFBROOT points to the .\Debug folder for the SandcastleBuilderGUI project
                path = Path.Combine(@"C:\Program Files (x86)\EWSoftware\Sandcastle Help File Builder\SandcastleBuilder.chm");
#else
                path = Path.Combine(Environment.ExpandEnvironmentVariables("%SHFBROOT%"), "SandcastleBuilder.chm");
#endif
                // If there's an anchor, split it off
                pos = topic.IndexOf('#');

                if(pos != -1)
                {
                    anchor = topic.Substring(pos);
                    topic = topic.Substring(0, pos);
                }

                Form form = new Form();
                form.CreateControl();
                Help.ShowHelp(form, path, HelpNavigator.Topic, "html/" + topic + ".htm" + anchor);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        #endregion
    }
}
