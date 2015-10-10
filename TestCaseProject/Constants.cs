using System;
using System.Drawing;

namespace TestDoc
{
    /// <summary>
    /// Test constant values in the syntax section
    /// </summary>
    public static class ConstantsTest
    {
        /// <summary>Object value</summary>
        public const object ObjectValue = null;

        /// <summary>Type value</summary>
        public const Type TypeValue = null;

        /// <summary>Enumeration value</summary>
        public const KnownColor EnumValue = KnownColor.Blue;

        /// <summary>Null string value</summary>
        public const string NullStringValue = null;

        /// <summary>String value</summary>
        public const string StringValue = "Test string";

        /// <summary>Boolean value</summary>
        public const bool BoolValue = true;

        /// <summary>Byte value</summary>
        public const byte ByteValue = 0x1A;

        /// <summary>Character value</summary>
        public const char CharValue = 'A';

        /// <summary>Short value</summary>
        public const short ShortValue = 1;

        /// <summary>Integer value</summary>
        public const int IntegerValue = Int32.MaxValue;

        /// <summary>Long value</summary>
        public const long LongValue = Int64.MaxValue;

        /// <summary>Float value</summary>
        public const float FloatValue = 1.52f;

        /// <summary>double value</summary>
        public const double DoubleValue = 2.55;

        /// <summary>Decimal value</summary>
        public const decimal DecimalValue = 10.52m;

        /// <summary>Unsigned integer value</summary>
        public const uint UnsignedIntegerValue = UInt32.MaxValue;

        /// <summary>Integer array value</summary>
        public const int[] IntArrayValue = null;

        /// <summary>Version value</summary>
        public const Version VersionValue = null;
    }
}
