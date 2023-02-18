using System;
using System.Collections.Generic;

namespace TestDoc.Generics.HardToMatchSignatures
{
    /// <summary>Class A.</summary>
    /// <typeparam name="T">T</typeparam>
    public class A<T>
    {
        /// <summary>Method M, TUT variant.</summary>
        /// <param name="f">f</param>
        /// <param name="x">x</param>
        /// <typeparam name="U">U</typeparam>
        /// <returns>Integer</returns>
        public int M<U>(Func<T, U, T> f, int x) => x;

        /// <summary>Method M, UTT variant.</summary>
        /// <param name="f">f</param>
        /// <param name="x">x</param>
        /// <typeparam name="U">U</typeparam>
        /// <returns>Integer</returns>
        public int M<U>(Func<U, T, T> f, int x) => x;
    }

    /// <summary>
    /// Class B
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <remarks>Should show both inherited overloads for M&lt;U&gt;</remarks>
    public class B<T> : A<T>
    {
    }

    /// <summary>
    /// RandomGenericClass
    /// </summary>
    /// <typeparam name="TName">TName</typeparam>
    /// <typeparam name="TIndex">TIndex</typeparam>
    /// <typeparam name="TType">TType</typeparam>
    public class RandomGenericClass<TName, TIndex, TType>
    {
        internal RandomGenericClass()
        {
            Console.WriteLine(" I am constructing. ");
        }

        /// <summary>
        /// Documentation for Overload int.
        /// </summary>
        /// <param name="overload">The parameter.</param>
        /// <returns>The return.</returns>
        public int[] OverloadedMethod(int[] overload)
        {
            return overload;
        }

        /// <summary>
        /// Documentation for Overload single.
        /// </summary>
        /// <param name="overload">The parameter.</param>
        /// <returns>The return.</returns>
        public Single[] OverloadedMethod(Single[] overload)
        {
            return overload;
        }

        /// <summary>
        /// Documentation for Overload double.
        /// </summary>
        /// <param name="overload">The parameter.</param>
        /// <returns>The return</returns>
        public double[] OverloadedMethod(double[] overload)
        {
            return overload;
        }
    }

    /// <summary>
    /// InheritedRandomGeneric
    /// </summary>
    /// <remarks>Should show all of the inherited OverloadedMethod members</remarks>
    public class InheritedRandomGeneric : RandomGenericClass<String, int, float>
    {
    }

    /// <summary>
    /// MyDataSeries TX, TY
    /// </summary>
    /// <typeparam name="TX">TX</typeparam>
    /// <typeparam name="TY">TY</typeparam>
    public abstract class MyDataSeries<TX, TY>
        where TX : IComparable
        where TY : IComparable
    {
        /// <summary>
        /// Append
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="yValues">yValues</param>
        public virtual void Append(IEnumerable<TX> x, params IEnumerable<TY>[] yValues) { }

        /// <summary>
        /// Append
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="yValues">yValues</param>
        public virtual void Append(TX x, params TY[] yValues) { }
    }

    /// <summary>
    /// MyDataSeries TX, TY, TZ
    /// </summary>
    /// <typeparam name="TX">TX</typeparam>
    /// <typeparam name="TY">TY</typeparam>
    /// <typeparam name="TZ">TZ</typeparam>
    public class MyDataSeries<TX, TY, TZ> : MyDataSeries<TX, TY>
        where TX : IComparable
        where TY : IComparable
        where TZ : IComparable
    {
        /// <summary>
        /// Append
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="yValues">yValues</param>
        public override void Append(IEnumerable<TX> x, params IEnumerable<TY>[] yValues) { }

        /// <summary>
        /// Append
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="yValues">yValues</param>
        public override void Append(TX x, params TY[] yValues) { }
    }

    /// <summary>
    /// MyTimeDataSeries
    /// </summary>
    /// <remarks>Should show the inherited overloaded Append methods</remarks>
    public class MyTimeDataSeries : MyDataSeries<DateTime, double, double>
    {
    }
}
