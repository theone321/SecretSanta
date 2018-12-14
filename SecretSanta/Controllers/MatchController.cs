using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.DataAccess;
using System.Linq;
using SecretSanta.Matching;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SecretSanta.Users;

namespace SecretSanta.Controllers {
    public class MatchController : Controller {
        private readonly IDataAccessor _dataAccessor;
        private readonly ICreateSecretMatch _createSecretMatch;
        private readonly ISessionManager _sessionManager;
        private readonly IPageModelBuilder _pageModelBuilder;

        public MatchController(IDataAccessor dataAccessor, ICreateSecretMatch createSecretMatch, 
            ISessionManager sessionManager, IPageModelBuilder pageModelBuilder) {
            _dataAccessor = dataAccessor;
            _createSecretMatch = createSecretMatch;
            _sessionManager = sessionManager;
            _pageModelBuilder = pageModelBuilder;
        }

        [HttpGet]
        public IActionResult Index() {
            List<string> registered = _dataAccessor.GetAllUsers().Select(n => n.RegisteredName).ToList();
            int matchCount = _dataAccessor.GetAllExistingMatches().Count;
            return View("Index", new IndexModel() { RegisteredNames = registered, MatchCounts = matchCount });
        }

        [HttpGet]
        public IActionResult GetMatch() {
            //verify access
            if (!VerifySessionCookie()) {
                return View("InvalidCredentials");
            }
            UserPageModel userModel = _pageModelBuilder.BuildUserPageModelFromDB(_sessionManager.GetSession().User);
            return View("UserPage", userModel);
        }
        
        public IActionResult CreateMatch(UserPageModel userModel) {
            //verify access
            if (!VerifySessionCookie()) {
                return View("InvalidCredentials");
            }
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);
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
            if (!VerifySessionCookie()) {
                return View("InvalidCredentials");
            }
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);
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
            if (!VerifySessionCookie()) {
                return View("InvalidCredentials");
            }

            if (userModel.SignificantOther?.UserId > 0) { 
                _dataAccessor.CreateRestriction(userModel.UserId, userModel.SignificantOther.UserId, true, false);
            }

            userModel = _pageModelBuilder.BuildUserPageModelFromDB(userModel.UserId);

            return View("UserPage", userModel);
        }

        private bool VerifySessionCookie() {
            return _sessionManager.VerifySessionCookie(HttpContext.Request.Cookies);
        }
    }
}