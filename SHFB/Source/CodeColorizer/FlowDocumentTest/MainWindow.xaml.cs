//===============================================================================================================
// System  : C# Code Colorizer Library
// File    : MainWindow.xaml
// Author  : Eric Woodruff
// Updated : 12/18/2012
// Compiler: Microsoft Visual C#
//
// This is used to demonstrate colorizing code for insertion into a XAML flow document.
//
// This code may be used in compiled form in any way you desire.  This file may be redistributed unmodified by
// any means PROVIDING it is not sold for profit without the author's written consent.  This notice, the author's
// name, and all copyright notices must remain intact in all applications, documentation, and source files.
//
// This code is provided "as is" with no warranty either express or implied. The author accepts no liability for
// any damage or loss of business that this product may cause.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  12/18/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

using ColorizerLibrary;

namespace FlowDocumentTest
{
    /// <summary>
    /// This is a simple demo of colorizing code and inserting it into a XAML flow document.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private data members
        //=====================================================================

        private CodeColorizer codeColorizer;
        private string flowTemplate;

        private static Regex reRemoveLineNumbers = new Regex(@"^\s*\d+\| ", RegexOptions.Multiline);
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            cboLanguage.SelectedIndex = 4;
            txtSampleText.Text = File.ReadAllText(@"..\..\MainWindow.xaml");

            // Load the template flow document
            flowTemplate = File.ReadAllText(@"..\..\..\ColorizerLibrary\Template\DocumentTemplate.xaml");

            // Create the code colorizer
            codeColorizer = new CodeColorizer();
            codeColorizer.Init();
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Colorize the given text using the current option settings
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnColorize_Click(object sender, RoutedEventArgs e)
        {
            string title;
            int tabSize;

            // The flow document colorizer only supports line numbering.  It doesn't support outlining.
            codeColorizer.NumberLines = chkNumberLines.IsChecked.Value;

            // Use zero to indicate the default tab size if not specified or not valid
            if(txtTabSize.Text.Trim().Length == 0 || !Int32.TryParse(txtTabSize.Text, out tabSize))
                tabSize = 0;

            if(!codeColorizer.FriendlyNames.TryGetValue(cboLanguage.Text, out title))
                title = cboLanguage.Text;

            lblTitle.Text = title;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                paraContent.Inlines.Clear();

                // Colorize the code
                string colorizedContent = codeColorizer.ColorizePlainText(txtSampleText.Text, cboLanguage.Text,
                    false, tabSize);

                // Insert the colorized code into the flow document template
                colorizedContent = flowTemplate.Replace("@CONTENT@", colorizedContent);

                // The following steps can take a few seconds for large documents with complex coloring such
                // as large XML files.
                var fd = XamlReader.Parse(colorizedContent) as FlowDocument;

                if(fd != null)
                    using(MemoryStream stream = new MemoryStream())
                    {
                        // Flow document elements are attached to their parent document.  To break the bond we
                        // need to stream it out and then back in again before adding them to the current
                        // document.  A side effect of this is that it converts the named styles into literal
                        // style elements so we don't need the named styles added to the end document.
                        Block b = fd.Blocks.FirstBlock;

                        TextRange range = new TextRange(b.ContentStart, b.ContentEnd);
                        range.Save(stream, DataFormats.XamlPackage);

                        range = new TextRange(paraContent.ContentEnd, paraContent.ContentEnd);
                        range.Load(stream, DataFormats.XamlPackage);
                    }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to colorize code: " + ex.Message, "Syntax Highlighter Test",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Copy the selected code block content to the clipboard
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void hlCopy_Click(object sender, RoutedEventArgs e)
        {
            // Note that this makes an assumption about the structure of the document
            var fe = ((Hyperlink)sender).Parent as FrameworkElement;

            fe = fe.Parent as FrameworkElement;

            var b = fe.Parent as BlockUIContainer;

            if(b != null)
            {
                var sec = b.NextBlock as Section;

                if(sec != null)
                {
                    var para = sec.Blocks.FirstBlock as Paragraph;

                    if(para != null)
                    {
                        TextRange r = new TextRange(para.ContentStart, para.ContentEnd);

                        // Remove line numbers from the front of each line if present and copy the text
                        // to the clipboard.
                        Clipboard.SetText(reRemoveLineNumbers.Replace(r.Text, String.Empty));
                    }
                }
            }
        }
        #endregion
    }
}
