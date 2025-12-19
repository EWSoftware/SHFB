//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MarkdownGoToDefinitionMouseProcessorProvider.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/16/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the class that creates the mouse processor specific to Markdown files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/16/2025  EFW  Created the code
//===============================================================================================================

using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;

namespace SandcastleBuilder.Package.GoToDefinition;

/// <summary>
/// This is used to provide the mouse processor that highlights available Markdown element link targets and goes
/// to their definitions in their related files.
/// </summary>
/// <remarks>For code entity references, this only supports C#.  See <see cref="CodeEntitySearcher"/> for the
/// reasons why.</remarks>
[Export(typeof(IMouseProcessorProvider))]
[TextViewRole(PredefinedTextViewRoles.Document)]
[ContentType("vs-markdown")]
[Name("SHFB Markdown Go To Definition Mouse Processor Provider")]
[Order(Before = "WordSelection")]
internal sealed class MarkdownGoToDefinitionMouseProcessorProvider : IMouseProcessorProvider
{
    [Import]
    private IViewClassifierAggregatorService AggregatorFactory = null;

    [Import]
    private ITextStructureNavigatorSelectorService NavigatorService = null;

    [Import]
    private SVsServiceProvider GlobalServiceProvider = null;

    /// <inheritdoc />
    public IMouseProcessor GetAssociatedProcessor(IWpfTextView view)
    {
        var options = new MefProviderOptions(GlobalServiceProvider);

        if(!options.EnableGoToDefinition || !options.EnableCtrlClickGoToDefinition)
            return null;

        var buffer = view.TextBuffer;

        return new MarkdownGoToDefinitionMouseProcessor(view, GlobalServiceProvider,
            AggregatorFactory.GetClassifier(view), NavigatorService.GetTextStructureNavigator(buffer),
            CtrlKeyState.GetStateForView(view));
    }
}
