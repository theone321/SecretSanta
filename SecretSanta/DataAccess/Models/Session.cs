using System;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess.Models {
    public interface ISession {
        string SessionId { get; set; }
        string User { get; set; }
        DateTime TimeStamp { get; set; }
    }
    public class Session : ISession {
        protected Session() { }

        public Session(string user) {
            User = user;
            SessionId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        [Key]
        public string SessionId { get; set; }
        public string User { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
