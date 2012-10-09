// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

#if !FxCop
using System;

#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler
{
#endif
#if !MinimalReader
    public sealed class AliasDefinitionList
    {
        private AliasDefinition[]/*!*/ elements;
        private int count = 0;
        public AliasDefinitionList()
        {
            this.elements = new AliasDefinition[4];
            //^ base();
        }
        public AliasDefinitionList(int capacity)
        {
            this.elements = new AliasDefinition[capacity];
            //^ base();
        }
        public void Add(AliasDefinition element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                AliasDefinition[] newElements = new AliasDefinition[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public AliasDefinitionList/*!*/ Clone()
        {
            AliasDefinition[] elements = this.elements;
            int n = this.count;
            AliasDefinitionList result = new AliasDefinitionList(n);
            result.count = n;
            AliasDefinition[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public AliasDefinition this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly AliasDefinitionList/*!*/ list;
            public Enumerator(AliasDefinitionList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public AliasDefinition Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class AssemblyNodeList
    {
        private AssemblyNode[]/*!*/ elements;
        private int count = 0;
        public AssemblyNodeList()
        {
            this.elements = new AssemblyNode[4];
            //^ base();
        }
        public AssemblyNodeList(int capacity)
        {
            this.elements = new AssemblyNode[capacity];
            //^ base();
        }
        public AssemblyNodeList(params AssemblyNode[] elements)
        {
            if (elements == null) elements = new AssemblyNode[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(AssemblyNode element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                AssemblyNode[] newElements = new AssemblyNode[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public AssemblyNode this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly AssemblyNodeList/*!*/ list;
            public Enumerator(AssemblyNodeList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public AssemblyNode Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
    public sealed class AssemblyReferenceList
    {
        private AssemblyReference[]/*!*/ elements;
        private int count = 0;
        public AssemblyReferenceList()
        {
            this.elements = new AssemblyReference[4];
            //^ base();
        }
        public AssemblyReferenceList(int capacity)
        {
            this.elements = new AssemblyReference[capacity];
            //^ base();
        }
        public void Add(AssemblyReference element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                AssemblyReference[] newElements = new AssemblyReference[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public AssemblyReferenceList/*!*/ Clone()
        {
            AssemblyReference[] elements = this.elements;
            int n = this.count;
            AssemblyReferenceList result = new AssemblyReferenceList(n);
            result.count = n;
            AssemblyReference[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public AssemblyReference this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly AssemblyReferenceList/*!*/ list;
            public Enumerator(AssemblyReferenceList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public AssemblyReference Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class AttributeList
    {
        private AttributeNode[]/*!*/ elements;
        private int count = 0;
        public AttributeList()
        {
            this.elements = new AttributeNode[8];
            //^ base();
        }
        public AttributeList(int capacity)
        {
            this.elements = new AttributeNode[capacity];
            //^ base();
        }
        public AttributeList(params AttributeNode[] elements)
        {
            if (elements == null) elements = new AttributeNode[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(AttributeNode element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                AttributeNode[] newElements = new AttributeNode[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public AttributeList/*!*/ Clone()
        {
            AttributeNode[] elements = this.elements;
            int n = this.count;
            AttributeList result = new AttributeList(n);
            result.count = n;
            AttributeNode[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public AttributeNode this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly AttributeList/*!*/ list;
            public Enumerator(AttributeList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public AttributeNode Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class BlockList
    {
        private Block[]/*!*/ elements;
        private int count = 0;
        public BlockList()
        {
            this.elements = new Block[4];
            //^ base();
        }
        public BlockList(int n)
        {
            this.elements = new Block[n];
            //^ base();
        }
        public void Add(Block element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                Block[] newElements = new Block[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public BlockList/*!*/ Clone()
        {
            Block[] elements = this.elements;
            int n = this.count;
            BlockList result = new BlockList(n);
            result.count = n;
            Block[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Block this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly BlockList/*!*/ list;
            public Enumerator(BlockList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Block Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#if !MinimalReader
    public sealed class CatchList
    {
        private Catch[]/*!*/ elements;
        private int count = 0;
        public CatchList()
        {
            this.elements = new Catch[4];
            //^ base();
        }
        public CatchList(int n)
        {
            this.elements = new Catch[n];
            //^ base();
        }
        public void Add(Catch element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                Catch[] newElements = new Catch[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public CatchList/*!*/ Clone()
        {
            Catch[] elements = this.elements;
            int n = this.count;
            CatchList result = new CatchList(n);
            result.count = n;
            Catch[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Catch this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly CatchList/*!*/ list;
            public Enumerator(CatchList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Catch Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class CompilationList
    {
        private Compilation[]/*!*/ elements;
        private int count = 0;
        public CompilationList()
        {
            this.elements = new Compilation[4];
            //^ base();
        }
        public CompilationList(int n)
        {
            this.elements = new Compilation[n];
            //^ base();
        }
        public CompilationList(params Compilation[] elements)
        {
            if (elements == null) elements = new Compilation[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(Compilation element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                Compilation[] newElements = new Compilation[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public CompilationList/*!*/ Clone()
        {
            Compilation[] elements = this.elements;
            int n = this.count;
            CompilationList result = new CompilationList(n);
            result.count = n;
            Compilation[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Compilation this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly CompilationList/*!*/ list;
            public Enumerator(CompilationList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Compilation Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class CompilationUnitList
    {
        private CompilationUnit[]/*!*/ elements;
        private int count = 0;
        public CompilationUnitList()
        {
            this.elements = new CompilationUnit[4];
            //^ base();
        }
        public CompilationUnitList(int n)
        {
            this.elements = new CompilationUnit[n];
            //^ base();
        }
        public CompilationUnitList(params CompilationUnit[] elements)
        {
            if (elements == null) elements = new CompilationUnit[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(CompilationUnit element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                CompilationUnit[] newElements = new CompilationUnit[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public CompilationUnitList/*!*/ Clone()
        {
            CompilationUnit[] elements = this.elements;
            int n = this.count;
            CompilationUnitList result = new CompilationUnitList(n);
            result.count = n;
            CompilationUnit[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public CompilationUnit this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly CompilationUnitList/*!*/ list;
            public Enumerator(CompilationUnitList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public CompilationUnit Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class CompilationUnitSnippetList
    {
        private CompilationUnitSnippet[]/*!*/ elements;
        private int count = 0;
        public CompilationUnitSnippetList()
        {
            this.elements = new CompilationUnitSnippet[4];
            //^ base();
        }
        public CompilationUnitSnippetList(int n)
        {
            this.elements = new CompilationUnitSnippet[n];
            //^ base();
        }
        public CompilationUnitSnippetList(params CompilationUnitSnippet[] elements)
        {
            if (elements == null) elements = new CompilationUnitSnippet[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(CompilationUnitSnippet element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                CompilationUnitSnippet[] newElements = new CompilationUnitSnippet[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public CompilationUnitSnippetList/*!*/ Clone()
        {
            CompilationUnitSnippet[] elements = this.elements;
            int n = this.count;
            CompilationUnitSnippetList result = new CompilationUnitSnippetList(n);
            result.count = n;
            CompilationUnitSnippet[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public CompilationUnitSnippet this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly CompilationUnitSnippetList/*!*/ list;
            public Enumerator(CompilationUnitSnippetList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public CompilationUnitSnippet Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
#if !NoWriter
    public sealed class EventList
    {
        private Event[]/*!*/ elements;
        private int count = 0;
        public EventList()
        {
            this.elements = new Event[8];
            //^ base();
        }
        public EventList(int n)
        {
            this.elements = new Event[n];
            //^ base();
        }
        public void Add(Event element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                Event[] newElements = new Event[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Event this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly EventList/*!*/ list;
            public Enumerator(EventList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Event Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
#if !MinimalReader
    public sealed class ErrorNodeList
    {
        private ErrorNode[]/*!*/ elements;
        private int count = 0;
        public ErrorNodeList()
        {
            this.elements = new ErrorNode[8];
            //^ base();
        }
        public ErrorNodeList(int n)
        {
            this.elements = new ErrorNode[n];
            //^ base();
        }
        public void Add(ErrorNode element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                ErrorNode[] newElements = new ErrorNode[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public ErrorNode this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly ErrorNodeList/*!*/ list;
            public Enumerator(ErrorNodeList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public ErrorNode Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
    public sealed class ExpressionList
    {
        private Expression[]/*!*/ elements;
        private int count = 0;
        public ExpressionList()
        {
            this.elements = new Expression[8];
            //^ base();
        }
        public ExpressionList(int n)
        {
            this.elements = new Expression[n];
            //^ base();
        }
        public ExpressionList(params Expression[] elements)
        {
            if (elements == null) elements = new Expression[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(Expression element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                Expression[] newElements = new Expression[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public ExpressionList/*!*/ Clone()
        {
            Expression[] elements = this.elements;
            int n = this.count;
            ExpressionList result = new ExpressionList(n);
            result.count = n;
            Expression[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
            set { this.count = value; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
            set { this.count = value; }
        }
        public Expression this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly ExpressionList/*!*/ list;
            public Enumerator(ExpressionList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Expression Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class ExceptionHandlerList
    {
        private ExceptionHandler[]/*!*/ elements = new ExceptionHandler[4];
        private int count = 0;
        public ExceptionHandlerList()
        {
            //^ base();
        }
        public ExceptionHandlerList(int n)
        {
            this.elements = new ExceptionHandler[n];
            //^ base();
        }
        public void Add(ExceptionHandler element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                ExceptionHandler[] newElements = new ExceptionHandler[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public ExceptionHandlerList/*!*/ Clone()
        {
            ExceptionHandler[] elements = this.elements;
            int n = this.count;
            ExceptionHandlerList result = new ExceptionHandlerList(n);
            result.count = n;
            ExceptionHandler[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public ExceptionHandler this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly ExceptionHandlerList/*!*/ list;
            public Enumerator(ExceptionHandlerList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public ExceptionHandler Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#if !MinimalReader
    public sealed class FaultHandlerList
    {
        private FaultHandler[]/*!*/ elements;
        private int count = 0;
        public FaultHandlerList()
        {
            this.elements = new FaultHandler[4];
            //^ base();
        }
        public FaultHandlerList(int n)
        {
            this.elements = new FaultHandler[n];
            //^ base();
        }
        public void Add(FaultHandler element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                FaultHandler[] newElements = new FaultHandler[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public FaultHandlerList/*!*/ Clone()
        {
            FaultHandler[] elements = this.elements;
            int n = this.count;
            FaultHandlerList result = new FaultHandlerList(n);
            result.count = n;
            FaultHandler[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public FaultHandler this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly FaultHandlerList/*!*/ list;
            public Enumerator(FaultHandlerList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public FaultHandler Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
#if !NoWriter || !MinimalReader
    public sealed class FieldList
    {
        private Field[]/*!*/ elements;
        private int count = 0;
        public FieldList()
        {
            this.elements = new Field[8];
            //^ base();
        }
        public FieldList(int capacity)
        {
            this.elements = new Field[capacity];
            //^ base();
        }
        public FieldList(params Field[] elements)
        {
            if (elements == null) elements = new Field[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(Field element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                Field[] newElements = new Field[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public FieldList/*!*/ Clone()
        {
            Field[] elements = this.elements;
            int n = this.count;
            FieldList result = new FieldList(n);
            result.count = n;
            Field[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Field this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly FieldList/*!*/ list;
            public Enumerator(FieldList /*!*/list)
            {
                this.index = -1;
                this.list = list;
            }
            public Field Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
#if !MinimalReader
    public sealed class FilterList
    {
        private Filter[]/*!*/ elements;
        private int count = 0;
        public FilterList()
        {
            this.elements = new Filter[4];
            //^ base();
        }
        public FilterList(int capacity)
        {
            this.elements = new Filter[capacity];
            //^ base();
        }
        public void Add(Filter element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                Filter[] newElements = new Filter[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public FilterList/*!*/ Clone()
        {
            Filter[] elements = this.elements;
            int n = this.count;
            FilterList result = new FilterList(n);
            result.count = n;
            Filter[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Filter this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly FilterList/*!*/ list;
            public Enumerator(FilterList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Filter Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class IdentifierList
    {
        private Identifier[]/*!*/ elements;
        private int count = 0;
        public IdentifierList()
        {
            this.elements = new Identifier[8];
            //^ base();
        }
        public IdentifierList(int capacity)
        {
            this.elements = new Identifier[capacity];
            //^ base();
        }
        public void Add(Identifier element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                Identifier[] newElements = new Identifier[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
            set { this.count = value; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
            set { this.count = value; }
        }
        public Identifier this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly IdentifierList/*!*/ list;
            public Enumerator(IdentifierList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Identifier Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
    public sealed class InstructionList
    {
        private Instruction[]/*!*/ elements;
        private int count = 0;
        public InstructionList()
        {
            this.elements = new Instruction[32];
            //^ base();
        }
        public InstructionList(int capacity)
        {
            this.elements = new Instruction[capacity];
            //^ base();
        }
        public void Add(Instruction element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 32) m = 32;
                Instruction[] newElements = new Instruction[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
            set { this.count = value; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
            set { this.count = value; }
        }
        public Instruction this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly InstructionList/*!*/ list;
            public Enumerator(InstructionList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Instruction Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class InterfaceList
    {
        private Interface[]/*!*/ elements;
        private int count = 0;
        public InterfaceList()
        {
            this.elements = new Interface[8];
            //^ base();
        }
        public InterfaceList(int capacity)
        {
            this.elements = new Interface[capacity];
            //^ base();
        }
        public InterfaceList(params Interface[] elements)
        {
            if (elements == null) elements = new Interface[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(Interface element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                Interface[] newElements = new Interface[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public InterfaceList/*!*/ Clone()
        {
            Interface[] elements = this.elements;
            int n = this.count;
            InterfaceList result = new InterfaceList(n);
            result.count = n;
            Interface[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
            set { this.count = value; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
            set { this.count = value; }
        }
        public int SearchFor(Interface element)
        {
            Interface[] elements = this.elements;
            for (int i = 0, n = this.count; i < n; i++)
                if ((object)elements[i] == (object)element) return i;
            return -1;
        }
        public Interface this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly InterfaceList/*!*/ list;
            public Enumerator(InterfaceList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Interface Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#if ExtendedRuntime
  public sealed class InvariantList{
    private Invariant[]/*!*/ elements;
    private int count = 0;
    public InvariantList(){
      this.elements = new Invariant[8];
      //^ base();
    }
    public InvariantList(int n){
      this.elements = new Invariant[n];
      //^ base();
    }
    public InvariantList(params Invariant[] elements){
      if (elements == null) elements = new Invariant[0];
      this.elements = elements;
      this.count = elements.Length;
      //^ base();
    }
    public void Add(Invariant element){
      int n = this.elements.Length;
      int i = this.count++;
      if (i == n){
        int m = n*2; if (m < 8) m = 8;
        Invariant[] newElements = new Invariant[m];
        for (int j = 0; j < n; j++) newElements[j] = elements[j];
        this.elements = newElements;
      }
      this.elements[i] = element;
    }
    public InvariantList/*!*/ Clone() {
      Invariant[] elements = this.elements;
      int n = this.count;
      InvariantList result = new InvariantList(n);
      result.count = n;
      Invariant[] newElements = result.elements;
      for (int i = 0; i < n; i++)
        newElements[i] = elements[i];
      return result;
    }
    public int Count{
      get{return this.count;}
      set{this.count = value;}
    }
    [Obsolete("Use Count property instead.")]
    public int Length{
      get{return this.count;}
      set{this.count = value;}
    }
    public Invariant this[int index]{
      get{
        return this.elements[index];
      }
      set{
        this.elements[index] = value;
      }
    }
    public Enumerator GetEnumerator(){
      return new Enumerator(this);
    }
    public struct Enumerator{
      private int index;
      private readonly InvariantList/*!*/ list;
      public Enumerator(InvariantList/*!*/ list) {
        this.index = -1;
        this.list = list;
      }
      public Invariant Current{
        get{
          return this.list[this.index];
        }
      }
      public bool MoveNext(){
        return ++this.index < this.list.count;
      }
      public void Reset(){
        this.index = -1;
      }
    }
  }
#endif
    public sealed class Int32List
    {
        private Int32[]/*!*/ elements;
        private int count = 0;
        public Int32List()
        {
            this.elements = new Int32[8];
            //^ base();
        }
        public Int32List(int capacity)
        {
            this.elements = new Int32[capacity];
            //^ base();
        }
        public Int32List(params Int32[] elements)
        {
            if (elements == null) elements = new Int32[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(Int32 element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                Int32[] newElements = new Int32[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Int32 this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly Int32List/*!*/ list;
            public Enumerator(Int32List/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Int32 Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#if !MinimalReader
    public sealed class ISourceTextList
    {
        private ISourceText[]/*!*/ elements = new ISourceText[4];
        private int count = 0;
        public ISourceTextList()
        {
            this.elements = new ISourceText[4];
            //^ base();
        }
        public ISourceTextList(int capacity)
        {
            this.elements = new ISourceText[capacity];
            //^ base();
        }
        public ISourceTextList(params ISourceText[] elements)
        {
            if (elements == null) elements = new ISourceText[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(ISourceText element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                ISourceText[] newElements = new ISourceText[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public ISourceText this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly ISourceTextList/*!*/ list;
            public Enumerator(ISourceTextList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public ISourceText Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class LocalDeclarationList
    {
        private LocalDeclaration[]/*!*/ elements;
        private int count = 0;
        public LocalDeclarationList()
        {
            this.elements = new LocalDeclaration[8];
            //^ base();
        }
        public LocalDeclarationList(int capacity)
        {
            this.elements = new LocalDeclaration[capacity];
            //^ base();
        }
        public void Add(LocalDeclaration element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                LocalDeclaration[] newElements = new LocalDeclaration[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public LocalDeclarationList/*!*/ Clone()
        {
            LocalDeclaration[] elements = this.elements;
            int n = this.count;
            LocalDeclarationList result = new LocalDeclarationList(n);
            result.count = n;
            LocalDeclaration[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public LocalDeclaration this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly LocalDeclarationList/*!*/ list;
            public Enumerator(LocalDeclarationList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public LocalDeclaration Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
#if ExtendedRuntime  
  public sealed class RequiresList{
    private Requires[]/*!*/ elements;
    private int count = 0;
    public RequiresList(){
      this.elements = new Requires[8];
      //^ base();
    }
    public RequiresList(int capacity){
      this.elements = new Requires[capacity];
      //^ base();
    }
    public void Add(Requires element){
      int n = this.elements.Length;
      int i = this.count++;
      if (i == n){
        int m = n*2; if (m < 8) m = 8;
        Requires[] newElements = new Requires[m];
        for (int j = 0; j < n; j++) newElements[j] = elements[j];
        this.elements = newElements;
      }
      this.elements[i] = element;
    }
    public RequiresList/*!*/ Clone() {
      Requires[] elements = this.elements;
      int n = this.count;
      RequiresList result = new RequiresList(n);
      result.count = n;
      Requires[] newElements = result.elements;
      for (int i = 0; i < n; i++)
        newElements[i] = elements[i];
      return result;
    }
    public int Count{
      get{return this.count;}
    }
    [Obsolete("Use Count property instead.")]
    public int Length{
      get{return this.count;}
    }
    public Requires this[int index]{
      get{
        return this.elements[index];
      }
      set{
        this.elements[index] = value;
      }
    }
    public Enumerator GetEnumerator(){
      return new Enumerator(this);
    }
    public struct Enumerator{
      private int index;
      private readonly RequiresList/*!*/ list;
      public Enumerator(RequiresList/*!*/ list) {
        this.index = -1;
        this.list = list;
      }
      public Requires Current{
        get{
          return this.list[this.index];
        }
      }
      public bool MoveNext(){
        return ++this.index < this.list.count;
      }
      public void Reset(){
        this.index = -1;
      }
    }
  }
  public sealed class EnsuresList{
    private Ensures[]/*!*/ elements;
    private int count = 0;
    public EnsuresList(){
      this.elements = new Ensures[8];
      //^ base();
    }
    public EnsuresList(int capacity){
      this.elements = new Ensures[capacity];
      //^ base();
    }
    public void Add(Ensures element){
      int n = this.elements.Length;
      int i = this.count++;
      if (i == n){
        int m = n*2; if (m < 8) m = 8;
        Ensures[] newElements = new Ensures[m];
        for (int j = 0; j < n; j++) newElements[j] = elements[j];
        this.elements = newElements;
      }
      this.elements[i] = element;
    }
    public EnsuresList/*!*/ Clone() {
      Ensures[] elements = this.elements;
      int n = this.count;
      EnsuresList result = new EnsuresList(n);
      result.count = n;
      Ensures[] newElements = result.elements;
      for (int i = 0; i < n; i++)
        newElements[i] = elements[i];
      return result;
    }
    public int Count{
      get{return this.count;}
    }
    [Obsolete("Use Count property instead.")]
    public int Length{
      get{return this.count;}
    }
    public Ensures this[int index]{
      get{
        return this.elements[index];
      }
      set{
        this.elements[index] = value;
      }
    }
    public Enumerator GetEnumerator(){
      return new Enumerator(this);
    }
    public struct Enumerator{
      private int index;
      private readonly EnsuresList/*!*/ list;
      public Enumerator(EnsuresList/*!*/ list) {
        this.index = -1;
        this.list = list;
      }
      public Ensures Current{
        get{
          return this.list[this.index];
        }
      }
      public bool MoveNext(){
        return ++this.index < this.list.count;
      }
      public void Reset(){
        this.index = -1;
      }
    }
  }    
#endif
    public sealed class LocalList
    {
        private Local[]/*!*/ elements;
        private int count = 0;
        public LocalList()
        {
            this.elements = new Local[8];
            //^ base();
        }
        public LocalList(int capacity)
        {
            this.elements = new Local[capacity];
            //^ base();
        }
        public void Add(Local element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                Local[] newElements = new Local[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Local this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public struct Enumerator
        {
            private int index;
            private readonly LocalList/*!*/ list;
            public Enumerator(LocalList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Local Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class MemberList
    {
        private Member[]/*!*/ elements;
        private int count = 0;
        public MemberList()
        {
            this.elements = new Member[16];
            //^ base();
        }
        public MemberList(int capacity)
        {
            this.elements = new Member[capacity];
            //^ base();
        }
        public MemberList(params Member[] elements)
        {
            if (elements == null) elements = new Member[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(Member element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 16) m = 16;
                Member[] newElements = new Member[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
#if !MinimalReader
        public bool Contains(Member element)
        {
            int n = this.count;
            for (int i = 0; i < n; i++)
                if (elements[i] == element)
                    return true;
            return false;
        }
        public void AddList(MemberList memberList)
        {
            if (memberList == null || memberList.Count == 0) return;
            int n = this.elements.Length;
            int newN = this.count + memberList.count;
            if (newN > n)
            {
                int m = newN; if (m < 16) m = 16;
                Member[] newElements = new Member[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            for (int i = this.count, j = 0; i < newN; ++i, ++j)
            {
                this.elements[i] = memberList.elements[j];
            }
            this.count = newN;
        }
        /// <summary>
        /// Removes member (by nulling slot) if present
        /// </summary>
        public void Remove(Member member)
        {
            int n = this.count;
            for (int i = 0; i < n; i++)
            {
                if (this.elements[i] == member)
                {
                    this.elements[i] = null;
                    return;
                }
            }
        }
#endif
        public void RemoveAt(int index)
        {
            if (index >= this.count || index < 0) return;
            int n = this.count;
            for (int i = index + 1; i < n; ++i)
                this.elements[i - 1] = this.elements[i];
            this.count--;
        }
        public MemberList/*!*/ Clone()
        {
            Member[] elements = this.elements;
            int n = this.count;
            MemberList result = new MemberList(n);
            result.count = n;
            Member[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Member this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly MemberList/*!*/ list;
            public Enumerator(MemberList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Member Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
        public Member[]/*!*/ ToArray()
        {
            Member[] m = new Member[this.count];
            Array.Copy(this.elements, m, this.count);
            return m;
        }
    }
#if !MinimalReader
    public sealed class MemberBindingList
    {
        private MemberBinding[]/*!*/ elements;
        private int count = 0;
        public MemberBindingList()
        {
            this.elements = new MemberBinding[8];
            //^ base();
        }
        public MemberBindingList(int capacity)
        {
            this.elements = new MemberBinding[capacity];
            //^ base();
        }
        public void Add(MemberBinding element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                MemberBinding[] newElements = new MemberBinding[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public MemberBinding this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly MemberBindingList/*!*/ list;
            public Enumerator(MemberBindingList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public MemberBinding Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
    public sealed class MethodList
    {
        private Method[]/*!*/ elements;
        private int count = 0;
        public MethodList()
        {
            this.elements = new Method[8];
            //^ base();
        }
        public MethodList(int capacity)
        {
            this.elements = new Method[capacity];
            //^ base();
        }
        public MethodList(params Method[] elements)
        {
            if (elements == null) elements = new Method[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(Method element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                Method[] newElements = new Method[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public MethodList/*!*/ Clone()
        {
            Method[] elements = this.elements;
            int n = this.count;
            MethodList result = new MethodList(n);
            result.count = n;
            Method[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Method this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly MethodList/*!*/ list;
            public Enumerator(MethodList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Method Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#if !NoWriter
    public sealed class ModuleList
    {
        private Module[]/*!*/ elements;
        private int count = 0;
        public ModuleList()
        {
            this.elements = new Module[4];
            //^ base();
        }
        public ModuleList(int capacity)
        {
            this.elements = new Module[capacity];
            //^ base();
        }
        public void Add(Module element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                Module[] newElements = new Module[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Module this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly ModuleList/*!*/ list;
            public Enumerator(ModuleList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Module Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
    public sealed class ModuleReferenceList
    {
        private ModuleReference[]/*!*/ elements;
        private int count = 0;
        public ModuleReferenceList()
        {
            this.elements = new ModuleReference[4];
            //^ base();
        }
        public ModuleReferenceList(int capacity)
        {
            this.elements = new ModuleReference[capacity];
            //^ base();
        }
        public ModuleReferenceList(params ModuleReference[] elements)
        {
            if (elements == null) elements = new ModuleReference[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(ModuleReference element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                ModuleReference[] newElements = new ModuleReference[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public ModuleReferenceList/*!*/ Clone()
        {
            ModuleReference[] elements = this.elements;
            int n = this.count;
            ModuleReferenceList result = new ModuleReferenceList(n);
            result.count = n;
            ModuleReference[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public ModuleReference this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly ModuleReferenceList/*!*/ list;
            public Enumerator(ModuleReferenceList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public ModuleReference Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class NamespaceList
    {
        private Namespace[]/*!*/ elements;
        private int count = 0;
        public NamespaceList()
        {
            this.elements = new Namespace[4];
            //^ base();
        }
        public NamespaceList(int capacity)
        {
            this.elements = new Namespace[capacity];
            //^ base();
        }
        public void Add(Namespace element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                Namespace[] newElements = new Namespace[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public NamespaceList/*!*/ Clone()
        {
            Namespace[] elements = this.elements;
            int n = this.count;
            NamespaceList result = new NamespaceList(n);
            result.count = n;
            Namespace[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Namespace this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly NamespaceList/*!*/ list;
            public Enumerator(NamespaceList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Namespace Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#if !FxCop
    public
#endif
 sealed class NodeList
    {
        private Node[]/*!*/ elements;
        private int count = 0;
        public NodeList()
        {
            this.elements = new Node[4];
            //^ base();
        }
        public NodeList(int capacity)
        {
            this.elements = new Node[capacity];
            //^ base();
        }
        public NodeList(params Node[] elements)
        {
            if (elements == null) elements = new Node[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(Node element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                Node[] newElements = new Node[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public NodeList/*!*/ Clone()
        {
            Node[] elements = this.elements;
            int n = this.count;
            NodeList result = new NodeList(n);
            result.count = n;
            Node[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Node this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly NodeList/*!*/ list;
            public Enumerator(NodeList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Node Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class ParameterList
    {
        public readonly static ParameterList/*!*/ Empty = new ParameterList(0);

        private Parameter[]/*!*/ elements;
        private int count = 0;
        public ParameterList()
        {
            this.elements = new Parameter[8];
            //^ base();
        }
        public ParameterList(int capacity)
        {
            this.elements = new Parameter[capacity];
            //^ base();
        }
        public ParameterList(params Parameter[] elements)
        {
            if (elements == null) elements = new Parameter[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(Parameter element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                Parameter[] newElements = new Parameter[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public ParameterList/*!*/ Clone()
        {
            Parameter[] elements = this.elements;
            int n = this.count;
            ParameterList result = new ParameterList(n);
            result.count = n;
            Parameter[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
            set { this.count = value; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
            set { this.count = value; }
        }
        public Parameter this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly ParameterList/*!*/ list;
            public Enumerator(ParameterList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Parameter Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < this.count; i++)
            {
                if (i > 0) res += ",";
                Parameter par = elements[i];
                if (par == null) continue;
                res += par.ToString();
            }
            return res;
        }
    }
#if !NoWriter
    public sealed class PropertyList
    {
        private Property[]/*!*/ elements;
        private int count = 0;
        public PropertyList()
        {
            this.elements = new Property[8];
            //^ base();
        }
        public PropertyList(int capacity)
        {
            this.elements = new Property[capacity];
            //^ base();
        }
        public void Add(Property element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                Property[] newElements = new Property[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Property this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly PropertyList/*!*/ list;
            public Enumerator(PropertyList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Property Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
    public sealed class ResourceList
    {
        private Resource[]/*!*/ elements;
        private int count = 0;
        public ResourceList()
        {
            this.elements = new Resource[4];
            //^ base();
        }
        public ResourceList(int capacity)
        {
            this.elements = new Resource[capacity];
            //^ base();
        }
        public void Add(Resource element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                Resource[] newElements = new Resource[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public ResourceList/*!*/ Clone()
        {
            Resource[] elements = this.elements;
            int n = this.count;
            ResourceList result = new ResourceList(n);
            result.count = n;
            Resource[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Resource this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly ResourceList/*!*/ list;
            public Enumerator(ResourceList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Resource Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#if !MinimalReader
    public sealed class ScopeList
    {
        private Scope[]/*!*/ elements;
        private int count = 0;
        public ScopeList()
        {
            this.elements = new Scope[32];
            //^ base();
        }
        public ScopeList(int capacity)
        {
            this.elements = new Scope[capacity];
            //^ base();
        }
        public ScopeList(params Scope[] elements)
        {
            if (elements == null) elements = new Scope[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(Scope element)
        {
            Scope[] elements = this.elements;
            int n = elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 32) m = 32;
                Scope[] newElements = new Scope[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public ScopeList/*!*/ Clone()
        {
            Scope[] elements = this.elements;
            int n = this.count;
            ScopeList result = new ScopeList(n);
            result.count = n;
            Scope[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public void Insert(Scope element, int index)
        {
            Scope[] elements = this.elements;
            int n = this.elements.Length;
            int i = this.count++;
            if (index >= i) throw new IndexOutOfRangeException();
            if (i == n)
            {
                int m = n * 2; if (m < 32) m = 32;
                Scope[] newElements = new Scope[m];
                for (int j = 0; j < index; j++) newElements[j] = elements[j];
                newElements[index] = element;
                for (int j = index; j < n; j++) newElements[j + 1] = elements[j];
                return;
            }
            for (int j = index; j < i; j++)
            {
                Scope t = elements[j];
                elements[j] = element;
                element = t;
            }
            elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public int SearchFor(Scope element)
        {
            Scope[] elements = this.elements;
            for (int i = 0, n = this.count; i < n; i++)
                if ((object)elements[i] == (object)element) return i;
            return -1;
        }
        public Scope this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly ScopeList/*!*/ list;
            public Enumerator(ScopeList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Scope Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
    public sealed class SecurityAttributeList
    {
        private SecurityAttribute[]/*!*/ elements;
        private int count = 0;
        public SecurityAttributeList()
        {
            this.elements = new SecurityAttribute[8];
            //^ base();
        }
        public SecurityAttributeList(int capacity)
        {
            this.elements = new SecurityAttribute[capacity];
            //^ base();
        }
        public void Add(SecurityAttribute element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 8) m = 8;
                SecurityAttribute[] newElements = new SecurityAttribute[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public SecurityAttributeList/*!*/ Clone()
        {
            SecurityAttribute[] elements = this.elements;
            int n = this.count;
            SecurityAttributeList result = new SecurityAttributeList(n);
            result.count = n;
            SecurityAttribute[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public SecurityAttribute this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly SecurityAttributeList/*!*/ list;
            public Enumerator(SecurityAttributeList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public SecurityAttribute Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#if !MinimalReader
    public sealed class SourceChangeList
    {
        private SourceChange[]/*!*/ elements;
        private int count = 0;
        public SourceChangeList()
        {
            this.elements = new SourceChange[4];
            //^ base();
        }
        public SourceChangeList(int capacity)
        {
            this.elements = new SourceChange[capacity];
            //^ base();
        }
        public SourceChangeList(params SourceChange[] elements)
        {
            if (elements == null) elements = new SourceChange[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(SourceChange element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                SourceChange[] newElements = new SourceChange[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public SourceChangeList/*!*/ Clone()
        {
            SourceChange[] elements = this.elements;
            int n = this.count;
            SourceChangeList result = new SourceChangeList(n);
            result.count = n;
            SourceChange[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public SourceChange this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly SourceChangeList/*!*/ list;
            public Enumerator(SourceChangeList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public SourceChange Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
    public sealed class StatementList
    {
        private Statement[]/*!*/ elements;
        private int count = 0;
        public StatementList()
        {
            this.elements = new Statement[32];
            //^ base();
        }
        public StatementList(int capacity)
        {
            this.elements = new Statement[capacity];
            //^ base();
        }
        public StatementList(params Statement[] elements)
        {
            if (elements == null) elements = new Statement[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(Statement statement)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 32) m = 32;
                Statement[] newElements = new Statement[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = statement;
        }
        public StatementList/*!*/ Clone()
        {
            Statement[] elements = this.elements;
            int n = this.count;
            StatementList result = new StatementList(n);
            result.count = n;
            Statement[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
            set { this.count = value; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
            set { this.count = value; }
        }
        public Statement this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly StatementList/*!*/ list;
            public Enumerator(StatementList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Statement Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#if !NoWriter
    public sealed class StringList
    {
        private string[]/*!*/ elements = new string[4];
        private int count = 0;
        public StringList()
        {
            this.elements = new string[4];
            //^ base();
        }
        public StringList(int capacity)
        {
            this.elements = new string[capacity];
            //^ base();
        }
        public StringList(params string[] elements)
        {
            if (elements == null) elements = new string[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public StringList(System.Collections.Specialized.StringCollection/*!*/ stringCollection)
        {
            int n = this.count = stringCollection == null ? 0 : stringCollection.Count;
            string[] elements = this.elements = new string[n];
            //^ base();
            if (n > 0) stringCollection.CopyTo(elements, 0);
        }
        public void Add(string element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                String[] newElements = new String[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public string this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly StringList/*!*/ list;
            public Enumerator(StringList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public String Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
#if !MinimalReader
    public sealed class SwitchCaseList
    {
        private SwitchCase[]/*!*/ elements = new SwitchCase[16];
        private int count = 0;
        public SwitchCaseList()
        {
            this.elements = new SwitchCase[16];
            //^ base();
        }
        public SwitchCaseList(int capacity)
        {
            this.elements = new SwitchCase[capacity];
            //^ base();
        }
        public void Add(SwitchCase element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 16) m = 16;
                SwitchCase[] newElements = new SwitchCase[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public SwitchCaseList/*!*/ Clone()
        {
            SwitchCase[] elements = this.elements;
            int n = this.count;
            SwitchCaseList result = new SwitchCaseList(n);
            result.count = n;
            SwitchCase[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public SwitchCase this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly SwitchCaseList/*!*/ list;
            public Enumerator(SwitchCaseList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public SwitchCase Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
    public sealed class TypeNodeList
    {
        private TypeNode[]/*!*/ elements;
        private int count = 0;
        public TypeNodeList()
        {
            this.elements = new TypeNode[32];
            //^ base();
        }
        public TypeNodeList(int capacity)
        {
            this.elements = new TypeNode[capacity];
            //^ base();
        }
        public TypeNodeList(params TypeNode[] elements)
        {
            if (elements == null) elements = new TypeNode[0];
            this.elements = elements;
            this.count = elements.Length;
            //^ base();
        }
        public void Add(TypeNode element)
        {
            TypeNode[] elements = this.elements;
            int n = elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 32) m = 32;
                TypeNode[] newElements = new TypeNode[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public TypeNodeList/*!*/ Clone()
        {
            TypeNode[] elements = this.elements;
            int n = this.count;
            TypeNodeList result = new TypeNodeList(n);
            result.count = n;
            TypeNode[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public void Insert(TypeNode element, int index)
        {
            TypeNode[] elements = this.elements;
            int n = this.elements.Length;
            int i = this.count++;
            if (index >= i) throw new IndexOutOfRangeException();
            if (i == n)
            {
                int m = n * 2; if (m < 32) m = 32;
                TypeNode[] newElements = new TypeNode[m];
                for (int j = 0; j < index; j++) newElements[j] = elements[j];
                newElements[index] = element;
                for (int j = index; j < n; j++) newElements[j + 1] = elements[j];
                return;
            }
            for (int j = index; j < i; j++)
            {
                TypeNode t = elements[j];
                elements[j] = element;
                element = t;
            }
            elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public int SearchFor(TypeNode element)
        {
            TypeNode[] elements = this.elements;
            for (int i = 0, n = this.count; i < n; i++)
                if ((object)elements[i] == (object)element) return i;
            return -1;
        }
        public TypeNode this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly TypeNodeList/*!*/ list;
            public Enumerator(TypeNodeList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public TypeNode Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#if !MinimalReader
    public sealed class TypeswitchCaseList
    {
        private TypeswitchCase[]/*!*/ elements = new TypeswitchCase[16];
        private int count = 0;
        public TypeswitchCaseList()
        {
            this.elements = new TypeswitchCase[16];
            //^ base();
        }
        public TypeswitchCaseList(int capacity)
        {
            this.elements = new TypeswitchCase[capacity];
            //^ base();
        }
        public void Add(TypeswitchCase element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 16) m = 16;
                TypeswitchCase[] newElements = new TypeswitchCase[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public TypeswitchCaseList/*!*/ Clone()
        {
            TypeswitchCase[] elements = this.elements;
            int n = this.count;
            TypeswitchCaseList result = new TypeswitchCaseList(n);
            result.count = n;
            TypeswitchCase[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public TypeswitchCase this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly TypeswitchCaseList/*!*/ list;
            public Enumerator(TypeswitchCaseList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public TypeswitchCase Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
    public sealed class UsedNamespaceList
    {
        private UsedNamespace[]/*!*/ elements;
        private int count = 0;
        public UsedNamespaceList()
        {
            this.elements = new UsedNamespace[4];
            //^ base();
        }
        public UsedNamespaceList(int capacity)
        {
            this.elements = new UsedNamespace[capacity];
            //^ base();
        }
        public void Add(UsedNamespace element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                UsedNamespace[] newElements = new UsedNamespace[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public UsedNamespaceList/*!*/ Clone()
        {
            UsedNamespace[] elements = this.elements;
            int n = this.count;
            UsedNamespaceList result = new UsedNamespaceList(n);
            result.count = n;
            UsedNamespace[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public UsedNamespace this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly UsedNamespaceList/*!*/ list;
            public Enumerator(UsedNamespaceList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public UsedNamespace Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }

    public sealed class VariableDeclarationList
    {
        private VariableDeclaration[]/*!*/ elements;
        private int count = 0;
        public VariableDeclarationList()
        {
            this.elements = new VariableDeclaration[4];
            //^ base();
        }
        public VariableDeclarationList(int capacity)
        {
            this.elements = new VariableDeclaration[capacity];
            //^ base();
        }
        public void Add(VariableDeclaration element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                VariableDeclaration[] newElements = new VariableDeclaration[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public VariableDeclaration this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly VariableDeclarationList/*!*/ list;
            public Enumerator(VariableDeclarationList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public VariableDeclaration Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
#endif
    public sealed class Win32ResourceList
    {
        private Win32Resource[]/*!*/ elements;
        private int count = 0;
        public Win32ResourceList()
        {
            this.elements = new Win32Resource[4];
            //^ base();
        }
        public Win32ResourceList(int capacity)
        {
            this.elements = new Win32Resource[capacity];
            //^ base();
        }
        public void Add(Win32Resource element)
        {
            int n = this.elements.Length;
            int i = this.count++;
            if (i == n)
            {
                int m = n * 2; if (m < 4) m = 4;
                Win32Resource[] newElements = new Win32Resource[m];
                for (int j = 0; j < n; j++) newElements[j] = elements[j];
                this.elements = newElements;
            }
            this.elements[i] = element;
        }
        public Win32ResourceList/*!*/ Clone()
        {
            Win32Resource[] elements = this.elements;
            int n = this.count;
            Win32ResourceList result = new Win32ResourceList(n);
            result.count = n;
            Win32Resource[] newElements = result.elements;
            for (int i = 0; i < n; i++)
                newElements[i] = elements[i];
            return result;
        }
        public int Count
        {
            get { return this.count; }
        }
        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get { return this.count; }
        }
        public Win32Resource this[int index]
        {
            get
            {
                return this.elements[index];
            }
            set
            {
                this.elements[index] = value;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        public struct Enumerator
        {
            private int index;
            private readonly Win32ResourceList/*!*/ list;
            public Enumerator(Win32ResourceList/*!*/ list)
            {
                this.index = -1;
                this.list = list;
            }
            public Win32Resource Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
            public bool MoveNext()
            {
                return ++this.index < this.list.count;
            }
            public void Reset()
            {
                this.index = -1;
            }
        }
    }
}
#else
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Cci
{
  public abstract class MetadataCollection<T> : IList<T>, IList{
    private List<T> innerList;
    internal MetadataCollection() : this(new List<T>()){}
    internal MetadataCollection(int capacity) : this(new List<T>(capacity)){}
    internal MetadataCollection(List<T> innerList){
      this.innerList = innerList;
    }
    internal MetadataCollection(ICollection<T> collection) : this(collection == null ? 0 : collection.Count){
      if (collection != null){
        this.innerList.AddRange(collection);
      }
    }
    public T this[int index]{
      get { return this.innerList[index]; }
      internal set { this.innerList[index] = value; }
    }
    public int IndexOf(T item){
      return this.innerList.IndexOf(item);
    }
    public bool Contains(T item){
      return this.innerList.Contains(item);
    }
    public void CopyTo(T[] array, int arrayIndex){
      this.innerList.CopyTo(array, arrayIndex);
    }
    public int Count{
      get { return this.innerList.Count; }
    }
    public bool IsReadOnly{
      get { return true; }
    }
    public Enumerator GetEnumerator(){
      return new Enumerator(this.innerList.GetEnumerator());
    }
    public struct Enumerator{
      private List<T>.Enumerator enumerator;
      public Enumerator(List<T>.Enumerator enumerator){
        this.enumerator = enumerator;
      }
      public bool MoveNext(){
        return this.enumerator.MoveNext();
      }
      public T Current{
        get { return this.enumerator.Current; }
      }
    }
    internal void Add(T item){
      this.innerList.Add(item);
    }
    void ICollection<T>.Add(T item){
      throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly);
    }
    void ICollection<T>.Clear(){
      throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly);
    }
    bool ICollection<T>.Remove(T item){
      throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly);
    }
    IEnumerator<T> IEnumerable<T>.GetEnumerator(){
      return this.innerList.GetEnumerator();
    }
    void IList<T>.Insert(int index, T item){
      throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly);
    }
    void IList<T>.RemoveAt(int index){
      throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly);
    }
    T IList<T>.this[int index]{
      get { return this[index]; }
      set { throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly); }
    }
    void ICollection.CopyTo(Array array, int index){
      ICollection list = this.innerList;
      list.CopyTo(array, index);
    }
    bool ICollection.IsSynchronized{
      get { return false; }
    }
    object ICollection.SyncRoot{
      get {
        ICollection list = this.innerList;
        return list.SyncRoot; 
      }
    }
    IEnumerator IEnumerable.GetEnumerator(){
      IEnumerable list = this.innerList;
      return list.GetEnumerator();
    }
    int IList.Add(object value){
      throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly);
    }
    bool IList.Contains(object value){
      IList list = this.innerList;
      return list.Contains(value);
    }
    int IList.IndexOf(object value){
      IList list = this.innerList;
      return list.IndexOf(value);
    }
    void IList.Insert(int index, object value){
      throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly);
    }
    bool IList.IsFixedSize{
      get { return true; }
    }
    void IList.Remove(object value){
      throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly);
    }
    void IList.RemoveAt(int index){
      throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly);
    }
    object IList.this[int index]{
      get { return this[index]; }
      set { throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly); }
    }
    void IList.Clear(){
      throw new NotSupportedException(ExceptionStrings.CollectionIsReadOnly);
    }
  }
  public sealed class AssemblyNodeCollection : MetadataCollection<AssemblyNode> {
    internal AssemblyNodeCollection() : base(){}
    internal AssemblyNodeCollection(int capacity) : base(capacity) { }
    internal AssemblyNodeCollection(ICollection<AssemblyNode> collection) : base(collection) { }
    internal AssemblyNodeCollection Clone() {
      return new AssemblyNodeCollection(this);
    }
  }
  public sealed class AssemblyReferenceCollection : MetadataCollection<AssemblyReference> {
    internal AssemblyReferenceCollection() : base(){}
    internal AssemblyReferenceCollection(int capacity) : base(capacity){}
  }
  public sealed class AttributeNodeCollection : MetadataCollection<AttributeNode> {
    internal AttributeNodeCollection() : base(){}
    internal AttributeNodeCollection(int capacity) : base(capacity){}
    internal AttributeNodeCollection(ICollection<AttributeNode> collection) : base(collection) { }
    internal AttributeNodeCollection Clone() {
      return new AttributeNodeCollection(this);
    }
  }
  public sealed class BlockCollection : MetadataCollection<Block> {
    internal BlockCollection() : base(){}
    internal BlockCollection(int capacity) : base(capacity) { }
    internal BlockCollection(ICollection<Block> collection) : base(collection) { }
    internal BlockCollection Clone() {
      return new BlockCollection(this);
    }
  }
  public sealed class CatchNodeCollection : MetadataCollection<CatchNode> {
    internal CatchNodeCollection() : base(){}
    internal CatchNodeCollection(int capacity) : base(capacity) { }
  }
  public sealed class ExpressionCollection : MetadataCollection<Expression> {
    internal ExpressionCollection() : base(){}
    internal ExpressionCollection(int capacity) : base(capacity){}
    internal ExpressionCollection(ICollection<Expression> collection) : base(collection) { }
    internal ExpressionCollection Clone() {
      return new ExpressionCollection(this);
    }
  }
  public sealed class InstructionCollection : MetadataCollection<Instruction> {
    internal InstructionCollection() : base(){}
    internal InstructionCollection(int capacity) : base(capacity){}
  }
  public sealed class InterfaceCollection : MetadataCollection<InterfaceNode> {
    internal InterfaceCollection() : base(){}
    internal InterfaceCollection(int capacity) : base(capacity){}
    internal InterfaceCollection(params InterfaceNode[] range) : base(range) {}   
    internal InterfaceCollection(ICollection<InterfaceNode> collection) : base(collection) { }
    internal InterfaceCollection Clone() {
      return new InterfaceCollection(this);
    }
  }
  public sealed class LocalCollection : MetadataCollection<Local> {
    internal LocalCollection() : base(){}
    internal LocalCollection(int capacity) : base(capacity){}
  }
  public sealed class MemberCollection : MetadataCollection<Member> {
    internal MemberCollection() : base(){}
    internal MemberCollection(int capacity) : base(capacity){}
    internal MemberCollection(ICollection<Member> collection) : base(collection) { }
    internal MemberCollection Clone() {
      return new MemberCollection(this);
    }
  }
  public sealed class MethodCollection : MetadataCollection<Method> {
    internal MethodCollection() : base(){}
    internal MethodCollection(int capacity) : base(capacity){}
    internal MethodCollection(params Method[] range) : base(range) {}
  }
  public sealed class ModuleReferenceCollection : MetadataCollection<ModuleReference> {
    internal ModuleReferenceCollection() : base(){}
    internal ModuleReferenceCollection(int capacity) : base(capacity){}
  }
  public sealed class NamespaceCollection : MetadataCollection<Namespace> {
    internal NamespaceCollection() : base(){}
    internal NamespaceCollection(int capacity) : base(capacity){}
  }
  public sealed class NodeCollection : MetadataCollection<Node> {
    internal NodeCollection() : base(){}
    internal NodeCollection(int capacity) : base(capacity){}
    internal NodeCollection(ICollection<Node> collection) : base(collection) { }
    internal NodeCollection Clone() {
      return new NodeCollection(this);
    }
  }
  public sealed class ParameterCollection : MetadataCollection<Parameter> {
    internal ParameterCollection() : base(){}
    internal ParameterCollection(int capacity) : base(capacity){}
    internal ParameterCollection(ICollection<Parameter> collection) : base(collection) { }
    internal ParameterCollection Clone() {
      return new ParameterCollection(this);
    }
  }
  public sealed class ResourceCollection : MetadataCollection<Resource> {
    internal ResourceCollection() : base(){}
    internal ResourceCollection(int capacity) : base(capacity){}
  }
  public sealed class SecurityAttributeCollection : MetadataCollection<SecurityAttribute> {
    internal SecurityAttributeCollection() : base(){}
    internal SecurityAttributeCollection(int capacity) : base(capacity){}
    internal SecurityAttributeCollection(ICollection<SecurityAttribute> collection) : base(collection) { }
    internal SecurityAttributeCollection Clone() {
      return new SecurityAttributeCollection(this);
    }
  }
  public sealed class StackVariableCollection : MetadataCollection<Local> {
    internal StackVariableCollection() : base(){}
    internal StackVariableCollection(int capacity) : base(capacity) { }
  }
  public sealed class StatementCollection : MetadataCollection<Statement> {
    internal StatementCollection() : base(){}
    internal StatementCollection(int capacity) : base(capacity){}
    internal StatementCollection(ICollection<Statement> collection) : base(collection) { }
    internal StatementCollection Clone() {
      return new StatementCollection(this);
    }
  }
  public sealed class TypeNodeCollection : MetadataCollection<TypeNode> {
    internal TypeNodeCollection() : base(){}
    internal TypeNodeCollection(int capacity) : base(capacity){}
    internal TypeNodeCollection(params TypeNode[] range) : base(range) {}
    internal TypeNodeCollection(ICollection<TypeNode> collection) : base(collection) { }
    internal TypeNodeCollection Clone() {
      return new TypeNodeCollection(this);
    }
  }
  public sealed class Win32ResourceCollection : MetadataCollection<Win32Resource> {
    internal Win32ResourceCollection() : base(){}
    internal Win32ResourceCollection(int capacity) : base(capacity) { }
  }
}
#endif