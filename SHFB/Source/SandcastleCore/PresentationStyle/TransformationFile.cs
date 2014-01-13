//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TransformationFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/04/2014
// Note    : Copyright 2012-2014, Eric Woodruff, All rights reserved
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
// -------  01/04/2014  EFW  Moved the code into Sandcastle.Core
//===============================================================================================================

using System.Collections.Generic;

namespace Sandcastle.Core.PresentationStyle
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

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="transformationFile">The transformation file to use</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        public TransformationFile(string transformationFile)
        {
            this.TransformationFilename = transformationFile;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="transformationFile">The transformation file to use</param>
        /// <param name="parameters">An optional set of key/value parameters for the transformation file</param>
        public TransformationFile(string transformationFile, IDictionary<string, string> parameters) : base(parameters)
        {
            this.TransformationFilename = transformationFile;
        }
        #endregion
    }
}
