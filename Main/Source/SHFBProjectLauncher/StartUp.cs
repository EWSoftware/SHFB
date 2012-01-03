//=============================================================================
// System  : Sandcastle Help File Builder Project Launcher
// File    : StartUp.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/31/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This application provides a way for the user to choose which application is
// used to load help file builder project files (.shfbproj).
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.1  04/19/2011  EFW  Created the code
// 1.9.3.3  11/19/2011  EFW  Fixed up parameter passed to Process.Start()
//=============================================================================

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using SandcastleBuilder.ProjectLauncher.Properties;

namespace SandcastleBuilder.ProjectLauncher
{
    /// <summary>
    /// This application provides a way for the user to choose which application is
    /// used to load help file builder project files (.shfbproj).
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
        /// This read-only properyt returns teh path to the Sandcastle Help File Builder
        /// </summary>
        /// <value>Returns the path to the help file builder or null if not found</value>
        public static string SHFBPath
        {
            get
            {
                string shfbPath = Environment.GetEnvironmentVariable("SHFBROOT",
                    EnvironmentVariableTarget.Machine);

                if(!String.IsNullOrEmpty(shfbPath))
                {
                    shfbPath = Path.Combine(shfbPath, @"SandcastleBuilderGUI.exe");

                    if(!File.Exists(shfbPath))
                        shfbPath = null;
                }

                return shfbPath;
            }
        }

        /// <summary>
        /// This read-only property returns the path to Visual Studio 2010 if
        /// it is installed.
        /// </summary>
        /// <value>Returns the path to Visual Studio 2010 or null if it or the VSPackage
        /// is not found.</value>
        public static string VS2010Path
        {
            get
            {
                string vs2010Path = Environment.GetEnvironmentVariable("VS100COMNTOOLS",
                    EnvironmentVariableTarget.Machine);

                if(!String.IsNullOrEmpty(vs2010Path))
                {
                    vs2010Path = Path.Combine(vs2010Path, @"..\IDE\devenv.exe");

                    if(!File.Exists(vs2010Path))
                        vs2010Path = null;
                    else
                    {
                        // Check for VSPackage too.  If not present, we can't load the project.
                        string vsPackagePath = Path.Combine(Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                            @"Microsoft\VisualStudio\10.0\Extensions\EWSoftware\SHFB");

                        if(!Directory.Exists(vsPackagePath) || !Directory.EnumerateFiles(vsPackagePath,
                          "SandcastleBuilder.Package.dll", SearchOption.AllDirectories).Any())
                            vs2010Path = null;
                    }
                }

                return vs2010Path;
            }
        }

        /// <summary>
        /// This read-only property returns the project name in a format suitable for passing to
        /// <c>Process.Start</c>.
        /// </summary>
        /// <value>If the path contains spaces and is not in quote marks already, it will be
        /// enclosed in quote marks.</value>
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

            // If no project or VS 2010 is present but no preference has been specified,
            // ask the user what to use.
            if(String.IsNullOrEmpty(ProjectToLoad) ||
              (!Settings.Default.AlwaysUseSelection && !String.IsNullOrEmpty(VS2010Path)) ||
              (String.IsNullOrEmpty(VS2010Path) && !Settings.Default.UseStandaloneGui))
                return success;

            try
            {
                if(!String.IsNullOrEmpty(VS2010Path) && !Settings.Default.UseStandaloneGui)
                    Process.Start(VS2010Path, ProjectNameParameter);
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
        /// <param name="useStandaloneGui">True to use the standalone GUI or false to use Visual
        /// Studio 2010.</param>
        /// <returns>True if successful, false if not</returns>
        public static bool LaunchWithSelectedApplication(bool useStandaloneGui)
        {
            bool success = false;

            if(String.IsNullOrEmpty(ProjectToLoad))
                return success;

            try
            {
                if(!String.IsNullOrEmpty(VS2010Path) && !useStandaloneGui)
                    Process.Start(VS2010Path, ProjectNameParameter);
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
        #endregion
    }
}
