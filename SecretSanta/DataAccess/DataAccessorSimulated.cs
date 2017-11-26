using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecretSanta.DataAccess
{
    public class DataAccessorSimulated : IDataAccessor
    {
        public IList<Name> GetAllPossibleNames()
        {
            return new List<Name> {
                new Name {
                    Id = 1,
                    RegisteredName = "Tobias Becker"
                },
                new Name {
                    Id = 2,
                    RegisteredName = "Angelia Becker"
                },
                new Name {
                    Id = 3,
                    RegisteredName = "Michael Marvin"
                },
                new Name {
                    Id = 4,
                    RegisteredName = "Sarah Marvin-Foley"
                },
                new Name {
                    Id = 5,
                    RegisteredName = "Jonathon Minelli"
                },
                new Name {
                    Id = 6,
                    RegisteredName = "Sarah Leahman"
                },
                new Name {
                    Id = 7,
                    RegisteredName = "Amanda Robinson"
                },
                new Name {
                    Id = 8,
                    RegisteredName = "Caleb Gaffney"
                },
                new Name {
                    Id = 9,
                    RegisteredName = "Dale Banas"
                },
                new Name {
                    Id = 10,
                    RegisteredName = "Dorothy Klein"
                },
                new Name {
                    Id = 11,
                    RegisteredName = "Lindsay Shockling"
                },
                new Name {
                    Id = 12,
                    RegisteredName = "Steve Rakar"
                }
            };
        }

        public Match GetExistingMatch(string requestor)
        {
            // TEMPORARY - in reality would return Matches.FirstOrDefault(m => m.RequestorName == requestor);
            if (requestor.Equals("Tobias Becker"))
            {
                return new Match
                {
                    Id = 1,
                    RequestorName = "Tobias Becker",
                    MatchedName = "Michael Marvin"
                };
            }
            return null;
        }

        public IList<MatchRestriction> GetMatchRestrictions(string requestor)
        {
            //TEMPORARY - in reality would ask database for a list of restrictions.
            //TODO - I would also use numbers and not names, string comparisons are way less safe than int comparisons
            var restrictions = new List<MatchRestriction> {
                new MatchRestriction {
                    Id = 1,
                    RequestorName = "Tobias Becker",
                    RestrictedName = "Angelia Becker"
                },
                new MatchRestriction {
                    Id = 2,
                    RequestorName = "Angelia Becker",
                    RestrictedName = "Tobias Becker"
                },
                new MatchRestriction {
                    Id = 3,
                    RequestorName = "Sarah Marvin-Foley",
                    RestrictedName = "Michael Marvin"
                },
                new MatchRestriction {
                    Id = 4,
                    RequestorName = "Michael Marvin",
                    RestrictedName = "Sarah Marvin-Foley"
                },
                new MatchRestriction {
                    Id = 5,
                    RequestorName = "Dale Banas",
                    RestrictedName = "Dorothy Klein"
                },
                new MatchRestriction {
                    Id = 6,
                    RequestorName = "Dorothy Klein",
                    RestrictedName = "Dale Banas"
                },
                new MatchRestriction {
                    Id = 7,
                    RequestorName = "Amanda Robinson",
                    RestrictedName = "Caleb Gaffney"
                },
                new MatchRestriction {
                    Id = 8,
                    RequestorName = "Caleb Gaffney",
                    RestrictedName = "Amanda Robinson"
                },
                new MatchRestriction {
                    Id = 9,
                    RequestorName = "Jonathon Minelli",
                    RestrictedName = "Lindsay Shockling"
                },
                new MatchRestriction {
                    Id = 10,
                    RequestorName = "Lindsay Shockling",
                    RestrictedName = "Jonathon Minelli"
                }
            };

            return restrictions;
        }

        public void CreateMatch(string requestor, string matchedName, bool allowReroll)
        {
            //Temporary no-op - would actually insert into table.
        }

        public IList<Match> GetAllExistingMatches()
        {
            // TEMPORARY - would get all existing matches from the database instead.
            var matches = new List<Match> {
                new Match {
                    Id = 1,
                    RequestorName = "Tobias Becker",
                    MatchedName = "Michael Marvin"
                }
            };

            return matches;
        }

        public bool AccountAlreadyRegistered(string username)
        {
            if (username == "Tobias Becker")
            {
                return true;
            }
            return false;
        }

        public bool VerifyCredentials(string username, string password)
        {
            if (username == "Tobias Becker" && password == "password1!")
            {
                return true;
            }
            if (username == "Michael Marvin" && password == "Hi")
            {
                return true;
            }
            return false;
        }

        public void RegisterAccount(string username, string password)
        {
            //no-op for now. Will save to Name table, and mark them as registered.
        }
    }
}
