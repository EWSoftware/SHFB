//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : SyntaxFilterTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/09/2011
// Note    : Copyright 2009-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter that ensures the syntax filter values
// are correct.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.3  11/10/2009  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This type converter ensures that syntax filter values are correct.
    /// </summary>
    public sealed class SyntaxFilterTypeConverter : TypeConverter
    {
        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to convert the given set of comma-separated syntax
        /// filter IDs to a set of recognized filter IDs.
        /// </summary>
        /// <param name="filterIds"></param>
        /// <returns>The validated and recognized set of syntax filter IDs.
        /// If possible, the value is condensed to one of a set of combination
        /// values such as None, All, AllButUsage, or Standard.</returns>
        public static string ToRecognizedFilterIds(string filterIds)
        {
            var allFilters = BuildComponentManager.SyntaxFiltersFrom("All");
            var definedFilters = BuildComponentManager.SyntaxFiltersFrom(filterIds);

            // Convert to None, All, AllButUsage, or Standard?  If not, then convert to the list of defined
            // filters that we know about.
            if(definedFilters.Count == 0)
                filterIds = "None";
            else
                if(definedFilters.Count == allFilters.Count)
                    filterIds = "All";
                else
                    if(definedFilters.Count == allFilters.Count(
                      af => af.Id.IndexOf("usage", StringComparison.OrdinalIgnoreCase) == -1))
                        filterIds = "AllButUsage";
                    else
                        if(definedFilters.Count == 3 && (definedFilters.All(df => df.Id == "CSharp" ||
                          df.Id == "VisualBasic" || df.Id == "CPlusPlus")))
                            filterIds = "Standard";
                        else
                            filterIds = String.Join(", ", definedFilters.Select(f => f.Id).ToArray());

            return filterIds;
        }
        #endregion

        #region Overridden methods
        //=====================================================================

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context,
         Type sourceType)
        {
            if(sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        /// <returns>A comma-separated list of recognized syntax filter IDs</returns>
        public override object ConvertFrom(ITypeDescriptorContext context,
          CultureInfo culture, object value)
        {
            if(value is string)
                return ToRecognizedFilterIds((string)value);

            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context,
          CultureInfo culture, object value, Type destinationType)
        {
            if(destinationType == typeof(string) && value != null)
                return value;

            return base.ConvertTo(context, culture, value, destinationType);
        }
        #endregion
    }
}
