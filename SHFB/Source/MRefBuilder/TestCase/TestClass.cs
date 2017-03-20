//===============================================================================================================
// System  : Sandcastle MRefBuilder
// File    : TestClass.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/16/2017
// Compiler: Microsoft Visual C#
//
// This class is used to test small snippets of code with MRefBuilder to diagnose problems
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/01/2012  EFW  Created the code
//===============================================================================================================

// For debugging:
// Command line arguments: /out:reflection.org C:\GH\SHFB\SHFB\Source\MRefBuilder\TestCase\bin\Debug\TestCase.dll
// Working directory: C:\GH\SHFB\SHFB\Deploy\

using System.Collections.Generic;

namespace ValueTupleTest
{
    /// <summary>
    /// Test value tuples
    /// </summary>
    public class ValueTupleTestClass
    {
/*        /// <summary>
        /// A value tuple field
        /// </summary>
        public (int X, int Y) TupleField;

        /// <summary>
        /// A value tuple property
        /// </summary>
        /// <value>A value tuple</value>
        public (string FirstName, string LastName) TupleProperty { get => ("First", "Last"); }

        /// <summary>
        /// A value tuple delegate
        /// </summary>
        /// <returns>A value tuple</returns>
        public delegate (int FirstValue, string SecondValue) TupleDelegate();

        /// <summary>
        /// A value tuple return value
        /// </summary>
        /// <returns>A value tuple</returns>
        public (string Name, int Age) TupleMethodReturnValue()
        {
            return ("John Doe", 35);
        }

        /// <summary>
        /// A value tuple parameter
        /// </summary>
        /// <param name="point">A value tuple parameter</param>
        public void TupleMethodParameter((int XPos, int YPos) point)
        {
        }

        /// <summary>
        /// A value tuple enumerable
        /// </summary>
        /// <returns>An enumerable list of tuples</returns>
        public IEnumerable<(string Name, int Age)> TupleEnumerable()
        {
            yield return ("John Doe", 35);
        }

        /// <summary>
        /// A contrived value tuple example with named elements spread across two declarations
        /// </summary>
        /// <param name="p">A multi-tuple value</param>
        public void Test(KeyValuePair<(string KeyX, int KeyY), (string ValueX, int ValueY)> p)
        {
            string x = p.Key.KeyX;
            int y = p.Value.ValueY;
        }*/
    }
}
