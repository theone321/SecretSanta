using SecretSanta.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SecretSanta.DataAccess.Models;
using SecretSanta.Constants;

namespace SecretSanta.DataAccess {
  public class DataAccessorPostgreSql : IDataAccessor {
    private readonly DomainModelPostgreSqlContext _context;

    public DataAccessorPostgreSql(DomainModelPostgreSqlContext context) {
      _context = context;
    }

    public bool AccountAlreadyRegistered(string username) {
      return _context.Users.Any(n => string.Equals(n.UserName, username, StringComparison.Ordinal));
    }

    public void RemoveMatch(int requestor, int matchedId, int eventId) {
      var matchToRemove = _context.Matches.FirstOrDefault(m => m.RequestorId == requestor && m.MatchedId == matchedId && m.EventId == eventId);
      if (matchToRemove != null) {
        _context.Matches.Remove(matchToRemove);
        _context.SaveChanges();
      }
    }

    public void CreateMatch(int requestor, int matchedId, bool allowReroll, int eventId) {
      var match = new Match() {
        RequestorId = requestor,
        MatchedId = matchedId,
        RerollAllowed = allowReroll,
        EventId = eventId
      };

      _context.Matches.Add(match);

      _context.SaveChanges();
    }

    public void CreateRestriction(int requestor, int restrictee, bool strict, bool makeReverse, int eventId) {
      var restrict = new MatchRestriction() {
        RequestorId = requestor,
        RestrictedId = restrictee,
        StrictRestriction = strict,
        EventId = eventId
      };

      _context.MatchRestrictions.Add(restrict);

      if (makeReverse) {
        var restrictReverse = new MatchRestriction() {
          RequestorId = restrictee,
          RestrictedId = requestor,
          StrictRestriction = strict,
          EventId = eventId
        };
        _context.MatchRestrictions.Add(restrictReverse);
      }

      _context.SaveChanges();
    }

    public IList<Match> GetAllExistingMatchesForEvent(int eventId) {
      return _context.Matches.Where(m => m.EventId == eventId).ToList();
    }

    public IList<User> GetAllUsersForEvent(int eventId) {
      var userEvents = _context.UserEvents.Where(ue => ue.EventId == eventId).ToList();
      return _context.Users.Where(u => userEvents.Any(ue => ue.UserId == u.Id)).ToList();
    }

    public IList<User> GetAllUsers() {
      return _context.Users.ToList();
    }

    public User GetUserById(int id) {
      return _context.Users.Find(id);
    }

    public User GetUserByUserName(string userName) {
      return _context.Users.FirstOrDefault(u => string.Equals(u.UserName, userName, StringComparison.Ordinal));
    }

    public Match GetExistingMatch(int requestor, int eventId) {
      return _context.Matches.Where(m => m.RequestorId == requestor && m.EventId == eventId).FirstOrDefault();
    }

    public IList<MatchRestriction> GetMatchRestrictions(int requestor, int eventId) {
      return _context.MatchRestrictions.Where(mr => mr.RequestorId == requestor && mr.EventId == eventId).ToList();
    }

    public int RegisterAccount(string username, string password) {
      //get the account first
      var user = _context.Users.FirstOrDefault(n => string.Equals(n.UserName, username, StringComparison.Ordinal));
      if (user != null) {
        throw new AlreadyRegisteredException();
      }
      else {
        //SHA256 hash the password
        string hashed = hashPassword(password);
        user = new User() {
          UserName = username,
          Password = hashed,
          IsSuperAdmin = false
        };

        if (!_context.Users.Any()) {
          //If there are no existing users, the first user is made a super admin to provider a way of resetting password, etc.
          user.IsSuperAdmin = true;
        }

        _context.Users.Add(user);
        _context.SaveChanges();

        return GetUserByUserName(username).Id;
      }
    }

    public void DeRegisterAccount(int id) {
      _context.Users.Remove(_context.Users.Find(id));
      //remove all matches where they are the requester or the matched
      var matches = _context.Matches.Where(m => m.RequestorId == id || m.MatchedId == id);
      if (matches.Any()) {
        _context.Matches.RemoveRange(matches);
      }
      _context.SaveChanges();
    }

    public void UpdateUserPassword(int id, string newPassword) {
      var user = GetUserById(id);
      if (user != null) {
        string hashed = hashPassword(newPassword);
        user.Password = hashed;
        _context.SaveChanges();
      }
    }

    public bool VerifyCredentials(string username, string password) {
      string dbPass = GetUserByUserName(username)?.Password;
      if (!string.IsNullOrEmpty(dbPass)) {
        string hashed = hashPassword(password);
        //compare the passwords
        return string.Equals(dbPass, hashed, StringComparison.Ordinal);
      }
      else {
        throw new UnregisteredUserException();
      }
    }

    public bool UserIsSuperAdmin(int userId) {
      return GetUserById(userId)?.IsSuperAdmin == true;
    }
    
    public void SetUserSuperAdmin(int id, bool admin) {
      var user = GetUserById(id);
      if (user != null) {
        user.IsSuperAdmin = admin;
        _context.SaveChanges();
      }
    }

    public string GetSettingValue(string setting, int eventId = 0) {
      if (setting.Equals(AdminSettings.SessionTimeout, StringComparison.InvariantCultureIgnoreCase)) {
        return _context.Settings.FirstOrDefault(s => string.Equals(s.Name, setting, StringComparison.InvariantCultureIgnoreCase))?.Value;
      }
      return _context.Settings.FirstOrDefault(s => s.EventId == eventId && string.Equals(s.Name, setting, StringComparison.Ordinal))?.Value;
    }

    public void SetSettingValue(string setting, string value, int eventId) {
      var settingObj = _context.Settings.FirstOrDefault(s => s.EventId == eventId && string.Equals(s.Name, setting, StringComparison.Ordinal));
      if (settingObj != null) {
        settingObj.Value = value;

        _context.SaveChanges();
      }
      else {
        AddSetting(setting, value, eventId);
      }
    }

    public Setting AddSetting(string settingName, string value, int eventId) {
      var setting = new Setting {
        Name = settingName,
        Value = value,
        EventId = eventId
      };

      _context.Settings.Add(setting);

      _context.SaveChanges();

      return setting;
    }

    public IList<Setting> GetAllSettingsForEvent(int eventId) {
      return _context.Settings.Where(s => s.EventId == eventId).ToList();
    }

    public string GetUserInterests(int id) {
      return GetUserById(id)?.Interests;
    }

    public void SetUserInterests(int id, string interests) {
      var name = GetUserById(id);
      if (name != null) {
        name.Interests = interests;
        _context.SaveChanges();
      }
    }

    public void SetUserRealName(int id, string name) {
      var user = GetUserById(id);
      if (user != null) {
        user.RegisteredName = name;
        _context.SaveChanges();
      }
    }

    public ISession GetSession(string username, string password) {
      if (VerifyCredentials(username, password)) {
        //kill all previous sessions for this user
        var toRemove = _context.Sessions.Where(s => string.Equals(s.User, username, StringComparison.Ordinal));
        if (toRemove.Any()) {
          _context.Sessions.RemoveRange(toRemove);
        }
        var session = new Session(username);
        _context.Sessions.Add(session);
        _context.SaveChanges();
        return session;
      }
      return null;
    }

    public bool VerifySession(string username, string sessionId) {
      bool verified = false;
      var session = _context.Sessions.FirstOrDefault(s => string.Equals(s.User, username, StringComparison.Ordinal) && string.Equals(s.SessionId, sessionId, StringComparison.Ordinal));
      if (session != null) {
        //check the timestamp, if less than the timeout then good
        var rightNow = DateTime.UtcNow;

        int.TryParse(GetSettingValue(AdminSettings.SessionTimeout), out int timeout);

        if ((rightNow - session.TimeStamp) < TimeSpan.FromMinutes(timeout)) {
          session.TimeStamp = DateTime.UtcNow;
          verified = true;
        }
        else {
          _context.Sessions.Remove(session);
        }
        _context.SaveChanges();
      }
      return verified;
    }

    public ISession GetSessionData(string sessionId) {
      var session = _context.Sessions.FirstOrDefault(s => string.Equals(s.SessionId, sessionId, StringComparison.Ordinal));
      if (session != null) {
        //check the timestamp, if less than the timeout then good
        var rightNow = DateTime.UtcNow;

        int.TryParse(GetSettingValue(AdminSettings.SessionTimeout), out int timeout);

        if ((rightNow - session.TimeStamp) < TimeSpan.FromMinutes(timeout)) {
          session.TimeStamp = DateTime.UtcNow;
        }
        else {
          _context.Sessions.Remove(session);
          session = null;
        }
        _context.SaveChanges();
        return session;
      }
      return null;
    }

    public void EndSession(string sessionId) {
      var session = _context.Sessions.FirstOrDefault(s => string.Equals(s.SessionId, sessionId, StringComparison.Ordinal));
      if (session != null) {
        _context.Sessions.Remove(session);
        _context.SaveChanges();
      }
    }

    private string hashPassword(string password) {
      byte[] bytes = Encoding.UTF8.GetBytes("santaSalt" + password);
      byte[] hashedBytes = SHA256.Create().ComputeHash(bytes);

      var hashedBuilder = new StringBuilder(256);
      foreach (byte b in hashedBytes) {
        hashedBuilder.Append(b.ToString("x2"));
      }

      return hashedBuilder.ToString();
    }

    public Event GetEvent(int id) {
      return _context.Events.FirstOrDefault(e => e.Id == id);
    }

    public Event GetEvent(string sharedEventId) {
      return _context.Events.FirstOrDefault(e => e.SharedId.Equals(sharedEventId));
    }

    public List<User> GetEventAdmins(int eventId) {
      var eventAdmins = _context.EventAdmins.Where(e => e.EventId == eventId).ToList();
      return _context.Users.Where(u => eventAdmins.Any(ea => ea.AdminId == u.Id)).ToList();
    }

    public int CreateEvent(Event newEvent, int eventOwnerUserId) {
      var e = _context.Events.Add(newEvent);
      _context.SaveChanges();
      _context.UserEvents.Add(new UserEvent {
        EventId = newEvent.Id,
        UserId = eventOwnerUserId
      });
      _context.SaveChanges();
      return newEvent.Id;
    }

    public void SetUserAdmin(int eventId, int userId, bool admin) {
      if (admin) {
        var newEventAdmin = new EventAdmin {
          EventId = eventId,
          AdminId = userId
        };
        _context.EventAdmins.Add(newEventAdmin);
        _context.SaveChanges();
      }
      else {
        var existingEventAdmin = _context.EventAdmins.FirstOrDefault(ea => ea.EventId == eventId && ea.AdminId == userId);
        if (existingEventAdmin != null) {
          _context.EventAdmins.Remove(existingEventAdmin);
          _context.SaveChanges();
        }
      }
    }

    public List<Event> GetEventsForUser(int userId) {
      var userEvents = _context.UserEvents.Where(ue => ue.UserId == userId).ToList();
      return _context.Events.Where(e => userEvents.Any(ue => ue.EventId == e.Id)).ToList();
    }

    public void AddUserToEvent(int userId, string sharedEventGuid) {
      var theEvent = _context.Events.FirstOrDefault(e => e.SharedId == sharedEventGuid);
      if (theEvent != null) {
        _context.UserEvents.Add(new UserEvent {
          EventId = theEvent.Id,
          UserId = userId
        });
        _context.SaveChanges();
      }
    }

    public void RemoveUserFromEvent(int userId, int eventId) {
      var eventUser = _context.UserEvents.FirstOrDefault(ue => ue.EventId == eventId && ue.UserId == userId);
      if (eventUser != null) {
        var userMatches = _context.Matches.Where(m => m.EventId == eventId && (m.MatchedId == userId || m.RequestorId == userId));
        foreach (var match in userMatches) {
          _context.Matches.Remove(match);
        }

        var userRestrictions = _context.MatchRestrictions.Where(r => r.EventId == eventId && (r.RequestorId == userId || r.RestrictedId == userId));
        foreach (var userRestriction in userRestrictions) {
          _context.MatchRestrictions.Remove(userRestriction);
        }

        _context.UserEvents.Remove(eventUser);
        _context.SaveChanges();
      }
    }

    public void RegenerateSharedIdForEvent(int eventId) {
      var theEvent = _context.Events.FirstOrDefault(e => e.Id == eventId);
      if (theEvent != null) {
        theEvent.SharedId = Guid.NewGuid().ToString();
        _context.Events.Update(theEvent);
        _context.SaveChanges();
      }
    }

    public void UpdateEvent(Event updatedEvent) {
      var existingEvent = _context.Events.FirstOrDefault(e => e.Id == updatedEvent.Id);
      if (existingEvent != null) {
        existingEvent.Name = updatedEvent.Name;
        existingEvent.Description = updatedEvent.Description;
        existingEvent.Location = updatedEvent.Location;
        existingEvent.StartDate = updatedEvent.StartDate;
        _context.Events.Update(existingEvent);
        _context.SaveChanges();
      }
    }

    public string GetEventTypeName(int id) {
      var eventType = _context.EventTypes.FirstOrDefault(et => et.Id == id);
      if (eventType != null) {
        return eventType.Name;
      }
      return string.Empty;
    }

    public List<EventType> GetEventTypes() {
      return _context.EventTypes.ToList();
    }

    public List<EventItem> GetItemsForEvent(int eventId) {
      return _context.EventItems.Where(ei => ei.EventId == eventId).ToList();
    }

    public void AddEventItem(EventItem theItem) {
      _context.EventItems.Add(theItem);
      _context.SaveChanges();
    }

    public void ClaimGift(int itemId, int userId) {
      var item = _context.EventItems.FirstOrDefault(ei => ei.Id == itemId);
      if (item != null) {
        item.UserIdBringingItem = userId;
        _context.SaveChanges();
      }
    }

    public void RemoveEventItem(int itemId) {
      var item = _context.EventItems.FirstOrDefault(ei => ei.Id == itemId);
      if (item != null) {
        _context.EventItems.Remove(item);
        _context.SaveChanges();
      }
    }
  }
}
