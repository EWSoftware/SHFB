<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

	<xsl:output indent="yes" encoding="UTF-8" />

	<xsl:key name="index" match="/reflection/apis/api" use="containers/namespace/@api" />

	<xsl:template match="/">
		<topics>
			<xsl:for-each select="/reflection/apis/api">
				<xsl:if test="not(topicdata/@notopic)">
					<topic id="{@id}" type="API"/>
				</xsl:if>
			</xsl:for-each>
		</topics>
	</xsl:template>

</xsl:stylesheet>
