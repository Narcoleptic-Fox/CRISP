using System.Diagnostics;

namespace CRISP.Template.Tests;

/// <summary>
/// Instantiates the template from this repository for a matrix of option combinations and builds
/// what comes out. A template that is never instantiated is a comment: the conditional markers,
/// the exclusion lists and the sourceName renames are only exercised here.
/// </summary>
/// <remarks>
/// Slow (restore + build per variant, plus npm ci for React ones), so it runs in its own CI job and
/// is tagged so a local <c>dotnet test</c> can filter it out. The repository root is located by
/// walking up from the test binary until <c>.template.config</c> is found, and each test installs
/// the template into a throwaway hive so the developer's own template cache is never touched.
/// </remarks>
[Trait("Category", "Template")]
public sealed class TemplateSmokeTests : IDisposable
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromMinutes(10);

    private readonly string _repoRoot = FindRepoRoot();
    private readonly string _work = Path.Combine(
        Path.GetTempPath(),
        "crisp-template-tests",
        Guid.NewGuid().ToString("N")
    );

    private string Hive => Path.Combine(_work, "hive");

    /// <summary>
    /// The matrix. Every non-default value appears at least once against defaults, plus the
    /// "everything off" corner, so each conditional is exercised in both states without paying for
    /// the full cross product.
    /// </summary>
    public static TheoryData<Variant> Variants() =>
        [
            // Defaults, and each frontend/sample state against them.
            new Variant("SampleApp", Frontend: "react", IncludeSample: true, Database: "postgres", Auth: "entra", Aspire: false),
            new Variant("ApiOnly", Frontend: "none", IncludeSample: true, Database: "postgres", Auth: "entra", Aspire: false),
            new Variant("Blank", Frontend: "react", IncludeSample: false, Database: "postgres", Auth: "entra", Aspire: false),
            new Variant("Bare", Frontend: "none", IncludeSample: false, Database: "postgres", Auth: "entra", Aspire: false),
            // Each other provider, with its own integration suite run against a real instance: the
            // provider class, the scaffolded migration and the test database are only proven there.
            // Aspire adds two projects and a wiring line in Program; a build is the check.
            new Variant("WithAspire", Frontend: "react", IncludeSample: true, Database: "postgres", Auth: "entra", Aspire: true),
            new Variant("OnMySql", Frontend: "none", IncludeSample: true, Database: "mysql", Auth: "entra", Aspire: false, RunServerTests: true),
            new Variant("OnSqlServer", Frontend: "none", IncludeSample: true, Database: "sqlserver", Auth: "entra", Aspire: false, RunServerTests: true),
            new Variant("OnSqlite", Frontend: "none", IncludeSample: true, Database: "sqlite", Auth: "entra", Aspire: false, RunServerTests: true),
        ];

    public sealed record Variant(
        string Name,
        string Frontend,
        bool IncludeSample,
        string Database,
        string Auth,
        bool Aspire,
        bool RunServerTests = false
    )
    {
        public bool React => Frontend == "react";

        public string Arguments =>
            $"--frontend {Frontend} --include-sample {IncludeSample.ToString().ToLowerInvariant()} "
            + $"--database {Database} --auth {Auth} --aspire {Aspire.ToString().ToLowerInvariant()}";

        public override string ToString() => $"{Name} ({Arguments})";
    }

    [Theory(Timeout = 900_000)]
    [MemberData(nameof(Variants))]
    public async Task Instantiate_ThenBuild_ThenRunCoreTests(Variant variant)
    {
        var name = variant.Name;
        var output = Path.Combine(_work, name);
        Directory.CreateDirectory(output);

        await Run(
            "dotnet",
            $"new install \"{_repoRoot}\" --debug:custom-hive \"{Hive}\"",
            _repoRoot
        );
        await Run(
            "dotnet",
            $"new crisp -n {name} -o \"{output}\" {variant.Arguments} --skip-restore --debug:custom-hive \"{Hive}\"",
            _repoRoot
        );

        // What the template must not leak, whatever the options.
        Directory.Exists(Path.Combine(output, ".template.config")).ShouldBeFalse();
        Directory.Exists(Path.Combine(output, ".memory_bank")).ShouldBeFalse();
        Directory.Exists(Path.Combine(output, "docs", "reviews")).ShouldBeFalse();
        File.Exists(Path.Combine(output, "CRISP.Templates.csproj")).ShouldBeFalse();
        Directory
            .EnumerateFiles(output, "*.cs", SearchOption.AllDirectories)
            .Any(file => Path.GetFileName(file).Contains("CRISP", StringComparison.Ordinal))
            .ShouldBeFalse("sourceName rename missed a file name");
        foreach (
            var file in Directory
                .EnumerateFiles(output, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                    Path.GetExtension(f)
                        is ".cs"
                            or ".csproj"
                            or ".slnx"
                            or ".yml"
                            or ".ts"
                            or ".tsx"
                            or ".md"
                            or ".props"
                )
        )
        {
            var text = await File.ReadAllTextAsync(file);
            text.ShouldNotContain("#if", customMessage: $"conditional marker left in {file}");
            text.ShouldNotContain("#endif", customMessage: $"conditional marker left in {file}");
        }

        var server = Path.Combine(output, "src", $"{name}.Server");
        var client = Path.Combine(output, "src", $"{name}.Client");

        // --frontend
        Directory.Exists(client).ShouldBe(variant.React);
        File.Exists(Path.Combine(output, ".github", "workflows", "ci-client.yml"))
            .ShouldBe(variant.React);
        if (!variant.React)
        {
            foreach (
                var file in new[]
                {
                    $"{name}.slnx",
                    Path.Combine(server, $"{name}.Server.csproj"),
                    Path.Combine("src", $"{name}.Core", $"{name}.Core.csproj"),
                }
            )
                (await File.ReadAllTextAsync(Path.Combine(output, file))).ShouldNotContain(
                    ".esproj",
                    customMessage: $"client reference left in {file}"
                );
        }

        // --include-sample
        Directory
            .Exists(Path.Combine(output, "src", $"{name}.Core", "Items"))
            .ShouldBe(variant.IncludeSample);
        Directory.Exists(Path.Combine(server, "Features", "Items")).ShouldBe(variant.IncludeSample);
        File.Exists(Path.Combine(server, "Data", "Entities", "ItemEntity.cs"))
            .ShouldBe(variant.IncludeSample);
        File.Exists(
                Path.Combine(
                    output,
                    "tests",
                    $"{name}.Server.Tests",
                    "Integration",
                    "ItemEndpointTests.cs"
                )
            )
            .ShouldBe(variant.IncludeSample);
        if (variant.React)
        {
            Directory
                .Exists(Path.Combine(client, "src", "features", "items"))
                .ShouldBe(variant.IncludeSample);
            (await File.ReadAllTextAsync(Path.Combine(client, "src", "routes", "index.tsx")))
                .Contains("ItemsPage")
                .ShouldBe(variant.IncludeSample);
        }

        if (variant.React)
        {
            // The Server references the esproj, and the JavaScript SDK needs node_modules before it
            // evaluates the project.
            await Run("npm", "ci --no-audit --no-fund", client);
        }

        if (!variant.IncludeSample || variant.Database != "postgres")
        {
            // No migration ships for this combination. The generated solution must be able to make
            // its own from the design-time factory, with no database running.
            Directory.Exists(Path.Combine(server, "Data", "Migrations")).ShouldBeFalse();
            await Run("dotnet", "tool restore", output);
            // The design-time factory lives in the Server assembly; it has to exist before ef can load it.
            await Run("dotnet", $"build \"{server}\" --nologo -v q", output);
            await Run(
                "dotnet",
                $"ef migrations add InitialCreate --project \"{server}\" --startup-project \"{server}\" --output-dir Data/Migrations",
                output
            );
            Directory
                .EnumerateFiles(Path.Combine(server, "Data", "Migrations"), "*_InitialCreate.cs")
                .ShouldNotBeEmpty();
        }

        // --database: exactly one provider folder survives, on both sides.
        foreach (var provider in new[] { "MySql", "Postgres", "SqlServer", "Sqlite" })
        {
            var expected = string.Equals(provider, variant.Database, StringComparison.OrdinalIgnoreCase);
            Directory.Exists(Path.Combine(server, "Data", "Providers", provider)).ShouldBe(expected, $"server provider folder {provider}");
            Directory.Exists(Path.Combine(output, "tests", $"{name}.Server.Tests", "TestSupport", "Providers", provider)).ShouldBe(expected, $"test provider folder {provider}");
        }

        // --aspire
        Directory.Exists(Path.Combine(output, "src", $"{name}.AppHost")).ShouldBe(variant.Aspire);
        Directory.Exists(Path.Combine(output, "src", $"{name}.ServiceDefaults")).ShouldBe(variant.Aspire);
        (await File.ReadAllTextAsync(Path.Combine(server, "Program.cs"))).Contains("AddServiceDefaults").ShouldBe(variant.Aspire);

        await Run("dotnet", $"build {name}.slnx --nologo -v q", output);
        await Run("dotnet", $"test --project tests/{name}.Core.Tests/{name}.Core.Tests.csproj --no-build", output);

        if (variant.RunServerTests)
        {
            // Needs Docker (Testcontainers) for every provider but SQLite. This is the only place the
            // non-default providers' runtime behaviour is exercised.
            await Run("dotnet", $"test --project tests/{name}.Server.Tests/{name}.Server.Tests.csproj --no-build", output);
        }

        if (variant.React)
        {
            Directory
                .EnumerateFiles(Path.Combine(client, "src", "api"), "*.ts")
                .Count()
                .ShouldBeGreaterThan(10, "TypeGen did not generate the client contracts");
            await Run("npx", "tsc --noEmit", client);
        }
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_work))
                Directory.Delete(_work, recursive: true);
        }
        catch (IOException)
        {
            // node_modules on Windows occasionally holds a handle a moment too long; a stale temp
            // folder is not a test failure.
        }
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".template.config")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not find the repository root (no .template.config above the test binary)."
        );
    }

    private static async Task Run(string command, string arguments, string workingDirectory)
    {
        // npm and npx are .cmd shims on Windows and cannot be started directly.
        var viaShell = OperatingSystem.IsWindows() && command is "npm" or "npx";
        var info = new ProcessStartInfo
        {
            FileName = viaShell ? "cmd.exe" : command,
            Arguments = viaShell ? $"/c {command} {arguments}" : arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        info.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        info.Environment["DOTNET_NOLOGO"] = "1";

        using var process =
            Process.Start(info)
            ?? throw new InvalidOperationException($"Could not start {command}.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(CommandTimeout);
        await process.WaitForExitAsync(timeout.Token);

        var output = await stdout + await stderr;
        process.ExitCode.ShouldBe(
            0,
            $"`{command} {arguments}` in {workingDirectory} failed:\n{output}"
        );
    }
}
