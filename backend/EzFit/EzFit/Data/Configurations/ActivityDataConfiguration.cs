using EzFit.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzFit.Data.Configurations;

public class ActivityDataConfiguration : IEntityTypeConfiguration<ActivityData>
{
    public void Configure(EntityTypeBuilder<ActivityData> builder)
    {
        builder.HasKey(a => a.EntryId);
        builder.Property(a => a.DistanceKm).HasColumnType("decimal(6,2)");
        builder.Property(a => a.ElevationM).HasColumnType("decimal(7,2)");

        builder.HasOne(a => a.Entry)
            .WithOne(e => e.ActivityData)
            .HasForeignKey<ActivityData>(a => a.EntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}