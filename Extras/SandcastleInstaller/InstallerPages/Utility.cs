//=============================================================================
// System  : Sandcastle Guided Installation
// File    : Utility.cs
// Author  : Eric Woodruff
// Updated : 03/05/2012
// Compiler: Microsoft Visual C#
//
// This file contains a class with utility and extension methods.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice and
// all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  02/06/2011  EFW  Created the code
// 1.0.0.1  04/23/2011  EFW  Updated Utility.RunInstaller() to accept an
//                           arguments parameter and have the afterInstall
//                           action take an integer parameter representing the
//                           process exit code.
// 1.1.0.0  03/05/2012  EFW  Converted to use WPF
//=============================================================================

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This class contains utility and extension methods
    /// </summary>
    public static class Utility
    {
        #region Private data members
        //=====================================================================

        private static Task<int> installTask;

        // The action to perform after running an installer
        private static Action<int> afterInstall;

        // The action to perform if the installer fails
        private static Action<Exception> installFailed;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the base path from which the
        /// installer is running.
        /// </summary>
        public static string BasePath
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        /// <summary>
        /// For testing, this returns a relative path offset to the install
        /// resource files.
        /// </summary>
        public static string PathOffset { get; set; }

        /// <summary>
        /// This read-only property returns the path to the additional install
        /// file resources.
        /// </summary>
        public static string InstallResourcesPath
        {
            get
            {
                if(String.IsNullOrEmpty(PathOffset))
                    return Path.Combine(BasePath, "InstallResources");

                return Path.Combine(Path.Combine(BasePath, PathOffset), "InstallResources");
            }
        }
        #endregion

        #region Find tools
        //=====================================================================

        /// <summary>
        /// Find a folder by searching the Program Files folders on all fixed
        /// drives.
        /// </summary>
        /// <param name="path">The path for which to search</param>
        /// <returns>The path if found or an empty string if not found</returns>
        public static string FindOnFixedDrives(string path)
        {
            StringBuilder sb = new StringBuilder(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

            // Check for 64-bit Windows.  The tools will be in the x86 folder.
            if(Directory.Exists(sb.ToString() + " (x86)"))
                sb.Append(" (x86)");

            sb.Append(path);

            foreach(DriveInfo di in DriveInfo.GetDrives())
                if(di.DriveType == DriveType.Fixed)
                {
                    sb[0] = di.Name[0];

                    if(Directory.Exists(sb.ToString()))
                        return sb.ToString();
                }

            return String.Empty;
        }

        /// <summary>
        /// This is used to find the named executable in one of the Visual
        /// Studio SDK installation folders.
        /// </summary>
        /// <param name="exeName">The name of the executable to find</param>
        /// <returns>The path if found or an empty string if not found</returns>
        /// <remarks>The search looks in all "Visual*" folders under the
        /// Program Files special folder on all fixed drives.</remarks>
        public static string FindSdkExecutable(string exeName)
        {
            StringBuilder sb = new StringBuilder(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            string folder;

            // Check for 64-bit Windows.  The tools will be in the x86 folder.
            if(Directory.Exists(sb.ToString() + " (x86)"))
                sb.Append(" (x86)");

            foreach(DriveInfo di in DriveInfo.GetDrives())
                if(di.DriveType == DriveType.Fixed)
                {
                    sb[0] = di.Name[0];
                    folder = sb.ToString();

                    if(!Directory.Exists(folder))
                        continue;

                    foreach(string dir in Directory.GetDirectories(folder, "Visual*"))
                    {
                        // If more than one, sort them and take the last one as it should be the most recent.
                        var file = Directory.GetFiles(dir, exeName, SearchOption.AllDirectories).OrderBy(
                            f => f).LastOrDefault();

                        if(file != null)
                            return Path.GetDirectoryName(file);
                    }
                }

            return String.Empty;
        }
        #endregion

        #region Run installer
        //=====================================================================

        /// <summary>
        /// Run the specified installer application and wait for it to exit
        /// </summary>
        /// <param name="installer">The installer to run</param>
        /// <param name="arguments">An optional list of arguments or null if there are none</param>
        /// <param name="afterInstallFinishes">The action to perform after the
        /// install finishes.</param>
        /// <param name="installerFailed">The action to perform if running the
        /// installer fails with an exception.</param>
        /// <returns>True if the installer was launched, false if not</returns>
        public static bool RunInstaller(string installer, string arguments, Action<int> afterInstallFinishes,
          Action<Exception> installerFailed)
        {
            if(!File.Exists(installer))
            {
                MessageBox.Show("Unable to locate installer: " + installer, "Install",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            ProcessStartInfo psi = new ProcessStartInfo(installer, arguments)
            {
                WorkingDirectory = Utility.BasePath
            };

            afterInstall = afterInstallFinishes;
            installFailed = installerFailed;

            var ui = TaskScheduler.FromCurrentSynchronizationContext();

            installTask = Task.Factory.StartNew<int>(() => RunInstallTask(psi));

            installTask.ContinueWith(t => InstallTaskCompleted(t.Result),
                CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, ui);

            installTask.ContinueWith(t => InstallTaskFailed(t.Exception),
                CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, ui);

            return true;
        }

        /// <summary>
        /// This is used to run the installer task
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <returns>The result of executing the installer process</returns>
        private static int RunInstallTask(ProcessStartInfo psi)
        {
            using(Process p = Process.Start(psi))
            {
                if(p != null)
                {
                    p.WaitForExit();
                    return p.ExitCode;
                }
            }

            throw new InvalidOperationException("Failed to create installation process");
        }

        /// <summary>
        /// This is called when the installer task completes successfully
        /// </summary>
        /// <param name="exitCode">The exit code returned by the installer task</param>
        private static void InstallTaskCompleted(int exitCode)
        {
            installTask.Dispose();
            installTask = null;

            if(afterInstall != null)
                afterInstall(exitCode);
        }

        /// <summary>
        /// This is called when the installer task fails with an exception
        /// </summary>
        /// <param name="exception">The exception thrown by the installer task</param>
        private static void InstallTaskFailed(AggregateException exception)
        {
            installTask.Dispose();
            installTask = null;

            if(exception != null)
                if(installFailed != null)
                    installFailed(exception);
                else
                    throw exception;
        }
        #endregion

        #region Exception message helper
        //=====================================================================

        /// <summary>
        /// This is used to format the message string in an exception
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <returns>The concatenated messages from the exception and any inner exceptions</returns>
        public static string ExceptionMessage(this Exception ex)
        {
            if(ex == null)
                return "No exception";

            StringBuilder sb = new StringBuilder(ex.Message, 1024);

            // Don't bother including the generic aggregate exception message
            if(ex is AggregateException)
                sb.Clear();

            while(ex.InnerException != null)
            {
                sb.Append("\r\n");
                ex = ex.InnerException;
                sb.Append(ex.Message);
            }

            return sb.ToString();
        }
        #endregion

        #region Open or run the given file
        //=====================================================================

        /// <summary>
        /// Open or run the specified file
        /// </summary>
        /// <param name="filename">The file to open or run</param>
        public static void Open(string filename)
        {
            if(!File.Exists(filename))
            {
                MessageBox.Show("Unable to locate file: " + filename, "Open/Run",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(filename);
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format(CultureInfo.CurrentUICulture, "Unable to open/run file '{0}'.  " +
                    "Reason: {1}", filename, ex.Message), "Open/Run", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Default flow document link click handler
        //=====================================================================

        /// <summary>
        /// This provides default hyperlink click behavior for flow documents
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        public static void HyperlinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = e.OriginalSource as Hyperlink;
            Uri absoluteUri;
            string path;

            if(link == null)
                return;

            // Convert relative URIs to absolute URIs
            if(!link.NavigateUri.IsAbsoluteUri)
            {
                path = link.NavigateUri.OriginalString;

                if(path.Length > 1 && path[0] == '/')
                    path = path.Substring(1);

                if(!Uri.TryCreate(Path.Combine(Environment.CurrentDirectory, path),
                  UriKind.RelativeOrAbsolute, out absoluteUri))
                {
                    MessageBox.Show("Invalid link: " + link.NavigateUri.OriginalString, "Install",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                link.NavigateUri = absoluteUri;
            }

            // It looks like an external link so try to launch it.  We don't handle link target so it
            // will always be launched in an external window.
            try
            {
                System.Diagnostics.Process.Start(link.NavigateUri.AbsoluteUri);
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format(CultureInfo.CurrentUICulture, "Unable to launch URL: {0}\r\n\r\n" +
                    "Reason: {1}", link.NavigateUri.Host, ex.Message), "Install", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }
        #endregion

        #region Flow document block collection extension method
        //=====================================================================

        /// <summary>
        /// This extension method is used to append the content of a flow document stored as a resource
        /// to the given flow document block collection.
        /// </summary>
        /// <param name="blocks">The block collection to which the elements are added</param>
        /// <param name="resourceName">The fully qualified name of the flow document resource</param>
        public static void AppendFrom(this BlockCollection blocks, string resourceName)
        {
            try
            {
                using(Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    FlowDocument flowDocument = (FlowDocument)XamlReader.Load(s);

                    foreach(var b in flowDocument.Blocks.ToArray())
                    {
                        flowDocument.Blocks.Remove(b);
                        blocks.Add(b);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format(CultureInfo.CurrentUICulture, "Unable to load flow document " +
                    "resource '{0}'.\r\n\r\nReason: {1}", resourceName, ex.Message), "Installer",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
