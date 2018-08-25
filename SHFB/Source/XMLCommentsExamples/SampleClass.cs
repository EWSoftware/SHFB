//===============================================================================================================
// System  : Sandcastle Tools - XML Comments Example
// File    : SampleClass.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/06/2015
// Note    : Copyright 2012-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This class is used to demonstrate the various XML comments elements.  It serves no useful purpose.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/05/2012  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: href sc langword

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Permissions;

namespace XMLCommentsExamples
{
    /// <summary>
    /// This class is used to demonstrate the various XML comments elements.  It serves no useful purpose.
    /// </summary>
    /// <threadsafety static="true" instance="false" />
    public class SampleClass
    {
        #region value Example

        /// <summary>
        /// This is used to get or set the sample number
        /// </summary>
        /// <value>The value can be any valid integer</value>
        /// <conceptualLink target="f512d714-e100-4296-916c-99a46e572e9d" />
        public int SampleNumber { get; set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sampleNumber">A sample number</param>
        private SampleClass(int sampleNumber)
        {
            this.SampleNumber = sampleNumber;
        }
        #endregion

        #region c/returns Example

        /// <summary>
        /// The <c>Increment</c> method increments the stored number by one
        /// </summary>
        /// <returns>The new sample number after being incremented</returns>
        /// <conceptualLink target="d0db2290-08bb-40cc-9797-23a342b96564" />
        /// <conceptualLink target="fa97c10b-a683-4d10-a01c-5787dbdd42d3" />
        public int Increment()
        {
            return ++this.SampleNumber;
        }
        #endregion

        #region conceptualLink Examples

        /// <summary>
        /// See the <conceptualLink target="db2703b4-12bc-4cf5-8642-544b41002809" /> topic
        /// for more information.
        /// </summary>
        /// <remarks><c>conceptualLink</c> is classed as one of the
        /// <conceptualLink target="9341fdc8-1571-405c-8e61-6a6b9b601b46">miscellaneous
        /// elements</conceptualLink>.</remarks>
        /// <conceptualLink target="db2703b4-12bc-4cf5-8642-544b41002809" />
        /// <conceptualLink target="16cdb957-a35b-4c17-bf5e-ea511b0218e3">See Also information</conceptualLink>
        public void ConceptualLinkExample()
        {
        }
        #endregion

        #region event Example

        /// <summary>
        /// This performs an action
        /// </summary>
        /// <event cref="SomethingHappened">This event is raised to let the user know
        /// something happened.</event>
        /// <conceptualLink target="81bf7ad3-45dc-452f-90d5-87ce2494a182" />
        public void PerformAnAction()
        {
            this.OnSomethingHappened();
        }
        #endregion

        #region example/code Example

        /// <summary>
        /// This returns a random number
        /// </summary>
        /// <returns>A random number using <see cref="SampleNumber"/> as the seed.</returns>
        /// <example>
        /// The following example demonstrates the use of this method.
        /// 
        /// <code language="cs">
        /// // Get a new random number
        /// SampleClass sc = new SampleClass(10);
        /// 
        /// int random = sc.GetRandomNumber();
        ///
        /// Console.WriteLine("Random value: {0}", random);
        /// </code>
        /// 
        /// <code language="vb">
        /// ' Get a new random number
        /// Dim sc As SampleClass = New SampleClass(10)
        /// 
        /// Dim random As Integer = sc.GetRandomNumber()
        ///
        /// Console.WriteLine("Random value: {0}", random)
        /// </code>
        /// </example>
        /// <conceptualLink target="1abd1992-e3d0-45b4-b43d-91fcfc5e5574" />
        /// <conceptualLink target="1bef716a-235b-4d96-a23e-f43b8dcf9abd" />
        public int GetRandomNumber()
        {
            return new Random(this.SampleNumber).Next();
        }
        #endregion

        #region exception Example

        /// <summary>
        /// This method processes text
        /// </summary>
        /// <param name="text">The text to process</param>
        /// <exception cref="ArgumentNullException">This is thrown if the <paramref name="text"/>
        /// parameter is null.</exception>
        /// <exception cref="ArgumentException">This is thrown if the <paramref name="text"/>
        /// parameter is an empty string.</exception>
        /// <exception cref="InvalidOperationException">This is thrown because the method
        /// is not currently implemented.</exception>
        /// <conceptualLink target="bbd1e65d-c87c-4b46-9a1a-259d3c5cd936" />
        public void ProcessText(string text)
        {
            if(text == null)
                throw new ArgumentNullException("text cannot be null");

            if(text.Trim().Length == 0)
                throw new ArgumentException("text cannot be an empty string");

            throw new InvalidOperationException();
        }
        #endregion

        #region filterpriority Example

        /// <summary>
        /// Filter priority example
        /// </summary>
        /// <remarks>This element is rarely used as <see cref="EditorBrowsableAttribute"/>
        /// does the same thing and is effective across all languages.</remarks>
        /// <filterpriority>1</filterpriority>
        /// <conceptualLink target="0522f3bf-0a57-4d70-a2a5-d64a14c5bcc9" />
        public void FilterPriorityExample()
        {
        }
        #endregion

        #region include Examples

		/// <include file="IncludeComments.xml" path="Comments/IncludeAllExample/*" />
		public void IncludeAllExample()
		{
            // This pulls in all comments for the method.
        }

		/// <summary>
		/// In this example, the <c>include</c> element is used to pull in sections of
        /// the comments.
		/// </summary>
		/// <remarks>
        /// <para>As shown in these examples (see source code).  Single and double quotes
        /// can be used on the XPath query when nested quotes are required.  The key is to
        /// be consistent.</para>
        /// 
        /// <para>This pulls in a single paragraph element.</para>
        /// 
        /// <include file="IncludeComments.xml" path="Comments/Example[@id='paraExample']/para" />
        /// 
        /// <para>This pulls in a all the content of the element.</para>
        /// 
        /// <include file="IncludeComments.xml" path='Comments/Example[@id="AnotherExample"]/*' />
        /// </remarks>
        /// <conceptualLink target="3de64a85-dafb-4a01-85dc-7f69a76ef790" />
        /// <seealso cref="IncludeAllExample"/>
		public void IncludeSectionsExample()
		{
		}
        #endregion

        #region list Examples

        /// <summary>
        /// This method shows various examples of the <c>list</c> XML comments element.
        /// </summary>
        /// <remarks>
        /// <para>A simple bulleted list.  The <c>term</c> and <c>description</c>
        /// elements are optional for simple string descriptions.</para>
        /// 
        /// <list type="bullet">
        ///   <item>First item</item>
        ///   <item>Second item</item>
        ///   <item>Third item</item>
        /// </list>
        ///
        /// <para>Bullet list with terms and definitions.  The term is highlighted and
        /// separated from the definition with a dash.</para>
        /// 
        /// <list type="bullet">
        ///   <item>
        ///     <term>maxLen</term>
        ///     <description>field must contain no more than the specified number
        /// of characters</description>
        ///   </item>
        ///   <item>
        ///     <term>minLen</term>
        ///     <description>field must contain at least the specified number
        /// of characters</description>
        ///   </item>
        ///   <item>
        ///     <term>maxVal</term>
        ///     <description>field must contain a number that is no larger than the
        /// specified value</description>
        ///   </item>
        ///   <item>
        ///     <term>minVal</term>
        ///     <description>field must contain a number that is no smaller than the
        /// specified value</description>
        ///   </item>
        ///   <item>
        ///     <term>pattern</term>
        ///     <description>field must match the specified regular expression
        /// </description>
        ///   </item>
        /// </list>
        /// 
        /// <para>A simple numbered list.  The <c>term</c> and <c>description</c>
        /// elements are optional for simple string descriptions.</para>
        /// 
        /// <list type="number">
        ///   <item>First item</item>
        ///   <item>Second item</item>
        ///   <item>Third item</item>
        /// </list>
        /// 
        /// <para>This next numbered list uses the optional <c>start</c> attribute to
        /// continue numbering where the last one left off.</para>
        /// 
        /// <list type="number" start="4">
        ///   <item>Fourth item</item>
        ///   <item>Fifth item</item>
        ///   <item>Sixth item</item>
        /// </list>
        /// 
        /// <para>Numbered list with terms and definitions.</para>
        /// 
        /// <list type="number">
        ///   <item>
        ///     <term>maxLen</term>
        ///     <description>field must contain no more than the specified number
        /// of characters</description>
        ///   </item>
        ///   <item>
        ///     <term>minLen</term>
        ///     <description>field must contain at least the specified number
        /// of characters</description>
        ///   </item>
        ///   <item>
        ///     <term>maxVal</term>
        ///     <description>field must contain a number that is no larger than the
        /// specified value</description>
        ///   </item>
        ///   <item>
        ///     <term>minVal</term>
        ///     <description>field must contain a number that is no smaller than the
        /// specified value</description>
        ///   </item>
        ///   <item>
        ///     <term>pattern</term>
        ///     <description>field must match the specified regular expression
        /// </description>
        ///   </item>
        /// </list>
        /// 
        /// <para>Definition list.</para>
        /// 
        /// <list type="definition">
        ///   <item>
        ///     <term>maxLen</term>
        ///     <description>field must contain no more than the specified number
        /// of characters</description>
        ///   </item>
        ///   <item>
        ///     <term>minLen</term>
        ///     <description>field must contain at least the specified number
        /// of characters</description>
        ///   </item>
        ///   <item>
        ///     <term>maxVal</term>
        ///     <description>field must contain a number that is no larger than the
        /// specified value</description>
        ///   </item>
        ///   <item>
        ///     <term>minVal</term>
        ///     <description>field must contain a number that is no smaller than the
        /// specified value</description>
        ///   </item>
        ///   <item>
        ///     <term>pattern</term>
        ///     <description>field must match the specified regular expression
        /// </description>
        ///   </item>
        /// </list>
        ///
        /// <para>Two-column table list with terms and definitions.</para>
        /// 
        /// <list type="table">
        ///   <listheader>
        ///     <term>Item</term>
        ///     <description>Description</description>
        ///   </listheader>
        ///   <item>
        ///     <term>maxLen</term>
        ///     <description>field must contain no more than the specified number
        /// of characters</description>
        ///   </item>
        ///   <item>
        ///     <term>minLen</term>
        ///     <description>field must contain at least the specified number
        /// of characters</description>
        ///   </item>
        ///   <item>
        ///     <term>maxVal</term>
        ///     <description>field must contain a number that is no larger than the
        /// specified value</description>
        ///   </item>
        ///   <item>
        ///     <term>minVal</term>
        ///     <description>field must contain a number that is no smaller than the
        /// specified value</description>
        ///   </item>
        ///   <item>
        ///     <term>pattern</term>
        ///     <description>field must match the specified regular expression
        /// </description>
        ///   </item>
        /// </list>
        /// 
        /// <para>A table with multiple columns.  <c>term</c> or <c>description</c>
        /// can be used to create the columns in each row.</para>
        /// 
        /// <list type="table">
        ///   <listheader>
        ///     <term>Column 1</term>
        ///     <term>Column 2</term>
        ///     <term>Column 3</term>
        ///     <term>Column 4</term>
        ///   </listheader>
        ///   <item>
        ///     <term>R1, C1</term>
        ///     <term>R1, C2</term>
        ///     <term>R1, C3</term>
        ///     <term>R1, C4</term>
        ///   </item>
        ///   <item>
        ///     <description>R2, C1</description>
        ///     <description>R2, C2</description>
        ///     <description>R2, C3</description>
        ///     <description>R2, C4</description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <conceptualLink target="e433d846-db15-4ac8-a5f5-f3428609ae6c" />
        public void VariousListExamples()
        {
        }
        #endregion

        #region note Examples

        /// <summary>
        /// This shows the result of the various <c>note</c> types.
        /// </summary>
        /// <remarks>
        /// <para>These are various examples of the different note types.</para>
        /// 
        /// <note>
        /// This example demonstrates the handling of a <c>note</c> element with no
        /// defined type.  It defaults to the "note" style.
        /// </note>
        /// 
        /// <note type="tip">
        /// Always document your code to help others understand how it is used.
        /// </note>
        /// 
        /// <note type="implement">
        /// Override this method in a derived class to do something useful
        /// </note>
        /// 
        /// <note type="caller">
        /// Calling this implementation will have no effect at all
        /// </note>
        /// 
        /// <note type="inherit">
        /// Types inheriting this base method will have no use for it as it does nothing
        /// </note>
        /// 
        /// <note type="caution">
        /// Use of this method is not recommended.
        /// </note>
        /// 
        /// <note type="warning">
        /// XML is case-sensitive so the note type must be entered as shown in order for
        /// it to be interpreted correctly.
        /// </note>
        /// 
        /// <note type="important">
        /// Calling this method excessively will only slow down your application.
        /// </note>
        /// 
        /// <note type="security">
        /// It is always safe to call this method.
        /// </note>
        /// 
        /// <note type="security note">
        /// This method requires no special privileges
        /// </note>
        /// 
        /// <note type="C#">
        /// Use parenthesis when calling this method in C#.
        /// </note>
        /// 
        /// <note type="VB">
        /// Parenthesis are not required when calling this method in Visual Basic.
        /// </note>
        /// 
        /// <note type="C++">
        /// Use parenthesis when calling this method in C++.
        /// </note>
        /// 
        /// <note type="J#">
        /// Use parenthesis when calling this method in J#.
        /// </note>
        /// 
        /// <para>See the <conceptualLink target="4302a60f-e4f4-4b8d-a451-5f453c4ebd46" />
        /// topic for a full list of all possible note types.</para>
        /// </remarks>
        public virtual void VariousNoteExamples()
        {
        }
        #endregion

        #region overloads Examples

        // Simple overloads form

        /// <summary>
        /// This is used to sum an enumerable list of values
        /// </summary>
        /// <param name="values">The values to sum</param>
        /// <returns>The sum of the values</returns>
        /// <overloads>There are two overloads for this method</overloads>
        /// <conceptualLink target="5b11b235-2b6c-4dfc-86b0-2e7dd98f2716" />
        public int SumValues(IEnumerable<int> values)
        {
            return values.Sum(v => v);
        }

        /// <summary>
        /// This is used to sum two enumerable list of values
        /// </summary>
        /// <param name="firstValues">The first set of values to sum</param>
        /// <param name="secondValues">The second set of values to sum</param>
        /// <returns>The sum of the values from both enumerable lists</returns>
        /// <conceptualLink target="5b11b235-2b6c-4dfc-86b0-2e7dd98f2716" />
        public int SumValues(IEnumerable<int> firstValues, IEnumerable<int> secondValues)
        {
            return firstValues.Sum(v => v) + secondValues.Sum(v => v);
        }

        // Expanded overloads form

        /// <summary>
        /// This is used to average an enumerable list of values
        /// </summary>
        /// <param name="values">The values to average</param>
        /// <returns>The average of the values</returns>
        /// <overloads>
        /// <summary>
        /// These methods are used to compute the average of enumerable lists of integers
        /// </summary>
        /// <remarks>
        /// These methods serve no other purpose than to demonstrate the use of the
        /// <c>overloads</c> XML comments element.
        /// </remarks>
        /// <example>
        /// <code language="cs">
        /// SampleClass sc = new SampleClass(0);
        /// 
        /// Console.WriteLine("Average: {0}", sc.Average(new[] { 1, 2, 3, 4 }));
        /// Console.WriteLine("Average: {0}", sc.Average(new[] { 1, 2, 3, 4 },
        ///     new[] { 10, 20, 30, 40}));
        /// </code>
        /// </example>
        /// </overloads>
        /// <conceptualLink target="5b11b235-2b6c-4dfc-86b0-2e7dd98f2716" />
        public double AverageValues(IEnumerable<double> values)
        {
            return values.Average(v => v);
        }

        /// <summary>
        /// This is used to get the average of two enumerable list of values
        /// </summary>
        /// <param name="firstValues">The first set of values to average</param>
        /// <param name="secondValues">The second set of values to average</param>
        /// <returns>The average of the values from both enumerable lists</returns>
        /// <conceptualLink target="5b11b235-2b6c-4dfc-86b0-2e7dd98f2716" />
        public double AverageValues(IEnumerable<double> firstValues,
          IEnumerable<double> secondValues)
        {
            return firstValues.Concat(secondValues).Average(v => v);
        }
        #endregion

        #region para Example

        /// <summary>
        /// A simple demonstration of the <c>para</c> element.
        /// </summary>
        /// <remarks>The <c>para</c> element on the first block of text is
        /// optional.
        /// 
        /// <para>Subsequent blocks of text must be wrapped in it to create new
        /// paragraphs.</para>
        /// 
        /// <para>Paragraph 1.</para>
        /// <para>Paragraph 2.</para>
        /// <para />
		/// <para></para>
		/// <para>Self-closing and empty paragraphs are ignored unless they create
        /// an initial paragraph break.</para>
	    /// <para>Paragraph 4.</para>
        /// 
        /// <p />HTML paragraph elements can also be used.
        /// <p>They work the same way.</p></remarks>
        /// <conceptualLink target="c7973ac7-5a4f-4e4d-9786-5ce659ac8e24" />
        public void ParagraphExample()
        {
        }
        #endregion

        #region param/paramref Example

        /// <summary>
        /// Executes a <see cref="SqlCommand" /> with the specified
        /// <paramref name="storedProcName" /> as a stored procedure initialized
        /// for updating the values of the specified <paramref name="row" />.
        /// </summary>
        /// <param name="storedProcName">The stored procedure name to execute</param>
        /// <param name="row">The row to update</param>
        /// <conceptualLink target="e54dcff7-f8f3-4a11-9d17-1cf7decd880e" />
        /// <conceptualLink target="fa7d6ea0-93ce-41f6-9417-2f98e80fe9f5" />
        public void CallStoredProcedure(string storedProcName, int row)
        {
            // ...
        }
        #endregion

        #region param Operator Example

        /// <summary>
        /// Addition operator overload
        /// </summary>
        /// <param name="left">The left value</param>
        /// <param name="right">The right value</param>
        /// <returns>A new instance containing the sum of the left and right
        /// sample numbers</returns>
        /// <conceptualLink target="e54dcff7-f8f3-4a11-9d17-1cf7decd880e" />
        public static SampleClass operator +(SampleClass left, SampleClass right)
        {
            return new SampleClass(left.SampleNumber + right.SampleNumber);
        }
        #endregion

        #region preliminary Example

        /// <summary>
        /// This method may go away or its signature may change in a later release.
        /// </summary>
        /// <preliminary />
        /// <conceptualLink target="c16bece7-694e-48ca-802d-cf3ae9205c55" />
        public void PreliminaryExample()
        {
        }

        /// <summary>
        /// A temporary method
        /// </summary>
        /// <preliminary>This method will be going away in the production release.</preliminary>
        /// <conceptualLink target="c16bece7-694e-48ca-802d-cf3ae9205c55" />
        public void TemporaryMethod()
        {
        }
        #endregion

        #region permission Example

        /// <summary>
        /// The <c>permission</c> element is used to document the types of permissions that
        /// a caller must be granted to access this method.
        /// </summary>
        /// <permission cref="SecurityPermission">
        /// <see cref="SecurityPermissionFlag.Execution">Execution</see> privilege.
        /// </permission>
        /// <permission cref="ReflectionPermission">
        /// <see cref="ReflectionPermissionFlag.MemberAccess">Member access</see> privilege
        /// for reflection.</permission>
        /// <conceptualLink target="4af64f3f-a9a3-42d7-a95c-bc0a40951286" />
        [SecurityPermission(SecurityAction.Demand, Execution = true)]
        [ReflectionPermission(SecurityAction.Demand, MemberAccess = true)]
        public void MethodRequiringSpecificPermissions()
        {
        }
        #endregion

        #region see/seealso cref Examples

        /// <summary>
        /// This event is raised when something interesting happens.
        /// </summary>
        /// <remarks><para>The <see cref="OnSomethingHappened"/> method is used to
        /// raise this event.  This event uses the general <see cref="EventHandler"/>
        /// delegate that passes <see cref="EventArgs.Empty">EventArgs.Empty</see>
        /// to the handlers.</para>
        /// 
        /// <para>Alternate <c>see</c> syntax: <see cref="EventArgs.Empty" qualifyHint="true"/></para>
        /// </remarks>
        /// <seealso cref="EventArgs"/>
        /// <seealso cref="EventArgs.Empty"/>
        /// <seealso cref="EventArgs.Empty" qualifyHint="true"/>
        /// <seealso cref="EventHandler"/>
        /// <seealso cref="OnSomethingHappened">XMLCommentsExamples.SampleClass.OnSomethingHappened</seealso>
        /// <conceptualLink target="983fed56-321c-4daf-af16-e3169b28ffcd" />
        /// <conceptualLink target="16cdb957-a35b-4c17-bf5e-ea511b0218e3" />
        public event EventHandler SomethingHappened;

        /// <summary>
        /// This is a protected virtual method used to raise the
        /// <see cref="SomethingHappened"/> event.
        /// </summary>
        /// <seealso cref="SomethingHappened"/>
        /// <conceptualLink target="983fed56-321c-4daf-af16-e3169b28ffcd" />
        /// <conceptualLink target="16cdb957-a35b-4c17-bf5e-ea511b0218e3" />
        protected virtual void OnSomethingHappened()
        {
            var handler = SomethingHappened;

            if(handler != null)
                handler(this, EventArgs.Empty);
        }
        #endregion

        #region see/seealso Method Overload Examples

        /// <summary>
        /// This version of the method takes no parameters.
        /// </summary>
        /// <remarks><para>To see all overloads, refer to the
        /// <see cref="DoSomething()" autoUpgrade="true" /> topic.</para>
        /// 
        /// <para>Other overloads:</para>
        /// 
        /// <list type="bullet">
        ///   <item>
        ///     <description><see cref="DoSomething(int)"/></description>
        ///   </item>
        ///   <item>
        ///     <description><see cref="DoSomething(string)"/></description>
        ///   </item>
        ///   <item>
        ///     <description><see cref="DoSomething(int, string)"/></description>
        ///   </item>
        /// </list>
        /// 
        /// <para>Alternate syntax to link to the overloads topic:
        /// <see cref="O:XMLCommentsExamples.SampleClass.DoSomething">DoSomething
        /// Overloads</see></para>
        /// </remarks>
        /// <overloads>This method has four overloads</overloads>
        /// <conceptualLink target="983fed56-321c-4daf-af16-e3169b28ffcd" />
        /// <conceptualLink target="16cdb957-a35b-4c17-bf5e-ea511b0218e3" />
        public void DoSomething()
        {
        }

        /// <summary>
        /// This version of the method takes an integer parameter.
        /// </summary>
        /// <param name="number">A number to use</param>
        /// <seealso cref="DoSomething()" autoUpgrade="true" />
        /// <seealso cref="DoSomething()" />
        /// <seealso cref="DoSomething(string)" />
        /// <seealso cref="DoSomething(int, string)" />
        /// <conceptualLink target="983fed56-321c-4daf-af16-e3169b28ffcd" />
        /// <conceptualLink target="16cdb957-a35b-4c17-bf5e-ea511b0218e3" />
        public void DoSomething(int number)
        {
        }

        /// <summary>
        /// This version of the method takes a string parameter.
        /// </summary>
        /// <param name="text">A text value to use</param>
        /// <seealso cref="DoSomething()"/>
        /// <seealso cref="DoSomething(int)"/>
        /// <seealso cref="DoSomething(int, string)"/>
        /// <conceptualLink target="983fed56-321c-4daf-af16-e3169b28ffcd" />
        /// <conceptualLink target="16cdb957-a35b-4c17-bf5e-ea511b0218e3" />
        public void DoSomething(string text)
        {
        }

        /// <summary>
        /// This version of the method takes both an integer and a string parameter.
        /// </summary>
        /// <param name="number">A number to use</param>
        /// <param name="text">A text value to use</param>
        /// <seealso cref="DoSomething()"/>
        /// <seealso cref="DoSomething(int)"/>
        /// <seealso cref="DoSomething(string)"/>
        /// <conceptualLink target="983fed56-321c-4daf-af16-e3169b28ffcd" />
        /// <conceptualLink target="16cdb957-a35b-4c17-bf5e-ea511b0218e3" />
        public void DoSomething(int number, string text)
        {
        }

        #endregion

        #region see/seealso href Examples

        /// <summary>
        /// This is used to demonstrate the external reference type of <c>see</c>
        /// and <c>seealso</c> elements.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///   <item>
        ///     <description>Basic link: <see href="https://GitHub.com/EWSoftware/SHFB"/></description>
        ///   </item>
        ///   <item>
        ///     <description>Link with target: <see href="https://GitHub.com/EWSoftware/SHFB/wiki"
        ///       target="_self" /></description>
        ///   </item>
        ///   <item>
        ///     <description>Link with alternate text: <see href="https://GitHub.com/EWSoftware/issues"
        ///       alt="Discuss SHFB on GitHub" /></description>
        ///   </item>
        ///   <item>
        ///     <description>Link with inner text: <see href="https://GitHub.com/EWSoftware/SHFB">
        ///       SHFB on GitHub</see></description>
        ///   </item>
        /// </list>
        /// <para>Equivalent <c>seealso</c> links are shown below.</para>
        /// </remarks>
        /// <seealso href="https://GitHub.com/EWSoftware/SHFB"/>
        /// <seealso href="https://GitHub.com/EWSoftware/SHFB/wiki" target="_self" />
        /// <seealso href="https://GitHub.com/EWSoftware/SHFB/issues" alt="Discuss SHFB on GitHub" />
        /// <seealso href="https://GitHub.com/EWSoftware/SHFB">SHFB on GitHub</seealso>
        /// <conceptualLink target="983fed56-321c-4daf-af16-e3169b28ffcd" />
        /// <conceptualLink target="16cdb957-a35b-4c17-bf5e-ea511b0218e3" />
        public void SeeElementExternalExample()
        {
        }
        #endregion

        #region see langword Examples

        /// <summary>
        /// This demonstrates the <c>see</c> element's <c>langword</c> form
        /// </summary>
        /// <returns>This method is <see langword="static" /> and always returns
        /// <see langword="null"/>.</returns>
        /// <conceptualLink target="983fed56-321c-4daf-af16-e3169b28ffcd" />
        public static string SeeLangWordExamples()
        {
            return null;
        }
        #endregion

        #region summary/remarks Example

        /// <summary>
        /// The summary is a brief description of the type or type member and will
        /// be displayed in IntelliSense and the Object Browser.
        /// </summary>
        /// <remarks>The remarks element should be used to provide more detailed information
        /// about the type or member such as how it is used, its processing, etc.
        /// 
        /// <para>Remarks will only appear in the help file and can be as long as
        /// necessary.</para></remarks>
        /// <conceptualLink target="c3743eaf-9ef2-4d35-8f59-876f1e48a6a8" />
        /// <conceptualLink target="3671b61e-0695-4f0f-bcb5-7cf9108dd642" />
        public void SummaryRemarksExample()
        {
        }
        #endregion

        #region token Example

        /// <summary>
        /// The help file was built on <token>BuildDate</token> using Sandcastle
        /// <token>SandcastleVersion</token>.
        /// </summary>
        /// <remarks>For tokens to be resolved, the <b>API Token Resolution</b> build component
        /// must be added to the SHFB project.</remarks>
        /// <conceptualLink target="8c9273f3-0000-43cd-bb53-932b80855297" />
        public void TokenExample()
        {
        }
        #endregion
    }
}
