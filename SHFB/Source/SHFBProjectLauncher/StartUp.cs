//===============================================================================================================
// System  : Sandcastle Help File Builder Project Launcher
// File    : StartUp.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/14/2016
// Note    : Copyright 2011-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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
using System.Windows.Forms;

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
                // VS 2017 editions can be installed side-by-side and no longer use an environment variable to
                // indicate their location so find the first one available based on the usual install location.
                string vsPath = FindVisualStudioPath(@"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\Common7\IDE");

                if(vsPath != null)
                    return vsPath;

                vsPath = FindVisualStudioPath(@"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Developer\Common7\IDE");

                if(vsPath != null)
                    return vsPath;

                vsPath = FindVisualStudioPath(@"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\Common7\IDE");

                if(vsPath != null)
                    return vsPath;

                // VS 2015 and earlier install to a single location pointed to by an environment variable
                vsPath = FindVisualStudioPath(@"%VS140COMNTOOLS%\..\IDE");

                if(vsPath != null)
                    return vsPath;

                vsPath = FindVisualStudioPath(@"%VS140COMNTOOLS%\..\IDE");

                if(vsPath != null)
                    return vsPath;

                return FindVisualStudioPath(@"%VS120COMNTOOLS%\..\IDE");
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

            if(!String.IsNullOrEmpty(ProjectToLoad) && !ProjectToLoad.EndsWith(".shfbproj",
              StringComparison.OrdinalIgnoreCase))
                ProjectToLoad = null;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Bring forward user preferences after a version update
            if(!Settings.Default.SettingsUpgraded)
            {
                Settings.Default.Upgrade();
                Settings.Default.SettingsUpgraded = true;
                Settings.Default.Save();
            }

            if(!LaunchWithPreferredApplication())
                Application.Run(new ProjectLauncherForm());
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

            // If no project or VS 2010 is present but no preference has been specified, ask the user what to use
            if(String.IsNullOrEmpty(ProjectToLoad) ||
              (!Settings.Default.AlwaysUseSelection && !String.IsNullOrEmpty(VisualStudioPath)) ||
              (String.IsNullOrEmpty(VisualStudioPath) && !Settings.Default.UseStandaloneGui))
                return success;

            try
            {
                if(!String.IsNullOrEmpty(VisualStudioPath) && !Settings.Default.UseStandaloneGui)
                    Process.Start(VisualStudioPath, ProjectNameParameter);
                else
                {
                    if(String.IsNullOrEmpty(SHFBPath))
                        throw new InvalidOperationException("Unable to locate the Sandcastle Help File Builder");

                    Process.Start(SHFBPath, ProjectNameParameter);
                }

                success = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to determine which application to use to launch the selected " +
                    "help file builder project: " + ex.Message, "Sandcastle Help File Builder Project Launcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
            }

            return success;
        }

        /// <summary>
        /// This is used to try to launch the project using the selected application
        /// </summary>
        /// <param name="useStandaloneGui">True to use the standalone GUI or false to use Visual Studio 2010</param>
        /// <returns>True if successful, false if not</returns>
        public static bool LaunchWithSelectedApplication(bool useStandaloneGui)
        {
            bool success = false;

            if(String.IsNullOrEmpty(ProjectToLoad))
                return success;

            try
            {
                if(!String.IsNullOrEmpty(VisualStudioPath) && !useStandaloneGui)
                    Process.Start(VisualStudioPath, ProjectNameParameter);
                else
                {
                    if(String.IsNullOrEmpty(SHFBPath))
                        throw new InvalidOperationException("Unable to locate the Sandcastle Help File Builder");

                    Process.Start(SHFBPath, ProjectNameParameter);
                }

                success = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to determine which application to use to launch the selected " +
                    "help file builder project: " + ex.Message, "Sandcastle Help File Builder Project Launcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
            }

            return success;
        }

        /// <summary>
        /// This is used to find the Visual Studio path using the given tools environment variable and version
        /// </summary>
        /// <param name="pathToCheck">The potential Visual Studio path to check</param>
        /// <returns>The path to the given Visual Studio version or null if not found or the SHFB VS Package is
        /// not installed for it.</returns>
        private static string FindVisualStudioPath(string pathToCheck)
        {
            string vsPath = Environment.ExpandEnvironmentVariables(pathToCheck);

            if(!String.IsNullOrEmpty(vsPath) && vsPath.IndexOf('%') == -1)
            {
                vsPath = Path.Combine(vsPath, @"devenv.exe");

                if(!File.Exists(vsPath))
                    vsPath = null;
                else
                {
                    // Check for VSPackage too.  If not present, we can't load the project.  It should exist
                    // in the All Users location.
                    string vsPackagePath = Path.Combine(Path.GetDirectoryName(vsPath), "Extensions");

                    if(!Directory.Exists(vsPackagePath) || !Directory.EnumerateFiles(vsPackagePath,
                      "SandcastleBuilder.Package.dll", SearchOption.AllDirectories).Any())
                        vsPath = null;
                }
            }
            else
                vsPath = null;

            return vsPath;
        }
        #endregion
    }
}
