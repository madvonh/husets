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

var api = builder.AddProject<Projects.RecipeCollection_Api>("api")
    .WithReference(dbResource)
    .WithReference(blobs)
    .WaitFor(dbResource)
    .WaitFor(blobs)
    .WithHttpHealthCheck("/health");

if (builder.ExecutionContext.IsPublishMode)
{
    builder.AddDockerfile(name: "frontend", contextPath: "../../frontend")
        .WithHttpEndpoint(targetPort: 80, name: "http")
        .WithExternalHttpEndpoints()
        .WithReference(api)
        .WaitFor(api);
}
else
{
    builder.AddViteApp(name: "frontend", appDirectory: "../../frontend")
        .WithExternalHttpEndpoints()
        .WithReference(api)
        .WaitFor(api);
}

builder.Build().Run();
