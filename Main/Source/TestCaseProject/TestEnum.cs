using System;

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
        Three
    }

    /// <summary>
    /// Test serializable enumerated type
    /// </summary>
    [Serializable]
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
        /// <summary>Three</summary>
        Four = 0x4
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
        /// <summary>Three</summary>
        Four = 0x4
    }
}
