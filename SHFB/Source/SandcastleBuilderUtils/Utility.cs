//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : Utility.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
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
// 12/15/2011  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This class contains utility and extension methods
    /// </summary>
    public static class Utility
    {
        #region Private data members
        //=====================================================================

        // Regular expressions used for encoding detection and parsing
        private static readonly Regex reXmlEncoding = new Regex("^<\\?xml.*?encoding\\s*=\\s*\"(?<Encoding>.*?)\".*?\\?>");

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
        /// This is used to obtain the execution priority for a plug-in within the given build step and behavior
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

        #region Build process helper and extension methods
        //=====================================================================

        /// <summary>
        /// This is used to read in a file using an appropriate encoding method
        /// </summary>
        /// <param name="filename">The file to load</param>
        /// <param name="encoding">Pass the default encoding to use.  On return, it contains the actual encoding
        /// for the file.</param>
        /// <returns>The contents of the file</returns>
        /// <remarks>When reading the file, it uses the default encoding specified but detects the encoding if
        /// byte order marks are present.  In addition, if the template is an XML file and it contains an
        /// encoding identifier in the XML tag, the file is read using that encoding.</remarks>
        public static string ReadWithEncoding(string filename, ref Encoding encoding)
        {
            Encoding fileEnc;
            string content;

            using(StreamReader sr = new StreamReader(filename, encoding, true))
            {
                content = sr.ReadToEnd();
                encoding = sr.CurrentEncoding;
            }

            Match m = reXmlEncoding.Match(content);

            // Re-read an XML file using the correct encoding?
            if(m.Success)
            {
                fileEnc = Encoding.GetEncoding(m.Groups["Encoding"].Value);

                if(fileEnc != encoding)
                {
                    encoding = fileEnc;

                    using(StreamReader sr = new StreamReader(filename, encoding, true))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }

            return content;
        }
        #endregion
    }
}
