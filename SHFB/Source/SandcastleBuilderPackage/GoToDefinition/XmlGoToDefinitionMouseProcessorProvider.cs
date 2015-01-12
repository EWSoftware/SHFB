//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : XmlGoToDefinitionMouseProcessorProvider.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us) - Based on code originally written by Noah Richards
// Updated : 01/09/2015
// Note    : Copyright 2014-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that creates the mouse processor specific to MAML files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
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
    /// This is used to provide the mouse processor that highlights available MAML element link targets and goes
    /// to their definitions in their related files.
    /// </summary>
    /// <remarks>For code entity references, this only supports C#.  See <see cref="CodeEntitySearcher"/> for the
    /// reasons why.</remarks>
    [Export(typeof(IMouseProcessorProvider))]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [ContentType("xml")]
    [Name("SHFB XML Go To Definition Mouse Processor Provider")]
    [Order(Before = "WordSelection")]
    internal sealed class XmlGoToDefinitionMouseProcessorProvider : IMouseProcessorProvider
    {
        [Import]
        private IClassifierAggregatorService AggregatorFactory = null;

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

            return new XmlGoToDefinitionMouseProcessor(view, GlobalServiceProvider,
                AggregatorFactory.GetClassifier(buffer), NavigatorService.GetTextStructureNavigator(buffer),
                CtrlKeyState.GetStateForView(view));
        }
    }
}
