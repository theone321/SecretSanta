using System;

namespace SecretSanta.DependencyWrappers {
    public interface IRandomWrapper {
        int Next(int num);
    }

    public class RandomWrapper : IRandomWrapper {
        private static Random _random;

        public RandomWrapper() {
            _random = new Random();
        }

        public int Next(int num) {
            return _random.Next(num);
        }
    }
}
