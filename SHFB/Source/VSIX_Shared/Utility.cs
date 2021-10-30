//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : Utility.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains a utility class with extension and utility methods.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/02/2011  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: Za

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using MsVsShellPackage = Microsoft.VisualStudio.Shell.Package;

using SandcastleBuilder.Package.Properties;
using SandcastleBuildItemMetadata = SandcastleBuilder.Utils.BuildItemMetadata;

namespace SandcastleBuilder.Package
{
    /// <summary>
    /// This class contains utility and extension methods
    /// </summary>
    public static class Utility
    {
        #region Private data members
        //=====================================================================

        // This is used to insert spaces for the Image project element alternate text
        private static readonly Regex reInsertSpaces = new Regex(@"((?<=[a-z0-9])[A-Z](?=[a-z0-9]))|((?<=[A-Za-z])\d+)");

        // Namespace and namespace group IDs are converted to NamespaceDoc and NamespaceGroupDoc type searches.
        // Overloads (O) are treated like member searches.
        private static readonly char[] cerTypes = new[] { 'N', 'G', 'T', 'F', 'P', 'E', 'M', 'O' };

        private static readonly Dictionary<string, string> cerOperatorToCodeOperator = new Dictionary<string, string>
        {
            { "op_Addition", "operator +" },
            { "op_BitwiseAnd", "operator &" },
            { "op_BitwiseOr", "operator |" },
            { "op_Decrement", "operator --" },
            { "op_Division", "operator /" },
            { "op_Equality", "operator ==" },
            { "op_ExclusiveOr", "operator ^" },
            { "op_Explicit", "operator explicit" },     // Reversed for search
            { "op_False", "operator false" },
            { "op_GreaterThan", "operator >" },
            { "op_GreaterThanOrEqual", "operator >=" },
            { "op_Implicit", "operator implicit" },     // Reversed for search
            { "op_Increment", "operator ++" },
            { "op_Inequality", "operator !=" },
            { "op_LeftShift", "operator <<" },
            { "op_LessThan", "operator <" },
            { "op_LessThanOrEqual", "operator <=" },
            { "op_LogicalNot", "operator !" },
            { "op_Modulus", "operator %" },
            { "op_Multiply", "operator *" },            // '*' is wildcard.  Search takes steps to compensate.
            { "op_OnesComplement", "operator ~" },
            { "op_RightShift", "operator >>" },
            { "op_Subtraction", "operator -" },
            { "op_True", "operator true" },
            { "op_UnaryNegation", "operator -" },
            { "op_UnaryPlus", "operator +" },
        };
        #endregion

        #region Extension methods
        //=====================================================================

        /// <summary>
        /// This is used to set the Image project element metadata
        /// </summary>
        /// <param name="element">The project element to update</param>
        public static void SetImageMetadata(this ProjectElement element)
        {
            string baseName = Path.GetFileNameWithoutExtension(element.GetMetadata(ProjectFileConstants.Include));

            if(String.IsNullOrEmpty(element.GetMetadata(SandcastleBuildItemMetadata.ImageId)))
                element.SetMetadata(SandcastleBuildItemMetadata.ImageId, baseName);

            if(String.IsNullOrEmpty(element.GetMetadata(SandcastleBuildItemMetadata.AlternateText)))
            {
                baseName = baseName.Replace("_", " ");
                element.SetMetadata(SandcastleBuildItemMetadata.AlternateText,
                    reInsertSpaces.Replace(baseName, " $&").Trim());
            }
        }

        /// <summary>
        /// This is used to determine if the given string looks like a code entity reference
        /// </summary>
        /// <param name="cer">The string to check</param>
        /// <returns>True if it looks like a code entity reference, false if not</returns>
        public static bool IsCodeEntityReference(this string cer)
        {
            if(!String.IsNullOrWhiteSpace(cer))
                return (cer.Length > 3 && Array.IndexOf(cerTypes, cer[0]) != -1 && cer[1] == ':');

            return false;
        }

        /// <summary>
        /// This is used to determine if the given string looks like a code reference ID
        /// </summary>
        /// <param name="id">The string to check</param>
        /// <returns>True if it looks like a code reference ID, false if not</returns>
        public static bool IsCodeReferenceId(this string id)
        {
            if(!String.IsNullOrWhiteSpace(id))
            {
                int pos = id.IndexOf('#');

                if(pos > 0 && pos < id.Length - 1)
                    return !Char.IsWhiteSpace(id[pos - 1]) && !Char.IsWhiteSpace(id[pos + 1]);
            }

            return false;
        }

        /// <summary>
        /// Convert a code entity reference operator name to its equivalent code definition
        /// </summary>
        /// <param name="internalOperatorName">The code entity reference operator name to convert</param>
        /// <returns>The converted operator as it appears in code or null if not recognized</returns>
        public static string ToCodeOperator(this string internalOperatorName)
        {
            if(!cerOperatorToCodeOperator.TryGetValue(internalOperatorName, out string name))
                name = null;

            return name;
        }
        #endregion

        #region General utility methods
        //=====================================================================

        /// <summary>
        /// Get a service from the Sandcastle Help File Builder package
        /// </summary>
        /// <param name="throwOnError">True to throw an exception if the service cannot be obtained,
        /// false to return null.</param>
        /// <typeparam name="TInterface">The interface to obtain</typeparam>
        /// <typeparam name="TService">The service used to get the interface</typeparam>
        /// <returns>The service or null if it could not be obtained</returns>
        public static TInterface GetServiceFromPackage<TInterface, TService>(bool throwOnError)
            where TInterface : class
            where TService : class
        {
            TInterface service = MsVsShellPackage.GetGlobalService(typeof(TService)) as TInterface;

            if(service == null && throwOnError)
                throw new InvalidOperationException("Unable to obtain service of type " + typeof(TService).Name);

            return service;
        }

        /// <summary>
        /// This displays a formatted message using the <see cref="IVsUIShell"/> service
        /// </summary>
        /// <param name="icon">The icon to show in the message box</param>
        /// <param name="message">The message format string</param>
        /// <param name="parameters">An optional list of parameters for the message format string</param>
        public static void ShowMessageBox(OLEMSGICON icon, string message, params object[] parameters)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Guid clsid = Guid.Empty;

            if(message == null)
                throw new ArgumentNullException(nameof(message));

            if(MsVsShellPackage.GetGlobalService(typeof(SVsUIShell)) is IVsUIShell uiShell)
            {
                ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(0, ref clsid,
                    Resources.PackageTitle, String.Format(CultureInfo.CurrentCulture, message, parameters),
                    String.Empty, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, icon, 0,
                    out _));
            }
        }

        /// <summary>
        /// Open a URL within Visual Studio using the <see cref="IVsWebBrowsingService"/> service
        /// </summary>
        /// <param name="url">The URL to display</param>
        public static void OpenUrl(string url)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            bool useExternalBrowser = false;

            if(String.IsNullOrEmpty(url))
                return;

            var options = SandcastleBuilderPackage.Instance.GeneralOptions;

            if(options != null)
                useExternalBrowser = options.UseExternalWebBrowser;

            if(!useExternalBrowser && MsVsShellPackage.GetGlobalService(typeof(SVsWebBrowsingService)) is IVsWebBrowsingService webBrowsingService)
            {
                ErrorHandler.ThrowOnFailure(webBrowsingService.Navigate(url, 0, out IVsWindowFrame frame));

                if(frame != null)
                    frame.Show();
            }
            else
                System.Diagnostics.Process.Start(url);
        }
        #endregion
    }
}
