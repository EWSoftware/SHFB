//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : TokenFileEditorPane.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/26/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to host the token file editor control
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/26/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.Shell.Interop;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.WPF;
using SandcastleBuilder.WPF.UserControls;

namespace SandcastleBuilder.Package.Editors
{
    /// <summary>
    /// This is used to host the token file editor control
    /// </summary>
    public class TokenEditorPane : SimpleEditorPane<TokenEditorFactory, TokenEditorControl>
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the filename
        /// </summary>
        public string Filename
        {
            get { return base.UIControl.Tokens.TokenFilePath; }
        }

        /// <summary>
        /// This read-only property returns the current token collection including any unsaved edits
        /// </summary>
        public TokenCollection Tokens
        {
            get
            {
                base.UIControl.CommitChanges();
                return base.UIControl.Tokens;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public TokenEditorPane()
        {
            base.UIControl.ContentModified += ucTokenEditor_ContentModified;
        }
        #endregion

        #region Abstract method implementations
        //=====================================================================

        /// <inheritdoc />
        protected override string GetFileExtension()
        {
            return ".tokens";
        }

        /// <inheritdoc />
        protected override Guid GetCommandSetGuid()
        {
            return Guid.Empty;
        }

        /// <inheritdoc />
        protected override void LoadFile(string fileName)
        {
            base.UIControl.LoadTokenFile(fileName);
        }

        /// <inheritdoc />
        protected override void SaveFile(string fileName)
        {
            Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true).SetWaitCursor();

            base.UIControl.CommitChanges();

            if(base.IsDirty || !fileName.Equals(base.UIControl.Tokens.TokenFilePath, StringComparison.OrdinalIgnoreCase))
            {
                base.UIControl.Tokens.TokenFilePath = fileName;
                base.UIControl.Tokens.Save();
            }
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// This is used to mark the file as dirty when the collection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucTokenEditor_ContentModified(object sender, System.Windows.RoutedEventArgs e)
        {
            base.OnContentChanged();
        }
        #endregion
    }
}
