using System;
using System.Threading.Tasks;

namespace Uniti.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            DoSomething();
        }
        
        public static async Task DoSomething()
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
    }
}