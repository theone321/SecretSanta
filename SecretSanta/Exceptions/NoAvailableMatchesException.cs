using System;

namespace SecretSanta.Exceptions {
    public class NoAvailableMatchesException : Exception {
        public NoAvailableMatchesException() : base("There are no matches available.") { }
    }
}
