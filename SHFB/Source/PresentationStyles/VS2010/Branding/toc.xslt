<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:template name="displayToc">
    <xsl:element name="div" namespace="{$xhtml}">
      <xsl:attribute name="id">toc</xsl:attribute>

      <xsl:if test="count(//parents)>0">
        <xsl:element name="div" namespace="{$xhtml}">
          <xsl:attribute name="id">toc_parent</xsl:attribute>
          <xsl:apply-templates select="//parents" mode="toc"/>
        </xsl:element>
      </xsl:if>

      <xsl:if test="count(//children)>0">
        <xsl:element name="div" namespace="{$xhtml}">
          <xsl:attribute name="id">toc_children</xsl:attribute>
          <xsl:apply-templates select="//children" mode="toc"/>
        </xsl:element>
      </xsl:if>

      <xsl:if test="count(//siblings)>0">
        <xsl:element name="div" namespace="{$xhtml}">
          <xsl:attribute name="id">toc_siblings</xsl:attribute>
          <xsl:apply-templates select="//siblings" mode="toc"/>
        </xsl:element>
      </xsl:if>

    </xsl:element>
  </xsl:template>

  <xsl:template match="toc" name ="toc" />
  <xsl:template match="parents" name="parents" />
  <xsl:template match="siblings" name="siblings" />
  <xsl:template match="children" name="children" />

  <xsl:template mode="toc" match="parents" name="mode-parents">
    <xsl:element name="div" namespace="{$xhtml}">
      <xsl:attribute name="class">OH_parentTocLinks</xsl:attribute>
      <xsl:apply-templates select="topic" mode="toc" />
    </xsl:element>
  </xsl:template>

  <xsl:template mode="toc" match="parents/topic | siblings/topic | children/topic" name="mode-parent-topic">
    <xsl:if test="title = $title">
      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="class">OH_selectedChild</xsl:attribute>
        <xsl:attribute name="title">
          <xsl:value-of select="title" disable-output-escaping="yes"/>
        </xsl:attribute>
        <xsl:value-of select="title" disable-output-escaping="yes"/>

      </xsl:element>
    </xsl:if>

    <xsl:if test="title != $title">
      <xsl:element name="a" namespace="{$xhtml}">
        <xsl:variable name="assetid" select="url"/>
        <xsl:attribute name="href">
          <xsl:value-of select="concat('ms.help?method=page&amp;id=', $assetid, '&amp;product=', $product, '&amp;productVersion=', $version, '&amp;locale=', $locale)"/>
        </xsl:attribute>
        <xsl:attribute name="title">
          <xsl:value-of select="title" disable-output-escaping="yes"/>
        </xsl:attribute>
        <xsl:value-of select="title" disable-output-escaping="yes"/>
      </xsl:element>
      <xsl:element name="br" namespace="{$xhtml}" />
    </xsl:if>
  </xsl:template>

  <xsl:template mode="toc" match="children" name="mode-children">
    <xsl:if test="@title = $title">
      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="class">OH_selfTocHeader</xsl:attribute>
        <xsl:value-of select="@title" disable-output-escaping="yes"/>
      </xsl:element>
    </xsl:if>

    <xsl:if test="count(topic)>0">
      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="class">OH_selfTocLinks</xsl:attribute>
        <xsl:apply-templates select="topic" mode="toc" />
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template mode="toc" match="siblings" name="mode-sibling">
    <xsl:element name="div" namespace="{$xhtml}">
      <xsl:attribute name="class">OH_siblingTocHeader</xsl:attribute>
      <xsl:value-of select="$relatedLinks"/>
    </xsl:element>
    <xsl:element name="div" namespace="{$xhtml}">
      <xsl:attribute name="class">OH_siblingTocLinks</xsl:attribute>
      <xsl:apply-templates select="topic" mode="toc" />
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>
