using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UniversityAPI.Utility.Interfaces;

namespace UniversityAPI.Utility.Helpers
{
    public class ConfigHelper : IConfigHelper
    {
        private const string PUBLICKEYNAME = "TOPPAN_UNIVERSITYAPI_PUBLIC_KEY";
        private const string JWTKEYKEYNAME = "TOPPAN_UNIVERSITYAPI_JWT_KEY";
        private const string DBCONNECTIONKEYNAME = "TOPPAN_UNIVERSITYAPI_DB_CONNECTION";
        private const string ADMININITUSERNAMEKEYNAME = "TOPPAN_UNIVERSITYAPI_ADMIN_INIT_USERNAME";
        private const string ADMININITPASSWORDKEYNAME = "TOPPAN_UNIVERSITYAPI_ADMIN_INIT_PASSWORD";
        private const string ADMININITEMAILKEYNAME = "TOPPAN_UNIVERSITYAPI_ADMIN_INIT_EMAIL";

        private readonly IConfiguration configuration;
        private readonly ILogger<IConfiguration> logger;

        public ConfigHelper(IConfiguration configuration, ILogger<IConfiguration> logger)
        {
            this.configuration = configuration;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public T GetValue<T>(string key)
        {
            TryGetValue<T>(key, out var value);
            return value;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            try
            {
                var configValue = configuration[key];
                if (!string.IsNullOrWhiteSpace(configValue))
                {
                    value = ConvertValue<T>(configValue);
                    return true;
                }

                var envKey = key.Replace(":", "_").ToUpperInvariant();
                var envValue = Environment.GetEnvironmentVariable(envKey);
                if (!string.IsNullOrWhiteSpace(envValue))
                {
                    value = ConvertValue<T>(envValue);
                    return true;
                }

                logger.LogWarning("Configuration key '{Key}' not found in configuration or environment variables.", key);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve configuration value for key '{Key}'", key);
            }

            value = default!;
            return false;
        }

        public T GetSection<T>(string sectionKey) where T : class, new()
        {
            try
            {
                return configuration.GetSection(sectionKey).Get<T>() ?? new T();
            }
            catch
            {
                return new T();
            }
        }

        public T GetPublicCipherKey<T>()
        {
            return GetValue<T>(PUBLICKEYNAME);
        }

        public T GetJwtKey<T>()
        {
            return GetValue<T>(JWTKEYKEYNAME);
        }

        public T GetDbConnection<T>()
        {
            return GetValue<T>(DBCONNECTIONKEYNAME);
        }

        public T GetAdminInitUsername<T>()
        {
            return GetValue<T>(ADMININITUSERNAMEKEYNAME);
        }

        public T GetAdminInitPassword<T>()
        {
            return GetValue<T>(ADMININITPASSWORDKEYNAME);
        }

        public T GetAdminInitEmail<T>()
        {
            return GetValue<T>(ADMININITEMAILKEYNAME);
        }

        public T ConvertValue<T>(string value)
        {
            var targetType = typeof(T);

            if (targetType == typeof(string))
            {
                return (T)(object)value;
            }

            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Handle enums
            if (underlyingType.IsEnum)
            {
                if (Enum.TryParse(underlyingType, value, ignoreCase: true, out var enumResult))
                {
                    return (T)enumResult;
                }

                throw new InvalidCastException($"Cannot convert '{value}' to enum type '{underlyingType.Name}'.");
            }

            // Use TypeConverter if available
            var converter = TypeDescriptor.GetConverter(underlyingType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                return (T)converter.ConvertFromString(value)!;
            }

            // Fallback to Convert.ChangeType
            return (T)Convert.ChangeType(value, underlyingType);
        }

        public T GetConfigSection<T>(string sectionName)
            where T : new()
        {
            var section = new T();
            configuration.GetSection(sectionName).Bind(section);
            return section;
        }
    }
}