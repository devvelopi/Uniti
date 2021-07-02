using System;
using System.Threading.Tasks;

namespace Uniti
{
    /// <summary>
    /// A container for a unit of work containing the action, rollback and status
    /// </summary>
    public class UnitOfWork
    {
        /// <summary>
        /// Default constructor with an action and optional rollback
        /// </summary>
        /// <param name="action">The unit action</param>
        /// <param name="rollback">Optional unit rollback</param>
        public UnitOfWork(Func<Task> action, Func<Task>? rollback = null)
        {
            Action = action;
            Rollback = rollback;
            Status = UnitOfWorkStatus.Waiting;
        }
        
        /// <summary>
        /// The status of the unit of work
        /// </summary>
        public UnitOfWorkStatus Status { get; set; }

        /// <summary>
        /// The unit of work action
        /// </summary>
        public Func<Task> Action { get; }

        /// <summary>
        /// The rollback of the action
        /// </summary>
        public Func<Task>? Rollback { get; }
    }
}