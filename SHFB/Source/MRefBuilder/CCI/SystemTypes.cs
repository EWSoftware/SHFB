// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 09/10/2012 - EFW - Added support to the TargetPlatform class for using the Frameworks.xml file to load
// framework assembly information.
// 11/22/2013 - EFW - Cleared out the conditional statements
// 04/20/2014 - EFW - Added a workaround for the .NET Micro Framework related to DictionaryEntry being a class
// 01/06/2014 - EFW - Added a TargetPlatform.Platform member to allow other classes to find out what platform
// is being used for the core framework types.
// 05/09/2015 - EFW - Removed obsolete core framework assembly definitions and related methods.
// 05/09/2016 - EFW - Fixed SystemAssemblyLocation so that it is set when all system types are redirected.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Compiler.Metadata;

namespace System.Compiler
{
    public static class TargetPlatform
    {
        public static bool DoNotLockFiles;
        public static bool GetDebugInfo;
        public static char GenericTypeNamesMangleChar = '_';

        /// <summary>
        /// This is used to get or set the system assembly location (mscorlib.dll or System.Runtime.dll)
        /// </summary>
        public static string SystemAssemblyLocation { get; set; }

        public static bool UseGenerics
        {
            get
            {
                Version v = TargetPlatform.TargetVersion;

                if (v == null)
                {
                    v = CoreSystemTypes.SystemAssembly.Version;

                    if (v == null)
                        v = typeof(object).Assembly.GetName().Version;
                }

                return v.Major > 1 || v.Minor > 2 || v.Minor == 2 && v.Build >= 3300;
            }
        }

        public static void Clear()
        {
            SystemAssemblyLocation = null;
            DoNotLockFiles = false;
            GetDebugInfo = false;
            PlatformAssembliesLocation = String.Empty;

            SystemTypes.Clear();
        }

        public static string Platform;
        public static Version TargetVersion;
        public static string TargetRuntimeVersion;
        public static bool AllSystemTypesRedirected;

        public static string PlatformAssembliesLocation = String.Empty;

        private static TrivialHashtable assemblyReferenceFor;

        public static TrivialHashtable AssemblyReferenceFor
        {
            get
            {
                if(TargetPlatform.assemblyReferenceFor == null)
                    throw new InvalidOperationException("AssemblyReferenceFor not set!  Has target platform " +
                        "information been set?");

                return TargetPlatform.assemblyReferenceFor;
            }
            set
            {
                TargetPlatform.assemblyReferenceFor = value;
            }
        }

        //!EFW
        /// <summary>
        /// Load target framework settings and assembly details
        /// </summary>
        /// <param name="platformType">The platform type</param>
        /// <param name="version">The framework version</param>
        public static void SetFrameworkInformation(string platformType, string version,
          IEnumerable<string> componentLocations)
        {
            var rdsd = new Sandcastle.Core.Reflection.ReflectionDataSetDictionary(componentLocations);
            var dataSet = rdsd.CoreFrameworkMatching(platformType, new Version(version), true);

            if(dataSet == null)
                throw new InvalidOperationException(String.Format("Unable to locate information for the " +
                    "framework version '{0} {1}' or a suitable redirected version on this system",
                    platformType, version));

            var coreLocation = dataSet.CoreFrameworkLocation;

            if(coreLocation == null)
                throw new InvalidOperationException(String.Format("A core framework location has not been " +
                    "defined for the framework '{0} {1}'", platformType, version));

            Platform = dataSet.Platform;
            TargetVersion = dataSet.Version;
            TargetRuntimeVersion = "v" + dataSet.Version.ToString();
            AllSystemTypesRedirected = dataSet.AllSystemTypesRedirected;
            GenericTypeNamesMangleChar = '`';
            PlatformAssembliesLocation = coreLocation.Path;

            var ad = dataSet.FindAssembly("mscorlib");

            if(ad != null)
                SystemAssemblyLocation = ad.Filename;
            else
            {
                // Frameworks that redirect all system types typically redirect them to System.Runtime
                ad = dataSet.FindAssembly("System.Runtime");

                if(ad == null)
                    throw new InvalidOperationException(String.Format("The system types assembly could not be " +
                        "found for the framework '{0} {1}'", platformType, version));

                SystemAssemblyLocation = ad.Filename;
            }

            // Load references to all the other framework assemblies
            var allAssemblies = dataSet.IncludedAssemblies.ToList();

            TrivialHashtable assemblyReferenceFor = new TrivialHashtable(allAssemblies.Count);

            // Loading mscorlib causes a reset of the reference cache and other info so we must ignore it
            foreach(var asm in allAssemblies)
                if(!asm.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) && File.Exists(asm.Filename))
                {
                    AssemblyReference aref = new AssemblyReference(asm.ToString());
                    aref.Location = asm.Filename;
                    assemblyReferenceFor[Identifier.For(asm.Name).UniqueIdKey] = aref;
                }

            TargetPlatform.assemblyReferenceFor = assemblyReferenceFor;
        }
    }

    public static class CoreSystemTypes
    {
        internal static bool Initialized;

        internal static bool IsInitialized { get { return Initialized; } }

        //system assembly (the basic runtime)
        public static AssemblyNode/*!*/ SystemAssembly;

        //Special base types
        public static Class/*!*/ Object;
        public static Class/*!*/ String;
        public static Class/*!*/ ValueType;
        public static Class/*!*/ Enum;
        public static Class/*!*/ MulticastDelegate;
        public static Class/*!*/ Array;
        public static Class/*!*/ Type;
        public static Class/*!*/ Delegate;
        public static Class/*!*/ Exception;
        public static Class/*!*/ Attribute;

        //primitive types
        public static Struct/*!*/ Boolean;
        public static Struct/*!*/ Char;
        public static Struct/*!*/ Int8;
        public static Struct/*!*/ UInt8;
        public static Struct/*!*/ Int16;
        public static Struct/*!*/ UInt16;
        public static Struct/*!*/ Int32;
        public static Struct/*!*/ UInt32;
        public static Struct/*!*/ Int64;
        public static Struct/*!*/ UInt64;
        public static Struct/*!*/ Single;
        public static Struct/*!*/ Double;
        public static Struct/*!*/ IntPtr;
        public static Struct/*!*/ UIntPtr;
        public static Struct/*!*/ DynamicallyTypedReference;

        //Classes need for System.TypeCode
        public static Class/*!*/ DBNull;
        public static Struct/*!*/ DateTime;
        public static Struct/*!*/ Decimal;

        //Special types
        public static Class/*!*/ IsVolatile;
        public static Struct/*!*/ Void;
        public static Struct/*!*/ ArgIterator;
        public static Struct/*!*/ RuntimeFieldHandle;
        public static Struct/*!*/ RuntimeMethodHandle;
        public static Struct/*!*/ RuntimeTypeHandle;
        public static Struct/*!*/ RuntimeArgumentHandle;

        //Special attributes    
        public static EnumNode SecurityAction;

        static CoreSystemTypes()
        {
            CoreSystemTypes.Initialize(TargetPlatform.DoNotLockFiles, TargetPlatform.GetDebugInfo);
        }

        public static void Clear()
        {
            lock (Module.GlobalLock)
            {
                if (Reader.StaticAssemblyCache != null)
                {
                    foreach (AssemblyNode cachedAssembly in new System.Collections.ArrayList(Reader.StaticAssemblyCache.Values))
                    {
                        if (cachedAssembly != null) cachedAssembly.Dispose();
                    }
                    Reader.StaticAssemblyCache.Clear();
                }
                //Dispose the system assemblies in case they were not in the static cache. It is safe to dispose an assembly more than once.
                if (CoreSystemTypes.SystemAssembly != null && CoreSystemTypes.SystemAssembly != AssemblyNode.Dummy)
                {
                    CoreSystemTypes.SystemAssembly.Dispose();
                    CoreSystemTypes.SystemAssembly = null;
                }
                CoreSystemTypes.ClearStatics();
                CoreSystemTypes.Initialized = false;
                TargetPlatform.AssemblyReferenceFor = new TrivialHashtable(0);
            }
        }
        public static void Initialize(bool doNotLockFile, bool getDebugInfo)
        {
            if (CoreSystemTypes.Initialized) CoreSystemTypes.Clear();
            if (SystemAssembly == null)
                SystemAssembly = CoreSystemTypes.GetSystemAssembly(doNotLockFile, getDebugInfo);
            if (SystemAssembly == null) throw new InvalidOperationException(ExceptionStrings.InternalCompilerError);
            if (TargetPlatform.TargetVersion == null)
            {
                TargetPlatform.TargetVersion = SystemAssembly.Version;
                if (TargetPlatform.TargetVersion == null)
                    TargetPlatform.TargetVersion = typeof(object).Module.Assembly.GetName().Version;
            }
            if (TargetPlatform.TargetVersion != null)
            {
                if (TargetPlatform.TargetVersion.Major > 1 || TargetPlatform.TargetVersion.Minor > 1 ||
                  (TargetPlatform.TargetVersion.Minor == 1 && TargetPlatform.TargetVersion.Build == 9999))
                {
                    if (SystemAssembly.IsValidTypeName(StandardIds.System, Identifier.For("Nullable`1")))
                        TargetPlatform.GenericTypeNamesMangleChar = '`';
                    else if (SystemAssembly.IsValidTypeName(StandardIds.System, Identifier.For("Nullable!1")))
                        TargetPlatform.GenericTypeNamesMangleChar = '!';
                    else if (TargetPlatform.TargetVersion.Major == 1 && TargetPlatform.TargetVersion.Minor == 2)
                        TargetPlatform.GenericTypeNamesMangleChar = (char)0;
                }
            }
            // This must be done in the order: Object, ValueType, Char, String
            // or else some of the generic type instantiations don't get filled
            // in correctly. (String ends up implementing IEnumerable<string>
            // instead of IEnumerable<char>.)
            Object = (Class)GetTypeNodeFor("System", "Object", ElementType.Object);
            ValueType = (Class)GetTypeNodeFor("System", "ValueType", ElementType.Class);
            Char = (Struct)GetTypeNodeFor("System", "Char", ElementType.Char);
            String = (Class)GetTypeNodeFor("System", "String", ElementType.String);
            Enum = (Class)GetTypeNodeFor("System", "Enum", ElementType.Class);
            MulticastDelegate = (Class)GetTypeNodeFor("System", "MulticastDelegate", ElementType.Class);
            Array = (Class)GetTypeNodeFor("System", "Array", ElementType.Class);
            Type = (Class)GetTypeNodeFor("System", "Type", ElementType.Class);
            Boolean = (Struct)GetTypeNodeFor("System", "Boolean", ElementType.Boolean);
            Int8 = (Struct)GetTypeNodeFor("System", "SByte", ElementType.Int8);
            UInt8 = (Struct)GetTypeNodeFor("System", "Byte", ElementType.UInt8);
            Int16 = (Struct)GetTypeNodeFor("System", "Int16", ElementType.Int16);
            UInt16 = (Struct)GetTypeNodeFor("System", "UInt16", ElementType.UInt16);
            Int32 = (Struct)GetTypeNodeFor("System", "Int32", ElementType.Int32);
            UInt32 = (Struct)GetTypeNodeFor("System", "UInt32", ElementType.UInt32);
            Int64 = (Struct)GetTypeNodeFor("System", "Int64", ElementType.Int64);
            UInt64 = (Struct)GetTypeNodeFor("System", "UInt64", ElementType.UInt64);
            Single = (Struct)GetTypeNodeFor("System", "Single", ElementType.Single);
            Double = (Struct)GetTypeNodeFor("System", "Double", ElementType.Double);
            IntPtr = (Struct)GetTypeNodeFor("System", "IntPtr", ElementType.IntPtr);
            UIntPtr = (Struct)GetTypeNodeFor("System", "UIntPtr", ElementType.UIntPtr);
            DynamicallyTypedReference = (Struct)GetTypeNodeFor("System", "TypedReference", ElementType.DynamicallyTypedReference);
            Delegate = (Class)GetTypeNodeFor("System", "Delegate", ElementType.Class);
            Exception = (Class)GetTypeNodeFor("System", "Exception", ElementType.Class);
            Attribute = (Class)GetTypeNodeFor("System", "Attribute", ElementType.Class);
            DBNull = (Class)GetTypeNodeFor("System", "DBNull", ElementType.Class);
            DateTime = (Struct)GetTypeNodeFor("System", "DateTime", ElementType.ValueType);
            Decimal = (Struct)GetTypeNodeFor("System", "Decimal", ElementType.ValueType);
            ArgIterator = (Struct)GetTypeNodeFor("System", "ArgIterator", ElementType.ValueType);
            IsVolatile = (Class)GetTypeNodeFor("System.Runtime.CompilerServices", "IsVolatile", ElementType.Class);
            Void = (Struct)GetTypeNodeFor("System", "Void", ElementType.Void);
            RuntimeFieldHandle = (Struct)GetTypeNodeFor("System", "RuntimeFieldHandle", ElementType.ValueType);
            RuntimeMethodHandle = (Struct)GetTypeNodeFor("System", "RuntimeMethodHandle", ElementType.ValueType);
            RuntimeTypeHandle = (Struct)GetTypeNodeFor("System", "RuntimeTypeHandle", ElementType.ValueType);
            RuntimeArgumentHandle = (Struct)GetTypeNodeFor("System", "RuntimeArgumentHandle", ElementType.ValueType);
            SecurityAction = GetTypeNodeFor("System.Security.Permissions", "SecurityAction", ElementType.ValueType) as EnumNode;
            CoreSystemTypes.Initialized = true;
            CoreSystemTypes.InstantiateGenericInterfaces();

            object dummy = TargetPlatform.AssemblyReferenceFor; //Force selection of target platform

            if(dummy == null)
                return;
        }
        private static void ClearStatics()
        {
            //Special base types
            Object = null;
            String = null;
            ValueType = null;
            Enum = null;
            MulticastDelegate = null;
            Array = null;
            Type = null;
            Delegate = null;
            Exception = null;
            Attribute = null;

            //primitive types
            Boolean = null;
            Char = null;
            Int8 = null;
            UInt8 = null;
            Int16 = null;
            UInt16 = null;
            Int32 = null;
            UInt32 = null;
            Int64 = null;
            UInt64 = null;
            Single = null;
            Double = null;
            IntPtr = null;
            UIntPtr = null;
            DynamicallyTypedReference = null;

            //Special types
            DBNull = null;
            DateTime = null;
            Decimal = null;
            RuntimeArgumentHandle = null;
            ArgIterator = null;
            RuntimeFieldHandle = null;
            RuntimeMethodHandle = null;
            RuntimeTypeHandle = null;
            IsVolatile = null;
            Void = null;
            SecurityAction = null;
        }
        private static void InstantiateGenericInterfaces()
        {
            if (TargetPlatform.TargetVersion != null && (TargetPlatform.TargetVersion.Major < 2 && TargetPlatform.TargetVersion.Minor < 2)) return;
            InstantiateGenericInterfaces(String);
            InstantiateGenericInterfaces(Boolean);
            InstantiateGenericInterfaces(Char);
            InstantiateGenericInterfaces(Int8);
            InstantiateGenericInterfaces(UInt8);
            InstantiateGenericInterfaces(Int16);
            InstantiateGenericInterfaces(UInt16);
            InstantiateGenericInterfaces(Int32);
            InstantiateGenericInterfaces(UInt32);
            InstantiateGenericInterfaces(Int64);
            InstantiateGenericInterfaces(UInt64);
            InstantiateGenericInterfaces(Single);
            InstantiateGenericInterfaces(Double);
            InstantiateGenericInterfaces(DBNull);
            InstantiateGenericInterfaces(DateTime);
            InstantiateGenericInterfaces(Decimal);
        }
        private static void InstantiateGenericInterfaces(TypeNode type)
        {
            if (type == null) return;
            InterfaceList interfaces = type.Interfaces;
            for (int i = 0, n = interfaces == null ? 0 : interfaces.Count; i < n; i++)
            {
                InterfaceExpression ifaceExpr = interfaces[i] as InterfaceExpression;
                if (ifaceExpr == null) continue;
                if (ifaceExpr.Template == null) { Debug.Assert(false); continue; }
                TypeNodeList templArgs = ifaceExpr.TemplateArguments;
                for (int j = 0, m = templArgs.Count; j < m; j++)
                {
                    InterfaceExpression ie = templArgs[j] as InterfaceExpression;
                    if (ie != null)
                        templArgs[j] = ie.Template.GetGenericTemplateInstance(type.DeclaringModule, ie.ConsolidatedTemplateArguments);
                }
                interfaces[i] = (Interface)ifaceExpr.Template.GetGenericTemplateInstance(type.DeclaringModule, ifaceExpr.ConsolidatedTemplateArguments);
            }
        }

        private static AssemblyNode/*!*/ GetSystemAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            AssemblyNode result;

            if(TargetPlatform.SystemAssemblyLocation == null || TargetPlatform.SystemAssemblyLocation.Length == 0)
                TargetPlatform.SystemAssemblyLocation = typeof(object).Module.Assembly.Location;

            result = (AssemblyNode)(new Reader(TargetPlatform.SystemAssemblyLocation, null, doNotLockFile, getDebugInfo, true, false)).ReadModule();

            if(result == null && TargetPlatform.TargetVersion != null && TargetPlatform.TargetVersion == typeof(object).Module.Assembly.GetName().Version)
            {
                TargetPlatform.SystemAssemblyLocation = typeof(object).Module.Assembly.Location;
                result = (AssemblyNode)(new Reader(TargetPlatform.SystemAssemblyLocation, null, doNotLockFile, getDebugInfo, true, false)).ReadModule();
            }

            if(result == null)
            {
                result = new AssemblyNode();
                System.Reflection.AssemblyName aname = typeof(object).Module.Assembly.GetName();
                result.Name = aname.Name;
                result.Version = TargetPlatform.TargetVersion;
                result.PublicKeyOrToken = aname.GetPublicKeyToken();
            }

            return result;
        }
        private static TypeNode/*!*/ GetTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode)
        {
            TypeNode result = null;
            if (SystemAssembly == null)
                Debug.Assert(false);
            else
                result = SystemAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemAssembly, nspace, name, typeCode);
            result.typeCode = typeCode;
            return result;
        }

        internal static TypeNode/*!*/ GetDummyTypeNode(AssemblyNode declaringAssembly, string/*!*/ nspace, string/*!*/ name, ElementType typeCode)
        {
            TypeNode result = null;
            switch (typeCode)
            {
                case ElementType.Object:
                case ElementType.String:
                case ElementType.Class:
                    if (name.Length > 1 && name[0] == 'I' && char.IsUpper(name[1]))
                        result = new Interface();
                    else if (name == "MulticastDelegate" || name == "Delegate")
                        result = new Class();
                    else if (name.EndsWith("Callback") || name.EndsWith("Delegate") || name == "ThreadStart" || name == "FrameGuardGetter" || name == "GuardThreadStart")
                        result = new DelegateNode();
                    else
                        result = new Class();
                    break;
                default:
                    if (name == "CciMemberKind")
                        result = new EnumNode();
                    else
                        result = new Struct();
                    break;
            }
            result.Name = Identifier.For(name);
            result.Namespace = Identifier.For(nspace);
            result.DeclaringModule = declaringAssembly;
            return result;
        }
    }

    public static class SystemTypes
    {
        private static bool Initialized;

        //system assembly (the basic runtime)
        public static AssemblyNode/*!*/ SystemAssembly
        {
            get { return CoreSystemTypes.SystemAssembly; }
            set { CoreSystemTypes.SystemAssembly = value; }
        }

        //Special base types
        public static Class/*!*/ Object { get { return CoreSystemTypes.Object; } }
        public static Class/*!*/ String { get { return CoreSystemTypes.String; } }
        public static Class/*!*/ ValueType { get { return CoreSystemTypes.ValueType; } }
        public static Class/*!*/ Enum { get { return CoreSystemTypes.Enum; } }
        public static Class/*!*/ Delegate { get { return CoreSystemTypes.Delegate; } }
        public static Class/*!*/ MulticastDelegate { get { return CoreSystemTypes.MulticastDelegate; } }
        public static Class/*!*/ Array { get { return CoreSystemTypes.Array; } }
        public static Class/*!*/ Type { get { return CoreSystemTypes.Type; } }
        public static Class/*!*/ Exception { get { return CoreSystemTypes.Exception; } }
        public static Class/*!*/ Attribute { get { return CoreSystemTypes.Attribute; } }

        //primitive types
        public static Struct/*!*/ Boolean { get { return CoreSystemTypes.Boolean; } }
        public static Struct/*!*/ Char { get { return CoreSystemTypes.Char; } }
        public static Struct/*!*/ Int8 { get { return CoreSystemTypes.Int8; } }
        public static Struct/*!*/ UInt8 { get { return CoreSystemTypes.UInt8; } }
        public static Struct/*!*/ Int16 { get { return CoreSystemTypes.Int16; } }
        public static Struct/*!*/ UInt16 { get { return CoreSystemTypes.UInt16; } }
        public static Struct/*!*/ Int32 { get { return CoreSystemTypes.Int32; } }
        public static Struct/*!*/ UInt32 { get { return CoreSystemTypes.UInt32; } }
        public static Struct/*!*/ Int64 { get { return CoreSystemTypes.Int64; } }
        public static Struct/*!*/ UInt64 { get { return CoreSystemTypes.UInt64; } }
        public static Struct/*!*/ Single { get { return CoreSystemTypes.Single; } }
        public static Struct/*!*/ Double { get { return CoreSystemTypes.Double; } }
        public static Struct/*!*/ IntPtr { get { return CoreSystemTypes.IntPtr; } }
        public static Struct/*!*/ UIntPtr { get { return CoreSystemTypes.UIntPtr; } }
        public static Struct/*!*/ DynamicallyTypedReference { get { return CoreSystemTypes.DynamicallyTypedReference; } }

        // Types required for a complete rendering
        // of binary attribute information
        public static Class/*!*/ AttributeUsageAttribute;
        public static Class/*!*/ ConditionalAttribute;
        public static Class/*!*/ DefaultMemberAttribute;
        public static Class/*!*/ InternalsVisibleToAttribute;
        public static Class/*!*/ ObsoleteAttribute;

        // Types required to render arrays
        public static Interface/*!*/ GenericICollection;
        public static Interface/*!*/ GenericIEnumerable;
        public static Interface/*!*/ GenericIList;
        public static Interface/*!*/ ICloneable;
        public static Interface/*!*/ ICollection;
        public static Interface/*!*/ IEnumerable;
        public static Interface/*!*/ IList;

        //Special types
        public static Struct/*!*/ ArgIterator { get { return CoreSystemTypes.ArgIterator; } }
        public static Class/*!*/ IsVolatile { get { return CoreSystemTypes.IsVolatile; } }
        public static Struct/*!*/ Void { get { return CoreSystemTypes.Void; } }
        public static Struct/*!*/ RuntimeFieldHandle { get { return CoreSystemTypes.RuntimeTypeHandle; } }
        public static Struct/*!*/ RuntimeMethodHandle { get { return CoreSystemTypes.RuntimeTypeHandle; } }
        public static Struct/*!*/ RuntimeTypeHandle { get { return CoreSystemTypes.RuntimeTypeHandle; } }
        public static Struct/*!*/ RuntimeArgumentHandle { get { return CoreSystemTypes.RuntimeArgumentHandle; } }

        //Special attributes    
        public static Class/*!*/ AllowPartiallyTrustedCallersAttribute;
        public static Class/*!*/ AssemblyCompanyAttribute;
        public static Class/*!*/ AssemblyConfigurationAttribute;
        public static Class/*!*/ AssemblyCopyrightAttribute;
        public static Class/*!*/ AssemblyCultureAttribute;
        public static Class/*!*/ AssemblyDelaySignAttribute;
        public static Class/*!*/ AssemblyDescriptionAttribute;
        public static Class/*!*/ AssemblyFileVersionAttribute;
        public static Class/*!*/ AssemblyFlagsAttribute;
        public static Class/*!*/ AssemblyInformationalVersionAttribute;
        public static Class/*!*/ AssemblyKeyFileAttribute;
        public static Class/*!*/ AssemblyKeyNameAttribute;
        public static Class/*!*/ AssemblyProductAttribute;
        public static Class/*!*/ AssemblyTitleAttribute;
        public static Class/*!*/ AssemblyTrademarkAttribute;
        public static Class/*!*/ AssemblyVersionAttribute;
        public static Class/*!*/ ClassInterfaceAttribute;
        public static Class/*!*/ CLSCompliantAttribute;
        public static Class/*!*/ ComImportAttribute;
        public static Class/*!*/ ComRegisterFunctionAttribute;
        public static Class/*!*/ ComSourceInterfacesAttribute;
        public static Class/*!*/ ComUnregisterFunctionAttribute;
        public static Class/*!*/ ComVisibleAttribute;
        public static Class/*!*/ DebuggableAttribute;
        public static Class/*!*/ DebuggerHiddenAttribute;
        public static Class/*!*/ DebuggerStepThroughAttribute;
        public static EnumNode DebuggingModes;
        public static Class/*!*/ DllImportAttribute;
        public static Class/*!*/ FieldOffsetAttribute;
        public static Class/*!*/ FlagsAttribute;
        public static Class/*!*/ GuidAttribute;
        public static Class/*!*/ ImportedFromTypeLibAttribute;
        public static Class/*!*/ InAttribute;
        public static Class/*!*/ IndexerNameAttribute;
        public static Class/*!*/ InterfaceTypeAttribute;
        public static Class/*!*/ MethodImplAttribute;
        public static Class/*!*/ NonSerializedAttribute;
        public static Class/*!*/ OptionalAttribute;
        public static Class/*!*/ OutAttribute;
        public static Class/*!*/ ParamArrayAttribute;
        public static Class/*!*/ RuntimeCompatibilityAttribute;
        public static Class/*!*/ SatelliteContractVersionAttribute;
        public static Class/*!*/ SerializableAttribute;
        public static Class/*!*/ SecurityAttribute;
        public static Class/*!*/ SecurityCriticalAttribute;
        public static Class/*!*/ SecurityTransparentAttribute;
        public static Class/*!*/ SecurityTreatAsSafeAttribute;
        public static Class/*!*/ STAThreadAttribute;
        public static Class/*!*/ StructLayoutAttribute;
        public static Class/*!*/ SuppressMessageAttribute;
        public static Class/*!*/ SuppressUnmanagedCodeSecurityAttribute;
        public static EnumNode SecurityAction;

        //Classes need for System.TypeCode
        public static Class/*!*/ DBNull;
        public static Struct/*!*/ DateTime;
        public static Struct/*!*/ Decimal { get { return CoreSystemTypes.Decimal; } }
        public static Struct/*!*/ TimeSpan;

        //Classes and interfaces used by the Framework
        public static Class/*!*/ Activator;
        public static Class/*!*/ AppDomain;
        public static Class/*!*/ ApplicationException;
        public static Class/*!*/ ArgumentException;
        public static Class/*!*/ ArgumentNullException;
        public static Class/*!*/ ArgumentOutOfRangeException;
        public static Class/*!*/ ArrayList;
        public static DelegateNode/*!*/ AsyncCallback;
        public static Class/*!*/ Assembly;
        public static Class/*!*/ CodeAccessPermission;
        public static Class/*!*/ CollectionBase;
        public static Class/*!*/ CultureInfo;
        public static Class/*!*/ DictionaryBase;
        public static Struct/*!*/ DictionaryEntry;
        public static Class/*!*/ DuplicateWaitObjectException;
        public static Class/*!*/ Environment;
        public static Class/*!*/ EventArgs;
        public static Class/*!*/ ExecutionEngineException;
        public static Struct/*!*/ GenericArraySegment;
        public static Class/*!*/ GenericArrayToIEnumerableAdapter;
        public static Class/*!*/ GenericDictionary;
        public static Interface/*!*/ GenericIComparable;
        public static Interface/*!*/ GenericIComparer;
        public static Interface/*!*/ GenericIDictionary;
        public static Interface/*!*/ GenericIEnumerator;
        public static Struct/*!*/ GenericKeyValuePair;
        public static Class/*!*/ GenericList;
        public static Struct/*!*/ GenericNullable;
        public static Class/*!*/ GenericQueue;
        public static Class/*!*/ GenericSortedDictionary;
        public static Class/*!*/ GenericStack;
        public static Class/*!*/ GC;
        public static Struct/*!*/ Guid;
        public static Class/*!*/ __HandleProtector;
        public static Struct/*!*/ HandleRef;
        public static Class/*!*/ Hashtable;
        public static Interface/*!*/ IASyncResult;
        public static Interface/*!*/ IComparable;
        public static Interface/*!*/ IDictionary;
        public static Interface/*!*/ IComparer;
        public static Interface/*!*/ IDisposable;
        public static Interface/*!*/ IEnumerator;
        public static Interface/*!*/ IFormatProvider;
        public static Interface/*!*/ IHashCodeProvider;
        public static Interface/*!*/ IMembershipCondition;
        public static Class/*!*/ IndexOutOfRangeException;
        public static Class/*!*/ InvalidCastException;
        public static Class/*!*/ InvalidOperationException;
        public static Interface/*!*/ IPermission;
        public static Interface/*!*/ ISerializable;
        public static Interface/*!*/ IStackWalk;
        public static Class/*!*/ Marshal;
        public static Class/*!*/ MarshalByRefObject;
        public static Class/*!*/ MemberInfo;
        public static Struct/*!*/ NativeOverlapped;
        public static Class/*!*/ Monitor;
        public static Class/*!*/ NotSupportedException;
        public static Class/*!*/ NullReferenceException;
        public static Class/*!*/ OutOfMemoryException;
        public static Class/*!*/ ParameterInfo;
        public static Class/*!*/ Queue;
        public static Class/*!*/ ReadOnlyCollectionBase;
        public static Class/*!*/ ResourceManager;
        public static Class/*!*/ ResourceSet;
        public static Class/*!*/ SerializationInfo;
        public static Class/*!*/ Stack;
        public static Class/*!*/ StackOverflowException;
        public static Class/*!*/ Stream;
        public static Struct/*!*/ StreamingContext;
        public static Class/*!*/ StringBuilder;
        public static Class/*!*/ StringComparer;
        public static EnumNode StringComparison;
        public static Class/*!*/ SystemException;
        public static Class/*!*/ Thread;
        public static Class/*!*/ WindowsImpersonationContext;

        static SystemTypes()
        {
            SystemTypes.Initialize(TargetPlatform.DoNotLockFiles, TargetPlatform.GetDebugInfo);
        }

        public static void Clear()
        {
            lock (Module.GlobalLock)
            {
                CoreSystemTypes.Clear();
                SystemTypes.ClearStatics();
                SystemTypes.Initialized = false;
            }
        }
        public static void Initialize(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemTypes.Initialized)
            {
                SystemTypes.Clear();
                CoreSystemTypes.Initialize(doNotLockFile, getDebugInfo);
            }
            else if (!CoreSystemTypes.Initialized)
            {
                CoreSystemTypes.Initialize(doNotLockFile, getDebugInfo);
            }

            if (TargetPlatform.TargetVersion == null)
            {
                TargetPlatform.TargetVersion = SystemAssembly.Version;
                if (TargetPlatform.TargetVersion == null)
                    TargetPlatform.TargetVersion = typeof(object).Module.Assembly.GetName().Version;
            }
            //TODO: throw an exception when the result is null

            AttributeUsageAttribute = (Class)GetTypeNodeFor("System", "AttributeUsageAttribute", ElementType.Class);
            ConditionalAttribute = (Class)GetTypeNodeFor("System.Diagnostics", "ConditionalAttribute", ElementType.Class);
            DefaultMemberAttribute = (Class)GetTypeNodeFor("System.Reflection", "DefaultMemberAttribute", ElementType.Class);
            InternalsVisibleToAttribute = (Class)GetTypeNodeFor("System.Runtime.CompilerServices", "InternalsVisibleToAttribute", ElementType.Class);
            ObsoleteAttribute = (Class)GetTypeNodeFor("System", "ObsoleteAttribute", ElementType.Class);

            GenericICollection = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "ICollection", 1, ElementType.Class);
            GenericIEnumerable = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IEnumerable", 1, ElementType.Class);
            GenericIList = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IList", 1, ElementType.Class);
            ICloneable = (Interface)GetTypeNodeFor("System", "ICloneable", ElementType.Class);
            ICollection = (Interface)GetTypeNodeFor("System.Collections", "ICollection", ElementType.Class);
            IEnumerable = (Interface)GetTypeNodeFor("System.Collections", "IEnumerable", ElementType.Class);
            IList = (Interface)GetTypeNodeFor("System.Collections", "IList", ElementType.Class);

            AllowPartiallyTrustedCallersAttribute = (Class)GetTypeNodeFor("System.Security", "AllowPartiallyTrustedCallersAttribute", ElementType.Class);
            AssemblyCompanyAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyCompanyAttribute", ElementType.Class);
            AssemblyConfigurationAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyConfigurationAttribute", ElementType.Class);
            AssemblyCopyrightAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyCopyrightAttribute", ElementType.Class);
            AssemblyCultureAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyCultureAttribute", ElementType.Class);
            AssemblyDelaySignAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyDelaySignAttribute", ElementType.Class);
            AssemblyDescriptionAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyDescriptionAttribute", ElementType.Class);
            AssemblyFileVersionAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyFileVersionAttribute", ElementType.Class);
            AssemblyFlagsAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyFlagsAttribute", ElementType.Class);
            AssemblyInformationalVersionAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyInformationalVersionAttribute", ElementType.Class);
            AssemblyKeyFileAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyKeyFileAttribute", ElementType.Class);
            AssemblyKeyNameAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyKeyNameAttribute", ElementType.Class);
            AssemblyProductAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyProductAttribute", ElementType.Class);
            AssemblyTitleAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyTitleAttribute", ElementType.Class);
            AssemblyTrademarkAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyTrademarkAttribute", ElementType.Class);
            AssemblyVersionAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyVersionAttribute", ElementType.Class);
            ClassInterfaceAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "ClassInterfaceAttribute", ElementType.Class);
            CLSCompliantAttribute = (Class)GetTypeNodeFor("System", "CLSCompliantAttribute", ElementType.Class);
            ComImportAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "ComImportAttribute", ElementType.Class);
            ComRegisterFunctionAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "ComRegisterFunctionAttribute", ElementType.Class);
            ComSourceInterfacesAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "ComSourceInterfacesAttribute", ElementType.Class);
            ComUnregisterFunctionAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "ComUnregisterFunctionAttribute", ElementType.Class);
            ComVisibleAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "ComVisibleAttribute", ElementType.Class);
            DebuggableAttribute = (Class)GetTypeNodeFor("System.Diagnostics", "DebuggableAttribute", ElementType.Class);
            DebuggerHiddenAttribute = (Class)GetTypeNodeFor("System.Diagnostics", "DebuggerHiddenAttribute", ElementType.Class);
            DebuggerStepThroughAttribute = (Class)GetTypeNodeFor("System.Diagnostics", "DebuggerStepThroughAttribute", ElementType.Class);
            DebuggingModes = DebuggableAttribute == null ? null : DebuggableAttribute.GetNestedType(Identifier.For("DebuggingModes")) as EnumNode;
            DllImportAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "DllImportAttribute", ElementType.Class);
            FieldOffsetAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "FieldOffsetAttribute", ElementType.Class);
            FlagsAttribute = (Class)GetTypeNodeFor("System", "FlagsAttribute", ElementType.Class);
            Guid = (Struct)GetTypeNodeFor("System", "Guid", ElementType.ValueType);
            GuidAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "GuidAttribute", ElementType.Class);
            ImportedFromTypeLibAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "ImportedFromTypeLibAttribute", ElementType.Class);
            InAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "InAttribute", ElementType.Class);
            IndexerNameAttribute = (Class)GetTypeNodeFor("System.Runtime.CompilerServices", "IndexerNameAttribute", ElementType.Class);
            InterfaceTypeAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "InterfaceTypeAttribute", ElementType.Class);
            MethodImplAttribute = (Class)GetTypeNodeFor("System.Runtime.CompilerServices", "MethodImplAttribute", ElementType.Class);
            NonSerializedAttribute = (Class)GetTypeNodeFor("System", "NonSerializedAttribute", ElementType.Class);
            OptionalAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "OptionalAttribute", ElementType.Class);
            OutAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "OutAttribute", ElementType.Class);
            ParamArrayAttribute = (Class)GetTypeNodeFor("System", "ParamArrayAttribute", ElementType.Class);
            RuntimeCompatibilityAttribute = (Class)GetTypeNodeFor("System.Runtime.CompilerServices", "RuntimeCompatibilityAttribute", ElementType.Class);
            SatelliteContractVersionAttribute = (Class)GetTypeNodeFor("System.Resources", "SatelliteContractVersionAttribute", ElementType.Class);
            SerializableAttribute = (Class)GetTypeNodeFor("System", "SerializableAttribute", ElementType.Class);
            SecurityAttribute = (Class)GetTypeNodeFor("System.Security.Permissions", "SecurityAttribute", ElementType.Class);
            SecurityCriticalAttribute = (Class)GetTypeNodeFor("System.Security", "SecurityCriticalAttribute", ElementType.Class);
            SecurityTransparentAttribute = (Class)GetTypeNodeFor("System.Security", "SecurityTransparentAttribute", ElementType.Class);
            SecurityTreatAsSafeAttribute = (Class)GetTypeNodeFor("System.Security", "SecurityTreatAsSafeAttribute", ElementType.Class);
            STAThreadAttribute = (Class)GetTypeNodeFor("System", "STAThreadAttribute", ElementType.Class);
            StructLayoutAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "StructLayoutAttribute", ElementType.Class);
            SuppressMessageAttribute = (Class)GetTypeNodeFor("System.Diagnostics.CodeAnalysis", "SuppressMessageAttribute", ElementType.Class);
            SuppressUnmanagedCodeSecurityAttribute = (Class)GetTypeNodeFor("System.Security", "SuppressUnmanagedCodeSecurityAttribute", ElementType.Class);
            SecurityAction = GetTypeNodeFor("System.Security.Permissions", "SecurityAction", ElementType.ValueType) as EnumNode;
            DBNull = (Class)GetTypeNodeFor("System", "DBNull", ElementType.Class);
            DateTime = (Struct)GetTypeNodeFor("System", "DateTime", ElementType.ValueType);
            TimeSpan = (Struct)GetTypeNodeFor("System", "TimeSpan", ElementType.ValueType);
            Activator = (Class)GetTypeNodeFor("System", "Activator", ElementType.Class);
            AppDomain = (Class)GetTypeNodeFor("System", "AppDomain", ElementType.Class);
            ApplicationException = (Class)GetTypeNodeFor("System", "ApplicationException", ElementType.Class);
            ArgumentException = (Class)GetTypeNodeFor("System", "ArgumentException", ElementType.Class);
            ArgumentNullException = (Class)GetTypeNodeFor("System", "ArgumentNullException", ElementType.Class);
            ArgumentOutOfRangeException = (Class)GetTypeNodeFor("System", "ArgumentOutOfRangeException", ElementType.Class);
            ArrayList = (Class)GetTypeNodeFor("System.Collections", "ArrayList", ElementType.Class);
            AsyncCallback = (DelegateNode)GetTypeNodeFor("System", "AsyncCallback", ElementType.Class);
            Assembly = (Class)GetTypeNodeFor("System.Reflection", "Assembly", ElementType.Class);
            CodeAccessPermission = (Class)GetTypeNodeFor("System.Security", "CodeAccessPermission", ElementType.Class);
            CollectionBase = (Class)GetTypeNodeFor("System.Collections", "CollectionBase", ElementType.Class);
            CultureInfo = (Class)GetTypeNodeFor("System.Globalization", "CultureInfo", ElementType.Class);
            DictionaryBase = (Class)GetTypeNodeFor("System.Collections", "DictionaryBase", ElementType.Class);

            // EFW - In the .NET Micro Framework this is a class not a structure.  Few if any of these are
            // actually used, this one included.  I'm loathe to remove them as they may be used to ensure
            // assemblies are loaded.  Using the as operator rather than a direct cast prevent it from failing.
            DictionaryEntry = GetTypeNodeFor("System.Collections", "DictionaryEntry", ElementType.ValueType) as Struct;

            DuplicateWaitObjectException = (Class)GetTypeNodeFor("System", "DuplicateWaitObjectException", ElementType.Class);
            Environment = (Class)GetTypeNodeFor("System", "Environment", ElementType.Class);
            EventArgs = (Class)GetTypeNodeFor("System", "EventArgs", ElementType.Class);
            ExecutionEngineException = (Class)GetTypeNodeFor("System", "ExecutionEngineException", ElementType.Class);
            GenericArraySegment = (Struct)GetGenericRuntimeTypeNodeFor("System", "ArraySegment", 1, ElementType.ValueType);
            GenericDictionary = (Class)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "Dictionary", 2, ElementType.Class);
            GenericIComparable = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IComparable", 1, ElementType.Class);
            GenericIComparer = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IComparer", 1, ElementType.Class);
            GenericIDictionary = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IDictionary", 2, ElementType.Class);
            GenericIEnumerator = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IEnumerator", 1, ElementType.Class);
            GenericKeyValuePair = (Struct)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "KeyValuePair", 2, ElementType.ValueType);
            GenericList = (Class)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "List", 1, ElementType.Class);
            GenericNullable = (Struct)GetGenericRuntimeTypeNodeFor("System", "Nullable", 1, ElementType.ValueType);
            GenericQueue = (Class)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "Queue", 1, ElementType.Class);
            GenericSortedDictionary = (Class)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "SortedDictionary", 2, ElementType.Class);
            GenericStack = (Class)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "Stack", 1, ElementType.Class);
            GC = (Class)GetTypeNodeFor("System", "GC", ElementType.Class);
            __HandleProtector = (Class)GetTypeNodeFor("System.Threading", "__HandleProtector", ElementType.Class);
            HandleRef = (Struct)GetTypeNodeFor("System.Runtime.InteropServices", "HandleRef", ElementType.ValueType);
            Hashtable = (Class)GetTypeNodeFor("System.Collections", "Hashtable", ElementType.Class);
            IASyncResult = (Interface)GetTypeNodeFor("System", "IAsyncResult", ElementType.Class);
            IComparable = (Interface)GetTypeNodeFor("System", "IComparable", ElementType.Class);
            IComparer = (Interface)GetTypeNodeFor("System.Collections", "IComparer", ElementType.Class);
            IDictionary = (Interface)GetTypeNodeFor("System.Collections", "IDictionary", ElementType.Class);
            IDisposable = (Interface)GetTypeNodeFor("System", "IDisposable", ElementType.Class);
            IEnumerator = (Interface)GetTypeNodeFor("System.Collections", "IEnumerator", ElementType.Class);
            IFormatProvider = (Interface)GetTypeNodeFor("System", "IFormatProvider", ElementType.Class);
            IHashCodeProvider = (Interface)GetTypeNodeFor("System.Collections", "IHashCodeProvider", ElementType.Class);
            IMembershipCondition = (Interface)GetTypeNodeFor("System.Security.Policy", "IMembershipCondition", ElementType.Class);
            IndexOutOfRangeException = (Class)GetTypeNodeFor("System", "IndexOutOfRangeException", ElementType.Class);
            InvalidCastException = (Class)GetTypeNodeFor("System", "InvalidCastException", ElementType.Class);
            InvalidOperationException = (Class)GetTypeNodeFor("System", "InvalidOperationException", ElementType.Class);
            IPermission = (Interface)GetTypeNodeFor("System.Security", "IPermission", ElementType.Class);
            ISerializable = (Interface)GetTypeNodeFor("System.Runtime.Serialization", "ISerializable", ElementType.Class);
            IStackWalk = (Interface)GetTypeNodeFor("System.Security", "IStackWalk", ElementType.Class);
            Marshal = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "Marshal", ElementType.Class);
            MarshalByRefObject = (Class)GetTypeNodeFor("System", "MarshalByRefObject", ElementType.Class);
            MemberInfo = (Class)GetTypeNodeFor("System.Reflection", "MemberInfo", ElementType.Class);
            Monitor = (Class)GetTypeNodeFor("System.Threading", "Monitor", ElementType.Class);
            NativeOverlapped = (Struct)GetTypeNodeFor("System.Threading", "NativeOverlapped", ElementType.ValueType);
            NotSupportedException = (Class)GetTypeNodeFor("System", "NotSupportedException", ElementType.Class);
            NullReferenceException = (Class)GetTypeNodeFor("System", "NullReferenceException", ElementType.Class);
            OutOfMemoryException = (Class)GetTypeNodeFor("System", "OutOfMemoryException", ElementType.Class);
            ParameterInfo = (Class)GetTypeNodeFor("System.Reflection", "ParameterInfo", ElementType.Class);
            Queue = (Class)GetTypeNodeFor("System.Collections", "Queue", ElementType.Class);
            ReadOnlyCollectionBase = (Class)GetTypeNodeFor("System.Collections", "ReadOnlyCollectionBase", ElementType.Class);
            ResourceManager = (Class)GetTypeNodeFor("System.Resources", "ResourceManager", ElementType.Class);
            ResourceSet = (Class)GetTypeNodeFor("System.Resources", "ResourceSet", ElementType.Class);
            SerializationInfo = (Class)GetTypeNodeFor("System.Runtime.Serialization", "SerializationInfo", ElementType.Class);
            Stack = (Class)GetTypeNodeFor("System.Collections", "Stack", ElementType.Class);
            StackOverflowException = (Class)GetTypeNodeFor("System", "StackOverflowException", ElementType.Class);
            Stream = (Class)GetTypeNodeFor("System.IO", "Stream", ElementType.Class);
            StreamingContext = (Struct)GetTypeNodeFor("System.Runtime.Serialization", "StreamingContext", ElementType.ValueType);
            StringBuilder = (Class)GetTypeNodeFor("System.Text", "StringBuilder", ElementType.Class);
            StringComparer = (Class)GetTypeNodeFor("System", "StringComparer", ElementType.Class);
            StringComparison = GetTypeNodeFor("System", "StringComparison", ElementType.ValueType) as EnumNode;
            SystemException = (Class)GetTypeNodeFor("System", "SystemException", ElementType.Class);
            Thread = (Class)GetTypeNodeFor("System.Threading", "Thread", ElementType.Class);
            WindowsImpersonationContext = (Class)GetTypeNodeFor("System.Security.Principal", "WindowsImpersonationContext", ElementType.Class);

            SystemTypes.Initialized = true;
            object dummy = TargetPlatform.AssemblyReferenceFor; //Force selection of target platform
            if (dummy == null) return;
        }
        private static void ClearStatics()
        {
            AttributeUsageAttribute = null;
            ConditionalAttribute = null;
            DefaultMemberAttribute = null;
            InternalsVisibleToAttribute = null;
            ObsoleteAttribute = null;

            GenericICollection = null;
            GenericIEnumerable = null;
            GenericIList = null;
            ICloneable = null;
            ICollection = null;
            IEnumerable = null;
            IList = null;

            //Special attributes    
            AllowPartiallyTrustedCallersAttribute = null;
            AssemblyCompanyAttribute = null;
            AssemblyConfigurationAttribute = null;
            AssemblyCopyrightAttribute = null;
            AssemblyCultureAttribute = null;
            AssemblyDelaySignAttribute = null;
            AssemblyDescriptionAttribute = null;
            AssemblyFileVersionAttribute = null;
            AssemblyFlagsAttribute = null;
            AssemblyInformationalVersionAttribute = null;
            AssemblyKeyFileAttribute = null;
            AssemblyKeyNameAttribute = null;
            AssemblyProductAttribute = null;
            AssemblyTitleAttribute = null;
            AssemblyTrademarkAttribute = null;
            AssemblyVersionAttribute = null;
            ClassInterfaceAttribute = null;
            CLSCompliantAttribute = null;
            ComImportAttribute = null;
            ComRegisterFunctionAttribute = null;
            ComSourceInterfacesAttribute = null;
            ComUnregisterFunctionAttribute = null;
            ComVisibleAttribute = null;
            DebuggableAttribute = null;
            DebuggerHiddenAttribute = null;
            DebuggerStepThroughAttribute = null;
            DebuggingModes = null;
            DllImportAttribute = null;
            FieldOffsetAttribute = null;
            FlagsAttribute = null;
            GuidAttribute = null;
            ImportedFromTypeLibAttribute = null;
            InAttribute = null;
            IndexerNameAttribute = null;
            InterfaceTypeAttribute = null;
            MethodImplAttribute = null;
            NonSerializedAttribute = null;
            OptionalAttribute = null;
            OutAttribute = null;
            ParamArrayAttribute = null;
            RuntimeCompatibilityAttribute = null;
            SatelliteContractVersionAttribute = null;
            SerializableAttribute = null;
            SecurityAttribute = null;
            SecurityCriticalAttribute = null;
            SecurityTransparentAttribute = null;
            SecurityTreatAsSafeAttribute = null;
            STAThreadAttribute = null;
            StructLayoutAttribute = null;
            SuppressMessageAttribute = null;
            SuppressUnmanagedCodeSecurityAttribute = null;
            SecurityAction = null;

            //Classes need for System.TypeCode
            DBNull = null;
            DateTime = null;
            TimeSpan = null;

            //Classes and interfaces used by the Framework
            Activator = null;
            AppDomain = null;
            ApplicationException = null;
            ArgumentException = null;
            ArgumentNullException = null;
            ArgumentOutOfRangeException = null;
            ArrayList = null;
            AsyncCallback = null;
            Assembly = null;
            CodeAccessPermission = null;
            CollectionBase = null;
            CultureInfo = null;
            DictionaryBase = null;
            DictionaryEntry = null;
            DuplicateWaitObjectException = null;
            Environment = null;
            EventArgs = null;
            ExecutionEngineException = null;
            GenericArraySegment = null;
            GenericArrayToIEnumerableAdapter = null;
            GenericDictionary = null;
            GenericIComparable = null;
            GenericIComparer = null;
            GenericIDictionary = null;
            GenericIEnumerator = null;
            GenericKeyValuePair = null;
            GenericList = null;
            GenericNullable = null;
            GenericQueue = null;
            GenericSortedDictionary = null;
            GenericStack = null;
            GC = null;
            Guid = null;
            __HandleProtector = null;
            HandleRef = null;
            Hashtable = null;
            IASyncResult = null;
            IComparable = null;
            IDictionary = null;
            IComparer = null;
            IDisposable = null;
            IEnumerator = null;
            IFormatProvider = null;
            IHashCodeProvider = null;
            IMembershipCondition = null;
            IndexOutOfRangeException = null;
            InvalidCastException = null;
            InvalidOperationException = null;
            IPermission = null;
            ISerializable = null;
            IStackWalk = null;
            Marshal = null;
            MarshalByRefObject = null;
            MemberInfo = null;
            NativeOverlapped = null;
            Monitor = null;
            NotSupportedException = null;
            NullReferenceException = null;
            OutOfMemoryException = null;
            ParameterInfo = null;
            Queue = null;
            ReadOnlyCollectionBase = null;
            ResourceManager = null;
            ResourceSet = null;
            SerializationInfo = null;
            Stack = null;
            StackOverflowException = null;
            Stream = null;
            StreamingContext = null;
            StringBuilder = null;
            StringComparer = null;
            StringComparison = null;
            SystemException = null;
            Thread = null;
            WindowsImpersonationContext = null;
        }

        private static TypeNode/*!*/ GetGenericRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, int numParams, ElementType typeCode)
        {
            if (TargetPlatform.GenericTypeNamesMangleChar != 0) name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;

            return SystemTypes.GetTypeNodeFor(nspace, name, typeCode);
        }
        private static TypeNode/*!*/ GetTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode)
        {
            TypeNode result = null;
            if (SystemAssembly == null)
                Debug.Assert(false);
            else
                result = SystemAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemAssembly, nspace, name, typeCode);
            result.typeCode = typeCode;
            return result;
        }
    }
}
