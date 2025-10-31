namespace YP_API.Helpers
{
    public class RecipeSearchParams : PaginationParams
    {
        public string Name { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> ExcludedAllergies { get; set; } = new List<string>();
        public List<int> IncludeIngredients { get; set; } = new List<int>();
        public List<int> ExcludeIngredients { get; set; } = new List<int>();
        public int? MaxPrepTime { get; set; }
        public int? MaxCookTime { get; set; }
        public decimal? MaxCalories { get; set; }
        public string SortBy { get; set; } = "name";
        public bool SortDescending { get; set; } = false;
    }

    public class IngredientSearchParams : PaginationParams
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public int? CategoryId { get; set; }
        public bool? HasAllergies { get; set; }
        public string SortBy { get; set; } = "name";
        public bool SortDescending { get; set; } = false;
    }
}
