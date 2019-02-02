using System;
using System.Collections.Generic;

namespace SecretSanta.Models.EventAdmin {
  public class EventAdminPageModel {
    public int UserId { get; set; }
    public string UserName { get; set; }
    public Guid SharedEventId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; }

    public bool AllowRegistration { get; set; }
    public bool AllowMatching { get; set; }
    public List<EventAdminUserSettings> UserList { get; set; } = new List<EventAdminUserSettings>();
  }

  public class EventAdminUserSettings {
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
}
