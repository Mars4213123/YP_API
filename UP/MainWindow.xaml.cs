using System.Windows;
using System.Windows.Controls;
using UP.Pages;
using UP.Services;

namespace UP
{
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;

        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;

            // Настройка API
            AppData.ApiService = new ApiService("https://localhost:7197/api/");

            OpenPages(new LogInPage());
        }

        public void OpenPages(Page page)
        {
            frame.Navigate(page);

            if (page is FrameworkElement frameworkElement)
            {
                frameworkElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                frameworkElement.VerticalAlignment = VerticalAlignment.Stretch;
            }
        }
    }
}