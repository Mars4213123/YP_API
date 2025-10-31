namespace YP_API.DTOs
{
    public class RecipeDto
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
        public List<RecipeIngredientDto> Ingredients { get; set; } = new List<RecipeIngredientDto>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Allergies { get; set; } = new List<string>();
    }

    public class RecipeIngredientDto
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string Category { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
    }

    public class CreateRecipeDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public int PrepTime { get; set; }
        public int CookTime { get; set; }
        public int Servings { get; set; }
        public decimal Calories { get; set; }
        public string ImageUrl { get; set; }
        public List<CreateRecipeIngredientDto> Ingredients { get; set; } = new List<CreateRecipeIngredientDto>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<int> AllergyIds { get; set; } = new List<int>();
    }

    public class CreateRecipeIngredientDto
    {
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
    }
}
