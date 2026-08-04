using EzFit.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzFit.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(p => p.UserId);
        builder.Property(p => p.WeightKg).HasColumnType("decimal(5,2)");
        builder.Property(p => p.HeightCm).HasColumnType("decimal(5,2)");
        builder.Property(p => p.Gender).IsRequired().HasMaxLength(20);
    }
}