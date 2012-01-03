//=============================================================================
// System  : Sandcastle Help File Builder
// File    : EntityReferenceWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/29/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to look up code entity references, code
// snippets, tokens, and images, and allows them to be dragged and dropped into
// a topic editor window.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  05/12/2008  EFW  Created the code
// 1.8.0.0  08/18/2008  EFW  Reworked for use with the new project format
// 1.9.3.3  12/11/2011  EFW  Rewrote to use the shared WPF Entity References
//                           user control.
//=============================================================================

using System.Windows.Forms;

using SandcastleBuilder.Utils;
using SandcastleBuilder.WPF.UserControls;

using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to look up code entity references, code snippets,
    /// tokens, and images, and allows them to be dragged and dropped into
    /// a topic editor window.
    /// </summary>
    public partial class EntityReferenceWindow : BaseContentEditor
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the current project
        /// </summary>
        public SandcastleProject CurrentProject
        {
            get { return ucEntityReferences.CurrentProject; }
            set { ucEntityReferences.CurrentProject = value; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public EntityReferenceWindow()
        {
            InitializeComponent();

            // Only allow the GIF animation at runtime.  If not, the control throws an exception when
            // hosted in here at design time.
            if(!this.DesignMode)
                ucEntityReferences.AllowAnimatedGif = true;

            ucEntityReferences.FileContentNeeded += ucEntityReferences_FileContentNeeded;
        }
        #endregion

        #region Routed event handlers
        //=====================================================================

        /// <summary>
        /// This is used to get information from token, content layout, and site map files open in editors
        /// so that current information is displayed for them in the entity references control.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucEntityReferences_FileContentNeeded(object sender, FileContentNeededEventArgs e)
        {
            ContentLayoutWindow contentLayoutWindow;
            SiteMapEditorWindow siteMapEditorWindow;
            TokenEditorWindow tokenEditorWindow;

            foreach(IDockContent content in this.DockPanel.Documents)
            {
                contentLayoutWindow = content as ContentLayoutWindow;

                if(contentLayoutWindow != null)
                    e.ContentLayoutFiles.Add(contentLayoutWindow.Filename, contentLayoutWindow.Topics);
                else
                {
                    siteMapEditorWindow = content as SiteMapEditorWindow;

                    if(siteMapEditorWindow != null)
                        e.SiteMapFiles.Add(siteMapEditorWindow.Filename, siteMapEditorWindow.Topics);
                    else
                    {
                        tokenEditorWindow = content as TokenEditorWindow;

                        if(tokenEditorWindow != null)
                            e.TokenFiles.Add(tokenEditorWindow.Filename, tokenEditorWindow.Tokens);
                    }
                }
            }
        }
        #endregion
    }
}
