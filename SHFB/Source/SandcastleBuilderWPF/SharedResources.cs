//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : SharedResources.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/19/2017
// Note    : Copyright 2011-2017, Eric Woodruff, All rights reserved
//
// This file contains a class used to load shared resources at runtime
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/27/2011  EFW  Created the code
//===============================================================================================================

using System;
using System.Windows;
using System.Windows.Media.Imaging;

using SandcastleBuilder.WPF.Controls;

namespace SandcastleBuilder.WPF
{
    /// <summary>
    /// This class is used to retrieve shared resources so that they can be merged with the user control
    /// resources at runtime.
    /// </summary>
    /// <remarks>When used from a VSPackage, it can't find the shared resources.  Since Visual Studio is the
    /// host application, we cannot use an <c>App.xaml</c> file to load them.  As such, this class is used to
    /// locate and load them at runtime.  This also works for the standalone GUI.</remarks>
    internal static class SharedResources
    {
        #region Private data members
        //=====================================================================

        private static ResourceDictionary splitButtonResources;
        private static BitmapImage cautionIcon, noteIcon, securityIcon, informationIcon;

        #endregion

        #region Shared resources
        //=====================================================================

        /// <summary>
        /// This read-only property returns the <see cref="SplitButton" /> control style resources
        /// </summary>
        internal static ResourceDictionary SplitButtonStyleResources
        {
            get
            {
                if(splitButtonResources == null)
                {
                    Uri sharedResources = new Uri("/SandcastleBuilder.WPF;component/Controls/SplitButtonStyle.xaml",
                        UriKind.Relative);
                    splitButtonResources = (ResourceDictionary)Application.LoadComponent(sharedResources);
                }

                return splitButtonResources;
            }
        }

        /// <summary>
        /// This read-only property returns the Caution icon for the <see cref="Maml.MamlToFlowDocumentConverter" />.
        /// </summary>
        public static BitmapImage CautionIcon
        {
            get
            {
                if(cautionIcon == null)
                {
                    Uri image = new Uri("pack://application:,,,/SandcastleBuilder.WPF;component/Resources/AlertCaution.png");

                    // Cache on load to prevent it locking the image
                    cautionIcon = new BitmapImage();
                    cautionIcon.BeginInit();
                    cautionIcon.CacheOption = BitmapCacheOption.OnLoad;
                    cautionIcon.UriSource = image;
                    cautionIcon.EndInit();
                }

                return cautionIcon;
            }
        }

        /// <summary>
        /// This read-only property returns the Note icon for the <see cref="Maml.MamlToFlowDocumentConverter" />.
        /// </summary>
        internal static BitmapImage NoteIcon
        {
            get
            {
                if(noteIcon == null)
                {
                    Uri image = new Uri("pack://application:,,,/SandcastleBuilder.WPF;component/Resources/AlertNote.png");

                    // Cache on load to prevent it locking the image
                    noteIcon = new BitmapImage();
                    noteIcon.BeginInit();
                    noteIcon.CacheOption = BitmapCacheOption.OnLoad;
                    noteIcon.UriSource = image;
                    noteIcon.EndInit();
                }

                return noteIcon;
            }
        }

        /// <summary>
        /// This read-only property returns the Security icon for the <see cref="Maml.MamlToFlowDocumentConverter" />.
        /// </summary>
        internal static BitmapImage SecurityIcon
        {
            get
            {
                if(securityIcon == null)
                {
                    Uri image = new Uri("pack://application:,,,/SandcastleBuilder.WPF;component/Resources/AlertSecurity.png");

                    // Cache on load to prevent it locking the image
                    securityIcon = new BitmapImage();
                    securityIcon.BeginInit();
                    securityIcon.CacheOption = BitmapCacheOption.OnLoad;
                    securityIcon.UriSource = image;
                    securityIcon.EndInit();
                }

                return securityIcon;
            }
        }

        /// <summary>
        /// This read-only property returns the information icon for the property pages
        /// </summary>
        public static BitmapImage InformationIcon
        {
            get
            {
                if(informationIcon == null)
                {
                    Uri image = new Uri("pack://application:,,,/SandcastleBuilder.WPF;component/Resources/Information.png");

                    // Cache on load to prevent it locking the image
                    informationIcon = new BitmapImage();
                    informationIcon.BeginInit();
                    informationIcon.CacheOption = BitmapCacheOption.OnLoad;
                    informationIcon.UriSource = image;
                    informationIcon.EndInit();
                }

                return informationIcon;
            }
        }
        #endregion
    }
}
