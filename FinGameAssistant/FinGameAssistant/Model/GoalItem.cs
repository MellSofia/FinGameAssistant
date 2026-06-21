using System;
using FinGameAssistant.Model;

namespace FinGameAssistant.Models
{
    public class GoalItem
    {
        private readonly Goal _goal;

        public GoalItem(Goal goal)
        {
            _goal = goal;
        }

        public int Id => _goal.Id;
        public string Name => _goal.Name;
        public decimal TargetAmount => _goal.TargetAmount;
        public decimal CurrentAmount => _goal.CurrentAmount;
        public DateTime? Deadline => _goal.Deadline;
        public bool IsCompleted => _goal.IsCompleted;
        public DateTime CreatedAt => _goal.CreatedAt;

        public int ProgressPercentage
        {
            get
            {
                if (TargetAmount <= 0) return 0;
                var percentage = (CurrentAmount / TargetAmount) * 100;
                return (int)Math.Min(percentage, 100);
            }
        }

        public string Icon
        {
            get
            {
                string name = Name?.ToLower() ?? "";

                if (name.Contains("машина") || name.Contains("авто")) return "🚗";
                if (name.Contains("квартира") || name.Contains("дом")) return "🏠";
                if (name.Contains("путешествие") || name.Contains("отпуск")) return "✈️";
                if (name.Contains("телефон") || name.Contains("айфон")) return "📱";
                if (name.Contains("ноутбук") || name.Contains("макбук")) return "💻";
                if (name.Contains("подушка") || name.Contains("резерв")) return "🛡️";

                return "🎯";
            }
        }

        public string ProgressColor
        {
            get
            {
                return ProgressPercentage switch
                {
                    >= 75 => "#27AE60",
                    >= 50 => "#F39C12",
                    >= 25 => "#E67E22",
                    _ => "#3498DB"
                };
            }
        }

        public string DeadlineColor
        {
            get
            {
                if (!Deadline.HasValue) return "#7F8C8D";

                var daysLeft = (Deadline.Value - DateTime.Now).TotalDays;

                if (daysLeft < 0) return "#E74C3C";
                if (daysLeft < 7) return "#E67E22";
                if (daysLeft < 30) return "#F39C12";

                return "#27AE60";
            }
        }
    }
}