using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models {
  public class RegisterUser {
    public string UserNameToRegister { get; set; }
    public string RealName { get; set; }
    [DataType(DataType.Password)]
    public string ChosenPassword { get; set; }
    [DataType(DataType.Password)]
    public string VerifyPassword { get; set; }

    // Doesn't make sense here anymore since registration is not tied to being part of any event
    //public bool AllowRegister { get; set; }
  }
}
