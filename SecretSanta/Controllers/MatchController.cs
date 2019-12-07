using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Constants;
using SecretSanta.DataAccess;
using SecretSanta.Matching;
using SecretSanta.Models.Event.SecretMatch;
using SecretSanta.Users.SecretMatch;
using SecretSanta.Users;

namespace SecretSanta.Controllers
{
    public class MatchController : BaseController
    {
        private readonly ICreateSecretMatch _createSecretMatch;
        private readonly IMatchEventPageModelBuilder _eventPageModelBuilder;

        private static readonly object _lockObject = new object();

        public MatchController(IDataAccessor dataAccessor, ICreateSecretMatch createSecretMatch,
            ISessionManager sessionManager, IMatchEventPageModelBuilder eventPageModelBuilder)
            : base(sessionManager, dataAccessor)
        {
            _createSecretMatch = createSecretMatch;
            _eventPageModelBuilder = eventPageModelBuilder;
        }

        [HttpGet]
        public IActionResult GetMatchEvent(int eventId)
        {
            //verify access
            if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session))
            {
                return View("InvalidCredentials");
            }

            _sessionManager.SetCurrentEventId(eventId);

            var userModel = _eventPageModelBuilder.BuildEventPageModelFromDB(session.User, eventId);
            return View("MatchUserPage", userModel);
        }

        public IActionResult CreateMatch(MatchEventPageModel userModel)
        {
            //verify access
            if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session))
            {
                return View("InvalidCredentials");
            }
            bool.TryParse(_dataAccessor.GetSettingValue(AdminSettings.AllowMatching, session.EventId), out var allowMatch);
            userModel.AllowMatching = allowMatch;
            if (userModel.UserId <= 0 || !userModel.AllowMatching)
            {
                //How? Why? Just start over
                return RedirectToAction("SignIn", "User");
            }

            var eventId = _sessionManager.GetCurrentEventId();
            lock (_lockObject)
            {
                if (_dataAccessor.GetExistingMatch(userModel.UserId, eventId) == null)
                {
                    userModel.TheirSecretMatchId = _createSecretMatch.FindRandomMatch(userModel.UserId, eventId);
                    userModel.TheirSecretMatchName = _dataAccessor.GetUserById(userModel.TheirSecretMatchId).RegisteredName;
                    _dataAccessor.CreateMatch(userModel.UserId, userModel.TheirSecretMatchId, userModel.AllowReroll, eventId);
                }
            }
            userModel = _eventPageModelBuilder.BuildEventPageModelFromDB(userModel.UserId, eventId);

            return View("MatchUserPage", userModel);
        }

        public IActionResult RerollResult(MatchEventPageModel userModel)
        {
            //verify access
            if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session))
            {
                return View("InvalidCredentials");
            }
            bool.TryParse(_dataAccessor.GetSettingValue(AdminSettings.AllowMatching, session.EventId), out var allowMatch);
            userModel.AllowMatching = allowMatch;
            if (userModel.UserId <= 0 || !userModel.AllowMatching)
            {
                //How? Why? Just start over
                return RedirectToAction("SignIn", "User");
            }

            if (userModel.TheirSecretMatchId > 0)
            {
                var eventId = _sessionManager.GetCurrentEventId();
                _dataAccessor.RemoveMatch(userModel.UserId, userModel.TheirSecretMatchId, eventId);
                _dataAccessor.CreateRestriction(userModel.UserId, userModel.TheirSecretMatchId, false, false, eventId);
            }
            userModel.AllowReroll = false;
            return RedirectToAction("CreateMatch", userModel);
        }

        [HttpPost]
        public IActionResult MakeHardRestriction(MatchEventPageModel userModel)
        {
            //verify access
            if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session))
            {
                return View("InvalidCredentials");
            }

            var eventId = _sessionManager.GetCurrentEventId();

            if (userModel.SignificantOther?.UserId > 0)
            {
                _dataAccessor.CreateRestriction(userModel.UserId, userModel.SignificantOther.UserId, true, false, eventId);
            }

            userModel = _eventPageModelBuilder.BuildEventPageModelFromDB(userModel.UserId, eventId);

            return View("MatchUserPage", userModel);
        }
    }
}