using EzFit.Entities;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}