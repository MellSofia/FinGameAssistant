using System;

namespace FinGameAssistant.Model
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; } 
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        
        public virtual User User { get; set; } = null!;
    }
}