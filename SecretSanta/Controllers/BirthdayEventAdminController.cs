using Microsoft.AspNetCore.Mvc;
using SecretSanta.Constants;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Models.Event.Shared;
using SecretSanta.Models.EventAdmin.Birthday;
using SecretSanta.Users;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Controllers {
  public class BirthdayEventAdminController : BaseController {
    public BirthdayEventAdminController(IDataAccessor dataAccessor, ISessionManager sessionManager)
        : base(sessionManager, dataAccessor) { }

    public IActionResult Index() {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("LogIn", "User");
      }

      bool.TryParse(_dataAccessor.GetSettingValue("AllowRegistration", session.EventId), out bool allowRegistration);

      var theEvent = _dataAccessor.GetEvent(session.EventId);

      var giftIdeas = BuildGiftIdeasModel(theEvent.Id);

      var userList = BuildUserSettingsModel(theEvent.Id);

      var currentUserId = _dataAccessor.GetUserByUserName(session.User).Id;

      var model = new BirthdayEventAdminPageModel {
        UserName = session.User,
        UserId = currentUserId,
        SharedEventId = theEvent.SharedId,
        EventName = theEvent.Name,
        EventId = theEvent.Id,
        EventSettings = new EventSettingsModel {
          AllowRegistration = allowRegistration,
          EventName = theEvent.Name,
          EventDescription = theEvent.Description,
          Location = theEvent.Location,
          EventDate = theEvent.StartDate
        },
        GiftIdeas = giftIdeas,
        UserSettings = userList
      };

      return View(model);
    }

    [HttpPost]
    public IActionResult UpdateSettings(BirthdayEventAdminPageModel model) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }

      var eventId = model.EventId;

      if (!ModelState.IsValid) {
        var theEvent = _dataAccessor.GetEvent(eventId);
        model.SharedEventId = theEvent.SharedId;
        model.EventName = theEvent.Name;
        model.UserName = session.User;
        model.UserId = _dataAccessor.GetUserByUserName(session.User).Id;
        model.GiftIdeas = BuildGiftIdeasModel(eventId);
        model.UserSettings = BuildUserSettingsModel(eventId);
        return View("Index", model);
      }

      _dataAccessor.SetSettingValue(AdminSettings.AllowRegistration, model.EventSettings.AllowRegistration.ToString(), eventId);

      _dataAccessor.UpdateEvent(new Event {
        Id = model.EventId,
        Name = model.EventSettings.EventName,
        Description = model.EventSettings.EventDescription,
        Location = model.EventSettings.Location,
        StartDate = model.EventSettings.EventDate
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

    [HttpPost]
    public IActionResult RemoveGiftIdea(int eventItemId) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }

      _dataAccessor.RemoveEventItem(eventItemId);

      return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult AddGiftIdea(int eventId) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }

      var model = new NewEventItemModel {
        EventId = eventId,
        IsGiftIdea = true,
        IsBroughtItem = false,
        FromEventAdmin = true,
        ControllerName = "BirthdayEventAdmin",
        ActionName = "AddGiftIdea"
      };

      return View("AddEventItem", model);
    }

    [HttpPost]
    public IActionResult AddGiftIdea(NewEventItemModel model) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }

      _dataAccessor.AddEventItem(new EventItem {
        EventId = model.EventId,
        IsGiftIdea = model.IsGiftIdea,
        IsBroughtItem = model.IsBroughtItem,
        ItemText = model.ItemText
      });

      return RedirectToAction("Index");
    }

    private List<EventAdminUserSettingsModel> BuildUserSettingsModel(int eventId) {
      var displayList = new List<EventAdminUserSettingsModel>();
      var users = _dataAccessor.GetAllUsersForEvent(eventId);
      foreach (var user in users) {
        var isAdmin = _dataAccessor.GetEventAdmins(eventId).Any(ea => ea.Id == user.Id);
        var display = new EventAdminUserSettingsModel {
          UserId = user.Id,
          Name = user.RegisteredName,
          UserName = user.UserName,
          IsAdmin = isAdmin
        };

        displayList.Add(display);
      }

      return displayList;
    }

    private List<GiftIdeaModel> BuildGiftIdeasModel(int eventId) {
      var giftIdeas = _dataAccessor.GetItemsForEvent(eventId).Where(ei => ei.IsGiftIdea);
      var giftIdeasModel = new List<GiftIdeaModel>();
      foreach (var giftIdea in giftIdeas) {
        giftIdeasModel.Add(new GiftIdeaModel {
          Id = giftIdea.Id,
          GiftIdeaText = giftIdea.ItemText
        });
      }
      return giftIdeasModel;
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