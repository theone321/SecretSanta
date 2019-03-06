using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models.EventAdmin.Birthday {
  public class BirthdayEventAdminPageModel {
    public int UserId { get; set; }
    public string UserName { get; set; }
    public Guid SharedEventId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; }

    public EventSettingsModel EventSettings { get; set; }
    public List<GiftIdeaModel> GiftIdeas { get; set; }
    public List<EventAdminUserSettingsModel> UserSettings { get; set; } = new List<EventAdminUserSettingsModel>();
  }

  public class EventAdminUserSettingsModel {
    public int UserId { get; set; }
    public string Name { get; set; }
    public string UserName { get; set; }
    public bool IsAdmin { get; set; }
  }

  public class GiftIdeaModel {
    public int Id { get; set; }
    public string GiftIdeaText { get; set; }
  }

  public class EventSettingsModel {
    public bool AllowRegistration { get; set; }
    [Required(ErrorMessage = "You must fill in the Event Name.")]
    public string EventName { get; set; }
    [Required(ErrorMessage = "You must fill in the Event Description.")]
    public string EventDescription { get; set; }
    [Required(ErrorMessage = "You must fill in the Event Location.")]
    public string Location { get; set; }
    [Required(ErrorMessage = "You must fill in the Event Date.")]
    public DateTime EventDate { get; set; }
  }
}