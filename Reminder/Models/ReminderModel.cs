using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BirthdayReminder.Models
{
    public class ReminderModel
    {
        [Required]
        [Range(1, 31)]
        public int Day { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required(ErrorMessage = "*")]

        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "incorrect email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(100, ErrorMessage = "100 characters maximum reached")]
        public string Name { get; set; }

        //[StringLength(160, ErrorMessage = "160 characters maximum reached")]
        //public string Description { get; set; }

        [Required(ErrorMessage = "Please activate Javascript")]
        [Range(-20, 20)]
        public int TimeZoneOffset { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(100)]
        public string Password { get; set; }

        //[Required()]
        //[Range(0, 7)]
        //public int DaysBefore { get; set; }
    }
}