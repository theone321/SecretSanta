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

            List<UserAdminSettings> users = new List<UserAdminSettings>();
            var names = _dataAccessor.GetAllPossibleNames();
            var matches = _dataAccessor.GetAllExistingMatches();
            foreach (Name name in names) {
                UserAdminSettings user = new UserAdminSettings();
                user.Name = name.RegisteredName;
                user.HasRegistered = name.HasRegistered;
                user.HasMatched = matches.Any(m => string.Equals(m.RequestorName, name.RegisteredName, StringComparison.InvariantCultureIgnoreCase));
                user.IsMatched = matches.Any(m => string.Equals(m.MatchedName, name.RegisteredName, StringComparison.InvariantCultureIgnoreCase));

                users.Add(user);
            }
            
            AdminModel options = new AdminModel {
                User = _session.User,
                AllowRegistration = allowRegistration,
                AllowMatching = allowMatching,
                UserList = users.OrderByDescending(u => u.HasRegistered).ToList()
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
        public IActionResult ResetUserPassword(string username) {
            if (!verifyAccess()) {
                return RedirectToAction("SignIn", "Match");
            }
            _dataAccessor.UpdateUserPassword(username, "password");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeRegisterUser(string username) {
            if (!verifyAccess()) {
                return RedirectToAction("SignIn", "Match");
            }
            _dataAccessor.DeRegisterAccount(username);
            return RedirectToAction("Index");
        }

        private bool verifyAccess() {
            if (HttpContext.Request.Cookies.TryGetValue("sessionId", out string sessionId)) {
                
                //verify user has session
                if ((_session = _dataAccessor.GetSessionData(sessionId)) != null) {
                    //verify that user is admin
                    return _dataAccessor.UserIsAdmin(_session.User);
                }
            }
            return false;
        }
    }
}