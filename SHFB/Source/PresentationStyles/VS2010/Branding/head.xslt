<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl branding"
                 xmlns:mtps="http://msdn2.microsoft.com/mtps"
                xmlns:xhtml="http://www.w3.org/1999/xhtml"
                xmlns:branding="urn:FH-Branding"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:cs="urn:Get-Paths"

>

  <!-- temporary fix to remove member filter options -->
  <xsl:template match="xhtml:div[mtps:MrefMemberlistFilter]" name="MrefMemberlistFilter"/>

  <xsl:template match="xhtml:title" name="title" >
    <xsl:variable name="title" select="."/>
    <xsl:copy>
      <xsl:value-of select="branding:GetHtmlDecode($title[1])"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xhtml:head" name="head" >
    <xsl:copy>
      <!-- favicon-->
      <xsl:element name="link" namespace="{$xhtml}">
        <xsl:attribute name="rel">
          <xsl:value-of select="'SHORTCUT ICON'"/>
        </xsl:attribute>
        <xsl:attribute name="href">
          <xsl:call-template name="ms-xhelp">
            <xsl:with-param name="ref" select="'favicon.ico'"/>
          </xsl:call-template>
        </xsl:attribute>
      </xsl:element>
      <!-- branding.css-->
      <xsl:element name="link" namespace="{$xhtml}">
        <xsl:attribute name="rel">
          <xsl:value-of select="'stylesheet'"/>
        </xsl:attribute>
        <xsl:attribute name="type">
          <xsl:value-of select="'text/css'"/>
        </xsl:attribute>
        <xsl:attribute name="href">
          <xsl:call-template name="ms-xhelp">
            <xsl:with-param name="ref" select="$css-file"/>
          </xsl:call-template>
        </xsl:attribute>
      </xsl:element>

      <!-- resource-based styles -->
      <xsl:element name="style" namespace="{$xhtml}">
        <xsl:attribute name="type">text/css</xsl:attribute>
        <xsl:text>.OH_CodeSnippetContainerTabLeftActive, .OH_CodeSnippetContainerTabLeft,.OH_CodeSnippetContainerTabLeftDisabled {background-image: url('</xsl:text>
        <xsl:call-template name="ms-xhelp">
          <xsl:with-param name="ref" select="'tabLeftBG.gif'"/>
        </xsl:call-template>
        <xsl:text>')}</xsl:text>
        <xsl:text>.OH_CodeSnippetContainerTabRightActive, .OH_CodeSnippetContainerTabRight,.OH_CodeSnippetContainerTabRightDisabled {background-image: url('</xsl:text>
        <xsl:call-template name="ms-xhelp">
          <xsl:with-param name="ref" select="'tabRightBG.gif'"/>
        </xsl:call-template>
        <xsl:text>')}</xsl:text>
        <xsl:text>.OH_footer { background-image: url('</xsl:text>
        <xsl:call-template name="ms-xhelp">
          <xsl:with-param name="ref" select="'footer_slice.gif'"/>
        </xsl:call-template>
        <xsl:text>'); background-position:top; background-repeat:repeat-x}</xsl:text>
      </xsl:element>

      <xsl:element name="script" namespace="{$xhtml}">
        <xsl:attribute name="src">
          <xsl:call-template name="ms-xhelp">
            <xsl:with-param name="ref" select="$js-file"/>
          </xsl:call-template>
        </xsl:attribute>
        <xsl:attribute name="type">
          <xsl:value-of select="'text/javascript'"/>
        </xsl:attribute>
        <xsl:comment/>
      </xsl:element>

      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xhtml:link[@rel='stylesheet']">
  </xsl:template>

  <xsl:template match="xhtml:script">
  </xsl:template>

  <!-- self-branding-->

  <xsl:template match="xhtml:head" mode="self-branding">
    <xsl:copy>
      <xsl:element name="link" namespace="{$xhtml}">
        <xsl:attribute name="rel">
          <xsl:value-of select="'stylesheet'"/>
        </xsl:attribute>
        <xsl:attribute name="type">
          <xsl:value-of select="'text/css'"/>
        </xsl:attribute>
        <xsl:attribute name="href">
          <xsl:call-template name="ms-xhelp">
            <xsl:with-param name="ref" select="$css-file"/>
          </xsl:call-template>
        </xsl:attribute>
      </xsl:element>

      <!-- resource-based styles -->
      <xsl:element name="script" namespace="{$xhtml}">
        <xsl:attribute name="src">
          <xsl:call-template name="ms-xhelp">
            <xsl:with-param name="ref" select="$js-file"/>
          </xsl:call-template>
        </xsl:attribute>
        <xsl:attribute name="type">
          <xsl:value-of select="'text/javascript'"/>
        </xsl:attribute>
        <xsl:comment/>
      </xsl:element>

      <xsl:apply-templates select="@*"/>
      <xsl:apply-templates select="node()" mode="self-branding"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xhtml:link[@rel='stylesheet']" mode="self-branding">
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
      <xsl:attribute name="href">
        <xsl:value-of select="branding:BuildContentPath($contentFolder,@href)"/>
      </xsl:attribute>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xhtml:script" mode="self-branding">
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
      <xsl:if test="@src">
        <xsl:attribute name="src">
          <xsl:value-of select="branding:BuildContentPath($contentFolder,@src)"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates mode="self-branding"/>
      <xsl:comment/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
