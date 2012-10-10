//=============================================================================
// System  : HTML to MAML Converter
// File    : ImageReferenceCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/30/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that used to contain a collection of image
// references.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  09/12/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace HtmlToMamlConversion
{
    /// <summary>
    /// This is a collection of image references
    /// </summary>
    public class ImageReferenceCollection : Collection<ImageReference>
    {
        #region Save to media content file
        //=====================================================================

        /// <summary>
        /// Write the image reference collection to a media content file.
        /// </summary>
        /// <param name="filename">The file to which the image reference
        /// collection is saved.</param>
        public void Save(string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter writer = null;

            try
            {
                settings.Indent = true;
                settings.CloseOutput = true;
                writer = XmlWriter.Create(filename, settings);

                writer.WriteStartDocument();

                // There are normally some attributes on this element but
                // they aren't used by Sandcastle so we'll ignore them.
                writer.WriteStartElement("stockSharedContentDefinitions");
                writer.WriteAttributeString("fileAssetGuid",
                    Guid.NewGuid().ToString());
                writer.WriteAttributeString("assetTypeId", "MediaContent");

                foreach(ImageReference ir in this)
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("id", ir.Id);
                    writer.WriteStartElement("image");

                    // The art build component assumes everything is in a
                    // single, common folder.  The build process will ensure
                    // that happens.  As such, we only need the filename here.
                    writer.WriteAttributeString("file", Path.GetFileName(
                        ir.SourceFile));

                    if(!String.IsNullOrEmpty(ir.AlternateText))
                    {
                        writer.WriteStartElement("altText");
                        writer.WriteValue(ir.AlternateText);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();   // </image>
                    writer.WriteEndElement();   // </item>
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
    }
}
