using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Models.Event.Birthday;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Users.Birthday {
  public class BirthdayEventPageModelBuilder : IBirthdayEventPageModelBuilder {
    private readonly IDataAccessor _dataAccessor;

    public BirthdayEventPageModelBuilder(IDataAccessor dataAccessor) {
      _dataAccessor = dataAccessor;
    }

    public BirthdayEventPageModel BuildEventPageModelFromDb(string userName, int eventId) {
      var user = _dataAccessor.GetUserByUserName(userName);
      return Build(user, eventId);
    }

    public BirthdayEventPageModel BuildEventPageModelFromDb(int userId, int eventId) {
      var user = _dataAccessor.GetUserById(userId);
      return Build(user, eventId);
    }

    private BirthdayEventPageModel Build(User user, int eventId) {
      var dbEvent = _dataAccessor.GetEvent(eventId);
      var birthdayEventItems = _dataAccessor.GetItemsForEvent(dbEvent.Id);

      var giftIdeas = new List<BirthdayEventPageModel.GiftIdeaModel>();
      foreach (var giftIdea in birthdayEventItems.Where(ei => ei.IsGiftIdea)) {
        giftIdeas.Add(new BirthdayEventPageModel.GiftIdeaModel {
          Id = giftIdea.Id,
          Gift = giftIdea.ItemText,
          WillBeBrought = giftIdea.UserIdBringingItem != null,
          BroughtBy = giftIdea.UserIdBringingItem != null ? _dataAccessor.GetUserById((int)giftIdea.UserIdBringingItem).UserName : null
        });
      }

      var broughtItems = new List<BirthdayEventPageModel.BringingItemModel>();
      foreach(var broughtItem in birthdayEventItems.Where(ei => ei.IsBroughtItem)) {
        broughtItems.Add(new BirthdayEventPageModel.BringingItemModel {
          Id = broughtItem.Id,
          Item = broughtItem.ItemText,
          BroughtBy = _dataAccessor.GetUserById((int)broughtItem.UserIdBringingItem).UserName
        });
      }

      return new BirthdayEventPageModel() {
        UserId = user.Id,
        UserName = user.UserName,
        Name = user.RegisteredName,
        EventId = eventId,
        EventDate = dbEvent.StartDate,
        EventName = dbEvent.Name,
        Location = dbEvent.Location,
        EventDescription = dbEvent.Description,
        SharedId = dbEvent.SharedId,
        BirthdayPersonUserId = dbEvent.IsSurpriseEvent ? null : (int?)dbEvent.BirthdayPersonUserId,
        GiftIdeas = giftIdeas,
        ItemsBeingBrought = broughtItems
      };
    }
  }
}
