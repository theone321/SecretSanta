using Moq;
using NUnit.Framework;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.DependencyWrappers;
using SecretSanta.Matching;
using System.Collections.Generic;

namespace SecretSantaTests {

    [TestFixture]
    public class CreateSecretMatchTests {
        private Mock<IDataAccessor> _dataAccessor;
        private Mock<IRandomWrapper> _randomWrapper;
        private CreateSecretMatch _createSecretMatch;
        private List<User> _possibleNames;
        private List<MatchRestriction> _existingRestrictions;
        private List<SecretSanta.DataAccess.Models.Match> _existingMatches;

        [SetUp]
        public void Setup() {
            _dataAccessor = new Mock<IDataAccessor>();
            _randomWrapper = new Mock<IRandomWrapper>();
            _createSecretMatch = new CreateSecretMatch(_dataAccessor.Object, _randomWrapper.Object);
            _possibleNames = new List<User>();
            _existingRestrictions = new List<MatchRestriction>();
            _existingMatches = new List<SecretSanta.DataAccess.Models.Match>();
        }

        [Test]
        public void Name_Of_Requestor_Cannot_Be_Matched() {
            _possibleNames = new List<User> {
                new User {
                    Id = 1,
                    RegisteredName = "Test User1"
                },
                new User {
                    Id = 2,
                    RegisteredName = "Test User2"
                }
            };

            _dataAccessor.Setup(d => d.GetAllUsers()).Returns(_possibleNames);
            _dataAccessor.Setup(d => d.GetMatchRestrictions(1)).Returns(_existingRestrictions);
            _dataAccessor.Setup(d => d.GetAllExistingMatches()).Returns(_existingMatches);
            _randomWrapper.Setup(r => r.Next(It.IsAny<int>())).Returns(0);

            var result = _createSecretMatch.FindRandomMatch(1);

            Assert.AreEqual(2, result);
        }

        [Test]
        public void Restricted_Matches_Cannot_Be_Matched() {
            _possibleNames.Add(new User {
                Id = 1,
                RegisteredName = "Test User1"
            });
            _possibleNames.Add(new User {
                Id = 2,
                RegisteredName = "Test User2"
            });
            _possibleNames.Add(new User {
                Id = 3,
                RegisteredName = "Test User3"
            });

            _existingRestrictions.Add(new MatchRestriction {
                Id = 1,
                RequestorId = 1,
                RestrictedId = 2,
                StrictRestriction = true
            });

            _dataAccessor.Setup(d => d.GetAllUsers()).Returns(_possibleNames);
            _dataAccessor.Setup(d => d.GetMatchRestrictions(1)).Returns(_existingRestrictions);
            _dataAccessor.Setup(d => d.GetAllExistingMatches()).Returns(new List<SecretSanta.DataAccess.Models.Match>());
            _randomWrapper.Setup(r => r.Next(It.IsAny<int>())).Returns(0);

            var result = _createSecretMatch.FindRandomMatch(1);

            Assert.AreEqual(3, result);
        }

        [Test]
        public void Users_That_Have_Been_Matched_To_Already_Cannot_Be_Matched() {
            _possibleNames.Add(new User {
                Id = 1,
                RegisteredName = "Test User1"
            });
            _possibleNames.Add(new User {
                Id = 2,
                RegisteredName = "Test User2"
            });
            _possibleNames.Add(new User {
                Id = 3,
                RegisteredName = "Test User3"
            });
            _possibleNames.Add(new User {
                Id = 4,
                RegisteredName = "Test User4"
            });

            _existingMatches.Add(new SecretSanta.DataAccess.Models.Match {
                Id = 1,
                RequestorId = 2,
                MatchedId = 3
            });
            _existingMatches.Add(new SecretSanta.DataAccess.Models.Match {
                Id = 1,
                RequestorId = 4,
                MatchedId = 2
            });

            _dataAccessor.Setup(d => d.GetAllUsers()).Returns(_possibleNames);
            _dataAccessor.Setup(d => d.GetMatchRestrictions(1)).Returns(_existingRestrictions);
            _dataAccessor.Setup(d => d.GetAllExistingMatches()).Returns(_existingMatches);
            _randomWrapper.Setup(r => r.Next(It.IsAny<int>())).Returns(0);

            var result = _createSecretMatch.FindRandomMatch(1);

            Assert.AreEqual(4, result);
        }

        [Test]
        public void Does_Not_Leave_A_Single_Unmatched_User() {
            _possibleNames.Add(new User {
                Id = 1,
                RegisteredName = "Test User1"
            });
            _possibleNames.Add(new User {
                Id = 2,
                RegisteredName = "Test User2"
            });
            _possibleNames.Add(new User {
                Id = 3,
                RegisteredName = "Test User3"
            });

            _existingMatches.Add(new SecretSanta.DataAccess.Models.Match {
                Id = 1,
                RequestorId = 2,
                MatchedId = 1
            });

            _dataAccessor.Setup(d => d.GetAllUsers()).Returns(_possibleNames);
            _dataAccessor.Setup(d => d.GetMatchRestrictions(1)).Returns(_existingRestrictions);
            _dataAccessor.Setup(d => d.GetAllExistingMatches()).Returns(_existingMatches);
            _randomWrapper.Setup(r => r.Next(It.IsAny<int>())).Returns(0);

            var result = _createSecretMatch.FindRandomMatch(1);

            Assert.AreEqual(3, result);
        }

        [Test]
        public void If_Only_Two_People_Left_Will_Allow_Matching_Each_Other() {
            _possibleNames.Add(new User {
                Id = 1,
                RegisteredName = "Test User1"
            });
            _possibleNames.Add(new User {
                Id = 2,
                RegisteredName = "Test User2"
            });

            _existingMatches.Add(new SecretSanta.DataAccess.Models.Match {
                Id = 1,
                RequestorId = 2,
                MatchedId = 1
            });

            _dataAccessor.Setup(d => d.GetAllUsers()).Returns(_possibleNames);
            _dataAccessor.Setup(d => d.GetMatchRestrictions(1)).Returns(_existingRestrictions);
            _dataAccessor.Setup(d => d.GetAllExistingMatches()).Returns(_existingMatches);
            _randomWrapper.Setup(r => r.Next(It.IsAny<int>())).Returns(0);

            var result = _createSecretMatch.FindRandomMatch(1);

            Assert.AreEqual(2, result);
        }
    }
}
