using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecretSanta.Constants;
using SecretSanta.DataAccess;
using SecretSanta.Models.Event;
using SecretSanta.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Controllers {
  public class EventController : BaseController {
    public EventController(IDataAccessor dataAccessor, ISessionManager sessionManager)
      : base(sessionManager, dataAccessor) { }

    [HttpGet]
    public IActionResult NewEvent() {
      _sessionManager.SetCurrentEventId(0);

      var model = new CreateEventModel {
        SharedId = Guid.NewGuid(),
        EventTypes = BuildEventTypeList()
      };
      
      return View("Create", model);
    }

    [HttpPost]
    public IActionResult NewEvent(CreateEventModel newEvent) {
      if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
        var session = _dataAccessor.GetSessionData(sessionId);

        if (!ModelState.IsValid) {
          newEvent.EventTypes = BuildEventTypeList();
          return View("Create", newEvent);
        }

        var eventTypeId = int.Parse(newEvent.ChosenEventType);

        var dbEvent = new DataAccess.Models.Event {
          Description = newEvent.EventDescription,
          Name = newEvent.EventName,
          Location = newEvent.Location,
          SharedId = newEvent.SharedId,
          StartDate = newEvent.EventDate,
          EventType = eventTypeId
        };

        var user = _dataAccessor.GetUserByUserName(session.User);
        var createdEventId = _dataAccessor.CreateEvent(dbEvent, user.Id);
        
        _dataAccessor.AddSetting(AdminSettings.AllowRegistration, newEvent.AllowRegistration.ToString(), createdEventId);
        _dataAccessor.SetUserAdmin(createdEventId, user.Id, true);
        _sessionManager.SetCurrentEventId(createdEventId);
        
        return GoToEventUserPage(createdEventId);
      }

      return RedirectToAction("SignIn", "User");
    }

    [HttpGet]
    public IActionResult ChooseEvent() {
      _sessionManager.SetCurrentEventId(0);
      if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
        var session = _dataAccessor.GetSessionData(sessionId);
        var user = _dataAccessor.GetUserByUserName(session.User);

        var events = _dataAccessor.GetEventsForUser(user.Id);

        var model = new ChooseEventModel() {
          Events = new List<EventModel>()
        };

        foreach (var anEvent in events) {
          model.Events.Add(new EventModel {
            Id = anEvent.Id,
            Name = anEvent.Name,
            StartDate = anEvent.StartDate,
            Description = anEvent.Description,
            Location = anEvent.Location,
            SharedId = anEvent.SharedId,
            EventType = _dataAccessor.GetEventTypeName(anEvent.EventType)
          });
        }

        return View("Choose", model);
      }

      return RedirectToAction("SignIn", "User");
    }

    [HttpPost]
    public IActionResult ChooseEvent(int chosenEventId) {
      _sessionManager.SetCurrentEventId(chosenEventId);

      var theEvent = _dataAccessor.GetEvent(chosenEventId);

      return GoToEventUserPage(chosenEventId);
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

        return GoToEventUserPage(theEvent.Id);
      }

      return RedirectToAction("SignIn", "User");
    }

    [HttpGet]
    public IActionResult Attendees(int eventId) {
      var registeredUsernames = _dataAccessor.GetAllUsersForEvent(eventId).Select(n => n.RegisteredName).ToList();
      var theEvent = _dataAccessor.GetEvent(eventId);
      return View("Attendees", new AttendeesModel() {
        RegisteredNames = registeredUsernames,
        EventId = eventId,
        EventName = theEvent.Name
      });
    }

    public IActionResult OpenEventAdmin() {
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      var eventId = session.EventId;

      var theEvent = _dataAccessor.GetEvent(eventId);

      if (theEvent.EventType == 1) {
        return RedirectToAction("Index", "MatchEventAdmin");
      }
      else {
        return RedirectToAction("Index", "BirthdayEventAdmin");
      }
    }

    private List<SelectListItem> BuildEventTypeList() {
      var result = new List<SelectListItem>();

      var eventTypes = _dataAccessor.GetEventTypes();

      foreach (var eventType in eventTypes) {
        result.Add(new SelectListItem {
          Value = eventType.Id.ToString(),
          Text = eventType.Name
        });
      }

      return result;
    }
  }
}