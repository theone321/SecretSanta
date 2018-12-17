using System.Collections.Generic;

namespace SecretSanta.Models {
    public class AdminModel {
        public int UserId { get; set; }
        public string User { get; set; }

        public bool AllowRegistration { get; set; }
        public bool AllowMatching { get; set; }
        public List<UserAdminSettings> UserList { get; set; } = new List<UserAdminSettings>();
    }

    public class UserAdminSettings {
        /// <summary>
        /// The User's Id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// The User's name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The User's user name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Does this user have a match of their own?
        /// </summary>
        public bool HasMatched { get; set; }
        /// <summary>
        /// Has somebody else gotten this user as their match?
        /// </summary>
        public bool IsMatched { get; set; }
        /// <summary>
        /// Is this user an Admin?
        /// </summary>
        public bool IsAdmin { get; set; }
    }
}
