using System;

namespace TestDoc.DocumentationInheritance
{
    #region Various documentation inheritance examples

    #region Base class
    //=========================================================================

    /// <summary>
    /// A base class from which to inherit documentation
    /// </summary>
    /// <remarks>
    /// <para>These remarks are for the base class.</para>
    ///
    /// <para>This information applies to all classes that derive from
    /// <see cref="BaseInheritDoc"/>:
    /// <list type="bullet">
    /// <item><description>Point #1.</description></item>
    /// <item><description>Point #2.</description></item>
    /// <item><description>Point #3.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class BaseInheritDoc
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BaseInheritDoc()
        {
        }

        /// <summary>
        /// The ToString implementation for BaseInheritDoc
        /// </summary>
        /// <returns>A string representing the object</returns>
        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// Summary for the method with an example
        /// </summary>
        /// <returns>True all the time</returns>
        /// <example>
        /// This example is from the base class
        /// <code>
        /// // 'x' is always true
        /// bool x = instance.MethodWithExample();
        /// </code>
        /// </example>
        public virtual bool MethodWithExample()
        {
            return true;
        }

        /// <summary>
        /// The method in the base class has lots of comments.
        /// </summary>
        /// <remarks>Remarks for the base class</remarks>
        /// <param name="x">The parameter</param>
        /// <exception cref="ArgumentException">Thrown if x is zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if x is
        /// less than zero.</exception>
        /// <example>
        /// <code>
        /// /// Example goes here
        /// </code>
        /// </example>
        /// <seealso cref="ToString" />
        /// <seealso cref="MethodWithExample"/>
        public virtual void MethodWithLotsOfComments(int x)
        {
        }

        /// <summary>
        /// A method with two examples
        /// </summary>
        /// <example>
        /// <span id="Example 1">
        /// This is example #1:
        /// <code>
        /// // Example #1
        /// </code>
        /// </span>
        /// <span id="Example 2">
        /// This is example #2:
        /// <code>
        /// // Example #2
        /// </code>
        /// </span>
        /// </example>
        protected virtual void MethodWithTwoExamples()
        {
            // By using a <span> with an ID, we can group comments for
            // selection by an override in a derived class.
        }
    }
    #endregion

    #region Derived class
    //=========================================================================

    /// <summary>
    /// This is a derived class with inherited documentation.
    /// </summary>
    /// <remarks>This will inherit just the last &lt;para&gt; tag from
    /// the base class's &lt;remarks&gt; tag:
    /// <inheritdoc select="para[last()]" />
    /// </remarks>
    public class DerivedClassWithInheritedDocs : BaseInheritDoc
    {
        // Note in the <remarks> tag above that we can inherit specific
        // parts of a comment tag's text by using an XPath query.  This
        // can allow you to merge comments from various sources into one
        // set of comments in a given tag.  An implied filter that limits
        // the selection to the <remarks> tag is added automatically.  If
        // the select attribute were omitted, the entire set of remarks
        // from the base class would be inherited.

        /// <inheritdoc cref="Object.ToString" />
        public override string ToString()
        {
            // This override ignores the base class comments and uses a
            // cref attribute to obtain the comments from
            // System.Object.ToString instead.
            return base.ToString();
        }

        /// <summary>
        /// This overloaded method does something
        /// </summary>
        /// <param name="p1">The string parameter</param>
        /// <overloads>
        /// <summary>There are two overloads for this method.</summary>
        /// <remarks>These remarks are from the overloads tag on the
        /// first version.</remarks>
        /// </overloads>
        public void OverloadedMethod(string p1)
        {
        }

        #pragma warning disable 1573
        /// <inheritdoc cref="OverloadedMethod(string)" />
        /// <param name="p2">The second string parameter</param>
        public void OverloadedMethod(string p1, string p2)
        {
            // Inherit documentation from the first overload and add
            // comments for the second parameter.

            // Note that because we supplied comments for one parameter
            // but not the other, the compiler will complain.  However,
            // we can shut it up by using a "#pragma warning" directive as
            // shown.
        }

        /// <inheritdoc cref="OverloadedMethod(string)"
        ///     select="param|overloads/*" />
        /// <param name="x">An integer parameter</param>
        public void OverloadedMethod(string p1, int x)
        {
            // This example inherits the comments from the <param> tag on
            // the first version, the content of the <overloads> tag on the
            // first version, and adds comments for the second parameter.
        }
        #pragma warning restore 1573

        /// <summary>
        /// An override of the method with an example
        /// </summary>
        /// <returns>Always returns false</returns>
        /// <example>
        /// <inheritdoc />
        /// <p/>This example applies to the derived class:
        /// <code>
        /// if(derivedInstance.MethodWithExample())
        ///     Console.WriteLine("This is never reached");
        /// </code>
        /// </example>
        public override bool MethodWithExample()
        {
            // The <example> tag inherits the example from the base class
            // and adds a new example of its own.  Again, an implied filter
            // limits the nested tag to inheriting comments from the
            // <example> tag in the base class's comments.

            return false;
        }

        /// <inheritdoc select="summary|remarks|param" />
        public override void MethodWithLotsOfComments(int x)
        {
            // For this override, we don't want all the comments, just those
            // from the <summary>, <remarks>, and <param> tags.
        }

        /// <summary>
        /// This only includes one of the examples
        /// </summary>
        /// <example>
        /// <inheritdoc select="span[@id='Example 2']" />
        /// </example>
        protected override void MethodWithTwoExamples()
        {
            // Here, we use a filter to select a group of comments in
            // a <span> tag from the base member's <example> tag.
        }

        /// <summary>
        /// This uses a shared example from a base member that is not
        /// public and this doesn't override.
        /// </summary>
        /// <example>
        /// <inheritdoc cref="MethodWithTwoExamples"
        ///     select="span[@id='Example 2']" />
        /// </example>
        public void MethodUsingSharedExample()
        {
            // This method uses a cref attribute and a select tag to inherit
            // a specific example from a member to which it has no relation.
        }
    }
    #endregion

    #endregion
}
