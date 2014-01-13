<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="msxsl" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="search.xslt" />
  <xsl:variable name="searchTitle">Risultati ricerca</xsl:variable>
  <xsl:variable name="searchButtonText">Cerca</xsl:variable>
  <xsl:variable name="searchSummaryTextResults">Risultati {0}-{1} di circa {2} per {3}</xsl:variable>
  <xsl:variable name="pageButtonPrev">indietro</xsl:variable>
  <xsl:variable name="pageButtonNext">avanti</xsl:variable>
  <xsl:variable name="noResultsText">La query non ha prodotto risultati.</xsl:variable>
</xsl:stylesheet>