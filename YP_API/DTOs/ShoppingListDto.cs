namespace YP_API.DTOs
{
    public class ShoppingListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsCompleted { get; set; }
        public List<ShoppingListItemDto> Items { get; set; } = new List<ShoppingListItemDto>();
    }

    public class ShoppingListItemDto
    {
        public int Id { get; set; }
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string Category { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public bool IsPurchased { get; set; }
    }

    public class UpdateShoppingItemDto
    {
        public bool IsPurchased { get; set; }
    }
}
