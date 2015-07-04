//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ESentCopyFromIndexComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/14/2014
// Compiler: Microsoft Visual C#
//
// This is a version of the CopyFromIndexComponent that stores the index data in one or more persistent ESENT
// databases.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.7.0  01/20/2013  EFW  Created the code
// -------  12/26/2013  EFW  Updated the build component to be discoverable via MEF
//===============================================================================================================

using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

using Microsoft.Ddue.Tools;
using Microsoft.Ddue.Tools.Commands;

using SandcastleBuilder.Components.Commands;
using SandcastleBuilder.Components.UI;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a version of the <c>CopyFromIndexComponent</c> that stores the index data in one or more
    /// persistent ESENT databases.
    /// </summary>
    public class ESentCopyFromIndexComponent : CopyFromIndexComponent
    {
        #region Build component factory for MEF - Reflection Index Data (ESENT Cache)
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component used for reflection index data
        /// </summary>
        [BuildComponentExport("Reflection Index Data (ESENT Cache)", IsVisible = true, IsConfigurable = true,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "This component is used to index reflection data for copying into topics.  It uses an " +
            "ESENT database to cache the .NET Framework reflection data.  This speeds up initialization and " +
            "conserves memory at the expense of some build time in larger projects.   For extremely large " +
            "projects, it is also possible to cache project reference index data to conserve memory.\r\n\r\n" +
            "The ESENT database engine is part of every version of Microsoft Windows and no set up is required.")]
        public sealed class ESentReflectionIndexDataComponentFactory : BuildComponentFactory
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public ESentReflectionIndexDataComponentFactory()
            {
                base.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Replace,
                    "Copy From Index Component", 1);
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new ESentCopyFromIndexComponent(base.BuildAssembler);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration
            {
                get
                {
                    return @"<index name=""reflection"" value=""/reflection/apis/api"" key=""@id"" cache=""15"" localCacheSize=""2500""
	frameworkCachePath=""{@LocalDataFolder}Cache\ReflectionIndexCache"" projectCachePath="""">
	<data base=""{@FrameworkReflectionDataFolder}"" recurse=""true"" files=""*.xml""
		duplicateWarning=""false"" groupId=""ReflectionIndexCache"">
		{@ReferenceLinkNamespaceFiles}
	</data>
	<data files=""reflection.xml"" groupId=""Project_Ref_{@UniqueID}"" />
</index>
<copy name=""reflection"" source=""*"" target=""/document/reference"" />";
                }
            }

            /// <inheritdoc />
            public override string ConfigureComponent(string currentConfiguration, CompositionContainer container)
            {
                using(var dlg = new ESentReflectionIndexConfigDlg(currentConfiguration))
                {
                    if(dlg.ShowDialog() == DialogResult.OK)
                        currentConfiguration = dlg.Configuration;
                }

                return currentConfiguration;
            }
        }
        #endregion

        #region Build component factory for MEF - Comments Index Data (ESENT Cache)
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component used for comments index data
        /// </summary>
        [BuildComponentExport("Comments Index Data (ESENT Cache)", IsVisible = true, IsConfigurable = true,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "This component is used to index framework comments for copying into topics.  It " +
            "uses an ESENT database to cache the .NET Framework comments.  This speeds up initialization and " +
            "conserves memory at the expense of some build time in larger projects.  For extremely large " +
            "projects, it is also possible to cache project comments data to conserve memory.\r\n\r\n" +
            "The ESENT database engine is part of every version of Microsoft Windows and no set up is required.")]
        public sealed class ESentCommentsIndexDataComponentFactory : BuildComponentFactory
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public ESentCommentsIndexDataComponentFactory()
            {
                base.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Replace,
                    "Copy From Index Component", 3);
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new ESentCopyFromIndexComponent(base.BuildAssembler);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration
            {
                get
                {
                    return @"<index name=""comments"" value=""/doc/members/member"" key=""@name"" cache=""30"" localCacheSize=""2500""
	frameworkCachePath=""{@LocalDataFolder}Cache\CommentsIndexCache"" projectCachePath="""">
	{@FrameworkCommentList}
	{@CommentFileList}
</index>
<copy name=""comments"" source=""*"" target=""/document/comments"" />";
                }
            }

            /// <inheritdoc />
            public override string ConfigureComponent(string currentConfiguration, CompositionContainer container)
            {
                using(var dlg = new ESentCommentsIndexConfigDlg(currentConfiguration))
                {
                    if(dlg.ShowDialog() == DialogResult.OK)
                        currentConfiguration = dlg.Configuration;
                }

                return currentConfiguration;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected ESentCopyFromIndexComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler, null)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                "[{0}, version {1}]\r\n    ESENT Copy From Index Component.  {2}\r\n" +
                "    https://GitHub.com/EWSoftware/SHFB", fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            base.Initialize(configuration);
        }

        /// <inheritdoc />
        protected override IndexedCache CreateIndex(XPathNavigator configuration)
        {
            return new ESentIndexedCache(this, base.Context, configuration);
        }
        #endregion
    }
}
