using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Constants;
using SecretSanta.DataAccess;
using SecretSanta.Matching;
using SecretSanta.Models;
using SecretSanta.Users;
using System.Linq;

namespace SecretSanta.Controllers {
  public class MatchController : BaseController {
    private readonly ICreateSecretMatch _createSecretMatch;
    private readonly IEventPageModelBuilder _pageModelBuilder;

    public MatchController(IDataAccessor dataAccessor, ICreateSecretMatch createSecretMatch,
        ISessionManager sessionManager, IEventPageModelBuilder pageModelBuilder)
        : base(sessionManager, dataAccessor) {
      _createSecretMatch = createSecretMatch;
      _pageModelBuilder = pageModelBuilder;
    }

    [HttpGet]
    public IActionResult Index(int eventId) {
      var registeredUsernames = _dataAccessor.GetAllUsersForEvent(eventId).Select(n => n.RegisteredName).ToList();
      int matchCount = _dataAccessor.GetAllExistingMatchesForEvent(eventId).Count;
      return View("Index", new IndexModel() { RegisteredNames = registeredUsernames, MatchCounts = matchCount, EventId = eventId });
    }

    //Rename this action... it's weird. You're not getting a match, just loading the user's page for a certain event.
    //Maybe move to User or Event controller?
    [HttpGet]
    public IActionResult GetMatch(int eventId) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      _sessionManager.SetCurrentEventId(eventId);

      var userModel = _pageModelBuilder.BuildEventPageModelFromDB(session.User, eventId);
      return View("UserPage", userModel);
    }

    public IActionResult CreateMatch(EventPageModel userModel) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }
      bool.TryParse(_dataAccessor.GetSettingValue(AdminSettings.AllowMatching, session.EventId), out var allowMatch);
      userModel.Event.AllowMatching = allowMatch;
      if (userModel.UserId <= 0 || !userModel.Event.AllowMatching) {
        //How? Why? Just start over
        return RedirectToAction("SignIn", "User");
      }

      var eventId = _sessionManager.GetCurrentEventId();

      userModel.TheirSecretMatchId = _createSecretMatch.FindRandomMatch(userModel.UserId, eventId);
      userModel.TheirSecretMatchName = _dataAccessor.GetUserById(userModel.TheirSecretMatchId).RegisteredName;
      _dataAccessor.CreateMatch(userModel.UserId, userModel.TheirSecretMatchId, userModel.AllowReroll, eventId);

      userModel = _pageModelBuilder.BuildEventPageModelFromDB(userModel.UserId, userModel.EventId);

      return View("UserPage", userModel);
    }

    public IActionResult RerollResult(EventPageModel userModel) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }
      bool.TryParse(_dataAccessor.GetSettingValue(AdminSettings.AllowMatching, session.EventId), out var allowMatch);
      userModel.Event.AllowMatching = allowMatch;
      if (userModel.UserId <= 0 || !userModel.Event.AllowMatching) {
        //How? Why? Just start over
        return RedirectToAction("SignIn", "User");
      }

      if (userModel.TheirSecretMatchId > 0) {
        var eventId = _sessionManager.GetCurrentEventId();
        _dataAccessor.RemoveMatch(userModel.UserId, userModel.TheirSecretMatchId, eventId);
        _dataAccessor.CreateRestriction(userModel.UserId, userModel.TheirSecretMatchId, false, false, eventId);
      }
      userModel.AllowReroll = false;
      return RedirectToAction("CreateMatch", userModel);
    }

    [HttpPost]
    public IActionResult MakeHardRestriction(EventPageModel userModel) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      var eventId = _sessionManager.GetCurrentEventId();

      if (userModel.SignificantOther?.UserId > 0) {
        _dataAccessor.CreateRestriction(userModel.UserId, userModel.SignificantOther.UserId, true, false, eventId);
      }

      userModel = _pageModelBuilder.BuildEventPageModelFromDB(userModel.UserId, eventId);

      return View("UserPage", userModel);
    }
  }
}