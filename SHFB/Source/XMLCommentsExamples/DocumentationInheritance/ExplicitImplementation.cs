//===============================================================================================================
// System  : Sandcastle Tools - XML Comments Example
// File    : ExplicitImplementation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
//
// This class is used to demonstrate the inheritdoc XML comments element.  It serves no useful purpose.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/06/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections;

namespace XMLCommentsExamples.DocumentationInheritance
{
    #region Interface implementation documentation inheritance
    /// <summary>
    /// A class with an explicit interface implementation
    /// </summary>
    /// <remarks>Note that you must enable the <b>DocumentExplicitInterfaceImplementations</b>
    /// SHFB project options in order to see the explicitly implemented members.</remarks>
    /// <conceptualLink target="86453FFB-B978-4A2A-9EB5-70E118CA8073" />
    public class ExplicitImplementation : ICollection, ICloneable, IEnumerable
    {
        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            // Comments are automatically inherited for explicit
            // interface members with no comments.
        }

        // Comments are automatically inherited for explicit
        // interface members with no comments.
        int ICollection.Count => 0;

        // Comments are automatically inherited for explicit
        // interface members with no comments.
        bool ICollection.IsSynchronized => true;

        // In this case, we inherit the <summary> and <returns>
        // comments and add a <remarks> comment.  Because we added
        // comments, we need to specify the <inheritdoc /> tag too.
        /// <inheritdoc />
        /// <remarks>This is a dummy class and always returns null.</remarks>
        object ICollection.SyncRoot => null;

        #endregion

        #region IEnumerable Members

        /// <inheritdoc />
        /// <returns>This is a dummy class so it throws an exception</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // In this case, we automatically inherit the base interface's
            // <summary> but override the <returns> documentation.  As above,
            // because we specified comments, we have to add the <inheritdoc />
            // tag too.
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region ICloneable Members

        /// <inheritdoc />
        public object Clone()
        {
            // Not explicitly implemented so we have to tell it to inherit
            // documentation on this one.
            return null;
        }

        #endregion
    }
    #endregion
}
