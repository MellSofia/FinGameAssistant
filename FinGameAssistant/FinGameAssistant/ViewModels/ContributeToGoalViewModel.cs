using FinGameAssistant.Model;
using FinGameAssistant.Data;
using System;
using System.Windows.Input;

namespace FinGameAssistant.ViewModels
{
    public class ContributeToGoalViewModel : ViewModelBase
    {
        private readonly AppDbContext _context;
        private readonly int _userId;
        private readonly int _goalId;

        private decimal _amount;
        private string _goalName;
        private decimal _currentAmount;
        private decimal _targetAmount;
        private decimal _userBalance;

        public event EventHandler GoalUpdated;

        public ContributeToGoalViewModel(int userId, int goalId)
        {
            _userId = userId;
            _goalId = goalId;
            _context = new AppDbContext();

            LoadData();

            ContributeCommand = new RelayCommand(Contribute, () => CanContribute);
        }

        private void LoadData()
        {
            var goal = _context.Goals.Find(_goalId);
            if (goal != null)
            {
                GoalName = goal.Name;
                _currentAmount = goal.CurrentAmount;
                _targetAmount = goal.TargetAmount;
            }

            var user = _context.Users.Find(_userId);
            if (user != null)
            {
                _userBalance = user.TotalBalance;
            }

            OnPropertyChanged(nameof(CurrentProgress));
            OnPropertyChanged(nameof(RemainingAmount));
            OnPropertyChanged(nameof(UserBalanceInfo));
            OnPropertyChanged(nameof(CanContribute));
        }

        public string GoalName
        {
            get => _goalName;
            set => SetProperty(ref _goalName, value);
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                SetProperty(ref _amount, value);
                OnPropertyChanged(nameof(CanContribute));
                (ContributeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string CurrentProgress
        {
            get
            {
                if (_targetAmount == 0) return "0 / 0 ₽";
                return $"{_currentAmount:N0} / {_targetAmount:N0} ₽";
            }
        }

        public decimal RemainingAmount => _targetAmount - _currentAmount;

        public string UserBalanceInfo => $"Ваш баланс: {_userBalance:N0} ₽";

        public bool CanContribute =>
            Amount > 0 &&
            Amount <= RemainingAmount &&
            Amount <= _userBalance;

        public ICommand ContributeCommand { get; }

        public void Contribute()
        {
            try
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    var goal = _context.Goals.Find(_goalId);
                    if (goal != null)
                    {
                        goal.CurrentAmount += Amount;
                        if (goal.CurrentAmount >= goal.TargetAmount)
                        {
                            goal.IsCompleted = true;
                            goal.CurrentAmount = goal.TargetAmount;
                        }
                    }

                    var user = _context.Users.Find(_userId);
                    if (user != null)
                    {
                        user.TotalBalance -= Amount;
                    }

                    var transactionRecord = new Transaction
                    {
                        UserId = _userId,
                        Amount = Amount,
                        Type = TransactionType.Expense,
                        Category = "Пополнение цели",
                        Description = $"Перевод на цель: {GoalName}",
                        Date = DateTime.Now
                    };
                    _context.Transactions.Add(transactionRecord);

                    _context.SaveChanges();
                    transaction.Commit();


                    GoalUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при пополнении: {ex.Message}",
                    "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}