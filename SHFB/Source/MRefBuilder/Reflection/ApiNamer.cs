// Copyright (c) Microsoft Corporation.  All rights reserved.
//

// Change history:
// 11/20/2013 - EFW - General code clean up

using System.Compiler;

namespace Sandcastle.Tools.Reflection
{
    /// <summary>
    /// This abstract base class is used to generate API member names
    /// </summary>
    public abstract class ApiNamer
    {
        /// <summary>
        /// This is used to get the API name for the given member based on its type
        /// </summary>
        /// <param name="api">The API member for which to get the name</param>
        /// <returns>The name of the fully qualified API member name</returns>
        public virtual string GetApiName(Member api)
        {
            if(api is Namespace space)
                return this.GetNamespaceName(space);

            if(api is TypeNode type)
                return this.GetTypeName(type);

            return this.GetMemberName(api);
        }

        /// <summary>
        /// This method is used to get the namespace name
        /// </summary>
        /// <param name="space">The namespace for which to get the name</param>
        /// <returns>The namespace name</returns>
        public abstract string GetNamespaceName(Namespace space);

        /// <summary>
        /// This method is used to get the type name
        /// </summary>
        /// <param name="type">The type for which to get the name</param>
        /// <returns>The type name</returns>
        public abstract string GetTypeName(TypeNode type);

        /// <summary>
        /// This method is used to get the member name
        /// </summary>
        /// <param name="space">The member for which to get the name</param>
        /// <returns>The member name</returns>
        public abstract string GetMemberName(Member member);
    }
}
