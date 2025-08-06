using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using UniversityAPI.Utility.Interfaces;

namespace UniversityAPI.Utility
{
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