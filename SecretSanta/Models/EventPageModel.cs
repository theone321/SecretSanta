using System.Collections.Generic;

namespace SecretSanta.Models {
  public class EventPageModel {
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
    public int TheirSecretMatchId { get; set; }
    public string TheirSecretMatchName { get; set; }
    public bool AllowReroll { get; set; }
    public string Interests { get; set; }
    public string MatchInterests { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; }
    //public bool UserIsAdmin { get; set; }
    public LimitedUser SignificantOther { get; set; }
    public List<LimitedUser> OtherUsers { get; set; }

    public class LimitedUser {
      public int UserId { get; set; }
      public string UserRealName { get; set; }
    }
  }
}
