using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UP.Pages
{
    public partial class LogInPage : Page
    {
        string secretCode = "qweasd";
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
            // Создаем новое окно на весь экран
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

            // Картинка
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