﻿using System.Collections.Generic;

namespace SecretSanta.DataAccess {
    public interface IDataAccessor {
        Match GetExistingMatch(string requestor);
        List<Match> GetAllExistingMatches();
        List<MatchRestriction> GetMatchRestrictions(string requestor);
        void CreateMatch(string requestor, string matchedName);
        List<Name> GetAllRegisteredNames();

    }

    public class DataAccessor : IDataAccessor {
        public List<Name> GetAllRegisteredNames() {
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

        public Match GetExistingMatch(string requestor) {
            // TEMPORARY - in reality would return Matches.FirstOrDefault(m => m.RequestorName == requestor);
            if (requestor.Equals("Tobias Becker")) {
                return new Match {
                    Id = 1,
                    RequestorName = "Tobias Becker",
                    MatchedName = "Michael Marvin"
                };
            }
            return null;
        }

        public List<MatchRestriction> GetMatchRestrictions(string requestor) {
            //TEMPORARY - in reality would ask database for a list of restrictions.
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

        public void CreateMatch(string requestor, string matchedName) {
            //Temporary no-op - would actually insert into table.
        }

        public List<Match> GetAllExistingMatches() {
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
    }
}
