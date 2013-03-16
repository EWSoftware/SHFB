//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PlugInInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/07/2013
// Note    : Copyright 2007-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to provide information about the loaded plug-ins
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.5.2.0  09/09/2007  EFW  Created the code
// 1.9.7.0  03/07/2013  EFW  Added SupportsConfiguration property
//===============================================================================================================

using System;

namespace SandcastleBuilder.Utils.PlugIn
{
    /// <summary>
    /// This class contains information about the loaded plug-ins
    /// </summary>
    public class PlugInInfo
    {
        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This read-only property returns the name of the plug-in
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This read-only property returns the copyright information for the plug-in
        /// </summary>
        public string Copyright { get; private set; }

        /// <summary>
        /// This read-only property returns a description of the plug-in
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// This read-only property returns the version number of the plug-in
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// This read-only property returns type information for the plug-in
        /// </summary>
        public Type PlugInType { get; private set; }

        /// <summary>
        /// This read-only property returns whether or not the plug-in supports configuration
        /// </summary>
        public bool SupportsConfiguration { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="t">Type information for the plug-in</param>
        internal PlugInInfo(Type t)
        {
            this.PlugInType = t;

            using(IPlugIn plugIn = (IPlugIn)Activator.CreateInstance(t))
            {
                this.Name = plugIn.Name;
                this.Copyright = plugIn.Copyright;
                this.Description = plugIn.Description;
                this.Version = plugIn.Version;
                this.SupportsConfiguration = plugIn.SupportsConfiguration;
            }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Get a new instance of the plug-in
        /// </summary>
        /// <returns>A new instance of the plug-in</returns>
        public IPlugIn NewInstance()
        {
            return (IPlugIn)Activator.CreateInstance(this.PlugInType);
        }
        #endregion
    }
}
