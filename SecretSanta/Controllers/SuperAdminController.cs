using Microsoft.AspNetCore.Mvc;
using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Models.SuperAdmin;
using SecretSanta.Users;
using System;
using System.Collections.Generic;

namespace SecretSanta.Controllers {
  public class SuperAdminController : BaseController {

    public SuperAdminController(IDataAccessor dataAccessor, ISessionManager sessionManager) 
      : base(sessionManager, dataAccessor) {}

    public IActionResult Index() {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("LogIn", "User");
      }

      var users = _dataAccessor.GetAllUsers();
      User currentUser = null;
      var displayList = new List<SuperAdminPageModel.SuperAdminUserSettings>();
      foreach (var user in users) {
        var display = new SuperAdminPageModel.SuperAdminUserSettings {
          UserId = user.Id,
          Name = user.RegisteredName,
          UserName = user.UserName,
          IsSuperAdmin = user.IsSuperAdmin
        };

        displayList.Add(display);

        if (string.Equals(user.UserName, session.User, StringComparison.Ordinal)) {
          currentUser = user;
        }
      }

      var model = new SuperAdminPageModel {
        UserId = currentUser?.Id ?? 0,
        UserName = session.User,
        UserList = displayList
      };

      return View(model);
    }

    [HttpPost]
    public IActionResult ToggleAdminAccess(int userId) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }
      var user = _dataAccessor.GetUserById(userId);
      var userIsSuperAdmin = _dataAccessor.UserIsSuperAdmin(userId);
      _dataAccessor.SetUserSuperAdmin(userId, !userIsSuperAdmin);
      return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ResetUserPassword(int userId) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }
      _dataAccessor.UpdateUserPassword(userId, "password");
      return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeRegisterUser(int userId) {
      if (!VerifyAccess(out var session)) {
        return RedirectToAction("SignIn", "User");
      }
      _dataAccessor.DeRegisterAccount(userId);
      return RedirectToAction("Index");
    }

    private bool VerifyAccess(out ISession session) {
      var sessionExists = _sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out session);
      if (sessionExists) {
        var user = _dataAccessor.GetUserByUserName(session.User);
        return _dataAccessor.UserIsSuperAdmin(user.Id);
      }
      return false;
    }
  }
}