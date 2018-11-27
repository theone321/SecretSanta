using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess.Models {
    public class User {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string RegisteredName { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public string Interests { get; set; }
    }
}
