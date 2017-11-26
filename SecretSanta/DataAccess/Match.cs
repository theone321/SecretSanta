using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess {
    public class Match {
        [Key]
        public int Id { get; set; }
        public string RequestorName { get; set; }
        public string MatchedName { get; set; }
        public bool RerollAllowed { get; set; }
    }
}
