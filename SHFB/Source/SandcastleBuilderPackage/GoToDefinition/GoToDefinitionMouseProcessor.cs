//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : GoToDefinitionMouseProcessor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us) - Based on code originally written by Noah Richards
// Updated : 01/09/2015
// Note    : Copyright 2014-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the abstract base class used to handle Ctrl+clicks on valid elements to go to their
// definition if possible.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/01/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Shell;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This is a abstract base class used to handle Ctrl+clicks on valid elements to go to their definition if
    /// possible (XML comments references, MAML link targets, etc.).  It also handles mouse moves (when Ctrl is
    /// pressed) to highlight references for which Go To Definition will (likely) be valid.
    /// </summary>
    internal abstract class GoToDefinitionMouseProcessor : MouseProcessorBase
    {
        #region Private data members
        //=====================================================================

        private CtrlKeyState ctrlState;
        private IClassifier aggregator;
        private ITextStructureNavigator navigator;
        private Point? mouseDownAnchorPoint;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the text view associated with the processor
        /// </summary>
        protected IWpfTextView TextView { get; private set; }

        /// <summary>
        /// This read-only property returns the service provider associated with the processor
        /// </summary>
        protected SVsServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// This read-only property returns the current underline span if there is one
        /// </summary>
        protected SnapshotSpan? CurrentUnderlineSpan
        {
            get
            {
                var classifier = UnderlineClassifierProvider.GetClassifierForView(this.TextView);

                if(classifier != null && classifier.CurrentUnderlineSpan != null)
                    return classifier.CurrentUnderlineSpan.Value.TranslateTo(this.TextView.TextSnapshot,
                        SpanTrackingMode.EdgeExclusive);

                return null;
            }
        }

        /// <summary>
        /// This read-only property returns the current underline span type if there is one
        /// </summary>
        protected string CurrentUnderlineSpanType
        {
            get
            {
                var classifier = UnderlineClassifierProvider.GetClassifierForView(this.TextView);

                return (classifier != null) ? classifier.CurrentUnderlineSpanType : null;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="textView">The text view to use</param>
        /// <param name="serviceProvider">The service provider to use</param>
        /// <param name="aggregator">The classifier tag aggregator to use</param>
        /// <param name="navigator">The text structure navigator to use</param>
        /// <param name="state">The Ctrl key state tracker to use</param>
        public GoToDefinitionMouseProcessor(IWpfTextView textView, SVsServiceProvider serviceProvider,
          IClassifier aggregator, ITextStructureNavigator navigator, CtrlKeyState state)
        {
            this.TextView = textView;
            this.ServiceProvider = serviceProvider;

            this.ctrlState = state;
            this.aggregator = aggregator;
            this.navigator = navigator;

            this.ctrlState.CtrlKeyStateChanged += (sender, args) =>
            {
                if(this.ctrlState.Enabled)
                    this.TryHighlightItemUnderMouse(this.RelativeToView(Mouse.PrimaryDevice.GetPosition(this.TextView.VisualElement)));
                else
                    this.SetHighlightSpan(null, null);
            };

            // Some other points to clear the highlight span:
            this.TextView.LostAggregateFocus += (sender, args) => this.SetHighlightSpan(null, null);
            this.TextView.VisualElement.MouseLeave += (sender, args) => this.SetHighlightSpan(null, null);
        }
        #endregion

        #region Mouse processor overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>Remember the location of the mouse on left button down so we only handle left button up if
        /// the mouse has stayed in a single location.</remarks>
        public override void PostprocessMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            mouseDownAnchorPoint = this.RelativeToView(e.GetPosition(this.TextView.VisualElement));
        }

        /// <inheritdoc />
        public override void PreprocessMouseUp(MouseButtonEventArgs e)
        {
            if(mouseDownAnchorPoint != null && ctrlState.Enabled)
            {
                var currentMousePosition = this.RelativeToView(e.GetPosition(this.TextView.VisualElement));

                if(!this.InDragOperation(mouseDownAnchorPoint.Value, currentMousePosition))
                {
                    ctrlState.Enabled = false;

                    if(this.CurrentUnderlineSpan != null && this.CurrentUnderlineSpan.Value.Length != 0)
                    {
                        string id = this.CurrentUnderlineSpan.Value.GetText().Trim(),
                            definitionType = this.CurrentUnderlineSpanType;

                        // Clear the highlight and selection.  We do this first as we may not have the same
                        // content if this is a Peek Definition window and it opens a new file.
                        this.SetHighlightSpan(null, null);
                        this.TextView.Selection.Clear();

                        this.GoToDefinition(id, definitionType);

                        e.Handled = true;
                    }
                }
            }

            mouseDownAnchorPoint = null;
        }

        /// <inheritdoc />
        public override void PreprocessMouseMove(MouseEventArgs e)
        {
            if(mouseDownAnchorPoint == null && ctrlState.Enabled && e.LeftButton == MouseButtonState.Released)
            {
                TryHighlightItemUnderMouse(this.RelativeToView(e.GetPosition(this.TextView.VisualElement)));
            }
            else
                if(mouseDownAnchorPoint != null)
                {
                    // Check and see if this is a drag.  If so, clear the highlight.
                    var currentMousePosition = this.RelativeToView(e.GetPosition(this.TextView.VisualElement));

                    if(this.InDragOperation(mouseDownAnchorPoint.Value, currentMousePosition))
                    {
                        mouseDownAnchorPoint = null;
                        this.SetHighlightSpan(null, null);
                    }
                }
        }

        /// <inheritdoc />
        public override void PreprocessMouseLeave(MouseEventArgs e)
        {
            mouseDownAnchorPoint = null;
        }
        #endregion

        #region Abstract methods
        //=====================================================================

        /// <summary>
        /// This is used to go to the definition based on the highlighted span
        /// </summary>
        /// <param name="id">The ID of the definition to go to</param>
        /// <param name="definitionType">A definition type to further classify the ID</param>
        protected abstract void GoToDefinition(string id, string definitionType);

        /// <summary>
        /// Process a list of spans to see if any one of them needs to be highlighted as a possible link target
        /// </summary>
        /// <param name="mousePoint">A snapshot span that represents the current mouse position</param>
        /// <param name="spans">The list of classification spans to check</param>
        /// <returns>True if a span was highlighted, false if not</returns>
        protected abstract bool ProcessSpans(SnapshotSpan mousePoint, IList<ClassificationSpan> spans);

        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to see if a drag operation is in progress
        /// </summary>
        /// <param name="anchorPoint">The mouse anchor point</param>
        /// <param name="currentPoint">The current mouse point</param>
        /// <returns>True if in a drag operation, false if not</returns>
        private bool InDragOperation(Point anchorPoint, Point currentPoint)
        {
            // If the mouse up is more than a drag away from the mouse down, this is a drag
            return Math.Abs(anchorPoint.X - currentPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(anchorPoint.Y - currentPoint.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }

        /// <summary>
        /// This is used to convert a position to its location relative to the view
        /// </summary>
        /// <param name="position">The position to convert</param>
        /// <returns>The position relative to the view</returns>
        private Point RelativeToView(Point position)
        {
            return new Point(position.X + this.TextView.ViewportLeft, position.Y + this.TextView.ViewportTop);
        }

        /// <summary>
        /// Try to highlight the item under the mouse if it is a valid link target
        /// </summary>
        /// <param name="position">The current mouse position</param>
        /// <returns>True if highlighted, false if not</returns>
        private bool TryHighlightItemUnderMouse(Point position)
        {
            bool updated = false;

            try
            {
                var line = this.TextView.TextViewLines.GetTextViewLineContainingYCoordinate(position.Y);

                if(line == null)
                    return false;

                var bufferPosition = line.GetBufferPositionFromXCoordinate(position.X);

                if(bufferPosition == null)
                    return false;

                // If the mouse is still inside the current underline span, we're already set
                var currentSpan = CurrentUnderlineSpan;

                if(currentSpan != null && currentSpan.Value.Contains(bufferPosition.Value))
                {
                    updated = true;
                    return true;
                }

                var extent = navigator.GetExtentOfWord(bufferPosition.Value);

                if(!extent.IsSignificant)
                    return false;

                updated = this.ProcessSpans(new SnapshotSpan(bufferPosition.Value, 1),
                    aggregator.GetClassificationSpans(new SnapshotSpan(line.Start, line.End)));
            }
            finally
            {
                if(!updated)
                    this.SetHighlightSpan(null, null);
            }

            return updated;
        }

        /// <summary>
        /// This is used to set the current highlight span
        /// </summary>
        /// <param name="span">The span to highlight with an underline or null to clear it</param>
        /// <param name="spanType">An optional span type to further classify the span</param>
        /// <returns>True if highlighted, false if not</returns>
        protected bool SetHighlightSpan(SnapshotSpan? span, string spanType)
        {
            var classifier = UnderlineClassifierProvider.GetClassifierForView(this.TextView);

            if(classifier != null)
            {
                Mouse.OverrideCursor = (span != null) ? Cursors.Hand : null;
                classifier.SetUnderlineSpan(span, spanType);
                return true;
            }

            return false;
        }
        #endregion
    }
}
