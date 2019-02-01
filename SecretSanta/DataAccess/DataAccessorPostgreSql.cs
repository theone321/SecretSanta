﻿using SecretSanta.Exceptions;
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
      Match match = new Match() {
        RequestorId = requestor,
        MatchedId = matchedId,
        RerollAllowed = allowReroll,
        EventId = eventId
      };

      _context.Matches.Add(match);

      _context.SaveChanges();
    }

    public void CreateRestriction(int requestor, int restrictee, bool strict, bool makeReverse, int eventId) {
      MatchRestriction restrict = new MatchRestriction() {
        RequestorId = requestor,
        RestrictedId = restrictee,
        StrictRestriction = strict,
        EventId = eventId
      };

      _context.MatchRestrictions.Add(restrict);

      if (makeReverse) {
        MatchRestriction restrictReverse = new MatchRestriction() {
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
      User user = _context.Users.FirstOrDefault(n => string.Equals(n.UserName, username, StringComparison.Ordinal));
      if (user != null) {
        throw new AlreadyRegisteredException();
      }
      else {
        //SHA256 hash the password
        string hashed = hashPassword(password);
        user = new User() {
          UserName = username,
          Password = hashed
        };

        //if (!_context.Users.Any()) {
          //If there are no existing users, the first user is made an admin
        //  user.IsAdmin = true;
        //}
        _context.Users.Add(user);
        _context.SaveChanges();

        return GetUserByUserName(username).Id;
      }
    }

    public void DeRegisterAccount(int id) {
      _context.Users.Remove(_context.Users.Find(id));
      //remove all matches where they are the requester or the matched
      IQueryable<Match> matches = _context.Matches.Where(m => m.RequestorId == id || m.MatchedId == id);
      if (matches.Any()) {
        _context.Matches.RemoveRange(matches);
      }
      _context.SaveChanges();
    }

    public void UpdateUserPassword(int id, string newPassword) {
      User user = GetUserById(id);
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

    //public bool UserIsAdmin(int userId, int eventId) {
    //  return GetUserById(userId)?.IsAdmin == true;
    //}
    //
    //public void SetUserAdmin(int id, bool admin) {
    //  User user = GetUserById(id);
    //  if (user != null) {
    //    user.IsAdmin = admin;
    //    _context.SaveChanges();
    //  }
    //}

    public string GetSettingValue(string setting, int eventId = 0) {
      return _context.Settings.FirstOrDefault(s => s.EventId == eventId && string.Equals(s.Name, setting, StringComparison.Ordinal))?.Value;
    }

    public void SetSettingValue(string setting, string value, int eventId) {
      var settingObj = _context.Settings.FirstOrDefault(s => s.EventId == eventId && string.Equals(s.Name, setting, StringComparison.Ordinal));
      if (settingObj != null) {
        settingObj.Value = value;

        _context.SaveChanges();
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
      User name = GetUserById(id);
      if (name != null) {
        name.Interests = interests;
        _context.SaveChanges();
      }
    }

    public void SetUserRealName(int id, string name) {
      User user = GetUserById(id);
      if (user != null) {
        user.RegisteredName = name;
        _context.SaveChanges();
      }
    }

    public ISession GetSession(string username, string password) {
      if (VerifyCredentials(username, password)) {
        //kill all previous sessions for this user
        IQueryable<Session> toRemove = _context.Sessions.Where(s => string.Equals(s.User, username, StringComparison.Ordinal));
        if (toRemove.Any()) {
          _context.Sessions.RemoveRange(toRemove);
        }
        Session session = new Session(username);
        _context.Sessions.Add(session);
        _context.SaveChanges();
        return session;
      }
      return null;
    }

    public bool VerifySession(string username, string sessionId) {
      bool verified = false;
      Session session = _context.Sessions.FirstOrDefault(s => string.Equals(s.User, username, StringComparison.Ordinal) && string.Equals(s.SessionId, sessionId, StringComparison.Ordinal));
      if (session != null) {
        //check the timestamp, if less than the timeout then good
        DateTime rightNow = DateTime.UtcNow;

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
      Session session = _context.Sessions.FirstOrDefault(s => string.Equals(s.SessionId, sessionId, StringComparison.Ordinal));
      if (session != null) {
        //check the timestamp, if less than the timeout then good
        DateTime rightNow = DateTime.UtcNow;

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
      Session session = _context.Sessions.FirstOrDefault(s => string.Equals(s.SessionId, sessionId, StringComparison.Ordinal));
      if (session != null) {
        _context.Sessions.Remove(session);
        _context.SaveChanges();
      }
    }

    private string hashPassword(string password) {
      byte[] bytes = Encoding.UTF8.GetBytes("santaSalt" + password);
      byte[] hashedBytes = SHA256.Create().ComputeHash(bytes);

      StringBuilder hashedBuilder = new StringBuilder(256);
      foreach (byte b in hashedBytes) {
        hashedBuilder.Append(b.ToString("x2"));
      }

      return hashedBuilder.ToString();
    }

    public Event GetEvent(int id) {
      return _context.Events.FirstOrDefault(e => e.Id == id);
    }

    public Event GetEvent(Guid sharedEventId) {
      return _context.Events.FirstOrDefault(e => e.SharedId.Equals(sharedEventId));
    }

    public List<User> GetEventAdmins(int eventId) {
      var eventAdmins = _context.EventAdmins.Where(e => e.EventId == eventId).ToList();
      return _context.Users.Where(u => eventAdmins.Any(ea => ea.AdminId == u.Id)).ToList();
    }

    public int CreateEvent(Event newEvent, int eventOwnerUserId) {
      var e = _context.Events.Add(newEvent);
      var eventId = e.Entity.Id;
      _context.UserEvents.Add(new UserEvent {
        EventId = eventId,
        UserId = eventOwnerUserId
      });
      _context.SaveChanges();
      return eventId;
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

    public void AddUserToEvent(int userId, Guid sharedEventGuid) {
      var theEvent = _context.Events.FirstOrDefault(e => e.SharedId == sharedEventGuid);
      if (theEvent != null) {
        _context.UserEvents.Add(new UserEvent {
          EventId = theEvent.Id,
          UserId = userId
        });
        _context.SaveChanges();
      }
    }
  }
}
