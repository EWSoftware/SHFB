using System;
using System.ComponentModel;
using System.Data.SqlClient;

namespace TestDoc.Overloads
{
    /// <summary>
    /// A test class with overloads
    /// </summary>
    public class OverloadClass
    {
        /// <summary>
        /// Tests the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        public void Test(string x)
        {
        }

        /// <summary>
        /// Tests the specified x.
        /// </summary>
        /// <param name="y">The y.</param>
        /// <obsolete>This method should not be used as it is obsolete.</obsolete>
        [Browsable(false), Obsolete("This method is now obsolete")]
        public void Test(int y)
        {
        }


        /// <summary>
        /// Method specific summary.
        /// </summary>
        /// <param name="value">A value</param>
        /// <overloads>
        /// <summary>
        /// Overload page summary.
        /// </summary>
        /// <seealso cref="HtmlTagCommentTest" />
        /// <remarks>
        /// Overload page remarks.
        /// </remarks>
        /// </overloads>
        /// <seealso cref="HtmlTagCommentTest" />
        /// <remarks>
        /// Method specific remarks.
        /// </remarks>
        public void OverloadedMethod(int value)
        {
        }

        /// <summary>
        /// Other overload summary
        /// </summary>
        /// <param name="value">A value</param>
        public void OverloadedMethod(string value)
        {
        }

        /// <summary>
        /// A test of HTML tags in comment text
        /// </summary>
        /// <remarks>Test some common HTML tags:<br/><br/>Two breaks above.
        /// <hr/>Horizontal rules<hr/>
        ///
        /// <p/>Entities: &#169; &lt; &gt; &amp; Space: !&#x20;!
        ///
        /// <p/>Paragraph 1.
        /// <p/>Paragraph 2.
        /// <p>Paragraph 3.</p>
        /// <p>Paragraph 4.</p>
        ///
        /// <h1>Heading 1</h1>
        /// <h2>Heading 2</h2>
        /// <h3>Heading 3</h3>
        /// <h4>Heading 4</h4>
        /// <h5>Heading 5</h5>
        /// <h6>Heading 6</h6>
        ///
        /// <p/><b>Bold</b> and <em>Emphasis</em>
        /// <p/><i>Italic</i> and <strong>Strong</strong>
        /// <p/><sub>Subscript</sub> and <sup>Superscript</sup>
        /// <p/><ins>Inserted</ins> and <del>Deleted</del>
        ///
        /// <pre>
        ///     Pre tag
        ///     text block
        /// </pre>
        ///
        /// <div style="background-color: yellow;">Yellow div</div>
        /// <p/><span style="background-color: cyan;">Cyan span</span>
        ///
        /// <p/>Here comes a long quotation:
        /// <blockquote>
        /// here is a long quotation
        /// here is a long quotation
        /// here is a long quotation
        /// here is a long quotation
        /// </blockquote>
        ///
        /// <p/>Abbreviation: <abbr title="United Nations">UN</abbr>
        /// <p/>Acronym: <acronym title="World Wide Web">WWW</acronym>
        ///
        /// <p/>Hyperlink: <a href="http://www.microsoft.com">Microsoft.com</a>
        ///
        /// <p/><table border="1">
        /// <tr>
        /// <th>Header 1</th>
        /// <th>Header 2</th>
        /// </tr>
        /// <tr>
        /// <td>row 1, cell 1</td>
        /// <td>row 1, cell 2</td>
        /// </tr>
        /// <tr>
        /// <td>row 2, cell 1</td>
        /// <td>row 2, cell 2</td>
        /// </tr>
        /// </table>
        ///
        /// <ul>
        /// <li>Coffee</li>
        /// <li>Milk</li>
        /// </ul>
        ///
        /// <ol>
        /// <li>Coffee</li>
        /// <li>Milk</li>
        /// </ol>
        ///
        /// <dl>
        /// <dt>Coffee</dt>
        /// <dd>Black hot drink</dd>
        /// <dt>Milk</dt>
        /// <dd>White cold drink</dd>
        /// </dl>
        ///
        /// <p/><img src="../Images/AddContent.gif" alt="Additional Content"/>
        /// </remarks>
        public void HtmlTagCommentTest()
        {
        }

        /// <summary>
        /// Executes a <see cref="SqlCommand" /> with the specified
        /// <paramref name="storedProcName" /> as a stored procedure
        /// initialized for updating the values of the specified
        /// <paramref name="row" />.  Test overloads link (see also tested below):
        /// <see cref="O:TestDoc.Overloads.OverloadClass.Test"/>
        /// </summary>
        /// <param name="storedProcName">The stored procedure</param>
        /// <param name="row">The row</param>
        /// <seealso cref="O:TestDoc.Overloads.OverloadClass.Test"/>
        public void CallStoredProcedure(string storedProcName, int row)
        {
        }
    }
}
