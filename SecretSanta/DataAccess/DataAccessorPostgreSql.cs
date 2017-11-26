using Microsoft.EntityFrameworkCore;
using SecretSanta.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SecretSanta.DataAccess.Models;
using System.Threading.Tasks;

namespace SecretSanta.DataAccess
{
    public class DataAccessorPostgreSql : IDataAccessor
    {
        private readonly DomainModelPostgreSqlContext _context;

        public DataAccessorPostgreSql(DomainModelPostgreSqlContext context)
        {
            _context = context;
        }

        public bool AccountAlreadyRegistered(string username)
        {
            return _context.Name.FirstOrDefault(n => string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase))?.HasRegistered == true;
        }

        public void CreateMatch(string requestor, string matchedName, bool allowReroll)
        {
            //_context.SaveChanges();
            throw new NotImplementedException();
        }

        public IList<Match> GetAllExistingMatches()
        {
            throw new NotImplementedException();
        }

        public IList<Name> GetAllPossibleNames()
        {
            return _context.Name.ToList();
        }

        public Match GetExistingMatch(string requestor)
        {
            throw new NotImplementedException();
        }

        public IList<MatchRestriction> GetMatchRestrictions(string requestor)
        {
            throw new NotImplementedException();
        }

        public void RegisterAccount(string username, string password)
        {
            //get the account first
            Name name = _context.Name.FirstOrDefault(n => !n.HasRegistered && string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase));
            if (name != null)
            {
                //SHA256 hash the password
                string hashed = hashPassword(password);

                name.Password = hashed;
                name.HasRegistered = true;

                _context.SaveChanges();
            }
            else
            {
                //TODO: New Exception
                throw new Exception("This user is already registered.");
            }
            
        }

        public bool VerifyCredentials(string username, string password)
        {
            string dbPass = _context.Name.FirstOrDefault(n => n.HasRegistered && string.Equals(n.RegisteredName, username, StringComparison.InvariantCultureIgnoreCase))?.Password;
            if (!string.IsNullOrEmpty(dbPass))
            {
                string hashed = hashPassword(password);
                //compare the passwords
                return string.Equals(dbPass, hashed, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                throw new UnregisteredUserException();
            }
        }


        private string hashPassword(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("santaSalt" + password);
            byte[] hashedBytes = SHA256.Create().ComputeHash(bytes);

            StringBuilder hashedBuilder = new StringBuilder(256);
            foreach (byte b in hashedBytes)
            {
                hashedBuilder.Append(b.ToString("x2"));
            }

            return hashedBuilder.ToString();
        }
    }
}
