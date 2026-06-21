using FinGameAssistant.Model;
using FinGameAssistant.Data;
using FinGameAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinGameAssistant.ViewModels
{
    public class AchievementsViewModel : ViewModelBase
    {
        public event EventHandler DataChanged;

        private readonly AppDbContext _context;
        private readonly GamificationService _gamificationService;
        private readonly int _userId;

        private List<Services.AchievementInfo> _achievements;
        private User _currentUser;
        private int _currentLevel;
        private string _currentLevelName;
        private int _currentExperience;
        private int _nextLevel;
        private string _nextLevelName;
        private int _nextLevelExperience;
        private double _experiencePercentage;
        private int _unlockedAchievementsCount;

        private MainViewModel _mainViewModel;

        public AchievementsViewModel(int userId)
        {
            _userId = userId;
            _context = new AppDbContext();
            _gamificationService = new GamificationService(_context);

            LoadData();
        }

        public void SetMainViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            _mainViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.CurrentUser) ||
                    e.PropertyName == nameof(MainViewModel.CurrentUser.Level) ||
                    e.PropertyName == nameof(MainViewModel.CurrentUser.Experience))
                {
                    SyncWithMainViewModel();
                }
            };

            SyncWithMainViewModel();
        }

        private void SyncWithMainViewModel()
        {
            if (_mainViewModel?.CurrentUser == null) return;

            CurrentLevel = _mainViewModel.CurrentUser.Level;
            CurrentLevelName = _gamificationService.GetLevelName(_mainViewModel.CurrentUser.Level);
            CurrentExperience = _mainViewModel.CurrentUser.Experience;

            var (nextLevel, requiredExp) = _gamificationService.GetNextLevelInfo(CurrentLevel);
            NextLevel = nextLevel;
            NextLevelName = _gamificationService.GetLevelName(nextLevel);
            NextLevelExperience = requiredExp;

            if (NextLevelExperience > CurrentExperience)
            {
                ExperiencePercentage = (double)CurrentExperience / NextLevelExperience * 100;
                if (ExperiencePercentage > 100) ExperiencePercentage = 100;
            }
            else
            {
                ExperiencePercentage = 100;
            }
        }

        private void LoadData()
        {
            _currentUser = _context.Users.Find(_userId);
            if (_currentUser == null) return;

            Achievements = _gamificationService.GetAllAchievementsWithProgress(_userId);

            UnlockedAchievementsCount = Achievements.Count(a => a.IsUnlocked);

            if (_mainViewModel == null)
            {
                CurrentLevel = _currentUser.Level;
                CurrentLevelName = _gamificationService.GetLevelName(_currentUser.Level);
                CurrentExperience = _currentUser.Experience;

                var (nextLevel, requiredExp) = _gamificationService.GetNextLevelInfo(CurrentLevel);
                NextLevel = nextLevel;
                NextLevelName = _gamificationService.GetLevelName(nextLevel);
                NextLevelExperience = requiredExp;

                if (NextLevelExperience > CurrentExperience)
                {
                    ExperiencePercentage = (double)CurrentExperience / NextLevelExperience * 100;
                    if (ExperiencePercentage > 100) ExperiencePercentage = 100;
                }
                else
                {
                    ExperiencePercentage = 100;
                }
            }
        }

        public void ReloadAchievements()
        {
            if (_mainViewModel != null)
            {
                SyncWithMainViewModel();
            }

            Achievements = _gamificationService.GetAllAchievementsWithProgress(_userId);
            UnlockedAchievementsCount = Achievements.Count(a => a.IsUnlocked);

            OnPropertyChanged(nameof(Achievements));
            OnPropertyChanged(nameof(CurrentLevel));
            OnPropertyChanged(nameof(CurrentLevelName));
            OnPropertyChanged(nameof(CurrentExperience));
            OnPropertyChanged(nameof(NextLevel));
            OnPropertyChanged(nameof(NextLevelName));
            OnPropertyChanged(nameof(NextLevelExperience));
            OnPropertyChanged(nameof(ExperiencePercentage));
            OnPropertyChanged(nameof(UnlockedAchievementsCount));
            OnPropertyChanged(nameof(ExperienceProgress));

            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public List<Services.AchievementInfo> Achievements
        {
            get => _achievements;
            set => SetProperty(ref _achievements, value);
        }

        public int CurrentLevel
        {
            get => _currentLevel;
            set => SetProperty(ref _currentLevel, value);
        }

        public string CurrentLevelName
        {
            get => _currentLevelName;
            set => SetProperty(ref _currentLevelName, value);
        }

        public int CurrentExperience
        {
            get => _currentExperience;
            set => SetProperty(ref _currentExperience, value);
        }

        public int NextLevel
        {
            get => _nextLevel;
            set => SetProperty(ref _nextLevel, value);
        }

        public string NextLevelName
        {
            get => _nextLevelName;
            set => SetProperty(ref _nextLevelName, value);
        }

        public int NextLevelExperience
        {
            get => _nextLevelExperience;
            set => SetProperty(ref _nextLevelExperience, value);
        }

        public string ExperienceProgress => $"{CurrentExperience} / {NextLevelExperience} XP";

        public double ExperiencePercentage
        {
            get => _experiencePercentage;
            set => SetProperty(ref _experiencePercentage, value);
        }

        public int UnlockedAchievementsCount
        {
            get => _unlockedAchievementsCount;
            set => SetProperty(ref _unlockedAchievementsCount, value);
        }
    }
}