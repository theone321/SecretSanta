namespace SecretSanta.Models.Event.Shared {
  public class NewEventItemModel {
    public int EventId { get; set; }
    public string ItemText { get; set; }
    public bool IsGiftIdea { get; set; }
    public bool IsBroughtItem { get; set; }
    public int UserIdBringingItem { get; set; }
    public bool FromEventAdmin { get; set; }
    public string ControllerName { get; set; }
    public string ActionName { get; set; }
  }
}
