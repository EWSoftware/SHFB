using ColorizerLibrary;

namespace TestDoc
{
    /// <summary>
    /// Third-party library reference test.  With no reference link data, the type should still appear in the
    /// TOC and topic titles on overloaded methods.
    /// </summary>
    public class ReferenceTest
    {
        /// <summary>
        /// The parameter is a type in a referenced assembly, not documented, but should still show up in
        /// the TOC title and topic title.
        /// </summary>
        /// <param name="colorizer">Colorizer</param>
        /// <param name="language">Colorizer language</param>
        public ReferenceTest(CodeColorizer colorizer, string language)
        {
        }

        /// <summary>
        /// Overload
        /// </summary>
        /// <param name="language">Colorizer language</param>
        public ReferenceTest(string language)
        {
        }

        /// <summary>
        /// Method referencing type in third-party assembly
        /// </summary>
        /// <param name="colorizer">Colorizer</param>
        public void SetColorizer(CodeColorizer colorizer)
        {
        }

        /// <summary>
        /// Method referencing type in third-party assembly
        /// </summary>
        /// <param name="colorizer">Colorizer</param>
        /// <param name="language">Colorizer language</param>
        public void SetColorizer(CodeColorizer colorizer, string language)
        {
        }
    }
}
