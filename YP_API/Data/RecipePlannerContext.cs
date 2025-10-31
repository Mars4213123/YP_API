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
        public DbSet<WeeklyMenu> WeeklyMenus { get; set; }
        public DbSet<MenuMeal> MenuMeals { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ShoppingItem> ShoppingItems { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).IsRequired().HasMaxLength(255);
                entity.Property(r => r.CookingTime);
                entity.Property(r => r.Calories);
                entity.Property(r => r.ImageUrl).HasMaxLength(500);
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<WeeklyMenu>(entity =>
            {
                entity.HasKey(wm => wm.Id);
                entity.Property(wm => wm.StartDate).IsRequired();
                entity.Property(wm => wm.EndDate).IsRequired();

                entity.HasOne(wm => wm.User)
                    .WithMany(u => u.WeeklyMenus)
                    .HasForeignKey(wm => wm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MenuMeal>(entity =>
            {
                entity.HasKey(mm => mm.Id);
                entity.Property(mm => mm.MealDate).IsRequired();
                entity.Property(mm => mm.MealType).HasConversion<string>();

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
                entity.Property(sl => sl.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(sl => sl.User)
                    .WithMany(u => u.ShoppingLists)
                    .HasForeignKey(sl => sl.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sl => sl.WeeklyMenu)
                    .WithMany(wm => wm.ShoppingLists)
                    .HasForeignKey(sl => sl.MenuId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ShoppingItem>(entity =>
            {
                entity.HasKey(si => si.Id);
                entity.Property(si => si.Name).IsRequired().HasMaxLength(100);
                entity.Property(si => si.Quantity).HasPrecision(10, 2);
                entity.Property(si => si.IsPurchased).HasDefaultValue(false);

                entity.HasOne(si => si.ShoppingList)
                    .WithMany(sl => sl.ShoppingItems)
                    .HasForeignKey(si => si.ListId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(f => f.User)
                    .WithMany(u => u.Favorites)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Recipe)
                    .WithMany(r => r.Favorites)
                    .HasForeignKey(f => f.RecipeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RecipeIngredient>(entity =>
            {
                entity.HasKey(ri => ri.Id);
                entity.Property(ri => ri.Name).IsRequired().HasMaxLength(100);
                entity.Property(ri => ri.Quantity).HasPrecision(10, 2);
                entity.Property(ri => ri.Unit).HasMaxLength(20);

                entity.HasOne(ri => ri.Recipe)
                    .WithMany(r => r.RecipeIngredients)
                    .HasForeignKey(ri => ri.RecipeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}