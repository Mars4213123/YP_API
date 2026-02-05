using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP.Elements;
using UP.Helpers;

namespace UP.Pages
{
    public partial class LogInPage : Page
    {
        private string secretCode = "qweasd";

        public LogInPage()
        {
            InitializeComponent();
            Loaded += (s, e) => UsernameTextBox.Focus();
            UsernameTextBox.KeyDown += TextBox_KeyDown;
            PasswordBox.KeyDown += TextBox_KeyDown;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await AttemptLogin();
        }

        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await AttemptLogin();
            }
        }

        private async Task AttemptLogin()
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowMessage("Введите имя пользователя", ToastType.Info);
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Введите пароль", ToastType.Info);
                PasswordBox.Focus();
                return;
            }

            LoginButton.IsEnabled = false;
            LoginButton.Content = "Вход...";

            try
            {
                var user = await AppData.ApiService.LoginAsync(username, password);

                var success = await AppData.InitializeAfterLogin(user);
                if (success)
                {
                    ShowMessage($"Успешный вход! Добро пожаловать, {user.Username}", ToastType.Success);
                    MainWindow.mainWindow.OpenPages(new Receipts());
                }
            }
            catch (HttpRequestException httpEx)
            {
                ShowMessage($"Ошибка сети: {httpEx.Message}\n\nПроверьте:\n1. Запущен ли бекенд\n2. Блокирует ли фаервол\n3. Доступен ли localhost:7197", ToastType.Error);
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка входа: {ex.Message}", ToastType.Error);
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Войти";
            }
        }

        private void ShowMessage(string message, ToastType type)
        {
            ToastContainer.ShowToast(message, type);

            //MessageBox.Show(message, "Вход в систему",
            //              MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RegisterText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new RegIn());
        }

        private void SecretCodeBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SecretCodeBox.Text == secretCode)
                {
                    ShowScaryImage();
                }
                SecretCodeBox.Clear();
            }
        }

        private void ShowScaryImage()
        {
            Window scaryWindow = new Window
            {
                Height = 1080,
                Width = 1920,
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,
                Topmost = true,
                Left = 0,
                Top = 0
            };

            Image img = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("pack://application:,,,/Images/scary.jpg")),
                Stretch = System.Windows.Media.Stretch.UniformToFill
            };

            scaryWindow.Content = img;
            scaryWindow.Show();
        }
    }
}