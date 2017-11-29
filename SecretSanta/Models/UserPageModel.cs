using System.Collections.Generic;
using SecretSanta.DataAccess;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models {
    public class UserPageModel {
        public string Name { get; set; }
        public string TheirSecretMatch { get; set; }
        public bool AllowReroll { get; set; }
        public string Interests { get; set; }
        public string MatchInterests { get; set; }
        public bool AllowMatching { get; set; }
        public bool UserIsAdmin { get; set; }
        public PasswordResetModel PasswordReset { get; set; } = new PasswordResetModel();
    }

    public class PasswordResetModel {
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        public string VerifyPassword { get; set; }
    }
}
