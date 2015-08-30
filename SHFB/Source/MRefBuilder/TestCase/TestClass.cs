//===============================================================================================================
// System  : Sandcastle MRefBuilder
// File    : TestClass.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/07/2015
// Compiler: Microsoft Visual C#
//
// This class is used to test small snippets of code with MRefBuilder to diagnose problems
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/01/2012  EFW  Created the code
//===============================================================================================================

// For debugging:
// Command line arguments: /out:reflection.org C:\GH\SHFB\SHFB\Source\MRefBuilder\TestCase\bin\Debug\TestCase.dll
// Working directory: C:\GH\SHFB\SHFB\Deploy\

using System;
using System.Collections.Generic;

namespace ShfbCrashTest
{
    public class MyClass<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
    {
        public bool Compare(KeyValuePair<TKey, TValue>[] other, bool nothing) { return false; }
        public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) { return 0; }
    }

    public class MyNewClass : MyClass<int, int>
    {
    }

    public abstract class MyDataSeries<TX, TY>
        where TX : IComparable
        where TY : IComparable
    {
        public virtual void Append(IEnumerable<TX> x, params IEnumerable<TY>[] yValues) { }
        public virtual void Append(TX x, params TY[] yValues) { }
    }

    public class MyDataSeries<TX, TY, TZ> : MyDataSeries<TX, TY>
        where TX : IComparable
        where TY : IComparable
        where TZ : IComparable
    {
        public override void Append(IEnumerable<TX> x, params IEnumerable<TY>[] yValues) { }
        public override void Append(TX x, params TY[] yValues) { }
    }

    public class MyTimeDataSeries : MyDataSeries<DateTime, double, double>
    {
    }

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

    public class InheritedRandomGeneric : RandomGenericClass<String, int, float> { }

}
