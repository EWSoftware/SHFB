//=============================================================================
// System  : Sandcastle Help File Builder
// File    : PromptToSaveDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/24/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to prompt to save files before a build.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.1  10/24/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

            foreach(BaseContentEditor editor in filesToSave)
            {
                if(editor is ProjectPropertiesWindow)
                {
                    if(!projectSeen)
                    {
                        lbFiles.Items.Insert(0, Path.GetFileName(
                            ((ProjectPropertiesWindow)editor).CurrentProject.Filename));
                        projectSeen = true;
                    }
                }
                else
                    if(editor is ProjectExplorerWindow)
                    {
                        if(!projectSeen)
                        {
                            lbFiles.Items.Insert(0, Path.GetFileName(
                                ((ProjectExplorerWindow)editor).CurrentProject.Filename));
                            projectSeen = true;
                        }
                    }
                    else
                        lbFiles.Items.Add(editor.TabText.Substring(0,
                            editor.TabText.Length - 1));
            }
        }
    }
}
