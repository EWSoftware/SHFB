//=============================================================================
// System  : Code Colorizer Library
// File    : TextColorizerControl.cs
// Author  : Jonathan de Halleux, (c) 2003
// Updated : 11/17/2006
// Compiler: Microsoft Visual C#
//
// This is used to display syntax colorized blocks of text in an HTML page.
// The original Code Project article by Jonathan can be found at:
// http://www.codeproject.com/csharp/highlightcs.asp.
//
// Modifications by Eric Woodruff (Eric@EWoodruff.us) 11/2006:
//
//      Allowed null or empty text.
//
//      Performance data is written to the debug window.
//
//=============================================================================

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using ColorizerLibrary;

namespace ColorizerLibrary.Controls
{
	/// <summary>
	/// A syntax colorizing control
	/// </summary>
    /// <remarks>Original Author: Jonathan de Halleux, dehalleux@pelikhan.com,
    /// 2003.
    /// <p/>Modified by Eric Woodruff (Eric@EWoodruff.us) 11/2006. The original
    /// Code Project article and code by Jonathan can be found at:
    /// <a href="http://www.codeproject.com/csharp/highlightcs.asp">
    /// http://www.codeproject.com/csharp/highlightcs.asp</a>.</remarks>
    [DefaultProperty("Text"), Description("A syntax colorizing control"),
		ToolboxData("<{0}:TextColorizerControl runat=server></{0}:TextColorizerControl>")]
	public class TextColorizerControl : System.Web.UI.WebControls.WebControl
	{
		#region Private data members

        private CodeColorizer colorizer;
        private string text;

		#endregion

        #region Properties
        /// <summary>
        /// The syntax engine does the job.
        /// </summary>
        [Browsable(false)]
        public CodeColorizer SyntaxEngine
        {
            get { return colorizer; }
            set { colorizer = value; }
        }

        /// <summary>
        /// Get or set the text to colorize
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""),
          Description("The text to display")]
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        #endregion

		/// <summary>
		/// Renders the code
		/// </summary>
		/// <remarks>You must specify a colorizing engine using SetSyntaxEngine
        /// before calling this method</remarks>
		/// <param name="writer">HTML writer</param>
		protected override void Render(HtmlTextWriter writer)
		{
            if(!String.IsNullOrEmpty(text))
    			if(colorizer == null)
	    		{
		    		writer.Write("<b>Colorizer not set !</b><br/>");
                    writer.Write(text);
			    }
                else
                {
		    	    writer.Write(colorizer.ProcessAndHighlightText(text));
#if DEBUG
        			System.Diagnostics.Debug.WriteLine( "Performance: " +
                        colorizer.BenchmarkSec + " s, " +
                        colorizer.BenchmarkSecPerChar + " s/char, " +
                        colorizer.BenchmarkAvgSec + " s" );
#endif
                }
		}
	}
}
