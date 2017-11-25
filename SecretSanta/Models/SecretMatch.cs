﻿using System.Collections.Generic;
using SecretSanta.DataAccess;

namespace SecretSanta.Models {
    public class SecretMatch {
        public string Name { get; set; }
        public string TheirSecretMatch { get; set; }
        public bool AllowReroll { get; set; }
    }
}
