using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess.Models {
    public class Name {
        [Key]
        public int Id { get; set; }
        public string RegisteredName { get; set; }
        public string Password { get; set; }
        public bool HasRegistered { get; set; }
        public bool IsAdmin { get; set; }
    }
}
