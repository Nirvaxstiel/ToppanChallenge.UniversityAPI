namespace UniversityAPI.Framework.Infrastructure.Transactions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using UniversityAPI.Framework.Database;
    using UniversityAPI.Framework.Model;

    public interface ITransactionProxy
    {
        Task<TResult> RunInTranAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> action, string contextInfo = null);
        Task RunInTranAsync(Func<ApplicationDbContext, Task> action, string contextInfo = null);
        Task<TResult> RunNoTranAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> action, string contextInfo = null);
        Task RunNoTranAsync(Func<ApplicationDbContext, Task> action, string contextInfo = null);
    }

    public class TransactionProxy(ApplicationDbContext dbContext, ILogger<TransactionProxy> logger) : ITransactionProxy
    {
        public async Task<TResult> RunInTranAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> action, string contextInfo = null)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var result = await action(dbContext);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, $"Transaction failed. Context: {contextInfo}");
                throw;
            }
        }

        public async Task RunInTranAsync(Func<ApplicationDbContext, Task> action, string contextInfo = null)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                await action(dbContext);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, $"Transaction failed. Context: {contextInfo}");
                throw;
            }
        }

        public async Task<TResult> RunNoTranAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> action, string contextInfo = null)
        {
            try
            {
                return await action(dbContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Non-transactional operation failed. Context: {contextInfo}");
                throw;
            }
        }

        public async Task RunNoTranAsync(Func<ApplicationDbContext, Task> action, string contextInfo = null)
        {
            try
            {
                await action(dbContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Non-transactional operation failed. Context: {contextInfo}");
                throw;
            }
        }
    }
}
