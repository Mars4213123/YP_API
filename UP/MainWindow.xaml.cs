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

namespace UP
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;

        public Pages.LogInPage CurrentMainPage { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;

            OpenPages(new Pages.LogInPage());
        }

        public void OpenPages(Page page)
        {
            frame.Content = page;

            if (page is Pages.LogInPage mainPage)
                CurrentMainPage = mainPage;
        }
    }
}
