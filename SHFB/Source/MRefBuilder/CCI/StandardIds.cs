// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 11/23/2013 - EFW - Cleared out the conditional statements

// Ignore Spelling: memberwise cci arglist

namespace System.Compiler
{
    public static class StandardIds
    {
        public static readonly Identifier Address = Identifier.For("Address");
        public static readonly Identifier AllowMultiple = Identifier.For("AllowMultiple");
        public static readonly Identifier ClassParameter = Identifier.For("class parameter");
        public static readonly Identifier Ctor = Identifier.For(".ctor");
        public static readonly Identifier CCtor = Identifier.For(".cctor");
        public static readonly Identifier Enum = Identifier.For("Enum");
        public static readonly Identifier Get = Identifier.For("Get");
        public static readonly Identifier Inherited = Identifier.For("Inherited");
        public static readonly Identifier Invoke = Identifier.For("Invoke");
        public static readonly Identifier Set = Identifier.For("Set");
        public static readonly Identifier System = Identifier.For("System");
        public static readonly Identifier This = Identifier.For("this");
        public static readonly Identifier TypeParameter = Identifier.For("type parameter");
        public static readonly Identifier Value__ = Identifier.For("value__");
        public static readonly Identifier _Deleted = Identifier.For("_Deleted");
        public static readonly Identifier opExplicit = Identifier.For("op_Explicit");
        public static readonly Identifier opImplicit = Identifier.For("op_Implicit");
        public static readonly Identifier Add = Identifier.For("Add");
        public static readonly Identifier Anonymity = Identifier.For("Anonymity");
        public static readonly Identifier ArgumentOutOfRangeException = Identifier.For("ArgumentOutOfRangeException");
        public static readonly Identifier Assembly = Identifier.For("Assembly");
        public static readonly Identifier Assert = Identifier.For("Assert");
        public static readonly Identifier BeginInvoke = Identifier.For("BeginInvoke");
        public static readonly Identifier callback = Identifier.For("callback");
        public static readonly Identifier CallingConvention = Identifier.For("CallingConvention");
        public static readonly Identifier CapitalObject = Identifier.For("Object");
        public static readonly Identifier CharSet = Identifier.For("CharSet");
        public static readonly Identifier Class = Identifier.For("Class");
        public static readonly Identifier Clear = Identifier.For("Clear");
        public static readonly Identifier Closure = Identifier.For("Closure");
        public static readonly Identifier Collection = Identifier.For("Collection");
        public static readonly Identifier Combine = Identifier.For("Combine");
        public static readonly Identifier Concat = Identifier.For("Concat");
        public static readonly Identifier Count = Identifier.For("Count");
        public static readonly Identifier CreateInstance = Identifier.For("CreateInstance");
        public static readonly Identifier Current = Identifier.For("Current");
        public static readonly Identifier Dispose = Identifier.For("Dispose");
        public static readonly Identifier ElementType = Identifier.For("ElementType");
        public static readonly Identifier Enter = Identifier.For("Enter");
        public static readonly Identifier EntryPoint = Identifier.For("EntryPoint");
        public static readonly Identifier ExactSpelling = Identifier.For("ExactSpelling");
        public static readonly Identifier Exit = Identifier.For("Exit");
        public static readonly Identifier EndInvoke = Identifier.For("EndInvoke");
        public static readonly new Identifier Equals = Identifier.For("Equals");
        public static readonly Identifier Finalize = Identifier.For("Finalize");
        public static readonly Identifier FromObject = Identifier.For("FromObject");
        public static readonly Identifier getCurrent = Identifier.For("get_Current");
        public static readonly Identifier getCount = Identifier.For("get_Count");
        public static readonly Identifier GetEnumerator = Identifier.For("GetEnumerator");
        public static readonly new Identifier GetHashCode = Identifier.For("GetHashCode");
        public static readonly Identifier getHasValue = Identifier.For("get_HasValue");
        public static readonly Identifier getItem = Identifier.For("get_Item");
        public static readonly Identifier GetTag = Identifier.For("GetTag");
        public static readonly Identifier GetTagAsType = Identifier.For("GetTagAsType");
        public static readonly Identifier getValue = Identifier.For("get_Value");
        public static readonly Identifier GetValue = Identifier.For("GetValue");
        public static readonly Identifier GetValueOrDefault = Identifier.For("GetValueOrDefault");
        public static readonly new Identifier GetType = Identifier.For("GetType");
        public static readonly Identifier Global = Identifier.For("global");
        public static readonly Identifier IFactory = Identifier.For("IFactory");
        public static readonly Identifier IEnumerableGetEnumerator = Identifier.For("IEnumerable.GetEnumerator");
        public static readonly Identifier IEnumeratorGetCurrent = Identifier.For("IEnumerator.get_Current");
        public static readonly Identifier IEnumeratorReset = Identifier.For("IEnumerator.Reset");
        public static readonly Identifier IndexOf = Identifier.For("IndexOf");
        public static readonly Identifier Insert = Identifier.For("Insert");
        public static readonly Identifier IsInterned = Identifier.For("IsInterned");
        public static readonly Identifier IsNull = Identifier.For("IsNull");
        public static readonly Identifier It = Identifier.For("it");
        public static readonly Identifier Item = Identifier.For("Item");
        public static readonly Identifier Length = Identifier.For("Length");
        public static readonly Identifier Main = Identifier.For("Main");
        public static readonly Identifier Method = Identifier.For("method");
        public static readonly new Identifier MemberwiseClone = Identifier.For("MemberwiseClone");
        public static readonly Identifier MoveNext = Identifier.For("MoveNext");
        public static readonly Identifier Namespace = Identifier.For("Namespace");
        public static readonly Identifier New = Identifier.For("New");
        public static readonly Identifier NewObj = Identifier.For(".newObj"); // used for locals representing new C() for value types
        public static readonly Identifier Object = Identifier.For("object");
        public static readonly Identifier opAddition = Identifier.For("op_Addition");
        public static readonly Identifier opBitwiseAnd = Identifier.For("op_BitwiseAnd");
        public static readonly Identifier opBitwiseOr = Identifier.For("op_BitwiseOr");
        public static readonly Identifier opComma = Identifier.For("op_Comma");
        public static readonly Identifier opDecrement = Identifier.For("op_Decrement");
        public static readonly Identifier opDivision = Identifier.For("op_Division");
        public static readonly Identifier opEquality = Identifier.For("op_Equality");
        public static readonly Identifier opExclusiveOr = Identifier.For("op_ExclusiveOr");
        public static readonly Identifier opFalse = Identifier.For("op_False");
        public static readonly Identifier opGreaterThan = Identifier.For("op_GreaterThan");
        public static readonly Identifier opGreaterThanOrEqual = Identifier.For("op_GreaterThanOrEqual");
        public static readonly Identifier opIncrement = Identifier.For("op_Increment");
        public static readonly Identifier opInequality = Identifier.For("op_Inequality");
        public static readonly Identifier opLeftShift = Identifier.For("op_LeftShift");
        public static readonly Identifier opLessThan = Identifier.For("op_LessThan");
        public static readonly Identifier opLessThanOrEqual = Identifier.For("op_LessThanOrEqual");
        public static readonly Identifier opLogicalNot = Identifier.For("op_LogicalNot");
        public static readonly Identifier opModulus = Identifier.For("op_Modulus");
        public static readonly Identifier opMultiply = Identifier.For("op_Multiply");
        public static readonly Identifier opOnesComplement = Identifier.For("op_OnesComplement");
        public static readonly Identifier opRightShift = Identifier.For("op_RightShift");
        public static readonly Identifier opSubtraction = Identifier.For("op_Subtraction");
        public static readonly Identifier opTrue = Identifier.For("op_True");
        public static readonly Identifier opUnaryNegation = Identifier.For("op_UnaryNegation");
        public static readonly Identifier opUnaryPlus = Identifier.For("op_UnaryPlus");
        public static readonly Identifier Pack = Identifier.For("Pack");
        public static readonly Identifier Phase = Identifier.For("Phase");
        public static readonly Identifier Position = Identifier.For("Position");
        public static readonly Identifier PreserveSig = Identifier.For("PreserveSig");
        public static readonly new Identifier ReferenceEquals = Identifier.For("ReferenceEquals");
        public static readonly Identifier Remove = Identifier.For("Remove");
        public static readonly Identifier Replace = Identifier.For("Replace");
        public static readonly Identifier Reset = Identifier.For("Reset");
        public static readonly Identifier result = Identifier.For("result");
        public static readonly Identifier SetLastError = Identifier.For("SetLastError");
        public static readonly Identifier SetValue = Identifier.For("SetValue");
        public static readonly Identifier Size = Identifier.For("Size");
        public static readonly Identifier StructuralTypes = Identifier.For("StructuralTypes");
        public static readonly Identifier Tag = Identifier.For("tag");
        public static readonly Identifier TagType = Identifier.For("tagType");
        public static readonly Identifier ThisValue = Identifier.For("this value: ");
        public static readonly Identifier ToObject = Identifier.For("ToObject");
        public static readonly new Identifier ToString = Identifier.For("ToString");
        public static readonly Identifier CciTypeExtensions = Identifier.For("System.Compiler.TypeExtensions");
        public static readonly Identifier Value = Identifier.For("value");
        public static readonly Identifier Var = Identifier.For("var");
        public static readonly Identifier __Arglist = Identifier.For("__arglist");
    }
}
