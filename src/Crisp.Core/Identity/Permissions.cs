namespace CRISP.Core.Identity;
public enum Permissions : int
{
    None = 0,


    CanReadUser,
    CanUpdateUser,
    CanDeleteUser,

    CanCreateRole,
    CanReadRole,
    CanUpdateRole,
    CanDeleteRole,

    AccessAll = int.MaxValue
}
