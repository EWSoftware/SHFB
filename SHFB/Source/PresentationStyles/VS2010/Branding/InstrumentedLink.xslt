<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
                                 xmlns:mtps="http://msdn2.microsoft.com/mtps"
                xmlns:xhtml="http://www.w3.org/1999/xhtml"
                xmlns:branding="urn:FH-Branding"
                xmlns:xs="http://www.w3.org/2001/XMLSchema">

  <xsl:template match="xhtml:a" name="insLink">
    <xsl:variable name="assetid" select="substring-after(translate(@href, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ'), '?ID=')"/>
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
      <xsl:if test="@id">
        <xsl:attribute name="id">
          <xsl:value-of select="@id"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@href">
        <xsl:attribute name="href">
          <xsl:choose>
			  <xsl:when test="@href='ms-xhelp:///?id=helponhelp.htm'">
				  <xsl:call-template name="ms-xhelp">
					  <xsl:with-param name="ref" select="$contentnotfound"/>
				  </xsl:call-template>
			  </xsl:when>
			  <xsl:when test="@href='install'">
				  <xsl:value-of select="concat('ms-xhelp:///?install=2','&amp;product=', $product, '&amp;version=', $version,'&amp;locale=', $locale)"/>
			  </xsl:when>
			  <xsl:when test="@href='install_setting'">
				  <xsl:value-of select="concat('ms-xhelp:///?install=2','&amp;product=', $product, '&amp;version=', $version,'&amp;locale=', $locale,'&amp;settings=')"/>
			  </xsl:when>
			  <xsl:when test="@class='mtps-external-link' or starts-with(@href,'#') or starts-with(@href,'http:') or starts-with(@href,'https') or starts-with(@href,'www') or starts-with(@href,'mailto')">
              <!-- external link or anchor-->
              <xsl:value-of select="@href"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="concat('ms.help?method=page&amp;id=', $assetid, '&amp;product=', $product, '&amp;productVersion=', $version,'&amp;topicVersion=', $topicVersion, '&amp;locale=', $locale,'&amp;topicLocale=', $topiclocale)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name="target">
          <xsl:choose>
            <xsl:when test="@class='mtps-external-link' or starts-with(@href,'http:') or starts-with(@href,'https') or starts-with(@href,'www') or starts-with(@href,'mailto')">
              <!-- external link -->
              <xsl:value-of select="'_blank'"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
     </xsl:if>
      <xsl:apply-templates />
      <xsl:if test="not(*) and not(text())">
        <xsl:comment/>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xhtml:a" mode="self-branding" name="insLink-self-branding">
    <xsl:variable name="assetid" select="branding:GetID(@href)"/>
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="self-branding"/>
      <xsl:if test="@href">
        <xsl:attribute name="href">
          <xsl:choose>
            <xsl:when test="starts-with(@href,'http:') or starts-with(@href,'https:') or starts-with(@href,'#') or starts-with(@href,'www') or starts-with(@href,'mailto')">
              <!-- external link or anchor-->
              <xsl:value-of select="@href"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
              <xsl:when test="$assetid">
                <xsl:value-of select="concat('ms.help?method=page&amp;id=', $assetid, '&amp;product=', $product, '&amp;productVersion=', $version, '&amp;topicVersion=', $topicVersion,'&amp;locale=', $locale,'&amp;topicLocale=', $topiclocale)"/>
              </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="@href"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name="target">
          <xsl:choose>
            <xsl:when test="starts-with(@href,'http:') or starts-with(@href,'https') or starts-with(@href,'www') or starts-with(@href,'mailto')">
              <!-- external link -->
              <xsl:value-of select="'_blank'"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates mode="self-branding"/>
      <xsl:if test="not(*) and not(text())">
        <xsl:comment/>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>
