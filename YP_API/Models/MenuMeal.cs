namespace YP_API.Models
{
    public class MenuMeal
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public int RecipeId { get; set; }
        public DateTime MealDate { get; set; }
        public MealType MealType { get; set; }

        public WeeklyMenu WeeklyMenu { get; set; }
        public Recipe Recipe { get; set; }
    }

    public enum MealType
    {
        Breakfast = 1,
        Lunch = 2,
        Dinner = 3,
        Snack = 4
    }
}

