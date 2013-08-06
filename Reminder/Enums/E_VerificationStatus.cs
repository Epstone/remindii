using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BirthdayReminder.Enums
{
    public enum E_VerificationStatus
    {
        successfull = 0,
        emailUnknown = 1,
        emailNotYetVerified = 2,
        wrongPassword
    }
}
