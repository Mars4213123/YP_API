using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

namespace UP
{
    public static class AppData
    {
        // Продукты в холодильнике
        public static ObservableCollection<string> Products { get; set; } = new ObservableCollection<string>();

        // Меню на неделю
        public static ObservableCollection<Pages.Receipts.DailyMenu> WeeklyMenu { get; set; } = new ObservableCollection<Pages.Receipts.DailyMenu>();

        // Список покупок
        public static ObservableCollection<string> ShoppingList { get; set; } = new ObservableCollection<string>();

        // Внизу файла AppData.cs, рядом с другими коллекциями
        public static ObservableCollection<Pages.RecipeDetailsPage.RecipeData> Favorites { get; set; } = new ObservableCollection<Pages.RecipeDetailsPage.RecipeData>();

    }
}


