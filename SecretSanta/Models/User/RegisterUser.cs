using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models.User {
  public class RegisterUserModel {
    public string UserNameToRegister { get; set; }
    public string RealName { get; set; }
    [DataType(DataType.Password)]
    public string ChosenPassword { get; set; }
    [DataType(DataType.Password)]
    public string VerifyPassword { get; set; }
  }
}
