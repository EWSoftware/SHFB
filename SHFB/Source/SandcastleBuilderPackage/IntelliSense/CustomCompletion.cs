//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : CustomCompletion.cs
// Author  : Sam Harwell  (sam@tunnelvisionlabs.com)
// Updated : 03/21/2014
// Note    : Copyright 2014, Sam Harwell, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a custom completion with support for placing the caret after insertion
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/21/2014  EFW  Added the code to the help file builder package.  Changed the insertion character to one
//                  that doesn't conflict with possible insertion text.
//===============================================================================================================

using System;
using System.Windows.Media;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace SandcastleBuilder.Package.IntelliSense
{
    /// <summary>
    /// Represents a completion item, including the icon, insertion text, and display text, in a CompletionSet.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="Completion"/> class by allowing the <see cref="Completion.InsertionText"/>
    /// to contain a <c>\xFF</c> character to represent the placement of the caret after the completion is
    /// inserted into the editor.
    /// </remarks>
    internal sealed class CustomCompletion : Completion, ICustomCommit
    {
        private readonly ICompletionSession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomCompletion"/> class with the specified text and
        /// description.
        /// </summary>
        /// <param name="session">The completion session.</param>
        /// <param name="displayText">The text that is to be displayed by an IntelliSense presenter.</param>
        /// <param name="insertionText">The text that is to be inserted into the buffer if this completion is
        /// committed.</param>
        /// <param name="description">A description that can be displayed with the display text of the
        /// completion.</param>
        /// <param name="iconSource">The icon.</param>
        /// <param name="iconAutomationText">The text to be used as the automation name for the icon.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="session"/> is <see langword="null"/>.</exception>
        public CustomCompletion(ICompletionSession session, string displayText, string insertionText,
          string description, ImageSource iconSource, string iconAutomationText) :
            base(displayText, insertionText, description, iconSource, iconAutomationText)
        {
            if(session == null)
                throw new ArgumentNullException("session");

            this.session = session;
        }

        /// <inheritdoc />
        public void Commit()
        {
            if(!session.SelectedCompletionSet.SelectionStatus.IsSelected)
                return;

            ITrackingSpan applicableTo = session.SelectedCompletionSet.ApplicableTo;

            using(ITextEdit edit = applicableTo.TextBuffer.CreateEdit())
            {
                // The insertion text is inserted without the \xFF character (if any)
                string insertionText = InsertionText.Replace("\xFF", "");
                edit.Replace(applicableTo.GetSpan(edit.Snapshot), insertionText);
                ITextSnapshot applied = edit.Apply();

                // The original position of the \xFF character determines the placement of the caret
                int caretOffset = InsertionText.IndexOf('\xFF');

                if(caretOffset >= 0)
                {
                    SnapshotPoint startPoint = applicableTo.GetStartPoint(applied);
                    SnapshotPoint caretPoint = startPoint + caretOffset;
                    session.TextView.Caret.MoveTo(caretPoint, PositionAffinity.Predecessor);
                }
            }
        }
    }
}
