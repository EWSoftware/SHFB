//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : GacEnumerator.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/19/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to get a list of assemblies in the GAC
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/04/2006  EFW  Created the code
// 07/20/2008  EFW  Removed all unused code and renamed the class
//===============================================================================================================

// Ignore Spelling: Gac

using System;
using System.Collections.ObjectModel;
using System.Text;

namespace SandcastleBuilder.Gui.Gac
{
    /// <summary>
    /// This class is used to get a list of assemblies in the Global Assembly Cache (GAC).
    /// </summary>
    public static class GacEnumerator
    {
        #region Private data members
        //=====================================================================

        // This will contain the GAC assembly information.  It can be a little time-consuming to enumerate the
        // GAC so we'll keep the list around after the first enumeration.
        private static Collection<string> gacList;

        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This static property is used to obtain a list containing the fully qualified names of all assemblies
        /// in the GAC.
        /// </summary>
        public static Collection<string> GacList
        {
            get
            {
                StringBuilder sb;
                uint chars;

                if(gacList == null)
                {
                    gacList = [];

                    try
                    {
                        NativeMethods.CreateAssemblyEnum(out IAssemblyEnum ae, IntPtr.Zero, null,
                            ASM_CACHE_FLAGS.ASM_CACHE_GAC, IntPtr.Zero);

                        while(ae.GetNextAssembly(IntPtr.Zero, out IAssemblyName an, 0) == 0)
                        {
                            chars = 0;
                            an.GetDisplayName(null, ref chars, ASM_DISPLAY_FLAGS.ALL);

                            if(chars > 0)
                            {
                                sb = new StringBuilder((int)chars);
                                an.GetDisplayName(sb, ref chars, ASM_DISPLAY_FLAGS.ALL);

                                if(sb.Length > 0)
                                    gacList.Add(sb.ToString());
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        throw new InvalidOperationException("Unable to enumerate the GAC", ex);
                    }
                }

                return gacList;
            }
        }
        #endregion
    }
}
