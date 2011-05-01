//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ComponentPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 04/10/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Components category properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.Build.Evaluation;

using SandcastleBuilder.Package.Properties;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Components category project properties
    /// </summary>
    [Guid("F3BA863D-9E18-477E-A62F-DFD679C8FEF7")]
    public partial class ComponentPropertiesPageControl : BasePropertyPage
    {
        #region Private data members
        //=====================================================================

        private ComponentConfigurationDictionary currentConfigs;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ComponentPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Components";
            this.HelpKeyword = "8dcbb69b-7a1a-4049-8e6b-2bf344efbbc9";

            try
            {
                // Show all but the hidden components
                lbAvailableComponents.Items.AddRange(BuildComponentManager.BuildComponents.Values.Where(
                    bc => !bc.IsHidden).Select(bc => bc.Id).ToArray());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading build components: " + ex.Message,
                    Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if(lbAvailableComponents.Items.Count != 0)
                lbAvailableComponents.SelectedIndex = 0;
            else
            {
                MessageBox.Show("No valid build components found", Resources.PackageTitle, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                gbAvailableComponents.Enabled = gbProjectAddIns.Enabled = false;
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

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
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool BindControlValue(Control c)
        {
            ProjectProperty projProp;
            int idx;

            currentConfigs = new ComponentConfigurationDictionary(null);
            lbProjectComponents.Items.Clear();

            if(this.ProjectMgr == null)
                return false;

            projProp = this.ProjectMgr.BuildProject.GetProperty("ComponentConfigurations");

            if(projProp != null && !String.IsNullOrEmpty(projProp.UnevaluatedValue))
                currentConfigs.FromXml(projProp.UnevaluatedValue);

            foreach(string key in currentConfigs.Keys)
            {
                idx = lbProjectComponents.Items.Add(key);
                lbProjectComponents.SetItemChecked(idx, currentConfigs[key].Enabled);
            }

            if(lbProjectComponents.Items.Count != 0)
                lbProjectComponents.SelectedIndex = 0;
            else
                btnConfigure.Enabled = btnDelete.Enabled = false;

            return true;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(Control c)
        {
            if(this.ProjectMgr == null)
                return false;

            this.ProjectMgr.SetProjectProperty("ComponentConfigurations", currentConfigs.ToXml());
            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Update the build component details when the selected index changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbAvailableComponents_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = (string)lbAvailableComponents.SelectedItem;

            BuildComponentInfo info = BuildComponentManager.BuildComponents[key];
            txtComponentCopyright.Text = info.Copyright;
            txtComponentVersion.Text = String.Format(CultureInfo.CurrentCulture, "Version {0}", info.Version);
            txtComponentDescription.Text = info.Description;
        }

        /// <summary>
        /// Update the enabled state of the build component based on its
        /// checked state.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbProjectComponents_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
        {
            string key = (string)lbProjectComponents.Items[e.Index];
            bool newState = (e.NewValue == CheckState.Checked);

            if(currentConfigs[key].Enabled != newState)
            {
                currentConfigs[key].Enabled = newState;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Add the selected build component to the project with a default configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddComponent_Click(object sender, EventArgs e)
        {
            string key = (string)lbAvailableComponents.SelectedItem;
            int idx = lbProjectComponents.FindStringExact(key);

            // Currently, no duplicates are allowed
            if(idx != -1)
                lbProjectComponents.SelectedIndex = idx;
            else
            {
                idx = lbProjectComponents.Items.Add(key);

                if(idx != -1)
                {
                    currentConfigs.Add(key, true, BuildComponentManager.BuildComponents[key].DefaultConfiguration);
                    lbProjectComponents.SelectedIndex = idx;
                    lbProjectComponents.SetItemChecked(idx, true);
                    btnConfigure.Enabled = btnDelete.Enabled = true;

                    this.IsDirty = true;
                }
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
            string newConfig, currentConfig, assembly, key = (string)lbProjectComponents.SelectedItem;

            BuildComponentInfo info = BuildComponentManager.BuildComponents[key];

            componentConfig = currentConfigs[key];
            currentConfig = componentConfig.Configuration;

            // If it doesn't have a configuration method, use the default configuration editor
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
                        {
                            componentConfig.Configuration = newConfig;
                            this.IsDirty = true;
                        }
                    }
                }

                return;
            }

            // Don't allow editing if set to "-"
            if(info.ConfigureMethod == "-")
            {
                MessageBox.Show("The selected component contains no editable configuration information",
                    Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Change into the component's folder so that it has a better chance of finding all of its
                // dependencies.
                assembly = BuildComponentManager.ResolveComponentPath(info.AssemblyPath);
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(assembly)));

                // The exception is BuildAssemblerLibrary.dll which is in the Sandcastle installation folder.
                // We'll have to resolve that one manually.
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                // Load the type and get a reference to the static configuration method
                Assembly asm = Assembly.LoadFrom(assembly);
                Type component = asm.GetType(info.TypeName);
                MethodInfo mi = null;

                if(component != null)
                    mi = component.GetMethod(info.ConfigureMethod, BindingFlags.Public | BindingFlags.Static);

                if(component != null && mi != null)
                {
                    // Invoke the method to let it configure the settings
                    newConfig = (string)mi.Invoke(null, new object[] { currentConfig });

                    // Only store it if new or if it changed
                    if(currentConfig != newConfig)
                    {
                        componentConfig.Configuration = newConfig;
                        this.IsDirty = true;
                    }
                }
                else
                    MessageBox.Show(String.Format(CultureInfo.InvariantCulture, "Unable to locate the '{0}' " +
                        "method in component '{1}' in assembly {2}", info.ConfigureMethod, info.TypeName,
                        assembly), Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch(IOException ioEx)
            {
                System.Diagnostics.Debug.WriteLine(ioEx.ToString());
                MessageBox.Show(String.Format(CultureInfo.InvariantCulture, "A file access error occurred " +
                    "trying to configure the component '{0}'.  Error: {1}", info.TypeName, ioEx.Message),
                    Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show(String.Format(CultureInfo.InvariantCulture, "An error occurred trying to " +
                    "configure the component '{0}'.  Error: {1}", info.TypeName, ex.Message),
                    Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            }
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
                this.IsDirty = true;

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
        #endregion
    }
}
