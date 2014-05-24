<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								version="2.0"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:xlink="http://www.w3.org/1999/xlink"
	>
	<!-- ======================================================================================== -->

	<xsl:import href="globalTemplates.xsl"/>

	<!-- ============================================================================================
	Code languages
	============================================================================================= -->

	<xsl:template name="t_codeLang">
		<xsl:param name="p_codeLang"/>
		<xsl:variable name="v_codeLangLC" select="translate($p_codeLang,$g_allUpperCaseLetters,$g_allLowerCaseLetters)"/>
		<xsl:choose>
			<xsl:when test="$v_codeLangLC = 'vbs' or $v_codeLangLC = 'vbscript'">
				<xsl:text>VBScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'vb' or $v_codeLangLC = 'vb#' or $v_codeLangLC = 'vb.net' or $v_codeLangLC = 'vbnet' or $v_codeLangLC = 'kblangvb' or $v_codeLangLC = 'visualbasic'" >
				<xsl:text>VisualBasic</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'visualbasicdeclaration'" >
				<xsl:text>VisualBasicDeclaration</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'visualbasicusage'" >
				<xsl:text>VisualBasicUsage</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'c#' or $v_codeLangLC = 'cs' or $v_codeLangLC = 'csharp'" >
				<xsl:text>CSharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'vb-c#' or $v_codeLangLC = 'visualbasicandcsharp'">
				<xsl:text>visualbasicANDcsharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'cpp' or $v_codeLangLC = 'cpp#' or $v_codeLangLC = 'c' or $v_codeLangLC = 'c++' or $v_codeLangLC = 'kblangcpp' or $v_codeLangLC = 'managedcplusplus'" >
				<xsl:text>ManagedCPlusPlus</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'j#' or $v_codeLangLC = 'jsharp'">
				<xsl:text>JSharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'f#' or $v_codeLangLC = 'fs' or $v_codeLangLC = 'fsharp' or $v_codeLangLC = 'fsscript'">
				<xsl:text>FSharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'jscript#' or $v_codeLangLC = 'jscript' or $v_codeLangLC = 'kbjscript' or $v_codeLangLC = 'jscript.net'">
				<xsl:text>JScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'js' or $v_codeLangLC = 'javascript' or $v_codeLangLC = 'ecmascript'">
				<xsl:text>JavaScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'xml' or $v_codeLangLC = 'xmllang' or $v_codeLangLC = 'xsl'">
				<xsl:text>xmlLang</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'html' or $v_codeLangLC = 'htm'">
				<xsl:text>html</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'xaml'">
				<xsl:text>XAML</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'aspnet' or $v_codeLangLC = 'asp.net'">
				<xsl:text>AspNet</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'pshell' or $v_codeLangLC = 'powershell' or $v_codeLangLC = 'ps1'">
				<xsl:text>pshell</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'sql' or $v_codeLangLC = 'sqlserver' or $v_codeLangLC = 'sql server'">
				<xsl:text>sql</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'py'">
				<xsl:text>Python</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'none'">
				<xsl:value-of select="$v_codeLangLC"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>other</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_codeLangName">
		<xsl:param name="p_codeLang"/>
		<xsl:variable name="v_codeLangUnique">
			<xsl:call-template name="t_codeLang">
				<xsl:with-param name="p_codeLang" select="$p_codeLang"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="$v_codeLangUnique = 'VBScript'">
				<xsl:text>VBScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'VisualBasic' or $v_codeLangUnique = 'VisualBasicUsage' or $v_codeLangUnique = 'VisualBasicDeclaration'" >
				<xsl:text>VB</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'CSharp'" >
				<xsl:text>CSharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'visualbasicANDcsharp'" >
				<xsl:text>VB and C#</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'ManagedCPlusPlus'" >
				<xsl:text>C++</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'JSharp'">
				<xsl:text>VJ#</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'FSharp'" >
				<xsl:text>FSharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'JScript'">
				<xsl:text>JScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'JavaScript'">
				<xsl:text>JavaScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'XAML'">
				<xsl:text>XAML</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'xmlLang'">
				<xsl:text>XML</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'html'">
				<xsl:text>HTML</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'AspNet'">
				<xsl:text>ASP.NET</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'pshell'">
				<xsl:text>PowerShell</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'sql'">
				<xsl:text>SQL</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangUnique = 'Python'">
				<xsl:text>Python</xsl:text>
			</xsl:when>
			<xsl:otherwise/>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_mshelpCodelangAttributes">
		<xsl:param name="snippets"/>
		<xsl:for-each select="$snippets">

			<xsl:if test="not(@language=preceding::*/@language)">
				<xsl:variable name="v_codeLang">
					<xsl:choose>
						<xsl:when test="@language = 'VBScript' or @language = 'vbs'">
							<xsl:text>VBScript</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'VisualBasic' or @language = 'vb' or @language = 'vb#' or @language = 'VB' or @language = 'kbLangVB'" >
							<xsl:text>kbLangVB</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'CSharp' or @language = 'c#' or @language = 'cs' or @language = 'C#'" >
							<xsl:text>CSharp</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'ManagedCPlusPlus' or @language = 'cpp' or @language = 'cpp#' or @language = 'c' or @language = 'c++' or @language = 'C++' or @language = 'kbLangCPP'" >
							<xsl:text>kbLangCPP</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'JSharp' or @language = 'j#' or @language = 'jsharp' or @language = 'VJ#'">
							<xsl:text>VJ#</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'JScript' or @language = 'js' or @language = 'jscript#' or @language = 'jscript' or @language = 'JScript' or @language = 'kbJScript'">
							<xsl:text>kbJScript</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'XAML' or @language = 'xaml'">
							<xsl:text>XAML</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'JavaScript' or @language = 'javascript'">
							<xsl:text>JavaScript</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'xml'">
							<xsl:text>xml</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'html'">
							<xsl:text>html</xsl:text>
						</xsl:when>
						<xsl:when test="@language = 'vb-c#'">
							<xsl:text>visualbasicANDcsharp</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:text>other</xsl:text>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$v_codeLang='other'"/>
					<!-- If $v_codeLang is already authored, then do nothing -->
					<xsl:when test="/document/metadata/attribute[@name='codelang']/text() = $v_codeLang"/>
					<xsl:otherwise>
						<MSHelp:Attr Name="DevLang" Value="{$v_codeLang}"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>

		</xsl:for-each>
	</xsl:template>

	<!-- ============================================================================================
	Syntax and Code sections
	============================================================================================= -->

	<xsl:template name="t_putSyntaxSections">
		<xsl:param name="p_nodes"/>

		<xsl:variable name="v_id" select="generate-id(msxsl:node-set($p_nodes))" />

		<!-- Count non-XAML snippets and XAML snippets with something other than boilerplate content -->
		<xsl:variable name="v_nodeCount" select="count(msxsl:node-set($p_nodes)/self::node()[@codeLanguage != 'XAML' or
			(@codeLanguage = 'XAML' and boolean(./div[@class='xamlAttributeUsageHeading' or
			@class='xamlObjectElementUsageHeading' or @class='xamlContentElementUsageHeading' or
			@class='xamlPropertyElementUsageHeading']))])" />

		<div class="OH_CodeSnippetContainer">
			<div class="OH_CodeSnippetContainerTabs">
				<xsl:choose>
					<xsl:when test="$v_nodeCount = 1">
						<div class="OH_CodeSnippetContainerTabLeftActive" id="{$v_id}_tabimgleft"><xsl:text> </xsl:text></div>
					</xsl:when>
					<xsl:otherwise>
						<div class="OH_CodeSnippetContainerTabLeft" id="{$v_id}_tabimgleft"><xsl:text> </xsl:text></div>
					</xsl:otherwise>
				</xsl:choose>

				<xsl:for-each select="msxsl:node-set($p_nodes)">
					<xsl:choose>
						<!-- Suppress tabs for boilerplate XAML which isn't currently shown -->
						<xsl:when test="@codeLanguage='XAML' and not(boolean(./div[
										@class='xamlAttributeUsageHeading' or @class='xamlObjectElementUsageHeading' or
										@class='xamlContentElementUsageHeading' or @class='xamlPropertyElementUsageHeading']))" />
						<xsl:otherwise>
							<div id="{$v_id}_tab{position()}">
								<xsl:attribute name="class">
									<xsl:choose>
										<xsl:when test="$v_nodeCount = 1">
											<xsl:text>OH_CodeSnippetContainerTabSolo</xsl:text>
										</xsl:when>
										<xsl:when test="position() = 1">
											<xsl:text>OH_CodeSnippetContainerTabFirst</xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:text>OH_CodeSnippetContainerTab</xsl:text>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:attribute>
								<xsl:choose>
									<xsl:when test="$v_nodeCount = 1">
										<include item="devlang_{@codeLanguage}" />
									</xsl:when>
									<xsl:otherwise>
										<!-- Use onclick rather than href or HV 2.0 messes up the link -->
										<a href="#" onclick="javascript:ChangeTab('{$v_id}','{@style}','{position()}','{$v_nodeCount}');return false;">
											<include item="devlang_{@codeLanguage}" />
										</a>
									</xsl:otherwise>
								</xsl:choose>
							</div>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>

				<xsl:choose>
					<xsl:when test="$v_nodeCount = 1">
						<div class="OH_CodeSnippetContainerTabRightActive" id="{$v_id}_tabimgright"><xsl:text> </xsl:text></div>
					</xsl:when>
					<xsl:otherwise>
						<div class="OH_CodeSnippetContainerTabRight" id="{$v_id}_tabimgright"><xsl:text> </xsl:text></div>
					</xsl:otherwise>
				</xsl:choose>
			</div>

			<div class="OH_CodeSnippetContainerCodeCollection">
				<div class="OH_CodeSnippetToolBar">
					<div class="OH_CodeSnippetToolBarText">
						<a id="{$v_id}_copyCode" href="#" onclick="javascript:CopyToClipboard('{$v_id}');return false;">
							<includeAttribute name="title" item="copyCode" />
							<include item="copyCode" />
						</a>
					</div>
				</div>

				<xsl:for-each select="msxsl:node-set($p_nodes)">
					<xsl:choose>
						<!-- Suppress snippets for boilerplate XAML which isn't currently shown -->
						<xsl:when test="@codeLanguage='XAML' and not(boolean(./div[
										@class='xamlAttributeUsageHeading' or @class='xamlObjectElementUsageHeading' or
										@class='xamlContentElementUsageHeading' or @class='xamlPropertyElementUsageHeading']))" />
						<xsl:otherwise>
							<div id="{$v_id}_code_Div{position()}" class="OH_CodeSnippetContainerCode">
								<xsl:attribute name="style">
									<xsl:choose>
										<xsl:when test="$v_nodeCount = 1 or position() = 1">
											<xsl:text>display: block</xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:text>display: none</xsl:text>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:attribute>
								<xsl:choose>
									<xsl:when test="@codeLanguage='XAML'">
										<xsl:call-template name="XamlSyntaxBlock" />
									</xsl:when>
									<xsl:otherwise>
										<pre xml:space="preserve"><xsl:copy-of select="node()"/></pre>
									</xsl:otherwise>
								</xsl:choose>
							</div>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>
			</div>
		</div>

		<!-- Register the tab set even for single tabs as we may need to hide the Copy link -->
		<script type="text/javascript">AddLanguageTabSet("<xsl:value-of select="$v_id" />");</script>
	</xsl:template>

	<xsl:template name="t_putCodeSections">
		<xsl:param name="p_nodes"/>

		<xsl:variable name="v_id" select="generate-id(msxsl:node-set($p_nodes))" />
		<xsl:variable name="v_nodeCount" select="count(msxsl:node-set($p_nodes))" />

		<div class="OH_CodeSnippetContainer">
			<xsl:choose>
				<!-- Omit the tab if there is a title attribute with a single space -->
				<xsl:when test="$v_nodeCount = 1 and msxsl:node-set($p_nodes)//@title = ' '" />
				<xsl:otherwise>
					<div class="OH_CodeSnippetContainerTabs">
						<xsl:choose>
							<xsl:when test="$v_nodeCount = 1">
								<div class="OH_CodeSnippetContainerTabLeftActive" id="{$v_id}_tabimgleft">
									<xsl:text> </xsl:text>
								</div>
							</xsl:when>
							<xsl:otherwise>
								<div class="OH_CodeSnippetContainerTabLeft" id="{$v_id}_tabimgleft">
									<xsl:text> </xsl:text>
								</div>
							</xsl:otherwise>
						</xsl:choose>

						<xsl:for-each select="msxsl:node-set($p_nodes)">
							<div id="{$v_id}_tab{position()}">
								<xsl:attribute name="class">
									<xsl:choose>
										<xsl:when test="$v_nodeCount = 1">
											<xsl:text>OH_CodeSnippetContainerTabSolo</xsl:text>
										</xsl:when>
										<xsl:when test="@phantom and position() = 1">
											<xsl:text>OH_CodeSnippetContainerTabDisabled</xsl:text>
										</xsl:when>
										<xsl:when test="@phantom">
											<xsl:text>OH_CodeSnippetContainerTabDisabledNotFirst</xsl:text>
										</xsl:when>
										<xsl:when test="position() = 1">
											<xsl:text>OH_CodeSnippetContainerTabFirst</xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:text>OH_CodeSnippetContainerTab</xsl:text>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:attribute>
								<xsl:choose>
									<xsl:when test="$v_nodeCount = 1">
										<xsl:choose>
											<xsl:when test="@title">
												<xsl:value-of select="@title" />
											</xsl:when>
											<xsl:otherwise>
												<include item="devlang_{@codeLanguage}" />
											</xsl:otherwise>
										</xsl:choose>
									</xsl:when>
									<xsl:otherwise>
										<!-- Use onclick rather than href or HV 2.0 messes up the link -->
										<a href="#" onclick="javascript:ChangeTab('{$v_id}','{@style}','{position()}','{$v_nodeCount}');return false;">
											<include item="devlang_{@codeLanguage}" />
										</a>
									</xsl:otherwise>
								</xsl:choose>
							</div>
						</xsl:for-each>

						<xsl:choose>
							<xsl:when test="$v_nodeCount = 1">
								<div class="OH_CodeSnippetContainerTabRightActive" id="{$v_id}_tabimgright">
									<xsl:text> </xsl:text>
								</div>
							</xsl:when>
							<xsl:otherwise>
								<div class="OH_CodeSnippetContainerTabRight" id="{$v_id}_tabimgright">
									<xsl:text> </xsl:text>
								</div>
							</xsl:otherwise>
						</xsl:choose>
					</div>
				</xsl:otherwise>
			</xsl:choose>

			<div class="OH_CodeSnippetContainerCodeCollection">
				<div class="OH_CodeSnippetToolBar">
					<div class="OH_CodeSnippetToolBarText">
						<a id="{$v_id}_copyCode" href="#" onclick="javascript:CopyToClipboard('{$v_id}');return false;">
							<includeAttribute name="title" item="copyCode" />
							<include item="copyCode" />
						</a>
					</div>
				</div>

				<xsl:for-each select="msxsl:node-set($p_nodes)">
					<div id="{$v_id}_code_Div{position()}" class="OH_CodeSnippetContainerCode">
						<xsl:attribute name="style">
							<xsl:choose>
								<xsl:when test="$v_nodeCount = 1 or position() = 1">
									<xsl:text>display: block</xsl:text>
								</xsl:when>
								<xsl:otherwise>
									<xsl:text>display: none</xsl:text>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:attribute>
						<xsl:choose>
							<xsl:when test="@phantom">
								<include item="noCodeExample" />
							</xsl:when>
							<xsl:otherwise>
								<pre xml:space="preserve"><xsl:copy-of select="node()"/></pre>
							</xsl:otherwise>
						</xsl:choose>
					</div>
				</xsl:for-each>
			</div>
		</div>

		<!-- Register the tab set even for single tabs as we may need to hide the Copy link -->
		<script type="text/javascript">AddLanguageTabSet("<xsl:value-of select="$v_id" />");</script>
	</xsl:template>

	<!-- ============================================================================================
	XAML Syntax
	============================================================================================= -->

	<xsl:template name="XamlSyntaxBlock">
		<!-- Branch based on page type -->
		<xsl:choose>
			<!-- Display boilerplate for page types that cannot be used in XAML -->
			<xsl:when test="$g_apiTopicSubGroup='method' or $g_apiTopicSubGroup='constructor' or
                      $g_apiTopicSubGroup='interface' or $g_apiTopicSubGroup='delegate' or
                      $g_apiTopicSubGroup='field'">
				<xsl:call-template name="ShowXamlSyntaxBoilerplate"/>
			</xsl:when>

			<!-- Class and structure -->
			<xsl:when test="$g_apiTopicSubGroup='class' or $g_apiTopicSubGroup='structure'">
				<xsl:choose>
					<xsl:when test="div[@class='xamlObjectElementUsageHeading']">
						<xsl:call-template name="ShowAutogeneratedXamlSyntax">
							<xsl:with-param name="autogenContent">
								<xsl:copy-of select="div[@class='xamlObjectElementUsageHeading']"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="ShowXamlSyntaxBoilerplate">
							<xsl:with-param name="p_messageId">
								<xsl:copy-of select="."/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>

			<!-- Enumeration -->
			<xsl:when test="$g_apiTopicSubGroup='enumeration'">
				<xsl:choose>
					<xsl:when test="div[@class='nonXamlAssemblyBoilerplate']"/>
					<xsl:otherwise>
						<pre xml:space="preserve"><include item="enumerationOverviewXamlSyntax"/><xsl:text/></pre>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>

			<!-- Property -->
			<xsl:when test="$g_apiTopicSubGroup='property' or $g_apiTopicSubSubGroup='attachedProperty'">
				<!-- Property Element Usage -->
				<xsl:if test="div[@class='xamlPropertyElementUsageHeading' or @class='xamlContentElementUsageHeading']">
					<xsl:call-template name="ShowAutogeneratedXamlSyntax">
						<xsl:with-param name="autogenContent">
							<xsl:copy-of select="div[@class='xamlPropertyElementUsageHeading' or @class='xamlContentElementUsageHeading']"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
				<!-- Attribute Usage -->
				<xsl:if test="div[@class='xamlAttributeUsageHeading']">
					<xsl:call-template name="ShowAutogeneratedXamlSyntax">
						<xsl:with-param name="autogenContent">
							<xsl:copy-of select="div[@class='xamlAttributeUsageHeading']"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
				<!-- Show auto-generated boilerplate if no other content to override it -->
				<xsl:if test="not(div[@class='xamlPropertyElementUsageHeading' or
								@class='xamlContentElementUsageHeading' or @class='xamlAttributeUsageHeading'])">
					<xsl:call-template name="ShowXamlSyntaxBoilerplate">
						<xsl:with-param name="p_messageId">
							<xsl:copy-of select="div/*"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
			</xsl:when>

			<!-- Event -->
			<xsl:when test="$g_apiTopicSubGroup='event' or $g_apiTopicSubSubGroup='attachedEvent'">
				<!-- If XamlSyntaxUsage component generated an Attribute Usage block, this template will show it -->
				<xsl:call-template name="ShowAutogeneratedXamlSyntax">
					<xsl:with-param name="autogenContent">
						<xsl:copy-of select="div[@class='xamlAttributeUsageHeading']"/>
					</xsl:with-param>
				</xsl:call-template>
				<!-- If XamlSyntaxUsage component generated a boilerplate block, this template will show it -->
				<xsl:call-template name="ShowXamlSyntaxBoilerplate">
					<xsl:with-param name="p_messageId">
						<xsl:copy-of select="div/*"/>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:when>

		</xsl:choose>
	</xsl:template>

	<!-- Displays one of the standard XAML boilerplate strings. -->
	<xsl:template name="ShowXamlSyntaxBoilerplate">
		<xsl:param name="p_messageId"/>

		<!-- Do not show any XAML syntax boilerplate strings -->
		<xsl:variable name="boilerplateId"/>

		<!-- If future requirements call for showing one or more boilerplate strings for XAML, use the commented out
				 code to specify the ids of the shared content items to include.
         NOTE: The markup like div/@class='interfaceOverviewXamlSyntax' is added by XamlUsageSyntax.cs in
				 BuildAssembler. -->
		<!--
    <xsl:variable name="boilerplateId">
      <xsl:value-of select="div/@class[.='interfaceOverviewXamlSyntax' or
                    .='propertyXamlSyntax_abstractType' or                    
                    .='classXamlSyntax_abstract']"/>
    </xsl:variable>
    -->

		<xsl:if test="$boilerplateId != ''">
			<pre xml:space="preserve"><include item="{$boilerplateId}">
          <xsl:choose>
            <xsl:when test="$p_messageId !='' or (count(msxsl:node-set($p_messageId)/*) &gt; 0)">
              <parameter><xsl:copy-of select="msxsl:node-set($p_messageId)"/></parameter>
            </xsl:when>
            <!-- Make sure we at least pass in an empty param because some boilerplates expect them -->
            <xsl:otherwise>
              <parameter/>
            </xsl:otherwise>
          </xsl:choose>
        </include><xsl:text/></pre>
		</xsl:if>
	</xsl:template>

	<!-- Displays the auto-generated XAML syntax for page types other than enumerations -->
	<xsl:template name="ShowAutogeneratedXamlSyntax">
		<xsl:param name="autogenContent"/>
		<xsl:if test="count(msxsl:node-set($autogenContent))>0">
			<xsl:for-each select="msxsl:node-set($autogenContent)/div">
				<pre xml:space="preserve"><xsl:copy-of select="node()"/><xsl:text/></pre>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>
