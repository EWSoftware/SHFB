//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : MemberIdMatchExpression.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/04/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to contain the member ID match expression settings
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/14/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This is used to hold the member ID match expression settings
    /// </summary>
    public class MemberIdMatchExpression : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private Regex regex;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// The member ID expression to match
        /// </summary>
        public string MatchExpression
        {
            get => field;
            set
            {
                if(field != value)
                {
                    field = value;
                    regex = null;

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The replacement member ID value
        /// </summary>
        public string ReplacementValue
        {
            get => field;
            set
            {
                if(field != value)
                {
                    field = value;

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Match and replace the member ID using a regular expression
        /// </summary>
        /// <value>True to match and replace as a regular expression, false to use literal match and replace</value>
        public bool MatchAsRegEx
        {
            get => field;
            set
            {
                if(field != value)
                {
                    field = value;

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This read-only property returns an error message describing any issues with the settings
        /// </summary>
        public string ErrorMessage
        {
            get => field;
            private set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This read-only property returns a <see cref="Regex"/> for the expression if it should be matched as a
        /// regular expression or null if not.
        /// </summary>
        /// <remarks>The regular expression is cached for future use</remarks>
        public Regex RegularExpression
        {
            get
            {
                if(this.MatchAsRegEx && regex == null)
                    regex = new Regex(this.MatchExpression);

                return regex;
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to validate the settings
        /// </summary>
        private void Validate()
        {
            List<string> problems = [];

            if(String.IsNullOrWhiteSpace(this.MatchExpression))
                problems.Add("A match expression is required");
            else
            {
                if(this.MatchAsRegEx)
                {
                    try
                    {
                        // Make an attempt at validating the expression.  Just its syntax, not necessarily that it
                        // will work in the reflection file.
                        Regex reTest = new(this.MatchExpression.Trim());

                        reTest.Replace("T:System.Object", (this.ReplacementValue ?? String.Empty).Trim());
                    }
                    catch(Exception ex)
                    {
                        problems.Add("Invalid regular expression: " + ex.Message);
                    }
                }
            }

            if(String.IsNullOrWhiteSpace(this.ReplacementValue))
                problems.Add("A replacement value is required");

            if(problems.Count != 0)
                this.ErrorMessage = String.Join(" / ", problems);
            else
                this.ErrorMessage = null;
        }
        #endregion

        #region INotifyPropertyChanged implementation
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
