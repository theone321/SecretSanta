using NUnit.Framework;
using SecretSanta.DataAccess;
using SecretSanta.Models;
using SecretSanta.Controllers;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using SecretSanta.Matching;

namespace SecretSantaTests {
    [TestFixture]
    public class MatchControllerTests {
        private MatchController _controller;
        private Mock<IDataAccessor> _dataAccessor;
        private Mock<ICreateSecretMatch> _createSecretMatch;

        [SetUp]
        public void Setup() {
            _dataAccessor = new Mock<IDataAccessor>();
            _createSecretMatch = new Mock<ICreateSecretMatch>();
            _controller = new MatchController(_dataAccessor.Object, _createSecretMatch.Object);
        }

        [Test]
        public void Initial_Load_Of_Match_Page_Has_Reroll_True_And_All_Registered_Names() {
            _dataAccessor.Setup(d => d.GetAllRegisteredNames()).Returns(new List<Name> {
                new Name {
                    Id = 1,
                    RegisteredName = "Tobias Becker"
                },
                new Name {
                    Id = 2,
                    RegisteredName = "Michael Marvin"
                }
            });

            var result = _controller.GetMatch();

            Assert.NotNull(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewResult.Model);
            Assert.AreEqual("GetMatch", viewResult.ViewName);
            var model = viewResult.Model as SecretMatch;
            Assert.IsTrue(model.AllowReroll);
            Assert.NotNull(model.RegisteredNames);
            Assert.AreEqual(2, model.RegisteredNames.Count);
        }

        [Test]
        public void Reroll_Result_Redirects_To_Create_Match_With_Allow_Reroll_False() {
            var secretMatch = new SecretMatch {
                AllowReroll = true,
                Name = "Steve Rakar",
                TheirSecretMatch = "Sarah Leahman",
                RegisteredNames = new List<Name>()
            };

            var result = _controller.RerollResult(secretMatch);

            Assert.NotNull(result);
            var viewResult = result as RedirectToActionResult;
            Assert.NotNull(viewResult);
            Assert.IsFalse((bool)viewResult.RouteValues["AllowReroll"]);
            Assert.AreEqual("Steve Rakar", viewResult.RouteValues["Name"]);
            Assert.AreEqual("Sarah Leahman", viewResult.RouteValues["TheirSecretMatch"]);
            Assert.AreEqual(0, ((List<Name>)viewResult.RouteValues["RegisteredNames"]).Count);
        }

        [Test]
        public void CreateMatch_With_An_Existing_Match_Returns_That_Match() {
            var secretMatch = new SecretMatch {
                AllowReroll = true,
                Name = "Michael Marvin",
                TheirSecretMatch = null,
                RegisteredNames = null
            };

            _dataAccessor.Setup(d => d.GetAllRegisteredNames()).Returns(new List<Name>());

            _dataAccessor.Setup(d => d.GetExistingMatch("Michael Marvin")).Returns(new SecretSanta.DataAccess.Match { Id = 1, RequestorName = "Michael Marvin", MatchedName = "Angelia Becker" });

            var result = _controller.CreateMatch(secretMatch);

            Assert.NotNull(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            var model = viewResult.Model as SecretMatch;
            Assert.NotNull(model);
            Assert.IsFalse(model.AllowReroll);
            Assert.AreEqual("Angelia Becker", model.TheirSecretMatch);
            Assert.AreEqual("Michael Marvin", model.Name);
            Assert.NotNull(model.RegisteredNames);
        }
    }
}
