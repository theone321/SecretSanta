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
        private DataAccess.Models.ISession _session;

        public MatchController(IDataAccessor dataAccessor, ICreateSecretMatch createSecretMatch) {
            _dataAccessor = dataAccessor;
            _createSecretMatch = createSecretMatch;
        }

        [HttpGet]
        public IActionResult Index() {
            List<string> registered = _dataAccessor.GetAllRegisteredNames().Select(n => n.RegisteredName).ToList();
            int matchCount = _dataAccessor.GetAllExistingMatches().Count;
            return View("Index", new IndexModel() { RegisteredNames = registered, MatchCounts = matchCount });
        }

        [HttpGet]
        public IActionResult GetMatch() {
            //verify access
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }
            UserPageModel secretMatch = buildUserPageModelFromDB(_session.User);
            return View("UserPage", secretMatch);
        }
        
        public IActionResult CreateMatch(UserPageModel secretMatch) {
            //verify access
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);
            secretMatch.AllowMatching = allowMatch;
            if (string.IsNullOrEmpty(secretMatch?.Name) || !secretMatch.AllowMatching) {
                //How? Why? Just start over
                return RedirectToAction("SignIn");
            }

            secretMatch.TheirSecretMatch = _createSecretMatch.FindRandomMatch(secretMatch.Name);
            _dataAccessor.CreateMatch(secretMatch.Name, secretMatch.TheirSecretMatch, secretMatch.AllowReroll);

            secretMatch = buildUserPageModelFromDB(secretMatch.Name);

            return View("UserPage", secretMatch);
        }

        public IActionResult RerollResult(UserPageModel secretMatch) {
            //verify access
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);
            secretMatch.AllowMatching = allowMatch;
            if (string.IsNullOrEmpty(secretMatch?.Name) || !secretMatch.AllowMatching) { 
                //How? Why? Just start over
                return RedirectToAction("SignIn");
            }

            if (!string.IsNullOrEmpty(secretMatch.TheirSecretMatch)) {
                _dataAccessor.RemoveMatch(secretMatch.Name, secretMatch.TheirSecretMatch);
                _dataAccessor.CreateRestriction(secretMatch.Name, secretMatch.TheirSecretMatch, false, false);
            }

            UserPageModel match = new UserPageModel() {
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
            if (!allowRegister) {
                return View("SignIn", new AuthenticatedUser());
            }

            if (!string.Equals(registration.ChosenPassword, registration.VerifyPassword, StringComparison.Ordinal)) {
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

            return RedirectToAction("GetMatch");
        }

        [HttpGet]
        public IActionResult SignIn() {
            try {
                if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
                    _session = _dataAccessor.GetSessionData(sessionId);
                    if (_session == null) {
                        //remove the associated cookie
                        Response.Cookies.Delete("sessionId");
                        throw new InvalidCredentialsException();
                    }

                    UserPageModel secretMatch = buildUserPageModelFromDB(_session.User);
                    if (string.IsNullOrEmpty(secretMatch.TheirSecretMatch)) {
                        return RedirectToAction("GetMatch");
                    }
                    return View("UserPage", secretMatch);
                }
                return View("SignIn", new AuthenticatedUser());
            }
            catch (InvalidCredentialsException) {
                return View("InvalidCredentials");
            }
            catch (UnregisteredUserException) {
                return View("InvalidCredentials");
            }
            catch (Exception) {
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult SignIn(AuthenticatedUser authUser) {
            try {
                //get a new session for this user
                _session = _dataAccessor.GetSession(authUser.Username, authUser.Password);
                if (_session == null) {
                    throw new InvalidCredentialsException();
                }

                //store the cookie
                Response.Cookies.Append("sessionId", _session.SessionId);

                UserPageModel secretMatch = buildUserPageModelFromDB(authUser.Username);
                if (string.IsNullOrEmpty(secretMatch.TheirSecretMatch)) {
                    return RedirectToAction("GetMatch");
                }
                return View("UserPage", secretMatch);
                
            }
            catch (InvalidCredentialsException) {
                return View("InvalidCredentials");
            }
            catch (UnregisteredUserException) {
                return View("InvalidCredentials");
            }
            catch (Exception) {
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult UpdateInterests(UserPageModel match) {
            //verify access
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }
            _dataAccessor.SetUserInterests(match.Name, match.Interests);
            if (!string.IsNullOrEmpty(match.TheirSecretMatch)) {
                match = buildUserPageModelFromDB(match.Name);
                return View("UserPage", match);
            }
            return RedirectToAction("GetMatch");
        }

        [HttpGet]
        public IActionResult LogOut() {
            if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
                _dataAccessor.EndSession(sessionId);
                HttpContext.Response.Cookies.Delete("sessionId");
            }
            return View("SignIn");
        }

        [HttpGet]
        public IActionResult OpenAdminPage() {
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult UpdatePassword(UserPageModel pageModel) {
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }
            //verify the passwords match, then verify the current password, then update
            if (!string.Equals(pageModel.PasswordReset.NewPassword, pageModel.PasswordReset.VerifyPassword, StringComparison.InvariantCulture)) {
                return View("PasswordsNotMatch");
            }
            //verify user
            if (!_dataAccessor.VerifyCredentials(_session.User, pageModel.PasswordReset.CurrentPassword)) {
                return View("InvalidCredentials");
            }
            //update password
            _dataAccessor.UpdateUserPassword(_session.User, pageModel.PasswordReset.NewPassword);
            return RedirectToAction("GetMatch");
        }

        private bool verifySessionCookie() {
            if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {

                _session = _dataAccessor.GetSessionData(sessionId);
                return (_session != null) ;
            }
            return false;
        }

        private UserPageModel buildUserPageModelFromDB(string username) {
            Match existingMatch = _dataAccessor.GetExistingMatch(username);
            string myInterests = _dataAccessor.GetUserInterests(username);
            string theirInterests = null;
            bool allowReroll = true;
            string theirMatch = null;
            if (existingMatch != null) {
                theirMatch = existingMatch.MatchedName;
                theirInterests = _dataAccessor.GetUserInterests(theirMatch);
                allowReroll = existingMatch.RerollAllowed;
            }
            
            bool isAdmin = _dataAccessor.UserIsAdmin(username);
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);

            return new UserPageModel() {
                Name = username,
                AllowReroll = allowReroll,
                TheirSecretMatch = theirMatch,
                Interests = myInterests,
                MatchInterests = theirInterests,
                UserIsAdmin = isAdmin,
                AllowMatching = allowMatch
            };
        }
    }
}