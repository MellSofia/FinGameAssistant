using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using FinGameAssistant.Model;
using FinGameAssistant.ViewModels;

namespace FinGameAssistant.Views
{
    public partial class AddTransactionWindow : Window
    {
        public AddTransactionViewModel ViewModel { get; private set; }

        public AddTransactionWindow(int userId)
        {
            InitializeComponent();
            ViewModel = new AddTransactionViewModel(userId);
            DataContext = ViewModel;

            ViewModel.TransactionSaved += OnTransactionSaved;
        }

        public AddTransactionWindow(int userId, Transaction transaction)
        {
            InitializeComponent();
            ViewModel = new AddTransactionViewModel(userId, transaction);
            DataContext = ViewModel;

            ViewModel.TransactionSaved += OnTransactionSaved;
        }

        private void OnTransactionSaved(object sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9,.]$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void AmountTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AmountTextBox.Text == "0" || AmountTextBox.Text == "0.00" || AmountTextBox.Text == "0,00")
            {
                AmountTextBox.Text = "";
            }
        }

        private void AmountTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AmountTextBox.Text))
            {
                AmountTextBox.Text = "0";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveTransaction();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}