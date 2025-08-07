using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UniversityAPI.Framework.Infrastructure.Transactions
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class TransactionalAttribute(bool useTransaction = true) : Attribute, IAsyncActionFilter
    {
        public bool UseTransaction { get; } = useTransaction;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<TransactionalAttribute>>();

            if (!this.UseTransaction)
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Non-transactional action failed.");
                    throw;
                }
                return;
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var resultContext = await next();
                if (resultContext.Exception == null || resultContext.ExceptionHandled)
                {
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.RollbackAsync();
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Transactional action failed.");
                throw;
            }
        }
    }
}