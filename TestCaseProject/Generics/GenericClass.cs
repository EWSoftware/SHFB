namespace TestDoc.Generics
{
    /// <summary>
    /// A test generic class
    /// </summary>
    public class GenericClass<TKey, TValue>
    {
        private TKey xkey;
        private TValue yvalue;

        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public GenericClass(TKey x, TValue y)
        {
            xkey = x;
            yvalue = y;
        }

        /// <summary>
        /// The Infragistics UltraGrid control is designed to present, and 
        /// optionally edit, both flat data (containing a single set of rows
        /// and columns) as well as hierarchical data in a variety of view
        /// styles.
        /// </summary>
        /// <remarks>
        /// <p class="body">Most of the settings that apply to the control are
        /// set off the <see cref="P:Infragistics.Win.UltraWinGrid.UltraGridBase.DisplayLayout"/>
        /// property.</p>
        /// <p></p>
        /// <p class="body">
        /// The following code snippet illustrates.
        /// </p>
        /// <p></p>
        /// <code>
        /// &lt;pre&gt; Literal pre tags are bad when nested in a code block
        ///             they should be removed or encoded if really needed.
        /// private void button5_Click(object sender, System.EventArgs e)
        /// {
        /// this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        /// this.ultraGrid1.DisplayLayout.Bands0.Columns1.Hidden = true;
        /// }
        /// &lt;/pre&gt;
        /// </code>
        /// </remarks>
        public void MethodWithBadComments()
        {
        }

        /// <summary>
        /// A generic method in the generic class
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="lhs">The left hand value</param>
        /// <param name="rhs">The right hand value</param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}
