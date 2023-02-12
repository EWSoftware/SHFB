// Ignore Spelling: cref seealso

// Ensure empty types are handled
namespace TestDoc.EmptyTypes
{
    /// <summary>
    /// Empty class
    /// </summary>
    public class EmptyTypes
    {
    }

    /// <summary>
    /// Empty structure
    /// </summary>
    public struct EmptyStruct
    {
    }

    /// <summary>
    /// Empty interface
    /// </summary>
    public interface IEmpyInterface
    {
    }

    /// <summary>
    /// Empty enumeration
    /// </summary>
    public enum EmptyEnum
    {
    }

#pragma warning disable CS1584, CS1658
    /// <summary>
    /// Empty cref tests
    /// </summary>
    public class EmptyCrefAttributes
    {
        /// <summary>
        /// <para>Test empty/missing cref attributes.</para>
        /// <para>Missing cref on see: <see/></para>
        /// <para>Empty cref on see: <see cref=""/></para>
        /// <para>Missing cref on seealso: <see/></para>
        /// <para>Empty cref on seealso: <seealso cref=""/></para>
        /// </summary>
        /// <conceptualLink>Missing target on conceptualLink</conceptualLink>
        /// <conceptualLink target="">Empty target on conceptualLink</conceptualLink>
        /// <exception>Missing cref on exception</exception>
        /// <exception cref="">Empty cref on exception</exception>
        /// <event>Missing cref on event</event>
        /// <event cref="">Empty cref on event</event>
        /// <permission>Missing cref on permission</permission>
        /// <permission cref="">Empty cref on permission</permission>
        public void EmptyCrefTests()
        {
        }
    }
#pragma warning restore CS1584, CS1658
}
