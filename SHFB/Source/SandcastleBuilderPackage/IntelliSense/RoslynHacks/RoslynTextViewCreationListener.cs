namespace SandcastleBuilder.Package.IntelliSense.RoslynHacks
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text;
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
        public RoslynTextViewCreationListener(SVsServiceProvider serviceProvider,
          IVsEditorAdaptersFactoryService editorAdaptersFactoryService, ICompletionBroker completionBroker)
        {
            _serviceProvider = serviceProvider;
            _editorAdaptersFactoryService = editorAdaptersFactoryService;
            _completionBroker = completionBroker;
        }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
#pragma warning disable VSTHRD010
            // Disable this in VS 2019 for now as it causes problems and non-async completions aren't supported
            // anyway.
            if(typeof(ITextBuffer).Assembly.GetName().Version.Major < 16)
            {
                var options = new MefProviderOptions(_serviceProvider);

                if(options.EnableExtendedXmlCommentsCompletion)
                {
                    ITextView textView = _editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

                    if(textView != null)
                    {
                        textView.Properties.AddProperty(typeof(RoslynKeyboardFilter),
                            new RoslynKeyboardFilter(_completionBroker, textViewAdapter, textView) { Enabled = true });
                    }
                }
            }
#pragma warning restore VSTHRD010
        }
    }
}
