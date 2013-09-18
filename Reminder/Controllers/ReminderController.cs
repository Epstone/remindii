using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BirthdayReminder.DataAccess;
using BirthdayReminder.Management;
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
          _repository = CreateReminderRepository();
        }

        return _repository;
      }
    }

    string ConnectionString
    {
      get
      {
        return ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
      }
    }

    private ReminderDatabase CreateReminderRepository()
    {
      var repo = new ReminderDatabase();
      repo.Initialize( this.ConnectionString, this.Logger );

      return repo;
    }

    LogUtility _logger;
    public LogUtility Logger
    {
      get
      {

        if (_logger == null)
        {
          _logger = new LogUtility();
          var loggerDatabase = new LoggerRepository().Initialize( this.ConnectionString );

          _logger.Initialize( loggerDatabase );
        }

        return _logger;
      }
    }

    IMessageService _messageService;
   public IMessageService MessageService
    {
      get
      {
        if (this._messageService == null)
        {
          this._messageService = new MailMessageService( this.Logger );
        }

        return _messageService;
      }
      set {
        this._messageService = value;
      }
    }



    //public void StoreLog(string message, string stacktrace, SeverityLevel severityLevel, string customMessage)
    //{
    //  this.ReminderRepository.AddLog( severityLevel, message, stacktrace, customMessage );
    //}
  }
}