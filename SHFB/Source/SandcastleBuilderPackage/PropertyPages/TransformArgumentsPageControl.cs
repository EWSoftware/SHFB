//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : TransformArgumentsPageControl.cs
// Author  : Eric Woodruff
// Updated : 08/24/2014
// Note    : Copyright 2012-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Transform Arguments category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.6.0  11/14/2012  EFW  Created the code
//          01/07/2014  EFW  Updated to use MEF for loading the presentation styles
// ==============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;

using SandcastleBuilder.Utils;

#if !STANDALONEGUI
using SandcastleBuilder.Package.Properties;
using SandcastleBuilder.Package.Nodes;
using _PersistStorageType = Microsoft.VisualStudio.Shell.Interop._PersistStorageType;
#endif

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Transform Arguments category project properties
    /// </summary>
    [Guid("5325A09E-BFFF-4056-A331-0FBDF9FF8AF4")]
    public partial class TransformArgumentsPageControl : BasePropertyPage
    {
        #region Private data members
        //=====================================================================

        private bool changingArg;
        private string lastStyle, messageBoxTitle, lastProjectName;

        private CompositionContainer componentContainer;
        private List<Lazy<PresentationStyleSettings, IPresentationStyleMetadata>> presentationStyles;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public TransformArgumentsPageControl()
        {
            InitializeComponent();

#if !STANDALONEGUI
            messageBoxTitle = Resources.PackageTitle;
#else
            messageBoxTitle = Constants.AppName;
#endif
            // Since we are dependent on the arguments from the selected presentation style, we'll refresh the
            // available arguments when necessary whenever one of the controls gains the focus.  There doesn't
            // seem to be a way to do it when the page gains focus or is made visible in Visual Studio.
            tvArguments.GotFocus += (s, e) => { this.BindControlValue(null); };
            txtValue.GotFocus += (s, e) => { this.BindControlValue(null); };

            this.Title = "Transform Args";
            this.HelpKeyword = "c584509f-0b18-49a8-ab06-114b0058a739";
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Try to load information about all available presentation styles so that their arguments can be loaded
        /// for use in the project.
        /// </summary>
        /// <returns>True on success, false on failure or if no project is loaded</returns>
        private void LoadAvailablePresentationStyles()
        {
            SandcastleProject currentProject = null;
            HashSet<string> presentationStyleIds = new HashSet<string>();
            string[] searchFolders;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if(componentContainer != null)
                {
                    componentContainer.Dispose();
                    componentContainer = null;
                }

                presentationStyles = new List<Lazy<PresentationStyleSettings, IPresentationStyleMetadata>>();

#if !STANDALONEGUI
                if(base.ProjectMgr != null)
                    currentProject = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;
#else
                currentProject = base.CurrentProject;
#endif
                lastProjectName = currentProject == null ? null : currentProject.Filename;

                if(currentProject != null)
                    searchFolders = new[] { currentProject.ComponentPath, Path.GetDirectoryName(currentProject.Filename) };
                else
                    searchFolders = new string[] { };

                componentContainer = ComponentUtilities.CreateComponentContainer(searchFolders);

                // There may be duplicate presentation style IDs across the assemblies found.  See
                // BuildComponentManger.GetComponentContainer() for the folder search precedence.  Only the
                // first component for a unique ID will be used.
                foreach(var style in componentContainer.GetExports<PresentationStyleSettings,
                  IPresentationStyleMetadata>())
                    if(!presentationStyleIds.Contains(style.Metadata.Id))
                    {
                        presentationStyles.Add(style);
                        presentationStyleIds.Add(style.Metadata.Id);
                    }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading presentation styles: " + ex.Message, messageBoxTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
            {
                var e = new TreeViewCancelEventArgs(null, false, TreeViewAction.Unknown);

                tvArguments_BeforeSelect(this, e);

                return !e.Cancel;
            }
        }

        /// <inheritdoc />
        protected override bool BindControlValue(Control control)
        {
            PresentationStyleSettings pss = null;
            ProjectProperty argsProp, styleProp;
            TransformComponentArgument tca, clone;
            TreeNode t;

#if !STANDALONEGUI
            SandcastleProject currentProject = null;

            if(base.ProjectMgr != null)
                currentProject = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;

            if(presentationStyles == null || currentProject == null || currentProject.Filename != lastProjectName)
                this.LoadAvailablePresentationStyles();

            if(base.ProjectMgr == null || currentProject == null)
            {
                tvArguments.Nodes.Clear();
                return false;
            }

            argsProp = this.ProjectMgr.BuildProject.GetProperty("TransformComponentArguments");
            styleProp = this.ProjectMgr.BuildProject.GetProperty("PresentationStyle");
#else
            if(presentationStyles == null || base.CurrentProject == null || base.CurrentProject.Filename != lastProjectName)
                this.LoadAvailablePresentationStyles();

            if(this.CurrentProject == null)
            {
                tvArguments.Nodes.Clear();
                return false;
            }

            argsProp = this.CurrentProject.MSBuildProject.GetProperty("TransformComponentArguments");
            styleProp = this.CurrentProject.MSBuildProject.GetProperty("PresentationStyle");
#endif
            if((styleProp == null && lastStyle != null) || (styleProp != null &&
              !String.IsNullOrEmpty(styleProp.UnevaluatedValue) &&
              styleProp.UnevaluatedValue.Equals(lastStyle, StringComparison.OrdinalIgnoreCase)))
                return true;

            tvArguments.Nodes.Clear();

            var transformComponentArgs = new Dictionary<string, TransformComponentArgument>();

            try
            {
                // Get the transform component arguments defined in the project if any
                if(argsProp != null && !String.IsNullOrEmpty(argsProp.UnevaluatedValue))
                {
                    using(var xr = new XmlTextReader("<Args>" + argsProp.UnevaluatedValue + "</Args>",
                      XmlNodeType.Element, new XmlParserContext(null, null, null, XmlSpace.Preserve)))
                    {
                        xr.Namespaces = false;
                        xr.MoveToContent();

                        foreach(var arg in XElement.Load(xr, LoadOptions.PreserveWhitespace).Descendants("Argument"))
                        {
                            tca = new TransformComponentArgument(arg);
                            transformComponentArgs.Add(tca.Key, tca);
                        }
                    }
                }

                if(styleProp != null && !String.IsNullOrWhiteSpace(styleProp.UnevaluatedValue))
                {
                    var style = presentationStyles.FirstOrDefault(s => s.Metadata.Id.Equals(
                        styleProp.UnevaluatedValue, StringComparison.OrdinalIgnoreCase));

                    if(style != null)
                        pss = style.Value;
                }

                if(pss == null)
                {
                    var style = presentationStyles.FirstOrDefault(s => s.Metadata.Id.Equals(
                        Constants.DefaultPresentationStyle, StringComparison.OrdinalIgnoreCase));

                    if(style != null)
                        pss = style.Value;
                    else
                        pss = presentationStyles.First().Value;
                }

                lastStyle = (styleProp != null) ? styleProp.UnevaluatedValue : Constants.DefaultPresentationStyle;

                // Create an entry for each transform component argument in the presentation style
                foreach(var arg in pss.TransformComponentArguments)
                {
                    t = tvArguments.Nodes.Add(arg.Key);
                    t.Tag = clone = arg.Clone();

                    // Use the value from the project or the cloned default if not present
                    if(transformComponentArgs.TryGetValue(arg.Key, out tca))
                    {
                        clone.Value = tca.Value;
                        clone.Content = tca.Content;
                    }
                }
            }
            catch(Exception ex)
            {
#if !STANDALONEGUI
                MessageBox.Show("Unable to load transform component arguments.  Error " + ex.Message,
                    Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                MessageBox.Show("Unable to load transform component arguments.  Error " + ex.Message,
                    Sandcastle.Core.Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
            }

            if(tvArguments.Nodes.Count != 0)
            {
                tvArguments.SelectedNode = tvArguments.Nodes[0];
                txtValue.Enabled = true;
            }
            else
                txtValue.Enabled = false;

            return true;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(Control control)
        {
            TransformComponentArgument tca;
            XElement root = new XElement("TransformComponentArguments");

            foreach(TreeNode t in tvArguments.Nodes)
            {
                tca = (TransformComponentArgument)t.Tag;

                root.Add(tca.ToXml());
            }

            var reader = root.CreateReader();
            reader.MoveToContent();

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;

            this.ProjectMgr.SetProjectProperty("TransformComponentArguments", _PersistStorageType.PST_PROJECT_FILE, reader.ReadInnerXml());
#else
            if(this.CurrentProject == null)
                return false;

            this.CurrentProject.MSBuildProject.SetProperty("TransformComponentArguments", reader.ReadInnerXml());
#endif
            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Validate the current transform argument and store its value
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvArguments_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if(tvArguments.SelectedNode == null)
                return;

            var arg = (TransformComponentArgument)tvArguments.SelectedNode.Tag;

            if(arg.Value != null || arg.Content == null)
                arg.Value = txtValue.Text;
            else
            {
                // Ensure the content is valid XML
                try
                {
                    arg.Content = XElement.Parse("<Content>" + txtValue.Text + "</Content>");
                }
                catch(XmlException ex)
                {
#if !STANDALONEGUI
                    MessageBox.Show("The value does not appear to be valid XML.  Error " + ex.Message,
                        Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                    MessageBox.Show("The value does not appear to be valid XML.  Error: " + ex.Message,
                        Sandcastle.Core.Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    e.Cancel = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Show the selected transformation argument value
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvArguments_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(tvArguments.SelectedNode == null)
                return;

            var arg = (TransformComponentArgument)tvArguments.SelectedNode.Tag;

            lblDescription.Text = arg.Description;
            chkIsForConceptualBuild.Checked = arg.IsForConceptualBuild;
            chkIsForReferenceBuild.Checked = arg.IsForReferenceBuild;

            changingArg = true;

            if(arg.Value != null || arg.Content == null)
            {
                txtValue.Multiline = false;
                txtValue.ScrollBars = ScrollBars.None;
                txtValue.Text = arg.Value;
            }
            else
            {
                txtValue.Multiline = true;
                txtValue.Height = this.Height - txtValue.Top - 5;
                txtValue.ScrollBars = ScrollBars.Vertical;

                var reader = arg.Content.CreateReader();
                reader.MoveToContent();
                txtValue.Text = reader.ReadInnerXml();
            }

            changingArg = false;
        }

        /// <summary>
        /// Mark the project as dirty if the transform argument value changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtValue_TextChanged(object sender, EventArgs e)
        {
            if(!changingArg)
                this.IsDirty = true;
        }
        #endregion
    }
}
