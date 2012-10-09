using System;

namespace TestDoc.Generics.OverloadBug
{
    /// <summary>
    /// Reported by David Hearn:
    ///
    /// <p/>The following problem occurs when a class inherits from a generic
    /// base class that contains overloaded methods. One of the overloaded
    /// methods has the generic type as a parameter.
    ///
    /// <p/>Hana and Prototype style: The generated documentation for the base
    /// class looks correct, but the derived class (Class2) shows the
    /// overloaded method with the generic parameter twice in the list of
    /// members, one time without an icon and one with an icon. The description
    /// for the one without an icon is messed up. The description for the one
    /// with the icon lists the summary twice. The second overloaded method is
    /// not shown.
    ///
    /// <p/>VS2005 style: The base class is correct but the derived class only
    /// shows the Contains(T) member and the "inherited from" description is
    /// incorrect as it shows the base class twice.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// Class to test contains.
    /// </summary>
    /// <typeparam name="T">
    /// Type of items.
    /// </typeparam>
    public class Class1<T>
    {
        /// <summary>
        /// Determine if item exists.
        /// </summary>
        /// <param name="item">
        /// The item to check.
        /// </param>
        /// <returns>
        /// Returns false.
        /// </returns>
        public bool Contains(T item)
        {
            return false;
        }

        /// <summary>
        /// Determine if item exists by id.
        /// </summary>
        /// <param name="id">
        /// The id to check.
        /// </param>
        /// <returns>
        /// Returns false.
        /// </returns>
        public bool Contains(Guid id)
        {
            return false;
        }
    }

    /// <summary>
    /// Class that inherits from <see cref="Class1{T}"/>.
    /// </summary>
    public class Class2 : Class1<int>
    {
    }
}
