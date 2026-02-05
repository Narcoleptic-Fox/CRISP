namespace CRISP.Core.Identity;
public interface IUserService :
    IQueryService<SingularQuery<User>, User>,
    IQueryService<GetUserByEmail, User>,
    IQueryService<GetUsers, PagedResponse<Users>>,
    IModifyService<UpdateUser>,
    IModifyService<DeleteCommand>
{
}
