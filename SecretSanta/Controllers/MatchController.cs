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
            List<string> registered = _dataAccessor.GetAllUsers().Select(n => n.RegisteredName).ToList();
            int matchCount = _dataAccessor.GetAllExistingMatches().Count;
            return View("Index", new IndexModel() { RegisteredNames = registered, MatchCounts = matchCount });
        }

        [HttpGet]
        public IActionResult GetMatch() {
            //verify access
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }
            UserPageModel userModel = buildUserPageModelFromDB(_session.User);
            return View("UserPage", userModel);
        }
        
        public IActionResult CreateMatch(UserPageModel userModel) {
            //verify access
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);
            userModel.AllowMatching = allowMatch;
            if (userModel.UserId <= 0 || !userModel.AllowMatching) {
                //How? Why? Just start over
                return RedirectToAction("SignIn");
            }

            userModel.TheirSecretMatchId = _createSecretMatch.FindRandomMatch(userModel.UserId);
            userModel.TheirSecretMatchName = _dataAccessor.GetUserById(userModel.TheirSecretMatchId).RegisteredName;
            _dataAccessor.CreateMatch(userModel.UserId, userModel.TheirSecretMatchId, userModel.AllowReroll);

            userModel = buildUserPageModelFromDB(userModel.UserId);

            return View("UserPage", userModel);
        }

        public IActionResult RerollResult(UserPageModel userModel) {
            //verify access
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);
            userModel.AllowMatching = allowMatch;
            if (userModel.UserId <= 0 || !userModel.AllowMatching) { 
                //How? Why? Just start over
                return RedirectToAction("SignIn");
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
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }

            if (userModel.SignificantOther?.UserId > 0) { 
                _dataAccessor.CreateRestriction(userModel.UserId, userModel.SignificantOther.UserId, true, false);
            }

            userModel = buildUserPageModelFromDB(userModel.UserId);

            return View("UserPage", userModel);
        }

        [HttpGet]
        public IActionResult Register() {
            bool.TryParse(_dataAccessor.GetSettingValue("AllowRegistration"), out bool allowRegister);
            return View("Register", new RegisterUser { AllowRegister=allowRegister });
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
            if (_dataAccessor.AccountAlreadyRegistered(registration.UserNameToRegister)) {
                return View("AlreadyRegistered", registration);
            }

            int id = _dataAccessor.RegisterAccount(registration.UserNameToRegister, registration.ChosenPassword);
            _dataAccessor.SetUserRealName(id, registration.RealName);
            //get a new session for this user
            DataAccess.Models.ISession session = _dataAccessor.GetSession(registration.UserNameToRegister, registration.ChosenPassword);
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
                    if (secretMatch.TheirSecretMatchId <= 0) {
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
                if (secretMatch.TheirSecretMatchId <= 0) {
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
        public IActionResult UpdateInterests(UserPageModel user) {
            //verify access
            if (!verifySessionCookie()) {
                return View("InvalidCredentials");
            }
            _dataAccessor.SetUserInterests(user.UserId, user.Interests);
            if (user.TheirSecretMatchId > 0) {
                user = buildUserPageModelFromDB(user.UserId);
                return View("UserPage", user);
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
            User user = _dataAccessor.GetUserByUserName(_session.User);
            _dataAccessor.UpdateUserPassword(user.Id, pageModel.PasswordReset.NewPassword);
            return RedirectToAction("GetMatch");
        }

        private bool verifySessionCookie() {
            if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {

                _session = _dataAccessor.GetSessionData(sessionId);
                return (_session != null) ;
            }
            return false;
        }

        private UserPageModel buildUserPageModelFromDB(string userName) {
            User user = _dataAccessor.GetUserByUserName(userName);
            return buildUserPageModelFromDB(user);
        }

        private UserPageModel buildUserPageModelFromDB(int userId) {
            User user = _dataAccessor.GetUserById(userId);
            return buildUserPageModelFromDB(user);
        }

        private UserPageModel buildUserPageModelFromDB(User user) {
            Match existingMatch = _dataAccessor.GetExistingMatch(user.Id);
            string myInterests = user.Interests;
            bool allowReroll = true;
            User theirMatch = null;
            if (existingMatch != null) {
                theirMatch = _dataAccessor.GetUserById(existingMatch.MatchedId);
                allowReroll = existingMatch.RerollAllowed;
            }
            
            bool isAdmin = _dataAccessor.UserIsAdmin(user.Id);
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);

            MatchRestriction restriction = _dataAccessor.GetMatchRestrictions(user.Id).FirstOrDefault(r => r.StrictRestriction);
            List<UserPageModel.LimitedUser> others = new List<UserPageModel.LimitedUser>();
            UserPageModel.LimitedUser sigOther = new UserPageModel.LimitedUser();
            if (restriction != null) {
                User sigOtherUser = _dataAccessor.GetUserById(restriction.RestrictedId);
                if (sigOtherUser != null) {  
                    sigOther = new UserPageModel.LimitedUser() { UserId = sigOtherUser.Id, UserRealName = sigOtherUser.RegisteredName };
                }
            }
            if (sigOther.UserId <= 0) {
                //we only need this list if their significant other isn't set
                foreach (User other in _dataAccessor.GetAllUsers())
                {
                    if (other.Id == user.Id) { continue; }
                    others.Add(new UserPageModel.LimitedUser() { UserId = other.Id, UserRealName = other.RegisteredName });
                }
            }


            return new UserPageModel() {
                UserId = user.Id,
                UserName = user.UserName,
                Name = user.RegisteredName,
                AllowReroll = allowReroll,
                TheirSecretMatchId = theirMatch?.Id ?? -1,
                TheirSecretMatchName = theirMatch?.RegisteredName,
                Interests = myInterests,
                MatchInterests = theirMatch?.Interests,
                UserIsAdmin = isAdmin,
                AllowMatching = allowMatch,
                SignificantOther = sigOther,
                OtherUsers = others
            };
        }
    }
}