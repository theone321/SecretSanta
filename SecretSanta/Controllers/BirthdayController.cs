using Microsoft.AspNetCore.Mvc;
using SecretSanta.DataAccess;
using SecretSanta.Models.Event.Shared;
using SecretSanta.Users;
using SecretSanta.Users.Birthday;

namespace SecretSanta.Controllers {
  public class BirthdayController : BaseController {
    private readonly IBirthdayEventPageModelBuilder _birthdayEventPageModelBuilder;

    public BirthdayController(IDataAccessor dataAccessor, ISessionManager sessionManager,
      IBirthdayEventPageModelBuilder birthdayEventPageModelBuilder)
        : base(sessionManager, dataAccessor) {
      _birthdayEventPageModelBuilder = birthdayEventPageModelBuilder;
    }

    [HttpGet]
    public IActionResult GetBirthdayEvent(int eventId) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      _sessionManager.SetCurrentEventId(eventId);

      var userModel = _birthdayEventPageModelBuilder.BuildEventPageModelFromDb(session.User, eventId);
      return View("BirthdayUserPage", userModel);
    }

    [HttpPost]
    public IActionResult ClaimGift(int userId, int giftId) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      _dataAccessor.ClaimGift(giftId, userId);

      return RedirectToAction("GetBirthdayEvent", new { eventId = session.EventId });
    }

    [HttpGet]
    public IActionResult AddAdditionalItem(int eventId) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      var userId = _dataAccessor.GetUserByUserName(session.User).Id;
      var model = new NewEventItemModel {
        UserIdBringingItem = userId,
        EventId = eventId,
        IsGiftIdea = false,
        IsBroughtItem = true,
        ControllerName = "Birthday",
        ActionName = "AddAdditionalItem",
        FromEventAdmin = false
      };

      return View("AddEventItem", model);
    }

    [HttpPost]
    public IActionResult AddAdditionalItem(NewEventItemModel model) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      _dataAccessor.AddEventItem(new DataAccess.Models.EventItem {
        EventId = model.EventId,
        UserIdBringingItem = model.UserIdBringingItem,
        IsGiftIdea = model.IsGiftIdea,
        IsBroughtItem = model.IsBroughtItem,
        ItemText = model.ItemText
      });

      return RedirectToAction("GetBirthdayEvent", new { eventId = model.EventId });
    }

    [HttpPost]
    public IActionResult RemoveAdditionalItem(int eventId, int itemId) {
      //verify access
      if (!_sessionManager.TryGetSessionCookie(HttpContext.Request.Cookies, out var session)) {
        return View("InvalidCredentials");
      }

      _dataAccessor.RemoveEventItem(itemId);

      return RedirectToAction("GetBirthdayEvent", new { eventId = eventId });
    }
  }
}
