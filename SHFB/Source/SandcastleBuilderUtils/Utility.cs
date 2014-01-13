//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : Utility.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/02/2014
// Note    : Copyright 2011-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a utility class with extension and utility methods.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.3.3  12/15/2011  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sandcastle.Core;
using Sandcastle.Core.Frameworks;

using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.BuildComponent;

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
                path = Path.Combine(ComponentUtilities.ToolsFolder, @"Help\SandcastleBuilder.chm");

                // It may not be there in development builds so look in the release folder.  If still not found,
                // just ignore it.
                if(!File.Exists(path))
                {
                    path = Path.Combine(Environment.GetFolderPath(Environment.Is64BitProcess ?
                        Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles),
                        @"EWSoftware\Sandcastle Help File Builder\Help\SandcastleBuilder.chm");

                    if(!File.Exists(path))
                        return;
                }

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

        #region Plug-in execution point extension methods
        //=====================================================================

        /// <summary>
        /// This is used to determine if the enumerable list of execution points contains an entry for the
        /// specified build step and behavior.
        /// </summary>
        /// <param name="executionPoints">An enumerable list of execution points to check</param>
        /// <param name="step">The build step</param>
        /// <param name="behavior">The behavior</param>
        /// <returns>True if the enumerable list of execution points contains an entry for the specified build
        /// step and behavior or false if it does not.</returns>
        public static bool RunsAt(this IEnumerable<ExecutionPoint> executionPoints, BuildStep step,
          ExecutionBehaviors behavior)
        {
            return executionPoints.Any(p => p.BuildStep == step && (p.Behavior & behavior) != 0);
        }

        /// <summary>
        /// This is used to obtain the execution priority for a plug-in in the given build step and behavior
        /// </summary>
        /// <param name="executionPoints">An enumerable list of execution points to search</param>
        /// <param name="step">The build step</param>
        /// <param name="behavior">The behavior</param>
        /// <returns>The execution priority is used to determine the order in which the plug-ins will be
        /// executed.  Those with a higher priority value will be executed before those with a lower value.
        /// Those with an identical priority may be executed in any order within their group.</returns>
        public static int PriorityFor(this IEnumerable<ExecutionPoint> executionPoints, BuildStep step,
          ExecutionBehaviors behavior)
        {
            var point = executionPoints.FirstOrDefault(p => p.BuildStep == step && (p.Behavior & behavior) != 0);

            if(point != null)
                return point.Priority;

            return ExecutionPoint.DefaultPriority;
        }
        #endregion

        #region Project conversion helper methods
        //=====================================================================

        /// <summary>
        /// This is used to convert old SHFB project framework version values to the new framework version values
        /// </summary>
        /// <param name="oldValue">The old value to convert</param>
        /// <returns>The equivalent new value</returns>
        internal static string ConvertFromOldValue(string oldValue)
        {
            FrameworkSettings fs = null;

            if(String.IsNullOrWhiteSpace(oldValue))
                return FrameworkDictionary.DefaultFrameworkTitle;

            oldValue = oldValue.Trim();

            if(oldValue.IndexOf(".NET ", StringComparison.OrdinalIgnoreCase) != -1 || Char.IsDigit(oldValue[0]))
            {
                oldValue = oldValue.ToUpperInvariant().Replace(".NET ", String.Empty).Trim();

                if(oldValue.Length == 0)
                    oldValue = "4.0";
                else
                    if(oldValue.Length > 3)
                        oldValue = oldValue.Substring(0, 3);

                oldValue = ".NET Framework " + oldValue;
            }
            else
                if(oldValue.IndexOf("Silverlight ", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    oldValue = oldValue.ToUpperInvariant().Trim();

                    if(oldValue.EndsWith(".0", StringComparison.Ordinal))
                        oldValue = oldValue.Substring(0, oldValue.Length - 2);
                }
                else
                    if(oldValue.IndexOf("Portable ", StringComparison.OrdinalIgnoreCase) != -1)
                        oldValue = ".NET Portable Library 4.0 (Legacy)";

            // If not found, default to .NET 4.0
            if(!FrameworkDictionary.AllFrameworks.TryGetValue(oldValue, out fs))
                return FrameworkDictionary.DefaultFrameworkTitle;

            return fs.Title;
        }
        #endregion
    }
}
