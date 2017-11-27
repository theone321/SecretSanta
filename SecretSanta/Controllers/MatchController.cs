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
        public IActionResult GetMatch() {
            //verify access
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }
            SecretMatch secretMatch = buildSecretMatchFromDB(_session.User);
            return View("GetMatch", secretMatch);
        }
        
        public IActionResult CreateMatch(SecretMatch secretMatch) {
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

            secretMatch = buildSecretMatchFromDB(secretMatch.Name);

            return View("GetMatch", secretMatch);
        }

        public IActionResult RerollResult(SecretMatch secretMatch) {
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
        public IActionResult SignIn()
        {
            try {
                if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
                    _session = _dataAccessor.GetSessionData(sessionId);
                    if (_session == null) {
                        //remove the associated cookie
                        Response.Cookies.Delete("sessionId");
                        throw new InvalidCredentialsException();
                    }

                    SecretMatch secretMatch = buildSecretMatchFromDB(_session.User);
                    if (string.IsNullOrEmpty(secretMatch.TheirSecretMatch)) {
                        return RedirectToAction("GetMatch");
                    }
                    return View("GetMatch", secretMatch);
                }
                return View("SignIn", new AuthenticatedUser());
            }
            catch (InvalidCredentialsException) {
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

                SecretMatch secretMatch = buildSecretMatchFromDB(authUser.Username);
                if (string.IsNullOrEmpty(secretMatch.TheirSecretMatch)) {
                    return RedirectToAction("GetMatch");
                }
                return View("GetMatch", secretMatch);
                
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
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }
            _dataAccessor.SetUserInterests(match.Name, match.Interests);
            if (!string.IsNullOrEmpty(match.TheirSecretMatch)) {
                match = buildSecretMatchFromDB(match.Name);
                return View("GetMatch", match);
            }
            return RedirectToAction("GetMatch");
        }

        [HttpGet]
        public IActionResult LogOut()
        {
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

        private bool verifySessionCookie() {
            if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {

                _session = _dataAccessor.GetSessionData(sessionId);
                return (_session != null) ;
            }
            return false;
        }

        private SecretMatch buildSecretMatchFromDB(string username)
        {
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

            return new SecretMatch() {
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