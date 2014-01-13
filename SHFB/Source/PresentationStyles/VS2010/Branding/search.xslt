<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  exclude-result-prefixes="msxsl"
  xmlns:mtps="http://msdn2.microsoft.com/mtps"
  xmlns:xhtml="http://www.w3.org/1999/xhtml"
  xmlns:opensearch="http://a9.com/-/spec/opensearch/1.1/"
  xmlns:mshelp="http://help.microsoft.com"
  xmlns:branding="urn:FH-Branding"
>
  <xsl:template match="rss" name="search-rss">
    <xsl:element name="html" namespace="{$xhtml}">
      <xsl:element name="head" namespace="{$xhtml}">

        <xsl:element name="title" namespace="{$xhtml}">
          <xsl:value-of select="$searchTitle"/>
        </xsl:element>

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

      </xsl:element>

      <xsl:element name="body" namespace="{$xhtml}">
        <xsl:attribute name="class">primary-mtps-offline-document</xsl:attribute>
        <xsl:attribute name="onload">
          <xsl:text>onLoad()</xsl:text>
        </xsl:attribute>

        <xsl:variable name="id" select="generate-id()" />

        <!-- high contrast color test-->
        <xsl:element name="span" namespace="{$xhtml}">
          <xsl:attribute name="id">HCColorTest</xsl:attribute>
          <xsl:text> </xsl:text>
        </xsl:element>

        <xsl:element name="div" namespace="{$xhtml}">
          <xsl:attribute name="class">OH_topic</xsl:attribute>

          <xsl:element name="div" namespace="{$xhtml}">
            <xsl:attribute name="class">OH_title</xsl:attribute>
            <xsl:element name="table" namespace="{$xhtml}">
              <xsl:attribute name="class">
              </xsl:attribute>
              <xsl:element name="tr" namespace="{$xhtml}">
                <xsl:element name="td" namespace="{$xhtml}">
                  <xsl:attribute name="class">OH_tdSearchTitle</xsl:attribute>
                  <xsl:value-of select="$searchTitle"/>
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

        <xsl:variable name="itemsPerPage">
          <xsl:value-of select="opensearch:itemsPerPage"/>
        </xsl:variable>
        <xsl:variable name="searchTerms">
          <xsl:value-of select="opensearch:Query/@searchTerms"/>
        </xsl:variable>
        <xsl:variable name="product">
          <xsl:choose>
            <xsl:when test="opensearch:Query/@product">
              <xsl:value-of select="opensearch:Query/@product" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'VS'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="version">
          <xsl:choose>
            <xsl:when test="opensearch:Query/@productVersion">
              <xsl:value-of select="opensearch:Query/@productVersion"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'100'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="locale">
          <xsl:choose>
            <xsl:when test="opensearch:Query/@locale">
              <xsl:value-of select="opensearch:Query/@locale"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'en-us'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:element name="div" namespace="{$xhtml}">
          <xsl:attribute name="id">mainSection</xsl:attribute>
          <xsl:element name="div" namespace="{$xhtml}">
            <xsl:attribute name="id">mainBody</xsl:attribute>

            <xsl:call-template name="displaySearchForm">
              <xsl:with-param name="searchTextboxValue" select="branding:GetUrlDecode($searchTerms)"/>
              <xsl:with-param name="searchButtonValue" select="$searchButtonText"/>
              <xsl:with-param name="itemsPerPage" select="$itemsPerPage"/>
              <xsl:with-param name="product" select="$product"/>
              <xsl:with-param name="version" select="$version"/>
              <xsl:with-param name="locale" select="$locale"/>
            </xsl:call-template>

            <xsl:variable name="pageNumber" select="ceiling(opensearch:startIndex div $itemsPerPage)"/>
            <xsl:variable name="maxPageNumber" select="ceiling(opensearch:totalResults div $itemsPerPage)"/>

            <xsl:choose>
              <xsl:when test="opensearch:startIndex">
                <!-- display search results statistics and page navigation -->
                <xsl:element name="div" namespace="{$xhtml}">
                  <xsl:element name="p" namespace="{$xhtml}">
                    <xsl:call-template name="tokens">
                      <xsl:with-param name="str" select="$searchSummaryTextResults" />
                      <xsl:with-param name="fromPage" select="opensearch:startIndex" />
                      <xsl:with-param name="currentPage" select="$pageNumber" />
                      <xsl:with-param name="itemPerPage" select="$itemsPerPage" />
                      <xsl:with-param name="totalResult" select="opensearch:totalResults" />
                      <xsl:with-param name="searchTerm" select="branding:GetUrlDecode($searchTerms)" />
                    </xsl:call-template>
                    
                    <!-- search paging navigation-->
                    <xsl:element name="span" namespace="{$xhtml}">
                      <xsl:attribute name="class">OH_searchPaging</xsl:attribute>

                      <!-- previous button-->
                      <xsl:if test="$pageNumber != 1">
                        <xsl:element name="span" namespace="{$xhtml}">
                          <xsl:attribute name="class">OH_resultNavLink</xsl:attribute>
                          <xsl:element name="a" namespace="{$xhtml}">
                            <xsl:attribute name="href">
                              <xsl:call-template name="buildSearchUrl">
                                <xsl:with-param name="page" select="$pageNumber - 1"/>
                                <xsl:with-param name="searchTerms" select="branding:GetUrlEncode($searchTerms)" />
                                <xsl:with-param name="itemsPerPage" select="$itemsPerPage"/>
                                <xsl:with-param name="product" select="$product"/>
                                <xsl:with-param name="version" select="$version"/>
                                <xsl:with-param name="locale" select="$locale"/>
                              </xsl:call-template>
                            </xsl:attribute>
                            <xsl:attribute name="title">
                              <xsl:value-of select="$pageButtonPrev"/>
                            </xsl:attribute>
                            <xsl:value-of select="$pageButtonPrev"/>
                          </xsl:element>
                        </xsl:element>
                      </xsl:if>

                      <!-- page buttons-->
                      <xsl:call-template name="displayPageButtons">
                        <xsl:with-param name="current" select="$pageNumber - 4"/>
                        <xsl:with-param name="pageNumber" select="$pageNumber"/>
                        <xsl:with-param name="maxPageNumber" select="$maxPageNumber"/>
                        <xsl:with-param name="searchTerms" select="$searchTerms"/>
                        <xsl:with-param name="itemsPerPage" select="$itemsPerPage"/>
                        <xsl:with-param name="product" select="$product"/>
                        <xsl:with-param name="version" select="$version"/>
                        <xsl:with-param name="locale" select="$locale"/>
                      </xsl:call-template>

                      <!-- next button-->
                      <xsl:if test="$pageNumber != $maxPageNumber">
                        <xsl:element name="span" namespace="{$xhtml}">
                          <xsl:attribute name="class">OH_resultNavLink</xsl:attribute>
                          <xsl:element name="a" namespace="{$xhtml}">
                            <xsl:attribute name="href">
                              <xsl:call-template name="buildSearchUrl">
                                <xsl:with-param name="page" select="$pageNumber + 1"/>
                                <xsl:with-param name="searchTerms" select="branding:GetUrlEncode($searchTerms)"/>
                                <xsl:with-param name="itemsPerPage" select="$itemsPerPage"/>
                                <xsl:with-param name="product" select="$product"/>
                                <xsl:with-param name="version" select="$version"/>
                                <xsl:with-param name="locale" select="$locale"/>
                              </xsl:call-template>
                            </xsl:attribute>
                            <xsl:attribute name="title">
                              <xsl:value-of select="$pageButtonNext"/>
                            </xsl:attribute>
                            <xsl:value-of select="$pageButtonNext"/>
                          </xsl:element>
                        </xsl:element>
                      </xsl:if>

                    </xsl:element>

                  </xsl:element>
                </xsl:element>

                <!-- display the search result items -->
                <xsl:element name="div" namespace="{$xhtml}">
                  <xsl:for-each select="channel/item">
                    <xsl:element name="p" namespace="{$xhtml}">

                      <xsl:element name="a" namespace="{$xhtml}">
                        <xsl:attribute name="href">
                          <xsl:value-of select="link"/>
                        </xsl:attribute>
                        <xsl:attribute name="title">
                          <xsl:value-of select="title"/>
                        </xsl:attribute>
                        <xsl:value-of select="branding:GetHtmlDecode(title)"/>
                      </xsl:element>
                      <xsl:element name="br" />

                      <xsl:value-of select="description"/>
                    </xsl:element>
                  </xsl:for-each>
                </xsl:element>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$noResultsText"/>
              </xsl:otherwise>
            </xsl:choose>

          </xsl:element>
        </xsl:element>

        <xsl:call-template name="footer"/>

      </xsl:element>
    </xsl:element>

  </xsl:template>

  <xsl:template match="xhtml:rss" name="f1-search-rss" mode="f1-error">
   
        <xsl:variable name="itemsPerPage">
          <xsl:value-of select="opensearch:itemsPerPage"/>
        </xsl:variable>
        <xsl:variable name="searchTerms">
          <xsl:value-of select="opensearch:Query/@searchTerms"/>
        </xsl:variable>
        <xsl:variable name="product">
          <xsl:choose>
            <xsl:when test="opensearch:Query/@product">
              <xsl:value-of select="opensearch:Query/@product" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'VS'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="version">
          <xsl:choose>
            <xsl:when test="opensearch:Query/@productVersion">
              <xsl:value-of select="opensearch:Query/@productVersion"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'100'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="locale">
          <xsl:choose>
            <xsl:when test="opensearch:Query/@locale">
              <xsl:value-of select="opensearch:Query/@locale"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'en-us'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

       
            <xsl:call-template name="displaySearchForm">
              <xsl:with-param name="searchTextboxValue" select="branding:GetUrlDecode($searchTerms)"/>
              <xsl:with-param name="searchButtonValue" select="$searchButtonText"/>
              <xsl:with-param name="itemsPerPage" select="$itemsPerPage"/>
              <xsl:with-param name="product" select="$product"/>
              <xsl:with-param name="version" select="$version"/>
              <xsl:with-param name="locale" select="$locale"/>
            </xsl:call-template>

            <xsl:variable name="pageNumber" select="ceiling(opensearch:startIndex div $itemsPerPage)"/>
            <xsl:variable name="maxPageNumber" select="ceiling(opensearch:totalResults div $itemsPerPage)"/>

            <xsl:choose>
              <xsl:when test="opensearch:startIndex">
                <!-- display search results statistics and page navigation -->
                <xsl:element name="div" namespace="{$xhtml}">
                  <xsl:element name="p" namespace="{$xhtml}">
                    <xsl:call-template name="tokens">
                      <xsl:with-param name="str" select="$searchSummaryTextResults" />
                      <xsl:with-param name="fromPage" select="opensearch:startIndex" />
                      <xsl:with-param name="currentPage" select="$pageNumber" />
                      <xsl:with-param name="itemPerPage" select="$itemsPerPage" />
                      <xsl:with-param name="totalResult" select="opensearch:totalResults" />
                      <xsl:with-param name="searchTerm" select="branding:GetUrlDecode($searchTerms)" />
                    </xsl:call-template>

                    <!-- search paging navigation-->
                    <xsl:element name="span" namespace="{$xhtml}">
                      <xsl:attribute name="class">OH_searchPaging</xsl:attribute>

                      <!-- previous button-->
                      <xsl:if test="$pageNumber != 1">
                        <xsl:element name="span" namespace="{$xhtml}">
                          <xsl:attribute name="class">OH_resultNavLink</xsl:attribute>
                          <xsl:element name="a" namespace="{$xhtml}">
                            <xsl:attribute name="href">
                              <xsl:call-template name="buildSearchUrl">
                                <xsl:with-param name="page" select="$pageNumber - 1"/>
                                <xsl:with-param name="searchTerms" select="branding:GetUrlEncode($searchTerms)" />
                                <xsl:with-param name="itemsPerPage" select="$itemsPerPage"/>
                                <xsl:with-param name="product" select="$product"/>
                                <xsl:with-param name="version" select="$version"/>
                                <xsl:with-param name="locale" select="$locale"/>
                              </xsl:call-template>
                            </xsl:attribute>
                            <xsl:attribute name="title">
                              <xsl:value-of select="$pageButtonPrev"/>
                            </xsl:attribute>
                            <xsl:value-of select="$pageButtonPrev"/>
                          </xsl:element>
                        </xsl:element>
                      </xsl:if>

                      <!-- page buttons-->
                      <xsl:call-template name="displayPageButtons">
                        <xsl:with-param name="current" select="$pageNumber - 4"/>
                        <xsl:with-param name="pageNumber" select="$pageNumber"/>
                        <xsl:with-param name="maxPageNumber" select="$maxPageNumber"/>
                        <xsl:with-param name="searchTerms" select="$searchTerms"/>
                        <xsl:with-param name="itemsPerPage" select="$itemsPerPage"/>
                        <xsl:with-param name="product" select="$product"/>
                        <xsl:with-param name="version" select="$version"/>
                        <xsl:with-param name="locale" select="$locale"/>
                      </xsl:call-template>

                      <!-- next button-->
                      <xsl:if test="$pageNumber != $maxPageNumber">
                        <xsl:element name="span" namespace="{$xhtml}">
                          <xsl:attribute name="class">OH_resultNavLink</xsl:attribute>
                          <xsl:element name="a" namespace="{$xhtml}">
                            <xsl:attribute name="href">
                              <xsl:call-template name="buildSearchUrl">
                                <xsl:with-param name="page" select="$pageNumber + 1"/>
                                <xsl:with-param name="searchTerms" select="branding:GetUrlEncode($searchTerms)"/>
                                <xsl:with-param name="itemsPerPage" select="$itemsPerPage"/>
                                <xsl:with-param name="product" select="$product"/>
                                <xsl:with-param name="version" select="$version"/>
                                <xsl:with-param name="locale" select="$locale"/>
                              </xsl:call-template>
                            </xsl:attribute>
                            <xsl:attribute name="title">
                              <xsl:value-of select="$pageButtonNext"/>
                            </xsl:attribute>
                            <xsl:value-of select="$pageButtonNext"/>
                          </xsl:element>
                        </xsl:element>
                      </xsl:if>

                    </xsl:element>

                  </xsl:element>
                </xsl:element>

                <!-- display the search result items -->
                <xsl:element name="div" namespace="{$xhtml}">
                  <xsl:for-each select="xhtml:channel/xhtml:item" >
                    <xsl:element name="p" namespace="{$xhtml}">

                      <xsl:element name="a" namespace="{$xhtml}">
                        <xsl:attribute name="href">
                          <xsl:value-of select="xhtml:link/node()[1]"/>
                        </xsl:attribute>
                        <xsl:attribute name="title">
                          <xsl:value-of select="xhtml:title/node()[1]"/>
                        </xsl:attribute>
                        <xsl:value-of select="branding:GetHtmlDecode(xhtml:title/node()[1])"/>
                      </xsl:element>
                      <xsl:element name="br" />

                      <xsl:value-of select="xhtml:description/node()[1]"/>
                    </xsl:element>
                  </xsl:for-each>
                </xsl:element>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$noResultsText"/>
              </xsl:otherwise>
            </xsl:choose>

         

  </xsl:template>

  <xsl:template name="displaySearchForm">
    <xsl:param name="searchTextboxValue" select="''"/>
    <xsl:param name="searchButtonValue" select="''"/>
    <xsl:param name="itemsPerPage" select="'10'"/>
    <xsl:param name="product" select="'vs'"/>
    <xsl:param name="version" select="'100'"/>
    <xsl:param name="locale" select="'en-us'"/>

    <xsl:element name="form" namespace="{$xhtml}">
      <xsl:attribute name="action">ms.help</xsl:attribute>
      <xsl:element name="input" namespace="{$xhtml}">
        <xsl:attribute name="name">method</xsl:attribute>
        <xsl:attribute name="type">hidden</xsl:attribute>
        <xsl:attribute name="value">search</xsl:attribute>
      </xsl:element>

      <xsl:element name="div" namespace="{$xhtml}">
        <xsl:attribute name="id">searchBox</xsl:attribute>

        <xsl:element name="input" namespace="{$xhtml}">
          <xsl:attribute name="id">qu</xsl:attribute>
          <xsl:attribute name="name">query</xsl:attribute>
          <xsl:attribute name="class">OH_searchTextbox</xsl:attribute>
          <xsl:attribute name="size">29</xsl:attribute>
          <xsl:attribute name="maxlength">1000</xsl:attribute>
          <xsl:attribute name="title">
            <xsl:value-of select="$searchButtonText"/>
          </xsl:attribute>
          <xsl:attribute name="value">
            <xsl:value-of select="$searchTextboxValue"/>
          </xsl:attribute>
          <xsl:attribute name="onfocus">
            <xsl:text><![CDATA[if (this.value=='' && !isSearchPage){document.getElementById('searchWatermark').style.display='none';}]]></xsl:text>
          </xsl:attribute>
          <xsl:attribute name="onblur">
            <xsl:text><![CDATA[if (this.value=='' && !isSearchPage){document.getElementById('searchWatermark').style.display='inline';}]]></xsl:text>
          </xsl:attribute>
          <xsl:attribute name="onkeyup">return SearchTextboxKeyUp(event)</xsl:attribute>
        </xsl:element>
        <xsl:element name="div" namespace="{$xhtml}">
          <xsl:attribute name="id">searchWatermark</xsl:attribute>
          <xsl:attribute name="class">OH_searchWatermark</xsl:attribute>
          <xsl:attribute name="onclick">
            <xsl:text><![CDATA[document.getElementById('searchWatermark').style.display='none';document.getElementById('qu').focus();]]></xsl:text>
          </xsl:attribute>
          <xsl:value-of select="$searchButtonText"/>
        </xsl:element>
        <xsl:element name="button" namespace="{$xhtml}">
          <xsl:attribute name="id">btnS</xsl:attribute>
          <xsl:attribute name="name">btnS</xsl:attribute>
          <xsl:attribute name="type">submit</xsl:attribute>
          <xsl:attribute name="title">
            <xsl:value-of select="$searchButtonText"/>
          </xsl:attribute>
          <xsl:choose>
            <xsl:when test="$searchButtonValue != ''">
              <xsl:value-of select="$searchButtonValue"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:element name="img" namespace="{$xhtml}">
                <xsl:attribute name="id">searchImage</xsl:attribute>
                <xsl:attribute name="class">OH_searchImage</xsl:attribute>
                <xsl:attribute name="src">
                  <xsl:call-template name="ms-xhelp">
                    <xsl:with-param name="ref" select="'search.gif'"/>
                  </xsl:call-template>
                </xsl:attribute>
                <xsl:attribute name="alt">
                  <xsl:value-of select="$searchButtonText"/>
                </xsl:attribute>
                <xsl:attribute name="title">
                  <xsl:value-of select="$searchButtonText"/>
                </xsl:attribute>
              </xsl:element>
              <xsl:element name="img" namespace="{$xhtml}">
                <xsl:attribute name="id">searchImageHC</xsl:attribute>
                <xsl:attribute name="class">OH_searchImage</xsl:attribute>
                <xsl:attribute name="style">display:none</xsl:attribute>
                <xsl:attribute name="src">
                  <xsl:call-template name="ms-xhelp">
                    <xsl:with-param name="ref" select="'search_bk.gif'"/>
                  </xsl:call-template>
                </xsl:attribute>
                <xsl:attribute name="alt">
                  <xsl:value-of select="$searchButtonText"/>
                </xsl:attribute>
                <xsl:attribute name="title">
                  <xsl:value-of select="$searchButtonText"/>
                </xsl:attribute>
              </xsl:element>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:element>
      </xsl:element>

      <xsl:element name="input" namespace="{$xhtml}">
        <xsl:attribute name="name">PageSize</xsl:attribute>
        <xsl:attribute name="type">hidden</xsl:attribute>
        <xsl:attribute name="value">
          <xsl:value-of select="$itemsPerPage"/>
        </xsl:attribute>
      </xsl:element>
      <xsl:element name="input" namespace="{$xhtml}">
        <xsl:attribute name="name">PageNumber</xsl:attribute>
        <xsl:attribute name="type">hidden</xsl:attribute>
        <xsl:attribute name="value">1</xsl:attribute>
      </xsl:element>
      <xsl:element name="input" namespace="{$xhtml}">
        <xsl:attribute name="name">locale</xsl:attribute>
        <xsl:attribute name="type">hidden</xsl:attribute>
        <xsl:attribute name="value">
          <xsl:value-of select="$locale"/>
        </xsl:attribute>
      </xsl:element>
      <xsl:element name="input" namespace="{$xhtml}">
        <xsl:attribute name="name">ProductVersion</xsl:attribute>
        <xsl:attribute name="type">hidden</xsl:attribute>
        <xsl:attribute name="value">
          <xsl:value-of select="$version"/>
        </xsl:attribute>
      </xsl:element>
      <xsl:element name="input" namespace="{$xhtml}">
        <xsl:attribute name="name">Product</xsl:attribute>
        <xsl:attribute name="type">hidden</xsl:attribute>
        <xsl:attribute name="value">
          <xsl:value-of select="$product"/>
        </xsl:attribute>
      </xsl:element>
      <xsl:if test="$launchingApp='VS'">
        <xsl:element name="input" namespace="{$xhtml}">
          <xsl:attribute name="name">launchingApp</xsl:attribute>
          <xsl:attribute name="type">hidden</xsl:attribute>
          <xsl:attribute name="value">
            <xsl:value-of select="$launchingApp"/>
          </xsl:attribute>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <!-- recursively render the page buttons, at most 4 buttons before current page and 4 after-->
  <xsl:template name="displayPageButtons">
    <xsl:param name="current"/>
    <xsl:param name="pageNumber"/>
    <xsl:param name="maxPageNumber"/>
    <xsl:param name="searchTerms"/>
    <xsl:param name="itemsPerPage" select="'10'"/>
    <xsl:param name="product" select="'vs'"/>
    <xsl:param name="version" select="'100'"/>
    <xsl:param name="locale" select="'en-us'"/>
    <xsl:if test="($current &lt;= $pageNumber + 4) and ($current &lt;= $maxPageNumber)">
      <xsl:if test="$current &gt;= 1">
        <xsl:element name="span" namespace="{$xhtml}">
          <xsl:choose>
            <xsl:when test="$current = $pageNumber">
              <xsl:attribute name="class">OH_currentPage</xsl:attribute>
              <xsl:element name="a" namespace="{$xhtml}">
                <xsl:attribute name="title">
                  <xsl:value-of select="$current"/>
                </xsl:attribute>
                <xsl:value-of select="$current"/>
              </xsl:element>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="class">OH_resultLink</xsl:attribute>
              <xsl:element name="a" namespace="{$xhtml}">
                <xsl:attribute name="href">
                  <xsl:call-template name="buildSearchUrl">
                    <xsl:with-param name="page" select="$current"/>
                    <xsl:with-param name="searchTerms" select="$searchTerms"/>
                    <xsl:with-param name="itemsPerPage" select="$itemsPerPage"/>
                    <xsl:with-param name="product" select="$product"/>
                    <xsl:with-param name="version" select="$version"/>
                    <xsl:with-param name="locale" select="$locale"/>
                  </xsl:call-template>
                </xsl:attribute>
                <xsl:attribute name="title">
                  <xsl:value-of select="$current"/>
                </xsl:attribute>
                <xsl:value-of select="$current"/>
              </xsl:element>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:element>
      </xsl:if>
      <xsl:call-template name="displayPageButtons">
        <xsl:with-param name="current" select="$current + 1"/>
        <xsl:with-param name="pageNumber" select="$pageNumber"/>
        <xsl:with-param name="maxPageNumber" select="$maxPageNumber"/>
        <xsl:with-param name="searchTerms" select="$searchTerms"/>
        <xsl:with-param name="itemsPerPage" select="$itemsPerPage"/>
        <xsl:with-param name="product" select="$product"/>
        <xsl:with-param name="version" select="$version"/>
        <xsl:with-param name="locale" select="$locale"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="buildSearchUrl">
    <xsl:param name="page"/>
    <xsl:param name="searchTerms"/>
    <xsl:param name="itemsPerPage" select="'10'"/>
    <xsl:param name="product" select="'vs'"/>
    <xsl:param name="version" select="'100'"/>
    <xsl:param name="locale" select="'en-us'"/>
    <xsl:value-of select="concat('ms.help?method=search&amp;query=',branding:GetUrlEncode($searchTerms),'&amp;PageSize=',$itemsPerPage,'&amp;PageNumber=',$page,'&amp;Product=',$product,'&amp;ProductVersion=',$version,'&amp;locale=',$locale)"/>
  </xsl:template>
  
  <xsl:template name="tokens">
    <xsl:param name="str" />
    <xsl:param name="fromPage" />
    <xsl:param name="currentPage" />
    <xsl:param name="itemPerPage" />
    <xsl:param name="totalResult" />
    <xsl:param name="searchTerm" />
    <xsl:choose>
      
      <xsl:when test="contains($str,'}') ">
        <xsl:variable name="tempString" select="substring-before($str,'}')" />
        <xsl:value-of select="substring-before($tempString,'{')"/>
        <xsl:choose>
          <xsl:when test ="substring-after($tempString,'{')='0'">
            <xsl:element name="span" namespace="{$xhtml}">
              <xsl:attribute name="class">OH_searchCount</xsl:attribute>
              <xsl:value-of select="$fromPage"/>
            </xsl:element>
          </xsl:when>
          <xsl:when test ="substring-after($tempString,'{')='1'">
            <xsl:element name="span" namespace="{$xhtml}">
              <xsl:attribute name="class">OH_searchCount</xsl:attribute>
              <xsl:choose>
                <xsl:when test="number($currentPage * $itemPerPage) &lt;=  number($totalResult)">
                  <xsl:value-of select="number($currentPage * $itemPerPage)"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="number($totalResult)"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:element>
          </xsl:when>
          <xsl:when test ="substring-after($tempString,'{')='2'">
            <xsl:element name="span" namespace="{$xhtml}">
              <xsl:attribute name="class">OH_searchCount</xsl:attribute>
              <xsl:value-of select="$totalResult"/>
            </xsl:element>
          </xsl:when>
          <xsl:when test ="substring-after($tempString,'{')='3'">
            <xsl:element name="span" namespace="{$xhtml}">
              <xsl:attribute name="class">OH_searchTerm</xsl:attribute>
              <xsl:value-of select="$searchTerm"/>
            </xsl:element>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="substring-after($tempString,'{')"/>
          </xsl:otherwise>
        </xsl:choose>
         
        <xsl:call-template name="tokens">
          <xsl:with-param name="str" select="substring-after($str,'}')"/>
          <xsl:with-param name="fromPage" select="$fromPage"/>
          <xsl:with-param name="currentPage" select="$currentPage"/>
          <xsl:with-param name="itemPerPage" select="$itemPerPage"/>
          <xsl:with-param name="totalResult" select="$totalResult"/>
          <xsl:with-param name="searchTerm" select="$searchTerm"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
          <xsl:value-of select="$str"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  
</xsl:stylesheet>
