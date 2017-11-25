using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.DataAccess;
using System.Linq;
using SecretSanta.Matching;

namespace SecretSanta.Controllers {
    public class MatchController : Controller {
        private IDataAccessor _dataAccessor;
        private ICreateSecretMatch _createSecretMatch;

        public MatchController(IDataAccessor dataAccessor, ICreateSecretMatch createSecretMatch) {
            _dataAccessor = dataAccessor;
            _createSecretMatch = createSecretMatch;
        }

        [HttpGet]
        public IActionResult GetMatch() {
            var registeredNames = _dataAccessor.GetAllRegisteredNames();
            return View("GetMatch", new SecretMatch { AllowReroll = true, RegisteredNames = registeredNames });
        }

        [HttpPost]
        public IActionResult CreateMatch(SecretMatch secretMatch) {
            // Probably a better way to keep this property value between each action, but it's 3am and I'm tired...
            secretMatch.RegisteredNames = _dataAccessor.GetAllRegisteredNames();
            var existingMatch = _dataAccessor.GetExistingMatch(secretMatch.Name);
            if (existingMatch != null) {
                secretMatch.TheirSecretMatch = existingMatch.MatchedName;
                secretMatch.AllowReroll = false;
                return View("GetMatch", secretMatch);
            }

            secretMatch.TheirSecretMatch = _createSecretMatch.FindRandomMatch(secretMatch.Name);

            var restrictions = _dataAccessor.GetMatchRestrictions(secretMatch.Name);
            while (restrictions.Any(r => r.RequestorName == secretMatch.Name && r.RestrictedName == secretMatch.TheirSecretMatch)) {
                secretMatch.TheirSecretMatch = _createSecretMatch.FindRandomMatch(secretMatch.Name);
            }

            _dataAccessor.CreateMatch(secretMatch.Name, secretMatch.TheirSecretMatch);

            return View("GetMatch", secretMatch);
        }

        [HttpPost]
        public IActionResult RerollResult(SecretMatch secretMatch) {
            secretMatch.AllowReroll = false;
            return RedirectToAction("CreateMatch", secretMatch);
        }
    }
}