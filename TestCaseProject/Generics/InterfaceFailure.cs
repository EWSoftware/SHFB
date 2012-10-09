using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDoc.Generics.InterfaceFailure
{
    /// <summary>
    /// See http://shfb.codeplex.com/WorkItem/View.aspx?WorkItemId=22970 for
    /// details.
    /// </summary>
    public interface ITestInterface
    {
        /// <summary>
        /// FooBar method
        /// </summary>
        /// <typeparam name="T">Some type</typeparam>
        /// <param name="iarg">Some other arg</param>
        /// <param name="arg">Some arg</param>
        /// <returns>Some value</returns>
        T FooBar<T>(int iarg, T arg);

        /// <summary>
        /// asfdasdf
        /// </summary>
        /// <typeparam name="T">asfdasdf</typeparam>
        /// <param name="arg">asdfasdf</param>
        /// <returns>asfdasdf</returns>
        object BarBaz<T>(T arg);

        /// <summary>
        /// asfdasdf
        /// </summary>
        /// <typeparam name="T">asdfasdf</typeparam>
        /// <typeparam name="U">af23rwer</typeparam>
        /// <param name="arg1">qwerqwer</param>
        /// <param name="arg2">zvczxcvzx</param>
        /// <returns></returns>
        T BazGlorp<T, U>(T arg1, U arg2);

        /// <summary>
        /// ertertw
        /// </summary>
        /// <typeparam name="T">tyurtyu</typeparam>
        /// <typeparam name="U">bdfgsdf</typeparam>
        /// <param name="arg1">nvbn</param>
        /// <param name="arg2">rtyrtyf</param>
        /// <returns></returns>
        object GlorpQux<T, U>(T arg1, U arg2);
    }

    /// <summary>
    /// TestClass
    /// </summary>
    public class TestClass : ITestInterface
    {
        #region ITestInterface Members

        /// <inheritdoc />
        public T FooBar<T>(int iarg, T arg)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public object BarBaz<T>(T arg)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public T BazGlorp<T, U>(T arg1, U arg2)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public object GlorpQux<T, U>(T arg1, U arg2)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// safasfdasfdasdfas
    /// </summary>
    public abstract class TestClass2
    {
        /// <summary>
        /// asfdasdfasdf
        /// </summary>
        /// <typeparam name="T">asfdasdfasdfas</typeparam>
        /// <param name="i">2342342</param>
        /// <returns>gasgsadgasgd</returns>
        public abstract T SomeMethod<T>(int i);
    }

    /// <summary>
    /// asfdaf2q234
    /// </summary>
    public class TestClass2Derived : TestClass2
    {
        /// <inheritdoc />
        public override T SomeMethod<T>(int i)
        {
            throw new NotImplementedException();
        }
    }

}
