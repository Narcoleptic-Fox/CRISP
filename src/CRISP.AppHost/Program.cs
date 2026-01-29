var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");
var database = builder.AddPostgres("postgres");

var webServer = builder.AddProject<Projects.CRISP_Server>("webServer")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(database)
    .WaitFor(database)
    .WithExternalHttpEndpoints();

var webApp = builder.AddViteApp("webApp", "../CRISP.Web")
    .WithReference(webServer)
    .WaitFor(webServer)
    .WithExternalHttpEndpoints();

builder.Build().Run();
