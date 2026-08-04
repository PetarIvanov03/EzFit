using System;
using System.Collections.Generic;

namespace EzFit.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public UserProfile? Profile { get; set; }
        public ICollection<Day> Days { get; set; } = new List<Day>();
    }
}
