//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : XmlGoToDefinitionMouseProcessor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/02/2018
// Note    : Copyright 2014-2018, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that provides the mouse processor handling specific to MAML elements
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

// Ignore Spelling: xlink

using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class provides the mouse processor handling specific to MAML elements
    /// </summary>
    /// <remarks>For code entity references, this only supports C#.  See <see cref="CodeEntitySearcher"/> for the
    /// reasons why.</remarks>
    internal sealed class XmlGoToDefinitionMouseProcessor : GoToDefinitionMouseProcessor
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public XmlGoToDefinitionMouseProcessor(IWpfTextView textView, SVsServiceProvider serviceProvider,
          IClassifier aggregator, ITextStructureNavigator navigator, CtrlKeyState state) :
            base(textView, serviceProvider, aggregator, navigator, state)
        {
        }
        #endregion

        #region Abstract method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool ProcessSpans(SnapshotSpan mousePoint, IList<ClassificationSpan> spans)
        {
            string elementName = null, attrName = null, spanText;

            foreach(var classification in spans)
            {
                var name = classification.ClassificationType.Classification.ToLowerInvariant();

                // Highlight the span if it matches what we are looking for and it contains the mouse span
                switch(name)
                {
                    case "xml name":
                        elementName = classification.Span.GetText();
                        break;

                    case "xml attribute":
                        attrName = classification.Span.GetText();
                        break;

                    case "xml attribute value":
                        if(classification.Span.Contains(mousePoint))
                        {
                            if((elementName == "image" || elementName == "link") && attrName == "xlink:href")
                            {
                                if(this.SetHighlightSpan(classification.Span, elementName))
                                    return true;
                            }

                            return false;
                        }
                        break;

                    case "xml text":
                        if(classification.Span.Contains(mousePoint))
                        {
                            spanText = classification.Span.GetText().Trim();

                            switch(elementName)
                            {
                                case "codeEntityReference":
                                    if(spanText.IsCodeEntityReference() && this.SetHighlightSpan(classification.Span, elementName))
                                        return true;
                                    break;

                                case "codeReference":
                                    if(spanText.IsCodeReferenceId() && this.SetHighlightSpan(classification.Span, elementName))
                                        return true;
                                    break;

                                case "token":
                                    if(this.SetHighlightSpan(classification.Span, elementName))
                                        return true;
                                    break;

                                default:
                                    // We only get info about the current line so we may just get some XML text
                                    // if the starting tag is on a prior line.  In such cases, see if the text
                                    // looks like an entity reference or a code reference ID.  If so, offer it as
                                    // a clickable link.  If not, ignore it.
                                    if(String.IsNullOrWhiteSpace(elementName))
                                    {
                                        // Ignore any leading whitespace on the span so that it only underlines
                                        // the text.
                                        string highlightText = classification.Span.GetText();

                                        int offset = highlightText.Length - highlightText.TrimStart().Length;
                                        var textSpan = new SnapshotSpan(classification.Span.Snapshot,
                                            classification.Span.Start.Position + offset,
                                            classification.Span.Length - offset);

                                        if(spanText.IsCodeEntityReference())
                                        {
                                            if(this.SetHighlightSpan(textSpan, "codeEntityReference"))
                                                return true;
                                        }
                                        else
                                            if(spanText.IsCodeReferenceId())
                                            {
                                                if(this.SetHighlightSpan(textSpan, "codeReference"))
                                                    return true;
                                            }
                                    }
                                    break;
                            }

                            return false;
                        }
                        break;

                    default:
                        break;
                }
            }

            return false;
        }

        /// <inheritdoc />
        protected override void GoToDefinition(string id, string definitionType)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            switch(definitionType)
            {
                case "codeEntityReference":
                    var entitySearcher = new CodeEntitySearcher(this.ServiceProvider);

                    if(!entitySearcher.GotoDefinitionFor(id))
                    {
                        Guid clsid = Guid.Empty;
                        int result;
                        var uiShell = this.ServiceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;

                        if(uiShell != null)
                            uiShell.ShowMessageBox(0, ref clsid, "Unable to navigate to code entity reference " +
                                "definition.", String.Format(CultureInfo.CurrentCulture, "Member ID: {0}\r\n\r\n" +
                                "If valid, the most likely cause is that it is not a member of a C# project " +
                                "within the current solution.  Navigating to members in non-C# projects and " +
                                ".NET Framework or reference assemblies is not supported.", id), String.Empty, 0,
                                OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                OLEMSGICON.OLEMSGICON_INFO, 0, out result);

                        System.Diagnostics.Debug.WriteLine("Unable to go to declaration for member ID: " + id);
                    }
                    break;

                case "codeReference":
                case "image":
                case "link":
                case "token":
                    var projectFileSearcher = new ProjectFileSearcher(this.ServiceProvider, this.TextView);

                    ProjectFileSearcher.IdType idType;

                    if(!Enum.TryParse(definitionType, true, out idType))
                        idType = ProjectFileSearcher.IdType.Unknown;

                    if(!projectFileSearcher.OpenFileFor(idType, id))
                    {
                        Guid clsid = Guid.Empty;
                        int result;
                        var uiShell = this.ServiceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;

                        if(uiShell != null)
                            uiShell.ShowMessageBox(0, ref clsid, "Unable to open file for element target.",
                                String.Format(CultureInfo.CurrentCulture, "Type: {0}\r\nID: {1}\r\n\r\nIf " +
                                "valid, it may not be a part of a help file builder project within this " +
                                "solution.", definitionType, id), String.Empty, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK,
                                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_INFO, 0, out result);

                        System.Diagnostics.Debug.WriteLine("Unable to go to open file for ID '{0}' ({1}): ", id,
                            definitionType);
                    }
                    break;

                default:
                    break;
            }
        }
        #endregion
    }
}
