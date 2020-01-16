//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : JavaScriptSerializer.cs
// Author  : J. Ritchie Carroll  (rcarroll@gmail.com)
// Updated : 02/05/2016
// Note    : Copyright 2007-2020, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file Provides proxy serialization and deserialization functionality.
//
// Design Decision:
//    This class does not exist in .NET core, so this just proxies functionality to Newtonsoft.Json. If the
//    JsonConvert function works well then original code can drop usages of the JavaScriptSerializer and delete
//    this class.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/15/2020  JRC  Created the code
//
//===============================================================================================================

using Newtonsoft.Json;

namespace System.Web.Script.Serialization
{
    /// <summary>
    /// Provides proxy serialization and deserialization functionality.
    /// </summary>
    public class JavaScriptSerializer
    {
        /// <summary>
        /// Gets or sets the maximum length of JSON strings that are accepted by the <see cref="JavaScriptSerializer"/> class.
        /// </summary>
        public Int32 MaxJsonLength { get; set; } = Int32.MaxValue;

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The serialized JSON string.</returns>
        public string Serialize (object obj)
        {
            string result = JsonConvert.SerializeObject(obj);
            return result.Length > this.MaxJsonLength ? result.Substring(0, this.MaxJsonLength) : result;
        }
    }
}
