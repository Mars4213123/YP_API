namespace YP_API.Models
{
    public class Recipe
    {
        public Recipe()
        {
            ImageUrl = string.Empty;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public int PrepTime { get; set; }
        public int CookTime { get; set; }
        public int Servings { get; set; }
        public decimal Calories { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Allergens { get; set; } = new List<string>();
        public string CuisineType { get; set; }
        public string Difficulty { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPublic { get; set; } = true;

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<MenuMeal> MenuMeals { get; set; } = new List<MenuMeal>();
        public ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
    }
}
