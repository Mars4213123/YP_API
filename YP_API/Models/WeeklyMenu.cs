namespace YP_API.Models
{
    public class WeeklyMenu
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public User User { get; set; }
        public ICollection<MenuMeal> MenuMeals { get; set; } = new List<MenuMeal>();
        public ICollection<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();
    }
}
