namespace UniversityAPI.Utility
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using UniversityAPI.Utility.Helpers;
    using UniversityAPI.Utility.Interfaces;

    public static class UtilityStartup
    {
        public static void AddUtilityLayer(this IServiceCollection services)
        {
            services.AddSingleton<IConfigHelper, ConfigHelper>();
        }

        public static void InjectStaticConfig(this IConfigHelper configHelper)
        {
            EncryptHelper.SetInstance(configHelper);
            TimeZoneHelper.SetInstance(configHelper);
        }
    }
}