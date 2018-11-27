using Moq;
using NUnit.Framework;
using SecretSanta.DataAccess;
using SecretSanta.DependencyWrappers;
using SecretSanta.Matching;
using System;
using System.Collections.Generic;
using SecretSanta.DataAccess.Models;
using System.Text;

namespace SecretSantaTests {
    /*
    [TestFixture]
    public class CreateSecretMatchTests {
        private Mock<IDataAccessor> _dataAccessor;
        private Mock<IRandomWrapper> _randomWrapper;
        private CreateSecretMatch _createSecretMatch;

        [SetUp]
        public void Setup() {
            _dataAccessor = new Mock<IDataAccessor>();
            _randomWrapper = new Mock<IRandomWrapper>();
            _createSecretMatch = new CreateSecretMatch(_dataAccessor.Object, _randomWrapper.Object);
        }

        [Test]
        public void Name_Of_Requestor_Cannot_Be_Matched() {
            var possibleNames = new List<User> {
                new User {
                    Id = 1,
                    RegisteredName = "Test User1",
                    HasRegistered = true
                },
                new User {
                    Id = 2,
                    RegisteredName = "Test User2",
                    HasRegistered = true
                }
            };

            _dataAccessor.Setup(d => d.GetAllRegisteredUsers()).Returns(possibleNames);
            _dataAccessor.Setup(d => d.GetMatchRestrictions("Test User1")).Returns(new List<MatchRestriction>());
            _dataAccessor.Setup(d => d.GetAllExistingMatches()).Returns(new List<SecretSanta.DataAccess.Models.Match>());
            _randomWrapper.Setup(r => r.Next(It.IsAny<int>())).Returns(0);

            var result = _createSecretMatch.FindRandomMatch("Test User1");

            Assert.AreEqual("Test User2", result);
        }

        [Test]
        public void Restricted_Matches_Cannot_Be_Matched() {
            var possibleNames = new List<User> {
                new User {
                    Id = 1,
                    RegisteredName = "Test User1",
                    HasRegistered = true
                },
                new User {
                    Id = 2,
                    RegisteredName = "Test User2",
                    HasRegistered = true
                },
                new User {
                    Id = 3,
                    RegisteredName = "Test User3",
                    HasRegistered = true
                }
            };

            _dataAccessor.Setup(d => d.GetAllRegisteredUsers()).Returns(possibleNames);
            _dataAccessor.Setup(d => d.GetMatchRestrictions("Test User1")).Returns(new List<MatchRestriction> {
                new MatchRestriction {
                    Id = 1,
                    RequestorName = "Test User1",
                    RestrictedName = "Test User2",
                    StrictRestriction = true
                }
            });
            _dataAccessor.Setup(d => d.GetAllExistingMatches()).Returns(new List<SecretSanta.DataAccess.Models.Match>());
            _randomWrapper.Setup(r => r.Next(It.IsAny<int>())).Returns(0);

            var result = _createSecretMatch.FindRandomMatch("Test User1");

            Assert.AreEqual("Test User3", result);
        }

        [Test]
        public void Users_That_Have_Been_Matched_To_Already_Cannot_Be_Matched() {
            var possibleNames = new List<User> {
                new User {
                    Id = 1,
                    RegisteredName = "Test User1",
                    HasRegistered = true
                },
                new User {
                    Id = 2,
                    RegisteredName = "Test User2",
                    HasRegistered = true
                },
                new User {
                    Id = 3,
                    RegisteredName = "Test User3",
                    HasRegistered = true
                },
                new User {
                    Id = 4,
                    RegisteredName = "Test User4",
                    HasRegistered = true
                }
            };

            _dataAccessor.Setup(d => d.GetAllRegisteredUsers()).Returns(possibleNames);
            _dataAccessor.Setup(d => d.GetMatchRestrictions("Test User1")).Returns(new List<MatchRestriction>());
            _dataAccessor.Setup(d => d.GetAllExistingMatches()).Returns(new List<SecretSanta.DataAccess.Models.Match> {
                new SecretSanta.DataAccess.Models.Match {
                    Id = 1,
                    RequestorName = "Test User2",
                    MatchedName = "Test User3"
                },
                new SecretSanta.DataAccess.Models.Match {
                    Id = 1,
                    RequestorName = "Test User4",
                    MatchedName = "Test User2"
                }
            });
            _randomWrapper.Setup(r => r.Next(It.IsAny<int>())).Returns(0);

            var result = _createSecretMatch.FindRandomMatch("Test User1");

            Assert.AreEqual("Test User4", result);
        }
    }*/
}
