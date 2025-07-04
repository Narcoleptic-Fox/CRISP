using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Crisp.AspNetCore.Security;

/// <summary>
/// Service for conducting security audits on CRISP applications.
/// Identifies potential security vulnerabilities and provides recommendations.
/// </summary>
public class SecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger;
    private readonly SecurityAuditOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityAuditService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">Security audit configuration options.</param>
    public SecurityAuditService(
        ILogger<SecurityAuditService> logger,
        SecurityAuditOptions options)
    {
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Performs a comprehensive security audit.
    /// </summary>
    /// <param name="assemblies">Assemblies to audit.</param>
    /// <returns>Security audit report.</returns>
    public SecurityAuditReport PerformAudit(params Assembly[] assemblies)
    {
        var report = new SecurityAuditReport
        {
            AuditTimestamp = DateTime.UtcNow,
            AuditedAssemblies = assemblies.Select(a => a.FullName ?? "Unknown").ToList()
        };

        _logger.LogInformation("Starting security audit for {AssemblyCount} assemblies", assemblies.Length);

        // Check for common security vulnerabilities
        CheckForSqlInjectionVulnerabilities(assemblies, report);
        CheckForXssVulnerabilities(assemblies, report);
        CheckForInsecureDeserialization(assemblies, report);
        CheckForWeakCryptography(assemblies, report);
        CheckForInsecureLogging(assemblies, report);
        CheckForMissingInputValidation(assemblies, report);
        CheckForInsecureDirectObjectReferences(assemblies, report);
        CheckForMissingAuthorizationChecks(assemblies, report);

        // Calculate overall security score
        report.SecurityScore = CalculateSecurityScore(report);

        _logger.LogInformation("Security audit completed. Score: {SecurityScore}/100", report.SecurityScore);

        return report;
    }

    private void CheckForSqlInjectionVulnerabilities(Assembly[] assemblies, SecurityAuditReport report)
    {
        var vulnerabilities = new List<SecurityVulnerability>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                foreach (var method in methods)
                {
                    if (method.GetMethodBody() != null)
                    {
                        // Check for potential SQL injection patterns
                        var methodSource = GetMethodSource(method);
                        if (ContainsSqlInjectionPattern(methodSource))
                        {
                            vulnerabilities.Add(new SecurityVulnerability
                            {
                                Type = SecurityVulnerabilityType.SqlInjection,
                                Severity = SecuritySeverity.High,
                                Location = $"{type.FullName}.{method.Name}",
                                Description = "Potential SQL injection vulnerability detected",
                                Recommendation = "Use parameterized queries or ORM frameworks"
                            });
                        }
                    }
                }
            }
        }

        report.Vulnerabilities.AddRange(vulnerabilities);
    }

    private void CheckForXssVulnerabilities(Assembly[] assemblies, SecurityAuditReport report)
    {
        var vulnerabilities = new List<SecurityVulnerability>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                // Check for controllers and handlers that might output user data
                if (IsControllerOrHandler(type))
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var method in methods)
                    {
                        // Check for methods that return string content without encoding
                        if (method.ReturnType == typeof(string) || method.ReturnType == typeof(Task<string>))
                        {
                            vulnerabilities.Add(new SecurityVulnerability
                            {
                                Type = SecurityVulnerabilityType.CrossSiteScripting,
                                Severity = SecuritySeverity.Medium,
                                Location = $"{type.FullName}.{method.Name}",
                                Description = "Method returns string content that may be vulnerable to XSS",
                                Recommendation = "Ensure all user input is properly encoded before output"
                            });
                        }
                    }
                }
            }
        }

        report.Vulnerabilities.AddRange(vulnerabilities);
    }

    private void CheckForInsecureDeserialization(Assembly[] assemblies, SecurityAuditReport report)
    {
        var vulnerabilities = new List<SecurityVulnerability>();
        var dangerousTypes = new[] { "BinaryFormatter", "JavaScriptSerializer", "XmlSerializer" };

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                // Check for dangerous serializer usage
                foreach (var field in fields)
                {
                    if (dangerousTypes.Any(dt => field.FieldType.Name.Contains(dt)))
                    {
                        vulnerabilities.Add(new SecurityVulnerability
                        {
                            Type = SecurityVulnerabilityType.InsecureDeserialization,
                            Severity = SecuritySeverity.High,
                            Location = $"{type.FullName}.{field.Name}",
                            Description = "Usage of potentially insecure deserializer",
                            Recommendation = "Use secure serializers like System.Text.Json"
                        });
                    }
                }

                foreach (var property in properties)
                {
                    if (dangerousTypes.Any(dt => property.PropertyType.Name.Contains(dt)))
                    {
                        vulnerabilities.Add(new SecurityVulnerability
                        {
                            Type = SecurityVulnerabilityType.InsecureDeserialization,
                            Severity = SecuritySeverity.High,
                            Location = $"{type.FullName}.{property.Name}",
                            Description = "Usage of potentially insecure deserializer",
                            Recommendation = "Use secure serializers like System.Text.Json"
                        });
                    }
                }
            }
        }

        report.Vulnerabilities.AddRange(vulnerabilities);
    }

    private void CheckForWeakCryptography(Assembly[] assemblies, SecurityAuditReport report)
    {
        var vulnerabilities = new List<SecurityVulnerability>();
        var weakAlgorithms = new[] { "MD5", "SHA1", "DES", "RC2" };

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                // Check for usage of weak cryptographic algorithms
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                foreach (var method in methods)
                {
                    var methodSource = GetMethodSource(method);
                    
                    foreach (var algorithm in weakAlgorithms)
                    {
                        if (methodSource.Contains(algorithm, StringComparison.OrdinalIgnoreCase))
                        {
                            vulnerabilities.Add(new SecurityVulnerability
                            {
                                Type = SecurityVulnerabilityType.WeakCryptography,
                                Severity = SecuritySeverity.Medium,
                                Location = $"{type.FullName}.{method.Name}",
                                Description = $"Usage of weak cryptographic algorithm: {algorithm}",
                                Recommendation = "Use strong algorithms like SHA256, AES, or modern alternatives"
                            });
                        }
                    }
                }
            }
        }

        report.Vulnerabilities.AddRange(vulnerabilities);
    }

    private void CheckForInsecureLogging(Assembly[] assemblies, SecurityAuditReport report)
    {
        var vulnerabilities = new List<SecurityVulnerability>();
        var sensitivePatterns = new[]
        {
            @"password",
            @"token",
            @"key",
            @"secret",
            @"credential",
            @"ssn",
            @"social\s*security",
            @"credit\s*card"
        };

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                foreach (var method in methods)
                {
                    var methodSource = GetMethodSource(method);

                    // Check for logging of sensitive information
                    if (methodSource.Contains("Log", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var pattern in sensitivePatterns)
                        {
                            if (Regex.IsMatch(methodSource, pattern, RegexOptions.IgnoreCase))
                            {
                                vulnerabilities.Add(new SecurityVulnerability
                                {
                                    Type = SecurityVulnerabilityType.SensitiveDataExposure,
                                    Severity = SecuritySeverity.Medium,
                                    Location = $"{type.FullName}.{method.Name}",
                                    Description = "Potential logging of sensitive information",
                                    Recommendation = "Avoid logging sensitive data or implement proper data masking"
                                });
                            }
                        }
                    }
                }
            }
        }

        report.Vulnerabilities.AddRange(vulnerabilities);
    }

    private void CheckForMissingInputValidation(Assembly[] assemblies, SecurityAuditReport report)
    {
        var vulnerabilities = new List<SecurityVulnerability>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t => IsControllerOrHandler(t));

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach (var method in methods)
                {
                    var parameters = method.GetParameters();
                    
                    foreach (var parameter in parameters)
                    {
                        // Check if parameter accepts user input but lacks validation attributes
                        if (IsUserInputParameter(parameter) && !HasValidationAttributes(parameter))
                        {
                            vulnerabilities.Add(new SecurityVulnerability
                            {
                                Type = SecurityVulnerabilityType.MissingInputValidation,
                                Severity = SecuritySeverity.Medium,
                                Location = $"{type.FullName}.{method.Name}({parameter.Name})",
                                Description = "Parameter lacks input validation",
                                Recommendation = "Add validation attributes or implement custom validation"
                            });
                        }
                    }
                }
            }
        }

        report.Vulnerabilities.AddRange(vulnerabilities);
    }

    private void CheckForInsecureDirectObjectReferences(Assembly[] assemblies, SecurityAuditReport report)
    {
        var vulnerabilities = new List<SecurityVulnerability>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t => IsControllerOrHandler(t));

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach (var method in methods)
                {
                    var parameters = method.GetParameters();
                    
                    // Check for methods that accept ID parameters without authorization checks
                    var idParameters = parameters.Where(p => 
                        p.Name?.Contains("id", StringComparison.OrdinalIgnoreCase) == true ||
                        p.ParameterType == typeof(Guid) ||
                        p.ParameterType == typeof(int) ||
                        p.ParameterType == typeof(long)).ToArray();

                    if (idParameters.Any() && !HasAuthorizationAttributes(method))
                    {
                        vulnerabilities.Add(new SecurityVulnerability
                        {
                            Type = SecurityVulnerabilityType.InsecureDirectObjectReference,
                            Severity = SecuritySeverity.Medium,
                            Location = $"{type.FullName}.{method.Name}",
                            Description = "Method accepts ID parameters without authorization checks",
                            Recommendation = "Implement proper authorization checks for resource access"
                        });
                    }
                }
            }
        }

        report.Vulnerabilities.AddRange(vulnerabilities);
    }

    private void CheckForMissingAuthorizationChecks(Assembly[] assemblies, SecurityAuditReport report)
    {
        var vulnerabilities = new List<SecurityVulnerability>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t => IsControllerOrHandler(t));

            foreach (var type in types)
            {
                // Check if type has authorization at class level
                var hasClassAuth = HasAuthorizationAttributes(type);

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach (var method in methods)
                {
                    // Skip if class has authorization or method has authorization
                    if (hasClassAuth || HasAuthorizationAttributes(method))
                        continue;

                    // Check for methods that perform sensitive operations
                    if (IsSensitiveOperation(method))
                    {
                        vulnerabilities.Add(new SecurityVulnerability
                        {
                            Type = SecurityVulnerabilityType.MissingAuthentication,
                            Severity = SecuritySeverity.High,
                            Location = $"{type.FullName}.{method.Name}",
                            Description = "Sensitive operation lacks authorization checks",
                            Recommendation = "Add [Authorize] attribute or implement custom authorization"
                        });
                    }
                }
            }
        }

        report.Vulnerabilities.AddRange(vulnerabilities);
    }

    private bool ContainsSqlInjectionPattern(string methodSource)
    {
        var patterns = new[]
        {
            @"SELECT\s+.*\s+FROM\s+.*\s*\+",
            @"INSERT\s+INTO\s+.*\s*\+",
            @"UPDATE\s+.*\s+SET\s+.*\s*\+",
            @"DELETE\s+FROM\s+.*\s*\+"
        };

        return patterns.Any(pattern => Regex.IsMatch(methodSource, pattern, RegexOptions.IgnoreCase));
    }

    private bool IsControllerOrHandler(Type type)
    {
        return type.Name.EndsWith("Controller") || 
               type.Name.EndsWith("Handler") ||
               type.GetInterfaces().Any(i => i.Name.Contains("Handler"));
    }

    private bool IsUserInputParameter(ParameterInfo parameter)
    {
        return parameter.ParameterType == typeof(string) ||
               parameter.ParameterType.IsClass ||
               parameter.GetCustomAttributes().Any(a => a.GetType().Name.Contains("FromBody"));
    }

    private bool HasValidationAttributes(ParameterInfo parameter)
    {
        var validationAttributes = new[] { "Required", "StringLength", "Range", "RegularExpression" };
        return parameter.GetCustomAttributes().Any(a => 
            validationAttributes.Any(va => a.GetType().Name.Contains(va)));
    }

    private bool HasAuthorizationAttributes(MethodInfo method)
    {
        var authAttributes = new[] { "Authorize", "AllowAnonymous" };
        return method.GetCustomAttributes().Any(a => 
            authAttributes.Any(aa => a.GetType().Name.Contains(aa)));
    }

    private bool HasAuthorizationAttributes(Type type)
    {
        var authAttributes = new[] { "Authorize", "AllowAnonymous" };
        return type.GetCustomAttributes().Any(a => 
            authAttributes.Any(aa => a.GetType().Name.Contains(aa)));
    }

    private bool IsSensitiveOperation(MethodInfo method)
    {
        var sensitiveNames = new[] { "Delete", "Remove", "Update", "Create", "Admin", "Manage" };
        return sensitiveNames.Any(name => method.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    private string GetMethodSource(MethodInfo method)
    {
        // This is a simplified implementation
        // In a real scenario, you might use Roslyn or other tools to get actual source code
        return method.ToString();
    }

    private int CalculateSecurityScore(SecurityAuditReport report)
    {
        int baseScore = 100;
        
        foreach (var vulnerability in report.Vulnerabilities)
        {
            int deduction = vulnerability.Severity switch
            {
                SecuritySeverity.Critical => 20,
                SecuritySeverity.High => 10,
                SecuritySeverity.Medium => 5,
                SecuritySeverity.Low => 2,
                _ => 1
            };

            baseScore -= deduction;
        }

        return Math.Max(0, baseScore);
    }
}

/// <summary>
/// Configuration options for security auditing.
/// </summary>
public class SecurityAuditOptions
{
    /// <summary>
    /// Whether to perform deep analysis of method bodies.
    /// </summary>
    public bool DeepAnalysis { get; set; } = true;

    /// <summary>
    /// Custom security patterns to check for.
    /// </summary>
    public List<string> CustomPatterns { get; set; } = new();

    /// <summary>
    /// Whether to include low-severity findings.
    /// </summary>
    public bool IncludeLowSeverity { get; set; } = true;
}

/// <summary>
/// Security audit report containing findings and recommendations.
/// </summary>
public class SecurityAuditReport
{
    /// <summary>
    /// Timestamp when the audit was performed.
    /// </summary>
    public DateTime AuditTimestamp { get; set; }

    /// <summary>
    /// List of audited assemblies.
    /// </summary>
    public List<string> AuditedAssemblies { get; set; } = new();

    /// <summary>
    /// List of security vulnerabilities found.
    /// </summary>
    public List<SecurityVulnerability> Vulnerabilities { get; set; } = new();

    /// <summary>
    /// Overall security score (0-100).
    /// </summary>
    public int SecurityScore { get; set; }

    /// <summary>
    /// Gets vulnerabilities by severity level.
    /// </summary>
    public IEnumerable<SecurityVulnerability> GetVulnerabilitiesBySeverity(SecuritySeverity severity)
    {
        return Vulnerabilities.Where(v => v.Severity == severity);
    }

    /// <summary>
    /// Gets a summary of the audit results.
    /// </summary>
    public string GetSummary()
    {
        var critical = GetVulnerabilitiesBySeverity(SecuritySeverity.Critical).Count();
        var high = GetVulnerabilitiesBySeverity(SecuritySeverity.High).Count();
        var medium = GetVulnerabilitiesBySeverity(SecuritySeverity.Medium).Count();
        var low = GetVulnerabilitiesBySeverity(SecuritySeverity.Low).Count();

        return $"Security Score: {SecurityScore}/100\n" +
               $"Critical: {critical}, High: {high}, Medium: {medium}, Low: {low}\n" +
               $"Total Vulnerabilities: {Vulnerabilities.Count}";
    }
}

/// <summary>
/// Represents a security vulnerability found during audit.
/// </summary>
public class SecurityVulnerability
{
    /// <summary>
    /// Type of vulnerability.
    /// </summary>
    public SecurityVulnerabilityType Type { get; set; }

    /// <summary>
    /// Severity level of the vulnerability.
    /// </summary>
    public SecuritySeverity Severity { get; set; }

    /// <summary>
    /// Location where the vulnerability was found.
    /// </summary>
    public string Location { get; set; } = "";

    /// <summary>
    /// Description of the vulnerability.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Recommended remediation steps.
    /// </summary>
    public string Recommendation { get; set; } = "";
}

/// <summary>
/// Types of security vulnerabilities.
/// </summary>
public enum SecurityVulnerabilityType
{
    SqlInjection,
    CrossSiteScripting,
    InsecureDeserialization,
    WeakCryptography,
    SensitiveDataExposure,
    MissingInputValidation,
    InsecureDirectObjectReference,
    MissingAuthentication,
    MissingAuthorization,
    InsecureConfiguration
}

/// <summary>
/// Security vulnerability severity levels.
/// </summary>
public enum SecuritySeverity
{
    Low,
    Medium,
    High,
    Critical
}