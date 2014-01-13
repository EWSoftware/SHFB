<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"  exclude-result-prefixes="msxsl branding"
                 xmlns:mtps="http://msdn2.microsoft.com/mtps"
                xmlns:xhtml="http://www.w3.org/1999/xhtml"
                xmlns:branding="urn:FH-Branding"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
>

  <xsl:import href="toc.xslt"/>

  <!-- pass all unbranded content through transform-->
  <xsl:template match="*|/" mode="self-branding">
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
      <xsl:if test="@style">
        <xsl:attribute name="style">
          <xsl:value-of select="@style"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates mode="self-branding"/>
      <xsl:if test="not(*) and not(text())">
        <xsl:value-of select="' '"/>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xhtml:body" name="body">
    <xsl:copy>
      <xsl:attribute name="onload">onLoad()</xsl:attribute>
      <xsl:attribute name="class">OH_body</xsl:attribute>
      <xsl:apply-templates select="@*"/>

      <!-- high contrast color test-->
      <xsl:element name="span" namespace="{$xhtml}">
        <xsl:attribute name="id">HCColorTest</xsl:attribute>
        <xsl:text> </xsl:text>
      </xsl:element>
      
      <!-- main frame-->
      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="class">OH_outerDiv</xsl:attribute>
        <xsl:call-template name="displayLeftNav"/>
        <xsl:element name="div" namespace="{$xhtml}">
          <xsl:attribute name="class">OH_outerContent</xsl:attribute>
          <xsl:apply-templates select="node()"/>
        </xsl:element>
      </xsl:element>
    </xsl:copy>
    <xsl:call-template name="footer"/>
  </xsl:template>

  <!-- self-branded -->

  <xsl:template match="xhtml:body" mode="self-branding" name="body-self-branding">
    <xsl:copy>
      <xsl:attribute name="onload">onLoad()</xsl:attribute>
      <xsl:attribute name="class">OH_VSIP_body</xsl:attribute>
      <xsl:apply-templates select="@*"/>
      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="class">OH_outerDiv</xsl:attribute>
        <xsl:call-template name="displayLeftNav"/>
        <xsl:element name="div" namespace="{$xhtml}">
          <xsl:attribute name="class">OH_outerContent</xsl:attribute>
          <xsl:apply-templates select="node()" mode="self-branding"/>
        </xsl:element>
      </xsl:element>

    </xsl:copy>
  </xsl:template>

  <xsl:template match="xhtml:img" mode="self-branding">
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
      <xsl:attribute name="src">
        <xsl:value-of select="branding:BuildContentPath($contentFolder,@src)"/>
      </xsl:attribute>
      <xsl:if test="@style">
        <xsl:attribute name="style">
          <xsl:value-of select="@style"/>
        </xsl:attribute>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <!-- error (404) page-->

  <xsl:template match="xhtml:body" mode="error" name="body-error">
    <xsl:copy>
      <xsl:attribute name="onload">onLoad()</xsl:attribute>
      <xsl:attribute name="class">OH_body</xsl:attribute>
      <xsl:apply-templates select="@*"/>

      <!-- high contrast color test-->
      <xsl:element name="span" namespace="{$xhtml}">
        <xsl:attribute name="id">HCColorTest</xsl:attribute>
        <xsl:text> </xsl:text>
      </xsl:element>

      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="class">OH_outerDiv</xsl:attribute>
        <xsl:element name="div" namespace="{$xhtml}">
          <xsl:attribute name="class">OH_outerContent</xsl:attribute>
          <xsl:apply-templates select="node()"/>
        </xsl:element>

      </xsl:element>
    </xsl:copy>
    <xsl:call-template name="footer"/>
  </xsl:template>
  <xsl:template match="xhtml:rss" />
  <xsl:template match="TocRoot" name="TocRoot"/>

  <!--New Add Template for Add Logo to Page Header-->
  <xsl:template match="*[@class='majorTitle']" name="body-majorTitle">
  </xsl:template>

  <xsl:template match="*[@class='topic']" name="body-topic">
    <xsl:apply-templates select="node()"/>
  </xsl:template>

  <xsl:template match="*[@class='title']" name="body-title">
    <xsl:variable name="id" select="generate-id()" />
    <xsl:element name="div" namespace="{$xhtml}">
      <xsl:attribute name="class">OH_topic</xsl:attribute>

      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="class">OH_title</xsl:attribute>
        <xsl:element name="table" namespace="{$xhtml}">
          <xsl:element name="tr" namespace="{$xhtml}">
            <xsl:element name="td" namespace="{$xhtml}">
              <xsl:attribute name="class">OH_tdTitleColumn</xsl:attribute>
              <xsl:choose>
                <xsl:when test="$error-page">
                  <xsl:value-of select="$errorTitle"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:variable name="bodyTitle" select="." />
                  <xsl:choose>
                    <xsl:when test="$bodyTitle = ''  ">
                      <xsl:value-of select="$title"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:apply-templates />
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:element>
            <xsl:element name="td" namespace="{$xhtml}">
              <xsl:attribute name="class">OH_tdLogoColumn</xsl:attribute>
              <xsl:element name="img" namespace="{$xhtml}">
                <xsl:attribute name="id">VSLogo</xsl:attribute>
                <xsl:attribute name="src">
                  <xsl:call-template name="ms-xhelp">
                    <xsl:with-param name="ref" select="'VS_logo_bk.gif'"/>
                  </xsl:call-template>
                </xsl:attribute>
                <xsl:attribute name="alt">Visual Studio 2010</xsl:attribute>
                <xsl:attribute name="title">Visual Studio 2010</xsl:attribute>
              </xsl:element>
              <xsl:element name="img" namespace="{$xhtml}">
                <xsl:attribute name="id">VSLogoHC</xsl:attribute>
                <xsl:attribute name="style">display:none</xsl:attribute>
                <xsl:attribute name="src">
                  <xsl:call-template name="ms-xhelp">
                    <xsl:with-param name="ref" select="'VS_logo_wh.gif'"/>
                  </xsl:call-template>
                </xsl:attribute>
                <xsl:attribute name="alt">Visual Studio 2010</xsl:attribute>
                <xsl:attribute name="title">Visual Studio 2010</xsl:attribute>
              </xsl:element>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

	<xsl:if test="not($error-page) and not($sp1-error-page)">
		  <xsl:call-template name="feedback-link"/>
	  </xsl:if>

    <xsl:call-template name="MTDisclaimer"/>

  </xsl:template>

  <xsl:template name="displayLeftNav">

    <xsl:if test="//TocRoot"> <!-- TocRoot present only if no &embedded=true in query string-->

      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="class">OH_leftNav</xsl:attribute>
        <xsl:attribute name="id">LeftNav</xsl:attribute>

        <xsl:element name="div" namespace="{$xhtml}">
          <xsl:attribute name="id">searchDiv</xsl:attribute>

          <xsl:call-template name="displaySearchForm">
            <xsl:with-param name="product" select="$catalogProductFamily"/>
            <xsl:with-param name="version" select="$catalogProductVersion"/>
            <xsl:with-param name="locale" select="$catalogLocale"/>
          </xsl:call-template>

        </xsl:element>

        <xsl:call-template name="displayToc"/>

      </xsl:element>

      <!-- TOC resize UI-->

      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="id">TocResize</xsl:attribute>
        <xsl:attribute name="class">OH_TocResize</xsl:attribute>
        <xsl:element name="img" namespace="{$xhtml}">
          <xsl:attribute name="id">ResizeImageIncrease</xsl:attribute>
          <xsl:attribute name="src">
            <xsl:call-template name="ms-xhelp">
              <xsl:with-param name="ref" select="'open.gif'"/>
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="onclick">onIncreaseToc()</xsl:attribute>
          <xsl:attribute name="alt">
            <xsl:value-of select="$resizeIncreaseTooltip"/>
          </xsl:attribute>
          <xsl:attribute name="title">
            <xsl:value-of select="$resizeIncreaseTooltip"/>
          </xsl:attribute>
        </xsl:element>
        <xsl:element name="img" namespace="{$xhtml}">
          <xsl:attribute name="id">ResizeImageReset</xsl:attribute>
          <xsl:attribute name="src">
            <xsl:call-template name="ms-xhelp">
              <xsl:with-param name="ref" select="'close.gif'"/>
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="style">display:none</xsl:attribute>
          <xsl:attribute name="onclick">onResetToc()</xsl:attribute>
          <xsl:attribute name="alt">
            <xsl:value-of select="$resizeResetTooltip"/>
          </xsl:attribute>
          <xsl:attribute name="title">
            <xsl:value-of select="$resizeResetTooltip"/>
          </xsl:attribute>
        </xsl:element>
      </xsl:element>

    </xsl:if>

  </xsl:template>

</xsl:stylesheet>
