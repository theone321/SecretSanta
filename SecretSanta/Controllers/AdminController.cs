using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.DataAccess;
using SecretSanta.Matching;
using SecretSanta.Models;
using SecretSanta.DataAccess.Models;

namespace SecretSanta.Controllers
{
    public class AdminController : Controller
    {
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

            AdminOptions options = new AdminOptions {
                User = _session.User,
                AllowRegistration = allowRegistration,
                AllowMatching = allowMatching
            };

            return View(options);
        }

        [HttpPost]
        public IActionResult UpdateSettings(AdminOptions options) {
            if (!verifyAccess()) {
                return RedirectToAction("SignIn", "Match");
            }

            _dataAccessor.SetSettingValue(nameof(options.AllowRegistration), options.AllowRegistration.ToString());
            _dataAccessor.SetSettingValue(nameof(options.AllowMatching), options.AllowMatching.ToString());

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