using System.Collections.Generic;
using SecretSanta.DataAccess;

namespace SecretSanta.Models {
    public class RegisterUser {
        public List<Name> PossibleNames { get; set; }
        public string NameToRegister { get; set; }
        public string ChosenPassword { get; set; }
    }
}
