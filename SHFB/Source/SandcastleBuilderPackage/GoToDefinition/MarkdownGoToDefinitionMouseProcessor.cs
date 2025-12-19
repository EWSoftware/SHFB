//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MarkdownGoToDefinitionMouseProcessor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/16/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the class that provides the mouse processor handling specific to Markdown elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/16/2025  EFW  Created the code
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

namespace SandcastleBuilder.Package.GoToDefinition;

/// <summary>
/// This class provides the mouse processor handling specific to Markdown elements
/// </summary>
/// <remarks>For code entity references, this only supports C#.  See <see cref="CodeEntitySearcher"/> for the
/// reasons why.</remarks>
internal sealed class MarkdownGoToDefinitionMouseProcessor : GoToDefinitionMouseProcessor
{
    #region Constructor
    //=====================================================================

    /// <inheritdoc />
    public MarkdownGoToDefinitionMouseProcessor(IWpfTextView textView, SVsServiceProvider serviceProvider,
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
        // Markdown doesn't classify the inner text of elements nor our link targets so we have to parse the
        // text to see if it's something we can use.
        var line = mousePoint.Snapshot.GetLineFromPosition(mousePoint.Start.Position);
        string lineText = line.GetText(), elementName = null, id;
        int tagIndex, endIndex, startIndex = mousePoint.Start.Position - line.Start.Position, nestedParen = 0;

        if(startIndex >= lineText.Length)
            startIndex = lineText.Length - 1;

        while(startIndex >= 0 && lineText[startIndex] != '@' && lineText[startIndex] != '>')
        {
            if(lineText[startIndex] == '<' || Char.IsWhiteSpace(lineText[startIndex]))
                return false;

            startIndex--;
        }

        endIndex = startIndex + 1;

        while(endIndex < lineText.Length && (nestedParen != 0 || lineText[endIndex] != ')') &&
          lineText[endIndex] != '<')
        {
            if(Char.IsWhiteSpace(lineText[endIndex]))
                return false;

            endIndex++;

            if(endIndex < lineText.Length)
            {
                if(lineText[endIndex] == '(')
                    nestedParen++;
                else
                {
                    if(nestedParen > 0 && lineText[endIndex] == ')')
                    {
                        nestedParen--;
                        endIndex++;
                    }
                }
            }
        }

        if(startIndex < 0)
            startIndex = 0;

        if(lineText[startIndex] == '@')
        {
            if(startIndex < 3 || lineText[startIndex - 1] != '(' || lineText[startIndex - 2] != ']')
                return false;

            startIndex++;

            if(endIndex - startIndex > 2 && lineText[startIndex + 1] == ':')
                elementName = "codeEntityReference";
            else
            {
                tagIndex = startIndex;

                while(tagIndex > 0 && lineText[tagIndex] != '[')
                    tagIndex--;

                if(tagIndex > 0 && lineText[tagIndex - 1] == '!')
                    elementName = "image";
                else
                    elementName = "link";
            }
        }
        else
        {
            if(lineText[startIndex] == '>')
            {
                tagIndex = startIndex;
                startIndex++;

                while(tagIndex > 0 && lineText[tagIndex] != '<')
                    tagIndex--;

                if(tagIndex < 0)
                    return false;

                elementName = lineText.Substring(tagIndex + 1, startIndex - tagIndex - 2);
                tagIndex = elementName.IndexOf(' ');

                if(tagIndex != -1)
                    elementName = elementName.Substring(0, tagIndex);
            }
            else
            {
                if(endIndex < lineText.Length - 2 && lineText[endIndex] == '<' && lineText[endIndex + 1] == '/')
                {
                    tagIndex = endIndex;

                    while(tagIndex < lineText.Length && lineText[tagIndex] != '>')
                        tagIndex++;

                    if(tagIndex >= lineText.Length)
                        return false;

                    elementName = lineText.Substring(endIndex + 2, tagIndex - endIndex - 2);
                }
            }
        }

        id = lineText.Substring(startIndex, endIndex - startIndex);

        if(elementName == null && id.Length > 3 && id[1] == ':')
            elementName = "codeEntityReference";

        if(elementName == null)
            return false;

        return this.SetHighlightSpan(new SnapshotSpan(line.Snapshot,
            new Span(line.Start.Position + startIndex, endIndex - startIndex)), elementName);
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
                    {
                        uiShell.ShowMessageBox(0, ref clsid, "Unable to navigate to code entity reference " +
                            "definition.", String.Format(CultureInfo.CurrentCulture, "Member ID: {0}\r\n\r\n" +
                            "If valid, the most likely cause is that it is not a member of a C# project " +
                            "within the current solution.  Navigating to members in non-C# projects and " +
                            ".NET Framework or reference assemblies is not supported.", id), String.Empty, 0,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                            OLEMSGICON.OLEMSGICON_INFO, 0, out _);
                    }

                    System.Diagnostics.Debug.WriteLine("Unable to go to declaration for member ID: " + id);
                }
                break;

            case "codeReference":
            case "image":
            case "link":
            case "token":
                var projectFileSearcher = new ProjectFileSearcher(this.ServiceProvider);

                ProjectFileSearcher.IdType idType;

                if(!Enum.TryParse(definitionType, true, out idType))
                    idType = ProjectFileSearcher.IdType.Unknown;

                if(!projectFileSearcher.OpenFileFor(idType, id))
                {
                    Guid clsid = Guid.Empty;

                    if(this.ServiceProvider.GetService(typeof(SVsUIShell)) is IVsUIShell uiShell)
                    {
                        uiShell.ShowMessageBox(0, ref clsid, "Unable to open file for element target.",
                            String.Format(CultureInfo.CurrentCulture, "Type: {0}\r\nID: {1}\r\n\r\nIf " +
                            "valid, it may not be a part of a help file builder project within this " +
                            "solution.", definitionType, id), String.Empty, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_INFO, 0, out _);
                    }

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
