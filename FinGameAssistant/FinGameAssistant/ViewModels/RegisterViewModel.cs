using FinGameAssistant.Model;
using FinGameAssistant.Data;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace FinGameAssistant.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly AppDbContext _context;

        private string _username;
        private string _email;
        private string _password;
        private string _confirmPassword;
        private bool _agreeToTerms;

        private string _usernameError;
        private string _emailError;
        private string _passwordError;
        private string _confirmPasswordError;

        public event EventHandler RegistrationSuccess;

        public RegisterViewModel()
        {
            _context = new AppDbContext();
            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
        }

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                ValidateUsername();
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                ValidateEmail();
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ValidatePassword();
                ValidateConfirmPassword();
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                ValidateConfirmPassword();
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool AgreeToTerms
        {
            get => _agreeToTerms;
            set
            {
                SetProperty(ref _agreeToTerms, value);
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string UsernameError
        {
            get => _usernameError;
            set => SetProperty(ref _usernameError, value);
        }

        public string EmailError
        {
            get => _emailError;
            set => SetProperty(ref _emailError, value);
        }

        public string PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        public string ConfirmPasswordError
        {
            get => _confirmPasswordError;
            set => SetProperty(ref _confirmPasswordError, value);
        }

        public int UserId { get; private set; }

        public ICommand RegisterCommand { get; }


        private void ValidateUsername()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = "Имя пользователя обязательно";
            }
            else if (Username.Length < 3)
            {
                UsernameError = "Имя пользователя должно быть не менее 3 символов";
            }
            else if (Username.Length > 20)
            {
                UsernameError = "Имя пользователя должно быть не более 20 символов";
            }
            else if (!Regex.IsMatch(Username, @"^[a-zA-Z0-9_]+$"))
            {
                UsernameError = "Имя пользователя может содержать только буквы, цифры и _";
            }
            else
            {
                var existingUser = _context.Users
                    .FirstOrDefault(u => u.Username == Username);

                if (existingUser != null)
                {
                    UsernameError = "Пользователь с таким именем уже существует";
                }
                else
                {
                    UsernameError = "";
                }
            }
        }

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email обязателен";
            }
            else if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                EmailError = "Введите корректный email";
            }
            else
            {
                var existingUser = _context.Users
                    .FirstOrDefault(u => u.Email == Email);

                if (existingUser != null)
                {
                    EmailError = "Пользователь с таким email уже существует";
                }
                else
                {
                    EmailError = "";
                }
            }
        }

        private void ValidatePassword()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Пароль обязателен";
            }
            else if (Password.Length < 6)
            {
                PasswordError = "Пароль должен быть не менее 6 символов";
            }
            else
            {
                PasswordError = "";
            }
        }

        private void ValidateConfirmPassword()
        {
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ConfirmPasswordError = "Подтверждение пароля обязательно";
            }
            else if (ConfirmPassword != Password)
            {
                ConfirmPasswordError = "Пароли не совпадают";
            }
            else
            {
                ConfirmPasswordError = "";
            }
        }

        private bool CanExecuteRegister()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   Password == ConfirmPassword &&
                   AgreeToTerms &&
                   string.IsNullOrEmpty(UsernameError) &&
                   string.IsNullOrEmpty(EmailError) &&
                   string.IsNullOrEmpty(PasswordError) &&
                   string.IsNullOrEmpty(ConfirmPasswordError);
        }

        private void ExecuteRegister()
        {
            try
            {
                if (_context.Users.Any(u => u.Username == Username))
                {
                    UsernameError = "Пользователь с таким именем уже существует";
                    return;
                }

                if (_context.Users.Any(u => u.Email == Email))
                {
                    EmailError = "Пользователь с таким email уже существует";
                    return;
                }

                var newUser = new User
                {
                    Username = Username,
                    Email = Email,
                    PasswordHash = HashPassword(Password),
                    Level = 1,
                    Experience = 0,
                    TotalBalance = 0,
                    CreatedAt = DateTime.Now,
                    LastLoginDate = null,
                    ConsecutiveDays = 0
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                UserId = newUser.Id;

                
                RegistrationSuccess?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при регистрации: {ex.Message}",
                    "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}