//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : StartUp.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/20/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This application provides a GUI that is used to edit configuration files that can be used to build HTML
// documentation help files using Sandcastle.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/02/2006  EFW  Created the code
//===============================================================================================================

using System;
using System.Windows.Forms;

using Microsoft.Build.Locator;

using Sandcastle.Core;
using SandcastleBuilder.Gui.Properties;

namespace SandcastleBuilder.Gui
{
    /// <summary>
    /// This class contains the main entry point and other start up code.
    /// </summary>
    public static class StartUp
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        [STAThread]
        private static void Main(string[] args)
        {
            string projectToLoad = (args.Length == 0) ? null : args[0];

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Bring forward user preferences after a version update
            if(!Settings.Default.SettingsUpgraded)
            {
                Settings.Default.Upgrade();
                Settings.Default.SettingsUpgraded = true;
                Settings.Default.Save();
            }

            try
            {
                MSBuildLocator.RegisterDefaults();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to register MSBuild defaults: " + ex.Message + "\r\n\r\nYou probably " +
                    "need to install the Microsoft Build Tools for Visual Studio 2017 or later.", Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            Application.Run(new MainForm(projectToLoad));
        }
    }
}
