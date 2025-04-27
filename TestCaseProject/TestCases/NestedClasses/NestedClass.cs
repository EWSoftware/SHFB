namespace TestDoc.NestedClasses
{
    /// <summary>
    /// Test
    /// </summary>
    public class Foo
    {
        /// <summary>
        /// Test
        /// </summary>
        public enum BarEnum
        {
            /// <summary>
            /// Ick
            /// </summary>
            Ick
        }

        /// <summary>
        /// Test
        /// </summary>
        public BarEnum Bar { get; set; }
    }

    /// <summary>
    /// This is the parent class
    /// </summary>
    public class ParentClass
    {
        #region Nested class
        /// <summary>
        /// This is the nested class
        /// </summary>
        public class NestedClass
        {
            #region Innermost nested class
            /// <summary>
            /// This is the innermost nested class
            /// </summary>
            public class InnermostClass
            {
                /// <summary>
                /// Constructor
                /// </summary>
                public InnermostClass()
                {
                }

                /// <summary>
                /// A method on the innermost class
                /// </summary>
                public void InnermostClassMethod()
                {
                }
            }
            #endregion

            /// <summary>
            /// Constructor
            /// </summary>
            public NestedClass()
            {
            }

            /// <summary>
            /// A property on the nested class
            /// </summary>
            public InnermostClass Value { get; set; }

            /// <summary>
            /// A method on the nested class
            /// </summary>
            /// <returns>Null</returns>
            public InnermostClass NestedClassMethod()
            {
                return null;
            }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ParentClass()
        {
        }

        /// <summary>
        /// A property on the parent class
        /// </summary>
        public NestedClass Value { get; set; }

        /// <summary>
        /// A method on the parent class
        /// </summary>
        /// <returns>Null</returns>
        /// <remarks>If <see langword="true"/>, do something.</remarks>
        public NestedClass ParentClassMethod()
        {
            return null;
        }
    }

    /// <summary>
    /// Another parent class
    /// </summary>
    public class AnotherParentClass
    {
        /// <summary>
        /// A nested class with the same name as a non-nested class
        /// </summary>
        public class TestClass2
        {
            /// <summary>
            /// Nested version's constructor
            /// </summary>
            public TestClass2()
            {
            }
        }
    }

    /// <summary>
    /// A non-nested class with the same name as a nested class
    /// </summary>
    public class TestClass2
    {
        /// <summary>
        /// Non-nested version's constructor
        /// </summary>
        public TestClass2()
        {
        }
    }


    /// <summary>
    /// Non-static parent class
    /// </summary>
    public class NonStaticParentClass
    {
        /// <summary>
        /// A nested static class
        /// </summary>
        public static class NestedStaticClass
        {
            /// <summary>
            /// Test method 1
            /// </summary>
            /// <returns>A string</returns>
            public static string TestMethod1()
            {
                return "Test";
            }

            /// <summary>
            /// Test method 2
            /// </summary>
            /// <returns>A integer</returns>
            public static int TestMethod2()
            {
                return 0;
            }
        }
    }

    // From SandcastleStyles:
    //
    // We have a situation with nested types where a summary is getting pulled
    // from a higher class even though we have a summary for that class. We are
    // using the VS2005 style. Using the example below, if you look at the
    // CoordinateSystem.ProjectedCoordinateSystems.World Members page, you get
    // the summary information from the CoordinateSystem.ProjectedCoordinateSystems
    // class, not the World class as expected.
    //
    // Result: All "Members" pages for nested classes pull the summary
    //         from the parent.  That shouldn't happen.

    /// <summary>The CoordinateSystem Class</summary>
    public sealed class CoordinateSystem
    {
        /// <summary>The ProjectedCoordinateSystems class</summary>
        public static class ProjectedCoordinateSystems
        {
            /// <summary>The World class</summary>
            public static class World
            {
                /// <summary>The Mercator coordinate system</summary>
                public static TestDoc.NestedClasses.CoordinateSystem Mercator => throw NIE;

                /// <summary>The Robinson coordinate system</summary>
                public static TestDoc.NestedClasses.CoordinateSystem Robinson => throw NIE;

                /// <summary>The Mollweide coordinate system</summary>
                public static TestDoc.NestedClasses.CoordinateSystem Mollweide => throw NIE;

                #region NIE
                private static global::System.Exception NIE => new global::System.NotImplementedException();

                #endregion
            }
        }
    }
}
