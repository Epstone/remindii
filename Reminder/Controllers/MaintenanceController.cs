using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BirthdayReminder.DataAccess;
using BirthdayReminder.Management;
using BirthdayReminder.Utility;
using System.Net;
using System.Configuration;
namespace BirthdayReminder.Controllers
{
  public class MaintenanceController : ReminderController
  {
    IMessageService _messageService;
    protected override void Initialize(System.Web.Routing.RequestContext requestContext)
    {
      base.Initialize( requestContext );
      if (this._messageService == null) this._messageService = new MailMessageService();
    }

    public MaintenanceController()
    {

    }
    public MaintenanceController(IMessageService messageService)
    {
      this._messageService = messageService;
    }
    public ActionResult Hourly(string secretKey)
    {
      if (secretKey == ConfigurationManager.AppSettings["SecretKey"])
      {
        SendReminders();
        return new ContentResult()
        {
          Content = "Success"
        };
      }
      else
      {
        return new ContentResult
        {
          Content = "wrong secret key"
        };
      }

    }

    private void SendReminders()
    {

      bool todoRemindersExisting = true;
      while (todoRemindersExisting)
      {
        var reminder = ReminderRepository.GetNextTimeHasComeReminder();
        if (reminder != null)
        {
          try
          {
            //send reminder
            _messageService.SendReminder( reminder );
            //increase next reminding date for this reminder 
            ReminderRepository.IncreaseNextRemindingDate( reminder.ID );
          }
          catch (Exception ex)
          {
            Logger.LogError( ex );
          }
        }
        //All reminders have been processed, exit loop
        else todoRemindersExisting = false;

      }
    }
  }
}
