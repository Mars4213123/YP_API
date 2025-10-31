using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Win32;
using System.Windows.Threading;

namespace UP.Pages
{
    /// <summary>
    /// Логика взаимодействия для Receipts.xaml
    /// </summary>
    public partial class Receipts : Page
    {
        public class DailyMenu
        {
            public string Day { get; set; }
            public string Meal { get; set; }
            public string Description { get; set; }
        }

        public class TimerItem
        {
            public string Name { get; set; }
            public string TimeLeft { get; set; }
            public DispatcherTimer Timer { get; set; }
            public DateTime EndTime { get; set; }
        }

        private ObservableCollection<string> _products;
        private ObservableCollection<DailyMenu> _weeklyMenu;
        private ObservableCollection<TimerItem> _activeTimers;
        private ObservableCollection<string> _shoppingList;

        public Receipts()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            // Инициализация коллекций
            _products = new ObservableCollection<string>();
            _weeklyMenu = new ObservableCollection<DailyMenu>();
            _activeTimers = new ObservableCollection<TimerItem>();
            _shoppingList = new ObservableCollection<string>();

            ProductsListView.ItemsSource = _products;
            WeeklyMenuItemsControl.ItemsSource = _weeklyMenu;
            ActiveTimersItemsControl.ItemsSource = _activeTimers;
            ShoppingListListView.ItemsSource = _shoppingList;

            // Обновляем видимость текста "Нет таймеров"
            UpdateNoTimersVisibility();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewProductTextBox.Text))
            {
                _products.Add(NewProductTextBox.Text.Trim());
                NewProductTextBox.Clear();
            }
        }

        private void RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is string product)
            {
                _products.Remove(product);
            }
        }

        private void GenerateMenu_Click(object sender, RoutedEventArgs e)
        {
            // Очищаем предыдущее меню
            _weeklyMenu.Clear();
            _shoppingList.Clear();

            // Пример генерации меню на неделю
            var sampleMenu = new List<DailyMenu>
            {
                new DailyMenu { Day = "Понедельник", Meal = "Завтрак", Description = "Овсяная каша с фруктами" },
                new DailyMenu { Day = "Понедельник", Meal = "Обед", Description = "Куриный суп с овощами" },
                new DailyMenu { Day = "Понедельник", Meal = "Ужин", Description = "Запеченная рыба с картофелем" },

                new DailyMenu { Day = "Вторник", Meal = "Завтрак", Description = "Творог с ягодами" },
                new DailyMenu { Day = "Вторник", Meal = "Обед", Description = "Гречневая каша с грибами" },
                new DailyMenu { Day = "Вторник", Meal = "Ужин", Description = "Овощное рагу" },

                new DailyMenu { Day = "Среда", Meal = "Завтрак", Description = "Омлет с овощами" },
                new DailyMenu { Day = "Среда", Meal = "Обед", Description = "Паста с томатным соусом" },
                new DailyMenu { Day = "Среда", Meal = "Ужин", Description = "Куриные котлеты с салатом" },

                new DailyMenu { Day = "Четверг", Meal = "Завтрак", Description = "Смузи из банана и ягод" },
                new DailyMenu { Day = "Четверг", Meal = "Обед", Description = "Овощной суп-пюре" },
                new DailyMenu { Day = "Четверг", Meal = "Ужин", Description = "Запеченная курица с брокколи" },

                new DailyMenu { Day = "Пятница", Meal = "Завтрак", Description = "Йогурт с гранолой" },
                new DailyMenu { Day = "Пятница", Meal = "Обед", Description = "Рис с овощами" },
                new DailyMenu { Day = "Пятница", Meal = "Ужин", Description = "Лосось на гриле с салатом" },

                new DailyMenu { Day = "Суббота", Meal = "Завтрак", Description = "Блины с медом" },
                new DailyMenu { Day = "Суббота", Meal = "Обед", Description = "Пицца домашняя" },
                new DailyMenu { Day = "Суббота", Meal = "Ужин", Description = "Стейк с овощами" },

                new DailyMenu { Day = "Воскресенье", Meal = "Завтрак", Description = "Яичница с беконом" },
                new DailyMenu { Day = "Воскресенье", Meal = "Обед", Description = "Плов с курицей" },
                new DailyMenu { Day = "Воскресенье", Meal = "Ужин", Description = "Салат Цезарь с курицей" }
            };

            foreach (var menu in sampleMenu)
            {
                _weeklyMenu.Add(menu);
            }

            // Генерируем примерный список покупок
            var sampleShoppingList = new List<string>
            {
                "Куриное филе - 500г",
                "Лосось - 300г",
                "Овощи для супа",
                "Фрукты для смузи",
                "Молоко - 1л",
                "Яйца - 10шт",
                "Овсяные хлопья",
                "Рис - 400г",
                "Макароны - 400г",
                "Сыр - 200г"
            };

            foreach (var item in sampleShoppingList)
            {
                _shoppingList.Add(item);
            }

        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DailyMenu menu)
            {
                var timerWindow = new TimerWindow(menu.Meal);
                if (timerWindow.ShowDialog() == true)
                {
                    var timerItem = new TimerItem
                    {
                        Name = menu.Meal,
                        EndTime = DateTime.Now.AddMinutes(timerWindow.SelectedMinutes),
                        Timer = new DispatcherTimer()
                    };

                    timerItem.Timer.Interval = TimeSpan.FromSeconds(1);
                    timerItem.Timer.Tick += (s, args) => UpdateTimer(timerItem);
                    timerItem.Timer.Start();

                    _activeTimers.Add(timerItem);
                    UpdateNoTimersVisibility();
                    UpdateTimer(timerItem); // Первоначальное обновление
                }
            }
        }

        private void UpdateTimer(TimerItem timerItem)
        {
            var timeLeft = timerItem.EndTime - DateTime.Now;

            if (timeLeft.TotalSeconds <= 0)
            {
                timerItem.Timer.Stop();
                timerItem.TimeLeft = "Время вышло!";
                MessageBox.Show($"Таймер '{timerItem.Name}' завершил работу!", "Таймер",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                timerItem.TimeLeft = $"{timeLeft:mm\\:ss}";
            }

            // Обновляем отображение
            var index = _activeTimers.IndexOf(timerItem);
            if (index >= 0)
            {
                _activeTimers[index] = timerItem;
            }
        }

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TimerItem timerItem)
            {
                timerItem.Timer.Stop();
                _activeTimers.Remove(timerItem);
                UpdateNoTimersVisibility();
            }
        }

        private void UpdateNoTimersVisibility()
        {
            NoTimersText.Visibility = _activeTimers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShoppingItem_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                // Можно добавить логику для отмеченных items
            }
        }

        private void ExportShoppingList_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                FileName = "Список_покупок.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var lines = new List<string> { "Список покупок:", "==================" };
                    foreach (var item in _shoppingList)
                    {
                        lines.Add($"- {item}");
                    }
                    System.IO.File.WriteAllLines(saveDialog.FileName, lines);

                    MessageBox.Show("Список покупок экспортирован!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Останавливаем все таймеры при выходе
            foreach (var timer in _activeTimers)
            {
                timer.Timer.Stop();
            }

            NavigationService?.GoBack();
        }

        private void OtherAllergiesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

    // Окно для установки таймера
    public partial class TimerWindow : Window
    {
        public int SelectedMinutes { get; private set; } = 10;

        public TimerWindow(string mealName)
        {
            InitializeComponent();
            Title = $"Таймер для: {mealName}";
        }

        private void InitializeComponent()
        {
            Width = 300;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Установите время таймера (минуты):",
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var slider = new Slider
            {
                Minimum = 1,
                Maximum = 120,
                Value = SelectedMinutes,
                Margin = new Thickness(0, 0, 0, 10)
            };
            slider.ValueChanged += (s, e) => SelectedMinutes = (int)e.NewValue;
            stackPanel.Children.Add(slider);

            var valueText = new TextBlock
            {
                Text = $"{SelectedMinutes} мин.",
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackPanel.Children.Add(valueText);

            slider.ValueChanged += (s, e) =>
            {
                SelectedMinutes = (int)e.NewValue;
                valueText.Text = $"{SelectedMinutes} мин.";
            };

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

            var okButton = new Button
            {
                Content = "Запустить",
                Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                Foreground = Brushes.White,
                
                Margin = new Thickness(0, 0, 10, 0)
            };
            okButton.Click += (s, e) => { DialogResult = true; Close(); };
            buttonPanel.Children.Add(okButton);

            var cancelButton = new Button
            {
                Content = "Отмена",
                Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Foreground = Brushes.White,
                
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(buttonPanel);

            Content = stackPanel;
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        }

    }
}
