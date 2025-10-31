using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using YP_API.Models;

namespace YP_API.Configurations
{
    public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
    {
        public void Configure(EntityTypeBuilder<Ingredient> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(i => i.StandardUnit)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasOne(i => i.Category)
                .WithMany(ic => ic.Ingredients)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class IngredientCategoryConfiguration : IEntityTypeConfiguration<IngredientCategory>
    {
        public void Configure(EntityTypeBuilder<IngredientCategory> builder)
        {
            builder.HasKey(ic => ic.Id);

            builder.Property(ic => ic.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ic => ic.Description)
                .HasMaxLength(500);

            builder.HasIndex(ic => ic.Name)
                .IsUnique();
        }
    }
}
