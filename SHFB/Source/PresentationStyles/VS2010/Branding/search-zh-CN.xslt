<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="msxsl" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="search.xslt" />
  <xsl:variable name="searchTitle">搜索结果</xsl:variable>
  <xsl:variable name="searchButtonText">搜索</xsl:variable>
  <xsl:variable name="searchSummaryTextResults">搜索 {3} 获得约 {2} 条结果，此处是第 {0}-{1} 条</xsl:variable>
  <xsl:variable name="pageButtonPrev">上一条</xsl:variable>
  <xsl:variable name="pageButtonNext">下一条</xsl:variable>
  <xsl:variable name="noResultsText">未找到任何符合您查询条件的结果。</xsl:variable>
</xsl:stylesheet>