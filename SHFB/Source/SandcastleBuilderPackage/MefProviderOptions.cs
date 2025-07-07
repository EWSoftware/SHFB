//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MefProviderOptions.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/06/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to contain the MEF provider configuration settings
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/08/2014  EFW  Created the code
// 01/09/2015  EFW  Moved options to the Visual Studio user settings store
//===============================================================================================================

using System;

using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace SandcastleBuilder.Package
{
    /// <summary>
    /// This class is used to contain the MEF provider configuration options
    /// </summary>
    /// <remarks>Settings are stored in an XML file in the user's local application data folder and will be used
    /// by all versions of Visual Studio in which the package is installed.  These are separate from the main
    /// package options but are editable using the package options page.  Since these are not directly related to
    /// the package, we don't want to force it to load just to access these few settings.</remarks>
    internal sealed class MefProviderOptions
    {
        #region Private data members
        //=====================================================================

        private readonly IServiceProvider serviceProvider;

        private const string CollectionPath = @"SHFB\MEF Provider Options";

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set whether or not the MAML and XML comments element Go To Definition and tool
        /// tip option is enabled.
        /// </summary>
        /// <value>This is true by default</value>
        public bool EnableGoToDefinition { get; set; }

        /// <summary>
        /// Related to the above, if enabled, Ctrl+clicking on a target will invoke the Go To Definition option
        /// </summary>
        /// <value>False by default.  This can be enabled if you would like to use Ctrl+Click in addition to
        /// the context menu Go To Definition command (bound to the F12 key by default).</value>
        public bool EnableCtrlClickGoToDefinition { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MefProviderOptions(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            if(!LoadConfiguration())
                EnableGoToDefinition = EnableCtrlClickGoToDefinition = true;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to load the MEF provider configuration settings
        /// </summary>
        /// <returns>True if loaded successfully or false if the settings could not be loaded</returns>
        /// <remarks>The settings are loaded from Visual Studio user setting store</remarks>
        private bool LoadConfiguration()
        {
            bool success = false;

            try
            {
                var settingsManager = new ShellSettingsManager(serviceProvider);
                var settingsStore = settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);

                if(settingsStore.CollectionExists(CollectionPath))
                {
                    EnableGoToDefinition = settingsStore.GetBoolean(CollectionPath,
                        nameof(EnableGoToDefinition), true);
                    EnableCtrlClickGoToDefinition = settingsStore.GetBoolean(CollectionPath,
                        nameof(EnableCtrlClickGoToDefinition), true);
                    success = true;
                }
            }
            catch(Exception ex)
            {
                // Ignore exceptions.  We'll just use the defaults.
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return success;
        }

        /// <summary>
        /// This is used to save the MEF provider configuration settings
        /// </summary>
        /// <remarks>The settings are saved to the Visual Studio user settings store</remarks>
        public bool SaveConfiguration()
        {
            bool success = false;

            try
            {
                var settingsManager = new ShellSettingsManager(serviceProvider);
                var settingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

                if(!settingsStore.CollectionExists(CollectionPath))
                    settingsStore.CreateCollection(CollectionPath);

                settingsStore.SetBoolean(CollectionPath, nameof(EnableGoToDefinition), EnableGoToDefinition);
                settingsStore.SetBoolean(CollectionPath, nameof(EnableCtrlClickGoToDefinition),
                    EnableCtrlClickGoToDefinition);
                success = true;
            }
            catch(Exception ex)
            {
                // Ignore exceptions
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return success;
        }
        #endregion
    }
}
