//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : NamespaceSummaryItemEditorDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/09/2011
// Note    : Copyright 2006-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit namespace summaries and to indicate
// which namespaces should appear in the help file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.2.0.0  09/04/2006  EFW  Created the code
// 1.4.0.0  02/12/2007  EFW  Added the ability to delete old namespaces
// 1.6.0.4  01/17/2008  EFW  Added more error info to help diagnose exceptions
//                           when an assembly fails to load.
// 1.6.0.6  03/07/2008  EFW  Added filter options and reworked the namespace
//                           extract to use a partial build rather than the
//                           assembly loader to prevent "assembly not found"
//                           errors caused by nested dependencies.
// 1.8.0.0  06/30/2008  EFW  Reworked to support MSBuild project format
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This form is used to edit namespace summaries and to indicate which
    /// namespaces should appear in the help file.
    /// </summary>
    public partial class NamespaceSummaryItemEditorDlg : Form
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject tempProject;
        private NamespaceSummaryItemCollection nsColl;
        private Dictionary<string, List<string>> namespaceInfo;
        private SortedDictionary<string, NamespaceSummaryItem> namespaceItems;
        private Thread buildThread;
        private BuildProcess buildProcess;
        #endregion

        #region Constructor
        //=====================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">The namespace summary item collection to edit</param>
        public NamespaceSummaryItemEditorDlg(NamespaceSummaryItemCollection items)
        {
            InitializeComponent();

            nsColl = items;
            namespaceItems = new SortedDictionary<string, NamespaceSummaryItem>();

            // Get a copy of the current namespace summary items
            foreach(NamespaceSummaryItem nsi in nsColl)
                namespaceItems.Add(nsi.Name, nsi);
        }
        #endregion

        #region Build methods
        //=====================================================================
        // Build methods

        /// <summary>
        /// This is called by the build process thread to update the main
        /// window with the current build step.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildStepChanged(object sender,
          BuildProgressEventArgs e)
        {
            if(this.InvokeRequired)
            {
                // Ignore it if we've already shut down or it hasn't
                // completed yet.
                if(!this.IsDisposed)
                    this.Invoke(new EventHandler<BuildProgressEventArgs>(
                        buildProcess_BuildStepChanged),
                        new object[] { sender, e });
            }
            else
            {
                lblProgress.Text = e.BuildStep.ToString();

                if(e.HasCompleted)
                {
                    // Switch back to the current project's folder
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(
                        nsColl.Project.Filename));

                    // If successful, load the namespace nodes, and enable
                    // the UI.
                    if(e.BuildStep == BuildStep.Completed)
                    {
                        this.LoadNamespaces(buildProcess.ReflectionInfoFilename);

                        cboAssembly.Enabled = txtSearchText.Enabled =
                            btnApplyFilter.Enabled = btnAll.Enabled =
                            btnNone.Enabled = true;
                    }

                    pbWait.Visible = lblProgress.Visible = false;
                    lbNamespaces.Focus();

                    buildThread = null;
                    buildProcess = null;
                }
            }
        }

        /// <summary>
        /// This is called by the build process thread to update the main
        /// window with information about its progress.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildProgress(object sender,
          BuildProgressEventArgs e)
        {
            if(this.InvokeRequired)
            {
                // Ignore it if we've already shut down
                if(!this.IsDisposed)
                    this.Invoke(new EventHandler<BuildProgressEventArgs>(
                        buildProcess_BuildProgress),
                        new object[] { sender, e });
            }
            else
            {
                if(e.BuildStep == BuildStep.Failed)
                {
                    MessageBox.Show("Unable to build project to obtain " +
                        "API information.  Please perform a normal build " +
                        "to identify and correct the problem.",
                        Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Load the namespace information from the reflection information
        /// </summary>
        /// <param name="reflectionFile">The reflection information filename</param>
        private void LoadNamespaces(string reflectionFile)
        {
            XPathDocument reflectionInfo;
            XPathNavigator navDoc, navNamespace, navLibrary;
            List<string> assemblies;
            string nsName, asmName;

            namespaceInfo = new Dictionary<string, List<string>>();

            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblProgress.Text = "Loading namespace information...";
                Application.DoEvents();

                reflectionInfo = new XPathDocument(reflectionFile);
                navDoc = reflectionInfo.CreateNavigator();

                // Namespace nodes don't contain assembly info so we'll have
                // to look at all types and add all unique namespaces from
                // their container info.
                foreach(XPathNavigator container in navDoc.Select(
                  "reflection/apis/api[starts-with(@id, 'T:')]/containers"))
                {
                    navNamespace = container.SelectSingleNode("namespace");
                    navLibrary = container.SelectSingleNode("library");

                    if(navNamespace != null && navLibrary != null)
                    {
                        nsName = navNamespace.GetAttribute("api",
                            String.Empty).Substring(2);
                        asmName = navLibrary.GetAttribute("assembly",
                            String.Empty);

                        if(namespaceInfo.TryGetValue(nsName, out assemblies))
                        {
                            if(!assemblies.Contains(asmName))
                                assemblies.Add(asmName);
                        }
                        else
                        {
                            assemblies = new List<string>();
                            assemblies.Add(asmName);
                            namespaceInfo.Add(nsName, assemblies);
                        }
                    }

                    Application.DoEvents();
                }

                // The global namespace (N:) isn't always listed but we'll
                // add it as it does show up in the reflection info anyway.
                if(!namespaceInfo.ContainsKey(String.Empty))
                    namespaceInfo.Add(String.Empty, new List<string>());

                // Add new namespaces to the list as temporary items.  They
                // will get added to the project if modified.
                foreach(string ns in namespaceInfo.Keys)
                {
                    nsName = (ns.Length == 0) ? "(global)" : ns;

                    if(!namespaceItems.ContainsKey(nsName))
                        namespaceItems.Add(ns, nsColl.CreateTemporaryItem(ns));

                    // Sort the assemblies for each namespace
                    assemblies = namespaceInfo[ns];
                    assemblies.Sort();
                }

                Application.DoEvents();

                // Add all unique assembly names to the assembly combo box
                assemblies = new List<string>();

                foreach(List<string> asmList in namespaceInfo.Values)
                    foreach(string asm in asmList)
                        if(!assemblies.Contains(asm))
                            assemblies.Add(asm);

                assemblies.Sort();
                assemblies.Insert(0, "<All>");
                cboAssembly.DataSource = assemblies;

                btnApplyFilter_Click(this, EventArgs.Empty);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// Do a partial build on load to gather new namespace information that
        /// isn't currently in the project's namespace list.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void NamespacesDlg_Load(object sender, EventArgs e)
        {
            string tempPath;

            cboAssembly.Enabled = txtSearchText.Enabled = btnAll.Enabled =
                btnNone.Enabled = btnApplyFilter.Enabled = btnDelete.Enabled =
                false;

            try
            {
                // Clone the project for the build and adjust its properties for our needs
                tempProject = new SandcastleProject(nsColl.Project);

                // The temporary project resides in the same folder as the current project (by filename
                // only, it isn't saved) to maintain relative paths.  However, build output is stored
                // in a temporary folder and it keeps the intermediate files.
                tempProject.CleanIntermediates = false;
                tempPath = Path.GetTempFileName();

                File.Delete(tempPath);
                tempPath = Path.Combine(Path.GetDirectoryName(tempPath), "SHFBPartialBuild");

                if(!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                tempProject.OutputPath = tempPath;

                buildProcess = new BuildProcess(tempProject, true);
                buildProcess.BuildStepChanged += buildProcess_BuildStepChanged;
                buildProcess.BuildProgress += buildProcess_BuildProgress;

                buildThread = new Thread(new ThreadStart(buildProcess.Build));
                buildThread.Name = "Namespace partial build thread";
                buildThread.IsBackground = true;
                buildThread.Start();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to build project to obtain API information.  Error: " +
                    ex.Message, Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Shut down the build process thread and clean up on exit
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void NamespacesDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(buildThread != null && buildThread.IsAlive)
            {
                if(MessageBox.Show("A build is currently taking place to " +
                  "obtain namespace information.  Do you want to abort it " +
                  "and close this form?", Constants.AppName,
                  MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                  DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    this.Cursor = Cursors.WaitCursor;

                    if(buildThread != null)
                        buildThread.Abort();

                    while(buildThread != null && !buildThread.Join(1000))
                        Application.DoEvents();

                    System.Diagnostics.Debug.WriteLine("Thread stopped");
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    buildThread = null;
                    buildProcess = null;
                }
            }

            // Add new items that were modified
            foreach(NamespaceSummaryItem item in lbNamespaces.Items)
                if(item.IsDirty && nsColl[item.Name] == null)
                    nsColl.Add(item);

            this.DialogResult = nsColl.IsDirty ? DialogResult.OK : DialogResult.Cancel;

            if(tempProject != null)
            {
                try
                {
                    // Delete the temporary project's working files
                    if(!String.IsNullOrEmpty(tempProject.OutputPath) &&
                      Directory.Exists(tempProject.OutputPath))
                        Directory.Delete(tempProject.OutputPath, true);
                }
                catch
                {
                    // Eat the exception.  We'll ignore it if the temporary files cannot be deleted.
                }

                tempProject.Dispose();
                tempProject = null;
            }

            GC.Collect(2);
            GC.WaitForPendingFinalizers();
            GC.Collect(2);
        }

        /// <summary>
        /// Store the changes and close the dialog box
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// View help for this form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            try
            {
#if DEBUG
                path += @"\..\..\..\Doc\Help\SandcastleBuilder.chm";
#else
                path += @"\SandcastleBuilder.chm";
#endif
                Form form = new Form();
                form.CreateControl();
                Help.ShowHelp(form, path, HelpNavigator.Topic,
                    "html/eb7e1bc7-21c5-4453-bbaf-dec8c62c15bd.htm");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
                    "Unable to open help file '{0}'.  Reason: {1}",
                    path, ex.Message), Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// When the item changes, show its summary in the text box and set
        /// the Appears In list box data source.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbNamespaces_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<string> assemblies;
            string name;

            if(lbNamespaces.SelectedIndex != -1)
            {
                NamespaceSummaryItem nsi =
                    (NamespaceSummaryItem)lbNamespaces.SelectedItem;
                txtSummary.Text = nsi.Summary;
                name = nsi.Name;

                if(name[0] == '(')
                    name = String.Empty;

                if(namespaceInfo.TryGetValue(name, out assemblies))
                    lbAppearsIn.DataSource = assemblies;
                else
                    lbAppearsIn.DataSource = null;
            }
            else
            {
                txtSummary.Text = null;
                lbAppearsIn.DataSource = null;
            }
        }

        /// <summary>
        /// Mark the summary item as documented or not when the check state
        /// changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbNamespaces_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            NamespaceSummaryItem nsi = (NamespaceSummaryItem)lbNamespaces.Items[e.Index];

            bool isChecked = (e.NewValue == CheckState.Checked);

            if(nsi.IsDocumented != isChecked)
                nsi.IsDocumented = isChecked;
        }

        /// <summary>
        /// Clear the selection to prevent accidental deletion of the text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtSummary_Enter(object sender, EventArgs e)
        {
            txtSummary.Select(0, 0);
            txtSummary.ScrollToCaret();
        }

        /// <summary>
        /// Store changes to the summary when the textbox loses focus
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtSummary_Leave(object sender, EventArgs e)
        {
            NamespaceSummaryItem nsi = (NamespaceSummaryItem)lbNamespaces.SelectedItem;

            if(nsi != null && nsi.Summary != txtSummary.Text)
                nsi.Summary = txtSummary.Text;
        }

        /// <summary>
        /// Delete an old namespace entry that is no longer needed.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            NamespaceSummaryItem nsi;
            int idx = lbNamespaces.SelectedIndex;

            if(idx == -1)
                lbNamespaces.SelectedIndex = 0;
            else
            {
                nsi = (NamespaceSummaryItem)lbNamespaces.Items[idx];
                lbNamespaces.Items.RemoveAt(idx);

                nsi = nsColl[nsi.Name];

                if(nsi != null)
                    nsColl.Remove(nsi);

                if(lbNamespaces.Items.Count == 0)
                    btnDelete.Enabled = txtSummary.Enabled = false;
                else
                    if(idx < lbNamespaces.Items.Count)
                        lbNamespaces.SelectedIndex = idx;
                    else
                        lbNamespaces.SelectedIndex =
                            lbNamespaces.Items.Count - 1;
            }
        }

        /// <summary>
        /// Apply the namespace filter to the namespace list
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnApplyFilter_Click(object sender, EventArgs e)
        {
            NamespaceSummaryItem nsi;
            List<string> assemblies;
            Regex reFilter = null;
            lbNamespaces.Items.Clear();
            string name;

            txtSearchText.Text = txtSearchText.Text.Trim();

            if(txtSearchText.Text.Length != 0)
                try
                {
                    reFilter = new Regex(txtSearchText.Text,
                        RegexOptions.IgnoreCase);
                }
                catch(ArgumentException ex)
                {
                    epErrors.SetError(txtSearchText, "The search regular " +
                        "expression is not valid: " + ex.Message);
                    return;
                }

            // Add the items to the listbox in sorted order by name
            foreach(string key in namespaceItems.Keys)
            {
                nsi = namespaceItems[key];

                // Filter by assembly?
                if(cboAssembly.SelectedIndex != 0)
                {
                    name = nsi.Name;

                    if(name[0] == '(')
                        name = String.Empty;

                    if(namespaceInfo.TryGetValue(name, out assemblies))
                        if(!assemblies.Contains(cboAssembly.Text))
                            continue;
                }

                // Filter by search text?
                if(reFilter != null && !reFilter.IsMatch(nsi.Name))
                    continue;

                lbNamespaces.Items.Add(nsi, nsi.IsDocumented);
            }

            if(lbNamespaces.Items.Count != 0)
            {
                btnDelete.Enabled = txtSummary.Enabled = true;
                lbNamespaces.SelectedIndex = 0;
            }
            else
                btnDelete.Enabled = txtSummary.Enabled = false;
        }

        /// <summary>
        /// Mark all namespaces as included
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAll_Click(object sender, EventArgs e)
        {
            for(int idx = 0; idx < lbNamespaces.Items.Count; idx++)
                lbNamespaces.SetItemChecked(idx, true);
        }

        /// <summary>
        /// Mark all namespaces as excluded
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>Note that at least one will need to be selected or the
        /// build will fail due to lack of information to document.</remarks>
        private void btnNone_Click(object sender, EventArgs e)
        {
            for(int idx = 0; idx < lbNamespaces.Items.Count; idx++)
                lbNamespaces.SetItemChecked(idx, false);
        }
        #endregion
    }
}
