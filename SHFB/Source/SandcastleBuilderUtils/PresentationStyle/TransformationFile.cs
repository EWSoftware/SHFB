//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : TransformationFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/24/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to represent an XSL transformation with optional parameters
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.6.0  10/24/2012  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;
using System.Xml.Linq;

namespace SandcastleBuilder.Utils.PresentationStyle
{
    /// <summary>
    /// This class is used to represent an XSL transformation with optional parameters
    /// </summary>
    public class TransformationFile : Dictionary<string, string>
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the transformation filename
        /// </summary>
        public string TransformationFilename { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="transformation">The XML element containing the settings</param>
        internal TransformationFile(XElement transformation)
        {
            this.TransformationFilename = transformation.Attribute("File").Value;

            foreach(var p in transformation.Descendants("Parameter"))
                this.Add(p.Attribute("Name").Value, p.Attribute("Value").Value);
        }
        #endregion
    }
}
