// The AppHost runs the whole local stack: database container, Migrator, server and client, with
// the dashboard for logs, traces and metrics. It is a development convenience; deployment is the
// Dockerfile and the Migrator, not this project.
var builder = DistributedApplication.CreateBuilder(args);

// SQLite is a file next to the server; there is no container to run.

// Schema first: the server waits for the Migrator to finish, so a fresh clone comes up migrated.
var migrator = builder
    .AddProject<Projects.CRISP_Migrator>("migrator")
    .WithEnvironment("ConnectionStrings__default", "Data Source=crisp.db");

var server = builder
    .AddProject<Projects.CRISP_Server>("server")
    .WithEnvironment("ConnectionStrings__default", "Data Source=crisp.db")
    .WaitForCompletion(migrator)
    .WithExternalHttpEndpoints();

//#if (IncludeReact)
builder.AddViteApp("client", "../CRISP.Client").WithReference(server).WaitFor(server).WithExternalHttpEndpoints();
//#endif

builder.Build().Run();
