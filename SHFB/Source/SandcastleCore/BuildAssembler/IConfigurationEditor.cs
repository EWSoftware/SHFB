//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IConfigurationEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/31/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains a class that defines the interface used to edit a build component configuration
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/23/2021  EFW  Created the code
//===============================================================================================================

using System.ComponentModel.Composition.Hosting;
using System.Xml.Linq;

namespace Sandcastle.Core.BuildAssembler
{
    /// <summary>
    /// This class defines the interface used to edit a component configuration
    /// </summary>
    public interface IConfigurationEditor
    {
        /// <summary>
        /// This method is used to edit a build component configuration
        /// </summary>
        /// <param name="configuration">An XML element containing the current configuration settings</param>
        /// <param name="container">A composition container that holds all of the exported components found
        /// by the build tool.  This can be used to locate and work with other components if necessary.</param>
        /// <returns>True if the configuration element was updated, false if not</returns>
        bool EditConfiguration(XElement configuration, CompositionContainer container);
    }
}
