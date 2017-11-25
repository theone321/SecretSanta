using System;

namespace SecretSanta.Exceptions {
    public class InvalidCredentialsException : Exception {
        public InvalidCredentialsException() : base("The username or password entered are incorrect.") { }
    }
}
