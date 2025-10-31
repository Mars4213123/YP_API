namespace YP_API.Models
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string StandardUnit { get; set; }

        public IngredientCategory Category { get; set; }
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<IngredientAllergy> IngredientAllergies { get; set; } = new List<IngredientAllergy>();
        public ICollection<UserInventory> UserInventories { get; set; } = new List<UserInventory>();
        public ICollection<ShoppingListItem> ShoppingListItems { get; set; } = new List<ShoppingListItem>();
    }

    public class IngredientCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }

    public class IngredientAllergy
    {
        public int IngredientId { get; set; }
        public int AllergyId { get; set; }

        public Ingredient Ingredient { get; set; }
        public Allergy Allergy { get; set; }
    }
}

