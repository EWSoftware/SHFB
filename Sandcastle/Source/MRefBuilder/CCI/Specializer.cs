// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Diagnostics;
#if FxCop
using InterfaceList = Microsoft.Cci.InterfaceCollection;
using MemberList = Microsoft.Cci.MemberCollection;
using MethodList = Microsoft.Cci.MethodCollection;
using TypeNodeList = Microsoft.Cci.TypeNodeCollection;
using Module = Microsoft.Cci.ModuleNode;
using Class = Microsoft.Cci.ClassNode;
using Interface = Microsoft.Cci.InterfaceNode;
#endif
#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler
{
#endif
    /* Specializer walks an IR, replacing references to type parameters with references to actual types.
   * The main complication is that structural types involving type parameters need to be reconstructed.
   * Other complications arise from the fact that IL is not orthogonal and requires different instructions
   * to be used depending on whether a type is a reference type or a value type. In templates, type parameters
   * are treated as reference types when method bodies are generated. In order to instantiate a template with
   * a value type argument, it is necessary to walk the method bodies and transform some expressions. This is
   * not possible to do in a single pass because method bodies can contain references to signatures defined
   * in parts of the IR that have not yet been visited and specialized. Consequently, Specializer ignores
   * method bodies.
   * 
   * Once all signatures have been fixed up by Specializer, it is necessary to use MethodBodySpecializer
   * to walk the method bodies and fix up the IL to deal with value types that replaced type parameters.
   * Another complication to deal with is that MemberBindings and NameBindings can refer to members
   * defined in structural types based on type parameters. These must be substituted with references to the
   * corresponding members of structural types based on the type arguments. Note that some structural types
   * are themselves implemented as templates.
   */

    /// <summary>
    /// This class specializes a normalized IR by replacing type parameters with type arguments.
    /// </summary>
#if !FxCop
    public
#endif
 class Specializer : StandardVisitor
    {
        public TypeNodeList pars;
        public TypeNodeList args;
        public Method CurrentMethod;
        public TypeNode CurrentType;
        public Module TargetModule;

        public Specializer(Module targetModule, TypeNodeList pars, TypeNodeList args)
        {
            Debug.Assert(pars != null && pars.Count > 0);
            Debug.Assert(args != null && args.Count > 0);
            this.pars = pars;
            this.args = args;
            this.TargetModule = targetModule;
        }
#if !MinimalReader
        public Specializer(Visitor callingVisitor)
            : base(callingVisitor)
        {
        }
        public override void TransferStateTo(Visitor targetVisitor)
        {
            base.TransferStateTo(targetVisitor);
            Specializer target = targetVisitor as Specializer;
            if (target == null) return;
            target.args = this.args;
            target.pars = this.pars;
            target.CurrentMethod = this.CurrentMethod;
            target.CurrentType = this.CurrentType;
        }
#endif
        public override DelegateNode VisitDelegateNode(DelegateNode delegateNode)
        {
            return this.VisitTypeNode(delegateNode) as DelegateNode;
        }
        public override Interface VisitInterfaceReference(Interface Interface)
        {
            return this.VisitTypeReference(Interface) as Interface;
        }
        public virtual Member VisitMemberReference(Member member)
        {
            if (member == null) return null;
#if false && !MinimalReader
      ParameterField pField = member as ParameterField;
      if (pField != null){
        if (pField.Parameter != null) pField.Type = pField.Parameter.Type;
        return pField;
      }
#endif
            TypeNode specializedType = this.VisitTypeReference(member.DeclaringType);
            if (specializedType == member.DeclaringType || specializedType == null) return member;
            return Specializer.GetCorrespondingMember(member, specializedType);
        }
        public static Member GetCorrespondingMember(Member/*!*/ member, TypeNode/*!*/ specializedType)
        {
            //member belongs to a structural type based on a type parameter.
            //return the corresponding member from the structural type based on the type argument.
            if (member.DeclaringType == null) { Debug.Fail(""); return null; }
            MemberList unspecializedMembers = member.DeclaringType.Members;
            MemberList specializedMembers = specializedType.Members;
            if (unspecializedMembers == null || specializedMembers == null) { Debug.Assert(false); return null; }
            int unspecializedOffset = 0;
            int specializedOffset = 0;
            //The offsets can become > 0 when the unspecialized type and/or specialized type is imported from another assembly 
            //(and the unspecialized type is in fact a partially specialized type.)
            for (int i = 0, n = specializedMembers == null ? 0 : specializedMembers.Count; i < n; i++)
            {
                Member unspecializedMember = unspecializedMembers[i - unspecializedOffset];
                Member specializedMember = specializedMembers[i - specializedOffset];
                if (unspecializedMember != null && specializedMember == null && unspecializedOffset == i &&
                  !(unspecializedMember is TypeParameter || unspecializedMember is ClassParameter))
                {
                    unspecializedOffset++; continue; //Keep current unspecialized member, skip over null specialized member
                }
                if (unspecializedMember == null && specializedMember != null && specializedOffset == i &&
                  !(specializedMember is TypeParameter || specializedMember is ClassParameter))
                {
                    specializedOffset++; continue; //Keep current specialized member, skip over null
                }
                if (unspecializedMember == member)
                {
                    Debug.Assert(specializedMember != null);
                    return specializedMember;
                }
            }
            Debug.Assert(false);
            return null;
        }
        public readonly Block DummyBody = new Block();
        public override Method VisitMethod(Method method)
        {
            if (method == null) return null;
            Method savedCurrentMethod = this.CurrentMethod;
            TypeNode savedCurrentType = this.CurrentType;
            this.CurrentMethod = method;
            this.CurrentType = method.DeclaringType;
            method.ThisParameter = (This)this.VisitThis(method.ThisParameter);
            method.Attributes = this.VisitAttributeList(method.Attributes);
            method.ReturnAttributes = this.VisitAttributeList(method.ReturnAttributes);
            method.SecurityAttributes = this.VisitSecurityAttributeList(method.SecurityAttributes);
            method.ReturnType = this.VisitTypeReference(method.ReturnType);
#if !MinimalReader
            method.ImplementedTypes = this.VisitTypeReferenceList(method.ImplementedTypes);
#endif
            method.Parameters = this.VisitParameterList(method.Parameters);
            if (TargetPlatform.UseGenerics && this.args != method.TemplateArguments)
            {
                method.TemplateArguments = this.VisitTypeReferenceList(method.TemplateArguments);
                method.TemplateParameters = this.VisitTypeParameterList(method.TemplateParameters);
            }
#if ExtendedRuntime
      method.Contract = this.VisitMethodContract(method.Contract);
#endif
            method.ImplementedInterfaceMethods = this.VisitMethodList(method.ImplementedInterfaceMethods);
            this.CurrentMethod = savedCurrentMethod;
            this.CurrentType = savedCurrentType;
            return method;
        }
#if ExtendedRuntime
    public override MethodContract VisitMethodContract(MethodContract contract){
      if (contract == null) return null;
      if (contract.Specializer == null) {
        contract.Specializer = new MethodContract.ContractSpecializerDelegate(this.VisitContractPart);
        contract.ensures = null;
        contract.modifies = null;
        contract.requires = null;
      } else {
        contract.ensures = this.VisitEnsuresList(contract.ensures);
        contract.modifies = this.VisitExpressionList(contract.modifies);
        contract.requires = this.VisitRequiresList(contract.requires);
      }
      return contract;
    }
    private object VisitContractPart(Method method, object part) {
      if (method == null) { Debug.Fail("method == null"); return part; }
      this.CurrentMethod = method;
      this.CurrentType = method.DeclaringType;
      EnsuresList es = part as EnsuresList;
      if (es != null) return this.VisitEnsuresList(es);
      RequiresList rs = part as RequiresList;
      if (rs != null) return this.VisitRequiresList(rs);
      return part;
    }
#endif
        public virtual MethodList VisitMethodList(MethodList methods)
        {
            if (methods == null) return null;
            int n = methods.Count;
            for (int i = 0; i < n; i++)
                methods[i] = (Method)this.VisitMemberReference(methods[i]);
            return methods;
        }
        public override TypeNode VisitTypeNode(TypeNode typeNode)
        {
            if (typeNode == null) return null;
            TypeNode savedCurrentType = this.CurrentType;
            if (savedCurrentType != null && savedCurrentType.TemplateArguments != null && savedCurrentType.TemplateArguments.Count > 0 &&
              typeNode.Template != null && (typeNode.Template.TemplateParameters == null || typeNode.Template.TemplateParameters.Count == 0))
            {
                typeNode.TemplateArguments = new TypeNodeList(0);
            }
            this.CurrentType = typeNode;
            typeNode.Attributes = this.VisitAttributeList(typeNode.Attributes);
            typeNode.SecurityAttributes = this.VisitSecurityAttributeList(typeNode.SecurityAttributes);
            Class c = typeNode as Class;
            if (c != null) c.BaseClass = (Class)this.VisitTypeReference(c.BaseClass);
            typeNode.Interfaces = this.VisitInterfaceReferenceList(typeNode.Interfaces);
            if (typeNode.ProvideTypeMembers != null && typeNode.ProvideNestedTypes != null && typeNode.ProviderHandle != null)
            {
                typeNode.members = null;
                typeNode.ProviderHandle = new SpecializerHandle(typeNode.ProvideNestedTypes, typeNode.ProvideTypeMembers, typeNode.ProviderHandle);
                typeNode.ProvideNestedTypes = new TypeNode.NestedTypeProvider(this.ProvideNestedTypes);
                typeNode.ProvideTypeMembers = new TypeNode.TypeMemberProvider(this.ProvideTypeMembers);
#if !MinimalReader
                DelegateNode delegateNode = typeNode as DelegateNode;
                if (delegateNode != null)
                {
                    if (!delegateNode.IsNormalized)
                    { //In the Normalized case Parameters are retrieved from the Invoke method, which means evaluating Members
                        delegateNode.Parameters = this.VisitParameterList(delegateNode.Parameters);
                        delegateNode.ReturnType = this.VisitTypeReference(delegateNode.ReturnType);
                    }
                }
#endif
            }
            else
            {
                typeNode.Members = this.VisitMemberList(typeNode.Members);
                DelegateNode delegateNode = typeNode as DelegateNode;
                if (delegateNode != null)
                {
                    delegateNode.Parameters = this.VisitParameterList(delegateNode.Parameters);
                    delegateNode.ReturnType = this.VisitTypeReference(delegateNode.ReturnType);
                }
            }
            this.CurrentType = savedCurrentType;
            return typeNode;
        }
        private void ProvideNestedTypes(TypeNode/*!*/ typeNode, object/*!*/ handle)
        {
            SpecializerHandle sHandler = (SpecializerHandle)handle;
            TypeNode savedCurrentType = this.CurrentType;
            this.CurrentType = typeNode;
            sHandler.NestedTypeProvider(typeNode, sHandler.Handle);
            TypeNodeList nestedTypes = typeNode.nestedTypes;
            for (int i = 0, n = nestedTypes == null ? 0 : nestedTypes.Count; i < n; i++)
            {
                //^ assert nestedTypes != null;
                TypeNode nt = nestedTypes[i];
                if (nt == null) continue;
                this.VisitTypeNode(nt);
            }
            this.CurrentType = savedCurrentType;
        }
        private void ProvideTypeMembers(TypeNode/*!*/ typeNode, object/*!*/ handle)
        {
            SpecializerHandle sHandler = (SpecializerHandle)handle;
            TypeNode savedCurrentType = this.CurrentType;
            this.CurrentType = typeNode;
            sHandler.TypeMemberProvider(typeNode, sHandler.Handle);
            typeNode.Members = this.VisitMemberList(typeNode.Members);
            DelegateNode delegateNode = typeNode as DelegateNode;
            if (delegateNode != null && delegateNode.IsNormalized)
            {
                delegateNode.Parameters = this.VisitParameterList(delegateNode.Parameters);
                delegateNode.ReturnType = this.VisitTypeReference(delegateNode.ReturnType);
            }
            this.CurrentType = savedCurrentType;
        }
        internal class SpecializerHandle
        {
            internal TypeNode.NestedTypeProvider/*!*/ NestedTypeProvider;
            internal TypeNode.TypeMemberProvider/*!*/ TypeMemberProvider;
            internal object/*!*/ Handle;
            internal SpecializerHandle(TypeNode.NestedTypeProvider/*!*/ nestedTypeProvider, TypeNode.TypeMemberProvider/*!*/ typeMemberProvider, object/*!*/ handle)
            {
                this.NestedTypeProvider = nestedTypeProvider;
                this.TypeMemberProvider = typeMemberProvider;
                this.Handle = handle;
                //^ base();
            }
        }
        public virtual Expression VisitTypeExpression(Expression expr)
        {
            TypeNodeList pars = this.pars;
            TypeNodeList args = this.args;
            Identifier id = expr as Identifier;
            if (id != null)
            {
                int key = id.UniqueIdKey;
                for (int i = 0, n = pars == null ? 0 : pars.Count, m = args == null ? 0 : args.Count; i < n && i < m; i++)
                {
                    //^ assert pars != null && args != null;
                    TypeNode par = pars[i];
                    if (par == null || par.Name == null) continue;
                    if (par.Name.UniqueIdKey == key) return new Literal(args[i], CoreSystemTypes.Type);
                }
                return id;
            }
#if !MinimalReader
            Debug.Assert(expr is QualifiedIdentifier || expr is Literal);
#endif
            return expr;
        }
        public override TypeNode VisitTypeParameter(TypeNode typeParameter)
        {
            if (typeParameter == null) return null;
            if (TargetPlatform.UseGenerics)
            {
                InterfaceList interfaces = typeParameter.Interfaces;
                if (interfaces == null || interfaces.Count == 0) return typeParameter;
                TypeNode baseType = this.VisitTypeReference(interfaces[0]);
                if (baseType is Interface)
                    typeParameter.Interfaces = this.VisitInterfaceReferenceList(typeParameter.Interfaces);
                else
                    typeParameter = this.ConvertToClassParameter(baseType, typeParameter);
                return typeParameter;
            }
            else
            {
                typeParameter.Interfaces = this.VisitInterfaceReferenceList(typeParameter.Interfaces);
                return null;
            }
        }
        private TypeNode ConvertToClassParameter(TypeNode baseType, TypeNode/*!*/ typeParameter)
        {
            ClassParameter result;
            if (typeParameter is MethodTypeParameter)
            {
                result = new MethodClassParameter();
            }
            else if (typeParameter is TypeParameter)
            {
                result = new ClassParameter();
                result.DeclaringType = typeParameter.DeclaringType;
            }
            else
                return typeParameter; //give up
            result.SourceContext = typeParameter.SourceContext;
            result.TypeParameterFlags = ((ITypeParameter)typeParameter).TypeParameterFlags;
#if ExtendedRuntime
      if (typeParameter.IsUnmanaged) { result.SetIsUnmanaged(); }
#endif
            result.Name = typeParameter.Name;
            result.Namespace = StandardIds.ClassParameter;
            result.BaseClass = baseType is Class ? (Class)baseType : CoreSystemTypes.Object;
            result.DeclaringMember = ((ITypeParameter)typeParameter).DeclaringMember;
            result.DeclaringModule = typeParameter.DeclaringModule;
            result.Flags = typeParameter.Flags & ~TypeFlags.Interface;
            //InterfaceList contraints = result.Interfaces = new InterfaceList();
            InterfaceList interfaces = typeParameter.Interfaces;
            for (int i = 1, n = interfaces == null ? 0 : interfaces.Count; i < n; i++)
            {
                //^ assert interfaces != null;
                interfaces.Add(this.VisitInterfaceReference(interfaces[i]));
            }
            return result;
        }
        public override TypeNode VisitTypeReference(TypeNode type)
        { //TODO: break up this method
            if (type == null) return null;
            TypeNodeList pars = this.pars;
            TypeNodeList args = this.args;
            switch (type.NodeType)
            {
                case NodeType.ArrayType:
                    ArrayType arrType = (ArrayType)type;
                    TypeNode elemType = this.VisitTypeReference(arrType.ElementType);
                    if (elemType == arrType.ElementType || elemType == null) return arrType;
                    if (arrType.IsSzArray()) return elemType.GetArrayType(1);
                    return elemType.GetArrayType(arrType.Rank, arrType.Sizes, arrType.LowerBounds);
#if !MinimalReader
                case NodeType.DelegateNode:
                    {
                        FunctionType ftype = type as FunctionType;
                        if (ftype == null) goto default;
                        TypeNode referringType = ftype.DeclaringType == null ? this.CurrentType : this.VisitTypeReference(ftype.DeclaringType);
                        return FunctionType.For(this.VisitTypeReference(ftype.ReturnType), this.VisitParameterList(ftype.Parameters), referringType);
                    }
#endif
                case NodeType.Pointer:
                    Pointer pType = (Pointer)type;
                    elemType = this.VisitTypeReference(pType.ElementType);
                    if (elemType == pType.ElementType || elemType == null) return pType;
                    return elemType.GetPointerType();
                case NodeType.Reference:
                    Reference rType = (Reference)type;
                    elemType = this.VisitTypeReference(rType.ElementType);
                    if (elemType == rType.ElementType || elemType == null) return rType;
                    return elemType.GetReferenceType();
#if ExtendedRuntime
        case NodeType.TupleType:{
          TupleType tType = (TupleType)type;
          bool reconstruct = false;
          MemberList members = tType.Members;
          int n = members == null ? 0 : members.Count;
          FieldList fields = new FieldList(n);
          for (int i = 0; i < n; i++){
            //^ assert members != null;
            Field f = members[i] as Field;
            if (f == null) continue;
            f = (Field)f.Clone();
            fields.Add(f);
            TypeNode oft = f.Type;
            TypeNode ft = f.Type = this.VisitTypeReference(f.Type);
            if (ft != oft) reconstruct = true;
          }
          if (!reconstruct) return tType;
          TypeNode referringType = tType.DeclaringType == null ? this.CurrentType : this.VisitTypeReference(tType.DeclaringType);
          return TupleType.For(fields, referringType);}
        case NodeType.TypeIntersection:{
          TypeIntersection tIntersect = (TypeIntersection)type;
          TypeNode referringType = tIntersect.DeclaringType == null ? this.CurrentType : this.VisitTypeReference(tIntersect.DeclaringType);
          return TypeIntersection.For(this.VisitTypeReferenceList(tIntersect.Types), referringType);}
        case NodeType.TypeUnion:{
          TypeUnion tUnion = (TypeUnion)type;
          TypeNode referringType = tUnion.DeclaringType == null ? this.CurrentType : this.VisitTypeReference(tUnion.DeclaringType);
          TypeNodeList types = this.VisitTypeReferenceList(tUnion.Types);
          if (referringType == null || types == null) { Debug.Fail(""); return null; }
          return TypeUnion.For(types, referringType);}
#endif
#if !MinimalReader
                case NodeType.ArrayTypeExpression:
                    ArrayTypeExpression aExpr = (ArrayTypeExpression)type;
                    aExpr.ElementType = this.VisitTypeReference(aExpr.ElementType);
                    return aExpr;
                case NodeType.BoxedTypeExpression:
                    BoxedTypeExpression bExpr = (BoxedTypeExpression)type;
                    bExpr.ElementType = this.VisitTypeReference(bExpr.ElementType);
                    return bExpr;
                case NodeType.ClassExpression:
                    {
                        ClassExpression cExpr = (ClassExpression)type;
                        cExpr.Expression = this.VisitTypeExpression(cExpr.Expression);
                        Literal lit = cExpr.Expression as Literal; //Could happen if the expression is a template parameter
                        if (lit != null) return lit.Value as TypeNode;
                        cExpr.TemplateArguments = this.VisitTypeReferenceList(cExpr.TemplateArguments);
                        return cExpr;
                    }
#endif
                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                    int key = type.UniqueKey;
                    for (int i = 0, n = pars == null ? 0 : pars.Count, m = args == null ? 0 : args.Count; i < n && i < m; i++)
                    {
                        //^ assert pars != null && args != null;
                        TypeNode tp = pars[i];
                        if (tp == null) continue;
                        if (tp.UniqueKey == key) return args[i];
                        if (tp.Name.UniqueIdKey == type.Name.UniqueIdKey && (tp is ClassParameter && type is TypeParameter))
                        {
                            //This shouldn't really happen, but in practice it does. Hack past it.
                            return args[i];
                        }
                    }
                    return type;
#if ExtendedRuntime
        case NodeType.ConstrainedType:{
          ConstrainedType conType = (ConstrainedType)type;
          TypeNode referringType = conType.DeclaringType == null ? this.CurrentType : this.VisitTypeReference(conType.DeclaringType);
          TypeNode underlyingType = this.VisitTypeReference(conType.UnderlyingType);
          Expression constraint = this.VisitExpression(conType.Constraint);
          if (referringType == null || underlyingType == null || constraint == null) { Debug.Fail(""); return null; }
          return new ConstrainedType(underlyingType, constraint, referringType);}
#endif
#if !MinimalReader
                case NodeType.FlexArrayTypeExpression:
                    FlexArrayTypeExpression flExpr = (FlexArrayTypeExpression)type;
                    flExpr.ElementType = this.VisitTypeReference(flExpr.ElementType);
                    return flExpr;
                case NodeType.FunctionTypeExpression:
                    FunctionTypeExpression ftExpr = (FunctionTypeExpression)type;
                    ftExpr.Parameters = this.VisitParameterList(ftExpr.Parameters);
                    ftExpr.ReturnType = this.VisitTypeReference(ftExpr.ReturnType);
                    return ftExpr;
                case NodeType.InvariantTypeExpression:
                    InvariantTypeExpression invExpr = (InvariantTypeExpression)type;
                    invExpr.ElementType = this.VisitTypeReference(invExpr.ElementType);
                    return invExpr;
#endif
                case NodeType.InterfaceExpression:
                    InterfaceExpression iExpr = (InterfaceExpression)type;
                    if (iExpr.Expression == null) goto default;
                    iExpr.Expression = this.VisitTypeExpression(iExpr.Expression);
                    iExpr.TemplateArguments = this.VisitTypeReferenceList(iExpr.TemplateArguments);
                    return iExpr;
#if !MinimalReader
                case NodeType.NonEmptyStreamTypeExpression:
                    NonEmptyStreamTypeExpression neExpr = (NonEmptyStreamTypeExpression)type;
                    neExpr.ElementType = this.VisitTypeReference(neExpr.ElementType);
                    return neExpr;
                case NodeType.NonNullTypeExpression:
                    NonNullTypeExpression nnExpr = (NonNullTypeExpression)type;
                    nnExpr.ElementType = this.VisitTypeReference(nnExpr.ElementType);
                    return nnExpr;
                case NodeType.NonNullableTypeExpression:
                    NonNullableTypeExpression nbExpr = (NonNullableTypeExpression)type;
                    nbExpr.ElementType = this.VisitTypeReference(nbExpr.ElementType);
                    return nbExpr;
                case NodeType.NullableTypeExpression:
                    NullableTypeExpression nuExpr = (NullableTypeExpression)type;
                    nuExpr.ElementType = this.VisitTypeReference(nuExpr.ElementType);
                    return nuExpr;
#endif
                case NodeType.OptionalModifier:
                    {
                        TypeModifier modType = (TypeModifier)type;
                        TypeNode modifiedType = this.VisitTypeReference(modType.ModifiedType);
                        TypeNode modifierType = this.VisitTypeReference(modType.Modifier);
                        if (modifiedType == null || modifierType == null) { return type; }
#if ExtendedRuntime
          if (modifierType != null && modifierType == SystemTypes.NullableType){
            if (modifiedType.IsValueType) return modifiedType;
            if (TypeNode.HasModifier(modifiedType, SystemTypes.NonNullType))
              modifiedType = TypeNode.StripModifier(modifiedType, SystemTypes.NonNullType);
            if (modifiedType.IsTemplateParameter) {
              return OptionalModifier.For(modifierType, modifiedType);
            }
            return modifiedType;
          }
          if (modifierType == SystemTypes.NonNullType) {
            if (modifiedType.IsValueType) return modifiedType;
            modifiedType = TypeNode.StripModifier(modifiedType, SystemTypes.NonNullType);
          }
          //^ assert modifiedType != null;
#endif
                        return OptionalModifier.For(modifierType, modifiedType);
                    }
                case NodeType.RequiredModifier:
                    {
                        TypeModifier modType = (TypeModifier)type;
                        TypeNode modifiedType = this.VisitTypeReference(modType.ModifiedType);
                        TypeNode modifierType = this.VisitTypeReference(modType.Modifier);
                        if (modifiedType == null || modifierType == null) { Debug.Fail(""); return type; }
                        return RequiredModifier.For(modifierType, modifiedType);
                    }
#if !MinimalReader
                case NodeType.OptionalModifierTypeExpression:
                    OptionalModifierTypeExpression optmodType = (OptionalModifierTypeExpression)type;
                    optmodType.ModifiedType = this.VisitTypeReference(optmodType.ModifiedType);
                    optmodType.Modifier = this.VisitTypeReference(optmodType.Modifier);
                    return optmodType;
                case NodeType.RequiredModifierTypeExpression:
                    RequiredModifierTypeExpression reqmodType = (RequiredModifierTypeExpression)type;
                    reqmodType.ModifiedType = this.VisitTypeReference(reqmodType.ModifiedType);
                    reqmodType.Modifier = this.VisitTypeReference(reqmodType.Modifier);
                    return reqmodType;
                case NodeType.PointerTypeExpression:
                    PointerTypeExpression pExpr = (PointerTypeExpression)type;
                    pExpr.ElementType = this.VisitTypeReference(pExpr.ElementType);
                    return pExpr;
                case NodeType.ReferenceTypeExpression:
                    ReferenceTypeExpression rExpr = (ReferenceTypeExpression)type;
                    rExpr.ElementType = this.VisitTypeReference(rExpr.ElementType);
                    return rExpr;
                case NodeType.StreamTypeExpression:
                    StreamTypeExpression sExpr = (StreamTypeExpression)type;
                    sExpr.ElementType = this.VisitTypeReference(sExpr.ElementType);
                    return sExpr;
                case NodeType.TupleTypeExpression:
                    TupleTypeExpression tuExpr = (TupleTypeExpression)type;
                    tuExpr.Domains = this.VisitFieldList(tuExpr.Domains);
                    return tuExpr;
                case NodeType.TypeExpression:
                    {
                        TypeExpression tExpr = (TypeExpression)type;
                        tExpr.Expression = this.VisitTypeExpression(tExpr.Expression);
                        if (tExpr.Expression is Literal) return type;
                        tExpr.TemplateArguments = this.VisitTypeReferenceList(tExpr.TemplateArguments);
                        return tExpr;
                    }
                case NodeType.TypeIntersectionExpression:
                    TypeIntersectionExpression tiExpr = (TypeIntersectionExpression)type;
                    tiExpr.Types = this.VisitTypeReferenceList(tiExpr.Types);
                    return tiExpr;
                case NodeType.TypeUnionExpression:
                    TypeUnionExpression tyuExpr = (TypeUnionExpression)type;
                    tyuExpr.Types = this.VisitTypeReferenceList(tyuExpr.Types);
                    return tyuExpr;
#endif
                default:
                    TypeNode declaringType = this.VisitTypeReference(type.DeclaringType);
                    if (declaringType != null)
                    {
                        Identifier tname = type.Name;
                        if (type.Template != null && type.IsGeneric) tname = type.Template.Name;
                        TypeNode nt = declaringType.GetNestedType(tname);
                        if (nt != null)
                        {
                            TypeNodeList arguments = type.TemplateArguments;
                            type = nt;
                            if (TargetPlatform.UseGenerics)
                            {
                                if (arguments != null && arguments.Count > 0 && nt.ConsolidatedTemplateParameters != null && nt.ConsolidatedTemplateParameters.Count > 0)
                                    type = nt.GetTemplateInstance(this.TargetModule, this.CurrentType, declaringType, arguments);
                            }
                        }
                    }
                    if (type.Template != null && (type.ConsolidatedTemplateParameters == null || type.ConsolidatedTemplateParameters.Count == 0))
                    {
                        if (!type.IsNotFullySpecialized && (!type.IsNormalized ||
                         (this.CurrentType != null && type.DeclaringModule == this.CurrentType.DeclaringModule))) return type;
                        //Type is a template instance, but some of its arguments were themselves parameters.
                        //See if any of these parameters are to be specialized by this specializer.
                        bool mustSpecializeFurther = false;
                        TypeNodeList targs = type.TemplateArguments;
                        int numArgs = targs == null ? 0 : targs.Count;
                        if (targs != null)
                        {
                            targs = targs.Clone();
                            for (int i = 0; i < numArgs; i++)
                            {
                                TypeNode targ = targs[i];
                                ITypeParameter tparg = targ as ITypeParameter;
                                if (tparg != null)
                                {
                                    for (int j = 0, np = pars == null ? 0 : pars.Count, m = args == null ? 0 : args.Count; j < np && j < m; j++)
                                    {
                                        //^ assert pars != null && args != null;
                                        if (TargetPlatform.UseGenerics)
                                        {
                                            ITypeParameter par = pars[j] as ITypeParameter;
                                            if (par == null) continue;
                                            if (tparg == par || (tparg.ParameterListIndex == par.ParameterListIndex && tparg.DeclaringMember == par.DeclaringMember))
                                            {
                                                targ = this.args[j]; break;
                                            }
                                        }
                                        else
                                        {
                                            if (targ == pars[j]) { targ = this.args[j]; break; }
                                        }
                                    }
                                }
                                else
                                {
                                    if (targ != type)
                                        targ = this.VisitTypeReference(targ);
                                    if (targ == type) continue;
                                }
                                mustSpecializeFurther |= targs[i] != targ;
                                targs[i] = targ;
                            }
                        }
                        if (targs == null || !mustSpecializeFurther) return type;
                        return type.Template.GetTemplateInstance(this.TargetModule, this.CurrentType, declaringType, targs);
                    }
                    TypeNodeList tPars = type.TemplateParameters;
                    if (tPars == null || tPars.Count == 0) return type; //Not a parameterized type. No need to get an instance.
                    TypeNodeList tArgs = new TypeNodeList();
                    for (int i = 0, n = tPars.Count; i < n; i++)
                    {
                        TypeNode tPar = tPars[i];
                        tArgs.Add(tPar); //Leave parameter in place if there is no match
                        if (tPar == null || tPar.Name == null) continue;
                        int idKey = tPar.Name.UniqueIdKey;
                        for (int j = 0, m = pars == null ? 0 : pars.Count, k = args == null ? 0 : args.Count; j < m && j < k; j++)
                        {
                            //^ assert pars != null && args != null;
                            TypeNode par = pars[j];
                            if (par == null || par.Name == null) continue;
                            if (par.Name.UniqueIdKey == idKey)
                            {
                                tArgs[i] = args[j];
                                break;
                            }
                        }
                    }
                    return type.GetTemplateInstance(this.TargetModule, this.CurrentType, this.VisitTypeReference(type.DeclaringType), tArgs);
            }
        }
    }
#if !NoWriter
    public class MethodBodySpecializer : Specializer
    {
        public TrivialHashtable/*!*/ alreadyVisitedNodes = new TrivialHashtable();
        public Method methodBeingSpecialized;
        public Method dummyMethod;

        public MethodBodySpecializer(Module module, TypeNodeList pars, TypeNodeList args)
            : base(module, pars, args)
        {
            //^ base;
        }
#if !MinimalReader
        public MethodBodySpecializer(Visitor callingVisitor)
            : base(callingVisitor)
        {
            //^ base;
        }
#endif
        public override Node Visit(Node node)
        {
            Literal lit = node as Literal;
            if (lit != null && lit.Value == null) return lit;
            Expression e = node as Expression;
            if (e != null && !(e is Local || e is Parameter))
                e.Type = this.VisitTypeReference(e.Type);
            return base.Visit(node);
        }

        public override Expression VisitAddressDereference(AddressDereference addr)
        {
            if (addr == null) return null;
            bool unboxDeref = addr.Address != null && addr.Address.NodeType == NodeType.Unbox;
            addr.Address = this.VisitExpression(addr.Address);
            if (addr.Address == null) return null;
            if (unboxDeref && addr.Address.NodeType != NodeType.Unbox) return addr.Address;
            Reference reference = addr.Address.Type as Reference;
            if (reference != null) addr.Type = reference.ElementType;
            return addr;
        }
        public override Statement VisitAssignmentStatement(AssignmentStatement assignment)
        {
            assignment = (AssignmentStatement)base.VisitAssignmentStatement(assignment);
            if (assignment == null) return null;
            Expression target = assignment.Target;
            Expression source = assignment.Source;
            TypeNode tType = target == null ? null : target.Type;
            TypeNode sType = source == null ? null : source.Type;
            if (tType != null && sType != null)
            {
                //^ assert target != null;
                if (tType.IsValueType)
                {
                    if (sType is Reference)
                        assignment.Source = new AddressDereference(source, tType);
                    else if (!sType.IsValueType && !(sType == CoreSystemTypes.Object && source is Literal && target.NodeType == NodeType.AddressDereference))
                        assignment.Source = new AddressDereference(new BinaryExpression(source, new MemberBinding(null, sType), NodeType.Unbox), sType);
                }
                else
                {
                    if (sType.IsValueType)
                    {
                        if (!(tType is Reference))
                            assignment.Source = new BinaryExpression(source, new MemberBinding(null, sType), NodeType.Box, tType);
                    }
                }
            }
            return assignment;
        }
        public override Expression VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression == null) return null;
            bool opnd1IsInst = binaryExpression.Operand1 != null && binaryExpression.Operand1.NodeType == NodeType.Isinst;
            binaryExpression = (BinaryExpression)base.VisitBinaryExpression(binaryExpression);
            if (binaryExpression == null) return null;
            Expression opnd1 = binaryExpression.Operand1;
            Expression opnd2 = binaryExpression.Operand2;
            Literal lit = opnd2 as Literal;
            TypeNode t = lit == null ? null : lit.Value as TypeNode;
            if (binaryExpression.NodeType == NodeType.Castclass /*|| binaryExpression.NodeType == NodeType.ExplicitCoercion*/)
            {
                //See if castclass must become box or unbox
                if (t != null)
                {
                    if (t.IsValueType)
                    {
                        AddressDereference adref = new AddressDereference(new BinaryExpression(opnd1, lit, NodeType.Unbox), t);
                        adref.Type = t;
                        return adref;
                    }
                    if (opnd1 != null && opnd1.Type != null && opnd1.Type.IsValueType)
                    {
                        return new BinaryExpression(opnd1, new MemberBinding(null, opnd1.Type), NodeType.Box, t);
                    }
                }
            }
            else if (binaryExpression.NodeType == NodeType.Unbox)
            {
                if (opnd1 != null && opnd1.Type != null && opnd1.Type.IsValueType)
                    return opnd1;
#if ExtendedRuntime
      }else if (binaryExpression.NodeType == NodeType.Box){
        if (t != null && t.IsReferenceType && !t.IsPointerType) { // using pointer types is a Sing# extension
          return opnd1;
        }
#endif
            }
            else if (binaryExpression.NodeType == NodeType.Eq)
            {
                //For value types, turn comparisons against null into false
                if (lit != null && lit.Value == null && opnd1 != null && opnd1.Type != null && opnd1.Type.IsValueType)
                    return Literal.False;
                lit = opnd1 as Literal;
                if (lit != null && lit.Value == null && opnd2 != null && opnd2.Type != null && opnd2.Type.IsValueType)
                    return Literal.False;
            }
            else if (binaryExpression.NodeType == NodeType.Ne)
            {
                //For value types, turn comparisons against null into true
                if (lit != null && lit.Value == null && opnd1 != null && opnd1.Type != null && opnd1.Type.IsValueType)
                {
                    if (opnd1IsInst && opnd1.Type == CoreSystemTypes.Boolean) return opnd1;
                    return Literal.True;
                }
                lit = opnd1 as Literal;
                if (lit != null && lit.Value == null && opnd2 != null && opnd2.Type != null && opnd2.Type.IsValueType)
                    return Literal.True;
            }
            else if (binaryExpression.NodeType == NodeType.Isinst)
            {
                //Do not emit isinst instruction if opnd1 is a value type.
                if (opnd1 != null && opnd1.Type != null && opnd1.Type.IsValueType)
                {
                    if (opnd1.Type == t)
                        return Literal.True;
                    else
                        return Literal.False;
                }
            }
            return binaryExpression;
        }
        public override Statement VisitBranch(Branch branch)
        {
            branch = (Branch)base.VisitBranch(branch);
            if (branch == null) return null;
            if (branch.Condition != null && !(branch.Condition is BinaryExpression))
            {
                //Deal with implicit comparisons against null
                TypeNode ct = branch.Condition.Type;
                if (ct != null && !ct.IsPrimitiveInteger && ct != CoreSystemTypes.Boolean && ct.IsValueType)
                {
                    if (branch.Condition.NodeType == NodeType.LogicalNot)
                        return null;
                    branch.Condition = null;
                }
            }
            return branch;
        }
        public override Expression VisitExpression(Expression expression)
        {
            if (expression == null) return null;
            switch (expression.NodeType)
            {
                case NodeType.Dup:
                case NodeType.Arglist:
                    expression.Type = this.VisitTypeReference(expression.Type);
                    return expression;
                case NodeType.Pop:
                    expression.Type = this.VisitTypeReference(expression.Type);
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
        public override Expression VisitIndexer(Indexer indexer)
        {
            indexer = (Indexer)base.VisitIndexer(indexer);
            if (indexer == null || indexer.Object == null) return null;
            ArrayType arrType = indexer.Object.Type as ArrayType;
            TypeNode elemType = null;
            if (arrType != null) elemType = indexer.Type = indexer.ElementType = arrType.ElementType;
            //if (elemType != null && elemType.IsValueType && !elemType.IsPrimitive)
            //return new AddressDereference(new UnaryExpression(indexer, NodeType.AddressOf), elemType);
            return indexer;
        }
        public override Expression VisitLiteral(Literal literal)
        {
            if (literal == null) return null;
            TypeNode t = literal.Value as TypeNode;
            if (t != null && literal.Type == CoreSystemTypes.Type)
                return new Literal(this.VisitTypeReference(t), literal.Type, literal.SourceContext);
            return (Literal)literal.Clone();
        }
        public override Expression VisitLocal(Local local)
        {
            if (local == null) return null;
            if (this.alreadyVisitedNodes[local.UniqueKey] != null) return local;
            this.alreadyVisitedNodes[local.UniqueKey] = local;
            return base.VisitLocal(local);
        }
#if !MinimalReader
        public override Statement VisitLocalDeclarationsStatement(LocalDeclarationsStatement localDeclarations)
        {
            if (localDeclarations == null) return null;
            localDeclarations.Type = this.VisitTypeReference(localDeclarations.Type);
            return localDeclarations;
        }
#endif
        public override Expression VisitParameter(Parameter parameter)
        {
#if !MinimalReader
            ParameterBinding pb = parameter as ParameterBinding;
            if (pb != null && pb.BoundParameter != null)
                pb.Type = pb.BoundParameter.Type;
#endif
            return parameter;
        }
#if !MinimalReader
        public override Expression VisitNameBinding(NameBinding nameBinding)
        {
            if (nameBinding == null) return null;
            nameBinding.BoundMember = this.VisitExpression(nameBinding.BoundMember);
            int n = nameBinding.BoundMembers == null ? 0 : nameBinding.BoundMembers.Count;
            for (int i = 0; i < n; i++)
            {
                //^ assert nameBinding.BoundMembers != null;
                nameBinding.BoundMembers[i] = this.VisitMemberReference(nameBinding.BoundMembers[i]);
            }
            return nameBinding;
        }
#endif
        public override Expression VisitMemberBinding(MemberBinding memberBinding)
        {
            if (memberBinding == null) return null;
            Expression tObj = memberBinding.TargetObject = this.VisitExpression(memberBinding.TargetObject);
            Member mem = this.VisitMemberReference(memberBinding.BoundMember);
            if (mem == this.dummyMethod)
                mem = this.methodBeingSpecialized;
            Debug.Assert(mem != null);
            memberBinding.BoundMember = mem;
            if (memberBinding == null) return null;
            Method method = memberBinding.BoundMember as Method;
            if (method != null)
            {
                //Need to take the address of the target object (this parameter), or need to box it, if this target object type is value type
                if (tObj != null && tObj.Type != null && tObj.Type.IsValueType)
                {
                    if (tObj.NodeType != NodeType.This)
                    {
                        if (method.DeclaringType != null && method.DeclaringType.IsValueType) //it expects the address of the value type
                            memberBinding.TargetObject = new UnaryExpression(memberBinding.TargetObject, NodeType.AddressOf, memberBinding.TargetObject.Type.GetReferenceType());
                        else
                        { //it expects a boxed copy of the value type
                            MemberBinding obType = new MemberBinding(null, memberBinding.TargetObject.Type);
                            memberBinding.TargetObject = new BinaryExpression(memberBinding.TargetObject, obType, NodeType.Box, method.DeclaringType);
                        }
                    }
                    else
                    {
                        //REVIEW: perhaps This nodes of value types should be explicitly typed as reference types
                        //TODO: assert false in that case
                    }
                }
            }
            return memberBinding;
        }
        public override Method VisitMethod(Method method)
        {
            if (method == null) return null;
            Method savedCurrentMethod = this.CurrentMethod;
            TypeNode savedCurrentType = this.CurrentType;
            this.CurrentMethod = method;
            this.CurrentType = method.DeclaringType;
            method.Body = this.VisitBlock(method.Body);
            this.CurrentMethod = savedCurrentMethod;
            this.CurrentType = savedCurrentType;
            return method;
        }
        public override Expression VisitConstruct(Construct cons)
        {
            cons = (Construct)base.VisitConstruct(cons);
            if (cons == null) return null;
            MemberBinding mb = cons.Constructor as MemberBinding;
            if (mb == null) return cons;
            Method meth = mb.BoundMember as Method;
            if (meth == null) return cons;
            ParameterList parameters = meth.Parameters;
            if (parameters == null) return cons;
            ExpressionList operands = cons.Operands;
            int n = operands == null ? 0 : operands.Count;
            if (n > parameters.Count) n = parameters.Count;
            for (int i = 0; i < n; i++)
            {
                //^ assert operands != null;
                Expression e = operands[i];
                if (e == null) continue;
                Parameter p = parameters[i];
                if (p == null) continue;
                if (e.Type == null || p.Type == null) continue;
                if (e.Type.IsValueType && !p.Type.IsValueType)
                    operands[i] = new BinaryExpression(e, new MemberBinding(null, e.Type), NodeType.Box, p.Type);
            }
            return cons;
        }
        public override Expression VisitMethodCall(MethodCall call)
        {
            call = (MethodCall)base.VisitMethodCall(call);
            if (call == null) return null;
            MemberBinding mb = call.Callee as MemberBinding;
            if (mb == null) return call;
            Method meth = mb.BoundMember as Method;
            if (meth == null) return call;
            ParameterList parameters = meth.Parameters;
            if (parameters == null) return call;
            ExpressionList operands = call.Operands;
            int n = operands == null ? 0 : operands.Count;
            if (n > parameters.Count) n = parameters.Count;
            for (int i = 0; i < n; i++)
            {
                //^ assert operands != null;
                Expression e = operands[i];
                if (e == null) continue;
                Parameter p = parameters[i];
                if (p == null) continue;
                if (e.Type == null || p.Type == null) continue;
                if (e.Type.IsValueType && !p.Type.IsValueType)
                    operands[i] = new BinaryExpression(e, new MemberBinding(null, e.Type), NodeType.Box, p.Type);
            }
            if (meth.ReturnType != null && call.Type != null && meth.ReturnType.IsValueType && !call.Type.IsValueType)
                return new BinaryExpression(call, new MemberBinding(null, meth.ReturnType), NodeType.Box, call.Type);
            return call;
        }
        public override Statement VisitReturn(Return Return)
        {
            Return = (Return)base.VisitReturn(Return);
            if (Return == null) return null;
            Expression rval = Return.Expression;
            if (rval == null || rval.Type == null || this.CurrentMethod == null || this.CurrentMethod.ReturnType == null)
                return Return;
            if (rval.Type.IsValueType && !this.CurrentMethod.ReturnType.IsValueType)
                Return.Expression = new BinaryExpression(rval, new MemberBinding(null, rval.Type), NodeType.Box, this.CurrentMethod.ReturnType);
            return Return;
        }
        public override TypeNode VisitTypeNode(TypeNode typeNode)
        {
            if (typeNode == null) return null;
            TypeNode savedCurrentType = this.CurrentType;
            this.CurrentType = typeNode;
            MemberList members = typeNode.Members;
            for (int i = 0, n = members == null ? 0 : members.Count; i < n; i++)
            {
                //^ assert members != null;
                Member mem = members[i];
                TypeNode t = mem as TypeNode;
                if (t != null) { this.VisitTypeNode(t); continue; }
                Method m = mem as Method;
                if (m != null) { this.VisitMethod(m); continue; }
            }
            this.CurrentType = savedCurrentType;
            return typeNode;
        }
        public override Expression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            if (unaryExpression == null) return null;
            return base.VisitUnaryExpression((UnaryExpression)unaryExpression.Clone());
        }
    }
#endif
}
