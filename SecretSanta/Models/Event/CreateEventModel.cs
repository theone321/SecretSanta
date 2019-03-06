using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models.Event {
  public class CreateEventModel {
    [Required(ErrorMessage = "You must fill in the Event Name.")]
    public string EventName { get; set; }
    [Required(ErrorMessage = "You must fill in the Event Description.")]
    public string EventDescription { get; set; }
    [Required(ErrorMessage = "You must fill in the Event Location.")]
    public string Location { get; set; }
    [Required(ErrorMessage = "You must fill in the Event Date.")]
    public DateTime EventDate { get; set; }
    public Guid SharedId { get; set; }
    public bool AllowMatching { get; set; }
    public bool AllowRegistration { get; set; }
    public List<SelectListItem> EventTypes { get; set; }
    public string ChosenEventType { get; set; }
  }
}
