using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models {
  public class UserPageModel {
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
    public int TheirSecretMatchId { get; set; }
    public string TheirSecretMatchName { get; set; }
    public bool AllowReroll { get; set; }
    public string Interests { get; set; }
    public string MatchInterests { get; set; }
    public bool AllowMatching { get; set; }
    public int EventId { get; set; }
    //public bool UserIsAdmin { get; set; }
    public PasswordResetModel PasswordReset { get; set; } = new PasswordResetModel();
    public LimitedUser SignificantOther { get; set; }
    public List<LimitedUser> OtherUsers { get; set; }

    public class LimitedUser {
      public int UserId { get; set; }
      public string UserRealName { get; set; }
    }
  }

  public class PasswordResetModel {
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }
    [DataType(DataType.Password)]
    public string VerifyPassword { get; set; }
  }
}
