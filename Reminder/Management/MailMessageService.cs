using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BirthdayReminder.Utility;
using System.Net.Mail;
using BirthdayReminder.Entities;
using BirthdayReminder.Extensions;
using System.Threading;
using System.Globalization;
using System.Configuration;
using System.Net;

namespace BirthdayReminder.Management
{
  public class MailMessageService : IMessageService
  {
    public MailMessageService()
    {
      Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }

    public void SendReminder(ReminderTask reminder)
    {
      Logger.LogInfo( string.Format( @"Sending remindii 
                                             Address: {0}
                                             Name:    {1},
                                             Date:    {2},", reminder.EmailAddress,
                                                     reminder.Name.Encode(),
                                                     reminder.Date.ToShortDateString() ) );
      //generate subject
      string subject = string.Format( "Remindii For: {0} on {1}"
              , reminder.Name
              , string.Format( "{0:D}", reminder.Date ) );

      //generate body
      string body = string.Format( @"
            <p>To delete this remindii immediately click this deletion link: http://remindii.com/delete/{0}</p>
            See you on http://remindii.com !"
          , reminder.ID );

      SendEmail( reminder.EmailAddress, body, subject );
    }

    public void SendVerificationEmail(string emailAddress, Guid verificationCode)
    {
      Logger.LogInfo( string.Format( "Sending verification mail to {0}", emailAddress ) );

      string subject = "Please Verify Your Email Address";

      string body = string.Format( @"Hey,<br />
                    thank you for using Remindii! <p>Before we can send you any notifications please click on the following link to verify your email address:</p>
                    <p>http://www.remindii.com/verify/{0}</p>

                    <p>We hope you will enjoy the Remindee service!</p>", verificationCode.ToString() );

      SendEmail( emailAddress, body, subject );
    }

    private void SendEmail(string address, string body, string subject)
    {
      var senderAddress = ConfigurationManager.AppSettings["SenderAddress"];
      var serverAddress = ConfigurationManager.AppSettings["ServerAddress"];
      var port = int.Parse( ConfigurationManager.AppSettings["Port"] );
      var username = ConfigurationManager.AppSettings["Username"];
      var password = ConfigurationManager.AppSettings["Password"];

      using (var smtpClient = new SmtpClient( serverAddress, port ))
      {
        smtpClient.EnableSsl = true;
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Credentials = new NetworkCredential( username, password );

        using (var mail = new MailMessage())
        {
          MailAddress receiverAdress = new MailAddress( address );
          mail.To.Add( receiverAdress );
          mail.From = new MailAddress( senderAddress );
          mail.Subject = subject;
          mail.Body = body;
          mail.IsBodyHtml = true;

          smtpClient.Send( mail );
        }

      }

    }

    //private void SendMailLocalServer(string emailTo, string emailFrom, string emailBody,
    //   string emailSubject, string smtpServerAdress)
    //{

    //}



  }
}