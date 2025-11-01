using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace UP.Pages
{
    public partial class HistoryPage : Page
    {
        public class MenuHistory
        {
            public string Id { get; set; }
            public string Period { get; set; }
            public string DateRange { get; set; }
            public DateTime CreatedDate { get; set; }
            public int TotalMeals { get; set; }
            public double AverageCalories { get; set; }
            public string PreferredCuisine { get; set; }
            public ObservableCollection<string> Dishes { get; set; } = new ObservableCollection<string>();
        }

        private ObservableCollection<MenuHistory> menuHistory = new ObservableCollection<MenuHistory>();
        private string historyFilePath = "menu_history.json";

        public HistoryPage()
        {
            InitializeComponent();
            HistoryItemsControl.ItemsSource = menuHistory;
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists(historyFilePath))
                {
                    string json = File.ReadAllText(historyFilePath);
                    var history = JsonSerializer.Deserialize<ObservableCollection<MenuHistory>>(json);

                    menuHistory.Clear();
                    foreach (var item in history.OrderByDescending(h => h.CreatedDate))
                    {
                        menuHistory.Add(item);
                    }
                }
                else
                {
                    // Загрузка тестовых данных
                    LoadSampleHistory();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                LoadSampleHistory();
            }
        }

        private void LoadSampleHistory()
        {
            menuHistory.Clear();

            menuHistory.Add(new MenuHistory
            {
                Id = "1",
                Period = "Неделя 45 - Ноябрь 2024",
                DateRange = "04.11.2024 - 10.11.2024",
                CreatedDate = DateTime.Now.AddDays(-3),
                TotalMeals = 21,
                AverageCalories = 1850,
                PreferredCuisine = "Итальянская",
                Dishes = new ObservableCollection<string> { "Паста Карбонара", "Салат Цезарь", "Овсяная каша" }
            });

            menuHistory.Add(new MenuHistory
            {
                Id = "2",
                Period = "Неделя 44 - Октябрь 2024",
                DateRange = "28.10.2024 - 03.11.2024",
                CreatedDate = DateTime.Now.AddDays(-10),
                TotalMeals = 18,
                AverageCalories = 1950,
                PreferredCuisine = "Азиатская",
                Dishes = new ObservableCollection<string> { "Вок с курицей", "Суши", "Рамен" }
            });

            menuHistory.Add(new MenuHistory
            {
                Id = "3",
                Period = "Неделя 43 - Октябрь 2024",
                DateRange = "21.10.2024 - 27.10.2024",
                CreatedDate = DateTime.Now.AddDays(-17),
                TotalMeals = 15,
                AverageCalories = 1750,
                PreferredCuisine = "Русская",
                Dishes = new ObservableCollection<string> { "Борщ", "Котлеты", "Гречневая каша" }
            });
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is MenuHistory history)
            {
                // Открываем детальное представление меню
                var historyDetailPage = new HistoryPage(history);
                NavigationService.Navigate(historyDetailPage);
            }
        }



        private void ShowStats_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is MenuHistory history)
            {
                ShowStatistics(history);
            }
        }

        private void ShowStatistics(MenuHistory history)
        {
            string statsMessage = $@"Статистика меню '{history.Period}':

📅 Период: {history.DateRange}
🍽️ Всего приемов пищи: {history.TotalMeals}
🔥 Средняя калорийность: {history.AverageCalories} ккал
🌍 Предпочтительная кухня: {history.PreferredCuisine}
📊 Популярные блюда:
{string.Join("\n", history.Dishes.Take(5).Select((d, i) => $"{i + 1}. {d}"))}";

            MessageBox.Show(statsMessage, "Статистика меню",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RepeatMenu_Click(object sender, RoutedEventArgs e)
        {
            if (menuHistory.Count > 0)
            {
                var latestMenu = menuHistory.First();

                var result = MessageBox.Show($"Повторить меню '{latestMenu.Period}'?\n\nЭто создаст новое меню на основе выбранного периода.",
                                           "Повтор меню",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Создаем копию меню
                    var newMenu = new MenuHistory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Period = $"Повтор: {latestMenu.Period}",
                        DateRange = $"{DateTime.Now:dd.MM.yyyy} - {DateTime.Now.AddDays(6):dd.MM.yyyy}",
                        CreatedDate = DateTime.Now,
                        TotalMeals = latestMenu.TotalMeals,
                        AverageCalories = latestMenu.AverageCalories,
                        PreferredCuisine = latestMenu.PreferredCuisine
                    };

                    foreach (var dish in latestMenu.Dishes)
                    {
                        newMenu.Dishes.Add(dish);
                    }

                    menuHistory.Insert(0, newMenu);
                    SaveHistory();

                    MessageBox.Show("Меню успешно повторено!\n\nПерейдите в раздел 'Меню на неделю' для просмотра.",
                                  "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Нет истории меню для повторения", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveHistory()
        {
            try
            {
                string json = JsonSerializer.Serialize(menuHistory, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(historyFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения истории: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddToHistory(MenuHistory history)
        {
            // Проверяем, нет ли уже меню с таким ID
            var existingHistory = menuHistory.FirstOrDefault(h => h.Id == history.Id);
            if (existingHistory == null)
            {
                menuHistory.Insert(0, history);
                SaveHistory();
            }
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Очистить всю историю меню?\n\nЭто действие нельзя отменить.",
                                       "Очистка истории",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                menuHistory.Clear();

                if (File.Exists(historyFilePath))
                {
                    File.Delete(historyFilePath);
                }

                MessageBox.Show("История меню очищена", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}