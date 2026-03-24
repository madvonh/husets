using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeCollection.DTOs;
using RecipeCollection.DTOs.RequestModels;
using RecipeCollection.DTOs.ResponseModels;
using RecipeCollection.Services;
using RecipeCollection.Data;
using RecipeCollection.Domain;
using RecipeCollection.Utilities;

namespace RecipeCollection.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this WebApplication app)
    {
        // Validation constants
        const long MaxImageSizeBytes = 10 * 1024 * 1024; // 10 MB
        var AllowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };

        app.MapPost("/ocr", async (
            [FromForm] OcrRequest request,
            IOcrService ocrService,
            IBlobStorageService blobService,
            ILogger<Program> logger) =>
        {
            var image = request.Image;
            if (image == null || image.Length == 0)
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Code = "INVALID_REQUEST",
                    Message = "Image file is required"
                });
            }

            // Validate image size
            if (image.Length > MaxImageSizeBytes)
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Code = "FILE_TOO_LARGE",
                    Message = $"Image size exceeds maximum allowed size of {MaxImageSizeBytes / 1024 / 1024} MB"
                });
            }

            // Validate image type
            if (!AllowedImageTypes.Contains(image.ContentType?.ToLowerInvariant()))
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Code = "INVALID_FILE_TYPE",
                    Message = $"Invalid image type. Allowed types: {string.Join(", ", AllowedImageTypes)}"
                });
            }

            try
            {
                // Upload to blob storage
                string imageRef;
                using (var stream = image.OpenReadStream())
                {
                    imageRef = await blobService.UploadImageAsync(stream, image.FileName, image.ContentType ?? "application/octet-stream");
                }

                // Extract text via OCR
                string extractedText;
                using (var stream = image.OpenReadStream())
                {
                    extractedText = await ocrService.ExtractTextFromImageAsync(stream);
                }

                logger.LogInformation("OCR completed for image: {ImageRef}", imageRef);

                return Results.Ok(new OcrResponse
                {
                    ImageRef = imageRef,
                    ExtractedText = extractedText
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OCR processing failed");
                return Results.Problem("OCR processing failed. Please try again.");
            }
        })
        .Accepts<OcrRequest>("multipart/form-data")
        .Produces<OcrResponse>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .DisableAntiforgery();

        app.MapPost("/recipes", async (
            CreateRecipeRequest request,
            IValidator<CreateRecipeRequest> validator,
            RecipeDbContext dbContext,
            IIngredientParser ingredientParser,
            ILogger<Program> logger) =>
        {
            // Validate request
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return Results.BadRequest(new ErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = errors
                });
            }

            try
            {
                var recipeId = $"recipe_{Guid.NewGuid()}";
                var normalizedTags = request.Tags != null ? TagNormalizer.Normalize(request.Tags) : [];
                var searchText = SearchTextBuilder.BuildSearchText(request.Title, request.RawText);

                // Create recipe
                var recipe = new Recipe
                {
                    Id = recipeId,
                    Pk = "recipe",
                    Type = "Recipe",
                    Title = request.Title,
                    RawText = request.RawText,
                    ImageRef = request.ImageRef,
                    SearchText = searchText,
                    NormalizedTags = normalizedTags
                };

                dbContext.Recipes.Add(recipe);

                // Parse and store ingredients
                var ingredients = ingredientParser.ParseIngredients(request.RawText, recipeId);
                foreach (var ingredient in ingredients)
                {
                    ingredient.Type = "RecipeIngredient";
                }
                dbContext.RecipeIngredients.AddRange(ingredients);

                await dbContext.SaveChangesAsync();

                logger.LogInformation("Created recipe: {RecipeId} with {IngredientCount} ingredients", recipeId, ingredients.Count);

                return Results.Created($"/recipes/{recipeId}", new RecipeDetailResponse
                {
                    Id = recipe.Id,
                    Title = recipe.Title,
                    RawText = recipe.RawText,
                    ImageRef = recipe.ImageRef,
                    Tags = recipe.NormalizedTags,
                    Ingredients = ingredients.Select(i => new IngredientDto
                    {
                        FreeText = i.FreeText,
                        CanonicalName = i.CanonicalName,
                        Position = i.Position
                    }).ToList(),
                    CreatedAt = recipe.CreatedAt
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create recipe");
                return Results.Problem("Failed to create recipe. Please try again.");
            }
        });

        app.MapGet("/recipes/{id}", async (
            string id,
            RecipeDbContext dbContext,
            ILogger<Program> logger) =>
        {
            try
            {
                var recipe = await dbContext.Recipes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(recipe => recipe.Id == id && recipe.Pk == "recipe");

                if (recipe == null)
                {
                    return Results.NotFound(new ErrorResponse
                    {
                        Code = "NOT_FOUND",
                        Message = "Recipe not found"
                    });
                }

                var ingredients = await dbContext.RecipeIngredients
                    .AsNoTracking()
                    .Where(ingredient => ingredient.RecipeId == id && ingredient.Pk == "recipe")
                    .OrderBy(ingredient => ingredient.Position)
                    .ToListAsync();

                return Results.Ok(new RecipeDetailResponse
                {
                    Id = recipe.Id,
                    Title = recipe.Title,
                    RawText = recipe.RawText,
                    ImageRef = recipe.ImageRef,
                    Tags = recipe.NormalizedTags,
                    Ingredients = ingredients.Select(i => new IngredientDto
                    {
                        FreeText = i.FreeText,
                        CanonicalName = i.CanonicalName,
                        Position = i.Position
                    }).ToList(),
                    CreatedAt = recipe.CreatedAt
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get recipe: {RecipeId}", id);
                return Results.Problem("Failed to retrieve recipe.");
            }
        });
        
        app.MapGet("/recipes", async (
            string? query,
            string? tag,
            RecipeDbContext dbContext,
            ILogger<Program> logger) =>
        {
            try
            {
                var recipes = await dbContext.Recipes
                    .AsNoTracking()
                    .Where(recipe => recipe.Pk == "recipe")
                    .ToListAsync();

                if (!string.IsNullOrWhiteSpace(tag))
                {
                    var normalizedTag = TagNormalizer.Normalize(tag);
                    recipes = recipes.Where(r => r.NormalizedTags != null && r.NormalizedTags.Contains(normalizedTag)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(query))
                {
                    var q = query.Trim().ToLowerInvariant();
                    recipes = recipes.Where(r => (r.SearchText ?? string.Empty).Contains(q)).ToList();
                }

                var summaries = recipes.Select(r => new RecipeSummaryResponse
                {
                    Id = r.Id,
                    Title = r.Title,
                    Tags = r.NormalizedTags ?? new List<string>(),
                    CreatedAt = r.CreatedAt,
                    ImageRef = r.ImageRef
                }).ToList();

                return Results.Ok(summaries);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to search recipes");
                return Results.Problem("Failed to search recipes.");
            }
        });

        app.MapPost("/recipes/{id}/tags", async (
            string id,
            AddTagRequest request,
            IValidator<AddTagRequest> validator,
            RecipeDbContext dbContext,
            ILogger<Program> logger) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return Results.BadRequest(new ErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = errors
                });
            }

            try
            {
                var recipe = await dbContext.Recipes
                    .FirstOrDefaultAsync(entity => entity.Id == id && entity.Pk == "recipe");

                if (recipe == null)
                {
                    return Results.NotFound(new ErrorResponse
                    {
                        Code = "NOT_FOUND",
                        Message = "Recipe not found"
                    });
                }

                var normalizedTag = TagNormalizer.Normalize(request.Tag);
                
                // Add tag if not already present (idempotent)
                if (!recipe.NormalizedTags.Contains(normalizedTag))
                {
                    recipe.NormalizedTags.Add(normalizedTag);
                    recipe.UpdatedAt = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync();
                }

                var ingredients = await dbContext.RecipeIngredients
                    .AsNoTracking()
                    .Where(ingredient => ingredient.RecipeId == id && ingredient.Pk == "recipe")
                    .OrderBy(ingredient => ingredient.Position)
                    .ToListAsync();

                logger.LogInformation("Added tag '{Tag}' to recipe: {RecipeId}", normalizedTag, id);

                return Results.Ok(new RecipeDetailResponse
                {
                    Id = recipe.Id,
                    Title = recipe.Title,
                    RawText = recipe.RawText,
                    ImageRef = recipe.ImageRef,
                    Tags = recipe.NormalizedTags,
                    Ingredients = ingredients.Select(i => new IngredientDto
                    {
                        FreeText = i.FreeText,
                        CanonicalName = i.CanonicalName,
                        Position = i.Position
                    }).ToList(),
                    CreatedAt = recipe.CreatedAt
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to add tag to recipe: {RecipeId}", id);
                return Results.Problem("Failed to add tag. Please try again.");
            }
        });

        app.MapDelete("/recipes/{id}/tags/{tag}", async (
            string id,
            string tag,
            RecipeDbContext dbContext,
            ILogger<Program> logger) =>
        {
            try
            {
                var recipe = await dbContext.Recipes
                    .FirstOrDefaultAsync(entity => entity.Id == id && entity.Pk == "recipe");

                if (recipe == null)
                {
                    return Results.NotFound(new ErrorResponse
                    {
                        Code = "NOT_FOUND",
                        Message = "Recipe not found"
                    });
                }

                var normalizedTag = TagNormalizer.Normalize(tag);
                
                // Remove tag if present (idempotent)
                if (recipe.NormalizedTags.Contains(normalizedTag))
                {
                    recipe.NormalizedTags.Remove(normalizedTag);
                    recipe.UpdatedAt = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync();
                }

                var ingredients = await dbContext.RecipeIngredients
                    .AsNoTracking()
                    .Where(ingredient => ingredient.RecipeId == id && ingredient.Pk == "recipe")
                    .OrderBy(ingredient => ingredient.Position)
                    .ToListAsync();

                logger.LogInformation("Removed tag '{Tag}' from recipe: {RecipeId}", normalizedTag, id);

                return Results.Ok(new RecipeDetailResponse
                {
                    Id = recipe.Id,
                    Title = recipe.Title,
                    RawText = recipe.RawText,
                    ImageRef = recipe.ImageRef,
                    Tags = recipe.NormalizedTags,
                    Ingredients = ingredients.Select(i => new IngredientDto
                    {
                        FreeText = i.FreeText,
                        CanonicalName = i.CanonicalName,
                        Position = i.Position
                    }).ToList(),
                    CreatedAt = recipe.CreatedAt
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to remove tag from recipe: {RecipeId}", id);
                return Results.Problem("Failed to remove tag. Please try again.");
            }
        });
    }
}
