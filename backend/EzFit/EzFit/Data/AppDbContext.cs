using EzFit.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace EzFit.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Day> Days => Set<Day>();
    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<NutritionData> NutritionData => Set<NutritionData>();
    public DbSet<ActivityData> ActivityData => Set<ActivityData>();
    public DbSet<SleepData> SleepData => Set<SleepData>();

    private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> NullableUtcConverter = new(
        v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Postgres "timestamp with time zone" refuses DateTime.Kind=Unspecified.
        // Treat every stored DateTime as UTC without hunting down each
        // construction site individually.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(UtcConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(NullableUtcConverter);
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}