﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : UserDefinedPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 04/20/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This user control is used to edit the User Defined category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2011  EFW  Created the code
// 10/28/2012  EFW  Updated for use in the standalone GUI
// 10/25/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
// ==============================================================================================================

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Build.Evaluation;
using Sandcastle.Core;

#if !STANDALONEGUI
using SandcastleBuilder.Package.Nodes;
#endif

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the User Defined Properties category project properties
    /// </summary>
    [Guid("D9EDDED9-4825-40F5-B394-262D9153C4E2")]
    public partial class UserDefinedPropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public UserDefinedPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "User Defined";
            this.HelpKeyword = "c4e3ce8e-6881-47a7-a429-49ec5c755c8c";
            this.MinimumSize = DetermineMinimumSize(ucUserDefinedPropertiesPageContent);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
            {
                ProjectProperty p;

                try
                {
#if !STANDALONEGUI
                    if(base.ProjectMgr != null)
                    {
                        foreach(var item in ucUserDefinedPropertiesPageContent.UserDefinedProperties)
                            if(item.WasModified)
                            {
                                p = this.ProjectMgr.BuildProject.SetProperty(item.Name, item.Value);
                                p.Xml.Condition = item.Condition;
                            }

                        this.ProjectMgr.BuildProject.ReevaluateIfNecessary();
                    }
#else
                    if(this.CurrentProject != null)
                    {
                        foreach(var item in ucUserDefinedPropertiesPageContent.UserDefinedProperties)
                            if(item.WasModified)
                            {
                                p = this.CurrentProject.MSBuildProject.SetProperty(item.Name, item.Value);
                                p.Xml.Condition = item.Condition;
                            }

                        this.CurrentProject.MSBuildProject.ReevaluateIfNecessary();
                    }
#endif
                }
                catch(Exception ex)
                {
                    MessageBox.Show("An error occurred storing the user-defined project properties:\r\n\r\n" +
                        ex.Message, Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }

                return true;
            }
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
#if !STANDALONEGUI
            if(this.ProjectMgr == null)
            {
                this.Enabled = false;
                return;
            }

            ucUserDefinedPropertiesPageContent.Project = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject;
#else
            if(this.CurrentProject == null)
            {
                this.Enabled = false;
                return;
            }

            ucUserDefinedPropertiesPageContent.Project = this.CurrentProject;
#endif
            ucUserDefinedPropertiesPageContent.PropertyChanged += this.OnPropertyChanged;
            ucUserDefinedPropertiesPageContent.ProjectIsEditable += UcUserDefinedPropertiesPageContent_ProjectIsEditable;

            ucUserDefinedPropertiesPageContent.LoadUserDefinedProperties();
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to see if the containing project is editable.  If not, changes are not allowed to the
        /// project properties.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>If editable, the page is marked dirty as a change will be made upon return to the caller</remarks>
        private void UcUserDefinedPropertiesPageContent_ProjectIsEditable(object sender, CancelEventArgs e)
        {
#if !STANDALONEGUI
            if(this.ProjectMgr == null || !this.ProjectMgr.QueryEditProjectFile(false))
            {
                e.Cancel = true;
                return;
            }
#endif
#pragma warning disable VSTHRD010
            this.IsDirty = true;
#pragma warning restore VSTHRD010
        }
        #endregion
    }
}
