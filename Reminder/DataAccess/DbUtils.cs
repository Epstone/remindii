using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using BirthdayReminder.Utility;

namespace BirthdayReminder.DataAccess
{
    public partial class Database
    {
        public void AddLog(SeverityLevel level, string ExceptionMessage, string ExceptionStacktrace, string customMessage)
        {
            var sqlCom = GetCommand(@"INSERT INTO logs 
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
                                );");

            sqlCom.Parameters.AddWithValue("@LogLevel", level.ToString());
            sqlCom.Parameters.AddWithValue("@ExceptionMessage", ExceptionMessage);
            sqlCom.Parameters.AddWithValue("@ExceptionStacktrace", ExceptionStacktrace);
            sqlCom.Parameters.AddWithValue("@CustomMessage", customMessage);
            sqlCom.Parameters.AddWithValue("@Date", DateTime.Now);

            sqlCom.ExecuteNonQuery();
        }

        public DataTable GetAllLogs(string severityLevel)
        {
            if (string.IsNullOrEmpty(severityLevel))
                severityLevel = "%";

            MySqlCommand sqlCom = GetCommand(@"SELECT `Date`,LogLevel,`ExceptionMessage`,`ExceptionStacktrace`,`CustomMessage` 
                                                            FROM logs 
                                                            WHERE (LogLevel like ?LogLevel )
                                                            ORDER BY Date DESC;");
            sqlCom.Parameters.AddWithValue("?LogLevel", severityLevel);

            return GetResultAsDataTable(sqlCom);
        }

        internal void ClearLogs()
        {
            string text = "TRUNCATE TABLE logs";
            var sqlCom = GetCommand(text);

            try
            {
                sqlCom.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Logger.LogError(ex);
            }
        }
    }
}
