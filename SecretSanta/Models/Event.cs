using System;

namespace SecretSanta.Models {
  public class Event {
    public string EventName { get; set; }
    public string EventDescription { get; set; }
    public string Location { get; set; }
    public DateTime EventDate { get; set; }
    public Guid SharedId { get; set; }
  }
}
