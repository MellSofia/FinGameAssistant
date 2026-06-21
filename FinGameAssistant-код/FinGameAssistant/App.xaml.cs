using System;
using System.Windows;
using FinGameAssistant.Views;

namespace FinGameAssistant
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;

                var loginWindow = new LoginWindow();
                loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                var result = loginWindow.ShowDialog();

                System.Diagnostics.Debug.WriteLine($"LoginWindow result: {result}");

                if (result == true)
                {
                    System.Diagnostics.Debug.WriteLine($"Открываем MainWindow с userId={loginWindow.LoggedUserId}, username={loginWindow.LoggedUsername}");

                    var mainWindow = new MainWindow(loginWindow.LoggedUserId, loginWindow.LoggedUsername);

                    ShutdownMode = ShutdownMode.OnMainWindowClose;
                    mainWindow.Show();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("LoginWindow закрыт без входа, завершаем приложение");
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка при запуске: {ex.Message}\n\n{ex.StackTrace}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}