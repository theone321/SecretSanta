using System;
using System.Collections.Generic;

namespace SecretSanta.Models.Event.Birthday {
  public class BirthdayEventPageModel {
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
    public List<GiftIdeaModel> GiftIdeas { get; set; }
    // Things like food
    public List<BringingItemModel> ItemsBeingBrought { get; set; }
    public int EventId { get; set; }
    public DateTime EventDate { get; set; }
    public string EventName { get; set; }
    public string Location { get; set; }
    public string EventDescription { get; set; }
    public string SharedId { get; set; }
    public int? BirthdayPersonUserId { get; set; }

    public class GiftIdeaModel {
      public int Id { get; set; }
      public string Gift { get; set; }
      public bool WillBeBrought { get; set; }
      public string BroughtBy { get; set; }
    }

    public class BringingItemModel {
      public int Id { get; set; }
      public string Item { get; set; }
      public string BroughtBy { get; set; }
    }
  }
}
