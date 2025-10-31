using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UP.Pages
{
    /// <summary>
    /// Логика взаимодействия для LogInPage.xaml
    /// </summary>
    public partial class LogInPage : Page
    {
        public LogInPage()
        {
            InitializeComponent();

            // Устанавливаем фокус на поле имени пользователя при загрузке
            Loaded += (s, e) => UsernameTextBox.Focus();

            // Обработка нажатия Enter для входа
            UsernameTextBox.KeyDown += TextBox_KeyDown;
            PasswordBox.KeyDown += TextBox_KeyDown;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AttemptLogin();
            }
        }

        

        private void AttemptLogin()
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // Простая валидация
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

            // Проверка учетных данных (заглушка)
            if (username == "admin" && password == "123")
            {
                ShowMessage("Вход выполнен успешно!");
                // Здесь переход на главную форму
            }
            else
            {
                ShowMessage("Неверное имя пользователя или пароль");
                PasswordBox.Clear();
                PasswordBox.Focus();
            }
        }

        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Вход в систему",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RegisterText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var registerPage = new RegIn();
            NavigationService?.Navigate(new RegIn());
        }
    }
}
