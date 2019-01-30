using Microsoft.AspNetCore.Http;

namespace SecretSanta.Users {
  public interface ISessionManager {
    bool TryGetSessionCookie(IRequestCookieCollection cookies, out DataAccess.Models.ISession session);
    void EndSession();
    void SetCurrentEventId(int eventId);
  }
}
