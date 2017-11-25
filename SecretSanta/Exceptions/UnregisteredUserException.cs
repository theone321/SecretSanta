using System;

namespace SecretSanta.Exceptions {
    public class UnregisteredUserException : Exception {
        public UnregisteredUserException() : base ("An unregistered user has attempted to log in.") { }
    }
}
