//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConceptualContent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/31/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to hold the conceptual content for a project during a build
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/24/2008  EFW  Created the code
// 06/06/2010  EFW  Added support for multi-format build output
// 04/03/2013  EFW  Added support for merging content from another project (plug-in support)
// 12/01/2015  EFW  Merged conceptual and reference topic build steps
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using Sandcastle.Core;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This class is used to hold the conceptual content settings for a project during a build
    /// </summary>
    public class ConceptualContentSettings
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the conceptual content image files
        /// </summary>
        public IList<ImageReference> ImageFiles { get; }

        /// <summary>
        /// This is used to get the conceptual content code snippet files
        /// </summary>
        public IList<ContentFile> CodeSnippetFiles { get; }

        /// <summary>
        /// This is used to get the conceptual content token files
        /// </summary>
        public IList<ContentFile> TokenFiles { get; }

        /// <summary>
        /// This is used to get the conceptual content layout files
        /// </summary>
        public IList<ContentFile> ContentLayoutFiles { get; }

        /// <summary>
        /// This is used to get a collection of the conceptual content topics
        /// </summary>
        /// <remarks>Each item in the collection represents one content layout file from the project</remarks>
        public IList<TopicCollection> Topics { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The project from which to load the settings</param>
        public ConceptualContentSettings(SandcastleProject project)
        {
            if(project == null)
                throw new ArgumentNullException(nameof(project));

            this.ImageFiles = project.ImagesReferences.ToList();
            this.CodeSnippetFiles = project.ContentFiles(BuildAction.CodeSnippets).OrderBy(f => f.LinkPath).ToList();
            this.TokenFiles = project.ContentFiles(BuildAction.Tokens).OrderBy(f => f.LinkPath).ToList();
            this.ContentLayoutFiles = project.ContentFiles(BuildAction.ContentLayout).ToList();
            this.Topics = project.ContentFiles(BuildAction.ContentLayout).Select(file => new TopicCollection(file)).ToList();
        }
        #endregion

        #region Build process methods
        //=====================================================================

        /// <summary>
        /// This is used to copy the additional content token, image, and topic files to the build folder
        /// </summary>
        /// <param name="builder">The build process</param>
        /// <remarks>This will copy the code snippet file if specified, save token information to a shared
        /// content file called <strong>_Tokens_.xml</strong> in the build process's working folder, copy the
        /// image files to the <strong>.\media</strong> folder in the build process's working folder, save the
        /// media map to a file called <strong>_MediaContent_.xml</strong> in the build process's working folder,
        /// and save the topic files to the <strong>.\ddueXml</strong> folder in the build process's working
        /// folder.  The topic files will have their content wrapped in a <c>&lt;topic&gt;</c> tag if needed and
        /// will be named using their <see cref="Topic.Id" /> value.</remarks>
        public void CopyContentFiles(BuildProcess builder)
        {
            string folder;
            bool missingFile = false;

            if(builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.ReportProgress("Copying standard token shared content file...");
            builder.SubstitutionTags.TransformTemplate("HelpFileBuilderTokens.tokens", builder.TemplateFolder,
                builder.WorkingFolder);

            builder.ReportProgress("Checking for other token files...");

            foreach(var tokenFile in this.TokenFiles)
                if(!File.Exists(tokenFile.FullPath))
                {
                    missingFile = true;
                    builder.ReportProgress("    Missing token file: {0}", tokenFile.FullPath);
                }
                else
                {
                    builder.ReportProgress("    {0} -> {1}", tokenFile.FullPath,
                        Path.Combine(builder.WorkingFolder, Path.GetFileName(tokenFile.FullPath)));
                    builder.SubstitutionTags.TransformTemplate(Path.GetFileName(tokenFile.FullPath),
                        Path.GetDirectoryName(tokenFile.FullPath), builder.WorkingFolder);
                }

            if(missingFile)
                throw new BuilderException("BE0052", "One or more token files could not be found");

            builder.ReportProgress("Checking for code snippets files...");

            foreach(var snippetsFile in this.CodeSnippetFiles)
                if(!File.Exists(snippetsFile.FullPath))
                {
                    missingFile = true;
                    builder.ReportProgress("    Missing code snippets file: {0}", snippetsFile.FullPath);
                }
                else
                    builder.ReportProgress("    Found {0}", snippetsFile.FullPath);

            if(missingFile)
                throw new BuilderException("BE0053", "One or more code snippets files could not be found");

            // Save the image info to a shared content file and copy the image files to the working folder
            folder = Path.Combine(builder.WorkingFolder, "media");

            if(!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Create the build process's help format output folders too if needed
            builder.EnsureOutputFoldersExist("media");

            builder.ReportProgress("Copying images and creating the media map file...");

            // Copy all image project items and create the content file
            this.SaveImageSharedContent(Path.Combine(builder.WorkingFolder, "_MediaContent_.xml"), folder, builder);

            // Copy the topic files
            folder = Path.Combine(builder.WorkingFolder, "ddueXml");

            if(!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            builder.ReportProgress("Generating conceptual topic files");

            // Get the list of valid framework namespaces for the referenced namespace search in each topic
            HashSet<string> validNamespaces = new HashSet<string>(Directory.EnumerateFiles(builder.FrameworkReflectionDataFolder,
                "*.xml", SearchOption.AllDirectories).Select(f => Path.GetFileNameWithoutExtension(f)));

            // Create topic files
            foreach(TopicCollection tc in Topics)
            {
                tc.Load();
                tc.GenerateConceptualTopics(folder, builder, validNamespaces);
            }
        }

        /// <summary>
        /// Write the image reference collection to a map file ready for use by <strong>BuildAssembler</strong>
        /// </summary>
        /// <param name="filename">The file to which the image reference collection is saved</param>
        /// <param name="imagePath">The path to which the image files should be copied</param>
        /// <param name="builder">The build process</param>
        /// <remarks>Images with their <see cref="ImageReference.CopyToMedia" /> property set to true are copied
        /// to the media folder immediately.</remarks>
        public void SaveImageSharedContent(string filename, string imagePath, BuildProcess builder)
        {
            string destFile;

            if(builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.EnsureOutputFoldersExist("media");

            using(var writer = XmlWriter.Create(filename, new XmlWriterSettings { Indent = true, CloseOutput = true }))
            {
                writer.WriteStartDocument();

                // There are normally some attributes on this element but they aren't used by Sandcastle so we'll
                // ignore them.
                writer.WriteStartElement("stockSharedContentDefinitions");

                foreach(var ir in this.ImageFiles)
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("id", ir.Id);
                    writer.WriteStartElement("image");

                    // The art build component assumes everything is in a single, common folder.  The build
                    // process will ensure that happens.  As such, we only need the filename here.
                    writer.WriteAttributeString("file", ir.Filename);

                    if(!String.IsNullOrEmpty(ir.AlternateText))
                    {
                        writer.WriteStartElement("altText");
                        writer.WriteValue(ir.AlternateText);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();   // </image>
                    writer.WriteEndElement();   // </item>

                    destFile = Path.Combine(imagePath, ir.Filename);

                    if(File.Exists(destFile))
                        builder.ReportWarning("BE0010", "Image file '{0}' already exists.  It will be replaced " +
                            "by '{1}'.", destFile, ir.FullPath);

                    builder.ReportProgress("    {0} -> {1}", ir.FullPath, destFile);

                    // The attributes are set to Normal so that it can be deleted after the build
                    File.Copy(ir.FullPath, destFile, true);
                    File.SetAttributes(destFile, FileAttributes.Normal);

                    // Copy it to the output media folders if CopyToMedia is true
                    if(ir.CopyToMedia)
                        foreach(string baseFolder in builder.HelpFormatOutputFolders)
                        {
                            destFile = Path.Combine(baseFolder + "media", ir.Filename);

                            builder.ReportProgress("    {0} -> {1} (Always copied)", ir.FullPath, destFile);

                            File.Copy(ir.FullPath, destFile, true);
                            File.SetAttributes(destFile, FileAttributes.Normal);
                        }
                }

                writer.WriteEndElement();   // </stockSharedContentDefinitions>
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// This is used to create the conceptual content build configuration files
        /// </summary>
        /// <param name="builder">The build process</param>
        /// <remarks>This will create the companion files used to resolve conceptual links and the
        /// <strong>_ContentMetadata_.xml</strong> configuration file.  It will also merge the conceptual topics
        /// into the BuildAssembler manifest file.</remarks>
        public void CreateConfigurationFiles(BuildProcess builder)
        {
            if(builder == null)
                throw new ArgumentNullException(nameof(builder));

            this.CreateCompanionFiles(builder);
            this.CreateContentMetadata(builder);
            this.MergeConceptualManifest(builder);
        }

        /// <summary>
        /// This is used to create the companion files used to resolve conceptual links
        /// </summary>
        /// <param name="builder">The build process</param>
        private void CreateCompanionFiles(BuildProcess builder)
        {
            string destFolder = Path.Combine(builder.WorkingFolder, "xmlComp");

            builder.ReportProgress("    Companion files");

            if(!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            foreach(TopicCollection tc in this.Topics)
                foreach(Topic t in tc)
                    t.WriteCompanionFile(destFolder, builder);
        }

        /// <summary>
        /// Create the content metadata file
        /// </summary>
        /// <param name="builder">The build process</param>
        /// <remarks>The content metadata file contains metadata information for each topic such as its title,
        /// table of contents title, help attributes, and index keywords.  Help attributes are a combination
        /// of the project-level help attributes and any parsed from the topic file.  Any replacement tags in
        /// the token values will be replaced with the appropriate project values.
        /// 
        /// <p/>A true MAML version of this file contains several extra attributes.  Since Sandcastle doesn't use
        /// them, I'm not going to waste time adding them.  The only stuff written is what is required by
        /// Sandcastle.  In addition, I'm putting the <c>title</c> and <c>PBM_FileVersion</c> item elements in
        /// here rather than use the separate companion files.  They all end up in the metadata section of the
        /// topic being built so this saves having two extra components in the configuration that do the same
        /// thing with different files.</remarks>
        private void CreateContentMetadata(BuildProcess builder)
        {
            builder.ReportProgress("    _ContentMetadata_.xml");

            using(var writer = XmlWriter.Create(Path.Combine(builder.WorkingFolder, "_ContentMetadata_.xml"),
              new XmlWriterSettings { Indent = true, CloseOutput = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("metadata");

                // Write out each topic and all of its sub-topics
                foreach(TopicCollection tc in this.Topics)
                    foreach(Topic t in tc)
                        t.WriteMetadata(writer, builder);

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// Merge the conceptual topic IDs into the BuildAssembler manifest file.
        /// </summary>
        /// <param name="builder">The build process</param>
        private void MergeConceptualManifest(BuildProcess builder)
        {
            string conceptualManifest = Path.Combine(builder.WorkingFolder, "ConceptualManifest.xml"),
                referenceManifest = Path.Combine(builder.WorkingFolder, "manifest.xml");

            builder.ReportProgress("    Merging topic IDs into manifest.xml");

            using(var writer = XmlWriter.Create(conceptualManifest, new XmlWriterSettings { Indent = true, CloseOutput = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("topics");

                foreach(TopicCollection tc in this.Topics)
                    foreach(Topic t in tc)
                        t.WriteManifest(writer, builder);

                if(File.Exists(referenceManifest))
                    foreach(var topic in ComponentUtilities.XmlStreamAxis(referenceManifest, "topic"))
                    {
                        writer.WriteStartElement("topic");

                        foreach(var attr in topic.Attributes())
                            writer.WriteAttributeString(attr.Name.LocalName, attr.Value);

                        writer.WriteEndElement();
                    }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            if(File.Exists(referenceManifest))
                File.Copy(referenceManifest, Path.ChangeExtension(referenceManifest, "old"), true);

            File.Copy(conceptualManifest, referenceManifest, true);
            File.Delete(conceptualManifest);
        }
        #endregion
    }
}
