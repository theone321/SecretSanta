using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.DataAccess;
using System.Linq;
using SecretSanta.Matching;
using System;
using SecretSanta.Exceptions;

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
            return View("GetMatch", secretMatch);
        }
        
        public IActionResult CreateMatch(SecretMatch secretMatch) {
            var previousMatch = secretMatch.TheirSecretMatch;
            if (previousMatch != null) {
                _dataAccessor.RemoveMatch(secretMatch.Name, previousMatch);
            }
            secretMatch.TheirSecretMatch = _createSecretMatch.FindRandomMatch(secretMatch.Name, previousMatch);

            _dataAccessor.CreateMatch(secretMatch.Name, secretMatch.TheirSecretMatch, secretMatch.AllowReroll);

            return View("GetMatch", secretMatch);
        }

        [HttpGet]
        public IActionResult RerollResult(string name, string matchName) {
            var secretMatch = new SecretMatch {
                Name = name,
                TheirSecretMatch = matchName,
                AllowReroll = false
            };
            return RedirectToAction("CreateMatch", secretMatch);
        }

        [HttpGet]
        public IActionResult Register() {
            var possibleNames = _dataAccessor.GetAllPossibleNames();
            return View("Register", new RegisterUser { PossibleNames = possibleNames });
        }

        [HttpPost]
        public IActionResult Register(RegisterUser registration) {
            if (_dataAccessor.AccountAlreadyRegistered(registration.NameToRegister)) {
                return View("AlreadyRegistered", registration);
            }
            _dataAccessor.RegisterAccount(registration.NameToRegister, registration.ChosenPassword);
            return RedirectToAction("GetMatch", new SecretMatch { Name = registration.NameToRegister, AllowReroll = true });
        }

        [HttpGet]
        public IActionResult SignIn() {
            return View("SignIn", new AuthenticatedUser());
        }

        [HttpPost]
        public IActionResult SignIn(AuthenticatedUser authUser) {
            try {
                if (!_dataAccessor.VerifyCredentials(authUser.Username, authUser.Password)) {
                    throw new InvalidCredentialsException();
                }
                var existingMatch = _dataAccessor.GetExistingMatch(authUser.Username);
                if (existingMatch == null) {
                    return RedirectToAction("GetMatch", new SecretMatch { Name = authUser.Username, AllowReroll = true });
                }
                return View("ExistingMatch", new SecretMatch { Name = authUser.Username, AllowReroll = existingMatch.RerollAllowed, TheirSecretMatch = existingMatch.MatchedName });
            }
            catch (InvalidCredentialsException) {
                return View("InvalidCredentials");
            }
            catch (Exception) {
                return View("Error");
            }
        }
    }
}