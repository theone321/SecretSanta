using Microsoft.AspNetCore.Mvc;
using SecretSanta.Constants;
using SecretSanta.DataAccess;
using SecretSanta.Models;
using SecretSanta.Users;
using System;
using System.Linq;

namespace SecretSanta.Controllers {
  public class EventController : BaseController {
    public EventController(IDataAccessor dataAccessor, ISessionManager sessionManager)
      : base(sessionManager, dataAccessor) { }

    [HttpGet]
    public IActionResult NewEvent() {
      _sessionManager.SetCurrentEventId(0);
      return View("Create", new Event { SharedId = new Guid() });
    }

    [HttpPost]
    public IActionResult NewEvent(Event newEvent) {
      if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
        var session = _dataAccessor.GetSessionData(sessionId);

        var dbEvent = new DataAccess.Models.Event {
          Description = newEvent.EventDescription,
          Name = newEvent.EventName,
          Location = newEvent.Location,
          SharedId = newEvent.SharedId,
          StartDate = newEvent.EventDate
        };

        var user = _dataAccessor.GetUserByUserName(session.User);
        var createdEventId = _dataAccessor.CreateEvent(dbEvent, user.Id);
        _dataAccessor.AddSetting(AdminSettings.AllowMatching, newEvent.AllowMatching.ToString(), createdEventId);
        _dataAccessor.AddSetting(AdminSettings.AllowRegistration, newEvent.AllowRegistration.ToString(), createdEventId);
        _dataAccessor.AddSetting(AdminSettings.SessionTimeout, "15", createdEventId);
        _dataAccessor.SetUserAdmin(createdEventId, user.Id, true);
        _sessionManager.SetCurrentEventId(createdEventId);

        return RedirectToAction("GetMatch", "Match", new { eventId = createdEventId });
      }

      return RedirectToAction("SignIn", "User");
    }

    [HttpGet]
    public IActionResult ChooseEvent() {
      _sessionManager.SetCurrentEventId(0);
      if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
        var session = _dataAccessor.GetSessionData(sessionId);
        var user = _dataAccessor.GetUserByUserName(session.User);

        var model = new ChooseEventModel() {
          Events = _dataAccessor.GetEventsForUser(user.Id),
        };

        //if (model.Events.Count == 1) {
        //  return RedirectToAction("GetMatch", "Match", new { eventId = model.Events.First().Id });
        //}

        return View("Choose", model);
      }

      return RedirectToAction("SignIn", "User");
    }

    [HttpPost]
    public IActionResult ChooseEvent(int chosenEventId) {
      _sessionManager.SetCurrentEventId(chosenEventId);
      return RedirectToAction("GetMatch", "Match", new { eventId = chosenEventId });
    }

    [HttpGet]
    public IActionResult ConnectWithEvent() {
      _sessionManager.SetCurrentEventId(0);
      return View("Join", new JoinEventModel());
    }

    [HttpPost]
    public IActionResult ConnectWithEvent(JoinEventModel model) {
      if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
        var guid = new Guid(model.SharedEventGuid);

        var theEvent = _dataAccessor.GetEvent(guid);
        var allowRegistration = _dataAccessor.GetSettingValue(AdminSettings.AllowRegistration, theEvent.Id);
        if (bool.TryParse(allowRegistration, out var allowRegister) && !allowRegister) {
          return View("CannotJoinEvent");
        }

        var session = _dataAccessor.GetSessionData(sessionId);
        var user = _dataAccessor.GetUserByUserName(session.User);

        _dataAccessor.AddUserToEvent(user.Id, guid);
        _sessionManager.SetCurrentEventId(theEvent.Id);
        return RedirectToAction("GetMatch", "Match", new { EventId = theEvent.Id });
      }

      return RedirectToAction("SignIn", "User");
    }
  }
}