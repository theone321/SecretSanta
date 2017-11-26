using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SecretSanta.DataAccess.Models {
    public interface ISession {
        string SessionId { get; set; }
        string User { get; set; }
        DateTime TimeStamp { get; set; }
    }
    public class Session : ISession
    {
        public Session(string user) {
            User = user;
            SessionId = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }

        public string SessionId { get; set; }
        public string User { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
