using Microsoft.AspNetCore.Mvc;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Exceptions;
using SecretSanta.Models;
using SecretSanta.Users;
using System;

namespace SecretSanta.Controllers {
    public class UserController : BaseController {
        private readonly IPageModelBuilder _pageModelBuilder;

        public UserController(IDataAccessor dataAccessor, ISessionManager sessionManager, IPageModelBuilder pageModelBuilder)
            : base(sessionManager, dataAccessor) {
            _pageModelBuilder = pageModelBuilder;
        }

        [HttpGet]
        public IActionResult Register() {
            bool.TryParse(_dataAccessor.GetSettingValue("AllowRegistration"), out bool allowRegister);
            return View("Register", new RegisterUser { AllowRegister = allowRegister });
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
            ISession session = _dataAccessor.GetSession(registration.UserNameToRegister, registration.ChosenPassword);
            if (session == null) {
                throw new InvalidCredentialsException();
            }

            //store the cookie
            Response.Cookies.Append("sessionId", session.SessionId);

            return RedirectToAction("GetMatch", "Match");
        }

        [HttpGet]
        public IActionResult SignIn() {
            try {
                if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
                    var session = _dataAccessor.GetSessionData(sessionId);
                    if (session == null) {
                        //remove the associated cookie
                        Response.Cookies.Delete("sessionId");
                        throw new InvalidCredentialsException();
                    }
                    
                    return RedirectToAction("GetMatch", "Match");
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
                var session = _dataAccessor.GetSession(authUser.Username, authUser.Password);
                if (session == null) {
                    throw new InvalidCredentialsException();
                }

                //store the cookie
                Response.Cookies.Append("sessionId", session.SessionId);
                return RedirectToAction("GetMatch", "Match");
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
            if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
                return View("InvalidCredentials");
            }
            _dataAccessor.SetUserInterests(user.UserId, user.Interests);
            if (user.TheirSecretMatchId > 0) {
                user = _pageModelBuilder.BuildUserPageModelFromDB(user.UserId);
                return View("UserPage", user);
            }
            return RedirectToAction("GetMatch", "Match");
        }

        [HttpGet]
        public IActionResult SignOut() {
            if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
                _dataAccessor.EndSession(sessionId);
                HttpContext.Response.Cookies.Delete("sessionId");
                _sessionManager.EndSession();
            }
            return View("SignIn");
        }

        [HttpGet]
        public IActionResult OpenAdminPage() {
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public IActionResult UpdatePassword() {
            if (!_sessionManager.TryGetSessionCookie(Request.Cookies, out var session)) {
                return View("InvalidCredentials");
            }
            var user = _pageModelBuilder.BuildUserPageModelFromDB(session.User);
            return View("UpdatePassword", user);
        }

        [HttpPost]
        public IActionResult UpdatePassword(UserPageModel pageModel) {
            if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
                return View("InvalidCredentials");
            }

            //verify the passwords match, then verify the current password, then update
            if (!string.Equals(pageModel.PasswordReset.NewPassword, pageModel.PasswordReset.VerifyPassword, StringComparison.InvariantCulture)) {
                return View("PasswordsNotMatch");
            }

            //verify user
            if (!_dataAccessor.VerifyCredentials(session.User, pageModel.PasswordReset.CurrentPassword)) {
                return View("InvalidCredentials");
            }

            //update password
            User user = _dataAccessor.GetUserByUserName(session.User);
            _dataAccessor.UpdateUserPassword(user.Id, pageModel.PasswordReset.NewPassword);
            return RedirectToAction("GetMatch", "Match");
        }
    }
}