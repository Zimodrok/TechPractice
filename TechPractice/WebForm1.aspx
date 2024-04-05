<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="TechPractice.WebForm1" %>

    <%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>  

<asp:Content ContentPlaceHolderId="CPH1" runat="server">
        <div>
                    <div class="home-container2">
            <div id="chartdiv"></div>
            <div class="container-container1">
                <ul class="container-ul list" runat="server" id="container">
                    <!-- Buttons will be dynamically added here -->
                </ul>
            <asp:Button ID="btnCreateEvent" runat="server" CssClass="container-button button" Text="Create Event" OnClick="btnCreateEvent_Click" />
            </div>
        </div>
        </div>
    <cc1:ModalPopupExtender ID="mp1" runat="server" PopupControlID="Panl1" TargetControlID="btnCreateEvent"  
    CancelControlID="Button2" BackgroundCssClass="Background">  
</cc1:ModalPopupExtender>  
<asp:Panel ID="Panl1" runat="server" CssClass="Popup" align="center" style = "display:none">  
    <iframe style=" width: 350px; height: 300px;" id="irm1" src="CreateEventView.aspx" runat="server"></iframe>  
   <br/>  
<asp:Button ID="Button2" runat="server" Text="Close"/>

</asp:Panel> 
<!--<div id="popup1" class="overlay" runat="server">
    <div class="popup">
        <h2>Enter Event Details</h2>
        <div class="content">
            <asp:Label ID="lblEventName" runat="server" AssociatedControlID="txtEventName" Text="Event Name:"></asp:Label>
            <asp:TextBox ID="txtEventName" runat="server" placeholder="Enter event name"></asp:TextBox><br />
            <asp:Label ID="lblEventTTL" runat="server" AssociatedControlID="txtEventTTL" Text="Event TTL:"></asp:Label>
            <asp:TextBox ID="txtEventTTL" runat="server" placeholder="Enter event TTL"></asp:TextBox><br />
            <asp:Label ID="lblEventTimeZones" runat="server" AssociatedControlID="txtEventTimeZones" Text="Event Time Zones:"></asp:Label>
            <asp:TextBox ID="txtEventTimeZones" runat="server" placeholder="Enter event time zones"></asp:TextBox><br />
            <asp:Label ID="lblEventPublicLink" runat="server" AssociatedControlID="txtEventPublicLink" Text="Event Public Link:"></asp:Label>
            <asp:TextBox ID="txtEventPublicLink" runat="server" placeholder="Enter event public link"></asp:TextBox><br />
        </div>
    </div>
</div>
    -->

</asp:Content>
