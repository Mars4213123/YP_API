namespace YP_API.Models
{
    public class WeeklyMenu
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalCalories { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public ShoppingList ShoppingList { get; set; }
        public ICollection<MenuMeal> MenuMeals { get; set; } = new List<MenuMeal>();
    }
}
