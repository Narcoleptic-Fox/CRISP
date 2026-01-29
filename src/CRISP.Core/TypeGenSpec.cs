using TypeGen.Core.SpecGeneration;
using CRISP.Core.Common;
using CRISP.Core.Identity;

namespace CRISP.Core;

/// <summary>
/// TypeGen specification for generating TypeScript types from C# contracts.
/// Run: dotnet typegen generate
/// </summary>
public class TypeGenSpec : GenerationSpec
{
    public TypeGenSpec()
    {
        // Common types
        AddInterface<ICommand>();
        AddInterface(typeof(ICommand<>));
        AddClass<CreateCommand>();
        AddClass<ModifyCommand>();
        AddClass<DeleteCommand>();
        AddClass<ArchiveCommand>();
        AddClass(typeof(PagedResponse<>));

        // Identity contracts
        AddClass<User>();
        AddClass<Users>();
        AddClass<Role>();
        AddClass<Roles>();
        
        // User commands
        AddClass<UpdateUser>();
        
        // Role commands  
        AddClass<CreateRole>();
        AddClass<UpdateRole>();
        
        // User queries
        AddClass<GetUserByEmail>();
        AddClass<GetUsers>();
        
        // Role queries
        AddClass<GetRoleByName>();
        AddClass<GetRoles>();
        
        // Enums
        AddEnum<Permissions>();
    }
}
