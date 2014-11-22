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
            if (completionBroker == null)
                throw new ArgumentNullException("completionBroker");
            if (textView == null)
                throw new ArgumentNullException("textView");

            _completionBroker = completionBroker;
            _textView = textView;
        }

        protected override OLECMDF QueryCommandStatus(ref Guid commandGroup, uint commandId)
        {
            if (commandGroup == typeof(VSConstants.VSStd2KCmdID).GUID)
            {
                switch ((VSConstants.VSStd2KCmdID)commandId)
                {
                case VSConstants.VSStd2KCmdID.RETURN:
                case VSConstants.VSStd2KCmdID.TAB:
                case VSConstants.VSStd2KCmdID.TYPECHAR:
                    if (!_completionBroker.IsCompletionActive(_textView))
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
            if (commandGroup == typeof(VSConstants.VSStd2KCmdID).GUID)
            {
                switch ((VSConstants.VSStd2KCmdID)commandId)
                {
                case VSConstants.VSStd2KCmdID.RETURN:
                    return HandleReturn();

                case VSConstants.VSStd2KCmdID.TAB:
                    return HandleTab();

                case VSConstants.VSStd2KCmdID.TYPECHAR:
                    char typedChar = Convert.ToChar(Marshal.GetObjectForNativeVariant(pvaIn));
                    return HandleTypeChar(typedChar);

                default:
                    break;
                }
            }

            return false;
        }

        private bool HandleReturn()
        {
            return HandleCompletion();
        }

        private bool HandleTab()
        {
            return HandleCompletion();
        }

        private bool HandleTypeChar(char typedChar)
        {
            switch (typedChar)
            {
            case '/':
            case '>':
                return HandleCompletion();

            default:
                return false;
            }
        }

        private bool HandleCompletion()
        {
            ReadOnlyCollection<ICompletionSession> completionSessions = _completionBroker.GetSessions(_textView);
            if (completionSessions.Count == 0)
                return false;

            ICompletionSession completionSession = completionSessions[0];
            if (completionSession.IsDismissed)
                return false;

            CompletionSet completionSet = completionSession.SelectedCompletionSet;
            CompletionSelectionStatus selectionStatus = completionSet.SelectionStatus;
            if (selectionStatus.Completion.GetType().Name == "CustomCommitCompletion")
            {
                // let Roslyn handle its own completions
                return false;
            }

            completionSession.Commit();
            return true;
        }
    }
}
