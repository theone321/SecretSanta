using System;
using System.Collections.Generic;

namespace SecretSanta.Models.Event {
  public class ChooseEventModel {
    public List<EventModel> Events { get; set; }
  }

  public class EventModel {
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public Guid SharedId { get; set; }
    public string EventType { get; set; }
  }
}
