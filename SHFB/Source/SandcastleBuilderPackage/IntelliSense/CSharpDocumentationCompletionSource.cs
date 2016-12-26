//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : CSharpDocumentationCompletionSource.cs
// Author  : Sam Harwell  (sam@tunnelvisionlabs.com)
// Updated : 03/24/2014
// Note    : Copyright 2014, Sam Harwell, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an augmented completion source that extends the default XML comments elements with the
// extended elements from Sandcastle.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/21/2014  EFW  Added the code to the help file builder package  Added a few extra completions for common
//                  element/attribute combinations.  Made the additions context sensitive so that they only
//                  appear when appropriate.
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace SandcastleBuilder.Package.IntelliSense
{
    /// <summary>
    /// This completion source augments the completion set returned by the C# language service within XML
    /// documentation comments to include custom tags which are implemented within the latest release of
    /// Sandcastle.
    /// </summary>
    internal sealed class CSharpDocumentationCompletionSource : ICompletionSource
    {
        // The text buffer associated with this completion source.
        private readonly ITextBuffer _textBuffer;

        // The completion source provider which created this instance.
        private readonly CSharpDocumentationCompletionSourceProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpDocumentationCompletionSource"/> class for the
        /// specified text buffer and provider.
        /// </summary>
        /// <param name="textBuffer">The text buffer associated with this completion source.</param>
        /// <param name="provider">The completion source provider.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="textBuffer"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="provider"/> is <see langword="null"/>.</para>
        /// </exception>
        public CSharpDocumentationCompletionSource(ITextBuffer textBuffer,
          CSharpDocumentationCompletionSourceProvider provider)
        {
            if(textBuffer == null)
                throw new ArgumentNullException("textBuffer");

            if(provider == null)
                throw new ArgumentNullException("provider");

            _textBuffer = textBuffer;
            _provider = provider;
        }

        /// <inheritdoc />
        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            // The IntelliSense calculation within comments is rather simple. To detect when the completion is
            // currently located within an XML documentation comment, the following "tricks" are used.
            // 
            // 1. The filtering algorithm in use never removes "!--" (indicating an XML comment) from the
            //    completion set, so it is always listed when completion is invoked.  In addition, the sorting
            //    algorithm always places this item first in the completion set.
            // 2. Since this particular tag is not valid elsewhere in C#, it is easily used to determine when the
            //    caret is inside an XML documentation comment.
            if(completionSets.Count == 0 || completionSets[0].Completions.Count == 0 ||
              completionSets[0].Completions[0].DisplayText != "!--")
            {
                // Not inside a documentation comment, so leave things alone
                return;
            }

            // Next, we need to determine if the user pressed "<" or simply pressed Ctrl+Space to invoke code
            // completion.  Since the C# IntelliSense provider doesn't include any existing "<" character in the
            // reported ApplicableTo tracking span, we must insert this character when it's not already present.
            // XML itself makes this easy - the "<" character will either appear immediately before the
            // ApplicableTo span or it will not be present.
            ITextSnapshot snapshot = _textBuffer.CurrentSnapshot;
            SnapshotPoint startPoint = completionSets[0].ApplicableTo.GetStartPoint(snapshot);

            string prefix = String.Empty;

            if(startPoint > 0 && snapshot.GetText(startPoint.Position - 1, 1) != "<")
                prefix = "<";

            // Use the GlyphKeyword glyph for "normal" XML tags, to match the glyphs used by C#
            // for the standard IntelliSense tags. Use the GlyphGroupMacro glyph for other completion
            // items that expand to something other that what the user wrote (e.g. "true" expands to
            // <see langword="true"/> as opposed to <true/>).
            //
            // The descriptions for custom tags is copied from the Sandcastle XML Comments Guide.  Obsolete
            // custom tags are not included.
            var iconSource = _provider.GlyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword,
                StandardGlyphItem.GlyphItemPublic);
            var macroIconSource = _provider.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMacro,
                StandardGlyphItem.GlyphItemPublic);

            // The elements are context sensitive so only show them if applicable based on the elements
            // determined to be valid by the C# IntelliSense provider.
            var allTags = completionSets.SelectMany(c => c.CompletionBuilders).Select(b => b.DisplayText).Concat(
                completionSets.SelectMany(c => c.Completions).Select(c => c.DisplayText)).ToList();

            // Elements allowed anywhere
            var completions = new List<Completion>
            {
                new CustomCompletion(session, "conceptualLink", prefix + "conceptualLink target=\"\xFF\"/>",
                    "This element is used to create a link to a MAML topic within the See Also section of a " +
                    "topic or an inline link to a MAML topic within one of the other XML comments elements.",
                    iconSource, ""),
                new SandcastleCompletion("inheritdoc", prefix + "inheritdoc/>", "This element can help minimize the " +
                    "effort required to document complex APIs by allowing common documentation to be " +
                    "inherited from base types/members.", iconSource, ""),
                new CustomCompletion(session, "inheritdocCref", prefix + "inheritdoc cref=\"\xFF\"/>",
                    "Inherit documentation from a specific member.", iconSource, ""),
                new CustomCompletion(session, "inheritdocCrefSelect", prefix + "inheritdoc cref=\"\xFF\" " +
                    "select=\"summary|remarks\"/>", "Inherit documentation from a specific member and comments.",
                    iconSource, ""),
                new SandcastleCompletion("token", prefix + "token", "This element represents a replaceable tag within " +
                    "a topic.", iconSource, "")
            };

            // General top-level elements
            if(allTags.Contains("exception"))
                completions.AddRange(new[]
                {
                    new SandcastleCompletion("AttachedEventComments", prefix + "AttachedEventComments", "This element " +
                        "is used to define the content that should appear on the auto-generated attached " +
                        "event member topic for a given WPF routed event member.", iconSource, ""),
                    new SandcastleCompletion("AttachedPropertyComments", prefix + "AttachedPropertyComments",
                        "This element is used to define the content that should appear on the auto-generated " +
                        "attached property member topic for a given WPF dependency property member.",
                        iconSource, ""),
                    new CustomCompletion(session, "event", prefix + "event cref=\"\xFF\"",
                        "This element is used to list events that can be raised by a type's member.",
                        iconSource, ""),
                    new SandcastleCompletion("overloads", prefix + "overloads", "This element is used to define the " +
                        "content that should appear on the auto-generated overloads topic for a given set of " +
                        "member overloads.", iconSource, ""),
                    new SandcastleCompletion("preliminary", prefix + "preliminary/>",
                        "This element is used to indicate that a particular type or member is preliminary and " +
                        "is subject to change.", iconSource, ""),
                    new SandcastleCompletion("threadsafety", prefix + "threadsafety static=\"true\" instance=\"false\"/>",
                        "This element is used to indicate whether or not a class or structure's static and " +
                        "instance members are safe for use in multi-threaded scenarios.", iconSource, "")
                });

            // General inline elements
            if(allTags.Contains("list"))
            {
                completions.AddRange(new[]
                {
                    new SandcastleCompletion("note", prefix + "note type=\"note\"", "This element is used to create a " +
                        "note-like section within a topic to draw attention to some important information.",
                        iconSource, "")
                });

                // Language-specific keyword extensions.  The C# provider allows these at any level but
                // Sandcastle only uses them if they are inline.
                completions.AddRange(new[]
                {
                    new SandcastleCompletion("null", prefix + "see langword=\"null\"/>", "Inserts the language-specific " +
                        "keyword 'null'.", macroIconSource, ""),
                    new SandcastleCompletion("static", prefix + "see langword=\"static\"/>", "Inserts the " +
                        "language-specific keyword 'static'.", macroIconSource, ""),
                    new SandcastleCompletion("virtual", prefix + "see langword=\"virtual\"/>", "Inserts the " +
                        "language-specific keyword 'virtual'.", macroIconSource, ""),
                    new SandcastleCompletion("true", prefix + "see langword=\"true\"/>", "Inserts the language-specific " +
                        "keyword 'true'.", macroIconSource, ""),
                    new SandcastleCompletion("false", prefix + "see langword=\"false\"/>", "Inserts the " +
                        "language-specific keyword 'false'.", macroIconSource, ""),
                    new SandcastleCompletion("abstract", prefix + "see langword=\"abstract\"/>", "Inserts the " +
                        "language-specific keyword 'abstract'.", macroIconSource, ""),
                    new SandcastleCompletion("sealed", prefix + "see langword=\"sealed\"/>", "Inserts the " +
                        "language-specific keyword 'sealed'.", macroIconSource, ""),
                    new SandcastleCompletion("async", prefix + "see langword=\"async\"/>", "Inserts the " +
                        "language-specific keyword 'async'.", macroIconSource, ""),
                    new SandcastleCompletion("await", prefix + "see langword=\"await\"/>", "Inserts the " +
                        "language-specific keyword 'await'.", macroIconSource, ""),
                    new SandcastleCompletion("asyncAwait", prefix + "see langword=\"async/await\"/>", "Inserts the " +
                        "language-specific keyword 'async/await'.", macroIconSource, "")
                });
            }

            // Code element extensions
            if(allTags.Contains("code"))
                completions.AddRange(new[]
                {
                    new CustomCompletion(session, "codeImport", prefix + "code language=\"\xFF\" title=\" \" " +
                        "source=\"..\\Path\\SourceFile.cs\" region=\"Region Name\"/>", "This element is used " +
                        "to indicate that a multi-line section of text should be imported from the named " +
                        "region of the named file and formatted as a code block.", iconSource, ""),
                    new CustomCompletion(session, "codeLanguage", prefix + "code language=\"\xFF\" " +
                        "title=\" \"></code>", "This element is used to indicate that a multi-line section of " +
                        "text should be formatted as a code block.", iconSource, ""),
                });

            // The augmented completion set is created from the previously existing one (created by the C#
            // language service), and a CompletionSet created for the custom XML tags. The moniker and display
            // name for the additional set are not used, since the augmented set always returns the values
            // reported by the original completion set.
            CompletionSet additionalCompletionSet = new CompletionSet("", "", completionSets[0].ApplicableTo,
                completions, Enumerable.Empty<Completion>());

            completionSets[0] = new AugmentedCompletionSet(session, completionSets[0], additionalCompletionSet);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Nothing to do for this sealed class
        }
    }
}
