// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 11/21/2013 - EFW - Cleared out the conditional statements and updated based on changes to ListTemplate.cs.

using System.Diagnostics;

namespace System.Compiler
{
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
    public class Specializer : StandardVisitor
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

        public Specializer(Visitor callingVisitor)
            : base(callingVisitor)
        {
        }

        public override void TransferStateTo(Visitor targetVisitor)
        {
            base.TransferStateTo(targetVisitor);

            if(!(targetVisitor is Specializer target))
                return;
            
            target.args = this.args;
            target.pars = this.pars;
            target.CurrentMethod = this.CurrentMethod;
            target.CurrentType = this.CurrentType;
        }

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
            if(member == null)
                return null;

            TypeNode specializedType = this.VisitTypeReference(member.DeclaringType);

            if(specializedType == member.DeclaringType || specializedType == null)
                return member;

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
            method.ImplementedTypes = this.VisitTypeReferenceList(method.ImplementedTypes);
            method.Parameters = this.VisitParameterList(method.Parameters);

            if(TargetPlatform.UseGenerics && this.args != method.TemplateArguments)
            {
                method.TemplateArguments = this.VisitTypeReferenceList(method.TemplateArguments);
                method.TemplateParameters = this.VisitTypeParameterList(method.TemplateParameters);
            }

            method.ImplementedInterfaceMethods = this.VisitMethodList(method.ImplementedInterfaceMethods);
            this.CurrentMethod = savedCurrentMethod;
            this.CurrentType = savedCurrentType;
            return method;
        }

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
            if(typeNode == null)
                return null;

            TypeNode savedCurrentType = this.CurrentType;

            if(savedCurrentType != null && savedCurrentType.TemplateArguments != null && savedCurrentType.TemplateArguments.Count > 0 &&
              typeNode.Template != null && (typeNode.Template.TemplateParameters == null || typeNode.Template.TemplateParameters.Count == 0))
            {
                typeNode.TemplateArguments = new TypeNodeList();
            }

            this.CurrentType = typeNode;
            typeNode.Attributes = this.VisitAttributeList(typeNode.Attributes);
            typeNode.SecurityAttributes = this.VisitSecurityAttributeList(typeNode.SecurityAttributes);
            Class c = typeNode as Class;

            if(c != null)
                c.BaseClass = (Class)this.VisitTypeReference(c.BaseClass);

            typeNode.Interfaces = this.VisitInterfaceReferenceList(typeNode.Interfaces);

            if(typeNode.ProvideTypeMembers != null && typeNode.ProvideNestedTypes != null && typeNode.ProviderHandle != null)
            {
                typeNode.members = null;
                typeNode.ProviderHandle = new SpecializerHandle(typeNode.ProvideNestedTypes, typeNode.ProvideTypeMembers, typeNode.ProviderHandle);
                typeNode.ProvideNestedTypes = new TypeNode.NestedTypeProvider(this.ProvideNestedTypes);
                typeNode.ProvideTypeMembers = new TypeNode.TypeMemberProvider(this.ProvideTypeMembers);

                DelegateNode delegateNode = typeNode as DelegateNode;

                if(delegateNode != null)
                {
                    if(!delegateNode.IsNormalized)
                    { //In the Normalized case Parameters are retrieved from the Invoke method, which means evaluating Members
                        delegateNode.Parameters = this.VisitParameterList(delegateNode.Parameters);
                        delegateNode.ReturnType = this.VisitTypeReference(delegateNode.ReturnType);
                    }
                }
            }
            else
            {
                typeNode.Members = this.VisitMemberList(typeNode.Members);
                DelegateNode delegateNode = typeNode as DelegateNode;

                if(delegateNode != null)
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
            
            if(expr is Identifier id)
            {
                int key = id.UniqueIdKey;
                for(int i = 0, n = pars == null ? 0 : pars.Count, m = args == null ? 0 : args.Count; i < n && i < m; i++)
                {
                    //^ assert pars != null && args != null;
                    TypeNode par = pars[i];
                    if(par == null || par.Name == null)
                        continue;
                    if(par.Name.UniqueIdKey == key)
                        return new Literal(args[i], CoreSystemTypes.Type);
                }
                return id;
            }

            Debug.Assert(expr is QualifiedIdentifier || expr is Literal);

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
                result = new ClassParameter
                {
                    DeclaringType = typeParameter.DeclaringType
                };
            }
            else
                return typeParameter; //give up

            result.SourceContext = typeParameter.SourceContext;
            result.TypeParameterFlags = ((ITypeParameter)typeParameter).TypeParameterFlags;

            result.Name = typeParameter.Name;
            result.Namespace = StandardIds.ClassParameter;
            result.BaseClass = baseType is Class c ? c : CoreSystemTypes.Object;
            result.DeclaringMember = ((ITypeParameter)typeParameter).DeclaringMember;
            result.DeclaringModule = typeParameter.DeclaringModule;
            result.Flags = typeParameter.Flags & ~TypeFlags.Interface;

            //InterfaceList constraints = result.Interfaces = new InterfaceList();
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

                case NodeType.DelegateNode:
                    {
                        FunctionType ftype = type as FunctionType;
                        if (ftype == null) goto default;
                        TypeNode referringType = ftype.DeclaringType == null ? this.CurrentType : this.VisitTypeReference(ftype.DeclaringType);
                        return FunctionType.For(this.VisitTypeReference(ftype.ReturnType), this.VisitParameterList(ftype.Parameters), referringType);
                    }

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
                        
                        //Could happen if the expression is a template parameter
                        if(cExpr.Expression is Literal lit)
                            return lit.Value as TypeNode;
                        
                        cExpr.TemplateArguments = this.VisitTypeReferenceList(cExpr.TemplateArguments);
                        return cExpr;
                    }

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

                case NodeType.InterfaceExpression:
                    InterfaceExpression iExpr = (InterfaceExpression)type;
                    if (iExpr.Expression == null) goto default;
                    iExpr.Expression = this.VisitTypeExpression(iExpr.Expression);
                    iExpr.TemplateArguments = this.VisitTypeReferenceList(iExpr.TemplateArguments);
                    return iExpr;

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

                case NodeType.OptionalModifier:
                    {
                        TypeModifier modType = (TypeModifier)type;
                        TypeNode modifiedType = this.VisitTypeReference(modType.ModifiedType);
                        TypeNode modifierType = this.VisitTypeReference(modType.Modifier);
                        if (modifiedType == null || modifierType == null) { return type; }

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

                    if(type.Template != null && (type.ConsolidatedTemplateParameters == null || type.ConsolidatedTemplateParameters.Count == 0))
                    {
                        if(!type.IsNotFullySpecialized && (!type.IsNormalized ||
                         (this.CurrentType != null && type.DeclaringModule == this.CurrentType.DeclaringModule)))
                        {
                            return type;
                        }

                        // Type is a template instance, but some of its arguments were themselves parameters.
                        // See if any of these parameters are to be specialized by this specializer.
                        bool mustSpecializeFurther = false;
                        TypeNodeList targs = type.TemplateArguments;
                        int numArgs = targs == null ? 0 : targs.Count;

                        if(targs != null)
                        {
                            targs = new TypeNodeList(targs);

                            for(int i = 0; i < numArgs; i++)
                            {
                                TypeNode targ = targs[i];

                                if(targ is ITypeParameter tparg)
                                {
                                    for(int j = 0, np = pars == null ? 0 : pars.Count, m = args == null ? 0 : args.Count; j < np && j < m; j++)
                                    {
                                        //^ assert pars != null && args != null;
                                        if(TargetPlatform.UseGenerics)
                                        {
                                            if(!(pars[j] is ITypeParameter par))
                                                continue;

                                            if(tparg == par || (tparg.ParameterListIndex == par.ParameterListIndex && tparg.DeclaringMember == par.DeclaringMember))
                                            {
                                                targ = this.args[j];
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if(targ == pars[j])
                                            {
                                                targ = this.args[j];
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if(targ != type)
                                        targ = this.VisitTypeReference(targ);

                                    if(targ == type)
                                        continue;
                                }

                                mustSpecializeFurther |= targs[i] != targ;
                                targs[i] = targ;
                            }
                        }

                        if(targs == null || !mustSpecializeFurther)
                            return type;

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
}
