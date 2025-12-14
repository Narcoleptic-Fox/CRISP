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
                                                             .WaitFor(database);


        builder.Build().Run();
    }
}