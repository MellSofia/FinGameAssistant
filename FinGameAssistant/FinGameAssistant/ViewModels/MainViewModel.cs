using FinGameAssistant.Model;
using FinGameAssistant.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FinGameAssistant.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly AppDbContext _context;
        private User _currentUser;
        private int _selectedTabIndex;
        private decimal _monthlyIncome;
        private decimal _monthlyExpense;
        private List<Transaction> _recentTransactions;
        private List<Goal> _activeGoals;
        private int _currentUserId;
        private string _currentUsername;

        private bool _isRefreshing = false;

        public TransactionsViewModel TransactionsViewModel { get; private set; }
        public GoalsViewModel GoalsViewModel { get; private set; }
        public AchievementsViewModel AchievementsViewModel { get; private set; }

        public ICommand OpenAddTransactionCommand { get; private set; }
        public ICommand AddTransactionCommand { get; private set; }

        public MainViewModel(int userId, string username)
        {
            try
            {
                _context = new AppDbContext();
                _context.Database.EnsureCreated();

                _currentUserId = userId;
                _currentUsername = username;

                LoadUser(_currentUserId);

                TransactionsViewModel = new TransactionsViewModel(_currentUserId);
                GoalsViewModel = new GoalsViewModel(_currentUserId);
                AchievementsViewModel = new AchievementsViewModel(_currentUserId);

                AchievementsViewModel.SetMainViewModel(this);

                if (TransactionsViewModel != null)
                    TransactionsViewModel.DataChanged += (s, e) => RefreshData();

                if (GoalsViewModel != null)
                    GoalsViewModel.DataChanged += (s, e) => RefreshData();

                if (AchievementsViewModel != null)
                    AchievementsViewModel.DataChanged += (s, e) => RefreshData();

                AddTransactionCommand = new RelayCommand(OpenAddTransactionTab);
                OpenAddTransactionCommand = new RelayCommand(OpenAddTransactionWindow);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка инициализации: {ex.Message}\n\nПроверьте подключение к MySQL",
                                              "Ошибка",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public decimal MonthlyIncome
        {
            get => _monthlyIncome;
            set => SetProperty(ref _monthlyIncome, value);
        }

        public decimal MonthlyExpense
        {
            get => _monthlyExpense;
            set => SetProperty(ref _monthlyExpense, value);
        }

        public List<Transaction> RecentTransactions
        {
            get => _recentTransactions;
            set => SetProperty(ref _recentTransactions, value);
        }

        public List<Goal> ActiveGoals
        {
            get => _activeGoals;
            set => SetProperty(ref _activeGoals, value);
        }

        private void LoadUser(int userId)
        {
            try
            {
                CurrentUser = _context.Users
                    .Include(u => u.Transactions)
                    .Include(u => u.Goals)
                    .Include(u => u.Achievements)
                    .FirstOrDefault(u => u.Id == userId);

                if (CurrentUser == null)
                {
                    CurrentUser = new User
                    {
                        Username = _currentUsername,
                        Email = $"{_currentUsername}@example.com",
                        PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password")),
                        Level = 1,
                        Experience = 0,
                        TotalBalance = 0,
                        CreatedAt = DateTime.Now,
                        LastLoginDate = DateTime.Now,
                        ConsecutiveDays = 1
                    };
                    _context.Users.Add(CurrentUser);
                    _context.SaveChanges();
                }

                LoadDashboardData();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки пользователя: {ex.Message}");
            }
        }

        private void LoadDashboardData()
        {
            if (CurrentUser == null) return;

            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            try
            {
                MonthlyIncome = _context.Transactions
                    .Where(t => t.UserId == CurrentUser.Id
                        && t.Type == TransactionType.Income
                        && t.Date >= startOfMonth
                        && t.Date <= endOfMonth)
                    .Sum(t => t.Amount);

                MonthlyExpense = _context.Transactions
                    .Where(t => t.UserId == CurrentUser.Id
                        && t.Type == TransactionType.Expense
                        && t.Date >= startOfMonth
                        && t.Date <= endOfMonth)
                    .Sum(t => t.Amount);

                RecentTransactions = _context.Transactions
                    .Where(t => t.UserId == CurrentUser.Id)
                    .OrderByDescending(t => t.Date)
                    .Take(5)
                    .ToList();

                ActiveGoals = _context.Goals
                    .Where(g => g.UserId == CurrentUser.Id && !g.IsCompleted)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void RefreshData()
        {
            if (_isRefreshing) return;

            try
            {
                _isRefreshing = true;

                _context.ChangeTracker.Clear();

                CurrentUser = _context.Users
                    .Include(u => u.Transactions)
                    .Include(u => u.Goals)
                    .Include(u => u.Achievements)
                    .FirstOrDefault(u => u.Id == _currentUserId);

                if (CurrentUser != null)
                {
                    LoadDashboardData();
                    OnPropertyChanged(nameof(CurrentUser));
                    OnPropertyChanged(nameof(CurrentUser.TotalBalance));
                    OnPropertyChanged(nameof(CurrentUser.Level));
                    OnPropertyChanged(nameof(CurrentUser.Experience));

                    TransactionsViewModel?.ReloadTransactions();
                    GoalsViewModel?.ReloadGoals();
                    AchievementsViewModel?.ReloadAchievements();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка обновления данных: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private void OpenAddTransactionTab()
        {
            SelectedTabIndex = 1;
        }

        private void OpenAddTransactionWindow()
        {
            try
            {
                var addWindow = new Views.AddTransactionWindow(_currentUserId);
                addWindow.Owner = System.Windows.Application.Current.MainWindow;

                addWindow.ViewModel.TransactionSaved += (s, e) =>
                {
                    RefreshData();
                };

                addWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при открытии окна: {ex.Message}");
            }
        }

        public void RefreshAchievements()
        {
            AchievementsViewModel?.ReloadAchievements();
        }

        public void Cleanup()
        {
            _context?.Dispose();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}