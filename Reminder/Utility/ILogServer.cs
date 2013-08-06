using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BirthdayReminder.Utility
{
    public interface ILogServer
    {
        void StoreLog(string message, string stacktrace, SeverityLevel severityLevel, string customMessage);
    }
}
