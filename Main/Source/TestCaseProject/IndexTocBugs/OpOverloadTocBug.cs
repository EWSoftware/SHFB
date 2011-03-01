using System;
using System.Collections.Generic;
using System.Text;

namespace TestDoc.IndexTocBugs
{
    /// <summary>
    /// Test1
    /// </summary>
    public struct Test1
    {
        /// <summary>
        /// Operator +s the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static Test1 operator +(Test1 left, Test1 right)
        {
            return left;
        }

        /// <summary>
        /// Operator +s the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static Test2 operator +(Test1 left, Test2 right)
        {
            return right;
        }

        /// <summary>
        /// Operator +s the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static Test2 operator +(Test2 left, Test1 right)
        {
            return left;
        }
    }

    /// <summary>
    /// Test2
    /// </summary>
    public struct Test2
    {
        /// <summary>
        /// Operator +s the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static Test2 operator +(Test2 left, Test2 right)
        {
            return left;
        }

        /// <summary>
        /// Operator +s the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static Test2 operator +(Test1 left, Test2 right)
        {
            return right;
        }

        /// <summary>
        /// Operator +s the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static Test2 operator +(Test2 left, Test1 right)
        {
            return left;
        }
    }

    /// <summary>
    /// Test2 - Supposedly causes a "file not found" exception with the
    /// VS2005 style.
    /// </summary>
    public struct Test3
    {
        /// <summary>
        /// operator *
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Test3 operator *(Test3 x, double y)
        {
            return x;
        }

        /// <summary>
        /// operator *
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <returns>returns</returns>
        public static Test3 operator *(Test3 x, Test3 y)
        {
            return x;
        }

        /// <summary>
        /// Multiply method
        /// </summary>
        /// <param name="x">x</param>
        /// <returns>returns</returns>
        public Test3 Multiply(Test3 x)
        {
            return this;
        }

        /// <summary>
        /// Multiply method
        /// </summary>
        /// <param name="x">x</param>
        /// <returns>returns</returns>
        public Test3 Multiply(double x)
        {
            return this;
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        /// <param name="value">The value to use</param>
        /// <returns>The converted value</returns>
        static public implicit operator int(Test3 value)
        {
            return 0;
        }

        /// <summary>
        /// Explicit conversion
        /// </summary>
        /// <param name="value">The value to use</param>
        /// <returns>The converted value</returns>
        static public explicit operator long(Test3 value)
        {
            return 0;
        }
    }
}
