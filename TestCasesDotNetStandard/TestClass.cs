//===============================================================================================================
// System  : Sandcastle MRefBuilder
// File    : TestClass.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/16/2021
// Compiler: Microsoft Visual C#
//
// This class is used to test small snippets of code with MRefBuilder to diagnose problems
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/16/2021  EFW  Created the code
//===============================================================================================================

// For debugging:
// Build and run this project to run SHFB, build then help file at least past the MRefBuilder step, then
// run the MRefBuilder project using the following debug settings:
//
// Start External Program: C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe
// Command line arguments: GenerateRefInfo.proj
// Working directory: C:\GH\SHFB\SHFB\Source\MRefBuilder\DotNetStandardTestCases\Docs\Help\Working\

using System;
using System.Collections.Generic;

#nullable enable

namespace DotNetStandardTestCases
{
    public enum TestEnum
    {
        One,
        Two,
        Three
    }

    // NOTE TO SELF AND ANYONE ELSE READING THIS:
    // The comments regarding nullable context and state may not be accurate as changes are made to add new
    // test cases.  They were accurate to start with but may not be now.  The end result in the reflection
    // output is correct based on testing but I haven't gone back and updated the comments to necessarily match.

    // Nullable(1)
    public class MixedMostlyNonNullable
    {
        public Dictionary<string, int[]?>? x;

        public string? TestProperty { get; set; }

        public int a1;

        // Type's context
        public string a;

        // Nullable(1)
        public int[] b;

        // Type's context
        public int?[]? c;

        public int[]? d;

        // Nullable({ 2, 1 })
        public object[]? e;

        // Nullable({ 2, 1, 2 }
        public Dictionary<string, object?>? f;

        // Method context: Nullable(1)
        // Return value: Type's context
        // a: Method's context (non-nullable)
        // x: Nullable(2)
        // y: Nullable(2) (value types aren't null so only the array's nullable state is indicated)
        // z: Nullable({1, 2})
        public delegate string CallThis(int a, string? x, int[]? y, int?[] z);

        public MixedMostlyNonNullable()
        {
            a = String.Empty;
            b = Array.Empty<int>();
            d = Array.Empty<int>();
        }

        public string? TestMethod(string x, int?[] y, int?[]? z)
        {
            return null;
        }
    }

    // Nullable(2)
    public class MixedMostlyNullable
    {
        public int a1;

        // Type's context
        public string? a;

        // Nullable(1)
        public int[] b;

        // Type's context
        public int?[]? c;

        // Type's context (value types aren't null so only the array's nullable state is indicated)
        public int[]? d;

        // Nullable({ 2, 1 })
        public object[]? e;

        // Nullable({ 2, 1, 2 })
        public Dictionary<string, object?>? f;

        // Method context: Nullable(1)
        // Return value: Nullable(1)
        // a: Method's context (non-nullable)
        // x: Method's context
        // y: Method's context (value types aren't null so only the array's nullable state is indicated)
        // z: Nullable({1, 2})
        public delegate string? CallThis(int a, string? x, int[]? y, int?[] z);

        public MixedMostlyNullable()
        {
            b = Array.Empty<int>();
        }

        public string TestMethod(string? x, int[]? y, int?[] z)
        {
            return String.Empty;
        }
    }

    // Nullable(2)
    public class AllNullable
    {
        // Type's context
        public string? a;

        // Type's context
        public int?[]? b;

        // Type's context
        public int?[]? c;

        // Type's context
        public object?[]? d;

        // Type's context
        public Dictionary<string, object?>? e;

        // Type's context
        public delegate string? CallThis(string? x, int?[]? y, int?[]? z);

        public string? TestMethod(string? x, int?[]? y, int?[]? z)
        {
            return null;
        }
    }

    // Nullable(1)
    public class AllNonNullable
    {
        // Type's context
        public string a;

        // Type's context
        public int[] b;

        // Type's context
        public int[] c;

        // Type's context
        public object[] d;

        // Type's context
        public Dictionary<string, object> e;

        // Type's context
        public delegate string CallThis(string x, int[] y, int[] z);

        public string TestMethod(string x, int?[] y, int[] z)
        {
            return String.Empty;
        }

        public AllNonNullable()
        {
            a = String.Empty;
            b = Array.Empty<int>();
            c = Array.Empty<int>();
            d = Array.Empty<object>();
            e = new Dictionary<string, object>();
        }
    }
}
