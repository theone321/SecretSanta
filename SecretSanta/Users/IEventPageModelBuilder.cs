using SecretSanta.Models;

namespace SecretSanta.Users {
    public interface IEventPageModelBuilder {
        EventPageModel BuildEventPageModelFromDB(string userName, int eventId);
        EventPageModel BuildEventPageModelFromDB(int userId, int eventId);
    }
}
