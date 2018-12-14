using Microsoft.AspNetCore.Http;
using SecretSanta.DataAccess;

namespace SecretSanta.Users {
    public class SessionManager : ISessionManager {
        private readonly IDataAccessor _dataAccessor;
        private DataAccess.Models.ISession _session;

        public SessionManager(IDataAccessor dataAccessor) {
            _dataAccessor = dataAccessor;
        }

        public bool VerifySessionCookie(IRequestCookieCollection cookies) {
            if (cookies.TryGetValue("sessionId", out string sessionId)) {
                _session = _dataAccessor.GetSessionData(sessionId);
                return _session != null;
            }
            return false;
        }

        public DataAccess.Models.ISession GetSession() {
            return _session;
        }

        public void EndSession() {
            _session = null;
        }
    }
}
