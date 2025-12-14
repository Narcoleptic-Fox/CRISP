using CRISP.Client.Common;
using CRISP.Core.Identity;

namespace CRISP.Client.Identity.State;

public class RoleTableState : BaseState, IState
{
    public static bool LocalStorage => true;
    public static string StorageKey => "RoleTableState";
    public IEnumerable<string>? Names { get; set; }
    public IEnumerable<Permissions>? Permissions { get; set; }
}
