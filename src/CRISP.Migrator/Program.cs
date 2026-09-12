using CRISP.Server.Data;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace CRISP.Migrator;

/// <summary>
/// Applies EF Core migrations out-of-band. The server never migrates at startup: a deploy that
/// changes the schema is a separate, deliberate step with its own log line and exit code.
/// </summary>
/// <remarks>
/// Connects directly by default. When <c>BASTION_HOST</c> is set, opens an SSH tunnel through the
/// bastion and reaches the database over it, which is how a private RDS instance is migrated from
/// a developer machine or a CI runner outside the VPC.
/// </remarks>
internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        Env.TraversePath().Load(".env");
        Env.TraversePath().Load(".env.local");

        var builder = Host.CreateApplicationBuilder(args);
        // Each migration already logs as it applies; echoing every SQL statement buries the
        // summary lines this tool exists to print.
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        var log = LoggerFactory
            .Create(b => b.AddSimpleConsole(o => o.SingleLine = true))
            .CreateLogger("Migrator");

        var target = ResolveTarget(builder.Configuration);
        if (target is null)
        {
            log.LogError(
                "No database configured. Set ConnectionStrings__default, or DATABASE_HOST/DATABASE_NAME/DATABASE_USER/DATABASE_PASSWORD. See .env.example."
            );
            return 2;
        }

        // The tunnel, if any, has to be up before AddDatabaseInfrastructure runs: it probes the
        // server version once at registration.
        using var tunnel = OpenTunnelIfConfigured(log, ref target);

        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:default"] = target.ConnectionString,
            }
        );
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDatabaseInfrastructure(builder.Configuration);

        using var host = builder.Build();
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
        log.LogInformation(
            "Target {Description}: {Count} pending migration(s).",
            target.Description,
            pending.Count
        );
        foreach (var migration in pending)
            log.LogInformation("  {Migration}", migration);

        await db.Database.MigrateAsync();

        // Exit 0 is not a receipt. Ask the database what it now has.
        var stillPending = (await db.Database.GetPendingMigrationsAsync()).ToList();
        if (stillPending.Count > 0)
        {
            log.LogError(
                "{Count} migration(s) still pending after MigrateAsync: {Migrations}",
                stillPending.Count,
                string.Join(", ", stillPending)
            );
            return 1;
        }

        var applied = (await db.Database.GetAppliedMigrationsAsync()).ToList();
        log.LogInformation(
            "Done. {Applied} migration(s) applied in total; latest is {Latest}.",
            applied.Count,
            applied.LastOrDefault() ?? "(none)"
        );
        return 0;
    }

    /// <summary>
    /// Resolves where to migrate. A full connection string wins; otherwise the parts are assembled.
    /// </summary>
    private static Target? ResolveTarget(IConfiguration configuration)
    {
        var explicitConnectionString = configuration.GetConnectionString("default");
        if (!string.IsNullOrWhiteSpace(explicitConnectionString))
            return new Target(
                explicitConnectionString,
                "ConnectionStrings:default",
                Host: null,
                Port: 0
            );

        var host = Env.GetString("DATABASE_HOST");
        var name = Env.GetString("DATABASE_NAME");
        var user = Env.GetString("DATABASE_USER");
        var password = Env.GetString("DATABASE_PASSWORD");
        if (
            string.IsNullOrWhiteSpace(host)
            || string.IsNullOrWhiteSpace(name)
            || string.IsNullOrWhiteSpace(user)
        )
            return null;

        var port = (uint)Env.GetInt("DATABASE_PORT", 3306);
        return new Target(
            $"server={host};port={port};database={name};uid={user};pwd={password};",
            $"{host}:{port}/{name}",
            host,
            port
        );
    }

    /// <summary>
    /// Opens an SSH tunnel when a bastion is configured and rewrites the target to go through it.
    /// </summary>
    /// <returns>The tunnel to dispose when done, or <c>null</c> when connecting directly.</returns>
    private static IDisposable? OpenTunnelIfConfigured(ILogger log, ref Target target)
    {
        var bastionHost = Env.GetString("BASTION_HOST");
        if (string.IsNullOrWhiteSpace(bastionHost))
            return null;

        if (target.Host is null)
            throw new InvalidOperationException(
                "BASTION_HOST is set but the target was given as a full connection string; use DATABASE_HOST/PORT so the tunnel knows what to forward to."
            );

        var bastionUser = Env.GetString("BASTION_USERNAME");
        var bastionKey = Env.GetString("BASTION_KEY_PATH");
        if (!File.Exists(bastionKey))
            throw new FileNotFoundException(
                $"BASTION_KEY_PATH '{bastionKey}' does not exist.",
                bastionKey
            );

        var localPort = (uint)Env.GetInt("LOCAL_FORWARD_PORT", 33700);

        log.LogInformation(
            "Opening SSH tunnel to {Host}:{Port} via {Bastion} on local port {LocalPort}.",
            target.Host,
            target.Port,
            bastionHost,
            localPort
        );

        var ssh = new SshClient(bastionHost, bastionUser, new PrivateKeyFile(bastionKey));
        ssh.Connect();
        var forwarded = new ForwardedPortLocal("127.0.0.1", localPort, target.Host, target.Port);
        ssh.AddForwardedPort(forwarded);
        forwarded.Start();

        var name = Env.GetString("DATABASE_NAME");
        var user = Env.GetString("DATABASE_USER");
        var password = Env.GetString("DATABASE_PASSWORD");
        target = target with
        {
            ConnectionString =
                $"server=127.0.0.1;port={localPort};database={name};uid={user};pwd={password};SslMode=None;",
            Description = $"{target.Description} via {bastionHost}",
        };

        return new Tunnel(ssh, forwarded);
    }

    private sealed record Target(
        string ConnectionString,
        string Description,
        string? Host,
        uint Port
    );

    private sealed class Tunnel(SshClient ssh, ForwardedPortLocal port) : IDisposable
    {
        public void Dispose()
        {
            port.Stop();
            port.Dispose();
            ssh.Disconnect();
            ssh.Dispose();
        }
    }
}
