//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : IPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/12/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in interface definition used to implement a build
// process plug-in for the Sandcastle Help File Builder.
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
// 1.8.0.0  08/12/2008  EFW  Modified to support the new project format
//=============================================================================

using System;
using System.Xml.XPath;

using SandcastleBuilder.Utils.BuildEngine;

// All classes go in the SandcastleBuilder.Utils.PlugIn namespace
namespace SandcastleBuilder.Utils.PlugIn
{
    /// <summary>
    /// This interface defines the methods required to implement a build
    /// process plug-in for the Sandcastle Help File Builder.
    /// </summary>
    /// <remarks>Derived classes must also implement the
    /// <see cref="IDisposable"/> methods.</remarks>
    public interface IPlugIn : IDisposable
    {
        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This read-only property returns a friendly name for the plug-in
        /// </summary>
        string Name { get; }

        /// <summary>
        /// This read-only property returns the version of the plug-in
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// This read-only property returns the copyright information for the
        /// plug-in.
        /// </summary>
        string Copyright { get; }

        /// <summary>
        /// This read-only property returns a brief description of the plug-in
        /// </summary>
        string Description { get; }

        /// <summary>
        /// This read-only property returns true if the plug-in should run in
        /// a partial build or false if it should not.
        /// </summary>
        /// <value>If this returns false, the plug-in will not be loaded when
        /// a partial build is performed.</value>
        bool RunsInPartialBuild { get; }

        /// <summary>
        /// This read-only property returns a collection of execution points
        /// that define when the plug-in should be invoked during the build
        /// process.
        /// </summary>
        ExecutionPointCollection ExecutionPoints { get; }
        #endregion

        #region Methods
        //=====================================================================
        // Methods

        /// <summary>
        /// This method is used by the Sandcastle Help File Builder to let the
        /// plug-in perform its own configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file
        /// builder project.</remarks>
        string ConfigurePlugIn(SandcastleProject project, string currentConfig);

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the
        /// build process.
        /// </summary>
        /// <param name="buildProcess">A reference to the current build
        /// process.</param>
        /// <param name="configuration">The configuration data that the plug-in
        /// should use to initialize itself.</param>
        void Initialize(BuildProcess buildProcess, XPathNavigator configuration);

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        void Execute(ExecutionContext context);
        #endregion
    }
}
