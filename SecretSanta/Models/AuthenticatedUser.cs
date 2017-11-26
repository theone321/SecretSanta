using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models {
    public class AuthenticatedUser {
        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
