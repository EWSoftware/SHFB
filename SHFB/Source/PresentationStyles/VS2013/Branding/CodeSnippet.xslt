<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:xhtml="http://www.w3.org/1999/xhtml"
                xmlns:mtps="http://msdn2.microsoft.com/mtps"
                xmlns:cs="urn:code-snippet"
								exclude-result-prefixes="msxsl"
>
	<!-- TODO: See ps-codesnippet.xslt for the replacement plan. -->

	<xsl:variable name="allOverLangTabsSetText">
		<xsl:for-each select="//mtps:CodeSnippet">
			<xsl:variable name="lang" select="./@Language"/>
			<xsl:variable name="displang" select="./@DisplayLanguage"/>
			<xsl:variable name="uselang">
				<xsl:if test="string-length($displang)>0">
					<xsl:value-of select="$displang"/>
				</xsl:if>
				<xsl:if test="string-length($displang)=0">
					<xsl:value-of select="$lang"/>
				</xsl:if>
			</xsl:variable>
			<xsl:if test="string-length($uselang)>0">
				<xsl:value-of select="concat($uselang,';')"/>
			</xsl:if>
		</xsl:for-each>
	</xsl:variable>

	<xsl:variable name="uniqueLangTabsSet">
		<value>Visual Basic</value>
		<value>C#</value>
		<value>Visual C++</value>
		<value>F#</value>
		<value>JScript</value>
	</xsl:variable>
	<xsl:variable name="uniqueLangTabsSetCount" select="count(msxsl:node-set($uniqueLangTabsSet)/value)" />

	<xsl:variable name="needToCombineVBTabs">
		<xsl:value-of select="cs:needToCombineVBTabs($allOverLangTabsSetText)"/>
	</xsl:variable>

	<!-- a free standing snippet is one that is not part of a group of snippets inside of a CollapsibleArea -->
	<xsl:key name="code-snippet-use" use="'free-standing-snippet'"
            match="//mtps:CodeSnippet[parent::*[count(descendant::mtps:CodeSnippet) = 1]
           or parent::xhtml:div[starts-with(@id,'snippetGroup') and count(mtps:CodeSnippet) = 1]
           or parent::mtps:CollapsibleArea[count(mtps:CodeSnippet) = 1]
           or (parent::mtps:CollapsibleArea and name(following-sibling::*[1])!='CodeSnippet' and name(preceding-sibling::*[1])!='CodeSnippet')
           or (//mtps:CodeSnippet[parent::*[count(descendant::mtps:CodeSnippet) > 1]] and name(following-sibling::*[1])!='CodeSnippet' and name(preceding-sibling::*[1])!='CodeSnippet')]"/>

	<!-- a lead snippet is the first child snippet beneath a CollapsibleArea or child of DIV with id beginning with "snippetGroup" -->
	<xsl:key name="code-snippet-use" use="'lead-snippet'"
				  match="//mtps:CodeSnippet[ancestor::mtps:CollapsibleArea[count(descendant::mtps:CodeSnippet) > 1] and (not(preceding-sibling::*[1][self::mtps:CodeSnippet])) and not(parent::xhtml:div[starts-with(@id,'snippetGroup') and count(mtps:CodeSnippet)>1])]
          | //mtps:CodeSnippet[parent::xhtml:div[count(mtps:CodeSnippet) > 1] and (not(preceding-sibling::*[1][self::mtps:CodeSnippet]))]"/>

	<!-- this just skips over any code snippet that is not standalone or a lead snippet
  as they will be processed elsewhere
  -->
	<xsl:template match="mtps:CodeSnippet" priority ="-2" name="CodeSnippet-pri2">
	</xsl:template>

	<!-- a lead code snippet output inside of a tabbed language select along with its siblings-->
	<xsl:template name="leadsnippet" match="key('code-snippet-use', 'lead-snippet')">
		<xsl:variable name="all" select="following-sibling::*[preceding-sibling::*[1][self::mtps:CodeSnippet]]"/>
		<xsl:variable name="last" select="$all[not(self::mtps:CodeSnippet)][1]"/>
		<xsl:variable name="lastpos">
			<xsl:for-each select="$all">
				<xsl:if test="generate-id()=generate-id($last)">
					<xsl:number count="*"/>
				</xsl:if>
			</xsl:for-each>
		</xsl:variable>
		<xsl:variable name="snippets" select=". | following-sibling::mtps:CodeSnippet[preceding-sibling::*[1][self::mtps:CodeSnippet]][position()!=$lastpos]"/>
		<xsl:choose>
			<xsl:when test="not($all[@Language=following-sibling::mtps:CodeSnippet/@Language])">
				<!-- render code snippet group -->
				<xsl:if test="@Language!='other'">
					<xsl:call-template name="renderSnippet">
						<xsl:with-param name="snippetCount" select="count($snippets)"/>
						<xsl:with-param name="snippets" select="$snippets"/>
						<xsl:with-param name="showLanTabs" select="true()" />
						<xsl:with-param name="unrecognized" select="'false'" />
					</xsl:call-template>
				</xsl:if>
				<!-- non-standard code snippets -->
				<xsl:for-each select="$snippets">
					<xsl:variable name="lang" select="./@Language"/>
					<xsl:variable name="displang" select="./@DisplayLanguage"/>
					<xsl:variable name="uselangtemp">
						<xsl:if test="string-length($displang)>0">
							<xsl:value-of select="$displang"/>
						</xsl:if>
						<xsl:if test="string-length($displang)=0">
							<xsl:value-of select="$lang"/>
						</xsl:if>
					</xsl:variable>
					<xsl:variable name="loweredLang" select="translate($uselangtemp, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')" />
					<xsl:variable name ="showLanTabs">
						<xsl:if test="string-length($uselangtemp)=0">
							<xsl:value-of select="false()"/>
						</xsl:if>
						<xsl:if test="string-length($uselangtemp)>0">
							<xsl:value-of select="true()"/>
						</xsl:if>
					</xsl:variable>
					<xsl:variable name="unrecognized">
						<xsl:choose>
							<xsl:when test="starts-with($loweredLang,'visual basic') or $loweredLang='c#' or $loweredLang='visual c++' or $loweredLang='jscript'or $loweredLang='f#'">
								<xsl:text>false</xsl:text>
							</xsl:when>
							<xsl:otherwise>
								<xsl:text>true</xsl:text>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
					<xsl:if test="$unrecognized='true'">
						<xsl:call-template name="renderSnippet">
							<xsl:with-param name="snippetCount" select="1" />
							<xsl:with-param name="snippets" select="." />
							<xsl:with-param name="showLanTabs" select="$showLanTabs" />
							<xsl:with-param name="unrecognized" select="'true'" />
						</xsl:call-template>
					</xsl:if>
				</xsl:for-each>
			</xsl:when>
			<xsl:otherwise>
				<!-- render multiple groups standalone-->
				<xsl:for-each select=". | $all">
					<xsl:variable name="snippet_count">
						<xsl:number count="*"/>
					</xsl:variable>
					<xsl:if test="$lastpos = '' or $snippet_count &lt; $lastpos">
						<xsl:call-template name="standalonesnippet"/>
					</xsl:if>
				</xsl:for-each>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- a free standing snippet is just output as is-->
	<xsl:template name="standalonesnippet" match="key('code-snippet-use', 'free-standing-snippet')">
		<xsl:variable name="lang" select="./@Language"/>
		<xsl:variable name="displang" select="./@DisplayLanguage"/>
		<xsl:variable name="majorLang">
			<xsl:call-template name="isMajorLanguage">
				<xsl:with-param name="lang" select="$displang"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="uselangtemp">
			<xsl:if test="string-length($displang)>0">
				<xsl:value-of select="$displang"/>
			</xsl:if>
			<xsl:if test="string-length($displang)=0">
				<xsl:value-of select="$lang"/>
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="loweredLang" select="translate($uselangtemp, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')" />
		<xsl:variable name ="showLanTabs">
			<xsl:if test="string-length($uselangtemp)=0">
				<xsl:value-of select="false()"/>
			</xsl:if>
			<xsl:if test="string-length($uselangtemp)>0">
				<xsl:value-of select="true()"/>
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="unrecognized">
			<xsl:choose>
				<xsl:when test="$majorLang='true'">
					<xsl:value-of select="'false'"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="'true'"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:call-template name="renderSnippet">
			<xsl:with-param name="snippetCount" select="1" />
			<xsl:with-param name="snippets" select="." />
			<xsl:with-param name="showLanTabs" select="$showLanTabs" />
			<xsl:with-param name="unrecognized" select="$unrecognized" />
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="renderSnippet">
		<xsl:param name="snippetCount" />
		<xsl:param name="snippets"/>
		<xsl:param name="showLanTabs" select="true()" />
		<xsl:param name="unrecognized" select="'false'" />
		<xsl:variable name="owner-id" select="generate-id()"/>
		<xsl:variable name="tabflagtemp" select="''" />
		<!-- this flag shows the visual basic and j# language tabs, the value might contains: usage, declaration, jsharp-->
		<xsl:variable name="tabflag">
			<xsl:for-each select="$snippets">
				<xsl:variable name="lang" select="./@Language"/>
				<xsl:variable name="displang" select="./@DisplayLanguage"/>
				<!-- the var uselang is the real language text that we show in output pages
        if the 'display language' not exist, we use 'language' property
        -->
				<xsl:variable name="uselang">
					<xsl:if test="string-length($displang)>0">
						<xsl:value-of select="$displang"/>
					</xsl:if>
					<xsl:if test="string-length($displang)=0">
						<xsl:value-of select="$lang"/>
					</xsl:if>
				</xsl:variable>
				<!-- we store the lowered text of language so that it is easier to find languages that we specified-->
				<xsl:variable name="loweredLang" select="translate($lang, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')" />
				<xsl:if test="contains($loweredLang, 'basic')">
					<xsl:if test="contains($loweredLang, 'declaration')">
						<xsl:value-of select="concat($tabflagtemp, 'Dec:', position(), ':declaration;')"/>
					</xsl:if>
					<xsl:if test="contains($loweredLang, 'usage')">
						<xsl:value-of select="concat($tabflagtemp, 'Usa:', position(), ':usage;')"/>
					</xsl:if>
				</xsl:if>
				<xsl:if test="contains(translate($lang, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jsharp')">
					<xsl:value-of select="concat($tabflagtemp, 'JS:', position(), ':jsharp;')"/>
				</xsl:if>
			</xsl:for-each>
		</xsl:variable>
		<!-- the index of visual basic declaration tab in a code snippet-->
		<xsl:variable name="declarationIndex">
			<xsl:if test="contains($tabflag, 'declaration')">
				<xsl:value-of select="substring-after(substring-before($tabflag, ':declaration'), 'Dec:')"/>
			</xsl:if>
		</xsl:variable>
		<!-- the index of visual basic usage tab in a code snippet-->
		<xsl:variable name="usageIndex">
			<xsl:if test="contains($tabflag, 'usage')">
				<xsl:value-of select="substring-after(substring-before($tabflag, ':usage'), 'Usa:')"/>
			</xsl:if>
		</xsl:variable>
		<!-- the index of jsharp tab in a code snippet-->
		<xsl:variable name="jsharpIndex">
			<xsl:if test="contains($tabflag, 'jsharp')">
				<xsl:value-of select="substring-after(substring-before($tabflag, ':jsharp'), 'JS:')"/>
			</xsl:if>
		</xsl:variable>
		<!-- the text of combined visual basic if there are both vb declaration and vb usage tab-->
		<xsl:variable name="needToCombineVBTab">
			<xsl:choose>
				<xsl:when test="string-length($declarationIndex)>0 and string-length($usageIndex)>0">
					<xsl:value-of select="true()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="false()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:element name="div" namespace="{$xhtml}">
			<xsl:attribute name="id">
				<xsl:value-of select="$owner-id"/>
			</xsl:attribute>
			<xsl:attribute name="class">codeSnippetContainer</xsl:attribute>

			<xsl:element name="div" namespace="{$xhtml}">
				<xsl:attribute name="class">codeSnippetContainerTabs</xsl:attribute>
				<xsl:attribute name="id">
					<xsl:value-of select="concat($owner-id, '_tabs')"/>
				</xsl:attribute>
				<xsl:choose>
					<xsl:when test="$unrecognized='true'">
						<xsl:variable name="lang" select="./@Language"/>
						<xsl:variable name="displang" select="./@DisplayLanguage"/>
						<xsl:variable name="uselang">
							<xsl:if test="string-length($displang)>0">
								<xsl:value-of select="$displang"/>
							</xsl:if>
							<xsl:if test="string-length($displang)=0">
								<xsl:value-of select="$lang"/>
							</xsl:if>
						</xsl:variable>
						<xsl:variable name="langLower" select="translate($lang, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')"/>
						<xsl:choose>
							<xsl:when test="$langLower and not(contains($langLower, 'other')) and not(contains($langLower, 'none'))">
								<xsl:element name="div" namespace="{$xhtml}">
									<xsl:attribute name="id">
										<xsl:value-of select="concat($owner-id, '_tab1')"/>
									</xsl:attribute>
									<xsl:attribute name="class">codeSnippetContainerTabSingle</xsl:attribute>
									<xsl:attribute name="EnableCopyCode">
										<xsl:value-of select="$snippets[1]/@EnableCopyCode"/>
									</xsl:attribute>
									<xsl:element name="a">
										<xsl:value-of select="$uselang"/>
									</xsl:element>
								</xsl:element>
							</xsl:when>
							<xsl:otherwise>
								<xsl:text> </xsl:text>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<xsl:if test="$showLanTabs">
							<xsl:for-each select="msxsl:node-set($uniqueLangTabsSet)/value">
								<xsl:variable name="uselang" select="."/>
								<xsl:variable name="uniqueLangIndex" select="position()" />
								<xsl:variable name="leadingVBTabIndex">
									<xsl:if test="$declarationIndex&lt;$usageIndex">
										<xsl:value-of select="$declarationIndex"/>
									</xsl:if>
									<xsl:if test="$declarationIndex&gt;$usageIndex">
										<xsl:value-of select="$usageIndex"/>
									</xsl:if>
								</xsl:variable>
								<xsl:variable name="followingVBTabIndex">
									<xsl:if test="$declarationIndex=$leadingVBTabIndex">
										<xsl:value-of select="$usageIndex"/>
									</xsl:if>
									<xsl:if test="$usageIndex=$leadingVBTabIndex">
										<xsl:value-of select="$declarationIndex"/>
									</xsl:if>
								</xsl:variable>
								<xsl:variable name="pos">
									<xsl:variable name="postemp">
										<xsl:for-each select="$snippets">
											<xsl:variable name="lang" select="./@Language"/>
											<xsl:variable name="displang" select="./@DisplayLanguage"/>
											<xsl:variable name="uselangtemp">
												<xsl:if test="string-length($displang)>0">
													<xsl:value-of select="$displang"/>
												</xsl:if>
												<xsl:if test="string-length($displang)=0">
													<xsl:value-of select="$lang"/>
												</xsl:if>
											</xsl:variable>
											<xsl:if test="contains($uselangtemp, $uselang)">
												<xsl:choose>
													<xsl:when test="($needToCombineVBTab='true') and (position()=$leadingVBTabIndex)">
														<xsl:value-of select="position()"/>
													</xsl:when>
													<xsl:when test="($needToCombineVBTab='true') and (position()=$followingVBTabIndex)" />
													<xsl:otherwise>
														<xsl:value-of select="position()"/>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:if>
										</xsl:for-each>
									</xsl:variable>
									<xsl:if test="string-length($postemp)>0">
										<xsl:value-of select="$postemp"/>
									</xsl:if>
									<xsl:if test="string-length($postemp)=0">
										<xsl:value-of select="$uniqueLangIndex + $uniqueLangTabsSetCount"/>
									</xsl:if>
								</xsl:variable>
								<xsl:variable name="majorLang">
									<xsl:call-template name="isMajorLanguage">
										<xsl:with-param name="lang" select="$snippets[position()=$pos]/@DisplayLanguage"/>
									</xsl:call-template>
								</xsl:variable>
								<xsl:element name="div" namespace="{$xhtml}">
									<xsl:attribute name="id">
										<xsl:value-of select="concat($owner-id, '_tab', $uniqueLangIndex)"/>
									</xsl:attribute>
									<xsl:attribute name="class">
										<xsl:choose>
											<xsl:when test="($uniqueLangIndex=1 or $uniqueLangTabsSetCount=1) and $pos&lt;=$uniqueLangTabsSetCount">
												<xsl:text>codeSnippetContainerTabActive</xsl:text>
											</xsl:when>
											<xsl:otherwise>
												<xsl:choose>
													<xsl:when test="($snippets[$pos]/text() or $snippets[$pos]/child::node()/text()) and $majorLang='true'">
														<xsl:text>codeSnippetContainerTab</xsl:text>
													</xsl:when>
													<xsl:otherwise>
														<xsl:if test="$uniqueLangIndex=1">
															<xsl:text>codeSnippetContainerTabPhantom</xsl:text>
														</xsl:if>
														<xsl:if test="$uniqueLangIndex!=1">
															<xsl:text>codeSnippetContainerTabPhantom</xsl:text>
														</xsl:if>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:otherwise>
										</xsl:choose>
									</xsl:attribute>
									<xsl:attribute name="EnableCopyCode">
										<xsl:value-of select="$snippets[1]/@EnableCopyCode"/>
									</xsl:attribute>
									<xsl:if test="not($snippets[$pos]/text() or $snippets[$pos]/child::node()/text()) or not($majorLang='true')">
										<xsl:attribute name="disabled">
											<xsl:text>true</xsl:text>
										</xsl:attribute>
									</xsl:if>
									<xsl:element name="a" namespace="{$xhtml}">
										<xsl:if test="($snippets[$pos]/text() or $snippets[$pos]/child::node()/text()) and $majorLang='true'">
											<!-- EFW - Use onclick rather than href or HV 2.0 messes up the link -->
											<xsl:attribute name="href">
												<xsl:text>#</xsl:text>
											</xsl:attribute>
											<xsl:attribute name="onclick">
												<xsl:text>javascript:ChangeTab('</xsl:text>
												<xsl:value-of select="$owner-id"/>
												<xsl:text>','</xsl:text>
												<xsl:value-of select="$uselang"/>
												<xsl:text>','</xsl:text>
												<xsl:value-of select="$uniqueLangIndex" />
												<xsl:text>','</xsl:text>
												<xsl:value-of select="$uniqueLangTabsSetCount" />
												<xsl:text>');return false;</xsl:text>
											</xsl:attribute>
										</xsl:if>
										<xsl:choose>
											<xsl:when test="$uselang='Visual Basic'">
												<xsl:value-of select="'VB'"/>
											</xsl:when>
											<xsl:when test="$uselang='Visual C++'">
												<xsl:value-of select="'C++'"/>
											</xsl:when>
											<xsl:otherwise>
												<xsl:value-of select="$uselang"/>
											</xsl:otherwise>
										</xsl:choose>
									</xsl:element>

								</xsl:element>
							</xsl:for-each>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>

			<xsl:element name="div" namespace="{$xhtml}">
				<xsl:attribute name="id">
					<xsl:value-of select="concat($owner-id, '_codecollection')"/>
				</xsl:attribute>
				<xsl:attribute name="class">codeSnippetContainerCodeContainer</xsl:attribute>
				<xsl:element name="div" namespace="{$xhtml}">
					<xsl:attribute name="class">codeSnippetToolBar</xsl:attribute>

					<xsl:element name="div" namespace="{$xhtml}">
						<xsl:attribute name="class">codeSnippetToolBarText</xsl:attribute>
						<xsl:if test="$unrecognized='true'">
							<xsl:call-template name="code-snippet-menu">
								<xsl:with-param name="id" select="$owner-id"/>
								<xsl:with-param name="snippetCount" select="$uniqueLangTabsSetCount"/>
								<xsl:with-param name="enableCopyCode" select="'true'"/>
								<xsl:with-param name="showPrint" select="'true'"/>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="$unrecognized='false'">
							<xsl:call-template name="code-snippet-menu">
								<xsl:with-param name="id" select="$owner-id"/>
								<xsl:with-param name="snippetCount" select="$uniqueLangTabsSetCount"/>
								<xsl:with-param name="enableCopyCode" select="$snippets[1]/@EnableCopyCode"/>
							</xsl:call-template>
						</xsl:if>
					</xsl:element>
				</xsl:element>

				<xsl:choose>
					<xsl:when test="$unrecognized='true'">
						<xsl:variable name="lang" select="./@Language"/>
						<xsl:variable name="displang" select="./@DisplayLanguage"/>
						<xsl:variable name="uselang">
							<xsl:if test="string-length($displang)>0">
								<xsl:value-of select="$displang"/>
							</xsl:if>
							<xsl:if test="string-length($displang)=0">
								<xsl:value-of select="$lang"/>
							</xsl:if>
						</xsl:variable>
						<xsl:call-template name="snippet-wrapper">
							<xsl:with-param name="id" select="$owner-id"/>
							<xsl:with-param name="pos" select="1"/>
							<xsl:with-param name="snippets" select="$snippets"/>
							<xsl:with-param name="snippet" select="$snippets[1]"/>
							<xsl:with-param name="ContainsMarkup" select="$snippets[1]/@ContainsMarkup"/>
							<xsl:with-param name="lang" select="$uselang"/>
							<xsl:with-param name="unrecognized" select="$unrecognized"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:for-each select="msxsl:node-set($uniqueLangTabsSet)/value">
							<xsl:variable name="uselang" select="."/>
							<xsl:variable name="uniqueLangIndex" select="position()" />
							<xsl:variable name="leadingVBTabIndex">
								<xsl:if test="$declarationIndex&lt;$usageIndex">
									<xsl:value-of select="$declarationIndex"/>
								</xsl:if>
								<xsl:if test="$declarationIndex&gt;$usageIndex">
									<xsl:value-of select="$usageIndex"/>
								</xsl:if>
							</xsl:variable>
							<xsl:variable name="followingVBTabIndex">
								<xsl:if test="$declarationIndex=$leadingVBTabIndex">
									<xsl:value-of select="$usageIndex"/>
								</xsl:if>
								<xsl:if test="$usageIndex=$leadingVBTabIndex">
									<xsl:value-of select="$declarationIndex"/>
								</xsl:if>
							</xsl:variable>
							<!-- dupe #1 - refactor-->
							<xsl:variable name="pos">
								<xsl:variable name="postemp">
									<xsl:for-each select="$snippets">
										<xsl:variable name="lang" select="./@Language"/>
										<xsl:variable name="displang" select="./@DisplayLanguage"/>
										<xsl:variable name="uselangtemp">
											<xsl:if test="string-length($displang)>0">
												<xsl:value-of select="$displang"/>
											</xsl:if>
											<xsl:if test="string-length($displang)=0">
												<xsl:value-of select="$lang"/>
											</xsl:if>
										</xsl:variable>
										<xsl:if test="contains($uselangtemp, $uselang)">
											<xsl:choose>
												<xsl:when test="($needToCombineVBTab='true') and (position()=$leadingVBTabIndex)">
													<xsl:value-of select="position()"/>
												</xsl:when>
												<xsl:when test="($needToCombineVBTab='true') and (position()=$followingVBTabIndex)"/>
												<xsl:otherwise>
													<xsl:value-of select="position()"/>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:if>
									</xsl:for-each>
								</xsl:variable>
								<xsl:if test="string-length($postemp)>0">
									<xsl:value-of select="$postemp"/>
								</xsl:if>
								<xsl:if test="string-length($postemp)=0">
									<xsl:value-of select="$uniqueLangIndex + $uniqueLangTabsSetCount"/>
								</xsl:if>
							</xsl:variable>
							<xsl:variable name="lang" select="$snippets[position()=$pos]/@Language"/>
							<xsl:variable name="displang" select="$snippets[position()=$pos]/@DisplayLanguage"/>
							<xsl:call-template name="snippet-wrapper">
								<xsl:with-param name="id" select="$owner-id"/>
								<xsl:with-param name="pos" select="$pos"/>
								<xsl:with-param name="uniqueLangIndex" select="$uniqueLangIndex"/>
								<xsl:with-param name="snippets" select="$snippets"/>
								<xsl:with-param name="snippet" select="$snippets[position()=$pos]"/>
								<xsl:with-param name="ContainsMarkup" select="$snippets[position()=$pos]/@ContainsMarkup"/>
								<xsl:with-param name="lang" select="$lang"/>
								<xsl:with-param name="needToCombineVBTab" select="$needToCombineVBTab"/>
								<xsl:with-param name="declarationIndex" select="$declarationIndex"/>
								<xsl:with-param name="usageIndex" select="$usageIndex"/>
								<xsl:with-param name="unrecognized" select="$unrecognized"/>
							</xsl:call-template>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
		</xsl:element>

		<xsl:element name="script" namespace="{$xhtml}">
			<xsl:text>addSpecificTextLanguageTagSet('</xsl:text>
			<xsl:value-of select="$owner-id"/>
			<xsl:text>');</xsl:text>
		</xsl:element>
	</xsl:template>

	<xsl:template name="code-snippet-menu">
		<xsl:param name="id"/>
		<xsl:param name="snippetCount"/>
		<xsl:param name="print-id" select="concat($id, '_PrintText')"/>
		<xsl:param name="code-id" select="concat($id, '_code')"/>
		<xsl:param name="codeDiv-id" select="concat($id, '_code_Div')"/>
		<xsl:param name="plainDiv-id" select="concat($id, '_Plain_Div')"/>
		<xsl:param name="enableCopyCode" select="'false'" />
		<xsl:param name="showPrint" select="'true'" />

		<xsl:element name="a" namespace="{$xhtml}">
			<xsl:attribute name="id">
				<xsl:value-of select="concat($id, '_copycode')"/>
			</xsl:attribute>
			<xsl:attribute name="href">
				<xsl:text>#</xsl:text>
			</xsl:attribute>
			<xsl:attribute name="onclick">
				<xsl:text>javascript:CopyToClipboard('</xsl:text>
				<xsl:value-of select="$id"/>
				<xsl:text>','</xsl:text>
				<xsl:value-of select="$snippetCount"/>
				<xsl:text>')</xsl:text>
			</xsl:attribute>
			<xsl:attribute name="title">
				<xsl:value-of select="$CopyText"/>
			</xsl:attribute>
			<xsl:if test="$enableCopyCode='false'">
				<xsl:attribute name="style">
					<xsl:text>display: none</xsl:text>
				</xsl:attribute>
			</xsl:if>
			<xsl:value-of select="$CopyText"/>
		</xsl:element>

		<xsl:element name="a" namespace="{$xhtml}">
			<xsl:attribute name="id">
				<xsl:value-of select="$print-id"/>
			</xsl:attribute>
			<xsl:attribute name="class">OH_PrintText</xsl:attribute>
			<xsl:attribute name="href">
				<xsl:text>#</xsl:text>
			</xsl:attribute>
			<xsl:attribute name="onclick">
				<xsl:text>javascript:Print('</xsl:text>
				<xsl:value-of select="$id"/>
				<xsl:text>','</xsl:text>
				<xsl:value-of select="$snippetCount"/>
				<xsl:text>')</xsl:text>
			</xsl:attribute>
			<xsl:if test="$showPrint='false'">
				<xsl:attribute name="style">
					<xsl:text>display: none;</xsl:text>
				</xsl:attribute>
			</xsl:if>
			<xsl:attribute name="title">
				<xsl:text>Print</xsl:text>
			</xsl:attribute>
			<xsl:value-of select="$PrintText"/>
		</xsl:element>
	</xsl:template>

	<xsl:template name="snippet-wrapper">
		<xsl:param name="id" select="1" />
		<xsl:param name="pos" select="1" />
		<xsl:param name="uniqueLangIndex" select="1" />
		<xsl:param name="lang" />
		<xsl:param name="snippets" />
		<xsl:param name="needToCombineVBTab"/>
		<xsl:param name="snippet" select="node()"/>
		<xsl:param name="declarationIndex"/>
		<xsl:param name="usageIndex"/>
		<xsl:param name="jsharpIndex"/>
		<xsl:param name="unrecognized" select="'false'" />
		<xsl:param name="ContainsMarkup" select="'false'" />
		<xsl:variable name="leadingVBTabIndex">
			<xsl:if test="$declarationIndex&lt;$usageIndex">
				<xsl:value-of select="$declarationIndex"/>
			</xsl:if>
			<xsl:if test="$declarationIndex&gt;$usageIndex">
				<xsl:value-of select="$usageIndex"/>
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="followingVBTabIndex">
			<xsl:if test="$declarationIndex=$leadingVBTabIndex">
				<xsl:value-of select="$usageIndex"/>
			</xsl:if>
			<xsl:if test="$usageIndex=$leadingVBTabIndex">
				<xsl:value-of select="$declarationIndex"/>
			</xsl:if>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="$unrecognized='true'">
				<xsl:call-template name="renderCodeDiv">
					<xsl:with-param name="id" select="concat($id, '_code_Div1')"/>
					<xsl:with-param name="pos" select="$pos"/>
					<xsl:with-param name="lang" select="$lang"/>
					<xsl:with-param name="plainCode" select="'false'"/>
					<xsl:with-param name="snippetCode" select="$snippet"/>
					<xsl:with-param name="unrecognized" select="$unrecognized"/>
					<xsl:with-param name="ContainsMarkup" select="$ContainsMarkup" />
				</xsl:call-template>
				<xsl:call-template name="renderCodeDiv">
					<xsl:with-param name="id" select="concat($id, '_code_Plain_Div1')"/>
					<xsl:with-param name="pos" select="$pos"/>
					<xsl:with-param name="uniqueLangIndex" select="$uniqueLangIndex"/>
					<xsl:with-param name="lang" select="$lang"/>
					<xsl:with-param name="plainCode" select="'true'"/>
					<xsl:with-param name="snippetCode" select="$snippet"/>
					<xsl:with-param name="unrecognized" select="$unrecognized"/>
					<xsl:with-param name="ContainsMarkup" select="$ContainsMarkup" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="($needToCombineVBTab='true') and ($pos=$leadingVBTabIndex)">
						<xsl:variable name="quote">'</xsl:variable>
						<xsl:variable name="leadingVBTabCode_c">
							<xsl:copy-of select="$snippets[number($leadingVBTabIndex)]" />
						</xsl:variable>
						<xsl:variable name="tet"  select="format-number($followingVBTabIndex, '#')"/>
						<xsl:variable name="followingVBTabCode_c">
							<xsl:copy-of select="$snippets[number($followingVBTabIndex)]" />
						</xsl:variable>
						<xsl:call-template name="renderCodeDiv">
							<xsl:with-param name="id" select="concat($id, '_code_Div', $uniqueLangIndex)"/>
							<xsl:with-param name="pos" select="$pos"/>
							<xsl:with-param name="uniqueLangIndex" select="$uniqueLangIndex"/>
							<xsl:with-param name="lang" select="$lang"/>
							<xsl:with-param name="snippetCode" select="concat($TextOfVBDeclaration, '&#xa;&#xa;',$snippets[number($leadingVBTabIndex)], '&#xa;&#xa;',$TextOfVBUsage, '&#xa;&#xa;', $snippets[number($followingVBTabIndex)])"/>
							<xsl:with-param name="ContainsMarkup" select="$ContainsMarkup" />
						</xsl:call-template>
						<xsl:call-template name="renderCodeDiv">
							<xsl:with-param name="id" select="concat($id, '_code_Plain_Div', $uniqueLangIndex)"/>
							<xsl:with-param name="pos" select="$pos"/>
							<xsl:with-param name="uniqueLangIndex" select="$uniqueLangIndex"/>
							<xsl:with-param name="lang" select="$lang"/>
							<xsl:with-param name="plainCode" select="'true'"/>
							<xsl:with-param name="snippetCode" select="concat($TextOfVBDeclaration, '&#xa;&#xa;',$leadingVBTabCode_c, '&#xa;&#xa;',$TextOfVBUsage, '&#xa;&#xa;', $followingVBTabCode_c)"/>
							<xsl:with-param name="ContainsMarkup" select="$ContainsMarkup" />
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="($needToCombineVBTab='true') and ($pos=$followingVBTabIndex)"/>
					<xsl:otherwise>
						<xsl:call-template name="renderCodeDiv">
							<xsl:with-param name="id" select="concat($id, '_code_Div', $uniqueLangIndex)"/>
							<xsl:with-param name="pos" select="$pos"/>
							<xsl:with-param name="uniqueLangIndex" select="$uniqueLangIndex"/>
							<xsl:with-param name="lang" select="$lang"/>
							<xsl:with-param name="plainCode" select="'false'"/>
							<xsl:with-param name="snippetCode" select="$snippet"/>
							<xsl:with-param name="ContainsMarkup" select="$ContainsMarkup" />
						</xsl:call-template>
						<xsl:call-template name="renderCodeDiv">
							<xsl:with-param name="id" select="concat($id, '_code_Plain_Div', $uniqueLangIndex)"/>
							<xsl:with-param name="pos" select="$pos"/>
							<xsl:with-param name="uniqueLangIndex" select="$uniqueLangIndex"/>
							<xsl:with-param name="lang" select="$lang"/>
							<xsl:with-param name="plainCode" select="'true'"/>
							<xsl:with-param name="snippetCode" select="$snippet"/>
							<xsl:with-param name="ContainsMarkup" select="$ContainsMarkup" />
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="renderCodeDiv">
		<xsl:param name="id"/>
		<xsl:param name="pos" />
		<xsl:param name="uniqueLangIndex" select="1" />
		<xsl:param name="lang" />
		<xsl:param name="plainCode" select="'false'" />
		<xsl:param name="snippetCode" />
		<xsl:param name="unrecognized" select="'false'" />
		<xsl:param name="ContainsMarkup"  />
		<xsl:element name="div" namespace="{$xhtml}">
			<xsl:attribute name="id">
				<xsl:value-of select="$id"/>
			</xsl:attribute>
			<xsl:attribute name="class">codeSnippetContainerCode</xsl:attribute>
			<xsl:attribute name="style">
				<xsl:if test="$plainCode='true'">
					<xsl:text>display: none</xsl:text>
				</xsl:if>
				<xsl:if test="$plainCode='false'">
					<xsl:if test="$unrecognized='true'">
						<xsl:text>display: block</xsl:text>
					</xsl:if>
					<xsl:if test="$unrecognized='true'">
						<xsl:if test="not($uniqueLangIndex=1)">
							<xsl:text>display: none</xsl:text>
						</xsl:if>
						<xsl:if test="$uniqueLangIndex=1">
							<xsl:text>display: block</xsl:text>
						</xsl:if>
					</xsl:if>
				</xsl:if>
			</xsl:attribute>
			<xsl:element name="pre" namespace="{$xhtml}">
				<xsl:choose>
					<xsl:when test="$ContainsMarkup='true'">
						<xsl:choose>
							<xsl:when test="$plainCode='true'">
								<xsl:value-of select="cs:ConvertWhiteSpace(cs:plainCode($snippetCode))" disable-output-escaping="yes"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:copy-of select="cs:ConvertWhiteSpace($snippetCode)" />
							</xsl:otherwise>
						</xsl:choose>

					</xsl:when>
					<xsl:otherwise>
						<xsl:choose>
							<xsl:when test="$plainCode='true'">
								<xsl:value-of select="cs:ConvertWhiteSpace(cs:plainCode($snippetCode))" disable-output-escaping="yes"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="cs:ConvertWhiteSpace(cs:test($snippetCode, $lang, 'en-us'))" disable-output-escaping="yes"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:otherwise>
				</xsl:choose>

			</xsl:element>
		</xsl:element>
	</xsl:template>

	<xsl:template name="isMajorLanguage">
		<xsl:param name="lang"/>
		<xsl:for-each select="msxsl:node-set($uniqueLangTabsSet)/value">
			<xsl:choose>
				<!-- True if it is an exact match -->
				<xsl:when test="$lang=.">
					<xsl:value-of select="'true'"/>
				</xsl:when>
				<!-- See if it contains the language -->
				<xsl:when test="contains($lang,.)">
					<xsl:variable name="loweredLang"
												select="translate($lang, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')" />
					<!-- See if it's a "Declaration" or "Usage" variant of the language.  If so, it's a match. -->
					<xsl:if test="contains($loweredLang,'declaration') or contains($loweredLang,'usage')">
						<xsl:value-of select="'true'"/>
					</xsl:if>
				</xsl:when>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

	<msxsl:script language="C#" implements-prefix="cs">
		<msxsl:using namespace="System.Collections.Generic" />
		<msxsl:using namespace="System.ComponentModel" />
		<msxsl:using namespace="System.IO" />
		<msxsl:using namespace="System.Globalization" />
		<msxsl:assembly name="System.Web" />
		<msxsl:using namespace="System.Web" />
		<![CDATA[
    public string GetUniqueLangTabs(string input)
    {
        // delete J# language
            input = input.Replace("J#;", "").Replace("JSharp;", "");

            string useVBLang = "Visual Basic;";
            if (input.IndexOf("VisualBasic;") != -1) useVBLang = "VisualBasic;";
            string useCSharpLang = "C#;";
            if (input.IndexOf("CSharp;") != -1) useVBLang = "CSharp;";
            string useCPPLang = "Visual C++;";
            if (input.IndexOf("ManagedCPlusPlus;") != -1) useVBLang = "ManagedCPlusPlus;";
            string useJSLang = "JScript;";
            string useFSharpLang = "F#;";
            if (input.IndexOf("FSharp;") != -1) useFSharpLang = "FSharp;";

            bool needToCombine = false;
            // combine declearation and usage into single vb tab if it exists   
            if ((input.IndexOf("Declaration") != -1) && (input.IndexOf("Usage") != -1))
                needToCombine = true;
            if ((input.IndexOf(useVBLang) != -1) &&
                ((input.IndexOf("Declaration") != -1) || (input.IndexOf("Usage") != -1)))
                needToCombine = true;


            string output = "";
            string collection = "";
            foreach (string temp in input.Split(';'))
            {
                if (collection.IndexOf(temp) == -1)
                {
                    string temp1 = temp;
                    if ((needToCombine) &&
                        ((temp1.IndexOf("Declaration") != -1) || (temp1.IndexOf("Usage") != -1)))
                    {
                        temp1 = useVBLang.Replace(";", "");
                    }
                    output += String.Format("{0};", temp1);
                    collection += temp1 + ";";
                }
            }

            if (needToCombine)
            {
                int firstVBTab = output.IndexOf(useVBLang);
                int secondVBTab = output.IndexOf(useVBLang, firstVBTab + 1);
                int last = secondVBTab + 13;
                if (last > output.Length) last = output.Length;
                output = output.Substring(0, secondVBTab) + output.Substring(last);
            }

            // order the lang tabs
            string orderedOutput = "";
            if (output.IndexOf(useVBLang) != -1)
            {
                orderedOutput += useVBLang;
                output = output.Replace(useVBLang, "");
            }
            if (output.IndexOf(useCSharpLang) != -1)
            {
                orderedOutput += useCSharpLang;
                output = output.Replace(useCSharpLang, "");
            }
            if (output.IndexOf(useCPPLang) != -1)
            {
                orderedOutput += useCPPLang;
                output = output.Replace(useCPPLang, "");
            }
            if (output.IndexOf(useJSLang) != -1)
            {
                orderedOutput += useJSLang;
                output = output.Replace(useJSLang, "");
            }
            if (output.IndexOf(useFSharpLang) != -1)
            {
                orderedOutput += useFSharpLang;
                output = output.Replace(useFSharpLang, "");
            }
            orderedOutput += output;
            return orderedOutput;
    }
      
    public string needToCombineVBTabs(string input)
    {
      // combine declearation and usage into single vb tab if it exists   
      if ((input.IndexOf("Declaration") != -1) && (input.IndexOf("Usage") != -1))
                return "true";
      if ((input.IndexOf("Visual Basic;") != -1) || (input.IndexOf("VisualBasic;") != -1))
                return "true";
      else return "false";
    }
    public string plainCode(string result)
    {
      return System.Web.HttpUtility.HtmlEncode(result);
    }
	  public string ConvertWhiteSpace(string result)
    {
      MatchCollection matches = Regex.Matches(result, "</span>(.*?)<span[^>]*>");
      
      for (int i = 0; i < matches.Count; i++)
      {
         Match match = Regex.Match(matches[i].Value,"<span[^>]*>");
         string endTag = match.Value;
         string textNode= matches[i].Groups[1].Value;
         
		     if(textNode != "")
		     {
                 textNode = textNode.Replace(" ", "&#160;");
                 result = result.Replace(matches[i].Value, "</span>" + textNode + endTag);
		     }
			
      }
	  
	    return result;
      
    }
    public string test(string result, string language, string locale)
    {
      CodeSnippet cs = new CodeSnippet();
      return cs.LiteralControlColorizedText(result, language, locale);
    }
    public string ColorizedText(string result, string language)
    {
      CodeSnippet cs = new CodeSnippet();
      return cs.GetColorizedText(result, language);
    }
    
    class CodeSnippet
    {
        public delegate string FixFunction(string source);

        /// <summary>
        /// End font tag
        /// </summary>
        public const string TAGEFONT = "</span>";

        /// <summary>
        /// Blue font tag
        /// </summary>
        public const string TAGFNTBLUE = "<span style=\"color:blue\">";

        /// <summary>
        /// Green font tag
        /// </summary>
        public const string TAGFNTGRN = "<span style=\"color:green;\">";

        /// <summary>
        /// Maroon font tag
        /// </summary>
        public const string TAGFNTMRN = "<span style=\"color:maroon\">";

        /// <summary>
        /// Red font tag
        /// </summary>
        public const string TAGFNTRED = "<span style=\"color:red\">";

        /// <summary>
        /// VB comment
        /// </summary>
        public const string VBCOMMENTPLACEHOLDER = "'commentText";

        /// <summary>
        /// C# keywords
        /// </summary>
        public string csharpSearchExpr;

        /// <summary>
        /// J# keywords
        /// </summary>
        public string jsharpSearchExpr;

        /// <summary>
        /// F# keywords
        /// </summary>
        public string fsharpSearchExpr;

        /// <summary>
        /// Visual Basic keywords
        /// </summary>
        public string vbSearchExpr;

        public CodeSnippet()
        {
            InitStringArrays();
        }

        /// <summary>
        /// Get all the literal control colorized text
        /// </summary>
        /// <param name="result">result string builder</param>
        public string LiteralControlColorizedText(string result, string language, string strlocale)
        {
            CultureInfo locale = new CultureInfo(strlocale);
            ///return this.GetColorizedText(System.Web.HttpUtility.HtmlDecode(result), language.ToLower(locale));
            return this.GetColorizedText(result, language.ToLower(locale));
        }

        /// <summary>
        /// Gets the lang from line.
        /// </summary>
        /// <param name="sourceLine">The source line.</param>
        /// <param name="defLang">The default language.</param>
        /// <returns>language</returns>
        public static string GetLangFromLine(string sourceLine, string defLang)
        {
            if (sourceLine == null)
            {
                return defLang;
            }

            Match langMatch = Regex.Match(sourceLine, "(?i)<%@\\s*Page\\s*.*Language\\s*=\\s*\"(?<lang>[^\"]+)\"");
            if (langMatch.Success)
            {
                return langMatch.Groups["lang"].ToString();
            }

            langMatch = Regex.Match(sourceLine, "(?i)(?=.*runat\\s*=\\s*\"?server\"?)<script.*language\\s*=\\s*\"(?<lang>[^\"]+)\".*>");
            if (langMatch.Success)
            {
                return langMatch.Groups["lang"].ToString();
            }

            langMatch = Regex.Match(sourceLine, "(?i)<%@\\s*WebService\\s*.*Language\\s*=\\s*\"?(?<lang>[^\"]+)\"?");
            if (langMatch.Success)
            {
                return langMatch.Groups["lang"].ToString();
            }

            return defLang;
        }

        /// <summary>
        /// Determines whether the specified source is a script block tag end.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// 	<c>true</c> if the specified source is a script block tag end; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsScriptBlockTagEnd(String source)
        {
            if (Regex.IsMatch(source, "</script.*>"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified source is a script block tag start.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// 	<c>true</c> if the specified source is a script block tag start; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsScriptBlockTagStart(String source)
        {
            if (Regex.IsMatch(source, "<script.*runat=\"?server\"?.*>"))
            {
                return true;
            }

            if (Regex.IsMatch(source, "(?i)<%@\\s*WebService"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Colorizes the quoted text.
        /// </summary>
        /// <param name="sourceLine">The source line.</param>
        /// <returns>colorized text</returns>
        public string ColorizeQuotedText(string sourceLine)
        {
            List<String> quotedText = new List<string>();

            MatchCollection matches = Regex.Matches(sourceLine, "(?i)(?x)&quot;(?<quotedText>.*?)&quot;", RegexOptions.Singleline);

            int quoteCount = 0;
            string quoteText = "\"**quotedText{0}\"";

            foreach (Match m in matches)
            {
                if (m.Success && !quotedText.Contains(m.Value))
                {
                    quotedText.Add(m.Value);
                }
            }

            foreach (string s in quotedText)
            {
                quoteCount++;
                string replacementQuote = string.Format(quoteText, quoteCount);
                sourceLine = sourceLine.Replace(s, replacementQuote);
            }

            quoteCount = 0;

            foreach (string s in quotedText)
            {
                quoteCount++;
                string replacementQuote = string.Format(quoteText, quoteCount);
                sourceLine = sourceLine.Replace(replacementQuote, TAGFNTMRN + s + TAGEFONT);
            }

            return sourceLine;
        }

        /// <summary>
        /// Default case source fix.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="sourceLine">The source line.</param>
        /// <param name="lang">The lang.</param>
        /// <param name="scriptBlock">if set to <c>true</c> [script block].</param>
        /// <param name="multiLine">if set to <c>true</c> [multi line].</param>
        public void DefaultCaseSourceFix(StringBuilder output, String sourceLine, String lang, ref bool scriptBlock, ref bool multiLine)
        {
            if (IsScriptBlockTagStart(sourceLine))
            {
                output.Append(this.FixAspxLine(sourceLine));
                scriptBlock = true;
            }
            else
            {
                if (IsScriptBlockTagEnd(sourceLine))
                {
                    output.Append(this.FixAspxLine(sourceLine));
                    scriptBlock = false;
                }
                else
                {
                    if (this.IsMultiLineTagStart(sourceLine) && !multiLine)
                    {
                        multiLine = true;
                        output.Append("<span style=\"color:blue;\"><b>" + System.Web.HttpUtility.HtmlEncode(sourceLine));
                    }
                    else
                    {
                        if (this.IsMultiLineTagEnd(sourceLine) && multiLine)
                        {
                            multiLine = false;
                            output.Append(System.Web.HttpUtility.HtmlEncode(sourceLine) + "</b></span>");
                        }
                        else
                        {
                            if (multiLine)
                            {
                                output.Append(System.Web.HttpUtility.HtmlEncode(sourceLine));
                            }
                            else
                            {
                                if (scriptBlock == true)
                                {
                                    if (String.Compare(lang, "c#", StringComparison.Ordinal) == 0)
                                    {
                                        output.Append(this.FixCSLine(sourceLine));
                                    }
                                    else if (String.Compare(lang, "vb", StringComparison.Ordinal) == 0)
                                    {
                                        output.Append(this.FixVBLine(sourceLine));
                                    }
                                    else if (String.Compare(lang, "jscript", StringComparison.Ordinal) == 0 || String.Compare(lang, "javascript", StringComparison.Ordinal) == 0)
                                    {
                                        output.Append(this.FixJavaScriptLine(sourceLine));
                                    }
                                    else if (String.Compare(lang, "f#", StringComparison.Ordinal) == 0)
                                    {
                                        output.Append(this.FixFSLine(sourceLine));
                                    }
                                }
                                else
                                {
                                    output.Append(this.FixAspxLine(sourceLine));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fixes ASPX line.
        /// </summary>
        /// <param name="sourceLine">The source line.</param>
        /// <returns>Colorized text</returns>
        public string FixAspxLine(string sourceLine)
        {
            string searchExpr; // search string
            string replaceExpr; // replace string

            if ((sourceLine == null) || (sourceLine.Length == 0))
            {
                return sourceLine;
            }

            // Search for \t and replace it with 4 spaces.
            sourceLine = Regex.Replace(sourceLine, "(?i)(\\t)", "    ");
            sourceLine = System.Web.HttpUtility.HtmlEncode(sourceLine);

            // Single line comment or #include references.
            searchExpr = "(?i)(?<a>(^.*))(?<b>(&lt;!--))(?<c>(.*))(?<d>(--&gt;))(?<e>(.*))";
            replaceExpr = "${a}" + TAGFNTGRN + "${b}${c}${d}" + TAGEFONT + "${e}";

            if (Regex.IsMatch(sourceLine, searchExpr))
            {
                return Regex.Replace(sourceLine, searchExpr, replaceExpr);
            }

            // Colorize <%@ <type>
            searchExpr = "(?i)" + "(?<a>(&lt;%@))" + "(?<b>(.*))" + "(?<c>(%&gt;))";
            replaceExpr = "<span style=\"color:blue;\"><b>${a}${b}${c}</b></span>";

            if (Regex.IsMatch(sourceLine, searchExpr))
            {
                sourceLine = Regex.Replace(sourceLine, searchExpr, replaceExpr);
            }

            // Colorize <%# <type>
            searchExpr = "(?i)" + "(?<a>(&lt;%#))" + "(?<b>(.*))" + "(?<c>(%&gt;))";
            replaceExpr = "${a}" + "<span style=\"color:red;\"><b>" + "${b}" + "</b></span>" + "${c}";

            if (Regex.IsMatch(sourceLine, searchExpr))
            {
                sourceLine = Regex.Replace(sourceLine, searchExpr, replaceExpr);
            }
            
            // Colorize tag <type>
            searchExpr = "(?i)" + "(?<a>(&lt;)(?!%)(?!/?asp:)(?!/?template)(?!/?property)(?!/?ibuyspy:)(/|!)?)" + "(?<b>[^;\\s&]+)" + "(?<c>(\\s|&gt;|\\Z))";
            replaceExpr = "${a}" + TAGFNTMRN + "${b}" + TAGEFONT + "${c}";

            if (Regex.IsMatch(sourceLine, searchExpr))
            {
                sourceLine = Regex.Replace(sourceLine, searchExpr, replaceExpr);
            }

            // Colorize asp:|template for runat=server tags <type>
            searchExpr = "(?i)(?<a>&lt;/?)(?<b>(asp:|template|property|IBuySpy:).*)(?<c>&gt;)?";
            replaceExpr = "${a}<span style=\"color:blue;\"><b>${b}</b></span>${c}";

            if (Regex.IsMatch(sourceLine, searchExpr))
            {
                sourceLine = Regex.Replace(sourceLine, searchExpr, replaceExpr);
            }

            // colorize begin of tag char(s) "<","</","<%"
            searchExpr = "(?i)(?<a>(&lt;)(/|!|%)?)";
            replaceExpr = TAGFNTBLUE + "${a}" + TAGEFONT;

            if (Regex.IsMatch(sourceLine, searchExpr))
            {
                sourceLine = Regex.Replace(sourceLine, searchExpr, replaceExpr);
            }

            // Colorize end of tag char(s) ">","/>"
            searchExpr = "(?i)(?<a>(/|%)?(&gt;))";
            replaceExpr = TAGFNTBLUE + "${a}" + TAGEFONT;

            if (Regex.IsMatch(sourceLine, searchExpr))
            {
                sourceLine = Regex.Replace(sourceLine, searchExpr, replaceExpr);
            }

            return sourceLine;
        }

        /// <summary>
        /// Fixes C# line.
        /// </summary>
        /// <param name="sourceLine">The source line.</param>
        /// <returns>Colorized text</returns>
        public string FixCSLine(string sourceLine)
        {
          return FixLine(sourceLine, this.csharpSearchExpr, true);
        }


        /// <summary>
        /// Fixes J# line.
        /// </summary>
        /// <param name="sourceLine">The source line.</param>
        /// <returns>Colorized text</returns>
        public string FixJSLine(string sourceLine)
        {
          return FixLine(sourceLine, this.jsharpSearchExpr, true);
        }

        /// <summary>
        /// Fixes F# line.
        /// </summary>
        /// <param name="sourceLine">The source line.</param>
        /// <returns>Colorized text</returns>
        public string FixFSLine(string sourceLine)
        {
          return FixLine(sourceLine, this.fsharpSearchExpr, false);
        }
        
        // prevColorizedSourceLine saves the previous colorized line
        string prevColorizedSourceLine = null;

        /// <summary>
        /// Fixes C#, J# or F# line.
        /// </summary>
        /// <param name="sourceLine">The source line.</param>
        /// <param name="regEx">The regular expression for this language.</param>
        /// <returns>Colorized text</returns>
        public string FixLine(string sourceLine,string regEx,bool blockComment)
        {
            if (sourceLine == null)
            {
                return null;
            }
            
            // SourceLine is empty when there is a line break, so adding xslt line break string with checking previous line break string to avoid introduce multiple line breaks.
            if(sourceLine == String.Empty && prevColorizedSourceLine != null && prevColorizedSourceLine.EndsWith("</span>") && !prevColorizedSourceLine.EndsWith("&#xA;  "))
              return "&#xA;  ";
            
            sourceLine = Regex.Replace(sourceLine, "(?i)(\\t)", "    ");
            sourceLine = System.Web.HttpUtility.HtmlEncode(sourceLine);
            if ((sourceLine.Trim().StartsWith("//*")))
            {
                sourceLine = sourceLine.Replace("//*", "/*");
                sourceLine = sourceLine + "*/";
            }
            if ((sourceLine.Trim().StartsWith("///")) || (sourceLine.Trim().StartsWith("//")))
                sourceLine = Regex.Replace(sourceLine, "(?<comment>//.*$)", TAGFNTGRN + "${comment}" + TAGEFONT);
            else
            {
                List<String> quotedText = new List<string>();

                MatchCollection matches = Regex.Matches(sourceLine, "(?i)(?x)&quot;(?<quotedText>.*?)&quot;", RegexOptions.Singleline);

                int quoteCount = 0;
                string quoteText = "\"**quotedText{0}\"";

                foreach (Match m in matches)
                {
                    if (m.Success && !quotedText.Contains(m.Value))
                    {
                        quotedText.Add(m.Value);
                    }
                }

                foreach (string s in quotedText)
                {
                    quoteCount++;
                    string replacementQuote = string.Format(quoteText, quoteCount);
                    sourceLine = sourceLine.Replace(s, replacementQuote);
                }

                sourceLine = Regex.Replace(sourceLine, regEx, TAGFNTBLUE + "${keyword}" + TAGEFONT);

                quoteCount = 0;

                foreach (string s in quotedText)
                {
                    quoteCount++;
                    string replacementQuote = string.Format(quoteText, quoteCount);
                    sourceLine = sourceLine.Replace(replacementQuote, TAGFNTMRN + s + TAGEFONT);
                }
                
                sourceLine = this.ColorizeQuotedText(sourceLine);
            }
            
            sourceLine = sourceLine.Replace("/*",  TAGFNTGRN + System.Web.HttpUtility.HtmlEncode("/*") );
            sourceLine = sourceLine.Replace("*/", System.Web.HttpUtility.HtmlEncode("*/") + TAGEFONT);
            
            prevColorizedSourceLine = sourceLine;

            return sourceLine;
        }

                /// <summary>
        /// Fixes JavaScript line.
        /// </summary>
        /// <param name="sourceLine">The source line.</param>
        /// <returns>Colorized text</returns>
        public string FixJavaScriptLine(string sourceLine)
        {
            if (sourceLine == null)
            {
                return null;
            }

            sourceLine = Regex.Replace(sourceLine, "(?i)(\\t)", "    ");
            sourceLine = System.Web.HttpUtility.HtmlEncode(sourceLine);

            sourceLine = Regex.Replace(sourceLine, this.jsharpSearchExpr, TAGFNTBLUE + "${keyword}" + TAGEFONT);
            sourceLine = Regex.Replace(sourceLine, "(?<comment>//.*$)", TAGFNTGRN + "${comment}" + TAGEFONT);

            return sourceLine;
        }

        /// <summary>
        /// Fixes Visual Basic line.
        /// </summary>
        /// <param name="sourceLine">The source line.</param>
        /// <returns>Colorized text</returns>
        public string FixVBLine(string sourceLine)
        {
            if (sourceLine == null)
            {
                return null;
            }

            sourceLine = Regex.Replace(sourceLine, "(?i)(\\t)", "    ");
            sourceLine = System.Web.HttpUtility.HtmlEncode(sourceLine);

            string tmp = sourceLine;
            tmp = tmp.Replace("\\&quot;", "");
            Match quotText = Regex.Match(tmp, "(?<quot>&quot;.*?&quot;)");
            while (quotText.Success)
            {
                tmp = tmp.Replace(quotText.Value, "");
                quotText = Regex.Match(tmp, "(?<quot>&quot;.*?&quot;)");
            }


            Match comment = Regex.Match(tmp, "(?<comment>'.*$)");
            string commentText = string.Empty;

            if (comment.Success)
            {
                commentText = comment.Value;
                sourceLine = sourceLine.Replace(commentText, VBCOMMENTPLACEHOLDER);
            }

            List<String> quotedText = new List<string>();

            MatchCollection matches = Regex.Matches(sourceLine, "(?i)(?x)&quot;(?<quotedText>.*?)&quot;", RegexOptions.Singleline);

            int quoteCount = 0;
            string quoteText = "\"**quotedText{0}\"";

            foreach (Match m in matches)
            {
                if (m.Success && !quotedText.Contains(m.Value))
                {
                    quotedText.Add(m.Value);
                }
            }

            foreach (string s in quotedText)
            {
                quoteCount++;
                string replacementQuote = string.Format(quoteText, quoteCount);
                sourceLine = sourceLine.Replace(s, replacementQuote);
            }

            sourceLine = Regex.Replace(sourceLine, this.vbSearchExpr, "${buffer}" + TAGFNTBLUE + "${keyword}" + TAGEFONT, RegexOptions.Singleline);

            quoteCount = 0;

            foreach (string s in quotedText)
            {
                quoteCount++;
                string replacementQuote = string.Format(quoteText, quoteCount);
                sourceLine = sourceLine.Replace(replacementQuote, TAGFNTMRN + s + TAGEFONT);
            }

            sourceLine = sourceLine.Replace(VBCOMMENTPLACEHOLDER, TAGFNTGRN + commentText + TAGEFONT);

            return sourceLine;
        }

        /// <summary>
        /// Colorizes input string based on input language
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="language">The language.</param>
        /// <returns>Colorized text</returns>
        public string GetColorizedText(string input, string language)
        {
            StringBuilder output = new StringBuilder(input.Length + (input.Length / 2));

            try
            {
                String sourceLine;
                StringReader sr = new StringReader(input);

                switch (language)
                {
                    case "c#":
                    case "csharp":
                    case "c++":
                    case "managedcplusplus":
                    case "j#":
                    case "jsharp":
                    case "f#":
                    case "fsharp":
                    {
                        List<string> stringList = new List<string>();
                        const string COMMENT = "__$$COMMENT##__";
                        StringBuilder tempInput = new StringBuilder(input);
                        string commentRegEx = @"//.*|/\*[\s\S]*?\*/";
                        MatchCollection matches = Regex.Matches(tempInput.ToString(), commentRegEx);
                        foreach (Match m in matches)
                        {
                            if (m.Success == true)
                            {
                                int count = m.Value.Length;
                                if (m.Value.EndsWith("\r")) count--;
                                stringList.Add(System.Web.HttpUtility.HtmlEncode(m.Value.Substring(0,count)));
                                tempInput.Replace(m.Value, COMMENT + (stringList.Count - 1));
                            }
                        }

                        StringReader tempSR = new StringReader(tempInput.ToString());
                        this.FixSource(output, tempSR, language);

                        for (int i = stringList.Count-1; i>=0; i--)
                        {
                            // Adding line break after a comment statement
                            output.Replace((COMMENT + i), (TAGFNTGRN + stringList[i] + TAGEFONT + "&#xA;  "));
                        }
                        break;
                    }
                    case "javascript":
                    case "jscript":
                        this.FixSource(output, sr, language);
                        break;
                    case "visual basic":
                    case "visualbasicusage":
                    case "visualbasic":
                    case "xaml":
                    case "visualbasicdeclaration":
                        this.FixSource(output, sr, language);
                        break;
                    default:
                        String lang = "VB";
                        bool scriptBlock = false;
                        bool multiLine = false;

                        while ((sourceLine = sr.ReadLine()) != null)
                        {
                            // First we want to grab the global language
                            // for this page by a Page directive.  Or
                            // possibly from a script block.
                            lang = GetLangFromLine(sourceLine, lang);
                            this.DefaultCaseSourceFix(output, sourceLine, lang, ref scriptBlock, ref multiLine);
                            // adding xslt line break string with checking previous line break string to avoid introduce multiple line breaks.
                            if(!sourceLine.EndsWith("&#xA;  "))
                              output.Append("&#xA;  ");
                        }

                        break;
                }
            }
            catch
            {
                throw;
            }
            
            return output.ToString();
        }

        /// <summary>
        /// List of special words that need to be found by the parser
        /// </summary>
        void InitStringArrays()
        {
          List<string> specialWords = new List<string>();

          // DataTypes
          specialWords.AddRange(new string[]
          {
            "Boolean", "Byte", "Char", "Date",
            "Decimal", "Double", "Integer", "Long",
            "Object", "Short", "Single", "String"
          });

          // Functions
          // specialWords.AddRange(new string[] { "Abs", "AppActivate", "Asc", "AscW",
          // "Atn", "Beep", "CallByName", "CBool", "CBtye", "CChar", "CDate", "CDbl",
          // "CDec", "ChDir", "ChDrive", "Choose", "Chr", "ChrW", "CInt", "CLng", "CObj",
          // "Command", "Conversion", "Cos", "CreateObject", "CShort", "CSng", "CStr",
          // "CType", "CurDir", "DateAdd", "DateDiff", "DatePart", "DateSerial",
          // "DateValue", "Day", "DDB", "DeleteSetting", "Dir", "Environ", "EOF",
          // "ErrorToString", "Exp", "FileAttr", "FileClose", "FileCopy", "FileDateTime",
          // "FileGet", "FileGetObject", "FileLen", "FileOpen", "FilePut", "FilePutObject",
          // "FileWidth", "Filter", "Fix", "Format", "FormatCurrency", "FormatDateTime",
          // "FormatNumber", "FormatPercent", "FreeFile", "FV", "GetAllSettings", "GetAttr",
          // "GetChar", "GetException", "GetObject", "GetSetting", "Hex", "Hour", "IIf",
          // "Input", "InputBox", "InputString", "InStr", "InStrRev", "Int", "IPmt", "IRR",
          // "IsArray", "IsDate", "IsDBNull", "IsError", "IsNothing", "IsNumeric", "IsReference",
          // "Join", "Kill", "LBound", "LCase", "Left", "Len", "LineInput", "Loc", "Lock",
          // "LOF", "Log", "LSet", "LTrim", "Mid", "Minute", "MIRR", "MkDir", "Month",
          // "MonthName", "MsgBox", "NPer", "NPV", "Oct", "Partition", "Pmt", "PPmt", "Print",
          // "PV", "QBColor", "Rate", "Rename", "Replace", "Reset", "RGB", "Right", "RmDir",
          // "Rnd", "Round", "RSet", "RTrim", "SaveSetting", "Second", "Seek", "SetAttr", "Sgn",
          // "Shell", "Sin", "SLN", "Space", "SPC", "Split", "Sqr", "Str", "StrComp", "StrConv",
          // "StrDup", "StrReverse", "Switch", "SYD", "SystemTypeName", "TAB", "Tan", "TimeSerial",
          // "TimeValue", "Trim", "TypeName", "UBound", "UCase", "Unlock", "Val", "VarType",
          // "VbTypeName", "Weekday", "WeekdayName", "Write", "Year" });

          // Keywords
          specialWords.AddRange(new string[]
          {
            "Alias", "Ansi", "As", "Assembly", "Auto", "ByRef",
            "ByVal", "Case", "Default", "DirectCast", "Each", "Else", "ElseIf", "End", "Error",
            "Explicit", "False", "For", "Friend", "Handles", "Implements", "In", "Lib", "Loop",
            "Me", "Module", "MustInherit", "MustOverride", "MyBase", "MyClass", "New", "Next",
            "Nothing", "NotInheritable", "NotOverridable", "Of", "Off", "On", "Option", "Optional",
            "Overloads", "Overridable", "Overrides", "ParamArray", "Preserve", "Private", "Protected",
            "Public", "ReadOnly", "Resume", "Shadows", "Shared", "Static", "Step", "Strict", "Then",
            "To", "True", "TypeOf", "Unicode", "Until", "When", "While", "WithEvents", "WriteOnly"
          });

          // Operators
          specialWords.AddRange(new string[]
          {
            "AddressOf", "And", "AndAlso", "GetType", "Is", "Like",
            "Mod", "Not", "Or", "OrElse", "Xor"
          });

          // Statements
          specialWords.AddRange(new string[]
          {
            "AddHandler", "Call", "Class", "Const", "Declare",
            "Delegate", "Dim", "Do", "Loop", "Enum", "Erase", "Error", "Event", "Exit", "For",
            "Next", "Function", "Get", "GoTo", "If", "Then", "Else", "Implements", "Imports",
            "Inherits", "Interface", "Mid", "Module", "Namespace", "On", "Option", "Property",
            "RaiseEvent", "Randomize", "ReDim", "REM", "RemoveHandler", "Resume", "Return",
            "Select", "Case", "Set", "Stop", "Structure", "Sub", "SyncLock", "Throw", "Try",
            "Catch", "Finally", "While", "End", "With"
          });

          String[] keywords = specialWords.ToArray();
          this.vbSearchExpr = "(?i)(?x)(?<buffer>(\\(|^\\s*?|\\s+?))(?<keyword>" + String.Join("|", keywords) + ")\\b";

          keywords = new String[]
          {
            "private", "protected", "public", "namespace", "class", "break",
            "for", "if", "else", "while", "switch", "case", "using",
            "return", "null", "void", "int", "bool", "string", "float",
            "this", "new", "true", "false", "const", "static", "base",
            "foreach", "in", "try", "catch", "finally", "get", "set", "char", "default"
          };

          this.csharpSearchExpr = "\\b(?<keyword>" + String.Join("|", keywords) + ")\\b(?<!//.*)";

          keywords = new String[]
          {
            "private", "protected", "public", "namespace", "class", "var",
            "for", "if", "else", "while", "switch", "case", "using", "get",
            "return", "null", "void", "int", "string", "float", "this", "set",
            "new", "true", "false", "const", "static", "package", "function",
            "internal", "extends", "super", "import", "default", "break", "try",
            "catch", "finally"
          };

          this.jsharpSearchExpr = "\\b(?<keyword>" + String.Join("|", keywords) + ")\\b(?<!//.*)";

          keywords = new String[]
          {
            "abstract", "and", "as", "asr", "assert", "atomic", "base", "begin", "break", "checked", "class",
            "component", "const", "constraint", "constructor", "continue", "default", "delegate", "do", "done", 
            "downcast", "downto", "eager", "elif", "else", "end", "event", "exception", "extern", "external",
            "false", "finally", "fixed", "for", "fun", "function", "functor", "global", "if", "in", "include",
            "inherit", "inline", "interface", "internal", "land", "lazy", "let", "lor", "lsl", "lsr", "lxor",
            "match", "member", "method", "mixin", "mod", "module", "mutable", "namespace", "new", "not struct",
            "null", "object", "of", "open", "or", "override", "parallel", "private", "process", "protected",
            "public", "pure", "rec", "return", "sealed",  "sig", "static", "struct", "tailcall", "then", "to", 
            "trait", "true", "try", "type", "upcast", "use", "val", "virtual", "void", "volatile", "when",
            "while", "with", "yield"
          };

          this.fsharpSearchExpr = "\\b(?<keyword>" + String.Join("|", keywords) + ")\\b(?<!//.*)";
        }

        /// <summary>
        /// Determines whether the specified source is a multi line tag end.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// 	<c>true</c> if the specified source is a multi line tag end; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMultiLineTagEnd(String source)
        {
            String searchExpr = "(?i)&gt;";

            source = System.Web.HttpUtility.HtmlEncode(source);
            if (Regex.IsMatch(source, searchExpr))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified source is a multi line tag start.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// 	<c>true</c> if the specified source is a multi line tag start; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMultiLineTagStart(String source)
        {
            String searchExpr = "(?i)(?!.*&gt;)(?<a>&lt;/?)(?<b>(asp:|template|property|IBuySpy:).*)";

            source = System.Web.HttpUtility.HtmlEncode(source);
            if (Regex.IsMatch(source, searchExpr))
            {
                return true;
            }

            return false;
        }

        public void FixSource(StringBuilder output, StringReader sr, string language)
        {
          FixFunction func;
          switch (language)
          {
            case "c#":
            case "csharp":
            case "c++":
            case "managedcplusplus":
              func = FixCSLine;
              break;
            case "j#":
            case "jsharp":
              func = FixJSLine;
              break;
            case "f#":
            case "fsharp":
              func = FixFSLine;
              break;
            case "javascript":
            case "jscript":
              func = FixJavaScriptLine;
              break;
            case "visual basic":
            case "visualbasicusage":
            case "visualbasic":
            case "xaml":
            case "visualbasicdeclaration":
              func = FixVBLine;
              break;
            default:
              throw new IndexOutOfRangeException("language");
              break;
          }

          String sourceLine;
          while ((sourceLine = sr.ReadLine()) != null)
          {
            output.Append(func(sourceLine));
            // adding xslt line break string with checking previous line break string to avoid introduce multiple line breaks.
            if(!sourceLine.EndsWith("&#xA;"))
              output.Append("&#xA;  ");
          }
        }
    }
    ]]>
	</msxsl:script>
</xsl:stylesheet>

