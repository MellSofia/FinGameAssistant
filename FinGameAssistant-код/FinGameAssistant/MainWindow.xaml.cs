using System;
using System.Windows;
using FinGameAssistant.ViewModels;

namespace FinGameAssistant
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow(int userId, string username)
        {
            try
            {
                InitializeComponent();
                _viewModel = new MainViewModel(userId, username);
                DataContext = _viewModel;
                Loaded += MainWindow_Loaded;
                Closed += MainWindow_Closed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации окна: {ex.Message}\n\n{ex.StackTrace}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Title = "FinGame Assistant";
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _viewModel?.Cleanup();
        }
    }
}