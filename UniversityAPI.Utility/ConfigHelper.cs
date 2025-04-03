using Microsoft.Extensions.Configuration;
using System.ComponentModel;

namespace UniversityAPI.Utility
{
    public static class ConfigHelper
    {
        private static readonly Lazy<IConfiguration> _configuration = new Lazy<IConfiguration>(() =>
            throw new InvalidOperationException("Configuration not initialized"));

        public static void Initialize(IConfiguration configuration)
        {
            _ = new Lazy<IConfiguration>(() => configuration);
        }

        public static IConfiguration Configuration => _configuration.Value;

        public static T GetDefaultValue<T>(string key)
        {
            try
            {
                var value = Configuration[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    return default(T);
                }

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)value;
                }

                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    return (T)converter.ConvertFromString(value);
                }

                // Fallback to Convert.ChangeType
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public static T GetSection<T>(string sectionKey) where T : class, new()
        {
            try
            {
                return Configuration.GetSection(sectionKey).Get<T>() ?? new T();
            }
            catch
            {
                return new T();
            }
        }
    }
}