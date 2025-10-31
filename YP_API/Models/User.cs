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

        public ICollection<UserAllergy> UserAllergies { get; set; }
        public ICollection<MenuPlan> MenuPlans { get; set; }
        public ICollection<UserFavoriteRecipe> FavoriteRecipes { get; set; }
        public ICollection<ShoppingList> ShoppingLists { get; set; }
    }
    public class UserAllergy
    {
        public int UserId { get; set; }
        public int AllergyId { get; set; }
        public User User { get; set; }
        public Allergy Allergy { get; set; }
    }
}
