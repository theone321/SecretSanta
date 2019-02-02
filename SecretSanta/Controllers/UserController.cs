using Microsoft.AspNetCore.Mvc;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Exceptions;
using SecretSanta.Models.User;
using SecretSanta.Models.Event;
using SecretSanta.Users;
using System;

namespace SecretSanta.Controllers {
  public class UserController : BaseController {
    private readonly IEventPageModelBuilder _pageModelBuilder;

    public UserController(IDataAccessor dataAccessor, ISessionManager sessionManager, IEventPageModelBuilder pageModelBuilder)
        : base(sessionManager, dataAccessor) {
      _pageModelBuilder = pageModelBuilder;
    }

    [HttpGet]
    public IActionResult Register() {
      return View("Register", new RegisterUserModel());
    }

    [HttpPost]
    public IActionResult Register(RegisterUserModel registration) {
      if (!string.Equals(registration.ChosenPassword, registration.VerifyPassword, StringComparison.Ordinal)) {
        return View("PasswordsNotMatch");
      }
      if (_dataAccessor.AccountAlreadyRegistered(registration.UserNameToRegister)) {
        return View("AlreadyRegistered", registration);
      }

      int id = _dataAccessor.RegisterAccount(registration.UserNameToRegister, registration.ChosenPassword);
      _dataAccessor.SetUserRealName(id, registration.RealName);
      //get a new session for this user
      ISession session = _dataAccessor.GetSession(registration.UserNameToRegister, registration.ChosenPassword);
      if (session == null) {
        throw new InvalidCredentialsException();
      }

      //store the cookie
      Response.Cookies.Append("sessionId", session.SessionId);

      return RedirectToAction("NewEvent", "Event");
    }

    [HttpGet]
    public IActionResult SignIn() {
      try {
        if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
          var session = _dataAccessor.GetSessionData(sessionId);
          if (session == null) {
            //remove the associated cookie
            Response.Cookies.Delete("sessionId");
            throw new InvalidCredentialsException();
          }

          var user = _dataAccessor.GetUserByUserName(session.User);
          var userEvents = _dataAccessor.GetEventsForUser(user.Id);

          if (userEvents.Count < 1) {
            return RedirectToAction("NewEvent", "Event");
          }
          else {
            return RedirectToAction("ChooseEvent", "Event");
          }
        }
        return View("SignIn", new AuthenticatedUser());
      }
      catch (InvalidCredentialsException) {
        return View("InvalidCredentials");
      }
      catch (UnregisteredUserException) {
        return View("InvalidCredentials");
      }
      catch (Exception) {
        return View("Error");
      }
    }

    [HttpPost]
    public IActionResult SignIn(AuthenticatedUser authUser) {
      try {
        //get a new session for this user
        var session = _dataAccessor.GetSession(authUser.Username, authUser.Password);
        if (session == null) {
          throw new InvalidCredentialsException();
        }

        //store the cookie
        Response.Cookies.Append("sessionId", session.SessionId);

        var user = _dataAccessor.GetUserByUserName(session.User);
        var userEvents = _dataAccessor.GetEventsForUser(user.Id);

        if (userEvents.Count < 1) {
          return RedirectToAction("NewEvent", "Event");
        }
        else {
          return RedirectToAction("ChooseEvent", "Event");
        }
      }
      catch (InvalidCredentialsException) {
        return View("InvalidCredentials");
      }
      catch (UnregisteredUserException) {
        return View("InvalidCredentials");
      }
      catch (Exception) {
        return View("Error");
      }
    }

    [HttpPost]
    public IActionResult UpdateInterests(EventPageModel user) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }
      _dataAccessor.SetUserInterests(user.UserId, user.Interests);
      var eventId = _sessionManager.GetCurrentEventId();
      if (user.TheirSecretMatchId > 0) {
        user = _pageModelBuilder.BuildEventPageModelFromDB(user.UserId, eventId);
        return View("UserPage", user);
      }
      return RedirectToAction("GetMatchEvent", "Match", new { eventId = _sessionManager.GetCurrentEventId() });
    }

    [HttpGet]
    public IActionResult SignOut() {
      if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
        _dataAccessor.EndSession(sessionId);
        HttpContext.Response.Cookies.Delete("sessionId");
        _sessionManager.EndSession();
      }
      return View("SignIn");
    }

    [HttpGet]
    public IActionResult OpenAdminPage() {
      return RedirectToAction("Index", "Admin", new { eventId = _sessionManager.GetCurrentEventId() });
    }

    [HttpGet]
    public IActionResult UpdatePassword() {
      if (!_sessionManager.TryGetSessionCookie(Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }
      var changePasswordModel = new ChangePasswordModel {
        EventId = session.EventId
      };
      return View("UpdatePassword", changePasswordModel);
    }

    [HttpPost]
    public IActionResult UpdatePassword(ChangePasswordModel changePasswordModel) {
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      //verify the passwords match, then verify the current password, then update
      if (!string.Equals(changePasswordModel.NewPassword, changePasswordModel.VerifyPassword, StringComparison.InvariantCulture)) {
        return View("PasswordsNotMatch");
      }

      //verify user
      if (!_dataAccessor.VerifyCredentials(session.User, changePasswordModel.CurrentPassword)) {
        return View("InvalidCredentials");
      }

      //update password
      User user = _dataAccessor.GetUserByUserName(session.User);
      _dataAccessor.UpdateUserPassword(user.Id, changePasswordModel.NewPassword);

      if (changePasswordModel.EventId > 0) {
        return RedirectToAction("GetMatchEvent", "Match", new { eventId = changePasswordModel.EventId });
      }
      else {
        return RedirectToAction("ChooseEvent", "Event");
      }
    }
  }
}