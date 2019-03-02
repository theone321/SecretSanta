using System;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models.Event {
  public class CreateEventModel {
    [Required]
    public string EventName { get; set; }
    [Required]
    public string EventDescription { get; set; }
    [Required]
    public string Location { get; set; }
    [Required]
    public DateTime EventDate { get; set; }
    public Guid SharedId { get; set; }
    public bool AllowMatching { get; set; }
    public bool AllowRegistration { get; set; }
  }
}
