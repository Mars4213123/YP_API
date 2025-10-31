namespace YP_API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserAllergy> UserAllergies { get; set; } = new List<UserAllergy>();
        public ICollection<MenuPlan> MenuPlans { get; set; } = new List<MenuPlan>();
        public ICollection<UserFavoriteRecipe> FavoriteRecipes { get; set; } = new List<UserFavoriteRecipe>();
        public ICollection<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();
        public ICollection<UserInventory> UserInventories { get; set; } = new List<UserInventory>();
    }
    public class UserAllergy
    {
        public int UserId { get; set; }
        public int AllergyId { get; set; }
        public User User { get; set; }
        public Allergy Allergy { get; set; }
    }
}
