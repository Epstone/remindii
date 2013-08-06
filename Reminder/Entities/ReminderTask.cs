using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BirthdayReminder.Entities
{
    public class ReminderTask
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public DateTime Date { get; set; }
    }
}