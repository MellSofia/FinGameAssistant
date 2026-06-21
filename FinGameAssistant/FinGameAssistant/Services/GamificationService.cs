using FinGameAssistant.Model;
using FinGameAssistant.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinGameAssistant.Services
{
    public class GamificationService
    {
        private readonly AppDbContext _context;

        private readonly Dictionary<int, string> _levels = new()
        {
            { 1, "Стажер" },
            { 2, "Начинающий" },
            { 3, "Любознательный" },
            { 4, "Активный" },
            { 5, "Опытный пользователь" },
            { 6, "Финансист" },
            { 7, "Эксперт" },
            { 8, "Мастер" },
            { 9, "Профессионал" },
            { 10, "Финансовый эксперт" }
        };

        private readonly Dictionary<int, int> _levelRequirements = new()
        {
            { 1, 0 },
            { 2, 100 },
            { 3, 250 },
            { 4, 450 },
            { 5, 700 },
            { 6, 1000 },
            { 7, 1350 },
            { 8, 1750 },
            { 9, 2200 },
            { 10, 2700 }
        };

        public GamificationService(AppDbContext context)
        {
            _context = context;
        }

        public void AddExperience(int userId, int amount)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return;

            user.Experience += amount;
            CheckLevelUp(user);
            _context.SaveChanges();
        }

        private void CheckLevelUp(User user)
        {
            int newLevel = user.Level;

            foreach (var req in _levelRequirements.OrderBy(r => r.Key))
            {
                if (user.Experience >= req.Value && req.Key > newLevel)
                {
                    newLevel = req.Key;
                }
            }

            if (newLevel > user.Level)
            {
                user.Level = newLevel;
            }
        }

        public string GetLevelName(int level)
        {
            return _levels.ContainsKey(level) ? _levels[level] : "Пользователь";
        }

        public (int nextLevel, int requiredExp) GetNextLevelInfo(int currentLevel)
        {
            var nextLevel = _levelRequirements.Keys
                .Where(l => l > currentLevel)
                .OrderBy(l => l)
                .FirstOrDefault();

            if (nextLevel == 0)
            {
                return (currentLevel, _levelRequirements[currentLevel]);
            }

            return (nextLevel, _levelRequirements[nextLevel]);
        }

        private void UnlockAchievement(int userId, string name, string description, string icon, int expReward)
        {
            var alreadyUnlocked = _context.Achievements
                .Any(a => a.UserId == userId && a.AchievementName == name);

            if (alreadyUnlocked) return;

            var achievement = new Achievement
            {
                UserId = userId,
                AchievementName = name,
                Description = description,
                Icon = icon,
                ExperienceReward = expReward,
                UnlockedAt = DateTime.Now
            };

            _context.Achievements.Add(achievement);
            _context.SaveChanges();

            AddExperience(userId, expReward);
        }

        private int GetTransactionCount(int userId)
        {
            return _context.Transactions.Count(t => t.UserId == userId);
        }

        private int GetGoalsCount(int userId)
        {
            return _context.Goals.Count(g => g.UserId == userId);
        }

        private int GetCompletedGoalsCount(int userId)
        {
            return _context.Goals.Count(g => g.UserId == userId && g.IsCompleted);
        }

        private decimal GetTotalExpenses(int userId)
        {
            return _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);
        }

        public void CheckEconomistAchievement(int userId)
        {
            var userAchievements = _context.Achievements
                .Where(a => a.UserId == userId)
                .Select(a => a.AchievementName)
                .ToHashSet();

            if (!userAchievements.Contains("Экономист"))
            {
                var totalExpenses = GetTotalExpenses(userId);

                if (totalExpenses >= 1000)
                {
                    UnlockAchievement(userId, "Экономист", "Потратить более 1000 ₽", "💰", 150);
                }
            }
        }

        public void CheckTransactionAchievements(int userId)
        {
            var userAchievements = _context.Achievements
                .Where(a => a.UserId == userId)
                .Select(a => a.AchievementName)
                .ToHashSet();

            var transactionCount = _context.Transactions.Count(t => t.UserId == userId);

            if (!userAchievements.Contains("Первый шаг") && transactionCount >= 1)
            {
                UnlockAchievement(userId, "Первый шаг", "Добавить первую транзакцию", "👣", 100);
            }

            if (!userAchievements.Contains("Трудяга") && transactionCount >= 10)
            {
                UnlockAchievement(userId, "Трудяга", "Добавить 10 транзакций", "💪", 200);
            }

            CheckEconomistAchievement(userId);
        }

        public void CheckGoalAchievements(int userId)
        {
            var userAchievements = _context.Achievements
                .Where(a => a.UserId == userId)
                .Select(a => a.AchievementName)
                .ToHashSet();

            if (!userAchievements.Contains("Копилочка"))
            {
                var goalCount = _context.Goals
                    .Count(g => g.UserId == userId);

                if (goalCount >= 1)
                {
                    UnlockAchievement(userId, "Копилочка", "Создать первую цель", "🐷", 150);
                }
            }
        }

        public void CheckGoalCompletedAchievement(int userId)
        {
            var userAchievements = _context.Achievements
                .Where(a => a.UserId == userId)
                .Select(a => a.AchievementName)
                .ToHashSet();

            if (!userAchievements.Contains("Первая цель"))
            {
                var completedGoals = _context.Goals
                    .Count(g => g.UserId == userId && g.IsCompleted);

                if (completedGoals >= 1)
                {
                    UnlockAchievement(userId, "Первая цель", "Достичь цели накопления", "🎯", 250);
                }
            }
        }

        public List<AchievementInfo> GetAllAchievementsWithProgress(int userId)
        {
            var userAchievements = _context.Achievements
                .Where(a => a.UserId == userId)
                .ToDictionary(a => a.AchievementName);

            var transactionCount = GetTransactionCount(userId);
            var goalsCount = GetGoalsCount(userId);
            var completedGoalsCount = GetCompletedGoalsCount(userId);
            var totalExpenses = GetTotalExpenses(userId);

            var allAchievements = new List<AchievementInfo>
            {
                new AchievementInfo
                {
                    Name = "Первый шаг",
                    Description = "Добавить первую транзакцию",
                    Icon = "👣",
                    ExperienceReward = 100,
                    Progress = transactionCount,
                    Target = 1,
                    IsUnlocked = userAchievements.ContainsKey("Первый шаг"),
                    UnlockedAt = userAchievements.ContainsKey("Первый шаг") ? userAchievements["Первый шаг"].UnlockedAt : null
                },
                new AchievementInfo
                {
                    Name = "Трудяга",
                    Description = "Добавить 10 транзакций",
                    Icon = "💪",
                    ExperienceReward = 200,
                    Progress = transactionCount,
                    Target = 10,
                    IsUnlocked = userAchievements.ContainsKey("Трудяга"),
                    UnlockedAt = userAchievements.ContainsKey("Трудяга") ? userAchievements["Трудяга"].UnlockedAt : null
                },
                new AchievementInfo
                {
                    Name = "Копилочка",
                    Description = "Создать первую цель",
                    Icon = "🐷",
                    ExperienceReward = 150,
                    Progress = goalsCount,
                    Target = 1,
                    IsUnlocked = userAchievements.ContainsKey("Копилочка"),
                    UnlockedAt = userAchievements.ContainsKey("Копилочка") ? userAchievements["Копилочка"].UnlockedAt : null
                },
                new AchievementInfo
                {
                    Name = "Первая цель",
                    Description = "Достичь цели накопления",
                    Icon = "🎯",
                    ExperienceReward = 250,
                    Progress = completedGoalsCount,
                    Target = 1,
                    IsUnlocked = userAchievements.ContainsKey("Первая цель"),
                    UnlockedAt = userAchievements.ContainsKey("Первая цель") ? userAchievements["Первая цель"].UnlockedAt : null
                },
                new AchievementInfo
                {
                    Name = "Экономист",
                    Description = "Потратить более 1000 ₽",
                    Icon = "💰",
                    ExperienceReward = 150,
                    Progress = (int)totalExpenses,
                    Target = 1000,
                    IsUnlocked = userAchievements.ContainsKey("Экономист"),
                    UnlockedAt = userAchievements.ContainsKey("Экономист") ? userAchievements["Экономист"].UnlockedAt : null
                }
            };

            return allAchievements;
        }
    }

    public class AchievementInfo
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public int ExperienceReward { get; set; }
        public int Progress { get; set; }
        public int Target { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedAt { get; set; }
    }
}