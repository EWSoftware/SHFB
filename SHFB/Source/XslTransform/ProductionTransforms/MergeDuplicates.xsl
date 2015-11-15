<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

	<xsl:output indent="yes" encoding="UTF-8" />

	<!-- Merges reflection nodes for APIs that are declared in multiple assemblies.  For example, some of the same
			 APIs in the Microsoft.Windows.Themes namespace are declared in:
					PresentationFramework.Aero.dll
					PresentationFramework.Luna.dll
					PresentationFramework.Classic.dll
					PresentationFramework.Royale.dll
    
			 This transform:
					- Gets rid of duplicate element nodes in a namespace's API node
					- Type API nodes: collapses duplicates into a single API node; saves library info for each duplicate
					- Member API nodes: collapses duplicates into a single API node; saves library info for each duplicate
					- For element lists, add library info to elements that are not in all duplicates
  -->
	<xsl:key name="index" match="/reflection/apis/api" use="@id" />

	<xsl:template match="/">
		<reflection>
			<xsl:copy-of select="/reflection/@*"/>
			<xsl:copy-of select="/reflection/assemblies" />
			<apis>
				<xsl:apply-templates select="/reflection/apis/api" />
			</apis>
		</reflection>
	</xsl:template>

	<xsl:template match="api">
		<xsl:copy-of select="." />
	</xsl:template>

	<xsl:template match="api[apidata/@group='namespace']">
		<api>
			<xsl:copy-of select="@*" />
			<xsl:for-each select="*">
				<xsl:choose>
					<xsl:when test="local-name()='elements'">
						<elements>
							<xsl:for-each select="element[not(@api=preceding-sibling::element/@api)]">
								<xsl:copy-of select="." />
							</xsl:for-each>
						</elements>
					</xsl:when>
					<xsl:otherwise>
						<xsl:copy-of select="." />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
		</api>
	</xsl:template>

	<xsl:template match="api[apidata/@group='type']">
		<xsl:variable name="duplicates" select="key('index',@id)" />
		<xsl:variable name="duplicatesCount" select="count($duplicates)"/>
		<xsl:choose>
			<!-- If dupes, merge them -->
			<xsl:when test="$duplicatesCount &gt; 1">
				<xsl:variable name="typeId" select="@id" />
				<xsl:if test="not(preceding-sibling::api[@id=$typeId])">
					<api>
						<xsl:copy-of select="@*" />
						<xsl:for-each select="*">
							<xsl:choose>
								<xsl:when test="local-name()='containers'">
									<containers>
										<xsl:copy-of select="$duplicates/containers/library" />
										<xsl:copy-of select="namespace" />
									</containers>
								</xsl:when>
								<xsl:when test="local-name()='elements'">
									<elements>
										<xsl:for-each select="$duplicates/elements/element">
											<xsl:variable name="elementId" select="@api"/>
											<xsl:if test="not(preceding::api[@id=$typeId]/elements/element[@api=$elementId])">
												<!-- Need to add library info to elements that are not in all duplicates -->
												<element>
													<xsl:copy-of select="@*"/>
													<xsl:copy-of select="*"/>
													<xsl:if test="count($duplicates/elements/element[@api=$elementId]) != $duplicatesCount">
														<libraries>
															<xsl:copy-of select="$duplicates/elements/element[@api=$elementId]/../../containers/library"/>
														</libraries>
													</xsl:if>
												</element>
											</xsl:if>
										</xsl:for-each>
									</elements>
								</xsl:when>
								<xsl:otherwise>
									<xsl:copy-of select="." />
								</xsl:otherwise>
							</xsl:choose>
						</xsl:for-each>
					</api>
				</xsl:if>
			</xsl:when>
			<!-- If no dupes, just copy it -->
			<xsl:otherwise>
				<xsl:copy-of select="." />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="api[apidata/@group='member']">
		<xsl:variable name="subgroup" select="apidata/@subgroup" />
		<xsl:variable name="duplicates" select="key('index',@id)[apidata[@subgroup=$subgroup]]" />
		<xsl:choose>
			<!-- If dupes, merge them -->
			<xsl:when test="count($duplicates)&gt;1">
				<xsl:variable name="memberId" select="@id" />
				<xsl:if test="not(preceding-sibling::api[@id=$memberId][apidata[@subgroup=$subgroup]])">
					<api>
						<xsl:copy-of select="@*" />
						<xsl:for-each select="*">
							<xsl:choose>
								<xsl:when test="local-name()='containers'">
									<containers>
										<!-- Include the library node for all the duplicates -->
										<xsl:copy-of select="$duplicates/containers/library" />
										<xsl:copy-of select="namespace|type" />
									</containers>
								</xsl:when>
								<xsl:otherwise>
									<xsl:copy-of select="." />
								</xsl:otherwise>
							</xsl:choose>
						</xsl:for-each>
					</api>
				</xsl:if>
			</xsl:when>
			<!-- If no dupes, just copy it -->
			<xsl:otherwise>
				<xsl:copy-of select="." />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
