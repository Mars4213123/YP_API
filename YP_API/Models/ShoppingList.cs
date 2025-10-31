namespace YP_API.Models
{
    public class ShoppingList
    {
        public int Id { get; set; }
        public int MenuPlanId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public MenuPlan MenuPlan { get; set; }
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

        public ShoppingList ShoppingList { get; set; }
        public Ingredient Ingredient { get; set; }
    }

    public class UserInventory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public Ingredient Ingredient { get; set; }
    }
}
