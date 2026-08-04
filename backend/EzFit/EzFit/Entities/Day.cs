using System;
using System.Collections.Generic;

namespace EzFit.Entities
{
    public class Day
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateOnly Date { get; set; }

        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    }
}
