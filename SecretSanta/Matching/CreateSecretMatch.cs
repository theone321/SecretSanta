using SecretSanta.DataAccess;
using SecretSanta.DependencyWrappers;
using System.Collections.Generic;
using System.Linq;
using SecretSanta.DataAccess.Models;
using SecretSanta.Exceptions;

namespace SecretSanta.Matching {
    public interface ICreateSecretMatch {
        int FindRandomMatch(int requestor);
    }

    public class CreateSecretMatch : ICreateSecretMatch {
        private static IRandomWrapper _random;
        private IDataAccessor _dataAccessor;

        public CreateSecretMatch(IDataAccessor dataAccessor, IRandomWrapper randomWrapper) {
            _dataAccessor = dataAccessor;
            _random = randomWrapper;
        }

        public int FindRandomMatch(int requestor) {
            var allUsers = _dataAccessor.GetAllUsers().ToList();
            allUsers.RemoveAll(n => n.Id == requestor);
            var usersThatCannotBeMatched = new List<User>();
            var restrictions = _dataAccessor.GetMatchRestrictions(requestor);
            var existingMatches = _dataAccessor.GetAllExistingMatches();
            foreach (var user in allUsers) {
                if (restrictions?.Any(r => r.RestrictedId == user.Id) == true) {
                    usersThatCannotBeMatched.Add(user);
                    continue;
                }
                //Remove any names that have already been matched to someone else.
                if (existingMatches?.Any(m => m.MatchedId == user.Id) == true) {
                    usersThatCannotBeMatched.Add(user);
                }
            }
            var finalNames = allUsers.Except(usersThatCannotBeMatched).ToList();
            //allow non-strict restrictions through if there is no match
            if (!finalNames.Any()) {
                usersThatCannotBeMatched.Clear();
                foreach (var user in allUsers) {
                    if (restrictions?.Any(r => r.StrictRestriction && r.RestrictedId == user.Id) == true) {
                        usersThatCannotBeMatched.Add(user);
                        continue;
                    }
                    //Remove any names that have already been matched to someone else.
                    if (existingMatches?.Any(m => m.MatchedId == user.Id) == true) {
                        usersThatCannotBeMatched.Add(user);
                    }
                }
                finalNames = allUsers.Except(usersThatCannotBeMatched).ToList();
            }
            //allow even strict restrictions through if there is no match, this is a last resort
            if (!finalNames.Any()) {
                usersThatCannotBeMatched.Clear();
                foreach (var user in allUsers) {
                    //Remove any names that have already been matched to someone else.
                    if (existingMatches?.Any(m => m.MatchedId == user.Id) == true) {
                        usersThatCannotBeMatched.Add(user);
                    }
                }
                finalNames = allUsers.Except(usersThatCannotBeMatched).ToList();
            }

            if (finalNames.Any()) {
                int randomNumber = _random.Next(finalNames.Count);

                // We want to avoid the situation where there is a single person left over without a match.
                // If the requestor is the match of the person the requestor is about to be matched with, do not match them.
                while (finalNames.Count() > 1 && 
                    existingMatches.Any(m => m.MatchedId == requestor && m.RequestorId == finalNames[randomNumber].Id)) {
                    finalNames.Remove(finalNames[randomNumber]);
                    randomNumber = _random.Next(finalNames.Count);
                }

                return finalNames[randomNumber].Id;
            }
            else {
                throw new NoAvailableMatchesException();
            }
        }
    }
}
