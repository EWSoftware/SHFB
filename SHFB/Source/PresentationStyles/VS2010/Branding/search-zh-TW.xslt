<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="msxsl" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="search.xslt" />
  <xsl:variable name="searchTitle">搜尋結果</xsl:variable>
  <xsl:variable name="searchButtonText">搜尋</xsl:variable>
  <xsl:variable name="searchSummaryTextResults">{0}-{1} 筆結果，共約 {2} 筆，屬於 {3}</xsl:variable>
  <xsl:variable name="pageButtonPrev">上一個</xsl:variable>
  <xsl:variable name="pageButtonNext">下一個</xsl:variable>
  <xsl:variable name="noResultsText">找不到您查詢的任何結果。</xsl:variable>
</xsl:stylesheet>