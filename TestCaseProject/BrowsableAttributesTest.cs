using System.ComponentModel;

namespace TestDoc.BrowsableAttributes
{
    /// <summary>
    /// The classes in this namespace test the <c>EditorBrowsableNever</c> and <c>NonBrowsable</c> settings
    /// </summary>
    /// <remarks>Only types and members explicitly marked with the attributes are excluded.  Types derived from
    /// hidden types and members that override hidden members will be visible unless also explicitly marked.
    /// This mimics the behavior of the Object Browser.</remarks>
    [System.Runtime.CompilerServices.CompilerGenerated()]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// Browsable attributes test
    /// </summary>
    /// <remarks>This class is visible but its members are not based on the related settings</remarks>
    public class EditorBrowsableTest
    {
        /// <summary>
        /// Marked as never editor browsable
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string HiddenProperty { get; set; }

        /// <summary>
        /// Marked as non-browsable
        /// </summary>
        /// <returns>A string</returns>
        [Browsable(false)]
        public virtual string HiddenMethod()
        {
            return null;
        }
    }

    /// <summary>
    /// This type is marked as never editor browsable
    /// </summary>
    /// <remarks>This will be visible unless editor browsable types are excluded</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EditorBrowsableDerived : EditorBrowsableTest
    {
        // HiddenProperty will not be visible

        /// <summary>
        /// This will be visible since it overrides the hidden member but isn't marked itself
        /// </summary>
        /// <returns>A string</returns>
        public override string HiddenMethod()
        {
            return base.HiddenMethod();
        }
    }

    /// <summary>
    /// This type is marked as non-browsable
    /// </summary>
    /// <remarks>This will be visible unless non-browsable types are excluded.  It derives from the type
    /// hidden with the <c>EditorBrowsableAttribute</c> but isn't marked itself.</remarks>
    [Browsable(false)]
    public class EditorBrowsableDerivedAgain : EditorBrowsableDerived
    {
        // HiddenProperty will not be visible

        /// <summary>
        /// This will be visible since it overrides the hidden member but isn't marked itself
        /// </summary>
        /// <returns>A string</returns>
        public override string HiddenMethod()
        {
            return null;
        }
    }

    /// <summary>
    /// And another browsable attributes test
    /// </summary>
    /// <remarks>This will be visible.  It derives from the type hidden with the <c>BrowsableAttribute</c> but
    /// isn't marked itself.</remarks>
    public class BrowsableDerived: EditorBrowsableDerivedAgain
    {
        // HiddenProperty will not be visible
        // HiddenMethod will be visible since it wasn't hidden in the parent type unless browsable types are
        // excluded.  It won't appear then since the parent type is hidden.
    }
}
