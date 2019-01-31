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
    private readonly IPageModelBuilder _pageModelBuilder;

    public MatchController(IDataAccessor dataAccessor, ICreateSecretMatch createSecretMatch,
        ISessionManager sessionManager, IPageModelBuilder pageModelBuilder)
        : base(sessionManager, dataAccessor) {
      _createSecretMatch = createSecretMatch;
      _pageModelBuilder = pageModelBuilder;
    }

    [HttpGet]
    public IActionResult Index() {
      var registeredUsernames = _dataAccessor.GetAllUsers().Select(n => n.RegisteredName).ToList();
      int matchCount = _dataAccessor.GetAllExistingMatchesForEvent(_sessionManager.GetCurrentEventId()).Count;
      return View("Index", new IndexModel() { RegisteredNames = registeredUsernames, MatchCounts = matchCount });
    }

    [HttpGet]
    public IActionResult GetMatch(int eventId) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      _sessionManager.SetCurrentEventId(eventId);

      var userModel = _pageModelBuilder.BuildUserPageModelFromDB(session.User, eventId);
      return View("UserPage", userModel);
    }

    public IActionResult CreateMatch(UserPageModel userModel) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }
      bool.TryParse(_dataAccessor.GetSettingValue(AdminSettings.AllowMatching, session.EventId), out var allowMatch);
      userModel.AllowMatching = allowMatch;
      if (userModel.UserId <= 0 || !userModel.AllowMatching) {
        //How? Why? Just start over
        return RedirectToAction("SignIn", "User");
      }

      var eventId = _sessionManager.GetCurrentEventId();

      userModel.TheirSecretMatchId = _createSecretMatch.FindRandomMatch(userModel.UserId, eventId);
      userModel.TheirSecretMatchName = _dataAccessor.GetUserById(userModel.TheirSecretMatchId).RegisteredName;
      _dataAccessor.CreateMatch(userModel.UserId, userModel.TheirSecretMatchId, userModel.AllowReroll, eventId);

      userModel = _pageModelBuilder.BuildUserPageModelFromDB(userModel.UserId, userModel.EventId);

      return View("UserPage", userModel);
    }

    public IActionResult RerollResult(UserPageModel userModel) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }
      bool.TryParse(_dataAccessor.GetSettingValue(AdminSettings.AllowMatching, session.EventId), out var allowMatch);
      userModel.AllowMatching = allowMatch;
      if (userModel.UserId <= 0 || !userModel.AllowMatching) {
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
    public IActionResult MakeHardRestriction(UserPageModel userModel) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      var eventId = _sessionManager.GetCurrentEventId();

      if (userModel.SignificantOther?.UserId > 0) {
        _dataAccessor.CreateRestriction(userModel.UserId, userModel.SignificantOther.UserId, true, false, eventId);
      }

      userModel = _pageModelBuilder.BuildUserPageModelFromDB(userModel.UserId, eventId);

      return View("UserPage", userModel);
    }
  }
}