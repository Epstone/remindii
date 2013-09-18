using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BirthdayReminder.DataAccess;
using BirthdayReminder.Utility;

namespace BirthdayReminder.Controllers
{
  public class VerificationController : ReminderController
  {
    protected override void Initialize(System.Web.Routing.RequestContext requestContext)
    {
      base.Initialize( requestContext );

      //set reminder and email count for master page

      ViewData["ReminderCount"] = ReminderRepository.GetReminderCount();
      ViewData["UserCount"] = ReminderRepository.GetUserCount();

    }

    [HandleError]
    public ActionResult VerifyMailAddress(string verificationCode)
    {
      if (string.IsNullOrEmpty( verificationCode )) return RedirectToAction( "Index" );


      ViewData["VerificationSuccess"] = ReminderRepository.VerifyMailAddress( verificationCode );
      string msg = "Verified mail address by code: " + verificationCode;
      Logger.LogInfo( msg );


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
  }
}
