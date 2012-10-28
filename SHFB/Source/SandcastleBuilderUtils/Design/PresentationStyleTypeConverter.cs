//===============================================================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : PresenationStyleTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/25/2012
// Note    : Copyright 2007-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter that allows you to select a presentation style from those defined in the
// presentation style definition files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  08/08/2006  EFW  Created the code
// 1.5.0.0  06/19/2007  EFW  Updated for use with the June CTP
// 1.9.6.0  10/25/2012  EFW  Rewrote the class to use the presentation style definition files
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.PresentationStyle;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This type converter allows you to select a presentation style from those defined in the presentation
    /// style definition files.
    /// </summary>
    public sealed class PresentationStyleTypeConverter : StringConverter
    {
        #region Private data members
        //=====================================================================

        private static PresentationStyleDictionary styles;
        private static StandardValuesCollection standardValues = InitializeStandardValues();
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the values in the collection
        /// </summary>
        public static PresentationStyleDictionary AllStyles
        {
            get { return styles; }
        }

        /// <summary>
        /// This is used to get the default presentation style to use
        /// </summary>
        /// <remarks>The default is the VS2005 style</remarks>
        public static string DefaultStyle
        {
            get
            {
                PresentationStyleSettings pss = null;

                // If the presentation style file didn't exist or wasn't valid, the collection will be null here
                if(styles == null)
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        "The presentation style definitions were not loaded.  Does a valid definition file " +
                        "exist ({0})?", PresentationStyleDictionary.PresentationStyleFilePath));

                if(!styles.TryGetValue("VS2005", out pss))
                    pss = styles.Values.First();

                return pss.Id;
            }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to get the standard values by loading the standard presentation style definition file
        /// used by the Sandcastle tools.
        /// </summary>
        private static StandardValuesCollection InitializeStandardValues()
        {
            try
            {
                styles = PresentationStyleDictionary.LoadStandardPresentationStyleDictionary();
            }
            catch
            {
                // Not much we can do, so just return a dummy set of values.  The DefaultStyle property will
                // fail and throw a better exception than we get if we let this method fail.
                return new StandardValuesCollection(new[] { "VS2005" });
            }

            return new StandardValuesCollection(styles.Keys);
        }

        /// <summary>
        /// This is overridden to return the values for the type converter's dropdown list.
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
        #endregion
    }
}
