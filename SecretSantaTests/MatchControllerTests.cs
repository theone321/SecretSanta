using NUnit.Framework;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Models;
using SecretSanta.Controllers;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using SecretSanta.Matching;
using SecretSanta.DependencyWrappers;

namespace SecretSantaTests {
    [TestFixture]
    public class MatchControllerTests {
        private MatchController _controller;
        private Mock<IDataAccessor> _dataAccessor;
        private ICreateSecretMatch _createSecretMatch;

        [SetUp]
        public void Setup() {
            _dataAccessor = new Mock<IDataAccessor>();
            _createSecretMatch = new CreateSecretMatch(_dataAccessor.Object, new RandomWrapper());
            _controller = new MatchController(_dataAccessor.Object, _createSecretMatch);
        }

        //[Test] //Now it just takes in data through the cookie, so you'll have to fake the cookie
        //public void GetMatch_Returns_To_GetMatch_View() {
        //    var secretMatch = new SecretMatch {
        //        AllowReroll = true,
        //        Name = "Test Person",
        //        TheirSecretMatch = null
        //    };

        //    var result = _controller.GetMatch(secretMatch);

        //    Assert.NotNull(result);
        //    var viewResult = result as ViewResult;
        //    Assert.NotNull(viewResult);
        //    Assert.AreEqual("GetMatch", viewResult.ViewName);
        //}

        [Test]
        public void CreateMatch_Unrestricted_Creates_Match_And_Returns_GetMatch_View() {
            var possibleNames = new List<Name> {
                new Name {
                    Id = 1,
                    RegisteredName = "Test Person1",
                    HasRegistered = true
                },
                new Name {
                    Id = 2,
                    RegisteredName = "Test Person2",
                    HasRegistered = true
                }
            };

            var secretMatch = new UserPageModel {
                AllowReroll = true,
                Name = "Test Person1",
                TheirSecretMatch = null
            };

            _dataAccessor.Setup(d => d.GetMatchRestrictions("Test Person1")).Returns(new List<MatchRestriction>()).Verifiable();
            _dataAccessor.Setup(d => d.GetAllRegisteredNames()).Returns(possibleNames).Verifiable();
            _dataAccessor.Setup(d => d.CreateMatch("Test Person1", "Test Person2", true)).Verifiable();

            var result = _controller.CreateMatch(secretMatch);

            Assert.NotNull(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual("GetMatch", viewResult.ViewName);
            var model = viewResult.Model as UserPageModel;
            Assert.NotNull(model);
            Assert.AreEqual("Test Person2", model.TheirSecretMatch);

            _dataAccessor.Verify(d => d.GetMatchRestrictions("Test Person1"), Times.Once); //This doesn't run, because the FindRandomMatch is what checks restrictions
            _dataAccessor.Verify(d => d.CreateMatch("Test Person1", "Test Person2", true), Times.Once);
            _dataAccessor.Verify(d => d.GetAllRegisteredNames(), Times.Once);
        }

        //[Test] //The controller right now doesn't check restrictions, it relies on FindRandomMatch to handle that. I don't know that it should check restrictions itself
        //public void CreateMatch_Restricted_Searches_For_Different_Match_And_Creates_Match() {
        //    var secretMatch = new SecretMatch {
        //        AllowReroll = true,
        //        Name = "Test Person1",
        //        TheirSecretMatch = null
        //    };

        //    _dataAccessor.Setup(d => d.GetMatchRestrictions("Test Person1")).Returns(new List<MatchRestriction> {
        //        new MatchRestriction {
        //            Id = 1,
        //            RequestorName = "Test Person1",
        //            RestrictedName = "Test Person2",
        //            StrictRestriction = true
        //        }
        //    }).Verifiable();
        //    _createSecretMatch.SetupSequence(c => c.FindRandomMatch("Test Person1")).Returns("Test Person2").Returns("Test Person3");
        //    _dataAccessor.Setup(d => d.CreateMatch("Test Person1", "Test Person3", true)).Verifiable();

        //    var result = _controller.CreateMatch(secretMatch);

        //    Assert.NotNull(result);
        //    var viewResult = result as ViewResult;
        //    Assert.NotNull(viewResult);
        //    Assert.AreEqual("GetMatch", viewResult.ViewName);
        //    var model = viewResult.Model as SecretMatch;
        //    Assert.NotNull(model);
        //    Assert.AreEqual("Test Person3", model.TheirSecretMatch);

        //    _dataAccessor.Verify(d => d.GetMatchRestrictions("Test Person1"), Times.Once);
        //    _dataAccessor.Verify(d => d.CreateMatch("Test Person1", "Test Person3", true), Times.Once);
        //    _dataAccessor.Verify(d => d.CreateMatch("Test Person1", "Test Person2", true), Times.Never);
        //    _createSecretMatch.Verify(c => c.FindRandomMatch("Test Person1"), Times.Exactly(2));
        //}

        [Test]
        public void Reroll_Result_Redirects_To_Create_Match_With_Allow_Reroll_False() {
            var possibleNames = new List<Name> {
                new Name {
                    Id = 1,
                    RegisteredName = "Test Person1",
                    HasRegistered = true
                },
                new Name {
                    Id = 2,
                    RegisteredName = "Test Person2",
                    HasRegistered = true
                },
                new Name {
                    Id = 3,
                    RegisteredName = "Test Person3",
                    HasRegistered = true
                }
            };

            var secretMatch = new UserPageModel {
                AllowReroll = true,
                Name = "Test Person1",
                TheirSecretMatch = "Test Person2"
            };

            _dataAccessor.Setup(d => d.GetAllRegisteredNames()).Returns(possibleNames);
            _dataAccessor.Setup(d => d.CreateRestriction("Test Person1", "Test Person2", false, false)).Verifiable();
            _dataAccessor.Setup(d => d.RemoveMatch("Test Person1", "Test Person2")).Verifiable();


            var result = _controller.RerollResult(secretMatch);
            
            Assert.NotNull(result);
            var viewResult = result as RedirectToActionResult;
            Assert.NotNull(viewResult);
            Assert.IsFalse((bool)viewResult.RouteValues["AllowReroll"]);
            Assert.AreEqual("Test Person1", viewResult.RouteValues["Name"]);
            Assert.IsNull(viewResult.RouteValues["TheirSecretMatch"]); //Now we don't pass this, since it's set up as a restriction
            _dataAccessor.Verify(d => d.CreateRestriction("Test Person1", "Test Person2", false, false), Times.Once);
            _dataAccessor.Verify(d => d.RemoveMatch("Test Person1", "Test Person2"), Times.Once);
        }

        [Test]
        public void Initial_SignIn_Returns_SignIn_View_With_New_AuthenticatedUser_Model() {
            var result = _controller.SignIn();

            Assert.NotNull(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            var model = viewResult.Model as AuthenticatedUser;
            Assert.NotNull(model);
        }

        [Test]
        public void SignIn_With_Valid_Credentials_And_No_Existing_Match_Returns_GetMatch_View() {
            _dataAccessor.Setup(d => d.VerifyCredentials("Test User", "TestPass!")).Returns(true);
            _dataAccessor.Setup(d => d.GetExistingMatch("Test User")).Returns((SecretSanta.DataAccess.Models.Match)null);

            var authenticatedUser = new AuthenticatedUser { Username = "Test User", Password = "TestPass!" };

            var result = _controller.SignIn(authenticatedUser);

            Assert.NotNull(result);
            var viewResult = result as RedirectToActionResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual("GetMatch", viewResult.ActionName);
            Assert.AreEqual("Test User", viewResult.RouteValues["Name"]);
            Assert.IsTrue((bool)viewResult.RouteValues["AllowReroll"]);

            _dataAccessor.Verify(d => d.VerifyCredentials("Test User", "TestPass!"), Times.Once);
            _dataAccessor.Verify(d => d.GetExistingMatch("Test User"), Times.Once);
        }

        [Test]
        public void SignIn_With_Valid_Credentials_And_Existing_Match_Returns_ExistingMatch_View() {
            _dataAccessor.Setup(d => d.VerifyCredentials("Test User", "TestPass!")).Returns(true);
            _dataAccessor.Setup(d => d.GetExistingMatch("Test User")).Returns(new SecretSanta.DataAccess.Models.Match { RequestorName = "Test User", Id = 1, MatchedName = "Test User1", RerollAllowed = false });

            var authenticatedUser = new AuthenticatedUser { Username = "Test User", Password = "TestPass!" };

            var result = _controller.SignIn(authenticatedUser);

            Assert.NotNull(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual("ExistingMatch", viewResult.ViewName);
            var model = viewResult.Model as UserPageModel;
            Assert.NotNull(model);
            Assert.AreEqual("Test User", model.Name);
            Assert.AreEqual("Test User1", model.TheirSecretMatch);
            Assert.IsFalse(model.AllowReroll);

            _dataAccessor.Verify(d => d.VerifyCredentials("Test User", "TestPass!"), Times.Once);
            _dataAccessor.Verify(d => d.GetExistingMatch("Test User"), Times.Once);
        }

        [Test]
        public void SignIn_With_Invalid_Credentials_Returns_InvalidCredentials_View() {
            _dataAccessor.Setup(d => d.VerifyCredentials("Test User", "TestPass!")).Returns(false);

            var authenticatedUser = new AuthenticatedUser { Username = "Test User", Password = "TestPass!" };

            var result = _controller.SignIn(authenticatedUser);

            Assert.NotNull(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual("InvalidCredentials", viewResult.ViewName);

            _dataAccessor.Verify(d => d.VerifyCredentials("Test User", "TestPass!"), Times.Once);
        }

        [Test]
        public void Initial_Register_Returns_Register_View_With_Model() {
            var possibleNames = new List<Name> {
                new Name {
                    Id = 1,
                    RegisteredName = "Test User1",
                    HasRegistered = false
                },
                new Name {
                    Id = 2,
                    RegisteredName = "Test User2", 
                    HasRegistered = true
                }
            };
            _dataAccessor.Setup(d => d.GetAllPossibleNames()).Returns(possibleNames);

            var result = _controller.Register();

            Assert.NotNull(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual("Register", viewResult.ViewName);
            var model = viewResult.Model as RegisterUser;
            Assert.NotNull(model);
            Assert.NotNull(model.PossibleNames);
            Assert.AreEqual(2, model.PossibleNames.Count);

            _dataAccessor.Verify(d => d.GetAllPossibleNames(), Times.Once);
        }

        [Test]
        public void Register_With_Already_Registered_Account_Returns_AlreadyRegistered_View() {
            _dataAccessor.Setup(d => d.AccountAlreadyRegistered("Test User")).Returns(true);
            var registerUser = new RegisterUser {
                NameToRegister = "Test User",
                ChosenPassword = "12345!"
            };

            var result = _controller.Register(registerUser);

            Assert.NotNull(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual("AlreadyRegistered", viewResult.ViewName);
            var model = viewResult.Model as RegisterUser;
            Assert.NotNull(model);
            Assert.AreEqual("Test User", model.NameToRegister);
            Assert.AreEqual("12345!", model.ChosenPassword);

            _dataAccessor.Verify(d => d.AccountAlreadyRegistered("Test User"), Times.Once);
            _dataAccessor.Verify(d => d.RegisterAccount("Test User", "12345!"), Times.Never);
        }

        [Test]
        public void Register_New_Account_Registers_Account_And_Redirects_To_GetMatch() {
            _dataAccessor.Setup(d => d.AccountAlreadyRegistered("Test User")).Returns(false);
            _dataAccessor.Setup(d => d.RegisterAccount("Test User", "12345!"));
            var registerUser = new RegisterUser {
                NameToRegister = "Test User",
                ChosenPassword = "12345!"
            };

            var result = _controller.Register(registerUser);

            Assert.NotNull(result);
            var viewResult = result as RedirectToActionResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual("GetMatch", viewResult.ActionName);
            Assert.AreEqual("Test User", viewResult.RouteValues["Name"]);
            Assert.IsTrue((bool)viewResult.RouteValues["AllowReroll"]);

            _dataAccessor.Verify(d => d.AccountAlreadyRegistered("Test User"), Times.Once);
            _dataAccessor.Verify(d => d.RegisterAccount("Test User", "12345!"), Times.Once);
        }
    }
}
