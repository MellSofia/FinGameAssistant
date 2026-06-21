using FinGameAssistant.Model;
using FinGameAssistant.Data;
using System;
using System.Windows.Input;

namespace FinGameAssistant.ViewModels
{
    public class AddGoalViewModel : ViewModelBase
    {
        private readonly AppDbContext _context;
        private readonly int _userId;
        private readonly int? _goalId;

        private string _name;
        private decimal _targetAmount;
        private DateTime? _deadline;

        public event EventHandler GoalSaved;

        public AddGoalViewModel(int userId)
        {
            _userId = userId;
            _goalId = null;
            _context = new AppDbContext();

            Title = "Новая цель";
            SaveCommand = new RelayCommand(SaveGoal, CanSaveGoal);
        }

        public AddGoalViewModel(int userId, Goal goal)
        {
            _userId = userId;
            _goalId = goal.Id;
            _context = new AppDbContext();

            _name = goal.Name;
            _targetAmount = goal.TargetAmount;
            _deadline = goal.Deadline;

            Title = "Редактирование цели";
            SaveCommand = new RelayCommand(SaveGoal, CanSaveGoal);
        }

        public string Title { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                OnPropertyChanged(nameof(CanSave));
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public decimal TargetAmount
        {
            get => _targetAmount;
            set
            {
                SetProperty(ref _targetAmount, value);
                OnPropertyChanged(nameof(CanSave));
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public DateTime? Deadline
        {
            get => _deadline;
            set => SetProperty(ref _deadline, value);
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(Name) && TargetAmount > 0;

        public ICommand SaveCommand { get; }

        public void SaveGoal()
        {
            try
            {
                if (_goalId.HasValue)
                {
                    var goal = _context.Goals.Find(_goalId.Value);
                    if (goal != null)
                    {
                        goal.Name = Name;
                        goal.TargetAmount = TargetAmount;
                        goal.Deadline = Deadline;
                    }
                }
                else 
                {
                    var goal = new Goal
                    {
                        UserId = _userId,
                        Name = Name,
                        TargetAmount = TargetAmount,
                        Deadline = Deadline,
                        CurrentAmount = 0,
                        IsCompleted = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Goals.Add(goal);
                }

                _context.SaveChanges();


                GoalSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool CanSaveGoal() => CanSave;
    }
}