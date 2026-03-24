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

        var recipe = modelBuilder.Entity<Recipe>()
            .ToContainer(nameof(Recipe))
            .HasMany<RecipeIngredient>()
            .WithOne()
            .HasPrincipalKey(r => r.Id)
            .HasForeignKey(i => i.RecipeId);
        /*recipe.Property(entity => entity.Title).ToJsonProperty("title");
        recipe.Property(entity => entity.RawText).ToJsonProperty("rawText");
        recipe.Property(entity => entity.ImageRef).ToJsonProperty("imageRef");
        recipe.Property(entity => entity.SearchText).ToJsonProperty("searchText");
        recipe.Property(entity => entity.NormalizedTags).ToJsonProperty("normalizedTags");
        recipe.Property(entity => entity.CreatedAt).ToJsonProperty("createdAt");
        recipe.Property(entity => entity.UpdatedAt).ToJsonProperty("updatedAt");*/

        var ingredient = modelBuilder.Entity<RecipeIngredient>()
            .ToContainer(nameof(RecipeIngredient))
            .HasOne<Recipe>();

        /*ingredient.Property(entity => entity.RecipeId).ToJsonProperty("recipeId");
        ingredient.Property(entity => entity.FreeText).ToJsonProperty("freeText");
        ingredient.Property(entity => entity.CanonicalName).ToJsonProperty("canonicalName");
        ingredient.Property(entity => entity.Position).ToJsonProperty("position");
        ingredient.Property(entity => entity.CreatedAt).ToJsonProperty("createdAt");*/
    }
}
