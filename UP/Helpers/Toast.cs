using System.Windows.Controls;
using UP.Elements;

namespace UP.Helpers
{

    public static class Toast
    {
        public static void ShowToast(this StackPanel container, string message, ToastType type = ToastType.Info, double durationSeconds = 3.0)
        {
            if (container.Children.Count > 3)
                return;

            var toast = new Notification(message, type ,durationSeconds);
            toast.Margin = new System.Windows.Thickness(0, 0, 0, 10);

            container.Children.Insert(0, toast);
        }
    }
}
