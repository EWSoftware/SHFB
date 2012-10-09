// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

#if !MinimalReader
using System;
using System.Diagnostics;

#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler
{
#endif
    public class UpdateSpecification
    {
        public Node Original;
        public Node Changes;
        public Node Deletions;
        public Node Insertions;
        //TODO: source change list?
    }
    public class Updater
    {
        public static void UpdateOriginal(UpdateSpecification update)
        {
            if (update == null) { Debug.Assert(false); return; }
            Updater updater = new Updater();
            updater.Visit(update.Original, update.Changes, update.Deletions, update.Insertions);
        }

        public Updater callingVisitor;
        public int currentSourcePositionDelta;
        public Document currentDocument;

        public Updater()
        {
        }
        public Updater(Updater callingVisitor)
        {
            this.callingVisitor = callingVisitor;
        }
        public virtual Node VisitUnknownNodeType(Node node, Node changes, Node deletions, Node insertions)
        {
            Updater visitor = this.GetVisitorFor(node);
            if (visitor == null) return node;
            if (this.callingVisitor != null)
                //Allow specialized state (unknown to this visitor) to propagate all the way down to the new visitor
                this.callingVisitor.TransferStateTo(visitor);
            this.TransferStateTo(visitor);
            node = visitor.Visit(node, changes, deletions, insertions);
            visitor.TransferStateTo(this);
            if (this.callingVisitor != null)
                //Propagate specialized state (unknown to this visitor) all the way up the chain
                visitor.TransferStateTo(this.callingVisitor);
            return node;
        }
        public virtual void TransferStateTo(Updater target)
        {
            if (target == null) return;
            target.currentSourcePositionDelta = this.currentSourcePositionDelta;
        }
        public virtual Updater GetVisitorFor(Node node)
        {
            if (node == null) return null;
            return (Updater)node.GetVisitorFor(this, this.GetType().Name);
        }
        public virtual Node Visit(Node node, Node changes, Node deletions, Node insertions)
        {
            if (node == null) return changes;
            switch (node.NodeType)
            {
                case NodeType.AddressDereference:
                    return this.VisitAddressDereference((AddressDereference)node, changes as AddressDereference, deletions as AddressDereference, insertions as AddressDereference);
                case NodeType.AliasDefinition:
                    return this.VisitAliasDefinition((AliasDefinition)node, changes as AliasDefinition, deletions as AliasDefinition, insertions as AliasDefinition);
                case NodeType.AnonymousNestedFunction:
                    return this.VisitAnonymousNestedFunction((AnonymousNestedFunction)node, changes as AnonymousNestedFunction, deletions as AnonymousNestedFunction, insertions as AnonymousNestedFunction);
                case NodeType.ApplyToAll:
                    return this.VisitApplyToAll((ApplyToAll)node, changes as ApplyToAll, deletions as ApplyToAll, insertions as ApplyToAll);
                case NodeType.Arglist:
                    return this.VisitExpression((Expression)node, changes as Expression, deletions as Expression, insertions as Expression);
                case NodeType.ArrayType:
                    Debug.Assert(false); return null;
                case NodeType.Assembly:
                    return this.VisitAssembly((AssemblyNode)node, changes as AssemblyNode, deletions as AssemblyNode, insertions as AssemblyNode);
                case NodeType.AssemblyReference:
                    return this.VisitAssemblyReference((AssemblyReference)node, changes as AssemblyReference, deletions as AssemblyReference, insertions as AssemblyReference);
                case NodeType.Assertion:
                    return this.VisitAssertion((Assertion)node, changes as Assertion, deletions as Assertion, insertions as Assertion);
                case NodeType.AssignmentExpression:
                    return this.VisitAssignmentExpression((AssignmentExpression)node, changes as AssignmentExpression, deletions as AssignmentExpression, insertions as AssignmentExpression);
                case NodeType.AssignmentStatement:
                    return this.VisitAssignmentStatement((AssignmentStatement)node, changes as AssignmentStatement, deletions as AssignmentStatement, insertions as AssignmentStatement);
                case NodeType.Attribute:
                    return this.VisitAttributeNode((AttributeNode)node, changes as AttributeNode, deletions as AttributeNode, insertions as AttributeNode);
                case NodeType.Base:
                    return this.VisitBase((Base)node, changes as Base, deletions as Base, insertions as Base);
                case NodeType.Block:
                    return this.VisitBlock((Block)node, changes as Block, deletions as Block, insertions as Block);
                case NodeType.BlockExpression:
                    return this.VisitBlockExpression((BlockExpression)node, changes as BlockExpression, deletions as BlockExpression, insertions as BlockExpression);
                case NodeType.Branch:
                    Debug.Assert(false); return null;
                case NodeType.Compilation:
                    return this.VisitCompilation((Compilation)node, changes as Compilation, deletions as Compilation, insertions as Compilation);
                case NodeType.CompilationUnit:
                    return this.VisitCompilationUnit((CompilationUnit)node, changes as CompilationUnit, deletions as CompilationUnit, insertions as CompilationUnit);
                case NodeType.CompilationUnitSnippet:
                    return this.VisitCompilationUnitSnippet((CompilationUnitSnippet)node, changes as CompilationUnitSnippet, deletions as CompilationUnitSnippet, insertions as CompilationUnitSnippet);
#if ExtendedRuntime
        case NodeType.ConstrainedType:
          return this.VisitConstrainedType((ConstrainedType)node, changes as ConstrainedType, deletions as ConstrainedType, insertions as ConstrainedType);
#endif
                case NodeType.Continue:
                    return this.VisitContinue((Continue)node, changes as Continue, deletions as Continue, insertions as Continue);
                case NodeType.CurrentClosure:
                    return this.VisitCurrentClosure((CurrentClosure)node, changes as CurrentClosure, deletions as CurrentClosure, insertions as CurrentClosure);
                case NodeType.DebugBreak:
                    return node;
                case NodeType.Call:
                case NodeType.Calli:
                case NodeType.Callvirt:
                case NodeType.Jmp:
                case NodeType.MethodCall:
                    return this.VisitMethodCall((MethodCall)node, changes as MethodCall, deletions as MethodCall, insertions as MethodCall);
                case NodeType.Catch:
                    return this.VisitCatch((Catch)node, changes as Catch, deletions as Catch, insertions as Catch);
                case NodeType.Class:
                    return this.VisitClass((Class)node, changes as Class, deletions as Class, insertions as Class);
                case NodeType.CoerceTuple:
                    return this.VisitCoerceTuple((CoerceTuple)node, changes as CoerceTuple, deletions as CoerceTuple, insertions as CoerceTuple);
                case NodeType.CollectionEnumerator:
                    return this.VisitCollectionEnumerator((CollectionEnumerator)node, changes as CollectionEnumerator, deletions as CollectionEnumerator, insertions as CollectionEnumerator);
                case NodeType.Composition:
                    return this.VisitComposition((Composition)node, changes as Composition, deletions as Composition, insertions as Composition);
                case NodeType.Construct:
                    return this.VisitConstruct((Construct)node, changes as Construct, deletions as Construct, insertions as Construct);
                case NodeType.ConstructArray:
                    return this.VisitConstructArray((ConstructArray)node, changes as ConstructArray, deletions as ConstructArray, insertions as ConstructArray);
                case NodeType.ConstructDelegate:
                    return this.VisitConstructDelegate((ConstructDelegate)node, changes as ConstructDelegate, deletions as ConstructDelegate, insertions as ConstructDelegate);
                case NodeType.ConstructFlexArray:
                    return this.VisitConstructFlexArray((ConstructFlexArray)node, changes as ConstructFlexArray, deletions as ConstructFlexArray, insertions as ConstructFlexArray);
                case NodeType.ConstructIterator:
                    return this.VisitConstructIterator((ConstructIterator)node, changes as ConstructIterator, deletions as ConstructIterator, insertions as ConstructIterator);
                case NodeType.ConstructTuple:
                    return this.VisitConstructTuple((ConstructTuple)node, changes as ConstructTuple, deletions as ConstructTuple, insertions as ConstructTuple);
                case NodeType.DelegateNode:
                    return this.VisitDelegateNode((DelegateNode)node, changes as DelegateNode, deletions as DelegateNode, insertions as DelegateNode);
                case NodeType.DoWhile:
                    return this.VisitDoWhile((DoWhile)node, changes as DoWhile, deletions as DoWhile, insertions as DoWhile);
                case NodeType.Dup:
                    return this.VisitExpression((Expression)node, changes as Expression, deletions as Expression, insertions as Expression);
                case NodeType.EndFilter:
                    return this.VisitEndFilter((EndFilter)node, changes as EndFilter, deletions as EndFilter, insertions as EndFilter);
                case NodeType.EndFinally:
                    return this.VisitEndFinally((EndFinally)node, changes as EndFinally, deletions as EndFinally, insertions as EndFinally);
                case NodeType.EnumNode:
                    return this.VisitEnumNode((EnumNode)node, changes as EnumNode, deletions as EnumNode, insertions as EnumNode);
                case NodeType.Event:
                    return this.VisitEvent((Event)node, changes as Event, deletions as Event, insertions as Event);
                case NodeType.Exit:
                    return this.VisitExit((Exit)node, changes as Exit, deletions as Exit, insertions as Exit);
                case NodeType.ExpressionSnippet:
                    return this.VisitExpressionSnippet((ExpressionSnippet)node, changes as ExpressionSnippet, deletions as ExpressionSnippet, insertions as ExpressionSnippet);
                case NodeType.ExpressionStatement:
                    return this.VisitExpressionStatement((ExpressionStatement)node, changes as ExpressionStatement, deletions as ExpressionStatement, insertions as ExpressionStatement);
                case NodeType.FaultHandler:
                    return this.VisitFaultHandler((FaultHandler)node, changes as FaultHandler, deletions as FaultHandler, insertions as FaultHandler);
                case NodeType.Field:
                    return this.VisitField((Field)node, changes as Field, deletions as Field, insertions as Field);
                case NodeType.FieldInitializerBlock:
                    return this.VisitFieldInitializerBlock((FieldInitializerBlock)node, changes as FieldInitializerBlock, deletions as FieldInitializerBlock, insertions as FieldInitializerBlock);
                case NodeType.Finally:
                    return this.VisitFinally((Finally)node, changes as Finally, deletions as Finally, insertions as Finally);
                case NodeType.Filter:
                    return this.VisitFilter((Filter)node, changes as Filter, deletions as Filter, insertions as Filter);
                case NodeType.Fixed:
                    return this.VisitFixed((Fixed)node, changes as Fixed, deletions as Fixed, insertions as Fixed);
                case NodeType.For:
                    return this.VisitFor((For)node, changes as For, deletions as For, insertions as For);
                case NodeType.ForEach:
                    return this.VisitForEach((ForEach)node, changes as ForEach, deletions as ForEach, insertions as ForEach);
                case NodeType.FunctionDeclaration:
                    return this.VisitFunctionDeclaration((FunctionDeclaration)node, changes as FunctionDeclaration, deletions as FunctionDeclaration, insertions as FunctionDeclaration);
                case NodeType.Goto:
                    return this.VisitGoto((Goto)node, changes as Goto, deletions as Goto, insertions as Goto);
                case NodeType.GotoCase:
                    return this.VisitGotoCase((GotoCase)node, changes as GotoCase, deletions as GotoCase, insertions as GotoCase);
                case NodeType.Identifier:
                    return this.VisitIdentifier((Identifier)node, changes as Identifier, deletions as Identifier, insertions as Identifier);
                case NodeType.If:
                    return this.VisitIf((If)node, changes as If, deletions as If, insertions as If);
                case NodeType.ImplicitThis:
                    return this.VisitImplicitThis((ImplicitThis)node, changes as ImplicitThis, deletions as ImplicitThis, insertions as ImplicitThis);
                case NodeType.Indexer:
                    return this.VisitIndexer((Indexer)node, changes as Indexer, deletions as Indexer, insertions as Indexer);
                case NodeType.InstanceInitializer:
                    return this.VisitInstanceInitializer((InstanceInitializer)node, changes as InstanceInitializer, deletions as InstanceInitializer, insertions as InstanceInitializer);
                case NodeType.StaticInitializer:
                    return this.VisitStaticInitializer((StaticInitializer)node, changes as StaticInitializer, deletions as StaticInitializer, insertions as StaticInitializer);
                case NodeType.Method:
                    return this.VisitMethod((Method)node, changes as Method, deletions as Method, insertions as Method);
                case NodeType.Interface:
                    return this.VisitInterface((Interface)node, changes as Interface, deletions as Interface, insertions as Interface);
                case NodeType.LabeledStatement:
                    return this.VisitLabeledStatement((LabeledStatement)node, changes as LabeledStatement, deletions as LabeledStatement, insertions as LabeledStatement);
                case NodeType.Literal:
                    return this.VisitLiteral((Literal)node, changes as Literal, deletions as Literal, insertions as Literal);
                case NodeType.Local:
                    return this.VisitLocal((Local)node, changes as Local, deletions as Local, insertions as Local);
                case NodeType.LocalDeclaration:
                    return this.VisitLocalDeclaration((LocalDeclaration)node, changes as LocalDeclaration, deletions as LocalDeclaration, insertions as LocalDeclaration);
                case NodeType.LocalDeclarationsStatement:
                    return this.VisitLocalDeclarationsStatement((LocalDeclarationsStatement)node, changes as LocalDeclarationsStatement, deletions as LocalDeclarationsStatement, insertions as LocalDeclarationsStatement);
                case NodeType.Lock:
                    return this.VisitLock((Lock)node, changes as Lock, deletions as Lock, insertions as Lock);
                case NodeType.LRExpression:
                    return this.VisitLRExpression((LRExpression)node, changes as LRExpression, deletions as LRExpression, insertions as LRExpression);
                case NodeType.MemberBinding:
                    return this.VisitMemberBinding((MemberBinding)node, changes as MemberBinding, deletions as MemberBinding, insertions as MemberBinding);
                case NodeType.TemplateInstance:
                    return this.VisitTemplateInstance((TemplateInstance)node, changes as TemplateInstance, deletions as TemplateInstance, insertions as TemplateInstance);
                case NodeType.StackAlloc:
                    return this.VisitStackAlloc((StackAlloc)node, changes as StackAlloc, deletions as StackAlloc, insertions as StackAlloc);
                case NodeType.Module:
                    return this.VisitModule((Module)node, changes as Module, deletions as Module, insertions as Module);
                case NodeType.ModuleReference:
                    return this.VisitModuleReference((ModuleReference)node, changes as ModuleReference, deletions as ModuleReference, insertions as ModuleReference);
                case NodeType.NameBinding:
                    return this.VisitNameBinding((NameBinding)node, changes as NameBinding, deletions as NameBinding, insertions as NameBinding);
                case NodeType.NamedArgument:
                    return this.VisitNamedArgument((NamedArgument)node, changes as NamedArgument, deletions as NamedArgument, insertions as NamedArgument);
                case NodeType.Namespace:
                    return this.VisitNamespace((Namespace)node, changes as Namespace, deletions as Namespace, insertions as Namespace);
                case NodeType.Nop:
                    return node;
                case NodeType.OptionalModifier:
                case NodeType.RequiredModifier:
                    return this.VisitTypeModifier((TypeModifier)node, changes as TypeModifier, deletions as TypeModifier, insertions as TypeModifier);
                case NodeType.Parameter:
                    return this.VisitParameter((Parameter)node, changes as Parameter, deletions as Parameter, insertions as Parameter);
                case NodeType.Pop:
                    return this.VisitExpression((Expression)node, changes as Expression, deletions as Expression, insertions as Expression);
                case NodeType.PrefixExpression:
                    return this.VisitPrefixExpression((PrefixExpression)node, changes as PrefixExpression, deletions as PrefixExpression, insertions as PrefixExpression);
                case NodeType.PostfixExpression:
                    return this.VisitPostfixExpression((PostfixExpression)node, changes as PostfixExpression, deletions as PostfixExpression, insertions as PostfixExpression);
                case NodeType.Property:
                    return this.VisitProperty((Property)node, changes as Property, deletions as Property, insertions as Property);
                case NodeType.QualifiedIdentifer:
                    return this.VisitQualifiedIdentifier((QualifiedIdentifier)node, changes as QualifiedIdentifier, deletions as QualifiedIdentifier, insertions as QualifiedIdentifier);
                case NodeType.Rethrow:
                case NodeType.Throw:
                    return this.VisitThrow((Throw)node, changes as Throw, deletions as Throw, insertions as Throw);
                case NodeType.Return:
                    return this.VisitReturn((Return)node, changes as Return, deletions as Return, insertions as Return);
                case NodeType.ResourceUse:
                    return this.VisitResourceUse((ResourceUse)node, changes as ResourceUse, deletions as ResourceUse, insertions as ResourceUse);
                case NodeType.Repeat:
                    return this.VisitRepeat((Repeat)node, changes as Repeat, deletions as Repeat, insertions as Repeat);
                case NodeType.SecurityAttribute:
                    return this.VisitSecurityAttribute((SecurityAttribute)node, changes as SecurityAttribute, deletions as SecurityAttribute, insertions as SecurityAttribute);
                case NodeType.SetterValue:
                    return this.VisitSetterValue((SetterValue)node, changes as SetterValue, deletions as SetterValue, insertions as SetterValue);
                case NodeType.StatementSnippet:
                    return this.VisitStatementSnippet((StatementSnippet)node, changes as StatementSnippet, deletions as StatementSnippet, insertions as StatementSnippet);
                case NodeType.Struct:
                    return this.VisitStruct((Struct)node, changes as Struct, deletions as Struct, insertions as Struct);
                case NodeType.Switch:
                    return this.VisitSwitch((Switch)node, changes as Switch, deletions as Switch, insertions as Switch);
                case NodeType.SwitchCase:
                    return this.VisitSwitchCase((SwitchCase)node, changes as SwitchCase, deletions as SwitchCase, insertions as SwitchCase);
                case NodeType.SwitchInstruction:
                    return this.VisitSwitchInstruction((SwitchInstruction)node, changes as SwitchInstruction, deletions as SwitchInstruction, insertions as SwitchInstruction);
                case NodeType.Typeswitch:
                    return this.VisitTypeswitch((Typeswitch)node, changes as Typeswitch, deletions as Typeswitch, insertions as Typeswitch);
                case NodeType.TypeswitchCase:
                    return this.VisitTypeswitchCase((TypeswitchCase)node, changes as TypeswitchCase, deletions as TypeswitchCase, insertions as TypeswitchCase);
                case NodeType.This:
                    return this.VisitThis((This)node, changes as This, deletions as This, insertions as This);
                case NodeType.Try:
                    return this.VisitTry((Try)node, changes as Try, deletions as Try, insertions as Try);
#if ExtendedRuntime
        case NodeType.TupleType:
          return this.VisitTupleType((TupleType)node, changes as TupleType, deletions as TupleType, insertions as TupleType);
        case NodeType.TypeAlias:
          return this.VisitTypeAlias((TypeAlias)node, changes as TypeAlias, deletions as TypeAlias, insertions as TypeAlias);
        case NodeType.TypeIntersection:
          return this.VisitTypeIntersection((TypeIntersection)node, changes as TypeIntersection, deletions as TypeIntersection, insertions as TypeIntersection);
#endif
                case NodeType.TypeMemberSnippet:
                    return this.VisitTypeMemberSnippet((TypeMemberSnippet)node, changes as TypeMemberSnippet, deletions as TypeMemberSnippet, insertions as TypeMemberSnippet);
                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                    return this.VisitTypeParameter((TypeNode)node, changes as TypeNode, deletions as TypeNode, insertions as TypeNode);
                case NodeType.TypeReference:
                    return this.VisitTypeReference((TypeReference)node, changes as TypeReference, deletions as TypeReference, insertions as TypeReference);
#if ExtendedRuntime
        case NodeType.TypeUnion:
          return this.VisitTypeUnion((TypeUnion)node, changes as TypeUnion, deletions as TypeUnion, insertions as TypeUnion);
#endif
                case NodeType.UsedNamespace:
                    return this.VisitUsedNamespace((UsedNamespace)node, changes as UsedNamespace, deletions as UsedNamespace, insertions as UsedNamespace);
                case NodeType.VariableDeclaration:
                    return this.VisitVariableDeclaration((VariableDeclaration)node, changes as VariableDeclaration, deletions as VariableDeclaration, insertions as VariableDeclaration);
                case NodeType.While:
                    return this.VisitWhile((While)node, changes as While, deletions as While, insertions as While);
                case NodeType.Yield:
                    return this.VisitYield((Yield)node, changes as Yield, deletions as Yield, insertions as Yield);

                case NodeType.Conditional:
                case NodeType.Cpblk:
                case NodeType.Initblk:
                    return this.VisitTernaryExpression((TernaryExpression)node, changes as TernaryExpression, deletions as TernaryExpression, insertions as TernaryExpression);

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
                case NodeType.Maplet:
                case NodeType.Mkrefany:
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
                    return this.VisitBinaryExpression((BinaryExpression)node, changes as BinaryExpression, deletions as BinaryExpression, insertions as BinaryExpression);

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
                    return this.VisitUnaryExpression((UnaryExpression)node, changes as UnaryExpression, deletions as UnaryExpression, insertions as UnaryExpression);
#if ExtendedRuntime
          // query node types
        case NodeType.QueryAggregate:
          return this.VisitQueryAggregate((QueryAggregate)node, changes as QueryAggregate, deletions as QueryAggregate, insertions as QueryAggregate);
        case NodeType.QueryAlias:
          return this.VisitQueryAlias((QueryAlias)node, changes as QueryAlias, deletions as QueryAlias, insertions as QueryAlias);
        case NodeType.QueryAll:
        case NodeType.QueryAny:
          return this.VisitQueryQuantifier((QueryQuantifier)node, changes as QueryQuantifier, deletions as QueryQuantifier, insertions as QueryQuantifier);
        case NodeType.QueryAxis:
          return this.VisitQueryAxis((QueryAxis)node, changes as QueryAxis, deletions as QueryAxis, insertions as QueryAxis);
        case NodeType.QueryCommit:
          return this.VisitQueryCommit((QueryCommit)node, changes as QueryCommit, deletions as QueryCommit, insertions as QueryCommit);
        case NodeType.QueryContext:
          return this.VisitQueryContext((QueryContext)node, changes as QueryContext, deletions as QueryContext, insertions as QueryContext);
        case NodeType.QueryDelete:
          return this.VisitQueryDelete((QueryDelete)node, changes as QueryDelete, deletions as QueryDelete, insertions as QueryDelete);
        case NodeType.QueryDifference:
          return this.VisitQueryDifference((QueryDifference)node, changes as QueryDifference, deletions as QueryDifference, insertions as QueryDifference);
        case NodeType.QueryDistinct:
          return this.VisitQueryDistinct((QueryDistinct)node, changes as QueryDistinct, deletions as QueryDistinct, insertions as QueryDistinct);
        case NodeType.QueryExists:
          return this.VisitQueryExists((QueryExists)node, changes as QueryExists, deletions as QueryExists, insertions as QueryExists);
        case NodeType.QueryFilter:
          return this.VisitQueryFilter((QueryFilter)node, changes as QueryFilter, deletions as QueryFilter, insertions as QueryFilter);
        case NodeType.QueryGeneratedType:
          return this.VisitQueryGeneratedType((QueryGeneratedType)node, changes as QueryGeneratedType, deletions as QueryGeneratedType, insertions as QueryGeneratedType);
        case NodeType.QueryGroupBy:
          return this.VisitQueryGroupBy((QueryGroupBy)node, changes as QueryGroupBy, deletions as QueryGroupBy, insertions as QueryGroupBy);
        case NodeType.QueryInsert:
          return this.VisitQueryInsert((QueryInsert)node, changes as QueryInsert, deletions as QueryInsert, insertions as QueryInsert);
        case NodeType.QueryIntersection:
          return this.VisitQueryIntersection((QueryIntersection)node, changes as QueryIntersection, deletions as QueryIntersection, insertions as QueryIntersection);
        case NodeType.QueryIterator:
          return this.VisitQueryIterator((QueryIterator)node, changes as QueryIterator, deletions as QueryIterator, insertions as QueryIterator);
        case NodeType.QueryJoin:
          return this.VisitQueryJoin((QueryJoin)node, changes as QueryJoin, deletions as QueryJoin, insertions as QueryJoin);
        case NodeType.QueryLimit:
          return this.VisitQueryLimit((QueryLimit)node, changes as QueryLimit, deletions as QueryLimit, insertions as QueryLimit);
        case NodeType.QueryOrderBy:        
          return this.VisitQueryOrderBy((QueryOrderBy)node, changes as QueryOrderBy, deletions as QueryOrderBy, insertions as QueryOrderBy);
        case NodeType.QueryOrderItem:
          return this.VisitQueryOrderItem((QueryOrderItem)node, changes as QueryOrderItem, deletions as QueryOrderItem, insertions as QueryOrderItem);
        case NodeType.QueryPosition:
          return this.VisitQueryPosition((QueryPosition)node, changes as QueryPosition, deletions as QueryPosition, insertions as QueryPosition);
        case NodeType.QueryProject:
          return this.VisitQueryProject((QueryProject)node, changes as QueryProject, deletions as QueryProject, insertions as QueryProject);          
        case NodeType.QueryQuantifiedExpression:
          return this.VisitQueryQuantifiedExpression((QueryQuantifiedExpression)node, changes as QueryQuantifiedExpression, deletions as QueryQuantifiedExpression, insertions as QueryQuantifiedExpression);
        case NodeType.QueryRollback:
          return this.VisitQueryRollback((QueryRollback)node, changes as QueryRollback, deletions as QueryRollback, insertions as QueryRollback);
        case NodeType.QuerySelect:
          return this.VisitQuerySelect((QuerySelect)node, changes as QuerySelect, deletions as QuerySelect, insertions as QuerySelect);
        case NodeType.QuerySingleton:
          return this.VisitQuerySingleton((QuerySingleton)node, changes as QuerySingleton, deletions as QuerySingleton, insertions as QuerySingleton);
        case NodeType.QueryTransact:
          return this.VisitQueryTransact((QueryTransact)node, changes as QueryTransact, deletions as QueryTransact, insertions as QueryTransact);
        case NodeType.QueryTypeFilter:
          return this.VisitQueryTypeFilter((QueryTypeFilter)node, changes as QueryTypeFilter, deletions as QueryTypeFilter, insertions as QueryTypeFilter);
        case NodeType.QueryUnion:
          return this.VisitQueryUnion((QueryUnion)node, changes as QueryUnion, deletions as QueryUnion, insertions as QueryUnion);
        case NodeType.QueryUpdate:
          return this.VisitQueryUpdate((QueryUpdate)node, changes as QueryUpdate, deletions as QueryUpdate, insertions as QueryUpdate);
        case NodeType.QueryYielder:
          return this.VisitQueryYielder((QueryYielder)node, changes as QueryYielder, deletions as QueryYielder, insertions as QueryYielder);
#endif
                default:
                    return this.VisitUnknownNodeType(node, changes, deletions, insertions);
            }
        }
        public virtual void UpdateSourceContext(Node node, Node changes)
        {
        }
        public virtual Expression VisitAddressDereference(AddressDereference addr, AddressDereference changes, AddressDereference deletions, AddressDereference insertions)
        {
            this.UpdateSourceContext(addr, changes);
            if (addr == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    addr.Address = this.VisitExpression(addr.Address, changes.Address, deletions.Address, insertions.Address);
                    addr.Alignment = changes.Alignment;
                    addr.Volatile = changes.Volatile;
                }
            }
            else if (deletions != null)
                return null;
            return addr;
        }
        public virtual AliasDefinition VisitAliasDefinition(AliasDefinition aliasDefinition, AliasDefinition changes, AliasDefinition deletions, AliasDefinition insertions)
        {
            this.UpdateSourceContext(aliasDefinition, changes);
            if (aliasDefinition == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    aliasDefinition.Alias = changes.Alias;
                    aliasDefinition.AliasedType = this.VisitTypeReference(aliasDefinition.AliasedType, changes.AliasedType, deletions.AliasedType, insertions.AliasedType);
                    aliasDefinition.AliasedExpression = changes.AliasedExpression;
                    aliasDefinition.AliasedUri = changes.AliasedUri;
                }
            }
            else if (deletions != null)
                return null;
            return aliasDefinition;
        }
        public virtual AliasDefinitionList VisitAliasDefinitionList(AliasDefinitionList aliasDefinitions, AliasDefinitionList changes, AliasDefinitionList deletions, AliasDefinitionList insertions)
        {
            if (changes == null || deletions == null || insertions == null) return aliasDefinitions;
            int n = aliasDefinitions == null ? 0 : aliasDefinitions.Count;
            if (n > changes.Count) { Debug.Assert(false); n = changes.Count; }
            if (n > deletions.Count) { Debug.Assert(false); n = deletions.Count; }
            if (n > insertions.Count) { Debug.Assert(false); n = insertions.Count; }
            if (aliasDefinitions != null)
                for (int i = 0; i < n; i++)
                    aliasDefinitions[i] = this.VisitAliasDefinition(aliasDefinitions[i], changes[i], deletions[i], insertions[i]);
            AliasDefinitionList result = new AliasDefinitionList(insertions.Count - n);
            for (int i = n, m = insertions.Count; i < m; i++)
                result.Add(insertions[i]);
            return result;
        }
        public virtual Expression VisitAnonymousNestedFunction(AnonymousNestedFunction func, AnonymousNestedFunction changes, AnonymousNestedFunction deletions, AnonymousNestedFunction insertions)
        {
            this.UpdateSourceContext(func, changes);
            if (func == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    func.Body = this.VisitBlock(func.Body, changes.Body, deletions.Body, insertions.Body);
                    func.Parameters = this.VisitParameterList(func.Parameters, changes.Parameters, deletions.Parameters, insertions.Parameters);
                }
            }
            else if (deletions != null)
                return null;
            return func;
        }
        public virtual Expression VisitApplyToAll(ApplyToAll applyToAll, ApplyToAll changes, ApplyToAll deletions, ApplyToAll insertions)
        {
            this.UpdateSourceContext(applyToAll, changes);
            if (applyToAll == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    applyToAll.Operand1 = this.VisitExpression(applyToAll.Operand1, changes.Operand1, deletions.Operand1, insertions.Operand1);
                    applyToAll.Operand2 = this.VisitExpression(applyToAll.Operand2, changes.Operand2, deletions.Operand2, insertions.Operand2);
                }
            }
            else if (deletions != null)
                return null;
            return applyToAll;
        }
        public virtual AssemblyNode VisitAssembly(AssemblyNode assembly, AssemblyNode changes, AssemblyNode deletions, AssemblyNode insertions)
        {
            this.UpdateSourceContext(assembly, changes);
            if (assembly == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    assembly.AssemblyReferences = this.VisitAssemblyReferenceList(assembly.AssemblyReferences, changes.AssemblyReferences, deletions.AssemblyReferences, insertions.AssemblyReferences);
                    assembly.Attributes = this.VisitAttributeList(assembly.Attributes, changes.Attributes, deletions.Attributes, insertions.Attributes);
                    assembly.Culture = changes.Culture;
                    assembly.ExportedTypes = this.VisitTypeReferenceList(assembly.ExportedTypes, changes.ExportedTypes, deletions.ExportedTypes, insertions.ExportedTypes);
                    assembly.Flags = changes.Flags;
                    assembly.Kind = changes.Kind;
                    assembly.ModuleAttributes = this.VisitAttributeList(assembly.ModuleAttributes, changes.ModuleAttributes, deletions.ModuleAttributes, insertions.ModuleAttributes);
                    assembly.ModuleReferences = this.VisitModuleReferenceList(assembly.ModuleReferences, changes.ModuleReferences, deletions.ModuleReferences, insertions.ModuleReferences);
                    assembly.Name = changes.Name;
                    assembly.SecurityAttributes = this.VisitSecurityAttributeList(assembly.SecurityAttributes, changes.SecurityAttributes, deletions.SecurityAttributes, insertions.SecurityAttributes);
                    assembly.Types = this.VisitTypeNodeList(assembly.Types, changes.Types, deletions.Types, insertions.Types);
                    assembly.Version = changes.Version;
                }
            }
            else if (deletions != null)
                return null;
            return assembly;
        }
        public virtual AssemblyReference VisitAssemblyReference(AssemblyReference assemblyReference, AssemblyReference changes, AssemblyReference deletions, AssemblyReference insertions)
        {
            this.UpdateSourceContext(assemblyReference, changes);
            if (assemblyReference == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    assemblyReference.Culture = changes.Culture;
                    assemblyReference.Flags = changes.Flags;
                    assemblyReference.Name = changes.Name;
                    assemblyReference.PublicKeyOrToken = changes.PublicKeyOrToken;
                    assemblyReference.Version = changes.Version;
                }
            }
            else if (deletions != null)
                return null;
            return assemblyReference;
        }
        public virtual AssemblyReferenceList VisitAssemblyReferenceList(AssemblyReferenceList assemblyReferences, AssemblyReferenceList changes, AssemblyReferenceList deletions, AssemblyReferenceList insertions)
        {
            if (changes == null || deletions == null || insertions == null) return assemblyReferences;
            int n = assemblyReferences == null ? 0 : assemblyReferences.Count;
            if (n > changes.Count) { Debug.Assert(false); n = changes.Count; }
            if (n > deletions.Count) { Debug.Assert(false); n = deletions.Count; }
            if (n > insertions.Count) { Debug.Assert(false); n = insertions.Count; }
            if (assemblyReferences != null)
                for (int i = 0; i < n; i++)
                    assemblyReferences[i] = this.VisitAssemblyReference(assemblyReferences[i], changes[i], deletions[i], insertions[i]);
            AssemblyReferenceList result = new AssemblyReferenceList(insertions.Count - n);
            for (int i = n, m = insertions.Count; i < m; i++)
                result.Add(insertions[i]);
            return result;
        }
        public virtual Statement VisitAssertion(Assertion assertion, Assertion changes, Assertion deletions, Assertion insertions)
        {
            this.UpdateSourceContext(assertion, changes);
            if (assertion == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    assertion.Condition = this.VisitExpression(assertion.Condition, changes.Condition, deletions.Condition, insertions.Condition);
                }
            }
            else if (deletions != null)
                return null;
            return assertion;
        }
        public virtual Expression VisitAssignmentExpression(AssignmentExpression assignment, AssignmentExpression changes, AssignmentExpression deletions, AssignmentExpression insertions)
        {
            this.UpdateSourceContext(assignment, changes);
            if (assignment == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    assignment.AssignmentStatement = this.VisitAssignmentStatement(assignment.AssignmentStatement as AssignmentStatement, changes.AssignmentStatement as AssignmentStatement, deletions.AssignmentStatement as AssignmentStatement, insertions.AssignmentStatement as AssignmentStatement);
                }
            }
            else if (deletions != null)
                return null;
            return assignment;
        }
        public virtual Statement VisitAssignmentStatement(AssignmentStatement assignment, AssignmentStatement changes, AssignmentStatement deletions, AssignmentStatement insertions)
        {
            this.UpdateSourceContext(assignment, changes);
            if (assignment == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    assignment.Operator = changes.Operator;
                    assignment.Source = this.VisitExpression(assignment.Source, changes.Source, deletions.Source, insertions.Source);
                    assignment.Target = this.VisitExpression(assignment.Target, changes.Target, deletions.Target, insertions.Target);
                }
            }
            else if (deletions != null)
                return null;
            return assignment;
        }
        public virtual AttributeNode VisitAttributeNode(AttributeNode attribute, AttributeNode changes, AttributeNode deletions, AttributeNode insertions)
        {
            this.UpdateSourceContext(attribute, changes);
            if (attribute == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    attribute.AllowMultiple = changes.AllowMultiple;
                    attribute.Constructor = this.VisitExpression(attribute.Constructor, changes.Constructor, deletions.Constructor, insertions.Constructor);
                    attribute.Expressions = this.VisitExpressionList(attribute.Expressions, changes.Expressions, deletions.Expressions, insertions.Expressions);
                    attribute.Target = changes.Target;
                }
            }
            else if (deletions != null)
                return null;
            return attribute;
        }
        public virtual AttributeList VisitAttributeList(AttributeList attributes, AttributeList changes, AttributeList deletions, AttributeList insertions)
        {
            if (changes == null || deletions == null || insertions == null) return attributes;
            int n = attributes == null ? 0 : attributes.Count;
            if (n > changes.Count) { Debug.Assert(false); n = changes.Count; }
            if (n > deletions.Count) { Debug.Assert(false); n = deletions.Count; }
            if (n > insertions.Count) { Debug.Assert(false); n = insertions.Count; }
            if (attributes != null)
                for (int i = 0; i < n; i++)
                    attributes[i] = this.VisitAttributeNode(attributes[i], changes[i], deletions[i], insertions[i]);
            AttributeList result = new AttributeList(insertions.Count - n);
            for (int i = n, m = insertions.Count; i < m; i++)
                result.Add(insertions[i]);
            return result;
        }
        public virtual Expression VisitBase(Base Base, Base changes, Base deletions, Base insertions)
        {
            this.UpdateSourceContext(Base, changes);
            if (Base == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return Base;
        }
        public virtual Expression VisitBinaryExpression(BinaryExpression binaryExpression, BinaryExpression changes, BinaryExpression deletions, BinaryExpression insertions)
        {
            this.UpdateSourceContext(binaryExpression, changes);
            if (binaryExpression == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    binaryExpression.NodeType = changes.NodeType;
                    binaryExpression.Operand1 = this.VisitExpression(binaryExpression.Operand1, changes.Operand1, deletions.Operand1, insertions.Operand1);
                    binaryExpression.Operand2 = this.VisitExpression(binaryExpression.Operand2, changes.Operand2, deletions.Operand2, insertions.Operand2);
                }
            }
            else if (deletions != null)
                return null;
            return binaryExpression;
        }
        public virtual Block VisitBlock(Block block, Block changes, Block deletions, Block insertions)
        {
            this.UpdateSourceContext(block, changes);
            if (block == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    block.Checked = changes.Checked;
                    block.Statements = this.VisitStatementList(block.Statements, changes.Statements, deletions.Statements, insertions.Statements);
                    block.SuppressCheck = changes.SuppressCheck;
                }
            }
            else if (deletions != null)
                return null;
            return block;
        }
        public virtual Expression VisitBlockExpression(BlockExpression blockExpression, BlockExpression changes, BlockExpression deletions, BlockExpression insertions)
        {
            this.UpdateSourceContext(blockExpression, changes);
            if (blockExpression == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    blockExpression.Block = this.VisitBlock(blockExpression.Block, changes.Block, deletions.Block, insertions.Block);
                }
            }
            else if (deletions != null)
                return null;
            return blockExpression;
        }
        public virtual BlockList VisitBlockList(BlockList blockList, BlockList changes, BlockList deletions, BlockList insertions)
        {
            if (changes == null || deletions == null || insertions == null) return blockList;
            int n = blockList == null ? 0 : blockList.Count;
            if (n > changes.Count) { Debug.Assert(false); n = changes.Count; }
            if (n > deletions.Count) { Debug.Assert(false); n = deletions.Count; }
            if (n > insertions.Count) { Debug.Assert(false); n = insertions.Count; }
            if (blockList != null)
                for (int i = 0; i < n; i++)
                    blockList[i] = this.VisitBlock(blockList[i], changes[i], deletions[i], insertions[i]);
            BlockList result = new BlockList(insertions.Count - n);
            for (int i = n, m = insertions.Count; i < m; i++)
                result.Add(insertions[i]);
            return result;
        }
        public virtual Catch VisitCatch(Catch Catch, Catch changes, Catch deletions, Catch insertions)
        {
            this.UpdateSourceContext(Catch, changes);
            if (Catch == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    Catch.Block = this.VisitBlock(Catch.Block, changes.Block, deletions.Block, insertions.Block);
                    Catch.Type = this.VisitTypeReference(Catch.Type, changes.Type);
                    Catch.Variable = this.VisitExpression(Catch.Variable, changes.Variable, deletions.Variable, insertions.Variable);
                }
            }
            else if (deletions != null)
                return null;
            return Catch;
        }
        public virtual CatchList VisitCatchList(CatchList catchers, CatchList changes, CatchList deletions, CatchList insertions)
        {
            if (changes == null || deletions == null || insertions == null) return catchers;
            int n = catchers == null ? 0 : catchers.Count;
            if (n > changes.Count) { Debug.Assert(false); n = changes.Count; }
            if (n > deletions.Count) { Debug.Assert(false); n = deletions.Count; }
            if (n > insertions.Count) { Debug.Assert(false); n = insertions.Count; }
            if (catchers != null)
                for (int i = 0; i < n; i++)
                    catchers[i] = this.VisitCatch(catchers[i], changes[i], deletions[i], insertions[i]);
            CatchList result = new CatchList(insertions.Count - n);
            for (int i = n, m = insertions.Count; i < m; i++)
                result.Add(insertions[i]);
            return result;
        }
        public virtual Class VisitClass(Class Class, Class changes, Class deletions, Class insertions)
        {
            TypeNode result = this.VisitTypeNode(Class, changes, deletions, insertions);
            Debug.Assert(result is Class);
            return result as Class;
        }
        public virtual Expression VisitCoerceTuple(CoerceTuple coerceTuple, CoerceTuple changes, CoerceTuple deletions, CoerceTuple insertions)
        {
            this.UpdateSourceContext(coerceTuple, changes);
            if (coerceTuple == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    coerceTuple.Fields = this.VisitFieldList(coerceTuple.Fields, changes.Fields, deletions.Fields, insertions.Fields);
                    coerceTuple.OriginalTuple = this.VisitExpression(coerceTuple.OriginalTuple, changes.OriginalTuple, deletions.OriginalTuple, insertions.OriginalTuple);
                }
            }
            else if (deletions != null)
                return null;
            return coerceTuple;
        }
        public virtual CollectionEnumerator VisitCollectionEnumerator(CollectionEnumerator ce, CollectionEnumerator changes, CollectionEnumerator deletions, CollectionEnumerator insertions)
        {
            this.UpdateSourceContext(ce, changes);
            if (ce == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    ce.Collection = this.VisitExpression(ce.Collection, changes.Collection, deletions.Collection, insertions.Collection);
                    //REVIEW: update method bindings?
                }
            }
            else if (deletions != null)
                return null;
            return ce;
        }
        public virtual Compilation VisitCompilation(Compilation compilation, Compilation changes, Compilation deletions, Compilation insertions)
        {
            this.UpdateSourceContext(compilation, changes);
            if (compilation == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    compilation.CompilerParameters = changes.CompilerParameters;
                    compilation.CompilationUnits = this.VisitCompilationUnitList(compilation.CompilationUnits, changes.CompilationUnits, deletions.CompilationUnits, insertions.CompilationUnits);
                }
            }
            else if (deletions != null)
                return null;
            return compilation;
        }
        public virtual CompilationUnit VisitCompilationUnit(CompilationUnit cUnit, CompilationUnit changes, CompilationUnit deletions, CompilationUnit insertions)
        {
            this.UpdateSourceContext(cUnit, changes);
            if (cUnit == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    cUnit.Nodes = this.VisitNodeList(cUnit.Nodes, changes.Nodes, deletions.Nodes, insertions.Nodes);
                    cUnit.PreprocessorDefinedSymbols = changes.PreprocessorDefinedSymbols;
                }
            }
            else if (deletions != null)
                return null;
            return cUnit;
        }
        public virtual CompilationUnitList VisitCompilationUnitList(CompilationUnitList compUnits, CompilationUnitList changes, CompilationUnitList deletions, CompilationUnitList insertions)
        {
            if (changes == null || deletions == null || insertions == null) return compUnits;
            int n = compUnits == null ? 0 : compUnits.Count;
            if (n > changes.Count) { Debug.Assert(false); n = changes.Count; }
            if (n > deletions.Count) { Debug.Assert(false); n = deletions.Count; }
            if (n > insertions.Count) { Debug.Assert(false); n = insertions.Count; }
            if (compUnits != null)
                for (int i = 0; i < n; i++)
                    compUnits[i] = this.VisitCompilationUnit(compUnits[i], changes[i], deletions[i], insertions[i]);
            CompilationUnitList result = new CompilationUnitList(insertions.Count - n);
            for (int i = n, m = insertions.Count; i < m; i++)
                result.Add(insertions[i]);
            return result;
        }
        public virtual CompilationUnitSnippet VisitCompilationUnitSnippet(CompilationUnitSnippet snippet, CompilationUnitSnippet changes, CompilationUnitSnippet deletions, CompilationUnitSnippet insertions)
        {
            CompilationUnit cu = this.VisitCompilationUnit(snippet, changes, deletions, insertions);
            Debug.Assert(cu is CompilationUnitSnippet);
            return cu as CompilationUnitSnippet;
        }
        public virtual Node VisitComposition(Composition comp, Composition changes, Composition deletions, Composition insertions)
        {
            this.UpdateSourceContext(comp, changes);
            if (comp == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    comp.Expression = this.VisitExpression(comp.Expression, changes.Expression, deletions.Expression, insertions.Expression);
                }
            }
            else if (deletions != null)
                return null;
            return comp;
        }
        public virtual Expression VisitConstruct(Construct cons, Construct changes, Construct deletions, Construct insertions)
        {
            this.UpdateSourceContext(cons, changes);
            if (cons == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    cons.Constructor = this.VisitExpression(cons.Constructor, changes.Constructor, deletions.Constructor, insertions.Constructor);
                    cons.Operands = this.VisitExpressionList(cons.Operands, changes.Operands, deletions.Operands, insertions.Operands);
                    cons.Owner = this.VisitExpression(cons.Owner, changes.Owner, deletions.Owner, insertions.Owner);
                }
            }
            else if (deletions != null)
                return null;
            return cons;
        }
        public virtual Expression VisitConstructArray(ConstructArray consArr, ConstructArray changes, ConstructArray deletions, ConstructArray insertions)
        {
            this.UpdateSourceContext(consArr, changes);
            if (consArr == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    consArr.ElementType = this.VisitTypeReference(consArr.ElementType, changes.ElementType);
                    consArr.Initializers = this.VisitExpressionList(consArr.Initializers, changes.Initializers, deletions.Initializers, insertions.Initializers);
                    consArr.Operands = this.VisitExpressionList(consArr.Operands, changes.Operands, deletions.Operands, insertions.Operands);
                    consArr.Rank = changes.Rank;
                    consArr.Owner = this.VisitExpression(consArr.Owner, changes.Owner, deletions.Owner, insertions.Owner);
                }
            }
            else if (deletions != null)
                return null;
            return consArr;
        }
        public virtual Expression VisitConstructDelegate(ConstructDelegate consDelegate, ConstructDelegate changes, ConstructDelegate deletions, ConstructDelegate insertions)
        {
            this.UpdateSourceContext(consDelegate, changes);
            if (consDelegate == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    consDelegate.DelegateType = this.VisitTypeReference(consDelegate.DelegateType, changes.DelegateType);
                    consDelegate.MethodName = changes.MethodName;
                    consDelegate.TargetObject = this.VisitExpression(consDelegate.TargetObject, changes.TargetObject, deletions.TargetObject, insertions.TargetObject);
                }
            }
            else if (deletions != null)
                return null;
            return consDelegate;
        }
        public virtual Expression VisitConstructFlexArray(ConstructFlexArray consArr, ConstructFlexArray changes, ConstructFlexArray deletions, ConstructFlexArray insertions)
        {
            this.UpdateSourceContext(consArr, changes);
            if (consArr == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    consArr.ElementType = this.VisitTypeReference(consArr.ElementType, changes.ElementType);
                    consArr.Initializers = this.VisitExpressionList(consArr.Initializers, changes.Initializers, deletions.Initializers, insertions.Initializers);
                    consArr.Operands = this.VisitExpressionList(consArr.Operands, changes.Operands, deletions.Operands, insertions.Operands);
                }
            }
            else if (deletions != null)
                return null;
            return consArr;
        }
        public virtual Expression VisitConstructIterator(ConstructIterator consIterator, ConstructIterator changes, ConstructIterator deletions, ConstructIterator insertions)
        {
            this.UpdateSourceContext(consIterator, changes);
            if (consIterator == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    consIterator.Body = this.VisitBlock(consIterator.Body, changes.Body, deletions.Body, insertions.Body);
                    consIterator.ElementType = this.VisitTypeReference(consIterator.ElementType, changes.ElementType);
                    consIterator.State = this.VisitClass(consIterator.State, changes.State, deletions.State, insertions.State);
                }
            }
            else if (deletions != null)
                return null;
            return consIterator;
        }
        public virtual Expression VisitConstructTuple(ConstructTuple consTuple, ConstructTuple changes, ConstructTuple deletions, ConstructTuple insertions)
        {
            this.UpdateSourceContext(consTuple, changes);
            if (consTuple == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    consTuple.Fields = this.VisitFieldList(consTuple.Fields, changes.Fields, deletions.Fields, insertions.Fields);
                }
            }
            else if (deletions != null)
                return null;
            return consTuple;
        }
#if ExtendedRuntime    
    public virtual TypeNode VisitConstrainedType(ConstrainedType cType, ConstrainedType changes, ConstrainedType deletions, ConstrainedType insertions){
      this.UpdateSourceContext(cType, changes);
      if (cType == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
          cType.Constraint = this.VisitExpression(cType.Constraint, changes.Constraint, deletions.Constraint, insertions.Constraint);
          cType.UnderlyingType = this.VisitTypeReference(cType.UnderlyingType, changes.UnderlyingType);
        }
      }else if (deletions != null)
        return null;
      return cType;
    }
#endif
        public virtual Statement VisitContinue(Continue Continue, Continue changes, Continue deletions, Continue insertions)
        {
            this.UpdateSourceContext(Continue, changes);
            if (Continue == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    Continue.Level = changes.Level;
                }
            }
            else if (deletions != null)
                return null;
            return Continue;
        }
        public virtual Expression VisitCurrentClosure(CurrentClosure currentClosure, CurrentClosure changes, CurrentClosure deletions, CurrentClosure insertions)
        {
            this.UpdateSourceContext(currentClosure, changes);
            if (currentClosure == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return currentClosure;
        }
        public virtual DelegateNode VisitDelegateNode(DelegateNode delegateNode, DelegateNode changes, DelegateNode deletions, DelegateNode insertions)
        {
            this.UpdateSourceContext(delegateNode, changes);
            if (delegateNode == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    delegateNode.Attributes = this.VisitAttributeList(delegateNode.Attributes, changes.Attributes, deletions.Attributes, insertions.Attributes);
#if !NoXml
                    delegateNode.Documentation = changes.Documentation;
#endif
                    delegateNode.Flags = changes.Flags;
                    delegateNode.Parameters = this.VisitParameterList(delegateNode.Parameters, changes.Parameters, deletions.Parameters, insertions.Parameters);
                    delegateNode.ReturnType = this.VisitTypeReference(delegateNode.ReturnType, changes.ReturnType);
                    delegateNode.SecurityAttributes = this.VisitSecurityAttributeList(delegateNode.SecurityAttributes, changes.SecurityAttributes, deletions.SecurityAttributes, insertions.SecurityAttributes);
                    delegateNode.TemplateParameters = this.VisitTypeReferenceList(delegateNode.TemplateParameters, changes.TemplateParameters, deletions.TemplateParameters, insertions.TemplateParameters);
                }
            }
            else if (deletions != null)
                return null;
            return delegateNode;
        }
        public virtual Statement VisitDoWhile(DoWhile doWhile, DoWhile changes, DoWhile deletions, DoWhile insertions)
        {
            this.UpdateSourceContext(doWhile, changes);
            if (doWhile == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    doWhile.Body = this.VisitBlock(doWhile.Body, changes.Body, deletions.Body, insertions.Body);
                    doWhile.Condition = this.VisitExpression(doWhile.Condition, changes.Condition, deletions.Condition, insertions.Condition);
                }
            }
            else if (deletions != null)
                return null;
            return doWhile;
        }
        public virtual Statement VisitEndFilter(EndFilter endFilter, EndFilter changes, EndFilter deletions, EndFilter insertions)
        {
            this.UpdateSourceContext(endFilter, changes);
            if (endFilter == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    endFilter.Value = this.VisitExpression(endFilter.Value, changes.Value, deletions.Value, insertions.Value);
                }
            }
            else if (deletions != null)
                return null;
            return endFilter;
        }
        public virtual Statement VisitEndFinally(EndFinally endFinally, EndFinally changes, EndFinally deletions, EndFinally insertions)
        {
            this.UpdateSourceContext(endFinally, changes);
            if (endFinally == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return endFinally;
        }
        public virtual EnumNode VisitEnumNode(EnumNode enumNode, EnumNode changes, EnumNode deletions, EnumNode insertions)
        {
            if (enumNode == null) return changes;
            TypeNode result = this.VisitTypeNode(enumNode, changes, deletions, insertions);
            Debug.Assert(result is EnumNode);
            if (result == enumNode)
            {
                Debug.Assert(changes != null);
                if (changes != null)
                    enumNode.UnderlyingType = this.VisitTypeReference(enumNode.UnderlyingType, changes.UnderlyingType);
                return enumNode;
            }
            return result as EnumNode;
        }
        public virtual Event VisitEvent(Event evnt, Event changes, Event deletions, Event insertions)
        {
            this.UpdateSourceContext(evnt, changes);
            if (evnt == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    evnt.Attributes = this.VisitAttributeList(evnt.Attributes, changes.Attributes, deletions.Attributes, insertions.Attributes);
#if !NoXml
                    evnt.Documentation = changes.Documentation;
#endif
                    evnt.Flags = changes.Flags;
                    evnt.HandlerAdder = this.VisitMethodReference(evnt.HandlerAdder, changes.HandlerAdder);
                    evnt.HandlerCaller = this.VisitMethodReference(evnt.HandlerCaller, changes.HandlerCaller);
                    evnt.HandlerFlags = changes.HandlerFlags;
                    evnt.HandlerRemover = this.VisitMethodReference(evnt.HandlerRemover, changes.HandlerRemover);
                    evnt.HandlerType = this.VisitTypeReference(evnt.HandlerType, changes.HandlerType);
                    evnt.InitialHandler = this.VisitExpression(evnt.InitialHandler, changes.InitialHandler, deletions.InitialHandler, insertions.InitialHandler);
                    evnt.Name = changes.Name;
                    evnt.OtherMethods = this.VisitMethodReferenceList(evnt.OtherMethods, changes.OtherMethods);
                    evnt.OverridesBaseClassMember = changes.OverridesBaseClassMember;
                }
            }
            else if (deletions != null)
                return null;
            return evnt;
        }
        public virtual Statement VisitExit(Exit exit, Exit changes, Exit deletions, Exit insertions)
        {
            this.UpdateSourceContext(exit, changes);
            if (exit == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    exit.Level = changes.Level;
                }
            }
            else if (deletions != null)
                return null;
            return exit;
        }
        public virtual Expression VisitExpression(Expression expression, Expression changes, Expression deletions, Expression insertions)
        {
            return this.Visit(expression, changes, deletions, insertions) as Expression;
        }
        public virtual ExpressionList VisitExpressionList(ExpressionList expressions, ExpressionList changes, ExpressionList deletions, ExpressionList insertions)
        {
            if (changes == null || deletions == null || insertions == null) return expressions;
            int n = expressions == null ? 0 : expressions.Count;
            if (n > changes.Count) { Debug.Assert(false); n = changes.Count; }
            if (n > deletions.Count) { Debug.Assert(false); n = deletions.Count; }
            if (n > insertions.Count) { Debug.Assert(false); n = insertions.Count; }
            if (expressions != null)
                for (int i = 0; i < n; i++)
                    expressions[i] = this.VisitExpression(expressions[i], changes[i], deletions[i], insertions[i]);
            ExpressionList result = new ExpressionList(insertions.Count - n);
            for (int i = n, m = insertions.Count; i < m; i++)
                result.Add(insertions[i]);
            return result;
        }
        public virtual Expression VisitExpressionSnippet(ExpressionSnippet snippet, ExpressionSnippet changes, ExpressionSnippet deletions, ExpressionSnippet insertions)
        {
            this.UpdateSourceContext(snippet, changes);
            if (snippet == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return snippet;
        }
        public virtual Statement VisitExpressionStatement(ExpressionStatement statement, ExpressionStatement changes, ExpressionStatement deletions, ExpressionStatement insertions)
        {
            this.UpdateSourceContext(statement, changes);
            if (statement == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    statement.Expression = this.VisitExpression(statement.Expression, changes.Expression, deletions.Expression, insertions.Expression);
                }
            }
            else if (deletions != null)
                return null;
            return statement;
        }
        public virtual FaultHandler VisitFaultHandler(FaultHandler faultHandler, FaultHandler changes, FaultHandler deletions, FaultHandler insertions)
        {
            this.UpdateSourceContext(faultHandler, changes);
            if (faultHandler == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    faultHandler.Block = this.VisitBlock(faultHandler.Block, changes.Block, deletions.Block, insertions.Block);
                }
            }
            else if (deletions != null)
                return null;
            return faultHandler;
        }
        public virtual FaultHandlerList VisitFaultHandlerList(FaultHandlerList faultHandlers, FaultHandlerList changes, FaultHandlerList deletions, FaultHandlerList insertions)
        {
            if (changes == null || deletions == null || insertions == null) return faultHandlers;
            int n = faultHandlers == null ? 0 : faultHandlers.Count;
            if (n > changes.Count) { Debug.Assert(false); n = changes.Count; }
            if (n > deletions.Count) { Debug.Assert(false); n = deletions.Count; }
            if (n > insertions.Count) { Debug.Assert(false); n = insertions.Count; }
            if (faultHandlers != null)
                for (int i = 0; i < n; i++)
                    faultHandlers[i] = this.VisitFaultHandler(faultHandlers[i], changes[i], deletions[i], insertions[i]);
            FaultHandlerList result = new FaultHandlerList(insertions.Count - n);
            for (int i = n, m = insertions.Count; i < m; i++)
                result.Add(insertions[i]);
            return result;
        }
        public virtual Field VisitField(Field field, Field changes, Field deletions, Field insertions)
        {
            this.UpdateSourceContext(field, changes);
            if (field == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    field.Attributes = this.VisitAttributeList(field.Attributes, changes.Attributes, deletions.Attributes, insertions.Attributes);
                    field.DefaultValue = this.VisitLiteral(field.DefaultValue, changes.DefaultValue, deletions.DefaultValue, insertions.DefaultValue);
#if !NoXml
                    field.Documentation = changes.Documentation;
#endif
                    field.Flags = changes.Flags;
                    field.HidesBaseClassMember = changes.HidesBaseClassMember;
                    field.ImplementedInterfaces = this.VisitInterfaceReferenceList(field.ImplementedInterfaces, changes.ImplementedInterfaces, deletions.ImplementedInterfaces, insertions.ImplementedInterfaces);
                    field.InitialData = changes.InitialData;
                    field.Initializer = changes.Initializer;
                    field.MarshallingInformation = changes.MarshallingInformation;
                    field.Name = changes.Name;
                    field.Offset = changes.Offset;
                    field.OverridesBaseClassMember = changes.OverridesBaseClassMember;
                    field.Section = changes.Section;
                    field.Type = this.VisitTypeReference(field.Type, changes.Type);
                }
            }
            else if (deletions != null)
                return null;
            return field;
        }
        public virtual Block VisitFieldInitializerBlock(FieldInitializerBlock block, FieldInitializerBlock changes, FieldInitializerBlock deletions, FieldInitializerBlock insertions)
        {
            this.UpdateSourceContext(block, changes);
            if (block == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    block.IsStatic = changes.IsStatic;
                }
            }
            else if (deletions != null)
                return null;
            return block;
        }
        public virtual FieldList VisitFieldList(FieldList fields, FieldList changes, FieldList deletions, FieldList insertions)
        {
            if (changes == null || deletions == null || insertions == null) return fields;
            int n = fields == null ? 0 : fields.Count;
            if (n > changes.Count) { Debug.Assert(false); n = changes.Count; }
            if (n > deletions.Count) { Debug.Assert(false); n = deletions.Count; }
            if (n > insertions.Count) { Debug.Assert(false); n = insertions.Count; }
            if (fields != null)
                for (int i = 0; i < n; i++)
                    fields[i] = this.VisitField(fields[i], changes[i], deletions[i], insertions[i]);
            FieldList result = new FieldList(insertions.Count - n);
            for (int i = n, m = insertions.Count; i < m; i++)
                result.Add(insertions[i]);
            return result;
        }
        public virtual Statement VisitFilter(Filter filter, Filter changes, Filter deletions, Filter insertions)
        {
            this.UpdateSourceContext(filter, changes);
            if (filter == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                    filter.Block = this.VisitBlock(filter.Block, changes.Block, deletions.Block, insertions.Block);
                }
            }
            else if (deletions != null)
                return null;
            return filter;
        }
        public virtual FilterList VisitFilterList(FilterList filters, FilterList changes, FilterList deletions, FilterList insertions)
        {
            if (filters == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return filters;
        }
        public virtual Statement VisitFinally(Finally Finally, Finally changes, Finally deletions, Finally insertions)
        {
            this.UpdateSourceContext(Finally, changes);
            if (Finally == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return Finally;
        }
        public virtual Statement VisitFixed(Fixed Fixed, Fixed changes, Fixed deletions, Fixed insertions)
        {
            this.UpdateSourceContext(Fixed, changes);
            if (Fixed == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return Fixed;
        }
        public virtual Statement VisitFor(For For, For changes, For deletions, For insertions)
        {
            this.UpdateSourceContext(For, changes);
            if (For == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return For;
        }
        public virtual Statement VisitForEach(ForEach forEach, ForEach changes, ForEach deletions, ForEach insertions)
        {
            this.UpdateSourceContext(forEach, changes);
            if (forEach == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return forEach;
        }
        public virtual Statement VisitFunctionDeclaration(FunctionDeclaration functionDeclaration, FunctionDeclaration changes, FunctionDeclaration deletions, FunctionDeclaration insertions)
        {
            this.UpdateSourceContext(functionDeclaration, changes);
            if (functionDeclaration == null) return changes;
            return functionDeclaration;
        }
        public virtual Expression VisitTemplateInstance(TemplateInstance instance, TemplateInstance changes, TemplateInstance deletions, TemplateInstance insertions)
        {
            this.UpdateSourceContext(instance, changes);
            if (instance == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return instance;
        }
        public virtual Expression VisitStackAlloc(StackAlloc alloc, StackAlloc changes, StackAlloc deletions, StackAlloc insertions)
        {
            this.UpdateSourceContext(alloc, changes);
            if (alloc == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return alloc;
        }
        public virtual Statement VisitGoto(Goto Goto, Goto changes, Goto deletions, Goto insertions)
        {
            this.UpdateSourceContext(Goto, changes);
            if (Goto == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return Goto;
        }
        public virtual Statement VisitGotoCase(GotoCase gotoCase, GotoCase changes, GotoCase deletions, GotoCase insertions)
        {
            this.UpdateSourceContext(gotoCase, changes);
            if (gotoCase == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return gotoCase;
        }
        public virtual Expression VisitIdentifier(Identifier identifier, Identifier changes, Identifier deletions, Identifier insertions)
        {
            this.UpdateSourceContext(identifier, changes);
            if (identifier == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return identifier;
        }
        public virtual Statement VisitIf(If If, If changes, If deletions, If insertions)
        {
            this.UpdateSourceContext(If, changes);
            if (If == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return If;
        }
        public virtual Expression VisitImplicitThis(ImplicitThis implicitThis, ImplicitThis changes, ImplicitThis deletions, ImplicitThis insertions)
        {
            this.UpdateSourceContext(implicitThis, changes);
            if (implicitThis == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return implicitThis;
        }
        public virtual Expression VisitIndexer(Indexer indexer, Indexer changes, Indexer deletions, Indexer insertions)
        {
            this.UpdateSourceContext(indexer, changes);
            if (indexer == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return indexer;
        }
        public virtual Interface VisitInterface(Interface Interface, Interface changes, Interface deletions, Interface insertions)
        {
            return (Interface)this.VisitTypeNode(Interface, changes, deletions, insertions);
        }
        public virtual InterfaceList VisitInterfaceReferenceList(InterfaceList interfaces, InterfaceList changes, InterfaceList deletions, InterfaceList insertions)
        {
            if (interfaces == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return interfaces;
        }
        public virtual Interface VisitInterfaceReference(Interface Interface, Interface changes)
        {
            return (Interface)this.VisitTypeReference(Interface, changes);
        }
        public virtual InstanceInitializer VisitInstanceInitializer(InstanceInitializer cons, InstanceInitializer changes, InstanceInitializer deletions, InstanceInitializer insertions)
        {
            return (InstanceInitializer)this.VisitMethod(cons, changes, deletions, insertions);
        }
        public virtual Statement VisitLabeledStatement(LabeledStatement lStatement, LabeledStatement changes, LabeledStatement deletions, LabeledStatement insertions)
        {
            this.UpdateSourceContext(lStatement, changes);
            if (lStatement == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return lStatement;
        }
        public virtual Literal VisitLiteral(Literal literal, Literal changes, Literal deletions, Literal insertions)
        {
            this.UpdateSourceContext(literal, changes);
            if (literal == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return literal;
        }
        public virtual Expression VisitLocal(Local local, Local changes, Local deletions, Local insertions)
        {
            this.UpdateSourceContext(local, changes);
            if (local == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return local;
        }
        public virtual LocalDeclaration VisitLocalDeclaration(LocalDeclaration localDeclaration, LocalDeclaration changes, LocalDeclaration deletions, LocalDeclaration insertions)
        {
            this.UpdateSourceContext(localDeclaration, changes);
            if (localDeclaration == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return localDeclaration;
        }
        public virtual LocalDeclarationList VisitLocalDeclarationList(LocalDeclarationList localDeclarations, LocalDeclarationList changes, LocalDeclarationList deletions, LocalDeclarationList insertions)
        {
            if (localDeclarations == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return localDeclarations;
        }
        public virtual Statement VisitLocalDeclarationsStatement(LocalDeclarationsStatement localDeclarations, LocalDeclarationsStatement changes, LocalDeclarationsStatement deletions, LocalDeclarationsStatement insertions)
        {
            this.UpdateSourceContext(localDeclarations, changes);
            if (localDeclarations == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return localDeclarations;
        }
        public virtual Statement VisitLock(Lock Lock, Lock changes, Lock deletions, Lock insertions)
        {
            this.UpdateSourceContext(Lock, changes);
            if (Lock == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return Lock;
        }
        public virtual Expression VisitLRExpression(LRExpression expr, LRExpression changes, LRExpression deletions, LRExpression insertions)
        {
            this.UpdateSourceContext(expr, changes);
            if (expr == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return expr;
        }
        public virtual Expression VisitMemberBinding(MemberBinding memberBinding, MemberBinding changes, MemberBinding deletions, MemberBinding insertions)
        {
            this.UpdateSourceContext(memberBinding, changes);
            if (memberBinding == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return memberBinding;
        }
        public virtual MemberList VisitMemberList(MemberList members, MemberList changes, MemberList deletions, MemberList insertions)
        {
            if (members == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return members;
        }
        public virtual Method VisitMethod(Method method, Method changes, Method deletions, Method insertions)
        {
            this.UpdateSourceContext(method, changes);
            if (method == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return method;
        }
        public virtual Expression VisitMethodCall(MethodCall call, MethodCall changes, MethodCall deletions, MethodCall insertions)
        {
            this.UpdateSourceContext(call, changes);
            if (call == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return call;
        }
        public virtual Method VisitMethodReference(Method method, Method changes)
        {
            return method;
        }
        public virtual MethodList VisitMethodReferenceList(MethodList methodList, MethodList changesList)
        {
            return methodList;
        }
        public virtual Module VisitModule(Module module, Module changes, Module deletions, Module insertions)
        {
            this.UpdateSourceContext(module, changes);
            if (module == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return module;
        }
        public virtual ModuleReference VisitModuleReference(ModuleReference moduleReference, ModuleReference changes, ModuleReference deletions, ModuleReference insertions)
        {
            this.UpdateSourceContext(moduleReference, changes);
            if (moduleReference == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return moduleReference;
        }
        public virtual ModuleReferenceList VisitModuleReferenceList(ModuleReferenceList moduleReferences, ModuleReferenceList changes, ModuleReferenceList deletions, ModuleReferenceList insertions)
        {
            return moduleReferences;
        }
        public virtual Expression VisitNameBinding(NameBinding nameBinding, NameBinding changes, NameBinding deletions, NameBinding insertions)
        {
            this.UpdateSourceContext(nameBinding, changes);
            if (nameBinding == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return nameBinding;
        }
        public virtual Expression VisitNamedArgument(NamedArgument namedArgument, NamedArgument changes, NamedArgument deletions, NamedArgument insertions)
        {
            this.UpdateSourceContext(namedArgument, changes);
            if (namedArgument == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return namedArgument;
        }
        public virtual Namespace VisitNamespace(Namespace nspace, Namespace changes, Namespace deletions, Namespace insertions)
        {
            this.UpdateSourceContext(nspace, changes);
            if (nspace == null) return changes;
            return nspace;
        }
        public virtual NamespaceList VisitNamespaceList(NamespaceList namespaces, NamespaceList changes, NamespaceList deletions, NamespaceList insertions)
        {
            if (namespaces == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return namespaces;
        }
        public virtual NodeList VisitNodeList(NodeList nodeList, NodeList changes, NodeList deletions, NodeList insertions)
        {
            if (changes == null || deletions == null || insertions == null) return nodeList;
            int n = nodeList == null ? 0 : nodeList.Count;
            if (n > changes.Count) { Debug.Assert(false); n = changes.Count; }
            if (n > deletions.Count) { Debug.Assert(false); n = deletions.Count; }
            if (n > insertions.Count) { Debug.Assert(false); n = insertions.Count; }
            if (nodeList != null)
                for (int i = 0; i < n; i++)
                    nodeList[i] = this.Visit(nodeList[i], changes[i], deletions[i], insertions[i]);
            NodeList result = new NodeList(insertions.Count - n);
            for (int i = n, m = insertions.Count; i < m; i++)
                result.Add(insertions[i]);
            return result;
        }
        public virtual Expression VisitParameter(Parameter parameter, Parameter changes, Parameter deletions, Parameter insertions)
        {
            this.UpdateSourceContext(parameter, changes);
            if (parameter == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return parameter;
        }
        public virtual ParameterList VisitParameterList(ParameterList parameterList, ParameterList changes, ParameterList deletions, ParameterList insertions)
        {
            if (parameterList == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return parameterList;
        }
        public virtual Expression VisitPrefixExpression(PrefixExpression pExpr, PrefixExpression changes, PrefixExpression deletions, PrefixExpression insertions)
        {
            this.UpdateSourceContext(pExpr, changes);
            if (pExpr == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return pExpr;
        }
        public virtual Expression VisitPostfixExpression(PostfixExpression pExpr, PostfixExpression changes, PostfixExpression deletions, PostfixExpression insertions)
        {
            this.UpdateSourceContext(pExpr, changes);
            if (pExpr == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return pExpr;
        }
        public virtual Property VisitProperty(Property property, Property changes, Property deletions, Property insertions)
        {
            this.UpdateSourceContext(property, changes);
            if (property == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return property;
        }
        public virtual Expression VisitQualifiedIdentifier(QualifiedIdentifier qualifiedIdentifier, QualifiedIdentifier changes, QualifiedIdentifier deletions, QualifiedIdentifier insertions)
        {
            this.UpdateSourceContext(qualifiedIdentifier, changes);
            if (qualifiedIdentifier == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return qualifiedIdentifier;
        }
        public virtual Statement VisitRepeat(Repeat repeat, Repeat changes, Repeat deletions, Repeat insertions)
        {
            this.UpdateSourceContext(repeat, changes);
            if (repeat == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return repeat;
        }
        public virtual Statement VisitResourceUse(ResourceUse resourceUse, ResourceUse changes, ResourceUse deletions, ResourceUse insertions)
        {
            this.UpdateSourceContext(resourceUse, changes);
            if (resourceUse == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return resourceUse;
        }
        public virtual Statement VisitReturn(Return Return, Return changes, Return deletions, Return insertions)
        {
            this.UpdateSourceContext(Return, changes);
            if (Return == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return Return;
        }
        public virtual SecurityAttribute VisitSecurityAttribute(SecurityAttribute attribute, SecurityAttribute changes, SecurityAttribute deletions, SecurityAttribute insertions)
        {
            this.UpdateSourceContext(attribute, changes);
            if (attribute == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return attribute;
        }
        public virtual SecurityAttributeList VisitSecurityAttributeList(SecurityAttributeList attributes, SecurityAttributeList changes, SecurityAttributeList deletions, SecurityAttributeList insertions)
        {
            if (attributes == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return attributes;
        }
        public virtual Expression VisitSetterValue(SetterValue value, SetterValue changes, SetterValue deletions, SetterValue insertions)
        {
            this.UpdateSourceContext(value, changes);
            if (value == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return value;
        }
        public virtual StatementList VisitStatementList(StatementList statements, StatementList changes, StatementList deletions, StatementList insertions)
        {
            if (statements == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return statements;
        }
        public virtual StatementSnippet VisitStatementSnippet(StatementSnippet snippet, StatementSnippet changes, StatementSnippet deletions, StatementSnippet insertions)
        {
            this.UpdateSourceContext(snippet, changes);
            if (snippet == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return snippet;
        }
        public virtual StaticInitializer VisitStaticInitializer(StaticInitializer cons, StaticInitializer changes, StaticInitializer deletions, StaticInitializer insertions)
        {
            return (StaticInitializer)this.VisitMethod(cons, changes, deletions, insertions);
        }
        public virtual Struct VisitStruct(Struct Struct, Struct changes, Struct deletions, Struct insertions)
        {
            return (Struct)this.VisitTypeNode(Struct, changes, deletions, insertions);
        }
        public virtual Statement VisitSwitch(Switch Switch, Switch changes, Switch deletions, Switch insertions)
        {
            this.UpdateSourceContext(Switch, changes);
            if (Switch == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return Switch;
        }
        public virtual SwitchCase VisitSwitchCase(SwitchCase switchCase, SwitchCase changes, SwitchCase deletions, SwitchCase insertions)
        {
            this.UpdateSourceContext(switchCase, changes);
            if (switchCase == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return switchCase;
        }
        public virtual SwitchCaseList VisitSwitchCaseList(SwitchCaseList switchCases, SwitchCaseList changes, SwitchCaseList deletions, SwitchCaseList insertions)
        {
            if (switchCases == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return switchCases;
        }
        public virtual Statement VisitSwitchInstruction(SwitchInstruction switchInstruction, SwitchInstruction changes, SwitchInstruction deletions, SwitchInstruction insertions)
        {
            this.UpdateSourceContext(switchInstruction, changes);
            if (switchInstruction == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return switchInstruction;
        }
        public virtual Statement VisitTypeswitch(Typeswitch Typeswitch, Typeswitch changes, Typeswitch deletions, Typeswitch insertions)
        {
            this.UpdateSourceContext(Typeswitch, changes);
            if (Typeswitch == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return Typeswitch;
        }
        public virtual TypeswitchCase VisitTypeswitchCase(TypeswitchCase typeswitchCase, TypeswitchCase changes, TypeswitchCase deletions, TypeswitchCase insertions)
        {
            this.UpdateSourceContext(typeswitchCase, changes);
            if (typeswitchCase == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return typeswitchCase;
        }
        public virtual TypeswitchCaseList VisitTypeswitchCaseList(TypeswitchCaseList typeswitchCases, TypeswitchCaseList changes, TypeswitchCaseList deletions, TypeswitchCaseList insertions)
        {
            if (typeswitchCases == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return typeswitchCases;
        }
        public virtual Expression VisitTargetExpression(Expression expression, Expression changes, Expression deletions, Expression insertions)
        {
            return this.VisitExpression(expression, changes, deletions, insertions);
        }
        public virtual Expression VisitTernaryExpression(TernaryExpression expression, TernaryExpression changes, TernaryExpression deletions, TernaryExpression insertions)
        {
            this.UpdateSourceContext(expression, changes);
            if (expression == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return expression;
        }
        public virtual Expression VisitThis(This This, This changes, This deletions, This insertions)
        {
            this.UpdateSourceContext(This, changes);
            if (This == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return This;
        }
        public virtual Statement VisitThrow(Throw Throw, Throw changes, Throw deletions, Throw insertions)
        {
            this.UpdateSourceContext(Throw, changes);
            if (Throw == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return Throw;
        }
        public virtual Statement VisitTry(Try Try, Try changes, Try deletions, Try insertions)
        {
            this.UpdateSourceContext(Try, changes);
            if (Try == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return Try;
        }
#if ExtendedRuntime    
    public virtual TupleType VisitTupleType(TupleType tuple, TupleType changes, TupleType deletions, TupleType insertions){
      return (TupleType)this.VisitTypeNode(tuple, changes, deletions, insertions);
    }
    public virtual TypeAlias VisitTypeAlias(TypeAlias tAlias, TypeAlias changes, TypeAlias deletions, TypeAlias insertions){
      this.UpdateSourceContext(tAlias, changes);
      if (tAlias == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return tAlias;
    }
    public virtual TypeIntersection VisitTypeIntersection(TypeIntersection typeIntersection, TypeIntersection changes, TypeIntersection deletions, TypeIntersection insertions){
      return (TypeIntersection)this.VisitTypeNode(typeIntersection, changes, deletions, insertions);
    }
#endif
        public virtual TypeMemberSnippet VisitTypeMemberSnippet(TypeMemberSnippet snippet, TypeMemberSnippet changes, TypeMemberSnippet deletions, TypeMemberSnippet insertions)
        {
            this.UpdateSourceContext(snippet, changes);
            return snippet;
        }
        public virtual TypeModifier VisitTypeModifier(TypeModifier typeModifier, TypeModifier changes, TypeModifier deletions, TypeModifier insertions)
        {
            this.UpdateSourceContext(typeModifier, changes);
            if (typeModifier == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return typeModifier;
        }
        public virtual TypeNode VisitTypeNode(TypeNode typeNode, TypeNode changes, TypeNode deletions, TypeNode insertions)
        {
            this.UpdateSourceContext(typeNode, changes);
            if (typeNode == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return typeNode;
        }
        public virtual TypeNodeList VisitTypeNodeList(TypeNodeList types, TypeNodeList changes, TypeNodeList deletions, TypeNodeList insertions)
        {
            if (types == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return types;
        }
        public virtual TypeNode VisitTypeParameter(TypeNode typeParameter, TypeNode changes, TypeNode deletions, TypeNode insertions)
        {
            this.UpdateSourceContext(typeParameter, changes);
            if (typeParameter == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return typeParameter;
        }
        public virtual TypeNodeList VisitTypeParameterList(TypeNodeList typeParameters, TypeNodeList changes, TypeNodeList deletions, TypeNodeList insertions)
        {
            if (typeParameters == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return typeParameters;
        }
        public virtual TypeNode VisitTypeReference(TypeNode type, TypeNode changes)
        {
            return type;
        }
        public virtual TypeReference VisitTypeReference(TypeReference type, TypeReference changes, TypeReference deletions, TypeReference insertions)
        {
            return type;
        }
        public virtual TypeNodeList VisitTypeReferenceList(TypeNodeList typeReferences, TypeNodeList changes, TypeNodeList deletions, TypeNodeList insertions)
        {
            if (typeReferences == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return typeReferences;
        }
#if ExtendedRuntime    
    public virtual TypeUnion VisitTypeUnion(TypeUnion typeUnion, TypeUnion changes, TypeUnion deletions, TypeUnion insertions){
      return (TypeUnion)this.VisitTypeNode(typeUnion, changes, deletions, insertions);
    }
#endif
        public virtual Expression VisitUnaryExpression(UnaryExpression unaryExpression, UnaryExpression changes, UnaryExpression deletions, UnaryExpression insertions)
        {
            this.UpdateSourceContext(unaryExpression, changes);
            if (unaryExpression == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return unaryExpression;
        }
        public virtual Statement VisitVariableDeclaration(VariableDeclaration variableDeclaration, VariableDeclaration changes, VariableDeclaration deletions, VariableDeclaration insertions)
        {
            this.UpdateSourceContext(variableDeclaration, changes);
            if (variableDeclaration == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return variableDeclaration;
        }
        public virtual UsedNamespace VisitUsedNamespace(UsedNamespace usedNamespace, UsedNamespace changes, UsedNamespace deletions, UsedNamespace insertions)
        {
            this.UpdateSourceContext(usedNamespace, changes);
            if (usedNamespace == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return usedNamespace;
        }
        public virtual UsedNamespaceList VisitUsedNamespaceList(UsedNamespaceList usedNspaces, UsedNamespaceList changes, UsedNamespaceList deletions, UsedNamespaceList insertions)
        {
            if (usedNspaces == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return usedNspaces;
        }
        public virtual Statement VisitWhile(While While, While changes, While deletions, While insertions)
        {
            this.UpdateSourceContext(While, changes);
            if (While == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return While;
        }
        public virtual Statement VisitYield(Yield Yield, Yield changes, Yield deletions, Yield insertions)
        {
            this.UpdateSourceContext(Yield, changes);
            if (Yield == null) return changes;
            if (changes != null)
            {
                if (deletions == null || insertions == null)
                    Debug.Assert(false);
                else
                {
                }
            }
            else if (deletions != null)
                return null;
            return Yield;
        }
#if ExtendedRuntime
    // query nodes
    public virtual Node VisitQueryAggregate(QueryAggregate qa, QueryAggregate changes, QueryAggregate deletions, QueryAggregate insertions){
      this.UpdateSourceContext(qa, changes);
      if (qa == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return qa;
    }
    public virtual Node VisitQueryAlias(QueryAlias alias, QueryAlias changes, QueryAlias deletions, QueryAlias insertions){
      this.UpdateSourceContext(alias, changes);
      if (alias == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return alias;
    }
    public virtual Node VisitQueryAxis(QueryAxis axis, QueryAxis changes, QueryAxis deletions, QueryAxis insertions){
      this.UpdateSourceContext(axis, changes);
      if (axis == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return axis;
    }
    public virtual Node VisitQueryCommit(QueryCommit qc, QueryCommit changes, QueryCommit deletions, QueryCommit insertions){
      this.UpdateSourceContext(qc, changes);
      if (qc == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return qc;
    }
    public virtual Node VisitQueryContext(QueryContext context, QueryContext changes, QueryContext deletions, QueryContext insertions){
      this.UpdateSourceContext(context, changes);
      if (context == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return context;
    }
    public virtual Node VisitQueryDelete(QueryDelete delete, QueryDelete changes, QueryDelete deletions, QueryDelete insertions){
      this.UpdateSourceContext(delete, changes);
      if (delete == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return delete;
    }
    public virtual Node VisitQueryDifference(QueryDifference diff, QueryDifference changes, QueryDifference deletions, QueryDifference insertions){
      this.UpdateSourceContext(diff, changes);
      if (diff == null) return changes;
      return diff;
    }
    public virtual Node VisitQueryDistinct(QueryDistinct distinct, QueryDistinct changes, QueryDistinct deletions, QueryDistinct insertions){
      this.UpdateSourceContext(distinct, changes);
      if (distinct == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return distinct;
    }
    public virtual Node VisitQueryExists(QueryExists exists, QueryExists changes, QueryExists deletions, QueryExists insertions){
      this.UpdateSourceContext(exists, changes);
      if (exists == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return exists;
    }
    public virtual Node VisitQueryFilter(QueryFilter filter, QueryFilter changes, QueryFilter deletions, QueryFilter insertions){
      this.UpdateSourceContext(filter, changes);
      if (filter == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return filter;
    }
    public virtual Node VisitQueryGroupBy(QueryGroupBy groupby, QueryGroupBy changes, QueryGroupBy deletions, QueryGroupBy insertions){
      this.UpdateSourceContext(groupby, changes);
      if (groupby == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return groupby;
    }
    public virtual Statement VisitQueryGeneratedType(QueryGeneratedType qgt, QueryGeneratedType changes, QueryGeneratedType deletions, QueryGeneratedType insertions){
      this.UpdateSourceContext(qgt, changes);
      if (qgt == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return qgt;
    }
    public virtual Node VisitQueryInsert(QueryInsert insert, QueryInsert changes, QueryInsert deletions, QueryInsert insertions){
      this.UpdateSourceContext(insert, changes);
      if (insert == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return insert;
    }
    public virtual Node VisitQueryIntersection(QueryIntersection intersection, QueryIntersection changes, QueryIntersection deletions, QueryIntersection insertions){
      this.UpdateSourceContext(intersection, changes);
      if (intersection == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return intersection;
    }
    public virtual Node VisitQueryIterator(QueryIterator xiterator, QueryIterator changes, QueryIterator deletions, QueryIterator insertions){
      this.UpdateSourceContext(xiterator, changes);
      if (xiterator == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return xiterator;      
    }
    public virtual Node VisitQueryJoin(QueryJoin join, QueryJoin changes, QueryJoin deletions, QueryJoin insertions){
      this.UpdateSourceContext(join, changes);
      if (join == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return join;
    }
    public virtual Node VisitQueryLimit(QueryLimit limit, QueryLimit changes, QueryLimit deletions, QueryLimit insertions){
      this.UpdateSourceContext(limit, changes);
      if (limit == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return limit;
    }
    public virtual Node VisitQueryOrderBy(QueryOrderBy orderby, QueryOrderBy changes, QueryOrderBy deletions, QueryOrderBy insertions){
      this.UpdateSourceContext(orderby, changes);
      if (orderby == null) return changes;
      return orderby;
    }
    public virtual Node VisitQueryOrderItem(QueryOrderItem item, QueryOrderItem changes, QueryOrderItem deletions, QueryOrderItem insertions){
      this.UpdateSourceContext(item, changes);
      if (item == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return item;
    }
    public virtual Node VisitQueryPosition(QueryPosition position, QueryPosition changes, QueryPosition deletions, QueryPosition insertions){
      this.UpdateSourceContext(position, changes);
      if (position == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return position;
    }
    public virtual Node VisitQueryProject(QueryProject project, QueryProject changes, QueryProject deletions, QueryProject insertions){
      this.UpdateSourceContext(project, changes);
      if (project == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return project;
    }
    public virtual Node VisitQueryRollback(QueryRollback qr, QueryRollback changes, QueryRollback deletions, QueryRollback insertions){
      this.UpdateSourceContext(qr, changes);
      if (qr == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return qr;
    }
    public virtual Node VisitQueryQuantifier(QueryQuantifier qq, QueryQuantifier changes, QueryQuantifier deletions, QueryQuantifier insertions){
      this.UpdateSourceContext(qq, changes);
      if (qq == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return qq;
    }
    public virtual Node VisitQueryQuantifiedExpression(QueryQuantifiedExpression qqe, QueryQuantifiedExpression changes, QueryQuantifiedExpression deletions, QueryQuantifiedExpression insertions){
      this.UpdateSourceContext(qqe, changes);
      if (qqe == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return qqe;
    }
    public virtual Node VisitQuerySelect(QuerySelect select, QuerySelect changes, QuerySelect deletions, QuerySelect insertions){
      this.UpdateSourceContext(select, changes);
      if (select == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return select;
    }
    public virtual Node VisitQuerySingleton(QuerySingleton singleton, QuerySingleton changes, QuerySingleton deletions, QuerySingleton insertions){
      this.UpdateSourceContext(singleton, changes);
      if (singleton == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return singleton;
    }
    public virtual Node VisitQueryTransact(QueryTransact qt, QueryTransact changes, QueryTransact deletions, QueryTransact insertions){
      this.UpdateSourceContext(qt, changes);
      if (qt == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return qt;
    }
    public virtual Node VisitQueryTypeFilter(QueryTypeFilter filter, QueryTypeFilter changes, QueryTypeFilter deletions, QueryTypeFilter insertions){
      this.UpdateSourceContext(filter, changes);
      if (filter == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return filter;
    }
    public virtual Node VisitQueryUnion(QueryUnion union, QueryUnion changes, QueryUnion deletions, QueryUnion insertions){
      this.UpdateSourceContext(union, changes);
      if (union == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return union;
    }
    public virtual Node VisitQueryUpdate(QueryUpdate update, QueryUpdate changes, QueryUpdate deletions, QueryUpdate insertions){
      this.UpdateSourceContext(update, changes);
      if (update == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return update;
    }    
    public virtual Node VisitQueryYielder(QueryYielder yielder, QueryYielder changes, QueryYielder deletions, QueryYielder insertions){
      this.UpdateSourceContext(yielder, changes);
      if (yielder == null) return changes;
      if (changes != null){
        if (deletions == null || insertions == null)
          Debug.Assert(false);
        else{
        }
      }else if (deletions != null)
        return null;
      return yielder;
    }
#endif
    }
}
#endif