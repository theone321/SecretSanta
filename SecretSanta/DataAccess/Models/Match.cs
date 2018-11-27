using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess.Models {
    public class Match {
        [Key]
        public int Id { get; set; }
        public int RequestorId { get; set; }
        public int MatchedId { get; set; }
        public bool RerollAllowed { get; set; }
    }
}
