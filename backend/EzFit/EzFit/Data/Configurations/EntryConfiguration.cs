using EzFit.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzFit.Data.Configurations;

public class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.Title).HasMaxLength(200);

        builder.HasOne(e => e.Day)
            .WithMany(d => d.Entries)
            .HasForeignKey(e => e.DayId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}