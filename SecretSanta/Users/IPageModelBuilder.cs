using SecretSanta.Models;

namespace SecretSanta.Users {
    public interface IPageModelBuilder {
        UserPageModel BuildUserPageModelFromDB(string userName, int eventId);
        UserPageModel BuildUserPageModelFromDB(int userId, int eventId);
    }
}
