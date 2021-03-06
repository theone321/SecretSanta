﻿using System.Collections.Generic;
using SecretSanta.DataAccess.Models;

namespace SecretSanta.DataAccess {
    public interface IDataAccessor {
        Match GetExistingMatch(string requestor);
        IList<Match> GetAllExistingMatches();
        IList<MatchRestriction> GetMatchRestrictions(string requestor);
        void CreateMatch(string requestor, string matchedName, bool allowReroll);
        void RemoveMatch(string requestor, string matchedName);
        void CreateRestriction(string requestor, string restrictee, bool strict, bool makeReverse);
        IList<Name> GetAllPossibleNames();
        IList<Name> GetAllRegisteredNames();
        bool AccountAlreadyRegistered(string username);
        bool VerifyCredentials(string username, string password);
        void RegisterAccount(string username, string password);
        void DeRegisterAccount(string username);
        void UpdateUserPassword(string username, string newPassword);
        bool UserIsAdmin(string username);
        string GetUserInterests(string username);
        void SetUserInterests(string username, string interests);
        string GetSettingValue(string setting);
        void SetSettingValue(string setting, string value);
        IList<Setting> GetAllSettings();
        ISession GetSession(string username, string password);
        ISession GetSessionData(string sessionId);
        bool VerifySession(string username, string sessionId);
        void EndSession(string sessionId);
    }
}
