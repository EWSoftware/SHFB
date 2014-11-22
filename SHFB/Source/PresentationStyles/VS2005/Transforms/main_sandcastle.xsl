<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

	<!-- stuff specified to comments authored in DDUEXML -->

	<xsl:import href="utilities_bibliography.xsl"/>
	<xsl:param name="bibliographyData" select="'../Data/bibliography.xml'"/>

	<xsl:param name="omitXmlnsBoilerplate" select="'false'" />
	<xsl:param name="omitVersionInformation" select="'false'" />

	<xsl:include href="htmlBody.xsl"/>
	<xsl:include href="utilities_reference.xsl" />

	<xsl:variable name="summary" select="normalize-space(/document/comments/summary)" />
	<xsl:variable name="abstractSummary" select="/document/comments/summary" />
	<xsl:variable name="hasSeeAlsoSection" select="boolean((count(/document/comments//seealso[not(ancestor::overloads)] |
		/document/comments/conceptualLink | /document/reference/elements/element/overloads//seealso |
		/document/reference/elements/element/overloads/conceptualLink) > 0)  or
		($group='type' or $group='member' or $group='list'))"/>
	<xsl:variable name="examplesSection" select="boolean(string-length(/document/comments/example[normalize-space(.)]) > 0)"/>
	<xsl:variable name="languageFilterSection" select="boolean(string-length(/document/comments/example[normalize-space(.)]) > 0)" />

	<xsl:template name="body">
		<!-- auto-inserted info -->
		<!-- <xsl:apply-templates select="/document/reference/attributes" /> -->
		<xsl:apply-templates select="/document/comments/preliminary" />
		<xsl:apply-templates select="/document/comments/summary" />
		<xsl:if test="$subgroup='overload'">
			<xsl:apply-templates select="/document/reference/elements" mode="overloadSummary" />
		</xsl:if>
		<!-- assembly information -->
		<xsl:if test="not($group='list' or $group='root' or $group='namespace' or $group='namespaceGroup')">
			<xsl:call-template name="requirementsInfo"/>
		</xsl:if>
		<!-- syntax -->
		<xsl:if test="not($group='list' or $group='root' or $group='namespace' or $group='namespaceGroup')">
			<xsl:apply-templates select="/document/syntax" />
		</xsl:if>
		<!-- members -->
		<xsl:choose>
			<xsl:when test="$group='root'">
				<xsl:apply-templates select="/document/reference/elements" mode="root" />
			</xsl:when>
			<xsl:when test="$group='namespace'">
				<xsl:apply-templates select="/document/reference/elements" mode="namespace" />
			</xsl:when>
			<xsl:when test="$group='namespaceGroup'">
				<xsl:apply-templates select="/document/reference/elements" mode="namespaceGroup" />
			</xsl:when>
			<xsl:when test="$subgroup='enumeration'">
				<xsl:apply-templates select="/document/reference/elements" mode="enumeration" />
			</xsl:when>
			<xsl:when test="$group='type'">
				<xsl:apply-templates select="/document/reference/elements" mode="type" />
			</xsl:when>
			<xsl:when test="$group='list'">
				<xsl:choose>
					<xsl:when test="$subgroup='overload'">
						<xsl:apply-templates select="/document/reference/elements" mode="overload" />
					</xsl:when>
					<xsl:when test="$subgroup='DerivedTypeList'">
						<xsl:apply-templates select="/document/reference/elements" mode="derivedType" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="/document/reference/elements" mode="member" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
		</xsl:choose>
		<!-- remarks -->
		<xsl:apply-templates select="/document/comments/remarks" />
		<!-- example -->
		<xsl:apply-templates select="/document/comments/example" />
		<!-- other comment sections -->
		<!-- permissions -->
		<xsl:call-template name="permissions" />
		<!-- events -->
		<xsl:call-template name="events" />
		<!-- exceptions -->
		<xsl:call-template name="exceptions" />
		<!-- contracts -->
		<xsl:call-template name="contracts" />
		<!-- inheritance -->
		<xsl:apply-templates select="/document/reference/family" />
		<xsl:apply-templates select="/document/comments/threadsafety" />
		<!--versions-->
		<xsl:if test="not($group='list' or $group='root' or $group='namespace' or $group='namespaceGroup')">
			<xsl:apply-templates select="/document/reference/versions" />
		</xsl:if>
		<!-- revisions -->
		<xsl:call-template name="t_revisionHistory"/>
		<!-- bibliography -->
		<xsl:call-template name="bibliography" />
		<!-- see also -->
		<xsl:call-template name="seealso" />

	</xsl:template>

	<xsl:template name="getParameterDescription">
		<xsl:param name="name" />
		<xsl:apply-templates select="/document/comments/param[@name=$name]" />
	</xsl:template>

	<xsl:template name="getReturnsDescription">
		<xsl:param name="name" />
		<xsl:apply-templates select="/document/comments/param[@name=$name]" />
	</xsl:template>

	<xsl:template name="getElementDescription">
		<xsl:apply-templates select="summary[1]" />
	</xsl:template>

	<xsl:template name="getOverloadSummary">
		<xsl:apply-templates select="overloads" mode="summary"/>
	</xsl:template>

	<xsl:template name="getOverloadSections">
		<xsl:apply-templates select="overloads" mode="sections"/>
	</xsl:template>

	<!-- block sections -->

	<xsl:template match="summary">
		<div class="summary">
			<xsl:apply-templates />
		</div>
	</xsl:template>

	<xsl:template match="overloads" mode="summary">
		<xsl:choose>
			<xsl:when test="count(summary) > 0">
				<xsl:apply-templates select="summary" />
			</xsl:when>
			<xsl:otherwise>
				<div class="summary">
					<xsl:apply-templates/>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="overloads" mode="sections">
		<xsl:apply-templates select="remarks" />
		<xsl:apply-templates select="example"/>
	</xsl:template>

	<xsl:template match="value">
		<xsl:call-template name="subSection">
			<xsl:with-param name="title">
				<xsl:choose>
					<xsl:when test="/document/reference/apidata[@subgroup='property']">
						<include item="propertyValueTitle" />
					</xsl:when>
					<xsl:otherwise>
						<include item="fieldValueTitle" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>
			<xsl:with-param name="content">
				<include item="typeLink">
					<parameter>
						<xsl:apply-templates select="/document/reference/returns[1]" mode="link">
							<xsl:with-param name="qualified" select="true()" />
						</xsl:apply-templates>
					</parameter>
				</include>
				<br />
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="returns">
		<xsl:call-template name="subSection">
			<xsl:with-param name="title">
				<include item="methodValueTitle" />
			</xsl:with-param>
			<xsl:with-param name="content">
				<include item="typeLink">
					<parameter>
						<xsl:apply-templates select="/document/reference/returns[1]" mode="link">
							<xsl:with-param name="qualified" select="true()" />
						</xsl:apply-templates>
					</parameter>
				</include>
				<br />
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="templates">
		<xsl:call-template name="subSection">
			<xsl:with-param name="title">
				<include item="templatesTitle" />
			</xsl:with-param>
			<xsl:with-param name="content">
				<dl>
					<xsl:for-each select="template">
						<xsl:variable name="templateName" select="@name" />
						<dt>
							<span class="parameter">
								<xsl:value-of select="$templateName"/>
							</span>
						</dt>
						<dd>
							<xsl:apply-templates select="/document/comments/typeparam[@name=$templateName]" />
						</dd>
					</xsl:for-each>
				</dl>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="remarks">
		<xsl:call-template name="section">
			<xsl:with-param name="toggleSwitch" select="'remarks'"/>
			<xsl:with-param name="title">
				<include item="remarksTitle" />
			</xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="example">
		<xsl:call-template name="section">
			<xsl:with-param name="toggleSwitch" select="'example'"/>
			<xsl:with-param name="title">
				<include item="examplesTitle" />
			</xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="para">
		<p>
			<xsl:apply-templates />
		</p>
	</xsl:template>

	<xsl:template match="code">
		<xsl:variable name="codeLang">
			<xsl:call-template name="getCodeLang">
				<xsl:with-param name="p_codeLang" select="@language" />
			</xsl:call-template>
		</xsl:variable>

		<xsl:call-template name="codeSection">
			<xsl:with-param name="codeLang" select="$codeLang" />
		</xsl:call-template>

	</xsl:template>

	<xsl:template name="events">
		<xsl:if test="count(/document/comments/event) &gt; 0">
			<xsl:call-template name="section">
				<xsl:with-param name="toggleSwitch" select="'events'"/>
				<xsl:with-param name="title">
					<include item="eventsTitle" />
				</xsl:with-param>
				<xsl:with-param name="content">
					<div class="tableSection">
						<table width="100%" cellspacing="2" cellpadding="5" frame="lhs" >
							<tr>
								<th>
									<include item="eventTypeHeader" />
								</th>
								<th>
									<include item="eventReasonHeader" />
								</th>
							</tr>
							<xsl:for-each select="/document/comments/event">
								<tr>
									<td>
										<referenceLink target="{@cref}" qualified="true" />
									</td>
									<td>
										<xsl:apply-templates select="." />
									</td>
								</tr>
							</xsl:for-each>
						</table>
					</div>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="exceptions">
		<xsl:if test="count(/document/comments/exception) &gt; 0">
			<xsl:call-template name="section">
				<xsl:with-param name="toggleSwitch" select="'exceptions'"/>
				<xsl:with-param name="title">
					<include item="exceptionsTitle" />
				</xsl:with-param>
				<xsl:with-param name="content">
					<div class="tableSection">
						<table width="100%" cellspacing="2" cellpadding="5" frame="lhs">
							<tr>
								<th class="exceptionNameColumn">
									<include item="exceptionNameHeader" />
								</th>
								<th class="exceptionConditionColumn">
									<include item="exceptionConditionHeader" />
								</th>
							</tr>
							<xsl:for-each select="/document/comments/exception">
								<tr>
									<td>
										<referenceLink target="{@cref}" qualified="true" />
									</td>
									<td>
										<xsl:apply-templates select="." />
									</td>
								</tr>
							</xsl:for-each>
						</table>
					</div>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="contracts">
		<xsl:variable name="requires" select="/document/comments/requires" />
		<xsl:variable name="ensures" select="/document/comments/ensures" />
		<xsl:variable name="ensuresOnThrow" select="/document/comments/ensuresOnThrow" />
		<xsl:variable name="invariants" select="/document/comments/invariant" />
		<xsl:variable name="setter" select="/document/comments/setter" />
		<xsl:variable name="getter" select="/document/comments/getter" />
		<xsl:variable name="pure" select="/document/comments/pure" />
		<xsl:if test="$requires or $ensures or $ensuresOnThrow or $invariants or $setter or $getter or $pure">
			<xsl:call-template name="section">
				<xsl:with-param name="toggleSwitch" select="'contracts'"/>
				<xsl:with-param name="title">
					<include item="contractsTitle" />
				</xsl:with-param>
				<xsl:with-param name="content">
					<!--Purity-->
					<xsl:if test="$pure">
						<xsl:text>This method is pure.</xsl:text>
					</xsl:if>
					<!--Contracts-->
					<div class="tableSection">
						<xsl:if test="$getter">
							<xsl:variable name="getterRequires" select="$getter/requires"/>
							<xsl:variable name="getterEnsures" select="$getter/ensures"/>
							<xsl:variable name="getterEnsuresOnThrow" select="$getter/ensuresOnThrow"/>
							<xsl:call-template name="subSection">
								<xsl:with-param name="title">
									<include item="getterTitle" />
								</xsl:with-param>
								<xsl:with-param name="content">
									<xsl:if test="$getterRequires">
										<xsl:call-template name="contractsTable">
											<xsl:with-param name="title">
												<include item="requiresNameHeader"/>
											</xsl:with-param>
											<xsl:with-param name="contracts" select="$getterRequires"/>
										</xsl:call-template>
									</xsl:if>
									<xsl:if test="$getterEnsures">
										<xsl:call-template name="contractsTable">
											<xsl:with-param name="title">
												<include item="ensuresNameHeader"/>
											</xsl:with-param>
											<xsl:with-param name="contracts" select="$getterEnsures"/>
										</xsl:call-template>
									</xsl:if>
									<xsl:if test="$getterEnsuresOnThrow">
										<xsl:call-template name="contractsTable">
											<xsl:with-param name="title">
												<include item="ensuresOnThrowNameHeader"/>
											</xsl:with-param>
											<xsl:with-param name="contracts" select="$getterEnsuresOnThrow"/>
										</xsl:call-template>
									</xsl:if>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="$setter">
							<xsl:variable name="setterRequires" select="$setter/requires"/>
							<xsl:variable name="setterEnsures" select="$setter/ensures"/>
							<xsl:variable name="setterEnsuresOnThrow" select="$setter/ensuresOnThrow"/>
							<xsl:call-template name="subSection">
								<xsl:with-param name="title">
									<include item="setterTitle" />
								</xsl:with-param>
								<xsl:with-param name="content">
									<xsl:if test="$setterRequires">
										<xsl:call-template name="contractsTable">
											<xsl:with-param name="title">
												<include item="requiresNameHeader"/>
											</xsl:with-param>
											<xsl:with-param name="contracts" select="$setterRequires"/>
										</xsl:call-template>
									</xsl:if>
									<xsl:if test="$setterEnsures">
										<xsl:call-template name="contractsTable">
											<xsl:with-param name="title">
												<include item="ensuresNameHeader"/>
											</xsl:with-param>
											<xsl:with-param name="contracts" select="$setterEnsures"/>
										</xsl:call-template>
									</xsl:if>
									<xsl:if test="$setterEnsuresOnThrow">
										<xsl:call-template name="contractsTable">
											<xsl:with-param name="title">
												<include item="ensuresOnThrowNameHeader"/>
											</xsl:with-param>
											<xsl:with-param name="contracts" select="$setterEnsuresOnThrow"/>
										</xsl:call-template>
									</xsl:if>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="$requires">
							<xsl:call-template name="contractsTable">
								<xsl:with-param name="title">
									<include item="requiresNameHeader"/>
								</xsl:with-param>
								<xsl:with-param name="contracts" select="$requires"/>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="$ensures">
							<xsl:call-template name="contractsTable">
								<xsl:with-param name="title">
									<include item="ensuresNameHeader"/>
								</xsl:with-param>
								<xsl:with-param name="contracts" select="$ensures"/>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="$ensuresOnThrow">
							<xsl:call-template name="contractsTable">
								<xsl:with-param name="title">
									<include item="ensuresOnThrowNameHeader"/>
								</xsl:with-param>
								<xsl:with-param name="contracts" select="$ensuresOnThrow"/>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="$invariants">
							<xsl:call-template name="contractsTable">
								<xsl:with-param name="title">
									<include item="invariantsNameHeader"/>
								</xsl:with-param>
								<xsl:with-param name="contracts" select="$invariants"/>
							</xsl:call-template>
						</xsl:if>
					</div>
					<!--Contracts link-->
					<div class="contractsLink">
						<a>
							<xsl:attribute name="target">
								<xsl:text>_blank</xsl:text>
							</xsl:attribute>
							<xsl:attribute name="href">
								<xsl:text>http://msdn.microsoft.com/en-us/devlabs/dd491992.aspx</xsl:text>
							</xsl:attribute>
							<xsl:text>Learn more about contracts</xsl:text>
						</a>
					</div>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="contractsTable">
		<xsl:param name="title"/>
		<xsl:param name="contracts"/>
		<table width="100%" cellspacing="3" cellpadding="5" frame="lhs" >
			<tr>
				<th class="contractsNameColumn">
					<xsl:copy-of select="$title"/>
				</th>
			</tr>
			<xsl:for-each select="$contracts">
				<tr>
					<td>
						<div class="code" style="margin-bottom: 0pt; white-space: pre-wrap;">
							<pre xml:space="preserve" style="margin-bottom: 0pt"><xsl:value-of select="."/></pre>
						</div>
						<xsl:if test="@description or @inheritedFrom or @exception">
							<div style="font-size:95%; margin-left: 10pt;
                        margin-bottom: 0pt">
								<table
								 class="contractaux"
								 width="100%" frame="void" rules="none" border="0">
									<colgroup>
										<col width="10%"/>
										<col width="90%"/>
									</colgroup>
									<xsl:if test="@description">
										<tr style="border-bottom: 0px none;">
											<td style="border-bottom: 0px none;">
												<i>
													<xsl:text>Description: </xsl:text>
												</i>
											</td>
											<td style="border-bottom: 0px none;">
												<xsl:value-of select="@description"/>
											</td>
										</tr>
									</xsl:if>
									<xsl:if test="@inheritedFrom">
										<tr style="border-bottom: 0px none;">
											<td style="border-bottom: 0px none;">
												<i>
													<xsl:text>Inherited From: </xsl:text>
												</i>
											</td>
											<td style="border-bottom: 0px none;">
												<!-- Change the ID type and strip "get_" and "set_" prefixes from property member IDs -->
												<xsl:variable name="inheritedMemberId">
													<xsl:choose>
														<xsl:when test="contains(@inheritedFrom, '.get_')">
															<xsl:value-of select="concat('P:', substring-before(substring(@inheritedFrom, 3), '.get_'), '.', substring-after(@inheritedFrom, '.get_'))"/>
														</xsl:when>
														<xsl:when test="contains(@inheritedFrom, '.set_')">
															<xsl:value-of select="concat('P:', substring-before(substring(@inheritedFrom, 3), '.set_'), '.', substring-after(@inheritedFrom, '.set_'))"/>
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="@inheritedFrom"/>
														</xsl:otherwise>
													</xsl:choose>
												</xsl:variable>
												<referenceLink target="{$inheritedMemberId}">
													<xsl:value-of select="@inheritedFromTypeName"/>
												</referenceLink>
											</td>
										</tr>
									</xsl:if>
									<xsl:if test="@exception">
										<tr style="border-bottom: 0px none;">
											<td style="border-bottom: 0px none;">
												<i>
													<xsl:text>Exception: </xsl:text>
												</i>
											</td>
											<td style="border-bottom: 0px none;">
												<referenceLink target="{@exception}" qualified="true" />
											</td>
										</tr>
									</xsl:if>
								</table>
							</div>
						</xsl:if>
					</td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>

	<xsl:template name="permissions">
		<xsl:if test="count(/document/comments/permission) &gt; 0">
			<xsl:call-template name="section">
				<xsl:with-param name="toggleSwitch" select="'permissions'" />
				<xsl:with-param name="title">
					<include item="permissionsTitle" />
				</xsl:with-param>
				<xsl:with-param name="content">
					<div class="tableSection">
						<table width="100%" cellspacing="2" cellpadding="5" frame="lhs" >
							<tr>
								<th class="permissionNameColumn">
									<include item="permissionNameHeader" />
								</th>
								<th class="permissionDescriptionColumn">
									<include item="permissionDescriptionHeader" />
								</th>
							</tr>
							<xsl:for-each select="/document/comments/permission">
								<tr>
									<td>
										<referenceLink target="{@cref}" qualified="true" />
									</td>
									<td>
										<xsl:apply-templates select="." />
									</td>
								</tr>
							</xsl:for-each>
						</table>
					</div>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="seealso">
		<xsl:if test="$hasSeeAlsoSection">
			<xsl:call-template name="section">
				<xsl:with-param name="toggleSwitch" select="'seeAlso'" />
				<xsl:with-param name="title">
					<include item="relatedTitle" />
				</xsl:with-param>
				<xsl:with-param name="content">
					<xsl:call-template name="autogenSeeAlsoLinks"/>
					<!-- EFW: For comments//seealso, exclude any nested in an overloads element -->
					<xsl:for-each select="/document/comments//seealso[not(ancestor::overloads)] | /document/reference/elements/element/overloads//seealso">
						<div class="seeAlsoStyle">
							<xsl:apply-templates select=".">
								<xsl:with-param name="displaySeeAlso" select="true()" />
							</xsl:apply-templates>
						</div>
					</xsl:for-each>
					<!-- EFW: Copy conceptualLink elements as-is -->
					<xsl:for-each select="/document/comments/conceptualLink | /document/reference/elements/element/overloads/conceptualLink">
						<div class="seeAlsoStyle">
							<xsl:copy-of select="."/>
						</div>
					</xsl:for-each>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="list[@type='bullet']">
		<ul>
			<xsl:for-each select="item">
				<li>
					<xsl:choose>
						<xsl:when test="term or description">
							<xsl:if test="term">
								<b><xsl:apply-templates select="term" /></b>
								<xsl:text> - </xsl:text>
							</xsl:if>
							<xsl:apply-templates select="description" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates />
						</xsl:otherwise>
					</xsl:choose>
				</li>
			</xsl:for-each>
		</ul>
	</xsl:template>

	<xsl:template match="list[@type='number']">
		<ol>
			<xsl:if test="@start">
				<xsl:attribute name="start">
					<xsl:value-of select="@start"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:for-each select="item">
				<li>
					<xsl:choose>
						<xsl:when test="term or description">
							<xsl:if test="term">
								<b><xsl:apply-templates select="term" /></b>
								<xsl:text> - </xsl:text>
							</xsl:if>
							<xsl:apply-templates select="description" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates />
						</xsl:otherwise>
					</xsl:choose>
				</li>
			</xsl:for-each>
		</ol>
	</xsl:template>

	<xsl:template match="list[@type='table']">
		<div class="tableSection">
			<table width="100%" cellspacing="2" cellpadding="5" frame="lhs" >
				<xsl:for-each select="listheader">
					<tr>
						<xsl:for-each select="*">
							<th>
								<xsl:apply-templates />
							</th>
						</xsl:for-each>
					</tr>
				</xsl:for-each>
				<xsl:for-each select="item">
					<tr>
						<xsl:for-each select="*">
							<td>
								<xsl:apply-templates />
							</td>
						</xsl:for-each>
					</tr>
				</xsl:for-each>
			</table>
		</div>
	</xsl:template>

	<xsl:template match="list[@type='definition']">
		<dl class="authored">
			<xsl:for-each select="item">
				<dt>
					<xsl:apply-templates select="term" />
				</dt>
				<dd>
					<xsl:apply-templates select="description" />
				</dd>
			</xsl:for-each>
		</dl>
	</xsl:template>

	<!-- inline tags -->

	<xsl:template match="conceptualLink">
		<xsl:copy>
			<xsl:copy-of select="@*" />
			<xsl:apply-templates />
		</xsl:copy>
	</xsl:template>

	<xsl:template match="see[@cref]">
		<xsl:choose>
			<xsl:when test="starts-with(@cref,'O:')">
				<referenceLink target="{concat('Overload:',substring(@cref,3))}" display-target="format" show-parameters="false">
					<xsl:choose>
						<xsl:when test="normalize-space(.)">
							<xsl:value-of select="." />
						</xsl:when>
						<xsl:otherwise>
							<include item="SeeAlsoOverloadLinkText">
								<parameter>{0}</parameter>
							</include>
						</xsl:otherwise>
					</xsl:choose>
				</referenceLink>
			</xsl:when>
			<xsl:when test="normalize-space(.)">
				<referenceLink target="{@cref}">
					<xsl:apply-templates />
				</referenceLink>
			</xsl:when>
			<xsl:otherwise>
				<referenceLink target="{@cref}"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="see[@href]">
		<xsl:call-template name="hyperlink">
			<xsl:with-param name="content" select="."/>
			<xsl:with-param name="href" select="@href"/>
			<xsl:with-param name="target" select="@target"/>
			<xsl:with-param name="alt" select="@alt"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="seealso[@href]">
		<xsl:param name="displaySeeAlso" select="false()" />
		<xsl:if test="$displaySeeAlso">
			<xsl:call-template name="hyperlink">
				<xsl:with-param name="content" select="."/>
				<xsl:with-param name="href" select="@href"/>
				<xsl:with-param name="target" select="@target"/>
				<xsl:with-param name="alt" select="@alt"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="hyperlink">
		<xsl:param name="content"/>
		<xsl:param name="href"/>
		<xsl:param name="target"/>
		<xsl:param name="alt"/>
		<a>
			<xsl:attribute name="href">
				<xsl:value-of select="$href"/>
			</xsl:attribute>
			<xsl:choose>
				<xsl:when test="normalize-space($target)">
					<xsl:attribute name="target">
						<xsl:value-of select="normalize-space($target)"/>
					</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="target">_blank</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:if test="normalize-space($alt)">
				<xsl:attribute name="title">
					<xsl:value-of select="normalize-space($alt)"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="normalize-space($content)">
					<xsl:value-of select="$content" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$href" />
				</xsl:otherwise>
			</xsl:choose>
		</a>
	</xsl:template>

	<xsl:template match="see[@langword]">
		<span class="keyword">
			<xsl:choose>
				<xsl:when test="@langword='null' or @langword='Nothing' or @langword='nullptr'">
					<span class="languageSpecificText">
						<span class="cs">null</span>
						<span class="vb">Nothing</span>
						<span class="cpp">nullptr</span>
						<span class="fs">null</span>
					</span>
				</xsl:when>
				<xsl:when test="@langword='static' or @langword='Shared'">
					<span class="languageSpecificText">
						<span class="cs">static</span>
						<span class="vb">Shared</span>
						<span class="cpp">static</span>
						<span class="fs">static</span>
					</span>
				</xsl:when>
				<xsl:when test="@langword='virtual' or @langword='Overridable'">
					<span class="languageSpecificText">
						<span class="cs">virtual</span>
						<span class="vb">Overridable</span>
						<span class="cpp">virtual</span>
						<span class="fs">virtual</span>
					</span>
				</xsl:when>
				<xsl:when test="@langword='true' or @langword='True'">
					<span class="languageSpecificText">
						<span class="cs">true</span>
						<span class="vb">True</span>
						<span class="cpp">true</span>
						<span class="fs">true</span>
					</span>
				</xsl:when>
				<xsl:when test="@langword='false' or @langword='False'">
					<span class="languageSpecificText">
						<span class="cs">false</span>
						<span class="vb">False</span>
						<span class="cpp">false</span>
						<span class="fs">false</span>
					</span>
				</xsl:when>
				<xsl:when test="@langword='abstract' or @langword='MustInherit'">
					<span class="languageSpecificText">
						<span class="cs">abstract</span>
						<span class="vb">MustInherit</span>
						<span class="cpp">abstract</span>
						<span class="fs">abstract</span>
					</span>
				</xsl:when>
				<xsl:when test="@langword='async' or @langword='async'">
					<span class="languageSpecificText">
						<span class="cs">async</span>
						<span class="vb">Async</span>
						<span class="cpp">async</span>
						<span class="fs">async</span>
					</span>
				</xsl:when>
				<xsl:when test="@langword='await' or @langword='Await' or @langword='let!'">
					<span class="languageSpecificText">
						<span class="cs">await</span>
						<span class="vb">Await</span>
						<span class="cpp">await</span>
						<span class="fs">let!</span>
					</span>
				</xsl:when>
				<xsl:when test="@langword='async/await' or @langword='Async/Await' or @langword='async/let!'">
					<span class="languageSpecificText">
						<span class="cs">async/await</span>
						<span class="vb">Async/Await</span>
						<span class="cpp">async/await</span>
						<span class="fs">async/let!</span>
					</span>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@langword" />
				</xsl:otherwise>
			</xsl:choose>
		</span>
		<xsl:choose>
			<xsl:when test="@langword='null' or @langword='Nothing' or @langword='nullptr'">
				<span class="nu">
					<include item="nullKeyword"/>
				</span>
			</xsl:when>
			<xsl:when test="@langword='static' or @langword='Shared'">
				<span class="nu">
					<include item="staticKeyword"/>
				</span>
			</xsl:when>
			<xsl:when test="@langword='virtual' or @langword='Overridable'">
				<span class="nu">
					<include item="virtualKeyword"/>
				</span>
			</xsl:when>
			<xsl:when test="@langword='true' or @langword='True'">
				<span class="nu">
					<include item="trueKeyword"/>
				</span>
			</xsl:when>
			<xsl:when test="@langword='false' or @langword='False'">
				<span class="nu">
					<include item="falseKeyword"/>
				</span>
			</xsl:when>
			<xsl:when test="@langword='abstract' or @langword='MustInherit'">
				<span class="nu">
					<include item="abstractKeyword"/>
				</span>
			</xsl:when>
			<xsl:when test="@langword='async' or @langword='async'">
				<span class="nu">
					<include item="asyncKeyword"/>
				</span>
			</xsl:when>
			<xsl:when test="@langword='await' or @langword='Await' or @langword='let!'">
				<span class="nu">
					<include item="awaitKeyword"/>
				</span>
			</xsl:when>
			<xsl:when test="@langword='async/await' or @langword='Async/Await' or @langword='async/let!'">
				<span class="nu">
					<include item="asyncAwaitKeyword"/>
				</span>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="seealso">
		<xsl:param name="displaySeeAlso" select="false()" />
		<xsl:if test="$displaySeeAlso">
			<xsl:choose>
				<xsl:when test="starts-with(@cref,'O:')">
					<referenceLink target="{concat('Overload:',substring(@cref,3))}" display-target="format" show-parameters="false">
						<xsl:choose>
							<xsl:when test="normalize-space(.)">
								<xsl:apply-templates />
							</xsl:when>
							<xsl:otherwise>
								<include item="SeeAlsoOverloadLinkText">
									<parameter>{0}</parameter>
								</include>
							</xsl:otherwise>
						</xsl:choose>
					</referenceLink>
				</xsl:when>
				<xsl:when test="normalize-space(.)">
					<referenceLink target="{@cref}" qualified="true">
						<xsl:apply-templates />
					</referenceLink>
				</xsl:when>
				<xsl:otherwise>
					<referenceLink target="{@cref}" qualified="true" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template match="c">
		<span class="code">
			<xsl:apply-templates/>
		</span>
	</xsl:template>

	<xsl:template match="paramref">
		<span class="parameter">
			<xsl:value-of select="@name" />
		</span>
	</xsl:template>

	<xsl:template match="typeparamref">
		<span class="typeparameter">
			<xsl:value-of select="@name" />
		</span>
	</xsl:template>

	<xsl:template match="syntax">
		<xsl:if test="count(*) > 0">
			<xsl:call-template name="section">
				<xsl:with-param name="toggleSwitch" select="'syntax'" />
				<xsl:with-param name="title">
					<include item="syntaxTitle"/>
				</xsl:with-param>
				<xsl:with-param name="content">
					<div id="syntaxCodeBlocks" class="code">
						<xsl:call-template name="syntaxBlocks" />
					</div>
					<!-- parameters & return value -->
					<xsl:apply-templates select="/document/reference/parameters" />
					<xsl:apply-templates select="/document/reference/templates" />
					<xsl:choose>
						<xsl:when test="/document/comments/value | /document/comments/returns">
							<xsl:apply-templates select="/document/comments/value" />
							<xsl:apply-templates select="/document/comments/returns" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:if test="/document/reference/returns[1] | /document/reference/eventhandler/type">
								<xsl:call-template name="defaultReturnSection" />
							</xsl:if>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:apply-templates select="/document/reference/implements" />
					<!-- usage note for extension methods -->
					<xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.Runtime.CompilerServices.ExtensionAttribute'] and boolean($api-subgroup='method')">
						<xsl:call-template name="subSection">
							<xsl:with-param name="title">
								<include item="extensionUsageTitle" />
							</xsl:with-param>
							<xsl:with-param name="content">
								<include item="extensionUsageText">
									<parameter>
										<xsl:apply-templates select="/document/reference/parameters/parameter[1]/type" mode="link" />
									</parameter>
								</include>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:if>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="defaultReturnSection">
		<xsl:call-template name="subSection">
			<xsl:with-param name="title">
				<xsl:choose>
					<xsl:when test="/document/reference/apidata[@subgroup='property']">
						<include item="propertyValueTitle" />
					</xsl:when>
					<xsl:when test="/document/reference/apidata[@subgroup='field']">
						<include item="fieldValueTitle" />
					</xsl:when>
					<xsl:when test="/document/reference/apidata[@subgroup='event']">
						<include item="valueTitle" />
					</xsl:when>
					<xsl:otherwise>
						<include item="methodValueTitle" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>
			<xsl:with-param name="content">
				<include item="typeLink">
					<parameter>
						<xsl:choose>
							<xsl:when test="/document/reference/attributes/attribute/type[@api='T:System.Runtime.CompilerServices.FixedBufferAttribute']">
								<xsl:apply-templates select="/document/reference/attributes/attribute/type[@api='T:System.Runtime.CompilerServices.FixedBufferAttribute']/../argument/typeValue/type" mode="link">
									<xsl:with-param name="qualified" select="true()" />
								</xsl:apply-templates>
							</xsl:when>
							<xsl:when test="/document/reference/apidata[@subgroup='event']">
								<xsl:apply-templates select="/document/reference/eventhandler/type" mode="link">
									<xsl:with-param name="qualified" select="true()" />
								</xsl:apply-templates>
							</xsl:when>
							<xsl:otherwise>
								<xsl:apply-templates select="/document/reference/returns[1]" mode="link">
									<xsl:with-param name="qualified" select="true()" />
								</xsl:apply-templates>
							</xsl:otherwise>
						</xsl:choose>
					</parameter>
				</include>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<!-- pass through html tags -->

	<xsl:template match="p|ol|ul|li|dl|dt|dd|table|tr|th|td|a|img|b|i|strong|em|del|sub|sup|br|hr|h1|h2|h3|h4|h5|h6|pre|div|span|blockquote|abbr|acronym|u|font|map|area">
		<xsl:copy>
			<xsl:copy-of select="@*" />
			<xsl:apply-templates />
		</xsl:copy>
	</xsl:template>

	<!-- extra tag support -->

	<xsl:template match="threadsafety">
		<xsl:call-template name="section">
			<xsl:with-param name="toggleSwitch" select="'threadSafety'" />
			<xsl:with-param name="title">
				<include item="threadSafetyTitle" />
			</xsl:with-param>
			<xsl:with-param name="content">
				<xsl:choose>
					<xsl:when test="normalize-space(.)">
						<xsl:apply-templates />
					</xsl:when>
					<xsl:when test="(not(@instance) and not(@static)) or (@static='true' and @instance='false')">
						<include item="ThreadSafetyBP" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:if test="@static='true'">
							<include item="staticThreadSafe" />
						</xsl:if>
						<xsl:if test="@static='false'">
							<include item="staticNotThreadSafe" />
						</xsl:if>
						<xsl:if test="@instance='true'">
							<include item="instanceThreadSafe" />
						</xsl:if>
						<xsl:if test="@instance='false'">
							<include item="instanceNotThreadSafe" />
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="note">
		<xsl:variable name="title">
			<xsl:choose>
				<xsl:when test="@type='note'">
					<xsl:text>noteTitle</xsl:text>
				</xsl:when>
				<xsl:when test="@type='tip'">
					<xsl:text>tipTitle</xsl:text>
				</xsl:when>
				<xsl:when test="@type='caution' or @type='warning'">
					<xsl:text>cautionTitle</xsl:text>
				</xsl:when>
				<xsl:when test="@type='security' or @type='security note'">
					<xsl:text>securityTitle</xsl:text>
				</xsl:when>
				<xsl:when test="@type='important'">
					<xsl:text>importantTitle</xsl:text>
				</xsl:when>
				<xsl:when test="@type='vb' or @type='VB' or @type='VisualBasic' or @type='visual basic note'">
					<xsl:text>visualBasicTitle</xsl:text>
				</xsl:when>
				<xsl:when test="@type='cs' or @type='CSharp' or @type='c#' or @type='C#' or @type='visual c# note'">
					<xsl:text>visualC#Title</xsl:text>
				</xsl:when>
				<xsl:when test="@type='cpp' or @type='c++' or @type='C++' or @type='CPP' or @type='visual c++ note'">
					<xsl:text>visualC++Title</xsl:text>
				</xsl:when>
				<xsl:when test="@type='JSharp' or @type='j#' or @type='J#' or @type='visual j# note'">
					<xsl:text>visualJ#Title</xsl:text>
				</xsl:when>
				<xsl:when test="@type='implement'">
					<xsl:text>NotesForImplementers</xsl:text>
				</xsl:when>
				<xsl:when test="@type='caller'">
					<xsl:text>NotesForCallers</xsl:text>
				</xsl:when>
				<xsl:when test="@type='inherit'">
					<xsl:text>NotesForInheritors</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>noteTitle</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="altTitle">
			<xsl:choose>
				<xsl:when test="@type='note' or @type='implement' or @type='caller' or @type='inherit'">
					<xsl:text>noteAltText</xsl:text>
				</xsl:when>
				<xsl:when test="@type='tip'">
					<xsl:text>tipAltText</xsl:text>
				</xsl:when>
				<xsl:when test="@type='caution' or @type='warning'">
					<xsl:text>cautionAltText</xsl:text>
				</xsl:when>
				<xsl:when test="@type='security' or @type='security note'">
					<xsl:text>securityAltText</xsl:text>
				</xsl:when>
				<xsl:when test="@type='important'">
					<xsl:text>importantAltText</xsl:text>
				</xsl:when>
				<xsl:when test="@type='vb' or @type='VB' or @type='VisualBasic' or @type='visual basic note'">
					<xsl:text>visualBasicAltText</xsl:text>
				</xsl:when>
				<xsl:when test="@type='cs' or @type='CSharp' or @type='c#' or @type='C#' or @type='visual c# note'">
					<xsl:text>visualC#AltText</xsl:text>
				</xsl:when>
				<xsl:when test="@type='cpp' or @type='c++' or @type='C++' or @type='CPP' or @type='visual c++ note'">
					<xsl:text>visualC++AltText</xsl:text>
				</xsl:when>
				<xsl:when test="@type='JSharp' or @type='j#' or @type='J#' or @type='visual j# note'">
					<xsl:text>visualJ#AltText</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>noteAltText</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="noteImg">
			<xsl:choose>
				<xsl:when test="@type='note' or @type='tip' or @type='implement' or @type='caller' or @type='inherit'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="@type='caution' or @type='warning'">
					<xsl:text>alert_caution.gif</xsl:text>
				</xsl:when>
				<xsl:when test="@type='security' or @type='security note'">
					<xsl:text>alert_security.gif</xsl:text>
				</xsl:when>
				<xsl:when test="@type='important'">
					<xsl:text>alert_caution.gif</xsl:text>
				</xsl:when>
				<xsl:when test="@type='vb' or @type='VB' or @type='VisualBasic' or @type='visual basic note'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="@type='cs' or @type='CSharp' or @type='c#' or @type='C#' or @type='visual c# note'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="@type='cpp' or @type='c++' or @type='C++' or @type='CPP' or @type='visual c++ note'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="@type='JSharp' or @type='j#' or @type='J#' or @type='visual j# note'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="alert">
			<table>
				<tr>
					<th>
						<img>
							<includeAttribute item="iconPath" name="src">
								<parameter>
									<xsl:value-of select="$noteImg"/>
								</parameter>
							</includeAttribute>
							<includeAttribute name="title" item="{$altTitle}" />
						</img>
						<xsl:text> </xsl:text>
						<include item="{$title}" />
					</th>
				</tr>
				<tr>
					<td>
						<xsl:apply-templates />
					</td>
				</tr>
			</table>
		</div>
	</xsl:template>

	<xsl:template match="preliminary">
		<div class="preliminary">
			<xsl:choose>
				<xsl:when test="normalize-space(.)">
					<xsl:apply-templates/>
				</xsl:when>
				<xsl:otherwise>
					<include item="preliminaryText" />
				</xsl:otherwise>
			</xsl:choose>
		</div>
	</xsl:template>

	<xsl:template name="createReferenceLink">
		<xsl:param name="id" />
		<xsl:param name="qualified" select="false()" />

		<referenceLink target="{$id}" qualified="{$qualified}" />

	</xsl:template>

	<xsl:template name="section">
		<xsl:param name="toggleSwitch" />
		<xsl:param name="title" />
		<xsl:param name="content" />

		<xsl:variable name="toggleTitle" select="concat($toggleSwitch,'Toggle')" />
		<xsl:variable name="toggleSection" select="concat($toggleSwitch,'Section')" />

		<h1 class="heading">
			<span onclick="ExpandCollapse({$toggleTitle})" style="cursor:default;" onkeypress="ExpandCollapse_CheckKey({$toggleTitle}, event)" tabindex="0">
				<img id="{$toggleTitle}" class="toggle" name="toggleSwitch">
					<includeAttribute name="src" item="iconPath">
						<parameter>collapse_all.gif</parameter>
					</includeAttribute>
				</img>
				<xsl:copy-of select="$title" />
			</span>
		</h1>

		<div id="{$toggleSection}" class="section" name="collapseableSection">
			<xsl:copy-of select="$content" />
		</div>

	</xsl:template>

	<xsl:template name="subSection">
		<xsl:param name="title" />
		<xsl:param name="content" />

		<h4 class="subHeading">
			<xsl:copy-of select="$title" />
		</h4>
		<xsl:copy-of select="$content" />

	</xsl:template>

	<xsl:template name="memberIntro">
		<xsl:if test="$subgroup='members'">
			<p>
				<xsl:apply-templates select="/document/comments/summary"/>
			</p>
		</xsl:if>
		<xsl:call-template name="memberIntroBoilerplate"/>
	</xsl:template>

	<xsl:template name="codelangAttributes">
		<xsl:call-template name="mshelpCodelangAttributes">
			<xsl:with-param name="snippets" select="/document/comments/example/code" />
		</xsl:call-template>
	</xsl:template>

	<!-- Footer stuff -->

	<xsl:template name="foot">
		<div id="footer">

			<div class="footerLine">
				<img width="100%" height="3px">
					<includeAttribute name="src" item="iconPath">
						<parameter>footer.gif</parameter>
					</includeAttribute>
					<includeAttribute name="alt" item="footerImage" />
					<includeAttribute name="title" item="footerImage" />
				</img>
			</div>

			<include item="footer">
				<parameter>
					<xsl:value-of select="$key"/>
				</parameter>
				<parameter>
					<xsl:call-template name="topicTitlePlain"/>
				</parameter>
				<parameter>
					<xsl:value-of select="/document/metadata/item[@id='PBM_FileVersion']" />
				</parameter>
				<parameter>
					<xsl:value-of select="/document/metadata/attribute[@name='TopicVersion']" />
				</parameter>
			</include>
		</div>
	</xsl:template>

	<!-- Bibliography -->
	<xsl:key name="citations" match="//cite" use="text()" />

	<xsl:variable name="hasCitations" select="boolean(count(//cite) > 0)"/>

	<xsl:template match="cite">
		<xsl:variable name="currentCitation" select="text()"/>
		<xsl:for-each select="//cite[generate-id(.)=generate-id(key('citations',text()))]">
			<!-- Distinct citations only -->
			<xsl:if test="$currentCitation=.">
				<xsl:choose>
					<xsl:when test="document($bibliographyData)/bibliography/reference[@name=$currentCitation]">
						<sup class="citation">
							<a>
								<xsl:attribute name="href">
									#cite<xsl:value-of select="position()"/>
								</xsl:attribute>[<xsl:value-of select="position()"/>]
							</a>
						</sup>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="bibliography">
		<xsl:if test="$hasCitations">
			<xsl:call-template name="section">
				<xsl:with-param name="toggleSwitch" select="'cite'" />
				<xsl:with-param name="title">
					<include item="bibliographyTitle"/>
				</xsl:with-param>
				<xsl:with-param name="content">
					<xsl:call-template name="autogenBibliographyLinks"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="autogenBibliographyLinks">
		<xsl:for-each select="//cite[generate-id(.)=generate-id(key('citations',text()))]">
			<!-- Distinct citations only -->
			<xsl:variable name="citation" select="."/>
			<xsl:variable name="entry" select="document($bibliographyData)/bibliography/reference[@name=$citation]"/>

			<xsl:call-template name="bibliographyReference">
				<xsl:with-param name="number" select="position()"/>
				<xsl:with-param name="data" select="$entry"/>
			</xsl:call-template>
		</xsl:for-each>
	</xsl:template>

	<!-- Revision History information template processing. -->
	<xsl:template name="t_revisionHistory" match="revisionHistory" >
		<xsl:if test="boolean(count(/document//revisionHistory) > 0)">
			<xsl:if test="not(/document//revisionHistory[@visible='false'])">
				<xsl:call-template name="section">
					<xsl:with-param name="toggleSwitch" select="'revisionHistorySection'"/>
					<xsl:with-param name="title">
						<include item="revisionHistoryTitle" />
					</xsl:with-param>
					<xsl:with-param name="content">
						<table>
							<tr>
								<th>
									<include item="revHistoryDate" />
								</th>
								<th>
									<include item="revHistoryVersion" />
								</th>
								<th>
									<include item="revHistoryDescription" />
								</th>
							</tr>
							<xsl:for-each select="/document//revisionHistory/revision">
								<xsl:if test="not(@visible='false')">
									<tr>
										<td>
											<xsl:value-of select="@date"/>
										</td>
										<td>
											<xsl:value-of select="@version"/>
										</td>
										<td>
											<xsl:apply-templates />
										</td>
									</tr>
								</xsl:if>
							</xsl:for-each>
						</table>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<!-- Pass through a chunk of markup.  This will allow build components to add HTML or other elements such as
			 "include" for localized shared content to a pre-transformed document.  This prevents it being removed as
			 unrecognized content by the transformations. -->
	<xsl:template match="markup">
		<xsl:copy-of select="node()"/>
	</xsl:template>
</xsl:stylesheet>
