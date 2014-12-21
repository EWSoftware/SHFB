//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : Utility.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/20/2014
// Note    : Copyright 2011-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a utility class with extension and utility methods.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/02/2011  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

using SandcastleBuilder.Package.Properties;
using SandcastleBuilderProjectElement = SandcastleBuilder.Utils.ProjectElement;

namespace SandcastleBuilder.Package
{
    /// <summary>
    /// This class contains utility and extension methods
    /// </summary>
    public static class Utility
    {
        #region Private data members
        //=====================================================================

        // PlatformUI assembly loaded flag and environment colors type
        private static bool shellAssemblyLoaded;
        private static Type environmentColors;

        // This is used to insert spaces for the Image project element alternate text
        private static Regex reInsertSpaces = new Regex(@"((?<=[a-z0-9])[A-Z](?=[a-z0-9]))|((?<=[A-Za-z])\d+)");

        // Namespace and namespace group IDs are converted to NamespaceDoc and NamespaceGroupDoc type searches.
        // Overloads (O) are treated like member searches.
        private static char[] cerTypes = new[] { 'N', 'G', 'T', 'F', 'P', 'E', 'M', 'O' };

        private static Dictionary<string, string> cerOperatorToCodeOperator = new Dictionary<string, string>
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

            if(String.IsNullOrEmpty(element.GetMetadata(SandcastleBuilderProjectElement.ImageId)))
                element.SetMetadata(SandcastleBuilderProjectElement.ImageId, baseName);

            if(String.IsNullOrEmpty(element.GetMetadata(SandcastleBuilderProjectElement.AlternateText)))
            {
                baseName = baseName.Replace("_", " ");
                element.SetMetadata(SandcastleBuilderProjectElement.AlternateText,
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
            string name;

            if(!cerOperatorToCodeOperator.TryGetValue(internalOperatorName, out name))
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
            IServiceProvider provider = SandcastleBuilderPackage.Instance;

            TInterface service = (provider == null) ? null : provider.GetService(typeof(TService)) as TInterface;

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
            Guid clsid = Guid.Empty;
            int result;

            if(message == null)
                throw new ArgumentNullException("message");

            IVsUIShell uiShell = GetServiceFromPackage<IVsUIShell, SVsUIShell>(true);

            ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(0, ref clsid,
                Resources.PackageTitle, String.Format(CultureInfo.CurrentCulture, message, parameters),
                String.Empty, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, icon, 0,
                out result));
        }

        /// <summary>
        /// Open a URL within Visual Studio using the <see cref="IVsWebBrowsingService"/> service
        /// </summary>
        /// <param name="url">The URL to display</param>
        public static void OpenUrl(string url)
        {
            IVsWindowFrame frame;
            IVsWebBrowsingService webBrowsingService = GetServiceFromPackage<IVsWebBrowsingService,
                SVsWebBrowsingService>(true);
            bool useExternalBrowser = false;

            if(String.IsNullOrEmpty(url))
                return;

            var options = SandcastleBuilderPackage.Instance.GeneralOptions;

            if(options != null)
                useExternalBrowser = options.UseExternalWebBrowser;

            if(!useExternalBrowser && webBrowsingService != null)
            {
                ErrorHandler.ThrowOnFailure(webBrowsingService.Navigate(url, 0, out frame));

                if(frame != null)
                    frame.Show();
            }
            else
                System.Diagnostics.Process.Start(url);
        }

        /// <summary>
        /// This is used to get the current dialog font for use in property pages, etc.
        /// </summary>
        /// <returns>The current dialog font or a Segoe UI 9pt font if it is not available</returns>
        public static Font GetDialogFont()
        {
            IUIHostLocale host = GetServiceFromPackage<IUIHostLocale, IUIHostLocale>(false);

            if(host != null)
            {
                UIDLGLOGFONT[] pLOGFONT = new UIDLGLOGFONT[1];

                if(host.GetDialogFont(pLOGFONT) == 0)
                    return Font.FromLogFont(pLOGFONT[0]);
            }

            return new Font("Segoe UI", 9.0f);
        }

        // TODO: This can go away in VS 2012 and later.  See remarks.
        /// <summary>
        /// This is used get a Visual Studio theme key in a version-agnostic manner.
        /// </summary>
        /// <param name="key">The theme key to get</param>
        /// <param name="alternateValue">An alternate value to use if not found or not supported</param>
        /// <returns>The theme key to use or the alternate value if not found or supported.  It will return null
        /// if this is Visual Studio 2010 which doesn't support the EnvironmentColors type.</returns>
        /// <remarks>When built exclusively under Visual Studio 2012 or later, this can go away in favor of
        /// using <c>EnvironmentColors</c> to access the named theme key.</remarks>
        public static object GetThemeKey(string key, object alternateValue)
        {
            if(!shellAssemblyLoaded)
            {
                try
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msenv.dll");

                    if(File.Exists(path))
                    {
                        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);

                        // Do not attempt to load it on VS2010, just use the alternate value
                        if(fvi.ProductMajorPart < 11)
                            return null;

                        var vsShell = Assembly.Load("Microsoft.VisualStudio.Shell.11.0, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

                        if(vsShell != null)
                            environmentColors = vsShell.GetType("Microsoft.VisualStudio.PlatformUI.EnvironmentColors");
                    }
                }
                catch(Exception ex)
                {
                    // Ignore exceptions, we'll just use the alternate value
                    System.Diagnostics.Debug.WriteLine(ex);
                }

                shellAssemblyLoaded = true;
            }

            if(environmentColors != null)
            {
                var prop = environmentColors.GetProperty(key);
                return prop.GetValue(null, null);
            }

            return alternateValue;
        }
        #endregion
    }
}
