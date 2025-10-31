namespace YP_API.Models
{
    public class Allergy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        public ICollection<RecipeAllergy> RecipeAllergies { get; set; }
        public ICollection<IngredientAllergy> IngredientAllergies { get; set; }
        public ICollection<UserAllergy> UserAllergies { get; set; }
    }

}
