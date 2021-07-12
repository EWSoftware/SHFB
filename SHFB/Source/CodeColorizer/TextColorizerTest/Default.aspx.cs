//===============================================================================================================
// System  : C# Code Colorizer Control Web Demo
// File    : Default.aspx.cs
// Author  : Jonathan de Halleux, (c) 2003
// Updated : 04/06/2021
//
// This is used to demonstrate the C# code colorizer control.  The original Code Project article by Jonathan can
// be found at:  http://www.codeproject.com/Articles/3767/Multiple-Language-Syntax-Highlighting-Part-2-C-Con
//
// Modifications by Eric Woodruff (Eric@EWoodruff.us) 11/2006-03/2007:
//
//      Reworked the demo to demonstrate the new features.
//
//===============================================================================================================

using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;

using ColorizerLibrary;

namespace TextColorizerTest
{
	/// <summary>
	/// Test page for the syntax highlighter
	/// </summary>
	public partial class WebForm1 : Page
    {
        private static CodeColorizer syntaxEngine;

        #region Page load handler
        //=====================================================================

        protected void Page_Load(object sender, EventArgs e)
		{
            if(!Page.IsPostBack)
            {
                // In this case, we'll use a static instance to improve performance with repeated use
                syntaxEngine = new CodeColorizer(Server.MapPath(@"~\highlight.xml"), Server.MapPath(@"~\highlight.xsl"));

                string demoSource = Server.MapPath("Default.aspx.cs");

                using(StreamReader sr = new StreamReader(demoSource))
                {
                    txtContent.Text = sr.ReadToEnd();
                }
            }
        }
        #endregion

        #region Button click handler
        //=====================================================================

        protected void btnProcess_Click(object sender, EventArgs e)
        {
            int tabSize;

            if(!Int32.TryParse(txtTabSize.Text, out tabSize))
                tabSize = 0;

            colorizerControl.SyntaxEngine = syntaxEngine;
            syntaxEngine.UseDefaultTitle = chkDefaultTitle.Checked;

            // Format the options and text into a tag and assign the text
            // to the control.  The text inside the tag is expected to be
            // HTML encoded.  Line numbering and outlining are ignored for
            // <code> tags.
            colorizerControl.Text = String.Format(CultureInfo.CurrentCulture,
                "The colorized text block: <{0} lang=\"{1}\" " +
                "numberLines=\"{2}\" outlining=\"{3}\" " +
                "tabSize=\"{4}\" keepSeeTags=\"{5}\" {6}>{7}</{0}>",
                cboTag.SelectedValue, cboLanguage.SelectedValue,
                chkNumberLines.Checked, chkOutlining.Checked, tabSize,
                chkKeepSeeTags.Checked,
                (txtTitle.Text.Length != 0) ? "title=\"" +
                    txtTitle.Text + "\"" : "",
                HttpUtility.HtmlEncode(txtContent.Text));

            btnProcess.Focus();
        }
        #endregion
    }
}
