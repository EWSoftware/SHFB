<%@ Page validateRequest="false" language="c#" Inherits="TextColorizerTest.WebForm1" CodeFile="Default.aspx.cs" %>
<%@ Register TagPrefix="cc1" Namespace="ColorizerLibrary.Controls" Assembly="ColorizerLibrary" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
<title>Colorizer Test</title>
<meta content="Microsoft Visual Studio 7.0" name="GENERATOR">
<meta content="C#" name="CODE_LANGUAGE">
<meta content="JavaScript" name="vs_defaultClientScript">
<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
<link href="highlight.css" type="text/css" rel="stylesheet">
<script type="text/javascript" src="highlight.js"></script>
<style>body { font-family: verdana, arial, sans-serif; font-size: 8.5pt; }</style>
</head>

<body>

<form id="Form1" method="post" runat="server">
<h3>Syntax Highlighting C# Control Demonstration</h3>
<ul>
    <li>Encapsulate HTML encoded source code in <b>pre</b> and <b>code</b>
tags.</li>
    <li>Use the <b>lang</b> parameter to specify the language:
        <ul>
            <li>cs = C#</li>
			<li>cpp = C++</li>
            <li>c = C</li>
            <li>fs = F#</li>
            <li>javascript = JavaScript</li>
            <li>jscriptnet = JScript.NET</li>
            <li>vbnet = VB.NET</li>
            <li>vbscript = VBScript</li>
            <li>jsharp = J#</li>
            <li>xml = XML</li>
            <li>xaml = XAML</li>
            <li>sql = SQL script</li>
            <li>pshell = PowerShell script</li>
            <li>py = Python</li>
            <li>Anything else = No highlighting</li>
        </ul></li>
</ul>

Example:
<pre>
&lt;pre lang="cpp"&gt;
/*
Testing the colorzing
*/
#define A_CONSTANT

using namespace std;

int main( int argc, char* argv[])
{
	const char* szHello="Hello world";
	cout&lt;&lt;szHello;

	// returning 0
	return 0;
}
&lt;/pre&gt;
</pre>

<p /><asp:CheckBox ID="chkNumberLines" runat="server" Text="Number lines"
    Checked="true" />
&nbsp;&nbsp;&nbsp;<asp:CheckBox ID="chkOutlining" runat="server"
    Text="Enable outlining of #region and #if/#else/#endif blocks"
    Checked="true" />
&nbsp;&nbsp;&nbsp;<asp:CheckBox ID="chkDefaultTitle" runat="server"
    Text="Use default title if none specified" Checked="true" />
<br /><asp:CheckBox ID="chkKeepSeeTags" runat="server"
    Text="Keep XML comment 'see' tags that appear in the code" />
    (NOTE: When enabled, &lt;see&gt; tags are output as normal HTML tags and
    will not be visible in the rendered text if they are self-closing.)

<p />Tab size override: <asp:TextBox ID="txtTabSize" runat="server"
    Columns="2" Text="0" />

<p />Tag: <asp:DropDownList ID="cboTag" runat="server" >
    <asp:ListItem Value="pre">pre - Multi-line</asp:ListItem>
    <asp:ListItem Value="code">code - Single line</asp:ListItem>
</asp:DropDownList> lang=
<asp:DropDownList ID="cboLanguage" runat="server" >
    <asp:ListItem Value="cs">cs - C#</asp:ListItem>
    <asp:ListItem Value="cpp">cpp - C++</asp:ListItem>
    <asp:ListItem Value="c">c - C</asp:ListItem>
    <asp:ListItem Value="fs">fs - F#</asp:ListItem>
    <asp:ListItem Value="javascript">javascript - JavaScript</asp:ListItem>
    <asp:ListItem Value="jscript.net">jscriptnet - JScript.NET</asp:ListItem>
    <asp:ListItem Value="vbnet">vbnet - VB.NET</asp:ListItem>
    <asp:ListItem Value="vbscript">vbscript - VBScript</asp:ListItem>
    <asp:ListItem Value="jsharp">jsharp - J#</asp:ListItem>
    <asp:ListItem Value="xml">xml - XML</asp:ListItem>
    <asp:ListItem Value="xaml">xaml - XAML</asp:ListItem>
    <asp:ListItem Value="sql">sql - SQL script</asp:ListItem>
    <asp:ListItem Value="pshell">pshell - PowerShell script</asp:ListItem>
    <asp:ListItem Value="py">py - Python</asp:ListItem>
    <asp:ListItem Value="None">None - No language</asp:ListItem>
</asp:DropDownList>

<p />Optional Title: <asp:TextBox ID="txtTitle" runat="server" Columns="50" />

<p />Tag Content:
<p /><asp:TextBox ID="txtContent" runat="server" Rows="14"
    Columns="77" TextMode="MultiLine" />

<p /><asp:Button ID="btnProcess" runat="server" Text="Process" OnClick="btnProcess_Click" />

<p /><cc1:TextColorizerControl id="colorizerControl" runat="server" />

</form>

</body>
</html>
