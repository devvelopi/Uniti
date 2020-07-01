using System;
using System.Threading.Tasks;
using Uniti.Core.UnitOfWorks;

namespace Uniti.Core.Example
{
    public class SomeService : ITransactional
    {
        private readonly IUnitOfWorkBuilder _uow;

        public SomeService(IUnitOfWorkBuilder uow)
        {
            _uow = uow;
        }

        public async Task SaveFileThenRestThenDatabase()
        {
            _uow.Add(async () => Console.WriteLine("Saving file"), 
                async () => Console.WriteLine("Deleting file"));
            _uow.Add(async () => Console.WriteLine("POST rest request"),
                async () => Console.WriteLine("DELETE rest request"));
            _uow.Add(async () => Console.WriteLine("Commiting Db Transaction"),
                async () => Console.WriteLine("Rolling back Db Transaction"));

        }
        
        public async Task StartAsync() => await _uow.StartAsync();

        public async Task CommitAsync(bool autoRollback = true) => await _uow.CommitAsync(autoRollback);

        public async Task RollbackAsync() => await _uow.RollbackAsync();
    }
}