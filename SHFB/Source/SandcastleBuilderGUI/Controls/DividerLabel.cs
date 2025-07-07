//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : DividerLabel.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains a simple derived label control that draws the label text followed by a dividing line to
// the right of the text.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/02/2006  EFW  Created the code
//===============================================================================================================

using System;
using System.Drawing;
using System.Drawing.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace SandcastleBuilder.Gui.Controls
{
	/// <summary>
    /// This is a simple derived label control that draws the label text followed by a dividing line to the
    /// right of the text.
	/// </summary>
    [Description("A label that draws a dividing line to the right of the text")]
    public class DividerLabel : Label
	{
        /// <summary>
        /// Constructor
        /// </summary>
		public DividerLabel()
		{
		}

        //=====================================================================
        // Methods, etc

        /// <summary>
        /// This is overridden to draw the text and the dividing line
        /// </summary>
        /// <param name="e">The event arguments</param>
		protected override void OnPaint(PaintEventArgs e)
		{
    		Point pt, pt2;
            Rectangle r = this.ClientRectangle;
            SizeF size;
            Graphics g = e?.Graphics;

            using StringFormat sf = new();
            sf.Trimming = StringTrimming.Character;
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Center;

            if(this.RightToLeft == RightToLeft.Yes)
                sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

            if(this.UseMnemonic)
                sf.HotkeyPrefix = HotkeyPrefix.Show;

            size = g.MeasureString(this.Text, this.Font, r.Size, sf);

            if(this.Enabled)
            {
                using Brush fb = new SolidBrush(this.ForeColor);
                g.DrawString(this.Text, this.Font, fb, r, sf);
            }
            else
                ControlPaint.DrawStringDisabled(g, this.Text, this.Font, this.BackColor, r, sf);

            pt = new Point(r.Left, r.Top + (r.Height / 2));
            pt2 = new Point(r.Right, pt.Y);

            if(this.RightToLeft == RightToLeft.Yes)
                pt2.X -= (int)size.Width;
            else
                pt.X += (int)size.Width;

            using Pen pb = new(ControlPaint.Dark(this.BackColor), SystemInformation.BorderSize.Height);
            
            if(this.FlatStyle == FlatStyle.Flat)
                g.DrawLine(pb, pt, pt2);
            else
            {
                using Pen pf = new(ControlPaint.LightLight(this.BackColor), SystemInformation.BorderSize.Height);
                
                g.DrawLine(pb, pt, pt2);

                int offset = (int)Math.Ceiling(SystemInformation.BorderSize.Height / 2.0);
                pt.Offset(0, offset);
                pt2.Offset(0, offset);

                g.DrawLine(pf, pt, pt2);
            }
        }
	}
}
