using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.DataAccess;
using System.Linq;
using SecretSanta.Matching;
using System;
using SecretSanta.Exceptions;
using SecretSanta.DataAccess.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SecretSanta.Controllers {
    public class MatchController : Controller {
        private IDataAccessor _dataAccessor;
        private ICreateSecretMatch _createSecretMatch;

        public MatchController(IDataAccessor dataAccessor, ICreateSecretMatch createSecretMatch) {
            _dataAccessor = dataAccessor;
            _createSecretMatch = createSecretMatch;
        }

        [HttpGet]
        public IActionResult GetMatch(SecretMatch secretMatch) {
            //verify access
            if (!verifySessionCookie(secretMatch.Name))
            {
                return View("InvalidCredentials");
            }

            secretMatch.Interests = _dataAccessor.GetUserInterests(secretMatch.Name);
            if (!string.IsNullOrEmpty(secretMatch.TheirSecretMatch))
            {
                secretMatch.MatchInterests = _dataAccessor.GetUserInterests(secretMatch.TheirSecretMatch);
            }
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);
            secretMatch.AllowMatching = allowMatch;
            secretMatch.UserIsAdmin = _dataAccessor.UserIsAdmin(secretMatch.Name);
            return View("GetMatch", secretMatch);
        }
        
        public IActionResult CreateMatch(SecretMatch secretMatch) {
            //verify access
            if (!verifySessionCookie(secretMatch.Name))
            {
                return View("InvalidCredentials");
            }
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);
            secretMatch.AllowMatching = allowMatch;
            if (string.IsNullOrEmpty(secretMatch?.Name) || !secretMatch.AllowMatching)
            { //How? Why? Just start over
                return RedirectToAction("SignIn");
            }
            secretMatch.TheirSecretMatch = _createSecretMatch.FindRandomMatch(secretMatch.Name);

            _dataAccessor.CreateMatch(secretMatch.Name, secretMatch.TheirSecretMatch, secretMatch.AllowReroll);

            secretMatch.Interests = _dataAccessor.GetUserInterests(secretMatch.Name);
            secretMatch.MatchInterests = _dataAccessor.GetUserInterests(secretMatch.TheirSecretMatch);

            return View("GetMatch", secretMatch);
        }

        public IActionResult RerollResult(SecretMatch secretMatch) {
            //verify access
            if (!verifySessionCookie(secretMatch.Name))
            {
                return View("InvalidCredentials");
            }
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);
            secretMatch.AllowMatching = allowMatch;
            if (string.IsNullOrEmpty(secretMatch?.Name) || !secretMatch.AllowMatching)
            { //How? Why? Just start over
                return RedirectToAction("SignIn");
            }

            if (!string.IsNullOrEmpty(secretMatch.TheirSecretMatch)) {
                _dataAccessor.RemoveMatch(secretMatch.Name, secretMatch.TheirSecretMatch);
                _dataAccessor.CreateRestriction(secretMatch.Name, secretMatch.TheirSecretMatch, false, false);
            }

            SecretMatch match = new SecretMatch() {
                Name = secretMatch.Name,
                AllowReroll = false
            };
            return RedirectToAction("CreateMatch", match);
        }

        [HttpGet]
        public IActionResult Register() {
            IList<Name> possibleNames = _dataAccessor.GetAllPossibleNames();
            bool.TryParse(_dataAccessor.GetSettingValue("AllowRegistration"), out bool allowRegister);
            return View("Register", new RegisterUser { PossibleNames = possibleNames, AllowRegister=allowRegister });
        }

        [HttpPost]
        public IActionResult Register(RegisterUser registration) {
            bool.TryParse(_dataAccessor.GetSettingValue("AllowRegistration"), out bool allowRegister);
            if (!allowRegister)
            {
                return View("SignIn", new AuthenticatedUser());
            }

            if (!string.Equals(registration.ChosenPassword, registration.VerifyPassword, StringComparison.Ordinal))
            {
                return View("PasswordsNotMatch");
            }
            if (_dataAccessor.AccountAlreadyRegistered(registration.NameToRegister)) {
                return View("AlreadyRegistered", registration);
            }

            _dataAccessor.RegisterAccount(registration.NameToRegister, registration.ChosenPassword);

            //get a new session for this user
            DataAccess.Models.ISession session = _dataAccessor.GetSession(registration.NameToRegister, registration.ChosenPassword);
            if (session == null) {
                throw new InvalidCredentialsException();
            }

            //store the cookie
            Response.Cookies.Append("sessionId", session.SessionId);

            return RedirectToAction("GetMatch", new SecretMatch { Name = registration.NameToRegister, AllowReroll = true });
        }

        [HttpGet]
        public IActionResult SignIn() {
            if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId))
            {
                DataAccess.Models.ISession session = _dataAccessor.GetSessionData(sessionId);

                Match existingMatch = _dataAccessor.GetExistingMatch(session.User);
                if (existingMatch == null)
                {
                    return RedirectToAction("GetMatch", new SecretMatch { Name = session.User, AllowReroll = true });
                }
                string myInterests = _dataAccessor.GetUserInterests(session.User);
                string theirInterests = _dataAccessor.GetUserInterests(existingMatch.MatchedName);

                return View("ExistingMatch", new SecretMatch { Name = session.User, AllowReroll = existingMatch.RerollAllowed, TheirSecretMatch = existingMatch.MatchedName, Interests = myInterests, MatchInterests = theirInterests });
            }
            return View("SignIn", new AuthenticatedUser());
        }

        [HttpPost]
        public IActionResult SignIn(AuthenticatedUser authUser) {
            try {
                //get a new session for this user
                DataAccess.Models.ISession session = _dataAccessor.GetSession(authUser.Username, authUser.Password);
                if (session == null) {
                    throw new InvalidCredentialsException();
                }

                //store the cookie
                Response.Cookies.Append("sessionId", session.SessionId);

                Match existingMatch = _dataAccessor.GetExistingMatch(authUser.Username);
                if (existingMatch == null) {
                    return RedirectToAction("GetMatch", new SecretMatch { Name = authUser.Username, AllowReroll = true });
                }
                string myInterests = _dataAccessor.GetUserInterests(authUser.Username);
                string theirInterests = _dataAccessor.GetUserInterests(existingMatch.MatchedName);

                return View("ExistingMatch", new SecretMatch { Name = authUser.Username, AllowReroll = existingMatch.RerollAllowed, TheirSecretMatch = existingMatch.MatchedName, Interests = myInterests, MatchInterests = theirInterests });
            }
            catch (InvalidCredentialsException) {
                return View("InvalidCredentials");
            }
            catch (Exception) {
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult UpdateInterests(SecretMatch match) {
            //verify access
            if (!verifySessionCookie(match.Name)) {
                return View("InvalidCredentials");
            }
            _dataAccessor.SetUserInterests(match.Name, match.Interests);
            if (!string.IsNullOrEmpty(match.TheirSecretMatch)) {
                return View("ExistingMatch", match);
            }
            return RedirectToAction("GetMatch", match);
        }

        [HttpGet]
        public IActionResult LogOut()
        {
            if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId))
            {
                _dataAccessor.EndSession(sessionId);
                HttpContext.Response.Cookies.Delete("sessionId");
            }
            return View("SignIn");
        }

        [HttpGet]
        public IActionResult OpenAdminPage() {
            return RedirectToAction("Index", "Admin");
        }

        private bool verifySessionCookie(string user) {
            if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
                return _dataAccessor.VerifySession(user, sessionId);
            }
            return false;
        }
    }
}