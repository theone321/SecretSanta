using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess.Models {
    public class MatchRestriction {
        [Key]
        public int Id { get; set; }
        public int RequestorId { get; set; }
        public int RestrictedId { get; set; }
        public bool StrictRestriction { get; set; }
    }
}
