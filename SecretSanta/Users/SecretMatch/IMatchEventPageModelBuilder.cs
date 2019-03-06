using SecretSanta.Models.Event.SecretMatch;

namespace SecretSanta.Users.SecretMatch {
    public interface IMatchEventPageModelBuilder {
        MatchEventPageModel BuildEventPageModelFromDB(string userName, int eventId);
        MatchEventPageModel BuildEventPageModelFromDB(int userId, int eventId);
    }
}
