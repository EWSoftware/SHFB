<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
                                 xmlns:mtps="http://msdn2.microsoft.com/mtps"
                xmlns:xhtml="http://www.w3.org/1999/xhtml"
                xmlns:branding="urn:FH-Branding"
                xmlns:xs="http://www.w3.org/2001/XMLSchema">

  <branding:MultiViewTable-js name="MultiViewTable.js"/>
  <branding:MultiViewTable-css name="MultiViewTable.css"/>


  <xsl:template match="mtps:MultiViewTable" name ="multiviewtable">
    <xsl:param name="MultiViewTable-id" select="generate-id()"/>
    <xsl:call-template name="comment-mtps"/>

    <xhtml:div class="OH_MultiViewContainerPanel">
      <xhtml:table style="border-width: 0px;border-collapse: collapse;" cellspacing="0" cellpadding="0">
        <xhtml:tbody>
          <xhtml:tr>
            <xsl:call-template name="MultiViewTable-HeaderCell">
              <xsl:with-param name="MultiViewTableHeaderA-id" select="concat($MultiViewTable-id, '_hda')"/>
            </xsl:call-template>
          </xhtml:tr>
        </xhtml:tbody>
      </xhtml:table>
      <xhtml:table class="OH_MultiViewContainerPanelDhtmlTable" style="border-width: 0px;border-collapse: collapse;" cellSpacing="0" cellPadding="0">
        <xhtml:tbody>
          <xhtml:tr>
            <xhtml:td class="OH_MultiViewTableCellListCell" style="width: 150px" valign="top">
              <xhtml:div>
                <xhtml:span class="OH_mvt_ShowTextLabel" title="Show">Show:</xhtml:span>
              </xhtml:div>

              <xsl:for-each select="descendant::mtps:MultiViewTableItem">
                <xsl:call-template name="MultiViewTable-ListButton">
                  <xsl:with-param name="BannerID" select="concat($MultiViewTable-id, '_hda')"/>
                  <xsl:with-param name="BannerText" select="attribute::ViewName"/>
                  <xsl:with-param name="Panel" select="concat($MultiViewTable-id, attribute::ID)"/>
                </xsl:call-template>
              </xsl:for-each>

            </xhtml:td>
            <xhtml:td class="OH_MultiViewTableCellContentCell" style="border-right:0px;"  vAlign="top">

              <xsl:for-each select="descendant::mtps:MultiViewTableItem">
                <xsl:call-template name="MultiViewTable-Panel">
                  <xsl:with-param name="PanelID" select="concat($MultiViewTable-id, attribute::ID)"/>
                  <xsl:with-param name="pos" select="position()"/>
                </xsl:call-template>
              </xsl:for-each>

            </xhtml:td>
          </xhtml:tr>
        </xhtml:tbody>
      </xhtml:table>
    </xhtml:div>

    <xhtml:script type="text/javascript" >
      var mvt_expImage = "MultiViewTable/ui_om_expand.gif";
      var mvt_collImage = "MultiViewTable/ui_om_collapse.gif";
      var currentDisplayPanel = "<xsl:value-of select="concat($MultiViewTable-id, descendant::mtps:MultiViewTableItem[1]/@ID)"/>";
    </xhtml:script>


  </xsl:template>

  <xsl:template name="MultiViewTable-HeaderCell">
    <xsl:param name="MultiViewTableHeaderA-id"/>

    <xsl:element name="td" namespace="{$xhtml}">
      <xsl:attribute name="class">OH_MultiViewTableHeaderCell</xsl:attribute>
      <xsl:attribute name="style">
        <xsl:value-of select="'width: 99%;border-right:0px;'" />
      </xsl:attribute>
      <xsl:attribute name="colspan">
        <xsl:value-of select="'5'" />
      </xsl:attribute>
      <xsl:element name="span" namespace="{$xhtml}">
        <xsl:attribute name="id">
          <xsl:value-of select="$MultiViewTableHeaderA-id" />
        </xsl:attribute>
        <xsl:value-of select="descendant::mtps:MultiViewTableItem[1]/@ViewName"></xsl:value-of>
      </xsl:element>
    </xsl:element>


    <xsl:element name="td" namespace="{$xhtml}">
      <xsl:attribute name="class">OH_MultiViewTableHeaderCell</xsl:attribute>
      <xsl:attribute name="style">width:1%;border-right:0px;</xsl:attribute>
      <xsl:attribute name="colSpan">5</xsl:attribute>
      <xsl:element name="a" namespace="{$xhtml}">
        <xsl:attribute name="title">
          <xsl:value-of select="'Click here to expand and collapse this table.'" />
        </xsl:attribute>
        <xsl:attribute name="style">
          <xsl:value-of select="'display:block; width: 21px; height: 18px'" />
        </xsl:attribute>
        <xsl:attribute name="id">
          <xsl:value-of select="concat($MultiViewTableHeaderA-id,'btn')" />
        </xsl:attribute>
        <xsl:attribute name="onclick">
          <xsl:text>ToggleMutliViewTable('</xsl:text>
          <xsl:value-of select= "concat($MultiViewTableHeaderA-id,'btn')"/>
          <xsl:text>')</xsl:text>
        </xsl:attribute>
        <xsl:element name="img" namespace="{$xhtml}">
          <xsl:attribute name="title">Click here to expand and collapse this table.</xsl:attribute>
          <xsl:attribute name="style">border-width:0px;</xsl:attribute>
          <xsl:attribute name="src">MultiViewTable/ui_om_expand.gif</xsl:attribute>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>


  <xsl:template name="MultiViewTable-ListButton">
    <xsl:param name="BannerID"/>
    <xsl:param name="BannerText"/>
    <xsl:param name="Panel"/>
    <xsl:element name="div" namespace="{$xhtml}">
      <xsl:attribute name="class">OH_mvt_ListBtn</xsl:attribute>
      <xsl:element name="a" namespace="{$xhtml}">
        <xsl:attribute name="href">
          <xsl:value-of select="'#'" />
        </xsl:attribute>
        <xsl:attribute name="onclick">
          <xsl:text>ShowThisTableView('</xsl:text>
          <xsl:value-of select= "$BannerID"/>
          <xsl:text>', '</xsl:text>
          <xsl:value-of select= "$BannerText"/>
          <xsl:text>', '</xsl:text>
          <xsl:value-of select= "$Panel"/>
          <xsl:text>');return false;</xsl:text>
        </xsl:attribute>
        <xsl:value-of select= "$BannerText"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>


  <xsl:template name="MultiViewTable-Panel">
    <xsl:param name="PanelID"/>
    <xsl:param name="pos"/>
    <xsl:element name="div" namespace="{$xhtml}">
      <xsl:attribute name="class">
        <xsl:if test="$pos=1">
          <xsl:value-of select="'OH_MultiViewPanelOpen'" />
        </xsl:if>
        <xsl:if test="not($pos=1)">
          <xsl:value-of select="'OH_MultiViewPanelClosed'" />
        </xsl:if>

      </xsl:attribute>
      <xsl:attribute name="id">
        <xsl:value-of select="$PanelID" />
      </xsl:attribute>
      <xsl:attribute name="style">
        <xsl:value-of select="'height: 300px'" />
      </xsl:attribute>
      <xsl:apply-templates select ="xhtml:InnerHtml"/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
