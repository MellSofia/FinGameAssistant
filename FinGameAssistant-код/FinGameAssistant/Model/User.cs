using System;
using System.Collections.Generic;

namespace FinGameAssistant.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public decimal TotalBalance { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginDate { get; set; }
        public int ConsecutiveDays { get; set; } = 0;

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
        public virtual ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
    }
}