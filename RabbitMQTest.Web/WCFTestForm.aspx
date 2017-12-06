<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WCFTestForm.aspx.cs" Inherits="RabbitMQTest.Web.WCFTestForm" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button ID="btnGenNo" runat="server" Text="通过WCF访问MQ取号服务" OnClick="btnGenNo_Click" />
    </div>
    </form>
</body>
</html>
