using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess.Models {
  public class Setting {
    [Key]
    public string Name { get; set; }
    public string Value { get; set; }
    public int EventId { get; set; }
  }
}
