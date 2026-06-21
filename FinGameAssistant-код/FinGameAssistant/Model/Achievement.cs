using System;

namespace FinGameAssistant.Model
{
    public class Achievement
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string AchievementName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int ExperienceReward { get; set; } = 0;

        public DateTime UnlockedAt { get; set; } = DateTime.Now;

        public virtual User User { get; set; } = null!;
    }
}