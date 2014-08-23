//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : CSharpDocumentationCompletionSourceProvider.cs
// Author  : Sam Harwell  (sam@tunnelvisionlabs.com)
// Updated : 06/20/2014
// Note    : Copyright 2014, Sam Harwell, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a provider for creating the augmented XML comments completion source
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/21/2014  EFW  Added the code to the help file builder package
//===============================================================================================================

using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace SandcastleBuilder.Package.IntelliSense
{
    /// <summary>
    /// This implementation of <see cref="ICompletionSourceProvider"/> is responsible for creating the
    /// <see cref="CSharpDocumentationCompletionSource"/> completion source to augment IntelliSense suggestion
    /// lists within C# XML documentation comments.
    /// </summary>
    /// <remarks>The <c>[Order(After = "default")]</c> metadata ensures that the C# language service has already
    /// computed its IntelliSense suggestions before the <see cref="CSharpDocumentationCompletionSource"/> is
    /// checked.  This allows the documentation completion source to <em>augment</em> the existing
    /// <see cref="CompletionSet"/> rather than provide a new <see cref="CompletionSet"/> which would be
    /// displayed in a separate tab in the completion dropdown UI.</remarks>
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("CSharp")]
    [Order(After = "default")]
    [Name("Sandcastle XML Comments Completion Source Provider")]
    internal sealed class CSharpDocumentationCompletionSourceProvider : ICompletionSourceProvider
    {
        /// <summary>
        /// Gets the <see cref="IGlyphService"/> instance which provides standard icons for
        /// <see cref="Completion"/> instances within the IntelliSense suggestion lists.
        /// </summary>
        [Import]
        internal IGlyphService GlyphService { get; private set; }

        // Caches the result of <see cref="IsRoslynInstalled"/>.
        private static bool? roslynInstalled;

        /// <summary>
        /// Gets the global <see cref="SVsServiceProvider"/> provided by the IDE through MEF,
        /// which provides access to other IDE services.
        /// </summary>
        [Import]
        internal SVsServiceProvider ServiceProvider { get; private set; }

        /// <inheritdoc />
        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            // Disable the custom completion source if Roslyn is installed since it uses an incompatible
            // implementation of IntelliSense.
            if(IsRoslynInstalled() ?? false)
                return null;

            return new CSharpDocumentationCompletionSource(textBuffer, this);
        }

        /// <summary>
        /// Determines if the Roslyn extensions for Visual Studio are installed.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the Roslyn extensions are installed.
        /// <para>-or-</para>
        /// <para><see langword="false"/> if the Roslyn extensions are not installed.</para>
        /// <para>-or-</para>
        /// <para>null if the result of this method has not been cached from a previous call and the
        /// service provider is <see langword="null"/> or could not be used to obtain an instance of
        /// <see cref="IVsShell"/>.</para>
        /// </returns>
        /// <remarks>
        /// This method caches the result after it is first checked with the IDE.  Taken from
        /// Microsoft.RestrictedUsage.CSharp.Utilities in Microsoft.VisualStudio.CSharp.Services.Language.dll.
        /// </remarks>
        private bool? IsRoslynInstalled()
        {
            if(roslynInstalled.HasValue)
                return roslynInstalled;

            if(ServiceProvider == null)
                return null;

            IVsShell vsShell = ServiceProvider.GetService(typeof(SVsShell)) as IVsShell;

            if(vsShell == null)
                return null;

            Guid guid = new Guid("6cf2e545-6109-4730-8883-cf43d7aec3e1");
            int isInstalled;

            if(ErrorHandler.Succeeded(vsShell.IsPackageInstalled(ref guid, out isInstalled)))
                roslynInstalled = (isInstalled != 0);

            return roslynInstalled;
        }
    }
}
