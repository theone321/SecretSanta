using Microsoft.AspNetCore.Http;

namespace SecretSanta.Users {
    public interface ISessionManager {
        bool VerifySessionCookie(IRequestCookieCollection cookies);
        DataAccess.Models.ISession GetSession();
        void EndSession();
    }
}
