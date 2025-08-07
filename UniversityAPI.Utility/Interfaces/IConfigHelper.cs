namespace UniversityAPI.Utility.Interfaces
{
    public interface IConfigHelper
    {
        T GetPublicCipherKey<T>();

        T GetJwtKey<T>();

        T GetDbConnection<T>();

        T GetAdminInitUsername<T>();

        T GetAdminInitPassword<T>();

        T GetAdminInitEmail<T>();

        T GetValue<T>(string key);

        T ConvertValue<T>(string value);

        T GetConfigSection<T>(string sectionName)
            where T : new();
    }
}