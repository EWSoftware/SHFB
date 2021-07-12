//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : PromptToSaveDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/20/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the form used to prompt to save files before a build.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/24/2008  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;

using SandcastleBuilder.Gui.ContentEditors;

namespace SandcastleBuilder.Gui
{
    /// <summary>
    /// This form is used to prompt to save files before a build
    /// </summary>
    public partial class PromptToSaveDlg : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filesToSave">A collection containing the editors with
        /// files to save.</param>
        public PromptToSaveDlg(Collection<BaseContentEditor> filesToSave)
        {
            bool projectSeen = false;

            InitializeComponent();

            if(filesToSave == null)
                throw new ArgumentNullException(nameof(filesToSave));

            foreach(BaseContentEditor editor in filesToSave)
            {
                if(editor is ProjectPropertiesWindow projectProperties)
                {
                    if(!projectSeen)
                    {
                        lbFiles.Items.Insert(0, Path.GetFileName(projectProperties.CurrentProject.Filename));
                        projectSeen = true;
                    }
                }
                else
                    if(editor is ProjectExplorerWindow projectExplorer)
                    {
                        if(!projectSeen)
                        {
                            lbFiles.Items.Insert(0, Path.GetFileName(projectExplorer.CurrentProject.Filename));
                            projectSeen = true;
                        }
                    }
                    else
                        lbFiles.Items.Add(editor.TabText.Substring(0, editor.TabText.Length - 1));
            }
        }
    }
}
