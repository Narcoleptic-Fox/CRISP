using CRISP.Core.Common;
using CRISP.Core.Identity;

namespace CRISP.Client.Identity.Services;

internal sealed class RoleService : IRoleService
{
    public ValueTask<Guid> Send(CreateRole command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<Role> Send(SingularQuery<Role> query, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<Role> Send(GetRoleByName query, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<PagedResponse<Roles>> Send(GetRoles query, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask Send(UpdateRole command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask Send(DeleteCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
