var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos");

if (!builder.ExecutionContext.IsPublishMode)
{
    cosmos.RunAsEmulator(emulator =>
    {
        emulator.WithLifetime(ContainerLifetime.Persistent);
    });
}

var dbResource = cosmos.AddCosmosDatabase("cosmosdb");

var api = builder.AddProject<Projects.RecipeCollection_Api>("api")
    .WithReference(dbResource)
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
