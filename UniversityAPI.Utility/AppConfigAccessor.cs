using Microsoft.Extensions.Configuration;

namespace UniversityAPI.Utility
{
    public class AppConfigAccessor : IAppConfigAccessor
    {
        private readonly IConfiguration _configuration;

        public AppConfigAccessor(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetValue(string key) => _configuration[key];

        public IConfigurationSection GetSection(string key) => _configuration.GetSection(key);
    }
}