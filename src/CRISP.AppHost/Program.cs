internal class Program
{
    private static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        IResourceBuilder<RedisResource> redis = builder.AddRedis("redis");
        IResourceBuilder<PostgresServerResource> database = builder.AddPostgres("postgres");

        IResourceBuilder<ProjectResource> webServer = builder.AddProject<Projects.CRISP_Server>("webServer")
                                                             .WithReference(redis)
                                                             .WaitFor(redis)
                                                             .WithReference(database)
                                                             .WaitFor(database)
                                                             .WithExternalHttpEndpoints();

        // Note: React frontend (CRISP.Web) runs separately via `npm run dev`
        // Aspire 13.x Node.js support requires CLI integration (aspire init)
        // TODO: Migrate to Aspire CLI for full stack orchestration

        builder.Build().Run();
    }
}
