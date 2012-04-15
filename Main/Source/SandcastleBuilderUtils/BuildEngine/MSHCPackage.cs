using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This class implements an MSHC package (which is a type of ZIP file).
    /// </summary>
    public class MSHCPackage : IDisposable
    {
        #region Private data members
        //=====================================================================

        Package m_package = null;
        XmlDocument m_defaultContentTypes = null;
        Dictionary<String, String> m_globalProperties = new Dictionary<String, String>();
        CompressionOption m_compression = CompressionOption.Maximum;
        String m_loggingPrefix = String.Empty;

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Opens an MSHC package with the specified modes.
        /// </summary>
        /// <param name="packagePath">The package to open.</param>
        /// <param name="openMode">The open mode.</param>
        /// <param name="openAccess">The access mode.</param>
        /// <param name="openShare">The shareing mode.</param>
        public MSHCPackage(String packagePath, FileMode openMode, FileAccess openAccess, FileShare openShare)
        {
            this.PackagePath = packagePath;
            m_package = Package.Open(this.PackagePath, openMode, openAccess, openShare);
        }

        /// <summary>
        /// Opens an MSHC package with the specified modes and <value>FileShare.None</value> 
        /// </summary>
        /// <param name="packagePath">The package to open.</param>
        /// <param name="openMode">The open mode.</param>
        /// <param name="openAccess">The access mode.</param>
        public MSHCPackage(String packagePath, FileMode openMode, FileAccess openAccess)
          : this(packagePath, openMode, openAccess, (openAccess == FileAccess.Read) ? FileShare.Read : FileShare.None)
        { }

        /// <summary>
        /// Opens an MSHC package with the specified open mode, <value>FileAccess.Read</value>, and <value>FileShare.None</value> 
        /// </summary>
        /// <param name="packagePath">The package to open.</param>
        /// <param name="openMode">The open mode.</param>
        public MSHCPackage(String packagePath, FileMode openMode)
            : this(packagePath, openMode, (openMode == FileMode.Open) ? FileAccess.Read : FileAccess.ReadWrite)
        { }

        /// <summary>
        /// Opens an MSHC package with the specified modes.
        /// </summary>
        /// <param name="packageName">The file name of the package to open.</param>
        /// <param name="packagePath">The folder containing the package file.</param>
        /// <param name="openMode">The open mode.</param>
        /// <param name="openAccess">The access mode.</param>
        /// <param name="openShare">The shareing mode.</param>
        public MSHCPackage(String packageName, String packagePath, FileMode openMode, FileAccess openAccess,
            FileShare openShare) : this(Path.Combine(packagePath, packageName), openMode, openAccess, openShare)
        { }

        /// <summary>
        /// Opens an MSHC package with the specified modes and <value>FileShare.None</value> 
        /// </summary>
        /// <param name="packageName">The file name of the package to open.</param>
        /// <param name="packagePath">The folder containing the package file.</param>
        /// <param name="openMode">The open mode.</param>
        /// <param name="openAccess">The access mode.</param>
        public MSHCPackage(String packageName, String packagePath, FileMode openMode, FileAccess openAccess)
            : this(Path.Combine(packagePath, packageName), openMode, openAccess)
        { }

        /// <summary>
        /// Opens an MSHC package with the specified open mode, <value>FileAccess.Read</value>, and <value>FileShare.None</value> 
        /// </summary>
        /// <param name="packageName">The file name of the package to open.</param>
        /// <param name="packagePath">The folder containing the package file.</param>
        /// <param name="openMode">The open mode.</param>
        public MSHCPackage(String packageName, String packagePath, FileMode openMode)
            : this(Path.Combine(packagePath, packageName), openMode)
        { }

        /// <summary>
        /// Opens an MSHC package with the <value>FileMode.Open</value>, <value>FileAccess.Read</value>, and <value>FileShare.None</value> 
        /// </summary>
        /// <param name="packageName">The file name of the package to open.</param>
        /// <param name="packagePath">The folder containing the package file.</param>
        public MSHCPackage(String packageName, String packagePath)
            : this(Path.Combine(packagePath, packageName), FileMode.Open)
        { }

        /// <summary>
        /// Represents the uncompressed contents of an MSHC package.
        /// </summary>
        /// <param name="packagePath">The folder that contains the package contents.</param>
        public MSHCPackage(String packagePath)
        {
            this.PackagePath = Path.GetFullPath(packagePath.TrimEnd('\\', '/') + "\\");
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <inheritDoc/>
        ~MSHCPackage()
        {
            this.Dispose(false);
        }

        /// <inheritDoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritDoc/>
        protected virtual void Dispose(bool disposing)
        {
            if(m_package != null)
            {
                try
                {
                    m_package.Close();
                }
#if DEBUG
                catch(Exception exp)
                {
                    Debug.Print(exp.Message);
                }
#else
				catch { }
#endif
            }
        }

        #endregion

        #region Public Properties
        //=====================================================================

        /// <summary>
        /// <value>True</value> if the package is open.
        /// </summary>
        public bool IsOpen
        {
            get { return (m_package != null); }
        }

        /// <summary>
        /// The full path of the package file.
        /// </summary>
        public String PackagePath { get; protected set; }

        /// <summary>
        /// The package file name (without the <value>.mshc</value> extension);
        /// </summary>
        public String PackageName
        {
            get
            {
                if(!String.IsNullOrEmpty(PackagePath))
                {
                    return Path.GetFileNameWithoutExtension(PackagePath);
                }
                return String.Empty;
            }
        }

        /// <summary>
        /// The compression to use when storing files in the package.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="F:System.IO.Packaging.CompressionOption.Maximum"/>. If you want to use
        /// a different compression you must change this property before storing data in the package.
        /// <para>To mix-and-match compression options, store the files individually and set this parameter
        /// before each file is stored.</para>
        /// </remarks>
        CompressionOption Compression
        {
            get { return m_compression; }
            set { m_compression = value; }
        }

        /// <summary>
        /// A collection of property names and values that is applied when a package manifest is used.
        /// </summary>
        /// <remarks>
        /// These values are used to test conditions in the package manifest according to MSBuild condition syntax.
        /// </remarks>
        public Dictionary<String, String> ManifestProperties
        {
            get { return m_globalProperties; }
        }

        /// <summary>
        /// If this property is set, the provided <see cref="T:System.IO.TextWriter"/> will be used to
        /// log extract and store operations.
        /// </summary>
        public TextWriter LoggingTarget { get; set; }

        /// <summary>
        /// The prefix to add the the start of each line in the <see cref="P:SandcastleBuilder.Utils.BuildEngine.MSHCPackage.LoggingTarget"/> 
        /// </summary>
        /// <remarks>
        /// The default value is an empty string.
        /// </remarks>
        public String LoggingPrefix
        {
            get { return (m_loggingPrefix == null) ? String.Empty : m_loggingPrefix; }
            set { m_loggingPrefix = (value == null) ? String.Empty : value; }
        }

        #endregion

        #region Methods
        #region Contents Queries
        //=====================================================================

        /// <summary>
        /// Returns a list of parts (files) in the package.
        /// </summary>
        /// <returns>An array of part names (with a leading <value>'/'</value>).</returns>
        /// <remarks>
        /// The returned array is static. Changing the contents of the package will not update it.
        /// </remarks>
        public String[] GetPartNames()
        {
            List<String> v_partNames = new List<String>();

            if(IsOpen)
            {
                foreach(PackagePart v_part in m_package.GetParts())
                {
                    v_partNames.Add(PartName(v_part));
                }
            }
            return v_partNames.ToArray();
        }

        /// <summary>
        /// Checks if a specific part is in the package.
        /// </summary>
        /// <param name="partName">The name of the part to find (with or without the leading <value>'/'</value>).</param>
        /// <returns><value>True</value> if the part exists.</returns>
        public bool PartExists(String partName)
        {
            if(IsOpen)
            {
                if(m_package.PartExists(PartUri(partName)))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Copy Single Part
        //=====================================================================

        /// <summary>
        /// Extracts a copy of a part from the package.
        /// </summary>
        /// <param name="partName">The name of the part to extract (with or without the leading <value>'/'</value>).</param>
        /// <param name="targetPath">The full path of the target file.</param>
        /// <returns><value>True</value> if successful.</returns>
        /// <remarks>The method will fail if the target file already exists.</remarks>
        public bool GetPart(String partName, String targetPath)
        {
            return GetPart(partName, targetPath, false);
        }
        /// <summary>
        /// Extracts a copy of a part from the package.
        /// </summary>
        /// <param name="partName">The name of the part to extract (with or without the leading <value>'/'</value>).</param>
        /// <param name="targetPath">The full path of the target file.</param>
        /// <param name="replaceTarget"><value>True</value> to replace the target file if it exists.</param>
        /// <returns><value>True</value> if successful.</returns>
        public bool GetPart(String partName, String targetPath, bool replaceTarget)
        {
            bool v_ret = false;

            if(PartExists(partName))
            {
                try
                {
                    if(replaceTarget || !File.Exists(targetPath))
                    {
                        String v_targetFolder = Path.GetDirectoryName(targetPath);
                        System.IO.FileStream v_stream;

                        if(LoggingTarget != null)
                        {
                            LoggingTarget.WriteLine("{0}  {1}{2} -> {3}", LoggingPrefix, PackagePath, PartUri(partName), targetPath);
                        }
                        if(!Directory.Exists(v_targetFolder))
                        {
                            Directory.CreateDirectory(v_targetFolder);
                        }
                        v_stream = System.IO.File.OpenWrite(targetPath);

                        if(v_stream != null)
                        {
                            v_ret = GetPart(partName, v_stream);
                            v_stream.Close();
                        }
                    }
                    else if((LoggingTarget != null) && !replaceTarget && File.Exists(targetPath))
                    {
                        LoggingTarget.WriteLine("{0}  Target file {1} already exists", LoggingPrefix, targetPath);
                    }
                }
                catch(Exception exp)
                {
                    try
                    {
                        if(LoggingTarget != null)
                        {
                            LoggingTarget.WriteLine("{0}{1}", LoggingPrefix, exp.Message);
                        }
                    }
                    catch
                    {
                        // Intentionally ignored, it failed while logging the error.
                    }

                    throw;
                }
            }
            return v_ret;
        }
        /// <summary>
        /// Extracts a part from the package to an output stream.
        /// </summary>
        /// <param name="partName">The name of the part to extract (with or without the leading <value>'/'</value>).</param>
        /// <param name="targetStream">The output stream to fill.</param>
        /// <returns><value>True</value> if successful.</returns>
        public bool GetPart(String partName, System.IO.Stream targetStream)
        {
            bool v_ret = false;

            try
            {
                if(PartExists(partName))
                {
                    PackagePart v_part = m_package.GetPart(PartUri(partName));
                    System.IO.Stream v_partStream = null;

                    if(v_part != null)
                    {
                        v_partStream = v_part.GetStream(FileMode.Open, FileAccess.Read);
                    }
                    if(v_partStream != null)
                    {
                        CopyStream(v_partStream, targetStream);
                        v_partStream.Close();
                        v_ret = true;
                    }
                }
            }
            catch(Exception exp)
            {
                try
                {
                    if(LoggingTarget != null)
                    {
                        LoggingTarget.WriteLine("{0}{1}", LoggingPrefix, exp.Message);
                    }
                }
                catch
                {
                    // Intentionally ignored, it failed while logging the error.
                }

                throw;
            }
            return v_ret;
        }

        //=====================================================================

        /// <summary>
        /// Stores a file in the package.
        /// </summary>
        /// <param name="partName">The name of the package part (with or without the leading <value>'/'</value>).</param>
        /// <param name="sourcePath">The full path of the source file.</param>
        /// <returns><value>True</value> if successful.</returns>
        /// <remarks>The method will fail if the package part already exists.
        /// <para>The package part's mime type is derived from the source file extension.</para>
        /// </remarks>
        public bool PutPart(String partName, String sourcePath)
        {
            return PutPart(partName, sourcePath, false);
        }
        /// <summary>
        /// Stores a file in the package.
        /// </summary>
        /// <param name="partName">The name of the package part (with or without the leading <value>'/'</value>).</param>
        /// <param name="sourcePath">The full path of the source file.</param>
        /// <param name="replaceTarget"><value>True</value> to replace the package part if it already exists.</param>
        /// <returns><value>True</value> if successful.</returns>
        /// <remarks>The package part's mime type is derived from the source file extension.</remarks>
        public bool PutPart(String partName, String sourcePath, bool replaceTarget)
        {
            return PutPart(partName, sourcePath, replaceTarget, DefaultPartType(sourcePath));
        }
        /// <summary>
        /// Stores a file in the package.
        /// </summary>
        /// <param name="partName">The name of the package part (with or without the leading <value>'/'</value>).</param>
        /// <param name="sourcePath">The full path of the source file.</param>
        /// <param name="replaceTarget"><value>True</value> to replace the package part if it already exists.</param>
        /// <param name="targetType">The package part's mime type.</param>
        /// <returns><value>True</value> if successful.</returns>
        public bool PutPart(String partName, String sourcePath, bool replaceTarget, String targetType)
        {
            bool v_ret = false;

            try
            {
                if(IsOpen)
                {
                    if(replaceTarget || !PartExists(partName))
                    {
                        System.IO.FileStream v_stream = System.IO.File.OpenRead(sourcePath);

                        if(v_stream != null)
                        {
                            if(LoggingTarget != null)
                            {
                                LoggingTarget.WriteLine("{0}  {1} -> {2}{3}", LoggingPrefix, sourcePath, PackagePath, PartUri(partName));
                            }
                            v_ret = PutPart(partName, v_stream, replaceTarget, targetType);
                            v_stream.Close();
                        }
                        else if(LoggingTarget != null)
                        {
                            LoggingTarget.WriteLine("{0}  Source file {1} not found", LoggingPrefix, sourcePath);
                        }
                    }
                    else if((LoggingTarget != null) && !replaceTarget && PartExists(partName))
                    {
                        LoggingTarget.WriteLine("{0}  Target part {1} already exists", LoggingPrefix, PartUri(partName));
                    }
                }
            }
            catch(Exception exp)
            {
                try
                {
                    if(LoggingTarget != null)
                    {
                        LoggingTarget.WriteLine("{0}{1}", LoggingPrefix, exp.Message);
                    }
                }
                catch
                {
                    // Intentionally ignored, it failed while logging the error.
                }

                throw;
            }
            return v_ret;
        }
        /// <summary>
        /// Stores an input stream in the package.
        /// </summary>
        /// <param name="partName">The name of the package part (with or without the leading <value>'/'</value>).</param>
        /// <param name="sourceStream">The input stream to store.</param>
        /// <returns><value>True</value> if successful.</returns>
        /// <remarks>The method will fail if the package part already exists.
        /// <para>The package part's mime type will be empty.</para></remarks>
        public bool PutPart(String partName, System.IO.Stream sourceStream)
        {
            return PutPart(partName, sourceStream, false);
        }
        /// <summary>
        /// Stores an input stream in the package.
        /// </summary>
        /// <param name="partName">The name of the package part (with or without the leading <value>'/'</value>).</param>
        /// <param name="sourceStream">The input stream to store.</param>
        /// <param name="replaceTarget"><value>True</value> to replace the package part if it already exists.</param>
        /// <returns><value>True</value> if successful.</returns>
        /// <remarks>The package part's mime type will be empty.</remarks>
        public bool PutPart(String partName, System.IO.Stream sourceStream, bool replaceTarget)
        {
            return PutPart(partName, sourceStream, replaceTarget, String.Empty);
        }
        /// <summary>
        /// Stores an input stream in the package.
        /// </summary>
        /// <param name="partName">The name of the package part (with or without the leading <value>'/'</value>).</param>
        /// <param name="sourceStream">The input stream to store.</param>
        /// <param name="replaceTarget"><value>True</value> to replace the package part if it already exists.</param>
        /// <param name="targetType">The package part's mime type.</param>
        /// <returns><value>True</value> if successful.</returns>
        public bool PutPart(String partName, System.IO.Stream sourceStream, bool replaceTarget, String targetType)
        {
            bool v_ret = false;

            try
            {
                if(IsOpen)
                {
                    PackagePart v_part;
                    System.IO.Stream v_partStream = null;

                    if(PartExists(partName))
                    {
                        if(replaceTarget)
                        {
                            m_package.DeletePart(PartUri(partName));
                        }
                        else
                        {
                            return false;
                        }
                    }

                    v_part = m_package.CreatePart(PartUri(partName), targetType, this.Compression);
                    if(v_part != null)
                    {
                        v_partStream = v_part.GetStream(FileMode.Create, FileAccess.ReadWrite);
                    }
                    if(v_partStream != null)
                    {
                        CopyStream(sourceStream, v_partStream);
                        v_partStream.Close();
                        v_ret = true;
                    }
                }
            }
            catch(Exception exp)
            {
                try
                {
                    if(LoggingTarget != null)
                    {
                        LoggingTarget.WriteLine("{0}{1}", LoggingPrefix, exp.Message);
                    }
                }
                catch
                {
                    // Intentionally ignored, it failed while logging the error.
                }

                throw;
            }
            return v_ret;
        }

        #endregion

        #region Copy All Parts
        //=====================================================================

        /// <summary>
        /// Extracts all parts from the package.
        /// </summary>
        /// <param name="targetFolder">The full path of the target folder.</param>
        /// <returns>The number of parts extracted.</returns>
        /// <remarks>If a part already has a file in the target folder it will not be extracted.</remarks>
        public int GetAllParts(String targetFolder)
        {
            return GetAllParts(targetFolder, false);
        }
        /// <summary>
        /// Extracts all parts from the package.
        /// </summary>
        /// <param name="targetFolder">The full path of the target folder.</param>
        /// <param name="replaceTargets"><value>True to replace existing files in the target folder.</value></param>
        /// <returns>The number of parts extracted.</returns>
        public int GetAllParts(String targetFolder, bool replaceTargets)
        {
            int v_partCount = 0;

            try
            {
                if(IsOpen)
                {
                    Uri v_targetFolder = new Uri(targetFolder);
                    Uri v_targetUri;
                    String v_targetPath;

                    if(!v_targetFolder.ToString().EndsWith("/", StringComparison.Ordinal))
                    {
                        v_targetFolder = new Uri(v_targetFolder.ToString() + "/");
                    }
                    if(LoggingTarget != null)
                    {
                        LoggingTarget.WriteLine("{0}{1} -> {2}", LoggingPrefix, PackagePath, targetFolder);
                    }

                    foreach(PackagePart v_part in m_package.GetParts())
                    {
                        v_targetUri = new Uri(v_targetFolder, PartName(v_part));
                        v_targetPath = Path.GetDirectoryName(v_targetUri.AbsolutePath);

                        if(!Directory.Exists(v_targetPath))
                        {
                            Directory.CreateDirectory(v_targetPath);
                        }
                        if(GetPart(PartName(v_part), v_targetUri.LocalPath, replaceTargets))
                        {
                            v_partCount++;
                        }
                    }
                }
            }
            catch(Exception exp)
            {
                try
                {
                    if(LoggingTarget != null)
                    {
                        LoggingTarget.WriteLine("{0}{1}", LoggingPrefix, exp.Message);
                    }
                }
                catch
                {
                    // Intentionally ignored, it failed while logging the error.
                }

                throw;
            }
            return v_partCount;
        }

        /// <summary>
        /// Stores the contents of a folder in the package.
        /// </summary>
        /// <param name="sourceFolder">The full path of the folder to store.</param>
        /// <returns>The number of files stored in the package.</returns>
        /// <remarks>All files within the folder are stored, and all subfolders are stored recursively.
        /// <para>If a part already exists in the package, it will not be replaced.</para>
        /// <para>The package parts' mime types are derived from the source file extensions.</para>
        /// </remarks>
        public int PutAllParts(String sourceFolder)
        {
            return PutAllParts(sourceFolder, false);
        }
        /// <summary>
        /// Stores the contents of a folder in the package.
        /// </summary>
        /// <param name="sourceFolder">The full path of the folder to store.</param>
        /// <param name="replaceTargets"><value>True to replace existing package parts.</value></param>
        /// <returns>The number of files stored in the package.</returns>
        /// <remarks>All files within the folder are stored, and all subfolders are stored recursively.
        /// <para>The package parts' mime types are derived from the source file extensions.</para>
        /// </remarks>
        public int PutAllParts(String sourceFolder, bool replaceTargets)
        {
            return PutAllParts(sourceFolder, true, replaceTargets);
        }
        /// <summary>
        /// Stores the contents of a folder in the package.
        /// </summary>
        /// <param name="sourceFolder">The full path of the folder to store.</param>
        /// <param name="recursive"><value>True</value> to store all subfolders recursively. <value>False</value> to store only the files within the source folder.</param>
        /// <param name="replaceTargets"><value>True to replace existing package parts.</value></param>
        /// <returns>The number of files stored in the package.</returns>
        /// <remarks>The package parts' mime types are derived from the source file extensions.</remarks>
        public int PutAllParts(String sourceFolder, bool recursive, bool replaceTargets)
        {
            return PutAllParts(sourceFolder, recursive, replaceTargets, null);
        }
        /// <summary>
        /// Stores the contents of a folder in the package.
        /// </summary>
        /// <param name="sourceFolder">The full path of the folder to store.</param>
        /// <param name="recursive"><value>True</value> to store all subfolders recursively. <value>False</value> to store only the files within the source folder.</param>
        /// <param name="replaceTargets"><value>True to replace existing package parts.</value></param>
        /// <param name="targetPath">The root location in the package where the files are to be stored (the default is the package root).</param>
        /// <returns>The number of files stored in the package.</returns>
        /// <remarks>The package parts' mime types are derived from the source file extensions.</remarks>
        private int PutAllParts(String sourceFolder, bool recursive, bool replaceTargets, String targetPath)
        {
            int v_partCount = 0;

            try
            {
                if(LoggingTarget != null)
                {
                    LoggingTarget.WriteLine("{0}{1} -> {2}", LoggingPrefix, sourceFolder, PackagePath);
                }

                DirectoryInfo v_sourceFolder = new DirectoryInfo(sourceFolder);

                foreach(FileInfo v_file in v_sourceFolder.GetFiles())
                {
                    Uri v_partUri = PartUri(String.IsNullOrEmpty(targetPath) ? v_file.Name : Path.Combine(targetPath, v_file.Name));

                    if(PutPart(PartName(v_partUri), v_file.FullName, replaceTargets))
                    {
                        v_partCount++;
                    }
                }

                if(recursive)
                {
                    foreach(DirectoryInfo v_folder in v_sourceFolder.GetDirectories())
                    {
                        String v_targetPath = v_folder.Name;
                        String v_loggingPrefix = LoggingPrefix;

                        if(!String.IsNullOrEmpty(targetPath))
                        {
                            v_targetPath = Path.Combine(targetPath, v_targetPath);
                        }
                        try
                        {
                            LoggingPrefix += "  ";
                            v_partCount += PutAllParts(v_folder.FullName, recursive, replaceTargets, v_targetPath);
                        }
                        finally
                        {
                            LoggingPrefix = v_loggingPrefix;
                        }
                    }
                }
            }
            catch(Exception exp)
            {
                try
                {
                    if(LoggingTarget != null)
                    {
                        LoggingTarget.WriteLine("{0}{1}", LoggingPrefix, exp.Message);
                    }
                }
                catch
                {
                    // Intentionally ignored, it failed while logging the error.
                }

                throw;
            }
            return v_partCount;
        }

        #endregion

        #region Copy Parts by manifest
        //=====================================================================

        /// <summary>
        /// Extracts the contents of a package according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="targetFolder">The full path of the target folder.</param>
        /// <param name="manifestPath">The full path of the manifest file.</param>
        /// <returns>The number of parts extracted.</returns>
        /// <remarks>If a part already has a file in the target folder it will not be extracted.</remarks>
        public int GetTheseParts(String targetFolder, String manifestPath)
        {
            return GetTheseParts(targetFolder, new PackageManifest(manifestPath));
        }
        /// <summary>
        /// Extracts the contents of a package according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="targetFolder">The full path of the target folder.</param>
        /// <param name="manifestPath">The full path of the manifest file.</param>
        /// <param name="replaceTargets"><value>True to replace existing files in the target folder.</value></param>
        /// <returns>The number of parts extracted.</returns>
        public int GetTheseParts(String targetFolder, String manifestPath, bool replaceTargets)
        {
            return GetTheseParts(targetFolder, new PackageManifest(manifestPath), replaceTargets);
        }
        /// <summary>
        /// Extracts the contents of a package according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="targetFolder">The full path of the target folder.</param>
        /// <param name="manifest">The manifest that specifies the parts to extract.</param>
        /// <returns>The number of parts extracted.</returns>
        /// <remarks>If a part already has a file in the target folder it will not be extracted.</remarks>
        protected int GetTheseParts(String targetFolder, PackageManifest manifest)
        {
            return GetTheseParts(targetFolder, manifest, false);
        }
        /// <summary>
        /// Extracts the contents of a package according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="targetFolder">The full path of the target folder.</param>
        /// <param name="manifest">The manifest that specifies the parts to extract.</param>
        /// <param name="replaceTargets"><value>True to replace existing files in the target folder.</value></param>
        /// <returns>The number of parts extracted.</returns>
        protected int GetTheseParts(String targetFolder, PackageManifest manifest, bool replaceTargets)
        {
            int v_partCount = 0;

            try
            {
                if(IsOpen && (manifest != null))
                {
                    manifest.Evaluate(ManifestProperties, GetPartNames());

                    foreach(PackageManifestItem v_manifestItem in manifest.Items)
                    {
                        if(v_manifestItem.CopyToOutputDirectory)
                        {
                            foreach(String v_partName in v_manifestItem)
                            {
                                String v_targetPath = Path.Combine(targetFolder, v_partName);

                                if(GetPart(v_partName, v_targetPath, replaceTargets))
                                {
                                    v_partCount++;
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception exp)
            {
                try
                {
                    if(LoggingTarget != null)
                    {
                        LoggingTarget.WriteLine("{0}{1}", LoggingPrefix, exp.Message);
                    }
                }
                catch
                {
                    // Intentionally ignored, it failed while logging the error.
                }

                throw;
            }
            return v_partCount;
        }

        //=====================================================================

        /// <summary>
        /// Stores files in the package according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="manifestPath">The full path of the manifest file.</param>
        /// <returns>The number of files stored in the package.</returns>
        /// <remarks>If a part already exists in the package, it will not be replaced.
        /// <para>The package parts' mime types are derived from the source file extensions.</para>
        /// </remarks>
        public int PutTheseParts(String manifestPath)
        {
            return PutTheseParts(new PackageManifest(manifestPath));
        }
        /// <summary>
        /// Stores files in the package according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="manifestPath">The full path of the manifest file.</param>
        /// <param name="replaceTargets"><value>True to replace existing package parts.</value></param>
        /// <returns>The number of files stored in the package.</returns>
        /// <remarks>The package parts' mime types are derived from the source file extensions.</remarks>
        public int PutTheseParts(String manifestPath, bool replaceTargets)
        {
            return PutTheseParts(new PackageManifest(manifestPath), replaceTargets);
        }
        /// <summary>
        /// Stores files in the package according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="manifest">The manifest that specifies the parts to store.</param>
        /// <returns>The number of files stored in the package.</returns>
        /// <remarks>If a part already exists in the package, it will not be replaced.
        /// <para>The package parts' mime types are derived from the source file extensions.</para>
        /// </remarks>
        protected int PutTheseParts(PackageManifest manifest)
        {
            return PutTheseParts(manifest, false);
        }
        /// <summary>
        /// Stores files in the package according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="manifest">The manifest that specifies the parts to store.</param>
        /// <param name="replaceTargets"><value>True to replace existing package parts.</value></param>
        /// <returns>The number of files stored in the package.</returns>
        /// <remarks>The package parts' mime types are derived from the source file extensions.</remarks>
        protected int PutTheseParts(PackageManifest manifest, bool replaceTargets)
        {
            int v_partCount = 0;

            try
            {
                if(IsOpen && (manifest != null))
                {
                    manifest.Evaluate(ManifestProperties);

                    foreach(PackageManifestItem v_manifestItem in manifest.Items)
                    {
                        if(v_manifestItem.CopyToOutputDirectory)
                        {
                            foreach(String v_partName in v_manifestItem)
                            {
                                String v_sourcePath = Path.Combine(manifest.SourceDir, v_partName);

                                if(PutPart(v_partName, v_sourcePath, replaceTargets, v_manifestItem.SubType))
                                {
                                    v_partCount++;
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception exp)
            {
                try
                {
                    if(LoggingTarget != null)
                    {
                        LoggingTarget.WriteLine("{0}{1}", LoggingPrefix, exp.Message);
                    }
                }
                catch
                {
                    // Intentionally ignored, it failed while logging the error.
                }

                throw;
            }
            return v_partCount;
        }

        //=====================================================================

        /// <summary>
        /// Copies files according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="manifestPath">The full path of the manifest file.</param>
        /// <returns>The number of files copied.</returns>
        /// <remarks>If a file already exists in the target folder it will not be copied.</remarks>
        public int CopyTheseParts(String manifestPath)
        {
            return CopyTheseParts(new PackageManifest(manifestPath));
        }
        /// <summary>
        /// Copies files according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="manifestPath">The full path of the manifest file.</param>
        /// <param name="replaceTargets"><value>True to replace existing files in the target folder.</value></param>
        /// <returns>The number of files copied.</returns>
        public int CopyTheseParts(String manifestPath, bool replaceTargets)
        {
            return CopyTheseParts(new PackageManifest(manifestPath), replaceTargets);
        }
        /// <summary>
        /// Copies files according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="manifest">The manifest that specifies the files to copy.</param>
        /// <returns>The number of files copied.</returns>
        /// <remarks>If a file already exists in the target folder it will not be copied.</remarks>
        protected int CopyTheseParts(PackageManifest manifest)
        {
            return CopyTheseParts(manifest, false);
        }
        /// <summary>
        /// Copies files according to a manifest composed of MSBuild <value>ItemGroup</value> and <value>Item</value> elements.
        /// </summary>
        /// <param name="manifest">The manifest that specifies the files to copy.</param>
        /// <param name="replaceTargets"><value>True to replace existing files in the target folder.</value></param>
        /// <returns>The number of files copied.</returns>
        protected int CopyTheseParts(PackageManifest manifest, bool replaceTargets)
        {
            int v_partCount = 0;

            try
            {
                if(manifest != null)
                {
                    String v_targetFolder = Path.GetFullPath(PackagePath.Replace('/', '\\').TrimEnd('\\'));

                    manifest.Evaluate(ManifestProperties);

                    foreach(PackageManifestItem v_manifestItem in manifest.Items)
                    {
                        if(v_manifestItem.CopyToOutputDirectory)
                        {
                            foreach(String v_partName in v_manifestItem)
                            {
                                String v_sourcePath = Path.Combine(manifest.SourceDir, v_partName);
                                String v_targetPath = Path.Combine(v_targetFolder, v_partName);

                                if(replaceTargets || !File.Exists(v_targetPath))
                                {
                                    if(!Directory.Exists(Path.GetDirectoryName(v_targetPath)))
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(v_targetPath));
                                    }
                                    if(LoggingTarget != null)
                                    {
                                        LoggingTarget.WriteLine("{0}  {1} -> {2}", LoggingPrefix, v_sourcePath, v_targetPath);
                                    }
                                    File.Copy(v_sourcePath, v_targetPath, replaceTargets);
                                    File.SetAttributes(v_targetPath, FileAttributes.Normal);
                                    v_partCount++;
                                }
                                else if(LoggingTarget != null)
                                {
                                    LoggingTarget.WriteLine("{0}  File {1} already exists", LoggingPrefix, v_targetPath);
                                }
                            }
                        }
                    }
                }
            }
#if DEBUG
            catch(Exception exp)
            {
                Debug.Print(exp.Message);
            }
#else
			catch { }
#endif
            return v_partCount;
        }

        #endregion
        #endregion

        #region Helper Methods
        //=====================================================================

        static private Uri PartUri(String partName)
        {
            String v_partName = partName.Replace('\\', '/');
            if(!v_partName.StartsWith("/", StringComparison.Ordinal))
            {
                v_partName = "/" + v_partName;
            }
            return new Uri(Uri.EscapeUriString(v_partName), UriKind.Relative);
        }
        static private String PartName(PackagePart part)
        {
            if(part != null)
            {
                return PartName(part.Uri);
            }
            return String.Empty;
        }
        static private String PartName(Uri partUri)
        {
            if(partUri != null)
            {
                return Uri.UnescapeDataString(partUri.ToString()).TrimStart('\\', '/');
            }
            return String.Empty;
        }

        /// <summary>
        /// Uses a the file extension of a part name to derive the default mime type.
        /// </summary>
        /// <param name="partName">The part name, with file extension.</param>
        /// <returns>The default mime type, or an empty string if none could be determined.</returns>
        public String DefaultPartType(String partName)
        {
            String v_partType = String.Empty;

            try
            {
                String v_nameExt = Path.GetExtension(partName).ToLower();
                XmlNamespaceManager v_namespaceManager;
                XmlNode v_typeNode;
                XmlNode v_typeAttribute;

                if(m_defaultContentTypes == null)
                {
                    m_defaultContentTypes = new XmlDocument();
                    m_defaultContentTypes.LoadXml(
                        "<?xml version='1.0' encoding='utf-8'?>" +
                        "<Types xmlns='http://schemas.openxmlformats.org/package/2006/content-types'>" +
                            "<Default Extension='css' ContentType='text/richtext' />" +
                            "<Default Extension='js' ContentType='text/richtext' />" +
                            "<Default Extension='gif' ContentType='image/gif' />" +
                            "<Default Extension='ico' ContentType='image/gif' />" +
                            "<Default Extension='png' ContentType='image/png' />" +
                            "<Default Extension='jpg' ContentType='image/jpeg' />" +
                            "<Default Extension='jpeg' ContentType='image/jpeg' />" +
                            "<Default Extension='xml' ContentType='text/xml' />" +
                            "<Default Extension='xslt' ContentType='text/richtext' />" +
                            "<Default Extension='htm' ContentType='text/html' />" +
                        "</Types>");
                }
                v_namespaceManager = new XmlNamespaceManager(m_defaultContentTypes.NameTable);
                v_namespaceManager.AddNamespace("openxml", "http://schemas.openxmlformats.org/package/2006/content-types");

                v_typeNode = m_defaultContentTypes.DocumentElement.SelectSingleNode(String.Format("openxml:Default[@Extension=substring-after('{0}','.')]", v_nameExt), v_namespaceManager);
                if(v_typeNode != null)
                {
                    v_typeAttribute = v_typeNode.Attributes.GetNamedItem("ContentType");
                    if(v_typeAttribute != null)
                    {
                        v_partType = v_typeAttribute.Value;
                    }
                }
            }
#if DEBUG
            catch(Exception exp)
            {
                Debug.Print(exp.Message);
            }
#else
			catch { }
#endif
            return v_partType;
        }

        private static void CopyStream(System.IO.Stream sourceStream, System.IO.Stream targetStream)
        {
            Byte[] v_buffer = new Byte[sourceStream.Length];

            sourceStream.Seek(0, SeekOrigin.Begin);
            sourceStream.Read(v_buffer, 0, (int)sourceStream.Length);
            targetStream.SetLength(0);
            targetStream.Write(v_buffer, 0, (int)sourceStream.Length);
        }

        #endregion

        #region Manifest Classes
        //=====================================================================

        /// <summary>
        /// This class represents a package manifest item
        /// </summary>
        protected class PackageManifestItem : List<String>
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="node">The XML node for the manifest item</param>
            public PackageManifestItem(XmlNode node)
            {
                this.Node = node;
            }

            /// <summary>
            /// This is used to get or set the XML node for the manifest item
            /// </summary>
            public XmlNode Node { get; protected set; }

            /// <summary>
            /// This read-only property is used to get the include item value
            /// </summary>
            public String Include
            {
                get
                {
                    XmlNode v_node = this.Node.Attributes.GetNamedItem("Include");
                    if(v_node != null)
                    {
                        return v_node.Value;
                    }
                    return String.Empty;
                }
            }

            /// <summary>
            /// This read-only property is used to get the sub-type value
            /// </summary>
            public String SubType
            {
                get
                {
                    XmlNode v_node = this.Node.SelectSingleNode("SubType");
                    if(v_node != null)
                    {
                        return v_node.InnerText;
                    }
                    return String.Empty;
                }
            }

            /// <summary>
            /// This read-only property is used to get the copy to output directory value
            /// </summary>
            public bool CopyToOutputDirectory
            {
                get
                {
                    XmlNode v_node = this.Node.SelectSingleNode("CopyToOutputDirectory");
                    if(v_node != null)
                    {
                        return Boolean.Parse(v_node.InnerText);
                    }
                    return true;
                }
            }
        }

        //=====================================================================

        /// <summary>
        /// This class represents a package manifest
        /// </summary>
        protected class PackageManifest : XmlDocument
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="filePath">The path to the manifest file</param>
            public PackageManifest(String filePath)
            {
                SourceDir = Path.GetDirectoryName(filePath);
                Items = new List<PackageManifestItem>();
                base.Load(filePath);
            }

            /// <summary>
            /// This is used to get or set the source directory
            /// </summary>
            public String SourceDir { get; set; }

            /// <summary>
            /// This is used to get or set the manifest items
            /// </summary>
            public List<PackageManifestItem> Items { get; protected set; }

            /// <summary>
            /// This is used to evaluate the properties
            /// </summary>
            /// <param name="properties">The property dictionary</param>
            public void Evaluate(Dictionary<String, String> properties)
            {
                DirectoryInfo v_DirectoryInfo = new DirectoryInfo(SourceDir);

                Items.Clear();

                foreach(XmlNode v_itemGroup in DocumentElement.SelectNodes("ItemGroup"))
                {
                    if(EvaluateCondition(v_itemGroup, properties))
                    {
                        foreach(XmlNode v_item in v_itemGroup.SelectNodes("Content"))
                        {
                            PackageManifestItem v_manifestItem = new PackageManifestItem(v_item);

                            foreach(FileInfo v_fileInfo in v_DirectoryInfo.GetFiles(v_manifestItem.Include))
                            {
                                v_manifestItem.Add(v_fileInfo.Name);
                            }
                            if(v_manifestItem.Count > 0)
                            {
                                Items.Add(v_manifestItem);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// This is used to evaluate the properties
            /// </summary>
            /// <param name="properties">The property dictionary</param>
            /// <param name="packageParts">The package parts</param>
            public void Evaluate(Dictionary<String, String> properties, String[] packageParts)
            {
                Items.Clear();

                foreach(XmlNode v_itemGroup in DocumentElement.SelectNodes("ItemGroup"))
                {
                    if(EvaluateCondition(v_itemGroup, properties))
                    {
                        foreach(XmlNode v_item in v_itemGroup.SelectNodes("Content"))
                        {
                            PackageManifestItem v_manifestItem = new PackageManifestItem(v_item);

                            foreach(String v_partName in packageParts)
                            {
                                if(Regex.IsMatch(v_partName.ToLower(), v_manifestItem.Include.Replace("*", ".*").Replace("?", ".").ToLower()))
                                {
                                    v_manifestItem.Add(v_partName);
                                }
                            }
                            if(v_manifestItem.Count > 0)
                            {
                                Items.Add(v_manifestItem);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// This is used to get or set the navigator
            /// </summary>
            private XPathNavigator Navigator { get; set; }

            /// <summary>
            /// This is used to evaluate a condition
            /// </summary>
            /// <param name="node">The node to evaluate</param>
            /// <param name="properties">The properties to evaluate</param>
            /// <returns>True if the condition is met, false if not</returns>
            private bool EvaluateCondition(XmlNode node, Dictionary<String, String> properties)
            {
                if(Navigator == null)
                {
                    Navigator = base.CreateNavigator();
                }
                if(node.Attributes != null)
                {
                    XmlNode v_condition = node.Attributes.GetNamedItem("Condition");
                    if(v_condition != null)
                    {
                        try
                        {
                            String v_conditionValue = v_condition.Value;
                            foreach(KeyValuePair<String, String> v_property in properties)
                            {
                                v_conditionValue = v_conditionValue.Replace("$(" + v_property.Key + ")", v_property.Value);
                            }
                            while(v_conditionValue.Contains("$("))
                            {
                                String v_property = v_conditionValue.Substring(v_conditionValue.IndexOf("$(", StringComparison.Ordinal));
                                if(v_property.Contains(")"))
                                {
                                    v_property = v_property.Substring(0, v_property.IndexOf(")", StringComparison.Ordinal) + 1);
                                    v_conditionValue = v_conditionValue.Replace(v_property, "false");
                                }
                                else
                                {
                                    break;
                                }
                            }
                            v_conditionValue = v_conditionValue.ToLower().Replace("'true'", "true").Replace("'false'", "false").Replace("true", "'true'").Replace("false", "'false'");
                            if(!v_conditionValue.Contains("$("))
                            {
                                Object v_result = Navigator.Evaluate(v_conditionValue);
                                if(v_result is Boolean)
                                {
                                    return (Boolean)v_result;
                                }
                            }
                        }
                        catch { }
                    }
                }
                return true;
            }
        }
        #endregion
    }
}
