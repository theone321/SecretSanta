using System.Collections.Generic;
using SecretSanta.DataAccess;

namespace SecretSanta.Models {
    public class SecretMatch {
        public string Name { get; set; }
        public string TheirSecretMatch { get; set; }
        public bool AllowReroll { get; set; }
        public string Interests { get; set; }
        public string MatchInterests { get; set; }
        public bool AllowMatching { get; set; }
        public bool UserIsAdmin { get; set; }
    }
}
