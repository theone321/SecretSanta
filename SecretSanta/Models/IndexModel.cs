using System.Collections.Generic;

namespace SecretSanta.Models {
  public class IndexModel {
    public List<string> RegisteredNames { get; set; }
    public int MatchCounts { get; set; }
    public int EventId { get; set; }
  }
}
