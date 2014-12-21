//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : SqlCopyFromIndexComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/14/2014
// Compiler: Microsoft Visual C#
//
// This is a version of the CopyFromIndexComponent that stores the index data in a persistent SQL database.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.7.0  01/20/2012  EFW  Created the code
// -------  12/26/2013  EFW  Updated the build component to be discoverable via MEF
//===============================================================================================================

using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
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
    /// This is a version of the <c>CopyFromIndexComponent</c> that stores the index data in a persistent SQL
    /// database.
    /// </summary>
    public class SqlCopyFromIndexComponent : CopyFromIndexComponent
    {
        #region Build component factory for MEF - Reflection Index Data (SQL Cache)
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component used for reflection index data
        /// </summary>
        [BuildComponentExport("Reflection Index Data (SQL Cache)", IsVisible = true, IsConfigurable = true,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "This component is used to index reflection data for copying into topics.  It uses a " +
            "Microsoft SQL Server database to cache the .NET Framework reflection data.  This speeds up " +
            "initialization and conserves memory at the expense of some build time in larger projects.  For " +
            "extremely large projects, it is also possible to cache project reference index data to conserve " +
            "memory.\r\n\r\nThis component requires access to a Microsoft SQL Server instance.  Express and " +
            "LocalDB versions are supported.  Some initial configuration and set up steps are required.")]
        public sealed class SqlReflectionIndexDataComponentFactory : BuildComponentFactory
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public SqlReflectionIndexDataComponentFactory()
            {
                base.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Replace,
                    "Copy From Index Component", 1);
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new SqlCopyFromIndexComponent(base.BuildAssembler);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration
            {
                get
                {
                    return @"<index name=""reflection"" value=""/reflection/apis/api"" key=""@id"" cache=""15""
	localCacheSize=""2500"" cacheProject=""false"" connectionString="""">
	<data base=""{@SHFBFolder}Data\{@TargetFrameworkIdentifier}"" recurse=""true"" files=""*.xml""
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
                using(var dlg = new SqlCopyFromIndexConfigDlg(currentConfiguration))
                {
                    if(dlg.ShowDialog() == DialogResult.OK)
                        currentConfiguration = dlg.Configuration;
                }

                return currentConfiguration;
            }
        }
        #endregion

        #region Build component factory for MEF - Comments Index Data (SQL Cache)
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component used for comments index data
        /// </summary>
        [BuildComponentExport("Comments Index Data (SQL Cache)", IsVisible = true, IsConfigurable = true,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "This component is used to index framework comments for copying into topics.  It " +
            "uses a Microsoft SQL Server database to cache the .NET Framework comments.  This speeds up " +
            "initialization and conserves memory at the expense of some build time in larger projects.  For " +
            "extremely large projects, it is also possible to cache project comments data to conserve " +
            "memory.\r\n\r\nThis component requires access to a Microsoft SQL Server instance.  Express and " +
            "LocalDB versions are supported.  Some initial configuration and set up steps are required.")]
        public sealed class SqlCommentsIndexDataComponentFactory : BuildComponentFactory
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public SqlCommentsIndexDataComponentFactory()
            {
                base.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Replace,
                    "Copy From Index Component", 3);
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new SqlCopyFromIndexComponent(base.BuildAssembler);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration
            {
                get
                {
                    return @"<index name=""comments"" value=""/doc/members/member"" key=""@name"" cache=""30"" localCacheSize=""2500""
	cacheProject=""false"" connectionString="""">
	{@FrameworkCommentList}
	{@CommentFileList}
</index>
<copy name=""comments"" source=""*"" target=""/document/comments"" />";
                }
            }

            /// <inheritdoc />
            public override string ConfigureComponent(string currentConfiguration, CompositionContainer container)
            {
                using(var dlg = new SqlCopyFromIndexConfigDlg(currentConfiguration))
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
        protected SqlCopyFromIndexComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler, null)
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
                "[{0}, version {1}]\r\n    SQL Copy From Index Component.  {2}\r\n" +
                "    http://SHFB.CodePlex.com", fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            base.Initialize(configuration);
        }

        /// <inheritdoc />
        protected override IndexedCache CreateIndex(XPathNavigator configuration)
        {
            return new SqlIndexedCache(this, base.Context, configuration);
        }
        #endregion
    }
}
