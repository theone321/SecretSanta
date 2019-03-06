using SecretSanta.Constants;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Models.Event.SecretMatch;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Users.SecretMatch {
  public class MatchEventPageModelBuilder : IMatchEventPageModelBuilder {
    private readonly IDataAccessor _dataAccessor;

    public MatchEventPageModelBuilder(IDataAccessor dataAccessor) {
      _dataAccessor = dataAccessor;
    }

    public MatchEventPageModel BuildEventPageModelFromDB(string userName, int eventId) {
      var user = _dataAccessor.GetUserByUserName(userName);
      return Build(user, eventId);
    }

    public MatchEventPageModel BuildEventPageModelFromDB(int userId, int eventId) {
      var user = _dataAccessor.GetUserById(userId);
      return Build(user, eventId);
    }

    private MatchEventPageModel Build(User user, int eventId) {
      var existingMatch = _dataAccessor.GetExistingMatch(user.Id, eventId);
      var myInterests = user.Interests;
      var allowReroll = true;
      User theirMatch = null;
      if (existingMatch != null) {
        theirMatch = _dataAccessor.GetUserById(existingMatch.MatchedId);
        allowReroll = existingMatch.RerollAllowed;
      }
      
      bool.TryParse(_dataAccessor.GetSettingValue(AdminSettings.AllowMatching, eventId), out bool allowMatch);

      var restriction = _dataAccessor.GetMatchRestrictions(user.Id, eventId).FirstOrDefault(r => r.StrictRestriction);
      var others = new List<MatchEventPageModel.LimitedUser>();
      var sigOther = new MatchEventPageModel.LimitedUser();
      if (restriction != null) {
        var sigOtherUser = _dataAccessor.GetUserById(restriction.RestrictedId);
        if (sigOtherUser != null) {
          sigOther = new MatchEventPageModel.LimitedUser() { UserId = sigOtherUser.Id, UserRealName = sigOtherUser.RegisteredName };
        }
      }
      if (sigOther.UserId <= 0) {
        //we only need this list if their significant other isn't set
        foreach (var other in _dataAccessor.GetAllUsersForEvent(eventId)) {
          if (other.Id == user.Id) { continue; }
          others.Add(new MatchEventPageModel.LimitedUser() { UserId = other.Id, UserRealName = other.RegisteredName });
        }
      }

      var matchCount = _dataAccessor.GetAllExistingMatchesForEvent(eventId).Count;
      var registeredCount = _dataAccessor.GetAllUsersForEvent(eventId).Count;

      var dbEvent = _dataAccessor.GetEvent(eventId);

      return new MatchEventPageModel() {
        UserId = user.Id,
        UserName = user.UserName,
        Name = user.RegisteredName,
        AllowReroll = allowReroll,
        TheirSecretMatchId = theirMatch?.Id ?? -1,
        TheirSecretMatchName = theirMatch?.RegisteredName,
        Interests = myInterests,
        MatchInterests = theirMatch?.Interests,
        EventId = eventId,
        AllowMatching = allowMatch,
        EventDate = dbEvent.StartDate,
        EventName = dbEvent.Name,
        Location = dbEvent.Location,
        EventDescription = dbEvent.Description,
        SharedId = dbEvent.SharedId,
        SignificantOther = sigOther,
        OtherUsers = others,
        MatchCounts = matchCount,
        RegisteredCount = registeredCount
      };
    }
  }
}
