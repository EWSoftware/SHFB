//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PlugInInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/06/2009
// Note    : Copyright 2007-2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to provide information about the loaded
// plug-ins.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/09/2007  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace SandcastleBuilder.Utils.PlugIn
{
    /// <summary>
    /// This class contains information about the loaded plug-ins
    /// </summary>
    public class PlugInInfo
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private string name, copyright, description;
        private Version version;
        private Type plugInType;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This read-only property returns the name of the plug-in
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// This read-only property returns the copyright information for the
        /// plug-in.
        /// </summary>
        public string Copyright
        {
            get { return copyright; }
        }

        /// <summary>
        /// This read-only property returns a description of the plug-in
        /// </summary>
        public string Description
        {
            get { return description; }
        }

        /// <summary>
        /// This read-only property returns the version number of the plug-in
        /// </summary>
        public Version Version
        {
            get { return version; }
        }

        /// <summary>
        /// This read-only property returns type information for the plug-in
        /// </summary>
        public Type PlugInType
        {
            get { return plugInType; }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="t">Type information for the plug-in</param>
        internal PlugInInfo(Type t)
        {
            plugInType = t;

            using(IPlugIn plugIn = (IPlugIn)Activator.CreateInstance(t))
            {
                name = plugIn.Name;
                copyright = plugIn.Copyright;
                description = plugIn.Description;
                version = plugIn.Version;
            }
        }

        /// <summary>
        /// Get a new instance of the plug-in
        /// </summary>
        /// <returns>A new instance of the plug-in</returns>
        public IPlugIn NewInstance()
        {
            return (IPlugIn)Activator.CreateInstance(plugInType);
        }
    }
}
