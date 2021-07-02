using System;
using System.Threading.Tasks;

namespace Uniti
{
    /// <summary>
    /// Provides contract to implement transactional functionality following unit of work pattern
    /// </summary>
    public interface IUnitOfWorkBuilder : ITransactional
    {
        /// <summary>
        /// Add an action at the start of the commit
        /// </summary>
        /// <param name="unit">The action to process at the start of commit</param>
        /// <param name="rollback">Optional action to process should a future action fail</param>
        /// <returns>Unit of work builder object</returns>
        IUnitOfWorkBuilder AddStart(Func<Task> unit, Func<Task> rollback = null);
        
        /// <summary>
        /// Add a predefined unit of work at the start of the commit
        /// </summary>
        /// <param name="unit">The unit of work to add to the start of the commit</param>
        /// <returns>Unit of work builder object</returns>
        IUnitOfWorkBuilder AddStart(UnitOfWork unit);
        
        /// <summary>
        /// Add an action to the commit
        /// </summary>
        /// <param name="unit">The action to process on commit</param>
        /// <param name="rollback">Optional action to process should a future action fail</param>
        /// <returns>Unit of work builder object</returns>
        IUnitOfWorkBuilder Add(Func<Task> unit, Func<Task> rollback = null);
        
        /// <summary>
        /// Add a predefined unit of work to the commit
        /// </summary>
        /// <param name="unit">The unit of work to add to the commit</param>
        /// <returns>Unit of work builder object</returns>
        IUnitOfWorkBuilder Add(UnitOfWork unit);
        
        /// <summary>
        /// Add a unit of work to the commit, processed immediately
        /// </summary>
        /// <param name="unit">The action to process immediately</param>
        /// <param name="rollback">Optional action to process should a future action fail</param>
        /// <typeparam name="T">The return from the action</typeparam>
        /// <returns>The return from the action</returns>
        Task<T> AddImmediateAsync<T>(Func<Task<T>> unit, Func<Task<T>> rollback = null);

        /// <summary>
        /// Add a unit of work to the end of the commit
        /// </summary>
        /// <param name="unit">The action to process at the end of commit</param>
        /// <param name="rollback">Optional action to process should a future action fail</param>
        /// <returns>Unit of work builder object</returns>
        IUnitOfWorkBuilder AddEnd(Func<Task> unit, Func<Task> rollback = null);
        
        /// <summary>
        /// Add a unit of work to the end of the commit
        /// </summary>
        /// <param name="unit">The unit of work to add to the end of the commit</param>
        /// <returns>Unit of work builder object</returns>
        IUnitOfWorkBuilder AddEnd(UnitOfWork unit);

        /// <summary>
        /// Subscribe a collection of transactional items to the unit of work
        /// </summary>
        /// <param name="transactionals"></param>
        void Subscribe(params ITransactional[] transactionals);
    }
}