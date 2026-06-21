using FinGameAssistant.Model;
using FinGameAssistant.Data;
using FinGameAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FinGameAssistant.ViewModels
{
    public class AddTransactionViewModel : ViewModelBase
    {
        private readonly AppDbContext _context;
        private readonly int _userId;
        private readonly int? _transactionId;

        private bool _isIncome = true;
        private bool _isExpense = false;
        private decimal _amount;
        private List<CategoryItem> _categories;
        private CategoryItem _selectedCategory;
        private string _newCategory = "";
        private DateTime _transactionDate = DateTime.Now;
        private string _description = "";

        public event EventHandler TransactionSaved;

        public AddTransactionViewModel(int userId)
        {
            _userId = userId;
            _transactionId = null;
            _context = new AppDbContext();
            _categories = new List<CategoryItem>();
            LoadCategories();
            SaveCommand = new RelayCommand(SaveTransaction, CanSaveTransaction);
        }

        public AddTransactionViewModel(int userId, Transaction transaction)
        {
            _userId = userId;
            _transactionId = transaction.Id;
            _context = new AppDbContext();
            _categories = new List<CategoryItem>();

            _amount = transaction.Amount;
            _isIncome = transaction.Type == TransactionType.Income;
            _isExpense = transaction.Type == TransactionType.Expense;
            _newCategory = transaction.Category;
            _transactionDate = transaction.Date;
            _description = transaction.Description ?? "";

            LoadCategories();
            SelectedCategory = Categories.FirstOrDefault(c => c.Name == transaction.Category);
            SaveCommand = new RelayCommand(SaveTransaction, CanSaveTransaction);
        }

        public bool IsIncome
        {
            get => _isIncome;
            set
            {
                SetProperty(ref _isIncome, value);
                if (value) IsExpense = !value;
                OnPropertyChanged(nameof(ButtonColor));
                OnPropertyChanged(nameof(ButtonText));
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsExpense
        {
            get => _isExpense;
            set
            {
                SetProperty(ref _isExpense, value);
                if (value) IsIncome = !value;
                OnPropertyChanged(nameof(ButtonColor));
                OnPropertyChanged(nameof(ButtonText));
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                SetProperty(ref _amount, value);
                OnPropertyChanged(nameof(CanSave));
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public List<CategoryItem> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public CategoryItem SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                if (value != null)
                {
                    NewCategory = value.Name;
                }
                OnPropertyChanged(nameof(CanSave));
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string NewCategory
        {
            get => _newCategory;
            set
            {
                SetProperty(ref _newCategory, value);
                OnPropertyChanged(nameof(CanSave));
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public DateTime TransactionDate
        {
            get => _transactionDate;
            set => SetProperty(ref _transactionDate, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string ButtonColor => IsIncome ? "#27AE60" : "#E74C3C";
        public string ButtonText => IsIncome ? "Добавить доход" : "Добавить расход";
        public bool CanSave => Amount > 0 && !string.IsNullOrWhiteSpace(NewCategory);
        public ICommand SaveCommand { get; }

        private void LoadCategories()
        {
            Categories = new List<CategoryItem>
            {
                new CategoryItem { Name = "Зарплата", Icon = "💼", Type = TransactionType.Income },
                new CategoryItem { Name = "Фриланс", Icon = "💻", Type = TransactionType.Income },
                new CategoryItem { Name = "Подарки", Icon = "🎁", Type = TransactionType.Income },
                new CategoryItem { Name = "Инвестиции", Icon = "📈", Type = TransactionType.Income },
                new CategoryItem { Name = "Продукты", Icon = "🛒", Type = TransactionType.Expense },
                new CategoryItem { Name = "Рестораны", Icon = "🍽️", Type = TransactionType.Expense },
                new CategoryItem { Name = "Кофе", Icon = "☕", Type = TransactionType.Expense },
                new CategoryItem { Name = "Транспорт", Icon = "🚗", Type = TransactionType.Expense },
                new CategoryItem { Name = "Развлечения", Icon = "🎮", Type = TransactionType.Expense },
                new CategoryItem { Name = "Здоровье", Icon = "💊", Type = TransactionType.Expense },
                new CategoryItem { Name = "Одежда", Icon = "👕", Type = TransactionType.Expense },
                new CategoryItem { Name = "Жильё", Icon = "🏠", Type = TransactionType.Expense },
                new CategoryItem { Name = "Связь", Icon = "📱", Type = TransactionType.Expense },
                new CategoryItem { Name = "Образование", Icon = "📚", Type = TransactionType.Expense }
            };
        }

        public void SaveTransaction()
        {
            try
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (_transactionId.HasValue)
                        {
                            var existingTransaction = _context.Transactions.Find(_transactionId.Value);
                            if (existingTransaction != null)
                            {
                                var user = _context.Users.Find(_userId);
                                if (user != null)
                                {
                                    if (existingTransaction.Type == TransactionType.Income)
                                        user.TotalBalance -= existingTransaction.Amount;
                                    else
                                        user.TotalBalance += existingTransaction.Amount;

                                    existingTransaction.Amount = Amount;
                                    existingTransaction.Type = IsIncome ? TransactionType.Income : TransactionType.Expense;
                                    existingTransaction.Category = NewCategory;
                                    existingTransaction.Description = Description;
                                    existingTransaction.Date = TransactionDate;

                                    if (IsIncome)
                                        user.TotalBalance += Amount;
                                    else
                                        user.TotalBalance -= Amount;
                                }
                            }
                        }
                        else
                        {
                            var newTransaction = new Transaction
                            {
                                UserId = _userId,
                                Amount = Amount,
                                Type = IsIncome ? TransactionType.Income : TransactionType.Expense,
                                Category = NewCategory,
                                Description = Description,
                                Date = TransactionDate
                            };
                            _context.Transactions.Add(newTransaction);

                            var user = _context.Users.Find(_userId);
                            if (user != null)
                            {
                                if (IsIncome)
                                    user.TotalBalance += Amount;
                                else
                                    user.TotalBalance -= Amount;
                            }
                        }

                        _context.SaveChanges();
                        transaction.Commit();

                        var gamificationService = new GamificationService(_context);
                        gamificationService.CheckTransactionAchievements(_userId);

                        string message = _transactionId.HasValue
                            ? "Транзакция обновлена!"
                            : "Транзакция добавлена!";

                        TransactionSaved?.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                                              "Ошибка",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
        }

        private bool CanSaveTransaction() => CanSave;
    }

    public class CategoryItem
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public TransactionType Type { get; set; }
        public override string ToString() => Name;
    }
}