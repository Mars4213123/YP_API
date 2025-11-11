namespace YP_API.Models
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string StandardUnit { get; set; }
        public List<string> Allergens { get; set; } = new List<string>();

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<UserInventory> UserInventories { get; set; } = new List<UserInventory>();
        public ICollection<ShoppingListItem> ShoppingListItems { get; set; } = new List<ShoppingListItem>();
    }
}

