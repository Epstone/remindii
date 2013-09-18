using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BirthdayReminder.Models;
using BirthdayReminder.DataAccess;
using BirthdayReminder.Utility;
using System.Globalization;
using BirthdayReminder.Enums;
using BirthdayReminder.Management;

namespace BirthdayReminder.Controllers
{
  public class HomeController : ReminderController
  {
    protected override void Initialize(System.Web.Routing.RequestContext requestContext)
    {
      base.Initialize( requestContext );

      //set reminder and email count for master page
      ViewData["ReminderCount"] = ReminderRepository.GetReminderCount();
      ViewData["UserCount"] = ReminderRepository.GetUserCount();
    }


    public HomeController()
    {

    }
    public HomeController(IMessageService messageService)
    {
      this.MessageService = messageService;
    }

    [HttpGet]
    [HandleError]
    public ActionResult Index()
    {
      ViewData["DaySelectList"] = GetDaySelectList();
      ViewData["MonthSelectList"] = Months;
      //ViewData["DaysBeforeSelect"] = GetDaysBeforeReminderSelectList();

      return View();
    }

    private SelectList GetDaySelectList()
    {
      //generate days selectlist
      List<int> days = new List<int>();
      for (int i = 1; i <= 31; i++)
      {
        days.Add( i );
      }

      return new SelectList( days, DateTime.Now.Day );
    }

    public SelectList Months
    {
      get
      {
        var dDur = new Dictionary<int, string>();
        dDur.Add( 1, "January" );
        dDur.Add( 2, "Febuary" );
        dDur.Add( 3, "March" );
        dDur.Add( 4, "April" );
        dDur.Add( 5, "May" );
        dDur.Add( 6, "June" );
        dDur.Add( 7, "July" );
        dDur.Add( 8, "August" );
        dDur.Add( 9, "September" );
        dDur.Add( 10, "October" );
        dDur.Add( 11, "November" );
        dDur.Add( 12, "December" );

        return new SelectList( dDur, "Key", "Value", DateTime.Now.Month );
      }
    }

    [HttpGet]
    public ActionResult TermsOfUse()
    {
      return View();
    }

    //public SelectList GetDaysBeforeReminderSelectList()
    //{
    //    //Days Before SelectList
    //    Dictionary<int, string> daysBefore = new Dictionary<int, string>();
    //    daysBefore.Add(0, "Same Day");
    //    daysBefore.Add(1, "One Day Before");
    //    daysBefore.Add(2, "Two Days Before");
    //    daysBefore.Add(7, "One Week Before");

    //    return new SelectList(daysBefore, "key", "value", daysBefore[1]);
    //}


    [HttpPost]
    [HandleError]
    public ActionResult CreateReminder(ReminderModel model)
    {
      if (ModelState.IsValid)
      {
        model.Email = model.Email.ToLowerInvariant();

        E_ReminderCreationStatus reminderCreationStatus = TryCreateReminder( model );


        switch (reminderCreationStatus)
        {
          case E_ReminderCreationStatus.successfull:
            ViewData["EmailVerified"] = true;
            return View( "Success" );
          case E_ReminderCreationStatus.successVerifyEmail:
            ViewData["EmailVerified"] = false;
            return View( "Success" );
          case E_ReminderCreationStatus.wrongPassword:
            ModelState.AddModelError( "Password", "Wrong password" );
            break;
        }
      }

      //We are to far now, some error must exist, return creation view to user again
      ViewData["DaySelectList"] = GetDaySelectList();
      ViewData["MonthSelectList"] = Months;
      //ViewData["DaysBeforeSelect"] = GetDaysBeforeReminderSelectList();

      return View( "Index", model );


    }

    private E_ReminderCreationStatus TryCreateReminder(ReminderModel model)
    {

      string passwordHash = Security.GetSHA1Hash( model.Password );
      E_ReminderCreationStatus resultStatus = E_ReminderCreationStatus.error;


      E_VerificationStatus verificationStatus = ReminderRepository.GetVerificationStatus( model.Email, passwordHash );
      int mailID = 0;
      DateTime nextReminder;
      switch (verificationStatus)
      {
        case E_VerificationStatus.emailUnknown:
          Guid verificationCode = Guid.NewGuid();
          int emailID = ReminderRepository.InsertUnverifiedEmail( model.Email, passwordHash, verificationCode );
          MessageService.SendVerificationEmail( model.Email, verificationCode );
          nextReminder = GetNextRemindingDate( model, DateTime.Now );
          ReminderRepository.AddReminder( model, emailID, nextReminder );

          resultStatus = E_ReminderCreationStatus.successVerifyEmail;

          Logger.LogInfo(
              string.Format( "unkown address remindii for:'{0}', Name: {1}"
              , model.Email, model.Name ) );
          break;

        case E_VerificationStatus.emailNotYetVerified:
          mailID = ReminderRepository.GetEmailID( model.Email );
          nextReminder = GetNextRemindingDate( model, DateTime.Now );
          ReminderRepository.AddReminder( model, mailID, nextReminder );
          resultStatus = E_ReminderCreationStatus.successVerifyEmail;

          Logger.LogInfo(
                   string.Format( "not verified remindii for:'{0}', Name: {1}"
                  , model.Email, model.Name ) );
          break;

        case E_VerificationStatus.successfull:
          mailID = ReminderRepository.GetEmailID( model.Email );
          nextReminder = GetNextRemindingDate( model, DateTime.Now );
          ReminderRepository.AddReminder( model, mailID, nextReminder );
          resultStatus = E_ReminderCreationStatus.successfull;
          Logger.LogInfo(
                  string.Format( "verified remindii for:'{0}', Name: {1}"
                  , model.Email, model.Name ) );
          break;

        case E_VerificationStatus.wrongPassword:
          resultStatus = E_ReminderCreationStatus.wrongPassword;
          Logger.LogInfo( "Wrong password for email: " + model.Email );
          break;
      }

      if (resultStatus == E_ReminderCreationStatus.error) throw new ApplicationException( "Fehler" );
      return resultStatus;

    }

    public DateTime GetNextRemindingDate(ReminderModel model, DateTime now)
    {
      //fix now (just take month and day of DateTime
      now = new DateTime( DateTime.Now.Year, now.Month, now.Day );

      //create new datetime with current year and look if its in the past or today's date
      DateTime reminderThisYear = new DateTime( now.Year, model.Month, model.Day )
          .AddHours( GetHourFix( model.TimeZoneOffset ) );

      //if its in the past create reminder for the next year
      if (reminderThisYear <= now)
        return reminderThisYear.AddYears( 1 );
      else
        return reminderThisYear;
    }

    public int GetHourFix(int timeZoneOffset)
    {
      return 2 + timeZoneOffset;
    }

    [HttpGet]
    [HandleError]
    public ActionResult FAQ()
    {
      return View();
    }

    [HttpGet]
    [HandleError]
    public ActionResult DeleteReminder(Guid reminderID)
    {

      ReminderRepository.DeleteReminder( reminderID );


      return View();
    }

    protected override void OnException(ExceptionContext filterContext)
    {
      Logger.LogError( filterContext.Exception );

      // Mark exception as handled
      filterContext.ExceptionHandled = true;

      // Redirect
      filterContext.Result = this.RedirectToAction( "Error", "Home" );
      base.OnException( filterContext );
    }

    public ActionResult Error()
    {
      return View();
    }

    public ActionResult MailTest()
    {
      ((MailMessageService)MessageService).SendEmail( "epstone2@gmail.com", "remindii test" + DateTime.UtcNow, "test mail" );

      return new ContentResult
      {
        Content = "ok"
      };
    }

  }
}
