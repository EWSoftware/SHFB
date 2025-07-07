//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : Token.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains a class representing a conceptual content token that can be used to insert a common item,
// value, or construct into topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/24/2008  EFW  Created the code
// 07/25/2008  EFW  Reworked to support new MSBuild project format
// 12/22/2011  EFW  Updated for use with the new token file editor
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sandcastle.Core.ConceptualContent
{
    /// <summary>
    /// This represents a conceptual content token that can be used to insert a common item, value, or construct
    /// into topics.
    /// </summary>
    public class Token : INotifyPropertyChanged
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the token name
        /// </summary>
        /// <value>If the value is null or empty, a new GUID is assigned as the name</value>
        public string TokenName
        {
            get;
            set
            {
                if(value == null || value != field)
                {
                    if(String.IsNullOrWhiteSpace(value))
                        field = Guid.NewGuid().ToString();
                    else
                        field = value;

                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the token value
        /// </summary>
        /// <value>The value can contain help file builder replacement tags.  These will be replaced at build
        /// time with the appropriate project value.</value>
        public string TokenValue
        {
            get;
            set
            {
                if(value != field)
                {
                    field = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the token is selected
        /// </summary>
        /// <remarks>Used by the editor for binding in the list box</remarks>
        public bool IsSelected
        {
            get;
            set
            {
                if(value != field)
                {
                    field = value;
                    this.OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>The token name defaults to a GUID</remarks>
        public Token() : this(null, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The token name</param>
        /// <param name="value">The token value</param>
        public Token(string name, string value)
        {
            this.TokenName = name;
            this.TokenValue = value;
        }
        #endregion

        #region INotifyPropertyChanged Members
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Convert to token element format
        //=====================================================================

        /// <summary>
        /// Convert the token to its <c>&lt;token&gt;</c> element form
        /// </summary>
        /// <returns>The token in its <c>&lt;token&gt;</c> element form</returns>
        public string ToToken()
        {
            return String.Concat("<token>", this.TokenName, "</token>");
        }
        #endregion
    }
}
