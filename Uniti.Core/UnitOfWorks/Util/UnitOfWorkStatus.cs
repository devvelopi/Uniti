namespace Uniti.Core.UnitOfWorks.Util
{
    /// <summary>
    /// Status identifiers used for unit of work elements
    /// </summary>
    public enum UnitOfWorkStatus
    {
        /// <summary>
        /// Unit is waiting to be run
        /// </summary>
        Waiting,
        /// <summary>
        /// Unit is in the process of running
        /// </summary>
        Running,
        /// <summary>
        /// Unit has run
        /// </summary>
        Run,
        /// <summary>
        /// Unit has been canceled prematurely
        /// </summary>
        Canceled,
        /// <summary>
        /// Unit is in the process of rolling back
        /// </summary>
        RollingBack,
        /// <summary>
        /// Unit has successfully rolled back
        /// </summary>
        RolledBack,
        /// <summary>
        /// Unit has failed to roll back
        /// </summary>
        RollbackFailed
    }
}