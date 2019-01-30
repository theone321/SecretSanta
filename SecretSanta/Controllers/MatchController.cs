using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
      int matchCount = _dataAccessor.GetAllExistingMatches().Count;
      return View("Index", new IndexModel() { RegisteredNames = registeredUsernames, MatchCounts = matchCount });
    }

    [HttpGet]
    public IActionResult GetMatch(int eventId) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      _sessionManager.SetCurrentEventId(eventId);

      var userModel = _pageModelBuilder.BuildUserPageModelFromDB(session.User);
      return View("UserPage", userModel);
    }

    public IActionResult CreateMatch(UserPageModel userModel) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }
      bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out var allowMatch);
      userModel.AllowMatching = allowMatch;
      if (userModel.UserId <= 0 || !userModel.AllowMatching) {
        //How? Why? Just start over
        return RedirectToAction("SignIn", "User");
      }

      userModel.TheirSecretMatchId = _createSecretMatch.FindRandomMatch(userModel.UserId);
      userModel.TheirSecretMatchName = _dataAccessor.GetUserById(userModel.TheirSecretMatchId).RegisteredName;
      _dataAccessor.CreateMatch(userModel.UserId, userModel.TheirSecretMatchId, userModel.AllowReroll);

      userModel = _pageModelBuilder.BuildUserPageModelFromDB(userModel.UserId);

      return View("UserPage", userModel);
    }

    public IActionResult RerollResult(UserPageModel userModel) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }
      bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out var allowMatch);
      userModel.AllowMatching = allowMatch;
      if (userModel.UserId <= 0 || !userModel.AllowMatching) {
        //How? Why? Just start over
        return RedirectToAction("SignIn", "User");
      }

      if (userModel.TheirSecretMatchId > 0) {
        _dataAccessor.RemoveMatch(userModel.UserId, userModel.TheirSecretMatchId);
        _dataAccessor.CreateRestriction(userModel.UserId, userModel.TheirSecretMatchId, false, false);
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

      if (userModel.SignificantOther?.UserId > 0) {
        _dataAccessor.CreateRestriction(userModel.UserId, userModel.SignificantOther.UserId, true, false);
      }

      userModel = _pageModelBuilder.BuildUserPageModelFromDB(userModel.UserId);

      return View("UserPage", userModel);
    }
  }
}