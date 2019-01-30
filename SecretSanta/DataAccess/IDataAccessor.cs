using SecretSanta.DataAccess.Models;
using System;
using System.Collections.Generic;

namespace SecretSanta.DataAccess {
  public interface IDataAccessor {
    Match GetExistingMatch(int requestor);
    IList<Match> GetAllExistingMatches();
    IList<MatchRestriction> GetMatchRestrictions(int requestor);
    void CreateMatch(int requestor, int matchedId, bool allowReroll);
    void RemoveMatch(int requestor, int matchedId);
    void CreateRestriction(int requestor, int restrictee, bool strict, bool makeReverse);
    IList<User> GetAllUsers();
    User GetUserById(int id);
    User GetUserByUserName(string userName);
    bool AccountAlreadyRegistered(string username);
    bool VerifyCredentials(string username, string password);
    int RegisterAccount(string username, string password);
    void DeRegisterAccount(int id);
    void UpdateUserPassword(int id, string newPassword);
    bool UserIsAdmin(int id);
    void SetUserAdmin(int id, bool admin);
    string GetUserInterests(int id);
    void SetUserInterests(int id, string interests);
    void SetUserRealName(int id, string name);
    string GetSettingValue(string setting);
    void SetSettingValue(string setting, string value);
    IList<Setting> GetAllSettings();
    ISession GetSession(string username, string password);
    ISession GetSessionData(string sessionId);
    bool VerifySession(string username, string sessionId);
    void EndSession(string sessionId);
    Event GetEvent(int id);
    List<User> GetEventAdmins(int eventId);
    int CreateEvent(Event newEvent, int eventOwnerUserId);
    void SetUserAdmin(int eventId, int userId, bool admin);
    List<Event> GetEventsForUser(int userId);
    void AddUserToEvent(int userId, Guid sharedEventGuid);
  }
}
