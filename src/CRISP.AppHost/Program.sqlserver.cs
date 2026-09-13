// The AppHost runs the whole local stack: database container, Migrator, server and client, with
// the dashboard for logs, traces and metrics. It is a development convenience; deployment is the
// Dockerfile and the Migrator, not this project.
var builder = DistributedApplication.CreateBuilder(args);

// The database resource is named "default" so the injected connection string is
// ConnectionStrings__default, the key the server and the Migrator already read.
var database = builder.AddSqlServer("sqlserver").WithDataVolume().AddDatabase("default", databaseName: "crisp");

// Schema first: the server waits for the Migrator to finish, so a fresh clone comes up migrated.
var migrator = builder
    .AddProject<Projects.CRISP_Migrator>("migrator")
    .WithReference(database)
    .WaitFor(database);

var server = builder
    .AddProject<Projects.CRISP_Server>("server")
    .WithReference(database)
    .WaitFor(database)
    .WaitForCompletion(migrator)
    .WithExternalHttpEndpoints();

//#if (IncludeReact)
builder.AddViteApp("client", "../CRISP.Client").WithReference(server).WaitFor(server).WithExternalHttpEndpoints();
//#endif

builder.Build().Run();
