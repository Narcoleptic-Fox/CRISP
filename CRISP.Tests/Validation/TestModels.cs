namespace CRISP.Tests.Validation;

/// <summary>
/// Test model representing a user.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the user's ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user's username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's age.
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Gets or sets the user's roles.
    /// </summary>
    public List<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the user's address.
    /// </summary>
    public Address? Address { get; set; }
}

/// <summary>
/// Test model representing an address.
/// </summary>
public class Address
{
    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ZIP or postal code.
    /// </summary>
    public string ZipCode { get; set; } = string.Empty;
}