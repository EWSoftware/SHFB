//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : CSharpGoToDefinitionMouseProcessorProvider.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us) - Based on code originally written by Noah Richards
// Updated : 06/19/2019
// Note    : Copyright 2014-2019, Eric Woodruff, All rights reserved
//
// This file contains the class that creates the mouse processor specific to C# code
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

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This is used to provide the mouse processor that highlights available <c>cref</c> attribute,
    /// conceptualLink element, and token element link targets in XML comments and goes to their definitions.
    /// </summary>
    /// <remarks>This only supports C#.  See <see cref="CodeEntitySearcher"/> for the reasons why.</remarks>
    [Export(typeof(IMouseProcessorProvider))]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [ContentType("csharp")]
    [Name("SHFB C# Go To Definition Mouse Processor Provider")]
    [Order(Before = "WordSelection")]
    internal sealed class CSharpGoToDefinitionMouseProcessorProvider : IMouseProcessorProvider
    {
        [Import]
        private readonly IClassifierAggregatorService AggregatorFactory = null;

        [Import]
        private readonly ITextStructureNavigatorSelectorService NavigatorService = null;

        [Import]
        private readonly SVsServiceProvider GlobalServiceProvider = null;

        /// <inheritdoc />
        public IMouseProcessor GetAssociatedProcessor(IWpfTextView view)
        {
            var options = new MefProviderOptions(GlobalServiceProvider);

            if(!options.EnableGoToDefinition || !options.EnableCtrlClickGoToDefinition)
                return null;

            var buffer = view.TextBuffer;

            return new CSharpGoToDefinitionMouseProcessor(view, GlobalServiceProvider, AggregatorFactory.GetClassifier(buffer),
                NavigatorService.GetTextStructureNavigator(buffer), CtrlKeyState.GetStateForView(view));
        }
    }
}
