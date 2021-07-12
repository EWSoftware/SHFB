//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : GoToDefinitionTextViewCreationListener.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2019
// Note    : Copyright 2015-2019, Eric Woodruff, All rights reserved
//
// This file contains a class used to connect the Go To Definition command filter to the text view adapters
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/07/2015  EFW  Created the code
//===============================================================================================================

using System.ComponentModel.Composition;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class is used to connect the Go To Definition command filter to the text view adapters
    /// </summary>
    /// <remarks>The command filter is only connected to C# and XML text view adapters.  The command will be
    /// available to C# and XML files in any project type, not just SHFB projects.</remarks>
    [Export(typeof(IVsTextViewCreationListener)), ContentType("csharp"), ContentType("xml"),
      TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class GoToDefinitionTextViewCreationListener : IVsTextViewCreationListener
    {
        #region MEF imports
        //=====================================================================

        [Import]
        private readonly IVsEditorAdaptersFactoryService editorAdaptersFactoryService = null;

        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        public IClassifierAggregatorService ClassifierAggregatorService { get; set; }

        [Import]
        public ITextStructureNavigatorSelectorService TextStructureNavigatorSelectorService { get; set; }

        #endregion

        #region IVsTextViewCreationListener implementation
        //=====================================================================

        /// <inheritdoc />
        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var options = new MefProviderOptions(ServiceProvider);

            if(options.EnableGoToDefinition)
            {
                var textView = editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

                if(textView != null)
                {
                    var filter = new GoToDefinitionCommandTarget(textView, this);

                    if(ErrorHandler.Succeeded(textViewAdapter.AddCommandFilter(filter, out IOleCommandTarget nextTarget)))
                    {
                        filter.NextTarget = nextTarget;
                        textView.Properties.GetOrCreateSingletonProperty(() => filter);
                    }
                }
            }
        }
        #endregion
    }
}

