using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDoc.LanguageFeatures
{
    /// <summary>
    /// Tests C# 7.2 ref return values.
    /// </summary>
    public class RefReturnTest
    {
        /// <summary>
        /// Value type ref return test.
        /// </summary>
        /// <param name="param">Ref parameter.</param>
        /// <returns>Ref return value.</returns>
        public ref int ValueRefReturnMethod(ref int param)
        {
            return ref param;
        }

        /// <summary>
        /// Value type ref return test.
        /// </summary>
        /// <param name="param1">Ref parameter 1.</param>
        /// <param name="param2">Ref parameter 2.</param>
        /// <returns>Ref return value.</returns>
        public ref int ValueRefReturnMethod(ref int param1, ref int param2)
        {
            return ref param1;
        }

        /// <summary>
        /// Reference type ref return test.
        /// </summary>
        /// <param name="param">Ref parameter.</param>
        /// <returns>Ref return value.</returns>
        public ref object ClassRefReturnMethod(ref object param)
        {
            return ref param;
        }

        /// <summary>
        /// Reference type ref return test.
        /// </summary>
        /// <param name="param1">Ref parameter 1.</param>
        /// <param name="param2">Ref parameter 2.</param>
        /// <returns>Ref return value.</returns>
        public ref object ClassRefReturnMethod(ref object param1, ref object param2)
        {
            return ref param1;
        }

        /// <summary>
        /// Generic type ref return test.
        /// </summary>
        /// <param name="param">Ref parameter.</param>
        /// <typeparam name="T">Type parameter.</typeparam>
        /// <returns>Ref return value.</returns>
        public ref T GenericRefReturnMethod<T>(ref T param)
        {
            return ref param;
        }

        /// <summary>
        /// Generic type ref return test.
        /// </summary>
        /// <param name="param1">Ref parameter 1.</param>
        /// <param name="param2">Ref parameter 2.</param>
        /// <typeparam name="T">Type parameter.</typeparam>
        /// <returns>Ref return value.</returns>
        public ref T GenericRefReturnMethod<T>(ref T param1, ref T param2)
        {
            return ref param1;
        }

        /// <summary>
        /// Value type return test.
        /// </summary>
        /// <param name="param">Parameter.</param>
        /// <returns>Return value.</returns>
        public int ValueReturnMethod(int param)
        {
            return param;
        }

        /// <summary>
        /// Value type return test.
        /// </summary>
        /// <param name="param1">Parameter 1.</param>
        /// <param name="param2">Parameter 2.</param>
        /// <returns>Return value.</returns>
        public int ValueReturnMethod(int param1, int param2)
        {
            return param1;
        }

        /// <summary>
        /// Reference type return test.
        /// </summary>
        /// <param name="param">Parameter 1.</param>
        /// <returns>Return value.</returns>
        public object ClassReturnMethod(object param)
        {
            return param;
        }

        /// <summary>
        /// Reference type return test.
        /// </summary>
        /// <param name="param1">Parameter 1.</param>
        /// <param name="param2">Parameter 2.</param>
        /// <returns>Return value.</returns>
        public object ClassReturnMethod(object param1, object param2)
        {
            return param1;
        }

        /// <summary>
        /// Generic type return test.
        /// </summary>
        /// <param name="param">Parameter 1.</param>
        /// <typeparam name="T">Type parameter.</typeparam>
        /// <returns>Return value.</returns>
        public T GenericReturnMethod<T>(T param)
        {
            return param;
        }

        /// <summary>
        /// Generic type return test.
        /// </summary>
        /// <param name="param1">Parameter 1.</param>
        /// <param name="param2">Parameter 2.</param>
        /// <typeparam name="T">Type parameter.</typeparam>
        /// <returns>Return value.</returns>
        public T GenericReturnMethod<T>(T param1, T param2)
        {
            return param1;
        }
    }
}
