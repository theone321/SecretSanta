using System;

namespace SecretSanta.Exceptions {
    public class AlreadyRegisteredException : Exception {
        public AlreadyRegisteredException() : base("This user is already registered.") { }

    }
}
