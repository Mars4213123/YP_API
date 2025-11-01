using YP_API.Models;

namespace YP_API.DTOs
{
    public class GenerateMenuRequestDto
    {
        public int Days { get; set; } = 7;
        public decimal? TargetCaloriesPerDay { get; set; }
        public List<string> CuisineTags { get; set; } = new List<string>();
        public List<string> MealTypes { get; set; } = new List<string> { "breakfast", "lunch", "dinner" };
        public List<int> AvailableIngredientIds { get; set; } = new List<int>();
        public bool UseInventory { get; set; } = false;
    }

    public class WeeklyMenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalCalories { get; set; }
        public List<MenuDayDto> Days { get; set; } = new List<MenuDayDto>();
        public ShoppingListDto ShoppingList { get; set; }
    }

    public class MenuDayDto
    {
        public DateTime Date { get; set; }
        public List<MenuItemDto> Meals { get; set; } = new List<MenuItemDto>();
    }

    public class MenuItemDto
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public string RecipeTitle { get; set; }
        public MealType MealType { get; set; }
        public decimal Calories { get; set; }
        public int PrepTime { get; set; }
        public string ImageUrl { get; set; }
    }
}

