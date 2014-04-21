<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:mtps="http://msdn2.microsoft.com/mtps"
								xmlns:xhtml="http://www.w3.org/1999/xhtml"
>
	<xsl:output version ="1.0" encoding="utf-8" method="xml" indent="no" omit-xml-declaration="yes" />

	<!-- ********************************************************************************************
  *************************************************************************************************
	IMPORTANT NOTE:
  *************************************************************************************************
	
	The branding transformations are being removed.  All Sandcastle output is self-branded by nature
	and the branding transformations thus serve no real purpose.  Most of the functionality has been
	moved back into the presentation style as of this release.  The remaining elements related to
	syntax sections, code sections, and language-specific text will be moved in a later release.
	
  *************************************************************************************************
	************************************************************************************************* -->

	<!-- ============================================================================================
	Global variables
	============================================================================================= -->

	<xsl:variable name="xhtml" select="'http://www.w3.org/1999/xhtml'"/>
	
	<!-- ============================================================================================
	Transforms
	============================================================================================= -->

	<!-- This must appear as the first template.  It passes through most elements as-is.  Empty elements except
			 those that can be self-closing get a space for content to keep them as full start and end elements when
			 saved. -->
	<xsl:template match="@* | node()" name="identity">
		<xsl:copy>
			<xsl:apply-templates select="@* | node()"/>
			<xsl:if test="not(*) and not(text()) and not(self::xhtml:br) and not(self::xhtml:hr) and
							not(self::xhtml:meta) and not(self::xhtml:link) and not(self::xhtml:img) and
							not(starts-with(name(.),'MSHelp'))">
				<xsl:value-of select="' '"/>
			</xsl:if>
		</xsl:copy>
	</xsl:template>

	<!-- TODO: Can't get rid of this element entirely yet since it contains information used by the code snippet
			 stuff. -->
	<xsl:template match="/xhtml:html/xhtml:head/xhtml:xml[@id='BrandingData']"/>

	<!-- TODO: Can't get rid of this element entirely yet as the code snippet stuff appears to have a dependency
			 on it when checking for parent elements. -->
	<xsl:template match="mtps:CollapsibleArea" name="ps-collapsible-area">
		<xsl:apply-templates select="node()"/>
	</xsl:template>

	<!-- TODO: This will need to be converted to a build component since we can't change how the syntax components
			 write out their LST elements without a lot of bother. -->
	<!-- Convert old-style LST to new style -->
	<xsl:template match="xhtml:span[@class='languageSpecificText']" name="old-lst">
		<xsl:choose>
			<xsl:when test="count(xhtml:span[@class]) = count(*)">
				<xsl:variable name="v_id" select="generate-id(.)"/>

				<xsl:element name="span" namespace="{$xhtml}">
					<xsl:attribute name="id">
						<xsl:value-of select="$v_id"/>
					</xsl:attribute>
					<xsl:text>&#160;</xsl:text>
				</xsl:element>
				<xsl:element name="script" namespace="{$xhtml}">
					<xsl:attribute name="type">
						<xsl:value-of select="'text/javascript'"/>
					</xsl:attribute>
					addToLanSpecTextIdSet("<xsl:value-of select="$v_id"/>?<xsl:value-of select ="'vb='"/><xsl:if test="xhtml:span[@class='vb']">
						<xsl:value-of select ="xhtml:span[@class='vb']"/>
					</xsl:if><xsl:value-of select ="'|cpp='"/><xsl:if test="xhtml:span[@class='cpp']">
						<xsl:value-of select ="xhtml:span[@class='cpp']"/>
					</xsl:if><xsl:value-of select ="'|cs='"/><xsl:if test="xhtml:span[@class='cs']">
						<xsl:value-of select ="xhtml:span[@class='cs']"/>
					</xsl:if><xsl:value-of select ="'|fs='"/><xsl:if test="xhtml:span[@class='fs']">
						<xsl:value-of select ="xhtml:span[@class='fs']"/>
					</xsl:if><xsl:value-of select ="'|nu='"/><xsl:if test="xhtml:span[@class='nu']">
						<xsl:value-of select ="xhtml:span[@class='nu']"/>
					</xsl:if><xsl:for-each select="xhtml:span[@class!='vb' and @class!='cpp' and @class!='cs' and @class!='fs' and @class!='nu']">
						<xsl:value-of select ="concat('|',@class,'=')"/>
						<xsl:value-of select ="."/>
					</xsl:for-each>");
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy-of select="."/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
