using SecretSanta.Models;

namespace SecretSanta.Users {
    public interface IPageModelBuilder {
        UserPageModel BuildUserPageModelFromDB(string userName);
        UserPageModel BuildUserPageModelFromDB(int userId);
    }
}
