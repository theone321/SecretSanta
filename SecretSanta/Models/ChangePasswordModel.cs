using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models {
  public class ChangePasswordModel {
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }
    [DataType(DataType.Password)]
    public string VerifyPassword { get; set; }
    public int EventId { get; set; }
  }
}
