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
