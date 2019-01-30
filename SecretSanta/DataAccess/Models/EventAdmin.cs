using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess.Models {
  public class EventAdmin {
    [Key]
    public int Id { get; set; }
    public int EventId { get; set; }
    public int AdminId { get; set; }
  }
}
