using SecretSanta.Constants;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Models;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Users {
  public class PageModelBuilder : IEventPageModelBuilder {
    private readonly IDataAccessor _dataAccessor;

    public PageModelBuilder(IDataAccessor dataAccessor) {
      _dataAccessor = dataAccessor;
    }

    public EventPageModel BuildEventPageModelFromDB(string userName, int eventId) {
      User user = _dataAccessor.GetUserByUserName(userName);
      return Build(user, eventId);
    }

    public EventPageModel BuildEventPageModelFromDB(int userId, int eventId) {
      User user = _dataAccessor.GetUserById(userId);
      return Build(user, eventId);
    }

    private EventPageModel Build(User user, int eventId) {
      Match existingMatch = _dataAccessor.GetExistingMatch(user.Id, eventId);
      string myInterests = user.Interests;
      bool allowReroll = true;
      User theirMatch = null;
      if (existingMatch != null) {
        theirMatch = _dataAccessor.GetUserById(existingMatch.MatchedId);
        allowReroll = existingMatch.RerollAllowed;
      }

      //bool isAdmin = _dataAccessor.GetEventAdmins(eventId).Any(u => u.Id == user.Id);
      bool.TryParse(_dataAccessor.GetSettingValue(AdminSettings.AllowMatching, eventId), out bool allowMatch);

      MatchRestriction restriction = _dataAccessor.GetMatchRestrictions(user.Id, eventId).FirstOrDefault(r => r.StrictRestriction);
      List<EventPageModel.LimitedUser> others = new List<EventPageModel.LimitedUser>();
      EventPageModel.LimitedUser sigOther = new EventPageModel.LimitedUser();
      if (restriction != null) {
        User sigOtherUser = _dataAccessor.GetUserById(restriction.RestrictedId);
        if (sigOtherUser != null) {
          sigOther = new EventPageModel.LimitedUser() { UserId = sigOtherUser.Id, UserRealName = sigOtherUser.RegisteredName };
        }
      }
      if (sigOther.UserId <= 0) {
        //we only need this list if their significant other isn't set
        foreach (User other in _dataAccessor.GetAllUsersForEvent(eventId)) {
          if (other.Id == user.Id) { continue; }
          others.Add(new EventPageModel.LimitedUser() { UserId = other.Id, UserRealName = other.RegisteredName });
        }
      }

      var dbEvent = _dataAccessor.GetEvent(eventId);

      var userEvent = new Models.Event {
        AllowMatching = allowMatch,
        EventDate = dbEvent.StartDate,
        EventName = dbEvent.Name,
        Location = dbEvent.Location,
        EventDescription = dbEvent.Description,
        SharedId = dbEvent.SharedId
      };

      return new EventPageModel() {
        UserId = user.Id,
        UserName = user.UserName,
        Name = user.RegisteredName,
        AllowReroll = allowReroll,
        TheirSecretMatchId = theirMatch?.Id ?? -1,
        TheirSecretMatchName = theirMatch?.RegisteredName,
        Interests = myInterests,
        MatchInterests = theirMatch?.Interests,
        //UserIsAdmin = isAdmin,
        EventId = eventId,
        Event = userEvent,
        SignificantOther = sigOther,
        OtherUsers = others
      };
    }
  }
}
