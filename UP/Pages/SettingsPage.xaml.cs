using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace UP.Pages
{
    public partial class SettingsPage : Page
    {
        private string settingsFilePath = "user_settings.json";

        public class UserSettings
        {
            public string UserName { get; set; } = "Гость";
            public string UserEmail { get; set; } = "";
            public bool GlutenFree { get; set; } = false;
            public bool LactoseFree { get; set; } = false;
            public bool Vegan { get; set; } = false;
            public bool MealReminders { get; set; } = true;
            public bool ShoppingReminders { get; set; } = true;
            public bool CookingTimers { get; set; } = true;
        }

        private UserSettings currentSettings;

        public SettingsPage()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);
                    currentSettings = JsonSerializer.Deserialize<UserSettings>(json);
                }
                else
                {
                    currentSettings = new UserSettings();
                }

                // Загружаем настройки в UI
                UserNameTextBox.Text = currentSettings.UserName;
                UserEmailTextBox.Text = currentSettings.UserEmail;
                GlutenFreeSetting.IsChecked = currentSettings.GlutenFree;
                LactoseFreeSetting.IsChecked = currentSettings.LactoseFree;
                VeganSetting.IsChecked = currentSettings.Vegan;
                MealRemindersSetting.IsChecked = currentSettings.MealReminders;
                ShoppingRemindersSetting.IsChecked = currentSettings.ShoppingReminders;
                CookingTimersSetting.IsChecked = currentSettings.CookingTimers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                currentSettings = new UserSettings();
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем настройки из UI
                currentSettings.UserName = UserNameTextBox.Text.Trim();
                currentSettings.UserEmail = UserEmailTextBox.Text.Trim();
                currentSettings.GlutenFree = GlutenFreeSetting.IsChecked ?? false;
                currentSettings.LactoseFree = LactoseFreeSetting.IsChecked ?? false;
                currentSettings.Vegan = VeganSetting.IsChecked ?? false;
                currentSettings.MealReminders = MealRemindersSetting.IsChecked ?? true;
                currentSettings.ShoppingReminders = ShoppingRemindersSetting.IsChecked ?? true;
                currentSettings.CookingTimers = CookingTimersSetting.IsChecked ?? true;

                // Сохраняем в файл
                string json = JsonSerializer.Serialize(currentSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsFilePath, json);

                // Обновляем имя пользователя в главном окне
                if (MainWindow.mainWindow != null)
                {
                    var currentUserText = MainWindow.mainWindow.FindName("CurrentUserText") as System.Windows.Controls.TextBlock;
                    if (currentUserText != null)
                    {
                        currentUserText.Text = string.IsNullOrWhiteSpace(currentSettings.UserName)
                            ? "Гость"
                            : currentSettings.UserName;
                    }
                }

                MessageBox.Show("Настройки успешно сохранены!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetToDefaults_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Сбросить все настройки к значениям по умолчанию?",
                                       "Сброс настроек",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                currentSettings = new UserSettings();
                LoadSettings(); // Перезагружаем UI с настройками по умолчанию

                // Удаляем файл настроек
                if (File.Exists(settingsFilePath))
                {
                    File.Delete(settingsFilePath);
                }

                MessageBox.Show("Настройки сброшены к значениям по умолчанию", "Сброс",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}