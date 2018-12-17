using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SecretSanta.DataAccess;
using SecretSanta.Users;

namespace SecretSanta.Controllers {
    public class BaseController : Controller {
        protected readonly ISessionManager _sessionManager;
        protected readonly IDataAccessor _dataAccessor;

        protected BaseController(ISessionManager sessionManager, IDataAccessor dataAccessor) {
            _sessionManager = sessionManager;
            _dataAccessor = dataAccessor;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            SetTempDataValues();
        }

        public override void OnActionExecuted(ActionExecutedContext context) {
            SetTempDataValues();
        }

        private void SetTempDataValues() {
            if (_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
                var user = _dataAccessor.GetUserByUserName(session.User);
                TempData["IsUserSignedIn"] = true;
                TempData["IsUserAdmin"] = user.IsAdmin;
            }
            else {
                TempData["IsUserSignedIn"] = false;
                TempData["IsUserAdmin"] = false;
            }
        }
    }
}