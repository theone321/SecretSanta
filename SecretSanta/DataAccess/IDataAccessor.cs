using SecretSanta.DataAccess.Models;
using System;
using System.Collections.Generic;

namespace SecretSanta.DataAccess {
  public interface IDataAccessor {
    Match GetExistingMatch(int requestor, int eventId);
    IList<Match> GetAllExistingMatchesForEvent(int eventId);
    IList<MatchRestriction> GetMatchRestrictions(int requestor, int eventId);
    void CreateMatch(int requestor, int matchedId, bool allowReroll, int eventId);
    void RemoveMatch(int requestor, int matchedId, int eventId);
    void CreateRestriction(int requestor, int restrictee, bool strict, bool makeReverse, int eventId);
    IList<User> GetAllUsers();
    User GetUserById(int id);
    User GetUserByUserName(string userName);
    bool AccountAlreadyRegistered(string username);
    bool VerifyCredentials(string username, string password);
    int RegisterAccount(string username, string password);
    void DeRegisterAccount(int id);
    void UpdateUserPassword(int id, string newPassword);
    //bool UserIsAdmin(int userId, int eventId);
    //void SetUserAdmin(int id, bool admin);
    string GetUserInterests(int id);
    void SetUserInterests(int id, string interests);
    void SetUserRealName(int id, string name);
    string GetSettingValue(string setting, int eventId = 0);
    void SetSettingValue(string setting, string value, int eventId);
    Setting AddSetting(string settingName, string value, int eventId);
    IList<Setting> GetAllSettingsForEvent(int eventId);
    ISession GetSession(string username, string password);
    ISession GetSessionData(string sessionId);
    bool VerifySession(string username, string sessionId);
    void EndSession(string sessionId);
    Event GetEvent(int id);
    Event GetEvent(Guid sharedEventId);
    List<User> GetEventAdmins(int eventId);
    int CreateEvent(Event newEvent, int eventOwnerUserId);
    void SetUserAdmin(int eventId, int userId, bool admin);
    List<Event> GetEventsForUser(int userId);
    void AddUserToEvent(int userId, Guid sharedEventGuid);
  }
}
