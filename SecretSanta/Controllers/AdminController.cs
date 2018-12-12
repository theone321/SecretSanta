using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.DataAccess;
using SecretSanta.Matching;
using SecretSanta.Models;
using SecretSanta.DataAccess.Models;

namespace SecretSanta.Controllers {
    public class AdminController : Controller {
        private IDataAccessor _dataAccessor;
        private ICreateSecretMatch _createSecretMatch;
        private ISession _session;

        public AdminController(IDataAccessor dataAccessor, ICreateSecretMatch createSecretMatch) {
            _dataAccessor = dataAccessor;
            _createSecretMatch = createSecretMatch;
        }

        [HttpGet]
        public IActionResult Index() {
            if (!verifyAccess()) {
                return RedirectToAction("LogIn", "Match");
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

                if (string.Equals(user.UserName, _session.User, StringComparison.Ordinal)) {
                    currentUser = user;
                }
            }

            AdminModel options = new AdminModel {
                User = _session.User,
                UserId = currentUser?.Id ?? 0,
                AllowRegistration = allowRegistration,
                AllowMatching = allowMatching,
                UserList = displayList
            };

            return View(options);
        }

        [HttpPost]
        public IActionResult UpdateSettings(AdminModel options) {
            if (!verifyAccess()) {
                return RedirectToAction("SignIn", "Match");
            }

            _dataAccessor.SetSettingValue(nameof(options.AllowRegistration), options.AllowRegistration.ToString());
            _dataAccessor.SetSettingValue(nameof(options.AllowMatching), options.AllowMatching.ToString());

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleAdminAccess(int userId)
        {
            if (!verifyAccess()) {
                return RedirectToAction("SignIn", "Match");
            }
            User user = _dataAccessor.GetUserById(userId);
            _dataAccessor.SetUserAdmin(userId, !user.IsAdmin);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ResetUserPassword(int userId) {
            if (!verifyAccess()) {
                return RedirectToAction("SignIn", "Match");
            }
            _dataAccessor.UpdateUserPassword(userId, "password");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeRegisterUser(int userId) {
            if (!verifyAccess()) {
                return RedirectToAction("SignIn", "Match");
            }
            _dataAccessor.DeRegisterAccount(userId);
            return RedirectToAction("Index");
        }

        private bool verifyAccess() {
            if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
                
                //verify user has session
                if ((_session = _dataAccessor.GetSessionData(sessionId)) != null) {
                    //verify that user is admin
                    User user = _dataAccessor.GetUserByUserName(_session.User);
                    return _dataAccessor.UserIsAdmin(user.Id);
                }
            }
            return false;
        }
    }
}