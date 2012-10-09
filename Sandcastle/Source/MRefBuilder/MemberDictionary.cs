// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 03/28/2012 - EFW - Fixed Contains() so that when checking for matching members it compares generic
// template parameters by name to match members with generic parameters correctly.

using System;
using System.Collections;
using System.Collections.Generic;

using System.Compiler;

using Microsoft.Ddue.Tools.Reflection;

namespace Microsoft.Ddue.Tools
{

    public class MemberDictionary : ICollection<Member>
    {

        private Dictionary<string, List<Member>> index = new Dictionary<string, List<Member>>();

        private TypeNode type;

        // construct the dictionary

        public MemberDictionary(TypeNode type, ApiFilter filter)
        {

            this.type = type;

            bool isSealed = type.IsSealed;

            // add all member of the type that the filter allows
            MemberList members = type.Members;
            for(int i = 0; i < members.Count; i++)
            {
                Member member = members[i];

                // don't add nested types
                if(member is TypeNode)
                    continue;

                // if our type is sealed, don't add protected members (is this check even necessary?)
                // if (isSealed && (member.IsFamily || member.IsFamilyAndAssembly)) continue;

                // don't add members that the filter rejects
                if(!filter.IsExposedMember(member))
                    continue;

                // okay, add the member
                AddMember(member);
            }

            // for enumerations, don't list inherited members
            if(type is EnumNode)
                return;

            // for interfaces, list members of inherited interfaces
            if(type is Interface)
            {

                InterfaceList contracts = type.Interfaces;
                for(int i = 0; i < contracts.Count; i++)
                {

                    Interface contract = contracts[i];

                    // members of hidden interfaces don't count
                    if(!filter.IsExposedType(contract))
                        continue;

                    // otherwise, add inherited interface members
                    MemberList contractMembers = contract.Members;
                    for(int j = 0; j < contractMembers.Count; j++)
                    {
                        Member contractMember = contractMembers[j];

                        // check for exposure; this is necessary to remove accessor methods
                        if(!filter.IsExposedMember(contractMember))
                            continue;

                        AddMember(contractMember);
                    }


                }

                return;
            }

            // don't list inherited memers for static classes
            if(type.IsAbstract && type.IsSealed)
                return;

            // now interate up through the type hierarchy
            for(TypeNode parentType = type.BaseType; parentType != null; parentType = parentType.BaseType)
            {

                // iterate through the members of each type
                MemberList parentMembers = parentType.Members;
                for(int i = 0; i < parentMembers.Count; i++)
                {
                    Member parentMember = parentMembers[i];

                    // don't add constructors
                    if((parentMember.NodeType == NodeType.InstanceInitializer) || (parentMember.NodeType == NodeType.StaticInitializer))
                        continue;

                    // don't add inherited static members
                    if(parentMember.IsStatic)
                        continue;

                    // don't add nested types
                    if(parentMember is TypeNode)
                        continue;

                    // if our type is sealed, don't add protected members
                    // if (isSealed && (parentMember.IsFamily || parentMember.IsFamilyAndAssembly)) continue;

                    // don't add members that the filter rejects
                    if(!filter.IsExposedMember(parentMember))
                        continue;

                    // don't add members we have overridden
                    if(this.Contains(parentMember))
                        continue;

                    // otherwise, add the member 
                    AddMember(parentMember);

                }

            }

        }

        public List<Member> AllMembers
        {
            get
            {
                List<Member> list = new List<Member>();
                foreach(List<Member> entries in index.Values)
                {
                    list.AddRange(entries);
                }
                return (list);
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                foreach(List<Member> entries in index.Values)
                {
                    count += entries.Count;
                }
                return (count);
            }
        }

        public IList<string> MemberNames
        {
            get
            {
                return (new List<string>(index.Keys));
            }
        }

        // access the data

        public TypeNode Type
        {
            get
            {
                return (type);
            }
        }

        bool ICollection<Member>.IsReadOnly
        {
            get
            {
                return (true);
            }
        }

        public List<Member> this[string name]
        {
            get
            {
                List<Member> members;
                index.TryGetValue(name, out members);
                return (members);
            }
        }

        public void Clear()
        {
            throw new InvalidOperationException();
        }

        public bool Contains(Member member)
        {
            // get candidate members with the same name
            List<Member> candidates;
            bool found = index.TryGetValue(member.Name.Name, out candidates);

            // if no candidates were found, we don't have the member
            if(!found)
                return (false);

            // iterate over the candidates, looking for one of the same type with the same parameters
            ParameterList parameters = GetParameters(member);
            foreach(Member candidate in candidates)
            {
                // candidates must be of the same type
                if(candidate.NodeType != member.NodeType)
                    continue;

                // get candidate parameters
                ParameterList candidateParameters = GetParameters(candidate);

                // number of parameters must match
                if(parameters.Count != candidateParameters.Count)
                    continue;

                // each parameter type must match
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

            // no candidates matched
            return false;
        }

        // methods to complete ICollection<Member>

        public void CopyTo(Member[] array, int index)
        {
            throw new NotImplementedException();
        }

        void ICollection<Member>.Add(Member member)
        {
            throw new InvalidOperationException();
        }

        IEnumerator<Member> IEnumerable<Member>.GetEnumerator()
        {
            return (GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (GetEnumerator());
        }

        public bool Remove(Member member)
        {
            throw new InvalidOperationException();
        }

        private static ParameterList GetParameters(Member member)
        {

            // if the member is a method, get it's parameter list
            Method method = member as Method;
            if(method != null)
            {
                return (method.Parameters);
            }

            // if the member is a property, get it's parameter list
            Property property = member as Property;
            if(property != null)
            {
                return (property.Parameters);
            }

            // member is neither a method nor a property
            return (new ParameterList());
            //return(null);

        }

        private void AddMember(Member member)
        {

            // get the member name
            string name = member.Name.Name;

            // look up the member list for that name
            List<Member> members;
            if(!index.TryGetValue(name, out members))
            {
                // if there isn't one already, make one
                members = new List<Member>();
                index.Add(name, members);
            }
            // add the member to the list
            members.Add(member);

        }

        // enumerator stuff

        private IEnumerator<Member> GetEnumerator()
        {
            return (AllMembers.GetEnumerator());
        }

    }

}
