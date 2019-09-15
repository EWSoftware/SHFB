//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : CSharpGoToDefinitionMouseProcessor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/02/2019
// Note    : Copyright 2014-2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that provides the mouse processor handling specific to C# code
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
    /// This class provides the mouse processor handling specific to C# code
    /// </summary>
    internal sealed class CSharpGoToDefinitionMouseProcessor : GoToDefinitionMouseProcessor
    {
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
        public CSharpGoToDefinitionMouseProcessor(IWpfTextView textView, SVsServiceProvider serviceProvider,
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
            string elementName = null, attrName = null, identifier = null;

            foreach(var classification in spans)
            {
                string name = classification.ClassificationType.Classification.ToLowerInvariant();

                // Not supporting VB for now due to problems looking up identifiers
                //if(name.StartsWith("vb ", StringComparison.Ordinal))
                //    name = name.Substring(3);

                if(name.IndexOf("identifier", StringComparison.Ordinal) != -1)
                    name = "identifier";

                // Highlight the span if it matches what we are looking for and it contains the mouse span
                switch(name)
                {
                    case "xml doc comment - name":
                        elementName = classification.Span.GetText().Trim();
                        break;

                    case "xml doc comment - attribute name":
                        attrName = classification.Span.GetText().Trim();
                        identifier = null;

                        if(attrName == "cref")
                            attrName = null;
                        break;

                    case "xml doc comment - attribute value":
                        if(elementName == "conceptualLink" && attrName == "target" &&
                          classification.Span.Contains(mousePoint) && classification.Span.Length > 1)
                        {
                            if(this.SetHighlightSpan(classification.Span, "link"))
                                return true;
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

                            if(span.Contains(mousePoint) && span.Length > 1 &&
                              this.SetHighlightSpan(span, "codeEntityReference"))
                                return true;
                        }
                        break;

                    case "xml doc comment - text":
                        if(elementName == "token" && classification.Span.Contains(mousePoint) &&
                          classification.Span.Length > 1)
                        {
                            if(this.SetHighlightSpan(classification.Span, "token"))
                                return true;
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

                        if(this.ServiceProvider.GetService(typeof(SVsUIShell)) is IVsUIShell uiShell)
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

                case "link":
                case "token":
                    var projectFileSearcher = new ProjectFileSearcher(this.ServiceProvider, this.TextView);

                    ProjectFileSearcher.IdType idType;

                    if(!Enum.TryParse(definitionType, true, out idType))
                        idType = ProjectFileSearcher.IdType.Unknown;

                    if(!projectFileSearcher.OpenFileFor(idType, id))
                    {
                        if(idType == ProjectFileSearcher.IdType.Link)
                            definitionType = "conceptualLink";

                        Guid clsid = Guid.Empty;

                        if(this.ServiceProvider.GetService(typeof(SVsUIShell)) is IVsUIShell uiShell)
                            uiShell.ShowMessageBox(0, ref clsid, "Unable to open file for XML comments element " +
                                "target.", String.Format(CultureInfo.CurrentCulture, "Type: {0}\r\nID: {1}\r\n\r\n" +
                                "If valid, it may not be a part of a help file builder project within this " +
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
