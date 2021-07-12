//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IPlugInConfigurationEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/28/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains a class that defines the interface used to edit a plug-in configuration
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/26/2021  EFW  Created the code
//===============================================================================================================

using System.Xml.Linq;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This class defines the interface used to edit a plug-in configuration
    /// </summary>
    public interface IPlugInConfigurationEditor
    {
        /// <summary>
        /// This method is used to edit a plug-in configuration
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="configuration">An XML element containing the current configuration settings</param>
        /// <returns>True if the configuration element was updated, false if not</returns>
        bool EditConfiguration(SandcastleProject project, XElement configuration);
    }
}
