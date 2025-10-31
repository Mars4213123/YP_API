namespace YP_API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<string> Allergies { get; set; } = new List<string>();

        public ICollection<WeeklyMenu> WeeklyMenus { get; set; } = new List<WeeklyMenu>();
        public ICollection<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();
        public ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
        public ICollection<UserInventory> Inventory { get; set; } = new List<UserInventory>();
    }
}
