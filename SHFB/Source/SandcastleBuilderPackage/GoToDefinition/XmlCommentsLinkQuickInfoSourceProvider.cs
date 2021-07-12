//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : XmlCommentsLinkQuickInfoSourceProvider.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2019
// Note    : Copyright 2014-2019, Eric Woodruff, All rights reserved
//
// This file contains the class that creates the quick info source specific to XML comments elements
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

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class creates the quick info source specific to XML comments elements
    /// </summary>
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("XML Comments Link Quick Info Provider")]
    [Order(After = "Default Quick Info Presenter")]
    [ContentType("csharp")]
    internal sealed class XmlCommentsLinkQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        private readonly SVsServiceProvider GlobalServiceProvider = null;

        [Import]
        internal IViewTagAggregatorFactoryService AggregatorFactory { get; set; }

        /// <inheritdoc />
        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            var options = new MefProviderOptions(GlobalServiceProvider);

            if(!options.EnableGoToDefinition)
                return null;

            return new XmlCommentsLinkQuickInfoSource(GlobalServiceProvider, textBuffer, this,
                options.EnableCtrlClickGoToDefinition);
        }
    }
}
