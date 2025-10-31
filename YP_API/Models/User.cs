namespace YP_API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WeeklyMenu> WeeklyMenus { get; set; } = new List<WeeklyMenu>();
        public ICollection<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }

    public class UserAllergy
    {
        public int UserId { get; set; }
        public int AllergyId { get; set; }
        public User User { get; set; }
        public Allergy Allergy { get; set; }
    }
}
