<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="msxsl" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="search.xslt" />
  <xsl:variable name="searchTitle">검색 결과</xsl:variable>
  <xsl:variable name="searchButtonText">검색</xsl:variable>
  <xsl:variable name="searchSummaryTextResults">{3}에 대한 검색 결과 약 {2}개 중 {0}-{1}</xsl:variable>
  <xsl:variable name="pageButtonPrev">이전</xsl:variable>
  <xsl:variable name="pageButtonNext">다음</xsl:variable>
  <xsl:variable name="noResultsText">쿼리에 대해 결과가 검색되지 않았습니다.</xsl:variable>
</xsl:stylesheet>