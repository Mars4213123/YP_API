namespace YP_API.Models
{
    public class MenuPlan
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalCalories { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public ICollection<MenuPlanItem> MenuPlanItems { get; set; }
        public ShoppingList ShoppingList { get; set; }
    }

    public class MenuPlanItem
    {
        public int Id { get; set; }
        public int MenuPlanId { get; set; }
        public int RecipeId { get; set; }
        public DateTime Date { get; set; }
        public MealType MealType { get; set; }

        public MenuPlan MenuPlan { get; set; }
        public Recipe Recipe { get; set; }
    }
}
