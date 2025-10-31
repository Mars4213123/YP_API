namespace YP_API.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? CookingTime { get; set; }
        public int? Calories { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<MenuMeal> MenuMeals { get; set; } = new List<MenuMeal>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }

    public class RecipeIngredient
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }

        public Recipe Recipe { get; set; }
        public Ingredient Ingredient { get; set; }
    }

    public class RecipeAllergy
    {
        public int RecipeId { get; set; }
        public int AllergyId { get; set; }

        public Recipe Recipe { get; set; }
        public Allergy Allergy { get; set; }
    }

    public class RecipeTag
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public string Tag { get; set; }

        public Recipe Recipe { get; set; }
    }

    public class UserFavoriteRecipe
    {
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public Recipe Recipe { get; set; }
    }
}
