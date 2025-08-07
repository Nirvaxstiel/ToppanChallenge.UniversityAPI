using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniversityAPI.Framework.Model;

namespace UniversityAPI.Framework.Infrastructure.Transactions
{
    public interface ITransactionProxy
    {
        Task<TResult> RunInTranAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> action, string contextInfo = null);
        Task RunInTranAsync(Func<ApplicationDbContext, Task> action, string contextInfo = null);
        Task<TResult> RunNoTranAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> action, string contextInfo = null);
        Task RunNoTranAsync(Func<ApplicationDbContext, Task> action, string contextInfo = null);
    }

    public class TransactionProxy : ITransactionProxy
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TransactionProxy> _logger;

        public TransactionProxy(ApplicationDbContext dbContext, ILogger<TransactionProxy> logger)
        {
            this._dbContext = dbContext;
            this._logger = logger;
        }

        public async Task<TResult> RunInTranAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> action, string contextInfo = null)
        {
            using var transaction = await this._dbContext.Database.BeginTransactionAsync();
            try
            {
                var result = await action(this._dbContext);
                await this._dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                this._logger.LogError(ex, $"Transaction failed. Context: {contextInfo}");
                throw;
            }
        }

        public async Task RunInTranAsync(Func<ApplicationDbContext, Task> action, string contextInfo = null)
        {
            using var transaction = await this._dbContext.Database.BeginTransactionAsync();
            try
            {
                await action(this._dbContext);
                await this._dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                this._logger.LogError(ex, $"Transaction failed. Context: {contextInfo}");
                throw;
            }
        }

        public async Task<TResult> RunNoTranAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> action, string contextInfo = null)
        {
            try
            {
                return await action(this._dbContext);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Non-transactional operation failed. Context: {contextInfo}");
                throw;
            }
        }

        public async Task RunNoTranAsync(Func<ApplicationDbContext, Task> action, string contextInfo = null)
        {
            try
            {
                await action(this._dbContext);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Non-transactional operation failed. Context: {contextInfo}");
                throw;
            }
        }
    }
}
