using System;
using System.Net.Mail;

namespace TestDoc.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="MailMessage"/>.
    /// </summary>
    /// <remarks>https://github.com/EWSoftware/SHFB/issues/1155 - Missing comments issues on extension blocks</remarks>
    public static class MailMessageExtensions
    {
        /// <param name="message">
        /// The <see cref="MailMessage"/> to send.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="message"/> is <see langword="null"/>.</para>
        /// </exception>
        extension(MailMessage message)
        {
            /// <summary>
            /// Sends the message using the default SMTP settings.
            /// </summary>
            /// <param name="smtpClientFactory">
            /// The <see cref="SmtpClient"/>.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <para><paramref name="smtpClientFactory"/> is <see langword="null"/>.</para>
            /// </exception>
            /// <inheritdoc cref="T:TestDoc.ExtensionMethods.MailMessageExtensions.&lt;G&gt;$BD2D78579B3497A706F11AA8C4CDB9C3.&lt;M&gt;$C473A6B887300CCFEC1B2F307CC14A94" />
            public void Send(SmtpClient smtpClientFactory)
            {
                // According to Microsoft, this is by design and doc tools need to merge the info.  Also need
                // to exclude the compiler generated type which isn't tagged with CompilerGenerated but is
                // tagged with SpecialName.
            }
        }
    }

    /// <summary>
    /// Test extension method documentation
    /// </summary>
    public static class ExtensionTests
    {
        /// <summary>
        /// Simple test of extension method documentation.  This shows up
        /// correctly.
        /// </summary>
        /// <param name="s">The string to modify</param>
        /// <param name="x">The string to append</param>
        /// <returns>s + x</returns>
        public static string ExtendTest(this string s, string x)
        {
            return s + x;
        }

        /// <summary>
        /// As of the June 2010 release, this method doesn't show up as a
        /// member of the class.  In addition, it causes an empty Extension
        /// Methods section to appear in every class member list page.  The
        /// apparent cause is that it extends type Object.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>The string value or null if null or an empty string</returns>
        public static string ToStringOrNull(this object value)
        {
            if(value == null || value == DBNull.Value)
                return null;

            string s = value.ToString();

            return (s.Length == 0) ? null : s;
        }

        /// <summary>
        /// An extension for <see cref="TestClass"/>.
        /// </summary>
        /// <param name="testClass">A <see cref="TestClass"/> instance.</param>
        public static void ExtendClass(this TestClass testClass)
        {
        }

        /// <summary>
        /// An extension for <see cref="TestStruct"/>.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendStruct(this TestStruct testStruct)
        {
        }

        /// <summary>
        /// An extension for nullable <see cref="TestStruct"/>.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendNullableStruct(this TestStruct? testStruct)
        {
        }

        /// <summary>
        /// An extension for <see cref="TestStruct"/> by reference.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendStructByRef(this in TestStruct testStruct)
        {
        }

        /// <summary>
        /// An extension for <see cref="TestStruct"/> by reference.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendStructByActualRef(this ref TestStruct testStruct)
        {
        }

        /// <summary>
        /// An extension for a nullable <see cref="TestStruct"/> by reference.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendNullableStructByRef(this in TestStruct? testStruct)
        {
        }

        /// <summary>
        /// An extension for a nullable <see cref="TestStruct"/> by reference.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendNullableStructByActualRef(this ref TestStruct? testStruct)
        {
        }

        /// <summary>
        /// Enum extension method
        /// </summary>
        /// <param name="value">The enum value</param>
        public static void EnumExtensionMethod(this Enum value)
        {
        }
    }
}
