// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Ignore Spelling: dup unstacker ret

// Change history:
// 11/22/2013 - EFW - Cleared out the conditional statements and updated based on changes to ListTemplate.cs.

using System.Diagnostics;

namespace System.Compiler
{
    /// <summary>
    /// Walks a normalized IR, removing push, pop and dup instructions, replacing them with references to local variables.
    /// Requires all Blocks to be basic blocks. I.e. any transfer statement is always the last statement in a block.
    /// (This precondition is established by Reader but not by Normalizer.)
    /// </summary>
    public class Unstacker : StandardVisitor
    {
        private TrivialHashtable/*!*/ SucessorBlock = new();
        private TrivialHashtable/*!*/ StackLocalsAtEntry = new();
        private LocalsStack/*!*/ localsStack = new();

        public Unstacker()
        {
            //^ base();
        }

        public override Statement VisitAssignmentStatement(AssignmentStatement assignment)
        {
            if (assignment == null) return null;
            assignment.Source = this.VisitExpression(assignment.Source);
            assignment.Target = this.VisitTargetExpression(assignment.Target);
            return assignment;
        }
        public override Expression VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression == null) return null;
            binaryExpression.Operand2 = this.VisitExpression(binaryExpression.Operand2);
            binaryExpression.Operand1 = this.VisitExpression(binaryExpression.Operand1);
            if (binaryExpression.Type == null) binaryExpression.Type = binaryExpression.Operand1.Type; //Hack: need proper inferencing
            return binaryExpression;
        }
        public override Block VisitBlock(Block block)
        {
            if(block == null)
                return null;
            
            LocalsStack stackLocalsAtEntry = (LocalsStack)this.StackLocalsAtEntry[block.UniqueKey];
            
            // If null, unreachable code or the very first block
            stackLocalsAtEntry ??= new LocalsStack();
            
            this.localsStack = stackLocalsAtEntry.Clone();
            base.VisitBlock(block);
            Block successor = (Block)this.SucessorBlock[block.UniqueKey];
            if (successor != null)
            {
                //Dropping off into successor.
                LocalsStack targetStack = (LocalsStack)this.StackLocalsAtEntry[successor.UniqueKey];
                if (targetStack != null && targetStack.top >= 0)
                {
                    //Another predecessor block has already decided what the stack for the successor block is going to look like.
                    //Reconcile the stack from this block with the stack expected by the successor block.
                    this.localsStack.Transfer(targetStack, block.Statements);
                }
                else
                {
                    this.StackLocalsAtEntry[successor.UniqueKey] = this.localsStack;
                }
            }
            return block;
        }

        public override Statement VisitBranch(Branch branch)
        {
            if(branch == null)
                return null;

            if(branch.Target == null)
                return null;

            branch.Condition = this.VisitExpression(branch.Condition);

            int n = this.localsStack.top + 1;
            LocalsStack targetStack = (LocalsStack)this.StackLocalsAtEntry[branch.Target.UniqueKey];

            if(targetStack == null)
            {
                this.StackLocalsAtEntry[branch.Target.UniqueKey] = this.localsStack.Clone();
                return branch;
            }

            // Target block has an entry stack that is different from the current stack.  Need to copy stack
            // before branching.
            if(n <= 0)
                return branch; //Empty stack, no need to copy

            StatementList statements = [];

            this.localsStack.Transfer(targetStack, statements);
            statements.Add(branch);

            return new Block(statements);
        }

        public override Statement VisitSwitchInstruction(SwitchInstruction switchInstruction)
        {
            if (switchInstruction == null) return null;
            switchInstruction.Expression = this.VisitExpression(switchInstruction.Expression);
            for (int i = 0, n = switchInstruction.Targets == null ? 0 : switchInstruction.Targets.Count; i < n; i++)
            {
                Block target = switchInstruction.Targets[i];
                if (target == null) continue;
                this.StackLocalsAtEntry[target.UniqueKey] = this.localsStack.Clone();
            }
            return switchInstruction;
        }
        public override ExpressionList VisitExpressionList(ExpressionList expressions)
        {
            if (expressions == null) return null;
            for (int i = expressions.Count - 1; i >= 0; i--)
                expressions[i] = this.VisitExpression(expressions[i]);
            return expressions;
        }
        public override Statement VisitExpressionStatement(ExpressionStatement statement)
        {
            if (statement == null) return null;
            Expression e = statement.Expression = this.VisitExpression(statement.Expression);
            if (e == null || e.Type == CoreSystemTypes.Void) return statement;
            if (e.NodeType == NodeType.Dup) return this.localsStack.Dup();
            return this.localsStack.Push(e);
        }
        public override Expression VisitExpression(Expression expression)
        {
            if (expression == null) return null;
            switch (expression.NodeType)
            {
                case NodeType.Dup:
                case NodeType.Arglist:
                    return expression;
                case NodeType.Pop:
                    if(expression is UnaryExpression uex)
                    {
                        Expression e = uex.Operand = this.VisitExpression(uex.Operand);
                        if(e == null)
                            return null;
                        uex.Type = CoreSystemTypes.Void;
                        return uex;
                    }
                    return this.localsStack.Pop();
                default:
                    return (Expression)this.Visit(expression);
            }
        }
        public override Expression VisitIndexer(Indexer indexer)
        {
            if (indexer == null) return null;
            indexer.Operands = this.VisitExpressionList(indexer.Operands);
            indexer.Object = this.VisitExpression(indexer.Object);
            return indexer;
        }
        public override Method VisitMethod(Method method)
        {
            if(method == null)
                return null;

            // body might not have been materialized, so make sure we do that first!
            Block body = method.Body;

            if (body == null)
                return null;

            BlockSorter blockSorter = new();
            BlockList sortedBlocks = blockSorter.SortedBlocks;
            this.SucessorBlock = blockSorter.SuccessorBlock;
            this.StackLocalsAtEntry = new TrivialHashtable();
            this.localsStack = new LocalsStack();
            ExceptionHandlerList ehandlers = method.ExceptionHandlers;
            for (int i = 0, n = ehandlers == null ? 0 : ehandlers.Count; i < n; i++)
            {
                ExceptionHandler ehandler = ehandlers[i];
                if (ehandler == null) continue;
                Block handlerStart = ehandler.HandlerStartBlock;
                if (handlerStart == null) continue;
                LocalsStack lstack = new();
                this.StackLocalsAtEntry[handlerStart.UniqueKey] = lstack;
                if (ehandler.HandlerType == NodeType.Catch)
                {
                    lstack.exceptionHandlerType = CoreSystemTypes.Object;
                    if (ehandler.FilterType != null) lstack.exceptionHandlerType = ehandler.FilterType;
                }
                else if (ehandler.HandlerType == NodeType.Filter)
                {
                    lstack.exceptionHandlerType = CoreSystemTypes.Object;
                    if (ehandler.FilterExpression != null)
                    {
                        lstack = new LocalsStack
                        {
                            exceptionHandlerType = CoreSystemTypes.Object
                        };

                        this.StackLocalsAtEntry[ehandler.FilterExpression.UniqueKey] = lstack;
                    }
                }
            }
            blockSorter.VisitMethodBody(body);
            for (int i = 0, n = sortedBlocks.Count; i < n; i++)
            {
                Block b = sortedBlocks[i];
                if (b == null) { Debug.Assert(false); continue; }
                this.VisitBlock(b);
            }
            return method;
        }
        public override Expression VisitMethodCall(MethodCall call)
        {
            if (call == null) return null;
            call.Operands = this.VisitExpressionList(call.Operands);
            call.Callee = this.VisitExpression(call.Callee);
            return call;
        }
        public override Expression VisitTernaryExpression(TernaryExpression expression)
        {
            if (expression == null) return null;
            expression.Operand3 = this.VisitExpression(expression.Operand3);
            expression.Operand2 = this.VisitExpression(expression.Operand2);
            expression.Operand1 = this.VisitExpression(expression.Operand1);
            return expression;
        }

        private class LocalsStack
        {
            private Local[]/*!*/ elements;
            internal int top = -1;
            internal TypeNode exceptionHandlerType;

            private void Grow()
            {
                int n = this.elements.Length;
                Local[] newElements = new Local[n + 8];
                for (int i = 0; i < n; i++) newElements[i] = this.elements[i];
                this.elements = newElements;
            }
            internal LocalsStack()
            {
                this.elements = new Local[8];
                //^ base();
            }
            private LocalsStack(LocalsStack/*!*/ other)
            {
                this.top = other.top;
                this.exceptionHandlerType = other.exceptionHandlerType;
                Local[] otherElements = other.elements;
                int n = otherElements.Length;
                Local[] elements = this.elements = new Local[n];
                //^ base();
                n = this.top + 1;
                for (int i = 0; i < n; i++)
                    elements[i] = otherElements[i];
            }
            internal LocalsStack/*!*/ Clone()
            {
                return new LocalsStack(this);
            }
            internal AssignmentStatement Dup()
            {
                int i = this.top;
                Expression topVal;
                if (this.top == -1 && this.exceptionHandlerType != null)
                {
                    topVal = new Expression(NodeType.Dup, this.exceptionHandlerType);
                }
                else
                {
                    Debug.Assert(i >= 0 && i < this.elements.Length);
                    topVal = this.elements[i];
                    //^ assume topVal != null;
                }
                Local dup = new(topVal.Type);
                if ((i = ++this.top) >= this.elements.Length) this.Grow();
                this.elements[i] = dup;
                return new AssignmentStatement(dup, topVal);
            }
            internal AssignmentStatement Push(Expression/*!*/ expr)
            {
                ////Debug.Assert(expr != null && expr.Type != null);
                int i = ++this.top;
                Debug.Assert(i >= 0);
                if (i >= this.elements.Length) this.Grow();
                Local loc = this.elements[i];
                if (loc == null || loc.Type != expr.Type)
                    this.elements[i] = loc = new Local(expr.Type);
                return new AssignmentStatement(loc, expr);
            }
            internal Expression Pop()
            {
                if (this.top == -1 && this.exceptionHandlerType != null)
                {
                    TypeNode t = this.exceptionHandlerType;
                    this.exceptionHandlerType = null;
                    return new Expression(NodeType.Pop, t);
                }
                int i = this.top--;
                Debug.Assert(i >= 0 && i < this.elements.Length);
                return this.elements[i];
            }
            internal void Transfer(LocalsStack/*!*/ targetStack, StatementList/*!*/ statements)
            {
                Debug.Assert(targetStack != null);
                if (targetStack == this) return;
                int n = this.top;
                Debug.Assert(n == targetStack.top);
                for (int i = 0; i <= n; i++)
                {
                    Local sloc = this.elements[i];
                    Local tloc = targetStack.elements[i];
                    if (sloc == tloc) continue;
                    Debug.Assert(sloc != null && tloc != null);
                    statements.Add(new AssignmentStatement(tloc, sloc));
                }
            }
        }

        private class BlockSorter : StandardVisitor
        {
            private readonly TrivialHashtable/*!*/ VisitedBlocks = new();
            private readonly TrivialHashtable/*!*/ BlocksThatDropThrough = new();
            private bool lastBranchWasUnconditional;
            internal BlockList/*!*/ SortedBlocks = [];
            internal TrivialHashtable/*!*/ SuccessorBlock = new();

            internal BlockSorter()
            {
                //^ base();
            }

            internal void VisitMethodBody(Block body)
            {
                if (body == null) return;
                StatementList statements = body.Statements;
                Block previousBlock = null;
                for (int i = 0, n = statements.Count; i < n; i++)
                {
                    if(statements[i] is not Block b)
                    {
                        Debug.Assert(false);
                        continue;
                    }

                    if (previousBlock != null && this.BlocksThatDropThrough[previousBlock.UniqueKey] != null)
                    {
                        this.SuccessorBlock[previousBlock.UniqueKey] = b;
                    }
                    this.VisitBlock(b);
                    previousBlock = b;
                }
            }
            public override Block VisitBlock(Block block)
            {
                if (block == null) return null;
                if (this.VisitedBlocks[block.UniqueKey] != null)
                {
                    return block;
                }
                this.VisitedBlocks[block.UniqueKey] = block;
                this.SortedBlocks.Add(block);
                this.lastBranchWasUnconditional = false;
                base.VisitBlock(block);
                if (!this.lastBranchWasUnconditional)
                    this.BlocksThatDropThrough[block.UniqueKey] = block;
                return block;
            }
            public override Statement VisitBranch(Branch branch)
            {
                if (branch == null) return null;
                if (branch.Target == null) return null;
                this.VisitBlock(branch.Target);
                this.lastBranchWasUnconditional = branch.Condition == null;
                return branch;
            }
            public override Statement VisitReturn(Return ret)
            {
                this.lastBranchWasUnconditional = true;
                return ret;
            }
            public override Statement VisitSwitchInstruction(SwitchInstruction switchInstruction)
            {
                if (switchInstruction == null) return null;
                switchInstruction.Expression = this.VisitExpression(switchInstruction.Expression);
                for (int i = 0, n = switchInstruction.Targets == null ? 0 : switchInstruction.Targets.Count; i < n; i++)
                {
                    Block target = switchInstruction.Targets[i];
                    if (target == null) continue;
                    this.VisitBlock(target);
                }
                return switchInstruction;
            }
            public override Statement VisitThrow(Throw Throw)
            {
                this.lastBranchWasUnconditional = true;
                return Throw;
            }
        }
    }
}
