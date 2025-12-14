using CRISP.Client.Common;

namespace CRISP.Client.Identity.State;

public class UserTableState : BaseState, IState
{
    public static bool LocalStorage => true;
    public static string StorageKey => "UserTableState";

    public IEnumerable<string>? Emails { get; set; }
    public IEnumerable<string>? UserNames { get; set; }
    public IEnumerable<string>? PhoneNumbers { get; set; }
    public bool? LockedOut { get; set; }
}
