// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics;
#if FxCop
using AttributeList = Microsoft.Cci.AttributeNodeCollection;
using BlockList = Microsoft.Cci.BlockCollection;
using ExpressionList = Microsoft.Cci.ExpressionCollection;
using InterfaceList = Microsoft.Cci.InterfaceCollection;
using MemberList = Microsoft.Cci.MemberCollection;
using NodeList = Microsoft.Cci.NodeCollection;
using ParameterList = Microsoft.Cci.ParameterCollection;
using SecurityAttributeList = Microsoft.Cci.SecurityAttributeCollection;
using StatementList = Microsoft.Cci.StatementCollection;
using TypeNodeList = Microsoft.Cci.TypeNodeCollection;
using Property = Microsoft.Cci.PropertyNode;
using Module = Microsoft.Cci.ModuleNode;
using Return = Microsoft.Cci.ReturnNode;
using Class = Microsoft.Cci.ClassNode;
using Interface = Microsoft.Cci.InterfaceNode;
using Event = Microsoft.Cci.EventNode;
using Throw = Microsoft.Cci.ThrowNode;
#endif
#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler
{
#endif

    /// <summary>
    /// Base for all classes that process the IR using the visitor pattern.
    /// </summary>
#if !FxCop
    public
#endif
 abstract class Visitor
    {
        /// <summary>
        /// Switches on node.NodeType to call a visitor method that has been specialized for node.
        /// </summary>
        /// <param name="node">The node to be visited.</param>
        /// <returns> Returns null if node is null. Otherwise returns an updated node (possibly a different object).</returns>
        public abstract Node Visit(Node node);
#if !MinimalReader
        /// <summary>
        /// Transfers the state from one visitor to another. This enables separate visitor instances to cooperative process a single IR.
        /// </summary>
        public virtual void TransferStateTo(Visitor targetVisitor)
        {
        }
#endif
        public virtual ExpressionList VisitExpressionList(ExpressionList list)
        {
            if (list == null) return null;
            for (int i = 0, n = list.Count; i < n; i++)
                list[i] = (Expression)this.Visit(list[i]);
            return list;
        }
    }

    /// <summary>
    /// Walks an IR, mutuating it into a new form
    /// </summary>   
#if !FxCop
    public
#endif
 class StandardVisitor : Visitor
    {
#if !MinimalReader
        public Visitor callingVisitor;
#endif
        protected bool memberListNamesChanged;

        public StandardVisitor()
        {
        }
#if !MinimalReader
        public StandardVisitor(Visitor callingVisitor)
        {
            this.callingVisitor = callingVisitor;
        }
#endif
        public virtual Node VisitUnknownNodeType(Node node)
        {
#if !MinimalReader
            Visitor visitor = this.GetVisitorFor(node);
            if (visitor == null) return node;
            if (this.callingVisitor != null)
                //Allow specialized state (unknown to this visitor) to propagate all the way down to the new visitor
                this.callingVisitor.TransferStateTo(visitor);
            this.TransferStateTo(visitor);
            node = visitor.Visit(node);
            visitor.TransferStateTo(this);
            if (this.callingVisitor != null)
                //Propagate specialized state (unknown to this visitor) all the way up the chain
                visitor.TransferStateTo(this.callingVisitor);
#else
      Debug.Assert(false);
#endif
            return node;
        }
#if !MinimalReader
        public virtual Visitor GetVisitorFor(Node node)
        {
            if (node == null) return null;
            return (Visitor)node.GetVisitorFor(this, this.GetType().Name);
        }
#endif
        public override Node Visit(Node node)
        {
            if (node == null) return null;
            switch (node.NodeType)
            {
#if !MinimalReader
                case NodeType.Acquire:
                    return this.VisitAcquire((Acquire)node);
#endif
                case NodeType.AddressDereference:
                    return this.VisitAddressDereference((AddressDereference)node);
#if !MinimalReader
                case NodeType.AliasDefinition:
                    return this.VisitAliasDefinition((AliasDefinition)node);
                case NodeType.AnonymousNestedFunction:
                    return this.VisitAnonymousNestedFunction((AnonymousNestedFunction)node);
                case NodeType.ApplyToAll:
                    return this.VisitApplyToAll((ApplyToAll)node);
#endif
                case NodeType.Arglist:
                    return this.VisitExpression((Expression)node);
#if !MinimalReader
                case NodeType.ArglistArgumentExpression:
                    return this.VisitArglistArgumentExpression((ArglistArgumentExpression)node);
                case NodeType.ArglistExpression:
                    return this.VisitArglistExpression((ArglistExpression)node);
#endif
                case NodeType.ArrayType:
                    Debug.Assert(false); return null;
                case NodeType.Assembly:
                    return this.VisitAssembly((AssemblyNode)node);
                case NodeType.AssemblyReference:
                    return this.VisitAssemblyReference((AssemblyReference)node);
#if !MinimalReader
                case NodeType.Assertion:
                    return this.VisitAssertion((Assertion)node);
                case NodeType.Assumption:
                    return this.VisitAssumption((Assumption)node);
                case NodeType.AssignmentExpression:
                    return this.VisitAssignmentExpression((AssignmentExpression)node);
#endif
                case NodeType.AssignmentStatement:
                    return this.VisitAssignmentStatement((AssignmentStatement)node);
                case NodeType.Attribute:
                    return this.VisitAttributeNode((AttributeNode)node);
#if !MinimalReader
                case NodeType.Base:
                    return this.VisitBase((Base)node);
#endif
                case NodeType.Block:
                    return this.VisitBlock((Block)node);
#if !MinimalReader
                case NodeType.BlockExpression:
                    return this.VisitBlockExpression((BlockExpression)node);
#endif
                case NodeType.Branch:
                    return this.VisitBranch((Branch)node);
#if !MinimalReader
                case NodeType.Compilation:
                    return this.VisitCompilation((Compilation)node);
                case NodeType.CompilationUnit:
                    return this.VisitCompilationUnit((CompilationUnit)node);
                case NodeType.CompilationUnitSnippet:
                    return this.VisitCompilationUnitSnippet((CompilationUnitSnippet)node);
#endif
#if ExtendedRuntime
        case NodeType.ConstrainedType:
          return this.VisitConstrainedType((ConstrainedType)node);
#endif
#if !MinimalReader
                case NodeType.Continue:
                    return this.VisitContinue((Continue)node);
                case NodeType.CurrentClosure:
                    return this.VisitCurrentClosure((CurrentClosure)node);
#endif
                case NodeType.DebugBreak:
                    return node;
                case NodeType.Call:
                case NodeType.Calli:
                case NodeType.Callvirt:
                case NodeType.Jmp:
#if !MinimalReader
                case NodeType.MethodCall:
#endif
                    return this.VisitMethodCall((MethodCall)node);
#if !MinimalReader
                case NodeType.Catch:
                    return this.VisitCatch((Catch)node);
#endif
                case NodeType.Class:
                    return this.VisitClass((Class)node);
#if !MinimalReader
                case NodeType.CoerceTuple:
                    return this.VisitCoerceTuple((CoerceTuple)node);
                case NodeType.CollectionEnumerator:
                    return this.VisitCollectionEnumerator((CollectionEnumerator)node);
                case NodeType.Composition:
                    return this.VisitComposition((Composition)node);
#endif
                case NodeType.Construct:
                    return this.VisitConstruct((Construct)node);
                case NodeType.ConstructArray:
                    return this.VisitConstructArray((ConstructArray)node);
#if !MinimalReader
                case NodeType.ConstructDelegate:
                    return this.VisitConstructDelegate((ConstructDelegate)node);
                case NodeType.ConstructFlexArray:
                    return this.VisitConstructFlexArray((ConstructFlexArray)node);
                case NodeType.ConstructIterator:
                    return this.VisitConstructIterator((ConstructIterator)node);
                case NodeType.ConstructTuple:
                    return this.VisitConstructTuple((ConstructTuple)node);
#endif
                case NodeType.DelegateNode:
                    return this.VisitDelegateNode((DelegateNode)node);
#if !MinimalReader
                case NodeType.DoWhile:
                    return this.VisitDoWhile((DoWhile)node);
#endif
                case NodeType.Dup:
                    return this.VisitExpression((Expression)node);
                case NodeType.EndFilter:
                    return this.VisitEndFilter((EndFilter)node);
                case NodeType.EndFinally:
                    return this.VisitEndFinally((EndFinally)node);
                case NodeType.EnumNode:
                    return this.VisitEnumNode((EnumNode)node);
                case NodeType.Event:
                    return this.VisitEvent((Event)node);
#if ExtendedRuntime
        case NodeType.EnsuresExceptional :
          return this.VisitEnsuresExceptional((EnsuresExceptional)node);
#endif
#if !MinimalReader
                case NodeType.Exit:
                    return this.VisitExit((Exit)node);
                case NodeType.Read:
                case NodeType.Write:
                    return this.VisitExpose((Expose)node);
                case NodeType.ExpressionSnippet:
                    return this.VisitExpressionSnippet((ExpressionSnippet)node);
#endif
                case NodeType.ExpressionStatement:
                    return this.VisitExpressionStatement((ExpressionStatement)node);
#if !MinimalReader
                case NodeType.FaultHandler:
                    return this.VisitFaultHandler((FaultHandler)node);
#endif
                case NodeType.Field:
                    return this.VisitField((Field)node);
#if !MinimalReader
                case NodeType.FieldInitializerBlock:
                    return this.VisitFieldInitializerBlock((FieldInitializerBlock)node);
                case NodeType.Finally:
                    return this.VisitFinally((Finally)node);
                case NodeType.Filter:
                    return this.VisitFilter((Filter)node);
                case NodeType.Fixed:
                    return this.VisitFixed((Fixed)node);
                case NodeType.For:
                    return this.VisitFor((For)node);
                case NodeType.ForEach:
                    return this.VisitForEach((ForEach)node);
                case NodeType.FunctionDeclaration:
                    return this.VisitFunctionDeclaration((FunctionDeclaration)node);
                case NodeType.Goto:
                    return this.VisitGoto((Goto)node);
                case NodeType.GotoCase:
                    return this.VisitGotoCase((GotoCase)node);
#endif
                case NodeType.Identifier:
                    return this.VisitIdentifier((Identifier)node);
#if !MinimalReader
                case NodeType.If:
                    return this.VisitIf((If)node);
                case NodeType.ImplicitThis:
                    return this.VisitImplicitThis((ImplicitThis)node);
#endif
                case NodeType.Indexer:
                    return this.VisitIndexer((Indexer)node);
                case NodeType.InstanceInitializer:
                    return this.VisitInstanceInitializer((InstanceInitializer)node);
#if ExtendedRuntime
        case NodeType.Invariant :
          return this.VisitInvariant((Invariant)node);
#endif
                case NodeType.StaticInitializer:
                    return this.VisitStaticInitializer((StaticInitializer)node);
                case NodeType.Method:
                    return this.VisitMethod((Method)node);
#if !MinimalReader
                case NodeType.TemplateInstance:
                    return this.VisitTemplateInstance((TemplateInstance)node);
                case NodeType.StackAlloc:
                    return this.VisitStackAlloc((StackAlloc)node);
#endif
                case NodeType.Interface:
                    return this.VisitInterface((Interface)node);
#if !MinimalReader
                case NodeType.LabeledStatement:
                    return this.VisitLabeledStatement((LabeledStatement)node);
#endif
                case NodeType.Literal:
                    return this.VisitLiteral((Literal)node);
                case NodeType.Local:
                    return this.VisitLocal((Local)node);
#if !MinimalReader
                case NodeType.LocalDeclaration:
                    return this.VisitLocalDeclaration((LocalDeclaration)node);
                case NodeType.LocalDeclarationsStatement:
                    return this.VisitLocalDeclarationsStatement((LocalDeclarationsStatement)node);
                case NodeType.Lock:
                    return this.VisitLock((Lock)node);
                case NodeType.LRExpression:
                    return this.VisitLRExpression((LRExpression)node);
#endif
                case NodeType.MemberBinding:
                    return this.VisitMemberBinding((MemberBinding)node);
#if ExtendedRuntime
        case NodeType.MethodContract :
          return this.VisitMethodContract((MethodContract)node);
#endif
                case NodeType.Module:
                    return this.VisitModule((Module)node);
                case NodeType.ModuleReference:
                    return this.VisitModuleReference((ModuleReference)node);
#if !MinimalReader
                case NodeType.NameBinding:
                    return this.VisitNameBinding((NameBinding)node);
#endif
                case NodeType.NamedArgument:
                    return this.VisitNamedArgument((NamedArgument)node);
#if !MinimalReader
                case NodeType.Namespace:
                    return this.VisitNamespace((Namespace)node);
#endif
                case NodeType.Nop:
#if !MinimalReader
                case NodeType.SwitchCaseBottom:
#endif
                    return node;
#if ExtendedRuntime
        case NodeType.EnsuresNormal :
          return this.VisitEnsuresNormal((EnsuresNormal)node);
        case NodeType.OldExpression :
          return this.VisitOldExpression((OldExpression)node);
        case NodeType.RequiresOtherwise :
          return this.VisitRequiresOtherwise((RequiresOtherwise)node);
        case NodeType.RequiresPlain :
          return this.VisitRequiresPlain((RequiresPlain)node);
#endif
                case NodeType.OptionalModifier:
                case NodeType.RequiredModifier:
                    //TODO: type modifers should only be visited via VisitTypeReference
                    return this.VisitTypeModifier((TypeModifier)node);
                case NodeType.Parameter:
                    return this.VisitParameter((Parameter)node);
                case NodeType.Pop:
                    return this.VisitExpression((Expression)node);
#if !MinimalReader
                case NodeType.PrefixExpression:
                    return this.VisitPrefixExpression((PrefixExpression)node);
                case NodeType.PostfixExpression:
                    return this.VisitPostfixExpression((PostfixExpression)node);
#endif
                case NodeType.Property:
                    return this.VisitProperty((Property)node);
#if !MinimalReader
                case NodeType.Quantifier:
                    return this.VisitQuantifier((Quantifier)node);
                case NodeType.Comprehension:
                    return this.VisitComprehension((Comprehension)node);
                case NodeType.ComprehensionBinding:
                    return this.VisitComprehensionBinding((ComprehensionBinding)node);
                case NodeType.QualifiedIdentifer:
                    return this.VisitQualifiedIdentifier((QualifiedIdentifier)node);
#endif
                case NodeType.Rethrow:
                case NodeType.Throw:
                    return this.VisitThrow((Throw)node);
#if !MinimalReader
                case NodeType.RefValueExpression:
                    return this.VisitRefValueExpression((RefValueExpression)node);
                case NodeType.RefTypeExpression:
                    return this.VisitRefTypeExpression((RefTypeExpression)node);
#endif
                case NodeType.Return:
                    return this.VisitReturn((Return)node);
#if !MinimalReader
                case NodeType.Repeat:
                    return this.VisitRepeat((Repeat)node);
                case NodeType.ResourceUse:
                    return this.VisitResourceUse((ResourceUse)node);
#endif
                case NodeType.SecurityAttribute:
                    return this.VisitSecurityAttribute((SecurityAttribute)node);
#if !MinimalReader
                case NodeType.SetterValue:
                    return this.VisitSetterValue((SetterValue)node);
                case NodeType.StatementSnippet:
                    return this.VisitStatementSnippet((StatementSnippet)node);
#endif
                case NodeType.Struct:
                    return this.VisitStruct((Struct)node);
#if !MinimalReader
                case NodeType.Switch:
                    return this.VisitSwitch((Switch)node);
                case NodeType.SwitchCase:
                    return this.VisitSwitchCase((SwitchCase)node);
#endif
                case NodeType.SwitchInstruction:
                    return this.VisitSwitchInstruction((SwitchInstruction)node);
#if !MinimalReader
                case NodeType.Typeswitch:
                    return this.VisitTypeswitch((Typeswitch)node);
                case NodeType.TypeswitchCase:
                    return this.VisitTypeswitchCase((TypeswitchCase)node);
#endif
                case NodeType.This:
                    return this.VisitThis((This)node);
#if !MinimalReader
                case NodeType.Try:
                    return this.VisitTry((Try)node);
#endif
#if ExtendedRuntime
        case NodeType.TypeContract:
          return this.VisitTypeContract((TypeContract)node);

        case NodeType.TupleType:
          return this.VisitTupleType((TupleType)node);
        case NodeType.TypeAlias:
          return this.VisitTypeAlias((TypeAlias)node);
        case NodeType.TypeIntersection:
          return this.VisitTypeIntersection((TypeIntersection)node);
#endif
#if !MinimalReader
                case NodeType.TypeMemberSnippet:
                    return this.VisitTypeMemberSnippet((TypeMemberSnippet)node);
#endif
                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                    return this.VisitTypeParameter((TypeNode)node);
#if ExtendedRuntime
        case NodeType.TypeUnion:
          return this.VisitTypeUnion((TypeUnion)node);
#endif
#if !MinimalReader
                case NodeType.TypeReference:
                    return this.VisitTypeReference((TypeReference)node);
                case NodeType.UsedNamespace:
                    return this.VisitUsedNamespace((UsedNamespace)node);
                case NodeType.VariableDeclaration:
                    return this.VisitVariableDeclaration((VariableDeclaration)node);
                case NodeType.While:
                    return this.VisitWhile((While)node);
                case NodeType.Yield:
                    return this.VisitYield((Yield)node);

                case NodeType.Conditional:
#endif
                case NodeType.Cpblk:
                case NodeType.Initblk:
                    return this.VisitTernaryExpression((TernaryExpression)node);

                case NodeType.Add:
                case NodeType.Add_Ovf:
                case NodeType.Add_Ovf_Un:
#if !MinimalReader
                case NodeType.AddEventHandler:
#endif
                case NodeType.And:
#if !MinimalReader
                case NodeType.As:
#endif
                case NodeType.Box:
                case NodeType.Castclass:
                case NodeType.Ceq:
                case NodeType.Cgt:
                case NodeType.Cgt_Un:
                case NodeType.Clt:
                case NodeType.Clt_Un:
#if !MinimalReader
                case NodeType.Comma:
#endif
                case NodeType.Div:
                case NodeType.Div_Un:
                case NodeType.Eq:
#if !MinimalReader
                case NodeType.ExplicitCoercion:
#endif
                case NodeType.Ge:
                case NodeType.Gt:
#if !MinimalReader
                case NodeType.Is:
                case NodeType.Iff:
                case NodeType.Implies:
#endif
                case NodeType.Isinst:
                case NodeType.Ldvirtftn:
                case NodeType.Le:
#if !MinimalReader
                case NodeType.LogicalAnd:
                case NodeType.LogicalOr:
#endif
                case NodeType.Lt:
                case NodeType.Mkrefany:
#if !MinimalReader
                case NodeType.Maplet:
#endif
                case NodeType.Mul:
                case NodeType.Mul_Ovf:
                case NodeType.Mul_Ovf_Un:
                case NodeType.Ne:
                case NodeType.Or:
#if !MinimalReader
                case NodeType.NullCoalesingExpression:
                case NodeType.Range:
#endif
                case NodeType.Refanyval:
                case NodeType.Rem:
                case NodeType.Rem_Un:
#if !MinimalReader
                case NodeType.RemoveEventHandler:
#endif
                case NodeType.Shl:
                case NodeType.Shr:
                case NodeType.Shr_Un:
                case NodeType.Sub:
                case NodeType.Sub_Ovf:
                case NodeType.Sub_Ovf_Un:
                case NodeType.Unbox:
                case NodeType.UnboxAny:
                case NodeType.Xor:
                    return this.VisitBinaryExpression((BinaryExpression)node);

                case NodeType.AddressOf:
#if !MinimalReader
                case NodeType.OutAddress:
                case NodeType.RefAddress:
#endif
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
#if !MinimalReader
                case NodeType.Decrement:
                case NodeType.DefaultValue:
                case NodeType.Increment:
#endif
                case NodeType.Ldftn:
                case NodeType.Ldlen:
                case NodeType.Ldtoken:
                case NodeType.Localloc:
                case NodeType.LogicalNot:
                case NodeType.Neg:
                case NodeType.Not:
#if !MinimalReader
                case NodeType.Parentheses:
#endif
                case NodeType.Refanytype:
                case NodeType.ReadOnlyAddressOf:
                case NodeType.Sizeof:
                case NodeType.SkipCheck:
#if !MinimalReader
                case NodeType.Typeof:
                case NodeType.UnaryPlus:
#endif
                    return this.VisitUnaryExpression((UnaryExpression)node);
#if ExtendedRuntime
          // query node types
        case NodeType.QueryAggregate:
          return this.VisitQueryAggregate((QueryAggregate)node);
        case NodeType.QueryAlias:
          return this.VisitQueryAlias((QueryAlias)node);
        case NodeType.QueryAll:
        case NodeType.QueryAny:
          return this.VisitQueryQuantifier((QueryQuantifier)node);
        case NodeType.QueryAxis:
          return this.VisitQueryAxis((QueryAxis)node);
        case NodeType.QueryCommit:
          return this.VisitQueryCommit((QueryCommit)node);
        case NodeType.QueryContext:
          return this.VisitQueryContext((QueryContext)node);
        case NodeType.QueryDelete:
          return this.VisitQueryDelete((QueryDelete)node);
        case NodeType.QueryDifference:
          return this.VisitQueryDifference((QueryDifference)node);
        case NodeType.QueryDistinct:
          return this.VisitQueryDistinct((QueryDistinct)node);
        case NodeType.QueryExists:
          return this.VisitQueryExists((QueryExists)node);
        case NodeType.QueryFilter:
          return this.VisitQueryFilter((QueryFilter)node);
        case NodeType.QueryGeneratedType:
          return this.VisitQueryGeneratedType((QueryGeneratedType)node);
        case NodeType.QueryGroupBy:
          return this.VisitQueryGroupBy((QueryGroupBy)node);
        case NodeType.QueryInsert:
          return this.VisitQueryInsert((QueryInsert)node);
        case NodeType.QueryIntersection:
          return this.VisitQueryIntersection((QueryIntersection)node);
        case NodeType.QueryIterator:
          return this.VisitQueryIterator((QueryIterator)node);
        case NodeType.QueryJoin:
          return this.VisitQueryJoin((QueryJoin)node);
        case NodeType.QueryLimit:
          return this.VisitQueryLimit((QueryLimit)node);
        case NodeType.QueryOrderBy:        
          return this.VisitQueryOrderBy((QueryOrderBy)node);
        case NodeType.QueryOrderItem:
          return this.VisitQueryOrderItem((QueryOrderItem)node);
        case NodeType.QueryPosition:
          return this.VisitQueryPosition((QueryPosition)node);
        case NodeType.QueryProject:
          return this.VisitQueryProject((QueryProject)node);          
        case NodeType.QueryQuantifiedExpression:
          return this.VisitQueryQuantifiedExpression((QueryQuantifiedExpression)node);
        case NodeType.QueryRollback:
          return this.VisitQueryRollback((QueryRollback)node);
        case NodeType.QuerySelect:
          return this.VisitQuerySelect((QuerySelect)node);
        case NodeType.QuerySingleton:
          return this.VisitQuerySingleton((QuerySingleton)node);
        case NodeType.QueryTransact:
          return this.VisitQueryTransact((QueryTransact)node);
        case NodeType.QueryTypeFilter:
          return this.VisitQueryTypeFilter((QueryTypeFilter)node);
        case NodeType.QueryUnion:
          return this.VisitQueryUnion((QueryUnion)node);
        case NodeType.QueryUpdate:
          return this.VisitQueryUpdate((QueryUpdate)node);
        case NodeType.QueryYielder:
          return this.VisitQueryYielder((QueryYielder)node);
#endif
                default:
                    return this.VisitUnknownNodeType(node);
            }
        }
        public virtual Expression VisitAddressDereference(AddressDereference addr)
        {
            if (addr == null) return null;
            addr.Address = this.VisitExpression(addr.Address);
            return addr;
        }
#if !MinimalReader
        public virtual AliasDefinition VisitAliasDefinition(AliasDefinition aliasDefinition)
        {
            if (aliasDefinition == null) return null;
            aliasDefinition.AliasedType = this.VisitTypeReference(aliasDefinition.AliasedType);
            return aliasDefinition;
        }
        public virtual AliasDefinitionList VisitAliasDefinitionList(AliasDefinitionList aliasDefinitions)
        {
            if (aliasDefinitions == null) return null;
            for (int i = 0, n = aliasDefinitions.Count; i < n; i++)
                aliasDefinitions[i] = this.VisitAliasDefinition(aliasDefinitions[i]);
            return aliasDefinitions;
        }
        public virtual Expression VisitAnonymousNestedFunction(AnonymousNestedFunction func)
        {
            if (func == null) return null;
            func.Parameters = this.VisitParameterList(func.Parameters);
            func.Body = this.VisitBlock(func.Body);
            return func;
        }
        public virtual Expression VisitApplyToAll(ApplyToAll applyToAll)
        {
            if (applyToAll == null) return null;
            applyToAll.Operand1 = this.VisitExpression(applyToAll.Operand1);
            applyToAll.Operand2 = this.VisitExpression(applyToAll.Operand2);
            return applyToAll;
        }
        public ArrayType VisitArrayType(ArrayType array)
        {
            Debug.Assert(false, "An array type exists only at runtime. It should be referred to, but never visited.");
            return null;
        }
#endif
        public virtual AssemblyNode VisitAssembly(AssemblyNode assembly)
        {
            if (assembly == null) return null;
            this.VisitModule(assembly);
            assembly.ModuleAttributes = this.VisitAttributeList(assembly.ModuleAttributes);
            assembly.SecurityAttributes = this.VisitSecurityAttributeList(assembly.SecurityAttributes);
            return assembly;
        }
        public virtual AssemblyReference VisitAssemblyReference(AssemblyReference assemblyReference)
        {
            return assemblyReference;
        }
#if !MinimalReader
        public virtual Statement VisitAssertion(Assertion assertion)
        {
            if (assertion == null) return null;
            assertion.Condition = this.VisitExpression(assertion.Condition);
            return assertion;
        }
        public virtual Statement VisitAssumption(Assumption assumption)
        {
            if (assumption == null) return null;
            assumption.Condition = this.VisitExpression(assumption.Condition);
            return assumption;
        }
        public virtual Expression VisitAssignmentExpression(AssignmentExpression assignment)
        {
            if (assignment == null) return null;
            assignment.AssignmentStatement = (Statement)this.Visit(assignment.AssignmentStatement);
            return assignment;
        }
#endif
        public virtual Statement VisitAssignmentStatement(AssignmentStatement assignment)
        {
            if (assignment == null) return null;
            assignment.Target = this.VisitTargetExpression(assignment.Target);
            assignment.Source = this.VisitExpression(assignment.Source);
            return assignment;
        }
        public virtual Expression VisitAttributeConstructor(AttributeNode attribute)
        {
            if (attribute == null) return null;
            return this.VisitExpression(attribute.Constructor);
        }
        public virtual AttributeNode VisitAttributeNode(AttributeNode attribute)
        {
            if (attribute == null) return null;
            attribute.Constructor = this.VisitAttributeConstructor(attribute);
            attribute.Expressions = this.VisitExpressionList(attribute.Expressions);
            return attribute;
        }
        public virtual AttributeList VisitAttributeList(AttributeList attributes)
        {
            if (attributes == null) return null;
            for (int i = 0, n = attributes.Count; i < n; i++)
                attributes[i] = this.VisitAttributeNode(attributes[i]);
            return attributes;
        }
#if !MinimalReader
        public virtual Expression VisitBase(Base Base)
        {
            return Base;
        }
#endif
        public virtual Expression VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression == null) return null;
            binaryExpression.Operand1 = this.VisitExpression(binaryExpression.Operand1);
            binaryExpression.Operand2 = this.VisitExpression(binaryExpression.Operand2);
            return binaryExpression;
        }
        public virtual Block VisitBlock(Block block)
        {
            if (block == null) return null;
            block.Statements = this.VisitStatementList(block.Statements);
            return block;
        }
#if !MinimalReader
        public virtual Expression VisitBlockExpression(BlockExpression blockExpression)
        {
            if (blockExpression == null) return null;
            blockExpression.Block = this.VisitBlock(blockExpression.Block);
            return blockExpression;
        }
#endif
        public virtual BlockList VisitBlockList(BlockList blockList)
        {
            if (blockList == null) return null;
            for (int i = 0, n = blockList.Count; i < n; i++)
                blockList[i] = this.VisitBlock(blockList[i]);
            return blockList;
        }
        public virtual Statement VisitBranch(Branch branch)
        {
            if (branch == null) return null;
            branch.Condition = this.VisitExpression(branch.Condition);
            return branch;
        }
#if !MinimalReader
        public virtual Statement VisitCatch(Catch Catch)
        {
            if (Catch == null) return null;
            Catch.Variable = this.VisitTargetExpression(Catch.Variable);
            Catch.Type = this.VisitTypeReference(Catch.Type);
            Catch.Block = this.VisitBlock(Catch.Block);
            return Catch;
        }
        public virtual CatchList VisitCatchList(CatchList catchers)
        {
            if (catchers == null) return null;
            for (int i = 0, n = catchers.Count; i < n; i++)
                catchers[i] = (Catch)this.VisitCatch(catchers[i]);
            return catchers;
        }
#endif
        public virtual Class VisitClass(Class Class)
        {
            return (Class)this.VisitTypeNode(Class);
        }
#if !MinimalReader
        public virtual Expression VisitCoerceTuple(CoerceTuple coerceTuple)
        {
            if (coerceTuple == null) return null;
            coerceTuple.OriginalTuple = this.VisitExpression(coerceTuple.OriginalTuple);
            return this.VisitConstructTuple(coerceTuple);
        }
        public virtual CollectionEnumerator VisitCollectionEnumerator(CollectionEnumerator ce)
        {
            if (ce == null) return null;
            ce.Collection = this.VisitExpression(ce.Collection);
            return ce;
        }
        public virtual Compilation VisitCompilation(Compilation compilation)
        {
            if (compilation == null) return null;
            Module module = compilation.TargetModule;
            if (module != null)
                module.Attributes = this.VisitAttributeList(module.Attributes);
            AssemblyNode assem = module as AssemblyNode;
            if (assem != null)
                assem.ModuleAttributes = this.VisitAttributeList(assem.ModuleAttributes);
            compilation.CompilationUnits = this.VisitCompilationUnitList(compilation.CompilationUnits);
            return compilation;
        }
        public virtual CompilationUnit VisitCompilationUnit(CompilationUnit cUnit)
        {
            if (cUnit == null) return null;
            cUnit.Nodes = this.VisitNodeList(cUnit.Nodes);
            return cUnit;
        }
        public virtual NodeList VisitNodeList(NodeList nodes)
        {
            if (nodes == null) return null;
            for (int i = 0, n = nodes.Count; i < n; i++)
                nodes[i] = this.Visit(nodes[i]);
            return nodes;
        }
        public virtual CompilationUnitList VisitCompilationUnitList(CompilationUnitList compilationUnits)
        {
            if (compilationUnits == null) return null;
            for (int i = 0, n = compilationUnits.Count; i < n; i++)
                compilationUnits[i] = (CompilationUnit)this.Visit(compilationUnits[i]);
            return compilationUnits;
        }
        public virtual CompilationUnit VisitCompilationUnitSnippet(CompilationUnitSnippet snippet)
        {
            return this.VisitCompilationUnit(snippet);
        }
        public virtual Node VisitComposition(Composition comp)
        {
            if (comp == null) return null;
            if (comp.GetType() == typeof(Composition))
            {
                comp.Expression = (Expression)this.Visit(comp.Expression);
                return comp;
            }
            return this.VisitUnknownNodeType(comp);
        }
#endif
        public virtual Expression VisitConstruct(Construct cons)
        {
            if (cons == null) return null;
            cons.Constructor = this.VisitExpression(cons.Constructor);
            cons.Operands = this.VisitExpressionList(cons.Operands);
#if !MinimalReader
            cons.Owner = this.VisitExpression(cons.Owner);
#endif
            return cons;
        }
        public virtual Expression VisitConstructArray(ConstructArray consArr)
        {
            if (consArr == null) return null;
            consArr.ElementType = this.VisitTypeReference(consArr.ElementType);
            consArr.Operands = this.VisitExpressionList(consArr.Operands);
#if !MinimalReader
            consArr.Initializers = this.VisitExpressionList(consArr.Initializers);
            consArr.Owner = this.VisitExpression(consArr.Owner);
#endif
            return consArr;
        }
#if !MinimalReader
        public virtual Expression VisitConstructDelegate(ConstructDelegate consDelegate)
        {
            if (consDelegate == null) return null;
            consDelegate.DelegateType = this.VisitTypeReference(consDelegate.DelegateType);
            consDelegate.TargetObject = this.VisitExpression(consDelegate.TargetObject);
            return consDelegate;
        }
        public virtual Expression VisitConstructFlexArray(ConstructFlexArray consArr)
        {
            if (consArr == null) return null;
            consArr.ElementType = this.VisitTypeReference(consArr.ElementType);
            consArr.Operands = this.VisitExpressionList(consArr.Operands);
            consArr.Initializers = this.VisitExpressionList(consArr.Initializers);
            return consArr;
        }
        public virtual Expression VisitConstructIterator(ConstructIterator consIterator)
        {
            return consIterator;
        }
        public virtual Expression VisitConstructTuple(ConstructTuple consTuple)
        {
            if (consTuple == null) return null;
            consTuple.Fields = this.VisitFieldList(consTuple.Fields);
            return consTuple;
        }
#endif
#if ExtendedRuntime
    public virtual TypeNode VisitConstrainedType(ConstrainedType cType){
      if (cType == null) return null;
      cType.UnderlyingType = this.VisitTypeReference(cType.UnderlyingType);
      cType.Constraint = this.VisitExpression(cType.Constraint);
      return cType;
    }
#endif
#if !MinimalReader
        public virtual Statement VisitContinue(Continue Continue)
        {
            return Continue;
        }
        public virtual Expression VisitCurrentClosure(CurrentClosure currentClosure)
        {
            return currentClosure;
        }
#endif
        public virtual DelegateNode VisitDelegateNode(DelegateNode delegateNode)
        {
            if (delegateNode == null) return null;
            delegateNode = (DelegateNode)this.VisitTypeNode(delegateNode);
            if (delegateNode == null) return null;
            delegateNode.Parameters = this.VisitParameterList(delegateNode.Parameters);
            delegateNode.ReturnType = this.VisitTypeReference(delegateNode.ReturnType);
            return delegateNode;
        }
#if !MinimalReader
        public virtual Statement VisitDoWhile(DoWhile doWhile)
        {
            if (doWhile == null) return null;
            doWhile.Invariants = this.VisitLoopInvariantList(doWhile.Invariants);
            doWhile.Body = this.VisitBlock(doWhile.Body);
            doWhile.Condition = this.VisitExpression(doWhile.Condition);
            return doWhile;
        }
#endif
        public virtual Statement VisitEndFilter(EndFilter endFilter)
        {
            if (endFilter == null) return null;
            endFilter.Value = this.VisitExpression(endFilter.Value);
            return endFilter;
        }
        public virtual Statement VisitEndFinally(EndFinally endFinally)
        {
            return endFinally;
        }
#if ExtendedRuntime
    public virtual EnsuresList VisitEnsuresList(EnsuresList Ensures) {
      if (Ensures == null) return null;
      for (int i = 0, n = Ensures.Count; i < n; i++)
        Ensures[i] = (Ensures) this.Visit(Ensures[i]);
      return Ensures;
    }
#endif
        public virtual EnumNode VisitEnumNode(EnumNode enumNode)
        {
            return (EnumNode)this.VisitTypeNode(enumNode);
        }
        public virtual Event VisitEvent(Event evnt)
        {
            if (evnt == null) return null;
            evnt.Attributes = this.VisitAttributeList(evnt.Attributes);
            evnt.HandlerType = this.VisitTypeReference(evnt.HandlerType);
            return evnt;
        }
#if ExtendedRuntime
    public virtual EnsuresExceptional VisitEnsuresExceptional(EnsuresExceptional exceptional) {
      if (exceptional == null) return null;
      exceptional.PostCondition = this.VisitExpression(exceptional.PostCondition);
      exceptional.Type = this.VisitTypeReference(exceptional.Type);
      exceptional.Variable = this.VisitExpression(exceptional.Variable);
      return exceptional;
    }
#endif
#if !MinimalReader
        public virtual Statement VisitExit(Exit exit)
        {
            return exit;
        }
        public virtual Statement VisitExpose(Expose @expose)
        {
            if (@expose == null) return null;
            @expose.Instance = this.VisitExpression(@expose.Instance);
            @expose.Body = this.VisitBlock(expose.Body);
            return expose;
        }
#endif

        public virtual Expression VisitExpression(Expression expression)
        {
            if (expression == null) return null;
            switch (expression.NodeType)
            {
                case NodeType.Dup:
                case NodeType.Arglist:
                    return expression;
                case NodeType.Pop:
                    UnaryExpression uex = expression as UnaryExpression;
                    if (uex != null)
                    {
                        uex.Operand = this.VisitExpression(uex.Operand);
                        return uex;
                    }
                    return expression;
                default:
                    return (Expression)this.Visit(expression);
            }
        }
        public override ExpressionList VisitExpressionList(ExpressionList expressions)
        {
            if (expressions == null) return null;
            for (int i = 0, n = expressions.Count; i < n; i++)
                expressions[i] = this.VisitExpression(expressions[i]);
            return expressions;
        }
#if !MinimalReader
        public virtual Expression VisitExpressionSnippet(ExpressionSnippet snippet)
        {
            return snippet;
        }
#endif
        public virtual Statement VisitExpressionStatement(ExpressionStatement statement)
        {
            if (statement == null) return null;
            statement.Expression = this.VisitExpression(statement.Expression);
            return statement;
        }
#if !MinimalReader
        public virtual Statement VisitFaultHandler(FaultHandler faultHandler)
        {
            if (faultHandler == null) return null;
            faultHandler.Block = this.VisitBlock(faultHandler.Block);
            return faultHandler;
        }
        public virtual FaultHandlerList VisitFaultHandlerList(FaultHandlerList faultHandlers)
        {
            if (faultHandlers == null) return null;
            for (int i = 0, n = faultHandlers.Count; i < n; i++)
                faultHandlers[i] = (FaultHandler)this.VisitFaultHandler(faultHandlers[i]);
            return faultHandlers;
        }
#endif
        public virtual Field VisitField(Field field)
        {
            if (field == null) return null;
            field.Attributes = this.VisitAttributeList(field.Attributes);
            field.Type = this.VisitTypeReference(field.Type);
#if !MinimalReader
            field.Initializer = this.VisitExpression(field.Initializer);
            field.ImplementedInterfaces = this.VisitInterfaceReferenceList(field.ImplementedInterfaces);
#endif
            return field;
        }
#if !MinimalReader
        public virtual Block VisitFieldInitializerBlock(FieldInitializerBlock block)
        {
            if (block == null) return null;
            block.Type = this.VisitTypeReference(block.Type);
            return this.VisitBlock(block);
        }
        public virtual FieldList VisitFieldList(FieldList fields)
        {
            if (fields == null) return null;
            for (int i = 0, n = fields.Count; i < n; i++)
                fields[i] = this.VisitField(fields[i]);
            return fields;
        }
        public virtual Statement VisitFilter(Filter filter)
        {
            if (filter == null) return null;
            filter.Expression = this.VisitExpression(filter.Expression);
            filter.Block = this.VisitBlock(filter.Block);
            return filter;
        }
        public virtual FilterList VisitFilterList(FilterList filters)
        {
            if (filters == null) return null;
            for (int i = 0, n = filters.Count; i < n; i++)
                filters[i] = (Filter)this.VisitFilter(filters[i]);
            return filters;
        }
        public virtual Statement VisitFinally(Finally Finally)
        {
            if (Finally == null) return null;
            Finally.Block = this.VisitBlock(Finally.Block);
            return Finally;
        }
        public virtual Statement VisitFixed(Fixed Fixed)
        {
            if (Fixed == null) return null;
            Fixed.Declarators = (Statement)this.Visit(Fixed.Declarators);
            Fixed.Body = this.VisitBlock(Fixed.Body);
            return Fixed;
        }
        public virtual Statement VisitFor(For For)
        {
            if (For == null) return null;
            For.Initializer = this.VisitStatementList(For.Initializer);
            For.Invariants = this.VisitLoopInvariantList(For.Invariants);
            For.Condition = this.VisitExpression(For.Condition);
            For.Incrementer = this.VisitStatementList(For.Incrementer);
            For.Body = this.VisitBlock(For.Body);
            return For;
        }
        public virtual Statement VisitForEach(ForEach forEach)
        {
            if (forEach == null) return null;
            forEach.TargetVariableType = this.VisitTypeReference(forEach.TargetVariableType);
            forEach.TargetVariable = this.VisitTargetExpression(forEach.TargetVariable);
            forEach.SourceEnumerable = this.VisitExpression(forEach.SourceEnumerable);
            forEach.InductionVariable = this.VisitTargetExpression(forEach.InductionVariable);
            forEach.Invariants = this.VisitLoopInvariantList(forEach.Invariants);
            forEach.Body = this.VisitBlock(forEach.Body);
            return forEach;
        }
        public virtual Statement VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            if (functionDeclaration == null) return null;
            functionDeclaration.Parameters = this.VisitParameterList(functionDeclaration.Parameters);
            functionDeclaration.ReturnType = this.VisitTypeReference(functionDeclaration.ReturnType);
            functionDeclaration.Body = this.VisitBlock(functionDeclaration.Body);
            return functionDeclaration;
        }
        public virtual Expression VisitTemplateInstance(TemplateInstance templateInstance)
        {
            if (templateInstance == null) return null;
            templateInstance.Expression = this.VisitExpression(templateInstance.Expression);
            templateInstance.TypeArguments = this.VisitTypeReferenceList(templateInstance.TypeArguments);
            return templateInstance;
        }
        public virtual Expression VisitStackAlloc(StackAlloc alloc)
        {
            if (alloc == null) return null;
            alloc.ElementType = this.VisitTypeReference(alloc.ElementType);
            alloc.NumberOfElements = this.VisitExpression(alloc.NumberOfElements);
            return alloc;
        }
        public virtual Statement VisitGoto(Goto Goto)
        {
            return Goto;
        }
        public virtual Statement VisitGotoCase(GotoCase gotoCase)
        {
            if (gotoCase == null) return null;
            gotoCase.CaseLabel = this.VisitExpression(gotoCase.CaseLabel);
            return gotoCase;
        }
#endif
        public virtual Expression VisitIdentifier(Identifier identifier)
        {
            return identifier;
        }
#if !MinimalReader
        public virtual Statement VisitIf(If If)
        {
            if (If == null) return null;
            If.Condition = this.VisitExpression(If.Condition);
            If.TrueBlock = this.VisitBlock(If.TrueBlock);
            If.FalseBlock = this.VisitBlock(If.FalseBlock);
            return If;
        }
        public virtual Expression VisitImplicitThis(ImplicitThis implicitThis)
        {
            return implicitThis;
        }
#endif
        public virtual Expression VisitIndexer(Indexer indexer)
        {
            if (indexer == null) return null;
            indexer.Object = this.VisitExpression(indexer.Object);
            indexer.Operands = this.VisitExpressionList(indexer.Operands);
            return indexer;
        }
        public virtual Interface VisitInterface(Interface Interface)
        {
            return (Interface)this.VisitTypeNode(Interface);
        }
        public virtual Interface VisitInterfaceReference(Interface Interface)
        {
            return (Interface)this.VisitTypeReference(Interface);
        }
        public virtual InterfaceList VisitInterfaceReferenceList(InterfaceList interfaceReferences)
        {
            if (interfaceReferences == null) return null;
            for (int i = 0, n = interfaceReferences.Count; i < n; i++)
                interfaceReferences[i] = this.VisitInterfaceReference(interfaceReferences[i]);
            return interfaceReferences;
        }
#if ExtendedRuntime
    public virtual Invariant VisitInvariant(Invariant @invariant){
      if (@invariant == null) return null;
      @invariant.Condition = VisitExpression(@invariant.Condition);
      return @invariant;
    }
    public virtual InvariantList VisitInvariantList(InvariantList invariants){
      if (invariants == null) return null;
      for (int i = 0, n = invariants.Count; i < n; i++)
        invariants[i] = this.VisitInvariant(invariants[i]);
      return invariants;
    }
    public virtual ModelfieldContract VisitModelfieldContract(ModelfieldContract mfC) {
      if (mfC == null) return null;
      mfC.Witness = this.VisitExpression(mfC.Witness);
      for (int i = 0, n = mfC.SatisfiesList.Count; i < n; i++)
        mfC.SatisfiesList[i] = this.VisitExpression(mfC.SatisfiesList[i]);
      return mfC;
    }
    public virtual ModelfieldContractList VisitModelfieldContractList(ModelfieldContractList mfCs) {
      if (mfCs == null) return null;
      for (int i = 0, n = mfCs.Count; i < n; i++)
        mfCs[i] = this.VisitModelfieldContract(mfCs[i]);
      return mfCs;
    }
#endif
        public virtual InstanceInitializer VisitInstanceInitializer(InstanceInitializer cons)
        {
            return (InstanceInitializer)this.VisitMethod(cons);
        }
#if !MinimalReader
        public virtual Statement VisitLabeledStatement(LabeledStatement lStatement)
        {
            if (lStatement == null) return null;
            lStatement.Statement = (Statement)this.Visit(lStatement.Statement);
            return lStatement;
        }
#endif
        public virtual Expression VisitLiteral(Literal literal)
        {
            return literal;
        }
        public virtual Expression VisitLocal(Local local)
        {
            if (local == null) return null;
            local.Type = this.VisitTypeReference(local.Type);
#if !MinimalReader
            LocalBinding lb = local as LocalBinding;
            if (lb != null)
            {
                Local loc = this.VisitLocal(lb.BoundLocal) as Local;
                if (loc != null)
                    lb.BoundLocal = loc;
            }
#endif
            return local;
        }
#if !MinimalReader
        public virtual Statement VisitLocalDeclarationsStatement(LocalDeclarationsStatement localDeclarations)
        {
            if (localDeclarations == null) return null;
            localDeclarations.Type = this.VisitTypeReference(localDeclarations.Type);
            localDeclarations.Declarations = this.VisitLocalDeclarationList(localDeclarations.Declarations);
            return localDeclarations;
        }
        public virtual LocalDeclarationList VisitLocalDeclarationList(LocalDeclarationList localDeclarations)
        {
            if (localDeclarations == null) return null;
            for (int i = 0, n = localDeclarations.Count; i < n; i++)
                localDeclarations[i] = this.VisitLocalDeclaration(localDeclarations[i]);
            return localDeclarations;
        }
        public virtual LocalDeclaration VisitLocalDeclaration(LocalDeclaration localDeclaration)
        {
            if (localDeclaration == null) return null;
            localDeclaration.InitialValue = this.VisitExpression(localDeclaration.InitialValue);
            return localDeclaration;
        }
        public virtual Statement VisitLock(Lock Lock)
        {
            if (Lock == null) return null;
            Lock.Guard = this.VisitExpression(Lock.Guard);
            Lock.Body = this.VisitBlock(Lock.Body);
            return Lock;
        }
        public virtual Expression VisitLRExpression(LRExpression expr)
        {
            if (expr == null) return null;
            expr.Expression = this.VisitExpression(expr.Expression);
            return expr;
        }
#endif
        public virtual Expression VisitMemberBinding(MemberBinding memberBinding)
        {
            if (memberBinding == null) return null;
            memberBinding.TargetObject = this.VisitExpression(memberBinding.TargetObject);
            return memberBinding;
        }
        public virtual MemberList VisitMemberList(MemberList members)
        {
            this.memberListNamesChanged = false;
            if (members == null) return null;
            for (int i = 0, n = members.Count; i < n; i++)
            {
                Member oldm = members[i];
                if (oldm != null)
                {
                    Identifier oldId = oldm.Name;
                    members[i] = (Member)this.Visit(oldm);
                    if (members[i] != null)
                    {
                        if (oldId != null && members[i].Name != null && members[i].Name.UniqueIdKey != oldId.UniqueIdKey)
                        {
                            this.memberListNamesChanged = true;
                        }
                    }
                }
            }
            return members;
        }
        public virtual Method VisitMethod(Method method)
        {
            if (method == null) return null;
            method.Attributes = this.VisitAttributeList(method.Attributes);
            method.ReturnAttributes = this.VisitAttributeList(method.ReturnAttributes);
            method.SecurityAttributes = this.VisitSecurityAttributeList(method.SecurityAttributes);
            method.ReturnType = this.VisitTypeReference(method.ReturnType);
#if !MinimalReader
            method.ImplementedTypes = this.VisitTypeReferenceList(method.ImplementedTypes);
#endif
            method.Parameters = this.VisitParameterList(method.Parameters);
            if (TargetPlatform.UseGenerics)
            {
                method.TemplateArguments = this.VisitTypeReferenceList(method.TemplateArguments);
                method.TemplateParameters = this.VisitTypeParameterList(method.TemplateParameters);
            }
#if ExtendedRuntime
      method.Contract = this.VisitMethodContract(method.Contract);
#endif
            method.Body = this.VisitBlock(method.Body);
            return method;
        }
        public virtual Expression VisitMethodCall(MethodCall call)
        {
            if (call == null) return null;
            call.Callee = this.VisitExpression(call.Callee);
            call.Operands = this.VisitExpressionList(call.Operands);
            call.Constraint = this.VisitTypeReference(call.Constraint);
            return call;
        }
#if !MinimalReader
        public virtual Expression VisitArglistArgumentExpression(ArglistArgumentExpression argexp)
        {
            if (argexp == null) return null;
            argexp.Operands = this.VisitExpressionList(argexp.Operands);
            return argexp;
        }
        public virtual Expression VisitArglistExpression(ArglistExpression argexp)
        {
            if (argexp == null) return null;
            return argexp;
        }
#endif
#if ExtendedRuntime
    public virtual MethodContract VisitMethodContract(MethodContract contract){
      if (contract == null) return null;
      // don't visit contract.DeclaringMethod
      // don't visit contract.OverriddenMethods
      contract.Requires = this.VisitRequiresList(contract.Requires);
      contract.Ensures = this.VisitEnsuresList(contract.Ensures);
      contract.Modifies = this.VisitExpressionList(contract.Modifies);
      return contract;
    }
#endif
        public virtual Module VisitModule(Module module)
        {
            if (module == null) return null;
            module.Attributes = this.VisitAttributeList(module.Attributes);
            module.Types = this.VisitTypeNodeList(module.Types);
            return module;
        }
        public virtual ModuleReference VisitModuleReference(ModuleReference moduleReference)
        {
            return moduleReference;
        }
#if !MinimalReader
        public virtual Expression VisitNameBinding(NameBinding nameBinding)
        {
            return nameBinding;
        }
#endif
        public virtual Expression VisitNamedArgument(NamedArgument namedArgument)
        {
            if (namedArgument == null) return null;
            namedArgument.Value = this.VisitExpression(namedArgument.Value);
            return namedArgument;
        }
#if !MinimalReader
        public virtual Namespace VisitNamespace(Namespace nspace)
        {
            if (nspace == null) return null;
            nspace.AliasDefinitions = this.VisitAliasDefinitionList(nspace.AliasDefinitions);
            nspace.UsedNamespaces = this.VisitUsedNamespaceList(nspace.UsedNamespaces);
            nspace.Attributes = this.VisitAttributeList(nspace.Attributes);
            nspace.Types = this.VisitTypeNodeList(nspace.Types);
            nspace.NestedNamespaces = this.VisitNamespaceList(nspace.NestedNamespaces);
            return nspace;
        }
        public virtual NamespaceList VisitNamespaceList(NamespaceList namespaces)
        {
            if (namespaces == null) return null;
            for (int i = 0, n = namespaces.Count; i < n; i++)
                namespaces[i] = this.VisitNamespace(namespaces[i]);
            return namespaces;
        }
#endif
#if ExtendedRuntime
    public virtual EnsuresNormal VisitEnsuresNormal(EnsuresNormal normal) {
      if (normal == null) return null;
      normal.PostCondition = this.VisitExpression(normal.PostCondition);
      return normal;
    }
    public virtual Expression VisitOldExpression(OldExpression oldExpression) {
      if (oldExpression == null) return null;
      oldExpression.expression = this.VisitExpression(oldExpression.expression);
      return oldExpression;
    }
    public virtual RequiresOtherwise VisitRequiresOtherwise(RequiresOtherwise otherwise) {
      if (otherwise == null) return null;
      otherwise.Condition = this.VisitExpression(otherwise.Condition);
      otherwise.ThrowException = this.VisitExpression(otherwise.ThrowException);
      return otherwise;
    }
    public virtual RequiresPlain VisitRequiresPlain(RequiresPlain plain) {
      if (plain == null) return null;
      plain.Condition = this.VisitExpression(plain.Condition);
      return plain;
    }
#endif
        public virtual Expression VisitParameter(Parameter parameter)
        {
            if (parameter == null) return null;
            parameter.Attributes = this.VisitAttributeList(parameter.Attributes);
            parameter.Type = this.VisitTypeReference(parameter.Type);
            parameter.DefaultValue = this.VisitExpression(parameter.DefaultValue);
#if !MinimalReader
            ParameterBinding pb = parameter as ParameterBinding;
            if (pb != null)
            {
                Parameter par = this.VisitParameter(pb.BoundParameter) as Parameter;
                if (par != null)
                    pb.BoundParameter = par;
            }
#endif
            return parameter;
        }
        public virtual ParameterList VisitParameterList(ParameterList parameterList)
        {
            if (parameterList == null) return null;
            for (int i = 0, n = parameterList.Count; i < n; i++)
                parameterList[i] = (Parameter)this.VisitParameter(parameterList[i]);
            return parameterList;
        }
#if !MinimalReader
        public virtual Expression VisitPrefixExpression(PrefixExpression pExpr)
        {
            if (pExpr == null) return null;
            pExpr.Expression = this.VisitExpression(pExpr.Expression);
            return pExpr;
        }
        public virtual Expression VisitPostfixExpression(PostfixExpression pExpr)
        {
            if (pExpr == null) return null;
            pExpr.Expression = this.VisitExpression(pExpr.Expression);
            return pExpr;
        }
#endif
        public virtual Property VisitProperty(Property property)
        {
            if (property == null) return null;
            property.Attributes = this.VisitAttributeList(property.Attributes);
            property.Parameters = this.VisitParameterList(property.Parameters);
            property.Type = this.VisitTypeReference(property.Type);
            return property;
        }
#if !MinimalReader
        public virtual Expression VisitQuantifier(Quantifier quantifier)
        {
            if (quantifier == null) return null;
            quantifier.Comprehension = (Comprehension)this.VisitComprehension(quantifier.Comprehension);
            return quantifier;
        }
        public virtual Expression VisitComprehension(Comprehension comprehension)
        {
            if (comprehension == null) return null;
            comprehension.BindingsAndFilters = this.VisitExpressionList(comprehension.BindingsAndFilters);
            comprehension.Elements = this.VisitExpressionList(comprehension.Elements);
            return comprehension;
        }
        public virtual ComprehensionBinding VisitComprehensionBinding(ComprehensionBinding comprehensionBinding)
        {
            if (comprehensionBinding == null) return null;
            comprehensionBinding.TargetVariableType = this.VisitTypeReference(comprehensionBinding.TargetVariableType);
            comprehensionBinding.TargetVariable = this.VisitTargetExpression(comprehensionBinding.TargetVariable);
            comprehensionBinding.AsTargetVariableType = this.VisitTypeReference(comprehensionBinding.AsTargetVariableType);
            comprehensionBinding.SourceEnumerable = this.VisitExpression(comprehensionBinding.SourceEnumerable);
            return comprehensionBinding;
        }
        public virtual Expression VisitQualifiedIdentifier(QualifiedIdentifier qualifiedIdentifier)
        {
            if (qualifiedIdentifier == null) return null;
            qualifiedIdentifier.Qualifier = this.VisitExpression(qualifiedIdentifier.Qualifier);
            return qualifiedIdentifier;
        }
        public virtual Expression VisitRefValueExpression(RefValueExpression refvalexp)
        {
            if (refvalexp == null) return null;
            refvalexp.Operand1 = this.VisitExpression(refvalexp.Operand1);
            refvalexp.Operand2 = this.VisitExpression(refvalexp.Operand2);
            return refvalexp;
        }
        public virtual Expression VisitRefTypeExpression(RefTypeExpression reftypexp)
        {
            if (reftypexp == null) return null;
            reftypexp.Operand = this.VisitExpression(reftypexp.Operand);
            return reftypexp;
        }
        public virtual Statement VisitRepeat(Repeat repeat)
        {
            if (repeat == null) return null;
            repeat.Body = this.VisitBlock(repeat.Body);
            repeat.Condition = this.VisitExpression(repeat.Condition);
            return repeat;
        }
#endif
#if ExtendedRuntime
    public virtual RequiresList VisitRequiresList(RequiresList Requires) {
      if (Requires == null) return null;
      for (int i = 0, n = Requires.Count; i < n; i++)
        Requires[i] = (Requires) this.Visit(Requires[i]);
      return Requires;
    }
#endif
        public virtual Statement VisitReturn(Return Return)
        {
            if (Return == null) return null;
            Return.Expression = this.VisitExpression(Return.Expression);
            return Return;
        }
#if !MinimalReader
        public virtual Statement VisitAcquire(Acquire @acquire)
        {
            if (@acquire == null) return null;
            @acquire.Target = (Statement)this.Visit(@acquire.Target);
            @acquire.Condition = this.VisitExpression(@acquire.Condition);
            @acquire.ConditionFunction = this.VisitExpression(@acquire.ConditionFunction);
            @acquire.Body = this.VisitBlock(@acquire.Body);
            return @acquire;
        }
        public virtual Statement VisitResourceUse(ResourceUse resourceUse)
        {
            if (resourceUse == null) return null;
            resourceUse.ResourceAcquisition = (Statement)this.Visit(resourceUse.ResourceAcquisition);
            resourceUse.Body = this.VisitBlock(resourceUse.Body);
            return resourceUse;
        }
#endif
        public virtual SecurityAttribute VisitSecurityAttribute(SecurityAttribute attribute)
        {
            return attribute;
        }
        public virtual SecurityAttributeList VisitSecurityAttributeList(SecurityAttributeList attributes)
        {
            if (attributes == null) return null;
            for (int i = 0, n = attributes.Count; i < n; i++)
                attributes[i] = this.VisitSecurityAttribute(attributes[i]);
            return attributes;
        }
#if !MinimalReader
        public virtual Expression VisitSetterValue(SetterValue value)
        {
            return value;
        }
#endif
        public virtual StatementList VisitStatementList(StatementList statements)
        {
            if (statements == null) return null;
            for (int i = 0, n = statements.Count; i < n; i++)
                statements[i] = (Statement)this.Visit(statements[i]);
            return statements;
        }
#if !MinimalReader
        public virtual StatementSnippet VisitStatementSnippet(StatementSnippet snippet)
        {
            return snippet;
        }
#endif
        public virtual StaticInitializer VisitStaticInitializer(StaticInitializer cons)
        {
            return (StaticInitializer)this.VisitMethod(cons);
        }
        public virtual Struct VisitStruct(Struct Struct)
        {
            return (Struct)this.VisitTypeNode(Struct);
        }
#if !MinimalReader
        public virtual Statement VisitSwitch(Switch Switch)
        {
            if (Switch == null) return null;
            Switch.Expression = this.VisitExpression(Switch.Expression);
            Switch.Cases = this.VisitSwitchCaseList(Switch.Cases);
            return Switch;
        }
        public virtual SwitchCase VisitSwitchCase(SwitchCase switchCase)
        {
            if (switchCase == null) return null;
            switchCase.Label = this.VisitExpression(switchCase.Label);
            switchCase.Body = this.VisitBlock(switchCase.Body);
            return switchCase;
        }
        public virtual SwitchCaseList VisitSwitchCaseList(SwitchCaseList switchCases)
        {
            if (switchCases == null) return null;
            for (int i = 0, n = switchCases.Count; i < n; i++)
                switchCases[i] = this.Visit(switchCases[i]) as SwitchCase;
            return switchCases;
        }
#endif
        public virtual Statement VisitSwitchInstruction(SwitchInstruction switchInstruction)
        {
            if (switchInstruction == null) return null;
            switchInstruction.Expression = this.VisitExpression(switchInstruction.Expression);
            return switchInstruction;
        }
#if !MinimalReader
        public virtual Statement VisitTypeswitch(Typeswitch Typeswitch)
        {
            if (Typeswitch == null) return null;
            Typeswitch.Expression = this.VisitExpression(Typeswitch.Expression);
            Typeswitch.Cases = this.VisitTypeswitchCaseList(Typeswitch.Cases);
            return Typeswitch;
        }
        public virtual TypeswitchCase VisitTypeswitchCase(TypeswitchCase typeswitchCase)
        {
            if (typeswitchCase == null) return null;
            typeswitchCase.LabelType = this.VisitTypeReference(typeswitchCase.LabelType);
            typeswitchCase.LabelVariable = this.VisitTargetExpression(typeswitchCase.LabelVariable);
            typeswitchCase.Body = this.VisitBlock(typeswitchCase.Body);
            return typeswitchCase;
        }
        public virtual TypeswitchCaseList VisitTypeswitchCaseList(TypeswitchCaseList typeswitchCases)
        {
            if (typeswitchCases == null) return null;
            for (int i = 0, n = typeswitchCases.Count; i < n; i++)
                typeswitchCases[i] = this.VisitTypeswitchCase(typeswitchCases[i]);
            return typeswitchCases;
        }
#endif
        public virtual Expression VisitTargetExpression(Expression expression)
        {
            return this.VisitExpression(expression);
        }
        public virtual Expression VisitTernaryExpression(TernaryExpression expression)
        {
            if (expression == null) return null;
            expression.Operand1 = this.VisitExpression(expression.Operand1);
            expression.Operand2 = this.VisitExpression(expression.Operand2);
            expression.Operand3 = this.VisitExpression(expression.Operand3);
            return expression;
        }
        public virtual Expression VisitThis(This This)
        {
            if (This == null) return null;
            This.Type = this.VisitTypeReference(This.Type);
#if !MinimalReader
            ThisBinding tb = This as ThisBinding;
            if (tb != null)
            {
                This boundThis = this.VisitThis(tb.BoundThis) as This;
                if (boundThis != null)
                    tb.BoundThis = boundThis;
            }
#endif
            return This;
        }
        public virtual Statement VisitThrow(Throw Throw)
        {
            if (Throw == null) return null;
            Throw.Expression = this.VisitExpression(Throw.Expression);
            return Throw;
        }
#if !MinimalReader
        public virtual Statement VisitTry(Try Try)
        {
            if (Try == null) return null;
            Try.TryBlock = this.VisitBlock(Try.TryBlock);
            Try.Catchers = this.VisitCatchList(Try.Catchers);
            Try.Filters = this.VisitFilterList(Try.Filters);
            Try.FaultHandlers = this.VisitFaultHandlerList(Try.FaultHandlers);
            Try.Finally = (Finally)this.VisitFinally(Try.Finally);
            return Try;
        }
#endif
#if ExtendedRuntime    
    public virtual TupleType VisitTupleType(TupleType tuple){
      return (TupleType)this.VisitTypeNode(tuple);
    }
    public virtual TypeAlias VisitTypeAlias(TypeAlias tAlias){
      if (tAlias == null) return null;
      if (tAlias.AliasedType is ConstrainedType)
        //The type alias defines the constrained type, rather than just referencing it
        tAlias.AliasedType = this.VisitConstrainedType((ConstrainedType)tAlias.AliasedType);
      else
        tAlias.AliasedType = this.VisitTypeReference(tAlias.AliasedType);
      return tAlias;
    }
    public virtual TypeContract VisitTypeContract(TypeContract contract){
      if (contract == null) return null;
      // don't visit contract.DeclaringType
      // don't visit contract.InheritedContracts
      contract.Invariants = this.VisitInvariantList(contract.Invariants);
      contract.ModelfieldContracts = this.VisitModelfieldContractList(contract.ModelfieldContracts);
      return contract;
    }

    public virtual TypeIntersection VisitTypeIntersection(TypeIntersection typeIntersection){
      return (TypeIntersection)this.VisitTypeNode(typeIntersection);
    }
#endif
#if !MinimalReader
        public virtual TypeMemberSnippet VisitTypeMemberSnippet(TypeMemberSnippet snippet)
        {
            return snippet;
        }
#endif
        public virtual TypeModifier VisitTypeModifier(TypeModifier typeModifier)
        {
            if (typeModifier == null) return null;
            typeModifier.Modifier = this.VisitTypeReference(typeModifier.Modifier);
            typeModifier.ModifiedType = this.VisitTypeReference(typeModifier.ModifiedType);
            return typeModifier;
        }
        public virtual TypeNode VisitTypeNode(TypeNode typeNode)
        {
            if (typeNode == null) return null;
            typeNode.Attributes = this.VisitAttributeList(typeNode.Attributes);
            typeNode.SecurityAttributes = this.VisitSecurityAttributeList(typeNode.SecurityAttributes);
            Class c = typeNode as Class;
            if (c != null) c.BaseClass = (Class)this.VisitTypeReference(c.BaseClass);
            typeNode.Interfaces = this.VisitInterfaceReferenceList(typeNode.Interfaces);
            typeNode.TemplateArguments = this.VisitTypeReferenceList(typeNode.TemplateArguments);
            typeNode.TemplateParameters = this.VisitTypeParameterList(typeNode.TemplateParameters);
            this.VisitMemberList(typeNode.Members);
            if (this.memberListNamesChanged) { typeNode.ClearMemberTable(); }
#if ExtendedRuntime
      // have to visit this *after* visiting the members since in Normalizer
      // it creates normalized method bodies for the invariant methods and
      // those shouldn't be visited again!!
      // REVIEW!! I don't think the method bodies created in Normalizer are necessarily normalized anymore!!
      typeNode.Contract = this.VisitTypeContract(typeNode.Contract);
#endif
            return typeNode;
        }
        public virtual TypeNodeList VisitTypeNodeList(TypeNodeList types)
        {
            if (types == null) return null;
            for (int i = 0; i < types.Count; i++) //Visiting a type may result in a new type being appended to this list
                types[i] = (TypeNode)this.Visit(types[i]);
            return types;
        }
        public virtual TypeNode VisitTypeParameter(TypeNode typeParameter)
        {
            if (typeParameter == null) return null;
            Class cl = typeParameter as Class;
            if (cl != null) cl.BaseClass = (Class)this.VisitTypeReference(cl.BaseClass);
            typeParameter.Attributes = this.VisitAttributeList(typeParameter.Attributes);
            typeParameter.Interfaces = this.VisitInterfaceReferenceList(typeParameter.Interfaces);
            return typeParameter;
        }
        public virtual TypeNodeList VisitTypeParameterList(TypeNodeList typeParameters)
        {
            if (typeParameters == null) return null;
            for (int i = 0, n = typeParameters.Count; i < n; i++)
                typeParameters[i] = this.VisitTypeParameter(typeParameters[i]);
            return typeParameters;
        }
        public virtual TypeNode VisitTypeReference(TypeNode type)
        {
            return type;
        }
#if !MinimalReader
        public virtual TypeReference VisitTypeReference(TypeReference type)
        {
            return type;
        }
#endif
        public virtual TypeNodeList VisitTypeReferenceList(TypeNodeList typeReferences)
        {
            if (typeReferences == null) return null;
            for (int i = 0, n = typeReferences.Count; i < n; i++)
                typeReferences[i] = this.VisitTypeReference(typeReferences[i]);
            return typeReferences;
        }
#if ExtendedRuntime 
    public virtual TypeUnion VisitTypeUnion(TypeUnion typeUnion){
      return (TypeUnion)this.VisitTypeNode(typeUnion);
    }
#endif
        public virtual Expression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            if (unaryExpression == null) return null;
            unaryExpression.Operand = this.VisitExpression(unaryExpression.Operand);
            return unaryExpression;
        }
#if !MinimalReader
        public virtual Statement VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            if (variableDeclaration == null) return null;
            variableDeclaration.Type = this.VisitTypeReference(variableDeclaration.Type);
            variableDeclaration.Initializer = this.VisitExpression(variableDeclaration.Initializer);
            return variableDeclaration;
        }
        public virtual UsedNamespace VisitUsedNamespace(UsedNamespace usedNamespace)
        {
            return usedNamespace;
        }
        public virtual UsedNamespaceList VisitUsedNamespaceList(UsedNamespaceList usedNspaces)
        {
            if (usedNspaces == null) return null;
            for (int i = 0, n = usedNspaces.Count; i < n; i++)
                usedNspaces[i] = this.VisitUsedNamespace(usedNspaces[i]);
            return usedNspaces;
        }
        public virtual ExpressionList VisitLoopInvariantList(ExpressionList expressions)
        {
            if (expressions == null) return null;
            for (int i = 0, n = expressions.Count; i < n; i++)
                expressions[i] = this.VisitExpression(expressions[i]);
            return expressions;
        }
        public virtual Statement VisitWhile(While While)
        {
            if (While == null) return null;
            While.Condition = this.VisitExpression(While.Condition);
            While.Invariants = this.VisitLoopInvariantList(While.Invariants);
            While.Body = this.VisitBlock(While.Body);
            return While;
        }
        public virtual Statement VisitYield(Yield Yield)
        {
            if (Yield == null) return null;
            Yield.Expression = this.VisitExpression(Yield.Expression);
            return Yield;
        }
#endif
#if ExtendedRuntime
    // query nodes
    public virtual Node VisitQueryAggregate(QueryAggregate qa){
      if (qa == null) return null;
      qa.Expression = this.VisitExpression(qa.Expression);
      return qa;
    }
    public virtual Node VisitQueryAlias(QueryAlias alias){
      if (alias == null) return null;
      alias.Expression = this.VisitExpression(alias.Expression);
      return alias;
    }
    public virtual Node VisitQueryAxis(QueryAxis axis){
      if (axis == null) return null;
      axis.Source = this.VisitExpression( axis.Source );
      return axis;
    }
    public virtual Node VisitQueryCommit(QueryCommit qc){
      return qc;
    }
    public virtual Node VisitQueryContext(QueryContext context){
      return context;
    }
    public virtual Node VisitQueryDelete(QueryDelete delete){
      if (delete == null) return null;
      delete.Source = this.VisitExpression(delete.Source);
      /*delete.Target =*/ this.VisitExpression(delete.Target); //REVIEW: why should this not be updated?
      return delete;
    }
    public virtual Node VisitQueryDifference(QueryDifference diff){
      if (diff == null) return null;
      diff.LeftSource = this.VisitExpression(diff.LeftSource);
      diff.RightSource = this.VisitExpression(diff.RightSource);
      return diff;
    }
    public virtual Node VisitQueryDistinct(QueryDistinct distinct){
      if (distinct == null) return null;
      distinct.Source = this.VisitExpression(distinct.Source);
      return distinct;
    }
    public virtual Node VisitQueryExists(QueryExists exists){
      if (exists == null) return null;
      exists.Source = this.VisitExpression(exists.Source);
      return exists;
    }
    public virtual Node VisitQueryFilter(QueryFilter filter){
      if (filter == null) return null;
      filter.Source = this.VisitExpression(filter.Source);
      filter.Expression = this.VisitExpression(filter.Expression);
      return filter;
    }
    public virtual Node VisitQueryGroupBy(QueryGroupBy groupby){
      if (groupby == null) return null;
      groupby.Source = this.VisitExpression(groupby.Source);
      groupby.GroupList = this.VisitExpressionList(groupby.GroupList);
      groupby.Having = this.VisitExpression(groupby.Having);
      return groupby;
    }
    public virtual Statement VisitQueryGeneratedType(QueryGeneratedType qgt){
      return qgt;
    }
    public virtual Node VisitQueryInsert(QueryInsert insert){
      if (insert == null) return null;
      insert.Location = this.VisitExpression(insert.Location);
      insert.HintList = this.VisitExpressionList(insert.HintList);
      insert.InsertList = this.VisitExpressionList(insert.InsertList);
      return insert;
    }
    public virtual Node VisitQueryIntersection(QueryIntersection intersection){
      if (intersection == null) return intersection;
      intersection.LeftSource = this.VisitExpression(intersection.LeftSource);
      intersection.RightSource = this.VisitExpression(intersection.RightSource);
      intersection.Type = intersection.LeftSource == null ? null : intersection.LeftSource.Type;
      return intersection;
    }
    public virtual Node VisitQueryIterator(QueryIterator xiterator){
      if (xiterator == null) return xiterator;
      xiterator.Expression = this.VisitExpression(xiterator.Expression);
      xiterator.HintList = this.VisitExpressionList(xiterator.HintList);
      return xiterator;      
    }
    public virtual Node VisitQueryJoin(QueryJoin join){
      if (join == null) return null;
      join.LeftOperand = this.VisitExpression(join.LeftOperand);
      join.RightOperand = this.VisitExpression(join.RightOperand);
      join.JoinExpression = this.VisitExpression(join.JoinExpression);
      return join;
    }
    public virtual Node VisitQueryLimit(QueryLimit limit){
      if (limit == null) return null;
      limit.Source = this.VisitExpression(limit.Source);
      limit.Expression = this.VisitExpression(limit.Expression);
      return limit;
    }
    public virtual Node VisitQueryOrderBy(QueryOrderBy orderby){
      if (orderby == null) return null;
      orderby.Source = this.VisitExpression(orderby.Source);
      orderby.OrderList = this.VisitExpressionList(orderby.OrderList);
      return orderby;
    }
    public virtual Node VisitQueryOrderItem(QueryOrderItem item){
      if (item == null) return null;
      item.Expression = this.VisitExpression(item.Expression);
      return item;
    }
    public virtual Node VisitQueryPosition(QueryPosition position){
      return position;
    }
    public virtual Node VisitQueryProject(QueryProject project){
      if (project == null) return null;
      project.Source = this.VisitExpression(project.Source);
      project.ProjectionList = this.VisitExpressionList(project.ProjectionList);
      return project;
    }
    public virtual Node VisitQueryRollback(QueryRollback qr){
      return qr;
    }
    public virtual Node VisitQueryQuantifier(QueryQuantifier qq){
      if (qq == null) return null;
      qq.Expression = this.VisitExpression(qq.Expression);
      return qq;
    }
    public virtual Node VisitQueryQuantifiedExpression(QueryQuantifiedExpression qqe){
      if (qqe == null) return null;
      qqe.Expression = this.VisitExpression(qqe.Expression);
      return qqe;
    }
    public virtual Node VisitQuerySelect(QuerySelect select){
      if (select == null) return null;
      select.Source = this.VisitExpression(select.Source);
      return select;
    }
    public virtual Node VisitQuerySingleton(QuerySingleton singleton){
      if (singleton == null) return null;
      singleton.Source = this.VisitExpression(singleton.Source);
      return singleton;
    }
    public virtual Node VisitQueryTransact(QueryTransact qt){
      if (qt == null) return null;
      qt.Source = this.VisitExpression(qt.Source);
      qt.Body = this.VisitBlock(qt.Body);
      qt.CommitBody = this.VisitBlock(qt.CommitBody);
      qt.RollbackBody = this.VisitBlock(qt.RollbackBody);
      return qt;
    }
    public virtual Node VisitQueryTypeFilter(QueryTypeFilter filter){
      if (filter == null) return null;
      filter.Source = this.VisitExpression(filter.Source);
      return filter;
    }
    public virtual Node VisitQueryUnion(QueryUnion union){
      if (union == null) return null;
      union.LeftSource = this.VisitExpression(union.LeftSource);
      union.RightSource = this.VisitExpression(union.RightSource);
      return union;
    }
    public virtual Node VisitQueryUpdate(QueryUpdate update){
      if (update == null) return null;
      update.Source = this.VisitExpression(update.Source);
      update.UpdateList = this.VisitExpressionList(update.UpdateList);
      return update;
    }    
    public virtual Node VisitQueryYielder(QueryYielder yielder){
      if (yielder == null) return null;
      yielder.Source = this.VisitExpression(yielder.Source);
      yielder.Target = this.VisitExpression(yielder.Target);
      yielder.Body = this.VisitBlock(yielder.Body);
      return yielder;
    }
#endif
#if !MinimalReader
        /// <summary>
        /// Return a type viewer for the current scope.
        /// [The type viewer acts like the identity function, except for dialects (e.g. Extensible Sing#)
        /// that allow extensions and differing views of types.]
        /// null can be returned to represent an identity-function type viewer.
        /// </summary>
        public virtual TypeViewer TypeViewer
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// Return the current scope's view of the argument type, by asking the current scope's type viewer.
        /// </summary>
        public virtual TypeNode/*!*/ GetTypeView(TypeNode/*!*/ type)
        {
            return TypeViewer.GetTypeView(this.TypeViewer, type);
        }
#endif
    }
#if !MinimalReader
    /// <summary>
    /// Provides methods for invoking a parser to obtain AST nodes corresponding to various types of code snippets.
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Parses the parser's source document as an entire compilation unit and adds the resulting AST nodes to the
        /// Nodes list of the given CompilationUnit instance.
        /// </summary>
        /// <param name="compilationUnit">The compilation unit whose Nodes list will receive the AST root node(s) that the parser produces.</param>
        void ParseCompilationUnit(CompilationUnit compilationUnit);
        Expression ParseExpression();
        void ParseStatements(StatementList statements);
        void ParseTypeMembers(TypeNode type);
    }
    ///<summary>Provides a way for general purpose code to construct parsers using an standard interface. 
    ///Useful for base classes without complete knowledge of all the different kinds of parsers that might be used in an application.</summary>
    public interface IParserFactory
    {
        IParser CreateParser(string fileName, int lineNumber, DocumentText text, Module symbolTable, ErrorNodeList errorNodes, CompilerParameters options);
    }
    public class SnippetParser : StandardVisitor
    {
        public IParserFactory/*!*/ DefaultParserFactory;
        public ErrorNodeList ErrorNodes;
        public Module SymbolTable;
        public StatementList CurrentStatementList;
        public CompilerParameters Options;

        public SnippetParser(IParserFactory/*!*/ defaultParserFactory, Module symbolTable, ErrorNodeList errorNodes, CompilerParameters options)
        {
            this.DefaultParserFactory = defaultParserFactory;
            this.ErrorNodes = errorNodes;
            this.SymbolTable = symbolTable;
            this.Options = options;
            this.CurrentStatementList = new StatementList(0);
            //^ base();
        }

        public override Node VisitUnknownNodeType(Node node)
        {
            return node; //Do not look for snippets inside unknown node types
        }
        public override Block VisitBlock(Block block)
        {
            if (block == null) return null;
            StatementList savedStatementList = this.CurrentStatementList;
            try
            {
                StatementList oldStatements = block.Statements;
                int n = oldStatements == null ? 0 : oldStatements.Count;
                StatementList newStatements = this.CurrentStatementList = block.Statements = new StatementList(n);
                for (int i = 0; i < n; i++)
                {
                    //^ assert oldStatements != null;
                    newStatements.Add((Statement)this.Visit(oldStatements[i]));
                }
                return block;
            }
            finally
            {
                this.CurrentStatementList = savedStatementList;
            }
        }
        public override CompilationUnit VisitCompilationUnitSnippet(CompilationUnitSnippet snippet)
        {
            if (snippet == null) return null;
            Document doc = snippet.SourceContext.Document;
            if (doc == null) return null;
            string fileName = doc.Name;
            int lineNumber = doc.LineNumber;
            DocumentText sourceText = doc.Text;
            IParserFactory pf = snippet.ParserFactory;
            IParser p;
            if (pf == null)
                p = this.DefaultParserFactory.CreateParser(fileName, lineNumber, sourceText, this.SymbolTable, this.ErrorNodes, this.Options);
            else
                p = pf.CreateParser(fileName, lineNumber, sourceText, this.SymbolTable, this.ErrorNodes, this.Options);
            if (p == null) return null;
            p.ParseCompilationUnit(snippet);
            return snippet;
        }
        public override Expression VisitExpressionSnippet(ExpressionSnippet snippet)
        {
            if (snippet == null) return null;
            Document doc = snippet.SourceContext.Document;
            if (doc == null) return null;
            string fileName = doc.Name;
            int lineNumber = doc.LineNumber;
            DocumentText sourceText = doc.Text;
            IParserFactory pf = snippet.ParserFactory;
            IParser p;
            if (pf == null)
                p = this.DefaultParserFactory.CreateParser(fileName, lineNumber, sourceText, this.SymbolTable, this.ErrorNodes, this.Options);
            else
                p = pf.CreateParser(fileName, lineNumber, sourceText, this.SymbolTable, this.ErrorNodes, this.Options);
            if (p == null) return null;
            return p.ParseExpression();
        }
        public override StatementSnippet VisitStatementSnippet(StatementSnippet snippet)
        {
            if (snippet == null) return null;
            Document doc = snippet.SourceContext.Document;
            if (doc == null) return null;
            string fileName = doc.Name;
            int lineNumber = doc.LineNumber;
            DocumentText sourceText = doc.Text;
            IParserFactory pf = snippet.ParserFactory;
            IParser p;
            if (pf == null)
                p = this.DefaultParserFactory.CreateParser(fileName, lineNumber, sourceText, this.SymbolTable, this.ErrorNodes, this.Options);
            else
                p = pf.CreateParser(fileName, lineNumber, sourceText, this.SymbolTable, this.ErrorNodes, this.Options);
            if (p == null) return null;
            p.ParseStatements(this.CurrentStatementList);
            return null;
        }
        public override TypeMemberSnippet VisitTypeMemberSnippet(TypeMemberSnippet snippet)
        {
            if (snippet == null) return null;
            Document doc = snippet.SourceContext.Document;
            if (doc == null) return null;
            string fileName = doc.Name;
            int lineNumber = doc.LineNumber;
            DocumentText sourceText = doc.Text;
            IParserFactory pf = snippet.ParserFactory;
            IParser p;
            if (pf == null)
                p = this.DefaultParserFactory.CreateParser(fileName, lineNumber, sourceText, this.SymbolTable, this.ErrorNodes, this.Options);
            else
                p = pf.CreateParser(fileName, lineNumber, sourceText, this.SymbolTable, this.ErrorNodes, this.Options);
            if (p == null) return null;
            p.ParseTypeMembers(snippet.DeclaringType);
            return null;
        }
    }
#endif
}
