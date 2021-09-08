﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MamlLinkQuickInfoSourceProvider.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/09/2015
// Note    : Copyright 2014-2015, Eric Woodruff, All rights reserved
//
// This file contains the class that creates the quick info source specific to MAML elements
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
    /// This class creates the quick info source specific to MAML elements
    /// </summary>
// TODO: Type is obsolete.  Update to use IAsyncQuickInfoSourceProvider
#pragma warning disable CS0618
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("MAML Link Quick Info Provider")]
    [Order(After = "Default Quick Info Presenter")]
    [ContentType("xml")]
    internal sealed class MamlLinkQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        private SVsServiceProvider GlobalServiceProvider = null;

        [Import]
        internal IViewTagAggregatorFactoryService AggregatorFactory { get; set; }

        /// <inheritdoc />
        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            var options = new MefProviderOptions(GlobalServiceProvider);

            if(!options.EnableGoToDefinition)
                return null;

            return new MamlLinkQuickInfoSource(GlobalServiceProvider, textBuffer, this,
                options.EnableCtrlClickGoToDefinition);
        }
    }
#pragma warning restore CS0618
}
