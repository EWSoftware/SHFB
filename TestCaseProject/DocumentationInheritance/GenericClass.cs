using System;
using System.Collections;
using System.Collections.Generic;

namespace TestDoc.DocumentationInheritance
{
    #region Generic-derived class with inherited documentation
    //=====================================================================

    /// <summary>
    /// A structure with a generic base class that has inherited documentation
    /// on its members.
    /// </summary>
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
