using System.Windows;
using UP.Services;

namespace UP
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppData.ApiService = new ApiService("https://localhost:7197/");
        }
    }
}