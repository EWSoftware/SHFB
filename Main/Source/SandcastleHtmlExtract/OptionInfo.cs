//=============================================================================
// System  : Sandcastle Help File Builder - HTML Extract
// File    : OptionInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/27/2008
// Note    : Copyright 2006-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the command line option information class.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.3.4.0  12/27/2006  EFW  Created the code
// 1.4.0.0  02/23/2007  EFW  Added support for a third value option
//=============================================================================

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SandcastleBuilder.HtmlExtract
{
    /// <summary>
    /// This class holds command line option information.
    /// </summary>
    public class OptionInfo
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private static Regex reSplit =
            new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        private string optionText, name, value, secondValue, thirdValue;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// Get the option text as specified
        /// </summary>
        public string OptionText
        {
            get { return optionText; }
        }

        /// <summary>
        /// Get the option name
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Get the option value
        /// </summary>
        public string Value
        {
            get { return value; }
        }

        /// <summary>
        /// Get the second option value if there was one
        /// </summary>
        public string SecondValue
        {
            get { return secondValue; }
        }

        /// <summary>
        /// Get the third option value if there was one
        /// </summary>
        public string ThirdValue
        {
            get { return thirdValue; }
        }
        #endregion

        //=====================================================================
        // Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="option">The option text to parse</param>
        public OptionInfo(string option)
        {
            int pos;

            if(String.IsNullOrEmpty(option))
                return;

            optionText = option.Trim();

            // Is is a project file
            if(optionText[0] != '-' && optionText[0] != '/')
            {
                name = "project";
                value = optionText;
                return;
            }

            pos = optionText.IndexOf('=');

            if(pos < 2)
                name = optionText.Substring(1).ToLower(
                    CultureInfo.InvariantCulture);
            else
            {
                name = optionText.Substring(1, pos - 1).Trim().ToLower(
                    CultureInfo.InvariantCulture);
                value = optionText.Substring(pos + 1).Trim();

                if(reSplit.IsMatch(value))
                {
                    string[] parts = reSplit.Split(value);
                    value = parts[0].Trim();
                    secondValue = parts[1].Trim();

                    if(parts.Length > 2)
                        thirdValue = parts[2].Trim();
                }

                // Strip quotes from around the values
                if(value[0] == '\"')
                    value = value.Substring(1, value.Length - 2).Trim();

                if(!String.IsNullOrEmpty(secondValue) && secondValue[0] == '\"')
                    secondValue = secondValue.Substring(1,
                        secondValue.Length - 2).Trim();

                if(!String.IsNullOrEmpty(thirdValue) && thirdValue[0] == '\"')
                    thirdValue = thirdValue.Substring(1,
                        thirdValue.Length - 2).Trim();
            }
        }
    }
}
