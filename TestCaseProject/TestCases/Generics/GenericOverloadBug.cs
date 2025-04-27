using System;

namespace TestDoc.Generics.OverloadBug
{
    /// <summary>
    /// Problems with members containing generic and non-generic types.
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
    /// <remarks>
    /// Reported by David Hearn:
    ///
    /// <para>The following problem occurs when a class inherits from a generic base class that contains
    /// overloaded methods.  One of the overloaded methods has the generic type as a parameter.</para>
    ///
    /// <para>Hana and Prototype style: The generated documentation for the base class looks correct, but the
    /// derived class (Class2) shows the overloaded method with the generic parameter twice in the list of
    /// members, one time without an icon and one with an icon. The description for the one without an icon is
    /// messed up. The description for the one with the icon lists the summary twice. The second overloaded
    /// method is not shown.</para>
    ///
    /// <para>VS2005 style: The base class is correct but the derived class only shows the Contains(T) member and
    /// the "inherited from" description is incorrect as it shows the base class twice.</para>
    /// 
    /// <para>Fixed 03/15/2012.</para>
    /// </remarks>
    public class Class2 : Class1<int>
    {
    }

    /// <summary>
    /// Summary
    /// </summary>
    /// <typeparam name="T1">Some Type</typeparam>
    /// <typeparam name="T2">Some Type</typeparam>
    public interface ITest2<T1, T2>
    {
        /// <summary>
        /// Summary
        /// </summary>
        /// <param name="paramT2">parameter</param>
        /// <returns>returns</returns>
        T1 Map(T2 paramT2);

        /// <summary>
        /// Summary
        /// </summary>
        /// <param name="paramT1">parameter</param>
        /// <returns>returns</returns>
        T2 Map(T1 paramT1);
    }

    /// <summary>
    /// Summary
    /// </summary>
    /// <typeparam name="T">Some Type</typeparam>
    /// <remarks>
    /// Reported by Sam Harwell
    /// 
    /// <para>When a base class contains members with generic parameters and the derived class's template
    /// parameters are different, it doesn't correctly match the inherited members and generates a duplicate
    /// member entry for the first inherited member it finds.  In this case, ITest2&lt;T1, T2&gt;.Map(T2).
    /// This throws off the document model transform resulting in an unrelated missing shared content item
    /// warning from BuildAssembler.</para>
    /// 
    /// <para>Fixed 03/11/2014.</para>
    /// </remarks>
    public interface ITest<T> : ITest2<Exception, T>
    {
        /// <summary>
        /// Summary
        /// </summary>
        /// <param name="paramString">parameter</param>
        /// <returns>returns</returns>
        T Map(string paramString);
    }
}
