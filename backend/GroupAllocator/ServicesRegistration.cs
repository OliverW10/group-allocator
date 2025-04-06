using GroupAllocator.Database;
using GroupAllocator.Services;

namespace GroupAllocator;

public static class ServicesRegistration
{
    public static void RegisterApplicationServices(this IServiceCollection collection)
    {
        collection
            .AddScoped<IUserService, UserService>()
            .AddScoped<IAutheticationService, AuthenticationService>()
            .AddScoped<ApplicationDbContext>()
            ;
    }
}
