using System.Collections.Generic;
using SecretSanta.DataAccess.Models;
using System.Linq;
using SecretSanta.Exceptions;
using System;

namespace SecretSanta.DataAccess {
    public class DataAccessorSimulated : IDataAccessor {
        private static List<Name> _names = new List<Name> {
                new Name {
                    Id = 1,
                    RegisteredName = "Tobias Becker", 
                    IsAdmin = true,
                    Interests = "a"
                },
                new Name {
                    Id = 2,
                    RegisteredName = "Angelia Becker",
                    Interests = "b"
                },
                new Name {
                    Id = 3,
                    RegisteredName = "Michael Marvin",
                    IsAdmin = true,
                    Interests = "c"
                },
                new Name {
                    Id = 4,
                    RegisteredName = "Sarah Marvin-Foley",
                    Interests = "d"
                },
                new Name {
                    Id = 5,
                    RegisteredName = "Jonathon Minelli",
                    Interests = "e"
                },
                new Name {
                    Id = 6,
                    RegisteredName = "Sarah Leahman",
                    Interests = "f"
                },
                new Name {
                    Id = 7,
                    RegisteredName = "Amanda Robinson",
                    Interests = "g"
                },
                new Name {
                    Id = 8,
                    RegisteredName = "Caleb Gaffney",
                    Interests = "h"
                },
                new Name {
                    Id = 9,
                    RegisteredName = "Dale Banas",
                    Interests = "i"
                },
                new Name {
                    Id = 10,
                    RegisteredName = "Dorothy Klein",
                    Interests = "j"
                },
                new Name {
                    Id = 11,
                    RegisteredName = "Lindsay Shockling",
                    Interests = "k"
                },
                new Name {
                    Id = 12,
                    RegisteredName = "Steve Rakar",
                    Interests = "l"
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

        private static List<Setting> _settings = new List<Setting> {
            new Setting() {
                Name = "AllowRegistration",
                Value = "true"
            },
            new Setting() {
                Name = "AllowMatching",
                Value = "false"
            },
            new Setting() {
                Name = "SessionTimeout",
                Value = "15" //minutes
            }
        };

        private static List<Session> _sessions = new List<Session>();

        public IList<Name> GetAllPossibleNames() {
            return _names;
        }

        public IList<Name> GetAllRegisteredNames() {
            return _names.Where(n => n.HasRegistered).ToList();
        }

        public Match GetExistingMatch(string requestor) {
            return _matches.Where(m => string.Equals(m.RequestorName, requestor, StringComparison.InvariantCultureIgnoreCase))?.FirstOrDefault();
        }

        public IList<MatchRestriction> GetMatchRestrictions(string requestor) {
            return _restrictions.Where(r => string.Equals(r.RequestorName, requestor, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public void RemoveMatch(string requestor, string matchedName) {
            _matches.RemoveAll(m => string.Equals(m.RequestorName, requestor, StringComparison.InvariantCultureIgnoreCase) && string.Equals(m.MatchedName, matchedName, StringComparison.InvariantCultureIgnoreCase));
        }

        public void CreateMatch(string requestor, string matchedName, bool allowReroll) {
            Match match = new Match() {
                Id = _matches.LastOrDefault()?.Id + 1 ?? 1,
                RequestorName = requestor,
                MatchedName = matchedName,
                RerollAllowed = allowReroll
            };
            _matches.Add(match);
        }

        public IList<Match> GetAllExistingMatches() {
            return _matches;
        }

        public bool AccountAlreadyRegistered(string username) {
            return _names.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase))?.HasRegistered == true;
        }

        public bool VerifyCredentials(string username, string password) {
            Name name = _names.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase));
            if (name != null) {
                return string.Equals(name.Password, password, StringComparison.OrdinalIgnoreCase);
            }
            throw new InvalidCredentialsException();
        }

        public void RegisterAccount(string username, string password) {
            Name name = _names.FirstOrDefault(n => !n.HasRegistered && string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase));
            if (name != null) {
                name.Password = password;
                name.HasRegistered = true;
            }
            else {
                //TODO: New Exception
                throw new Exception("This user is already registered.");
            }
        }

        public void CreateRestriction(string requestor, string restrictee, bool strict, bool makeReverse) {
            MatchRestriction restrict = new MatchRestriction() {
                Id = _restrictions?.LastOrDefault()?.Id + 1 ?? 1,
                RequestorName = requestor,
                RestrictedName = restrictee,
                StrictRestriction = strict
            };

            _restrictions.Add(restrict);

            if (makeReverse) {
                MatchRestriction restrictReverse = new MatchRestriction() {
                    Id = _restrictions?.LastOrDefault()?.Id + 1 ?? 1,
                    RequestorName = restrictee,
                    RestrictedName = requestor,
                    StrictRestriction = strict
                };
                _restrictions.Add(restrictReverse);
            }
        }

        public bool UserIsAdmin(string username) {
            return _names.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase))?.IsAdmin == true;
        }

        public string GetSettingValue(string setting) {
            return _settings.FirstOrDefault(s => string.Equals(s.Name, setting, StringComparison.Ordinal))?.Value;
        }

        public void SetSettingValue(string setting, string value) {
            Setting settingObj =_settings.FirstOrDefault(s => string.Equals(s.Name, setting, StringComparison.Ordinal));
            if (settingObj != null) {
                settingObj.Value = value;
            }
        }

        public IList<Setting> GetAllSettings() {
            return _settings;
        }

        public string GetUserInterests(string username) {
            return _names.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase))?.Interests;
        }

        public void SetUserInterests(string username, string interests) {
            Name name = _names.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase));
            if (name != null) {
                name.Interests = interests;
            }
        }

        public ISession GetSession(string username, string password) {
            if (VerifyCredentials(username, password)) {
                //kill all previous sessions for this user
                _sessions.RemoveAll(s => string.Equals(s.User, username, StringComparison.InvariantCultureIgnoreCase));
                Session session = new Session(username);
                _sessions.Add(session);
                return session;
            }
            return null;
        }

        public bool VerifySession(string username, string sessionId) {
            Session session = _sessions.FirstOrDefault(s => string.Equals(s.User, username, StringComparison.InvariantCultureIgnoreCase) && string.Equals(s.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
            if (session != null) {
                //check the timestamp, if less than the timeout then good
                DateTime rightNow = DateTime.UtcNow;

                int.TryParse(GetSettingValue("SessionTimeout"), out int timeout);

                if ((rightNow - session.TimeStamp) < TimeSpan.FromMinutes(timeout)) {
                    session.TimeStamp = DateTime.UtcNow;
                    return true;
                }
                else {
                    _sessions.Remove(session);
                }
            }
            return false;
        }

        public ISession GetSessionData(string sessionId) {
            Session session = _sessions.FirstOrDefault(s => string.Equals(s.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
            if (session != null) {
                //check the timestamp, if less than the timeout then good
                DateTime rightNow = DateTime.UtcNow;

                int.TryParse(GetSettingValue("SessionTimeout"), out int timeout);

                if ((rightNow - session.TimeStamp) < TimeSpan.FromMinutes(timeout)) {
                    session.TimeStamp = DateTime.UtcNow;
                    return session;
                }
                else {
                    _sessions.Remove(session);
                }
            }
            return null;
        }

        public void EndSession(string sessionId) {
            _sessions.RemoveAll(s => string.Equals(s.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
