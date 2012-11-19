//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : TransformArgumentsPageControl.cs
// Author  : Eric Woodruff
// Updated : 11/18/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Transform Arguments category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.6.0  11/14/2012  EFW  Created the code
// ==============================================================================================================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

using Microsoft.Build.Evaluation;

#if !STANDALONEGUI
using SandcastleBuilder.Package.Properties;
#endif

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.PresentationStyle;

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
        private string lastStyle;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public TransformArgumentsPageControl()
        {
            InitializeComponent();

            // Since we are dependent on the arguments from the selected presentation style, we'll refresh the
            // available arguments when necessary whenever one of the controls gains the focus.  There doesn't
            // seem to be a way to do it when the page gains focus or is made visible in Visual Studio.
            tvArguments.GotFocus += (s, e) => { this.BindControlValue(null); };
            txtValue.GotFocus += (s, e) => { this.BindControlValue(null); };

            this.Title = "Transform Args";
            this.HelpKeyword = "TODO: Add help topic";
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
            if(this.ProjectMgr == null)
                return false;

            argsProp = this.ProjectMgr.BuildProject.GetProperty("TransformComponentArguments");
            styleProp = this.ProjectMgr.BuildProject.GetProperty("PresentationStyle");
#else
            if(this.CurrentProject == null)
                return false;

            argsProp = this.CurrentProject.MSBuildProject.GetProperty("TransformComponentArguments");
            styleProp = this.CurrentProject.MSBuildProject.GetProperty("PresentationStyle");
#endif
            if(styleProp != null && !String.IsNullOrEmpty(styleProp.UnevaluatedValue) && styleProp.UnevaluatedValue.Equals(lastStyle))
                return true;

            var transformComponentArgs = new Dictionary<string, TransformComponentArgument>();

            tvArguments.Nodes.Clear();

            try
            {
                // Get the transform component arguments defined in the project if any
                if(argsProp != null && !String.IsNullOrEmpty(argsProp.UnevaluatedValue))
                {
                    var xr = new XmlTextReader("<Args>" + argsProp.UnevaluatedValue + "</Args>",
                        XmlNodeType.Element, new XmlParserContext(null, null, null, XmlSpace.Preserve));
                    xr.Namespaces = false;
                    xr.MoveToContent();

                    foreach(var arg in XElement.Load(xr, LoadOptions.PreserveWhitespace).Descendants("Argument"))
                    {
                        tca = new TransformComponentArgument(arg);
                        transformComponentArgs.Add(tca.Key, tca);
                    }
                }

                if(styleProp == null || String.IsNullOrEmpty(styleProp.UnevaluatedValue) ||
                  !PresentationStyleDictionary.AllStyles.TryGetValue(styleProp.UnevaluatedValue, out pss))
                    pss = PresentationStyleDictionary.AllStyles[PresentationStyleDictionary.DefaultStyle];

                lastStyle = pss.Id;

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
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if(tvArguments.SelectedNode == null)
                return;

            var arg = (TransformComponentArgument)tvArguments.SelectedNode.Tag;

            if(arg.Value != null)
                arg.Value = txtValue.Text;
            else
            {
                // Ensure the content is valid XML
                try
                {
                    XElement.Parse("<Content>" + txtValue.Text + "</Content>");
                }
                catch(XmlException ex)
                {
#if !STANDALONEGUI
                    MessageBox.Show("The value does not appear to be valid XML.  Error " + ex.Message,
                        Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                    MessageBox.Show("The value does not appear to be valid XML.  Error " + ex.Message,
                        Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    e.Cancel = true;
                    return;
                }

                arg.Content = txtValue.Text;
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

            if(arg.Value != null)
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
                txtValue.Text = arg.Content;
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
