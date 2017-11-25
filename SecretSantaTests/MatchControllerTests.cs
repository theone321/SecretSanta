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
        public void Reroll_Result_Redirects_To_Create_Match_With_Allow_Reroll_False() {
            var secretMatch = new SecretMatch {
                AllowReroll = true,
                Name = "Steve Rakar",
                TheirSecretMatch = "Sarah Leahman"
            };

            var result = _controller.RerollResult(secretMatch);

            Assert.NotNull(result);
            var viewResult = result as RedirectToActionResult;
            Assert.NotNull(viewResult);
            Assert.IsFalse((bool)viewResult.RouteValues["AllowReroll"]);
            Assert.AreEqual("Steve Rakar", viewResult.RouteValues["Name"]);
            Assert.AreEqual("Sarah Leahman", viewResult.RouteValues["TheirSecretMatch"]);
        }
    }
}
