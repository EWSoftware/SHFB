using System;
using System.Windows.Forms;

namespace TestDoc.Overloads
{
    /// <summary>
    /// A form class.  Compiling help for this using the Hana or VS2005 styles
    /// results in a bunch of Overloads pages in the TOC that shouldn't be
    /// there as they belong to the base Form class, not this one.  The
    /// Prototype style works as expected.
    /// </summary>
    public class OverloadsPageBugForm : System.Windows.Forms.Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OverloadsPageBugForm()
        {
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
