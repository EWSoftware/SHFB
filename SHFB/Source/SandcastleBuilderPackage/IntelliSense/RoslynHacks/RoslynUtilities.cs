//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : RoslynUtilities.cs
// Author  : Sam Harwell  (sam@tunnelvisionlabs.com)
// Updated : 06/02/2014
// Note    : Copyright 2014, Sam Harwell, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains utility methods for detecting the presence of Roslyn extensions for Visual Studio.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/02/2014  SH   Created the code
//===============================================================================================================

namespace SandcastleBuilder.Package.IntelliSense.RoslynHacks
{
    using System;
    using Microsoft.VisualStudio.Text;
    using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
    using IVsShell = Microsoft.VisualStudio.Shell.Interop.IVsShell;
    using SVsShell = Microsoft.VisualStudio.Shell.Interop.SVsShell;

    // Taken from Microsoft.RestrictedUsage.CSharp.Utilities in Microsoft.VisualStudio.CSharp.Services.Language.dll
    internal static class RoslynUtilities
    {
        /// <summary>
        /// Caches the result of <see cref="IsRoslynInstalled"/>.
        /// </summary>
        private static bool? roslynInstalled;

        /// <summary>
        /// Determines whether or not the IDE uses a "final" build of the .NET Compiler Platform.
        /// </summary>
        public static bool IsFinalRoslyn
        {
            get
            {
                return typeof(ITextBuffer).Assembly.GetName().Version.Major >= 14;
            }
        }

        /// <summary>
        /// Determines if the Roslyn extensions for Visual Studio are installed.
        /// </summary>
        /// <remarks>
        /// This method caches the result after it is first checked with the IDE.
        /// </remarks>
        /// <param name="serviceProvider">A service provider for accessing global IDE services.</param>
        /// <returns>
        /// <see langword="true"/> if the Roslyn extensions are installed.
        /// <para>-or-</para>
        /// <para><see langword="false"/> if the Roslyn extensions are not installed.</para>
        /// <para>-or-</para>
        /// <para>null if the result of this method has not been cached from a previous call, and <paramref name="serviceProvider"/> is <see langword="null"/> or could not be used to obtain an instance of <see cref="IVsShell"/>.</para>
        /// </returns>
        public static bool? IsRoslynInstalled(IServiceProvider serviceProvider)
        {
            if (roslynInstalled.HasValue)
                return roslynInstalled;

            if (IsFinalRoslyn)
            {
                roslynInstalled = true;
                return true;
            }

            if (serviceProvider == null)
                return null;

            IVsShell vsShell = serviceProvider.GetService(typeof(SVsShell)) as IVsShell;
            if (vsShell == null)
                return null;

            Guid guid = new Guid("6cf2e545-6109-4730-8883-cf43d7aec3e1");
            int isInstalled;
            if (ErrorHandler.Succeeded(vsShell.IsPackageInstalled(ref guid, out isInstalled)))
                roslynInstalled = isInstalled != 0;

            return roslynInstalled;
        }
    }
}
