using System;

namespace FinGameAssistant.Model
{
    public class Goal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; } = 0;
        public DateTime? Deadline { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        
        public virtual User User { get; set; } = null!;

        public decimal RemainingAmount => TargetAmount - CurrentAmount;
    }
}