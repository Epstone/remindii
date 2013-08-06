<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/BirthdayReminder.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Birthday Reminder - Success
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h1>
		Success</h1>
	<p>You have successfully created a new Reminder!<br />
	Please make sure that our mails are not catched by your spam filter! :)</p>
	<% if (!(bool)ViewData["EmailVerified"])
	{%>
	<p>
		Please notice that you have not yet verified your email address yet. Do this by following the link in our verification email sent to your address.<br />
	</p>
	<p>
		Thank your for your cooporation!</p>
	<%}
	else
	{%><p>
		Thanks for using the remindii service!</p>
	<%} %>
	<%= Html.ActionLink("Create another remindii", "Index")%>
</asp:Content>
