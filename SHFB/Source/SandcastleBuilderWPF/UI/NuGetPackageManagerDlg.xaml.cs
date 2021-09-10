//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : NuGetPackageManagerDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/09/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains the form used to manage NuGet packages in a help file builder project
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/06/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;

using SandcastleBuilder.WPF.NuGet;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Documents;
using System.Diagnostics;

using Microsoft.Build.Evaluation;
using System.IO;
using System.Xml.Linq;

namespace SandcastleBuilder.WPF.UI
{
    /// <summary>
    /// This form is used to manage NuGet packages in a help file builder project
    /// </summary>
    /// <remarks>This is not intended to be as comprehensive as the one in Visual Studio.  It just implements
    /// enough to support adding and removing packages from help file builder projects.</remarks>
    public partial class NuGetPackageManagerDlg : Window
    {
        #region Private data members
        //=====================================================================

        private readonly Project project;
        private List<INuGetPackageSource> packageSources;
        private ObservableCollection<NuGetPackage> foundPackages;
        private IEnumerable<INuGetPackageSource> searchSources;
        private IEnumerable<string> searchKeywords;
        private bool startNewSearch, isSearching, includePreRelease, installedOnly;
        private readonly Dictionary<string, string> projectPackages;
        private readonly IProgress<string> reportProgress;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentProject">The current project</param>
        public NuGetPackageManagerDlg(Project currentProject)
        {
            project = currentProject ?? throw new ArgumentNullException(nameof(currentProject));
            projectPackages = new Dictionary<string, string>();
            startNewSearch = true;
            reportProgress = new Progress<string>(this.ReportProgress);

            foreach(var p in project.GetItems("PackageReference"))
                projectPackages[p.UnevaluatedInclude] = p.GetMetadataValue("Version");

            InitializeComponent();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Search for packages as needed
        /// </summary>
        private async void SearchForPackages()
        {
            try
            {
                lblAction.Text = "Searching...";
                grdAction.Visibility = Visibility.Visible;
                svDetails.IsEnabled = false;

                isSearching = true;

                var packagesFound = await Task.Run(() =>
                {
                    var packages = new List<NuGetPackage>();

                    // If limiting results to installed packages, make sure they are all there
                    if(installedOnly && startNewSearch)
                    {
                        foreach(var s in searchSources)
                        {
                            packages.AddRange(s.SearchForPackages(projectPackages.Keys.Select(k => "Id:" + k),
                                startNewSearch, includePreRelease));
                        }
                    }

                    foreach(var s in searchSources)
                        packages.AddRange(s.SearchForPackages(searchKeywords, startNewSearch, includePreRelease));

                    if(installedOnly)
                        return packages.Where(p => projectPackages.ContainsKey(p.Id)).ToList();

                    return packages;

                }).ConfigureAwait(true);

                // Combine results if already found in by a prior search or in another source in the current
                // search.  A bit of brute force here but it'll work for what this needs to do.
                var packageIndex = foundPackages.ToDictionary(k => k.Id, v => v);

                foreach(var p in packagesFound)
                {
                    if(packageIndex.TryGetValue(p.Id, out NuGetPackage prior))
                    {
                        if(NuGetPackage.CompareSemanticVersion(prior.LatestVersion, p.LatestVersion) >= 0)
                            prior.CombineVersions(p.Versions);
                        else
                        {
                            p.CombineVersions(prior.Versions);
                            foundPackages[foundPackages.IndexOf(prior)] = p;
                            packageIndex[p.Id] = p;
                            prior = p;
                        }
                    }
                    else
                    {
                        foundPackages.Add(p);
                        packageIndex.Add(p.Id, p);
                        prior = p;
                    }

                    // Set installed package info if it's in the project
                    if(projectPackages.TryGetValue(p.Id, out string version))
                    {
                        prior.IsInstalled = true;
                        prior.InstalledVersion = version;
                    }
                }

                startNewSearch = packagesFound.Count == 0;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Search failed: " + ex.Message, "NuGet Package Search Failure",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                grdAction.Visibility = Visibility.Hidden;
                isSearching = false;
                svDetails.IsEnabled = foundPackages.Count != 0;

                if(lbPackages.SelectedIndex == -1 && foundPackages.Count != 0)
                    lbPackages.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// This is called to run MSBuild to restore the package reference information for the project
        /// </summary>
        private async void RestorePackages()
        {
            try
            {
                project.Save();

                lblAction.Text = "Applying package reference change...";
                grdAction.Visibility = Visibility.Visible;

                grdOptions.IsEnabled = svDetails.IsEnabled = false;

                await Task.Run(() => this.ExecutePackageRestore()).ConfigureAwait(true);
            }
            catch(Exception ex)
            {
                txtBuildOutput.AppendText("\r\n");
                txtBuildOutput.AppendText(ex.ToString());
            }
            finally
            {
                grdOptions.IsEnabled = true;
                this.btnSearch_Click(this, null);
            }
        }

        /// <summary>
        /// This is used to report build progress
        /// </summary>
        /// <param name="message">The progress message</param>
        private void ReportProgress(string message)
        {
            txtBuildOutput.AppendText(message + "\r\n");
            txtBuildOutput.CaretIndex = txtBuildOutput.Text.Length;
            txtBuildOutput.ScrollToEnd();
        }

        /// <summary>
        /// This is used to run the project restore
        /// </summary>
        private void ExecutePackageRestore()
        {
            Process currentProcess = null;

            reportProgress.Report("Clearing NuGet cache files...");

            // Delete the content of the .\obj folder to force the restore
            string objPath = Path.Combine(Path.GetDirectoryName(project.FullPath), "obj");

            if(Directory.Exists(objPath))
                foreach(string file in Directory.EnumerateFiles(objPath, "*", SearchOption.AllDirectories))
                    File.Delete(file);

            // Use the latest version of MSBuild available rather than a specific version
            string latestToolsVersion = ProjectCollection.GlobalProjectCollection.Toolsets.FirstOrDefault(
                t => t.ToolsVersion.Equals("Current", StringComparison.OrdinalIgnoreCase))?.ToolsVersion;

            if(latestToolsVersion == null)
            {
                latestToolsVersion = ProjectCollection.GlobalProjectCollection.Toolsets.Max(
                    t => Version.TryParse(t.ToolsVersion, out Version ver) ? ver : new Version()).ToString();
            }

            string msbuildExePath = Path.Combine(ProjectCollection.GlobalProjectCollection.Toolsets.First(
                t => t.ToolsVersion == latestToolsVersion).ToolsPath, "MSBuild.exe");


            reportProgress.Report("Executing package restore...");

            try
            {
                currentProcess = new Process();

                ProcessStartInfo psi = currentProcess.StartInfo;

                // Set CreateNoWindow to true to suppress the window rather than setting WindowStyle to hidden as
                // WindowStyle has no effect on command prompt windows and they always appear.
                psi.CreateNoWindow = true;
                psi.FileName = msbuildExePath;
                psi.Arguments = $"/t:restore {project.FullPath}";
                psi.WorkingDirectory = Path.GetDirectoryName(project.FullPath);
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = psi.RedirectStandardError = true;

                currentProcess.Start();

                // Spawn two separate tasks so that we can capture both STDOUT and STDERR without the risk of a
                // deadlock.
                using(var stdOutReader = Task.Run(() => this.ReadOutputStream(currentProcess.StandardOutput)))
                using(var stdErrReader = Task.Run(() => this.ReadOutputStream(currentProcess.StandardError)))
                using(var processWaiter = Task.Run(() =>
                {
                    bool hasExited;

                    do
                    {
                        hasExited = currentProcess.WaitForExit(1000);

                    } while(!hasExited);
                }))
                {
                    Task.WaitAll(processWaiter, stdOutReader, stdErrReader);
                }
            }
            catch(AggregateException ex)
            {
                var canceledEx = ex.InnerExceptions.FirstOrDefault(e => e is OperationCanceledException);

                if(canceledEx != null)
                    throw canceledEx;

                throw;
            }
            finally
            {
                if(currentProcess != null)
                    currentProcess.Dispose();
            }
        }

        /// <summary>
        /// This is used to capture output from the given process stream
        /// </summary>
        private void ReadOutputStream(StreamReader stream)
        {
            string line;

            do
            {
                line = stream.ReadLine();

                if(line != null)
                    reportProgress.Report(line);

            } while(line != null);
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Index the package sources when the form is loaded
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private async void NuGetPackageManagerDlg_Loaded(object sender, RoutedEventArgs e)
        {
            txtKeywords.IsEnabled = btnSearch.IsEnabled = false;
            lblAction.Text = "Indexing local package sources...";
            svDetails.IsEnabled = false;

            packageSources = NuGetPackageManager.PackageSources.ToList();
            foundPackages = new ObservableCollection<NuGetPackage>();

            cboPackageSource.Items.Add("All Sources");

            foreach(var ps in packageSources)
                cboPackageSource.Items.Add(ps.PackageSourceName);

            cboPackageSource.SelectedIndex = 0;

            await Task.WhenAll(packageSources.Select(ps => Task.Run(() => ps.IndexPackageSource()))).ConfigureAwait(true);

            lbPackages.ItemsSource = foundPackages = new ObservableCollection<NuGetPackage>();

            txtKeywords.IsEnabled = btnSearch.IsEnabled = true;
            grdAction.Visibility = Visibility.Hidden;

            this.btnSearch_Click(sender, e);
        }

        /// <summary>
        /// Load more results when scrolled to the end of the current list if not done yet
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbPackages_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(!startNewSearch && !isSearching)
            {
                var border = VisualTreeHelper.GetChild((ListBox)sender, 0);

                if(border != null)
                {
                    if(VisualTreeHelper.GetChild(border, 0) is ScrollViewer scrollViewer &&
                      scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                    {
                        this.SearchForPackages();
                    }
                }
            }
        }

        /// <summary>
        /// Search for packages matching the keywords
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            searchSources = packageSources;

            if(cboPackageSource.SelectedIndex > 0)
                searchSources = new[] { packageSources[cboPackageSource.SelectedIndex - 1] };

            if(!this.IsVisible || searchSources == null || !searchSources.All(s => s.IsIndexed))
                return;

            startNewSearch = true;
            searchKeywords = txtKeywords.Text.Trim().Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            includePreRelease = chkPrerelease.IsChecked ?? false;
            installedOnly = rbInstalledOnly.IsChecked ?? false;
            foundPackages.Clear();

            this.SearchForPackages();
        }

        /// <summary>
        /// Install or update the selected package reference
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            if(lbPackages.SelectedItem is NuGetPackage selectedPackage)
            {
                XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
                XDocument projectXml = XDocument.Parse(project.Xml.RawXml);

                // Ensure that the elements required to support package references are present in the project.
                // If not, add them.  Search the raw XML rather than using the API as it may have the targets
                // we are looking for already imported but not by the actual project.
                if(!projectXml.Root.Elements(msbuild + "Import").Any(i => i.Attribute("Project").Value.IndexOf(
                  "Microsoft.Common.props", StringComparison.OrdinalIgnoreCase) != -1))
                {
                    // Use the API to add the elements
                    var import = project.Xml.AddImport(@"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props");
                    import.Condition = @"Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')";

                    // The import may not be at the start were it needs to be so move it
                    import.Parent.RemoveChild(import);
                    project.Xml.InsertBeforeChild(import, project.Xml.FirstChild);
                }

                if(!projectXml.Root.Elements(msbuild + "Import").Any(i => i.Attribute("Project").Value.IndexOf(
                  "Microsoft.Common.targets", StringComparison.OrdinalIgnoreCase) != -1))
                {
                    var import = project.Xml.AddImport(@"$(MSBuildToolsPath)\Microsoft.Common.targets");
                    import.Condition = "'$(MSBuildRestoreSessionId)' != ''";

                    // This import needs to appear right before the SandcastleHelpFileBuilder.targets import.
                    // For it, we can use the API to find it as it will be in the project.
                    var shfbTargets = project.Imports.First(i => i.ImportingElement.Project.IndexOf(
                        "SandcastleHelpFileBuilder.targets", StringComparison.OrdinalIgnoreCase) != -1);

                    if(String.IsNullOrWhiteSpace(shfbTargets.ImportingElement.Condition))
                        shfbTargets.ImportingElement.Condition = "'$(MSBuildRestoreSessionId)' == ''";

                    import.Parent.RemoveChild(import);
                    project.Xml.InsertBeforeChild(import, shfbTargets.ImportingElement);
                }

                string version = (string)cboVersion.SelectedItem;

                var item = project.GetItems("PackageReference").FirstOrDefault(
                    pr => pr.UnevaluatedInclude == selectedPackage.Id);

                if(item != null)
                    item.SetMetadataValue("Version", version);
                else
                {
                    project.AddItem("PackageReference", selectedPackage.Id,
                        new[] { new KeyValuePair<string, string>("Version", version) });
                }

                projectPackages[selectedPackage.Id] = version;

                this.RestorePackages();
            }
        }

        /// <summary>
        /// Open a hyperlink target in the browser
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if(e?.OriginalSource is Hyperlink link && link.NavigateUri != null)
            {
                try
                {
                    Process.Start(link.NavigateUri.AbsoluteUri);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Unable to launch URL: {link.NavigateUri.Host}\r\n\r\nReason: {ex.Message}",
                        "NuGet Package Manager", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        /// <summary>
        /// Install or update the selected package
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void imgInstallUpdatePackage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.btnInstall_Click(sender, e);
        }

        /// <summary>
        /// Remove package
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void imgRemovePackage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(lbPackages.SelectedItem is NuGetPackage selectedPackage)
            {
                var item = project.GetItems("PackageReference").FirstOrDefault(
                    pr => pr.UnevaluatedInclude == selectedPackage.Id);

                if(item != null)
                {
                    project.RemoveItem(item);
                    projectPackages.Remove(selectedPackage.Id);

                    this.RestorePackages();
                }
            }
        }
        #endregion
    }
}
