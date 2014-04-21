<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xlink="http://www.w3.org/1999/xlink"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
        >
	<!-- ======================================================================================== -->

	<msxsl:script language="C#" implements-prefix="ddue">
    <msxsl:using namespace="System" />
    <msxsl:using namespace="System.Globalization"/>
    <msxsl:using namespace="System.Text.RegularExpressions" />
		<msxsl:assembly name="System.Web" />
		<msxsl:using namespace="System.Web" />
		<![CDATA[
				public static string TrimEol(string input)
				{
						return input.TrimEnd('\r','\n','\t',' ');
				}

				//Regular expression to check that a string is in a valid Guid representation.
				private static Regex guidChecker = new Regex("[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}");

				public static string GuidChecker(string id)
				{
						return guidChecker.IsMatch(id).ToString();
				}
      
				public static string IsValidDate(string dateString)
				{
						CultureInfo culture = CultureInfo.InvariantCulture;
						DateTime dt = DateTime.MinValue;
        
						try
						{
							dt = DateTime.Parse(dateString, culture);
						}
						catch (FormatException)
						{
							Console.WriteLine(string.Format("Error: IsValidDate: Unable to convert '{0}' for culture {1}.", dateString, culture.Name));
							return "false";
						}
						return "true";
				}

    ]]>
  </msxsl:script>
</xsl:stylesheet>
