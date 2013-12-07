// Copyright (c) Microsoft Corporation.  All rights reserved.
//

// Change history:
// 11/20/2013 - EFW - Cleaned up the code and removed unused members

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection
{
    public class ApiVisitor : IDisposable
    {
        #region Private data members
        //=====================================================================

        private List<AssemblyNode> accessoryAssemblies, assemblies;
        private Dictionary<string, Namespace> catalog;
        private ApiFilter filter;
        private AssemblyResolver resolver;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the assembly resolver in use
        /// </summary>
        public AssemblyResolver Resolver
        {
            get { return resolver; }
        }

        /// <summary>
        /// This read-only property returns the API filter in use
        /// </summary>
        public ApiFilter ApiFilter
        {
            get { return filter; }
        }

        /// <summary>
        /// This read-only property returns an enumerable list of the loaded accessory assemblies
        /// </summary>
        public IEnumerable<AssemblyNode> AccessoryAssemblies
        {
            get { return accessoryAssemblies; }
        }

        /// <summary>
        /// This read-only property returns a list of the assemblies to parse
        /// </summary>
        public IEnumerable<AssemblyNode> Assemblies
        {
            get { return assemblies; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resolver">The assembly resolver to use</param>
        /// <param name="filter">The API filter to use</param>
        protected ApiVisitor(AssemblyResolver resolver, ApiFilter filter)
        {
            accessoryAssemblies = new List<AssemblyNode>();
            assemblies = new List<AssemblyNode>();
            catalog = new Dictionary<string, Namespace>();

            this.resolver = resolver;
            this.filter = filter;
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the API filter if not done explicitly
        /// with <see cref="Dispose()"/>.
        /// </summary>
        ~ApiVisitor()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the API filter
        /// </summary>
        /// <overloads>There are two overloads for this method</overloads>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This can be overridden by derived classes to add their own disposal code if necessary.
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                foreach(AssemblyNode assembly in assemblies)
                    assembly.Dispose();

                foreach(AssemblyNode accessoryAssembly in accessoryAssemblies)
                    accessoryAssembly.Dispose();
            }
        }
        #endregion

        #region General helper methods
        //=====================================================================

        /// <summary>
        /// This is used to load a set of accessory assemblies from the given folder
        /// </summary>
        /// <param name="filePattern">The file pattern used to find the assemblies</param>
        public void LoadAccessoryAssemblies(string filePattern)
        {
            string directoryPath = Path.GetDirectoryName(filePattern), filePath = Path.GetFileName(filePattern);

            if(String.IsNullOrEmpty(directoryPath))
                directoryPath = Environment.CurrentDirectory;

            directoryPath = Path.GetFullPath(directoryPath);

            foreach(string file in Directory.EnumerateFiles(directoryPath, filePath))
                this.LoadAccessoryAssembly(file);
        }

        /// <summary>
        /// This is used to load an accessory assembly
        /// </summary>
        /// <param name="filePath">The path of the accessory assembly to load</param>
        public void LoadAccessoryAssembly(string filePath)
        {
            // This causes non-classes to register as classes
            AssemblyNode assembly = AssemblyNode.GetAssembly(filePath, null, false, false, false, false);

            if(assembly != null)
            {
                if(assembly.Name == "mscorlib")
                    ResetMscorlib(assembly);

                resolver.Add(assembly);
                accessoryAssemblies.Add(assembly);
            }
        }

        /// <summary>
        /// This is used to load a set of assemblies to parse from the given folder
        /// </summary>
        /// <param name="filePattern">The file pattern used to find the assemblies</param>
        public void LoadAssemblies(string filePattern)
        {
            string directoryPath = Path.GetDirectoryName(filePattern), filePath = Path.GetFileName(filePattern);

            if(String.IsNullOrEmpty(directoryPath))
                directoryPath = Environment.CurrentDirectory;

            directoryPath = Path.GetFullPath(directoryPath);

            foreach(string file in Directory.EnumerateFiles(directoryPath, filePath))
                this.LoadAssembly(file);
        }

        /// <summary>
        /// This is used to load an assembly that will be parsed and documented
        /// </summary>
        /// <param name="filePath">The path of the documented assembly to load</param>
        public void LoadAssembly(string filePath)
        {
            // This causes non-classes to register as classes
            AssemblyNode assembly = AssemblyNode.GetAssembly(filePath, null, false, false, false, false);

            if(assembly != null)
            {
                if(assembly.Name == "mscorlib")
                    ResetMscorlib(assembly);

                resolver.Add(assembly);
                assemblies.Add(assembly);
            }
        }

        /// <summary>
        /// This is used to reset the target platform information if mscorlib is specified as one of the
        /// reference assemblies or documented assemblies.
        /// </summary>
        /// <param name="assembly">The system assembly for the target platform (mscorlib)</param>
        private static void ResetMscorlib(AssemblyNode assembly)
        {
            TargetPlatform.Clear();
            CoreSystemTypes.Clear();
            CoreSystemTypes.SystemAssembly = assembly;
            CoreSystemTypes.Initialize(true, false);
        }

        /// <summary>
        /// This is used to get a namespace name
        /// </summary>
        /// <param name="type">The type for which to get the namespace</param>
        /// <returns>The namespace for the type</returns>
        private static string GetNamespaceName(TypeNode type)
        {
            TypeNode parent = type.DeclaringType;

            if(parent != null)
                return GetNamespaceName(parent);

            return (type.Namespace == null) ? String.Empty : type.Namespace.Name;
        }

        /// <summary>
        /// This is used to build the catalog of types in the parsed assemblies
        /// </summary>
        /// <param name="types">The list of types from an assembly</param>
        private void StoreTypes(TypeNodeList types)
        {
            for(int i = 0; i < types.Count; i++)
                this.StoreType(types[i]);
        }

        /// <summary>
        /// Store the type in the catalog indexed by namespace
        /// </summary>
        /// <param name="type">The type to store in the catalog</param>
        private void StoreType(TypeNode type)
        {
            string spaceName = GetNamespaceName(type);
            Namespace space;

            if(!catalog.TryGetValue(spaceName, out space))
            {
                space = new Namespace(new Identifier(spaceName));
                catalog.Add(spaceName, space);
            }

            if(space.Types == null)
                throw new InvalidOperationException("Null type list encountered");

            space.Types.Add(type);

            // Store nested types as well
            foreach(var member in type.Members)
            {
                TypeNode nestedType = member as TypeNode;

                if(nestedType != null)
                    this.StoreType(nestedType);
            }
        }
        #endregion

        #region Visitor methods
        //=====================================================================

        /// <summary>
        /// This is called to visit all namespaces, types, and members in the list of assemblies to be documented
        /// </summary>
        public void VisitApis()
        {
            // Store types.  We have to do this after all assemblies are registered because the resolution may
            // not work unless all the assemblies we need are in the resolver cache.
            foreach(AssemblyNode assembly in assemblies)
                this.StoreTypes(assembly.Types);

            NamespaceList spaces = new NamespaceList();

            foreach(Namespace space in catalog.Values)
                if(filter.IsExposedNamespace(space))
                    spaces.Add(space);

            this.VisitNamespaces(spaces);
        }

        /// <summary>
        /// This method can be overridden in derived classes to handle common tasks that should occur before an
        /// API member of any kind is visited.
        /// </summary>
        /// <param name="entity">The entity to be visited</param>
        /// <remarks>The default implementation does nothing</remarks>
        protected virtual void VisitEntity(Member entity)
        {
        }

        /// <summary>
        /// This is used to visit a list of namespaces
        /// </summary>
        /// <param name="spaces">The list of namespaces to visit</param>
        protected virtual void VisitNamespaces(NamespaceList spaces)
        {
            // Visit the namespaces in sorted order
            var sortedNamespaces = spaces.OrderBy(s => s.FullName).ToList();

            foreach(Namespace space in sortedNamespaces)
                if(filter.IsExposedNamespace(space))
                    this.VisitNamespace(space);
        }

        /// <summary>
        /// This is used to visit a namespace
        /// </summary>
        /// <param name="space">The namespace to visit</param>
        protected virtual void VisitNamespace(Namespace space)
        {
            this.VisitEntity(space);
            this.VisitTypes(space.Types);
        }

        /// <summary>
        /// This is used to visit a list of types
        /// </summary>
        /// <param name="types">The list of types to visit</param>
        protected virtual void VisitTypes(TypeNodeList types)
        {
            // Visit the types in sorted order
            foreach(TypeNode type in types.OrderBy(t => t.FullName))
                if(filter.IsExposedType(type) || filter.HasExposedMembers(type))
                    this.VisitType(type);
        }

        /// <summary>
        /// This is used to visit a type
        /// </summary>
        /// <param name="type">The type to visit</param>
        protected virtual void VisitType(TypeNode type)
        {
            this.VisitEntity(type);
            this.VisitMembers(type.Members);
        }

        /// <summary>
        /// This is used to visit a list of members
        /// </summary>
        /// <param name="members">The list of members to visit</param>
        protected virtual void VisitMembers(MemberList members)
        {
            // Visit the members in sorted order
            foreach(Member member in members.OrderBy(m => m.FullName))
            {
                // Don't visit nested types either as they are already visited
                if(!(member is TypeNode) && filter.IsExposedMember(member))
                    this.VisitMember(member);
            }
        }

        /// <summary>
        /// This is used to visit a member
        /// </summary>
        /// <param name="member">The member to visit</param>
        protected virtual void VisitMember(Member member)
        {
            this.VisitEntity(member);
        }
        #endregion
    }
}
