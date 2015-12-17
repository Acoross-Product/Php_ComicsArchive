<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Download.aspx.cs" Inherits="Achive.WebPages.Download" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:TextBox ID="comic_id" runat="server"></asp:TextBox><br />
        <asp:Button ID="download" runat="server" Text="다운로드" OnClick="download_Click" /><br />
    </div>
    </form>
</body>
</html>
