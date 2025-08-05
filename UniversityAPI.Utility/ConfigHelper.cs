using Microsoft.Extensions.Configuration;
using System.ComponentModel;

namespace UniversityAPI.Utility
{
    public static class ConfigHelper
    {
        private const string PUBLIC_KEY_NAME = "TOPPAN_UNIVERSITYAPI_PUBLIC_KEY";
        private const string JWT_KEY_KEY_NAME = "TOPPAN_UNIVERSITYAPI_PUBLIC_KEY";
        private const string DB_CONNECTION_KEY_NAME = "TOPPAN_UNIVERSITYAPI_PUBLIC_KEY";
        private const string ADMIN_INIT_USERNAME_KEY_NAME = "TOPPAN_UNIVERSITYAPI_PUBLIC_KEY";
        private const string ADMIN_INIT_PASSWORD_KEY_NAME = "TOPPAN_UNIVERSITYAPI_PUBLIC_KEY";
        private const string ADMIN_INIT_EMAIL_KEY_NAME = "TOPPAN_UNIVERSITYAPI_PUBLIC_KEY";
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static IConfiguration Configuration => _configuration ?? throw new InvalidOperationException("Configuration not initialized");

        /// <summary>
        /// Gets a configuration value with automatic fallback to environment variables.
        /// Priority: 1. Dotnet User-Secrets/Configuration, 2. Environment Variables, 3. Default value
        /// </summary>
        public static T GetValue<T>(string key, T defaultValue = default(T))
        {
            try
            {
                // First, try to get from configuration (dotnet user-secrets, appsettings, etc.)
                var configValue = Configuration[key];
                if (!string.IsNullOrWhiteSpace(configValue))
                {
                    return ConvertValue<T>(configValue);
                }

                // Second, try environment variable (convert key format: "Admin:InitialPassword" -> "ADMIN_INITIAL_PASSWORD")
                var envValue = Environment.GetEnvironmentVariable(key);
                if (!string.IsNullOrWhiteSpace(envValue))
                {
                    return ConvertValue<T>(envValue);
                }

                // Finally, return default value
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Legacy method - now uses GetValue internally
        /// </summary>
        public static T GetDefaultValue<T>(string key)
        {
            return GetValue<T>(key, default);
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

        public static T GetPublicCipherKey<T>()
        {
            return GetValue<T>(PUBLIC_KEY_NAME);
        }

        public static T GetJwtKey<T>()
        {
            return GetValue<T>(JWT_KEY_KEY_NAME);
        }

        public static T GetDbConnection<T>()
        {
            return GetValue<T>(DB_CONNECTION_KEY_NAME);
        }

        public static T GetAdminInitUsername<T>()
        {
            return GetValue<T>(ADMIN_INIT_USERNAME_KEY_NAME);
        }

        public static T GetAdminInitPassword<T>()
        {
            return GetValue<T>(ADMIN_INIT_PASSWORD_KEY_NAME);
        }

        public static T GetAdminInitEmail<T>()
        {
            return GetValue<T>(ADMIN_INIT_EMAIL_KEY_NAME);
        }

        private static T ConvertValue<T>(string value)
        {
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
    }
}