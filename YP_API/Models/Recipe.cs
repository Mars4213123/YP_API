namespace YP_API.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public int PrepTime { get; set; }
        public int CookTime { get; set; }
        public int Servings { get; set; }
        public decimal Calories { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPublic { get; set; } = true;

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<RecipeAllergy> RecipeAllergies { get; set; } = new List<RecipeAllergy>();
        public ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();
        public ICollection<UserFavoriteRecipe> FavoriteByUsers { get; set; } = new List<UserFavoriteRecipe>();
        public ICollection<MenuPlanItem> MenuPlanItems { get; set; } = new List<MenuPlanItem>();
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
