using SecretSanta.Models.Event.Birthday;

namespace SecretSanta.Users.Birthday {
  public interface IBirthdayEventPageModelBuilder {
    BirthdayEventPageModel BuildEventPageModelFromDb(int userId, int eventId);
    BirthdayEventPageModel BuildEventPageModelFromDb(string userName, int eventId);
  }
}
