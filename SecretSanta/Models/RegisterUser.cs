using System.Collections.Generic;
using SecretSanta.DataAccess.Models;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models {
    public class RegisterUser {
        public string UserNameToRegister { get; set; }
        public string RealName { get; set; }
        [DataType(DataType.Password)]
        public string ChosenPassword { get; set; }
        [DataType(DataType.Password)]
        public string VerifyPassword { get; set; }

        public bool AllowRegister { get; set; }
    }
}
