using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDoc.LanguageFeatures
{
    /// <summary>
    /// Tests C# 7.2 readonly struct.
    /// </summary>
    public class ReadOnlyStructTest
    {
        /// <summary>
        /// Field test.
        /// </summary>
        public ReadOnlyStruct ReadOnlyStructField;

        /// <summary>
        /// Property test.
        /// </summary>
        public ReadOnlyStruct ReadOnlyStructProperty { get; set; }

        /// <summary>
        /// Method test.
        /// </summary>
        /// <returns>Readonly struct value.</returns>
        public ReadOnlyStruct Method()
        {
            return new ReadOnlyStruct(0, 0);
        }

        /// <summary>
        /// Method test.
        /// </summary>
        /// <param name="param">Readonly struct param.</param>
        /// <returns>Readonly struct value.</returns>
        public ReadOnlyStruct Method(ReadOnlyStruct param)
        {
            return param;
        }

        /// <summary>
        /// Method test.
        /// </summary>
        /// <param name="param1">Readonly struct param 1.</param>
        /// <param name="param2">Readonly struct param 2.</param>
        /// <returns>Readonly struct value.</returns>
        public ReadOnlyStruct Method(ReadOnlyStruct param1, ReadOnlyStruct param2)
        {
            return param1;
        }
    }

    /// <summary>
    /// Readonly struct.
    /// </summary>
    public readonly struct ReadOnlyStruct
    {
        /// <summary>
        /// Field.
        /// </summary>
        public readonly int Field;

        /// <summary>
        /// Property.
        /// </summary>
        public int Property { get; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="field">Field param.</param>
        /// <param name="property">Property param.</param>
        public ReadOnlyStruct(int field, int property)
        {
            this.Field = field;
            this.Property = property;
        }

        /// <summary>
        /// Method.
        /// </summary>
        /// <param name="other">Readonly struct param.</param>
        /// <returns>Readonly struct return value.</returns>
        public ReadOnlyStruct Method(ReadOnlyStruct other)
        {
            return new ReadOnlyStruct(other.Field, other.Property);
        }
    }
}
