using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using YP_API.Models;

namespace YP_API.Configurations
{
    public class AllergyConfiguration : IEntityTypeConfiguration<Allergy>
    {
        public void Configure(EntityTypeBuilder<Allergy> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Description)
                .HasMaxLength(500);

            builder.HasIndex(a => a.Code)
                .IsUnique();
        }
    }

    public class RecipeAllergyConfiguration : IEntityTypeConfiguration<RecipeAllergy>
    {
        public void Configure(EntityTypeBuilder<RecipeAllergy> builder)
        {
            builder.HasKey(ra => new { ra.RecipeId, ra.AllergyId });

            builder.HasOne(ra => ra.Recipe)
                .WithMany(r => r.RecipeAllergies)
                .HasForeignKey(ra => ra.RecipeId);

            builder.HasOne(ra => ra.Allergy)
                .WithMany(a => a.RecipeAllergies)
                .HasForeignKey(ra => ra.AllergyId);
        }
    }

    public class IngredientAllergyConfiguration : IEntityTypeConfiguration<IngredientAllergy>
    {
        public void Configure(EntityTypeBuilder<IngredientAllergy> builder)
        {
            builder.HasKey(ia => new { ia.IngredientId, ia.AllergyId });

            builder.HasOne(ia => ia.Ingredient)
                .WithMany(i => i.IngredientAllergies)
                .HasForeignKey(ia => ia.IngredientId);

            builder.HasOne(ia => ia.Allergy)
                .WithMany(a => a.IngredientAllergies)
                .HasForeignKey(ia => ia.AllergyId);
        }
    }
}