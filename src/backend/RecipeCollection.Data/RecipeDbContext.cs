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
        /*var document = modelBuilder.Entity<CosmosDocument>();
        //document.ToContainer(_containerName);
        document.HasKey(entity => entity.Id);
        document.HasPartitionKey(entity => entity.Pk);
        document.Property(entity => entity.Id).ToJsonProperty("id");
        document.Property(entity => entity.Pk).ToJsonProperty("pk");
        document.Property(entity => entity.Type).ToJsonProperty("type");
        document.HasDiscriminator(entity => entity.Type)
            .HasValue<Recipe>("Recipe")
            .HasValue<RecipeIngredient>("RecipeIngredient");*/

        modelBuilder.Entity<Recipe>()
            .ToContainer(nameof(Recipe))
            .HasMany<RecipeIngredient>()
            .WithOne()
            .HasPrincipalKey(r => r.Id)
            .HasForeignKey(i => i.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
            .ToContainer(nameof(RecipeIngredient))
            .HasOne<Recipe>();
    }
}
