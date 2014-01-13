<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
                xmlns:xhtml="http://www.w3.org/1999/xhtml"
>

  <xsl:template match="@* | node()" name="identity">
      <xsl:copy>
        <xsl:apply-templates select="@* | node()"/>
        <xsl:if test="not(*) and not(text()) and not(self::xhtml:br) and not(self::xhtml:hr)">
          <xsl:value-of select="' '"/>
        </xsl:if>
      </xsl:copy>
  </xsl:template>

  <xsl:template match="@* | node()" name="identity-self-branding" mode="self-branding">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" mode="self-branding"/>
      <xsl:if test="not(*) and not(text()) and not(self::xhtml:br) and not(self::xhtml:hr)">
        <xsl:value-of select="' '"/>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
