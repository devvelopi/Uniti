# Uniti
An implementation of the unit of work pattern that extends beyond traditional database transactions, allowing management of custom commit and rollback functionality.

#### Regarding Unit of Work 
Technically, Uniti does not follow the traditional 'unit of work' in a sense. This library is useful as a 
utility to wrap generally non-transactional data sources and actions in a 'transaction'.

This can be useful in the following situations where traditionally there is no transactional functionality:
 - NoSQL databases (I.e. DynamoDb) where you can mirror a create with a rollback of delete.
 - File storage (I.e. S3) where you can mirror a create with a rollback of delete.
 - Database transactions where you can treat the `ITransactional` interface as a thin abstraction between data access.
 - Database transactions to implement rollback after database commit (i.e. Deleting created, creating deleted, re-updating)

## Installation

*Coming soon*

## Usage

### Basic usage

Basic usage is simple, a unit of work is simply 2 actions. A commit and rollback action.

```c#
using Uniti.Core.UnitOfWorks;
...
public async Task DoSomething()
{
    var uow = new UnitOfWorkBuilder();
    await uow.StartAsync();

    uow.Add(async () => Console.WriteLine("Unit #1 Run" ), async () => Console.WriteLine("Unit #1 Rollback"));
    uow.Add(async () => Console.WriteLine("Unit #2 Run"),async () => Console.WriteLine("Unit #2 Rollback"));

    // Unit that exceptions
    uow.Add(async () =>
    {
        Console.WriteLine("Unit #3 Exception");
        throw new Exception("Unit #3 Exception");
    }, async () => Console.WriteLine("Unit #3 Rollback"));

    // Commiting runs all actions automatically
    await uow.CommitAsync();
}
```

This will output
```
Unit #1 Run
Unit #2 Run
Unit #3 Exception
Unit #2 Rollback
Unit #1 Rollback
```

### Advanced Usage

More advanced usage involves usage of `IUnitOfWorkBuilder` in nested scenarios with `ITransactional`.

```c#
using System;
using System.Threading.Tasks;
using Uniti.Core.UnitOfWorks;

namespace Uniti.Core.Example
{
    public class SomeService : ITransactional
    {
        private readonly IUnitOfWorkBuilder _uow;

        public SomeService()
        {
            _uow = new UnitOfWorkBuilder();
        }

        public async Task SaveFileThenRestThenDatabase()
        {
            _uow.Add(async () => Console.WriteLine("Saving file"), 
                async () => Console.WriteLine("Deleting file"));
            _uow.Add(async () => Console.WriteLine("POST rest request"),
                async () => Console.WriteLine("DELETE rest request"));
            _uow.Add(async () => Console.WriteLine("Commiting Db Transaction"), 
                async () => Console.WriteLine("Rolling back Db Transaction"))
            
        }
        
        public async Task StartAsync() => await _uow.StartAsync();

        public async Task CommitAsync(bool autoRollback = true) => await _uow.CommitAsync(autoRollback);

        public async Task RollbackAsync() => await _uow.RollbackAsync();
    }
}
```

Additionally, if using a higher level unit of work, for example in an AspNetCore Action Filter (wrapping a http request),
you can use the built in `IUnitOfWorkBuilder.Subscribe` functionality, which automatically connects the start and commit to 
the start and end of the unit of work respectively.

For example, using the previous example:

```c#
public class A 
{
    private readonly IUnitOfWorkBuilder _uow;
    private readonly ISomeService _someService;
    
    public A(ISomeService someService) 
    {
        _uow = new UnitOfWorkBuilder();
        _someService = someService;
    }
    
    public async Task B() 
    {   
        // Adds the commit and rollback functionality automatically
        _uow.Subscribe(_someService);
        // The actions will not fire until the _uow is committed
        await _someService.SaveFileThenRestThenDatabase();
        // Add additional commit actions
        _uow.Add(async () => Console.WriteLine("Another unit"));
        // Runs all actions added to the _uow
        await _uow.CommitAsync();
    }
}
```

A final note, the `ITransactional` interface is non-reliant on the unit of work functionality,
if the unit of work mechanism is not needed, you are able to write your own custom transaction management
logic using this interface. 

## TODO
- Non-async unit addition
- Pass-through of action return as rollback parameter

## Works well with

### Repositori
A shameless plug for my other project [Repositori](https://github.com/jaseaman/Repositori), an implementation of the Repository pattern

This allows a generic thin CRUD abstraction layer over any data source.

It works well with Uniti by using Uniti's `ITransactional` interface. 
Such as the pseudo code below.

```c#
public class MockRepository<TEntity, TIdentifier> : IRepository<TEntity, TIdentifier>, ITransactional 
{
    protected readonly IUnitOfWorkBuilder _uow;
    
    public MockRepository() 
    {
        _uow = new UnitOfWorkBuilder();
    }

    ...
    /// <inheritdoc />
    public async Task<TEntity> CreateAsync(TEntity entity)
    {
        _uow.Add(
        async () => 
        { 
            //  Create functionality
        },
        async () => 
        {
            // Rollback create functionality
            await DeleteAsync(entity);
        })
        return entity;
    }
    ...
        
    public async Task StartAsync() => await _uow.StartAsync();

    public async Task CommitAsync() => await _uow.CommitAsync();
    
    public async Task RollbackUnitAsync() => await _uow.RollbackAsync();
}
```

### Mediati

Yet another shameless plug, my library used as a Mediator pattern [Mediati](https://github.com/jaseaman/Mediati).

With Mediati by using the pipeline behaviours, it is possible to add a decorator to commands to automagically subscribe
the `ITransactional` DI constructor arguments to the command / query or event, improving code re-use.