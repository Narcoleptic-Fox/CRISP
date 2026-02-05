using TypeGen.Core.SpecGeneration;
using CRISP.Core.Common;
using CRISP.Core.Identity;
using CRISP.Core.Todos;

namespace CRISP.Core;

/// <summary>
/// TypeGen specification for generating TypeScript types from C# contracts.
/// Run: dotnet-typegen generate
/// Output: src/CRISP.Web/src/types/
/// </summary>
public class TypeGenSpec : GenerationSpec
{
    public TypeGenSpec()
    {
        // Common types - use interfaces for cleaner TS output
        AddInterface<ICommand>();
        AddInterface(typeof(ICommand<>));
        AddInterface<CreateCommand>();
        AddInterface<ModifyCommand>();
        AddInterface<DeleteCommand>();
        AddInterface<ArchiveCommand>();
        AddInterface(typeof(PagedResponse<>));
        AddInterface<BaseModel>();
        AddInterface(typeof(PagedQuery<>));
        AddInterface(typeof(IQuery<>));

        // Identity models
        AddInterface<User>();
        AddInterface<Users>();
        AddInterface<Role>();
        AddInterface<Roles>();
        
        // User commands
        AddInterface<UpdateUser>();
        
        // Role commands  
        AddInterface<CreateRole>();
        AddInterface<UpdateRole>();
        
        // User queries
        AddInterface<GetUserByEmail>();
        AddInterface<GetUsers>();
        
        // Role queries
        AddInterface<GetRoleByName>();
        AddInterface<GetRoles>();
        
        // Todo models
        AddInterface<Todos.Todo>();
        AddInterface<Todos.Todos>();
        
        // Todo commands
        AddInterface<CreateTodo>();
        AddInterface<UpdateTodo>();
        AddInterface<CompleteTodo>();
        AddInterface<UncompleteTodo>();
        
        // Todo queries
        AddInterface<GetTodos>();
        
        // Enums - use const enum for erasableSyntaxOnly compatibility
        AddEnum<Permissions>().StringInitializers();
    }
}
