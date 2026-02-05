using CRISP.ServiceDefaults.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace CRISP.Server.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<Guid>, ISoftDelete
    {
        public bool IsDeleted { get; set; }
    }
}
