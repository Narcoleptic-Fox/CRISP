using System.Text;

namespace CRISP.Core.Identity;

public sealed record GetUserByEmail : IQuery<User>
{
    public GetUserByEmail() { }
    public GetUserByEmail(string email)
    {
        Email = email;
    }
    public string Email { get; set; } = string.Empty;
}

public sealed record GetUsers : PagedQuery<Users>
{
    public IEnumerable<string>? Emails { get; set; }
    public IEnumerable<string>? UserNames { get; set; }
    public IEnumerable<string>? PhoneNumbers { get; set; }
    public bool? LockedOut { get; set; }

    public override string ToQueryString()
    {
        var builder = new StringBuilder(base.ToQueryString());
        if (Emails is not null && Emails.Any())
        {
            builder.Append($"&emails={string.Join(',', Emails)}");
        }
        if (UserNames is not null && UserNames.Any())
        {
            builder.Append($"&usernames={string.Join(',', UserNames)}");
        }
        if (PhoneNumbers is not null && PhoneNumbers.Any())
        {
            builder.Append($"&phoneNumbers={string.Join(',', PhoneNumbers)}");
        }
        if (LockedOut.HasValue)
        {
            builder.Append($"&lockedOut={LockedOut.Value}");
        }
        return builder.ToString();
    }
}
