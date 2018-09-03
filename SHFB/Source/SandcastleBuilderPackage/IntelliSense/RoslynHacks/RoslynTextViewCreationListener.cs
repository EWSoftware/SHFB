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
            var options = new MefProviderOptions(_serviceProvider);

            // only hook when necessary
#pragma warning disable VSTHRD010
            if((!RoslynUtilities.IsRoslynInstalled(_serviceProvider) ?? true) || !options.EnableExtendedXmlCommentsCompletion)
                return;
#pragma warning restore VSTHRD010

            ITextView textView = _editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

            if(textView == null)
                return;

            RoslynKeyboardFilter filter = new RoslynKeyboardFilter(_completionBroker, textViewAdapter, textView);
            filter.Enabled = true;
            textView.Properties.AddProperty(typeof(RoslynKeyboardFilter), filter);
        }
    }
}
