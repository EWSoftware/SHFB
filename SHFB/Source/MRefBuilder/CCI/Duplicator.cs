// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 08/07/2012 - EFW - Fixed Duplicator.VisitTypeReference so that it won't get stuck in an endless loop
// when a type contains a nested type that itself implements a nested type from within the containing type.
// 11/21/2013 - EFW - Cleared out the conditional statements and updated based on changes to ListTemplate.cs.

using System.Diagnostics;

namespace System.Compiler
{
    /* The idea here is to do a tree traversal of the IR graph, rewriting the IR with duplicate nodes from the
     * bottom up. Nodes that may appear more than once in the graph keep track of their duplicates in the
     * DuplicateFor hash table and all references to these nodes are replaced with references to the
     * corresponding duplicates.
     * 
     * A complication arises because of the need to duplicate IR subgraphs, such as Methods, Types,
     * CompilationUnits and individual Modules.  The subgraphs contain references to "foreign" nodes that should
     * not be duplicated and it is thus necessary to be able to tell whether or not a node should be duplicated.
     * This is done by tracking all the types that are members of the subgraph to be duplicated in the
     * TypesToBeDuplicated hash table. Types are duplicated only if they are members of this table, while fields
     * and methods are duplicated only if their declaring types are members of this table. 
     * 
     * Since every type contains a reference to its declaring module, the module in which duplicated types will
     * be inserted must be specified to the constructor.
     */

    /// <summary>
    /// Walks an IR, duplicating it while fixing up self references to point to the duplicate IR. Only good for one duplication. 
    /// Largest unit of duplication is a single module.
    /// </summary>
    public class Duplicator : StandardVisitor
    {
        public TrivialHashtable/*!*/ DuplicateFor;
        public TrivialHashtable/*!*/ TypesToBeDuplicated;
        public Module/*!*/ TargetModule;
        public TypeNode TargetType;
        public Method TargetMethod;
        public TypeNode OriginalTargetType;
        public bool SkipBodies;
        public bool RecordOriginalAsTemplate;
        public bool CopyDocumentation;

        /// <param name="module">The module into which the duplicate IR will be grafted.</param>
        /// <param name="type">The type into which the duplicate Member will be grafted. Ignored if entire type, or larger unit is duplicated.</param>
        public Duplicator(Module/*!*/ module, TypeNode type)
        {
            this.TargetModule = module;
            this.TargetType = this.OriginalTargetType = type;
            this.DuplicateFor = new TrivialHashtable();
            this.TypesToBeDuplicated = new TrivialHashtable();
            //^ base();
        }

        public Duplicator(Visitor/*!*/ callingVisitor)
            : base(callingVisitor)
        {
            /*^
            //Dummy initializations to satisfy compiler.
            Duplicator cdv = callingVisitor as Duplicator;
            if (cdv == null) {
              this.DuplicateFor = new TrivialHashtable();
              this.TypesToBeDuplicated = new TrivialHashtable();
              this.TargetModule = new Module();
              this.TargetType = this.OriginalTargetType = new Class();
            } else {
              this.DuplicateFor = cdv.DuplicateFor;
              this.TypesToBeDuplicated = cdv.TypesToBeDuplicated;
              this.TargetModule = cdv.TargetModule;
              this.TargetType = cdv.TargetType;
              this.OriginalTargetType = cdv.OriginalTargetType;
            }
            base; //The real initializations happen here
            ^*/
        }
        public override void TransferStateTo(Visitor targetVisitor)
        {
            base.TransferStateTo(targetVisitor);

            if(!(targetVisitor is Duplicator target))
                return;

            target.DuplicateFor = this.DuplicateFor;
            target.OriginalTargetType = this.OriginalTargetType;
            target.RecordOriginalAsTemplate = this.RecordOriginalAsTemplate;
            target.SkipBodies = this.SkipBodies;
            target.TargetMethod = this.TargetMethod;
            target.TargetModule = this.TargetModule;
            target.TargetType = this.TargetType;
            target.TypesToBeDuplicated = this.TypesToBeDuplicated;
        }

        public virtual void FindTypesToBeDuplicated(Node node)
        {
        }

        public virtual void FindTypesToBeDuplicated(NodeList nodes)
        {
            if (nodes == null)
                return;
            
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                Node node = nodes[i];

                if (node == null)
                    continue;

                if (node is Namespace ns)
                    this.FindTypesToBeDuplicated(ns);
                else
                    this.FindTypesToBeDuplicated(node);
            }
        }
        public virtual void FindTypesToBeDuplicated(Namespace nspace)
        {
            if (nspace == null) return;
            this.FindTypesToBeDuplicated(nspace.Types);
            this.FindTypesToBeDuplicated(nspace.NestedNamespaces);
        }

        public virtual void FindTypesToBeDuplicated(NamespaceList namespaces)
        {
            if (namespaces == null) return;
            for (int i = 0, n = namespaces.Count; i < n; i++)
            {
                Namespace nspace = namespaces[i];
                if (nspace == null) continue;
                this.FindTypesToBeDuplicated(nspace.Types);

                this.FindTypesToBeDuplicated(nspace.NestedNamespaces);

            }
        }
        public virtual void FindTypesToBeDuplicated(TypeNodeList types)
        {
            if (types == null) return;
            for (int i = 0, n = types.Count; i < n; i++)
            {
                TypeNode t = types[i];
                if (t == null) continue;
                this.TypesToBeDuplicated[t.UniqueKey] = t;
                this.FindTypesToBeDuplicated(t.NestedTypes);
                this.FindTypesToBeDuplicated(t.TemplateParameters);
            }
        }
        public override Node Visit(Node node)
        {
            node = base.Visit(node);

            if(node is Expression e)
                e.Type = this.VisitTypeReference(e.Type);

            return node;
        }

        public override Expression VisitAddressDereference(AddressDereference addr)
        {
            if (addr == null) return null;
            return base.VisitAddressDereference((AddressDereference)addr.Clone());
        }

        public override AliasDefinition VisitAliasDefinition(AliasDefinition aliasDefinition)
        {
            if (aliasDefinition == null) return null;
            return base.VisitAliasDefinition((AliasDefinition)aliasDefinition.Clone());
        }

        public override AliasDefinitionList VisitAliasDefinitionList(AliasDefinitionList aliasDefinitions)
        {
            if(aliasDefinitions == null)
                return null;

            return base.VisitAliasDefinitionList(new AliasDefinitionList(aliasDefinitions));
        }

        public override Expression VisitAnonymousNestedFunction(AnonymousNestedFunction func)
        {
            if (func == null) return null;
            AnonymousNestedFunction dup = (AnonymousNestedFunction)func.Clone();
            if (func.Method != null)
            {
                dup.Method = this.VisitMethod(func.Method);
                //^ assume dup.Method != null;
                dup.Parameters = dup.Method.Parameters;
                dup.Body = dup.Method.Body;
                return dup;
            }
            return base.VisitAnonymousNestedFunction(dup);
        }
        public override Expression VisitApplyToAll(ApplyToAll applyToAll)
        {
            if (applyToAll == null) return null;
            return base.VisitApplyToAll((ApplyToAll)applyToAll.Clone());
        }

        public override AssemblyNode VisitAssembly(AssemblyNode assembly)
        {
            if (assembly == null) return null;
            this.FindTypesToBeDuplicated(assembly.Types);
            return base.VisitAssembly((AssemblyNode)assembly.Clone());
        }
        public override AssemblyReference VisitAssemblyReference(AssemblyReference assemblyReference)
        {
            if (assemblyReference == null) return null;
            return base.VisitAssemblyReference((AssemblyReference)assemblyReference.Clone());
        }

        public override Statement VisitAssertion(Assertion assertion)
        {
            if (assertion == null) return null;
            return base.VisitAssertion((Assertion)assertion.Clone());
        }
        public override Statement VisitAssumption(Assumption Assumption)
        {
            if (Assumption == null) return null;
            return base.VisitAssumption((Assumption)Assumption.Clone());
        }
        public override Expression VisitAssignmentExpression(AssignmentExpression assignment)
        {
            if (assignment == null) return null;
            return base.VisitAssignmentExpression((AssignmentExpression)assignment.Clone());
        }

        public override Statement VisitAssignmentStatement(AssignmentStatement assignment)
        {
            if (assignment == null) return null;
            return base.VisitAssignmentStatement((AssignmentStatement)assignment.Clone());
        }
        public override Expression VisitAttributeConstructor(AttributeNode attribute)
        {
            if (attribute == null || attribute.Constructor == null) return null;
            return this.VisitExpression((Expression)attribute.Constructor.Clone());
        }
        public override AttributeNode VisitAttributeNode(AttributeNode attribute)
        {
            if (attribute == null) return null;
            return base.VisitAttributeNode((AttributeNode)attribute.Clone());
        }

        public override AttributeList VisitAttributeList(AttributeList attributes)
        {
            if(attributes == null)
                return null;

            return base.VisitAttributeList(new AttributeList(attributes));
        }

        public override Expression VisitBase(Base Base)
        {
            if (Base == null) return null;
            return base.VisitBase((Base)Base.Clone());
        }

        public override Expression VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression == null) return null;
            binaryExpression = (BinaryExpression)base.VisitBinaryExpression((BinaryExpression)binaryExpression.Clone());
            return binaryExpression;
        }
        public override Block VisitBlock(Block block)
        {
            if (block == null) return null;
            Block dup = (Block)this.DuplicateFor[block.UniqueKey];
            if (dup != null) return dup;
            this.DuplicateFor[block.UniqueKey] = dup = (Block)block.Clone();
            return base.VisitBlock(dup);
        }

        public override Expression VisitBlockExpression(BlockExpression blockExpression)
        {
            if (blockExpression == null) return null;
            return base.VisitBlockExpression((BlockExpression)blockExpression.Clone());
        }

        public override BlockList VisitBlockList(BlockList blockList)
        {
            if(blockList == null)
                return null;

            return base.VisitBlockList(new BlockList(blockList));
        }

        public override Statement VisitBranch(Branch branch)
        {
            if (branch == null) return null;
            branch = (Branch)base.VisitBranch((Branch)branch.Clone());
            if (branch == null) return null;
            branch.Target = this.VisitBlock(branch.Target);
            return branch;
        }

        public override Statement VisitCatch(Catch Catch)
        {
            if (Catch == null) return null;
            return base.VisitCatch((Catch)Catch.Clone());
        }

        public override CatchList VisitCatchList(CatchList catchers)
        {
            if(catchers == null)
                return null;

            return base.VisitCatchList(new CatchList(catchers));
        }

        public override Expression VisitCoerceTuple(CoerceTuple coerceTuple)
        {
            if (coerceTuple == null) return null;
            return base.VisitCoerceTuple((CoerceTuple)coerceTuple.Clone());
        }
        public override CollectionEnumerator VisitCollectionEnumerator(CollectionEnumerator ce)
        {
            if (ce == null) return null;
            return base.VisitCollectionEnumerator((CollectionEnumerator)ce.Clone());
        }
        public override Compilation VisitCompilation(Compilation compilation)
        {
            if (compilation == null || compilation.TargetModule == null) return null;
            this.FindTypesToBeDuplicated(compilation.TargetModule.Types);
            return base.VisitCompilation((Compilation)compilation.Clone());
        }
        public override CompilationUnit VisitCompilationUnit(CompilationUnit cUnit)
        {
            if (cUnit == null) return null;
            this.FindTypesToBeDuplicated(cUnit.Nodes);
            return base.VisitCompilationUnit((CompilationUnit)cUnit.Clone());
        }
        public override CompilationUnit VisitCompilationUnitSnippet(CompilationUnitSnippet snippet)
        {
            if (snippet == null) return null;
            return base.VisitCompilationUnitSnippet((CompilationUnitSnippet)snippet.Clone());
        }
        public override Node VisitComposition(Composition comp)
        {
            if (comp == null) return null;
            return base.VisitComposition((Composition)comp.Clone());
        }

        public override Expression VisitConstruct(Construct cons)
        {
            if (cons == null) return null;
            return base.VisitConstruct((Construct)cons.Clone());
        }
        public override Expression VisitConstructArray(ConstructArray consArr)
        {
            if (consArr == null) return null;
            return base.VisitConstructArray((ConstructArray)consArr.Clone());
        }

        public override Expression VisitConstructDelegate(ConstructDelegate consDelegate)
        {
            if (consDelegate == null) return null;
            return base.VisitConstructDelegate((ConstructDelegate)consDelegate.Clone());
        }
        public override Expression VisitConstructFlexArray(ConstructFlexArray consArr)
        {
            if (consArr == null) return null;
            return base.VisitConstructFlexArray((ConstructFlexArray)consArr.Clone());
        }
        public override Expression VisitConstructIterator(ConstructIterator consIterator)
        {
            if (consIterator == null) return null;
            return base.VisitConstructIterator((ConstructIterator)consIterator.Clone());
        }
        public override Expression VisitConstructTuple(ConstructTuple consTuple)
        {
            if (consTuple == null) return null;
            return base.VisitConstructTuple((ConstructTuple)consTuple.Clone());
        }

        public override Statement VisitContinue(Continue Continue)
        {
            if (Continue == null) return null;
            return base.VisitContinue((Continue)Continue.Clone());
        }
        public override Expression VisitCurrentClosure(CurrentClosure currentClosure)
        {
            if (currentClosure == null) return null;
            return base.VisitCurrentClosure((CurrentClosure)currentClosure.Clone());
        }

        public override DelegateNode VisitDelegateNode(DelegateNode delegateNode)
        {
            return this.VisitTypeNode(delegateNode) as DelegateNode;
        }

        public override Statement VisitDoWhile(DoWhile doWhile)
        {
            if (doWhile == null) return null;
            return base.VisitDoWhile((DoWhile)doWhile.Clone());
        }

        public override Statement VisitEndFilter(EndFilter endFilter)
        {
            if (endFilter == null) return null;
            return base.VisitEndFilter((EndFilter)endFilter.Clone());
        }
        public override Statement VisitEndFinally(EndFinally endFinally)
        {
            if (endFinally == null) return null;
            return base.VisitEndFinally((EndFinally)endFinally.Clone());
        }

        public override Event VisitEvent(Event evnt)
        {
            if (evnt == null) return null;
            Event dup = (Event)this.DuplicateFor[evnt.UniqueKey];
            if (dup != null) return dup;
            this.DuplicateFor[evnt.UniqueKey] = dup = (Event)evnt.Clone();

            if (this.CopyDocumentation) dup.Documentation = evnt.Documentation;

            dup.HandlerAdder = this.VisitMethod(evnt.HandlerAdder);
            dup.HandlerCaller = this.VisitMethod(evnt.HandlerCaller);
            dup.HandlerRemover = this.VisitMethod(evnt.HandlerRemover);
            dup.OtherMethods = this.VisitMethodList(evnt.OtherMethods);
            dup.DeclaringType = this.TargetType;
            return base.VisitEvent(dup);
        }

        public virtual ExceptionHandler VisitExceptionHandler(ExceptionHandler handler)
        {
            if (handler == null) return null;
            handler = (ExceptionHandler)handler.Clone();
            handler.BlockAfterHandlerEnd = this.VisitBlock(handler.BlockAfterHandlerEnd);
            handler.BlockAfterTryEnd = this.VisitBlock(handler.BlockAfterTryEnd);
            handler.FilterExpression = this.VisitBlock(handler.FilterExpression);
            handler.FilterType = this.VisitTypeReference(handler.FilterType);
            handler.HandlerStartBlock = this.VisitBlock(handler.HandlerStartBlock);
            handler.TryStartBlock = this.VisitBlock(handler.TryStartBlock);
            return handler;
        }

        public virtual ExceptionHandlerList VisitExceptionHandlerList(ExceptionHandlerList handlers)
        {
            if(handlers == null)
                return null;

            int n = handlers.Count;
            ExceptionHandlerList result = new ExceptionHandlerList();

            for(int i = 0; i < n; i++)
                result.Add(this.VisitExceptionHandler(handlers[i]));

            return result;
        }

        public override Statement VisitExit(Exit exit)
        {
            if (exit == null) return null;
            return base.VisitExit((Exit)exit.Clone());
        }
        public override Statement VisitExpose(Expose Expose)
        {
            if (Expose == null) return null;
            return base.VisitExpose((Expose)Expose.Clone());
        }

        public override Expression VisitExpression(Expression expression)
        {
            if (expression == null) return null;
            switch (expression.NodeType)
            {
                case NodeType.Dup:
                case NodeType.Arglist:
                    expression = (Expression)expression.Clone();
                    break;
                case NodeType.Pop:
                    if(expression is UnaryExpression uex)
                    {
                        uex = (UnaryExpression)uex.Clone();
                        uex.Operand = this.VisitExpression(uex.Operand);
                        expression = uex;
                    }
                    else
                        expression = (Expression)expression.Clone();
                    break;
                default:
                    expression = (Expression)this.Visit(expression);
                    break;
            }
            if (expression == null) return null;
            expression.Type = this.VisitTypeReference(expression.Type);
            return expression;
        }

        public override ExpressionList VisitExpressionList(ExpressionList expressions)
        {
            if(expressions == null)
                return null;

            return base.VisitExpressionList(new ExpressionList(expressions));
        }

        public override Expression VisitExpressionSnippet(ExpressionSnippet snippet)
        {
            if (snippet == null) return null;
            return base.VisitExpressionSnippet((ExpressionSnippet)snippet.Clone());
        }

        public override Statement VisitExpressionStatement(ExpressionStatement statement)
        {
            if (statement == null) return null;
            return base.VisitExpressionStatement((ExpressionStatement)statement.Clone());
        }

        public override Statement VisitFaultHandler(FaultHandler faultHandler)
        {
            if (faultHandler == null) return null;
            return base.VisitFaultHandler((FaultHandler)faultHandler.Clone());
        }

        public override FaultHandlerList VisitFaultHandlerList(FaultHandlerList faultHandlers)
        {
            if(faultHandlers == null)
                return null;

            return base.VisitFaultHandlerList(new FaultHandlerList(faultHandlers));
        }

        public override Field VisitField(Field field)
        {
            if (field == null)
                return null;

            Field dup = (Field)this.DuplicateFor[field.UniqueKey];
            
            if (dup != null)
                return dup;
            
            this.DuplicateFor[field.UniqueKey] = dup = (Field)field.Clone();
            
            if (field.MarshallingInformation != null)
                dup.MarshallingInformation = field.MarshallingInformation.Clone();

            if(dup is ParameterField pField)
                pField.Parameter = (Parameter)this.VisitParameter(pField.Parameter);

            dup.DeclaringType = this.TargetType;

            if (this.CopyDocumentation)
                dup.Documentation = field.Documentation;

            return base.VisitField(dup);
        }

        public override Block VisitFieldInitializerBlock(FieldInitializerBlock block)
        {
            if (block == null) return null;
            return base.VisitFieldInitializerBlock((FieldInitializerBlock)block.Clone());
        }

        public override FieldList VisitFieldList(FieldList fields)
        {
            if(fields == null)
                return null;

            return base.VisitFieldList(new FieldList(fields));
        }

        public override Statement VisitFilter(Filter filter)
        {
            if (filter == null) return null;
            return base.VisitFilter((Filter)filter.Clone());
        }

        public override FilterList VisitFilterList(FilterList filters)
        {
            if(filters == null)
                return null;

            return base.VisitFilterList(new FilterList(filters));
        }

        public override Statement VisitFinally(Finally Finally)
        {
            if (Finally == null) return null;
            return base.VisitFinally((Finally)Finally.Clone());
        }
        public override Statement VisitFixed(Fixed Fixed)
        {
            if (Fixed == null) return null;
            return base.VisitFixed((Fixed)Fixed.Clone());
        }
        public override Statement VisitFor(For For)
        {
            if (For == null) return null;
            return base.VisitFor((For)For.Clone());
        }
        public override Statement VisitForEach(ForEach forEach)
        {
            if (forEach == null) return null;
            return base.VisitForEach((ForEach)forEach.Clone());
        }
        public override Statement VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            if (functionDeclaration == null) return null;
            return base.VisitFunctionDeclaration((FunctionDeclaration)functionDeclaration.Clone());
        }
        public override Statement VisitGoto(Goto Goto)
        {
            if (Goto == null) return null;
            return base.VisitGoto((Goto)Goto.Clone());
        }
        public override Statement VisitGotoCase(GotoCase gotoCase)
        {
            if (gotoCase == null) return null;
            return base.VisitGotoCase((GotoCase)gotoCase.Clone());
        }

        public override Expression VisitIdentifier(Identifier identifier)
        {
            if (identifier == null) return null;
            return base.VisitIdentifier((Identifier)identifier.Clone());
        }

        public override Statement VisitIf(If If)
        {
            if (If == null) return null;
            return base.VisitIf((If)If.Clone());
        }
        public override Expression VisitImplicitThis(ImplicitThis implicitThis)
        {
            if (implicitThis == null) return null;
            return base.VisitImplicitThis((ImplicitThis)implicitThis.Clone());
        }

        public override Expression VisitIndexer(Indexer indexer)
        {
            if (indexer == null) return null;
            indexer = (Indexer)base.VisitIndexer((Indexer)indexer.Clone());
            if (indexer == null) return null;
            indexer.ElementType = this.VisitTypeReference(indexer.ElementType);
            return indexer;
        }

        public override InterfaceList VisitInterfaceReferenceList(InterfaceList interfaceReferences)
        {
            if(interfaceReferences == null)
                return null;

            return base.VisitInterfaceReferenceList(new InterfaceList(interfaceReferences));
        }

        public override Statement VisitLabeledStatement(LabeledStatement lStatement)
        {
            if (lStatement == null) return null;
            return base.VisitLabeledStatement((LabeledStatement)lStatement.Clone());
        }

        public override Expression VisitLiteral(Literal literal)
        {
            if (literal == null) return null;
            TypeNode t = literal.Value as TypeNode;
            if (t != null)
                return new Literal(this.VisitTypeReference(t), literal.Type, literal.SourceContext);
            ArrayType at = literal.Type as ArrayType;
            if (at != null && at.ElementType is TypeNode)
            {
                TypeNode cloneType = this.VisitTypeReference(literal.Type);
                TypeNode[] val = literal.Value as TypeNode[];
                int len = val == null ? 0 : val.Length;
                TypeNode[] newVal = val == null ? null : new TypeNode[len];
                for (int i = 0; i < len; i++)
                {
                    newVal[i] = this.VisitTypeReference(val[i]);
                }
                return new Literal(newVal, cloneType);
            }
            return (Literal)literal.Clone();
        }
        public override Expression VisitLocal(Local local)
        {
            if (local == null) return null;
            Local dup = (Local)this.DuplicateFor[local.UniqueKey];
            if (dup != null) return dup;
            this.DuplicateFor[local.UniqueKey] = dup = (Local)local.Clone();
            return base.VisitLocal(dup);
        }

        public override LocalDeclaration VisitLocalDeclaration(LocalDeclaration localDeclaration)
        {
            if (localDeclaration == null) return null;
            return base.VisitLocalDeclaration((LocalDeclaration)localDeclaration.Clone());
        }

        public override LocalDeclarationList VisitLocalDeclarationList(LocalDeclarationList localDeclarations)
        {
            if(localDeclarations == null)
                return null;

            return base.VisitLocalDeclarationList(new LocalDeclarationList(localDeclarations));
        }

        public override Statement VisitLocalDeclarationsStatement(LocalDeclarationsStatement localDeclarations)
        {
            if (localDeclarations == null) return null;
            return base.VisitLocalDeclarationsStatement((LocalDeclarationsStatement)localDeclarations.Clone());
        }
        public override Statement VisitLock(Lock Lock)
        {
            if (Lock == null) return null;
            return base.VisitLock((Lock)Lock.Clone());
        }
        public override Statement VisitAcquire(Acquire acquire)
        {
            if (acquire == null) return null;
            return base.VisitAcquire((Acquire)acquire.Clone());
        }
        public override Statement VisitResourceUse(ResourceUse resourceUse)
        {
            if (resourceUse == null) return null;
            return base.VisitResourceUse((ResourceUse)resourceUse.Clone());
        }
        public override Expression VisitLRExpression(LRExpression expr)
        {
            if (expr == null) return null;
            return base.VisitLRExpression((LRExpression)expr.Clone());
        }

        public override Expression VisitMemberBinding(MemberBinding memberBinding)
        {
            if (memberBinding == null) return null;
            memberBinding = (MemberBinding)memberBinding.Clone();
            memberBinding.TargetObject = this.VisitExpression(memberBinding.TargetObject);
            memberBinding.Type = this.VisitTypeReference(memberBinding.Type);
            memberBinding.BoundMember = this.VisitMemberReference(memberBinding.BoundMember);
            return memberBinding;
        }

        public override MemberList VisitMemberList(MemberList members)
        {
            if(members == null)
                return null;

            return base.VisitMemberList(new MemberList(members));
        }

        public virtual Member VisitMemberReference(Member member)
        {
            if (member == null)
                return null;

            Member dup = (Member)this.DuplicateFor[member.UniqueKey];

            if(dup != null)
                return dup;

            if(member is ParameterField && !(member.DeclaringType is ClosureClass))
                return member; // Can happen when duplicating expressions within a method

            TypeNode t = member as TypeNode;

            if(t != null)
                member = this.VisitTypeReference(t);

            if(member == null)
                return null;

            if(member is Method method && method.Template != null && method.TemplateArguments != null && method.TemplateArguments.Count > 0)
            {
                Method template = this.VisitMemberReference(method.Template) as Method;
                bool needNewInstance = template != null && template != method.Template;
                TypeNodeList args = new TypeNodeList(method.TemplateArguments);

                for(int i = 0, n = args.Count; i < n; i++)
                {
                    TypeNode arg = this.VisitTypeReference(args[i]);

                    if(arg != null && arg != args[i])
                    {
                        args[i] = arg;
                        needNewInstance = true;
                    }
                }

                if(needNewInstance)
                {
                    //^ assert template != null;
                    return template.GetTemplateInstance(this.TargetType, args);
                }

                return method;
            }

            TypeNode declaringType = member.DeclaringType;

            if(declaringType == null)
                return member;

            if(declaringType.Template == null && this.TypesToBeDuplicated[declaringType.UniqueKey] == null)
                return member;

            TypeNode tgtType = this.VisitTypeReference(declaringType); //duplicates its members

            if(tgtType == null)
                return null;

            dup = (Member)this.DuplicateFor[member.UniqueKey];

            if(dup == null)
            {
                if(declaringType.Template != null)
                {
                    if(tgtType != declaringType && tgtType != null)
                        return Specializer.GetCorrespondingMember(member, tgtType);

                    return member;
                }

                //Can get here when tgtType has not yet been completely duplicated
                TypeNode savedTargetType = this.TargetType;
                this.TargetType = tgtType;
                dup = (Member)this.Visit(member);
                this.TargetType = savedTargetType;
            }

            return dup;
        }

        public virtual MemberList VisitMemberReferenceList(MemberList members)
        {
            if(members == null)
                return null;

            int n = members.Count;
            MemberList dup = new MemberList();

            for(int i = 0; i < n; i++)
                dup.Add(this.VisitMemberReference(members[i]));

            return dup;
        }

        public readonly Block DummyBody = new Block();

        public override Method VisitMethod(Method method)
        {
            if (method == null) return null;
            Method dup = (Method)this.DuplicateFor[method.UniqueKey];
            if (dup != null) return dup;
            this.DuplicateFor[method.UniqueKey] = dup = (Method)method.Clone();
            dup.ProviderHandle = null;
            if (TargetPlatform.UseGenerics)
            {
                this.FindTypesToBeDuplicated(method.TemplateParameters);
            }

            dup.LocalList = null;

            Method savedTarget = this.TargetMethod;
            this.TargetMethod = dup;
            if (TargetPlatform.UseGenerics)
            {
                dup.TemplateParameters = this.VisitTypeParameterList(method.TemplateParameters);
            }

            if (dup.Scope != null)
            {
                this.TypesToBeDuplicated[dup.Scope.UniqueKey] = dup.Scope;
                dup.Scope = this.VisitTypeNode(dup.Scope) as MethodScope;
            }

            dup.DeclaringMember = this.VisitMemberReference(dup.DeclaringMember);

            if (this.CopyDocumentation) dup.Documentation = method.Documentation;

            dup.ImplementedInterfaceMethods = this.VisitMethodReferenceList(method.ImplementedInterfaceMethods);
            dup.DeclaringType = this.TargetType;
            if (!method.IsAbstract) dup.Body = this.DummyBody;
            if (this.RecordOriginalAsTemplate)
            {
                if (method.Template != null)
                    dup.Template = method.Template;
                else
                    dup.Template = method;
            }
            dup.PInvokeModule = this.VisitModuleReference(dup.PInvokeModule);
            if (method.ReturnTypeMarshallingInformation != null)
                dup.ReturnTypeMarshallingInformation = method.ReturnTypeMarshallingInformation.Clone();
            dup.ThisParameter = (This)this.VisitParameter(dup.ThisParameter);
            dup = base.VisitMethod(dup);
            ////^ assume dup != null;
            dup.fullName = null;

            dup.DocumentationId = null;

            dup.ProviderHandle = method; // we always need the handle, as we may use it for attributes.
            dup.Attributes = null;
            dup.ProvideMethodAttributes = new Method.MethodAttributeProvider(this.ProvideMethodAttributes);
            if (!this.SkipBodies && !method.IsAbstract)
            {
                dup.Body = null;
                dup.ProvideBody = new Method.MethodBodyProvider(this.ProvideMethodBody);
            }

            if(this.SkipBodies)
                dup.Instructions = new InstructionList();

            this.TargetMethod = savedTarget;
            return dup;
        }
        public override Expression VisitMethodCall(MethodCall call)
        {
            if (call == null) return null;
            return base.VisitMethodCall((MethodCall)call.Clone());
        }

        public virtual MethodList VisitMethodList(MethodList methods)
        {
            if(methods == null)
                return null;

            int n = methods.Count;
            MethodList dup = new MethodList();

            for(int i = 0; i < n; i++)
                dup.Add(this.VisitMethod(methods[i]));

            return dup;
        }

        public virtual MethodList VisitMethodReferenceList(MethodList methods)
        {
            if(methods == null)
                return null;

            int n = methods.Count;
            MethodList dup = new MethodList();

            for(int i = 0; i < n; i++)
                dup.Add((Method)this.VisitMemberReference(methods[i]));

            return dup;
        }

        public override Module VisitModule(Module module)
        {
            if (module == null) return null;
            Module dup = (Module)module.Clone();
            if (this.TargetModule == null) this.TargetModule = dup;
            this.FindTypesToBeDuplicated(module.Types);
            return base.VisitModule(dup);
        }
        public virtual Module VisitModuleReference(Module module)
        {
            if (module == null) return null;
            Module dup = (Module)this.DuplicateFor[module.UniqueKey];
            if (dup != null) return dup;
            for (int i = 0, n = this.TargetModule.ModuleReferences == null ? 0 : this.TargetModule.ModuleReferences.Count; i < n; i++)
            {
                //^ assert this.TargetModule.ModuleReferences != null;
                ModuleReference modRef = this.TargetModule.ModuleReferences[i];
                if (modRef == null) continue;
                if (String.Compare(module.Name, modRef.Name, true, System.Globalization.CultureInfo.InvariantCulture) != 0) continue;
                this.DuplicateFor[module.UniqueKey] = modRef.Module; return modRef.Module;
            }
            if (this.TargetModule.ModuleReferences == null)
                this.TargetModule.ModuleReferences = new ModuleReferenceList();
            this.TargetModule.ModuleReferences.Add(new ModuleReference(module.Name, module));
            this.DuplicateFor[module.UniqueKey] = module;
            return module;
        }
        public override ModuleReference VisitModuleReference(ModuleReference moduleReference)
        {
            if (moduleReference == null) return null;
            return base.VisitModuleReference((ModuleReference)moduleReference.Clone());
        }

        public override Expression VisitNameBinding(NameBinding nameBinding)
        {
            if (nameBinding == null) return null;
            nameBinding = (NameBinding)nameBinding.Clone();
            nameBinding.BoundMember = this.VisitExpression(nameBinding.BoundMember);
            nameBinding.BoundMembers = this.VisitMemberReferenceList(nameBinding.BoundMembers);
            return nameBinding;
        }

        public override Expression VisitNamedArgument(NamedArgument namedArgument)
        {
            if (namedArgument == null) return null;
            return base.VisitNamedArgument((NamedArgument)namedArgument.Clone());
        }

        public override Namespace VisitNamespace(Namespace nspace)
        {
            if (nspace == null) return null;
            return base.VisitNamespace((Namespace)nspace.Clone());
        }

        public override NamespaceList VisitNamespaceList(NamespaceList namespaces)
        {
            if(namespaces == null)
                return null;

            return base.VisitNamespaceList(new NamespaceList(namespaces));
        }

        public override NodeList VisitNodeList(NodeList nodes)
        {
            if(nodes == null)
                return null;

            return base.VisitNodeList(new NodeList(nodes));
        }

        public override Expression VisitParameter(Parameter parameter)
        {
            if (parameter == null) return null;
            Parameter dup = (Parameter)this.DuplicateFor[parameter.UniqueKey];
            if (dup != null)
            {
                if (dup.DeclaringMethod == null) dup.DeclaringMethod = this.TargetMethod;
                return dup;
            }
            this.DuplicateFor[parameter.UniqueKey] = dup = (Parameter)parameter.Clone();
            if (dup.MarshallingInformation != null)
                dup.MarshallingInformation = dup.MarshallingInformation.Clone();
            dup.DeclaringMethod = this.TargetMethod;
            return base.VisitParameter(dup);
        }

        public override ParameterList VisitParameterList(ParameterList parameterList)
        {
            if(parameterList == null)
                return null;

            return base.VisitParameterList(new ParameterList(parameterList));
        }

        public override Expression VisitPrefixExpression(PrefixExpression pExpr)
        {
            if (pExpr == null) return null;
            return base.VisitPrefixExpression((PrefixExpression)pExpr.Clone());
        }
        public override Expression VisitPostfixExpression(PostfixExpression pExpr)
        {
            if (pExpr == null) return null;
            return base.VisitPostfixExpression((PostfixExpression)pExpr.Clone());
        }

        public override Property VisitProperty(Property property)
        {
            if (property == null) return null;
            Property dup = (Property)this.DuplicateFor[property.UniqueKey];
            if (dup != null) return dup;
            this.DuplicateFor[property.UniqueKey] = dup = (Property)property.Clone();
            dup.Attributes = this.VisitAttributeList(property.Attributes);

            if (this.CopyDocumentation) dup.Documentation = property.Documentation;

            dup.Type = this.VisitTypeReference(property.Type);
            dup.Getter = this.VisitMethod(property.Getter);
            dup.Setter = this.VisitMethod(property.Setter);
            dup.OtherMethods = this.VisitMethodList(property.OtherMethods);
            dup.DeclaringType = this.TargetType;
            dup.Parameters = this.VisitParameterList(dup.Parameters);
            return dup;
        }

        public override Expression VisitQuantifier(Quantifier quantifier)
        {
            if (quantifier == null) return null;
            return base.VisitQuantifier((Quantifier)quantifier.Clone());
        }
        public override Expression VisitComprehension(Comprehension Comprehension)
        {
            if (Comprehension == null) return null;
            return base.VisitComprehension((Comprehension)Comprehension.Clone());
        }
        public override ComprehensionBinding VisitComprehensionBinding(ComprehensionBinding comprehensionBinding)
        {
            if (comprehensionBinding == null) return null;
            return base.VisitComprehensionBinding((ComprehensionBinding)comprehensionBinding.Clone());
        }
        public override Expression VisitQualifiedIdentifier(QualifiedIdentifier qualifiedIdentifier)
        {
            if (qualifiedIdentifier == null) return null;
            return base.VisitQualifiedIdentifier((QualifiedIdentifier)qualifiedIdentifier.Clone());
        }
        public override Statement VisitRepeat(Repeat repeat)
        {
            if (repeat == null) return null;
            return base.VisitRepeat((Repeat)repeat.Clone());
        }

        public override Statement VisitReturn(Return Return)
        {
            if (Return == null) return null;
            return base.VisitReturn((Return)Return.Clone());
        }
        public override SecurityAttribute VisitSecurityAttribute(SecurityAttribute attribute)
        {
            if (attribute == null) return null;
            return base.VisitSecurityAttribute((SecurityAttribute)attribute.Clone()); ;
        }

        public override SecurityAttributeList VisitSecurityAttributeList(SecurityAttributeList attributes)
        {
            if(attributes == null)
                return null;

            return base.VisitSecurityAttributeList(new SecurityAttributeList(attributes));
        }

        public override Expression VisitSetterValue(SetterValue value)
        {
            if (value == null) return null;
            return base.VisitSetterValue((SetterValue)value.Clone());
        }

        public override StatementList VisitStatementList(StatementList statements)
        {
            if(statements == null)
                return null;

            return base.VisitStatementList(new StatementList(statements));
        }

        public override StatementSnippet VisitStatementSnippet(StatementSnippet snippet)
        {
            if (snippet == null) return null;
            return base.VisitStatementSnippet((StatementSnippet)snippet.Clone());
        }
        public override Statement VisitSwitch(Switch Switch)
        {
            if (Switch == null) return null;
            return base.VisitSwitch((Switch)Switch.Clone());
        }
        public override SwitchCase VisitSwitchCase(SwitchCase switchCase)
        {
            if (switchCase == null) return null;
            return base.VisitSwitchCase((SwitchCase)switchCase.Clone());
        }

        public override SwitchCaseList VisitSwitchCaseList(SwitchCaseList switchCases)
        {
            if(switchCases == null)
                return null;

            return base.VisitSwitchCaseList(new SwitchCaseList(switchCases));
        }

        public override Statement VisitSwitchInstruction(SwitchInstruction switchInstruction)
        {
            if (switchInstruction == null) return null;
            switchInstruction = (SwitchInstruction)base.VisitSwitchInstruction((SwitchInstruction)switchInstruction.Clone());
            if (switchInstruction == null) return null;
            switchInstruction.Targets = this.VisitBlockList(switchInstruction.Targets);
            return switchInstruction;
        }

        public override Statement VisitTypeswitch(Typeswitch Typeswitch)
        {
            if (Typeswitch == null) return null;
            return base.VisitTypeswitch((Typeswitch)Typeswitch.Clone());
        }
        public override TypeswitchCase VisitTypeswitchCase(TypeswitchCase typeswitchCase)
        {
            if (typeswitchCase == null) return null;
            return base.VisitTypeswitchCase((TypeswitchCase)typeswitchCase.Clone());
        }

        public override TypeswitchCaseList VisitTypeswitchCaseList(TypeswitchCaseList typeswitchCases)
        {
            if(typeswitchCases == null)
                return null;

            return base.VisitTypeswitchCaseList(new TypeswitchCaseList(typeswitchCases));
        }

        public override Expression VisitTernaryExpression(TernaryExpression expression)
        {
            if (expression == null) return null;
            return base.VisitTernaryExpression((TernaryExpression)expression.Clone());
        }
        public override Expression VisitThis(This This)
        {
            if (This == null) return null;
            This dup = (This)this.DuplicateFor[This.UniqueKey];
            if (dup != null) return dup;
            this.DuplicateFor[This.UniqueKey] = dup = (This)This.Clone();
            return base.VisitThis(dup);
        }
        public override Statement VisitThrow(Throw Throw)
        {
            if (Throw == null) return null;
            return base.VisitThrow((Throw)Throw.Clone());
        }

        public override Statement VisitTry(Try Try)
        {
            if (Try == null) return null;
            return base.VisitTry((Try)Try.Clone());
        }

        public override TypeModifier VisitTypeModifier(TypeModifier typeModifier)
        {
            if (typeModifier == null) return null;
            return base.VisitTypeModifier((TypeModifier)typeModifier.Clone());
        }

        public override TypeNode VisitTypeNode(TypeNode type)
        {
            if (type == null) return null;
            TypeNode dup = this.VisitTypeNode(type, null, null, null, true);
            ////^ assume dup != null;
            TypeNodeList nestedTypes = type.NestedTypes;
            if (nestedTypes != null && nestedTypes.Count > 0)
                this.VisitNestedTypes(dup, nestedTypes);
            return dup;
        }

        internal TypeNode VisitTypeNode(TypeNode type, Identifier mangledName, TypeNodeList templateArguments, TypeNode template, bool delayVisitToNestedTypes)
        {
            if (type == null) return null;
            TypeNode dup = (TypeNode)this.DuplicateFor[type.UniqueKey];
            if (dup != null) return dup;
            this.DuplicateFor[type.UniqueKey] = dup = (TypeNode)type.Clone();
            if (mangledName != null)
            {
                this.TargetModule.StructurallyEquivalentType[mangledName.UniqueIdKey] = dup;
                dup.TemplateArguments = templateArguments;
            }
            dup.arrayTypes = null;
            dup.constructors = null;
            dup.consolidatedTemplateArguments = null;
            dup.consolidatedTemplateParameters = null;
#if DEBUG
            dup.DebugLabel = null;
#endif

            dup.DocumentationId = null;
            if (this.CopyDocumentation) dup.Documentation = type.Documentation;

            dup.defaultMembers = null;

            dup.explicitCoercionFromTable = null;
            dup.explicitCoercionMethods = null;
            dup.implicitCoercionFromTable = null;
            dup.implicitCoercionMethods = null;
            dup.implicitCoercionToTable = null;

            dup.memberCount = 0;
            dup.memberTable = null;
            dup.modifierTable = null;
            dup.NestedTypes = null;
            dup.pointerType = null;
            dup.ProviderHandle = null;
            dup.ProvideTypeAttributes = null;
            dup.ProvideTypeMembers = null;
            dup.ProvideNestedTypes = null;
            dup.referenceType = null;

            dup.runtimeType = null;

            dup.structurallyEquivalentMethod = null;
            TypeParameter tp = dup as TypeParameter;
            if (tp != null) tp.structuralElementTypes = null;
            ClassParameter cp = dup as ClassParameter;
            if (cp != null) cp.structuralElementTypes = null;
            dup.szArrayTypes = null;
            if (this.RecordOriginalAsTemplate) dup.Template = type;
            dup.TemplateArguments = null;
            dup.TemplateInstances = null;
            dup.DeclaringModule = this.TargetModule;
            dup.DeclaringType = this.TargetType;
            TypeNode savedTargetType = this.TargetType;
            this.TargetType = dup;
            dup.Attributes = this.VisitAttributeList(type.Attributes);
            dup.SecurityAttributes = this.VisitSecurityAttributeList(type.SecurityAttributes);
            Class c = dup as Class;
            if (c != null) c.BaseClass = (Class)this.VisitTypeReference(c.BaseClass);
            dup.Interfaces = this.VisitInterfaceReferenceList(dup.Interfaces);
            dup.TemplateParameters = this.VisitTypeReferenceList(type.TemplateParameters);
            dup.consolidatedTemplateParameters = null;

            if (dup is MethodScope)
                dup.members = this.VisitMemberList(type.members);
            else

                if (!this.RecordOriginalAsTemplate)
                {
                    if (!delayVisitToNestedTypes)
                        dup.nestedTypes = this.VisitNestedTypes(dup, type.NestedTypes);
                    dup.members = null;
                    dup.ProvideTypeMembers = new TypeNode.TypeMemberProvider(this.ProvideTypeMembers);
                    dup.ProviderHandle = type;
                }
                else
                {
                    dup.members = null;
                    dup.ProvideNestedTypes = new TypeNode.NestedTypeProvider(this.ProvideNestedTypes);
                    dup.ProvideTypeMembers = new TypeNode.TypeMemberProvider(this.ProvideTypeMembers);
                    dup.ProviderHandle = type;
                }
            DelegateNode delegateNode = dup as DelegateNode;
            if (delegateNode != null)
            {

                if (!delegateNode.IsNormalized || !this.RecordOriginalAsTemplate)
                {
                    if (!delegateNode.IsNormalized)
                        ((DelegateNode)type).ProvideMembers();
                    delegateNode.Parameters = this.VisitParameterList(delegateNode.Parameters);
                    delegateNode.ReturnType = this.VisitTypeReference(delegateNode.ReturnType);
                }
                else
                {
                    delegateNode.Parameters = null;
                    delegateNode.ReturnType = null;
                }
            }
            dup.membersBeingPopulated = false;
            this.TargetType = savedTargetType;
            return dup;
        }
        private void ProvideNestedTypes(TypeNode/*!*/ dup, object/*!*/ handle)
        {
            TypeNode template = (TypeNode)handle;
            TypeNode savedTargetType = this.TargetType;
            Module savedTargetModule = this.TargetModule;
            this.TargetType = dup;
            //^ assume dup.DeclaringModule != null;
            this.TargetModule = dup.DeclaringModule;
            this.FindTypesToBeDuplicated(template.NestedTypes);
            dup.NestedTypes = this.VisitNestedTypes(dup, template.NestedTypes);
            this.TargetModule = savedTargetModule;
            this.TargetType = savedTargetType;
        }
        private void ProvideTypeMembers(TypeNode/*!*/ dup, object/*!*/ handle)
        {
            TypeNode template = (TypeNode)handle;
            Debug.Assert(!template.membersBeingPopulated);
            TypeNode savedTargetType = this.TargetType;
            Module savedTargetModule = this.TargetModule;
            this.TargetType = dup;
            //^ assume dup.DeclaringModule != null;
            this.TargetModule = dup.DeclaringModule;
            this.FindTypesToBeDuplicated(template.NestedTypes);
            dup.Members = this.VisitMemberList(template.Members);
            DelegateNode delegateNode = dup as DelegateNode;
            if (delegateNode != null && delegateNode.IsNormalized)
            {
                Debug.Assert(dup.Members != null && dup.Members.Count > 0 && dup.Members[0] != null);
                delegateNode.Parameters = this.VisitParameterList(delegateNode.Parameters);
                delegateNode.ReturnType = this.VisitTypeReference(delegateNode.ReturnType);
            }
            this.TargetModule = savedTargetModule;
            this.TargetType = savedTargetType;
        }
        protected virtual void ProvideMethodBody(Method/*!*/ dup, object/*!*/ handle, bool asInstructionList)
        {
            if (asInstructionList)
            {
                // We don't really have a way to provide instructions, but we set it to an empty list
                dup.Instructions = new InstructionList();
                return;
            }

            Method template = (Method)handle;
            Block tbody = template.Body;
            if (tbody == null)
            {
                dup.ProvideBody = null;
                return;
            }
            TypeNode savedTargetType = this.TargetType;
            this.TargetType = dup.DeclaringType;
            dup.Body = this.VisitBlock(tbody);

            dup.ExceptionHandlers = this.VisitExceptionHandlerList(template.ExceptionHandlers);

            this.TargetType = savedTargetType;
        }
        protected virtual void ProvideMethodAttributes(Method/*!*/ dup, object/*!*/ handle)
        {
            Method template = (Method)handle;
            AttributeList tattributes = template.Attributes;
            if (tattributes == null)
            {
                dup.ProvideMethodAttributes = null;
                return;
            }
            TypeNode savedTargetType = this.TargetType;
            this.TargetType = dup.DeclaringType;
            dup.Attributes = this.VisitAttributeList(tattributes);
            this.TargetType = savedTargetType;
        }

        public virtual TypeNodeList VisitNestedTypes(TypeNode/*!*/ declaringType, TypeNodeList types)
        {
            if(types == null)
                return null;

            TypeNodeList dupTypes = new TypeNodeList(types);

            for(int i = 0, n = types.Count; i < n; i++)
            {
                TypeNode nt = types[i];

                if(nt == null)
                    continue;

                TypeNode ntdup;

                if(TargetPlatform.UseGenerics)
                    ntdup = dupTypes[i] = this.VisitTypeNode(nt, null, null, null, true);
                else
                    ntdup = dupTypes[i] = this.VisitTypeReference(nt);

                if(ntdup != nt && ntdup != null)
                {
                    if(this.RecordOriginalAsTemplate)
                        ntdup.Template = nt;

                    ntdup.DeclaringType = declaringType;
                    ntdup.DeclaringModule = declaringType.DeclaringModule;
                }
            }

            for(int i = 0, n = types.Count; i < n; i++)
            {
                TypeNode nt = types[i];

                if(nt == null)
                    continue;

                TypeNodeList nestedTypes = nt.NestedTypes;
                
                if(nestedTypes == null || nestedTypes.Count == 0)
                    continue;

                TypeNode ntDup = dupTypes[i];

                if(ntDup == null)
                {
                    Debug.Fail("");
                    continue;
                }

                this.VisitNestedTypes(ntDup, nestedTypes);
            }

            return dupTypes;
        }

        public override TypeNodeList VisitTypeNodeList(TypeNodeList types)
        {
            if(types == null)
                return null;

            types = base.VisitTypeNodeList(new TypeNodeList(types));

            if(this.TargetModule == null)
                return types;

            if(types == null)
                return null;

            if(this.TargetModule.Types == null)
                this.TargetModule.Types = new TypeNodeList();

            for(int i = 0, n = types.Count; i < n; i++)
                this.TargetModule.Types.Add(types[i]);

            return types;
        }

        public override TypeNode VisitTypeParameter(TypeNode typeParameter)
        {
            if (typeParameter == null) return null;
            if (TargetPlatform.UseGenerics)
            {
                if (this.TypesToBeDuplicated[typeParameter.UniqueKey] != null)
                {
                    TypeParameter tp = typeParameter as TypeParameter;
                    if (tp != null)
                        tp.structuralElementTypes = this.VisitTypeReferenceList(tp.StructuralElementTypes);
                    else
                    {
                        ClassParameter cp = typeParameter as ClassParameter;
                        if (cp != null)
                            cp.structuralElementTypes = this.VisitTypeReferenceList(cp.StructuralElementTypes);
                    }
                    return this.VisitTypeNode(typeParameter);
                }
            }
            return base.VisitTypeParameter(typeParameter);
        }

        public override TypeNodeList VisitTypeParameterList(TypeNodeList typeParameters)
        {
            if(typeParameters == null)
                return null;

            return base.VisitTypeParameterList(new TypeNodeList(typeParameters));
        }

        // !EFW - Yes, static.  See related code below.
        private static int recursionCounter;

        public override TypeNode VisitTypeReference(TypeNode type)
        {
            if (type == null) return null;
            TypeNode dup = (TypeNode)this.DuplicateFor[type.UniqueKey];
            if (dup != null && (dup.Template != type || this.RecordOriginalAsTemplate)) return dup;
            switch (type.NodeType)
            {
                case NodeType.ArrayType:
                    ArrayType arrType = (ArrayType)type;
                    TypeNode elemType = this.VisitTypeReference(arrType.ElementType);
                    if (elemType == arrType.ElementType) return arrType;
                    if (elemType == null) { Debug.Fail(""); return null; }
                    this.TypesToBeDuplicated[arrType.UniqueKey] = arrType;
                    dup = elemType.GetArrayType(arrType.Rank, arrType.Sizes, arrType.LowerBounds);
                    break;
                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                    if (this.RecordOriginalAsTemplate) return type;
                    if (this.TypesToBeDuplicated[type.UniqueKey] == null) return type;
                    dup = this.VisitTypeNode(type);
                    break;

                case NodeType.DelegateNode:
                    {
                        FunctionType ftype = type as FunctionType;
                        if (ftype == null) goto default;
                        dup = FunctionType.For(this.VisitTypeReference(ftype.ReturnType), this.VisitParameterList(ftype.Parameters), this.TargetType);
                        break;
                    }

                case NodeType.Pointer:
                    Pointer pType = (Pointer)type;
                    elemType = this.VisitTypeReference(pType.ElementType);
                    if (elemType == pType.ElementType) return pType;
                    if (elemType == null) { Debug.Fail(""); return null; }
                    dup = elemType.GetPointerType();
                    break;
                case NodeType.Reference:
                    Reference rType = (Reference)type;
                    elemType = this.VisitTypeReference(rType.ElementType);
                    if (elemType == rType.ElementType) return rType;
                    if (elemType == null) { Debug.Fail(""); return null; }
                    dup = elemType.GetReferenceType();
                    break;

                //These types typically have only one reference and do not have pointer identity. Just duplicate them.
                case NodeType.ArrayTypeExpression:
                    ArrayTypeExpression aExpr = (ArrayTypeExpression)type.Clone();
                    elemType = this.VisitTypeReference(aExpr.ElementType);
                    if (elemType == null) { Debug.Fail(""); return aExpr; }
                    aExpr.ElementType = elemType;
                    return aExpr;
                case NodeType.BoxedTypeExpression:
                    BoxedTypeExpression bExpr = (BoxedTypeExpression)type.Clone();
                    bExpr.ElementType = this.VisitTypeReference(bExpr.ElementType);
                    return bExpr;
                case NodeType.ClassExpression:
                    ClassExpression cExpr = (ClassExpression)type.Clone();
                    cExpr.Expression = this.VisitExpression(cExpr.Expression);
                    cExpr.TemplateArguments = this.VisitTypeReferenceList(cExpr.TemplateArguments);
                    return cExpr;

                case NodeType.FlexArrayTypeExpression:
                    FlexArrayTypeExpression flExpr = (FlexArrayTypeExpression)type.Clone();
                    flExpr.ElementType = this.VisitTypeReference(flExpr.ElementType);
                    return flExpr;

                case NodeType.FunctionPointer:
                    FunctionPointer funcPointer = (FunctionPointer)type.Clone();
                    funcPointer.ParameterTypes = this.VisitTypeReferenceList(funcPointer.ParameterTypes);
                    funcPointer.ReturnType = this.VisitTypeReference(funcPointer.ReturnType);
                    return funcPointer;

                case NodeType.FunctionTypeExpression:
                    FunctionTypeExpression ftExpr = (FunctionTypeExpression)type.Clone();
                    ftExpr.Parameters = this.VisitParameterList(ftExpr.Parameters);
                    ftExpr.ReturnType = this.VisitTypeReference(ftExpr.ReturnType);
                    return ftExpr;
                case NodeType.InvariantTypeExpression:
                    InvariantTypeExpression invExpr = (InvariantTypeExpression)type.Clone();
                    invExpr.ElementType = this.VisitTypeReference(invExpr.ElementType);
                    return invExpr;

                case NodeType.InterfaceExpression:
                    InterfaceExpression iExpr = (InterfaceExpression)type.Clone();
                    iExpr.Expression = this.VisitExpression(iExpr.Expression);
                    iExpr.TemplateArguments = this.VisitTypeReferenceList(iExpr.TemplateArguments);
                    return iExpr;

                case NodeType.NonEmptyStreamTypeExpression:
                    NonEmptyStreamTypeExpression neExpr = (NonEmptyStreamTypeExpression)type.Clone();
                    neExpr.ElementType = this.VisitTypeReference(neExpr.ElementType);
                    return neExpr;
                case NodeType.NonNullTypeExpression:
                    NonNullTypeExpression nnExpr = (NonNullTypeExpression)type.Clone();
                    nnExpr.ElementType = this.VisitTypeReference(nnExpr.ElementType);
                    return nnExpr;
                case NodeType.NonNullableTypeExpression:
                    NonNullableTypeExpression nbExpr = (NonNullableTypeExpression)type.Clone();
                    nbExpr.ElementType = this.VisitTypeReference(nbExpr.ElementType);
                    return nbExpr;
                case NodeType.NullableTypeExpression:
                    NullableTypeExpression nuExpr = (NullableTypeExpression)type.Clone();
                    nuExpr.ElementType = this.VisitTypeReference(nuExpr.ElementType);
                    return nuExpr;

                case NodeType.OptionalModifier:
                    TypeModifier modType = (TypeModifier)type;
                    TypeNode modified = this.VisitTypeReference(modType.ModifiedType);
                    TypeNode modifier = this.VisitTypeReference(modType.Modifier);
                    if (modified == null || modifier == null) { Debug.Fail(""); return null; }
                    return OptionalModifier.For(modifier, modified);
                case NodeType.RequiredModifier:
                    modType = (TypeModifier)type;
                    modified = this.VisitTypeReference(modType.ModifiedType);
                    modifier = this.VisitTypeReference(modType.Modifier);
                    if (modified == null || modifier == null) { Debug.Fail(""); return null; }
                    return RequiredModifier.For(modifier, modified);

                case NodeType.OptionalModifierTypeExpression:
                    OptionalModifierTypeExpression optmodType = (OptionalModifierTypeExpression)type.Clone();
                    optmodType.ModifiedType = this.VisitTypeReference(optmodType.ModifiedType);
                    optmodType.Modifier = this.VisitTypeReference(optmodType.Modifier);
                    return optmodType;
                case NodeType.RequiredModifierTypeExpression:
                    RequiredModifierTypeExpression reqmodType = (RequiredModifierTypeExpression)type.Clone();
                    reqmodType.ModifiedType = this.VisitTypeReference(reqmodType.ModifiedType);
                    reqmodType.Modifier = this.VisitTypeReference(reqmodType.Modifier);
                    return reqmodType;
                case NodeType.PointerTypeExpression:
                    PointerTypeExpression pExpr = (PointerTypeExpression)type.Clone();
                    elemType = this.VisitTypeReference(pExpr.ElementType);
                    if (elemType == null) { Debug.Fail(""); return pExpr; }
                    pExpr.ElementType = elemType;
                    return pExpr;
                case NodeType.ReferenceTypeExpression:
                    ReferenceTypeExpression rExpr = (ReferenceTypeExpression)type.Clone();
                    elemType = this.VisitTypeReference(rExpr.ElementType);
                    if (elemType == null) { Debug.Fail(""); return rExpr; }
                    rExpr.ElementType = elemType;
                    return rExpr;
                case NodeType.StreamTypeExpression:
                    StreamTypeExpression sExpr = (StreamTypeExpression)type.Clone();
                    sExpr.ElementType = this.VisitTypeReference(sExpr.ElementType);
                    return sExpr;
                case NodeType.TupleTypeExpression:
                    TupleTypeExpression tuExpr = (TupleTypeExpression)type.Clone();
                    tuExpr.Domains = this.VisitFieldList(tuExpr.Domains);
                    return tuExpr;
                case NodeType.TypeExpression:
                    TypeExpression tExpr = (TypeExpression)type.Clone();
                    tExpr.Expression = this.VisitExpression(tExpr.Expression);
                    tExpr.TemplateArguments = this.VisitTypeReferenceList(tExpr.TemplateArguments);
                    return tExpr;
                case NodeType.TypeIntersectionExpression:
                    TypeIntersectionExpression tiExpr = (TypeIntersectionExpression)type.Clone();
                    tiExpr.Types = this.VisitTypeReferenceList(tiExpr.Types);
                    return tiExpr;
                case NodeType.TypeUnionExpression:
                    TypeUnionExpression tyuExpr = (TypeUnionExpression)type.Clone();
                    tyuExpr.Types = this.VisitTypeReferenceList(tyuExpr.Types);
                    return tyuExpr;

                default:
                    if (type.Template != null && type.Template != type && (type.TemplateArguments != null ||
                    (!this.RecordOriginalAsTemplate && type.ConsolidatedTemplateArguments != null && type.ConsolidatedTemplateArguments.Count > 0)))
                    {
                        TypeNode templ = this.VisitTypeReference(type.Template);

                        ////^ assume templ != null;
                        if (TargetPlatform.UseGenerics)
                        {
                            if (templ.Template != null)
                            {
                                if (this.RecordOriginalAsTemplate)
                                    templ = templ.Template;
                                else
                                    templ = this.VisitTypeReference(templ.Template);
                                ////^ assume templ != null;
                            }
                            if (type.DeclaringType != null)
                            {
                                TypeNode declType = this.VisitTypeReference(type.DeclaringType);

                                if (declType != null)
                                {
                                    TypeNode templDup = declType.GetNestedType(templ.Name);

                                    if (templDup == null)
                                    {
                                        // Can happen when template is nested in a type that is still being duplicated
                                        templDup = (TypeNode)templ.Clone();
                                        templDup.DeclaringModule = this.TargetModule;
                                        templDup.Template = templ;
                                        declType.NestedTypes.Add(templDup);
                                        templ = templDup;
                                    }
                                    else
                                    {
                                        templ = templDup;
                                        if (templ.Template != null)
                                        {
                                            if (this.RecordOriginalAsTemplate)
                                                templ = templ.Template;
                                            else
                                            {
                                                if (templ.Template.DeclaringType == null)
                                                    templ.Template.DeclaringType = templ.DeclaringType.Template;
                                                templ = this.VisitTypeReference(templ.Template);
                                            }
                                            ////^ assume templ != null;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (templ.Template != null) return type;
                        }

                        bool duplicateReference = templ != type.Template;

                        TypeNodeList targs = type.TemplateArguments == null ? new TypeNodeList() :
                            new TypeNodeList(type.TemplateArguments);

                        if(!this.RecordOriginalAsTemplate)
                            targs = type.ConsolidatedTemplateArguments == null ? new TypeNodeList() :
                                new TypeNodeList(type.ConsolidatedTemplateArguments);

                        for(int i = 0, n = targs == null ? 0 : targs.Count; i < n; i++)
                        {
                            TypeNode targ = targs[i];

                            if(targ == null)
                                continue;

                            // !EFW - Okay, this is a REALLY ugly hack but I couldn't think of any other way
                            // around it.  If a type contains a nested type that itself implements a nested
                            // type from within the containing type (still with me?), this gets into an
                            // endless loop and eventually overflows the stack.  As with the other case
                            // similar to this (Nodes.cs), I'm assuming 20 levels of recursion is enough of
                            // an indication that we've hit the problem.  In this case, if it hits the
                            // maximum level, we'll just return the current type.  This seems to work okay.
                            //
                            // The abbreviated example:
                            //
                            // public abstract partial class MyClass<TD, TT>
                            // {
                            //     public interface IBase
                            //     {
                            //         NestedClass<T> RequestElementsTyped<T>();
                            //     }
                            //
                            //     public class NestedClass<T> : IEquatable<NestedClass<T>>
                            //     {
                            //         public bool Equals(NestedClass<T> other)
                            //         {
                            //             throw new NotImplementedException();
                            //         }
                            //     }
                            //
                            //     public interface IAnother : IBase { }
                            //}
                            //
                            if(recursionCounter > 20)
                                return type;

                            recursionCounter++;

                            TypeNode targDup = this.VisitTypeReference(targ);

                            recursionCounter--;

                            if(targ != targDup)
                                duplicateReference = true;

                            targs[i] = targDup;
                        }

                        if(!duplicateReference)
                            return type;

                        if(!this.RecordOriginalAsTemplate)
                            dup = templ.GetGenericTemplateInstance(this.TargetModule, targs);
                        else
                            dup = templ.GetTemplateInstance(this.TargetModule, this.TargetType, type.DeclaringType, targs);

                        this.DuplicateFor[type.UniqueKey] = dup;
                        return dup;
                    }
                    if (this.TypesToBeDuplicated[type.UniqueKey] == null) return type;
                    TypeNode savedTargetType = this.TargetType;
                    TypeNode declaringType = this.VisitTypeReference(type.DeclaringType);
                    if (declaringType != null)
                    {
                        dup = (TypeNode)this.DuplicateFor[type.UniqueKey];
                        if (dup != null) return dup;
                        if (declaringType == type.DeclaringType)
                        {
                            //Trying to duplicate a nested type into a type that is not the duplicate of the declaring type.
                            //In this case, type is being duplicated into the original target type.
                            declaringType = this.OriginalTargetType;
                        }
                    }
                    this.TargetType = declaringType;
                    dup = (TypeNode)this.Visit(type);
                    this.TargetType = savedTargetType;
                    break;
            }
            this.DuplicateFor[type.UniqueKey] = dup;
            return dup;
        }

        public override TypeNodeList VisitTypeReferenceList(TypeNodeList typeReferences)
        {
            if(typeReferences == null)
                return null;

            return base.VisitTypeReferenceList(new TypeNodeList(typeReferences));
        }

        public override Expression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            if (unaryExpression == null) return null;
            unaryExpression = (UnaryExpression)base.VisitUnaryExpression((UnaryExpression)unaryExpression.Clone());
            return unaryExpression;
        }

        public override Statement VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            if (variableDeclaration == null) return null;
            return base.VisitVariableDeclaration((VariableDeclaration)variableDeclaration.Clone());
        }
        public override UsedNamespace VisitUsedNamespace(UsedNamespace usedNamespace)
        {
            if (usedNamespace == null) return null;
            return base.VisitUsedNamespace((UsedNamespace)usedNamespace.Clone());
        }

        public override UsedNamespaceList VisitUsedNamespaceList(UsedNamespaceList usedNspaces)
        {
            if(usedNspaces == null)
                return null;

            return base.VisitUsedNamespaceList(new UsedNamespaceList(usedNspaces));
        }

        public override Statement VisitWhile(While While)
        {
            if (While == null) return null;
            return base.VisitWhile((While)While.Clone());
        }

        public override Statement VisitYield(Yield Yield)
        {
            if (Yield == null) return null;
            return base.VisitYield((Yield)Yield.Clone());
        }
    }
}
