using FinGameAssistant.Model;
using FinGameAssistant.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using FinGameAssistant.Services;

namespace FinGameAssistant.ViewModels
{
    public class TransactionsViewModel : ViewModelBase
    {
        public event EventHandler DataChanged;
        private readonly AppDbContext _context;
        private readonly int _userId;

        private List<Transaction> _allTransactions;
        private List<Transaction> _filteredTransactions;
        private FilterItem _selectedDateFilter;
        private FilterItem _selectedTypeFilter;
        private decimal _totalIncome;
        private decimal _totalExpense;

        public TransactionsViewModel(int userId)
        {
            _userId = userId;
            _context = new AppDbContext();

            DateFilters = new List<FilterItem>
            {
                new FilterItem { Id = 1, Name = "За сегодня" },
                new FilterItem { Id = 2, Name = "За неделю" },
                new FilterItem { Id = 3, Name = "За месяц" },
                new FilterItem { Id = 4, Name = "За 3 месяца" },
                new FilterItem { Id = 5, Name = "За год" },
                new FilterItem { Id = 6, Name = "Все время" }
            };

            TypeFilters = new List<FilterItem>
            {
                new FilterItem { Id = 0, Name = "Все" },
                new FilterItem { Id = 1, Name = "Доходы" },
                new FilterItem { Id = 2, Name = "Расходы" }
            };

            SelectedDateFilter = DateFilters.FirstOrDefault(f => f.Id == 3);
            SelectedTypeFilter = TypeFilters.FirstOrDefault(f => f.Id == 0);

            ApplyFiltersCommand = new RelayCommand(ApplyFilters);
            EditTransactionCommand = new RelayCommand<Transaction>(EditTransaction);
            DeleteTransactionCommand = new RelayCommand<Transaction>(DeleteTransaction);
            OpenAddTransactionCommand = new RelayCommand(OpenAddTransaction);

            LoadTransactions();
        }

        public List<FilterItem> DateFilters { get; set; }
        public List<FilterItem> TypeFilters { get; set; }

        public FilterItem SelectedDateFilter
        {
            get => _selectedDateFilter;
            set
            {
                SetProperty(ref _selectedDateFilter, value);
                ApplyFilters();
            }
        }

        public FilterItem SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set
            {
                SetProperty(ref _selectedTypeFilter, value);
                ApplyFilters();
            }
        }

        public List<Transaction> FilteredTransactions
        {
            get => _filteredTransactions;
            set => SetProperty(ref _filteredTransactions, value);
        }

        public decimal TotalIncome
        {
            get => _totalIncome;
            set => SetProperty(ref _totalIncome, value);
        }

        public decimal TotalExpense
        {
            get => _totalExpense;
            set => SetProperty(ref _totalExpense, value);
        }

        public ICommand ApplyFiltersCommand { get; }
        public ICommand EditTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }
        public ICommand OpenAddTransactionCommand { get; }


        public void ReloadTransactions()
        {
            _context.ChangeTracker.Clear();
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            try
            {
                _context.ChangeTracker.Clear();

                _allTransactions = _context.Transactions
                    .Where(t => t.UserId == _userId)
                    .OrderByDescending(t => t.Date)
                    .ToList();

                ApplyFilters();

                OnPropertyChanged(nameof(FilteredTransactions));
                OnPropertyChanged(nameof(TotalIncome));
                OnPropertyChanged(nameof(TotalExpense));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки транзакций: {ex.Message}");
            }
        }


        private void ApplyFilters()
        {
            if (_allTransactions == null) return;

            var filtered = _allTransactions.AsEnumerable();
            var now = DateTime.Now;

            if (SelectedDateFilter?.Id != 6)
            {
                DateTime startDate = SelectedDateFilter?.Id switch
                {
                    1 => now.Date,
                    2 => now.AddDays(-7).Date,
                    3 => new DateTime(now.Year, now.Month, 1),
                    4 => now.AddMonths(-3).Date,
                    5 => new DateTime(now.Year, 1, 1),
                    _ => DateTime.MinValue
                };

                filtered = filtered.Where(t => t.Date >= startDate && t.Date <= now);
            }

            filtered = SelectedTypeFilter?.Id switch
            {
                1 => filtered.Where(t => t.Type == TransactionType.Income),
                2 => filtered.Where(t => t.Type == TransactionType.Expense),
                _ => filtered
            };

            FilteredTransactions = filtered.OrderByDescending(t => t.Date).ToList();

            TotalIncome = FilteredTransactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            TotalExpense = FilteredTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            OnPropertyChanged(nameof(FilteredTransactions));
            OnPropertyChanged(nameof(TotalIncome));
            OnPropertyChanged(nameof(TotalExpense));
        }

        private void EditTransaction(Transaction transaction)
        {
            if (transaction == null) return;

            try
            {
                var editWindow = new Views.AddTransactionWindow(_userId, transaction);
                editWindow.Owner = System.Windows.Application.Current.MainWindow;

                editWindow.ViewModel.TransactionSaved += (s, e) =>
                {
                    _context.ChangeTracker.Clear();

                    _allTransactions = _context.Transactions
                        .Where(t => t.UserId == _userId)
                        .OrderByDescending(t => t.Date)
                        .ToList();

                    ApplyFilters();

                    DataChanged?.Invoke(this, EventArgs.Empty);

                    OnPropertyChanged(nameof(FilteredTransactions));
                    OnPropertyChanged(nameof(TotalIncome));
                    OnPropertyChanged(nameof(TotalExpense));
                };

                editWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при редактировании: {ex.Message}");
            }
        }

        private void DeleteTransaction(Transaction transaction)
        {
            if (transaction == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Удалить транзакцию от {transaction.Date:dd.MM.yyyy} на сумму {transaction.Amount:N2} ₽?",
                "Подтверждение удаления",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    using (var dbTransaction = _context.Database.BeginTransaction())
                    {
                        _context.Transactions.Remove(transaction);

                        var user = _context.Users.Find(_userId);
                        if (user != null)
                        {
                            if (transaction.Type == TransactionType.Income)
                                user.TotalBalance -= transaction.Amount;
                            else
                                user.TotalBalance += transaction.Amount;
                        }

                        _context.SaveChanges();
                        dbTransaction.Commit();
                    }

                    LoadTransactions();
                    DataChanged?.Invoke(this, EventArgs.Empty);

                    var gamificationService = new GamificationService(_context);
                    gamificationService.CheckTransactionAchievements(_userId);

                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }

        private void OpenAddTransaction()
        {
            try
            {
                var addWindow = new Views.AddTransactionWindow(_userId);
                addWindow.Owner = System.Windows.Application.Current.MainWindow;

                addWindow.ViewModel.TransactionSaved += (s, e) =>
                {
                    LoadTransactions();
                    DataChanged?.Invoke(this, EventArgs.Empty); 
                };

                addWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при открытии окна: {ex.Message}");
            }
        }
    }

    public class FilterItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}