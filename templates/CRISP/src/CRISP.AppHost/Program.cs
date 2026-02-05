var builder = DistributedApplication.CreateBuilder(args);

//#if (IncludeRedis)
var redis = builder.AddRedis("redis");
//#endif
//#if (UsePostgres)
var database = builder.AddPostgres("postgres");
//#endif
//#if (UseSqlServer)
var database = builder.AddSqlServer("sqlserver");
//#endif
//#if (UseSqlite)
// SQLite doesn't need Aspire resource - configured in Server project
//#endif

var webServer = builder.AddProject<Projects.CRISP_Server>("webServer")
//#if (IncludeRedis)
    .WithReference(redis)
    .WaitFor(redis)
//#endif
//#if (UsePostgres || UseSqlServer)
    .WithReference(database)
    .WaitFor(database)
//#endif
    .WithExternalHttpEndpoints();

//#if (IncludeReact)
var webApp = builder.AddViteApp("webApp", "../CRISP.Web")
    .WithReference(webServer)
    .WaitFor(webServer)
    .WithExternalHttpEndpoints();
//#endif

builder.Build().Run();
