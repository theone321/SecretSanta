using Microsoft.AspNetCore.Mvc;
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
        _dataAccessor.SetUserAdmin(createdEventId, user.Id, true);

        return RedirectToAction("GetMatch", "Match", new { eventId = createdEventId });
      }

      return RedirectToAction("SignIn", "User");
    }

    [HttpGet]
    public IActionResult ChooseEvent() {
      if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
        var session = _dataAccessor.GetSessionData(sessionId);
        var user = _dataAccessor.GetUserByUserName(session.User);

        var model = new ChooseEventModel() {
          Events = _dataAccessor.GetEventsForUser(user.Id),
        };

        if (model.Events.Count == 1) {
          return RedirectToAction("GetMatch", "Match", new { eventId = model.Events.First().Id });
        }

        return View("Choose", model);
      }

      return RedirectToAction("SignIn", "User");
    }

    [HttpPost]
    public IActionResult ChooseEvent(int chosenEventId) {
      return RedirectToAction("GetMatch", "Match", new { eventId = chosenEventId });
    }

    [HttpGet]
    public IActionResult ConnectWithEvent() {
      return View("Join", new JoinEventModel());
    }

    [HttpPost]
    public IActionResult ConnectWithEvent(JoinEventModel model) {
      if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
        var session = _dataAccessor.GetSessionData(sessionId);
        var user = _dataAccessor.GetUserByUserName(session.User);
        var guid = new Guid(model.SharedEventGuid);
        _dataAccessor.AddUserToEvent(user.Id, guid);
        var theEvent = _dataAccessor.GetEventsForUser(user.Id).First(e => e.SharedId == guid);
        RedirectToAction("GetMatch", "Match", new { EventId = theEvent.Id });
      }

      return RedirectToAction("SignIn", "User");
    }
  }
}