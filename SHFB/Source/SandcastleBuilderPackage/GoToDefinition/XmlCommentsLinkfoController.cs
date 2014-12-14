//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : XmlCommentsLinkQuickInfoController.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that acts as the controller for providing quick info tips for XML comments
// elements.
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

using System.Collections.Generic;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class acts as the controller for providing quick info tips for XML comments elements
    /// </summary>
    internal sealed class XmlCommentsLinkQuickInfoController : IIntellisenseController
    {
        #region Private data members
        //=====================================================================

        private ITextView textView;
        private IList<ITextBuffer> textBuffers;
        private XmlCommentsLinkQuickInfoControllerProvider provider;
        private IQuickInfoSession session;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="textView">The text view to use</param>
        /// <param name="textBuffers">The text buffers to use</param>
        /// <param name="provider">The provider to use</param>
        internal XmlCommentsLinkQuickInfoController(ITextView textView, IList<ITextBuffer> textBuffers,
          XmlCommentsLinkQuickInfoControllerProvider provider)
        {
            this.textView = textView;
            this.textBuffers = textBuffers;
            this.provider = provider;

            textView.MouseHover += OnTextViewMouseHover;
        }
        #endregion

        #region IIntellisenseController implementation
        //=====================================================================

        /// <inheritdoc />
        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            textBuffers.Add(subjectBuffer);
        }

        /// <inheritdoc />
        public void Detach(ITextView textView)
        {
            if(this.textView == textView)
            {
                textView.MouseHover -= this.OnTextViewMouseHover;
                textView = null;
            }
        }

        /// <inheritdoc />
        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            textBuffers.Remove(subjectBuffer);
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is handled to trigger the quick info when needed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            SnapshotPoint? point = textView.BufferGraph.MapDownToFirstMatch(
                new SnapshotPoint(textView.TextSnapshot, e.Position), PointTrackingMode.Positive,
                snapshot => textBuffers.Contains(snapshot.TextBuffer), PositionAffinity.Predecessor);

            if(point != null)
            {
                var triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
                    PointTrackingMode.Positive);

                if(provider.QuickInfoBroker.IsQuickInfoActive(textView))
                    session = provider.QuickInfoBroker.TriggerQuickInfo(textView, triggerPoint, true);
            }
        }
        #endregion
    }
}
