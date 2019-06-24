namespace SandcastleBuilder.Package.IntelliSense.RoslynHacks
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Text.Editor;
    using IVsTextView = Microsoft.VisualStudio.TextManager.Interop.IVsTextView;
    using OLECMDEXECOPT = Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT;
    using OLECMDF = Microsoft.VisualStudio.OLE.Interop.OLECMDF;

    internal sealed class RoslynKeyboardFilter : TextViewCommandFilter
    {
        private readonly ICompletionBroker _completionBroker;
        private readonly ITextView _textView;

        public RoslynKeyboardFilter(ICompletionBroker completionBroker, IVsTextView textViewAdapter, ITextView textView)
            : base(textViewAdapter)
        {
            _completionBroker = completionBroker ?? throw new ArgumentNullException(nameof(completionBroker));
            _textView = textView ?? throw new ArgumentNullException(nameof(textView));
        }

        protected override OLECMDF QueryCommandStatus(ref Guid commandGroup, uint commandId)
        {
            if(commandGroup == typeof(VSConstants.VSStd2KCmdID).GUID)
            {
                switch((VSConstants.VSStd2KCmdID)commandId)
                {
                    case VSConstants.VSStd2KCmdID.RETURN:
                    case VSConstants.VSStd2KCmdID.TAB:
                    case VSConstants.VSStd2KCmdID.TYPECHAR:
                        if(!_completionBroker.IsCompletionActive(_textView))
                            return 0;

                        return OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED;

                    default:
                        break;
                }
            }

            return 0;
        }

        protected override bool HandlePreExec(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
            if(commandGroup == typeof(VSConstants.VSStd2KCmdID).GUID)
            {
                switch((VSConstants.VSStd2KCmdID)commandId)
                {
                    case VSConstants.VSStd2KCmdID.RETURN:
                        return HandleCompletion('\n');

                    case VSConstants.VSStd2KCmdID.TAB:
                        return HandleCompletion('\t');

                    case VSConstants.VSStd2KCmdID.TYPECHAR:
                        char typedChar = Convert.ToChar(Marshal.GetObjectForNativeVariant(pvaIn));

                        switch(typedChar)
                        {
                            case '/':
                            case '>':
                            case ' ':
                                return HandleCompletion(typedChar);

                            default:
                                return false;
                        }

                    default:
                        break;
                }
            }

            return false;
        }

        // !EFW - This has been throwing random null reference exceptions in VS 2019.  It's not something I've
        // been able to duplicate so I've added null checks everywhere to see if that stops it.
        private bool HandleCompletion(char commitCharacter)
        {
            ReadOnlyCollection<ICompletionSession> completionSessions = _completionBroker.GetSessions(_textView);

            if(completionSessions == null || completionSessions.Count == 0)
                return false;

            ICompletionSession completionSession = completionSessions[0];

            if(completionSession == null || completionSession.IsDismissed)
                return false;

            CompletionSet completionSet = completionSession.SelectedCompletionSet;
            CompletionSelectionStatus selectionStatus = completionSet?.SelectionStatus;

            if(!(selectionStatus?.Completion is SandcastleCompletion))
            {
                // Workaround some odd behavior in the completion when trying to enter "</".  This prevents it
                // from converting it to an XML comment ("<!--/-->").
                if(commitCharacter == '/' && selectionStatus?.Completion?.InsertionText == "!--")
                    completionSession.Dismiss();

                // Let other providers handle their own completions
                return false;
            }

            string insertionText = selectionStatus.Completion.InsertionText;

            if(insertionText == null)
                return false;

            completionSession.Commit();

            bool passCharacterToEditor;

            switch(commitCharacter)
            {
                case '/':
                case '>':
                    // If the insertion text doesn't end with '>' or '/>', allow the user to complete the item and insert
                    // the closing element character by typing '/' or '>'.
                    passCharacterToEditor = !insertionText.EndsWith(">");
                    break;

                case ' ':
                    // Only pass the space through if the completion item doesn't contain any replaceable elements
                    passCharacterToEditor = insertionText.IndexOf('\xFF') < 0;
                    break;

                case '\n':
                case '\t':
                default:
                    // These items trigger completion, but aren't written to the output
                    passCharacterToEditor = false;
                    break;
            }

            return !passCharacterToEditor;
        }
    }
}
