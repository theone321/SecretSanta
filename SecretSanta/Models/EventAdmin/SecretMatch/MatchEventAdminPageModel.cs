using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models.EventAdmin.SecretMatch {
  public class MatchEventAdminPageModel {
    public int UserId { get; set; }
    public string UserName { get; set; }
    public Guid SharedEventId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; }

    public EventSettingsModel EventSettings { get; set; }
    public List<EventAdminUserSettingsModel> UserSettings { get; set; } = new List<EventAdminUserSettingsModel>();
  }

  public class EventAdminUserSettingsModel {
    /// <summary>
    /// The User's Id
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// The User's name
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The User's user name
    /// </summary>
    public string UserName { get; set; }
    /// <summary>
    /// Does this user have a match of their own?
    /// </summary>
    public bool HasMatched { get; set; }
    /// <summary>
    /// Has somebody else gotten this user as their match?
    /// </summary>
    public bool IsMatched { get; set; }
    /// <summary>
    /// Is this user an Admin?
    /// </summary>
    public bool IsAdmin { get; set; }
  }

  public class EventSettingsModel {
    public bool AllowRegistration { get; set; }
    public bool AllowMatching { get; set; }
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
