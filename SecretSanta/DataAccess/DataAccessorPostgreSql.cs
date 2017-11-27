using SecretSanta.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SecretSanta.DataAccess.Models;

namespace SecretSanta.DataAccess {
    public class DataAccessorPostgreSql : IDataAccessor {
        private readonly DomainModelPostgreSqlContext _context;

        public DataAccessorPostgreSql(DomainModelPostgreSqlContext context) {
            _context = context;
        }

        public bool AccountAlreadyRegistered(string username) {
            return _context.Names.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase))?.HasRegistered == true;
        }

        public void RemoveMatch(string requestor, string matchedName) {
            var matchToRemove = _context.Matches.First(m => string.Equals(m.RequestorName, requestor, StringComparison.InvariantCultureIgnoreCase) && string.Equals(m.MatchedName, matchedName, StringComparison.InvariantCultureIgnoreCase));
            if (matchToRemove != null) {
                _context.Matches.Remove(matchToRemove);
                _context.SaveChanges();
            }
        }

        public void CreateMatch(string requestor, string matchedName, bool allowReroll) {
            Match match = new Match() {
                RequestorName = requestor,
                MatchedName = matchedName,
                RerollAllowed = allowReroll
            };

            _context.Matches.Add(match);

            _context.SaveChanges();
        }

        public void CreateRestriction(string requestor, string restrictee, bool strict, bool makeReverse) {
            MatchRestriction restrict = new MatchRestriction() {
                RequestorName = requestor,
                RestrictedName = restrictee,
                StrictRestriction = strict
            };

            _context.MatchRestrictions.Add(restrict);

            if (makeReverse) {
                MatchRestriction restrictReverse = new MatchRestriction() {
                    RequestorName = restrictee,
                    RestrictedName = requestor,
                    StrictRestriction = strict
                };
                _context.MatchRestrictions.Add(restrictReverse);
            }
        }

        public IList<Match> GetAllExistingMatches() {
            return _context.Matches.ToList();
        }

        public IList<Name> GetAllPossibleNames() {
            return _context.Names.ToList();
        }

        public IList<Name> GetAllRegisteredNames() {
            return _context.Names.Where(n => n.HasRegistered).ToList();
        }

        public Match GetExistingMatch(string requestor) {
            return _context.Matches.Where(m => string.Equals(m.RequestorName, requestor, StringComparison.InvariantCultureIgnoreCase))?.FirstOrDefault();
        }

        public IList<MatchRestriction> GetMatchRestrictions(string requestor) {
            return _context.MatchRestrictions.Where(mr => string.Equals(mr.RequestorName, requestor, StringComparison.InvariantCultureIgnoreCase))?.ToList();
        }

        public void RegisterAccount(string username, string password) {
            //get the account first
            Name name = _context.Names.FirstOrDefault(n => !n.HasRegistered && string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase));
            if (name != null) {
                //SHA256 hash the password
                string hashed = hashPassword(password);

                name.Password = hashed;
                name.HasRegistered = true;

                _context.SaveChanges();
            }
            else {
                //TODO: New Exception
                throw new Exception("This user is already registered.");
            }

        }

        public bool VerifyCredentials(string username, string password) {
            string dbPass = _context.Names.FirstOrDefault(n => n.HasRegistered && string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase))?.Password;
            if (!string.IsNullOrEmpty(dbPass)) {
                string hashed = hashPassword(password);
                //compare the passwords
                return string.Equals(dbPass, hashed, StringComparison.OrdinalIgnoreCase);
            }
            else {
                throw new UnregisteredUserException();
            }
        }

        public bool UserIsAdmin(string username) {
            return _context.Names.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase))?.IsAdmin == true;
        }

        public string GetSettingValue(string setting) {
            return _context.Settings.FirstOrDefault(s => string.Equals(s.Name, setting, StringComparison.Ordinal))?.Value;
        }

        public void SetSettingValue(string setting, string value) {
            Setting settingObj = _context.Settings.FirstOrDefault(s => string.Equals(s.Name, setting, StringComparison.Ordinal));
            if (settingObj != null) {
                settingObj.Value = value;

                _context.SaveChanges();
            }
        }

        public IList<Setting> GetAllSettings() {
            return _context.Settings.ToList();
        }

        public string GetUserInterests(string username) {
            return _context.Names.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase))?.Interests;
        }

        public void SetUserInterests(string username, string interests) {
            Name name = _context.Names.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase));
            if (name != null) {
                name.Interests = interests;
                _context.SaveChanges();
            }
        }

        public ISession GetSession(string username, string password) {
            if (VerifyCredentials(username, password)) {
                //kill all previous sessions for this user
                var toRemove = _context.Sessions.Where(s => string.Equals(s.User, username, StringComparison.InvariantCultureIgnoreCase));
                if (toRemove.Any()) {
                    _context.Sessions.RemoveRange(toRemove);
                }
                Session session = new Session(username);
                _context.Sessions.Add(session);
                _context.SaveChanges();
                return session;
            }
            return null;
        }

        public bool VerifySession(string username, string sessionId) {
            bool verified = false;
            Session session = _context.Sessions.FirstOrDefault(s => string.Equals(s.User, username, StringComparison.InvariantCultureIgnoreCase) && string.Equals(s.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
            if (session != null) {
                //check the timestamp, if less than the timeout then good
                DateTime rightNow = DateTime.UtcNow;

                int.TryParse(GetSettingValue("SessionTimeout"), out int timeout);

                if ((rightNow - session.TimeStamp) < TimeSpan.FromMinutes(timeout)) {
                    session.TimeStamp = DateTime.UtcNow;
                    verified = true;
                }
                else {
                    _context.Sessions.Remove(session);
                }
                _context.SaveChanges();
            }
            return verified;
        }

        public ISession GetSessionData(string sessionId) {
            Session session = _context.Sessions.FirstOrDefault(s => string.Equals(s.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
            if (session != null) {
                //check the timestamp, if less than the timeout then good
                DateTime rightNow = DateTime.UtcNow;

                int.TryParse(GetSettingValue("SessionTimeout"), out int timeout);

                if ((rightNow - session.TimeStamp) < TimeSpan.FromMinutes(timeout)) {
                    session.TimeStamp = DateTime.UtcNow;
                }
                else {
                    _context.Sessions.Remove(session);
                    session = null;
                }
                _context.SaveChanges();
                return session;
            }
            return null;
        }

        public void EndSession(string sessionId) {
            Session session = _context.Sessions.FirstOrDefault(s => string.Equals(s.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
            if (session != null)
            {
                _context.Sessions.Remove(session);
                _context.SaveChanges();
            }
        }

        private string hashPassword(string password) {
            byte[] bytes = Encoding.UTF8.GetBytes("santaSalt" + password);
            byte[] hashedBytes = SHA256.Create().ComputeHash(bytes);

            StringBuilder hashedBuilder = new StringBuilder(256);
            foreach (byte b in hashedBytes) {
                hashedBuilder.Append(b.ToString("x2"));
            }

            return hashedBuilder.ToString();
        }


    }
}
