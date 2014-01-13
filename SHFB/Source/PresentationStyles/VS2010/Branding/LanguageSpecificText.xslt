<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:mtps="http://msdn2.microsoft.com/mtps"
                xmlns:branding="urn:FH-Branding"
                xmlns:xhtml="http://www.w3.org/1999/xhtml">


  <branding:LanguageSpecificText-js name="LanguageSpecificText.js"/>
  <xsl:template match="mtps:LanguageSpecificText" name="lst">
    <xsl:variable name="id" select="generate-id()" />
    <xsl:element name="span" namespace="{$xhtml}">
      <xsl:attribute name="id">
        <xsl:value-of select="$id"/>
      </xsl:attribute>
     <xsl:value-of select="''" />
    </xsl:element>
    <xsl:element name="script" namespace="{$xhtml}">
      <xsl:text>addToLanSpecTextIdSet('</xsl:text>
      <xsl:value-of select="concat($id, '?', 'cs=', @devLangcs, '|vb=', @devLangvb, '|cpp=', @devLangcpp, '|nu=', @devLangnu)"/>
      <xsl:text>');</xsl:text>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>