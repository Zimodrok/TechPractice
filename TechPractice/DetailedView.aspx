<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DetailedView.aspx.cs" Inherits="TechPractice.DetailedView" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Event Details</h1>
            <ul>
                <li>Name: <asp:Label ID="lblName" runat="server" /></li>
                <li>TTL: <asp:Label ID="lblTTL" runat="server" /></li>
                <li>Time Zones: <asp:Label ID="lblTimeZones" runat="server" /></li>
                <li>Public Link: <asp:HyperLink ID="lblPublicLink" runat="server" NavigateUrl='<%# Eval("PublicLink") %>' /></li>
            </ul>
        </div>
    </form>
</body>
</html>