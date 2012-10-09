//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : Utility.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/15/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a utility class with extension and utility methods.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  04/02/2011  EFW  Created the code
//=============================================================================

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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

        // This is used to insert spaces for the Image project element alternate text
        private static Regex reInsertSpaces = new Regex(@"((?<=[a-z0-9])[A-Z](?=[a-z0-9]))|((?<=[A-Za-z])\d+)");
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
        #endregion
    }
}
