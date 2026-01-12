using Microsoft.EntityFrameworkCore;
using YP_API.Models;

namespace YP_API.Data
{
    public class RecipePlannerContext : DbContext
    {
        public RecipePlannerContext(DbContextOptions<RecipePlannerContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }

        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ShoppingListItem> ShoppingListItems { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<UserAllergy> UserAllergies { get; set; }
        public DbSet<FridgeItem> FridgeItems { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ”казываем точные имена таблиц
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Recipe>().ToTable("recipes");
            modelBuilder.Entity<Ingredient>().ToTable("ingredients");
            modelBuilder.Entity<RecipeIngredient>().ToTable("recipe_ingredients");
            modelBuilder.Entity<Menu>().ToTable("menus");
            modelBuilder.Entity<MenuItem>().ToTable("menu_items");
            modelBuilder.Entity<ShoppingList>().ToTable("shopping_lists");
            modelBuilder.Entity<ShoppingListItem>().ToTable("shopping_list_items");
            modelBuilder.Entity<UserFavorite>().ToTable("user_favorites");

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Password).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.HasIndex(u => u.Username).IsUnique();
            });

            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).IsRequired().HasMaxLength(200);
                entity.Property(r => r.Description).HasMaxLength(1000);
                entity.Property(r => r.Instructions).HasColumnType("text");
                entity.Property(r => r.ImageUrl).HasMaxLength(500);
                entity.Property(r => r.Calories).HasPrecision(10, 2);
            });

            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(i => i.Name).IsUnique();
            });

            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(uf => uf.Id);
                entity.HasIndex(uf => new { uf.UserId, uf.RecipeId }).IsUnique();
            });
        }
    }
}