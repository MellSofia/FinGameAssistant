using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using FinGameAssistant.ViewModels;

namespace FinGameAssistant.Views
{
    public partial class ContributeToGoalWindow : Window
    {
        public ContributeToGoalViewModel ViewModel { get; private set; }

        public ContributeToGoalWindow(int userId, int goalId)
        {
            InitializeComponent();
            ViewModel = new ContributeToGoalViewModel(userId, goalId);
            DataContext = ViewModel;

            ViewModel.GoalUpdated += (s, e) =>
            {
                DialogResult = true;
                Close();
            };
        }

        private void NumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9,.]$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void ContributeButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Contribute();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}