<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Upload.aspx.cs" Inherits="Achive.WebPages.Upload" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:FileUpload ID="fileupload1" runat="server" text="zip..." AllowMultiple="true"/><br />
        <asp:Button ID="register" runat="server" Text="등록" OnClick="register_Click" /><br />
        <asp:Image ID="output_title_img" runat="server" /><br />
    </div>
    </form>
</body>
</html>
