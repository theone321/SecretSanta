using Microsoft.AspNetCore.Http;
using SecretSanta.DataAccess;

namespace SecretSanta.Users {
  public class SessionManager : ISessionManager {
    private readonly IDataAccessor _dataAccessor;
    private DataAccess.Models.ISession _session;

    public SessionManager(IDataAccessor dataAccessor) {
      _dataAccessor = dataAccessor;
    }

    public bool TryGetSessionCookie(IRequestCookieCollection cookies, out DataAccess.Models.ISession session) {
      if (cookies.TryGetValue("sessionId", out string sessionId)) {
        _session = _dataAccessor.GetSessionData(sessionId);
        session = _session;
        return _session != null;
      }
      session = null;
      return false;
    }

    public void EndSession() {
      _session = null;
    }

    public void SetCurrentEventId(int eventId) {
      _session.EventId = eventId;
    }
  }
}
