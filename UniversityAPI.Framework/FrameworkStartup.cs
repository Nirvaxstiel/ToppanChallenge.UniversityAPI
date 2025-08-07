using Microsoft.Extensions.DependencyInjection;
using UniversityAPI.Framework.Infrastructure.Transactions;

namespace UniversityAPI.Framework
{
    public static class FrameworkStartup
    {
        public static void AddFrameworkLayer(this IServiceCollection services)
        {
            services.AddScoped<ITransactionProxy, TransactionProxy>();
        }
    }
}