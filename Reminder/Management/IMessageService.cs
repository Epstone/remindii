using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BirthdayReminder.Entities;

namespace BirthdayReminder.Management
{
    public interface IMessageService
    {
        void SendVerificationEmail(string emailAddress, Guid verificationCode);
        void SendReminder(ReminderTask reminder);
    }
}
