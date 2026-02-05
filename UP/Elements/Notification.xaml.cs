using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace UP.Elements
{
    /// <summary>
    /// Логика взаимодействия для Notification.xaml
    /// </summary>
    /// 
    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }
    public partial class Notification : UserControl
    {
        private readonly DispatcherTimer _timer;
        public Notification(string message, ToastType type = ToastType.Info, double durationSeconds = 3.0)
        {
            InitializeComponent();
            MessageTextBlock.Text = message;

            ToastBorder.Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30));

            Color textColor;
            switch (type)
            {
                case ToastType.Success:
                    textColor = Color.FromRgb(0x4C, 0xAF, 0x50); // #4CAF50
                    break;
                case ToastType.Error:
                    textColor = Color.FromRgb(0xF4, 0x43, 0x36); // #F44336
                    break;
                case ToastType.Warning:
                    textColor = Color.FromRgb(0xFF, 0x98, 0x00); // #FF9800
                    break;
                case ToastType.Info:
                default:
                    textColor = Colors.White; // белый текст
                    break;
            }

            MessageTextBlock.Foreground = new SolidColorBrush(textColor);

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            BeginAnimation(OpacityProperty, fadeIn);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(durationSeconds)
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _timer.Stop();
            HideWithAnimation();
        }

        private void HideWithAnimation()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (s, _) => (Parent as Panel)?.Children.Remove(this);
            BeginAnimation(OpacityProperty, fadeOut);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            _timer.Stop();
            HideWithAnimation();
        }

    }
}
