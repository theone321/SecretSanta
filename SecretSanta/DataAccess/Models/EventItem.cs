namespace SecretSanta.DataAccess.Models {
  public class EventItem {
    public int Id { get; set; }
    public int EventId { get; set; }
    public string ItemText { get; set; }
    public bool IsGiftIdea { get; set; }
    public bool IsBroughtItem { get; set; }
    public int? UserIdBringingItem { get; set; }
  }
}
