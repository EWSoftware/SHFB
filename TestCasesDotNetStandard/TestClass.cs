namespace DotNetStandardTestCases
{
    /// <summary>
    /// Test class for extension method tests
    /// </summary>
    public class TestClass
    {
        /// <summary>
        /// X value
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Init only setter test
        /// </summary>
        public int XInitOnly { get; init; }

        /// <summary>
        /// Y value
        /// </summary>
        public int Y { get; set; }

        // These non-nested private protected members do not show up

        /// <summary>**DO NOT DOCUMENT**</summary>
        private protected int Field;

        /// <summary>**DO NOT DOCUMENT**</summary>
        private protected int Property { get; set; }

        /// <summary>**DO NOT DOCUMENT**</summary>
        private protected void Method() { this.Event?.Invoke(42); }

        /// <summary>**DO NOT DOCUMENT**</summary>
        private protected event Delegate Event;

        // These nested items are showing up when they should not

        /// <summary>**DO NOT DOCUMENT**</summary>
        private protected class DocTest { }

        /// <summary>**DO NOT DOCUMENT**</summary>
        private protected readonly struct MemberStruct { }

        /// <summary>**DO NOT DOCUMENT**</summary>
        private protected sealed class MemberClass { }

        /// <summary>**DO NOT DOCUMENT**</summary>
        private protected delegate void Delegate(int n);
    }
}
