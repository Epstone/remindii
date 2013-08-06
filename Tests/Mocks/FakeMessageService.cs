using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BirthdayReminder.Controllers;
using BirthdayReminder.Management;
using BirthdayReminder.Entities;

namespace Tests.Mocks
{
    public class FakeMessageService : IMessageService
    {
        bool _isVerficationMailSent;
        public string SentVerificationCode { get; set; }
        public int VerificationEmailsSentCount { get; set; }
        public ReminderTask SentoutReminder { get; set; }
        public int ReminderEmailsSentCount { get; set; }
        public bool IsVerificationEmailSent()
        {
            return _isVerficationMailSent;
        }


        public void SendVerificationEmail(string emailAddress, Guid verificationCode)
        {
            Console.WriteLine("Sending Verification Mail");
            _isVerficationMailSent = true;
            SentVerificationCode = verificationCode.ToString();
            VerificationEmailsSentCount++;
        }

        public void SendReminder(ReminderTask reminder)
        {
            this.SentoutReminder = reminder;
            this.ReminderEmailsSentCount++;
        }
    }
}
