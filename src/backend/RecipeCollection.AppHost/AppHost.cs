var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.RecipeCollection_Api>("api")
    .WithHttpHealthCheck("/health");
    
    builder.AddViteApp(name: "frontend", appDirectory: "../../frontend" )
        .WithReference(api)
        .WaitFor(api)
        .WithExternalHttpEndpoints();

builder.Build().Run();
