//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : GoToDefinitionCommandTarget.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2019
// Note    : Copyright 2015-2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to enable the Go To Definition context menu command in XML comments and MAML
// files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/07/2015  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: xlink

using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class is used to enable the Go To Definition context menu command in XML comments and MAML files
    /// </summary>
    /// <remarks>Since there isn't enough common code to warrant a separate base class as there is with the
    /// mouse processors, this command filter handles both XML comments and MAML elements.</remarks>
    internal sealed class GoToDefinitionCommandTarget : IOleCommandTarget
    {
        #region Private data members
        //=====================================================================

        private readonly IWpfTextView textView;
        private readonly GoToDefinitionTextViewCreationListener provider;
        private readonly bool isCodeFile;
        private bool goToDefInvoked;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns the next command target in the chain
        /// </summary>
        internal IOleCommandTarget NextTarget { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="textView">The text view to use</param>
        /// <param name="provider">The service provider to use</param>
        /// <param name="isCodeFile">True if this is a code file, false if not</param>
        public GoToDefinitionCommandTarget(IWpfTextView textView, GoToDefinitionTextViewCreationListener provider,
          bool isCodeFile)
        {
            this.textView = textView;
            this.provider = provider;
            this.isCodeFile = isCodeFile;
        }
        #endregion

        #region IOleCommandTarget implementation
        //=====================================================================

        /// <summary>
        /// Execute the command if applicable
        /// </summary>
        /// <param name="pguidCmdGroup">The unique identifier of the command group</param>
        /// <param name="nCmdID">The command ID to be executed</param>
        /// <param name="nCmdexecopt">Specifies how the object should execute the command</param>
        /// <param name="pvaIn">A structure with optional input arguments</param>
        /// <param name="pvaOut">A structure used to receive output arguments if any</param>
        /// <returns>Returns <c>S_OK</c> on success or a failure code if unsuccessful.</returns>
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if(pguidCmdGroup == typeof(VSConstants.VSStd97CmdID).GUID &&
              (VSConstants.VSStd97CmdID)nCmdID == VSConstants.VSStd97CmdID.GotoDefn && !goToDefInvoked &&
              TryGoToDefinition())
            {
                return VSConstants.S_OK;
            }

            return this.NextTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <summary>
        /// Query the status of the command
        /// </summary>
        /// <param name="pguidCmdGroup">The unique identifier of the command group</param>
        /// <param name="cCmds">The number of commands in the command array</param>
        /// <param name="prgCmds">An array of command structures being queried</param>
        /// <param name="pCmdText">A pointer to a structure used to return command text or status information</param>
        /// <returns>Returns <c>S_OK</c> on success or a failure code if unsuccessful.</returns>
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            int result = this.NextTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

            if(pguidCmdGroup == typeof(VSConstants.VSStd97CmdID).GUID)
                for(uint idx = 0; idx < cCmds; idx++)
                    if((VSConstants.VSStd97CmdID)prgCmds[idx].cmdID == VSConstants.VSStd97CmdID.GotoDefn)
                    {
                        // Always enabled and supported.  We won't try to figure out whether it will work for
                        // the target under the cursor here.
                        prgCmds[idx].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                        result = VSConstants.S_OK;
                        break;
                    }

            return result;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to try and go to the definition under the cursor
        /// </summary>
        /// <returns>True if successful, false if not</returns>
        private bool TryGoToDefinition()
        {
            try
            {
                // In VS 2015, we invoke the actual Go To Definition command for code entities.  This prevents
                // an endless loop as it will try to invoke ours first.
                goToDefInvoked = true;

                var pos = textView.Caret.Position.BufferPosition;
                var buffer = pos.Snapshot.TextBuffer;
                var nav = provider.TextStructureNavigatorSelectorService.GetTextStructureNavigator(buffer);
                var extent = nav.GetExtentOfWord(pos);

                if(extent.IsSignificant)
                {
                    var line = pos.GetContainingLine();
                    var aggregator = provider.ClassifierAggregatorService.GetClassifier(buffer);
                    var identifierSpan = ProcessSpans(pos, aggregator.GetClassificationSpans(
                        new SnapshotSpan(line.Start, line.End)), out string definitionType);

                    if(identifierSpan != null)
                    {
#pragma warning disable VSTHRD010
                        this.GoToDefinition(identifierSpan.Value.GetText().Trim(), definitionType);
#pragma warning restore VSTHRD010
                        return true;
                    }
                }
            }
            finally
            {
                goToDefInvoked = false;
            }

            return false;
        }

        /// <summary>
        /// This is used to process the spans and try to figure out whether or not we have something we can use
        /// for Go To Definition and, i so, the definition type.
        /// </summary>
        /// <param name="cursorPos">The cursor position.</param>
        /// <param name="spans">The spans to process.</param>
        /// <param name="definitionType">On return, this contains the definition type if one was found or null
        /// if not.</param>
        /// <returns>The snapshot span for the definition if one was found, null if not</returns>
        private SnapshotSpan? ProcessSpans(SnapshotPoint cursorPos, IList<ClassificationSpan> spans,
          out string definitionType)
        {
            string elementName = null, attrName = null, identifier = null, spanText;

            definitionType = null;

            foreach(var classification in spans)
            {
                string name = classification.ClassificationType.Classification.ToLowerInvariant();

                // Not supporting VB for now due to problems looking up identifiers
                //if(name.StartsWith("vb ", StringComparison.Ordinal))
                //    name = name.Substring(3);

                if(name.IndexOf("identifier", StringComparison.Ordinal) != -1)
                    name = "identifier";

                // Return the span if it matches what we are looking for and it contains the cursor
                switch(name)
                {
                    case "xml name":
                        elementName = classification.Span.GetText();
                        break;

                    case "xml attribute":
                        attrName = classification.Span.GetText();
                        break;

                    case "xml attribute value":
                        if(classification.Span.Contains(cursorPos))
                        {
                            if((elementName == "image" || elementName == "link") && attrName == "xlink:href")
                            {
                                definitionType = elementName;
                                return classification.Span;
                            }

                            return null;
                        }
                        break;

                    case "xml text":
                        if(classification.Span.Contains(cursorPos))
                        {
                            spanText = classification.Span.GetText().Trim();

                            switch(elementName)
                            {
                                case "codeEntityReference":
                                    if(spanText.IsCodeEntityReference())
                                    {
                                        definitionType = elementName;
                                        return classification.Span;
                                    }
                                    break;

                                case "codeReference":
                                    if(spanText.IsCodeReferenceId())
                                    {
                                        definitionType = elementName;
                                        return classification.Span;
                                    }
                                    break;

                                case "token":
                                    definitionType = elementName;
                                    return classification.Span;

                                default:
                                    // We only get info about the current line so we may just get some XML text
                                    // if the starting tag is on a prior line.  In such cases, see if the text
                                    // looks like an entity reference or a code reference ID.  If so, return it
                                    // as the definition.  If not, ignore it.
                                    if(String.IsNullOrWhiteSpace(elementName))
                                    {
                                        // Ignore any leading whitespace on the span
                                        string highlightText = classification.Span.GetText();

                                        int offset = highlightText.Length - highlightText.TrimStart().Length;
                                        var textSpan = new SnapshotSpan(classification.Span.Snapshot,
                                            classification.Span.Start.Position + offset,
                                            classification.Span.Length - offset);

                                        if(spanText.IsCodeEntityReference())
                                        {
                                            definitionType = "codeEntityReference";
                                            return textSpan;
                                        }
                                        else
                                            if(spanText.IsCodeReferenceId())
                                            {
                                                definitionType = "codeReference";
                                                return textSpan;
                                            }
                                    }
                                    break;
                            }

                            return null;
                        }
                        break;

                    case "xml doc tag":
                        // Track the last seen element or attribute.  The classifier in VS2013 and earlier does
                        // not break up the XML comments into elements and attributes so we may get a mix of text
                        // in the "tag".
                        attrName = classification.Span.GetText();

                        // As above, for conceptualLink, the next XML doc attribute will be the target
                        if(attrName.StartsWith("<conceptualLink", StringComparison.Ordinal))
                            attrName = "conceptualLink";

                        // For token, the next XML doc comment will contain the token name
                        if(attrName == "<token>")
                            attrName = "token";
                        break;

                    case "xml doc attribute":
                        if(attrName == "conceptualLink" && classification.Span.Contains(cursorPos) &&
                          classification.Span.Length > 2)
                        {
                            // Drop the quotes from the span
                            var span = new SnapshotSpan(classification.Span.Snapshot, classification.Span.Start + 1,
                                classification.Span.Length - 2);

                            definitionType = "conceptualLink";
                            return span;
                        }
                        break;

                    case "xml doc comment":
                        if(attrName == "token" && classification.Span.Contains(cursorPos) &&
                          classification.Span.Length > 1)
                        {
                            definitionType = "token";
                            return classification.Span;
                        }
                        break;

                    // VS2015 is more specific in its classifications
                    case "xml doc comment - name":
                        elementName = classification.Span.GetText().Trim();
                        break;

                    case "xml doc comment - attribute name":
                        attrName = identifier = null;
                        break;

                    case "xml doc comment - attribute value":
                        if(elementName == "conceptualLink" && attrName == "target" &&
                          classification.Span.Contains(cursorPos) && classification.Span.Length > 1)
                        {
                            definitionType = "conceptualLink";
                            return classification.Span;
                        }
                        break;

                    case "identifier":
                    case "keyword":
                    case "operator":
                        if(attrName != null)
                        {
                            identifier += classification.Span.GetText();

                            if(name == "keyword")
                                identifier += " ";
                        }
                        break;

                    case "punctuation":
                        if(identifier != null)
                            identifier += classification.Span.GetText();
                        break;

                    case "xml doc comment - attribute quotes":
                        if(identifier != null)
                        {
                            // Set the span to that of the identifier
                            var span = new SnapshotSpan(classification.Span.Snapshot,
                                classification.Span.Start - identifier.Length, identifier.Length);

                            if(span.Contains(cursorPos) && span.Length > 1)
                            {
                                definitionType = "codeEntityReference";
                                return span;
                            }
                        }
                        break;

                    case "xml doc comment - text":
                        if(elementName == "token" && classification.Span.Contains(cursorPos) &&
                          classification.Span.Length > 1)
                        {
                            definitionType = "token";
                            return classification.Span;
                        }
                        break;

                    default:
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// This is used to go to the definition based on the span under the cursor
        /// </summary>
        /// <param name="id">The ID of the definition to go to</param>
        /// <param name="definitionType">A definition type to further classify the ID</param>
        private void GoToDefinition(string id, string definitionType)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            switch(definitionType)
            {
                case "codeEntityReference":
                    var entitySearcher = new CodeEntitySearcher(provider.ServiceProvider);

                    if(!entitySearcher.GotoDefinitionFor(id))
                    {
                        Guid clsid = Guid.Empty;

                        if(provider.ServiceProvider.GetService(typeof(SVsUIShell)) is IVsUIShell uiShell)
                            uiShell.ShowMessageBox(0, ref clsid, "Unable to navigate to XML comments member " +
                                "definition.", String.Format(CultureInfo.CurrentCulture, "Member ID: {0}\r\n\r\n" +
                                "If valid, the most likely cause is that it is not a member of a C# project " +
                                "within the current solution.  Navigating to members in non-C# projects and " +
                                ".NET Framework or reference assemblies is not supported.", id), String.Empty, 0,
                                OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                OLEMSGICON.OLEMSGICON_INFO, 0, out int result);

                        System.Diagnostics.Debug.WriteLine("Unable to go to declaration for member ID: " + id);
                    }
                    break;

                case "codeReference":
                case "conceptualLink":
                case "image":
                case "link":
                case "token":
                    var projectFileSearcher = new ProjectFileSearcher(provider.ServiceProvider, textView);

                    ProjectFileSearcher.IdType idType;

                    if(definitionType == "conceptualLink")
                        idType = ProjectFileSearcher.IdType.Link;
                    else
                        if(!Enum.TryParse(definitionType, true, out idType))
                            idType = ProjectFileSearcher.IdType.Unknown;

                    if(!projectFileSearcher.OpenFileFor(idType, id))
                    {
                        Guid clsid = Guid.Empty;

                        if(provider.ServiceProvider.GetService(typeof(SVsUIShell)) is IVsUIShell uiShell)
                            uiShell.ShowMessageBox(0, ref clsid, "Unable to open file for element target.",
                                String.Format(CultureInfo.CurrentCulture, "Type: {0}\r\nID: {1}\r\n\r\nIf " +
                                "valid, it may not be a part of a help file builder project within this " +
                                "solution.", definitionType, id), String.Empty, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK,
                                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_INFO, 0, out int result);

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
