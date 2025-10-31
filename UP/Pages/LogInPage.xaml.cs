using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UP.Pages
{
    public partial class LogInPage : Page
    {
        public LogInPage()
        {
            InitializeComponent();

            Loaded += (s, e) => UsernameTextBox.Focus();
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

            if (username == "admin" && password == "123")
            {
                // Переход на главную страницу
                MainWindow.mainWindow.OpenPages(new Receipts());
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
            NavigationService?.Navigate(new RegIn());
        }
    }
}