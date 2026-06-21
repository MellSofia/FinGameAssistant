using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinGameAssistant.Data;
using FinGameAssistant.Model;

namespace FinGameAssistant.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AppDbContext _context;
        private bool _isLoginMode = true;

        public int LoggedUserId { get; private set; }
        public string LoggedUsername { get; private set; } = "";

        public LoginWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            _context.Database.EnsureCreated();

            DataContext = this;
            UpdateUI();
        }

        public bool IsLoginMode
        {
            get => _isLoginMode;
            set
            {
                _isLoginMode = value;
                UpdateUI();
            }
        }

        public bool IsRegisterMode => !_isLoginMode;
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string ActionButtonText => IsLoginMode ? "Войти" : "Зарегистрироваться";
        public string ActionButtonColor => IsLoginMode ? "#3498DB" : "#27AE60";

        private void UpdateUI()
        {
            OnPropertyChanged(nameof(IsLoginMode));
            OnPropertyChanged(nameof(IsRegisterMode));
            OnPropertyChanged(nameof(ActionButtonText));
            OnPropertyChanged(nameof(ActionButtonColor));
        }

        private void LoginTab_Click(object sender, RoutedEventArgs e)
        {
            IsLoginMode = true;
        }

        private void RegisterTab_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Owner = this;

            if (registerWindow.ShowDialog() == true)
            {
                UsernameTextBox.Text = registerWindow.RegisteredUsername;
                IsLoginMode = true;
                PasswordBox.Password = "";
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoginMode)
                Login();
        }

        private void Login()
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите имя пользователя и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var user = _context.Users
                    .FirstOrDefault(u => u.Username == username || u.Email == username);

                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string hashedPassword = HashPassword(password);
                if (user.PasswordHash != hashedPassword)
                {
                    MessageBox.Show("Неверный пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var today = DateTime.Today;

                if (user.LastLoginDate == null)
                {
                    user.ConsecutiveDays = 1;
                }
                else if (user.LastLoginDate.Value.Date == today.AddDays(-1))
                {
                    user.ConsecutiveDays++;
                }
                else if (user.LastLoginDate.Value.Date < today.AddDays(-1))
                {
                    user.ConsecutiveDays = 1;
                }

                user.LastLoginDate = DateTime.Now;
                _context.SaveChanges();

                LoggedUserId = user.Id;
                LoggedUsername = user.Username;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var guest = _context.Users
                    .FirstOrDefault(u => u.Username == "Гость");

                if (guest == null)
                {
                    guest = new User
                    {
                        Username = "Гость",
                        Email = "guest@localhost",
                        PasswordHash = HashPassword("guest"),
                        Level = 1,
                        Experience = 0,
                        TotalBalance = 0,
                        CreatedAt = DateTime.Now,
                        LastLoginDate = DateTime.Now,
                        ConsecutiveDays = 1
                    };
                    _context.Users.Add(guest);
                    _context.SaveChanges();
                }
                else
                {
                    var today = DateTime.Today;
                    if (guest.LastLoginDate?.Date == today.AddDays(-1))
                    {
                        guest.ConsecutiveDays++;
                    }
                    else if (guest.LastLoginDate?.Date != today)
                    {
                        guest.ConsecutiveDays = 1;
                    }
                    guest.LastLoginDate = DateTime.Now;
                    _context.SaveChanges();
                }

                LoggedUserId = guest.Id;
                LoggedUsername = "Гость";
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе как гость: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}