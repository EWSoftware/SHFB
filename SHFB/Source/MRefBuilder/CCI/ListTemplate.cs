// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 11/22/2013 - EFW - Reworked the classes to derive from List<T> so that they are LINQ-friendly and cleared out
// the unused classes.

// Ignore Spelling: Typeswitch

using System.Collections.Generic;

namespace System.Compiler
{
    public sealed class AliasDefinitionList : List<AliasDefinition>
    {
        public AliasDefinitionList(IEnumerable<AliasDefinition> collection) : base(collection)
        {
        }
    }
    
    public sealed class AssemblyReferenceList : List<AssemblyReference>
    {
        public AssemblyReferenceList()
        {
        }

        public AssemblyReferenceList(IEnumerable<AssemblyReference> collection) : base(collection)
        {
        }
    }
    
    public sealed class AttributeList : List<AttributeNode>
    {
        public AttributeList()
        {
        }

        public AttributeList(IEnumerable<AttributeNode> collection) : base(collection)
        {
        }
    }
    
    public sealed class BlockList : List<Block>
    {
        public BlockList()
        {
        }

        public BlockList(IEnumerable<Block> collection) : base(collection)
        {
        }
    }

    public sealed class CatchList : List<Catch>
    {
        public CatchList(IEnumerable<Catch> collection) : base(collection)
        {
        }
    }

    public sealed class CompilationUnitList : List<CompilationUnit>
    {
        public CompilationUnitList(IEnumerable<CompilationUnit> collection) : base(collection)
        {
        }
    }
    
    public sealed class ErrorNodeList : List<ErrorNode>
    {
    }

    public sealed class ExpressionList : List<Expression>
    {
        public ExpressionList()
        {
        }

        public ExpressionList(IEnumerable<Expression> collection) : base(collection)
        {
        }
    }

    public sealed class ExceptionHandlerList : List<ExceptionHandler>
    {
    }

    public sealed class FaultHandlerList : List<FaultHandler>
    {
        public FaultHandlerList(IEnumerable<FaultHandler> collection) : base(collection)
        {
        }
    }

    public sealed class FieldList : List<Field>
    {
        public FieldList()
        {
        }

        public FieldList(IEnumerable<Field> collection) : base(collection)
        {
        }
    }

    public sealed class FilterList : List<Filter>
    {
        public FilterList(IEnumerable<Filter> collection) : base(collection)
        {
        }
    }

    public sealed class InstructionList : List<Instruction>
    {
    }

    public sealed class InterfaceList : List<Interface>
    {
        public InterfaceList()
        {
        }

        public InterfaceList(IEnumerable<Interface> collection) : base(collection)
        {
        }
    }

    public sealed class LocalDeclarationList : List<LocalDeclaration>
    {
        public LocalDeclarationList()
        {
        }

        public LocalDeclarationList(IEnumerable<LocalDeclaration> collection) : base(collection)
        {
        }
    }

    public sealed class LocalList : List<Local>
    {
    }

    public sealed class MemberList : List<Member>
    {
        public MemberList()
        {
        }

        public MemberList(IEnumerable<Member> collection) : base(collection)
        {
        }
    }

    public sealed class MethodList : List<Method>
    {
        public MethodList()
        {
        }

        public MethodList(IEnumerable<Method> collection) : base(collection)
        {
        }
    }

    public sealed class ModuleReferenceList : List<ModuleReference>
    {
    }
    
    public sealed class NamespaceList : List<Namespace>
    {
        public NamespaceList()
        {
        }

        public NamespaceList(IEnumerable<Namespace> collection) : base(collection)
        {
        }
    }

    public sealed class NodeList : List<Node>
    {
        public NodeList()
        {
        }

        public NodeList(IEnumerable<Node> collection) : base(collection)
        {
        }
    }
    
    public sealed class ParameterList : List<Parameter>
    {
        public readonly static ParameterList Empty = [];

        public ParameterList()
        {
        }

        public ParameterList(IEnumerable<Parameter> collection) : base(collection)
        {
        }
    }
    
    public sealed class ResourceList : List<Resource>
    {
    }
    
    public sealed class StatementList : List<Statement>
    {
        public StatementList()
        {
        }

        public StatementList(IEnumerable<Statement> collection) : base(collection)
        {
        }
    }

    public sealed class SwitchCaseList : List<SwitchCase>
    {
        public SwitchCaseList(IEnumerable<SwitchCase> collection) : base(collection)
        {
        }
    }

    public sealed class TypeNodeList : List<TypeNode>
    {
        public TypeNodeList()
        {
        }

        public TypeNodeList(IEnumerable<TypeNode> collection) : base(collection)
        {
        }
    }
    
    public sealed class TypeswitchCaseList : List<TypeswitchCase>
    {
        public TypeswitchCaseList(IEnumerable<TypeswitchCase> collection) : base(collection)
        {
        }
    }

    public sealed class UsedNamespaceList : List<UsedNamespace>
    {
        public UsedNamespaceList(IEnumerable<UsedNamespace> collection) : base(collection)
        {
        }
    }

    public sealed class Win32ResourceList : List<Win32Resource>
    {
    }
}
