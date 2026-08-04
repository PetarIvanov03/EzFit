using EzFit.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzFit.Data.Configurations;

public class SleepDataConfiguration : IEntityTypeConfiguration<SleepData>
{
    public void Configure(EntityTypeBuilder<SleepData> builder)
    {
        builder.HasKey(s => s.EntryId);

        builder.HasOne(s => s.Entry)
            .WithOne(e => e.SleepData)
            .HasForeignKey<SleepData>(s => s.EntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}