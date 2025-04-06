using Microsoft.Extensions.Configuration;

namespace UniversityAPI.Utility
{
    public interface IAppConfigAccessor
    {
        string GetValue(string key);

        IConfigurationSection GetSection(string key);
    }
}