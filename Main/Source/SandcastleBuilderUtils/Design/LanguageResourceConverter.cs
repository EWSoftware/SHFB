//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : LanguageResourceConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/15/2011
// Note    : Copyright 2006-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter that allows you to select a culture
// from a list representing a set of available language resource files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.3.0.0  09/15/2006  EFW  Created the code
// 1.5.0.2  07/12/2007  EFW  Reworked support for language resource files
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This type converter allows you to select a culture from a list
    /// representing a set of available language resource folders.
    /// </summary>
    internal sealed class LanguageResourceConverter : CultureInfoConverter
    {
        #region Private data members
        //=====================================================================

        private bool initialized;   // Initialized flag
        #endregion

        #region Private culture info comparer class
        //=====================================================================

        /// <summary>
        /// This is used to compare two culture info objects by display name
        /// </summary>
        private sealed class CultureInfoComparer : IComparer
        {
            /// <summary>
            /// Compare two items
            /// </summary>
            /// <param name="x">The first item to compare</param>
            /// <param name="y">The second item to compare</param>
            /// <returns>-1 if item 1 is less than item 2, 0 if they are equal,
            /// or 1 if item 1 is greater than item 2.</returns>
            public int Compare(object x, object y)
            {
                if(x == null)
                    return (y == null) ? 0 : -1;

                if(y == null)
                    return 1;

                return CultureInfo.CurrentCulture.CompareInfo.Compare(((CultureInfo)x).DisplayName,
                    ((CultureInfo)y).DisplayName, CompareOptions.StringSort);
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to return the values for the type converter's
        /// dropdown list.
        /// </summary>
        /// <param name="context">The format context object</param>
        /// <returns>Returns the standard values for the type</returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            string[] files;
            int idx = 0;

            if(!initialized)
            {
                string name = Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location) +
                    @"\SharedContent";

                if(Directory.Exists(name))
                    files = Directory.GetFiles(name, "SharedBuilderContent_*.xml");
                else
                    files = new string[0];

                CultureInfo[] ci = new CultureInfo[files.Length];

                // Find the available language resources
                foreach(string s in files)
                {
                    name = Path.GetFileNameWithoutExtension(s);
                    name = name.Substring(name.LastIndexOf('_') + 1);
                    ci[idx++] = new CultureInfo(name);
                }

                Array.Sort(ci, new CultureInfoComparer());

                StandardValuesCollection svc = new StandardValuesCollection(ci);

                // Use reflection to set the base class's values field to
                // our limited array.
                FieldInfo fi = typeof(CultureInfoConverter).GetField(
                    "values", BindingFlags.NonPublic | BindingFlags.Instance);
                fi.SetValue(this, svc);
                initialized = true;
            }

            return base.GetStandardValues(context);
        }
        #endregion
    }
}
