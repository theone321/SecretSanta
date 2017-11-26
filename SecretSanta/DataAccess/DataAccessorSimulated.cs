using System.Collections.Generic;
using SecretSanta.DataAccess.Models;
using System.Linq;
using SecretSanta.Exceptions;
using System;

namespace SecretSanta.DataAccess
{
    public class DataAccessorSimulated : IDataAccessor
    {
        private static List<Name> _names = new List<Name> {
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

        private static List<MatchRestriction> _restrictions = new List<MatchRestriction> {
                new MatchRestriction {
                    Id = 1,
                    RequestorName = "Tobias Becker",
                    RestrictedName = "Angelia Becker",
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 2,
                    RequestorName = "Angelia Becker",
                    RestrictedName = "Tobias Becker",
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 3,
                    RequestorName = "Sarah Marvin-Foley",
                    RestrictedName = "Michael Marvin",
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 4,
                    RequestorName = "Michael Marvin",
                    RestrictedName = "Sarah Marvin-Foley",
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 5,
                    RequestorName = "Dale Banas",
                    RestrictedName = "Dorothy Klein",
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 6,
                    RequestorName = "Dorothy Klein",
                    RestrictedName = "Dale Banas",
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 7,
                    RequestorName = "Amanda Robinson",
                    RestrictedName = "Caleb Gaffney",
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 8,
                    RequestorName = "Caleb Gaffney",
                    RestrictedName = "Amanda Robinson",
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 9,
                    RequestorName = "Jonathon Minelli",
                    RestrictedName = "Lindsay Shockling",
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 10,
                    RequestorName = "Lindsay Shockling",
                    RestrictedName = "Jonathon Minelli",
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 11,
                    RequestorName = "Andrew Sansone",
                    RestrictedName = "Heather Sansone",
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 12,
                    RequestorName = "Heather Sansone",
                    RestrictedName = "Andrew Sansone",
                    StrictRestriction = true
                }
            };

        private static List<Match> _matches = new List<Match>();

        public IList<Name> GetAllPossibleNames()
        {
            return _names;
        }

        public IList<Name> GetAllRegisteredNames()
        {
            return _names.Where(n => n.HasRegistered).ToList();
        }

        public Match GetExistingMatch(string requestor)
        {
            return _matches.Where(m => string.Equals(m.RequestorName, requestor, StringComparison.InvariantCultureIgnoreCase))?.FirstOrDefault();
        }

        public IList<MatchRestriction> GetMatchRestrictions(string requestor)
        {
            return _restrictions.Where(r => string.Equals(r.RequestorName, requestor, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public void CreateMatch(string requestor, string matchedName, bool allowReroll)
        {
            Match match = new Match()
            {
                Id = _matches.Last().Id + 1,
                RequestorName = requestor,
                MatchedName = matchedName,
                RerollAllowed = allowReroll
            };
            _matches.Add(match);
        }

        public IList<Match> GetAllExistingMatches()
        {
            return _matches;
        }

        public bool AccountAlreadyRegistered(string username)
        {
            return _names.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase))?.HasRegistered == true;
        }

        public bool VerifyCredentials(string username, string password)
        {
            Name name = _names.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase));
            if (name != null)
            {
                return string.Equals(name.Password, password, StringComparison.OrdinalIgnoreCase);
            }
            throw new InvalidCredentialsException();
        }

        public void RegisterAccount(string username, string password)
        {
            Name name = _names.FirstOrDefault(n => !n.HasRegistered && string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase));
            if (name != null)
            {
                name.Password = password;
                name.HasRegistered = true;
            }
            else
            {
                //TODO: New Exception
                throw new Exception("This user is already registered.");
            }
        }

        public void CreateRestriction(string requestor, string restrictee, bool strict, bool makeReverse)
        {
            MatchRestriction restrict = new MatchRestriction()
            {
                Id = _restrictions?.LastOrDefault()?.Id + 1 ?? 1,
                RequestorName = requestor,
                RestrictedName = restrictee,
                StrictRestriction = strict
            };

            _restrictions.Add(restrict);

            if (makeReverse)
            {
                MatchRestriction restrictReverse = new MatchRestriction()
                {
                    Id = _restrictions?.LastOrDefault()?.Id + 1 ?? 1,
                    RequestorName = restrictee,
                    RestrictedName = requestor,
                    StrictRestriction = strict
                };
                _restrictions.Add(restrictReverse);
            }
        }
    }
}
