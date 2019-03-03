using Microsoft.AspNetCore.Mvc;
using SecretSanta.Constants;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Models.EventAdmin;
using SecretSanta.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Controllers {
  public class EventAdminController : BaseController {
    public EventAdminController(IDataAccessor dataAccessor, ISessionManager sessionManager)
        : base(sessionManager, dataAccessor) { }

    [HttpGet]
    public IActionResult Index() {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("LogIn", "User");
      }

      bool.TryParse(_dataAccessor.GetSettingValue("AllowRegistration", session.EventId), out bool allowRegistration);
      bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching", session.EventId), out bool allowMatching);

      var theEvent = _dataAccessor.GetEvent(session.EventId);

      var displayList = BuildUserSettingsModel(theEvent.Id);

      var currentUserId = _dataAccessor.GetUserByUserName(session.User).Id;

      var options = new EventAdminPageModel {
        UserName = session.User,
        UserId = currentUserId,
        SharedEventId = theEvent.SharedId,
        EventName = theEvent.Name,
        EventId = theEvent.Id,
        EventSettings = new EventSettingsModel {
          AllowRegistration = allowRegistration,
          AllowMatching = allowMatching,
          EventName = theEvent.Name,
          EventDescription = theEvent.Description,
          Location = theEvent.Location,
          EventDate = theEvent.StartDate
        },
        UserSettings = displayList
      };

      return View(options);
    }

    [HttpPost]
    public IActionResult UpdateSettings(EventAdminPageModel options) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }

      var eventId = options.EventId;

      if (!ModelState.IsValid) {
        var theEvent = _dataAccessor.GetEvent(eventId);
        options.SharedEventId = theEvent.SharedId;
        options.EventName = theEvent.Name;
        options.UserName = session.User;
        options.UserId = _dataAccessor.GetUserByUserName(session.User).Id;
        options.UserSettings = BuildUserSettingsModel(eventId);
        return View("Index", options);
      }

      _dataAccessor.SetSettingValue(AdminSettings.AllowRegistration, options.EventSettings.AllowRegistration.ToString(), eventId);
      _dataAccessor.SetSettingValue(AdminSettings.AllowMatching, options.EventSettings.AllowMatching.ToString(), eventId);

      _dataAccessor.UpdateEvent(new Event {
        Id = options.EventId,
        Name = options.EventSettings.EventName,
        Description = options.EventSettings.EventDescription,
        Location = options.EventSettings.Location,
        StartDate = options.EventSettings.EventDate
      });

      return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ToggleAdminAccess(int userId) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }
      var user = _dataAccessor.GetUserById(userId);
      var eventId = _sessionManager.GetCurrentEventId();
      var userIsAnAdminForThisEvent = _dataAccessor.GetEventAdmins(eventId).Any(u => u.Id == userId);
      _dataAccessor.SetUserAdmin(eventId, userId, !userIsAnAdminForThisEvent);
      return RedirectToAction("Index");
    }
    
    [HttpPost]
    public IActionResult RemoveUserFromEvent(int userId) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }

      var user = _dataAccessor.GetUserById(userId);
      var eventId = _sessionManager.GetCurrentEventId();
      _dataAccessor.RemoveUserFromEvent(user.Id, eventId);
      //We want a new Shared ID for this event so the removed user cannot easily rejoin.
      _dataAccessor.RegenerateSharedIdForEvent(eventId);
      return RedirectToAction("Index");
    }

    private List<EventAdminUserSettingsModel> BuildUserSettingsModel(int eventId) {
      var displayList = new List<EventAdminUserSettingsModel>();
      var users = _dataAccessor.GetAllUsersForEvent(eventId);
      var matches = _dataAccessor.GetAllExistingMatchesForEvent(eventId);
      foreach (var user in users) {
        var isAdmin = _dataAccessor.GetEventAdmins(eventId).Any(ea => ea.Id == user.Id);
        var display = new EventAdminUserSettingsModel {
          UserId = user.Id,
          Name = user.RegisteredName,
          UserName = user.UserName,
          HasMatched = matches.Any(m => m.RequestorId == user.Id),
          IsMatched = matches.Any(m => m.MatchedId == user.Id),
          IsAdmin = isAdmin
        };

        displayList.Add(display);
      }

      return displayList;
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