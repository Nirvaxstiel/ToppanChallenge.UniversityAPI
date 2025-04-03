using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UniversityAPI.Utility
{
    public static class UtilityStartup
    {
        public static void AddUtilityLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IAppConfigAccessor, AppConfigAccessor>();
            ConfigHelper.Initialize(configuration);
        }
    }
}