using System;
using System.Collections.Generic;

namespace SecretSanta.Models.Event.SecretMatch {
  public class MatchEventPageModel {
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
    public int TheirSecretMatchId { get; set; }
    public string TheirSecretMatchName { get; set; }
    public bool AllowReroll { get; set; }
    public string Interests { get; set; }
    public string MatchInterests { get; set; }
    public int EventId { get; set; }
    public bool AllowMatching { get; set; }
    public DateTime EventDate { get; set; }
    public string EventName { get; set; }
    public string Location { get; set; }
    public string EventDescription { get; set; }
    public string SharedId { get; set; }
    public LimitedUser SignificantOther { get; set; }
    public List<LimitedUser> OtherUsers { get; set; }
    public int RegisteredCount { get; set; }
    public int MatchCounts { get; set; }

    public class LimitedUser {
      public int UserId { get; set; }
      public string UserRealName { get; set; }
    }
  }
}
