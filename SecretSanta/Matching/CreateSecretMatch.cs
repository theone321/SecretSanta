﻿using SecretSanta.DataAccess;
using SecretSanta.DependencyWrappers;
using System.Collections.Generic;
using System.Linq;
using SecretSanta.DataAccess.Models;
using System;
using SecretSanta.Exceptions;

namespace SecretSanta.Matching {
    public interface ICreateSecretMatch {
        string FindRandomMatch(string requestor);
    }

    public class CreateSecretMatch : ICreateSecretMatch {
        private static IRandomWrapper _random;
        private IDataAccessor _dataAccessor;

        public CreateSecretMatch(IDataAccessor dataAccessor, IRandomWrapper randomWrapper) {
            _dataAccessor = dataAccessor;
            _random = randomWrapper;
        }

        public string FindRandomMatch(string requestor) {
            var allNames = _dataAccessor.GetAllRegisteredNames().ToList();
            allNames.RemoveAll(n => string.Equals(n.RegisteredName, requestor, StringComparison.InvariantCultureIgnoreCase));
            var removedNames = new List<Name>();
            var restrictions = _dataAccessor.GetMatchRestrictions(requestor);
            var existingMatches = _dataAccessor.GetAllExistingMatches();
            foreach (var name in allNames) {
                if (restrictions?.Any(r => string.Equals(r.RestrictedName, name.RegisteredName, StringComparison.InvariantCultureIgnoreCase)) == true) {
                    removedNames.Add(name);
                    continue;
                }
                //Remove any names that have already been matched to someone else.
                if (existingMatches?.Any(m => string.Equals(m.MatchedName, name.RegisteredName, StringComparison.InvariantCultureIgnoreCase)) == true) {
                    removedNames.Add(name);
                }
            }
            var finalNames = allNames.Except(removedNames).ToList();
            //allow non-strict restrictions through if there is no match
            if (!finalNames.Any()) {
                removedNames.Clear();
                foreach (var name in allNames) {
                    if (restrictions?.Any(r => r.StrictRestriction && string.Equals(r.RestrictedName, name.RegisteredName, StringComparison.InvariantCultureIgnoreCase)) == true) {
                        removedNames.Add(name);
                        continue;
                    }
                    //Remove any names that have already been matched to someone else.
                    if (existingMatches?.Any(m => m.MatchedName == name.RegisteredName) == true) {
                        removedNames.Add(name);
                    }
                }
                finalNames = allNames.Except(removedNames).ToList();
            }
            //allow even strict restrictions through if there is no match, this is a last resort
            if (!finalNames.Any()) {
                removedNames.Clear();
                foreach (var name in allNames) {
                    //Remove any names that have already been matched to someone else.
                    if (existingMatches?.Any(m => m.MatchedName == name.RegisteredName) == true) {
                        removedNames.Add(name);
                    }
                }
                finalNames = allNames.Except(removedNames).ToList();
            }

            if (finalNames.Any()) {
                var randomNumber = _random.Next(finalNames.Count);
                return finalNames[randomNumber].RegisteredName;
            }
            else {
                throw new NoAvailableMatchesException();
            }
        }
    }
}
