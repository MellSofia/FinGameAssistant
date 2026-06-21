using FinGameAssistant.Model;
using FinGameAssistant.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using FinGameAssistant.Views;
using System.Windows;
using FinGameAssistant.Services;
using FinGameAssistant.Models;

namespace FinGameAssistant.ViewModels
{
    public class GoalsViewModel : ViewModelBase
    {
        public event EventHandler DataChanged;
        private readonly AppDbContext _context;
        private readonly int _userId;
        private GamificationService _gamificationService;

        private List<GoalItem> _activeGoals;
        private List<GoalItem> _completedGoals;
        public ICommand ContributeToGoalCommand { get; }

        public GoalsViewModel(int userId)
        {
            _userId = userId;
            _context = new AppDbContext();
            _gamificationService = new GamificationService(_context);

            OpenAddGoalCommand = new RelayCommand(OpenAddGoal);
            EditGoalCommand = new RelayCommand<GoalItem>(EditGoal);
            DeleteGoalCommand = new RelayCommand<GoalItem>(DeleteGoal);
            ContributeToGoalCommand = new RelayCommand<GoalItem>(ContributeToGoal);

            LoadGoals();
        }

        public List<GoalItem> ActiveGoals
        {
            get => _activeGoals;
            set => SetProperty(ref _activeGoals, value);
        }

        public List<GoalItem> CompletedGoals
        {
            get => _completedGoals;
            set => SetProperty(ref _completedGoals, value);
        }

        public bool HasNoActiveGoals => ActiveGoals == null || ActiveGoals.Count == 0;

        public ICommand OpenAddGoalCommand { get; }
        public ICommand EditGoalCommand { get; }
        public ICommand DeleteGoalCommand { get; }

        private void LoadGoals()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var goals = _context.Goals
                    .Where(g => g.UserId == _userId)
                    .OrderByDescending(g => g.CreatedAt)
                    .ToList();

                ActiveGoals = goals
                    .Where(g => !g.IsCompleted)
                    .Select(g => new GoalItem(g))
                    .ToList();

                CompletedGoals = goals
                    .Where(g => g.IsCompleted)
                    .Select(g => new GoalItem(g))
                    .ToList();

                OnPropertyChanged(nameof(HasNoActiveGoals));
                OnPropertyChanged(nameof(ActiveGoals));
                OnPropertyChanged(nameof(CompletedGoals));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки целей: {ex.Message}");
            }
        }

        private void OpenAddGoal()
        {
            try
            {
                var addWindow = new AddGoalWindow(_userId);
                addWindow.Owner = Application.Current.MainWindow;

                addWindow.ViewModel.GoalSaved += (s, e) =>
                {
                    LoadGoals();
                    DataChanged?.Invoke(this, EventArgs.Empty);

                    _gamificationService.CheckGoalAchievements(_userId);

                    (Application.Current.MainWindow.DataContext as MainViewModel)?.RefreshAchievements();
                };

                addWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна: {ex.Message}");
            }
        }

        private void EditGoal(GoalItem goalItem)
        {
            if (goalItem == null) return;

            try
            {
                var goal = _context.Goals.Find(goalItem.Id);
                if (goal != null)
                {
                    var editWindow = new AddGoalWindow(_userId, goal);
                    editWindow.Owner = Application.Current.MainWindow;
                    editWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    editWindow.ViewModel.GoalSaved += (s, e) =>
                    {
                        LoadGoals();
                        DataChanged?.Invoke(this, EventArgs.Empty);
                    };

                    editWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}");
            }
        }

        private void DeleteGoal(GoalItem goalItem)
        {
            if (goalItem == null) return;

            var result = MessageBox.Show(
                $"Удалить цель '{goalItem.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var goal = _context.Goals.Find(goalItem.Id);
                    if (goal != null)
                    {
                        _context.Goals.Remove(goal);
                        _context.SaveChanges();

                        LoadGoals();
                        DataChanged?.Invoke(this, EventArgs.Empty);

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }

        private void ContributeToGoal(GoalItem goalItem)
        {
            if (goalItem == null)
            {
                MessageBox.Show("Цель не найдена");
                return;
            }

            try
            {
                var contributeWindow = new ContributeToGoalWindow(_userId, goalItem.Id);
                contributeWindow.Owner = Application.Current.MainWindow;
                contributeWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                contributeWindow.ViewModel.GoalUpdated += (s, e) =>
                {
                    _context.ChangeTracker.Clear();
                    LoadGoals();
                    DataChanged?.Invoke(this, EventArgs.Empty);

                    _gamificationService.CheckGoalAchievements(_userId);
                    _gamificationService.CheckGoalCompletedAchievement(_userId);

                    (Application.Current.MainWindow.DataContext as MainViewModel)?.RefreshAchievements();
                };

                contributeWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при пополнении: {ex.Message}");
            }
        }

        public void ReloadGoals()
        {
            LoadGoals();
        }
    }
}