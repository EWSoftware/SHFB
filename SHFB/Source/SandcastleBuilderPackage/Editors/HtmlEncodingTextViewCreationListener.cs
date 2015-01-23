//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : HtmlEncodingTextViewCreationListener.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/07/2015
// Note    : Copyright 2012-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to connect the HTML encoding command filter to the text view adapters
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/23/2012  EFW  Created the code
//===============================================================================================================

using System.ComponentModel.Composition;
using System.Diagnostics;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace SandcastleBuilder.Package.Editors
{
    /// <summary>
    /// This class is used to connect the HTML encoding command filter to the text view adapters
    /// </summary>
    /// <remarks>The command filter is only connected to HTML and XML text view adapters.  The command will be
    /// available to HTML and XML files in any project type, not just SHFB projects.</remarks>
    [Export(typeof(IVsTextViewCreationListener)), ContentType("HTML"), ContentType("XML"),
      TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class HtmlEncodingTextViewCreationListener : IVsTextViewCreationListener
    {
        #region Private data members
        //=====================================================================

        // The Import attribute causes the composition container to assign a value to this when an instance
        // is created.  It is not assigned to within this class.
        [Import]
        private IVsEditorAdaptersFactoryService adaptersFactory = null;

        [Import]
        internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        #endregion

        #region IVsTextViewCreationListener implementation
        //=====================================================================

        /// <summary>
        /// This connects our command filter to the text view adapter
        /// </summary>
        /// <param name="textViewAdapter">The text view adapter to use</param>
        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            HtmlEncodingCommandTarget filter;
            IOleCommandTarget nextTarget;

            var wpfTextView = adaptersFactory.GetWpfTextView(textViewAdapter);

            if(wpfTextView == null)
            {
                Debug.Fail("Unable to get IWpfTextView from text view adapter");
                return;
            }

            filter = new HtmlEncodingCommandTarget(wpfTextView);

            if(ErrorHandler.Succeeded(textViewAdapter.AddCommandFilter(filter, out nextTarget)))
            {
                filter.NextTarget = nextTarget;
                wpfTextView.Properties.GetOrCreateSingletonProperty(() => filter);
            }
        }
        #endregion
    }
}
