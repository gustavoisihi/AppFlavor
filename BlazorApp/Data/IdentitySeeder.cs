using Microsoft.AspNetCore.Identity;

namespace BlazorApp.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
        {
            await using var scope = services.CreateAsyncScope();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { "Admin", "Usuario", "Supervisor" })
                if (!await roles.RoleExistsAsync(role))
                    await roles.CreateAsync(new IdentityRole(role));

            var email = config["Seed:AdminEmail"];
            var senha = config["Seed:AdminPassword"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha)) return;
            if (await users.FindByEmailAsync(email) is not null) return;

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Nome = "Administrador",
                Ativo = true
            };

            var result = await users.CreateAsync(admin, senha);
            if (result.Succeeded) await users.AddToRoleAsync(admin, "Admin");
        }
    }
}
