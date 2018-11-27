using System.Collections.Generic;
using SecretSanta.DataAccess.Models;
using System.Linq;
using SecretSanta.Exceptions;
using System;

namespace SecretSanta.DataAccess {
    public class DataAccessorSimulated : IDataAccessor {
        private static List<User> _users = new List<User> {
                new User {
                    Id = 1,
                    UserName = "userA",
                    RegisteredName = "User A", 
                    IsAdmin = true,
                    Interests = "a",
                    Password = "pass"
                },
                new User {
                    Id = 2,
                    UserName = "userB",
                    RegisteredName = "User B",
                    Interests = "b",
                    Password = "pass"
                },
                new User {
                    Id = 3,
                    UserName = "userC",
                    RegisteredName = "User C",
                    IsAdmin = true,
                    Interests = "c",
                    Password = "pass"
                },
                new User {
                    Id = 4,
                    UserName = "userD",
                    RegisteredName = "User D",
                    Interests = "d",
                    Password = "pass"
                },
                new User {
                    Id = 5,
                    UserName = "userE",
                    RegisteredName = "User E",
                    Interests = "e"
                },
                new User {
                    Id = 6,
                    UserName = "userF",
                    RegisteredName = "User F",
                    Interests = "f"
                },
                new User {
                    Id = 7,
                    UserName = "userG",
                    RegisteredName = "User G",
                    Interests = "g"
                },
                new User {
                    Id = 8,
                    UserName = "userH",
                    RegisteredName = "User H",
                    Interests = "h"
                },
                new User {
                    Id = 9,
                    UserName = "userI",
                    RegisteredName = "User I",
                    Interests = "i"
                },
                new User {
                    Id = 10,
                    UserName = "userJ",
                    RegisteredName = "User J",
                    Interests = "j"
                },
                new User {
                    Id = 11,
                    UserName = "userK",
                    RegisteredName = "User K",
                    Interests = "k"
                },
                new User {
                    Id = 12,
                    UserName = "userL",
                    RegisteredName = "User L",
                    Interests = "l"
                }
            };

        private static List<MatchRestriction> _restrictions = new List<MatchRestriction> {
                new MatchRestriction {
                    Id = 1,
                    RequestorId = 1,
                    RestrictedId = 2,
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 2,
                    RequestorId = 2,
                    RestrictedId = 1,
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 3,
                    RequestorId = 3,
                    RestrictedId = 4,
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 4,
                    RequestorId = 4,
                    RestrictedId = 3,
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 5,
                    RequestorId = 5,
                    RestrictedId = 6,
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 6,
                    RequestorId = 6,
                    RestrictedId = 5,
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 7,
                    RequestorId = 7,
                    RestrictedId = 8,
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 8,
                    RequestorId = 8,
                    RestrictedId = 7,
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 9,
                    RequestorId = 9,
                    RestrictedId = 10,
                    StrictRestriction = true
                },
                new MatchRestriction {
                    Id = 10,
                    RequestorId = 10,
                    RestrictedId = 9,
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
                Value = "true"
            },
            new Setting() {
                Name = "SessionTimeout",
                Value = "15" //minutes
            }
        };

        private static List<Session> _sessions = new List<Session>();

        public IList<User> GetAllUsers() {
            return _users;
        }

        public User GetUserById(int id) {
            return _users.Find(u => u.Id == id);
        }

        public User GetUserByUserName(string username) {
            return _users.Find(u => string.Equals(u.UserName, username, StringComparison.Ordinal));
        }

        public Match GetExistingMatch(int requestor) {
            return _matches.Where(m => m.RequestorId == requestor)?.FirstOrDefault();
        }

        public IList<MatchRestriction> GetMatchRestrictions(int requestor) {
            return _restrictions.Where(r => r.RequestorId == requestor).ToList();
        }

        public void RemoveMatch(int requestor, int matchedId) {
            _matches.RemoveAll(m => m.RequestorId == requestor && m.MatchedId == matchedId);
        }

        public void CreateMatch(int requestor, int matchedId, bool allowReroll) {
            Match match = new Match() {
                Id = _matches.LastOrDefault()?.Id + 1 ?? 1,
                RequestorId = requestor,
                MatchedId = matchedId,
                RerollAllowed = allowReroll
            };
            _matches.Add(match);
        }

        public IList<Match> GetAllExistingMatches() {
            return _matches;
        }

        public bool AccountAlreadyRegistered(string username) {
            return _users.Any(n => string.Equals(n.UserName, username, StringComparison.Ordinal));
        }

        public bool VerifyCredentials(string username, string password) {
            User name = _users.FirstOrDefault(n => string.Equals(n.UserName, username, StringComparison.Ordinal));
            if (name != null) {
                return string.Equals(name.Password, password, StringComparison.Ordinal);
            }
            throw new InvalidCredentialsException();
        }

        public int RegisterAccount(string username, string password) {
            User user = _users.FirstOrDefault(n => string.Equals(n.UserName, username, StringComparison.Ordinal));
            if (user != null) {
                //TODO: New Exception
                throw new Exception("This user is already registered.");
            }
            else {
                user = new User()
                {
                    UserName = username,
                    Password = password
                };
                _users.Add(user);

                return GetUserByUserName(username).Id;
            }
        }

        public void DeRegisterAccount(int id) {
            User user = GetUserById(id);
            _users.Remove(user);
            //remove all matches where they are the requester or the matched
            _matches.RemoveAll(m => m.RequestorId == id || m.MatchedId == id);
        }

        public void UpdateUserPassword(int id, string newPassword) {
            User user = GetUserById(id);
            if (user != null) {
                user.Password = newPassword;
            }
        }

        public void CreateRestriction(int requestor, int restrictee, bool strict, bool makeReverse) {
            MatchRestriction restrict = new MatchRestriction() {
                Id = _restrictions?.LastOrDefault()?.Id + 1 ?? 1,
                RequestorId = requestor,
                RestrictedId = restrictee,
                StrictRestriction = strict
            };

            _restrictions.Add(restrict);

            if (makeReverse) {
                MatchRestriction restrictReverse = new MatchRestriction() {
                    Id = _restrictions?.LastOrDefault()?.Id + 1 ?? 1,
                    RequestorId = restrictee,
                    RestrictedId = requestor,
                    StrictRestriction = strict
                };
                _restrictions.Add(restrictReverse);
            }
        }

        public bool UserIsAdmin(int id) {
            return GetUserById(id)?.IsAdmin == true;
        }

        public void SetUserAdmin(int id, bool admin) {
            User user = GetUserById(id);
            if (user != null) {
                user.IsAdmin = admin;
            }
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

        public string GetUserInterests(int id) {
            return GetUserById(id)?.Interests;
        }

        public void SetUserInterests(int id, string interests) {
            User user = GetUserById(id);
            if (user != null) {
                user.Interests = interests;
            }
        }

        public void SetUserRealName(int id, string name) {
            User user = GetUserById(id);
            if (user != null) {
                user.RegisteredName = name;
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
