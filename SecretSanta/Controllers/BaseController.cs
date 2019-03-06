using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SecretSanta.DataAccess;
using SecretSanta.Users;
using System.Linq;

namespace SecretSanta.Controllers {
  public class BaseController : Controller {
    protected readonly ISessionManager _sessionManager;
    protected readonly IDataAccessor _dataAccessor;

    protected BaseController(ISessionManager sessionManager, IDataAccessor dataAccessor) {
      _sessionManager = sessionManager;
      _dataAccessor = dataAccessor;
    }

    public override void OnActionExecuting(ActionExecutingContext context) {
      SetTempDataValues();
    }

    public override void OnActionExecuted(ActionExecutedContext context) {
      SetTempDataValues();
    }

    public IActionResult GoToEventUserPage(int eventId) {
      var theEvent = _dataAccessor.GetEvent(eventId);
      if (theEvent.EventType == 1) {
        return RedirectToAction("GetMatchEvent", "Match", new { eventId = eventId });
      }
      else {
        return RedirectToAction("GetBirthdayEvent", "Birthday", new { eventId = eventId });
      }
    }

    public IActionResult GoToEventAdminPage(int eventId) {
      var theEvent = _dataAccessor.GetEvent(eventId);
      if (theEvent.EventType == 1) {
        return RedirectToAction("Index", "MatchEventAdmin");
      }
      else {
        return RedirectToAction("Index", "BirthdayEventAdmin");
      }
    }

    private void SetTempDataValues() {
      if (_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        var user = _dataAccessor.GetUserByUserName(session.User);

        TempData["IsUserSignedIn"] = true;
        TempData["EventId"] = session.EventId;
        TempData["UserHasMultipleEvents"] = _dataAccessor.GetEventsForUser(user.Id).Count > 1;
        TempData["IsUserSuperAdmin"] = user.IsSuperAdmin;

        if (session.EventId > 0) {
          var isEventAdmin = _dataAccessor.GetEventAdmins(session.EventId).Any(u => u.Id == user.Id);
          TempData["IsUserAdmin"] = isEventAdmin;
        }
        else {
          TempData["IsUserAdmin"] = false;
        }
      }
      else {
        TempData["IsUserSignedIn"] = false;
        TempData["IsUserAdmin"] = false;
        TempData["UserHasMultipleEvents"] = false;
        TempData["IsUserSuperAdmin"] = false;
      }
    }
  }
}