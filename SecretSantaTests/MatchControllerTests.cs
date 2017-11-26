using NUnit.Framework;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
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
        public void GetMatch_Returns_To_GetMatch_View() {
            var secretMatch = new SecretMatch {
                AllowReroll = true,
                Name = "Test Person",
                TheirSecretMatch = null
            };

            var result = _controller.GetMatch(secretMatch);

            Assert.NotNull(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual("GetMatch", viewResult.ViewName);
        }

        [Test]
        public void CreateMatch_Unrestricted_Creates_Match_And_Returns_GetMatch_View() {
            var secretMatch = new SecretMatch {
                AllowReroll = true,
                Name = "Test Person1",
                TheirSecretMatch = null
            };

            _dataAccessor.Setup(d => d.GetMatchRestrictions("Test Person1")).Returns(new List<MatchRestriction>()).Verifiable();
            _createSecretMatch.Setup(c => c.FindRandomMatch("Test Person1")).Returns("Test Person2").Verifiable();
            _dataAccessor.Setup(d => d.CreateMatch("Test Person1", "Test Person2", true)).Verifiable();

            var result = _controller.CreateMatch(secretMatch);

            Assert.NotNull(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual("GetMatch", viewResult.ViewName);
            var model = viewResult.Model as SecretMatch;
            Assert.NotNull(model);
            Assert.AreEqual("Test Person2", model.TheirSecretMatch);

            _dataAccessor.Verify(d => d.GetMatchRestrictions("Test Person1"), Times.Once);
            _dataAccessor.Verify(d => d.CreateMatch("Test Person1", "Test Person2", true), Times.Once);
            _createSecretMatch.Verify(c => c.FindRandomMatch("Test Person1"), Times.Once);
        }

        [Test]
        public void CreateMatch_Restricted_Searches_For_Different_Match_And_Creates_Match() {
            var secretMatch = new SecretMatch {
                AllowReroll = true,
                Name = "Test Person1",
                TheirSecretMatch = null
            };

            _dataAccessor.Setup(d => d.GetMatchRestrictions("Test Person1")).Returns(new List<MatchRestriction> {
                new MatchRestriction {
                    Id = 1,
                    RequestorName = "Test Person1",
                    RestrictedName = "Test Person2"
                }
            }).Verifiable();
            _createSecretMatch.SetupSequence(c => c.FindRandomMatch("Test Person1")).Returns("Test Person2").Returns("Test Person3");
            _dataAccessor.Setup(d => d.CreateMatch("Test Person1", "Test Person3", true)).Verifiable();

            var result = _controller.CreateMatch(secretMatch);

            Assert.NotNull(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual("GetMatch", viewResult.ViewName);
            var model = viewResult.Model as SecretMatch;
            Assert.NotNull(model);
            Assert.AreEqual("Test Person3", model.TheirSecretMatch);

            _dataAccessor.Verify(d => d.GetMatchRestrictions("Test Person1"), Times.Once);
            _dataAccessor.Verify(d => d.CreateMatch("Test Person1", "Test Person3", true), Times.Once);
            _dataAccessor.Verify(d => d.CreateMatch("Test Person1", "Test Person2", true), Times.Never);
            _createSecretMatch.Verify(c => c.FindRandomMatch("Test Person1"), Times.Exactly(2));
        }

        [Test]
        public void Reroll_Result_Redirects_To_Create_Match_With_Allow_Reroll_False() {
            var secretMatch = new SecretMatch {
                AllowReroll = true,
                Name = "Test Person1",
                TheirSecretMatch = "Test Person2"
            };

            var result = _controller.RerollResult(secretMatch);

            Assert.NotNull(result);
            var viewResult = result as RedirectToActionResult;
            Assert.NotNull(viewResult);
            Assert.IsFalse((bool)viewResult.RouteValues["AllowReroll"]);
            Assert.AreEqual("Test Person1", viewResult.RouteValues["Name"]);
            Assert.AreEqual("Test Person2", viewResult.RouteValues["TheirSecretMatch"]);
        }
    }
}
