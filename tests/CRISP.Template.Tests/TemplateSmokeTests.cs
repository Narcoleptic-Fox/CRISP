using System.Diagnostics;

namespace CRISP.Template.Tests;

/// <summary>
/// Instantiates the template from this repository for every option combination and builds what
/// comes out. A template that is never instantiated is a comment: the conditional markers, the
/// exclusion lists and the sourceName renames are only exercised here.
/// </summary>
/// <remarks>
/// Slow (restore + build per variant, plus npm ci for the React one), so it runs in its own CI job
/// and is tagged so a local <c>dotnet test</c> can filter it out. The repository root is located by
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

    [Theory(Timeout = 900_000)]
    [InlineData("react", "SampleApp")]
    [InlineData("none", "ApiOnly")]
    public async Task Instantiate_ThenBuild_ThenRunCoreTests(string frontend, string name)
    {
        var output = Path.Combine(_work, frontend);
        Directory.CreateDirectory(output);

        await Run(
            "dotnet",
            $"new install \"{_repoRoot}\" --debug:custom-hive \"{Hive}\"",
            _repoRoot
        );
        await Run(
            "dotnet",
            $"new crisp -n {name} -o \"{output}\" --frontend {frontend} --skip-restore --debug:custom-hive \"{Hive}\"",
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

        var client = Path.Combine(output, "src", $"{name}.Client");
        if (frontend == "react")
        {
            Directory.Exists(client).ShouldBeTrue();
            File.Exists(Path.Combine(output, ".github", "workflows", "ci-client.yml"))
                .ShouldBeTrue();
            // The Server references the esproj, and the JavaScript SDK needs node_modules before it
            // evaluates the project.
            await Run("npm", "ci --no-audit --no-fund", client);
        }
        else
        {
            Directory.Exists(client).ShouldBeFalse();
            File.Exists(Path.Combine(output, ".github", "workflows", "ci-client.yml"))
                .ShouldBeFalse();
            foreach (
                var file in new[]
                {
                    $"{name}.slnx",
                    Path.Combine("src", $"{name}.Server", $"{name}.Server.csproj"),
                    Path.Combine("src", $"{name}.Core", $"{name}.Core.csproj"),
                    Path.Combine(".github", "workflows", "ci.yml"),
                }
            )
            {
                var text = await File.ReadAllTextAsync(Path.Combine(output, file));
                text.ShouldNotContain("#if", customMessage: $"conditional marker left in {file}");
                text.ShouldNotContain(
                    "IncludeReact",
                    customMessage: $"conditional marker left in {file}"
                );
                text.ShouldNotContain(".esproj", customMessage: $"client reference left in {file}");
            }
        }

        await Run("dotnet", $"build {name}.slnx --nologo -v q", output);
        await Run(
            "dotnet",
            $"test --project tests/{name}.Core.Tests/{name}.Core.Tests.csproj --no-build",
            output
        );

        if (frontend == "react")
        {
            Directory
                .EnumerateFiles(Path.Combine(client, "src", "api"), "*.ts")
                .Count()
                .ShouldBeGreaterThan(20, "TypeGen did not generate the client contracts");
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
