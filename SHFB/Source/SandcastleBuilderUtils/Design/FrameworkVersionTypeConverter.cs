//===============================================================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : FrameworkVersionTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/17/2012
// Note    : Copyright 2006-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter that allows you to select a .NET Framework version to use in the project
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  08/08/2006  EFW  Created the code
// 1.9.2.0  01/16/2011  EFW  Updated to support selection of Silverlight Framework versions
// 1.9.3.2  08/20/2011  EFW  Updated to support selection of .NET Portable Framework versions
// 1.9.5.0  09/10/2012  EFW  Rewrote the class to use the .NET Framework definition file
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

using SandcastleBuilder.Utils.Frameworks;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This type converter allows you to select a .NET Framework version to use in the project
    /// </summary>
    public class FrameworkVersionTypeConverter : StringConverter
    {
        #region Private data members
        //=====================================================================

        private static FrameworkDictionary frameworks;
        private static StandardValuesCollection standardValues = InitializeStandardValues();
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the values in the collection
        /// </summary>
        public static FrameworkDictionary AllFrameworks
        {
            get { return frameworks; }
        }

        /// <summary>
        /// This is used to get the default framework version to use
        /// </summary>
        /// <remarks>The default is the .NET Framework 4.0</remarks>
        public static string DefaultFramework
        {
            get
            {
                FrameworkSettings fs = null;

                // If the framework file didn't exist or wasn't valid, the collection will be null here
                if(frameworks == null)
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        "The framework definitions were not loaded.  Does a valid definition file exist ({0})?",
                        FrameworkDictionary.FrameworkFilePath));

                if(!frameworks.TryGetValue(".NET Framework 4.0", out fs))
                    fs = frameworks.Values.First();

                return fs.Title;
            }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to get the standard values by loading the standard framework definition file used by
        /// the Sandcastle tools.
        /// </summary>
        private static StandardValuesCollection InitializeStandardValues()
        {
            try
            {
                frameworks = FrameworkDictionary.LoadSandcastleFrameworkDictionary();
            }
            catch
            {
                // Not much we can do, so just return a dummy set of values.  The DefaultFramework property will
                // fail and throw a better exception than we get if we let this method fail.
                return new StandardValuesCollection(new[] { ".NET Framework 4.0" });
            }

            return new StandardValuesCollection(frameworks.Keys);
        }

        /// <summary>
        /// This is overridden to return the values for the type converter's dropdown list
        /// </summary>
        /// <param name="context">The format context object</param>
        /// <returns>Returns the standard values for the type</returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return standardValues;
        }

        /// <summary>
        /// This is overridden to indicate that the values are exclusive and values outside the list cannot be
        /// entered.
        /// </summary>
        /// <param name="context">The format context object</param>
        /// <returns>Always returns true</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// This is overridden to indicate that standard values are supported and can be chosen from a list
        /// </summary>
        /// <param name="context">The format context object</param>
        /// <returns>Always returns true</returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// This is used to convert old SHFB project framework version values to the new framework version values
        /// </summary>
        /// <param name="oldValue">The old value to convert</param>
        /// <returns>The equivalent new value</returns>
        internal static string ConvertFromOldValue(string oldValue)
        {
            FrameworkSettings fs = null;

            if(String.IsNullOrWhiteSpace(oldValue))
                return DefaultFramework;

            oldValue = oldValue.Trim();

            if(oldValue.IndexOf(".NET ", StringComparison.OrdinalIgnoreCase) != -1 || Char.IsDigit(oldValue[0]))
            {
                oldValue = oldValue.ToUpperInvariant().Replace(".NET ", String.Empty).Trim();

                if(oldValue.Length == 0)
                    oldValue = "4.0";
                else
                    if(oldValue.Length > 3)
                        oldValue = oldValue.Substring(0, 3);

                oldValue = ".NET Framework " + oldValue;
            }
            else
                if(oldValue.IndexOf("Silverlight ", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    oldValue = oldValue.ToUpperInvariant().Trim();

                    if(oldValue.EndsWith(".0", StringComparison.Ordinal))
                        oldValue = oldValue.Substring(0, oldValue.Length - 2);
                }
                else
                    if(oldValue.IndexOf("Portable ", StringComparison.OrdinalIgnoreCase) != -1)
                        oldValue = ".NET Portable Library 4.0 (Legacy)";

            // If not found, default to .NET 4.0
            if(!frameworks.TryGetValue(oldValue, out fs))
                return DefaultFramework;

            return fs.Title;
        }
        #endregion
    }
}
