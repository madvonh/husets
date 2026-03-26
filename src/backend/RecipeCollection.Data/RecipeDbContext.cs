using Microsoft.EntityFrameworkCore;
using RecipeCollection.Domain;

namespace RecipeCollection.Data;

public class RecipeDbContext : DbContext
{

    public RecipeDbContext(DbContextOptions<RecipeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Recipe> Recipes => Set<Recipe>();

    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>()
            .ToContainer(nameof(Recipe))
            .HasPartitionKey(r => r.Pk)
            .HasMany<RecipeIngredient>()
            .WithOne()
            .HasPrincipalKey(r => r.Id)
            .HasForeignKey(i => i.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
            .ToContainer(nameof(RecipeIngredient))
            .HasOne<Recipe>();
    }
}
