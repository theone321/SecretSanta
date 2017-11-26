using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess.Models {
    public class MatchRestriction {
        [Key]
        public int Id { get; set; }
        public string RequestorName { get; set; }
        public string RestrictedName { get; set; }
        public bool StrictRestriction { get; set; }
    }
}
