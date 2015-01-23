//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MSHelp2PropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 04/28/2013
// Note    : Copyright 2011-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the MS Help 2 category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
// 1.9.6.0  10/28/2012  EFW  Updated for use in the standalone GUI
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.Build.Evaluation;

#if !STANDALONEGUI
using SandcastleBuilder.Package.Nodes;
#endif
using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the MS Help 2 category project properties
    /// </summary>
    [Guid("90AFB619-1ED0-4CB2-BC36-3A5430C635C0")]
    public partial class MSHelp2PropertiesPageControl : BasePropertyPage
    {
        #region Private data members
        //=====================================================================

        private MSHelpAttrCollection attributes;
        private bool attributesChanged;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MSHelp2PropertiesPageControl()
        {
            InitializeComponent();
            dgvHelpAttributes.AutoGenerateColumns = false;

            this.Title = "MS Help 2";
            this.HelpKeyword = "d0c2dabd-3caf-4586-b81d-cbd765dec7cf";

            cboMSHelp2SdkLinkType.DisplayMember = cboCollectionTocStyle.DisplayMember = "Value";
            cboMSHelp2SdkLinkType.ValueMember = cboCollectionTocStyle.ValueMember = "Key";

            cboMSHelp2SdkLinkType.DataSource = (new Dictionary<string, string> {
                { MSHelp2SdkLinkType.Msdn.ToString(), "Online links to MSDN help topics" },
                { MSHelp2SdkLinkType.Index.ToString(), "Index links within the collection" },
                { MSHelp2SdkLinkType.None.ToString(), "No SDK links" } }).ToList();

            cboCollectionTocStyle.DataSource = (new Dictionary<string, string> {
                { CollectionTocStyle.Hierarchical.ToString(), "Group content under a root container node" },
                { CollectionTocStyle.Flat.ToString(), "List content at the root level" } }).ToList();

            dgvHelpAttributes.EditingControlShowing += (s, e) =>
            {
                attributesChanged = true;
                base.OnPropertyChanged(s, e);
            };
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
            {
                txtHelpFileVersion.Text = txtHelpFileVersion.Text.Trim();
                txtPlugInNamespaces.Text = txtPlugInNamespaces.Text.Trim();

                if(txtHelpFileVersion.Text.Length == 0)
                    txtHelpFileVersion.Text = "1.0.0.0";

                if(txtPlugInNamespaces.Text.Length == 0)
                    txtPlugInNamespaces.Text = "ms.vsipcc+, ms.vsexpresscc+";

                dgvHelpAttributes.EndEdit();
                return true;
            }
        }

        /// <inheritdoc />
        protected override bool IsEscapedProperty(string propertyName)
        {
            switch(propertyName)
            {
                case "HelpFileVersion":
                case "PlugInNamespaces":
                    return true;

                default:
                    return false;
            }
        }

        /// <inheritdoc />
        protected override bool BindControlValue(System.Windows.Forms.Control control)
        {
            ProjectProperty projProp;

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;
#else
            if(this.CurrentProject == null)
                return false;
#endif
            if(control.Name == "dgvHelpAttributes")
            {
                attributesChanged = false;

#if !STANDALONEGUI
                attributes = new MSHelpAttrCollection(
                    ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject);
                projProp = this.ProjectMgr.BuildProject.GetProperty("HelpAttributes");
#else
                attributes = new MSHelpAttrCollection(this.CurrentProject);
                projProp = this.CurrentProject.MSBuildProject.GetProperty("HelpAttributes");
#endif
                if(projProp != null && !String.IsNullOrEmpty(projProp.UnevaluatedValue))
                    attributes.FromXml(projProp.UnevaluatedValue);

                dgvHelpAttributes.DataSource = attributes;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(System.Windows.Forms.Control control)
        {
#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;
#else
            if(this.CurrentProject == null)
                return false;
#endif
            if(control.Name == "dgvHelpAttributes")
            {
                if(attributesChanged)
                {
                    dgvHelpAttributes.EndEdit();
                    dgvHelpAttributes.DataSource = null;
                    attributes.Sort();

#if !STANDALONEGUI
                    this.ProjectMgr.SetProjectProperty("HelpAttributes", attributes.ToXml());
#else
                    this.CurrentProject.MSBuildProject.SetProperty("HelpAttributes", attributes.ToXml());
#endif
                    attributesChanged = false;

                    dgvHelpAttributes.DataSource = attributes;
                }
                return true;
            }

            return false;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Add new help attribute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddAttribute_Click(object sender, EventArgs e)
        {
            dgvHelpAttributes.EndEdit();
            attributes.Add("NoName", null);
            dgvHelpAttributes.CurrentCell = dgvHelpAttributes[0, attributes.Count - 1];
            dgvHelpAttributes.Focus();
            this.IsDirty = attributesChanged = true;
        }

        /// <summary>
        /// Delete the selected attribute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDeleteAttribute_Click(object sender, EventArgs e)
        {
            int idx;

            if(dgvHelpAttributes.SelectedRows.Count != 0)
            {
                idx = dgvHelpAttributes.SelectedRows[0].Index;

                if(idx < attributes.Count)
                {
                    dgvHelpAttributes.EndEdit();
                    attributes.RemoveAt(idx);
                    this.IsDirty = attributesChanged = true;
                }
            }
        }

        /// <summary>
        /// Insert a default set of attributes if they are not already there
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDefaultAttributes_Click(object sender, EventArgs e)
        {
            dgvHelpAttributes.EndEdit();
            dgvHelpAttributes.DataSource = null;

            attributes.Add("DocSet", "NetFramework");
            attributes.Add("DocSet", "{@HtmlEncHelpName}");
            attributes.Add("TargetOS", "Windows");
            attributes.Sort();

            dgvHelpAttributes.DataSource = attributes;
            this.IsDirty = attributesChanged = true;
        }
        #endregion
    }
}
