namespace TestDoc.LanguageFeatures
{
    /// <summary>
    /// Tests C# 7.2 ref struct.
    /// </summary>
    public class RefStructTest
    {
        /// <summary>
        /// Method test.
        /// </summary>
        /// <returns>Ref struct value.</returns>
        public RefStruct Method()
        {
            return new RefStruct(0, 0);
        }

        /// <summary>
        /// Method test.
        /// </summary>
        /// <param name="param">Ref struct param.</param>
        /// <returns>Ref struct value.</returns>
        public RefStruct Method(RefStruct param)
        {
            return param;
        }

        /// <summary>
        /// Method test.
        /// </summary>
        /// <param name="param1">Ref struct param 1.</param>
        /// <param name="param2">Ref struct param 2.</param>
        /// <returns>Ref struct value.</returns>
        public RefStruct Method(RefStruct param1, RefStruct param2)
        {
            return param1;
        }
    }

    /// <summary>
    /// Ref struct.
    /// </summary>
    public ref struct RefStruct
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
        public RefStruct(int field, int property)
        {
            this.Field = field;
            this.Property = property;
        }

        /// <summary>
        /// Method.
        /// </summary>
        /// <param name="other">Ref struct param.</param>
        /// <returns>Ref struct return value.</returns>
        public RefStruct Method(RefStruct other)
        {
            return new RefStruct(other.Field, other.Property);
        }
    }
}
