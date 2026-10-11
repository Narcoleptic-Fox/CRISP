using System.Text.RegularExpressions;

namespace CRISP.Server.Data;

/// <summary>
/// Expands <c>${NAME}</c> tokens in a connection string from environment variables, so a committed
/// settings file can name the secret without carrying it.
/// </summary>
/// <remarks>
/// <c>appsettings.Development.json</c> ships with <c>Password=${DBPASS}</c>. .NET configuration does
/// not interpolate environment variables inside JSON values, so the expansion happens here, once,
/// where the connection string is read. A token whose variable is unset is an error that names the
/// variable: a connection attempt with a literal <c>${DBPASS}</c> would fail later with a message
/// about authentication, which is the wrong clue.
/// </remarks>
public static partial class ConnectionStringExpansion
{
    [GeneratedRegex(@"\$\{(?<name>[A-Za-z_][A-Za-z0-9_]*)\}")]
    private static partial Regex Token();

    /// <summary>Replaces every <c>${NAME}</c> with the value of environment variable <c>NAME</c>.</summary>
    /// <param name="connectionString">The connection string, possibly containing tokens.</param>
    /// <param name="environment">Where to read variables from; defaults to the process environment.</param>
    /// <returns>The expanded connection string.</returns>
    /// <exception cref="InvalidOperationException">A token names a variable that is not set.</exception>
    public static string Expand(string connectionString, Func<string, string?>? environment = null)
    {
        var read = environment ?? Environment.GetEnvironmentVariable;
        return Token()
            .Replace(
                connectionString,
                match =>
                {
                    var name = match.Groups["name"].Value;
                    return read(name)
                        ?? throw new InvalidOperationException(
                            $"The connection string references ${{{name}}} but the environment variable '{name}' is not set."
                        );
                }
            );
    }
}
