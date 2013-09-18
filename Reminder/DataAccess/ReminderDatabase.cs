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
using System.Configuration;
using MySqlRepository;
using System.Data;

namespace BirthdayReminder.DataAccess
{
  public class ReminderDatabase
  {
    MySqlRepositoryBase _db;
    LogUtility _logger;

    public void Initialize(string connectionString, LogUtility logger)
    {
      _logger = logger;

      _db = new MySqlRepositoryBase();
      this._db.Initialize( connectionString, connectionString );
    }



    internal E_VerificationStatus GetVerificationStatus(string email, string passwordHash)
    {
      E_VerificationStatus resultStatus = E_VerificationStatus.emailUnknown;

      var sqlCom = _db.GetReadCommand( @"
                SELECT `EmailID`, `EmailAddress`, `PasswordHash`, EmailVerificationID
                FROM email 
                WHERE `EmailAddress` LIKE ?Address;" );
      sqlCom.Parameters.AddWithValue( "?Address", email );


      try
      {
        using (var rdr = sqlCom.ExecuteReader())
        {
          while (rdr.Read())
          {
            if (rdr.GetString( "PasswordHash" ) == passwordHash)
            {
              resultStatus = E_VerificationStatus.successfull;
              if (rdr["EmailVerificationID"] != DBNull.Value)
                resultStatus = E_VerificationStatus.emailNotYetVerified;
            }
            else
              resultStatus = E_VerificationStatus.wrongPassword;
          }
        }
      }
      finally
      {
        _db.CloseConnections();
      }

      return resultStatus;
    }


    public void AddReminder(ReminderModel reminder, int emailID, DateTime nextRemindingDate)
    {
      try
      {
        var sqlCom = _db.GetWriteCommand( @"INSERT INTO reminder 
                                              (Day, Month, Name, EmailID, TimeZoneOffset, 
                                                CreationDate, NextRemindingDate, ReminderID)
                                      VALUES (?Day, ?Month, ?Name, ?EmailID,?TimeZoneOffset,
                                              ?CreationDate, ?NextRemindingDate,?ID);" );
        sqlCom.Parameters.AddWithValue( "?Day", reminder.Day );
        sqlCom.Parameters.AddWithValue( "?Month", reminder.Month );
        sqlCom.Parameters.AddWithValue( "?Name", reminder.Name );
        sqlCom.Parameters.AddWithValue( "?EmailID", emailID );
        sqlCom.Parameters.AddWithValue( "?TimeZoneOffset", reminder.TimeZoneOffset );
        sqlCom.Parameters.AddWithValue( "?CreationDate", DateTime.Now );
        sqlCom.Parameters.AddWithValue( "?NextRemindingDate", nextRemindingDate );
        sqlCom.Parameters.AddWithValue( "?ID", Guid.NewGuid() );

        sqlCom.ExecuteNonQuery();
      }
      finally
      {
        _db.CloseConnections();
      }
    }

    public int InsertUnverifiedEmail(string emailAddress, string passwordHash, Guid verificationCode)
    {
      // Insert Verification Task
      try
      {
        var sqlCom = _db.GetWriteCommand( @"INSERT INTO email_verification (VerificationCode, VerificationCodeExpiry)
                                    VALUES (?VerificationCode, ?VerificationCodeExpiry);" );

        sqlCom.Parameters.AddWithValue( "?VerificationCode", verificationCode );
        sqlCom.Parameters.AddWithValue( "?VerificationCodeExpiry", DateTime.Now.AddDays( 2 ) );

        sqlCom.ExecuteNonQuery();

        int verificationTaskID = (int)sqlCom.LastInsertedId;

        //Insert Unverified Email Address
        sqlCom = _db.GetWriteCommand( @"INSERT INTO email (EmailAddress, PasswordHash, EmailVerificationID, 
                                                                        RegistrationDate)
                                  VALUES (?EmailAddress, ?PasswordHash, ?EmailVerificationID, ?Date);" );
        sqlCom.Parameters.AddWithValue( "?EmailAddress", emailAddress );
        sqlCom.Parameters.AddWithValue( "?PasswordHash", passwordHash );
        sqlCom.Parameters.AddWithValue( "?EmailVerificationID", verificationTaskID );
        sqlCom.Parameters.AddWithValue( "?Date", DateTime.Now );

        sqlCom.ExecuteNonQuery();


        return (int)sqlCom.LastInsertedId;
      }
      finally
      {
        _db.CloseConnections();
      }
    }

    public bool VerifyMailAddress(string verificationCode)
    {
      try
      {
        var sqlCom = _db.GetWriteCommand( @"
                    SELECT COUNT(VerificationCode) FROM email_verification 
	                    WHERE `VerificationCode` LIKE ?Code;
                    
                    DELETE FROM email_verification 
	                    WHERE `VerificationCode` LIKE ?Code;" );
        sqlCom.Parameters.AddWithValue( "?Code", verificationCode );

        return Convert.ToBoolean( sqlCom.ExecuteScalar() );
      }
      finally
      {
        _db.CloseConnections();
      }
    }

    public int GetEmailID(string mailAddress)
    {
      try
      {
        var sqlCom = _db.GetReadCommand( @"
                                SELECT `EmailID`
                                FROM email 
                                WHERE `EmailAddress` LIKE ?Address;" );
        sqlCom.Parameters.AddWithValue( "?Address", mailAddress );

        return Convert.ToInt32( sqlCom.ExecuteScalar() );
      }
      finally
      {
        _db.CloseConnections();
      }
    }

    internal ReminderTask GetNextTimeHasComeReminder()
    {
      ReminderTask reminder = null;
      try
      {
        var sqlCom = _db.GetReadCommand( @"SELECT 
                                     ReminderID, Name, e.EmailAddress, Day, Month
                                    FROM reminder r
	
                                    INNER JOIN email e
                                        ON e.EmailID = r.EmailID

                                    WHERE 
                                        (`NextRemindingDate` < Now())
                                    AND
                                        (e.EmailVerificationID IS Null)
                                    LIMIT 1;" );
        using (var rdr = sqlCom.ExecuteReader())
        {
          while (rdr.Read())
          {
            var date = new DateTime( DateTime.Now.Year
                                    , rdr.GetInt32( "Month" )
                                    , rdr.GetInt32( "Day" ) );
            reminder = new ReminderTask()
            {
              EmailAddress = rdr.GetString( "EmailAddress" ),
              ID = rdr.GetGuid( "ReminderID" ),
              Name = rdr.GetString( "Name" ),
              Date = date
            };
          }
        }
        return reminder;
      }
      finally
      {
        _db.CloseConnections();
      }
    }

    internal void IncreaseNextRemindingDate(Guid reminderID)
    {
      try
      {
        var sqlCom = _db.GetWriteCommand( @"UPDATE reminder r 
                    SET r.NextRemindingDate = DATE_ADD(r.NextRemindingDate,INTERVAL 1 YEAR) 
                    WHERE r.ReminderID LIKE ?ReminderID LIMIT 1;" );

        sqlCom.Parameters.AddWithValue( "?ReminderID", reminderID );
        sqlCom.ExecuteNonQuery();
      }
      finally
      {
        _db.CloseConnections();
      }
    }

    internal void DeleteReminder(Guid reminderID)
    {
      try
      {
        var sqlCom = _db.GetWriteCommand( "DELETE FROM reminder WHERE ReminderID LIKE ?ID LIMIT 1;" );
        sqlCom.Parameters.AddWithValue( "?ID", reminderID );


        try
        {
          sqlCom.ExecuteNonQuery();
        }
        catch (MySqlException ex)
        {
          _logger.Log( ex, SeverityLevel.Error,
              string.Format( "Could not delete reminder with ID {0}", reminderID ) );
          throw ex;
        }
      }
      finally
      {
        _db.CloseConnections();
      }

    }


    public int GetUserCount()
    {
      try
      {
        string cmd = "SELECT COUNT(EmailID) FROM email";

        var sqlCom = _db.GetReadCommand( cmd );

        return Convert.ToInt32( sqlCom.ExecuteScalar() );
      }
      finally
      {
        _db.CloseConnections();
      }
    }

    public int GetReminderCount()
    {
      try
      {
        string cmd = "SELECT COUNT(ReminderID) FROM reminder";

        var sqlCom = _db.GetReadCommand( cmd );

        return Convert.ToInt32( sqlCom.ExecuteScalar() );
      }
      finally
      {
        _db.CloseConnections();
      }
    }
  }
}