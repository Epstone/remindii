using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using BirthdayReminder.DataAccess;
using BirthdayReminder.Models;
using BirthdayReminder.Controllers;
using Tests.Mocks;
using BirthdayReminder.Utility;
using System.Web.Mvc;
using BirthdayReminder.Management;
using System.Web;

using BirthdayReminder.Entities;
using System.Configuration;
namespace Tests
{
  [TestFixture]
  public class MainTest
  {

    ReminderDatabase _db;
    LogUtility _logger;

    [SetUp]
    public void Setup()
    {
      var connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
      _db = new ReminderDatabase();

      var logRepo = new LoggerRepository();
      logRepo.Initialize( connectionString );

      _logger = new LogUtility();
      _logger.Initialize( logRepo );

      _db.Initialize( connectionString, _logger );
    }

    /// <summary>
    /// This test runs through all possible reminder creation states
    /// </summary>
    [Test]
    public void CreateReminder()
    {
      ReminderModel reminder = new ReminderModel()
      {
        Day = 7,
        Month = 6,
        Email = Guid.NewGuid().ToString() + "@gmx.de",
        Name = "Patrick Epstein",
        Password = "Blah",
        TimeZoneOffset = -2
      };

      //Create reminder with unknown email address
      FakeMessageService messageService = new FakeMessageService();
      HomeController controller = new HomeController( messageService );
      var controllerResult = controller.CreateReminder( reminder ) as ViewResult;

      //Verify Controller Result
      Assert.That( controllerResult.ViewName == "Success" );
      Assert.That( (bool)controllerResult.ViewData["EmailVerified"], Is.False );
      //Assert verification email sent
      Assert.That( messageService.IsVerificationEmailSent(), Is.True );
      string verificationCode = messageService.SentVerificationCode;
      Assert.False( string.IsNullOrEmpty( verificationCode ) );
      Assert.That( messageService.VerificationEmailsSentCount, Is.EqualTo( 1 ) );


      //Try to create another reminder, same mail address still not verfied
      reminder.Name = "Heinz Erhardt";
      reminder.Day = 14;
      reminder.Month = 4;
      controllerResult = controller.CreateReminder( reminder ) as ViewResult;

      //Verify Controller Result
      Assert.That( controllerResult.ViewName == "Success" );
      Assert.That( (bool)controllerResult.ViewData["EmailVerified"], Is.False );
      //Verify no new email was sent out
      Assert.That( messageService.VerificationEmailsSentCount, Is.EqualTo( 1 ) );

      //Try to verify email address (emulate link clicking in email)
      VerificationController verificationController = new VerificationController();
      controllerResult = verificationController.VerifyMailAddress( verificationCode ) as ViewResult;

      //Verify Controller Result
      Assert.True( (bool)controllerResult.ViewData["VerificationSuccess"] );


      //Try to create another reminder, same mail address and now also verfied
      reminder.Name = "Karl Gustav";
      reminder.Day = 18;
      reminder.Month = 2;
      controllerResult = controller.CreateReminder( reminder ) as ViewResult;

      //Verify Controller Result
      Assert.That( controllerResult.ViewName == "Success" );
      Assert.That( (bool)controllerResult.ViewData["EmailVerified"], Is.True );

      //Verify no new email was sent out
      Assert.That( messageService.VerificationEmailsSentCount, Is.EqualTo( 1 ) );

    }


    [Test]
    public void GetNextRemindingDateTest()
    {
      var homeController = new HomeController();
      ReminderModel germanyModel = new ReminderModel()
      {
        Day = 7,
        Month = 6,
        TimeZoneOffset = -2 //Germany daylight saving time (sommerzeit)
      };

      //assert with date later than reminder date of this year
      var germanyActual = homeController.GetNextRemindingDate( germanyModel, DateTime.Now );
      DateTime germanyExpected = new DateTime( 2012, 6, 7, 0, 0, 0 );
      Assert.AreEqual( germanyActual, germanyExpected );

      //assert wiht date before reminder date
      DateTime beforeDate = new DateTime( 2009, 6, 4 );
      germanyActual = homeController.GetNextRemindingDate( germanyModel, beforeDate );
      germanyExpected = new DateTime( 2011, 6, 7, 0, 0, 0 );
      Assert.AreEqual( germanyActual, germanyExpected );
    }


    [Test]
    public void HourlyMaintenanceTest()
    {
      FakeMessageService messageMock = new FakeMessageService();
      //Create reminder with unknown email address at first
      string emailAddress = Guid.NewGuid().ToString() + "@gmx.de";
      ReminderModel reminder = GetReminder( emailAddress );

      var homeController = new HomeController( messageMock );
      homeController.CreateReminder( reminder );

      //Verify that no reminder mail has been sent
      Assert.That( messageMock.ReminderEmailsSentCount, Is.EqualTo( 0 ) );

      //Insert a remindii directly into the database
      int mailID = _db.GetEmailID( emailAddress );
      _db.AddReminder( reminder, mailID, DateTime.Now.AddDays( -1 ) );


      //Run hourly maintenance and check that no mail is sent out (email not verified yet)
      var maintenanceController = new MaintenanceController( messageMock );


      Assert.That( messageMock.ReminderEmailsSentCount, Is.EqualTo( 0 ) );

      //Verify user
      new VerificationController().VerifyMailAddress( messageMock.SentVerificationCode );

      //Run hourly maintenance and no the reminder should be sent out
      new MaintenanceController( messageMock ).Hourly( "1234" );
      Assert.That( messageMock.ReminderEmailsSentCount, Is.EqualTo( 1 ) );
      Assert.That( messageMock.SentoutReminder.EmailAddress, Is.EqualTo( emailAddress ) );
      Assert.That( messageMock.SentoutReminder.Name, Is.EqualTo( reminder.Name ) );

      //run hourly again and verify that no other reminder will be sent out
      new MaintenanceController( messageMock ).Hourly( "1234" );
      Assert.That( messageMock.ReminderEmailsSentCount, Is.EqualTo( 1 ) );
    }

    private static ReminderModel GetReminder(string emailAddress)
    {
      ReminderModel reminder = new ReminderModel()
      {
        Day = 7,
        Month = 6,
        Email = emailAddress,
        Name = "Patrick Epstein",
        Password = "Blah",
        TimeZoneOffset = -2
      };
      return reminder;
    }

    [Test]
    public void SendReminderTest()
    {
      ReminderTask reminder = new ReminderTask()
      {
        Date = DateTime.Now,
        EmailAddress = "bla@bla.com",
        ID = Guid.NewGuid(),
        Name = "Test Bday"
      };

      MailMessageService messageService = new MailMessageService(_logger);
      messageService.SendReminder( reminder );
    }
    [Test]
    public void SendVerificationMailTest()
    {
      MailMessageService messageService = new MailMessageService( _logger );
      messageService.SendVerificationEmail( "bla@test.com", Guid.NewGuid() );
    }

    [Test]
    public void GetReminderAndUserCountTest()
    {


      Console.WriteLine( "User Count = " + _db.GetUserCount() );
      Console.WriteLine( "Reminder Count = " + _db.GetReminderCount() );
    }
  }
}
