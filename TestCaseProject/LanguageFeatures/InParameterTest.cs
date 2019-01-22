using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDoc.LanguageFeatures
{
    /// <summary>
    /// Tests C# 7.2 in parameters.
    /// </summary>
    public class InParameterTest
    {
        /// <summary>
        /// In parameter test.
        /// </summary>
        /// <param name="param">In parameter.</param>
        public void InParameterMethod(in int param)
        {
        }

        /// <summary>
        /// In parameter test.
        /// </summary>
        /// <param name="param1">In parameter 1.</param>
        /// <param name="param2">In parameter 2.</param>
        public void InParameterMethod(in int param1, in int param2)
        {
        }

        /// <summary>
        /// Out parameter test.
        /// </summary>
        /// <param name="param">Out parameter.</param>
        public void OutParameterMethod(out int param)
        {
            param = 0;
        }

        /// <summary>
        /// Out parameter test.
        /// </summary>
        /// <param name="param1">Out parameter 1.</param>
        /// <param name="param2">Out parameter 2.</param>
        public void OutParameterMethod(out int param1, out int param2)
        {
            param1 = 0;
            param2 = 0;
        }

        /// <summary>
        /// Ref parameter test.
        /// </summary>
        /// <param name="param">Ref parameter.</param>
        public void RefParameterMethod(ref int param)
        {
        }

        /// <summary>
        /// Ref parameter test.
        /// </summary>
        /// <param name="param1">Ref parameter 1.</param>
        /// <param name="param2">Ref parameter 2.</param>
        public void RefParameterMethod(ref int param1, ref int param2)
        {
        }
    }
}
