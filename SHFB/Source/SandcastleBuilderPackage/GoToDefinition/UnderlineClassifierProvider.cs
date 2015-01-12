//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : UnderlineClassifierProvider.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us) - Based on code originally written by Noah Richards
// Updated : 01/09/2015
// Note    : Copyright 2014-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that creates the underline classifier used by the MAML and XML comments Go To
// Definition handlers.
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

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class is used to create the underline classifier used by the MAML and XML comments Go To Definition
    /// handlers.
    /// </summary>
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("csharp")]
    [ContentType("xml")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class UnderlineClassifierProvider : IViewTaggerProvider
    {
        #region Internal data members
        //=====================================================================

        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(UnderlineClassifier.UnderlineClassifierType)]
        internal static ClassificationTypeDefinition underlineClassificationType = null;

        internal static IClassificationType UnderlineClassification;

        [Import]
        private SVsServiceProvider serviceProvider = null;
        #endregion

        #region IViewTaggerProvider implementation
        //=====================================================================

        /// <inheritdoc />
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var options = new MefProviderOptions(serviceProvider);

            if(textView.TextBuffer != buffer || !options.EnableGoToDefinition || !options.EnableCtrlClickGoToDefinition)
                return null;

            if(UnderlineClassification == null)
                UnderlineClassification = ClassificationRegistry.GetClassificationType(UnderlineClassifier.UnderlineClassifierType);

            return GetClassifierForView(textView) as ITagger<T>;
        }
        #endregion

        #region Helper method
        //=====================================================================

        /// <summary>
        /// This helper method is used to get the underline classifier for the given view
        /// </summary>
        /// <param name="view">The view for which to get the underline classifier</param>
        /// <returns>The underline classifier for the view or null if not found or it could not be set</returns>
        public static UnderlineClassifier GetClassifierForView(ITextView view)
        {
            if(UnderlineClassification == null)
                return null;

            return view.Properties.GetOrCreateSingletonProperty(() => new UnderlineClassifier());
        }
        #endregion
    }
}
