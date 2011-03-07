using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TestDoc.Generics.MissingMembersBug
{
    #region Action delegate bug
    //=====================================================================

    /// <summary>
    /// Due to the Action delegate, members are not listed if compiled as
    /// a .NET 4.0 assembly (May 2008 release).
    /// </summary>
    public interface Intro
    {
        /// <summary>
        /// Some method.
        /// </summary>
        void Method(Action<int, int> i);
    }

    /// <summary>
    /// Due to the Action delegate, members are not listed if compiled as
    /// a .NET 4.0 assembly (May 2008 release).
    /// </summary>
    public class Class1 : Intro
    {
        // Note also that inheritdoc doesn't work here either.  Most likely it
        // is related to the issue demonstrated by Generics.InterfaceFailure.TestClass.
        /// <inheritdoc />
        public void Method(Action<int, int> i)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Missing documentation issues with members containing delegates
    //=====================================================================

    /// <summary>
    /// This is to test the fix related to missing member documentation due
    /// to an incorrect framework path setting being passed to MRefBuilder.
    /// </summary>
    public static class DelegageMemberIssues
    {
        /// <summary>
        /// Invokes a handler on each <see cref="Exception"/> contained by this <see cref="AggregateException"/>.
        /// </summary>
        /// <param name="aggregateException">
        /// The <see cref="AggregateException"/>.
        /// </param>
        /// <param name="predicate">
        /// The predicate to execute for each exception. The predicate accepts as an argument the 
        /// <see cref="Exception"/> to be processed and returns a boolean to indicate whether 
        /// the exception was handled.
        /// </param>
        /// <param name="leaveStructureIntact">
        /// <see langword="true"/> if the re-thrown <see cref="AggregateException"/>
        /// should maintain the same hierarchy as the original.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="aggregateException"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="predicate"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="AggregateException">
        /// Not all of the inner exceptions were handled.
        /// </exception>
        public static void Handle(this AggregateException aggregateException, Func<Exception, bool> predicate, bool leaveStructureIntact)
        {
        }

        /// <summary>
        /// Applies arguments to a function, and returns a simpler function.
        /// </summary>
        /// <typeparam name="T1">
        /// The type of the first argument.
        /// </typeparam>
        /// <typeparam name="T2">
        /// The type of the second argument.
        /// </typeparam>
        /// <typeparam name="T3">
        /// The type of the third argument.
        /// </typeparam>
        /// <typeparam name="T4">
        /// The type of the fourth argument.
        /// </typeparam>
        /// <typeparam name="T5">
        /// The type of the fifth argument.
        /// </typeparam>
        /// <typeparam name="T6">
        /// The type of the sixth argument.
        /// </typeparam>
        /// <typeparam name="T7">
        /// The type of the seventh argument.
        /// </typeparam>
        /// <typeparam name="T8">
        /// The type of the eighth argument.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The type of the result.
        /// </typeparam>
        /// <param name="method">
        /// The function to which the arguments are applied.
        /// </param>
        /// <param name="a1">
        /// The first argument to apply.
        /// </param>
        /// <returns>
        /// A function which applies the supplied arguments to the <paramref name="method"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="method"/> is <see langword="null"/>.
        /// </exception>
        public static Func<T2, T3, T4, T5, T6, T7, T8, TResult> Apply<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> method, T1 a1)
        {
            if(null == method)
                throw new ArgumentNullException("method");
            return (a2, a3, a4, a5, a6, a7, a8) => method(a1, a2, a3, a4, a5, a6, a7, a8);
        }

        /// <summary>
        /// Applies arguments to a function, and returns a simpler function.
        /// </summary>
        /// <typeparam name="T1">
        /// The type of the first argument.
        /// </typeparam>
        /// <typeparam name="T2">
        /// The type of the second argument.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The type of the result.
        /// </typeparam>
        /// <param name="method">
        /// The function to which the arguments are applied.
        /// </param>
        /// <param name="a1">
        /// The first argument to apply.
        /// </param>
        /// <returns>
        /// A function which applies the supplied arguments to the <paramref name="method"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="method"/> is <see langword="null"/>.
        /// </exception>
        public static Func<T2, TResult> Apply<T1, T2, TResult>(this Func<T1, T2, TResult> method, T1 a1)
        {
            if(null == method)
                throw new ArgumentNullException("method");
            return a2 => method(a1, a2);
        }

        /// <summary>
        /// Applies a transformation to the result of a task.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the source task.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The return type of the transformation.
        /// </typeparam>
        /// <param name="source">
        /// The source task.
        /// </param>
        /// <param name="selector">
        /// The function which transforms the result of the task.
        /// </param>
        /// <returns>
        /// A task which returns the transformed result.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="source"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="selector"/> is <see langword="null"/>.</para>
        /// </exception>
        public static Task<TResult> Select<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> selector)
        {
            if(source == null)
                throw new ArgumentNullException("source");
            if(selector == null)
                throw new ArgumentNullException("selector");
            return source.ContinueWith(t => selector(t.Result), TaskContinuationOptions.NotOnCanceled);
        }

    }
    #endregion

    #region LINQ bug
    //=====================================================================

    /// <summary>
    /// Another May 2008 missing member bug in .NET 4.0 assemblies
    /// </summary>
    public abstract class BugTest
    {
        /// <summary>
        /// This Public Property should show up in the help file
        /// </summary>
        public String PublicProperty { get; set; }

        /// <summary>
        /// This protected Method should also show up in the help
        /// </summary>
        protected virtual Dictionary<string, object> GetPropertyValues()
        {
            // If this piece of code is uncommented, the members of this class will not show up
            // in the generated Help file. Besides this, you cannot see the members of this class
            // when you click on APIFilters and navigate to this class.
            #region Sandcastle bug
            var dict = this.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(x => (x.CanRead && x.CanWrite))
                .ToDictionary(x => x.Name, x => x.GetValue(this, null));
            #endregion

            // This piece of code does the same like the code above, 
            // but now Sandcastle will generate the correct help file.

            #region Workaround
            //Dictionary<string, object> dict = new Dictionary<string, object>();
            //var propertyInfos = this.GetType()
            //                        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            //foreach (var item in propertyInfos)
            //    if (item.CanRead && item.CanWrite)
            //        dict.Add(item.Name, item.GetValue(this, null));
            #endregion

            return dict;
        }
    }
    #endregion

    #region Tuple bug
    //=====================================================================

    /// <summary>
    /// May 2008 generics bug
    /// </summary>
    public class TupleBug
    {
        /// <summary>
        /// String property
        /// </summary>
        public string StringProperty { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TupleBug()
        {
        }

        /// <summary>
        /// The use of the Tuple causes all members to be removed in the May
        /// 2008 release of Sandcastle.
        /// </summary>
        /// <param name="value">A value</param>
        public void MissingMembers(Tuple<string, int> value)
        {
        }
    }
    #endregion
}
