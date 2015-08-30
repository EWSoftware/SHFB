using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// A scrollbale panel with double-buffer support.
    /// </summary>
    [ComVisible(true),
     ClassInterface(ClassInterfaceType.AutoDispatch),
     DefaultProperty("BorderStyle"),
     DefaultEvent("Paint"),
     Docking(DockingBehavior.Ask),
     Designer("System.Windows.Forms.Design.PanelDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    class MyScrollablePanel : Panel
    {
        public MyScrollablePanel()
        {
            this.SetStyle(ControlStyles.UserPaint | 
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            this.Invalidate();

            base.OnScroll(se);
        }
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
