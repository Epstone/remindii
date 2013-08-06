using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;
using BirthdayReminder.Utility;

namespace BirthdayReminder.DataAccess
{
    public partial class Database : IDisposable
    {
        MySqlConnection _connection;

        private static string GetConnectionString()
        {
            Console.WriteLine(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile.ToString());
            return ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        }

        private MySqlCommand GetCommand(string text)
        {
           
            return new MySqlCommand(text, GetOpenConnection());
        }

        private MySqlConnection GetOpenConnection()
        {
            if (_connection == null)
            {
                _connection = new MySqlConnection(GetConnectionString());
                _connection.Open();
            }

            return _connection;
        }

        private DataTable GetResultAsDataTable(MySqlCommand sqlCom)
        {
            try
            {
                DataTable result = new DataTable();
                using (MySqlDataAdapter da = new MySqlDataAdapter(sqlCom))
                {
                    da.Fill(result);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw ex;
            }
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                if (this._connection.State != ConnectionState.Closed) this._connection.Close();
                this._connection.Dispose();
            }
        }

    }
}