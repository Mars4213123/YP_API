using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UP.Pages
{
    public partial class RegIn : Page
    {
        public RegIn()
        {
            InitializeComponent();
            Loaded += (s, e) => UsernameTextBox.Focus();

            // Обработка Enter для навигации
            UsernameTextBox.KeyDown += TextBox_KeyDown;
            EmailTextBox.KeyDown += TextBox_KeyDown;
            PasswordBox.KeyDown += TextBox_KeyDown;
            ConfirmPasswordBox.KeyDown += TextBox_KeyDown;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == UsernameTextBox)
                    EmailTextBox.Focus();
                else if (sender == EmailTextBox)
                    PasswordBox.Focus();
                else if (sender == PasswordBox)
                    ConfirmPasswordBox.Focus();
                else if (sender == ConfirmPasswordBox)
                    _ = RegisterAsync(); // Вызов без await
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            await RegisterAsync();
        }

        private async Task RegisterAsync()
        {
            if (!ValidateForm())
                return;

            // Находим кнопку регистрации
            var registerButton = FindName("RegisterButton") as Button;
            if (registerButton != null)
            {
                registerButton.IsEnabled = false;
                registerButton.Content = "Регистрация...";
            }

            try
            {
                var user = await AppData.ApiService.RegisterAsync(
                    UsernameTextBox.Text.Trim(),
                    EmailTextBox.Text.Trim(),
                    PasswordBox.Password);

                if (user != null)
                {
                    var success = await AppData.InitializeAfterLogin(user);
                    if (success)
                    {
                        MessageBox.Show("Регистрация успешна! Теперь вы можете войти в систему.",
                                      "Успешная регистрация",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        MainWindow.mainWindow.OpenPages(new LogInPage());
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось зарегистрировать пользователя", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (registerButton != null)
                {
                    registerButton.IsEnabled = true;
                    registerButton.Content = "Зарегистрироваться";
                }
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Введите имя пользователя", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return false;
            }

            if (UsernameTextBox.Text.Length < 3)
            {
                MessageBox.Show("Имя пользователя должно содержать минимум 3 символа", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Введите email", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            if (!IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Введите корректный email адрес", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            if (PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordBox.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new LogInPage());
        }

        private void LoginText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPages(new LogInPage());
        }
    }
}