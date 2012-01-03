//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : SharedResources.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/27/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to load shared resources at runtime
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/27/2011  EFW  Created the code
//=============================================================================

using System;
using System.Windows;

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
        #endregion
    }
}
