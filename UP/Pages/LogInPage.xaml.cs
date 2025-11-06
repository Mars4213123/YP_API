using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP.Models;
using UP.Services;

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

        private async Task<string> TestApiConnection()
        {
            try
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var testUrls = new[]
                {
                    "https://localhost:7197/swagger",
                    "https://localhost:7197/api/recipes",
                    "https://localhost:7197/"
                };

                foreach (var url in testUrls)
                {
                    try
                    {
                        var response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            return $"✅ API доступен: {url}";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to connect to {url}: {ex.Message}");
                    }
                }

                return "❌ API недоступен. Убедитесь, что:\n• Бекенд запущен\n• Порт 7197 свободен\n• Приложение имеет доступ к сети";
            }
            catch (Exception ex)
            {
                return $"❌ Ошибка проверки: {ex.Message}";
            }
        }

        private async void CheckConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            var status = await TestApiConnection();
            MessageBox.Show(status, "Проверка подключения");
        }

        private async void CreateTestUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = await AppData.ApiService.RegisterAsync("asd", "asd@test.com", "asd", new List<string>());
                MessageBox.Show($"✅ Пользователь 'asd' создан!\nМожно войти с паролем 'asd'", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка создания пользователя: {ex.Message}", "Ошибка");
            }
        }

        private async System.Threading.Tasks.Task AttemptLogin()
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowMessage("Введите имя пользователя");
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Введите пароль");
                PasswordBox.Focus();
                return;
            }

            LoginButton.IsEnabled = false;
            LoginButton.Content = "Вход...";

            try
            {
                // Проверяем подключение
                var connectionStatus = await TestApiConnection();
                if (connectionStatus.Contains("❌"))
                {
                    ShowMessage(connectionStatus);
                    return;
                }

                // Пробуем войти
                var user = await AppData.ApiService.LoginAsync(username, password);
                AppData.InitializeAfterLogin(user);

                ShowMessage($"Успешный вход! Добро пожаловать, {user.Username}");
                MainWindow.mainWindow.OpenPages(new Receipts());
            }
            catch (HttpRequestException httpEx)
            {
                ShowMessage($"Ошибка сети: {httpEx.Message}\n\nПроверьте:\n1. Запущен ли бекенд\n2. Блокирует ли фаервол\n3. Доступен ли localhost:7197");
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка входа: {ex.Message}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Войти";
            }
        }

        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Вход в систему",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RegisterText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new RegIn());
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