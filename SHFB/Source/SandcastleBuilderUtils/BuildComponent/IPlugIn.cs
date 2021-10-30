//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : IPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/16/2021
// Note    : Copyright 2007-2021, Eric Woodruff, All rights reserved
//
// This file contains a plug-in interface definition used to implement a Sandcastle Help File Builder build
// process plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/09/2007  EFW  Created the code
// 08/12/2008  EFW  Modified to support the new project format
// 03/07/2013  EFW  Added SupportsConfiguration property
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Xml.Linq;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This interface defines the methods required to implement a Sandcastle Help File Builder build process
    /// plug-in.
    /// </summary>
    /// <remarks>Derived classes must also implement the <see cref="IDisposable"/> interface.  Plug-ins are
    /// singletons in nature.  The composition container will create instances as needed and will dispose of them
    /// when the container is disposed of.</remarks>
    public interface IPlugIn : IDisposable
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns an enumerable list of execution points that define when the plug-in
        /// should be invoked during the build process.
        /// </summary>
        IEnumerable<ExecutionPoint> ExecutionPoints { get; }

        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        void Initialize(BuildProcess buildProcess, XElement configuration);

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        void Execute(ExecutionContext context);

        #endregion
    }
}
