// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 03/28/2012 - EFW - Fixed Contains() so that when checking for matching members it compares generic
// template parameters by name to match members with generic parameters correctly.
// 11/25/2013 - EFW - Cleaned up the code and removed unused members.  Added support for the visibility options
// in the API filter.
// 11/18/2014 - EFW - Added a hack to work around a unique case using "new" to re-implement interface methods
// 08/28/2017 - EFW - Updated the constructor to remove virtual/overridden members from base types when an
// overridden member is excluded via the API filter.
// 10/05/2021 - EFW - Added support for excluding internal members inherited from base types in other assemblies
// and private members from base types.

using System;
using System.Collections;
using System.Collections.Generic;

using System.Compiler;

using Sandcastle.Tools.Reflection;

namespace Sandcastle.Tools
{
    public sealed class MemberDictionary : ICollection<Member>
    {
        #region Private data members
        //=====================================================================

        private readonly Dictionary<string, List<Member>> index;
        private readonly TypeNode type;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type for which the dictionary is created</param>
        /// <param name="filter">The API filter used to exclude unwanted members</param>
        public MemberDictionary(TypeNode type, ApiFilter filter)
        {
            if(filter == null)
                throw new ArgumentNullException(nameof(filter));

            this.type = type ?? throw new ArgumentNullException(nameof(type));

            index = new Dictionary<string, List<Member>>();

            // !EFW - Track excluded overridden members.  These need to be filtered out below or the inherited
            // base member shows up in the member list.
            var excludedOverriddenMembers = new List<Member>();

            // Add all member of the type except nested types and members that the filter rejects
            foreach(var member in type.Members)
                if(!(member is TypeNode) && filter.IsExposedMember(member))
                    this.AddMember(member);
                else
                    if(member.OverridesBaseClassMember)
                        excludedOverriddenMembers.Add(member.OverriddenMember);

            // For enumerations, don't list inherited members
            if(type is EnumNode)
                return;

            // For interfaces, list members of inherited interfaces
            if(type is Interface && filter.IncludeInheritedMembers)
            {
                var derivedMembers = new HashSet<string>();
                string nameAndParams;
                int pos;

                // !EFW - This is a hack to filter out duplicate members by name and parameters for cases where
                // an interface member in the derived type uses the "new" keyword to re-implement a member using
                // the same name as in the base type.  In such cases, the member is not truly hidden as it is in
                // a class since it still needs to be explicitly implemented when derived from but we don't want
                // to see it duplicated below as part of the inherited members.
                foreach(var m in this)
                {
                    pos = m.FullName.LastIndexOf('(');

                    if(pos != -1)
                        pos = m.FullName.LastIndexOf('.', pos, pos);
                    else
                        pos = m.FullName.LastIndexOf('.');

                    if(pos != -1)
                        derivedMembers.Add(m.FullName.Substring(pos));
                    else
                        derivedMembers.Add(m.FullName);
                }

                foreach(var contract in type.Interfaces)
                {
                    // Members of hidden interfaces don't count
                    if(filter.IsExposedType(contract))
                    {
                        // Otherwise, add inherited interface members except those rejected by the filters.  This
                        // is necessary to remove accessor methods.
                        foreach(var contractMember in contract.Members)
                        {
                            pos = contractMember.FullName.LastIndexOf('(');

                            if(pos != -1)
                                pos = contractMember.FullName.LastIndexOf('.', pos, pos);
                            else
                                pos = contractMember.FullName.LastIndexOf('.');

                            if(pos != -1)
                                nameAndParams = contractMember.FullName.Substring(pos);
                            else
                                nameAndParams = contractMember.FullName;

                            if(!filter.IsExcludedFrameworkMember(type, contractMember) &&
                              filter.IsExposedMember(contractMember) && !derivedMembers.Contains(nameAndParams))
                            {
                                this.AddMember(contractMember);
                                derivedMembers.Add(nameAndParams);
                            }
                        }
                    }
                }

                return;
            }

            // Don't list inherited members for static classes
            if(type.IsAbstract && type.IsSealed)
                return;

            // If not including inherited members, don't go any further
            if(!filter.IncludeInheritedMembers)
                return;

            // Now iterate up through the type hierarchy
            for(TypeNode parentType = type.BaseType; parentType != null; parentType = parentType.BaseType)
                foreach(var parentMember in parentType.Members)
                {
                    // Don't add constructors
                    if(parentMember.NodeType == NodeType.InstanceInitializer ||
                      parentMember.NodeType == NodeType.StaticInitializer)
                        continue;

                    // Don't add inherited static members
                    if(parentMember.IsStatic)
                        continue;

                    // Don't add nested types
                    if(parentMember is TypeNode)
                        continue;

                    // Don't add protected members if the derived type is sealed and they are not wanted
                    if(!filter.IncludeSealedProtected && type.IsSealed && (parentMember.IsFamily ||
                      parentMember.IsFamilyOrAssembly))
                        continue;

                    // Don't add members that the filter rejects
                    if(filter.IsExcludedFrameworkMember(type, parentMember) || !filter.IsExposedMember(parentMember))
                    {
                        if(parentMember.OverridesBaseClassMember)
                            excludedOverriddenMembers.Add(parentMember.OverriddenMember);

                        continue;
                    }

                    // Don't add members we have overridden
                    if(this.Contains(parentMember))
                        continue;

                    // Don't add virtual members that have had their overridden counterparts excluded
                    if(excludedOverriddenMembers.Contains(parentMember))
                    {
                        if(parentMember.OverridesBaseClassMember)
                            excludedOverriddenMembers.Add(parentMember.OverriddenMember);

                        continue;
                    }

                    // If not wanted, exclude internal members inherited from types in other assemblies and
                    // private members of base classes.  Inherited framework internal/private members are still
                    // included as they are controlled by other visibility settings.
                    if(((parentMember.IsAssembly || parentMember.IsFamilyAndAssembly) &&
                      parentMember.DeclaringType.DeclaringModule.Name != type.DeclaringModule.Name) ||
                      (parentMember.IsPrivate && parentMember.DeclaringType.Name.Name != type.Name.Name))
                    {
                        string memberNamespace = parentMember.DeclaringType.Namespace.Name;

                        if(!filter.InternalAndPrivateIfExternal &&
                          memberNamespace != "System" && memberNamespace != "Microsoft" &&
                          !memberNamespace.StartsWith("System.", StringComparison.Ordinal) &&
                          !memberNamespace.StartsWith("Microsoft.", StringComparison.Ordinal))
                        {
                            continue;
                        }
                    }

                    // Otherwise, add the member 
                    this.AddMember(parentMember);
                }
        }
        #endregion

        #region ICollection<Member> Members
        //=====================================================================

        /// <summary>
        /// This member is not implemented and should not be called
        /// </summary>
        /// <param name="member">Not used</param>
        void ICollection<Member>.Add(Member member)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This member is not implemented and should not be called
        /// </summary>
        void ICollection<Member>.Clear()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Contains(Member member)
        {
            if(member == null)
                throw new ArgumentNullException(nameof(member));

            // Get candidate members with the same name
            if(!index.TryGetValue(member.Name.Name, out List<Member> candidates))
                return false;

            // Iterate over the candidates looking for one of the same type with the same parameters
            ParameterList parameters = GetParameters(member);

            foreach(Member candidate in candidates)
            {
                // Candidates must be of the same type
                if(candidate.NodeType != member.NodeType)
                    continue;

                // Get candidate parameters
                ParameterList candidateParameters = GetParameters(candidate);

                // Number of parameters must match
                if(parameters.Count != candidateParameters.Count)
                    continue;

                // Each parameter type must match
                bool parameterMismatch = false;

                for(int i = 0; i < parameters.Count; i++)
                    if(parameters[i].Type != candidateParameters[i].Type)
                    {
                        // !EFW - Template parameters always cause a mismatch here and it can cause an
                        // overridden member to be included incorrectly.  So, if either type is not a
                        // template parameter or, if both are template parameters and the names don't
                        // match, consider it a mismatch.  If not, carry on.  It's not perfect but it
                        // should work for most cases.
                        if(!parameters[i].Type.IsTemplateParameter ||
                          !candidateParameters[i].Type.IsTemplateParameter ||
                          parameters[i].Type.FullName != candidateParameters[i].Type.FullName)
                        {
                            parameterMismatch = true;
                            break;
                        }
                    }

                // If the parameters match, we have the member
                if(!parameterMismatch)
                    return true;
            }

            // No candidates matched
            return false;
        }

        /// <summary>
        /// This member is not implemented and should not be called
        /// </summary>
        /// <param name="array">Not used</param>
        /// <param name="index">Not used</param>
        void ICollection<Member>.CopyTo(Member[] array, int index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        /// <remarks>Note that the count returned is the sum of all members including overloads, inherited
        /// members, etc.</remarks>
        public int Count
        {
            get
            {
                int count = 0;

                foreach(var entries in index.Values)
                    count += entries.Count;

                return count;
            }
        }

        /// <summary>
        /// This always returns true
        /// </summary>
        bool ICollection<Member>.IsReadOnly => true;

        /// <summary>
        /// This member is not implemented and should not be called
        /// </summary>
        /// <param name="member">Not used</param>
        bool ICollection<Member>.Remove(Member member)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEnumerable<Member> Members
        //=====================================================================

        /// <inheritdoc />
        public IEnumerator<Member> GetEnumerator()
        {
            return this.AllMembers.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        //=====================================================================

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.AllMembers.GetEnumerator();
        }
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the type information
        /// </summary>
        public TypeNode Type => type;

        /// <summary>
        /// This read-only property returns an enumerable list of all members in the dictionary
        /// </summary>
        /// <remarks>There may be multiple members for each member name in the dictionary</remarks>
        public IEnumerable<Member> AllMembers
        {
            get
            {
                foreach(var entries in index.Values)
                    foreach(var entry in entries)
                        yield return entry;
            }
        }

        /// <summary>
        /// This read-only property returns an enumerable list of member names from the dictionary
        /// </summary>
        public IEnumerable<string> MemberNames => index.Keys;

        /// <summary>
        /// Item indexer.  This returns an enumerable list of all members for the matching name
        /// </summary>
        /// <param name="name">The member name to find</param>
        /// <returns>An enumerable list of members for the given name or an empty enumeration if a match was not
        /// found.</returns>
        public IEnumerable<Member> this[string name]
        {
            get
            {
                index.TryGetValue(name, out List<Member> members);

                if(members != null)
                    foreach(var m in members)
                        yield return m;
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to get the parameters from a member if applicable
        /// </summary>
        /// <param name="member">The member from which to get the parameters</param>
        /// <returns>The parameter list if one exists or an empty parameter list if there are no parameters</returns>
        private static ParameterList GetParameters(Member member)
        {
            // If the member is a method, get it's parameter list
            if(member is Method method)
                return method.Parameters;

            // If the member is a property, get it's parameter list
            if(member is Property property)
                return property.Parameters;

            // Member is neither a method nor a property
            return new ParameterList();
        }

        /// <summary>
        /// Add a new member to the dictionary
        /// </summary>
        /// <param name="member">The member to add</param>
        private void AddMember(Member member)
        {
            string name = member.Name.Name;

            // Look up the member list for the name
            if(!index.TryGetValue(name, out List<Member> members))
            {
                // If there isn't one already, make one
                members = new List<Member>();
                index.Add(name, members);
            }

            // Add the member to the list
            members.Add(member);
        }
        #endregion
    }
}
