// Copyright (c) Microsoft Corporation.  All rights reserved.
//

// Change history:
// 03/15/2012 - EFW - Fixed GetTemplateMember() and ParametersMatch() to properly check for template
// parameters when there are method overloads in which one uses a generic type and the other does not.
// 11/30/2012 - EFW - Added updates based on changes submitted by ComponentOne to fix crashes caused by
// obfuscated member names.
// 03/11/2013 - EFW - Added more code to ParametersMatch() to try and get a proper match when comparing
// parameters lists with generic types.
// 07/31/2014 - EFW - Applied fix from Jared Moore related to generic template parameter matching under a
// specific set of conditions.
// 03/27/2015 - EFW - Fixed another issue in ParametersMatch() related to array types in method parameters
// in derived generics types.
// 08/07/2015 - EFW - Expanded upon the 03/27 fix in ParametersMatch() to handle arrays with generic array
// element types.
// 06/09/2016 - EFW - Added yet another condition to ParametersMatch() to try and match odd intrinsic/generic
// element type pairings.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection
{
    /// <summary>
    /// This class contains a set of utility and extension methods
    /// </summary>
    public static class ReflectionUtilities
    {
        #region API member helper methods
        //=====================================================================

        /// <summary>
        /// Get the namespace for the given type
        /// </summary>
        /// <param name="type">The type for which to get the namespace</param>
        /// <returns>The namespace for the given type</returns>
        public static Namespace GetNamespace(this TypeNode type)
        {
            if(type.DeclaringType != null)
                return GetNamespace(type.DeclaringType);

            return new Namespace(type.Namespace);
        }

        /// <summary>
        /// Get the template type for the given type
        /// </summary>
        /// <param name="type">The type for which to get the template type</param>
        /// <returns>The type itself if it is not generic or the template type if it is</returns>
        public static TypeNode GetTemplateType(this TypeNode type)
        {
            if(type == null)
                throw new ArgumentNullException("type");

            // Only generic types have templates
            if(!type.IsGeneric)
                return type;

            // If the type is not nested, life is simpler.
            if(type.DeclaringType == null)
            {
                // If the type is not specified, the type is the template
                if(type.TemplateArguments == null)
                    return type;

                // Otherwise, construct the template type identifier and use it to fetch the template type
                Identifier name = new Identifier(String.Format(CultureInfo.InvariantCulture, "{0}`{1}",
                    type.GetUnmangledNameWithoutTypeParameters(), type.TemplateArguments.Count));

                TypeNode template = type.DeclaringModule.GetType(type.Namespace, name);

                // !EFW - Added by ComponentOne
                if(template == null)
                    template = type;

                return template;
            }

            // If the type is nested, life is harder.  We have to walk up the chain, constructing unspecialized
            // identifiers as we go, then walk back down the chain, fetching template types as we go.

            // Create a stack to keep track of identifiers
            Stack<Identifier> identifiers = new Stack<Identifier>();

            // Populate the stack with the identifiers of all the types up to the outermost type
            TypeNode current = type;

            while(true)
            {
                int count = 0;

                if(current.TemplateArguments != null && current.TemplateArguments.Count > count)
                    count = current.TemplateArguments.Count;

                if(current.TemplateParameters != null && current.TemplateParameters.Count > count)
                    count = current.TemplateParameters.Count;

                if(count == 0)
                    identifiers.Push(new Identifier(current.GetUnmangledNameWithoutTypeParameters()));
                else
                    identifiers.Push(new Identifier(String.Format(CultureInfo.InvariantCulture, "{0}`{1}",
                        current.GetUnmangledNameWithoutTypeParameters(), count)));

                if(current.DeclaringType == null)
                    break;

                current = current.DeclaringType;
            }

            // Fetch a TypeNode representing the outermost type
            current = current.DeclaringModule.GetType(current.Namespace, identifiers.Pop());

            // Move down the stack to the inner type we want
            while(identifiers.Count > 0 && current != null)
                current = (TypeNode)current.GetMembersNamed(identifiers.Pop()).FirstOrDefault();

            // !EFW - Added by ComponentOne
            if(current == null)
                return type;

            // Whew, finally we've got it
            return current;
        }

        /// <summary>
        /// This is used to get an enumerable list of implemented properties
        /// </summary>
        /// <param name="property">The property from which to get the implemented properties</param>
        /// <returns>An enumerable list of implemented properties</returns>
        public static IEnumerable<Property> GetImplementedProperties(this Property property)
        {
            // Get an accessor
            Method accessor = property.Getter;

            if(accessor == null)
                accessor = property.Setter;

            if(accessor != null)
            {
                // Get the interface methods corresponding to this accessor and look for properties corresponding
                // to these methods
                foreach(var method in accessor.GetImplementedMethods())
                {
                    Property entry = method.GetPropertyFromAccessor();

                    if(entry != null)
                        yield return entry;
                }
            }
        }

        /// <summary>
        /// This is used to get an enumerable list of implemented events
        /// </summary>
        /// <param name="trigger">The event from which to get the implemented events</param>
        /// <returns>An enumerable list of implemented events</returns>
        public static IEnumerable<Event> GetImplementedEvents(this Event trigger)
        {
            // Get interface methods corresponding to this adder and then get the events corresponding to the
            // implemented adders.
            foreach(Method implementedAdder in trigger.HandlerAdder.GetImplementedMethods())
            {
                Event implementedTrigger = implementedAdder.GetEventFromAdder();

                if(implementedTrigger != null)
                    yield return implementedTrigger;
            }
        }

        /// <summary>
        /// This is used to get an enumerable list of implemented methods
        /// </summary>
        /// <param name="method">The method from which to get the implemented methods</param>
        /// <returns>An enumerable list of implemented methods</returns>
        public static IEnumerable<Method> GetImplementedMethods(this Method method)
        {
            // Return explicit implementations first if any
            MethodList explicitImplementations = method.ImplementedInterfaceMethods;

            if(explicitImplementations != null)
                foreach(var explicitImplementation in explicitImplementations)
                    yield return explicitImplementation;

            // Then return implicit implementations if any
            MethodList implicitImplementations = method.ImplicitlyImplementedInterfaceMethods;

            if(implicitImplementations != null)
                foreach(var implicitImplementation in implicitImplementations)
                    yield return implicitImplementation;
        }

        /// <summary>
        /// Get the template member for the given member
        /// </summary>
        /// <param name="member">The member for which to get the template member</param>
        /// <returns>The template member for the given member.  This will be the member itself if it is not
        /// generic or it is not specialized.</returns>
        public static Member GetTemplateMember(this Member member)
        {
            if(member == null)
                throw new ArgumentNullException("member");

            // If the containing type isn't generic, the member is the template member
            TypeNode type = member.DeclaringType;

            if(!type.IsGeneric)
                return member;

            // If the containing type isn't specialized, the member is the template member
            if(!type.IsSpecialized())
                return member;

            // Get the template type and look for members with the same name
            TypeNode template = member.DeclaringType.GetTemplateType();
            MemberList candidates = template.GetMembersNamed(member.Name);

            // If no candidates, say so (this shouldn't happen)
            if(candidates.Count == 0)
                throw new InvalidOperationException("No members in the template had the name found in the " +
                    "specialization.  This is not possible but apparently it happened.");

            // If only one candidate, return it
            if(candidates.Count == 1)
                return candidates[0];

            // Multiple candidates, so now we need to compare parameters
            ParameterList parameters = member.GetParameters();

            // !EFW - If there are no generic parameters in this method, check for an exact match
            // first ignoring all methods with generic parameters.  This prevents it matching a
            // generic overload of the method prematurely and returning the wrong one as the match.
            bool hasNoGenericParams = true;

            foreach(var p in parameters)
                if(p.Type.IsTemplateParameter)
                {
                    hasNoGenericParams = false;
                    break;
                }

            if(hasNoGenericParams)
                for(int i = 0; i < candidates.Count; i++)
                {
                    Member candidate = candidates[i];

                    // Candidate must be same kind of node
                    if(candidate.NodeType != member.NodeType)
                        continue;

                    // Match exactly, failing if compared to a version with generic parameters
                    if(ParametersMatch(parameters, candidate.GetParameters(), true, false))
                        return candidate;
                }

            for(int i = 0; i < candidates.Count; i++)
            {
                Member candidate = candidates[i];

                // Candidate must be same kind of node
                if(candidate.NodeType != member.NodeType)
                    continue;

                // Allow matches to versions with generic parameters
                if(ParametersMatch(parameters, candidate.GetParameters(), false, false))
                    return candidate;
            }

            // !EFW - If we get here, it's probably some really complicated signature involving multiple
            // generic types and/or mixes of intrinsic array types and generic template parameter array types.
            // So, give it one final last ditch attempt allowing intrinsic array types to match template
            // parameter array types.  If that fails, we will give up.  If there's a better way to do this, I'm
            // not aware of it.  https://github.com/EWSoftware/SHFB/issues/302
            for(int i = 0; i < candidates.Count; i++)
            {
                Member candidate = candidates[i];

                // Candidate must be same kind of node
                if(candidate.NodeType != member.NodeType)
                    continue;

                // Allow matches to versions with generic parameters
                if(ParametersMatch(parameters, candidate.GetParameters(), false, true))
                    return candidate;
            }

            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "{0}\r\n{1}\r\n" +
                "No members in the template matched the parameters of the specialization.  This is not " +
                "possible but apparently it happened.", member.DeclaringType.FullName, member.FullName));
        }

        /// <summary>
        /// This is used to see if the given member is the default member
        /// </summary>
        /// <param name="member">The member to check</param>
        /// <returns>True if it is the default member, false if not</returns>
        public static bool IsDefaultMember(this Member member)
        {
            if(member == null)
                throw new ArgumentNullException("member");

            return member.DeclaringType.DefaultMembers.Any(m => m == member);
        }

        /// <summary>
        /// Get an event from an adder method
        /// </summary>
        /// <param name="adder">The adder method from which to get the event</param>
        /// <returns>The event for the method or null if there isn't one</returns>
        private static Event GetEventFromAdder(this Method adder)
        {
            if(adder == null)
                throw new ArgumentNullException("adder");

            return adder.DeclaringType.Members.FirstOrDefault(m => m.NodeType == NodeType.Event &&
                ((Event)m).HandlerAdder == adder) as Event;
        }

        /// <summary>
        /// Get the parameters from the given member
        /// </summary>
        /// <param name="member">The member from which to get the parameters</param>
        /// <returns>An list of parameters</returns>
        private static ParameterList GetParameters(this Member member)
        {
            Method method = member as Method;

            if(method != null)
                return method.Parameters;

            Property property = member as Property;

            if(property != null)
                return property.Parameters;

            return new ParameterList();
        }

        /// <summary>
        /// Get a property from an accessor method
        /// </summary>
        /// <param name="accessor">The accessor method from which to get the property</param>
        /// <returns>The property for the method or null if there isn't one</returns>
        private static Property GetPropertyFromAccessor(this Method accessor)
        {
            if(accessor == null)
                throw new ArgumentNullException("accessor");

            return accessor.DeclaringType.Members.FirstOrDefault(m =>
            {
                Property p = m as Property;

                return (m.NodeType == NodeType.Property && (p.Getter == accessor || p.Setter == accessor));
            }) as Property;
        }

        /// <summary>
        /// This is used to determine if a type or any of its containing types is specialized
        /// </summary>
        /// <param name="type">The type to check for specialization</param>
        /// <returns>True if the type is specialized (it has template arguments), false if not</returns>
        private static bool IsSpecialized(this TypeNode type)
        {
            for(TypeNode t = type; t != null; t = t.DeclaringType)
            {
                TypeNodeList templates = t.TemplateArguments;

                if(templates != null && templates.Count > 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// See if the given sets of parameters match each other
        /// </summary>
        /// <param name="parameters1">The first set of parameters</param>
        /// <param name="parameters2">The second set of parameters</param>
        /// <param name="noMatchOnGenericVersions">True to fail the match if either parameter is
        /// generic, false to allow matching to generic parameters even if the other isn't.</param>
        /// <returns>True if they match, false if not</returns>
        /// <param name="allowMismatchedArrayTypes">True to allow a match with mismatched array types or false to
        /// not allow a match.  If true, we're getting pretty desperate for a match.</param>
        /// <remarks>When <paramref name="noMatchOnGenericVersions"/> is true, it prevents matching a
        /// non-generic overload of the method to a generic version of the method.  This allows the
        /// non-generic version to be matched correctly (i.e. Contains(T) and Contains(Guid)).  If not
        /// done, the generic version is matched to both methods and the reflection info contains a
        /// duplicate generic method and loses the non-generic overload.</remarks>
        private static bool ParametersMatch(ParameterList parameters1, ParameterList parameters2,
          bool noMatchOnGenericVersions, bool allowMismatchedArrayTypes)
        {
            if(parameters1.Count != parameters2.Count)
                return false;

            for(int i = 0; i < parameters1.Count; i++)
            {
                TypeNode type1 = parameters1[i].Type;
                TypeNode type2 = parameters2[i].Type;

                // !EFW - Fail the match if we are looking for a non-generic match
                if(noMatchOnGenericVersions && (type1.IsTemplateParameter || type2.IsTemplateParameter))
                    return false;

                // We can't determine the equivalence of template parameters; this is probably not good
                if(type1.IsTemplateParameter || type2.IsTemplateParameter)
                {
                    // !EFW - As a fallback, compare the type parameter positions.  If they don't match, this
                    // probably isn't the one we want.
                    int p1 = GetTemplateParameterPosition(parameters1[i].DeclaringMethod.DeclaringType, type1.Name.Name),
                        p2 = GetTemplateParameterPosition(parameters2[i].DeclaringMethod.DeclaringType, type2.Name.Name);

                    if(p1 != -1 && p2 != -1 && p1 != p2)
                    {
                        // !EFW - Another test case supplied by Jared Moore.  If the types are something like
                        // MyBaseClass<T, T> and MyBaseClass<T, U> we can still provide a match by comparing
                        // all possible positions.  As long as they intersect, it's probably a good match.
                        var positions1 = GetTemplateParameterPositions(parameters1[i].DeclaringMethod.DeclaringType,
                            type1.Name.Name);
                        var positions2 = GetTemplateParameterPositions(parameters2[i].DeclaringMethod.DeclaringType,
                            type2.Name.Name);

                        // If we found any but none of them are the same, then no match.
                        if(positions1.Any() && positions2.Any() && !positions1.Intersect(positions2).Any())
                            return false;
                    }
                }
                else
                {
                    // The node type must be the same; this is probably a fast check
                    if(type1.NodeType != type2.NodeType)
                        return false;

                    // If they are "normal" types, we will compare them.  Comparing arrays, pointers, etc. is
                    // dangerous, because the types they contain may be template parameters
                    if(type1.NodeType == NodeType.Class || type1.NodeType == NodeType.Struct ||
                      type1.NodeType == NodeType.Interface || type1.NodeType == NodeType.EnumNode ||
                      type1.NodeType == NodeType.DelegateNode)
                    {
                        type1 = type1.GetTemplateType();
                        type2 = type2.GetTemplateType();

                        if(!type2.IsStructurallyEquivalentTo(type1))
                            return false;
                    }

                    // !EFW - Comparing array types may be dangerous but, as it turns out, is necessary.  If
                    // two overloads take an array as a parameter, it always returns the first overload as the
                    // match in derived types.  As such, we do need to compare the array element types.  For
                    // generic types, we can get the underlying template type from the declaring method's type
                    // and match that.
                    // https://github.com/EWSoftware/SHFB/issues/57
                    if(type1.NodeType == NodeType.ArrayType)
                    {
                        type1 = ((ArrayType)type1).ElementType;
                        type2 = ((ArrayType)type2).ElementType;

                        if(type2.IsTemplateParameter)
                        {
                            // Get the position from the second set of parameters
                            int pos = GetTemplateParameterPosition(parameters2[i].DeclaringMethod.DeclaringType,
                                type2.Name.Name);

                            // Get the actual type from the first set of parameters
                            var declType = parameters1[i].DeclaringMethod.DeclaringType;

                            if(pos != -1 && declType.TemplateArguments != null && pos < declType.TemplateArguments.Count)
                                type2 = declType.TemplateArguments[pos];
                        }

                        if(type1.NodeType != type2.NodeType || !type2.IsStructurallyEquivalentTo(type1))
                        {
                            // !EFW - Yet another edge case to check.  In this case for example,
                            // KeyValue<int, int> didn't match KeyValue<TKey, TValue> and it failed to find any
                            // matches.  The fix is to see if both types are generic and compare the template
                            // parameter names.  This is getting rather complicated isn't it?
                            // https://github.com/EWSoftware/SHFB/issues/154
                            if(!type1.IsGeneric || !type2.IsGeneric || type1.Template == null || type2.Template == null ||
                              type1.Template.TemplateParameters.Count != type2.Template.TemplateParameters.Count ||
                              type1.Template.TemplateParameters.Select(t => t.Name.Name).Except(
                              type2.Template.TemplateParameters.Select(t => t.Name.Name)).Count() != 0)
                            {
                                // If this is the last ditch attempt and were allowing mismatched array types,
                                // we're pretty much screwed so carry on.  This can happen in some really
                                // complex cases were we end up with an intrinsic type and a template parameter:
                                // https://github.com/EWSoftware/SHFB/issues/302
                                if(!allowMismatchedArrayTypes || type1.StructuralElementTypes == null ||
                                  type2.StructuralElementTypes == null || type1.StructuralElementTypes.Count == 0 ||
                                  type2.StructuralElementTypes.Count == 0 ||
                                  type1.StructuralElementTypes[0].IsTemplateParameter == type2.StructuralElementTypes[0].IsTemplateParameter)
                                    return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// !EFW - Get the position of the given parameter name in the given declaring type's template
        /// arguments or parameters.
        /// </summary>
        /// <param name="declaringType">The declaring type from which to get the parameter position</param>
        /// <param name="name">The parameter name to look up</param>
        /// <returns>-1 if not found or a non-negative position if it was found</returns>
        private static int GetTemplateParameterPosition(TypeNode declaringType, string name)
        {
            int position = -1;

            if(declaringType.TemplateArguments != null)
            {
                var arg = declaringType.TemplateArguments.FirstOrDefault(a => a.Name.Name == name);

                if(arg != null)
                    position = declaringType.TemplateArguments.IndexOf(arg);
            }

            if(position == -1 && declaringType.TemplateParameters != null)
            {
                var param = declaringType.TemplateParameters.FirstOrDefault(p => p.Name.Name == name);

                if(param != null)
                    position = declaringType.TemplateParameters.IndexOf(param);
            }

            return position;
        }

        /// <summary>
        /// !EFW - Get the positions of the given parameter name in the given declaring type's template
        /// arguments or parameters.
        /// </summary>
        /// <param name="declaringType">The declaring type from which to get the parameter position</param>
        /// <param name="name">The parameter name to look up</param>
        /// <returns>An enumerable list of the possible parameter positions or an empty list if no positions
        /// match.</returns>
        private static IEnumerable<int> GetTemplateParameterPositions(TypeNode declaringType, string name)
        {
            List<int> positions = new List<int>();

            if(declaringType.TemplateArguments != null)
            {
                // For types like "MyChildClass<T> : MyBaseClass<T, T>", the parameter list index and item is
                // identical for both arguments so we've got to figure it out the long way positionally.
                for(int i = 0; i < declaringType.TemplateArguments.Count; i++)
                    if(declaringType.TemplateArguments[i].Name.Name == name)
                        positions.Add(i);
            }

            if(positions.Count == 0 && declaringType.TemplateParameters != null)
            {
                for(int i = 0; i < declaringType.TemplateParameters.Count; i++)
                    if(declaringType.TemplateParameters[i].Name.Name == name)
                        positions.Add(i);
            }

            return positions;
        }        
        #endregion

        #region XML character checking methods
        //=====================================================================

        // EFW - Submitted by ComponentOne.  These are used to prevent crashes caused by obfuscated member names
        private static Regex reBadXmlChars = new Regex("[^\u0020-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]");

        /// <summary>
        /// This is used to check for bad XML characters in a member name
        /// </summary>
        /// <param name="name">The member name to check</param>
        /// <returns>True if the name contains bad characters, false if not</returns>
        public static bool HasInvalidXmlCharacters(this string name)
        {
            if(String.IsNullOrEmpty(name))
                return false;

            return reBadXmlChars.IsMatch(name);
        }

        /// <summary>
        /// This is used to translate a value, replacing bad XML characters with their hex equivalent
        /// </summary>
        /// <param name="translateValue">The value to check and translate</param>
        /// <returns>The unchanged value if it contains no bad characters or the translated value if it does
        /// contain bad characters.</returns>
        /// <remarks>This prevents crashes caused by obfuscated member names and encrypted values</remarks>
        public static string TranslateToValidXmlValue(this string translateValue)
        {
            if(String.IsNullOrEmpty(translateValue) || !reBadXmlChars.IsMatch(translateValue))
                return translateValue;

            return reBadXmlChars.Replace(translateValue, m => ((int)m.Value[0]).ToString("X2", CultureInfo.InvariantCulture));
        }
        #endregion
    }
}
