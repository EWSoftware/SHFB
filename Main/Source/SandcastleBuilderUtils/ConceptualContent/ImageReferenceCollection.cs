//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ImageReferenceCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/06/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the conceptual content
// image reference information.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  04/24/2008  EFW  Created the code
// 1.8.0.0  07/25/2008  EFW  Reworked to support new MSBuild project format
// 1.9.0.0  06/06/2010  EFW  Added support for multi-format build output
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;

using Microsoft.Build.BuildEngine;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This collection class is used to hold the conceptual content image
    /// references for a project.
    /// </summary>
    public class ImageReferenceCollection : BindingList<ImageReference>
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject projectFile;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This can be used to get an image by its unique ID (case-sensitive)
        /// </summary>
        /// <param name="id">The ID of the item to get.</param>
        /// <value>Returns the image with the specified
        /// <see cref="ImageReference.Id" /> or null if not found.</value>
        public ImageReference this[string id]
        {
            get
            {
                foreach(ImageReference ir in this)
                    if(ir.Id == id)
                        return ir;

                return null;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The project file containing the image
        /// build items.</param>
        public ImageReferenceCollection(SandcastleProject project)
        {
            projectFile = project;
            this.Refresh();
        }
        #endregion

        #region Sort collection
        //=====================================================================

        /// <summary>
        /// This is used to sort the collection
        /// </summary>
        /// <remarks>Values are sorted by display title and ID.  Comparisons
        /// are case-insensitive.</remarks>
        public void Sort()
        {
            ((List<ImageReference>)base.Items).Sort((x, y) =>
            {
                int result = String.Compare(x.DisplayTitle, y.DisplayTitle, StringComparison.CurrentCultureIgnoreCase);

                if(result == 0)
                    result = String.Compare(x.Id, y.Id, StringComparison.OrdinalIgnoreCase);

                return result;
            });
        }
        #endregion

        #region Write the image reference collection to a map file
        //=====================================================================

        /// <summary>
        /// Write the image reference collection to a map file ready for use
        /// by <b>BuildAssembler</b>.
        /// </summary>
        /// <param name="filename">The file to which the image reference
        /// collection is saved.</param>
        /// <param name="imagePath">The path to which the image files
        /// should be copied.</param>
        /// <param name="builder">The build process</param>
        /// <remarks>Images with their <see cref="ImageReference.CopyToMedia" />
        /// property set to true are copied to the media folder immediately.</remarks>
        public void SaveAsSharedContent(string filename, string imagePath, BuildProcess builder)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter writer = null;
            string destFile;

            builder.EnsureOutputFoldersExist("media");

            try
            {
                settings.Indent = true;
                settings.CloseOutput = true;
                writer = XmlWriter.Create(filename, settings);

                writer.WriteStartDocument();

                // There are normally some attributes on this element but
                // they aren't used by Sandcastle so we'll ignore them.
                writer.WriteStartElement("stockSharedContentDefinitions");

                foreach(ImageReference ir in this)
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("id", ir.Id);
                    writer.WriteStartElement("image");

                    // The art build component assumes everything is in a
                    // single, common folder.  The build process will ensure
                    // that happens.  As such, we only need the filename here.
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
                    {
                        builder.ReportWarning("BE0010", "Image file '{0}' " +
                            "already exists.  It will be replaced by '{1}'.",
                            destFile, ir.FullPath);
                    }

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
            finally
            {
                if(writer != null)
                    writer.Close();
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================
        
        /// <summary>
        /// This is used to refresh the collection by loading the image
        /// build items from the project.
        /// </summary>
        public void Refresh()
        {
            this.Clear();

            foreach(BuildItem item in projectFile.MSBuildProject.GetEvaluatedItemsByName(BuildAction.Image.ToString()))
                this.Add(new ImageReference(new FileItem(new ProjectElement(projectFile, item))));

            this.Sort();
        }

        /// <summary>
        /// Find the image with the specified ID (case-insensitive).
        /// </summary>
        /// <param name="id">The ID to find</param>
        /// <returns>The image if found or null if not found</returns>
        public ImageReference FindId(string id)
        {
            foreach(ImageReference ir in this)
                if(String.Compare(ir.Id, id, StringComparison.OrdinalIgnoreCase) == 0)
                    return ir;

            return null;
        }

        /// <summary>
        /// Find the image with the specified filename.
        /// </summary>
        /// <param name="filename">The filename to find</param>
        /// <returns>The image if found or null if not found</returns>
        public ImageReference FindImageFile(string filename)
        {
            foreach(ImageReference ir in this)
                if(String.Compare(ir.FullPath, filename, StringComparison.OrdinalIgnoreCase) == 0)
                    return ir;

            return null;
        }
        #endregion
    }
}
