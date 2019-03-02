using SecretSanta.Constants;
using SecretSanta.DataAccess.Models;
using SecretSanta.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.DataAccess {
  public class DataAccessorSimulated : IDataAccessor {
    private static List<User> _users = new List<User> {
      new User { Id = 1, UserName = "userA", RegisteredName = "User A", IsSuperAdmin = true, Interests = "a", Password = "pass" },
      new User { Id = 2, UserName = "userB", RegisteredName = "User B", Interests = "b", Password = "pass" },
      new User { Id = 3, UserName = "userC", RegisteredName = "User C", IsSuperAdmin = true, Interests = "c", Password = "pass" },
      new User { Id = 4, UserName = "userD", RegisteredName = "User D", Interests = "d", Password = "pass" },
      new User { Id = 5, UserName = "userE", RegisteredName = "User E", Interests = "e" },
      new User { Id = 6, UserName = "userF", RegisteredName = "User F", Interests = "f" },
      new User { Id = 7, UserName = "userG", RegisteredName = "User G", Interests = "g" },
      new User { Id = 8, UserName = "userH", RegisteredName = "User H", Interests = "h" },
      new User { Id = 9, UserName = "userI", RegisteredName = "User I", Interests = "i" },
      new User { Id = 10, UserName = "userJ", RegisteredName = "User J", Interests = "j" },
      new User { Id = 11, UserName = "userK", RegisteredName = "User K", Interests = "k" },
      new User { Id = 12, UserName = "userL", RegisteredName = "User L", Interests = "l" }
    };

    private static List<MatchRestriction> _restrictions = new List<MatchRestriction> {
      new MatchRestriction { Id = 1, RequestorId = 1, RestrictedId = 2, StrictRestriction = true },
      new MatchRestriction { Id = 2, RequestorId = 2, RestrictedId = 1, StrictRestriction = true },
      new MatchRestriction { Id = 3, RequestorId = 3, RestrictedId = 4, StrictRestriction = true },
      new MatchRestriction { Id = 4, RequestorId = 4, RestrictedId = 3, StrictRestriction = true },
      new MatchRestriction { Id = 5, RequestorId = 5, RestrictedId = 6, StrictRestriction = true },
      new MatchRestriction { Id = 6, RequestorId = 6, RestrictedId = 5, StrictRestriction = true },
      new MatchRestriction { Id = 7, RequestorId = 7, RestrictedId = 8, StrictRestriction = true },
      new MatchRestriction { Id = 8, RequestorId = 8, RestrictedId = 7, StrictRestriction = true },
      new MatchRestriction { Id = 9, RequestorId = 9, RestrictedId = 10, StrictRestriction = true },
      new MatchRestriction { Id = 10, RequestorId = 10, RestrictedId = 9, StrictRestriction = true }
    };

    private static List<Match> _matches = new List<Match>();

    private static List<Setting> _settings = new List<Setting> {
      new Setting() { Name = AdminSettings.AllowRegistration, Value = "false", EventId = 1 },
      new Setting() { Name = AdminSettings.AllowMatching, Value = "true", EventId = 1 },
      new Setting() { Name = AdminSettings.AllowRegistration, Value = "true", EventId = 2 },
      new Setting() { Name = AdminSettings.AllowMatching, Value = "false", EventId = 2 },
      new Setting() { Name = AdminSettings.SessionTimeout, Value = "15" }, //value is in minutes
    };

    private static List<Event> _events = new List<Event> {
      new Event {
        Id = 1,
        Name = "Secret Santa",
        Description = "Annual secret santa gift exchange",
        Location = "My house",
        StartDate = new DateTime(2019, 12, 15, 19, 30, 00),
        SharedId = new Guid("0123456789ABCDEFFEDCBA9876543210")
      },
      new Event {
        Id = 2,
        Name = "Someone Else's Secret Santa",
        Description = "The other event",
        Location = "Somebody's House",
        StartDate = new DateTime(2019, 12, 17, 18, 30, 00),
        SharedId = new Guid("FEDCBA98765432100123456789ABCDEF")
      }
    };

    private static List<EventAdmin> _eventAdmins = new List<EventAdmin> {
      new EventAdmin { Id = 1, EventId = 1, AdminId = 1 },
      new EventAdmin { Id = 2, EventId = 2, AdminId = 3 }
    };

    private static List<UserEvent> _userEvents = new List<UserEvent> {
      new UserEvent { Id = 1, UserId = 1, EventId = 1 },
      new UserEvent { Id = 2, UserId = 2, EventId = 1 },
      new UserEvent { Id = 3, UserId = 3, EventId = 2 },
      new UserEvent { Id = 4, UserId = 3, EventId = 1 },
      new UserEvent { Id = 5, UserId = 4, EventId = 1 },
      new UserEvent { Id = 6, UserId = 5, EventId = 2 },
      new UserEvent { Id = 7, UserId = 5, EventId = 1 },
      new UserEvent { Id = 8, UserId = 6, EventId = 1 },
      new UserEvent { Id = 9, UserId = 7, EventId = 1 },
      new UserEvent { Id = 10, UserId = 8, EventId = 1 },
      new UserEvent { Id = 11, UserId = 9, EventId = 1 },
      new UserEvent { Id = 12, UserId = 9, EventId = 2 },
      new UserEvent { Id = 13, UserId = 10, EventId = 1 },
      new UserEvent { Id = 14, UserId = 10, EventId = 2 },
      new UserEvent { Id = 15, UserId = 11, EventId = 2 }
    };

    private static List<Session> _sessions = new List<Session>();

    public IList<User> GetAllUsersForEvent(int eventId) {
      return _users.Where(u => _userEvents.Where(ue => ue.EventId == eventId).Any(ue => ue.UserId == u.Id)).ToList();
    }

    public IList<User> GetAllUsers() {
      return _users.ToList();
    }

    public User GetUserById(int id) {
      return _users.Find(u => u.Id == id);
    }

    public User GetUserByUserName(string username) {
      return _users.Find(u => string.Equals(u.UserName, username, StringComparison.Ordinal));
    }

    public Match GetExistingMatch(int requestor, int eventId) {
      return _matches.Where(m => m.RequestorId == requestor && m.EventId == eventId)?.FirstOrDefault();
    }

    public IList<MatchRestriction> GetMatchRestrictions(int requestor, int eventId) {
      return _restrictions.Where(r => r.RequestorId == requestor && r.EventId == eventId).ToList();
    }

    public void RemoveMatch(int requestor, int matchedId, int eventId) {
      _matches.RemoveAll(m => m.RequestorId == requestor && m.MatchedId == matchedId && m.EventId == eventId);
    }

    public void CreateMatch(int requestor, int matchedId, bool allowReroll, int eventId) {
      var match = new Match() {
        Id = _matches.LastOrDefault()?.Id + 1 ?? 1,
        RequestorId = requestor,
        MatchedId = matchedId,
        RerollAllowed = allowReroll,
        EventId = eventId
      };
      _matches.Add(match);
    }

    public IList<Match> GetAllExistingMatchesForEvent(int eventId) {
      return _matches.Where(m => m.EventId == eventId).ToList();
    }

    public bool AccountAlreadyRegistered(string username) {
      return _users.Any(n => string.Equals(n.UserName, username, StringComparison.Ordinal));
    }

    public bool VerifyCredentials(string username, string password) {
      var name = _users.FirstOrDefault(n => string.Equals(n.UserName, username, StringComparison.Ordinal));
      if (name != null) {
        return string.Equals(name.Password, password, StringComparison.Ordinal);
      }
      throw new InvalidCredentialsException();
    }

    public int RegisterAccount(string username, string password) {
      var user = _users.FirstOrDefault(n => string.Equals(n.UserName, username, StringComparison.Ordinal));
      if (user != null) {
        //TODO: New Exception
        throw new Exception("This user is already registered.");
      }
      else {
        user = new User() {
          UserName = username,
          Password = password,
          IsSuperAdmin = false
        };

        _users.Add(user);

        return GetUserByUserName(username).Id;
      }
    }

    public void DeRegisterAccount(int id) {
      var user = GetUserById(id);
      _users.Remove(user);
      //remove all matches where they are the requester or the matched
      _matches.RemoveAll(m => m.RequestorId == id || m.MatchedId == id);
    }

    public void UpdateUserPassword(int id, string newPassword) {
      var user = GetUserById(id);
      if (user != null) {
        user.Password = newPassword;
      }
    }

    public void CreateRestriction(int requestor, int restrictee, bool strict, bool makeReverse, int eventId) {
      var restrict = new MatchRestriction() {
        Id = _restrictions?.LastOrDefault()?.Id + 1 ?? 1,
        RequestorId = requestor,
        RestrictedId = restrictee,
        StrictRestriction = strict,
        EventId = eventId
      };

      _restrictions.Add(restrict);

      if (makeReverse) {
        var restrictReverse = new MatchRestriction() {
          Id = _restrictions?.LastOrDefault()?.Id + 1 ?? 1,
          RequestorId = restrictee,
          RestrictedId = requestor,
          StrictRestriction = strict,
          EventId = eventId
        };
        _restrictions.Add(restrictReverse);
      }
    }

    public bool UserIsSuperAdmin(int id) {
      return GetUserById(id)?.IsSuperAdmin == true;
    }
    
    public void SetUserSuperAdmin(int id, bool admin) {
      var user = GetUserById(id);
      if (user != null) {
        user.IsSuperAdmin = admin;
      }
    }

    public string GetSettingValue(string setting, int eventId) {
      if (setting.Equals(AdminSettings.SessionTimeout, StringComparison.InvariantCultureIgnoreCase)) {
        return _settings.FirstOrDefault(s => string.Equals(s.Name, setting, StringComparison.InvariantCultureIgnoreCase))?.Value;
      }
      return _settings.FirstOrDefault(s => s.EventId == eventId && string.Equals(s.Name, setting, StringComparison.Ordinal))?.Value;
    }

    public void SetSettingValue(string setting, string value, int eventId) {
      var settingObj = _settings.FirstOrDefault(s => s.EventId == eventId && string.Equals(s.Name, setting, StringComparison.Ordinal));
      if (settingObj != null) {
        settingObj.Value = value;
      }
    }

    public Setting AddSetting(string settingName, string value, int eventId) {
      var setting = new Setting {
        Name = settingName,
        Value = value,
        EventId = eventId
      };

      _settings.Add(setting);

      return setting;
    }

    public IList<Setting> GetAllSettingsForEvent(int eventId) {
      return _settings.Where(s => s.EventId == eventId).ToList();
    }

    public string GetUserInterests(int id) {
      return GetUserById(id)?.Interests;
    }

    public void SetUserInterests(int id, string interests) {
      var user = GetUserById(id);
      if (user != null) {
        user.Interests = interests;
      }
    }

    public void SetUserRealName(int id, string name) {
      var user = GetUserById(id);
      if (user != null) {
        user.RegisteredName = name;
      }
    }

    public ISession GetSession(string username, string password) {
      if (VerifyCredentials(username, password)) {
        //kill all previous sessions for this user
        _sessions.RemoveAll(s => string.Equals(s.User, username, StringComparison.InvariantCultureIgnoreCase));
        var session = new Session(username);
        _sessions.Add(session);
        return session;
      }
      return null;
    }

    public bool VerifySession(string username, string sessionId) {
      var session = _sessions.FirstOrDefault(s => string.Equals(s.User, username, StringComparison.InvariantCultureIgnoreCase) && string.Equals(s.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
      if (session != null) {
        //check the timestamp, if less than the timeout then good
        var rightNow = DateTime.UtcNow;

        int.TryParse(GetSettingValue(AdminSettings.SessionTimeout, 0), out int timeout);

        if ((rightNow - session.TimeStamp) < TimeSpan.FromMinutes(timeout)) {
          session.TimeStamp = DateTime.UtcNow;
          return true;
        }
        else {
          _sessions.Remove(session);
        }
      }
      return false;
    }

    public ISession GetSessionData(string sessionId) {
      var session = _sessions.FirstOrDefault(s => string.Equals(s.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
      if (session != null) {
        //check the timestamp, if less than the timeout then good
        var rightNow = DateTime.UtcNow;

        int.TryParse(GetSettingValue(AdminSettings.SessionTimeout, 0), out int timeout);

        if ((rightNow - session.TimeStamp) < TimeSpan.FromMinutes(timeout)) {
          session.TimeStamp = DateTime.UtcNow;
          return session;
        }
        else {
          _sessions.Remove(session);
        }
      }
      return null;
    }

    public void EndSession(string sessionId) {
      _sessions.RemoveAll(s => string.Equals(s.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
    }

    public Event GetEvent(int id) {
      return _events.FirstOrDefault(e => e.Id == id);
    }

    public Event GetEvent(Guid sharedEventId) {
      return _events.FirstOrDefault(e => e.SharedId.Equals(sharedEventId));
    }

    public List<User> GetEventAdmins(int eventId) {
      var admins = _eventAdmins.Where(ea => ea.EventId == eventId);
      var ret = _users.Join(admins, user => user.Id, admin => admin.AdminId, (user, admin) => user).ToList();
      return ret;
    }

    public int CreateEvent(Event newEvent, int eventOwnerUserId) {
      newEvent.Id = _events?.Max(e => e.Id) + 1 ?? 1;
      _events.Add(newEvent);
      _userEvents.Add(new UserEvent {
        EventId = newEvent.Id,
        UserId = eventOwnerUserId,
      });
      return newEvent.Id;
    }

    public void SetUserAdmin(int eventId, int userId, bool admin) {
      if (admin) {
        _eventAdmins.Add(new EventAdmin {
          EventId = eventId,
          AdminId = userId
        });
      }
      else {
        var existingEventAdmin = _eventAdmins.FirstOrDefault(ea => ea.Id == userId && ea.EventId == eventId);
        if (existingEventAdmin != null) {
          _eventAdmins.Remove(existingEventAdmin);
        }
      }
    }

    public List<Event> GetEventsForUser(int userId) {
      var userEvents = _userEvents.Where(ue => ue.UserId == userId).ToList();
      return _events.Where(e => userEvents.Any(ue => e.Id == ue.EventId)).ToList();
    }

    public void AddUserToEvent(int userId, Guid sharedEventGuid) {
      var theEvent = _events.FirstOrDefault(e => e.SharedId == sharedEventGuid);
      if (theEvent != null) {
        _userEvents.Add(new UserEvent {
          EventId = theEvent.Id,
          UserId = userId
        });
      }
    }

    public void RemoveUserFromEvent(int userId, int eventId) {
      var eventUser = _userEvents.FirstOrDefault(ue => ue.EventId == eventId && ue.UserId == userId);
      if (eventUser != null) {
        var userMatches = _matches.Where(m => m.EventId == eventId && (m.MatchedId == userId || m.RequestorId == userId));
        foreach (var match in userMatches) {
          _matches.Remove(match);
        }

        var userRestrictions = _restrictions.Where(r => r.EventId == eventId && (r.RequestorId == userId || r.RestrictedId == userId));
        foreach (var userRestriction in userRestrictions) {
          _restrictions.Remove(userRestriction);
        }

        _userEvents.Remove(eventUser);
      }
    }

    public void RegenerateSharedIdForEvent(int eventId) {
      var theEvent = _events.FirstOrDefault(e => e.Id == eventId);
      if (theEvent != null) {
        theEvent.SharedId = Guid.NewGuid();
      }
    }
  }
}
