//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : BuildComponentFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/27/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an abstract base class that defines the factory method for build components as well as
// designer and build tool interaction methods.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/23/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;

using Sandcastle.Core.UI;

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This is an abstract base class that defines the factory method for build components as well as designer
    /// and build tool interaction methods.
    /// </summary>
    /// <remarks>Build components are non-shared and instances are created as needed</remarks>
    public abstract class BuildComponentFactory
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to import the build assembler instance that is passed to the build component when it
        /// is created.
        /// </summary>
        /// <value>This may be null when used in a designer or configuration tool which only needs access to the
        /// designer methods on the factory class.</value>
        [Import(typeof(BuildAssemblerCore), AllowDefault = true)]
        protected BuildAssemblerCore BuildAssembler { get; set; }

        /// <summary>
        /// This read-only property can be overridden to provide a list of build components on which this
        /// component depends.
        /// </summary>
        /// <value>If the indicated components do not exist in the project settings or in the configuration file
        /// already, the build tool can use this information to add them automatically with a default
        /// configuration.  It returns an empty list by default.</value>
        public virtual IEnumerable<string> Dependencies
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// This is used to get or set a placement action for reference content builds
        /// </summary>
        /// <value>The default is to not place the component.  Components that are exposed to designers and
        /// build tools should set this to define a proper placement action.</value>
        public ComponentPlacement ReferenceBuildPlacement { get; set; }

        /// <summary>
        /// This is used to get or set a placement action for conceptual content builds
        /// </summary>
        /// <value>The default is to not place the component.  Components that are exposed to designers and
        /// build tools should set this to define a proper placement action.</value>
        public ComponentPlacement ConceptualBuildPlacement { get; set; }

        /// <summary>
        /// This read-only property can be overridden to define a default configuration for the build component
        /// </summary>
        /// <value>It returns an empty string by default</value>
        public virtual string DefaultConfiguration
        {
            get { return String.Empty; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        protected BuildComponentFactory()
        {
            this.ReferenceBuildPlacement = new ComponentPlacement();
            this.ConceptualBuildPlacement = new ComponentPlacement();
        }
        #endregion

        #region Abstract methods
        //=====================================================================

        /// <summary>
        /// This is implemented to provide a build component factory
        /// </summary>
        /// <returns>A new instance of a build component</returns>
        public abstract BuildComponentCore Create();

        #endregion

        #region Designer and configuration tool interaction methods
        //=====================================================================

        /// <summary>
        /// This method can be overridden to let designers and configuration tools allow the user to edit the
        /// component configuration.
        /// </summary>
        /// <param name="currentConfiguration">A string containing the current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The base implementation uses a generic editor dialog that edits the XML as text</remarks>
        public virtual string ConfigureComponent(string currentConfiguration)
        {
            using(var dlg = new ConfigurationEditorDlg())
            {
                dlg.Configuration = currentConfiguration;

                if(dlg.ShowDialog() == DialogResult.OK)
                    currentConfiguration = dlg.Configuration;
            }

            return currentConfiguration;
        }
        #endregion
    }
}
