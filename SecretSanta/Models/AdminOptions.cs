using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecretSanta.Models
{
    public class AdminModel {
        public string User { get; set; }

        public bool AllowRegistration { get; set; }
        public bool AllowMatching { get; set; }
        public List<UserAdminSettings> UserList { get; set; } = new List<UserAdminSettings>();
    }

    public class UserAdminSettings {
        /// <summary>
        /// The User's name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Whether the User is registered
        /// </summary>
        public bool HasRegistered { get; set; }
        /// <summary>
        /// Does this user have a match of their own?
        /// </summary>
        public bool HasMatched { get; set; }
        /// <summary>
        /// Has somebody else gotten this user as their match?
        /// </summary>
        public bool IsMatched { get; set; }
    }
}
