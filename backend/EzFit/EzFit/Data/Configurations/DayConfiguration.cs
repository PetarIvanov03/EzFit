using EzFit.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzFit.Data.Configurations;

public class DayConfiguration : IEntityTypeConfiguration<Day>
{
    public void Configure(EntityTypeBuilder<Day> builder)
    {
        builder.HasIndex(d => new { d.UserId, d.Date }).IsUnique();

        builder.HasOne(d => d.User)
            .WithMany(u => u.Days)
            .HasForeignKey(d => d.UserId);
    }
}