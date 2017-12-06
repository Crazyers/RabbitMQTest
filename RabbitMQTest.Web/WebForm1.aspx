<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="RabbitMQTest.Web.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button ID="btnGenerateNo" runat="server" Text="只生成单号" OnClick="btnGenerateNo_Click" />
        <br />
        <br />
        <asp:Button ID="btnGenerateNoAndInsert" runat="server" Text="生成单号并向主表和子表中Insert资料" OnClick="btnGenerateNoAndInsert_Click" />
    </div>
    </form>
</body>
</html>
