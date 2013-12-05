// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 11/22/2013 - EFW - Cleared out the conditional statements and unused code and updated based on changes to
// ListTemplate.cs.

using System.CodeDom.Compiler;
using System.Diagnostics;

namespace System.Compiler
{
    /// <summary>
    /// Base for all classes that process the IR using the visitor pattern.
    /// </summary>
    public abstract class Visitor
    {
        /// <summary>
        /// Switches on node.NodeType to call a visitor method that has been specialized for node.
        /// </summary>
        /// <param name="node">The node to be visited.</param>
        /// <returns> Returns null if node is null. Otherwise returns an updated node (possibly a different object).</returns>
        public abstract Node Visit(Node node);

        /// <summary>
        /// Transfers the state from one visitor to another. This enables separate visitor instances to cooperative process a single IR.
        /// </summary>
        public virtual void TransferStateTo(Visitor targetVisitor)
        {
        }

        public virtual ExpressionList VisitExpressionList(ExpressionList list)
        {
            if (list == null) return null;
            for (int i = 0, n = list.Count; i < n; i++)
                list[i] = (Expression)this.Visit(list[i]);
            return list;
        }
    }

    /// <summary>
    /// Walks an IR, mutating it into a new form
    /// </summary>   
    public class StandardVisitor : Visitor
    {
        public Visitor callingVisitor;

        protected bool memberListNamesChanged;

        public StandardVisitor()
        {
        }

        public StandardVisitor(Visitor callingVisitor)
        {
            this.callingVisitor = callingVisitor;
        }

        public virtual Node VisitUnknownNodeType(Node node)
        {
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

            return node;
        }

        public virtual Visitor GetVisitorFor(Node node)
        {
            if (node == null) return null;
            return (Visitor)node.GetVisitorFor(this, this.GetType().Name);
        }

        public override Node Visit(Node node)
        {
            if (node == null) return null;
            switch (node.NodeType)
            {

                case NodeType.Acquire:
                    return this.VisitAcquire((Acquire)node);

                case NodeType.AddressDereference:
                    return this.VisitAddressDereference((AddressDereference)node);

                case NodeType.AliasDefinition:
                    return this.VisitAliasDefinition((AliasDefinition)node);
                case NodeType.AnonymousNestedFunction:
                    return this.VisitAnonymousNestedFunction((AnonymousNestedFunction)node);
                case NodeType.ApplyToAll:
                    return this.VisitApplyToAll((ApplyToAll)node);

                case NodeType.Arglist:
                    return this.VisitExpression((Expression)node);

                case NodeType.ArglistArgumentExpression:
                    return this.VisitArglistArgumentExpression((ArglistArgumentExpression)node);
                case NodeType.ArglistExpression:
                    return this.VisitArglistExpression((ArglistExpression)node);

                case NodeType.ArrayType:
                    Debug.Assert(false); return null;
                case NodeType.Assembly:
                    return this.VisitAssembly((AssemblyNode)node);
                case NodeType.AssemblyReference:
                    return this.VisitAssemblyReference((AssemblyReference)node);

                case NodeType.Assertion:
                    return this.VisitAssertion((Assertion)node);
                case NodeType.Assumption:
                    return this.VisitAssumption((Assumption)node);
                case NodeType.AssignmentExpression:
                    return this.VisitAssignmentExpression((AssignmentExpression)node);

                case NodeType.AssignmentStatement:
                    return this.VisitAssignmentStatement((AssignmentStatement)node);
                case NodeType.Attribute:
                    return this.VisitAttributeNode((AttributeNode)node);

                case NodeType.Base:
                    return this.VisitBase((Base)node);

                case NodeType.Block:
                    return this.VisitBlock((Block)node);

                case NodeType.BlockExpression:
                    return this.VisitBlockExpression((BlockExpression)node);

                case NodeType.Branch:
                    return this.VisitBranch((Branch)node);

                case NodeType.Compilation:
                    return this.VisitCompilation((Compilation)node);
                case NodeType.CompilationUnit:
                    return this.VisitCompilationUnit((CompilationUnit)node);
                case NodeType.CompilationUnitSnippet:
                    return this.VisitCompilationUnitSnippet((CompilationUnitSnippet)node);

                case NodeType.Continue:
                    return this.VisitContinue((Continue)node);
                case NodeType.CurrentClosure:
                    return this.VisitCurrentClosure((CurrentClosure)node);

                case NodeType.DebugBreak:
                    return node;
                case NodeType.Call:
                case NodeType.Calli:
                case NodeType.Callvirt:
                case NodeType.Jmp:

                case NodeType.MethodCall:
                    return this.VisitMethodCall((MethodCall)node);

                case NodeType.Catch:
                    return this.VisitCatch((Catch)node);

                case NodeType.Class:
                    return this.VisitClass((Class)node);

                case NodeType.CoerceTuple:
                    return this.VisitCoerceTuple((CoerceTuple)node);
                case NodeType.CollectionEnumerator:
                    return this.VisitCollectionEnumerator((CollectionEnumerator)node);
                case NodeType.Composition:
                    return this.VisitComposition((Composition)node);

                case NodeType.Construct:
                    return this.VisitConstruct((Construct)node);
                case NodeType.ConstructArray:
                    return this.VisitConstructArray((ConstructArray)node);

                case NodeType.ConstructDelegate:
                    return this.VisitConstructDelegate((ConstructDelegate)node);
                case NodeType.ConstructFlexArray:
                    return this.VisitConstructFlexArray((ConstructFlexArray)node);
                case NodeType.ConstructIterator:
                    return this.VisitConstructIterator((ConstructIterator)node);
                case NodeType.ConstructTuple:
                    return this.VisitConstructTuple((ConstructTuple)node);

                case NodeType.DelegateNode:
                    return this.VisitDelegateNode((DelegateNode)node);

                case NodeType.DoWhile:
                    return this.VisitDoWhile((DoWhile)node);

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

                case NodeType.Exit:
                    return this.VisitExit((Exit)node);
                case NodeType.Read:
                case NodeType.Write:
                    return this.VisitExpose((Expose)node);
                case NodeType.ExpressionSnippet:
                    return this.VisitExpressionSnippet((ExpressionSnippet)node);

                case NodeType.ExpressionStatement:
                    return this.VisitExpressionStatement((ExpressionStatement)node);

                case NodeType.FaultHandler:
                    return this.VisitFaultHandler((FaultHandler)node);

                case NodeType.Field:
                    return this.VisitField((Field)node);

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

                case NodeType.Identifier:
                    return this.VisitIdentifier((Identifier)node);

                case NodeType.If:
                    return this.VisitIf((If)node);
                case NodeType.ImplicitThis:
                    return this.VisitImplicitThis((ImplicitThis)node);

                case NodeType.Indexer:
                    return this.VisitIndexer((Indexer)node);
                case NodeType.InstanceInitializer:
                    return this.VisitInstanceInitializer((InstanceInitializer)node);

                case NodeType.StaticInitializer:
                    return this.VisitStaticInitializer((StaticInitializer)node);
                case NodeType.Method:
                    return this.VisitMethod((Method)node);

                case NodeType.TemplateInstance:
                    return this.VisitTemplateInstance((TemplateInstance)node);
                case NodeType.StackAlloc:
                    return this.VisitStackAlloc((StackAlloc)node);

                case NodeType.Interface:
                    return this.VisitInterface((Interface)node);

                case NodeType.LabeledStatement:
                    return this.VisitLabeledStatement((LabeledStatement)node);

                case NodeType.Literal:
                    return this.VisitLiteral((Literal)node);
                case NodeType.Local:
                    return this.VisitLocal((Local)node);

                case NodeType.LocalDeclaration:
                    return this.VisitLocalDeclaration((LocalDeclaration)node);
                case NodeType.LocalDeclarationsStatement:
                    return this.VisitLocalDeclarationsStatement((LocalDeclarationsStatement)node);
                case NodeType.Lock:
                    return this.VisitLock((Lock)node);
                case NodeType.LRExpression:
                    return this.VisitLRExpression((LRExpression)node);

                case NodeType.MemberBinding:
                    return this.VisitMemberBinding((MemberBinding)node);

                case NodeType.Module:
                    return this.VisitModule((Module)node);
                case NodeType.ModuleReference:
                    return this.VisitModuleReference((ModuleReference)node);

                case NodeType.NameBinding:
                    return this.VisitNameBinding((NameBinding)node);

                case NodeType.NamedArgument:
                    return this.VisitNamedArgument((NamedArgument)node);

                case NodeType.Namespace:
                    return this.VisitNamespace((Namespace)node);

                case NodeType.Nop:
                case NodeType.SwitchCaseBottom:
                    return node;

                case NodeType.OptionalModifier:
                case NodeType.RequiredModifier:
                    //TODO: type modifiers should only be visited via VisitTypeReference
                    return this.VisitTypeModifier((TypeModifier)node);
                case NodeType.Parameter:
                    return this.VisitParameter((Parameter)node);
                case NodeType.Pop:
                    return this.VisitExpression((Expression)node);

                case NodeType.PrefixExpression:
                    return this.VisitPrefixExpression((PrefixExpression)node);
                case NodeType.PostfixExpression:
                    return this.VisitPostfixExpression((PostfixExpression)node);

                case NodeType.Property:
                    return this.VisitProperty((Property)node);

                case NodeType.Quantifier:
                    return this.VisitQuantifier((Quantifier)node);
                case NodeType.Comprehension:
                    return this.VisitComprehension((Comprehension)node);
                case NodeType.ComprehensionBinding:
                    return this.VisitComprehensionBinding((ComprehensionBinding)node);
                case NodeType.QualifiedIdentifer:
                    return this.VisitQualifiedIdentifier((QualifiedIdentifier)node);

                case NodeType.Rethrow:
                case NodeType.Throw:
                    return this.VisitThrow((Throw)node);

                case NodeType.RefValueExpression:
                    return this.VisitRefValueExpression((RefValueExpression)node);
                case NodeType.RefTypeExpression:
                    return this.VisitRefTypeExpression((RefTypeExpression)node);

                case NodeType.Return:
                    return this.VisitReturn((Return)node);

                case NodeType.Repeat:
                    return this.VisitRepeat((Repeat)node);
                case NodeType.ResourceUse:
                    return this.VisitResourceUse((ResourceUse)node);

                case NodeType.SecurityAttribute:
                    return this.VisitSecurityAttribute((SecurityAttribute)node);

                case NodeType.SetterValue:
                    return this.VisitSetterValue((SetterValue)node);
                case NodeType.StatementSnippet:
                    return this.VisitStatementSnippet((StatementSnippet)node);

                case NodeType.Struct:
                    return this.VisitStruct((Struct)node);

                case NodeType.Switch:
                    return this.VisitSwitch((Switch)node);
                case NodeType.SwitchCase:
                    return this.VisitSwitchCase((SwitchCase)node);

                case NodeType.SwitchInstruction:
                    return this.VisitSwitchInstruction((SwitchInstruction)node);

                case NodeType.Typeswitch:
                    return this.VisitTypeswitch((Typeswitch)node);
                case NodeType.TypeswitchCase:
                    return this.VisitTypeswitchCase((TypeswitchCase)node);

                case NodeType.This:
                    return this.VisitThis((This)node);

                case NodeType.Try:
                    return this.VisitTry((Try)node);

                case NodeType.TypeMemberSnippet:
                    return this.VisitTypeMemberSnippet((TypeMemberSnippet)node);

                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                    return this.VisitTypeParameter((TypeNode)node);

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
                case NodeType.Cpblk:
                case NodeType.Initblk:
                    return this.VisitTernaryExpression((TernaryExpression)node);

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
                case NodeType.NullCoalesingExpression:
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
                    return this.VisitBinaryExpression((BinaryExpression)node);

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
                case NodeType.ReadOnlyAddressOf:
                case NodeType.Sizeof:
                case NodeType.SkipCheck:
                case NodeType.Typeof:
                case NodeType.UnaryPlus:
                    return this.VisitUnaryExpression((UnaryExpression)node);

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

        public virtual Expression VisitBase(Base Base)
        {
            return Base;
        }

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

        public virtual Expression VisitBlockExpression(BlockExpression blockExpression)
        {
            if (blockExpression == null) return null;
            blockExpression.Block = this.VisitBlock(blockExpression.Block);
            return blockExpression;
        }

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

        public virtual Class VisitClass(Class Class)
        {
            return (Class)this.VisitTypeNode(Class);
        }

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

        public virtual Expression VisitConstruct(Construct cons)
        {
            if (cons == null) return null;
            cons.Constructor = this.VisitExpression(cons.Constructor);
            cons.Operands = this.VisitExpressionList(cons.Operands);
            cons.Owner = this.VisitExpression(cons.Owner);
            return cons;
        }
        public virtual Expression VisitConstructArray(ConstructArray consArr)
        {
            if (consArr == null) return null;
            consArr.ElementType = this.VisitTypeReference(consArr.ElementType);
            consArr.Operands = this.VisitExpressionList(consArr.Operands);
            consArr.Initializers = this.VisitExpressionList(consArr.Initializers);
            consArr.Owner = this.VisitExpression(consArr.Owner);
            return consArr;
        }

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

        public virtual Statement VisitContinue(Continue Continue)
        {
            return Continue;
        }
        public virtual Expression VisitCurrentClosure(CurrentClosure currentClosure)
        {
            return currentClosure;
        }

        public virtual DelegateNode VisitDelegateNode(DelegateNode delegateNode)
        {
            if (delegateNode == null) return null;
            delegateNode = (DelegateNode)this.VisitTypeNode(delegateNode);
            if (delegateNode == null) return null;
            delegateNode.Parameters = this.VisitParameterList(delegateNode.Parameters);
            delegateNode.ReturnType = this.VisitTypeReference(delegateNode.ReturnType);
            return delegateNode;
        }

        public virtual Statement VisitDoWhile(DoWhile doWhile)
        {
            if (doWhile == null) return null;
            doWhile.Invariants = this.VisitLoopInvariantList(doWhile.Invariants);
            doWhile.Body = this.VisitBlock(doWhile.Body);
            doWhile.Condition = this.VisitExpression(doWhile.Condition);
            return doWhile;
        }

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

        public virtual Expression VisitExpressionSnippet(ExpressionSnippet snippet)
        {
            return snippet;
        }

        public virtual Statement VisitExpressionStatement(ExpressionStatement statement)
        {
            if (statement == null) return null;
            statement.Expression = this.VisitExpression(statement.Expression);
            return statement;
        }

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

        public virtual Field VisitField(Field field)
        {
            if (field == null) return null;
            field.Attributes = this.VisitAttributeList(field.Attributes);
            field.Type = this.VisitTypeReference(field.Type);
            field.Initializer = this.VisitExpression(field.Initializer);
            field.ImplementedInterfaces = this.VisitInterfaceReferenceList(field.ImplementedInterfaces);
            return field;
        }

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

        public virtual Expression VisitIdentifier(Identifier identifier)
        {
            return identifier;
        }

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

        public virtual InstanceInitializer VisitInstanceInitializer(InstanceInitializer cons)
        {
            return (InstanceInitializer)this.VisitMethod(cons);
        }

        public virtual Statement VisitLabeledStatement(LabeledStatement lStatement)
        {
            if (lStatement == null) return null;
            lStatement.Statement = (Statement)this.Visit(lStatement.Statement);
            return lStatement;
        }

        public virtual Expression VisitLiteral(Literal literal)
        {
            return literal;
        }
        public virtual Expression VisitLocal(Local local)
        {
            if (local == null) return null;
            local.Type = this.VisitTypeReference(local.Type);

            LocalBinding lb = local as LocalBinding;
            if (lb != null)
            {
                Local loc = this.VisitLocal(lb.BoundLocal) as Local;
                if (loc != null)
                    lb.BoundLocal = loc;
            }

            return local;
        }

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
            method.ImplementedTypes = this.VisitTypeReferenceList(method.ImplementedTypes);
            method.Parameters = this.VisitParameterList(method.Parameters);

            if (TargetPlatform.UseGenerics)
            {
                method.TemplateArguments = this.VisitTypeReferenceList(method.TemplateArguments);
                method.TemplateParameters = this.VisitTypeParameterList(method.TemplateParameters);
            }

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

        public virtual Expression VisitNameBinding(NameBinding nameBinding)
        {
            return nameBinding;
        }

        public virtual Expression VisitNamedArgument(NamedArgument namedArgument)
        {
            if (namedArgument == null) return null;
            namedArgument.Value = this.VisitExpression(namedArgument.Value);
            return namedArgument;
        }

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

        public virtual Expression VisitParameter(Parameter parameter)
        {
            if (parameter == null) return null;
            parameter.Attributes = this.VisitAttributeList(parameter.Attributes);
            parameter.Type = this.VisitTypeReference(parameter.Type);
            parameter.DefaultValue = this.VisitExpression(parameter.DefaultValue);
            ParameterBinding pb = parameter as ParameterBinding;

            if (pb != null)
            {
                Parameter par = this.VisitParameter(pb.BoundParameter) as Parameter;
                if (par != null)
                    pb.BoundParameter = par;
            }

            return parameter;
        }

        public virtual ParameterList VisitParameterList(ParameterList parameterList)
        {
            if (parameterList == null) return null;
            for (int i = 0, n = parameterList.Count; i < n; i++)
                parameterList[i] = (Parameter)this.VisitParameter(parameterList[i]);
            return parameterList;
        }

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

        public virtual Property VisitProperty(Property property)
        {
            if (property == null) return null;
            property.Attributes = this.VisitAttributeList(property.Attributes);
            property.Parameters = this.VisitParameterList(property.Parameters);
            property.Type = this.VisitTypeReference(property.Type);
            return property;
        }

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


        public virtual Statement VisitReturn(Return Return)
        {
            if (Return == null) return null;
            Return.Expression = this.VisitExpression(Return.Expression);
            return Return;
        }

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

        public virtual Expression VisitSetterValue(SetterValue value)
        {
            return value;
        }

        public virtual StatementList VisitStatementList(StatementList statements)
        {
            if (statements == null) return null;
            for (int i = 0, n = statements.Count; i < n; i++)
                statements[i] = (Statement)this.Visit(statements[i]);
            return statements;
        }

        public virtual StatementSnippet VisitStatementSnippet(StatementSnippet snippet)
        {
            return snippet;
        }

        public virtual StaticInitializer VisitStaticInitializer(StaticInitializer cons)
        {
            return (StaticInitializer)this.VisitMethod(cons);
        }
        public virtual Struct VisitStruct(Struct Struct)
        {
            return (Struct)this.VisitTypeNode(Struct);
        }

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

        public virtual Statement VisitSwitchInstruction(SwitchInstruction switchInstruction)
        {
            if (switchInstruction == null) return null;
            switchInstruction.Expression = this.VisitExpression(switchInstruction.Expression);
            return switchInstruction;
        }

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
            if(This == null)
                return null;

            This.Type = this.VisitTypeReference(This.Type);
            ThisBinding tb = This as ThisBinding;

            if (tb != null)
            {
                This boundThis = this.VisitThis(tb.BoundThis) as This;
                if (boundThis != null)
                    tb.BoundThis = boundThis;
            }

            return This;
        }

        public virtual Statement VisitThrow(Throw Throw)
        {
            if (Throw == null) return null;
            Throw.Expression = this.VisitExpression(Throw.Expression);
            return Throw;
        }

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

        public virtual TypeMemberSnippet VisitTypeMemberSnippet(TypeMemberSnippet snippet)
        {
            return snippet;
        }

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

        public virtual TypeReference VisitTypeReference(TypeReference type)
        {
            return type;
        }

        public virtual TypeNodeList VisitTypeReferenceList(TypeNodeList typeReferences)
        {
            if (typeReferences == null) return null;
            for (int i = 0, n = typeReferences.Count; i < n; i++)
                typeReferences[i] = this.VisitTypeReference(typeReferences[i]);
            return typeReferences;
        }

        public virtual Expression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            if (unaryExpression == null) return null;
            unaryExpression.Operand = this.VisitExpression(unaryExpression.Operand);
            return unaryExpression;
        }

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
    }

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
}
