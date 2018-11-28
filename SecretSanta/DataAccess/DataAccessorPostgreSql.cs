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
            return _context.Users.Any(n => string.Equals(n.UserName, username, StringComparison.Ordinal));
        }

        public void RemoveMatch(int requestor, int matchedId) {
            var matchToRemove = _context.Matches.FirstOrDefault(m => m.RequestorId == requestor && m.MatchedId == matchedId);
            if (matchToRemove != null) {
                _context.Matches.Remove(matchToRemove);
                _context.SaveChanges();
            }
        }

        public void CreateMatch(int requestor, int matchedId, bool allowReroll) {
            Match match = new Match() {
                RequestorId = requestor,
                MatchedId = matchedId,
                RerollAllowed = allowReroll
            };

            _context.Matches.Add(match);

            _context.SaveChanges();
        }

        public void CreateRestriction(int requestor, int restrictee, bool strict, bool makeReverse) {
            MatchRestriction restrict = new MatchRestriction() {
                RequestorId = requestor,
                RestrictedId = restrictee,
                StrictRestriction = strict
            };

            _context.MatchRestrictions.Add(restrict);

            if (makeReverse) {
                MatchRestriction restrictReverse = new MatchRestriction() {
                    RequestorId = restrictee,
                    RestrictedId = requestor,
                    StrictRestriction = strict
                };
                _context.MatchRestrictions.Add(restrictReverse);
            }
        }

        public IList<Match> GetAllExistingMatches() {
            return _context.Matches.ToList();
        }

        public IList<User> GetAllUsers() {
            return _context.Users.ToList();
        }

        public User GetUserById(int id) {
            return _context.Users.Find(id);
        }

        public User GetUserByUserName(string userName) {
            return _context.Users.FirstOrDefault(u => string.Equals(u.UserName, userName, StringComparison.Ordinal));
        }

        public Match GetExistingMatch(int requestor) {
            return _context.Matches.Where(m => m.RequestorId == requestor).FirstOrDefault();
        }

        public IList<MatchRestriction> GetMatchRestrictions(int requestor) {
            return _context.MatchRestrictions.Where(mr => mr.RequestorId  == requestor).ToList();
        }

        public int RegisterAccount(string username, string password) {
            //get the account first
            User user = _context.Users.FirstOrDefault(n => string.Equals(n.UserName, username, StringComparison.Ordinal));
            if (user != null) {
                //TODO: New Exception
                throw new Exception("This user is already registered.");
            }
            else {
                //SHA256 hash the password
                string hashed = hashPassword(password);
                user = new User() {
                    UserName = username,
                    Password = hashed
                };

                if (!_context.Users.Any()) {
                    //If there are no existing users, the first user is made an admin
                    user.IsAdmin = true;
                }
                _context.Users.Add(user);
                _context.SaveChanges();

                return GetUserByUserName(username).Id;
            }
        }

        public void DeRegisterAccount(int id) {
            _context.Users.Remove(_context.Users.Find(id));
            //remove all matches where they are the requester or the matched
            IQueryable<Match> matches = _context.Matches.Where(m => m.RequestorId == id || m.MatchedId == id);
            if (matches.Any()) {
                _context.Matches.RemoveRange(matches);
            }
            _context.SaveChanges();
        }

        public void UpdateUserPassword(int id, string newPassword) {
            User user = GetUserById(id);
            if (user != null) {
                string hashed = hashPassword(newPassword);
                user.Password = hashed;
                _context.SaveChanges();
            }
        }

        public bool VerifyCredentials(string username, string password) {
            string dbPass = GetUserByUserName(username)?.Password;
            if (!string.IsNullOrEmpty(dbPass)) {
                string hashed = hashPassword(password);
                //compare the passwords
                return string.Equals(dbPass, hashed, StringComparison.Ordinal);
            }
            else {
                throw new UnregisteredUserException();
            }
        }

        public bool UserIsAdmin(int id) {
            return GetUserById(id)?.IsAdmin == true;
        }

        public void SetUserAdmin(int id, bool admin) {
            User user = GetUserById(id);
            if (user != null) {
                user.IsAdmin = admin;
                _context.SaveChanges();
            }
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

        public string GetUserInterests(int id) {
            return GetUserById(id)?.Interests;
        }

        public void SetUserInterests(int id, string interests) {
            User name = GetUserById(id);
            if (name != null) {
                name.Interests = interests;
                _context.SaveChanges();
            }
        }

        public void SetUserRealName(int id, string name) {
            User user = GetUserById(id);
            if (user != null) {
                user.RegisteredName = name;
                _context.SaveChanges();
            }
        }

        public ISession GetSession(string username, string password) {
            if (VerifyCredentials(username, password)) {
                //kill all previous sessions for this user
                IQueryable<Session> toRemove = _context.Sessions.Where(s => string.Equals(s.User, username, StringComparison.Ordinal));
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
            Session session = _context.Sessions.FirstOrDefault(s => string.Equals(s.User, username, StringComparison.Ordinal) && string.Equals(s.SessionId, sessionId, StringComparison.Ordinal));
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
            Session session = _context.Sessions.FirstOrDefault(s => string.Equals(s.SessionId, sessionId, StringComparison.Ordinal));
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
            Session session = _context.Sessions.FirstOrDefault(s => string.Equals(s.SessionId, sessionId, StringComparison.Ordinal));
            if (session != null) {
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
