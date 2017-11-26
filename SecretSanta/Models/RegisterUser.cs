using System.Collections.Generic;
using SecretSanta.DataAccess.Models;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models {
    public class RegisterUser {
        public IList<Name> PossibleNames { get; set; }
        public string NameToRegister { get; set; }
        [DataType(DataType.Password)]
        public string ChosenPassword { get; set; }
        [DataType(DataType.Password)]
        public string VerifyPassword { get; set; }

        public bool AllowRegister { get; set; }
    }
}
