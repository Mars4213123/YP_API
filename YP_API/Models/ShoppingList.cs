namespace YP_API.Models
{
    public class ShoppingList
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsCompleted { get; set; } = false;
        public WeeklyMenu WeeklyMenu { get; set; }
        public ICollection<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();
    }
    public class ShoppingListItem
    {
        public int Id { get; set; }
        public int ShoppingListId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public bool IsPurchased { get; set; } = false;
        public string Category { get; set; }

        public ShoppingList ShoppingList { get; set; }
        public Ingredient Ingredient { get; set; }
    }
}
