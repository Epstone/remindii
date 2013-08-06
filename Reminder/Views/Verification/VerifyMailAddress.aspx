<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/BirthdayReminder.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	VerifyMailAddress
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

	<% if ((bool)ViewData["VerificationSuccess"]) { %>
	<h1>Success</h1>
	<p>Your email address is now verified!</p> <p>We will send out your remindii when the time has come! (:</p>
	<% } else { %>
	<h1>Allready activated?</h1>
	Sorry, we could not activate your email address. Probably it is allready activated?
	<% } %>

</asp:Content>
