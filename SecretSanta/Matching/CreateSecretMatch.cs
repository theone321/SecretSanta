using SecretSanta.DataAccess;
using SecretSanta.DependencyWrappers;
using System.Collections.Generic;
using System.Linq;
using SecretSanta.DataAccess.Models;
using System;
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
            List<User> allUsers = _dataAccessor.GetAllUsers().ToList();
            allUsers.RemoveAll(n => n.Id == requestor);
            List<User> removedNames = new List<User>();
            IList<MatchRestriction> restrictions = _dataAccessor.GetMatchRestrictions(requestor);
            IList<Match> existingMatches = _dataAccessor.GetAllExistingMatches();
            foreach (User user in allUsers) {
                if (restrictions?.Any(r => r.RestrictedId == user.Id) == true) {
                    removedNames.Add(user);
                    continue;
                }
                //Remove any names that have already been matched to someone else.
                if (existingMatches?.Any(m => m.MatchedId == user.Id) == true) {
                    removedNames.Add(user);
                }
            }
            List<User> finalNames = allUsers.Except(removedNames).ToList();
            //allow non-strict restrictions through if there is no match
            if (!finalNames.Any()) {
                removedNames.Clear();
                foreach (User user in allUsers) {
                    if (restrictions?.Any(r => r.StrictRestriction && r.RestrictedId == user.Id) == true) {
                        removedNames.Add(user);
                        continue;
                    }
                    //Remove any names that have already been matched to someone else.
                    if (existingMatches?.Any(m => m.MatchedId == user.Id) == true) {
                        removedNames.Add(user);
                    }
                }
                finalNames = allUsers.Except(removedNames).ToList();
            }
            //allow even strict restrictions through if there is no match, this is a last resort
            if (!finalNames.Any()) {
                removedNames.Clear();
                foreach (User user in allUsers) {
                    //Remove any names that have already been matched to someone else.
                    if (existingMatches?.Any(m => m.MatchedId == user.Id) == true) {
                        removedNames.Add(user);
                    }
                }
                finalNames = allUsers.Except(removedNames).ToList();
            }

            if (finalNames.Any()) {
                int randomNumber = _random.Next(finalNames.Count);
                return finalNames[randomNumber].Id;
            }
            else {
                throw new NoAvailableMatchesException();
            }
        }
    }
}
