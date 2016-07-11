//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : TransformArgumentsPageControl.cs
// Author  : Eric Woodruff
// Updated : 10/26/2015
// Note    : Copyright 2012-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Transform Arguments category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/14/2012  EFW  Created the code
// 01/07/2014  EFW  Updated to use MEF for loading the presentation styles
// ==============================================================================================================

using System;
using System.Collections.Generic;
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

        private bool loadingInfo, changingArg;
        private string lastStyle, messageBoxTitle;

        private ComponentCache componentCache;
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

            // Since we are dependent on the arguments from the selected presentation style, we'll refresh the
            // available arguments when necessary whenever one of the controls gains the focus.  There doesn't
            // seem to be a way to do it when the page gains focus or is made visible in Visual Studio.
            tvArguments.GotFocus += (s, e) => this.BindControlValue(null);
            txtValue.GotFocus += (s, e) => this.BindControlValue(null);
#else
            messageBoxTitle = Constants.AppName;

            this.VisibleChanged += (s, e) => this.BindControlValue(null);
#endif
            this.Title = "Transform Args";
            this.HelpKeyword = "c584509f-0b18-49a8-ab06-114b0058a739";
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
            SandcastleProject currentProject = null;
            string[] searchFolders;

            if(loadingInfo)
                return true;

#if !STANDALONEGUI
            if(this.ProjectMgr != null)
                currentProject = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject;
#else
            currentProject = this.CurrentProject;
#endif
            if(currentProject == null)
                tvArguments.Nodes.Clear();
            else
            {
                searchFolders = new[] { currentProject.ComponentPath, Path.GetDirectoryName(currentProject.Filename) };

                if(componentCache == null)
                {
                    componentCache = ComponentCache.CreateComponentCache(currentProject.Filename);

                    componentCache.ComponentContainerLoaded += componentCache_ComponentContainerLoaded;
                    componentCache.ComponentContainerLoadFailed += componentCache_ComponentContainerLoadFailed;
                    componentCache.ComponentContainerReset += componentCache_ComponentContainerReset;
                }

                if(componentCache.LoadComponentContainer(searchFolders))
                    this.componentCache_ComponentContainerLoaded(this, EventArgs.Empty);
                else
                    this.componentCache_ComponentContainerReset(this, EventArgs.Empty);
            }

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

            this.ProjectMgr.SetProjectProperty("TransformComponentArguments", reader.ReadInnerXml());
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
            if(tvArguments.SelectedNode == null || !tvArguments.Enabled)
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
                    MessageBox.Show("The value does not appear to be valid XML.  Error " + ex.Message,
                        messageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);

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
            if(tvArguments.SelectedNode == null || !tvArguments.Enabled)
                return;

            var arg = (TransformComponentArgument)tvArguments.SelectedNode.Tag;

            txtDescription.Text = arg.Description;
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
            if(!changingArg && tvArguments.SelectedNode != null)
                this.IsDirty = true;
        }

        /// <summary>
        /// This is called when the component cache is reset prior to loading it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerReset(object sender, EventArgs e)
        {
            if(!this.IsDisposed)
            {
                loadingInfo = true;
                lastStyle = null;
                tvArguments.Enabled = false;
                tvArguments.Nodes.Clear();
                tvArguments.Nodes.Add(new TreeNode("Loading..."));
            }
        }

        /// <summary>
        /// This is called when the component cache load operation fails
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoadFailed(object sender, EventArgs e)
        {
            if(!this.IsDisposed)
            {
                tvArguments.Enabled = loadingInfo = false;
                tvArguments.Nodes.Clear();
                tvArguments.Nodes.Add(new TreeNode("Unable to load transform component arguments"));
                txtDescription.Text = componentCache.LastError.ToString();
            }
        }

        /// <summary>
        /// This is called when the component cache has finished being loaded and is available for use
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoaded(object sender, EventArgs e)
        {
            PresentationStyleSettings pss = null;
            ProjectProperty argsProp, styleProp;
            TransformComponentArgument tca, clone;
            TreeNode t;

#if !STANDALONEGUI
            if(this.IsDisposed || this.ProjectMgr == null)
                return;

            argsProp = this.ProjectMgr.BuildProject.GetProperty("TransformComponentArguments");
            styleProp = this.ProjectMgr.BuildProject.GetProperty("PresentationStyle");
#else
            if(this.IsDisposed || this.CurrentProject == null)
                return;

            argsProp = this.CurrentProject.MSBuildProject.GetProperty("TransformComponentArguments");
            styleProp = this.CurrentProject.MSBuildProject.GetProperty("PresentationStyle");
#endif
            // Skip it if already loaded and nothing changed
            if((styleProp == null && lastStyle != null) || (styleProp != null &&
              !String.IsNullOrEmpty(styleProp.UnevaluatedValue) &&
              styleProp.UnevaluatedValue.Equals(lastStyle, StringComparison.OrdinalIgnoreCase)))
                return;

            tvArguments.Enabled = true;
            tvArguments.Nodes.Clear();

            var transformComponentArgs = new Dictionary<string, TransformComponentArgument>();

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                tvArguments.BeginUpdate();

                HashSet<string> presentationStyleIds = new HashSet<string>();

                presentationStyles = new List<Lazy<PresentationStyleSettings, IPresentationStyleMetadata>>();

                // There may be duplicate presentation style IDs across the assemblies found.  See
                // BuildComponentManger.GetComponentContainer() for the folder search precedence.  Only the
                // first component for a unique ID will be used.
                foreach(var style in componentCache.ComponentContainer.GetExports<PresentationStyleSettings,
                  IPresentationStyleMetadata>())
                    if(!presentationStyleIds.Contains(style.Metadata.Id))
                    {
                        presentationStyles.Add(style);
                        presentationStyleIds.Add(style.Metadata.Id);
                    }

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
                System.Diagnostics.Debug.WriteLine(ex);

                MessageBox.Show("Unable to load transform component arguments.  Error " + ex.Message,
                    messageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                tvArguments.EndUpdate();
                Cursor.Current = Cursors.Default;
            }

            if(tvArguments.Nodes.Count != 0)
            {
                tvArguments.SelectedNode = tvArguments.Nodes[0];
                txtValue.Enabled = true;
            }
            else
                txtValue.Enabled = false;

            loadingInfo = false;
        }
        #endregion
    }
}
