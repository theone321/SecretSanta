using System.Collections.Generic;

namespace SecretSanta.DataAccess {
    public interface IDataAccessor {
        Match GetExistingMatch(string requestor);
        IList<Match> GetAllExistingMatches();
        IList<MatchRestriction> GetMatchRestrictions(string requestor);
        void CreateMatch(string requestor, string matchedName, bool allowReroll);
        IList<Name> GetAllPossibleNames();
        bool AccountAlreadyRegistered(string username);
        bool VerifyCredentials(string username, string password);
        void RegisterAccount(string username, string password);
    }
}
