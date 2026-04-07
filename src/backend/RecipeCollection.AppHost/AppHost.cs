var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos");

var storage = builder.AddAzureStorage("storage");

if (!builder.ExecutionContext.IsPublishMode)
{
    cosmos.RunAsEmulator(emulator =>
    {
        emulator.WithLifetime(ContainerLifetime.Persistent);
    });

    storage.RunAsEmulator(emulator =>
    {
        emulator.WithLifetime(ContainerLifetime.Persistent);
    });
}

var dbResource = cosmos.AddCosmosDatabase("cosmosdb");
var blobs = storage.AddBlobs("blobs");

if (builder.ExecutionContext.IsPublishMode)
{
    var api = builder.AddDockerfile(name: "api", contextPath: "..")
        .WithReference(dbResource)
        .WithReference(blobs)
        .WaitFor(dbResource)
        .WaitFor(blobs);

    builder.AddDockerfile(name: "frontend", contextPath: "../../frontend")
        .WithExternalHttpEndpoints();
}
else
{
    var api = builder.AddProject<Projects.RecipeCollection_Api>("api")
        .WithReference(dbResource)
        .WithReference(blobs)
        .WaitFor(dbResource)
        .WaitFor(blobs);

    builder.AddViteApp(name: "frontend", appDirectory: "../../frontend")
        .WithExternalHttpEndpoints()
        .WithReference(api)
        .WaitFor(api);
}

builder.Build().Run();
