namespace TestDoc.InterfaceTests
{
    /// <summary>
    /// SerializeIn interface
    /// </summary>
    public interface ISerializeIn
    {
        /// <summary>
        /// A SerializeIn function with specific archive and version number
        /// </summary>
        /// <param name="ar">The specific archive</param>
        /// <param name="version">The version number</param>
        void SerializeIn(short ar, uint version);
    }

    /// <summary>
    /// SerializeOut interface
    /// </summary>
    public interface ISerializeOut
    {
        /// <summary>
        /// A SerializeOut function with specific archive
        /// </summary>
        /// <param name="ar">The specific archive</param>
        void SerializeOut(short ar);

        /// <summary>
        /// Get the Lee-based type
        /// </summary>
        short TypeID { get; }
    }

    /// <summary>
    /// Serialize interface
    /// </summary>
    public interface ISerialize : ISerializeIn, ISerializeOut
    {

    }

    /// <summary>
    /// Lee object interface
    /// </summary>
    public interface ILeeObject : ISerialize
    {
        /// <summary>
        /// Database ID at the server side
        /// </summary>
        long Dbid { get; set; }
    }
}
