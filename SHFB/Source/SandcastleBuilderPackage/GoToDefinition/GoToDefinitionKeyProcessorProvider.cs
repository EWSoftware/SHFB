//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : GoToDefinitionKeyProcessorProvider.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us) - Based on code originally written by Noah Richards
// Updated : 01/09/2015
// Note    : Copyright 2014-2015, Eric Woodruff, All rights reserved
//
// This file contains the class used to provide the key processor that tracks Ctrl key state for C# and XML files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/01/2014  EFW  Created the code
//===============================================================================================================

using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This is used to provide the key processor that tracks Ctrl key state for C# and XML files
    /// </summary>
    /// <remarks>This is used in conjunction with the Go To Definition mouse tracker for XML comments and MAML
    /// link elements.  For code, this only supports C#.  See <see cref="CodeEntitySearcher"/> for the reasons
    /// why.</remarks>
    [Export(typeof(IKeyProcessorProvider))]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [ContentType("csharp")]
    [ContentType("xml")]
    [Name("SHFB Go To Definition Key Processor Provider")]
    [Order(Before = "VisualStudioKeyboardProcessor")]
    internal sealed class GoToDefinitionKeyProcessorProvider : IKeyProcessorProvider
    {
        [Import]
        private SVsServiceProvider serviceProvider = null;

        public KeyProcessor GetAssociatedProcessor(IWpfTextView view)
        {
            var options = new MefProviderOptions(serviceProvider);

            if(!options.EnableGoToDefinition || !options.EnableCtrlClickGoToDefinition)
                return null;

            return view.Properties.GetOrCreateSingletonProperty(typeof(GoToDefinitionKeyProcessor),
                () => new GoToDefinitionKeyProcessor(CtrlKeyState.GetStateForView(view)));
        }
    }
}
