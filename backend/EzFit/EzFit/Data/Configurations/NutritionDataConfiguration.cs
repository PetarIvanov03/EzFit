using EzFit.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzFit.Data.Configurations;

public class NutritionDataConfiguration : IEntityTypeConfiguration<NutritionData>
{
    public void Configure(EntityTypeBuilder<NutritionData> builder)
    {
        builder.HasKey(n => n.EntryId);
        builder.Property(n => n.Protein).HasColumnType("decimal(6,2)");
        builder.Property(n => n.Fats).HasColumnType("decimal(6,2)");
        builder.Property(n => n.Carbs).HasColumnType("decimal(6,2)");

        builder.HasOne(n => n.Entry)
            .WithOne(e => e.NutritionData)
            .HasForeignKey<NutritionData>(n => n.EntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}