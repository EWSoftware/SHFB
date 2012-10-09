using System;
using System.Drawing;

namespace TestDoc
{
    /// <summary>
    /// This is a dummy attribute used for testing
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class DummyAttribute : Attribute
    {
        /// <summary>Object value</summary>
        public object ObjectValue { get; set; }

        /// <summary>Type value</summary>
        public Type TypeValue { get; set; }

        /// <summary>Enumeration valuie</summary>
        public KnownColor EnumValue { get; set; }

        /// <summary>String value</summary>
        public string StringValue { get; set; }

//bool, byte, char, short, int, long, float, and double
        /// <summary>Boolean value</summary>
        public bool BoolValue { get; set; }

        /// <summary>Byte value</summary>
        public byte ByteValue { get; set; }

        /// <summary>Character value</summary>
        public char CharValue { get; set; }

        /// <summary>Short value</summary>
        public short ShortValue { get; set; }

        /// <summary>Integer value</summary>
        public int IntegerValue { get; set; }

        /// <summary>Long value</summary>
        public long LongValue { get; set; }

        /// <summary>Float value</summary>
        public float FloatValue { get; set; }

        /// <summary>double value</summary>
        public double DoubleValue { get; set; }

        /// <summary>Unsigned integer value</summary>
        public uint UnsignedIntegerValue { get; set; }

        /// <summary>Integer array value</summary>
        public int[] IntArrayValue { get; set; }
    }
}
