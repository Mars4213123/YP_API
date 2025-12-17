namespace YP_API.Helpers
{
    public class RecipeSearchParams : PaginationParams
    {
        public string Name { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> ExcludedAllergens { get; set; } = new List<string>();
        public List<string> CuisineTypes { get; set; } = new List<string>();
        public int? MaxPrepTime { get; set; }
        public int? MaxCookTime { get; set; }
        public decimal? MaxCalories { get; set; }
        public string? Difficulty { get; set; }
        public string SortBy { get; set; } = "name";
        public bool SortDescending { get; set; } = false;
    }

    public class IngredientSearchParams : PaginationParams
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public bool? HasAllergens { get; set; }
        public string SortBy { get; set; } = "name";
        public bool SortDescending { get; set; } = false;
    }
}

