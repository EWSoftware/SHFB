//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : CSharpGoToDefinitionMouseProcessor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that provides the mouse processor handling specific to C# code
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

using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Shell;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class provides the mouse processor handling specific to C# code
    /// </summary>
    internal sealed class CSharpGoToDefinitionMouseProcessor : GoToDefinitionMouseProcessor
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
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
            string attrName = null;

            foreach(var classification in spans)
            {
                var name = classification.ClassificationType.Classification.ToLowerInvariant();

                // Not supporting VB for now due to problems looking up identifiers
                //if(name.StartsWith("vb ", StringComparison.Ordinal))
                //    name = name.Substring(3);

                // Highlight the span if it matches what we are looking for and it contains the mouse span
                switch(name)
                {
                    case "xml doc tag":
                        // Track the last seen XML "cref".  Note that the classifier doesn't break up the XML
                        // comments into elements and attributes so we may get a mix of text in the "tag".
                        attrName = classification.Span.GetText();

                        // If it contains "cref", tne next XML doc attribute value will be the target
                        if(attrName.IndexOf("cref=") != -1)
                            attrName = "cref";

                        // As above, for conceptualLink, the next XML doc attribute will be the target
                        if(attrName.StartsWith("<conceptualLink", StringComparison.Ordinal))
                            attrName = "conceptualLink";

                        // For token, the next XML doc comment will contain the token name
                        if(attrName == "<token>")
                            attrName = "token";
                        break;

                    case "xml doc attribute":
                        if((attrName == "cref" || attrName == "conceptualLink") &&
                          classification.Span.Contains(mousePoint) && classification.Span.Length > 2)
                        {
                            // Drop the quotes from the span
                            var span = new SnapshotSpan(classification.Span.Snapshot, classification.Span.Start + 1,
                                classification.Span.Length - 2);

                            if(this.SetHighlightSpan(span, (attrName == "cref") ? "codeEntityReference" : "link"))
                                return true;
                        }
                        break;

                    case "xml doc comment":
                        if(attrName == "token" && classification.Span.Contains(mousePoint) &&
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
                            uiShell.ShowMessageBox(0, ref clsid, "Unable to navigate to XML comments member " +
                                "definition.", String.Format(CultureInfo.CurrentCulture, "Member ID: {0}\r\n\r\n" +
                                "If valid, the most likely cause is that it is not a member of a C# project " +
                                "within the current solution.  Navigating to members in non-C# projects and " +
                                ".NET Framework or reference assemblies is not supported.", id), String.Empty, 0,
                                OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                OLEMSGICON.OLEMSGICON_INFO, 0, out result);

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
                        int result;
                        var uiShell = this.ServiceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;

                        if(uiShell != null)
                            uiShell.ShowMessageBox(0, ref clsid, "Unable to open file for XML comments element " +
                                "target.", String.Format(CultureInfo.CurrentCulture, "Type: {0}\r\nID: {1}\r\n\r\n" +
                                "If valid, it may not be a part of a help file builder project within this " +
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
