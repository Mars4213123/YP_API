using Microsoft.EntityFrameworkCore;
using YP_API.Configurations;
using YP_API.Models;

namespace YP_API.Data
{
    public class RecipePlannerContext : DbContext
    {
        public RecipePlannerContext(DbContextOptions<RecipePlannerContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<IngredientCategory> IngredientCategories { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<MenuPlan> MenuPlans { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; }

        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<RecipeAllergy> RecipeAllergies { get; set; }
        public DbSet<RecipeTag> RecipeTags { get; set; }
        public DbSet<IngredientAllergy> IngredientAllergies { get; set; }
        public DbSet<UserAllergy> UserAllergies { get; set; }
        public DbSet<UserFavoriteRecipe> UserFavoriteRecipes { get; set; }
        public DbSet<MenuPlanItem> MenuPlanItems { get; set; }
        public DbSet<ShoppingListItem> ShoppingListItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).IsRequired().HasMaxLength(200);
                entity.Property(r => r.Description).HasMaxLength(1000);
                entity.Property(r => r.Instructions).HasMaxLength(4000);
                entity.Property(r => r.ImageUrl).HasMaxLength(500);
                entity.Property(r => r.Calories).HasPrecision(10, 2);
            });

            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Name).IsRequired().HasMaxLength(100);
                entity.Property(i => i.StandardUnit).IsRequired().HasMaxLength(20);
                entity.HasIndex(i => i.Name).IsUnique();

                entity.HasOne(i => i.Category)
                    .WithMany(ic => ic.Ingredients)
                    .HasForeignKey(i => i.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<IngredientCategory>(entity =>
            {
                entity.HasKey(ic => ic.Id);
                entity.Property(ic => ic.Name).IsRequired().HasMaxLength(100);
                entity.Property(ic => ic.Description).HasMaxLength(500);
                entity.HasIndex(ic => ic.Name).IsUnique();
            });

            modelBuilder.Entity<Allergy>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Code).IsRequired().HasMaxLength(50);
                entity.Property(a => a.Description).HasMaxLength(500);
                entity.HasIndex(a => a.Code).IsUnique();
            });

            modelBuilder.Entity<RecipeIngredient>(entity =>
            {
                entity.HasKey(ri => ri.Id);
                entity.Property(ri => ri.Quantity).HasPrecision(10, 3);
                entity.Property(ri => ri.Unit).HasMaxLength(20);
            });

            modelBuilder.Entity<RecipeAllergy>()
                .HasKey(ra => new { ra.RecipeId, ra.AllergyId });

            modelBuilder.Entity<IngredientAllergy>()
                .HasKey(ia => new { ia.IngredientId, ia.AllergyId });

            modelBuilder.Entity<UserAllergy>()
                .HasKey(ua => new { ua.UserId, ua.AllergyId });

            modelBuilder.Entity<UserFavoriteRecipe>()
                .HasKey(uf => new { uf.UserId, uf.RecipeId });

            modelBuilder.Entity<MenuPlan>()
                .HasIndex(mp => new { mp.UserId, mp.StartDate, mp.EndDate });

            modelBuilder.Entity<ShoppingListItem>()
                .HasIndex(sli => new { sli.ShoppingListId, sli.IsPurchased });

            modelBuilder.Entity<UserInventory>()
                .HasIndex(ui => new { ui.UserId, ui.IngredientId })
                .IsUnique();
        }
    }
}