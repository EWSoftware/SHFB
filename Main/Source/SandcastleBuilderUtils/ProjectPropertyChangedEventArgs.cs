//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ProjectPropertyChangedEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/21/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the event arguments class that specifies which property
// changed along with its old and new values.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  06/21/2008  EFW  Created the code
//=============================================================================

using System;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This event arguments class is used to specify which project property
    /// changed along with the old and new values.
    /// </summary>
    public class ProjectPropertyChangedEventArgs : EventArgs
    {
        #region Private data members
        //=====================================================================

        private string propertyName, oldValue, newValue;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the property name
        /// </summary>
        public string PropertyName
        {
            get { return propertyName; }
        }

        /// <summary>
        /// This read-only property returns the old value
        /// </summary>
        public string OldValue
        {
            get { return oldValue; }
        }

        /// <summary>
        /// This read-only property returns the new value
        /// </summary>
        public string NewValue
        {
            get { return newValue; }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="oldVal">The old value</param>
        /// <param name="newVal">The new value</param>
        public ProjectPropertyChangedEventArgs(string name, string oldVal,
            string newVal)
        {
            propertyName = name;
            oldValue = oldVal;
            newValue = newVal;
        }
    }
}
