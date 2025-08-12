namespace UniversityAPI.Framework
{
    using Microsoft.Extensions.DependencyInjection;
    using UniversityAPI.Framework.Infrastructure.Transactions;

    public static class FrameworkStartup
    {
        public static void AddFrameworkLayer(this IServiceCollection services)
        {
            services.AddScoped<ITransactionProxy, TransactionProxy>();
        }
    }
}