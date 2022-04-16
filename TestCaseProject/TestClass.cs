﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TestDoc
{
    /// <summary>
    /// A test class
    /// </summary>
    /// <remarks><para>Token test: <token>SHFB</token></para>
    /// <para>NamespaceGroupDoc link test: <see cref="G:TestDoc.Generics"/></para>
    /// <para>NamespaceDoc link test: <see cref="N:TestDoc.DocumentationInheritance"/></para>
    /// </remarks>
    public class TestClass
    {
        #region Fields

        /// <summary>
        /// A test constant
        /// </summary>
        public const int TestConstant = 1234;

        /// <summary>
        /// A public string field
        /// </summary>
        public string publicStringField;

        /// <summary>
        /// A protected string field
        /// </summary>
        protected string protectedStringField;

        /// <summary>
        /// An internal string field
        /// </summary>
        internal string internalStringField;

        /// <summary>
        /// A protected internal string field
        /// </summary>
        protected internal string protectedInternalStringField;

        /// <summary>
        /// A private string field
        /// </summary>
        private string privateStringField;

        // Undocumented private static field
        private static string privateStaticField;
        #endregion

        #region Constructors
        static TestClass()
        {
            privateStaticField = String.Empty;
        }

        /// <summary>
        /// A public constructor
        /// </summary>
        /// <remarks>This has see and seealso tags with href attributes.
        /// 
        /// <p/>See href: <see href="https://GitHub.com/EWSoftware/SHFB" />
        /// <p/>See href: <see href="https://GitHub.com/EWSoftware/SHFB">SHFB on GitHub</see></remarks>
        /// <seealso href="https://GitHub.com/EWSoftware/SHFB"/>
        /// <seealso href="https://GitHub.com/EWSoftware/SHFB">SHFB on GitHub</seealso>
        public TestClass()
        {
            publicStringField = String.Empty;
        }

        /// <summary>
        /// A protected constructor
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="test2">Test 2</param>
        protected TestClass(string test, int test2)
        {
            protectedStringField = test;
        }

        /// <summary>
        /// An internal constructor
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="test2">Test 2</param>
        internal TestClass(string test, long test2)
        {
            internalStringField = test;
        }

        /// <summary>
        /// A protected internal constructor
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="test2">Test 2</param>
        protected internal TestClass(string test, decimal test2)
        {
            protectedInternalStringField = test;
        }

        /// <summary>
        /// A private constructor
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="test2">Test 2</param>
        private TestClass(string test, float test2)
        {
            privateStringField = test;
        }
        #endregion

        #region Properties
        /// <summary>
        /// A public property.  The getter and setter have an attribute.
        /// </summary>
        /// <value>If the value is <see cref="String.Empty"/>, blah blah blah.</value>
        /// <remarks>// Die Fehlermeldung an den ScriptManager übergeben</remarks>
        /// <example>
        /// <code title="Nested code block example" lang="VB.NET">
        /// <code source="ExampleFiles\Class1.vb" region="Test Region" />
        ///
        /// ' ... more stuff here ...
        ///
        /// <code source="ExampleFiles\Class1.vb" region="Embedded snippet" />
        /// </code>
        /// </example>
        public virtual string PublicProperty
        {
            [Dummy(StringValue = "Test get attribute")]
            get { return publicStringField; }
            [Dummy(StringValue = "Test set attribute", BoolValue = true)]
            set { publicStringField = value; }
        }

        /// <summary>
        /// A protected property
        /// </summary>
        protected virtual string ProtectedProperty
        {
            get { return protectedStringField; }
            set { protectedStringField = value; }
        }

        /// <summary>
        /// An internal property
        /// </summary>
        internal string InternalProperty
        {
            get { return internalStringField; }
        }

        /// <summary>
        /// A protected internal property
        /// </summary>
        protected internal string ProtectedInternalProperty
        {
            get { return protectedInternalStringField; }
        }

        /// <summary>
        /// A private property
        /// </summary>
        private string PrivateProperty
        {
            get { return privateStringField; }
        }

        /**
        <summary>
        C-style XML comments test.
        A boolean value indicating whether this instance has it.
        </summary>
        <returns>
        True iff the instance has it.
        </returns>
        <example>
        The following code prints the value of
        the <see cref="HasIt" /> property for a few numbers.
        <code>
        for (int i = 0; i &lt; 5; i++) {
        Thing x = new Thing(i);
        Console.WriteLine("{0}: {1}", x, x.HasIt);
        </code>
        The output is shown below.
        <code lang="none">
        0: True
        1: False
        2: False
        3: False
        4: True
        </code>
        </example>
        */
        public bool HasIt
        {
            get { return false; }
        }

        /// <summary>
        /// An automatic property with a private setter
        /// </summary>
        /// <remarks>The visibility should be indicated in the syntax section</remarks>
        public string AutoPropertyPrivateSetter
        {
            get;
            private set;
        }

        /// <summary>
        /// An automatic property with a protected getter (bad example but it's just a test)
        /// </summary>
        /// <remarks>The visibility should be indicated in the syntax section</remarks>
        public string AutoPropertyProtectedGetter
        {
            protected get;
            set;
        }
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// A test event
        /// </summary>
        public event EventHandler TestEvent;

        /// <summary>
        /// A cancelable event
        /// </summary>
        public event CancelEventHandler CancelableEvent;
        #endregion

        #region Methods
        /// <summary>
        /// A public method
        /// </summary>
        /// <returns>String</returns>
        /// <remarks>See <see cref="System.Collections.Generic.List{T}"/>
        /// for a generic list class.
        /// <p/>See <see cref="IndexTocBugs.IndexTest.TestArrayMethod(System.String[])">IndexTest.TestArrayMethod</see> for
        /// other details.</remarks>
        /// <example>
        /// <code lang="JavaScript">
        /// function Test()
        /// {
        ///    var x;
        ///    x = 1;
        ///    return x + 1;
        /// }
        /// </code>
        ///
        /// <code lang="jscript.net">
        /// // JScript.NET
        /// function Test() : Integer
        /// {
        ///    var x;
        ///    x = 1;
        ///    return x + 1;
        /// }
        /// </code>
        ///
        /// <code lang="XAML">
        /// <![CDATA[
        /// <!-- A XAML Example -->
        /// <Style x:Key="SpecialButton" TargetType="{x:Type Button}">
        ///   <Style.Triggers>
        ///     <!-- #region XAML Snippet -->
        ///     <Trigger Property="Button.IsMouseOver" Value="true">
        ///       <Setter Property = "Background" Value="Red"/>
        ///     </Trigger>
        ///     <Trigger Property="Button.IsPressed" Value="true">
        ///       <Setter Property = "Foreground" Value="Green"/>
        ///     </Trigger>
        ///     <!-- #endregion -->
        ///   </Style.Triggers>
        /// </Style>]]>
        /// </code>
        ///
        /// <code lang="cs" title="Test Code" keepSeeTags="true">
        /// // Test
        /// public void M1()
        /// {
        /// }
        ///
        /// #region Test
        ///
        /// // &lt;summary&gt;
        /// // This is called to generate the namespace summary file and to
        /// // purge the unwanted namespaces from the reflection information
        /// // file.
        /// // &lt;/summary&gt;
        /// // &lt;param name="reflectionFile"&gt;The reflection file to fix&lt;/param&gt;
        /// protected void GenerateNamespaceSummaries(string reflectionFile)
        /// {
        ///     XmlDocument sourceFile;
        ///     XmlNodeList nsElements, nsItems;
        ///     XmlNode item, member, tag;
        ///     NamespaceSummaryItem nsi;
        ///     string nsName, summaryText;
        ///     bool hasExclusions = false, isDocumented;
        ///
        ///     // Test &lt;see&gt; tag retention
        ///     this.<see cref="M:TestDoc.TestClass.ProtectedMethod"/>;
        ///     string x = this.<see cref="M:System.Object.ToString">ToString</see>();
        ///
        ///     this.ReportProgress(BuildStep.GenerateNamespaceSummaries,
        ///         "-------------------------------\r\n" +
        ///         "Generating namespace summary information...");
        ///
        ///     try
        ///     {
        ///         // Test
        ///     }
        ///     catch(Exception ex)
        ///     {
        ///         throw new BuilderException("Error generating namespace " +
        ///             "summaries: " + ex.Message, ex);
        ///     }
        /// }
        /// #endregion
        ///
        /// // Another test
        /// public void M2()
        /// {
        /// }
        ///
        /// // End Test
        /// </code>
        /// </example>
        public virtual string PublicMethod()
        {
            if(TestEvent != null)
                TestEvent(this, EventArgs.Empty);

            if(CancelableEvent != null)
                CancelableEvent(this, new CancelEventArgs());

            return publicStringField;
        }

#if DEBUG
        /// <summary>
        /// A protected method (debug)
        /// </summary>
        /// <returns>String</returns>
        protected virtual string ProtectedMethod()
        {
            return protectedStringField;
        }
#else
        /// <summary>
        /// A protected method (release)
        /// </summary>
        protected virtual string ProtectedMethod()
        {
            return protectedStringField;
        }
#endif

        /// <summary>
        /// An internal method
        /// </summary>
        internal string InternalMethod()
        {
            return internalStringField;
        }

        /// <summary>
        /// A protected internal method
        /// </summary>
        /// <returns>String</returns>
        /// <example>
        /// <code title="NOTE: Watch &amp; learn">
        /// // See VB.NET example for VB.NET code extraction test
        /// </code>
        /// <code lang="vbnet" source="ExampleFiles\Class1.vb" title="Whole class" />
        /// <code lang="vbnet" source="ExampleFiles\Class1.vb" region="Test Region"
        ///   title="Test region from class" />
        /// <code lang="vbnet" source="ExampleFiles\Class1.vb" region="Embedded snippet"
        ///   title="Test region from method" />
        /// <code source="ExampleFiles\CppClass.cpp" region="How to xyz" lang="cpp" />
        /// <code lang="xaml" source="ExampleFiles\Test.xaml" title="Whole XAML File" />
        /// <code lang="xaml" source="ExampleFiles\Test.xaml" region="XAML Snippet"
        ///   title="Test region from XAML file" />
        /// <code lang="sql" source="ExampleFiles\Test.sql" title="Whole SQL File" />
        /// <code lang="sql" source="ExampleFiles\Test.sql" region="SQL Snippet"
        ///   title="Test region from SQL file" />
        /// </example>
        protected internal virtual string ProtectedInternalMethod()
        {
            return protectedInternalStringField;
        }

        /// <summary>
        /// A private method()
        /// </summary>
        /// <returns>String</returns>
        private string PrivateMethod()
        {
            return privateStringField;
        }

        /// <summary>
        /// Computes the sum of an array of double values.
        /// </summary>
        /// <param name="values">The array of doubles</param>
        /// <returns>Returns the sum for the array of double values if the array is
        /// not null and not empty.</returns>
        /// <remarks>
        /// <para>Test definition list:</para>
        /// <list type="definition">
        /// <item>
        /// <term>maxLen</term>
        /// <description>field must contain no more than the specified number
        /// of characters
        /// </description>
        /// </item>
        /// <item>
        /// <term>minLen</term>
        /// <description>field must contain at least the specified number
        /// of characters
        /// </description>
        /// </item>
        /// <item>
        /// <term>maxVal</term>
        /// <description>field must contain a number that is no larger than the
        /// specified value
        /// </description>
        /// </item>
        /// <item>
        /// <term>minVal</term>
        /// <description>field must contain a number that is no smaller than the
        /// specified value
        /// </description>
        /// </item>
        /// <item>
        /// <term>pattern</term>
        /// <description>field must match the specified regular expression
        /// </description>
        /// </item>
        /// </list>
        /// 
        /// <para>Test bullet list with terms and definitions:</para>
        /// <list type="bullet">
        /// <item>
        /// <term>maxLen</term>
        /// <description>field must contain no more than the specified number
        /// of characters
        /// </description>
        /// </item>
        /// <item>
        /// <term>minLen</term>
        /// <description>field must contain at least the specified number
        /// of characters
        /// </description>
        /// </item>
        /// <item>
        /// <term>maxVal</term>
        /// <description>field must contain a number that is no larger than the
        /// specified value
        /// </description>
        /// </item>
        /// <item>
        /// <term>minVal</term>
        /// <description>field must contain a number that is no smaller than the
        /// specified value
        /// </description>
        /// </item>
        /// <item>
        /// <term>pattern</term>
        /// <description>field must match the specified regular expression
        /// </description>
        /// </item>
        /// </list>
        /// 
        /// <para>Test numbered list with terms and definitions:</para>
        /// <list type="number">
        /// <item>
        /// <term>maxLen</term>
        /// <description>field must contain no more than the specified number
        /// of characters
        /// </description>
        /// </item>
        /// <item>
        /// <term>minLen</term>
        /// <description>field must contain at least the specified number
        /// of characters
        /// </description>
        /// </item>
        /// <item>
        /// <term>maxVal</term>
        /// <description>field must contain a number that is no larger than the
        /// specified value
        /// </description>
        /// </item>
        /// <item>
        /// <term>minVal</term>
        /// <description>field must contain a number that is no smaller than the
        /// specified value
        /// </description>
        /// </item>
        /// <item>
        /// <term>pattern</term>
        /// <description>field must match the specified regular expression
        /// </description>
        /// </item>
        /// </list>
        /// 
        /// <para>Test table list with terms and definitions:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Item</term>
        /// <description>Description</description>
        /// </listheader>
        /// <item>
        /// <term>maxLen</term>
        /// <description>field must contain no more than the specified number
        /// of characters
        /// </description>
        /// </item>
        /// <item>
        /// <term>minLen</term>
        /// <description>field must contain at least the specified number
        /// of characters
        /// </description>
        /// </item>
        /// <item>
        /// <term>maxVal</term>
        /// <description>field must contain a number that is no larger than the
        /// specified value
        /// </description>
        /// </item>
        /// <item>
        /// <term>minVal</term>
        /// <description>field must contain a number that is no smaller than the
        /// specified value
        /// </description>
        /// </item>
        /// <item>
        /// <term>pattern</term>
        /// <description>field must match the specified regular expression
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        public static double Sum(double[] values)
        {
            double sum = 0.0;
            foreach(double value in values)
                sum += value;

            return sum;
        }
        #endregion

        #region Optional arguments test
        //=====================================================================

        /// <summary>
        /// .NET 4.0 optional arguments test.  The June 2010 and earlier releases of Sandcastle
        /// do not support optional argument values in the syntax section.
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="isUsed">Is used</param>
        /// <param name="optionalByAttr">Optional by attribute</param>
        /// <param name="smallAmount">Small amount</param>
        /// <param name="testString">Test string</param>
        /// <param name="value">Test value</param>
        /// <remarks>Conceptual link tests: <conceptualLink target="303c996a-2911-4c08-b492-6496c82b3edb"/> and
        /// <conceptualLink target="ba47a67c-4825-4fcd-b806-2b2e02d4373a">Another MAML topic</conceptualLink> and
        /// <conceptualLink target="dc4fcc96-283e-4202-9ecc-08a65e0c9313#BuildTools" />.</remarks>
        public void OptionalArgumentTest(int value, [Optional]int optionalByAttr,
          string testString = "Hello", bool isUsed = true, float smallAmount = 2.55f,
          double? amount = null)
        {
        }
        #endregion

        #region Attribute value test
        //=====================================================================

        /// <summary>
        /// The June 2010 and earlier releases of Sandcastle do not support numeric attribute values
        /// in the syntax sections.
        /// </summary>
        [DummyAttribute(
            ObjectValue = null,
            TypeValue = typeof(TestClass),
            EnumValue = KnownColor.Blue,
            StringValue = "Hello World",
            BoolValue = true,
            ByteValue = 0x1F,
            CharValue = 'X',
            ShortValue = Int16.MaxValue,
            IntegerValue = Int32.MaxValue,
            LongValue = Int64.MaxValue,
            FloatValue = 2.55f,
            DoubleValue = 1234.56789,
            UnsignedIntegerValue = UInt32.MaxValue,
            IntArrayValue = new[] { 1, 2, 3, 4 }
            )]
        public void AttributeValueTest()
        {
        }
        #endregion

        #region Value tuple tests
        //=====================================================================

        /// <summary>
        /// Mode and range
        /// </summary>
        public (TestFlags mode, string range) ModeAndRange { get; set; }

        /// <summary>
        /// Mode and range
        /// </summary>
        public (int mode, string range) AnotherTupleProperty { get; set; }

        /// <summary>
        /// A value tuple test method
        /// </summary>
        /// <param name="p1">Parameter 1</param>
        /// <param name="p2">Parameter 2</param>
        /// <returns>A value tuple</returns>
        public (string X, TestClass Y) ValueTupleMethod((double Value, string Label) p1,
          (string Test, ReferenceTest Something) p2)
        {
            return ("Test", null);
        }
        #endregion

        /// <summary> 
        /// Increment method increments the stored number by one. 
        /// </summary>
        /// <remarks>
        /// <para>See details at <a href="http://shfb.codeplex.com/Thread/View.aspx?ThreadId=57205"
        /// target="_blank">Change Note Icon</a></para>
        /// <note type="note">note</note>
        /// <note type="caution">caution</note>
        /// <note type="warning">warning</note>
        /// <note type="security">security</note>
        /// <note type="security note">security note</note>
        /// <note type="important">important</note>
        /// <note type="vb">vb</note>
        /// <note type="cs">cs</note>
        /// <note type="cpp">cpp</note>
        /// <note type="JSharp">JSharp</note>
        /// <note type="implement">implement</note>
        /// <note type="caller">caller</note>
        /// <note type="inherit">inherit</note>
        /// <note type="todo">To Do list</note>
        /// <note type="todo" title="Project Road Map">
        ///     <para>This alert has a custom title.</para>
        /// </note>
        /// </remarks>
        [Obsolete()]
        public void Increment()
        {
        }

        /// <summary>Test see elements with generic references:
        /// <para><see cref="Predicate{T}"/></para>
        /// <para><see cref="List{T}"/></para>
        /// <para><see cref="List{T}.Clear()"/></para>
        /// <para><see cref="List{T}.RemoveAll(Predicate{T})"/></para>
        /// </summary>
        [Obsolete("This is obsolete.  Use something else instead")]
        public static void TestSeeWithGenerics()
        {
        }

        // These test the reflection file referenced namespace filter to ensure it picks up type specializations

        /// <summary>
        /// Authors for the search response
        /// </summary>
        /// <remarks>
        /// This usually corresponds to the <c>Author</c> element(s) in an OpenSearch response, which is part of the Atom namespace. The server
        /// does not need to provide this information. If it is not provided, an empty list will be returned.
        /// </remarks>
        [Obsolete("This is obsolete.  Use something else instead", true)]
        public System.Collections.ObjectModel.Collection<System.ServiceModel.Syndication.SyndicationPerson> Authors
        {
            get { return null ; }
        }

        /// <summary>
        /// Search queries that the search client can use
        /// </summary>
        /// <remarks>
        /// <para>This corresponds to the <c>Query</c> elements in an OpenSearch response.</para>
        /// </remarks>
        public IEnumerable<string> Queries
        {
            get { return null; }
        }

        /// <summary>
        /// Search results in "raw" form
        /// </summary>
        /// <remarks>
        /// This property provides the items returned by the search as an enumeration of <c>System.ServiceModel.Syndication.SyndicationItem</c>, which
        /// provides low-level access to the items for further processing.
        /// </remarks>
        public IEnumerable<System.ServiceModel.Syndication.SyndicationItem> ItemsRaw
        {
            get { return null; }
        }

        /// <summary>
        /// Test method parameter attributes
        /// </summary>
        /// <param name="callerMemberName">Caller member name</param>
        /// <param name="callerLineNumber">Caller line number</param>
        /// <param name="callerFilePath">Caller file path</param>
        public void TestMethodParameterAttributes([CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "")
        {
        }
    }

    /// <summary>
    /// A test derived class
    /// </summary>
    /// <seealso cref="TestDerivedClass.Finalize"/>
    public class TestDerivedClass : TestClass
    {
        /// <summary>
        /// Test finalizer
        /// </summary>
        ~TestDerivedClass()
        {
        }

        /// <summary>
        /// Override of public property
        /// </summary>
        /// <example>
        /// Test code block colorizer:
        ///
        /// <code lang="cs" numberLines="false" outlining="false">
        /// // Test
        /// public override string PublicProperty
        /// {
        ///     get
        ///     {
        ///         return base.PublicProperty;
        ///     }
        /// }
        /// </code>
        /// <code lang="vbnet" numberLines="false" outlining="false">
        /// ' Test
        /// Public ReadOnly Overrides PublicProperty As String
        ///     Get
        ///         Return MyBase.PublicProperty
        ///     End Get
        /// End Property
        /// </code>
        /// </example>
        public override string PublicProperty
        {
            get { return base.PublicProperty; }
            set { base.PublicProperty = value; }
        }

        /// <summary>
        /// Override of protected method
        /// </summary>
        /// <returns>A string</returns>
        /// <example>
        /// Test code block colorizer:
        ///
        /// <code lang="cs" outlining="false">
        /// // Test
        /// protected override string ProtectedMethod()
        /// {
        ///     return base.ProtectedMethod();
        /// }
        /// </code>
        /// </example>
        protected override string ProtectedMethod()
        {
            return base.ProtectedMethod();
        }

        /// <summary>
        /// An override of the protected internal method
        /// </summary>
        /// <remarks>
        /// Test numbers list and new start attribute.
        /// <list type="number">
        ///   <item><description>First Point</description></item>
        ///   <item><description>Second Point</description></item>
        ///   <item><description>Third Point</description></item>
        /// </list>
        ///
        /// Some intervening text followed by a numbered list that
        /// starts out where the other left off.
        ///
        /// <list type="number" start="4">
        ///   <item><description>Fourth Point</description></item>
        ///   <item><description>Fifth Point</description></item>
        ///   <item><description>Sixth Point</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code source="..\TestClass.cs" region="Methods"
        ///     title="Methods Region" />
        /// </example>
        /// <returns>String</returns>
        protected internal override string ProtectedInternalMethod()
        {
            return base.ProtectedInternalMethod();
        }

        /// <summary>
        /// This event is raised when the dirty property changes
        /// </summary>
        public event EventHandler DirtyChanged;

        /// <summary>
        /// This raises the <see cref="DirtyChanged"/> event.
        /// </summary>
        /// <param name="e">The event arguments</param>
        private void OnDirtyChanged(EventArgs e)
        {
            if(DirtyChanged != null)
                DirtyChanged(this, e);
        }

        /// <summary>
        /// A generic method in a non-generic class
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="lhs">The left hand value</param>
        /// <param name="rhs">The right hand value</param>
        /// <preliminary>This might change later</preliminary>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>
        /// Test
        /// </summary>
        /// <param name="x">Test</param>
        /// <param name="y">Test</param>
        /// <preliminary/>
        public static void TestThis<T, K>(KeyValuePair<T, K> x, long y)
        {
        }

        /// <inheritdoc cref="Object.ToString" />
        /// <see cref="TestThis{T,K}(KeyValuePair{T, K}, long)"/>
        public override string ToString()
        {
            return base.ToString();
        }
    }

    /// <summary>
    /// A test sealed derived class
    /// </summary>
    public sealed class TestSealedDerivedClass : TestDerivedClass
    {
        /// <summary>
        /// An override of the public property in the sealed class
        /// </summary>
        public override string PublicProperty
        {
            get { return base.PublicProperty; }
            set { base.PublicProperty = value; }
        }

        // These bad includes are intentional to test the ShowMissingComponent

        /// <summary>
        /// An override of the public method in the sealed class
        /// </summary>
        /// <returns><para><see cref="String"/></para></returns>
        // For testing bad include: <include file='MissingExample.xml' path='Examples/Not/There[@name="Ex1"]/*' />
        public override string PublicMethod()
        {
            return base.PublicMethod();
        }

        /// <summary>
        /// An override of the protected method in the sealed class
        /// </summary>
        /// <returns>String</returns>
        /// <include file='Doc\ExampleFiles\Example.xml' path='Test/Example[@id="1"]/*' />
        protected override string ProtectedMethod()
        {
            return base.ProtectedMethod();
        }
    }
}
