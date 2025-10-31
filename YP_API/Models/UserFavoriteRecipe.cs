namespace YP_API.Models
{
    public class UserFavoriteRecipe
    {
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public Recipe Recipe { get; set; }
    }
}
