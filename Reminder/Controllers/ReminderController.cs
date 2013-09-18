using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BirthdayReminder.DataAccess;
using BirthdayReminder.Utility;

namespace BirthdayReminder.Controllers
{
  public class ReminderController : Controller
  {

    ReminderDatabase _repository;
    public ReminderDatabase ReminderRepository
    {
      get
      {
        if (_repository == null)
        {
          _repository = new ReminderDatabase();
          var connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
          _repository.Initialize( connectionString );
        }

        return _repository;
      }
    }

    public void StoreLog(string message, string stacktrace, SeverityLevel severityLevel, string customMessage)
    {
      this.ReminderRepository.AddLog( severityLevel, message, stacktrace, customMessage );
    }
  }
}