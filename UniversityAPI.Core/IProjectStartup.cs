using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UniversityAPI.Core
{
    public interface IProjectStartup
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
}
