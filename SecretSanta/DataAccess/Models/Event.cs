using System;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess.Models {
  public class Event {
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public string SharedId { get; set; }
    public int EventType { get; set; }
    public int BirthdayPersonUserId { get; set; }
    public bool IsSurpriseEvent { get; set; }
  }
}
