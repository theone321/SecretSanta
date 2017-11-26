using SecretSanta.DataAccess;
using SecretSanta.DependencyWrappers;
using System.Collections.Generic;
using System.Linq;

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
            var allNames = _dataAccessor.GetAllPossibleNames().ToList();
            allNames.RemoveAll(n => n.RegisteredName == requestor);
            var removedNames = new List<Name>();
            var restrictions = _dataAccessor.GetMatchRestrictions(requestor);
            var existingMatches = _dataAccessor.GetAllExistingMatches();
            foreach (var name in allNames) {
                if (restrictions.Any(r => r.RequestorName == requestor && r.RestrictedName == name.RegisteredName)) {
                    removedNames.Add(name);
                    continue;
                }
                //Remove any names that have already been matched to someone else.
                if (existingMatches.Any(m => m.MatchedName == name.RegisteredName)) {
                    removedNames.Add(name);
                }
            }
            allNames = allNames.Except(removedNames).ToList();
            var randomNumber = _random.Next(allNames.Count);
            return allNames[randomNumber].RegisteredName;
        }
    }
}
