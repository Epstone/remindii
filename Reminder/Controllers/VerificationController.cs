using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BirthdayReminder.DataAccess;
using BirthdayReminder.Utility;

namespace BirthdayReminder.Controllers
{
    public class VerificationController : Controller
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            //set reminder and email count for master page
            using (var db = new Database())
            {
                ViewData["ReminderCount"] = db.GetReminderCount();
                ViewData["UserCount"] = db.GetUserCount();
            }
        }

        [HandleError]
        public ActionResult VerifyMailAddress(string verificationCode)
        {
            if (string.IsNullOrEmpty(verificationCode)) return RedirectToAction("Index");

            using (var db = new Database())
            {
                ViewData["VerificationSuccess"] = db.VerifyMailAddress(verificationCode);
                string msg = "Verified mail address by code: " + verificationCode;
                Logger.LogInfo(msg);
            }

            return View();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Logger.LogError(filterContext.Exception);

            // Mark exception as handled
            filterContext.ExceptionHandled = true;

            // Redirect
            filterContext.Result = this.RedirectToAction("Error", "Home");
            
            base.OnException(filterContext);
        }
    }
}
