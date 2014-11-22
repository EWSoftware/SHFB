namespace SandcastleBuilder.Package.IntelliSense.RoslynHacks
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("Roslyn Languages")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class RoslynTextViewCreationListener : IVsTextViewCreationListener
    {
        private readonly SVsServiceProvider _serviceProvider;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;
        private readonly ICompletionBroker _completionBroker;

        [ImportingConstructor]
        public RoslynTextViewCreationListener(SVsServiceProvider serviceProvider, IVsEditorAdaptersFactoryService editorAdaptersFactoryService, ICompletionBroker completionBroker)
        {
            _serviceProvider = serviceProvider;
            _editorAdaptersFactoryService = editorAdaptersFactoryService;
            _completionBroker = completionBroker;
        }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            // only hook when necessary
            if (!RoslynUtilities.IsRoslynInstalled(_serviceProvider) ?? true)
                return;

            ITextView textView = _editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            RoslynKeyboardFilter filter = new RoslynKeyboardFilter(_completionBroker, textViewAdapter, textView);
            filter.Enabled = true;
            textView.Properties.AddProperty(typeof(RoslynKeyboardFilter), filter);
        }
    }
}
