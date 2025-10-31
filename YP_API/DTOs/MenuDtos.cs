using YP_API.Models;

namespace YP_API.DTOs
{
    public class GenerateMenuRequestDto
    {
        public int Days { get; set; } = 7;
        public int? TargetCalories { get; set; }
        public List<string> CuisineTags { get; set; } = new List<string>();
        public List<int> AvailableIngredientIds { get; set; } = new List<int>();
    }

    public class MenuPlanDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalCalories { get; set; }
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
        public int RecipeId { get; set; }
        public string RecipeTitle { get; set; }
        public MealType MealType { get; set; }
        public decimal Calories { get; set; }
        public int PrepTime { get; set; }
    }

    public class ShoppingListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ShoppingListItemDto> Items { get; set; } = new List<ShoppingListItemDto>();
    }

    public class ShoppingListItemDto
    {
        public int Id { get; set; }
        public string IngredientName { get; set; }
        public string Category { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public bool IsPurchased { get; set; }
    }
}
