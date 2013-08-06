using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BirthdayReminder.Models;
using BirthdayReminder.Controllers;
using BirthdayReminder.Enums;
using BirthdayReminder.Entities;
using MySql.Data.MySqlClient;
using BirthdayReminder.Utility;

namespace BirthdayReminder.DataAccess
{
    public partial class Database
    {
        internal E_VerificationStatus GetVerificationStatus(string email, string passwordHash)
        {
            E_VerificationStatus resultStatus = E_VerificationStatus.emailUnknown;

            var sqlCom = GetCommand(@"
                SELECT `EmailID`, `EmailAddress`, `PasswordHash`, EmailVerificationID
                FROM email 
                WHERE `EmailAddress` LIKE ?Address;");
            sqlCom.Parameters.AddWithValue("?Address", email);


            using (var rdr = sqlCom.ExecuteReader())
            {
                while (rdr.Read())
                {
                    if (rdr.GetString("PasswordHash") == passwordHash)
                    {
                        resultStatus = E_VerificationStatus.successfull;
                        if (rdr["EmailVerificationID"] != DBNull.Value)
                            resultStatus = E_VerificationStatus.emailNotYetVerified;
                    }
                    else
                        resultStatus = E_VerificationStatus.wrongPassword;
                }
            }

            return resultStatus;
        }


        public void AddReminder(ReminderModel reminder, int emailID, DateTime nextRemindingDate)
        {
            var sqlCom = GetCommand(@"INSERT INTO reminder 
                                              (Day, Month, Name, EmailID, TimeZoneOffset, 
                                                CreationDate, NextRemindingDate, ReminderID)
                                      VALUES (?Day, ?Month, ?Name, ?EmailID,?TimeZoneOffset,
                                              ?CreationDate, ?NextRemindingDate,?ID);");
            sqlCom.Parameters.AddWithValue("?Day", reminder.Day);
            sqlCom.Parameters.AddWithValue("?Month", reminder.Month);
            sqlCom.Parameters.AddWithValue("?Name", reminder.Name);
            sqlCom.Parameters.AddWithValue("?EmailID", emailID);
            sqlCom.Parameters.AddWithValue("?TimeZoneOffset", reminder.TimeZoneOffset);
            sqlCom.Parameters.AddWithValue("?CreationDate", DateTime.Now);
            sqlCom.Parameters.AddWithValue("?NextRemindingDate", nextRemindingDate);
            sqlCom.Parameters.AddWithValue("?ID", Guid.NewGuid());

            sqlCom.ExecuteNonQuery();
        }

        public int InsertUnverifiedEmail(string emailAddress, string passwordHash, Guid verificationCode)
        {
            // Insert Verification Task
            var sqlCom = GetCommand(@"INSERT INTO email_verification (VerificationCode, VerificationCodeExpiry)
                                    VALUES (?VerificationCode, ?VerificationCodeExpiry);");

            sqlCom.Parameters.AddWithValue("?VerificationCode", verificationCode);
            sqlCom.Parameters.AddWithValue("?VerificationCodeExpiry", DateTime.Now.AddDays(2));

            sqlCom.ExecuteNonQuery();

            int verificationTaskID = (int)sqlCom.LastInsertedId;

            //Insert Unverified Email Address
            sqlCom = GetCommand(@"INSERT INTO email (EmailAddress, PasswordHash, EmailVerificationID, 
                                                                        RegistrationDate)
                                  VALUES (?EmailAddress, ?PasswordHash, ?EmailVerificationID, ?Date);");
            sqlCom.Parameters.AddWithValue("?EmailAddress", emailAddress);
            sqlCom.Parameters.AddWithValue("?PasswordHash", passwordHash);
            sqlCom.Parameters.AddWithValue("?EmailVerificationID", verificationTaskID);
            sqlCom.Parameters.AddWithValue("?Date", DateTime.Now);

            sqlCom.ExecuteNonQuery();

            return (int)sqlCom.LastInsertedId;
        }

        public bool VerifyMailAddress(string verificationCode)
        {
            var sqlCom = GetCommand(@"
                    SELECT COUNT(VerificationCode) FROM email_verification 
	                    WHERE `VerificationCode` LIKE ?Code;
                    
                    DELETE FROM email_verification 
	                    WHERE `VerificationCode` LIKE ?Code;");
            sqlCom.Parameters.AddWithValue("?Code", verificationCode);

            return Convert.ToBoolean(sqlCom.ExecuteScalar());
        }

        public int GetEmailID(string mailAddress)
        {
            var sqlCom = GetCommand(@"
                                SELECT `EmailID`
                                FROM email 
                                WHERE `EmailAddress` LIKE ?Address;");
            sqlCom.Parameters.AddWithValue("?Address", mailAddress);

            return Convert.ToInt32(sqlCom.ExecuteScalar());
        }

        internal ReminderTask GetNextTimeHasComeReminder()
        {
            ReminderTask reminder = null;
            var sqlCom = GetCommand(@"SELECT 
                                     ReminderID, Name, e.EmailAddress, Day, Month
                                    FROM reminder r
	
                                    INNER JOIN email e
                                        ON e.EmailID = r.EmailID

                                    WHERE 
                                        (`NextRemindingDate` < Now())
                                    AND
                                        (e.EmailVerificationID IS Null)
                                    LIMIT 1;");
            using (var rdr = sqlCom.ExecuteReader())
            {
                while (rdr.Read())
                {
                    var date = new DateTime(DateTime.Now.Year
                                            , rdr.GetInt32("Month")
                                            , rdr.GetInt32("Day"));
                    reminder = new ReminderTask()
                    {
                        EmailAddress = rdr.GetString("EmailAddress"),
                        ID = rdr.GetGuid("ReminderID"),
                        Name = rdr.GetString("Name"),
                        Date = date
                    };
                }
            }
            return reminder;
        }

        internal void IncreaseNextRemindingDate(Guid reminderID)
        {
            var sqlCom = GetCommand(@"UPDATE reminder r 
                    SET r.NextRemindingDate = DATE_ADD(r.NextRemindingDate,INTERVAL 1 YEAR) 
                    WHERE r.ReminderID LIKE ?ReminderID LIMIT 1;");

            sqlCom.Parameters.AddWithValue("?ReminderID", reminderID);
            sqlCom.ExecuteNonQuery();
        }

        internal void DeleteReminder(Guid reminderID)
        {
            var sqlCom = GetCommand("DELETE FROM reminder WHERE ReminderID LIKE ?ID LIMIT 1;");
            sqlCom.Parameters.AddWithValue("?ID", reminderID);


            try
            {
                sqlCom.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Logger.Log(ex, SeverityLevel.Error,
                    string.Format("Could not delete reminder with ID {0}", reminderID));
                throw ex;
            }

        }


        public int GetUserCount()
        {
            string cmd = "SELECT COUNT(EmailID) FROM email";

            var sqlCom = GetCommand(cmd);

            return Convert.ToInt32(sqlCom.ExecuteScalar());
        }

        public int GetReminderCount()
        {
            string cmd = "SELECT COUNT(ReminderID) FROM reminder";

            var sqlCom = GetCommand(cmd);

            return Convert.ToInt32(sqlCom.ExecuteScalar());
        }
    }
}