using SecretSanta.DataAccess;
using SecretSanta.DataAccess.Models;
using SecretSanta.Models;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Users {
    public class PageModelBuilder : IPageModelBuilder {
        private readonly IDataAccessor _dataAccessor;

        public PageModelBuilder(IDataAccessor dataAccessor) {
            _dataAccessor = dataAccessor;
        }

        public UserPageModel BuildUserPageModelFromDB(string userName) {
            User user = _dataAccessor.GetUserByUserName(userName);
            return Build(user);
        }

        public UserPageModel BuildUserPageModelFromDB(int userId) {
            User user = _dataAccessor.GetUserById(userId);
            return Build(user);
        }

        private UserPageModel Build(User user) {
            Match existingMatch = _dataAccessor.GetExistingMatch(user.Id);
            string myInterests = user.Interests;
            bool allowReroll = true;
            User theirMatch = null;
            if (existingMatch != null) {
                theirMatch = _dataAccessor.GetUserById(existingMatch.MatchedId);
                allowReroll = existingMatch.RerollAllowed;
            }

            bool isAdmin = _dataAccessor.UserIsAdmin(user.Id);
            bool.TryParse(_dataAccessor.GetSettingValue("AllowMatching"), out bool allowMatch);

            MatchRestriction restriction = _dataAccessor.GetMatchRestrictions(user.Id).FirstOrDefault(r => r.StrictRestriction);
            List<UserPageModel.LimitedUser> others = new List<UserPageModel.LimitedUser>();
            UserPageModel.LimitedUser sigOther = new UserPageModel.LimitedUser();
            if (restriction != null) {
                User sigOtherUser = _dataAccessor.GetUserById(restriction.RestrictedId);
                if (sigOtherUser != null) {
                    sigOther = new UserPageModel.LimitedUser() { UserId = sigOtherUser.Id, UserRealName = sigOtherUser.RegisteredName };
                }
            }
            if (sigOther.UserId <= 0) {
                //we only need this list if their significant other isn't set
                foreach (User other in _dataAccessor.GetAllUsers()) {
                    if (other.Id == user.Id) { continue; }
                    others.Add(new UserPageModel.LimitedUser() { UserId = other.Id, UserRealName = other.RegisteredName });
                }
            }


            return new UserPageModel() {
                UserId = user.Id,
                UserName = user.UserName,
                Name = user.RegisteredName,
                AllowReroll = allowReroll,
                TheirSecretMatchId = theirMatch?.Id ?? -1,
                TheirSecretMatchName = theirMatch?.RegisteredName,
                Interests = myInterests,
                MatchInterests = theirMatch?.Interests,
                UserIsAdmin = isAdmin,
                AllowMatching = allowMatch,
                SignificantOther = sigOther,
                OtherUsers = others
            };
        }
    }
}
