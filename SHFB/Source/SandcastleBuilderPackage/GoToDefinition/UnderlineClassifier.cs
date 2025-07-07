//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : UnderlineClassifier.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us) - Based on code originally written by Noah Richards
// Updated : 05/26/2021
// Note    : Copyright 2014-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to set and track the current underline span for the MAML and XML Comments
// Go To Definition handlers.
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

using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class is used to set and track the current underline span for the MAML and XML Comments Go To
    /// Definition handlers.
    /// </summary>
    internal sealed class UnderlineClassifier : ITagger<ClassificationTag>
    {
        #region Private data members
        //=====================================================================

        internal const string UnderlineClassifierType = "SHFB Underline Classification";

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the current underline span if any
        /// </summary>
        public SnapshotSpan? CurrentUnderlineSpan { get; private set; }

        /// <summary>
        /// This read-only property is used to get an optional span type for the current underline span
        /// </summary>
        /// <value>This can be used to classify the span to perform different actions when clicked</value>
        public string CurrentUnderlineSpanType { get; private set; }

        #endregion

        #region Events
        //=====================================================================

        /// <inheritdoc />
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        /// <summary>
        /// This is used to raise the <see cref="TagsChanged"/> event
        /// </summary>
        /// <param name="span"></param>
        private void SendEvent(SnapshotSpan span)
        {
            this.TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <inheritdoc />
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if(this.CurrentUnderlineSpan != null && spans.Count != 0)
            {
                SnapshotSpan request = new(spans[0].Start, spans[spans.Count - 1].End);
                SnapshotSpan underline = this.CurrentUnderlineSpan.Value.TranslateTo(request.Snapshot, SpanTrackingMode.EdgeInclusive);

                if(underline.IntersectsWith(request))
                    yield return new TagSpan<ClassificationTag>(underline,
                        new ClassificationTag(UnderlineClassifierProvider.UnderlineClassification));
            }
        }

        /// <summary>
        /// This is used to set the span to underline
        /// </summary>
        /// <param name="span">The span to underline</param>
        /// <param name="spanType">An optional span type to classify the span</param>
        public void SetUnderlineSpan(SnapshotSpan? span, string spanType)
        {
            var oldSpan = this.CurrentUnderlineSpan;

            this.CurrentUnderlineSpan = span;
            this.CurrentUnderlineSpanType = spanType;

            if((oldSpan == null && this.CurrentUnderlineSpan == null) || (oldSpan != null &&
              this.CurrentUnderlineSpan != null && oldSpan == this.CurrentUnderlineSpan))
                return;

            if(this.CurrentUnderlineSpan == null)
                this.SendEvent(oldSpan.Value);
            else
            {
                SnapshotSpan updateSpan = this.CurrentUnderlineSpan.Value;

                if(oldSpan != null)
                    updateSpan = new SnapshotSpan(updateSpan.Snapshot,
                        Span.FromBounds(Math.Min(updateSpan.Start, oldSpan.Value.Start),
                            Math.Max(updateSpan.End, oldSpan.Value.End)));

                this.SendEvent(updateSpan);
            }
        }
        #endregion
    }
}
