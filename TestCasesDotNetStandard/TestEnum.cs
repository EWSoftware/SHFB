using System;
using System.Diagnostics.CodeAnalysis;

namespace DotNetStandardTestCases
{
    /// <summary>
    /// Test general enumerated type
    /// </summary>
    public enum TestGeneral
    {
        /// <summary>None</summary>
        /// <remarks>Some remarks about this item.</remarks>
        None,
        /// <summary>One</summary>
        /// <remarks>Test remarks.</remarks>
        One,
        /// <summary>Two</summary>
        Two,
        /// <summary>Three</summary>
        Three,
        /// <summary>Test obsolete attribute</summary>
        [Obsolete("This enum member is obsolete")]
        Obsolete,
        /// <summary>Test experimental attribute</summary>
        [Experimental("TEST0003")]
        Experimental,
        /// <summary>Test obsolete and experimental attribute</summary>
        [Obsolete("This experimental enum member is obsolete"), Experimental("TEST0003")]
        ExperimentalObsolete,
        /// <summary>Test preliminary</summary>
        /// <preliminary>Preliminary enum element</preliminary>
        Preliminary,
        /// <summary>Test preliminary and obsolete</summary>
        /// <preliminary />
        [Obsolete("Test preliminary with obsolete")]
        PreliminaryObsolete,
        /// <summary>Test preliminary, obsolete, and experimental</summary>
        /// <preliminary />
        [Obsolete("Test preliminary with obsolete"), Experimental("TEST0006")]
        PreliminaryObsoleteExperimental
    }

    /// <summary>
    /// Test serializable enumerated type
    /// </summary>
    [Serializable, Experimental("TEST0004")]
    public enum TestSerializable
    {
        /// <summary>None</summary>
        None,
        /// <summary>One</summary>
        One,
        /// <summary>Two</summary>
        Two,
        /// <summary>Three</summary>
        Three
    }

    /// <summary>
    /// Test obsolete experimental enumerated type
    /// </summary>
    [Serializable, Obsolete("This experimental enumerated type is obsolete"), Experimental("TEST0005")]
    public enum TestObsoleteExperimentalEnum
    {
        /// <summary>None</summary>
        None,
        /// <summary>One</summary>
        One,
        /// <summary>Two</summary>
        Two,
        /// <summary>Three</summary>
        Three
    }
}
