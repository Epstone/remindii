using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BirthdayReminder.Utility;
using BirthdayReminder.DataAccess;
namespace BirthdayReminder.Management
{
    public class LogManagement : ILogServer
    {
        public void StoreLog(string message, string stacktrace, SeverityLevel severityLevel, string customMessage)
        {
            using (var db = new Database())
            {
                db.AddLog(severityLevel, message, stacktrace, customMessage);
            }
        }
    }
}
