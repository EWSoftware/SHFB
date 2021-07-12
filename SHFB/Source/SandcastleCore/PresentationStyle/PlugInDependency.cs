//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PlugInDependency.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/06/2021
// Note    : Copyright 2014-2021, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to define a plug-in dependency for a presentation style
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/12/2014  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.PresentationStyle
{
    /// <summary>
    /// This class is used to define a plug-in upon which a presentation style depends and the default
    /// configuration to use for it.
    /// </summary>
    public class PlugInDependency
    {
        /// <summary>
        /// The ID of the plug-in upon which the presentation style depends
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The default configuration to use for the plug-in
        /// </summary>
        /// <remarks>This can be either an empty <c>configuration</c> element or one filled in with default
        /// values for any plug-in configuration parameters.  If the plug-in is visible to the user and has been
        /// added to the project, the project configuration will override this one.</remarks>
        public string Configuration { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The ID of the plug-in depended upon</param>
        /// <param name="configuration">The default configuration.  If null or empty, an empty
        /// <c>configuration</c> element will be used.</param>
        public PlugInDependency(string id, string configuration)
        {
            this.Id = id;
            this.Configuration = String.IsNullOrWhiteSpace(configuration) ? "<configuration />" : configuration;
        }
    }
}
