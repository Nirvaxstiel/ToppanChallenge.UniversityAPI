using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UniversityAPI.Service
{
    public static class ServiceStartup
    {
        public static void AddServiceLayer(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUniversityService, UniversityService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
        }
    }
}