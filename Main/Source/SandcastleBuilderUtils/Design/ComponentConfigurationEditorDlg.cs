//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : ComponentConfigurationEditorDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/09/2011
// Note    : Copyright 2007-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to select and edit the third-party build
// component configurations.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.2  09/10/2007  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is used to select and edit the third-party build component
    /// configurations.
    /// </summary>
    /// <remarks>To be editable, the build component configuration file must
    /// be present in the <b>.\Build Components</b> folder or a subfolder
    /// beneath it.  The build components folder is found under the common
    /// application data folder.</remarks>
    internal partial class ComponentConfigurationEditorDlg : Form
    {
        #region Private data members
        // The current configurations
        private ComponentConfigurationDictionary currentConfigs;

        #endregion

        //=====================================================================
        // Methods, etc.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configs">The current configurations</param>
        internal ComponentConfigurationEditorDlg(
          ComponentConfigurationDictionary configs)
        {
            int idx;

            InitializeComponent();

            currentConfigs = configs;

            try
            {
                // Show all but the hidden components
                foreach(string key in BuildComponentManager.BuildComponents.Keys)
                    if(!BuildComponentManager.BuildComponents[key].IsHidden)
                        lbAvailableComponents.Items.Add(key);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading build components: " +
                    ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if(lbAvailableComponents.Items.Count != 0)
                lbAvailableComponents.SelectedIndex = 0;
            else
            {
                MessageBox.Show("No valid build components found",
                    Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                gbAvailableComponents.Enabled = gbProjectAddIns.Enabled = false;
            }

            foreach(string key in currentConfigs.Keys)
            {
                idx = lbProjectComponents.Items.Add(key);
                lbProjectComponents.SetItemChecked(idx,
                    currentConfigs[key].Enabled);
            }

            if(lbProjectComponents.Items.Count != 0)
                lbProjectComponents.SelectedIndex = 0;
            else
                btnConfigure.Enabled = btnDelete.Enabled = false;
        }

        /// <summary>
        /// Close this form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Update the build component details when the selected index changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbAvailableComponents_SelectedIndexChanged(object sender,
          EventArgs e)
        {
            string key = (string)lbAvailableComponents.SelectedItem;

            BuildComponentInfo info = BuildComponentManager.BuildComponents[key];
            txtComponentCopyright.Text = info.Copyright;
            txtComponentVersion.Text = String.Format(CultureInfo.CurrentCulture,
                "Version {0}", info.Version);
            txtComponentDescription.Text = info.Description;
        }

        /// <summary>
        /// Update the enabled state of the build component based on its
        /// checked state.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbProjectComponents_ItemCheck(object sender,
          ItemCheckEventArgs e)
        {
            string key = (string)lbProjectComponents.Items[e.Index];
            bool newState = (e.NewValue == CheckState.Checked);

            if(currentConfigs[key].Enabled != newState)
                currentConfigs[key].Enabled = newState;
        }

        /// <summary>
        /// Add the selected build component to the project with a default
        /// configuration.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddComponent_Click(object sender, EventArgs e)
        {
            string key = (string)lbAvailableComponents.SelectedItem;
            int idx = lbProjectComponents.FindStringExact(key);

            // Currently, no duplicates are allowed
            if(idx != -1)
            {
                lbProjectComponents.SelectedIndex = idx;
                return;
            }

            idx = lbProjectComponents.Items.Add(key);

            if(idx != -1)
            {
                currentConfigs.Add(key, true,
                    BuildComponentManager.BuildComponents[key].DefaultConfiguration);
                lbProjectComponents.SelectedIndex = idx;
                lbProjectComponents.SetItemChecked(idx, true);
                btnConfigure.Enabled = btnDelete.Enabled = true;

                currentConfigs.OnDictionaryChanged(new ListChangedEventArgs(
                    ListChangedType.ItemAdded, -1));
            }
        }

        /// <summary>
        /// Edit the selected build component's project configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnConfigure_Click(object sender, EventArgs e)
        {
            BuildComponentConfiguration componentConfig;
            string newConfig, currentConfig, assembly,
                key = (string)lbProjectComponents.SelectedItem;

            BuildComponentInfo info = BuildComponentManager.BuildComponents[key];

            componentConfig = currentConfigs[key];
            currentConfig = componentConfig.Configuration;

            // If it doesn't have a configuration method, use the default
            // configuration editor.
            if(String.IsNullOrEmpty(info.ConfigureMethod))
            {
                using(ConfigurationEditorDlg dlg = new ConfigurationEditorDlg())
                {
                    dlg.Configuration = currentConfig;

                    if(dlg.ShowDialog() == DialogResult.OK)
                    {
                        // Only store it if new or if it changed
                        newConfig = dlg.Configuration;

                        if(currentConfig != newConfig)
                            componentConfig.Configuration = newConfig;
                    }
                }

                return;
            }

            // Don't allow editing if set to "-"
            if(info.ConfigureMethod == "-")
            {
                MessageBox.Show("The selected component contains no " +
                    "editable configuration information",
                    Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Change into the component's folder so that it has a
                // better chance of finding all of its dependencies.
                assembly = BuildComponentManager.ResolveComponentPath(
                    info.AssemblyPath);
                Directory.SetCurrentDirectory(Path.GetDirectoryName(
                    Path.GetFullPath(assembly)));

                // The exception is BuildAssemblerLibrary.dll which is in
                // the Sandcastle installation folder.  We'll have to resolve
                // that one manually.
                AppDomain.CurrentDomain.AssemblyResolve +=
                    new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                // Load the type and get a reference to the static
                // configuration method.
                Assembly asm = Assembly.LoadFrom(assembly);
                Type component = asm.GetType(info.TypeName);
                MethodInfo mi = null;

                if(component != null)
                    mi = component.GetMethod(info.ConfigureMethod,
                        BindingFlags.Public | BindingFlags.Static);

                if(component != null && mi != null)
                {
                    // Invoke the method to let it configure the settings
                    newConfig = (string)mi.Invoke(null, new object[] {
                        currentConfig });

                    // Only store it if new or if it changed
                    if(currentConfig != newConfig)
                        componentConfig.Configuration = newConfig;
                }
                else
                    MessageBox.Show(String.Format(CultureInfo.InvariantCulture,
                        "Unable to locate the '{0}' method in component " +
                        "'{1}' in assembly {2}", info.ConfigureMethod,
                        info.TypeName, assembly), Constants.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch(IOException ioEx)
            {
                System.Diagnostics.Debug.WriteLine(ioEx.ToString());
                MessageBox.Show(String.Format(CultureInfo.InvariantCulture,
                    "A file access error occurred trying to configure the " +
                    "component '{0}'.  Error: {1}", info.TypeName, ioEx.Message),
                    Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show(String.Format(CultureInfo.InvariantCulture,
                    "An error occurred trying to configure the component " +
                    "'{0}'.  Error: {1}", info.TypeName, ex.Message),
                    Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -=
                    new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            }
        }

        /// <summary>
        /// This is handled to resolve dependent assemblies and load them
        /// when necessary.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="args">The event arguments</param>
        /// <returns>The loaded assembly</returns>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly asm = null;

            string[] nameInfo = args.Name.Split(new char[] { ',' });
            string resolveName = nameInfo[0];

            // See if it has already been loaded
            foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                if(args.Name == a.FullName)
                {
                    asm = a;
                    break;
                }

            // Is it a Sandcastle component?
            if(asm == null)
            {
                resolveName = Directory.EnumerateFiles(BuildComponentManager.SandcastlePath, "*.dll",
                    SearchOption.AllDirectories).FirstOrDefault(
                    f => resolveName == Path.GetFileNameWithoutExtension(f));

                if(resolveName != null)
                    asm = Assembly.LoadFile(resolveName);
            }

            if(asm == null)
                throw new FileLoadException("Unable to resolve reference to " + args.Name);

            return asm;
        }

        /// <summary>
        /// Delete the selected build component from the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string key = (string)lbProjectComponents.SelectedItem;
            int idx = lbProjectComponents.SelectedIndex;

            if(currentConfigs.ContainsKey(key))
            {
                currentConfigs.Remove(key);
                currentConfigs.OnDictionaryChanged(new ListChangedEventArgs(
                    ListChangedType.ItemDeleted, -1));

                lbProjectComponents.Items.RemoveAt(idx);

                if(lbProjectComponents.Items.Count == 0)
                    btnConfigure.Enabled = btnDelete.Enabled = false;
                else
                    if(idx < lbProjectComponents.Items.Count)
                        lbProjectComponents.SelectedIndex = idx;
                    else
                        lbProjectComponents.SelectedIndex = idx - 1;
            }
        }

        /// <summary>
        /// View help for this form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

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
                    "html/8dcbb69b-7a1a-4049-8e6b-2bf344efbbc9.htm");
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
    }
}
