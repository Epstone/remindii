using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using BirthdayReminder.Utility;
using MySqlRepository;

namespace BirthdayReminder.DataAccess
{
  public partial class ReminderDatabase
  {
    MySqlRepositoryBase _db;

    private DataTable GetResultAsDataTable(MySqlCommand sqlCom)
    {
      try
      {
        DataTable result = new DataTable();
        using (MySqlDataAdapter da = new MySqlDataAdapter( sqlCom ))
        {
          da.Fill( result );
        }

        return result;
      }
      catch (Exception ex)
      {
        Logger.LogError( ex );
        throw ex;
      }
    }

    public void AddLog(SeverityLevel level, string ExceptionMessage, string ExceptionStacktrace, string customMessage)
    {
      try
      {
        var sqlCom = _db.GetWriteCommand( @"INSERT INTO logs 
                                 (
                                  LogLevel,
                                  ExceptionMessage,
                                  ExceptionStacktrace,
                                  CustomMessage,
                                  Date
                                  )
                                Values
                                (
                                  @LogLevel,
                                  @ExceptionMessage,
                                  @ExceptionStacktrace,
                                  @CustomMessage,
                                  @Date
                                );" );

        sqlCom.Parameters.AddWithValue( "@LogLevel", level.ToString() );
        sqlCom.Parameters.AddWithValue( "@ExceptionMessage", ExceptionMessage );
        sqlCom.Parameters.AddWithValue( "@ExceptionStacktrace", ExceptionStacktrace );
        sqlCom.Parameters.AddWithValue( "@CustomMessage", customMessage );
        sqlCom.Parameters.AddWithValue( "@Date", DateTime.Now );

        sqlCom.ExecuteNonQuery();
      }
      finally
      {
        _db.CloseConnections();
      }
    }

    public DataTable GetAllLogs(string severityLevel)
    {
      if (string.IsNullOrEmpty( severityLevel ))
        severityLevel = "%";

      try
      {
        MySqlCommand sqlCom = _db.GetReadCommand( @"SELECT `Date`,LogLevel,`ExceptionMessage`,`ExceptionStacktrace`,`CustomMessage` 
                                                            FROM logs 
                                                            WHERE (LogLevel like ?LogLevel )
                                                            ORDER BY Date DESC;" );
        sqlCom.Parameters.AddWithValue( "?LogLevel", severityLevel );

        var result = GetResultAsDataTable( sqlCom );

        return result;
      }
      finally
      {
        _db.CloseConnections();
      }
    }

    internal void ClearLogs()
    {
      string text = "TRUNCATE TABLE logs";
      var sqlCom = _db.GetWriteCommand( text );

      try
      {
        sqlCom.ExecuteNonQuery();
      }
      catch (MySqlException ex)
      {
        Logger.LogError( ex );
      }
      finally
      {
        _db.CloseConnections();
      }
    }
  }
}
