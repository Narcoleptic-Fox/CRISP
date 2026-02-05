namespace CRISP.Core.Identity;
public interface IRoleService :
    ICreateService<CreateRole>,
    IQueryService<SingularQuery<Role>, Role>,
    IQueryService<GetRoleByName, Role>,
    IQueryService<GetRoles, PagedResponse<Roles>>,
    IModifyService<UpdateRole>,
    IModifyService<DeleteCommand>
{
}
