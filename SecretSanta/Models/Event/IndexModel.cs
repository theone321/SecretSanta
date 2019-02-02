using System.Collections.Generic;

namespace SecretSanta.Models.Event {
  public class AttendeesModel {
    public List<string> RegisteredNames { get; set; }
    public int MatchCounts { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; }
  }
}
