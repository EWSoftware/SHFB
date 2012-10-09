// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

#if !NoReflection && !MinimalReader && WHIDBEY
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;

#if CCINamespace
using Microsoft.Cci.Metadata;
namespace Microsoft.Cci{
#else
using System.Compiler.Metadata;
using ElementType = System.Compiler.Metadata.ElementType;
namespace System.Compiler
{
#endif

    public class ReGenerator
    {
        protected TrivialHashtable/*!*/ exceptionStartCount = new TrivialHashtable();
        protected TrivialHashtable/*!*/ catchStart = new TrivialHashtable();
        protected TrivialHashtable/*!*/ catchTypeNode = new TrivialHashtable();
        protected TrivialHashtable/*!*/ finallyStart = new TrivialHashtable();
        protected TrivialHashtable/*!*/ exceptionEndCount = new TrivialHashtable();
        protected ILGenerator/*!*/ ILGenerator;
        protected TrivialHashtable/*!*/ labelIndex = new TrivialHashtable();
        protected List<LocalBuilder/*!*/>/*!*/ locals = new List<LocalBuilder/*!*/>();
        protected TrivialHashtable/*!*/ localsIndex = new TrivialHashtable();

        public ReGenerator(ILGenerator/*!*/ ilGenerator)
        {
            this.ILGenerator = ilGenerator;
            //^ base();
        }

        public virtual void VisitMethod(Method method)
        {
            if (method == null) { Debug.Assert(false); return; }
            this.VisitExceptionHandlers(method.ExceptionHandlers);
            this.VisitBlock(method.Body);
        }

        protected virtual void VisitExceptionHandlers(ExceptionHandlerList ehandlers)
        {
            //TODO: the list of ehandlers is sorted so that nested blocks always come first
            //When a handler does not have exactly the same start and end blocks as the previous handler, it is either a parent or a sibling.
            //When this transition occurs an end exception can be emitted when processing the last handler block of the previous handler.
            //Need to keep counts of the number of times a block starts a handler protected region and the number of times that a block
            //is the one following a handler exceptional region.
        }

        protected virtual Label GetLabel(Block/*!*/ block)
        {
            object label = this.labelIndex[block.UniqueKey];
            if (label == null)
            {
                label = this.ILGenerator.DefineLabel();
                this.labelIndex[block.UniqueKey] = label;
            }
            return (Label)label;
        }
        protected virtual int GetLocalVarIndex(Local/*!*/ loc)
        {
            object index = this.localsIndex[loc.UniqueKey];
            if (index is int) return (int)index;
            Type ltype = loc.Type == null ? null : loc.Type.GetRuntimeType();
            if (ltype == null) { Debug.Fail(""); return 0; }
            LocalBuilder locb = this.ILGenerator.DeclareLocal(ltype, loc.Pinned);
            int i = this.locals.Count;
            this.locals.Add(locb);
            this.localsIndex[loc.UniqueKey] = i;
            return i;
        }

        protected virtual void Visit(Node node)
        {
            if (node == null) return;
            switch (node.NodeType)
            {
                case NodeType.AddressDereference:
                    this.VisitAddressDereference((AddressDereference)node); return;
                case NodeType.Arglist:
                    this.VisitExpression((Expression)node); return;
                case NodeType.AssignmentStatement:
                    this.VisitAssignmentStatement((AssignmentStatement)node); return;
                case NodeType.Base:
                    this.VisitBase((Base)node); return;
                case NodeType.Block:
                    this.VisitBlock((Block)node); return;
                case NodeType.BlockExpression:
                    this.VisitBlockExpression((BlockExpression)node); return;
                case NodeType.Branch:
                    this.VisitBranch((Branch)node); return;
                case NodeType.DebugBreak:
                    this.VisitStatement((Statement)node); return;
                case NodeType.Call:
                case NodeType.Calli:
                case NodeType.Callvirt:
                case NodeType.Jmp:
                case NodeType.MethodCall:
                    this.VisitMethodCall((MethodCall)node); return;
                case NodeType.Construct:
                    this.VisitConstruct((Construct)node); return;
                case NodeType.ConstructArray:
                    this.VisitConstructArray((ConstructArray)node); return;
                case NodeType.Dup:
                    this.VisitExpression((Expression)node); return;
                case NodeType.EndFilter:
                    this.VisitEndFilter((EndFilter)node); return;
                case NodeType.EndFinally:
                    this.VisitStatement((Statement)node); return;
                case NodeType.ExpressionStatement:
                    this.VisitExpressionStatement((ExpressionStatement)node); return;
                case NodeType.Indexer:
                    this.VisitIndexer((Indexer)node); return;
                case NodeType.Literal:
                    this.VisitLiteral((Literal)node); return;
                case NodeType.Local:
                    this.VisitLocal((Local)node); return;
                case NodeType.MemberBinding:
                    this.VisitMemberBinding((MemberBinding)node); return;
                case NodeType.Nop:
                    this.VisitStatement((Statement)node); return;
                case NodeType.Parameter:
                    this.VisitParameter((Parameter)node); return;
                case NodeType.Pop:
                    this.VisitExpression((Expression)node); return;
                case NodeType.Rethrow:
                case NodeType.Throw:
                    this.VisitThrow((Throw)node); return;
                case NodeType.Return:
                    this.VisitReturn((Return)node); return;
                case NodeType.SwitchCaseBottom:
                    return;
                case NodeType.SwitchInstruction:
                    this.VisitSwitchInstruction((SwitchInstruction)node); return;
                case NodeType.This:
                    this.VisitThis((This)node); return;
                case NodeType.Cpblk:
                case NodeType.Initblk:
                    this.VisitTernaryExpression((TernaryExpression)node); return;
                case NodeType.Add:
                case NodeType.Add_Ovf:
                case NodeType.Add_Ovf_Un:
                case NodeType.And:
                case NodeType.Box:
                case NodeType.Castclass:
                case NodeType.Ceq:
                case NodeType.Cgt:
                case NodeType.Cgt_Un:
                case NodeType.Clt:
                case NodeType.Clt_Un:
                case NodeType.Div:
                case NodeType.Div_Un:
                case NodeType.Eq:
                case NodeType.Ge:
                case NodeType.Gt:
                case NodeType.Is:
                case NodeType.Isinst:
                case NodeType.Ldvirtftn:
                case NodeType.Le:
                case NodeType.Lt:
                case NodeType.Mkrefany:
                case NodeType.Mul:
                case NodeType.Mul_Ovf:
                case NodeType.Mul_Ovf_Un:
                case NodeType.Ne:
                case NodeType.Or:
                case NodeType.Refanyval:
                case NodeType.Rem:
                case NodeType.Rem_Un:
                case NodeType.Shl:
                case NodeType.Shr:
                case NodeType.Shr_Un:
                case NodeType.Sub:
                case NodeType.Sub_Ovf:
                case NodeType.Sub_Ovf_Un:
                case NodeType.Unbox:
                case NodeType.UnboxAny:
                case NodeType.Xor:
                    this.VisitBinaryExpression((BinaryExpression)node); return;
                case NodeType.AddressOf:
                case NodeType.OutAddress:
                case NodeType.RefAddress:
                case NodeType.ReadOnlyAddressOf:
                    this.VisitAddressOf((UnaryExpression)node); return;
                case NodeType.Ckfinite:
                case NodeType.Conv_I:
                case NodeType.Conv_I1:
                case NodeType.Conv_I2:
                case NodeType.Conv_I4:
                case NodeType.Conv_I8:
                case NodeType.Conv_Ovf_I:
                case NodeType.Conv_Ovf_I1:
                case NodeType.Conv_Ovf_I1_Un:
                case NodeType.Conv_Ovf_I2:
                case NodeType.Conv_Ovf_I2_Un:
                case NodeType.Conv_Ovf_I4:
                case NodeType.Conv_Ovf_I4_Un:
                case NodeType.Conv_Ovf_I8:
                case NodeType.Conv_Ovf_I8_Un:
                case NodeType.Conv_Ovf_I_Un:
                case NodeType.Conv_Ovf_U:
                case NodeType.Conv_Ovf_U1:
                case NodeType.Conv_Ovf_U1_Un:
                case NodeType.Conv_Ovf_U2:
                case NodeType.Conv_Ovf_U2_Un:
                case NodeType.Conv_Ovf_U4:
                case NodeType.Conv_Ovf_U4_Un:
                case NodeType.Conv_Ovf_U8:
                case NodeType.Conv_Ovf_U8_Un:
                case NodeType.Conv_Ovf_U_Un:
                case NodeType.Conv_R4:
                case NodeType.Conv_R8:
                case NodeType.Conv_R_Un:
                case NodeType.Conv_U:
                case NodeType.Conv_U1:
                case NodeType.Conv_U2:
                case NodeType.Conv_U4:
                case NodeType.Conv_U8:
                case NodeType.Ldftn:
                case NodeType.Ldlen:
                case NodeType.Ldtoken:
                case NodeType.Localloc:
                case NodeType.Neg:
                case NodeType.Not:
                case NodeType.Refanytype:
                case NodeType.Sizeof:
                    this.VisitUnaryExpression((UnaryExpression)node); return;
                default:
                    Debug.Assert(false, "invalid node: " + node.NodeType.ToString());
                    return;
            }
        }

        protected virtual void VisitAddressDereference(AddressDereference adr)
        {
            if (adr == null) return;
            this.Visit(adr.Address);
            if (adr.Alignment > 0)
                this.ILGenerator.Emit(OpCodes.Unaligned, (byte)adr.Alignment);
            if (adr.Volatile)
                this.ILGenerator.Emit(OpCodes.Volatile);
            if (adr.Type == null) return;
            switch (adr.Type.typeCode)
            {
                case ElementType.Int8: this.ILGenerator.Emit(OpCodes.Ldind_I1); return;
                case ElementType.UInt8: this.ILGenerator.Emit(OpCodes.Ldind_U1); return;
                case ElementType.Int16: this.ILGenerator.Emit(OpCodes.Ldind_I2); return;
                case ElementType.Char:
                case ElementType.UInt16: this.ILGenerator.Emit(OpCodes.Ldind_U2); return;
                case ElementType.Int32: this.ILGenerator.Emit(OpCodes.Ldind_I4); return;
                case ElementType.UInt32: this.ILGenerator.Emit(OpCodes.Ldind_U4); return;
                case ElementType.Int64:
                case ElementType.UInt64: this.ILGenerator.Emit(OpCodes.Ldind_I8); return;
                case ElementType.UIntPtr:
                case ElementType.IntPtr: this.ILGenerator.Emit(OpCodes.Ldind_I); return;
                case ElementType.Single: this.ILGenerator.Emit(OpCodes.Ldind_R4); return;
                case ElementType.Double: this.ILGenerator.Emit(OpCodes.Ldind_R8); return;
                default:
                    if (adr.Type.IsValueType && !(adr.Type is TypeParameter))
                    {
                        this.ILGenerator.Emit(OpCodes.Ldobj, adr.Type.GetRuntimeType());
                        return;
                    }
                    else if (TypeNode.StripModifiers(adr.Type) is Pointer)
                    {
                        this.ILGenerator.Emit(OpCodes.Ldind_I);
                        return;
                    }
                    this.ILGenerator.Emit(OpCodes.Ldind_Ref);
                    return;
            }
        }
        protected virtual void VisitAddressOf(UnaryExpression expr)
        {
            if (expr == null) return;
            Expression operand = expr.Operand;
            if (operand == null) return;
            switch (operand.NodeType)
            {
                case NodeType.Indexer:
                    Indexer indexer = (Indexer)operand;
                    this.Visit(indexer.Object);
                    if (indexer.Operands != null && indexer.Operands.Count == 1)
                        this.Visit(indexer.Operands[0]);
                    if (expr.NodeType == NodeType.ReadOnlyAddressOf)
                        this.ILGenerator.Emit(OpCodes.Readonly);
                    if (indexer.ElementType != null)
                        this.ILGenerator.Emit(OpCodes.Ldelema, indexer.ElementType.GetRuntimeType());
                    return;
                case NodeType.Local:
                    int li = this.GetLocalVarIndex((Local)operand);
                    if (li < 256)
                        this.ILGenerator.Emit(OpCodes.Ldloca_S, this.locals[li]);
                    else
                        this.ILGenerator.Emit(OpCodes.Ldloca, this.locals[li]);
                    return;
                case NodeType.MemberBinding:
                    MemberBinding mb = (MemberBinding)operand;
                    Field f = mb.BoundMember as Field;
                    if (f == null) { Debug.Fail(""); return; }
                    System.Reflection.FieldInfo fieldInfo = f.GetFieldInfo();
                    if (fieldInfo == null) { Debug.Fail(""); return; }
                    if (mb.TargetObject != null)
                    {
                        this.Visit(mb.TargetObject);
                        this.ILGenerator.Emit(OpCodes.Ldflda, fieldInfo);
                    }
                    else
                    {
                        this.ILGenerator.Emit(OpCodes.Ldsflda, fieldInfo);
                    }
                    return;
                case NodeType.Parameter:
                    ParameterBinding pb = operand as ParameterBinding;
                    if (pb != null) operand = pb.BoundParameter;
                    int pi = ((Parameter)operand).ArgumentListIndex;
                    if (pi < 256)
                        this.ILGenerator.Emit(OpCodes.Ldarga_S, (byte)pi);
                    else
                        this.ILGenerator.Emit(OpCodes.Ldarga, (ushort)pi);
                    return;
            }
        }
        protected virtual void VisitAssignmentStatement(AssignmentStatement assignment)
        {
            if (assignment == null) return;
            Expression target = assignment.Target;
            if (target == null) { Debug.Fail(""); return; }
            switch (target.NodeType)
            {
                case NodeType.Local:
                    Local loc = (Local)target;
                    this.Visit(assignment.Source);
                    int li = this.GetLocalVarIndex(loc);
                    switch (li)
                    {
                        case 0: this.ILGenerator.Emit(OpCodes.Stloc_0); return;
                        case 1: this.ILGenerator.Emit(OpCodes.Stloc_1); return;
                        case 2: this.ILGenerator.Emit(OpCodes.Stloc_2); return;
                        case 3: this.ILGenerator.Emit(OpCodes.Stloc_3); return;
                        default:
                            if (li < 256)
                                this.ILGenerator.Emit(OpCodes.Stloc_S, this.locals[li]);
                            else
                                this.ILGenerator.Emit(OpCodes.Stloc, this.locals[li]);
                            return;
                    }
                case NodeType.MemberBinding:
                    MemberBinding mb = (MemberBinding)target;
                    Field f = mb.BoundMember as Field;
                    if (f == null) { Debug.Fail(""); return; }
                    System.Reflection.FieldInfo fieldInfo = f.GetFieldInfo();
                    if (fieldInfo == null) { Debug.Fail(""); return; }
                    if (mb.TargetObject != null) this.Visit(mb.TargetObject);
                    this.Visit(assignment.Source);
                    if (mb.TargetObject != null)
                    {
                        if (mb.Alignment != -1)
                            this.ILGenerator.Emit(OpCodes.Unaligned, (byte)mb.Alignment);
                        if (mb.Volatile)
                            this.ILGenerator.Emit(OpCodes.Volatile);
                        this.ILGenerator.Emit(OpCodes.Stfld, fieldInfo);
                    }
                    else
                        this.ILGenerator.Emit(OpCodes.Stsfld, fieldInfo);
                    return;
                case NodeType.Parameter:
                    ParameterBinding pb = target as ParameterBinding;
                    if (pb != null) target = pb.BoundParameter;
                    Parameter par = (Parameter)target;
                    this.Visit(assignment.Source);
                    int pi = par.ArgumentListIndex;
                    if (pi < 256)
                        this.ILGenerator.Emit(OpCodes.Starg_S, (byte)pi);
                    else
                        this.ILGenerator.Emit(OpCodes.Starg, (ushort)pi);
                    return;
                case NodeType.Indexer:
                    Indexer indexer = (Indexer)target;
                    this.Visit(indexer.Object);
                    if (indexer.Operands != null && indexer.Operands.Count == 1)
                        this.Visit(indexer.Operands[0]);
                    this.Visit(assignment.Source);
                    Type elementType = indexer.ElementType == null ? null : indexer.ElementType.GetRuntimeType();
                    if (elementType == null) { Debug.Fail(""); return; }
                    this.ILGenerator.Emit(OpCodes.Ldelema, elementType);
                    System.Reflection.Emit.OpCode opCode;
                    //^ assert indexer.ElementType != null;
                    switch (indexer.ElementType.typeCode)
                    {
                        case ElementType.UIntPtr:
                        case ElementType.IntPtr: opCode = OpCodes.Stelem_I; break;
                        case ElementType.Boolean:
                        case ElementType.Int8:
                        case ElementType.UInt8: opCode = OpCodes.Stelem_I1; break;
                        case ElementType.Char:
                        case ElementType.Int16:
                        case ElementType.UInt16: opCode = OpCodes.Stelem_I2; break;
                        case ElementType.Int32:
                        case ElementType.UInt32: opCode = OpCodes.Stelem_I4; break;
                        case ElementType.Int64:
                        case ElementType.UInt64: opCode = OpCodes.Stelem_I8; break;
                        case ElementType.Single: opCode = OpCodes.Stelem_R4; break;
                        case ElementType.Double: opCode = OpCodes.Stelem_R8; break;
                        default:
                            if (indexer.ElementType.NodeType == NodeType.TypeParameter || indexer.ElementType.NodeType == NodeType.ClassParameter)
                                opCode = OpCodes.Stelem;
                            else if (TypeNode.StripModifiers(indexer.ElementType) is Pointer)
                                opCode = OpCodes.Stelem_I;
                            else
                                opCode = OpCodes.Stelem_Ref;
                            break;
                    }
                    if (opCode.Name == OpCodes.Stelem.Name)
                        this.ILGenerator.Emit(opCode, indexer.ElementType.GetRuntimeType());
                    else
                        this.ILGenerator.Emit(opCode);
                    return;
                case NodeType.AddressDereference:
                    AddressDereference adr = (AddressDereference)target;
                    if (adr.Type == null) { Debug.Fail(""); return; }
                    this.Visit(adr.Address);
                    if (adr.Type.IsValueType || adr.Type is TypeParameter)
                    {
                        Literal lit = assignment.Source as Literal;
                        if (lit != null && lit.Value == null)
                        {
                            this.ILGenerator.Emit(OpCodes.Initobj, adr.Type.GetRuntimeType());
                            return;
                        }
                    }
                    this.Visit(assignment.Source);
                    if (adr.Alignment > 0)
                        this.ILGenerator.Emit(OpCodes.Unaligned, (byte)adr.Alignment);
                    if (adr.Volatile)
                        this.ILGenerator.Emit(OpCodes.Volatile);
                    TypeNode adrType = TypeNode.StripModifiers(adr.Type);
                    //^ assert adrType != null;
                    switch (adrType.typeCode)
                    {
                        case ElementType.Int8:
                        case ElementType.UInt8: this.ILGenerator.Emit(OpCodes.Stind_I1); return;
                        case ElementType.Int16:
                        case ElementType.UInt16: this.ILGenerator.Emit(OpCodes.Stind_I2); return;
                        case ElementType.Int32:
                        case ElementType.UInt32: this.ILGenerator.Emit(OpCodes.Stind_I4); return;
                        case ElementType.Int64:
                        case ElementType.UInt64: this.ILGenerator.Emit(OpCodes.Stind_I8); return;
                        case ElementType.Single: this.ILGenerator.Emit(OpCodes.Stind_R4); return;
                        case ElementType.Double: this.ILGenerator.Emit(OpCodes.Stind_R8); return;
                        case ElementType.UIntPtr:
                        case ElementType.IntPtr: this.ILGenerator.Emit(OpCodes.Stind_I); return;
                        default:
                            if (adrType != null && (adrType.IsValueType ||
                              adrType.NodeType == NodeType.TypeParameter || adrType.NodeType == NodeType.ClassParameter))
                            {
                                this.ILGenerator.Emit(OpCodes.Stobj, adrType.GetRuntimeType());
                                return;
                            }
                            if (adrType.NodeType == NodeType.Pointer)
                            {
                                this.ILGenerator.Emit(OpCodes.Stind_I);
                                return;
                            }
                            this.ILGenerator.Emit(OpCodes.Stind_Ref);
                            return;
                    }
                default:
                    Debug.Assert(false, "unexpected assignment target");
                    return;
            }
        }
        protected virtual void VisitBase(Base Base)
        {
            this.ILGenerator.Emit(OpCodes.Ldarg_0);
        }
        protected virtual void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression == null) return;
            System.Reflection.Emit.OpCode opCode = OpCodes.Nop;
            this.Visit(binaryExpression.Operand1);
            switch (binaryExpression.NodeType)
            {
                case NodeType.Castclass: opCode = OpCodes.Castclass; goto writeOpCodeAndToken;
                case NodeType.Isinst: opCode = OpCodes.Isinst; goto writeOpCodeAndToken;
                case NodeType.Unbox: opCode = OpCodes.Unbox; goto writeOpCodeAndToken;
                case NodeType.UnboxAny: opCode = OpCodes.Unbox_Any; goto writeOpCodeAndToken;
                case NodeType.Box: opCode = OpCodes.Box; goto writeOpCodeAndToken;
                case NodeType.Refanyval: opCode = OpCodes.Refanyval; goto writeOpCodeAndToken;
                case NodeType.Mkrefany: opCode = OpCodes.Mkrefany; goto writeOpCodeAndToken;
                writeOpCodeAndToken:
                    Literal lit = binaryExpression.Operand2 as Literal;
                if (lit != null)
                    this.ILGenerator.Emit(opCode, ((TypeNode)lit.Value).GetRuntimeType());
                else
                    this.ILGenerator.Emit(opCode, ((TypeNode)((MemberBinding)binaryExpression.Operand2).BoundMember).GetRuntimeType());
                return;
                case NodeType.Ldvirtftn:
                System.Reflection.MethodInfo meth = ((Method)((MemberBinding)binaryExpression.Operand2).BoundMember).GetMethodInfo();
                if (meth == null) { Debug.Fail(""); return; }
                this.ILGenerator.Emit(OpCodes.Ldvirtftn, meth);
                return;
            }
            this.Visit(binaryExpression.Operand2);
            switch (binaryExpression.NodeType)
            {
                case NodeType.Add: opCode = OpCodes.Add; break;
                case NodeType.Sub: opCode = OpCodes.Sub; break;
                case NodeType.Mul: opCode = OpCodes.Mul; break;
                case NodeType.Div: opCode = OpCodes.Div; break;
                case NodeType.Div_Un: opCode = OpCodes.Div_Un; break;
                case NodeType.Rem: opCode = OpCodes.Rem; break;
                case NodeType.Rem_Un: opCode = OpCodes.Rem_Un; break;
                case NodeType.And: opCode = OpCodes.And; break;
                case NodeType.Or: opCode = OpCodes.Or; break;
                case NodeType.Xor: opCode = OpCodes.Xor; break;
                case NodeType.Shl: opCode = OpCodes.Shl; break;
                case NodeType.Shr: opCode = OpCodes.Shr; break;
                case NodeType.Shr_Un: opCode = OpCodes.Shr_Un; break;
                case NodeType.Add_Ovf: opCode = OpCodes.Add_Ovf; break;
                case NodeType.Add_Ovf_Un: opCode = OpCodes.Add_Ovf_Un; break;
                case NodeType.Mul_Ovf: opCode = OpCodes.Mul_Ovf; break;
                case NodeType.Mul_Ovf_Un: opCode = OpCodes.Mul_Ovf_Un; break;
                case NodeType.Sub_Ovf: opCode = OpCodes.Sub_Ovf; break;
                case NodeType.Sub_Ovf_Un: opCode = OpCodes.Sub_Ovf_Un; break;
                case NodeType.Ceq: opCode = OpCodes.Ceq; break;
                case NodeType.Cgt: opCode = OpCodes.Cgt; break;
                case NodeType.Cgt_Un: opCode = OpCodes.Cgt_Un; break;
                case NodeType.Clt: opCode = OpCodes.Clt; break;
                case NodeType.Clt_Un: opCode = OpCodes.Clt_Un; break;
            }
            this.ILGenerator.Emit(opCode);
        }
        protected virtual void VisitBlock(Block block)
        {
            if (block == null) return;
            if (this.catchStart[block.UniqueKey] != null)
                this.ILGenerator.BeginCatchBlock((Type)this.catchTypeNode[block.UniqueKey]);
            else if (this.finallyStart[block.UniqueKey] != null)
                this.ILGenerator.BeginFinallyBlock();
            else
            {
                object count = this.exceptionEndCount[block.UniqueKey];
                for (int i = 0, n = count == null ? 0 : (int)count; i < n; i++)
                    this.ILGenerator.EndExceptionBlock();
                count = this.exceptionStartCount[block.UniqueKey];
                for (int i = 0, n = count == null ? 0 : (int)count; i < n; i++)
                    this.ILGenerator.BeginExceptionBlock();
            }
            Label label = this.GetLabel(block);
            this.ILGenerator.MarkLabel(label);
            StatementList statements = block.Statements;
            if (statements == null) return;
            if (block.HasLocals) this.ILGenerator.BeginScope();
            for (int i = 0, n = statements.Count; i < n; i++)
                this.Visit(statements[i]);
            if (block.HasLocals) this.ILGenerator.EndScope();
        }
        protected virtual void VisitBlockExpression(BlockExpression blockExpression)
        {
            if (blockExpression == null) return;
            this.VisitBlock(blockExpression.Block);
        }
        protected virtual void VisitBranch(Branch branch)
        {
            if (branch == null) return;
            BinaryExpression bex = branch.Condition as BinaryExpression;
            UnaryExpression uex = null;
            NodeType typeOfCondition = NodeType.Nop;
            if (bex != null)
            {
                switch (bex.NodeType)
                {
                    case NodeType.Eq:
                    case NodeType.Ge:
                    case NodeType.Gt:
                    case NodeType.Le:
                    case NodeType.Lt:
                    case NodeType.Ne:
                        this.Visit(bex.Operand1);
                        this.Visit(bex.Operand2);
                        if (bex.Operand1 != null && bex.Operand1.Type != null && bex.Operand1.Type.IsUnsignedPrimitiveNumeric)
                            branch.BranchIfUnordered = true; //Overloaded to mean branch if unsigned for integer operands
                        typeOfCondition = bex.NodeType;
                        break;
                    case NodeType.And:
                    case NodeType.Or:
                    case NodeType.Xor:
                    case NodeType.Isinst:
                    case NodeType.Castclass:
                        typeOfCondition = NodeType.If;
                        goto default;
                    default:
                        this.Visit(branch.Condition);
                        break;
                }
            }
            else
            {
                uex = branch.Condition as UnaryExpression;
                if (uex != null && uex.NodeType == NodeType.LogicalNot)
                {
                    this.Visit(uex.Operand);
                    typeOfCondition = NodeType.LogicalNot;
                }
                else if (branch.Condition != null)
                {
                    typeOfCondition = NodeType.If;
                    this.Visit(branch.Condition);
                }
            }
            Label target = this.GetLabel(branch.Target);
            System.Reflection.Emit.OpCode opCode = OpCodes.Nop;
            if (branch.ShortOffset)
            {
                switch (typeOfCondition)
                {
                    case NodeType.Nop:
                        if (branch.Condition == null)
                        {
                            if (branch.LeavesExceptionBlock)
                                opCode = OpCodes.Leave_S;
                            else
                                opCode = OpCodes.Br_S;
                            break;
                        }
                        else
                        {
                            opCode = OpCodes.Brtrue_S; break;
                        }
                    case NodeType.If:
                        opCode = OpCodes.Brtrue_S; break;
                    case NodeType.LogicalNot:
                        opCode = OpCodes.Brfalse_S; break;
                    case NodeType.Eq:
                        opCode = OpCodes.Beq_S; break;
                    case NodeType.Ge:
                        if (branch.BranchIfUnordered)
                            opCode = OpCodes.Bge_Un_S;
                        else
                            opCode = OpCodes.Bge_S;
                        break;
                    case NodeType.Gt:
                        if (branch.BranchIfUnordered)
                            opCode = OpCodes.Bgt_Un_S;
                        else
                            opCode = OpCodes.Bgt_S;
                        break;
                    case NodeType.Le:
                        if (branch.BranchIfUnordered)
                            opCode = OpCodes.Ble_Un_S;
                        else
                            opCode = OpCodes.Ble_S;
                        break;
                    case NodeType.Lt:
                        if (branch.BranchIfUnordered)
                            opCode = OpCodes.Blt_Un_S;
                        else
                            opCode = OpCodes.Blt_S;
                        break;
                    case NodeType.Ne:
                        opCode = OpCodes.Bne_Un_S;
                        break;
                }
                this.ILGenerator.Emit(opCode, target);
            }
            else
            {
                switch (typeOfCondition)
                {
                    case NodeType.Nop:
                        if (branch.Condition == null)
                        {
                            if (branch.LeavesExceptionBlock)
                                opCode = OpCodes.Leave;
                            else
                                opCode = OpCodes.Br;
                            break;
                        }
                        else
                        {
                            opCode = OpCodes.Brtrue; break;
                        }
                    case NodeType.If:
                        opCode = OpCodes.Brtrue; break;
                    case NodeType.LogicalNot:
                        opCode = OpCodes.Brfalse; break;
                    case NodeType.Eq:
                        opCode = OpCodes.Beq; break;
                    case NodeType.Ge:
                        if (branch.BranchIfUnordered)
                            opCode = OpCodes.Bge_Un;
                        else
                            opCode = OpCodes.Bge;
                        break;
                    case NodeType.Gt:
                        if (branch.BranchIfUnordered)
                            opCode = OpCodes.Bgt_Un;
                        else
                            opCode = OpCodes.Bgt;
                        break;
                    case NodeType.Le:
                        if (branch.BranchIfUnordered)
                            opCode = OpCodes.Ble_Un;
                        else
                            opCode = OpCodes.Ble;
                        break;
                    case NodeType.Lt:
                        if (branch.BranchIfUnordered)
                            opCode = OpCodes.Blt_Un;
                        else
                            opCode = OpCodes.Blt;
                        break;
                    case NodeType.Ne:
                        opCode = OpCodes.Bne_Un; break;
                }
                this.ILGenerator.Emit(opCode, target);
            }
        }
        protected virtual void VisitMethodCall(MethodCall call)
        {
            if (call == null) return;
            MemberBinding mb = (MemberBinding)call.Callee;
            this.Visit(mb.TargetObject);
            ExpressionList arguments = call.Operands;
            if (arguments == null) arguments = new ExpressionList(0);
            this.VisitExpressionList(arguments);
            if (call.IsTailCall)
                this.ILGenerator.Emit(OpCodes.Tailcall);
            else if (call.Constraint != null)
                this.ILGenerator.Emit(OpCodes.Constrained, call.Constraint.GetRuntimeType());
            if (call.NodeType == NodeType.Calli)
            {
                FunctionPointer fp = (FunctionPointer)mb.BoundMember;
                CallingConventionFlags callConv = fp.CallingConvention & CallingConventionFlags.ArgumentConvention;
                if (callConv != CallingConventionFlags.VarArg)
                {
                    Type[] parameterTypes = new Type[fp.ParameterTypes.Count];
                    for (int i = 0, n = fp.ParameterTypes.Count; i < n; i++)
                    {
                        parameterTypes[i] = fp.ParameterTypes[i].GetRuntimeType();
                    }
                    this.ILGenerator.EmitCalli(OpCodes.Calli, (System.Runtime.InteropServices.CallingConvention)callConv,
                      fp.ReturnType.GetRuntimeType(), parameterTypes);
                }
                else
                {
                    Type[] parameterTypes = new Type[fp.VarArgStart];
                    Type[] optionalParameterTypes = new Type[fp.ParameterTypes.Count - fp.VarArgStart];
                    for (int i = 0, n = fp.ParameterTypes.Count; i < n; i++)
                    {
                        if (i < fp.VarArgStart)
                            parameterTypes[i] = fp.ParameterTypes[i].GetRuntimeType();
                        else
                            optionalParameterTypes[i - fp.VarArgStart] = fp.ParameterTypes[i].GetRuntimeType();
                    }
                    this.ILGenerator.EmitCalli(OpCodes.Calli, (System.Reflection.CallingConventions)callConv,
                      fp.ReturnType.GetRuntimeType(), parameterTypes, optionalParameterTypes);
                }
                return;
            }
            Method method = (Method)mb.BoundMember;
            System.Reflection.MethodInfo methodInfo = method.GetMethodInfo();
            if (methodInfo == null) { Debug.Fail(""); return; }
            System.Reflection.Emit.OpCode opCode;
            switch (call.NodeType)
            {
                case NodeType.Callvirt: opCode = OpCodes.Callvirt; break;
                case NodeType.Jmp: opCode = OpCodes.Jmp; break;
                default: opCode = OpCodes.Call; break;
            }
            if ((method.CallingConvention & CallingConventionFlags.ArgumentConvention) == CallingConventionFlags.VarArg ||
              (method.CallingConvention & CallingConventionFlags.ArgumentConvention) == CallingConventionFlags.C)
            {
                int varArgStart = method.Parameters.Count;
                Type[] optionalParameterTypes = new Type[arguments.Count - varArgStart];
                for (int i = varArgStart, n = arguments.Count; i < n; i++)
                    optionalParameterTypes[i - varArgStart] = arguments[i].Type.GetRuntimeType();
                this.ILGenerator.EmitCall(opCode, methodInfo, optionalParameterTypes);
            }
            else
                this.ILGenerator.EmitCall(opCode, methodInfo, null);
        }
        protected virtual void VisitConstruct(Construct cons)
        {
            if (cons == null) return;
            ExpressionList operands = cons.Operands;
            if (operands != null)
            {
                this.VisitExpressionList(cons.Operands);
            }
            Method method = (Method)((MemberBinding)cons.Constructor).BoundMember;
            System.Reflection.MethodInfo methodInfo = method.GetMethodInfo();
            if (methodInfo != null) this.ILGenerator.Emit(OpCodes.Newobj, methodInfo);
        }
        protected virtual void VisitConstructArray(ConstructArray consArr)
        {
            if (consArr == null || consArr.Operands == null || consArr.Operands.Count == 0) return;
            this.Visit(consArr.Operands[0]);
            this.ILGenerator.Emit(OpCodes.Newarr, consArr.ElementType.GetRuntimeType());
        }
        protected virtual void VisitEndFilter(EndFilter endFilter)
        {
            this.ILGenerator.Emit(OpCodes.Endfilter);
        }
        protected virtual void VisitExpression(Expression expression)
        {
            if (expression == null) return;
            switch (expression.NodeType)
            {
                case NodeType.Dup:
                    this.ILGenerator.Emit(OpCodes.Dup);
                    return;
                case NodeType.Pop:
                    UnaryExpression unex = expression as UnaryExpression;
                    if (unex != null)
                    {
                        this.Visit(unex.Operand);
                        this.ILGenerator.Emit(OpCodes.Pop);
                    }
                    return;
                case NodeType.Arglist:
                    this.ILGenerator.Emit(OpCodes.Arglist);
                    return;
            }
        }
        protected virtual void VisitExpressionList(ExpressionList expressions)
        {
            if (expressions == null) return;
            for (int i = 0, n = expressions.Count; i < n; i++)
                this.Visit(expressions[i]);
        }
        protected virtual void VisitExpressionStatement(ExpressionStatement statement)
        {
            if (statement == null) return;
            this.Visit(statement.Expression);
        }
        protected virtual void VisitIndexer(Indexer indexer)
        {
            if (indexer == null || indexer.Operands == null || indexer.Operands.Count == 0) return;
            this.Visit(indexer.Object);
            this.Visit(indexer.Operands[0]);
            System.Reflection.Emit.OpCode opCode;
            switch (indexer.ElementType.typeCode)
            {
                case ElementType.Boolean:
                case ElementType.Int8: opCode = OpCodes.Ldelem_I1; break;
                case ElementType.UInt8: opCode = OpCodes.Ldelem_U1; break;
                case ElementType.Int16: opCode = OpCodes.Ldelem_I2; break;
                case ElementType.Char:
                case ElementType.UInt16: opCode = OpCodes.Ldelem_U2; break;
                case ElementType.Int32: opCode = OpCodes.Ldelem_I4; break;
                case ElementType.UInt32: opCode = OpCodes.Ldelem_U4; break;
                case ElementType.Int64:
                case ElementType.UInt64: opCode = OpCodes.Ldelem_I8; break;
                case ElementType.UIntPtr:
                case ElementType.IntPtr: opCode = OpCodes.Ldelem_I; break;
                case ElementType.Single: opCode = OpCodes.Ldelem_R4; break;
                case ElementType.Double: opCode = OpCodes.Ldelem_R8; break;
                default:
                    if (indexer.ElementType.NodeType == NodeType.TypeParameter || indexer.ElementType.NodeType == NodeType.ClassParameter)
                        opCode = OpCodes.Ldelem;
                    else if (indexer.ElementType is Pointer)
                        opCode = OpCodes.Ldelem_I;
                    else
                        opCode = OpCodes.Ldelem_Ref;
                    break;
            }
            if (opCode.Name == OpCodes.Ldelem.Name)
                this.ILGenerator.Emit(opCode, indexer.ElementType.GetRuntimeType());
            else
                this.ILGenerator.Emit(opCode);
        }
        protected virtual void VisitLocal(Local local)
        {
            if (local == null) return;
            int li = this.GetLocalVarIndex(local);
            switch (li)
            {
                case 0: this.ILGenerator.Emit(OpCodes.Ldloc_0); return;
                case 1: this.ILGenerator.Emit(OpCodes.Ldloc_1); return;
                case 2: this.ILGenerator.Emit(OpCodes.Ldloc_2); return;
                case 3: this.ILGenerator.Emit(OpCodes.Ldloc_3); return;
                default:
                    if (li < 256)
                        this.ILGenerator.Emit(OpCodes.Ldloc_S, (byte)li);
                    else
                        this.ILGenerator.Emit(OpCodes.Ldloc, (ushort)li);
                    return;
            }
        }
        protected virtual void VisitLiteral(Literal literal)
        {
            if (literal == null) return;
            IConvertible ic = literal.Value as IConvertible;
            if (ic == null)
            {
                Debug.Assert(literal.Value == null && !literal.Type.IsValueType);
                this.ILGenerator.Emit(OpCodes.Ldnull); return;
            }
            TypeCode tc = ic.GetTypeCode();
            switch (tc)
            {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                    long n = ic.ToInt64(null);
                    switch (n)
                    {
                        case -1: this.ILGenerator.Emit(OpCodes.Ldc_I4_M1); break;
                        case 0: this.ILGenerator.Emit(OpCodes.Ldc_I4_0); break;
                        case 1: this.ILGenerator.Emit(OpCodes.Ldc_I4_1); break;
                        case 2: this.ILGenerator.Emit(OpCodes.Ldc_I4_2); break;
                        case 3: this.ILGenerator.Emit(OpCodes.Ldc_I4_3); break;
                        case 4: this.ILGenerator.Emit(OpCodes.Ldc_I4_4); break;
                        case 5: this.ILGenerator.Emit(OpCodes.Ldc_I4_5); break;
                        case 6: this.ILGenerator.Emit(OpCodes.Ldc_I4_6); break;
                        case 7: this.ILGenerator.Emit(OpCodes.Ldc_I4_7); break;
                        case 8: this.ILGenerator.Emit(OpCodes.Ldc_I4_8); break;
                        default:
                            if (n >= System.SByte.MinValue && n <= System.SByte.MaxValue)
                            {
                                this.ILGenerator.Emit(OpCodes.Ldc_I4_S, (byte)n);
                            }
                            else if (n >= System.Int32.MinValue && n <= System.Int32.MaxValue ||
                              n <= System.UInt32.MaxValue && (tc == TypeCode.Char || tc == TypeCode.UInt16 || tc == TypeCode.UInt32))
                            {
                                if (n == System.UInt32.MaxValue && tc != TypeCode.Int64)
                                    this.ILGenerator.Emit(OpCodes.Ldc_I4_M1);
                                else
                                {
                                    this.ILGenerator.Emit(OpCodes.Ldc_I4, (int)n);
                                }
                            }
                            else
                            {
                                this.ILGenerator.Emit(OpCodes.Ldc_I8, (long)n);
                                tc = TypeCode.Empty; //Suppress conversion to long
                            }
                            break;
                    }
                    if (tc == TypeCode.Int64)
                        this.ILGenerator.Emit(OpCodes.Conv_I8);
                    return;

                case TypeCode.UInt64:
                    this.ILGenerator.Emit(OpCodes.Ldc_I8, ic.ToUInt64(null));
                    return;

                case TypeCode.Single:
                    this.ILGenerator.Emit(OpCodes.Ldc_R4, ic.ToSingle(null));
                    return;

                case TypeCode.Double:
                    this.ILGenerator.Emit(OpCodes.Ldc_R8, ic.ToDouble(null));
                    return;

                case TypeCode.String:
                    this.ILGenerator.Emit(OpCodes.Ldstr, (string)literal.Value);
                    return;
            }
            Debug.Assert(false, "Unexpected literal type");
        }
        protected virtual void VisitMemberBinding(MemberBinding memberBinding)
        {
            if (memberBinding == null) return;
            System.Reflection.FieldInfo fieldInfo = ((Field)memberBinding.BoundMember).GetFieldInfo();
            if (memberBinding.TargetObject != null)
            {
                this.Visit(memberBinding.TargetObject);
                this.ILGenerator.Emit(OpCodes.Ldfld, fieldInfo);
            }
            else
            {
                this.ILGenerator.Emit(OpCodes.Ldsfld, fieldInfo);
            }
            return;
        }
        protected virtual void VisitParameter(Parameter parameter)
        {
            if (parameter == null) return;
            ParameterBinding pb = parameter as ParameterBinding;
            if (pb != null) parameter = pb.BoundParameter;
            int pi = parameter.ArgumentListIndex;
            switch (pi)
            {
                case 0: this.ILGenerator.Emit(OpCodes.Ldarg_0); return;
                case 1: this.ILGenerator.Emit(OpCodes.Ldarg_1); return;
                case 2: this.ILGenerator.Emit(OpCodes.Ldarg_2); return;
                case 3: this.ILGenerator.Emit(OpCodes.Ldarg_3); return;
                default:
                    if (pi < 256)
                        this.ILGenerator.Emit(OpCodes.Ldarg_S, (byte)pi);
                    else
                        this.ILGenerator.Emit(OpCodes.Ldarg, (ushort)pi);
                    return;
            }
        }
        protected virtual void VisitReturn(Return Return)
        {
            if (Return == null) return;
            if (Return.Expression != null)
            {
                this.Visit(Return.Expression);
            }
            this.ILGenerator.Emit(OpCodes.Ret);
        }
        protected virtual void VisitStatement(Statement statement)
        {
            if (statement == null) return;
            switch (statement.NodeType)
            {
                case NodeType.Nop: this.ILGenerator.Emit(OpCodes.Nop); break;
                case NodeType.DebugBreak: this.ILGenerator.Emit(OpCodes.Break); break;
                case NodeType.EndFinally: this.ILGenerator.Emit(OpCodes.Endfinally); break;
            }
        }
        protected virtual void VisitSwitchInstruction(SwitchInstruction switchInstruction)
        {
            if (switchInstruction == null) return;
            this.Visit(switchInstruction.Expression);
            BlockList targets = switchInstruction.Targets;
            int n = targets != null ? targets.Count : 0;
            Label[] labelTable = new Label[n];
            for (int i = 0; i < n; i++)
                labelTable[i] = this.GetLabel(targets[i]);
            this.ILGenerator.Emit(OpCodes.Switch, labelTable);
        }
        protected virtual void VisitTernaryExpression(TernaryExpression expression)
        {
            if (expression == null) return;
            this.Visit(expression.Operand1);
            this.Visit(expression.Operand2);
            this.Visit(expression.Operand3);
            if (expression.NodeType == NodeType.Cpblk)
                this.ILGenerator.Emit(OpCodes.Cpblk);
            else
                this.ILGenerator.Emit(OpCodes.Initblk);
        }
        protected virtual void VisitThis(This This)
        {
            this.ILGenerator.Emit(OpCodes.Ldarg_0);
        }
        protected virtual void VisitThrow(Throw Throw)
        {
            if (Throw == null) return;
            if (Throw.NodeType == NodeType.Rethrow)
                this.ILGenerator.Emit(OpCodes.Rethrow);
            else
            {
                this.Visit(Throw.Expression);
                this.ILGenerator.Emit(OpCodes.Throw);
            }
        }
        protected virtual void VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            if (unaryExpression == null) return;
            switch (unaryExpression.NodeType)
            {
                case NodeType.Ldtoken:
                    Literal lit = unaryExpression.Operand as Literal;
                    if (lit != null)
                        this.ILGenerator.Emit(OpCodes.Ldtoken, ((TypeNode)lit.Value).GetRuntimeType());
                    else
                    {
                        Member m = ((MemberBinding)unaryExpression.Operand).BoundMember;
                        Method meth = m as Method;
                        if (meth != null)
                        {
                            System.Reflection.MethodInfo methInfo = meth.GetMethodInfo();
                            if (methInfo == null) return;
                            this.ILGenerator.Emit(OpCodes.Ldtoken, methInfo);
                        }
                        else
                        {
                            System.Reflection.FieldInfo fieldInfo = ((Field)m).GetFieldInfo();
                            if (fieldInfo == null) return;
                            this.ILGenerator.Emit(OpCodes.Ldtoken, fieldInfo);
                        }
                    }
                    return;
                case NodeType.Ldftn:
                    {
                        System.Reflection.MethodInfo methInfo = ((Method)((MemberBinding)unaryExpression.Operand).BoundMember).GetMethodInfo();
                        if (methInfo != null) this.ILGenerator.Emit(OpCodes.Ldftn, methInfo);
                        return;
                    }
                case NodeType.Sizeof:
                    this.ILGenerator.Emit(OpCodes.Sizeof, ((TypeNode)((Literal)unaryExpression.Operand).Value).GetRuntimeType());
                    return;
            }
            this.Visit(unaryExpression.Operand);
            System.Reflection.Emit.OpCode opCode = OpCodes.Nop;
            switch (unaryExpression.NodeType)
            {
                case NodeType.Neg: opCode = OpCodes.Neg; break;
                case NodeType.Not: opCode = OpCodes.Not; break;
                case NodeType.Conv_I1: opCode = OpCodes.Conv_I1; break;
                case NodeType.Conv_I2: opCode = OpCodes.Conv_I2; break;
                case NodeType.Conv_I4: opCode = OpCodes.Conv_I4; break;
                case NodeType.Conv_I8: opCode = OpCodes.Conv_I8; break;
                case NodeType.Conv_R4: opCode = OpCodes.Conv_R4; break;
                case NodeType.Conv_R8: opCode = OpCodes.Conv_R8; break;
                case NodeType.Conv_U4: opCode = OpCodes.Conv_U4; break;
                case NodeType.Conv_U8: opCode = OpCodes.Conv_U8; break;
                case NodeType.Conv_R_Un: opCode = OpCodes.Conv_R_Un; break;
                case NodeType.Conv_Ovf_I1_Un: opCode = OpCodes.Conv_Ovf_I1_Un; break;
                case NodeType.Conv_Ovf_I2_Un: opCode = OpCodes.Conv_Ovf_I2_Un; break;
                case NodeType.Conv_Ovf_I4_Un: opCode = OpCodes.Conv_Ovf_I4_Un; break;
                case NodeType.Conv_Ovf_I8_Un: opCode = OpCodes.Conv_Ovf_I8_Un; break;
                case NodeType.Conv_Ovf_U1_Un: opCode = OpCodes.Conv_Ovf_U1_Un; break;
                case NodeType.Conv_Ovf_U2_Un: opCode = OpCodes.Conv_Ovf_U2_Un; break;
                case NodeType.Conv_Ovf_U4_Un: opCode = OpCodes.Conv_Ovf_U4_Un; break;
                case NodeType.Conv_Ovf_U8_Un: opCode = OpCodes.Conv_Ovf_U8_Un; break;
                case NodeType.Conv_Ovf_I_Un: opCode = OpCodes.Conv_Ovf_I_Un; break;
                case NodeType.Conv_Ovf_U_Un: opCode = OpCodes.Conv_Ovf_U_Un; break;
                case NodeType.Ldlen: opCode = OpCodes.Ldlen; break;
                case NodeType.Conv_Ovf_I1: opCode = OpCodes.Conv_Ovf_I1; break;
                case NodeType.Conv_Ovf_U1: opCode = OpCodes.Conv_Ovf_U1; break;
                case NodeType.Conv_Ovf_I2: opCode = OpCodes.Conv_Ovf_I2; break;
                case NodeType.Conv_Ovf_U2: opCode = OpCodes.Conv_Ovf_U2; break;
                case NodeType.Conv_Ovf_I4: opCode = OpCodes.Conv_Ovf_I4; break;
                case NodeType.Conv_Ovf_U4: opCode = OpCodes.Conv_Ovf_U4; break;
                case NodeType.Conv_Ovf_I8: opCode = OpCodes.Conv_Ovf_I8; break;
                case NodeType.Conv_Ovf_U8: opCode = OpCodes.Conv_Ovf_U8; break;
                case NodeType.Ckfinite: opCode = OpCodes.Ckfinite; break;
                case NodeType.Conv_U2: opCode = OpCodes.Conv_U2; break;
                case NodeType.Conv_U1: opCode = OpCodes.Conv_U1; break;
                case NodeType.Conv_I: opCode = OpCodes.Conv_I; break;
                case NodeType.Conv_Ovf_I: opCode = OpCodes.Conv_Ovf_I; break;
                case NodeType.Conv_Ovf_U: opCode = OpCodes.Conv_Ovf_U; break;
                case NodeType.Conv_U: opCode = OpCodes.Conv_U; break;
                case NodeType.Localloc: opCode = OpCodes.Localloc; break;
                case NodeType.Refanytype: opCode = OpCodes.Refanytype; break;
            }
            this.ILGenerator.Emit(opCode);
        }
    }
}
#endif