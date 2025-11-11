namespace YP_API.Models
{
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
}

