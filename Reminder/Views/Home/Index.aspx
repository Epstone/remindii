<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/BirthdayReminder.Master" Inherits="System.Web.Mvc.ViewPage<BirthdayReminder.Models.ReminderModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Remindii - A Simple Anniversary And Birthday Reminder
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<%bool isMobileClient = Request.IsMobileClient(this.Response); %>
	<p>
		<span class="faq">What is it all about?</span> <span class="answer">Remindii is a simple to use email anniversary reminder service. </span><span class="no_mobile">Please fill out the form and we will remind you through a simple email for the anniversary.</span>
	</p>
	<%Html.BeginForm("CreateReminder", "Home", FormMethod.Post);
   {	%>
	<%=Html.ValidationMessageFor(m=>m.TimeZoneOffset)%>
	<div class="name_box has-input">
		<div class="eg">
			e.g. Susan's birthday</div>
		<label for="tbx_name" class="large">
			Remind me for:</label>
		<%=Html.TextBoxFor(m => m.Name, new { id = "tbx_name" })%>
		<%=Html.ValidationMessageFor(m=>m.Name) %>
	</div>
	<div class="has-input">
		<label for="dd_day" class="large">
			When?</label>
		<%= Html.DropDownListFor(m => m.Month, (SelectList)ViewData["MonthSelectList"], new { id = "dd_month" })%>
		<label for="dd_day" class="no-width no_block" style="margin-left: 1em;">
			Day</label>
		<%= Html.DropDownListFor(m => m.Day, ViewData["DaySelectList"] as SelectList, new { id = "dd_day" ,@class="no-width"})%>
	</div>
	<div class="has-input">
		<label for="tbx_email" class="large">
			Your email address:</label>
		<%= Html.TextBoxFor(m => m.Email, new { id = "tbx_email" })%>
		<%=Html.ValidationMessageFor(m=>m.Email) %>
	</div>
	<div class="has-input">
		<div class="eg">
			e.g. first pets or teachers name</div>
		<label for="tbx_password" class="large">
			Password:</label>
		<%=Html.PasswordFor(m => m.Password, new { id = "tbx_password" })%>
		<%=Html.ValidationMessageFor(m=>m.Password) %>
	</div>
	<%=Html.HiddenFor(m => m.TimeZoneOffset, new { id = "tbx_offset" })%>
	<div class="has-input">
		<label for="btn_submit" class="large done">
			Done!
		</label>
		<input type="submit" value="Create Reminder" class="btn" id="btn_submit" />
	</div>
	<%Html.EndForm(); %>
	<%}%>
	<% if (!isMobileClient)
	{ %>
	<script type="text/javascript" src="https://apis.google.com/js/plusone.js"></script>
	<g:plusone></g:plusone>
	<div id="fb-root">
	</div>
	<script src="http://connect.facebook.net/en_US/all.js#xfbml=1"></script>
	<fb:like href="http://remindii.com" send="false" width="450" show_faces="true" font=""></fb:like>
	<% } %>
	<script type="text/javascript">
		$(function () {

			$("#tbx_offset").val(get_time_zone_offset());
		});

		function get_time_zone_offset() {
			var current_date = new Date();
			var gmt_offset = current_date.getTimezoneOffset() / 60;
			return gmt_offset;
		}</script>
	<%--<br />
		When to remind?
		<%=Html.DropDownListFor(m => m.DaysBefore, (SelectList)ViewData["DaysBeforeSelect"])%>--%>
	<%--<div class="description_box">
		Optional Description: (What to do on that day?)<br />
		<%=Html.TextAreaFor(m=>m.Description) %></div>
	<%=Html.ValidationMessageFor(m=>m.Description) %>--%>
</asp:Content>
