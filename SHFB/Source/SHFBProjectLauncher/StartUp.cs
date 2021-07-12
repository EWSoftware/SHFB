//===============================================================================================================
// System  : Sandcastle Help File Builder Project Launcher
// File    : StartUp.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/21/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This application provides a way for the user to choose which application is used to load help file builder
// project files (.shfbproj).
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/19/2011  EFW  Created the code
// 11/19/2011  EFW  Fixed up parameter passed to Process.Start()
// 09/22/2012  EFW  Updated to launch latest Visual Studio version with the VS Package installed
//===============================================================================================================

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

using SandcastleBuilder.ProjectLauncher.Properties;

namespace SandcastleBuilder.ProjectLauncher
{
    /// <summary>
    /// This application provides a way for the user to choose which application is used to load help file
    /// builder project files (.shfbproj).
    /// </summary>
    internal static class StartUp
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the project file to load
        /// </summary>
        public static string ProjectToLoad { get; private set; }

        /// <summary>
        /// This read-only property returns the path to the Sandcastle Help File Builder
        /// </summary>
        /// <value>Returns the path to the help file builder or null if not found</value>
        public static string SHFBPath
        {
            get
            {
                string shfbPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "SandcastleBuilderGUI.exe");

                if(!File.Exists(shfbPath))
                    shfbPath = null;

                return shfbPath;
            }
        }

        /// <summary>
        /// This read-only property returns the path to the latest version of Visual Studio if it is installed
        /// and the SHFB VS Package has been installed for it.
        /// </summary>
        /// <value>Returns the path to Visual Studio or null if it or the VSPackage is not found in any of the
        /// installed versions.</value>
        public static string VisualStudioPath
        {
            get
            {
                var latestVersion = Sandcastle.Installer.VisualStudioInstance.AllInstances.Where(
                    i => i.HelpFileBuilderIsInstalled).LastOrDefault();

                return latestVersion?.DevEnvPath;
            }
        }

        /// <summary>
        /// This read-only property returns the project name in a format suitable for passing to
        /// <c>Process.Start</c>.
        /// </summary>
        /// <value>If the path contains spaces and is not in quote marks already, it will be enclosed in quote
        /// marks.</value>
        private static string ProjectNameParameter
        {
            get
            {
                string project = ProjectToLoad;

                if(project != null && project.Length > 1 && project.IndexOf(' ') != -1 && project[0] != '\"')
                    project = "\"" + project + "\"";

                return project;
            }
        }
        #endregion

        #region Main entry point
        //=====================================================================

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ProjectToLoad = (args.Length == 0) ? null : args[0];

            if(!String.IsNullOrWhiteSpace(ProjectToLoad) && !ProjectToLoad.EndsWith(".shfbproj",
              StringComparison.OrdinalIgnoreCase))
                ProjectToLoad = null;

            // Bring forward user preferences after a version update
            if(!Settings.Default.SettingsUpgraded)
            {
                Settings.Default.Upgrade();
                Settings.Default.SettingsUpgraded = true;
                Settings.Default.Save();
            }

            if(!LaunchWithPreferredApplication())
            {
                var app = new Application();

                app.Run(new ProjectLauncherDlg());
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to try to launch the project using the user's preferred application
        /// </summary>
        /// <returns>True if successful, false if not</returns>
        public static bool LaunchWithPreferredApplication()
        {
            bool success = false;

            // If no project or Visual Studio is present but no preference has been specified, ask the user what
            // to use.
            if(String.IsNullOrWhiteSpace(ProjectToLoad) ||
              (!Settings.Default.AlwaysUseSelection && !String.IsNullOrWhiteSpace(VisualStudioPath)) ||
              (String.IsNullOrWhiteSpace(VisualStudioPath) && !Settings.Default.UseStandaloneGui))
                return success;

            try
            {
                if(!String.IsNullOrWhiteSpace(VisualStudioPath) && !Settings.Default.UseStandaloneGui)
                    Process.Start(VisualStudioPath, ProjectNameParameter);
                else
                {
                    if(String.IsNullOrWhiteSpace(SHFBPath))
                        throw new InvalidOperationException("Unable to locate the Sandcastle Help File Builder");

                    Process.Start(SHFBPath, ProjectNameParameter);
                }

                success = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to determine which application to use to launch the selected " +
                    "help file builder project: " + ex.Message, "Sandcastle Help File Builder Project Launcher",
                    MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);
            }

            return success;
        }

        /// <summary>
        /// This is used to try to launch the project using the selected application
        /// </summary>
        /// <param name="useStandaloneGui">True to use the standalone GUI or false to use Visual Studio</param>
        /// <returns>True if successful, false if not</returns>
        public static bool LaunchWithSelectedApplication(bool useStandaloneGui)
        {
            bool success = false;

            if(String.IsNullOrWhiteSpace(ProjectToLoad))
                return success;

            try
            {
                if(!String.IsNullOrWhiteSpace(VisualStudioPath) && !useStandaloneGui)
                    Process.Start(VisualStudioPath, ProjectNameParameter);
                else
                {
                    if(String.IsNullOrWhiteSpace(SHFBPath))
                        throw new InvalidOperationException("Unable to locate the Sandcastle Help File Builder");

                    Process.Start(SHFBPath, ProjectNameParameter);
                }

                success = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to determine which application to use to launch the selected " +
                    "help file builder project: " + ex.Message, "Sandcastle Help File Builder Project Launcher",
                    MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);
            }

            return success;
        }
        #endregion
    }
}
