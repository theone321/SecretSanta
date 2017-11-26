using System.Collections.Generic;

namespace SecretSanta.DataAccess {
    public interface IDataAccessor {
        Match GetExistingMatch(string requestor);
        List<Match> GetAllExistingMatches();
        List<MatchRestriction> GetMatchRestrictions(string requestor);
        void CreateMatch(string requestor, string matchedName, bool allowReroll);
        List<Name> GetAllRegisteredNames();
        bool AccountAlreadyRegistered(string username);
        bool VerifyCredentials(string username, string password);
        void RegisterAccount(string username, string password);
    }
}
