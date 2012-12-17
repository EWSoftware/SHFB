//===============================================================================================================
// System  : Sandcastle Tools - XML Comments Example
// File    : SetDocumentation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This class is used to demonstrate the inheritdoc XML comments element.  It servers no useful purpose.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  12/06/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections;
using System.Collections.Generic;

namespace XMLCommentsExamples.DocumentationInheritance
{
    #region Generic-derived class with inherited documentation
    //=====================================================================

    /// <summary>
    /// A structure with a generic base class that has inherited documentation on its members
    /// </summary>
    /// <conceptualLink target="86453FFB-B978-4A2A-9EB5-70E118CA8073" />
    public struct SetDocumentation : ISet<int>
    {
        /// <inheritdoc />
        public IEnumerator<int> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        void ICollection<int>.Add(int item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool SetEquals(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool Add(int item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Contains(int item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CopyTo(int[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Remove(int item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
    #endregion
}
