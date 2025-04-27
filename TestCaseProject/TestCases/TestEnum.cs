using System;
using System.Diagnostics.CodeAnalysis;

namespace TestDoc
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

    /// <summary>
    /// Test flags enumerated type
    /// </summary>
    [Flags]
    public enum TestFlags
    {
        /// <summary>None</summary>
        None = 0x0,
        /// <summary>One</summary>
        One = 0x01,
        /// <summary>Two</summary>
        Two = 0x2,
        /// <summary>Four</summary>
        Four = 0x4,
        /// <summary>Large flags value</summary>
        LargeFlags = 0x10800520
    }

    /// <summary>
    /// Test flags, serializable enumerated type
    /// </summary>
    [Flags, Serializable]
    public enum TestFlagsSerializable
    {
        /// <summary>None</summary>
        None = 0x0,
        /// <summary>One</summary>
        One = 0x01,
        /// <summary>Two</summary>
        Two = 0x2,
        /// <summary>Four</summary>
        Four = 0x4
    }

    /// <summary>
    /// Test flags with large numbers for testing the separator formatting options
    /// </summary>
    [Serializable]
    public enum TestEnumFormatting
    {
        /// <summary>One thousand</summary>
        OneThousand = 1000,
        /// <summary>Two thousand</summary>
        TwoThousand = 2000,
        /// <summary>Ten thousand</summary>
        TenThousand = 10000,
        /// <summary>One hundred thousand</summary>
        OneHundredThousand = 100000,
    }


    /// <summary>
    /// Test full text index with words containing digits
    /// </summary>
    [Serializable]
    public enum Foo1
    {
        /// <summary>One thousand</summary>
        OneThousand = 1000,
        /// <summary>Two thousand</summary>
        TwoThousand = 2000,
        /// <summary>Ten thousand</summary>
        TenThousand = 10000,
        /// <summary>One hundred thousand</summary>
        OneHundredThousand = 100000,
    }

    /// <summary>
    /// Test full text index with words containing digits
    /// </summary>
    [Serializable]
    public enum Foo2
    {
        /// <summary>One thousand</summary>
        OneThousand = 1000,
        /// <summary>Two thousand</summary>
        TwoThousand = 2000,
        /// <summary>Ten thousand</summary>
        TenThousand = 10000,
        /// <summary>One hundred thousand</summary>
        OneHundredThousand = 100000,
    }
}
