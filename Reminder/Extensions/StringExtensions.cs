using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BirthdayReminder.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

        public static string Encode(this string input)
        {
            return System.Web.HttpUtility.HtmlEncode(input);
        }

        public static bool HasValue(this string input)
        {
            return !string.IsNullOrEmpty(input);
        }

        public static string PathCombine(this string currentPart, string nextPart)
        {
            return Path.Combine(currentPart, nextPart);
        }
        public static bool IsNotEmpty(this string instance)
        {
            return instance != null && instance.Length > 0;
        }
    }
}
