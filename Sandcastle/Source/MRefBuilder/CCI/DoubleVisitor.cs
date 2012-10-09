// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

#if !MinimalReader
using System;
using System.Collections;
using System.Diagnostics;

#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler
{
#endif
    /// <summary>
    /// Base for all classes that process two IR trees, possibily transforming one of them.
    /// </summary>
    public abstract class DoubleVisitor
    {
        /// <summary>
        /// Switches on node.NodeType to call a visitor method that has been specialized for node.
        /// </summary>
        /// <returns> Returns null if node1 is null. Otherwise returns an updated node (possibly a different object).</returns>
        public abstract Node Visit(Node node1, Node node2);

        /// <summary>
        /// Transfers the state from one visitor to another. This enables separate visitor instances to cooperative process a single IR.
        /// </summary>
        public virtual void TransferStateTo(DoubleVisitor targetVisitor)
        {
        }
    }

    /// <summary>
    /// Walks an IR, mutuating it into a new form
    /// </summary>   
    public abstract class StandardDoubleVisitor : DoubleVisitor
    {
        public DoubleVisitor callingVisitor;

        protected StandardDoubleVisitor()
        {
        }
        protected StandardDoubleVisitor(DoubleVisitor callingVisitor)
        {
            this.callingVisitor = callingVisitor;
        }
        public virtual Node VisitUnknownNodeType(Node node1, Node node2)
        {
            if (node1 == null) { Debug.Fail(""); return null; }
            DoubleVisitor visitor = this.GetVisitorFor(node1);
            if (visitor == null) return node1;
            if (this.callingVisitor != null)
                //Allow specialized state (unknown to this visitor) to propagate all the way down to the new visitor
                this.callingVisitor.TransferStateTo(visitor);
            this.TransferStateTo(visitor);
            node1 = visitor.Visit(node1, node2);
            visitor.TransferStateTo(this);
            if (this.callingVisitor != null)
                //Propagate specialized state (unknown to this visitor) all the way up the chain
                visitor.TransferStateTo(this.callingVisitor);
            return node1;
        }
        public virtual DoubleVisitor GetVisitorFor(Node/*!*/ node1)
        {
            if (node1 == null) { Debug.Fail(""); return null; }
            return (DoubleVisitor)node1.GetVisitorFor(this, this.GetType().Name);
        }
        public override Node Visit(Node node1, Node node2)
        {
            if (node1 == null) return null;
            switch (node1.NodeType)
            {
                case NodeType.AddressDereference:
                    return this.VisitAddressDereference((AddressDereference)node1, node2 as AddressDereference);
                case NodeType.AliasDefinition:
                    return this.VisitAliasDefinition((AliasDefinition)node1, node2 as AliasDefinition);
                case NodeType.AnonymousNestedFunction:
                    return this.VisitAnonymousNestedFunction((AnonymousNestedFunction)node1, node2 as AnonymousNestedFunction);
                case NodeType.ApplyToAll:
                    return this.VisitApplyToAll((ApplyToAll)node1, node2 as ApplyToAll);
                case NodeType.Arglist:
                    return this.VisitExpression((Expression)node1, node2 as Expression);
                case NodeType.ArrayType:
                    Debug.Assert(false); return null;
                case NodeType.Assembly:
                    return this.VisitAssembly((AssemblyNode)node1, node2 as AssemblyNode);
                case NodeType.AssemblyReference:
                    return this.VisitAssemblyReference((AssemblyReference)node1, node2 as AssemblyReference);
                case NodeType.Assertion:
                    return this.VisitAssertion((Assertion)node1, node2 as Assertion);
                case NodeType.Assumption:
                    return this.VisitAssumption((Assumption)node1, node2 as Assumption);
                case NodeType.AssignmentExpression:
                    return this.VisitAssignmentExpression((AssignmentExpression)node1, node2 as AssignmentExpression);
                case NodeType.AssignmentStatement:
                    return this.VisitAssignmentStatement((AssignmentStatement)node1, node2 as AssignmentStatement);
                case NodeType.Attribute:
                    return this.VisitAttributeNode((AttributeNode)node1, node2 as AttributeNode);
                case NodeType.Base:
                    return this.VisitBase((Base)node1, node2 as Base);
                case NodeType.Block:
                    return this.VisitBlock((Block)node1, node2 as Block);
                case NodeType.BlockExpression:
                    return this.VisitBlockExpression((BlockExpression)node1, node2 as BlockExpression);
                case NodeType.Branch:
                    return this.VisitBranch((Branch)node1, node2 as Branch);
                case NodeType.Compilation:
                    return this.VisitCompilation((Compilation)node1, node2 as Compilation);
                case NodeType.CompilationUnit:
                    return this.VisitCompilationUnit((CompilationUnit)node1, node2 as CompilationUnit);
                case NodeType.CompilationUnitSnippet:
                    return this.VisitCompilationUnitSnippet((CompilationUnitSnippet)node1, node2 as CompilationUnitSnippet);
#if ExtendedRuntime
        case NodeType.ConstrainedType:
          return this.VisitConstrainedType((ConstrainedType)node1, node2 as ConstrainedType);
#endif
                case NodeType.Continue:
                    return this.VisitContinue((Continue)node1, node2 as Continue);
                case NodeType.CurrentClosure:
                    return this.VisitCurrentClosure((CurrentClosure)node1, node2 as CurrentClosure);
                case NodeType.DebugBreak:
                    return node1;
                case NodeType.Call:
                case NodeType.Calli:
                case NodeType.Callvirt:
                case NodeType.Jmp:
                case NodeType.MethodCall:
                    return this.VisitMethodCall((MethodCall)node1, node2 as MethodCall);
                case NodeType.Catch:
                    return this.VisitCatch((Catch)node1, node2 as Catch);
                case NodeType.Class:
                    return this.VisitClass((Class)node1, node2 as Class);
                case NodeType.CoerceTuple:
                    return this.VisitCoerceTuple((CoerceTuple)node1, node2 as CoerceTuple);
                case NodeType.CollectionEnumerator:
                    return this.VisitCollectionEnumerator((CollectionEnumerator)node1, node2 as CollectionEnumerator);
                case NodeType.Composition:
                    return this.VisitComposition((Composition)node1, node2 as Composition);
                case NodeType.Construct:
                    return this.VisitConstruct((Construct)node1, node2 as Construct);
                case NodeType.ConstructArray:
                    return this.VisitConstructArray((ConstructArray)node1, node2 as ConstructArray);
                case NodeType.ConstructDelegate:
                    return this.VisitConstructDelegate((ConstructDelegate)node1, node2 as ConstructDelegate);
                case NodeType.ConstructFlexArray:
                    return this.VisitConstructFlexArray((ConstructFlexArray)node1, node2 as ConstructFlexArray);
                case NodeType.ConstructIterator:
                    return this.VisitConstructIterator((ConstructIterator)node1, node2 as ConstructIterator);
                case NodeType.ConstructTuple:
                    return this.VisitConstructTuple((ConstructTuple)node1, node2 as ConstructTuple);
                case NodeType.DelegateNode:
                    return this.VisitDelegateNode((DelegateNode)node1, node2 as DelegateNode);
                case NodeType.DoWhile:
                    return this.VisitDoWhile((DoWhile)node1, node2 as DoWhile);
                case NodeType.Dup:
                    return this.VisitExpression((Expression)node1, node2 as Expression);
                case NodeType.EndFilter:
                    return this.VisitEndFilter((EndFilter)node1, node2 as EndFilter);
                case NodeType.EndFinally:
                    return this.VisitEndFinally((EndFinally)node1, node2 as EndFinally);
                case NodeType.EnumNode:
                    return this.VisitEnumNode((EnumNode)node1, node2 as EnumNode);
                case NodeType.Event:
                    return this.VisitEvent((Event)node1, node2 as Event);
#if ExtendedRuntime
        case NodeType.EnsuresExceptional :
          return this.VisitEnsuresExceptional((EnsuresExceptional)node1, node2 as EnsuresExceptional);
#endif
                case NodeType.Exit:
                    return this.VisitExit((Exit)node1, node2 as Exit);
                case NodeType.Read:
                case NodeType.Write:
                    return this.VisitExpose((Expose)node1, node2 as Expose);
                case NodeType.ExpressionSnippet:
                    return this.VisitExpressionSnippet((ExpressionSnippet)node1, node2 as ExpressionSnippet);
                case NodeType.ExpressionStatement:
                    return this.VisitExpressionStatement((ExpressionStatement)node1, node2 as ExpressionStatement);
                case NodeType.FaultHandler:
                    return this.VisitFaultHandler((FaultHandler)node1, node2 as FaultHandler);
                case NodeType.Field:
                    return this.VisitField((Field)node1, node2 as Field);
                case NodeType.FieldInitializerBlock:
                    return this.VisitFieldInitializerBlock((FieldInitializerBlock)node1, node2 as FieldInitializerBlock);
                case NodeType.Finally:
                    return this.VisitFinally((Finally)node1, node2 as Finally);
                case NodeType.Filter:
                    return this.VisitFilter((Filter)node1, node2 as Filter);
                case NodeType.Fixed:
                    return this.VisitFixed((Fixed)node1, node2 as Fixed);
                case NodeType.For:
                    return this.VisitFor((For)node1, node2 as For);
                case NodeType.ForEach:
                    return this.VisitForEach((ForEach)node1, node2 as ForEach);
                case NodeType.FunctionDeclaration:
                    return this.VisitFunctionDeclaration((FunctionDeclaration)node1, node2 as FunctionDeclaration);
                case NodeType.Goto:
                    return this.VisitGoto((Goto)node1, node2 as Goto);
                case NodeType.GotoCase:
                    return this.VisitGotoCase((GotoCase)node1, node2 as GotoCase);
                case NodeType.Identifier:
                    return this.VisitIdentifier((Identifier)node1, node2 as Identifier);
                case NodeType.If:
                    return this.VisitIf((If)node1, node2 as If);
                case NodeType.ImplicitThis:
                    return this.VisitImplicitThis((ImplicitThis)node1, node2 as ImplicitThis);
                case NodeType.Indexer:
                    return this.VisitIndexer((Indexer)node1, node2 as Indexer);
                case NodeType.InstanceInitializer:
                    return this.VisitInstanceInitializer((InstanceInitializer)node1, node2 as InstanceInitializer);
                case NodeType.Interface:
                    return this.VisitInterface((Interface)node1, node2 as Interface);
#if ExtendedRuntime
        case NodeType.Invariant :
          return this.VisitInvariant((Invariant)node1, node2 as Invariant);
#endif
                case NodeType.LabeledStatement:
                    return this.VisitLabeledStatement((LabeledStatement)node1, node2 as LabeledStatement);
                case NodeType.Literal:
                    return this.VisitLiteral((Literal)node1, node2 as Literal);
                case NodeType.Local:
                    return this.VisitLocal((Local)node1, node2 as Local);
                case NodeType.LocalDeclaration:
                    return this.VisitLocalDeclaration((LocalDeclaration)node1, node2 as LocalDeclaration);
                case NodeType.LocalDeclarationsStatement:
                    return this.VisitLocalDeclarationsStatement((LocalDeclarationsStatement)node1, node2 as LocalDeclarationsStatement);
                case NodeType.Lock:
                    return this.VisitLock((Lock)node1, node2 as Lock);
                case NodeType.LRExpression:
                    return this.VisitLRExpression((LRExpression)node1, node2 as LRExpression);
                case NodeType.MemberBinding:
                    return this.VisitMemberBinding((MemberBinding)node1, node2 as MemberBinding);
                case NodeType.Method:
                    return this.VisitMethod((Method)node1, node2 as Method);
#if ExtendedRuntime
        case NodeType.MethodContract :
          return this.VisitMethodContract((MethodContract)node1, node2 as MethodContract);
#endif
                case NodeType.TemplateInstance:
                    return this.VisitTemplateInstance((TemplateInstance)node1, node2 as TemplateInstance);
                case NodeType.StackAlloc:
                    return this.VisitStackAlloc((StackAlloc)node1, node2 as StackAlloc);
                case NodeType.Module:
                    return this.VisitModule((Module)node1, node2 as Module);
                case NodeType.ModuleReference:
                    return this.VisitModuleReference((ModuleReference)node1, node2 as ModuleReference);
                case NodeType.NameBinding:
                    return this.VisitNameBinding((NameBinding)node1, node2 as NameBinding);
                case NodeType.NamedArgument:
                    return this.VisitNamedArgument((NamedArgument)node1, node2 as NamedArgument);
                case NodeType.Namespace:
                    return this.VisitNamespace((Namespace)node1, node2 as Namespace);
                case NodeType.Nop:
                    return node1;
#if ExtendedRuntime
        case NodeType.EnsuresNormal :
          return this.VisitEnsuresNormal((EnsuresNormal)node1, node2 as EnsuresNormal);
        case NodeType.OldExpression :
          return this.VisitOldExpression((OldExpression)node1, node2 as OldExpression);
        case NodeType.RequiresOtherwise :
          return this.VisitRequiresOtherwise((RequiresOtherwise)node1, node2 as RequiresOtherwise);
        case NodeType.RequiresPlain :
          return this.VisitRequiresPlain((RequiresPlain)node1, node2 as RequiresPlain);
#endif
                case NodeType.OptionalModifier:
                case NodeType.RequiredModifier:
                    return this.VisitTypeModifier((TypeModifier)node1, node2 as TypeModifier);
                case NodeType.Parameter:
                    return this.VisitParameter((Parameter)node1, node2 as Parameter);
                case NodeType.Pop:
                    return this.VisitExpression((Expression)node1, node2 as Expression);
                case NodeType.PrefixExpression:
                    return this.VisitPrefixExpression((PrefixExpression)node1, node2 as PrefixExpression);
                case NodeType.PostfixExpression:
                    return this.VisitPostfixExpression((PostfixExpression)node1, node2 as PostfixExpression);
                case NodeType.Property:
                    return this.VisitProperty((Property)node1, node2 as Property);
                case NodeType.Quantifier:
                    return this.VisitQuantifier((Quantifier)node1, node2 as Quantifier);
                case NodeType.Comprehension:
                    return this.VisitComprehension((Comprehension)node1, node2 as Comprehension);
                case NodeType.ComprehensionBinding:
                    return this.VisitComprehensionBinding((ComprehensionBinding)node1, node2 as ComprehensionBinding);
                case NodeType.QualifiedIdentifer:
                    return this.VisitQualifiedIdentifier((QualifiedIdentifier)node1, node2 as QualifiedIdentifier);
                case NodeType.Rethrow:
                case NodeType.Throw:
                    return this.VisitThrow((Throw)node1, node2 as Throw);
                case NodeType.Return:
                    return this.VisitReturn((Return)node1, node2 as Return);
                case NodeType.ResourceUse:
                    return this.VisitResourceUse((ResourceUse)node1, node2 as ResourceUse);
                case NodeType.Repeat:
                    return this.VisitRepeat((Repeat)node1, node2 as Repeat);
                case NodeType.SecurityAttribute:
                    return this.VisitSecurityAttribute((SecurityAttribute)node1, node2 as SecurityAttribute);
                case NodeType.SetterValue:
                    return this.VisitSetterValue((SetterValue)node1, node2 as SetterValue);
                case NodeType.StaticInitializer:
                    return this.VisitStaticInitializer((StaticInitializer)node1, node2 as StaticInitializer);
                case NodeType.StatementSnippet:
                    return this.VisitStatementSnippet((StatementSnippet)node1, node2 as StatementSnippet);
                case NodeType.Struct:
                    return this.VisitStruct((Struct)node1, node2 as Struct);
                case NodeType.Switch:
                    return this.VisitSwitch((Switch)node1, node2 as Switch);
                case NodeType.SwitchCase:
                    return this.VisitSwitchCase((SwitchCase)node1, node2 as SwitchCase);
                case NodeType.SwitchInstruction:
                    return this.VisitSwitchInstruction((SwitchInstruction)node1, node2 as SwitchInstruction);
                case NodeType.Typeswitch:
                    return this.VisitTypeswitch((Typeswitch)node1, node2 as Typeswitch);
                case NodeType.TypeswitchCase:
                    return this.VisitTypeswitchCase((TypeswitchCase)node1, node2 as TypeswitchCase);
                case NodeType.This:
                    return this.VisitThis((This)node1, node2 as This);
                case NodeType.Try:
                    return this.VisitTry((Try)node1, node2 as Try);
#if ExtendedRuntime
        case NodeType.TupleType:
          return this.VisitTupleType((TupleType)node1, node2 as TupleType);
        case NodeType.TypeAlias:
          return this.VisitTypeAlias((TypeAlias)node1, node2 as TypeAlias);
        case NodeType.TypeIntersection:
          return this.VisitTypeIntersection((TypeIntersection)node1, node2 as TypeIntersection);
        case NodeType.TypeContract :
          return this.VisitTypeContract((TypeContract)node1, node2 as TypeContract);
#endif
                case NodeType.TypeMemberSnippet:
                    return this.VisitTypeMemberSnippet((TypeMemberSnippet)node1, node2 as TypeMemberSnippet);
                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                    return this.VisitTypeParameter((TypeNode)node1, node2 as TypeNode);
#if ExtendedRuntime
        case NodeType.TypeUnion:
          return this.VisitTypeUnion((TypeUnion)node1, node2 as TypeUnion);
#endif
                case NodeType.TypeReference:
                    return this.VisitTypeReference((TypeReference)node1, node2 as TypeReference);
                case NodeType.UsedNamespace:
                    return this.VisitUsedNamespace((UsedNamespace)node1, node2 as UsedNamespace);
                case NodeType.VariableDeclaration:
                    return this.VisitVariableDeclaration((VariableDeclaration)node1, node2 as VariableDeclaration);
                case NodeType.While:
                    return this.VisitWhile((While)node1, node2 as While);
                case NodeType.Yield:
                    return this.VisitYield((Yield)node1, node2 as Yield);

                case NodeType.Conditional:
                case NodeType.Cpblk:
                case NodeType.Initblk:
                    return this.VisitTernaryExpression((TernaryExpression)node1, node2 as TernaryExpression);

                case NodeType.Add:
                case NodeType.Add_Ovf:
                case NodeType.Add_Ovf_Un:
                case NodeType.AddEventHandler:
                case NodeType.And:
                case NodeType.As:
                case NodeType.Box:
                case NodeType.Castclass:
                case NodeType.Ceq:
                case NodeType.Cgt:
                case NodeType.Cgt_Un:
                case NodeType.Clt:
                case NodeType.Clt_Un:
                case NodeType.Comma:
                case NodeType.Div:
                case NodeType.Div_Un:
                case NodeType.Eq:
                case NodeType.ExplicitCoercion:
                case NodeType.Ge:
                case NodeType.Gt:
                case NodeType.Is:
                case NodeType.Iff:
                case NodeType.Implies:
                case NodeType.Isinst:
                case NodeType.Ldvirtftn:
                case NodeType.Le:
                case NodeType.LogicalAnd:
                case NodeType.LogicalOr:
                case NodeType.Lt:
                case NodeType.Mkrefany:
                case NodeType.Maplet:
                case NodeType.Mul:
                case NodeType.Mul_Ovf:
                case NodeType.Mul_Ovf_Un:
                case NodeType.Ne:
                case NodeType.Or:
                case NodeType.Range:
                case NodeType.Refanyval:
                case NodeType.Rem:
                case NodeType.Rem_Un:
                case NodeType.RemoveEventHandler:
                case NodeType.Shl:
                case NodeType.Shr:
                case NodeType.Shr_Un:
                case NodeType.Sub:
                case NodeType.Sub_Ovf:
                case NodeType.Sub_Ovf_Un:
                case NodeType.Unbox:
                case NodeType.UnboxAny:
                case NodeType.Xor:
                    return this.VisitBinaryExpression((BinaryExpression)node1, node2 as BinaryExpression);

                case NodeType.AddressOf:
                case NodeType.OutAddress:
                case NodeType.RefAddress:
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
                case NodeType.Decrement:
                case NodeType.DefaultValue:
                case NodeType.Increment:
                case NodeType.Ldftn:
                case NodeType.Ldlen:
                case NodeType.Ldtoken:
                case NodeType.Localloc:
                case NodeType.LogicalNot:
                case NodeType.Neg:
                case NodeType.Not:
                case NodeType.Parentheses:
                case NodeType.Refanytype:
                case NodeType.Sizeof:
                case NodeType.SkipCheck:
                case NodeType.Typeof:
                case NodeType.UnaryPlus:
                    return this.VisitUnaryExpression((UnaryExpression)node1, node2 as UnaryExpression);
#if ExtendedRuntime
          // query node1 types
        case NodeType.QueryAggregate:
          return this.VisitQueryAggregate((QueryAggregate)node1, node2 as QueryAggregate);
        case NodeType.QueryAlias:
          return this.VisitQueryAlias((QueryAlias)node1, node2 as QueryAlias);
        case NodeType.QueryAll:
        case NodeType.QueryAny:
          return this.VisitQueryQuantifier((QueryQuantifier)node1, node2 as QueryQuantifier);
        case NodeType.QueryAxis:
          return this.VisitQueryAxis((QueryAxis)node1, node2 as QueryAxis);
        case NodeType.QueryCommit:
          return this.VisitQueryCommit((QueryCommit)node1, node2 as QueryCommit);
        case NodeType.QueryContext:
          return this.VisitQueryContext((QueryContext)node1, node2 as QueryContext);
        case NodeType.QueryDelete:
          return this.VisitQueryDelete((QueryDelete)node1, node2 as QueryDelete);
        case NodeType.QueryDifference:
          return this.VisitQueryDifference((QueryDifference)node1, node2 as QueryDifference);
        case NodeType.QueryDistinct:
          return this.VisitQueryDistinct((QueryDistinct)node1, node2 as QueryDistinct);
        case NodeType.QueryExists:
          return this.VisitQueryExists((QueryExists)node1, node2 as QueryExists);
        case NodeType.QueryFilter:
          return this.VisitQueryFilter((QueryFilter)node1, node2 as QueryFilter);
        case NodeType.QueryGeneratedType:
          return this.VisitQueryGeneratedType((QueryGeneratedType)node1, node2 as QueryGeneratedType);
        case NodeType.QueryGroupBy:
          return this.VisitQueryGroupBy((QueryGroupBy)node1, node2 as QueryGroupBy);
        case NodeType.QueryInsert:
          return this.VisitQueryInsert((QueryInsert)node1, node2 as QueryInsert);
        case NodeType.QueryIntersection:
          return this.VisitQueryIntersection((QueryIntersection)node1, node2 as QueryIntersection);
        case NodeType.QueryIterator:
          return this.VisitQueryIterator((QueryIterator)node1, node2 as QueryIterator);
        case NodeType.QueryJoin:
          return this.VisitQueryJoin((QueryJoin)node1, node2 as QueryJoin);
        case NodeType.QueryLimit:
          return this.VisitQueryLimit((QueryLimit)node1, node2 as QueryLimit);
        case NodeType.QueryOrderBy:        
          return this.VisitQueryOrderBy((QueryOrderBy)node1, node2 as QueryOrderBy);
        case NodeType.QueryOrderItem:
          return this.VisitQueryOrderItem((QueryOrderItem)node1, node2 as QueryOrderItem);
        case NodeType.QueryPosition:
          return this.VisitQueryPosition((QueryPosition)node1, node2 as QueryPosition);
        case NodeType.QueryProject:
          return this.VisitQueryProject((QueryProject)node1, node2 as QueryProject);          
        case NodeType.QueryQuantifiedExpression:
          return this.VisitQueryQuantifiedExpression((QueryQuantifiedExpression)node1, node2 as QueryQuantifiedExpression);
        case NodeType.QueryRollback:
          return this.VisitQueryRollback((QueryRollback)node1, node2 as QueryRollback);
        case NodeType.QuerySelect:
          return this.VisitQuerySelect((QuerySelect)node1, node2 as QuerySelect);
        case NodeType.QuerySingleton:
          return this.VisitQuerySingleton((QuerySingleton)node1, node2 as QuerySingleton);
        case NodeType.QueryTransact:
          return this.VisitQueryTransact((QueryTransact)node1, node2 as QueryTransact);
        case NodeType.QueryTypeFilter:
          return this.VisitQueryTypeFilter((QueryTypeFilter)node1, node2 as QueryTypeFilter);
        case NodeType.QueryUnion:
          return this.VisitQueryUnion((QueryUnion)node1, node2 as QueryUnion);
        case NodeType.QueryUpdate:
          return this.VisitQueryUpdate((QueryUpdate)node1, node2 as QueryUpdate);
        case NodeType.QueryYielder:
          return this.VisitQueryYielder((QueryYielder)node1, node2 as QueryYielder);
#endif
                default:
                    return this.VisitUnknownNodeType(node1, node2);
            }
        }
        public virtual Expression VisitAddressDereference(AddressDereference addr1, AddressDereference addr2)
        {
            if (addr1 == null) return null;
            if (addr2 == null)
                addr1.Address = this.VisitExpression(addr1.Address, null);
            else
                addr1.Address = this.VisitExpression(addr1.Address, addr2.Address);
            return addr1;
        }
        public virtual AliasDefinition VisitAliasDefinition(AliasDefinition aliasDefinition1, AliasDefinition aliasDefinition2)
        {
            if (aliasDefinition1 == null) return null;
            if (aliasDefinition2 == null)
                aliasDefinition1.AliasedType = this.VisitTypeReference(aliasDefinition1.AliasedType, null);
            else
                aliasDefinition1.AliasedType = this.VisitTypeReference(aliasDefinition1.AliasedType, aliasDefinition2.AliasedType);
            return aliasDefinition1;
        }
        public virtual AliasDefinitionList VisitAliasDefinitionList(AliasDefinitionList aliasDefinitions1, AliasDefinitionList aliasDefinitions2)
        {
            if (aliasDefinitions1 == null) return null;
            for (int i = 0, n = aliasDefinitions1.Count, m = aliasDefinitions2 == null ? 0 : aliasDefinitions2.Count; i < n; i++)
            {
                //^ assert aliasDefinitions2 != null;
                if (i >= m)
                    aliasDefinitions1[i] = this.VisitAliasDefinition(aliasDefinitions1[i], null);
                else
                    aliasDefinitions1[i] = this.VisitAliasDefinition(aliasDefinitions1[i], aliasDefinitions2[i]);
            }
            return aliasDefinitions1;
        }
        public virtual Expression VisitAnonymousNestedFunction(AnonymousNestedFunction func1, AnonymousNestedFunction func2)
        {
            if (func1 == null) return null;
            if (func2 == null)
            {
                func1.Parameters = this.VisitParameterList(func1.Parameters, null);
                func1.Body = this.VisitBlock(func1.Body, null);
            }
            else
            {
                func1.Parameters = this.VisitParameterList(func1.Parameters, func2.Parameters);
                func1.Body = this.VisitBlock(func1.Body, func2.Body);
            }
            return func1;
        }
        public virtual Expression VisitApplyToAll(ApplyToAll applyToAll1, ApplyToAll applyToAll2)
        {
            if (applyToAll1 == null) return null;
            if (applyToAll2 == null)
            {
                applyToAll1.Operand1 = this.VisitExpression(applyToAll1.Operand1, null);
                applyToAll1.Operand2 = this.VisitExpression(applyToAll1.Operand2, null);
            }
            else
            {
                applyToAll1.Operand1 = this.VisitExpression(applyToAll1.Operand1, applyToAll2.Operand1);
                applyToAll1.Operand2 = this.VisitExpression(applyToAll1.Operand2, applyToAll2.Operand2);
            }
            return applyToAll1;
        }
        public ArrayType VisitArrayType(ArrayType array1, ArrayType array2)
        {
            Debug.Assert(false, "An array type exists only at runtime. It should be referred to, but never visited.");
            return null;
        }
        public virtual AssemblyNode VisitAssembly(AssemblyNode assembly1, AssemblyNode assembly2)
        {
            if (assembly1 == null) return null;
            this.VisitModule(assembly1, assembly2);
            if (assembly2 == null)
            {
                assembly1.ModuleAttributes = this.VisitAttributeList(assembly1.ModuleAttributes, null);
                assembly1.SecurityAttributes = this.VisitSecurityAttributeList(assembly1.SecurityAttributes, null);
            }
            else
            {
                assembly1.ModuleAttributes = this.VisitAttributeList(assembly1.ModuleAttributes, assembly2.ModuleAttributes);
                assembly1.SecurityAttributes = this.VisitSecurityAttributeList(assembly1.SecurityAttributes, assembly2.SecurityAttributes);
            }
            return assembly1;
        }
        public virtual AssemblyReference VisitAssemblyReference(AssemblyReference assemblyReference1, AssemblyReference assemblyReference2)
        {
            return assemblyReference1;
        }
        public virtual Statement VisitAssertion(Assertion assertion1, Assertion assertion2)
        {
            if (assertion1 == null) return null;
            if (assertion2 == null)
                assertion1.Condition = this.VisitExpression(assertion1.Condition, null);
            else
                assertion1.Condition = this.VisitExpression(assertion1.Condition, assertion2.Condition);
            return assertion1;
        }
        public virtual Statement VisitAssumption(Assumption Assumption1, Assumption Assumption2)
        {
            if (Assumption1 == null) return null;
            if (Assumption2 == null)
                Assumption1.Condition = this.VisitExpression(Assumption1.Condition, null);
            else
                Assumption1.Condition = this.VisitExpression(Assumption1.Condition, Assumption2.Condition);
            return Assumption1;
        }
        public virtual Expression VisitAssignmentExpression(AssignmentExpression assignment1, AssignmentExpression assignment2)
        {
            if (assignment1 == null) return null;
            if (assignment2 == null)
                assignment1.AssignmentStatement = (Statement)this.Visit(assignment1.AssignmentStatement, null);
            else
                assignment1.AssignmentStatement = (Statement)this.Visit(assignment1.AssignmentStatement, assignment2.AssignmentStatement);
            return assignment1;
        }
        public virtual Statement VisitAssignmentStatement(AssignmentStatement assignment1, AssignmentStatement assignment2)
        {
            if (assignment1 == null) return null;
            if (assignment2 == null)
            {
                assignment1.Target = this.VisitTargetExpression(assignment1.Target, null);
                assignment1.Source = this.VisitExpression(assignment1.Source, null);
            }
            else
            {
                assignment1.Target = this.VisitTargetExpression(assignment1.Target, assignment2.Target);
                assignment1.Source = this.VisitExpression(assignment1.Source, assignment2.Source);
            }
            return assignment1;
        }
        public virtual Expression VisitAttributeConstructor(AttributeNode attribute1, AttributeNode attribute2)
        {
            if (attribute1 == null) return null;
            if (attribute2 == null)
                return this.VisitExpression(attribute1.Constructor, null);
            else
                return this.VisitExpression(attribute1.Constructor, attribute2.Constructor);
        }
        public virtual AttributeNode VisitAttributeNode(AttributeNode attribute1, AttributeNode attribute2)
        {
            if (attribute1 == null) return null;
            if (attribute2 == null)
            {
                attribute1.Constructor = this.VisitAttributeConstructor(attribute1, null);
                attribute1.Expressions = this.VisitExpressionList(attribute1.Expressions, null);
            }
            else
            {
                attribute1.Constructor = this.VisitAttributeConstructor(attribute1, attribute2);
                attribute1.Expressions = this.VisitExpressionList(attribute1.Expressions, attribute2.Expressions);
            }
            return attribute1;
        }
        public virtual AttributeList VisitAttributeList(AttributeList attributes1, AttributeList attributes2)
        {
            if (attributes1 == null) return null;
            for (int i = 0, n = attributes1.Count, m = attributes2 == null ? 0 : attributes2.Count; i < n; i++)
            {
                //^ assert attributes2 != null;
                if (i >= m)
                    attributes1[i] = this.VisitAttributeNode(attributes1[i], null);
                else
                    attributes1[i] = this.VisitAttributeNode(attributes1[i], attributes2[i]);
            }
            return attributes1;
        }
        public virtual Expression VisitBase(Base Base1, Base Base2)
        {
            return Base1;
        }
        public virtual Expression VisitBinaryExpression(BinaryExpression binaryExpression1, BinaryExpression binaryExpression2)
        {
            if (binaryExpression1 == null) return null;
            if (binaryExpression2 == null)
            {
                binaryExpression1.Operand1 = this.VisitExpression(binaryExpression1.Operand1, null);
                binaryExpression1.Operand2 = this.VisitExpression(binaryExpression1.Operand2, null);
            }
            else
            {
                binaryExpression1.Operand1 = this.VisitExpression(binaryExpression1.Operand1, binaryExpression2.Operand1);
                binaryExpression1.Operand2 = this.VisitExpression(binaryExpression1.Operand2, binaryExpression2.Operand2);
            }
            return binaryExpression1;
        }
        public virtual Block VisitBlock(Block block1, Block block2)
        {
            if (block1 == null) return null;
            if (block2 == null)
                block1.Statements = this.VisitStatementList(block1.Statements, null);
            else
                block1.Statements = this.VisitStatementList(block1.Statements, block2.Statements);
            return block1;
        }
        public virtual Expression VisitBlockExpression(BlockExpression blockExpression1, BlockExpression blockExpression2)
        {
            if (blockExpression1 == null) return null;
            if (blockExpression2 == null)
                blockExpression1.Block = this.VisitBlock(blockExpression1.Block, null);
            else
                blockExpression1.Block = this.VisitBlock(blockExpression1.Block, blockExpression2.Block);
            return blockExpression1;
        }
        public virtual BlockList VisitBlockList(BlockList blockList1, BlockList blockList2)
        {
            if (blockList1 == null) return null;
            for (int i = 0, n = blockList1.Count, m = blockList2 == null ? 0 : blockList2.Count; i < n; i++)
            {
                //^ assert blockList2 != null;
                if (i >= m)
                    blockList1[i] = this.VisitBlock(blockList1[i], null);
                else
                    blockList1[i] = this.VisitBlock(blockList1[i], blockList2[i]);
            }
            return blockList1;
        }
        public virtual Statement VisitBranch(Branch branch1, Branch branch2)
        {
            if (branch1 == null) return null;
            if (branch2 == null)
                branch1.Condition = this.VisitExpression(branch1.Condition, null);
            else
                branch1.Condition = this.VisitExpression(branch1.Condition, branch2.Condition);
            return branch1;
        }
        public virtual Statement VisitCatch(Catch Catch1, Catch Catch2)
        {
            if (Catch1 == null) return null;
            if (Catch2 == null)
            {
                Catch1.Variable = this.VisitTargetExpression(Catch1.Variable, null);
                Catch1.Type = this.VisitTypeReference(Catch1.Type, null);
                Catch1.Block = this.VisitBlock(Catch1.Block, null);
            }
            else
            {
                Catch1.Variable = this.VisitTargetExpression(Catch1.Variable, Catch2.Variable);
                Catch1.Type = this.VisitTypeReference(Catch1.Type, Catch2.Type);
                Catch1.Block = this.VisitBlock(Catch1.Block, Catch2.Block);
            }
            return Catch1;
        }
        public virtual CatchList VisitCatchList(CatchList catchers1, CatchList catchers2)
        {
            if (catchers1 == null) return null;
            for (int i = 0, n = catchers1.Count, m = catchers2 == null ? 0 : catchers2.Count; i < n; i++)
            {
                //^ assert catchers2 != null;
                if (i >= m)
                    catchers1[i] = (Catch)this.VisitCatch(catchers1[i], null);
                else
                    catchers1[i] = (Catch)this.VisitCatch(catchers1[i], catchers2[i]);
            }
            return catchers1;
        }
        public virtual Class VisitClass(Class Class1, Class Class2)
        {
            return (Class)this.VisitTypeNode(Class1, Class2);
        }
        public virtual Expression VisitCoerceTuple(CoerceTuple coerceTuple1, CoerceTuple coerceTuple2)
        {
            if (coerceTuple1 == null) return null;
            if (coerceTuple2 == null)
                coerceTuple1.OriginalTuple = this.VisitExpression(coerceTuple1.OriginalTuple, null);
            else
                coerceTuple1.OriginalTuple = this.VisitExpression(coerceTuple1.OriginalTuple, coerceTuple2.OriginalTuple);
            return this.VisitConstructTuple(coerceTuple1, coerceTuple2);
        }
        public virtual CollectionEnumerator VisitCollectionEnumerator(CollectionEnumerator ce1, CollectionEnumerator ce2)
        {
            if (ce1 == null) return null;
            if (ce2 == null)
                ce1.Collection = this.VisitExpression(ce1.Collection, null);
            else
                ce1.Collection = this.VisitExpression(ce1.Collection, ce2.Collection);
            return ce1;
        }
        public virtual Compilation VisitCompilation(Compilation compilation1, Compilation compilation2)
        {
            if (compilation1 == null) return null;
            Module module1 = compilation1.TargetModule;
            AssemblyNode assem1 = module1 as AssemblyNode;
            Module module2 = compilation2 == null ? null : compilation2.TargetModule;
            AssemblyNode assem2 = module2 as AssemblyNode;
            if (module1 != null)
            {
                if (module2 == null)
                    module1.Attributes = this.VisitAttributeList(module1.Attributes, null);
                else
                    module1.Attributes = this.VisitAttributeList(module1.Attributes, module2.Attributes);
            }
            if (assem1 != null)
            {
                if (assem2 == null)
                    assem1.ModuleAttributes = this.VisitAttributeList(assem1.ModuleAttributes, null);
                else
                    assem1.ModuleAttributes = this.VisitAttributeList(assem1.ModuleAttributes, assem2.ModuleAttributes);
            }
            compilation1.CompilationUnits = this.VisitCompilationUnitList(compilation1.CompilationUnits, compilation2 == null ? null : compilation2.CompilationUnits);
            return null;
        }
        public virtual CompilationUnit VisitCompilationUnit(CompilationUnit cUnit1, CompilationUnit cUnit2)
        {
            if (cUnit1 == null) return null;
            cUnit1.Nodes = this.VisitNodeList(cUnit1.Nodes, cUnit2 == null ? null : cUnit2.Nodes);
            return cUnit1;
        }
        public virtual CompilationUnitList VisitCompilationUnitList(CompilationUnitList cUnits1, CompilationUnitList cUnits2)
        {
            if (cUnits1 == null) return null;
            for (int i = 0, n = cUnits1.Count, m = cUnits2 == null ? 0 : cUnits2.Count; i < n; i++)
            {
                //^ assert cUnits2 != null;
                if (i >= m)
                    cUnits1[i] = (CompilationUnit)this.VisitCompilationUnit(cUnits1[i], null);
                else
                    cUnits1[i] = (CompilationUnit)this.VisitCompilationUnit(cUnits1[i], cUnits2[i]);
            }
            return cUnits1;
        }
        public virtual CompilationUnitSnippet VisitCompilationUnitSnippet(CompilationUnitSnippet snippet1, CompilationUnitSnippet snippet2)
        {
            return snippet1;
        }
        public virtual Node VisitComposition(Composition comp1, Composition comp2)
        {
            if (comp1 == null) return null;
            if (comp1.GetType() == typeof(Composition))
            {
                comp1.Expression = (Expression)this.Visit(comp1.Expression, comp2 == null ? null : comp2.Expression);
                return comp1;
            }
            return this.VisitUnknownNodeType(comp1, comp2);
        }
        public virtual Expression VisitConstruct(Construct cons1, Construct cons2)
        {
            if (cons1 == null) return null;
            if (cons2 == null)
            {
                cons1.Constructor = this.VisitExpression(cons1.Constructor, null);
                cons1.Operands = this.VisitExpressionList(cons1.Operands, null);
                cons1.Owner = this.VisitExpression(cons1.Owner, null);
            }
            else
            {
                cons1.Constructor = this.VisitExpression(cons1.Constructor, cons2.Constructor);
                cons1.Operands = this.VisitExpressionList(cons1.Operands, cons2.Operands);
                cons1.Owner = this.VisitExpression(cons1.Owner, cons2.Owner);
            }
            return cons1;
        }
        public virtual Expression VisitConstructArray(ConstructArray consArr1, ConstructArray consArr2)
        {
            if (consArr1 == null) return null;
            if (consArr2 == null)
            {
                consArr1.ElementType = this.VisitTypeReference(consArr1.ElementType, null);
                consArr1.Operands = this.VisitExpressionList(consArr1.Operands, null);
                consArr1.Initializers = this.VisitExpressionList(consArr1.Initializers, null);
                consArr1.Owner = this.VisitExpression(consArr1.Owner, null);
            }
            else
            {
                consArr1.ElementType = this.VisitTypeReference(consArr1.ElementType, consArr2.ElementType);
                consArr1.Operands = this.VisitExpressionList(consArr1.Operands, consArr2.Operands);
                consArr1.Initializers = this.VisitExpressionList(consArr1.Initializers, consArr2.Initializers);
                consArr1.Owner = this.VisitExpression(consArr1.Owner, consArr2.Owner);
            }
            return consArr1;
        }
        public virtual Expression VisitConstructDelegate(ConstructDelegate consDelegate1, ConstructDelegate consDelegate2)
        {
            if (consDelegate1 == null) return null;
            if (consDelegate2 == null)
            {
                consDelegate1.DelegateType = this.VisitTypeReference(consDelegate1.DelegateType, null);
                consDelegate1.TargetObject = this.VisitExpression(consDelegate1.TargetObject, null);
            }
            else
            {
                consDelegate1.DelegateType = this.VisitTypeReference(consDelegate1.DelegateType, consDelegate2.DelegateType);
                consDelegate1.TargetObject = this.VisitExpression(consDelegate1.TargetObject, consDelegate2.TargetObject);
            }
            return consDelegate1;
        }
        public virtual Expression VisitConstructFlexArray(ConstructFlexArray consArr1, ConstructFlexArray consArr2)
        {
            if (consArr1 == null) return null;
            if (consArr2 == null)
            {
                consArr1.ElementType = this.VisitTypeReference(consArr1.ElementType, null);
                consArr1.Operands = this.VisitExpressionList(consArr1.Operands, null);
                consArr1.Initializers = this.VisitExpressionList(consArr1.Initializers, null);
            }
            else
            {
                consArr1.ElementType = this.VisitTypeReference(consArr1.ElementType, consArr2.ElementType);
                consArr1.Operands = this.VisitExpressionList(consArr1.Operands, consArr2.Operands);
                consArr1.Initializers = this.VisitExpressionList(consArr1.Initializers, consArr2.Initializers);
            }
            return consArr1;
        }
        public virtual Expression VisitConstructIterator(ConstructIterator consIterator1, ConstructIterator consIterator2)
        {
            return consIterator1;
        }
        public virtual Expression VisitConstructTuple(ConstructTuple consTuple1, ConstructTuple consTuple2)
        {
            if (consTuple1 == null) return null;
            if (consTuple2 == null)
                consTuple1.Fields = this.VisitFieldList(consTuple1.Fields, null);
            else
                consTuple1.Fields = this.VisitFieldList(consTuple1.Fields, consTuple2.Fields);
            return consTuple1;
        }
#if ExtendedRuntime    
    public virtual TypeNode VisitConstrainedType(ConstrainedType cType1, ConstrainedType cType2){
      if (cType1 == null) return null;
      if (cType2 == null){
        cType1.UnderlyingType = this.VisitTypeReference(cType1.UnderlyingType, null);
        cType1.Constraint = this.VisitExpression(cType1.Constraint, null);
      }else{
        cType1.UnderlyingType = this.VisitTypeReference(cType1.UnderlyingType, cType2.UnderlyingType);
        cType1.Constraint = this.VisitExpression(cType1.Constraint, cType2.Constraint);
      }
      return cType1;
    }
#endif
        public virtual Statement VisitContinue(Continue Continue1, Continue Continue2)
        {
            return Continue1;
        }
        public virtual Expression VisitCurrentClosure(CurrentClosure currentClosure1, CurrentClosure currentClosure2)
        {
            return currentClosure1;
        }
        public virtual DelegateNode VisitDelegateNode(DelegateNode delegateNode1, DelegateNode delegateNode2)
        {
            if (delegateNode1 == null) return null;
            if (delegateNode2 == null)
            {
                delegateNode1 = (DelegateNode)this.VisitTypeNode(delegateNode1, null);
                if (delegateNode1 == null) return null;
                delegateNode1.Parameters = this.VisitParameterList(delegateNode1.Parameters, null);
                delegateNode1.ReturnType = this.VisitTypeReference(delegateNode1.ReturnType, null);
            }
            else
            {
                delegateNode1 = (DelegateNode)this.VisitTypeNode(delegateNode1, delegateNode2);
                if (delegateNode1 == null) return null;
                delegateNode1.Parameters = this.VisitParameterList(delegateNode1.Parameters, delegateNode2.Parameters);
                delegateNode1.ReturnType = this.VisitTypeReference(delegateNode1.ReturnType, delegateNode2.ReturnType);
            }
            return delegateNode1;
        }
        public virtual Statement VisitDoWhile(DoWhile doWhile1, DoWhile doWhile2)
        {
            if (doWhile1 == null) return null;
            if (doWhile2 == null)
            {
                doWhile1.Invariants = this.VisitExpressionList(doWhile1.Invariants, null);
                doWhile1.Body = this.VisitBlock(doWhile1.Body, null);
                doWhile1.Condition = this.VisitExpression(doWhile1.Condition, null);
            }
            else
            {
                doWhile1.Invariants = this.VisitExpressionList(doWhile1.Invariants, doWhile2.Invariants);
                doWhile1.Body = this.VisitBlock(doWhile1.Body, doWhile2.Body);
                doWhile1.Condition = this.VisitExpression(doWhile1.Condition, doWhile2.Condition);
            }
            return doWhile1;
        }
        public virtual Statement VisitEndFilter(EndFilter endFilter1, EndFilter endFilter2)
        {
            if (endFilter1 == null) return null;
            if (endFilter2 == null)
                endFilter1.Value = this.VisitExpression(endFilter1.Value, null);
            else
                endFilter1.Value = this.VisitExpression(endFilter1.Value, endFilter2.Value);
            return endFilter1;
        }
        public virtual Statement VisitEndFinally(EndFinally endFinally1, EndFinally endFinally2)
        {
            return endFinally1;
        }
#if ExtendedRuntime
    public virtual EnsuresList VisitEnsuresList(EnsuresList ensures1, EnsuresList ensures2) {
      if (ensures1 == null) return null;
      for (int i = 0, n = ensures1.Count, m = ensures2 == null ? 0 : ensures2.Count; i < n; i++) {
        //^ assert ensures2 != null;
        if (i >= m)
          ensures1[i] = (Ensures)this.Visit(ensures1[i], null);
        else
          ensures1[i] = (Ensures)this.Visit(ensures1[i], ensures2[i]);
      }
      return ensures1;
    }
#endif
        public virtual EnumNode VisitEnumNode(EnumNode enumNode1, EnumNode enumNode2)
        {
            return (EnumNode)this.VisitTypeNode(enumNode1, enumNode2);
        }
        public virtual Event VisitEvent(Event evnt1, Event evnt2)
        {
            if (evnt1 == null) return null;
            if (evnt2 == null)
            {
                evnt1.Attributes = this.VisitAttributeList(evnt1.Attributes, null);
                evnt1.HandlerType = this.VisitTypeReference(evnt1.HandlerType, null);
            }
            else
            {
                evnt1.Attributes = this.VisitAttributeList(evnt1.Attributes, evnt2.Attributes);
                evnt1.HandlerType = this.VisitTypeReference(evnt1.HandlerType, evnt2.HandlerType);
            }
            return evnt1;
        }
#if ExtendedRuntime
    public virtual EnsuresExceptional VisitEnsuresExceptional(EnsuresExceptional exceptional1, EnsuresExceptional exceptional2) {
      if (exceptional1 == null) return null;
      if (exceptional2 == null) {
        exceptional1.PostCondition = this.VisitExpression(exceptional1.PostCondition, null);
        exceptional1.Type = this.VisitTypeReference(exceptional1.Type, null);
        exceptional1.Variable = this.VisitExpression(exceptional1.Variable, null);
      }else{
        exceptional1.PostCondition = this.VisitExpression(exceptional1.PostCondition, exceptional2.PostCondition);
        exceptional1.Type = this.VisitTypeReference(exceptional1.Type, exceptional2.Type);
        exceptional1.Variable = this.VisitExpression(exceptional1.Variable, exceptional2.Variable);
      }
      return exceptional1;
    }
#endif
        public virtual Statement VisitExit(Exit exit1, Exit exit2)
        {
            return exit1;
        }

        public virtual Statement VisitExpose(Expose expose1, Expose expose2)
        {
            if (expose1 == null) return null;
            if (expose2 == null)
            {
                expose1.Instance = this.VisitExpression(expose1.Instance, null);
                expose1.Body = this.VisitBlock(expose1.Body, null);
            }
            else
            {
                expose1.Instance = this.VisitExpression(expose1.Instance, expose1.Instance);
                expose1.Body = this.VisitBlock(expose1.Body, expose2.Body);
            }
            return expose1;
        }

        public virtual Expression VisitExpression(Expression expression1, Expression expression2)
        {
            if (expression1 == null) return null;
            switch (expression1.NodeType)
            {
                case NodeType.Dup:
                case NodeType.Arglist:
                    return expression1;
                case NodeType.Pop:
                    UnaryExpression uex1 = expression1 as UnaryExpression;
                    UnaryExpression uex2 = expression2 as UnaryExpression;
                    if (uex1 != null)
                    {
                        uex1.Operand = this.VisitExpression(uex1.Operand, uex2 == null ? null : uex2.Operand);
                        return uex1;
                    }
                    return expression1;
                default:
                    return (Expression)this.Visit(expression1, expression2);
            }
        }
        public virtual ExpressionList VisitExpressionList(ExpressionList list1, ExpressionList list2)
        {
            if (list1 == null) return null;
            for (int i = 0, n = list1.Count, m = list2 == null ? 0 : list2.Count; i < n; i++)
            {
                //^ assert list2 != null;
                if (i >= m)
                    list1[i] = (Expression)this.Visit(list1[i], null);
                else
                    list1[i] = (Expression)this.Visit(list1[i], list2[i]);
            }
            return list1;
        }
        public virtual Expression VisitExpressionSnippet(ExpressionSnippet snippet1, ExpressionSnippet snippet2)
        {
            return snippet1;
        }
        public virtual Statement VisitExpressionStatement(ExpressionStatement statement1, ExpressionStatement statement2)
        {
            if (statement1 == null) return null;
            if (statement2 == null)
                statement1.Expression = this.VisitExpression(statement1.Expression, null);
            else
                statement1.Expression = this.VisitExpression(statement1.Expression, statement2.Expression);
            return statement1;
        }
        public virtual Statement VisitFaultHandler(FaultHandler faultHandler1, FaultHandler faultHandler2)
        {
            if (faultHandler1 == null) return null;
            if (faultHandler2 == null)
                faultHandler1.Block = this.VisitBlock(faultHandler1.Block, null);
            else
                faultHandler1.Block = this.VisitBlock(faultHandler1.Block, faultHandler2.Block);
            return faultHandler1;
        }
        public virtual FaultHandlerList VisitFaultHandlerList(FaultHandlerList faultHandlers1, FaultHandlerList faultHandlers2)
        {
            if (faultHandlers1 == null) return null;
            for (int i = 0, n = faultHandlers1.Count, m = faultHandlers2 == null ? 0 : faultHandlers2.Count; i < n; i++)
            {
                //^ assert faultHandlers2 != null;
                if (i >= m)
                    faultHandlers1[i] = (FaultHandler)this.VisitFaultHandler(faultHandlers1[i], null);
                else
                    faultHandlers1[i] = (FaultHandler)this.VisitFaultHandler(faultHandlers1[i], faultHandlers2[i]);
            }
            return faultHandlers1;
        }
        public virtual Field VisitField(Field field1, Field field2)
        {
            if (field1 == null) return null;
            if (field2 == null)
            {
                field1.Attributes = this.VisitAttributeList(field1.Attributes, null);
                field1.Type = this.VisitTypeReference(field1.Type, null);
                field1.Initializer = this.VisitExpression(field1.Initializer, null);
                field1.ImplementedInterfaces = this.VisitInterfaceReferenceList(field1.ImplementedInterfaces, null);
            }
            else
            {
                field1.Attributes = this.VisitAttributeList(field1.Attributes, field2.Attributes);
                field1.Type = this.VisitTypeReference(field1.Type, field2.Type);
                field1.Initializer = this.VisitExpression(field1.Initializer, field2.Initializer);
                field1.ImplementedInterfaces = this.VisitInterfaceReferenceList(field1.ImplementedInterfaces, field2.ImplementedInterfaces);
            }
            return field1;
        }
        public virtual Block VisitFieldInitializerBlock(FieldInitializerBlock block1, FieldInitializerBlock block2)
        {
            if (block1 == null) return null;
            if (block2 == null)
                block1.Type = this.VisitTypeReference(block1.Type, null);
            else
                block1.Type = this.VisitTypeReference(block1.Type, block2.Type);
            return this.VisitBlock(block1, block2);
        }
        public virtual FieldList VisitFieldList(FieldList fields1, FieldList fields2)
        {
            if (fields1 == null) return null;
            for (int i = 0, n = fields1.Count, m = fields2 == null ? 0 : fields2.Count; i < n; i++)
            {
                //^ assert fields2 != null;
                if (i >= m)
                    fields1[i] = this.VisitField(fields1[i], null);
                else
                    fields1[i] = this.VisitField(fields1[i], fields2[i]);
            }
            return fields1;
        }
        public virtual Statement VisitFilter(Filter filter1, Filter filter2)
        {
            if (filter1 == null) return null;
            if (filter2 == null)
            {
                filter1.Expression = this.VisitExpression(filter1.Expression, null);
                filter1.Block = this.VisitBlock(filter1.Block, null);
            }
            else
            {
                filter1.Expression = this.VisitExpression(filter1.Expression, filter2.Expression);
                filter1.Block = this.VisitBlock(filter1.Block, filter2.Block);
            }
            return filter1;
        }
        public virtual FilterList VisitFilterList(FilterList filters1, FilterList filters2)
        {
            if (filters1 == null) return null;
            for (int i = 0, n = filters1.Count, m = filters2 == null ? 0 : filters2.Count; i < n; i++)
            {
                //^ assert filters2 != null;
                if (i >= m)
                    filters1[i] = (Filter)this.VisitFilter(filters1[i], null);
                else
                    filters1[i] = (Filter)this.VisitFilter(filters1[i], filters2[i]);
            }
            return filters1;
        }
        public virtual Statement VisitFinally(Finally Finally1, Finally Finally2)
        {
            if (Finally1 == null) return null;
            if (Finally2 == null)
                Finally1.Block = this.VisitBlock(Finally1.Block, null);
            else
                Finally1.Block = this.VisitBlock(Finally1.Block, Finally2.Block);
            return Finally1;
        }
        public virtual Statement VisitFixed(Fixed fixed1, Fixed fixed2)
        {
            if (fixed1 == null) return null;
            if (fixed2 == null)
            {
                fixed1.Declarators = (Statement)this.Visit(fixed1.Declarators, null);
                fixed1.Body = this.VisitBlock(fixed1.Body, null);
            }
            else
            {
                fixed1.Declarators = (Statement)this.Visit(fixed1.Declarators, fixed2.Declarators);
                fixed1.Body = this.VisitBlock(fixed1.Body, fixed2.Body);
            }
            return fixed1;
        }
        public virtual Statement VisitFor(For For1, For For2)
        {
            if (For1 == null) return null;
            if (For2 == null)
            {
                For1.Initializer = this.VisitStatementList(For1.Initializer, null);
                For1.Invariants = this.VisitExpressionList(For1.Invariants, null);
                For1.Condition = this.VisitExpression(For1.Condition, null);
                For1.Incrementer = this.VisitStatementList(For1.Incrementer, null);
                For1.Body = this.VisitBlock(For1.Body, null);
            }
            else
            {
                For1.Initializer = this.VisitStatementList(For1.Initializer, For2.Initializer);
                For1.Invariants = this.VisitExpressionList(For1.Invariants, For2.Invariants);
                For1.Condition = this.VisitExpression(For1.Condition, For2.Condition);
                For1.Incrementer = this.VisitStatementList(For1.Incrementer, For2.Incrementer);
                For1.Body = this.VisitBlock(For1.Body, For2.Body);
            }
            return For1;
        }
        public virtual Statement VisitForEach(ForEach forEach1, ForEach forEach2)
        {
            if (forEach1 == null) return null;
            if (forEach2 == null)
            {
                forEach1.TargetVariableType = this.VisitTypeReference(forEach1.TargetVariableType, null);
                forEach1.TargetVariable = this.VisitTargetExpression(forEach1.TargetVariable, null);
                forEach1.SourceEnumerable = this.VisitExpression(forEach1.SourceEnumerable, null);
                forEach1.InductionVariable = this.VisitTargetExpression(forEach1.InductionVariable, null);
                forEach1.Invariants = this.VisitExpressionList(forEach1.Invariants, null);
                forEach1.Body = this.VisitBlock(forEach1.Body, null);
            }
            else
            {
                forEach1.TargetVariableType = this.VisitTypeReference(forEach1.TargetVariableType, forEach2.TargetVariableType);
                forEach1.TargetVariable = this.VisitTargetExpression(forEach1.TargetVariable, forEach2.TargetVariable);
                forEach1.SourceEnumerable = this.VisitExpression(forEach1.SourceEnumerable, forEach2.SourceEnumerable);
                forEach1.InductionVariable = this.VisitTargetExpression(forEach1.InductionVariable, forEach2.InductionVariable);
                forEach1.Invariants = this.VisitExpressionList(forEach1.Invariants, forEach2.Invariants);
                forEach1.Body = this.VisitBlock(forEach1.Body, forEach2.Body);
            }
            return forEach1;
        }
        public virtual Statement VisitFunctionDeclaration(FunctionDeclaration functionDeclaration1, FunctionDeclaration functionDeclaration2)
        {
            if (functionDeclaration1 == null) return null;
            if (functionDeclaration2 == null)
            {
                functionDeclaration1.Parameters = this.VisitParameterList(functionDeclaration1.Parameters, null);
                functionDeclaration1.ReturnType = this.VisitTypeReference(functionDeclaration1.ReturnType, null);
                functionDeclaration1.Body = this.VisitBlock(functionDeclaration1.Body, null);
            }
            else
            {
                functionDeclaration1.Parameters = this.VisitParameterList(functionDeclaration1.Parameters, functionDeclaration2.Parameters);
                functionDeclaration1.ReturnType = this.VisitTypeReference(functionDeclaration1.ReturnType, functionDeclaration2.ReturnType);
                functionDeclaration1.Body = this.VisitBlock(functionDeclaration1.Body, functionDeclaration2.Body);
            }
            return functionDeclaration1;
        }
        public virtual Expression VisitTemplateInstance(TemplateInstance genericInstance1, TemplateInstance genericInstance2)
        {
            if (genericInstance1 == null) return null;
            if (genericInstance2 == null)
            {
                genericInstance1.Expression = this.VisitExpression(genericInstance1.Expression, null);
                genericInstance1.TypeArguments = this.VisitTypeReferenceList(genericInstance1.TypeArguments, null);
            }
            else
            {
                genericInstance1.Expression = this.VisitExpression(genericInstance1.Expression, genericInstance2.Expression);
                genericInstance1.TypeArguments = this.VisitTypeReferenceList(genericInstance1.TypeArguments, genericInstance2.TypeArguments);
            }
            return genericInstance1;
        }
        public virtual Expression VisitStackAlloc(StackAlloc alloc1, StackAlloc alloc2)
        {
            if (alloc1 == null) return null;
            if (alloc2 == null)
            {
                alloc1.ElementType = this.VisitTypeReference(alloc1.ElementType, null);
                alloc1.NumberOfElements = this.VisitExpression(alloc1.NumberOfElements, null);
            }
            else
            {
                alloc1.ElementType = this.VisitTypeReference(alloc1.ElementType, alloc2.ElementType);
                alloc1.NumberOfElements = this.VisitExpression(alloc1.NumberOfElements, alloc2.NumberOfElements);
            }
            return alloc1;
        }
        public virtual Statement VisitGoto(Goto Goto1, Goto Goto2)
        {
            return Goto1;
        }
        public virtual Statement VisitGotoCase(GotoCase gotoCase1, GotoCase gotoCase2)
        {
            if (gotoCase1 == null) return null;
            if (gotoCase2 == null)
            {
                gotoCase1.CaseLabel = this.VisitExpression(gotoCase1.CaseLabel, null);
            }
            else
            {
                gotoCase1.CaseLabel = this.VisitExpression(gotoCase1.CaseLabel, gotoCase2.CaseLabel);
            }
            return gotoCase1;
        }
        public virtual Expression VisitIdentifier(Identifier identifier1, Identifier identifier2)
        {
            return identifier1;
        }
        public virtual Statement VisitIf(If If1, If If2)
        {
            if (If1 == null) return null;
            if (If2 == null)
            {
                If1.Condition = this.VisitExpression(If1.Condition, null);
                If1.TrueBlock = this.VisitBlock(If1.TrueBlock, null);
                If1.FalseBlock = this.VisitBlock(If1.FalseBlock, null);
            }
            else
            {
                If1.Condition = this.VisitExpression(If1.Condition, If2.Condition);
                If1.TrueBlock = this.VisitBlock(If1.TrueBlock, If2.TrueBlock);
                If1.FalseBlock = this.VisitBlock(If1.FalseBlock, If2.FalseBlock);
            }
            return If1;
        }
        public virtual Expression VisitImplicitThis(ImplicitThis implicitThis1, ImplicitThis implicitThis2)
        {
            return implicitThis1;
        }
        public virtual Expression VisitIndexer(Indexer indexer1, Indexer indexer2)
        {
            if (indexer1 == null) return null;
            if (indexer2 == null)
            {
                indexer1.Object = this.VisitExpression(indexer1.Object, null);
                indexer1.Operands = this.VisitExpressionList(indexer1.Operands, null);
            }
            else
            {
                indexer1.Object = this.VisitExpression(indexer1.Object, indexer2.Object);
                indexer1.Operands = this.VisitExpressionList(indexer1.Operands, indexer2.Operands);
            }
            return indexer1;
        }
        public virtual Interface VisitInterface(Interface Interface1, Interface Interface2)
        {
            return (Interface)this.VisitTypeNode(Interface1, Interface2);
        }
        public virtual Interface VisitInterfaceReference(Interface Interface1, Interface Interface2)
        {
            return (Interface)this.VisitTypeReference(Interface1, Interface2);
        }
        public virtual InterfaceList VisitInterfaceReferenceList(InterfaceList interfaceReferences1, InterfaceList interfaceReferences2)
        {
            if (interfaceReferences1 == null) return null;
            for (int i = 0, n = interfaceReferences1.Count, m = interfaceReferences2 == null ? 0 : interfaceReferences2.Count; i < n; i++)
            {
                //^ assert interfaceReferences2 != null;
                if (i >= m)
                    interfaceReferences1[i] = this.VisitInterfaceReference(interfaceReferences1[i], null);
                else
                    interfaceReferences1[i] = this.VisitInterfaceReference(interfaceReferences1[i], interfaceReferences2[i]);
            }
            return interfaceReferences1;
        }
#if ExtendedRuntime
    public virtual Invariant VisitInvariant(Invariant invariant1, Invariant invariant2){
      if (invariant1 == null) return null;
      if (invariant2 == null){
        invariant1.Condition = this.VisitExpression(invariant1.Condition, null);
      }else{
        invariant1.Condition = this.VisitExpression(invariant1.Condition, invariant2.Condition);
      }
      return invariant1;
    }
    public virtual InvariantList VisitInvariantList(InvariantList Invariants1, InvariantList Invariants2){
      if (Invariants1 == null) return null;
      for (int i = 0, n = Invariants1.Count, m = Invariants2 == null ? 0 : Invariants2.Count; i < n; i++){
        //^ assert Invariants2 != null;
        if (i >= m)
          Invariants1[i] = this.VisitInvariant(Invariants1[i], null);
        else
          Invariants1[i] = this.VisitInvariant(Invariants1[i], Invariants2[i]);
      }
      return Invariants1;
    }
#endif
        public virtual InstanceInitializer VisitInstanceInitializer(InstanceInitializer cons1, InstanceInitializer cons2)
        {
            return (InstanceInitializer)this.VisitMethod(cons1, cons2);
        }
        public virtual Statement VisitLabeledStatement(LabeledStatement lStatement1, LabeledStatement lStatement2)
        {
            if (lStatement1 == null) return null;
            if (lStatement2 == null)
                lStatement1.Statement = (Statement)this.Visit(lStatement1.Statement, null);
            else
                lStatement1.Statement = (Statement)this.Visit(lStatement1.Statement, lStatement2.Statement);
            return lStatement1;
        }
        public virtual Expression VisitLiteral(Literal literal1, Literal literal2)
        {
            return literal1;
        }
        public virtual Expression VisitLocal(Local local1, Local local2)
        {
            if (local1 == null) return null;
            if (local2 == null)
                local1.Type = this.VisitTypeReference(local1.Type, null);
            else
                local1.Type = this.VisitTypeReference(local1.Type, local2.Type);
            return local1;
        }
        public virtual LocalDeclaration VisitLocalDeclaration(LocalDeclaration localDeclaration1, LocalDeclaration localDeclaration2)
        {
            if (localDeclaration1 == null) return null;
            if (localDeclaration2 == null)
                localDeclaration1.InitialValue = this.VisitExpression(localDeclaration1.InitialValue, null);
            else
                localDeclaration1.InitialValue = this.VisitExpression(localDeclaration1.InitialValue, localDeclaration2.InitialValue);
            return localDeclaration1;
        }
        public virtual LocalDeclarationList VisitLocalDeclarationList(LocalDeclarationList localDeclarations1, LocalDeclarationList localDeclarations2)
        {
            if (localDeclarations1 == null) return null;
            for (int i = 0, n = localDeclarations1.Count, m = localDeclarations2 == null ? 0 : localDeclarations2.Count; i < n; i++)
            {
                //^ assert localDeclarations2 != null;
                if (i >= m)
                    localDeclarations1[i] = this.VisitLocalDeclaration(localDeclarations1[i], null);
                else
                    localDeclarations1[i] = this.VisitLocalDeclaration(localDeclarations1[i], localDeclarations2[i]);
            }
            return localDeclarations1;
        }
        public virtual Statement VisitLocalDeclarationsStatement(LocalDeclarationsStatement localDeclarations1, LocalDeclarationsStatement localDeclarations2)
        {
            if (localDeclarations1 == null) return null;
            if (localDeclarations2 == null)
            {
                localDeclarations1.Type = this.VisitTypeReference(localDeclarations1.Type, null);
                localDeclarations1.Declarations = this.VisitLocalDeclarationList(localDeclarations1.Declarations, null);
            }
            else
            {
                localDeclarations1.Type = this.VisitTypeReference(localDeclarations1.Type, localDeclarations2.Type);
                localDeclarations1.Declarations = this.VisitLocalDeclarationList(localDeclarations1.Declarations, localDeclarations2.Declarations);
            }
            return localDeclarations1;
        }
        public virtual Statement VisitLock(Lock lock1, Lock lock2)
        {
            if (lock1 == null) return null;
            if (lock2 == null)
            {
                lock1.Guard = this.VisitExpression(lock1.Guard, null);
                lock1.Body = this.VisitBlock(lock1.Body, null);
            }
            else
            {
                lock1.Guard = this.VisitExpression(lock1.Guard, lock2.Guard);
                lock1.Body = this.VisitBlock(lock1.Body, lock2.Body);
            }
            return lock1;
        }
        public virtual Expression VisitLRExpression(LRExpression expr1, LRExpression expr2)
        {
            if (expr1 == null) return null;
            if (expr2 == null)
                expr1.Expression = this.VisitExpression(expr1.Expression, null);
            else
                expr1.Expression = this.VisitExpression(expr1.Expression, expr2.Expression);
            return expr1;
        }
        public virtual Expression VisitMemberBinding(MemberBinding memberBinding1, MemberBinding memberBinding2)
        {
            if (memberBinding1 == null) return null;
            if (memberBinding2 == null)
                memberBinding1.TargetObject = this.VisitExpression(memberBinding1.TargetObject, null);
            else
                memberBinding1.TargetObject = this.VisitExpression(memberBinding1.TargetObject, memberBinding2.TargetObject);
            return memberBinding1;
        }
        public virtual MemberList VisitMemberList(MemberList members1, MemberList members2)
        {
            if (members1 == null) return null;
            for (int i = 0, n = members1.Count, m = members2 == null ? 0 : members2.Count; i < n; i++)
            {
                //^ assert members2 != null;
                if (i >= m)
                    members1[i] = (Member)this.Visit(members1[i], null);
                else
                    members1[i] = (Member)this.Visit(members1[i], members2[i]);
            }
            return members1;
        }
        public virtual Method VisitMethod(Method method1, Method method2)
        {
            if (method1 == null) return null;
            if (method2 == null)
            {
                method1.Attributes = this.VisitAttributeList(method1.Attributes, null);
                method1.ReturnAttributes = this.VisitAttributeList(method1.ReturnAttributes, null);
                method1.SecurityAttributes = this.VisitSecurityAttributeList(method1.SecurityAttributes, null);
                method1.ReturnType = this.VisitTypeReference(method1.ReturnType, null);
                method1.ImplementedTypes = this.VisitTypeReferenceList(method1.ImplementedTypes, null);
                method1.Parameters = this.VisitParameterList(method1.Parameters, null);
                method1.Body = this.VisitBlock(method1.Body, null);
            }
            else
            {
                method1.Attributes = this.VisitAttributeList(method1.Attributes, method2.Attributes);
                method1.ReturnAttributes = this.VisitAttributeList(method1.ReturnAttributes, method2.ReturnAttributes);
                method1.SecurityAttributes = this.VisitSecurityAttributeList(method1.SecurityAttributes, method2.SecurityAttributes);
                method1.ReturnType = this.VisitTypeReference(method1.ReturnType, method2.ReturnType);
                method1.ImplementedTypes = this.VisitTypeReferenceList(method1.ImplementedTypes, method2.ImplementedTypes);
                method1.Parameters = this.VisitParameterList(method1.Parameters, method2.Parameters);
#if ExtendedRuntime
        method1.Contract = this.VisitMethodContract(method1.Contract, method2.Contract);
#endif
                method1.Body = this.VisitBlock(method1.Body, method2.Body);
            }
            return method1;
        }
        public virtual Expression VisitMethodCall(MethodCall call1, MethodCall call2)
        {
            if (call1 == null) return null;
            if (call2 == null)
            {
                call1.Callee = this.VisitExpression(call1.Callee, null);
                call1.Operands = this.VisitExpressionList(call1.Operands, null);
            }
            else
            {
                call1.Callee = this.VisitExpression(call1.Callee, call2.Callee);
                call1.Operands = this.VisitExpressionList(call1.Operands, call2.Operands);
            }
            return call1;
        }
#if ExtendedRuntime
    public virtual MethodContract VisitMethodContract(MethodContract contract1, MethodContract contract2) {
      if (contract1 == null) return null;
      if (contract2 == null) {
        // don't visit contract.DeclaringMethod
        // don't visit contract.OverriddenMethods
        contract1.Requires = this.VisitRequiresList(contract1.Requires, null);
        contract1.Ensures = this.VisitEnsuresList(contract1.Ensures, null);
        contract1.Modifies = this.VisitExpressionList(contract1.Modifies, null);
      }else{
        // don't visit contract.DeclaringMethod
        // don't visit contract.OverriddenMethods
        contract1.Requires = this.VisitRequiresList(contract1.Requires,contract2.Requires);
        contract1.Ensures = this.VisitEnsuresList(contract1.Ensures,contract2.Ensures);
        contract1.Modifies = this.VisitExpressionList(contract1.Modifies,contract2.Modifies);
      }
      return contract1;
    }
#endif
        public virtual Module VisitModule(Module module1, Module module2)
        {
            if (module1 == null) return null;
            if (module2 == null)
            {
                module1.Attributes = this.VisitAttributeList(module1.Attributes, null);
                module1.Types = this.VisitTypeNodeList(module1.Types, null);
            }
            else
            {
                module1.Attributes = this.VisitAttributeList(module1.Attributes, module2.Attributes);
                module1.Types = this.VisitTypeNodeList(module1.Types, module2.Types);
            }
            return module1;
        }
        public virtual ModuleReference VisitModuleReference(ModuleReference moduleReference1, ModuleReference moduleReference2)
        {
            return moduleReference1;
        }
        public virtual Expression VisitNameBinding(NameBinding nameBinding1, NameBinding nameBinding2)
        {
            return nameBinding1;
        }
        public virtual Expression VisitNamedArgument(NamedArgument namedArgument1, NamedArgument namedArgument2)
        {
            if (namedArgument1 == null) return null;
            if (namedArgument2 == null)
                namedArgument1.Value = this.VisitExpression(namedArgument1.Value, null);
            else
                namedArgument1.Value = this.VisitExpression(namedArgument1.Value, namedArgument2.Value);
            return namedArgument1;
        }
        public virtual Namespace VisitNamespace(Namespace nspace1, Namespace nspace2)
        {
            if (nspace1 == null) return null;
            if (nspace2 == null)
            {
                nspace1.AliasDefinitions = this.VisitAliasDefinitionList(nspace1.AliasDefinitions, null);
                nspace1.UsedNamespaces = this.VisitUsedNamespaceList(nspace1.UsedNamespaces, null);
                nspace1.Attributes = this.VisitAttributeList(nspace1.Attributes, null);
                nspace1.Types = this.VisitTypeNodeList(nspace1.Types, null);
                nspace1.NestedNamespaces = this.VisitNamespaceList(nspace1.NestedNamespaces, null);
            }
            else
            {
                nspace1.AliasDefinitions = this.VisitAliasDefinitionList(nspace1.AliasDefinitions, nspace2.AliasDefinitions);
                nspace1.UsedNamespaces = this.VisitUsedNamespaceList(nspace1.UsedNamespaces, nspace2.UsedNamespaces);
                nspace1.Attributes = this.VisitAttributeList(nspace1.Attributes, nspace2.Attributes);
                nspace1.Types = this.VisitTypeNodeList(nspace1.Types, nspace2.Types);
                nspace1.NestedNamespaces = this.VisitNamespaceList(nspace1.NestedNamespaces, nspace2.NestedNamespaces);
            }
            return nspace1;
        }
        public virtual NamespaceList VisitNamespaceList(NamespaceList namespaces1, NamespaceList namespaces2)
        {
            if (namespaces1 == null) return null;
            for (int i = 0, n = namespaces1.Count, m = namespaces2 == null ? 0 : namespaces2.Count; i < n; i++)
            {
                //^ assert namespaces2 != null;
                if (i >= m)
                    namespaces1[i] = this.VisitNamespace(namespaces1[i], null);
                else
                    namespaces1[i] = this.VisitNamespace(namespaces1[i], namespaces2[i]);
            }
            return namespaces1;
        }
        public virtual NodeList VisitNodeList(NodeList nodes1, NodeList nodes2)
        {
            if (nodes1 == null) return null;
            for (int i = 0, n = nodes1.Count, m = nodes2 == null ? 0 : nodes2.Count; i < n; i++)
            {
                //^ assert nodes2 != null;
                if (i >= m)
                    nodes1[i] = (CompilationUnit)this.Visit(nodes1[i], null);
                else
                    nodes1[i] = (CompilationUnit)this.Visit(nodes1[i], nodes2[i]);
            }
            return nodes1;
        }
#if ExtendedRuntime
    public virtual EnsuresNormal VisitEnsuresNormal(EnsuresNormal normal1, EnsuresNormal normal2) {
      if (normal1 == null) return null;
      if (normal2 == null)
        normal1.PostCondition = this.VisitExpression(normal1.PostCondition, null);
      else
        normal1.PostCondition = this.VisitExpression(normal1.PostCondition, normal2.PostCondition);
      return normal1;
    }
    public virtual Expression VisitOldExpression(OldExpression oldExpression1, OldExpression oldExpression2) {
      if (oldExpression1 == null) return null;
      if (oldExpression2 == null)
        oldExpression1.expression = this.VisitExpression(oldExpression1.expression, null);
      else
        oldExpression1.expression = this.VisitExpression(oldExpression1.expression, oldExpression2.expression);
      return oldExpression1;
    }
    public virtual RequiresOtherwise VisitRequiresOtherwise(RequiresOtherwise otherwise1, RequiresOtherwise otherwise2) {
      if (otherwise1 == null) return null;
      if (otherwise2 == null) {
        otherwise1.Condition = this.VisitExpression(otherwise1.Condition, null);
        otherwise1.ThrowException = this.VisitExpression(otherwise1.ThrowException, null);
      }else{
        otherwise1.Condition = this.VisitExpression(otherwise1.Condition, otherwise2.Condition);
        otherwise1.ThrowException = this.VisitExpression(otherwise1.ThrowException, otherwise2.ThrowException);
      }
      return otherwise1;
    }
#endif
        public virtual Expression VisitParameter(Parameter parameter1, Parameter parameter2)
        {
            if (parameter1 == null) return null;
            if (parameter2 == null)
            {
                parameter1.Attributes = this.VisitAttributeList(parameter1.Attributes, null);
                parameter1.Type = this.VisitTypeReference(parameter1.Type, null);
                parameter1.DefaultValue = this.VisitExpression(parameter1.DefaultValue, null);
            }
            else
            {
                parameter1.Attributes = this.VisitAttributeList(parameter1.Attributes, parameter2.Attributes);
                parameter1.Type = this.VisitTypeReference(parameter1.Type, parameter2.Type);
                parameter1.DefaultValue = this.VisitExpression(parameter1.DefaultValue, parameter2.DefaultValue);
            }
            return parameter1;
        }
        public virtual ParameterList VisitParameterList(ParameterList parameterList1, ParameterList parameterList2)
        {
            if (parameterList1 == null) return null;
            for (int i = 0, n = parameterList1.Count, m = parameterList2 == null ? 0 : parameterList2.Count; i < n; i++)
            {
                //^ assert parameterList2 != null;
                if (i >= m)
                    parameterList1[i] = (Parameter)this.VisitParameter(parameterList1[i], null);
                else
                    parameterList1[i] = (Parameter)this.VisitParameter(parameterList1[i], parameterList2[i]);
            }
            return parameterList1;
        }
#if ExtendedRuntime
    public virtual RequiresPlain VisitRequiresPlain(RequiresPlain plain1, RequiresPlain plain2) {
      if (plain1 == null) return null;
      if (plain2 == null)
        plain1.Condition = this.VisitExpression(plain1.Condition, null);
      else
        plain1.Condition = this.VisitExpression(plain1.Condition, plain2.Condition);
      return plain1;
    }
#endif
        public virtual Expression VisitPrefixExpression(PrefixExpression pExpr1, PrefixExpression pExpr2)
        {
            if (pExpr1 == null) return null;
            if (pExpr2 == null)
                pExpr1.Expression = this.VisitExpression(pExpr1.Expression, null);
            else
                pExpr1.Expression = this.VisitExpression(pExpr1.Expression, pExpr2.Expression);
            return pExpr1;
        }
        public virtual Expression VisitPostfixExpression(PostfixExpression pExpr1, PostfixExpression pExpr2)
        {
            if (pExpr1 == null) return null;
            if (pExpr2 == null)
                pExpr1.Expression = this.VisitExpression(pExpr1.Expression, null);
            else
                pExpr1.Expression = this.VisitExpression(pExpr1.Expression, pExpr2.Expression);
            return pExpr1;
        }
        public virtual Property VisitProperty(Property property1, Property property2)
        {
            if (property1 == null) return null;
            if (property2 == null)
            {
                property1.Attributes = this.VisitAttributeList(property1.Attributes, null);
                property1.Parameters = this.VisitParameterList(property1.Parameters, null);
                property1.Type = this.VisitTypeReference(property1.Type, null);
            }
            else
            {
                property1.Attributes = this.VisitAttributeList(property1.Attributes, property2.Attributes);
                property1.Parameters = this.VisitParameterList(property1.Parameters, property2.Parameters);
                property1.Type = this.VisitTypeReference(property1.Type, property2.Type);
            }
            return property1;
        }
        public virtual Expression VisitQuantifier(Quantifier quantifier1, Quantifier quantifier2)
        {
            if (quantifier1 == null) return null;
            if (quantifier2 == null)
            {
                quantifier1.Comprehension = (Comprehension)this.VisitComprehension(quantifier1.Comprehension, null);
            }
            else
            {
                quantifier1.Comprehension = (Comprehension)this.VisitComprehension(quantifier1.Comprehension, quantifier2.Comprehension);
            }
            return quantifier1;
        }
        public virtual Expression VisitComprehension(Comprehension comprehension1, Comprehension comprehension2)
        {
            if (comprehension1 == null) return null;
            if (comprehension2 == null)
            {
                comprehension1.BindingsAndFilters = this.VisitExpressionList(comprehension1.BindingsAndFilters, null);
                comprehension1.Elements = this.VisitExpressionList(comprehension1.Elements, null);
            }
            else
            {
                comprehension1.BindingsAndFilters = this.VisitExpressionList(comprehension1.BindingsAndFilters, comprehension2.BindingsAndFilters);
                comprehension1.Elements = this.VisitExpressionList(comprehension1.Elements, comprehension2.Elements);
            }
            return comprehension1;
        }
        public virtual ComprehensionBinding VisitComprehensionBinding(ComprehensionBinding comprehensionBinding1, ComprehensionBinding comprehensionBinding2)
        {
            if (comprehensionBinding1 == null) return null;
            if (comprehensionBinding2 == null)
            {
                comprehensionBinding1.TargetVariableType = this.VisitTypeReference(comprehensionBinding1.TargetVariableType, null);
                comprehensionBinding1.TargetVariable = this.VisitTargetExpression(comprehensionBinding1.TargetVariable, null);
                comprehensionBinding1.SourceEnumerable = this.VisitExpression(comprehensionBinding1.SourceEnumerable, null);
            }
            else
            {
                comprehensionBinding1.TargetVariableType = this.VisitTypeReference(comprehensionBinding1.TargetVariableType, comprehensionBinding2.TargetVariableType);
                comprehensionBinding1.TargetVariable = this.VisitTargetExpression(comprehensionBinding1.TargetVariable, comprehensionBinding2.TargetVariable);
                comprehensionBinding1.SourceEnumerable = this.VisitExpression(comprehensionBinding1.SourceEnumerable, comprehensionBinding2.SourceEnumerable);
            }
            return comprehensionBinding1;
        }
        public virtual Expression VisitQualifiedIdentifier(QualifiedIdentifier qualifiedIdentifier1, QualifiedIdentifier qualifiedIdentifier2)
        {
            if (qualifiedIdentifier1 == null) return null;
            if (qualifiedIdentifier2 == null)
                qualifiedIdentifier1.Qualifier = this.VisitExpression(qualifiedIdentifier1.Qualifier, null);
            else
                qualifiedIdentifier1.Qualifier = this.VisitExpression(qualifiedIdentifier1.Qualifier, qualifiedIdentifier2.Qualifier);
            return qualifiedIdentifier1;
        }
        public virtual Statement VisitRepeat(Repeat repeat1, Repeat repeat2)
        {
            if (repeat1 == null) return null;
            if (repeat2 == null)
            {
                repeat1.Body = this.VisitBlock(repeat1.Body, null);
                repeat1.Condition = this.VisitExpression(repeat1.Condition, null);
            }
            else
            {
                repeat1.Body = this.VisitBlock(repeat1.Body, repeat2.Body);
                repeat1.Condition = this.VisitExpression(repeat1.Condition, repeat2.Condition);
            }
            return repeat1;
        }
#if ExtendedRuntime
    public virtual RequiresList VisitRequiresList(RequiresList requires1, RequiresList requires2) {
      if (requires1 == null) return null;
      for (int i = 0, n = requires1.Count, m = requires2 == null ? 0 : requires2.Count; i < n; i++) {
        //^ assert requires2 != null;
        if (i >= m)
          requires1[i] = (Requires)this.Visit(requires1[i], null);
        else
          requires1[i] = (Requires)this.Visit(requires1[i], requires2[i]);
      }
      return requires1;
    }
#endif
        public virtual Statement VisitResourceUse(ResourceUse resourceUse1, ResourceUse resourceUse2)
        {
            if (resourceUse1 == null) return null;
            if (resourceUse2 == null)
            {
                resourceUse1.ResourceAcquisition = (Statement)this.Visit(resourceUse1.ResourceAcquisition, null);
                resourceUse1.Body = this.VisitBlock(resourceUse1.Body, null);
            }
            else
            {
                resourceUse1.ResourceAcquisition = (Statement)this.Visit(resourceUse1.ResourceAcquisition, resourceUse2.ResourceAcquisition);
                resourceUse1.Body = this.VisitBlock(resourceUse1.Body, resourceUse2.Body);
            }
            return resourceUse1;
        }
        public virtual Statement VisitReturn(Return Return1, Return Return2)
        {
            if (Return1 == null) return null;
            if (Return2 == null)
                Return1.Expression = this.VisitExpression(Return1.Expression, null);
            else
                Return1.Expression = this.VisitExpression(Return1.Expression, Return2.Expression);
            return Return1;
        }
        public virtual SecurityAttribute VisitSecurityAttribute(SecurityAttribute attribute1, SecurityAttribute attribute2)
        {
            return attribute1;
        }
        public virtual SecurityAttributeList VisitSecurityAttributeList(SecurityAttributeList attributes1, SecurityAttributeList attributes2)
        {
            if (attributes1 == null) return null;
            for (int i = 0, n = attributes1.Count, m = attributes2 == null ? 0 : attributes2.Count; i < n; i++)
            {
                //^ assert attributes2 != null;
                if (i >= m)
                    attributes1[i] = this.VisitSecurityAttribute(attributes1[i], null);
                else
                    attributes1[i] = this.VisitSecurityAttribute(attributes1[i], attributes2[i]);
            }
            return attributes1;
        }
        public virtual Expression VisitSetterValue(SetterValue value1, SetterValue value2)
        {
            return value1;
        }
        public virtual StatementList VisitStatementList(StatementList statements1, StatementList statements2)
        {
            if (statements1 == null) return null;
            for (int i = 0, n = statements1.Count, m = statements2 == null ? 0 : statements2.Count; i < n; i++)
            {
                //^ assert statements2 != null;
                if (i >= m)
                    statements1[i] = (Statement)this.Visit(statements1[i], null);
                else
                    statements1[i] = (Statement)this.Visit(statements1[i], statements2[i]);
            }
            return statements1;
        }
        public virtual StatementSnippet VisitStatementSnippet(StatementSnippet snippet1, StatementSnippet snippet2)
        {
            return snippet1;
        }
        public virtual StaticInitializer VisitStaticInitializer(StaticInitializer cons1, StaticInitializer cons2)
        {
            return (StaticInitializer)this.VisitMethod(cons1, cons2);
        }
        public virtual Struct VisitStruct(Struct Struct1, Struct Struct2)
        {
            return (Struct)this.VisitTypeNode(Struct1, Struct2);
        }
        public virtual Statement VisitSwitch(Switch Switch1, Switch Switch2)
        {
            if (Switch1 == null) return null;
            if (Switch2 == null)
            {
                Switch1.Expression = this.VisitExpression(Switch1.Expression, null);
                Switch1.Cases = this.VisitSwitchCaseList(Switch1.Cases, null);
            }
            else
            {
                Switch1.Expression = this.VisitExpression(Switch1.Expression, Switch2.Expression);
                Switch1.Cases = this.VisitSwitchCaseList(Switch1.Cases, Switch2.Cases);
            }
            return Switch1;
        }
        public virtual SwitchCase VisitSwitchCase(SwitchCase switchCase1, SwitchCase switchCase2)
        {
            if (switchCase1 == null) return null;
            if (switchCase2 == null)
            {
                switchCase1.Label = this.VisitExpression(switchCase1.Label, null);
                switchCase1.Body = this.VisitBlock(switchCase1.Body, null);
            }
            else
            {
                switchCase1.Label = this.VisitExpression(switchCase1.Label, switchCase2.Label);
                switchCase1.Body = this.VisitBlock(switchCase1.Body, switchCase2.Body);
            }
            return switchCase1;
        }
        public virtual SwitchCaseList VisitSwitchCaseList(SwitchCaseList switchCases1, SwitchCaseList switchCases2)
        {
            if (switchCases1 == null) return null;
            for (int i = 0, n = switchCases1.Count, m = switchCases2 == null ? 0 : switchCases2.Count; i < n; i++)
            {
                //^ assert switchCases2 != null;
                if (i >= m)
                    switchCases1[i] = this.VisitSwitchCase(switchCases1[i], null);
                else
                    switchCases1[i] = this.VisitSwitchCase(switchCases1[i], switchCases2[i]);
            }
            return switchCases1;
        }
        public virtual Statement VisitSwitchInstruction(SwitchInstruction switchInstruction1, SwitchInstruction switchInstruction2)
        {
            if (switchInstruction1 == null) return null;
            if (switchInstruction2 == null)
                switchInstruction1.Expression = this.VisitExpression(switchInstruction1.Expression, null);
            else
                switchInstruction1.Expression = this.VisitExpression(switchInstruction1.Expression, switchInstruction2.Expression);
            return switchInstruction1;
        }
        public virtual Statement VisitTypeswitch(Typeswitch Typeswitch1, Typeswitch Typeswitch2)
        {
            if (Typeswitch1 == null) return null;
            if (Typeswitch2 == null)
            {
                Typeswitch1.Expression = this.VisitExpression(Typeswitch1.Expression, null);
                Typeswitch1.Cases = this.VisitTypeswitchCaseList(Typeswitch1.Cases, null);
            }
            else
            {
                Typeswitch1.Expression = this.VisitExpression(Typeswitch1.Expression, Typeswitch2.Expression);
                Typeswitch1.Cases = this.VisitTypeswitchCaseList(Typeswitch1.Cases, Typeswitch2.Cases);
            }
            return Typeswitch1;
        }
        public virtual TypeswitchCase VisitTypeswitchCase(TypeswitchCase typeswitchCase1, TypeswitchCase typeswitchCase2)
        {
            if (typeswitchCase1 == null) return null;
            if (typeswitchCase2 == null)
            {
                typeswitchCase1.LabelType = this.VisitTypeReference(typeswitchCase1.LabelType, null);
                typeswitchCase1.LabelVariable = this.VisitTargetExpression(typeswitchCase1.LabelVariable, null);
                typeswitchCase1.Body = this.VisitBlock(typeswitchCase1.Body, null);
            }
            else
            {
                typeswitchCase1.LabelType = this.VisitTypeReference(typeswitchCase1.LabelType, typeswitchCase2.LabelType);
                typeswitchCase1.LabelVariable = this.VisitTargetExpression(typeswitchCase1.LabelVariable, typeswitchCase2.LabelVariable);
                typeswitchCase1.Body = this.VisitBlock(typeswitchCase1.Body, typeswitchCase2.Body);
            }
            return typeswitchCase1;
        }
        public virtual TypeswitchCaseList VisitTypeswitchCaseList(TypeswitchCaseList typeswitchCases1, TypeswitchCaseList typeswitchCases2)
        {
            if (typeswitchCases1 == null) return null;
            for (int i = 0, n = typeswitchCases1.Count, m = typeswitchCases2 == null ? 0 : typeswitchCases2.Count; i < n; i++)
            {
                //^ assert typeswitchCases2 != null;
                if (i >= m)
                    typeswitchCases1[i] = this.VisitTypeswitchCase(typeswitchCases1[i], null);
                else
                    typeswitchCases1[i] = this.VisitTypeswitchCase(typeswitchCases1[i], typeswitchCases2[i]);
            }
            return typeswitchCases1;
        }
        public virtual Expression VisitTargetExpression(Expression expression1, Expression expression2)
        {
            return this.VisitExpression(expression1, expression2);
        }
        public virtual Expression VisitTernaryExpression(TernaryExpression expression1, TernaryExpression expression2)
        {
            if (expression1 == null) return null;
            if (expression2 == null)
            {
                expression1.Operand1 = this.VisitExpression(expression1.Operand1, null);
                expression1.Operand2 = this.VisitExpression(expression1.Operand2, null);
                expression1.Operand3 = this.VisitExpression(expression1.Operand3, null);
            }
            else
            {
                expression1.Operand1 = this.VisitExpression(expression1.Operand1, expression2.Operand1);
                expression1.Operand2 = this.VisitExpression(expression1.Operand2, expression2.Operand2);
                expression1.Operand3 = this.VisitExpression(expression1.Operand3, expression2.Operand3);
            }
            return expression1;
        }
        public virtual Expression VisitThis(This This1, This This2)
        {
            if (This1 == null) return null;
            if (This2 == null)
                This1.Type = this.VisitTypeReference(This1.Type, null);
            else
                This1.Type = this.VisitTypeReference(This1.Type, This2.Type);
            return This1;
        }
        public virtual Statement VisitThrow(Throw Throw1, Throw Throw2)
        {
            if (Throw1 == null) return null;
            if (Throw2 == null)
                Throw1.Expression = this.VisitExpression(Throw1.Expression, null);
            else
                Throw1.Expression = this.VisitExpression(Throw1.Expression, Throw2.Expression);
            return Throw1;
        }
        public virtual Statement VisitTry(Try Try1, Try Try2)
        {
            if (Try1 == null) return null;
            if (Try2 == null)
            {
                Try1.TryBlock = this.VisitBlock(Try1.TryBlock, null);
                Try1.Catchers = this.VisitCatchList(Try1.Catchers, null);
                Try1.Filters = this.VisitFilterList(Try1.Filters, null);
                Try1.FaultHandlers = this.VisitFaultHandlerList(Try1.FaultHandlers, null);
                Try1.Finally = (Finally)this.VisitFinally(Try1.Finally, null);
            }
            else
            {
                Try1.TryBlock = this.VisitBlock(Try1.TryBlock, Try2.TryBlock);
                Try1.Catchers = this.VisitCatchList(Try1.Catchers, Try2.Catchers);
                Try1.Filters = this.VisitFilterList(Try1.Filters, Try2.Filters);
                Try1.FaultHandlers = this.VisitFaultHandlerList(Try1.FaultHandlers, Try2.FaultHandlers);
                Try1.Finally = (Finally)this.VisitFinally(Try1.Finally, Try2.Finally);
            }
            return Try1;
        }
#if ExtendedRuntime    
    public virtual TupleType VisitTupleType(TupleType tuple1, TupleType tuple2){
      return (TupleType)this.VisitTypeNode(tuple1, tuple2);
    }
    public virtual TypeAlias VisitTypeAlias(TypeAlias tAlias1, TypeAlias tAlias2){
      if (tAlias1 == null) return null;
      if (tAlias2 == null){
        if (tAlias1.AliasedType is ConstrainedType)
          //The type alias defines the constrained type, rather than just referencing it
          tAlias1.AliasedType = this.VisitConstrainedType((ConstrainedType)tAlias1.AliasedType, null);
        else
          tAlias1.AliasedType = this.VisitTypeReference(tAlias1.AliasedType, null);
      }else{
        if (tAlias1.AliasedType is ConstrainedType)
          //The type alias defines the constrained type, rather than just referencing it
          tAlias1.AliasedType = this.VisitConstrainedType((ConstrainedType)tAlias1.AliasedType, tAlias2.AliasedType as ConstrainedType);
        else
          tAlias1.AliasedType = this.VisitTypeReference(tAlias1.AliasedType, tAlias2.AliasedType);
      }
      return tAlias1;
    }
    public virtual TypeIntersection VisitTypeIntersection(TypeIntersection typeIntersection1, TypeIntersection typeIntersection2){
      return (TypeIntersection)this.VisitTypeNode(typeIntersection1, typeIntersection2);
    }
    public virtual TypeContract VisitTypeContract(TypeContract contract1, TypeContract contract2) {
      if (contract1 == null) return null;
      if (contract2 == null) {
        // don't visit contract.DeclaringType
        // don't visit contract.InheitedContracts
        contract1.Invariants = this.VisitInvariantList(contract1.Invariants, null);
      }else{
        // don't visit contract.DeclaringType
        // don't visit contract.InheitedContracts
        contract1.Invariants = this.VisitInvariantList(contract1.Invariants, contract2.Invariants);
      }
      return contract1;
    }
#endif
        public virtual TypeMemberSnippet VisitTypeMemberSnippet(TypeMemberSnippet snippet1, TypeMemberSnippet snippet2)
        {
            return snippet1;
        }
        public virtual TypeModifier VisitTypeModifier(TypeModifier typeModifier1, TypeModifier typeModifier2)
        {
            if (typeModifier1 == null) return null;
            if (typeModifier2 == null)
            {
                typeModifier1.Modifier = this.VisitTypeReference(typeModifier1.Modifier, null);
                typeModifier1.ModifiedType = this.VisitTypeReference(typeModifier1.ModifiedType, null);
            }
            else
            {
                typeModifier1.Modifier = this.VisitTypeReference(typeModifier1.Modifier, typeModifier2.Modifier);
                typeModifier1.ModifiedType = this.VisitTypeReference(typeModifier1.ModifiedType, typeModifier2.ModifiedType);
            }
            return typeModifier1;
        }
        public virtual TypeNode VisitTypeNode(TypeNode typeNode1, TypeNode typeNode2)
        {
            if (typeNode1 == null) return null;
            if (typeNode2 == null)
            {
                typeNode1.Attributes = this.VisitAttributeList(typeNode1.Attributes, null);
                typeNode1.SecurityAttributes = this.VisitSecurityAttributeList(typeNode1.SecurityAttributes, null);
                Class c = typeNode1 as Class;
                if (c != null) c.BaseClass = (Class)this.VisitTypeReference(c.BaseClass, null);
                typeNode1.Interfaces = this.VisitInterfaceReferenceList(typeNode1.Interfaces, null);
                typeNode1.TemplateArguments = this.VisitTypeReferenceList(typeNode1.TemplateArguments, null);
                typeNode1.TemplateParameters = this.VisitTypeParameterList(typeNode1.TemplateParameters, null);
                typeNode1.Members = this.VisitMemberList(typeNode1.Members, null);
            }
            else
            {
                typeNode1.Attributes = this.VisitAttributeList(typeNode1.Attributes, typeNode2.Attributes);
                typeNode1.SecurityAttributes = this.VisitSecurityAttributeList(typeNode1.SecurityAttributes, typeNode2.SecurityAttributes);
                Class c1 = typeNode1 as Class;
                Class c2 = typeNode2 as Class;
                if (c1 != null) c1.BaseClass = (Class)this.VisitTypeReference(c1.BaseClass, c2 == null ? null : c2.BaseClass);
                typeNode1.Interfaces = this.VisitInterfaceReferenceList(typeNode1.Interfaces, typeNode2.Interfaces);
                typeNode1.TemplateArguments = this.VisitTypeReferenceList(typeNode1.TemplateArguments, typeNode2.TemplateArguments);
                typeNode1.TemplateParameters = this.VisitTypeParameterList(typeNode1.TemplateParameters, typeNode2.TemplateParameters);
                typeNode1.Members = this.VisitMemberList(typeNode1.Members, typeNode2.Members);
            }
            return typeNode1;
        }
        public virtual TypeNodeList VisitTypeNodeList(TypeNodeList types1, TypeNodeList types2)
        {
            if (types1 == null) return null;
            if (types2 == null)
            {
                for (int i = 0; i < types1.Count; i++) //Visiting a type may result in a new type being appended to this list
                    types1[i] = (TypeNode)this.Visit(types1[i], null);
            }
            else
            {
                for (int i = 0; i < types1.Count; i++)
                { //Visiting a type may result in a new type being appended to this list
                    if (i >= types2.Count)
                        types1[i] = (TypeNode)this.Visit(types1[i], null);
                    else
                        types1[i] = (TypeNode)this.Visit(types1[i], types2[i]);
                }
            }
            return types1;
        }
        public virtual TypeNode VisitTypeParameter(TypeNode typeParameter1, TypeNode typeParameter2)
        {
            if (typeParameter1 == null) return null;
            if (typeParameter2 == null)
                typeParameter1.Interfaces = this.VisitInterfaceReferenceList(typeParameter1.Interfaces, null);
            else
                typeParameter1.Interfaces = this.VisitInterfaceReferenceList(typeParameter1.Interfaces, typeParameter2.Interfaces);
            return typeParameter1;
        }
        public virtual TypeNodeList VisitTypeParameterList(TypeNodeList typeParameters1, TypeNodeList typeParameters2)
        {
            if (typeParameters1 == null) return null;
            for (int i = 0, n = typeParameters1.Count, m = typeParameters2 == null ? 0 : typeParameters2.Count; i < n; i++)
            {
                //^ assert typeParameters2 != null;
                if (i >= m)
                    typeParameters1[i] = this.VisitTypeParameter(typeParameters1[i], null);
                else
                    typeParameters1[i] = this.VisitTypeParameter(typeParameters1[i], typeParameters2[i]);
            }
            return typeParameters1;
        }
        public virtual TypeNode VisitTypeReference(TypeNode type1, TypeNode type2)
        {
            return type1;
        }
        public virtual TypeReference VisitTypeReference(TypeReference typeReference1, TypeReference variableDeclaration2)
        {
            if (typeReference1 == null) return null;
            if (variableDeclaration2 == null)
            {
                typeReference1.Type = this.VisitTypeReference(typeReference1.Type, null);
                typeReference1.Expression = this.VisitTypeReference(typeReference1.Expression, null);
            }
            else
            {
                typeReference1.Type = this.VisitTypeReference(typeReference1.Type, variableDeclaration2.Type);
                typeReference1.Expression = this.VisitTypeReference(typeReference1.Expression, variableDeclaration2.Expression);
            }
            return typeReference1;
        }
        public virtual TypeNodeList VisitTypeReferenceList(TypeNodeList typeReferences1, TypeNodeList typeReferences2)
        {
            if (typeReferences1 == null) return null;
            for (int i = 0, n = typeReferences1.Count, m = typeReferences2 == null ? 0 : typeReferences2.Count; i < n; i++)
            {
                //^ assert typeReferences2 != null;
                if (i >= m)
                    typeReferences1[i] = this.VisitTypeReference(typeReferences1[i], null);
                else
                    typeReferences1[i] = this.VisitTypeReference(typeReferences1[i], typeReferences2[i]);
            }
            return typeReferences1;
        }
#if ExtendedRuntime    
    public virtual TypeUnion VisitTypeUnion(TypeUnion typeUnion1, TypeUnion typeUnion2){
      return (TypeUnion)this.VisitTypeNode(typeUnion1, typeUnion2);
    }
#endif
        public virtual Expression VisitUnaryExpression(UnaryExpression unaryExpression1, UnaryExpression unaryExpression2)
        {
            if (unaryExpression1 == null) return null;
            if (unaryExpression2 == null)
                unaryExpression1.Operand = this.VisitExpression(unaryExpression1.Operand, null);
            else
                unaryExpression1.Operand = this.VisitExpression(unaryExpression1.Operand, unaryExpression2.Operand);
            return unaryExpression1;
        }
        public virtual Statement VisitVariableDeclaration(VariableDeclaration variableDeclaration1, VariableDeclaration variableDeclaration2)
        {
            if (variableDeclaration1 == null) return null;
            if (variableDeclaration2 == null)
            {
                variableDeclaration1.Type = this.VisitTypeReference(variableDeclaration1.Type, null);
                variableDeclaration1.Initializer = this.VisitExpression(variableDeclaration1.Initializer, null);
            }
            else
            {
                variableDeclaration1.Type = this.VisitTypeReference(variableDeclaration1.Type, variableDeclaration2.Type);
                variableDeclaration1.Initializer = this.VisitExpression(variableDeclaration1.Initializer, variableDeclaration2.Initializer);
            }
            return variableDeclaration1;
        }
        public virtual UsedNamespace VisitUsedNamespace(UsedNamespace usedNamespace1, UsedNamespace usedNamespace2)
        {
            return usedNamespace1;
        }
        public virtual UsedNamespaceList VisitUsedNamespaceList(UsedNamespaceList usedNspaces1, UsedNamespaceList usedNspaces2)
        {
            if (usedNspaces1 == null) return null;
            for (int i = 0, n = usedNspaces1.Count, m = usedNspaces2 == null ? 0 : usedNspaces2.Count; i < n; i++)
            {
                //^ assert usedNspaces2 != null;
                if (i >= m)
                    usedNspaces1[i] = this.VisitUsedNamespace(usedNspaces1[i], null);
                else
                    usedNspaces1[i] = this.VisitUsedNamespace(usedNspaces1[i], usedNspaces2[i]);
            }
            return usedNspaces1;
        }
        public virtual Statement VisitWhile(While While1, While While2)
        {
            if (While1 == null) return null;
            if (While2 == null)
            {
                While1.Invariants = this.VisitExpressionList(While1.Invariants, null);
                While1.Condition = this.VisitExpression(While1.Condition, null);
                While1.Body = this.VisitBlock(While1.Body, null);
            }
            else
            {
                While1.Invariants = this.VisitExpressionList(While1.Invariants, While2.Invariants);
                While1.Condition = this.VisitExpression(While1.Condition, While2.Condition);
                While1.Body = this.VisitBlock(While1.Body, While2.Body);
            }
            return While1;
        }
        public virtual Statement VisitYield(Yield Yield1, Yield Yield2)
        {
            if (Yield1 == null) return null;
            if (Yield2 == null)
                Yield1.Expression = this.VisitExpression(Yield1.Expression, null);
            else
                Yield1.Expression = this.VisitExpression(Yield1.Expression, Yield2.Expression);
            return Yield1;
        }
#if ExtendedRuntime
    // query nodes
    public virtual Node VisitQueryAggregate(QueryAggregate qa1, QueryAggregate qa2){
      if (qa1 == null) return null;
      if (qa2 == null)
        qa1.Expression = this.VisitExpression(qa1.Expression, null);
      else
        qa1.Expression = this.VisitExpression(qa1.Expression, qa2.Expression);
      return qa1;
    }
    public virtual Node VisitQueryAlias(QueryAlias alias1, QueryAlias alias2){
      if (alias1 == null) return null;
      if (alias2 == null)
        alias1.Expression = this.VisitExpression(alias1.Expression, null);
      else
        alias1.Expression = this.VisitExpression(alias1.Expression, alias2.Expression);
      return alias1;
    }
    public virtual Node VisitQueryAxis(QueryAxis axis1, QueryAxis axis2){
      if (axis1 == null) return null;
      if (axis2 == null)
        axis1.Source = this.VisitExpression(axis1.Source, null);
      else
        axis1.Source = this.VisitExpression(axis1.Source, axis2.Source);
      return axis1;
    }
    public virtual Node VisitQueryCommit(QueryCommit qc1, QueryCommit qc2){
      return qc1;
    }
    public virtual Node VisitQueryContext(QueryContext context1, QueryContext context2){
      return context1;
    }
    public virtual Node VisitQueryDelete(QueryDelete delete1, QueryDelete delete2){
      if (delete1 == null) return null;
      if (delete2 == null){
        delete1.Source = this.VisitExpression(delete1.Source, null);
        /*delete1.Target =*/ this.VisitExpression(delete1.Target, null); //REVIEW: why should this not be updated?
      }else{
        delete1.Source = this.VisitExpression(delete1.Source, delete2.Source);
        /*delete1.Target =*/ this.VisitExpression(delete1.Target, delete2.Target); //REVIEW: why should this not be updated?
      }
      return delete1;
    }
    public virtual Node VisitQueryDifference(QueryDifference diff1, QueryDifference diff2){
      if (diff1 == null) return null;
      if (diff2 == null){
        diff1.LeftSource = this.VisitExpression(diff1.LeftSource, null);
        diff1.RightSource = this.VisitExpression(diff1.RightSource, null);
      }else{
        diff1.LeftSource = this.VisitExpression(diff1.LeftSource, diff2.LeftSource);
        diff1.RightSource = this.VisitExpression(diff1.RightSource, diff2.RightSource);
      }
      return diff1;
    }
    public virtual Node VisitQueryDistinct(QueryDistinct distinct1, QueryDistinct distinct2){
      if (distinct1 == null) return null;
      if (distinct2 == null)
        distinct1.Source = this.VisitExpression(distinct1.Source, null);
      else
        distinct1.Source = this.VisitExpression(distinct1.Source, distinct2.Source);
      return distinct1;
    }
    public virtual Node VisitQueryExists(QueryExists exists1, QueryExists exists2){
      if (exists1 == null) return null;
      if (exists2 == null)
        exists1.Source = this.VisitExpression(exists1.Source, null);
      else
        exists1.Source = this.VisitExpression(exists1.Source, exists2.Source);
      return exists1;
    }
    public virtual Node VisitQueryFilter(QueryFilter filter1, QueryFilter filter2){
      if (filter1 == null) return null;
      if (filter2 == null){
        filter1.Source = this.VisitExpression(filter1.Source, null);
        filter1.Expression = this.VisitExpression(filter1.Expression, null);
      }else{
        filter1.Source = this.VisitExpression(filter1.Source, filter2.Source);
        filter1.Expression = this.VisitExpression(filter1.Expression, filter2.Expression);
      }
      return filter1;
    }
    public virtual Node VisitQueryGroupBy(QueryGroupBy groupby1, QueryGroupBy groupby2){
      if (groupby1 == null) return null;
      if (groupby2 == null){
        groupby1.Source = this.VisitExpression(groupby1.Source, null);
        groupby1.GroupList = this.VisitExpressionList(groupby1.GroupList, null);
        groupby1.Having = this.VisitExpression(groupby1.Having, null);
      }else{
        groupby1.Source = this.VisitExpression(groupby1.Source, groupby2.Source);
        groupby1.GroupList = this.VisitExpressionList(groupby1.GroupList, groupby2.GroupList);
        groupby1.Having = this.VisitExpression(groupby1.Having, groupby2.Having);
      }
      return groupby1;
    }
    public virtual Statement VisitQueryGeneratedType(QueryGeneratedType qgt1, QueryGeneratedType qgt2){
      return qgt1;
    }
    public virtual Node VisitQueryInsert(QueryInsert insert1, QueryInsert insert2){
      if (insert1 == null) return null;
      if (insert2 == null){
        insert1.Location = this.VisitExpression(insert1.Location, null);
        insert1.HintList = this.VisitExpressionList(insert1.HintList, null);
        insert1.InsertList = this.VisitExpressionList(insert1.InsertList, null);
      }else{
        insert1.Location = this.VisitExpression(insert1.Location, insert2.Location);
        insert1.HintList = this.VisitExpressionList(insert1.HintList, insert2.HintList);
        insert1.InsertList = this.VisitExpressionList(insert1.InsertList, insert2.InsertList);
      }
      return insert1;
    }
    public virtual Node VisitQueryIntersection(QueryIntersection intersection1, QueryIntersection intersection2){
      if (intersection1 == null) return null;
      if (intersection2 == null){
        intersection1.LeftSource = this.VisitExpression(intersection1.LeftSource, null);
        intersection1.RightSource = this.VisitExpression(intersection1.RightSource, null);
        intersection1.Type = intersection1.LeftSource == null ? null : intersection1.LeftSource.Type;
      }else{
        intersection1.LeftSource = this.VisitExpression(intersection1.LeftSource, intersection2.LeftSource);
        intersection1.RightSource = this.VisitExpression(intersection1.RightSource, intersection2.RightSource);
        intersection1.Type = intersection1.LeftSource == null ? null : intersection1.LeftSource.Type;
      }
      return intersection1;
    }
    public virtual Node VisitQueryIterator(QueryIterator xiterator1, QueryIterator xiterator2){
      if (xiterator1 == null) return null;
      if (xiterator2 == null){
        xiterator1.Expression = this.VisitExpression(xiterator1.Expression, null);
        xiterator1.HintList = this.VisitExpressionList(xiterator1.HintList, null);
      }else{
        xiterator1.Expression = this.VisitExpression(xiterator1.Expression, xiterator2.Expression);
        xiterator1.HintList = this.VisitExpressionList(xiterator1.HintList, xiterator2.HintList);
      }
      return xiterator1;      
    }
    public virtual Node VisitQueryJoin(QueryJoin join1, QueryJoin join2){
      if (join1 == null) return null;
      if (join2 == null){
        join1.LeftOperand = this.VisitExpression(join1.LeftOperand, null);
        join1.RightOperand = this.VisitExpression(join1.RightOperand, null);
        join1.JoinExpression = this.VisitExpression(join1.JoinExpression, null);
      }else{
        join1.LeftOperand = this.VisitExpression(join1.LeftOperand, join2.LeftOperand);
        join1.RightOperand = this.VisitExpression(join1.RightOperand, join2.RightOperand);
        join1.JoinExpression = this.VisitExpression(join1.JoinExpression, join2.JoinExpression);
      }
      return join1;
    }
    public virtual Node VisitQueryLimit(QueryLimit limit1, QueryLimit limit2){
      if (limit1 == null) return null;
      if (limit2 == null){
        limit1.Source = this.VisitExpression(limit1.Source, null);
        limit1.Expression = this.VisitExpression(limit1.Expression, null);
      }else{
        limit1.Source = this.VisitExpression(limit1.Source, limit2.Source);
        limit1.Expression = this.VisitExpression(limit1.Expression, limit2.Expression);
      }
      return limit1;
    }
    public virtual Node VisitQueryOrderBy(QueryOrderBy orderby1, QueryOrderBy orderby2){
      if (orderby1 == null) return null;
      if (orderby2 == null){
        orderby1.Source = this.VisitExpression(orderby1.Source, null);
        orderby1.OrderList = this.VisitExpressionList(orderby1.OrderList, null);
      }else{
        orderby1.Source = this.VisitExpression(orderby1.Source, orderby2.Source);
        orderby1.OrderList = this.VisitExpressionList(orderby1.OrderList, orderby2.OrderList);
      }
      return orderby1;
    }
    public virtual Node VisitQueryOrderItem(QueryOrderItem item1, QueryOrderItem item2){
      if (item1 == null) return null;
      if (item2 == null)
        item1.Expression = this.VisitExpression(item1.Expression, null);
      else
        item1.Expression = this.VisitExpression(item1.Expression, item2.Expression);
      return item1;
    }
    public virtual Node VisitQueryPosition(QueryPosition position1, QueryPosition position2){
      return position1;
    }
    public virtual Node VisitQueryProject(QueryProject project1, QueryProject project2){
      if (project1 == null) return null;
      if (project2 == null){
        project1.Source = this.VisitExpression(project1.Source, null);
        project1.ProjectionList = this.VisitExpressionList(project1.ProjectionList, null);
      }else{
        project1.Source = this.VisitExpression(project1.Source, project2.Source);
        project1.ProjectionList = this.VisitExpressionList(project1.ProjectionList, project2.ProjectionList);
      }
      return project1;
    }
    public virtual Node VisitQueryRollback(QueryRollback qr1, QueryRollback qr2){
      return qr1;
    }
    public virtual Node VisitQueryQuantifier(QueryQuantifier qq1, QueryQuantifier qq2){
      if (qq1 == null) return null;
      if (qq2 == null)
        qq1.Expression = this.VisitExpression(qq1.Expression, null);
      else
        qq1.Expression = this.VisitExpression(qq1.Expression, qq2.Expression);
      return qq1;
    }
    public virtual Node VisitQueryQuantifiedExpression(QueryQuantifiedExpression qqe1, QueryQuantifiedExpression qqe2){
      if (qqe1 == null) return null;
      if (qqe2 == null)
        qqe1.Expression = this.VisitExpression(qqe1.Expression, null);
      else
        qqe1.Expression = this.VisitExpression(qqe1.Expression, qqe2.Expression);
      return qqe1;
    }
    public virtual Node VisitQuerySelect(QuerySelect select1, QuerySelect select2){
      if (select1 == null) return null;
      if (select2 == null)
        select1.Source = this.VisitExpression(select1.Source, null);
      else
        select1.Source = this.VisitExpression(select1.Source, select2.Source);
      return select1;
    }
    public virtual Node VisitQuerySingleton(QuerySingleton singleton1, QuerySingleton singleton2){
      if (singleton1 == null) return null;
      if (singleton2 == null)
        singleton1.Source = this.VisitExpression(singleton1.Source, null);
      else
        singleton1.Source = this.VisitExpression(singleton1.Source, singleton2.Source);
      return singleton1;
    }
    public virtual Node VisitQueryTransact(QueryTransact qt1, QueryTransact qt2){
      if (qt1 == null) return null;
      if (qt2 == null){
        qt1.Source = this.VisitExpression(qt1.Source, null);
        qt1.Body = this.VisitBlock(qt1.Body, null);
        qt1.CommitBody = this.VisitBlock(qt1.CommitBody, null);
        qt1.RollbackBody = this.VisitBlock(qt1.RollbackBody, null);
      }else{
        qt1.Source = this.VisitExpression(qt1.Source, qt2.Source);
        qt1.Body = this.VisitBlock(qt1.Body, qt2.Body);
        qt1.CommitBody = this.VisitBlock(qt1.CommitBody, qt2.CommitBody);
        qt1.RollbackBody = this.VisitBlock(qt1.RollbackBody, qt2.RollbackBody);
      }
      return qt1;
    }
    public virtual Node VisitQueryTypeFilter(QueryTypeFilter filter1, QueryTypeFilter filter2){
      if (filter1 == null) return null;
      if (filter2 == null)
        filter1.Source = this.VisitExpression(filter1.Source, null);
      else
        filter1.Source = this.VisitExpression(filter1.Source, filter2.Source);
      return filter1;
    }
    public virtual Node VisitQueryUnion(QueryUnion union1, QueryUnion union2){
      if (union1 == null) return null;
      if (union2 == null){
        union1.LeftSource = this.VisitExpression(union1.LeftSource, null);
        union1.RightSource = this.VisitExpression(union1.RightSource, null);
      }else{
        union1.LeftSource = this.VisitExpression(union1.LeftSource, union2.LeftSource);
        union1.RightSource = this.VisitExpression(union1.RightSource, union2.RightSource);
      }
      return union1;
    }
    public virtual Node VisitQueryUpdate(QueryUpdate update1, QueryUpdate update2){
      if (update1 == null) return null;
      if (update2 == null){
        update1.Source = this.VisitExpression(update1.Source, null);
        update1.UpdateList = this.VisitExpressionList(update1.UpdateList, null);
      }else{
        update1.Source = this.VisitExpression(update1.Source, update2.Source);
        update1.UpdateList = this.VisitExpressionList(update1.UpdateList, update2.UpdateList);
      }
      return update1;
    }    
    public virtual Node VisitQueryYielder(QueryYielder yielder1, QueryYielder yielder2){
      if (yielder1 == null) return null;
      if (yielder2 == null){
        yielder1.Source = this.VisitExpression(yielder1.Source, null);
        yielder1.Target = this.VisitExpression(yielder1.Target, null);
        yielder1.Body = this.VisitBlock(yielder1.Body, null);
      }else{
        yielder1.Source = this.VisitExpression(yielder1.Source, yielder2.Source);
        yielder1.Target = this.VisitExpression(yielder1.Target, yielder2.Target);
        yielder1.Body = this.VisitBlock(yielder1.Body, yielder2.Body);
      }
      return yielder1;
    }
#endif
    }
}
#endif
