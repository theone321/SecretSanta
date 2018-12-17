using Microsoft.AspNetCore.Mvc;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Matching;
using SecretSanta.Models;
using SecretSanta.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Controllers {
    public class AdminController : Controller {
        private readonly IDataAccessor _dataAccessor;
        private readonly ICreateSecretMatch _createSecretMatch;
        private readonly ISessionManager _sessionManager;

        public AdminController(IDataAccessor dataAccessor, ICreateSecretMatch createSecretMatch, ISessionManager sessionManager) {
            _dataAccessor = dataAccessor;
            _createSecretMatch = createSecretMatch;
            _sessionManager = sessionManager;
        }

        [HttpGet]
        public IActionResult Index() {
            if (!VerifyAccess(out var session)) {
                return RedirectToAction("LogIn", "User");
            }

            bool.TryParse(_dataAccessor.GetSettingValue("AllowRegistration"), out bool allowRegistration);
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatching);

            List<UserAdminSettings> displayList = new List<UserAdminSettings>();
            IList<User> users = _dataAccessor.GetAllUsers();
            IList<Match> matches = _dataAccessor.GetAllExistingMatches();
            User currentUser = null;
            foreach (User user in users) {
                UserAdminSettings display = new UserAdminSettings {
                    UserId = user.Id,
                    Name = user.RegisteredName,
                    UserName = user.UserName,
                    HasMatched = matches.Any(m => m.RequestorId == user.Id),
                    IsMatched = matches.Any(m => m.MatchedId == user.Id),
                    IsAdmin = user.IsAdmin
                };

                displayList.Add(display);

                if (string.Equals(user.UserName, session.User, StringComparison.Ordinal)) {
                    currentUser = user;
                }
            }

            AdminModel options = new AdminModel {
                User = session.User,
                UserId = currentUser?.Id ?? 0,
                AllowRegistration = allowRegistration,
                AllowMatching = allowMatching,
                UserList = displayList
            };

            return View(options);
        }

        [HttpPost]
        public IActionResult UpdateSettings(AdminModel options) {
            if (!VerifyAccess(out var session)) {
                return RedirectToAction("SignIn", "User");
            }

            _dataAccessor.SetSettingValue(nameof(options.AllowRegistration), options.AllowRegistration.ToString());
            _dataAccessor.SetSettingValue(nameof(options.AllowMatching), options.AllowMatching.ToString());

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleAdminAccess(int userId) {
            if (!VerifyAccess(out var session)) {
                return RedirectToAction("SignIn", "User");
            }
            User user = _dataAccessor.GetUserById(userId);
            _dataAccessor.SetUserAdmin(userId, !user.IsAdmin);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ResetUserPassword(int userId) {
            if (!VerifyAccess(out var session)) {
                return RedirectToAction("SignIn", "User");
            }
            _dataAccessor.UpdateUserPassword(userId, "password");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeRegisterUser(int userId) {
            if (!VerifyAccess(out var session)) {
                return RedirectToAction("SignIn", "User");
            }
            _dataAccessor.DeRegisterAccount(userId);
            return RedirectToAction("Index");
        }

        private bool VerifyAccess(out ISession session) {
            var sessionExists = _sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out session);
            if (sessionExists) {
                var user = _dataAccessor.GetUserByUserName(session.User);
                return _dataAccessor.UserIsAdmin(user.Id);
            }
            return false;
        }
    }
}