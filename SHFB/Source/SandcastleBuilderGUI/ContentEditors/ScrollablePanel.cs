//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : ScrollablePanel.cs
// Author  : RussKie
// Updated : 08/30/2014
// Note    : Copyright 2009-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a scrollable panel with double-buffer support
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// =====================================================================================================
// 08/30/2015  RK   Added by RussKie
//===============================================================================================================

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// A scrollable panel with double-buffer support
    /// </summary>
    [ComVisible(true),
     ClassInterface(ClassInterfaceType.AutoDispatch),
     DefaultProperty("BorderStyle"),
     DefaultEvent("Paint"),
     Docking(DockingBehavior.Ask),
     Designer("System.Windows.Forms.Design.PanelDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    class ScrollablePanel : Panel
    {
        /// <inheritdoc />
        public ScrollablePanel()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
        }

        /// <inheritdoc />
        protected override void OnScroll(ScrollEventArgs se)
        {
            this.Invalidate();

            base.OnScroll(se);
        }

        /// <inheritdoc />
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_CLIPCHILDREN
                return cp;
            }
        }
    }
}
