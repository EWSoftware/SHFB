namespace TestDoc.CodeSnippetGrouping
{
    /// <summary>
    /// This class is used to test various combinations of code snippets
    /// </summary>
    /// <remarks>The old VS2010 presentation style that used the branding transformations had several bugs and
    /// undesirable behaviors when it came to grouping and displaying code snippets.  The examples in this class
    /// demonstrate the various test cases used to break the old branding transform grouping behavior.  The new
    /// grouping behavior in the SyntaxComponent and the updated presentation styles that support it will
    /// eventually handle all of these correctly.</remarks>
    public class TestSnippets
    {
        /// <summary>
        /// Standard languages grouped
        /// </summary>
        /// <remarks><para>The simplest case with one set of grouped snippets.</para>
        /// 
        /// <para>In the new grouping method, if Visual Basic Usage Syntax is included, it will be grouped in
        /// the syntax section and will cause VB code snippets to be displayed when selected since it shares the
        /// same style keyword as the Visual Basic Declaration Syntax generator.  Note that when a new topic is
        /// selected, the VB language selection will persist but the syntax section will show the declaration
        /// syntax again rather than the usage syntax.  No attempt will be made to keep that in sync since it is
        /// rarely used and I think most people know how to use the members and are more interested in the
        /// syntax which shows the return type, parameters, etc.</para>
        /// </remarks>
        /// <example>
        /// <para>Code snippets for the standard set of languages.</para>
        /// <code language="cs">
        /// /// C#
        /// </code>
        /// <code language="vb">
        /// ''' VB.NET
        /// </code>
        /// <code language="cpp">
        /// /// C++
        /// </code>
        /// <code language="fs">
        /// /// F#
        /// </code>
        /// </example>
        public void StandardLanguagesGrouped()
        {
        }

        /// <summary>
        /// Standard languages with intervening text
        /// </summary>
        /// <remarks>These should be rendered into standalone tabs that are always visible.</remarks>
        /// <example>
        /// <para>C# example:</para>
        /// <code language="cs">
        /// /// C#
        /// </code>
        /// VB.NET example:
        /// <code language="vb">
        /// ''' VB.NET
        /// </code>
        /// <para>C++ example:</para>
        /// <code language="cpp">
        /// /// C++
        /// </code>
        /// F# example:
        /// <code language="fs">
        /// /// F#
        /// </code>
        /// </example>
        public void StandardLanguagesInterveningText()
        {
        }

        /// <summary>
        /// Standard languages, multiple snippets
        /// </summary>
        /// <remarks>The desired behavior here is to group these into two sets of snippets.</remarks>
        /// <example>
        /// <para>Multiple code snippets for the standard set of languages.</para>
        /// <code language="cs">
        /// /// C# #1
        /// </code>
        /// <code language="vb">
        /// ''' VB.NET #1
        /// </code>
        /// <code language="cpp">
        /// /// C++ #1
        /// </code>
        /// <code language="fs">
        /// /// F# #1
        /// </code>
        /// <code language="cs">
        /// /// C# #2
        /// </code>
        /// <code language="vb">
        /// ''' VB.NET #2
        /// </code>
        /// <code language="cpp">
        /// /// C++ #2
        /// </code>
        /// <code language="fs">
        /// /// F# #2
        /// </code>
        /// </example>
        public void StandardLanguagesMultipleSnippets()
        {
        }

        /// <summary>
        /// Standard languages, multiple snippets grouped
        /// </summary>
        /// <remarks>The desired behavior here is to group these into two sets of snippets.</remarks>
        /// <example>
        /// <para>Multiple code snippets for the standard set of languages.</para>
        /// <code language="cs">
        /// /// C# #1
        /// </code>
        /// <code language="cs">
        /// /// C# #2
        /// </code>
        /// <code language="vb">
        /// ''' VB.NET #1
        /// </code>
        /// <code language="vb">
        /// ''' VB.NET #2
        /// </code>
        /// <code language="cpp">
        /// /// C++ #1
        /// </code>
        /// <code language="cpp">
        /// /// C++ #2
        /// </code>
        /// <code language="fs">
        /// /// F# #1
        /// </code>
        /// <code language="fs">
        /// /// F# #2
        /// </code>
        /// </example>
        public void StandardLanguagesMultipleSnippetsGrouped()
        {
        }

        /// <summary>
        /// Standard languages grouped, one with title
        /// </summary>
        /// <remarks>The C#, C++, and F# snippets will be grouped and the VB snippet will be split out into a
        /// standalone tab with title.</remarks>
        /// <example>
        /// <para>Code snippets for the standard set of languages.</para>
        /// <code language="cs">
        /// /// C#
        /// </code>
        /// <code language="vb" title="A VB.NET Example">
        /// ''' VB.NET
        /// </code>
        /// <code language="cpp">
        /// /// C++
        /// </code>
        /// <code language="fs">
        /// /// F#
        /// </code>
        /// </example>
        public void StandardLanguagesGroupedOneWithTitle()
        {
        }

        /// <summary>
        /// Standard languages grouped plus non-standard language
        /// </summary>
        /// <remarks>With the standard set of syntax filters, this will group those in the syntax filter and
        /// split the JavaScript snippet off into its own standalone tab.</remarks>
        /// <example>
        /// <para>Code snippets for the standard set of languages plus a non-standard one.</para>
        /// <code language="cs">
        /// /// C#
        /// </code>
        /// <code language="vb">
        /// ''' VB.NET
        /// </code>
        /// <code language="cpp">
        /// /// C++
        /// </code>
        /// <code language="fs">
        /// /// F#
        /// </code>
        /// <code language="javascript">
        /// /// JavaScript
        /// </code>
        /// </example>
        public void StandardLanguagesGroupedPlusNonStandard()
        {
        }

        /// <summary>
        /// Standard languages grouped plus non-standard language inside standard set
        /// </summary>
        /// <remarks>With the standard set of syntax filters, this will group those in the syntax filter and
        /// split the JavaScript snippet off into its own standalone tab.</remarks>
        /// <example>
        /// <para>Code snippets for the standard set of languages plus non-standard one inside the set</para>
        /// <code language="cs">
        /// /// C#
        /// </code>
        /// <code language="vb">
        /// ''' VB.NET
        /// </code>
        /// <code language="javascript">
        /// /// JavaScript
        /// </code>
        /// <code language="cpp">
        /// /// C++
        /// </code>
        /// <code language="fs">
        /// /// F#
        /// </code>
        /// </example>
        public void StandardLanguagesGroupedPlusNonStandardInside()
        {
        }

        /// <summary>
        /// Code blocks with and without titles
        /// </summary>
        /// <remarks>This will group the C# and C++ snippets and split the VB and F# snippets out into separate
        /// standalone tabs (VB with no title and F# with title).</remarks>
        /// <example>
        /// <code language="cs">
        /// /// C#
        /// </code>
        /// <code language="vb" title=" ">
        /// ''' VB.NET (suppressed title)
        /// </code>
        /// <code language="cpp">
        /// /// C++
        /// </code>
        /// <code language="fs" title="F# Example">
        /// /// F#
        /// </code>
        /// </example>
        public void CodeBlocksWithAndWithoutTitle()
        {
        }
    }
}
