//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : MissingTagPropertiesPageContent.xaml.cs
// Author  : Eric Woodruff
// Updated : 10/06/2017
// Note    : Copyright 2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Missing Tags category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/04/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Windows.Controls;
using System.Windows;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the Missing Tags category properties
    /// </summary>
    public partial class MissingTagPropertiesPageContent : UserControl
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the selected missing tags values
        /// </summary>
        /// <value>The values for this page are stored in a single property so only one checkbox is bound and we
        /// get or set the values as a group.</value>
        public MissingTags MissingTags
        {
            get
            {
                MissingTags tags = MissingTags.None;

                if(chkAutoDocumentConstructors.IsChecked ?? false)
                    tags |= MissingTags.AutoDocumentCtors;

                if(chkAutoDocumentDisposeMethods.IsChecked ?? false)
                    tags |= MissingTags.AutoDocumentDispose;

                if(chkShowMissingIncludeTargets.IsChecked ?? false)
                    tags |= MissingTags.IncludeTargets;

                if(chkShowMissingNamespaces.IsChecked ?? false)
                    tags |= MissingTags.Namespace;

                if(chkShowMissingParams.IsChecked ?? false)
                    tags |= MissingTags.Parameter;

                if(chkShowMissingRemarks.IsChecked ?? false)
                    tags |= MissingTags.Remarks;

                if(chkShowMissingReturns.IsChecked ?? false)
                    tags |= MissingTags.Returns;

                if(chkShowMissingSummaries.IsChecked ?? false)
                    tags |= MissingTags.Summary;

                if(chkShowMissingTypeParams.IsChecked ?? false)
                    tags |= MissingTags.TypeParameter;

                if(chkShowMissingValues.IsChecked ?? false)
                    tags |= MissingTags.Value;

                return tags;
            }
            set
            {
                chkAutoDocumentConstructors.IsChecked = ((value & MissingTags.AutoDocumentCtors) != 0);
                chkAutoDocumentDisposeMethods.IsChecked = ((value & MissingTags.AutoDocumentDispose) != 0);
                chkShowMissingIncludeTargets.IsChecked = ((value & MissingTags.IncludeTargets) != 0);
                chkShowMissingNamespaces.IsChecked = ((value & MissingTags.Namespace) != 0);
                chkShowMissingParams.IsChecked = ((value & MissingTags.Parameter) != 0);
                chkShowMissingRemarks.IsChecked = ((value & MissingTags.Remarks) != 0);
                chkShowMissingReturns.IsChecked = ((value & MissingTags.Returns) != 0);
                chkShowMissingSummaries.IsChecked = ((value & MissingTags.Summary) != 0);
                chkShowMissingTypeParams.IsChecked = ((value & MissingTags.TypeParameter) != 0);
                chkShowMissingValues.IsChecked = ((value & MissingTags.Value) != 0);
            }
        }
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised when any of the unbound control property values changes
        /// </summary>
        public event EventHandler PropertyChanged;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MissingTagPropertiesPageContent()
        {
            InitializeComponent();
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This notifies the parent control of changes to the unbound checkbox controls
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            this.PropertyChanged?.Invoke(sender, e);
        }
        #endregion
    }
}
