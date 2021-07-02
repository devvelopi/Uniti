using System.Threading.Tasks;

namespace Uniti
{
    /// <summary>
    /// Provides functionality to being, commit and rollback operations
    /// </summary>
    public interface ITransactional
    {
        /// <summary>
        /// Start the transaction
        /// </summary>
        /// <returns>Awaitable task</returns>
        Task StartAsync();

        /// <summary>
        /// Commit the transaction
        /// </summary>
        /// <param name="autoRollback"></param>
        /// <returns>Awaitable task</returns>
        Task CommitAsync(bool autoRollback = true);
        
        /// <summary>
        /// Rollback the transaction, returning to status at the start of the transaction
        /// </summary>
        /// <returns>Awaitable task</returns>
        Task RollbackAsync();
    }
}