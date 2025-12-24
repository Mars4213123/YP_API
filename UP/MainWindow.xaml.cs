using System.Windows;
using System.Windows.Controls;
using UP.Pages;

namespace UP
{
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;

        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;

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