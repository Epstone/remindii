using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BirthdayReminder.DataAccess;

namespace BirthdayReminder.Utility
{
  public class LogUtility
  {
    LoggerRepository _db;

    public void Initialize(LoggerRepository db)
    {
      this._db = db;
    }

    public void LogInfo(string customMessage)
    {
      _db.AddLog( SeverityLevel.Info, string.Empty, string.Empty, customMessage );
    }

    public void LogError(Exception exception)
    {
      _db.AddLog( SeverityLevel.Error, exception.Message, exception.StackTrace, string.Empty );
    }
    public void Log(Exception ex, SeverityLevel lvl, string customMessage){

      _db.AddLog( lvl, ex.Message, ex.StackTrace, customMessage );

    }
    

  }
}