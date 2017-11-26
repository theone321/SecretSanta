using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecretSanta.Models
{
    public class AdminOptions
    {
        public string User { get; set; }

        public bool AllowRegistration { get; set; }
        public bool AllowMatching { get; set; }

    }
}
