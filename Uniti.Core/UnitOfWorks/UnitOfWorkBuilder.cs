using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uniti.Core.UnitOfWorks.Util;

namespace Uniti.Core.UnitOfWorks
{
    /// <summary>
    /// Default implementation of unit of work management
    /// </summary>
    public class UnitOfWorkBuilder : IUnitOfWorkBuilder
    {
        /// <summary>
        /// Actions to run at the start of the unit commit
        /// </summary>
        protected readonly List<UnitOfWork> OnStartActions = new List<UnitOfWork>();
        /// <summary>
        /// Actions to run during the commit
        /// </summary>
        protected readonly List<UnitOfWork> Actions = new List<UnitOfWork>();
        /// <summary>
        /// Actions to run at the end of the commit
        /// </summary>
        protected readonly List<UnitOfWork> OnEndActions = new List<UnitOfWork>();

        /// <summary>
        /// All actions that are yet to be run
        /// </summary>
        protected virtual IEnumerable<UnitOfWork> WaitingActions => OnStartActions.Concat(Actions).Concat(OnEndActions)
            .Where(uow => uow.Status == UnitOfWorkStatus.Waiting);

        /// <summary>
        /// All actions that have already been run
        /// </summary>
        protected virtual IEnumerable<UnitOfWork> RunActions => OnStartActions.Concat(Actions).Concat(OnEndActions)
            .Where(uow => uow.Status == UnitOfWorkStatus.Run);

        /// <inheritdoc />
        public virtual async Task StartAsync()
        {
            foreach (var uow in OnStartActions.Where(uow => uow.Status == UnitOfWorkStatus.Waiting))
            {
                uow.Status = UnitOfWorkStatus.Running;
                await uow.Action();
                uow.Status = UnitOfWorkStatus.Run;
            }
        }

        /// <inheritdoc />
        public virtual IUnitOfWorkBuilder AddStart(Func<Task> unit, Func<Task> rollback = null) =>
            AddStart(new UnitOfWork(unit, rollback));


        /// <inheritdoc />
        public virtual IUnitOfWorkBuilder AddStart(UnitOfWork unit)
        {
            OnStartActions.Add(unit);
            return this;
        }

        /// <inheritdoc />
        public virtual IUnitOfWorkBuilder Add(Func<Task> unit, Func<Task> rollback = null) =>
            Add(new UnitOfWork(unit, rollback));

        /// <inheritdoc />
        public virtual IUnitOfWorkBuilder Add(UnitOfWork unit)
        {
            Actions.Add(unit);
            return this;
        }

        /// <inheritdoc />
        public virtual async Task<T> AddImmediateAsync<T>(Func<Task<T>> unit, Func<Task<T>> rollback = null)
        {
            var result = await unit();
            Add(new UnitOfWork(unit, rollback) {Status = UnitOfWorkStatus.Run});
            return result;
        }

        /// <inheritdoc />
        public virtual IUnitOfWorkBuilder AddEnd(Func<Task> unit, Func<Task> rollback = null) =>
            Add(new UnitOfWork(unit, rollback));

        /// <inheritdoc />
        public virtual IUnitOfWorkBuilder AddEnd(UnitOfWork unit)
        {
            OnEndActions.Add(unit);
            return this;
        }

        /// <inheritdoc />
        public virtual void Subscribe(params ITransactional[] transactionals)
        {
            foreach (var r in transactionals)
            {
                AddStart(async () => await r.StartAsync(),
                    async () => await r.RollbackAsync());
                AddEnd(async () => await r.CommitAsync(),
                    async () => await r.RollbackAsync());
            }
        }

        /// <inheritdoc />
        public virtual async Task CommitAsync(bool autoRollback = true)
        {
            try
            {
                foreach (var action in WaitingActions)
                {
                    action.Status = UnitOfWorkStatus.Running;
                    await action.Action();
                    action.Status = UnitOfWorkStatus.Run;
                }
            }
            catch
            {
                if (autoRollback) await RollbackAsync();
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task RollbackAsync()
        {
            var rollbackExceptions = new List<Exception>();
            foreach (var action in RunActions.Reverse())
            {
                action.Status = UnitOfWorkStatus.RollingBack;
                try
                {
                    await action?.Rollback();
                }
                catch (Exception e)
                {
                    rollbackExceptions.Add(e);
                    Console.WriteLine(e);
                    action.Status = UnitOfWorkStatus.RollbackFailed;
                    continue;
                }

                action.Status = UnitOfWorkStatus.RolledBack;
            }

            if (rollbackExceptions.Any())
                throw new AggregateException("Unit of work rollback has failed", rollbackExceptions);
        }
    }
}