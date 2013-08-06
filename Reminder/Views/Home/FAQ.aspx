<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/BirthdayReminder.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Remindii - FAQ
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h1>
		FAQ</h1>
	<p>
		<span class="faq">When will I receive the reminder?</span> You will receive the reminder email once a year between midnight and 3 o'clock in the morning your time.
	</p>
	<p>
		<span class="faq">How can I delete a reminder?</span> There is a deletion link inside each notification email.</p>
	<p>
		<span class="faq">Will you ever sell those email addresses?</span> No.
	</p>
</asp:Content>
