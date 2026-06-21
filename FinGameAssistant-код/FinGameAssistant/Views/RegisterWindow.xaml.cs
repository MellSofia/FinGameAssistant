using System;
using System.Windows;
using System.Windows.Controls;
using FinGameAssistant.ViewModels;

namespace FinGameAssistant.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterViewModel ViewModel { get; private set; }
        public int RegisteredUserId { get; private set; }
        public string RegisteredUsername { get; private set; } = "";

        public RegisterWindow()
        {
            InitializeComponent();
            ViewModel = new RegisterViewModel();
            DataContext = ViewModel;

            PasswordBox.PasswordChanged += (s, e) =>
            {
                ViewModel.Password = PasswordBox.Password;
            };

            ConfirmPasswordBox.PasswordChanged += (s, e) =>
            {
                ViewModel.ConfirmPassword = ConfirmPasswordBox.Password;
            };

            ViewModel.RegistrationSuccess += (s, e) =>
            {
                RegisteredUserId = ViewModel.UserId;
                RegisteredUsername = ViewModel.Username;
                DialogResult = true;
                Close();
            };
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}