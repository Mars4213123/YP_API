using System;
using System.Text.RegularExpressions;
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

            // Обработка Enter для быстрой навигации
            UsernameTextBox.KeyDown += TextBox_KeyDown;
            EmailTextBox.KeyDown += TextBox_KeyDown;
            PasswordBox.KeyDown += TextBox_KeyDown;
            ConfirmPasswordBox.KeyDown += TextBox_KeyDown;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Переход к следующему полю или регистрации
                if (sender == UsernameTextBox)
                    EmailTextBox.Focus();
                else if (sender == EmailTextBox)
                    PasswordBox.Focus();
                else if (sender == PasswordBox)
                    ConfirmPasswordBox.Focus();
                else if (sender == ConfirmPasswordBox)
                    RegisterButton_Click(null, null);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            // Регистрация пользователя
            if (RegisterUser())
            {
               
                // Возврат на страницу входа
                NavigationService?.Navigate(new LogInPage());
            }
        }

        private bool ValidateForm()
        {
            // Проверка имени пользователя
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                ShowError("Введите имя пользователя");
                UsernameTextBox.Focus();
                return false;
            }

            if (UsernameTextBox.Text.Length < 3)
            {
                ShowError("Имя пользователя должно содержать минимум 3 символа");
                UsernameTextBox.Focus();
                return false;
            }

            // Проверка email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                ShowError("Введите email");
                EmailTextBox.Focus();
                return false;
            }

            if (!IsValidEmail(EmailTextBox.Text))
            {
                ShowError("Введите корректный email адрес");
                EmailTextBox.Focus();
                return false;
            }

            // Проверка пароля
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError("Введите пароль");
                PasswordBox.Focus();
                return false;
            }

            if (PasswordBox.Password.Length < 6)
            {
                ShowError("Пароль должен содержать минимум 6 символов");
                PasswordBox.Focus();
                return false;
            }

            // Проверка подтверждения пароля
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ShowError("Пароли не совпадают");
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

        private bool RegisterUser()
        {
            try
            {
                // Здесь должна быть реальная логика регистрации
                // Например, сохранение в базу данных, вызов API и т.д.

                var userData = new
                {
                    Username = UsernameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Password = PasswordBox.Password, // В реальном приложении пароль должен хэшироваться
                    RegistrationDate = DateTime.Now
                };

                // Заглушка для демонстрации
                // В реальном приложении здесь будет вызов сервиса регистрации

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка валидации",
                          MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Возврат на страницу входа
            NavigationService?.Navigate(new LogInPage());
        }

        private void LoginText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Переход на страницу входа
            NavigationService?.Navigate(new LogInPage());
        }
    }
}