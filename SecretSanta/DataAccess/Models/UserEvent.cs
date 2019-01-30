using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess.Models {
  public class UserEvent {
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
  }
}
