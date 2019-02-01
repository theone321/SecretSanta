using Microsoft.AspNetCore.Mvc;
using SecretSanta.Constants;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Matching;
using SecretSanta.Models;
using SecretSanta.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Controllers {
  public class AdminController : BaseController {
    private readonly ICreateSecretMatch _createSecretMatch;

    public AdminController(IDataAccessor dataAccessor, ICreateSecretMatch createSecretMatch, ISessionManager sessionManager)
        : base(sessionManager, dataAccessor) {
      _createSecretMatch = createSecretMatch;
    }

    [HttpGet]
    public IActionResult Index() {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("LogIn", "User");
      }

      bool.TryParse(_dataAccessor.GetSettingValue("AllowRegistration", session.EventId), out bool allowRegistration);
      bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching", session.EventId), out bool allowMatching);

      var theEvent = _dataAccessor.GetEvent(session.EventId);

      List<UserAdminSettings> displayList = new List<UserAdminSettings>();
      IList<User> users = _dataAccessor.GetAllUsersForEvent(theEvent.Id);
      IList<Match> matches = _dataAccessor.GetAllExistingMatchesForEvent(theEvent.Id);
      User currentUser = null;
      foreach (User user in users) {
        UserAdminSettings display = new UserAdminSettings {
          UserId = user.Id,
          Name = user.RegisteredName,
          UserName = user.UserName,
          HasMatched = matches.Any(m => m.RequestorId == user.Id),
          IsMatched = matches.Any(m => m.MatchedId == user.Id),
          //IsAdmin = user.IsAdmin
        };

        displayList.Add(display);

        if (string.Equals(user.UserName, session.User, StringComparison.Ordinal)) {
          currentUser = user;
        }
      }

      AdminModel options = new AdminModel {
        User = session.User,
        UserId = currentUser?.Id ?? 0,
        AllowRegistration = allowRegistration,
        AllowMatching = allowMatching,
        SharedEventId = theEvent.SharedId,
        EventName = theEvent.Name,
        EventId = theEvent.Id,
        UserList = displayList
      };

      return View(options);
    }

    [HttpPost]
    public IActionResult UpdateSettings(AdminModel options) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }

      _dataAccessor.SetSettingValue(AdminSettings.AllowRegistration, options.AllowRegistration.ToString(), session.EventId);
      _dataAccessor.SetSettingValue(AdminSettings.AllowMatching, options.AllowMatching.ToString(), session.EventId);

      return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ToggleAdminAccess(int userId) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }
      User user = _dataAccessor.GetUserById(userId);
      var eventId = _sessionManager.GetCurrentEventId();
      var userIsAnAdminForThisEvent = _dataAccessor.GetEventAdmins(eventId).Any(u => u.Id == userId);
      _dataAccessor.SetUserAdmin(eventId, userId, !userIsAnAdminForThisEvent);
      return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ResetUserPassword(int userId) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }
      _dataAccessor.UpdateUserPassword(userId, "password");
      return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeRegisterUser(int userId) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }
      _dataAccessor.DeRegisterAccount(userId);
      return RedirectToAction("Index");
    }

    private bool VerifyAccess(out ISession session) {
      var sessionExists = _sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out session);
      if (sessionExists) {
        var user = _dataAccessor.GetUserByUserName(session.User);
        var eventId = session.EventId;
        var userIsAdminForThisEvent = _dataAccessor.GetEventAdmins(eventId).Any(u => u.Id == user.Id);
        return userIsAdminForThisEvent;
      }
      return false;
    }
  }
}