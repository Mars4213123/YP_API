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
        public DbSet<WeeklyMenu> WeeklyMenus { get; set; }
        public DbSet<MenuMeal> MenuMeals { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ShoppingListItem> ShoppingListItems { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Allergies)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    );
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).IsRequired().HasMaxLength(200);
                entity.Property(r => r.Description).HasMaxLength(1000);
                entity.Property(r => r.Instructions).HasColumnType("text"); // MySQL text type
                entity.Property(r => r.ImageUrl).HasMaxLength(500);
                entity.Property(r => r.Calories).HasPrecision(10, 2);
                entity.Property(r => r.Tags)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    );
                entity.Property(r => r.Allergens)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    );
                entity.Property(r => r.CuisineType).HasMaxLength(50);
                entity.Property(r => r.Difficulty).HasMaxLength(20);
            });

            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Name).IsRequired().HasMaxLength(100);
                entity.Property(i => i.Category).IsRequired().HasMaxLength(50);
                entity.Property(i => i.StandardUnit).IsRequired().HasMaxLength(20);
                entity.Property(i => i.Allergens)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    );
                entity.HasIndex(i => i.Name).IsUnique();
            });

            modelBuilder.Entity<RecipeIngredient>(entity =>
            {
                entity.HasKey(ri => ri.Id);
                entity.Property(ri => ri.Quantity).HasPrecision(10, 3);
                entity.Property(ri => ri.Unit).HasMaxLength(20);

                entity.HasOne(ri => ri.Recipe)
                    .WithMany(r => r.RecipeIngredients)
                    .HasForeignKey(ri => ri.RecipeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ri => ri.Ingredient)
                    .WithMany(i => i.RecipeIngredients)
                    .HasForeignKey(ri => ri.IngredientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WeeklyMenu>(entity =>
            {
                entity.HasKey(wm => wm.Id);
                entity.Property(wm => wm.Name).IsRequired().HasMaxLength(200);
                entity.Property(wm => wm.TotalCalories).HasPrecision(10, 2);

                entity.HasOne(wm => wm.User)
                    .WithMany(u => u.WeeklyMenus)
                    .HasForeignKey(wm => wm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wm => wm.ShoppingList)
                    .WithOne(sl => sl.WeeklyMenu)
                    .HasForeignKey<ShoppingList>(sl => sl.MenuId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MenuMeal>(entity =>
            {
                entity.HasKey(mm => mm.Id);

                entity.HasOne(mm => mm.WeeklyMenu)
                    .WithMany(wm => wm.MenuMeals)
                    .HasForeignKey(mm => mm.MenuId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mm => mm.Recipe)
                    .WithMany(r => r.MenuMeals)
                    .HasForeignKey(mm => mm.RecipeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ShoppingList>(entity =>
            {
                entity.HasKey(sl => sl.Id);
                entity.Property(sl => sl.Name).IsRequired().HasMaxLength(200);

                entity.HasOne(sl => sl.WeeklyMenu)
                    .WithOne(wm => wm.ShoppingList)
                    .HasForeignKey<ShoppingList>(sl => sl.MenuId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ShoppingListItem>(entity =>
            {
                entity.HasKey(sli => sli.Id);
                entity.Property(sli => sli.Quantity).HasPrecision(10, 3);
                entity.Property(sli => sli.Unit).HasMaxLength(20);
                entity.Property(sli => sli.Category).HasMaxLength(50);

                entity.HasOne(sli => sli.ShoppingList)
                    .WithMany(sl => sl.Items)
                    .HasForeignKey(sli => sli.ShoppingListId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sli => sli.Ingredient)
                    .WithMany(i => i.ShoppingListItems)
                    .HasForeignKey(sli => sli.IngredientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(uf => uf.Id);

                entity.HasOne(uf => uf.User)
                    .WithMany(u => u.Favorites)
                    .HasForeignKey(uf => uf.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uf => uf.Recipe)
                    .WithMany(r => r.Favorites)
                    .HasForeignKey(uf => uf.RecipeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserInventory>(entity =>
            {
                entity.HasKey(ui => ui.Id);
                entity.Property(ui => ui.Quantity).HasPrecision(10, 3);
                entity.Property(ui => ui.Unit).HasMaxLength(20);

                entity.HasOne(ui => ui.User)
                    .WithMany(u => u.Inventory)
                    .HasForeignKey(ui => ui.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ui => ui.Ingredient)
                    .WithMany(i => i.UserInventories)
                    .HasForeignKey(ui => ui.IngredientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserInventory>()
                .HasIndex(ui => new { ui.UserId, ui.IngredientId })
                .IsUnique();

            modelBuilder.Entity<UserFavorite>()
                .HasIndex(uf => new { uf.UserId, uf.RecipeId })
                .IsUnique();
        }
    }
}