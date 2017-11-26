﻿using System.ComponentModel.DataAnnotations;

namespace SecretSanta.DataAccess {
    public class MatchRestriction {
        [Key]
        public int Id { get; set; }
        public string RequestorName { get; set; }
        public string RestrictedName { get; set; }
    }
}
