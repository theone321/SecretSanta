using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecretSanta.DataAccess
{
    public class DataAccessorPostgreSql : IDataAccessor
    {
        public bool AccountAlreadyRegistered(string username)
        {
            throw new NotImplementedException();
        }

        public void CreateMatch(string requestor, string matchedName, bool allowReroll)
        {
            throw new NotImplementedException();
        }

        public List<Match> GetAllExistingMatches()
        {
            throw new NotImplementedException();
        }

        public List<Name> GetAllRegisteredNames()
        {
            throw new NotImplementedException();
        }

        public Match GetExistingMatch(string requestor)
        {
            throw new NotImplementedException();
        }

        public List<MatchRestriction> GetMatchRestrictions(string requestor)
        {
            throw new NotImplementedException();
        }

        public void RegisterAccount(string username, string password)
        {
            throw new NotImplementedException();
        }

        public bool VerifyCredentials(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
