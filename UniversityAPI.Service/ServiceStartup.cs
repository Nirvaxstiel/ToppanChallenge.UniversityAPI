namespace UniversityAPI.Service
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using UniversityAPI.Service.Authentication;
    using UniversityAPI.Service.Authentication.Interface;
    using UniversityAPI.Service.University;
    using UniversityAPI.Service.University.Interface;
    using UniversityAPI.Service.User;
    using UniversityAPI.Service.User.Interface;

    public static class ServiceStartup
    {
        public static void AddServiceLayer(this IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUniversityService, UniversityService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
        }
    }
}