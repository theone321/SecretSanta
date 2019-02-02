using System;

namespace SecretSanta.Models.Event {
  public class CreateEventModel {
    public string EventName { get; set; }
    public string EventDescription { get; set; }
    public string Location { get; set; }
    public DateTime EventDate { get; set; }
    public Guid SharedId { get; set; }
    public bool AllowMatching { get; set; }
    public bool AllowRegistration { get; set; }
  }
}
