using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using FinGameAssistant.Model;
using FinGameAssistant.ViewModels;

namespace FinGameAssistant.Views
{
    public partial class AddGoalWindow : Window
    {
        public AddGoalViewModel ViewModel { get; private set; }

        public AddGoalWindow(int userId)
        {
            InitializeComponent();
            ViewModel = new AddGoalViewModel(userId);
            DataContext = ViewModel;

            ViewModel.GoalSaved += (s, e) =>
            {
                DialogResult = true;
                Close();
            };
        }

        public AddGoalWindow(int userId, Goal goal)
        {
            InitializeComponent();
            ViewModel = new AddGoalViewModel(userId, goal);
            DataContext = ViewModel;

            ViewModel.GoalSaved += (s, e) =>
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveGoal();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}