// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.Compiler;


namespace Microsoft.Ddue.Tools.Reflection {

    public class ApiVisitor : IDisposable {

        private static Comparison < Member > memberComparison = new Comparison < Member >(CompareMembers);

        // Sorting logic

        private static Comparison < Namespace > namespaceComparison = new Comparison < Namespace >(CompareMembers);

        private static Comparison < TypeNode > typeComparison = new Comparison < TypeNode >(CompareMembers);

        private List < AssemblyNode > accessoryAssemblies = new List < AssemblyNode >();

        // Disposal logic

        private List < AssemblyNode > assemblies = new List < AssemblyNode >();

        // object model store

        private Dictionary < string, Namespace > catalog = new Dictionary < string, Namespace >();

        private ApiFilter filter;

        // Keep track of any metadata load errors

        private Dictionary < string, Exception > loadErrors = new Dictionary < string, Exception >();

        // Revised assembly storage

        private AssemblyResolver resolver = new AssemblyResolver();

        protected ApiVisitor(ApiFilter filter) {
            this.filter = filter;
        }

        protected ApiVisitor() : this(new NoFilter()) { }

        public AssemblyNode[] AccessoryAssemblies {
            get {
                return (accessoryAssemblies.ToArray());
            }
        }

        public ApiFilter ApiFilter {
            get {
                return (filter);
            }
            set {
                filter = value;
            }
        }

        public AssemblyNode[] Assemblies {
            get {
                return (assemblies.ToArray());
            }
        }

        public Dictionary < string, Exception > LoadErrors {
            get {
                return (loadErrors);
            }
        }

        public AssemblyResolver Resolver {
            get {
                return (resolver);
            }
            set {
                resolver = value;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                foreach (AssemblyNode assembly in assemblies) {
                    //				Console.WriteLine(loadedModule.Name);
                    assembly.Dispose();
                }
                foreach (AssemblyNode accessoryAssembly in accessoryAssemblies) {
                    accessoryAssembly.Dispose();
                }
            }
        }

        public void LoadAccessoryAssemblies(string filePattern) {

            // get the full path to the relevent directory
            string directoryPath = Path.GetDirectoryName(filePattern);
            if ((directoryPath == null) || (directoryPath.Length == 0)) directoryPath = Environment.CurrentDirectory;
            directoryPath = Path.GetFullPath(directoryPath);

            // get the file name, which may contain wildcards
            string filePath = Path.GetFileName(filePattern);

            // look up the files and load them
            string[] files = Directory.GetFiles(directoryPath, filePath);
            foreach (string file in files) {
                LoadAccessoryAssembly(file);
            }
        }

        // Accessory modules

        //private IDictionary cache = new Hashtable();

        public void LoadAccessoryAssembly(string filePath) {
            AssemblyNode assembly = AssemblyNode.GetAssembly(filePath, null, false, false, false, false); // this causes non-classes to register as classes
            if (assembly != null) {
                if (assembly.Name == "mscorlib") ResetMscorlib(assembly);

                resolver.Add(assembly);
                //assembly.AssemblyReferenceResolutionAfterProbingFailed += unresolvedModuleHandler;
                accessoryAssemblies.Add(assembly);
            }
        }

        public void LoadAssemblies(string filePattern) {

            // get the full path to the relevent directory
            string directoryPath = Path.GetDirectoryName(filePattern);
            if ((directoryPath == null) || (directoryPath.Length == 0)) directoryPath = Environment.CurrentDirectory;
            directoryPath = Path.GetFullPath(directoryPath);

            // get the file name, which may contain wildcards
            string filePath = Path.GetFileName(filePattern);

            // look up the files and load them
            string[] files = Directory.GetFiles(directoryPath, filePath);
            foreach (string file in files) {
                LoadAssembly(file);
            }

        }

        // Parsing logic

        public void LoadAssembly(string filePath) {
            //Console.WriteLine("loading {0}", filePath);
            //AssemblyNode assembly = AssemblyNode.GetAssembly(filePath);   // this causes mscorlib to be missing members
            //AssemblyNode assembly = AssemblyNode.GetAssembly(filePath, cache); // this causes compact framework non-classes to register as classes
            //AssemblyNode assembly = AssemblyNode.GetAssembly(filePath, cache, false, false, true, false); // this causes missing mscorlib members
            //AssemblyNode assembly = AssemblyNode.GetAssembly(filePath, cache, false, false, false, false); // this causes non-classes to register as classes
            //AssemblyNode assembly = AssemblyNode.GetAssembly(filePath, null, false, false, true, false); // this causes missing mscorlib members
            AssemblyNode assembly = AssemblyNode.GetAssembly(filePath, null, false, false, false, false); // this causes non-classes to register as classes

            if (assembly != null) {
                if (assembly.Name == "mscorlib") ResetMscorlib(assembly);

                // Console.WriteLine("assembly = {0}", assembly.Name);
                resolver.Add(assembly);
                //assembly.AssemblyReferenceResolutionAfterProbingFailed += unresolvedModuleHandler;
                assemblies.Add(assembly);
                //Console.WriteLine("{0} has {1} types", assembly.Name, assembly.ExportedTypes.Count);
                //StoreTypes(assembly.Types);
            }

        }

        // Visit Object Model

        public void VisitApis() {

            // store types
            // we have to do this after all assemblies are registered because the resolution may not work unless
            // all the assemblies we need are in the resolver cache
            //Console.WriteLine("storing types");
            foreach (AssemblyNode assembly in assemblies) {
                //Console.WriteLine("assembly {0}", assembly.Name);
                //Console.WriteLine("has {0} types", assembly.Types.Count);
                StoreTypes(assembly.Types);
                //Console.WriteLine("done with assembly");
            }
            //Console.WriteLine("done storing types");

            //Console.WriteLine("visiting namespaces");
            NamespaceList spaces = new NamespaceList();
            foreach (Namespace space in catalog.Values) {
                if (filter.IsExposedNamespace(space)) spaces.Add(space);
            }
            VisitNamespaces(spaces);
        }

        protected virtual void VisitEntity(Member entity) {
            // inherit and insert logic here
        }

        protected virtual void VisitMember(Member member) {
            VisitEntity(member);
        }

        protected virtual void VisitMembers(MemberList members) {
            // sort members by name
            Member[] sorted_members = new Member[members.Count];
            for (int i = 0; i < members.Count; i++) sorted_members[i] = members[i];
            Array.Sort < Member >(sorted_members, memberComparison);
            // visit them
            foreach (Member member in sorted_members) {
                // don't visit nested types, as they are already visited
                if (member is TypeNode) continue;
                if (filter.IsExposedMember(member))
                {
                    VisitMember(member);
                }
            }
        }

        protected virtual void VisitNamespace(Namespace space) {
            //Console.WriteLine("Visit Entity {0}",space.FullName);
            VisitEntity(space);
            TypeNodeList types = space.Types;
            VisitTypes(types);
        }

        protected virtual void VisitNamespaces(NamespaceList spaces) {
            // sort namespaces by name
            Namespace[] sorted_spaces = new Namespace[spaces.Count];
            for (int i = 0; i < spaces.Count; i++) sorted_spaces[i] = spaces[i];
            Array.Sort < Namespace >(sorted_spaces, namespaceComparison);
            // visit them
            foreach (Namespace space in sorted_spaces) {
                if (filter.IsExposedNamespace(space)) VisitNamespace(space);
            }
        }

        protected virtual void VisitType(TypeNode type) {
            //Console.WriteLine(type.FullName);
            VisitEntity(type);
            MemberList members = type.Members;
            //Console.WriteLine("{0}: {1}", type.FullName, members.Length);
            VisitMembers(members);
        }

        protected virtual void VisitTypes(TypeNodeList types) {
            // sort types by name
            TypeNode[] sorted_types = new TypeNode[types.Count];
            for (int i = 0; i < types.Count; i++) sorted_types[i] = types[i];
            Array.Sort < TypeNode >(sorted_types, typeComparison);
            // visit them
            foreach (TypeNode type in sorted_types) {
                //Console.WriteLine("visiting {0}", type.Name);
                //visit this type if it is exposed, or has members that are set as exposed
                if (filter.IsExposedType(type) || filter.HasExposedMembers(type))
                    VisitType(type); //visit type and members
            }
        }

        private static int CompareMembers(Member a, Member b) {
            return (String.Compare(a.FullName, b.FullName));
        }

        private static int CompareNamespaces(Namespace a, Namespace b) {
            return (String.Compare(a.Name.Name, b.Name.Name));
        }

        private static int CompareTypes(TypeNode a, TypeNode b) {
            return (String.Compare(a.Name.Name, b.Name.Name));
        }

        private static string GetNamespaceName(TypeNode type) {
            TypeNode parent = type.DeclaringType;
            if (parent != null) {
                return (GetNamespaceName(parent));
            } else {
                if (type.Namespace == null) {
                    return (String.Empty);
                } else {
                    return (type.Namespace.Name);
                }
            }
        }

        private void ResetMscorlib(AssemblyNode assembly) {
            TargetPlatform.Clear();
            CoreSystemTypes.Clear();
            CoreSystemTypes.SystemAssembly = assembly;
            CoreSystemTypes.Initialize(true, false);
        }

        private void StoreType(TypeNode type) {
            //Console.WriteLine("type: {0} ", type.Name);
            /*
            if (type.Name == null) {
                // CCI seems to occasionally construct corrupted, phantom types, which we should reject 
                // Console.WriteLine("unidentified type rejected");
                return;
            }
            */
            string spaceName = GetNamespaceName(type);
            //Console.WriteLine("in space: {0}", spaceName);
            Namespace space;
            if (!catalog.TryGetValue(spaceName, out space)) {
                space = new Namespace(new Identifier(spaceName));
                catalog.Add(spaceName, space);
            }
            if (space.Types == null) Console.WriteLine("null type list");
            space.Types.Add(type);

            //Console.WriteLine("getting members");
            MemberList members = type.Members;
            //Console.WriteLine("got {0} members", members.Count);
            for (int i = 0; i < members.Count; i++) {
                TypeNode nestedType = members[i] as TypeNode;
                if (nestedType != null) {
                    //Console.WriteLine("nested type {0}", type.FullName);
                    StoreType(nestedType);
                }
            }
            //Console.WriteLine("done storing type");
        }

        private void StoreTypes(TypeNodeList types)
        {
            //Console.WriteLine("{0} types", types.Length);

            for (int i = 0; i < types.Count; i++)
                StoreType(types[i]);
        }
    }
}
