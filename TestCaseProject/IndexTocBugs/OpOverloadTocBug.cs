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
        /// Operator + (Test1, Test1).  <see cref="operator -"/>
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static Test1 operator +(Test1 left, Test1 right)
        {
            return left;
        }

        /// <summary>
        /// Operator + (Test1, Test2)
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Right operand</returns>
        public static Test2 operator +(Test1 left, Test2 right)
        {
            return right;
        }

        /// <summary>
        /// Operator + (Test2, Test1).
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static Test2 operator +(Test2 left, Test1 right)
        {
            return left;
        }

        /// <summary>
        /// Operator -
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static Test2 operator -(Test2 left, Test1 right)
        {
            return left;
        }

        /// <summary>
        /// Operator *
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static Test2 operator *(Test2 left, Test1 right)
        {
            return left;
        }

        /// <summary>
        /// Operator /
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static Test2 operator /(Test2 left, Test1 right)
        {
            return left;
        }
    
        /// <summary>
        /// Operator %
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static Test2 operator %(Test2 left, Test1 right)
        {
            return left;
        }

        /// <summary>
        /// Operator &amp;
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static Test2 operator &(Test2 left, Test1 right)
        {
            return left;
        }

        /// <summary>
        /// Operator |
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static Test2 operator |(Test2 left, Test1 right)
        {
            return left;
        }

        /// <summary>
        /// Operator ^
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static Test2 operator ^(Test2 left, Test1 right)
        {
            return left;
        }

        /// <summary>
        /// Operator &lt;&lt;
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="bits">The bits.</param>
        /// <returns>Left operand</returns>
        public static Test1 operator <<(Test1 left, int bits)
        {
            return left;
        }

        /// <summary>
        /// Operator &gt;&gt;
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="bits">The bits.</param>
        /// <returns>Left operand</returns>
        public static Test1 operator >>(Test1 left, int bits)
        {
            return left;
        }

        /// <summary>
        /// Operator ==
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static bool operator ==(Test2 left, Test1 right)
        {
            return true;
        }

        /// <summary>
        /// Operator !=
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static bool operator !=(Test2 left, Test1 right)
        {
            return true;
        }

        /// <summary>
        /// Operator &lt;
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static bool operator <(Test2 left, Test1 right)
        {
            return true;
        }

        /// <summary>
        /// Operator &gt;
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static bool operator >(Test2 left, Test1 right)
        {
            return true;
        }

        /// <summary>
        /// Operator &lt;=
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static bool operator <=(Test2 left, Test1 right)
        {
            return true;
        }

        /// <summary>
        /// Operator >=
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
        public static bool operator >=(Test2 left, Test1 right)
        {
            return true;
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        /// <param name="test1">Test</param>
        /// <returns>String</returns>
        public static implicit operator String(Test1 test1)
        {
            return "Test";
        }

        /// <summary>
        /// Explicit conversion
        /// </summary>
        /// <param name="s">String</param>
        /// <returns>Test1</returns>
        public static explicit operator Test1(string s)
        {
            return new Test1();
        }

        /// <summary>
        /// Operator +
        /// </summary>
        /// <param name="left">The left.</param>
        /// <returns>Left operand</returns>
        public static Test1 operator +(Test1 left)
        {
            return left;
        }

        /// <summary>
        /// Operator -
        /// </summary>
        /// <param name="left">The left.</param>
        /// <returns>Left operand</returns>
        public static Test1 operator -(Test1 left)
        {
            return left;
        }

        /// <summary>
        /// Operator !
        /// </summary>
        /// <param name="left">The left.</param>
        /// <returns>Left operand</returns>
        public static bool operator !(Test1 left)
        {
            return true;
        }

        /// <summary>
        /// Operator ~
        /// </summary>
        /// <param name="left">The left.</param>
        /// <returns>Left operand</returns>
        public static Test1 operator ~(Test1 left)
        {
            return left;
        }

        /// <summary>
        /// Operator ++
        /// </summary>
        /// <param name="left">The left.</param>
        /// <returns>Left operand</returns>
        public static Test1 operator ++(Test1 left)
        {
            return left;
        }

        /// <summary>
        /// Operator --
        /// </summary>
        /// <param name="left">The left.</param>
        /// <returns>Left operand</returns>
        public static Test1 operator --(Test1 left)
        {
            return left;
        }

        /// <summary>
        /// Operator true
        /// </summary>
        /// <param name="left">The left.</param>
        /// <returns>Left operand</returns>
        public static bool operator true(Test1 left)
        {
            return false;
        }

        /// <summary>
        /// Operator false
        /// </summary>
        /// <param name="left">The left.</param>
        /// <returns>Left operand</returns>
        public static bool operator false(Test1 left)
        {
            return false;
        }

        /// <summary>
        /// Test
        /// </summary>
        /// <param name="obj">Test</param>
        /// <returns>Test</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Test
        /// </summary>
        /// <returns>Test</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
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
        /// <returns>Left operand</returns>
        public static Test2 operator +(Test2 left, Test2 right)
        {
            return left;
        }

        /// <summary>
        /// Operator +s the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Right operand</returns>
        public static Test2 operator +(Test1 left, Test2 right)
        {
            return right;
        }

        /// <summary>
        /// Operator +s the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Left operand</returns>
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
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>x</returns>
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
