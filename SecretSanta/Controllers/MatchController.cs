using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.DataAccess;
using System.Linq;
using SecretSanta.Matching;
using System;
using SecretSanta.Exceptions;
using SecretSanta.DataAccess.Models;
using System.Collections.Generic;

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
            secretMatch.Interests = _dataAccessor.GetUserInterests(secretMatch.Name);
            if (!string.IsNullOrEmpty(secretMatch.TheirSecretMatch))
            {
                secretMatch.MatchInterests = _dataAccessor.GetUserInterests(secretMatch.TheirSecretMatch);
            }

            return View("GetMatch", secretMatch);
        }
        
        public IActionResult CreateMatch(SecretMatch secretMatch) {
            if (string.IsNullOrEmpty(secretMatch?.Name))
            { //How? Why? Just start over
                return RedirectToAction("SignIn");
            }
            secretMatch.TheirSecretMatch = _createSecretMatch.FindRandomMatch(secretMatch.Name);

            _dataAccessor.CreateMatch(secretMatch.Name, secretMatch.TheirSecretMatch, secretMatch.AllowReroll);

            secretMatch.Interests = _dataAccessor.GetUserInterests(secretMatch.Name);
            secretMatch.MatchInterests = _dataAccessor.GetUserInterests(secretMatch.TheirSecretMatch);

            return View("GetMatch", secretMatch);
        }

        public IActionResult RerollResult(SecretMatch secretMatch) {
            if (string.IsNullOrEmpty(secretMatch?.Name))
            { //How? Why? Just start over
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
            return View("Register", new RegisterUser { PossibleNames = possibleNames });
        }

        [HttpPost]
        public IActionResult Register(RegisterUser registration) {
            if (!string.Equals(registration.ChosenPassword, registration.VerifyPassword, StringComparison.Ordinal))
            {
                return View("PasswordsNotMatch");
            }
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
                Match existingMatch = _dataAccessor.GetExistingMatch(authUser.Username);
                if (existingMatch == null) {
                    return RedirectToAction("GetMatch", new SecretMatch { Name = authUser.Username, AllowReroll = true });
                }
                string myInterests = _dataAccessor.GetUserInterests(authUser.Username);
                string theirInterests = _dataAccessor.GetUserInterests(existingMatch.MatchedName);

                return View("ExistingMatch", new SecretMatch { Name = authUser.Username, AllowReroll = existingMatch.RerollAllowed, TheirSecretMatch = existingMatch.MatchedName, Interests = myInterests, MatchInterests = theirInterests });
            }
            catch (InvalidCredentialsException) {
                return View("InvalidCredentials");
            }
            catch (Exception) {
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult UpdateInterests(SecretMatch match)
        {
            _dataAccessor.SetUserInterests(match.Name, match.Interests);
            if (!string.IsNullOrEmpty(match.TheirSecretMatch))
            {
                return View("ExistingMatch", match);
            }
            return RedirectToAction("GetMatch", match);
        }
    }
}