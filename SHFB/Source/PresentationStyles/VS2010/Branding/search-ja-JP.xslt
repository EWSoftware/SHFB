<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="msxsl" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="search.xslt" />
  <xsl:variable name="searchTitle">検索結果</xsl:variable>
  <xsl:variable name="searchButtonText">検索</xsl:variable>
  <xsl:variable name="searchSummaryTextResults">{3} の検索結果、約 {2} 件中 {0}-{1} 件</xsl:variable>
  <xsl:variable name="pageButtonPrev">前へ</xsl:variable>
  <xsl:variable name="pageButtonNext">次へ</xsl:variable>
  <xsl:variable name="noResultsText">クエリの結果は見つかりませんでした。</xsl:variable>
</xsl:stylesheet>