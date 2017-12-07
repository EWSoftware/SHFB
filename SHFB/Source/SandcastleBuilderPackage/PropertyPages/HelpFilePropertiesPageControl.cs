//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : HelpFilePropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 11/10/2017
// Note    : Copyright 2011-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Help File category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2011  EFW  Created the code
// 10/27/2012  EFW  Added support for the new presentation style definition file
// 12/13/2013  EFW  Added support for namespace grouping
// 11/10/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.Build.Evaluation;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Help Format category project properties
    /// </summary>
    [Guid("714E7D74-A81C-403B-91E9-D052F0438FC6")]
    public partial class HelpFilePropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public HelpFilePropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Help File";
            this.HelpKeyword = "1b2dff59-92cc-4578-b261-f3849f30c26c";
            this.MinimumSize = DetermineMinimumSize(ucHelpFilePropertiesPageContent);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
            {
                ucHelpFilePropertiesPageContent.FixUpPropertyValues();
                return true;
            }
        }

        /// <inheritdoc />
        protected override bool IsEscapedProperty(string propertyName)
        {
            switch(propertyName)
            {
                case "HelpTitle":
                case "HelpFileVersion":
                case "HtmlHelpName":
                case "CopyrightHref":
                case "CopyrightText":
                case "FeedbackEMailAddress":
                case "FeedbackEMailLinkText":
                case "HeaderText":
                case "FooterText":
                case "PresentationStyle":
                case "RootNamespaceTitle":
                    return true;

                default:
                    return false;
            }
        }

        /// <inheritdoc />
        protected override bool BindControlValue(string propertyName)
        {
            ProjectProperty projProp = null;

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;
#else
            if(this.CurrentProject == null)
                return false;
#endif
            // Add the project's selected language to the list if it is not there
            if(propertyName == "Language")
            {
#if !STANDALONEGUI
                projProp = this.ProjectMgr.BuildProject.GetProperty("Language");
#else
                projProp = this.CurrentProject.MSBuildProject.GetProperty("Language");
#endif
                ucHelpFilePropertiesPageContent.SetLanguage(projProp?.UnevaluatedValue ?? "en-US");
                return true;
            }

            return false;
        }
        #endregion
    }
}
