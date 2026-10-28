using Microsoft.AspNetCore.Identity;

namespace BlazorApp.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
    }

}
