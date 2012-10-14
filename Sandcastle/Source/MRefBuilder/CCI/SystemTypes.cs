// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 09/10/2012 - EFW - Added support to the TargetPlatform class for using the Frameworks.xml file to load
// framework assembly information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

#if FxCop
using InterfaceList = Microsoft.Cci.InterfaceCollection;
using TypeNodeList = Microsoft.Cci.TypeNodeCollection;
using Module = Microsoft.Cci.ModuleNode;
using Class = Microsoft.Cci.ClassNode;
using Interface = Microsoft.Cci.InterfaceNode;
#endif
#if CCINamespace
using Microsoft.Cci.Metadata;
#else
using System.Compiler.Metadata;
#endif

#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler
{
#endif
#if !FxCop
    public
#endif
 sealed class SystemAssemblyLocation
    {
        static string location;
        public static string Location
        {
            get
            {
                return location;
            }
            set
            {
                //Debug.Assert(location == null || location == value, string.Format("You attempted to set the mscorlib.dll location to\r\n\r\n{0}\r\n\r\nbut it was already set to\r\n\r\n{1}\r\n\r\nThis may occur if you have multiple projects that target different platforms. Make sure all of your projects target the same platform.\r\n\r\nYou may try to continue, but targeting multiple platforms during the same session is not supported, so you may see erroneous behavior.", value, location));
                location = value;
            }
        }
        public static AssemblyNode ParsedAssembly;
    }
#if ExtendedRuntime
  public sealed class SystemCompilerRuntimeAssemblyLocation{
    public static string Location {
      get { return location; }
      set {
        location = value;
        Identifier id = Identifier.For("System.Compiler.Runtime");
        AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[id.UniqueIdKey];
        if (aref == null) {
          aref = new AssemblyReference(typeof(ComposerAttribute).Assembly.FullName);
          TargetPlatform.AssemblyReferenceFor[id.UniqueIdKey] = aref;
        }
        aref.Location = value;
      }
    }
    private static string location = null; //Can be set by compiler in cross compilation scenarios
    public static AssemblyNode ParsedAssembly;
  }
#endif
#if !NoData && !ROTOR
    public sealed class SystemDataAssemblyLocation
    {
        public static string Location = null;
    }
#endif
#if !NoXml && !NoRuntimeXml
    public sealed class SystemXmlAssemblyLocation
    {
        public static string Location = null;
    }
#endif
#if !FxCop
    public
#endif
 sealed class TargetPlatform
    {
        private TargetPlatform() { }
        public static bool DoNotLockFiles;
        public static bool GetDebugInfo;
        public static char GenericTypeNamesMangleChar = '_';

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
            SystemAssemblyLocation.Location = null;
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = null;
#endif
#if !NoData && !ROTOR
            SystemDataAssemblyLocation.Location = null;
#endif
#if !NoXml && !NoRuntimeXml
            SystemXmlAssemblyLocation.Location = null;
#endif
            TargetPlatform.DoNotLockFiles = false;
            TargetPlatform.GetDebugInfo = false;
            TargetPlatform.PlatformAssembliesLocation = "";
            SystemTypes.Clear();
        }
        public static System.Collections.IDictionary StaticAssemblyCache
        {
            get { return Reader.StaticAssemblyCache; }
        }
        public static Version TargetVersion =
#if WHIDBEY
 new Version(2, 0, 50727);  // Default for a WHIDBEY compiler
#else
      new Version(1, 0, 5000);  // Default for an Everett compiler
#endif
        public static string TargetRuntimeVersion;

        public static int LinkerMajorVersion
        {
            get
            {
                switch (TargetVersion.Major)
                {
                    case 2: return 8;
                    case 1: return 7;
                    default: return 6;
                }
            }
        }
        public static int LinkerMinorVersion
        {
            get
            {
                return TargetVersion.Minor;
            }
        }

        public static int MajorVersion { get { return TargetVersion.Major; } }
        public static int MinorVersion { get { return TargetVersion.Minor; } }
        public static int Build { get { return TargetVersion.Build; } }

        public static string/*!*/ PlatformAssembliesLocation = String.Empty;
        private static TrivialHashtable assemblyReferenceFor;
        public static TrivialHashtable/*!*/ AssemblyReferenceFor
        {
            get
            {
                if (TargetPlatform.assemblyReferenceFor == null)
                    TargetPlatform.SetupAssemblyReferenceFor();
                //^ assume TargetPlatform.assemblyReferenceFor != null;
                return TargetPlatform.assemblyReferenceFor;
            }
            set
            {
                TargetPlatform.assemblyReferenceFor = value;
            }
        }
#if !FxCop
        private readonly static string[]/*!*/ FxAssemblyNames =
          new string[]{"Accessibility", "CustomMarshalers", "IEExecRemote", "IEHost", "IIEHost", "ISymWrapper", 
                    "Microsoft.JScript", "Microsoft.VisualBasic", "Microsoft.VisualBasic.Vsa", "Microsoft.VisualC",
                    "Microsoft.Vsa", "Microsoft.Vsa.Vb.CodeDOMProcessor", "mscorcfg", "Regcode", "System",
                    "System.Configuration.Install", "System.Data", "System.Design", "System.DirectoryServices",
                    "System.Drawing", "System.Drawing.Design", "System.EnterpriseServices", 
                    "System.Management", "System.Messaging", "System.Runtime.Remoting", "System.Runtime.Serialization.Formatters.Soap",
                    "System.Security", "System.ServiceProcess", "System.Web", "System.Web.Mobile", "System.Web.RegularExpressions",
                    "System.Web.Services", "System.Windows.Forms", "System.Xml", "TlbExpCode", "TlbImpCode", "cscompmgd",
                    "vjswfchtml", "vjswfccw", "VJSWfcBrowserStubLib", "vjswfc", "vjslibcw", "vjslib", "vjscor", "VJSharpCodeProvider"};
        private readonly static string[]/*!*/ FxAssemblyToken =
          new string[]{"b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a",
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a",
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b77a5c561934e089",
                    "b03f5f7f11d50a3a", "b77a5c561934e089", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a",
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", 
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b77a5c561934e089", "b03f5f7f11d50a3a",
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a",
                    "b03f5f7f11d50a3a", "b77a5c561934e089", "b77a5c561934e089", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a",
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a"};
        private readonly static string[]/*!*/ FxAssemblyVersion1 =
          new string[]{"1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
                    "7.0.3300.0", "7.0.3300.0", "7.0.3300.0", "7.0.3300.0",
                    "7.0.3300.0", "7.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", 
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "7.0.3300.0",
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "7.0.3300.0"};
        private readonly static string[]/*!*/ FxAssemblyVersion1_1 =
          new string[]{"1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0",
                    "7.0.5000.0", "7.0.5000.0", "7.0.5000.0", "7.0.5000.0",
                    "7.0.5000.0", "7.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0",
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0",
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", 
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", 
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0",
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "7.0.5000.0",
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "7.0.5000.0"};
        private static string[]/*!*/ FxAssemblyVersion2Build3600 =
          new string[]{"2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0",
                    "8.0.1200.0", "8.0.1200.0", "8.0.1200.0", "8.0.1200.0",
                    "8.0.1200.0", "8.0.1200.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0",
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0",
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", 
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", 
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0",
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "8.0.1200.0",
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "7.0.5000.0"};
        private static string[]/*!*/ FxAssemblyVersion2 =
          new string[]{"2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0",
                    "8.0.0.0", "8.0.0.0", "8.0.0.0", "8.0.0.0",
                    "8.0.0.0", "8.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0",
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0",
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", 
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", 
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0",
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "8.0.0.0",
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0"};
#endif
        private static void SetupAssemblyReferenceFor()
        {
#if FxCop
      TargetPlatform.SetToPostV1_1(Path.GetDirectoryName(typeof(object).Module.Assembly.Location));
#else
            Version version = TargetPlatform.TargetVersion;
            if (version == null) version = typeof(object).Module.Assembly.GetName().Version;
            TargetPlatform.SetTo(version);
#endif
        }
#if !FxCop
        public static void SetTo(Version/*!*/ version)
        {
            if (version == null) throw new ArgumentNullException();
            if (version.Major == 1)
            {
                if (version.Minor == 0 && version.Build == 3300) TargetPlatform.SetToV1();
                else if (version.Minor == 0 && version.Build == 5000) TargetPlatform.SetToV1_1();
                else if (version.Minor == 1 && version.Build == 9999) TargetPlatform.SetToPostV1_1(TargetPlatform.PlatformAssembliesLocation);
            }
            else if (version.Major == 2)
            {
                if (version.Minor == 0 && version.Build == 3600) TargetPlatform.SetToV2Beta1();
                else TargetPlatform.SetToV2();
            }
            else
                TargetPlatform.SetToV1();
        }
        public static void SetTo(Version/*!*/ version, string/*!*/ platformAssembliesLocation)
        {
            if (version == null || platformAssembliesLocation == null) throw new ArgumentNullException();
            if (version.Major == 1)
            {
                if (version.Minor == 0 && version.Build == 3300) TargetPlatform.SetToV1(platformAssembliesLocation);
                else if (version.Minor == 0 && version.Build == 5000) TargetPlatform.SetToV1_1(platformAssembliesLocation);
                else if (version.Minor == 1 && version.Build == 9999) TargetPlatform.SetToPostV1_1(platformAssembliesLocation);
            }
            else if (version.Major == 2)
            {
                if (version.Minor == 0 && version.Build == 3600) TargetPlatform.SetToV2Beta1(platformAssembliesLocation);
                else TargetPlatform.SetToV2(platformAssembliesLocation);
            }
            else
                TargetPlatform.SetToV1(platformAssembliesLocation);
        }
        public static void SetToV1()
        {
            TargetPlatform.SetToV1(TargetPlatform.PlatformAssembliesLocation);
        }
        public static void SetToV1(string platformAssembliesLocation)
        {
            TargetPlatform.TargetVersion = new Version(1, 0, 3300);
            TargetPlatform.TargetRuntimeVersion = "v1.0.3705";
            if (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0)
                platformAssembliesLocation = TargetPlatform.PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Module.Assembly.Location), "..\\v1.0.3705");
            else
                TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
            TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            TrivialHashtable assemblyReferenceFor = new TrivialHashtable(46);
            for (int i = 0, n = TargetPlatform.FxAssemblyNames.Length; i < n; i++)
            {
                string name = TargetPlatform.FxAssemblyNames[i];
                string version = TargetPlatform.FxAssemblyVersion1[i];
                string token = TargetPlatform.FxAssemblyToken[i];
                AssemblyReference aref = new AssemblyReference(name + ", Version=" + version + ", Culture=neutral, PublicKeyToken=" + token);
                aref.Location = platformAssembliesLocation + "\\" + name + ".dll";
                //^ assume name != null;
                assemblyReferenceFor[Identifier.For(name).UniqueIdKey] = aref;
            }
            TargetPlatform.assemblyReferenceFor = assemblyReferenceFor;
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = SystemCompilerRuntimeAssemblyLocation.Location;
#endif
        }
        public static void SetToV1_1()
        {
            TargetPlatform.SetToV1_1(TargetPlatform.PlatformAssembliesLocation);
        }
        public static void SetToV1_1(string/*!*/ platformAssembliesLocation)
        {
            TargetPlatform.TargetVersion = new Version(1, 0, 5000);
            TargetPlatform.TargetRuntimeVersion = "v1.1.4322";
            if (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0)
                platformAssembliesLocation = TargetPlatform.PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Module.Assembly.Location), "..\\v1.1.4322");
            else
                TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
            TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            TrivialHashtable assemblyReferenceFor = new TrivialHashtable(46);
            for (int i = 0, n = TargetPlatform.FxAssemblyNames.Length; i < n; i++)
            {
                string name = TargetPlatform.FxAssemblyNames[i];
                string version = TargetPlatform.FxAssemblyVersion1_1[i];
                string token = TargetPlatform.FxAssemblyToken[i];
                AssemblyReference aref = new AssemblyReference(name + ", Version=" + version + ", Culture=neutral, PublicKeyToken=" + token);
                aref.Location = platformAssembliesLocation + "\\" + name + ".dll";
                //^ assume name != null;
                assemblyReferenceFor[Identifier.For(name).UniqueIdKey] = aref;
            }
            TargetPlatform.assemblyReferenceFor = assemblyReferenceFor;
        }
        public static void SetToV2()
        {
            TargetPlatform.SetToV2(TargetPlatform.PlatformAssembliesLocation);
        }
        public static void SetToV2(string platformAssembliesLocation)
        {
            TargetPlatform.TargetVersion = new Version(2, 0, 50727);
            TargetPlatform.TargetRuntimeVersion = "v2.0.50727";
            TargetPlatform.GenericTypeNamesMangleChar = '`';
            if (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0)
                platformAssembliesLocation = TargetPlatform.PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Module.Assembly.Location), "..\\v2.0.50727");
            else
                TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
            TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
            TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            TrivialHashtable assemblyReferenceFor = new TrivialHashtable(46);
            for (int i = 0, n = TargetPlatform.FxAssemblyNames.Length; i < n; i++)
            {
                string name = TargetPlatform.FxAssemblyNames[i];
                string version = TargetPlatform.FxAssemblyVersion2[i];
                string token = TargetPlatform.FxAssemblyToken[i];
                AssemblyReference aref = new AssemblyReference(name + ", Version=" + version + ", Culture=neutral, PublicKeyToken=" + token);
                aref.Location = platformAssembliesLocation + "\\" + name + ".dll";
                //^ assume name != null;
                assemblyReferenceFor[Identifier.For(name).UniqueIdKey] = aref;
            }
            TargetPlatform.assemblyReferenceFor = assemblyReferenceFor;
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = SystemCompilerRuntimeAssemblyLocation.Location;
#endif
        }
        public static void SetToV2Beta1()
        {
            TargetPlatform.SetToV2Beta1(TargetPlatform.PlatformAssembliesLocation);
        }
        public static void SetToV2Beta1(string/*!*/ platformAssembliesLocation)
        {
            TargetPlatform.TargetVersion = new Version(2, 0, 3600);
            TargetPlatform.GenericTypeNamesMangleChar = '!';
            string dotNetDirLocation = null;
            if (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0)
            {
                DirectoryInfo dotNetDir = new FileInfo(new Uri(typeof(object).Module.Assembly.Location).LocalPath).Directory.Parent;
                dotNetDirLocation = dotNetDir.FullName;
                if (dotNetDirLocation != null) dotNetDirLocation = dotNetDirLocation.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
                DateTime creationTime = DateTime.MinValue;
                foreach (DirectoryInfo subdir in dotNetDir.GetDirectories("v2.0*"))
                {
                    if (subdir == null) continue;
                    if (subdir.CreationTime < creationTime) continue;
                    FileInfo[] mscorlibs = subdir.GetFiles("mscorlib.dll");
                    if (mscorlibs != null && mscorlibs.Length == 1)
                    {
                        platformAssembliesLocation = subdir.FullName;
                        creationTime = subdir.CreationTime;
                    }
                }
            }
            else
                TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
            if (dotNetDirLocation != null && (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0))
            {
                int pos = dotNetDirLocation.IndexOf("FRAMEWORK");
                if (pos > 0 && dotNetDirLocation.IndexOf("FRAMEWORK64") < 0)
                {
                    dotNetDirLocation = dotNetDirLocation.Replace("FRAMEWORK", "FRAMEWORK64");
                    if (Directory.Exists(dotNetDirLocation))
                    {
                        DirectoryInfo dotNetDir = new DirectoryInfo(dotNetDirLocation);
                        DateTime creationTime = DateTime.MinValue;
                        foreach (DirectoryInfo subdir in dotNetDir.GetDirectories("v2.0*"))
                        {
                            if (subdir == null) continue;
                            if (subdir.CreationTime < creationTime) continue;
                            FileInfo[] mscorlibs = subdir.GetFiles("mscorlib.dll");
                            if (mscorlibs != null && mscorlibs.Length == 1)
                            {
                                platformAssembliesLocation = subdir.FullName;
                                creationTime = subdir.CreationTime;
                            }
                        }
                    }
                }
            }
            TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
            TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            TrivialHashtable assemblyReferenceFor = new TrivialHashtable(46);
            for (int i = 0, n = TargetPlatform.FxAssemblyNames.Length; i < n; i++)
            {
                string name = TargetPlatform.FxAssemblyNames[i];
                string version = TargetPlatform.FxAssemblyVersion2Build3600[i];
                string token = TargetPlatform.FxAssemblyToken[i];
                AssemblyReference aref = new AssemblyReference(name + ", Version=" + version + ", Culture=neutral, PublicKeyToken=" + token);
                aref.Location = platformAssembliesLocation + "\\" + name + ".dll";
                //^ assume name != null;
                assemblyReferenceFor[Identifier.For(name).UniqueIdKey] = aref;
            }
            TargetPlatform.assemblyReferenceFor = assemblyReferenceFor;
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = SystemCompilerRuntimeAssemblyLocation.Location;
#endif
        }
#endif
        /// <summary>
        /// Use this to set the target platform to a platform with a superset of the platform assemblies in version 1.1, but
        /// where the public key tokens and versions numbers are determined by reading in the actual assemblies from
        /// the supplied location. Only assemblies recognized as platform assemblies in version 1.1 will be unified.
        /// </summary>
        public static void SetToPostV1_1(string/*!*/ platformAssembliesLocation)
        {
            TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
            TargetPlatform.TargetVersion = new Version(1, 1, 9999);
            TargetPlatform.TargetRuntimeVersion = "v1.1.9999";
            TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
#if FxCop
      TargetPlatform.assemblyReferenceFor = new TrivialHashtable(0);
#else
            TargetPlatform.assemblyReferenceFor = new TrivialHashtable(46);
            string[] dlls = Directory.GetFiles(platformAssembliesLocation, "*.dll");
            foreach (string dll in dlls)
            {
                if (dll == null) continue;
                string assemName = Path.GetFileNameWithoutExtension(dll);
                int i = Array.IndexOf(TargetPlatform.FxAssemblyNames, assemName);
                if (i < 0) continue;
                AssemblyNode assem = AssemblyNode.GetAssembly(Path.Combine(platformAssembliesLocation, dll));
                if (assem == null) continue;
                TargetPlatform.assemblyReferenceFor[Identifier.For(assem.Name).UniqueIdKey] = new AssemblyReference(assem);
            }
#endif
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = SystemCompilerRuntimeAssemblyLocation.Location;
#endif
        }
        private static void InitializeStandardAssemblyLocationsWithDefaultValues(string platformAssembliesLocation)
        {
            SystemAssemblyLocation.Location = platformAssembliesLocation + "\\mscorlib.dll";
#if ExtendedRuntime
      if (SystemCompilerRuntimeAssemblyLocation.Location == null)
#if CCINamespace
        SystemCompilerRuntimeAssemblyLocation.Location = platformAssembliesLocation+"\\Microsoft.Cci.Runtime.dll";
#else
        SystemCompilerRuntimeAssemblyLocation.Location = platformAssembliesLocation+"\\system.compiler.runtime.dll";
#endif
      // If the System.Compiler.Runtime assembly does not exist at this location, DO NOTHING (don't load another one)
      // as this signals the fact that the types may need to be loaded from the SystemAssembly instead.
#endif
#if !NoData && !ROTOR
            if (SystemDataAssemblyLocation.Location == null)
                SystemDataAssemblyLocation.Location = platformAssembliesLocation + "\\system.data.dll";
#endif
#if !NoXml && !NoRuntimeXml
            if (SystemXmlAssemblyLocation.Location == null)
                SystemXmlAssemblyLocation.Location = platformAssembliesLocation + "\\system.xml.dll";
#endif
        }

        //!EFW
        /// <summary>
        /// Load target framework settings and assembly details
        /// </summary>
        /// <param name="platformType">The platform type</param>
        /// <param name="version">The framework version</param>
        public static void SetFrameworkInformation(string platformType, string version)
        {
            var fd = Microsoft.Ddue.Tools.Frameworks.FrameworkDictionary.LoadSandcastleFrameworkDictionary();

            var fs = fd.FrameworkMatching(platformType, new Version(version), true);

            if(fs == null)
                throw new InvalidOperationException(String.Format("Unable to locate information for the " +
                    "framework version '{0} {1}' or a suitable redirected version on this system",
                    platformType, version));

            var coreLocation = fs.AssemblyLocations.First(l => l.IsCoreLocation);

            if(coreLocation == null)
                throw new InvalidOperationException(String.Format("A core framework location has not been " +
                    "defined for the framework '{0} {1}'", platformType, version));

            TargetPlatform.TargetVersion = fs.Version;
            TargetPlatform.TargetRuntimeVersion = "v" + fs.Version.ToString();
            TargetPlatform.GenericTypeNamesMangleChar = '`';
            TargetPlatform.PlatformAssembliesLocation = coreLocation.Path;

            // Set references to the common core framework assemblies
            var ad = fs.FindAssembly("mscorlib");

            if(ad != null)
                SystemAssemblyLocation.Location = ad.Filename;

            ad = fs.FindAssembly("System.Data");

            if(ad != null)
                SystemDataAssemblyLocation.Location = ad.Filename;

            ad = fs.FindAssembly("System.Xml");

            if(ad != null)
                SystemXmlAssemblyLocation.Location = ad.Filename;

            // Load references to all the other framework assemblies
            var allAssemblies = fs.AllAssemblies.ToList();

            TrivialHashtable assemblyReferenceFor = new TrivialHashtable(allAssemblies.Count);

            // Loading mscorlib causes a reset of the reference cache and other info so we must ignore it.
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
#if ExtendedRuntime
  public sealed class ExtendedRuntimeTypes{ //TODO: move all types from System.Compiler.Runtime into here.
    public static AssemblyNode/*!*/ SystemCompilerRuntimeAssembly;

    public static Interface/*!*/ ConstrainedType;
    public static Interface/*!*/ ITemplateParameter;
    public static Class/*!*/ NullableType;
    public static Class/*!*/ NonNullType;
    public static Class/*!*/ NotNullAttribute;
    public static Class/*!*/ NotNullGenericArgumentsAttribute;
    public static Class/*!*/ DelayedAttribute;
    public static Class/*!*/ NotDelayedAttribute;
    public static Class/*!*/ EncodedTypeSpecAttribute;
    public static Class/*!*/ StrictReadonlyAttribute;
    public static Interface/*!*/ TupleType;
    public static Interface/*!*/ TypeAlias;
    public static Interface/*!*/ TypeDefinition;
    public static Interface/*!*/ TypeIntersection;
    public static Interface/*!*/ TypeUnion;

    public static Method nonNullTypeAssertInitialized;
    public static Method NonNullTypeAssertInitialized {
      get {
        if (nonNullTypeAssertInitialized == null) {
          if (NonNullType != null) {
            nonNullTypeAssertInitialized = NonNullType.GetMethod(Identifier.For("AssertInitialized"), SystemTypes.Object);
          }
        }
        return nonNullTypeAssertInitialized;
      }
    }

    public static Method nonNullTypeAssertInitializedGeneric;
    public static Method NonNullTypeAssertInitializedGeneric {
      get {
        if (nonNullTypeAssertInitializedGeneric != null) {
          return nonNullTypeAssertInitializedGeneric;
        }

        if (NonNullType != null) {
          MemberList ml = NonNullType.GetMembersNamed(Identifier.For("AssertInitialized"));
          if (ml != null && ml.Count > 0) {
            foreach (Member mem in ml) {
              Method m = mem as Method;
              if (m == null) continue;
              if (m.IsGeneric) {
                nonNullTypeAssertInitializedGeneric = m;
                break;
              }
            }
          }
        }

        return nonNullTypeAssertInitializedGeneric;
      }
    }
    static ExtendedRuntimeTypes(){
      ExtendedRuntimeTypes.Initialize(TargetPlatform.DoNotLockFiles, TargetPlatform.GetDebugInfo);
    }

    public static void Initialize(bool doNotLockFile, bool getDebugInfo) {
      SystemCompilerRuntimeAssembly = ExtendedRuntimeTypes.GetSystemCompilerRuntimeAssembly(doNotLockFile, getDebugInfo);
      if (SystemCompilerRuntimeAssembly == null) throw new InvalidOperationException(ExceptionStrings.InternalCompilerError);
#if CCINamespace
      const string CciNs = "Microsoft.Cci";
      const string ContractsNs = "Microsoft.Contracts";
      //const string CompilerGuardsNs = "Microsoft.Contracts";
#else
      const string CciNs = "System.Compiler";
      const string ContractsNs = "Microsoft.Contracts";
      //const string CompilerGuardsNs = "Microsoft.Contracts";
#endif
      ConstrainedType = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "IConstrainedType", ElementType.Class);
      ITemplateParameter = (Interface)GetCompilerRuntimeTypeNodeFor(CciNs, "ITemplateParameter", ElementType.Class);
      NullableType = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NullableType", ElementType.Class);
      NonNullType = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NonNullType", ElementType.Class);
      NotNullAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NotNullAttribute", ElementType.Class);
      NotNullGenericArgumentsAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NotNullGenericArgumentsAttribute", ElementType.Class);
      DelayedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "DelayedAttribute", ElementType.Class);
      NotDelayedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NotDelayedAttribute", ElementType.Class);
      EncodedTypeSpecAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "EncodedTypeSpecAttribute", ElementType.Class);
      StrictReadonlyAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "StrictReadonlyAttribute", ElementType.Class);
      TupleType = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ITupleType", ElementType.Class);
      TypeAlias = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ITypeAlias", ElementType.Class);
      TypeDefinition = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ITypeDefinition", ElementType.Class);
      TypeIntersection = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ITypeIntersection", ElementType.Class);
      TypeUnion = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ITypeUnion", ElementType.Class);
    }

    public static void Clear(){
      lock (Module.GlobalLock){
        if (ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly != AssemblyNode.Dummy && ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly != null) {
          ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly.Dispose();
          ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly = null;
        }
        ConstrainedType = null;
        ITemplateParameter = null;
        NullableType = null;
        NonNullType = null;
        NotNullAttribute = null;
        NotNullGenericArgumentsAttribute = null;
        DelayedAttribute = null;
        NotDelayedAttribute = null;
        EncodedTypeSpecAttribute = null;
        StrictReadonlyAttribute = null;
        TupleType = null;
        TypeAlias = null;
        TypeDefinition = null;
        TypeIntersection = null;
        TypeUnion = null;
        nonNullTypeAssertInitialized = null;
        nonNullTypeAssertInitializedGeneric = null;
      }
    }
    private static AssemblyNode/*!*/ GetSystemCompilerRuntimeAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemCompilerRuntimeAssemblyLocation.ParsedAssembly != null) 
        return SystemCompilerRuntimeAssemblyLocation.ParsedAssembly;
      if (SystemCompilerRuntimeAssemblyLocation.Location == null || SystemCompilerRuntimeAssemblyLocation.Location.Length == 0)
        SystemCompilerRuntimeAssemblyLocation.Location = typeof(ComposerAttribute).Module.Assembly.Location;
      AssemblyNode result = (AssemblyNode)(new Reader(SystemCompilerRuntimeAssemblyLocation.Location, null, doNotLockFile, getDebugInfo, true, false)).ReadModule();
      if (result == null) {
        if (CoreSystemTypes.SystemAssembly.GetType(Identifier.For("Microsoft.Contracts"), Identifier.For("NonNullType")) != null) {
          result = SystemTypes.SystemAssembly;
        } else {
          SystemCompilerRuntimeAssemblyLocation.Location = typeof(ComposerAttribute).Module.Assembly.Location;
          result = (AssemblyNode)(new Reader(SystemCompilerRuntimeAssemblyLocation.Location, null, doNotLockFile, getDebugInfo, true, false)).ReadModule();
        }
      }
      if (result == null) {
        result = new AssemblyNode();
        System.Reflection.AssemblyName aname = typeof(ComposerAttribute).Module.Assembly.GetName();
        result.Name = aname.Name;
        result.Version = aname.Version;
        result.PublicKeyOrToken = aname.GetPublicKeyToken();
      }
      TargetPlatform.AssemblyReferenceFor[Identifier.For(result.Name).UniqueIdKey] = new AssemblyReference(result);
      return result;
    }
    private static TypeNode/*!*/ GetCompilerRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      return ExtendedRuntimeTypes.GetCompilerRuntimeTypeNodeFor(nspace, name, 0, typeCode);
    }
    private static TypeNode/*!*/ GetCompilerRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, int numParams, ElementType typeCode) {
      if (TargetPlatform.GenericTypeNamesMangleChar != 0 && numParams > 0)
        name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
      TypeNode result = null;
      if (SystemCompilerRuntimeAssembly == null)
        Debug.Assert(false);
      else
        result = SystemCompilerRuntimeAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemCompilerRuntimeAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
  }
#endif
#if !FxCop
    public
#endif
 sealed class CoreSystemTypes
    {
        private CoreSystemTypes() { }
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
#if !FxCop
        public static Class/*!*/ Delegate;
        public static Class/*!*/ Exception;
        public static Class/*!*/ Attribute;
#endif
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

#if !MinimalReader
        //Classes need for System.TypeCode
        public static Class/*!*/ DBNull;
        public static Struct/*!*/ DateTime;
        public static Struct/*!*/ Decimal;
#endif

        //Special types
        public static Class/*!*/ IsVolatile;
        public static Struct/*!*/ Void;
        public static Struct/*!*/ ArgIterator;
        public static Struct/*!*/ RuntimeFieldHandle;
        public static Struct/*!*/ RuntimeMethodHandle;
        public static Struct/*!*/ RuntimeTypeHandle;
#if !MinimalReader
        public static Struct/*!*/ RuntimeArgumentHandle;
#endif
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
#if !MinimalReader
            Delegate = (Class)GetTypeNodeFor("System", "Delegate", ElementType.Class);
            Exception = (Class)GetTypeNodeFor("System", "Exception", ElementType.Class);
            Attribute = (Class)GetTypeNodeFor("System", "Attribute", ElementType.Class);
            DBNull = (Class)GetTypeNodeFor("System", "DBNull", ElementType.Class);
            DateTime = (Struct)GetTypeNodeFor("System", "DateTime", ElementType.ValueType);
            Decimal = (Struct)GetTypeNodeFor("System", "Decimal", ElementType.ValueType);
#endif
            ArgIterator = (Struct)GetTypeNodeFor("System", "ArgIterator", ElementType.ValueType);
            IsVolatile = (Class)GetTypeNodeFor("System.Runtime.CompilerServices", "IsVolatile", ElementType.Class);
            Void = (Struct)GetTypeNodeFor("System", "Void", ElementType.Void);
            RuntimeFieldHandle = (Struct)GetTypeNodeFor("System", "RuntimeFieldHandle", ElementType.ValueType);
            RuntimeMethodHandle = (Struct)GetTypeNodeFor("System", "RuntimeMethodHandle", ElementType.ValueType);
            RuntimeTypeHandle = (Struct)GetTypeNodeFor("System", "RuntimeTypeHandle", ElementType.ValueType);
#if !MinimalReader
            RuntimeArgumentHandle = (Struct)GetTypeNodeFor("System", "RuntimeArgumentHandle", ElementType.ValueType);
#endif
            SecurityAction = GetTypeNodeFor("System.Security.Permissions", "SecurityAction", ElementType.ValueType) as EnumNode;
            CoreSystemTypes.Initialized = true;
            CoreSystemTypes.InstantiateGenericInterfaces();
#if !NoWriter
            Literal.Initialize();
#endif
            object dummy = TargetPlatform.AssemblyReferenceFor; //Force selection of target platform
            if (dummy == null) return;
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
#if !MinimalReader
            Delegate = null;
            Exception = null;
            Attribute = null;
#endif
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
#if !MinimalReader
            DBNull = null;
            DateTime = null;
            Decimal = null;
            RuntimeArgumentHandle = null;
#endif
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
#if !MinimalReader
            InstantiateGenericInterfaces(DBNull);
            InstantiateGenericInterfaces(DateTime);
            InstantiateGenericInterfaces(Decimal);
#endif
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
            AssemblyNode result = SystemAssemblyLocation.ParsedAssembly;
            if (result != null)
            {
                result.TargetRuntimeVersion = TargetPlatform.TargetRuntimeVersion;
                result.MetadataFormatMajorVersion = 1;
                result.MetadataFormatMinorVersion = 1;
                result.LinkerMajorVersion = 8;
                result.LinkerMinorVersion = 0;
                return result;
            }
            if (SystemAssemblyLocation.Location == null || SystemAssemblyLocation.Location.Length == 0)
                SystemAssemblyLocation.Location = typeof(object).Module.Assembly.Location;
            result = (AssemblyNode)(new Reader(SystemAssemblyLocation.Location, null, doNotLockFile, getDebugInfo, true, false)).ReadModule();
            if (result == null && TargetPlatform.TargetVersion != null && TargetPlatform.TargetVersion == typeof(object).Module.Assembly.GetName().Version)
            {
                SystemAssemblyLocation.Location = typeof(object).Module.Assembly.Location;
                result = (AssemblyNode)(new Reader(SystemAssemblyLocation.Location, null, doNotLockFile, getDebugInfo, true, false)).ReadModule();
            }
            if (result == null)
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
#if !FxCop
    public
#endif
 sealed class SystemTypes
    {
        private SystemTypes() { }
        private static bool Initialized;

        //system assembly (the basic runtime)
        public static AssemblyNode/*!*/ SystemAssembly
        {
            get { return CoreSystemTypes.SystemAssembly; }
            set { CoreSystemTypes.SystemAssembly = value; }
        }
#if ExtendedRuntime
    public static AssemblyNode/*!*/ SystemCompilerRuntimeAssembly {
      get{return ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly;}
      set{ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly = value;}
    }
#if !NoData
    public static AssemblyNode/*!*/ SystemDataAssembly;
#endif
#if !NoXml && !NoRuntimeXml
    public static AssemblyNode /*!*/SystemXmlAssembly;
#endif
#endif
#if !FxCop
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
#endif

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

#if !MinimalReader
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
#if !WHIDBEYwithGenerics
        public static Class/*!*/ GenericArrayToIEnumerableAdapter;
#endif
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
#endif
#if ExtendedRuntime
    public static Interface/*!*/ ConstrainedType { get { return ExtendedRuntimeTypes.ConstrainedType; } }
    public static Interface/*!*/ TupleType { get { return ExtendedRuntimeTypes.TupleType; } }
    public static Interface/*!*/ TypeAlias { get { return ExtendedRuntimeTypes.TypeAlias; } }
    public static Interface/*!*/ TypeDefinition { get { return ExtendedRuntimeTypes.TypeDefinition; } }
    public static Interface/*!*/ TypeIntersection { get { return ExtendedRuntimeTypes.TypeIntersection; } }
    public static Interface/*!*/ TypeUnion { get { return ExtendedRuntimeTypes.TypeUnion; } }
    public static Class/*!*/ AnonymousAttribute;
    public static TypeNode/*!*/ AnonymityEnum;
    public static Class/*!*/ ComposerAttribute;
    public static Class/*!*/ CustomVisitorAttribute;
    public static Class/*!*/ TemplateAttribute;
    public static Class/*!*/ TemplateInstanceAttribute;
    public static Class/*!*/ UnmanagedStructTemplateParameterAttribute;
    public static Class/*!*/ TemplateParameterFlagsAttribute;
    public static Struct/*!*/ GenericBoxed;
    public static Class/*!*/ GenericIEnumerableToGenericIListAdapter;
    public static Struct/*!*/ GenericInvariant;
    public static Struct/*!*/ GenericNonEmptyIEnumerable;
    public static Struct/*!*/ GenericNonNull;
    public static Class/*!*/ GenericStreamUtility;
    public static Class/*!*/ GenericUnboxer;
    public static Interface/*!*/ ITemplateParameter { get { return ExtendedRuntimeTypes.ITemplateParameter; } }
    public static Class/*!*/ ElementTypeAttribute;
    public static Interface/*!*/ IDbTransactable;
    public static Interface/*!*/ IAggregate;
    public static Interface/*!*/ IAggregateGroup;
    public static Class/*!*/ StreamNotSingletonException;
    public static EnumNode SqlHint;
    public static Class/*!*/ SqlFunctions;
    public static Class/*!*/ XmlAttributeAttributeClass;
    public static Class/*!*/ XmlChoiceIdentifierAttributeClass;
    public static Class/*!*/ XmlElementAttributeClass;
    public static Class/*!*/ XmlIgnoreAttributeClass;
    public static Class/*!*/ XmlTypeAttributeClass;
    public static Interface/*!*/ INullable;
    public static Struct/*!*/ SqlBinary;
    public static Struct/*!*/ SqlBoolean;
    public static Struct/*!*/ SqlByte;
    public static Struct/*!*/ SqlDateTime;
    public static Struct/*!*/ SqlDecimal;
    public static Struct/*!*/ SqlDouble;
    public static Struct/*!*/ SqlGuid;
    public static Struct/*!*/ SqlInt16;
    public static Struct/*!*/ SqlInt32;
    public static Struct/*!*/ SqlInt64;
    public static Struct/*!*/ SqlMoney;
    public static Struct/*!*/ SqlSingle;
    public static Struct/*!*/ SqlString;
    public static Interface/*!*/ IDbConnection;
    public static Interface/*!*/ IDbTransaction;
    public static EnumNode IsolationLevel;

    //OrdinaryExceptions
    public static Class/*!*/ NoChoiceException;
    public static Class/*!*/ IllegalUpcastException;
    public static EnumNode/*!*/ CciMemberKind;
    public static Class/*!*/ CciMemberKindAttribute;
    //NonNull  
    public static Class/*!*/ Range;
    //Invariants
    public static DelegateNode/*!*/ InitGuardSetsDelegate;
    public static DelegateNode/*!*/ CheckInvariantDelegate;
    public static DelegateNode/*!*/ FrameGuardGetter;
    public static Class/*!*/ ObjectInvariantException;
    public static DelegateNode/*!*/ ThreadConditionDelegate;
    public static DelegateNode/*!*/ GuardThreadStart;
    public static Class/*!*/ Guard;
    public static Class/*!*/ ContractMarkers;
    //public static Interface IReduction;
    public static Class/*!*/ AssertHelpers;
    public static DelegateNode/*!*/ ThreadStart;
    //CheckedExceptions
    public static Interface/*!*/ ICheckedException;
    public static Class/*!*/ CheckedException;

    // Contracts
    public static Class/*!*/ UnreachableException;
    public static Class/*!*/ ContractException;
    public static Class/*!*/ NullTypeException;
    public static Class/*!*/ AssertException;
    public static Class/*!*/ AssumeException;
    public static Class/*!*/ InvalidContractException;
    public static Class/*!*/ RequiresException;
    public static Class/*!*/ EnsuresException;
    public static Class/*!*/ ModifiesException;
    public static Class/*!*/ ThrowsException;
    public static Class/*!*/ DoesException;
    public static Class/*!*/ InvariantException;
    public static Class/*!*/ ContractMarkerException;

    public static Class/*!*/ AdditiveAttribute;
    public static Class/*!*/ InsideAttribute;
    public static Class/*!*/ SpecPublicAttribute;
    public static Class/*!*/ SpecProtectedAttribute;
    public static Class/*!*/ SpecInternalAttribute;
    public static Class/*!*/ PureAttribute;
    public static Class/*!*/ OwnedAttribute;
    public static Class/*!*/ RepAttribute;
    public static Class/*!*/ PeerAttribute;
    public static Class/*!*/ CapturedAttribute;
    public static Class/*!*/ LockProtectedAttribute;
    public static Class/*!*/ RequiresLockProtectedAttribute;
    public static Class/*!*/ ImmutableAttribute;
    public static Class/*!*/ RequiresImmutableAttribute;
    public static Class/*!*/ RequiresCanWriteAttribute;
    public static Class/*!*/ StateIndependentAttribute;
    public static Class/*!*/ ConfinedAttribute;
    public static Class/*!*/ ModelfieldContractAttribute;
    public static Class/*!*/ ModelfieldAttribute;    
    public static Class/*!*/ SatisfiesAttribute;  //Stores a satisfies clause of a modelfield
    public static Class/*!*/ ModelfieldException;

    public static Class/*!*/ OnceAttribute;
    public static Class/*!*/ WriteConfinedAttribute;
    public static Class/*!*/ GlobalReadAttribute;
    public static Class/*!*/ GlobalAccessAttribute;
    public static Class/*!*/ GlobalWriteAttribute;
    public static Class/*!*/ FreshAttribute;
    public static Class/*!*/ EscapesAttribute;
    
    public static Class/*!*/ ModelAttribute;
    public static Class/*!*/ RequiresAttribute;
    public static Class/*!*/ EnsuresAttribute;
    public static Class/*!*/ ModifiesAttribute;
    public static Class/*!*/ HasWitnessAttribute;
    public static Class/*!*/ InferredReturnValueAttribute;
    public static Class/*!*/ ThrowsAttribute;
    public static Class/*!*/ DoesAttribute;
    public static Class/*!*/ InvariantAttribute;
    public static Class/*!*/ NoDefaultActivityAttribute;
    public static Class/*!*/ NoDefaultContractAttribute;
    public static Class/*!*/ ReaderAttribute;
    public static Class/*!*/ ShadowsAssemblyAttribute;
    public static Class/*!*/ VerifyAttribute;
    public static Class/*!*/ NonNullType { get { return ExtendedRuntimeTypes.NonNullType; } }
    public static Method NonNullTypeAssertInitialized { get { return ExtendedRuntimeTypes.NonNullTypeAssertInitialized; } }
    public static Method NonNullTypeAssertInitializedGeneric { get { return ExtendedRuntimeTypes.NonNullTypeAssertInitializedGeneric; } }

    public static Class/*!*/ NullableType { get { return ExtendedRuntimeTypes.NullableType; } }
    public static Class/*!*/ NotNullAttribute { get { return ExtendedRuntimeTypes.NotNullAttribute; } }
    public static Class/*!*/ NotNullGenericArgumentsAttribute { get { return ExtendedRuntimeTypes.NotNullGenericArgumentsAttribute; } }
    public static Class/*!*/ EncodedTypeSpecAttribute { get { return ExtendedRuntimeTypes.EncodedTypeSpecAttribute; } }
    public static Class/*!*/ DependentAttribute;
    public static Class/*!*/ ElementsRepAttribute;
    public static Class/*!*/ ElementsPeerAttribute;
    public static Class/*!*/ ElementAttribute;
    public static Class/*!*/ ElementCollectionAttribute;
    public static Class/*!*/ RecursionTerminationAttribute;
    public static Class/*!*/ NoReferenceComparisonAttribute;
    public static Class/*!*/ ResultNotNewlyAllocatedAttribute;

    // This attribute is recognized by the Bartok compiler and marks methods without heap allocation.
    // Thus the presence of this attribute implies [ResultNotNewlyAllocated] for pure methods.
    public static Class/*!*/ BartokNoHeapAllocationAttribute {
      //^ [NoDefaultContract]
      get
        //^ modifies noHeapAllocationAttribute;
      {
        if(noHeapAllocationAttribute == null) {
          noHeapAllocationAttribute = (Class)GetCompilerRuntimeTypeNodeFor(
            @"System.Runtime.CompilerServices", @"NoHeapAllocationAttribute", ElementType.Class);
        }
        return noHeapAllocationAttribute;
      }
    }
    private static Class noHeapAllocationAttribute;    
#endif

        static SystemTypes()
        {
            SystemTypes.Initialize(TargetPlatform.DoNotLockFiles, TargetPlatform.GetDebugInfo);
        }

#if FxCop
    internal static event EventHandler<EventArgs> ClearingSystemTypes;
    internal static void RaiseClearingSystemTypes()
    {
      EventHandler<EventArgs> handler = ClearingSystemTypes;
      if (handler != null) handler(null, EventArgs.Empty);
    }
#endif
        public static void Clear()
        {
            lock (Module.GlobalLock)
            {
                CoreSystemTypes.Clear();
#if FxCop
        RaiseClearingSystemTypes(); 
#endif
#if ExtendedRuntime
        ExtendedRuntimeTypes.Clear();
        if (SystemTypes.SystemCompilerRuntimeAssembly != null && SystemTypes.SystemCompilerRuntimeAssembly != AssemblyNode.Dummy) {
          SystemTypes.SystemCompilerRuntimeAssembly.Dispose();
          SystemTypes.SystemCompilerRuntimeAssembly = null;
        }
#if !NoData && !ROTOR
        if (SystemTypes.SystemDataAssembly != null && SystemTypes.SystemDataAssembly != AssemblyNode.Dummy) {
          SystemTypes.SystemDataAssembly.Dispose();
          SystemTypes.SystemDataAssembly = null;
        }
#endif
#if !NoXml && !NoRuntimeXml
        if (SystemTypes.SystemXmlAssembly != null && SystemTypes.SystemXmlAssembly != AssemblyNode.Dummy) {
          SystemTypes.SystemXmlAssembly.Dispose();
          SystemTypes.SystemXmlAssembly = null;
        }
#endif
#endif
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
#if ExtendedRuntime
        ExtendedRuntimeTypes.Initialize(doNotLockFile, getDebugInfo);
#endif
            }
            else if (!CoreSystemTypes.Initialized)
            {
                CoreSystemTypes.Initialize(doNotLockFile, getDebugInfo);
#if ExtendedRuntime
        ExtendedRuntimeTypes.Clear();
        ExtendedRuntimeTypes.Initialize(doNotLockFile, getDebugInfo);
#endif
            }

            if (TargetPlatform.TargetVersion == null)
            {
                TargetPlatform.TargetVersion = SystemAssembly.Version;
                if (TargetPlatform.TargetVersion == null)
                    TargetPlatform.TargetVersion = typeof(object).Module.Assembly.GetName().Version;
            }
            //TODO: throw an exception when the result is null
#if ExtendedRuntime
#if !NoData && !ROTOR
      SystemDataAssembly = SystemTypes.GetSystemDataAssembly(doNotLockFile, getDebugInfo);
#endif
#if !NoXml && !NoRuntimeXml
      SystemXmlAssembly = SystemTypes.GetSystemXmlAssembly(doNotLockFile, getDebugInfo);
#endif
#endif
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

#if !MinimalReader
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
            DictionaryEntry = (Struct)GetTypeNodeFor("System.Collections", "DictionaryEntry", ElementType.ValueType);
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
#endif
#if ExtendedRuntime
#if !NoXml && !NoRuntimeXml
      XmlAttributeAttributeClass = (Class)GetXmlTypeNodeFor("System.Xml.Serialization", "XmlAttributeAttribute", ElementType.Class);
      XmlChoiceIdentifierAttributeClass = (Class)GetXmlTypeNodeFor("System.Xml.Serialization", "XmlChoiceIdentifierAttribute", ElementType.Class);
      XmlElementAttributeClass = (Class)GetXmlTypeNodeFor("System.Xml.Serialization", "XmlElementAttribute", ElementType.Class);
      XmlIgnoreAttributeClass = (Class)GetXmlTypeNodeFor("System.Xml.Serialization", "XmlIgnoreAttribute", ElementType.Class);
      XmlTypeAttributeClass = (Class)GetXmlTypeNodeFor("System.Xml.Serialization", "XmlTypeAttribute", ElementType.Class);
#endif

#if !NoData
      INullable = (Interface) GetDataTypeNodeFor("System.Data.SqlTypes", "INullable", ElementType.Class);
      SqlBinary = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlBinary", ElementType.ValueType);
      SqlBoolean = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlBoolean", ElementType.ValueType);
      SqlByte = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlByte", ElementType.ValueType);
      SqlDateTime = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlDateTime", ElementType.ValueType);
      SqlDecimal = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlDecimal", ElementType.ValueType);
      SqlDouble = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlDouble", ElementType.ValueType);
      SqlGuid = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlGuid", ElementType.ValueType);
      SqlInt16 = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlInt16", ElementType.ValueType);
      SqlInt32 = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlInt32", ElementType.ValueType);
      SqlInt64 = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlInt64", ElementType.ValueType);
      SqlMoney = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlMoney", ElementType.ValueType);
      SqlSingle = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlSingle", ElementType.ValueType);
      SqlString = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlString", ElementType.ValueType);
      IDbConnection = (Interface)GetDataTypeNodeFor("System.Data", "IDbConnection", ElementType.Class);
      IDbTransaction = (Interface)GetDataTypeNodeFor("System.Data", "IDbTransaction", ElementType.Class);
      IsolationLevel = GetDataTypeNodeFor("System.Data", "IsolationLevel", ElementType.ValueType) as EnumNode;
#endif
#if CCINamespace
      const string CciNs = "Microsoft.Cci";
      const string ContractsNs = "Microsoft.Contracts";
      const string CompilerGuardsNs = "Microsoft.Contracts";
#else
      const string CciNs = "System.Compiler";
      const string ContractsNs = "Microsoft.Contracts";
      const string CompilerGuardsNs = "Microsoft.Contracts";
#endif
      const string GuardsNs = "Microsoft.Contracts";
      AnonymousAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "AnonymousAttribute", ElementType.Class);
      AnonymityEnum = GetCompilerRuntimeTypeNodeFor(CciNs, "Anonymity", ElementType.ValueType);
      ComposerAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "ComposerAttribute", ElementType.Class);
      CustomVisitorAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "CustomVisitorAttribute", ElementType.Class);
      TemplateAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "TemplateAttribute", ElementType.Class);
      TemplateInstanceAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "TemplateInstanceAttribute", ElementType.Class);
      UnmanagedStructTemplateParameterAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "UnmanagedStructTemplateParameterAttribute", ElementType.Class);
      TemplateParameterFlagsAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "TemplateParameterFlagsAttribute", ElementType.Class);
#if !WHIDBEYwithGenerics
      GenericArrayToIEnumerableAdapter = (Class)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ArrayToIEnumerableAdapter", 1, ElementType.Class);
#endif
      GenericBoxed = (Struct)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "Boxed", 1, ElementType.ValueType);
      GenericIEnumerableToGenericIListAdapter = (Class)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "GenericIEnumerableToGenericIListAdapter", 1, ElementType.Class);
      GenericInvariant = (Struct)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "Invariant", 1, ElementType.ValueType);
      GenericNonEmptyIEnumerable = (Struct)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "NonEmptyIEnumerable", 1, ElementType.ValueType);
      GenericNonNull = (Struct)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "NonNull", 1, ElementType.ValueType);
      GenericStreamUtility = (Class)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "StreamUtility", 1, ElementType.Class);
      GenericUnboxer = (Class)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "Unboxer", 1, ElementType.Class);
      ElementTypeAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "ElementTypeAttribute", ElementType.Class);
      IDbTransactable = (Interface)GetCompilerRuntimeTypeNodeFor("System.Data", "IDbTransactable", ElementType.Class);
      IAggregate = (Interface)GetCompilerRuntimeTypeNodeFor("System.Query", "IAggregate", ElementType.Class);
      IAggregateGroup = (Interface)GetCompilerRuntimeTypeNodeFor("System.Query", "IAggregateGroup", ElementType.Class);
      StreamNotSingletonException = (Class)GetCompilerRuntimeTypeNodeFor("System.Query", "StreamNotSingletonException", ElementType.Class);
      SqlHint = GetCompilerRuntimeTypeNodeFor("System.Query", "SqlHint", ElementType.ValueType) as EnumNode;
      SqlFunctions = (Class)GetCompilerRuntimeTypeNodeFor("System.Query", "SqlFunctions", ElementType.Class);

            #region Contracts
      Range = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "Range", ElementType.Class);
      //Ordinary Exceptions
      NoChoiceException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NoChoiceException", ElementType.Class);
      IllegalUpcastException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "IllegalUpcastException", ElementType.Class);
      CciMemberKind = (EnumNode)GetCompilerRuntimeTypeNodeFor(CciNs, "CciMemberKind", ElementType.ValueType);
      CciMemberKindAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "CciMemberKindAttribute", ElementType.Class);
      //Checked Exceptions
      ICheckedException = (Interface)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ICheckedException", ElementType.Class);
      CheckedException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "CheckedException", ElementType.Class);
      ContractMarkers = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ContractMarkers", ElementType.Class);
      //Invariant
      InitGuardSetsDelegate = (DelegateNode) GetCompilerRuntimeTypeNodeFor(GuardsNs, "InitGuardSetsDelegate", ElementType.Class);
      CheckInvariantDelegate = (DelegateNode) GetCompilerRuntimeTypeNodeFor(GuardsNs, "CheckInvariantDelegate", ElementType.Class);
      FrameGuardGetter = (DelegateNode) GetCompilerRuntimeTypeNodeFor(GuardsNs, "FrameGuardGetter", ElementType.Class);
      ObjectInvariantException = (Class)GetCompilerRuntimeTypeNodeFor("Microsoft.Contracts", "ObjectInvariantException", ElementType.Class);
      ThreadConditionDelegate = (DelegateNode) GetCompilerRuntimeTypeNodeFor(GuardsNs, "ThreadConditionDelegate", ElementType.Class);
      GuardThreadStart = (DelegateNode) GetCompilerRuntimeTypeNodeFor(GuardsNs, "GuardThreadStart", ElementType.Class);
      Guard = (Class) GetCompilerRuntimeTypeNodeFor(GuardsNs, "Guard", ElementType.Class);
      ThreadStart = (DelegateNode) GetTypeNodeFor("System.Threading", "ThreadStart", ElementType.Class);
      AssertHelpers = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "AssertHelpers", ElementType.Class);
            #region Exceptions
      UnreachableException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "UnreachableException", ElementType.Class);
      ContractException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ContractException", ElementType.Class);
      NullTypeException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NullTypeException", ElementType.Class);
      AssertException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "AssertException", ElementType.Class);
      AssumeException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "AssumeException", ElementType.Class);
      InvalidContractException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "InvalidContractException", ElementType.Class);
      RequiresException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "RequiresException", ElementType.Class);
      EnsuresException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "EnsuresException", ElementType.Class);
      ModifiesException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ModifiesException", ElementType.Class);
      ThrowsException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ThrowsException", ElementType.Class);
      DoesException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "DoesException", ElementType.Class);
      InvariantException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "InvariantException", ElementType.Class);
      ContractMarkerException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ContractMarkerException", ElementType.Class);
      #endregion
            #region Attributes
      AdditiveAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "AdditiveAttribute", ElementType.Class);
      InsideAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "InsideAttribute", ElementType.Class);
      PureAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "PureAttribute", ElementType.Class);
      ConfinedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "ConfinedAttribute", ElementType.Class);

            #region modelfield attributes and exceptions
      ModelfieldContractAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ModelfieldContractAttribute", ElementType.Class);
      ModelfieldAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ModelfieldAttribute", ElementType.Class);
      SatisfiesAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "SatisfiesAttribute", ElementType.Class);
      ModelfieldException = (Class)GetCompilerRuntimeTypeNodeFor("Microsoft.Contracts", "ModelfieldException", ElementType.Class);
      #endregion

      /* Diego's Attributes for Purity and WriteEffects */
        OnceAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "OnceAttribute", ElementType.Class);
        WriteConfinedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "WriteConfinedAttribute", ElementType.Class);
        GlobalReadAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "GlobalReadAttribute", ElementType.Class);
        GlobalWriteAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "GlobalWriteAttribute", ElementType.Class);
        GlobalAccessAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "GlobalAccessAttribute", ElementType.Class);
        FreshAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "FreshAttribute", ElementType.Class);
        EscapesAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "EscapesAttribute", ElementType.Class);
        /*  */

      StateIndependentAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "StateIndependentAttribute", ElementType.Class);
      SpecPublicAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "SpecPublicAttribute", ElementType.Class);
      SpecProtectedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "SpecProtectedAttribute", ElementType.Class);
      SpecInternalAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "SpecInternalAttribute", ElementType.Class);

      OwnedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "OwnedAttribute", ElementType.Class);
      RepAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "RepAttribute", ElementType.Class);
      PeerAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "PeerAttribute", ElementType.Class);
      CapturedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "CapturedAttribute", ElementType.Class);
      LockProtectedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "LockProtectedAttribute", ElementType.Class);
      RequiresLockProtectedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "RequiresLockProtectedAttribute", ElementType.Class);
      ImmutableAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "ImmutableAttribute", ElementType.Class);
      RequiresImmutableAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "RequiresImmutableAttribute", ElementType.Class);
      RequiresCanWriteAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "RequiresCanWriteAttribute", ElementType.Class);

      ModelAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ModelAttribute", ElementType.Class);
      RequiresAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "RequiresAttribute", ElementType.Class);
      EnsuresAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "EnsuresAttribute", ElementType.Class);
      ModifiesAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ModifiesAttribute", ElementType.Class);
      HasWitnessAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "HasWitnessAttribute", ElementType.Class);
      InferredReturnValueAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "InferredReturnValueAttribute", ElementType.Class);
      ThrowsAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ThrowsAttribute", ElementType.Class);
      DoesAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "DoesAttribute", ElementType.Class);
      InvariantAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "InvariantAttribute", ElementType.Class);
      NoDefaultActivityAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "NoDefaultActivityAttribute", ElementType.Class);
      NoDefaultContractAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "NoDefaultContractAttribute", ElementType.Class);
      ReaderAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "ReaderAttribute", ElementType.Class);

      ShadowsAssemblyAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ShadowsAssemblyAttribute", ElementType.Class);
      VerifyAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "VerifyAttribute", ElementType.Class);
      DependentAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "DependentAttribute", ElementType.Class);
      ElementsRepAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ElementsRepAttribute", ElementType.Class);
      ElementsPeerAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ElementsPeerAttribute", ElementType.Class);
      ElementAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ElementAttribute", ElementType.Class);
      ElementCollectionAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ElementCollectionAttribute", ElementType.Class);
      RecursionTerminationAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "RecursionTerminationAttribute", ElementType.Class);
      NoReferenceComparisonAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NoReferenceComparisonAttribute", ElementType.Class);
      ResultNotNewlyAllocatedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ResultNotNewlyAllocatedAttribute", ElementType.Class);
            #endregion
      #endregion
#endif
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
#if !MinimalReader
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
#if !WHIDBEYwithGenerics
            GenericArrayToIEnumerableAdapter = null;
#endif
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
#endif
#if ExtendedRuntime
      AnonymousAttribute = null;
      AnonymityEnum = null;
      ComposerAttribute = null;
      CustomVisitorAttribute = null;
      TemplateAttribute = null;
      TemplateInstanceAttribute = null;
      UnmanagedStructTemplateParameterAttribute = null;
      TemplateParameterFlagsAttribute = null;
      GenericBoxed = null;
      GenericIEnumerableToGenericIListAdapter = null;
      GenericInvariant = null;
      GenericNonEmptyIEnumerable = null;
      GenericNonNull = null;
      GenericStreamUtility = null;
      GenericUnboxer = null;
      ElementTypeAttribute = null;
      IDbTransactable = null;
      IAggregate = null;
      IAggregateGroup = null;
      StreamNotSingletonException = null;
      SqlHint = null;
      SqlFunctions = null;
      XmlAttributeAttributeClass = null;
      XmlChoiceIdentifierAttributeClass = null;
      XmlElementAttributeClass = null;
      XmlIgnoreAttributeClass = null;
      XmlTypeAttributeClass = null;
      INullable = null;
      SqlBinary = null;
      SqlBoolean = null;
      SqlByte = null;
      SqlDateTime = null;
      SqlDecimal = null;
      SqlDouble = null;
      SqlGuid = null;
      SqlInt16 = null;
      SqlInt32 = null;
      SqlInt64 = null;
      SqlMoney = null;
      SqlSingle = null;
      SqlString = null;
      IDbConnection = null;
      IDbTransaction = null;
      IsolationLevel = null;

      //OrdinaryExceptions
      NoChoiceException = null;
      IllegalUpcastException = null;
      //NonNull  
      Range = null;
      //Invariants
      InitGuardSetsDelegate = null;
      CheckInvariantDelegate = null;
      ObjectInvariantException = null;
      ThreadConditionDelegate = null;
      GuardThreadStart = null;
      Guard = null;
      ContractMarkers = null;
      //IReduction = null;
      AssertHelpers = null;
      ThreadStart = null;
      //CheckedExceptions
      ICheckedException = null;
      CheckedException = null;

      // Contracts
      UnreachableException = null;
      ContractException = null;
      NullTypeException = null;
      AssertException = null;
      AssumeException = null;
      InvalidContractException = null;
      RequiresException = null;
      EnsuresException = null;
      ModifiesException = null;
      ThrowsException = null;
      DoesException = null;
      InvariantException = null;
      ContractMarkerException = null;

      AdditiveAttribute = null;
      InsideAttribute = null;
      SpecPublicAttribute = null;
      SpecProtectedAttribute = null;
      SpecInternalAttribute = null;
      PureAttribute = null;
      OwnedAttribute = null;
      RepAttribute = null;
      PeerAttribute = null;
      CapturedAttribute = null;
      LockProtectedAttribute = null;
      RequiresLockProtectedAttribute = null;
      ImmutableAttribute = null;
      RequiresImmutableAttribute = null;
      RequiresCanWriteAttribute = null;
      StateIndependentAttribute = null;
      ConfinedAttribute = null;
      ModelfieldContractAttribute = null;
      ModelfieldAttribute = null;
      SatisfiesAttribute = null;
      ModelfieldException = null;

        /* Diego's Attributes for Purity Analysis and Write effects */
      OnceAttribute = null;
      WriteConfinedAttribute = null;
      GlobalReadAttribute = null;
      GlobalWriteAttribute = null;
      GlobalAccessAttribute = null;
      FreshAttribute = null;
      EscapesAttribute = null;
        /* */

      ModelAttribute = null;
      RequiresAttribute = null;
      EnsuresAttribute = null;
      ModifiesAttribute = null;
      HasWitnessAttribute = null;
      InferredReturnValueAttribute = null;
      ThrowsAttribute = null;
      DoesAttribute = null;
      InvariantAttribute = null;
      NoDefaultActivityAttribute = null;
      NoDefaultContractAttribute = null;
      ReaderAttribute = null;
      ShadowsAssemblyAttribute = null;
      VerifyAttribute = null;
      DependentAttribute = null;
      ElementsRepAttribute = null;
      ElementsPeerAttribute = null;
      ElementAttribute = null;
      ElementCollectionAttribute = null;
      RecursionTerminationAttribute = null;
      NoReferenceComparisonAttribute = null;
      ResultNotNewlyAllocatedAttribute = null;
      noHeapAllocationAttribute = null;
#endif
        }

#if !NoData && !ROTOR
        private static AssemblyNode/*!*/ GetSystemDataAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            System.Reflection.AssemblyName aName = typeof(System.Data.IDataReader).Module.Assembly.GetName();
            Identifier SystemDataId = Identifier.For(aName.Name);
            AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[SystemDataId.UniqueIdKey];
            if (aref == null)
            {
                aref = new AssemblyReference();
                aref.Name = aName.Name;
                aref.PublicKeyOrToken = aName.GetPublicKeyToken();
                aref.Version = TargetPlatform.TargetVersion;
                TargetPlatform.AssemblyReferenceFor[SystemDataId.UniqueIdKey] = aref;
            }
            if (SystemDataAssemblyLocation.Location == null || SystemDataAssemblyLocation.Location.Length == 0)
                SystemDataAssemblyLocation.Location = typeof(System.Data.IDataReader).Module.Assembly.Location;
            if (aref.assembly == null) aref.Location = SystemDataAssemblyLocation.Location;
            return aref.assembly = AssemblyNode.GetAssembly(aref);
        }
#endif
#if !NoXml && !NoRuntimeXml
        private static AssemblyNode/*!*/ GetSystemXmlAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            System.Reflection.AssemblyName aName = typeof(System.Xml.XmlNode).Module.Assembly.GetName();
            Identifier SystemXmlId = Identifier.For(aName.Name);
            AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[SystemXmlId.UniqueIdKey];
            if (aref == null)
            {
                aref = new AssemblyReference();
                aref.Name = aName.Name;
                aref.PublicKeyOrToken = aName.GetPublicKeyToken();
                aref.Version = TargetPlatform.TargetVersion;
                TargetPlatform.AssemblyReferenceFor[SystemXmlId.UniqueIdKey] = aref;
            }
            if (SystemXmlAssemblyLocation.Location == null || SystemXmlAssemblyLocation.Location.Length == 0)
                SystemXmlAssemblyLocation.Location = typeof(System.Xml.XmlNode).Module.Assembly.Location;
            if (aref.assembly == null) aref.Location = SystemXmlAssemblyLocation.Location;
            return aref.assembly = AssemblyNode.GetAssembly(aref);
        }
#endif
        private static TypeNode/*!*/ GetGenericRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, int numParams, ElementType typeCode)
        {
            if (TargetPlatform.GenericTypeNamesMangleChar != 0) name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
#if ExtendedRuntime
      if (TargetPlatform.TargetVersion != null && TargetPlatform.TargetVersion.Major == 1 && TargetPlatform.TargetVersion.Minor < 2)
        return SystemTypes.GetCompilerRuntimeTypeNodeFor(nspace, name, typeCode);
      else
#endif
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
#if ExtendedRuntime
    private static TypeNode/*!*/ GetCompilerRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      return SystemTypes.GetCompilerRuntimeTypeNodeFor(nspace, name, 0, typeCode);
    }
    private static TypeNode/*!*/ GetCompilerRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, int numParams, ElementType typeCode) {
      if (TargetPlatform.GenericTypeNamesMangleChar != 0 && numParams > 0)
        name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
      TypeNode result = null;
      if (SystemCompilerRuntimeAssembly == null)
        Debug.Assert(false);
      else
        result = SystemCompilerRuntimeAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemCompilerRuntimeAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
#if !NoData
    private static TypeNode/*!*/ GetDataTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemDataAssembly == null)
        Debug.Assert(false);
      else
        result = SystemDataAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemDataAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
#endif
#if !NoXml && !NoRuntimeXml
    private static TypeNode/*!*/ GetXmlTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemXmlAssembly == null)
        Debug.Assert(false);
      else
        result = SystemXmlAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemXmlAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
#endif
#endif
    }
}
